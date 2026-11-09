using System.Text.Json;
using Microsoft.Build.Locator;

namespace MsbuildAnalyzer;

// --- [COMPOSITION] ----------------------------------------------------------------------

internal static class Program {
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true };

    public static int Main(string[] args) =>
        args switch {
            [var root] => Run(Path.GetFullPath(root)),
            _ => Refuse("Usage: MsbuildAnalyzer <workspace-root>, with workspace-relative files on stdin, one per line"),
        };

    private static int Run(string root) =>
        toSeq(MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.DotNetSdk, WorkingDirectory = root }))
            .Head
            .Match(
                Some: sdk => Write(root, sdk),
                None: () => Refuse($"No .NET SDK resolves from {root}"));

    private static int Write(string root, VisualStudioInstance sdk) {
        MSBuildLocator.RegisterInstance(sdk);
        Console.Out.Write(JsonSerializer.Serialize(Workspace.Analyze(root, toSeq(Console.In.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))), Json));
        return 0;
    }

    private static int Refuse(string message) {
        Console.Error.WriteLine(message);
        return 2;
    }
}
