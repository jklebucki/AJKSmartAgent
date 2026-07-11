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
}