using Microsoft.Extensions.Logging;
using ProjectStatistics.DependencyProcesses;
using ProjectStatistics.Helpers;

namespace ProjectStatistics.LanguageStatistics;

[Language("C#", "C++", "Java")]
[DependencyProcess<SourceMonitorDependencyProcess>]
public class SourceMonitorLanguageStatistics : ILanguageStatistics
{
    private const string ProjectNameReplacement = "{{project_name}}";
    private const string ProjectDirectoryReplacement = "{{project_directory}}";
    private const string ProjectFileDirectoryReplacement = "{{project_file_directory}}";
    private const string ProjectLanguageReplacement = "{{project_language}}";
    private const string ReportsPathReplacement = "{{reports_path}}";

    private readonly ILogger<SourceMonitorLanguageStatistics> _logger;
    private readonly ProcessHelper _processHelper;

    public SourceMonitorLanguageStatistics(GitHelper gitHelper, ProcessHelper processHelper,
        ILogger<SourceMonitorLanguageStatistics> logger)
    {
        GitHelper = gitHelper;
        _processHelper = processHelper;
        _logger = logger;
    }

    private GitHelper GitHelper { get; }

    public Task GetStatisticsAsync(Repository repository, CancellationToken token = default)
    {
        return ProcessRepository(repository, token);
    }

    private async Task ProcessRepository(Repository repository, CancellationToken token = default)
    {
        var reportsPath = Path.Combine(repository.Language, "Reports", repository.Name + ".xml");
        if (File.Exists(reportsPath))
        {
            _logger.LogInformation("Reports already exist for {RepositoryName}. Skipping...", repository.Name);
            return;
        }

        var isSuccess = await GitHelper.CloneRepository(repository, token);

        if (!isSuccess) return;

        await CalculateStatisticsUsingSourceMonitor(repository);
    }

    private Task<bool> CalculateStatisticsUsingSourceMonitor(Repository repository)
    {
        return Task.Run(async () =>
        {
            _logger.LogInformation("Calculating statistics for {RepositoryName}", repository.Name);
            var xmlPath = await CreateSourceMonitorXml(repository);

            var result = await _processHelper.RunAsync(Resources.SourceMonitor.SourceMonitorExe, $"/C \"{xmlPath}\"");
            if (result.Success)
                _logger.LogInformation("Statistics for {RepositoryName} calculated successfully", repository.Name);
            else
                _logger.LogError("Error while calculating statistics for {RepositoryName}", repository.Name);

            return result.Success;
        });
    }

    private Task<string> CreateSourceMonitorXml(Repository repository)
    {
        return Task.Run(async () =>
        {
            var xmlDirectory = await PathHelper.BuildFullPath(repository.Language, "SourceMonitor", repository.Name);

            var reportsPath = await PathHelper.BuildFullPath(repository.Language, "Reports");
            
            var projectDirectory = await PathHelper.BuildFullPath(repository.Language, "Repositories", repository.Name);

            var xmlPath = Path.Combine(xmlDirectory, $"{repository.Name}.xml");

            if (File.Exists(xmlPath))
            {
                _logger.LogInformation("SourceMonitor xml file already exists for {RepositoryName}. Skipping...",
                    repository.Name);
                return xmlPath;
            }

            var xml = Resources.SourceMonitor.TemplateXml
                .Replace(ProjectNameReplacement, repository.Name)
                .Replace(ProjectDirectoryReplacement, projectDirectory)
                .Replace(ProjectFileDirectoryReplacement, xmlDirectory)
                .Replace(ProjectLanguageReplacement, repository.Language)
                .Replace(ReportsPathReplacement, reportsPath);
            await File.WriteAllTextAsync(xmlPath, xml);
            return xmlPath;
        });
    }
}