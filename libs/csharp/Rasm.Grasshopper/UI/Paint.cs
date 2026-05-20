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

// --- [TYPES] -----------------------------------------------------------------------------
public enum CanvasPaintPhase { BeforeBackground, AfterBackground, BeforeGroups, AfterGroups, BeforeWires, AfterWires, BeforeObjects, AfterObjects, }

// --- [MODELS] ----------------------------------------------------------------------------
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
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintStyle(Color Edge, Option<Color> Fill = default, float Thickness = 1f, Option<Font> Font = default, Color Background = default);

[Union]
public partial record DrawMark {
    private DrawMark() { }
    public sealed record LineCase(PointF A, PointF B, PaintStyle Style) : DrawMark;
    public sealed record RectangleCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed record EllipseCase(RectangleF Bounds, PaintStyle Style) : DrawMark;
    public sealed record TextCase(string Value, PointF Location, PaintStyle Style) : DrawMark;
    public sealed record ImageCase(Eto.Drawing.Image Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed record IconCase(IIcon Value, RectangleF Frame, PaintStyle Style) : DrawMark;
    public sealed record WireCase(PointF Source, PointF Target, WireKind Kind, PaintStyle Style) : DrawMark;

    public static DrawMark Line(PointF a, PointF b, Color colour, float thickness = 1f) =>
        new LineCase(A: a, B: b, Style: new PaintStyle(Edge: colour, Thickness: thickness));
    public static DrawMark Rectangle(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new RectangleCase(Bounds: bounds, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark Ellipse(RectangleF bounds, Color edge, Option<Color> fill = default, float thickness = 1f) =>
        new EllipseCase(Bounds: bounds, Style: new PaintStyle(Edge: edge, Fill: fill, Thickness: thickness));
    public static DrawMark Label(string value, PointF location, Color colour, Option<Font> font = default) =>
        new TextCase(Value: value, Location: location, Style: new PaintStyle(Edge: colour, Font: font));
    public static DrawMark Image(Eto.Drawing.Image value, RectangleF frame) =>
        new ImageCase(Value: value, Frame: frame, Style: new PaintStyle(Edge: Colors.Transparent));
    public static DrawMark IconGlyph(IIcon value, RectangleF frame, Color background) =>
        new IconCase(Value: value, Frame: frame, Style: new PaintStyle(Edge: Colors.Transparent, Background: background));
    public static DrawMark WirePreview(PointF source, PointF target, WireKind kind = WireKind.Tentative) =>
        new WireCase(Source: source, Target: target, Kind: kind, Style: new PaintStyle(Edge: Colors.Transparent));

    internal Fin<Unit> Apply(PaintScope scope) =>
        Try.lift<Unit>(f: () => this switch {
            LineCase line => DrawLine(scope: scope, line: line),
            RectangleCase rectangle => DrawRectangle(scope: scope, rectangle: rectangle),
            EllipseCase ellipse => DrawEllipse(scope: scope, ellipse: ellipse),
            TextCase text => DrawText(scope: scope, text: text),
            ImageCase image => DrawImage(scope: scope, image: image),
            IconCase icon => DrawIcon(scope: scope, icon: icon),
            WireCase wire => DrawWire(scope: scope, wire: wire),
            _ => unit,
        }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(DrawMark)), detail: $"{GetType().Name} draw failed"));

    private static Unit DrawLine(PaintScope scope, LineCase line) {
        using Pen pen = new(color: line.Style.Edge, thickness: line.Style.Thickness);
        scope.Graphics.Content.DrawLine(pen, line.A.X, line.A.Y, line.B.X, line.B.Y);
        return unit;
    }

    private static Unit DrawRectangle(PaintScope scope, RectangleCase rectangle) {
        _ = rectangle.Style.Fill.IfSome(fill => {
            using SolidBrush brush = new(color: fill);
            scope.Graphics.Content.FillRectangle(brush: brush, rectangle: rectangle.Bounds);
        });
        using Pen pen = new(color: rectangle.Style.Edge, thickness: rectangle.Style.Thickness);
        scope.Graphics.Content.DrawRectangle(pen: pen, rectangle: rectangle.Bounds);
        return unit;
    }

    private static Unit DrawEllipse(PaintScope scope, EllipseCase ellipse) {
        _ = ellipse.Style.Fill.IfSome(fill => {
            using SolidBrush brush = new(color: fill);
            scope.Graphics.Content.FillEllipse(brush: brush, rectangle: ellipse.Bounds);
        });
        using Pen pen = new(color: ellipse.Style.Edge, thickness: ellipse.Style.Thickness);
        scope.Graphics.Content.DrawEllipse(pen: pen, rectangle: ellipse.Bounds);
        return unit;
    }

    private static Unit DrawText(PaintScope scope, TextCase text) {
        using SolidBrush brush = new(color: text.Style.Edge);
        scope.Graphics.Content.DrawText(font: text.Style.Font.IfNone(SystemFonts.Default()), brush: brush, location: text.Location, text: text.Value);
        return unit;
    }

    private static Unit DrawImage(PaintScope scope, ImageCase image) {
        scope.Graphics.Content.DrawImage(image: image.Value, rectangle: image.Frame);
        return unit;
    }

    private static Unit DrawIcon(PaintScope scope, IconCase icon) {
        icon.Value.Draw(context: new IconContext(
            context: Eto.Drawing.Context.CreateFromContent(graphics: scope.Graphics),
            frame: icon.Frame,
            background: icon.Style.Background));
        return unit;
    }

    private static Unit DrawWire(PaintScope scope, WireCase wire) {
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

// --- [SERVICES] --------------------------------------------------------------------------
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

    // --- [OPERATIONS] ----------------------------------------------------------------------
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
