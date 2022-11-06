using Octokit;
using Volo.Abp.DependencyInjection;

namespace OctokitGetData;

public interface IGithubService : ISingletonDependency
{
    Task<PaginateResult<Repository>> GetPagedMostStarredRepositories(Language language, int count,
        int perPage = 30, int page = 1, IEnumerable<Repository>? oldValue = null);
}