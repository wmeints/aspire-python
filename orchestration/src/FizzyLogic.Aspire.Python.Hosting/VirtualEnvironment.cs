namespace FizzyLogic.Aspire.Python.Hosting;

public class VirtualEnvironment(string projectDirectory)
{
    /// <summary>
    /// Locates an executable in the virtual environment
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string? GetExecutable(string name)
    {
        string[] allowedExtensions = [".exe", ".cmd", ".bat", ""];
        string[] executableDirectories = ["bin", "Scripts"];

        foreach (var executableDirectory in executableDirectories)
        {
            foreach (var extension in allowedExtensions)
            {
                string executablePath = Path.Join(projectDirectory, ".venv", executableDirectory, name + extension);

                if (File.Exists(executablePath))
                {
                    return executablePath;
                }
            }
        }

        return null;
    }

    public string GetRequiredExecutable(string name)
    {
        return GetExecutable(name) ?? throw new RequiredExecutableNotFoundException(
            $"The executable {name} could not be found in the virtual environment. " +
            "Make sure the virtual environment is initialized and the executable is installed.");
    }
}