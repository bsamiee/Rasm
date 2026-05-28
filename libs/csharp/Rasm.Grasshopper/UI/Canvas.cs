using System.Globalization;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.Undo;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhDocument = Grasshopper2.Doc.Document;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
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

[SkipUnionOps]
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

[SkipUnionOps]
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

[SkipUnionOps]
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

[GenerateUnionOps]
[Union]
public partial record CanvasOp {
    private CanvasOp() { }
    public sealed partial record SnapshotCase(bool OpenEditor) : CanvasOp;
    public sealed partial record PickCase(PointF Point, CoordinateSystem Source, CanvasPickPolicy Policy, PickTolerance Tolerance) : CanvasOp;
    public sealed partial record MapCase(CanvasLocus Locus, CoordinateSystem From, CoordinateSystem To) : CanvasOp;
    public sealed partial record InvalidateCase(RepaintRequest Repaint) : CanvasOp;
    public sealed partial record InstantiateCase(Option<string> SearchText, bool MouseCentred) : CanvasOp;
    public sealed partial record FocusCase : CanvasOp;
    public sealed partial record DetailCase : CanvasOp;
    public sealed partial record ActionsCase : CanvasOp;
    public sealed partial record RenderCase(int Width, int Height, CanvasBitmapLayers Layers) : CanvasOp;
    public sealed partial record PickMapCase : CanvasOp;
    public sealed partial record ViewCase(CanvasViewOp Request) : CanvasOp;
    public sealed partial record SnapFeedbackCase(bool Clear) : CanvasOp;
    public sealed partial record InteractionCase(CanvasInteractionPolicy Policy) : CanvasOp;
    public sealed partial record WindowSelectCase(WindowSelection Window, Grasshopper2.Extensions.SelectionMode Mode, CanvasWindowScope Scope) : CanvasOp;

    public static CanvasOp Snapshot(bool openEditor = false) => new SnapshotCase(OpenEditor: openEditor);
    public static CanvasOp Pick(PointF point, CoordinateSystem source = CoordinateSystem.Content, CanvasPickPolicy policy = CanvasPickPolicy.All) =>
        Pick(point: point, source: source, policy: policy, tolerance: PickTolerance.Create(0f));
    public static CanvasOp Pick(PointF point, CoordinateSystem source, CanvasPickPolicy policy, PickTolerance tolerance) =>
        new PickCase(Point: point, Source: source, Policy: policy, Tolerance: tolerance);
    public static CanvasOp Map(CanvasLocus locus, CoordinateSystem from, CoordinateSystem to) => new MapCase(Locus: locus, From: from, To: to);
    public static CanvasOp Invalidate(RepaintRequest? repaint = null) => new InvalidateCase(Repaint: repaint ?? RepaintRequest.Canvas);
    public static CanvasOp Instantiate(string? searchText = null, bool mouseCentred = true) => new InstantiateCase(SearchText: Optional(searchText), MouseCentred: mouseCentred);
    public static readonly CanvasOp Focus = new FocusCase();
    public static readonly CanvasOp Detail = new DetailCase();
    public static CanvasOp Actions() => new ActionsCase();
    public static CanvasOp Render(int width, int height, CanvasBitmapLayers layers = CanvasBitmapLayers.All) => new RenderCase(Width: width, Height: height, Layers: layers);
    public static readonly CanvasOp PickMap = new PickMapCase();
    public static CanvasOp View(CanvasViewOp view) => new ViewCase(Request: view);
    public static CanvasOp SnapFeedback(bool clear = false) => new SnapFeedbackCase(Clear: clear);
    public static CanvasOp Interaction(CanvasInteractionPolicy policy) => new InteractionCase(Policy: policy);
    public static CanvasOp WindowSelect(WindowSelection window, Grasshopper2.Extensions.SelectionMode mode, CanvasWindowScope scope = CanvasWindowScope.ObjectsAndWires) => new WindowSelectCase(Window: window, Mode: mode, Scope: scope);

    // GH2 ObjectList.WindowSelect(..., considerForeground, considerBackground, considerWires):
    // CanvasWindowScope.Groups maps to considerBackground (objects below wires); canvas flag is WindowSelectGroups.
    internal GrasshopperUiPolicy UiPolicy => Switch(
        snapshotCase: static s => GrasshopperUiPolicy.Canvas(openEditor: s.OpenEditor),
        pickCase: static _ => GrasshopperUiPolicy.Canvas(),
        mapCase: static _ => GrasshopperUiPolicy.Canvas(),
        invalidateCase: static i => GrasshopperUiPolicy.Canvas(repaint: i.Repaint),
        instantiateCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
        focusCase: static _ => GrasshopperUiPolicy.Canvas(),
        detailCase: static _ => GrasshopperUiPolicy.Canvas(),
        actionsCase: static _ => GrasshopperUiPolicy.Canvas(),
        renderCase: static _ => GrasshopperUiPolicy.Canvas(),
        pickMapCase: static _ => GrasshopperUiPolicy.Canvas(),
        viewCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true, repaint: RepaintRequest.Canvas),
        snapFeedbackCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true, repaint: RepaintRequest.Canvas),
        interactionCase: static i => i.Policy.Projection.IsSome
            ? GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas)
            : GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas),
        windowSelectCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas));
}

[SkipUnionOps]
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

[ComplexValueObject]
[ValidationError<UiFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct CanvasInteractionPolicy {
    public bool AllowPan { get; }
    public bool AllowZoom { get; }
    public bool ShowTilesWhenEmpty { get; }
    public bool WindowSelectObjects { get; }
    public bool WindowSelectWires { get; }
    public bool WindowSelectGroups { get; }
    public Option<bool> ViewportDragging { get; }
    public Option<CanvasActionPolicy> Actions { get; }
    public Option<CanvasProjectionPolicy> Projection { get; }
    public bool ClearSnapFeedback { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref UiFault? validationError,
        ref bool allowPan,
        ref bool allowZoom,
        ref bool showTilesWhenEmpty,
        ref bool windowSelectObjects,
        ref bool windowSelectWires,
        ref bool windowSelectGroups,
        ref Option<bool> viewportDragging,
        ref Option<CanvasActionPolicy> actions,
        ref Option<CanvasProjectionPolicy> projection,
        ref bool clearSnapFeedback) {
        Op op = Op.Of(name: nameof(CanvasInteractionPolicy));
        bool centreInvalid = projection
            .Bind(static policy => policy.Centre)
            .Map(static centre => !float.IsFinite(centre.X) || !float.IsFinite(centre.Y))
            .IfNone(noneValue: false);
        bool zoomInvalid = projection
            .Bind(static policy => policy.Zoom)
            .Map(static zoom => !float.IsFinite(zoom) || zoom <= 0f)
            .IfNone(noneValue: false);
        Seq<string> errors = Seq(
            centreInvalid ? "Projection.Centre must be finite." : "",
            zoomInvalid ? "Projection.Zoom must be finite and > 0." : "")
            .Filter(static message => !string.IsNullOrEmpty(message));
        validationError = errors.IsEmpty ? null : UiFault.Create(op: op, message: string.Join("; ", errors));
        _ = (allowPan, allowZoom, showTilesWhenEmpty, windowSelectObjects, windowSelectWires, windowSelectGroups, viewportDragging, actions, clearSnapFeedback);
    }

    public static CanvasInteractionPolicy Default => Create(
        allowPan: true,
        allowZoom: true,
        showTilesWhenEmpty: true,
        windowSelectObjects: true,
        windowSelectWires: true,
        windowSelectGroups: true,
        viewportDragging: default,
        actions: default,
        projection: default,
        clearSnapFeedback: false);
}

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

[ValueObject<float>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError<UiFault>]
public readonly partial struct PickTolerance {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref float value) =>
        validationError = float.IsFinite(value) && value >= 0f
            ? null
            : UiFault.Create(op: Op.Of(name: nameof(PickTolerance)), message: string.Create(CultureInfo.InvariantCulture, $"must be finite and >= 0 (got {value:R})."));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasActionSnapshot(bool AllowDrag, bool AllowWireSelect, bool AllowObjectSelect, bool AllowMakeWire, bool AllowDeleteWire, bool AllowModifyWire, bool HasMakeWireFilter, bool HasDeleteWireFilter, bool AllowMakeObject, bool AllowDeleteObject, bool AllowObjectResponse, bool AllowDropFile, bool AllowWireMenu, bool AllowObjectMenu, bool AllowCanvasMenu) {
    internal static CanvasActionSnapshot Of(CanvasActions actions) =>
        new(
            AllowDrag: actions.AllowDrag,
            AllowWireSelect: actions.AlloWireSelect,
            AllowObjectSelect: actions.AllowObjectSelect,
            AllowMakeWire: actions.AllowMakeWire,
            AllowDeleteWire: actions.AllowDeleteWire,
            AllowModifyWire: actions.AllowModifyWire,
            HasMakeWireFilter: actions.MakeWireFilter is not null,
            HasDeleteWireFilter: actions.DeleteWireFilter is not null,
            AllowMakeObject: actions.AllowMakeObject,
            AllowDeleteObject: actions.AllowDeleteObject,
            AllowObjectResponse: actions.AllowObjectResponse,
            AllowDropFile: actions.AllowDropFile,
            AllowWireMenu: actions.AllowWireMenu,
            AllowObjectMenu: actions.AllowObjectMenu,
            AllowCanvasMenu: actions.AllowCanvasMenu);

    internal CanvasActionPolicy ToPolicy() =>
        new(
            AllowDrag: Some(AllowDrag),
            AllowWireSelect: Some(AllowWireSelect),
            AllowObjectSelect: Some(AllowObjectSelect),
            AllowMakeWire: Some(AllowMakeWire),
            AllowDeleteWire: Some(AllowDeleteWire),
            AllowModifyWire: Some(AllowModifyWire),
            AllowMakeObject: Some(AllowMakeObject),
            AllowDeleteObject: Some(AllowDeleteObject),
            AllowObjectResponse: Some(AllowObjectResponse),
            AllowDropFile: Some(AllowDropFile),
            AllowWireMenu: Some(AllowWireMenu),
            AllowObjectMenu: Some(AllowObjectMenu),
            AllowCanvasMenu: Some(AllowCanvasMenu),
            MakeWireFilter: default,
            DeleteWireFilter: default);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasActionPolicy(Option<bool> AllowDrag = default, Option<bool> AllowWireSelect = default, Option<bool> AllowObjectSelect = default, Option<bool> AllowMakeWire = default, Option<bool> AllowDeleteWire = default, Option<bool> AllowModifyWire = default, Option<bool> AllowMakeObject = default, Option<bool> AllowDeleteObject = default, Option<bool> AllowObjectResponse = default, Option<bool> AllowDropFile = default, Option<bool> AllowWireMenu = default, Option<bool> AllowObjectMenu = default, Option<bool> AllowCanvasMenu = default, Option<Func<(IParameter source, IParameter target), bool>> MakeWireFilter = default, Option<Func<(IParameter source, IParameter target), bool>> DeleteWireFilter = default) {
    internal static CanvasActionPolicy Capture(CanvasActions actions) =>
        CanvasActionSnapshot.Of(actions: actions).ToPolicy() with {
            MakeWireFilter = Optional(actions.MakeWireFilter),
            DeleteWireFilter = Optional(actions.DeleteWireFilter),
        };

    internal Unit Apply(CanvasActions actions) {
        Seq<(Option<bool> Flag, Action<bool> Set)> toggles = [
            (AllowDrag, value => actions.AllowDrag = value),
            (AllowWireSelect, value => actions.AlloWireSelect = value),
            (AllowObjectSelect, value => actions.AllowObjectSelect = value),
            (AllowMakeWire, value => actions.AllowMakeWire = value),
            (AllowDeleteWire, value => actions.AllowDeleteWire = value),
            (AllowModifyWire, value => actions.AllowModifyWire = value),
            (AllowMakeObject, value => actions.AllowMakeObject = value),
            (AllowDeleteObject, value => actions.AllowDeleteObject = value),
            (AllowObjectResponse, value => actions.AllowObjectResponse = value),
            (AllowDropFile, value => actions.AllowDropFile = value),
            (AllowWireMenu, value => actions.AllowWireMenu = value),
            (AllowObjectMenu, value => actions.AllowObjectMenu = value),
            (AllowCanvasMenu, value => actions.AllowCanvasMenu = value),
        ];
        _ = toggles.Iter(row => row.Flag.Iter(row.Set));
        _ = MakeWireFilter.Iter(value => actions.MakeWireFilter = value);
        _ = DeleteWireFilter.Iter(value => actions.DeleteWireFilter = value);
        return unit;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasDetailSnapshot(bool ShowUndoHistory, float ZuiVariableParameterThreshold, float ZuiVariableParameterState, float ZuiWireDetailingThreshold, float ZuiWireDetailingState, CanvasActionSnapshot Actions);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasSnapFeedbackSnapshot(Option<SnappingSnapshot> X, Option<SnappingSnapshot> Y);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasBitmap(int Width, int Height, ReadOnlyMemory<byte> Png);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasMappedLocus(CanvasLocus Source, CanvasLocus Target, CoordinateSystem From, CoordinateSystem To);

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiPixelScale(float LogicalPixelSize, float PointsPerPixel, bool FromPaintGraphics);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasSnapshot(
    bool HasEditor,
    bool HasCanvas,
    bool HasDocument,
    RectangleF VisibleFrame,
    RectangleF ContentBounds,
    PointF ProjectionCentre,
    float ProjectionZoom,
    bool WindowSelectObjects,
    bool WindowSelectWires,
    bool WindowSelectGroups,
    float LogicalPixelSize = 1f,
    float PointsPerPixel = 1f);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasPickSnapshot(Pick Kind, Option<PointF> Point, int SelectedCount, int DeselectedCount, Option<WireEnds> WireUnderPick, Option<Guid> ObjectUnderPick, Option<Guid> InletUnderPick, Option<Guid> OutletUnderPick);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasWindowSnapshot(CanvasSnapshot Canvas, int SelectedCount, int DeselectedCount);

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class UiRail {
    // Quartz hard ceiling — beyond 16384×16384 the framework silently downsamples (Apple WWDC22
    // CoreGraphics session). Per-canvas effective max derives from the host window's screen pixel
    // dimensions; capped at the architectural ceiling. The static constant remains the upper bound,
    // RenderDimensionLimit() returns the device-respecting bound for any concrete Canvas.
    internal const int MaxRenderDimension = 16384;

    internal static int RenderDimensionLimit(GhCanvas canvas) =>
        Optional((canvas.ControlObject as Eto.Forms.Control)?.ParentWindow)
            .Filter(static window => window.LogicalPixelSize > 0f)
            .Map(window => {
                SizeF screenSize = window.Screen.Bounds.Size;
                int devicePixelMax = (int)MathF.Round(MathF.Max(screenSize.Width, screenSize.Height) * window.LogicalPixelSize);
                return Math.Min(MaxRenderDimension, devicePixelMax > 0 ? devicePixelMax : MaxRenderDimension);
            })
            .IfNone(MaxRenderDimension);

    // --- [OPERATIONS] -------------------------------------------------------------------------
    internal static Fin<CanvasResult> CanvasDispatch(GrasshopperUi.Scope scope, CanvasOp op) =>
        op.Switch(
            state: scope,
            snapshotCase: static (scope, _) =>
                from canvas in scope.NeedCanvas()
                from document in scope.NeedDocument()
                from objects in scope.NeedObjects()
                select (CanvasResult)new CanvasResult.SnapshotResult(
                    Snapshot: SnapshotOf(scope: scope, canvas: canvas, document: document, objects: objects)),
            pickCase: static (scope, p) =>
                Op.Of(name: nameof(CanvasOp.Pick)).AcceptPoint(value: p.Point, detail: "non-finite point")
                    .Bind(valid => PickAt(scope: scope, point: valid, source: p.Source, policy: p.Policy, tolerance: p.Tolerance)
                        .Map(pick => (CanvasResult)new CanvasResult.PickResult(Pick: pick))),
            mapCase: static (scope, m) =>
                Optional(m.Locus)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Map)), detail: "locus is required"))
                    .Bind(locus => scope.NeedCanvas().Bind(canvas => locus.Map(canvas: canvas, from: m.From, to: m.To)
                        .Map(mapped => (CanvasResult)new CanvasResult.MapResult(Mapped: new CanvasMappedLocus(Source: locus, Target: mapped, From: m.From, To: m.To))))),
            invalidateCase: static (scope, _) =>
                scope.NeedCanvas().Map(_ => CanvasResult.Unit),
            instantiateCase: static (scope, ins) =>
                scope.NeedCanvas()
                    .Bind(canvas => Optional(canvas.AllowedActions.AllowMakeObject)
                        .Filter(static allowed => allowed)
                        .ToFin(Fail: UiFault.MutationRejected(op: Op.Of(name: nameof(CanvasOp.Instantiate)), detail: "canvas disallows object creation"))
                        .Map(_ => {
                            canvas.ShowInstantiationPopup(mouseCentred: ins.MouseCentred, initialText: ins.SearchText.IfNone(string.Empty));
                            return CanvasResult.Unit;
                        })),
            focusCase: static (scope, _) =>
                scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.FocusResult(FocusedNomen: FocusNomenOf(canvas: canvas))),
            detailCase: static (scope, _) =>
                scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.DetailResult(
                    Detail: new CanvasDetailSnapshot(
                        ShowUndoHistory: canvas.ShowUndoHistory,
                        ZuiVariableParameterThreshold: canvas.ZuiVariableParameterThreshold,
                        ZuiVariableParameterState: canvas.ZuiVariableParameterState,
                        ZuiWireDetailingThreshold: canvas.ZuiWireDetailingThreshold,
                        ZuiWireDetailingState: canvas.ZuiWireDetailingState,
                        Actions: CanvasActionSnapshot.Of(actions: canvas.AllowedActions)))),
            actionsCase: static (scope, _) =>
                scope.NeedCanvas().Map(canvas => (CanvasResult)new CanvasResult.ActionsResult(Snapshot: CanvasActionSnapshot.Of(actions: canvas.AllowedActions))),
            renderCase: static (scope, r) =>
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
            pickMapCase: static (scope, _) =>
                scope.NeedCanvas().Bind(canvas => EncodeBitmap(bitmap: canvas.DrawPickMap(), width: 0, height: 0, op: Op.Of(name: nameof(CanvasOp.PickMap)))),
            viewCase: static (scope, v) =>
                ViewDispatch(scope: scope, view: v.Request),
            snapFeedbackCase: static (scope, f) =>
                scope.NeedCanvas().Map(canvas => {
                    _ = f.Clear ? ClearSnapFeedback(canvas: canvas) : unit;
                    return (CanvasResult)new CanvasResult.SnapFeedbackResult(Feedback: SnapFeedbackOf(canvas: canvas));
                }),
            interactionCase: static (scope, ic) =>
                from canvas in scope.NeedCanvas()
                let before = InteractionOf(canvas: canvas, document: scope.Document)
                from projected in ic.Policy.Projection.TraverseM(projection => ApplyProjection(scope: scope, policy: projection)).As()
                from applied in Op.Of(name: nameof(CanvasOp.Interaction)).Attempt(body: () => {
                    canvas.AllowPan = ic.Policy.AllowPan;
                    canvas.AllowZoom = ic.Policy.AllowZoom;
                    canvas.ShowTilesWhenEmpty = ic.Policy.ShowTilesWhenEmpty;
                    canvas.WindowSelectObjects = ic.Policy.WindowSelectObjects;
                    canvas.WindowSelectWires = ic.Policy.WindowSelectWires;
                    canvas.WindowSelectGroups = ic.Policy.WindowSelectGroups;
                    _ = projected;
                    _ = ic.Policy.ViewportDragging.Iter(value => canvas.ViewportDragging = value);
                    _ = ic.Policy.Actions.Iter(policy => policy.Apply(actions: canvas.AllowedActions));
                    _ = ic.Policy.ClearSnapFeedback ? ClearSnapFeedback(canvas: canvas) : unit;
                    return unit;
                }, what: "canvas interaction")
                select (CanvasResult)new CanvasResult.InteractionResult(
                    Interaction: new CanvasInteractionSnapshot(Before: before, After: InteractionOf(canvas: canvas, document: scope.Document))),
            windowSelectCase: static (scope, ws) =>
                scope.NeedObjects().Bind(objs => scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Map(doc => {
                    SelectionResult result = objs.WindowSelect(window: ws.Window, mode: ws.Mode,
                        considerForeground: (ws.Scope & CanvasWindowScope.Objects) == CanvasWindowScope.Objects,
                        considerBackground: (ws.Scope & CanvasWindowScope.Groups) == CanvasWindowScope.Groups,
                        considerWires: (ws.Scope & CanvasWindowScope.Wires) == CanvasWindowScope.Wires);
                    return (CanvasResult)new CanvasResult.WindowResult(Window: new CanvasWindowSnapshot(
                        Canvas: SnapshotOf(scope: scope, canvas: canvas, document: doc, objects: objs),
                        SelectedCount: result.SelectedCount,
                        DeselectedCount: result.DeselectedCount));
                }))));

    private readonly record struct ViewTarget(GhCanvas Canvas, GhDocument Document, GhObjectList Objects);

    private static Fin<ViewTarget> NeedViewTarget(GrasshopperUi.Scope scope) =>
        from canvas in scope.NeedCanvas()
        from document in scope.NeedDocument()
        from objects in scope.NeedObjects()
        select new ViewTarget(Canvas: canvas, Document: document, Objects: objects);

    private static Fin<CanvasResult> NavigateView(
        GrasshopperUi.Scope scope,
        CanvasViewPolicy rawPolicy,
        float defaultPadding,
        Op op,
        Func<ViewTarget, Fin<RectangleF>> frame) =>
        from target in NeedViewTarget(scope)
        let policy = ResolveView(raw: rawPolicy, defaultPadding: defaultPadding)
        from validPolicy in policy.MaximumZoom >= policy.MinimumZoom
            ? Fin.Succ(value: policy)
            : Fin.Fail<CanvasViewPolicy>(error: UiFault.InvalidInput(op: op, detail: "invalid zoom range"))
        from resolvedFrame in frame(arg: target)
        select (CanvasResult)NavigateTo(
            scope: scope,
            canvas: target.Canvas,
            frame: resolvedFrame,
            policy: validPolicy,
            document: target.Document,
            objects: target.Objects);

    private static Fin<CanvasResult> ViewDispatch(GrasshopperUi.Scope scope, CanvasViewOp view) =>
        view.Switch(
            state: scope,
            boundsCase: static (scope, n) =>
                from frame in Op.Of(name: nameof(CanvasViewOp.Bounds)).AcceptRect(value: n.Region, detail: "non-finite frame")
                from result in NavigateView(
                    scope: scope,
                    rawPolicy: n.Policy,
                    defaultPadding: CanvasViewPolicy.DefaultPadding,
                    op: Op.Of(name: nameof(CanvasViewOp.Bounds)),
                    frame: _ => Fin.Succ(frame))
                select result,
            selectionCase: static (scope, f) => {
                CanvasViewPolicy resolved = ResolveView(raw: f.Policy, defaultPadding: CanvasViewPolicy.SelectionFitPadding);
                return NavigateView(
                    scope: scope,
                    rawPolicy: f.Policy,
                    defaultPadding: CanvasViewPolicy.SelectionFitPadding,
                    op: Op.Of(name: nameof(CanvasViewOp.Selection)),
                    frame: target => {
                        IEnumerable<IDocumentObject> targets = f.Ids
                            .Map(list => list.Choose(id => Optional(target.Objects.Find(instanceId: id))))
                            .IfNone(toSeq(target.Objects.SelectedObjects));
                        return FrameOf(targets: targets)
                            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasViewOp.Selection)), detail: "no target objects"))
                            .Bind(aggregate => Op.Of(name: nameof(CanvasViewOp.Selection)).AcceptRect(
                                value: aggregate,
                                detail: "non-finite or zero-size aggregate",
                                requirePositive: true)
                                .Map(valid => RectangleF.Inflate(rectangle: valid, width: resolved.Padding, height: resolved.Padding)));
                    });
            },
            fitCase: static (scope, ft) =>
                NavigateView(
                    scope: scope,
                    rawPolicy: ft.Policy,
                    defaultPadding: CanvasViewPolicy.DefaultPadding,
                    op: Op.Of(name: nameof(CanvasViewOp.Fit)),
                    frame: target => ft.Target.Switch(
                        state: (target.Canvas, target.Objects),
                        contentCase: static (state, _) => Fin.Succ(state.Canvas.ContentBounds),
                        selectionCase: static (state, _) => FrameOf(targets: state.Objects.SelectedObjects)
                            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasViewOp.Fit)), detail: "no selected objects to fit")),
                        viewportCase: static (state, _) => Fin.Succ(state.Canvas.VisibleFrame))),
            positionCase: static (scope, pos) =>
                from target in NeedViewTarget(scope)
                from _ in Op.Of(name: nameof(CanvasViewOp.Position)).Attempt(
                    body: () => { target.Canvas.Navigate(position: pos.Where, duration: pos.Duration); return unit; },
                    what: "FlexControl.Navigate(ContentPosition)")
                select (CanvasResult)new CanvasResult.SnapshotResult(
                    Snapshot: SnapshotOf(scope: scope, canvas: target.Canvas, document: target.Document, objects: target.Objects)),
            projectionCase: static (scope, pr) =>
                from zoom in Op.Of(name: nameof(CanvasViewOp.Projection)).Attempt(
                    body: () => ZoomFactor.Create(pr.Zoom),
                    what: nameof(ZoomFactor))
                from _ in ApplyProjection(scope: scope, policy: new CanvasProjectionPolicy(Centre: Some(pr.Centre), Zoom: Some(zoom.Value)))
                from target in NeedViewTarget(scope)
                select (CanvasResult)new CanvasResult.SnapshotResult(
                    Snapshot: SnapshotOf(scope: scope, canvas: target.Canvas, document: target.Document, objects: target.Objects)));

    private static Fin<Unit> ApplyProjection(GrasshopperUi.Scope scope, CanvasProjectionPolicy policy) =>
        from document in scope.NeedDocument()
        from canvas in scope.NeedCanvas()
        let centre = policy.Centre.IfNone(document.Projection.centre)
        from zoom in policy.Zoom.Match(
            Some: value => Op.Of(name: nameof(CanvasViewOp.Projection)).Attempt(body: () => ZoomFactor.Create(value), what: nameof(ZoomFactor)),
            None: () => Op.Of(name: nameof(CanvasViewOp.Projection)).Attempt(
                body: () => ZoomFactor.Create(document.Projection.zoom),
                what: nameof(ZoomFactor)))
        from validCentre in Op.Of(name: nameof(CanvasViewOp.Projection)).AcceptPoint(value: centre, detail: "non-finite centre")
        from _ in Try.lift(f: () => {
            canvas.Projection = canvas.Projection.SetZoom(zoom: zoom.Value).SetCentre(centre: validCentre, frame: canvas.VisibleFrame);
            document.Projection = (validCentre, zoom.Value);
            return unit;
        }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(CanvasViewOp.Projection)), detail: $"projection assign threw: {error.Message}"))
        select unit;

    private static CanvasViewPolicy ResolveView(CanvasViewPolicy raw, float defaultPadding = CanvasViewPolicy.DefaultPadding) {
        TimeSpan duration = raw.Duration == default ? Animators.DurationToTimeSpan(duration: GhDuration.Normal) : raw.Duration;
        float minimum = float.IsFinite(raw.MinimumZoom) && raw.MinimumZoom > 0f ? raw.MinimumZoom : CanvasViewPolicy.DefaultMinimumZoom;
        float maximum = float.IsFinite(raw.MaximumZoom) && raw.MaximumZoom >= minimum ? raw.MaximumZoom : Math.Max(minimum, CanvasViewPolicy.DefaultMaximumZoom);
        float padding = float.IsFinite(raw.Padding) ? raw.Padding : defaultPadding;
        return new(MinimumZoom: minimum, MaximumZoom: maximum, Duration: duration, Padding: padding);
    }

    private static CanvasResult.SnapshotResult NavigateTo(GrasshopperUi.Scope scope, GhCanvas canvas, RectangleF frame, CanvasViewPolicy policy, GhDocument document, GhObjectList objects) {
        canvas.Navigate(frame: frame, zoomLimits: (policy.MinimumZoom, policy.MaximumZoom), duration: policy.Duration);
        return new CanvasResult.SnapshotResult(Snapshot: SnapshotOf(scope: scope, canvas: canvas, document: document, objects: objects));
    }

    internal static UiPixelScale PixelScale(GhCanvas canvas, Option<ControlGraphics> graphics = default) =>
        graphics
            .Filter(static g => g.Content.PointsPerPixel > 0f)
            .Map(static g => new UiPixelScale(
                LogicalPixelSize: 1f / g.Content.PointsPerPixel,
                PointsPerPixel: g.Content.PointsPerPixel,
                FromPaintGraphics: true))
            .IfNone(() => {
                float logicalPixelSize = Optional(canvas.ControlObject as Eto.Forms.Control)
                    .Bind(static control => Optional(control.ParentWindow))
                    .Map(static window => window.LogicalPixelSize)
                    .Filter(static value => value > 0f)
                    .IfNone(1f);
                return new UiPixelScale(
                    LogicalPixelSize: logicalPixelSize,
                    PointsPerPixel: logicalPixelSize > 0f ? 1f / logicalPixelSize : 1f,
                    FromPaintGraphics: false);
            });

    internal static CanvasSnapshot SnapshotOf(GrasshopperUi.Scope scope, GhCanvas canvas, GhDocument document, GhObjectList objects) {
        UiPixelScale pixelScale = PixelScale(canvas: canvas);
        return new(
            HasEditor: scope.Editor.IsSome,
            HasCanvas: scope.Canvas.IsSome,
            HasDocument: scope.Document.IsSome,
            VisibleFrame: canvas.VisibleFrame,
            ContentBounds: canvas.ContentBounds,
            ProjectionCentre: document.Projection.centre,
            ProjectionZoom: document.Projection.zoom,
            WindowSelectObjects: canvas.WindowSelectObjects,
            WindowSelectWires: canvas.WindowSelectWires,
            WindowSelectGroups: canvas.WindowSelectGroups,
            LogicalPixelSize: pixelScale.LogicalPixelSize,
            PointsPerPixel: pixelScale.PointsPerPixel);
    }

    internal static Fin<CanvasPickSnapshot> PickAt(GrasshopperUi.Scope scope, PointF point, CoordinateSystem source, CanvasPickPolicy policy, PickTolerance tolerance) =>
        scope.NeedCanvas().Map(canvas => {
            PointF content = source == CoordinateSystem.Content ? point : canvas.Map(point: point, from: source, to: CoordinateSystem.Content);
            RectangleF pickBounds = tolerance.Value <= 0f
                ? new RectangleF(x: content.X, y: content.Y, width: 0f, height: 0f)
                : RectangleF.Inflate(
                    rectangle: new RectangleF(x: content.X, y: content.Y, width: 0f, height: 0f),
                    width: tolerance.Value,
                    height: tolerance.Value);
            SelectionResult result = canvas.ResolvePick(
                point: new PointF(
                    x: pickBounds.X + (pickBounds.Width * 0.5f),
                    y: pickBounds.Y + (pickBounds.Height * 0.5f)),
                includeGrips: (policy & CanvasPickPolicy.Grips) == CanvasPickPolicy.Grips,
                includeForeground: (policy & CanvasPickPolicy.Foreground) == CanvasPickPolicy.Foreground,
                includeBackground: (policy & CanvasPickPolicy.Background) == CanvasPickPolicy.Background,
                includeWires: (policy & CanvasPickPolicy.Wires) == CanvasPickPolicy.Wires,
                recursive: (policy & CanvasPickPolicy.Recursive) == CanvasPickPolicy.Recursive);
            return PickSnapshotOf(result: result);
        });

    internal static Option<string> FocusNomenOf(GhCanvas canvas) =>
        Optional(canvas.FocusObject).Map(static target => target switch {
            IInteraction interaction => interaction.Nomen.Name,
            _ => target.GetType().Name,
        });

    private static CanvasInteractionPolicy InteractionOf(GhCanvas canvas, Option<GhDocument> document = default) =>
        CanvasInteractionPolicy.Create(
            allowPan: canvas.AllowPan,
            allowZoom: canvas.AllowZoom,
            showTilesWhenEmpty: canvas.ShowTilesWhenEmpty,
            windowSelectObjects: canvas.WindowSelectObjects,
            windowSelectWires: canvas.WindowSelectWires,
            windowSelectGroups: canvas.WindowSelectGroups,
            viewportDragging: Some(canvas.ViewportDragging),
            actions: Some(CanvasActionPolicy.Capture(actions: canvas.AllowedActions)),
            projection: document.Map(static doc => new CanvasProjectionPolicy(
                Centre: Some(doc.Projection.centre),
                Zoom: Some(doc.Projection.zoom))),
            clearSnapFeedback: false);

    private static Unit ClearSnapFeedback(GhCanvas canvas) {
        canvas.SnapXAction = null;
        canvas.SnapYAction = null;
        return unit;
    }

    private static CanvasSnapFeedbackSnapshot SnapFeedbackOf(GhCanvas canvas) =>
        new(X: SnapChannels(x: Optional(canvas.SnapXAction), y: Option<SnappingAction>.None), Y: SnapChannels(x: Option<SnappingAction>.None, y: Optional(canvas.SnapYAction)));

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

    internal static Option<SnappingSnapshot> SnapChannels(Option<SnappingAction> x, Option<SnappingAction> y) =>
        Optional((X: x, Y: y))
            .Filter(static snap => snap.X.IsSome || snap.Y.IsSome)
            .Map(static snap => new SnappingSnapshot(
                Dx: snap.X.Map(static action => action.ΔX).IfNone(0f),
                Dy: snap.Y.Map(static action => action.ΔY).IfNone(0f),
                Magnitude: snap.X.Map(static action => action.Magnitude).IfNone(0f) + snap.Y.Map(static action => action.Magnitude).IfNone(0f),
                XLabel: snap.X.Map(static action => action.LabelText).IfNone(string.Empty),
                YLabel: snap.Y.Map(static action => action.LabelText).IfNone(string.Empty),
                Lines: snap.X.Map(static action => toSeq(action.Lines)).IfNone(Seq<LineF>()) + snap.Y.Map(static action => toSeq(action.Lines)).IfNone(Seq<LineF>()),
                LabelPoint: snap.X.Map(static action => action.LabelPoint) | snap.Y.Map(static action => action.LabelPoint),
                LabelAnchor: snap.X.Map(static action => action.LabelAnchor) | snap.Y.Map(static action => action.LabelAnchor)));

    internal static Fin<int> SelectionDispatch(GhDocumentMethods methods, SelectionOp op) =>
        op.Switch(
            state: methods,
            allCase: static (m, _) => Fin.Succ(value: m.SelectAll()),
            noneCase: static (m, _) => Fin.Succ(value: m.DeselectAll()),
            invertCase: static (m, _) => Fin.Succ(value: m.InvertSelection()),
            growCase: static (m, g) => Fin.Succ(value: m.GrowSelection(upstream: g.Upstream, downstream: g.Downstream)),
            shiftCase: static (m, s) => Fin.Succ(value: m.ShiftSelection(upstream: s.Upstream)));

    internal static Fin<int> ClipboardDispatch(GhDocumentMethods methods, ClipboardOp op, ActionList actions) =>
        op.Switch(
            state: (methods, actions),
            verbCase: static (s, op) => op.Verb.Run(methods: s.methods, kind: op.Kind, behaviour: op.Behaviour, actions: s.actions));

    internal static Fin<Option<Guid>> ComposeDispatch(GhDocumentMethods methods, GhObjectList objects, ObjectScope subject, ComposeOp op, ActionList actions) =>
        from _ in subject.RequireMutationScope(op: Op.Of(name: nameof(ComposeDispatch)), use: ScopeUse.Compose)
        from created in subject.Switch(
            state: (methods, objects, op, actions),
            selectionCase: static (s, _) => s.op.Apply(methods: s.methods, objects: s.objects, targets: null, actions: s.actions),
            objectsCase: static (s, selected) => selected.Ids
                .TraverseM(id => ResolveObject(objects: s.objects, id: id, op: Op.Of(name: nameof(ObjectScope.Objects))))
                .As()
                .Map(static resolved => resolved.ToArray())
                .Bind(targets => s.op.Apply(methods: s.methods, objects: s.objects, targets: targets, actions: s.actions)),
            primaryCase: static (_, _) => Fin.Fail<Option<Guid>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ComposeDispatch)), detail: ScopeUse.Compose.RejectsPrimaryDetail())),
            primaryAndSecondaryCase: static (_, _) => Fin.Fail<Option<Guid>>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ComposeDispatch)), detail: ScopeUse.Compose.RejectsPrimaryDetail())))
        select created;

    internal static Fin<IDocumentObject> ResolveObject(GhObjectList objects, Guid id, Op op) =>
        op.AcceptValue(value: id)
            .MapFail(_ => UiFault.InvalidInput(op: op, detail: "empty Guid"))
            .Bind(valid => Optional(objects.Find(instanceId: valid))
                .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"object {valid} not found")));

    internal static Fin<IDocumentObject> ResolveObject(GrasshopperUi.Scope scope, Guid id, Op op) =>
        scope.NeedObjects().Bind(objs => ResolveObject(objects: objs, id: id, op: op));

    internal static int RunDelete(GhDocumentMethods methods, IDocumentObject[]? targets, bool dataOnly, Seq<WireEnds> wires, ActionList actions) =>
        DocumentDeleteMode.Resolve(targets: targets, dataOnly: dataOnly, wires: wires).Run(methods: methods, targets: targets, wires: wires, actions: actions);

    internal static Fin<ClipboardKind> ValidateClipboard(string name, ClipboardKind clipboard) =>
        clipboard switch {
            ClipboardKind.Instance => Fin.Fail<ClipboardKind>(error: UiFault.InvalidInput(op: Op.Of(name: name), detail: "ClipboardKind.Instance is not supported")),
            _ => Fin.Succ(value: clipboard),
        };

    internal static Fin<Unit> PreflightCompose(Op op, Func<(bool Ok, string WhyNot)> check) =>
        check() switch {
            (true, _) => Fin.Succ(value: unit),
            (false, string whyNot) => Fin.Fail<Unit>(error: UiFault.MutationRejected(op: op, detail: string.IsNullOrWhiteSpace(value: whyNot) ? "pre-flight rejected" : whyNot)),
        };

    internal static DocumentHistorySnapshot HistorySnapshotOf(GhDocument document) =>
        new(
            IsEmpty: document.Undo.IsEmpty,
            CanUndo: document.Undo.FirstUndo is not null,
            CanRedo: document.Undo.FirstRedo is not null,
            UndoCount: document.Undo.CentralUndoSequence.Count(),
            RedoCount: document.Undo.CentralRedoSequence.Count());

    internal static Fin<Snapshot<DocumentMutationDelta>> RunMutation(
        GrasshopperUi.Scope scope,
        Op op,
        Func<GhDocumentMethods, GhObjectList, ActionList, Fin<DocumentMutationReceipt>> mutate,
        DocumentMutationPolicy policy) =>
        from methods in scope.NeedMethods()
        from document in scope.NeedDocument()
        from objects in scope.NeedObjects()
        let actions = ActionList.Empty
        from receipt in mutate(arg1: methods, arg2: objects, arg3: actions).Bind(result => result.Changed switch {
            >= 0 => Fin.Succ(value: result),
            _ => Fin.Fail<DocumentMutationReceipt>(error: UiFault.MutationRejected(op: op, detail: string.Create(CultureInfo.InvariantCulture, $"count={result.Changed}"))),
        })
        from _ in CommitActions(document: document, name: VerbNounOf(op: op), actions: actions)
        select Snapshot.Of(
            payload: new DocumentMutationDelta(Changed: receipt.Changed, After: DocumentSnapshotOf(document: document, objects: objects), Created: receipt.Created),
            ownerId: Some(document.Hash));

    internal static Fin<Unit> CommitActions(GhDocument document, VerbNoun name, ActionList actions) =>
        actions.Count switch {
            <= 0 => Fin.Succ(value: unit),
            _ => Try.lift(f: () => {
                document.Undo.Do(name: name, actions: actions);
                return unit;
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(CommitActions)), detail: $"History.Do threw: {error.Message}")),
        };

    internal static Fin<Unit> CommitActions(GhDocument document, Op op, ActionList actions) =>
        CommitActions(document: document, name: VerbNounOf(op: op), actions: actions);

    internal static VerbNoun VerbNounOf(Op op) {
        string name = op.ToString();
        int dot = name.IndexOf(value: '.', comparisonType: StringComparison.Ordinal);
        return dot > 0 && dot < name.Length - 1
            ? (Verb: name[(dot + 1)..], Noun: name[..dot])
            : (Verb: "Edit", Noun: name);
    }

    private static Seq<WireEnds> WiresOrEmpty(IEnumerable<WireEnds>? wires) =>
        Optional(wires).Map(static w => toSeq(w)).IfNone(Seq<WireEnds>());

    internal static DocumentSnapshot DocumentSnapshotOf(GhDocument document, GhObjectList objects) {
        Seq<WireEnds> allWires = WiresOrEmpty(wires: objects.AllWires);
        Seq<WireEnds> selectedWires = WiresOrEmpty(wires: objects.SelectedWires);
        int wireCount = allWires.Count(wire => Wire.IsConnected(objects: objects, wire: wire));
        int selectedWireCount = selectedWires.Count(wire => Wire.IsConnected(objects: objects, wire: wire));
        return new DocumentSnapshot(
            Hash: document.Hash, Modified: document.Modified, Modifications: document.Modifications,
            ObjectCount: objects.Count, PinCount: objects.PinCount, ExpiredCount: objects.ExpiredCount,
            SelectedObjectCount: objects.SelectedCount, SelectedWireCount: selectedWireCount,
            SelectedDanglingWireCount: selectedWires.Count - selectedWireCount,
            WireCount: wireCount, DanglingWireCount: allWires.Count - wireCount,
            AttributeBounds: objects.AttributeBounds, PivotBounds: objects.PivotBounds,
            ProjectionCentre: document.Projection.centre, ProjectionZoom: document.Projection.zoom);
    }

    internal static DocumentObjectSnapshot DocumentObjectSnapshotOf(IDocumentObject obj) =>
        new(Id: obj.InstanceId, Name: obj.Nomen.Name, DisplayName: obj.DisplayName,
            Selected: obj.Selected, Activity: string.Create(CultureInfo.InvariantCulture, $"{obj.Activity}"),
            Display: string.Create(CultureInfo.InvariantCulture, $"{obj.Display}"), Phase: string.Create(CultureInfo.InvariantCulture, $"{obj.Phase}"), State: string.Create(CultureInfo.InvariantCulture, $"{obj.State}"),
            Bounds: obj.Attributes.Bounds, Pivot: obj.Attributes.Pivot);

}
