using ProjectStatistics.Helpers;

namespace ProjectStatistics.DependencyProcesses;

public class GitDependencyProcess : IDependencyProcess
{
    private readonly ProcessHelper _processHelper;
    public GitDependencyProcess(ProcessHelper processHelper)
    {
        _processHelper = processHelper;
    }
    public string Name => "Git";
    public string ProcessName => "git";

    public bool IsThereProcess()
    {
        var environmentVariable = Environment.GetEnvironmentVariable("PATH");
        var paths = environmentVariable?.Split(';');
        return paths != null && paths.Any(path => File.Exists(Path.Combine(path, "git.exe")));
    }

    // winget install --id Git.Git -e --source winget
    public async Task<bool> SolveDependencyAsync(CancellationToken token = default)
    {
        var result = await _processHelper.RunAsync("winget", "install --id Git.Git -e --source winget");
        if (result.Success)
        {
            return true;
        }
        token.ThrowIfCancellationRequested();
        throw new Exception("Failed to install Git");
    }
}