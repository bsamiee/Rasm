using Eto.Forms;
using Grasshopper2;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Sparkles;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.UI;

namespace Rasm.Grasshopper.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CanvasSparkle(
    FlexControl control, Grasshopper2.UI.Animation.Duration duration, Motion motion, bool attachedToContent, Func<double, Node> view, IPlugInLifetime owner) : ISparkle, IDisposable {
    private readonly Animated<double> progress = Animators.Unfinished(0.0, 1.0, duration, motion);
    private readonly Guest guest = new(owner, IO.lift(control.ScheduleRedraw));

    public bool Sparkling { get => field && progress.State is not Grasshopper2.UI.Animation.State.Finished; private set; } = true;

    public void Draw(ControlGraphics graphics) => Sparkling = owner.Answer(
        (from at in IO.lift(() => control.Animate(progress))
         from finished in IO.lift(() => progress.State is Grasshopper2.UI.Animation.State.Finished)
         from drawn in finished ? IO.lift(guest.Dispose) : CanvasPainting.Painted(guest, control, graphics, attachedToContent, view(at))
         select !finished)
        .Catch(error => IO.lift(guest.Dispose) >> IO.fail<bool>(error)).As(),
        failed: false);

    public void Dispose() => guest.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasPainting {
    // --- [PHASES]
    public static IO<Unit> Overlay<TArgs>(HostEvent<TArgs> phase, Func<TArgs, Node> view, IPlugInLifetime owner) where TArgs : CanvasPaintEventArgs =>
        from guest in use(() => new Guest(owner, CanvasWires.Redrawn))
        from painted in phase.Inline(args => Painted(guest, args.Canvas, args.Graphics, content: true, view(args)), owner)
        select unit;

    public static IO<Unit> Overlay(FlexControl control, Func<ControlDrawEventArgs, Node> view, IPlugInLifetime owner) =>
        from guest in use(() => new Guest(owner, IO.lift(control.ScheduleRedraw)))
        from painted in GrasshopperEvents.FlexControlDraw(control).Inline(args => Painted(guest, control, args.Graphics, content: false, view(args)), owner)
        select unit;

    public static IO<Unit> Background(Canvas canvas, Func<CanvasSkin, CanvasSkin> paper, IPlugInLifetime owner) =>
        GrasshopperEvents.BeforePaintBackground(canvas).Inline(args =>
            IO.lift(static () => Settings.SandBox).Bind(sandbox => unless(sandbox,
                (from projection in use(args.Graphics.ProjectionState)
                 from painted in IO.lift(() => {
                     paper(args.Skin.Canvas).DrawCanvas(args.Graphics.Content, args.Canvas.Projection, args.Canvas.VisibleFrame, 1f, 1f);
                     args.OverrideDefaultPainting();
                 })
                 select unit).Bracket()).As()), owner);

    internal static IO<Unit> Painted(Guest guest, IFlexControl control, ControlGraphics graphics, bool content, Node view) =>
        (from projection in use(graphics.ProjectionState)
         let target = content ? graphics.Content : graphics.Control
         from device in IO.lift(() => Guest.Device(target, Optional(graphics.AssociatedControl as Control)))
         from painted in guest.Paint(target, device, content ? control.VisibleFrame : control.InnerBounds, Visibility.Shown, view)
         select unit).Bracket();

    // --- [SPARKLES]
    public static IO<Unit> Spark(
        FlexControl control, Grasshopper2.UI.Animation.Duration duration, Motion motion, bool attachedToContent, Func<double, Node> view, IPlugInLifetime owner) =>
        IO.lift(() => new CanvasSparkle(control, duration, motion, attachedToContent, view, owner))
            .Bind(sparkle => IO.lift(() => control.AddSparkle(sparkle)));
}
