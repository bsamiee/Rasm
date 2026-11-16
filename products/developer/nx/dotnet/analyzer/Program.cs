using System.Text.Json;
using Microsoft.Build.Locator;

namespace MsbuildAnalyzer;

// --- [COMPOSITION] ----------------------------------------------------------------------

internal static class Program {
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true };

    public static int Main(string[] args) =>
        IO.lift(() => args switch {
            [var root] => Registered(Path.GetFullPath(root), () => Write(Path.GetFullPath(root))),
            _ => Error.New("Usage: MsbuildAnalyzer <workspace-root>, with workspace-relative files on stdin, one per line"),
        }).RunSafe().IfFail(static error => {
            Console.Error.WriteLine(error.Message);
            return 1;
        });

    private static Fin<int> Registered(string root, Func<int> run) =>
        toSeq(MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.DotNetSdk, WorkingDirectory = root }))
            .Head
            .ToFin(Error.New($"No .NET SDK resolves from {root}"))
            .Map(sdk => {
                MSBuildLocator.RegisterInstance(sdk);
                return run();
            });

    private static int Write(string root) {
        Console.Out.Write(JsonSerializer.Serialize(Workspace.Analyze(root, Listed()), Json));
        return 0;
    }

    private static Seq<string> Listed() =>
        toSeq(Console.In.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
