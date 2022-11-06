using System.Reflection;
using Octokit;
using Octokit.Internal;
using Range = Octokit.Range;

namespace OctokitGetData;

public class OctokitGithubService : IGithubService
{
    private readonly GitHubClient _client;

    public OctokitGithubService()
    {
        _client = new GitHubClient(new ProductHeaderValue("OctokitGetData"));
    }

    public async Task<PaginateResult<Repository>> GetPagedMostStarredRepositories(Language language, int count,
        int perPage = 30, int page = 1, IEnumerable<Repository>? oldValue = null)
    {
        async Task<PaginateResultFactoryResult<Repository>> Factory(int p, object func)
        {
            p %= 10;
            var request = new SearchRepositoriesRequest
            {
                Language = language,
                SortField = RepoSearchSort.Stars,
                Order = SortDirection.Descending,
                PerPage = perPage,
                Page = p
            };
            if (oldValue != null && oldValue.Any())
            {
                request.Stars = Range.LessThanOrEquals(oldValue.Min(r => r.StargazersCount));
            }

            try
            {
                var result = await _client.Search.SearchRepo(request);
                return new PaginateResultFactoryResult<Repository>(result.Items)
                {
                    CurrentPage = p++,
                    PageSize = result.Items.Count,
                    PageCount = count / perPage
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Language : {language} " + e.Message);
            }
        }

        object maxStarredFunc(object sender,int page)
        {
            if (sender is PaginateResult<Repository> result)
            {
                return result?.AllPages?.MinBy(r => r.StargazersCount)?.StargazersCount ?? 0;
            }
            return 0;
        };

        return await PaginateResult<Repository>.CreateFromFactory(Factory, page,maxStarredFunc, oldValue);
    }
}