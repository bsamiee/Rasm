using System.Runtime.CompilerServices;
using System.Text;
using HarfBuzzSharp;
using Rasm.UI.Styling;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;
using Topten.RichTextKit.Editor;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TextStyle(Topten.RichTextKit.Style Style, SKTypeface Typeface, Seq<Feature> Features);

public sealed record Glyphs(IStyle Style, SKTypeface Typeface, Arr<ushort> Ids, Arr<SKPoint> Points, SKPoint Origin, float Advance);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Typeset {
    private Typeset(TextBlock lines) => Lines = lines;

    public TextBlock Lines { get; }
    public abstract SKSize Size { get; }
    public abstract SKRect Overhang { get; }
    public abstract bool Truncated { get; }
    public Option<float> Baseline => toSeq(Lines.Lines).Head.Map(static line => line.YCoord + line.BaseLine);

    public sealed record Block(TextBlock Lines) : Typeset(Lines) {
        public override SKSize Size => new(Lines.MeasuredWidth, Lines.MeasuredHeight);
        public override SKRect Overhang => Lines.MeasuredOverhang;
        public override bool Truncated => Lines.Truncated;
    }

    public sealed record Featured(TextBlock Lines, Seq<Glyphs> Runs, float Advance, float Width, SKRect Ink, bool Clipped) : Typeset(Lines) {
        public override SKSize Size => new(Advance, Lines.MeasuredHeight);
        public override SKRect Overhang => new(float.Max(0f, -Ink.Left), float.Max(0f, -Ink.Top), float.Max(0f, Ink.Right - Width), float.Max(0f, Ink.Bottom - Lines.MeasuredHeight));
        public override bool Truncated => Lines.Truncated || Clipped;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Editor : ITextDocumentView {
    // --- [STATE]
    private readonly TextDocument document;
    private Laid.Text laid;
    private string opened;
    private TextRange selection;
    private Option<float> ghost;
    private Option<TextRange> restore;
    private int marked;
    private float scroll;

    internal Editor(Laid.Text laid) {
        this.laid = laid;
        opened = laid.Source.Content;
        document = new TextDocument { SingleLineMode = Single(laid.Source), PlainTextMode = true, DefaultStyle = laid.TextStyle.Style, DefaultAlignment = laid.Source.Alignment };
        document.LineWrap = !document.SingleLineMode;
        document.SetMargins(0f, 0f, 0f, 0f);
        document.Text = opened;
        document.RegisterView(this);
        selection = new TextRange(document.Length - 1);
        Synced(laid);
    }

    public string Text => document.Text;
    public string Selected => document.GetText(selection.Normalized).ToString();
    public TextRange Selection => selection;
    public Option<TextRange> Marked => document.IsImeComposing ? Some(new TextRange(document.ImeCompositionOffset, document.ImeCompositionOffset + marked)) : None;
    public bool CanUndo => document.UndoManager.CanUndo;
    public bool CanRedo => document.UndoManager.CanRedo;

    public SKRect Caret =>
        document.GetCaretInfo(selection.CaretPosition).CaretRectangle switch {
            var at => SKRect.Create(at.Left + laid.Origin.X - scroll, at.Top + laid.Origin.Y, at.Width, at.Height),
        };

    // --- [INPUT]
    internal Option<IO<Seq<Edit<string>>>> Next(Gesture gesture, Laid.Text next, SKRect bounds) =>
        gesture.SwitchPartially(
            (Editor: this, Laid: next, At: bounds.Location),
            @default: static (_, _) => Option<IO<Seq<Edit<string>>>>.None,
            press: static (held, press) => Some(held.Editor.Changed(held.Laid, () => held.Editor.Pointed(press, press.At - held.At))),
            perform: static (held, perform) => Operation(held.Laid, perform).Map(operation => operation(held.Editor)));

    internal static Option<Func<Editor, IO<Seq<Edit<string>>>>> Operation(Laid.Text next, Gesture.Perform perform) =>
        perform.Act.SwitchPartially(
            (Laid: next, Extend: perform.Modifiers.Contains(Modifier.Shift), Single: Single(next.Source)),
            @default: static (_, _) => Option<Func<Editor, IO<Seq<Edit<string>>>>>.None,
            confirm: static (held, _) => Some(fun((Editor editor) => held.Single ? editor.Committed() : editor.Changed(held.Laid, () => editor.Inserted("\n")))),
            cancel: static (_, _) => Some(fun(static (Editor editor) => editor.Cancelled())),
            navigate: static (held, navigate) => Some(fun((Editor editor) => editor.Changed(held.Laid, () => editor.Moved(navigate.Toward, held.Extend)))),
            delete: static (held, delete) => Some(fun((Editor editor) => editor.Changed(held.Laid, () => editor.Erased(delete.Toward)))),
            next: static (held, _) => held.Single ? None : Some(fun((Editor editor) => editor.Changed(held.Laid, () => editor.Inserted("\t")))),
            selectAll: static (held, _) => Some(fun((Editor editor) => editor.Changed(held.Laid, () => editor.selection = editor.Clamped(editor.document.GetSelectionRange(editor.selection.CaretPosition, SelectionKind.Document))))),
            insert: static (held, insert) => Some(fun((Editor editor) => editor.Changed(held.Laid, () => editor.Inserted(insert.Text)))),
            compose: static (held, compose) => Some(fun((Editor editor) => editor.Changed(held.Laid, () => editor.Composed(compose.Marked, compose.Offset)))),
            undo: static (held, _) => Some(fun((Editor editor) => editor.Changed(held.Laid, () => editor.document.Undo(editor)))),
            redo: static (held, _) => Some(fun((Editor editor) => editor.Changed(held.Laid, () => editor.document.Redo(editor)))));

    private static bool Single(Node.Text text) => text.MaxLines.Exists(static lines => lines is 1);

    private void Pointed(Gesture.Press press, SKPoint local) =>
        selection = press switch {
            { Dragged.IsNone: true, Clicks: >= 2 } => Clamped(document.GetSelectionRange(selection.CaretPosition, press.Clicks is 2 ? SelectionKind.Word : SelectionKind.Paragraph)),
            { Dragged.IsNone: true } when !press.Modifiers.Contains(Modifier.Shift) => Clamped(new TextRange(At(local))),
            _ => At(local) switch { var at => Clamped(new TextRange(selection.Start, at.CodePointIndex, at.AltPosition)) },
        };

    private CaretPosition At(SKPoint local) => document.HitTest(local.X - laid.Origin.X + scroll, local.Y - laid.Origin.Y).CaretPosition;

    private TextRange Clamped(TextRange range) => range.Clamp(document.Length - 1);

    // --- [EDITS]
    private IO<Seq<Edit<string>>> Changed(Laid.Text next, Action change) =>
        IO.lift(() => {
            Synced(next);
            string before = Text;
            change();
            Synced(next);
            return string.Equals(Text, before, StringComparison.Ordinal) ? Seq<Edit<string>>() : [new Edit<string>.Preview(Text)];
        });

    private void Moved(Navigation toward, bool extend) {
        float? remembered = ghost.ToNullable();
        CaretPosition moved = document.Navigate(selection.CaretPosition, toward.Key, laid.ContentBox.Height, ref remembered);
        ghost = Optional(remembered);
        selection = Clamped(extend ? new TextRange(selection.Start, moved.CodePointIndex, moved.AltPosition) : new TextRange(moved));
    }

    private void Erased(Option<Navigation> toward) {
        float? held = null;
        CaretPosition caret = selection.CaretPosition;
        Option<int> target = selection.IsRange ? None : toward.Map(navigation => document.Navigate(caret, navigation.Key, 0f, ref held).CodePointIndex);
        ghost = None;
        document.ReplaceText(this, target.Map(index => new TextRange(selection.End, index)).IfNone(selection), "",
            target.Map(index => index < selection.End ? EditSemantics.Backspace : EditSemantics.ForwardDelete).IfNone(EditSemantics.None));
    }

    private void Inserted(string text) {
        Finished();
        ghost = None;
        document.ReplaceText(this, selection, text, EditSemantics.Typing);
    }

    private void Composed(string text, int offset) {
        if (text.Length is 0) {
            Finished();
            return;
        }
        if (restore.IsNone) {
            restore = selection;
            document.StartImeComposition(this, selection);
        }
        StyledText styled = new();
        styled.AddText(text, laid.TextStyle.Style.Modify(underline: UnderlineStyle.ImeInput));
        document.UpdateImeComposition(this, styled, offset);
    }

    private void Finished() {
        _ = restore.Iter(initial => {
            document.FinishImeComposition(this);
            selection = initial;
        });
        restore = None;
    }

    internal IO<Seq<Edit<string>>> Committed() =>
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
        float width = next.ContentBox.Width;
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
        using SKAutoCanvasRestore restored = new(canvas);
        canvas.ClipRect(content, antialias: true);
        canvas.Translate(next.Origin.X - scroll, next.Origin.Y);
        document.Paint(canvas, 0f, content.Height, options);
        if (selection.IsRange || caret <= 0f) return;
        using SKPaint paint = new() { Color = ink, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = caret };
        SKRect at = document.GetCaretInfo(selection.CaretPosition).CaretRectangle;
        canvas.DrawLine(at.Right + (caret / 2f), at.Top, at.Left + (caret / 2f), at.Bottom, paint);
    }

    // --- [DOCUMENT]
    void ITextDocumentView.OnReset() => selection = new TextRange(document.Length - 1);

    void ITextDocumentView.OnRedraw() { }

    void ITextDocumentView.OnDocumentWillChange(ITextDocumentView view) { }

    void ITextDocumentView.OnDocumentChange(ITextDocumentView view, DocumentChangeInfo info) =>
        (selection, marked) = (ReferenceEquals(view, this) || info.IsUndoing, info.Semantics) switch {
            (true, EditSemantics.ImeComposition) => (new TextRange(info.CodePointIndex + info.ImeCaretOffset), info.NewLength),
            (true, _) => (new TextRange(info.CodePointIndex + info.NewLength), marked),
            (false, _) => (selection.UpdateForEdit(info.CodePointIndex, info.OldLength, info.NewLength), marked),
        };

    void ITextDocumentView.OnDocumentDidChange(ITextDocumentView view) { }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Typesetting {
    // --- [RESOLVE]
    private static readonly Topten.RichTextKit.Style Plain = new();

    private static readonly Func<(TextFace Face, float Size, SKColor Ink, SKColor Halo), TextStyle> Faces = memo<(TextFace Face, float Size, SKColor Ink, SKColor Halo), TextStyle>(Faced);

    public static TextStyle Resolve(Cascade within, Styling.Style style) =>
        within.Theme.Face.Combine(style.Face) switch {
            var face => Faces((face, within.Theme.FontSize * style.Metrics[Metric.FontSize],
                within.Theme.Color(style.Text, style, None, None).Map(ink => (SKColor)Styles.Legible(style, within.Ground, ink, within.Theme.Contrast.Text)).IfNone(SKColors.Transparent),
                face.Halo.Bind(halo => within.Theme.Color(halo.Ink, style, None, None)).Map(static halo => (SKColor)halo).IfNone(Plain.HaloColor))),
        };

    private static TextStyle Faced((TextFace Face, float Size, SKColor Ink, SKColor Halo) held) {
        Topten.RichTextKit.Style resolved = new() {
            FontFamily = held.Face.Family.IfNone(Plain.FontFamily),
            FontSize = held.Size,
            FontWeight = held.Face.Weight.Map(static weight => (int)weight).IfNone(Plain.FontWeight),
            FontWidth = held.Face.Width.IfNone(Plain.FontWidth),
            FontItalic = held.Face.Italic.IfNone(Plain.FontItalic),
            LetterSpacing = held.Face.LetterSpacing.IfNone(Plain.LetterSpacing),
            LineHeight = held.Face.LineHeight.IfNone(Plain.LineHeight),
            Underline = held.Face.Underline.IfNone(Plain.Underline),
            StrikeThrough = held.Face.StrikeThrough.IfNone(Plain.StrikeThrough),
            FontVariant = held.Face.Variant.IfNone(Plain.FontVariant),
            TextColor = held.Ink,
            HaloColor = held.Halo,
            HaloWidth = held.Face.Halo.Map(static halo => halo.Width).IfNone(Plain.HaloWidth),
            HaloBlur = held.Face.Halo.Map(static halo => halo.Blur).IfNone(Plain.HaloBlur),
        };
        resolved.Seal();
        return new TextStyle(resolved, FontMapper.Default.TypefaceFromStyle(resolved, ignoreFontVariants: false), held.Face.Features.IfNone([]));
    }

    private static TextStyle Spanned(Cascade within, TextStyle face, Span span) =>
        span.Role.IsNone && span.Conditions.IsEmpty
            ? face
            : Resolve(within, Styles.Resolve(within, span.Role.IfNone(within.Enclosing.Role), span.Conditions.Union(within.Enclosing.Conditions)));

    // --- [SET]
    public static Typeset Set(Node.Text text, Cascade within, TextStyle face, float width) =>
        text.Spans.Map(span => (span.Text, Face: Spanned(within, face, span))) switch {
            var spans => spans.Exists(static span => !span.Face.Features.IsEmpty) ? Featured(text, spans, width) : new Typeset.Block(Lined(text, spans, width)),
        };

    internal static float MinContent(Typeset typeset) => Narrowest(typeset.Lines) + float.Max(0f, typeset.Size.Width - typeset.Lines.MeasuredWidth);

    private static TextBlock Lined(Node.Text text, Seq<(string Text, TextStyle Face)> spans, float measure) {
        TextBlock block = new() { MaxWidth = float.IsFinite(measure) ? measure : null, MaxLines = text.MaxLines.ToNullable(), Alignment = text.Alignment };
        _ = spans.Iter(span => block.AddText(span.Text, span.Face.Style));
        return block;
    }

    private static float Narrowest(TextBlock text) {
        text.Layout();
        return toSeq(Enumerable.Range(0, text.Length + 1).Where(index => Spaced(text, index - 1) != Spaced(text, index)).Chunk(2))
            .Fold(0f, (widest, word) => float.Max(widest, MathF.Abs(Caret(text, word[1]) - Caret(text, word[0]))));
    }

    private static bool Spaced(TextBlock text, int index) => index < 0 || index >= text.Length || text.CodePoints[index] switch { var point => Rune.IsValid(point) && Rune.IsWhiteSpace(new Rune(point)) };

    private static float Caret(TextBlock text, int index) => text.GetCaretInfo(new CaretPosition(index)).CaretXCoord;

    // --- [FEATURES]
    private sealed record Shaped(FontRun Run, Arr<ushort> Ids, Arr<SKPoint> Offsets, float Advance);

    private sealed record Reshaped(TextLine Line, Seq<Shaped> Runs) {
        public float Advance { get; } = Runs.Fold(0f, static (sum, run) => sum + run.Advance);
    }

    private static readonly ConditionalWeakTable<SKTypeface, Font> Shapers = [];

    private static Typeset Featured(Node.Text text, Seq<(string Text, TextStyle Face)> spans, float width) {
        HashMap<IStyle, Seq<Feature>> features = HashMap<IStyle, Seq<Feature>>().AddOrUpdateRange(spans.Map(static span => ((IStyle)span.Face.Style, span.Face.Features)));
        return Fitted(width, []);

        Typeset Fitted(float measure, Seq<int> prior) {
            TextBlock lines = Lined(text, spans, measure);
            Seq<Reshaped> reshaped = toSeq(lines.Lines).Map(line => new Reshaped(line,
                toSeq(line.Runs.Where(static run => run.RunKind is not FontRunKind.TrailingWhitespace).OrderBy(static run => run.XCoord))
                    .Map(run => features[run.Style] switch {
                        var featured => run.RunKind is FontRunKind.Ellipsis || featured.IsEmpty ? Kept(run) : Shape(run, featured),
                    })));
            Seq<int> breaks = toSeq(lines.Lines).Map(static line => line.End);
            Seq<Reshaped> over = reshaped.Filter(line => line.Advance > width);
            float advance = reshaped.Fold(0f, static (widest, line) => float.Max(widest, line.Advance));
            return !over.IsEmpty && breaks != prior
                ? Fitted(over.Fold(measure, (least, line) => float.Min(least, line.Line.Width * width / line.Advance)), breaks)
                : Placed(lines, reshaped, float.IsFinite(width) ? width : advance, advance, !over.IsEmpty, Leaning(lines));
        }
    }

    private static Shaped Shape(FontRun run, Seq<Feature> features) {
        using HarfBuzzSharp.Buffer buffer = new();
        buffer.AddUtf32(run.CodePoints.AsSpan());
        buffer.Direction = run.Direction is TextDirection.RTL ? Direction.RightToLeft : Direction.LeftToRight;
        buffer.GuessSegmentProperties();
        Shapers.GetValue(run.Typeface, Shaper).Shape(buffer, [.. features]);
        float scale = run.Style.FontSize / run.Typeface.UnitsPerEm;
        GlyphPosition[] placed = buffer.GlyphPositions;
        Seq<float> pens = toSeq(placed).Scan(0f, (pen, at) => pen + (at.XAdvance * scale));
        return new Shaped(run, [.. buffer.GlyphInfos.Select(static glyph => (ushort)glyph.Codepoint)],
            [.. placed.Zip(pens, (at, pen) => new SKPoint(pen + (at.XOffset * scale), -at.YOffset * scale))], pens.Last.IfNone(0f));
    }

    private static Font Shaper(SKTypeface typeface) {
        using Blob blob = typeface.OpenStream(out int index).ToHarfBuzzBlob();
        using Face face = new(blob, index);
        Font font = new(face);
        font.SetVariations([.. typeface.VariationDesignPosition.Select(static at => new Variation { Tag = at.Axis, Value = at.Value })]);
        return font;
    }

    private static Shaped Kept(FontRun run) =>
        new SKPoint(run.XCoord, run.Line.YCoord + run.Line.BaseLine) switch {
            var origin => new Shaped(run, [.. run.Glyphs.ToArray()], [.. run.GlyphPositions.ToArray().Select(at => at - origin)], run.Width),
        };

    private static float Leaning(TextBlock lines) =>
        toSeq(lines.Lines).Choose(line => (Slack: (lines.MaxWidth ?? lines.MeasuredWidth) - line.Width, Starts: toSeq(line.Runs).Filter(static run => run.RunKind is not FontRunKind.TrailingWhitespace).Map(static run => run.XCoord)) switch {
            { Slack: var slack, Starts.IsEmpty: false } held when slack.CompareTo(0f) is not 0 => Some(held.Starts.Fold(float.PositiveInfinity, float.Min) / slack),
            _ => Option<float>.None,
        }).Head.IfNone(0f);

    private static Typeset.Featured Placed(TextBlock lines, Seq<Reshaped> reshaped, float reference, float advance, bool clipped, float lean) {
        Seq<Glyphs> runs =
            from line in reshaped
            from placed in line.Runs.Zip(line.Runs.Scan(lean * (reference - line.Advance), static (x, run) => x + run.Advance))
            let origin = new SKPoint(placed.Second, line.Line.YCoord + line.Line.BaseLine)
            select new Glyphs(placed.First.Run.Style, placed.First.Run.Typeface, placed.First.Ids, [.. placed.First.Offsets.Map(at => at + origin)], origin, placed.First.Advance);
        return new Typeset.Featured(lines, runs, advance, reference, runs.Fold(SKRect.Empty, static (ink, run) => Union(ink, Ink(run))), clipped);
    }

    private static SKRect Ink(Glyphs run) {
        using SKFont font = new(run.Typeface, run.Style.FontSize);
        _ = font.GetGlyphWidths(run.Ids.AsSpan(), out SKRect[] bounds);
        return bounds.Zip(run.Points, static (bound, at) => SKRect.Create(bound.Left + at.X, bound.Top + at.Y, bound.Width, bound.Height)).Aggregate(SKRect.Empty, Union);
    }

    private static SKRect Union(SKRect held, SKRect next) => held.IsEmpty ? next : next.IsEmpty ? held : SKRect.Union(held, next);

    // --- [PAINT]
    internal static void Paint(SKCanvas canvas, PaintCache cache, Laid.Text laid, SKColor ink) =>
        laid.Typeset.Switch(
            (Canvas: canvas, Cache: cache, Laid: laid, Ink: ink, Resting: laid.TextStyle.Style.TextColor),
            block: static (held, block) => (held.Ink == held.Resting ? block.Lines : Reinked(block.Lines, held.Resting, held.Ink)).Paint(held.Canvas, held.Laid.Origin, held.Cache.Raster),
            featured: static (held, featured) => held.Cache.Blobs(held.Laid, featured).Iter(run => Lettered(
                held.Canvas, run.Style, run.Blob, featured.Runs.Filter(glyphs => ReferenceEquals(glyphs.Style, run.Style)), held.Laid.Origin,
                run.Style.TextColor == held.Resting ? held.Ink : run.Style.TextColor)));

    private static void Lettered(SKCanvas canvas, IStyle style, SKTextBlob blob, Seq<Glyphs> runs, SKPoint origin, SKColor ink) {
        Seq<(SKPoint From, SKPoint To, float Width)> rules = runs.Bind(run => Ruled(style, run, blob)).Strict();
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.Translate(origin.X, origin.Y);
        if (style.HaloColor != SKColor.Empty) {
            using SKMaskFilter? blur = style.HaloBlur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, style.HaloBlur) : null;
            using SKPaint halo = new() {
                Color = style.HaloColor, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = style.HaloWidth, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round, MaskFilter = blur,
            };
            _ = rules.Iter(rule => {
                halo.StrokeWidth = rule.Width + style.HaloWidth;
                canvas.DrawLine(rule.From, rule.To, halo);
            });
            halo.StrokeWidth = style.HaloWidth;
            canvas.DrawText(blob, 0f, 0f, halo);
        }
        using SKPaint fill = new() { Color = ink, IsAntialias = true };
        canvas.DrawText(blob, 0f, 0f, fill);
        _ = rules.Iter(rule => {
            fill.StrokeWidth = rule.Width;
            canvas.DrawLine(rule.From, rule.To, fill);
        });
    }

    private static Seq<(SKPoint From, SKPoint To, float Width)> Ruled(IStyle style, Glyphs run, SKTextBlob blob) {
        using SKFont font = new(run.Typeface, style.FontSize);
        SKFontMetrics metrics = font.Metrics;
        (float start, float end) = (run.Origin.X, run.Origin.X + run.Advance);
        float under = float.Max(run.Origin.Y + metrics.UnderlinePosition.GetValueOrDefault(), run.Origin.Y + 1f);
        float thickness = float.Max(metrics.UnderlineThickness ?? 1f, 1f);
        Seq<(float From, float To)> spans = style.Underline switch {
            UnderlineStyle.None => [],
            UnderlineStyle.Gapped => toSeq(blob.GetIntercepts(under - (thickness / 2f), under + thickness).Chunk(2))
                .Fold((Seq: Seq<(float From, float To)>(), At: start), (held, gap) => (gap[0] - thickness) switch {
                    var stop => (stop > held.At && held.At < end ? held.Seq.Add((held.At, float.Min(stop, end))) : held.Seq, float.Max(held.At, gap[1] + thickness)),
                }) switch { var held => held.At < end ? held.Seq.Add((held.At, end)) : held.Seq, },
            _ => [(start, end)],
        };
        Seq<(SKPoint From, SKPoint To, float Width)> underlined = spans.Map(span => (new SKPoint(span.From, under), new SKPoint(span.To, under), style.Underline is UnderlineStyle.ImeTargetConverted ? thickness * 2f : thickness));
        float struck = run.Origin.Y + metrics.StrikeoutPosition.GetValueOrDefault();
        return style.StrikeThrough is StrikeThroughStyle.None
            ? underlined
            : underlined.Add((new SKPoint(start, struck), new SKPoint(end, struck), float.Max(metrics.StrikeoutThickness ?? 1f, 1f)));
    }

    private static TextBlock Reinked(TextBlock block, SKColor rest, SKColor ink) {
        TextBlock inked = block.Copy(0, block.Length);
        _ = toSeq(block.StyleRuns)
            .Choose(run => Optional(run.Style as Topten.RichTextKit.Style).Filter(style => style.TextColor == rest).Map(style => (run.Start, run.Length, Style: style.Modify(textColor: ink))))
            .Iter(run => inked.ApplyStyle(run.Start, run.Length, run.Style));
        return inked;
    }
}
