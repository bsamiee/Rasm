using Rasm.UI.Inputs;
using Rasm.UI.Layout;
using Rasm.UI.Scenes;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;
using Topten.RichTextKit.Editor;

namespace Rasm.UI.Text;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Editor : ITextDocumentView {
    private static readonly FrozenDictionary<KeyCode, (NavigationKind Plain, NavigationKind Word, NavigationKind Fine)> Moves =
        new Dictionary<KeyCode, (NavigationKind Plain, NavigationKind Word, NavigationKind Fine)> {
            [KeyCode.ArrowLeft] = (NavigationKind.CharacterLeft, NavigationKind.WordLeft, NavigationKind.LineHome),
            [KeyCode.ArrowRight] = (NavigationKind.CharacterRight, NavigationKind.WordRight, NavigationKind.LineEnd),
            [KeyCode.ArrowUp] = (NavigationKind.LineUp, NavigationKind.LineUp, NavigationKind.DocumentHome),
            [KeyCode.ArrowDown] = (NavigationKind.LineDown, NavigationKind.LineDown, NavigationKind.DocumentEnd),
            [KeyCode.Home] = (NavigationKind.LineHome, NavigationKind.LineHome, NavigationKind.DocumentHome),
            [KeyCode.End] = (NavigationKind.LineEnd, NavigationKind.LineEnd, NavigationKind.DocumentEnd),
            [KeyCode.PageUp] = (NavigationKind.PageUp, NavigationKind.PageUp, NavigationKind.PageUp),
            [KeyCode.PageDown] = (NavigationKind.PageDown, NavigationKind.PageDown, NavigationKind.PageDown),
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<KeyCode, Func<Editor, Modifiers, bool>> Commands =
        new Dictionary<KeyCode, Func<Editor, Modifiers, bool>> {
            [KeyCode.Backspace] = static (editor, modifiers) => editor.Erase(EditSemantics.Backspace, modifiers.HasFlag(Modifiers.Alternate) ? NavigationKind.WordLeft : NavigationKind.CharacterLeft),
            [KeyCode.Delete] = static (editor, modifiers) => editor.Erase(EditSemantics.ForwardDelete, modifiers.HasFlag(Modifiers.Alternate) ? NavigationKind.WordRight : NavigationKind.CharacterRight),
            [KeyCode.Enter] = static (editor, _) => !editor.document.SingleLineMode && editor.Replace("\n"),
            [KeyCode.KeyA] = static (editor, modifiers) => modifiers.HasFlag(Modifiers.Fine) && editor.Select(new TextRange(0, editor.document.Length - 1)),
            [KeyCode.KeyZ] = static (editor, modifiers) => modifiers.HasFlag(Modifiers.Fine) && editor.Revert(modifiers.HasFlag(Modifiers.Snap)),
        }.ToFrozenDictionary();

    private readonly TextDocument document;
    private TextRange selection;
    private Option<float> ghost;
    private TextRange composing;
    private bool focused;

    public Editor(Theme theme, TypeRole type, string text, bool singleLine) {
        document = new TextDocument { SingleLineMode = singleLine, LineWrap = !singleLine, PlainTextMode = true, DefaultStyle = Paragraphs.Style(theme, type, SKColors.Black) };
        document.SetMargins(0f, 0f, 0f, 0f);
        document.Text = text;
        document.RegisterView(this);
    }

    public string Text => document.GetText(new TextRange(0, document.Length - 1)).ToString();

    public TextRange Selection => selection;

    // --- [VIEW]
    public Node.Text View(Node.Text text, Func<IO<Unit>> changed) =>
        text with {
            Content = Text,
            Selection = focused ? Some(selection) : None,
            State = focused ? InteractionState.Focused : text.State,
            Cursor = CursorKind.Text,
            Focusable = true,
            Handler = Some<Func<Input, Laid, IO<Response>>>((input, laid) =>
                from response in IO.lift(() => Receive(input, laid.Content))
                from shown in unless(response is Response.Ignored, changed()).As()
                select response),
        };

    // --- [INPUT]
    private Response Receive(Input input, SKRect frame) {
        document.PageWidth = frame.Width;
        return input.Switch(
            (Editor: this, Frame: frame),
            pointer: static (held, pointer) => held.Editor.Pointed(held.Frame, pointer),
            click: static (held, click) => click.Count >= 2
                ? Answered(held.Editor.Select(held.Editor.document.GetSelectionRange(held.Editor.selection.CaretPosition, click.Count is 2 ? SelectionKind.Word : SelectionKind.Paragraph)))
                : Response.Ignored,
            scroll: static (_, _) => Response.Ignored,
            gestured: static (_, _) => Response.Ignored,
            key: static (held, key) => key.Down ? held.Editor.Keyed(key, held.Frame) : Response.Ignored,
            composition: static (held, composition) => Answered(held.Editor.Composed(composition)),
            focus: static (held, focus) => Answered(held.Editor.Focused(focus.Gained)),
            act: static (_, _) => Response.Ignored);
    }

    private Response Pointed(SKRect frame, Input.Pointer pointer) =>
        (Index(frame, pointer.At), pointer.Buttons.HasFlag(PointerButtons.Primary), pointer.Phase) switch {
            (var at, true, PointerPhase.Down) => Focused(gained: true) && Select(pointer.Modifiers.HasFlag(Modifiers.Snap) ? new TextRange(selection.Start, at) : new TextRange(at)) ? Response.Capture : Response.Ignored,
            (var at, true, PointerPhase.Move) => Answered(Select(new TextRange(selection.Start, at))),
            (_, _, PointerPhase.Up) => Response.Release,
            (_, _, PointerPhase.Down or PointerPhase.Move or PointerPhase.Leave) => Response.Ignored,
        };

    private int Index(SKRect frame, SKPoint at) =>
        Paragraphs.Origin(frame, document.MeasuredHeight) switch {
            var origin => int.Clamp(document.HitTest(at.X - origin.X, at.Y - origin.Y).ClosestCodePointIndex, 0, document.Length - 1),
        };

    private Response Keyed(Input.Key key, SKRect frame) =>
        Optional(Moves.GetValueOrDefault(key.Code)).Filter(static move => move.Plain is not NavigationKind.None)
            .Map(move => Moved(key.Modifiers switch {
                var held when held.HasFlag(Modifiers.Fine) => move.Fine,
                var held when held.HasFlag(Modifiers.Alternate) => move.Word,
                _ => move.Plain,
            }, frame.Height, key.Modifiers.HasFlag(Modifiers.Snap)))
            .IfNone(() => Optional(Commands.GetValueOrDefault(key.Code)).Exists(command => command(this, key.Modifiers)))
            ? Response.Handled
            : Response.Ignored;

    private static Response Answered(bool handled) => handled ? Response.Handled : Response.Ignored;

    // --- [EDITS]
    private bool Moved(NavigationKind kind, float page, bool extend) {
        float? remembered = ghost.ToNullable();
        CaretPosition moved = document.Navigate(selection.CaretPosition, kind, page, ref remembered);
        ghost = Optional(remembered);
        return Select(extend ? new TextRange(selection.Start, moved.CodePointIndex, moved.AltPosition) : new TextRange(moved));
    }

    private bool Select(TextRange range) {
        selection = range;
        return true;
    }

    private bool Replace(string text) {
        ghost = None;
        document.ReplaceText(this, selection, text, EditSemantics.Typing);
        return true;
    }

    private bool Erase(EditSemantics semantics, NavigationKind toward) {
        float? held = null;
        TextRange range = selection.IsRange ? selection : new TextRange(selection.End, document.Navigate(selection.CaretPosition, toward, 0f, ref held).CodePointIndex);
        ghost = None;
        document.ReplaceText(this, range, "", semantics);
        return true;
    }

    private bool Revert(bool redo) {
        if (redo) document.Redo(this);
        else document.Undo(this);
        return true;
    }

    private bool Composed(Input.Composition composition) {
        if (composition.Final && document.IsImeComposing) {
            document.FinishImeComposition(this);
            selection = composing;
        }
        if (composition.Final) return Replace(composition.Text);
        if (!document.IsImeComposing) {
            composing = selection;
            document.StartImeComposition(this, selection);
        }
        StyledText styled = new();
        styled.AddText(composition.Text, document.DefaultStyle);
        document.UpdateImeComposition(this, styled, styled.Length);
        return true;
    }

    private bool Focused(bool gained) {
        if (!gained) document.FinishImeComposition(this);
        focused = gained;
        return true;
    }

    // --- [DOCUMENT]
    void ITextDocumentView.OnReset() => selection = new TextRange(0);

    void ITextDocumentView.OnRedraw() { }

    void ITextDocumentView.OnDocumentWillChange(ITextDocumentView view) { }

    void ITextDocumentView.OnDocumentChange(ITextDocumentView view, DocumentChangeInfo info) =>
        selection = (ReferenceEquals(view, this) || info.IsUndoing, info.Semantics) switch {
            (true, EditSemantics.ImeComposition) => new TextRange(info.CodePointIndex + info.ImeCaretOffset),
            (true, _) => new TextRange(info.CodePointIndex + info.NewLength),
            (false, _) => selection.UpdateForEdit(info.CodePointIndex, info.OldLength, info.NewLength),
        };

    void ITextDocumentView.OnDocumentDidChange(ITextDocumentView view) { }
}
