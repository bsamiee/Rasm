using System.Text.Json;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;

namespace MsbuildAnalyzer;

// --- [COMPOSITION] ----------------------------------------------------------------------

internal static class Program {
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true };

    public static int Main(string[] args) =>
        args switch {
            ["version", var version] => Registered(Environment.CurrentDirectory, () => Versioned(version)),
            [var root] => Registered(Path.GetFullPath(root), () => Write(Path.GetFullPath(root))),
            _ => Refuse("Usage: MsbuildAnalyzer <workspace-root>, with workspace-relative files on stdin, one per line; or MsbuildAnalyzer version <version>, with a project file on stdin"),
        };

    private static int Registered(string root, Func<int> run) =>
        toSeq(MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.DotNetSdk, WorkingDirectory = root }))
            .Head
            .Match(
                Some: sdk => {
                    MSBuildLocator.RegisterInstance(sdk);
                    return run();
                },
                None: () => Refuse($"No .NET SDK resolves from {root}"));

    private static int Write(string root) {
        Console.Out.Write(JsonSerializer.Serialize(Workspace.Analyze(root, toSeq(Console.In.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))), Json));
        return 0;
    }

    private static int Versioned(string version) {
        using ProjectCollection collection = new();
        using XmlReader reader = XmlReader.Create(Console.In);
        ProjectRootElement project = ProjectRootElement.Create(reader, collection, preserveFormatting: true);
        _ = project.AddProperty("Version", version);
        project.Save(Console.Out);
        return 0;
    }

    private static int Refuse(string message) {
        Console.Error.WriteLine(message);
        return 2;
    }
}
