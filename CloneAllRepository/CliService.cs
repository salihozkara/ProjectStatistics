using Microsoft.Extensions.Logging;
using Shared;
using Shared.Helpers;
using Volo.Abp.DependencyInjection;

namespace CloneAllRepository;

public class CliService : ISingletonDependency
{
    public static bool IsStopRequested = false;

    private readonly List<Repository> _errorRepositories = new();
    private readonly GitHelper _gitHelper;

    public CliService(GitHelper gitHelper, ILogger<CliService> logger)
    {
        _gitHelper = gitHelper;
        Logger = logger;
    }

    private ILogger<CliService> Logger { get; }

    public async Task RunAsync(string[] args)
    {
        foreach (var repository in Resources.RepositoriesJson.Value)
        {
            if (IsStopRequested)
                break;
            try
            {
                await _gitHelper.CloneRepository(repository);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while cloning repository");
                _errorRepositories.Add(repository);
            }
        }

        var i = 0;
        while (_errorRepositories.Count > 0)
        {
            i++;
            foreach (var errorRepository in _errorRepositories)
                try
                {
                    await _gitHelper.CloneRepository(errorRepository);
                    _errorRepositories.Remove(errorRepository);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error while cloning repository");
                }


            if (i == 3) break;
        }

        _errorRepositories.ToJsonFile();
    }

    private string ToSizeString(long totalSize)
    {
        var size = totalSize;
        var sizeString = "KB";
        if (size > 1024)
        {
            size /= 1024;
            sizeString = "MB";
        }

        if (size > 1024)
        {
            size /= 1024;
            sizeString = "GB";
        }

        if (size > 1024)
        {
            size /= 1024;
            sizeString = "TB";
        }

        return $"{size} {sizeString}";
    }
}