using Praxiara.Orchestrator.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHostedService<OrchestrationWorker>();

var host = builder.Build();
host.Run();