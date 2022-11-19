namespace ProjectStatistics.DependencyProcesses;

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