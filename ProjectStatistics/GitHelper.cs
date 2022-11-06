using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics;

public class GitHelper : ISingletonDependency
{
    private ILogger<GitHelper> Logger { get; }
    private ProcessHelper ProcessHelper { get; }

    public GitHelper(ProcessHelper processHelper, ILogger<GitHelper> logger)
    {
        ProcessHelper = processHelper;
        Logger = logger;
    }

    public Task<bool> CloneRepository(Repository repository)
    {
        return Task.Run(async () =>
        {
            Logger.LogInformation($"Cloning {repository.Name}...");
            var path = await PathHelper.BuildFullPath(repository.Language, "Repositories");

            var isSuccess = await ProcessHelper.RunAsync("git", $"clone {repository.CloneUrl}", path);
            
            if (isSuccess)
            {
                Logger.LogInformation($"Cloned {repository.Name} successfully.");
            }
            else
            {
                Logger.LogError($"Failed to clone {repository.Name}.");
            }
            
            return isSuccess;
        });
    }
}