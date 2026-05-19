using System.IO;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.UI;
using Rhino;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record CanvasUiRequest<T> {
    private readonly Func<CanvasUi.Scope, Fin<T>> run;

    internal CanvasUiRequest(Func<CanvasUi.Scope, Fin<T>> run, bool openEditor) {
        this.run = run;
        OpenEditor = openEditor;
    }

    internal bool OpenEditor { get; }

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
    Option<CanvasWireSnapshot> WireUnderPick,
    Option<Guid> ObjectUnderPick,
    Option<Guid> InletUnderPick,
    Option<Guid> OutletUnderPick) {
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
            WireUnderPick: Optional(new CanvasWireSnapshot(Source: result.WireUnderPick.Source, Target: result.WireUnderPick.Target))
                .Filter(static wire => wire.Source != Guid.Empty || wire.Target != Guid.Empty),
            ObjectUnderPick: Optional(result.ObjectUnderPick).Filter(static id => id != Guid.Empty),
            InletUnderPick: Optional(result.InletUnderPick).Filter(static id => id != Guid.Empty),
            OutletUnderPick: Optional(result.OutletUnderPick).Filter(static id => id != Guid.Empty));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasBitmapSnapshot(int Width, int Height, ReadOnlyMemory<byte> Png);

// --- [SERVICES] -------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record CanvasUi {
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Scope(Option<GhEditor> Editor, Option<GhCanvas> Canvas) {
        public static Scope Active(bool openEditor) {
            GhEditor? editor = (GhEditor.Instance, openEditor) switch {
                (GhEditor current, _) => current,
                (_, true) => GhEditor.ShowEditor(createVisible: true),
                _ => null,
            };
            return new(Editor: Optional(editor), Canvas: Optional(editor?.Canvas));
        }
    }

    public Fin<T> Use<T>(CanvasUiRequest<T> request) =>
        from valid in Optional(request).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
        from result in OnUiThread(run: () => valid.Run(scope: Scope.Active(openEditor: valid.OpenEditor)))
        select result;

    internal static Fin<T> OnUiThread<T>(Func<Fin<T>> run) =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: nameof(OnUiThread)).InvalidInput())
            .Bind(valid => RhinoApp.IsOnMainThread switch {
                true => Protect(valid: valid),
                false => Try.lift<Fin<T>>(f: () => {
                    // BOUNDARY ADAPTER -- Rhino owns the UI thread and InvokeAndWait exposes no return channel.
                    Fin<T> result = Fin.Fail<T>(error: Op.Of(name: nameof(OnUiThread)).InvalidResult());
                    RhinoApp.InvokeAndWait(action: () => result = Protect(valid: valid));
                    return result;
                })
                    .Run()
                    .MapFail(_ => Op.Of(name: nameof(OnUiThread)).InvalidResult())
                    .Bind(static result => result),
            });

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid) =>
        Optional(valid)
            .ToFin(Fail: Op.Of(name: nameof(Protect)).InvalidInput())
            .Bind(callback => Try.lift<Fin<T>>(f: callback)
                .Run()
                .MapFail(_ => Op.Of(name: nameof(Protect)).InvalidResult())
                .Bind(static result => result));
}

public static class CanvasUiRequest {
    public static CanvasUiRequest<CanvasSnapshot> Snapshot(bool openEditor = false) =>
        new(run: scope => Fin.Succ(value: new CanvasSnapshot(HasEditor: scope.Editor.IsSome, HasCanvas: scope.Canvas.IsSome)), openEditor: openEditor);

    public static CanvasUiRequest<Unit> Invalidate() =>
        new(run: scope => scope.Canvas
            .ToFin(Fail: Op.Of(name: nameof(Invalidate)).InvalidInput())
            .Map(canvas => {
                // BOUNDARY ADAPTER -- GH2 owns canvas invalidation; Rasm exposes only the completed request.
                canvas.Invalidate();
                return unit;
            }), openEditor: false);

    public static CanvasUiRequest<CanvasPickSnapshot> Pick(PointF point, CanvasPickPolicy policy) =>
        new(run: scope => Optional(point)
            .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y))
            .ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
            .Bind(validPoint => scope.Canvas.ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
            .Map(canvas => {
                // BOUNDARY ADAPTER -- GH2 SelectionResult is snapshotted; Document/native references stay inside the boundary.
                SelectionResult result = canvas.ResolvePick(point: validPoint, includeGrips: policy.IncludeGrips, includeForeground: policy.IncludeForeground, includeBackground: policy.IncludeBackground, includeWires: policy.IncludeWires, recursive: policy.Recursive);
                return CanvasPickSnapshot.Of(result: result);
            })), openEditor: false);

    public static CanvasUiRequest<Unit> Instantiate(bool mouseCentred, string? initialText = null) =>
        new(run: scope => scope.Canvas
            .ToFin(Fail: Op.Of(name: nameof(Instantiate)).InvalidInput())
            .Map(canvas => {
                // BOUNDARY ADAPTER -- GH2 owns instantiation popup placement; Rasm exposes only completion.
                canvas.ShowInstantiationPopup(mouseCentred: mouseCentred, initialText: initialText);
                return unit;
            }), openEditor: true);

    public static CanvasUiRequest<CanvasBitmapSnapshot> Bitmap(int width, int height, bool drawBackground, bool drawWires, bool drawMessages) =>
        new(run: scope => Optional((Width: width, Height: height))
            .Filter(static size => size.Width > 0 && size.Height > 0)
            .ToFin(Fail: Op.Of(name: nameof(Bitmap)).InvalidInput())
            .Bind(size => scope.Canvas.ToFin(Fail: Op.Of(name: nameof(Bitmap)).InvalidInput())
            .Bind(canvas => {
                // BOUNDARY ADAPTER -- Eto Bitmap is disposable; only encoded PNG bytes cross back into FP space.
                using Bitmap bitmap = canvas.DrawToBitmap(width: size.Width, height: size.Height, drawBackground: drawBackground, drawWires: drawWires, drawMessages: drawMessages);
                using MemoryStream stream = new();
                bitmap.Save(stream: stream, format: ImageFormat.Png);
                return Optional(bitmap)
                    .Map(valid => new CanvasBitmapSnapshot(Width: valid.Width, Height: valid.Height, Png: stream.ToArray()))
                    .ToFin(Fail: Op.Of(name: nameof(Bitmap)).InvalidResult());
            })), openEditor: false);
}
