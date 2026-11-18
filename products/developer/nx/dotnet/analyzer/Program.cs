using System.Text.Json;
using Microsoft.Build.Locator;

namespace MsbuildAnalyzer;

// --- [COMPOSITION] ----------------------------------------------------------------------

internal static class Program {
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true };

    public static int Main(string[] args) =>
        (args switch {
            ["upgrade", var solution] => Upgraded(solution, dry: false),
            ["upgrade", var solution, "--dry-run"] => Upgraded(solution, dry: true),
            [var root] => Registered(Path.GetFullPath(root), () => IO.lift(() => Write(Path.GetFullPath(root)))),
            _ => IO.fail<int>(Error.New("Usage: MsbuildAnalyzer <workspace-root>, with workspace-relative files on stdin, one per line, or MsbuildAnalyzer upgrade <solution> [--dry-run]")),
        }).RunSafe().IfFail(static error => {
            Console.Error.WriteLine(error.Message);
            return 1;
        });

    private static IO<int> Upgraded(string solution, bool dry) =>
        Path.GetFullPath(solution) is var path && Path.GetDirectoryName(path) is { } root
            ? Registered(root, () => Catalog.Upgrade(root, path, dry))
            : IO.fail<int>(Error.New($"{solution} names no folder"));

    private static IO<int> Registered(string root, Func<IO<int>> run) =>
        from sdk in IO.lift(() => toSeq(MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.DotNetSdk, WorkingDirectory = root })).Head.ToFin(Error.New($"No .NET SDK resolves from {root}")))
        from _ in IO.lift(() => MSBuildLocator.RegisterInstance(sdk))
        from code in run()
        select code;

    private static int Write(string root) {
        Console.Out.Write(JsonSerializer.Serialize(Workspace.Analyze(root, Listed()), Json));
        return 0;
    }

    private static Seq<string> Listed() =>
        toSeq(Console.In.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
