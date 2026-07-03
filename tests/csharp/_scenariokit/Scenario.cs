using Rasm.Bridge.Contract;
using Rhino;
using Rhino.Display;

namespace Rasm.ScenarioKit;

// --- [TYPES] --------------------------------------------------------------------------------

// Scenario attributes are the in-host manifest: discovery reads theme, requirements, budget,
// and static Fin<Unit>(ScenarioContext) entrypoints BY FULL NAME over staged assemblies, so the
// attribute's full name and member signatures are frozen wire law. There is no pre-host registry.
[AttributeUsage(AttributeTargets.Method)]
public sealed class RhinoScenarioAttribute(string theme) : Attribute {
    public string Theme { get; } = theme;
    public string[] Requires { get; init; } = [];
    public int BudgetMs { get; init; }
}

// One closed fact-key grammar: every ScenarioContext/DocumentScope wire key renders through a
// row. Prefixes come from the Contract EvidenceRole vocabulary the session fold classifies by,
// so render and parse share one string owner — rendered strings never change.
[SmartEnum]
internal sealed partial class FactKey {
    public static readonly FactKey Reference = new(static argument => EvidenceRole.Reference.FactPrefix + argument);
    public static readonly FactKey ObjectManifest = new(static argument => EvidenceRole.ObjectManifest.FactPrefix + argument);
    public static readonly FactKey GeometryManifest = new(static argument => EvidenceRole.GeometryManifest.FactPrefix + argument);
    public static readonly FactKey ViewportManifest = new(static argument => EvidenceRole.ViewportManifest.FactPrefix + argument);
    public static readonly FactKey Gh2Manifest = new(static argument => EvidenceRole.Gh2CanvasManifest.FactPrefix + argument);
    public static readonly FactKey Artifact = new(static argument => EvidenceRole.Artifact.FactPrefix + argument);
    public static readonly FactKey CaseStart = new(static argument => $"{EvidenceRole.Assertion.FactPrefix}{argument}.start");
    public static readonly FactKey CaseStatus = new(static argument => $"{EvidenceRole.Assertion.FactPrefix}{argument}.status");
    public static readonly FactKey ScratchPath = new(static _ => "scratch.path");
    public static readonly FactKey Stamp = new(static _ => "stamp");
    public static readonly FactKey DocumentBefore = new(static _ => "document.before.objects");
    public static readonly FactKey DocumentOpened = new(static _ => "document.opened.objects");
    public static readonly FactKey DocumentAfter = new(static _ => "document.after.objects");

    [UseDelegateFromConstructor]
    public partial string Render(string argument);
}

// --- [SERVICES] -----------------------------------------------------------------------------

// ScenarioContext is the SDK boundary: assert+fact calls write runner-owned facts while the SDK
// stays wire-blind, and scope registration lets the runner drain leaks before unload.
public sealed class ScenarioContext {
    private readonly List<DocumentScope> scopes = [];
    private readonly Action<string, object?> sink;

    internal ScenarioContext(RhinoDoc doc, Action<string, object?> sink, string scenario = "") {
        Doc = doc;
        this.sink = sink;
        Scenario = scenario;
    }

    public RhinoDoc Doc { get; }
    public string Scenario { get; }

    internal int FactCount { get; private set; }
    internal int AssertionCount { get; private set; }
    internal int ReferenceCount { get; private set; }

    internal RhinoView? RealizedView =>
        scopes.Exists(match: static scope => scope.IsLive && scope.ViewportRealized) ? Doc.Views.ActiveView : null;

    public Fin<Unit> Require(string label, bool observed) {
        Fact(key: label, value: observed);
        AssertionCount++;
        return observed ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Error.New(message: $"require '{label}': observed false"));
    }

    public Fin<T> Expect<T>(string label, Fin<T> projection) {
        ArgumentNullException.ThrowIfNull(argument: projection);
        Fact(key: label, value: projection switch {
            Fin<T>.Succ(T value) => value,
            Fin<T>.Fail(Error error) => $"FAIL: {error.Message}",
            _ => "FAIL: unresolved projection",
        });
        AssertionCount++;
        return projection;
    }

    public void Note<T>(EvidenceName key, T value) => Fact(key: key.Key, value: value);

    // One reference verb owns every actual (typed value or raw JsonElement via T). The supervisor
    // fold consumes exactly {name, actual, tolerance}; admission is decided supervisor-side by
    // evidence mode and corpus state, never asserted by the SDK.
    public Fin<Unit> Certify<T>(EvidenceName key, T actual, ReferenceTolerance tolerance) {
        if (string.IsNullOrWhiteSpace(value: key.Key)) {
            throw new ArgumentException(message: "evidence key cannot be blank", paramName: nameof(key));
        }
        ReferenceCount++;
        Fact(key: FactKey.Reference.Render(argument: key.Key), value: new {
            name = key.Key,
            actual,
            tolerance,
        });
        return Fin.Succ(value: unit);
    }

    public void ObjectManifest<T>(EvidenceName key, T value) =>
        Fact(key: FactKey.ObjectManifest.Render(argument: key.Key), value: value);

    public void GeometryManifest<T>(EvidenceName key, T value) =>
        Fact(key: FactKey.GeometryManifest.Render(argument: key.Key), value: value);

    public void ViewportManifest<T>(EvidenceName key, T value) =>
        Fact(key: FactKey.ViewportManifest.Render(argument: key.Key), value: value);

    public void Gh2CanvasManifest<T>(EvidenceName key, T value) =>
        Fact(key: FactKey.Gh2Manifest.Render(argument: key.Key), value: value);

    public void Artifact(string path, EvidenceRole role) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: path);
        ArgumentNullException.ThrowIfNull(argument: role);
        Fact(key: FactKey.Artifact.Render(argument: role.Key), value: path);
    }

    public string Scratch(string stem) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: stem);
        string path = Path.Combine(path1: Path.GetTempPath(), path2: stem);
        Fact(key: FactKey.ScratchPath.Render(argument: stem), value: path);
        return path;
    }

    public string Stamp(string stem) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: stem);
        string stamp = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{stem}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
        Fact(key: FactKey.Stamp.Render(argument: stem), value: stamp);
        return stamp;
    }

    public Fin<Unit> Case(string name, Func<Fin<Unit>> action) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: name);
        ArgumentNullException.ThrowIfNull(argument: action);
        Fact(key: FactKey.CaseStart.Render(argument: name), value: true);
        Fin<Unit> result = action();
        Fact(key: FactKey.CaseStatus.Render(argument: name), value: result is Fin<Unit>.Succ ? "ok" : result is Fin<Unit>.Fail(Error error) ? $"failed:{error.Message}" : "unresolved");
        return result;
    }

    public void Fact(string key, object? value) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: key);
        FactCount++;
        sink(key, value);
    }

    internal int DrainScopes() {
        // Leaked scopes are reported before forced disposal restores the document for unload.
        int leaked = 0;
        foreach (DocumentScope scope in scopes) {
            if (scope.IsLive) {
                leaked++;
                scope.Dispose();
            }
        }
        scopes.Clear();
        return leaked;
    }

    internal void Register(DocumentScope scope) => scopes.Add(item: scope);
}
