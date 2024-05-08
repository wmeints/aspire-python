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
    public static IResourceBuilder<PythonProjectResource> AddPythonProjectWithVirtualEnvironment(this IDistributedApplicationBuilder builder, string name, string projectDirectory, string entrypoint = "main.py", params string[]? args)
    {
        var rootDirectory = builder.AppHostDirectory;
        var absoluteProjectDirectory = Path.GetFullPath(Path.Join(rootDirectory, projectDirectory));
        
        PythonProjectResource projectResource;

        var instrumentationExecutable = Path.Join(absoluteProjectDirectory, ".venv", "bin", "opentelemetry-instrument");
        var pythonExecutable = Path.Join(absoluteProjectDirectory, ".venv", "bin", "python");
        bool usesInstrumentation = false;

        if (File.Exists(instrumentationExecutable))
        {
            usesInstrumentation = true;
            projectResource = new PythonProjectResource(name, instrumentationExecutable, absoluteProjectDirectory);
        }
        else
        {
            projectResource = new PythonProjectResource(name, pythonExecutable, absoluteProjectDirectory);
        }

        var resourceBuilder = builder.AddResource(projectResource).WithArgs(context =>
        {
            if (usesInstrumentation)
            {
                // // Export the logs to the OTLP endpoint only. We already have logging.
                context.Args.Add("--traces_exporter");
                context.Args.Add("otlp");
                
                context.Args.Add("--logs_exporter");
                context.Args.Add("console,otlp");
                
                context.Args.Add("--metrics_exporter");
                context.Args.Add("otlp");

                context.Args.Add(pythonExecutable);
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
        if (usesInstrumentation)
        {
            resourceBuilder.WithOtlpExporter();
            
            // Make sure to attach the logging instrumentation setting so we can capture logs.
            // Without this you'll need to configure logging yourself. Which is kind of a pain.
            resourceBuilder.WithEnvironment("OTEL_PYTHON_LOGGING_AUTO_INSTRUMENTATION_ENABLED", "true");
        }

        resourceBuilder.WithManifestPublishingCallback(context => WriteProjectAsDockerFile(context, projectResource));

        return resourceBuilder;
    }

    /// <summary>
    /// Writes out the publishing manifest for the python project.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resource"></param>
    private static async Task WriteProjectAsDockerFile(ManifestPublishingContext context, PythonProjectResource resource)
    {
        var dockerFilePath = Path.Combine(resource.WorkingDirectory, "Dockerfile");
        var manifestRelativeDockerFilePath = context.GetManifestRelativePath(dockerFilePath);
        var manifestRelativeWorkingDirectory = context.GetManifestRelativePath(resource.WorkingDirectory);

        if (!File.Exists(dockerFilePath))
        {
            throw new InvalidOperationException("Dockerfile not found in project directory. Please provide a Dockerfile in the project directory.");
        }

        context.Writer.WriteString("type", "dockerfile.v0");
        context.Writer.WriteString("path", manifestRelativeDockerFilePath);
        context.Writer.WriteString("context", manifestRelativeWorkingDirectory);

        await context.WriteEnvironmentVariablesAsync(resource).ConfigureAwait(false);

        context.WriteBindings(resource);
    }

    private static string DiscoverPythonExecutable(string workingDirectory)
    {
        // Check for the well-known .venv folder inside the working directory.
        // If it exists, we can assume that the python executable is located in the .venv folder.
        if (Directory.Exists(Path.Join(workingDirectory, ".venv")))
        {
            var virtualEnvironmentPath = Path.Join(workingDirectory, ".venv");
            var windowsPythonExecutable = Path.Join(virtualEnvironmentPath, "bin", "python.exe");
            var unixPythonExecutable = Path.Join(virtualEnvironmentPath, "bin", "python");

            // One of these entries should return a value. If not, we're dealing with a non-standard setup.
            // The user should provide the python executable path manually.
            if (File.Exists(unixPythonExecutable))
            {
                return unixPythonExecutable;
            }

            if (File.Exists(windowsPythonExecutable))
            {
                return windowsPythonExecutable;
            }
        }

        return "python";
    }

    /// <summary>
    /// Detect whether the project contains a requirements file
    /// </summary>
    /// <param name="projectRootDir"></param>
    /// <returns></returns>
    private static IEnumerable<string> GetRequirementsFiles(string projectRootDir)
    {
        var projectFiles = Directory.GetFiles(projectRootDir);

        // List of candidate files that may contain dependencies
        // This list is derived from common python project conventions
        string[] candidateFiles =
        [
            "requirements.txt",
            "requirements.lock",
            "dev-requirements.txt",
            "test-requirements.txt",
            "dev-requirements.lock"
        ];

        foreach (var file in candidateFiles)
        {
            if (projectFiles.Contains(file))
            {
                yield return file;
            }
        }
    }
}