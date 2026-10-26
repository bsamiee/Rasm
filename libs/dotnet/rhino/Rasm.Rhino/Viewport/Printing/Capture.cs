using System.Drawing;
using Rasm.Drafting;
using Rasm.Rhino.Modeling;
using Rasm.UI;
using Rhino;
using Rhino.Display;

namespace Rasm.Rhino.Viewport.Printing;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PixelDensity : IPresented<PixelDensity, double> {
    public static PixelDensity MinValue { get; } = new(1d / Length.FromMeters(1).Inches.ToDouble());
    public static PixelDensity MaxValue { get; } = new(uint.MaxValue / Length.FromMeters(1).Inches.ToDouble());
    public static Presentation<double> Presentation { get; } = new() { Track = None, Step = 1d, Decimals = 0 };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[Union]
public abstract partial record CaptureSubject {
    public sealed record View(Size Media, PixelDensity Density) : CaptureSubject;

    public sealed record Page(PixelDensity Density) : CaptureSubject;
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

    public sealed record Margins(PrinterMargins Inset) : MediaLayout;

    public sealed record Maximize() : MediaLayout;

    public sealed record MatchViewport() : MediaLayout;
}

public sealed record MediaOffset(ViewCaptureSettings.AnchorLocation Anchor, bool FromMargin, Length X, Length Y);

public sealed record CaptureRequest(CaptureArea Area, Option<CaptureScale> Scale, Seq<MediaLayout> Layout, Option<MediaOffset> Offset, Func<ViewCaptureSettings, Fin<Unit>> Options);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewCaptures {
    // --- [SETTINGS]
    public static IO<ViewCaptureSettings> Settings(ViewportRef row, CaptureSubject subject, CaptureRequest request, Action<IDisposable> retain) =>
        Valid(
            () => subject.Switch(
                row,
                view: static (target, view) => Framed(target, new ViewCaptureSettings(target.View, view.Media, view.Density)),
                page: static (target, page) => Conversions.Cast<RhinoPageView>(target.View).Map(pageView => new ViewCaptureSettings(pageView, page.Density))),
            Some((ViewCaptureSettings settings) => Configured(row, settings, request)), retain);

    public static IO<ViewCaptureSettings> Preview(ViewCaptureSettings basis, Size pixels, Action<IDisposable> retain) =>
        Valid(() => Refused.UnlessFound(basis.CreatePreviewSettings(pixels), nameof(ViewCaptureSettings.CreatePreviewSettings)), None, retain);

    private static ViewCaptureSettings Framed(ViewportRef row, ViewCaptureSettings settings) {
        if (row.Viewport.ViewportType is ViewportType.DetailViewport || row.Viewport.ParentView is null)
            settings.SetViewport(row.Viewport);
        return settings;
    }

    private static IO<ViewCaptureSettings> Valid(Func<Fin<ViewCaptureSettings>> create, Option<Func<ViewCaptureSettings, IO<Unit>>> configure, Action<IDisposable> retain) =>
        Copies.Owned(create,
            answer =>
                from settings in IO.lift(answer)
                from configured in configure.Traverse(step => step(settings)).As()
                from valid in IO.lift(() => Refused.Unless(settings.IsValid, settings, nameof(settings)))
                select valid,
            static answer => IO.lift(() => { _ = answer.IfSucc(static settings => settings.Dispose()); }), retain);

    private static IO<Unit> Configured(ViewportRef row, ViewCaptureSettings settings, CaptureRequest request) =>
        from framed in IO.lift(() => {
            if (row.Viewport.ParentView is null)
                settings.Document = row.View.Document;
            request.Area.Switch(
                settings,
                fullView: static (target, _) => target.ViewArea = ViewCaptureSettings.ViewAreaMapping.View,
                extents: static (target, _) => target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Extents,
                screenWindow: static (target, window) => { target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window; target.SetWindowRect(window.First, window.Second); },
                worldWindow: static (target, window) => { target.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window; target.SetWindowRect(window.First, window.Second); });
        })
        from laid in request.Layout.TraverseM(layout => layout.Switch(
            settings,
            crop: static (target, crop) => IO.lift(() => target.SetLayout(target.MediaSize, crop.CropRectangle)),
            margins: static (target, margins) => IO.lift(() => Refused.Unless(
                target.SetMargins(UnitSystem.Millimeters, Millimeters(margins.Inset.Left), Millimeters(margins.Inset.Top), Millimeters(margins.Inset.Right), Millimeters(margins.Inset.Bottom)),
                nameof(ViewCaptureSettings.SetMargins))),
            maximize: static (target, _) => IO.lift(target.MaximizePrintableArea),
            matchViewport: static (target, _) => IO.lift(() => Refused.Unless(target.MatchViewportAspectRatio(), nameof(ViewCaptureSettings.MatchViewportAspectRatio))))).As()
        from placed in IO.lift(() => {
            _ = request.Offset.Iter(offset => {
                settings.OffsetAnchor = offset.Anchor;
                settings.SetOffset(UnitSystem.Millimeters, offset.FromMargin, Millimeters(offset.X), Millimeters(offset.Y));
            });
            _ = request.Scale.Iter(scale => scale.Switch(
                settings,
                toValue: static (target, value) => target.SetModelScaleToValue(UnitsNet.QuantityValue.Inverse(value.Scale).ToDouble()),
                toFit: static (target, _) => target.SetModelScaleToFit(promptOnChange: false)));
        })
        from edited in IO.lift(() => request.Options(settings))
        select edited;

    private static double Millimeters(Length length) => length.Millimeters.ToDouble();
}
