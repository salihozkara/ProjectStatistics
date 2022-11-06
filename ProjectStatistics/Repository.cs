using System.Text.Json;

namespace ProjectStatistics;

public class Repository
{
    public string Name { get; set; }
    public string CloneUrl { get; set; }
    public string Language { get; set; }

    public Repository(string name, string cloneUrl, string language)
    {
        Name = name;
        CloneUrl = cloneUrl;
        Language = language;
    }

    public Repository()
    {
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}