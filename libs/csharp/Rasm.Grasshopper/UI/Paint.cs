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
}

// P-003 fix: expanded snapshot carries the Skin instance for callers to query sub-skins (Wires, Shades, Grips, etc.).
[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintSkinSnapshot(bool HasSkin, string SkinType, Option<Skin> Skin);

// --- [SERVICES] --------------------------------------------------------------------------
public static partial class Paint {
    public static GrasshopperUiIntent<PaintSkinSnapshot> Skin() =>
        IntentFactory.Canvas<PaintSkinSnapshot>(run: scope => scope.NeedSkin().Map(skin =>
            new PaintSkinSnapshot(
                HasSkin: true,
                SkinType: skin.GetType().FullName ?? skin.GetType().Name,
                Skin: Some(skin))));

    public static GrasshopperUiIntent<IDisposable> Hook(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint) =>
        IntentFactory.Canvas<IDisposable>(run: scope =>
            from canvas in scope.NeedCanvas()
            from valid in Optional(paint).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: "null paint callback"))
            from phaseCase in PhaseCases.Find(p => p.Phase == phase)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Hook)), detail: $"unknown phase {phase}"))
            select (IDisposable)PaintSubscription.Attach(canvas: canvas, phaseCase: phaseCase, paint: valid));

    // P-002 fix: parameterized — no longer asymmetric (was always true-only in pre-refactor).
    public static GrasshopperUiIntent<Unit> RedrawOnMouseMove(bool enabled = true) =>
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

// --- [OPERATIONS] ------------------------------------------------------------------------
// P-004 fix: drawing helpers on PaintScope via extension methods using ControlGraphics + Skin.
public static class PaintScopeExtensions {
    public static Unit DrawLine(this PaintScope scope, PointF a, PointF b, Color colour, float thickness = 1f) {
        using Pen pen = new(color: colour, thickness: thickness);
        scope.Graphics.Content.DrawLine(pen, a.X, a.Y, b.X, b.Y);
        return unit;
    }

    public static Unit DrawRectangle(this PaintScope scope, RectangleF bounds, Color edge, Color? fill = null, float thickness = 1f) {
        Optional(fill).IfSome(f => {
            using SolidBrush brush = new(color: f);
            scope.Graphics.Content.FillRectangle(brush: brush, rectangle: bounds);
        });
        using Pen pen = new(color: edge, thickness: thickness);
        scope.Graphics.Content.DrawRectangle(pen: pen, rectangle: bounds);
        return unit;
    }

    public static Unit DrawText(this PaintScope scope, string text, PointF location, Color colour, Font? font = null) {
        using SolidBrush brush = new(color: colour);
        scope.Graphics.Content.DrawText(
            font: font ?? SystemFonts.Default(),
            brush: brush,
            location: location,
            text: text);
        return unit;
    }
}
