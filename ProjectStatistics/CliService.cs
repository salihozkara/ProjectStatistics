using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ProjectStatistics.Helpers;
using ProjectStatistics.LanguageStatistics;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics;

public class CliService : ISingletonDependency
{
    private readonly ConcurrentBag<Repository> _errorRepositories = new();
    private readonly LanguageStatisticsFactory _languageStatisticsFactory;

    public CliService(LanguageStatisticsFactory languageStatisticsFactory, ILogger<CliService> logger)
    {
        _languageStatisticsFactory = languageStatisticsFactory;
        Logger = logger;
    }

    private ILogger<CliService> Logger { get; }

    public async Task RunAsync(string[] args)
    {
        var arg = ArgsHelper.Parse(args);

        var languages = _languageStatisticsFactory.GetSupportedLanguages();

        var repositories = Resources.RepositoriesJson
            .Where(r => languages.Contains(r.Language))
            .OrderBy(x => x.Size)
            .ToArray();

        var dataCount = repositories.Length;
        var perComputerDataCount = repositories.Length / arg.ComputerCount;
        var skipCount = perComputerDataCount * arg.ComputerIndex;
        var firstSkipCount = skipCount / 2;
        var lastSkipCount = skipCount - firstSkipCount;
        var firstTakeCount = perComputerDataCount / 2;
        var lastTakeCount = perComputerDataCount - firstTakeCount;

        var firstRepositories = repositories
            .Skip(firstSkipCount)
            .Take(firstTakeCount)
            .ToArray();

        var lastRepositories = repositories
            .SkipLast(lastSkipCount)
            .TakeLast(lastTakeCount)
            .ToArray();

        Logger.LogInformation($"First: {firstSkipCount} - {firstSkipCount + firstTakeCount}");
        Logger.LogInformation($"Last: {dataCount - (lastSkipCount + lastTakeCount)} - {dataCount - lastSkipCount}");

        var firstTasks = firstRepositories.Select(r => ProcessRepository(r));
        var lastTasks = lastRepositories.Select(r => ProcessRepository(r));

        var tasks = firstTasks.Concat(lastTasks).OrderBy(_ => Guid.NewGuid()).ToArray();


        await Task.WhenAll(tasks);

        var tryCount = 0;
        while (_errorRepositories.Any() && tryCount < 3)
        {
            tryCount++;
            Logger.LogInformation($"Try {tryCount} to process {_errorRepositories.Count} repositories");
            tasks = _errorRepositories.Select(r => ProcessRepository(r)).ToArray();
            _errorRepositories.Clear();
            await Task.WhenAll(tasks);
        }

        Logger.LogInformation("Done");

        if (_errorRepositories.Any()) Logger.LogError($"Failed to process {_errorRepositories.Count} repositories");
    }

    private Task ProcessRepository(Repository repository, CancellationToken token = default)
    {
        var languageStatistics = _languageStatisticsFactory.GetLanguageStatistics(repository.Language);
        var task = Task.Run(async () =>
        {
            try
            {
                await languageStatistics.GetStatisticsAsync(repository, token);
                try
                {
                    await DeleteDirectory(Path.Combine(repository.Language, "Repositories", repository.Name));
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Failed to delete {repository.Name}");
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while processing {Repository}", repository);
                _errorRepositories.Add(repository);
            }
        }, token);

        return task;
    }

    private static Task DeleteDirectory(string path)
    {
        if (!Directory.Exists(path)) return Task.CompletedTask;

        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        var directories = Directory.GetDirectories(path);
        foreach (var directory in directories) DeleteDirectory(directory);

        Directory.Delete(path, false);

        return Task.CompletedTask;
    }
}