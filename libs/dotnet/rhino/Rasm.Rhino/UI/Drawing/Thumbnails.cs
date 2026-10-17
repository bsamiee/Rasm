using Eto.Drawing;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.UI.Drawing;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Thumbnail {
    public abstract IO<Unit> Paint(PlotCanvas canvas, RectangleF bounds, MarkColor ink);

    public abstract Option<Image> Drag { get; }

    public static void Release(Thumbnail thumbnail) =>
        thumbnail.Switch(raster: static raster => raster.Pixels.Dispose(), linework: static _ => { }, hatching: static _ => { });

    private static IO<Unit> Placed(PlotCanvas canvas, RectangleF bounds, Size extent, IO<Unit> draw) =>
        DisposalOps.Within(canvas.Graphics.SaveTransformState, _ => IO.lift(() => {
            canvas.Graphics.TranslateTransform(bounds.Location);
            canvas.Graphics.ScaleTransform(bounds.Width / extent.Width, bounds.Height / extent.Height);
        }).Bind(_ => draw));

    public sealed record Raster(Bitmap Pixels) : Thumbnail {
        public override Option<Image> Drag => Some<Image>(Pixels);

        public override IO<Unit> Paint(PlotCanvas canvas, RectangleF bounds, MarkColor ink) =>
            IO.lift(() => {
                canvas.Graphics.ImageInterpolation = ImageInterpolation.High;
                canvas.Graphics.DrawImage(Pixels, bounds);
            });
    }

    public sealed record Linework(Seq<Seq<PointF>> Dashes, Seq<Seq<PointF>> CurveShapes, Seq<Seq<PointF>> TextSurfaceShapes, Size Extent) : Thumbnail {
        public override Option<Image> Drag => None;

        public override IO<Unit> Paint(PlotCanvas canvas, RectangleF bounds, MarkColor ink) =>
            Placed(canvas, bounds, Extent,
                from color in IO.lift(() => Plots.Resolve(canvas, new MarkStyle.Fill(ink, 1f)))
                from dashes in DisposalOps.Within(
                    () => canvas.Graphics.AntiAlias,
                    smoothed => IO.lift(() => {
                        canvas.Graphics.AntiAlias = smoothed && !HostUtils.RunningOnOSX;
                        _ = Dashes.Iter(points => canvas.Graphics.FillPolygon(color, [.. points]));
                    }),
                    smoothed => IO.lift(() => { canvas.Graphics.AntiAlias = smoothed; }))
                from curves in IO.lift(() => CurveShapes.Filter(static points => points.Count > 1).Iter(points => canvas.Graphics.DrawLines(color, points)))
                from text in DisposalOps.Within(static () => new GraphicsPath { FillMode = FillMode.Alternate }, glyphs => IO.lift(() => {
                    _ = TextSurfaceShapes.Filter(static points => points.Count > 1).Iter(points => {
                        glyphs.AddLines(points);
                        glyphs.CloseFigure();
                    });
                    canvas.Graphics.FillPath(color, glyphs);
                }))
                select unit);
    }

    public sealed record Hatching(Seq<Line> Lines, Size Extent) : Thumbnail {
        public override Option<Image> Drag => None;

        public override IO<Unit> Paint(PlotCanvas canvas, RectangleF bounds, MarkColor ink) =>
            Placed(canvas, bounds, Extent, IO.lift(() => {
                Color color = Plots.Resolve(canvas, new MarkStyle.Fill(ink, 1f));
                if (Lines.IsEmpty)
                    canvas.Graphics.FillRectangle(color, 0f, 0f, Extent.Width, Extent.Height);
                else
                    _ = Lines.Iter(line => canvas.Graphics.DrawLine(color, (float)line.FromX, (float)-line.FromY, (float)line.ToX, (float)-line.ToY));
            }));
    }
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class Checkerboard {
    public static readonly Checkerboard Content = new(Colors.Snow, Colors.LightGrey, static bounds => bounds.Height / 4f);
    public static readonly Checkerboard Frame = new(Colors.White, Colors.DarkGray, static _ => 10f);

    public Color Light { get; }
    public Color Dark { get; }

    [UseDelegateFromConstructor]
    public partial float Square(RectangleF bounds);
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
                DisposalOps.Acquire(IO.lift(() => new Disposal<IO<Unit>>(close, static effect => effect.Run())), keep).Map(unit),
                EventKind.InstanceDefinitionTableEvent.Inline(args => Changed(release, args), sink, keep),
                Releasing(EventKind.CloseDocument, release, sink, keep),
                Releasing(EventKind.WorksessionFileChanged, release, sink, keep))
            .TraverseM(static attach => attach).As(), (_, lease) => retain(new DefinitionThumbnails(held, lease)))
        select unit;

    public void Dispose() => attached.Dispose();

    // --- [READS]
    public IO<Thumbnail.Raster> Definition(RhinoDoc doc, ComponentRef<InstanceDefinition> address, PreviewMethod method, Size extent) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from pixels in HostTheme.Device(extent)
        let key = new Key(doc.RuntimeSerialNumber, definition.Index, method, pixels)
        from cached in atomicIO(() => held.Value.Match(
            Some: cache => cache.Find(key), None: static () => throw Errors.Cancelled.ToException()))
        from raster in cached.Match(Some: static found => IO.pure(found), None: () => Rendered(doc, address, key))
        select raster;

    private IO<Thumbnail.Raster> Rendered(RhinoDoc doc, ComponentRef<InstanceDefinition> address, Key key) =>
        from raster in Definitions.Preview(doc, address, key.Method, key.Pixels, bitmap => Thumbnails.Owned(bitmap, raster => {
            if (!atomic(() => {
                Option<HashMap<Key, Thumbnail.Raster>> prior = held.Value;
                held.Value = prior.Map(cache => cache.Add(key, raster));
                return prior.IsSome;
            })) throw Errors.Cancelled.ToException();
        }))
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
    public static IO<Disposal<Thumbnail>> Meshes(RhinoDoc doc, Seq<(Mesh Mesh, Option<System.Drawing.Color> Color)> meshes, Size extent, Action<Disposal<Thumbnail>> retain) =>
        from fallback in TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes => IO.lift(() => attributes.DrawColor(doc)))
        from pixels in HostTheme.Device(extent)
        from raster in Lent(() => Refused.UnlessFound(
            DrawingUtilities.CreateMeshPreviewImage(doc, meshes.Map(static pair => pair.Mesh), meshes.Map(pair => pair.Color.IfNone(fallback)), pixels),
            nameof(DrawingUtilities.CreateMeshPreviewImage)).ThrowIfFail(), retain)
        select raster;

    public static IO<Disposal<Thumbnail>> Style(RhinoDoc doc, ComponentRef<DimensionStyle> address, Size extent, bool transparent, Action<Disposal<Thumbnail>> retain) =>
        from style in TableOps.Find(doc.DimStyles, address, includeDeleted: false)
        from pixels in HostTheme.Device(extent)
        from raster in Lent(() => Refused.UnlessFound(style.CreatePreviewBitmap(pixels.Width, pixels.Height, transparent), nameof(DimensionStyle.CreatePreviewBitmap)).ThrowIfFail(), retain)
        select raster;

    internal static IO<Thumbnail.Raster> Owned(System.Drawing.Bitmap lent, Action<Thumbnail.Raster> retain) =>
        DisposalOps.Acquire(acquired => DisposalOps.Within(() => lent.ToEto(), shown => DisposalOps.Acquire(IO.lift(() => new Thumbnail.Raster(new Bitmap(shown))), static raster => IO.lift(raster.Pixels.Dispose), raster => acquired(raster.Pixels))),
            (raster, _) => retain(raster));

    private static IO<Disposal<Thumbnail>> Lent(Func<System.Drawing.Bitmap> render, Action<Disposal<Thumbnail>> retain) =>
        DisposalOps.Acquire(acquired => DisposalOps.Within(render, bitmap => Owned(bitmap, raster => acquired(raster.Pixels))
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

    private static Seq<Seq<PointF>> Channel(Curve curve, Linetype linetype, Size extent, double scale, int kind) =>
        toSeq(DrawingUtilities.CreateLinetypePreviewGeometryEx(curve, linetype, extent.Width, extent.Height, scale, kind))
            .Map(static run => toSeq(run).Map(static point => new PointF(point.X, point.Y)).Strict())
            .Strict();

    // --- [CHECKER]
    public static IO<Unit> Checker(PlotCanvas canvas, RectangleF bounds, Checkerboard board) =>
        IO.lift(() => {
            float square = board.Square(bounds);
            canvas.Graphics.FillRectangle(board.Light, bounds);
            _ = (from column in toSeq(Range(0, (int)MathF.Ceiling(bounds.Width / square)))
                 from row in toSeq(Range(0, (int)MathF.Ceiling(bounds.Height / square)))
                 where (column + row) % 2 is 1
                 select RectangleF.Intersect(new RectangleF(bounds.X + (column * square), bounds.Y + (row * square), square, square), bounds))
                .Iter(cell => canvas.Graphics.FillRectangle(board.Dark, cell));
        });
}
