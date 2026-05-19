using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.UI;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record CanvasUiRequest<T> {
    private readonly Func<CanvasUi.Scope, Fin<T>> run;

    internal CanvasUiRequest(Func<CanvasUi.Scope, Fin<T>> run) => this.run = run;

    internal Fin<T> Run(CanvasUi.Scope scope) => run(arg: scope);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasSnapshot(bool HasEditor, bool HasCanvas);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasPickPolicy(
    bool IncludeGrips,
    bool IncludeForeground,
    bool IncludeBackground,
    bool IncludeWires,
    bool Recursive) {
    public static CanvasPickPolicy All => new(IncludeGrips: true, IncludeForeground: true, IncludeBackground: true, IncludeWires: true, Recursive: true);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasWireSnapshot(Guid Source, Guid Target);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasPickSnapshot(
    Pick Kind,
    Option<PointF> Point,
    int SelectedCount,
    int SelectedWireCount,
    int SelectedObjectCount,
    int DeselectedCount,
    int DeselectedWireCount,
    int DeselectedObjectCount,
    CanvasWireSnapshot WireUnderPick,
    Guid ObjectUnderPick,
    Guid InletUnderPick,
    Guid OutletUnderPick) {
    internal static CanvasPickSnapshot Of(SelectionResult result) =>
        new(
            Kind: result.Kind,
            Point: result.Point switch {
                PointF point => Optional(point),
                _ => None,
            },
            SelectedCount: result.SelectedCount,
            SelectedWireCount: result.SelectedWireCount,
            SelectedObjectCount: result.SelectedObjectCount,
            DeselectedCount: result.DeselectedCount,
            DeselectedWireCount: result.DeselectedWireCount,
            DeselectedObjectCount: result.DeselectedObjectCount,
            WireUnderPick: new CanvasWireSnapshot(Source: result.WireUnderPick.Source, Target: result.WireUnderPick.Target),
            ObjectUnderPick: result.ObjectUnderPick,
            InletUnderPick: result.InletUnderPick,
            OutletUnderPick: result.OutletUnderPick);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasBitmapSnapshot(int Width, int Height);

// --- [SERVICES] -------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record CanvasUi {
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Scope(Option<GhEditor> Editor, Option<GhCanvas> Canvas) {
        public static Scope Active() {
            GhEditor? editor = GhEditor.Instance ?? GhEditor.ShowEditor(createVisible: true);
            return new(Editor: Optional(editor), Canvas: Optional(editor?.Canvas));
        }
    }

    public Fin<T> Use<T>(CanvasUiRequest<T> request) =>
        from valid in Optional(request).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
        from result in Protect(valid: () => valid.Run(scope: Scope.Active()))
        select result;

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid) =>
        Optional(valid)
            .ToFin(Fail: Op.Of(name: nameof(Protect)).InvalidInput())
            .Bind(callback => Try.lift<Fin<T>>(f: callback)
                .Run()
                .MapFail(_ => Op.Of(name: nameof(Protect)).InvalidResult())
                .Bind(static result => result));
}

public static class CanvasUiRequest {
    public static CanvasUiRequest<CanvasSnapshot> Snapshot() =>
        new(run: scope => Fin.Succ(value: new CanvasSnapshot(HasEditor: scope.Editor.IsSome, HasCanvas: scope.Canvas.IsSome)));

    public static CanvasUiRequest<Unit> Invalidate() =>
        new(run: scope => scope.Canvas
            .ToFin(Fail: Op.Of(name: nameof(Invalidate)).InvalidInput())
            .Map(canvas => {
                // BOUNDARY ADAPTER -- GH2 owns canvas invalidation; Rasm exposes only the completed request.
                canvas.Invalidate();
                return unit;
            }));

    public static CanvasUiRequest<CanvasPickSnapshot> Pick(PointF point, CanvasPickPolicy policy) =>
        new(run: scope => scope.Canvas
            .ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
            .Map(canvas => {
                // BOUNDARY ADAPTER -- GH2 SelectionResult is snapshotted; Document/native references stay inside the boundary.
                SelectionResult result = canvas.ResolvePick(point: point, includeGrips: policy.IncludeGrips, includeForeground: policy.IncludeForeground, includeBackground: policy.IncludeBackground, includeWires: policy.IncludeWires, recursive: policy.Recursive);
                return CanvasPickSnapshot.Of(result: result);
            }));

    public static CanvasUiRequest<Unit> Search() =>
        new(run: scope => scope.Canvas
            .ToFin(Fail: Op.Of(name: nameof(Search)).InvalidInput())
            .Map(canvas => {
                // BOUNDARY ADAPTER -- GH2 owns search popup placement; Rasm exposes only completion.
                canvas.ShowSearchPopup();
                return unit;
            }));

    public static CanvasUiRequest<Unit> Instantiate(bool mouseCentred, string? initialText = null) =>
        new(run: scope => scope.Canvas
            .ToFin(Fail: Op.Of(name: nameof(Instantiate)).InvalidInput())
            .Map(canvas => {
                // BOUNDARY ADAPTER -- GH2 owns instantiation popup placement; Rasm exposes only completion.
                canvas.ShowInstantiationPopup(mouseCentred: mouseCentred, initialText: initialText);
                return unit;
            }));

    public static CanvasUiRequest<CanvasBitmapSnapshot> Bitmap(int width, int height, bool drawBackground, bool drawWires, bool drawMessages) =>
        new(run: scope => scope.Canvas
            .ToFin(Fail: Op.Of(name: nameof(Bitmap)).InvalidInput())
            .Bind(canvas => {
                // BOUNDARY ADAPTER -- Eto Bitmap is disposable; only dimensions cross back into FP space.
                using Bitmap bitmap = canvas.DrawToBitmap(width: width, height: height, drawBackground: drawBackground, drawWires: drawWires, drawMessages: drawMessages);
                return Optional(bitmap)
                    .Map(valid => new CanvasBitmapSnapshot(Width: valid.Width, Height: valid.Height))
                    .ToFin(Fail: Op.Of(name: nameof(Bitmap)).InvalidResult());
            }));
}
