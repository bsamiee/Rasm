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

    public Fin<PaintTextMeasurement> MeasureText(string value, Option<UiFont> font = default) {
        ControlGraphics graphics = Graphics;
        return Optional(value)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(MeasureText)), detail: "text is required"))
            .Bind(valid => Try.lift(f: () =>
                font.IfNone(UiFont.Empty()).Use(run: resolved => {
                    SizeF size = graphics.Content.MeasureString(font: resolved, text: valid);
                    return new PaintTextMeasurement(Text: valid, Size: size);
                })).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(MeasureText)), detail: error.Message)));
    }
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

    internal T Use<T>(Func<Font, T> run) {
        T UseFamily(FamilyCase family) {
            using Font owned = new(family: family.Name, size: family.Size, style: family.Style, decoration: family.Decoration);
            return run(arg: owned);
        }
        return this switch {
            SystemCase system => run(arg: SystemFonts.Cached(systemFont: system.Kind, size: system.Size, decoration: system.Decoration)),
            FamilyCase family => UseFamily(family: family),
            NativeCase native => run(arg: native.Value),
            _ => run(arg: SystemFonts.Default()),
        };
    }
}

// Polymorphic fill source. Solid uses Brushes.Cached for hot-path reuse; gradient/texture cases
// build a new Brush per draw since each carries unique positioning. Each case owns its brush
// construction via CreateBrush — DrawMark callers see one uniform surface.
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

    // Solid path returns the cached singleton; gradient/texture paths allocate fresh. Callers must
    // never Dispose the returned Brush when the source is Solid — the cache owns the lifetime.
    internal Brush CreateBrush() =>
        Switch<Brush>(
            solidCase: static s => Brushes.Cached(color: s.Colour),
            linearCase: static l => new LinearGradientBrush(startColor: l.Start, endColor: l.End, startPoint: l.From, endPoint: l.To) { Wrap = l.Wrap },
            radialCase: static r => new RadialGradientBrush(startColor: r.Centre, endColor: r.Edge, center: r.Origin, gradientOrigin: r.Focus, radius: r.Radius) { Wrap = r.Wrap },
            textureCase: static t => new TextureBrush(image: t.Source, opacity: t.Opacity));

    internal bool IsCached => this is SolidCase;
}

// Polymorphic edge source. Solid case uses Pens.Cached when the configured pen is vanilla
// (Eto.Drawing.Pen ctor defaults: PenLineCap.Square, PenLineJoin.Miter, MiterLimit 10).
[Union]
public partial record EdgeSource {
    private EdgeSource() { }
    public sealed record SolidCase(Color Colour) : EdgeSource;
    public sealed record BrushBackedCase(FillSource Source) : EdgeSource;

    public static EdgeSource Solid(Color colour) => new SolidCase(Colour: colour);
    public static EdgeSource FromFill(FillSource source) => new BrushBackedCase(Source: source);

    // Returns a Pen with the given thickness + dash. For Solid+vanilla cap/join the cached singleton
    // is returned; otherwise a fresh Pen is allocated (caller must Dispose via using).
    internal Pen CreatePen(float thickness, PenLineCap cap, PenLineJoin join, float miterLimit, DashStyle dash) {
        bool vanilla = cap == PenLineCap.Square && join == PenLineJoin.Miter && miterLimit == 10f;
        return Switch(
            solidCase: s => vanilla
                ? Pens.Cached(color: s.Colour, thickness: thickness, dashStyle: dash)
                : new Pen(color: s.Colour, thickness: thickness) { LineCap = cap, LineJoin = join, MiterLimit = miterLimit, DashStyle = dash },
            brushBackedCase: b => new Pen(brush: b.Source.CreateBrush(), thickness: thickness) { LineCap = cap, LineJoin = join, MiterLimit = miterLimit, DashStyle = dash });
    }

    internal bool IsCached(PenLineCap cap, PenLineJoin join, float miterLimit) =>
        this is SolidCase && cap == PenLineCap.Square && join == PenLineJoin.Miter && miterLimit == 10f;
}

// Per-corner rounded rectangle radii (clockwise from top-left).
[StructLayout(LayoutKind.Auto)]
public readonly record struct CornerRadii(float TopLeft, float TopRight, float BottomRight, float BottomLeft) {
    public static CornerRadii Uniform(float radius) => new(TopLeft: radius, TopRight: radius, BottomRight: radius, BottomLeft: radius);
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

    internal Fin<Unit> Apply(PaintScope scope) =>
        Try.lift(f: () => this switch {
            LineCase line => Draw(scope: scope, style: line.Style, run: graphics => {
                using Pen pen = line.Style.Pen();
                graphics.DrawLine(pen, line.A.X, line.A.Y, line.B.X, line.B.Y);
                return unit;
            }),
            RectangleCase rectangle => Draw(scope: scope, style: rectangle.Style, run: graphics => {
                PaintShape shape = PaintShape.Rectangle(bounds: rectangle.Bounds);
                return DrawShape(graphics: graphics, style: rectangle.Style, shape: shape);
            }),
            RoundedRectangleCase rounded => Draw(scope: scope, style: rounded.Style, run: graphics => {
                using IGraphicsPath path = GraphicsPath.GetRoundRect(rectangle: rounded.Bounds, radius: rounded.Radius);
                PaintShape shape = PaintShape.Path(path: path);
                return DrawShape(graphics: graphics, style: rounded.Style, shape: shape);
            }),
            RoundedCornersCase corners => Draw(scope: scope, style: corners.Style, run: graphics => {
                using IGraphicsPath path = corners.Radii.IsUniform
                    ? GraphicsPath.GetRoundRect(rectangle: corners.Bounds, radius: corners.Radii.TopLeft)
                    : GraphicsPath.GetRoundRect(
                        rectangle: corners.Bounds,
                        nwRadius: corners.Radii.TopLeft,
                        neRadius: corners.Radii.TopRight,
                        seRadius: corners.Radii.BottomRight,
                        swRadius: corners.Radii.BottomLeft);
                PaintShape shape = PaintShape.Path(path: path);
                return DrawShape(graphics: graphics, style: corners.Style, shape: shape);
            }),
            EllipseCase ellipse => Draw(scope: scope, style: ellipse.Style, run: graphics => {
                PaintShape shape = PaintShape.Ellipse(bounds: ellipse.Bounds);
                return DrawShape(graphics: graphics, style: ellipse.Style, shape: shape);
            }),
            PathCase path => Draw(scope: scope, style: path.Style, run: graphics => {
                PaintShape shape = PaintShape.Path(path: path.Geometry);
                return DrawShape(graphics: graphics, style: path.Style, shape: shape);
            }),
            TextCase text => Draw(scope: scope, style: text.Style, run: graphics =>
                text.Style.Font.IfNone(UiFont.Empty()).Use(run: font => {
                    using SolidBrush brush = PaintStyle.Brush(color: text.Style.Edge);
                    graphics.DrawText(font, brush, text.Frame, text.Value, text.Wrap, text.Alignment, text.Trimming);
                    return unit;
                })),
            ImageCase image => Draw(scope: scope, style: image.Style, run: graphics => {
                graphics.DrawImage(image: image.Value, rectangle: image.Frame);
                return unit;
            }),
            GhIconCase icon => Draw(scope: scope, style: icon.Style, run: _ => {
                icon.Value.Draw(context: new IconContext(
                    context: Eto.Drawing.Context.CreateFromContent(graphics: scope.Graphics),
                    frame: icon.Frame,
                    background: icon.Style.Background));
                return unit;
            }),
            WireCase wire => DrawWire(scope: scope, wire: wire),
            _ => unit,
        }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(DrawMark)), detail: $"{GetType().Name} draw failed: {error.Message}"));

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
    public sealed record Hook(CanvasPaintPhase Phase, DrawPlan Plan) : PaintRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Paint.Hook(phase: Phase, paint: Plan.Apply).Run(scope: scope);
    }
    public sealed record RedrawOnMouseMove(bool Enabled = true) : PaintRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas);
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Paint.RedrawOnMouseMove(enabled: Enabled).Run(scope: scope);
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

    internal static GrasshopperUiIntent<Subscription> Hook(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(paint).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null paint callback"))
            from validPhase in Optional(phase).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null phase"))
            let handler = (EventHandler<CanvasPaintEventArgs>)((_, args) => GrasshopperUi.Protect(valid: () => valid(arg: new PaintScope(
                Phase: validPhase,
                Graphics: args.Graphics,
                Skin: args.Skin) {
                Background = Optional(args as CanvasBackgroundPaintEventArgs),
            })).Ignore())
            from sub in Subscription.Bind(
                attach: () => validPhase.Attach(canvas: canvas, handler: handler),
                detach: () => validPhase.Detach(canvas: canvas, handler: handler),
                marshalToUi: true)
            select sub);

    internal static GrasshopperUiIntent<Unit> RedrawOnMouseMove(bool enabled = true) =>
        GhUi.Canvas(
            repaint: RepaintRequest.Canvas,
            run: scope => scope.NeedCanvas().Map(canvas => { canvas.RedrawOnMouseMove = enabled; return unit; }));
}
