using System.Numerics;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.UI;
using SkiaSharp;

namespace Rasm.Rhino.UI.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate Fin<Duration> EventTime();

public delegate Fin<int> EventClicks();

public delegate Eto.Forms.Cursor ClosedHand();

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostInput {
    // --- [MODIFIERS]
    public static LanguageExt.HashSet<Modifier> Modifiers(Keys keys) =>
        toHashSet(Seq(
                (Keys: Application.Instance.CommonModifier, Modifier: Modifier.Common),
                (Keys: Application.Instance.AlternateModifier, Modifier: Modifier.Alternate),
                (Keys: Keys.Shift, Modifier: Modifier.Shift))
            .Filter(row => (keys & row.Keys) is not Keys.None)
            .Map(static row => row.Modifier));

    public static LanguageExt.HashSet<PointerButton> Buttons(MouseButtons buttons) =>
        toHashSet(Seq((Buttons: MouseButtons.Primary, Button: PointerButton.Primary), (Buttons: MouseButtons.Alternate, Button: PointerButton.Alternate))
            .Filter(row => buttons.HasFlag(row.Buttons))
            .Map(static row => row.Button));

    // --- [TIME]
    public static Fin<Duration> Now => Found<EventTime>().Bind(static time => time());

    public static Fin<int> Clicks => Found<EventClicks>().Bind(static clicks => clicks());

    // --- [POINTER]
    public static SKPoint Point(PointF at) => new(at.X, at.Y);

    public static Option<Input> Pressed(MouseEventArgs e, int clicks) => Pointed(e, new Phase<PointerSample>.Began(Sample(e)), clicks);

    public static Option<Input> Moved(MouseEventArgs e, int clicks) => Pointed(e, new Phase<PointerSample>.Changed(Sample(e)), clicks);

    public static Option<Input> Released(MouseEventArgs e, int clicks) => Pointed(e, new Phase<PointerSample>.Ended(Sample(e)), clicks);

    public static Input Left(MouseEventArgs e) => new Input.Hover(None) { Modifiers = Modifiers(e.Modifiers) };

    public static Input Wheeled(MouseEventArgs e) => new Input.Wheel(Point(e.Location), new Vector2(e.Delta.Width, e.Delta.Height)) { Modifiers = Modifiers(e.Modifiers) };

    private static Option<Input> Pointed(MouseEventArgs e, Phase<PointerSample> stroke, int clicks) => Input.Pointed(stroke, Buttons(e.Buttons), clicks, Modifiers(e.Modifiers));

    private static PointerSample Sample(MouseEventArgs e) => new(Point(e.Location), e.Pressure);

    // --- [KEYS]
    public static Option<Input> Keyed(Mount mount, KeyEventArgs e) => Input.Keyed(mount, e.KeyData.ToShortcutString(), Modifiers(e.Modifiers));

    // --- [CURSOR]
    public static Fin<Eto.Forms.Cursor> Cursor(Rasm.UI.Cursor cursor) =>
        cursor.Switch(
            @default: static () => Cursors.Default,
            pointer: static () => Cursors.Pointer,
            text: static () => Cursors.IBeam,
            crosshair: static () => Cursors.Crosshair,
            move: static () => Cursors.SizeAll,
            grab: static () => Cursors.Move,
            grabbing: static () => Found<ClosedHand>().Map(static hand => hand()),
            columnResize: static () => Cursors.VerticalSplit,
            rowResize: static () => Cursors.HorizontalSplit,
            notAllowed: static () => Cursors.NotAllowed);

    // --- [HOST]
    private static Fin<TRow> Found<TRow>() where TRow : Delegate => Refused.UnlessFound(Platform.Instance.Find<TRow>(), typeof(TRow).Name).Map(static row => row());
}
