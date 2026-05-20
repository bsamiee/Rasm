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
    Skin Skin,
    bool DefaultBackgroundOverridden) {
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

public abstract record DrawMark {
    public sealed record Line(PointF A, PointF B, Color Colour, float Thickness = 1f) : DrawMark;
    public sealed record Rectangle(RectangleF Bounds, Color Edge, Option<Color> Fill = default, float Thickness = 1f) : DrawMark;
    public sealed record Ellipse(RectangleF Bounds, Color Edge, Option<Color> Fill = default, float Thickness = 1f) : DrawMark;
    public sealed record Label(string Value, PointF Location, Color Colour, Option<Font> Font = default) : DrawMark;
    public sealed record Image(Eto.Drawing.Image Value, RectangleF Frame) : DrawMark;
    public sealed record IconGlyph(IIcon Value, RectangleF Frame, Color Background) : DrawMark;
    public sealed record WirePreview(PointF Source, PointF Target, WireKind Kind = WireKind.Tentative) : DrawMark;

    internal Fin<Unit> Apply(PaintScope scope) =>
        Try.lift<Unit>(f: () => {
            switch (this) {
                case Line line:
                    using (Pen pen = new(color: line.Colour, thickness: line.Thickness)) {
                        scope.Graphics.Content.DrawLine(pen, line.A.X, line.A.Y, line.B.X, line.B.Y);
                    }
                    break;
                case Rectangle rectangle:
                    _ = rectangle.Fill.IfSome(fill => {
                        using SolidBrush brush = new(color: fill);
                        scope.Graphics.Content.FillRectangle(brush: brush, rectangle: rectangle.Bounds);
                    });
                    using (Pen pen = new(color: rectangle.Edge, thickness: rectangle.Thickness)) {
                        scope.Graphics.Content.DrawRectangle(pen: pen, rectangle: rectangle.Bounds);
                    }
                    break;
                case Ellipse ellipse:
                    _ = ellipse.Fill.IfSome(fill => {
                        using SolidBrush brush = new(color: fill);
                        scope.Graphics.Content.FillEllipse(brush: brush, rectangle: ellipse.Bounds);
                    });
                    using (Pen pen = new(color: ellipse.Edge, thickness: ellipse.Thickness)) {
                        scope.Graphics.Content.DrawEllipse(pen: pen, rectangle: ellipse.Bounds);
                    }
                    break;
                case Label text:
                    using (SolidBrush brush = new(color: text.Colour)) {
                        scope.Graphics.Content.DrawText(font: text.Font.IfNone(SystemFonts.Default()), brush: brush, location: text.Location, text: text.Value);
                    }
                    break;
                case Image image:
                    scope.Graphics.Content.DrawImage(image: image.Value, rectangle: image.Frame);
                    break;
                case IconGlyph icon:
                    icon.Value.Draw(context: new IconContext(
                        context: Eto.Drawing.Context.CreateFromContent(graphics: scope.Graphics),
                        frame: icon.Frame,
                        background: icon.Background));
                    break;
                case WirePreview wire:
                    WireSkin skin = scope.Skin.Wires[wire.Kind];
                    WireShape shape = WireShape.Create(source: wire.Source, target: wire.Target);
                    using (Pen pen = new(color: skin.Normal, thickness: skin.Outer.Width)) {
                        skin.Outer.AssignToPen(pen: pen);
                        shape.Draw(graphics: scope.Graphics.Content, edge: pen);
                    }
                    break;
                default:
                    throw new InvalidOperationException(message: $"Unknown draw mark {GetType().Name}");
            }
            return unit;
        }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(DrawMark)), detail: $"{GetType().Name} draw failed"));
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
        IntentFactory.Canvas<PaintSkinSnapshot>(run: scope => scope.NeedSkin().Map(skin =>
            new PaintSkinSnapshot(
                HasSkin: true,
                SkinType: skin.GetType().FullName ?? skin.GetType().Name,
                Skin: Some(skin))));

    internal static GrasshopperUiIntent<IDisposable> Hook(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint) =>
        IntentFactory.Canvas<IDisposable>(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(paint).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null paint callback"))
            from phaseCase in PhaseCases.Find(p => p.Phase == phase)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: $"unknown phase {phase}"))
            select (IDisposable)PaintSubscription.Attach(canvas: canvas, phaseCase: phaseCase, paint: valid));

    internal static GrasshopperUiIntent<Unit> RedrawOnMouseMove(bool enabled = true) =>
        IntentFactory.Canvas<Unit>(
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
                    Skin: args.Skin,
                    DefaultBackgroundOverridden: (args as CanvasBackgroundPaintEventArgs)?.DefaultOverridden ?? false) {
                    Background = Optional(args as CanvasBackgroundPaintEventArgs),
                }));
            EventHandler<CanvasPaintEventArgs> handler = Handler;
            phaseCase.Attach(arg1: canvas, arg2: handler);
            return new PaintSubscription(dispose: () => phaseCase.Detach(arg1: canvas, arg2: handler));
        }
    }
}
