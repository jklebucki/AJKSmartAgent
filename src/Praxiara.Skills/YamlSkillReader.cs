using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Praxiara.Skills;

public sealed class YamlSkillReader
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public SkillDefinition Read(string yaml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(yaml);

        var definition = _deserializer.Deserialize<SkillDefinition>(yaml)
            ?? throw new InvalidSkillDefinitionException("The document did not contain a skill definition.");

        Validate(definition);
        return definition;
    }

    private static void Validate(SkillDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.Id) || !definition.Id.Contains('.', StringComparison.Ordinal))
        {
            throw new InvalidSkillDefinitionException("A skill id must be a namespaced identifier.");
        }

        if (!Version.TryParse(definition.Version, out _))
        {
            throw new InvalidSkillDefinitionException("A skill version must use a numeric semantic version.");
        }

        if (definition.Steps.Count == 0)
        {
            throw new InvalidSkillDefinitionException("A skill must contain at least one step.");
        }

        var duplicateStep = definition.Steps
            .GroupBy(step => step.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateStep is not null)
        {
            throw new InvalidSkillDefinitionException($"Step id '{duplicateStep.Key}' is duplicated.");
        }
    }
}