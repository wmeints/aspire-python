using System.Collections;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Publishing;

namespace FizzyLogic.Aspire.Python.Hosting;

public static class PythonProjectResourceBuilderExtensions
{
    /// <summary>
    /// Adds a python project that's built with a virtual environment to the distributed application. We assume that you
    /// have a Dockerfile in your project directory and that the virtual environment is located in the .venv directory.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name">The name of the python project</param>
    /// <param name="projectDirectory">The directory where the project files are located</param>
    /// <param name="entrypoint">Script to run when the project is started</param>
    /// <param name="args">Extra command line arguments for the project</param>
    /// <returns>Returns the resource builder</returns>
    public static IResourceBuilder<PythonProjectResource> AddPythonProjectWithVirtualEnvironment(
        this IDistributedApplicationBuilder builder, string name, string projectDirectory,
        string entrypoint = "main.py", params string[]? args)
    {
        var absoluteProjectDirectory = Path.GetFullPath(Path.Join(builder.AppHostDirectory, projectDirectory));
        var virtualEnvironment = new VirtualEnvironment(absoluteProjectDirectory);
        var instrumentationExecutable = virtualEnvironment.GetExecutable("opentelemetry-instrument");
        var pythonExecutable = virtualEnvironment.GetRequiredExecutable("python");
        var projectExecutable = instrumentationExecutable ?? pythonExecutable;

        var projectResource = new PythonProjectResource(name, projectExecutable, absoluteProjectDirectory);

        var resourceBuilder = builder.AddResource(projectResource).WithArgs(context =>
        {
            if (!string.IsNullOrEmpty(instrumentationExecutable))
            {
                // // Export the logs to the OTLP endpoint only. We already have logging.
                context.Args.Add("--traces_exporter");
                context.Args.Add("otlp");

                context.Args.Add("--logs_exporter");
                context.Args.Add("console,otlp");

                context.Args.Add("--metrics_exporter");
                context.Args.Add("otlp");

                context.Args.Add(pythonExecutable!);
            }

            // Always include the entrypoint script as the first argument
            context.Args.Add(entrypoint);

            if (args is not null)
            {
                // Next include the provided set of arguments
                foreach (var arg in args)
                {
                    context.Args.Add(arg);
                }
            }
        });

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