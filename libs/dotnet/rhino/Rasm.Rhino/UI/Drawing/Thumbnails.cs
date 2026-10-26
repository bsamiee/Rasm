using System.Numerics;
using Eto.Drawing;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.UI;
using Rasm.UI.Styling;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.UI;
using SkiaSharp;

namespace Rasm.Rhino.UI.Drawing;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Thumbnail {
    public Node Drawn(Role ink) =>
        Switch(
            ink,
            raster: static (_, raster) => (Node)new Node.Image(raster.Pixels) { Fit = ImageFit.Contain },
            linework: static (role, linework) => Plotted(linework.Extent, role,
                linework.Dashes.Map(static points => (Figure)new Figure.Polygon(points))
                + linework.CurveShapes.Filter(static points => points.Count > 1).Map(static points => (Figure)new Figure.Polyline(points))
                + linework.TextSurfaceShapes.Filter(static points => points.Count > 1).Map(static points => (Figure)new Figure.Polygon(points))),
            hatching: static (role, hatching) => Plotted(hatching.Extent, role, hatching.Lines.IsEmpty
                ? [new Figure.Region(None, None)]
                : hatching.Lines.Map(static line => (Figure)new Figure.Rule(new Vector2((float)line.FromX, (float)-line.FromY), new Vector2((float)line.ToX, (float)-line.ToY)))));

    public static void Release(Thumbnail thumbnail) =>
        thumbnail.Switch(raster: static raster => raster.Pixels.Dispose(), linework: static _ => { }, hatching: static _ => { });

    private static Node.Plot Plotted(Size extent, Role ink, Seq<Figure> figures) =>
        new(
            new Rasm.UI.PlotPlane.Cartesian(new Rasm.UI.Axis.Linear(Rasm.UI.AxisRange.Create(0f, extent.Width)), new Rasm.UI.Axis.Linear(Rasm.UI.AxisRange.Create(extent.Height, 0f))),
            figures.Map(figure => (Node)new Node.Mark(figure) { Role = ink }));

    public sealed record Raster(SKImage Pixels) : Thumbnail;

    public sealed record Linework(Seq<Seq<Vector2>> Dashes, Seq<Seq<Vector2>> CurveShapes, Seq<Seq<Vector2>> TextSurfaceShapes, Size Extent) : Thumbnail;

    public sealed record Hatching(Seq<Line> Lines, Size Extent) : Thumbnail;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinitionThumbnails : IDisposable {
    // --- [LIFETIME]
    private readonly Ref<Option<HashMap<Key, Thumbnail.Raster>>> held;
    private readonly IDisposable attached;

    private DefinitionThumbnails(Ref<Option<HashMap<Key, Thumbnail.Raster>>> held, IDisposable attached) => (this.held, this.attached) = (held, attached);

    public static IO<Unit> Open(IPlugInSink sink, IO<Unit> released, Action<DefinitionThumbnails> retain) =>
        from held in IO.lift(static () => Ref(Some(HashMap<Key, Thumbnail.Raster>())))
        let close = atomicIO(() => {
            Option<HashMap<Key, Thumbnail.Raster>> prior = held.Value;
            held.Value = None;
            return prior.Match(Some: static cache => toSeq(cache.Values), None: static () => Seq<Thumbnail.Raster>());
        }).Bind(Released).Uninterruptible()
        let release = fun((Func<Key, bool> dropped) => Release(held, released, dropped))
        from attached in DisposalOps.Acquire(keep => Seq(
                DisposalOps.Acquire(IO.pure(unit), _ => close, keep),
                EventKind.InstanceDefinitionTableEvent.Inline(args => Changed(release, args), sink, keep),
                Releasing(EventKind.CloseDocument, release, sink, keep),
                Releasing(EventKind.WorksessionFileChanged, release, sink, keep))
            .TraverseM(static attach => attach).As(), (_, lease) => retain(new DefinitionThumbnails(held, lease)))
        select unit;

    public void Dispose() => attached.Dispose();

    // --- [READS]
    public IO<Thumbnail.Raster> Definition(RhinoDoc doc, ComponentRef<InstanceDefinition> address, PreviewMethod method, Size extent, float scale) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        let key = new Key(doc.RuntimeSerialNumber, definition.Index, method, Thumbnails.Pixels(extent, scale))
        from cache in IO.lift(() => held.Value.ToFin(Errors.Cancelled))
        from raster in cache.Find(key).Match(Some: static found => IO.pure(found), None: () => Rendered(doc, address, key))
        select raster;

    private IO<Thumbnail.Raster> Rendered(RhinoDoc doc, ComponentRef<InstanceDefinition> address, Key key) =>
        from raster in Definitions.Preview(doc, address, key.Method, key.Pixels, bitmap => Thumbnails.Owned(bitmap, raster => atomicIO(() => {
            Option<HashMap<Key, Thumbnail.Raster>> prior = held.Value;
            held.Value = prior.Map(cache => cache.Add(key, raster));
            return guard(prior.IsSome, Errors.Cancelled).ToFin();
        }).Bind(IO.lift)))
        select raster;

    // --- [RELEASE]
    private static IO<Unit> Changed(Func<Func<Key, bool>, IO<Unit>> release, InstanceDefinitionTableEventArgs args) =>
        IO.lift(() => args.EventType).Bind(kind => when(
            kind is not InstanceDefinitionTableEventType.Sorted,
            from touched in IO.lift(() => (
                Document: EventKind.InstanceDefinitionTableEvent.DocumentSerial(args),
                Definitions: toHashSet(args.InstanceDefinitionIndex.Cons(Conversions.Rows(args.NewState.GetContainers()).Map(static container => container.Index)))))
            from dropped in release(key => touched.Document == Some(key.Document) && touched.Definitions.Contains(key.Definition))
            select dropped).As());

    private static IO<Unit> Releasing<TArgs>(DocumentEvent<TArgs> row, Func<Func<Key, bool>, IO<Unit>> release, IPlugInSink sink, Action<IDisposable> retain) =>
        row.Inline(args => IO.lift(() => row.DocumentSerial(args)).Bind(document => release(key => document == Some(key.Document))), sink, retain);

    private static IO<Unit> Release(Ref<Option<HashMap<Key, Thumbnail.Raster>>> held, IO<Unit> released, Func<Key, bool> dropped) =>
        (from gone in atomicIO(() => held.Value.Match(
            Some: cache => {
                HashMap<Key, Thumbnail.Raster> removed = cache.Filter((key, _) => dropped(key));
                held.Value = Some(cache.RemoveRange(removed.Keys));
                return toSeq(removed.Values);
            },
            None: static () => Seq<Thumbnail.Raster>()))
         from disposed in Released(gone)
         select gone).Uninterruptible()
            .Bind(gone => when(!gone.IsEmpty, released).As());

    private static IO<Unit> Released(Seq<Thumbnail.Raster> rasters) =>
        DisposalOps.Release(rasters.Map(static raster => raster.Pixels));

    private sealed record Key(uint Document, int Definition, PreviewMethod Method, System.Drawing.Size Pixels);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Thumbnails {
    // --- [RASTERS]
    public static IO<Disposal<Thumbnail>> Meshes(
        RhinoDoc doc, Seq<(Mesh Mesh, Option<System.Drawing.Color> Color)> meshes, Size extent, float scale, Action<Disposal<Thumbnail>> retain) =>
        from fallback in TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes => IO.lift(() => attributes.DrawColor(doc)))
        from raster in Lent(() => Refused.UnlessFound(
            DrawingUtilities.CreateMeshPreviewImage(doc, meshes.Map(static pair => pair.Mesh), meshes.Map(pair => pair.Color.IfNone(fallback)), Pixels(extent, scale)),
            nameof(DrawingUtilities.CreateMeshPreviewImage)).ThrowIfFail(), retain)
        select raster;

    public static IO<Disposal<Thumbnail>> Style(RhinoDoc doc, ComponentRef<DimensionStyle> address, Size extent, float scale, bool transparent, Action<Disposal<Thumbnail>> retain) =>
        from style in TableOps.Find(doc.DimStyles, address, includeDeleted: false)
        let pixels = Pixels(extent, scale)
        from raster in Lent(() => Refused.UnlessFound(style.CreatePreviewBitmap(pixels.Width, pixels.Height, transparent), nameof(DimensionStyle.CreatePreviewBitmap)).ThrowIfFail(), retain)
        select raster;

    internal static System.Drawing.Size Pixels(Size extent, float scale) => new((int)MathF.Ceiling(extent.Width * scale), (int)MathF.Ceiling(extent.Height * scale));

    internal static IO<Thumbnail.Raster> Owned(System.Drawing.Bitmap lent, Func<Thumbnail.Raster, IO<Unit>> publish) =>
        DisposalOps.Acquire(acquired => DisposalOps.Within(() => lent.ToEto(), shown => DisposalOps.Acquire(
                IO.lift(() => Refused.UnlessFound(SKImage.FromEncodedData(shown.ToByteArray(ImageFormat.Png)), nameof(SKImage.FromEncodedData)).Map(static image => new Thumbnail.Raster(image))),
                static raster => raster.Pixels, acquired)),
            (raster, _) => publish(raster));

    private static IO<Disposal<Thumbnail>> Lent(Func<System.Drawing.Bitmap> render, Action<Disposal<Thumbnail>> retain) =>
        DisposalOps.Acquire(acquired => DisposalOps.Within(render, bitmap => Owned(bitmap, raster => IO.lift(() => acquired(raster.Pixels)))
            .Map(static raster => new Disposal<Thumbnail>(raster, Thumbnail.Release))),
            (lease, _) => retain(lease));

    // --- [LINEWORK]
    private const double Span = 50d;

    public static IO<Seq<Thumbnail>> Linetypes(Seq<Linetype> linetypes, Size extent) =>
        DisposalOps.Within(static () => new LineCurve(Point3d.Origin, new Point3d(Span, 0d, 0d)), curve =>
            from tallest in IO.lift(() => linetypes.Fold(0d, static (held, linetype) => Math.Max(held, linetype.ShapeBounds.Diagonal.Y)))
            from previews in linetypes.TraverseM(linetype => Previewed(curve, linetype, extent, tallest)).As()
            select previews);

    public static IO<Thumbnail> Pattern(HatchPattern pattern, double angle, Size extent) =>
        IO.lift<Thumbnail>(() => new Thumbnail.Hatching(toSeq(pattern.CreatePreviewGeometry(extent.Width, extent.Height, angle)), extent));

    private static IO<Thumbnail> Previewed(Curve curve, Linetype linetype, Size extent, double tallest) =>
        DisposalOps.Within(() => new Linetype(linetype) { WidthUnits = UnitSystem.None }, copy =>
            from factor in IO.lift(() => Factor(copy))
            from rescaled in Rescaled(copy, factor)
            let scale = tallest > 0d ? 0.6d * Span * extent.Height / extent.Width / tallest : factor
            select (Thumbnail)new Thumbnail.Linework(
                Channel(curve, copy, extent, scale, 0), Channel(curve, copy, extent, scale, 1), Channel(curve, copy, extent, scale, 2), extent));

    private static double Factor(Linetype linetype) =>
        linetype.SegmentCount > 1 && linetype.PatternLength > 0d && (Span / linetype.PatternLength is < 5d or > 50d) ? 10d / linetype.PatternLength : 1d;

    private static IO<Unit> Rescaled(Linetype copy, double factor) =>
        IO.lift(() => Callbacks.Each(
            toSeq(Range(0, copy.SegmentCount)),
            index => {
                copy.GetSegment(index, out double length, out bool solid);
                return copy.SetSegment(index, length * factor, solid);
            },
            nameof(Linetype.SetSegment)));

    private static Seq<Seq<Vector2>> Channel(Curve curve, Linetype linetype, Size extent, double scale, int kind) =>
        toSeq(DrawingUtilities.CreateLinetypePreviewGeometryEx(curve, linetype, extent.Width, extent.Height, scale, kind))
            .Map(static run => toSeq(run).Map(static point => new Vector2(point.X, point.Y)).Strict())
            .Strict();
}
