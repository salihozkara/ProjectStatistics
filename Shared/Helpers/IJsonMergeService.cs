using Volo.Abp.DependencyInjection;

namespace Shared.Helpers;

public interface IJsonMergeService : ISingletonDependency
{
    IEnumerable<T> Merge<T>(string source, string patch);
    IEnumerable<T> Merge<T>(IEnumerable<T> source, IEnumerable<T> patch);
    IEnumerable<T> Merge<T>(IEnumerable<T> source, IEnumerable<T> patch, Func<T, object> predicate);
    IEnumerable<T> Merge<T>(IEnumerable<T> source, string patchPath);
    IEnumerable<T> Merge<T>(IEnumerable<T> source, string patchPath, Func<T, object> predicate);
}