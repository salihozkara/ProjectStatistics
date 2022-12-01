using System.Diagnostics;
using System.Text.Json;

namespace ProjectStatistics;

public static class Resources
{
    private const string ResFolder = "./Res";


    public static IEnumerable<Repository> RepositoriesJson => JsonSerializer
                                                                  .Deserialize<Repository[]>(
                                                                      File.ReadAllText(
                                                                          $"{ResFolder}/repositories.json")) ??
                                                              Array.Empty<Repository>();

    public static class SourceMonitor
    {
        public static readonly string TemplateXml = File.ReadAllText($"{ResFolder}/SourceMonitor/template.xml");

        public static Process SourceMonitorExe => new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = $"{ResFolder}/SourceMonitor/SourceMonitor.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = $"{ResFolder}/SourceMonitor/SourceMonitor.exe"
            }
        };
    }
}