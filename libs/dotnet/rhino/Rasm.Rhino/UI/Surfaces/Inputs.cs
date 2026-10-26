using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.UI;
using SkiaSharp;

namespace Rasm.Rhino.UI.Surfaces;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostInput {
    // --- [MODIFIERS]
    public static KeyModifiers Modifiers(Keys keys) =>
        new(Common: keys.HasFlag(Application.Instance.CommonModifier), Alternate: keys.HasFlag(Application.Instance.AlternateModifier), Shift: keys.HasFlag(Keys.Shift));

    // --- [TIME]
    public static Fin<Duration> Now => SceneView.Drawn.EventTime;

    public static Fin<int> Clicks => SceneView.Drawn.EventClicks;

    // --- [POINTER]
    public static SKPoint Point(PointF at) => new(at.X, at.Y);

    public static Option<Input> Pressed(MouseEventArgs e, int clicks) => Pointed(e, new Phase<PointerSample>.Began(Sample(e)), clicks);

    public static Option<Input> Moved(MouseEventArgs e, int clicks) => Pointed(e, new Phase<PointerSample>.Changed(Sample(e)), clicks);

    public static Option<Input> Released(MouseEventArgs e, int clicks) => Pointed(e, new Phase<PointerSample>.Ended(Sample(e)), clicks);

    public static Input Left(MouseEventArgs e) => new Input.Hover(None) { Modifiers = Modifiers(e.Modifiers) };

    public static Input Wheeled(MouseEventArgs e) => new Input.Wheel(Point(e.Location), new Vector2(e.Delta.Width, e.Delta.Height)) { Modifiers = Modifiers(e.Modifiers) };

    private static Option<Input> Pointed(MouseEventArgs e, Phase<PointerSample> stroke, int clicks) =>
        Input.Pointed(stroke, e.Buttons.HasFlag(MouseButtons.Primary), e.Buttons.HasFlag(MouseButtons.Alternate), clicks, Modifiers(e.Modifiers));

    private static PointerSample Sample(MouseEventArgs e) => new(Point(e.Location), e.Pressure);

    // --- [KEYS]
    public static Option<Input> Keyed(Mount mount, KeyEventArgs e) => Input.Keyed(mount, e.KeyData.ToShortcutString(), Modifiers(e.Modifiers));

    // --- [ENTRIES]
    public sealed record Entry(Control Shown, TextControl Field, Keys Commit, Func<string> Read, Action<string> Write);

    public static IO<Entry> Line(Action<IDisposable> retain) =>
        from box in DisposalOps.Acquire(IO.lift(static () => new TextBox()), retain)
        select new Entry(box, box, Keys.Enter, () => box.Text, text => box.Text = text);

    public static IO<Entry> Area(Option<int> visible, Action<IDisposable> retain) =>
        from area in DisposalOps.Acquire(IO.lift(static () => new TextArea()), retain)
        from fitted in IO.lift(() => {
            (area.Wrap, area.AcceptsReturn, area.AcceptsTab, area.SpellCheck) = (true, true, false, false);
            _ = visible.Iter(lines => area.Height = (int)MathF.Ceiling(area.Font.LineHeight * lines));
        })
        select new Entry(area, area, Application.Instance.CommonModifier | Keys.Enter, () => area.Text, text => area.Text = text);

    public static IO<IO<Unit>> Bound(Entry entry, IO<(string Value, Func<Edit<string>, IO<Unit>> Edited)> latest, IPlugInSink sink, Action<IDisposable> retain) =>
        Callbacks.Handler<EventArgs>(_ => IO.lift(entry.Read).Bind(typed => Edited(latest, Edit.Preview(typed))), new CallbackSite(sink, entry.Field.GetType(), nameof(TextControl.TextChanged))) switch {
            var changed =>
                from previewed in Subscriptions.Attach(h => entry.Field.TextChanged += h, h => entry.Field.TextChanged -= h, changed, retain)
                let shown = latest.Bind(current => Subscriptions.Detached(h => entry.Field.TextChanged += h, h => entry.Field.TextChanged -= h, changed,
                    IO.lift(entry.Read).Bind(text => when(!string.Equals(text, current.Value, StringComparison.Ordinal), IO.lift(() => entry.Write(current.Value))).As())))
                let committed = IO.lift(entry.Read).Bind(typed => Edited(latest, Edit.Commit(typed)))
                let answers = HashMap((entry.Commit, committed), (Keys.Escape, Edited(latest, Edit.Cancel<string>()).Bind(_ => shown)))
                from keys in Subscriptions.Attach(h => entry.Field.KeyDown += h, h => entry.Field.KeyDown -= h,
                    Callbacks.Synchronous<KeyEventArgs>(args => Answered(args, answers), new CallbackSite(sink, entry.Field.GetType(), nameof(Control.KeyDown))), retain)
                from lost in Subscriptions.Attach(h => entry.Field.LostFocus += h, h => entry.Field.LostFocus -= h,
                    Callbacks.Handler<EventArgs>(_ => committed, new CallbackSite(sink, entry.Field.GetType(), nameof(Control.LostFocus))), retain)
                select shown,
        };

    private static IO<Unit> Edited(IO<(string Value, Func<Edit<string>, IO<Unit>> Edited)> latest, Edit<string> edit) => latest.Bind(current => current.Edited(edit));

    private static IO<Unit> Answered(KeyEventArgs args, HashMap<Keys, IO<Unit>> answers) =>
        answers.Find(args.KeyData)
            .Traverse(answer => IO.lift(() => args.Handled = true).Bind(_ => answer)).As()
            .Map(unit);

    // --- [CURSOR]
    public static Fin<Cursor> Cursor(CursorShape cursor) =>
        cursor.Switch(
            @default: static () => Cursors.Default,
            pointer: static () => Cursors.Pointer,
            text: static () => Cursors.IBeam,
            crosshair: static () => Cursors.Crosshair,
            move: static () => Cursors.SizeAll,
            grab: static () => Cursors.Move,
            grabbing: static () => SceneView.Drawn.ClosedHand,
            columnResize: static () => Cursors.VerticalSplit,
            rowResize: static () => Cursors.HorizontalSplit,
            notAllowed: static () => Cursors.NotAllowed);
}
