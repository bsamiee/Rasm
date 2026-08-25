# [RASM_RHINO_DISPLAY_DRAW]

`Marks` owns the package's ONE draw dispatch over four canvases — the live display pipeline, the retained `CustomDisplay` overlay, an interactive Eto surface, and a replayed page — and `DisplayMark` is its one vocabulary: the SCREEN band is the kernel `Rasm.Interaction` mark algebra composed verbatim, the WORLD band is `WorldMark`, the RhinoCommon payloads only a `DisplayPipeline` or `CustomDisplay` can draw, and the SPRITE band is the `DisplayBitmap` blit family only the pipeline's GPU blend path serves. The partition is HOST knowledge: a world mark routed to a surface, a kernel fill routed to the pipeline, and a sprite routed to a retained overlay each land as a typed refusal row on the receipt, never a silent partial draw.

The Eto half of the old two-backend algebra is DELETED, not moved: paths, fills, strokes, glyph blocks, poses, clips, text shaping, the paint program, the resource stock, and hit-testing are `Interaction/paint`'s (`Mark`, `PathSpec`, `FillSource`, `StrokeSpec`, `GlyphBlock`, `PosePlan`, `PaintProgram`, `PaintStock`, `Surface`), and a consumer wanting the retained screen program calls `PaintProgram.Of` directly. What stays here is what RhinoCommon alone can know — the `DisplayPen` projection with its halo, taper, and pattern axes, the `DisplayMaterial` custody bracket, the iso-banding effect, the sprite cache, and the world mark family. `PerceptualColor` remains the only colour source and every host egress composes the kernel `ToDrawing` rail, so an out-of-gamut ink refuses typed instead of clipping.

## [01]-[INDEX]

- [02]-[STYLE]: `StrokeCap`, `StrokeJoin`, `WidthSpace`, `PatternTrait`, `PatternLaw`, `PenDecoration`, `Stroke`, `PenRhythm`, `ShadedFace`, `ShadedMaterial`, `BlendUse`, `BlendPair` — the display-pen and shaded-appearance projections.
- [03]-[ASSETS]: `PathPrimitive`, `SpriteSource`, `ISpriteFiles`, `SpriteRef`, `SpriteSheet`, `PointUse`, `VectorTip`, `PolygonPaint`, `IsoMode`, `IsoGap`, `IsoBanding` — lowered pipeline geometry, native sprite custody, and the world-mark vocabularies.
- [04]-[MARKS]: `WorldMark`, `SpriteAnchor`, `SpriteMark`, `DisplayMark`, `Canvas`, `DrawReceipt`, `Marks` — the one mark union, the four canvases, and the accounted paint dispatch.

## [02]-[STYLE]

- Owner: `Stroke` is the display-pen spec — colour, ladder-free screen-or-world thickness, cap, join, the KERNEL `Dash`, decoration, and pattern policy — and `Mint` is its one `DisplayPen` projection; `PenRhythm` is the pipeline's interval projection of the kernel dash; `ShadedFace`/`ShadedMaterial` bracket the disposable `DisplayMaterial`; `BlendUse` mirrors the host blend roster and `BlendPair` is the source-and-destination pair every sprite blit names once.
- Cases: `WidthSpace` closes the thickness regime at two rows carrying `CoordinateSystem` — the `bool WorldWidth` and the ternary that read it delete; `PatternTrait` is the pattern capability vocabulary (`Autoscale`, `BySegment`, `WorldLength` — every corner legal, law `Open`) and `PatternLaw` carries the set beside the scale and offset the host pattern engine reads; `PenDecoration` owns the halo and taper axes as admitted component records, so an anonymous tuple with hand positivity guards no longer rides the stroke.
- Law: the DASH is the kernel's. `Interaction.Dash` is the one dash vocabulary and `PenRhythm.Table` is its total pipeline projection into width-multiple intervals; `PenRhythm.Admit` states the host bound — `DisplayPen.SetPattern` accepts at most eight entries (`PACKAGE_LIMIT_AS_LAW`), so a longer `PatternedCase` refuses at `Stroke.Of` rather than truncating inside a paint call. The kernel `PatternedCase.Offset` serves the Eto dash alone; the pipeline pattern offset is `PatternLaw`'s own column, and the two never alias.
- Law: this pen is a DISPLAY thickness — screen pixels or world units — and never a paper weight. A plotted line width reads the `Drawing/sheet` `LineWidth` ladder at the publishing surface; a screen hairline is the kernel `StrokeSpec.Hairline` device law; neither aliases this column, and a `Stroke` fed into a plot is the strata violation the publish page refuses.
- Law: shaded appearance is `ShadedMaterial`, never a host `DisplayMaterial` — the native is disposable and carries eight raw screen colours, so it mints inside `Use`, serves exactly the bracketed draw call, and releases on every exit; the second face is `Option`, so a one-sided material spells no back band rather than mirroring the front. Every quantization inside the bracket rides `PerceptualColor.ToDrawing`, so a wide-gamut face refuses typed before the native exists.
- Law: `BlendPair.Over` is the one canonical row — the Porter-Duff source-over pair (`SourceAlpha`, `InverseSourceAlpha`) both boundaries hand-spelled at every blit — and any other composition names its pair explicitly.
- Growth: a pattern axis is one `PatternTrait` row; a decoration axis is one component record on `PenDecoration`; a blend mode is one `BlendUse` row.
- Boundary: no `System.Drawing` or Eto colour becomes domain state; `LineCapStyle`/`LineJoinStyle`/`CoordinateSystem`/`BlendMode` live only as row columns.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using PenLineCap = Eto.Drawing.PenLineCap;
using PenLineJoin = Eto.Drawing.PenLineJoin;
using Rasm.Domain;
using Rasm.Interaction;
using Rasm.Numerics;
using Rhino.Display;
using Rhino.Geometry;
using System.Collections.Frozen;
using Thinktecture;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
// The Eto column is the correspondence the kernel `StrokeSpec` projection reads through `For`; the Rhino column is
// the host consequence — one row carries both directions, so neither backend re-derives a cap mapping.
[SmartEnum<int>]
public sealed partial class StrokeCap {
    public static readonly StrokeCap Butt = new(0, PenLineCap.Butt, LineCapStyle.Flat);
    public static readonly StrokeCap Round = new(1, PenLineCap.Round, LineCapStyle.Round);
    public static readonly StrokeCap Square = new(2, PenLineCap.Square, LineCapStyle.Square);
    internal PenLineCap Eto { get; }
    internal LineCapStyle Rhino { get; }

    internal static StrokeCap For(PenLineCap cap) => ByEto.Value[cap];
    private static readonly Lazy<FrozenDictionary<PenLineCap, StrokeCap>> ByEto =
        new(static () => Items.ToFrozenDictionary(static row => row.Eto, static row => row));
}

[SmartEnum<int>]
public sealed partial class StrokeJoin {
    public static readonly StrokeJoin Round = new(0, PenLineJoin.Round, LineJoinStyle.Round);
    public static readonly StrokeJoin Miter = new(1, PenLineJoin.Miter, LineJoinStyle.Miter);
    public static readonly StrokeJoin Bevel = new(2, PenLineJoin.Bevel, LineJoinStyle.Bevel);
    internal PenLineJoin Eto { get; }
    internal LineJoinStyle Rhino { get; }

    internal static StrokeJoin For(PenLineJoin join) => ByEto.Value[join];
    private static readonly Lazy<FrozenDictionary<PenLineJoin, StrokeJoin>> ByEto =
        new(static () => Items.ToFrozenDictionary(static row => row.Eto, static row => row));
}

// Two rows over the thickness regime: the `bool WorldWidth` and the ternary that read it are the deleted form.
[SmartEnum<int>]
public sealed partial class WidthSpace {
    public static readonly WidthSpace Screen = new(key: 0, native: CoordinateSystem.Screen);
    public static readonly WidthSpace World = new(key: 1, native: CoordinateSystem.World);
    internal CoordinateSystem Native { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PatternTrait : ICapability<PatternTrait> {
    public static readonly PatternTrait Autoscale = new(key: "autoscale");
    public static readonly PatternTrait BySegment = new(key: "by-segment");
    public static readonly PatternTrait WorldLength = new(key: "world-length");
    // Every corner is legal — an autoscaled per-segment world-length pattern is a real host configuration.
    public static CapabilityLaw<PatternTrait> Law => CapabilityLaw<PatternTrait>.Open;
}

[SmartEnum<int>]
public sealed partial class BlendUse {
    public static readonly BlendUse Zero = Row(0, BlendMode.Zero);
    public static readonly BlendUse One = Row(1, BlendMode.One);
    public static readonly BlendUse SourceColor = Row(2, BlendMode.SourceColor);
    public static readonly BlendUse InverseSourceColor = Row(3, BlendMode.OneMinusSourceColor);
    public static readonly BlendUse SourceAlpha = Row(4, BlendMode.SourceAlpha);
    public static readonly BlendUse InverseSourceAlpha = Row(5, BlendMode.OneMinusSourceAlpha);
    public static readonly BlendUse DestinationAlpha = Row(6, BlendMode.DestinationAlpha);
    public static readonly BlendUse InverseDestinationAlpha = Row(7, BlendMode.OneMinusDestinationAlpha);
    public static readonly BlendUse DestinationColor = Row(8, BlendMode.DestinationColor);
    public static readonly BlendUse InverseDestinationColor = Row(9, BlendMode.OneMinusDestinationColor);
    public static readonly BlendUse SaturatedSourceAlpha = Row(10, BlendMode.SourceAlphaSaturate);

    private static BlendUse Row(int key, BlendMode native) => new(key, native);

    internal BlendMode Native { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
// The one canonical pair is the Porter-Duff source-over both boundaries hand-spelled at every blit.
public readonly record struct BlendPair(BlendUse Source, BlendUse Destination) {
    public static readonly BlendPair Over = new(Source: BlendUse.SourceAlpha, Destination: BlendUse.InverseSourceAlpha);
}

[ComplexValueObject]
public sealed partial class PatternLaw {
    public CapabilitySet<PatternTrait> Traits { get; }
    public PositiveMagnitude Scale { get; }
    public float Offset { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilitySet<PatternTrait> traits,
        ref PositiveMagnitude scale,
        ref float offset) =>
        validationError = float.IsFinite(offset)
            ? null
            : new ValidationError(message: "PatternLaw requires a finite pattern offset.");

    // Accessor-backed: the trait roster fills from its own static constructor.
    public static PatternLaw Portable => Seed.Value;
    private static readonly Lazy<PatternLaw> Seed = new(static () =>
        Validate(CapabilitySet<PatternTrait>.Of(), PositiveMagnitude.Create(value: 1d), 0f, out PatternLaw? law) is null
            ? law!
            : throw new InvalidOperationException("PatternLaw.Portable"));
}

[ComplexValueObject]
public sealed partial class PenHalo {
    public PerceptualColor Colour { get; }
    public PositiveMagnitude Width { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PerceptualColor colour, ref PositiveMagnitude width) =>
        validationError = null;
}

[ComplexValueObject]
public sealed partial class PenTaper {
    public PositiveMagnitude Start { get; }
    public PositiveMagnitude End { get; }
    public Point2d At { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PositiveMagnitude start, ref PositiveMagnitude end, ref Point2d at) =>
        validationError = double.IsFinite(at.X) && double.IsFinite(at.Y)
            ? null
            : new ValidationError(message: "PenTaper requires a finite anchor.");
}

// The two decoration axes as ONE admitted owner: the anonymous halo and taper tuples and their hand positivity
// guards delete — component admission is each record's own, and `Bare` is the undecorated spelling.
public sealed record PenDecoration(Option<PenHalo> Halo, Option<PenTaper> Taper) {
    public static readonly PenDecoration Bare = new(Halo: None, Taper: None);
}

public sealed record Stroke {
    private Stroke(
        PerceptualColor colour, PositiveMagnitude width, WidthSpace space, StrokeCap cap, StrokeJoin join,
        Dash dash, PenDecoration decoration, PatternLaw pattern, float miter) =>
        (Colour, Width, Space, Cap, Join, Dash, Decoration, Pattern, Miter) =
        (colour, width, space, cap, join, dash, decoration, pattern, miter);

    public PerceptualColor Colour { get; }
    public PositiveMagnitude Width { get; }
    public WidthSpace Space { get; }
    public StrokeCap Cap { get; }
    public StrokeJoin Join { get; }
    public Dash Dash { get; }
    public PenDecoration Decoration { get; }
    public PatternLaw Pattern { get; }
    public float Miter { get; }

    // Independent admissions accumulate: a bad miter AND an over-long pattern report together.
    public static Fin<Stroke> Of(
        PerceptualColor colour,
        PositiveMagnitude width,
        WidthSpace space,
        StrokeCap cap,
        StrokeJoin join,
        Dash dash,
        Option<PenDecoration> decoration = default,
        Option<PatternLaw> pattern = default,
        float miter = 10f,
        Op? key = null) {
        Op op = key.OrDefault();
        return (
                (float.IsFinite(miter) && miter >= 1f
                    ? Validation<Error, float>.Success(miter)
                    : Validation<Error, float>.Fail(op.InvalidInput(axis: nameof(miter)))),
                PenRhythm.Admit(dash: dash, key: op).ToValidation())
            .Apply((admittedMiter, admittedDash) => new Stroke(
                colour, width, space, cap, join, admittedDash,
                decoration.IfNone(PenDecoration.Bare), pattern.IfNone(PatternLaw.Portable), admittedMiter))
            .As().ToFin();
    }

    // The ONE `DisplayPen` projection; quantization rides the kernel egress, so an out-of-gamut ink refuses here.
    internal Fin<DisplayPen> Mint(Op key) =>
        from ink in Colour.ToDrawing(key: key)
        from halo in Decoration.Halo.Traverse(row => row.Colour.ToDrawing(key: key).Map(quantized => (Row: row, Ink: quantized))).As()
        select Seat(ink: ink, halo: halo);

    private DisplayPen Seat(System.Drawing.Color ink, Option<(PenHalo Row, System.Drawing.Color Ink)> halo) {
        DisplayPen pen = new() {
            Color = ink,
            Thickness = (float)Width.Value,
            ThicknessSpace = Space.Native,
            CapStyle = Cap.Rhino,
            JoinStyle = Join.Rhino,
        };
        _ = halo.Iter(row => (pen.HaloColor, pen.HaloThickness) = (row.Ink, (float)row.Row.Width.Value));
        _ = Decoration.Taper.Iter(row => pen.SetTaper((float)row.Start.Value, (float)row.End.Value, new Point2f((float)row.At.X, (float)row.At.Y)));
        Seq<float> gaps = PenRhythm.Table(dash: Dash);
        _ = Op.SideWhen(!gaps.IsEmpty, () => {
            pen.SetPattern(gaps.Map(gap => gap * (float)Width.Value).AsEnumerable());
            (pen.PatternAutoscale, pen.PatternScale, pen.PatternOffset, pen.PatternBySegment, pen.PatternLengthInWorldUnits) = (
                Pattern.Traits.Admits(PatternTrait.Autoscale),
                (float)Pattern.Scale.Value,
                Pattern.Offset,
                Pattern.Traits.Admits(PatternTrait.BySegment),
                Pattern.Traits.Admits(PatternTrait.WorldLength));
        });
        return pen;
    }

    internal double CullOutset {
        get {
            double stroke = Width.Value * (Join == StrokeJoin.Miter ? Miter : 1f) / 2d;
            return Decoration.Halo.Map(row => double.Max(stroke, row.Width.Value / 2d)).IfNone(stroke);
        }
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The pipeline projection of the KERNEL dash: one total interval table in width multiples, and one host bound —
// `DisplayPen.SetPattern` takes at most eight entries (PACKAGE_LIMIT_AS_LAW), so admission refuses what the paint
// call would otherwise throw on. The kernel `PatternedCase.Offset` is the Eto dash's and never read here.
internal static class PenRhythm {
    private const int MaxIntervals = 8;

    internal static Fin<Dash> Admit(Dash dash, Op key) => dash is Dash.PatternedCase row
        ? !row.Intervals.IsEmpty
            && row.Intervals.Count <= MaxIntervals
            && row.Intervals.ForAll(static gap => float.IsFinite(gap) && gap > 0f)
            ? Fin.Succ(dash)
            : Fin.Fail<Dash>(key.InvalidInput(axis: nameof(Dash.PatternedCase.Intervals)))
        : Fin.Succ(dash);

    internal static Seq<float> Table(Dash dash) => dash.Switch(
        solidCase: static _ => Seq<float>(),
        dashedCase: static _ => [3f, 1f],
        dottedCase: static _ => [1f, 1f],
        dashDotCase: static _ => [3f, 1f, 1f, 1f],
        dashDotDotCase: static _ => [3f, 1f, 1f, 1f, 1f, 1f],
        patternedCase: static row => row.Intervals);
}

// --- [MODELS] -------------------------------------------------------------------------------
// `DisplayMaterial` is a disposable host native carrying eight raw `System.Drawing.Color` channels, so it is the one
// shaded appearance the slice cannot publish: `ShadedFace` states one side in `PerceptualColor` under `ShineAxis.Unit`,
// `ShadedMaterial` pairs the sides, and the native mints and dies inside `Use`.
[ComplexValueObject]
public sealed partial class ShadedFace {
    public PerceptualColor Diffuse { get; }
    public PerceptualColor Specular { get; }
    public PerceptualColor Ambient { get; }
    public PerceptualColor Emission { get; }
    public double Shine { get; }
    public double Transparency { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref PerceptualColor diffuse,
        ref PerceptualColor specular,
        ref PerceptualColor ambient,
        ref PerceptualColor emission,
        ref double shine,
        ref double transparency) =>
        // `ShineAxis` on the modes page owns both host regimes; `Unit` is the `DisplayMaterial` one, where shine and
        // transparency are BOTH unit-interval, unlike the attribute editor's `Editor` regime this face never enters.
        validationError = ShineAxis.Unit.Shine(shine) && ShineAxis.Unit.Transparency(transparency)
            ? null
            : new ValidationError(message: "Shaded face shine or transparency leaves the unit interval.");

    // The four channels quantize as one accumulating pass, so a two-channel gamut breach reports both.
    internal Validation<Error, (System.Drawing.Color Diffuse, System.Drawing.Color Specular, System.Drawing.Color Ambient, System.Drawing.Color Emission)> Quantized(Op key) =>
        (Diffuse.ToDrawing(key: key).ToValidation(),
         Specular.ToDrawing(key: key).ToValidation(),
         Ambient.ToDrawing(key: key).ToValidation(),
         Emission.ToDrawing(key: key).ToValidation())
        .Apply(static (diffuse, specular, ambient, emission) => (diffuse, specular, ambient, emission)).As();
}

public sealed record ShadedMaterial(ShadedFace Front, Option<ShadedFace> Back) {
    internal Fin<TResult> Use<TResult>(Func<DisplayMaterial, Fin<TResult>> project, Op key) =>
        from front in Front.Quantized(key: key).ToFin()
        from back in Back.Traverse(face => face.Quantized(key: key).ToFin().Map(channels => (Face: face, Channels: channels))).As()
        from projected in key.Catch(() => {
            using DisplayMaterial native = new(
                diffuse: front.Diffuse, specular: front.Specular, ambient: front.Ambient, emission: front.Emission,
                shine: Front.Shine, transparency: Front.Transparency);
            _ = back.Iter(row => {
                native.IsTwoSided = true;
                (native.BackDiffuse, native.BackSpecular) = (row.Channels.Diffuse, row.Channels.Specular);
                (native.BackAmbient, native.BackEmission) = (row.Channels.Ambient, row.Channels.Emission);
                (native.BackShine, native.BackTransparency) = (row.Face.Shine, row.Face.Transparency);
            });
            return project(native);
        })
        select projected;
}
```

## [03]-[ASSETS]

- Owner: `PathPrimitive` is the pipeline's lowered screen geometry — four curve-mintable cases — and `Lower` is the ONE projection from the kernel `PathSpec` onto it; `SpriteRef` admits sprite bytes under a content-hash identity and `SpriteSheet` owns the `DisplayBitmap` cache; `PointUse`, `VectorTip`, and `PolygonPaint` are world-mark vocabularies; `IsoBanding` owns banded-shading data with `IsoGap` closing its gap tri-state.
- Law: the composite lowering is the KERNEL's twice over — the authored figure is `Interaction.PathSpec` and the corner walk, radius clamp, and cardinal-spline arithmetic are `Rasm.Parametric`'s (`ParametricOp.RoundedRectangle`, `ParametricOp.CardinalSpline`), so this page carries no curve arithmetic a NURBS emission could disagree with. Frame-local coordinates ARE screen coordinates (`Plane.WorldXY` over the rectangle's own intervals), and angles stay RADIANS end to end — the kernel answers radians and `ArcCurve` consumes them, so the old degree round-trip is deleted whole.
- Law: an elliptical `ArcCase` refuses on the pipeline — `PathPrimitive.Arc` is circular because the pipeline curve mint is — and the refusal names the corner; a non-circular arc rides `CurveCase` or `EllipseCase` instead. The role-resolved OS faces, fills, glyph blocks, panes, clips, and poses refuse the same way at the mark projection: each is an Eto-surface capability the kernel replays and the pipeline cannot.
- Law: sprite bytes admitted by `ISpriteFiles` enter once through `SpriteRef.Of` under `ContentHash.Hex` — the kernel's one lowercase identity text — so two sources with one payload share one cache row and no raw path crosses the asset boundary.
- Law: the sheet's lifecycle is a `Cell`-stepped state machine, never a monitor barrier — `Use` enters through a guarded step that declines once draining begins, a leave decrements, and the last borrower leaving a draining sheet performs all-attempted disposal. `Release(Op)` carries immediate cleanup faults; `Dispose` is only the host-required adapter.
- Law: `PointUse` carries one row per DISTINCT host marker — `PointStyle` is an aliased enum (`Circle`≡`RoundSimple`, `Square`≡`Simple`, `SolidSquare`≡`VariableDot`, `RoundDot`≡`SolidRound`≡`SolidCircle`), so a row per alias would seat values no host call can tell apart.
- Cases: `VectorTip` closes the vector-anchor axis (`Plain`, `Anchored`) — the `bool AnchorPoint` deletes; `PolygonPaint` closes the fill-and-edge product at its three LEGAL corners (`Filled`, `Edged`, `Full`) — the `(false, false)` corner that drew nothing is unrepresentable; `IsoGap` closes the gap tri-state (`Painted`, `Discarded`) under an `Option` — the tuple's `bool Discard` deletes.
- Boundary: cache disposal closes admission, drains every draw-scoped use, releases each native bitmap once, and clears the table.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// RADIANS end to end: the kernel Parametric lowering answers radians and `ArcCurve` consumes them, so the old
// degree round-trip through `ToDegrees`/`ToRadians` is deleted whole and `VectorAngle` carries the admission.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathPrimitive {
    private PathPrimitive() { }
    public sealed record Line(Point2d From, Point2d To) : PathPrimitive;
    public sealed record Arc(Point2d Center, double Radius, VectorAngle Start, VectorAngle Sweep) : PathPrimitive;
    public sealed record Bezier(Point2d Start, Point2d Control1, Point2d Control2, Point2d End) : PathPrimitive;
    public sealed record Ellipse(Point2d Centre, double RadiusX, double RadiusY) : PathPrimitive;

    // TOTAL: every case mints a pipeline curve — the unreachable composite arms of the old seven-case union are the
    // deleted form, because lowering happens before this family exists.
    internal Curve Mint() => Switch(
        line: static row => (Curve)new LineCurve(new Point3d(row.From.X, row.From.Y, 0.0), new Point3d(row.To.X, row.To.Y, 0.0)),
        arc: static row => new ArcCurve(new global::Rhino.Geometry.Arc(
            new Plane(
                new Point3d(row.Center.X, row.Center.Y, 0.0),
                new Vector3d(Math.Cos(row.Start.Value), Math.Sin(row.Start.Value), 0.0),
                new Vector3d(-Math.Sin(row.Start.Value), Math.Cos(row.Start.Value), 0.0)),
            row.Radius,
            row.Sweep.Value)),
        bezier: static row => new BezierCurve([
            new(row.Start.X, row.Start.Y, 0.0), new(row.Control1.X, row.Control1.Y, 0.0),
            new(row.Control2.X, row.Control2.Y, 0.0), new(row.End.X, row.End.Y, 0.0)]).ToNurbsCurve(),
        ellipse: static row => new global::Rhino.Geometry.Ellipse(
            new Plane(new Point3d(row.Centre.X, row.Centre.Y, 0.0), Vector3d.ZAxis),
            row.RadiusX,
            row.RadiusY).ToNurbsCurve());

    // The ONE projection from the kernel screen vocabulary onto pipeline geometry. Composite figures lower through
    // `Rasm.Parametric` — the same fold the kernel exemption seats at this boundary — and a primitive lowers to
    // itself, so only the composite arms and the elliptical-arc corner can refuse.
    internal static Fin<Seq<PathPrimitive>> Lower(PathSpec spec, Op key) => spec.Switch(
        state: key,
        lineCase: static (_, row) => Fin.Succ(Seq<PathPrimitive>(new Line(P(row.From), P(row.To)))),
        polylineCase: static (_, row) => Fin.Succ(Chained(row.Points, closed: false)),
        polygonCase: static (_, row) => Fin.Succ(Chained(row.Points, closed: true)),
        rectCase: static (_, row) => Fin.Succ(Edges(row.Frame)),
        roundRectCase: static (op, row) => Outlined(new ParametricOp.RoundedRectangle(
            Frame: Plane.WorldXY,
            X: new Interval(row.Frame.X, row.Frame.X + row.Frame.Width),
            Y: new Interval(row.Frame.Y, row.Frame.Y + row.Frame.Height),
            NW: row.NW, NE: row.NE, SE: row.SE, SW: row.SW), op),
        ellipseCase: static (_, row) => Fin.Succ(Seq<PathPrimitive>(new Ellipse(
            new Point2d(row.Frame.X + (row.Frame.Width / 2.0), row.Frame.Y + (row.Frame.Height / 2.0)),
            row.Frame.Width / 2.0,
            row.Frame.Height / 2.0))),
        // Circular alone: the pipeline arc mint is circular, so a non-square frame refuses the corner by name and an
        // elliptical arc rides `CurveCase` or `EllipseCase` instead of drawing a circle nobody asked for.
        arcCase: static (op, row) => Math.Abs(row.Frame.Width - row.Frame.Height) <= float.Epsilon
            ? Fin.Succ(Seq<PathPrimitive>(new Arc(
                new Point2d(row.Frame.X + (row.Frame.Width / 2.0), row.Frame.Y + (row.Frame.Height / 2.0)),
                row.Frame.Width / 2.0,
                row.Start,
                row.Sweep)))
            : Fin.Fail<Seq<PathPrimitive>>(op.Unsupported(typeof(PathSpec.ArcCase), typeof(DisplayPipeline))),
        bezierCase: static (_, row) => Fin.Succ(Seq<PathPrimitive>(new Bezier(P(row.From), P(row.ControlA), P(row.ControlB), P(row.To)))),
        curveCase: static (op, row) => Outlined(new ParametricOp.CardinalSpline(
            Frame: Plane.WorldXY,
            Points: new Arr<Point2d>([.. toSeq(row.Points).Map(P)]),
            Tension: (float)row.Tension.Value,
            Closed: false), op),
        compositeCase: static (op, row) => row.Figures.TraverseM(figure => Lower(spec: figure, key: op)).As()
            .Map(static lowered => lowered.Bind(static run => run).Strict()));

    private static Point2d P(Eto.Drawing.PointF at) => new(at.X, at.Y);

    private static Seq<PathPrimitive> Chained(Seq<Eto.Drawing.PointF> points, bool closed) {
        Seq<Point2d> run = toSeq(points).Map(P).Strict();
        Seq<PathPrimitive> lines = run.Zip(run.Skip(1)).Map(static pair => (PathPrimitive)new Line(pair.Item1, pair.Item2)).Strict();
        return closed && run.Count >= 3 ? lines.Add(new Line(run.Last, run.Head)) : lines;
    }

    private static Seq<PathPrimitive> Edges(Eto.Drawing.RectangleF frame) => Seq<PathPrimitive>(
        new Line(new Point2d(frame.X, frame.Y), new Point2d(frame.X + frame.Width, frame.Y)),
        new Line(new Point2d(frame.X + frame.Width, frame.Y), new Point2d(frame.X + frame.Width, frame.Y + frame.Height)),
        new Line(new Point2d(frame.X + frame.Width, frame.Y + frame.Height), new Point2d(frame.X, frame.Y + frame.Height)),
        new Line(new Point2d(frame.X, frame.Y + frame.Height), new Point2d(frame.X, frame.Y)));

    // The one seam: the kernel answers frame-local primitives in radians over `Plane.WorldXY`, so the mapping is
    // coordinates 1:1 with no angle conversion; an `Outline` is the only result these ops produce.
    private static Fin<Seq<PathPrimitive>> Outlined(ParametricOp op, Op key) =>
        Parametric.Apply(op, key).Bind(result => result is ParametricResult.Outline outline
            ? Fin.Succ(toSeq(outline.Run).Map(Planar).Strict())
            : Fin.Fail<Seq<PathPrimitive>>(key.InvalidResult(detail: result.GetType().Name)));

    private static PathPrimitive Planar(PlanarPrimitive primitive) => primitive.Switch(
        segment: static row => (PathPrimitive)new Line(row.From, row.To),
        sweep: static row => new Arc(
            Center: row.Center,
            Radius: row.Radius,
            Start: VectorAngle.Create(value: row.Start),
            Sweep: VectorAngle.Create(value: row.Angle)),
        cubic: static row => new Bezier(row.Start, row.Control1, row.Control2, row.End));
}

// Host truth: `PointStyle` is an ALIASED enum — `Circle` and `RoundSimple` are both 4, `Square` and `Simple` both 0,
// `SolidSquare` and `VariableDot` both 50, and `RoundDot`, `SolidRound`, and `SolidCircle` are all 51 — so the roster
// carries one row per DISTINCT marker, named for the enum's own primary spelling.
[SmartEnum<int>]
public sealed partial class PointUse {
    public static readonly PointUse Simple = Row(0, PointStyle.Simple);
    public static readonly PointUse Control = Row(1, PointStyle.ControlPoint);
    public static readonly PointUse Active = Row(2, PointStyle.ActivePoint);
    public static readonly PointUse Cross = Row(3, PointStyle.X);
    public static readonly PointUse RoundSimple = Row(4, PointStyle.RoundSimple);
    public static readonly PointUse RoundControl = Row(5, PointStyle.RoundControlPoint);
    public static readonly PointUse RoundActive = Row(6, PointStyle.RoundActivePoint);
    public static readonly PointUse Triangle = Row(7, PointStyle.Triangle);
    public static readonly PointUse Heart = Row(8, PointStyle.Heart);
    public static readonly PointUse Chevron = Row(9, PointStyle.Chevron);
    public static readonly PointUse Clover = Row(10, PointStyle.Clover);
    public static readonly PointUse Tag = Row(11, PointStyle.Tag);
    public static readonly PointUse Asterisk = Row(12, PointStyle.Asterisk);
    public static readonly PointUse Pin = Row(13, PointStyle.Pin);
    public static readonly PointUse ArrowTail = Row(14, PointStyle.ArrowTail);
    public static readonly PointUse ArrowTip = Row(15, PointStyle.ArrowTip);
    public static readonly PointUse VariableDot = Row(16, PointStyle.VariableDot);
    public static readonly PointUse RoundDot = Row(17, PointStyle.RoundDot);
    public static readonly PointUse None = Row(18, PointStyle.None);

    private static PointUse Row(int key, PointStyle native) => new(key, native);

    internal PointStyle Native { get; }
}

// The vector-anchor axis as rows: `Anchored` draws the anchor dot beside the arrow, `Plain` does not, and the host
// bool is the row's own projection column.
[SmartEnum<int>]
public sealed partial class VectorTip {
    public static readonly VectorTip Plain = new(key: 0, dot: false);
    public static readonly VectorTip Anchored = new(key: 1, dot: true);
    internal bool Dot { get; }
}

// Three LEGAL corners of the fill-and-edge product; the `(false, false)` corner that drew nothing is unrepresentable.
[SmartEnum<int>]
public sealed partial class PolygonPaint {
    public static readonly PolygonPaint Filled = new(key: 0, fill: true, edge: false);
    public static readonly PolygonPaint Edged = new(key: 1, fill: false, edge: true);
    public static readonly PolygonPaint Full = new(key: 2, fill: true, edge: true);
    internal bool Fill { get; }
    internal bool Edge { get; }
}

[SmartEnum<int>]
public sealed partial class IsoMode {
    public static readonly IsoMode Off = Row(0, IsoDrawMode.None);
    public static readonly IsoMode Directional = Row(1, IsoDrawMode.DirectionalLight);
    public static readonly IsoMode DirectionalXY = Row(2, IsoDrawMode.DirectionalLightXY);
    public static readonly IsoMode DirectionalXYDots = Row(3, IsoDrawMode.DirectionalLightXYDots);
    public static readonly IsoMode CameraX = Row(4, IsoDrawMode.DirectionalLightCameraX);
    public static readonly IsoMode CameraY = Row(5, IsoDrawMode.DirectionalLightCameraY);
    public static readonly IsoMode CameraXY = Row(6, IsoDrawMode.DirectionalLightCameraXY);
    public static readonly IsoMode CameraXYDots = Row(7, IsoDrawMode.DirectionalLightCameraXYDots);
    public static readonly IsoMode CameraZ = Row(8, IsoDrawMode.DirectionalLightCameraZ);
    public static readonly IsoMode Point = Row(9, IsoDrawMode.PointLight);
    public static readonly IsoMode PointCamera = Row(10, IsoDrawMode.PointLightCamera);
    public static readonly IsoMode Cylindrical = Row(11, IsoDrawMode.CylindricalStatic);
    public static readonly IsoMode Distance = Row(12, IsoDrawMode.DirectionalDistance);
    public static readonly IsoMode DistanceCamera = Row(13, IsoDrawMode.DirectionalDistanceCamera);

    private static IsoMode Row(int key, IsoDrawMode native) => new(key, native);

    internal IsoDrawMode Native { get; }
}

// The gap tri-state as a closed family under an `Option`: absence is no gap, `Painted` draws it in an admitted
// colour, `Discarded` cuts it — the tuple's `bool Discard` deletes.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IsoGap {
    private IsoGap() { }
    public sealed record Painted(PerceptualColor Colour, double Size) : IsoGap;
    public sealed record Discarded(double Size) : IsoGap;

    internal bool Valid => Switch(
        painted: static row => double.IsFinite(row.Size) && row.Size >= 0.0,
        discarded: static row => double.IsFinite(row.Size) && row.Size >= 0.0);
}

public interface ISpriteFiles {
    Fin<ReadOnlyMemory<byte>> Read(string asset, Op key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpriteSource {
    private SpriteSource() { }
    public sealed record Bytes(ReadOnlyMemory<byte> Value) : SpriteSource;
    public sealed record File(ISpriteFiles Files, string Asset) : SpriteSource;

    internal bool Valid => Switch(
        bytes: static row => !row.Value.IsEmpty,
        file: static row => row.Files is not null && !string.IsNullOrWhiteSpace(row.Asset));

    internal Fin<ReadOnlyMemory<byte>> Read(Op key) => Switch(
        key,
        bytes: static (_, row) => Fin.Succ(row.Value),
        file: static (op, row) => op.Catch(() => row.Files.Read(row.Asset, op)));
}

public sealed record SpriteRef {
    private SpriteRef(string key, ReadOnlyMemory<byte> content) => (Key, Content) = (key, content);
    public string Key { get; }
    internal ReadOnlyMemory<byte> Content { get; }

    // `ContentHash.Hex` is the kernel's ONE identity text — lowercase, admission-refusing-uppercase — so the sprite
    // key round-trips through every identity seam the branch owns; a hand `X32` format forked the alphabet.
    public static Fin<SpriteRef> Of(SpriteSource source, Op? key = null) {
        Op op = key.OrDefault();
        return guard(source is not null && source.Valid, op.InvalidInput()).ToFin()
            .Bind(_ => source.Read(op))
            .Bind(content => {
                ReadOnlyMemory<byte> owned = content.ToArray();
                return guard(!owned.IsEmpty, op.InvalidInput()).ToFin()
                    .Map(_ => new SpriteRef(ContentHash.Hex(ContentHash.Of(owned.Span)), owned));
            });
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record IsoBanding(
    IsoMode Mode,
    Vector3d Direction,
    Point3d Anchor,
    int Frequency,
    double Rotation,
    double Falloff,
    Option<IsoGap> Gap,
    PerceptualColor From,
    PerceptualColor To,
    Rasm.Numerics.Dimension Bands) {
    internal bool Valid => Mode is not null
        && Direction.IsValid
        && Anchor.IsValid
        && Frequency > 0
        && double.IsFinite(Rotation)
        && double.IsFinite(Falloff)
        && Bands.Value is > 0 and <= 10
        && Gap.Match(Some: static row => row.Valid, None: static () => true);

    internal Fin<IsoDrawEffect> Mint(Op key) =>
        from _ in guard(Valid, key.InvalidInput()).ToFin()
        from ramp in From.Ramp(To, Bands).Traverse(colour => colour.ToDrawing(key: key)).As()
        from gap in Gap.Traverse(row => row switch {
            IsoGap.Painted painted => painted.Colour.ToDrawing(key: key).Map(ink => (Ink: Some(ink), painted.Size, Discard: false)),
            IsoGap.Discarded cut => Fin.Succ((Ink: Option<System.Drawing.Color>.None, cut.Size, Discard: true)),
            _ => Fin.Fail<(Option<System.Drawing.Color>, double, bool)>(key.InvalidInput()),
        }).As()
        from effect in key.Catch(() => {
            IsoDrawEffect value = new() { DrawMode = Mode.Native, Direction = Direction, Point = Anchor, Frequency = Frequency, RotationRadians = Rotation, Falloff = Falloff, UsedBandColorCount = Bands.Value };
            _ = gap.Iter(row => {
                _ = row.Ink.Iter(ink => value.GapColor = ink);
                (value.GapSize, value.DiscardGap) = (row.Size, row.Discard);
            });
            _ = ramp.Map(static (ink, index) => (ink, index)).Iter(row => ignore(value.SetBandColor(row.index, row.ink)));
            return Fin.Succ(value);
        })
        select effect;
}

// --- [SERVICES] -----------------------------------------------------------------------------
// The lifecycle is a stepped state machine: `Use` enters through a guarded step that DECLINES once draining begins,
// the leave decrements, and the LAST borrower out of a draining sheet performs the native disposal — so `Dispose`
// under live borrowers defers instead of blocking on a monitor pulse, and a second `Dispose` reads `Refused`. The
// cache is the operation owner's own mutable registry and stays with it.
public sealed class SpriteSheet : IDisposable {
    private enum SheetPhase { Open, Draining, Released }
    private sealed record SheetGate(int Active, SheetPhase Phase);

    private readonly System.Collections.Concurrent.ConcurrentDictionary<(string Key, BlendMode Src, BlendMode Dst), Lazy<Fin<DisplayBitmap>>> cache = new();
    private readonly Atom<SheetGate> gate = Atom(new SheetGate(Active: 0, Phase: SheetPhase.Open));

    internal Fin<TResult> Use<TResult>(
        SpriteRef sprite,
        BlendPair blend,
        Func<DisplayBitmap, Fin<TResult>> use,
        Op key) =>
        Cell.Step(gate, static held => held.Phase is SheetPhase.Open ? Some(held with { Active = held.Active + 1 }) : None, key.InvalidContext())
            .Switch(
                state: (Self: this, Sprite: sprite, Blend: blend, Use: use, Key: key),
                committed: static (ctx, _) => ctx.Self.Borrowed(ctx.Sprite, ctx.Blend, ctx.Use, ctx.Key),
                ceded: static (ctx, _) => Fin.Fail<TResult>(ctx.Key.InvalidContext()),
                refused: static (_, row) => Fin.Fail<TResult>(row.Cause),
                contended: static (ctx, _) => Fin.Fail<TResult>(ctx.Key.InvalidResult()));

    private Fin<TResult> Borrowed<TResult>(SpriteRef sprite, BlendPair blend, Func<DisplayBitmap, Fin<TResult>> use, Op key) {
        Fin<TResult> primary = key.Catch(() => {
            Lazy<Fin<DisplayBitmap>> cached = cache.GetOrAdd(
                (sprite.Key, blend.Source.Native, blend.Destination.Native),
                _ => new Lazy<Fin<DisplayBitmap>>(
                    () => Load(sprite: sprite, blend: blend, key: key),
                    LazyThreadSafetyMode.ExecutionAndPublication));
            return cached.Value.Match(
                Succ: bitmap => key.Catch(() => use(bitmap)),
                Fail: failure => (
                    cache.TryRemove(new KeyValuePair<(string, BlendMode, BlendMode), Lazy<Fin<DisplayBitmap>>>((sprite.Key, blend.Source.Native, blend.Destination.Native), cached)),
                    Fin.Fail<TResult>(failure)).Item2);
        });
        return primary.Settled(held: Seq(unit), release: _ => Leave(key), key: key);
    }

    // Last borrower out of a draining sheet disposes on the same rail, so a drain fault cannot replace or vanish
    // beside the use outcome.
    private Fin<Unit> Leave(Op key) => key.Catch(() => {
        Transition<SheetGate> left = Cell.Commit(gate, static held => held with { Active = held.Active - 1 });
        return left.Current is { Active: 0, Phase: SheetPhase.Draining } ? Drain(key) : Fin.Succ(unit);
    });

    private Fin<Unit> Drain(Op key) =>
        Cell.Step(gate, static held => held.Phase is SheetPhase.Draining ? Some(held with { Phase = SheetPhase.Released }) : None, Errors.None)
            .Switch(
                state: (Self: this, Key: key),
                committed: static (context, _) => Custody.Release(
                    held: toSeq(context.Self.cache.Values)
                        .Filter(static bitmap => bitmap.IsValueCreated)
                        .Choose(static bitmap => bitmap.Value.ToOption()),
                    release: bitmap => context.Key.Catch(() => Fin.Succ(value: Op.Side(bitmap.Dispose))),
                    key: context.Key).Map(_ => Op.Side(context.Self.cache.Clear)),
                ceded: static (_, _) => Fin.Succ(unit),
                refused: static (_, _) => Fin.Succ(unit),
                contended: static (_, _) => Fin.Succ(unit));

    private static Fin<DisplayBitmap> Load(SpriteRef sprite, BlendPair blend, Op key) =>
        key.Catch(() => Fin.Succ(value: new System.IO.MemoryStream(sprite.Content.ToArray())))
            .Bind(stream => new Lease<System.IO.MemoryStream>.Owned(Value: stream).Use(
                body: input => key.Catch(() => Fin.Succ(value: new System.Drawing.Bitmap(input)))
                    .Bind(encoded => new Lease<System.Drawing.Bitmap>.Owned(Value: encoded).Use(
                        body: source => key.Catch(() => Fin.Succ(value: new System.Drawing.Bitmap(source)))
                            .Bind(bitmap => new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap).Use(
                                body: copy => key.Catch(() => Fin.Succ(value: new DisplayBitmap(copy)))
                                    .Bind(loaded => key.Catch(() => Fin.Succ(value: Op.Side(() =>
                                            loaded.SetBlendFunction(blend.Source.Native, blend.Destination.Native))))
                                        .Map(_ => loaded)
                                        .Rollback(
                                            release: () => key.Catch(() => Fin.Succ(value: Op.Side(loaded.Dispose))),
                                            key: key)),
                                key: key)),
                        key: key)),
                key: key));

    public Fin<Unit> Release(Op? key = null) {
        Op op = key.OrDefault();
        Transition<SheetGate> closing = Cell.Step(
            gate,
            static held => held.Phase is SheetPhase.Open ? Some(held with { Phase = SheetPhase.Draining }) : None,
            Errors.None);
        return closing is Transition<SheetGate>.Committed { State.Active: 0 }
            ? Drain(op)
            : Fin.Succ(unit);
    }

    public void Dispose() => ignore(Release());
}
```

## [04]-[MARKS]

- Owner: `DisplayMark` partitions the three payload bands by backend capability while preserving one public concept; `WorldMark` is the RhinoCommon world band, grown by the retained-overlay fold (`Points`, `Vector`, `Polygon`, `Label3d` — the old eight-case `RetainedMark` deletes whole); `SpriteMark` is the ONE `DisplayBitmap` blit family whose `SpriteAnchor` closes the three anchor shapes the host publishes; `Canvas` names the four backends, each case CARRYING what its backend consumes; `Marks.Paint` is the one dispatch and `DrawReceipt` its accounted evidence.
- Entry: `Marks.Paint(canvas, marks, key)` draws one batch and accounts every mark as drawn, culled, or refused — a capability-illegal `Canvas × mark` corner lands a typed refusal ROW on the receipt and the batch continues, while a HOST fault aborts typed; `DrawReceipt.IsValid` is the empty refusal set, so a silent partial draw is unrepresentable.
- Law: the corner table is LAW, per canvas: the PIPELINE draws world marks, sprite blits, and the stroke-and-plain-text projection of kernel screen marks (fills, glyph blocks, panes, clips, poses, and OS-role faces refuse — Eto-surface capabilities); the RETAINED overlay draws the `CustomDisplay`-addressable world subset (`Points`, `Vector`, `Polygon`, undecorated `Curve`, `Label3d`) and refuses the rest; the SURFACE and PAGE replay kernel screen marks through `PaintProgram.Replay` — the kernel owns draw, cull, hit, and stock — and refuse world and sprite bands. NAMED LOSS: the per-entry backend twins (`Pipeline`/`Surface`/`ScreenPipeline`/`ScreenSurface`/`WorldPipeline`) and their per-arm refusals; bought back as the explicit corner rows this dispatch names and the receipt reports.
- Law: `Surface` and `Page` are two quality postures over one Graphics replay — `Surface` carries its caller's `ScenePolicy`, `Page` is pinned `Fidelity` because a printed page never trades quality for latency — and both hand the kernel the stock and timeline the replay is gauged against.
- Law: render order is input order; hit-testing is the KERNEL's (`PaintProgram.Hit` over the screen band) and this page answers none — the world band hit-tests through the host pick pipeline, a different owner.
- Law: an arrowhead is a HOST primitive, never a re-derived triangle — `WorldMark.Arrowhead` folds `DrawAnnotationArrowhead(Arrowhead, Transform, Color)` so head shape rides the `DimensionStyle.ArrowType` row or the user-block the `Arrowhead` carries.
- Law: the retained fold is loss-stated — `AddArc`/`AddCircle`/`AddLine` convenience adds collapse onto `AddCurve` over the caller's own curve (same geometry, one arm), and a DECORATED stroke (halo, taper, pattern) refuses on the retained arm because `CustomDisplay` draws colour-and-width alone; witness: `RetainedMark.Line(line, colour, 2)` rebuilds as `new WorldMark.Curve(new LineCurve(line.From, line.To), stroke)`.
- Law: mark admission folds every payload, style presence, and finite coordinate before dispatch; no invalid child reaches a backend, and every colour egress rides `ToDrawing` inside the arm that draws it.
- Growth: a world drawable is one `WorldMark` case and one arm per canvas that admits it; a sprite anchor is one `SpriteAnchor` case; a canvas is one `Canvas` case carrying its own context and its corner rows.
- Boundary: the kernel `Mark` vocabulary is composed VERBATIM — no local screen union, path carrier, stroke, fill, pose, text style, or paint program exists on this page, and a consumer wanting the retained screen program calls `PaintProgram.Of`.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WorldMark {
    private WorldMark() { }
    public sealed record Curve(global::Rhino.Geometry.Curve Value, Stroke Stroke) : WorldMark;
    public sealed record MeshShaded(Mesh Value, ShadedMaterial Material) : WorldMark;
    public sealed record MeshBanded(Mesh Value, PerceptualColor Colour, IsoBanding Banding) : WorldMark;
    public sealed record MeshFalseColors(Mesh Value) : WorldMark;
    public sealed record SubDShaded(SubD Value, ShadedMaterial Material) : WorldMark;
    public sealed record SubDWires(SubD Value, PerceptualColor Colour, float Width) : WorldMark;
    public sealed record BrepShaded(Brep Value, ShadedMaterial Material) : WorldMark;
    public sealed record BrepWires(Brep Value, PerceptualColor Colour, int Density) : WorldMark;
    public sealed record Block(InstanceDefinition Definition, ShadedMaterial Material, Transform Placement) : WorldMark;
    public sealed record Clipping(ClippingPlaneSurface Value, PerceptualColor Colour) : WorldMark;
    public sealed record Hatch(global::Rhino.Geometry.Hatch Value, PerceptualColor Lines, PerceptualColor Fill) : WorldMark;
    public sealed record Text(TextEntity Value, PerceptualColor Colour) : WorldMark;
    public sealed record Annotation(AnnotationBase Value, RhinoObject Owner, PerceptualColor Colour) : WorldMark;
    public sealed record Arrowhead(global::Rhino.Geometry.Arrowhead Value, Transform Placement, PerceptualColor Colour) : WorldMark;
    public sealed record Direction(SurfaceDirectionIndicators Value) : WorldMark;
    public sealed record Curvature(Brep Value, PerceptualColor Colour) : WorldMark;
    public sealed record Draft(Mesh Value, PerceptualColor Colour) : WorldMark;
    // The retained-overlay fold: four cases the old `RetainedMark` union carried land here, drawable on BOTH the
    // pipeline and the retained overlay, so the parallel retained vocabulary deletes whole.
    public sealed record Points(Seq<Point3d> Values, PerceptualColor Colour, PointUse Style, Rasm.Numerics.Dimension Radius) : WorldMark;
    public sealed record Vector(Point3d Anchor, Vector3d Span, PerceptualColor Colour, VectorTip Tip) : WorldMark;
    public sealed record Polygon(Seq<Point3d> Ring, PerceptualColor Fill, PerceptualColor Edge, PolygonPaint Paint) : WorldMark;
    public sealed record Label3d(Text3d Value, PerceptualColor Colour) : WorldMark;

    internal bool Valid => Switch(
        curve: static row => row.Value is not null && row.Stroke is not null,
        meshShaded: static row => row.Value is not null && row.Material is not null,
        meshBanded: static row => row.Value is not null && row.Banding is { Valid: true },
        meshFalseColors: static row => row.Value is not null,
        subDShaded: static row => row.Value is not null && row.Material is not null,
        subDWires: static row => row.Value is not null && row.Width > 0f && float.IsFinite(row.Width),
        brepShaded: static row => row.Value is not null && row.Material is not null,
        brepWires: static row => row.Value is not null && row.Density >= 0,
        block: static row => row.Definition is not null && row.Material is not null,
        clipping: static row => row.Value is not null,
        hatch: static row => row.Value is not null,
        text: static row => row.Value is not null,
        annotation: static row => row.Value is not null && row.Owner is not null,
        arrowhead: static row => row.Value is not null && row.Placement.IsValid,
        direction: static row => row.Value is not null,
        curvature: static row => row.Value is not null,
        draft: static row => row.Value is not null,
        points: static row => !row.Values.IsEmpty && row.Style is not null,
        vector: static row => row.Anchor.IsValid && row.Span.IsValid && row.Tip is not null,
        polygon: static row => row.Ring.Count >= 3 && row.Paint is not null,
        label3d: static row => row.Value is not null);
}

// The three anchor shapes the host sprite surface publishes: a screen blit, a world blit with its sizing regime,
// and the point-cloud batch with optional per-point colour. Sizing reuses `WidthSpace` — the same screen-or-world
// axis the stroke thickness rides — so no `bool WorldSized` survives.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpriteAnchor {
    private SpriteAnchor() { }
    public sealed record Screen(Point2d At, Size2i Extent) : SpriteAnchor;
    public sealed record World(Point3d At, PositiveMagnitude Size, WidthSpace Sizing, Option<PerceptualColor> Tint) : SpriteAnchor;
    public sealed record Cloud(Seq<Point3d> Points, Option<Seq<PerceptualColor>> Colours, PositiveMagnitude Size, WidthSpace Sizing) : SpriteAnchor;

    internal bool Valid => Switch(
        screen: static row => double.IsFinite(row.At.X) && double.IsFinite(row.At.Y) && row.Extent.Width > 0 && row.Extent.Height > 0,
        world: static row => row.At.IsValid,
        cloud: static row => !row.Points.IsEmpty
            && row.Colours.Match(Some: colours => colours.Count == row.Points.Count, None: static () => true));
}

// ONE blit family over the three anchors: the screen sprite, the world sprite, and the sprite cloud were three
// sibling spellings of one `DisplayBitmap` draw discriminated only by anchor shape.
public sealed record SpriteMark(SpriteRef Sprite, BlendPair Blend, SpriteAnchor Anchor) {
    internal bool Valid => Sprite is not null && Anchor is { Valid: true };
}

// The three payload bands under one public concept. `Screen` is the KERNEL mark composed verbatim — the partition
// is which BACKEND can draw a payload, which is RhinoCommon knowledge the kernel cannot hold.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DisplayMark {
    private DisplayMark() { }
    public sealed record Screen(Mark Value) : DisplayMark;
    public sealed record World(WorldMark Value) : DisplayMark;
    public sealed record Sprite(SpriteMark Value) : DisplayMark;

    internal bool Valid => Switch(
        screen: static row => row.Value is not null,
        world: static row => row.Value is not null && row.Value.Valid,
        sprite: static row => row.Value is not null && row.Value.Valid);
}

// Each case CARRIES what its backend consumes, so the dispatch takes no side context and a canvas cannot arrive
// missing the stock its replay needs. `Surface` and `Page` are two postures over one Graphics replay: the page is
// pinned `Fidelity` because a printed sheet never trades quality for latency.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Canvas {
    private Canvas() { }
    public sealed record Pipeline(ConduitFrame Frame, SpriteSheet Sprites) : Canvas;
    public sealed record Retained(CustomDisplay Display) : Canvas;
    public sealed record Surface(Lease<Graphics> Target, ScenePolicy Policy, PaintStock Stock, MonotonicTimeline Clock) : Canvas;
    public sealed record Page(Lease<Graphics> Target, PaintStock Stock, MonotonicTimeline Clock) : Canvas;

    internal bool Valid => Switch(
        pipeline: static row => row.Frame.Pipeline is not null && row.Sprites is not null,
        retained: static row => row.Display is not null,
        surface: static row => row.Target is not null && row.Policy is not null && row.Stock is not null && row.Clock is not null,
        page: static row => row.Target is not null && row.Stock is not null && row.Clock is not null);
}

// --- [MODELS] -------------------------------------------------------------------------------
// Accountability, not narration: every mark lands in exactly one column, refusals carry their typed cause, and the
// evidence fold is the EMPTY refusal set — a batch that skipped a mark cannot read valid.
public readonly record struct DrawReceipt(Rasm.Numerics.Dimension Drawn, Rasm.Numerics.Dimension Culled, Seq<Error> Refused) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Refused.IsEmpty);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Marks {
    private enum Outcome { Drawn, Culled }

    // Capability corners PARTITION onto the receipt; host faults ABORT typed. The fold is one pass: each admitted
    // mark answers drawn-or-culled, each illegal corner adds its typed refusal row, and the counts derive.
    public static Fin<DrawReceipt> Paint(Canvas canvas, Seq<DisplayMark> marks, Op? key = null) {
        Op op = key.OrDefault();
        return guard(canvas is not null && canvas.Valid
                && marks.ForAll(static mark => mark is not null && mark.Valid), op.InvalidInput()).ToFin()
            .Bind(_ => canvas.Switch(
                (Marks: marks, Op: op),
                pipeline: static (ctx, backend) => Immediate(ctx.Marks, ctx.Op, mark =>
                    PipelineArm(backend.Frame, backend.Sprites, mark, ctx.Op)),
                retained: static (ctx, backend) => Immediate(ctx.Marks, ctx.Op, mark =>
                    RetainedArm(backend.Display, mark, ctx.Op)),
                surface: static (ctx, backend) => Replayed(
                    ctx.Marks, backend.Target, backend.Policy, backend.Stock, backend.Clock, ctx.Op),
                page: static (ctx, backend) => Replayed(
                    ctx.Marks, backend.Target, ScenePolicy.Fidelity, backend.Stock, backend.Clock, ctx.Op)));
    }

    // One accounting fold serves both immediate backends: `None` is a capability refusal already recorded, a host
    // fault inside an arm aborts through the rail, and the counts derive from the outcomes.
    private static Fin<DrawReceipt> Immediate(
        Seq<DisplayMark> marks, Op op, Func<DisplayMark, Fin<Either<Error, Outcome>>> arm) =>
        marks.TraverseM(arm).As().Map(outcomes => new DrawReceipt(
            Drawn: Rasm.Numerics.Dimension.Create(value: outcomes.Count(static row => row is Either.Right<Error, Outcome>(Outcome.Drawn))),
            Culled: Rasm.Numerics.Dimension.Create(value: outcomes.Count(static row => row is Either.Right<Error, Outcome>(Outcome.Culled))),
            Refused: outcomes.Choose(static row => row.Match(Left: Some, Right: static _ => Option<Error>.None))));

    // The kernel owns the whole screen replay — draw, cull, stock, gauge — so the surface arms build one
    // `PaintProgram` from the screen band and fold the kernel receipt; world and sprite bands are the refusal rows.
    private static Fin<DrawReceipt> Replayed(
        Seq<DisplayMark> marks, Lease<Graphics> target, ScenePolicy policy, PaintStock stock, MonotonicTimeline clock, Op op) {
        Seq<Mark> screen = marks.Choose(static mark => mark is DisplayMark.Screen row ? Some(row.Value) : None);
        Seq<Error> refused = marks.Choose(mark => mark switch {
            DisplayMark.World row => Some((Error)op.Unsupported(row.Value.GetType(), typeof(Graphics))),
            DisplayMark.Sprite => Some((Error)op.Unsupported(typeof(SpriteMark), typeof(Graphics))),
            _ => Option<Error>.None,
        });
        return PaintProgram.Of(marks: screen, key: op)
            .Bind(program => program.Replay(target: target, policy: policy, stock: stock, clock: clock, lane: DispatchLane.Paced, key: op))
            .Map(receipt => new DrawReceipt(Drawn: receipt.Drawn, Culled: receipt.Culled, Refused: refused));
    }

    // --- [PIPELINE_ARM]
    private static Fin<Either<Error, Outcome>> PipelineArm(ConduitFrame frame, SpriteSheet sprites, DisplayMark mark, Op op) => mark.Switch(
        (Frame: frame, Sprites: sprites, Op: op),
        screen: static (ctx, row) => PipelineScope.With(
            ctx.Frame.Pipeline,
            [new RenderAspect.Screen()],
            () => ScreenProjection(ctx.Frame, row.Value, ctx.Op),
            ctx.Op),
        world: static (ctx, row) => WorldPipeline(ctx.Frame, row.Value, ctx.Op).Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        sprite: static (ctx, row) => SpritePipeline(ctx.Frame, ctx.Sprites, row.Value, ctx.Op));

    // The stroke-and-plain-text projection of the KERNEL screen vocabulary: `StrokeCase` lowers through the one
    // `PathSpec` seam onto pipeline curves under the 2d projection, `TextCase` rides `Draw2dText` when its face
    // NAMES a family — an OS-role face is an Eto capability and refuses — and every other case is a corner row.
    private static Fin<Either<Error, Outcome>> ScreenProjection(ConduitFrame frame, Mark mark, Op op) => mark switch {
        Mark.StrokeCase row =>
            (from primitives in PathPrimitive.Lower(spec: row.Path, key: op)
             from stroke in Stroke.Of(
                 colour: row.Stroke.Colour,
                 width: row.Stroke.Width,
                 space: WidthSpace.Screen,
                 cap: StrokeCap.For(row.Stroke.Cap),
                 join: StrokeJoin.For(row.Stroke.Join),
                 dash: row.Stroke.Dash,
                 key: op)
             from pen in stroke.Mint(key: op)
             from drawn in op.Catch(() => Fin.Succ(primitives.Iter(primitive => {
                 using global::Rhino.Geometry.Curve curve = primitive.Mint();
                 frame.Pipeline.DrawCurve(curve, pen);
             })))
             select Either.Right<Error, Outcome>(Outcome.Drawn)),
        Mark.TextCase { Face.Source: TypeSource.FamilyCase family } row =>
            from ink in row.Ink.ToDrawing(key: op)
            from drawn in op.Catch(() => Fin.Succ(Op.Side(() => frame.Pipeline.Draw2dText(
                row.Text,
                ink,
                new Point2d(row.At.X, row.At.Y),
                row.Block.Map(static block => block.Align == Eto.Drawing.FormattedTextAlignment.Center).IfNone(false),
                (int)row.Face.Size.Map(static size => size.Value).IfNone(12d),
                family.Family.Value))))
            select Either.Right<Error, Outcome>(Outcome.Drawn),
        _ => Fin.Succ(Either.Left<Error, Outcome>(op.Unsupported(mark.GetType(), typeof(DisplayPipeline)))),
    };

    private static Fin<Either<Error, Outcome>> SpritePipeline(ConduitFrame frame, SpriteSheet sprites, SpriteMark mark, Op op) =>
        sprites.Use(mark.Sprite, mark.Blend, bitmap => mark.Anchor.Switch(
            (Frame: frame, Bitmap: bitmap, Op: op),
            screen: static (ctx, row) => ctx.Op.Catch(() => Fin.Succ(Op.Side(() =>
                ctx.Frame.Pipeline.DrawSprite(ctx.Bitmap, new Point2d(row.At.X, row.At.Y), row.Extent.Width, row.Extent.Height)))),
            world: static (ctx, row) =>
                from tint in row.Tint.Traverse(colour => colour.ToDrawing(key: ctx.Op)).As()
                from drawn in ctx.Op.Catch(() => Fin.Succ(Op.Side(() => ctx.Frame.Pipeline.DrawSprite(
                    ctx.Bitmap, row.At, (float)row.Size.Value,
                    tint.IfNone(System.Drawing.Color.White),
                    row.Sizing == WidthSpace.World))))
                select drawn,
            cloud: static (ctx, row) =>
                from inks in row.Colours.Traverse(colours => colours.Traverse(colour => colour.ToDrawing(key: ctx.Op)).As()).As()
                from drawn in ctx.Op.Catch(() => {
                    DisplayBitmapDrawList list = new();
                    _ = inks.Match(
                        Some: colours => Op.Side(() => list.SetPoints(row.Points.AsEnumerable(), colours.AsEnumerable())),
                        None: () => Op.Side(() => list.SetPoints(row.Points.AsEnumerable())));
                    ctx.Frame.Pipeline.DrawSprites(ctx.Bitmap, list, (float)row.Size.Value, row.Sizing == WidthSpace.World);
                    return Fin.Succ(unit);
                })
                select drawn), op)
        .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn));

    private static Fin<Unit> WorldPipeline(ConduitFrame frame, WorldMark mark, Op key) => mark.Switch(
        (Frame: frame, Op: key),
        curve: static (ctx, row) => row.Stroke.Mint(key: ctx.Op)
            .Bind(pen => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawCurve(row.Value, pen))),
        // Every shaded arm mints its native inside `ShadedMaterial.Use`, draws within that bracket, and releases on exit.
        meshShaded: static (ctx, row) => row.Material.Use(
            material => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawMeshShaded(row.Value, material)), ctx.Op),
        meshBanded: static (ctx, row) =>
            from ink in row.Colour.ToDrawing(key: ctx.Op)
            from effect in row.Banding.Mint(ctx.Op)
            from drawn in ctx.Op.Catch(() => Fin.Succ(Op.Side(() => ctx.Frame.Pipeline.DrawMeshShaded(row.Value, ink, effect))))
            select drawn,
        meshFalseColors: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawMeshFalseColors(row.Value)),
        subDShaded: static (ctx, row) => row.Material.Use(
            material => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawSubDShaded(row.Value, material)), ctx.Op),
        subDWires: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawSubDWires(row.Value, ink, row.Width))),
        brepShaded: static (ctx, row) => row.Material.Use(
            material => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawBrepShaded(row.Value, material)), ctx.Op),
        brepWires: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawBrepWires(row.Value, ink, row.Density))),
        block: static (ctx, row) => row.Material.Use(
            material => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawInstanceDefinitionShaded(row.Definition, material, row.Placement)), ctx.Op),
        clipping: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawClippingPlaneWires(row.Value, ink))),
        hatch: static (ctx, row) =>
            from lines in row.Lines.ToDrawing(key: ctx.Op)
            from fill in row.Fill.ToDrawing(key: ctx.Op)
            from drawn in ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawHatch(row.Value, lines, fill))
            select drawn,
        text: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawText(row.Value, ink))),
        annotation: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawAnnotation(row.Value, row.Owner, ink))),
        arrowhead: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawAnnotationArrowhead(row.Value, row.Placement, ink))),
        direction: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawSurfaceDirectionIndicators(row.Value)),
        curvature: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawCurvaturePreview(row.Value, ink))),
        draft: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawDraftAnglePreview(row.Value, ink))),
        points: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => Fin.Succ(Op.Side(() =>
                ctx.Frame.Pipeline.DrawPoints(row.Values.AsEnumerable(), row.Style.Native, row.Radius.Value, ink))))),
        vector: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => Fin.Succ(Op.Side(() => {
                ctx.Frame.Pipeline.DrawArrow(new Line(row.Anchor, row.Anchor + row.Span), ink);
                _ = Op.SideWhen(row.Tip.Dot, () => ctx.Frame.Pipeline.DrawPoint(row.Anchor, ink));
            })))),
        polygon: static (ctx, row) =>
            from fill in row.Fill.ToDrawing(key: ctx.Op)
            from edge in row.Edge.ToDrawing(key: ctx.Op)
            from drawn in ctx.Op.Catch(() => Fin.Succ(Op.Side(() => {
                _ = Op.SideWhen(row.Paint.Fill, () => ctx.Frame.Pipeline.DrawPolygon(row.Ring.AsEnumerable(), fill, filled: true));
                _ = Op.SideWhen(row.Paint.Edge, () => ctx.Frame.Pipeline.DrawPolygon(row.Ring.AsEnumerable(), edge, filled: false));
            })))
            select drawn,
        label3d: static (ctx, row) => row.Colour.ToDrawing(key: ctx.Op)
            .Bind(ink => ctx.Op.Catch(() => Fin.Succ(Op.Side(() => ctx.Frame.Pipeline.Draw3dText(row.Value, ink))))));

    // --- [RETAINED_ARM]
    // The `CustomDisplay`-addressable subset: colour-and-width alone, so a decorated stroke refuses the corner by
    // name and the convenience adds collapse onto `AddCurve`.
    private static Fin<Either<Error, Outcome>> RetainedArm(CustomDisplay display, DisplayMark mark, Op op) => mark switch {
        DisplayMark.World { Value: WorldMark.Points row } =>
            row.Colour.ToDrawing(key: op).Bind(ink => op.Catch(() => Fin.Succ(Op.Side(() =>
                display.AddPoints(row.Values.AsEnumerable(), ink, row.Style.Native, row.Radius.Value)))))
            .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        DisplayMark.World { Value: WorldMark.Vector row } =>
            row.Colour.ToDrawing(key: op).Bind(ink => op.Catch(() => Fin.Succ(Op.Side(() =>
                display.AddVector(row.Anchor, row.Span, ink, row.Tip.Dot)))))
            .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        DisplayMark.World { Value: WorldMark.Polygon row } =>
            from fill in row.Fill.ToDrawing(key: op)
            from edge in row.Edge.ToDrawing(key: op)
            from drawn in op.Catch(() => Fin.Succ(Op.Side(() =>
                display.AddPolygon(row.Ring.AsEnumerable(), fill, edge, row.Paint.Fill, row.Paint.Edge))))
            select Either.Right<Error, Outcome>(Outcome.Drawn),
        DisplayMark.World { Value: WorldMark.Curve { Stroke.Decoration: var decoration } row }
            when decoration == PenDecoration.Bare =>
            row.Stroke.Colour.ToDrawing(key: op).Bind(ink => op.Catch(() => Fin.Succ(Op.Side(() =>
                display.AddCurve(row.Value, ink, (int)Math.Max(1d, row.Stroke.Width.Value))))))
            .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        DisplayMark.World { Value: WorldMark.Label3d row } =>
            row.Colour.ToDrawing(key: op).Bind(ink => op.Catch(() => Fin.Succ(Op.Side(() =>
                display.AddText(row.Value, ink)))))
            .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        _ => Fin.Succ(Either.Left<Error, Outcome>(op.Unsupported(mark.GetType(), typeof(CustomDisplay)))),
    };
}
```

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-display.md` — `DisplayPipeline` draw verbs, `DisplayPen`/`DisplayBitmap` surfaces); `Eto.Drawing` (`Rasm.Rhino/.api/api-eto-drawing.md` — `PenLineCap`/`PenLineJoin` aliased at the pen boundary); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` mark rows, `[ComplexValueObject]` pens); kernel `Interaction/paint` (`Marks` partition floor) + `Numerics/atoms` (`PerceptualColor`).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
