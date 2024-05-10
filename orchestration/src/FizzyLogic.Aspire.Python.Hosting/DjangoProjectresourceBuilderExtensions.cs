using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace FizzyLogic.Aspire.Python.Hosting;

public static class DjangoProjectResourceBuilderExtensions
{
    /// <summary>
    /// Adds a django project to the distributed application.
    /// </summary>
    /// <param name="builder">Distributed application builder to extend</param>
    /// <param name="name">Name of the django project</param>
    /// <param name="projectDirectory">The directory containing the django project</param>
    /// <returns>Returns the resource builder for further extension</returns>
    public static IResourceBuilder<DjangoProjectResource> AddDjangoProjectWithVirtualEnvironment(this IDistributedApplicationBuilder builder, string name, string projectDirectory, int httpPort = 8000)
    {
        var absoluteProjectDirectory = Path.GetFullPath(Path.Join(builder.AppHostDirectory, projectDirectory));
        var virtualEnvironment = new VirtualEnvironment(absoluteProjectDirectory);
        var instrumentationExecutable = virtualEnvironment.GetExecutable("opentelemetry-instrument");
        var pythonExecutable = virtualEnvironment.GetRequiredExecutable("python");
        var projectExecutable = instrumentationExecutable ?? pythonExecutable;
        var projectResource = new DjangoProjectResource(name, projectExecutable, absoluteProjectDirectory);

        var resourceBuilder = builder.AddResource(projectResource).WithArgs(context =>
        {
            if (!string.IsNullOrEmpty(instrumentationExecutable))
            {
                // Export the logs to the OTLP endpoint only. We already have logging.
                context.Args.Add("--traces_exporter");
                context.Args.Add("otlp");

                context.Args.Add("--logs_exporter");
                context.Args.Add("console,otlp");

                context.Args.Add("--metrics_exporter");
                context.Args.Add("otlp");

                context.Args.Add(pythonExecutable);
            }

            // Always include the entrypoint script as the first argument
            context.Args.Add("manage.py");
            context.Args.Add("runserver");

            var serverPort = projectResource.GetEndpoint("http").Port.ToString()!;

            // Bind Django to the target port of the HTTP endpoint
            // Also, make sure we bind to the 0.0.0.0 address to allow external connections.
            context.Args.Add(serverPort);
        });

        // Automatically add the HTTP endpoint for the Django project (which in this case is always on port 8000)
        // Expose the port on the HTTP_PORT to allow the Django app to bind to the correct port.
        // Do not proxy the endpoint as Django doesn't seem to like that.
        resourceBuilder.WithHttpEndpoint(port: 8000, name: "http", env: "PORT", isProxied: false);

        // Make sure to wire up the OTLP exporter automatically when we're using opentelemetry instrumentation.
        if (!string.IsNullOrEmpty(instrumentationExecutable))
        {
            resourceBuilder.WithOtlpExporter();

            // Make sure to attach the logging instrumentation setting, so we can capture logs.
            // Without this you'll need to configure logging yourself. Which is kind of a pain.
            resourceBuilder.WithEnvironment("OTEL_PYTHON_LOGGING_AUTO_INSTRUMENTATION_ENABLED", "true");
        }

        // Python projects need their own Dockerfile, we can't provide it through the hosting package.
        // Maybe in the future we can add a way to provide a Dockerfile template.
        resourceBuilder.WithManifestPublishingCallback(context => projectResource.WriteDockerFileManifestAsync(context));

        return resourceBuilder;
    }
}