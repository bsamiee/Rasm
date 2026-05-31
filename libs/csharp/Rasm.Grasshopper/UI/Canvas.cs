using System.Globalization;
using System.Reflection;
using Eto.Forms;
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

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasWindow(PointF Start, PointF End) {
    public static CanvasWindow FromRect(RectangleF rect) =>
        new(Start: new PointF(x: rect.Left, y: rect.Top), End: new PointF(x: rect.Right, y: rect.Bottom));

    internal Fin<WindowSelection> Selection(Op op) {
        PointF start = Start;
        PointF end = End;
        return op.AcceptPoint(value: start, detail: "non-finite window start")
            .Bind(validStart => op.AcceptPoint(value: end, detail: "non-finite window end")
                .Map(validEnd => new WindowSelection(startPoint: validStart).Adapt(newEndPoint: validEnd)));
    }
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
    public sealed partial record RenderCase(CanvasRenderPolicy Policy) : CanvasOp;
    public sealed partial record PickMapCase : CanvasOp;
    public sealed partial record ViewCase(CanvasViewOp Request) : CanvasOp;
    public sealed partial record SnapFeedbackCase(bool Clear) : CanvasOp;
    public sealed partial record InteractionCase(CanvasInteractionPolicy Policy) : CanvasOp;
    public sealed partial record WindowSelectCase(CanvasWindow Window, SelectionMode Mode, CanvasWindowScope Scope) : CanvasOp;
    public sealed partial record InlineEditCase(RectangleF Frame, string Initial, Func<string, Fin<Unit>> Apply, Option<Func<Fin<Unit>>> Cancel) : CanvasOp;
    public sealed partial record ValueEditorCase(AbstractParameter Parameter, Control Control) : CanvasOp;

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
    public static CanvasOp Render(CanvasRenderPolicy policy) => new RenderCase(Policy: policy);
    public static CanvasOp Render(int width, int height, CanvasBitmapLayers layers = CanvasBitmapLayers.All, DrawPlan overlay = default) =>
        Render(policy: new CanvasRenderPolicy(Width: width, Height: height, Layers: Some(layers), Overlay: overlay));
    public static readonly CanvasOp PickMap = new PickMapCase();
    public static CanvasOp View(CanvasViewOp view) => new ViewCase(Request: view);
    public static CanvasOp SnapFeedback(bool clear = false) => new SnapFeedbackCase(Clear: clear);
    public static CanvasOp Interaction(CanvasInteractionPolicy policy) => new InteractionCase(Policy: policy);
    public static CanvasOp WindowSelect(CanvasWindow window, SelectionMode mode, CanvasWindowScope scope = CanvasWindowScope.ObjectsAndWires) =>
        new WindowSelectCase(Window: window, Mode: mode, Scope: scope);
    public static CanvasOp WindowSelect(RectangleF window, SelectionMode mode, CanvasWindowScope scope = CanvasWindowScope.ObjectsAndWires) =>
        WindowSelect(window: CanvasWindow.FromRect(rect: window), mode: mode, scope: scope);
    public static CanvasOp InlineEdit(RectangleF frame, string initial, Func<string, Fin<Unit>> apply, Func<Fin<Unit>>? cancel = null) =>
        new InlineEditCase(Frame: frame, Initial: initial, Apply: apply, Cancel: Optional(cancel));
    public static CanvasOp ValueEditor(AbstractParameter parameter, Control control) =>
        new ValueEditorCase(Parameter: parameter, Control: control);

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
        windowSelectCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        inlineEditCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
        valueEditorCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true));
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
        Option<CanvasProjectionPolicy> proj = projection;
        UiFault? fault = null;
        _ = op.AcceptAll(
                value: unit,
                o => proj.Bind(static p => p.Centre) is { IsSome: true, Case: PointF centre }
                    ? o.AcceptPoint(value: centre, detail: "Projection.Centre must be finite.").Map(static _ => unit)
                    : Fin.Succ(unit),
                o => proj.Bind(static p => p.Zoom) is { IsSome: true, Case: float zoom }
                    ? o.AcceptFinite(value: zoom, detail: "Projection.Zoom must be finite and > 0.", requirePositive: true).Map(static _ => unit)
                    : Fin.Succ(unit))
            .IfFail(err => { fault = (UiFault)err; return unit; });
        validationError = fault;
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
public readonly record struct CanvasRenderPolicy(
    int Width,
    int Height,
    Option<CanvasBitmapLayers> Layers = default,
    DrawPlan Overlay = default) {
    internal CanvasBitmapLayers EffectiveLayers => Layers.IfNone(CanvasBitmapLayers.All);

    public static CanvasRenderPolicy operator |(CanvasRenderPolicy left, CanvasRenderPolicy right) =>
        new(
            Width: right.Width > 0 ? right.Width : left.Width,
            Height: right.Height > 0 ? right.Height : left.Height,
            Layers: right.Layers.IsSome ? right.Layers : left.Layers,
            Overlay: left.Overlay + right.Overlay);
}

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
    // Quartz silently downsamples beyond 16384² (WWDC22 CoreGraphics); RenderDimensionLimit derives the
    // device-respecting bound per canvas, this constant is the architectural ceiling.
    internal const int MaxRenderDimension = 16384;

    // Single ParentWindow-chain owner: RenderDimensionLimit and PixelScale both read logical pixel size and
    // screen extent through this projector instead of duplicating the as-Control / ParentWindow / Screen walk.
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct WindowMetrics(float LogicalPixelSize, SizeF ScreenSize);

    private static Option<WindowMetrics> WindowMetricsOf(GhCanvas canvas) =>
        Optional((canvas.ControlObject as Control)?.ParentWindow)
            .Filter(static w => w.LogicalPixelSize > 0f)
            .Map(static w => new WindowMetrics(LogicalPixelSize: w.LogicalPixelSize, ScreenSize: w.Screen.Bounds.Size));

    internal static int RenderDimensionLimit(GhCanvas canvas) =>
        WindowMetricsOf(canvas: canvas)
            .Map(static m => {
                int devicePixelMax = (int)MathF.Round(MathF.Max(m.ScreenSize.Width, m.ScreenSize.Height) * m.LogicalPixelSize, MidpointRounding.ToEven);
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
                    CanvasRenderPolicy policy = r.Policy;
                    return Optional(policy)
                        .Filter(static p => p.Width > 0)
                        .Filter(p => p.Width <= limit && p.Height > 0 && p.Height <= limit)
                        .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasOp.Render)), detail: $"dimensions out of [1, {limit}]"))
                        .Bind(valid =>
                            from bitmap in RenderBitmap(canvas: canvas, policy: valid)
                            from encoded in EncodeBitmap(
                                bitmap: bitmap,
                                width: valid.Width,
                                height: valid.Height,
                                op: Op.Of(name: nameof(CanvasOp.Render)))
                            select encoded);
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
                ws.Window.Selection(op: Op.Of(name: nameof(CanvasOp.WindowSelect)))
                    .Bind(selection => scope.NeedObjects().Bind(objs => scope.NeedCanvas().Bind(canvas => scope.NeedDocument().Bind(doc =>
                        from result in Op.Of(name: nameof(CanvasOp.WindowSelect)).Attempt(
                            body: () => objs.WindowSelect(
                                window: selection,
                                mode: ws.Mode.Gh,
                                considerForeground: (ws.Scope & CanvasWindowScope.Objects) == CanvasWindowScope.Objects,
                                considerBackground: (ws.Scope & CanvasWindowScope.Groups) == CanvasWindowScope.Groups,
                                considerWires: (ws.Scope & CanvasWindowScope.Wires) == CanvasWindowScope.Wires),
                            what: "ObjectList.WindowSelect")
                        select (CanvasResult)new CanvasResult.WindowResult(Window: new CanvasWindowSnapshot(
                            Canvas: SnapshotOf(scope: scope, canvas: canvas, document: doc, objects: objs),
                            SelectedCount: result.SelectedCount,
                            DeselectedCount: result.DeselectedCount)))))),
            inlineEditCase: static (scope, edit) =>
                from canvas in scope.NeedCanvas()
                from frame in Op.Of(name: nameof(CanvasOp.InlineEdit)).AcceptRect(value: edit.Frame, detail: "invalid inline editor frame", requirePositive: true)
                from apply in Optional(edit.Apply).ToFin(Fail: UiFault.InvalidInput(op: CanvasOp.InlineEditCase.SelfOp, detail: "apply callback is required"))
                from _ in Op.Of(name: nameof(CanvasOp.InlineEdit)).Attempt(body: () => {
                    System.Action? cancel = edit.Cancel is { IsSome: true, Case: Func<Fin<Unit>> run }
                        ? () => GrasshopperUi.Handler(valid: run).Ignore()
                        : null;
                    canvas.ShowInlineEditor(frame: frame, initial: edit.Initial ?? string.Empty, apply: text => InlineResult(text: text, apply: apply), cancel: cancel);
                    return unit;
                }, what: "Canvas.ShowInlineEditor")
                select CanvasResult.Unit,
            valueEditorCase: static (scope, edit) =>
                from canvas in scope.NeedCanvas()
                from parameter in Optional(edit.Parameter).ToFin(Fail: UiFault.InvalidInput(op: CanvasOp.ValueEditorCase.SelfOp, detail: "parameter is required"))
                from control in Optional(edit.Control).ToFin(Fail: UiFault.InvalidInput(op: CanvasOp.ValueEditorCase.SelfOp, detail: "control is required"))
                from _ in ShowValueEditor(canvas: canvas, parameter: parameter, control: control)
                select CanvasResult.Unit);

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
            selectionCase: static (scope, f) =>
                NavigateView(
                    scope: scope,
                    rawPolicy: f.Policy,
                    defaultPadding: CanvasViewPolicy.SelectionFitPadding,
                    op: Op.Of(name: nameof(CanvasViewOp.Selection)),
                    frame: target => {
                        float padding = ResolveView(raw: f.Policy, defaultPadding: CanvasViewPolicy.SelectionFitPadding).Padding;
                        Fin<Seq<IDocumentObject>> targets = f.Ids
                            .Map(list => list.TraverseM(id => ResolveObject(objects: target.Objects, id: id, op: Op.Of(name: nameof(CanvasViewOp.Selection)))).As())
                            .IfNone(Fin.Succ(value: toSeq(target.Objects.SelectedObjects)));
                        return targets.Bind(resolved => FrameOf(targets: resolved)
                            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(CanvasViewOp.Selection)), detail: "no target objects"))
                            .Bind(aggregate => Op.Of(name: nameof(CanvasViewOp.Selection)).AcceptRect(
                                value: aggregate,
                                detail: "non-finite or zero-size aggregate",
                                requirePositive: true)
                                .Map(valid => RectangleF.Inflate(rectangle: valid, width: padding, height: padding))));
                    }),
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
                from _ in NavigatePosition(target: target, position: pos.Where, duration: pos.Duration)
                select (CanvasResult)new CanvasResult.SnapshotResult(
                    Snapshot: SnapshotOf(scope: scope, canvas: target.Canvas, document: target.Document, objects: target.Objects)),
            projectionCase: static (scope, pr) =>
                from _ in ApplyProjection(scope: scope, policy: new CanvasProjectionPolicy(Centre: Some(pr.Centre), Zoom: Some(pr.Zoom)))
                from target in NeedViewTarget(scope)
                select (CanvasResult)new CanvasResult.SnapshotResult(
                    Snapshot: SnapshotOf(scope: scope, canvas: target.Canvas, document: target.Document, objects: target.Objects)));

    // Sole ZoomFactor admission across all three callers: the policy's zoom defaults to the live document
    // zoom before the single Create, so projectionCase passes its raw zoom straight in (no pre-validation).
    private static Fin<Unit> ApplyProjection(GrasshopperUi.Scope scope, CanvasProjectionPolicy policy) =>
        from document in scope.NeedDocument()
        from canvas in scope.NeedCanvas()
        let centre = policy.Centre.IfNone(document.Projection.centre)
        from zoom in Op.Of(name: nameof(CanvasViewOp.Projection)).Attempt(
            body: () => ZoomFactor.Create(policy.Zoom.IfNone(document.Projection.zoom)),
            what: nameof(ZoomFactor))
        from validCentre in Op.Of(name: nameof(CanvasViewOp.Projection)).AcceptPoint(value: centre, detail: "non-finite centre")
        from _ in Try.lift(f: () => {
            // Invariant: canvas.Projection (live FlexControl state) is the source of truth; document.Projection is
            // GH2's cached copy. A throw between the two assigns leaves the cache stale, but the next navigate/paint
            // re-syncs from the canvas — the partial state self-heals rather than corrupting the view.
            canvas.Projection = canvas.Projection.SetZoom(zoom: zoom.Value).SetCentre(centre: validCentre, frame: canvas.VisibleFrame);
            document.Projection = (validCentre, zoom.Value);
            return unit;
        }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(CanvasViewOp.Projection)), detail: $"projection assign threw: {error.Message}"))
        select unit;

    private static Fin<Unit> NavigatePosition(ViewTarget target, ContentPosition position, GhDuration duration) =>
        Op.Of(name: nameof(CanvasViewOp.Position)).Attempt(body: () => {
            RectangleF content = target.Canvas.ContentBounds.IsEmpty ? target.Canvas.VisibleFrame : target.Canvas.ContentBounds;
            RectangleF frame = target.Canvas.VisibleFrame;
            RectangleF region = position switch {
                ContentPosition.Top => new RectangleF(x: frame.X, y: content.Top, width: frame.Width, height: frame.Height),
                ContentPosition.Bottom => new RectangleF(x: frame.X, y: content.Bottom - frame.Height, width: frame.Width, height: frame.Height),
                ContentPosition.Left => new RectangleF(x: content.Left, y: frame.Y, width: frame.Width, height: frame.Height),
                ContentPosition.Right => new RectangleF(x: content.Right - frame.Width, y: frame.Y, width: frame.Width, height: frame.Height),
                ContentPosition.Centre => RectangleF.FromCenter(center: content.Center, size: frame.Size),
                ContentPosition.TopLeft => new RectangleF(x: content.Left, y: content.Top, width: frame.Width, height: frame.Height),
                ContentPosition.TopRight => new RectangleF(x: content.Right - frame.Width, y: content.Top, width: frame.Width, height: frame.Height),
                ContentPosition.BottomLeft => new RectangleF(x: content.Left, y: content.Bottom - frame.Height, width: frame.Width, height: frame.Height),
                ContentPosition.BottomRight => new RectangleF(x: content.Right - frame.Width, y: content.Bottom - frame.Height, width: frame.Width, height: frame.Height),
                ContentPosition.Fit => content,
                ContentPosition.HundredPercent => RectangleF.FromCenter(center: frame.Center, size: target.Canvas.InnerBounds.Size),
                _ => frame,
            };
            target.Canvas.Navigate(
                frame: region,
                zoomLimits: (CanvasViewPolicy.DefaultMinimumZoom, position == ContentPosition.Fit ? 1f : CanvasViewPolicy.DefaultMaximumZoom),
                duration: duration);
            return unit;
        }, what: "FlexControl.Navigate(RectangleF)");

    internal static CanvasViewPolicy ResolveView(CanvasViewPolicy raw, float defaultPadding = CanvasViewPolicy.DefaultPadding) {
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
            .Filter(static g => g.Content.PointsPerPixel > 0f || g.ScreenScale > 0f)
            .Map(g => new UiPixelScale(
                LogicalPixelSize: g.ScreenScale > 0f ? g.ScreenScale : WindowMetricsOf(canvas: canvas).Map(static m => m.LogicalPixelSize).IfNone(1f),
                PointsPerPixel: g.Content.PointsPerPixel > 0f ? g.Content.PointsPerPixel : 1f,
                FromPaintGraphics: true))
            .IfNone(() => {
                float logicalPixelSize = WindowMetricsOf(canvas: canvas).Map(static m => m.LogicalPixelSize).IfNone(1f);
                return new UiPixelScale(
                    LogicalPixelSize: logicalPixelSize,
                    PointsPerPixel: 1f,
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
            float radius = tolerance.Value;
            Seq<PointF> probes = radius <= 0f
                ? Seq(content)
                : ProbeLattice(centre: content, radius: radius);
            SelectionResult Resolve(PointF probe) => canvas.ResolvePick(
                point: probe,
                includeGrips: (policy & CanvasPickPolicy.Grips) == CanvasPickPolicy.Grips,
                includeForeground: (policy & CanvasPickPolicy.Foreground) == CanvasPickPolicy.Foreground,
                includeBackground: (policy & CanvasPickPolicy.Background) == CanvasPickPolicy.Background,
                includeWires: (policy & CanvasPickPolicy.Wires) == CanvasPickPolicy.Wires,
                recursive: (policy & CanvasPickPolicy.Recursive) == CanvasPickPolicy.Recursive);
            return probes
                .Map(Resolve)
                .Find(static pick => pick.Kind != Pick.None)
                .Map(PickSnapshotOf)
                .IfNone(PickSnapshotOf(result: Resolve(probe: content)));
        });

    // Only an IInteraction carries a domain Nomen; a non-interaction focus projects to None rather than
    // leaking its CLR type name (native-sentinel -> Option discipline).
    internal static Option<string> FocusNomenOf(GhCanvas canvas) =>
        Optional(canvas.FocusObject).Bind(static target => target switch {
            IInteraction interaction => Optional(interaction.Nomen.Name),
            _ => Option<string>.None,
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
            ObjectUnderPick: result.ObjectUnderPick.NonEmpty(),
            InletUnderPick: result.InletUnderPick.NonEmpty(),
            OutletUnderPick: result.OutletUnderPick.NonEmpty());

    // FrameOf reads bounds directly; no Attributes.Layout() mutation during read.
    private static Option<RectangleF> FrameOf(IEnumerable<IDocumentObject> targets) =>
        toSeq(targets).Fold(
            initialState: Option<RectangleF>.None,
            f: static (acc, obj) => acc.Match(
                Some: bounds => Some(RectangleF.Union(rect1: bounds, rect2: obj.Attributes.AggregateBounds)),
                None: () => Some(obj.Attributes.AggregateBounds)));

    private static Grasshopper2.Parsing.Result<Unit> InlineResult(string text, Func<string, Fin<Unit>> apply) =>
        GrasshopperUi.Protect(valid: () => apply(arg: text)).Match(
            Succ: _ => Grasshopper2.Parsing.Result<Unit>.Succeed(value: unit, underlying: text),
            Fail: error => Grasshopper2.Parsing.Result<Unit>.Fail(error: error.Message, underlying: text));

    private static readonly Lazy<Fin<MethodInfo>> ShowValueEditorPopup = new(() =>
        Optional(typeof(GhCanvas).GetMethod(
                name: "ShowValueEditorPopup",
                bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                types: [typeof(AbstractParameter), typeof(Control)],
                modifiers: null))
            .ToFin(Fail: UiFault.MutationRejected(op: Op.Of(name: nameof(CanvasOp.ValueEditor)), detail: "Canvas.ShowValueEditorPopup(AbstractParameter, Control) not found")));

    private static Fin<Unit> ShowValueEditor(GhCanvas canvas, AbstractParameter parameter, Control control) =>
        from method in ShowValueEditorPopup.Value
        from _ in Op.Of(name: nameof(CanvasOp.ValueEditor)).Attempt(body: () => {
            _ = method.Invoke(obj: canvas, parameters: [parameter, control]);
            return unit;
        }, what: "Canvas.ShowValueEditorPopup")
        select unit;

    private static Fin<Bitmap> RenderBitmap(GhCanvas canvas, CanvasRenderPolicy policy) =>
        Op.Of(name: nameof(CanvasOp.Render)).Attempt(body: () => {
            string? fault = null;
            CanvasBitmapLayers layers = policy.EffectiveLayers;
            EventHandler<CanvasPaintEventArgs>? handler = policy.Overlay.Marks.IsEmpty
                ? null
                : (_, args) => _ = policy.Overlay.Apply(scope: new PaintScope(
                    Phase: CanvasPaintPhase.AfterObjects,
                    Graphics: args.Graphics,
                    Skin: args.Skin)).IfFail(error => { fault = error.Message; return unit; });
            _ = Optional(handler).Iter(h => CanvasPaintPhase.AfterObjects.Attach(canvas: canvas, handler: h));
            try {
                Bitmap bitmap = canvas.DrawToBitmap(
                    width: policy.Width,
                    height: policy.Height,
                    drawBackground: (layers & CanvasBitmapLayers.Background) == CanvasBitmapLayers.Background,
                    drawWires: (layers & CanvasBitmapLayers.Wires) == CanvasBitmapLayers.Wires,
                    drawMessages: (layers & CanvasBitmapLayers.Messages) == CanvasBitmapLayers.Messages);
                return string.IsNullOrEmpty(value: fault)
                    ? bitmap
                    : throw new InvalidOperationException(message: fault);
            } finally {
                _ = Optional(handler).Iter(h => CanvasPaintPhase.AfterObjects.Detach(canvas: canvas, handler: h));
            }
        }, what: "Canvas.DrawToBitmap");

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
                XLabel: snap.X.Map(static action => new SnapLabel(Text: action.LabelText, Point: action.LabelPoint, Anchor: Some(action.LabelAnchor))),
                YLabel: snap.Y.Map(static action => new SnapLabel(Text: action.LabelText, Point: action.LabelPoint, Anchor: Some(action.LabelAnchor))),
                Lines: snap.X.Map(static action => toSeq(action.Lines)).IfNone(Seq<LineF>()) + snap.Y.Map(static action => toSeq(action.Lines)).IfNone(Seq<LineF>())));

    internal static Fin<int> SelectionDispatch(GhDocumentMethods methods, GhObjectList objects, SelectionOp op) =>
        op.Switch(
            state: (methods, objects),
            allCase: static (s, _) => Op.Of(name: nameof(SelectionOp.All)).Attempt(body: s.methods.SelectAll, what: "DocumentMethods.SelectAll"),
            noneCase: static (s, _) => Op.Of(name: nameof(SelectionOp.None)).Attempt(body: s.methods.DeselectAll, what: "DocumentMethods.DeselectAll"),
            invertCase: static (s, _) => Op.Of(name: nameof(SelectionOp.Invert)).Attempt(body: s.methods.InvertSelection, what: "DocumentMethods.InvertSelection"),
            growCase: static (s, g) => GraphSelection(op: Op.Of(name: nameof(SelectionOp.Grow)), objects: s.objects, upstream: g.Upstream, downstream: g.Downstream, replace: false),
            shiftCase: static (s, shift) => GraphSelection(op: Op.Of(name: nameof(SelectionOp.Shift)), objects: s.objects, upstream: shift.Upstream, downstream: !shift.Upstream, replace: true));

    private static Seq<PointF> ProbeLattice(PointF centre, float radius) =>
        toSeq(Enumerable.Range(start: -1, count: 3)
            .SelectMany(dx => Enumerable.Range(start: -1, count: 3)
                .Select(dy => (Dx: dx, Dy: dy, Point: new PointF(x: centre.X + (dx * radius), y: centre.Y + (dy * radius)))))
            .OrderBy(static probe => Math.Abs(probe.Dx) + Math.Abs(probe.Dy))
            .Select(static probe => probe.Point));

    private static Fin<int> GraphSelection(Op op, GhObjectList objects, bool upstream, bool downstream, bool replace) =>
        op.Attempt(body: () => {
            System.Collections.Generic.HashSet<Guid> next = replace ? [] : [.. objects.SelectedObjects.Select(static obj => obj.InstanceId)];
            Seq<IDocumentObject> selected = toSeq(objects.SelectedObjects);
            foreach (IDocumentObject obj in selected) {
                _ = Op.SideWhen(upstream, () => AddImmediate(objects: objects, owner: obj.InstanceId, upstream: true, selected: next));
                _ = Op.SideWhen(downstream, () => AddImmediate(objects: objects, owner: obj.InstanceId, upstream: false, selected: next));
            }
            int changed = 0;
            _ = Op.SideWhen(replace, () => {
                foreach (IDocumentObject obj in selected.Filter(obj => !next.Contains(obj.InstanceId))) {
                    obj.Selection = ObjectSelection.Unselected;
                    changed++;
                }
            });
            foreach (Guid id in next) {
                if (objects.Find(instanceId: id) is { Selection: not ObjectSelection.Selected } obj) {
                    obj.Selection = ObjectSelection.Selected;
                    changed++;
                }
            }
            return changed;
        }, what: "Connectivity selection expansion");

    private static Unit AddImmediate(GhObjectList objects, Guid owner, bool upstream, System.Collections.Generic.HashSet<Guid> selected) {
        IEnumerable<ConnectiveObject> nodes = upstream
            ? objects.Connectivity.FindImmediateInputs(owner)
            : objects.Connectivity.FindImmediateOutputs(owner);
        foreach (ConnectiveObject node in nodes) {
            _ = selected.Add(item: node.Id);
        }
        return unit;
    }

    internal static Fin<Option<Guid>> ComposeDispatch(GhDocumentMethods methods, GhObjectList objects, ObjectScope subject, ComposeOp op, ActionList actions) =>
        subject.Switch(
            state: (methods, objects, op, actions),
            selectionCase: static (s, _) => s.op.Apply(methods: s.methods, objects: s.objects, targets: null, actions: s.actions),
            objectsCase: static (s, selected) => selected.Ids
                .TraverseM(id => ResolveObject(objects: s.objects, id: id, op: Op.Of(name: nameof(ObjectScope.Objects))))
                .As()
                .Map(static resolved => resolved.ToArray())
                .Bind(targets => s.op.Apply(methods: s.methods, objects: s.objects, targets: targets, actions: s.actions)),
            primaryCase: static (_, _) => ScopeUse.Compose.RejectPrimary<Option<Guid>>(op: Op.Of(name: nameof(ComposeDispatch))),
            primaryAndSecondaryCase: static (_, _) => ScopeUse.Compose.RejectPrimary<Option<Guid>>(op: Op.Of(name: nameof(ComposeDispatch))));

    internal static Fin<IDocumentObject> ResolveObject(GhObjectList objects, Guid id, Op op) =>
        op.AcceptValue(value: id)
            .MapFail(_ => UiFault.InvalidInput(op: op, detail: "empty Guid"))
            .Bind(valid => Optional(objects.Find(instanceId: valid))
                .ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"object {valid} not found")));

    internal static Fin<IDocumentObject> ResolveObject(GrasshopperUi.Scope scope, Guid id, Op op) =>
        scope.NeedObjects().Bind(objs => ResolveObject(objects: objs, id: id, op: op));

    internal static int RunDelete(GhDocumentMethods methods, IDocumentObject[]? targets, bool dataOnly, Seq<WireEnds> wires, ActionList actions) =>
        DocumentDeleteMode.Resolve(targets: targets, dataOnly: dataOnly).Run(methods: methods, targets: targets, wires: wires, actions: actions);

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

    // Materialize the central undo/redo sequences once into node data: the same walk the count previously
    // discarded now yields per-node (Name, Count) for node-targeted history navigation.
    internal static DocumentHistorySnapshot HistorySnapshotOf(GhDocument document) {
        Seq<DocumentHistoryNode> undoNodes = HistoryNodesOf(sequence: document.Undo.CentralUndoSequence);
        Seq<DocumentHistoryNode> redoNodes = HistoryNodesOf(sequence: document.Undo.CentralRedoSequence);
        return new(
            IsEmpty: document.Undo.IsEmpty,
            CanUndo: document.Undo.FirstUndo is not null,
            CanRedo: document.Undo.FirstRedo is not null,
            UndoNodes: undoNodes,
            RedoNodes: redoNodes);
    }

    private static Seq<DocumentHistoryNode> HistoryNodesOf(IEnumerable<Node> sequence) =>
        toSeq(sequence).Map(static node => new DocumentHistoryNode(
            Name: Optional(node.Record?.Name.ToString()).IfNone(string.Empty),
            Count: node.Record?.Count ?? 0));

    internal static Fin<Snapshot<TDelta>> RunMutation<TDelta>(
        GrasshopperUi.Scope scope,
        Op op,
        Func<GhDocumentMethods, GhDocument, GhObjectList, ActionList, Fin<(DocumentMutationReceipt Receipt, TDelta Payload)>> mutate) =>
        from methods in scope.NeedMethods()
        from document in scope.NeedDocument()
        from objects in scope.NeedObjects()
        let actions = ActionList.Empty
        from mutation in Optional(mutate)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "mutation delegate is required"))
            .Bind(run => run(arg1: methods, arg2: document, arg3: objects, arg4: actions))
        from receipt in mutation.Receipt.Changed switch {
            >= 0 => Fin.Succ(value: mutation.Receipt),
            _ => Fin.Fail<DocumentMutationReceipt>(error: UiFault.MutationRejected(op: op, detail: string.Create(CultureInfo.InvariantCulture, $"count={mutation.Receipt.Changed}"))),
        }
        from _ in CommitActions(scope: scope, document: document, name: VerbNounOf(op: op), actions: actions)
        select Snapshot.Of(payload: mutation.Payload, ownerId: Some(document.Hash));

    internal static Fin<Unit> CommitActions(GrasshopperUi.Scope scope, GhDocument document, VerbNoun name, ActionList actions) =>
        actions.Count <= 0
            ? Fin.Succ(value: unit)
            : scope.UndoGroup is { IsSome: true, Case: UndoGroup bag }
                ? Try.lift(f: () => {
                    _ = bag.Add(list: actions);
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(CommitActions)), detail: $"UndoGroup add threw: {error.Message}"))
                : CommitActions(document: document, name: name, actions: actions);

    internal static Fin<Snapshot<DocumentMutationDelta>> RunDocumentMutation(
        GrasshopperUi.Scope scope,
        Op op,
        Func<GhDocumentMethods, GhObjectList, ActionList, Fin<DocumentMutationReceipt>> mutate) =>
        RunMutation(
            scope: scope,
            op: op,
            mutate: (methods, document, objects, actions) =>
                from receipt in mutate(arg1: methods, arg2: objects, arg3: actions)
                select (
                    Receipt: receipt,
                    Payload: new DocumentMutationDelta(
                        Changed: receipt.Changed,
                        After: DocumentSnapshotOf(document: document, objects: objects),
                        Created: receipt.Created)));

    internal static Fin<Unit> CommitActions(GhDocument document, VerbNoun name, ActionList actions) =>
        actions.Count switch {
            <= 0 => Fin.Succ(value: unit),
            _ => Try.lift(f: () => {
                document.Undo.Do(name: name, actions: actions);
                return unit;
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(CommitActions)), detail: $"History.Do threw: {error.Message}")),
        };

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
        Seq<WireEnds> allWires = Wire.AllWireEnds(objects: objects);
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
            Selected: obj.Selected, Activity: obj.Activity.Inv(),
            Display: obj.Display.Inv(), Phase: obj.Phase.Inv(), State: obj.State.Inv(),
            Bounds: obj.Attributes.Bounds, Pivot: obj.Attributes.Pivot);

}
