using System.IO;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using UiShape = Grasshopper2.UI.Skinning.Shape;

namespace Rasm.Grasshopper.UI;

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
    bool WindowSelectGroups);

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
public readonly record struct CanvasPickSnapshot(
    Pick Kind,
    Option<PointF> Point,
    int SelectedCount,
    int SelectedWireCount,
    int SelectedObjectCount,
    int DeselectedCount,
    int DeselectedWireCount,
    int DeselectedObjectCount,
    Option<WireEnds> WireUnderPick,
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
            WireUnderPick: Optional(new WireEnds(source: result.WireUnderPick.Source, target: result.WireUnderPick.Target))
                .Filter(static wire => wire.Source != Guid.Empty && wire.Target != Guid.Empty),
            ObjectUnderPick: Optional(result.ObjectUnderPick).Filter(static id => id != Guid.Empty),
            InletUnderPick: Optional(result.InletUnderPick).Filter(static id => id != Guid.Empty),
            OutletUnderPick: Optional(result.OutletUnderPick).Filter(static id => id != Guid.Empty));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasBitmapSnapshot(int Width, int Height, ReadOnlyMemory<byte> Png);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasMappedPoint(PointF Source, PointF Target, CoordinateSystem From, CoordinateSystem To);

// --- [INTENTS] --------------------------------------------------------------------------
public static class CanvasIntent {
    public static GrasshopperUiIntent<CanvasSnapshot> Snapshot(bool openEditor = false) =>
        new(
            run: scope => scope.Canvas.Match(
                Some: canvas => Fin.Succ(value: SnapshotOf(scope: scope, canvas: canvas)),
                None: () => Fin.Succ(value: new CanvasSnapshot(
                    HasEditor: scope.Editor.IsSome,
                    HasCanvas: false,
                    HasDocument: false,
                    VisibleFrame: RectangleF.Empty,
                    ContentBounds: RectangleF.Empty,
                    ProjectionCentre: PointF.Empty,
                    ProjectionZoom: 1,
                    WindowSelectObjects: false,
                    WindowSelectWires: false,
                    WindowSelectGroups: false))),
            policy: new GrasshopperUiPolicy(OpenEditor: openEditor));

    public static GrasshopperUiIntent<Unit> Invalidate(bool scheduled = false) =>
        new(
            run: scope => scope.Canvas
                .ToFin(Fail: Op.Of(name: nameof(Invalidate)).InvalidInput())
                .Map((GhCanvas canvas) => {
                    Action<GhCanvas> invalidate = scheduled switch {
                        true => static target => target.ScheduleRedraw(),
                        false => static target => target.Invalidate(),
                    };
                    invalidate(obj: canvas);
                    return unit;
                }),
            policy: GrasshopperUiPolicy.Canvas());

    public static GrasshopperUiIntent<CanvasPickSnapshot> Pick(PointF point, CanvasPickPolicy policy) =>
        Pick(point: point, source: CoordinateSystem.Content, policy: policy);

    public static GrasshopperUiIntent<CanvasPickSnapshot> Pick(PointF point, CoordinateSystem source, CanvasPickPolicy policy) =>
        new(
            run: scope => Optional(point)
                .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y))
                .ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
                .Bind(validPoint => scope.Canvas
                    .ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())
                    .Map(canvas => {
                        PointF mapped = source switch {
                            CoordinateSystem.Content => validPoint,
                            _ => canvas.Map(point: validPoint, from: source, to: CoordinateSystem.Content),
                        };
                        return CanvasPickSnapshot.Of(result: canvas.ResolvePick(
                        point: mapped,
                        includeGrips: policy.IncludeGrips,
                        includeForeground: policy.IncludeForeground,
                        includeBackground: policy.IncludeBackground,
                        includeWires: policy.IncludeWires,
                        recursive: policy.Recursive));
                    })),
            policy: GrasshopperUiPolicy.Canvas());

    public static GrasshopperUiIntent<CanvasMappedPoint> Map(PointF point, CoordinateSystem source, CoordinateSystem target) =>
        new(
            run: scope => Optional(point)
                .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y))
                .ToFin(Fail: Op.Of(name: nameof(Map)).InvalidInput())
                .Bind(validPoint => scope.Canvas
                    .ToFin(Fail: Op.Of(name: nameof(Map)).InvalidInput())
                    .Map(canvas => new CanvasMappedPoint(Source: validPoint, Target: canvas.Map(point: validPoint, from: source, to: target), From: source, To: target))),
            policy: GrasshopperUiPolicy.Canvas());

    public static GrasshopperUiIntent<Unit> Instantiate(bool mouseCentred, string? initialText = null) =>
        new(
            run: scope => scope.Canvas
                .ToFin(Fail: Op.Of(name: nameof(Instantiate)).InvalidInput())
                .Map(canvas => {
                    canvas.ShowInstantiationPopup(mouseCentred: mouseCentred, initialText: initialText);
                    return unit;
                }),
            policy: GrasshopperUiPolicy.Canvas(openEditor: true));

    public static GrasshopperUiIntent<CanvasBitmapSnapshot> Bitmap(int width, int height, bool drawBackground, bool drawWires, bool drawMessages) =>
        new(
            run: scope => Optional((Width: width, Height: height))
                .Filter(static size => size.Width > 0 && size.Height > 0)
                .ToFin(Fail: Op.Of(name: nameof(Bitmap)).InvalidInput())
                .Bind(size => scope.Canvas
                    .ToFin(Fail: Op.Of(name: nameof(Bitmap)).InvalidInput())
                    .Bind(canvas => Encode(
                        bitmap: canvas.DrawToBitmap(width: size.Width, height: size.Height, drawBackground: drawBackground, drawWires: drawWires, drawMessages: drawMessages),
                        name: nameof(Bitmap)))),
            policy: GrasshopperUiPolicy.Canvas());

    public static GrasshopperUiIntent<CanvasBitmapSnapshot> PickMap() =>
        new(
            run: scope => scope.Canvas
                .ToFin(Fail: Op.Of(name: nameof(PickMap)).InvalidInput())
                .Bind(canvas => Encode(bitmap: canvas.DrawPickMap(), name: nameof(PickMap))),
            policy: GrasshopperUiPolicy.Canvas());

    public static GrasshopperUiIntent<CanvasSnapshot> Navigate(RectangleF frame, float minimumZoom, float maximumZoom, TimeSpan duration = default) =>
        new(
            run: scope => Optional(frame)
                .Filter(value => value.Width > 0 && value.Height > 0 && float.IsFinite(minimumZoom) && float.IsFinite(maximumZoom) && minimumZoom > 0 && maximumZoom >= minimumZoom)
                .ToFin(Fail: Op.Of(name: nameof(Navigate)).InvalidInput())
                .Bind(validFrame => scope.Canvas
                    .ToFin(Fail: Op.Of(name: nameof(Navigate)).InvalidInput())
                    .Map(canvas => {
                        canvas.Navigate(frame: validFrame, zoomLimits: (minimumZoom, maximumZoom), duration: duration);
                        return SnapshotOf(scope: scope, canvas: canvas);
                    })),
            policy: GrasshopperUiPolicy.Canvas(openEditor: true, repaint: true));

    public static GrasshopperUiIntent<CanvasSnapshot> FrameSelection(float padding = 48, float minimumZoom = 0.05f, float maximumZoom = 2f, TimeSpan duration = default) =>
        new(
            run: scope =>
                from objects in scope.Objects.ToFin(Fail: Op.Of(name: nameof(FrameSelection)).InvalidInput())
                from frame in FrameOf(objects: objects.SelectedObjects, padding: padding, name: nameof(FrameSelection))
                from snapshot in Navigate(frame: frame, minimumZoom: minimumZoom, maximumZoom: maximumZoom, duration: duration).Run(scope: scope)
                select snapshot,
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<CanvasSnapshot> FrameObjects(Seq<Guid> ids, float padding = 48, float minimumZoom = 0.05f, float maximumZoom = 2f, TimeSpan duration = default) =>
        new(
            run: scope =>
                from objects in scope.Objects.ToFin(Fail: Op.Of(name: nameof(FrameObjects)).InvalidInput())
                from frame in FrameOf(objects: ids.Choose(id => Optional(objects.Find(instanceId: id))), padding: padding, name: nameof(FrameObjects))
                from snapshot in Navigate(frame: frame, minimumZoom: minimumZoom, maximumZoom: maximumZoom, duration: duration).Run(scope: scope)
                select snapshot,
            policy: GrasshopperUiPolicy.Document(repaint: true));

    public static GrasshopperUiIntent<bool> ViewportDragging(bool enabled) =>
        new(
            run: scope => scope.Canvas
                .ToFin(Fail: Op.Of(name: nameof(ViewportDragging)).InvalidInput())
                .Map(canvas => {
                    canvas.ViewportDragging = enabled;
                    return canvas.ViewportDragging;
                }),
            policy: GrasshopperUiPolicy.Canvas(repaint: true));

    private static CanvasSnapshot SnapshotOf(GrasshopperUi.Scope scope, GhCanvas canvas) =>
        new(
            HasEditor: scope.Editor.IsSome,
            HasCanvas: true,
            HasDocument: scope.Document.IsSome,
            VisibleFrame: canvas.VisibleFrame,
            ContentBounds: canvas.ContentBounds,
            ProjectionCentre: canvas.Projection.Origin,
            ProjectionZoom: canvas.Projection.Zoom,
            WindowSelectObjects: canvas.WindowSelectObjects,
            WindowSelectWires: canvas.WindowSelectWires,
            WindowSelectGroups: canvas.WindowSelectGroups);

    private static Fin<RectangleF> FrameOf(IEnumerable<IDocumentObject> objects, float padding, string name) =>
        Optional(objects)
            .ToFin(Fail: Op.Of(name: name).InvalidInput())
            .Bind((IEnumerable<IDocumentObject> valid) => toSeq(valid)
                .Map(static (IDocumentObject obj) => {
                    obj.Attributes.Layout(shape: UiShape.Default);
                    return obj.Attributes.AggregateBounds;
                })
                .Fold(
                    Option<RectangleF>.None,
                    static (Option<RectangleF> state, RectangleF next) => state.Match(
                        Some: current => Some(RectangleF.Union(current, next)),
                        None: () => Some(next)))
                .Map((RectangleF bounds) => {
                    bounds.Inflate(width: Math.Max(0, padding), height: Math.Max(0, padding));
                    return bounds;
                })
                .Filter(static bounds => bounds.Width > 0 && bounds.Height > 0)
                .ToFin(Fail: Op.Of(name: name).InvalidResult()));

    private static Fin<CanvasBitmapSnapshot> Encode(Bitmap? bitmap, string name) =>
        Optional(bitmap)
            .ToFin(Fail: Op.Of(name: name).InvalidResult())
            .Map(valid => {
                using Bitmap owned = valid;
                using MemoryStream stream = new();
                owned.Save(stream: stream, format: ImageFormat.Png);
                return new CanvasBitmapSnapshot(Width: owned.Width, Height: owned.Height, Png: stream.ToArray());
            });
}
