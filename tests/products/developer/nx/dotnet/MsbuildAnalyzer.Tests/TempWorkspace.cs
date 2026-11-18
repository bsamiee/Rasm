using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Xunit;

namespace MsbuildAnalyzer.Tests;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class TempWorkspace : IDisposable {
    private readonly DirectoryInfo root = Directory.CreateTempSubdirectory("nx-dotnet-");

    // --- [FILES]

    public static string Project(params ReadOnlySpan<string> elements) =>
        $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
          </PropertyGroup>
          {string.Concat(elements)}
        </Project>
        """;

    public TempWorkspace Write(string path, string text) {
        FileInfo file = new(Path.Combine(root.FullName, path));
        file.Directory?.Create();
        File.WriteAllText(file.FullName, text);
        return this;
    }

    // --- [ANALYSIS]

    public string Located(string path) => Path.Combine(root.FullName, path);

    public string Read(string path) => File.ReadAllText(Located(path));

    public JsonElement Analyze(params ReadOnlySpan<string> files) => JsonElement.Parse(Run(string.Join('\n', files.ToArray()), root.FullName));

    public static string Run(string input, params ReadOnlySpan<string> arguments) {
        using Process process = new() { StartInfo = new ProcessStartInfo("dotnet") { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true } };
        foreach (string argument in (ReadOnlySpan<string>)[Path.Combine(AppContext.BaseDirectory, "MsbuildAnalyzer.dll"), .. arguments]) process.StartInfo.ArgumentList.Add(argument);
        process.Start();
        process.StandardInput.Write(input);
        process.StandardInput.Close();
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        Assert.True(process.WaitForExit(TimeSpan.FromMinutes(3)), "Analyzer timed out");
        Assert.True(process.ExitCode is 0, string.Create(CultureInfo.InvariantCulture, $"Analyzer exited {process.ExitCode}. stderr:\n{stderr}"));
        return stdout;
    }

    public JsonElement Node(string project) =>
        Assert.Single(Analyze(project).GetProperty("nodes").EnumerateArray(), node => node.GetProperty("file").ValueEquals(project));

    // --- [LIFECYCLE]

    public void Dispose() => root.Delete(recursive: true);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Reported {
    extension(JsonElement element) {
        public string?[] Strings(params ReadOnlySpan<string> path) =>
            [.. path.ToArray().Aggregate(element, static (held, name) => held.GetProperty(name)).EnumerateArray().Select(static item => item.GetString())];
    }
}
