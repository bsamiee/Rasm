using System.Diagnostics.Tracing;
using System.Globalization;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using NodaTime.Text;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using SuperLinq;

namespace Rasm.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Decoration(int Line) {
    public Option<System.Range> Range { get; init; }
    public Option<Role> Role { get; init; }
    public HashSet<Condition> Conditions { get; init; }
    public Option<Role> Glyph { get; init; }
    public Option<string> Help { get; init; }
}

public sealed record Folding(Seq<System.Range> Ranges, HashSet<Key> Closed, Func<Edit<HashSet<Key>>, IO<Unit>> Toggled);

public sealed record Hunk(System.Range Original, System.Range Modified);

public sealed record Source(Seq<string> Lines, Func<Seq<Key>, Part.Choice> Choice) {
    private const char Figure = '\u2007';

    public Func<string, Seq<Span>> Tokens { get; init; } = static line => [line];
    public Seq<Decoration> Decorations { get; init; }
    public Option<Search> Find { get; init; }
    public Option<Folding> Folding { get; init; }
    public bool Wrap { get; init; }
    public bool Numbered { get; init; } = true;
    public HashSet<Key> Realized { get; init; }

    private Seq<(int Start, int End)> Folds => Folding.Map(folding => folding.Ranges.Map(range => Sources.Clamped(range, Lines.Count))).IfNone([]);

    public Node Drawn(CultureInfo culture) {
        HashSet<int> hidden = Folding.Map(folding => toHashSet(Folds.Filter(fold => folding.Closed.Contains(Sources.Line(fold.Start))).Bind(static fold => Sources.Spanned((fold.Start + 1, fold.End))))).IfNone([]);
        return Laid(culture, toSeq(Range(0, Lines.Count)).Filter(line => !hidden.Contains(line)).Map(static line => Some(line)), None);
    }

    // --- [ROWS]
    internal Node Laid(CultureInfo culture, Seq<Option<int>> rows, Option<Seq<Key>> follows) {
        Seq<(Key Key, Option<int> Line)> keyed = rows.Map((line, index) => (line.Map(Sources.Line).IfNone(() => Sources.Line(Lines.Count + index)), line)).Strict();
        HashMap<Key, Option<int>> lines = toHashMap(keyed);
        Part.Choice choice = Choice(keyed.Map(static held => held.Key));
        HashMap<int, Seq<Decoration>> marks = toHashMap(Decorations.GroupBy(static mark => mark.Line).Select(static group => (group.Key, toSeq(group))));
        HashSet<int> folds = toHashSet(Folds.Map(static fold => fold.Start));
        int digits = Lines.Count.ToString(culture).Length;
        bool glyphs = Decorations.Exists(static mark => mark.Glyph.IsSome);
        Node body = Sources.Listed(choice, key => Row(culture, choice.Value.Holds(key), lines[key], marks, folds, digits, glyphs), Wrap, Role.Line, Realized) with {
            Lift = Sources.Copied(key => lines.Find(key).Bind(static line => line).Bind(line => Lines.At(line))),
        };
        Node vertical = new Node.Scroll(PhysicalAxis.Vertical, body) { Follows = follows, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit };
        return Wrap ? vertical : new Node.Scroll(PhysicalAxis.Horizontal, vertical) { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit };
    }

    private Node.Flex Row(CultureInfo culture, bool current, Option<int> line, HashMap<int, Seq<Decoration>> marks, HashSet<int> folds, int digits, bool glyphs) {
        Seq<Decoration> here = line.Bind(marks.Find).IfNone([]);
        Seq<Node> gutter = (Numbered ? Seq<Node>(new Node.Text([line.Map(at => (at + 1).ToString(culture)).IfNone("").PadLeft(digits, Figure)])) : [])
            + (glyphs ? Seq(here.Find(static mark => mark.Glyph.IsSome).Map(static mark => (Node)new Node.Box { Role = mark.Glyph, Conditions = mark.Conditions }).IfNone(static () => Sources.Spacer(Role.Status))) : [])
            + Folding.Map(folding => line.Filter(folds.Contains).Map(start => Folded(folding, start)).IfNone(static () => Sources.Spacer(Role.Chevron))).ToSeq();
        return new Node.Flex(PhysicalAxis.Horizontal, [
            new Node.Flex(PhysicalAxis.Horizontal, gutter) {
                Role = Role.Gutter,
                Conditions = current ? [Condition.Selected] : [],
                Height = Sizing.Grow.Unit,
                Sticks = Wrap ? None : Some(PhysicalAxis.Horizontal),
            },
            new Node.Text(line.Map(at => Spans(culture, at, here)).IfNone([])) { MaxLines = Wrap ? None : Some(1), Width = Sizing.Grow.Unit },
        ]) {
            Role = Role.Line,
            Width = Sizing.Grow.Unit,
            Conditions = toHashSet(here.Filter(static mark => mark.Range.IsNone).Bind(static mark => toSeq(mark.Conditions))),
            Accessible = new Accessible { Help = Sources.Joined(here.Choose(static mark => mark.Help)) },
        };
    }

    private Seq<Span> Spans(CultureInfo culture, int line, Seq<Decoration> here) {
        string text = Lines[line];
        Seq<(int Start, int End, Decoration Mark)> hits = Find.Map(query => query.Marked(text, culture).Fold((At: 0, Hits: Seq<(int Start, int End, Decoration Mark)>()), (held, span) =>
            (held.At + span.Text.Length, span.Role == Some(Role.Mark) ? held.Hits.Add((held.At, held.At + span.Text.Length, new Decoration(line) { Role = Role.Mark })) : held.Hits)).Hits).IfNone([]);
        return Sources.Overlaid(Tokens(text), here.Choose(mark => mark.Range.Map(range => Sources.Clamped(range, text.Length) switch { var (start, end) => (Start: start, End: end, Mark: mark) })) + hits);
    }

    private static Node Folded(Folding folding, int start) {
        Key key = Sources.Line(start);
        bool open = !folding.Closed.Contains(key);
        return new Part.Press(_ => folding.Toggled(Edit.Of(EditPhase.Commit, folding.Closed.SymmetricExcept([key])))).Drawn(
            new Node.Box { Role = Role.Chevron, Conditions = open ? [Condition.Selected] : [], Accessible = new Accessible { Role = AccessRole.Disclosure, Expanded = open } });
    }
}

public sealed record Pair(Key Key, Source Original, Source Modified, Seq<Hunk> Hunks) {
    public Node Drawn(CultureInfo culture) {
        (Seq<Option<int>> left, Seq<Option<int>> right) = Aligned;
        Option<Seq<Key>> linked = Some(Seq(Key));
        return new Node.Flex(PhysicalAxis.Horizontal, [
            Changed(Original, static hunk => hunk.Original, Condition.Faulted).Laid(culture, left, linked),
            new Node.Box { Role = Role.Divider, Height = Sizing.Grow.Unit },
            Changed(Modified, static hunk => hunk.Modified, Condition.Success).Laid(culture, right, linked),
        ]) {
            Key = Key,
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };
    }

    private (Seq<Option<int>> Left, Seq<Option<int>> Right) Aligned {
        get {
            (int Original, int Modified, Seq<Option<int>> Left, Seq<Option<int>> Right) walked = Hunks.Fold((Original: 0, Modified: 0, Left: Seq<Option<int>>(), Right: Seq<Option<int>>()), (held, hunk) => {
                (int Start, int End) original = Sources.Clamped(hunk.Original, Original.Lines.Count);
                (int Start, int End) modified = Sources.Clamped(hunk.Modified, Modified.Lines.Count);
                (Seq<Option<int>> Left, Seq<Option<int>> Right) kept = Paired((held.Original, original.Start), (held.Modified, modified.Start));
                (Seq<Option<int>> Left, Seq<Option<int>> Right) changed = Paired(original, modified);
                return (original.End, modified.End, held.Left + kept.Left + changed.Left, held.Right + kept.Right + changed.Right);
            });
            (Seq<Option<int>> Left, Seq<Option<int>> Right) rest = Paired((walked.Original, Original.Lines.Count), (walked.Modified, Modified.Lines.Count));
            return (walked.Left + rest.Left, walked.Right + rest.Right);
        }
    }

    private Source Changed(Source side, Func<Hunk, System.Range> lines, Condition change) =>
        side with { Decorations = side.Decorations + Hunks.Bind(hunk => Sources.Spanned(Sources.Clamped(lines(hunk), side.Lines.Count))).Map(line => new Decoration(line) { Conditions = [change] }) };

    private static (Seq<Option<int>> Left, Seq<Option<int>> Right) Paired((int Start, int End) left, (int Start, int End) right) {
        (Seq<int> from, Seq<int> to) = (Sources.Spanned(left), Sources.Spanned(right));
        int rows = int.Max(from.Count, to.Count);
        return (Padded(from, rows), Padded(to, rows));
    }

    private static Seq<Option<int>> Padded(Seq<int> lines, int rows) => lines.Map(static line => Some(line)) + toSeq(Range(0, rows - lines.Count)).Map(static _ => Option<int>.None);
}

public sealed record ValueTree<TItem>(Seq<TItem> Roots, Func<TItem, Key> Keyed, Func<TItem, Seq<TItem>> Children, Func<TItem, string> Name, Func<TItem, string> Value, Func<Seq<Key>, Part.Choice> Choice)
    where TItem : notnull {
    public Func<TItem, Option<string>> Kind { get; init; } = static _ => None;
    public HashSet<Key> Open { get; init; }
    public Func<Edit<HashSet<Key>>, IO<Unit>> Toggled { get; init; } = static _ => IO.pure(unit);
    public Option<Search> Find { get; init; }
    public Option<Func<Seq<TItem>, string>> Path { get; init; }
    public HashSet<Key> Realized { get; init; }

    public HashSet<Key> Opened(int depth) => toHashSet(Within(Roots, depth));

    public Node Drawn(CultureInfo culture) {
        Seq<Row<TItem>> rows = Row<TItem>.Flatten(Roots, Children, Keyed, Find.IsSome ? Opened(int.MaxValue) : Open,
            item => Find.ForAll(query => query.Matches(Name(item), culture) || query.Matches(Value(item), culture)));
        HashMap<Key, Seq<TItem>> chains = toHashMap(rows.Zip(rows.Scan(Seq<TItem>(), static (chain, row) => chain.Take(row.Depth).Add(row.Item)).Tail).Map(static held => (held.First.Key, held.Second)));
        return new Node.Scroll(PhysicalAxis.Vertical, Row<TItem>.Drawn(rows, Choice, row => Face(row, culture), Realized) with {
            Lift = Sources.Copied(key => chains.Find(key).Bind(chain => Path.Match(Some: path => Some(path(chain)), None: () => chain.Last.Map(Value)))),
            Accessible = new Accessible { Role = AccessRole.Tree },
        }) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };
    }

    private Seq<Key> Within(Seq<TItem> items, int depth) =>
        depth > 0 ? items.Bind(item => Children(item) switch { var below => below.IsEmpty ? [] : Keyed(item).Cons(Within(below, depth - 1)) }) : [];

    private Node.Flex Face(Row<TItem> row, CultureInfo culture) {
        Seq<TItem> below = Children(row.Item);
        return new Node.Flex(PhysicalAxis.Horizontal,
            toSeq(Range(0, row.Depth)).Map(static _ => Sources.Spacer(Role.Chevron))
            + Seq(
                row.Open.Map(open => new Part.Press(_ => Toggled(Edit.Of(EditPhase.Commit, Open.SymmetricExcept([row.Key])))).Drawn(new Node.Box { Role = Role.Chevron, Conditions = open ? [Condition.Selected] : [] }))
                    .IfNone(static () => Sources.Spacer(Role.Chevron)),
                new Node.Text(Sources.Marked(Find, Name(row.Item), culture)) { MaxLines = 1 },
                new Node.Text(Sources.Marked(Find, Value(row.Item), culture)) { Role = Role.Code, MaxLines = 1, Width = Sizing.Grow.Unit })
            + Kind(row.Item).Map(static kind => (Node)new Node.Text([kind]) { Role = Role.Caption }).ToSeq()
            + (below.IsEmpty ? [] : Seq<Node>(new Node.Text([below.Count.ToString(culture)]) { Role = Role.Badge }))) {
            Accessible = new Accessible { Role = AccessRole.TreeItem, Label = Name(row.Item), Expanded = row.Open },
        };
    }
}

public sealed record Levels(HashSet<EventLevel> Shown, Func<EventLevel, string> Named, Func<Edit<HashSet<EventLevel>>, IO<Unit>> Edited);

public sealed record Log<TEntry>(Seq<TEntry> Entries, Func<TEntry, Key> Keyed, Func<TEntry, Instant> Time, Func<TEntry, EventLevel> Level, Func<TEntry, string> Message, Func<Seq<Key>, Part.Choice> Choice)
    where TEntry : notnull {
    public Func<TEntry, Option<string>> Source { get; init; } = static _ => None;
    public Func<TEntry, Map<string, string>> Fields { get; init; } = static _ => default;
    public Option<Levels> Levels { get; init; }
    public Option<TextField> Filter { get; init; }
    public Option<LocalTimePattern> Stamp { get; init; }
    public DateTimeZone Zone { get; init; } = DateTimeZone.Utc;
    public bool Grouped { get; init; } = true;
    public bool Wrap { get; init; }
    public bool Follows { get; init; } = true;
    public HashSet<Key> Realized { get; init; }

    public Node Drawn(CultureInfo culture) {
        Option<Search> query = Filter.Map(static field => new Search(field.Text));
        Seq<TEntry> kept = Entries.Filter(entry => Levels.ForAll(levels => levels.Shown.Contains(Level(entry)))
            && query.ForAll(held => held.Matches(Message(entry), culture) || Source(entry).Exists(source => held.Matches(source, culture))));
        Seq<(TEntry Entry, int Count)> runs = Grouped
            ? toSeq(kept.AsIterable().GroupAdjacent(entry => (Level(entry), Source(entry), Message(entry))).Select(static run => (run.First(), run.Count())))
            : kept.Map(static entry => (entry, 1));
        HashMap<Key, (TEntry Entry, int Count)> keyed = toHashMap(runs.Map(run => (Keyed(run.Entry), run)));
        Node rows = Sources.Listed(Choice(runs.Map(run => Keyed(run.Entry))), key => Row(keyed[key], query, culture), Wrap, Role.ListRow, Realized) with {
            Lift = Sources.Copied(key => keyed.Find(key).Map(run => string.Join(' ', Stamp.Map(pattern => Stamped(pattern, run.Entry, culture)).ToSeq() + Source(run.Entry).ToSeq() + Seq(Message(run.Entry))))),
        };
        Seq<Node> bar = Levels.Map(levels => Leveled(levels, culture)).IfNone([]) + Filter.Map(static field => field.Drawn).ToSeq();
        return new Node.Flex(PhysicalAxis.Vertical,
            (bar.IsEmpty ? [] : Seq<Node>(new Node.Flex(PhysicalAxis.Horizontal, bar) { Role = Role.Toolbar, Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.Toolbar } }))
            .Add(new Node.Scroll(PhysicalAxis.Vertical, rows) { KeepsEnd = Follows, Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit })) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        };
    }

    private Seq<Node> Leveled(Levels levels, CultureInfo culture) =>
        toSeq(Entries.GroupBy(Level).OrderBy(static group => group.Key)).Map(group => new Toggle(levels.Shown.Contains(group.Key) ? Check.On : Check.Off,
            edit => levels.Edited(new Edit<HashSet<EventLevel>>(edit.Phase, edit.Value.Map(check => check == Check.On ? levels.Shown.TryAdd(group.Key) : levels.Shown.Remove(group.Key))))) {
            Role = Role.Button,
            Content = [
                new Node.Box { Role = Role.Status, Conditions = Sources.Signal(group.Key) },
                new Node.Text([levels.Named(group.Key)]),
                new Node.Text([group.Count().ToString(culture)]) { Role = Role.Caption },
            ],
        }.Drawn);

    private Node.Flex Row((TEntry Entry, int Count) run, Option<Search> query, CultureInfo culture) {
        HashSet<Condition> signal = Sources.Signal(Level(run.Entry));
        return new Node.Flex(PhysicalAxis.Horizontal,
            Stamp.Map(pattern => (Node)new Node.Text([Stamped(pattern, run.Entry, culture)]) { Role = Role.Code }).ToSeq()
            + Seq(signal.IsEmpty ? Sources.Spacer(Role.Status) : new Node.Box { Role = Role.Status, Conditions = signal })
            + Source(run.Entry).Map(source => (Node)new Node.Text(Sources.Marked(query, source, culture)) { Role = Role.Caption, MaxLines = 1 }).ToSeq()
            + Seq<Node>(new Node.Text(Sources.Marked(query, Message(run.Entry), culture)) { MaxLines = Wrap ? None : Some(1), Width = Sizing.Grow.Unit })
            + Fields(run.Entry).ToSeq().Map(static field => (Node)new Node.Flex(PhysicalAxis.Horizontal, [new Node.Text([field.Key]) { Role = Role.Caption }, new Node.Text([field.Value]) { Role = Role.Code }]) { Role = Role.Chip })
            + (run.Count > 1 ? Seq<Node>(new Node.Text([run.Count.ToString(culture)]) { Role = Role.Badge }) : [])) {
            Role = Role.ListRow,
            Width = Sizing.Grow.Unit,
            Conditions = signal,
        };
    }

    private string Stamped(LocalTimePattern pattern, TEntry entry, CultureInfo culture) => pattern.WithCulture(culture).Format(Time(entry).InZone(Zone).TimeOfDay);
}

public sealed record Document(string Text) {
    private const string Bullet = "\u2022";
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();

    public Func<Uri, IO<Unit>> Followed { get; init; } = static _ => IO.pure(unit);
    public Func<Option<string>, string, Seq<Span>> Tokens { get; init; } = static (_, line) => [line];

    public Node Drawn(CultureInfo culture) => Stacked(MarkdownParser.Parse(Text, Pipeline), culture);

    // --- [BLOCKS]
    private Node.Flex Stacked(ContainerBlock container, CultureInfo culture) =>
        new(PhysicalAxis.Vertical, toSeq(container.AsEnumerable()).Choose(block => Laid(block, culture))) { Role = Role.Stack, Width = Sizing.Grow.Unit };

    private Option<Node> Laid(Block block, CultureInfo culture) =>
        block switch {
            HeadingBlock heading => new Node.Text(Inlines(heading.Inline)) { Role = heading.Level > 1 ? Role.Headline : Role.Title, Accessible = new Accessible { Role = AccessRole.Heading } },
            ParagraphBlock paragraph => new Node.Text(Inlines(paragraph.Inline)) { Width = Sizing.Grow.Unit },
            CodeBlock code => Coded(code),
            HtmlBlock html => new Node.Text([html.Lines.ToString()]) { Role = Role.Code, Width = Sizing.Grow.Unit },
            ThematicBreakBlock => new Node.Box { Role = Role.Divider, Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.Separator } },
            LeafBlock leaf => new Node.Text(Inlines(leaf.Inline)) { Width = Sizing.Grow.Unit },
            QuoteBlock quote => new Node.Flex(PhysicalAxis.Horizontal, [new Node.Box { Role = Role.Divider, Height = Sizing.Grow.Unit }, Stacked(quote, culture)]) { Role = Role.Stack, Width = Sizing.Grow.Unit },
            ListBlock list => Listed(list, culture),
            Table table => Tabled(table, culture),
            ContainerBlock container => Stacked(container, culture),
            _ => Option<Node>.None,
        };

    private Node.Flex Coded(CodeBlock code) {
        Option<string> language = Optional(code as FencedCodeBlock).Bind(static fenced => Optional(fenced.Info)).Filter(static info => info.Length > 0);
        Seq<Span> spans = toSeq(code.Lines.Lines.Take(code.Lines.Count)).Map((line, index) => (index > 0 ? Seq<Span>("\n") : []) + Tokens(language, line.Slice.ToString())).Bind(static line => line);
        return new Node.Flex(PhysicalAxis.Vertical, [new Node.Text(spans) { Role = Role.Code }]) { Role = Role.Section, Width = Sizing.Grow.Unit };
    }

    private Node.Flex Listed(ListBlock list, CultureInfo culture) {
        int first = Optional(list.OrderedStart).Bind(parseInt).IfNone(1);
        return new Node.Flex(PhysicalAxis.Vertical, toSeq(list.OfType<ListItemBlock>()).Map((item, index) => (Node)new Node.Flex(PhysicalAxis.Horizontal, [
            new Node.Text([list.IsOrdered ? string.Create(culture, $"{first + index}{list.OrderedDelimiter}") : Bullet]),
            Stacked(item, culture),
        ]) { Role = Role.Stack, Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.ListItem } })) {
            Role = Role.Stack,
            Width = Sizing.Grow.Unit,
            Accessible = new Accessible { Role = AccessRole.List },
        };
    }

    private Node.Grid Tabled(Table table, CultureInfo culture) {
        Seq<TableColumnDefinition> columns = toSeq(table.ColumnDefinitions);
        return new Node.Grid(new Tracks(columns.Map(static _ => (Sizing)Sizing.Fit.Content)), toSeq(table.OfType<TableRow>()).Map(row => toSeq(row.OfType<TableCell>()).Map(cell => (Node)(Stacked(cell, culture) with {
            Role = row.IsHeader ? Role.Header : Role.Cell,
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
            CodeInline code => Seq(style with { Text = code.Content, Role = Role.Code }),
            EmphasisInline emphasis => Inlined(emphasis, style with { Role = emphasis.DelimiterCount > 1 ? Role.Strong : Role.Emphasis }),
            LinkInline link => Inlined(link, Linked(style, link.Url)),
            AutolinkInline automatic => Seq(Linked(style with { Text = automatic.Url }, automatic.Url)),
            LineBreakInline line => Seq(style with { Text = line.IsHard ? "\n" : " " }),
            HtmlEntityInline entity => Seq(style with { Text = entity.Transcoded.ToString() }),
            HtmlInline html => Seq(style with { Text = html.Tag }),
            ContainerInline nested => Inlined(nested, style),
            _ => Seq<Span>(),
        });

    private Span Linked(Span style, string? url) =>
        Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri? uri) ? style with { Role = Role.Link, Part = new Part.Press(_ => Followed(uri)) } : style;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Sources {
    internal static Key Line(int index) => new Key.Ordinal((uint)index);

    internal static (int Start, int End) Clamped(System.Range range, int length) => (int.Clamp(range.Start.GetOffset(length), 0, length), int.Clamp(range.End.GetOffset(length), 0, length));

    internal static Seq<int> Spanned((int Start, int End) lines) => toSeq(Range(lines.Start, int.Max(0, lines.End - lines.Start)));

    internal static Node Spacer(Role role) => new Node.Box { Role = role, Opacity = Fraction.MinValue };

    internal static Option<string> Joined(Seq<string> parts) => parts.IsEmpty ? None : Some(string.Join('\n', parts));

    internal static Seq<Span> Marked(Option<Search> find, string text, CultureInfo culture) => find.Map(query => query.Marked(text, culture)).IfNone([text]);

    internal static HashSet<Condition> Signal(EventLevel level) =>
        level switch {
            EventLevel.Critical or EventLevel.Error => [Condition.Faulted],
            EventLevel.Warning => [Condition.Warning],
            _ => [],
        };

    internal static Lift Copied(Func<Key, Option<string>> text) => new(keys => IO.pure<Transfer>(new Transfer.Text(string.Join('\n', keys.Choose(text)))));

    internal static Node Listed(Part.Choice choice, Func<Key, Node> row, bool wrap, Role role, HashSet<Key> realized) =>
        wrap
            ? choice.Drawn(row, static rows => new Node.Flex(PhysicalAxis.Vertical, rows) { Width = Sizing.Grow.Unit })
            : choice.Drawn(row, PhysicalAxis.Vertical, new Tracks([Sizing.Grow.Unit]), realized) with { Rows = role };

    internal static Seq<Span> Overlaid(Seq<Span> tokens, Seq<(int Start, int End, Decoration Mark)> marks) =>
        tokens.Fold((At: 0, Spans: Seq<Span>()), (held, token) => (held.At + token.Text.Length, held.Spans + toSeq(
            toSeq(marks.Bind(static mark => Seq(mark.Start, mark.End)).Filter(cut => cut > held.At && cut < held.At + token.Text.Length).Add(held.At).Add(held.At + token.Text.Length).Distinct().Order())
                .AsIterable().Window(2, static cut => (From: cut[0], To: cut[1])))
            .Map(cut => Piece(token, held.At, cut, marks)))).Spans;

    private static Span Piece(Span token, int at, (int From, int To) cut, Seq<(int Start, int End, Decoration Mark)> marks) {
        Seq<Decoration> over = marks.Filter(mark => mark.Start <= cut.From && mark.End >= cut.To).Map(static mark => mark.Mark);
        return token with {
            Text = token.Text[(cut.From - at)..(cut.To - at)],
            Role = over.Choose(static mark => mark.Role).Last | token.Role,
            Conditions = over.Fold(token.Conditions, static (held, mark) => held.Union(mark.Conditions)),
        };
    }
}
