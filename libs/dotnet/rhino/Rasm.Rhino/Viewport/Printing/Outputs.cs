using System.Drawing;
using Rasm.Drafting;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Rhino.Viewport.Printing;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record PageSource {
    public sealed record ViewSet(ViewportSet Set, CaptureRequest Request, Option<ViewportType> Kind = default) : PageSource;

    public sealed record Named(string Name, ViewportTarget Viewport, CaptureRequest Request) : PageSource;
}

[Union]
public abstract partial record RasterSource {
    public sealed record Printed(PageSource Source) : RasterSource;

    public sealed record Screen(ViewportSet Set, ViewCapture Capture) : RasterSource;

    public sealed record Mode(ViewportSet Set, Guid Id, Option<Size> Pixels, Seq<Action<DisplayPipelineAttributes>> Edits) : RasterSource;

    public sealed record Posed(ViewportTarget Viewport, Seq<CameraPose> Poses, Option<Size> Pixels) : RasterSource;

    public sealed record Depth(ViewportTarget Viewport, Action<ZBufferCapture> Setup) : RasterSource;
}

[Union]
public abstract partial record PdfSource {
    public sealed record Captured(PageSource Source, Seq<PageMark> Marks) : PdfSource;

    public sealed record Blank(Sheet Sheet, double Density, Seq<PageMark> Marks) : PdfSource;
}

internal sealed record PdfPage(RhinoDoc Document, FilePdf Pdf, int Number, double Dpi, int Height, Option<RhinoObject> Context) {
    internal PointF Placed((Length X, Length Y) point) => new(Dots(point.X), Height - Dots(point.Y));

    internal float Dots(Length length) => (float)(length.Inches.ToDouble() * Dpi);
}

public sealed record PageMark {
    private PageMark(Func<PdfPage, IO<Unit>> draw) => Draw = draw;

    internal Func<PdfPage, IO<Unit>> Draw { get; }

    // --- [MARKS]
    public static PageMark Text(
        Seq<FieldRun> runs, (Length X, Length Y) anchor, Length height, global::Rhino.DocObjects.Font font, Color fill, Option<(Color Color, Length Width)> outline, double rotation,
        TextHorizontalAlignment horizontal, TextVerticalAlignment vertical) =>
        new(page =>
            from words in FieldFormulas.Format(page.Document, FieldFormulas.Text(runs), obj: page.Context, topParent: page.Context)
            let at = page.Placed(anchor)
            let edge = outline.IfNone((Color.Empty, Length.Zero))
            from drawn in IO.lift(() => page.Pdf.DrawText(page.Number, words, at.X, at.Y, (float)height.DtpPoints.ToDouble(), font, fill, edge.Color,
                (float)edge.Width.DtpPoints.ToDouble(), (float)double.RadiansToDegrees(rotation), horizontal, vertical))
            select drawn);

    public static PageMark Line((Length X, Length Y) start, (Length X, Length Y) next, Seq<(Length X, Length Y)> rest, Option<Color> fill, Color stroke, Length width) =>
        new(page => IO.lift(() => page.Pdf.DrawPolyline(page.Number, [.. from point in start.Cons(next.Cons(rest)) select page.Placed(point)], Conversions.Unset(fill), stroke, page.Dots(width))));

    public static PageMark Image(Bitmap bitmap, (Length X, Length Y) corner, (Length Width, Length Height) extent, double rotation) =>
        new(page => IO.lift(() => {
            PointF at = page.Placed((corner.X, corner.Y + extent.Height));
            page.Pdf.DrawBitmap(page.Number, bitmap, at.X, at.Y, page.Dots(extent.Width), page.Dots(extent.Height), (float)double.RadiansToDegrees(rotation));
        }));
}

public sealed record PageText(Seq<FieldRun> Header, Seq<FieldRun> Footer);

public sealed record FileOutput(OutputFolder Folder, Stem Stem, bool Overwrite, Seq<NamePart> Scope, OutputVersioning Version) {
    public OutputConvention Convention { get; init; } = OutputConvention.Default;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Outputs {
    // --- [PAGES]
    private static IO<Seq<(IO<ViewCaptureSettings> Settings, Option<RhinoObject> Context)>> Planned(RhinoDoc doc, PageSource source) =>
        source.Switch(
            doc,
            viewSet: static (document, views) =>
                from rows in Viewports.Resolve(document, views.Set)
                from pages in rows.Filter(row => views.Kind.ForAll(kind => row.Viewport.ViewportType == kind)).TraverseM(row =>
                    Context(document, row).Map(context => (Settings: ViewCaptures.Settings(row, views.Request), Context: context))).As()
                select pages,
            named: static (document, named) =>
                from row in Viewports.Resolve(document, named.Viewport)
                from index in NamedViews.Find(document, named.Name)
                from context in Context(document, row)
                select Seq((Settings: Projected(document, row, index, named), Context: context)));

    private static IO<Option<RhinoObject>> Context(RhinoDoc doc, ViewportRef row) =>
        row.View is RhinoPageView page
            ? new ObjectTarget.Lookup(table => table.GetObjectList(
                    new ObjectEnumeratorSettings { SpaceFilter = ActiveSpace.PageSpace, ViewportFilter = page.MainViewport, HiddenObjects = true, LockedObjects = true }))
                .Objects(doc).Map(static found => found.Head)
            : IO.pure(Option<RhinoObject>.None);

    private static IO<ViewCaptureSettings> Projected(RhinoDoc doc, ViewportRef row, int index, PageSource.Named named) =>
        from mode in Products.Of(() => row.Viewport.DisplayMode)
        from free in use(() => new ViewportRef(row.View, new RhinoViewport(), None))
        from framed in IO.lift(() => {
            free.Viewport.Size = named.Request.Media.IfNone(() => row.Viewport.Size);
            free.Viewport.DisplayMode = mode;
        })
        from restored in NamedViews.Applied(doc, index, free.Viewport, new RestorePacing.Immediate())
        from settings in ViewCaptures.Settings(free, named.Request)
        select settings;

    private static IO<TValue> Pages<TValue>(RhinoDoc doc, Seq<PageSource> sources, Func<Seq<(IO<ViewCaptureSettings> Settings, Option<RhinoObject> Context)>, IO<TValue>> body) =>
        sources.TraverseM(source => Planned(doc, source)).As().Map(static planned => planned.Flatten()).Bind(body).Bracket();

    private static IO<ViewCaptureSettings> Decorated(RhinoDoc doc, (IO<ViewCaptureSettings> Settings, Option<RhinoObject> Context) page, PageText text) =>
        from settings in page.Settings
        from decorated in Products.Each(Seq(
            FieldFormulas.Format(doc, FieldFormulas.Text(text.Header), obj: page.Context, topParent: page.Context).Bind(header => IO.lift(() => { settings.HeaderText = header; })),
            FieldFormulas.Format(doc, FieldFormulas.Text(text.Footer), obj: page.Context, topParent: page.Context).Bind(footer => IO.lift(() => { settings.FooterText = footer; }))))
        select settings;

    // --- [SHOTS]
    private static IO<TValue> Shots<TValue>(RhinoDoc doc, Seq<RasterSource> sources, Func<Seq<IO<(Bitmap Bitmap, Option<double> Density)>>, IO<TValue>> body) =>
        sources.TraverseM(source => source.Switch(
                doc,
                printed: static (document, printed) => Planned(document, printed.Source).Map(static pages => pages.Map(static page =>
                    from settings in page.Settings
                    from bitmap in Products.Of(() => ViewCapture.CaptureToBitmap(settings))
                    select (bitmap, Some(settings.Resolution)))),
                screen: static (document, screen) => Views(document, screen.Set).Map(views => views.Map(view =>
                    Products.Of(() => screen.Capture.CaptureToBitmap(view)).Map(static bitmap => (bitmap, Option<double>.None)))),
                mode: static (document, mode) => Views(document, mode.Set).Map(views => views.Map(view =>
                    (from description in Products.Of(() => DisplayModeDescription.GetDisplayMode(mode.Id))
                     from edited in IO.lift(() => mode.Edits.Iter(edit => edit(description.DisplayAttributes)))
                     from bitmap in Products.Of(() => view.CaptureToBitmap(mode.Pixels.IfNone(() => view.ActiveViewport.Size), description.DisplayAttributes))
                     select (bitmap, Option<double>.None)).Bracket())),
                posed: static (document, posed) => IO.pure(posed.Poses.Map(pose =>
                    Viewports.With(document, posed.Viewport, row => Cameras.Within(row.Viewport, camera => IO.lift(() => Cameras.Pose(camera, pose)).Map(unit),
                        Products.Of(() => row.View.CaptureToBitmap(posed.Pixels.IfNone(() => row.Viewport.Size))).Map(static bitmap => (bitmap, Option<double>.None)))))),
                depth: static (document, depth) => IO.pure(Seq(
                    Viewports.With(document, depth.Viewport, row => ZBuffer.Capture(row.Viewport, depth.Setup, static buffer => buffer.Grayscale))
                        .Map(static bitmap => (bitmap, Option<double>.None)))))).As()
            .Map(static planned => planned.Flatten()).Bind(body).Bracket();

    private static IO<Seq<RhinoView>> Views(RhinoDoc doc, ViewportSet set) =>
        Viewports.Resolve(doc, set).Map(static rows => toSeq(rows.Map(static row => row.View).DistinctBy(static view => view.RuntimeSerialNumber)).Strict());

    // --- [TARGETS]
    private static int Pixels(Length length, double density) => int.CreateChecked(double.Round(length.Inches.ToDouble() * density, MidpointRounding.ToEven));

    private static IO<Unit> Added(RhinoDoc doc, FilePdf pdf, PageText text, bool layers, (IO<ViewCaptureSettings> Settings, Option<RhinoObject> Context) page, Seq<PageMark> marks) =>
        (from settings in Decorated(doc, page, text)
         from number in IO.lift(() => {
             pdf.LayersAsOptionalContentGroups = layers && !settings.RasterMode;
             return pdf.AddPage(settings);
         })
         from drawn in marks.TraverseM(mark => mark.Draw(new PdfPage(doc, pdf, number, settings.Resolution, settings.MediaSize.Height, page.Context))).As()
         select unit).Bracket();

    private static IO<Unit> Blank(RhinoDoc doc, FilePdf pdf, PdfSource.Blank blank) =>
        (Dpi: Pixels(Length.FromInches(1), blank.Density), Width: Pixels(blank.Sheet.Extent.Width, blank.Density), Height: Pixels(blank.Sheet.Extent.Height, blank.Density)) switch {
            var page =>
                from number in IO.lift(() => pdf.AddPage(page.Width, page.Height, page.Dpi))
                from drawn in blank.Marks.TraverseM(mark => mark.Draw(new PdfPage(doc, pdf, number, blank.Density, page.Height, None))).As()
                select unit,
        };

    private static IO<FilePdf> Composed(RhinoDoc doc, Seq<PdfSource> sources, PageText text, bool layers) =>
        from pdf in Products.Of(static () => FilePdf.Create())
        from added in sources.TraverseM(source => source.Switch(
            (Doc: doc, Pdf: pdf, Text: text, Layers: layers),
            captured: static (state, captured) => Pages(state.Doc, Seq(captured.Source), pages =>
                pages.TraverseM(page => Added(state.Doc, state.Pdf, state.Text, state.Layers, page, captured.Marks)).As().Map(unit)),
            blank: static (state, blank) => Blank(state.Doc, state.Pdf, blank))).As()
        select pdf;

    private static Fin<Seq<OutputName>> Names(int count, FileExtension extension, FileOutput output) =>
        toSeq(Range(0, count)).Traverse(index => count > 1 ? SequenceNumber.Validated(index).Map(Some) : Option<SequenceNumber>.None)
            .Map(numbers => numbers.Map(number => new OutputName(output.Scope, output.Version, Seq<NamePart>(), number, extension))).As();

    private static IO<Seq<(OutputName Name, OutputPath Path)>> Written<T>(RhinoDoc doc, Seq<T> planned, FileExtension extension, FileOutput output, Func<T, OutputPath, IO<Unit>> write) =>
        from names in IO.lift(() => Names(planned.Count, extension, output))
        from rows in Destinations.Resolve(doc, output.Folder, output.Stem, output.Overwrite, output.Convention, names)
        from written in planned.Zip(rows).TraverseM(pair => write(pair.First, pair.Second.Path)).As()
        select rows;

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToPdf(RhinoDoc doc, IterableNE<PdfSource> sources, PageText text, bool layers, FileOutput output) =>
        from names in IO.lift(() => Names(1, FileExtension.Create(".pdf"), output))
        from rows in Destinations.Resolve(doc, output.Folder, output.Stem, output.Overwrite, output.Convention, names)
        from pdf in Composed(doc, toSeq(sources), text, layers)
        from written in rows.TraverseM(row => FilePublication.Write(row.Path, output.Overwrite, stream => IO.lift(() => pdf.Write(stream)))).As()
        select rows;

    public static IO<Unit> ToPdf(RhinoDoc doc, IterableNE<PdfSource> sources, PageText text, bool layers, Stream target) =>
        Composed(doc, toSeq(sources), text, layers).Bind(pdf => IO.lift(() => pdf.Write(target)));

    public static IO<Unit> ToPrinter(RhinoDoc doc, IterableNE<PageSource> sources, PageText text, string printer, int copies) =>
        Pages(doc, toSeq(sources), pages =>
            pages.TraverseM(page => Decorated(doc, page, text)).As().Bind(settings =>
                Products.Accepted(() => ViewCapture.SendToPrinter(printer, [.. settings], copies))));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToRasters(
        RhinoDoc doc, IterableNE<RasterSource> sources, FileExtension extension, Func<Bitmap, Option<double>, Stream, IO<Unit>> encode, FileOutput output) =>
        Shots(doc, toSeq(sources), shots => Written(doc, shots, extension, output, (shot, path) =>
            shot.Bind(captured => FilePublication.Write(path, output.Overwrite, stream => encode(captured.Bitmap, captured.Density, stream))).Bracket()));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToSvg(RhinoDoc doc, IterableNE<PageSource> sources, PageText text, FileOutput output) =>
        Pages(doc, toSeq(sources), pages => Written(doc, pages, FileExtension.Create(".svg"), output, (page, path) =>
            Decorated(doc, page, text).Bind(settings =>
                FilePublication.Write(path, output.Overwrite, stream => IO.lift(() => ViewCapture.CaptureToSvg(settings).Save(stream)))).Bracket()));
}
