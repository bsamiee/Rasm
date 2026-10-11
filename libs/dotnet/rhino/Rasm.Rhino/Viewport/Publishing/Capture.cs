using System.Drawing;
using System.Drawing.Imaging;
using Rasm.Drafting;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Shapes;
using Rhino;
using Rhino.Display;

namespace Rasm.Rhino.Viewport.Publishing;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record CaptureSubject {
    public sealed record View(Size Media, Dpi Dpi) : CaptureSubject;

    public sealed record Page(Dpi Dpi) : CaptureSubject;
}

[Union]
public abstract partial record CaptureArea {
    public sealed record FullView() : CaptureArea;

    public sealed record Extents() : CaptureArea;

    public sealed record ScreenWindow(Point2d First, Point2d Second) : CaptureArea;

    public sealed record WorldWindow(Point3d First, Point3d Second) : CaptureArea;
}

[Union]
public abstract partial record CaptureScale {
    public sealed record ToValue(DrawingScale Scale) : CaptureScale;

    public sealed record ToFit() : CaptureScale;
}

[Union]
public abstract partial record MediaLayout {
    public sealed record Crop(Rectangle CropRectangle) : MediaLayout;

    public sealed record Margins(PrinterMargins Millimeters) : MediaLayout;

    public sealed record Maximize() : MediaLayout;
}

public sealed record MediaOffset(ViewCaptureSettings.AnchorLocation Anchor, bool FromMargin, double X, double Y);

public sealed record CaptureRequest(
    CaptureSubject Subject, CaptureArea Area, Option<CaptureScale> Scale, Option<MediaLayout> Layout, bool MatchViewportAspect, Option<MediaOffset> Offset,
    Func<ViewCaptureSettings, Fin<Unit>> Options);

[SmartEnum]
public sealed partial class RasterEncoding {
    public static readonly RasterEncoding Png = new(ImageFormat.Png, FileExtension.Create(".png"));
    public static readonly RasterEncoding Jpeg = new(ImageFormat.Jpeg, FileExtension.Create(".jpg"));
    public static readonly RasterEncoding Tiff = new(ImageFormat.Tiff, FileExtension.Create(".tif"));

    public ImageFormat Format { get; }

    public FileExtension Extension { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewCaptures {
    // --- [SETTINGS]
    public static IO<ViewCaptureSettings> Settings(ViewportRef row, CaptureRequest request, Action<IDisposable> retain) =>
        Valid(
            () => request.Subject.Switch(
                row,
                view: static (target, view) => new ViewCaptureSettings(target.View, view.Media, view.Dpi),
                page: static (target, page) => WrongType.Unless<RhinoPageView>(target.View).Map(pageView => new ViewCaptureSettings(pageView, page.Dpi))),
            settings => Configured(row, settings, request), retain);

    public static IO<ViewCaptureSettings> Preview(ViewCaptureSettings basis, Size pixels, Action<IDisposable> retain) =>
        Valid(() => Missing.Unless(basis.CreatePreviewSettings(pixels), nameof(ViewCaptureSettings.CreatePreviewSettings)), static _ => IO.pure(unit), retain);

    private static IO<ViewCaptureSettings> Valid(Func<Fin<ViewCaptureSettings>> create, Func<ViewCaptureSettings, IO<Unit>> configure, Action<IDisposable> retain) =>
        Copies.Owned(create,
            answer =>
                from settings in IO.lift(answer)
                from configured in configure(settings)
                from valid in IO.lift(() => Refused.Unless(settings.IsValid, settings, nameof(ViewCaptureSettings.IsValid)))
                select valid,
            static answer => IO.lift(() => { _ = answer.IfSucc(static settings => settings.Dispose()); }), retain);

    private static IO<Unit> Configured(ViewportRef row, ViewCaptureSettings settings, CaptureRequest request) =>
        from framed in IO.lift(() => {
            settings.Document = row.View.Document;
            _ = row.Detail.Iter(_ => settings.SetViewport(row.Viewport));
            request.Area.Switch(
                settings,
                fullView: static (target, _) => target.ViewArea = ViewCaptureSettings.ViewAreaMapping.View,
                extents: static (target, _) => target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Extents,
                screenWindow: static (target, window) => { target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window; target.SetWindowRect(window.First, window.Second); },
                worldWindow: static (target, window) => { target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window; target.SetWindowRect(window.First, window.Second); });
        })
        from laid in request.Layout.Traverse(layout => layout.Switch(
            settings,
            crop: static (target, crop) => IO.lift(() => target.SetLayout(target.MediaSize, crop.CropRectangle)),
            margins: static (target, margins) => IO.lift(() => Refused.Unless(
                target.SetMargins(UnitSystem.Millimeters, margins.Millimeters.Left, margins.Millimeters.Top, margins.Millimeters.Right, margins.Millimeters.Bottom),
                nameof(ViewCaptureSettings.SetMargins))),
            maximize: static (target, _) => IO.lift(target.MaximizePrintableArea))).As()
        from matched in when(request.MatchViewportAspect, IO.lift(() => Refused.Unless(settings.MatchViewportAspectRatio(), nameof(ViewCaptureSettings.MatchViewportAspectRatio)))).As()
        from placed in IO.lift(() => {
            _ = request.Offset.Iter(offset => { settings.OffsetAnchor = offset.Anchor; settings.SetOffset(UnitSystem.Millimeters, offset.FromMargin, offset.X, offset.Y); });
            _ = request.Scale.Iter(scale => scale.Switch(
                settings,
                toValue: static (target, value) => target.SetModelScaleToValue(UnitsNet.QuantityValue.Inverse(value.Scale).ToDouble()),
                toFit: static (target, _) => target.SetModelScaleToFit(promptOnChange: false)));
        })
        from edited in IO.lift(() => request.Options(settings))
        select edited;

    // --- [RASTERS]
    public static IO<Bitmap> CaptureToBitmap(RhinoView view, Func<ViewCapture, Fin<Unit>> edit, Action<IDisposable> retain) =>
        from capture in IO.lift(static () => new ViewCapture())
        from edited in IO.lift(() => edit(capture))
        from bitmap in Copies.Owned(() => capture.CaptureToBitmap(view),
                static bitmap => Missing.Unless(bitmap, nameof(ViewCapture.CaptureToBitmap)),
                static bitmap => DisposalOps.Release(Conversions.Rows(new[] { bitmap })), retain)
        select bitmap;

    public static IO<Bitmap> CaptureToBitmap(RhinoView view, Guid modeId, Option<Size> pixels, Func<DisplayPipelineAttributes, Fin<Unit>> edit, Action<IDisposable> retain) =>
        Viewports.WithMode(modeId, mode => IO.lift(() => edit(mode.DisplayAttributes)).Bind(_ =>
            Copies.Owned(() => pixels.Match(Some: size => view.CaptureToBitmap(size, mode), None: () => view.CaptureToBitmap(mode)),
                static bitmap => Missing.Unless(bitmap, nameof(RhinoView.CaptureToBitmap)),
                static bitmap => DisposalOps.Release(Conversions.Rows(new[] { bitmap })), retain)));
}
