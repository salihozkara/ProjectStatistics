using Shared;

var path = @"E:\merge";

var files = Directory.GetFiles(path)
    .Select(Path.GetFileNameWithoutExtension);

var reposNames = Resources.RepositoriesJson.Select(r => r.Name);

var reposNamesGrouped = Resources.RepositoriesJson.GroupBy(r => r.Name).Where(r => r.Count() > 1).ToList();

var different = reposNames.Except(files).Order().ToList();

different.ToJsonFile();