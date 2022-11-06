using System.Collections.Concurrent;
using JsonMerge;
using JsonNet.ContractResolvers;
using Newtonsoft.Json;
using Octokit;
using OctokitGetData;
using Shared;
using Volo.Abp.DependencyInjection;

public class CliService : ISingletonDependency
{
    private readonly IGithubService _githubService;
    private readonly IJsonMergeService _jsonMergeService;

    public CliService(IGithubService githubService, IJsonMergeService jsonMergeService)
    {
        _githubService = githubService;
        _jsonMergeService = jsonMergeService;
    }


    public async Task RunAsync(string[] args)
    {
        var oldValue = new List<ExtendRepository>();

        if (File.Exists(CliConsts.OldValuePath))
            oldValue = JsonConvert.DeserializeObject<List<ExtendRepository>>(
                await File.ReadAllTextAsync(CliConsts.OldValuePath)) ?? oldValue;

        var groups = oldValue.GroupBy(x => x.EnumLanguage).ToDictionary(x => x.Key, x => x.ToList());
        
        ConcurrentDictionary<Language, IEnumerable<ExtendRepository>?> result = new();

        var tasks = CliConsts.Languages.Select(language => Task.Run(async () =>
            {
                var languageOldValue = groups.ContainsKey(language) ? groups[language] : new List<ExtendRepository>();
                var minStar = languageOldValue.Any() ? languageOldValue.Min(x => x.StargazersCount) : 0;
                if (languageOldValue.Count >= 3500)
                    return;
                var pages = _githubService.GetPagedMostStarredRepositories(language,
                    languageOldValue.Count + 250, 100, languageOldValue.Count / 100, minStar);

                if (!result.ContainsKey(language)) result.TryAdd(language, new List<ExtendRepository>());

                var extendRepositories = result[language] as List<ExtendRepository>;

                await foreach (var page in pages)
                {
                    extendRepositories?.AddRange(page.Items.Select(r =>
                        ExtendRepository.RepositoryExtend(r, language)));
                }

                result[language] = extendRepositories;
            }))
            .ToList();

        await Task.WhenAll(tasks);

        var newValue = result.SelectMany(x => x.Value).OrderByDescending(r => r.StargazersCount).ToList();


        var repos = _jsonMergeService.Merge(oldValue, newValue).ToList();

        repos = repos.OrderByDescending(r => r.StargazersCount).ToList();
        repos = repos.DistinctBy(r => r.CloneUrl).ToList();


        repos.ToJsonFile($"./Jsons/repos{repos.Count}.json");
        repos.ToJsonFile(CliConsts.OldValuePath);
    }
}