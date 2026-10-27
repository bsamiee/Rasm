using Grasshopper2;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Sparkles;

namespace Rasm.Grasshopper.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CanvasSparkle(FlexControl control, Grasshopper2.UI.Animation.Duration duration, Motion motion, Func<double, ControlGraphics, IO<Unit>> view, IGrasshopperPlugin owner) : ISparkle {
    private readonly Animated<double> progress = Animators.Unfinished(0.0, 1.0, duration, motion);

    public bool Sparkling { get => field && progress.State is not Grasshopper2.UI.Animation.State.Finished; private set; } = true;

    public void Draw(ControlGraphics graphics) => Sparkling = owner.Session.Answer(
        from at in IO.lift(() => control.Animate(progress))
        from finished in IO.lift(() => progress.State is Grasshopper2.UI.Animation.State.Finished)
        from drawn in unless(finished, view(at, graphics)).As()
        select !finished,
        failed: false);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasPainting {
    // --- [BACKGROUNDS]
    public static IO<Unit> Background(Canvas canvas, Func<CanvasSkin, CanvasSkin> paper, IGrasshopperPlugin owner) =>
        Hooks.On<CanvasBackgroundPaintEventArgs>(h => canvas.BeforePaintBackground += h, h => canvas.BeforePaintBackground -= h, args =>
            IO.lift(static () => Settings.SandBox).Bind(sandbox => unless(sandbox,
                (from projection in use(args.Graphics.ProjectionState)
                 from painted in IO.lift(() => {
                     paper(args.Skin.Canvas).DrawCanvas(args.Graphics.Content, args.Canvas.Projection, args.Canvas.VisibleFrame, 1f, 1f);
                     args.OverrideDefaultPainting();
                 })
                 select unit).Bracket()).As()), owner);

    // --- [SPARKLES]
    public static IO<Unit> Spark(FlexControl control, Grasshopper2.UI.Animation.Duration duration, Motion motion, Func<double, ControlGraphics, IO<Unit>> view, IGrasshopperPlugin owner) =>
        IO.lift(() => control.AddSparkle(new CanvasSparkle(control, duration, motion, view, owner)));
}
