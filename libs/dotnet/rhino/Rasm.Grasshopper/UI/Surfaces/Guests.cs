using Eto;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Flex;
using Rasm.Rhino;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Grasshopper.UI.Surfaces;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Guest(IPlugInSink sink, IO<Unit> redraw) : IDisposable {
    // --- [STATE]
    private sealed record Mounted(Mount Mount, Shipped Shipped, Shipped Stated, TimeProvider Clock, Node View);

    private sealed record Raster(Bitmap Bitmap, BitmapData Data, SKSurface Surface, PaintCache Cache, SKSizeI Pixels, float Device);

    private static readonly SKSurfaceProperties Properties = new(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.Unknown);
    private readonly SKRegion pending = new();
    private readonly Retained<IDisposable> owned = new(DisposalOps.Release);
    private Option<Mounted> mounted;
    private Option<Raster> raster;
    private Option<Func<bool, IO<Unit>>> running;

    public Option<string> Help => mounted.Bind(static held => held.Mount.Help);

    public Option<Fin<Eto.Forms.Cursor>> Cursor => mounted.Map(static held => HostInput.Cursor(held.Mount.Cursor));

    public Option<SKSize> Preferred(SKSize offered) => mounted.Map(held => held.Mount.Scene.Preferred(offered));

    // --- [SESSION]
    public static IO<Unit> Opened(IPlugInSink owner, TimeProvider clock) =>
        IO.lift(() => Styles.Embedded.Bind(shipped => SceneView.Shipping.Publish(owner, (shipped, clock)))).Bind(static published => published)
            .Catch(static error => error.IsExceptional, static error => IO.fail<Unit>(new UnheldView(typeof(Guest), error)));

    // --- [PAINT]
    public IO<Unit> Paint(Graphics graphics, float device, RectangleF box, Visibility visibility, Node view) =>
        from held in Shown(new SKSize(box.Width, box.Height), visibility, view)
        let pixels = new SKSizeI((int)MathF.Ceiling(box.Width * device), (int)MathF.Ceiling(box.Height * device))
        from painted in when(pixels is { Width: > 0, Height: > 0 },
            from target in Rastered(pixels, device)
            from now in Now(held.Clock)
            from drawn in IO.lift(() => Drawn(target, held.Mount, now))
            from shown in IO.lift(() => graphics.DrawImage(target.Bitmap, box))
            select unit).As()
        select unit;

    public static Fin<float> Device(Graphics graphics, Option<Control> surface) =>
        surface.Match(
            Some: static control => Refused.UnlessFound(control.ParentWindow, nameof(Control.ParentWindow)).Map(static window => window.LogicalPixelSize),
            None: static () => 1f)
        .Map(density => graphics.ScaleFactor() * density);

    public static Fin<float> Device(Grasshopper2.Eto.Drawing.Context context) =>
        Device(context.Graphics, context.Bitmap is not null ? None : Optional(context.Control as Control));

    public static Action<Grasshopper2.Eto.Drawing.Context, Rectangle> TooltipPainter(IPlugInSink sink, Node view, CallbackSite site) =>
        (context, details) => Callbacks.Run(
            DisposalOps.Within(() => new Guest(sink, IO.pure(unit)), guest =>
                from device in IO.lift(() => Device(context))
                from painted in guest.Paint(context.Graphics, device, details, Visibility.Shown, view)
                select unit),
            site);

    private IO<Mounted> Shown(SKSize size, Visibility visibility, Node view) =>
        mounted.Match(
            Some: held =>
                from now in Now(held.Clock)
                from viewed in ReferenceEquals(held.View, view)
                    ? IO.pure(held)
                    : IO.lift(() => held.Mount.Show(view, now)).Bind(moved => Settled(held with { View = view }, moved))
                from hosted in (viewed.Mount.Host with { Size = size, Visibility = visibility }) switch {
                    var host when host == viewed.Mount.Host => IO.pure(viewed),
                    var host => Settled(viewed, viewed.Mount.Rehost(host, now)),
                }
                select hosted,
            None: () =>
                from shipping in SceneView.Shipping.Of(sink)
                from theme in CanvasTheme.Current
                from stated in IO.lift(() => HostTheme.Stated(shipping.Shipped, compact: false))
                from made in IO.lift(() => Mount.Create(new Host(size, theme, visibility, Cycles: false, RowText.Culture), stated, view, []))
                from rethemed in CanvasTheme.Changed.Inline(_ => Rethemed, sink, owned.Retain)
                from applied in Settled(new Mounted(made.Mount, shipping.Shipped, stated, shipping.Clock, view), made)
                select applied);

    private IO<Unit> Rethemed =>
        mounted.Traverse(held =>
            from theme in CanvasTheme.Current
            from stated in IO.lift(() => HostTheme.Stated(held.Shipped, compact: false))
            from now in Now(held.Clock)
            let host = held.Mount.Host with { Theme = theme }
            from moved in stated.Equals(held.Stated)
                ? IO.pure(held.Mount.Rehost(host, now))
                : IO.lift(() => Mount.Create(host, stated, held.View, held.Mount.Scene.Adjusted))
            from applied in Applied(held with { Stated = stated }, moved)
            select unit).As().Map(static _ => unit);

    private IO<Raster> Rastered(SKSizeI pixels, float device) =>
        raster.Filter(held => held.Pixels == pixels && held.Device.Equals(device)).Match(
            Some: static held => IO.pure(held),
            None: () =>
                from released in raster.Traverse(Released).As()
                from order in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<BitmapOrder>(), nameof(BitmapOrder)).Map(static row => row()()))
                from allocated in IO.lift(() => {
                    Bitmap bitmap = new(pixels.Width, pixels.Height, PixelFormat.Format32bppRgba);
                    BitmapData data = bitmap.Lock();
                    SKImageInfo info = new(pixels.Width, pixels.Height, order, data.PremultipliedAlpha ? SKAlphaType.Premul : SKAlphaType.Unpremul, SKColorSpace.CreateSrgb());
                    return Refused.UnlessFound(SKSurface.Create(info, data.Data, data.ScanWidth, Properties), nameof(SKSurface.Create))
                        .Map(surface => new Raster(bitmap, data, surface, new PaintCache(info, Properties), pixels, device))
                        .MapFail(error => {
                            data.Dispose();
                            bitmap.Dispose();
                            return error;
                        });
                })
                from stored in IO.lift(() => {
                    _ = pending.SetRect(SKRectI.Create(pixels));
                    raster = Some(allocated);
                })
                select allocated);

    private Unit Drawn(Raster target, Mount mount, Duration now) {
        if (pending.IsEmpty) return unit;
        SKCanvas canvas = target.Surface.Canvas;
        canvas.ResetMatrix();
        canvas.Scale(target.Device);
        Painter.Paint(canvas, target.Cache, mount, pending, now);
        canvas.Flush();
        pending.SetEmpty();
        return unit;
    }

    // --- [INPUT]
    public IO<Response> Receive(Input input) =>
        mounted.Match(
            Some: held =>
                from now in IO.lift(static () => HostInput.Now)
                let moved = held.Mount.Receive(input, now)
                from applied in Applied(held, moved)
                select moved.Output.Answer.IsNone ? Response.Ignored : (held.Mount.Captured.IsSome, moved.Mount.Captured.IsSome) switch {
                    (false, true) => Response.Capture,
                    (true, false) => Response.Release,
                    _ => Response.Handled,
                },
            None: static () => IO.pure(Response.Ignored));

    public IO<Response> Keyed(KeyEventArgs e) =>
        mounted.Bind(held => HostInput.Keyed(held.Mount, e)).Match(Some: Receive, None: static () => IO.pure(Response.Ignored));

    public IO<Unit> Rehost(Visibility visibility) =>
        mounted.Filter(held => held.Mount.Host.Visibility != visibility).Traverse(held =>
            from now in Now(held.Clock)
            from applied in Applied(held, held.Mount.Rehost(held.Mount.Host with { Visibility = visibility }, now))
            select unit).As().Map(static _ => unit);

    public static MouseEventArgs Local(ResponseMouseArgs args, PointF origin) =>
        new(args.Buttons, args.Modifiers, new PointF(args.ContentLocation.X - origin.X, args.ContentLocation.Y - origin.Y), args.Delta, args.Pressure, args.UnderlyingEtoEventArgs.IsDirectionInverted);

    // --- [OUTPUT]
    private IO<Mounted> Applied(Mounted held, (Mount Mount, Output Output) moved) =>
        from next in Settled(held, moved)
        from redrawn in moved.Output.Damage.IsEmpty ? IO.pure(unit) : redraw
        select next;

    private IO<Mounted> Settled(Mounted held, (Mount Mount, Output Output) moved) =>
        from damaged in IO.lift(() => Damaged(moved.Output.Damage))
        let next = held with { Mount = moved.Mount }
        from stored in IO.lift(() => mounted = Some(next))
        from linked in Linked(moved.Mount.Animating)
        from ran in running.Traverse(run => run(moved.Mount.Animating)).As()
        from answered in moved.Output.Answer.IfNone(IO.pure(unit))
        select next;

    private Unit Damaged(Seq<SKRect> damage) =>
        raster.Iter(held => damage.Iter(rect => pending.Op(
            SKRectI.Ceiling(SKRect.Create(rect.Left * held.Device, rect.Top * held.Device, rect.Width * held.Device, rect.Height * held.Device), outwards: true), SKRegionOperation.Union)));

    private IO<Unit> Linked(bool animating) =>
        when(animating && running.IsNone,
            from link in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<FrameLink>(), nameof(FrameLink)))
            from run in link()(Grasshopper2.UI.Editor.Instance.Canvas, sink)(new Ticks(this), owned.Retain)
            from stored in IO.lift(() => running = Some(run))
            select unit).As();

    // --- [TIME]
    private static IO<Duration> Now(TimeProvider clock) => IO.lift(() => Duration.FromSeconds(clock.GetTimestamp() / (double)clock.TimestampFrequency));

    private sealed record Ticks(Guest Guest) : Sink<Duration> {
        public override IO<Unit> Post(Duration value) =>
            Guest.mounted.Traverse(held => Guest.Applied(held, held.Mount.Tick(value))).As().Map(static _ => unit);

        public override IO<Unit> Complete() => IO.pure(unit);

        public override IO<Unit> Fail(Error Error) => IO.pure(unit);

        public override Sink<X> Comap<X>(Func<X, Duration> f) => Sink<X>.Void.Combine((X x) => (x, f(x)), this);
    }

    // --- [RELEASE]
    private static IO<Unit> Released(Raster held) =>
        Callbacks.Each(Seq(IO.lift(held.Cache.Dispose), IO.lift(held.Surface.Dispose), IO.lift(held.Data.Dispose), IO.lift(held.Bitmap.Dispose))).Map(static _ => unit);

    public void Dispose() {
        owned.Dispose();
        _ = raster.Iter(static held => Released(held).Run());
        raster = None;
        mounted = None;
        running = None;
        pending.Dispose();
    }
}
