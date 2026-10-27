using System.Globalization;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using ICU4N;
using ICU4N.Globalization;
using ICU4N.Text;
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
    private static readonly HashMap<FontVariant, Variant> Variants = HashMap(
        (FontVariant.SuperScript, new Variant(new Feature(Tag.Parse("sups")), OpenTypeMetricsTag.SuperScriptEmYSize, OpenTypeMetricsTag.SuperScriptEmYOffset, 1f)),
        (FontVariant.SubScript, new Variant(new Feature(Tag.Parse("subs")), OpenTypeMetricsTag.SubScriptEmYSize, OpenTypeMetricsTag.SubScriptEmYOffset, -1f)));

    internal static TextStyle Resolve(Shaping shaping, ColorCache colorCache, Cascade cascade, RoleStyle style) {
        TextFace face = cascade.Theme.Face.Combine(style.Look.Face);
        TextStyle resolved = new() {
            FontFamily = face.Family.IfNone(Plain.FontFamily),
            FontSize = cascade.Theme.FontSize * style.Metrics[Metric.FontSize],
            FontWeight = face.Weight.Map(static weight => (int)weight).IfNone(Plain.FontWeight),
            FontWidth = face.Width.IfNone(Plain.FontWidth),
            FontItalic = face.Italic.IfNone(Plain.FontItalic),
            Underline = face.Underline.IfNone(Plain.Underline),
            StrikeThrough = face.StrikeThrough.IfNone(Plain.StrikeThrough),
            LineHeight = face.LineHeight.IfNone(Plain.LineHeight),
            TextColor = cascade.Theme.Color(style.Text, style).Map(color => (SKColor)Styles.Legible(colorCache, style, cascade.Background, color, cascade.Theme.Contrast.Text)).IfNone(SKColors.Transparent),
            BackgroundColor = face.Background.Bind(color => cascade.Theme.Color(color, style)).Map(static color => (SKColor)color).IfNone(Plain.BackgroundColor),
            HaloColor = face.Halo.Bind(halo => cascade.Theme.Color(halo.Color, style)).Map(static halo => (SKColor)halo).IfNone(Plain.HaloColor),
            HaloWidth = face.Halo.Map(static halo => halo.Width).IfNone(Plain.HaloWidth),
            HaloBlur = face.Halo.Map(static halo => halo.Blur).IfNone(Plain.HaloBlur),
            LetterSpacing = face.LetterSpacing.IfNone(Plain.LetterSpacing),
            TextDirection = face.Direction.IfNone(Plain.TextDirection),
            ReplacementCharacter = face.Replacement.IfNone(Plain.ReplacementCharacter),
            Features = face.Features.IfNone([]),
            Variant = face.Variant.Bind(Variants.Find),
            Variations = face.Variations,
            Palette = toSeq(face.Palette.IfNone([]).Select(static (entry, index) => (Value: entry, Index: index)))
                .Choose(held => cascade.Theme.Color(held.Value, style).Map(color => new SKFontPaletteOverride { Index = (ushort)held.Index, Color = (uint)(SKColor)color })),
            Dark = cascade.Theme.Conditions.Contains(Condition.Dark),
            Stroked = face.Stroked,
        };
        return resolved with { Cell = Cell(shaping, resolved, face.Mono.Exists(static mono => mono)) };
    }

    private static Option<float> Cell(Shaping shaping, TextStyle style, bool mono) =>
        Some(shaping.Mapper.TypefaceFromStyle(style, ignoreFontVariants: false)).Filter(primary => mono || primary.IsFixedPitch).Bind(primary => shaping.HarfBuzzFont(primary) switch {
            var font => font.TryGetGlyph('0', out uint zero) ? Some(font.GetHorizontalGlyphAdvance(zero) * style.FontSize / primary.UnitsPerEm) : None,
        });

    private static TextStyle SpanStyle(Shaping shaping, ColorCache colorCache, Cascade cascade, TextStyle face, Span span) =>
        (span.Role.IsNone && span.Conditions.IsEmpty
            ? face
            : Resolve(shaping, colorCache, cascade, Styles.Resolve(cascade, span.Role.IfNone(cascade.Enclosing.Role), span.Conditions.Union(cascade.Enclosing.Conditions)))) with { Language = span.Language, Part = span.Part, };

    private static Option<TextSelection> Selected(Node.Text text, Cascade cascade) =>
        from range in text.Selected
        let style = Styles.Resolve(cascade, Role.Selection, [])
        from fill in style.Solid
        from color in cascade.Theme.Color(fill, style)
        select new TextSelection(range, (SKColor)color);

    // --- [RUNS]
    private sealed record StyledRun(string Text, TextStyle Style);

    private sealed record Segment(int Start, int End);

    private const int TextPresentation = 0xFE0E;
    private const int EmojiPresentation = 0xFE0F;

    private static readonly string[] Emoji = [$"und-{UScript.GetShortName(UScript.SymbolsEmoji)}"];

    private static Seq<StyledRun> FaceRuns(Shaping shaping, string text, TextStyle style, CultureInfo culture) {
        Font primary = shaping.HarfBuzzFont(shaping.Mapper.TypefaceFromStyle(style, ignoreFontVariants: false));
        shaping.Graphemes.SetText(text);
        return toSeq(Segments(shaping.Graphemes)
            .GroupAdjacent(segment => Fallback(text[segment.Start..segment.End], style, primary, culture), (face, segments) => new StyledRun(text[segments[0].Start..segments[^1].End], face.Map(fallback => style with { Fallback = fallback }).IfNone(style)))).Strict();
    }

    private static Option<SKTypeface> Fallback(string grapheme, TextStyle style, Font primary, CultureInfo culture) {
        Seq<int> points = toSeq(grapheme.EnumerateRunes()).Map(static rune => rune.Value).Strict();
        Option<(string[] Tags, int Point)> wanted = (points.Exists(static point => point is TextPresentation) ? None : Presenting(points)).Map(static point => (Tags: Emoji, Point: point))
            | (points.ForAll(point => primary.TryGetGlyph(point, out uint _)) ? None : points.Head.Map(point => (Tags: new[] { style.Language.IfNone(culture).Name }, Point: point)));
        return wanted.Bind(held => Optional(SKFontManager.Default.MatchCharacter(style.FontFamily, style.FontWeight, (int)style.FontWidth, style.FontItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright, held.Tags, held.Point)));
    }

    private static Option<int> Presenting(Seq<int> points) =>
        points.Find(static point => UChar.HasBinaryProperty(point, UProperty.Emoji_Presentation) && !UChar.HasBinaryProperty(point, UProperty.Emoji_Component))
        | points.Find(static point => point is EmojiPresentation)
        | points.Find(static point => UChar.HasBinaryProperty(point, UProperty.Emoji_Modifier))
        | points.Find(static point => UChar.HasBinaryProperty(point, UProperty.Emoji_Presentation));

    private static Seq<StyledRun> Extracted(StyledText styled, int from, int length) => length > 0 ? StyleRuns(styled.Extract(from, length)) : [];

    private static Seq<StyledRun> StyleRuns(StyledText styled) => toSeq(styled.StyleRuns).Map(static run => new StyledRun(Utf32Utils.FromUtf32(run.CodePoints), (TextStyle)run.Style));

    private static Seq<Segment> Segments(BreakIterator breaks) =>
        toSeq(SuperEnumerable.Generate(breaks.First(), _ => breaks.Next()).TakeWhile(static stop => stop != BreakIterator.Done).Lag(1, static (end, start) => new Segment(start, end)).Skip(1)).Strict();

    // --- [SET]
    private const string Ellipsis = "…";

    private sealed record LineWidth(float Laid, float Shaped);

    internal static Typeset Set(Shaping shaping, ColorCache colorCache, Node.Text text, Cascade cascade, TextStyle style, float width, CultureInfo culture) {
        Setting setting = new(text, style, width, culture, Selected(text, cascade));
        Seq<StyledRun> runs = text.Spans.Bind(span => FaceRuns(shaping, span.Text, SpanStyle(shaping, colorCache, cascade, style, span), culture)).Strict();
        Option<Seq<StyledRun>> cut = text.MaxLines.Filter(static lines => lines is 1).Bind(_ => Cut(shaping, setting, runs));
        return Fitted(shaping, setting, cut.IfNone(runs), cut.IsSome);
    }

    internal static float MinContent(Shaping shaping, Typeset set) {
        TextBlock lines = set.Lines;
        string text = lines.ToString();
        BreakIterator breaks = shaping.LineBreaks(set.Setting.Culture);
        breaks.SetText(text);
        return Segments(breaks).Fold(0f, (widest, segment) => float.Max(widest, MathF.Abs(
            Caret(lines, lines.CharacterToCodePointIndex(segment.Start + text.AsSpan(segment.Start, segment.End - segment.Start).TrimEnd().Length)) - Caret(lines, lines.CharacterToCodePointIndex(segment.Start)))));
    }

    internal static TextBlock Tinted(Shaping shaping, Typeset set, SKColor tint) {
        TextBlock tinted = Block(shaping, set.Setting, StyleRuns(set.Lines).Map(run => run.Style.TextColor == set.Setting.Style.TextColor ? run with { Style = run.Style with { TextColor = tint } } : run), set.Lines.MaxWidth ?? float.PositiveInfinity);
        _ = Reshaped(shaping, tinted, set.Setting.Culture, set.Setting.Width);
        return tinted;
    }

    internal static Option<float> CapHeight(Shaping shaping, FontRun run) =>
        Position(shaping.HarfBuzzFont(run.Typeface), OpenTypeMetricsTag.CapHeight).Map(units => units * run.Style.FontSize / run.Typeface.UnitsPerEm);

    private static Typeset Fitted(Shaping shaping, Setting setting, Seq<StyledRun> runs, bool cut) {
        return Fit(setting.Width, []);

        Typeset Fit(float measure, Seq<int> prior) {
            TextBlock lines = Block(shaping, setting, runs, measure);
            Seq<LineWidth> shaped = Reshaped(shaping, lines, setting.Culture, setting.Width);
            Seq<int> breaks = toSeq(lines.Lines).Map(static line => line.End);
            Seq<LineWidth> over = shaped.Filter(line => line.Shaped > setting.Width);
            return !over.IsEmpty && breaks != prior
                ? Fit(over.Fold(measure, (least, line) => float.Min(least, line.Laid * setting.Width / line.Shaped)), breaks)
                : new Typeset(shaping, setting, lines, cut || !over.IsEmpty);
        }
    }

    private static TextBlock Block(Shaping shaping, Setting setting, Seq<StyledRun> runs, float measure) {
        TextBlock block = new() {
            FontMapper = shaping.Mapper,
            MaxWidth = float.IsFinite(measure) ? measure : null,
            MaxLines = setting.Text.MaxLines.ToNullable(),
            MaxHeight = setting.Text.MaxHeight.ToNullable(),
            Alignment = setting.Text.Alignment,
            BaseDirection = setting.Culture.TextInfo.IsRightToLeft ? TextDirection.RTL : TextDirection.LTR,
        };
        foreach (StyledRun run in runs)
            block.AddText(run.Text, run.Style);
        return block;
    }

    private static Option<Seq<StyledRun>> Cut(Shaping shaping, Setting setting, Seq<StyledRun> runs) {
        Setting free = setting with { Text = setting.Text with { MaxLines = None, MaxHeight = None } };
        TextBlock whole = Block(shaping, free, runs, float.PositiveInfinity);
        _ = Reshaped(shaping, whole, setting.Culture, float.PositiveInfinity);
        if (Widest(whole) <= setting.Width)
            return None;
        TextBlock mark = Block(shaping, free, [new StyledRun(Ellipsis, setting.Style)], float.PositiveInfinity);
        _ = Reshaped(shaping, mark, setting.Culture, float.PositiveInfinity);
        Seq<int> stops = toSeq(whole.CaretIndicies);
        (float origin, float finish, float budget) = (Caret(whole, 0), Caret(whole, whole.Length), float.Max(0f, setting.Width - Widest(mark)));
        int head = stops.Fold(0, (kept, stop) => MathF.Abs(Caret(whole, stop) - origin) <= budget * (float)setting.Text.Truncation.Head ? stop : kept);
        float rest = budget - MathF.Abs(Caret(whole, head) - origin);
        int tail = stops.Rev().Fold(whole.Length, (kept, stop) => stop >= head && MathF.Abs(finish - Caret(whole, stop)) <= rest ? stop : kept);
        return Extracted(whole, 0, head) + Seq(new StyledRun(Ellipsis, setting.Style)) + Extracted(whole, tail, whole.Length - tail);
    }

    private static float Caret(TextBlock lines, int index) => lines.GetCaretInfo(new CaretPosition(index)).CaretXCoord;

    // --- [RESHAPE]
    private sealed record Paragraph(TextAlignment Alignment, TextDirection Direction);

    private const int ParagraphSeparator = 0x2029;

    private static readonly Func<Paragraph, float> LeadingSlack = memo<Paragraph, float>(static paragraph => {
        TextBlock probe = new() { MaxWidth = 100f, Alignment = paragraph.Alignment, BaseDirection = paragraph.Direction };
        probe.AddText("0", Plain);
        SKRect padding = probe.MeasuredPadding;
        return padding.Left / (padding.Left + padding.Right);
    });

    internal static float Widest(TextBlock lines) => toSeq(lines.Lines).Map(Advance).Fold(0f, float.Max);

    private static float Advance(TextLine line) => VisibleRuns(line).Fold(0f, static (sum, run) => sum + run.Width);

    private static Seq<FontRun> VisibleRuns(TextLine line) => toSeq(line.Runs).Filter(static run => run.RunKind is not FontRunKind.TrailingWhitespace);

    private static Seq<LineWidth> Reshaped(Shaping shaping, TextBlock lines, CultureInfo culture, float width) {
        float box = lines.MaxWidth ?? lines.MeasuredWidth;
        float lean = LeadingSlack(new Paragraph(lines.Alignment, lines.BaseDirection));
        Seq<(TextLine Line, Seq<(FontRun Run, float Delta)> Runs)> shaped = toSeq(lines.Lines)
            .Map(line => (Line: line, Runs: toSeq(line.Runs.OrderBy(static run => run.XCoord)).Map(run => (Run: run, Delta: Delta(shaping, lines, run, culture, lean))).Strict()))
            .Strict();
        Seq<LineWidth> measured = shaped.Map(static held => Advance(held.Line) switch {
            var now => new LineWidth(now - held.Runs.Fold(0f, static (sum, run) => sum + run.Delta), now),
        }).Strict();
        float target = float.IsFinite(width) ? width : measured.Map(static line => line.Shaped).Fold(0f, float.Max);
        _ = shaped.Zip(measured).Iter(pair => pair.First.Runs.Fold(lean * (target - pair.Second.Shaped - (box - pair.First.Line.Width)), static (shift, held) => {
            held.Run.XCoord += shift;
            held.Run.MoveGlyphs(shift, 0f);
            return shift + held.Delta;
        }));
        return measured;
    }

    private static float Delta(Shaping shaping, TextBlock lines, FontRun run, CultureInfo culture, float lean) =>
        run is { RunKind: not FontRunKind.TrailingWhitespace, Style: TextStyle { ReplacementCharacter: '\0' } style }
        && (run.Typeface.VariationDesignPositionCount > 0 || !style.Features.IsEmpty || style.Variant.IsSome || style.Language.IsSome || style.Cell.IsSome)
            ? Reshape(lines, run, style, shaping.HarfBuzzFont(run.Typeface), shaping.Buffer, culture, lean)
            : 0f;

    private static float Reshape(TextBlock lines, FontRun run, TextStyle style, Font font, HarfBuzzSharp.Buffer buffer, CultureInfo culture, float lean) {
        Slice<int> context = run.RunKind is FontRunKind.Ellipsis ? run.CodePoints : lines.CodePoints.AsSlice();
        using Language language = new(style.Language.IfNone(culture).Name);
        Option<uint[]> unfeatured = style.Variant.Map(_ => GlyphIds(Shape(font, buffer, run, context, language, [.. style.Features])));
        ReadOnlySpan<GlyphInfo> infos = Shape(font, buffer, run, context, language, [.. style.Features + style.Variant.Map(static variant => variant.Feature).ToSeq()]);
        ReadOnlySpan<GlyphPosition> at = buffer.GetGlyphPositionSpan();
        ReadOnlySpan<int> text = context.AsSpan();
        (float scale, bool rtl) = (style.FontSize / run.Typeface.UnitsPerEm, run.Direction is TextDirection.RTL);
        uint[] featured = unfeatured.IsSome ? GlyphIds(infos) : [];
        Option<(float Em, float Rise)> synthetic =
            from plain in unfeatured
            where plain.AsSpan().SequenceEqual(featured)
            from variant in style.Variant
            from size in Position(font, variant.Size)
            from offset in Position(font, variant.Offset)
            select (Em: (float)size / run.Typeface.UnitsPerEm, Rise: variant.Direction * offset * scale);
        (float em, float rise) = synthetic.IfNone((Em: 1f, Rise: 0f));
        SKPoint origin = new(run.XCoord + (style.LetterSpacing * lean), run.Line.YCoord + run.Line.BaseLine - rise);
        int overhangStart = run.Start;
        ushort[] glyphs = new ushort[overhangStart + infos.Length];
        SKPoint[] points = new SKPoint[infos.Length];
        int[] clusters = new int[infos.Length];
        float[] carets = new float[run.Length];
        (float pen, float opened, int first) = (0f, 0f, 0);
        for (int index = 0; index < infos.Length; index++) {
            (int cluster, bool opens) = ((int)infos[index].Cluster + context.Start, index is 0 || infos[index].Cluster != infos[index - 1].Cluster);
            (opened, first) = opens ? (pen, index) : (opened, first);
            if (!rtl && (opens || pen < carets[cluster - run.Start]))
                carets[cluster - run.Start] = pen;
            glyphs[overhangStart + index] = (ushort)infos[index].Codepoint;
            clusters[index] = cluster;
            points[index] = new SKPoint(origin.X + pen + (at[index].XOffset * scale * em), origin.Y - (at[index].YOffset * scale * em));
            pen += at[index] is { XAdvance: 0 } && text[(int)infos[index].Cluster] is ParagraphSeparator ? style.FontSize * 2f / 3f : at[index].XAdvance * scale * em;
            if (index + 1 == infos.Length || infos[index].Cluster != infos[index + 1].Cluster) {
                float advance = pen + style.LetterSpacing - opened;
                float snapped = Snapped(style.Cell, advance);
                for (int glyph = first; glyph <= index; glyph++)
                    points[glyph].X += (snapped - advance) / 2f;
                pen = opened + snapped;
            }
            if (rtl && (opens || pen > carets[cluster - run.Start]))
                carets[cluster - run.Start] = pen;
        }
        if (rtl)
            carets[0] = pen;
        for (int index = rtl ? run.Length - 2 : 1; index >= 0 && index < run.Length; index += rtl ? -1 : 1)
            carets[index] = carets[index] is 0f ? carets[index + (rtl ? 1 : -1)] : carets[index];
        run.Glyphs = new Slice<ushort>(glyphs, overhangStart, infos.Length);
        run.GlyphPositions = new Slice<SKPoint>(points);
        run.Clusters = new Slice<int>(clusters);
        run.RelativeCodePointXCoords = new Slice<float>(carets);
        if (synthetic.IsSome)
            run.Style = style with { FontSize = style.FontSize * em };
        float delta = pen - run.Width;
        run.Width = pen;
        return delta;
    }

    private static ReadOnlySpan<GlyphInfo> Shape(Font font, HarfBuzzSharp.Buffer buffer, FontRun run, Slice<int> context, Language language, Feature[] features) {
        buffer.Reset();
        buffer.Direction = run.Direction is TextDirection.RTL ? Direction.RightToLeft : Direction.LeftToRight;
        buffer.Language = language;
        buffer.AddUtf32(context.AsSpan(), run.Start - context.Start, run.Length);
        buffer.GuessSegmentProperties();
        font.Shape(buffer, features);
        return buffer.GetGlyphInfoSpan();
    }

    private static uint[] GlyphIds(ReadOnlySpan<GlyphInfo> infos) {
        uint[] ids = new uint[infos.Length];
        for (int index = 0; index < infos.Length; index++)
            ids[index] = infos[index].Codepoint;
        return ids;
    }

    private static Option<int> Position(Font font, OpenTypeMetricsTag tag) => font.OpenTypeMetrics.TryGetPosition(tag, out int units) ? Some(units) : None;

    private static float Snapped(Option<float> cell, float advance) =>
        cell.Map(width => advance > 0f ? float.Max(1f, MathF.Round(advance / width, MidpointRounding.ToEven)) * width : advance).IfNone(advance);

    // --- [OUTLINES]
    internal static SKPath Outline(Typeset set, SKPoint origin) {
        using SKPathBuilder builder = new();
        foreach (FontRun run in GlyphRuns(set.Lines)) {
            using SKFont font = new(run.Typeface, run.Style.FontSize);
            using SKPath glyphs = font.GetTextPath(MemoryMarshal.AsBytes(run.Glyphs.AsSpan()), SKTextEncoding.GlyphId, run.GlyphPositions.AsSpan());
            builder.AddPath(glyphs, origin.X, origin.Y);
        }
        return builder.Detach();
    }

    internal static Option<SKTextBlob> Along(Typeset set, SKPath path, float offset) {
        using SKTextBlobBuilder builder = new();
        using SKPathMeasure measure = new(path);
        float baseline = set.At(Baseline.First).IfNone(0f);
        foreach (FontRun run in GlyphRuns(set.Lines)) {
            ReadOnlySpan<SKPoint> at = run.GlyphPositions.AsSpan();
            float[] widths = new float[at.Length];
            SKPoint[] offsets = new SKPoint[at.Length];
            for (int index = 0; index < at.Length; index++) {
                widths[index] = (index + 1 < at.Length ? at[index + 1].X : run.XCoord + run.Width) - at[index].X;
                offsets[index] = new SKPoint(at[index].X - run.XCoord, at[index].Y - baseline);
            }
            using SKPath along = measure.GetSegment(offset + run.XCoord, measure.Length, startWithMoveTo: true);
            using SKFont font = new(run.Typeface, run.Style.FontSize);
            builder.AddPathPositionedRun(run.Glyphs.AsSpan(), font, widths, offsets, along);
        }
        return Optional(builder.Build());
    }

    private static Seq<FontRun> GlyphRuns(TextBlock lines) => toSeq(lines.Lines).Bind(VisibleRuns).Filter(static run => run.Glyphs.Length > 0);

    // --- [PAINT]
    internal static TextPaintOptions Options(Typeset set, TextPaintOptions raster) => set.Setting.Selection.Map(selection => WithSelection(raster.Clone(), selection)).IfNone(raster);

    private static TextPaintOptions WithSelection(TextPaintOptions options, TextSelection selection) {
        options.Selection = selection.Range;
        options.SelectionColor = selection.Color;
        return options;
    }
}
