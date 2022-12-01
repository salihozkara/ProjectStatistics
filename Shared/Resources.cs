﻿using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Shared;

public static class Resources
{
    private const string ResFolder = "Res";

    private static readonly string DynamicResFolder = GetOrCreateResFolder();

    private static string GetOrCreateResFolder()
    {
        var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        const string slnName = "ProjectStatistics";
        var privateSlnFolderName = "."+slnName.ToLower();
        var resFolder = Path.Combine(userFolder, privateSlnFolderName, ResFolder);
        if (!Directory.Exists(resFolder))         Directory.CreateDirectory(resFolder);
        var baseResFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, ResFolder);
        foreach (var file in Directory.GetFiles(baseResFolder, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = file[(baseResFolder.Length + 1)..];
            var targetFile = Path.Combine(resFolder, relativePath);
            if(File.Exists(targetFile)) continue;
            var targetFolder = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder!);
            File.Copy(file, targetFile, true);
        }

        return resFolder;
    }
    public static ResValue<IEnumerable<Repository>> RepositoriesJson => new($"{DynamicResFolder}/repositories.json",
        JsonSerializer
            .Deserialize<Repository[]>(
                File.ReadAllText(
                    $"{DynamicResFolder}/repositories.json")) ??
        Array.Empty<Repository>());

    public static class SourceMonitor
    {
        public static readonly ResValue<string> TemplateXml = new($"{DynamicResFolder}/SourceMonitor/template.xml",
            $"{DynamicResFolder}/SourceMonitor/template.xml");

        public static ResValue<Process> SourceMonitorExe => new($"{DynamicResFolder}/SourceMonitor/SourceMonitor.exe", new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = $"{ResFolder}/SourceMonitor/SourceMonitor.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = $"{DynamicResFolder}/SourceMonitor/SourceMonitor.exe"
            }
        });
    }

    public static class Jsons
    {
        public static string Path => $"{DynamicResFolder}/Jsons";
        
        private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            MaxDepth = 100,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Double,
            StringEscapeHandling = StringEscapeHandling.Default,
            Culture = CultureInfo.InvariantCulture,
            CheckAdditionalContent = true,
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };
        public static Dictionary<string, dynamic?> Values
        {
            get
            {
                var jsons = new Dictionary<string, dynamic?>();
                var jsonsFolder = $"{DynamicResFolder}/Jsons";
            
                foreach (var file in Directory.GetFiles(jsonsFolder, "*.json", SearchOption.AllDirectories))
                {
                    var relativePath = file[(jsonsFolder.Length + 1)..];
                    var json = JsonConvert.DeserializeObject(File.ReadAllText(file), typeof(ExpandoObject), _jsonSerializerSettings);
                    jsons.Add(relativePath, json);
                }
            
                return jsons;
            }
        }
    
        public static ResValue<T?> GetJson<T>(string path)
        {
            return new ResValue<T?>($"{Path}/{path}", JsonSerializer.Deserialize<T>(File.ReadAllText($"{Path}/{path}")));
        }
        
        public static void SaveJson<T>(string path, T value)
        {
            var json = JsonConvert.SerializeObject(value, _jsonSerializerSettings);
            var pathToFile = $"{Path}/{path}";
            var folder = System.IO.Path.GetDirectoryName(pathToFile);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder!);
            
            File.WriteAllText(pathToFile, json);
        }
    }

}

public struct ResValue<T>
{
    public string Path { get; }
    public T Value { get; }

    public ResValue(string path, T value)
    {
        Path = System.IO.Path.GetFullPath(path);
        Value = value;
    }
}