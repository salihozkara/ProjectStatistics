namespace OctokitGetData;

public class Page<T>
{
    public int TotalCount { get; set; }
    public int PageCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize => Items.Count();
    public IEnumerable<T> Items { get; set; }
}