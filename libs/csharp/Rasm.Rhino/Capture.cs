using System.Runtime.InteropServices;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;
using XmlDocument = System.Xml.XmlDocument;

namespace Rasm.Rhino;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "settings")]
public abstract partial record CaptureArea {
    private CaptureArea() { }
    public sealed record Viewport : CaptureArea;
    public sealed record Extents : CaptureArea;
    public sealed record Crop(DrawingRectangle Value) : CaptureArea;
    public sealed record ScreenWindow(Point2d A, Point2d B) : CaptureArea;
    public sealed record WorldWindow(Point3d A, Point3d B) : CaptureArea;

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op op) =>
        Optional(settings).ToFin(Fail: op.InvalidInput()).Bind(active => Switch(
            (Settings: active, Op: op),
            viewport: static (ctx, _) =>
                Fin.Succ(value: Op.Side(() => ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.View)),
            extents: static (ctx, _) =>
                Fin.Succ(value: Op.Side(() => ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Extents)),
            crop: static (ctx, area) =>
                (area.Value is { Width: > 0, Height: > 0 }
                && area.Value.Left >= 0
                && area.Value.Top >= 0
                && area.Value.Right <= ctx.Settings.MediaSize.Width
                && area.Value.Bottom <= ctx.Settings.MediaSize.Height)
                    ? Fin.Succ(value: Op.Side(() => ctx.Settings.SetLayout(mediaSize: ctx.Settings.MediaSize, cropRectangle: area.Value)))
                    : Fin.Fail<Unit>(error: ctx.Op.InvalidInput()),
            screenWindow: static (ctx, area) =>
                (area.A.IsValid && area.B.IsValid && area.A != area.B)
                    ? Fin.Succ(value: Op.Side(() => {
                        ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
                        ctx.Settings.SetWindowRect(screenPoint1: area.A, screenPoint2: area.B);
                    }))
                    : Fin.Fail<Unit>(error: ctx.Op.InvalidInput()),
            worldWindow: static (ctx, area) =>
                (area.A.IsValid && area.B.IsValid && area.A != area.B)
                    ? Fin.Succ(value: Op.Side(() => {
                        ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
                        ctx.Settings.SetWindowRect(worldPoint1: area.A, worldPoint2: area.B);
                    }))
                    : Fin.Fail<Unit>(error: ctx.Op.InvalidInput())));
}

public readonly record struct CaptureCodec(CaptureFormat Format) {
    public static CaptureCodec Of(CaptureFormat format) =>
        new(Format: format);

    internal Fin<CaptureResult> Render(ViewCaptureSettings settings, Op op) =>
        Format switch {
            CaptureFormat.Bitmap => Optional(ViewCapture.CaptureToBitmap(settings: settings))
                .ToFin(Fail: op.InvalidResult())
                .Map(static value => (CaptureResult)new CaptureResult.Bitmap(Value: value)),
            CaptureFormat.Svg => Optional(ViewCapture.CaptureToSvg(settings: settings))
                .ToFin(Fail: op.InvalidResult())
                .Map(static value => (CaptureResult)new CaptureResult.Svg(Value: value)),
            _ => Fin.Fail<CaptureResult>(error: op.InvalidInput()),
        };
}

public enum CaptureFormat { Bitmap, Svg }

[Union]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Closed [Union] (private ctor) with sealed leaf cases and no finalizer; the Switch-based Dispose owns the only managed handle. The virtual Dispose(bool) pattern guards external derivation and finalization, neither of which this union admits.")]
public abstract partial record CaptureResult : IDisposable {
    private CaptureResult() { }
    public sealed record Bitmap(DrawingBitmap Value) : CaptureResult;
    public sealed record Svg(XmlDocument Value) : CaptureResult;

    public void Dispose() {
        _ = Switch(
            state: unit,
            bitmap: static (_, c) => Op.Side(c.Value.Dispose),
            svg: static (_, _) => unit);
        GC.SuppressFinalize(this);
    }
}

[Union(SwitchMapStateParameterName = "settings")]
public abstract partial record CaptureScaleMode {
    private CaptureScaleMode() { }
    public sealed record Ratio(double Scale) : CaptureScaleMode;
    public sealed record Fit : CaptureScaleMode;

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op op) =>
        Switch(
            (Settings: settings, Op: op),
            ratio: static (ctx, mode) =>
                CaptureMeasure.Positive(value: mode.Scale, op: ctx.Op)
                    .Map(scale => Op.Side(() => ctx.Settings.SetModelScaleToValue(scale: scale))),
            fit: static (ctx, _) => Fin.Succ(value: Op.Side(() => ctx.Settings.SetModelScaleToFit(promptOnChange: false))));
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureDecor(
    bool DrawBackground = true,
    bool DrawGrid = false,
    bool DrawAxis = false,
    bool DrawGridAxes = false,
    bool DrawLockedObjects = true,
    bool DrawSelectedObjectsOnly = false,
    bool DrawMargins = false,
    bool DrawClippingPlanes = true,
    bool DrawLights = true,
    bool DrawWallpaper = true,
    bool DrawBackgroundBitmap = false,
    bool UsePrintWidths = false,
    ViewCaptureSettings.ColorMode OutputColor = ViewCaptureSettings.ColorMode.DisplayColor,
    Option<double> WireThicknessScale = default,
    Option<double> PointSizeMillimeters = default,
    Option<double> ArrowheadSizeMillimeters = default,
    Option<double> TextDotPointSize = default,
    Option<string> HeaderText = default,
    Option<string> FooterText = default,
    Option<CaptureLayout> Layout = default) {
    public static CaptureDecor Publish { get; } = new(UsePrintWidths: true);

    internal bool SupportsTransparentBitmap =>
        !DrawSelectedObjectsOnly
        && !DrawMargins
        && !UsePrintWidths
        && OutputColor == ViewCaptureSettings.ColorMode.DisplayColor
        && WireThicknessScale.IsNone
        && PointSizeMillimeters.IsNone
        && ArrowheadSizeMillimeters.IsNone
        && TextDotPointSize.IsNone
        && HeaderText.IsNone
        && FooterText.IsNone
        && Layout.IsNone;

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op op) {
        CaptureDecor self = this;
        return from active in Optional(settings).ToFin(Fail: op.InvalidInput())
               from wire in self.WireThicknessScale.TraverseM(v => CaptureMeasure.Positive(v, op)).As()
               from point in self.PointSizeMillimeters.TraverseM(v => CaptureMeasure.Finite(v, op)).As()
               from arrow in self.ArrowheadSizeMillimeters.TraverseM(v => CaptureMeasure.Positive(v, op)).As()
               from dot in self.TextDotPointSize.TraverseM(v => CaptureMeasure.Positive(v, op)).As()
               from configured in Fin.Succ(value: Op.Side(() => {
                   active.DrawBackground = self.DrawBackground;
                   active.DrawGrid = self.DrawGrid;
                   active.DrawAxis = self.DrawAxis;
                   active.DrawLockedObjects = self.DrawLockedObjects;
                   active.DrawSelectedObjectsOnly = self.DrawSelectedObjectsOnly;
                   active.DrawMargins = self.DrawMargins;
                   active.DrawClippingPlanes = self.DrawClippingPlanes;
                   active.DrawLights = self.DrawLights;
                   active.DrawWallpaper = self.DrawWallpaper;
                   active.DrawBackgroundBitmap = self.DrawBackgroundBitmap;
                   active.UsePrintWidths = self.UsePrintWidths;
                   active.OutputColor = self.OutputColor;
                   _ = wire.Iter(value => active.WireThicknessScale = value);
                   _ = point.Iter(value => active.PointSizeMillimeters = value);
                   _ = arrow.Iter(value => active.ArrowheadSizeMillimeters = value);
                   _ = dot.Iter(value => active.TextDotPointSize = value);
                   _ = self.HeaderText.Iter(value => active.HeaderText = value);
                   _ = self.FooterText.Iter(value => active.FooterText = value);
               }))
               from layout in self.Layout.Map(value => value.Apply(settings: active, op: op)).IfNone(Fin.Succ(value: unit))
               select layout;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureLayout(
    Option<CaptureArea> Area = default,
    Option<CaptureMargins> Margins = default,
    Option<CaptureOffset> Offset = default,
    Option<CaptureScale> Scale = default,
    bool MatchAspect = false,
    bool Maximize = false) {
    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op op) {
        CaptureLayout self = this;
        return from active in Optional(settings).ToFin(Fail: op.InvalidInput())
               from area in self.Area.Map(capture => capture.Apply(settings: active, op: op)).IfNone(Fin.Succ(value: unit))
               from margins in self.Margins.Case switch {
                   CaptureMargins value when RhinoMath.IsValidDouble(x: value.Left) && value.Left >= 0.0
                       && RhinoMath.IsValidDouble(x: value.Top) && value.Top >= 0.0
                       && RhinoMath.IsValidDouble(x: value.Right) && value.Right >= 0.0
                       && RhinoMath.IsValidDouble(x: value.Bottom) && value.Bottom >= 0.0
                       && value.Units is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits =>
                       op.Confirm(success: active.SetMargins(lengthUnits: value.Units, left: value.Left, top: value.Top, right: value.Right, bottom: value.Bottom)),
                   CaptureMargins => Fin.Fail<Unit>(error: op.InvalidInput()),
                   _ => Fin.Succ(value: unit),
               }
               from offset in self.Offset.Map(value =>
                   RhinoMath.IsValidDouble(x: value.X) && RhinoMath.IsValidDouble(x: value.Y)
                   && value.Units is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits
                       ? Fin.Succ(value: Op.Side(() => {
                           active.SetOffset(lengthUnits: value.Units, fromMargin: value.FromMargin, x: value.X, y: value.Y);
                           active.OffsetAnchor = value.Anchor;
                       }))
                       : Fin.Fail<Unit>(error: op.InvalidInput())).IfNone(Fin.Succ(value: unit))
               from maximized in self.Maximize ? Fin.Succ(value: Op.Side(active.MaximizePrintableArea)) : Fin.Succ(value: unit)
               from aspect in self.MatchAspect ? op.Confirm(success: active.MatchViewportAspectRatio()) : Fin.Succ(value: unit)
               from scale in self.Scale.Map(value => value.Apply(settings: active, op: op)).IfNone(Fin.Succ(value: unit))
               select unit;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureMargins(UnitSystem Units, double Left, double Top, double Right, double Bottom);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureOffset(UnitSystem Units, bool FromMargin, double X, double Y, ViewCaptureSettings.AnchorLocation Anchor);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureRecipe(
    Option<DrawingSize> Size = default,
    Option<double> Dpi = default,
    bool Raster = false,
    Option<CaptureDecor> Decor = default,
    Option<CaptureScaleMode> Scale = default) {
    private readonly Policy policy;
    public const double DefaultScreenDpi = 96d;
    public static CaptureDecor DefaultScreenDecor => new();

    private CaptureRecipe(Option<DrawingSize> size, Option<double> dpi, bool raster, Option<CaptureDecor> decor, Option<CaptureScaleMode> scale, Policy policy)
        : this(Size: size, Dpi: dpi, Raster: raster, Decor: decor, Scale: scale) =>
        this.policy = policy;

    internal CaptureRecipe WithPolicy(double fallbackDpi, CaptureDecor fallbackDecor, Func<CaptureDecor, RhinoView, CaptureDecor> rewrite) =>
        new(size: Size, dpi: Dpi, raster: Raster, decor: Decor, scale: Scale, policy: new Policy(FallbackDpi: Some(fallbackDpi), FallbackDecor: Some(fallbackDecor), DecorRewrite: Some(rewrite)));

    internal Fin<T> Render<T>(
        RhinoView view,
        Option<RhinoViewport> viewport,
        Func<ViewCaptureSettings, Fin<T>> project,
        Op op) {
        CaptureRecipe self = this;
        return Optional(project).ToFin(Fail: op.InvalidInput())
            .Bind(validProject => self.Use(
                view: view,
                viewport: viewport,
                project: (_, _, settings) => validProject(arg: settings),
                op: op));
    }

    internal Fin<ViewCaptureSettings> Open(
        RhinoView view,
        Option<RhinoViewport> viewport,
        Op op) {
        CaptureRecipe self = this;
        return from activeView in op.Need(view)
               from validRewrite in self.Rewrite(op: op)
               from dpi in CaptureMeasure.Positive(value: self.Dpi.IfNone(self.policy.FallbackDpi.IfNone(DefaultScreenDpi)), op: op)
               from opened in UI.RhinoUi.Protect(valid: () => {
                   ViewCaptureSettings? settings = null;
                   try {
                       settings = self.Create(view: activeView, viewport: viewport, dpi: dpi);
                       return self.Configure(view: activeView, viewport: viewport, rewrite: validRewrite, settings: settings, op: op)
                           .Map(_ => {
                               ViewCaptureSettings result = settings;
                               settings = null;
                               return result;
                           });
                   } finally {
                       settings?.Dispose();
                   }
               })
               select opened;
    }

    internal Fin<DrawingBitmap> TransparentBitmap(
        RhinoView view,
        ViewCaptureSettings settings,
        Op op) {
        CaptureRecipe self = this;
        return op.Need(view).Bind(activeView =>
            op.Need(settings).Bind(activeSettings =>
                self.Rewrite(op: op).Bind(validRewrite => {
                    CaptureDecor decor = validRewrite(arg1: self.Decor.IfNone(self.policy.FallbackDecor.IfNone(DefaultScreenDecor)), arg2: activeView);
                    return AcceptTransparent(view: activeView, decor: decor, op: op)
                        .Bind(_ => UI.RhinoUi.Protect(valid: () => Optional(new ViewCapture {
                            Width = activeSettings.MediaSize.Width,
                            Height = activeSettings.MediaSize.Height,
                            TransparentBackground = true,
                            DrawGrid = decor.DrawGrid,
                            DrawAxes = decor.DrawAxis,
                            DrawGridAxes = decor.DrawGridAxes,
                            ScaleScreenItems = true,
                        }.CaptureToBitmap(sourceView: activeView)).ToFin(Fail: op.InvalidResult())));
                })));
    }

    internal Option<RhinoViewport> Viewport(RhinoView view) {
        CaptureRecipe self = this;
        return Optional(view).Bind(active => active is RhinoPageView && self.Size.IsNone
            ? Option<RhinoViewport>.None
            : Some(active.ActiveViewport));
    }

    private Fin<T> Use<T>(
        RhinoView view,
        Option<RhinoViewport> viewport,
        Func<CaptureDecor, RhinoView, ViewCaptureSettings, Fin<T>> project,
        Op op) {
        CaptureRecipe self = this;
        return from activeView in op.Need(view)
               from validRewrite in self.Rewrite(op: op)
               from validProject in Optional(project).ToFin(Fail: op.InvalidInput())
               from dpi in CaptureMeasure.Positive(value: self.Dpi.IfNone(self.policy.FallbackDpi.IfNone(DefaultScreenDpi)), op: op)
               from result in UI.RhinoUi.Protect(valid: () => {
                   ViewCaptureSettings? settings = null;
                   try {
                       settings = self.Create(view: activeView, viewport: viewport, dpi: dpi);
                       return self.Configure(view: activeView, viewport: viewport, rewrite: validRewrite, settings: settings, op: op)
                           .Bind(decor => validProject(arg1: decor, arg2: activeView, arg3: settings));
                   } finally {
                       settings?.Dispose();
                   }
               })
               select result;
    }

    private ViewCaptureSettings Create(RhinoView view, Option<RhinoViewport> viewport, double dpi) {
        ViewCaptureSettings settings = view is RhinoPageView page && viewport.IsNone && Size.IsNone
            ? new ViewCaptureSettings(sourcePageView: page, dpi: dpi)
            : new ViewCaptureSettings(sourceView: view, mediaSize: Size.IfNone(() => viewport.Map(static active => active.Size).IfNone(() => view.ActiveViewport.Size)), dpi: dpi);
        settings.Document = view.Document;
        return settings;
    }

    private Fin<CaptureDecor> Configure(
        RhinoView view,
        Option<RhinoViewport> viewport,
        Func<CaptureDecor, RhinoView, CaptureDecor> rewrite,
        ViewCaptureSettings settings,
        Op op) {
        CaptureRecipe self = this;
        CaptureDecor decor = rewrite(arg1: self.Decor.IfNone(self.policy.FallbackDecor.IfNone(DefaultScreenDecor)), arg2: view);
        _ = viewport.Iter(settings.SetViewport);
        settings.RasterMode = self.Raster;
        return decor.Apply(settings: settings, op: op)
            .Bind(_ => self.Scale.Map(mode => mode.Apply(settings: settings, op: op)).IfNone(Fin.Succ(value: unit)))   // top-level Scale fires after layout-nested scale — last-write override
            .Bind(_ => settings.IsValid ? Fin.Succ(value: decor) : Fin.Fail<CaptureDecor>(error: op.InvalidResult()));
    }

    private Fin<Func<CaptureDecor, RhinoView, CaptureDecor>> Rewrite(Op op) =>
        Fin.Succ(value: policy.DecorRewrite.IfNone(static () => static (decor, _) => decor));

    private static Fin<Unit> AcceptTransparent(RhinoView view, CaptureDecor decor, Op op) =>
        guard(view is not RhinoPageView && decor.SupportsTransparentBitmap, op.InvalidInput()).ToFin();

    private readonly record struct Policy(
        Option<double> FallbackDpi = default,
        Option<CaptureDecor> FallbackDecor = default,
        Option<Func<CaptureDecor, RhinoView, CaptureDecor>> DecorRewrite = default);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureScale(
    Option<CaptureScaleMode> Mode = default,
    Option<double> Horizontal = default,
    Option<double> Vertical = default,
    Option<double> PrintWidth = default,
    Option<bool> Thickness = default,
    Option<bool> MatchLinetype = default,
    Option<bool> PageLinetypes = default) {
    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op op) {
        CaptureScale self = this;
        return from active in Optional(settings).ToFin(Fail: op.InvalidInput())
               from modeApplied in self.Mode.Map(value => value.Apply(settings: active, op: op)).IfNone(Fin.Succ(value: unit))
               from horizontalApplied in ApplyDouble(self.Horizontal, op, value => active.HorizontalScale = value)
               from verticalApplied in ApplyDouble(self.Vertical, op, value => active.VerticalScale = value)
               from widthApplied in ApplyDouble(self.PrintWidth, op, value => active.DefaultPrintWidthMillimeters = value)
               from flags in Fin.Succ(value: Op.Side(() => {
                   _ = self.Thickness.Iter(value => active.ApplyDisplayModeThicknessScales = value);
                   _ = self.MatchLinetype.Iter(value => active.MatchLinetypePatternDefinition = value);
                   _ = self.PageLinetypes.Iter(value => active.LinetypeWidthUnitsArePageLengths = value);
               }))
               select flags;
    }

    private static Fin<Unit> ApplyDouble(Option<double> source, Op op, Action<double> apply) =>
        source.TraverseM(v => CaptureMeasure.Positive(v, op)).As()
            .Map(value => value.Iter(apply));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
file static class CaptureMeasure {
    internal static Fin<double> Finite(double value, Op op) =>
        from _ in guard(RhinoMath.IsValidDouble(x: value), op.InvalidInput()).ToFin()
        select value;

    internal static Fin<double> Positive(double value, Op op) =>
        from _ in guard(RhinoMath.IsValidDouble(x: value) && value > 0.0, op.InvalidInput()).ToFin()
        select value;
}
