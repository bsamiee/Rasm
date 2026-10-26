using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rhino.Display;
using SkiaSharp;

namespace Rasm.Rhino.UI.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate IO<Unit> OpaqueTarget(SceneView view, Action<IDisposable> retain);

public interface IDrawnPlatform {
    public Fin<Duration> EventTime { get; }

    public Fin<int> EventClicks { get; }

    public Fin<Cursor> ClosedHand { get; }

    public Fin<float> PointerTravel(PointF at, PointF last);

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInSink sink, Action<IDisposable> retain);

    public IO<Unit> SlotSeated(Control slot, Option<string> label);

    public IO<Func<Mount, Output, IO<Unit>>> NativeScene(SceneView view, Action<IDisposable> retain);

    public IO<OpaqueTarget> OpaqueTargets(IPlugInSink owner, Action<IDisposable> retain);

    public Fin<Unit> ExtendedPresent(Control view, Graphics graphics, SKImage frame, RectangleF destination);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BitmapPixels(Bitmap Bitmap) : IDisposable {
    public Eto.Drawing.BitmapData Data { get; } = Bitmap.Lock();

    public SKPixmap Pixmap =>
        new(new SKImageInfo(Bitmap.Width, Bitmap.Height, (Data.TranslateArgbToData(unchecked((int)0xFFFF0000)) & 0xFF) is 0xFF ? SKColorType.Rgba8888 : SKColorType.Bgra8888,
                Data.PremultipliedAlpha ? SKAlphaType.Premul : SKAlphaType.Unpremul, HostPixels.Srgb),
            Data.Data, Data.ScanWidth);

    public static readonly Func<SKSizeI, Fin<(BitmapPixels Pixels, SKPixmap Pixmap)>> Allocated =
        static pixels => new BitmapPixels(new Bitmap(pixels.Width, pixels.Height, Eto.Drawing.PixelFormat.Format32bppRgba)) switch { var held => (held, held.Pixmap) };

    public void Dispose() {
        Data.Dispose();
        Bitmap.Dispose();
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneView : Drawable {
    // --- [STATE]
    private sealed record Seat(PixelLayout Holder, Control Control);

    private readonly RowScope scope;
    private readonly PixelLayout seats = new();
    private Node view;
    private Option<IDisposable> listening;
    private Option<SceneHost<BitmapPixels>> session;
    private PlacedControls<Seat> placing;
    private Option<CursorShape> pointer;
    private (bool Shown, bool Key) native;
    private int marking;

    public SceneView(RowScope scope, Node view) {
        (this.scope, this.view) = (scope, view);
        placing = Seating;
        Content = seats;
        CanFocus = true;
        Style = HandlerStyle;
        Callbacks.Run(ViewOps.WhileLoaded(this, Listened, Site(nameof(ViewOps.WhileLoaded)), lease => listening = Some(lease)), Site());
    }

    public static string HandlerStyle { get; } = $"{typeof(SceneView).Assembly.GetName().Name}.{nameof(SceneView)}";

    public bool Cycles { get; init; }

    public Func<Seq<Key>, IO<Control>> Slots { get; init; } = static path => IO.fail<Control>(new Refused(nameof(Slots), $"no control for slot /{string.Join('/', path.Map(static key => key.Identifier))}"));

    internal Option<Mount> Current => session.Bind(static held => held.Current);

    internal IPlugInSink Sink => scope.Sink;

    public static PlugInSlot<TimeProvider> Clocks { get; } = new();

    public static IDrawnPlatform Drawn => Platform.Instance.CreateShared<IDrawnPlatform>();

    internal static PlugInSlot<OpaqueTarget> Opaque { get; } = new();

    internal static OpaqueTarget Bitmapped { get; } = static (_, _) => IO.pure(unit);

    // --- [SHOW]
    public IO<Unit> Show(Node next) =>
        IO.lift(() => view = next).Bind(_ => session.Traverse(held => held.Show(next)).As()).Map(static _ => unit);

    // --- [SESSION]
    public static Seq<Func<IPlugInSink, Action<IDisposable>, IO<Unit>>> Opened(ViewCatalog catalog, TimeProvider clock) =>
        catalog.Rows.Bind(ViewCatalog.Tree).Exists(static held => held.Children.Exists(static child => !child.Controls.IsEmpty))
            ? [(owner, retain) => Published(owner, clock, retain)]
            : [];

    private static IO<Unit> Published(IPlugInSink owner, TimeProvider clock, Action<IDisposable> retain) =>
        from clocked in Clocks.Publish(owner, clock)
        from opaque in (from target in Drawn.OpaqueTargets(owner, retain)
                        from published in Opaque.Publish(owner, target)
                        select unit).MapFail(static error => new UnheldView(typeof(SceneView), error))
        select unit;

    // --- [LIFETIME]
    private IO<Unit> Listened(Action<IDisposable> retain) =>
        from clock in Clocks.Of(scope.Sink)
        from projected in Drawn.NativeScene(this, retain)
        from host in DisposalOps.Acquire(IO.lift(() => new SceneHost<BitmapPixels>(Backend(projected, retain), clock)), Released, retain)
        from held in IO.lift(() => session = Some(host))
        from shown in host.Show(view)
        from hosted in Rehosted
        from theme in HostTheme.In(this).Inline(_ => Rehosted, scope.Sink, retain)
        from visible in scope.OnVisibility(_ => Rehosted, retain)
        select unit;

    private SceneBackend<BitmapPixels> Backend(Func<Mount, Output, IO<Unit>> projected, Action<IDisposable> retain) =>
        new(BitmapPixels.Allocated, damage => IO.lift(() => damage.Iter(rect => Invalidate(Rectangle.Ceiling(new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height))))), tick => Drawn.FrameLink(this, tick, scope.Sink, retain)) { Applied = (mount, output) => Applied(projected, mount, output) };

    private IO<Unit> Rehosted =>
        session.Traverse(held =>
            from host in Hosting
            from moved in held.Rehost(host, HostTheme.Stated(HostTheme.Compact(this)))
            select unit).As().Map(static _ => unit);

    private IO<Host> Hosting =>
        scope.Visibility.Map(visible => new Host(
            new SKSize(Math.Max(0, Width), Math.Max(0, Height)),
            HostTheme.Projected(HostTheme.Compact(this)),
            Visibility.Of(visible && native.Shown, native.Key),
            Cycles,
            RowText.Culture));

    private IO<Unit> Released(SceneHost<BitmapPixels> host) =>
        from released in host.Released
        from unseated in placing.Released
        from cleared in IO.lift(() => {
            (session, placing, pointer) = (None, Seating, None);
            host.Dispose();
        })
        select unit;

    internal IO<Unit> Natively(bool shown, bool key) => IO.lift(() => native = (shown, key)).Bind(_ => Rehosted);

    internal void Marked(int offset) => marking = offset;

    // --- [OUTPUT]
    private IO<Unit> Applied(Func<Mount, Output, IO<Unit>> projected, Mount mount, Output output) =>
        from placed in placing.Reconciled(mount)
        from stored in IO.lift(() => placing = placed)
        from cursor in when(pointer != Some(mount.Cursor), IO.lift(() => HostInput.Cursor(mount.Cursor)).Map(shown => {
            (Cursor, pointer) = (shown, Some(mount.Cursor));
            return unit;
        })).As()
        from shown in projected(mount, output)
        from fitted in IO.lift(Fitted)
        select unit;

    private Unit Fitted() =>
        session.Bind(held => held.Preferred(new SKSize(Math.Max(0, Width), float.PositiveInfinity)))
            .Map(static preferred => preferred.Height)
            .Filter(float.IsFinite)
            .Map(static preferred => (int)MathF.Ceiling(preferred))
            .Filter(preferred => preferred != Height)
            .Iter(preferred => Height = preferred);

    // --- [SLOTS]
    private PlacedControls<Seat> Seating =>
        new(
            (path, placed, element) =>
                from control in Slots(path)
                from seat in IO.lift(() => {
                    PixelLayout holder = new();
                    holder.Add(control, placed.Frame.Left, placed.Frame.Top);
                    seats.Add(holder, placed.Clip.Left, placed.Clip.Top);
                    return Sized(new Seat(holder, control), placed);
                })
                from seated in Drawn.SlotSeated(control, element.Bind(static held => held.Accessible.Label))
                select seat,
            (seat, placed) => IO.lift(() => {
                seats.Move(seat.Holder, placed.Clip.Left, placed.Clip.Top);
                seat.Holder.Move(seat.Control, placed.Frame.Left, placed.Frame.Top);
                return Sized(seat, placed);
            }),
            seat => IO.lift(() => Unseated(seat)));

    private static Seat Sized(Seat seat, (SKRectI Clip, SKRectI Frame) placed) {
        seat.Holder.Size = new Size(placed.Clip.Width, placed.Clip.Height);
        seat.Control.Size = new Size(placed.Frame.Width, placed.Frame.Height);
        return seat;
    }

    private Unit Unseated(Seat seat) {
        seats.Remove(seat.Holder);
        seat.Holder.Remove(seat.Control);
        seat.Control.Dispose();
        seat.Holder.Dispose();
        return unit;
    }

    // --- [PAINT]
    protected override void OnPaint(PaintEventArgs e) {
        Callbacks.Run(Painted(e.Graphics), Site());
        base.OnPaint(e);
    }

    private IO<Unit> Painted(Graphics graphics) =>
        session.Traverse(held =>
            from scale in IO.lift(() => Refused.UnlessFound(ParentWindow, nameof(ParentWindow)).Map(static window => window.LogicalPixelSize))
            from target in held.Paint(scale)
            from shown in target.Traverse(painted => Blitted(graphics, painted)).As()
            select unit).As().Map(static _ => unit);

    private IO<Unit> Blitted(Graphics graphics, SceneTarget<BitmapPixels> target) =>
        target.Switch(
            (View: this, Graphics: graphics, Destination: new RectangleF(0f, 0f, Width, Height)),
            standard: static (held, standard) => IO.lift(() => held.Graphics.DrawImage(standard.Pixels.Bitmap, new RectangleF(0f, 0f, standard.Info.Width, standard.Info.Height), held.Destination)),
            extended: static (held, extended) =>
                IO.lift(() => Drawn.ExtendedPresent(held.View, held.Graphics, extended.Surface.Snapshot(), held.Destination)));

    // --- [INPUT]
    internal bool Receive(Input input, Duration now) =>
        Callbacks.Answer(session.Match(Some: held => held.Receive(input, now), None: static () => IO.pure(value: false)), static () => false, Site());

    internal bool Timed(Input input) =>
        Callbacks.Answer(session.Match(Some: held => held.Now.Bind(now => held.Receive(input, now)), None: static () => IO.pure(value: false)), static () => false, Site());

    private bool Received(Fin<Option<Input>> input) =>
        Callbacks.Answer(from held in IO.lift(() => input) from now in IO.lift(static () => HostInput.Now) select held.Exists(shown => Receive(shown, now)), static () => false, Site());

    protected override void OnMouseDown(MouseEventArgs e) {
        e.Handled = Received(HostInput.Clicks.Map(clicks => HostInput.Pressed(e, clicks)));
        if (e.Handled && !HasFocus) Focus();
        if (!e.Handled) base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        e.Handled = Received(HostInput.Clicks.Map(clicks => HostInput.Moved(e, clicks)));
        if (!e.Handled) base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        e.Handled = Received(HostInput.Clicks.Map(clicks => HostInput.Released(e, clicks)));
        if (!e.Handled) base.OnMouseUp(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        _ = Timed(HostInput.Left(e));
        base.OnMouseLeave(e);
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        _ = Timed(new Input.Focused());
    }

    protected override void OnLostFocus(EventArgs e) {
        _ = Timed(new Input.Blurred());
        base.OnLostFocus(e);
    }

    protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        Callbacks.Run(Rehosted, Site());
    }

    // --- [KEYS]
    protected override void OnKeyDown(KeyEventArgs e) {
        e.Handled = Current.Exists(mount => HostInput.Keyed(mount, e).Exists(Timed));
        if (!e.Handled) base.OnKeyDown(e);
    }

    protected override void OnTextInput(TextInputEventArgs e) {
        e.Cancel = Timed(new Input.Perform(new Act.Insert(e.Text), new Target.Focused()));
        base.OnTextInput(e);
    }

    protected override void OnTextComposition(TextCompositionEventArgs e) {
        e.Handled = Timed(new Input.Perform(new Act.Compose(e.IsActive ? e.Text : "", e.IsActive ? marking : 0), new Target.Focused()));
        base.OnTextComposition(e);
    }

    protected override void OnTextInsertionBoundsRequested(TextInsertionBoundsEventArgs e) {
        e.Bounds = Current.Bind(static held => held.Caret).Map<RectangleF?>(static caret => new RectangleF(caret.Left, caret.Top, caret.Width, caret.Height)).ValueUnsafe();
        base.OnTextInsertionBoundsRequested(e);
    }

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing) {
                _ = listening.Iter(static lease => lease.Dispose());
                seats.Dispose();
            }
        } finally {
            base.Dispose(disposing);
        }
    }

    private CallbackSite Site([CallerMemberName] string member = "") => new(scope.Sink, typeof(SceneView), member);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPixels {
    internal static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

    extension(Color4f color) {
        public SKColorF ToSKColorF() => new(color.R, color.G, color.B, color.A);
    }

    extension(System.Drawing.Bitmap bitmap) {
        public IO<SKImage> ToSKImage() =>
            DisposalOps.Within(
                () => bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb),
                static data => IO.lift(() => Refused.UnlessFound(
                    SKImage.FromPixelCopy(new SKImageInfo(data.Width, data.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul, Srgb), data.Scan0, data.Stride), nameof(SKImage.FromPixelCopy))),
                data => IO.lift(() => bitmap.UnlockBits(data)));
    }
}
