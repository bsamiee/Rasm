using System.Drawing;
using System.Xml;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.FileIO;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSubject {
    public sealed record View(Size Pixels, double Dpi) : CaptureSubject;

    public sealed record Page(double Dpi) : CaptureSubject;

    public sealed record Preview(CaptureSubject Source, Size Pixels) : CaptureSubject;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureArea {
    public sealed record FullView() : CaptureArea;

    public sealed record Extents() : CaptureArea;

    public sealed record ScreenWindow(Point2d First, Point2d Second) : CaptureArea;

    public sealed record WorldWindow(Point3d First, Point3d Second) : CaptureArea;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureScale {
    public sealed record Native() : CaptureScale;

    public sealed record ToValue(double Scale) : CaptureScale;

    public sealed record ToFit() : CaptureScale;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MediaLayout {
    public sealed record Bound() : MediaLayout;

    public sealed record Crop(Size Media, Rectangle CropRectangle) : MediaLayout;

    public sealed record Margins(UnitSystem Units, double Left, double Top, double Right, double Bottom) : MediaLayout;

    public sealed record Maximize() : MediaLayout;
}

public sealed record MediaOffset(UnitSystem Units, bool FromMargin, double X, double Y);

public sealed record MediaPlacement(Option<MediaOffset> Offset, Option<ViewCaptureSettings.AnchorLocation> Anchor, bool MatchViewportAspect);

public sealed record CaptureRequest(CaptureSubject Subject, CaptureArea Area, CaptureScale Scale, MediaLayout Layout, MediaPlacement Placement);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PdfPage {
    public abstract Func<FilePdf, int, IO<Unit>> Draw { get; init; }

    public sealed record Capture(ViewCaptureSettings Settings, Func<FilePdf, int, IO<Unit>> Draw) : PdfPage;

    public sealed record Blank(int WidthInDots, int HeightInDots, int DotsPerInch, Func<FilePdf, int, IO<Unit>> Draw) : PdfPage;
}

public sealed record PdfOptions(bool LayersAsOptionalContentGroups, Option<Func<FilePdf, IO<Unit>>> PreWrite, Action<Error> Reject);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DepthRequest {
    public sealed record Stats() : DepthRequest;

    public sealed record Samples(Seq<System.Drawing.Point> Pixels) : DepthRequest;

    public sealed record Grayscale() : DepthRequest;
}

public sealed record DepthSample(System.Drawing.Point Pixel, float Z, Point3d World);

public sealed record DepthRange(float MinZ, float MaxZ);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DepthOutput {
    public sealed record Stats() : DepthOutput;

    public sealed record Samples(Seq<DepthSample> Values) : DepthOutput;

    public sealed record Grayscale(Bitmap Image) : DepthOutput;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DepthHits {
    public sealed record Empty() : DepthHits;

    public sealed record Hit(int Hits, DepthRange Range) : DepthHits;
}

public sealed record DepthCapture(DepthHits Hits, DepthOutput Output);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Captures {
    // --- [SETTINGS]
    public static IO<TValue> WithSettings<TValue>(Seq<(ViewportRef Row, CaptureRequest Request)> pages, Func<Seq<ViewCaptureSettings>, IO<TValue>> body) =>
        Disposal.Using(Disposal.AcquireAll(pages.Map(static page => Prepare(page.Row, page.Request))), body);

    // --- [MEDIA]
    public static IO<Bitmap> ToBitmap(ViewCaptureSettings settings) =>
        IO.lift(() => Missing.Unless(ViewCapture.CaptureToBitmap(settings), nameof(ViewCapture.CaptureToBitmap)));

    public static IO<XmlDocument> ToSvg(ViewCaptureSettings settings) =>
        IO.lift(() => Missing.Unless(ViewCapture.CaptureToSvg(settings), nameof(ViewCapture.CaptureToSvg)));

    public static IO<Unit> SendToPrinter(string printerName, Seq<ViewCaptureSettings> pages, int copies) =>
        IO.lift(() => Refused.Unless(ViewCapture.SendToPrinter(printerName, [.. pages], copies), nameof(ViewCapture.SendToPrinter)));

    public static IO<Unit> ToPdf(string path, Seq<PdfPage> pages, PdfOptions options) =>
        from pdf in IO.lift(static () => Missing.Unless(FilePdf.Create(), nameof(FilePdf.Create)))
        from added in pages.TraverseM(page => Added(pdf, page, options.LayersAsOptionalContentGroups)).As()
        from written in Disposal.Using(
            options.PreWrite.Match(
                Some: preWrite => Events.Attach(
                    static handler => FilePdf.PreWrite += handler,
                    static handler => FilePdf.PreWrite -= handler,
                    Answers.Handler<FilePdfEventArgs>(args => when(ReferenceEquals(args.Pdf, pdf), preWrite(pdf)).As(), options.Reject)),
                None: static () => IO.pure(Thinktecture.Empty.Disposable())),
            _ => IO.lift(() => pdf.Write(path)))
        select unit;

    private static IO<Unit> Added(FilePdf pdf, PdfPage page, bool layersAsOptionalContentGroups) =>
        page.Switch(
            (Pdf: pdf, Layers: layersAsOptionalContentGroups),
            capture: static (state, capture) => IO.lift(() => {
                state.Pdf.LayersAsOptionalContentGroups = state.Layers && !capture.Settings.RasterMode;
                return state.Pdf.AddPage(capture.Settings);
            }).Bind(number => capture.Draw(state.Pdf, number)),
            blank: static (state, blank) => IO.lift(() => state.Pdf.AddPage(blank.WidthInDots, blank.HeightInDots, blank.DotsPerInch))
                .Bind(number => blank.Draw(state.Pdf, number)));

    private static IO<ViewCaptureSettings> Prepare(ViewportRef row, CaptureRequest request) =>
        from doc in IO.lift(() => Missing.Unless(row.View.Document, nameof(RhinoView.Document)))
        from settings in request.Subject.Switch(
            (Row: row, Doc: doc, Request: request),
            view: static (state, view) =>
                from settings in IO.lift(() => new ViewCaptureSettings(state.Row.View, view.Pixels, view.Dpi))
                from configured in GeometryOps.OnFailure(Configure(state.Row, state.Doc, settings, state.Request), IO.lift(settings.Dispose))
                select settings,
            page: static (state, page) =>
                from pageView in IO.lift(() => Optional(state.Row.View as RhinoPageView).ToFin(new WrongViewKind(nameof(RhinoPageView))))
                from settings in IO.lift(() => new ViewCaptureSettings(pageView, page.Dpi))
                from configured in GeometryOps.OnFailure(Configure(state.Row, state.Doc, settings, state.Request), IO.lift(settings.Dispose))
                select settings,
            preview: static (state, preview) =>
                Disposal.Using(
                    Prepare(state.Row, state.Request with { Subject = preview.Source }),
                    basis => IO.lift(() => Missing.Unless(basis.CreatePreviewSettings(preview.Pixels), nameof(ViewCaptureSettings.CreatePreviewSettings)))))
        from valid in GeometryOps.OnFailure(IO.lift(() => Refused.Unless(settings.IsValid, nameof(ViewCaptureSettings.IsValid))), IO.lift(settings.Dispose))
        select settings;

    private static IO<Unit> Configure(ViewportRef row, RhinoDoc doc, ViewCaptureSettings settings, CaptureRequest request) =>
        from bound in IO.lift(() => {
            settings.Document = doc;
            _ = row.Detail.Iter(_ => settings.SetViewport(row.Viewport));
        })
        from area in IO.lift(() => request.Area.Switch(
            settings,
            fullView: static (target, _) => target.ViewArea = ViewCaptureSettings.ViewAreaMapping.View,
            extents: static (target, _) => target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Extents,
            screenWindow: static (target, window) => {
                target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
                target.SetWindowRect(window.First, window.Second);
            },
            worldWindow: static (target, window) => {
                target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
                target.SetWindowRect(window.First, window.Second);
            }))
        from layout in request.Layout.Switch(
            settings,
            bound: static (_, _) => IO.pure(unit),
            crop: static (target, crop) => IO.lift(() => target.SetLayout(crop.Media, crop.CropRectangle)),
            margins: static (target, margins) => IO.lift(() =>
                Refused.Unless(target.SetMargins(margins.Units, margins.Left, margins.Top, margins.Right, margins.Bottom), nameof(ViewCaptureSettings.SetMargins))),
            maximize: static (target, _) => IO.lift(target.MaximizePrintableArea))
        from placement in IO.lift(() => {
            _ = request.Placement.Offset.Iter(offset => settings.SetOffset(offset.Units, offset.FromMargin, offset.X, offset.Y));
            _ = request.Placement.Anchor.Iter(anchor => settings.OffsetAnchor = anchor);
            return Refused.Unless(!request.Placement.MatchViewportAspect || settings.MatchViewportAspectRatio(), nameof(ViewCaptureSettings.MatchViewportAspectRatio));
        })
        from scale in IO.lift(() => request.Scale.Switch(
            settings,
            native: static (_, _) => { },
            toValue: static (target, scale) => target.SetModelScaleToValue(scale.Scale),
            toFit: static (target, _) => target.SetModelScaleToFit(promptOnChange: false)))
        select scale;

    // --- [VIEWS]
    public static IO<Bitmap> Capture(RhinoView view, ViewCapture capture) =>
        IO.lift(() => Missing.Unless(capture.CaptureToBitmap(view), nameof(ViewCapture.CaptureToBitmap)));

    public static IO<Bitmap> CaptureMode(RhinoView view, Option<Size> size, DisplayModeDescription mode) =>
        IO.lift(() => Missing.Unless(size.Match(Some: pixels => view.CaptureToBitmap(pixels, mode), None: () => view.CaptureToBitmap(mode)), nameof(RhinoView.CaptureToBitmap)));

    // --- [DEPTH]
    public static IO<DepthCapture> CaptureDepth(RhinoViewport viewport, Action<ZBufferCapture> configure, DepthRequest request) =>
        from extent in IO.lift(() => viewport.Size)
        from depth in Disposal.Using(() => new ZBufferCapture(viewport), capture =>
            from configured in IO.lift(() => configure(capture))
            from output in request.Switch(
                (Capture: capture, Extent: extent),
                stats: static (_, _) => IO.pure<DepthOutput>(new DepthOutput.Stats()),
                samples: static (state, samples) => Sampled(state.Capture, state.Extent, samples.Pixels),
                grayscale: static (state, _) => IO.lift(() => Missing.Unless(state.Capture.GrayscaleDib(), nameof(ZBufferCapture.GrayscaleDib)))
                    .Map<DepthOutput>(static image => new DepthOutput.Grayscale(image)))
            from stats in IO.lift(() => (Hits: capture.HitCount(), MinZ: capture.MinZ(), MaxZ: capture.MaxZ()))
            select new DepthCapture(
                stats.Hits > 0 ? new DepthHits.Hit(stats.Hits, new DepthRange(stats.MinZ, stats.MaxZ)) : new DepthHits.Empty(),
                output))
        select depth;

    private static IO<DepthOutput> Sampled(ZBufferCapture capture, Size extent, Seq<System.Drawing.Point> pixels) =>
        IO.lift(() => pixels.TraverseM(pixel =>
                SampleOutsideExtent.Unless(pixel, extent).Map(_ => new DepthSample(pixel, capture.ZValueAt(pixel.X, pixel.Y), capture.WorldPointAt(pixel.X, pixel.Y))))
            .As())
            .Map<DepthOutput>(static values => new DepthOutput.Samples(values));
}
