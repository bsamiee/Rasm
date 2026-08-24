# [RASM_RHINO_CAPTURE]

Capture ownership (`Rasm.Rhino.Viewport`) prepares native `ViewCaptureSettings` batches, drives the transparent-raster, depth, and frame-sequence host facades, and converges every product on one `CaptureArtifact` family. The page is sink-FREE: preparation, measurement, and the artifact vocabulary live here, while raster, vector, and printer DELIVERY are cases of `Exchange/publish`'s `Landing` — an S4 owner this S3 page never names and never reaches upward for.

`ViewportFault` seats here as the sub-domain's refusal family, coded on the kernel `FaultBand.HostViewport` row; every generated owner across `Viewport/*` stamps `[ValidationError]`. Measurement stays receipt evidence on successful artifacts, while failures preserve their exact cause without a measured-fault wrapper.

Drawing STANDARDS are the kernel's whole: margins come from `SheetFrame`, model scale from `DrawingScale`, plotted magnitudes from `LineGroup`/`Terminator`/`TextHeight`, output resolution from `PlotResolution`, and the sun study's north bearing from `NorthPosture` over the model's own declination — this page authors no standards figure of its own.

## [01]-[INDEX]

- [02]-[FAULT]: `ViewportFault` — the sub-domain's one refusal family on the kernel band registry, and the folder law that seats it.
- [03]-[SPEC_AXES]: admitted extents, origins, resolution, subject, area, scale, media layout, and the `CaptureFeature` capability table with its per-surface rosters.
- [04]-[ARTIFACT_ROWS]: the transparent and depth request specifications, the depth projection/payload pair, and the one `CaptureArtifact` family with its coverage carriage.
- [05]-[FRAME_SEQUENCE]: document-custodied animation capture — sequence kinds, the `SunWindow` calendar family, output rows, the generated host transcription, and the frame receipt.
- [06]-[RUN_RAIL]: sink-free plans, the modality union with its own demand and identity, the nested preparation bracket, and the one measured execution fold.

## [02]-[FAULT]

- Owner: `ViewportFault` is the direct host-boundary family on `FaultBand.HostViewport`; generated-value refusals cross the kernel validation bridge.
- Cases: `HostRefused` is the semantic viewport refusal; `KernelFault` owns generated admission and foreign host failures retain their original `Error`.
- Law: one fault family per kernel band row. `DraftFault` never codes viewport failures, and generated owners stamp only `[ValidationError]`.
- Law: the generated fault-case identity supplies the numeric code, while this root's total `Message` switch supplies presentation.
- Law: measurement decorates a successful artifact; failure preserves the original cause without minting a viewport wrapper.
- Law: no category or string identity is stored or wired; telemetry projects the numeric identity only for domain faults.
- Receipt: none — the fault IS the evidence; the band registry proof and the `OwnerOf` reverse read are the kernel's.
- Packages: `Domain/rails` for `FaultBand` and the rail; Thinktecture.Runtime.Extensions for generated unions and values; `Modeling/solids` for `BenchEvidence`.
- Growth: a new refusal class is one case, one offset row, and one message row inside the band's span; the band's own span guard throws at type init when the span is spent.
- Boundary: `ViewportFault` is the Viewport family alone — Exchange, Render, Plugin, and Persistence each mint their own on their own band row, and the kernel `UiFault` stays the one UI refusal family.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Modeling;
using Thinktecture;

namespace Rasm.Rhino.Viewport;

// --- [ERRORS] -------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewportFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.HostViewport;
    private ViewportFault() { }

    [FaultCase(0)] public sealed partial record HostRefused(Op Key, string Member, string Detail) : ViewportFault;

    public sealed override string Message => Switch(
        hostRefused: static fault => $"Viewport host member '{fault.Member}' refused '{fault.Key}': {fault.Detail}");
}
```

## [03]-[SPEC_AXES]

- Owner: `Size2i`, `Offset2i`, and `CaptureDpi` are the generated pixel-extent, pixel-origin, and resolution values; `CaptureAnchor` and `CaptureColor` mirror their host enums keyed on the host ordinal itself; `TargetReach` carries the two address regimes a capture admits; `CaptureSubject`, `CaptureArea`, `CaptureScale`, and `MediaLayout` close the request axes; `CaptureFeature` is the host toggle vocabulary as a kernel capability, `CaptureSurface` the three projection surfaces that read it, and `CaptureDecor` the settings-side decoration.
- Entry: `Size2i.Of` / `Offset2i.Of` / `CaptureDpi.Of` admit through the generated `Validate`; `CaptureDpi.Of(PlotResolution)` is the output-class arity, so no capture request spells a DPI literal. `CaptureSubject.View`/`Page`/`Preview` admit a scalar address through `TargetReach`; `MediaLayout.Viewport`/`Crop`/`Margins`/`Maximize` admit the media frame, and `Margins` takes either an authored `SheetMargin` or the `SheetSize` whose standard publishes one.
- Auto: a `CaptureSurface` row's ROSTER derives by filtering the feature table on that surface's own column, and its DEFAULT derives from a declared seed — so a new host toggle joins every surface it names by declaring that surface's column, and no per-surface roster is hand-kept beside the table it mirrors. The three `FrozenSet`-wrapping value objects the prior page carried, each with its own `All(column is not null)` roster guard, have no successor: `CaptureSurface.Depth.Roster` IS the former `Everything`, `CapabilitySet<CaptureFeature>.None` the former `Surfaces`, and the remaining named preset is the surface's own `Default`.
- Law: drawing figures are the kernel's. `MediaLayout.Margins(SheetSize, …)` reads `SheetFrame.For(size.Standard).Margin(size, key)` — the standard's own binding-and-edge quad, ISO 5457's 20 mm binding against 10 mm elsewhere included; `CaptureScale.ToValue` takes a reduced `DrawingScale` pair and lowers its `Ratio` at the one host member; `PrintFidelity.For(size, …)` derives the default print width from `LineGroup.For(size)`'s narrow rung, the arrowhead from `Terminator.Size(group.Wide)`, and the text-dot point size from `TextHeight.For(size)`. Five free doubles, four free margin doubles, and a free scale double all delete for rungs a standard publishes.
- Law: a unit regime crosses this page as the kernel `ModelUnit` and lowers `.System` only at the host member that takes a `UnitSystem`; `UnitSystem.CustomUnits` refuses at admission because a custom scale lives on `LengthUnit`, which no margin or offset member accepts.
- Law: a host `bool` never leaves its row. `OffsetOrigin`, `AspectPolicy`, and `PrintWidthPolicy` each carry a delegate column performing their own host write, so the two-row vocabulary IS the lowering and no consumer reads the flag; each names one INDEPENDENT host parameter with no legal-corner law between them, which is why they stay three rows rather than folding into a capability set.
- Law: absence at construction is unrepresentable, not guarded per use. `Size2i` and `CaptureDpi` refuse the default struct outright, `Offset2i` names its legal default `Origin` because a zero pixel origin is a real address, and the eleven `IsValid` re-probes the prior page carried against `default(T)` ghosts have no successor.
- Law: native `System.Drawing.Size` and `Rectangle` values mint only through the owners' own projections — `Size2i.Native` and `Offset2i.Window(Size2i)` — and an integer position never rides the extent type.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Rhino.Document;
using Rasm.Rhino.HostUi;
using Rasm.Rhino.Modeling;
using System.Collections.Frozen;
using System.Globalization;
using System.Runtime.InteropServices;
using Thinktecture;
using UnitsNet.Units;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] --------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Size2i : IDisallowDefaultValue {
    public int Width { get; }
    public int Height { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int width, ref int height) =>
        validationError = ValidityClaim.All(width > 0, height > 0, (long)width * height <= int.MaxValue)
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(Size2i), "positive pixel extents whose area fits an int" }));

    public static Fin<Size2i> Of(int width, int height, Op? key = null) =>
        key.OrDefault().AcceptValidated<Size2i>(fault: Validate(width, height, out Size2i admitted), admitted: admitted);

    internal System.Drawing.Size Native => new(Width, Height);
}

[ComplexValueObject(AllowDefaultStructs = true, DefaultInstancePropertyName = "Origin")]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Offset2i {
    public int X { get; }
    public int Y { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int x, ref int y) =>
        validationError = ValidityClaim.All(x >= 0, y >= 0)
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(Offset2i), "nonnegative pixel coordinates" }));

    public static Fin<Offset2i> Of(int x, int y, Op? key = null) =>
        key.OrDefault().AcceptValidated<Offset2i>(fault: Validate(x, y, out Offset2i admitted), admitted: admitted);

    internal System.Drawing.Rectangle Window(Size2i extent) => new(X, Y, extent.Width, extent.Height);
}

[ValueObject<double>]
[ValidationError]
public readonly partial struct CaptureDpi : IDisallowDefaultValue {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = ValidityClaim.All(ValidityClaim.Finite(value), value > 0.0)
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(CaptureDpi), value, "a finite positive resolution" }));

    public static Fin<CaptureDpi> Of(double value, Op? key = null) =>
        key.OrDefault().AcceptValidated<CaptureDpi>(candidate: value);

    public static Fin<CaptureDpi> Of(PlotResolution resolution, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(value: resolution).Bind(row => Of(value: row.Dpi.Value, key: op));
    }
}

[SmartEnum<int>]
public sealed partial class CaptureAnchor {
    public static readonly CaptureAnchor LowerLeft = new(key: (int)ViewCaptureSettings.AnchorLocation.LowerLeft);
    public static readonly CaptureAnchor LowerRight = new(key: (int)ViewCaptureSettings.AnchorLocation.LowerRight);
    public static readonly CaptureAnchor UpperLeft = new(key: (int)ViewCaptureSettings.AnchorLocation.UpperLeft);
    public static readonly CaptureAnchor UpperRight = new(key: (int)ViewCaptureSettings.AnchorLocation.UpperRight);
    public static readonly CaptureAnchor Center = new(key: (int)ViewCaptureSettings.AnchorLocation.Center);

    internal ViewCaptureSettings.AnchorLocation Native => (ViewCaptureSettings.AnchorLocation)Key;
}

[SmartEnum<int>]
public sealed partial class CaptureColor {
    public static readonly CaptureColor Display = new(key: (int)ViewCaptureSettings.ColorMode.DisplayColor);
    public static readonly CaptureColor Print = new(key: (int)ViewCaptureSettings.ColorMode.PrintColor);
    public static readonly CaptureColor Monochrome = new(key: (int)ViewCaptureSettings.ColorMode.BlackAndWhite);

    internal ViewCaptureSettings.ColorMode Native => (ViewCaptureSettings.ColorMode)Key;
}

// The transparent row refuses a DETAIL because `ViewCapture.CaptureToBitmap` takes a `RhinoView` and a detail is a
// viewport inside one — the one exclusion the two regimes do not share.
[SmartEnum<int>]
internal sealed partial class TargetReach {
    internal static readonly TargetReach Row = new(
        key: 0, admits: static target => target is not ViewportTarget.EveryCase);
    internal static readonly TargetReach View = new(
        key: 1, admits: static target => target is not ViewportTarget.EveryCase and not ViewportTarget.DetailCase);

    [UseDelegateFromConstructor]
    internal partial bool Admits(ViewportTarget target);

    internal Fin<ViewportTarget> Admit(ViewportTarget target, Op key) =>
        from held in key.Need(value: target)
        from _reach in guard(Admits(target: held), key.InvalidInput()).ToFin()
        select held;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSubject {
    private CaptureSubject() { }

    internal sealed record ViewCase(ViewportTarget Target, Size2i Pixels, CaptureDpi Dpi) : CaptureSubject;
    internal sealed record PageCase(ViewportTarget Target, CaptureDpi Dpi) : CaptureSubject;
    internal sealed record PreviewCase(CaptureSubject Source, Size2i Pixels) : CaptureSubject;

    public static Fin<CaptureSubject> View(ViewportTarget target, Size2i pixels, CaptureDpi dpi, Op? key = null) {
        Op op = key.OrDefault();
        return TargetReach.Row.Admit(target: target, key: op)
            .Map(valid => (CaptureSubject)new ViewCase(Target: valid, Pixels: pixels, Dpi: dpi));
    }

    public static Fin<CaptureSubject> Page(ViewportTarget target, CaptureDpi dpi, Op? key = null) {
        Op op = key.OrDefault();
        return from valid in op.Need(value: target)
               from _page in guard(valid is ViewportTarget.PageCase, op.InvalidInput()).ToFin()
               select (CaptureSubject)new PageCase(Target: valid, Dpi: dpi);
    }

    public static Fin<CaptureSubject> Preview(CaptureSubject source, Size2i pixels, Op? key = null) {
        Op op = key.OrDefault();
        return from valid in op.Need(value: source)
               from _source in guard(valid is ViewCase or PageCase, op.InvalidInput()).ToFin()
               select (CaptureSubject)new PreviewCase(Source: valid, Pixels: pixels);
    }

    internal ViewportTarget Address => Switch(
        viewCase: static view => view.Target,
        pageCase: static page => page.Target,
        previewCase: static preview => preview.Source.Address);

    // Recursive on the preview arm: the basis mints, the preview derives from it, and the basis releases the moment
    // the derivation returns, so no completed basis outlives the settings it produced.
    internal Fin<Lease<ViewCaptureSettings>> Realize(ViewportRef row, Op key) => Switch(
        (Row: row, Op: key),
        viewCase: static (ctx, view) => Lease<ViewCaptureSettings>.Acquire(
            mint: () => new ViewCaptureSettings(ctx.Row.View, view.Pixels.Native, (double)view.Dpi), key: ctx.Op),
        pageCase: static (ctx, page) => ctx.Op.Need(ctx.Row.View as RhinoPageView).Bind(view =>
            Lease<ViewCaptureSettings>.Acquire(mint: () => new ViewCaptureSettings(view, (double)page.Dpi), key: ctx.Op)),
        previewCase: static (ctx, preview) => preview.Source.Realize(row: ctx.Row, key: ctx.Op)
            .Bind(basis => basis.Use(
                body: held => Lease<ViewCaptureSettings>.Acquire(
                    mint: () => held.CreatePreviewSettings(preview.Pixels.Native), key: ctx.Op),
                key: ctx.Op)));
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
        guard(ValidityClaim.All(a.IsValid, b.IsValid, a != b), key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (CaptureArea)new ScreenWindowCase(A: a, B: b));

    public static Fin<CaptureArea> WorldWindow(Point3d a, Point3d b, Op? key = null) =>
        guard(ValidityClaim.All(a.IsValid, b.IsValid, a != b), key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (CaptureArea)new WorldWindowCase(A: a, B: b));

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) => Switch(
        (Settings: settings, Op: key),
        fullViewCase: static (ctx, _) => ctx.Op.Catch(() => ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.View),
        extentsCase: static (ctx, _) => ctx.Op.Catch(() => ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Extents),
        screenWindowCase: static (ctx, area) => ctx.Op.Catch(() => {
            ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
            ctx.Settings.SetWindowRect(screenPoint1: area.A, screenPoint2: area.B);
        }),
        worldWindowCase: static (ctx, area) => ctx.Op.Catch(() => {
            ctx.Settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
            ctx.Settings.SetWindowRect(worldPoint1: area.A, worldPoint2: area.B);
        }));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureScale {
    private CaptureScale() { }

    internal sealed record NativeCase : CaptureScale;
    internal sealed record ToValueCase(DrawingScale Scale) : CaptureScale;
    internal sealed record ToFitCase : CaptureScale;

    public static CaptureScale Native { get; } = new NativeCase();
    public static CaptureScale ToFit { get; } = new ToFitCase();

    public static Fin<CaptureScale> ToValue(DrawingScale scale, Op? key = null) =>
        key.OrDefault().Need(value: scale).Map(static admitted => (CaptureScale)new ToValueCase(Scale: admitted));

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) => Switch(
        (Settings: settings, Op: key),
        nativeCase: static (_, _) => Fin.Succ(value: unit),
        toValueCase: static (ctx, value) => ctx.Op.Catch(() => ctx.Settings.SetModelScaleToValue(scale: value.Scale.Ratio)),
        toFitCase: static (ctx, _) => ctx.Op.Catch(() => ctx.Settings.SetModelScaleToFit(promptOnChange: false)));
}

// --- [POLICIES] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class OffsetOrigin {
    public static readonly OffsetOrigin Margin = new(key: 0, seat: static (settings, offset) =>
        Op.Side(() => settings.SetOffset(lengthUnits: offset.Units.System, fromMargin: true, x: offset.X, y: offset.Y)));
    public static readonly OffsetOrigin Media = new(key: 1, seat: static (settings, offset) =>
        Op.Side(() => settings.SetOffset(lengthUnits: offset.Units.System, fromMargin: false, x: offset.X, y: offset.Y)));

    [UseDelegateFromConstructor]
    internal partial Unit Seat(ViewCaptureSettings settings, CaptureOffset offset);
}

[SmartEnum<int>]
public sealed partial class AspectPolicy {
    public static readonly AspectPolicy MatchViewport = new(key: 0,
        apply: static (settings, key) => key.Catch(() => settings.MatchViewportAspectRatio()
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(new ViewportFault.HostRefused(
                Key: key, Member: nameof(ViewCaptureSettings.MatchViewportAspectRatio), Detail: "answered false"))));
    public static readonly AspectPolicy PreserveMedia = new(key: 1,
        apply: static (_, _) => Fin.Succ(value: unit));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Apply(ViewCaptureSettings settings, Op key);
}

// CTB/STB participation: the host decides whether plotted widths come from the objects' print widths or from the
// screen, and no drawing standard publishes that switch — it stays a host row.
[SmartEnum<int>]
public sealed partial class PrintWidthPolicy {
    public static readonly PrintWidthPolicy Model = new(key: 0, seat: static settings => Op.Side(() => settings.UsePrintWidths = true));
    public static readonly PrintWidthPolicy Screen = new(key: 1, seat: static settings => Op.Side(() => settings.UsePrintWidths = false));

    [UseDelegateFromConstructor]
    internal partial Unit Seat(ViewCaptureSettings settings);
}

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class CaptureCrop {
    public Size2i Media { get; }
    public Offset2i Origin { get; }
    public Size2i Extent { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Size2i media,
        ref Offset2i origin,
        ref Size2i extent) =>
        validationError = ValidityClaim.All(
            (long)origin.X + extent.Width <= media.Width,
            (long)origin.Y + extent.Height <= media.Height)
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(CaptureCrop), "a crop window inside the media extent" }));

    public static Fin<CaptureCrop> Of(Size2i media, Offset2i origin, Size2i extent, Op? key = null) =>
        key.OrDefault().AcceptValidated<CaptureCrop>(fault: Validate(media, origin, extent, out CaptureCrop? admitted), admitted: admitted);
}

// The regime arrives already admitted and lowers to `UnitSystem` only at the host member; a custom scale lives on
// `LengthUnit`, which that member cannot take, so `CustomUnits` refuses here rather than at the native call.
[ComplexValueObject]
[ValidationError]
public sealed partial class CaptureOffset {
    public ModelUnit Units { get; }
    public OffsetOrigin Origin { get; }
    public double X { get; }
    public double Y { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ModelUnit units,
        ref OffsetOrigin origin,
        ref double x,
        ref double y) =>
        validationError = ValidityClaim.All(
            units is { System: not UnitSystem.CustomUnits },
            ValidityClaim.Finite([x, y]), x >= 0.0, y >= 0.0)
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] {
                Op.Of(), nameof(CaptureOffset), "a non-custom unit regime and finite nonnegative offsets" }));

    public static Fin<CaptureOffset> Of(ModelUnit units, OffsetOrigin origin, double x, double y, Op? key = null) =>
        key.OrDefault().AcceptValidated<CaptureOffset>(fault: Validate(units, origin, x, y, out CaptureOffset? admitted), admitted: admitted);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class CaptureBanner {
    public string Header { get; }
    public string Footer { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string header,
        ref string footer) {
        header = header?.Trim() ?? string.Empty;
        footer = footer?.Trim() ?? string.Empty;
        validationError = header.Length > 0 || footer.Length > 0
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(CaptureBanner) }));
    }

    public static Fin<CaptureBanner> Of(string header, string footer, Op? key = null) =>
        key.OrDefault().AcceptValidated<CaptureBanner>(fault: Validate(header, footer, out CaptureBanner? admitted), admitted: admitted);
}

// The wire-thickness scale is the ONE column no standard publishes — a pure display multiplier — so it stays an
// admitted magnitude beside five rungs that are all read off the sheet extent.
[ComplexValueObject]
public sealed partial class PrintFidelity {
    public PrintWidthPolicy Widths { get; }
    public LineGroup Group { get; }
    public Terminator Arrowhead { get; }
    public TextHeight Dot { get; }
    public LineWidth Point { get; }
    public PositiveMagnitude WireScale { get; }

    public static Fin<PrintFidelity> For(
        SheetSize size,
        PrintWidthPolicy widths,
        Option<Terminator> arrowhead = default,
        Option<PositiveMagnitude> wireScale = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admittedWidths in op.Need(value: widths)
               from group in LineGroup.For(size: size, key: op)
               from dot in TextHeight.For(size: size, key: op)
               from scale in wireScale.Match(Some: Fin.Succ, None: () => op.AcceptValidated<PositiveMagnitude>(candidate: 1.0))
               select Create(
                   widths: admittedWidths,
                   group: group,
                   arrowhead: arrowhead.IfNone(Terminator.ClosedArrow),
                   dot: dot,
                   point: group.Wide,
                   wireScale: scale);
    }

    internal Unit Seat(ViewCaptureSettings settings) {
        _ = Widths.Seat(settings: settings);
        settings.WireThicknessScale = (double)WireScale;
        settings.PointSizeMillimeters = Point.Width.Millimeters;
        settings.ArrowheadSizeMillimeters = Arrowhead.Size(width: Group.Wide).Millimeters;
        settings.TextDotPointSize = Dot.Height.As(LengthUnit.DtpPoint);
        settings.DefaultPrintWidthMillimeters = Group.Narrow.Width.Millimeters;
        return unit;
    }
}

[ComplexValueObject]
public sealed partial class MediaPlacement {
    public static MediaPlacement Default { get; } = Create(
        offset: None,
        anchor: None,
        aspect: AspectPolicy.MatchViewport);

    public Option<CaptureOffset> Offset { get; }
    public Option<CaptureAnchor> Anchor { get; }
    public AspectPolicy Aspect { get; }

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) => key.Catch(() => {
        MediaPlacement self = this;
        _ = self.Offset.Iter(offset => offset.Origin.Seat(settings: settings, offset: offset));
        _ = self.Anchor.Iter(anchor => settings.OffsetAnchor = anchor.Native);
        return self.Aspect.Apply(settings: settings, key: key);
    });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MediaLayout {
    private MediaLayout() { }
    internal sealed record ViewportCase(MediaPlacement Placement) : MediaLayout;
    internal sealed record CropCase(CaptureCrop Crop, MediaPlacement Placement) : MediaLayout;
    internal sealed record MarginsCase(SheetMargin Margins, ModelUnit Units, MediaPlacement Placement) : MediaLayout;
    internal sealed record MaximizeCase(MediaPlacement Placement) : MediaLayout;

    public static MediaLayout Default { get; } = new ViewportCase(Placement: MediaPlacement.Default);

    public static Fin<MediaLayout> Viewport(Option<MediaPlacement> placement = default, Op? key = null) =>
        Placed(placement: placement, op: key.OrDefault())
            .Map(static admitted => (MediaLayout)new ViewportCase(Placement: admitted));

    public static Fin<MediaLayout> Crop(CaptureCrop crop, Option<MediaPlacement> placement = default, Op? key = null) {
        Op op = key.OrDefault();
        return from admittedCrop in op.Need(value: crop)
               from admittedPlacement in Placed(placement: placement, op: op)
               select (MediaLayout)new CropCase(Crop: admittedCrop, Placement: admittedPlacement);
    }

    // Two admissions, one case, discriminated by input SHAPE: an authored quad, or the sheet whose own standard
    // publishes one.
    public static Fin<MediaLayout> Margins(SheetMargin margins, ModelUnit units, Option<MediaPlacement> placement = default, Op? key = null) {
        Op op = key.OrDefault();
        return from admittedMargins in op.Need(value: margins)
               from admittedUnits in op.Need(value: units)
               from admittedPlacement in Placed(placement: placement, op: op)
               select (MediaLayout)new MarginsCase(Margins: admittedMargins, Units: admittedUnits, Placement: admittedPlacement);
    }

    public static Fin<MediaLayout> Margins(SheetSize size, ModelUnit units, Option<MediaPlacement> placement = default, Op? key = null) {
        Op op = key.OrDefault();
        return from admittedSize in op.Need(value: size)
               from frame in SheetFrame.For(standard: admittedSize.Standard).Margin(size: admittedSize, key: op)
               from layout in Margins(margins: frame, units: units, placement: placement, key: op)
               select layout;
    }

    public static Fin<MediaLayout> Maximize(Option<MediaPlacement> placement = default, Op? key = null) =>
        Placed(placement: placement, op: key.OrDefault())
            .Map(static admitted => (MediaLayout)new MaximizeCase(Placement: admitted));

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) => Switch(
        (Settings: settings, Op: key),
        viewportCase: static (ctx, layout) => layout.Placement.Apply(settings: ctx.Settings, key: ctx.Op),
        cropCase: static (ctx, layout) => ctx.Op.Catch(() => {
            ctx.Settings.SetLayout(mediaSize: layout.Crop.Media.Native, cropRectangle: layout.Crop.Origin.Window(extent: layout.Crop.Extent));
            return layout.Placement.Apply(settings: ctx.Settings, key: ctx.Op);
        }),
        marginsCase: static (ctx, layout) =>
            from inset in layout.Margins.In(unit: layout.Units, key: ctx.Op)
            from _seated in ctx.Op.Catch(() => ctx.Settings.SetMargins(
                    lengthUnits: layout.Units.System,
                    left: inset.Left,
                    top: inset.Top,
                    right: inset.Right,
                    bottom: inset.Bottom)
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(new ViewportFault.HostRefused(
                    Key: ctx.Op, Member: nameof(ViewCaptureSettings.SetMargins), Detail: "answered false")))
            from placed in layout.Placement.Apply(settings: ctx.Settings, key: ctx.Op)
            select placed,
        maximizeCase: static (ctx, layout) => ctx.Op.Catch(() => {
            ctx.Settings.MaximizePrintableArea();
            return layout.Placement.Apply(settings: ctx.Settings, key: ctx.Op);
        }));

    private static Fin<MediaPlacement> Placed(Option<MediaPlacement> placement, Op op) =>
        op.Need(value: placement.IfNone(MediaPlacement.Default));
}

// --- [CAPABILITY] ---------------------------------------------------------------------------
// The host toggle roster as a kernel CAPABILITY vocabulary: three projection surfaces read the same rows, and a
// row participates on a surface exactly when it declares that surface's column. Provenance is the host member
// roster — `ViewCaptureSettings.Draw*`, `ViewCapture.Draw*`, `ZBufferCapture.Show*`.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CaptureFeature : ICapability<CaptureFeature> {
    public static readonly CaptureFeature Grid = Row(key: "grid",
        settings: static (target, on) => target.DrawGrid = on,
        transparent: static (target, on) => target.DrawGrid = on);
    public static readonly CaptureFeature Axes = Row(key: "axes",
        settings: static (target, on) => target.DrawAxis = on,
        transparent: static (target, on) => target.DrawAxes = on);
    public static readonly CaptureFeature Raster = Row(key: "raster",
        settings: static (target, on) => target.RasterMode = on);
    public static readonly CaptureFeature Background = Row(key: "background",
        settings: static (target, on) => target.DrawBackground = on);
    public static readonly CaptureFeature BackgroundBitmap = Row(key: "background-bitmap",
        settings: static (target, on) => target.DrawBackgroundBitmap = on);
    public static readonly CaptureFeature Wallpaper = Row(key: "wallpaper",
        settings: static (target, on) => target.DrawWallpaper = on);
    public static readonly CaptureFeature LockedObjects = Row(key: "locked",
        settings: static (target, on) => target.DrawLockedObjects = on);
    public static readonly CaptureFeature SelectedOnly = Row(key: "selected-only",
        settings: static (target, on) => target.DrawSelectedObjectsOnly = on);
    public static readonly CaptureFeature ClippingPlanes = Row(key: "clipping",
        settings: static (target, on) => target.DrawClippingPlanes = on);
    public static readonly CaptureFeature Lights = Row(key: "lights",
        settings: static (target, on) => target.DrawLights = on,
        depth: static (target, on) => target.ShowLights(on: on));
    public static readonly CaptureFeature MarginLines = Row(key: "margins",
        settings: static (target, on) => target.DrawMargins = on);
    public static readonly CaptureFeature GridAxes = Row(key: "grid-axes",
        transparent: static (target, on) => target.DrawGridAxes = on);
    public static readonly CaptureFeature ScaleScreenItems = Row(key: "scale-screen-items",
        transparent: static (target, on) => target.ScaleScreenItems = on);
    public static readonly CaptureFeature Isocurves = Row(key: "isocurves",
        depth: static (target, on) => target.ShowIsocurves(on: on));
    public static readonly CaptureFeature MeshWires = Row(key: "mesh-wires",
        depth: static (target, on) => target.ShowMeshWires(on: on));
    public static readonly CaptureFeature Curves = Row(key: "curves",
        depth: static (target, on) => target.ShowCurves(on: on));
    public static readonly CaptureFeature Points = Row(key: "points",
        depth: static (target, on) => target.ShowPoints(on: on));
    public static readonly CaptureFeature Text = Row(key: "text",
        depth: static (target, on) => target.ShowText(on: on));
    public static readonly CaptureFeature Annotations = Row(key: "annotations",
        depth: static (target, on) => target.ShowAnnotations(on: on));

    internal Option<Action<ViewCaptureSettings, bool>> Settings { get; }
    internal Option<Action<ViewCapture, bool>> Transparent { get; }
    internal Option<Action<ZBufferCapture, bool>> Depth { get; }

    // ONE fold serves every projection surface: an absent column is a row this surface does not project, so a
    // fourth host facade is a fourth column and not a fourth loop.
    internal static Unit Apply<TTarget>(
        TTarget target,
        Func<CaptureFeature, Option<Action<TTarget, bool>>> column,
        CapabilitySet<CaptureFeature> held) =>
        toSeq(Items).Iter(feature => column(feature).Iter(write => write(target, held.Admits(capability: feature))));

    private static CaptureFeature Row(
        string key,
        Action<ViewCaptureSettings, bool>? settings = null,
        Action<ViewCapture, bool>? transparent = null,
        Action<ZBufferCapture, bool>? depth = null) =>
        new(key: key, settings: Optional(settings), transparent: Optional(transparent), depth: Optional(depth));
}

[SmartEnum<int>]
public sealed partial class CaptureSurface {
    public static readonly CaptureSurface Settings = new(
        key: 0,
        holds: static feature => feature.Settings.IsSome,
        seed: static () => Seq(CaptureFeature.Background, CaptureFeature.LockedObjects, CaptureFeature.ClippingPlanes, CaptureFeature.Lights));
    public static readonly CaptureSurface Transparent = new(
        key: 1,
        holds: static feature => feature.Transparent.IsSome,
        seed: static () => Seq(CaptureFeature.ScaleScreenItems));
    public static readonly CaptureSurface Depth = new(
        key: 2,
        holds: static feature => feature.Depth.IsSome,
        seed: static () => Seq(CaptureFeature.Isocurves, CaptureFeature.MeshWires, CaptureFeature.Curves, CaptureFeature.Points));

    [UseDelegateFromConstructor]
    internal partial bool Holds(CaptureFeature feature);

    [UseDelegateFromConstructor]
    internal partial Seq<CaptureFeature> Seed();

    // Accessor-backed: the generator fills both rosters from its own static constructor, so an eager field would
    // freeze an EMPTY table.
    public CapabilitySet<CaptureFeature> Roster => Rosters.Value[this].Roster;
    public CapabilitySet<CaptureFeature> Default => Rosters.Value[this].Default;

    private static readonly Lazy<FrozenDictionary<CaptureSurface, (CapabilitySet<CaptureFeature> Roster, CapabilitySet<CaptureFeature> Default)>> Rosters =
        new(static () => Items.ToFrozenDictionary(
            static row => row,
            static row => (
                Roster: CapabilitySet<CaptureFeature>.Of(toSeq(CaptureFeature.Items).Filter(row.Holds).ToArray()),
                Default: CapabilitySet<CaptureFeature>.Of(row.Seed().ToArray()))));

    // The consumer-seam requirement as a VALUE: a held set naming a row this surface does not project refuses at
    // the request's own mint, so the projection fold can never silently skip a toggle a caller asked for. The
    // refusal rides the capability owner's `Require` door, so it names the UNPROJECTED rows, not the whole request.
    internal Fin<CapabilitySet<CaptureFeature>> Admit(Option<CapabilitySet<CaptureFeature>> held, Op key) {
        CapabilitySet<CaptureFeature> requested = held.IfNone(Default);
        return Roster
            .Require(demanded: requested, refuse: missing => new KernelFault.InvalidValue(nameof(CaptureSurface), string.Join(" | ", new object?[] { key, $"capture features this surface projects; unprojected <{missing.Wire}>" })))
            .Map(_ => requested);
    }
}

[ComplexValueObject]
public sealed partial class CaptureDecor {
    public static CaptureDecor Plain { get; } = Create(
        features: CaptureSurface.Settings.Default,
        outputColor: CaptureColor.Display,
        banner: None,
        fidelity: None);

    public CapabilitySet<CaptureFeature> Features { get; }
    public CaptureColor OutputColor { get; }
    public Option<CaptureBanner> Banner { get; }
    public Option<PrintFidelity> Fidelity { get; }

    public static Fin<CaptureDecor> Of(
        Option<CapabilitySet<CaptureFeature>> features = default,
        Option<CaptureColor> outputColor = default,
        Option<CaptureBanner> banner = default,
        Option<PrintFidelity> fidelity = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return CaptureSurface.Settings.Admit(held: features, key: op).Map(held => Create(
            features: held,
            outputColor: outputColor.IfNone(CaptureColor.Display),
            banner: banner,
            fidelity: fidelity));
    }

    internal Fin<Unit> Apply(ViewCaptureSettings settings, Op key) => key.Catch(() => {
        CaptureDecor self = this;
        settings.OutputColor = self.OutputColor.Native;
        _ = CaptureFeature.Apply(target: settings, column: static row => row.Settings, held: self.Features);
        _ = self.Banner.Iter(banner => {
            settings.HeaderText = banner.Header;
            settings.FooterText = banner.Footer;
        });
        _ = self.Fidelity.Iter(row => row.Seat(settings: settings));
        return Fin.Succ(value: unit);
    });
}
```

- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`, `[ValueObject]`, `[ComplexValueObject]`, `[ValidationError]`, `[UseDelegateFromConstructor]`, `IDisallowDefaultValue`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `guard`); UnitsNet (`Length.As(LengthUnit)`, `LengthUnit.DtpPoint`, `Length.Millimeters`); `Rasm/Drawing/sheet` (`SheetSize`, `SheetMargin`, `SheetFrame`, `DrawingScale`, `LineGroup`, `LineWidth`, `Terminator`, `TextHeight`, `PlotResolution`); `Rasm/Domain/validation` (`ICapability`, `CapabilitySet`, `Require`); `Rasm/Domain/rails` (`Op`, `Lease<T>`, `ValidityClaim`, `FaultBand`); `Rasm.Rhino/.api/api-rhinocommon-display.md` (`ViewCaptureSettings`, `ViewCapture`, `ZBufferCapture`); `Rasm.Rhino/.api/api-rhinocommon-document.md` (`AnimationProperties`).
- Growth: a new host toggle is one `CaptureFeature` row declaring the surfaces it writes; a new projection surface is one `CaptureSurface` row and one `Apply` column argument; a new media frame is one `MediaLayout` case.

## [04]-[ARTIFACT_ROWS]

- Owner: `TransparentCaptureSpec` and `DepthCaptureSpec` are the two facade requests this page drives; `DepthProjection` is the depth REQUEST vocabulary and `DepthPayload` the RESULT it produces, one shape per side of the same three questions; `DepthField` joins the payload to the buffer census; `CaptureArtifact` is the one result family, and `RunOutcome` its neutral shell projection.
- Entry: `TransparentCaptureSpec.Of` and `DepthCaptureSpec.Of` admit their address through `TargetReach` and their feature set through `CaptureSurface`; `CaptureArtifact.Raster(mint, extent, coverage, key)` takes custody of a host raster at the moment it is minted.
- Law: the depth PROJECTION and the depth PAYLOAD are two vocabularies over three questions, not one declared twice — `DepthProjection.SamplesCase` carries the pixels a caller asked about and `DepthPayload.SamplesCase` the samples the buffer answered. Each site names which side it is on: one family carries a request column absent from every result beside a result column absent from every request, so the two vocabularies stay two.
- Law: depth configuration precedes projection — `SetDisplayMode` and every `Show*` write invalidate the native grayscale cache, so the depth rail applies mode and channels once, then projects. `MinZ`/`MaxZ`/`ZValueAt` return `float` host precision carried unwidened; `WorldPointAt` is the per-pixel screen-to-world unprojection a single-distance camera read cannot answer; `GrayscaleDib` returns the capture-cached bitmap, which SURVIVES capsule disposal, so the grayscale row is its ONE caller and hands it straight into a lease — a sampling arm reaching it for pixel bounds pays a full grayscale render and leaks that bitmap, so sample bounds read the bound viewport's own `Size` instead.
- Law: a raster leaves under kernel custody with its coverage carriage DECLARED. `RasterCase` carries `Lease<System.Drawing.Bitmap>` and the `AlphaLayout` its request implies — `Straight` where the transparent facade was asked for an alpha background, `Opaque` where the settings rail draws none, because transparency exists only on the instance facade — so a consumer reads pixels through `PixelLease`'s GDI arities against a carriage it was handed rather than one it guessed. The `owned = null` try/finally the prior page spelled at two sites has no successor: the extent is the REQUEST's own, so nothing between acquisition and construction can fail.
- Law: vector egress is exactly the formats the host writes — `ViewCaptureWriter` is not a delivery row, on the catalogued unreachability (`api-rhinocommon-runtime.md` `[ENTRYPOINT_SCOPE]`): its one drive entry is `Draw(nint constPtrPrintInfo, RhinoDoc)`, `ViewCaptureSettings.ConstPointer()` is `internal`, and no public `ViewCapture` member accepts a writer, so a subclass compiles and never receives a frame. The refusal is host-shaped, not permanent, and the catalog row is where a bundle publishing a public frame source surfaces.
- Boundary: DELIVERY is not here. `Exchange/publish` owns the `Landing` union whose raster, vector, and printer arms consume `Captures.Stage`'s prepared batch and mint the matching `CaptureArtifact` cases; this page publishes the sink-free preparation and the artifact vocabulary, so Viewport (S3) never names Exchange (S4) and the forbidden upward edge cannot exist.
- Boundary: `CaptureArtifact.Summary` is the neutral run projection the shell's completion-notice row consumes; the artifact family itself never reaches a notification surface, because every announce operand beyond the outcome — the localized label, the observer that receives the reply, the timeline that stamps it — belongs to the caller, and a scripted or bridge-run capture must reach no notification surface at all.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
public readonly record struct DepthSample(Offset2i Pixel, float Z, Point3d World);

public readonly record struct DepthRange(float MinZ, float MaxZ);

// The RESULT side: what the buffer answered.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DepthPayload {
    private DepthPayload() { }
    public sealed record StatsCase : DepthPayload;
    public sealed record SamplesCase(Seq<DepthSample> Rows) : DepthPayload;
    public sealed record GrayscaleCase(Lease<System.Drawing.Bitmap> Pixels) : DepthPayload;
}

// The REQUEST side: what the caller asked the buffer for.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DepthProjection {
    private DepthProjection() { }

    internal sealed record StatsCase : DepthProjection;
    internal sealed record SamplesCase(Seq<Offset2i> Pixels) : DepthProjection;
    internal sealed record GrayscaleCase : DepthProjection;

    public static DepthProjection Stats { get; } = new StatsCase();
    public static DepthProjection Grayscale { get; } = new GrayscaleCase();

    public static Fin<DepthProjection> Samples(ReadOnlySpan<Offset2i> pixels, Op? key = null) =>
        guard(pixels.Length > 0, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (DepthProjection)new SamplesCase(Pixels: toSeq(pixels.ToArray()).Strict()));

    // `Offset2i` refuses a negative component at construction, so only the upper bound stays live here.
    internal Fin<DepthPayload> Project(ZBufferCapture capture, Size2i extent, Op key) => Switch(
        (Capture: capture, Extent: extent, Op: key),
        statsCase: static (_, _) => Fin.Succ(value: (DepthPayload)new DepthPayload.StatsCase()),
        samplesCase: static (ctx, projection) => projection.Pixels
            .TraverseM(pixel => guard(pixel.X < ctx.Extent.Width && pixel.Y < ctx.Extent.Height, ctx.Op.InvalidInput())
                .ToFin()
                .Bind(_ => ctx.Op.Catch(() => Fin.Succ(new DepthSample(
                    Pixel: pixel,
                    Z: ctx.Capture.ZValueAt(x: pixel.X, y: pixel.Y),
                    World: ctx.Capture.WorldPointAt(x: pixel.X, y: pixel.Y))))))
            .As()
            .Map(static rows => (DepthPayload)new DepthPayload.SamplesCase(Rows: rows.Strict())),
        grayscaleCase: static (ctx, _) => ctx.Op
            .Catch(() => Optional(ctx.Capture.GrayscaleDib()).ToFin(Fail: new ViewportFault.HostRefused(
                Key: ctx.Op, Member: nameof(ZBufferCapture.GrayscaleDib), Detail: "returned no bitmap")))
            .Bind(bitmap => Lease<System.Drawing.Bitmap>.Acquire(mint: () => bitmap, key: ctx.Op))
            .Map(static pixels => (DepthPayload)new DepthPayload.GrayscaleCase(Pixels: pixels)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DepthField(int Hits, Option<DepthRange> Range, DepthPayload Payload);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureArtifact : IDetachedDocumentResult {
    private CaptureArtifact() { }

    public Option<BenchEvidence> Bench { get; init; }

    public sealed record RasterCase(Lease<System.Drawing.Bitmap> Pixels, Size2i Extent, AlphaLayout Coverage) : CaptureArtifact;
    public sealed record VectorCase(System.Xml.XmlDocument Svg) : CaptureArtifact;
    public sealed record PrintedCase(Dimension Pages) : CaptureArtifact;
    public sealed record DepthCase(DepthField Field) : CaptureArtifact;
    public sealed record SequenceCase(SequenceReceipt Receipt) : CaptureArtifact;

    // Custody opens AT the mint and the extent is the request's own, so nothing between acquisition and
    // construction can refuse and strand a host raster.
    internal static Fin<CaptureArtifact> Raster(Func<System.Drawing.Bitmap> mint, Size2i extent, AlphaLayout coverage, Op key) =>
        Lease<System.Drawing.Bitmap>.Acquire(mint: mint, key: key)
            .Map(pixels => (CaptureArtifact)new RasterCase(Pixels: pixels, Extent: extent, Coverage: coverage));

    public RunOutcome Summary(HostText label) => Switch(
        state: label,
        rasterCase: static (text, row) => (RunOutcome)new RunOutcome.Completed(Label: text, Scale: Scale(nameof(RasterCase.Extent), $"{row.Extent.Width}x{row.Extent.Height}")),
        vectorCase: static (text, _) => new RunOutcome.Completed(Label: text, Scale: FrozenDictionary<string, string>.Empty),
        printedCase: static (text, row) => new RunOutcome.Completed(Label: text, Scale: Scale(nameof(PrintedCase.Pages), row.Pages.Value.ToString(CultureInfo.InvariantCulture))),
        depthCase: static (text, row) => new RunOutcome.Completed(Label: text, Scale: Scale(nameof(DepthField.Hits), row.Field.Hits.ToString(CultureInfo.InvariantCulture))),
        sequenceCase: static (text, row) => new RunOutcome.Completed(Label: text, Scale: Scale(nameof(SequenceReceipt), row.Receipt.Echo.HtmlFileName)));

    private static FrozenDictionary<string, string> Scale(string field, string value) =>
        new Dictionary<string, string>(StringComparer.Ordinal) { [field] = value }.ToFrozenDictionary(StringComparer.Ordinal);
}

public sealed record TransparentCaptureSpec(
    ViewportTarget Target,
    Size2i Extent,
    CapabilitySet<CaptureFeature> Features,
    Option<Dimension> RealtimePasses) {

    public static Fin<TransparentCaptureSpec> Of(
        ViewportTarget target,
        Size2i extent,
        Option<CapabilitySet<CaptureFeature>> features = default,
        Option<Dimension> realtimePasses = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from address in TargetReach.View.Admit(target: target, key: op)
               from held in CaptureSurface.Transparent.Admit(held: features, key: op)
               from _passes in guard(realtimePasses.ForAll(static passes => passes.Value >= 1), op.InvalidInput()).ToFin()
               select new TransparentCaptureSpec(
                   Target: address,
                   Extent: extent,
                   Features: held,
                   RealtimePasses: realtimePasses);
    }

    // `ViewCapture` publishes no disposal member, so the facade is a configured VALUE the capture call consumes —
    // the raster it answers is the only thing custody applies to.
    internal ViewCapture Facade() {
        ViewCapture facade = new() { Width = Extent.Width, Height = Extent.Height, TransparentBackground = true };
        _ = CaptureFeature.Apply(target: facade, column: static row => row.Transparent, held: Features);
        _ = RealtimePasses.Iter(passes => facade.RealtimeRenderPasses = passes.Value);
        return facade;
    }
}

public sealed record DepthCaptureSpec(
    ViewportTarget Target,
    Option<ResourceId> Mode,
    CapabilitySet<CaptureFeature> Channels,
    DepthProjection Projection) {

    public static Fin<DepthCaptureSpec> Of(
        ViewportTarget target,
        Option<Guid> mode = default,
        Option<CapabilitySet<CaptureFeature>> channels = default,
        Option<DepthProjection> projection = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from address in TargetReach.Row.Admit(target: target, key: op)
               from held in CaptureSurface.Depth.Admit(held: channels, key: op)
               select new DepthCaptureSpec(
                   Target: address,
                   Mode: mode.Bind(ResourceId.Maybe),
                   Channels: held,
                   Projection: projection.IfNone(DepthProjection.Stats));
    }
}
```

- Packages: `Rasm/Domain/rails` (`Lease<T>`, `Op`); `Rasm/Interaction/paint` (`AlphaLayout`, `PixelLease` GDI arities); `Rasm/Numerics` (`Dimension`); `Rasm.Rhino/HostUi/shell` (`RunOutcome`, `HostText`); `Rasm.Rhino/Modeling/solids` (`BenchEvidence`); BCL `System.Xml` (`XmlDocument`), `System.Drawing` (`Bitmap`).
- Growth: a new artifact modality is one `CaptureArtifact` case with every `Summary` arm loudly broken; a new depth question is one `DepthProjection` case and one `DepthPayload` case landing together.

## [05]-[FRAME_SEQUENCE]

- Owner: `SequenceKind` closes the four motion cases — turntable, dual-track path, single-track flythrough, and a sun study over one `SunWindow`; `SequenceTrack` carries a track as a path-curve id or an admitted point row set, written through the `TrackSlot` setter columns so camera and target share one dispatch; `SunPlace` composes the kernel `SolarSite` with the bearing a `NorthPosture` row answers; `SunWindow` closes the two calendar windows; `SequenceOutput`, `SequenceFidelity`, and `FrameSequenceSpec` carry the output and fidelity axes; `SequenceReceipt` is the host read-back; `SequenceMap` is the generated host transcription.
- Entry: `CaptureRequest.Sequence(SequenceOp, Op?)` enters `Captures.Run`, so sequence custody and sequence evidence share the capture dispatch and return `CaptureArtifact.SequenceCase`.
- Auto: the name-mirrored halves of the host write are GENERATED. `SequenceMap` transcribes `SequenceOutput`, `SequenceFidelity`, and the twelve-column read-back through Mapperly under `RequiredMappingStrategy.Source`, so a new column with no host slot is a build break rather than a silently dropped field. What stays hand-written is what no name correspondence expresses: the `TrackSlot` slot dispatch, and the calendar DECOMPOSITION of one `LocalDate`/`LocalTime` pair into twelve host integer members.
- Law: geodetic site and compass bearing are the kernel's. `SunPlace` carries `SolarSite` — latitude, longitude, NodaTime `Offset` timezone, and elevation, all admitted at their published bands — beside the `NorthPosture` row and the model's own declination, so the bearing DERIVES rather than riding a free `northAngle` double. NAMED GAIN: timezone and elevation become required, which is what exposed that the host animation solves sun angles carrying no zone at all.
- Law: the sun window carries NodaTime shapes because `AnimationProperties`'s Start/End slots are a zone-free WALL CLOCK — `LocalDate`, `LocalTime`, `Duration`, and `Period` are the carriers that spelling names, and a `DateTimeOffset` here attaches an offset the host does not store. NAMED LOSS: `SequenceKind.DaySun` and `Season` stop being separately nameable cases; bought back because the WINDOW case is the name, both arms write the same Start/End slots, and the window's own `CaptureType` column answers which host study runs.
- Law: `RhinoDoc.AnimationProperties` GET mints a detached native copy and SET commits it — in-place mutation without the set-back is inert. Adopt is one copy-edit-commit inside the shared undo bracket: the fresh copy preserves every member the spec leaves unstated, the spec writes land, the property set commits, and the receipt re-reads committed state.
- Law: the spec configures and the host animation tools record — `Images`, `Dates`, and `CurrentFrame` are host-written receipts read back as evidence, never spec inputs. A day study spaces frames by `MinutesBetweenFrames` and a seasonal study by `DaysBetweenFrames`; each window writes only its own spacing member.
- Law: `SequenceOutput` admits extension, animation name, and HTML name as canonical filename components through an ACCUMULATING `Validation`, so a caller with three broken components learns all three; separators, special dot components, platform-invalid characters, and trailing dots or spaces never reach native output metadata.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using NodaTime;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Riok.Mapperly.Abstractions;
using Thinktecture;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
internal sealed partial class TrackSlot {
    internal static readonly TrackSlot Camera = new(
        key: 0,
        curve: static (native, id) => { native.CameraPathId = id; return unit; },
        points: static (native, points) => { native.CameraPoints = points; return unit; });
    internal static readonly TrackSlot Target = new(
        key: 1,
        curve: static (native, id) => { native.TargetPathId = id; return unit; },
        points: static (native, points) => { native.TargetPoints = points; return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Curve(AnimationProperties native, Guid id);

    [UseDelegateFromConstructor]
    internal partial Unit Points(AnimationProperties native, Point3d[] points);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SequenceTrack {
    private SequenceTrack() { }

    internal sealed record CurveCase(ResourceId PathId) : SequenceTrack;
    internal sealed record PointsCase(Seq<Point3d> Rows) : SequenceTrack;

    public static Fin<SequenceTrack> Curve(Guid pathId, Op? key = null) {
        Op op = key.OrDefault();
        return ResourceId.Admit(value: pathId, key: op).Map(static id => (SequenceTrack)new CurveCase(PathId: id));
    }

    public static Fin<SequenceTrack> Points(ReadOnlySpan<Point3d> points, Op? key = null) {
        Seq<Point3d> rows = toSeq(points.ToArray()).Strict();
        return guard(rows.Count >= 2 && rows.ForAll(static point => point.IsValid), key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (SequenceTrack)new PointsCase(Rows: rows));
    }

    internal Unit Seat(AnimationProperties native, TrackSlot slot) => Switch(
        (Native: native, Slot: slot),
        curveCase: static (ctx, track) => ctx.Slot.Curve(native: ctx.Native, id: track.PathId.Value),
        pointsCase: static (ctx, track) => ctx.Slot.Points(native: ctx.Native, points: track.Rows.ToArray()));
}

// Keyed on the host ordinal itself, so `Op.Row` resolves a read-back with no hand switch and no parallel
// `Native` column to drift from the key.
[SmartEnum<int>]
public sealed partial class SequenceMode {
    public static readonly SequenceMode Path = new(key: (int)AnimationProperties.CaptureTypes.Path);
    public static readonly SequenceMode Turntable = new(key: (int)AnimationProperties.CaptureTypes.Turntable);
    public static readonly SequenceMode Flythrough = new(key: (int)AnimationProperties.CaptureTypes.Flythrough);
    public static readonly SequenceMode DaySun = new(key: (int)AnimationProperties.CaptureTypes.DaySunStudy);
    public static readonly SequenceMode Season = new(key: (int)AnimationProperties.CaptureTypes.SeasonalSunStudy);
    public static readonly SequenceMode Unset = new(key: (int)AnimationProperties.CaptureTypes.None);

    internal AnimationProperties.CaptureTypes Native => (AnimationProperties.CaptureTypes)Key;
}

// The four legal corners of the host's method-plus-render-flag product; `(true, true)` is unrepresentable because
// no row spells it, which is the `ReplaceFlags` clause satisfied by row enumeration rather than a corner law.
[SmartEnum<int>]
public sealed partial class SequenceFidelity {
    public static readonly SequenceFidelity Draft = new(key: 0, captureMethod: Preview, renderFull: false, renderPreview: false);
    public static readonly SequenceFidelity Recorded = new(key: 1, captureMethod: Full, renderFull: false, renderPreview: false);
    public static readonly SequenceFidelity RenderedPreview = new(key: 2, captureMethod: Full, renderFull: false, renderPreview: true);
    public static readonly SequenceFidelity Rendered = new(key: 3, captureMethod: Full, renderFull: true, renderPreview: false);

    // The host wire tokens `AnimationProperties.CaptureMethod` reads back.
    internal const string Preview = "preview";
    internal const string Full = "full";

    internal string CaptureMethod { get; }
    internal bool RenderFull { get; }
    internal bool RenderPreview { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
// The declination is `GeoReference.RotationRadians`, handed in by the caller that holds the model — `Rasm.Element`
// is above this boundary's reference set, so the model's own figure crosses as a value.
public sealed record SunPlace(SolarSite Site, NorthPosture Posture, VectorAngle Declination) {
    public static Fin<SunPlace> Of(SolarSite site, NorthPosture posture, Option<VectorAngle> declination = default, Op? key = null) {
        Op op = key.OrDefault();
        return from admittedSite in op.Need(value: site)
               from admittedPosture in op.Need(value: posture)
               from bearing in declination.Match(Some: Fin.Succ, None: () => op.AcceptValidated<VectorAngle>(candidate: 0.0))
               select new SunPlace(Site: admittedSite, Posture: admittedPosture, Declination: bearing);
    }

    // The host slot bears north counter-clockwise off `+X` in DEGREES (Rhino `Sun.North`'s own convention), so the
    // radian bearing the posture row answers projects once, here.
    public VectorAngle North => Posture.Rotation(declination: Declination);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunWindow {
    private SunWindow() { }

    internal sealed record DayCase(LocalDate Date, LocalTime From, LocalTime Until, Duration Step) : SunWindow;
    internal sealed record SeasonCase(LocalDate From, LocalDate Until, Period Step) : SunWindow;

    // The host stores years 1800..2199 in its own integer slots, so the window admits that band once.
    internal const int FirstYear = 1800;
    internal const int LastYear = 2199;

    public static Fin<SunWindow> Day(LocalDate date, LocalTime from, LocalTime until, Duration step, Op? key = null) {
        Op op = key.OrDefault();
        return guard(Admits(date) && from < until && step > Duration.Zero, op.InvalidInput()).ToFin()
            .Map(_ => (SunWindow)new DayCase(Date: date, From: from, Until: until, Step: step));
    }

    public static Fin<SunWindow> Season(LocalDate from, LocalDate until, Period step, Op? key = null) {
        Op op = key.OrDefault();
        return guard(Admits(from) && Admits(until) && from < until && step.Days >= 1, op.InvalidInput()).ToFin()
            .Map(_ => (SunWindow)new SeasonCase(From: from, Until: until, Step: step));
    }

    internal SequenceMode Mode => Switch(
        dayCase: static _ => SequenceMode.DaySun,
        seasonCase: static _ => SequenceMode.Season);

    // The calendar DECOMPOSITION no name correspondence expresses: one date-and-time pair fills twelve host
    // integer slots.
    internal Unit Seat(AnimationProperties native) => Switch(
        native,
        dayCase: static (host, window) => {
            (host.StartYear, host.StartMonth, host.StartDay) = (window.Date.Year, window.Date.Month, window.Date.Day);
            (host.EndYear, host.EndMonth, host.EndDay) = (window.Date.Year, window.Date.Month, window.Date.Day);
            (host.StartHour, host.StartMinutes, host.StartSeconds) = (window.From.Hour, window.From.Minute, window.From.Second);
            (host.EndHour, host.EndMinutes, host.EndSeconds) = (window.Until.Hour, window.Until.Minute, window.Until.Second);
            host.MinutesBetweenFrames = (int)window.Step.TotalMinutes;
            return unit;
        },
        seasonCase: static (host, window) => {
            (host.StartYear, host.StartMonth, host.StartDay) = (window.From.Year, window.From.Month, window.From.Day);
            (host.EndYear, host.EndMonth, host.EndDay) = (window.Until.Year, window.Until.Month, window.Until.Day);
            host.DaysBetweenFrames = window.Step.Days;
            return unit;
        });

    private static bool Admits(LocalDate date) => date.Year is >= FirstYear and <= LastYear;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SequenceKind {
    private SequenceKind() { }

    internal sealed record TurntableCase : SequenceKind;
    internal sealed record PathCase(SequenceTrack Camera, SequenceTrack Focus) : SequenceKind;
    internal sealed record FlythroughCase(SequenceTrack Track) : SequenceKind;
    internal sealed record SunCase(SunPlace Place, SunWindow Window) : SequenceKind;

    public static SequenceKind Turntable { get; } = new TurntableCase();

    public static Fin<SequenceKind> Path(SequenceTrack camera, SequenceTrack focus, Op? key = null) {
        Op op = key.OrDefault();
        return from lens in op.Need(value: camera)
               from aim in op.Need(value: focus)
               select (SequenceKind)new PathCase(Camera: lens, Focus: aim);
    }

    public static Fin<SequenceKind> Flythrough(SequenceTrack track, Op? key = null) =>
        key.OrDefault().Need(value: track)
            .Map(static admitted => (SequenceKind)new FlythroughCase(Track: admitted));

    public static Fin<SequenceKind> Sun(SunPlace place, SunWindow window, Op? key = null) {
        Op op = key.OrDefault();
        return from site in op.Need(value: place)
               from span in op.Need(value: window)
               select (SequenceKind)new SunCase(Place: site, Window: span);
    }

    internal SequenceMode Mode => Switch(
        turntableCase: static _ => SequenceMode.Turntable,
        pathCase: static _ => SequenceMode.Path,
        flythroughCase: static _ => SequenceMode.Flythrough,
        sunCase: static kind => kind.Window.Mode);

    internal Unit Seat(AnimationProperties native) {
        native.CaptureType = Mode.Native;
        return Switch(
            native,
            turntableCase: static (_, _) => unit,
            pathCase: static (host, kind) => {
                _ = kind.Camera.Seat(native: host, slot: TrackSlot.Camera);
                return kind.Focus.Seat(native: host, slot: TrackSlot.Target);
            },
            flythroughCase: static (host, kind) => kind.Track.Seat(native: host, slot: TrackSlot.Camera),
            sunCase: static (host, kind) => {
                _ = SequenceMap.Apply(place: kind.Place, host: host);
                return kind.Window.Seat(native: host);
            });
    }
}

public sealed record SequenceOutput(DocumentPath Folder, string Extension, string Name, Option<string> HtmlFileName) {
    public static Fin<SequenceOutput> Of(
        DocumentPath folder,
        string extension,
        string name,
        Option<string> htmlFileName = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from admittedFolder in op.Need(value: folder)
               from ext in op.AcceptText(value: extension)
               from parts in (
                       Component(value: ext.Trim().TrimStart('.'), op: op).ToValidation(),
                       Component(value: name, op: op).ToValidation(),
                       htmlFileName.Traverse(value => Component(value: value, op: op)).As().ToValidation())
                   .Apply(static (admittedExtension, label, html) => (Extension: admittedExtension, Name: label, Html: html))
                   .As()
                   .ToFin()
               select new SequenceOutput(
                   Folder: admittedFolder,
                   Extension: parts.Extension,
                   Name: parts.Name,
                   HtmlFileName: parts.Html);
    }

    private static Fin<string> Component(string value, Op op) =>
        from admitted in op.AcceptText(value: value)
        let component = admitted.Trim()
        from _clauses in (
                guard(component is not "." and not "..", op.InvalidInput(axis: "dot component")).ToFin().ToValidation(),
                guard(component.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0, op.InvalidInput(axis: "invalid character")).ToFin().ToValidation(),
                guard(component.IndexOfAny(['/', '\\']) < 0, op.InvalidInput(axis: "separator")).ToFin().ToValidation(),
                guard(!component.EndsWith('.') && !component.EndsWith(' '), op.InvalidInput(axis: "trailing dot or space")).ToFin().ToValidation())
            .Apply(static (_, _, _, _) => unit)
            .As()
            .ToFin()
        select component;
}

public sealed record FrameSequenceSpec(
    SequenceKind Kind,
    Dimension Frames,
    ViewportTarget Target,
    Option<ResourceId> Mode,
    Option<SequenceOutput> Output,
    Option<SequenceFidelity> Fidelity) {

    public static Fin<FrameSequenceSpec> Of(
        SequenceKind kind,
        Dimension frames,
        ViewportTarget target,
        Option<Guid> mode = default,
        Option<SequenceOutput> output = default,
        Option<SequenceFidelity> fidelity = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return from motion in op.Need(value: kind)
               from _frames in guard(frames.Value >= 1, op.InvalidInput()).ToFin()
               from address in TargetReach.Row.Admit(target: target, key: op)
               select new FrameSequenceSpec(
                   Kind: motion,
                   Frames: frames,
                   Target: address,
                   Mode: mode.Bind(ResourceId.Maybe),
                   Output: output,
                   Fidelity: fidelity);
    }

    internal Unit Seat(AnimationProperties native, RhinoViewport viewport) {
        _ = Kind.Seat(native: native);
        native.FrameCount = Frames.Value;
        native.ViewportName = viewport.Name;
        _ = Mode.Iter(id => native.DisplayMode = id.Value);
        _ = Output.Iter(rows => SequenceMap.Apply(output: rows, host: native));
        return Fidelity.Iter(row => SequenceMap.Apply(fidelity: row, host: native));
    }
}

// The host's own read-back, mapped column-for-column. The admitted mode and the undo serial are RAIL facts the
// host never wrote, so they ride BESIDE the echo rather than inside a record the generated map would have to
// fabricate them for.
public sealed record SequenceEcho(
    int Frames,
    int CurrentFrame,
    string ViewportName,
    Option<ResourceId> DisplayMode,
    string Folder,
    string Extension,
    string Name,
    string HtmlFileName,
    string HtmlPath,
    Seq<string> Images,
    Seq<string> Dates);

public sealed record SequenceReceipt(
    SequenceMode Mode,
    SequenceEcho Echo,
    // An inspect never opens a bracket, so absence is structural: `0u` is a live host serial's neighbour, not a
    // spelling for "none".
    Option<uint> UndoRecord = default) : IDetachedDocumentResult {

    internal SequenceReceipt Stamp(uint undoRecord) => this with { UndoRecord = Some(undoRecord) };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SequenceOp {
    private SequenceOp() { }

    internal sealed record InspectCase : SequenceOp;
    internal sealed record AdoptCase(FrameSequenceSpec Spec) : SequenceOp;

    public static SequenceOp Inspect { get; } = new InspectCase();

    public static Fin<SequenceOp> Adopt(FrameSequenceSpec spec, Op? key = null) =>
        key.OrDefault().Need(value: spec)
            .Map(static admitted => (SequenceOp)new AdoptCase(Spec: admitted));

    internal Seq<SessionNeed> Needs => Switch(
        inspectCase: static _ => Seq(SessionNeed.Read),
        adoptCase: static _ => SessionNeed.Mutation(custody: UndoCustody.Recorded, redraw: RedrawPolicy.None));

    internal long Scale => Switch(
        inspectCase: static _ => 1L,
        adoptCase: static adopt => adopt.Spec.Frames.Value);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// Source-side completeness makes a new column with no host slot a build break; target completeness stays off
// because the sibling seats fill the host's remaining members.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class SequenceMap {
    [MapProperty(nameof(SequenceOutput.Folder), nameof(AnimationProperties.FolderName))]
    [MapProperty(nameof(SequenceOutput.Extension), nameof(AnimationProperties.FileExtension))]
    [MapProperty(nameof(SequenceOutput.Name), nameof(AnimationProperties.AnimationName))]
    public static partial void Apply(SequenceOutput output, [MappingTarget] AnimationProperties host);

    public static partial void Apply(SequenceFidelity fidelity, [MappingTarget] AnimationProperties host);

    [MapProperty([nameof(SunPlace.Site), nameof(SolarSite.LatitudeDeg)], [nameof(AnimationProperties.Latitude)])]
    [MapProperty([nameof(SunPlace.Site), nameof(SolarSite.LongitudeDeg)], [nameof(AnimationProperties.Longitude)])]
    [MapProperty(nameof(SunPlace.North), nameof(AnimationProperties.NorthAngle))]
    public static partial void Apply(SunPlace place, [MappingTarget] AnimationProperties host);

    [MapProperty(nameof(AnimationProperties.FrameCount), nameof(SequenceEcho.Frames))]
    [MapProperty(nameof(AnimationProperties.FolderName), nameof(SequenceEcho.Folder))]
    [MapProperty(nameof(AnimationProperties.FileExtension), nameof(SequenceEcho.Extension))]
    [MapProperty(nameof(AnimationProperties.AnimationName), nameof(SequenceEcho.Name))]
    [MapProperty(nameof(AnimationProperties.HtmlFullPath), nameof(SequenceEcho.HtmlPath))]
    public static partial SequenceEcho Read(AnimationProperties host);

    [UserMapping]
    private static string Path(DocumentPath folder) => folder.Value;

    [UserMapping]
    private static double Bearing(VectorAngle north) => double.RadiansToDegrees((double)north);

    // The host answers `Guid.Empty` for an unset display mode, which is the spine owner's own absence spelling.
    [UserMapping]
    private static Option<ResourceId> Addressed(Guid value) => ResourceId.Maybe(value);

    [UserMapping]
    private static Seq<string> Rows(string[] values) => toSeq(values).Strict();
}
```

- Packages: NodaTime (`LocalDate`, `LocalTime`, `Duration`, `Period`); Riok.Mapperly (`[Mapper]`, `[MapProperty]`, `[MappingTarget]`, `[UserMapping]`, `RequiredMappingStrategy.Source`); Thinktecture.Runtime.Extensions; LanguageExt.Core (`Validation`, `.Apply`, `Traverse`); `Rasm/Numerics/calculus` (`SolarSite`); `Rasm/Drawing/sheet` (`NorthPosture`); `Rasm/Domain/rails` (`Op`); `Rasm.Rhino/.api/api-rhinocommon-document.md` (`AnimationProperties`).
- Growth: a new motion study is one `SequenceKind` case and one `SequenceMode` row on the host ordinal; a new calendar window is one `SunWindow` case; a new output column is one member with its `[MapProperty]` row.

## [06]-[RUN_RAIL]

- Owner: `CapturePlan` is the sink-free preparation value the whole estate shares; `CaptureRequest` closes the three modalities this page executes and answers its OWN session demand and bench identity; `PrepareGate` and `PreparedCapture` are the prepared-program window; `Captures` is the one run rail.
- Entry: `Captures.Run(DocumentSession, MonotonicTimeline, CaptureRequest, Op?) : Fin<CaptureArtifact>` is the sole public execution rail; `CaptureRequest.Transparent`/`Depth`/`Sequence` admit each modality; internal `Captures.Stage(session, plans, consume, key)` shares settings acquisition with the Exchange landing without exposing `ViewCaptureSettings`.
- Auto: the crossing DERIVES from the request — `Needs` answers the session demand, `Identity` the bench operation and input scale — so `Run` is one marshal, one measurement, and one three-armed dispatch, where the prior page carried four near-identical `*Run` helpers differing only in those two columns.
- Law: preparation is a NEST, not a fold with hand compensation. Each plan's native settings enter under `Lease<ViewCaptureSettings>` and the next plan prepares INSIDE that lease's `Use`, so a refusal partway through unwinds the frames already entered, each releasing its own row through the kernel's ruled aggregation — a cleanup fault ALWAYS appends to the primary and never rides a discard. Reverse release order and partial-batch compensation are consequences of the nesting, so neither a `Rev()` nor a merge fold survives on this page.
- Law: the prepared window is a CLOSED state, not a boolean. `PrepareGate` steps `Live → Released` through `Cell.Step` when the nest unwinds, so a `Use` after release reads a typed `InvalidContext` and a second release is a declined step rather than a repeated teardown.
- Law: preparation applies viewport → area → layout → scale → decoration exactly once, then derives a preview from that completed basis when requested. Viewport binding precedes window projection, aspect matching, and fit scaling, and the bound viewport is the resolved row's own — a page address resolves to `RhinoPageView.MainViewport` and a detail address to `DetailViewObject.Viewport` at the target resolution, so no capture-side re-addressing exists. A settings handle never appears on a public signature.
- Law: bench identity spells the REQUEST factory (`nameof(CaptureRequest.<verb>)`), never the private staging helper that happens to share the verb's name — an unqualified `nameof` binds the helper and re-keys every recorded row the moment that helper is renamed.
- Law: run-rail timing stamps `CaptureArtifact.Bench` on success; failure keeps the original cause, and no second measurement fault exists.
- Boundary: every entry crosses the kernel dispatch on the immediate lane and proves its own `SessionNeed` set inside the same window — `UiThread.Run(new UiDispatch<T>.Blocking(() => session.Demand(…)), DispatchLane.Immediate, key)` — so the crossing asserts the thread and the demand serializes the host call, and neither authority is re-derived at a call site. Target resolution, host work, and release stay inside that scope.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Modeling;
using Thinktecture;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PrepareGate {
    private PrepareGate() { }
    internal sealed record Live : PrepareGate;
    internal sealed record Released : PrepareGate;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record CapturePlan(CaptureSubject Subject, CaptureArea Area, CaptureScale Scale, MediaLayout Layout, CaptureDecor Decor) {
    public static Fin<CapturePlan> Of(
        CaptureSubject subject,
        Option<CaptureArea> area = default,
        Option<CaptureScale> scale = default,
        Option<MediaLayout> layout = default,
        Option<CaptureDecor> decor = default,
        Op? key = null) =>
        key.OrDefault().Need(value: subject).Map(origin => new CapturePlan(
            Subject: origin,
            Area: area.IfNone(CaptureArea.FullView),
            Scale: scale.IfNone(CaptureScale.Native),
            Layout: layout.IfNone(MediaLayout.Default),
            Decor: decor.IfNone(CaptureDecor.Plain)));

    internal Fin<Unit> Seat(ViewportRef row, ViewCaptureSettings settings, Op key) =>
        from _bind in key.Catch(() => settings.SetViewport(viewport: row.Viewport))
        from _area in Area.Apply(settings: settings, key: key)
        from _layout in Layout.Apply(settings: settings, key: key)
        from _scale in Scale.Apply(settings: settings, key: key)
        from _decor in Decor.Apply(settings: settings, key: key)
        from _valid in key.Catch(() => settings.IsValid
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(new ViewportFault.HostRefused(
                Key: key, Member: nameof(ViewCaptureSettings.IsValid), Detail: "settings are invalid")))
        select unit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureRequest {
    private CaptureRequest() { }
    internal sealed record TransparentCase(TransparentCaptureSpec Spec) : CaptureRequest;
    internal sealed record DepthCase(DepthCaptureSpec Spec) : CaptureRequest;
    internal sealed record SequenceCase(SequenceOp Operation) : CaptureRequest;

    public static Fin<CaptureRequest> Transparent(TransparentCaptureSpec spec, Op? key = null) =>
        key.OrDefault().Need(value: spec).Map(static admitted => (CaptureRequest)new TransparentCase(Spec: admitted));

    public static Fin<CaptureRequest> Depth(DepthCaptureSpec spec, Op? key = null) =>
        key.OrDefault().Need(value: spec).Map(static admitted => (CaptureRequest)new DepthCase(Spec: admitted));

    public static Fin<CaptureRequest> Sequence(SequenceOp operation, Op? key = null) =>
        key.OrDefault().Need(value: operation).Map(static admitted => (CaptureRequest)new SequenceCase(Operation: admitted));

    internal Seq<SessionNeed> Needs => Switch(
        transparentCase: static _ => Seq(SessionNeed.Redraw),
        depthCase: static _ => Seq(SessionNeed.Redraw),
        sequenceCase: static row => row.Operation.Needs);

    // A bare `nameof(Depth)` inside the rail class binds whichever private helper shares the verb's name, so the
    // identity spells the REQUEST factory and a helper rename cannot re-key a bench corpus.
    internal (string Operation, long Scale) Identity => Switch(
        transparentCase: static row => (nameof(Transparent), (long)row.Spec.Extent.Width * row.Spec.Extent.Height),
        depthCase: static row => (nameof(Depth), row.Spec.Projection is DepthProjection.SamplesCase samples ? samples.Pixels.Count : 1L),
        sequenceCase: static row => (nameof(Sequence), row.Operation.Scale));
}

// --- [RESOURCES] ----------------------------------------------------------------------------
// Custody of each native setting belongs to the lease that acquired it, never to this value.
internal sealed class PreparedCapture : IDisposable {
    private readonly Seq<ViewCaptureSettings> rows;
    private readonly Atom<PrepareGate> gate = Atom<PrepareGate>(new PrepareGate.Live());
    private readonly Op key;

    private PreparedCapture(Seq<ViewCaptureSettings> rows, Op key) => (this.rows, this.key) = (rows, key);

    internal static Fin<TOut> Bracket<TOut>(Seq<ViewCaptureSettings> rows, Func<PreparedCapture, Fin<TOut>> body, Op key) =>
        Lease<PreparedCapture>.Acquire(mint: () => new PreparedCapture(rows: rows, key: key), key: key)
            .Bind(window => window.Use(body: body, key: key));

    internal Fin<TOut> Use<TOut>(Func<Seq<ViewCaptureSettings>, Fin<TOut>> body, Op key) =>
        from consumer in key.Need(value: body)
        from _live in guard(gate.Value is PrepareGate.Live, key.InvalidContext()).ToFin()
        from output in key.Catch(() => consumer(rows))
        select output;

    public void Dispose() => _ = Cell.Step(
        gate,
        static held => held is PrepareGate.Live ? Some<PrepareGate>(new PrepareGate.Released()) : None,
        key.InvalidContext());
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Captures {
    public static Fin<CaptureArtifact> Run(
        DocumentSession session, MonotonicTimeline timeline, CaptureRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return from owner in Optional(session).ToFin(Fail: op.MissingContext())
               from clock in op.Need(timeline)
               from admitted in op.Need(value: request)
               from artifact in Crossed(
                   session: owner,
                   timeline: clock,
                   needs: admitted.Needs,
                   identity: admitted.Identity,
                   body: document => admitted.Switch(
                       (Document: document, Op: op),
                       transparentCase: static (ctx, row) => Transparent(document: ctx.Document, spec: row.Spec, key: ctx.Op),
                       depthCase: static (ctx, row) => Depth(document: ctx.Document, spec: row.Spec, key: ctx.Op),
                       sequenceCase: static (ctx, row) => Sequenced(document: ctx.Document, request: row.Operation, key: ctx.Op)),
                   key: op)
               select artifact;
    }

    private static Fin<CaptureArtifact> Crossed(
        DocumentSession session,
        MonotonicTimeline timeline,
        Seq<SessionNeed> needs,
        (string Operation, long Scale) identity,
        Func<RhinoDoc, Fin<CaptureArtifact>> body,
        Op key) =>
        UiThread.Run(
            new UiDispatch<CaptureArtifact>.Blocking(() => session.Demand(
                use: document => Measured(timeline: timeline, identity: identity, run: () => body(document), key: key),
                key: key,
                needs: needs.ToArray())),
            DispatchLane.Immediate,
            key);

    private static Fin<CaptureArtifact> Measured(
        MonotonicTimeline timeline,
        (string Operation, long Scale) identity,
        Func<Fin<CaptureArtifact>> run,
        Op key) => BenchBand.Measured(
            timeline: timeline,
            operation: identity.Operation,
            inputScale: identity.Scale,
            run: run)
        .Bind(measured => measured.Outcome.Map(artifact => artifact with { Bench = Some(measured.Evidence) }));

    internal static Fin<TOut> Stage<TOut>(
        DocumentSession session,
        ReadOnlySpan<CapturePlan> plans,
        Func<PreparedCapture, Fin<TOut>> consume,
        Op? key = null) {
        Op op = key.OrDefault();
        Seq<CapturePlan> requested = toSeq(plans.ToArray()).Strict();
        return from owner in Optional(session).ToFin(Fail: op.MissingContext())
               from body in op.Need(value: consume)
               from _rows in guard(!requested.IsEmpty, op.InvalidInput()).ToFin()
               from output in UiThread.Run(
                   new UiDispatch<TOut>.Blocking(() => owner.Demand(
                       use: document => Prepared(document: document, plans: requested, body: body, key: op),
                       key: op,
                       needs: [SessionNeed.Redraw])),
                   DispatchLane.Immediate,
                   op)
               select output;
    }

    // Reversing the roster before the fold is what puts the FIRST plan in the OUTERMOST lease frame, which is why
    // the settings reach the body in request order and release in reverse.
    private static Fin<TOut> Prepared<TOut>(RhinoDoc document, Seq<CapturePlan> plans, Func<PreparedCapture, Fin<TOut>> body, Op key) =>
        plans.Rev().Fold(
            (Func<Seq<ViewCaptureSettings>, Fin<TOut>>)(held => PreparedCapture.Bracket(rows: held, body: body, key: key)),
            (inner, plan) => held => PrepareOne(document: document, plan: plan, key: key)
                .Bind(lease => lease.Use(body: native => inner(held.Add(native)), key: key)))(Seq<ViewCaptureSettings>());

    private static Fin<Lease<ViewCaptureSettings>> PrepareOne(RhinoDoc document, CapturePlan plan, Op key) =>
        from admitted in key.Need(value: plan)
        from row in admitted.Subject.Address.ResolveOne(document: document, key: key)
        from settings in admitted.Subject.Realize(row: row, key: key)
        from _seated in admitted.Seat(row: row, settings: settings.Resource, key: key)
        select settings;

    // The ONE arm that asks for an alpha background, so its coverage carriage is `Straight` by construction.
    private static Fin<CaptureArtifact> Transparent(RhinoDoc document, TransparentCaptureSpec spec, Op key) =>
        from row in spec.Target.ResolveOne(document: document, key: key)
        from facade in key.Catch(() => Fin.Succ(value: spec.Facade()))
        from artifact in CaptureArtifact.Raster(
            mint: () => facade.CaptureToBitmap(sourceView: row.View),
            extent: spec.Extent,
            coverage: AlphaLayout.Straight,
            key: key)
        select artifact;

    private static Fin<CaptureArtifact> Depth(RhinoDoc document, DepthCaptureSpec spec, Op key) =>
        from viewport in spec.Target.ResolveViewport(document: document, key: key)
        from extent in key.Catch(() => Fin.Succ(value: viewport.Size))
            .Bind(size => Size2i.Of(width: size.Width, height: size.Height, key: key))
        from capture in Lease<ZBufferCapture>.Acquire(mint: () => new ZBufferCapture(viewport: viewport), key: key)
        from field in capture.Use(
            body: held => key.Catch(() => Fin.Succ(value: Configured(capture: held, spec: spec)))
                .Bind(_ => spec.Projection.Project(capture: held, extent: extent, key: key))
                .Bind(payload => key.Catch(() => Fin.Succ(value: Field(capture: held, payload: payload)))),
            key: key)
        select (CaptureArtifact)new CaptureArtifact.DepthCase(Field: field);

    private static Unit Configured(ZBufferCapture capture, DepthCaptureSpec spec) {
        _ = spec.Mode.Iter(id => capture.SetDisplayMode(modeId: id.Value));
        return CaptureFeature.Apply(target: capture, column: static row => row.Depth, held: spec.Channels);
    }

    private static DepthField Field(ZBufferCapture capture, DepthPayload payload) {
        int hits = capture.HitCount();
        float min = capture.MinZ();
        float max = capture.MaxZ();
        return new DepthField(
            Hits: hits,
            Range: ValidityClaim.All(hits > 0, float.IsFinite(min), float.IsFinite(max), min <= max)
                ? Some(new DepthRange(MinZ: min, MaxZ: max))
                : None,
            Payload: payload);
    }

    private static Fin<CaptureArtifact> Sequenced(RhinoDoc document, SequenceOp request, Op key) =>
        request.Switch(
            (Document: document, Op: key),
            inspectCase: static (ctx, _) => Read(document: ctx.Document, key: ctx.Op)
                .Map(static receipt => (CaptureArtifact)new CaptureArtifact.SequenceCase(Receipt: receipt)),
            adoptCase: static (ctx, adopt) => DocumentCommit.Sealed(
                document: ctx.Document,
                name: nameof(CaptureRequest.Sequence),
                recordsUndo: true,
                redraw: RedrawPolicy.None,
                run: () =>
                    from viewport in adopt.Spec.Target.ResolveViewport(document: ctx.Document, key: ctx.Op)
                    from native in Lease<AnimationProperties>.Acquire(mint: () => ctx.Document.AnimationProperties, key: ctx.Op)
                    from _commit in native.Use(
                        body: held => ctx.Op.Catch(() => {
                            _ = adopt.Spec.Seat(native: held, viewport: viewport);
                            ctx.Document.AnimationProperties = held;
                            return Fin.Succ(value: unit);
                        }),
                        key: ctx.Op)
                    from receipt in Read(document: ctx.Document, key: ctx.Op)
                    select receipt,
                stamp: static (receipt, serial) => receipt.Stamp(undoRecord: serial),
                project: static receipt => Fin.Succ((CaptureArtifact)new CaptureArtifact.SequenceCase(Receipt: receipt)),
                op: ctx.Op));

    private static Fin<SequenceReceipt> Read(RhinoDoc document, Op key) =>
        Lease<AnimationProperties>.Acquire(mint: () => document.AnimationProperties, key: key)
            .Bind(native => native.Use(
                body: held => from mode in key.Row<AnimationProperties.CaptureTypes, SequenceMode>(
                                  candidate: held.CaptureType, ordinal: static value => (int)value)
                              from echo in key.Catch(() => Fin.Succ(value: SequenceMap.Read(host: held)))
                              select new SequenceReceipt(Mode: mode, Echo: echo),
                key: key));
}
```

- Packages: `Rasm/Interaction/dispatch` (`UiThread.Run`, `UiDispatch<T>.Blocking`, `DispatchLane.Immediate`); `Rasm/Domain/rails` (`Lease<T>`, `Cell.Step`, `Transition<T>`, `Op`); `Rasm/Parametric/projections` (`MonotonicTimeline`); `Rasm.Rhino/Document/session` (`DocumentSession.Demand`, `SessionNeed`, `UndoCustody`); `Rasm.Rhino/Document/commit` (`DocumentCommit.Sealed`, `RedrawPolicy`); `Rasm.Rhino/Document/tables` (`ViewportTarget`, `ViewportRef`); `Rasm.Rhino/Modeling/solids` (`BenchBand.Measured`, `BenchEvidence`); LanguageExt.Core (`Atom`, `Fold`, `guard`).
- Growth: a new capture modality is one `CaptureRequest` case answering its own `Needs` and `Identity`, and one `Run` arm — the marshal, the demand window, and the measurement are untouched.

Question: how does each admitted `Run` request reach one `CaptureArtifact` while every native handle stays leased?

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Capture request dispatch
    accDescr: One capture entry crosses the kernel dispatch under the request's own session demand, brackets the measured body, dispatches the transparent, depth, and sequence arms, and rejoins at one artifact, while the sink-free stage entry hands a prepared settings batch to the Exchange landing that owns delivery.
    Run([Captures.Run]) -->|"UiDispatch Blocking · DispatchLane.Immediate"| Demand[session.Demand — request.Needs]
    Demand -->|"BenchBand.Measured"| Shape{Request case?}
    Shape -->|"transparent"| Alpha[ViewCapture facade — AlphaLayout.Straight]
    Shape -->|"depth"| Depth[ZBufferCapture — configure then project]
    Shape -->|"sequence"| Seq[Inspect or sealed adopt]
    Alpha -->|"returns"| Artifact[/CaptureArtifact/]
    Depth -->|"returns"| Artifact
    Seq -->|"returns"| Artifact
    Stage([Captures.Stage]) -->|"nested Lease Use per plan"| Prepared[PreparedCapture window]
    Prepared -->|"Seq ViewCaptureSettings"| Landing["Exchange/publish Landing — raster · vector · printer"]
    Landing -->|"mints"| Artifact
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
