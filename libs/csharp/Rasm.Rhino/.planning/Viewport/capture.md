# [RASM_RHINO_CAPTURE]

The capture render specification (`Rasm.Rhino.Viewport`). `CapturePlan` owns the settings-representable subject, area, scale, media layout, and decoration axes without delivery state; `CaptureRequest` pairs a non-empty plan span with one settings-driven egress and admits scalar bitmap/SVG delivery only at cardinality one; and `TransparentCaptureSpec` owns the distinct facade path whose transparency and screen-item flags have no `ViewCaptureSettings` representation. `Captures` crosses `HostThread.OnSession` once, resolves every target inside that UI-scoped demand, binds the addressed viewport before viewport-dependent writes, constructs each basis `ViewCaptureSettings` once, and derives a preview only after its basis is complete. Preparation folds iteratively into one reverse-disposed lease set, so scalar capture, PDF staging, and printer spooling share one acquisition body without exposing or retaining a settings handle.

## [01]-[INDEX]

- [02]-[SPEC_AXES]: Admitted capture extents, origins, subjects, area and scale cases, layout, and settings decoration.
- [03]-[DELIVERY_ROWS]: Settings-driven egress, the separate transparent facade specification, and capture artifacts.
- [04]-[RUN_RAIL]: Sink-free plans, cardinality-admitted delivery requests, internal prepared leases, and the UI-scoped execution fold.

## [02]-[SPEC_AXES]

- Owner: `Size2i`, `Offset2i`, and `CaptureDpi` — private-construction values for positive integer extents, nonnegative integer origins, and finite positive DPI. `CaptureAnchor` and `CaptureColor` `[SmartEnum<int>]` — package-owned rows carrying the complete host anchor and output-color projections. `CaptureSubject` `[Union]` — factory-only view and page bases plus a preview that wraps either admitted base; every target is scalar, and the page case requires `ViewportTarget.PageCase`. `CaptureArea` and `CaptureScale` `[Union]` — factory-only closed families that admit window geometry and model scale before a host write.
- Owner: `CaptureCrop`, `CaptureMargins`, `CaptureOffset`, `MediaLayout`, `CaptureBanner`, `PrintFidelity`, and `CaptureDecor` — admitted settings-only values. Crop admission performs checked `long` bounds arithmetic; every physical magnitude is finite and nonnegative in a known physical unit; and `MediaLayout` admits exactly one explicit crop strategy or automatic maximization before `Apply` lifts `SetMargins` and `MatchViewportAspectRatio` refusal into the rail.
- Law: `MediaLayout` and `CaptureDecor` contain only members represented by `ViewCaptureSettings`. `DrawGridAxes`, `ScaleScreenItems`, and transparency belong exclusively to `TransparentCaptureSpec`; no settings-driven request silently ignores them.
- Law: native `System.Drawing.Size` and `Rectangle` values are minted only inside preparation, and integer position never rides the extent type. Preview preparation configures its view/page basis in full, calls `CreatePreviewSettings` once, validates the derived settings, and retires the basis before egress.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.HostUi;

namespace Rasm.Rhino.Viewport;

// --- [VALUES] -------------------------------------------------------------------------------
public readonly record struct Size2i {
    private Size2i(int width, int height) => (Width, Height) = (width, height);

    public int Width { get; }
    public int Height { get; }

    public static Fin<Size2i> Of(int width, int height, Op? key = null) =>
        guard(width > 0 && height > 0 && (long)width * height <= int.MaxValue, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new Size2i(width: width, height: height));

    internal bool IsValid => Width > 0 && Height > 0 && (long)Width * Height <= int.MaxValue;
    internal System.Drawing.Size Native => new(Width, Height);
}

public readonly record struct Offset2i {
    private Offset2i(int x, int y) => (X, Y) = (x, y);

    public int X { get; }
    public int Y { get; }

    public static Fin<Offset2i> Of(int x, int y, Op? key = null) =>
        guard(x >= 0 && y >= 0, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new Offset2i(x: x, y: y));

    internal bool IsValid => X >= 0 && Y >= 0;
}

public readonly record struct CaptureDpi {
    private CaptureDpi(double value) => Value = value;

    public double Value { get; }

    public static Fin<CaptureDpi> Of(double value, Op? key = null) =>
        guard(double.IsFinite(value) && value > 0.0, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new CaptureDpi(value: value));

    internal bool IsValid => double.IsFinite(Value) && Value > 0.0;
}

[SmartEnum<int>]
public sealed partial class CaptureAnchor {
    public static readonly CaptureAnchor LowerLeft = new(key: 0, native: ViewCaptureSettings.AnchorLocation.LowerLeft);
    public static readonly CaptureAnchor LowerRight = new(key: 1, native: ViewCaptureSettings.AnchorLocation.LowerRight);
    public static readonly CaptureAnchor UpperLeft = new(key: 2, native: ViewCaptureSettings.AnchorLocation.UpperLeft);
    public static readonly CaptureAnchor UpperRight = new(key: 3, native: ViewCaptureSettings.AnchorLocation.UpperRight);
    public static readonly CaptureAnchor Center = new(key: 4, native: ViewCaptureSettings.AnchorLocation.Center);

    internal ViewCaptureSettings.AnchorLocation Native { get; }
}

[SmartEnum<int>]
public sealed partial class CaptureColor {
    public static readonly CaptureColor Display = new(key: 0, native: ViewCaptureSettings.ColorMode.DisplayColor);
    public static readonly CaptureColor Print = new(key: 1, native: ViewCaptureSettings.ColorMode.PrintColor);
    public static readonly CaptureColor Monochrome = new(key: 2, native: ViewCaptureSettings.ColorMode.BlackAndWhite);

    internal ViewCaptureSettings.ColorMode Native { get; }
}

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSubject {
    private CaptureSubject() { }

    internal sealed record ViewCase(ViewportTarget Target, Size2i Pixels, CaptureDpi Dpi) : CaptureSubject;
    internal sealed record PageCase(ViewportTarget Target, CaptureDpi Dpi) : CaptureSubject;
    internal sealed record PreviewCase(CaptureSubject Source, Size2i Pixels) : CaptureSubject;

    public static Fin<CaptureSubject> View(ViewportTarget target, Size2i pixels, CaptureDpi dpi, Op? key = null) {
        Op op = key.OrDefault();
        return from valid in Admit(target: target, key: op)
               from _extent in guard(pixels.IsValid, op.InvalidInput())
               from _dpi in guard(dpi.IsValid, op.InvalidInput())
               select (CaptureSubject)new ViewCase(Target: valid, Pixels: pixels, Dpi: dpi);
    }

    public static Fin<CaptureSubject> Page(ViewportTarget target, CaptureDpi dpi, Op? key = null) {
        Op op = key.OrDefault();
        return from valid in Admit(target: target, key: op)
               from _page in guard(valid is ViewportTarget.PageCase, op.InvalidInput())
               from _dpi in guard(dpi.IsValid, op.InvalidInput())
               select (CaptureSubject)new PageCase(Target: valid, Dpi: dpi);
    }

    public static Fin<CaptureSubject> Preview(CaptureSubject source, Size2i pixels, Op? key = null) {
        Op op = key.OrDefault();
        return from valid in Optional(source).ToFin(Fail: op.InvalidInput())
               from _source in guard(valid is ViewCase or PageCase, op.InvalidInput())
               from _extent in guard(pixels.IsValid, op.InvalidInput())
               select (CaptureSubject)new PreviewCase(Source: valid, Pixels: pixels);
    }

    internal ViewportTarget Address => Switch(
        viewCase: static view => view.Target,
        pageCase: static page => page.Target,
        previewCase: static preview => preview.Source.Address);

    internal Fin<RhinoViewport> Viewport(ViewportRef row, Op key) => Switch(
        state: (Row: row, Op: key),
        viewCase: static (ctx, _) => Fin.Succ(value: ctx.Row.Viewport),
        pageCase: static (ctx, _) => Optional(ctx.Row.View as RhinoPageView)
            .ToFin(Fail: ctx.Op.InvalidInput())
            .Bind(page => ctx.Op.Catch(() => Optional(page.MainViewport).ToFin(Fail: ctx.Op.InvalidResult()))),
        previewCase: static (ctx, preview) => preview.Source.Viewport(row: ctx.Row, key: ctx.Op));

    private static Fin<ViewportTarget> Admit(ViewportTarget target, Op? key) {
        Op op = key.OrDefault();
        return from valid in Optional(target).ToFin(Fail: op.InvalidInput())
               from _ in guard(valid is not ViewportTarget.EveryCase, op.InvalidInput())
               select valid;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureArea {
    private CaptureArea() { }

    internal sealed record FullViewCase : CaptureArea;
    internal sealed record ExtentsCase : CaptureArea;
    internal sealed record ScreenWindowCase(Point2d A, Point2d B) : CaptureArea;
    internal sealed record WorldWindowCase(Point3d A, Point3d B) : CaptureArea;

    public static CaptureArea FullView { get; } = new FullViewCase();
    public static CaptureArea Extents { get; } = new ExtentsCase();

    public static Fin<CaptureArea> ScreenWindow(Point2d a, Point2d b, Op? key = null) =>
        guard(a.IsValid && b.IsValid && a != b, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (CaptureArea)new ScreenWindowCase(A: a, B: b));

    public static Fin<CaptureArea> WorldWindow(Point3d a, Point3d b, Op? key = null) =>
        guard(a.IsValid && b.IsValid && a != b, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (CaptureArea)new WorldWindowCase(A: a, B: b));

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) => Switch(
        state: (Settings: settings, Op: key),
        fullViewCase: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ(
            value: Op.Side(() => ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.View))),
        extentsCase: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ(
            value: Op.Side(() => ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Extents))),
        screenWindowCase: static (ctx, area) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => {
            ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
            ctx.Settings.SetWindowRect(screenPoint1: area.A, screenPoint2: area.B);
        }))),
        worldWindowCase: static (ctx, area) => ctx.Op.Catch(() => Fin.Succ(value: Op.Side(() => {
            ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
            ctx.Settings.SetWindowRect(worldPoint1: area.A, worldPoint2: area.B);
        }))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureScale {
    private CaptureScale() { }

    internal sealed record NativeCase : CaptureScale;
    internal sealed record ToValueCase(PositiveMagnitude Scale) : CaptureScale;
    internal sealed record ToFitCase : CaptureScale;

    public static CaptureScale Native { get; } = new NativeCase();
    public static CaptureScale ToFit { get; } = new ToFitCase();
    public static Fin<CaptureScale> ToValue(PositiveMagnitude scale, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: scale.Value)
            .Map(static admitted => (CaptureScale)new ToValueCase(Scale: admitted));

    internal bool IsValid => Switch(
        nativeCase: static _ => true,
        toValueCase: static value => double.IsFinite(value.Scale.Value) && value.Scale.Value > EpsilonPolicy.ZeroTolerance,
        toFitCase: static _ => true);

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) => Switch(
        state: (Settings: settings, Op: key),
        nativeCase: static (_, _) => Fin.Succ(value: unit),
        toValueCase: static (ctx, value) => ctx.Op.Catch(() => Fin.Succ(
            value: Op.Side(() => ctx.Settings.SetModelScaleToValue(scale: value.Scale.Value)))),
        toFitCase: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ(
            value: Op.Side(() => ctx.Settings.SetModelScaleToFit(promptOnChange: false))));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record CaptureCrop {
    private CaptureCrop(Size2i media, Offset2i origin, Size2i extent) => (Media, Origin, Extent) = (media, origin, extent);

    public Size2i Media { get; }
    public Offset2i Origin { get; }
    public Size2i Extent { get; }

    public static Fin<CaptureCrop> Of(Size2i media, Offset2i origin, Size2i extent, Op? key = null) {
        Op op = key.OrDefault();
        return from _values in guard(media.IsValid && origin.IsValid && extent.IsValid, op.InvalidInput())
               from _x in guard((long)origin.X + extent.Width <= media.Width, op.InvalidInput())
               from _y in guard((long)origin.Y + extent.Height <= media.Height, op.InvalidInput())
               select new CaptureCrop(media: media, origin: origin, extent: extent);
    }
}

public sealed record CaptureMargins {
    private CaptureMargins(UnitSystem units, double left, double top, double right, double bottom) =>
        (Units, Left, Top, Right, Bottom) = (units, left, top, right, bottom);

    public UnitSystem Units { get; }
    public double Left { get; }
    public double Top { get; }
    public double Right { get; }
    public double Bottom { get; }

    public static Fin<CaptureMargins> Of(UnitSystem units, double left, double top, double right, double bottom, Op? key = null) {
        double[] values = [left, top, right, bottom];
        return guard(
                Enum.IsDefined(value: units) && units is not UnitSystem.Unset and not UnitSystem.None and not UnitSystem.CustomUnits
                    && values.All(static value => double.IsFinite(value) && value >= 0.0),
                key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new CaptureMargins(units: units, left: left, top: top, right: right, bottom: bottom));
    }
}

public sealed record CaptureOffset {
    private CaptureOffset(UnitSystem units, bool fromMargin, double x, double y) => (Units, FromMargin, X, Y) = (units, fromMargin, x, y);

    public UnitSystem Units { get; }
    public bool FromMargin { get; }
    public double X { get; }
    public double Y { get; }

    public static Fin<CaptureOffset> Of(UnitSystem units, bool fromMargin, double x, double y, Op? key = null) =>
        guard(
            Enum.IsDefined(value: units) && units is not UnitSystem.Unset and not UnitSystem.None and not UnitSystem.CustomUnits
                && double.IsFinite(x) && x >= 0.0 && double.IsFinite(y) && y >= 0.0,
            key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new CaptureOffset(units: units, fromMargin: fromMargin, x: x, y: y));
}

public sealed record CaptureBanner {
    private CaptureBanner(string header, string footer) => (Header, Footer) = (header, footer);

    public string Header { get; }
    public string Footer { get; }

    public static Fin<CaptureBanner> Of(string header, string footer, Op? key = null) {
        Op op = key.OrDefault();
        return from admittedHeader in Optional(header).ToFin(Fail: op.InvalidInput())
               from admittedFooter in Optional(footer).ToFin(Fail: op.InvalidInput())
               let normalizedHeader = admittedHeader.Trim()
               let normalizedFooter = admittedFooter.Trim()
               from _ in guard(normalizedHeader.Length > 0 || normalizedFooter.Length > 0, op.InvalidInput())
               select new CaptureBanner(header: normalizedHeader, footer: normalizedFooter);
    }
}

public sealed record PrintFidelity {
    private PrintFidelity(bool usePrintWidths, double wireScale, double pointSize, double arrowSize, double textDotSize, double defaultWidth) =>
        (UsePrintWidths, WireThicknessScale, PointSizeMillimeters, ArrowheadSizeMillimeters, TextDotPointSize, DefaultPrintWidthMillimeters) =
            (usePrintWidths, wireScale, pointSize, arrowSize, textDotSize, defaultWidth);

    public bool UsePrintWidths { get; }
    public double WireThicknessScale { get; }
    public double PointSizeMillimeters { get; }
    public double ArrowheadSizeMillimeters { get; }
    public double TextDotPointSize { get; }
    public double DefaultPrintWidthMillimeters { get; }

    public static Fin<PrintFidelity> Of(
        bool usePrintWidths,
        double wireThicknessScale,
        double pointSizeMillimeters,
        double arrowheadSizeMillimeters,
        double textDotPointSize,
        double defaultPrintWidthMillimeters,
        Op? key = null) {
        double[] values = [wireThicknessScale, pointSizeMillimeters, arrowheadSizeMillimeters, textDotPointSize, defaultPrintWidthMillimeters];
        return guard(values.All(static value => double.IsFinite(value) && value >= 0.0), key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new PrintFidelity(
                usePrintWidths: usePrintWidths,
                wireScale: wireThicknessScale,
                pointSize: pointSizeMillimeters,
                arrowSize: arrowheadSizeMillimeters,
                textDotSize: textDotPointSize,
                defaultWidth: defaultPrintWidthMillimeters));
    }
}

public sealed record MediaLayout {
    private MediaLayout(
        Option<CaptureCrop> crop,
        Option<CaptureMargins> margins,
        Option<CaptureOffset> offset,
        Option<CaptureAnchor> anchor,
        bool maximizePrintable,
        bool matchAspect) =>
        (Crop, Margins, Offset, Anchor, MaximizePrintable, MatchAspect) = (crop, margins, offset, anchor, maximizePrintable, matchAspect);

    public Option<CaptureCrop> Crop { get; }
    public Option<CaptureMargins> Margins { get; }
    public Option<CaptureOffset> Offset { get; }
    public Option<CaptureAnchor> Anchor { get; }
    public bool MaximizePrintable { get; }
    public bool MatchAspect { get; }

    public static MediaLayout Default { get; } = new(
        crop: None,
        margins: None,
        offset: None,
        anchor: None,
        maximizePrintable: false,
        matchAspect: true);

    public static Fin<MediaLayout> Of(
        Option<CaptureCrop> crop = default,
        Option<CaptureMargins> margins = default,
        Option<CaptureOffset> offset = default,
        Option<CaptureAnchor> anchor = default,
        bool maximizePrintable = false,
        bool matchAspect = true,
        Op? key = null) =>
        guard(
            (crop.IsNone || margins.IsNone) && (!maximizePrintable || (crop.IsNone && margins.IsNone)),
            key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new MediaLayout(
                crop: crop,
                margins: margins,
                offset: offset,
                anchor: anchor,
                maximizePrintable: maximizePrintable,
                matchAspect: matchAspect));

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) {
        MediaLayout self = this;
        return from _crop in self.Crop.Match(
                   Some: crop => key.Catch(() => Fin.Succ(value: Op.Side(() => settings.SetLayout(
                       mediaSize: crop.Media.Native,
                       cropRectangle: new System.Drawing.Rectangle(crop.Origin.X, crop.Origin.Y, crop.Extent.Width, crop.Extent.Height))))),
                   None: static () => Fin.Succ(value: unit))
               from _margins in self.Margins.Match(
                   Some: margins => key.Catch(() => key.Confirm(success: settings.SetMargins(
                       lengthUnits: margins.Units,
                       left: margins.Left,
                       top: margins.Top,
                       right: margins.Right,
                       bottom: margins.Bottom))),
                   None: static () => Fin.Succ(value: unit))
               from _offset in self.Offset.Match(
                   Some: offset => key.Catch(() => Fin.Succ(value: Op.Side(() => settings.SetOffset(
                       lengthUnits: offset.Units,
                       fromMargin: offset.FromMargin,
                       x: offset.X,
                       y: offset.Y)))),
                   None: static () => Fin.Succ(value: unit))
               from _anchor in self.Anchor.Match(
                   Some: anchor => key.Catch(() => Fin.Succ(value: Op.Side(() => settings.OffsetAnchor = anchor.Native))),
                   None: static () => Fin.Succ(value: unit))
               from _maximize in self.MaximizePrintable
                   ? key.Catch(() => Fin.Succ(value: Op.Side(settings.MaximizePrintableArea)))
                   : Fin.Succ(value: unit)
               from _aspect in self.MatchAspect
                   ? key.Catch(() => key.Confirm(success: settings.MatchViewportAspectRatio()))
                   : Fin.Succ(value: unit)
               select unit;
    }
}

public sealed record CaptureDecor {
    private CaptureDecor(
        bool grid,
        bool axes,
        bool raster,
        CaptureColor outputColor,
        bool background,
        bool backgroundBitmap,
        bool wallpaper,
        bool lockedObjects,
        bool selectedOnly,
        bool clippingPlanes,
        bool lights,
        bool marginLines,
        Option<CaptureBanner> banner,
        Option<PrintFidelity> fidelity) =>
        (Grid, Axes, Raster, OutputColor, Background, BackgroundBitmap, Wallpaper, LockedObjects, SelectedOnly, ClippingPlanes, Lights, MarginLines, Banner, Fidelity) =
            (grid, axes, raster, outputColor, background, backgroundBitmap, wallpaper, lockedObjects, selectedOnly, clippingPlanes, lights, marginLines, banner, fidelity);

    public bool Grid { get; }
    public bool Axes { get; }
    public bool Raster { get; }
    public CaptureColor OutputColor { get; }
    public bool Background { get; }
    public bool BackgroundBitmap { get; }
    public bool Wallpaper { get; }
    public bool LockedObjects { get; }
    public bool SelectedOnly { get; }
    public bool ClippingPlanes { get; }
    public bool Lights { get; }
    public bool MarginLines { get; }
    public Option<CaptureBanner> Banner { get; }
    public Option<PrintFidelity> Fidelity { get; }

    public static CaptureDecor Of(
        bool grid = false,
        bool axes = false,
        bool raster = false,
        Option<CaptureColor> outputColor = default,
        bool background = true,
        bool backgroundBitmap = false,
        bool wallpaper = false,
        bool lockedObjects = true,
        bool selectedOnly = false,
        bool clippingPlanes = true,
        bool lights = true,
        bool marginLines = false,
        Option<CaptureBanner> banner = default,
        Option<PrintFidelity> fidelity = default) =>
        new(
            grid: grid,
            axes: axes,
            raster: raster,
            outputColor: outputColor.IfNone(CaptureColor.Display),
            background: background,
            backgroundBitmap: backgroundBitmap,
            wallpaper: wallpaper,
            lockedObjects: lockedObjects,
            selectedOnly: selectedOnly,
            clippingPlanes: clippingPlanes,
            lights: lights,
            marginLines: marginLines,
            banner: banner,
            fidelity: fidelity);

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) => key.Catch(() => {
        CaptureDecor self = this;
        settings.OutputColor = self.OutputColor.Native;
        settings.RasterMode = self.Raster;
        settings.DrawGrid = self.Grid;
        settings.DrawAxis = self.Axes;
        settings.DrawBackground = self.Background;
        settings.DrawBackgroundBitmap = self.BackgroundBitmap;
        settings.DrawWallpaper = self.Wallpaper;
        settings.DrawLockedObjects = self.LockedObjects;
        settings.DrawSelectedObjectsOnly = self.SelectedOnly;
        settings.DrawClippingPlanes = self.ClippingPlanes;
        settings.DrawLights = self.Lights;
        settings.DrawMargins = self.MarginLines;
        _ = self.Banner.Iter(banner => {
            settings.HeaderText = banner.Header;
            settings.FooterText = banner.Footer;
        });
        _ = self.Fidelity.Iter(row => {
            settings.UsePrintWidths = row.UsePrintWidths;
            settings.WireThicknessScale = row.WireThicknessScale;
            settings.PointSizeMillimeters = row.PointSizeMillimeters;
            settings.ArrowheadSizeMillimeters = row.ArrowheadSizeMillimeters;
            settings.TextDotPointSize = row.TextDotPointSize;
            settings.DefaultPrintWidthMillimeters = row.DefaultPrintWidthMillimeters;
        });
        return Fin.Succ(value: unit);
    });
}
```

## [03]-[DELIVERY_ROWS]

- Owner: `CaptureSink` `[Union]` — factory-only bitmap, SVG, and printer delivery over one prepared batch. Bitmap and SVG admit exactly one plan; printer delivery consumes any non-empty plan sequence in one `SendToPrinter` call. `CaptureArtifact` `[Union]` — capture-minted, publicly readable leased raster, SVG document, or dispatched page count; its one raster transfer disposes the native bitmap unless lease construction settles. Every capture, conversion, and printer call crosses `Op.Catch`; a null bitmap/SVG and a refused printer dispatch remain typed failures.
- Owner: `TransparentDecor` and `TransparentCaptureSpec` — the separate `ViewCapture` facade request carrying only target, extent, grid, axes, combined grid-axes, and screen-item scaling. The facade path cannot receive media layout, model scale, settings color, or print-fidelity fields.
- Boundary: `CaptureSink` cannot name transparency, and `CaptureRequest` cannot carry facade-only flags. Delivery incompatibility is structurally unrepresentable.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureArtifact : IDetachedDocumentResult {
    private CaptureArtifact() { }

    public sealed record RasterCase : CaptureArtifact {
        internal RasterCase(Lease<System.Drawing.Bitmap> pixels, Size2i extent) => (Pixels, Extent) = (pixels, extent);
        public Lease<System.Drawing.Bitmap> Pixels { get; }
        public Size2i Extent { get; }
    }

    public sealed record VectorCase : CaptureArtifact {
        internal VectorCase(System.Xml.XmlDocument svg) => Svg = svg;
        public System.Xml.XmlDocument Svg { get; }
    }

    public sealed record PrintedCase : CaptureArtifact {
        internal PrintedCase(int pages) => Pages = pages;
        public int Pages { get; }
    }

    internal static Fin<CaptureArtifact> Raster(System.Drawing.Bitmap bitmap, Op key) => key.Catch(() => {
        System.Drawing.Bitmap? owned = bitmap;
        try {
            return Size2i.Of(width: bitmap.Width, height: bitmap.Height, key: key).Match(
                Succ: extent => {
                    CaptureArtifact artifact = new RasterCase(
                        pixels: new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap),
                        extent: extent);
                    owned = null;
                    return Fin.Succ(value: artifact);
                },
                Fail: Fin.Fail<CaptureArtifact>);
        } finally {
            owned?.Dispose();
        }
    });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSink {
    private CaptureSink() { }

    internal sealed record BitmapCase : CaptureSink;
    internal sealed record SvgCase : CaptureSink;
    internal sealed record PrinterCase(string PrinterName, Dimension Copies) : CaptureSink;

    public static CaptureSink Bitmap { get; } = new BitmapCase();
    public static CaptureSink Svg { get; } = new SvgCase();

    public static Fin<CaptureSink> Printer(string printerName, Dimension copies, Op? key = null) {
        Op op = key.OrDefault();
        return from name in op.AcceptText(value: printerName)
               from _ in guard(copies.Value >= 1, op.InvalidInput())
               select (CaptureSink)new PrinterCase(PrinterName: name, Copies: copies);
    }

    internal bool AcceptsMany => Switch(
        bitmapCase: static _ => false,
        svgCase: static _ => false,
        printerCase: static _ => true);

    internal Fin<CaptureArtifact> Render(PreparedCapture prepared, Op op) => prepared.Use(
        body: settings => Switch(
            state: (Settings: settings, Op: op),
            bitmapCase: static (ctx, _) => Rasterized(settings: ctx.Settings, op: ctx.Op),
            svgCase: static (ctx, _) => Vectorized(settings: ctx.Settings, op: ctx.Op),
            printerCase: static (ctx, sink) => Printed(
                settings: ctx.Settings,
                printerName: sink.PrinterName,
                copies: sink.Copies,
                op: ctx.Op)),
        key: op);

    private static Fin<ViewCaptureSettings> One(Seq<ViewCaptureSettings> settings, Op op) =>
        from _ in guard(settings.Count == 1, op.InvalidInput())
        from row in settings.Head.ToFin(Fail: op.MissingContext())
        select row;

    private static Fin<CaptureArtifact> Rasterized(Seq<ViewCaptureSettings> settings, Op op) =>
        from row in One(settings: settings, op: op)
        from bitmap in op.Catch(() => Optional(ViewCapture.CaptureToBitmap(settings: row)).ToFin(Fail: op.InvalidResult()))
        from artifact in CaptureArtifact.Raster(bitmap: bitmap, key: op)
        select artifact;

    private static Fin<CaptureArtifact> Vectorized(Seq<ViewCaptureSettings> settings, Op op) =>
        from row in One(settings: settings, op: op)
        from artifact in op.Catch(() => Optional(ViewCapture.CaptureToSvg(settings: row)).ToFin(Fail: op.InvalidResult())
            .Map(static svg => (CaptureArtifact)new CaptureArtifact.VectorCase(svg: svg)))
        select artifact;

    private static Fin<CaptureArtifact> Printed(Seq<ViewCaptureSettings> settings, string printerName, Dimension copies, Op op) =>
        from _ in op.Catch(() => op.Confirm(success: ViewCapture.SendToPrinter(
            printerName: printerName,
            settings: settings.ToArray(),
            copies: copies.Value)))
        select (CaptureArtifact)new CaptureArtifact.PrintedCase(pages: settings.Count);
}

public sealed record TransparentDecor(bool Grid, bool Axes, bool GridAxes, bool ScaleScreenItems) {
    public static TransparentDecor Plain { get; } = new(Grid: false, Axes: false, GridAxes: false, ScaleScreenItems: true);
}

public sealed record TransparentCaptureSpec {
    private TransparentCaptureSpec(ViewportTarget target, Size2i extent, TransparentDecor decor) =>
        (Target, Extent, Decor) = (target, extent, decor);

    public ViewportTarget Target { get; }
    public Size2i Extent { get; }
    public TransparentDecor Decor { get; }

    public static Fin<TransparentCaptureSpec> Of(
        ViewportTarget target,
        Size2i extent,
        Option<TransparentDecor> decor = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from validTarget in Optional(target).ToFin(Fail: op.InvalidInput())
               from _target in guard(
                   validTarget is not ViewportTarget.EveryCase and not ViewportTarget.DetailCase,
                   op.InvalidInput())
               from _extent in guard(extent.IsValid, op.InvalidInput())
               from validDecor in Optional(decor.IfNone(TransparentDecor.Plain)).ToFin(Fail: op.InvalidInput())
               select new TransparentCaptureSpec(target: validTarget, extent: extent, decor: validDecor);
    }
}
```

## [04]-[RUN_RAIL]

- Owner: `CapturePlan` — the sink-free preparation value. `CaptureRequest` — one non-empty plan sequence paired with one settings-driven sink, with sink-derived cardinality admission. `PreparedCapture` — the one internal disposable prepared-program resource; its settings sequence carries arity, its `Use` gate rejects use after disposal, and reverse release retires every native setting after the sole consumer settles.
- Entry: `Captures.Run(DocumentSession, CaptureRequest, Op?)` prepares and delivers one settings-driven request; `Captures.Run(DocumentSession, TransparentCaptureSpec, Op?)` executes the facade-only request; one internal `Stage(DocumentSession, ReadOnlySpan<CapturePlan>, ...)` brackets the non-empty plan span for PDF composition without a public `ViewCaptureSettings` callback. Printer delivery is a `CaptureSink` case on the same `Run` dispatch, never a second spool entry.
- Law: every entry crosses `HostThread.OnSession` with `SessionNeed.Redraw`; target resolution, settings construction, field application, host validation, delivery, and disposal occur inside the same Rhino command-thread scope.
- Law: batch preparation is one iterative `Fold`. A failed acquisition reverse-disposes every previously prepared setting, and the completed `PreparedCapture` reverse-disposes its sequence after the sole consumer settles.
- Law: preparation applies viewport → area → layout → scale → decoration exactly once, then derives a preview from that completed basis when requested. Viewport binding precedes window projection, aspect matching, and fit scaling; a page subject binds `RhinoPageView.MainViewport`, while view and detail subjects bind the resolved viewport. A settings handle never appears on a public signature, and internal prepared resources reject every use after their lease closes.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record CapturePlan {
    private CapturePlan(CaptureSubject subject, CaptureArea area, CaptureScale scale, MediaLayout layout, CaptureDecor decor) =>
        (Subject, Area, Scale, Layout, Decor) = (subject, area, scale, layout, decor);

    public CaptureSubject Subject { get; }
    public CaptureArea Area { get; }
    public CaptureScale Scale { get; }
    public MediaLayout Layout { get; }
    public CaptureDecor Decor { get; }

    public static Fin<CapturePlan> Of(
        CaptureSubject subject,
        Option<CaptureArea> area = default,
        Option<CaptureScale> scale = default,
        Option<MediaLayout> layout = default,
        Option<CaptureDecor> decor = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from origin in Optional(subject).ToFin(Fail: op.InvalidInput())
               from resolvedArea in Optional(area.IfNone(CaptureArea.FullView)).ToFin(Fail: op.InvalidInput())
               from resolvedScale in Optional(scale.IfNone(CaptureScale.Native)).ToFin(Fail: op.InvalidInput())
               from _scale in guard(resolvedScale.IsValid, op.InvalidInput())
               from resolvedLayout in Optional(layout.IfNone(MediaLayout.Default)).ToFin(Fail: op.InvalidInput())
               from resolvedDecor in Optional(decor.IfNone(CaptureDecor.Of())).ToFin(Fail: op.InvalidInput())
               select new CapturePlan(
                   subject: origin,
                   area: resolvedArea,
                   scale: resolvedScale,
                   layout: resolvedLayout,
                   decor: resolvedDecor);
    }
}

public sealed record CaptureRequest {
    private CaptureRequest(Seq<CapturePlan> plans, CaptureSink sink) => (Plans, Sink) = (plans, sink);

    public Seq<CapturePlan> Plans { get; }
    public CaptureSink Sink { get; }

    public static Fin<CaptureRequest> Of(CaptureSink sink, ReadOnlySpan<CapturePlan> plans, Op? key = null) {
        Op op = key.OrDefault();
        return from admittedSink in Optional(sink).ToFin(Fail: op.InvalidInput())
               from admittedPlans in toSeq(plans.ToArray())
                   .TraverseM(plan => Optional(plan).ToFin(Fail: op.InvalidInput())).As()
               from _rows in guard(!admittedPlans.IsEmpty, op.InvalidInput())
               from _arity in guard(admittedSink.AcceptsMany || admittedPlans.Count == 1, op.InvalidInput())
               select new CaptureRequest(plans: admittedPlans.Strict(), sink: admittedSink);
    }
}

// --- [RESOURCES] ----------------------------------------------------------------------------
internal sealed class PreparedCapture : IDisposable {
    private readonly Seq<ViewCaptureSettings> settings;
    private bool disposed;

    internal PreparedCapture(Seq<ViewCaptureSettings> settings) => this.settings = settings;

    internal Fin<TOut> Use<TOut>(Func<Seq<ViewCaptureSettings>, Fin<TOut>> body, Op key) =>
        from consumer in Optional(body).ToFin(Fail: key.InvalidInput())
        from _live in guard(!disposed, key.InvalidResult())
        from output in key.Catch(() => consumer(settings))
        select output;

    public void Dispose() {
        if (disposed) return;
        disposed = true;
        _ = Release(settings);
    }

    internal static Unit Release(Seq<ViewCaptureSettings> rows) {
        Exception? first = null;
        _ = rows.Rev().Iter(row => {
            try {
                row.Dispose();
            } catch (Exception error) {
                first ??= error;
            }
        });
        if (first is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(first).Throw();
        return unit;
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Captures {
    public static Fin<CaptureArtifact> Run(DocumentSession session, CaptureRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in Optional(request).ToFin(Fail: op.InvalidInput())
               from artifact in HostThread.OnSession(
                   session: session,
                   body: document => Prepare(document: document, plans: admitted.Plans, key: op)
                       .Bind(lease => lease.Use(prepared => admitted.Sink.Render(prepared: prepared, op: op))),
                   op: op,
                   needs: [SessionNeed.Redraw])
               select artifact;
    }

    public static Fin<CaptureArtifact> Run(DocumentSession session, TransparentCaptureSpec spec, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in Optional(spec).ToFin(Fail: op.InvalidInput())
               from artifact in HostThread.OnSession(
                   session: session,
                   body: document => from row in ResolveOne(document: document, target: admitted.Target, key: op)
                                     from captured in Transparent(row: row, spec: admitted, key: op)
                                     select captured,
                   op: op,
                   needs: [SessionNeed.Redraw])
               select artifact;
    }

    internal static Fin<TOut> Stage<TOut>(
        DocumentSession session,
        ReadOnlySpan<CapturePlan> plans,
        Func<PreparedCapture, Fin<TOut>> consume,
        Op? key = null) {
        Op op = key.OrDefault();
        Seq<CapturePlan> requested = toSeq(plans.ToArray());
        return from body in Optional(consume).ToFin(Fail: op.InvalidInput())
               from admitted in requested.TraverseM(plan => Optional(plan).ToFin(Fail: op.InvalidInput())).As()
               from _rows in guard(!admitted.IsEmpty, op.InvalidInput())
               from output in HostThread.OnSession(
                   session: session,
                   body: document => Prepare(document: document, plans: admitted, key: op)
                       .Bind(lease => lease.Use(body)),
                   op: op,
                   needs: [SessionNeed.Redraw])
               select output;
    }

    private static Fin<Lease<PreparedCapture>> Prepare(RhinoDoc document, Seq<CapturePlan> plans, Op key) =>
        plans.Fold(
                Fin.Succ(value: Seq<ViewCaptureSettings>()),
                (state, plan) => state.Bind(held => PrepareOne(document: document, plan: plan, key: key).Match(
                    Succ: prepared => Fin.Succ(value: held.Add(prepared)),
                    Fail: error => key.Catch(() => Fin.Succ(value: PreparedCapture.Release(rows: held)))
                        .Bind(_ => Fin.Fail<Seq<ViewCaptureSettings>>(error: error)))))
            .Map(rows => (Lease<PreparedCapture>)new Lease<PreparedCapture>.Owned(Value: new PreparedCapture(settings: rows)));

    private static Fin<ViewCaptureSettings> PrepareOne(RhinoDoc document, CapturePlan plan, Op key) =>
        from admitted in Optional(plan).ToFin(Fail: key.InvalidInput())
        from row in ResolveOne(document: document, target: admitted.Subject.Address, key: key)
        from basis in Settings(row: row, subject: admitted.Subject, key: key)
        from configured in Apply(row: row, settings: basis, plan: admitted, key: key).Match(
            Succ: _ => Fin.Succ(value: basis),
            Fail: error => {
                basis.Dispose();
                return Fin.Fail<ViewCaptureSettings>(error: error);
            })
        from settings in Previewed(settings: configured, subject: admitted.Subject, key: key)
        select settings;

    private static Fin<Unit> Apply(ViewportRef row, ViewCaptureSettings settings, CapturePlan plan, Op key) =>
        from viewport in plan.Subject.Viewport(row: row, key: key)
        from _bind in key.Catch(() => Fin.Succ(value: Op.Side(() => settings.SetViewport(viewport: viewport))))
        from _area in plan.Area.Apply(settings: settings, key: key)
        from _layout in plan.Layout.Apply(settings: settings, key: key)
        from _scale in plan.Scale.Apply(settings: settings, key: key)
        from _decor in plan.Decor.Apply(settings: settings, key: key)
        from _valid in guard(settings.IsValid, key.InvalidResult())
        select unit;

    private static Fin<ViewportRef> ResolveOne(RhinoDoc document, ViewportTarget target, Op key) =>
        from rows in target.Resolve(document: document, key: key)
        from _single in guard(rows.Count == 1, key.InvalidInput())
        from row in rows.Head.ToFin(Fail: key.MissingContext())
        select row;

    private static Fin<ViewCaptureSettings> Settings(ViewportRef row, CaptureSubject subject, Op key) => subject.Switch(
        state: (Row: row, Op: key),
        viewCase: static (ctx, view) => ctx.Op.Catch(() => Fin.Succ(
            value: new ViewCaptureSettings(ctx.Row.View, view.Pixels.Native, view.Dpi.Value))),
        pageCase: static (ctx, page) => Optional(ctx.Row.View as RhinoPageView).ToFin(Fail: ctx.Op.InvalidInput())
            .Bind(view => ctx.Op.Catch(() => Fin.Succ(value: new ViewCaptureSettings(view, page.Dpi.Value)))),
        previewCase: static (ctx, preview) => guard(preview.Source is CaptureSubject.ViewCase or CaptureSubject.PageCase, ctx.Op.InvalidInput())
            .ToFin()
            .Bind(_ => Settings(row: ctx.Row, subject: preview.Source, key: ctx.Op)));

    private static Fin<ViewCaptureSettings> Previewed(ViewCaptureSettings settings, CaptureSubject subject, Op key) => subject.Switch(
        state: (Settings: settings, Op: key),
        viewCase: static (ctx, _) => Fin.Succ(value: ctx.Settings),
        pageCase: static (ctx, _) => Fin.Succ(value: ctx.Settings),
        previewCase: static (ctx, preview) => ctx.Op.Catch(() => {
            using ViewCaptureSettings basis = ctx.Settings;
            ViewCaptureSettings? derived = basis.CreatePreviewSettings(preview.Pixels.Native);
            try {
                if (derived is null || !derived.IsValid) return Fin.Fail<ViewCaptureSettings>(error: ctx.Op.InvalidResult());
                ViewCaptureSettings admitted = derived;
                derived = null;
                return Fin.Succ(value: admitted);
            } finally {
                derived?.Dispose();
            }
        }));

    private static Fin<CaptureArtifact> Transparent(ViewportRef row, TransparentCaptureSpec spec, Op key) => key.Catch(() => {
        ViewCapture facade = new() {
            Width = spec.Extent.Width,
            Height = spec.Extent.Height,
            TransparentBackground = true,
            DrawGrid = spec.Decor.Grid,
            DrawAxes = spec.Decor.Axes,
            DrawGridAxes = spec.Decor.GridAxes,
            ScaleScreenItems = spec.Decor.ScaleScreenItems,
        };
        return from bitmap in Optional(facade.CaptureToBitmap(sourceView: row.View)).ToFin(Fail: key.InvalidResult())
               from artifact in CaptureArtifact.Raster(bitmap: bitmap, key: key)
               select artifact;
    });
}
```

Question: How does each admitted `Run` request reach one `CaptureArtifact` while native settings remain bracketed?

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Capture request dispatch
    accDescr: One capture entry dispatches settings and transparent requests, keeps prepared settings bracketed, and rejoins bitmap, SVG, and printer arms at one artifact.
    Run([Captures.Run]) --> Shape{Request shape?}
    Shape -->|"settings"| Prepare[Prepare owned batch]
    Shape -->|"transparent"| Transparent[Capture transparent bitmap]
    Prepare -->|"dispatches"| Sink{Sink case?}
    Sink -->|"bitmap"| Raster[Capture bitmap]
    Sink -->|"SVG"| Vector[Capture SVG]
    Sink -->|"printer"| Print[Send page batch]
    Raster -->|"returns"| Artifact[/CaptureArtifact/]
    Vector -->|"returns"| Artifact
    Print -->|"returns"| Artifact
    Transparent -->|"returns"| Artifact
    linkStyle 7,8,9,10 stroke:#50FA7B,color:#F8F8F2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef success fill:#50FA7BBF,stroke:#50FA7B,color:#282A36
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    class Shape,Prepare,Sink,Raster,Vector,Print,Transparent primary
    class Artifact success
    class Run boundary
```
