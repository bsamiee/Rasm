using Eto.Drawing;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Components;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.UI.Assets;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Thumbnail {
    public abstract IO<Unit> Paint(PlotCanvas canvas, RectangleF bounds, MarkColor ink);

    public abstract Option<Image> Drag { get; }

    public static void Release(Thumbnail thumbnail) =>
        thumbnail.Switch(raster: static raster => raster.Pixels.Dispose(), linework: static _ => { }, hatching: static _ => { });

    private static IO<Unit> Placed(PlotCanvas canvas, RectangleF bounds, Size extent, Action draw) =>
        IO.lift(() => {
            canvas.Graphics.SaveTransform();
            canvas.Graphics.TranslateTransform(bounds.Location);
            canvas.Graphics.ScaleTransform(bounds.Width / extent.Width, bounds.Height / extent.Height);
        }).Bracket(Use: _ => IO.lift(draw), Fin: _ => IO.lift(canvas.Graphics.RestoreTransform));

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
            Placed(canvas, bounds, Extent, () => {
                Color color = Plots.Resolve(canvas, new MarkStyle.Fill(ink, 1f));
                bool smoothed = canvas.Graphics.AntiAlias;
                canvas.Graphics.AntiAlias = smoothed && !HostUtils.RunningOnOSX;
                _ = Dashes.Iter(points => canvas.Graphics.FillPolygon(color, [.. points]));
                canvas.Graphics.AntiAlias = smoothed;
                _ = CurveShapes.Filter(static points => points.Count > 1).Iter(points => canvas.Graphics.DrawLines(color, points));
                using GraphicsPath glyphs = new() { FillMode = FillMode.Alternate };
                _ = TextSurfaceShapes.Filter(static points => points.Count > 1).Iter(points => {
                    glyphs.AddLines(points);
                    glyphs.CloseFigure();
                });
                canvas.Graphics.FillPath(color, glyphs);
            });
    }

    public sealed record Hatching(Seq<Line> Lines, Size Extent) : Thumbnail {
        public override Option<Image> Drag => None;

        public override IO<Unit> Paint(PlotCanvas canvas, RectangleF bounds, MarkColor ink) =>
            Placed(canvas, bounds, Extent, () => {
                Color color = Plots.Resolve(canvas, new MarkStyle.Fill(ink, 1f));
                if (Lines.IsEmpty)
                    canvas.Graphics.FillRectangle(color, 0f, 0f, Extent.Width, Extent.Height);
                else
                    _ = Lines.Iter(line => canvas.Graphics.DrawLine(color, (float)line.FromX, (float)-line.FromY, (float)line.ToX, (float)-line.ToY));
            });
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
    private readonly Atom<Cache> held;
    private readonly IDisposable attached;

    private DefinitionThumbnails(Atom<Cache> held, IDisposable attached) => (this.held, this.attached) = (held, attached);

    public static IO<DefinitionThumbnails> Open(IPlugInSink sink, IO<Unit> released) =>
        from held in IO.lift(static () => Atom(new Cache(HashMap<Key, Thumbnail.Raster>(), Seq<Thumbnail.Raster>())))
        let release = fun((Func<Key, bool> dropped) => Release(held, released, dropped))
        from attached in DisposalOps.AcquireAll(
            Seq(
                IO.pure<IDisposable>(new Disposal<Atom<Cache>>(held, static cache => cache.Value.Entries.Values.Iter(Thumbnail.Release))),
                EventKind.InstanceDefinitionTableEvent.Inline(args => Changed(release, args), sink),
                Releasing(EventKind.CloseDocument, release, sink),
                Releasing(EventKind.WorksessionFileChanged, release, sink)),
            DisposalOps.Release)
        select new DefinitionThumbnails(held, DisposalOps.Composite(attached, new CallbackSite(sink, typeof(DefinitionThumbnails), nameof(Dispose))));

    public void Dispose() => attached.Dispose();

    // --- [READS]
    public IO<Thumbnail.Raster> Definition(RhinoDoc doc, ComponentRef<InstanceDefinition> address, PreviewMethod method, Size extent) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from pixels in Themes.Device(extent)
        let key = new Key(doc.RuntimeSerialNumber, definition.Index, method, pixels)
        from cached in held.ValueIO.Map(cache => cache.Entries.Find(key))
        from raster in cached.Match(Some: static found => IO.pure(found), None: () => Rendered(doc, address, key))
        select raster;

    private IO<Thumbnail.Raster> Rendered(RhinoDoc doc, ComponentRef<InstanceDefinition> address, Key key) =>
        from raster in Definitions.Preview(doc, address, key.Method, key.Pixels, Thumbnails.Owned)
        from stored in held.SwapIO(cache => new Cache(cache.Entries.Add(key, raster), Seq<Thumbnail.Raster>()))
        select raster;

    // --- [RELEASE]
    private static IO<Unit> Changed(Func<Func<Key, bool>, IO<Unit>> release, InstanceDefinitionTableEventArgs args) =>
        IO.lift(() => args.EventType).Bind(kind => when(
            kind != InstanceDefinitionTableEventType.Sorted,
            from touched in IO.lift(() => (
                Document: EventKind.InstanceDefinitionTableEvent.DocumentSerial(args),
                Definitions: toHashSet(args.InstanceDefinitionIndex.Cons(Conversions.Rows(args.NewState.GetContainers()).Map(static container => container.Index)))))
            from dropped in release(key => touched.Document == Some(key.Document) && touched.Definitions.Contains(key.Definition))
            select dropped).As());

    private static IO<IDisposable> Releasing<TArgs>(DocumentEvent<TArgs> row, Func<Func<Key, bool>, IO<Unit>> release, IPlugInSink sink) =>
        row.Inline(args => IO.lift(() => row.DocumentSerial(args)).Bind(document => release(key => document == Some(key.Document))), sink);

    private static IO<Unit> Release(Atom<Cache> held, IO<Unit> released, Func<Key, bool> dropped) =>
        from swapped in held.SwapIO(cache => cache.Entries.Filter((key, _) => dropped(key)) switch {
            var gone => new Cache(cache.Entries.RemoveRange(gone.Keys), toSeq(gone.Values)),
        })
        from disposed in IO.lift(() => swapped.Released.Iter(Thumbnail.Release))
        from redrawn in when(!swapped.Released.IsEmpty, released).As()
        select unit;

    private sealed record Key(uint Document, int Definition, PreviewMethod Method, System.Drawing.Size Pixels);

    private sealed record Cache(HashMap<Key, Thumbnail.Raster> Entries, Seq<Thumbnail.Raster> Released);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Thumbnails {
    // --- [RASTERS]
    public static IO<Disposal<Thumbnail>> Meshes(RhinoDoc doc, Seq<(Mesh Mesh, Option<System.Drawing.Color> Color)> meshes, Size extent) =>
        from fallback in TableOps.WithAttributes(doc.CreateDefaultAttributes, None, attributes => IO.lift(() => attributes.DrawColor(doc)))
        from pixels in Themes.Device(extent)
        from raster in Lent(IO.lift(() => Missing.Unless(
            DrawingUtilities.CreateMeshPreviewImage(doc, meshes.Map(static pair => pair.Mesh), meshes.Map(pair => pair.Color.IfNone(fallback)), pixels),
            nameof(DrawingUtilities.CreateMeshPreviewImage))))
        select raster;

    public static IO<Disposal<Thumbnail>> Style(RhinoDoc doc, ComponentRef<DimensionStyle> address, Size extent, bool transparent) =>
        from style in TableOps.Find(doc.DimStyles, address, includeDeleted: false)
        from pixels in Themes.Device(extent)
        from raster in Lent(IO.lift(() => Missing.Unless(style.CreatePreviewBitmap(pixels.Width, pixels.Height, transparent), nameof(DimensionStyle.CreatePreviewBitmap))))
        select raster;

    internal static IO<Thumbnail.Raster> Owned(System.Drawing.Bitmap lent) =>
        (from shown in use(() => lent.ToEto())
         from pixels in IO.lift(() => new Bitmap(shown.Size, PixelFormat.Format32bppRgba))
         from drawn in DisposalOps.OnFailure(
             use(() => new Graphics(pixels)).Bind(canvas => IO.lift(() => canvas.DrawImage(shown, 0f, 0f))).Bracket(),
             IO.lift(pixels.Dispose))
         select new Thumbnail.Raster(pixels)).Bracket();

    private static IO<Disposal<Thumbnail>> Lent(IO<System.Drawing.Bitmap> render) =>
        use(render).Bind(Owned).Bracket().Map(static raster => new Disposal<Thumbnail>(raster, Thumbnail.Release));

    // --- [LINEWORK]
    private const double Span = 50d;

    public static IO<Seq<Thumbnail>> Linetypes(Seq<Linetype> linetypes, Size extent) =>
        (from curve in use(static () => new LineCurve(Point3d.Origin, new Point3d(Span, 0d, 0d)))
         from tallest in IO.lift(() => linetypes.Fold(0d, static (held, linetype) => Math.Max(held, linetype.ShapeBounds.Diagonal.Y)))
         from previews in linetypes.TraverseM(linetype => Previewed(curve, linetype, extent, tallest)).As()
         select previews).Bracket();

    public static IO<Thumbnail> Pattern(HatchPattern pattern, double angle, Size extent) =>
        IO.lift<Thumbnail>(() => new Thumbnail.Hatching(toSeq(pattern.CreatePreviewGeometry(extent.Width, extent.Height, angle)), extent));

    private static IO<Thumbnail> Previewed(Curve curve, Linetype linetype, Size extent, double tallest) =>
        (from copy in use(() => new Linetype(linetype) { WidthUnits = UnitSystem.None })
         from factor in IO.lift(() => Factor(copy))
         from rescaled in Rescaled(copy, factor)
         let scale = tallest > 0d ? 0.6d * Span * extent.Height / extent.Width / tallest : factor
         select (Thumbnail)new Thumbnail.Linework(
             Channel(curve, copy, extent, scale, 0), Channel(curve, copy, extent, scale, 1), Channel(curve, copy, extent, scale, 2), extent)).Bracket();

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
                 where (column + row) % 2 == 1
                 select RectangleF.Intersect(new RectangleF(bounds.X + (column * square), bounds.Y + (row * square), square, square), bounds))
                .Iter(cell => canvas.Graphics.FillRectangle(board.Dark, cell));
        });
}
