using System.Diagnostics.Tracing;
using System.Globalization;
using NodaTime.Text;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SuperLinq;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Row<TItem>(TItem Item, Key Key, int Depth, Option<bool> Open) where TItem : notnull {
    public Seq<View> Lead(Option<IO<Unit>> toggle) => LanguageExt.Seq.repeat(Disclosure.Chevron(None, None), Depth).Add(Disclosure.Chevron(Open, toggle));

    public static Seq<Row<TItem>> Flatten(Seq<TItem> roots, Func<TItem, Seq<TItem>> children, Func<TItem, Key> key, Func<TItem, bool> open, Func<TItem, bool> keep) {
        return roots.Filter(Holds).Bind(root => toSeq(SuperEnumerable.TraverseDepthFirst(Laid(root, 0), node => node.Opened
                ? node.Members.Map(item => Laid(item, node.Depth + 1))
                : [])))
            .Map(node => new Row<TItem>(node.Item, key(node.Item), node.Depth, node.Members.IsEmpty ? None : Some(node.Opened)));

        bool Holds(TItem item) => SuperEnumerable.TraverseDepthFirst(item, node => children(node)).Any(keep);

        (TItem Item, int Depth, Seq<TItem> Members, bool Opened) Laid(TItem item, int depth) => (item, depth, children(item).Filter(Holds), open(item));
    }

    public static View Drawn(Seq<Row<TItem>> rows, Func<Seq<Key>, Part.Choice> choice, Func<Row<TItem>, View> drawn, HashSet<Key> realized, bool wraps = false) {
        HashMap<Key, Row<TItem>> keyed = toHashMap(rows.Map(static row => (row.Key, row)));
        Part.Choice chosen = choice(rows.Map(static row => row.Key));
        View Face(Key key) => keyed[key] switch { var row => drawn(row) with { Sticks = row.Open.Map(static _ => PhysicalAxis.Vertical) } };
        return wraps
            ? chosen.Drawn(Face, static members => new View.Flex(PhysicalAxis.Vertical, members) { Width = Sizing.Grow.Unit })
            : chosen.Drawn(Face, PhysicalAxis.Vertical, new Tracks([Sizing.Grow.Unit]), realized) switch {
                var lazy => rows.Head.Bind(row => Face(row.Key).Role).Match(Some: role => lazy with { Rows = role }, None: () => lazy),
            };
    }
}

public sealed record Search(Seq<string> Terms) {
    private const CompareOptions Folded = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    public static Search Everything { get; } = new("");

    public Search(string pattern) : this(toSeq(pattern.Split('*', StringSplitOptions.RemoveEmptyEntries))) { }

    internal static Search Of(Option<TextField> field) => field.Map(static held => held.Search).IfNone(Everything);

    public bool Matches(string text, CultureInfo culture) => Hits(text, culture).IsSome;

    public Seq<Span> Marked(string text, CultureInfo culture) =>
        Hits(text, culture).Match(
            Some: hits => hits.Fold((At: 0, Spans: Seq<Span>()), (held, hit) => (hit.End.Value, held.Spans + Plain(text[held.At..hit.Start.Value]).Add(new Span(text[hit]) { Role = Roles.Mark })))
                switch { var held => held.Spans + Plain(text[held.At..]), },
            None: () => [text]);

    internal Option<Seq<System.Range>> Hits(string text, CultureInfo culture) =>
        Terms.FoldM(Seq<System.Range>(), (hits, segment) => {
            int at = hits.Last.Map(static hit => hit.End.Value).IfNone(0);
            int index = culture.CompareInfo.IndexOf(text.AsSpan(at), segment.AsSpan(), Folded, out int length);
            return index < 0 ? Option<Seq<System.Range>>.None : Some(hits.Add(new System.Range(at + index, at + index + length)));
        }).As();

    private static Seq<Span> Plain(string text) => text.Length > 0 ? [text] : [];
}

public sealed record TextField(string Text, Func<Edit<string>, IO<Unit>> Edited) {
    private static readonly Key Entry = new Key.Named("entry");

    public Option<string> Placeholder { get; init; }
    public Option<Key> Glyph { get; init; }
    public bool Clears { get; init; }
    public Option<int> MaxLines { get; init; } = Some(1);

    public Search Search => new(Text);

    public View Drawn =>
        new View.Flex(PhysicalAxis.Horizontal,
            Glyph.Map(static role => (View)new View.Box { Role = role, Align = UnitPoint.Center }).ToSeq()
            + Seq<View>(new View.Text([Text]) {
                Key = Entry,
                MaxLines = MaxLines,
                Placeholder = Placeholder,
                Width = Sizing.Grow.Unit,
                Edited = Some(Edited),
                Accessible = new Accessible { Role = AccessRole.TextBox, Label = Placeholder },
            })
            + (Clears && Text.Length > 0 ? Seq(Part.Press.Dismiss(Edited(Edit.Of(EditPhase.Commit, "")))) : [])) {
            Role = Roles.Field,
            Width = Sizing.Grow.Unit,
        };
}

public sealed record ValueTree<TItem>(Seq<TItem> Roots, Func<TItem, Key> Keyed, Func<TItem, Seq<TItem>> Children, Func<TItem, string> Name, Func<TItem, string> Value, Func<Seq<Key>, Part.Choice> Choice)
    where TItem : notnull {
    public Func<TItem, Option<string>> Kind { get; init; } = static _ => None;
    public HashSet<Key> Open { get; init; }
    public Func<Edit<HashSet<Key>>, IO<Unit>> Toggled { get; init; } = static _ => IO.pure(unit);
    public Search Find { get; init; } = Search.Everything;
    public Option<Func<Seq<TItem>, string>> Path { get; init; }
    public HashSet<Key> Realized { get; init; }

    public HashSet<Key> Opened(int depth) =>
        toHashSet(Row<TItem>.Flatten(Roots, Children, Keyed, static _ => true, static _ => true).Filter(row => row.Depth < depth && row.Open.IsSome).Map(static row => row.Key));

    public View Drawn(CultureInfo culture) {
        Seq<Row<TItem>> rows = Row<TItem>.Flatten(Roots, Children, Keyed, item => !Find.Terms.IsEmpty || Open.Contains(Keyed(item)), item => Find.Terms.IsEmpty || Find.Matches(Name(item), culture) || Find.Matches(Value(item), culture));
        HashMap<Key, Seq<TItem>> chains = toHashMap(rows.Zip(rows.Scan(Seq<TItem>(), static (chain, row) => chain.Take(row.Depth).Add(row.Item)).Tail).Map(static held => (held.First.Key, held.Second)));
        return new View.Scroll(PhysicalAxis.Vertical, Row<TItem>.Drawn(rows, Choice, row => Face(row, culture), Realized) with {
            Lift = Lift.Text(key => chains.Find(key).Bind(chain => Path.Map(path => path(chain)) || chain.Last.Map(Value))),
            Accessible = new Accessible { Role = AccessRole.Tree },
        }).Grown;
    }

    private View.Flex Face(Row<TItem> row, CultureInfo culture) {
        Seq<TItem> below = Children(row.Item);
        string name = Name(row.Item);
        return new View.Flex(PhysicalAxis.Horizontal,
            row.Lead(Some(Toggled(Edit.Of(EditPhase.Commit, Open.SymmetricExcept([row.Key])))))
            + Seq<View>(
                new View.Text(Find.Marked(name, culture)) { MaxLines = 1 },
                new View.Text(Find.Marked(Value(row.Item), culture)) { Role = Roles.Code, MaxLines = 1, Width = Sizing.Grow.Unit })
            + Kind(row.Item).Map(static kind => (View)new View.Text([kind]) { Role = Roles.Caption }).ToSeq()
            + (below.IsEmpty ? [] : Seq<View>(new View.Text([below.Count.ToString(culture)]) { Role = Roles.Badge }))) {
            Accessible = new Accessible { Role = AccessRole.TreeItem, Label = name, Expanded = row.Open },
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

    public View Drawn(CultureInfo culture) {
        Search query = Search.Of(Filter);
        Seq<TEntry> kept = Entries.Filter(entry => Levels.ForAll(levels => levels.Shown.Contains(Level(entry)))
            && (query.Matches(Message(entry), culture) || Source(entry).Exists(source => query.Matches(source, culture))));
        Seq<(TEntry Entry, int Count)> runs = Grouped
            ? toSeq(kept.AsIterable().GroupAdjacent(entry => (Level(entry), Source(entry), Message(entry))).Select(static run => (Entry: run.First(), Count: run.Count())))
            : kept.Map(static entry => (Entry: entry, Count: 1));
        Seq<Row<(TEntry Entry, int Count)>> rows = runs.Map(run => new Row<(TEntry Entry, int Count)>(run, Keyed(run.Entry), 0, None));
        HashMap<Key, TEntry> keyed = toHashMap(rows.Map(static row => (row.Key, row.Item.Entry)));
        View listed = Row<(TEntry Entry, int Count)>.Drawn(rows, Choice, row => Line(row.Item.Entry, row.Item.Count, query, culture), Realized, Wrap) with {
            Lift = Lift.Text(key => keyed.Find(key).Map(entry => string.Join(' ', Stamp.Map(pattern => Stamped(pattern, entry, culture)).ToSeq() + Source(entry).ToSeq() + Seq(Message(entry))))),
        };
        Seq<View> bar = Levels.Map(levels => Leveled(levels, culture)).IfNone([]) + Filter.Map(static field => field.Drawn).ToSeq();
        return new View.Flex(PhysicalAxis.Vertical,
            (bar.IsEmpty ? [] : Seq<View>(new View.Flex(PhysicalAxis.Horizontal, bar) { Role = Roles.Toolbar, Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.Toolbar } }))
            .Add(new View.Scroll(PhysicalAxis.Vertical, listed) { KeepsEnd = Follows }.Grown)).Grown;
    }

    private Seq<View> Leveled(Levels levels, CultureInfo culture) =>
        toSeq(Entries.GroupBy(Level).OrderBy(static group => group.Key)).Map(group => new Toggle(levels.Shown.Contains(group.Key),
            edit => levels.Edited(edit.Map(shown => shown ? levels.Shown.TryAdd(group.Key) : levels.Shown.Remove(group.Key)).As())) {
            Kind = ToggleKind.Button,
            Content = [
                new View.Box { Role = Roles.Status, Conditions = Signal(group.Key) },
                new View.Text([levels.Named(group.Key)]),
                new View.Text([group.Count().ToString(culture)]) { Role = Roles.Caption },
            ],
        }.Drawn);

    private View.Flex Line(TEntry entry, int count, Search query, CultureInfo culture) {
        HashSet<Condition> signal = Signal(Level(entry));
        return new View.Flex(PhysicalAxis.Horizontal,
            Stamp.Map(pattern => (View)new View.Text([Stamped(pattern, entry, culture)]) { Role = Roles.Code }).ToSeq()
            + Seq<View>(new View.Box { Role = Roles.Status, Conditions = signal, Opacity = signal.IsEmpty ? Fraction.MinValue : Fraction.MaxValue })
            + Source(entry).Map(source => (View)new View.Text(query.Marked(source, culture)) { Role = Roles.Caption, MaxLines = 1 }).ToSeq()
            + Seq<View>(new View.Text(query.Marked(Message(entry), culture)) { MaxLines = Wrap ? None : Some(1), Width = Sizing.Grow.Unit })
            + Fields(entry).ToSeq().Map(static field => (View)new View.Flex(PhysicalAxis.Horizontal, [new View.Text([field.Key]) { Role = Roles.Caption }, new View.Text([field.Value]) { Role = Roles.Code }]) { Role = Roles.Chip })
            + (count > 1 ? Seq<View>(new View.Text([count.ToString(culture)]) { Role = Roles.Badge }) : [])) {
            Role = Roles.ListRow,
            Width = Sizing.Grow.Unit,
            Conditions = signal,
        };
    }

    private string Stamped(LocalTimePattern pattern, TEntry entry, CultureInfo culture) => pattern.WithCulture(culture).Format(Time(entry).InZone(Zone).TimeOfDay);

    private static HashSet<Condition> Signal(EventLevel level) =>
        level switch {
            EventLevel.Critical or EventLevel.Error => [Condition.Faulted],
            EventLevel.Warning => [Condition.Warning],
            _ => [],
        };
}
