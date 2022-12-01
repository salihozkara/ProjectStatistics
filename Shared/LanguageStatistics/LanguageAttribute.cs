namespace Shared.LanguageStatistics;

public class LanguageAttribute : Attribute
{
    public LanguageAttribute(params string[] languages)
    {
        Languages = languages;
    }

    public string[] Languages { get; }
}