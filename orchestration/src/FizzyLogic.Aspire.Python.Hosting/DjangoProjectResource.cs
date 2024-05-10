using Aspire.Hosting.ApplicationModel;

namespace FizzyLogic.Aspire.Python.Hosting;

public class DjangoProjectResource(string name, string executablePath, string workingDirectory) : PythonProjectResource(name, executablePath, workingDirectory), IResourceWithEndpoints;