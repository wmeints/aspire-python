using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Publishing;

namespace FizzyLogic.Aspire.Python.Hosting;

/// <summary>
/// A resource that represents a python project with FastAPI.
/// </summary>
/// <param name="name"></param>
public class PythonProjectResource(string name, string pythonExecutable, string workingDirectory): ExecutableResource(name, pythonExecutable, workingDirectory),  IResourceWithEnvironment, IResourceWithArgs, IResourceWithServiceDiscovery
{
    /// <summary>
    /// Writes out the publishing manifest for the python project.
    /// </summary>
    /// <param name="context"></param>
    public async Task WriteDockerFileManifestAsync(ManifestPublishingContext context)
    {
        var dockerFilePath = Path.Combine(WorkingDirectory, "Dockerfile");
        var manifestRelativeDockerFilePath = context.GetManifestRelativePath(dockerFilePath);
        var manifestRelativeWorkingDirectory = context.GetManifestRelativePath(WorkingDirectory);

        if (!File.Exists(dockerFilePath))
        {
            throw new InvalidOperationException(
                "Dockerfile not found in project directory. Please provide a Dockerfile in the project directory.");
        }

        context.Writer.WriteString("type", "dockerfile.v0");
        context.Writer.WriteString("path", manifestRelativeDockerFilePath);
        context.Writer.WriteString("context", manifestRelativeWorkingDirectory);

        await context.WriteEnvironmentVariablesAsync(this).ConfigureAwait(false);

        context.WriteBindings(this);
    }   
}