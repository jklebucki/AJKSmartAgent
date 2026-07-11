var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Praxiara_Api>("api")
    .WithExternalHttpEndpoints();
builder.AddProject<Projects.Praxiara_Browser_Worker>("browser-worker");
builder.AddProject<Projects.Praxiara_Orchestrator_Worker>("orchestrator-worker");

builder.Build().Run();