using System.Collections.Concurrent;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Skinning;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CanvasPaintPhase {
    private delegate Unit PaintWire(GhCanvas canvas, EventHandler<CanvasPaintEventArgs> handler);

    public static readonly CanvasPaintPhase BeforeBackground = Phase(key: 0, attach: static (c, h) => c.BeforePaintBackground += h, detach: static (c, h) => c.BeforePaintBackground -= h);
    public static readonly CanvasPaintPhase AfterBackground = Phase(key: 1, attach: static (c, h) => c.AfterPaintBackground += h, detach: static (c, h) => c.AfterPaintBackground -= h);
    public static readonly CanvasPaintPhase BeforeGroups = Phase(key: 2, attach: static (c, h) => c.BeforePaintGroups += h, detach: static (c, h) => c.BeforePaintGroups -= h);
    public static readonly CanvasPaintPhase AfterGroups = Phase(key: 3, attach: static (c, h) => c.AfterPaintGroups += h, detach: static (c, h) => c.AfterPaintGroups -= h);
    public static readonly CanvasPaintPhase BeforeWires = Phase(key: 4, attach: static (c, h) => c.BeforePaintWires += h, detach: static (c, h) => c.BeforePaintWires -= h);
    public static readonly CanvasPaintPhase AfterWires = Phase(key: 5, attach: static (c, h) => c.AfterPaintWires += h, detach: static (c, h) => c.AfterPaintWires -= h);
    public static readonly CanvasPaintPhase BeforeObjects = Phase(key: 6, attach: static (c, h) => c.BeforePaintObjects += h, detach: static (c, h) => c.BeforePaintObjects -= h);
    public static readonly CanvasPaintPhase AfterObjects = Phase(key: 7, attach: static (c, h) => c.AfterPaintObjects += h, detach: static (c, h) => c.AfterPaintObjects -= h);

    [UseDelegateFromConstructor]
    internal partial Unit Attach(GhCanvas canvas, EventHandler<CanvasPaintEventArgs> handler);

    [UseDelegateFromConstructor]
    internal partial Unit Detach(GhCanvas canvas, EventHandler<CanvasPaintEventArgs> handler);

    private static CanvasPaintPhase Phase(int key, Action<GhCanvas, EventHandler<CanvasPaintEventArgs>> attach, Action<GhCanvas, EventHandler<CanvasPaintEventArgs>> detach) =>
        new(
            key: key,
            attach: (c, h) => { attach(arg1: c, arg2: h); return unit; },
            detach: (c, h) => { detach(arg1: c, arg2: h); return unit; });
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintScope(
    CanvasPaintPhase Phase,
    ControlGraphics Graphics,
    Skin Skin) {
    public bool DefaultBackgroundOverridden =>
        Background.Map(static args => args.DefaultOverridden).IfNone(noneValue: false);

    public Unit OverrideDefault() =>
        Background.Match(
            Some: args => { args.OverrideDefaultPainting(); return unit; },
            None: () => unit);

    internal Option<CanvasBackgroundPaintEventArgs> Background { get; init; }

    public Fin<Unit> Apply(DrawMark mark) {
        PaintScope current = this;
        return Optional(mark)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Apply)), detail: "draw mark is required"))
            .Bind(valid => valid.Apply(scope: current));
    }

    // TextMeasure-routed to avoid Graphics.MeasureString state leak on macOS (Eto.Mac 2.11
    // GraphicsHandler IL@37767 mutates a shared FormattedText inheriting prior wrap/alignment).
    public Fin<PaintTextMeasurement> MeasureText(string value, Option<UiFont> font = default) =>
        Optional(value)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(MeasureText)), detail: "text is required"))
            .Bind(valid => Try.lift(f: () =>
                font.IfNone(UiFont.Empty()).Use(run: resolved =>
                    new PaintTextMeasurement(Text: valid, Size: TextMeasure.Single(font: resolved, text: valid))
                )).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(MeasureText)), detail: error.Message)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintStyle(
    Color Edge,
    Option<Color> Fill = default,
    float Thickness = 1f,
    Option<UiFont> Font = default,
    Color Background = default,
    PenLineCap LineCap = PenLineCap.Butt,
    PenLineJoin LineJoin = PenLineJoin.Miter,
    Option<DashStyle> Dash = default,
    float MiterLimit = 10f,
    bool AntiAlias = true,
    ImageInterpolation ImageInterpolation = ImageInterpolation.Default,
    PixelOffsetMode PixelOffset = PixelOffsetMode.None) {
    internal Pen Pen() =>
        new(color: Edge, thickness: Thickness) {
            LineCap = LineCap,
            LineJoin = LineJoin,
            DashStyle = Dash.IfNone(DashStyles.Solid),
            MiterLimit = MiterLimit,
        };

    internal static SolidBrush Brush(Color color) => new(color: color);

    internal Unit Assign(Graphics graphics) {
        graphics.AntiAlias = AntiAlias;
        graphics.ImageInterpolation = ImageInterpolation;
        graphics.PixelOffsetMode = PixelOffset;
        return unit;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintTextMeasurement(string Text, SizeF Size);

// Process-static measurement cache. Key includes FontDecoration explicitly — Eto.Drawing.Font
// equality excludes it. Per-thread FormattedText scratch (AppKit text APIs are main-thread-only).
file static class TextMeasure {
    private readonly record struct Key(
        Font Font, string Text,
        FormattedTextWrapMode Wrap, FormattedTextAlignment Alignment, FormattedTextTrimming Trimming,
        float MaxWidth, float MaxHeight, FontDecoration Decoration);

    private const int CacheSizeLimit = 1024;
    private static readonly ConcurrentDictionary<Key, SizeF> Cache = new();
    private static int trimming;
    [ThreadStatic] private static FormattedText? scratch;

    private static FormattedText Scratch() => scratch ??= new FormattedText();

    internal static SizeF Single(Font font, string text) =>
        Measure(font: font, text: text,
            wrap: FormattedTextWrapMode.None,
            alignment: FormattedTextAlignment.Left,
            trimming: FormattedTextTrimming.None,
            maxSize: new SizeF(float.MaxValue, float.MaxValue));

    internal static SizeF Measure(Font font, string text,
        FormattedTextWrapMode wrap, FormattedTextAlignment alignment, FormattedTextTrimming trimming,
        SizeF maxSize) {
        _ = Cache.Count >= CacheSizeLimit ? TrimCache() : unit;
        return Cache.GetOrAdd(
            key: new Key(Font: font, Text: text, Wrap: wrap, Alignment: alignment, Trimming: trimming,
                MaxWidth: maxSize.Width, MaxHeight: maxSize.Height, Decoration: font.FontDecoration),
            valueFactory: static k => {
                FormattedText ft = Scratch();
                ft.Font = k.Font;
                ft.Text = k.Text;
                ft.Wrap = k.Wrap;
                ft.Alignment = k.Alignment;
                ft.Trimming = k.Trimming;
                ft.MaximumSize = new SizeF(width: k.MaxWidth, height: k.MaxHeight);
                return ft.Measure();
            });
    }

    // Bounded eviction — drop the oldest half (FIFO via Keys snapshot order). Single-flight via
    // Interlocked CAS so concurrent Measure() callers don't double-trim; the loser fast-returns.
    private static Unit TrimCache() {
        if (Interlocked.Exchange(ref trimming, 1) == 1) {
            return unit;
        }
        try {
            _ = toSeq(Cache.Keys).Take(Cache.Count / 2).Iter(static key => Cache.TryRemove(key, out _));
        } finally {
            _ = Interlocked.Exchange(ref trimming, 0);
        }
        return unit;
    }

    internal static int Count => Cache.Count;
    internal static Unit Clear() { Cache.Clear(); return unit; }
}

[Union]
public partial record UiFont {
    private UiFont() { }
    public sealed record SystemCase(SystemFont Kind, float? Size = null, FontDecoration Decoration = FontDecoration.None) : UiFont;
    public sealed record FamilyCase(string Name, float Size, FontStyle Style = FontStyle.None, FontDecoration Decoration = FontDecoration.None) : UiFont;
    public sealed record NativeCase(Font Value) : UiFont;
    public sealed record EmptyCase : UiFont;

    public static UiFont Empty() => new EmptyCase();
    public static UiFont System(SystemFont kind, float? size = null, FontDecoration decoration = FontDecoration.None) =>
        new SystemCase(Kind: kind, Size: size, Decoration: decoration);
    public static UiFont Family(string family, float size, FontStyle style = FontStyle.None, FontDecoration decoration = FontDecoration.None) =>
        new FamilyCase(Name: family, Size: size, Style: style, Decoration: decoration);
    public static UiFont Native(Font value) => new NativeCase(Value: value);

    // State-threaded dispatch passes the caller's run delegate through `state` so every arm stays
    // `static`. Family case owns a transient Font via `using`; system/native/empty cases hit cached
    // or caller-owned instances directly.
    internal T Use<T>(Func<Font, T> run) =>
        Switch(state: run,
            systemCase: static (r, s) => r(arg: SystemFonts.Cached(systemFont: s.Kind, size: s.Size, decoration: s.Decoration)),
            familyCase: static (r, f) => {
                using Font owned = new(family: f.Name, size: f.Size, style: f.Style, decoration: f.Decoration);
                return r(arg: owned);
            },
            nativeCase: static (r, n) => r(arg: n.Value),
            emptyCase: static (r, _) => r(arg: SystemFonts.Default()));
}

// Solid → Brushes.Cached; gradient/texture allocate fresh. Mac Brush.Dispose is inert under
// Eto 2.11 (handler is non-IDisposable); using-blocks future-proof against later API changes.
[Union]
public partial record FillSource {
    private FillSource() { }
    public sealed record SolidCase(Color Colour) : FillSource;
    public sealed record LinearCase(Color Start, Color End, PointF From, PointF To, GradientWrapMode Wrap = GradientWrapMode.Pad) : FillSource;
    public sealed record RadialCase(Color Centre, Color Edge, PointF Origin, PointF Focus, SizeF Radius, GradientWrapMode Wrap = GradientWrapMode.Pad) : FillSource;
    public sealed record TextureCase(Image Source, float Opacity = 1f) : FillSource;

    public static FillSource Solid(Color colour) => new SolidCase(Colour: colour);
    public static FillSource Linear(Color start, Color end, PointF from, PointF to, GradientWrapMode wrap = GradientWrapMode.Pad) =>
        new LinearCase(Start: start, End: end, From: from, To: to, Wrap: wrap);
    public static FillSource Radial(Color centre, Color edge, PointF origin, PointF focus, SizeF radius, GradientWrapMode wrap = GradientWrapMode.Pad) =>
        new RadialCase(Centre: centre, Edge: edge, Origin: origin, Focus: focus, Radius: radius, Wrap: wrap);
    public static FillSource Texture(Image source, float opacity = 1f) =>
        new TextureCase(Source: source, Opacity: opacity);

    internal Brush CreateBrush() =>
        Switch<Brush>(
            solidCase: static s => Brushes.Cached(color: s.Colour),
            linearCase: static l => new LinearGradientBrush(startColor: l.Start, endColor: l.End, startPoint: l.From, endPoint: l.To) { Wrap = l.Wrap },
            radialCase: static r => new RadialGradientBrush(startColor: r.Centre, endColor: r.Edge, center: r.Origin, gradientOrigin: r.Focus, radius: r.Radius) { Wrap = r.Wrap },
            textureCase: static t => new TextureBrush(image: t.Source, opacity: t.Opacity));

    internal bool IsCached => this is SolidCase;
}

// Solid + vanilla cap/join/miter → Pens.Cached. IsVanilla matches Eto.Mac.PenHandler.Create
// defaults (Square cap, Miter join, MiterLimit=10f); non-vanilla paths allocate fresh.
[Union]
public partial record EdgeSource {
    private EdgeSource() { }
    public sealed record SolidCase(Color Colour) : EdgeSource;
    public sealed record BrushBackedCase(FillSource Source) : EdgeSource;

    public static EdgeSource Solid(Color colour) => new SolidCase(Colour: colour);
    public static EdgeSource FromFill(FillSource source) => new BrushBackedCase(Source: source);

    internal Pen CreatePen(float thickness, PenLineCap cap, PenLineJoin join, float miterLimit, DashStyle dash) =>
        Switch(
            solidCase: s => IsVanilla(cap: cap, join: join, miterLimit: miterLimit)
                ? Pens.Cached(color: s.Colour, thickness: thickness, dashStyle: dash)
                : new Pen(color: s.Colour, thickness: thickness) { LineCap = cap, LineJoin = join, MiterLimit = miterLimit, DashStyle = dash },
            brushBackedCase: b => new Pen(brush: b.Source.CreateBrush(), thickness: thickness) { LineCap = cap, LineJoin = join, MiterLimit = miterLimit, DashStyle = dash });

    internal bool IsCached(PenLineCap cap, PenLineJoin join, float miterLimit) =>
        this is SolidCase && IsVanilla(cap: cap, join: join, miterLimit: miterLimit);

    // 10f is the literal Eto default; bit-exact equality is correct (no arithmetic separates the
    // requested value from the constant) and Pens.Cached treats DashStyle by reference identity.
    internal const float DefaultMiterLimit = 10f;
    private static bool IsVanilla(PenLineCap cap, PenLineJoin join, float miterLimit) =>
        cap == PenLineCap.Square && join == PenLineJoin.Miter && miterLimit == DefaultMiterLimit;
}

// Per-corner radii (TL/TR/BR/BL clockwise). NaN/negative inputs mis-render in GraphicsPath; the
// construction-time invariant prevents that.
[ComplexValueObject]
[ValidationError<UiFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct CornerRadii {
    public float TopLeft { get; }
    public float TopRight { get; }
    public float BottomRight { get; }
    public float BottomLeft { get; }

    // Accumulates every invalid corner into one composite message — caller sees all four diagnostics
    // at once instead of fixing them one-by-one across rebuild cycles.
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref float topLeft, ref float topRight, ref float bottomRight, ref float bottomLeft) {
        Op op = Op.Of(name: nameof(CornerRadii));
        Seq<string> errors = Seq(
            Valid(topLeft) ? "" : $"TopLeft must be finite and >= 0 (got {topLeft:R}).",
            Valid(topRight) ? "" : $"TopRight must be finite and >= 0 (got {topRight:R}).",
            Valid(bottomRight) ? "" : $"BottomRight must be finite and >= 0 (got {bottomRight:R}).",
            Valid(bottomLeft) ? "" : $"BottomLeft must be finite and >= 0 (got {bottomLeft:R}).")
            .Filter(static s => !string.IsNullOrEmpty(s));
        validationError = errors.IsEmpty ? null : UiFault.Create(op: op, message: string.Join("; ", errors));
    }

    private static bool Valid(float corner) => float.IsFinite(corner) && corner >= 0f;

    public static CornerRadii Uniform(float radius) => Create(topLeft: radius, topRight: radius, bottomRight: radius, bottomLeft: radius);
    internal bool IsUniform => TopLeft == TopRight && TopLeft == BottomRight && TopLeft == BottomLeft;
}

[Union]
public partial record DrawMark {
    private DrawMark() { }
    public sealed record LineCase(PointF A, PointF B, PaintStyle Style) : DrawMark;
    public sealed record RectangleCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed record RoundedRectangleCase(RectangleF Bounds, float Radius, PaintStyle Style) : DrawMark;
    public sealed record RoundedCornersCase(RectangleF Bounds, CornerRadii Radii, PaintStyle Style) : DrawMark;
    public sealed record EllipseCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed record PathCase(IGraphicsPath Geometry, PaintStyle Style) : DrawMark;
    public sealed record TextCase(string Value, RectangleF Frame, PaintStyle Style, FormattedTextWrapMode Wrap, FormattedTextAlignment Alignment, FormattedTextTrimming Trimming) : DrawMark;
    public sealed record ImageCase(Image Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed record GhIconCase(IIcon Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed record WireCase(PointF Source, PointF Target, WireKind Kind, PaintStyle Style) : DrawMark;

    public static DrawMark Line(PointF a, PointF b, Color colour, float thickness = 1f) =>
        new LineCase(A: a, B: b, Style: new PaintStyle(Edge: colour, Thickness: thickness));
    public static DrawMark Rectangle(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RectangleCase(Bounds: bounds, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark RoundedRectangle(RectangleF bounds, float radius, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RoundedRectangleCase(Bounds: bounds, Radius: radius, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark RoundedCorners(RectangleF bounds, CornerRadii radii, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RoundedCornersCase(Bounds: bounds, Radii: radii, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark Ellipse(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new EllipseCase(Bounds: bounds, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark Path(IGraphicsPath path, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new PathCase(Geometry: path, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark Label(
        string value,
        RectangleF frame,
        Color colour,
        Option<UiFont> font = default,
        FormattedTextWrapMode wrap = FormattedTextWrapMode.Word,
        FormattedTextAlignment alignment = FormattedTextAlignment.Left,
        FormattedTextTrimming trimming = FormattedTextTrimming.WordEllipsis) =>
        new TextCase(Value: value, Frame: frame, Style: new PaintStyle(Edge: colour, Font: font), Wrap: wrap, Alignment: alignment, Trimming: trimming);
    public static DrawMark Image(Image value, RectangleF frame) =>
        new ImageCase(Value: value, Frame: frame, Style: new PaintStyle(Edge: Colors.Transparent));
    public static DrawMark IconGlyph(IIcon value, RectangleF frame, Color background) =>
        new GhIconCase(Value: value, Frame: frame, Style: new PaintStyle(Edge: Colors.Transparent, Background: background));
    public static DrawMark WirePreview(PointF source, PointF target, WireKind kind = WireKind.Tentative) =>
        new WireCase(Source: source, Target: target, Kind: kind, Style: new PaintStyle(Edge: Colors.Transparent));

    // Per-arm Op.Of(name: nameof(...)) inside each Switch arm gives compile-time provenance for
    // failure attribution — `$"DrawMark.{GetType().Name}"` runtime interpolation would be opaque to
    // analyzer rules (CSP0801). Op.Attempt collapses the Try.lift boundary into each arm.
    internal Fin<Unit> Apply(PaintScope scope) =>
        Switch(state: scope,
            lineCase: static (s, line) => Op.Of(name: nameof(LineCase)).Attempt(body: () => Draw(scope: s, style: line.Style, run: graphics => {
                using Pen pen = line.Style.Pen();
                graphics.DrawLine(pen, line.A.X, line.A.Y, line.B.X, line.B.Y);
                return unit;
            }), what: "line draw"),
            rectangleCase: static (s, r) => Op.Of(name: nameof(RectangleCase)).Attempt(body: () => Draw(scope: s, style: r.Style, run: graphics =>
                DrawShape(graphics: graphics, style: r.Style, shape: PaintShape.Rectangle(bounds: r.Bounds))), what: "rectangle draw"),
            roundedRectangleCase: static (s, r) => Op.Of(name: nameof(RoundedRectangleCase)).Attempt(body: () => Draw(scope: s, style: r.Style, run: graphics => {
                using IGraphicsPath path = GraphicsPath.GetRoundRect(rectangle: r.Bounds, radius: r.Radius);
                return DrawShape(graphics: graphics, style: r.Style, shape: PaintShape.Path(path: path));
            }), what: "rounded rectangle draw"),
            roundedCornersCase: static (s, c) => Op.Of(name: nameof(RoundedCornersCase)).Attempt(body: () => Draw(scope: s, style: c.Style, run: graphics => {
                using IGraphicsPath path = c.Radii.IsUniform
                    ? GraphicsPath.GetRoundRect(rectangle: c.Bounds, radius: c.Radii.TopLeft)
                    : GraphicsPath.GetRoundRect(
                        rectangle: c.Bounds,
                        nwRadius: c.Radii.TopLeft,
                        neRadius: c.Radii.TopRight,
                        seRadius: c.Radii.BottomRight,
                        swRadius: c.Radii.BottomLeft);
                return DrawShape(graphics: graphics, style: c.Style, shape: PaintShape.Path(path: path));
            }), what: "rounded corners draw"),
            ellipseCase: static (s, e) => Op.Of(name: nameof(EllipseCase)).Attempt(body: () => Draw(scope: s, style: e.Style, run: graphics =>
                DrawShape(graphics: graphics, style: e.Style, shape: PaintShape.Ellipse(bounds: e.Bounds))), what: "ellipse draw"),
            pathCase: static (s, p) => Op.Of(name: nameof(PathCase)).Attempt(body: () => Draw(scope: s, style: p.Style, run: graphics =>
                DrawShape(graphics: graphics, style: p.Style, shape: PaintShape.Path(path: p.Geometry))), what: "path draw"),
            textCase: static (s, t) => Op.Of(name: nameof(TextCase)).Attempt(body: () => Draw(scope: s, style: t.Style, run: graphics =>
                t.Style.Font.IfNone(UiFont.Empty()).Use(run: font => {
                    using SolidBrush brush = PaintStyle.Brush(color: t.Style.Edge);
                    graphics.DrawText(font, brush, t.Frame, t.Value, t.Wrap, t.Alignment, t.Trimming);
                    return unit;
                })), what: "text draw"),
            imageCase: static (s, i) => Op.Of(name: nameof(ImageCase)).Attempt(body: () => Draw(scope: s, style: i.Style, run: graphics => {
                graphics.DrawImage(image: i.Value, rectangle: i.Frame);
                return unit;
            }), what: "image draw"),
            ghIconCase: static (s, g) => Op.Of(name: nameof(GhIconCase)).Attempt(body: () => Draw(scope: s, style: g.Style, run: _ => {
                g.Value.Draw(context: new IconContext(
                    context: Eto.Drawing.Context.CreateFromContent(graphics: s.Graphics),
                    frame: g.Frame,
                    background: g.Style.Background));
                return unit;
            }), what: "icon draw"),
            wireCase: static (s, w) => Op.Of(name: nameof(WireCase)).Attempt(body: () => DrawWire(scope: s, wire: w), what: "wire draw"));

    private static Unit Draw(PaintScope scope, PaintStyle style, Func<Graphics, Unit> run) {
        Graphics graphics = scope.Graphics.Content;
        using IDisposable state = graphics.SaveTransformState();
        _ = style.Assign(graphics: graphics);
        return run(arg: graphics);
    }

    private static Unit DrawShape(Graphics graphics, PaintStyle style, PaintShape shape) {
        _ = style.Fill.IfSome(fill => {
            using SolidBrush brush = PaintStyle.Brush(color: fill);
            shape.Fill(graphics, brush);
        });
        using Pen pen = style.Pen();
        shape.Stroke(graphics, pen);
        return unit;
    }

    private readonly record struct PaintShape(Action<Graphics, SolidBrush> Fill, Action<Graphics, Pen> Stroke) {
        internal static PaintShape Rectangle(RectangleF bounds) =>
            new(Fill: (graphics, brush) => graphics.FillRectangle(brush: brush, rectangle: bounds),
                Stroke: (graphics, pen) => graphics.DrawRectangle(pen: pen, rectangle: bounds));

        internal static PaintShape Ellipse(RectangleF bounds) =>
            new(Fill: (graphics, brush) => graphics.FillEllipse(brush: brush, rectangle: bounds),
                Stroke: (graphics, pen) => graphics.DrawEllipse(pen: pen, rectangle: bounds));

        internal static PaintShape Path(IGraphicsPath path) =>
            new(Fill: (graphics, brush) => graphics.FillPath(brush: brush, path: path),
                Stroke: (graphics, pen) => graphics.DrawPath(pen: pen, path: path));
    }

    private static Unit DrawWire(PaintScope scope, WireCase wire) {
        Graphics graphics = scope.Graphics.Content;
        using IDisposable state = graphics.SaveTransformState();
        _ = wire.Style.Assign(graphics: graphics);
        WireSkin skin = scope.Skin.Wires[wire.Kind];
        WireShape shape = WireShape.Create(source: wire.Source, target: wire.Target);
        using Pen outerPen = new(color: skin.Normal, thickness: skin.Outer.Width);
        skin.Outer.AssignToPen(pen: outerPen);
        shape.Draw(graphics: graphics, edge: outerPen);
        _ = Optional(skin.Inner).Iter(inner => {
            using Pen innerPen = new(color: skin.Normal, thickness: inner.Width);
            inner.AssignToPen(pen: innerPen);
            shape.Draw(graphics: graphics, edge: innerPen);
        });
        return unit;
    }
}

public readonly record struct DrawPlan(Seq<DrawMark> Marks) {
    public static DrawPlan Empty => new(Marks: Seq<DrawMark>());
    public static DrawPlan operator +(DrawPlan left, DrawPlan right) => new(Marks: left.Marks + right.Marks);
    public static DrawPlan Add(DrawPlan left, DrawPlan right) => left + right;
    internal Fin<Unit> Apply(PaintScope scope) => Marks.TraverseM(scope.Apply).Map(static marks => unit).As();
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintSkinSnapshot(bool HasSkin, string SkinType, Option<Skin> Skin);

public abstract record PaintRequest<T> : GhUiRequest<T> {
    public sealed record Skin : PaintRequest<PaintSkinSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<PaintSkinSnapshot> Apply(GrasshopperUi.Scope scope) => Paint.Skin().Run(scope: scope);
    }
    public sealed record Hook(CanvasPaintPhase Phase, DrawPlan Plan, MotionClock? Clock = null) : PaintRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Paint.Hook(phase: Phase, paint: Plan.Apply, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope);
    }
    public sealed record RedrawOnMouseMove(bool Enabled = true, MotionClock? Clock = null) : PaintRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas);
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Paint.RedrawOnMouseMove(enabled: Enabled, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope);
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Paint {
    internal static GrasshopperUiIntent<PaintSkinSnapshot> Skin() =>
        GhUi.Canvas(run: scope => scope.NeedSkin().Map(skin =>
            new PaintSkinSnapshot(
                HasSkin: true,
                SkinType: skin.GetType().FullName ?? skin.GetType().Name,
                Skin: Some(skin))));

    // Paint.Hook pairs Resume/Pause symmetrically inside attach/detach (no PauseWhenFinished
    // equivalent like Motion.* methods have), so the Pacer's `active` counter stays balanced
    // when Paint hooks share a Canvas with Motion runners.
    internal static GrasshopperUiIntent<Subscription> Hook(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(paint).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null paint callback"))
            from validPhase in Optional(phase).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null phase"))
            from pacer in Motion.PacerOption(canvas: canvas, clock: clock)
            let handler = (EventHandler<CanvasPaintEventArgs>)((_, args) => GrasshopperUi.Protect(valid: () => valid(arg: new PaintScope(
                Phase: validPhase,
                Graphics: args.Graphics,
                Skin: args.Skin) {
                Background = Optional(args as CanvasBackgroundPaintEventArgs),
            })).Ignore())
            from sub in Subscription.Bind(
                attach: () => {
                    _ = validPhase.Attach(canvas: canvas, handler: handler);
                    _ = pacer is { IsSome: true, Case: Motion.Pacer p } ? p.Resume() : Op.Side(canvas.ScheduleRedraw);
                },
                detach: () => {
                    _ = validPhase.Detach(canvas: canvas, handler: handler);
                    _ = pacer is { IsSome: true, Case: Motion.Pacer p } ? Op.Side(() => { _ = p.Pause(); _ = p.Release(); }) : unit;
                },
                marshalToUi: true)
            select sub);

    // RedrawOnMouseMove + Pacer coordination: when MotionClock.DisplayLink, the returned Subscription
    // holds a Pacer Resume for the mouse-move pump lifetime, ensuring vsync alignment between paint
    // events triggered by motion runners and those triggered by mouse moves. MessageLoop path is a
    // simple flag toggle returning Subscription.Empty. Symmetric detach restores prior toggle state.
    internal static GrasshopperUiIntent<Subscription> RedrawOnMouseMove(bool enabled = true, MotionClock? clock = null) =>
        GhUi.Canvas(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                from canvas in scope.NeedCanvas()
                from pacer in Motion.PacerOption(canvas: canvas, clock: clock ?? MotionClock.MessageLoop)
                let priorEnabled = canvas.RedrawOnMouseMove
                from sub in Subscription.Bind(
                    attach: () => {
                        canvas.RedrawOnMouseMove = enabled;
                        _ = pacer is { IsSome: true, Case: Motion.Pacer p } ? p.Resume() : unit;
                    },
                    detach: () => {
                        canvas.RedrawOnMouseMove = priorEnabled;
                        _ = pacer is { IsSome: true, Case: Motion.Pacer p } ? Op.Side(() => { _ = p.Pause(); _ = p.Release(); }) : unit;
                    },
                    marshalToUi: true)
                select sub);
}
