using Rhino;

namespace Rasm.Bridge.Scenarios;

// --- [SERVICES] -----------------------------------------------------------------------------

// Ownership: the document resource capsule. Open admits the live document once and registers
// with the owning context; dispose restores a clean object table both directions (deterministic
// isolation on the bridge-dedicated host). ViewportRealized feeds the runner's
// auto-capture-on-failure trigger; an undisposed scope is drained by the runner as a named leak.
public sealed class DocumentScope : IDisposable {
    private DocumentScope(RhinoDoc doc) => Doc = doc;

    public RhinoDoc Doc { get; }

    public bool ViewportRealized => Doc.Views.ActiveView is not null;

    internal bool IsLive { get; private set; } = true;

    public static Fin<DocumentScope> Open(ScenarioContext ctx) {
        ArgumentNullException.ThrowIfNull(argument: ctx);
        DocumentScope scope = new(doc: ctx.Doc);
        scope.Doc.Objects.Clear();
        ctx.Register(scope: scope);
        return Fin.Succ(value: scope);
    }

    public void Dispose() {
        if (IsLive) {
            IsLive = false;
            Doc.Objects.Clear();
            Doc.Modified = false;
        }
    }
}

// Ownership: explicit green-path capture. The runner binds the capture surface per scenario run
// and clears it on the run bracket and at cargo disposal (the hook is the SDK's one mutable
// static; it lives and dies with the cargo ALC). Outside a run the call degrades to a typed
// failure — never a throw, never a stray file.
public static class Capture {
    internal static Func<string, Fin<string>>? Hook { get; set; }

    public static Fin<string> Snapshot(string label) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: label);
        return Hook is { } hook
            ? hook(label)
            : Fin.Fail<string>(error: Error.New(message: $"Capture.Snapshot('{label}'): no capture surface bound — outside a bridge scenario run"));
    }
}
