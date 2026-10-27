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
    private static readonly HashMap<FontVariant, (Feature Feature, OpenTypeMetricsTag Size, OpenTypeMetricsTag Offset, float Direction)> Variants = HashMap(
        (FontVariant.SuperScript, (new Feature(Tag.Parse("sups")), OpenTypeMetricsTag.SuperScriptEmYSize, OpenTypeMetricsTag.SuperScriptEmYOffset, 1f)),
        (FontVariant.SubScript, (new Feature(Tag.Parse("subs")), OpenTypeMetricsTag.SubScriptEmYSize, OpenTypeMetricsTag.SubScriptEmYOffset, -1f)));

    internal static TextStyle Resolve(Shaping shaping, Cascade cascade, RoleStyle style) {
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
            TextColor = cascade.Theme.Color(style.Text, style).Map(color => (SKColor)cascade.Theme.Legible(style, cascade.Background, color, text: true)).IfNone(SKColors.Transparent),
            BackgroundColor = face.Background.Bind(color => cascade.Theme.Color(color, style)).Map(static color => (SKColor)color).IfNone(Plain.BackgroundColor),
            HaloColor = face.Halo.Bind(halo => cascade.Theme.Color(halo.Color, style)).Map(static halo => (SKColor)halo).IfNone(Plain.HaloColor),
            HaloWidth = face.Halo.Map(static halo => halo.Width).IfNone(Plain.HaloWidth),
            HaloBlur = face.Halo.Map(static halo => halo.Blur).IfNone(Plain.HaloBlur),
            LetterSpacing = face.LetterSpacing.IfNone(Plain.LetterSpacing),
            TextDirection = face.Direction.IfNone(Plain.TextDirection),
            ReplacementCharacter = face.Replacement.IfNone(Plain.ReplacementCharacter),
            Features = toSeq(face.Features.AsIterable().Select(static feature => new Feature(feature.Key, feature.Value))),
            Variant = face.Variant.Bind(Variants.Find),
            Variations = face.Variations,
            Palette = toSeq(face.Palette.AsIterable()).Choose(entry => cascade.Theme.Color(entry.Value, style).Map(color => new SKFontPaletteOverride { Index = entry.Key, Color = (uint)(SKColor)color })),
            Dark = cascade.Theme.Conditions.Contains(Condition.Dark),
            Stroked = face.Stroked,
        };
        return resolved with { Cell = Cell(shaping, resolved) };
    }

    private static Option<float> Cell(Shaping shaping, TextStyle style) =>
        Some(shaping.Mapper.TypefaceFromStyle(style, ignoreFontVariants: false)).Filter(static primary => primary.IsFixedPitch).Bind(primary => shaping.HarfBuzzFont(primary) switch {
            var font => font.TryGetGlyph('0', out uint zero) ? Some(font.GetHorizontalGlyphAdvance(zero) * style.FontSize / primary.UnitsPerEm) : None,
        });

    private static TextStyle SpanStyle(Shaping shaping, Cascade cascade, TextStyle face, Span span, int index) =>
        (span.Role.IsNone && span.Conditions.IsEmpty
            ? face
            : Resolve(shaping, cascade, Styles.Resolve(cascade, span.Role.IfNone(cascade.Enclosing.Role), span.Conditions.Union(cascade.Enclosing.Conditions)))) with { Language = span.Language, Span = span.Part.Map(part => (index, part)), };

    private static Option<(TextRange Range, SKColor Color)> Selected(Node.Text text, Cascade cascade) =>
        from range in text.Selected
        let style = Styles.Resolve(cascade, Role.Selection, [])
        from fill in style.Solid
        from color in cascade.Theme.Color(fill, style)
        select (range, (SKColor)color);

    // --- [RUNS]
    private const int TextPresentation = 0xFE0E;
    private const int EmojiPresentation = 0xFE0F;

    private static readonly string[] Emoji = [$"und-{UScript.GetShortName(UScript.SymbolsEmoji)}"];

    private static Seq<(string Text, TextStyle Style)> FaceRuns(Shaping shaping, string text, TextStyle style, CultureInfo culture) {
        Font primary = shaping.HarfBuzzFont(shaping.Mapper.TypefaceFromStyle(style, ignoreFontVariants: false));
        shaping.Graphemes.SetText(text);
        return toSeq(Segments(shaping.Graphemes)
            .GroupAdjacent(segment => Fallback(text[segment.Start..segment.End], style, primary, culture),
                (face, segments) => (Text: text[segments[0].Start..segments[^1].End], Style: face.Map(fallback => style with { Fallback = fallback }).IfNone(style)))).Strict();
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

    private static Seq<(int Start, int End)> Segments(BreakIterator breaks) =>
        toSeq(SuperEnumerable.Generate(breaks.First(), _ => breaks.Next()).TakeWhile(static stop => stop != BreakIterator.Done).Lag(1, static (end, start) => (Start: start, End: end)).Skip(1)).Strict();

    // --- [SET]
    private const string Ellipsis = "…";

    internal static Typeset Set(Shaping shaping, Node.Text text, Cascade cascade, TextStyle style, float width, CultureInfo culture) {
        StyledText runs = new();
        _ = text.Spans.Map((span, index) => FaceRuns(shaping, span.Text, SpanStyle(shaping, cascade, style, span, index), culture)).Flatten().Iter(run => runs.AddText(run.Text, run.Style));
        Option<StyledText> cut = text.MaxLines.Filter(static lines => lines is 1).Bind(_ => Cut(shaping, text, style, width, culture, runs));
        return Fit(width, []);

        Typeset Fit(float measure, Seq<int> prior) {
            TextBlock lines = Block(shaping, text, culture, cut.IfNone(runs), measure);
            Seq<(float Laid, float Shaped)> over = Reshaped(shaping, lines, culture, width).Filter(line => line.Shaped > width);
            Seq<int> breaks = toSeq(lines.Lines).Map(static line => line.End);
            return !over.IsEmpty && breaks != prior
                ? Fit(over.Fold(measure, (least, line) => float.Min(least, line.Laid * width / line.Shaped)), breaks)
                : new Typeset(shaping, text, style, width, Selected(text, cascade), lines, cut.IsSome || !over.IsEmpty);
        }
    }

    internal static float MinContent(Shaping shaping, Typeset set, CultureInfo culture) {
        if (set.Text.MaxLines.Exists(static lines => lines is 1))
            return EllipsisWidth(shaping, set.Text, set.Style, culture);
        TextBlock lines = set.Lines;
        string text = lines.ToString();
        BreakIterator breaks = shaping.LineBreaks(culture);
        breaks.SetText(text);
        return Segments(breaks).Fold(0f, (widest, segment) => float.Max(widest, MathF.Abs(
            Caret(lines, lines.CharacterToCodePointIndex(segment.Start + text.AsSpan(segment.Start, segment.End - segment.Start).TrimEnd().Length)) - Caret(lines, lines.CharacterToCodePointIndex(segment.Start)))));
    }

    private static TextBlock Block(Shaping shaping, Node.Text text, CultureInfo culture, StyledText runs, float measure) {
        TextBlock block = new() {
            FontMapper = shaping.Mapper,
            MaxWidth = float.IsFinite(measure) ? measure : null,
            MaxLines = text.MaxLines.ToNullable(),
            MaxHeight = text.MaxHeight.ToNullable(),
            Alignment = text.Alignment,
            BaseDirection = culture.TextInfo.IsRightToLeft ? TextDirection.RTL : TextDirection.LTR,
        };
        block.AddText(runs);
        return block;
    }

    private static float EllipsisWidth(Shaping shaping, Node.Text text, TextStyle style, CultureInfo culture) {
        TextBlock mark = Block(shaping, text with { MaxLines = None, MaxHeight = None }, culture, new StyledText(), float.PositiveInfinity);
        mark.AddText(Ellipsis, style);
        _ = Reshaped(shaping, mark, culture, float.PositiveInfinity);
        return Widest(mark);
    }

    private static Option<StyledText> Cut(Shaping shaping, Node.Text text, TextStyle style, float width, CultureInfo culture, StyledText runs) {
        TextBlock whole = Block(shaping, text with { MaxLines = None, MaxHeight = None }, culture, runs, float.PositiveInfinity);
        _ = Reshaped(shaping, whole, culture, float.PositiveInfinity);
        if (Widest(whole) <= width)
            return None;
        Seq<int> stops = toSeq(whole.CaretIndicies);
        (float origin, float finish, float budget) = (Caret(whole, 0), Caret(whole, whole.Length), float.Max(0f, width - EllipsisWidth(shaping, text, style, culture)));
        int head = stops.Fold(0, (kept, stop) => MathF.Abs(Caret(whole, stop) - origin) <= budget * (float)text.Truncation.Head ? stop : kept);
        float rest = budget - MathF.Abs(Caret(whole, head) - origin);
        int tail = stops.Rev().Fold(whole.Length, (kept, stop) => stop >= head && MathF.Abs(finish - Caret(whole, stop)) <= rest ? stop : kept);
        whole.DeleteText(head, tail - head);
        whole.InsertText(head, Ellipsis, style);
        return whole;
    }

    private static float Caret(TextBlock lines, int index) => lines.GetCaretInfo(new CaretPosition(index)).CaretXCoord;

    // --- [RESHAPE]
    private const int ParagraphSeparator = 0x2029;

    private static readonly Func<(TextAlignment Alignment, TextDirection Direction), float> LeadingSlack = memo<(TextAlignment Alignment, TextDirection Direction), float>(static paragraph => {
        TextBlock probe = new() { MaxWidth = 100f, Alignment = paragraph.Alignment, BaseDirection = paragraph.Direction };
        probe.AddText("0", Plain);
        SKRect padding = probe.MeasuredPadding;
        return padding.Left / (padding.Left + padding.Right);
    });

    internal static float Widest(TextBlock lines) => toSeq(lines.Lines).Map(Advance).Fold(0f, float.Max);

    private static float Advance(TextLine line) => VisibleRuns(line).Fold(0f, static (sum, run) => sum + run.Width);

    private static Seq<FontRun> VisibleRuns(TextLine line) => toSeq(line.Runs).Filter(static run => run.RunKind is not FontRunKind.TrailingWhitespace);

    private static Seq<(float Laid, float Shaped)> Reshaped(Shaping shaping, TextBlock lines, CultureInfo culture, float width) {
        float box = lines.MaxWidth ?? lines.MeasuredWidth;
        float lean = LeadingSlack((lines.Alignment, lines.BaseDirection));
        Seq<(TextLine Line, Seq<(FontRun Run, float Delta)> Runs)> shaped = toSeq(lines.Lines)
            .Map(line => (Line: line, Runs: toSeq(line.Runs.OrderBy(static run => run.XCoord)).Map(run => (Run: run, Delta: Delta(shaping, lines, run, culture, lean))).Strict()))
            .Strict();
        Seq<(float Laid, float Shaped)> measured = shaped.Map(static held => Advance(held.Line) switch {
            var now => (Laid: now - held.Runs.Fold(0f, static (sum, run) => sum + run.Delta), Shaped: now),
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

    private static uint[] GlyphIds(ReadOnlySpan<GlyphInfo> infos) => [.. infos.ToArray().Select(static info => info.Codepoint)];

    private static float Snapped(Option<float> cell, float advance) =>
        cell.Map(width => advance > 0f ? float.Max(1f, MathF.Round(advance / width, MidpointRounding.ToEven)) * width : advance).IfNone(advance);

    private static Option<int> Position(Font font, OpenTypeMetricsTag tag) => font.OpenTypeMetrics.TryGetPosition(tag, out int units) ? Some(units) : None;

    // --- [OUTLINES]
    internal static SKPath Outline(Typeset set) {
        using SKPathBuilder builder = new();
        foreach (FontRun run in GlyphRuns(set)) {
            using SKFont font = new(run.Typeface, run.Style.FontSize);
            using SKPath glyphs = font.GetTextPath(MemoryMarshal.AsBytes(run.Glyphs.AsSpan()), SKTextEncoding.GlyphId, run.GlyphPositions.AsSpan());
            builder.AddPath(glyphs);
        }
        return builder.Detach();
    }

    internal static Option<SKTextBlob> Along(Typeset set, SKPath path, float offset) {
        using SKTextBlobBuilder builder = new();
        using SKPathMeasure measure = new(path);
        float baseline = set.At(Baseline.First).IfNone(0f);
        foreach (FontRun run in GlyphRuns(set)) {
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

    private static Seq<FontRun> GlyphRuns(Typeset set) => toSeq(set.Lines.Lines).Bind(VisibleRuns).Filter(static run => run.Glyphs.Length > 0);

    // --- [PAINT]
    internal static void Paint(SKCanvas canvas, Typeset set, SKPoint origin, TextPaintOptions raster, SKColor tint) {
        Seq<(FontRun Run, IStyle Resting)> tinted = tint == set.Style.TextColor
            ? []
            : toSeq(set.Lines.Lines).Bind(static line => toSeq(line.Runs)).Filter(run => run.Style.TextColor == set.Style.TextColor).Map(static run => (Run: run, Resting: run.Style)).Strict();
        _ = tinted.Iter(held => held.Run.Style = ((TextStyle)held.Resting) with { TextColor = tint });
        set.Lines.Paint(canvas, origin, set.Selection.Match(Some: selection => Highlighted(raster.Clone(), selection), None: () => raster));
        _ = tinted.Iter(static held => held.Run.Style = held.Resting);
    }

    private static TextPaintOptions Highlighted(TextPaintOptions options, (TextRange Range, SKColor Color) selection) {
        options.Selection = selection.Range;
        options.SelectionColor = selection.Color;
        return options;
    }
}
