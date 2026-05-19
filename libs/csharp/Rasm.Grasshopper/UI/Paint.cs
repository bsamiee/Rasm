using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;

namespace Rasm.Grasshopper.UI;

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

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintScope(CanvasPaintPhase Phase, ControlGraphics Graphics, Skin Skin, bool DefaultBackgroundOverridden) {
    public Unit OverrideDefault() =>
        Background.Match(
            Some: args => {
                args.OverrideDefaultPainting();
                return unit;
            },
            None: () => unit);

    internal Option<CanvasBackgroundPaintEventArgs> Background { get; init; }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PaintSkinSnapshot(bool HasSkin, string SkinType);

// --- [INTENTS] --------------------------------------------------------------------------
public static class PaintIntent {
    public static GrasshopperUiIntent<PaintSkinSnapshot> Skin() =>
        new(
            run: scope => scope.Skin
                .Map(skin => new PaintSkinSnapshot(HasSkin: true, SkinType: skin.GetType().FullName ?? skin.GetType().Name))
                .ToFin(Fail: Op.Of(name: nameof(Skin)).InvalidInput()),
            policy: GrasshopperUiPolicy.Canvas());

    public static GrasshopperUiIntent<IDisposable> Hook(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint) =>
        new(
            run: scope =>
                from canvas in scope.Canvas.ToFin(Fail: Op.Of(name: nameof(Hook)).InvalidInput())
                from valid in Optional(paint).ToFin(Fail: Op.Of(name: nameof(Hook)).InvalidInput())
                from subscription in PaintSubscription.Attach(canvas: canvas, phase: phase, paint: valid)
                select (IDisposable)subscription,
            policy: GrasshopperUiPolicy.Canvas());

    public static GrasshopperUiIntent<Unit> RedrawOnMouseMove() =>
        new(
            run: scope => scope.Canvas
                .ToFin(Fail: Op.Of(name: nameof(RedrawOnMouseMove)).InvalidInput())
                .Map(canvas => {
                    canvas.RedrawOnMouseMove = true;
                    return unit;
                }),
            policy: GrasshopperUiPolicy.Canvas(repaint: true));

    private sealed class PaintSubscription : IDisposable {
        private readonly Action dispose;

        private PaintSubscription(Action dispose) => this.dispose = dispose;

        public void Dispose() =>
            _ = GrasshopperUi.OnUiThread(run: () => GrasshopperUi.Protect(valid: () => {
                dispose();
                return Fin.Succ(value: unit);
            }));

        internal static Fin<PaintSubscription> Attach(Canvas canvas, CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint) =>
            phase switch {
                CanvasPaintPhase.BeforeBackground => AttachCanvas(add: handler => canvas.BeforePaintBackground += handler, remove: handler => canvas.BeforePaintBackground -= handler, phase: phase, paint: paint),
                CanvasPaintPhase.AfterBackground => AttachCanvas(add: handler => canvas.AfterPaintBackground += handler, remove: handler => canvas.AfterPaintBackground -= handler, phase: phase, paint: paint),
                CanvasPaintPhase.BeforeGroups => AttachCanvas(add: handler => canvas.BeforePaintGroups += handler, remove: handler => canvas.BeforePaintGroups -= handler, phase: phase, paint: paint),
                CanvasPaintPhase.AfterGroups => AttachCanvas(add: handler => canvas.AfterPaintGroups += handler, remove: handler => canvas.AfterPaintGroups -= handler, phase: phase, paint: paint),
                CanvasPaintPhase.BeforeWires => AttachCanvas(add: handler => canvas.BeforePaintWires += handler, remove: handler => canvas.BeforePaintWires -= handler, phase: phase, paint: paint),
                CanvasPaintPhase.AfterWires => AttachCanvas(add: handler => canvas.AfterPaintWires += handler, remove: handler => canvas.AfterPaintWires -= handler, phase: phase, paint: paint),
                CanvasPaintPhase.BeforeObjects => AttachCanvas(add: handler => canvas.BeforePaintObjects += handler, remove: handler => canvas.BeforePaintObjects -= handler, phase: phase, paint: paint),
                CanvasPaintPhase.AfterObjects => AttachCanvas(add: handler => canvas.AfterPaintObjects += handler, remove: handler => canvas.AfterPaintObjects -= handler, phase: phase, paint: paint),
                _ => Fin.Fail<PaintSubscription>(error: Op.Of(name: nameof(Attach)).InvalidInput()),
            };

        private static PaintSubscription AttachCanvas(Action<EventHandler<CanvasPaintEventArgs>> add, Action<EventHandler<CanvasPaintEventArgs>> remove, CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> paint) {
            void Handler(object? sender, CanvasPaintEventArgs args) =>
                _ = GrasshopperUi.Protect(valid: () => paint(arg: new PaintScope(Phase: phase, Graphics: args.Graphics, Skin: args.Skin, DefaultBackgroundOverridden: (args as CanvasBackgroundPaintEventArgs)?.DefaultOverridden ?? false) {
                    Background = Optional(args as CanvasBackgroundPaintEventArgs),
                }));
            EventHandler<CanvasPaintEventArgs> handler = Handler;
            add(handler);
            return new PaintSubscription(dispose: () => remove(handler));
        }
    }
}
