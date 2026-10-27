using System.Runtime.CompilerServices;
using Rasm.Drafting;
using Rasm.Rhino.Display;
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
    public sealed record ViewSet(ViewportSet Set, CaptureSubject Subject, CaptureRequest Request, Option<ViewportType> Kind = default) : PageSource;

    public sealed record Named(string Name, ViewportTarget Viewport, CaptureSubject.View Frame, CaptureRequest Request) : PageSource;
}

[Union]
public abstract partial record RasterSource {
    public sealed record Printed(PageSource Source) : RasterSource;

    public sealed record Screen(ViewportSet Set, ViewCapture Capture) : RasterSource;

    public sealed record Mode(ViewportSet Set, Guid Id, Option<System.Drawing.Size> Pixels, Seq<ModeEdit> Edits) : RasterSource;
}

[Union]
public abstract partial record PdfSource {
    public sealed record Captured(PageSource Source, Seq<PageMark> Marks) : PdfSource;

    public sealed record Blank(Sheet Sheet, double Density, Seq<PageMark> Marks) : PdfSource;
}

[Union]
public abstract partial record PageMark {
    public sealed record TextMark(
        Seq<FieldRun> Runs, (Length X, Length Y) Anchor, Length Height, Font Font, System.Drawing.Color Fill,
        Option<(System.Drawing.Color Color, Length Width)> Outline, double Rotation,
        TextHorizontalAlignment Horizontal, TextVerticalAlignment Vertical) : PageMark;

    public sealed record LineMark(
        (Length X, Length Y) Start, (Length X, Length Y) Next, Seq<(Length X, Length Y)> Rest,
        Option<System.Drawing.Color> Fill, System.Drawing.Color Stroke, Length Width) : PageMark;

    public sealed record ImageMark(System.Drawing.Bitmap Bitmap, (Length X, Length Y) Corner, (Length Width, Length Height) Extent, double Rotation) : PageMark;
}

public sealed record PageText(Seq<FieldRun> Header, Seq<FieldRun> Footer);

public sealed record FileOutput(OutputFolder Folder, Stem Stem, bool Overwrite, Seq<NamePart> Scope, OutputVersioning Version);

internal sealed record PlannedPage(IO<ViewCaptureSettings> Settings, Option<RhinoObject> Context);

internal sealed record PlannedShot(IO<(System.Drawing.Bitmap Bitmap, Option<double> Density)> Capture);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PageStreams {
    // --- [PAGES]
    private static IO<Seq<PlannedPage>> Planned(RhinoDoc doc, PageSource source) =>
        source.Switch(
            doc,
            viewSet: static (document, views) =>
                from rows in Viewports.Resolve(document, views.Set)
                from pages in rows.Filter(row => views.Kind.ForAll(kind => row.Viewport.ViewportType == kind)).TraverseM(row =>
                    Context(document, row).Map(context => new PlannedPage(ViewCaptures.Settings(row, views.Subject, views.Request), context))).As()
                select pages,
            named: static (document, named) =>
                from row in Viewports.Resolve(document, named.Viewport)
                from index in NamedViews.Find(document, named.Name)
                from context in Context(document, row)
                select Seq(new PlannedPage(Projected(document, row, index, named), context)));

    private static IO<Option<RhinoObject>> Context(RhinoDoc doc, ViewportRef row) =>
        row.View is RhinoPageView page
            ? new ObjectTarget.Lookup(table => table.GetObjectList(
                    new ObjectEnumeratorSettings { SpaceFilter = ActiveSpace.PageSpace, ViewportFilter = page.MainViewport, HiddenObjects = true, LockedObjects = true }))
                .Objects(doc).Map(static found => found.Head)
            : IO.pure(Option<RhinoObject>.None);

    private static IO<ViewCaptureSettings> Projected(RhinoDoc doc, ViewportRef row, int index, PageSource.Named named) =>
        from mode in Products.Made(row.Viewport.DisplayMode)
        from free in use(() => new ViewportRef(row.View, new RhinoViewport(), None))
        from framed in IO.lift(() => {
            free.Viewport.Size = named.Frame.Media;
            free.Viewport.DisplayMode = mode;
        })
        from restored in NamedViews.Applied(doc, index, free.Viewport, new RestorePacing.Immediate())
        from settings in ViewCaptures.Settings(free, named.Frame, named.Request)
        select settings;

    private static IO<TValue> Pages<TValue>(RhinoDoc doc, Seq<PageSource> sources, Func<Seq<PlannedPage>, IO<TValue>> body) =>
        sources.TraverseM(source => Planned(doc, source)).As().Map(static planned => planned.Flatten()).Bind(body).Bracket();

    private static IO<string> Formatted(RhinoDoc doc, Seq<FieldRun> runs, Option<RhinoObject> context) =>
        TextFieldOps.Format(doc, TextFieldOps.Text(runs), obj: context, topParent: context);

    private static IO<ViewCaptureSettings> Decorated(RhinoDoc doc, PlannedPage page, PageText text) =>
        from settings in page.Settings
        from decorated in Callbacks.Each(Seq(
            Formatted(doc, text.Header, page.Context).Bind(header => IO.lift(() => { settings.HeaderText = header; })),
            Formatted(doc, text.Footer, page.Context).Bind(footer => IO.lift(() => { settings.FooterText = footer; }))))
        select settings;

    // --- [SHOTS]
    private static IO<TValue> Shots<TValue>(RhinoDoc doc, Seq<RasterSource> sources, Func<Seq<PlannedShot>, IO<TValue>> body) =>
        sources.TraverseM(source => source.Switch(
                doc,
                printed: static (document, printed) => Planned(document, printed.Source).Map(static pages => pages.Map(static page => new PlannedShot(
                    from settings in page.Settings
                    from bitmap in Rastered(() => ViewCapture.CaptureToBitmap(settings))
                    select (bitmap, Some(settings.Resolution))))),
                screen: static (document, screen) => Views(document, screen.Set).Map(views => views.Map(view => new PlannedShot(
                    Rastered(() => screen.Capture.CaptureToBitmap(view)).Map(static bitmap => (bitmap, Option<double>.None))))),
                mode: static (document, mode) => Views(document, mode.Set).Map(views => views.Map(view => new PlannedShot(
                    DisplayModes.CaptureToBitmap(view, mode.Id, mode.Pixels, mode.Edits).Map(static bitmap => (bitmap, Option<double>.None))))))).As()
            .Map(static planned => planned.Flatten()).Bind(body).Bracket();

    private static IO<Seq<RhinoView>> Views(RhinoDoc doc, ViewportSet set) =>
        Viewports.Resolve(doc, set).Map(static rows => toSeq(rows.Map(static row => row.View).DistinctBy(static view => view.RuntimeSerialNumber)).Strict());

    private static IO<System.Drawing.Bitmap> Rastered(Func<System.Drawing.Bitmap?> capture, [CallerArgumentExpression(nameof(capture))] string call = "") =>
        unitIO.Bind(_ => Products.Made(capture(), call));

    // --- [DRAWING]
    private static float Dots(Length length, double dpi) => (float)(length.Inches.ToDouble() * dpi);

    private static System.Drawing.PointF Placed((Length X, Length Y) point, (double Dpi, int Height) page) =>
        new(Dots(point.X, page.Dpi), page.Height - Dots(point.Y, page.Dpi));

    private static IO<Unit> Drawn(RhinoDoc doc, FilePdf pdf, int number, (double Dpi, int Height) page, Option<RhinoObject> context, PageMark mark) =>
        mark.Switch(
            (Doc: doc, Pdf: pdf, Number: number, Page: page, Context: context),
            textMark: static (state, text) =>
                from words in Formatted(state.Doc, text.Runs, state.Context)
                let at = Placed(text.Anchor, state.Page)
                let outline = text.Outline.IfNone((System.Drawing.Color.Empty, Length.Zero))
                from drawn in IO.lift(() => state.Pdf.DrawText(
                    state.Number, words, at.X, at.Y, (float)text.Height.DtpPoints.ToDouble(), text.Font, text.Fill,
                    outline.Color, (float)outline.Width.DtpPoints.ToDouble(),
                    (float)double.RadiansToDegrees(text.Rotation), text.Horizontal, text.Vertical))
                select drawn,
            lineMark: static (state, line) => IO.lift(() => state.Pdf.DrawPolyline(
                state.Number,
                [.. from point in line.Start.Cons(line.Next.Cons(line.Rest)) select Placed(point, state.Page)],
                Conversions.Unset(line.Fill), line.Stroke, Dots(line.Width, state.Page.Dpi))),
            imageMark: static (state, image) =>
                from at in IO.pure(Placed((image.Corner.X, image.Corner.Y + image.Extent.Height), state.Page))
                from drawn in IO.lift(() => state.Pdf.DrawBitmap(
                    state.Number, image.Bitmap, at.X, at.Y, Dots(image.Extent.Width, state.Page.Dpi), Dots(image.Extent.Height, state.Page.Dpi),
                    (float)double.RadiansToDegrees(image.Rotation)))
                select drawn);

    // --- [TARGETS]
    private static int Pixels(Length length, double density) => int.CreateChecked(double.Round(length.Inches.ToDouble() * density, MidpointRounding.ToEven));

    private static IO<Unit> Added(RhinoDoc doc, FilePdf pdf, PageText text, bool layers, PlannedPage page, Seq<PageMark> marks) =>
        (from settings in Decorated(doc, page, text)
         from number in IO.lift(() => {
             pdf.LayersAsOptionalContentGroups = layers && !settings.RasterMode;
             return pdf.AddPage(settings);
         })
         from drawn in marks.TraverseM(mark => Drawn(doc, pdf, number, (settings.Resolution, settings.MediaSize.Height), page.Context, mark)).As()
         select unit).Bracket();

    private static IO<Unit> Blank(RhinoDoc doc, FilePdf pdf, PdfSource.Blank blank) =>
        (Dpi: Pixels(Length.FromInches(1), blank.Density), Width: Pixels(blank.Sheet.Extent.Width, blank.Density), Height: Pixels(blank.Sheet.Extent.Height, blank.Density)) switch {
            var page =>
                from number in IO.lift(() => pdf.AddPage(page.Width, page.Height, page.Dpi))
                from drawn in blank.Marks.TraverseM(mark => Drawn(doc, pdf, number, (blank.Density, page.Height), None, mark)).As()
                select unit,
        };

    private static IO<FilePdf> Composed(RhinoDoc doc, Seq<PdfSource> sources, PageText text, bool layers) =>
        from pdf in IO.lift(static () => Conversions.Found(FilePdf.Create()))
        from added in sources.TraverseM(source => source.Switch(
            (Doc: doc, Pdf: pdf, Text: text, Layers: layers),
            captured: static (state, captured) => Pages(state.Doc, Seq(captured.Source), pages =>
                pages.TraverseM(page => Added(state.Doc, state.Pdf, state.Text, state.Layers, page, captured.Marks)).As().Map(unit)),
            blank: static (state, blank) => Blank(state.Doc, state.Pdf, blank))).As()
        select pdf;

    private static Fin<Seq<OutputName>> Names(int count, FileExtension extension, FileOutput output) =>
        toSeq(Range(0, count)).Traverse(index => count > 1 ? SequenceNumber.TryCreate(index, out SequenceNumber? number, out RhinoFault? fault) ? Fin.Succ(Some(number)) : fault : Option<SequenceNumber>.None)
            .Map(numbers => numbers.Map(number => new OutputName(output.Scope, output.Version, Seq<NamePart>(), number, extension))).As();

    private static IO<Seq<(OutputName Name, OutputPath Path)>> Written<T>(RhinoDoc doc, Seq<T> planned, FileExtension extension, FileOutput output, Func<T, OutputPath, IO<Unit>> write) =>
        from names in IO.lift(Names(planned.Count, extension, output))
        from rows in Destinations.Resolve(doc, output.Folder, output.Stem, output.Overwrite, names)
        from written in planned.Zip(rows).TraverseM(pair => write(pair.First, pair.Second.Path)).As()
        select rows;

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToPdf(RhinoDoc doc, IterableNE<PdfSource> sources, PageText text, bool layers, FileOutput output) =>
        from names in IO.lift(Names(1, FileExtension.Create(".pdf"), output))
        from rows in Destinations.Resolve(doc, output.Folder, output.Stem, output.Overwrite, names)
        from pdf in Composed(doc, toSeq(sources), text, layers)
        from written in rows.TraverseM(row => FilePublication.Write(row.Path, output.Overwrite, stream => IO.lift(() => pdf.Write(stream)))).As()
        select rows;

    public static IO<Unit> ToPdf(RhinoDoc doc, IterableNE<PdfSource> sources, PageText text, bool layers, Stream target) =>
        Composed(doc, toSeq(sources), text, layers).Bind(pdf => IO.lift(() => pdf.Write(target)));

    public static IO<Unit> ToPrinter(RhinoDoc doc, IterableNE<PageSource> sources, PageText text, string printer, int copies) =>
        Pages(doc, toSeq(sources), pages =>
            pages.TraverseM(page => Decorated(doc, page, text)).As().Bind(settings =>
                IO.lift(() => RhinoFault.Unless(ViewCapture.SendToPrinter(printer, [.. settings], copies)))));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToRasters(
        RhinoDoc doc, IterableNE<RasterSource> sources, FileExtension extension, Func<System.Drawing.Bitmap, Option<double>, Stream, IO<Unit>> encode, FileOutput output) =>
        Shots(doc, toSeq(sources), shots => Written(doc, shots, extension, output, (shot, path) =>
            shot.Capture.Bind(captured => FilePublication.Write(path, output.Overwrite, stream => encode(captured.Bitmap, captured.Density, stream))).Bracket()));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToSvg(RhinoDoc doc, IterableNE<PageSource> sources, PageText text, FileOutput output) =>
        Pages(doc, toSeq(sources), pages => Written(doc, pages, FileExtension.Create(".svg"), output, (page, path) =>
            Decorated(doc, page, text).Bind(settings =>
                FilePublication.Write(path, output.Overwrite, stream => IO.lift(() => ViewCapture.CaptureToSvg(settings).Save(stream)))).Bracket()));
}
