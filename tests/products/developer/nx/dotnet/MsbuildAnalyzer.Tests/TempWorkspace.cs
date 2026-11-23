using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Xunit;
using Xunit.Sdk;

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

    public string Located(string path) => Path.Combine(root.FullName, path);

    public string Read(string path) => File.ReadAllText(Located(path));

    // --- [ANALYSIS]

    public JsonElement Analyze(params ReadOnlySpan<string> files) => JsonElement.Parse(Run(string.Join('\n', files), root.FullName));

    public static string Run(string input, params ReadOnlySpan<string> arguments) =>
        Launched(Path.GetTempPath(), input, [Path.Combine(AppContext.BaseDirectory, "MsbuildAnalyzer.dll"), .. arguments]);

    public ImmutableArray<string> Written(params ReadOnlySpan<string> arguments) {
        FrozenSet<string> before = Files().ToFrozenSet(StringComparer.Ordinal);
        Launched(root.FullName, "", arguments);
        return [.. Files().Where(file => !before.Contains(file))];
    }

    private string[] Files() =>
        [.. root.EnumerateFiles("*", SearchOption.AllDirectories).Select(file => Path.GetRelativePath(root.FullName, file.FullName).Replace('\\', '/'))];

    private static string Launched(string folder, string input, ReadOnlySpan<string> arguments) {
        using Process process = new() { StartInfo = new ProcessStartInfo("dotnet") { WorkingDirectory = folder, UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true } };
        foreach (string argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        string command = string.Join(' ', process.StartInfo.ArgumentList);
        ConcurrentQueue<string?> stderr = [];
        process.ErrorDataReceived += (_, line) => stderr.Enqueue(line.Data);
        process.Start();
        process.BeginErrorReadLine();
        process.StandardInput.Write(input);
        process.StandardInput.Close();
        string stdout = process.StandardOutput.ReadToEnd();
        Assert.True(process.WaitForExit(TimeSpan.FromMinutes(3)), $"dotnet {command} timed out");
        process.WaitForExit();
        Assert.True(process.ExitCode is 0, string.Create(CultureInfo.InvariantCulture, $"dotnet {command} exited {process.ExitCode}.\n{stdout}\n{string.Join('\n', stderr)}"));
        return stdout;
    }

    // --- [LIFECYCLE]

    public void Dispose() => root.Delete(recursive: true);
}

public abstract class Analyzed : IDisposable {
    private readonly Lazy<JsonElement> report;

    protected Analyzed(params ReadOnlySpan<(string Path, string Text)> files) {
        foreach ((string path, string text) in files) Workspace.Write(path, text);
        string[] paths = [.. files.ToArray().Select(static file => file.Path)];
        report = new(() => Workspace.Analyze(paths));
    }

    protected TempWorkspace Workspace { get; } = new();

    public JsonElement Report => report.Value;

    public JsonElement Node(string file) =>
        Report.GetProperty("nodes").EnumerateArray().Where(node => node.GetProperty("file").ValueEquals(file)).ToArray() is [var node]
            ? node
            : throw FailException.ForFailure($"{file} is not one node of the report, whose failures are {Report.GetProperty("failures")}");

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) Workspace.Dispose();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Reported {
    extension(JsonElement element) {
        public string?[] Strings(params ReadOnlySpan<string> path) =>
            [.. path.ToArray().Aggregate(element, static (held, name) => held.GetProperty(name)).EnumerateArray().Select(static item => item.GetString())];

        public JsonElement Mode(string configuration) =>
            Assert.Single(element.GetProperty("declared").EnumerateArray(), mode => mode.GetProperty("configuration").ValueEquals(configuration));
    }
}
