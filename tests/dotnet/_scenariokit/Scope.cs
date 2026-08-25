using Rasm.Bridge.Contract;
using Rhino;

namespace Rasm.ScenarioKit;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Snapshot(string Path, int Width, int Height, bool OnFailure, ArtifactRef? Artifact = null);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DocumentScope : IDisposable {
    private readonly ScenarioContext ctx;
    private readonly int openedWithObjects;

    private DocumentScope(RhinoDoc doc, ScenarioContext ctx, int openedWithObjects) {
        Doc = doc;
        this.ctx = ctx;
        this.openedWithObjects = openedWithObjects;
    }

    public RhinoDoc Doc { get; }

    public bool ViewportRealized => Doc.Views.ActiveView is not null;

    internal bool IsLive { get; private set; } = true;

    public static Fin<DocumentScope> Open(ScenarioContext ctx) {
        ArgumentNullException.ThrowIfNull(argument: ctx);
        return Try.lift(f: () => {
            int before = ctx.Doc.Objects.Count;
            DocumentScope scope = new(doc: ctx.Doc, ctx: ctx, openedWithObjects: before);
            scope.Doc.Objects.Clear();
            ctx.Fact(key: FactKey.DocumentBefore.Render(argument: string.Empty), value: before);
            ctx.Register(scope: scope);
            return scope;
        }).Run();
    }

    public Fin<Unit> Done() =>
        IsLive
            ? Try.lift(f: () => {
                ctx.Fact(key: FactKey.DocumentOpened.Render(argument: string.Empty), value: openedWithObjects);
                ctx.Fact(key: FactKey.DocumentAfter.Render(argument: string.Empty), value: Doc.Objects.Count);
                Dispose();
                return unit;
            }).Run()
            : Fin.Succ(value: unit);

    public void Dispose() {
        if (IsLive) {
            IsLive = false;
            Doc.Objects.Clear();
            Doc.Modified = false;
        }
    }
}

public static class Capture {
    internal static Func<string, Fin<Snapshot>>? Hook { get; set; }

    public static Fin<Snapshot> Snapshot(string label) {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument: label);
        return Hook is { } hook
            ? hook(label)
            : Fin.Fail<Snapshot>(error: Error.New(message: $"Capture.Snapshot('{label}'): no capture surface bound — outside a bridge scenario run"));
    }
}
