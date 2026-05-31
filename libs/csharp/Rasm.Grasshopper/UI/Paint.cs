using System.Diagnostics.CodeAnalysis;
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
// Theme-reactive: each kind resolves from the live paint Skin (GH2 Shades/Canvasses tokens) when one is
// supplied, falling back to the Eto system palette for tokens GH2 does not model (LinkText) and for
// construction sites that resolve before a canvas Skin is known (Option<Skin>.None).
[SmartEnum<int>]
public sealed partial class SystemColorKind {
    private delegate Color ColorSource(Option<Skin> skin);

    public static readonly SystemColorKind ControlText = new(key: 0, resolve: static skin => Token(skin, static s => s.Shades[ShadeKind.Normal].Text, SystemColors.ControlText));
    public static readonly SystemColorKind Control = new(key: 1, resolve: static skin => Token(skin, static s => s.Shades[ShadeKind.Normal].Slab, SystemColors.Control));
    public static readonly SystemColorKind ControlBackground = new(key: 2, resolve: static skin => Token(skin, static s => s.Shades[ShadeKind.Normal].Apex, SystemColors.ControlBackground));
    public static readonly SystemColorKind WindowBackground = new(key: 3, resolve: static skin => Token(skin, static s => s.Canvasses[CanvasKind.Normal].Background, SystemColors.WindowBackground));
    public static readonly SystemColorKind Highlight = new(key: 4, resolve: static skin => Token(skin, static s => s.Shades[ShadeKind.NormalSelected].Apex, SystemColors.Highlight));
    public static readonly SystemColorKind HighlightText = new(key: 5, resolve: static skin => Token(skin, static s => s.Shades[ShadeKind.NormalSelected].Text, SystemColors.HighlightText));
    public static readonly SystemColorKind Selection = new(key: 6, resolve: static skin => Token(skin, static s => s.Shades[ShadeKind.NormalSelected].Slab, SystemColors.Selection));
    public static readonly SystemColorKind SelectionText = new(key: 7, resolve: static skin => Token(skin, static s => s.Shades[ShadeKind.NormalSelected].TextY, SystemColors.SelectionText));
    public static readonly SystemColorKind DisabledText = new(key: 8, resolve: static skin => Token(skin, static s => s.Shades[ShadeKind.Disabled].Text, SystemColors.DisabledText));
    public static readonly SystemColorKind LinkText = new(key: 9, resolve: static _ => SystemColors.LinkText);

    private static Color Token(Option<Skin> skin, Func<Skin, Color> fromSkin, Color fallback) =>
        skin.Match(Some: fromSkin, None: () => fallback);

    [UseDelegateFromConstructor]
    internal partial Color Resolve(Option<Skin> skin);
}

// Icon render-state filter. Each case carries the IconContext colour-filter transform the host
// applies during draw (WithDisabledFilter desaturates+compresses, WithGreyscaleFilter maps to
// luminance, WithFadingFilter blends toward the icon background). None is identity.
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
public readonly record struct PaintScope(CanvasPaintPhase Phase, ControlGraphics Graphics, Skin Skin) {
    public bool DefaultBackgroundOverridden =>
        Background.Map(static args => args.DefaultOverridden).IfNone(noneValue: false);

    public Unit OverrideDefault() =>
        Background.Match(
            Some: args => { args.OverrideDefaultPainting(); return unit; },
            None: () => unit);

    internal Option<CanvasBackgroundPaintEventArgs> Background { get; init; }

    internal bool VisibilityCulling { get; init; } = true;

    internal bool ClipActive { get; init; }

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

    // TextMeasure-routed: Eto.Mac 2.11 GraphicsHandler.MeasureString leaks FormattedText state across calls.
    public Fin<PaintTextMeasurement> MeasureText(string value, Option<UiFont> font = default) =>
        MeasureText(
            value: value,
            font: font,
            wrap: FormattedTextWrapMode.None,
            alignment: FormattedTextAlignment.Left,
            trimming: FormattedTextTrimming.None,
            maxSize: new SizeF(width: float.MaxValue, height: float.MaxValue));

    public Fin<PaintTextMeasurement> MeasureText(
        string value,
        Option<UiFont> font,
        FormattedTextWrapMode wrap,
        FormattedTextAlignment alignment,
        FormattedTextTrimming trimming,
        SizeF maxSize) =>
        Optional(value)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(MeasureText)), detail: "text is required"))
            .Bind(valid => Try.lift(f: () =>
                font.IfNone(UiFont.Empty()).Use(run: resolved =>
                    new PaintTextMeasurement(
                        Text: valid,
                        Size: TextMeasure.Measure(
                            font: resolved,
                            text: valid,
                            wrap: wrap,
                            alignment: alignment,
                            trimming: trimming,
                            maxSize: maxSize))
                )).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(MeasureText)), detail: error.Message)));

    // SaveTransformState does NOT capture clipping; ResetClip in finally is mandatory.
    public Fin<Unit> Clip(IGraphicsPath path, Func<PaintScope, Fin<Unit>> body) {
        PaintScope self = this;
        return Optional(path)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Clip)), detail: "clip path is required"))
            .Bind(validPath => self.ClipActive
                ? Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Clip)), detail: "nested clipped draw marks are not supported by Eto clip restoration"))
                : Optional(body).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Clip)), detail: "body is required"))
            .Bind(validBody => {
                Graphics graphics = self.Graphics.Content;
                graphics.SetClip(path: validPath);
                try {
                    return validBody(arg: self with { ClipActive = true });
                } finally {
                    graphics.ResetClip();
                }
            }));
    }
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

    // Eto 2.11 has no Pen.DashOffset setter and DashStyle is sealed-immutable, so DashStyleIntern dedups
    // quantized-offset DashStyle instances; EdgeSource/EdgeBrush/FillBrush keep each Pen/Brush caller-owned.
    internal Pen Pen() {
        DashStyle dash = OffsetDash();
        Color edgeColour = Edge;
        EdgeSource edge = EdgeBrush.Map(EdgeSource.FromFill).IfNone(EdgeSource.Solid(colour: edgeColour));
        return edge.CreatePen(
            thickness: Thickness,
            cap: LineCap,
            join: LineJoin,
            miterLimit: MiterLimit,
            dash: dash);
    }

    private DashStyle OffsetDash() {
        DashStyle baseline = Dash.IfNone(DashStyles.Solid);
        return DashOffset == 0f ? baseline : DashStyleIntern.WithOffset(baseline: baseline, offset: DashOffset);
    }

    internal static SolidBrush Brush(Color color) => new(color: color);

    private static PaintStyle CreateEdge(
        Color edge,
        Option<Color> fill = default,
        float thickness = 1f,
        Option<UiFont> font = default,
        Color background = default) =>
        Create(
            edge: edge,
            fill: fill,
            thickness: thickness,
            font: font,
            background: background,
            lineCap: PenLineCap.Butt,
            lineJoin: PenLineJoin.Miter,
            dash: default,
            miterLimit: EdgeSource.DefaultMiterLimit,
            antiAlias: true,
            imageInterpolation: ImageInterpolation.Default,
            pixelOffset: PixelOffsetMode.None,
            dashOffset: 0f,
            fillBrush: default,
            edgeBrush: default);

    internal static PaintStyle ForEdge(Color edge, Option<Color> fill = default, float thickness = 1f) =>
        CreateEdge(edge: edge, fill: fill, thickness: thickness);

    internal static PaintStyle ForEdgeText(Color edge, Option<UiFont> font = default) =>
        CreateEdge(edge: edge, font: font);

    internal static PaintStyle ForTransparent(Color background = default) =>
        CreateEdge(edge: Colors.Transparent, background: background);

    internal static PaintStyle ForSystemColor(SystemColorKind kind, Option<Skin> skin = default, Option<Color> fill = default, float thickness = 1f) =>
        CreateEdge(edge: kind.Resolve(skin: skin), fill: fill, thickness: thickness);

    internal Unit Assign(Graphics graphics) {
        graphics.AntiAlias = AntiAlias;
        graphics.ImageInterpolation = ImageInterpolation;
        graphics.PixelOffsetMode = PixelOffset;
        return unit;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintTextMeasurement(string Text, SizeF Size);

// Canonical LRU: Dictionary + doubly-linked list. All ops O(1) — head is LRU, tail is MRU.
file sealed class BoundedCache<TKey, TValue> where TKey : notnull {
    private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> entries;
    private readonly LinkedList<KeyValuePair<TKey, TValue>> order = new();
    private readonly Lock gate = new();
    private readonly int capacity;

    internal BoundedCache(int capacity) {
        this.capacity = capacity;
        entries = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(capacity);
    }

    internal TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
        using (gate.EnterScope()) {
            if (entries.TryGetValue(key: key, value: out LinkedListNode<KeyValuePair<TKey, TValue>>? hit)) {
                order.Remove(node: hit);
                order.AddLast(node: hit);
                return hit.Value.Value;
            }
            TValue fresh = valueFactory(arg: key);
            if (entries.Count >= capacity && order.First is LinkedListNode<KeyValuePair<TKey, TValue>> evict) {
                order.RemoveFirst();
                _ = entries.Remove(key: evict.Value.Key);
            }
            LinkedListNode<KeyValuePair<TKey, TValue>> node = new(value: new KeyValuePair<TKey, TValue>(key: key, value: fresh));
            order.AddLast(node: node);
            entries.Add(key: key, value: node);
            return fresh;
        }
    }

}

// Key includes FontDecoration explicitly — Eto.Drawing.Font equality excludes it. AppKit text APIs
// are main-thread-only; [ThreadStatic] scratch FormattedText avoids cross-thread reuse.
file static class TextMeasure {
    private readonly record struct Key(Font Font, string Text, FormattedTextWrapMode Wrap, FormattedTextAlignment Alignment, FormattedTextTrimming Trimming, float MaxWidth, float MaxHeight, FontDecoration Decoration);

    private static readonly BoundedCache<Key, SizeF> Cache = new(capacity: 1024);
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
file static class DashStyleIntern {
    private const float Quantum = 0.01f;
    private static readonly BoundedCache<(int DashesRef, int Bucket), DashStyle> Cache = new(capacity: 4096);

    internal static DashStyle WithOffset(DashStyle baseline, float offset) {
        int bucket = (int)MathF.Round(offset / Quantum);
        if (bucket == 0) {
            return baseline;
        }
        int dashesRef = baseline.Dashes is float[] dashes ? RuntimeHelpers.GetHashCode(dashes) : 0;
        return Cache.GetOrAdd(
            key: (DashesRef: dashesRef, Bucket: bucket),
            valueFactory: key => new DashStyle(offset: key.Bucket * Quantum, dashes: baseline.Dashes));
    }
}

file static class SystemFonts {
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct Key(SystemFont Kind, float? Size, FontDecoration Decoration);

    private static readonly BoundedCache<Key, Font> Cache = new(capacity: 128);

    internal static Font Cached(SystemFont systemFont, float? size, FontDecoration decoration) =>
        Cache.GetOrAdd(
            key: new Key(Kind: systemFont, Size: size, Decoration: decoration),
            valueFactory: static k => new Font(systemFont: k.Kind, size: k.Size, decoration: k.Decoration));

    internal static Font Default() => Cached(systemFont: SystemFont.Default, size: null, decoration: FontDecoration.None);
}

[SkipUnionOps]
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

[SkipUnionOps]
[Union]
public partial record FillSource {
    private FillSource() { }
    public sealed record SolidCase(Color Colour) : FillSource;
    public sealed record LinearCase(Color Start, Color End, PointF From, PointF To, GradientWrapMode Wrap = GradientWrapMode.Pad) : FillSource;
    public sealed record RadialCase(Color Centre, Color Edge, PointF Origin, PointF Focus, SizeF Radius, GradientWrapMode Wrap = GradientWrapMode.Pad) : FillSource;
    public sealed record AngularCase(Color Start, Color End, RectangleF Rect, float Angle, GradientWrapMode Wrap = GradientWrapMode.Pad) : FillSource;
    public sealed record TextureCase(Image Source, float Opacity = 1f) : FillSource;

    public static FillSource Solid(Color colour) => new SolidCase(Colour: colour);
    public static FillSource Linear(Color start, Color end, PointF from, PointF to, GradientWrapMode wrap = GradientWrapMode.Pad) =>
        new LinearCase(Start: start, End: end, From: from, To: to, Wrap: wrap);
    public static FillSource Radial(Color centre, Color edge, PointF origin, PointF focus, SizeF radius, GradientWrapMode wrap = GradientWrapMode.Pad) =>
        new RadialCase(Centre: centre, Edge: edge, Origin: origin, Focus: focus, Radius: radius, Wrap: wrap);
    // Angle-keyed linear gradient over a rect (Eto's LinearGradientBrush(rect,start,end,angle) ctor).
    public static FillSource Angular(Color start, Color end, RectangleF rect, float angle, GradientWrapMode wrap = GradientWrapMode.Pad) =>
        new AngularCase(Start: start, End: end, Rect: rect, Angle: angle, Wrap: wrap);
    public static FillSource Texture(Image source, float opacity = 1f) =>
        new TextureCase(Source: source, Opacity: opacity);

    internal Brush CreateBrush() =>
        Switch<Brush>(
            solidCase: static s => new SolidBrush(color: s.Colour),
            linearCase: static l => new LinearGradientBrush(startColor: l.Start, endColor: l.End, startPoint: l.From, endPoint: l.To) { Wrap = l.Wrap },
            radialCase: static r => new RadialGradientBrush(startColor: r.Centre, endColor: r.Edge, center: r.Origin, gradientOrigin: r.Focus, radius: r.Radius) { Wrap = r.Wrap },
            angularCase: static a => new LinearGradientBrush(rectangle: a.Rect, startColor: a.Start, endColor: a.End, angle: a.Angle) { Wrap = a.Wrap },
            textureCase: static t => new TextureBrush(image: t.Source, opacity: t.Opacity));

    // Per-frame fill paths reuse one brush per FillSource (structural key) instead of rebuilding each paint;
    // Eto brush Dispose is inert, so a shared cached brush is safe and never needs disposal. Mirrors TextMeasure.
    internal Brush CachedBrush() => FillBrushCache.Of(source: this);
}

file static class FillBrushCache {
    private static readonly BoundedCache<FillSource, Brush> Cache = new(capacity: 256);
    internal static Brush Of(FillSource source) => Cache.GetOrAdd(key: source, valueFactory: static s => s.CreateBrush());
}

[SkipUnionOps]
[Union]
public partial record EdgeSource {
    private EdgeSource() { }
    public sealed record SolidCase(Color Colour) : EdgeSource;
    public sealed record BrushBackedCase(FillSource Source) : EdgeSource;

    public static EdgeSource Solid(Color colour) => new SolidCase(Colour: colour);
    public static EdgeSource FromFill(FillSource source) => new BrushBackedCase(Source: source);

    // One brush-projected pen builder: solid backs with a SolidBrush, brush-backed reuses the FillBrushCache
    // (Eto pen-backing brush Dispose is inert), so both pens share the single initializer block.
    [SuppressMessage(category: "Reliability", checkId: "CA2000:Dispose objects before losing scope", Justification = "Eto brush Dispose is inert; the brush is owned by the returned Pen (and the brush-backed arm reuses a shared cached brush that must not be disposed).")]
    internal Pen CreatePen(float thickness, PenLineCap cap, PenLineJoin join, float miterLimit, DashStyle dash) {
        Brush brush = Switch(solidCase: static s => new SolidBrush(color: s.Colour), brushBackedCase: static b => b.Source.CachedBrush());
        return new Pen(brush: brush, thickness: thickness) { LineCap = cap, LineJoin = join, MiterLimit = miterLimit, DashStyle = dash };
    }

    internal const float DefaultMiterLimit = 10f;
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
    public sealed partial record RoundedRectangleCase(RectangleF Bounds, float Radius, PaintStyle Style) : DrawMark;
    public sealed partial record RoundedCornersCase(RectangleF Bounds, CornerRadii Radii, PaintStyle Style) : DrawMark;
    public sealed partial record EllipseCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed partial record PathCase(IGraphicsPath Geometry, PaintStyle Style) : DrawMark;
    public sealed partial record TextCase(string Value, RectangleF Frame, PaintStyle Style, FormattedTextWrapMode Wrap, FormattedTextAlignment Alignment, FormattedTextTrimming Trimming) : DrawMark;
    public sealed partial record ImageCase(Image Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed partial record GhIconCase(IIcon Value, RectangleF Frame, PaintStyle Style, IconAdjust Adjust) : DrawMark;
    public sealed partial record WireCase(PointF Source, PointF Target, WireKind Kind, PaintStyle Style) : DrawMark;
    public sealed partial record ArcCase(RectangleF Bounds, float StartAngle, float SweepAngle, PaintStyle Style, Option<SystemColorKind> SystemColor = default) : DrawMark;
    public sealed partial record PieCase(RectangleF Bounds, float StartAngle, float SweepAngle, PaintStyle Style) : DrawMark;
    public sealed partial record PolylineCase(ReadOnlyMemory<PointF> Points, PaintStyle Style) : DrawMark;
    public sealed partial record PolygonCase(ReadOnlyMemory<PointF> Points, PaintStyle Style) : DrawMark;
    public sealed partial record BezierCase(PointF Start, PointF Control1, PointF Control2, PointF End, PaintStyle Style) : DrawMark;
    public sealed partial record CurveCase(ReadOnlyMemory<PointF> Points, float Tension, PaintStyle Style) : DrawMark;
    public sealed partial record CapsuleCase(Capsule Value, Parts Elements, Shade Shade) : DrawMark;
    // Composite marks: a clip or transform applied to a nested DrawPlan (leaf marks + composite).
    public sealed partial record ClippedCase(IGraphicsPath Clip, DrawPlan Plan) : DrawMark;
    public sealed partial record TransformedCase(IMatrix Transform, DrawPlan Plan) : DrawMark;

    public static DrawMark Line(PointF a, PointF b, Color colour, float thickness = 1f) =>
        new LineCase(A: a, B: b, Style: PaintStyle.ForEdge(edge: colour, thickness: thickness));
    public static DrawMark Rectangle(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RectangleCase(Bounds: bounds, Style: PaintStyle.ForEdge(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark RoundedRectangle(RectangleF bounds, float radius, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RoundedRectangleCase(Bounds: bounds, Radius: radius, Style: PaintStyle.ForEdge(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark RoundedCorners(RectangleF bounds, CornerRadii radii, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RoundedCornersCase(Bounds: bounds, Radii: radii, Style: PaintStyle.ForEdge(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark Ellipse(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new EllipseCase(Bounds: bounds, Style: PaintStyle.ForEdge(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark Path(IGraphicsPath path, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new PathCase(Geometry: path, Style: PaintStyle.ForEdge(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark Label(
        string value,
        RectangleF frame,
        Color colour,
        Option<UiFont> font = default,
        FormattedTextWrapMode wrap = FormattedTextWrapMode.Word,
        FormattedTextAlignment alignment = FormattedTextAlignment.Left,
        FormattedTextTrimming trimming = FormattedTextTrimming.WordEllipsis) =>
        new TextCase(Value: value, Frame: frame, Style: PaintStyle.ForEdgeText(edge: colour, font: font), Wrap: wrap, Alignment: alignment, Trimming: trimming);
    public static DrawMark Image(Image value, RectangleF frame) =>
        new ImageCase(Value: value, Frame: frame, Style: PaintStyle.ForTransparent());
    public static DrawMark IconGlyph(IIcon value, RectangleF frame, Color background, IconAdjust? adjust = null) =>
        new GhIconCase(Value: value, Frame: frame, Style: PaintStyle.ForTransparent(background: background), Adjust: adjust ?? IconAdjust.None);
    public static DrawMark WirePreview(PointF source, PointF target, WireKind kind = WireKind.Tentative) =>
        new WireCase(Source: source, Target: target, Kind: kind, Style: PaintStyle.ForTransparent());
    public static DrawMark Arc(RectangleF bounds, float startAngle, float sweepAngle, Color edge, float thickness = 1f) =>
        new ArcCase(Bounds: bounds, StartAngle: startAngle, SweepAngle: sweepAngle, Style: PaintStyle.ForEdge(edge: edge, thickness: thickness));
    public static DrawMark Pie(RectangleF bounds, float startAngle, float sweepAngle, Color fill, Option<Color> edge = default, float thickness = 1f) =>
        new PieCase(Bounds: bounds, StartAngle: startAngle, SweepAngle: sweepAngle, Style: PaintStyle.ForEdge(edge: edge.IfNone(fill), fill: Some(fill), thickness: thickness));
    public static DrawMark Polyline(ReadOnlyMemory<PointF> points, Color edge, float thickness = 1f) =>
        new PolylineCase(Points: points, Style: PaintStyle.ForEdge(edge: edge, thickness: thickness));
    public static DrawMark Polygon(ReadOnlyMemory<PointF> points, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new PolygonCase(Points: points, Style: PaintStyle.ForEdge(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark Bezier(PointF start, PointF control1, PointF control2, PointF end, Color edge, float thickness = 1f) =>
        new BezierCase(Start: start, Control1: control1, Control2: control2, End: end, Style: PaintStyle.ForEdge(edge: edge, thickness: thickness));
    public static DrawMark Curve(ReadOnlyMemory<PointF> points, Color edge, float tension = 0.5f, Option<Color> fill = default, float thickness = 1f) =>
        new CurveCase(Points: points, Tension: tension, Style: PaintStyle.ForEdge(edge: edge, fill: fill, thickness: thickness));
    public static DrawMark SystemArc(RectangleF bounds, float startAngle, float sweepAngle, SystemColorKind color, float thickness = 1f) {
        ArgumentNullException.ThrowIfNull(color);
        return new ArcCase(Bounds: bounds, StartAngle: startAngle, SweepAngle: sweepAngle, Style: PaintStyle.ForSystemColor(kind: color, thickness: thickness), SystemColor: Some(color));
    }
    public static DrawMark DrawCapsule(Capsule capsule, Shade shade, Parts elements = Parts.All) =>
        new CapsuleCase(Value: capsule, Elements: elements, Shade: shade);
    public static DrawMark Clipped(IGraphicsPath clip, DrawPlan plan) => new ClippedCase(Clip: clip, Plan: plan);
    public static DrawMark Transformed(IMatrix transform, DrawPlan plan) => new TransformedCase(Transform: transform, Plan: plan);

    internal Fin<Unit> Apply(PaintScope scope) =>
        Switch(state: scope,
            lineCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(LineCase)), what: "line draw", bounds: BoundsOf(points: new[] { c.A, c.B }), draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Line(a: c.A, b: c.B))),
            rectangleCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(RectangleCase)), what: "rectangle draw", bounds: c.Bounds, draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Rectangle(bounds: c.Bounds))),
            roundedRectangleCase: static (s, c) => DrawPathStyled(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(RoundedRectangleCase)),
                what: "rounded rectangle draw",
                bounds: c.Bounds,
                path: () => GraphicsPath.GetRoundRect(rectangle: c.Bounds, radius: c.Radius)),
            roundedCornersCase: static (s, c) => DrawPathStyled(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(RoundedCornersCase)),
                what: "rounded corners draw",
                bounds: c.Bounds,
                path: () => c.Radii.IsUniform
                    ? GraphicsPath.GetRoundRect(rectangle: c.Bounds, radius: c.Radii.TopLeft)
                    : GraphicsPath.GetRoundRect(
                        rectangle: c.Bounds,
                        nwRadius: c.Radii.TopLeft,
                        neRadius: c.Radii.TopRight,
                        seRadius: c.Radii.BottomRight,
                        swRadius: c.Radii.BottomLeft)),
            ellipseCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(EllipseCase)), what: "ellipse draw", bounds: c.Bounds, draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Ellipse(bounds: c.Bounds))),
            pathCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(PathCase)), what: "path draw", draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Path(path: c.Geometry))),
            textCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(TextCase)), what: "text draw", bounds: c.Frame, draw: g =>
                c.Style.Font.IfNone(UiFont.Empty()).Use(run: font => {
                    using SolidBrush brush = PaintStyle.Brush(color: c.Style.Edge);
                    g.DrawText(font, brush, c.Frame, c.Value, c.Wrap, c.Alignment, c.Trimming);
                    return unit;
                })),
            imageCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(ImageCase)), what: "image draw", bounds: c.Frame, draw: g => {
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
            wireCase: static (s, w) => DrawStyled(scope: s, style: w.Style, op: Op.Of(name: nameof(WireCase)), what: "wire draw", draw: graphics => {
                WireSkin skin = s.Skin.Wires[w.Kind];
                WireShape shape = WireShape.Create(source: w.Source, target: w.Target);
                using Pen outerPen = new(color: skin.Normal, thickness: skin.Outer.Width);
                skin.Outer.AssignToPen(pen: outerPen);
                shape.Draw(graphics: graphics, edge: outerPen);
                _ = Optional(skin.Inner).Iter(inner => {
                    using Pen innerPen = new(color: skin.SelectedGlow, thickness: inner.Width);
                    inner.AssignToPen(pen: innerPen);
                    shape.Draw(graphics: graphics, edge: innerPen);
                });
                return unit;
            }),
            arcCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(ArcCase)), what: "arc draw", bounds: c.Bounds, draw: g => {
                using Pen pen = (c.SystemColor is { IsSome: true, Case: SystemColorKind kind }
                    ? PaintStyle.ForSystemColor(kind: kind, skin: Some(s.Skin), thickness: c.Style.Thickness)
                    : c.Style).Pen();
                g.DrawArc(pen, c.Bounds, c.StartAngle, c.SweepAngle);
                return unit;
            }),
            // Pie wedge as one closed figure (centre -> arc start -> arc -> back to centre) so fill AND both
            // radii stroke together; FillPie + DrawArc could not stroke the radii.
            pieCase: static (s, c) => DrawPathStyled(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(PieCase)),
                what: "pie draw",
                bounds: c.Bounds,
                path: () => {
                    GraphicsPath path = new();
                    path.MoveTo(new PointF(x: c.Bounds.X + (c.Bounds.Width / 2f), y: c.Bounds.Y + (c.Bounds.Height / 2f)));
                    path.AddArc(c.Bounds, c.StartAngle, c.SweepAngle);
                    path.CloseFigure();
                    return path;
                }),
            polylineCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(PolylineCase)), what: "polyline draw", bounds: BoundsOf(points: c.Points),
                draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Polyline(points: c.Points.Span.ToArray()))),
            polygonCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: Op.Of(name: nameof(PolygonCase)), what: "polygon draw", bounds: BoundsOf(points: c.Points),
                draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Polygon(points: c.Points.Span.ToArray()))),
            bezierCase: static (s, c) => DrawPathStyled(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(BezierCase)),
                what: "bezier draw",
                bounds: BoundsOf(points: new[] { c.Start, c.Control1, c.Control2, c.End }),
                path: () => {
                    GraphicsPath path = new();
                    path.AddBezier(start: c.Start, control1: c.Control1, control2: c.Control2, end: c.End);
                    return path;
                }),
            curveCase: static (s, c) => DrawPathStyled(
                scope: s,
                style: c.Style,
                op: Op.Of(name: nameof(CurveCase)),
                what: "curve draw",
                bounds: BoundsOf(points: c.Points),
                path: () => {
                    GraphicsPath path = new();
                    path.AddCurve(points: c.Points.Span.ToArray(), tension: c.Tension);
                    return path;
                }),
            capsuleCase: static (s, c) => Op.Of(name: nameof(CapsuleCase)).Attempt(body: () => {
                c.Value.Draw(graphics: s.Graphics.Content, elements: c.Elements, shade: c.Shade, skin: s.Skin);
                return unit;
            }, what: "capsule draw"),
            clippedCase: static (s, c) => s.Clip(path: c.Clip, body: clipScope => c.Plan.Apply(scope: clipScope)),
            transformedCase: static (s, c) => {
                Graphics graphics = s.Graphics.Content;
                using IDisposable state = graphics.SaveTransformState();
                graphics.MultiplyTransform(matrix: c.Transform);
                return c.Plan.Apply(scope: s with { VisibilityCulling = false });
            });

    // Eto owns the bounding-box reduction: seed a degenerate rect at points[0] and Union each point's
    // degenerate rect (RectangleF's two-corner ctor + RectangleF.Union, both already used across the file).
    private static RectangleF BoundsOf(ReadOnlyMemory<PointF> points) =>
        points.Length == 0
            ? RectangleF.Empty
            : toSeq(points.Span.ToArray()).Fold(
                initialState: new RectangleF(points.Span[0], points.Span[0]),
                f: static (acc, point) => RectangleF.Union(acc, new RectangleF(point, point)));

    private static Fin<Unit> DrawStyled(PaintScope scope, PaintStyle style, Op op, string what, RectangleF bounds, Func<Graphics, Unit> draw) =>
        !scope.VisibilityCulling || bounds == RectangleF.Empty || scope.IsVisible(bounds: bounds)
            ? DrawStyled(scope: scope, style: style, op: op, what: what, draw: draw)
            : Fin.Succ(unit);

    private static Fin<Unit> DrawStyled(PaintScope scope, PaintStyle style, Op op, string what, Func<Graphics, Unit> draw) =>
        Optional(draw).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "draw delegate is required"))
            .Bind(valid => op.Attempt(body: () => {
                Graphics graphics = scope.Graphics.Content;
                using IDisposable state = graphics.SaveTransformState();
                _ = style.Assign(graphics: graphics);
                return valid(arg: graphics);
            }, what: what));

    private static Fin<Unit> DrawPathStyled(PaintScope scope, PaintStyle style, Op op, string what, Func<IGraphicsPath> path) =>
        DrawStyled(scope: scope, style: style, op: op, what: what, draw: g => {
            using IGraphicsPath owned = path();
            return DrawShape(style: style, graphics: g, shape: PaintShape.Path(path: owned));
        });

    private static Fin<Unit> DrawPathStyled(PaintScope scope, PaintStyle style, Op op, string what, RectangleF bounds, Func<IGraphicsPath> path) =>
        !scope.VisibilityCulling || bounds == RectangleF.Empty || scope.IsVisible(bounds: bounds)
            ? DrawPathStyled(scope: scope, style: style, op: op, what: what, path: path)
            : Fin.Succ(unit);

    private static Unit DrawShape(PaintStyle style, Graphics graphics, PaintShape shape) {
        _ = style.FillBrush.IfSome(source => shape.Fill(graphics, source.CachedBrush()));
        _ = style.FillBrush.IfNone(() => style.Fill.IfSome(colour => {
            using SolidBrush brush = PaintStyle.Brush(colour);
            shape.Fill(graphics, brush);
        }));
        using Pen pen = style.Pen();
        shape.Stroke(graphics, pen);
        return unit;
    }

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

        internal static PaintShape Polygon(PointF[] points) =>
            new(
                Fill: (graphics, brush) => graphics.FillPolygon(brush, points),
                Stroke: (graphics, pen) => graphics.DrawPolygon(pen, points));

        // Stroke-only: an open polyline has no fill, so DrawShape routing is for op/cull uniformity, not fill.
        internal static PaintShape Polyline(PointF[] points) =>
            new(
                Fill: static (_, _) => { },
                Stroke: (graphics, pen) => graphics.DrawLines(pen, points));
    }

}

public readonly record struct DrawPlan(Seq<DrawMark> Marks) {
    public static DrawPlan Empty => new(Marks: Seq<DrawMark>());
    public static DrawPlan operator +(DrawPlan left, DrawPlan right) => new(Marks: left.Marks + right.Marks);
    internal Fin<Unit> Apply(PaintScope scope) => Marks.TraverseM(scope.Apply).Map(static marks => unit).As();
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintSkinSnapshot(Option<CanvasSkin> Appearance) {
    public bool HasSkin => Appearance.IsSome;
    public Option<Skin> Skin => Appearance.Map(static a => a.Effective);
}

public abstract record PaintRequest<T> : GhUiRequest<T> {
    public sealed record Skin : PaintRequest<PaintSkinSnapshot> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(); internal override Fin<PaintSkinSnapshot> Apply(GrasshopperUi.Scope scope) => Paint.Skin().Run(scope: scope); }
    public sealed record RedrawOnMouseMove(bool Enabled = true, MotionClock? Clock = null) : PaintRequest<Subscription> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas); internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Paint.RedrawOnMouseMove(enabled: Enabled, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record FloatingDrawable(DrawPlan Plan, Size Size, Option<string> Title = default) : PaintRequest<Subscription> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Paint.FloatingDrawable(plan: Plan, size: Size, title: Title).Run(scope: scope);
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Paint {
    internal static GrasshopperUiIntent<PaintSkinSnapshot> Skin() =>
        GhUi.Canvas(run: scope => scope.NeedCanvasSkin().Map(appearance => new PaintSkinSnapshot(Appearance: Some(appearance))));

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
                    _ = Op.SideWhen(adoptedPacer.IsNone, () => _ = Motion.PacerResume(pacer: pacer, canvas: canvas));
                },
                detach: () => _ = validPhase.Detach(canvas: canvas, handler: handler),
                marshalToUi: true)
            select ownsPacerLifecycle
                ? Subscription.DisposeOnce(Subscription.PaintPacer(
                    paintHook: paintSub,
                    pacerRelease: () => _ = Motion.PacerRelease(pacer: pacer)))
                : paintSub);

    internal static GrasshopperUiIntent<Subscription> RedrawOnMouseMove(bool enabled = true, MotionClock? clock = null) =>
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
                    sustainMouseRedraw: enabled ? () => RedrawOnMouseMoveInstall.IsTop(canvas: canvas, token: token.Value) : null).Run(scope: scope)
                from arm in Events.BindMarshaled(
                    attach: () => {
                        token.Value = RedrawOnMouseMoveInstall.Enter(canvas: canvas, enabled: enabled);
                        _ = Motion.PacerResume(pacer: pacer, canvas: canvas);
                    },
                    detach: () => RedrawOnMouseMoveInstall.Exit(canvas: canvas, token: token.Value))
                select Subscription.DisposeOnce(Subscription.PaintPacer(
                    paintHook: sustain | arm,
                    pacerRelease: () => _ = Motion.PacerRelease(pacer: pacer))));

    internal static GrasshopperUiIntent<Subscription> FloatingDrawable(DrawPlan plan, Size size, Option<string> title) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from skin in scope.NeedSkin()
            from validPlan in Optional(plan).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(FloatingDrawable)), detail: "null draw plan"))
            from validWidth in Op.Of(name: nameof(FloatingDrawable)).AcceptFinite(value: size.Width, detail: "width must be finite and positive", requirePositive: true)
            from validHeight in Op.Of(name: nameof(FloatingDrawable)).AcceptFinite(value: size.Height, detail: "height must be finite and positive", requirePositive: true)
            from app in GrasshopperUi.NeedApplication(op: Op.Of(name: nameof(FloatingDrawable)))
            let validSize = new Size(width: (int)Math.Ceiling(validWidth), height: (int)Math.Ceiling(validHeight))
            let owner = Optional(canvas.ControlObject as Control).Bind(static control => Optional(control.ParentWindow))
            let screenScale = UiRail.PixelScale(canvas: canvas).LogicalPixelSize
            let drawable = new Drawable { Size = validSize }
            let form = new FloatingForm(app: app, owner: owner) {
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
                },
                marshalToUi: true)
            select sub);
}

file static class RedrawOnMouseMoveInstall {
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct Entry(Guid Token, bool Enabled);

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct State(bool Baseline, Seq<Entry> Entries);

    private static readonly Atom<HashMap<int, State>> Stacks = Atom(value: HashMap<int, State>());

    internal static Guid Enter(GhCanvas canvas, bool enabled) {
        Guid token = Guid.NewGuid();
        int key = RuntimeHelpers.GetHashCode(canvas);
        _ = Stacks.Swap(map => map.AddOrUpdate(
            key: key,
            Some: state => state with { Entries = state.Entries + new Entry(Token: token, Enabled: enabled) },
            None: () => new State(Baseline: canvas.RedrawOnMouseMove, Entries: Seq(new Entry(Token: token, Enabled: enabled)))));
        canvas.RedrawOnMouseMove = enabled;
        return token;
    }

    internal static bool IsTop(GhCanvas canvas, Guid token) {
        int key = RuntimeHelpers.GetHashCode(canvas);
        return Stacks.Value.Find(key: key)
            .Map(state => !state.Entries.IsEmpty && state.Entries.Last().Token == token && state.Entries.Last().Enabled)
            .IfNone(false);
    }

    internal static void Exit(GhCanvas canvas, Guid token) {
        int key = RuntimeHelpers.GetHashCode(canvas);
        bool restore = canvas.RedrawOnMouseMove;
        _ = Stacks.Swap(map => map.Find(key).Match(
            Some: state => {
                Seq<Entry> kept = state.Entries.Filter(entry => entry.Token != token);
                restore = kept.IsEmpty ? state.Baseline : kept.Last().Enabled;
                return kept.IsEmpty ? map.Remove(key) : map.SetItem(key, state with { Entries = kept });
            },
            None: () => map));
        canvas.RedrawOnMouseMove = restore;
    }
}
