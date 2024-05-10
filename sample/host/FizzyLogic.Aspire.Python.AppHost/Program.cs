using FizzyLogic.Aspire.Python.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddPythonProjectWithVirtualEnvironment("batch", "../../apps/batch");
builder.AddFlaskProjectWithVirtualEnvironment("flask-service", "../../apps/flask-service");
builder.AddDjangoProjectWithVirtualEnvironment("django-service", "../../apps/django-service");

// builder.AddExecutable("django-service", ".venv/bin/python", "../../apps/django-service", "manage.py", "runserver", "8000").WithHttpEndpoint(targetPort: 8000, env: "DJANGO_HTTP_PORT");

builder.Build().Run();
