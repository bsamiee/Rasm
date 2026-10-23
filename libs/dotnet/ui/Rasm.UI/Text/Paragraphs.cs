using HarfBuzzSharp;
using Rasm.UI.Layout;
using Rasm.UI.Scenes;
using Rasm.UI.Styling;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;

namespace Rasm.UI.Text;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Run(Seq<ushort> Glyphs, Seq<SKPoint> Positions, SKTypeface Typeface, float Size, float Width, float Ascent, float Height);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Paragraphs {
    private const int Units = 512;

    // --- [MEASURE]
    public static SKSize Measure(Theme theme, Cascade cascade, Node.Text text, float width) =>
        text.Type.IfNone(cascade.Type) switch {
            var type when type.Features.IsEmpty => Block(theme, type, text, width, SKColors.Black) switch {
                var block => new SKSize(block.MeasuredWidth, block.MeasuredHeight),
            },
            var type => Shaped(theme, type, text.Content) switch {
                var run => new SKSize(run.Width, run.Height),
            },
        };

    public static HitTestResult HitTest(Theme theme, Cascade cascade, Node.Text text, SKRect frame, SKPoint at) =>
        Block(theme, text.Type.IfNone(cascade.Type), text, frame.Width, SKColors.Black) switch {
            var block => block.HitTest(at.X - frame.Left, at.Y - Origin(frame, block.MeasuredHeight).Y),
        };

    // --- [PAINT]
    public static Unit Paint(SKCanvas canvas, Theme theme, Cascade cascade, Node.Text text, SKRect frame, SKColor ink, SKColor selection) {
        TypeRole type = text.Type.IfNone(cascade.Type);
        if (!type.Features.IsEmpty) return Drawn(canvas, Shaped(theme, type, text.Content), text.Align, frame, ink);
        TextBlock block = Block(theme, type, text, frame.Width, ink);
        SKPoint origin = Origin(frame, block.MeasuredHeight);
        block.Paint(canvas, origin, text.Selection.Map(range => new TextPaintOptions { Selection = range, SelectionColor = selection }).IfNone(TextPaintOptions.Default));
        return text.Selection.Filter(static range => !range.IsRange).Iter(range => {
            SKRect caret = block.GetCaretInfo(new CaretPosition(range.End, range.AltPosition)).CaretRectangle;
            using SKPaint paint = new() { Color = ink, IsAntialias = true };
            canvas.DrawRect(SKRect.Create(origin.X + caret.Left, origin.Y + caret.Top, float.Max(1f, caret.Width), caret.Height), paint);
        });
    }

    // --- [STYLE]
    internal static Style Style(Theme theme, TypeRole type, SKColor ink) =>
        new() { FontFamily = theme.FontFamily, FontSize = theme.FontSize * type.Scale, FontWeight = type.Weight, TextColor = ink };

    private static TextBlock Block(Theme theme, TypeRole type, Node.Text text, float width, SKColor ink) {
        TextBlock block = new() {
            MaxWidth = float.IsFinite(width) ? width : null,
            MaxLines = text.MaxLines.ToNullable(),
            Alignment = text.Align,
        };
        block.AddText(text.Content, Style(theme, type, ink));
        return block;
    }

    internal static SKPoint Origin(SKRect frame, float height) => new(frame.Left, frame.Top + float.Max(0f, (frame.Height - height) / 2f));

    // --- [SHAPED]
    private static Run Shaped(Theme theme, TypeRole type, string content) {
        SKTypeface typeface = FontMapper.Default.TypefaceFromStyle(Style(theme, type, SKColors.Black), ignoreFontVariants: false);
        float size = theme.FontSize * type.Scale;
        using Blob blob = typeface.OpenStream(out int index).ToHarfBuzzBlob();
        using Face face = new(blob, index) { UnitsPerEm = typeface.UnitsPerEm };
        using Font font = new(face);
        font.SetScale(Units, Units);
        font.SetFunctionsOpenType();
        using HarfBuzzSharp.Buffer buffer = new();
        buffer.AddUtf16(content);
        buffer.GuessSegmentProperties();
        font.Shape(buffer, [.. type.Features.Select(Feature.Parse)]);
        float scale = size / Units;
        Seq<GlyphPosition> placed = toSeq(buffer.GetGlyphPositionSpan().ToArray());
        Seq<float> pens = placed.Scan(0f, (pen, at) => pen + (at.XAdvance * scale));
        using SKFont metrics = new(typeface, size);
        return new Run(
            toSeq(buffer.GetGlyphInfoSpan().ToArray()).Map(static info => (ushort)info.Codepoint),
            placed.Zip(pens).Map(pair => new SKPoint(pair.Second + (pair.First.XOffset * scale), -pair.First.YOffset * scale)),
            typeface, size, pens.Last.IfNone(0f), -metrics.Metrics.Ascent, metrics.Metrics.Descent - metrics.Metrics.Ascent + metrics.Metrics.Leading);
    }

    private static Unit Drawn(SKCanvas canvas, Run run, TextAlignment align, SKRect frame, SKColor ink) {
        using SKFont font = new(run.Typeface, run.Size);
        using SKTextBlobBuilder builder = new();
        builder.AddPositionedRun([.. run.Glyphs], font, [.. run.Positions]);
        using SKTextBlob? blob = builder.Build();
        using SKPaint paint = new() { Color = ink, IsAntialias = true };
        float lead = (frame.Width - run.Width) * align switch { TextAlignment.Right => 1f, TextAlignment.Center => 0.5f, _ => 0f };
        _ = Optional(blob).Iter(built => canvas.DrawText(built, frame.Left + lead, Origin(frame, run.Height).Y + run.Ascent, paint));
        return unit;
    }
}
