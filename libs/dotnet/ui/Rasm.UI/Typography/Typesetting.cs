using HarfBuzzSharp;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;
using Topten.RichTextKit;
using Topten.RichTextKit.Utils;

namespace Rasm.UI.Typography;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Typesetting {
    // --- [RESOLVE]
    private static readonly Style Plain = new();

    public static TextStyle Resolve(Shaping shaping, Cascade within, RoleStyle style) {
        TextFace face = within.Theme.Face.Combine(style.Look.Face);
        return new TextStyle(shaping.Faces.FromStyle(new Style {
            FontFamily = face.Family.IfNone(Plain.FontFamily),
            FontSize = within.Theme.FontSize * style.Metrics[Metric.FontSize],
            FontWeight = face.Weight.Map(static weight => (int)weight).IfNone(Plain.FontWeight),
            FontWidth = face.Width.IfNone(Plain.FontWidth),
            FontItalic = face.Italic.IfNone(Plain.FontItalic),
            LetterSpacing = face.LetterSpacing.IfNone(Plain.LetterSpacing),
            LineHeight = face.LineHeight.IfNone(Plain.LineHeight),
            Underline = face.Underline.IfNone(Plain.Underline),
            StrikeThrough = face.StrikeThrough.IfNone(Plain.StrikeThrough),
            FontVariant = face.Variant.IfNone(Plain.FontVariant),
            TextColor = within.Theme.Color(style.Text, style).Map(color => (SKColor)Styles.Legible(style, within.Ground, color, within.Theme.Contrast.Text)).IfNone(SKColors.Transparent),
            HaloColor = face.Halo.Bind(halo => within.Theme.Color(halo.Color, style)).Map(static halo => (SKColor)halo).IfNone(Plain.HaloColor),
            HaloWidth = face.Halo.Map(static halo => halo.Width).IfNone(Plain.HaloWidth),
            HaloBlur = face.Halo.Map(static halo => halo.Blur).IfNone(Plain.HaloBlur),
        }), face.Features.IfNone([]));
    }

    private static TextStyle Spanned(Shaping shaping, Cascade within, TextStyle face, Span span) =>
        span.Role.IsNone && span.Conditions.IsEmpty
            ? face
            : Resolve(shaping, within, Styles.Resolve(within, span.Role.IfNone(within.Enclosing.Role), span.Conditions.Union(within.Enclosing.Conditions)));

    // --- [SET]
    private sealed record Faced(string Text, TextStyle Face);

    private sealed record Widths(float Was, float Now);

    public static Typeset Set(Shaping shaping, Node.Text text, Cascade within, TextStyle face, float width) {
        Seq<Faced> spans = text.Spans.Map(span => new Faced(span.Text, Spanned(shaping, within, face, span))).Strict();
        return spans.Exists(static span => !span.Face.Features.IsEmpty) ? Featured(shaping, text, spans, width) : Measured(Lined(text, spans, width));
    }

    internal static float MinContent(Typeset typeset) => Narrowest(typeset.Lines) + float.Max(0f, typeset.Size.Width - typeset.Lines.MeasuredWidth);

    private static Typeset Measured(TextBlock lines) => new(lines, new SKSize(lines.MeasuredWidth, lines.MeasuredHeight), Clipped: false);

    private static float Advance(TextLine line) => toSeq(line.Runs).Filter(static run => run.RunKind is not FontRunKind.TrailingWhitespace).Fold(0f, static (sum, run) => sum + run.Width);

    private static TextBlock Lined(Node.Text text, Seq<Faced> spans, float measure) {
        TextBlock block = new() { MaxWidth = float.IsFinite(measure) ? measure : null, MaxLines = text.MaxLines.ToNullable(), Alignment = text.Alignment };
        _ = spans.Iter(span => block.AddText(span.Text, span.Face.Style));
        return block;
    }

    private static float Narrowest(TextBlock text) =>
        toSeq(text.WordBoundaryIndicies.Window(2)).Fold(0f, (widest, word) => float.Max(widest, MathF.Abs(Caret(text, word[1]) - Caret(text, word[0]))));

    private static float Caret(TextBlock text, int index) => text.GetCaretInfo(new CaretPosition(index)).CaretXCoord;

    // --- [SHAPING]
    private static Typeset Featured(Shaping shaping, Node.Text text, Seq<Faced> spans, float width) {
        HashMap<IStyle, Feature[]> features = HashMap<IStyle, Feature[]>().AddOrUpdateRange(spans.Filter(static span => !span.Face.Features.IsEmpty).Map(static span => (span.Face.Style, (Feature[])[.. span.Face.Features])));
        return Fitted(width, []);

        Typeset Fitted(float measure, Seq<int> prior) {
            TextBlock lines = Lined(text, spans, measure);
            Seq<Widths> shaped = Reshaped(shaping, lines, features, width);
            Seq<int> breaks = toSeq(lines.Lines).Map(static line => line.End);
            Seq<Widths> over = shaped.Filter(line => line.Now > width);
            return !over.IsEmpty && breaks != prior
                ? Fitted(over.Fold(measure, (least, line) => float.Min(least, line.Was * width / line.Now)), breaks)
                : new Typeset(lines, new SKSize(toSeq(lines.Lines).Fold(0f, static (widest, line) => float.Max(widest, Advance(line))), lines.MeasuredHeight), Clipped: !over.IsEmpty);
        }
    }

    private static Seq<Widths> Reshaped(Shaping shaping, TextBlock lines, HashMap<IStyle, Feature[]> features, float width) {
        Seq<(Seq<(FontRun Run, float Delta)> Runs, float Was, float Now)> shaped = toSeq(lines.Lines).Map(line => {
            Seq<(FontRun Run, float Delta)> runs = toSeq(line.Runs.Where(static run => run.RunKind is not FontRunKind.TrailingWhitespace).OrderBy(static run => run.XCoord))
                .Map(run => (Run: run, Delta: features.Find(run.Style).Filter(_ => run.RunKind is FontRunKind.Normal).Map(featured => {
                    float was = run.Width;
                    return Reshape(run, shaping.Shaped(run.Typeface), featured, shaping.Buffer) - was;
                }).IfNone(0f)))
                .Strict();
            float now = Advance(line);
            return (Runs: runs, Was: now - runs.Fold(0f, static (sum, held) => sum + held.Delta), Now: now);
        }).Strict();
        float lean = lines.Alignment switch {
            TextAlignment.Left => 0f,
            TextAlignment.Center => 0.5f,
            TextAlignment.Right => 1f,
            _ => lines.BaseDirection is TextDirection.RTL ? 1f : 0f,
        };
        float grown = (float.IsFinite(width) ? width : shaped.Fold(0f, static (widest, line) => float.Max(widest, line.Now))) - (lines.MaxWidth ?? lines.MeasuredWidth);
        _ = shaped.Iter(line => line.Runs.Fold(lean * (grown - (line.Now - line.Was)), static (shift, held) => {
            if (shift.CompareTo(0f) is not 0) {
                held.Run.XCoord += shift;
                held.Run.MoveGlyphs(shift, 0f);
            }
            return shift + held.Delta;
        }));
        return shaped.Map(static line => new Widths(line.Was, line.Now));
    }

    private static float Reshape(FontRun run, Font font, Feature[] features, HarfBuzzSharp.Buffer buffer) {
        buffer.ClearContents();
        buffer.AddUtf32(run.CodePoints.AsSpan());
        buffer.Direction = run.Direction is TextDirection.RTL ? Direction.RightToLeft : Direction.LeftToRight;
        buffer.GuessSegmentProperties();
        font.Shape(buffer, features);
        float scale = run.Style.FontSize / run.Typeface.UnitsPerEm;
        ReadOnlySpan<GlyphInfo> infos = buffer.GetGlyphInfoSpan();
        ReadOnlySpan<GlyphPosition> at = buffer.GetGlyphPositionSpan();
        ushort[] glyphs = new ushort[run.Start + infos.Length];
        SKPoint[] points = new SKPoint[run.Start + infos.Length];
        SKPoint origin = new(run.XCoord, run.Line.YCoord + run.Line.BaseLine);
        float pen = 0f;
        for (int index = 0; index < infos.Length; index++) {
            glyphs[run.Start + index] = (ushort)infos[index].Codepoint;
            points[run.Start + index] = new SKPoint(origin.X + pen + (at[index].XOffset * scale), origin.Y - (at[index].YOffset * scale));
            pen += at[index].XAdvance * scale;
        }
        run.Glyphs = new Slice<ushort>(glyphs, run.Start, infos.Length);
        run.GlyphPositions = new Slice<SKPoint>(points, run.Start, infos.Length);
        run.Width = pen;
        return pen;
    }

    // --- [PAINT]
    internal static void Paint(SKCanvas canvas, Laid.Text laid, SKColor ink, TextPaintOptions raster) {
        if (ink == laid.TextStyle.Style.TextColor) {
            laid.Typeset.Lines.Paint(canvas, laid.Origin, raster);
            return;
        }
        (SKSize size, SKRect overhang) = (laid.Typeset.Size, laid.Typeset.Lines.MeasuredOverhang);
        using SKColorFilter filter = SKColorFilter.CreateBlendMode(ink, SKBlendMode.SrcIn);
        using SKPaint wash = new() { ColorFilter = filter };
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        _ = canvas.SaveLayer(new SKRect(laid.Origin.X - overhang.Left, laid.Origin.Y - overhang.Top, laid.Origin.X + size.Width + overhang.Right, laid.Origin.Y + size.Height + overhang.Bottom), wash);
        laid.Typeset.Lines.Paint(canvas, laid.Origin, raster);
    }
}
