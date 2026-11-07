using System.Globalization;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SuperLinq;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Decoration(int Line) {
    public Option<System.Range> Range { get; init; }
    public Option<Key> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }
    public Option<Key> Glyph { get; init; }
    public Option<string> Help { get; init; }
}

public sealed record Folding(Seq<System.Range> Ranges, HashSet<Key> Closed, Func<Edit<HashSet<Key>>, IO<Unit>> Toggled);

public sealed record CodeView(Seq<string> Lines, Func<Seq<Key>, Part.Choice> Choice) {

    // --- [STATE]

    public Func<string, Seq<Span>> Tokens { get; init; } = static line => [line];
    public Seq<Decoration> Decorations { get; init; }
    public Search Find { get; init; } = Search.Everything;
    public Option<Folding> Folding { get; init; }
    public bool Wrap { get; init; }
    public bool Numbered { get; init; } = true;
    public HashSet<Key> Realized { get; init; }

    private Seq<System.Range> Folds => Folding.Map(folding => folding.Ranges.Map(range => Clamped(range, Lines.Count))).IfNone([]);

    // --- [DRAWING]

    public View Drawn(CultureInfo culture) {
        HashSet<int> hidden = Folding.Map(folding => toHashSet(Folds.Filter(fold => folding.Closed.Contains(Line(fold.Start.Value))).Bind(static fold => Spanned((fold.Start.Value + 1)..fold.End)))).IfNone([]);
        return Drawn(culture, toSeq(Range(0, Lines.Count)).Filter(line => !hidden.Contains(line)).Map(static line => Some(line)), None);
    }

    internal static System.Range Clamped(System.Range range, int length) =>
        int.Clamp(range.Start.GetOffset(length), 0, length) switch { var start => start..int.Clamp(range.End.GetOffset(length), start, length) };

    internal static Seq<int> Spanned(System.Range lines) => toSeq(Range(lines.Start.Value, lines.End.Value - lines.Start.Value));

    internal static View Number(Option<int> number, int count, CultureInfo culture) =>
        new View.Overlay(new View.Text([count.ToString(culture)]) { Opacity = Fraction.MinValue }, number.Map(at => (View)new View.Text([at.ToString(culture)]) { Align = new UnitPoint(Fraction.MaxValue, Fraction.Half) }).ToSeq());

    // --- [ROWS]

    internal View Drawn(CultureInfo culture, Seq<Option<int>> shown, Option<Seq<Key>> follows) {
        Seq<Row<Option<int>>> rows = shown.Map((line, index) => new Row<Option<int>>(line, Line(line.IfNone(Lines.Count + index)), 0, None)).Strict();
        HashMap<Key, Option<int>> lines = toHashMap(rows.Map(static row => (row.Key, row.Item)));
        Part.Choice choice = Choice(rows.Map(static row => row.Key));
        HashMap<int, Seq<Decoration>> marks = toHashMap(Decorations.GroupBy(static mark => mark.Line).Select(static group => (group.Key, toSeq(group))));
        HashSet<int> folds = toHashSet(Folds.Map(static fold => fold.Start.Value));
        bool glyphs = Decorations.Exists(static mark => mark.Glyph.IsSome);
        View body = Row<Option<int>>.Drawn(rows, _ => choice, row => Lined(culture, choice.Value.Holds(row.Key), row.Item, marks, folds, glyphs), Realized, Wrap) with {
            Lift = Lift.Text(key => lines.Find(key).Bind(static line => line).Bind(line => Lines.At(line))),
        };
        View vertical = new View.Scroll(PhysicalAxis.Vertical, body) { Follows = follows }.Grown;
        return Wrap ? vertical : new View.Scroll(PhysicalAxis.Horizontal, vertical).Grown;
    }

    private static Key.Ordinal Line(int index) => new((uint)index);

    private View.Flex Lined(CultureInfo culture, bool current, Option<int> line, HashMap<int, Seq<Decoration>> marks, HashSet<int> folds, bool glyphs) {
        Seq<Decoration> here = line.Bind(marks.Find).IfNone([]);
        Seq<string> helps = here.Choose(static mark => mark.Help);
        Seq<View> gutter = (Numbered ? Seq(Number(line.Map(static at => at + 1), Lines.Count, culture)) : [])
            + (glyphs
                ? Seq(here.Find(static mark => mark.Glyph.IsSome).Match(
                    Some: static mark => (View)new View.Box { Role = mark.Glyph, Conditions = mark.Conditions },
                    None: static () => new View.Box { Role = Roles.Status, Opacity = Fraction.MinValue }))
                : [])
            + Folding.Map(folding => Disclosure.Chevron(
                line.Filter(folds.Contains).Map(start => !folding.Closed.Contains(Line(start))),
                line.Map(start => folding.Toggled(Edit.Of(EditPhase.Commit, folding.Closed.SymmetricExcept([Line(start)])))))).ToSeq();
        return new View.Flex(PhysicalAxis.Horizontal, [
            new View.Flex(PhysicalAxis.Horizontal, gutter) {
                Role = Roles.Gutter,
                Conditions = current ? [Condition.Selected] : [],
                Height = Sizing.Grow.Unit,
                Sticks = Wrap ? None : Some(PhysicalAxis.Horizontal),
            },
            new View.Text(line.Map(at => Spans(culture, at, here)).IfNone([])) { MaxLines = Wrap ? None : Some(1), Width = Sizing.Grow.Unit },
        ]) {
            Role = Roles.Line,
            Width = Sizing.Grow.Unit,
            Conditions = toHashSet(here.Filter(static mark => mark.Range.IsNone).Bind(static mark => toSeq(mark.Conditions))),
            Accessible = new Accessible { Help = helps.IsEmpty ? None : Some(string.Join('\n', helps)) },
        };
    }

    private Seq<Span> Spans(CultureInfo culture, int line, Seq<Decoration> here) {
        string text = Lines[line];
        Seq<(int Start, int End, Decoration Mark)> overlaid = (here + Find.Hits(text, culture).IfNone([]).Map(hit => new Decoration(line) { Range = hit, Role = Roles.Mark })).Choose(mark => mark.Range.Map(range => Clamped(range, text.Length) switch {
            var clamped => (Start: clamped.Start.Value, End: clamped.End.Value, Mark: mark),
        })).Strict();
        Seq<Span> pieces = Span.Cut(Tokens(text), overlaid.Bind(static mark => Seq(mark.Start, mark.End)));
        return pieces.Zip(pieces.Scan(0, static (at, piece) => at + piece.Text.Length)).Map(held =>
            overlaid.Filter(mark => mark.Start <= held.Second && mark.End >= held.Second + held.First.Text.Length).Map(static mark => mark.Mark) switch {
                var over => held.First with {
                    Role = over.Choose(static mark => mark.Role).Last | held.First.Role,
                    Conditions = over.Fold(held.First.Conditions, static (conditions, mark) => conditions.Union(mark.Conditions)),
                },
            });
    }
}

public sealed record Diff(Key Key, CodeView Original, CodeView Modified, Seq<(System.Range Original, System.Range Modified)> Hunks) {
    public View Drawn(CultureInfo culture) {
        (int originals, int modifieds) = (Original.Lines.Count, Modified.Lines.Count);
        static Seq<(Option<int> Left, Option<int> Right)> Paired(System.Range left, System.Range right) =>
            toSeq(CodeView.Spanned(left).Map(static line => Some(line)).ZipLongest(CodeView.Spanned(right).Map(static line => Some(line)), static (from, to) => (Left: from, Right: to)));
        (int Original, int Modified, Seq<(Option<int> Left, Option<int> Right)> Rows) walked = Hunks.Fold((Original: 0, Modified: 0, Rows: Seq<(Option<int> Left, Option<int> Right)>()), (held, hunk) =>
            (CodeView.Clamped(hunk.Original, originals), CodeView.Clamped(hunk.Modified, modifieds)) switch {
                var (original, modified) => (original.End.Value, modified.End.Value, held.Rows + Paired(held.Original..original.Start, held.Modified..modified.Start) + Paired(original, modified)),
            });
        Seq<(Option<int> Left, Option<int> Right)> rows = walked.Rows + Paired(walked.Original..originals, walked.Modified..modifieds);
        Option<Seq<Key>> linked = Some(Seq(Key));
        return new View.Flex(PhysicalAxis.Horizontal, [
            Changed(Original, static hunk => hunk.Original, Condition.Faulted).Drawn(culture, rows.Map(static row => row.Left), linked),
            View.Divider(PhysicalAxis.Horizontal),
            Changed(Modified, static hunk => hunk.Modified, Condition.Success).Drawn(culture, rows.Map(static row => row.Right), linked),
        ]) {
            Key = Key,
        }.Grown;
    }

    private CodeView Changed(CodeView side, Func<(System.Range Original, System.Range Modified), System.Range> lines, Condition change) =>
        side with { Decorations = side.Decorations + Hunks.Bind(hunk => CodeView.Spanned(CodeView.Clamped(lines(hunk), side.Lines.Count))).Map(line => new Decoration(line) { Conditions = [change] }) };
}

public sealed record Document(string Text) {

    // --- [STATE]

    public Option<Func<Uri, IO<Unit>>> Followed { get; init; }
    public Func<Option<string>, string, Seq<Span>> Tokens { get; init; } = static (_, line) => [line];

    // --- [DRAWING]

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();

    public View Drawn(CultureInfo culture) => Stacked(MarkdownParser.Parse(Text, Pipeline), culture);

    // --- [BLOCKS]

    private const string Bullet = "•";

    private View.Flex Stacked(ContainerBlock container, CultureInfo culture) =>
        new(PhysicalAxis.Vertical, toSeq(container.AsEnumerable()).Choose(block => Drawn(block, culture))) { Role = Roles.Stack, Width = Sizing.Grow.Unit };

    private Option<View> Drawn(Block block, CultureInfo culture) =>
        block switch {
            HeadingBlock heading => new View.Text(Inlines(heading.Inline)) { Role = heading.Level > 1 ? Roles.Headline : Roles.Title, Accessible = new Accessible { Role = AccessRole.Heading } },
            ParagraphBlock paragraph => new View.Text(Inlines(paragraph.Inline)) { Width = Sizing.Grow.Unit },
            CodeBlock code => Fenced(code),
            HtmlBlock html => new View.Text([html.Lines.ToString()]) { Role = Roles.Code, Width = Sizing.Grow.Unit },
            ThematicBreakBlock => View.Divider(PhysicalAxis.Vertical),
            LeafBlock leaf => new View.Text(Inlines(leaf.Inline)) { Width = Sizing.Grow.Unit },
            QuoteBlock quote => new View.Flex(PhysicalAxis.Horizontal, [View.Divider(PhysicalAxis.Horizontal), Stacked(quote, culture)]) { Role = Roles.Stack, Width = Sizing.Grow.Unit },
            ListBlock list => Listed(list, culture),
            Table table => Tabled(table, culture),
            ContainerBlock container => Stacked(container, culture),
            _ => Option<View>.None,
        };

    private View.Flex Fenced(CodeBlock code) {
        Option<string> language = Optional(code as FencedCodeBlock).Bind(static fenced => Optional(fenced.Info)).Filter(static info => info.Length > 0);
        Seq<Span> spans = toSeq(code.Lines.Lines.Take(code.Lines.Count)).Map((line, index) => (index > 0 ? Seq<Span>("\n") : []) + Tokens(language, line.Slice.ToString())).Bind(static line => line);
        return new View.Flex(PhysicalAxis.Vertical, [new View.Text(spans) { Role = Roles.Code }]) { Role = Roles.Section, Width = Sizing.Grow.Unit };
    }

    private View.Flex Listed(ListBlock list, CultureInfo culture) {
        int first = Optional(list.OrderedStart).Bind(parseInt).IfNone(1);
        return new View.Flex(PhysicalAxis.Vertical, toSeq(list.OfType<ListItemBlock>()).Map((item, index) => (View)new View.Flex(PhysicalAxis.Horizontal, [
            new View.Text([list.IsOrdered ? string.Create(culture, $"{first + index}{list.OrderedDelimiter}") : Bullet]),
            Stacked(item, culture),
        ]) { Role = Roles.Stack, Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.ListItem } })) {
            Role = Roles.Stack,
            Width = Sizing.Grow.Unit,
            Accessible = new Accessible { Role = AccessRole.List },
        };
    }

    private View.Grid Tabled(Table table, CultureInfo culture) {
        Seq<TableColumnDefinition> columns = toSeq(table.ColumnDefinitions);
        return new View.Grid(new Tracks(columns.Map(static _ => (Sizing)Sizing.Fit.Content)), toSeq(table.OfType<TableRow>()).Map(row => toSeq(row.OfType<TableCell>()).Map(cell => (View)(Stacked(cell, culture) with {
            Role = row.IsHeader ? Roles.Header : Roles.Cell,
            Width = None,
            Align = new UnitPoint(columns.At(cell.ColumnIndex).Bind(static column => Optional(column.Alignment)).Map(static align => align switch {
                TableColumnAlign.Center => Fraction.Half,
                TableColumnAlign.Right => Fraction.MaxValue,
                _ => Fraction.MinValue,
            }).IfNone(Fraction.MinValue), Fraction.MinValue),
            Accessible = new Accessible { Role = AccessRole.GridCell },
        })))) {
            Accessible = new Accessible { Role = AccessRole.Grid },
        };
    }

    // --- [INLINES]

    private Seq<Span> Inlines(ContainerInline? container) => Optional(container).Map(held => Inlined(held, new Span(""))).IfNone([]);

    private Seq<Span> Inlined(ContainerInline container, Span style) =>
        toSeq(container.AsEnumerable()).Bind(inline => inline switch {
            LiteralInline literal => Seq(style with { Text = literal.Content.ToString() }),
            CodeInline code => Seq(style with { Text = code.Content, Role = Roles.Code }),
            EmphasisInline emphasis => Inlined(emphasis, style with { Role = emphasis.DelimiterCount > 1 ? Roles.Strong : Roles.Emphasis }),
            LinkInline link => Inlined(link, Linked(style, link.Url)),
            AutolinkInline automatic => Seq(Linked(style with { Text = automatic.Url }, automatic.Url)),
            LineBreakInline line => Seq(style with { Text = line.IsHard ? "\n" : " " }),
            HtmlEntityInline entity => Seq(style with { Text = entity.Transcoded.ToString() }),
            HtmlInline html => Seq(style with { Text = html.Tag }),
            ContainerInline nested => Inlined(nested, style),
            _ => Seq<Span>(),
        });

    private Span Linked(Span style, string? url) =>
        Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? uri) ? style with { Role = Roles.Link, Part = Followed.Map(followed => (Part)new Part.Press(_ => followed(uri))) } : style;
}
