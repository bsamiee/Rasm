using System.Collections.Frozen;
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

// The closed grammar for composite and constant wire keys. Prefix lanes (reference., manifest.*,
// artifact.) render straight off the Contract's EvidenceRole.FactPrefix, so render and parse
// share the Contract as their one string owner — rendered strings never change.
[SmartEnum]
internal sealed partial class FactKey {
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
    // The manifest admission table derives from the Contract: every role whose FactPrefix sits in
    // the manifest fact-key family is a lane, so a new Contract manifest lane needs no SDK edit.
    private static readonly FrozenSet<EvidenceRole> ManifestLanes = EvidenceRole.Items
        .Where(predicate: static role => role.FactPrefix.StartsWith(value: "manifest.", comparisonType: StringComparison.Ordinal))
        .ToFrozenSet();

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
        Fact(key: label, value: projection.Match<object?>(Succ: static value => value, Fail: static error => $"FAIL: {error.Message}"));
        AssertionCount++;
        return projection;
    }

    public void Note<T>(EvidenceName key, T value) => Fact(key: key.Key, value: value);

    // One reference verb owns every actual (typed value or raw JsonElement via T). The supervisor
    // fold consumes exactly {name, actual, tolerance}; admission is decided supervisor-side by
    // evidence mode and corpus state, never asserted by the SDK.
    public Fin<Unit> Certify<T>(EvidenceName key, T actual, ReferenceTolerance tolerance) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: key.Key, paramName: nameof(key));
        ReferenceCount++;
        Fact(key: EvidenceRole.Reference.FactPrefix + key.Key, value: new {
            name = key.Key,
            actual,
            tolerance,
        });
        return Fin.Succ(value: unit);
    }

    // One manifest verb owns all four manifest lanes: the role argument is the modality, and the
    // admission table gates it — an unknown lane is an input guard, never a mis-prefixed fact.
    public void Manifest<T>(EvidenceRole role, EvidenceName key, T value) {
        ArgumentNullException.ThrowIfNull(argument: role);
        _ = ManifestLanes.Contains(item: role)
            ? role : throw new ArgumentOutOfRangeException(paramName: nameof(role), actualValue: role.Key, message: "role does not own a manifest lane");
        Fact(key: role.FactPrefix + key.Key, value: value);
    }

    public void Artifact(string path, EvidenceRole role) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: path);
        ArgumentNullException.ThrowIfNull(argument: role);
        Fact(key: EvidenceRole.Artifact.FactPrefix + role.Key, value: path);
    }

    // A rooted or upward-traversing stem would silently escape the scratch root; normalization gates it.
    public string Scratch(string stem) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: stem);
        string root = Path.GetTempPath();
        string path = Path.GetFullPath(path: Path.Combine(path1: root, path2: stem));
        _ = path.StartsWith(value: root, comparisonType: StringComparison.Ordinal)
            ? path : throw new ArgumentOutOfRangeException(paramName: nameof(stem), actualValue: stem, message: "stem escapes the scratch root");
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
        // Host boundary: Try converts a throwing sub-case to typed failure so the status fact
        // always lands and sibling cases still run; the Error keeps the exception for the fold.
        Fin<Unit> result = Try.lift(f: action).Run();
        Fact(key: FactKey.CaseStatus.Render(argument: name), value: result.Match(Succ: static _ => "ok", Fail: static error => $"failed:{error.Message}"));
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
