namespace ProjectStatistics;

public static class PathHelper
{
    public static Task<string> BuildPath(params string[] paths)
    {
        return Task.Run(() =>
        {
            paths = paths.Append("./").ToArray();
            var path = Path.Combine(paths);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        });
    }

    public static async Task<string> BuildFullPath(params string[] paths)
    {
        return Path.GetFullPath(await BuildPath(paths));
    }
}