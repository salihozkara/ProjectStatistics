using System.Collections;
using System.Collections.Concurrent;

namespace OctokitGetData;

public delegate Task<PaginateResultFactoryResult<T>> PaginateResultFactory<T>(int page, object func);
public delegate object PaginateResultFactoryParams(object sender, int page);

public class PaginateResultFactoryResult<T>
{
    public PaginateResultFactoryResult(IEnumerable<T> items)
    {
        Items = new ConcurrentBag<T>(items);
    }

    public ConcurrentBag<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    // public bool HasNextPage { get; set; }
    // public bool HasPreviousPage { get; set; }
    // public int NextPage { get; set; }
    // public int PreviousPage { get; set; }
}

public class PaginateResult<T> : IEnumerable<T>
{
    private readonly ConcurrentDictionary<int, IEnumerable<T>> _pages = new();
    private int _currentPage = 0;
    private int _lastPage = 0;
    private int _totalCount = 0;
    public IEnumerable<T> LastPage => _pages[_lastPage];
    public IEnumerable<T> AllPages => _pages.Values.SelectMany(x => x);

    // public int CurrentPage => _currentPage;
    // public int LastPage => _lastPage;
    // public int TotalCount => _totalCount;

    public static async Task<PaginateResult<T>> CreateFromFactory(PaginateResultFactory<T> factory, int page,
        PaginateResultFactoryParams factoryParams, IEnumerable<T>? oldValue = null)
    {
        var result = new PaginateResult<T>();
        if (oldValue != null)
        {
            result.AddNextPage(oldValue);
        }
        var paramsResult = factoryParams(result, page);

        var factoryResult = await factory(page, paramsResult);
        result.AddNextPage(factoryResult.Items);

        for (var i = page + 1; i<= factoryResult.PageCount; i++)
        {
            try
            {
                paramsResult = factoryParams(result, i);
                factoryResult = await factory(i, paramsResult);
                result.AddNextPage(factoryResult.Items);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting page {0}: {1}", i, e.Message);
            }
        }

        return result;
    }

    public void AddNextPage(IEnumerable<T> items)
    {
        var enumerable = items as T[] ?? items.ToArray();
        _pages.TryAdd(_currentPage, enumerable);
        _lastPage = _currentPage;
        _totalCount += enumerable.Length;
        _currentPage++;
        _totalCount += enumerable.Length;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i <= _lastPage; i++)
        {
            foreach (var item in _pages[i])
            {
                yield return item;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}