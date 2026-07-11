using Praxiara.Skills;

namespace Praxiara.UnitTests;

public sealed class SkillReaderTests
{
    [Fact]
    public void ReadParsesValidSkill()
    {
        const string yaml = """
            id: ifs.invoice.lookup
            name: Invoice lookup
            version: 1.0.0
            site: ifs-cloud
            steps:
              - id: find-invoice
                action: ifs_find_invoice
            """;

        var result = new YamlSkillReader().Read(yaml);

        Assert.Equal("ifs.invoice.lookup", result.Id);
        Assert.Single(result.Steps);
    }

    [Fact]
    public void ReadParsesRepositoryIfsExample()
    {
        var fixturePath = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "resend-customer-invoice.skill.yaml");
        var yaml = File.ReadAllText(fixturePath);

        var result = new YamlSkillReader().Read(yaml);

        Assert.Equal("ifs.invoice.resend", result.Id);
        Assert.Equal(Praxiara.Domain.Policy.RiskLevel.R4BusinessCommit, result.Risk.FinalActionLevel);
        Assert.Contains(result.Steps, step => step.RequiresApproval);
    }
}