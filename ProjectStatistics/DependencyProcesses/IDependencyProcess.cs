using ProjectStatistics.Helpers;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics.DependencyProcesses;

public interface IDependencyProcess : IScopedDependency
{
    string Name { get; }
    string ProcessName { get; }
    
    bool IsThereProcess();
    Task<bool> SolveDependencyAsync(CancellationToken token = default);
}

public class DependencyProcessAttribute<T> : Attribute where T : IDependencyProcess
{
    public Type Type { get; } = typeof(T);
}

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

public class SourceMonitorDependencyProcess : IDependencyProcess
{
    public string Name => "SourceMonitor";
    public string ProcessName => "SourceMonitor";

    public bool IsThereProcess()
    {
        try
        {
            var p = Resources.SourceMonitor.SourceMonitorExe;
            return File.Exists(p.StartInfo.FileName);
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public Task<bool> SolveDependencyAsync(CancellationToken token = default)
    {
        throw new Exception("SourceMonitor is not available for download");
    }
}

