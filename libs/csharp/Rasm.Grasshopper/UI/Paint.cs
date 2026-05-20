using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Skinning;
using Rasm.Domain;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public enum CanvasPaintPhase { BeforeBackground, AfterBackground, BeforeGroups, AfterGroups, BeforeWires, AfterWires, BeforeObjects, AfterObjects, }

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintScope(
    CanvasPaintPhase Phase,
    ControlGraphics Graphics,
    Skin Skin) {
    public bool DefaultBackgroundOverridden =>
        Background.Map(static args => args.DefaultOverridden).IfNone(false);

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

    public Fin<PaintTextMeasurement> MeasureText(string value, UiFont font = null!) {
        ControlGraphics graphics = Graphics;
        return Optional(value)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(MeasureText)), detail: "text is required"))
            .Bind(valid => Try.lift<PaintTextMeasurement>(f: () =>
                (font ?? UiFont.Empty()).Use(run: resolved => {
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
    UiFont Font = null!,
    Color Background = default,
    PenLineCap LineCap = PenLineCap.Butt,
    PenLineJoin LineJoin = PenLineJoin.Miter,
    DashStyle Dash = null!,
    float MiterLimit = 10f,
    bool AntiAlias = true,
    ImageInterpolation ImageInterpolation = ImageInterpolation.Default,
    PixelOffsetMode PixelOffset = PixelOffsetMode.None) {
    internal Pen Pen() =>
        new(color: Edge, thickness: Thickness) {
            LineCap = LineCap,
            LineJoin = LineJoin,
            DashStyle = Dash ?? DashStyles.Solid,
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

    internal T Use<T>(Func<Font, T> run) =>
        this switch {
            SystemCase system => run(arg: SystemFonts.Cached(systemFont: system.Kind, size: system.Size, decoration: system.Decoration)),
            FamilyCase family => UseOwned(font: new Font(family: family.Name, size: family.Size, style: family.Style, decoration: family.Decoration), run: run),
            NativeCase native => run(arg: native.Value),
            _ => run(arg: SystemFonts.Default()),
        };

    private static T UseOwned<T>(Font font, Func<Font, T> run) {
        using Font owned = font;
        return run(arg: owned);
    }
}

[Union]
public partial record DrawMark {
    private DrawMark() { }
    public sealed record LineCase(PointF A, PointF B, PaintStyle Style) : DrawMark;
    public sealed record RectangleCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed record RoundedRectangleCase(RectangleF Bounds, float Radius, PaintStyle Style) : DrawMark;
    public sealed record EllipseCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed record PathCase(IGraphicsPath Geometry, PaintStyle Style) : DrawMark;
    public sealed record TextCase(string Value, RectangleF Frame, PaintStyle Style, FormattedTextWrapMode Wrap, FormattedTextAlignment Alignment, FormattedTextTrimming Trimming) : DrawMark;
    public sealed record ImageCase(Eto.Drawing.Image Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed record GhIconCase(IIcon Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed record WireCase(PointF Source, PointF Target, WireKind Kind, PaintStyle Style) : DrawMark;

    public static DrawMark Line(PointF a, PointF b, Color colour, float thickness = 1f) =>
        new LineCase(A: a, B: b, Style: new PaintStyle(Edge: colour, Thickness: thickness));
    public static DrawMark Rectangle(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RectangleCase(Bounds: bounds, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark RoundedRectangle(RectangleF bounds, float radius, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RoundedRectangleCase(Bounds: bounds, Radius: radius, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark Ellipse(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new EllipseCase(Bounds: bounds, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark Path(IGraphicsPath path, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new PathCase(Geometry: path, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark Label(
        string value,
        RectangleF frame,
        Color colour,
        UiFont font = null!,
        FormattedTextWrapMode wrap = FormattedTextWrapMode.Word,
        FormattedTextAlignment alignment = FormattedTextAlignment.Left,
        FormattedTextTrimming trimming = FormattedTextTrimming.WordEllipsis) =>
        new TextCase(Value: value, Frame: frame, Style: new PaintStyle(Edge: colour, Font: font ?? UiFont.Empty()), Wrap: wrap, Alignment: alignment, Trimming: trimming);
    public static DrawMark Image(Eto.Drawing.Image value, RectangleF frame) =>
        new ImageCase(Value: value, Frame: frame, Style: new PaintStyle(Edge: Colors.Transparent));
    public static DrawMark IconGlyph(IIcon value, RectangleF frame, Color background) =>
        new GhIconCase(Value: value, Frame: frame, Style: new PaintStyle(Edge: Colors.Transparent, Background: background));
    public static DrawMark WirePreview(PointF source, PointF target, WireKind kind = WireKind.Tentative) =>
        new WireCase(Source: source, Target: target, Kind: kind, Style: new PaintStyle(Edge: Colors.Transparent));

    internal Fin<Unit> Apply(PaintScope scope) =>
        Try.lift<Unit>(f: () => this switch {
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
            EllipseCase ellipse => Draw(scope: scope, style: ellipse.Style, run: graphics => {
                PaintShape shape = PaintShape.Ellipse(bounds: ellipse.Bounds);
                return DrawShape(graphics: graphics, style: ellipse.Style, shape: shape);
            }),
            PathCase path => Draw(scope: scope, style: path.Style, run: graphics => {
                PaintShape shape = PaintShape.Path(path: path.Geometry);
                return DrawShape(graphics: graphics, style: path.Style, shape: shape);
            }),
            TextCase text => Draw(scope: scope, style: text.Style, run: graphics =>
                (text.Style.Font ?? UiFont.Empty()).Use(run: font => {
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
        }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(DrawMark)), detail: $"{GetType().Name} draw failed"));

    private static Unit Draw(PaintScope scope, PaintStyle style, Func<Graphics, Unit> run) {
        Graphics graphics = scope.Graphics.Content;
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
        _ = wire.Style.Assign(graphics: scope.Graphics.Content);
        WireSkin skin = scope.Skin.Wires[wire.Kind];
        WireShape shape = WireShape.Create(source: wire.Source, target: wire.Target);
        using Pen pen = new(color: skin.Normal, thickness: skin.Outer.Width);
        skin.Outer.AssignToPen(pen: pen);
        shape.Draw(graphics: scope.Graphics.Content, edge: pen);
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
    public sealed record Hook(CanvasPaintPhase Phase, DrawPlan Plan) : PaintRequest<IDisposable> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<IDisposable> Apply(GrasshopperUi.Scope scope) => Paint.Hook(phase: Phase, paint: Plan.Apply).Run(scope: scope);
    }
    public sealed record RedrawOnMouseMove(bool Enabled = true) : PaintRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas);
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Paint.RedrawOnMouseMove(enabled: Enabled).Run(scope: scope);
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Paint {
    internal static GrasshopperUiIntent<PaintSkinSnapshot> Skin() =>
        GhUi.Canvas<PaintSkinSnapshot>(run: scope => scope.NeedSkin().Map(skin =>
            new PaintSkinSnapshot(
                HasSkin: true,
                SkinType: skin.GetType().FullName ?? skin.GetType().Name,
                Skin: Some(skin))));

    internal static GrasshopperUiIntent<IDisposable> Hook(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint) =>
        GhUi.Canvas<IDisposable>(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(paint).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null paint callback"))
            from phaseCase in PhaseCases.Find(p => p.Phase == phase)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: $"unknown phase {phase}"))
            select (IDisposable)PaintSubscription.Attach(canvas: canvas, phaseCase: phaseCase, paint: valid));

    internal static GrasshopperUiIntent<Unit> RedrawOnMouseMove(bool enabled = true) =>
        GhUi.Canvas<Unit>(
            repaint: RepaintRequest.Canvas,
            run: scope => scope.NeedCanvas().Map(canvas => { canvas.RedrawOnMouseMove = enabled; return unit; }));

    // --- [OPERATIONS] -------------------------------------------------------------------------
    private readonly record struct PaintPhaseCase(
        CanvasPaintPhase Phase,
        Action<GhCanvas, EventHandler<CanvasPaintEventArgs>> Attach,
        Action<GhCanvas, EventHandler<CanvasPaintEventArgs>> Detach);

    private static readonly Seq<PaintPhaseCase> PhaseCases = Seq(
        new PaintPhaseCase(Phase: CanvasPaintPhase.BeforeBackground, Attach: static (c, h) => c.BeforePaintBackground += h, Detach: static (c, h) => c.BeforePaintBackground -= h),
        new PaintPhaseCase(Phase: CanvasPaintPhase.AfterBackground, Attach: static (c, h) => c.AfterPaintBackground += h, Detach: static (c, h) => c.AfterPaintBackground -= h),
        new PaintPhaseCase(Phase: CanvasPaintPhase.BeforeGroups, Attach: static (c, h) => c.BeforePaintGroups += h, Detach: static (c, h) => c.BeforePaintGroups -= h),
        new PaintPhaseCase(Phase: CanvasPaintPhase.AfterGroups, Attach: static (c, h) => c.AfterPaintGroups += h, Detach: static (c, h) => c.AfterPaintGroups -= h),
        new PaintPhaseCase(Phase: CanvasPaintPhase.BeforeWires, Attach: static (c, h) => c.BeforePaintWires += h, Detach: static (c, h) => c.BeforePaintWires -= h),
        new PaintPhaseCase(Phase: CanvasPaintPhase.AfterWires, Attach: static (c, h) => c.AfterPaintWires += h, Detach: static (c, h) => c.AfterPaintWires -= h),
        new PaintPhaseCase(Phase: CanvasPaintPhase.BeforeObjects, Attach: static (c, h) => c.BeforePaintObjects += h, Detach: static (c, h) => c.BeforePaintObjects -= h),
        new PaintPhaseCase(Phase: CanvasPaintPhase.AfterObjects, Attach: static (c, h) => c.AfterPaintObjects += h, Detach: static (c, h) => c.AfterPaintObjects -= h));

    private sealed class PaintSubscription : IDisposable {
        private readonly System.Action dispose;
        private PaintSubscription(System.Action dispose) => this.dispose = dispose;
        public void Dispose() =>
            _ = GrasshopperUi.OnUiThread(run: () => GrasshopperUi.Protect(valid: () => {
                dispose();
                return Fin.Succ(value: unit);
            }));

        internal static PaintSubscription Attach(GhCanvas canvas, PaintPhaseCase phaseCase, Func<PaintScope, Fin<Unit>> paint) {
            void Handler(object? sender, CanvasPaintEventArgs args) =>
                _ = GrasshopperUi.Protect(valid: () => paint(arg: new PaintScope(
                    Phase: phaseCase.Phase,
                    Graphics: args.Graphics,
                    Skin: args.Skin) {
                    Background = Optional(args as CanvasBackgroundPaintEventArgs),
                }));
            EventHandler<CanvasPaintEventArgs> handler = Handler;
            phaseCase.Attach(arg1: canvas, arg2: handler);
            return new PaintSubscription(dispose: () => phaseCase.Detach(arg1: canvas, arg2: handler));
        }
    }
}
