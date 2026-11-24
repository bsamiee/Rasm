using System.Globalization;
using System.Text.Json;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using Microsoft.Build.Logging;

namespace MsbuildAnalyzer;

// --- [MODELS] ---------------------------------------------------------------------------

internal sealed record Outcome(bool Succeeded, DateTimeOffset Started, DateTimeOffset Finished, ImmutableArray<string> Lines);

// --- [COMPOSITION] ----------------------------------------------------------------------

internal static class Program {
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true };

    public static int Main(string[] args) =>
        (args switch {
            ["upgrade", var solution] => Upgraded(solution, dry: false),
            ["upgrade", var solution, "--dry-run"] => Upgraded(solution, dry: true),
            ["results", var root, var log] => Registered(Path.GetFullPath(root), () => IO.lift(() => Results(Path.GetFullPath(log)))),
            [var root] => Registered(Path.GetFullPath(root), () => IO.lift(() => Write(Path.GetFullPath(root)))),
            _ => IO.fail<int>(Error.New("Usage: MsbuildAnalyzer <workspace-root>, with workspace-relative files on stdin, one per line, or MsbuildAnalyzer upgrade <solution> [--dry-run], or MsbuildAnalyzer results <workspace-root> <binlog>")),
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
        Console.Out.Write(JsonSerializer.Serialize(Workspace.Analyze(root, toSeq(Console.In.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))), Json));
        return 0;
    }

    private static int Results(string log) {
        using BuildEventArgsReader reader = BinaryLogReplayEventSource.OpenBuildEventsReader(log);
        Seq<BuildEventArgs> raised = toSeq(LanguageExt.List.unfold(reader, static held => Optional(held.Read()).Map(read => (read, held)))).Strict();
        ILookup<string, string> lines = raised
            .Choose(static read => read switch {
                BuildErrorEventArgs { ProjectFile: { } project } error => Some((Project: project, Line: string.Create(CultureInfo.InvariantCulture, $"{error.File}({error.LineNumber},{error.ColumnNumber}): error {error.Code}: {error.Message}"))),
                BuildWarningEventArgs { ProjectFile: { } project } warning => Some((Project: project, Line: string.Create(CultureInfo.InvariantCulture, $"{warning.File}({warning.LineNumber},{warning.ColumnNumber}): warning {warning.Code}: {warning.Message}"))),
                _ => None,
            })
            .AsEnumerable()
            .ToLookup(static line => line.Project, static line => line.Line, StringComparer.Ordinal);
        ILookup<string, DateTime> started = raised
            .Choose(static read => read is ProjectStartedEventArgs { ProjectFile: { } project } begun ? Some((Project: project, begun.Timestamp)) : None)
            .AsEnumerable()
            .ToLookup(static begun => begun.Project, static begun => begun.Timestamp, StringComparer.Ordinal);
        FrozenDictionary<(int Project, int Target), string> targets = raised
            .Choose(static read => read is TargetStartedEventArgs { BuildEventContext: { } at } begun ? Some((Key: (at.ProjectContextId, at.TargetId), begun.TargetName)) : None)
            .AsEnumerable()
            .ToFrozenDictionary(static begun => begun.Key, static begun => begun.TargetName);
        FrozenSet<int> roots = raised
            .Choose(static read => read is ProjectStartedEventArgs { ParentProjectBuildEventContext.ProjectContextId: BuildEventContext.InvalidProjectContextId, BuildEventContext: { } at } ? Some(at.ProjectContextId) : None)
            .AsEnumerable()
            .ToFrozenSet();
        FrozenSet<int> requested = raised
            .Choose(read => read is ProjectStartedEventArgs { ParentProjectBuildEventContext: { } parent, BuildEventContext: { } at } begun
                && roots.Contains(parent.ProjectContextId)
                && targets.TryGetValue((parent.ProjectContextId, parent.TargetId), out string? named)
                && named.Equals(begun.TargetNames, StringComparison.OrdinalIgnoreCase)
                    ? Some(at.ProjectContextId)
                    : None)
            .AsEnumerable()
            .ToFrozenSet();
        Console.Out.Write(JsonSerializer.Serialize(
            raised.Choose(read => read is ProjectFinishedEventArgs { ProjectFile: { } project, BuildEventContext: { } at } finished && requested.Contains(at.ProjectContextId) ? Some((Project: project, finished.Succeeded, finished.Timestamp)) : None).AsEnumerable().GroupBy(static finished => finished.Project, StringComparer.Ordinal).ToDictionary(
                static finished => finished.Key,
                finished => new Outcome(finished.All(static each => each.Succeeded), new DateTimeOffset(started[finished.Key].Min()), new DateTimeOffset(finished.Max(static each => each.Timestamp)), [.. lines[finished.Key].Distinct(StringComparer.Ordinal)]),
                StringComparer.Ordinal),
            Json));
        return 0;
    }
}
