# [RASM_RHINO_ETO_CANVAS]

The `Eto.Drawing` immediate-2D owner of `Rasm.Rhino.Eto` — one retained `Mark` scene vocabulary rendered by one fold into the host `Graphics` command stream, with geometry, paint, typography, transform, pixel, and lifecycle concerns as closed rows. The census hand-rolled hit testing beside its paint code and hard-coded font metrics; both die here structurally: pointer hit-testing derives from the SAME `PathSpec` the paint drew through the host `FillContains`/`StrokeContains` tests, and every measurement flows through `FormattedText.Measure`/`MeasureString` over `TypeRole` rows resolved from the host `SystemFonts` roster. Color enters as the kernel `PerceptualColor` and quantizes to `Eto.Drawing.Color` at exactly one mint site — perceptual blending, ramps, and contrast stay kernel math per the one-color-derivation law. `Surface` owns the `Drawable` lifecycle — mount, paint subscription, invalidation policy, IME composition, off-event graphics leases — and `PixelLease` rides `Bitmap.Lock` through the kernel `Lease<T>` rail so a locked pixel window can never outlive its scope.

## [01]-[INDEX]

- [02]-[PIGMENT_EDGE]: `Pigment` — the ONE `PerceptualColor` → `Color` quantization site — plus `FillSpec` (solid, linear, radial, texture) and `StrokeSpec` with `DashRow` — the paint vocabulary minting host brushes and pens only inside the render fold.
- [03]-[FIGURE]: `Figure` + `PathSpec` + `Pose` — retained path geometry as rows, one `Build` fold to `GraphicsPath`, hit-testing off the built path, and affine composition through the host `Matrix` factories.
- [04]-[GLYPHS]: `TypeRole` rows over `SystemFonts` + `GlyphBlock` — shaped, wrapped, aligned, trimmed text with `Measure` as the single metric authority.
- [05]-[SCENE]: `Mark` + `Scene` — the retained draw tree, ONE render fold with save/restore transform, clip, and visibility culling, and ONE hit-test fold over the same geometry.
- [06]-[SURFACE]: `SurfaceSpec` + `Surface` + `PixelLease` — `Drawable` mount and lifecycle, invalidation rows, off-event `Graphics` leases, IME verbs, and `Bitmap.Lock` pixel windows under `Lease<T>`.

## [02]-[PIGMENT_EDGE]

- Owner: `Pigment` — the boundary projector holding the ONE `PerceptualColor.ToRgb()` → `Eto.Drawing.Color` quantization in the sub-domain — plus `FillSpec`, the closed `[Union]` over the host brush family, and `StrokeSpec`, the one stroke record whose `DashRow` `[SmartEnum<int>]` carries the host dash presets and a custom-pattern case. Host `Color.Blend`/`Distance`/`ToHSB`/`ToHSL`/`ToCMYK` are naive sRGB math and never called: mixing, ramps, and contrast happen on `PerceptualColor` before this edge.
- Cases: `FillSpec` `Solid(PerceptualColor)` · `Linear(PerceptualColor From, PerceptualColor To, PointF Start, PointF End)` · `Radial(PerceptualColor From, PerceptualColor To, PointF Center, PointF Origin, SizeF Radius)` · `Textured(Image, UnitInterval Opacity)`; `DashRow` `Continuous` · `Dashed` · `Dotted` · `DashDotted` · `Patterned` (custom offsets).
- Law: brushes and pens are minted inside the render window and disposed with it — `Mint` returns the live host object, the scene fold owns the `using` bracket (the named platform-forced seam), and a brush cached across frames without a token owner is the rejected form.
- Packages: Rasm (kernel — `PerceptualColor`, `BlendPath`, `UnitInterval`), Eto.Drawing (host — `SolidBrush`, `LinearGradientBrush`, `RadialGradientBrush`, `TextureBrush`, `Pen`, `DashStyle`, `DashStyles`, `PenLineCap`, `PenLineJoin`).
- Growth: a new host brush kind is one `FillSpec` case; a new dash preset is one `DashRow` row; a gradient with N stops is a kernel `Ramp` projected onto a stop sequence at this edge when the host surface admits it.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Drawing;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Rhino.Eto;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Pigment {
    public static Color ToColor(PerceptualColor colour) =>
        colour.ToRgb() switch {
            { } rgb => Color.FromArgb(red: rgb.Red, green: rgb.Green, blue: rgb.Blue, alpha: (int)Math.Clamp(value: rgb.Alpha * 255.0, min: 0.0, max: 255.0)),
        };
}

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DashRow {
    public static readonly DashRow Continuous = new(key: 0, style: static _ => DashStyles.Solid);
    public static readonly DashRow Dashed = new(key: 1, style: static _ => DashStyles.Dash);
    public static readonly DashRow Dotted = new(key: 2, style: static _ => DashStyles.Dot);
    public static readonly DashRow DashDotted = new(key: 3, style: static _ => DashStyles.DashDot);
    public static readonly DashRow Patterned = new(key: 4, style: static offsets => new DashStyle(0f, [.. offsets]));
    [UseDelegateFromConstructor]
    internal partial DashStyle Style(Seq<float> offsets);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FillSpec {
    private FillSpec() { }
    public sealed record Solid(PerceptualColor Colour) : FillSpec;
    public sealed record Linear(PerceptualColor From, PerceptualColor To, PointF Start, PointF End) : FillSpec;
    public sealed record Radial(PerceptualColor From, PerceptualColor To, PointF Center, PointF Origin, SizeF Radius) : FillSpec;
    public sealed record Textured(Image Source, UnitInterval Opacity) : FillSpec;

    internal Brush Mint() => Switch(
        solid: static spec => new SolidBrush(color: Pigment.ToColor(colour: spec.Colour)),
        linear: static spec => new LinearGradientBrush(startColor: Pigment.ToColor(colour: spec.From), endColor: Pigment.ToColor(colour: spec.To), startPoint: spec.Start, endPoint: spec.End),
        radial: static spec => new RadialGradientBrush(startColor: Pigment.ToColor(colour: spec.From), endColor: Pigment.ToColor(colour: spec.To), center: spec.Center, gradientOrigin: spec.Origin, radius: spec.Radius),
        textured: static spec => (Brush)new TextureBrush(image: spec.Source, opacity: (float)spec.Opacity.Value));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record StrokeSpec(
    PerceptualColor Colour,
    PositiveMagnitude Thickness,
    DashRow Dash,
    Seq<float> DashOffsets = default,
    PenLineCap Cap = PenLineCap.Butt,
    PenLineJoin Join = PenLineJoin.Miter,
    float MiterLimit = 10f) {
    internal Pen Mint() {
        Pen pen = new(color: Pigment.ToColor(colour: Colour), thickness: (float)Thickness.Value) { LineCap = Cap, LineJoin = Join, MiterLimit = MiterLimit };
        _ = Op.SideWhen(Dash != DashRow.Continuous, () => pen.DashStyle = Dash.Style(offsets: DashOffsets));
        return pen;
    }
}
```

## [03]-[FIGURE]

- Owner: `Figure` — the closed `[Union]` of path atoms over the verified `GraphicsPath` construction surface — `PathSpec`, the one retained-geometry record folding figures into a `GraphicsPath` with `FillMode`, closure, and transform, and `Pose`, the affine composition union folding to `IMatrix` through the host `Matrix` factories. Hit-testing is a member of the SAME record the paint draws — `Hits(point, stroke?)` routes `FillContains` for closed geometry and `StrokeContains` under the probe pen otherwise — so the census-era parallel hit-test geometry copy is structurally impossible: one geometry value, two total reads.
- Cases: `Figure` `LineTo(PointF, PointF)` · `Curve(PointF Start, PointF ControlA, PointF ControlB, PointF End)` (bezier) · `Sweep(RectangleF, float StartAngle, float SweepAngle)` (arc) · `Through(Seq<PointF>, float Tension)` (cardinal spline) · `Round(RectangleF)` (ellipse) · `Box(RectangleF)` · `RoundedBox(RectangleF, float NW, float NE, float SE, float SW)` · `Chain(Seq<PointF>)` (polyline); `Pose` `Shift(float, float)` · `Turn(float)` · `Grow(float, float, PointF About)` · `Explicit(float XX, float YX, float XY, float YY, float X0, float Y0)` · `Stacked(Seq<Pose>)` · `Inverted(Pose)`.
- Law: `Build` is the one materialization — figures fold in declaration order onto a fresh `GraphicsPath`, closure appends `CloseFigure`, and the built path is a per-call product the caller owns; `RoundedBox` folds through the host `GetRoundRect` factory so corner geometry is never hand-approximated.
- Law: `Pose` folds right-to-left onto one `IMatrix` — `Stacked` folds the verified `Append` onto an identity `Matrix.Create` product, `Inverted` rides `Matrix.Inverse` — and a hand-rolled 2×3 multiply is the deleted form.
- Growth: a new path atom the host ships is one `Figure` case; a new transform modality is one `Pose` case; both break the folds at compile time.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Pose {
    private Pose() { }
    public sealed record Shift(float OffsetX, float OffsetY) : Pose;
    public sealed record Turn(float Angle) : Pose;
    public sealed record Grow(float ScaleX, float ScaleY, PointF About) : Pose;
    public sealed record Explicit(float XX, float YX, float XY, float YY, float X0, float Y0) : Pose;
    public sealed record Stacked(Seq<Pose> Poses) : Pose;
    public sealed record Inverted(Pose Body) : Pose;

    internal IMatrix ToMatrix() => Switch(
        shift: static pose => Matrix.FromTranslation(distanceX: pose.OffsetX, distanceY: pose.OffsetY),
        turn: static pose => Matrix.FromRotation(angle: pose.Angle),
        grow: static pose => Matrix.FromScaleAt(scaleX: pose.ScaleX, scaleY: pose.ScaleY, centerX: pose.About.X, centerY: pose.About.Y),
        @explicit: static pose => Matrix.Create(xx: pose.XX, yx: pose.YX, xy: pose.XY, yy: pose.YY, x0: pose.X0, y0: pose.Y0),
        stacked: static pose => pose.Poses.Fold(Matrix.Create(), static (folded, part) => (Op.Side(() => folded.Append(part.ToMatrix())), folded).Item2),
        inverted: static pose => Matrix.Inverse(matrix: pose.Body.ToMatrix()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Figure {
    private Figure() { }
    public sealed record LineTo(PointF From, PointF To) : Figure;
    public sealed record Curve(PointF Start, PointF ControlA, PointF ControlB, PointF End) : Figure;
    public sealed record Sweep(RectangleF Bounds, float StartAngle, float SweepAngle) : Figure;
    public sealed record Through(Seq<PointF> Points, float Tension = 0.5f) : Figure;
    public sealed record Round(RectangleF Bounds) : Figure;
    public sealed record Box(RectangleF Bounds) : Figure;
    public sealed record RoundedBox(RectangleF Bounds, float NW, float NE, float SE, float SW) : Figure;
    public sealed record Chain(Seq<PointF> Points) : Figure;

    internal Unit AddTo(GraphicsPath path) => Switch(
        state: path,
        lineTo: static (p, figure) => Op.Side(() => p.AddLine(startX: figure.From.X, startY: figure.From.Y, endX: figure.To.X, endY: figure.To.Y)),
        curve: static (p, figure) => Op.Side(() => p.AddBezier(start: figure.Start, control1: figure.ControlA, control2: figure.ControlB, end: figure.End)),
        sweep: static (p, figure) => Op.Side(() => p.AddArc(x: figure.Bounds.X, y: figure.Bounds.Y, width: figure.Bounds.Width, height: figure.Bounds.Height, startAngle: figure.StartAngle, sweepAngle: figure.SweepAngle)),
        through: static (p, figure) => Op.Side(() => p.AddCurve(points: figure.Points, tension: figure.Tension)),
        round: static (p, figure) => Op.Side(() => p.AddEllipse(x: figure.Bounds.X, y: figure.Bounds.Y, width: figure.Bounds.Width, height: figure.Bounds.Height)),
        box: static (p, figure) => Op.Side(() => p.AddRectangle(x: figure.Bounds.X, y: figure.Bounds.Y, width: figure.Bounds.Width, height: figure.Bounds.Height)),
        roundedBox: static (p, figure) => Op.Side(() => p.AddPath(path: GraphicsPath.GetRoundRect(rectangle: figure.Bounds, nwRadius: figure.NW, neRadius: figure.NE, seRadius: figure.SE, swRadius: figure.SW))),
        chain: static (p, figure) => Op.Side(() => p.AddLines(points: figure.Points)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record PathSpec(Seq<Figure> Figures, bool Closed = false, Option<Pose> Pose = default) {
    public static PathSpec Of(params ReadOnlySpan<Figure> figures) => new(Figures: toSeq(figures.ToArray()));

    internal GraphicsPath Build() {
        GraphicsPath path = new();
        _ = Figures.Iter(figure => figure.AddTo(path: path));
        _ = Op.SideWhen(Closed, path.CloseFigure);
        _ = Pose.Iter(pose => path.Transform(matrix: pose.ToMatrix()));
        return path;
    }

    public bool Hits(PointF point, Option<StrokeSpec> stroke = default) {
        using GraphicsPath path = Build();
        return stroke.Filter(_ => !Closed).Match(
            Some: spec => {
                using Pen probe = spec.Mint();
                return path.StrokeContains(pen: probe, point: point);
            },
            None: () => path.FillContains(point: point));
    }
}
```

## [04]-[GLYPHS]

- Owner: `TypeRole` `[SmartEnum<int>]` — the typography-role rows over the verified host `SystemFonts` roster (`Default`, `Bold`, `Label`, `Menu`, `MenuBar`, `Message`, `Palette`, `StatusBar`, `TitleBar`, `ToolTip`, `User`), each row a resolve column so a call site holds a role, never loose font parameters — and `GlyphBlock`, the one shaped-text record carrying text, role, foreground, wrap, alignment, trimming, and maximum extent, minting the host `FormattedText` and measuring through `Measure()`. The census hard-coded font metrics; the metric authority is now the shaped block itself, and `Graphics.MeasureString(Font, string)` survives only as the unshaped fast probe for single-line unwrapped text.
- Law: a role resolves once per use site into the host `Font` and rides the block — a raw `new Font(...)` in scene code is the deleted form; a custom face enters as a new `TypeRole` row whose resolve column constructs it, so face policy stays a declaration.
- Law: `Measure` is the single measurement verb — layout reserves by the measured `SizeF`, truncation and wrapping ride the host `FormattedTextWrapMode`/`FormattedTextTrimming` rows, and an advance estimated from character counts is the deleted form.
- Growth: a new role is one row; a new text treatment (max-extent ellipsis, right-aligned numeric) is field values on the block, never a sibling text type.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TypeRole {
    public static readonly TypeRole Body = new(key: 0, resolve: static () => SystemFonts.Default());
    public static readonly TypeRole Strong = new(key: 1, resolve: static () => SystemFonts.Bold());
    public static readonly TypeRole Caption = new(key: 2, resolve: static () => SystemFonts.Label());
    public static readonly TypeRole MenuText = new(key: 3, resolve: static () => SystemFonts.Menu());
    public static readonly TypeRole StatusText = new(key: 4, resolve: static () => SystemFonts.StatusBar());
    public static readonly TypeRole HintText = new(key: 5, resolve: static () => SystemFonts.ToolTip());
    public static readonly TypeRole TitleText = new(key: 6, resolve: static () => SystemFonts.TitleBar());
    public static readonly TypeRole MenuBarText = new(key: 7, resolve: static () => SystemFonts.MenuBar());
    public static readonly TypeRole MessageText = new(key: 8, resolve: static () => SystemFonts.Message());
    public static readonly TypeRole PaletteText = new(key: 9, resolve: static () => SystemFonts.Palette());
    public static readonly TypeRole UserText = new(key: 10, resolve: static () => SystemFonts.User());
    [UseDelegateFromConstructor]
    internal partial Font Resolve();
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record GlyphBlock(
    string Text,
    TypeRole Role,
    PerceptualColor Foreground,
    FormattedTextWrapMode Wrap = FormattedTextWrapMode.Word,
    FormattedTextAlignment Alignment = FormattedTextAlignment.Left,
    FormattedTextTrimming Trimming = FormattedTextTrimming.WordEllipsis,
    Option<SizeF> MaxExtent = default) {
    internal FormattedText Shape() {
        FormattedText shaped = new() {
            Text = Text,
            Font = Role.Resolve(),
            Wrap = Wrap,
            Alignment = Alignment,
            Trimming = Trimming,
            ForegroundBrush = new SolidBrush(color: Pigment.ToColor(colour: Foreground)),
        };
        _ = MaxExtent.Iter(extent => shaped.MaximumSize = extent);
        return shaped;
    }

    public SizeF Measure() {
        using FormattedText shaped = Shape();
        return shaped.Measure();
    }
}
```

## [05]-[SCENE]

- Owner: `Mark` — the closed `[Union]` draw tree (`Stroked`, `Filled`, `Bordered` fill-then-stroke, `Written` glyph block, `Blit` image, `Grouped` transform/clip/children) — and `Scene`, the retained value whose ONE `Render(Graphics, Op)` fold issues the entire host command stream and whose ONE `HitTest(PointF)` fold answers pointer containment over the identical geometry. Render state is a strict discipline: `Grouped` pushes `SaveTransformState()` (the host returns the restore handle as `IDisposable`), applies its pose through `MultiplyTransform`, clips through `SetClip` so descendants render bounded, and the `using` window is the named platform-forced seam.
- Cases: `Stroked(PathSpec, StrokeSpec)` · `Filled(PathSpec, FillSpec)` · `Bordered(PathSpec, FillSpec, StrokeSpec)` · `Written(GlyphBlock, PointF)` · `Blit(Image, RectangleF Source, RectangleF Target)` · `Grouped(Option<Pose>, Option<PathSpec> Clip, Seq<Mark> Children)`.
- Law: paint objects live exactly one draw — each arm mints its brush/pen inside its own `using` window, and the `Written` arm brackets its shaped `FormattedText` the same way (`FormattedText` derives `Widget` and is disposable); `HitTest` returns the FIRST hit in reverse paint order (topmost wins) as `Option<Mark>`, the grouped probe maps the point through the group pose's inverse and gates on the clip so hit truth shares the paint's transform frame, and marks needing identity ride a `Grouped` wrapper keyed by the consumer.
- Law: quality knobs (`AntiAlias`, `ImageInterpolation`, `PixelOffsetMode`) are `ScenePolicy` values applied once at render entry, never per-mark toggles.
- Boundary: this fold is the shared paint spine — `chrome.md`'s print pages and this page's `Surface` both hand it a `Graphics`; Rhino viewport HUD drawing is the Display unit's conduit and never enters here.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ScenePolicy(bool AntiAlias = true, ImageInterpolation Interpolation = ImageInterpolation.High, PixelOffsetMode Offset = PixelOffsetMode.Half) {
    public static readonly ScenePolicy Crisp = new();
}

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Mark {
    private Mark() { }
    public sealed record Stroked(PathSpec Path, StrokeSpec Stroke) : Mark;
    public sealed record Filled(PathSpec Path, FillSpec Fill) : Mark;
    public sealed record Bordered(PathSpec Path, FillSpec Fill, StrokeSpec Stroke) : Mark;
    public sealed record Written(GlyphBlock Block, PointF At) : Mark;
    public sealed record Blit(Image Source, RectangleF From, RectangleF Target) : Mark;
    public sealed record Grouped(Option<Pose> Pose, Option<PathSpec> Clip, Seq<Mark> Children) : Mark;

    internal Unit Draw(Graphics graphics) => Switch(
        state: graphics,
        stroked: static (g, mark) => Op.Side(() => { using GraphicsPath path = mark.Path.Build(); using Pen pen = mark.Stroke.Mint(); g.DrawPath(pen: pen, path: path); }),
        filled: static (g, mark) => Op.Side(() => { using GraphicsPath path = mark.Path.Build(); using Brush brush = mark.Fill.Mint(); g.FillPath(brush: brush, path: path); }),
        bordered: static (g, mark) => Op.Side(() => {
            using GraphicsPath path = mark.Path.Build();
            using Brush brush = mark.Fill.Mint();
            using Pen pen = mark.Stroke.Mint();
            g.FillPath(brush: brush, path: path);
            g.DrawPath(pen: pen, path: path);
        }),
        written: static (g, mark) => Op.Side(() => { using FormattedText shaped = mark.Block.Shape(); g.DrawText(formattedText: shaped, location: mark.At); }),
        blit: static (g, mark) => Op.Side(() => g.DrawImage(image: mark.Source, source: mark.From, destination: mark.Target)),
        grouped: static (g, mark) => Op.Side(() => {
            using IDisposable window = g.SaveTransformState();
            _ = mark.Pose.Iter(pose => g.MultiplyTransform(matrix: pose.ToMatrix()));
            _ = mark.Clip.Iter(clip => { using GraphicsPath built = clip.Build(); g.SetClip(path: built); });
            _ = mark.Children.Iter(child => child.Draw(graphics: g));
        }));

    internal static Option<Mark> Topmost(Seq<Mark> marks, PointF point) =>
        marks.Rev().Map(mark => mark.Probe(point: point)).Somes().HeadOrNone();

    internal Option<Mark> Probe(PointF point) => Switch(
        state: point,
        stroked: static (at, mark) => mark.Path.Hits(point: at, stroke: Some(mark.Stroke)) ? Some((Mark)mark) : None,
        filled: static (at, mark) => mark.Path.Hits(point: at) ? Some((Mark)mark) : None,
        bordered: static (at, mark) => mark.Path.Hits(point: at, stroke: Some(mark.Stroke)) ? Some((Mark)mark) : None,
        written: static (at, mark) => new RectangleF(mark.At, mark.Block.Measure()).Contains(at) ? Some((Mark)mark) : None,
        blit: static (at, mark) => mark.Target.Contains(at) ? Some((Mark)mark) : None,
        grouped: static (at, mark) => {
            PointF local = mark.Pose.Map(pose => Matrix.Inverse(matrix: pose.ToMatrix()).TransformPoint(point: at)).IfNone(at);
            return mark.Clip.Map(clip => clip.Hits(point: local)).IfNone(true) ? Topmost(marks: mark.Children, point: local) : None;
        });
}

public sealed record Scene(Seq<Mark> Marks, ScenePolicy Policy) {
    public static Scene Of(params ReadOnlySpan<Mark> marks) => new(Marks: toSeq(marks.ToArray()), Policy: ScenePolicy.Crisp);

    public Fin<Unit> Render(Graphics graphics, Op? key = null) =>
        key.OrDefault().Catch(() => {
            graphics.AntiAlias = Policy.AntiAlias;
            graphics.ImageInterpolation = Policy.Interpolation;
            graphics.PixelOffsetMode = Policy.Offset;
            _ = Marks.Iter(mark => mark.Draw(graphics: graphics));
            return Fin.Succ(value: unit);
        });

    public Option<Mark> HitTest(PointF point) => Mark.Topmost(marks: Marks, point: point);
}
```

## [06]-[SURFACE]

- Owner: `SurfaceSpec` + `Surface` — the `Drawable` lifecycle owner: `Mount` constructs the host, subscribes `Paint` once (the named platform-forced event seam), and holds the live scene in an `Atom<Scene>` so a scene swap plus an invalidation row is the whole redraw protocol — plus `Redraw`, the invalidation union (`Whole` full-surface, `Region(Rectangle)` bounded, `Immediate(Rectangle)` synchronous `Update`), `Acquire`, the off-event graphics lease riding the kernel `Lease<Graphics>.Owned` so an out-of-band draw disposes deterministically, and `PixelLease`, the `Bitmap.Lock` window under the same rail with the unlocked `GetPixel`/`SetPixel` pair and `ToByteArray(ImageFormat)` egress beside it.
- Law: the paint handler renders the CURRENT atom value — no captured scene, no dirty flags: swap then invalidate, and the host replays the whole stream on the next paint; pointer wiring reads `Surface.HitTest` over the same value, so paint and hit truth cannot diverge.
- Law: the realized `Drawable` is its surface handle — `Surface.Of(host)` recovers the mounted owner from the control the way `Bind.Owned` recovers receipts, so an `Element.Painted` consumer swaps scenes and hit-tests through the one control `Realize` returned, and a parallel surface registry is the deleted form.
- Law: IME composition rides the host verbs — `CancelComposition`/`CommitComposition` project `CancelTextComposition`/`CommitTextComposition` — and a text-editing overlay that ignores composition state is the named defect.
- Law: `Acquire` succeeds only where the host advertises it — `SupportsCreateGraphics` gates the lease with a typed `UiFault.Unavailable`, never a downstream null.
- Growth: a new invalidation modality is one `Redraw` case; frame pacing, display-link cadence, and animation clocks are the Viewport unit's motion owner — this surface exposes swap-and-invalidate and nothing temporal.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Redraw {
    private Redraw() { }
    public sealed record Whole : Redraw;
    public sealed record Region(Rectangle Bounds) : Redraw;
    public sealed record Immediate(Rectangle Bounds) : Redraw;
}

public sealed record SurfaceSpec(Scene Initial, bool LargeCanvas = false, bool Focusable = false) {
    public Fin<Drawable> Mount(Op? key = null) => Surface.Mount(spec: this, key: key).Map(static surface => surface.Host);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class Surface {
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Drawable, Surface> Mounted = new();
    private readonly Atom<Scene> scene;
    private Surface(Drawable host, Atom<Scene> scene) { Host = host; this.scene = scene; }

    public Drawable Host { get; }

    public static Fin<Surface> Mount(SurfaceSpec spec, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => {
            Atom<Scene> held = Atom(spec.Initial);
            Drawable host = new(largeCanvas: spec.LargeCanvas) { CanFocus = spec.Focusable };
            host.Paint += (_, args) => ignore(held.Value.Render(graphics: args.Graphics, key: op));
            Surface surface = new(host: host, scene: held);
            Mounted.Add(host, surface);
            return Fin.Succ(value: surface);
        });
    }

    public static Option<Surface> Of(Drawable host) =>
        Mounted.TryGetValue(host, out Surface? held) ? Some(held) : None;

    public Unit Swap(Func<Scene, Scene> next, Redraw redraw) {
        _ = scene.Swap(next);
        return redraw.Switch(
            state: Host,
            whole: static (host, _) => Op.Side(host.Invalidate),
            region: static (host, bounded) => Op.Side(() => host.Invalidate(rect: bounded.Bounds)),
            immediate: static (host, bounded) => Op.Side(() => host.Update(region: bounded.Bounds)));
    }

    public Option<Mark> HitTest(PointF point) => scene.Value.HitTest(point: point);

    public Fin<TResult> Acquire<TResult>(Func<Graphics, TResult> draw, Op? key = null) =>
        Host.SupportsCreateGraphics
            ? key.OrDefault().Catch(() => Fin.Succ(value: new Lease<Graphics>.Owned(Value: Host.CreateGraphics()).Use(project: draw)))
            : Fin.Fail<TResult>(error: new UiFault.Unavailable(Key: key.OrDefault(), Capability: nameof(Host.SupportsCreateGraphics)));

    public Unit CancelComposition() => Op.Side(Host.CancelTextComposition);

    public Unit CommitComposition() => Op.Side(Host.CommitTextComposition);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class PixelLease {
    public static Fin<TResult> Locked<TResult>(Bitmap bitmap, Func<BitmapData, TResult> read, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: new Lease<BitmapData>.Owned(Value: bitmap.Lock()).Use(project: read)));

    public static Fin<PerceptualColor> Sample(Bitmap bitmap, Point at, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: bitmap.GetPixel(position: at))).Bind(colour =>
            PerceptualColor.OfRgb(red: (byte)colour.Rb, green: (byte)colour.Gb, blue: (byte)colour.Bb, alpha: colour.A, key: key));

    public static Fin<byte[]> Encode(Bitmap bitmap, ImageFormat format, Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: bitmap.ToByteArray(imageFormat: format)));
}
```
