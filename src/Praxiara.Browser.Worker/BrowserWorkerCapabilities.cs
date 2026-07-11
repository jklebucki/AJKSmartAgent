internal sealed record BrowserWorkerCapabilities(
    string Engine,
    bool SemanticObservations,
    bool Tracing,
    bool ManualTakeover);