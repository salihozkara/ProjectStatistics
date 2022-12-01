// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

var gitProcessies = Process.GetProcessesByName("git");

foreach (var process in gitProcessies) process.Kill();