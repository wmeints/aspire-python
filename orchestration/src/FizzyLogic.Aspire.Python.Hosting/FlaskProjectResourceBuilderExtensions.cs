using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace FizzyLogic.Aspire.Python.Hosting;

public static class FlaskProjectResourceBuilderExtensions
{
    /// <summary>
    /// Adds a flask service to the distributed application
    /// </summary>
    /// <param name="builder">Distributed application builder instance</param>
    /// <param name="name">The name of the service</param>
    /// <param name="projectDirectory">The directory containing the project</param>
    /// <param name="entrypoint">The script containing the flask app variable</param>
    /// <returns>Returns the resource builder for further extension</returns>
    public static IResourceBuilder<FlaskProjectResource> AddFlaskProjectWithVirtualEnvironment(this IDistributedApplicationBuilder builder, string name, string projectDirectory, string entrypoint = "main.py")
    {
        var absoluteProjectDirectory = Path.GetFullPath(Path.Join(builder.AppHostDirectory, projectDirectory));
        var virtualEnvironment = new VirtualEnvironment(absoluteProjectDirectory);
        var instrumentationExecutable = virtualEnvironment.GetExecutable("opentelemetry-instrument");
        var flaskExecutable = virtualEnvironment.GetExecutable("flask");
        var projectExecutable = instrumentationExecutable ?? flaskExecutable!;
        var projectResource = new FlaskProjectResource(name, projectExecutable, absoluteProjectDirectory);

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
                
                context.Args.Add(flaskExecutable!);
            }

            // Always include the entrypoint script as the first argument
            context.Args.Add("--app");
            context.Args.Add(Path.GetFileNameWithoutExtension(entrypoint));
            context.Args.Add("run");
        });
        
        // Automatically add the HTTP endpoint for the Flask project (which in this case is always on port 5000)
        // Expose the port on the FLASK_RUN_PORT to allow the Flask app to bind to the correct port.
        resourceBuilder.WithHttpEndpoint(targetPort: 5000, name: "http", env: "FLASK_RUN_PORT");
        
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