# [RASM_RHINO_DISPLAY_DRAW]

`Marks` owns the package's ONE draw dispatch over four canvases — the live display pipeline, the retained `CustomDisplay` overlay, an interactive Eto surface, and a replayed page — and `DisplayMark` is its one vocabulary: the SCREEN band is the kernel `Rasm.Interaction` mark algebra composed verbatim, the WORLD band is `WorldMark`, the RhinoCommon payloads only a `DisplayPipeline` or `CustomDisplay` can draw, and the SPRITE band is the `DisplayBitmap` blit family only the pipeline's GPU blend path serves. The partition is HOST knowledge: a world mark routed to a surface, a kernel fill routed to the pipeline, and a sprite routed to a retained overlay each land as a typed refusal row on `DrawTally`, never a silent partial draw.

The Eto half of the old two-backend algebra is DELETED, not moved: paths, fills, strokes, glyph blocks, poses, clips, text shaping, the paint program, the resource stock, and hit-testing are `Interaction/paint`'s (`Mark`, `PathSpec`, `FillSource`, `StrokeSpec`, `GlyphBlock`, `PosePlan`, `PaintProgram`, `PaintStock`, `Surface`), and a consumer wanting the retained screen program calls `PaintProgram.Of` directly. What stays here is what RhinoCommon alone can know — the `DisplayPen` projection with its halo, taper, and pattern axes, the `DisplayMaterial` custody bracket, the iso-banding effect, the sprite cache, and the world mark family. `PerceptualColor` remains the only colour source and every host egress composes the kernel `ToDrawing` projection, so an out-of-gamut ink refuses typed instead of clipping.

## [01]-[INDEX]

- [02]-[STYLE]: `StrokeCap`, `StrokeJoin`, `WidthSpace`, `PatternTrait`, `PatternLaw`, `PenDecoration`, `Stroke`, `PenRhythm`, `ShadedFace`, `ShadedMaterial`, `BlendUse`, `BlendPair` — the display-pen and shaded-appearance projections.
- [03]-[ASSETS]: `PathPrimitive`, `SpriteSource`, `ISpriteFiles`, `SpriteRef`, `SpriteSheet`, `PointUse`, `VectorTip`, `PolygonPaint`, `IsoMode`, `IsoGap`, `IsoBanding` — lowered pipeline geometry, native sprite custody, and the world-mark vocabularies.
- [04]-[MARKS]: `WorldMark`, `SpriteAnchor`, `SpriteMark`, `DisplayMark`, `Canvas`, `DrawTally`, `Marks` — the one mark union, the four canvases, and the accounted paint dispatch.

## [02]-[STYLE]

- Owner: `Stroke` is the display-pen spec — colour, ladder-free screen-or-world thickness, cap, join, the KERNEL `Dash`, decoration, and pattern policy — and `Mint` is its one `DisplayPen` projection; `PenRhythm` is the pipeline's interval projection of the kernel dash; `ShadedFace`/`ShadedMaterial` bracket the disposable `DisplayMaterial`; `BlendUse` mirrors the host blend roster and `BlendPair` is the source-and-destination pair every sprite blit names once.
- Cases: `WidthSpace` closes the thickness regime at two rows carrying `CoordinateSystem` — the `bool WorldWidth` and the ternary that read it delete; `PatternTrait` is the pattern capability vocabulary (`Autoscale`, `BySegment`, `WorldLength` — every corner legal, law `Open`) and `PatternLaw` carries the set beside the scale and offset the host pattern engine reads; `PenDecoration` owns the halo and taper axes as admitted component records, so an anonymous tuple with hand positivity guards no longer rides the stroke.
- Law: the DASH is the kernel's. `Interaction.Dash` is the one dash vocabulary and `PenRhythm.Table` is its total pipeline projection into width-multiple intervals; `PenRhythm.Admit` states the host bound — `DisplayPen.SetPattern` accepts at most eight entries (`PACKAGE_LIMIT_AS_LAW`), so a longer `PatternedCase` refuses at `Stroke.Of` rather than truncating inside a paint call. The kernel `PatternedCase.Offset` serves the Eto dash alone; the pipeline pattern offset is `PatternLaw`'s own column, and the two never alias.
- Law: this pen is a DISPLAY thickness — screen pixels or world units — and never a paper weight. A plotted line width reads the `Drawing/sheet` `LineWidth` ladder at the publishing surface; a screen hairline is the kernel `StrokeSpec.Hairline` device law; neither aliases this column, and a `Stroke` fed into a plot is the strata violation the publish page refuses.
- Law: shaded appearance is `ShadedMaterial`, never a host `DisplayMaterial` — the native is disposable and carries eight raw screen colours, so it mints inside `Use`, serves exactly the bracketed draw call, and releases on every exit; the second face is `Option`, so a one-sided material spells no back band rather than mirroring the front. Every quantization inside the bracket rides `PerceptualColor.ToDrawing`, so a wide-gamut face refuses typed before the native exists.
- Law: `BlendPair.Over` is the one canonical row — the Porter-Duff source-over pair (`SourceAlpha`, `InverseSourceAlpha`) both boundaries hand-spelled at every blit — and any other composition names its pair explicitly.
- Growth: a pattern axis is one `PatternTrait` row; a decoration axis is one component record on `PenDecoration`; a blend mode is one `BlendUse` row.
- Boundary: no `System.Drawing` or Eto colour becomes domain state; `LineCapStyle`/`LineJoinStyle`/`CoordinateSystem`/`BlendMode` live only as row columns.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
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

    private static BlendUse Row(int key, BlendMode native) => new(native);

    internal BlendMode Native { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
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

    public static Fin<PatternLaw> Portable => Seed.Value;
    private static readonly Lazy<Fin<PatternLaw>> Seed = new(static () =>
        (Validate(CapabilitySet<PatternTrait>.Of(), PositiveMagnitude.Create(value: 1d), 0f, out PatternLaw? law), law) switch {
            (null, PatternLaw seeded) => Fin.Succ(seeded),
            (ValidationError refusal, _) => Fin.Fail<PatternLaw>(new KernelFault.InvalidValue(Label: nameof(Portable), Requirement: refusal.Message)),
            _ => Fin.Fail<PatternLaw>(new KernelFault.InvalidResult()),
        });
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

    public static Fin<Stroke> Of(
        PerceptualColor colour,
        PositiveMagnitude width,
        WidthSpace space,
        StrokeCap cap,
        StrokeJoin join,
        Dash dash,
        Option<PenDecoration> decoration = default,
        Option<PatternLaw> pattern = default,
        float miter = 10f) {
        return (
                (float.IsFinite(miter) && miter >= 1f
                    ? Validation<Error, float>.Success(miter)
                    : Validation<Error, float>.Fail(new KernelFault.InvalidInput(Axis: Some(nameof(miter))))),
                PenRhythm.Admit(dash: dash).ToValidation(),
                pattern.Match(Some: Validation<Error, PatternLaw>.Success, None: () => Portable.ToValidation()))
            .Apply((admittedMiter, admittedDash, admittedPattern) => new Stroke(
                colour, width, space, cap, join, admittedDash,
                decoration.IfNone(PenDecoration.Bare), admittedPattern, admittedMiter))
            .As().ToFin();
    }

    internal Fin<DisplayPen> Mint() =>
        from ink in Colour.ToDrawing()
        from halo in Decoration.Halo.Traverse(row => row.Colour.ToDrawing().Map(quantized => (Row: row, Ink: quantized))).As()
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
        _ = HostEdge.SideWhen(!gaps.IsEmpty, () => {
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

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class PenRhythm {
    private const int MaxIntervals = 8;

    internal static Fin<Dash> Admit(Dash dash) => dash is Dash.PatternedCase row
        ? !row.Intervals.IsEmpty
            && row.Intervals.Count <= MaxIntervals
            && row.Intervals.ForAll(static gap => float.IsFinite(gap) && gap > 0f)
            ? Fin.Succ(dash)
            : Fin.Fail<Dash>(new KernelFault.InvalidInput(Axis: Some(nameof(Dash.PatternedCase.Intervals))))
        : Fin.Succ(dash);

    internal static Seq<float> Table(Dash dash) => dash.Switch(
        solidCase: static _ => Seq<float>(),
        dashedCase: static _ => [3f, 1f],
        dottedCase: static _ => [1f, 1f],
        dashDotCase: static _ => [3f, 1f, 1f, 1f],
        dashDotDotCase: static _ => [3f, 1f, 1f, 1f, 1f, 1f],
        patternedCase: static row => row.Intervals);
}

// --- [MODELS] --------------------------------------------------------------------------
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
        validationError = ShineAxis.Unit.Shine(shine) && ShineAxis.Unit.Transparency(transparency)
            ? null
            : new ValidationError(message: "Shaded face shine or transparency leaves the unit interval.");

    internal Validation<Error, (System.Drawing.Color Diffuse, System.Drawing.Color Specular, System.Drawing.Color Ambient, System.Drawing.Color Emission)> Quantized() =>
        (Diffuse.ToDrawing().ToValidation(),
         Specular.ToDrawing().ToValidation(),
         Ambient.ToDrawing().ToValidation(),
         Emission.ToDrawing().ToValidation())
        .Apply(static (diffuse, specular, ambient, emission) => (diffuse, specular, ambient, emission)).As();
}

public sealed record ShadedMaterial(ShadedFace Front, Option<ShadedFace> Back) {
    internal Fin<TResult> Use<TResult>(Func<DisplayMaterial, Fin<TResult>> project) =>
        from front in Front.Quantized().ToFin()
        from back in Back.Traverse(face => face.Quantized().ToFin().Map(channels => (Face: face, Channels: channels))).As()
        from projected in Try.lift(() => {
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
        }).Run().Bind(static inner => inner)
        select projected;
}
```

## [03]-[ASSETS]

- Owner: `PathPrimitive` is the pipeline's lowered screen geometry — four curve-mintable cases — and `Lower` is the ONE projection from the kernel `PathSpec` onto it; `SpriteRef` admits sprite bytes under a content-hash identity and `SpriteSheet` owns the `DisplayBitmap` cache; `PointUse`, `VectorTip`, and `PolygonPaint` are world-mark vocabularies; `IsoBanding` owns banded-shading data with `IsoGap` closing its gap tri-state.
- Law: the composite lowering is the KERNEL's twice over — the authored figure is `Interaction.PathSpec` and the corner walk, radius clamp, and cardinal-spline arithmetic are `Rasm.Parametric`'s (`ParametricOp.RoundedRectangle`, `ParametricOp.CardinalSpline`), so this page carries no curve arithmetic a NURBS emission could disagree with. Frame-local coordinates ARE screen coordinates (`Plane.WorldXY` over the rectangle's own intervals), and angles stay RADIANS end to end — the kernel answers radians and `ArcCurve` consumes them, so the old degree round-trip is deleted whole.
- Law: an elliptical `ArcCase` refuses on the pipeline — `PathPrimitive.Arc` is circular because the pipeline curve mint is — and the refusal names the corner; a non-circular arc rides `CurveCase` or `EllipseCase` instead. The role-resolved OS faces, fills, glyph blocks, panes, clips, and poses refuse the same way at the mark projection: each is an Eto-surface capability the kernel replays and the pipeline cannot.
- Law: sprite bytes admitted by `ISpriteFiles` enter once through `SpriteRef.Of` under `ContentHash.Hex` — the kernel's one lowercase identity text — so two sources with one payload share one cache row and no raw path crosses the asset boundary.
- Law: the sheet's lifecycle is a `Cell`-stepped state machine, never a monitor barrier — `Use` enters through a guarded step that declines once draining begins, a leave decrements, and the last borrower leaving a draining sheet performs all-attempted disposal. `Release()` carries immediate cleanup faults; `Dispose` is only the host-required adapter.
- Law: `PointUse` carries one row per DISTINCT host marker — `PointStyle` is an aliased enum (`Circle`≡`RoundSimple`, `Square`≡`Simple`, `SolidSquare`≡`VariableDot`, `RoundDot`≡`SolidRound`≡`SolidCircle`), so a row per alias would seat values no host call can tell apart.
- Cases: `VectorTip` closes the vector-anchor axis (`Plain`, `Anchored`) — the `bool AnchorPoint` deletes; `PolygonPaint` closes the fill-and-edge product at its three LEGAL corners (`Filled`, `Edged`, `Full`) — the `(false, false)` corner that drew nothing is unrepresentable; `IsoGap` closes the gap tri-state (`Painted`, `Discarded`) under an `Option` — the tuple's `bool Discard` deletes.
- Boundary: cache disposal closes admission, drains every draw-scoped use, releases each native bitmap once, and clears the table.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathPrimitive {
    private PathPrimitive() { }
    public sealed record Line(Point2d From, Point2d To) : PathPrimitive;
    public sealed record Arc(Point2d Center, double Radius, VectorAngle Start, VectorAngle Sweep) : PathPrimitive;
    public sealed record Bezier(Point2d Start, Point2d Control1, Point2d Control2, Point2d End) : PathPrimitive;
    public sealed record Ellipse(Point2d Centre, double RadiusX, double RadiusY) : PathPrimitive;

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

    internal static Fin<Seq<PathPrimitive>> Lower(PathSpec spec) => spec.Switch(
        state: key,
        lineCase: static (_, row) => Fin.Succ(Seq<PathPrimitive>(new Line(P(row.From), P(row.To)))),
        polylineCase: static (_, row) => Fin.Succ(Chained(row.Points, closed: false)),
        polygonCase: static (_, row) => Fin.Succ(Chained(row.Points, closed: true)),
        rectCase: static (_, row) => Fin.Succ(Edges(row.Frame)),
        roundRectCase: static (row) => Outlined(new ParametricOp.RoundedRectangle(
            Frame: Plane.WorldXY,
            X: new Interval(row.Frame.X, row.Frame.X + row.Frame.Width),
            Y: new Interval(row.Frame.Y, row.Frame.Y + row.Frame.Height),
            NW: row.NW, NE: row.NE, SE: row.SE, SW: row.SW)),
        ellipseCase: static (_, row) => Fin.Succ(Seq<PathPrimitive>(new Ellipse(
            new Point2d(row.Frame.X + (row.Frame.Width / 2.0), row.Frame.Y + (row.Frame.Height / 2.0)),
            row.Frame.Width / 2.0,
            row.Frame.Height / 2.0))),
        arcCase: static (row) => Math.Abs(row.Frame.Width - row.Frame.Height) <= float.Epsilon
            ? Fin.Succ(Seq<PathPrimitive>(new Arc(
                new Point2d(row.Frame.X + (row.Frame.Width / 2.0), row.Frame.Y + (row.Frame.Height / 2.0)),
                row.Frame.Width / 2.0,
                row.Start,
                row.Sweep)))
            : Fin.Fail<Seq<PathPrimitive>>(new KernelFault.Unsupported(typeof(PathSpec.ArcCase), typeof(DisplayPipeline))),
        bezierCase: static (_, row) => Fin.Succ(Seq<PathPrimitive>(new Bezier(P(row.From), P(row.ControlA), P(row.ControlB), P(row.To)))),
        curveCase: static (row) => Outlined(new ParametricOp.CardinalSpline(
            Frame: Plane.WorldXY,
            Points: new Arr<Point2d>([.. toSeq(row.Points).Map(P)]),
            Tension: row.Tension,
            Closed: false)),
        compositeCase: static (row) => row.Figures.TraverseM(figure => Lower(spec: figure)).As()
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

    private static Fin<Seq<PathPrimitive>> Outlined(ParametricOp op) =>
        Parametric.Apply().Bind(result => result is ParametricResult.Outline outline
            ? Fin.Succ(toSeq(outline.Run).Map(Planar).Strict())
            : Fin.Fail<Seq<PathPrimitive>>(new KernelFault.InvalidResult(Detail: Some(result.GetType().Name))));

    private static PathPrimitive Planar(PlanarPrimitive primitive) => primitive.Switch(
        segment: static row => (PathPrimitive)new Line(row.From, row.To),
        arc: static row => new Arc(Center: row.Center, Radius: row.Radius.Value, Start: row.Start, Sweep: row.Sweep),
        bezier: static row => new Bezier(row.Start, row.Control1, row.Control2, row.End));
}

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

    private static PointUse Row(int key, PointStyle native) => new(native);

    internal PointStyle Native { get; }
}

[SmartEnum<int>]
public sealed partial class VectorTip {
    public static readonly VectorTip Plain = new(key: 0, dot: false);
    public static readonly VectorTip Anchored = new(key: 1, dot: true);
    internal bool Dot { get; }
}

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

    private static IsoMode Row(int key, IsoDrawMode native) => new(native);

    internal IsoDrawMode Native { get; }
}

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
    Fin<ReadOnlyMemory<byte>> Read(string asset);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpriteSource {
    private SpriteSource() { }
    public sealed record Bytes(ReadOnlyMemory<byte> Value) : SpriteSource;
    public sealed record File(ISpriteFiles Files, string Asset) : SpriteSource;

    internal bool Valid => Switch(
        bytes: static row => !row.Value.IsEmpty,
        file: static row => row.Files is not null && !string.IsNullOrWhiteSpace(row.Asset));

    internal Fin<ReadOnlyMemory<byte>> Read() => Switch(bytes: static (_, row) => Fin.Succ(row.Value),
        file: static (op, row) => Try.lift(() => row.Files.Read(row.Asset)).Run().Bind(static inner => inner));
}

public sealed record SpriteRef {
    private SpriteRef(string key, ReadOnlyMemory<byte> content) => (Key, Content) = (content);
    public string Key { get; }
    internal ReadOnlyMemory<byte> Content { get; }

    public static Fin<SpriteRef> Of(SpriteSource source) {
        return guard(source is not null && source.Valid, new KernelFault.InvalidInput()).ToFin()
            .Bind(_ => source.Read())
            .Bind(content => {
                ReadOnlyMemory<byte> owned = content.ToArray();
                return guard(!owned.IsEmpty, new KernelFault.InvalidInput()).ToFin()
                    .Map(_ => new SpriteRef(ContentHash.Hex(ContentHash.Of(owned.Span)), owned));
            });
    }
}

// --- [MODELS] --------------------------------------------------------------------------
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

    internal Fin<IsoDrawEffect> Mint() =>
        from _ in guard(Valid, new KernelFault.InvalidInput()).ToFin()
        from ramp in From.Ramp(To, Bands).Traverse(colour => colour.ToDrawing()).As()
        from gap in Gap.Traverse(row => row switch {
            IsoGap.Painted painted => painted.Colour.ToDrawing().Map(ink => (Ink: Some(ink), painted.Size, Discard: false)),
            IsoGap.Discarded cut => Fin.Succ((Ink: Option<System.Drawing.Color>.None, cut.Size, Discard: true)),
            _ => Fin.Fail<(Option<System.Drawing.Color>, double, bool)>(new KernelFault.InvalidInput()),
        }).As()
        from effect in Try.lift(() => {
            IsoDrawEffect value = new() { DrawMode = Mode.Native, Direction = Direction, Point = Anchor, Frequency = Frequency, RotationRadians = Rotation, Falloff = Falloff, UsedBandColorCount = Bands.Value };
            _ = gap.Iter(row => {
                _ = row.Ink.Iter(ink => value.GapColor = ink);
                (value.GapSize, value.DiscardGap) = (row.Size, row.Discard);
            });
            _ = ramp.Map(static (ink, index) => (ink, index)).Iter(row => ignore(value.SetBandColor(row.index, row.ink)));
            return Fin.Succ(value);
        }).Run().Bind(static inner => inner)
        select effect;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SpriteSheet : IDisposable {
    private enum SheetPhase { Open, Draining, Released }
    private sealed record SheetGate(int Active, SheetPhase Phase);

    private readonly System.Collections.Concurrent.ConcurrentDictionary<(string Key, BlendMode Src, BlendMode Dst), Lazy<Fin<DisplayBitmap>>> cache = new();
    private readonly Atom<SheetGate> gate = Atom(new SheetGate(Active: 0, Phase: SheetPhase.Open));

    internal Fin<TResult> Use<TResult>(
        SpriteRef sprite,
        BlendPair blend,
        Func<DisplayBitmap, Fin<TResult>> use) =>
        Cell.Step(gate, static held => held.Phase is SheetPhase.Open ? Some(held with { Active = held.Active + 1 }) : None, new KernelFault.InvalidContext())
            .Switch(
                state: (Self: this, Sprite: sprite, Blend: blend, Use: use),
                committed: static (ctx, _) => ctx.Self.Borrowed(ctx.Sprite, ctx.Blend, ctx.Use, ctx.Key),
                ceded: static (ctx, _) => Fin.Fail<TResult>(new KernelFault.InvalidContext()),
                refused: static (_, row) => Fin.Fail<TResult>(row.Cause),
                contended: static (ctx, _) => Fin.Fail<TResult>(new KernelFault.InvalidResult()));

    private Fin<TResult> Borrowed<TResult>(SpriteRef sprite, BlendPair blend, Func<DisplayBitmap, Fin<TResult>> use) {
        Fin<TResult> primary = Try.lift(() => {
            Lazy<Fin<DisplayBitmap>> cached = cache.GetOrAdd(
                (sprite.Key, blend.Source.Native, blend.Destination.Native),
                _ => new Lazy<Fin<DisplayBitmap>>(
                    () => Load(sprite: sprite, blend: blend),
                    LazyThreadSafetyMode.ExecutionAndPublication));
            return cached.Value.Match(
                Succ: bitmap => Try.lift(() => use(bitmap)).Run().Bind(static inner => inner),
                Fail: failure => (
                    cache.TryRemove(new KeyValuePair<(string, BlendMode, BlendMode), Lazy<Fin<DisplayBitmap>>>((sprite.Key, blend.Source.Native, blend.Destination.Native), cached)),
                    Fin.Fail<TResult>(failure)).Item2);
        }).Run().Bind(static inner => inner);
        return primary.Settled(held: Seq(unit), release: _ => Leave());
    }

    private Fin<Unit> Leave() => Try.lift(() => {
        Transition<SheetGate> left = Cell.Commit(gate, static held => held with { Active = held.Active - 1 });
        return left.Current is { Active: 0, Phase: SheetPhase.Draining } ? Drain() : Fin.Succ(unit);
    }).Run().Bind(static inner => inner);

    private Fin<Unit> Drain() =>
        Cell.Step(gate, static held => held.Phase is SheetPhase.Draining ? Some(held with { Phase = SheetPhase.Released }) : None, Errors.None)
            .Switch(
                state: this,
                committed: static (context, _) => Custody.Release(
                    held: toSeq(context.cache.Values)
                        .Filter(static bitmap => bitmap.IsValueCreated)
                        .Choose(static bitmap => bitmap.Value.ToOption()),
                    release: bitmap => Try.lift(() => Fin.Succ(value: HostEdge.Side(bitmap.Dispose))).Run().Bind(static inner => inner)).Map(_ => HostEdge.Side(context.cache.Clear)),
                ceded: static (_, _) => Fin.Succ(unit),
                refused: static (_, _) => Fin.Succ(unit),
                contended: static (_, _) => Fin.Succ(unit));

    private static Fin<DisplayBitmap> Load(SpriteRef sprite, BlendPair blend) =>
        Try.lift(() => Fin.Succ(value: new System.IO.MemoryStream(sprite.Content.ToArray()))).Run().Bind(static inner => inner)
            .Bind(stream => new Lease<System.IO.MemoryStream>.Owned(Value: stream).Use(
                body: input => Try.lift(() => Fin.Succ(value: new System.Drawing.Bitmap(input))).Run().Bind(static inner => inner)
                    .Bind(encoded => new Lease<System.Drawing.Bitmap>.Owned(Value: encoded).Use(
                        body: source => Try.lift(() => Fin.Succ(value: new System.Drawing.Bitmap(source))).Run().Bind(static inner => inner)
                            .Bind(bitmap => new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap).Use(
                                body: copy => Try.lift(() => Fin.Succ(value: new DisplayBitmap(copy))).Run().Bind(static inner => inner)
                                    .Bind(loaded => Try.lift(() => Fin.Succ(value: HostEdge.Side(() =>
                                            loaded.SetBlendFunction(blend.Source.Native, blend.Destination.Native)))).Run().Bind(static inner => inner)
                                        .Map(_ => loaded)
                                        .Rollback(
                                            release: () => Try.lift(() => Fin.Succ(value: HostEdge.Side(loaded.Dispose))).Run().Bind(static inner => inner)))))))));

    public Fin<Unit> Release() {
        Transition<SheetGate> closing = Cell.Step(
            gate,
            static held => held.Phase is SheetPhase.Open ? Some(held with { Phase = SheetPhase.Draining }) : None,
            Errors.None);
        return closing is Transition<SheetGate>.Committed { State.Active: 0 }
            ? Drain()
            : Fin.Succ(unit);
    }

    public void Dispose() => ignore(Release());
}
```

## [04]-[MARKS]

- Owner: `DisplayMark` partitions the three payload bands by backend capability while preserving one public concept; `WorldMark` is the RhinoCommon world band, grown by the retained-overlay fold (`Points`, `Vector`, `Polygon`, `Label3d` — the old eight-case `RetainedMark` deletes whole); `SpriteMark` is the ONE `DisplayBitmap` blit family whose `SpriteAnchor` closes the three anchor shapes the host publishes; `Canvas` names the four backends, each case CARRYING what its backend consumes; `Marks.Paint` is the one dispatch and `DrawTally` its accounted evidence.
- Entry: `Marks.Paint(canvas, marks)` draws one batch and accounts every mark as drawn, culled, or refused — a capability-illegal `Canvas × mark` corner lands a typed refusal ROW on `DrawTally` and the batch continues, while a HOST fault aborts typed; `DrawTally.IsValid` is the empty refusal set, so a silent partial draw is unrepresentable.
- Law: the corner table is LAW, per canvas: the PIPELINE draws world marks, sprite blits, and the stroke-and-plain-text projection of kernel screen marks (fills, glyph blocks, panes, clips, poses, and OS-role faces refuse — Eto-surface capabilities); the RETAINED overlay draws the `CustomDisplay`-addressable world subset (`Points`, `Vector`, `Polygon`, undecorated `Curve`, `Label3d`) and refuses the rest; the SURFACE and PAGE replay kernel screen marks through `PaintProgram.Replay` — the kernel owns draw, cull, hit, and stock — and refuse world and sprite bands. NAMED LOSS: the per-entry backend twins (`Pipeline`/`Surface`/`ScreenPipeline`/`ScreenSurface`/`WorldPipeline`) and their per-arm refusals; bought back as the explicit corner rows this dispatch names and `DrawTally` reports.
- Law: `Surface` and `Page` are two quality postures over one Graphics replay — `Surface` carries its caller's `ScenePolicy`, `Page` is pinned `Fidelity` because a printed page never trades quality for latency — and both hand the kernel the stock and timeline the replay is gauged against.
- Law: render order is input order; hit-testing is the KERNEL's (`PaintProgram.Hit` over the screen band) and this page answers none — the world band hit-tests through the host pick pipeline, a different owner.
- Law: an arrowhead is a HOST primitive, never a re-derived triangle — `WorldMark.Arrowhead` folds `DrawAnnotationArrowhead(Arrowhead, Transform, Color)` so head shape rides the `DimensionStyle.ArrowType` row or the user-block the `Arrowhead` carries.
- Law: the retained fold is loss-stated — `AddArc`/`AddCircle`/`AddLine` convenience adds collapse onto `AddCurve` over the caller's own curve (same geometry, one arm), and a DECORATED stroke (halo, taper, pattern) refuses on the retained arm because `CustomDisplay` draws colour-and-width alone; witness: `RetainedMark.Line(line, colour, 2)` rebuilds as `new WorldMark.Curve(new LineCurve(line.From, line.To), stroke)`.
- Law: mark admission folds every payload, style presence, and finite coordinate before dispatch; no invalid child reaches a backend, and every colour egress rides `ToDrawing` inside the arm that draws it.
- Growth: a world drawable is one `WorldMark` case and one arm per canvas that admits it; a sprite anchor is one `SpriteAnchor` case; a canvas is one `Canvas` case carrying its own context and its corner rows.
- Boundary: the kernel `Mark` vocabulary is composed VERBATIM — no local screen union, path carrier, stroke, fill, pose, text style, or paint program exists on this page, and a consumer wanting the retained screen program calls `PaintProgram.Of`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

public sealed record SpriteMark(SpriteRef Sprite, BlendPair Blend, SpriteAnchor Anchor) {
    internal bool Valid => Sprite is not null && Anchor is { Valid: true };
}

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

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DrawTally(Rasm.Numerics.Dimension Drawn, Rasm.Numerics.Dimension Culled, Seq<Error> Refused) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Refused.IsEmpty);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Marks {
    private enum Outcome { Drawn, Culled }

    public static Fin<DrawTally> Paint(Canvas canvas, Seq<DisplayMark> marks) {
        return guard(canvas is not null && canvas.Valid
                && marks.ForAll(static mark => mark is not null && mark.Valid), new KernelFault.InvalidInput()).ToFin()
            .Bind(_ => canvas.Switch(
                marks,
                pipeline: static (ctx, backend) => Immediate(ctx, mark =>
                    PipelineArm(backend.Frame, backend.Sprites, mark)),
                retained: static (ctx, backend) => Immediate(ctx, mark =>
                    RetainedArm(backend.Display, mark)),
                surface: static (ctx, backend) => Replayed(
                    ctx, backend.Target, backend.Policy, backend.Stock, backend.Clock),
                page: static (ctx, backend) => Replayed(
                    ctx, backend.Target, ScenePolicy.Fidelity, backend.Stock, backend.Clock)));
    }

    private static Fin<DrawTally> Immediate(
        Seq<DisplayMark> marks, Func<DisplayMark, Fin<Either<Error, Outcome>>> arm) =>
        marks.TraverseM(arm).As().Map(outcomes => new DrawTally(
            Drawn: Rasm.Numerics.Dimension.Create(value: outcomes.Count(static row => row is Either.Right<Error, Outcome>(Outcome.Drawn))),
            Culled: Rasm.Numerics.Dimension.Create(value: outcomes.Count(static row => row is Either.Right<Error, Outcome>(Outcome.Culled))),
            Refused: outcomes.Choose(static row => row.Match(Left: Some, Right: static _ => Option<Error>.None))));

    private static Fin<DrawTally> Replayed(
        Seq<DisplayMark> marks, Lease<Graphics> target, ScenePolicy policy, PaintStock stock, MonotonicTimeline clock) {
        Seq<Mark> screen = marks.Choose(static mark => mark is DisplayMark.Screen row ? Some(row.Value) : None);
        Seq<Error> refused = marks.Choose(mark => mark switch {
            DisplayMark.World row => Some((Error)new KernelFault.Unsupported(row.Value.GetType(), typeof(Graphics))),
            DisplayMark.Sprite => Some((Error)new KernelFault.Unsupported(typeof(SpriteMark), typeof(Graphics))),
            _ => Option<Error>.None,
        });
        return PaintProgram.Of(marks: screen)
            .Bind(program => program.Replay(target: target, policy: policy, stock: stock, clock: clock, lane: DispatchLane.Paced))
            .Map(tally => new DrawTally(Drawn: tally.Drawn, Culled: tally.Culled, Refused: refused));
    }

    // --- [PIPELINE_ARM]
    private static Fin<Either<Error, Outcome>> PipelineArm(ConduitFrame frame, SpriteSheet sprites, DisplayMark mark) => mark.Switch(
        (Frame: frame, Sprites: sprites),
        screen: static (ctx, row) => PipelineScope.With(
            ctx.Frame.Pipeline,
            [new RenderAspect.Screen()],
            () => ScreenProjection(ctx.Frame, row.Value)),
        world: static (ctx, row) => WorldPipeline(ctx.Frame, row.Value).Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        sprite: static (ctx, row) => SpritePipeline(ctx.Frame, ctx.Sprites, row.Value));

    private static Fin<Either<Error, Outcome>> ScreenProjection(ConduitFrame frame, Mark mark) => mark switch {
        Mark.StrokeCase row =>
            (from primitives in PathPrimitive.Lower(spec: row.Path)
             from stroke in Stroke.Of(
                 colour: row.Stroke.Colour,
                 width: row.Stroke.Width,
                 space: WidthSpace.Screen,
                 cap: StrokeCap.For(row.Stroke.Cap),
                 join: StrokeJoin.For(row.Stroke.Join),
                 dash: row.Stroke.Dash)
             from pen in stroke.Mint()
             from drawn in Try.lift(() => Fin.Succ(primitives.Iter(primitive => {
                 using global::Rhino.Geometry.Curve curve = primitive.Mint();
                 frame.Pipeline.DrawCurve(curve, pen);
             }))).Run().Bind(static inner => inner)
             select Either.Right<Error, Outcome>(Outcome.Drawn)),
        Mark.TextCase { Face.Source: TypeSource.FamilyCase family } row =>
            from ink in row.Ink.ToDrawing()
            from drawn in Try.lift(() => Fin.Succ(HostEdge.Side(() => frame.Pipeline.Draw2dText(
                row.Text,
                ink,
                new Point2d(row.At.X, row.At.Y),
                row.Block.Map(static block => block.Align == Eto.Drawing.FormattedTextAlignment.Center).IfNone(false),
                (int)row.Face.Size.Map(static size => size.Value).IfNone(12d),
                family.Family.Value)))).Run().Bind(static inner => inner)
            select Either.Right<Error, Outcome>(Outcome.Drawn),
        _ => Fin.Succ(Either.Left<Error, Outcome>(new KernelFault.Unsupported(mark.GetType(), typeof(DisplayPipeline)))),
    };

    private static Fin<Either<Error, Outcome>> SpritePipeline(ConduitFrame frame, SpriteSheet sprites, SpriteMark mark) =>
        sprites.Use(mark.Sprite, mark.Blend, bitmap => mark.Anchor.Switch(
            (Frame: frame, Bitmap: bitmap),
            screen: static (ctx, row) => Try.lift(() => Fin.Succ(HostEdge.Side(() =>
                ctx.Frame.Pipeline.DrawSprite(ctx.Bitmap, new Point2d(row.At.X, row.At.Y), row.Extent.Width, row.Extent.Height)))).Run().Bind(static inner => inner),
            world: static (ctx, row) =>
                from tint in row.Tint.Traverse(colour => colour.ToDrawing()).As()
                from drawn in Try.lift(() => Fin.Succ(HostEdge.Side(() => ctx.Frame.Pipeline.DrawSprite(
                    ctx.Bitmap, row.At, (float)row.Size.Value,
                    tint.IfNone(System.Drawing.Color.White),
                    row.Sizing == WidthSpace.World)))).Run().Bind(static inner => inner)
                select drawn,
            cloud: static (ctx, row) =>
                from inks in row.Colours.Traverse(colours => colours.Traverse(colour => colour.ToDrawing()).As()).As()
                from drawn in Try.lift(() => {
                    DisplayBitmapDrawList list = new();
                    _ = inks.Match(
                        Some: colours => HostEdge.Side(() => list.SetPoints(row.Points.AsEnumerable(), colours.AsEnumerable())),
                        None: () => HostEdge.Side(() => list.SetPoints(row.Points.AsEnumerable())));
                    ctx.Frame.Pipeline.DrawSprites(ctx.Bitmap, list, (float)row.Size.Value, row.Sizing == WidthSpace.World);
                    return Fin.Succ(unit);
                }).Run().Bind(static inner => inner)
                select drawn))
        .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn));

    private static Fin<Unit> WorldPipeline(ConduitFrame frame, WorldMark mark) => mark.Switch(
        frame,
        curve: static (ctx, row) => row.Stroke.Mint()
            .Bind(pen => Try.lift(() => ctx.Pipeline.DrawCurve(row.Value, pen)).Run().Bind(static inner => inner)),
        meshShaded: static (ctx, row) => row.Material.Use(
            material => Try.lift(() => ctx.Pipeline.DrawMeshShaded(row.Value, material)).Run().Bind(static inner => inner)),
        meshBanded: static (ctx, row) =>
            from ink in row.Colour.ToDrawing()
            from effect in row.Banding.Mint()
            from drawn in Try.lift(() => Fin.Succ(HostEdge.Side(() => ctx.Pipeline.DrawMeshShaded(row.Value, ink, effect)))).Run().Bind(static inner => inner)
            select drawn,
        meshFalseColors: static (ctx, row) => Try.lift(() => ctx.Pipeline.DrawMeshFalseColors(row.Value)).Run().Bind(static inner => inner),
        subDShaded: static (ctx, row) => row.Material.Use(
            material => Try.lift(() => ctx.Pipeline.DrawSubDShaded(row.Value, material)).Run().Bind(static inner => inner)),
        subDWires: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => ctx.Pipeline.DrawSubDWires(row.Value, ink, row.Width)).Run().Bind(static inner => inner)),
        brepShaded: static (ctx, row) => row.Material.Use(
            material => Try.lift(() => ctx.Pipeline.DrawBrepShaded(row.Value, material)).Run().Bind(static inner => inner)),
        brepWires: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => ctx.Pipeline.DrawBrepWires(row.Value, ink, row.Density)).Run().Bind(static inner => inner)),
        block: static (ctx, row) => row.Material.Use(
            material => Try.lift(() => ctx.Pipeline.DrawInstanceDefinitionShaded(row.Definition, material, row.Placement)).Run().Bind(static inner => inner)),
        clipping: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => ctx.Pipeline.DrawClippingPlaneWires(row.Value, ink)).Run().Bind(static inner => inner)),
        hatch: static (ctx, row) =>
            from lines in row.Lines.ToDrawing()
            from fill in row.Fill.ToDrawing()
            from drawn in Try.lift(() => ctx.Pipeline.DrawHatch(row.Value, lines, fill)).Run().Bind(static inner => inner)
            select drawn,
        text: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => ctx.Pipeline.DrawText(row.Value, ink)).Run().Bind(static inner => inner)),
        annotation: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => ctx.Pipeline.DrawAnnotation(row.Value, row.Owner, ink)).Run().Bind(static inner => inner)),
        arrowhead: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => ctx.Pipeline.DrawAnnotationArrowhead(row.Value, row.Placement, ink)).Run().Bind(static inner => inner)),
        direction: static (ctx, row) => Try.lift(() => ctx.Pipeline.DrawSurfaceDirectionIndicators(row.Value)).Run().Bind(static inner => inner),
        curvature: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => ctx.Pipeline.DrawCurvaturePreview(row.Value, ink)).Run().Bind(static inner => inner)),
        draft: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => ctx.Pipeline.DrawDraftAnglePreview(row.Value, ink)).Run().Bind(static inner => inner)),
        points: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => Fin.Succ(HostEdge.Side(() =>
                ctx.Pipeline.DrawPoints(row.Values.AsEnumerable(), row.Style.Native, row.Radius.Value, ink)))).Run().Bind(static inner => inner)),
        vector: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => Fin.Succ(HostEdge.Side(() => {
                ctx.Pipeline.DrawArrow(new Line(row.Anchor, row.Anchor + row.Span), ink);
                _ = HostEdge.SideWhen(row.Tip.Dot, () => ctx.Pipeline.DrawPoint(row.Anchor, ink));
            }))).Run().Bind(static inner => inner)),
        polygon: static (ctx, row) =>
            from fill in row.Fill.ToDrawing()
            from edge in row.Edge.ToDrawing()
            from drawn in Try.lift(() => Fin.Succ(HostEdge.Side(() => {
                _ = HostEdge.SideWhen(row.Paint.Fill, () => ctx.Pipeline.DrawPolygon(row.Ring.AsEnumerable(), fill, filled: true));
                _ = HostEdge.SideWhen(row.Paint.Edge, () => ctx.Pipeline.DrawPolygon(row.Ring.AsEnumerable(), edge, filled: false));
            }))).Run().Bind(static inner => inner)
            select drawn,
        label3d: static (ctx, row) => row.Colour.ToDrawing()
            .Bind(ink => Try.lift(() => Fin.Succ(HostEdge.Side(() => ctx.Pipeline.Draw3dText(row.Value, ink)))).Run().Bind(static inner => inner)));

    // --- [RETAINED_ARM]
    private static Fin<Either<Error, Outcome>> RetainedArm(CustomDisplay display, DisplayMark mark) => mark switch {
        DisplayMark.World { Value: WorldMark.Points row } =>
            row.Colour.ToDrawing().Bind(ink => Try.lift(() => Fin.Succ(HostEdge.Side(() =>
                display.AddPoints(row.Values.AsEnumerable(), ink, row.Style.Native, row.Radius.Value)))).Run().Bind(static inner => inner))
            .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        DisplayMark.World { Value: WorldMark.Vector row } =>
            row.Colour.ToDrawing().Bind(ink => Try.lift(() => Fin.Succ(HostEdge.Side(() =>
                display.AddVector(row.Anchor, row.Span, ink, row.Tip.Dot)))).Run().Bind(static inner => inner))
            .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        DisplayMark.World { Value: WorldMark.Polygon row } =>
            from fill in row.Fill.ToDrawing()
            from edge in row.Edge.ToDrawing()
            from drawn in Try.lift(() => Fin.Succ(HostEdge.Side(() =>
                display.AddPolygon(row.Ring.AsEnumerable(), fill, edge, row.Paint.Fill, row.Paint.Edge)))).Run().Bind(static inner => inner)
            select Either.Right<Error, Outcome>(Outcome.Drawn),
        DisplayMark.World { Value: WorldMark.Curve { Stroke.Decoration: var decoration } row }
            when decoration == PenDecoration.Bare =>
            row.Stroke.Colour.ToDrawing().Bind(ink => Try.lift(() => Fin.Succ(HostEdge.Side(() =>
                display.AddCurve(row.Value, ink, (int)Math.Max(1d, row.Stroke.Width.Value))))).Run().Bind(static inner => inner))
            .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        DisplayMark.World { Value: WorldMark.Label3d row } =>
            row.Colour.ToDrawing().Bind(ink => Try.lift(() => Fin.Succ(HostEdge.Side(() =>
                display.AddText(row.Value, ink)))).Run().Bind(static inner => inner))
            .Map(static _ => Either.Right<Error, Outcome>(Outcome.Drawn)),
        _ => Fin.Succ(Either.Left<Error, Outcome>(new KernelFault.Unsupported(mark.GetType(), typeof(CustomDisplay)))),
    };
}
```

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-display.md` — `DisplayPipeline` draw verbs, `DisplayPen`/`DisplayBitmap` surfaces); `Eto.Drawing` (`Rasm.Rhino/.api/api-eto-drawing.md` — `PenLineCap`/`PenLineJoin` aliased at the pen boundary); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` mark rows, `[ComplexValueObject]` pens); kernel `Interaction/paint` (`Marks` partition floor) + `Numerics/atoms` (`PerceptualColor`).

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
