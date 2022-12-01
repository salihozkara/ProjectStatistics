using Castle.Components.DictionaryAdapter.Xml;
using Shared;


var a = Resources.RepositoriesJson.Value;
var path = @"E:\merge";

var files = Directory.GetFiles(path)
    .Select(Path.GetFileNameWithoutExtension);

var reposNames = Resources.RepositoriesJson.Value.Select(r => r.Name);

var reposNamesGrouped = Resources.RepositoriesJson.Value.GroupBy(r => r.Name).Where(r => r.Count() > 1).ToList();

var different = reposNames.Except(files).Order().ToList();

different.ToJsonFile();