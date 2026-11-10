using Eto.Drawing;
using Eto.Forms;
using Rasm.UI.Input;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Eto;

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Projection {

    // --- [THEME]

    internal static Fin<Styling.Theme> Themed() {
        Font font = SystemFonts.Default();
        return Styling.Theme.Validated(new Seeds(Tone(SystemColors.WindowBackground), Tone(SystemColors.ControlText), Tone(SystemColors.Highlight)),
            new TextFace { Family = font.FamilyName }, font.Size, Chrome());
    }

    private static HashMap<Token, SKColorF> Chrome() =>
        toHashMap(Seq(
            (Token.Canvas, SystemColors.WindowBackground), (Token.CanvasText, SystemColors.ControlText),
            (Token.AccentColor, SystemColors.Highlight), (Token.AccentColorText, SystemColors.HighlightText),
            (Token.Highlight, SystemColors.Selection), (Token.HighlightText, SystemColors.SelectionText),
            (Token.ButtonFace, SystemColors.Control), (Token.ButtonText, SystemColors.ControlText),
            (Token.Field, SystemColors.ControlBackground), (Token.FieldText, SystemColors.ControlText))
            .Map(static held => (held.Item1, new SKColorF(held.Item2.R, held.Item2.G, held.Item2.B, held.Item2.A))));

    private static Unicolour Tone(Color color) => new(ColourSpace.Rgb, color.R, color.G, color.B, color.A);

    // --- [INPUT]

    private static readonly HashMap<CursorShape, CursorType> Shapes = HashMap(
        (CursorShape.Default, CursorType.Default),
        (CursorShape.Pointer, CursorType.Pointer),
        (CursorShape.Text, CursorType.IBeam),
        (CursorShape.Crosshair, CursorType.Crosshair),
        (CursorShape.Move, CursorType.SizeAll),
        (CursorShape.Grab, CursorType.Move),
        (CursorShape.Grabbing, CursorType.Move),
        (CursorShape.ColumnResize, CursorType.VerticalSplit),
        (CursorShape.RowResize, CursorType.HorizontalSplit),
        (CursorShape.NotAllowed, CursorType.NotAllowed));

    internal static Cursor Pointer(CursorShape shape) => Cursors.Cached(Shapes[shape]);

    internal static KeyModifiers Modifiers(Keys keys) =>
        new(Primary: (keys & Application.Instance.CommonModifier) is not Keys.None, Alternate: (keys & Application.Instance.AlternateModifier) is not Keys.None, Shift: keys.HasFlag(Keys.Shift));

    internal static SKPoint At(PointF location, SKPoint origin) => new(location.X + origin.X, location.Y + origin.Y);

    internal static Option<InputEvent> Pointed(MouseEventArgs args, EditPhase phase, int clicks, SKPoint origin) =>
        InputEvent.Pointed(Edit.Of(phase, new PointerSample(At(args.Location, origin))), args.Buttons.HasFlag(MouseButtons.Primary), args.Buttons.HasFlag(MouseButtons.Alternate), clicks, Modifiers(args.Modifiers));

    internal static Option<InputEvent> Keyed(Option<Mount> mount, KeyEventArgs args) =>
        Modifiers(args.Modifiers) switch {
            var modifiers => mount.Bind(held => InputEvent.Keyed(held, args.KeyData.ToShortcutString(), modifiers))
                | Acted(args.Key, modifiers).Map(act => (InputEvent)new InputEvent.Perform(act, new Target.Focused()) { Modifiers = modifiers }),
        };

    private static Option<Act> Acted(Keys key, KeyModifiers modifiers) =>
        (key, modifiers) switch {
            (Keys.Tab, { Shift: true }) => Act.Previous,
            (Keys.Tab, _) => Act.Next,
            (Keys.Home, { Primary: true }) => Act.Top,
            (Keys.End, { Primary: true }) => Act.Bottom,
            (Keys.A, { Primary: true }) => Act.SelectAll,
            (Keys.Enter, _) => Act.Confirm,
            (Keys.Space, _) => Act.Press,
            (Keys.Escape, _) => Act.Cancel,
            (Keys.Left, _) => Act.Left,
            (Keys.Right, _) => Act.Right,
            (Keys.Up, _) => Act.Up,
            (Keys.Down, _) => Act.Down,
            (Keys.PageUp, _) => Act.PageUp,
            (Keys.PageDown, _) => Act.PageDown,
            (Keys.Home, _) => Act.Home,
            (Keys.End, _) => Act.End,
            (Keys.Delete or Keys.Backspace, _) => Act.Delete,
            _ => None,
        };
}
