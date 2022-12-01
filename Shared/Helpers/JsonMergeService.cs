using Newtonsoft.Json;

namespace Shared.Helpers;

public class JsonMergeService : IJsonMergeService
{
    public IEnumerable<T> Merge<T>(string source, string patch)
    {
        var sourceList = JsonConvert.DeserializeObject<IEnumerable<T>>(source);
        var patchList = JsonConvert.DeserializeObject<IEnumerable<T>>(patch);
        return Merge(sourceList, patchList);
    }

    public IEnumerable<T> Merge<T>(IEnumerable<T> source, IEnumerable<T> patch)
    {
        var merged = source.Concat(patch);
        return merged.Distinct();
    }

    public IEnumerable<T> Merge<T>(IEnumerable<T> source, IEnumerable<T> patch, Func<T, object> predicate)
    {
        var merged = source.Concat(patch);
        return merged.DistinctBy(predicate);
    }


    public IEnumerable<T> Merge<T>(IEnumerable<T> source, string patchPath)
    {
        var patchContent = File.ReadAllText(patchPath);
        var patch = JsonConvert.DeserializeObject<IEnumerable<T>>(patchContent);
        return Merge(source, patch);
    }

    public IEnumerable<T> Merge<T>(IEnumerable<T> source, string patchPath, Func<T, object> predicate)
    {
        var patchContent = File.ReadAllText(patchPath);
        var patch = JsonConvert.DeserializeObject<IEnumerable<T>>(patchContent);
        return Merge(source, patch, predicate);
    }
}