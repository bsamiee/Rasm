using System.Runtime.CompilerServices;
using HarfBuzzSharp;
using Rasm.UI.Styling;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;
using Topten.RichTextKit.Editor;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TextStyle(Topten.RichTextKit.Style Style, SKTypeface Typeface, Option<Font> Shaper);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Typeset {
    public abstract SKSize Size { get; }
    public abstract SKRect Overhang { get; }

    public sealed record Block(TextBlock Text) : Typeset {
        public override SKSize Size => new(Text.MeasuredWidth, Text.MeasuredHeight);
        public override SKRect Overhang => Text.MeasuredOverhang;
    }

    public sealed record Run(SKShaper.Result Shaped, SKFontMetrics Metrics) : Typeset {
        public override SKSize Size => new(Shaped.Width, Metrics.Descent - Metrics.Ascent + Metrics.Leading);
        public override SKRect Overhang => SKRect.Empty;
    }
}

[ValueObject<NavigationKind>(SkipIParsable = true, SkipIComparable = true)]
public sealed partial class Caret {
    public EditSemantics Erasing =>
        _value switch {
            NavigationKind.CharacterLeft or NavigationKind.WordLeft or NavigationKind.LineUp or NavigationKind.PageUp or NavigationKind.LineHome or NavigationKind.DocumentHome => EditSemantics.Backspace,
            NavigationKind.None or NavigationKind.CharacterRight or NavigationKind.WordRight or NavigationKind.LineDown or NavigationKind.PageDown or NavigationKind.LineEnd or NavigationKind.DocumentEnd => EditSemantics.ForwardDelete,
        };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref NavigationKind value) =>
        validationError = value is NavigationKind.None ? new ValidationError("a caret moves toward some position") : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EditCommand {
    public sealed record Move(Caret Toward, bool Extend) : EditCommand;

    public sealed record Erase(Caret Toward) : EditCommand;

    public sealed record Insert(string Text) : EditCommand;

    public sealed record Compose(string Marked, int Offset) : EditCommand;

    public sealed record Undo : EditCommand;

    public sealed record Redo : EditCommand;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Editor : ITextDocumentView {
    private readonly TextDocument document;
    private Laid.Text laid;
    private string opened;
    private TextRange selection;
    private Option<float> ghost;
    private Option<(TextRange Initial, int Length)> composing;
    private float scroll;

    internal Editor(Node.Text text, Laid.Text laid) {
        this.laid = laid;
        opened = text.Content;
        document = new TextDocument { SingleLineMode = text.MaxLines.Exists(static lines => lines is 1), PlainTextMode = true, DefaultStyle = laid.TextStyle.Style };
        document.LineWrap = !document.SingleLineMode;
        document.SetMargins(0f, 0f, 0f, 0f);
        document.Text = opened;
        document.RegisterView(this);
        selection = new TextRange(document.Length - 1);
    }

    public string Text => document.GetText(new TextRange(0, document.Length - 1)).ToString();
    public string Selected => document.GetText(selection.Normalized).ToString();
    public TextRange Selection => selection;
    public Option<TextRange> Marked => composing.Map(static held => new TextRange(held.Initial.Minimum, held.Initial.Minimum + held.Length));
    public bool CanUndo => document.UndoManager.CanUndo;
    public bool CanRedo => document.UndoManager.CanRedo;

    public SKRect Caret =>
        document.GetCaretInfo(selection.CaretPosition).CaretRectangle switch {
            var at => SKRect.Create(at.Left + laid.Origin.X - scroll, at.Top + laid.Origin.Y, at.Width, at.Height),
        };

    // --- [INPUT]
    internal Option<IO<Seq<Edit<string>>>> Next(Input input, Laid.Text next, SKRect bounds, bool pressing) =>
        input.SwitchPartially(
            (Editor: this, Laid: next, At: bounds.Location, Pressing: pressing),
            @default: static (_, _) => None,
            pointer: static (held, pointer) => (pointer.Phase, held.Pressing) switch {
                (PointerPhase.Move, false) or (PointerPhase.Leave, _) => None,
                _ => Some(held.Editor.Changed(held.Laid, () => held.Editor.Pointed(pointer, pointer.At - held.At))),
            },
            perform: static (held, perform) => held.Editor.Performed(held.Laid, perform.Act),
            edit: static (held, edit) => Some(held.Editor.Changed(held.Laid, () => held.Editor.Apply(edit.Command))),
            focus: static (held, focus) => Some(focus.Gained ? IO.pure(Seq<Edit<string>>()) : held.Editor.Committed()));

    private Option<IO<Seq<Edit<string>>>> Performed(Laid.Text next, Act act) =>
        act.SwitchPartially(
            (Editor: this, Laid: next),
            @default: static (_, _) => None,
            confirm: static (held, _) => Some(held.Editor.document.SingleLineMode ? held.Editor.Committed() : held.Editor.Changed(held.Laid, () => held.Editor.Inserted("\n"))),
            cancel: static (held, _) => Some(held.Editor.Cancelled()),
            all: static (held, _) => Some(held.Editor.Changed(held.Laid, () => held.Editor.selection = held.Editor.document.GetSelectionRange(held.Editor.selection.CaretPosition, SelectionKind.Document))),
            tab: static (held, _) => held.Editor.document.SingleLineMode ? None : Some(held.Editor.Changed(held.Laid, () => held.Editor.Inserted("\t"))));

    private void Pointed(Input.Pointer pointer, SKPoint local) =>
        selection = pointer switch {
            { Phase: PointerPhase.Down, Count: >= 2 } => document.GetSelectionRange(selection.CaretPosition, pointer.Count is 2 ? SelectionKind.Word : SelectionKind.Paragraph),
            { Phase: PointerPhase.Down, Modifiers.Shift: false } => new TextRange(Index(local)),
            { Phase: PointerPhase.Down or PointerPhase.Move } => new TextRange(selection.Start, Index(local)),
            _ => selection,
        };

    private int Index(SKPoint local) => int.Clamp(document.HitTest(local.X - laid.Origin.X + scroll, local.Y - laid.Origin.Y).ClosestCodePointIndex, 0, document.Length - 1);

    // --- [EDITS]
    private IO<Seq<Edit<string>>> Changed(Laid.Text next, Action change) =>
        IO.lift(() => {
            Synced(next);
            string before = Text;
            change();
            Synced(next);
            return string.Equals(Text, before, StringComparison.Ordinal) ? Seq<Edit<string>>() : [new Edit<string>.Preview(Text)];
        });

    private void Apply(EditCommand command) =>
        command.Switch(
            this,
            move: static (editor, move) => editor.Moved(move.Toward, move.Extend),
            erase: static (editor, erase) => editor.Erased(erase.Toward),
            insert: static (editor, insert) => editor.Inserted(insert.Text),
            compose: static (editor, compose) => editor.Composed(compose.Marked, compose.Offset),
            undo: static (editor, _) => editor.document.Undo(editor),
            redo: static (editor, _) => editor.document.Redo(editor));

    private void Moved(Caret toward, bool extend) {
        float? remembered = ghost.ToNullable();
        CaretPosition moved = document.Navigate(selection.CaretPosition, (NavigationKind)toward, laid.Frame.Height - (2f * Layouts.Inset(laid)), ref remembered);
        ghost = Optional(remembered);
        int at = int.Min(moved.CodePointIndex, document.Length - 1);
        selection = extend ? new TextRange(selection.Start, at, moved.AltPosition) : new TextRange(at, moved.AltPosition);
    }

    private void Erased(Caret toward) {
        float? held = null;
        TextRange range = selection.IsRange ? selection : new TextRange(selection.End, document.Navigate(selection.CaretPosition, (NavigationKind)toward, 0f, ref held).CodePointIndex);
        ghost = None;
        document.ReplaceText(this, range, "", toward.Erasing);
    }

    private void Inserted(string text) {
        Finished();
        ghost = None;
        document.ReplaceText(this, selection, text, EditSemantics.Typing);
    }

    private void Composed(string marked, int offset) {
        if (marked.Length is 0) {
            Finished();
            return;
        }
        TextRange initial = composing.Map(static held => held.Initial).IfNone(selection);
        if (composing.IsNone) document.StartImeComposition(this, initial);
        StyledText styled = new();
        styled.AddText(marked, laid.TextStyle.Style.Modify(underline: UnderlineStyle.ImeInput));
        composing = (initial, styled.Length);
        document.UpdateImeComposition(this, styled, offset);
    }

    private void Finished() {
        _ = composing.Iter(held => {
            document.FinishImeComposition(this);
            selection = held.Initial;
        });
        composing = None;
    }

    private IO<Seq<Edit<string>>> Committed() =>
        IO.lift(() => {
            Finished();
            document.UndoManager.Clear();
            opened = Text;
            return Seq<Edit<string>>(new Edit<string>.Commit(opened));
        });

    private IO<Seq<Edit<string>>> Cancelled() =>
        IO.lift(() => {
            Finished();
            document.Text = opened;
            return Seq<Edit<string>>(new Edit<string>.Cancel());
        });

    private void Synced(Laid.Text next) {
        laid = next;
        float width = Layouts.Content(next).Width;
        if (!ReferenceEquals(document.DefaultStyle, next.TextStyle.Style)) document.DefaultStyle = next.TextStyle.Style;
        if (document.PageWidth.CompareTo(width) is not 0) document.PageWidth = width;
        float caret = document.GetCaretInfo(selection.CaretPosition).CaretXCoord;
        float span = width - next.Style.Metrics[Metric.Stroke];
        scroll = float.Clamp(float.Max(caret - span, float.Min(scroll, caret)), 0f, float.Max(0f, document.MeasuredWidth - span));
    }

    // --- [PAINT]
    internal void Paint(SKCanvas canvas, Laid.Text next, SKRect content, TextPaintOptions raster, SKColor highlight, SKColor ink, float caret) {
        Synced(next);
        TextPaintOptions options = raster.Clone();
        options.Selection = selection.IsRange ? selection : null;
        options.SelectionColor = highlight;
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(content, antialias: true);
        canvas.Translate(next.Origin.X - scroll, next.Origin.Y);
        document.Paint(canvas, 0f, content.Height, options);
        if (selection.IsRange) return;
        using SKPaint paint = new() { Color = ink, IsAntialias = true };
        SKRect at = document.GetCaretInfo(selection.CaretPosition).CaretRectangle;
        canvas.DrawRect(SKRect.Create(at.Left, at.Top, caret, at.Height), paint);
    }

    // --- [DOCUMENT]
    void ITextDocumentView.OnReset() => selection = new TextRange(document.Length - 1);

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Typesetting {
    private static readonly ConditionalWeakTable<SKTypeface, Font> Fonts = [];

    // --- [RESOLVE]
    public static TextStyle Resolve(Theme theme, Styling.Style style) {
        Topten.RichTextKit.Style resolved = new() {
            FontFamily = theme.Family,
            FontSize = theme.Type * style.Metrics[Metric.Type],
            FontWeight = style.Look.Weight,
            TextColor = theme.Color(style.Look.Text, style, Interaction.Rest, None).Map(static ink => (SKColor)ink).IfNone(SKColors.Transparent),
        };
        resolved.Seal();
        SKTypeface typeface = FontMapper.Default.TypefaceFromStyle(resolved, ignoreFontVariants: false);
        return new TextStyle(resolved, typeface, style.Look.Features.IsEmpty ? None : Some(Fonts.GetValue(typeface, Shaping)));
    }

    private static Font Shaping(SKTypeface typeface) {
        using Blob blob = typeface.OpenStream(out int index).ToHarfBuzzBlob();
        using Face face = new(blob, index);
        return new Font(face);
    }

    // --- [SET]
    public static Typeset Set(Node.Text text, Styling.Style style, TextStyle face, float width) =>
        face.Shaper.Filter(_ => text.MaxLines.Exists(static lines => lines is 1) && !text.Part.Exists(static part => part.Edits)).Match(
            Some: font => Shaped(text, style, face, font),
            None: () => (Typeset)Blocked(text, face, width));

    private static Typeset.Block Blocked(Node.Text text, TextStyle face, float width) {
        TextBlock block = new() { MaxWidth = float.IsFinite(width) ? width : null, MaxLines = text.MaxLines.ToNullable(), Alignment = text.Align };
        block.AddText(text.Content, face.Style);
        return new Typeset.Block(block);
    }

    private static Typeset.Run Shaped(Node.Text text, Styling.Style style, TextStyle face, Font font) {
        using HarfBuzzSharp.Buffer buffer = new();
        buffer.AddUtf16(text.Content);
        buffer.GuessSegmentProperties();
        font.Shape(buffer, [.. style.Look.Features]);
        float scale = face.Style.FontSize / face.Typeface.UnitsPerEm;
        GlyphInfo[] glyphs = buffer.GetGlyphInfoSpan().ToArray();
        GlyphPosition[] placed = buffer.GetGlyphPositionSpan().ToArray();
        Seq<float> pens = toSeq(placed).Scan(0f, (pen, at) => pen + (at.XAdvance * scale));
        using SKFont metrics = face.Typeface.ToFont(face.Style.FontSize);
        return new Typeset.Run(
            new SKShaper.Result(
                [.. glyphs.Select(static glyph => glyph.Codepoint)],
                [.. glyphs.Select(static glyph => glyph.Cluster)],
                [.. placed.Zip(pens, (at, pen) => new SKPoint(pen + (at.XOffset * scale), -at.YOffset * scale))],
                pens.Last.IfNone(0f)),
            metrics.Metrics);
    }

    // --- [PAINT]
    internal static void Paint(SKCanvas canvas, Laid.Text laid, SKColor ink, TextPaintOptions raster) =>
        laid.Typeset.Switch(
            (Canvas: canvas, Laid: laid, Ink: ink, Raster: raster),
            block: static (held, block) =>
                (held.Ink == held.Laid.TextStyle.Style.TextColor ? block.Text : Inked(block.Text, held.Laid.TextStyle.Style.Modify(textColor: held.Ink)))
                .Paint(held.Canvas, held.Laid.Origin, held.Raster),
            run: static (held, run) => {
                using SKFont font = new(held.Laid.TextStyle.Typeface, held.Laid.TextStyle.Style.FontSize) { Edging = held.Raster.Edging, Subpixel = held.Raster.SubpixelPositioning, Hinting = held.Raster.Hinting };
                using SKTextBlobBuilder builder = new();
                builder.AddPositionedRun([.. run.Shaped.Codepoints.Select(static glyph => (ushort)glyph)], font, run.Shaped.Points);
                using SKTextBlob? blob = builder.Build();
                using SKPaint paint = new() { Color = held.Ink, IsAntialias = true };
                _ = Optional(blob).Iter(built => held.Canvas.DrawText(built, held.Laid.Origin.X, held.Laid.Origin.Y - run.Metrics.Ascent, paint));
            });

    private static TextBlock Inked(TextBlock block, IStyle style) {
        TextBlock inked = block.Copy(0, block.Length);
        inked.ApplyStyle(0, inked.Length, style);
        return inked;
    }
}
