using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Rasm.UI.Automation;
using Rasm.UI.Hosting;
using Rasm.UI.Input;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Eto;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class SceneView : Drawable {

    // --- [STATE]

    private sealed class Backing(SKImageInfo format) : IDisposable {
        private Option<Bitmap> held;

        internal Unit Blit(Graphics graphics, SKSurface surface, Size logical, float scale) {
            Size pixels = Size.Ceiling(logical * scale);
            Bitmap target = held.Filter(bitmap => bitmap.Size == pixels).IfNone(() => Allocated(pixels));
            using (BitmapData data = target.Lock()) {
                _ = data.Flipped
                    ? toSeq(Range(0, pixels.Height)).ForAll(row => surface.ReadPixels(format.WithSize(pixels.Width, 1), data.Data + ((pixels.Height - 1 - row) * data.ScanWidth), data.ScanWidth, 0, row))
                    : surface.ReadPixels(format.WithSize(pixels.Width, pixels.Height), data.Data, data.ScanWidth, 0, 0);
            }
            graphics.ImageInterpolation = ImageInterpolation.None;
            graphics.DrawImage(target, new RectangleF(PointF.Empty, logical));
            return unit;
        }

        private Bitmap Allocated(Size pixels) {
            Dispose();
            Bitmap made = new(pixels, PixelFormat.Format32bppRgba);
            held = made;
            return made;
        }

        public void Dispose() {
            _ = held.Iter(static bitmap => bitmap.Dispose());
            held = None;
        }
    }

    private sealed record Placed(Panel Clip, PixelLayout Inner, Control Control);

    private sealed record Popped(Hosted.Popup Popup, Window Owner, SKImageInfo Format) : IDisposable {
        internal Drawable Surface { get; } = new();
        internal Backing Backing { get; } = new(Format);
        internal FloatingForm Form { get; } = new() { Owner = Owner, WindowStyle = WindowStyle.None, ShowInTaskbar = false, ShowActivated = false };

        public void Dispose() {
            Form.Close();
            Surface.Dispose();
            Form.Dispose();
            Backing.Dispose();
        }
    }

    private readonly SceneHost host;
    private readonly StyleSheet sheet;
    private readonly Action<Error> faulted;
    private readonly SKImageInfo format = Projection.Format();
    private readonly Backing backing;
    private readonly PixelLayout layout = new();
    private readonly UITimer frames;
    private PlacedControls<Placed> controls;
    private HashMap<Seq<Key>, Popped> popups;
    private Option<Func<Duration, IO<Unit>>> ticking;

    public SceneView(StyleSheet sheet, TimeProvider clock, Duration frame, Action<Error> faulted, Seq<SKTypeface> embedded = default) {
        (this.sheet, this.faulted) = (sheet, faulted);
        backing = new Backing(format);
        host = new SceneHost(new SceneBackend(format, static info => SKSurface.Create(info) is { } made ? made : new UIFault(info, UIReason.Refused), Invalidated, Linked) { Applied = Applied }, clock, embedded);
        controls = new PlacedControls<Placed>(Added, Moved, Removed);
        frames = new UITimer((_, _) => Run(ticking.Map(tick => host.Now.Bind(tick)).IfNone(IO.pure(unit)))) { Interval = frame.TotalSeconds };
        Content = layout;
        CanFocus = true;
    }

    public Func<Hosted.Slot, Option<AutomationElement>, IO<Control>> Slotted { get; init; } = static (_, _) => IO.pure<Control>(new Panel());

    public Option<Mount> Current => host.Current;

    // --- [SESSIONS]

    public IO<Unit> Show(View view) => host.Show(view);

    private IO<Unit> Rehosted =>
        from theme in IO.lift(Projection.Themed)
        from hosted in host.Rehost(new Host(new SKSize(Width, Height), theme, Visibility.Of(Visible, HasFocus), Parent is Window ? Roles.Window : Roles.Panel, CultureInfo.CurrentUICulture), sheet)
        select hosted;

    private bool Received(InputEvent input) => host.Now.Bind(now => host.Receive(input, now)).RunSafe().IfFail(Faulted);

    private Unit Run(IO<Unit> effect) => effect.RunSafe().Match(Succ: static _ => unit, Fail: error => Faulted(error) switch { _ => unit });

    private bool Faulted(Error error) {
        faulted(error);
        return false;
    }

    // --- [BACKEND]

    private IO<Unit> Invalidated(Seq<SKRectI> damage) =>
        IO.lift(() => {
            _ = damage.Iter(rect => Invalidate(new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height), invalidateChildren: false));
            return popups.Values.ToSeq().Iter(static held => held.Surface.Invalidate());
        });

    private IO<Func<bool, IO<Unit>>> Linked(Func<Duration, IO<Unit>> tick) =>
        IO.lift(() => {
            ticking = tick;
            return fun((bool animating) => IO.lift(() => Animated(animating)));
        });

    private Unit Animated(bool animating) {
        if (animating && !frames.Started) frames.Start();
        if (!animating && frames.Started) frames.Stop();
        return unit;
    }

    private IO<Unit> Applied(Mount mount) =>
        from next in controls.Reconciled(mount)
        from shown in IO.lift(() => {
            controls = next;
            Cursor = Projection.Pointer(mount.Cursor);
            ToolTip = mount.Help.ValueUnsafe();
            popups = Reopened(mount);
            return unit;
        })
        select shown;

    // --- [CONTROLS]

    private IO<Placed> Added(Hosted.Slot slot, Option<AutomationElement> element) =>
        slot.Entry.Match(Some: text => IO.lift<Control>(() => Entry(slot.Path, text)), None: () => Slotted(slot, element)).Map(control => {
            PixelLayout inner = new();
            inner.Add(control, slot.ControlRect.Left, slot.ControlRect.Top);
            Placed placed = new(new Panel { Content = inner }, inner, control);
            layout.Add(placed.Clip, slot.ClipRect.Left, slot.ClipRect.Top);
            return Sized(placed, slot);
        });

    private IO<Placed> Moved(Placed placed, Hosted.Slot slot) =>
        IO.lift(() => {
            layout.Move(placed.Clip, slot.ClipRect.Left, slot.ClipRect.Top);
            placed.Inner.Move(placed.Control, slot.ControlRect.Left, slot.ControlRect.Top);
            _ = (from text in slot.Entry from box in Optional(placed.Control as TextBox) where !string.Equals(box.Text, text.Content, StringComparison.Ordinal) select (Box: box, text.Content)).Iter(static held => held.Box.Text = held.Content);
            return Sized(placed, slot);
        });

    private IO<Unit> Removed(Placed placed) =>
        IO.lift(() => {
            layout.Remove(placed.Clip);
            placed.Clip.Dispose();
            return unit;
        });

    private static Placed Sized(Placed placed, Hosted.Slot slot) {
        placed.Clip.Size = new Size(slot.ClipRect.Width, slot.ClipRect.Height);
        placed.Control.Size = new Size(slot.ControlRect.Width, slot.ControlRect.Height);
        return placed;
    }

    private TextBox Entry(Seq<Key> path, View.Text text) {
        TextBox box = new() { Text = text.Content, PlaceholderText = text.Placeholder.ValueUnsafe() };
        box.TextChanged += (_, _) => Edited(path, EditPhase.Step, box.Text);
        box.LostFocus += (_, _) => Edited(path, EditPhase.Commit, box.Text);
        box.KeyDown += (_, args) => {
            args.Handled = args.Key is Keys.Enter;
            _ = args.Handled ? Edited(path, EditPhase.Commit, box.Text) : unit;
        };
        return box;
    }

    private Unit Edited(Seq<Key> path, EditPhase phase, string value) =>
        Run(controls.Controls.Find(path).Bind(static held => held.Slot.Entry).Bind(static text => text.Edited).Map(edited => edited(Edit.Of(phase, value))).IfNone(IO.pure(unit)));

    // --- [POPUPS]

    private HashMap<Seq<Key>, Popped> Reopened(Mount mount) {
        HashMap<Seq<Key>, Hosted.Popup> wanted = toHashMap(mount.Hosted.Choose(static hosted => Optional(hosted as Hosted.Popup)).Map(static popup => (popup.Path, popup)));
        _ = popups.Filter((path, _) => !wanted.ContainsKey(path)).Values.ToSeq().Iter(static held => held.Dispose());
        return wanted.Map((path, popup) => Framed(popups.Find(path).Map(held => held with { Popup = popup }).IfNone(() => Opened(popup))));
    }

    private Popped Opened(Hosted.Popup popup) {
        Popped held = new(popup, ParentWindow, format);
        held.Form.Content = held.Surface;
        held.Surface.Paint += (_, args) => Painted(Some(popup.Path), held.Surface, args.Graphics, held.Backing);
        held.Surface.MouseDown += (_, args) => args.Handled = Pointed(args, EditPhase.Begin, 1, popup.Path);
        held.Surface.MouseMove += (_, args) => args.Handled = Pointed(args, EditPhase.Step, 0, popup.Path);
        held.Surface.MouseUp += (_, args) => args.Handled = Pointed(args, EditPhase.Commit, 0, popup.Path);
        held.Form.Show();
        return held;
    }

    private Popped Framed(Popped held) {
        held.Form.Location = Point.Round(PointToScreen(new PointF(held.Popup.Frame.Left, held.Popup.Frame.Top)));
        held.Form.ClientSize = Size.Ceiling(new SizeF(held.Popup.Frame.Width, held.Popup.Frame.Height));
        return held;
    }

    private bool Pointed(MouseEventArgs args, EditPhase phase, int clicks, Seq<Key> popup) =>
        popups.Find(popup).Map(static held => held.Popup.Frame.Location).Bind(origin => Projection.Pointed(args, phase, clicks, origin)).Exists(Received);

    // --- [DRAWING]

    private Unit Painted(Option<Seq<Key>> popup, Control surface, Graphics graphics, Backing target) =>
        Run(Optional(surface.ParentWindow).Map(static window => window.LogicalPixelSize)
            .Traverse(scale => host.Paint(scale, popup).Map(drawn => drawn.Iter(held => target.Blit(graphics, held, surface.Size, scale))))
            .As()
            .Map(static _ => unit));

    protected override void OnPaint(PaintEventArgs e) {
        _ = Painted(None, this, e.Graphics, backing);
        base.OnPaint(e);
    }

    // --- [EVENTS]

    protected override void OnLoadComplete(EventArgs e) {
        base.OnLoadComplete(e);
        _ = Run(Rehosted);
    }

    protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        _ = Loaded ? Run(Rehosted) : unit;
    }

    protected override void OnThemeChanged(EventArgs e) {
        base.OnThemeChanged(e);
        _ = Run(Rehosted);
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        _ = Run(Rehosted);
    }

    protected override void OnLostFocus(EventArgs e) {
        base.OnLostFocus(e);
        _ = Run(Rehosted);
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        e.Handled = Projection.Pointed(e, EditPhase.Begin, 1, SKPoint.Empty).Exists(Received);
        _ = e.Handled && e.Buttons.HasFlag(MouseButtons.Primary) && CaptureMouse();
        base.OnMouseDown(e);
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e) {
        e.Handled = Projection.Pointed(e, EditPhase.Begin, 2, SKPoint.Empty).Exists(Received);
        base.OnMouseDoubleClick(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        e.Handled = Projection.Pointed(e, EditPhase.Step, 0, SKPoint.Empty).Exists(Received);
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        ReleaseMouseCapture();
        e.Handled = Projection.Pointed(e, EditPhase.Commit, 0, SKPoint.Empty).Exists(Received);
        base.OnMouseUp(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        e.Handled = Received(new InputEvent.Hover(None));
        base.OnMouseLeave(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        e.Handled = Received(Projection.Scrolled(e, SKPoint.Empty, SystemFonts.Default().LineHeight));
        base.OnMouseWheel(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        e.Handled = Projection.Keyed(Current, e).Exists(Received);
        base.OnKeyDown(e);
    }

    // --- [RELEASE]

    protected override void Dispose(bool disposing) {
        if (disposing) {
            frames.Dispose();
            _ = Run(controls.Released);
            _ = popups.Values.ToSeq().Iter(static held => held.Dispose());
            backing.Dispose();
            layout.Dispose();
            host.Dispose();
        }
        base.Dispose(disposing);
    }
}
