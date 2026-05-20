using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Rasm.Domain;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
public enum CanvasPaintPhase {
    BeforeBackground,
    AfterBackground,
    BeforeGroups,
    AfterGroups,
    BeforeWires,
    AfterWires,
    BeforeObjects,
    AfterObjects,
}

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

    public Fin<Unit> Apply(PaintMark mark) {
        PaintScope current = this;
        return Optional(mark)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Apply)), detail: "paint mark is required"))
            .Bind(valid => valid.Apply(scope: current));
    }
}

public abstract record PaintMark {
    public sealed record Line(PointF A, PointF B, Color Colour, float Thickness = 1f) : PaintMark;
    public sealed record Rectangle(RectangleF Bounds, Color Edge, Option<Color> Fill = default, float Thickness = 1f) : PaintMark;
    public sealed record Label(string Value, PointF Location, Color Colour, Option<Font> Font = default) : PaintMark;

    internal Fin<Unit> Apply(PaintScope scope) => this switch {
        Line line => Try.lift<Unit>(f: () => {
            using Pen pen = new(color: line.Colour, thickness: line.Thickness);
            scope.Graphics.Content.DrawLine(pen, line.A.X, line.A.Y, line.B.X, line.B.Y);
            return unit;
        }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Line)), detail: "DrawLine threw")),
        Rectangle rectangle => Try.lift<Unit>(f: () => {
            _ = rectangle.Fill.IfSome(fill => {
                using SolidBrush brush = new(color: fill);
                scope.Graphics.Content.FillRectangle(brush: brush, rectangle: rectangle.Bounds);
            });
            using Pen pen = new(color: rectangle.Edge, thickness: rectangle.Thickness);
            scope.Graphics.Content.DrawRectangle(pen: pen, rectangle: rectangle.Bounds);
            return unit;
        }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Rectangle)), detail: "DrawRectangle threw")),
        Label text => Try.lift<Unit>(f: () => {
            using SolidBrush brush = new(color: text.Colour);
            scope.Graphics.Content.DrawText(font: text.Font.IfNone(SystemFonts.Default()), brush: brush, location: text.Location, text: text.Value);
            return unit;
        }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Label)), detail: "DrawText threw")),
        _ => Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(PaintMark)), detail: "unknown paint mark")),
    };
}

public readonly record struct PaintPlan(Seq<PaintMark> Marks) {
    public static PaintPlan Empty => new(Marks: Seq<PaintMark>());
    public static PaintPlan operator +(PaintPlan left, PaintPlan right) => new(Marks: left.Marks + right.Marks);
    public static PaintPlan Add(PaintPlan left, PaintPlan right) => left + right;
    internal Fin<Unit> Apply(PaintScope scope) => Marks.TraverseM(scope.Apply).Map(static marks => unit).As();
}

// P-003 fix: expanded snapshot carries the Skin instance for callers to query sub-skins (Wires, Shades, Grips, etc.).
[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintSkinSnapshot(bool HasSkin, string SkinType, Option<Skin> Skin);

public abstract record PaintRequest<T> : GhUiRequest<T> {
    public sealed record Skin : PaintRequest<PaintSkinSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<PaintSkinSnapshot> Apply(GrasshopperUi.Scope scope) => Paint.Skin().Run(scope: scope);
    }
    public sealed record Hook(CanvasPaintPhase Phase, PaintPlan Plan) : PaintRequest<IDisposable> {
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

    // P-002 fix: parameterized — no longer asymmetric (was always true-only in pre-refactor).
    internal static GrasshopperUiIntent<Unit> RedrawOnMouseMove(bool enabled = true) =>
        IntentFactory.Canvas<Unit>(
            repaint: RepaintRequest.Canvas,
            run: scope => scope.NeedCanvas().Map(canvas => { canvas.RedrawOnMouseMove = enabled; return unit; }));

    // --- [OPERATIONS] ----------------------------------------------------------------------
    // P-001 fix: 8-arm switch on CanvasPaintPhase becomes Seq<PaintPhaseCase> lattice.
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
