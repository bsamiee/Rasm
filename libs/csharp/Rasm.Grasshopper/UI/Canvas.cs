using System.Globalization;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhDocument = Grasshopper2.Doc.Document;
using GhDuration = Grasshopper2.UI.Animation.Duration;
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

[Flags]
public enum CanvasPickPolicy {
    None = 0, Grips = 1, Foreground = 2, Background = 4, Wires = 8, Recursive = 16,
    All = Grips | Foreground | Background | Wires | Recursive,
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
            pointCase: static (state, p) => Op.Of(name: nameof(CanvasLocus))
                .AcceptPoint(value: p.Value, detail: "non-finite point")
                .Map(value => Point(value: state.Canvas.Map(point: value, from: state.From, to: state.To))),
            boundsCase: static (state, b) => Op.Of(name: nameof(CanvasLocus))
                .AcceptRect(value: b.Value, detail: "non-finite bounds")
                .Map(value => Bounds(value: state.Canvas.Map(rectangle: value, from: state.From, to: state.To))));
}

[Union]
public partial record CanvasViewOp {
    private CanvasViewOp() { }
    public sealed record BoundsCase(RectangleF Region, CanvasViewPolicy Policy) : CanvasViewOp;
    public sealed record SelectionCase(Option<Seq<Guid>> Ids, CanvasViewPolicy Policy) : CanvasViewOp;
    public sealed record FitCase(CanvasFitTarget Target, CanvasViewPolicy Policy) : CanvasViewOp;
    public sealed record ProjectionCase(PointF Centre, float Zoom) : CanvasViewOp;
    public sealed record PositionCase(ContentPosition Where, GhDuration Duration) : CanvasViewOp;

    public static CanvasViewOp Bounds(RectangleF bounds, CanvasViewPolicy policy = default) => new BoundsCase(Region: bounds, Policy: policy);
    public static CanvasViewOp Selection(Seq<Guid>? ids = null, CanvasViewPolicy policy = default) => new SelectionCase(Ids: Optional(ids), Policy: policy);
    public static CanvasViewOp Fit(CanvasFitTarget target, CanvasViewPolicy policy = default) => new FitCase(Target: target, Policy: policy);
    public static CanvasViewOp Projection(PointF centre, float zoom) => new ProjectionCase(Centre: centre, Zoom: zoom);
    // Snap-to-edge / preset positioning via GH2-native ContentPosition (Top/Bottom/Left/Right/Centre/Fit/HundredPercent/TopLeft/...).
    public static CanvasViewOp Position(ContentPosition position, GhDuration duration = GhDuration.Normal) => new PositionCase(Where: position, Duration: duration);
}

[Union]
public partial record CanvasOp {
    private CanvasOp() { }
    public sealed record SnapshotCase(bool OpenEditor) : CanvasOp;
    public sealed record PickCase(PointF Point, CoordinateSystem Source, CanvasPickPolicy Policy) : CanvasOp;
    public sealed record MapCase(CanvasLocus Locus, CoordinateSystem From, CoordinateSystem To) : CanvasOp;
    public sealed record InvalidateCase(RepaintRequest Repaint) : CanvasOp;
    public sealed record InstantiateCase(Option<string> SearchText, bool MouseCentred) : CanvasOp;
    public sealed record SearchCase : CanvasOp;
    public sealed record FocusCase : CanvasOp;
    public sealed record DetailCase : CanvasOp;
    public sealed record ActionsCase : CanvasOp;
    public sealed record RenderCase(int Width, int Height, CanvasBitmapLayers Layers) : CanvasOp;
    public sealed record PickMapCase : CanvasOp;
    public sealed record ViewCase(CanvasViewOp Request) : CanvasOp;
    public sealed record SnapFeedbackCase(bool Clear) : CanvasOp;
    public sealed record InteractionCase(CanvasInteractionPolicy Policy) : CanvasOp;
    public sealed record WindowSelectCase(WindowSelection Window, Grasshopper2.Extensions.SelectionMode Mode, CanvasWindowScope Scope) : CanvasOp;

    public static CanvasOp Snapshot(bool openEditor = false) => new SnapshotCase(OpenEditor: openEditor);
    public static CanvasOp Pick(PointF point, CoordinateSystem source = CoordinateSystem.Content, CanvasPickPolicy policy = CanvasPickPolicy.All) => new PickCase(Point: point, Source: source, Policy: policy);
    public static CanvasOp Map(CanvasLocus locus, CoordinateSystem from, CoordinateSystem to) => new MapCase(Locus: locus, From: from, To: to);
    public static CanvasOp Invalidate(RepaintRequest? repaint = null) => new InvalidateCase(Repaint: repaint ?? RepaintRequest.Canvas);
    public static CanvasOp Instantiate(string? searchText = null, bool mouseCentred = true) => new InstantiateCase(SearchText: Optional(searchText), MouseCentred: mouseCentred);
    public static readonly CanvasOp Search = new SearchCase();
    public static readonly CanvasOp Focus = new FocusCase();
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
    public sealed record FocusResult(Option<string> FocusedNomen) : CanvasResult;
    public sealed record UnitResult : CanvasResult;
    public static readonly CanvasResult Unit = new UnitResult();
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasViewPolicy(
    float MinimumZoom = CanvasViewPolicy.DefaultMinimumZoom,
    float MaximumZoom = CanvasViewPolicy.DefaultMaximumZoom,
    TimeSpan Duration = default,
    float Padding = CanvasViewPolicy.DefaultPadding) {
    public const float DefaultMinimumZoom = 0.05f;
    public const float DefaultMaximumZoom = 2f;
    public const float DefaultPadding = 0f;
    public const float SelectionFitPadding = 48f;
}

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

// Float not double — Eto.Drawing zoom and GH2 Projection are float-keyed.
[ValueObject<float>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError<UiFault>]
public readonly partial struct ZoomFactor {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref float value) =>
        validationError = float.IsFinite(value) && value is > 0f and <= 1000f
            ? null
            : UiFault.Create(op: Op.Of(name: nameof(ZoomFactor)), message: string.Create(CultureInfo.InvariantCulture, $"must be finite within (0, 1000] (got {value:R})."));
}

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
            searchCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
            focusCase: static _ => GrasshopperUiPolicy.Canvas(),
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
    // Quartz hard ceiling — beyond 16384×16384 the framework silently downsamples (Apple WWDC22
    // CoreGraphics session). Per-canvas effective max derives from the host window's screen pixel
    // dimensions; capped at the architectural ceiling. The static constant remains the upper bound,
    // RenderDimensionLimit() returns the device-respecting bound for any concrete Canvas.
    internal const int MaxRenderDimension = 16384;

    internal static int RenderDimensionLimit(GhCanvas canvas) {
        Eto.Forms.Window? window = (canvas.ControlObject as Eto.Forms.Control)?.ParentWindow;
        if (window is null) {
            return MaxRenderDimension;
        }
        float logicalPx = window.LogicalPixelSize;
        if (logicalPx <= 0f) {
            return MaxRenderDimension;
        }
        Eto.Drawing.SizeF screenSize = window.Screen.Bounds.Size;
        int devicePixelMax = (int)MathF.Round(MathF.Max(screenSize.Width, screenSize.Height) * logicalPx);
        return Math.Min(MaxRenderDimension, devicePixelMax > 0 ? devicePixelMax : MaxRenderDimension);
    }

    // --- [OPERATIONS] -------------------------------------------------------------------------
    internal static Fin<CanvasResult> CanvasDispatch(GrasshopperUi.Scope scope, CanvasOp op) => op switch {
        CanvasOp.SnapshotCase =>
            scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.SnapshotResult(
                Snapshot: SnapshotOf(canvas: canvas, document: canvas.Document, objects: canvas.Document.Objects))),
        CanvasOp.PickCase p =>
            Op.Of(name: nameof(CanvasOp.Pick)).AcceptPoint(value: p.Point, detail: "non-finite point")
                .Bind(valid => scope.NeedCanvas().Map(canvas => {
                    PointF content = p.Source == CoordinateSystem.Content ? valid : canvas.Map(point: valid, from: p.Source, to: CoordinateSystem.Content);
                    SelectionResult result = canvas.ResolvePick(
                        point: content,
                        includeGrips: (p.Policy & CanvasPickPolicy.Grips) == CanvasPickPolicy.Grips,
                        includeForeground: (p.Policy & CanvasPickPolicy.Foreground) == CanvasPickPolicy.Foreground,
                        includeBackground: (p.Policy & CanvasPickPolicy.Background) == CanvasPickPolicy.Background,
                        includeWires: (p.Policy & CanvasPickPolicy.Wires) == CanvasPickPolicy.Wires,
                        recursive: (p.Policy & CanvasPickPolicy.Recursive) == CanvasPickPolicy.Recursive);
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
        CanvasOp.SearchCase =>
            scope.NeedCanvas().Map(canvas => { canvas.ShowSearchPopup(); return CanvasResult.Unit; }),
        CanvasOp.FocusCase =>
            scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.FocusResult(
                FocusedNomen: Optional(canvas.FocusObject).Map(static target => target switch {
                    IInteraction interaction => interaction.Nomen.Name,
                    _ => target.GetType().Name,
                }))),
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
            scope.NeedCanvas().Bind(canvas => {
                int limit = RenderDimensionLimit(canvas: canvas);
                return Optional((r.Width, r.Height))
                    .Filter(dim => dim.Width > 0 && dim.Width <= limit && dim.Height > 0 && dim.Height <= limit)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Render)), detail: $"dimensions out of [1, {limit}]"))
                    .Bind(dim => EncodeBitmap(
                        bitmap: canvas.DrawToBitmap(
                            width: dim.Width, height: dim.Height,
                            drawBackground: (r.Layers & CanvasBitmapLayers.Background) == CanvasBitmapLayers.Background,
                            drawWires: (r.Layers & CanvasBitmapLayers.Wires) == CanvasBitmapLayers.Wires,
                            drawMessages: (r.Layers & CanvasBitmapLayers.Messages) == CanvasBitmapLayers.Messages),
                        width: dim.Width, height: dim.Height,
                        op: Op.Of(name: nameof(CanvasOp.Render))));
            }),
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
                from frame in Op.Of(name: nameof(CanvasViewOp.Bounds)).AcceptRect(value: n.Region, detail: "non-finite frame")
                let policy = ResolveView(raw: n.Policy)
                from validZoom in policy.MaximumZoom >= policy.MinimumZoom
                    ? Fin.Succ(value: policy)
                    : Fin.Fail<CanvasViewPolicy>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasViewOp.Bounds)), detail: "invalid zoom range"))
                from canvas in scope.NeedCanvas()
                from doc in scope.NeedDocument()
                from objs in scope.NeedObjects()
                select (CanvasResult)NavigateTo(canvas: canvas, frame: frame, policy: validZoom, document: doc, objects: objs),
            CanvasViewOp.SelectionCase f =>
                from canvas in scope.NeedCanvas()
                from doc in scope.NeedDocument()
                from objs in scope.NeedObjects()
                let policy = ResolveView(raw: f.Policy, defaultPadding: CanvasViewPolicy.SelectionFitPadding)
                let targets = f.Ids
                    .Map(list => list.Choose(id => Optional(objs.Find(instanceId: id))))
                    .IfNone(toSeq(objs.SelectedObjects))
                from aggregate in FrameOf(targets: targets)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasViewOp.Selection)), detail: "no target objects"))
                from validAggregate in Op.Of(name: nameof(CanvasViewOp.Selection)).AcceptRect(value: aggregate, detail: "non-finite or zero-size aggregate", requirePositive: true)
                select (CanvasResult)NavigateTo(
                    canvas: canvas,
                    frame: RectangleF.Inflate(rectangle: validAggregate, width: policy.Padding, height: policy.Padding),
                    policy: policy,
                    document: doc,
                    objects: objs),
            CanvasViewOp.FitCase ft =>
                scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Bind(doc => scope.NeedObjects().Map(objs => {
                    RectangleF frame = ft.Target.Switch(
                        state: (canvas, objs),
                        contentCase: static (state, _) => state.canvas.ContentBounds,
                        selectionCase: static (state, _) => FrameOf(targets: state.objs.SelectedObjects).IfNone(RectangleF.Empty),
                        viewportCase: static (state, _) => state.canvas.VisibleFrame);
                    return (CanvasResult)NavigateTo(canvas: canvas, frame: frame, policy: ResolveView(raw: ft.Policy), document: doc, objects: objs);
                }))),
            CanvasViewOp.PositionCase pos =>
                from canvas in scope.NeedCanvas()
                from doc in scope.NeedDocument()
                from objs in scope.NeedObjects()
                from _ in Op.Of(name: nameof(CanvasViewOp.Position)).Attempt(
                    body: () => { canvas.Navigate(position: pos.Where, duration: pos.Duration); return unit; },
                    what: "FlexControl.Navigate(ContentPosition)")
                select (CanvasResult)new CanvasResult.SnapshotResult(Snapshot: SnapshotOf(canvas: canvas, document: doc, objects: objs)),
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
        from _ in Try.lift(f: () => {
            canvas.Projection = canvas.Projection.SetZoom(zoom: valid.Zoom).SetCentre(centre: valid.Centre, frame: canvas.VisibleFrame);
            document.Projection = (valid.Centre, valid.Zoom);
            return unit;
        }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(CanvasViewOp.Projection)), detail: $"projection assign threw: {error.Message}"))
        select unit;

    private static CanvasViewPolicy ResolveView(CanvasViewPolicy raw, float defaultPadding = CanvasViewPolicy.DefaultPadding) {
        TimeSpan duration = raw.Duration == default ? Animators.DurationToTimeSpan(duration: GhDuration.Normal) : raw.Duration;
        bool minOk = float.IsFinite(raw.MinimumZoom) && raw.MinimumZoom > 0f;
        bool maxOk = float.IsFinite(raw.MaximumZoom) && raw.MaximumZoom >= raw.MinimumZoom;
        float minimum = minOk ? raw.MinimumZoom : CanvasViewPolicy.DefaultMinimumZoom;
        float maximum = (minOk && maxOk) ? raw.MaximumZoom : Math.Max(val1: minimum, val2: CanvasViewPolicy.DefaultMaximumZoom);
        float padding = float.IsFinite(raw.Padding) && raw.Padding > 0f ? raw.Padding : defaultPadding;
        return new(MinimumZoom: minimum, MaximumZoom: maximum, Duration: duration, Padding: padding);
    }

    private static CanvasResult.SnapshotResult NavigateTo(GhCanvas canvas, RectangleF frame, CanvasViewPolicy policy, GhDocument document, GhObjectList objects) {
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
