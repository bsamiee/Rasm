using Rhino;

namespace Rasm.TestKit.Scenarios;

// --- [SERVICES] -----------------------------------------------------------------------------

// DocumentScope admits the live document once, clears object state on open/dispose, and registers
// leak drainage with the context. ViewportRealized feeds the runner's failure-capture trigger.
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

// Capture is the green-path SDK seam; the runner binds Hook only for the run bracket and cargo
// lifetime. Unbound calls fail typed instead of throwing or writing stray files.
public static class Capture {
    internal static Func<string, Fin<string>>? Hook { get; set; }

    public static Fin<string> Snapshot(string label) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: label);
        return Hook is { } hook
            ? hook(label)
            : Fin.Fail<string>(error: Error.New(message: $"Capture.Snapshot('{label}'): no capture surface bound — outside a bridge scenario run"));
    }
}
