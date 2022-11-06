using Volo.Abp.DependencyInjection;

namespace ProjectStatistics;

public interface ILanguageStatistics : ISingletonDependency
{
    Task GetLanguageStatisticsAsync(Repository repository);
}