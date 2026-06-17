using System.Runtime.CompilerServices;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
// Theme-reactive: live Skin supplies GH2 tokens; Eto fills unmapped or pre-canvas colours. Each item is one
// (source, fallback) policy row — shade-keyed and canvas-keyed sources collapse to two factories, LinkText has no skin token.
[SmartEnum<int>]
public sealed partial class SystemColorKind {
    private delegate Color ColorSource(Option<Skin> skin);

    public static readonly SystemColorKind ControlText = ShadeBased(key: 0, shade: ShadeKind.Normal, component: static s => s.Text, fallback: SystemColors.ControlText);
    public static readonly SystemColorKind Control = ShadeBased(key: 1, shade: ShadeKind.Normal, component: static s => s.Slab, fallback: SystemColors.Control);
    public static readonly SystemColorKind ControlBackground = ShadeBased(key: 2, shade: ShadeKind.Normal, component: static s => s.Apex, fallback: SystemColors.ControlBackground);
    public static readonly SystemColorKind WindowBackground = CanvasBased(key: 3, component: static c => c.Background, fallback: SystemColors.WindowBackground);
    public static readonly SystemColorKind Highlight = ShadeBased(key: 4, shade: ShadeKind.NormalSelected, component: static s => s.Apex, fallback: SystemColors.Highlight);
    public static readonly SystemColorKind HighlightText = ShadeBased(key: 5, shade: ShadeKind.NormalSelected, component: static s => s.Text, fallback: SystemColors.HighlightText);
    public static readonly SystemColorKind Selection = ShadeBased(key: 6, shade: ShadeKind.NormalSelected, component: static s => s.Slab, fallback: SystemColors.Selection);
    public static readonly SystemColorKind SelectionText = ShadeBased(key: 7, shade: ShadeKind.NormalSelected, component: static s => s.TextAlternate, fallback: SystemColors.SelectionText);
    public static readonly SystemColorKind DisabledText = ShadeBased(key: 8, shade: ShadeKind.Disabled, component: static s => s.Text, fallback: SystemColors.DisabledText);
    public static readonly SystemColorKind LinkText = new(key: 9, resolve: static _ => SystemColors.LinkText);
    public static readonly SystemColorKind NormalEdge = ShadeBased(key: 10, shade: ShadeKind.Normal, component: static s => s.Edge, fallback: SystemColors.ControlText);
    public static readonly SystemColorKind NormalGlint = ShadeBased(key: 11, shade: ShadeKind.Normal, component: static s => s.Apex, fallback: SystemColors.ControlBackground);
    public static readonly SystemColorKind DisabledBackground = ShadeBased(key: 12, shade: ShadeKind.Disabled, component: static s => s.Slab, fallback: SystemColors.Control);
    public static readonly SystemColorKind HiddenText = ShadeBased(key: 13, shade: ShadeKind.Hidden, component: static s => s.Text, fallback: SystemColors.DisabledText);
    public static readonly SystemColorKind HiddenEdge = ShadeBased(key: 14, shade: ShadeKind.Hidden, component: static s => s.Edge, fallback: SystemColors.ControlText);
    public static readonly SystemColorKind TentativeText = ShadeBased(key: 15, shade: ShadeKind.Tentative, component: static s => s.Text, fallback: SystemColors.ControlText);
    public static readonly SystemColorKind CanvasForeground = CanvasBased(key: 16, component: static c => c.Foreground, fallback: SystemColors.ControlText);
    public static readonly SystemColorKind CanvasEdge = CanvasBased(key: 17, component: static c => c.Edge, fallback: SystemColors.ControlText);
    public static readonly SystemColorKind CanvasShadow = CanvasBased(key: 18, component: static c => c.Shadow, fallback: SystemColors.ControlText);

    [UseDelegateFromConstructor]
    internal partial Color Resolve(Option<Skin> skin);

    // Shade-keyed rows read s.Shades[shade].component; canvas-keyed rows read s.Canvasses[Normal].component; both fall to an Eto colour pre-skin.
    private static SystemColorKind ShadeBased(int key, ShadeKind shade, Func<Shade, Color> component, Color fallback) =>
        new(key: key, resolve: skin => skin.Match(Some: s => component(arg: s.Shades[shade]), None: () => fallback));

    private static SystemColorKind CanvasBased(int key, Func<Grasshopper2.UI.Skinning.CanvasSkin, Color> component, Color fallback) =>
        new(key: key, resolve: skin => skin.Match(Some: s => component(arg: s.Canvasses[CanvasKind.Normal]), None: () => fallback));
}

// Mirrors host IconContext filters: disabled, greyscale, fade, or identity.
[SmartEnum<int>]
public sealed partial class IconAdjust {
    private const float FadeFactor = 0.5f;
    private delegate IconContext AdjustSource(IconContext context);

    public static readonly IconAdjust None = new(key: 0, apply: static context => context);
    public static readonly IconAdjust Disabled = new(key: 1, apply: static context => context.WithDisabledFilter());
    public static readonly IconAdjust Greyscale = new(key: 2, apply: static context => context.WithGreyscaleFilter());
    public static readonly IconAdjust Faded = new(key: 3, apply: static context => context.WithFadingFilter(background: context.Background, factor: FadeFactor));

    [UseDelegateFromConstructor]
    internal partial IconContext Apply(IconContext context);
}

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

    private static CanvasPaintPhase Phase(int key, Action<GhCanvas, EventHandler<CanvasPaintEventArgs>> attach, Action<GhCanvas, EventHandler<CanvasPaintEventArgs>> detach) =>
        new(
            key: key,
            attach: (c, h) => { attach(arg1: c, arg2: h); return unit; },
            detach: (c, h) => { detach(arg1: c, arg2: h); return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Attach(GhCanvas canvas, EventHandler<CanvasPaintEventArgs> handler);

    [UseDelegateFromConstructor]
    internal partial Unit Detach(GhCanvas canvas, EventHandler<CanvasPaintEventArgs> handler);
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintScope(CanvasPaintPhase Phase, ControlGraphics Graphics, Skin Skin) {
    internal Option<CanvasBackgroundPaintEventArgs> Background { get; init; }

    internal bool VisibilityCulling { get; init; } = true;

    internal bool ClipActive { get; init; }

    public bool DefaultBackgroundOverridden =>
        Background.Map(static args => args.DefaultOverridden).IfNone(noneValue: false);

    public Unit OverrideDefault() =>
        Background.Match(
            Some: args => { args.OverrideDefaultPainting(); return unit; },
            None: () => unit);

    public float PointsPerPixel => Graphics.Content.PointsPerPixel > 0f ? Graphics.Content.PointsPerPixel : 1f;

    public float Dpi => Graphics.Content.DPI;

    public UiPixelScale PixelScale => new(
        LogicalPixelSize: Graphics.ScreenScale > 0f ? Graphics.ScreenScale : 1f,
        PointsPerPixel: PointsPerPixel,
        FromPaintGraphics: true);

    public bool IsVisible(RectangleF bounds) => Graphics.Content.IsVisible(rectangle: bounds);

    public Fin<Unit> Apply(DrawMark mark) {
        PaintScope current = this;
        return Optional(mark)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Apply)), detail: "draw mark is required"))
            .Bind(valid => valid.Apply(scope: current));
    }

    // TextMetricsCache-routed: Eto.Mac 2.11 GraphicsHandler.MeasureString leaks FormattedText state across calls.
    // None maxSize is the unbounded default (SizeF is not const-expressible as a default parameter).
    public Fin<PaintTextMeasurement> MeasureText(
        string value,
        Option<UiFont> font = default,
        FormattedTextWrapMode wrap = FormattedTextWrapMode.None,
        FormattedTextAlignment alignment = FormattedTextAlignment.Left,
        FormattedTextTrimming trimming = FormattedTextTrimming.None,
        Option<SizeF> maxSize = default) =>
        Optional(value)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(MeasureText)), detail: "text is required"))
            .Bind(valid => Try.lift(f: () =>
                font.IfNone(UiFont.Empty()).Use(run: resolved =>
                    new PaintTextMeasurement(
                        Text: valid,
                        Size: TextMetricsCache.Measure(
                            font: resolved,
                            text: valid,
                            wrap: wrap,
                            alignment: alignment,
                            trimming: trimming,
                            maxSize: maxSize.IfNone(() => new SizeF(width: float.MaxValue, height: float.MaxValue))))
                )).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(MeasureText)), detail: error.Message)));

    // SaveTransformState does NOT capture clipping; ResetClip in finally is mandatory. One polymorphic ClipGeometry
    // discriminates path versus rect; both reject nesting (Eto restores no clip stack and has no path-intersect primitive).
    public Fin<Unit> Clip(ClipGeometry region, Func<PaintScope, Fin<Unit>> body) {
        PaintScope self = this;
        Graphics graphics = self.Graphics.Content;
        return Optional(region)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Clip)), detail: "clip region is required"))
            .Bind(valid => self.ClipActive
                ? Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Clip)), detail: "nested clipped draw marks are not supported by Eto clip restoration"))
                : DoClip(scope: self, graphics: graphics, body: body, setClip: () => valid.SetClip(graphics: graphics)));
    }

    // One cull predicate: culling disabled, degenerate bounds, or in-viewport runs the draw; otherwise succeeds idle.
    internal Fin<Unit> WhenVisible(RectangleF bounds, Func<Fin<Unit>> draw) =>
        !VisibilityCulling || bounds == RectangleF.Empty || IsVisible(bounds: bounds)
            ? draw()
            : Fin.Succ(unit);

    // BOUNDARY ADAPTER -- Eto clip restore: SetClip has no stack, so ResetClip MUST run in finally. Static so the
    // body delegate closes over the passed graphics/scope locals, never the struct `this` (CS1673).
    private static Fin<Unit> DoClip(PaintScope scope, Graphics graphics, Func<PaintScope, Fin<Unit>> body, Action setClip) =>
        Optional(body)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Clip)), detail: "body is required"))
            .Bind(validBody => {
                setClip();
                try {
                    return validBody(arg: scope with { ClipActive = true });
                } finally {
                    graphics.ResetClip();
                }
            });
}

// Clip region as a closed family: a path clip or an optionally-intersecting rect clip. The discriminant is the value
// shape, never a name suffix; both apply through Eto's stackless SetClip at a single depth.
[Union]
public abstract partial record ClipGeometry {
    private ClipGeometry() { }
    public sealed record PathCase(IGraphicsPath Geometry) : ClipGeometry;
    public sealed record RectCase(RectangleF Bounds, bool Intersect = false) : ClipGeometry;

    public static ClipGeometry Path(IGraphicsPath geometry) => new PathCase(Geometry: geometry);
    public static ClipGeometry Rect(RectangleF bounds, bool intersect = false) => new RectCase(Bounds: bounds, Intersect: intersect);

    public static implicit operator ClipGeometry(RectangleF bounds) => new RectCase(Bounds: bounds);

    // BOUNDARY ADAPTER -- Eto SetClip: the rect arm intersects against the live ClipBounds when requested; otherwise replaces.
    internal Unit SetClip(Graphics graphics) =>
        Switch(state: graphics,
            pathCase: static (g, p) => { g.SetClip(path: p.Geometry); return unit; },
            rectCase: static (g, r) => { g.SetClip(rectangle: r.Intersect ? RectangleF.Intersect(g.ClipBounds, r.Bounds) : r.Bounds); return unit; });
}

[ComplexValueObject]
[ValidationError<UiFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct PaintStyle {
    public Color Edge { get; }
    public Option<Color> Fill { get; }
    public float Thickness { get; }
    public Option<UiFont> Font { get; }
    public Color Background { get; }
    public PenLineCap LineCap { get; }
    public PenLineJoin LineJoin { get; }
    public Option<DashStyle> Dash { get; }
    public float MiterLimit { get; }
    public bool AntiAlias { get; }
    public ImageInterpolation ImageInterpolation { get; }
    public PixelOffsetMode PixelOffset { get; }
    public float DashOffset { get; }
    public Option<FillSource> FillBrush { get; }
    public Option<FillSource> EdgeBrush { get; }

    private const float MiterLimitDefault = 10f;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref UiFault? validationError,
        ref Color edge,
        ref Option<Color> fill,
        ref float thickness,
        ref Option<UiFont> font,
        ref Color background,
        ref PenLineCap lineCap,
        ref PenLineJoin lineJoin,
        ref Option<DashStyle> dash,
        ref float miterLimit,
        ref bool antiAlias,
        ref ImageInterpolation imageInterpolation,
        ref PixelOffsetMode pixelOffset,
        ref float dashOffset,
        ref Option<FillSource> fillBrush,
        ref Option<FillSource> edgeBrush) {
        Op op = Op.Of(name: nameof(PaintStyle));
        float thicknessValue = thickness;
        float miterLimitValue = miterLimit;
        float dashOffsetValue = dashOffset;
        Fin<Unit> valid =
            from t in op.AcceptFinite(value: thicknessValue, detail: $"Thickness must be finite and >= 0 (got {thicknessValue:R}).", nonNegative: true)
            from d in op.AcceptFinite(value: dashOffsetValue, detail: $"DashOffset must be finite (got {dashOffsetValue:R}).")
            from m in op.AcceptFinite(value: miterLimitValue, detail: $"MiterLimit must be finite and >= 1 (got {miterLimitValue:R}).", min: 1f)
            select unit;
        UiFault? fault = null;
        _ = valid.IfFail(err => { fault = (UiFault)err; return unit; });
        _ = (edge, fill, font, background, lineCap, lineJoin, dash, antiAlias, imageInterpolation, pixelOffset, fillBrush, edgeBrush);
        validationError = fault;
    }

    public bool HasVisibleStroke => EdgeBrush.IsSome || Edge.A > 0f;

    internal static PaintStyle Style(Color edge, Option<Color> fill = default, float thickness = 1f, Option<UiFont> font = default, Color background = default) =>
        Create(
            edge: edge,
            fill: fill,
            thickness: thickness,
            font: font,
            background: background,
            lineCap: PenLineCap.Butt,
            lineJoin: PenLineJoin.Miter,
            dash: default,
            miterLimit: MiterLimitDefault,
            antiAlias: true,
            imageInterpolation: ImageInterpolation.Default,
            pixelOffset: PixelOffsetMode.None,
            dashOffset: 0f,
            fillBrush: default,
            edgeBrush: default);

    internal static PaintStyle ForSystemColor(SystemColorKind kind, Option<Skin> skin = default, Option<Color> fill = default, float thickness = 1f) =>
        Style(edge: kind.Resolve(skin: skin), fill: fill, thickness: thickness);

    internal static SolidBrush Brush(Color color) => new(color: color);

    // Eto 2.11 lacks Pen.DashOffset and immutable DashStyle forces quantized interning; caller-owned Pen/Brush lifetimes stay intact.
    internal Pen Pen() {
        Color edgeColour = Edge;
        Brush brush = EdgeBrush.Map(static source => source.CachedBrush()).IfNone(() => new SolidBrush(color: edgeColour));
        return new Pen(brush: brush, thickness: Thickness) { LineCap = LineCap, LineJoin = LineJoin, MiterLimit = MiterLimit, DashStyle = OffsetDash() };
    }

    internal Unit Assign(Graphics graphics) {
        graphics.AntiAlias = AntiAlias;
        graphics.ImageInterpolation = ImageInterpolation;
        graphics.PixelOffsetMode = PixelOffset;
        return unit;
    }

    private DashStyle OffsetDash() {
        DashStyle baseline = Dash.IfNone(DashStyles.Solid);
        return DashOffset == 0f ? baseline : DashStyleCache.WithOffset(baseline: baseline, offset: DashOffset);
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintTextMeasurement(string Text, SizeF Size);

[SkipUnionOps]
[Union]
public partial record UiFont {
    private UiFont() { }
    public sealed record SystemCase(SystemFont Kind, float? Size = null, FontDecoration Decoration = FontDecoration.None) : UiFont;
    public sealed record FamilyCase(string Name, float Size, FontStyle Style = FontStyle.None, FontDecoration Decoration = FontDecoration.None) : UiFont;
    public sealed record NativeCase(Font Value) : UiFont;
    public sealed record EmptyCase : UiFont;

    // Font equality excludes Decoration; cache keys carry it and stay main-thread-only with AppKit.
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct SystemFontKey(SystemFont Kind, float? Size, FontDecoration Decoration);

    private static readonly BoundedCache<SystemFontKey, Font> SystemFontCache = new(capacity: 128, evict: static font => font.Dispose());

    private static Font CachedSystem(SystemFont systemFont, float? size, FontDecoration decoration) =>
        SystemFontCache.GetOrAdd(
            key: new SystemFontKey(Kind: systemFont, Size: size, Decoration: decoration),
            valueFactory: static k => new Font(systemFont: k.Kind, size: k.Size, decoration: k.Decoration));

    public static UiFont Empty() => new EmptyCase();
    public static UiFont System(SystemFont kind, float? size = null, FontDecoration decoration = FontDecoration.None) =>
        new SystemCase(Kind: kind, Size: size, Decoration: decoration);
    public static UiFont Family(string family, float size, FontStyle style = FontStyle.None, FontDecoration decoration = FontDecoration.None) =>
        new FamilyCase(Name: family, Size: size, Style: style, Decoration: decoration);
    public static UiFont Native(Font value) => new NativeCase(Value: value);

    // State-threading keeps union arms static; Family owns transient Font, other cases use cached/caller-owned fonts.
    internal T Use<T>(Func<Font, T> run) =>
        Switch(state: run,
            systemCase: static (r, s) => r(arg: CachedSystem(systemFont: s.Kind, size: s.Size, decoration: s.Decoration)),
            familyCase: static (r, f) => {
                using Font owned = new(family: f.Name, size: f.Size, style: f.Style, decoration: f.Decoration);
                return r(arg: owned);
            },
            nativeCase: static (r, n) => r(arg: n.Value),
            emptyCase: static (r, _) => r(arg: CachedSystem(systemFont: SystemFont.Default, size: null, decoration: FontDecoration.None)));
}

[SkipUnionOps]
[Union]
public partial record FillSource {
    private FillSource() { }
    public sealed record SolidCase(Color Colour) : FillSource;
    public sealed record LinearCase(Color Start, Color End, PointF From, PointF To, GradientWrapMode Wrap = GradientWrapMode.Pad, Option<IMatrix> Transform = default) : FillSource;
    public sealed record RadialCase(Color Centre, Color Edge, PointF Origin, PointF Focus, SizeF Radius, GradientWrapMode Wrap = GradientWrapMode.Pad, Option<IMatrix> Transform = default) : FillSource;
    public sealed record AngularCase(Color Start, Color End, RectangleF Rect, float Angle, GradientWrapMode Wrap = GradientWrapMode.Pad) : FillSource;
    public sealed record TextureCase(Image Source, float Opacity = 1f, Option<IMatrix> Transform = default) : FillSource;

    // Per-frame fill reuses one inert Eto brush per FillSource; texture keys use image identity, other cases use structural keys.
    private static readonly BoundedCache<FillSource, Brush> BrushCache = new(
        capacity: 256,
        evict: static brush => brush.Dispose(),
        comparer: EqualityComparer<FillSource>.Create(
            equals: static (x, y) => (x, y) switch {
                (TextureCase a, TextureCase b) => ReferenceEquals(a.Source, b.Source) && a.Opacity == b.Opacity,
                (null, null) => true,
                (null, _) or (_, null) => false,
                _ => x.Equals(y),
            },
            getHashCode: static s => s switch {
                TextureCase texture => HashCode.Combine(RuntimeHelpers.GetHashCode(texture.Source), texture.Opacity),
                _ => s.GetHashCode(),
            }));

    public static FillSource Solid(Color colour) => new SolidCase(Colour: colour);
    public static FillSource Linear(Color start, Color end, PointF from, PointF to, GradientWrapMode wrap = GradientWrapMode.Pad, Option<IMatrix> transform = default) =>
        new LinearCase(Start: start, End: end, From: from, To: to, Wrap: wrap, Transform: transform);
    public static FillSource Radial(Color centre, Color edge, PointF origin, PointF focus, SizeF radius, GradientWrapMode wrap = GradientWrapMode.Pad, Option<IMatrix> transform = default) =>
        new RadialCase(Centre: centre, Edge: edge, Origin: origin, Focus: focus, Radius: radius, Wrap: wrap, Transform: transform);
    // Angle-keyed linear gradient over a rect (Eto's LinearGradientBrush(rect,start,end,angle) ctor).
    public static FillSource Angular(Color start, Color end, RectangleF rect, float angle, GradientWrapMode wrap = GradientWrapMode.Pad) =>
        new AngularCase(Start: start, End: end, Rect: rect, Angle: angle, Wrap: wrap);
    public static FillSource Texture(Image source, float opacity = 1f, Option<IMatrix> transform = default) =>
        new TextureCase(Source: source, Opacity: opacity, Transform: transform);

    // BOUNDARY ADAPTER -- Eto brush transform: ITransformBrush.Transform is mutable post-construction; each
    // gradient/texture arm applies the caller's Some transform to the just-built brush before returning.
    internal Brush CreateBrush() =>
        Switch(
            solidCase: static s => new SolidBrush(color: s.Colour),
            linearCase: static l => Transformed(new LinearGradientBrush(startColor: l.Start, endColor: l.End, startPoint: l.From, endPoint: l.To) { Wrap = l.Wrap }, l.Transform),
            radialCase: static r => Transformed(new RadialGradientBrush(startColor: r.Centre, endColor: r.Edge, center: r.Origin, gradientOrigin: r.Focus, radius: r.Radius) { Wrap = r.Wrap }, r.Transform),
            angularCase: static a => new LinearGradientBrush(rectangle: a.Rect, startColor: a.Start, endColor: a.End, angle: a.Angle) { Wrap = a.Wrap },
            textureCase: static t => Transformed(new TextureBrush(image: t.Source, opacity: t.Opacity), t.Transform));

    private static Brush Transformed(Brush brush, Option<IMatrix> transform) =>
        transform.Match(
            Some: matrix => { ((ITransformBrush)brush).Transform = matrix; return brush; },
            None: () => brush);

    // A Some Transform carries a mutable/reference IMatrix unsafe as an equality key, so transformed brushes bypass
    // the structural BrushCache and build fresh; the None path stays cached.
    private bool Cacheable =>
        Switch(
            solidCase: static _ => true,
            linearCase: static l => l.Transform.IsNone,
            radialCase: static r => r.Transform.IsNone,
            angularCase: static _ => true,
            textureCase: static t => t.Transform.IsNone);

    internal Brush CachedBrush() => Cacheable ? BrushCache.GetOrAdd(key: this, valueFactory: static s => s.CreateBrush()) : CreateBrush();

}

[ComplexValueObject]
[ValidationError<UiFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct CornerRadii {
    public float TopLeft { get; }
    public float TopRight { get; }
    public float BottomRight { get; }
    public float BottomLeft { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref UiFault? validationError, ref float topLeft, ref float topRight, ref float bottomRight, ref float bottomLeft) {
        Op op = Op.Of(name: nameof(CornerRadii));
        float topLeftValue = topLeft;
        float topRightValue = topRight;
        float bottomRightValue = bottomRight;
        float bottomLeftValue = bottomLeft;
        Fin<Unit> valid =
            from tl in op.AcceptFinite(value: topLeftValue, detail: $"TopLeft must be finite and >= 0 (got {topLeftValue:R}).", nonNegative: true)
            from tr in op.AcceptFinite(value: topRightValue, detail: $"TopRight must be finite and >= 0 (got {topRightValue:R}).", nonNegative: true)
            from br in op.AcceptFinite(value: bottomRightValue, detail: $"BottomRight must be finite and >= 0 (got {bottomRightValue:R}).", nonNegative: true)
            from bl in op.AcceptFinite(value: bottomLeftValue, detail: $"BottomLeft must be finite and >= 0 (got {bottomLeftValue:R}).", nonNegative: true)
            select unit;
        UiFault? fault = null;
        _ = valid.IfFail(err => { fault = (UiFault)err; return unit; });
        validationError = fault;
    }

    public static CornerRadii Uniform(float radius) => Create(topLeft: radius, topRight: radius, bottomRight: radius, bottomLeft: radius);
    internal bool IsUniform => TopLeft == TopRight && TopLeft == BottomRight && TopLeft == BottomLeft;
}

[GenerateUnionOps]
[Union]
public partial record DrawMark {
    private DrawMark() { }
    public sealed partial record LineCase(PointF A, PointF B, PaintStyle Style) : DrawMark;
    public sealed partial record RectangleCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed partial record RoundedCornersCase(RectangleF Bounds, CornerRadii Radii, PaintStyle Style) : DrawMark;
    public sealed partial record EllipseCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed partial record PathCase(IGraphicsPath Geometry, PaintStyle Style) : DrawMark;
    public sealed partial record TextCase(string Value, RectangleF Frame, PaintStyle Style, FormattedTextWrapMode Wrap, FormattedTextAlignment Alignment, FormattedTextTrimming Trimming) : DrawMark;
    public sealed partial record ImageCase(Image Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed partial record GhIconCase(IIcon Value, RectangleF Frame, PaintStyle Style, IconAdjust Adjust) : DrawMark;
    public sealed partial record WireCase(PointF Source, PointF Target, WireKind Kind, PaintStyle Style, bool SourceSelected = false, bool TargetSelected = false) : DrawMark;
    public sealed partial record ArcCase(RectangleF Bounds, float StartAngle, float SweepAngle, PaintStyle Style, Option<SystemColorKind> SystemColor = default) : DrawMark;
    public sealed partial record PieCase(RectangleF Bounds, float StartAngle, float SweepAngle, PaintStyle Style) : DrawMark;
    public sealed partial record PolylineCase(ReadOnlyMemory<PointF> Points, PaintStyle Style) : DrawMark;
    public sealed partial record PolygonCase(ReadOnlyMemory<PointF> Points, PaintStyle Style) : DrawMark;
    public sealed partial record BezierCase(PointF Start, PointF Control1, PointF Control2, PointF End, PaintStyle Style) : DrawMark;
    public sealed partial record CurveCase(ReadOnlyMemory<PointF> Points, float Tension, PaintStyle Style) : DrawMark;
    public sealed partial record CapsuleCase(Capsule Value, Parts Elements, Shade Shade, Option<RectangleF> Bounds = default) : DrawMark;
    public sealed partial record ClippedCase(IGraphicsPath Clip, DrawPlan Plan) : DrawMark;
    public sealed partial record TransformedCase(IMatrix Transform, DrawPlan Plan) : DrawMark;

    public static DrawMark Line(PointF a, PointF b, Color colour, float thickness = 1f) =>
        new LineCase(A: a, B: b, Style: PaintStyle.Style(edge: colour, thickness: thickness));
    public static DrawMark Rectangle(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RectangleCase(Bounds: bounds, Style: PaintStyle.Style(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark RoundedRectangle(RectangleF bounds, float radius, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RoundedCornersCase(Bounds: bounds, Radii: CornerRadii.Uniform(radius: radius), Style: PaintStyle.Style(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark RoundedCorners(RectangleF bounds, CornerRadii radii, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RoundedCornersCase(Bounds: bounds, Radii: radii, Style: PaintStyle.Style(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark Ellipse(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new EllipseCase(Bounds: bounds, Style: PaintStyle.Style(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark Path(IGraphicsPath path, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new PathCase(Geometry: path, Style: PaintStyle.Style(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark Label(
        string value,
        RectangleF frame,
        Color colour,
        Option<UiFont> font = default,
        FormattedTextWrapMode wrap = FormattedTextWrapMode.Word,
        FormattedTextAlignment alignment = FormattedTextAlignment.Left,
        FormattedTextTrimming trimming = FormattedTextTrimming.WordEllipsis) =>
        new TextCase(Value: value, Frame: frame, Style: PaintStyle.Style(edge: colour, font: font), Wrap: wrap, Alignment: alignment, Trimming: trimming);
    public static DrawMark Image(Image value, RectangleF frame) =>
        new ImageCase(Value: value, Frame: frame, Style: PaintStyle.Style(edge: Colors.Transparent));
    public static DrawMark IconGlyph(IIcon value, RectangleF frame, Color background, IconAdjust? adjust = null) =>
        new GhIconCase(Value: value, Frame: frame, Style: PaintStyle.Style(edge: Colors.Transparent, background: background), Adjust: adjust ?? IconAdjust.None);
    public static DrawMark WirePreview(PointF source, PointF target, WireKind kind = WireKind.Tentative, bool sourceSelected = false, bool targetSelected = false) =>
        new WireCase(Source: source, Target: target, Kind: kind, Style: PaintStyle.Style(edge: Colors.Transparent), SourceSelected: sourceSelected, TargetSelected: targetSelected);
    public static DrawMark Arc(RectangleF bounds, float startAngle, float sweepAngle, Color edge, float thickness = 1f) =>
        new ArcCase(Bounds: bounds, StartAngle: startAngle, SweepAngle: sweepAngle, Style: PaintStyle.Style(edge: edge, thickness: thickness));
    public static DrawMark Pie(RectangleF bounds, float startAngle, float sweepAngle, Color fill, Option<Color> edge = default, float thickness = 1f) =>
        new PieCase(Bounds: bounds, StartAngle: startAngle, SweepAngle: sweepAngle, Style: PaintStyle.Style(edge: edge.IfNone(fill), fill: Some(fill), thickness: thickness));
    public static DrawMark Polyline(ReadOnlyMemory<PointF> points, Color edge, float thickness = 1f) =>
        new PolylineCase(Points: points, Style: PaintStyle.Style(edge: edge, thickness: thickness));
    public static DrawMark Polygon(ReadOnlyMemory<PointF> points, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new PolygonCase(Points: points, Style: PaintStyle.Style(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark Bezier(PointF start, PointF control1, PointF control2, PointF end, Color edge, float thickness = 1f) =>
        new BezierCase(Start: start, Control1: control1, Control2: control2, End: end, Style: PaintStyle.Style(edge: edge, thickness: thickness));
    public static DrawMark Curve(ReadOnlyMemory<PointF> points, Color edge, float tension = 0.5f, Option<Color> fill = default, float thickness = 1f) =>
        new CurveCase(Points: points, Tension: tension, Style: PaintStyle.Style(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark SystemArc(RectangleF bounds, float startAngle, float sweepAngle, SystemColorKind color, float thickness = 1f) {
        ArgumentNullException.ThrowIfNull(argument: color);
        return new ArcCase(Bounds: bounds, StartAngle: startAngle, SweepAngle: sweepAngle, Style: PaintStyle.ForSystemColor(kind: color, thickness: thickness), SystemColor: Some(color));
    }
    public static DrawMark DrawCapsule(Capsule capsule, Shade shade, Parts elements = Parts.All) =>
        new CapsuleCase(Value: capsule, Elements: elements, Shade: shade);
    public static DrawMark Clipped(IGraphicsPath clip, DrawPlan plan) => new ClippedCase(Clip: clip, Plan: plan);
    public static DrawMark Transformed(IMatrix transform, DrawPlan plan) => new TransformedCase(Transform: transform, Plan: plan);

    internal Fin<Unit> Apply(PaintScope scope) =>
        Switch(state: scope,
            // Shaped arms share the DrawMarked base: cull on bounds, save transform state, assign style, fill then stroke via one PaintShape.
            lineCase: static (s, c) => DrawMarked(scope: s, style: c.Style, op: Op.Of(name: nameof(LineCase)), what: "line draw", bounds: Some(BoundsOf(c.A, c.B)), shape: PaintShape.Line(a: c.A, b: c.B)),
            rectangleCase: static (s, c) => DrawMarked(scope: s, style: c.Style, op: Op.Of(name: nameof(RectangleCase)), what: "rectangle draw", bounds: Some(c.Bounds), shape: PaintShape.Rectangle(bounds: c.Bounds)),
            ellipseCase: static (s, c) => DrawMarked(scope: s, style: c.Style, op: Op.Of(name: nameof(EllipseCase)), what: "ellipse draw", bounds: Some(c.Bounds), shape: PaintShape.Ellipse(bounds: c.Bounds)),
            pathCase: static (s, c) => DrawMarked(scope: s, style: c.Style, op: Op.Of(name: nameof(PathCase)), what: "path draw", bounds: Some(c.Geometry.Bounds), shape: PaintShape.Path(path: c.Geometry)),
            polylineCase: static (s, c) => DrawMarked(scope: s, style: c.Style, op: Op.Of(name: nameof(PolylineCase)), what: "polyline draw", bounds: Some(BoundsOf(c.Points.Span)), shape: PaintShape.Polyline(points: c.Points)),
            polygonCase: static (s, c) => DrawMarked(scope: s, style: c.Style, op: Op.Of(name: nameof(PolygonCase)), what: "polygon draw", bounds: Some(BoundsOf(c.Points.Span)), shape: PaintShape.Polygon(points: c.Points)),
            arcCase: static (s, c) => DrawMarked(
                scope: s,
                // System-coloured arc re-resolves its edge against the live skin; a plain arc keeps its admitted style.
                style: c.SystemColor is { IsSome: true, Case: SystemColorKind kind } ? PaintStyle.ForSystemColor(kind: kind, skin: Some(s.Skin), thickness: c.Style.Thickness) : c.Style,
                op: Op.Of(name: nameof(ArcCase)),
                what: "arc draw",
                bounds: Some(c.Bounds),
                shape: PaintShape.Arc(bounds: c.Bounds, startAngle: c.StartAngle, sweepAngle: c.SweepAngle)),
            roundedCornersCase: static (s, c) => DrawMarked(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(RoundedCornersCase)),
                what: "rounded corners draw",
                bounds: Some(c.Bounds),
                path: () => c.Radii.IsUniform
                    ? GraphicsPath.GetRoundRect(rectangle: c.Bounds, radius: c.Radii.TopLeft)
                    : GraphicsPath.GetRoundRect(
                        rectangle: c.Bounds,
                        nwRadius: c.Radii.TopLeft,
                        neRadius: c.Radii.TopRight,
                        seRadius: c.Radii.BottomRight,
                        swRadius: c.Radii.BottomLeft)),
            // Pie wedge as one closed figure (centre -> arc start -> arc -> back to centre) so fill AND both
            // radii stroke together; FillPie + DrawArc could not stroke the radii.
            pieCase: static (s, c) => DrawMarked(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(PieCase)),
                what: "pie draw",
                bounds: Some(c.Bounds),
                path: () => {
                    GraphicsPath path = new();
                    path.MoveTo(new PointF(x: c.Bounds.X + (c.Bounds.Width / 2f), y: c.Bounds.Y + (c.Bounds.Height / 2f)));
                    path.AddArc(c.Bounds.X, c.Bounds.Y, c.Bounds.Width, c.Bounds.Height, c.StartAngle, c.SweepAngle);
                    path.CloseFigure();
                    return path;
                }),
            bezierCase: static (s, c) => DrawMarked(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(BezierCase)),
                what: "bezier draw",
                bounds: Some(BoundsOf(c.Start, c.Control1, c.Control2, c.End)),
                path: () => {
                    GraphicsPath path = new();
                    path.AddBezier(start: c.Start, control1: c.Control1, control2: c.Control2, end: c.End);
                    return path;
                }),
            curveCase: static (s, c) => DrawMarked(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(CurveCase)),
                what: "curve draw",
                bounds: Some(BoundsOf(c.Points.Span)),
                // Eto IGraphicsPath.AddCurve takes IEnumerable<PointF>; there is no ReadOnlySpan overload, so ToEnumerable stays.
                path: () => {
                    GraphicsPath path = new();
                    path.AddCurve(points: MemoryMarshal.ToEnumerable(c.Points), tension: c.Tension);
                    return path;
                }),
            // Text fill uses the same inert brush cache as shapes; no `using` around cached brushes.
            textCase: static (s, c) => c.Frame.Size == SizeF.Empty
                ? Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(TextCase)), detail: "text frame size must be non-empty"))
                : DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(TextCase)), what: "text draw", bounds: Some(c.Frame), draw: g =>
                    c.Style.Font.IfNone(UiFont.Empty()).Use(run: font => {
                        Brush brush = c.Style.FillBrush.Map(static fill => fill.CachedBrush()).IfNone(() => PaintStyle.Brush(color: c.Style.Edge));
                        g.DrawText(font, brush, c.Frame, c.Value, c.Wrap, c.Alignment, c.Trimming);
                        return unit;
                    })),
            imageCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(ImageCase)), what: "image draw", bounds: Some(c.Frame), draw: g => {
                g.DrawImage(image: c.Value, rectangle: c.Frame);
                return unit;
            }),
            ghIconCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(GhIconCase)), what: "icon draw", draw: _ => {
                c.Value.Draw(context: c.Adjust.Apply(context: new IconContext(
                    context: Eto.Drawing.Context.CreateFromContent(graphics: s.Graphics),
                    frame: c.Frame,
                    background: c.Style.Background)));
                return unit;
            }),
            // Skin indexer and shape build are host touches kept inside the guarded body so an absent-key throw lands in the Fin rail;
            // bounds are omitted because the WireShape bezier bow can exceed the endpoint chord box, and the cull bounds derive from that host shape.
            wireCase: static (s, w) => DrawStyled(scope: s, style: w.Style, op: Op.Of(name: nameof(WireCase)), what: "wire draw", draw: graphics => {
                WireSkin skin = s.Skin.Wires[w.Kind];
                WireShape shape = WireShape.Create(source: w.Source, target: w.Target);
                Color outerColour = w.SourceSelected && w.TargetSelected ? skin.SelectedGlow : skin.Normal;
                using Pen outerPen = new(color: outerColour, thickness: skin.Outer.Width);
                skin.Outer.AssignToPen(pen: outerPen);
                shape.Draw(graphics: graphics, edge: outerPen);
                // Inner glow is the selection highlight: draw it only when the wire touches a selected endpoint.
                _ = Optional(skin.Inner).Filter(_ => w.SourceSelected || w.TargetSelected).Iter(inner => {
                    using Pen innerPen = new(color: skin.SelectedGlow, thickness: inner.Width);
                    inner.AssignToPen(pen: innerPen);
                    shape.Draw(graphics: graphics, edge: innerPen);
                });
                return unit;
            }),
            capsuleCase: static (s, c) => s.WhenVisible(
                bounds: c.Bounds.IfNone(() => c.Value.Bounds),
                draw: () => Op.Of(name: nameof(CapsuleCase)).Attempt(body: () => {
                    c.Value.Draw(graphics: s.Graphics.Content, elements: c.Elements, shade: c.Shade, skin: s.Skin);
                    return unit;
                }, what: "capsule draw")),
            clippedCase: static (s, c) => s.Clip(region: ClipGeometry.Path(geometry: c.Clip), body: clipScope => c.Plan.Apply(scope: clipScope)),
            // Host transform mutations route through op.Attempt like every sibling arm; the saved state stays live across the plan
            // and disposes via the using when the body returns. The inner plan rail flattens through Bind-identity (no doubled Fin).
            transformedCase: static (s, c) => Op.Of(name: nameof(TransformedCase)).Attempt(body: () => {
                Graphics graphics = s.Graphics.Content;
                using IDisposable state = graphics.SaveTransformState();
                graphics.MultiplyTransform(matrix: c.Transform);
                return c.Plan.Apply(scope: s with { VisibilityCulling = false });
            }, what: "transformed draw").Bind(static inner => inner));

    // RectangleF.Union owns bounds reduction; one params-span absorber serves fixed-arity vertices and point runs alike.
    // The (point, SizeF.Empty) seed is an exact zero-size rect; the (point, point) ctor inflates +1px per axis.
    private static RectangleF BoundsOf(params ReadOnlySpan<PointF> points) {
        // BOUNDARY ADAPTER -- ReadOnlySpan has no IEnumerable; span-index fold is the only zero-alloc path.
        RectangleF acc = points.Length == 0 ? RectangleF.Empty : new RectangleF(points[0], SizeF.Empty);
        for (int i = 1; i < points.Length; i++) {
            acc = RectangleF.Union(acc, new RectangleF(points[i], SizeF.Empty));
        }
        return acc;
    }

    // DrawMarked is the shaped-arm base: cull, transform-state save, style assign, then fill+stroke through one PaintShape.
    private static Fin<Unit> DrawMarked(PaintScope scope, PaintStyle style, Op op, string what, PaintShape shape, Option<RectangleF> bounds = default) =>
        DrawStyled(scope: scope, style: style, op: op, what: what, bounds: bounds, draw: g => DrawShape(style: style, graphics: g, shape: shape));

    // Path-producing shaped arms own a transient IGraphicsPath; the owned path is disposed after one fill+stroke pass.
    private static Fin<Unit> DrawMarked(PaintScope scope, PaintStyle style, Op op, string what, Func<IGraphicsPath> path, Option<RectangleF> bounds = default) =>
        DrawStyled(scope: scope, style: style, op: op, what: what, bounds: bounds, draw: g => {
            using IGraphicsPath owned = path();
            return DrawShape(style: style, graphics: g, shape: PaintShape.Path(path: owned));
        });

    private static Fin<Unit> DrawStyled(PaintScope scope, PaintStyle style, Op op, string what, Func<Graphics, Unit> draw, Option<RectangleF> bounds = default) {
        Fin<Unit> drawn() =>
            Optional(draw).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "draw delegate is required"))
                .Bind(valid => op.Attempt(body: () => {
                    Graphics graphics = scope.Graphics.Content;
                    using IDisposable state = graphics.SaveTransformState();
                    _ = style.Assign(graphics: graphics);
                    return valid(arg: graphics);
                }, what: what));
        return bounds.Match(Some: b => scope.WhenVisible(bounds: b, draw: drawn), None: drawn);
    }

    private static Unit DrawShape(PaintStyle style, Graphics graphics, PaintShape shape) {
        _ = style.FillBrush.IfSome(source => shape.Fill(graphics, source.CachedBrush()));
        _ = style.FillBrush.IfNone(() => style.Fill.IfSome(colour => {
            using SolidBrush brush = PaintStyle.Brush(colour);
            shape.Fill(graphics, brush);
        }));
        return Op.SideWhen(style.HasVisibleStroke, () => {
            using Pen pen = style.Pen();
            shape.Stroke(graphics, pen);
        });
    }

    // PaintShape is the fill+stroke pair every shaped arm reduces to; stroke-only shapes carry an inert fill.
    // Eto's DrawLines accepts IEnumerable<PointF> and FillPolygon/DrawPolygon accept params PointF[]; neither has a span overload.
    private readonly record struct PaintShape(Action<Graphics, Brush> Fill, Action<Graphics, Pen> Stroke) {
        internal static PaintShape Line(PointF a, PointF b) =>
            new(
                Fill: static (_, _) => { },
                Stroke: (graphics, pen) => graphics.DrawLine(pen, a.X, a.Y, b.X, b.Y));

        internal static PaintShape Rectangle(RectangleF bounds) =>
            new(
                Fill: (graphics, brush) => graphics.FillRectangle(brush: brush, rectangle: bounds),
                Stroke: (graphics, pen) => graphics.DrawRectangle(pen: pen, rectangle: bounds));

        internal static PaintShape Ellipse(RectangleF bounds) =>
            new(
                Fill: (graphics, brush) => graphics.FillEllipse(brush: brush, rectangle: bounds),
                Stroke: (graphics, pen) => graphics.DrawEllipse(pen: pen, rectangle: bounds));

        internal static PaintShape Path(IGraphicsPath path) =>
            new(
                Fill: (graphics, brush) => graphics.FillPath(brush: brush, path: path),
                Stroke: (graphics, pen) => graphics.DrawPath(pen: pen, path: path));

        internal static PaintShape Polygon(ReadOnlyMemory<PointF> points) =>
            new(
                Fill: (graphics, brush) => graphics.FillPolygon(brush, [.. MemoryMarshal.ToEnumerable(points)]),
                Stroke: (graphics, pen) => graphics.DrawPolygon(pen, [.. MemoryMarshal.ToEnumerable(points)]));

        // Stroke-only: an open polyline has no fill, so DrawShape routing is for op/cull uniformity, not fill.
        internal static PaintShape Polyline(ReadOnlyMemory<PointF> points) =>
            new(
                Fill: static (_, _) => { },
                Stroke: (graphics, pen) => graphics.DrawLines(pen, MemoryMarshal.ToEnumerable(points)));

        // Stroke-only: an arc is an open curve drawn via a transient AddArc path (proven six-float AddArc + DrawPath).
        internal static PaintShape Arc(RectangleF bounds, float startAngle, float sweepAngle) =>
            new(
                Fill: static (_, _) => { },
                Stroke: (graphics, pen) => {
                    using GraphicsPath path = new();
                    path.AddArc(bounds.X, bounds.Y, bounds.Width, bounds.Height, startAngle, sweepAngle);
                    graphics.DrawPath(pen: pen, path: path);
                });
    }

}

public readonly record struct DrawPlan(Seq<DrawMark> Marks, Option<CanvasPaintPhase> Phase = default, Option<RectangleF> Bounds = default) {
    public static DrawPlan Empty => new(Marks: Seq<DrawMark>());

    // Marks concatenate; the right operand's phase/bounds win when present (Option `|` is left-biased toward Some).
    public static DrawPlan operator +(DrawPlan left, DrawPlan right) =>
        new(Marks: left.Marks + right.Marks, Phase: right.Phase | left.Phase, Bounds: right.Bounds | left.Bounds);

    public DrawPlan For(CanvasPaintPhase phase) => this with { Phase = Some(phase) };
    public DrawPlan Within(RectangleF bounds) => this with { Bounds = Some(bounds) };

    // Phase-gated and bounds-culled: a plan pinned to another phase succeeds idle; a bounds-pinned plan defers to
    // the scope's cull predicate before traversing its marks.
    internal Fin<Unit> Apply(PaintScope scope) {
        Option<CanvasPaintPhase> phase = Phase;
        Option<RectangleF> bounds = Bounds;
        Seq<DrawMark> marks = Marks;
        return phase.Map(phase => phase != scope.Phase).IfNone(noneValue: false)
            ? Fin.Succ(unit)
            : scope.WhenVisible(
                bounds: bounds.IfNone(RectangleF.Empty),
                draw: () => marks.TraverseM(scope.Apply).Map(static _ => unit).As());
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintSkinSnapshot(Option<CanvasSkin> Appearance) {
    public bool HasSkin => Appearance.IsSome;
    public Option<Skin> Skin => Appearance.Map(static a => a.Effective);
}

// --- [SERVICES] ---------------------------------------------------------------------------
public static partial class Paint {
    public static GrasshopperUiIntent<PaintSkinSnapshot> Skin() =>
        GhUi.Canvas(run: scope => scope.NeedCanvasSkin().Map(appearance => new PaintSkinSnapshot(Appearance: Some(appearance))));

    public static GrasshopperUiIntent<Subscription> Hook(
        CanvasPaintPhase phase,
        Func<PaintScope, Fin<Unit>> paint,
        MotionClock? clock = null) =>
        Hook(
            phase: phase,
            paint: paint,
            clock: clock ?? MotionClock.MessageLoop);

    internal static GrasshopperUiIntent<Subscription> Hook(
        CanvasPaintPhase phase,
        Func<PaintScope, Fin<Unit>> paint,
        MotionClock clock,
        Option<Motion.Pacer> adoptedPacer = default,
        bool ownsPacerLifecycle = true,
        Func<bool>? sustainMouseRedraw = null) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(paint).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null paint callback"))
            from validPhase in Optional(phase).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null phase"))
            from pacer in adoptedPacer.Match(
                Some: p => Fin.Succ(value: Some(value: p)),
                None: () => Motion.PacerOption(canvas: canvas, clock: clock))
            let handler = (EventHandler<CanvasPaintEventArgs>)((_, args) => {
                _ = GrasshopperUi.Handler(valid: () => valid(arg: new PaintScope(
                    Phase: validPhase,
                    Graphics: args.Graphics,
                    Skin: args.Skin) {
                    Background = Optional(args as CanvasBackgroundPaintEventArgs),
                })).Ignore();
                _ = Op.SideWhen(sustainMouseRedraw?.Invoke() == true, () => canvas.RedrawOnMouseMove = true);
            })
            from paintSub in Subscription.Bind(
                attach: () => {
                    _ = validPhase.Attach(canvas: canvas, handler: handler);
                    _ = Op.SideWhen(adoptedPacer.IsNone, () => _ = pacer.ResumeOr(canvas: canvas));
                },
                detach: () => _ = validPhase.Detach(canvas: canvas, handler: handler),
                marshalToUi: true)
            select ownsPacerLifecycle
                ? Subscription.DisposeOnce(Subscription.PaintPacer(
                    paintHook: paintSub,
                    pacerRelease: () => _ = pacer.ReleaseOnce()))
                : paintSub);

    public static GrasshopperUiIntent<Subscription> RedrawOnMouseMove(bool enabled = true, MotionClock? clock = null) =>
        GhUi.Canvas(
            repaint: RepaintRequest.Canvas,
            run: scope =>
                from canvas in scope.NeedCanvas()
                from pacer in Motion.PacerOption(canvas: canvas, clock: clock ?? MotionClock.MessageLoop)
                let token = new StrongBox<Guid>()
                from sustain in Hook(
                    phase: CanvasPaintPhase.AfterWires,
                    paint: static _ => Fin.Succ(value: unit),
                    clock: clock ?? MotionClock.MessageLoop,
                    adoptedPacer: pacer,
                    ownsPacerLifecycle: false,
                    sustainMouseRedraw: enabled ? () => CanvasLease.RedrawLease.IsTop(canvas: canvas, token: token.Value, value: true) : null).Run(scope: scope)
                from arm in Events.BindMarshaled(
                    attach: () => {
                        token.Value = CanvasLease.RedrawLease.Enter(canvas: canvas, value: enabled);
                        _ = pacer.ResumeOr(canvas: canvas);
                    },
                    detach: () => CanvasLease.RedrawLease.Exit(canvas: canvas, token: token.Value))
                select Subscription.DisposeOnce(Subscription.PaintPacer(
                    paintHook: sustain | arm,
                    pacerRelease: () => _ = pacer.ReleaseOnce())));

    public static GrasshopperUiIntent<Subscription> FloatingDrawable(DrawPlan plan, Size size, Option<string> title = default) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from skin in scope.NeedSkin()
            from validPlan in Optional(plan).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(FloatingDrawable)), detail: "null draw plan"))
            from validWidth in Op.Of(name: nameof(FloatingDrawable)).AcceptFinite(value: size.Width, detail: "width must be finite and positive", requirePositive: true)
            from validHeight in Op.Of(name: nameof(FloatingDrawable)).AcceptFinite(value: size.Height, detail: "height must be finite and positive", requirePositive: true)
            let validSize = new Size(width: (int)Math.Ceiling(validWidth), height: (int)Math.Ceiling(validHeight))
            let owner = Optional(canvas.ControlObject as Control).Bind(static control => Optional(control.ParentWindow))
            let screenScale = UiRail.PixelScale(canvas: canvas).LogicalPixelSize
            let drawable = new Drawable { Size = validSize }
            let form = new FloatingForm(owner: owner) {
                Content = drawable,
                Size = validSize,
                Title = title.IfNone(string.Empty),
            }
            let paintHandler = (EventHandler<PaintEventArgs>)((_, args) => {
                using Bitmap surface = new(validSize, PixelFormat.Format32bppRgba);
                using ControlGraphics graphics = new(bitmap: surface);
                graphics.ScreenScale = screenScale;
                _ = GrasshopperUi.Handler(valid: () => validPlan.Apply(scope: new PaintScope(
                    Phase: CanvasPaintPhase.AfterObjects,
                    Graphics: graphics,
                    Skin: skin))).Ignore();
                args.Graphics.DrawImage(image: surface, x: 0, y: 0);
            })
            from sub in Subscription.Bind(
                attach: () => {
                    drawable.Paint += paintHandler;
                    form.Show();
                },
                detach: () => {
                    drawable.Paint -= paintHandler;
                    form.Close();
                    form.Dispose();
                },
                marshalToUi: true)
            select sub);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Key includes FontDecoration explicitly -- Eto.Drawing.Font equality excludes it. AppKit text APIs
// are main-thread-only; [ThreadStatic] scratch FormattedText avoids cross-thread reuse.
file static class TextMetricsCache {
    private readonly record struct Key(Font Font, string Text, FormattedTextWrapMode Wrap, FormattedTextAlignment Alignment, FormattedTextTrimming Trimming, float MaxWidth, float MaxHeight, FontDecoration Decoration);

    private static readonly BoundedCache<Key, SizeF> Cache = new(capacity: 1024);
    [ThreadStatic] private static FormattedText? scratch;

    private static FormattedText Scratch() => scratch ??= new FormattedText();

    internal static SizeF Measure(Font font, string text,
        FormattedTextWrapMode wrap, FormattedTextAlignment alignment, FormattedTextTrimming trimming,
        SizeF maxSize) =>
        Cache.GetOrAdd(
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

// Dedupes by (dashes array identity, quantized offset). Quantum 0.01 matches DashStyle.Equals
// tolerance and is perceptually identical at any pen thickness.
file static class DashStyleCache {
    private const float Quantum = 0.01f;
    private static readonly BoundedCache<(int DashesRef, int Bucket), DashStyle> Cache = new(capacity: 4096);

    internal static DashStyle WithOffset(DashStyle baseline, float offset) {
        int bucket = (int)MathF.Round(offset / Quantum, MidpointRounding.ToEven);
        if (bucket == 0) {
            return baseline;
        }
        int dashesRef = baseline.Dashes is float[] dashes ? RuntimeHelpers.GetHashCode(dashes) : 0;
        return Cache.GetOrAdd(
            key: (DashesRef: dashesRef, Bucket: bucket),
            valueFactory: key => new DashStyle(offset: key.Bucket * Quantum, dashes: baseline.Dashes));
    }
}
