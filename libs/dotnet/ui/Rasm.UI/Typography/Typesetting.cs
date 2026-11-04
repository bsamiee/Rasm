using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using ICU4N;
using ICU4N.Globalization;
using ICU4N.Text;
using Rasm.UI.Layout;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;
using Topten.RichTextKit;
using Topten.RichTextKit.Utils;

namespace Rasm.UI.Typography;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed class Typeset(Shaping shaping, View.Text text, TextStyle style, float width, float minContent, Option<(TextRange Range, SKColorF Color)> selection, TextBlock source, Seq<Typeset.Line> lines, bool clipped) {
    private Lazy<SKRect> Ink => field ??= new(() => Typesetting.Bounds(this));
    private readonly Lazy<Option<float>> capHeight = new(() =>
        from run in lines.Bind(static line => line.Runs).Head
        from units in Typesetting.Position(shaping.HarfBuzzFont(run.Run.Typeface), OpenTypeMetricsTag.CapHeight)
        select units * Typesetting.PerUnit(run.Run.Typeface, run.Run.FontSize));

    internal View.Text Text { get; } = text;
    internal TextStyle Style { get; } = style;
    internal float Width { get; } = width;
    internal float MinContent { get; } = minContent;
    internal Option<(TextRange Range, SKColorF Color)> Selection { get; } = selection;
    internal TextBlock Source { get; } = source;
    internal Seq<Line> Lines { get; } = lines;
    internal SKSize Size { get; } = new(lines.AsEnumerable().Aggregate(0f, static (widest, line) => float.MaxNumber(widest, line.Width)), lines.Last.Map(static line => line.Y + line.Metrics.Height).IfNone(0f));
    internal bool Truncated { get; } = clipped;
    internal Map<int, Seq<(Slice<int> Text, SKRect Bounds)>> Parts { get; } = toSeq(
        from line in lines.AsEnumerable()
        let metrics = line.Metrics
        from run in line.Runs.AsEnumerable()
        select (run.Run, Bounds: SKRect.Create(run.X, line.Y, run.Run.Width, metrics.Height)))
        .Filter(static held => held.Run.Kind is FontRunKind.Normal)
        .Fold(Map<int, Seq<(Slice<int>, SKRect)>>(), static (held, run) => run.Run.Style.Span.Map(head => held.AddOrUpdate(head, Some: runs => runs.Add((run.Run.Text, run.Bounds)), None: () => [(run.Run.Text, run.Bounds)])).IfNone(held));

    internal SKRect Overhang => Ink.Value switch {
        var bounds => new(float.Max(0f, -bounds.Left), float.Max(0f, -bounds.Top), float.Max(0f, bounds.Right - Size.Width), float.Max(0f, bounds.Bottom - Size.Height)),
    };

    internal Option<float> At(Baseline baseline) =>
        from line in Lines.At(baseline.Line.GetOffset(Lines.Count))
        from rise in (float)baseline.Rise switch {
            0f => Some(0f),
            var share => capHeight.Value.Map(cap => cap * share),
        }
        select line.Y + line.Metrics.Baseline - rise;

    internal Option<int> PartAt(SKPoint local) =>
        HitTest(local).OverCodePointIndex switch {
            < 0 => None,
            var index => ((TextStyle)Source.GetStyleAtOffset(index + 1)).Span,
        };

    internal HitTestResult HitTest(SKPoint at) => Lines.Map(static (line, index) => (Line: line, Index: index)) switch {
        var rows => (from row in rows.Find(row => at.Y < row.Line.Y + row.Line.Metrics.Height) | rows.Last
                     let line = row.Line
                     let over = at.Y >= line.Y && at.Y < line.Y + line.Metrics.Height
                     let hit = line.HitTest(at.X)
                     select hit with {
                         ClosestLine = row.Index,
                         OverLine = over ? row.Index : -1,
                         OverCodePointIndex = over ? hit.OverCodePointIndex : -1,
                     }).IfNone(new HitTestResult { ClosestCodePointIndex = 0, OverCodePointIndex = -1, ClosestLine = -1, OverLine = -1 }),
    };

    internal sealed record Run(Slice<int> Text, TextStyle Style, SKTypeface Typeface, TextDirection Direction, ReadOnlyMemory<ushort> Glyphs, ReadOnlyMemory<SKPoint> Positions, ReadOnlyMemory<int> Clusters, ReadOnlyMemory<float> Carets, LanguageExt.HashSet<int> Unsafe, SKFontMetrics Metrics, float Em = 1f, float Rise = 0f, FontRunKind Kind = FontRunKind.Normal) {
        private static readonly Comparer<int> Descending = Comparer<int>.Create(static (first, second) => second.CompareTo(first));
        private readonly Lazy<Seq<int>> stops = new(() => toSeq(Enumerable.Range(0, Clusters.Length).Select(index => Clusters.Span[index]).Append(Text.Start).Append(Text.Start + Text.Length).Distinct().Order()));
        private Comparer<int> Order => Direction is TextDirection.RTL ? Descending : Comparer<int>.Default;
        internal int End => Text.Start + Text.Length;
        internal float Width => Advance(Text.Start, End);
        internal float Origin => Direction is TextDirection.RTL ? Carets.Span[^1] : Carets.Span[0];
        internal float FontSize => Style.FontSize * Em;
        internal Seq<int> Stops => stops.Value.Filter(stop => stop >= Text.Start && stop <= End);
        internal float HalfLeading => (((Metrics.Descent - Metrics.Ascent) * (Style.LineHeight - 1f)) + Metrics.Leading) / 2f;
        internal float Caret(int index) => Carets.Span[index - Text.Start] - Origin;
        internal float Advance(int first, int last) => Direction is TextDirection.RTL ? Carets.Span[first - Text.Start] - Carets.Span[last - Text.Start] : Carets.Span[last - Text.Start] - Carets.Span[first - Text.Start];
        internal bool HasBoundary(int point) => point == Text.Start || point == End || Clusters.Span.BinarySearch(point, Order) >= 0;

        internal Run Slice(int start, int end, FontRunKind kind) {
            if (start == Text.Start && end == End) return Kind == kind ? this : this with { Kind = kind };
            int offset = Bound(Direction is TextDirection.RTL ? end : start);
            int count = Bound(Direction is TextDirection.RTL ? start : end) - offset;
            return this with { Text = Text.SubSlice(start - Text.Start, end - start), Glyphs = Glyphs.Slice(offset, count), Positions = Positions.Slice(offset, count), Clusters = Clusters.Slice(offset, count), Carets = Carets.Slice(start - Text.Start, end - start + 1), Kind = kind };

            int Bound(int point) {
                int index = Clusters.Span.BinarySearch(point, Order);
                return index < 0 ? ~index : Direction is TextDirection.RTL
                    ? Clusters.Span[index..].IndexOfAnyExcept(point) switch { < 0 => Clusters.Length, var after => index + after }
                    : Clusters.Span[..index].LastIndexOfAnyExcept(point) + 1;
            }
        }
    }

    internal sealed record Line(Seq<(Run Run, float X)> Runs, int Start, int End, float Y) {
        internal float Width => Typesetting.Width(Runs.AsEnumerable().Select(static held => held.Run));
        internal (float Baseline, float Height) Metrics => Measure(Runs.AsEnumerable().Select(static held => held.Run));

        internal HitTestResult HitTest(float x) {
            (Run last, float lastX) = Runs[^1];
            if (last.Kind is FontRunKind.TrailingWhitespace or FontRunKind.Ellipsis
                && (last.Direction is TextDirection.LTR ? x >= lastX + last.Width : x < lastX)
                && last.Text[^1] is '\n' or 0x2029)
                return new HitTestResult { ClosestCodePointIndex = last.End - 1, OverCodePointIndex = -1 };
            Seq<(Run Run, float X)> visible = Runs.Filter(static held => held.Run.Kind is not FontRunKind.Ellipsis);
            return visible.Filter(held => x >= held.X && x < held.X + held.Run.Width)
                .Bind(static held => toSeq(held.Run.Stops.AsEnumerable().Window(2, pair => (Start: pair[0], End: pair[1], held.Run, X1: held.X + held.Run.Caret(pair[0]), X2: held.X + held.Run.Caret(pair[1])))))
                .Find(cluster => x >= float.Min(cluster.X1, cluster.X2) && x < float.Max(cluster.X1, cluster.X2))
                .Map(cluster => (Index: (cluster.Run.Text[cluster.Start - cluster.Run.Text.Start] is '\n',
                    x < float.Lerp(float.Min(cluster.X1, cluster.X2), float.Max(cluster.X1, cluster.X2), 0.5f), cluster.Run.Direction is TextDirection.LTR) switch {
                        (true, _, _) or (false, true, true) or (false, false, false) => cluster.Start,
                        _ => cluster.End,
                    }, Over: cluster.Start))
                .IfNone(() => (visible.Filter(held => x < held.X || x >= held.X + held.Run.Width)
                    .Map(held => (Left: x < held.X, held.Run, held.X) switch {
                        var edge => (Index: (edge.Left, edge.Run.Direction) switch {
                            (true, TextDirection.LTR) or (false, TextDirection.RTL) => edge.Run.Text.Start,
                            _ => edge.Run.End,
                        },
                            X: edge.Left ? edge.X : edge.X + edge.Run.Width),
                    })
                    .AsEnumerable().DefaultIfEmpty((Index: -1, X: x)).MinBy(edge => MathF.Abs(edge.X - x)).Index, Over: -1)) switch {
                        var hit => new() { ClosestCodePointIndex = hit.Index, OverCodePointIndex = hit.Over, AltCaretPosition = hit.Index == End },
                    };
        }

        internal static (float Baseline, float Height) Measure(IEnumerable<Run> runs) =>
            runs.Aggregate((Baseline: 0f, Descent: 0f), static (held, run) => {
                float leading = run.HalfLeading;
                return (float.MaxNumber(held.Baseline, -run.Metrics.Ascent + run.Rise + leading), float.MaxNumber(held.Descent, run.Metrics.Descent - run.Rise + leading));
            }) switch { var metrics => (metrics.Baseline, metrics.Baseline + metrics.Descent), };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Typesetting {
    // --- [RESOLVE]
    private static readonly Style Plain = new();
    private static readonly HashMap<FontVariant, (Feature Feature, OpenTypeMetricsTag Size, OpenTypeMetricsTag Offset, float Direction)> Variants = HashMap(
        (FontVariant.SuperScript, (new Feature(Tag.Parse("sups")), OpenTypeMetricsTag.SuperScriptEmYSize, OpenTypeMetricsTag.SuperScriptEmYOffset, 1f)),
        (FontVariant.SubScript, (new Feature(Tag.Parse("subs")), OpenTypeMetricsTag.SubScriptEmYSize, OpenTypeMetricsTag.SubScriptEmYOffset, -1f)));

    internal static TextStyle Resolve(Shaping shaping, Cascade cascade, RoleStyle style) {
        TextFace face = cascade.Theme.Face.Combine(style.Face);
        TextStyle resolved = new() {
            FontFamily = face.Family.IfNone(Plain.FontFamily),
            FontSize = cascade.Theme.FontSize * style.Metrics[Metric.FontSize],
            FontWeight = face.Weight.Map(static weight => (int)weight).IfNone(Plain.FontWeight),
            FontWidth = face.Width.IfNone(Plain.FontWidth),
            FontItalic = face.Italic.IfNone(Plain.FontItalic),
            Underline = face.Underline.IfNone(Plain.Underline),
            StrikeThrough = face.StrikeThrough.IfNone(Plain.StrikeThrough),
            LineHeight = face.LineHeight.IfNone(Plain.LineHeight),
            TextColor = Colored(cascade, style),
            BackgroundColor = face.Background.Bind(color => cascade.Theme.Color(color, style)).IfNone(Plain.BackgroundColor),
            HaloColor = face.Halo.Bind(halo => cascade.Theme.Color(halo.Color, style)).IfNone(Plain.HaloColor),
            HaloWidth = face.Halo.Map(static halo => halo.Width).IfNone(Plain.HaloWidth),
            HaloBlur = face.Halo.Map(static halo => halo.Blur).IfNone(Plain.HaloBlur),
            LetterSpacing = face.LetterSpacing.IfNone(Plain.LetterSpacing),
            TextDirection = face.Direction.IfNone(Plain.TextDirection),
            ReplacementCharacter = face.Replacement.IfNone(Plain.ReplacementCharacter),
            Features = toSeq(face.Features.AsIterable().Select(static feature => new Feature(feature.Key, feature.Value))),
            Variant = face.Variant.Filter(Variants.ContainsKey),
            Variations = face.Variations,
            Palette = toSeq(face.Palette.AsIterable()).Choose(entry => cascade.Theme.Color(entry.Value, style).Map(color => new SKFontPaletteOverride { Index = entry.Key, Color = (uint)(SKColor)color })),
            Dark = cascade.Theme.Conditions.Contains(Condition.Dark),
            Stroked = face.Stroked,
        };
        return resolved with { Cell = Cell(shaping, resolved) };
    }

    private static SKColorF Colored(Cascade cascade, RoleStyle style) =>
        cascade.Theme.Color(style.Text, style).Map(color => cascade.Theme.Legible(style, cascade.Background, color, text: true)).IfNone(SKColors.Transparent);

    private static Option<float> Cell(Shaping shaping, TextStyle style) =>
        Some(shaping.Instance(shaping.Mapper.TypefaceFromStyle(style, ignoreFontVariants: false), style)).Filter(static primary => primary.IsFixedPitch).Bind(primary => shaping.HarfBuzzFont(primary) switch {
            var font => font.TryGetGlyph('0', out uint zero) ? Some(font.GetHorizontalGlyphAdvance(zero) * PerUnit(primary, style.FontSize)).Filter(static width => width > 0f) : None,
        });

    private static TextStyle SpanStyle(Shaping shaping, Cascade cascade, TextStyle face, Span span, int head) =>
        (span.Role.IsNone && span.Conditions.IsEmpty
            ? face
            : Resolve(shaping, cascade, cascade.Resolve(span.Role.IfNone(cascade.Enclosing.Role), span.Conditions.Union(cascade.Enclosing.Conditions)))) with { Language = span.Language, Span = span.Part.Map(_ => head), };

    private static Option<(TextRange Range, SKColorF Color)> Selected(View.Text text, Cascade cascade) =>
        from range in text.Selected
        let style = cascade.Resolve(Roles.Selection, [])
        from fill in style.Solid
        from color in cascade.Theme.Color(fill, style)
        select (range, color);

    // --- [RUNS]
    private const int TextPresentation = 0xFE0E;
    private const int EmojiPresentation = 0xFE0F;

    private static readonly string[] Emoji = [$"und-{UScript.GetShortName(UScript.SymbolsEmoji)}"];

    private static Seq<(string Text, TextStyle Style)> FaceRuns(Shaping shaping, string text, TextStyle style, CultureInfo culture) {
        Font primary = shaping.HarfBuzzFont(shaping.Mapper.TypefaceFromStyle(style, ignoreFontVariants: false));
        return toSeq(Segments(shaping.Graphemes, text)
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

    private static Seq<(int Start, int End)> Segments(BreakIterator breaks, string text) {
        breaks.SetText(text);
        return toSeq(SuperEnumerable.Generate(breaks.First(), _ => breaks.Next()).TakeWhile(static stop => stop != BreakIterator.Done).Window(2, static pair => (Start: pair[0], End: pair[1]))).Strict();
    }

    // --- [SET]
    internal static Typeset Set(Shaping shaping, View.Text text, Cascade cascade, TextStyle style, float width, CultureInfo culture) {
        TextBlock source = new() { FontMapper = shaping.Mapper, BaseDirection = culture.TextInfo.IsRightToLeft ? TextDirection.RTL : TextDirection.LTR };
        _ = text.Placeholder.Filter(_ => text.Edited.IsNone && text.Content.Length is 0).Match(
                Some: placeholder => FaceRuns(shaping, placeholder, style with { TextColor = Colored(cascade, cascade.Resolve(Roles.Placeholder, cascade.Enclosing.Conditions)) }, culture),
                None: () => text.Spans.Fold(Seq<(Span Span, int Head)>(), static (held, span) => held.Add((span, held.Last.Filter(prior => span.Part.IsSome && prior.Span.Part == span.Part).Map(static prior => prior.Head).IfNone(held.Count))))
                    .Map(held => FaceRuns(shaping, held.Span.Text, SpanStyle(shaping, cascade, style, held.Span, held.Head), culture)).Flatten())
            .Iter(run => source.AddText(run.Text, run.Style));
        Slice<int> context = source.CodePoints.AsSlice();
        Seq<Typeset.Run> runs = toSeq(source.FontRuns).Map(run => Shaped(shaping, run.CodePoints, context, run.Start, (TextStyle)run.Style, shaping.Instance(run.Typeface, (TextStyle)run.Style), run.Direction, culture)).Strict();
        LanguageExt.HashSet<int> unsafeBreaks = toHashSet(runs.Bind(static run => toSeq(run.Unsafe)));
        Seq<(int End, int Measure, bool Required)> breaks = Breaks();
        Lazy<Typeset.Run> mark = new(() => {
            StyledText points = new();
            TextStyle plain = style with { Span = None, Variant = None };
            points.AddText("…", plain);
            SKTypeface face = FontFallback.GetFontRuns(points.CodePoints.AsSlice(), shaping.Mapper.TypefaceFromStyle(plain, ignoreFontVariants: true)).First().Typeface;
            return Shaped(shaping, points.CodePoints.AsSlice(), points.CodePoints.AsSlice(), 0, plain, shaping.Instance(face, plain), source.BaseDirection, culture, FontRunKind.Ellipsis);
        });
        float minimum = text.MaxLines.Exists(static max => max is 1) ? mark.Value.Width
            : breaks.AsEnumerable().Window(2, static pair => (Start: pair[0].End, pair[1].Measure))
                .Select(segment => Width(Ranged(segment.Start, segment.Measure, segment.Measure))).Aggregate(0f, float.MaxNumber);
        float available = float.IsFinite(width) ? width : float.PositiveInfinity;
        Seq<(int Start, int End, Seq<Typeset.Run> Runs)> chunks = [];
        (int start, float y) = (0, 0f);
        while (start < source.Length && text.MaxLines.ForAll(max => chunks.Count < max)) {
            int limit = breaks.Find(stop => stop.End > start && stop.Required).Map(static stop => stop.End).IfNone(source.Length);
            Seq<(int End, int Measure, bool Required)> choices = breaks.Filter(stop => stop.End > start && stop.End <= limit);
            Option<(int End, Seq<Typeset.Run> Runs)> fit = choices.Rev()
                .Map(stop => (stop.End, Runs: Ranged(start, stop.End, stop.Measure)))
                .Find(held => Width(held.Runs) <= available);
            (int end, Seq<Typeset.Run> chosen) = fit.IfNone(() => Emergency(start, choices.Head.Map(static stop => stop.End).IfNone(limit)));
            float height = Typeset.Line.Measure(chosen).Height;
            if (text.MaxHeight.Exists(max => y + height > max)) break;
            chunks = chunks.Add((start, end, chosen));
            (start, y) = (end, y + height);
        }
        bool clipped = start < source.Length || (text.MaxLines.Exists(static max => max is 1) && chunks.Exists(chunk => Width(chunk.Runs) > available));
        Seq<(int Start, int End, Seq<Typeset.Run> Runs)> plans = clipped
            ? toSeq(chunks.Zip(chunks.Scan(0f, static (height, chunk) => height + Typeset.Line.Measure(chunk.Runs).Height)).AsEnumerable()
                .Select(static (held, index) => (Chunk: held.First, Y: held.Second, Count: index + 1))).Rev()
                .Map(held => (Plan: Elision(held.Chunk), held.Y, held.Count))
                .Find(held => text.MaxHeight.ForAll(max => held.Y + Typeset.Line.Measure(held.Plan.Runs).Height <= max))
                .Map(held => chunks.Take(held.Count - 1).Add(held.Plan)).IfNone([])
            : chunks;
        float box = float.IsFinite(available) ? available : plans.AsEnumerable().Select(static plan => Width(plan.Runs)).Aggregate(0f, float.MaxNumber);
        Seq<Typeset.Line> lines = plans.Fold((Lines: Seq<Typeset.Line>(), Y: 0f), (held, plan) => {
            Typeset.Line line = Positioned(plan, held.Y);
            return (held.Lines.Add(line), held.Y + line.Metrics.Height);
        }).Lines;
        return new Typeset(shaping, text, style, width, minimum, Selected(text, cascade), source, lines, clipped || lines.Exists(line => line.Width > available));

        (int Start, int End, Seq<Typeset.Run> Runs) Elision((int Start, int End, Seq<Typeset.Run> Runs) chunk) {
            (int end, int measure) = text.MaxLines.Exists(static max => max is 1)
                ? breaks.Find(static stop => stop.Required).Map(static stop => (stop.End, stop.Measure)).IfNone((source.Length, source.Length))
                : (chunk.End, breaks.Find(stop => stop.End == chunk.End).Map(static stop => stop.Measure).IfNone(chunk.End));
            return (chunk.Start, end, Elided(Ranged(chunk.Start, end, measure).Filter(static run => run.Kind is not FontRunKind.TrailingWhitespace), mark.Value, available, text.MaxLines.Exists(static max => max is 1) ? (float)text.Truncation : 1f));
        }

        Seq<(int End, int Measure, bool Required)> Breaks() {
            BreakIterator iterator = shaping.LineBreaks(culture);
            iterator.SetText(source.ToString());
            Seq<(int End, int Measure, bool Required)> stops = [(0, 0, false)];
            for (int at = iterator.First(), next = iterator.Next(); next != BreakIterator.Done; (at, next) = (next, iterator.Next())) {
                int end = source.CharacterToCodePointIndex(next);
                int measure = end;
                while (measure > source.CharacterToCodePointIndex(at) && UChar.GetIntPropertyValue(source.CodePoints[measure - 1], UProperty.Line_Break) is LineBreak.Space or LineBreak.CarriageReturn or LineBreak.LineFeed or LineBreak.NextLine or LineBreak.MandatoryBreak)
                    measure--;
                bool required = UChar.GetIntPropertyValue(source.CodePoints[end - 1], UProperty.Line_Break) is LineBreak.CarriageReturn or LineBreak.LineFeed or LineBreak.NextLine or LineBreak.MandatoryBreak;
                stops = stops.Add((end, measure, required));
            }
            return stops;
        }

        (int End, Seq<Typeset.Run> Runs) Emergency(int first, int last) {
            Seq<(int End, Seq<Typeset.Run> Runs)> choices = toSeq(runs.Bind(static run => run.Stops).Filter(stop => stop > first && stop <= last).Distinct().Order())
                .Map(stop => (End: stop, Runs: Ranged(first, stop, stop))).Strict();
            return choices.Rev().Find(held => Width(held.Runs) <= available).IfNone(() => choices[0]);
        }

        Seq<Typeset.Run> Ranged(int first, int last, int measure) {
            Slice<int> line = context.SubSlice(first - context.Start, last - first);
            return runs.Filter(run => run.End > first && run.Text.Start < last).Bind(run => {
                int from = Math.Max(first, run.Text.Start);
                int to = Math.Min(last, run.End);
                return to <= measure ? Seq(Fragment(from, to, FontRunKind.Normal))
                    : from >= measure ? Seq(Fragment(from, to, FontRunKind.TrailingWhitespace))
                    : Seq(Fragment(from, measure, FontRunKind.Normal), Fragment(measure, to, FontRunKind.TrailingWhitespace));

                Typeset.Run Fragment(int begin, int end, FontRunKind kind) => (kind is FontRunKind.TrailingWhitespace ? source.BaseDirection : run.Direction) switch {
                    var direction => direction != run.Direction || ((begin == first || begin != run.Text.Start) && unsafeBreaks.Contains(begin))
                        || ((end == last || end != run.End) && unsafeBreaks.Contains(end)) || !run.HasBoundary(begin) || !run.HasBoundary(end)
                        ? Shaped(shaping, context.SubSlice(begin - context.Start, end - begin), line, begin - first, run.Style, run.Typeface, direction, culture, kind)
                        : run.Slice(begin, end, kind),
                };
            }).Strict();
        }

        Seq<Typeset.Run> Elided(Seq<Typeset.Run> entries, Typeset.Run marker, float space, float headShare) {
            if (entries.IsEmpty) return [marker];
            float budget = float.Max(0f, space - marker.Width);
            float headBudget = headShare is 0f ? 0f : budget * headShare;
            Seq<(int At, float Prefix, float Suffix)> stops = toSeq(entries[0].Text.Start.Cons(entries.Bind(static run => run.Stops)).Distinct().Order())
                .Map(stop => (At: stop,
                    Prefix: entries.AsEnumerable().Where(run => run.Text.Start < stop).Aggregate(0f, (sum, run) => sum + run.Advance(run.Text.Start, Math.Min(stop, run.End))),
                    Suffix: entries.AsEnumerable().Where(run => run.End > stop).Aggregate(0f, (sum, run) => sum + run.Advance(Math.Max(stop, run.Text.Start), run.End)))).Strict();
            return (from head in stops.Rev()
                    where head.Prefix <= headBudget
                    let prefix = entries.Filter(run => run.Text.Start < head.At).Map(run => run.Slice(run.Text.Start, Math.Min(head.At, run.End), run.Kind))
                    from tail in stops.Filter(stop => stop.At >= head.At)
                    where tail.Suffix <= budget - head.Prefix
                    let parts = prefix.Add(marker) + entries.Filter(run => run.End > tail.At).Map(run => run.Slice(Math.Max(tail.At, run.Text.Start), run.End, run.Kind))
                    let shaped = Joined(parts)
                    where Width(shaped) <= space
                    select shaped).Head.IfNone(Seq(marker));

            Seq<Typeset.Run> Joined(Seq<Typeset.Run> parts) {
                TextBlock display = new() { FontMapper = shaping.Mapper, BaseDirection = source.BaseDirection };
                _ = parts.Iter(run => display.AddText(run.Text, run.Style));
                Seq<FontRun> directional = toSeq(display.FontRuns);
                Slice<int> displayed = display.CodePoints.AsSlice();
                return toSeq(from piece in parts.Zip(parts.Scan(0, static (offset, run) => offset + run.Text.Length)).AsEnumerable()
                             from run in directional.AsEnumerable()
                             where run.End > piece.Second && run.Start < piece.Second + piece.First.Text.Length
                             let begin = int.Max(piece.Second, run.Start)
                             let end = int.Min(piece.Second + piece.First.Text.Length, run.End)
                             select Shaped(shaping, piece.First.Text.SubSlice(begin - piece.Second, end - begin), displayed, begin,
                                 piece.First.Style, shaping.Instance(run.Typeface, piece.First.Style), run.Direction, culture, piece.First.Kind));
            }
        }

        Typeset.Line Positioned((int Start, int End, Seq<Typeset.Run> Runs) plan, float top) {
            float advance = Width(plan.Runs);
            float lean = Alignment(text.Alignment, source.BaseDirection);
            float origin = (float.IsFinite(box) ? lean * (box - advance) : 0f) - (source.BaseDirection is TextDirection.RTL ? plan.Runs.AsEnumerable().Where(static run => run.Kind is FontRunKind.TrailingWhitespace).Aggregate(0f, static (sum, run) => sum + run.Width) : 0f);
            Seq<(TextDirection Direction, Seq<Typeset.Run> Runs, float Width)> groups = toSeq(plan.Runs.AsEnumerable().GroupAdjacent(static run => run.Direction,
                static (direction, members) => toSeq(members) switch { var grouped => (direction, grouped, grouped.AsEnumerable().Aggregate(0f, static (sum, run) => sum + run.Width)) })).Strict();
            float extent = groups.AsEnumerable().Aggregate(0f, static (sum, group) => sum + group.Width);
            Seq<(Typeset.Run Run, float X)> placed = toSeq(
                from runGroup in groups.Zip(groups.Scan(0f, static (sum, directional) => sum + directional.Width)).AsEnumerable()
                let left = origin + (source.BaseDirection is TextDirection.LTR ? runGroup.Second : extent - runGroup.Second - runGroup.First.Width)
                from held in runGroup.First.Runs.Zip(runGroup.First.Runs.Scan(0f, static (sum, run) => sum + run.Width)).AsEnumerable()
                select (held.First, left + (runGroup.First.Direction is TextDirection.LTR ? held.Second : runGroup.First.Width - held.Second - held.First.Width)));
            return new Typeset.Line(placed, plan.Start, plan.End, top);
        }
    }

    internal static float Width(IEnumerable<Typeset.Run> runs) => runs.Aggregate(0f, static (sum, run) => run.Kind is FontRunKind.TrailingWhitespace ? sum : sum + run.Width);

    private static float Alignment(TextAlignment alignment, TextDirection direction) => (alignment, direction) switch {
        (TextAlignment.Center, _) => 0.5f,
        (TextAlignment.Right, _) or (TextAlignment.Auto, TextDirection.RTL) => 1f,
        _ => 0f,
    };

    // --- [SHAPE]
    private static Typeset.Run Shaped(Shaping shaping, Slice<int> text, Slice<int> context, int offset, TextStyle style, SKTypeface typeface, TextDirection direction, CultureInfo culture, FontRunKind kind = FontRunKind.Normal) {
        Font font = shaping.HarfBuzzFont(typeface);
        if (style.ReplacementCharacter is not '\0') return Replacement();
        HarfBuzzSharp.Buffer buffer = shaping.Buffer;
        using Language language = new(style.Language.IfNone(culture).IetfLanguageTag);
        Option<(Feature Feature, OpenTypeMetricsTag Size, OpenTypeMetricsTag Offset, float Direction)> variant = style.Variant.Bind(Variants.Find);
        uint[] unfeatured = variant.IsSome ? GlyphIds(Shape([.. style.Features])) : [];
        ReadOnlySpan<GlyphInfo> infos = Shape([.. style.Features + variant.Map(static value => value.Feature).ToSeq()]);
        ReadOnlySpan<GlyphPosition> at = buffer.GetGlyphPositionSpan();
        bool same = unfeatured.Length == infos.Length;
        for (int index = 0; same && index < infos.Length; index++) same = unfeatured[index] == infos[index].Codepoint;
        float scale = PerUnit(typeface, style.FontSize);
        Option<(float Em, float Rise)> synthetic = from value in variant
                                                   where same
                                                   from size in Position(font, value.Size)
                                                   from rise in Position(font, value.Offset)
                                                   select (Em: PerUnit(typeface, size), Rise: value.Direction * rise * scale);
        (float em, float raised) = synthetic.IfNone((Em: 1f, Rise: 0f));
        using SKFont native = new(typeface, style.FontSize * em);
        ushort[] glyphs = new ushort[infos.Length];
        SKPoint[] positions = new SKPoint[infos.Length];
        int[] clusters = new int[infos.Length];
        float[] carets = new float[text.Length + 1];
        LanguageExt.HashSet<int> unsafeBreaks = [];
        bool rtl = direction is TextDirection.RTL;
        float? cellWidth = style.Cell.ToNullable();
        float pen = 0f;
        for (int first = 0; first < infos.Length;) {
            int end = first + 1;
            while (end < infos.Length && infos[end].Cluster == infos[first].Cluster) end++;
            int cluster = (int)infos[first].Cluster - offset + text.Start;
            float opened = pen;
            for (int index = first; index < end; index++) {
                glyphs[index] = (ushort)infos[index].Codepoint;
                clusters[index] = cluster;
                unsafeBreaks = infos[index].GlyphFlags.HasFlag(GlyphFlags.UnsafeToBreak) ? unsafeBreaks.TryAdd(cluster) : unsafeBreaks;
                positions[index] = new SKPoint(pen + (at[index].XOffset * scale * em), -raised - (at[index].YOffset * scale * em));
                pen += at[index].XAdvance is 0 && text[cluster - text.Start] is 0x2029 ? style.FontSize * 2f / 3f : at[index].XAdvance * scale * em;
            }
            float advance = pen + style.LetterSpacing - opened;
            float snapped = cellWidth is float cell && advance > 0f ? float.Max(1f, MathF.Round(advance / cell, MidpointRounding.ToEven)) * cell : advance;
            for (int glyph = first; glyph < end; glyph++) positions[glyph].X += (snapped - advance) / 2f;
            pen = opened + snapped;
            int stop = rtl ? first is 0 ? text.Start + text.Length : (int)infos[first - 1].Cluster - offset + text.Start : end == infos.Length ? text.Start + text.Length : (int)infos[end].Cluster - offset + text.Start;
            carets.AsSpan(cluster - text.Start, stop - cluster).Fill(rtl ? pen : opened);
            first = end;
        }
        carets[^1] = rtl ? 0f : pen;
        return new Typeset.Run(text, style, typeface, direction, glyphs, positions, clusters, carets, unsafeBreaks, native.Metrics, em, raised, kind);

        ReadOnlySpan<GlyphInfo> Shape(Feature[] features) {
            buffer.Reset();
            buffer.Flags = (offset is 0 ? BufferFlags.BeginningOfText : BufferFlags.Default)
                | (offset + text.Length == context.Length ? BufferFlags.EndOfText : BufferFlags.Default);
            buffer.Direction = direction is TextDirection.RTL ? Direction.RightToLeft : Direction.LeftToRight;
            buffer.Language = language;
            buffer.AddUtf32(context.AsSpan(), offset, text.Length);
            buffer.GuessSegmentProperties();
            font.Shape(buffer, features);
            return buffer.GetGlyphInfoSpan();
        }

        Typeset.Run Replacement() {
            _ = font.TryGetGlyph(style.ReplacementCharacter, out uint glyph);
            float advance = (font.GetHorizontalGlyphAdvance(glyph) * PerUnit(typeface, style.FontSize)) + style.LetterSpacing;
            bool rightToLeft = direction is TextDirection.RTL;
            string value = Utf32Utils.FromUtf32(text);
            Utf32Buffer decoded = new(value);
            Seq<(int Start, int End)> graphemes = Segments(shaping.Graphemes, value).Map(held => (decoded.Utf16OffsetToUtf32Offset(held.Start), decoded.Utf16OffsetToUtf32Offset(held.End))).Strict();
            float[] boundaries = new float[text.Length + 1];
            for (int index = 0; index < graphemes.Count; index++)
                boundaries.AsSpan(graphemes[index].Start, graphemes[index].End - graphemes[index].Start).Fill((rightToLeft ? graphemes.Count - index : index) * advance);
            boundaries[^1] = rightToLeft ? 0f : graphemes.Count * advance;
            using SKFont replacement = new(typeface, style.FontSize);
            return new Typeset.Run(text, style, typeface, direction,
                Enumerable.Repeat((ushort)glyph, graphemes.Count).ToArray(),
                Enumerable.Range(0, graphemes.Count).Select(index => new SKPoint(index * advance, 0f)).ToArray(),
                Enumerable.Range(0, graphemes.Count).Select(index => text.Start + graphemes[rightToLeft ? graphemes.Count - index - 1 : index].Start).ToArray(),
                boundaries, [], replacement.Metrics, Kind: kind);
        }

        static uint[] GlyphIds(ReadOnlySpan<GlyphInfo> infos) {
            uint[] glyphs = new uint[infos.Length];
            for (int index = 0; index < infos.Length; index++) glyphs[index] = infos[index].Codepoint;
            return glyphs;
        }
    }

    internal static Option<int> Position(Font font, OpenTypeMetricsTag tag) => font.OpenTypeMetrics.TryGetPosition(tag, out int units) ? Some(units) : None;

    internal static float PerUnit(SKTypeface typeface, float size) => size / typeface.UnitsPerEm;

    // --- [OUTLINES]
    internal static SKRect Bounds(Typeset set) {
        using SKTextBlobBuilder builder = new();
        foreach ((Typeset.Run run, SKPoint origin) in GlyphRuns(set)) {
            using SKFont font = new(run.Typeface, run.FontSize);
            SKRawRunBuffer<SKPoint> allocated = builder.AllocateRawPositionedRun(font, run.Glyphs.Length);
            run.Glyphs.Span.CopyTo(allocated.Glyphs);
            for (int index = 0; index < run.Positions.Length; index++) allocated.Positions[index] = run.Positions.Span[index] + origin;
        }
        using SKTextBlob? blob = builder.Build();
        return blob?.Bounds ?? SKRect.Empty;
    }

    internal static SKPath Outline(Typeset set) {
        using SKPathBuilder builder = new();
        foreach ((Typeset.Run run, SKPoint origin) in GlyphRuns(set)) {
            using SKFont font = new(run.Typeface, run.FontSize);
            using SKPath glyphs = font.GetTextPath(MemoryMarshal.AsBytes(run.Glyphs.Span), SKTextEncoding.GlyphId, run.Positions.Span);
            builder.AddPath(glyphs, origin.X, origin.Y);
        }
        return builder.Detach();
    }

    internal static Option<SKTextBlob> Along(Typeset set, SKPath path, float offset) {
        using SKTextBlobBuilder builder = new();
        using SKPathMeasure measure = new(path);
        float baseline = set.At(Baseline.First).IfNone(0f);
        foreach ((Typeset.Run run, SKPoint origin) in GlyphRuns(set)) {
            ReadOnlySpan<SKPoint> at = run.Positions.Span;
            float[] widths = new float[at.Length];
            SKPoint[] offsets = new SKPoint[at.Length];
            for (int index = 0; index < at.Length; index++) {
                widths[index] = (index + 1 < at.Length ? at[index + 1].X : run.Origin + run.Width) - at[index].X;
                offsets[index] = new SKPoint(at[index].X - run.Origin, at[index].Y + origin.Y - baseline);
            }
            using SKPath along = measure.GetSegment(offset + origin.X + run.Origin, measure.Length, startWithMoveTo: true);
            using SKFont font = new(run.Typeface, run.FontSize);
            builder.AddPathPositionedRun(run.Glyphs.Span, font, widths, offsets, along);
        }
        return Optional(builder.Build());
    }

    private static IEnumerable<(Typeset.Run Run, SKPoint Origin)> GlyphRuns(Typeset set) {
        float lean = Alignment(set.Text.Alignment, set.Source.BaseDirection);
        return from line in set.Lines.AsEnumerable()
               let metrics = line.Metrics
               from held in line.Runs.AsEnumerable()
               where held.Run.Kind is not FontRunKind.TrailingWhitespace && held.Run.Glyphs.Length > 0
               select (held.Run, new SKPoint(held.X - held.Run.Origin + (held.Run.Style.LetterSpacing * lean), line.Y + metrics.Baseline));
    }

    // --- [PAINT]
    internal static Option<SKTextBlob> Blob(Typeset.Run run, TextPaintOptions raster) {
        using SKFont font = new(run.Typeface, run.FontSize) { Edging = raster.Edging, Hinting = raster.Hinting, Subpixel = raster.SubpixelPositioning };
        using SKTextBlobBuilder builder = new();
        builder.AddPositionedRun(run.Glyphs.Span, font, run.Positions.Span);
        return Optional(builder.Build());
    }

    internal static void Paint(SKCanvas canvas, Typeset set, SKPoint origin, TextPaintOptions raster, SKColorF tint, Func<Typeset.Run, Option<SKTextBlob>> blobs) {
        using SKAutoCanvasRestore restore = new(canvas, doSave: true);
        canvas.Translate(origin);
        using SKPaint background = new();
        using SKPaint handle = new() { Color = raster.SelectionHandleColor };
        foreach (Typeset.Line line in set.Lines) {
            float height = line.Metrics.Height;
            foreach ((Typeset.Run run, float x) in line.Runs) {
                if (run.Kind is FontRunKind.Normal && run.Style.BackgroundColor != SKColors.Transparent) {
                    background.ColorF = run.Style.BackgroundColor;
                    canvas.DrawRect(SKRect.Create(x, line.Y, run.Width, height), background);
                }
                if (set.Selection.Case is not (TextRange range, SKColorF color) || run.Kind is FontRunKind.Ellipsis || range.Maximum <= run.Text.Start || range.Minimum >= run.End) continue;
                float from = x + run.Caret(int.Max(run.Text.Start, range.Minimum));
                float to = x + run.Caret(int.Min(run.End, range.Maximum));
                background.ColorF = color;
                canvas.DrawRect(new SKRect(float.Min(from, to), line.Y, float.Max(from, to), line.Y + height), background);
                if (raster.SelectionHandleScale <= 0f) continue;
                float scale = raster.SelectionHandleScale;
                if (range.Minimum >= run.Text.Start && range.Minimum < run.End) Handle(new SKPoint(from, line.Y));
                if (range.Maximum > run.Text.Start && range.Maximum <= run.End) Handle(new SKPoint(to, line.Y + height));

                void Handle(SKPoint edge) {
                    canvas.DrawRect(new SKRect(edge.X - scale, line.Y, edge.X + scale, line.Y + height), handle);
                    canvas.DrawCircle(edge, 5f * scale, handle);
                }
            }
        }
        using SKPaint ink = new() { IsAntialias = raster.Edging is not SKFontEdging.Alias };
        using SKPaint halo = new() { Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Square, StrokeJoin = SKStrokeJoin.Round, IsAntialias = ink.IsAntialias };
        foreach ((Typeset.Run run, SKPoint at) in GlyphRuns(set)) {
            if (blobs(run).Case is not SKTextBlob blob) continue;
            ink.ColorF = run.Style.TextColor == set.Style.TextColor ? tint : run.Style.TextColor;
            ink.PathEffect = null;
            halo.ColorF = run.Style.HaloColor;
            halo.StrokeWidth = run.Style.HaloWidth;
            halo.PathEffect = null;
            using SKMaskFilter? blur = run.Style.HaloBlur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, run.Style.HaloBlur) : null;
            halo.MaskFilter = blur;
            if (run.Style.HaloColor != SKColors.Transparent) canvas.DrawText(blob, at.X, at.Y, halo);
            canvas.DrawText(blob, at.X, at.Y, ink);
            if (run.Kind is FontRunKind.Normal) {
                float inset = run.Style.LetterSpacing * Alignment(set.Text.Alignment, set.Source.BaseDirection);
                Decorated(run, blob, at - new SKPoint(inset, 0f), inset);
            }
        }

        void Decorated(Typeset.Run run, SKTextBlob blob, SKPoint at, float inset) {
            if (run.Style.StrikeThrough is not StrikeThroughStyle.None)
                Rule((run.Metrics.StrikeoutPosition ?? 0f) - run.Rise, float.Max(1f, run.Metrics.StrikeoutThickness ?? 1f), dashed: false);
            if (run.Style.Underline is UnderlineStyle.None) return;
            float thickness = float.Max(1f, run.Metrics.UnderlineThickness ?? 1f);
            float y = float.Max(1f, run.Metrics.UnderlinePosition ?? 0f) - run.Rise;
            if (run.Style.Underline is not UnderlineStyle.Gapped) {
                Rule(y, run.Style.Underline is UnderlineStyle.ImeTargetConverted ? thickness * 2f : thickness, run.Style.Underline is UnderlineStyle.ImeInput or UnderlineStyle.ImeConverted);
                return;
            }
            float[] gaps = blob.GetIntercepts(y - (thickness / 2f), y + thickness);
            float left = run.Origin;
            for (int index = 0; index < gaps.Length; index += 2) {
                Stroke(left, gaps[index] + inset - thickness, y, thickness);
                left = gaps[index + 1] + inset + thickness;
            }
            Stroke(left, run.Origin + run.Width, y, thickness);

            void Rule(float position, float weight, bool dashed) {
                using SKPathEffect? dash = dashed ? SKPathEffect.CreateDash([weight, weight], weight) : null;
                ink.PathEffect = dash;
                halo.PathEffect = dash;
                Stroke(run.Origin, run.Origin + run.Width, position, weight);
            }

            void Stroke(float from, float to, float position, float weight) {
                if (from >= to) return;
                ink.StrokeWidth = weight;
                halo.StrokeWidth = weight + run.Style.HaloWidth;
                SKPoint start = new(at.X + from, at.Y + position);
                SKPoint end = new(at.X + to, at.Y + position);
                if (run.Style.HaloColor != SKColors.Transparent) canvas.DrawLine(start, end, halo);
                canvas.DrawLine(start, end, ink);
            }
        }
    }
}
