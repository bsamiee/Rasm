using System.Runtime.CompilerServices;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Primitives;
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
public readonly record struct PaintScope(CanvasPaintPhase Phase, ControlGraphics Graphics, Skin Skin) {
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

    // TextMeasure-routed: Eto.Mac 2.11 GraphicsHandler.MeasureString leaks FormattedText state across calls.
    public Fin<PaintTextMeasurement> MeasureText(string value, Option<UiFont> font = default) =>
        Optional(value)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(MeasureText)), detail: "text is required"))
            .Bind(valid => Try.lift(f: () =>
                font.IfNone(UiFont.Empty()).Use(run: resolved =>
                    new PaintTextMeasurement(Text: valid, Size: TextMeasure.Single(font: resolved, text: valid))
                )).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(MeasureText)), detail: error.Message)));

    // SaveTransformState does NOT capture clipping; ResetClip in finally is mandatory.
    public Fin<Unit> Clip(IGraphicsPath path, Func<PaintScope, Fin<Unit>> body) {
        PaintScope self = this;
        return Optional(path)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Clip)), detail: "clip path is required"))
            .Bind(validPath => Optional(body).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Clip)), detail: "body is required"))
            .Bind(validBody => {
                Graphics graphics = self.Graphics.Content;
                graphics.SetClip(path: validPath);
                try {
                    return validBody(arg: self);
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
        Fin<Unit> finite =
            from t in op.AcceptFinite(value: thicknessValue, detail: $"Thickness must be finite and >= 0 (got {thicknessValue:R}).", nonNegative: true)
            from d in op.AcceptFinite(value: dashOffsetValue, detail: $"DashOffset must be finite (got {dashOffsetValue:R}).")
            select unit;
        Fin<Unit> miter = float.IsFinite(miterLimitValue) && miterLimitValue >= 1f
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(error: UiFault.InvalidInput(op: op, detail: $"MiterLimit must be finite and >= 1 (got {miterLimitValue:R})."));
        Fin<Unit> valid = finite.Bind(_ => miter);
        UiFault? fault = null;
        _ = valid.IfFail(err => { fault = (UiFault)err; return unit; });
        _ = edge;
        _ = fill;
        _ = font;
        _ = background;
        _ = lineCap;
        _ = lineJoin;
        _ = dash;
        _ = antiAlias;
        _ = imageInterpolation;
        _ = pixelOffset;
        _ = fillBrush;
        _ = edgeBrush;
        validationError = fault;
    }

    // Eto 2.11 lacks Pen.DashOffset setter; DashStyle is sealed-immutable. DashStyleIntern deduplicates
    // quantized-offset DashStyle instances while each disposable Pen/Brush remains caller-owned.
    // EdgeSource owns pen construction; EdgeBrush selects brush-backed strokes, FillBrush owns fills only.
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

    internal int Count {
        get { using (gate.EnterScope()) { return entries.Count; } }
    }

    internal Unit Clear() {
        using (gate.EnterScope()) {
            entries.Clear();
            order.Clear();
        }
        return unit;
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

    internal static int Count => Cache.Count;
    internal static Unit Clear() => Cache.Clear();
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

    internal static int Count => Cache.Count;
    internal static Unit Clear() => Cache.Clear();
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
            solidCase: static s => new SolidBrush(color: s.Colour),
            linearCase: static l => new LinearGradientBrush(startColor: l.Start, endColor: l.End, startPoint: l.From, endPoint: l.To) { Wrap = l.Wrap },
            radialCase: static r => new RadialGradientBrush(startColor: r.Centre, endColor: r.Edge, center: r.Origin, gradientOrigin: r.Focus, radius: r.Radius) { Wrap = r.Wrap },
            textureCase: static t => new TextureBrush(image: t.Source, opacity: t.Opacity));
}

[SkipUnionOps]
[Union]
public partial record EdgeSource {
    private EdgeSource() { }
    public sealed record SolidCase(Color Colour) : EdgeSource;
    public sealed record BrushBackedCase(FillSource Source) : EdgeSource;

    public static EdgeSource Solid(Color colour) => new SolidCase(Colour: colour);
    public static EdgeSource FromFill(FillSource source) => new BrushBackedCase(Source: source);

    internal Pen CreatePen(float thickness, PenLineCap cap, PenLineJoin join, float miterLimit, DashStyle dash) =>
        Switch(
            solidCase: s => new Pen(color: s.Colour, thickness: thickness) { LineCap = cap, LineJoin = join, MiterLimit = miterLimit, DashStyle = dash },
            brushBackedCase: b => new Pen(brush: b.Source.CreateBrush(), thickness: thickness) { LineCap = cap, LineJoin = join, MiterLimit = miterLimit, DashStyle = dash });

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
    public sealed partial record GhIconCase(IIcon Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed partial record WireCase(PointF Source, PointF Target, WireKind Kind, PaintStyle Style) : DrawMark;
    public sealed partial record CapsuleCase(Capsule Value, Parts Elements, Shade Shade) : DrawMark;

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
    public static DrawMark IconGlyph(IIcon value, RectangleF frame, Color background) =>
        new GhIconCase(Value: value, Frame: frame, Style: PaintStyle.ForTransparent(background: background));
    public static DrawMark WirePreview(PointF source, PointF target, WireKind kind = WireKind.Tentative) =>
        new WireCase(Source: source, Target: target, Kind: kind, Style: PaintStyle.ForTransparent());
    public static DrawMark DrawCapsule(Capsule capsule, Shade shade, Parts elements = Parts.All) =>
        new CapsuleCase(Value: capsule, Elements: elements, Shade: shade);

    internal Fin<Unit> Apply(PaintScope scope) =>
        Switch(state: scope,
            lineCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: LineCase.SelfOp, what: "line draw", draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Line(a: c.A, b: c.B))),
            rectangleCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: RectangleCase.SelfOp, what: "rectangle draw", draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Rectangle(bounds: c.Bounds))),
            roundedRectangleCase: static (s, c) => DrawPathStyled(
                scope: s,
                style: c.Style,
                op: RoundedRectangleCase.SelfOp,
                what: "rounded rectangle draw",
                path: () => GraphicsPath.GetRoundRect(rectangle: c.Bounds, radius: c.Radius)),
            roundedCornersCase: static (s, c) => DrawPathStyled(
                scope: s,
                style: c.Style,
                op: RoundedCornersCase.SelfOp,
                what: "rounded corners draw",
                path: () => c.Radii.IsUniform
                    ? GraphicsPath.GetRoundRect(rectangle: c.Bounds, radius: c.Radii.TopLeft)
                    : GraphicsPath.GetRoundRect(
                        rectangle: c.Bounds,
                        nwRadius: c.Radii.TopLeft,
                        neRadius: c.Radii.TopRight,
                        seRadius: c.Radii.BottomRight,
                        swRadius: c.Radii.BottomLeft)),
            ellipseCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: EllipseCase.SelfOp, what: "ellipse draw", draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Ellipse(bounds: c.Bounds))),
            pathCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: PathCase.SelfOp, what: "path draw", draw: g => DrawShape(style: c.Style, graphics: g, shape: PaintShape.Path(path: c.Geometry))),
            textCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: TextCase.SelfOp, what: "text draw", draw: g =>
                c.Style.Font.IfNone(UiFont.Empty()).Use(run: font => {
                    using SolidBrush brush = PaintStyle.Brush(color: c.Style.Edge);
                    g.DrawText(font, brush, c.Frame, c.Value, c.Wrap, c.Alignment, c.Trimming);
                    return unit;
                })),
            imageCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: ImageCase.SelfOp, what: "image draw", draw: g => {
                g.DrawImage(image: c.Value, rectangle: c.Frame);
                return unit;
            }),
            ghIconCase: static (s, c) => DrawStyled(scope: s, style: c.Style, op: GhIconCase.SelfOp, what: "icon draw", draw: _ => {
                c.Value.Draw(context: new IconContext(
                    context: Eto.Drawing.Context.CreateFromContent(graphics: s.Graphics),
                    frame: c.Frame,
                    background: c.Style.Background));
                return unit;
            }),
            wireCase: static (s, w) => DrawStyled(scope: s, style: w.Style, op: WireCase.SelfOp, what: "wire draw", draw: graphics => {
                WireSkin skin = s.Skin.Wires[w.Kind];
                WireShape shape = WireShape.Create(source: w.Source, target: w.Target);
                using Pen outerPen = new(color: skin.Normal, thickness: skin.Outer.Width);
                skin.Outer.AssignToPen(pen: outerPen);
                shape.Draw(graphics: graphics, edge: outerPen);
                _ = Optional(skin.Inner).Iter(inner => {
                    using Pen innerPen = new(color: skin.Normal, thickness: inner.Width);
                    inner.AssignToPen(pen: innerPen);
                    shape.Draw(graphics: graphics, edge: innerPen);
                });
                return unit;
            }),
            capsuleCase: static (s, c) => CapsuleCase.SelfOp.Attempt(body: () => {
                c.Value.Draw(graphics: s.Graphics.Content, elements: c.Elements, shade: c.Shade, skin: s.Skin);
                return unit;
            }, what: "capsule draw"));

    private static Fin<Unit> DrawStyled(PaintScope scope, PaintStyle style, Op op, string what, Func<Graphics, Unit> draw) =>
        op.Attempt(body: () => {
            Graphics graphics = scope.Graphics.Content;
            using IDisposable state = graphics.SaveTransformState();
            _ = style.Assign(graphics: graphics);
            return draw(arg: graphics);
        }, what: what);

    private static Fin<Unit> DrawPathStyled(PaintScope scope, PaintStyle style, Op op, string what, Func<IGraphicsPath> path) =>
        DrawStyled(scope: scope, style: style, op: op, what: what, draw: g => {
            using IGraphicsPath owned = path();
            return DrawShape(style: style, graphics: g, shape: PaintShape.Path(path: owned));
        });

    private static Unit DrawShape(PaintStyle style, Graphics graphics, PaintShape shape) {
        _ = style.FillBrush.IfSome(source => {
            using Brush brush = source.CreateBrush();
            shape.Fill(graphics, brush);
        });
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
    public sealed record Skin : PaintRequest<PaintSkinSnapshot> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(); internal override Fin<PaintSkinSnapshot> Apply(GrasshopperUi.Scope scope) => Paint.Skin().Run(scope: scope); }
    public sealed record Hook(CanvasPaintPhase Phase, DrawPlan Plan, MotionClock? Clock = null) : PaintRequest<Subscription> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(); internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Paint.Hook(phase: Phase, paint: Plan.Apply, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record RedrawOnMouseMove(bool Enabled = true, MotionClock? Clock = null) : PaintRequest<Subscription> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas); internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Paint.RedrawOnMouseMove(enabled: Enabled, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
    public sealed record SolutionOverlay(DrawPlan Plan, MotionClock? Clock = null) : PaintRequest<Subscription> { internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled); internal override Fin<Subscription> Apply(GrasshopperUi.Scope scope) => Paint.Hook(phase: CanvasPaintPhase.AfterObjects, paint: Plan.Apply, clock: Clock ?? MotionClock.MessageLoop).Run(scope: scope); }
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Paint {
    internal static GrasshopperUiIntent<PaintSkinSnapshot> Skin() =>
        GhUi.Canvas(run: scope => scope.NeedSkin().Map(skin =>
            new PaintSkinSnapshot(
                HasSkin: true,
                SkinType: skin.GetType().FullName ?? skin.GetType().Name,
                Skin: Some(skin))));

    internal static GrasshopperUiIntent<Subscription> Hook(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint, MotionClock clock) =>
        GhUi.Canvas(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(paint).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null paint callback"))
            from validPhase in Optional(phase).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null phase"))
            from pacer in Motion.PacerOption(canvas: canvas, clock: clock)
            let handler = (EventHandler<CanvasPaintEventArgs>)((_, args) => GrasshopperUi.Handler(valid: () => valid(arg: new PaintScope(
                Phase: validPhase,
                Graphics: args.Graphics,
                Skin: args.Skin) {
                Background = Optional(args as CanvasBackgroundPaintEventArgs),
            })).Ignore())
            from sub in Subscription.Bind(
                attach: () => {
                    _ = validPhase.Attach(canvas: canvas, handler: handler);
                    _ = Motion.PacerResume(pacer: pacer, canvas: canvas);
                },
                detach: () => {
                    _ = validPhase.Detach(canvas: canvas, handler: handler);
                    _ = Motion.PacerRelease(pacer: pacer);
                },
                marshalToUi: true)
            select sub);

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
                        _ = Motion.PacerResume(pacer: pacer, canvas: canvas);
                    },
                    detach: () => {
                        canvas.RedrawOnMouseMove = priorEnabled;
                        _ = Motion.PacerRelease(pacer: pacer);
                    },
                    marshalToUi: true)
                select sub);
}
