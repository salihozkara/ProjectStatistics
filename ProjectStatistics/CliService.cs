using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ProjectStatistics.LanguageStatistics;
using Volo.Abp.DependencyInjection;

namespace ProjectStatistics;

public class CliService : ISingletonDependency
{
    private readonly LanguageStatisticsFactory _languageStatisticsFactory;
    private readonly ConcurrentBag<Repository> _errorRepositories = new();
    private ILogger<CliService> Logger { get; }

    public CliService(LanguageStatisticsFactory languageStatisticsFactory, ILogger<CliService> logger)
    {
        _languageStatisticsFactory = languageStatisticsFactory;
        Logger = logger;
    }

    public async Task RunAsync(string[] args)
    {
        var repositories = Resources.RepositoriesJson;

        var languages = _languageStatisticsFactory.GetSupportedLanguages();

        repositories = repositories
            .Where(r => languages.Contains(r.Language))
#if DEBUG
            .TakeLast(1)
#endif
            .ToArray();
        
        
        var tasks = repositories.Select(r => ProcessRepository(r));

        await Task.WhenAll(tasks);
        
        Logger.LogInformation("Done");
        
        var tryCount = 0;
        while (_errorRepositories.Any() && tryCount < 3)
        {
            tryCount++;
            Logger.LogInformation($"Try {tryCount} to process {_errorRepositories.Count} repositories");
            tasks = _errorRepositories.Select(r => ProcessRepository(r)).ToList();
            _errorRepositories.Clear();
            await Task.WhenAll(tasks);
        }
        
        Logger.LogInformation("Done");
        
        if (_errorRepositories.Any())
        {
            Logger.LogError($"Failed to process {_errorRepositories.Count} repositories");
        }
    }

    private Task ProcessRepository(Repository repository, CancellationToken token = default)
    {
        var languageStatistics = _languageStatisticsFactory.GetLanguageStatistics(repository.Language);
        var task = Task.Run(async () =>
        {
            try
            {
                await languageStatistics.GetStatisticsAsync(repository, token);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while processing {Repository}", repository);
                _errorRepositories.Add(repository);
            }
        }, token);
        
        return task;
    }
}