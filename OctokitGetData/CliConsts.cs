using Octokit;

namespace OctokitGetData;

public class CliConsts
{
    public static readonly List<Language> Languages = new() { Language.CSharp, Language.CPlusPlus, Language.Java };
    public const string OldValuePath = @".\Jsons\repos.json";
}