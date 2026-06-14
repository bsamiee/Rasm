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

    internal ScenarioContext(RhinoDoc doc, Action<string, object?> sink) {
        Doc = doc;
        this.sink = sink;
    }

    public RhinoDoc Doc { get; }

    internal int FactCount { get; private set; }

    internal RhinoView? RealizedView =>
        scopes.Exists(match: static scope => scope.IsLive && scope.ViewportRealized) ? Doc.Views.ActiveView : null;

    public Fin<Unit> Require(string label, bool observed) {
        Fact(key: label, value: observed);
        return observed ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Error.New(message: $"require '{label}': observed false"));
    }

    public Fin<T> Expect<T>(string label, Fin<T> projection) {
        ArgumentNullException.ThrowIfNull(argument: projection);
        Fact(key: label, value: projection switch {
            Fin<T>.Succ(T value) => value,
            Fin<T>.Fail(Error error) => $"FAIL: {error.Message}",
            _ => "FAIL: unresolved projection",
        });
        return projection;
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
