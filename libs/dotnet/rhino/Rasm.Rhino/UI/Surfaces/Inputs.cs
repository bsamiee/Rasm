using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
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
