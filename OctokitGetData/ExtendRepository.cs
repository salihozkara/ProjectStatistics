using JsonNet.ContractResolvers;
using Newtonsoft.Json;
using Octokit;

namespace OctokitGetData;

public class ExtendRepository : Repository
{
    public ExtendRepository(string url, string htmlUrl, string cloneUrl, string gitUrl, string sshUrl, string svnUrl,
        string mirrorUrl, long id, string nodeId, User owner, string name, string fullName, bool isTemplate,
        string description, string homepage, string language, bool @private, bool fork, int forksCount,
        int stargazersCount, string defaultBranch, int openIssuesCount, DateTimeOffset? pushedAt,
        DateTimeOffset createdAt, DateTimeOffset updatedAt, RepositoryPermissions permissions, Repository parent,
        Repository source, LicenseMetadata license, bool hasIssues, bool hasWiki, bool hasDownloads, bool hasPages,
        int subscribersCount, long size, bool? allowRebaseMerge, bool? allowSquashMerge, bool? allowMergeCommit,
        bool archived, int watchersCount, bool? deleteBranchOnMerge, RepositoryVisibility visibility,
        IEnumerable<string> topics, bool? allowAutoMerge, Language enumLanguage) : base(url, htmlUrl, cloneUrl, gitUrl,
        sshUrl, svnUrl, mirrorUrl, id, nodeId, owner, name, fullName, isTemplate, description, homepage, language,
        @private, fork, forksCount, stargazersCount, defaultBranch, openIssuesCount, pushedAt, createdAt, updatedAt,
        permissions, parent, source, license, hasIssues, hasWiki, hasDownloads, hasPages, subscribersCount, size,
        allowRebaseMerge, allowSquashMerge, allowMergeCommit, archived, watchersCount, deleteBranchOnMerge, visibility,
        topics, allowAutoMerge)
    {
        EnumLanguage = enumLanguage;
    }

    private ExtendRepository()
    {
    }

    public Language EnumLanguage { get; set; }

    public static ExtendRepository RepositoryExtend(Repository repository, Language enumLanguage)
    {
        var result = new ExtendRepository();
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new PrivateSetterContractResolver()
        };

        JsonConvert.PopulateObject(JsonConvert.SerializeObject(repository), result, settings);

        result.EnumLanguage = enumLanguage;

        return result;
    }
}