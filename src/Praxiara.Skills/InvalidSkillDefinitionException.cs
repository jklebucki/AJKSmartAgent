namespace Praxiara.Skills;

public sealed class InvalidSkillDefinitionException(string message) : FormatException(message);