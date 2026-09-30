using System.Drawing;
using System.Xml;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.FileIO;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSubject {
    public sealed record View(Size Pixels, double Dpi) : CaptureSubject;

    public sealed record Page(double Dpi) : CaptureSubject;
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
    private PdfPage(Func<FilePdf, int, IO<Unit>> draw) => Draw = draw;

    public Func<FilePdf, int, IO<Unit>> Draw { get; }

    public sealed record Capture(ViewCaptureSettings Settings, Func<FilePdf, int, IO<Unit>> Draw) : PdfPage(Draw);

    public sealed record Blank(int WidthInDots, int HeightInDots, int DotsPerInch, Func<FilePdf, int, IO<Unit>> Draw) : PdfPage(Draw);
}

public sealed record ViewShot(Size Pixels, bool DrawGrid, bool DrawAxes, bool DrawGridAxes);

public sealed record DepthSample(System.Drawing.Point Pixel, float Z, Point3d World);

public sealed record DepthRange(int Hits, float MinZ, float MaxZ);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class CaptureMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(@ViewShot.Pixels.Width), nameof(ViewCapture.Width))]
    [MapProperty(nameof(@ViewShot.Pixels.Height), nameof(ViewCapture.Height))]
    [MapValue(nameof(ViewCapture.ScaleScreenItems), false)]
    internal static partial ViewCapture ToCapture(ViewShot shot);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(ViewShot.DrawAxes), nameof(DisplayPipelineAttributes.ViewDisplayAttributes.DrawWorldAxes))]
    [MapperIgnoreSource(nameof(ViewShot.Pixels), Justification = "RhinoView.CaptureToBitmap size")]
    internal static partial void Update(ViewShot shot, DisplayPipelineAttributes.ViewDisplayAttributes overlays);
}

public static class Captures {
    // --- [SETTINGS]
    public static IO<TValue> WithSettings<TValue>(Seq<(ViewportRef Row, CaptureRequest Request)> pages, Func<Seq<ViewCaptureSettings>, IO<TValue>> body) =>
        DisposalOps.Using(DisposalOps.AcquireAll(pages.Map(Prepare)), body);

    public static IO<ViewCaptureSettings> Preview(ViewCaptureSettings basis, Size pixels) =>
        IO.lift(() => Missing.Unless(basis.CreatePreviewSettings(pixels), nameof(ViewCaptureSettings.CreatePreviewSettings)));

    private static IO<ViewCaptureSettings> Prepare((ViewportRef Row, CaptureRequest Request) target) =>
        target.Request.Subject.Switch(
            target,
            view: static (state, view) => Configured(state.Row, state.Request, () => new ViewCaptureSettings(state.Row.View, view.Pixels, view.Dpi)),
            page: static (state, page) =>
                from pageView in IO.lift(() => Optional(state.Row.View as RhinoPageView).ToFin(new WrongType(typeof(RhinoPageView), state.Row.View.GetType())))
                from settings in Configured(state.Row, state.Request, () => new ViewCaptureSettings(pageView, page.Dpi))
                select settings);

    private static IO<ViewCaptureSettings> Configured(ViewportRef row, CaptureRequest request, Func<ViewCaptureSettings> create) =>
        from settings in IO.lift(create)
        from configured in DisposalOps.OnFailure(Configure(row, settings, request), IO.lift(settings.Dispose))
        select configured;

    private static IO<ViewCaptureSettings> Configure(ViewportRef row, ViewCaptureSettings settings, CaptureRequest request) =>
        from doc in IO.lift(() => Missing.Unless(row.View.Document, nameof(RhinoView.Document)))
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
        from valid in IO.lift(() => Refused.Unless(settings.IsValid, nameof(ViewCaptureSettings.IsValid)))
        select settings;

    // --- [MEDIA]
    public static IO<Bitmap> ToBitmap(ViewCaptureSettings settings) =>
        IO.lift(() => Missing.Unless(ViewCapture.CaptureToBitmap(settings), nameof(ViewCapture.CaptureToBitmap)));

    public static IO<XmlDocument> ToSvg(ViewCaptureSettings settings) =>
        IO.lift(() => Missing.Unless(ViewCapture.CaptureToSvg(settings), nameof(ViewCapture.CaptureToSvg)));

    public static IO<Unit> SendToPrinter(string printerName, Seq<ViewCaptureSettings> pages, int copies) =>
        IO.lift(() => Refused.Unless(ViewCapture.SendToPrinter(printerName, [.. pages], copies), nameof(ViewCapture.SendToPrinter)));

    public static IO<Unit> ToPdf(string path, Seq<PdfPage> pages, bool layersAsOptionalContentGroups, Option<Func<FilePdf, IO<Unit>>> beforeWrite) =>
        from pdf in IO.lift(static () => Missing.Unless(FilePdf.Create(), nameof(FilePdf.Create)))
        from added in pages.TraverseM(page => Added(pdf, page, layersAsOptionalContentGroups)).As()
        from drawn in beforeWrite.Traverse(draw => draw(pdf)).As()
        from written in IO.lift(() => pdf.Write(path))
        select written;

    private static IO<Unit> Added(FilePdf pdf, PdfPage page, bool layersAsOptionalContentGroups) =>
        IO.lift(() => page.Switch(
                (Pdf: pdf, Layers: layersAsOptionalContentGroups),
                capture: static (state, capture) => {
                    state.Pdf.LayersAsOptionalContentGroups = state.Layers && !capture.Settings.RasterMode;
                    return state.Pdf.AddPage(capture.Settings);
                },
                blank: static (state, blank) => state.Pdf.AddPage(blank.WidthInDots, blank.HeightInDots, blank.DotsPerInch)))
            .Bind(number => page.Draw(pdf, number));

    // --- [VIEWS]
    /// <summary>Captures <paramref name="view"/> in its main viewport's display mode, a realtime mode yields the frame shown when the host turn began</summary>
    public static IO<Bitmap> Capture(RhinoView view, ViewShot shot) =>
        DisposalOps.Using(
            IO.lift(() => Missing.Unless(view.MainViewport.DisplayMode, nameof(RhinoViewport.DisplayMode))),
            mode => IO.lift(() => Answers.Present(mode.DisplayAttributes.RealtimeDisplayId).Match(
                Some: _ => Missing.Unless(CaptureMapper.ToCapture(shot).CaptureToBitmap(view), nameof(ViewCapture.CaptureToBitmap)),
                None: () => {
                    CaptureMapper.Update(shot, mode.DisplayAttributes.ViewSpecificAttributes);
                    return Missing.Unless(view.CaptureToBitmap(shot.Pixels, mode), nameof(RhinoView.CaptureToBitmap));
                })));

    // --- [DEPTH]
    public static IO<Option<DepthRange>> Hits(ZBufferCapture capture) =>
        IO.lift(() => Some(new DepthRange(capture.HitCount(), capture.MinZ(), capture.MaxZ())).Filter(static range => range.Hits > 0));

    public static IO<Seq<DepthSample>> Samples(RhinoViewport viewport, ZBufferCapture capture, Seq<System.Drawing.Point> pixels) =>
        from extent in IO.lift(() => viewport.Size)
        from samples in IO.lift(() => pixels
            .Traverse(pixel => SampleOutsideExtent.Unless(pixel, extent)
                .Map(_ => new DepthSample(pixel, capture.ZValueAt(pixel.X, pixel.Y), capture.WorldPointAt(pixel.X, pixel.Y))))
            .As())
        select samples;
}
