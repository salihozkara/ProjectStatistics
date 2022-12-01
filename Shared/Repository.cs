using System.Diagnostics;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ProjectStatistics;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + ",nq}")]
public class Repository
{
    private string GetDebuggerDisplay => $"{Name} ({Index})";
    public Repository(string name, string cloneUrl, string language, long size)
    {
        Name = name;
        CloneUrl = cloneUrl;
        Language = language;
        Size = size;
    }

    public string Name { get; set; }
    public string CloneUrl { get; set; }
    public string Language { get; set; }
    public long Size { get; set; }
    public int Index { get; set; }
    
    public string FullName { get; set; }


    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
    
    
}