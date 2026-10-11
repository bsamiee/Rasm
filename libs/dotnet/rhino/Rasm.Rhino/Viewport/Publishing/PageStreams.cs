using Rasm.Drafting;
using Rasm.Imaging.Output;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Rhino.Viewport.Publishing;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record PageSource {
    public sealed record ViewSet(ViewportSet Set, CaptureRequest Request) : PageSource;

    public sealed record PageDetails(ViewportSet Pages, CaptureRequest Request) : PageSource;

    public sealed record Named(string Name, ViewportTarget Viewport, CaptureSubject.View Frame) : PageSource;
}

[Union]
public abstract partial record PdfSource {
    public sealed record Captured(PageSource Source, Seq<PageMark> Marks) : PdfSource;

    public sealed record Blank(Sheet Sheet, Seq<PageMark> Marks) : PdfSource;
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

public sealed record FileOutput(Destination Destination, Seq<NamePart> Scope, OutputVersioning Version);

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct CopyCount : System.Numerics.IMinMaxValue<CopyCount> {
    public static CopyCount MinValue { get; } = new(1);
    public static CopyCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PageStreams {
    // --- [PAGES]
    private sealed record CapturedPage(Func<Action<IDisposable>, IO<ViewCaptureSettings>> Settings, Option<RhinoObject> Parent);

    private static IO<(PageSource Source, Seq<ViewportRef> Rows)> Held(RhinoDoc doc, PageSource source, Action<IDisposable> retain) =>
        source.Switch(
            (Document: doc, Retain: retain),
            viewSet: static (state, views) => Viewports.ResolveViewports(state.Document, views.Set, state.Retain),
            pageDetails: static (state, details) => DisposalOps.Within((Action<IDisposable> owned) =>
                Viewports.ResolveViewports(state.Document, details.Pages, owned).Bind(pages => Copies.Owned(() =>
                    (from page in pages.Filter(static row => row.Detail.IsNone).Choose(static row => Optional(row.View as RhinoPageView))
                     from detail in toSeq(page.GetDetailViews())
                     select new ViewportRef(page, Some(detail))).Strict(),
                    static rows => Conversions.NonEmpty(rows, nameof(RhinoPageView.GetDetailViews)),
                    DisposalOps.Release, state.Retain)), DisposalOps.Release),
            named: static (state, named) => Viewports.ResolveViewport(state.Document, named.Viewport, state.Retain).Map(static row => Seq(row)))
        .Map(rows => (source, rows));

    private static IO<Option<RhinoObject>> Parented(RhinoDoc doc, ViewportRef row) =>
        row switch {
            { Detail.IsSome: true } => IO.pure(row.Detail.Map<RhinoObject>(static detail => detail)),
            { View: RhinoPageView page } => new ObjectTarget.Lookup(table => table.GetObjectList(new ObjectEnumeratorSettings { SpaceFilter = ActiveSpace.PageSpace, ViewportFilter = page.MainViewport })).Objects(doc).Map(static found => found.Head),
            _ => IO.pure(Option<RhinoObject>.None),
        };

    private static IO<ViewCaptureSettings> Projected(RhinoDoc doc, ViewportRef row, ViewportInfo camera, CaptureSubject.View frame, Action<IDisposable> retain) =>
        Copies.Owned(() => new ViewCaptureSettings(row.View, frame.Media, frame.Dpi), settings =>
            DisposalOps.Within((Action<IDisposable> temporary) =>
                from mode in Copies.Owned(() => row.Viewport.DisplayMode,
                    static made => Missing.Unless(made, nameof(RhinoViewport.DisplayMode)),
                    static made => DisposalOps.Release(Conversions.Rows(new[] { made })), temporary)
                from viewport in Copies.Owned(static () => new RhinoViewport(), static made => Fin.Succ(made), temporary)
                from restored in IO.lift(() => {
                    viewport.Size = frame.Media;
                    viewport.DisplayMode = mode;
                    return Cameras.Restore(viewport, camera, camera.TargetPoint);
                })
                from valid in IO.lift(() => {
                    settings.Document = doc;
                    settings.SetViewport(viewport);
                    return Refused.Unless(settings.IsValid, settings, nameof(settings));
                })
                select valid,
                DisposalOps.Release), retain);

    private static IO<Seq<CapturedPage>> Requested(RhinoDoc doc, Seq<ViewportRef> rows, CaptureRequest request) =>
        rows.TraverseM(row => Parented(doc, row).Map(parent => new CapturedPage(retain => ViewCaptures.Settings(row, request, retain), parent))).As();

    private static IO<Seq<CapturedPage>> Planned(RhinoDoc doc, PageSource source, Seq<ViewportRef> rows) =>
        source.Switch(
            (Doc: doc, Rows: rows),
            viewSet: static (held, views) => Requested(held.Doc, held.Rows, views.Request),
            pageDetails: static (held, details) => Requested(held.Doc, held.Rows, details.Request),
            named: static (held, named) => IO.pure(
                from row in held.Rows
                select new CapturedPage(retain => NamedViews.Read(held.Doc, named.Name, view => Projected(held.Doc, row, view.Viewport, named.Frame, retain)), None)));

    private static IO<TValue> Pages<TValue>(RhinoDoc doc, Seq<PageSource> sources, Func<Seq<CapturedPage>, IO<TValue>> body) =>
        from live in IO.lift(() => HeadlessCapture.Unless(!doc.IsHeadless))
        from result in DisposalOps.Within((Action<IDisposable> retain) =>
            sources.TraverseM(source => Held(doc, source, retain)).As()
                .Bind(held => held.TraverseM(entry => Planned(doc, entry.Source, entry.Rows)).As())
                .Map(static planned => planned.Flatten()).Bind(body),
            DisposalOps.Release)
        select result;

    private static IO<string> Formatted(RhinoDoc doc, Seq<FieldRun> runs, Option<RhinoObject> parent) =>
        TextFieldOps.Format(doc, TextFieldOps.Text(runs), obj: parent, topParent: parent);

    private static IO<ViewCaptureSettings> Decorated(RhinoDoc doc, CapturedPage page, PageText text, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from settings in page.Settings(owned)
            from decorated in Callbacks.Each(Seq(
                Formatted(doc, text.Header, page.Parent).Bind(header => IO.lift(() => { settings.HeaderText = header; })),
                Formatted(doc, text.Footer, page.Parent).Bind(footer => IO.lift(() => { settings.FooterText = footer; }))))
            select settings,
            (_, lease) => retain(lease));

    // --- [DRAWING]
    private static float Dots(Length length, double dpi) => (float)(length.Inches.ToDouble() * dpi);

    private static System.Drawing.PointF Placed((Length X, Length Y) point, (double Dpi, int Height) page) =>
        new(Dots(point.X, page.Dpi), page.Height - Dots(point.Y, page.Dpi));

    private static IO<Unit> Drawn(RhinoDoc doc, FilePdf pdf, int number, (double Dpi, int Height) page, Option<RhinoObject> parent, PageMark mark) =>
        mark.Switch(
            (Doc: doc, Pdf: pdf, Number: number, Page: page, Parent: parent),
            textMark: static (state, text) =>
                from words in Formatted(state.Doc, text.Runs, state.Parent)
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
    private static int Points(Length length) => (int)UnitsNet.QuantityValue.Round(length.DtpPoints, MidpointRounding.ToEven);

    private static IO<Unit> Added(RhinoDoc doc, FilePdf pdf, PageText text, bool layers, CapturedPage page, Seq<PageMark> marks) =>
        DisposalOps.Within((Action<IDisposable> retain) =>
         from settings in Decorated(doc, page, text, retain)
         from number in IO.lift(() => {
             pdf.LayersAsOptionalContentGroups = layers && !settings.RasterMode;
             return pdf.AddPage(settings);
         })
         from drawn in marks.TraverseM(mark => Drawn(doc, pdf, number, (settings.Resolution, settings.MediaSize.Height), page.Parent, mark)).As()
         select unit, DisposalOps.Release);

    private static IO<Unit> Blank(RhinoDoc doc, FilePdf pdf, Sheet sheet, Seq<PageMark> marks) =>
        (Dpi: Points(Length.FromInches(1)), Width: Points(sheet.Extent.Width), Height: Points(sheet.Extent.Height)) switch {
            var page =>
                from number in IO.lift(() => pdf.AddPage(page.Width, page.Height, page.Dpi))
                from drawn in marks.TraverseM(mark => Drawn(doc, pdf, number, (page.Dpi, page.Height), None, mark)).As()
                select unit,
        };

    private static IO<FilePdf> Composed(RhinoDoc doc, Seq<PdfSource> sources, PageText text, bool layers) =>
        from pdf in IO.lift(static () => Missing.Unless(FilePdf.Create(), nameof(FilePdf.Create)))
        from added in sources.TraverseM(source => source.Switch(
            (Doc: doc, Pdf: pdf, Text: text, Layers: layers),
            captured: static (state, captured) => Pages(state.Doc, Seq(captured.Source), pages =>
                pages.TraverseM(page => Added(state.Doc, state.Pdf, state.Text, state.Layers, page, captured.Marks)).As().Map(static _ => unit)),
            blank: static (state, blank) => Blank(state.Doc, state.Pdf, blank.Sheet, blank.Marks))).As()
        select pdf;

    private static Seq<OutputName> Names(int count, FileExtension extension, FileOutput output) =>
        toSeq(Range(0, count)).Map(index => new OutputName(output.Scope, output.Version, Seq<NamePart>(), Callbacks.Found(count > 1, SequenceNumber.Create(index)), extension));

    private static IO<Unit> Saved(OutputPath path, Action write) =>
        IO.lift(write).Catch(
            static error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>(),
            error => IO.fail<Unit>(new PublishRefused(path, error)));

    private static IO<Seq<(OutputName Name, OutputPath Path)>> Written(
        RhinoDoc doc, Seq<PageSource> sources, FileExtension extension, FileOutput output, Func<ViewCaptureSettings, OutputPath, IO<Unit>> write) =>
        Pages(doc, sources, pages =>
            from rows in Destinations.Resolve(doc, output.Destination, Names(pages.Count, extension, output))
            from written in pages.Zip(rows).TraverseM(pair => DisposalOps.Within((Action<IDisposable> retain) =>
                pair.First.Settings(retain).Bind(settings => write(settings, pair.Second.Path)), DisposalOps.Release)).As()
            select rows);

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToPdf(RhinoDoc doc, IterableNE<PdfSource> sources, PageText text, bool layers, FileOutput output) =>
        from rows in Destinations.Resolve(doc, output.Destination, Names(1, FileExtension.Create(".pdf"), output))
        from pdf in Composed(doc, toSeq(sources), text, layers)
        from written in rows.TraverseM(row => Saved(row.Path, () => pdf.Write(row.Path))).As()
        select rows;

    public static IO<Unit> ToPdf(RhinoDoc doc, IterableNE<PdfSource> sources, PageText text, bool layers, Stream target) =>
        Composed(doc, toSeq(sources), text, layers).Bind(pdf => IO.lift(() => pdf.Write(target)));

    public static IO<Unit> ToPrinter(RhinoDoc doc, IterableNE<PageSource> sources, PageText text, string printer, CopyCount copies) =>
        Pages(doc, toSeq(sources), pages => DisposalOps.Within((Action<IDisposable> retain) =>
            pages.TraverseM(page => Decorated(doc, page, text, retain)).As().Bind(settings =>
                IO.lift(() => Refused.Unless(ViewCapture.SendToPrinter(printer, [.. settings], copies), nameof(ViewCapture.SendToPrinter)))),
            DisposalOps.Release));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToRasters(RhinoDoc doc, IterableNE<PageSource> sources, RasterEncoding encoding, FileOutput output) =>
        Written(doc, toSeq(sources), encoding.Extension, output, (settings, path) =>
            DisposalOps.Within(() => ViewCapture.CaptureToBitmap(settings), bitmap =>
                IO.lift(() => Missing.Unless(bitmap, nameof(ViewCapture.CaptureToBitmap))).Bind(held =>
                    IO.lift(() => held.Save(path, encoding.Format))),
                static bitmap => DisposalOps.Release(Conversions.Rows(new[] { bitmap }))));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> ToSvg(RhinoDoc doc, IterableNE<PageSource> sources, FileOutput output) =>
        Written(doc, toSeq(sources), FileExtension.Create(".svg"), output, static (settings, path) =>
            Saved(path, () => ViewCapture.CaptureToSvg(settings).Save(path)));
}
