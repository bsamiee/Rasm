using Rasm.Bridge.Contract;
using Rhino;
using Rhino.Display;

namespace Rasm.ScenarioKit;

// --- [TYPES] ---------------------------------------------------------------------------

[AttributeUsage(AttributeTargets.Method)]
public sealed class RhinoScenarioAttribute(string theme) : Attribute {
    public string Theme { get; } = theme;
    public string[] Requires { get; init; } = [];
    public int BudgetMs { get; init; }
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Snapshot(string Path, int Width, int Height, bool OnFailure, ArtifactRef Artifact);

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class ScenarioContext {
    internal interface IScope : IDisposable {
        internal bool IsLive { get; }
        internal Fin<Unit> Complete();
    }

    private readonly List<IScope> scopes = [];
    private readonly Action<string, object?> sink;
    private readonly Func<EvidenceName, Fin<Snapshot>> captureRhino;
    private readonly Func<EvidenceName, Fin<Snapshot>> captureGrasshopper;

    internal ScenarioContext(
        RhinoDoc doc, DirectoryInfo scratchDirectory, string scenario, Action<string, object?> sink,
        Func<EvidenceName, Fin<Snapshot>> captureRhino, Func<EvidenceName, Fin<Snapshot>> captureGrasshopper) {
        Doc = doc;
        ScratchDirectory = scratchDirectory;
        Scenario = scenario;
        this.sink = sink;
        this.captureRhino = captureRhino;
        this.captureGrasshopper = captureGrasshopper;
    }

    public RhinoDoc Doc { get; }
    public DirectoryInfo ScratchDirectory { get; }
    public string Scenario { get; }

    internal int FactCount { get; private set; }

    internal RhinoView? RealizedView =>
        scopes.Exists(static scope => scope is RhinoDocumentScope { IsLive: true, ViewportRealized: true }) ? Doc.Views.ActiveView : null;

    // --- [BRACKETS]

    public Fin<T> WithRhinoDocument<T>(Func<RhinoDocumentScope, Fin<T>> action) =>
        Use(RhinoDocumentScope.Open(this), action);

    public Fin<T> WithGrasshopperDocument<T>(Func<GrasshopperDocumentScope, Fin<T>> action) =>
        Use(GrasshopperDocumentScope.Open(this), action);

    public Fin<Unit> Case(EvidenceName name, Func<Fin<Unit>> action) {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(action);
        Fact(EvidenceName.Create($"{EvidenceRole.Assertion.FactPrefix}{name.Key}.start"), value: true);
        Fin<Unit> result = CaptureHost(action);
        Fact(
            EvidenceName.Create($"{EvidenceRole.Assertion.FactPrefix}{name.Key}.status"),
            result.Match(Succ: static _ => "ok", Fail: static error => $"failed:{error.Message}"));
        return result;
    }

    // --- [EVIDENCE]

    public Fin<Unit> Require(EvidenceName label, bool observed) {
        ArgumentNullException.ThrowIfNull(label);
        Fact(label, observed);
        return observed ? unit : Error.New($"require '{label.Key}': observed false");
    }

    public Fin<T> Expect<T>(EvidenceName label, Fin<T> projection) {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(projection);
        Fact(label, projection.Match<object?>(Succ: static value => value, Fail: static error => $"FAIL: {error.Message}"));
        return projection;
    }

    public void Manifest<T>(EvidenceRole role, EvidenceName name, T value) {
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(name);
        _ = role.IsManifest ? role : throw new ArgumentOutOfRangeException(nameof(role), role.Key, "role does not own a manifest lane");
        Fact(EvidenceName.Create(role.FactPrefix + name.Key), value);
    }

    public void Artifact(FileInfo file, EvidenceRole role) {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(role);
        Fact(EvidenceName.Create(EvidenceRole.Artifact.FactPrefix + role.Key), file.FullName);
    }

    public void Fact(EvidenceName key, object? value) {
        ArgumentNullException.ThrowIfNull(key);
        FactCount++;
        sink(key.Key, value);
    }

    public Fin<Snapshot> CaptureSnapshot(EvidenceName label) {
        ArgumentNullException.ThrowIfNull(label);
        return captureRhino(label);
    }

    public Fin<Snapshot> CaptureGrasshopper(EvidenceName label) {
        ArgumentNullException.ThrowIfNull(label);
        return captureGrasshopper(label);
    }

    public FileInfo ScratchFile(string fileName) {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        string path = Path.GetFullPath(fileName, ScratchDirectory.FullName);
        string relative = Path.GetRelativePath(ScratchDirectory.FullName, path);
        bool escapes = Path.IsPathFullyQualified(fileName)
            || string.Equals(relative, "..", StringComparison.Ordinal)
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
        _ = escapes ? throw new ArgumentOutOfRangeException(nameof(fileName), fileName, "scratch file must stay under the scenario scratch root") : path;
        Fact(EvidenceName.Create("scratch.path"), path);
        return new FileInfo(path);
    }

    // --- [HOST_FUNNEL]

    internal static Fin<T> CaptureHost<T>(Func<Fin<T>> body) {
        try {
            return body();
        } catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException and not AccessViolationException) {
            return Error.New(error.Message, error);
        }
    }

    internal int DrainScopes() {
        int leaked = 0;
        for (int index = scopes.Count - 1; index >= 0; index--) {
            IScope scope = scopes[index];
            if (scope.IsLive) {
                leaked++;
                _ = scope.Complete();
            }
        }
        scopes.Clear();
        return leaked;
    }

    internal void Register(IScope scope) => scopes.Add(scope);

    private static Fin<T> Use<TScope, T>(Fin<TScope> acquired, Func<TScope, Fin<T>> action) where TScope : IScope {
        ArgumentNullException.ThrowIfNull(action);
        return acquired.Bind(scope => CaptureHost(() => action(scope)).BiBind(
            Succ: value => scope.Complete().Map(_ => value),
            Fail: primary => scope.Complete().BiBind(
                Succ: _ => Fin.Fail<T>(primary),
                Fail: cleanup => Fin.Fail<T>(primary + cleanup))));
    }
}
