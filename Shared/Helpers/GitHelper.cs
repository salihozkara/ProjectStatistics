using Microsoft.Extensions.Logging;
using ProjectStatistics.DependencyProcesses;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics.Helpers;

[DependencyProcess<GitDependencyProcess>]
public class GitHelper : ISingletonDependency
{
    public GitHelper(ProcessHelper processHelper, ILogger<GitHelper> logger)
    {
        ProcessHelper = processHelper;
        Logger = logger;
    }

    private ILogger<GitHelper> Logger { get; }
    private ProcessHelper ProcessHelper { get; }

    public Task<bool> CloneRepository(Repository repository, CancellationToken token = default)
    {
        return Task.Run(async () =>
        {
            Logger.LogInformation($"Cloning {repository.FullName}...");
            var path = await PathHelper.BuildFullPath(repository.Language, "Repositories",repository.FullName);

            var repositoryPath = Path.Combine(path, repository.FullName);
            if (Directory.Exists(repositoryPath))
            {
                var directoryInfo = new DirectoryInfo(repositoryPath);
                var dirs = directoryInfo.GetDirectories().Where(d => d.Name != ".git").ToList();
                var dirSize =
                    dirs.Sum(d => d.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length)) / 1024;
                if (dirSize < repository.Size)
                {
                    Logger.LogInformation(
                        $"Repository {repository.FullName} already exists, but is smaller than expected. Deleting and cloning again...");
                    Directory.Delete(repositoryPath, true);
                }
                else
                {
                    Logger.LogInformation($"Repository {repository.FullName} already exists. Skipping...");
                    return true;
                }
            }

            var result = await ProcessHelper.RunAsync("git", $"clone {repository.CloneUrl}", path);

            if (result.Success)
            {
                Logger.LogInformation($"Cloned {repository.FullName} successfully.");
            }
            else
            {
                Logger.LogError($"Failed to clone {repository.FullName}.");
                token.ThrowIfCancellationRequested();
            }

            return result.Success;
        }, token);
    }
}