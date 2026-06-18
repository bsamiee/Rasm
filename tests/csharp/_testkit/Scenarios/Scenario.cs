using System.Text.Json;
using Rasm.Bridge.Contract;
using Rhino;
using Rhino.Display;

namespace Rasm.TestKit.Scenarios;

// --- [TYPES] --------------------------------------------------------------------------------

// Scenario attributes are the in-host manifest: discovery reads theme, requirements, budget,
// and static Fin<Unit>(ScenarioContext) entrypoints. There is no pre-host registry lane.
[AttributeUsage(AttributeTargets.Method)]
public sealed class RhinoScenarioAttribute(string theme) : Attribute {
    public string Theme { get; } = theme;
    public string[] Requires { get; init; } = [];
    public int BudgetMs { get; init; }
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

    public Fin<Unit> Certify<T>(EvidenceName key, T actual, ReferenceTolerance tolerance) {
        if (string.IsNullOrWhiteSpace(value: key.Key)) {
            throw new ArgumentException(message: "evidence key cannot be blank", paramName: nameof(key));
        }
        ReferenceCount++;
        Fact(key: $"reference.{key.Key}", value: new {
            name = key.Key,
            actual,
            tolerance,
            admission = ReferenceAdmission.Reviewed.Key,
        });
        return Fin.Succ(value: unit);
    }

    public Fin<Unit> Reference(EvidenceName key, JsonElement actual, ReferenceTolerance tolerance) {
        if (string.IsNullOrWhiteSpace(value: key.Key)) {
            throw new ArgumentException(message: "evidence key cannot be blank", paramName: nameof(key));
        }
        ReferenceCount++;
        Fact(key: $"reference.{key.Key}", value: new {
            name = key.Key,
            actual,
            tolerance,
            admission = ReferenceAdmission.Reviewed.Key,
        });
        return Fin.Succ(value: unit);
    }

    public void ObjectManifest<T>(EvidenceName key, T value) =>
        Fact(key: $"manifest.object.{key.Key}", value: value);

    public void GeometryManifest<T>(EvidenceName key, T value) =>
        Fact(key: $"manifest.geometry.{key.Key}", value: value);

    public void ViewportManifest<T>(EvidenceName key, T value) =>
        Fact(key: $"manifest.viewport.{key.Key}", value: value);

    public void Gh2CanvasManifest<T>(EvidenceName key, T value) =>
        Fact(key: $"manifest.gh2.{key.Key}", value: value);

    public void Artifact(string path, EvidenceRole role) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: path);
        ArgumentNullException.ThrowIfNull(argument: role);
        Fact(key: $"artifact.{role.Key}", value: path);
    }

    public string Scratch(string stem) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: stem);
        string path = Path.Combine(path1: Path.GetTempPath(), path2: stem);
        Fact(key: "scratch.path", value: path);
        return path;
    }

    public string Stamp(string stem) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: stem);
        string stamp = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{stem}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
        Fact(key: "stamp", value: stamp);
        return stamp;
    }

    public Fin<Unit> Case(string name, Func<Fin<Unit>> action) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: name);
        ArgumentNullException.ThrowIfNull(argument: action);
        Fact(key: $"case.{name}.start", value: true);
        Fin<Unit> result = action();
        Fact(key: $"case.{name}.status", value: result is Fin<Unit>.Succ ? "ok" : result is Fin<Unit>.Fail(Error error) ? $"failed:{error.Message}" : "unresolved");
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
