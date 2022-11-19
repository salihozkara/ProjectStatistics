using Volo.Abp.DependencyInjection;

namespace ProjectStatistics.DependencyProcesses;

public interface IDependencyProcess : IScopedDependency
{
    string Name { get; }
    string ProcessName { get; }
    
    bool IsThereProcess();
    Task<bool> SolveDependencyAsync(CancellationToken token = default);
}