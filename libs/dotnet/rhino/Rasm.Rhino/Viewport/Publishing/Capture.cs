using System.Drawing;
using System.Drawing.Imaging;
using Rasm.Drafting;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.Display;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Viewport.Publishing;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSubject {
    private CaptureSubject(Dpi dpi) => Dpi = dpi;

    public Dpi Dpi { get; }

    public sealed record View(Size Media, Dpi Dpi) : CaptureSubject(Dpi);

    public sealed record Page(Dpi Dpi) : CaptureSubject(Dpi);
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
    CaptureSubject Subject,
    CaptureArea Area,
    Option<CaptureScale> Scale,
    Option<MediaLayout> Layout,
    bool MatchViewportAspect,
    Option<MediaOffset> Offset,
    Func<ViewCaptureSettings, Fin<Unit>> Options);

public sealed record CaptureOverlays(bool DrawGrid, bool DrawAxes, bool DrawGridAxes);

public sealed record ViewShot(
    Size Pixels,
    CaptureOverlays Overlays,
    bool ScaleScreenItems,
    bool TransparentBackground,
    bool Preview,
    Option<int> RealtimeRenderPasses);

[SmartEnum]
public sealed partial class RasterEncoding {
    public static readonly RasterEncoding Png = new(ImageFormat.Png, FileExtension.Create(".png"));
    public static readonly RasterEncoding Jpeg = new(ImageFormat.Jpeg, FileExtension.Create(".jpg"));
    public static readonly RasterEncoding Tiff = new(ImageFormat.Tiff, FileExtension.Create(".tif"));

    public ImageFormat Format { get; }

    public FileExtension Extension { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class CaptureMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Both)]
    [MapProperty(nameof(@ViewShot.Pixels.Width), nameof(ViewCapture.Width))]
    [MapProperty(nameof(@ViewShot.Pixels.Height), nameof(ViewCapture.Height))]
    [MapNestedProperties(nameof(ViewShot.Overlays))]
    internal static partial void Update(ViewShot shot, ViewCapture capture);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(CaptureOverlays.DrawAxes), nameof(DisplayPipelineAttributes.ViewDisplayAttributes.DrawWorldAxes))]
    internal static partial void Update(CaptureOverlays overlays, DisplayPipelineAttributes.ViewDisplayAttributes attributes);
}

public static class ViewCaptures {
    // --- [SETTINGS]
    public static IO<ViewCaptureSettings> Settings(ViewportRef row, CaptureRequest request) =>
        from settings in IO.lift(() => request.Subject.Switch(
            row,
            view: static (target, view) => new ViewCaptureSettings(target.View, view.Media, view.Dpi),
            page: static (target, page) => WrongType.Unless<RhinoPageView>(target.View).Map(pageView => new ViewCaptureSettings(pageView, page.Dpi))))
        from configured in DisposalOps.OnFailure(Configured(row, settings, request), IO.lift(settings.Dispose))
        select configured;

    public static IO<ViewCaptureSettings> Preview(ViewCaptureSettings basis, Size pixels) =>
        from preview in IO.lift(() => Missing.Unless(basis.CreatePreviewSettings(pixels), nameof(ViewCaptureSettings.CreatePreviewSettings)))
        from valid in DisposalOps.OnFailure(
            IO.lift(() => Refused.Unless(preview.IsValid, preview, nameof(ViewCaptureSettings.IsValid))),
            IO.lift(preview.Dispose))
        select valid;

    private static IO<ViewCaptureSettings> Configured(ViewportRef row, ViewCaptureSettings settings, CaptureRequest request) =>
        from document in IO.lift(() => Missing.Unless(row.View.Document, nameof(RhinoView.Document)))
        from framed in IO.lift(() => {
            settings.Document = document;
            _ = row.Detail.Iter(_ => settings.SetViewport(row.Viewport));
            request.Area.Switch(
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
                });
        })
        from laid in IO.lift(() => request.Layout.Traverse(layout => layout.Switch(
            settings,
            crop: static (target, crop) => {
                target.SetLayout(target.MediaSize, crop.CropRectangle);
                return Fin.Succ(unit);
            },
            margins: static (target, margins) => Refused.Unless(
                target.SetMargins(UnitSystem.Millimeters, margins.Millimeters.Left, margins.Millimeters.Top, margins.Millimeters.Right, margins.Millimeters.Bottom),
                nameof(ViewCaptureSettings.SetMargins)),
            maximize: static (target, _) => {
                target.MaximizePrintableArea();
                return Fin.Succ(unit);
            })).As())
        from matched in when(request.MatchViewportAspect, IO.lift(() => Refused.Unless(settings.MatchViewportAspectRatio(), nameof(ViewCaptureSettings.MatchViewportAspectRatio)))).As()
        from placed in IO.lift(() => {
            _ = request.Offset.Iter(offset => {
                settings.OffsetAnchor = offset.Anchor;
                settings.SetOffset(UnitSystem.Millimeters, offset.FromMargin, offset.X, offset.Y);
            });
            _ = request.Scale.Iter(scale => scale.Switch(
                settings,
                toValue: static (target, value) => target.SetModelScaleToValue(UnitsNet.QuantityValue.Inverse(value.Scale).ToDouble()),
                toFit: static (target, _) => target.SetModelScaleToFit(promptOnChange: false)));
        })
        from edited in IO.lift(() => request.Options(settings))
        from valid in IO.lift(() => Refused.Unless(settings.IsValid, settings, nameof(ViewCaptureSettings.IsValid)))
        select valid;

    // --- [RASTERS]
    public static IO<Bitmap> CaptureToBitmap(RhinoView view, ViewShot shot) =>
        IO.lift(() => {
            ViewCapture capture = new();
            CaptureMapper.Update(shot, capture);
            return Missing.Unless(capture.CaptureToBitmap(view), nameof(ViewCapture.CaptureToBitmap));
        });

    public static IO<Bitmap> CaptureToBitmap(RhinoView view, Guid modeId, Option<Size> pixels, CaptureOverlays overlays) =>
        Viewports.WithMode(modeId, mode => IO.lift(() => {
            CaptureMapper.Update(overlays, mode.DisplayAttributes.ViewSpecificAttributes);
            return Missing.Unless(
                pixels.Match(Some: size => view.CaptureToBitmap(size, mode), None: () => view.CaptureToBitmap(mode)),
                nameof(RhinoView.CaptureToBitmap));
        }));
}
