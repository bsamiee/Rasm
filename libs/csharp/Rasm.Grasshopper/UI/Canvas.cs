using System.Runtime.InteropServices;
using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Flags]
public enum CanvasBitmapLayers {
    None = 0, Background = 1, Wires = 2, Messages = 4,
    All = Background | Wires | Messages,
}

[Flags]
public enum CanvasWindowScope {
    None = 0, Objects = 1, Wires = 2, Groups = 4, ObjectsAndWires = Objects | Wires,
    All = Objects | Wires | Groups,
}

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
public partial record CanvasLocus {
    private CanvasLocus() { }
    public sealed record PointCase(PointF Value) : CanvasLocus;
    public sealed record BoundsCase(RectangleF Value) : CanvasLocus;
    public static CanvasLocus Point(PointF value) => new PointCase(Value: value);
    public static CanvasLocus Bounds(RectangleF value) => new BoundsCase(Value: value);
    internal Fin<CanvasLocus> Map(GhCanvas canvas, CoordinateSystem from, CoordinateSystem to) =>
        Switch(
            state: (Canvas: canvas, From: from, To: to),
            pointCase: static (state, p) => Optional(p.Value)
                .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasLocus)), detail: "non-finite point"))
                .Map(value => Point(value: state.Canvas.Map(point: value, from: state.From, to: state.To))),
            boundsCase: static (state, b) => Optional(b.Value)
                .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Width) && float.IsFinite(value.Height))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasLocus)), detail: "non-finite bounds"))
                .Map(value => Bounds(value: state.Canvas.Map(rectangle: value, from: state.From, to: state.To))));
}

[Union]
public partial record CanvasViewOp {
    private CanvasViewOp() { }
    public sealed record BoundsCase(RectangleF Region, CanvasNavigationPolicy Policy) : CanvasViewOp;
    public sealed record SelectionCase(Option<Seq<Guid>> Ids, CanvasFramePolicy Policy) : CanvasViewOp;
    public sealed record FitCase(CanvasFitTarget Target, CanvasNavigationPolicy Policy) : CanvasViewOp;
    public sealed record ProjectionCase(PointF Centre, float Zoom) : CanvasViewOp;

    public static CanvasViewOp Bounds(RectangleF bounds, CanvasNavigationPolicy policy = default) => new BoundsCase(Region: bounds, Policy: policy);
    public static CanvasViewOp Selection(Seq<Guid>? ids = null, CanvasFramePolicy policy = default) => new SelectionCase(Ids: Optional(ids), Policy: policy);
    public static CanvasViewOp Fit(CanvasFitTarget target, CanvasNavigationPolicy policy = default) => new FitCase(Target: target, Policy: policy);
    public static CanvasViewOp Projection(PointF centre, float zoom) => new ProjectionCase(Centre: centre, Zoom: zoom);
}

[Union]
public partial record CanvasOp {
    private CanvasOp() { }
    public sealed record SnapshotCase(bool OpenEditor) : CanvasOp;
    public sealed record PickCase(PointF Point, CoordinateSystem Source) : CanvasOp;
    public sealed record MapCase(CanvasLocus Locus, CoordinateSystem From, CoordinateSystem To) : CanvasOp;
    public sealed record InvalidateCase(RepaintRequest Repaint) : CanvasOp;
    public sealed record InstantiateCase(Option<string> SearchText, bool MouseCentred) : CanvasOp;
    public sealed record DetailCase : CanvasOp;
    public sealed record ActionsCase : CanvasOp;
    public sealed record RenderCase(int Width, int Height, CanvasBitmapLayers Layers) : CanvasOp;
    public sealed record PickMapCase : CanvasOp;
    public sealed record ViewCase(CanvasViewOp Request) : CanvasOp;
    public sealed record SnapFeedbackCase(bool Clear) : CanvasOp;
    public sealed record InteractionCase(CanvasInteractionPolicy Policy) : CanvasOp;
    public sealed record WindowSelectCase(WindowSelection Window, Grasshopper2.Extensions.SelectionMode Mode, CanvasWindowScope Scope) : CanvasOp;

    public static CanvasOp Snapshot(bool openEditor = false) => new SnapshotCase(OpenEditor: openEditor);
    public static CanvasOp Pick(PointF point, CoordinateSystem source = CoordinateSystem.Content) => new PickCase(Point: point, Source: source);
    public static CanvasOp Map(CanvasLocus locus, CoordinateSystem from, CoordinateSystem to) => new MapCase(Locus: locus, From: from, To: to);
    public static CanvasOp Invalidate(RepaintRequest? repaint = null) => new InvalidateCase(Repaint: repaint ?? RepaintRequest.Canvas);
    public static CanvasOp Instantiate(string? searchText = null, bool mouseCentred = true) => new InstantiateCase(SearchText: Optional(searchText), MouseCentred: mouseCentred);
    public static readonly CanvasOp Detail = new DetailCase();
    public static CanvasOp Actions() => new ActionsCase();
    public static CanvasOp Render(int width, int height, CanvasBitmapLayers layers = CanvasBitmapLayers.All) => new RenderCase(Width: width, Height: height, Layers: layers);
    public static readonly CanvasOp PickMap = new PickMapCase();
    public static CanvasOp View(CanvasViewOp view) => new ViewCase(Request: view);
    public static CanvasOp SnapFeedback(bool clear = false) => new SnapFeedbackCase(Clear: clear);
    public static CanvasOp Interaction(CanvasInteractionPolicy policy) => new InteractionCase(Policy: policy);
    public static CanvasOp WindowSelect(WindowSelection window, Grasshopper2.Extensions.SelectionMode mode, CanvasWindowScope scope = CanvasWindowScope.ObjectsAndWires) => new WindowSelectCase(Window: window, Mode: mode, Scope: scope);
}

[Union]
public partial record CanvasResult {
    private CanvasResult() { }
    public sealed record SnapshotResult(CanvasSnapshot Snapshot) : CanvasResult;
    public sealed record PickResult(CanvasPickSnapshot Pick) : CanvasResult;
    public sealed record MapResult(CanvasMappedLocus Mapped) : CanvasResult;
    public sealed record BitmapResult(CanvasBitmap Bitmap) : CanvasResult;
    public sealed record InteractionResult(CanvasInteractionSnapshot Interaction) : CanvasResult;
    public sealed record WindowResult(CanvasWindowSnapshot Window) : CanvasResult;
    public sealed record DetailResult(CanvasDetailSnapshot Detail) : CanvasResult;
    public sealed record ActionsResult(CanvasActionSnapshot Snapshot) : CanvasResult;
    public sealed record SnapFeedbackResult(CanvasSnapFeedbackSnapshot Feedback) : CanvasResult;
    public sealed record UnitResult : CanvasResult;
    public static readonly CanvasResult Unit = new UnitResult();
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasNavigationPolicy(float MinimumZoom = 0.05f, float MaximumZoom = 2f, TimeSpan Duration = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasFramePolicy(float Padding = 48f, CanvasNavigationPolicy Navigation = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasInteractionPolicy(
    bool AllowPan = true, bool AllowZoom = true, bool ShowTilesWhenEmpty = true,
    bool WindowSelectObjects = true, bool WindowSelectWires = true, bool WindowSelectGroups = true,
    Option<bool> ViewportDragging = default,
    Option<CanvasActionPolicy> Actions = default,
    Option<CanvasProjectionPolicy> Projection = default,
    bool ClearSnapFeedback = false);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasInteractionSnapshot(CanvasInteractionPolicy Before, CanvasInteractionPolicy After);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasProjectionPolicy(Option<PointF> Centre = default, Option<float> Zoom = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasActionPolicy(
    Option<bool> AllowDrag = default,
    Option<bool> AllowWireSelect = default,
    Option<bool> AllowObjectSelect = default,
    Option<bool> AllowMakeWire = default,
    Option<bool> AllowDeleteWire = default,
    Option<bool> AllowModifyWire = default,
    Option<bool> AllowMakeObject = default,
    Option<bool> AllowDeleteObject = default,
    Option<bool> AllowObjectResponse = default,
    Option<bool> AllowDropFile = default,
    Option<bool> AllowWireMenu = default,
    Option<bool> AllowObjectMenu = default,
    Option<bool> AllowCanvasMenu = default,
    Option<Func<(IParameter source, IParameter target), bool>> MakeWireFilter = default,
    Option<Func<(IParameter source, IParameter target), bool>> DeleteWireFilter = default) {
    internal static CanvasActionPolicy Capture(CanvasActions actions) =>
        new(
            AllowDrag: Some(actions.AllowDrag),
            AllowWireSelect: Some(actions.AlloWireSelect),
            AllowObjectSelect: Some(actions.AllowObjectSelect),
            AllowMakeWire: Some(actions.AllowMakeWire),
            AllowDeleteWire: Some(actions.AllowDeleteWire),
            AllowModifyWire: Some(actions.AllowModifyWire),
            AllowMakeObject: Some(actions.AllowMakeObject),
            AllowDeleteObject: Some(actions.AllowDeleteObject),
            AllowObjectResponse: Some(actions.AllowObjectResponse),
            AllowDropFile: Some(actions.AllowDropFile),
            AllowWireMenu: Some(actions.AllowWireMenu),
            AllowObjectMenu: Some(actions.AllowObjectMenu),
            AllowCanvasMenu: Some(actions.AllowCanvasMenu),
            MakeWireFilter: Optional(actions.MakeWireFilter),
            DeleteWireFilter: Optional(actions.DeleteWireFilter));

    internal Unit Apply(CanvasActions actions) {
        _ = AllowDrag.Iter(value => actions.AllowDrag = value);
        _ = AllowWireSelect.Iter(value => actions.AlloWireSelect = value);
        _ = AllowObjectSelect.Iter(value => actions.AllowObjectSelect = value);
        _ = AllowMakeWire.Iter(value => actions.AllowMakeWire = value);
        _ = AllowDeleteWire.Iter(value => actions.AllowDeleteWire = value);
        _ = AllowModifyWire.Iter(value => actions.AllowModifyWire = value);
        _ = AllowMakeObject.Iter(value => actions.AllowMakeObject = value);
        _ = AllowDeleteObject.Iter(value => actions.AllowDeleteObject = value);
        _ = AllowObjectResponse.Iter(value => actions.AllowObjectResponse = value);
        _ = AllowDropFile.Iter(value => actions.AllowDropFile = value);
        _ = AllowWireMenu.Iter(value => actions.AllowWireMenu = value);
        _ = AllowObjectMenu.Iter(value => actions.AllowObjectMenu = value);
        _ = AllowCanvasMenu.Iter(value => actions.AllowCanvasMenu = value);
        _ = MakeWireFilter.Iter(value => actions.MakeWireFilter = value);
        _ = DeleteWireFilter.Iter(value => actions.DeleteWireFilter = value);
        return unit;
    }
}

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
public readonly record struct CanvasSnapFeedbackSnapshot(
    Option<SnappingSnapshot> X,
    Option<SnappingSnapshot> Y);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasBitmap(int Width, int Height, ReadOnlyMemory<byte> Png);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasMappedLocus(CanvasLocus Source, CanvasLocus Target, CoordinateSystem From, CoordinateSystem To);

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

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasWindowSnapshot(CanvasSnapshot Canvas, int SelectedCount, int DeselectedCount);

internal sealed record CanvasRequest(CanvasOp Op) : GhUiRequest<CanvasResult> {
    internal override GrasshopperUiPolicy Policy => CanvasPolicyOf(op: Op);
    internal override Fin<CanvasResult> Apply(GrasshopperUi.Scope scope) => UiRail.CanvasDispatch(scope: scope, op: Op);

    internal static GrasshopperUiPolicy CanvasPolicyOf(CanvasOp op) =>
        op.Switch(
            snapshotCase: static s => GrasshopperUiPolicy.Canvas(openEditor: s.OpenEditor),
            pickCase: static _ => GrasshopperUiPolicy.Canvas(),
            mapCase: static _ => GrasshopperUiPolicy.Canvas(),
            invalidateCase: static i => GrasshopperUiPolicy.Canvas(repaint: i.Repaint),
            instantiateCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
            detailCase: static _ => GrasshopperUiPolicy.Canvas(),
            actionsCase: static _ => GrasshopperUiPolicy.Canvas(),
            renderCase: static _ => GrasshopperUiPolicy.Canvas(),
            pickMapCase: static _ => GrasshopperUiPolicy.Canvas(),
            viewCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true, repaint: RepaintRequest.Canvas),
            snapFeedbackCase: static _ => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas),
            interactionCase: static i => i.Policy.Projection.IsSome
                ? GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas)
                : GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas),
            windowSelectCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas));
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class UiRail {
    // --- [OPERATIONS] -------------------------------------------------------------------------
    internal static Fin<CanvasResult> CanvasDispatch(GrasshopperUi.Scope scope, CanvasOp op) => op switch {
        CanvasOp.SnapshotCase =>
            scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.SnapshotResult(
                Snapshot: SnapshotOf(canvas: canvas, document: canvas.Document, objects: canvas.Document.Objects))),
        CanvasOp.PickCase p =>
            Optional(p.Point).Filter(static pt => float.IsFinite(pt.X) && float.IsFinite(pt.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Pick)), detail: "non-finite point"))
                .Bind(valid => scope.NeedCanvas().Map(canvas => {
                    PointF content = p.Source == CoordinateSystem.Content ? valid : canvas.Map(point: valid, from: p.Source, to: CoordinateSystem.Content);
                    SelectionResult result = canvas.ResolvePick(point: content, includeGrips: true, includeForeground: true, includeBackground: true, includeWires: true, recursive: true);
                    return (CanvasResult)new CanvasResult.PickResult(Pick: PickSnapshotOf(result: result));
                })),
        CanvasOp.MapCase m =>
            Optional(m.Locus)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Map)), detail: "locus is required"))
                .Bind(locus => scope.NeedCanvas().Bind(canvas => locus.Map(canvas: canvas, from: m.From, to: m.To)
                    .Map(mapped => (CanvasResult)new CanvasResult.MapResult(Mapped: new CanvasMappedLocus(Source: locus, Target: mapped, From: m.From, To: m.To))))),
        CanvasOp.InvalidateCase =>
            scope.NeedCanvas().Map(_ => CanvasResult.Unit),
        CanvasOp.InstantiateCase ins =>
            scope.NeedCanvas()
                .Bind(canvas => Optional(canvas.AllowedActions.AllowMakeObject)
                    .Filter(static allowed => allowed)
                    .ToFin(Fail: UiFault.MutationRejected(op: Op.Of(name: nameof(CanvasOp.Instantiate)), detail: "canvas disallows object creation"))
                    .Map(_ => {
                        canvas.ShowInstantiationPopup(mouseCentred: ins.MouseCentred, initialText: ins.SearchText.IfNone(string.Empty));
                        return CanvasResult.Unit;
                    })),
        CanvasOp.DetailCase =>
            scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.DetailResult(
                Detail: new CanvasDetailSnapshot(
                    ShowUndoHistory: canvas.ShowUndoHistory,
                    ZuiVariableParameterThreshold: canvas.ZuiVariableParameterThreshold,
                    ZuiVariableParameterState: canvas.ZuiVariableParameterState,
                    ZuiWireDetailingThreshold: canvas.ZuiWireDetailingThreshold,
                    ZuiWireDetailingState: canvas.ZuiWireDetailingState,
                    Actions: ActionSnapshotOf(canvas: canvas)))),
        CanvasOp.ActionsCase =>
            scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.ActionsResult(Snapshot: ActionSnapshotOf(canvas: canvas))),
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
        CanvasOp.ViewCase v =>
            ViewDispatch(scope: scope, view: v.Request),
        CanvasOp.SnapFeedbackCase f =>
            scope.NeedCanvas().Map(canvas => {
                _ = f.Clear ? ClearSnapFeedback(canvas: canvas) : unit;
                return (CanvasResult)new CanvasResult.SnapFeedbackResult(Feedback: SnapFeedbackOf(canvas: canvas));
            }),
        CanvasOp.InteractionCase ic =>
            scope.NeedCanvas().Bind(canvas => {
                CanvasInteractionPolicy before = InteractionOf(canvas: canvas, document: scope.Document);
                canvas.AllowPan = ic.Policy.AllowPan;
                canvas.AllowZoom = ic.Policy.AllowZoom;
                canvas.ShowTilesWhenEmpty = ic.Policy.ShowTilesWhenEmpty;
                canvas.WindowSelectObjects = ic.Policy.WindowSelectObjects;
                canvas.WindowSelectWires = ic.Policy.WindowSelectWires;
                canvas.WindowSelectGroups = ic.Policy.WindowSelectGroups;
                _ = ic.Policy.ViewportDragging.Iter(value => canvas.ViewportDragging = value);
                _ = ic.Policy.Actions.Iter(policy => policy.Apply(actions: canvas.AllowedActions));
                _ = ic.Policy.ClearSnapFeedback ? ClearSnapFeedback(canvas: canvas) : unit;
                return ic.Policy.Projection.TraverseM(projection => ApplyProjection(scope: scope, policy: projection)).As()
                    .Map(_ => (CanvasResult)new CanvasResult.InteractionResult(
                        Interaction: new CanvasInteractionSnapshot(Before: before, After: InteractionOf(canvas: canvas, document: scope.Document))));
            }),
        CanvasOp.WindowSelectCase ws =>
            scope.NeedObjects().Bind(objs => scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Map(doc => {
                SelectionResult result = objs.WindowSelect(window: ws.Window, mode: ws.Mode,
                    considerForeground: (ws.Scope & CanvasWindowScope.Objects) == CanvasWindowScope.Objects,
                    considerBackground: (ws.Scope & CanvasWindowScope.Groups) == CanvasWindowScope.Groups,
                    considerWires: (ws.Scope & CanvasWindowScope.Wires) == CanvasWindowScope.Wires);
                return (CanvasResult)new CanvasResult.WindowResult(Window: new CanvasWindowSnapshot(
                    Canvas: SnapshotOf(canvas: canvas, document: doc, objects: objs),
                    SelectedCount: result.SelectedCount,
                    DeselectedCount: result.DeselectedCount));
            }))),
        _ => Fin.Fail<CanvasResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasDispatch)), detail: "unknown CanvasOp")),
    };

    private static Fin<CanvasResult> ViewDispatch(GrasshopperUi.Scope scope, CanvasViewOp view) =>
        view switch {
            CanvasViewOp.BoundsCase n =>
                Optional((Frame: n.Region, Policy: ResolveNavigation(n.Policy)))
                    .Filter(static s => float.IsFinite(s.Frame.X) && float.IsFinite(s.Frame.Y) && float.IsFinite(s.Frame.Width) && float.IsFinite(s.Frame.Height) && s.Policy.MinimumZoom > 0 && s.Policy.MaximumZoom >= s.Policy.MinimumZoom)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasViewOp.Bounds)), detail: "non-finite frame or invalid zoom range"))
                    .Bind(valid => scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Bind(doc => scope.NeedObjects().Map(objs =>
                        (CanvasResult)NavigateTo(canvas: canvas, frame: valid.Frame, policy: valid.Policy, document: doc, objects: objs))))),
            CanvasViewOp.SelectionCase f =>
                from canvas in scope.NeedCanvas()
                from doc in scope.NeedDocument()
                from objs in scope.NeedObjects()
                let frame = ResolveFrame(raw: f.Policy)
                let targets = f.Ids
                    .Map(list => list.Choose(id => Optional(objs.Find(instanceId: id))))
                    .IfNone(toSeq(objs.SelectedObjects))
                from aggregate in FrameOf(targets: targets)
                    .Filter(static bounds => float.IsFinite(bounds.Width) && float.IsFinite(bounds.Height) && bounds.Width > 0 && bounds.Height > 0)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasViewOp.Selection)), detail: "no target objects"))
                select (CanvasResult)NavigateTo(
                    canvas: canvas,
                    frame: RectangleF.Inflate(rectangle: aggregate, width: frame.Padding, height: frame.Padding),
                    policy: ResolveNavigation(raw: frame.Navigation),
                    document: doc,
                    objects: objs),
            CanvasViewOp.FitCase ft =>
                scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Bind(doc => scope.NeedObjects().Map(objs => {
                    RectangleF frame = ft.Target.Switch(
                        contentCase: _ => canvas.ContentBounds,
                        selectionCase: _ => FrameOf(targets: objs.SelectedObjects).IfNone(RectangleF.Empty),
                        viewportCase: _ => canvas.VisibleFrame);
                    return (CanvasResult)NavigateTo(canvas: canvas, frame: frame, policy: ResolveNavigation(raw: ft.Policy), document: doc, objects: objs);
                }))),
            CanvasViewOp.ProjectionCase pr =>
                from _ in ApplyProjection(scope: scope, policy: new CanvasProjectionPolicy(Centre: Some(pr.Centre), Zoom: Some(pr.Zoom)))
                from canvas in scope.NeedCanvas()
                from doc in scope.NeedDocument()
                from objs in scope.NeedObjects()
                select (CanvasResult)new CanvasResult.SnapshotResult(Snapshot: SnapshotOf(canvas: canvas, document: doc, objects: objs)),
            _ => Fin.Fail<CanvasResult>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ViewDispatch)), detail: "unknown CanvasViewOp")),
        };

    private static Fin<Unit> ApplyProjection(GrasshopperUi.Scope scope, CanvasProjectionPolicy policy) =>
        from document in scope.NeedDocument()
        from canvas in scope.NeedCanvas()
        let centre = policy.Centre.IfNone(document.Projection.centre)
        let zoom = policy.Zoom.IfNone(document.Projection.zoom)
        from valid in Optional((Centre: centre, Zoom: zoom))
            .Filter(static s => float.IsFinite(s.Centre.X) && float.IsFinite(s.Centre.Y) && s.Zoom > 0 && float.IsFinite(s.Zoom))
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasViewOp.Projection)), detail: "non-finite centre or zoom"))
        select ((Func<Unit>)(() => {
            canvas.Projection = canvas.Projection.SetZoom(zoom: valid.Zoom).SetCentre(centre: valid.Centre, frame: canvas.VisibleFrame);
            document.Projection = (valid.Centre, valid.Zoom);
            return unit;
        }))();

    private static CanvasNavigationPolicy ResolveNavigation(CanvasNavigationPolicy raw) =>
        (raw.MinimumZoom, raw.MaximumZoom, raw.Duration) switch {
            (float minimum, float maximum, TimeSpan duration) when float.IsFinite(minimum) && minimum > 0f && float.IsFinite(maximum) && maximum >= minimum =>
                new(MinimumZoom: minimum, MaximumZoom: maximum, Duration: duration == default ? TimeSpan.FromMilliseconds(value: 250) : duration),
            (float minimum, _, TimeSpan duration) when float.IsFinite(minimum) && minimum > 0f =>
                new(MinimumZoom: minimum, MaximumZoom: Math.Max(val1: minimum, val2: 2f), Duration: duration == default ? TimeSpan.FromMilliseconds(value: 250) : duration),
            (_, _, TimeSpan duration) =>
                new(MinimumZoom: 0.05f, MaximumZoom: 2f, Duration: duration == default ? TimeSpan.FromMilliseconds(value: 250) : duration),
        };

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

    private static CanvasInteractionPolicy InteractionOf(GhCanvas canvas, Option<GhDocument> document = default) =>
        new(
            AllowPan: canvas.AllowPan,
            AllowZoom: canvas.AllowZoom,
            ShowTilesWhenEmpty: canvas.ShowTilesWhenEmpty,
            WindowSelectObjects: canvas.WindowSelectObjects,
            WindowSelectWires: canvas.WindowSelectWires,
            WindowSelectGroups: canvas.WindowSelectGroups,
            ViewportDragging: Some(canvas.ViewportDragging),
            Actions: Some(CanvasActionPolicy.Capture(actions: canvas.AllowedActions)),
            Projection: document.Map(static doc => new CanvasProjectionPolicy(
                Centre: Some(doc.Projection.centre),
                Zoom: Some(doc.Projection.zoom))));

    private static Unit ClearSnapFeedback(GhCanvas canvas) {
        canvas.SnapXAction = null;
        canvas.SnapYAction = null;
        return unit;
    }

    private static CanvasSnapFeedbackSnapshot SnapFeedbackOf(GhCanvas canvas) =>
        new(
            X: Layout.SnapshotOf(x: Optional(canvas.SnapXAction), y: Option<SnappingAction>.None),
            Y: Layout.SnapshotOf(x: Option<SnappingAction>.None, y: Optional(canvas.SnapYAction)));

    private static CanvasPickSnapshot PickSnapshotOf(SelectionResult result) =>
        new(Kind: result.Kind, Point: Optional(result.Point),
            SelectedCount: result.SelectedCount, DeselectedCount: result.DeselectedCount,
            WireUnderPick: Optional(result.WireUnderPick).Filter(static wire => wire.Source != Guid.Empty && wire.Target != Guid.Empty),
            ObjectUnderPick: Optional(result.ObjectUnderPick).Filter(static id => id != Guid.Empty),
            InletUnderPick: Optional(result.InletUnderPick).Filter(static id => id != Guid.Empty),
            OutletUnderPick: Optional(result.OutletUnderPick).Filter(static id => id != Guid.Empty));

    // FrameOf reads bounds directly; no Attributes.Layout() mutation during read.
    private static Option<RectangleF> FrameOf(IEnumerable<IDocumentObject> targets) =>
        toSeq(targets).Fold(
            initialState: Option<RectangleF>.None,
            f: static (acc, obj) => acc.Match(
                Some: bounds => Some(RectangleF.Union(rect1: bounds, rect2: obj.Attributes.AggregateBounds)),
                None: () => Some(obj.Attributes.AggregateBounds)));

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
            }).Run().MapFail(error => UiFault.MutationRejected(op: op, detail: $"PNG encode failed: {error.Message}")));

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
