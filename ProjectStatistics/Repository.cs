using System.Text.Json;

namespace ProjectStatistics;

public class Repository
{
    public string Name { get; set; }
    public string CloneUrl { get; set; }
    public string Language { get; set; }
    public long Size { get; set; }

    public Repository(string name, string cloneUrl, string language, long size)
    {
        Name = name;
        CloneUrl = cloneUrl;
        Language = language;
        Size = size;
    }
    

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}