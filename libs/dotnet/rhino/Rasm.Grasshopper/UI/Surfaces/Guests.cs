using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Flex;
using Rasm.Rhino;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Grasshopper.UI.Surfaces;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Guest(IPlugInSink sink, IO<Unit> redraw) : IDisposable {
    // --- [STATE]
    private sealed record Session(SceneHost<BitmapPixels> Host, DefaultStyles Stated, ColorScheme Theme);

    private readonly Retained<IDisposable> owned = new(DisposalOps.Release);
    private Option<Session> session;

    private Option<Mount> Current => session.Bind(static held => held.Host.Current);

    public Option<string> Help => Current.Bind(static mount => mount.Help);

    public Option<Fin<Eto.Forms.Cursor>> Cursor => Current.Map(static mount => HostInput.Cursor(mount.Cursor));

    public Option<SKSize> Preferred(SKSize offered) => session.Bind(held => held.Host.Preferred(offered));

    // --- [SESSION]
    public static IO<Unit> Opened(IPlugInSink owner, TimeProvider clock) => SceneView.Clocks.Publish(owner, clock);

    private IO<Session> Held =>
        session.Match(
            Some: static held => IO.pure(held),
            None: () =>
                from clock in SceneView.Clocks.Of(sink)
                from theme in CanvasTheme.Current
                from host in IO.lift(() => new SceneHost<BitmapPixels>(
                    new SceneBackend<BitmapPixels>(BitmapPixels.Allocated, _ => redraw, tick => Editors.Canvas.Bind(canvas => SceneView.Drawn.FrameLink(canvas, tick, sink, owned.Retain))),
                    clock))
                let made = new Session(host, HostTheme.Stated(compact: false), theme)
                from stored in IO.lift(() => session = Some(made))
                from rethemed in CanvasTheme.Changed.Inline(_ => Rethemed, sink, owned.Retain)
                select made);

    private IO<Unit> Rethemed =>
        session.Traverse(held =>
            from theme in CanvasTheme.Current
            let stated = HostTheme.Stated(compact: false)
            from stored in IO.lift(() => session = Some(held with { Stated = stated, Theme = theme }))
            from moved in held.Host.Current.Traverse(mount => held.Host.Rehost(mount.Host with { Theme = theme }, stated)).As()
            select unit).As().Map(static _ => unit);

    // --- [PAINT]
    public IO<Unit> Paint(Graphics graphics, float device, RectangleF box, Visibility visibility, Node view) =>
        from held in Held
        from shown in held.Host.Show(view)
        from hosted in held.Host.Rehost(new Host(new SKSize(box.Width, box.Height), held.Theme, visibility, HostPlacement.Panel, RowText.Culture), held.Stated)
        from target in held.Host.Paint(device)
        from drawn in target.Traverse(painted => Blitted(graphics, painted, box)).As()
        select unit;

    private static IO<Unit> Blitted(Graphics graphics, SceneTarget<BitmapPixels> target, RectangleF box) =>
        target.Pixels.Match(
            Some: pixels => IO.lift(() => graphics.DrawImage(pixels.Bitmap, new RectangleF(0f, 0f, target.Info.Width, target.Info.Height), box)),
            None: () => DisposalOps.Within(target.Surface.Snapshot, frame =>
                DisposalOps.Within(
                    IO.lift(() => BitmapPixels.Allocated(new SKSizeI(frame.Width, frame.Height))),
                    allocated =>
                        from read in IO.lift(() => Refused.Unless(frame.ReadPixels(allocated.Pixmap), nameof(SKImage.ReadPixels)))
                        from drawn in IO.lift(() => graphics.DrawImage(allocated.Pixels.Bitmap, new RectangleF(0f, 0f, frame.Width, frame.Height), box))
                        select unit,
                    static allocated => IO.lift(() => {
                        allocated.Pixmap.Dispose();
                        allocated.Pixels.Dispose();
                    }))));

    public static Fin<float> Device(Graphics graphics, Option<Control> surface) =>
        surface.Match(
            Some: static control => Refused.UnlessFound(control.ParentWindow, nameof(Control.ParentWindow)).Map(static window => window.LogicalPixelSize),
            None: static () => 1f)
        .Map(density => graphics.ScaleFactor() * density);

    public static Fin<float> Device(Context context) =>
        Device(context.Graphics, context.Bitmap is not null ? None : Optional(context.Control as Control));

    public static Action<Context, Rectangle> TooltipPainter(IPlugInSink sink, Node view, CallbackSite site) =>
        (context, details) => Callbacks.Run(
            DisposalOps.Within(() => new Guest(sink, IO.pure(unit)), guest =>
                from device in IO.lift(() => Device(context))
                from painted in guest.Paint(context.Graphics, device, details, Visibility.Shown, view)
                select unit),
            site);

    // --- [INPUT]
    public IO<Response> Receive(Input input) =>
        session.Match(
            Some: held =>
                from before in IO.lift(() => Current.Exists(static mount => mount.Captured.IsSome))
                from now in IO.lift(static () => HostInput.Now)
                from answered in held.Host.Receive(input, now)
                from after in IO.lift(() => Current.Exists(static mount => mount.Captured.IsSome))
                select !answered ? Response.Ignored : (before, after) switch {
                    (false, true) => Response.Capture,
                    (true, false) => Response.Release,
                    _ => Response.Handled,
                },
            None: static () => IO.pure(Response.Ignored));

    public IO<Response> Keyed(KeyEventArgs e) =>
        Current.Bind(mount => HostInput.Keyed(mount, e)).Match(Some: Receive, None: static () => IO.pure(Response.Ignored));

    public IO<Unit> Rehost(Visibility visibility) =>
        session.Traverse(held => held.Host.Current.Filter(mount => mount.Host.Visibility != visibility)
            .Traverse(mount => held.Host.Rehost(mount.Host with { Visibility = visibility }, held.Stated)).As()).As().Map(static _ => unit);

    public static MouseEventArgs Local(ResponseMouseArgs args, PointF origin) =>
        new(args.Buttons, args.Modifiers, new PointF(args.ContentLocation.X - origin.X, args.ContentLocation.Y - origin.Y), args.Delta, args.Pressure, args.UnderlyingEtoEventArgs.IsDirectionInverted);

    // --- [RELEASE]
    public void Dispose() {
        Option<Session> held = session;
        session = None;
        _ = held.Iter(static current => current.Host.Released.Run());
        owned.Dispose();
        _ = held.Iter(static current => current.Host.Dispose());
    }
}
