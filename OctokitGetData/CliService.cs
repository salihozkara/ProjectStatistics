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

    public CliService(OctokitGithubService githubService, IJsonMergeService jsonMergeService)
    {
        _githubService = githubService;
        _jsonMergeService = jsonMergeService;
    }


    public async Task RunAsync(string[] args)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new PrivateSetterContractResolver()
        };
        var oldValue = new List<ExtendRepository>();

        if(File.Exists(CliConsts.OldValuePath))
            oldValue = JsonConvert.DeserializeObject<List<ExtendRepository>>(await File.ReadAllTextAsync(CliConsts.OldValuePath), settings);

        var groups = oldValue.GroupBy(x => x.EnumLanguage).ToDictionary(x => x.Key, x => x.ToList());
        ConcurrentDictionary<Language,PaginateResult<ExtendRepository>> result = new ();
        
        var tasks = CliConsts.Languages.Select(language => Task.Run(async () =>
            {
                
                var languageOldValue = groups.ContainsKey(language) ? groups[language] : new List<ExtendRepository>();
                if(languageOldValue.Count >= 3500)
                    return;
                var repositories = await _githubService.GetPagedMostStarredRepositories(language, languageOldValue.Count + 250, 100, languageOldValue.Count / 100, languageOldValue);
                var newRepositories = repositories.Select(r =>ExtendRepository.RepositoryExtend(r, language)).ToList();
                if (!result.ContainsKey(language)) result.TryAdd(language, new PaginateResult<ExtendRepository>());
                result[language].AddNextPage(newRepositories);
            }))
            .ToList();
        
        await Task.WhenAll(tasks);
        
        var test2 = result.SelectMany(x=>x.Value).OrderByDescending(r=>r.StargazersCount).ToList();
        
         
        var repos = _jsonMergeService.Merge(test2, 
            oldValue).ToList();
        
        repos = repos.OrderByDescending(r=>r.StargazersCount).ToList();
        repos = repos.DistinctBy(r=>r.CloneUrl).ToList();
        
        
        repos.ToJsonFile($"./Jsons/repos{repos.Count}.json");
        repos.ToJsonFile(CliConsts.OldValuePath);

    }
}