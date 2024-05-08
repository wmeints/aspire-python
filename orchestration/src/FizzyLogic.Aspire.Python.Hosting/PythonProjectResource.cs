using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace FizzyLogic.Aspire.Python.Hosting;

/// <summary>
/// A resource that represents a python project with FastAPI.
/// </summary>
/// <param name="name"></param>
public class PythonProjectResource(string name, string pythonExecutable, string workingDirectory): ExecutableResource(name, pythonExecutable, workingDirectory),  IResourceWithEnvironment, IResourceWithArgs, IResourceWithServiceDiscovery
{
    
}