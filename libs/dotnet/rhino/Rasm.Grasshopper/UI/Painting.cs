using Eto.Forms;
using Grasshopper2;
using Grasshopper2.UI.Animation;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Sparkles;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.UI;

namespace Rasm.Grasshopper.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CanvasSparkle(
    FlexControl control, Grasshopper2.UI.Animation.Duration duration, Motion motion, bool attachedToContent, Func<double, Node> view, CallbackSite site) : ISparkle, IDisposable {
    private readonly Animated<double> progress = Animators.Unfinished(0.0, 1.0, duration, motion);
    private readonly Guest guest = new(site.Sink, IO.lift(control.ScheduleRedraw));

    public bool Sparkling { get => field && progress.State is not Grasshopper2.UI.Animation.State.Finished; private set; } = true;

    public void Draw(ControlGraphics graphics) => Sparkling = Callbacks.Answer(
        from at in IO.lift(() => control.Animate(progress))
        from finished in IO.lift(() => progress.State is Grasshopper2.UI.Animation.State.Finished)
        from drawn in finished ? IO.lift(guest.Dispose) : CanvasPainting.Painted(guest, control, graphics, attachedToContent, view(at))
        select !finished,
        Released,
        site);

    private bool Released() {
        guest.Dispose();
        return false;
    }

    public void Dispose() => guest.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasPainting {
    // --- [PHASES]
    public static IO<Unit> Overlay<TArgs>(HostEvent<TArgs> phase, Func<TArgs, Node> view, IPlugInSink sink, Action<IDisposable> retain) where TArgs : CanvasPaintEventArgs =>
        from guest in DisposalOps.Acquire(IO.lift(() => new Guest(sink, CanvasWires.Redrawn)), retain)
        from painted in phase.Inline(args => Painted(guest, args.Canvas, args.Graphics, content: true, view(args)), sink, retain)
        select unit;

    public static IO<Unit> Overlay(FlexControl control, Func<ControlDrawEventArgs, Node> view, IPlugInSink sink, Action<IDisposable> retain) =>
        from guest in DisposalOps.Acquire(IO.lift(() => new Guest(sink, IO.lift(control.ScheduleRedraw))), retain)
        from painted in GrasshopperEvents.FlexControlDraw(control).Inline(args => Painted(guest, control, args.Graphics, content: false, view(args)), sink, retain)
        select unit;

    public static IO<Unit> Background(Canvas canvas, Func<CanvasSkin, CanvasSkin> paper, IPlugInSink sink, Action<IDisposable> retain) =>
        GrasshopperEvents.BeforePaintBackground(canvas).Inline(args =>
            IO.lift(static () => Settings.SandBox).Bind(sandbox => unless(sandbox,
                DisposalOps.Within(args.Graphics.ProjectionState, _ => IO.lift(() => {
                    paper(args.Skin.Canvas).DrawCanvas(args.Graphics.Content, args.Canvas.Projection, args.Canvas.VisibleFrame, 1f, 1f);
                    args.OverrideDefaultPainting();
                }))).As()), sink, retain);

    internal static IO<Unit> Painted(Guest guest, IFlexControl control, ControlGraphics graphics, bool content, Node view) =>
        DisposalOps.Within(graphics.ProjectionState, _ => (content ? graphics.Content : graphics.Control) switch {
            var target =>
                from device in IO.lift(() => Guest.Device(target, Optional(graphics.AssociatedControl as Control)))
                from painted in guest.Paint(target, device, content ? control.VisibleFrame : control.InnerBounds, Visibility.Shown, view)
                select unit,
        });

    // --- [SPARKLES]
    public static IO<Unit> Spark(
        FlexControl control, Grasshopper2.UI.Animation.Duration duration, Motion motion, bool attachedToContent, Func<double, Node> view, IPlugInSink sink) =>
        IO.lift(() => new CanvasSparkle(control, duration, motion, attachedToContent, view, new CallbackSite(sink, typeof(CanvasSparkle), nameof(ISparkle.Draw))))
            .Bind(sparkle => IO.lift(() => control.AddSparkle(sparkle)));
}
