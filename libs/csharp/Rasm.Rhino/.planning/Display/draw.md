# [RASM_RHINO_DISPLAY_DRAW]

`Marks` owns THE retained mark, path, stroke, and fill vocabulary for the package — one algebra over the Rhino display pipeline and Eto canvas. Stroked screen marks, labels, whole sprites, and bare groups render on both backends from the same retained geometry, and hit-testing is backend-free; fill styles, `Written` glyph blocks, windowed sprites, and posed or clipped groups are Eto-surface capabilities that fail the pipeline with typed backend evidence, world marks remain pipeline-only and fail the same way when routed to Eto — every capability edge is a typed refusal, never a silent partial draw. `Marks.Program` mounts this vocabulary into the Eto surface and print flow, which never name it.

`ConduitFrame` supplies the scoped pipeline, `PipelineScope.With` owns matched state stacks, and `PerceptualColor` remains the only color source. Native pens, brushes, and paths are paint-scoped; `SpriteSheet` lends cached bitmaps only inside draw callbacks and drains every borrower before disposal. System fonts arrive host-cached through the Eto `TypeRole`.

## [01]-[INDEX]

- [02]-[STYLE]: `Stroke`, `Dash`, `FillStyle`, `TextStyle`, and `Quant` own backend projection.
- [03]-[ASSETS]: `ScreenPath`, `Pose`, `SpriteSheet`, and `IsoBanding` own retained geometry, affine projection, and native resources.
- [04]-[MARKS]: `Marks.Render`, `Marks.Hit`, and `Marks.Program` close draw, hit-test, and seam-export modalities over one `Mark` union.

## [02]-[STYLE]

- Owner: style values carry backend-neutral width, color, cap, join, miter, dash, fill, and text policy.
- Entry: each native style mints inside the paint call and releases before egress.
- Law: color quantizes once through `Quant` — `Sys`, `Vec`, and `Eto` are its three backend projections and `Pigment` stays the perceptual-to-sRGB correspondence `Quant.Eto` composes, so a paint call reaching `Pigment` directly forks the one quantization; perceptual ramps resolve before the backend receives channel values.
- Law: shaded appearance is `ShadedMaterial`, never a host `DisplayMaterial` — the native is disposable and carries eight raw screen colours that bypass both the one colour source and the one quantization, so it mints inside `ShadedMaterial.Use`, serves exactly the draw call inside that bracket, and releases on every exit; the second face is `Option`, so a one-sided material spells no back band rather than mirroring the front.
- Law: typography composes the Eto owners — `TextStyle` carries the Eto `TypeRole` plus an optional point size, measurement and Eto text paint ride `GlyphBlock`, and the pipeline arm reads size and family facts off the role-resolved cached font; a font-role table or `FormattedText` shaping minted here is the deleted form.
- Law: one `Dash` family serves both backends — standard cases mint the host dash style and the interval table from one owner, `Patterned` carries caller intervals with offset bounded by the host's own eight-entry pen cap; `StrokePattern` stays the Rhino-pen pattern policy axis beside it.
- Growth: a style treatment is one vocabulary row, one `Dash` case, or one `FillStyle` case.
- Boundary: no Eto or `System.Drawing` color becomes domain state.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Rhino.Eto;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class StrokeCap {
    public static readonly StrokeCap Butt = new(0, PenLineCap.Butt, LineCapStyle.Flat);
    public static readonly StrokeCap Round = new(1, PenLineCap.Round, LineCapStyle.Round);
    public static readonly StrokeCap Square = new(2, PenLineCap.Square, LineCapStyle.Square);
    internal PenLineCap Eto { get; }
    internal LineCapStyle Rhino { get; }
}

[SmartEnum<int>]
public sealed partial class StrokeJoin {
    public static readonly StrokeJoin Round = new(0, PenLineJoin.Round, LineJoinStyle.Round);
    public static readonly StrokeJoin Miter = new(1, PenLineJoin.Miter, LineJoinStyle.Miter);
    public static readonly StrokeJoin Bevel = new(2, PenLineJoin.Bevel, LineJoinStyle.Bevel);
    internal PenLineJoin Eto { get; }
    internal LineJoinStyle Rhino { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Dash {
    private Dash() { }
    public sealed record Solid : Dash;
    public sealed record Dashed : Dash;
    public sealed record Dotted : Dash;
    public sealed record DashDotted : Dash;
    public sealed record DashDotDotted : Dash;
    public sealed record Patterned(float Offset, Seq<float> Intervals) : Dash;

    // Host truth: `DisplayPen.SetPattern` accepts at most eight dash/gap entries, so a longer pattern refuses at admission
    // rather than truncating or throwing inside the paint call.
    internal const int MaxIntervals = 8;

    internal bool Valid => this is not Patterned row
        || (!row.Intervals.IsEmpty
            && row.Intervals.Count <= MaxIntervals
            && float.IsFinite(row.Offset)
            && row.Intervals.ForAll(static gap => float.IsFinite(gap) && gap > 0f));

    internal Seq<float> Pattern() => Switch(
        solid: static _ => Seq<float>(),
        dashed: static _ => [3f, 1f],
        dotted: static _ => [1f, 1f],
        dashDotted: static _ => [3f, 1f, 1f, 1f],
        dashDotDotted: static _ => [3f, 1f, 1f, 1f, 1f, 1f],
        patterned: static row => row.Intervals);

    internal DashStyle Eto() => Switch(
        solid: static _ => DashStyles.Solid,
        dashed: static _ => DashStyles.Dash,
        dotted: static _ => DashStyles.Dot,
        dashDotted: static _ => DashStyles.DashDot,
        dashDotDotted: static _ => DashStyles.DashDotDot,
        patterned: static row => new DashStyle(row.Offset, [.. row.Intervals]));
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FillStyle {
    private FillStyle() { }
    public sealed record Solid(PerceptualColor Color) : FillStyle;
    public sealed record Linear(PerceptualColor Start, PerceptualColor End, Point2d From, Point2d To) : FillStyle;
    public sealed record Radial(PerceptualColor Start, PerceptualColor End, Point2d Center, Point2d Origin, Size2f Radius) : FillStyle;
    public sealed record Texture(SpriteRef Sprite, float Opacity) : FillStyle;

    internal bool Valid => Switch(
        solid: static _ => true,
        linear: static row => Quant.Finite(row.From) && Quant.Finite(row.To),
        radial: static row => Quant.Finite(row.Center) && Quant.Finite(row.Origin) && Quant.Positive(row.Radius),
        texture: static row => row.Sprite is not null && row.Opacity is >= 0f and <= 1f && float.IsFinite(row.Opacity));
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct Size2f(float Width, float Height);

[ComplexValueObject]
public sealed partial class StrokePattern {
    public bool Autoscale { get; }
    public float Scale { get; }
    public float Offset { get; }
    public bool BySegment { get; }
    public bool WorldLength { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref bool autoscale,
        ref float scale,
        ref float offset,
        ref bool bySegment,
        ref bool worldLength) =>
        validationError = float.IsFinite(scale) && scale > 0f && float.IsFinite(offset)
            ? null
            : new ValidationError(message: "Stroke pattern is not finite and positive.");
}

public sealed record Stroke {
    private Stroke(
        double width,
        PerceptualColor color,
        StrokeCap cap,
        StrokeJoin join,
        Dash dash,
        Option<(PerceptualColor Color, double Width)> halo,
        Option<(double Start, double End, Point2d At)> taper,
        bool worldWidth,
        StrokePattern pattern,
        float miter) =>
        (Width, Color, Cap, Join, Dash, Halo, Taper, WorldWidth, Pattern, Miter) =
        (width, color, cap, join, dash, halo, taper, worldWidth, pattern, miter);

    public double Width { get; }
    public PerceptualColor Color { get; }
    public StrokeCap Cap { get; }
    public StrokeJoin Join { get; }
    public Dash Dash { get; }
    public Option<(PerceptualColor Color, double Width)> Halo { get; }
    public Option<(double Start, double End, Point2d At)> Taper { get; }
    public bool WorldWidth { get; }
    public StrokePattern Pattern { get; }
    public float Miter { get; }

    public static Fin<Stroke> Of(
        double width,
        PerceptualColor color,
        StrokeCap cap,
        StrokeJoin join,
        Dash dash,
        Option<(PerceptualColor Color, double Width)> halo,
        Option<(double Start, double End, Point2d At)> taper,
        bool worldWidth,
        StrokePattern pattern,
        float miter = 10f,
        Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(cap is not null && join is not null && dash is not null && dash.Valid && pattern is not null
                   && float.IsFinite(miter) && miter >= 1f
                   && taper.Match(Some: static row => Quant.Finite(row.At), None: static () => true), op.InvalidInput()).ToFin()
               from admitted in op.Positive(width)
               from bounds in (halo.ToSeq().Map(static row => row.Width) + taper.ToSeq().Bind(static row => Seq(row.Start, row.End)))
                   .TraverseM(op.Positive).As()
               select new Stroke(admitted, color, cap, join, dash, halo, taper, worldWidth, pattern, miter);
    }

    internal Pen Eto() => new(Quant.Eto(Color), (float)Width) {
        LineCap = Cap.Eto,
        LineJoin = Join.Eto,
        MiterLimit = Miter,
        DashStyle = Dash.Eto(),
    };

    internal DisplayPen Rhino() {
        DisplayPen pen = new() {
            Color = Quant.Sys(Color),
            Thickness = (float)Width,
            ThicknessSpace = WorldWidth ? CoordinateSystem.World : CoordinateSystem.Screen,
            CapStyle = Cap.Rhino,
            JoinStyle = Join.Rhino,
        };
        _ = Halo.Iter(row => (pen.HaloColor, pen.HaloThickness) = (Quant.Sys(row.Color), (float)row.Width));
        _ = Taper.Iter(row => pen.SetTaper((float)row.Start, (float)row.End, Quant.Pt2(row.At)));
        Seq<float> gaps = Dash.Pattern();
        _ = Op.SideWhen(!gaps.IsEmpty, () => {
            pen.SetPattern(gaps.Map(gap => gap * (float)Width).AsEnumerable());
            (pen.PatternAutoscale, pen.PatternScale, pen.PatternOffset, pen.PatternBySegment, pen.PatternLengthInWorldUnits) =
                (Pattern.Autoscale, Pattern.Scale, Pattern.Offset, Pattern.BySegment, Pattern.WorldLength);
        });
        return pen;
    }

    internal double CullOutset {
        get {
            double stroke = Width * (Join == StrokeJoin.Miter ? Miter : 1f) / 2d;
            return Halo.Map(row => double.Max(stroke, row.Width / 2d)).IfNone(stroke);
        }
    }
}

// `DisplayMaterial` is a disposable host native carrying eight raw `System.Drawing.Color` channels, so it is the one shaded
// appearance vocabulary the slice cannot publish: `ShadedFace` states one side in `PerceptualColor` under `ShineAxis.Unit`,
// `ShadedMaterial` pairs the two sides, and the native mints and dies inside `Use`.
[ComplexValueObject]
public sealed partial class ShadedFace {
    public PerceptualColor Diffuse { get; }
    public PerceptualColor Specular { get; }
    public PerceptualColor Ambient { get; }
    public PerceptualColor Emission { get; }
    public double Shine { get; }
    public double Transparency { get; }

    [BoundaryAdapter]
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
}

public sealed record ShadedMaterial(ShadedFace Front, Option<ShadedFace> Back) {
    internal bool Valid => Front is not null && Back.Match(Some: static face => face is not null, None: static () => true);

    internal Fin<TResult> Use<TResult>(Func<DisplayMaterial, Fin<TResult>> project, Op key) {
        ShadedMaterial self = this;
        return key.Catch(() => {
            using DisplayMaterial native = new(
                diffuse: Quant.Sys(self.Front.Diffuse),
                specular: Quant.Sys(self.Front.Specular),
                ambient: Quant.Sys(self.Front.Ambient),
                emission: Quant.Sys(self.Front.Emission),
                shine: self.Front.Shine,
                transparency: self.Front.Transparency);
            _ = self.Back.Iter(face => {
                native.IsTwoSided = true;
                (native.BackDiffuse, native.BackSpecular) = (Quant.Sys(face.Diffuse), Quant.Sys(face.Specular));
                (native.BackAmbient, native.BackEmission) = (Quant.Sys(face.Ambient), Quant.Sys(face.Emission));
                (native.BackShine, native.BackTransparency) = (face.Shine, face.Transparency);
            });
            return project(native);
        });
    }
}

public sealed record TextStyle {
    private TextStyle(TypeRole role, Option<float> size) => (Role, Size) = (role, size);

    public TypeRole Role { get; }
    public Option<float> Size { get; }

    public static Fin<TextStyle> Of(TypeRole role, Option<float> size = default, Op? key = null) {
        Op op = key.OrDefault();
        return guard(role is not null, op.InvalidInput()).ToFin()
            .Bind(_ => size.Match(
                Some: points => op.Positive(points).Map(value => new TextStyle(role, Some(value))),
                None: () => Fin.Succ(new TextStyle(role, None))));
    }

    // Eto TypeRole resolves the host-cached SystemFonts instance; the shared font is never disposed here.
    internal TResult Use<TResult>(Func<global::Eto.Drawing.Font, TResult> project) =>
        project(Role.Resolve(size: Size.ToNullable()));

    // Metrics are the Eto owner's and so is their affinity: shaping reaches the platform text stack, so a measurement
    // answers on the rail here exactly as it does there rather than being flattened into a bare extent.
    internal Fin<Size2f> Measure(string text, Op? key = null) =>
        new GlyphBlock(text, Role, size: Size).Measure(key)
            .Map(static measured => new Size2f(measured.Width, measured.Height));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class Quant {
    internal static bool Finite(Point2d point) => double.IsFinite(point.X) && double.IsFinite(point.Y);

    internal static bool Finite(Point3d point) => point.IsValid;

    internal static bool Finite(Vector3d vector) => vector.IsValid;

    internal static bool Positive(Size2f extent) =>
        float.IsFinite(extent.Width) && extent.Width > 0f && float.IsFinite(extent.Height) && extent.Height > 0f;

    internal static System.Drawing.Color Sys(PerceptualColor color) =>
        color.ToRgb() switch {
            var (red, green, blue, alpha) => System.Drawing.Color.FromArgb(alpha, red, green, blue),
        };

    internal static Color4f Vec(PerceptualColor color) =>
        color.ToRgb(RgbProfile.Srgb) switch {
            var (red, green, blue, alpha) => new Color4f((float)red, (float)green, (float)blue, (float)alpha),
        };

    // The Eto projection sits beside `Sys` and `Vec` so all three backends reach ONE quantizer; `Pigment` stays the
    // perceptual-to-sRGB correspondence owner this member composes rather than a second entry paint calls reach past it.
    internal static global::Eto.Drawing.Color Eto(PerceptualColor color) => Pigment.ToColor(color);

    internal static PointF Eto(Point2d point) => new((float)point.X, (float)point.Y);
    internal static Point2f Pt2(Point2d point) => new((float)point.X, (float)point.Y);
}
```

## [03]-[ASSETS]

- Owner: `ScreenPath` builds and hit-tests one retained path over the kernel-lowered primitive run it stores at admission; `Pose` brackets affine projection through host matrices; `SpriteSheet` owns both native bitmap caches; `IsoBanding` owns banded-shading data.
- Law: sprite bytes or assets admitted by `ISpriteFiles` enter once through `SpriteRef.Of` with a stable key; raw paths never cross the asset boundary.
- Law: path paint and hit-testing use the same `GraphicsPath`; sprite caches key source identity and blend policy together.
- Law: `Pose.Use` brackets every materialized `IMatrix` — `Stacked` appends each bracketed child onto one owned identity matrix, `Inverted` disposes its source after `Matrix.Inverse`, and no matrix escapes its projection.
- Law: a composite screen segment is DERIVED ONCE and the derivation is the KERNEL's. `PathSegment.Lower` folds a rounded rectangle through `ParametricOp.RoundedRectangle` and a spline through `ParametricOp.CardinalSpline`, and maps the returned `ParametricResult.Outline` run onto `Line`/`Arc`/`Bezier`; the corner walk, the radius clamp, and the `tension / 3` neighbour offsets live in `Rasm.Parametric` alone, so this page carries no arithmetic a NURBS emission could disagree with. A primitive lowers to itself, so the fold is total and a zero radius drops its arc.
- Law: the lowering runs ONCE, at `ScreenPath.Of`, and the resolved run rides the value as `Run`. A path paints and hit-tests from that stored run, so the kernel refusal folds onto the one admission rail every `ScreenPath` already crosses and neither backend re-derives per frame; `Segments` stays the authored request shape and `Run` the drawn truth.
- Boundary: the seam hands the kernel the screen rectangle's own intervals on `Plane.WorldXY`, so frame-local coordinates ARE screen coordinates and the kernel's `Center + R·(cos θ, sin θ)` parameterization is the backend's own; the whole conversion is `ToDegrees` on `Start` and the signed `Angle`, and any sign correction here would mirror the arcs off the corner points the same kernel computed.
- Law: every screen arc stays in world XY while its radial X axis preserves the Eto start angle; the tangent Y axis completes the in-plane basis — the corner walk emits ordinary `Arc` segments, so it inherits that one settled angle mapping rather than minting a second.
- Boundary: cache disposal closes admission, drains every draw-scoped use, releases each native bitmap once, and clears both tables.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathSegment {
    private PathSegment() { }
    public sealed record Line(Point2d From, Point2d To) : PathSegment;
    public sealed record Arc(Point2d Center, float Radius, float Start, float Sweep) : PathSegment;
    public sealed record Bezier(Point2d Start, Point2d Control1, Point2d Control2, Point2d End) : PathSegment;
    public sealed record Spline(Seq<Point2d> Points, float Tension) : PathSegment;
    public sealed record Ellipse(Point2d Origin, Size2f Extent) : PathSegment;
    public sealed record Rectangle(Point2d Origin, Size2f Extent) : PathSegment;
    public sealed record RoundRectangle(Point2d Origin, Size2f Extent, float NW, float NE, float SE, float SW) : PathSegment;

    internal bool Valid => Switch(
        line: static row => Quant.Finite(row.From) && Quant.Finite(row.To),
        arc: static row => Quant.Finite(row.Center) && float.IsFinite(row.Radius) && row.Radius > 0f && float.IsFinite(row.Start) && float.IsFinite(row.Sweep),
        bezier: static row => Quant.Finite(row.Start) && Quant.Finite(row.Control1) && Quant.Finite(row.Control2) && Quant.Finite(row.End),
        spline: static row => row.Points.Count >= 4 && float.IsFinite(row.Tension) && row.Points.ForAll(Quant.Finite),
        ellipse: static row => Quant.Finite(row.Origin) && Quant.Positive(row.Extent),
        rectangle: static row => Quant.Finite(row.Origin) && Quant.Positive(row.Extent),
        roundRectangle: static row => Quant.Finite(row.Origin) && Quant.Positive(row.Extent)
            && ValidRadii(row.Extent, row.NW, row.NE, row.SE, row.SW));

    private static bool ValidRadii(Size2f extent, params ReadOnlySpan<float> radii) {
        float cap = float.Min(extent.Width, extent.Height) / 2f;
        return Iterable<float>.FromSpan(radii).ForAll(radius => float.IsFinite(radius) && radius is >= 0f && radius <= cap);
    }

    // The one lowering, and it is the KERNEL's: a composite segment becomes a `ParametricOp` whose `Outline` run
    // this seam maps back onto primitives. Frame-local coordinates ARE screen coordinates because the frame is
    // `Plane.WorldXY` over the screen rectangle's own intervals, so the mapping is coordinates 1:1 and `ToDegrees` on
    // the two angles. A primitive lowers to itself, so the fold is total and only the two composite arms can refuse.
    internal Fin<Seq<PathSegment>> Lower(Op key) => Switch(
        key,
        line: static (_, row) => Fin.Succ(Seq<PathSegment>(row)),
        arc: static (_, row) => Fin.Succ(Seq<PathSegment>(row)),
        bezier: static (_, row) => Fin.Succ(Seq<PathSegment>(row)),
        ellipse: static (_, row) => Fin.Succ(Seq<PathSegment>(row)),
        spline: static (op, row) => Outlined(new ParametricOp.CardinalSpline(
            Frame: Plane.WorldXY,
            Points: new Arr<Point2d>([.. row.Points]),
            Tension: row.Tension,
            Closed: false), op),
        rectangle: static (op, row) => Cornered(row.Origin, row.Extent, 0f, 0f, 0f, 0f, op),
        roundRectangle: static (op, row) => Cornered(row.Origin, row.Extent, row.NW, row.NE, row.SE, row.SW, op));

    internal Unit Add(GraphicsPath path) => Switch(
        path,
        line: static (target, row) => Op.Side(() => target.AddLine((float)row.From.X, (float)row.From.Y, (float)row.To.X, (float)row.To.Y)),
        arc: static (target, row) => Op.Side(() => target.AddArc((float)row.Center.X - row.Radius, (float)row.Center.Y - row.Radius, row.Radius * 2f, row.Radius * 2f, row.Start, row.Sweep)),
        bezier: static (target, row) => Op.Side(() => target.AddBezier(Quant.Eto(row.Start), Quant.Eto(row.Control1), Quant.Eto(row.Control2), Quant.Eto(row.End))),
        ellipse: static (target, row) => Op.Side(() => target.AddEllipse((float)row.Origin.X, (float)row.Origin.Y, row.Extent.Width, row.Extent.Height)),
        // Unreachable: `ScreenPath.Of` stores the lowered run and both backends draw from it, so no composite arm
        // reaches a backend at all.
        spline: static (_, _) => unit,
        rectangle: static (_, _) => unit,
        roundRectangle: static (_, _) => unit);

    // Composite arms answer NONE, the same shape the `Add` arms take: `ScreenPath.Of` stores the lowered run and both
    // backends draw from it, so a composite segment reaches no backend and has no curve to answer. Absence spells that
    // directly and contributes nothing to the curve run, where a throw would spell an unreachable state as a crash.
    internal Option<Curve> Rhino() => Switch(
        line: static row => Some((Curve)new LineCurve(new Point3d(row.From.X, row.From.Y, 0.0), new Point3d(row.To.X, row.To.Y, 0.0))),
        arc: static row => Some<Curve>(new ArcCurve(new global::Rhino.Geometry.Arc(
            new Plane(
                new Point3d(row.Center.X, row.Center.Y, 0.0),
                new Vector3d(Math.Cos(RhinoMath.ToRadians(row.Start)), Math.Sin(RhinoMath.ToRadians(row.Start)), 0.0),
                new Vector3d(-Math.Sin(RhinoMath.ToRadians(row.Start)), Math.Cos(RhinoMath.ToRadians(row.Start)), 0.0)),
            row.Radius,
            RhinoMath.ToRadians(row.Sweep)))),
        bezier: static row => Some<Curve>(new BezierCurve([new(row.Start.X, row.Start.Y, 0.0), new(row.Control1.X, row.Control1.Y, 0.0), new(row.Control2.X, row.Control2.Y, 0.0), new(row.End.X, row.End.Y, 0.0)]).ToNurbsCurve()),
        ellipse: static row => Some<Curve>(new global::Rhino.Geometry.Ellipse(
            new Plane(new Point3d(row.Origin.X + (row.Extent.Width / 2.0), row.Origin.Y + (row.Extent.Height / 2.0), 0.0), Vector3d.ZAxis),
            row.Extent.Width / 2.0,
            row.Extent.Height / 2.0).ToNurbsCurve()),
        spline: static _ => Option<Curve>.None,
        rectangle: static _ => Option<Curve>.None,
        roundRectangle: static _ => Option<Curve>.None);

    private static Fin<Seq<PathSegment>> Cornered(
        Point2d origin, Size2f extent, float nw, float ne, float se, float sw, Op key) =>
        Outlined(new ParametricOp.RoundedRectangle(
            Frame: Plane.WorldXY,
            X: new Interval(origin.X, origin.X + extent.Width),
            Y: new Interval(origin.Y, origin.Y + extent.Height),
            NW: nw, NE: ne, SE: se, SW: sw), key);

    // The one seam: the kernel answers frame-local primitives in radians, this page draws screen primitives in
    // degrees, and nothing else crosses. An `Outline` is the only result these two ops produce, so any other case is
    // a kernel contract break rather than a shape this page can render.
    private static Fin<Seq<PathSegment>> Outlined(ParametricOp op, Op key) =>
        Parametric.Apply(op, key).Bind(result => result is ParametricResult.Outline outline
            ? Fin.Succ(toSeq(outline.Run).Map(Screen).Strict())
            : Fin.Fail<Seq<PathSegment>>(key.InvalidResult(detail: result.GetType().Name)));

    private static PathSegment Screen(PlanarPrimitive primitive) => primitive.Switch(
        segment: static row => (PathSegment)new Line(row.From, row.To),
        sweep: static row => new Arc(
            Center: row.Center,
            Radius: (float)row.Radius,
            Start: (float)RhinoMath.ToDegrees(row.Start),
            Sweep: (float)RhinoMath.ToDegrees(row.Angle)),
        cubic: static row => new Bezier(row.Start, row.Control1, row.Control2, row.End));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Pose {
    private Pose() { }
    public sealed record Shift(float X, float Y) : Pose;
    public sealed record Turn(float Angle) : Pose;
    public sealed record Grow(float X, float Y, Point2d About) : Pose;
    public sealed record Explicit(float XX, float YX, float XY, float YY, float X0, float Y0) : Pose;
    public sealed record Stacked(Seq<Pose> Poses) : Pose;
    public sealed record Inverted(Pose Body) : Pose;

    internal bool Valid => Switch(
        shift: static row => float.IsFinite(row.X) && float.IsFinite(row.Y),
        turn: static row => float.IsFinite(row.Angle),
        grow: static row => float.IsFinite(row.X)
            && row.X != 0f
            && float.IsFinite(row.Y)
            && row.Y != 0f
            && Quant.Finite(row.About),
        @explicit: static row => Seq(row.XX, row.YX, row.XY, row.YY, row.X0, row.Y0).ForAll(float.IsFinite)
            && (((double)row.XX * row.YY) - ((double)row.XY * row.YX)) is var determinant
            && double.IsFinite(determinant)
            && determinant != 0d,
        stacked: static row => row.Poses.ForAll(static pose => pose is not null && pose.Valid),
        inverted: static row => row.Body is not null && row.Body.Valid);

    internal TResult Use<TResult>(Func<IMatrix, TResult> project) {
        using IMatrix matrix = Mint();
        return project(matrix);
    }

    internal Point2d Unproject(Point2d point) => Use(matrix => {
        using IMatrix inverse = Matrix.Inverse(matrix);
        PointF local = inverse.TransformPoint(Quant.Eto(point));
        return new Point2d(local.X, local.Y);
    });

    private IMatrix Mint() => Switch(
        shift: static row => Matrix.FromTranslation(row.X, row.Y),
        turn: static row => Matrix.FromRotation(row.Angle),
        grow: static row => Matrix.FromScaleAt(row.X, row.Y, (float)row.About.X, (float)row.About.Y),
        @explicit: static row => Matrix.Create(row.XX, row.YX, row.XY, row.YY, row.X0, row.Y0),
        stacked: static row => Folded(row.Poses),
        inverted: static row => row.Body.Use(Matrix.Inverse));

    private static IMatrix Folded(Seq<Pose> poses) {
        IMatrix folded = Matrix.Create();
        try {
            _ = poses.Iter(pose => pose.Use(matrix => Op.Side(() => folded.Append(matrix))));
            return folded;
        } catch {
            folded.Dispose();
            throw;
        }
    }
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

    public static Fin<SpriteRef> Of(SpriteSource source, Op? key = null) {
        Op op = key.OrDefault();
        return guard(source is not null && source.Valid, op.InvalidInput()).ToFin()
            .Bind(_ => source.Read(op))
            .Bind(content => {
                ReadOnlyMemory<byte> owned = content.ToArray();
                return guard(!owned.IsEmpty, op.InvalidInput()).ToFin().Map(_ => new SpriteRef(
                    ContentHash.Of(owned.Span).ToString("X32", System.Globalization.CultureInfo.InvariantCulture),
                    owned));
            });
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ScreenPath {
    private ScreenPath(Seq<PathSegment> segments, Seq<PathSegment> run, bool closed) =>
        (Segments, Run, Closed) = (segments, run, closed);

    // The authored request shape; `Run` is the kernel-lowered primitive run both backends actually draw.
    public Seq<PathSegment> Segments { get; }
    public Seq<PathSegment> Run { get; }
    public bool Closed { get; }

    // A `ScreenPath` exists only through `Of`, so validity is the stored evidence: an admitted request shape AND a
    // non-empty lowered run, which is what the consuming `Mark` and `Shape` validity folds read.
    internal bool Valid => ValidSegments(Segments) && !Run.IsEmpty;

    // Admission lowers once: the composite arithmetic is the kernel's, its refusal folds onto this rail, and neither
    // backend re-derives a corner walk or a spline span per paint or per hit test.
    public static Fin<ScreenPath> Of(Seq<PathSegment> segments, bool closed, Op? key = null) {
        Op op = key.OrDefault();
        return guard(ValidSegments(segments), op.InvalidInput()).ToFin()
            .Bind(_ => segments.TraverseM(segment => segment.Lower(op)).As())
            .Map(lowered => new ScreenPath(segments, lowered.Bind(static run => run).Strict(), closed));
    }

    private static bool ValidSegments(Seq<PathSegment> segments) =>
        !segments.IsEmpty && segments.ForAll(static segment => segment is not null && segment.Valid);

    internal GraphicsPath Eto() {
        GraphicsPath path = new();
        _ = Run.Iter(segment => segment.Add(path));
        _ = Op.SideWhen(Closed, path.CloseFigure);
        return path;
    }

    // The run materializes INSIDE the try, so every curve the fold minted reaches the disposal even when a later
    // segment throws; a composite arm contributes nothing, exactly as it contributes nothing to the Eto path.
    internal Fin<TResult> UseCurves<TResult>(Func<Seq<Curve>, Fin<TResult>> use, Op key) => key.Catch(() => {
        Seq<Curve> curves = Seq<Curve>();
        try {
            curves = Run.Choose(static segment => segment.Rhino()).Strict();
            return use(curves);
        }
        finally { _ = curves.Iter(static curve => curve.Dispose()); }
    });

    internal bool Hit(Point2d point, Option<Stroke> stroke, bool filled) {
        using GraphicsPath path = Eto();
        return filled && Closed && path.FillContains(Quant.Eto(point))
            || stroke.Match(
                Some: value => { using Pen pen = value.Eto(); return path.StrokeContains(pen, Quant.Eto(point)); },
                None: static () => false);
    }
}

public sealed record IsoBanding(
    IsoMode Mode,
    Vector3d Direction,
    Point3d Anchor,
    int Frequency,
    double Rotation,
    double Falloff,
    Option<(PerceptualColor Color, double Size, bool Discard)> Gap,
    PerceptualColor From,
    PerceptualColor To,
    Dimension Bands) {
    internal bool Valid => Mode is not null
        && Quant.Finite(Direction)
        && Quant.Finite(Anchor)
        && Frequency > 0
        && double.IsFinite(Rotation)
        && double.IsFinite(Falloff)
        && Bands.Value is > 0 and <= 10
        && Gap.Match(Some: static row => row.Size >= 0.0 && double.IsFinite(row.Size), None: static () => true);

    internal Fin<IsoDrawEffect> Mint(Op key) =>
        from _ in guard(Valid, key.InvalidInput()).ToFin()
        from colors in Fin.Succ(From.Ramp(To, Bands))
        from effect in key.Catch(() => {
            IsoDrawEffect value = new() { DrawMode = Mode.Native, Direction = Direction, Point = Anchor, Frequency = Frequency, RotationRadians = Rotation, Falloff = Falloff, UsedBandColorCount = Bands.Value };
            _ = Gap.Iter(row => (value.GapColor, value.GapSize, value.DiscardGap) = (Quant.Sys(row.Color), row.Size, row.Discard));
            _ = colors.Map(static (color, index) => (color, index)).Iter(row => ignore(value.SetBandColor(row.index, Quant.Sys(row.color))));
            return Fin.Succ(value);
        })
        select effect;
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class SpriteSheet : IDisposable {
    private readonly System.Collections.Concurrent.ConcurrentDictionary<(string Key, BlendMode Src, BlendMode Dst), Lazy<DisplayBitmap>> pipeline = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<Bitmap>> surface = new(StringComparer.Ordinal);
    private readonly object gate = new();
    private int active;
    private int released;

    internal Fin<TResult> Pipeline<TResult>(
        SpriteRef sprite,
        BlendUse source,
        BlendUse destination,
        Func<DisplayBitmap, Fin<TResult>> use,
        Op key) =>
        Use(pipeline, (sprite.Key, source.Native, destination.Native), () => LoadPipeline(sprite, source.Native, destination.Native), use, key);

    internal Fin<TResult> Surface<TResult>(SpriteRef sprite, Func<Bitmap, Fin<TResult>> use, Op key) =>
        Use(surface, sprite.Key, () => LoadSurface(sprite), use, key);

    private Fin<TResult> Use<TKey, TSheet, TResult>(
        System.Collections.Concurrent.ConcurrentDictionary<TKey, Lazy<TSheet>> table,
        TKey slot,
        Func<TSheet> load,
        Func<TSheet, Fin<TResult>> use,
        Op key)
        where TKey : notnull {
        Fin<TSheet> admitted;
        lock (gate) {
            admitted = guard(released == 0, key.InvalidContext()).ToFin().Bind(_ => {
                Lazy<TSheet> cached = table.GetOrAdd(
                    slot, _ => new Lazy<TSheet>(load, LazyThreadSafetyMode.ExecutionAndPublication));
                return key.Catch(() => Fin.Succ(cached.Value)).BindFail(failure => (
                    table.TryRemove(new KeyValuePair<TKey, Lazy<TSheet>>(slot, cached)),
                    Fin.Fail<TSheet>(failure)).Item2);
            });
            if (admitted.IsSucc) { active++; }
        }
        return admitted.Bind(resource => {
            try { return key.Catch(() => use(resource)); }
            finally { Settle(); }
        });
    }

    private void Settle() {
        lock (gate) {
            active--;
            if (active == 0) { Monitor.PulseAll(gate); }
        }
    }

    private static DisplayBitmap LoadPipeline(SpriteRef sprite, BlendMode source, BlendMode destination) {
        using System.IO.MemoryStream stream = new(sprite.Content.ToArray());
        using System.Drawing.Bitmap encoded = new(stream);
        using System.Drawing.Bitmap bitmap = new(encoded);
        DisplayBitmap loaded = new(bitmap);
        try {
            loaded.SetBlendFunction(source, destination);
            return loaded;
        }
        catch {
            loaded.Dispose();
            throw;
        }
    }

    private static Bitmap LoadSurface(SpriteRef sprite) {
        using System.IO.MemoryStream stream = new(sprite.Content.ToArray());
        using Bitmap encoded = new(stream);
        return encoded.Clone();
    }

    public void Dispose() {
        lock (gate) {
            if (released != 0) { return; }
            released = 1;
            while (active != 0) { Monitor.Wait(gate); }
            _ = toSeq(pipeline.Values).Filter(static bitmap => bitmap.IsValueCreated).Iter(static bitmap => bitmap.Value.Dispose());
            _ = toSeq(surface.Values).Filter(static bitmap => bitmap.IsValueCreated).Iter(static bitmap => bitmap.Value.Dispose());
            pipeline.Clear();
            surface.Clear();
        }
    }
}
```

## [04]-[MARKS]

- Owner: `Mark` partitions screen and world payloads by backend capability while preserving one public concept; this union is THE package draw vocabulary, and the Eto surface consumes it only through `Marks.Program`.
- Entry: `Marks.Render` draws one mark batch onto a `Canvas` backend and returns the drawn count; `Marks.Hit` answers hit ordinals with no backend receiver; `Marks.Program` mints the Eto `PaintProgram`, so surface mounting and print flow reuse this vocabulary without naming it.
- Law: render order is the input order; the first failed draw aborts and preserves its typed fault.
- Law: the pipeline screen arm is stroke-shaped — a `Path` fill, a `Written` block, a windowed `Sprite`, and a posed or clipped `Group` are Eto-surface capabilities and fail the pipeline with typed backend evidence before any partial draw.
- Law: hit-testing returns source ordinals, so overlapping marks retain z-order evidence; topmost is the last ordinal. Geometry answers from the mark alone, but a text mark's extent is HOST metrics, so the predicate rides the Eto owner's measurement rail and a refused measure is a typed fault rather than a silent miss.
- Law: an arrowhead is a HOST primitive, never a re-derived triangle — `WorldMark.Arrowhead` folds `DisplayPipeline.DrawAnnotationArrowhead(Arrowhead, Transform, Color)` so head shape rides the `DimensionStyle.ArrowType` row or the user-block `BlockId` the `Arrowhead` carries, and scale, orientation, and position ride the one `Transform`; a dimension overlay, a direction grip, and a section-cut leader all draw through this case rather than each hand-rolling a path in `ScreenMark.Path`.
- Law: `ScreenPath` is the sole retained screen geometry carrier; `ScreenMark.Path` applies stroke and fill once across paint, clip culling, and hit-testing.
- Law: mark admission folds every nested path, pose, clip, style presence, coordinate, and extent before render or hit-test dispatch; no invalid child or non-finite screen scalar reaches a backend.
- Law: `Group` poses, clips, and sequences children on the Eto backend; a posed or clipped group routed to the pipeline fails with typed backend evidence, while a bare group sequences on both backends.
- Law: `Label` is the dual-backend plain-text mark; `Written` carries a full `GlyphBlock` (wrap, alignment, trimming, max extent) and is Eto-only, failing the pipeline with the same typed evidence.
- Law: a `Label` shapes ONCE, at admission, and holds the measurement on the rail — hit-testing walks every mark per pointer pixel and paint would shape again, so both read the one held verdict and centring can never disagree with the hit rectangle; the shaping therefore happens where the mark is built, which is the seam that draws it.
- Boundary: world marks routed to Eto or to hit-testing fail before any partial draw.
- Growth: a drawable is one inner-union case and one backend arm; callers and the public entry remain unchanged.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Canvas {
    private Canvas() { }
    public sealed record Pipeline(ConduitFrame Frame) : Canvas;
    public sealed record Surface(Graphics Graphics) : Canvas;

    internal bool Valid => Switch(
        pipeline: static row => row.Frame.Pipeline is not null,
        surface: static row => row.Graphics is not null);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScreenMark {
    private ScreenMark() { }
    public sealed record Path(ScreenPath Value, Option<Stroke> Stroke, Option<FillStyle> Fill) : ScreenMark;
    public sealed record Label(string Text, Point2d At, TextStyle Style, PerceptualColor Color, bool Centered) : ScreenMark {
        // Shaped ONCE, at admission, and HELD on the rail: a pointer move hit-tests every mark in a frame, so a per-probe
        // measure re-shapes the whole label set on every mouse pixel and paint re-shapes it again. The verdict stays a
        // `Fin` because host metrics refuse, so one shaping answers both the hit rectangle and the centring offset — a
        // malformed label carries the refusal its `Valid` row already spells rather than a fabricated zero extent.
        internal Fin<Size2f> Extent { get; } = Style is null || string.IsNullOrEmpty(Text)
            ? Fin.Fail<Size2f>(Op.Of(name: nameof(Label)).InvalidInput())
            : Style.Measure(Text);
    }

    public sealed record Written(GlyphBlock Block, Point2d At) : ScreenMark;
    public sealed record Sprite(SpriteRef Value, Point2d At, Size2i Extent, Option<(Point2d Origin, Size2f Extent)> Window, BlendUse Source, BlendUse Destination) : ScreenMark;
    public sealed record Group(Option<Pose> Pose, Option<ScreenPath> Clip, Seq<ScreenMark> Children) : ScreenMark;

    internal bool Valid => Switch(
        path: static row => row.Value is not null
            && row.Value.Valid
            && (row.Stroke.IsSome || row.Fill.IsSome)
            && row.Stroke.Match(Some: static stroke => stroke is not null, None: static () => true)
            && row.Fill.Match(Some: static fill => fill is not null && fill.Valid, None: static () => true),
        label: static row => !string.IsNullOrEmpty(row.Text) && Quant.Finite(row.At) && row.Style is not null,
        written: static row => row.Block is not null && Quant.Finite(row.At),
        sprite: static row => row.Value is not null && row.Source is not null && row.Destination is not null
            && Quant.Finite(row.At)
            && row.Extent.Width > 0
            && row.Extent.Height > 0
            && row.Window.Match(
                Some: static held => Quant.Finite(held.Origin) && Quant.Positive(held.Extent),
                None: static () => true),
        group: static row => row.Children.ForAll(static child => child is not null && child.Valid)
            && row.Clip.Match(Some: static clip => clip is not null && clip.Valid && clip.Closed, None: static () => true)
            && row.Pose.Match(Some: static pose => pose is not null && pose.Valid, None: static () => true));

    // Geometry answers from the mark alone; TEXT cannot — a label's extent is host metrics, so the two text arms carry
    // the measurement rail and the whole predicate rides it rather than flattening a refused measure into a miss.
    internal Fin<bool> HitTest(Point2d at, Op key) => Switch(
        (At: at, Key: key),
        path: static (ctx, row) => Fin.Succ(row.Value.Hit(ctx.At, row.Stroke, row.Fill.IsSome)),
        label: static (ctx, row) => row.Extent.Map(size => new RectangleF(
                row.Centered ? (float)row.At.X - (size.Width / 2f) : (float)row.At.X,
                row.Centered ? (float)row.At.Y - (size.Height / 2f) : (float)row.At.Y,
                size.Width,
                size.Height).Contains(Quant.Eto(ctx.At))),
        written: static (ctx, row) => row.Block.Measure(ctx.Key)
            .Map(size => new RectangleF(Quant.Eto(row.At), size).Contains(Quant.Eto(ctx.At))),
        sprite: static (ctx, row) => Fin.Succ(
            new RectangleF((float)row.At.X, (float)row.At.Y, row.Extent.Width, row.Extent.Height).Contains(Quant.Eto(ctx.At))),
        group: static (ctx, row) => row.Pose.Map(pose => pose.Unproject(ctx.At)).IfNone(ctx.At) is var local
            && row.Clip.Map(clip => clip.Hit(local, None, filled: true)).IfNone(true)
                ? row.Children.Map(child => (Child: child, Local: local, ctx.Key))
                    .TraverseM(static item => item.Child.HitTest(item.Local, item.Key)).As()
                    .Map(static hits => hits.Exists(static hit => hit))
                : Fin.Succ(false));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WorldMark {
    private WorldMark() { }
    public sealed record Curve(global::Rhino.Geometry.Curve Value, Stroke Stroke) : WorldMark;
    public sealed record MeshShaded(Mesh Value, ShadedMaterial Material) : WorldMark;
    public sealed record MeshBanded(Mesh Value, PerceptualColor Color, IsoBanding Banding) : WorldMark;
    public sealed record MeshFalseColors(Mesh Value) : WorldMark;
    public sealed record SubDShaded(SubD Value, ShadedMaterial Material) : WorldMark;
    public sealed record SubDWires(SubD Value, PerceptualColor Color, float Width) : WorldMark;
    public sealed record BrepShaded(Brep Value, ShadedMaterial Material) : WorldMark;
    public sealed record BrepWires(Brep Value, PerceptualColor Color, int Density) : WorldMark;
    public sealed record Block(InstanceDefinition Definition, ShadedMaterial Material, Transform Placement) : WorldMark;
    public sealed record Clipping(ClippingPlaneSurface Value, PerceptualColor Color) : WorldMark;
    public sealed record Hatch(global::Rhino.Geometry.Hatch Value, PerceptualColor Lines, PerceptualColor Fill) : WorldMark;
    public sealed record Text(TextEntity Value, PerceptualColor Color) : WorldMark;
    public sealed record Annotation(AnnotationBase Value, RhinoObject Owner, PerceptualColor Color) : WorldMark;
    public sealed record Arrowhead(global::Rhino.Geometry.Arrowhead Value, Transform Placement, PerceptualColor Color) : WorldMark;
    public sealed record Sprite(SpriteRef Value, Point3d At, float Size, bool WorldSized, PerceptualColor Tint, BlendUse Source, BlendUse Destination) : WorldMark;
    public sealed record SpriteCloud(SpriteRef Sprite, Seq<Point3d> Points, float Size, bool WorldSized, Option<Seq<PerceptualColor>> Colors, BlendUse Source, BlendUse Destination) : WorldMark;
    public sealed record Direction(SurfaceDirectionIndicators Value) : WorldMark;
    public sealed record Curvature(Brep Value, PerceptualColor Color) : WorldMark;
    public sealed record Draft(Mesh Value, PerceptualColor Color) : WorldMark;

    internal bool Valid => Switch(
        curve: static row => row.Value is not null && row.Stroke is not null,
        meshShaded: static row => row.Value is not null && row.Material is { Valid: true },
        meshBanded: static row => row.Value is not null && row.Banding is not null && row.Banding.Valid,
        meshFalseColors: static row => row.Value is not null,
        subDShaded: static row => row.Value is not null && row.Material is { Valid: true },
        subDWires: static row => row.Value is not null && row.Width > 0f && float.IsFinite(row.Width),
        brepShaded: static row => row.Value is not null && row.Material is { Valid: true },
        brepWires: static row => row.Value is not null && row.Density >= 0,
        block: static row => row.Definition is not null && row.Material is { Valid: true },
        clipping: static row => row.Value is not null,
        hatch: static row => row.Value is not null,
        text: static row => row.Value is not null,
        annotation: static row => row.Value is not null && row.Owner is not null,
        arrowhead: static row => row.Value is not null && row.Placement.IsValid,
        sprite: static row => row.Value is not null && row.Source is not null && row.Destination is not null
            && row.Size > 0f && float.IsFinite(row.Size),
        spriteCloud: static row => row.Sprite is not null && row.Source is not null && row.Destination is not null
            && !row.Points.IsEmpty
            && row.Size > 0f
            && float.IsFinite(row.Size)
            && row.Colors.Match(Some: colors => colors.Count == row.Points.Count, None: static () => true),
        direction: static row => row.Value is not null,
        curvature: static row => row.Value is not null,
        draft: static row => row.Value is not null);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Mark {
    private Mark() { }
    public sealed record Screen(ScreenMark Value) : Mark;
    public sealed record World(WorldMark Value) : Mark;

    internal bool Valid => Switch(
        screen: static row => row.Value is not null && row.Value.Valid,
        world: static row => row.Value is not null && row.Value.Valid);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Marks {
    public static Fin<int> Render(Canvas canvas, SpriteSheet sprites, Seq<Mark> marks, Op? key = null) {
        Op op = key.OrDefault();
        return guard(sprites is not null, op.InvalidInput()).ToFin()
            .Bind(_ => guard(canvas is not null && canvas.Valid
                && marks.ForAll(static mark => mark is not null && mark.Valid), op.InvalidInput()).ToFin())
            .Bind(_ => canvas.Switch(
                (Marks: marks, Sprites: sprites, Op: op),
                pipeline: static (ctx, backend) => ctx.Marks.Map(mark => (backend.Frame, ctx.Sprites, ctx.Op, Mark: mark))
                    .TraverseM(static item => Pipeline(item.Frame, item.Sprites, item.Mark, item.Op)).As()
                    .Map(static drawn => drawn.Count),
                surface: static (ctx, backend) =>
                    from _ in guard(ctx.Marks.ForAll(static mark => mark is Mark.Screen), ctx.Op.Unsupported(typeof(WorldMark), typeof(Graphics))).ToFin()
                    from drawn in ctx.Marks.Map(mark => (backend.Graphics, ctx.Sprites, ctx.Op, Mark: mark))
                        .TraverseM(static item => Surface(item.Graphics, item.Sprites, item.Mark, item.Op)).As()
                    select drawn.Count));
    }

    public static Fin<Seq<int>> Hit(Seq<Mark> marks, Point2d point, Op? key = null) {
        Op op = key.OrDefault();
        return guard(Quant.Finite(point), op.InvalidInput()).ToFin()
            .Bind(_ => guard(marks.ForAll(static mark => mark is Mark.Screen screen && screen.Valid), op.Unsupported(typeof(WorldMark), typeof(Marks))).ToFin())
            .Bind(_ => marks.Map(static (mark, index) => (Mark: ((Mark.Screen)mark).Value, Index: index))
                .TraverseM(item => item.Mark.HitTest(point, op).Map(hit => (Hit: hit, item.Index))).As())
            .Map(static hits => hits.Choose(static item => item.Hit ? Some(item.Index) : None));
    }

    public static PaintProgram Program(SpriteSheet sprites, Seq<Mark> marks, Op? key = null) => new(
        Paint: graphics => Render(new Canvas.Surface(graphics), sprites, marks, key).Map(static _ => unit),
        Hit: point => Hit(marks, new Point2d(point.X, point.Y), key));

    private static Fin<Unit> Pipeline(ConduitFrame frame, SpriteSheet sprites, Mark mark, Op key) => mark.Switch(
        (Frame: frame, Sprites: sprites, Op: key),
        screen: static (ctx, row) => PipelineScope.With(
            ctx.Frame.Pipeline,
            [new RenderAspect.Screen()],
            () => ScreenPipeline(ctx.Frame, ctx.Sprites, row.Value, ctx.Op),
            ctx.Op),
        world: static (ctx, row) => WorldPipeline(ctx.Frame, ctx.Sprites, row.Value, ctx.Op));

    private static Fin<Unit> Surface(Graphics graphics, SpriteSheet sprites, Mark mark, Op key) => mark.Switch(
        (Graphics: graphics, Sprites: sprites, Op: key),
        screen: static (ctx, row) => ScreenSurface(ctx.Graphics, ctx.Sprites, row.Value, ctx.Op),
        world: static (ctx, row) => Fin.Fail<Unit>(ctx.Op.Unsupported(row.Value.GetType(), typeof(Graphics))));

    private static Fin<Unit> Stroked(ConduitFrame frame, ScreenPath path, Stroke stroke, Op key) =>
        path.UseCurves(curves => key.Catch(() => curves.Iter(curve => frame.Pipeline.DrawCurve(curve, stroke.Rhino()))), key);

    private static Fin<Unit> ScreenPipeline(ConduitFrame frame, SpriteSheet sprites, ScreenMark mark, Op key) => mark.Switch(
        (Frame: frame, Sprites: sprites, Op: key),
        path: static (ctx, row) => row.Fill.Match(
            Some: _ => Fin.Fail<Unit>(ctx.Op.Unsupported(typeof(FillStyle), typeof(DisplayPipeline))),
            None: () => row.Stroke.ToFin(ctx.Op.InvalidInput()).Bind(stroke => Stroked(ctx.Frame, row.Value, stroke, ctx.Op))),
        label: static (ctx, row) => row.Style.Use(font => ctx.Op.Catch(() => ctx.Frame.Pipeline.Draw2dText(row.Text, Quant.Sys(row.Color), row.At, row.Centered, (int)font.Size, font.FamilyName))),
        written: static (ctx, row) => Fin.Fail<Unit>(ctx.Op.Unsupported(typeof(ScreenMark.Written), typeof(DisplayPipeline))),
        sprite: static (ctx, row) => row.Window.IsSome
            ? Fin.Fail<Unit>(ctx.Op.Unsupported(typeof(ScreenMark.Sprite), typeof(DisplayPipeline)))
            : ctx.Sprites.Pipeline(row.Value, row.Source, row.Destination, bitmap => ctx.Op.Catch(() =>
                Fin.Succ(Op.Side(() => ctx.Frame.Pipeline.DrawSprite(bitmap, row.At, row.Extent.Width, row.Extent.Height)))), ctx.Op),
        group: static (ctx, row) => row.Pose.IsNone && row.Clip.IsNone
            ? row.Children.Map(child => (ctx.Frame, ctx.Sprites, ctx.Op, Child: child))
                .TraverseM(static item => ScreenPipeline(item.Frame, item.Sprites, item.Child, item.Op)).As().Map(static _ => unit)
            : Fin.Fail<Unit>(ctx.Op.Unsupported(typeof(ScreenMark.Group), typeof(DisplayPipeline))));

    private static Fin<Unit> ScreenSurface(Graphics graphics, SpriteSheet sprites, ScreenMark mark, Op key) => mark.Switch(
        (Graphics: graphics, Sprites: sprites, Op: key),
        path: static (ctx, row) => DrawPath(ctx.Graphics, ctx.Sprites, row, ctx.Op),
        // Paint reads the mark's OWN held extent, so the drawn origin and the hit rectangle derive from one shaping.
        label: static (ctx, row) => ctx.Op.Catch(() => row.Extent.Map(measured =>
            new GlyphBlock(row.Text, row.Style.Role, foreground: Some(row.Color), size: row.Style.Size)
                .Draw(ctx.Graphics, row.Centered
                    ? new PointF((float)row.At.X - (measured.Width / 2f), (float)row.At.Y - (measured.Height / 2f))
                    : Quant.Eto(row.At)))),
        written: static (ctx, row) => ctx.Op.Catch(() => Fin.Succ(row.Block.Draw(ctx.Graphics, Quant.Eto(row.At)))),
        sprite: static (ctx, row) => ctx.Sprites.Surface(row.Value, bitmap => ctx.Op.Catch(() => Fin.Succ(Op.Side(() =>
            ctx.Graphics.DrawImage(
                bitmap,
                row.Window.Match(
                    Some: static held => new RectangleF(Quant.Eto(held.Origin), new SizeF(held.Extent.Width, held.Extent.Height)),
                    None: () => new RectangleF(0f, 0f, bitmap.Width, bitmap.Height)),
                new RectangleF((float)row.At.X, (float)row.At.Y, row.Extent.Width, row.Extent.Height))))), ctx.Op),
        group: static (ctx, row) => ctx.Op.Catch(() => {
            using IDisposable window = ctx.Graphics.SaveTransformState();
            _ = row.Pose.Iter(pose => pose.Use(matrix => Op.Side(() => ctx.Graphics.MultiplyTransform(matrix))));
            _ = row.Clip.Iter(clip => { using GraphicsPath built = clip.Eto(); ctx.Graphics.SetClip(built); });
            return row.Children.Map(child => (ctx.Graphics, ctx.Sprites, ctx.Op, Child: child))
                .TraverseM(static item => ScreenSurface(item.Graphics, item.Sprites, item.Child, item.Op)).As().Map(static _ => unit);
        }));

    private static Fin<Unit> DrawPath(Graphics graphics, SpriteSheet sprites, ScreenMark.Path row, Op key) => key.Catch(() => {
        using GraphicsPath path = row.Value.Eto();
        RectangleF bounds = path.Bounds;
        float outset = (float)row.Stroke.Map(static stroke => stroke.CullOutset).IfNone(0d);
        RectangleF painted = new(bounds.X - outset, bounds.Y - outset, bounds.Width + (2f * outset), bounds.Height + (2f * outset));
        return graphics.IsVisible(painted)
            ? row.Fill.Match(
                Some: fill => Brush(fill, sprites, brush => key.Catch(() => Fin.Succ(Op.Side(() => graphics.FillPath(brush, path)))), key),
                None: static () => Fin.Succ(unit))
                .Bind(_ => key.Catch(() => Fin.Succ(row.Stroke.Iter(stroke => {
                    using Pen pen = stroke.Eto();
                    graphics.DrawPath(pen, path);
                }))))
            : Fin.Succ(unit);
    });

    private static Fin<TResult> Brush<TResult>(
        FillStyle fill,
        SpriteSheet sprites,
        Func<Brush, Fin<TResult>> use,
        Op key) => fill.Switch(
        (Sprites: sprites, Use: use, Op: key),
        solid: static (ctx, row) => ctx.Op.Catch(() => {
            using Brush brush = new SolidBrush(Quant.Eto(row.Color));
            return ctx.Use(brush);
        }),
        linear: static (ctx, row) => ctx.Op.Catch(() => {
            using Brush brush = new LinearGradientBrush(Quant.Eto(row.Start), Quant.Eto(row.End), Quant.Eto(row.From), Quant.Eto(row.To));
            return ctx.Use(brush);
        }),
        radial: static (ctx, row) => ctx.Op.Catch(() => {
            using Brush brush = new RadialGradientBrush(
                Quant.Eto(row.Start), Quant.Eto(row.End), Quant.Eto(row.Center), Quant.Eto(row.Origin), new SizeF(row.Radius.Width, row.Radius.Height));
            return ctx.Use(brush);
        }),
        texture: static (ctx, row) => ctx.Sprites.Surface(row.Sprite, image => ctx.Op.Catch(() => {
            using Brush brush = new TextureBrush(image, row.Opacity);
            return ctx.Use(brush);
        }), ctx.Op));

    private static Fin<Unit> WorldPipeline(ConduitFrame frame, SpriteSheet sprites, WorldMark mark, Op key) => mark.Switch(
        (Frame: frame, Sprites: sprites, Op: key),
        curve: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawCurve(row.Value, row.Stroke.Rhino())),
        // Every shaded arm mints its native inside `ShadedMaterial.Use`, draws within that bracket, and releases on exit.
        meshShaded: static (ctx, row) => row.Material.Use(
            material => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawMeshShaded(row.Value, material)), ctx.Op),
        meshBanded: static (ctx, row) => row.Banding.Mint(ctx.Op).Bind(effect => ctx.Op.Catch(() => Fin.Succ(Op.Side(() =>
            ctx.Frame.Pipeline.DrawMeshShaded(row.Value, Quant.Sys(row.Color), effect))))),
        meshFalseColors: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawMeshFalseColors(row.Value)),
        subDShaded: static (ctx, row) => row.Material.Use(
            material => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawSubDShaded(row.Value, material)), ctx.Op),
        subDWires: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawSubDWires(row.Value, Quant.Sys(row.Color), row.Width)),
        brepShaded: static (ctx, row) => row.Material.Use(
            material => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawBrepShaded(row.Value, material)), ctx.Op),
        brepWires: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawBrepWires(row.Value, Quant.Sys(row.Color), row.Density)),
        block: static (ctx, row) => row.Material.Use(
            material => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawInstanceDefinitionShaded(row.Definition, material, row.Placement)), ctx.Op),
        clipping: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawClippingPlaneWires(row.Value, Quant.Sys(row.Color))),
        hatch: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawHatch(row.Value, Quant.Sys(row.Lines), Quant.Sys(row.Fill))),
        text: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawText(row.Value, Quant.Sys(row.Color))),
        annotation: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawAnnotation(row.Value, row.Owner, Quant.Sys(row.Color))),
        arrowhead: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawAnnotationArrowhead(row.Value, row.Placement, Quant.Sys(row.Color))),
        sprite: static (ctx, row) => ctx.Sprites.Pipeline(row.Value, row.Source, row.Destination, bitmap => ctx.Op.Catch(() => Fin.Succ(Op.Side(() =>
            ctx.Frame.Pipeline.DrawSprite(bitmap, row.At, row.Size, Quant.Sys(row.Tint), row.WorldSized)))), ctx.Op),
        spriteCloud: static (ctx, row) => ctx.Sprites.Pipeline(row.Sprite, row.Source, row.Destination, bitmap => ctx.Op.Catch(() => {
            DisplayBitmapDrawList list = new();
            _ = row.Colors.Match(
                Some: colors => Op.Side(() => list.SetPoints(row.Points.AsEnumerable(), colors.Map(Quant.Sys).AsEnumerable())),
                None: () => Op.Side(() => list.SetPoints(row.Points.AsEnumerable())));
            ctx.Frame.Pipeline.DrawSprites(bitmap, list, row.Size, row.WorldSized);
            return Fin.Succ(unit);
        }), ctx.Op),
        direction: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawSurfaceDirectionIndicators(row.Value)),
        curvature: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawCurvaturePreview(row.Value, Quant.Sys(row.Color))),
        draft: static (ctx, row) => ctx.Op.Catch(() => ctx.Frame.Pipeline.DrawDraftAnglePreview(row.Value, Quant.Sys(row.Color))));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
