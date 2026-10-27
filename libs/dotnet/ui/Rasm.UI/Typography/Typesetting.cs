using System.Globalization;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using ICU4N;
using ICU4N.Globalization;
using ICU4N.Text;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;
using Topten.RichTextKit;
using Topten.RichTextKit.Utils;

namespace Rasm.UI.Typography;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Typesetting {
    // --- [RESOLVE]
    private static readonly Style Plain = new();
    private static readonly HashMap<FontVariant, Feature> Variants = HashMap((FontVariant.SuperScript, new Feature(Tag.Parse("sups"))), (FontVariant.SubScript, new Feature(Tag.Parse("subs"))));

    internal static TextStyle Resolve(Shaping shaping, Cascade within, RoleStyle style) {
        TextFace face = within.Theme.Face.Combine(style.Look.Face);
        TextStyle resolved = new() {
            FontFamily = face.Family.IfNone(Plain.FontFamily),
            FontSize = within.Theme.FontSize * style.Metrics[Metric.FontSize],
            FontWeight = face.Weight.Map(static weight => (int)weight).IfNone(Plain.FontWeight),
            FontWidth = face.Width.IfNone(Plain.FontWidth),
            FontItalic = face.Italic.IfNone(Plain.FontItalic),
            Underline = face.Underline.IfNone(Plain.Underline),
            StrikeThrough = face.StrikeThrough.IfNone(Plain.StrikeThrough),
            LineHeight = face.LineHeight.IfNone(Plain.LineHeight),
            TextColor = within.Theme.Color(style.Text, style).Map(color => (SKColor)Styles.Legible(style, within.Background, color, within.Theme.Contrast.Text)).IfNone(SKColors.Transparent),
            BackgroundColor = face.Background.Bind(color => within.Theme.Color(color, style)).Map(static color => (SKColor)color).IfNone(Plain.BackgroundColor),
            HaloColor = face.Halo.Bind(halo => within.Theme.Color(halo.Color, style)).Map(static halo => (SKColor)halo).IfNone(Plain.HaloColor),
            HaloWidth = face.Halo.Map(static halo => halo.Width).IfNone(Plain.HaloWidth),
            HaloBlur = face.Halo.Map(static halo => halo.Blur).IfNone(Plain.HaloBlur),
            LetterSpacing = face.LetterSpacing.IfNone(Plain.LetterSpacing),
            TextDirection = face.Direction.IfNone(Plain.TextDirection),
            ReplacementCharacter = face.Replacement.IfNone(Plain.ReplacementCharacter),
            Features = face.Features.IfNone([]) + face.Variant.Bind(Variants.Find).ToSeq(),
            Variations = face.Variations,
            Palette = toSeq(face.Palette.IfNone([]).Select(static (ink, index) => (Ink: ink, Index: index)))
                .Choose(held => within.Theme.Color(held.Ink, style).Map(color => new SKFontPaletteOverride { Index = (ushort)held.Index, Color = (uint)(SKColor)color })),
            Stroked = face.Stroked,
        };
        return resolved with { Cell = Celled(shaping, resolved, face.Mono) };
    }

    private static Option<float> Celled(Shaping shaping, TextStyle style, bool mono) =>
        Some(shaping.Mapper.TypefaceFromStyle(style, ignoreFontVariants: false)).Filter(primary => mono || primary.IsFixedPitch).Bind(primary => shaping.HarfBuzzFont(primary) switch {
            var font => font.TryGetGlyph('0', out uint zero) ? Some(font.GetHorizontalGlyphAdvance(zero) * style.FontSize / primary.UnitsPerEm) : None,
        });

    private static TextStyle SpanStyle(Shaping shaping, Cascade within, TextStyle face, Span span) =>
        (span.Role.IsNone && span.Conditions.IsEmpty
            ? face
            : Resolve(shaping, within, Styles.Resolve(within, span.Role.IfNone(within.Enclosing.Role), span.Conditions.Union(within.Enclosing.Conditions)))) with { Language = span.Language, };

    // --- [PIECES]
    private sealed record StyledRun(string Text, TextStyle Style);

    private sealed record Segment(int Start, int End);

    private const int Presentation = 0xFE0F;

    private static readonly string[] Emoji = [$"und-{UScript.GetShortName(UScript.SymbolsEmoji)}"];

    private static Seq<StyledRun> Pieces(Shaping shaping, string text, TextStyle style) {
        using SKFont primary = new(shaping.Mapper.TypefaceFromStyle(style, ignoreFontVariants: false));
        shaping.Graphemes.SetText(text);
        return toSeq(Segments(shaping.Graphemes)
            .GroupAdjacent(segment => Matched(text[segment.Start..segment.End], style, primary), (face, segments) => new StyledRun(text[segments[0].Start..segments[^1].End], face.Map(matched => style with { Matched = matched }).IfNone(style)))).Strict();
    }

    private static Option<SKTypeface> Matched(string grapheme, TextStyle style, SKFont primary) {
        Seq<int> points = toSeq(grapheme.EnumerateRunes()).Map(static rune => rune.Value).Strict();
        Option<string[]> tags = points.Exists(static point => point is Presentation || UChar.HasBinaryProperty(point, UProperty.Emoji_Presentation))
            ? Some(Emoji)
            : style.Language.Filter(_ => !primary.ContainsGlyphs([.. points])).Map(static culture => new[] { culture.Name });
        return from bcp47 in tags
               from last in points.Last
               from face in Optional(SKFontManager.Default.MatchCharacter(style.FontFamily, style.FontWeight, (int)style.FontWidth, style.FontItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright, bcp47, last))
               select face;
    }

    private static Seq<StyledRun> Kept(StyledText styled, int from, int length) => length > 0 ? Pieced(styled.Extract(from, length)) : [];

    private static Seq<StyledRun> Pieced(StyledText styled) => toSeq(styled.StyleRuns).Map(static run => new StyledRun(Utf32Utils.FromUtf32(run.CodePoints), (TextStyle)run.Style));

    private static Seq<Segment> Segments(BreakIterator breaks) =>
        toSeq(SuperEnumerable.Generate(breaks.First(), _ => breaks.Next()).TakeWhile(static stop => stop != BreakIterator.Done).Lag(1, static (end, start) => new Segment(start, end)).Skip(1)).Strict();

    // --- [SET]
    private const string Ellipsis = "…";

    private sealed record Widths(float Was, float Now);

    internal static Typeset Set(Shaping shaping, Node.Text text, Cascade within, TextStyle face, float width) {
        Seq<StyledRun> pieces = text.Spans.Bind(span => Pieces(shaping, span.Text, SpanStyle(shaping, within, face, span))).Strict();
        Option<Seq<StyledRun>> cut = text.MaxLines.Filter(static lines => lines is 1).Bind(_ => Cut(shaping, text, pieces, face, width));
        return Fitted(shaping, text, cut.IfNone(pieces), face, width, cut.IsSome);
    }

    internal static float MinContent(Typeset typeset) {
        TextBlock lines = typeset.Lines;
        string text = lines.ToString();
        BreakIterator breaks = BreakIterator.GetLineInstance(toSeq(lines.StyleRuns).Head.Bind(static run => ((TextStyle)run.Style).Language).IfNone(CultureInfo.InvariantCulture));
        breaks.SetText(text);
        return Segments(breaks).Fold(0f, (widest, segment) => float.Max(widest, MathF.Abs(
            Caret(lines, lines.CharacterToCodePointIndex(segment.Start + text.AsSpan(segment.Start, segment.End - segment.Start).TrimEnd().Length)) - Caret(lines, lines.CharacterToCodePointIndex(segment.Start)))));
    }

    private static Typeset Fitted(Shaping shaping, Node.Text text, Seq<StyledRun> pieces, TextStyle face, float width, bool cut) {
        return Fit(width, []);

        Typeset Fit(float measure, Seq<int> prior) {
            TextBlock lines = Block(shaping, text, pieces, measure);
            Seq<Widths> shaped = Reshaped(shaping, lines, width);
            Seq<int> breaks = toSeq(lines.Lines).Map(static line => line.End);
            Seq<Widths> over = shaped.Filter(line => line.Now > width);
            return !over.IsEmpty && breaks != prior
                ? Fit(over.Fold(measure, (least, line) => float.Min(least, line.Was * width / line.Now)), breaks)
                : new Typeset(lines, cut || !over.IsEmpty, TintedBlocks(shaping, text, lines, face.TextColor, width));
        }
    }

    private static TextBlock Block(Shaping shaping, Node.Text text, Seq<StyledRun> pieces, float measure) {
        TextBlock block = new() {
            FontMapper = shaping.Mapper,
            MaxWidth = float.IsFinite(measure) ? measure : null,
            MaxLines = text.MaxLines.ToNullable(),
            MaxHeight = text.MaxHeight.ToNullable(),
            Alignment = text.Alignment,
        };
        foreach (StyledRun piece in pieces)
            block.AddText(piece.Text, piece.Style);
        return block;
    }

    private static Option<Seq<StyledRun>> Cut(Shaping shaping, Node.Text text, Seq<StyledRun> pieces, TextStyle face, float width) {
        Node.Text free = text with { MaxLines = None, MaxHeight = None };
        TextBlock whole = Block(shaping, free, pieces, float.PositiveInfinity);
        _ = Reshaped(shaping, whole, float.PositiveInfinity);
        if (Typeset.Widest(whole) <= width)
            return None;
        TextBlock mark = Block(shaping, free, [new StyledRun(Ellipsis, face)], float.PositiveInfinity);
        _ = Reshaped(shaping, mark, float.PositiveInfinity);
        Seq<int> stops = toSeq(whole.CaretIndicies);
        (float origin, float finish, float budget) = (Caret(whole, 0), Caret(whole, whole.Length), float.Max(0f, width - Typeset.Widest(mark)));
        int head = stops.Fold(0, (kept, stop) => Caret(whole, stop) - origin <= budget * (float)text.Truncation.Head ? stop : kept);
        float rest = budget - (Caret(whole, head) - origin);
        int tail = stops.Rev().Fold(whole.Length, (kept, stop) => stop >= head && finish - Caret(whole, stop) <= rest ? stop : kept);
        return Kept(whole, 0, head) + Seq(new StyledRun(Ellipsis, face)) + Kept(whole, tail, whole.Length - tail);
    }

    private static Func<SKColor, TextBlock> TintedBlocks(Shaping shaping, Node.Text text, TextBlock lines, SKColor resting, float width) =>
        memo<SKColor, TextBlock>(ink => {
            TextBlock tinted = Block(shaping, text, Pieced(lines).Map(piece => piece.Style.TextColor == resting ? piece with { Style = piece.Style with { TextColor = ink } } : piece), lines.MaxWidth ?? float.PositiveInfinity);
            _ = Reshaped(shaping, tinted, width);
            return tinted;
        });

    private static float Caret(TextBlock lines, int index) => lines.GetCaretInfo(new CaretPosition(index)).CaretXCoord;

    // --- [RESHAPE]
    private static Seq<Widths> Reshaped(Shaping shaping, TextBlock lines, float width) {
        float box = lines.MaxWidth ?? lines.MeasuredWidth;
        Seq<TextLine> layout = toSeq(lines.Lines).Strict();
        float lean = layout.Map(line => (Left: Inked(line).Fold(float.PositiveInfinity, static (least, run) => float.Min(least, run.XCoord)), Slack: box - line.Width))
            .Filter(static held => float.IsFinite(held.Left))
            .MaxBy(static held => held.Slack) is { Slack: > 0f } widest ? widest.Left / widest.Slack : 0f;
        Seq<(TextLine Line, Seq<(FontRun Run, float Delta)> Runs)> shaped = layout
            .Map(line => (Line: line, Runs: toSeq(line.Runs.OrderBy(static run => run.XCoord)).Map(run => (Run: run, Delta: Delta(shaping, lines, run, lean))).Strict()))
            .Strict();
        Seq<Widths> measured = shaped.Map(static held => Typeset.Advance(held.Line) switch {
            var now => new Widths(now - held.Runs.Fold(0f, static (sum, run) => sum + run.Delta), now),
        }).Strict();
        float target = float.IsFinite(width) ? width : measured.Fold(0f, static (widest, line) => float.Max(widest, line.Now));
        _ = shaped.Zip(measured).Iter(pair => pair.First.Runs.Fold(lean * (target - pair.Second.Now - (box - pair.First.Line.Width)), static (shift, held) => {
            held.Run.XCoord += shift;
            held.Run.MoveGlyphs(shift, 0f);
            return shift + held.Delta;
        }));
        return measured;
    }

    private static Seq<FontRun> Inked(TextLine line) => toSeq(line.Runs).Filter(static run => run.RunKind is not FontRunKind.TrailingWhitespace);

    private static float Delta(Shaping shaping, TextBlock lines, FontRun run, float lean) =>
        run is { RunKind: not FontRunKind.TrailingWhitespace, Style: TextStyle { ReplacementCharacter: '\0' } style }
        && (run.Typeface.VariationDesignPositionCount > 0 || !style.Features.IsEmpty || style.Language.IsSome || style.Cell.IsSome)
            ? Reshape(lines, run, style, shaping.HarfBuzzFont(run.Typeface), shaping.Buffer, lean)
            : 0f;

    private static float Reshape(TextBlock lines, FontRun run, TextStyle style, Font font, HarfBuzzSharp.Buffer buffer, float lean) {
        Slice<int> context = run.RunKind is FontRunKind.Ellipsis ? run.CodePoints : lines.CodePoints.AsSlice();
        buffer.Reset();
        buffer.Direction = run.Direction is TextDirection.RTL ? Direction.RightToLeft : Direction.LeftToRight;
        _ = style.Language.Iter(culture => buffer.Language = new Language(culture));
        buffer.AddUtf32(context.AsSpan(), run.Start - context.Start, run.Length);
        buffer.GuessSegmentProperties();
        font.Shape(buffer, [.. style.Features]);
        ReadOnlySpan<GlyphInfo> infos = buffer.GetGlyphInfoSpan();
        ReadOnlySpan<GlyphPosition> at = buffer.GetGlyphPositionSpan();
        (float scale, bool rtl) = (style.FontSize / run.Typeface.UnitsPerEm, run.Direction is TextDirection.RTL);
        SKPoint origin = new(run.XCoord + (style.LetterSpacing * lean), run.Line.YCoord + run.Line.BaseLine);
        ushort[] glyphs = new ushort[run.Start + infos.Length];
        SKPoint[] points = new SKPoint[infos.Length];
        int[] clusters = new int[infos.Length];
        float[] carets = new float[run.Length];
        (float pen, float opened, int first) = (0f, 0f, 0);
        for (int index = 0; index < infos.Length; index++) {
            (int cluster, bool opens) = ((int)infos[index].Cluster + context.Start, index is 0 || infos[index].Cluster != infos[index - 1].Cluster);
            (opened, first) = opens ? (pen, index) : (opened, first);
            if (!rtl && (opens || pen < carets[cluster - run.Start]))
                carets[cluster - run.Start] = pen;
            glyphs[run.Start + index] = (ushort)infos[index].Codepoint;
            clusters[index] = cluster;
            points[index] = new SKPoint(origin.X + pen + (at[index].XOffset * scale), origin.Y - (at[index].YOffset * scale));
            pen += at[index].XAdvance * scale;
            if (index + 1 == infos.Length || infos[index].Cluster != infos[index + 1].Cluster) {
                float advance = pen + style.LetterSpacing - opened;
                float snapped = Snapped(style.Cell, advance);
                points[first].X += (snapped - advance) / 2f;
                pen = opened + snapped;
            }
            if (rtl && (opens || pen > carets[cluster - run.Start]))
                carets[cluster - run.Start] = pen;
        }
        if (rtl)
            carets[0] = pen;
        for (int index = rtl ? run.Length - 2 : 1; index >= 0 && index < run.Length; index += rtl ? -1 : 1)
            carets[index] = carets[index] is 0f ? carets[index + (rtl ? 1 : -1)] : carets[index];
        run.Glyphs = new Slice<ushort>(glyphs, run.Start, infos.Length);
        run.GlyphPositions = new Slice<SKPoint>(points);
        run.Clusters = new Slice<int>(clusters);
        run.RelativeCodePointXCoords = new Slice<float>(carets);
        float delta = pen - run.Width;
        run.Width = pen;
        return delta;
    }

    private static float Snapped(Option<float> cell, float advance) =>
        cell.Map(width => advance > 0f ? float.Max(1f, MathF.Round(advance / width, MidpointRounding.ToEven)) * width : advance).IfNone(advance);

    // --- [OUTLINES]
    internal static SKPath Outline(LayoutNode.Text layout) {
        using SKPathBuilder builder = new();
        foreach (FontRun run in Glyphed(layout.Typeset.Lines)) {
            using SKFont font = new(run.Typeface, run.Style.FontSize);
            using SKPath glyphs = font.GetTextPath(MemoryMarshal.AsBytes(run.Glyphs.AsSpan()), SKTextEncoding.GlyphId, run.GlyphPositions.AsSpan());
            builder.AddPath(glyphs, layout.Origin.X, layout.Origin.Y);
        }
        return builder.Detach();
    }

    internal static Option<SKTextBlob> Along(Typeset set, SKPath path, float offset) {
        using SKTextBlobBuilder builder = new();
        using SKPathMeasure measure = new(path);
        float baseline = set.Baseline.IfNone(0f);
        foreach (FontRun run in Glyphed(set.Lines)) {
            ReadOnlySpan<SKPoint> at = run.GlyphPositions.AsSpan();
            float[] widths = new float[at.Length];
            SKPoint[] offsets = new SKPoint[at.Length];
            for (int index = 0; index < at.Length; index++) {
                widths[index] = (index + 1 < at.Length ? at[index + 1].X : run.XCoord + run.Width) - at[index].X;
                offsets[index] = new SKPoint(at[index].X - at[0].X, at[index].Y - baseline);
            }
            using SKPath along = measure.GetSegment(offset + at[0].X, measure.Length, startWithMoveTo: true);
            using SKFont font = new(run.Typeface, run.Style.FontSize);
            builder.AddPathPositionedRun(run.Glyphs.AsSpan(), font, widths, offsets, along);
        }
        return Optional(builder.Build());
    }

    private static Seq<FontRun> Glyphed(TextBlock lines) => toSeq(lines.Lines).Bind(Inked).Filter(static run => run.Glyphs.Length > 0);

    // --- [PAINT]
    internal static void Paint(SKCanvas canvas, LayoutNode.Text layout, SKColor ink, TextPaintOptions raster) =>
        _ = layout.TextStyle.Stroked.Match(
            Some: width => {
                using SKPath outline = Outline(layout);
                using SKPaint pen = new() { Color = ink, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = width };
                canvas.DrawPath(outline, pen);
            },
            None: () => (ink == layout.TextStyle.TextColor ? layout.Typeset.Lines : layout.Typeset.Tinted(ink)).Paint(canvas, layout.Origin, raster));
}
