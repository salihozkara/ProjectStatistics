using Microsoft.Extensions.Logging;

namespace ProjectStatistics;

[Language("C#", "C++", "Java")]
public class SourceMonitorLanguageStatistics : ILanguageStatistics
{
    private const string ProjectNameReplacement = "{{project_name}}";
    private const string ProjectDirectoryReplacement = "{{project_directory}}";
    private const string ProjectLanguageReplacement = "{{project_language}}";
    private const string ReportsPathReplacement = "{{reports_path}}";


    private readonly string _xmlTemplate;
    private readonly ILogger<SourceMonitorLanguageStatistics> _logger;
    private readonly ProcessHelper _processHelper;
    private GitHelper GitHelper { get; }

    public SourceMonitorLanguageStatistics(GitHelper gitHelper, ProcessHelper processHelper,
        ILogger<SourceMonitorLanguageStatistics> logger)
    {
        GitHelper = gitHelper;
        _processHelper = processHelper;
        _logger = logger;
        _xmlTemplate = File.ReadAllText(CliConsts.SourceMonitorXmlTemplatePath);
    }

    public Task GetLanguageStatisticsAsync(Repository repository)
    {
        return ProcessRepository(repository);
    }

    private async Task ProcessRepository(Repository repository)
    {
        try
        {
            var isSuccess = await GitHelper.CloneRepository(repository);

            if (!isSuccess)
            {
                return;
            }

            await CalculateStatisticsUsingSourceMonitor(repository);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while processing repository {repository}", repository.Name);
        }
    }

    private Task<bool> CalculateStatisticsUsingSourceMonitor(Repository repository)
    {
        return Task.Run(async () =>
        {
            _logger.LogInformation("Calculating statistics for {RepositoryName}", repository.Name);
            var xmlPath = await CreateSourceMonitorXml(repository);

            var isSuccess = await _processHelper.RunAsync("cmd.exe", $"/C SourceMonitor.exe /C \"{xmlPath}\"");

            if (isSuccess)
            {
                _logger.LogInformation("Statistics for {RepositoryName} calculated successfully", repository.Name);
            }
            else
            {
                _logger.LogError("Error while calculating statistics for {RepositoryName}", repository.Name);
            }

            return isSuccess;
        });
    }

    private Task<string> CreateSourceMonitorXml(Repository repository)
    {
        return Task.Run(async () =>
        {
            var directory = await PathHelper.BuildFullPath(repository.Language, "Repositories", repository.Name);

            var reportsPath = await PathHelper.BuildFullPath(repository.Language, "Reports");

            var xml = _xmlTemplate
                .Replace(ProjectNameReplacement, repository.Name)
                .Replace(ProjectDirectoryReplacement, directory)
                .Replace(ProjectLanguageReplacement, repository.Language)
                .Replace(ReportsPathReplacement, reportsPath);
            var xmlPath = Path.Combine(directory, $"{repository.Name}.xml");
            await File.WriteAllTextAsync(xmlPath, xml);
            return xmlPath;
        });
    }
}