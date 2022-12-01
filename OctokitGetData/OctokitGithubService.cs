using Microsoft.Extensions.Logging;
using Octokit;
using static Octokit.Range;

namespace OctokitGetData;

public class OctokitGithubService : IGithubService
{
    private readonly GitHubClient _client;
    private readonly ILogger<OctokitGithubService> _logger;

    public OctokitGithubService(ILogger<OctokitGithubService> logger)
    {
        _logger = logger;
        _client = new GitHubClient(new ProductHeaderValue("OctokitGetData"));
    }

    public async IAsyncEnumerable<Page<Repository>> GetPagedMostStarredRepositories(Language language, int count,
        int perPage = 30, int page = 1, int maxStarCount = -1)
    {
        for (var i = page; i <= count / perPage; i++)
            yield return new Page<Repository>
            {
                Items = await Factory(i, language, perPage, maxStarCount),
                TotalCount = count,
                CurrentPage = i,
                PageCount = count / perPage
            };
    }

    private async Task<IEnumerable<Repository>> Factory(int page, Language language, int perPage, int maxStarCount)
    {
        page %= 10;
        var request = new SearchRepositoriesRequest
        {
            Language = language,
            SortField = RepoSearchSort.Stars,
            Order = SortDirection.Descending,
            PerPage = perPage,
            Page = page
        };
        if (maxStarCount > 0) request.Stars = LessThanOrEquals(maxStarCount);

        try
        {
            var result = await _client.Search.SearchRepo(request);
            return result.Items;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error while getting repositories from github Language: {language}, Page: {page}, PerPage: {perPage}, MaxStarCount: {maxStarCount}, Exception: {exception}",
                language, page, perPage, maxStarCount, e.Message);
            return new List<Repository>();
        }
    }
}