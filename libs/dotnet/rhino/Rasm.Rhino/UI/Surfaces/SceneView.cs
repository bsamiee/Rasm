using System.Drawing.Imaging;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.UI;
using Rhino.Display;
using SkiaSharp;

namespace Rasm.RhinoCommon.UI.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate IO<Unit> OpaqueTarget(SceneView view);

public interface IDrawnPlatform {
    public Fin<Duration> EventTime { get; }

    public Fin<int> EventClicks { get; }

    public Fin<Cursor> ClosedHand { get; }

    public Fin<float> PointerTravel(PointF at, PointF last);

    public IO<Func<bool, IO<Unit>>> FrameLink(Control view, Func<Duration, IO<Unit>> tick, IPlugInLifetime owner);

    public IO<Unit> SlotSeated(Control slot, Option<string> label);

    public IO<Func<Mount, Output, IO<Unit>>> NativeScene(SceneView view);

    public IO<OpaqueTarget> OpaqueTargets(IPlugInLifetime owner);

    public Fin<Unit> ExtendedPresent(Control view, Graphics graphics, SKImage frame, RectangleF destination);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BitmapPixels(Bitmap Bitmap) : IDisposable {
    public Eto.Drawing.BitmapData Data { get; } = Bitmap.Lock();

    public SKPixmap Pixmap =>
        new(new SKImageInfo(Bitmap.Width, Bitmap.Height, (Data.TranslateArgbToData(unchecked((int)0xFFFF0000)) & 0xFF) is 0xFF ? SKColorType.Rgba8888 : SKColorType.Bgra8888,
                Data.PremultipliedAlpha ? SKAlphaType.Premul : SKAlphaType.Unpremul, HostPixels.Srgb),
            Data.Data, Data.ScanWidth);

    public static readonly Func<SKSizeI, Fin<Allocation<BitmapPixels>>> Allocated =
        static pixels => new BitmapPixels(new Bitmap(pixels.Width, pixels.Height, Eto.Drawing.PixelFormat.Format32bppRgba)) switch { var held => new Allocation<BitmapPixels>(held, held.Pixmap) };

    public void Dispose() {
        Data.Dispose();
        Bitmap.Dispose();
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneView : Drawable {
    // --- [STATE]
    private sealed record Seat(PixelLayout Holder, Control Control, Func<SlotKind, IO<Unit>> Restated, EnvIO Scope);

    private sealed record Entry(Control Control, Func<SlotKind, IO<Unit>> Restated);

    private readonly RowScope rows;
    private readonly PixelLayout seats = new();
    private Node view;
    private Option<SceneHost<BitmapPixels>> session;
    private PlacedControls<Seat> placing;
    private Option<CursorShape> pointer;
    private (bool Shown, bool Key) native;

    public SceneView(RowScope scope, Node view) {
        (rows, this.view) = (scope, view);
        placing = Seating;
        Content = seats;
        CanFocus = true;
        Style = HandlerStyle;
        _ = Owner.Run(ViewOps.WhileLoaded(this, Listened, Owner).Under(Owned));
    }

    public static string HandlerStyle { get; } = $"{typeof(SceneView).Assembly.GetName().Name}.{nameof(SceneView)}";

    public Func<Seq<Key>, IO<Control>> Slots { get; init; } =
        static path => IO.fail<Control>(new RhinoFault($"/{string.Join('/', path.Map(static key => key.Identifier))}", RhinoReason.Missing));

    internal Option<Mount> Current => session.Bind(static held => held.Current);

    internal IPlugInLifetime Owner => rows.Owner;

    internal EnvIO Owned { get; } = EnvIO.New(token: CancellationToken.None);

    public static PlugInSlot<TimeProvider> Clocks { get; } = new();

    public static IDrawnPlatform Drawn => Platform.Instance.CreateShared<IDrawnPlatform>();

    internal static PlugInSlot<OpaqueTarget> Opaque { get; } = new();

    internal static OpaqueTarget Bitmapped { get; } = static _ => IO.pure(unit);

    // --- [SHOW]
    public IO<Unit> Show(Node next) =>
        IO.lift(() => view = next).Bind(_ => session.Traverse(held => held.Show(next)).As()).Map(static _ => unit);

    // --- [SESSION]
    public static Seq<Func<IPlugInViews, IO<Unit>>> Opened(ViewCatalog catalog, TimeProvider clock) =>
        catalog.Rows.Bind(ViewCatalog.Tree).Exists(static held => held.Children.Exists(static child => !child.Controls.IsEmpty))
            ? [owner => Published(owner, clock)]
            : [];

    private static IO<Unit> Published(IPlugInLifetime owner, TimeProvider clock) =>
        from clocked in Clocks.Publish(owner, clock)
        from opaque in (from target in Drawn.OpaqueTargets(owner)
                        from published in Opaque.Publish(owner, target)
                        select unit).MapFail(static error => new RhinoFault(nameof(SceneView), RhinoReason.Unheld, Inner: error))
        select unit;

    // --- [LIFETIME]
    private IO<Unit> Listened =>
        from lease in IO.env
        from clock in Clocks.Of(Owner)
        from projected in Drawn.NativeScene(this)
        from host in use(IO.lift(() => new SceneHost<BitmapPixels>(Backend(projected, lease), clock)), Released)
        from held in IO.lift(() => session = Some(host))
        from shown in host.Show(view)
        from hosted in Rehosted
        from theme in HostTheme.In(this).Inline(_ => Rehosted, Owner)
        from visible in rows.OnVisibility(_ => Rehosted)
        select unit;

    private SceneBackend<BitmapPixels> Backend(Func<Mount, Output, IO<Unit>> projected, EnvIO lease) =>
        new(
            BitmapPixels.Allocated,
            damage => IO.lift(() => damage.Iter(rect => Invalidate(Rectangle.Ceiling(new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height))))),
            tick => Drawn.FrameLink(this, tick, Owner).Under(lease)) {
            Applied = (mount, output) => Applied(projected, mount, output),
        };

    private IO<Unit> Rehosted =>
        session.Traverse(held =>
            from host in Hosting
            from moved in held.Rehost(host, HostTheme.Stated(HostTheme.Compact(this)))
            select unit).As().Map(static _ => unit);

    private IO<Host> Hosting =>
        rows.Visibility.Map(visible => new Host(
            new SKSize(Math.Max(0, Width), Math.Max(0, Height)),
            HostTheme.Projected(HostTheme.Compact(this)),
            Visibility.Of(visible && native.Shown, native.Key),
            HostPlacement.Panel,
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
            (slot, placed, element) =>
                from scope in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
                from made in slot.Kind.Switch(
                        (View: this, slot.Path),
                        text: static (held, text) => held.View.Entered(text),
                        native: static (held, _) => use(held.View.Slots(held.Path)).Map(static control => new Entry(control, static _ => IO.pure(unit))))
                    .Under(scope)
                    .Catch(error => IO.lift(scope.Dispose) >> IO.fail<Entry>(error))
                from seat in IO.lift(() => {
                    PixelLayout holder = new();
                    holder.Add(made.Control, placed.Frame.Left, placed.Frame.Top);
                    seats.Add(holder, placed.Clip.Left, placed.Clip.Top);
                    return Sized(new Seat(holder, made.Control, made.Restated, scope), placed);
                })
                from seated in Drawn.SlotSeated(made.Control, element.Bind(static held => held.Accessible.Label))
                select seat,
            (seat, slot, placed) =>
                from restated in seat.Restated(slot.Kind)
                from moved in IO.lift(() => {
                    seats.Move(seat.Holder, placed.Clip.Left, placed.Clip.Top);
                    seat.Holder.Move(seat.Control, placed.Frame.Left, placed.Frame.Top);
                    return Sized(seat, placed);
                })
                select moved,
            seat => IO.lift(() => Unseated(seat)));

    private IO<Entry> Entered(SlotKind.Text text) =>
        from entry in text.MaxLines.Exists(static lines => lines is 1) ? HostInput.Line() : HostInput.Area(text.MaxLines)
        from held in IO.lift(() => Atom(text))
        from shown in HostInput.Bound(entry, IO.lift(() => (held.Value.Value, held.Value.Edited)), Owner)
        select new Entry(entry.Shown, kind => kind.Switch(
            (Held: held, Shown: shown),
            text: static (state, next) => IO.lift(() => state.Held.Swap(_ => next)).Bind(_ => state.Shown),
            native: static (_, _) => IO.pure(unit)));

    private static Seat Sized(Seat seat, SlotFrame placed) {
        seat.Holder.Size = new Size(placed.Clip.Width, placed.Clip.Height);
        seat.Control.Size = new Size(placed.Frame.Width, placed.Frame.Height);
        return seat;
    }

    private Unit Unseated(Seat seat) {
        seats.Remove(seat.Holder);
        seat.Holder.Remove(seat.Control);
        seat.Scope.Dispose();
        seat.Holder.Dispose();
        return unit;
    }

    // --- [PAINT]
    protected override void OnPaint(PaintEventArgs e) {
        _ = Owner.Run(Painted(e.Graphics));
        base.OnPaint(e);
    }

    private IO<Unit> Painted(Graphics graphics) =>
        session.Traverse(held =>
            from scale in IO.lift(() => Conversions.Found(ParentWindow).Map(static window => window.LogicalPixelSize))
            from target in held.Paint(scale)
            from shown in target.Traverse(painted => Blitted(graphics, painted)).As()
            select unit).As().Map(static _ => unit);

    private IO<Unit> Blitted(Graphics graphics, SceneTarget<BitmapPixels> target) =>
        new RectangleF(0f, 0f, Width, Height) switch {
            var destination => target.Pixels.Match(
                Some: pixels => IO.lift(() => graphics.DrawImage(pixels.Bitmap, new RectangleF(0f, 0f, target.Info.Width, target.Info.Height), destination)),
                None: () => IO.lift(() => Drawn.ExtendedPresent(this, graphics, target.Surface.Snapshot(), destination))),
        };

    // --- [INPUT]
    internal bool Receive(Input input, Duration now) =>
        Owner.Answer(session.Match(Some: held => held.Receive(input, now), None: static () => IO.pure(value: false)), failed: false);

    internal bool Timed(Input input) =>
        Owner.Answer(session.Match(Some: held => held.Now.Bind(now => held.Receive(input, now)), None: static () => IO.pure(value: false)), failed: false);

    private bool Received(Fin<Option<Input>> input) =>
        Owner.Answer(from held in IO.lift(() => input) from now in IO.lift(static () => HostInput.Now) select held.Exists(shown => Receive(shown, now)), failed: false);

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
        _ = Owner.Run(Rehosted);
    }

    // --- [KEYS]
    protected override void OnKeyDown(KeyEventArgs e) {
        e.Handled = Current.Exists(mount => HostInput.Keyed(mount, e).Exists(Timed));
        if (!e.Handled) base.OnKeyDown(e);
    }

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            _ = Owner.Run(IO.lift(Owned.DisposeResources));
            Owned.Dispose();
            seats.Dispose();
        }
        base.Dispose(disposing);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostPixels {
    internal static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

    extension(Color4f color) {
        public SKColorF ToSKColorF() => new(color.R, color.G, color.B, color.A);
    }

    extension(System.Drawing.Bitmap bitmap) {
        public IO<SKImage> ToSKImage() =>
            IO.lift(() => bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                .Bind(data => IO.lift(() => Conversions.Found(
                        SKImage.FromPixelCopy(new SKImageInfo(data.Width, data.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul, Srgb), data.Scan0, data.Stride)))
                    .Finally(IO.lift(() => bitmap.UnlockBits(data))));
    }
}
