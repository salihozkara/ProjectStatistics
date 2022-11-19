using Microsoft.Extensions.DependencyInjection;

namespace ProjectStatistics.DependencyProcesses;

public static class DependencyProcessesChecker
{
    public static async Task<bool> DependencyProcessesCheck(this IServiceProvider serviceProvider, CancellationToken token = default)
    {
        var dependencyProcesses = serviceProvider.GetRequiredService<IEnumerable<IDependencyProcess>>();

        foreach (var dependencyProcess in dependencyProcesses)
        {
            if (!dependencyProcess.IsThereProcess())
            {
                return await dependencyProcess.SolveDependencyAsync(token);
            }
        }

        return true;
    }
}