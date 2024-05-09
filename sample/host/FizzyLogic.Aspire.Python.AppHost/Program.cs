using FizzyLogic.Aspire.Python.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddPythonProjectWithVirtualEnvironment("batch", "../../apps/batch");
builder.AddFlaskProjectWithVirtualEnvironment("flask-service", "../../apps/flask-service");

builder.Build().Run();
