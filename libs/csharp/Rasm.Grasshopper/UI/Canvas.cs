using System.IO;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.UI.Flex;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
[Flags]
public enum CanvasBitmapLayers {
    None = 0,
    Background = 1,
    Wires = 2,
    Messages = 4,
    All = Background | Wires | Messages,
}

[Flags]
public enum CanvasWindowScope {
    None = 0,
    Objects = 1,
    Wires = 2,
    Groups = 4,
    ObjectsAndWires = Objects | Wires,
    All = Objects | Wires | Groups,
}

public enum InvalidateMode { Immediate, Scheduled }

[Union]
public partial record CanvasFitTarget {
    private CanvasFitTarget() { }
    public sealed record ContentCase : CanvasFitTarget;
    public sealed record SelectionCase : CanvasFitTarget;
    public sealed record ViewportCase : CanvasFitTarget;
    public static readonly CanvasFitTarget Content = new ContentCase();
    public static readonly CanvasFitTarget Selection = new SelectionCase();
    public static readonly CanvasFitTarget Viewport = new ViewportCase();
}

[Union]
public partial record CanvasOp {
    private CanvasOp() { }
    public sealed record SnapshotCase(bool OpenEditor) : CanvasOp;
    public sealed record PickCase(PointF Point, CoordinateSystem Source) : CanvasOp;
    public sealed record MapCase(PointF Point, CoordinateSystem From, CoordinateSystem To) : CanvasOp;
    public sealed record InvalidateCase(InvalidateMode Mode) : CanvasOp;
    public sealed record InstantiateCase(Option<string> SearchText, bool MouseCentred) : CanvasOp;
    public sealed record SearchPopupCase : CanvasOp;
    public sealed record DetailCase : CanvasOp;
    public sealed record UndoHistoryCase(bool Visible) : CanvasOp;
    public sealed record RenderCase(int Width, int Height, CanvasBitmapLayers Layers) : CanvasOp;
    public sealed record PickMapCase : CanvasOp;
    public sealed record NavigateCase(RectangleF Bounds, CanvasNavigationPolicy Policy) : CanvasOp;
    public sealed record FrameCase(Option<Seq<Guid>> Ids, CanvasFramePolicy Policy) : CanvasOp;
    public sealed record FitCase(CanvasFitTarget Target, CanvasNavigationPolicy Policy) : CanvasOp;
    public sealed record ProjectionCase(PointF Centre, float Zoom) : CanvasOp;
    public sealed record InteractionCase(CanvasInteractionPolicy Policy) : CanvasOp;
    public sealed record WindowSelectCase(WindowSelection Window, Grasshopper2.Extensions.SelectionMode Mode, CanvasWindowScope Scope) : CanvasOp;

    public static CanvasOp Snapshot(bool openEditor = false) => new SnapshotCase(OpenEditor: openEditor);
    public static CanvasOp Pick(PointF point, CoordinateSystem source = CoordinateSystem.Content) => new PickCase(Point: point, Source: source);
    public static CanvasOp Map(PointF point, CoordinateSystem from, CoordinateSystem to) => new MapCase(Point: point, From: from, To: to);
    public static CanvasOp Invalidate(InvalidateMode mode = InvalidateMode.Immediate) => new InvalidateCase(Mode: mode);
    public static CanvasOp Instantiate(string? searchText = null, bool mouseCentred = true) => new InstantiateCase(SearchText: Optional(searchText), MouseCentred: mouseCentred);
    public static readonly CanvasOp SearchPopup = new SearchPopupCase();
    public static readonly CanvasOp Detail = new DetailCase();
    public static CanvasOp UndoHistory(bool visible) => new UndoHistoryCase(Visible: visible);
    public static CanvasOp Render(int width, int height, CanvasBitmapLayers layers = CanvasBitmapLayers.All) => new RenderCase(Width: width, Height: height, Layers: layers);
    public static readonly CanvasOp PickMap = new PickMapCase();
    public static CanvasOp Navigate(RectangleF frame, CanvasNavigationPolicy policy = default) => new NavigateCase(Bounds: frame, Policy: policy);
    public static CanvasOp Frame(Seq<Guid>? ids = null, CanvasFramePolicy policy = default) => new FrameCase(Ids: Optional(ids), Policy: policy);
    public static CanvasOp Fit(CanvasFitTarget target, CanvasNavigationPolicy policy = default) => new FitCase(Target: target, Policy: policy);
    public static CanvasOp Projection(PointF centre, float zoom) => new ProjectionCase(Centre: centre, Zoom: zoom);
    public static CanvasOp Interaction(CanvasInteractionPolicy policy) => new InteractionCase(Policy: policy);
    public static CanvasOp WindowSelect(WindowSelection window, Grasshopper2.Extensions.SelectionMode mode, CanvasWindowScope scope = CanvasWindowScope.ObjectsAndWires) => new WindowSelectCase(Window: window, Mode: mode, Scope: scope);
}

[Union]
public partial record CanvasResult {
    private CanvasResult() { }
    public sealed record SnapshotResult(CanvasSnapshot Snapshot) : CanvasResult;
    public sealed record PickResult(CanvasPickSnapshot Pick) : CanvasResult;
    public sealed record MapResult(CanvasMappedPoint Mapped) : CanvasResult;
    public sealed record BitmapResult(CanvasBitmap Bitmap) : CanvasResult;
    public sealed record InteractionResult(CanvasInteractionPolicy Effective) : CanvasResult;
    public sealed record DetailResult(CanvasDetailSnapshot Detail) : CanvasResult;
    public sealed record UnitResult : CanvasResult;
    public static readonly CanvasResult Unit = new UnitResult();
}

// --- [MODELS] ----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasNavigationPolicy(float MinimumZoom = 0.05f, float MaximumZoom = 2f, TimeSpan Duration = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasFramePolicy(float Padding = 48f, CanvasNavigationPolicy Navigation = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasInteractionPolicy(
    bool AllowPan = true, bool AllowZoom = true, bool ShowTilesWhenEmpty = true,
    bool WindowSelectObjects = true, bool WindowSelectWires = true, bool WindowSelectGroups = true,
    Option<bool> ViewportDragging = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasActionSnapshot(
    bool AllowDrag,
    bool AllowWireSelect,
    bool AllowObjectSelect,
    bool AllowMakeWire,
    bool AllowDeleteWire,
    bool AllowModifyWire,
    bool HasMakeWireFilter,
    bool HasDeleteWireFilter,
    bool AllowMakeObject,
    bool AllowDeleteObject,
    bool AllowObjectResponse,
    bool AllowDropFile,
    bool AllowWireMenu,
    bool AllowObjectMenu,
    bool AllowCanvasMenu);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasDetailSnapshot(
    bool ShowUndoHistory,
    float ZuiVariableParameterThreshold,
    float ZuiVariableParameterState,
    float ZuiWireDetailingThreshold,
    float ZuiWireDetailingState,
    CanvasActionSnapshot Actions);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasBitmap(int Width, int Height, ReadOnlyMemory<byte> Png);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasMappedPoint(PointF Source, PointF Target, CoordinateSystem From, CoordinateSystem To);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasSnapshot(
    bool HasEditor, bool HasCanvas, bool HasDocument,
    RectangleF VisibleFrame, RectangleF ContentBounds,
    PointF ProjectionCentre, float ProjectionZoom,
    bool WindowSelectObjects, bool WindowSelectWires, bool WindowSelectGroups);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasPickSnapshot(
    Pick Kind, Option<PointF> Point,
    int SelectedCount, int DeselectedCount,
    Option<WireEnds> WireUnderPick,
    Option<Guid> ObjectUnderPick,
    Option<Guid> InletUnderPick,
    Option<Guid> OutletUnderPick);

public abstract record CanvasRequest<T> : GhUiRequest<T> {
    public sealed record Use(CanvasOp Op) : CanvasRequest<CanvasResult> {
        internal override GrasshopperUiPolicy Policy => CanvasPolicyOf(op: Op);
        internal override Fin<CanvasResult> Apply(GrasshopperUi.Scope scope) => UiRail.CanvasDispatch(scope: scope, op: Op);
    }

    internal static GrasshopperUiPolicy CanvasPolicyOf(CanvasOp op) =>
        GrasshopperUiPolicy.Canvas(openEditor: UiRail.CanvasNeedsEditor(op: op), repaint: UiRail.CanvasRepaintFor(op: op));
}

// --- [SERVICES] --------------------------------------------------------------------------
internal static partial class UiRail {
    internal static GrasshopperUiIntent<CanvasResult> Canvas(CanvasOp op) {
        ArgumentNullException.ThrowIfNull(argument: op);
        return GhUi.Apply(new CanvasRequest<CanvasResult>.Use(Op: op));
    }

    // --- [OPERATIONS] ----------------------------------------------------------------------
    internal static Fin<CanvasResult> CanvasDispatch(GrasshopperUi.Scope scope, CanvasOp op) => op switch {
        CanvasOp.SnapshotCase s =>
            scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.SnapshotResult(Snapshot: scope.Document
                .Bind(d => scope.Objects.Map(o => SnapshotOf(canvas: canvas, document: d, objects: o)))
                .IfNone(EmptySnapshotOf(canvas: canvas)))),
        CanvasOp.PickCase p =>
            Optional(p.Point).Filter(static pt => float.IsFinite(pt.X) && float.IsFinite(pt.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Pick)), detail: "non-finite point"))
                .Bind(valid => scope.NeedCanvas().Map(canvas => {
                    PointF content = p.Source == CoordinateSystem.Content ? valid : canvas.Map(point: valid, from: p.Source, to: CoordinateSystem.Content);
                    SelectionResult result = canvas.ResolvePick(point: content, includeGrips: true, includeForeground: true, includeBackground: true, includeWires: true, recursive: true);
                    return (CanvasResult)new CanvasResult.PickResult(Pick: PickSnapshotOf(result: result));
                })),
        CanvasOp.MapCase m =>
            Optional(m.Point).Filter(static pt => float.IsFinite(pt.X) && float.IsFinite(pt.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Map)), detail: "non-finite point"))
                .Bind(valid => scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.MapResult(
                    Mapped: new CanvasMappedPoint(Source: valid, Target: canvas.Map(point: valid, from: m.From, to: m.To), From: m.From, To: m.To)))),
        CanvasOp.InvalidateCase i =>
            scope.NeedCanvas().Map(canvas => {
                System.Action invalidate = i.Mode switch {
                    InvalidateMode.Immediate => canvas.Invalidate,
                    InvalidateMode.Scheduled => canvas.ScheduleRedraw,
                    _ => new System.Action(static () => { }),
                };
                invalidate();
                return CanvasResult.Unit;
            }),
        CanvasOp.InstantiateCase ins =>
            scope.NeedCanvas().Map(canvas => {
                canvas.ShowInstantiationPopup(mouseCentred: ins.MouseCentred, initialText: ins.SearchText.IfNone(string.Empty));
                return CanvasResult.Unit;
            }),
        CanvasOp.SearchPopupCase =>
            Fin.Fail<CanvasResult>(error: UiFault.MutationRejected(
                op: Op.Of(name: nameof(CanvasOp.SearchPopup)),
                detail: "Grasshopper2 Canvas.ShowSearchPopup is an empty WIP stub in RhinoWIP 9.0.26132.12306")),
        CanvasOp.DetailCase =>
            scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.DetailResult(
                Detail: new CanvasDetailSnapshot(
                    ShowUndoHistory: canvas.ShowUndoHistory,
                    ZuiVariableParameterThreshold: canvas.ZuiVariableParameterThreshold,
                    ZuiVariableParameterState: canvas.ZuiVariableParameterState,
                    ZuiWireDetailingThreshold: canvas.ZuiWireDetailingThreshold,
                    ZuiWireDetailingState: canvas.ZuiWireDetailingState,
                    Actions: ActionSnapshotOf(canvas: canvas)))),
        CanvasOp.UndoHistoryCase h =>
            scope.NeedCanvas().Map(canvas => {
                canvas.ShowUndoHistory = h.Visible;
                return CanvasResult.Unit;
            }),
        CanvasOp.RenderCase r =>
            Optional((r.Width, r.Height))
                .Filter(static dim => dim.Width is > 0 and <= 16384 && dim.Height is > 0 and <= 16384)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Render)), detail: "dimensions out of [1, 16384]"))
                .Bind(dim => scope.NeedCanvas().Bind(canvas => EncodeBitmap(
                    bitmap: canvas.DrawToBitmap(
                        width: dim.Width, height: dim.Height,
                        drawBackground: (r.Layers & CanvasBitmapLayers.Background) == CanvasBitmapLayers.Background,
                        drawWires: (r.Layers & CanvasBitmapLayers.Wires) == CanvasBitmapLayers.Wires,
                        drawMessages: (r.Layers & CanvasBitmapLayers.Messages) == CanvasBitmapLayers.Messages),
                    width: dim.Width, height: dim.Height,
                    op: Op.Of(name: nameof(CanvasOp.Render))))),
        CanvasOp.PickMapCase =>
            scope.NeedCanvas().Bind(canvas => EncodeBitmap(bitmap: canvas.DrawPickMap(), width: 0, height: 0, op: Op.Of(name: nameof(CanvasOp.PickMap)))),
        CanvasOp.NavigateCase n =>
            Optional((Frame: n.Bounds, Policy: ResolveNavigation(n.Policy)))
                .Filter(static s => float.IsFinite(s.Frame.X) && float.IsFinite(s.Frame.Y) && float.IsFinite(s.Frame.Width) && float.IsFinite(s.Frame.Height) && s.Policy.MinimumZoom > 0 && s.Policy.MaximumZoom >= s.Policy.MinimumZoom)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Navigate)), detail: "non-finite frame or invalid zoom range"))
                .Bind(valid => scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Bind(doc => scope.NeedObjects().Map(objs =>
                    (CanvasResult)NavigateTo(canvas: canvas, frame: valid.Frame, policy: valid.Policy, document: doc, objects: objs))))),
        CanvasOp.FrameCase f =>
            scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Bind(doc => scope.NeedObjects().Bind(objs => {
                CanvasFramePolicy fp = ResolveFrame(f.Policy);
                IEnumerable<IDocumentObject> targets = f.Ids
                    .Map(list => (IEnumerable<IDocumentObject>)list.Choose(id => Optional(objs.Find(instanceId: id))).AsEnumerable())
                    .IfNone(objs.SelectedObjects);
                RectangleF aggregate = FrameOf(targets: targets);
                return float.IsFinite(aggregate.Width) && float.IsFinite(aggregate.Height) && aggregate.Width > 0 && aggregate.Height > 0
                    ? Fin.Succ<CanvasResult>(value: NavigateTo(canvas: canvas, frame: RectangleF.Inflate(rectangle: aggregate, width: fp.Padding, height: fp.Padding), policy: ResolveNavigation(fp.Navigation), document: doc, objects: objs))
                    : Fin.Fail<CanvasResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Frame)), detail: "no target objects"));
            }))),
        CanvasOp.FitCase ft =>
            scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Bind(doc => scope.NeedObjects().Map(objs => {
                RectangleF frame = ft.Target switch {
                    CanvasFitTarget.ContentCase => canvas.ContentBounds,
                    CanvasFitTarget.SelectionCase => FrameOf(targets: objs.SelectedObjects),
                    CanvasFitTarget.ViewportCase => canvas.VisibleFrame,
                    _ => canvas.ContentBounds,
                };
                return (CanvasResult)NavigateTo(canvas: canvas, frame: frame, policy: ResolveNavigation(ft.Policy), document: doc, objects: objs);
            }))),
        CanvasOp.ProjectionCase pr =>
            Optional((pr.Centre, pr.Zoom))
                .Filter(static s => float.IsFinite(s.Centre.X) && float.IsFinite(s.Centre.Y) && s.Zoom > 0 && float.IsFinite(s.Zoom))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Projection)), detail: "non-finite centre or zoom"))
                .Bind(valid => scope.NeedDocument().Bind(doc => scope.NeedObjects().Bind(objs => scope.NeedCanvas().Map(canvas => {
                    doc.Projection = (valid.Centre, valid.Zoom);
                    return (CanvasResult)new CanvasResult.SnapshotResult(Snapshot: SnapshotOf(canvas: canvas, document: doc, objects: objs));
                })))),
        CanvasOp.InteractionCase ic =>
            scope.NeedCanvas().Map(canvas => {
                canvas.AllowPan = ic.Policy.AllowPan;
                canvas.AllowZoom = ic.Policy.AllowZoom;
                canvas.ShowTilesWhenEmpty = ic.Policy.ShowTilesWhenEmpty;
                canvas.WindowSelectObjects = ic.Policy.WindowSelectObjects;
                canvas.WindowSelectWires = ic.Policy.WindowSelectWires;
                canvas.WindowSelectGroups = ic.Policy.WindowSelectGroups;
                _ = ic.Policy.ViewportDragging.Iter(value => canvas.ViewportDragging = value);
                return (CanvasResult)new CanvasResult.InteractionResult(Effective: ic.Policy);
            }),
        CanvasOp.WindowSelectCase ws =>
            scope.NeedObjects().Bind(objs => scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Map(doc => {
                _ = objs.WindowSelect(window: ws.Window, mode: ws.Mode,
                    considerForeground: (ws.Scope & CanvasWindowScope.Objects) == CanvasWindowScope.Objects,
                    considerBackground: (ws.Scope & CanvasWindowScope.Groups) == CanvasWindowScope.Groups,
                    considerWires: (ws.Scope & CanvasWindowScope.Wires) == CanvasWindowScope.Wires);
                return (CanvasResult)new CanvasResult.SnapshotResult(Snapshot: SnapshotOf(canvas: canvas, document: doc, objects: objs));
            }))),
        _ => Fin.Fail<CanvasResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasDispatch)), detail: "unknown CanvasOp")),
    };

    internal static bool CanvasNeedsEditor(CanvasOp op) => op switch {
        CanvasOp.SnapshotCase { OpenEditor: true } => true,
        CanvasOp.InstantiateCase or CanvasOp.NavigateCase or CanvasOp.FrameCase or CanvasOp.FitCase => true,
        _ => false,
    };

    internal static RepaintRequest CanvasRepaintFor(CanvasOp op) => op switch {
        CanvasOp.NavigateCase or CanvasOp.FrameCase or CanvasOp.FitCase or CanvasOp.ProjectionCase or CanvasOp.InteractionCase or CanvasOp.WindowSelectCase or CanvasOp.UndoHistoryCase => RepaintRequest.Canvas,
        CanvasOp.InvalidateCase { Mode: InvalidateMode.Scheduled } => RepaintRequest.Scheduled,
        _ => RepaintRequest.None,
    };

    private static CanvasNavigationPolicy ResolveNavigation(CanvasNavigationPolicy raw) =>
        raw.MinimumZoom <= 0
            ? new CanvasNavigationPolicy(MinimumZoom: 0.05f, MaximumZoom: 2f, Duration: raw.Duration == default ? TimeSpan.FromMilliseconds(value: 250) : raw.Duration)
            : raw with { Duration = raw.Duration == default ? TimeSpan.FromMilliseconds(value: 250) : raw.Duration };

    private static CanvasFramePolicy ResolveFrame(CanvasFramePolicy raw) =>
        new(Padding: raw.Padding <= 0 ? 48f : raw.Padding, Navigation: ResolveNavigation(raw.Navigation));

    private static CanvasResult.SnapshotResult NavigateTo(GhCanvas canvas, RectangleF frame, CanvasNavigationPolicy policy, GhDocument document, GhObjectList objects) {
        canvas.Navigate(frame: frame, zoomLimits: (policy.MinimumZoom, policy.MaximumZoom), duration: policy.Duration);
        return new CanvasResult.SnapshotResult(Snapshot: SnapshotOf(canvas: canvas, document: document, objects: objects));
    }

    internal static CanvasSnapshot SnapshotOf(GhCanvas canvas, GhDocument document, GhObjectList objects) =>
        new(HasEditor: true, HasCanvas: true, HasDocument: true,
            VisibleFrame: canvas.VisibleFrame,
            ContentBounds: canvas.ContentBounds,
            ProjectionCentre: document.Projection.centre,
            ProjectionZoom: document.Projection.zoom,
            WindowSelectObjects: canvas.WindowSelectObjects,
            WindowSelectWires: canvas.WindowSelectWires,
            WindowSelectGroups: canvas.WindowSelectGroups);

    private static CanvasSnapshot EmptySnapshotOf(GhCanvas canvas) =>
        new(HasEditor: true, HasCanvas: true, HasDocument: false,
            VisibleFrame: canvas.VisibleFrame,
            ContentBounds: canvas.ContentBounds,
            ProjectionCentre: default, ProjectionZoom: 0f,
            WindowSelectObjects: canvas.WindowSelectObjects,
            WindowSelectWires: canvas.WindowSelectWires,
            WindowSelectGroups: canvas.WindowSelectGroups);

    private static CanvasPickSnapshot PickSnapshotOf(SelectionResult result) =>
        new(Kind: result.Kind, Point: Optional(result.Point),
            SelectedCount: result.SelectedCount, DeselectedCount: result.DeselectedCount,
            WireUnderPick: Optional(result.WireUnderPick).Filter(static wire => wire.Source != Guid.Empty && wire.Target != Guid.Empty),
            ObjectUnderPick: Optional(result.ObjectUnderPick).Filter(static id => id != Guid.Empty),
            InletUnderPick: Optional(result.InletUnderPick).Filter(static id => id != Guid.Empty),
            OutletUnderPick: Optional(result.OutletUnderPick).Filter(static id => id != Guid.Empty));

    // FrameOf reads bounds directly; no Attributes.Layout() mutation during read.
    private static RectangleF FrameOf(IEnumerable<IDocumentObject> targets) =>
        targets.Aggregate(
            seed: Option<RectangleF>.None,
            func: static (acc, obj) => acc.Match(
                Some: bounds => Some(RectangleF.Union(rect1: bounds, rect2: obj.Attributes.AggregateBounds)),
                None: () => Some(obj.Attributes.AggregateBounds)))
            .IfNone(RectangleF.Empty);

    private static Fin<CanvasResult> EncodeBitmap(Bitmap? bitmap, int width, int height, Op op) =>
        Optional(bitmap)
            .ToFin(Fail: UiFault.MutationRejected(op: op, detail: "DrawToBitmap returned null"))
            .Bind(owned => Try.lift<CanvasResult>(f: () => {
                using Bitmap image = owned;
                using MemoryStream stream = new();
                image.Save(stream: stream, format: ImageFormat.Png);
                int w = width > 0 ? width : image.Width;
                int h = height > 0 ? height : image.Height;
                return new CanvasResult.BitmapResult(Bitmap: new CanvasBitmap(Width: w, Height: h, Png: stream.ToArray()));
            }).Run().MapFail(_ => UiFault.MutationRejected(op: op, detail: "PNG encode failed")));

    private static CanvasActionSnapshot ActionSnapshotOf(GhCanvas canvas) =>
        new(
            AllowDrag: canvas.AllowedActions.AllowDrag,
            AllowWireSelect: canvas.AllowedActions.AlloWireSelect,
            AllowObjectSelect: canvas.AllowedActions.AllowObjectSelect,
            AllowMakeWire: canvas.AllowedActions.AllowMakeWire,
            AllowDeleteWire: canvas.AllowedActions.AllowDeleteWire,
            AllowModifyWire: canvas.AllowedActions.AllowModifyWire,
            HasMakeWireFilter: canvas.AllowedActions.MakeWireFilter is not null,
            HasDeleteWireFilter: canvas.AllowedActions.DeleteWireFilter is not null,
            AllowMakeObject: canvas.AllowedActions.AllowMakeObject,
            AllowDeleteObject: canvas.AllowedActions.AllowDeleteObject,
            AllowObjectResponse: canvas.AllowedActions.AllowObjectResponse,
            AllowDropFile: canvas.AllowedActions.AllowDropFile,
            AllowWireMenu: canvas.AllowedActions.AllowWireMenu,
            AllowObjectMenu: canvas.AllowedActions.AllowObjectMenu,
            AllowCanvasMenu: canvas.AllowedActions.AllowCanvasMenu);

}
