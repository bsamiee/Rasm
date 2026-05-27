using System.Runtime.InteropServices;
using DrawingRectangle = System.Drawing.Rectangle;

namespace Rasm.Rhino;

[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureMargins(UnitSystem Units, double Left, double Top, double Right, double Bottom);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureOffset(UnitSystem Units, bool FromMargin, double X, double Y, ViewCaptureSettings.AnchorLocation Anchor);

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

[Union(SwitchMapStateParameterName = "settings")]
public abstract partial record CaptureScaleMode {
    private CaptureScaleMode() { }
    public sealed record Ratio(double Scale) : CaptureScaleMode;
    public sealed record Fit : CaptureScaleMode;

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op op) =>
        Switch(
            (Settings: settings, Op: op),
            ratio: static (ctx, mode) =>
                RhinoMath.IsValidDouble(x: mode.Scale) && mode.Scale > 0.0
                    ? Fin.Succ(value: Op.Side(() => ctx.Settings.SetModelScaleToValue(scale: mode.Scale)))
                    : Fin.Fail<Unit>(error: ctx.Op.InvalidInput()),
            fit: static (ctx, _) => Fin.Succ(value: Op.Side(() => ctx.Settings.SetModelScaleToFit(promptOnChange: false))));
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
        source.Map(value => RhinoMath.IsValidDouble(x: value) && value > 0.0
            ? Fin.Succ(value: Op.Side(() => apply(obj: value)))
            : Fin.Fail<Unit>(error: op.InvalidInput()))
        .IfNone(Fin.Succ(value: unit));
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
               from aspect in self.MatchAspect ? op.Confirm(success: active.MatchViewportAspectRatio()) : Fin.Succ(value: unit)
               from maximized in self.Maximize ? Fin.Succ(value: Op.Side(active.MaximizePrintableArea)) : Fin.Succ(value: unit)
               from scale in self.Scale.Map(value => value.Apply(settings: active, op: op)).IfNone(Fin.Succ(value: unit))
               select unit;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CaptureDecor(
    bool DrawBackground = true,
    bool DrawGrid = false,
    bool DrawAxis = false,
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
    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op op) {
        CaptureDecor self = this;
        return from active in Optional(settings).ToFin(Fail: op.InvalidInput())
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
                   _ = self.WireThicknessScale.Iter(value => active.WireThicknessScale = value);
                   _ = self.PointSizeMillimeters.Iter(value => active.PointSizeMillimeters = value);
                   _ = self.ArrowheadSizeMillimeters.Iter(value => active.ArrowheadSizeMillimeters = value);
                   _ = self.TextDotPointSize.Iter(value => active.TextDotPointSize = value);
                   _ = self.HeaderText.Iter(value => active.HeaderText = value);
                   _ = self.FooterText.Iter(value => active.FooterText = value);
               }))
               from layout in self.Layout.Map(value => value.Apply(settings: active, op: op)).IfNone(Fin.Succ(value: unit))
               select layout;
    }
}
