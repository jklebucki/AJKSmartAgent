namespace Praxiara.Application.Abstractions;

public sealed record VerificationResult(bool Succeeded, string Code, string? Detail);