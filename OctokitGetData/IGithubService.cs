using Octokit;
using Volo.Abp.DependencyInjection;

namespace OctokitGetData;

public interface IGithubService : ISingletonDependency
{
    IAsyncEnumerable<Page<Repository>> GetPagedMostStarredRepositories(Language language, int count,
        int perPage = 30, int page = 1, int maxStarCount = 0);
}