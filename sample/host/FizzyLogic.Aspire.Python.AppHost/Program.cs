using FizzyLogic.Aspire.Python.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddPythonProjectWithVirtualEnvironment("batch", "../../apps/batch");

builder.Build().Run();
