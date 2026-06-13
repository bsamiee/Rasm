using Rhino;
using Rhino.Display;

namespace Rasm.Bridge.Scenarios;

// --- [TYPES] --------------------------------------------------------------------------------

// Ownership: the scenario declaration surface. Theme, capability requirements, and budget are
// read in-host by post-load discovery — the attribute IS the scenario manifest; no parallel
// registry, no pre-host enumeration lane exists. Entrypoint shape: static Fin<Unit> over one
// ScenarioContext parameter.
[AttributeUsage(AttributeTargets.Method)]
public sealed class RhinoScenarioAttribute(string theme) : Attribute {
    public string Theme { get; } = theme;
    public string[] Requires { get; init; } = [];
    public int BudgetMs { get; init; }
}

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: the entrypoint's single parameter — fused assert+fact (one call emits the evidence
// AND decides the rail), the document handle, and the scope registry the runner drains. Facts
// ride a runner-owned sink so the SDK stays wire-blind: the runner counts, stamps, spools, and
// relays; the SDK never names a wire type.
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
        // D-2 precondition: an undisposed scope is a named leak — the runner facts it, then this
        // forced dispose restores the document before the ALC unload begins.
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
