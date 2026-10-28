using System.Globalization;
using System.Numerics;
using Rasm.UI.Components;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;

namespace Rasm.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ColumnSort(Key Column, bool Descending);

public sealed record Column<TRow>(Key Key, Node Header) where TRow : notnull {
    public Option<Func<TRow, string>> Text { get; init; }
    public Option<Func<TRow, Node>> Cell { get; init; }
    public Option<Comparison<TRow>> Order { get; init; }
    public Option<Func<TRow, Edit<string>, IO<Unit>>> Renamed { get; init; }
    public Option<Func<Seq<TRow>, Node>> Footer { get; init; }
    public Sizing Width { get; init; } = Sizing.Grow.Unit;
    public Fraction Align { get; init; } = Fraction.MinValue;
    public bool Hidden { get; init; }
    public bool Pinned { get; init; }

    public static Column<TRow> Toggled(Key key, string label, Role glyph, Func<TRow, Check> value, Func<TRow, Edit<Check>, IO<Unit>> edited) =>
        new(key, new Node.Flex(PhysicalAxis.Horizontal, [Glyphed(glyph, Check.On) with { Accessible = new Accessible { Role = AccessRole.Image, Label = label } }]) { Role = Role.Button }) {
            Cell = Some<Func<TRow, Node>>(row => new Toggle(value(row), edit => edited(row, edit)) { Role = Role.Button, Content = [Glyphed(glyph, value(row))] }.Drawn switch {
                var drawn => drawn with { Conditions = drawn.Conditions.TryAdd(Condition.Borderless), Accessible = drawn.Accessible.Combine(new Accessible { Label = label }) },
            }),
            Order = Some<Comparison<TRow>>((left, right) => Rank(value(left)).CompareTo(Rank(value(right)))),
            Width = Sizing.Fit.Content,
        };

    private static Node.Box Glyphed(Role glyph, Check check) => new() { Role = glyph, Conditions = check == Check.Off ? [Condition.Disabled] : [] };

    private static int Rank(Check check) => check.Map(off: 0, on: 2, mixed: 1);
}

public sealed record Table<TRow>(Content<Seq<TRow>> Rows, Func<TRow, Key> Keyed, Seq<Column<TRow>> Columns, Func<Seq<Key>, Part.Choice> Choice) where TRow : notnull {
    // --- [STATE]
    public HashSet<Key> Realized { get; init; }
    public Seq<ColumnSort> Sort { get; init; }
    public Option<Func<Edit<Seq<ColumnSort>>, IO<Unit>>> Sorted { get; init; }
    public Option<Func<Edit<Seq<Column<TRow>>>, IO<Unit>>> Arranged { get; init; }
    public Option<Search> Filter { get; init; }
    public Option<Func<TRow, Seq<TRow>>> Children { get; init; }
    public Option<Func<TRow, Key>> Group { get; init; }
    public Option<Func<Key, Seq<TRow>, Node>> GroupFace { get; init; }
    public HashSet<Key> Open { get; init; }
    public Option<Func<Edit<HashSet<Key>>, IO<Unit>>> Expanded { get; init; }
    public Option<Func<Seq<Key>, Key, Placement, Option<IO<Unit>>>> Moved { get; init; }
    public bool Numbered { get; init; }
    public bool KeepsEnd { get; init; }
    public string Unexpected { get; init; } = "";

    private Option<Search> Query => Filter.Filter(static search => search.Pattern.Length > 0);

    private Option<AccessRole> CellRole => Children.IsSome ? None : Some(AccessRole.GridCell);

    // --- [DRAW]
    public Node Drawn(CultureInfo culture) {
        Seq<Column<TRow>> shown = toSeq(Columns.Filter(static column => !column.Hidden).OrderByDescending(static column => column.Pinned));
        Func<TRow, bool> kept = Query.Match(
            Some: query => (Func<TRow, bool>)(row => shown.Exists(column => column.Text.Exists(text => query.Matches(text(row), culture)))),
            None: static () => static _ => true);
        Seq<Row<Either<IGrouping<Key, TRow>, TRow>>> rows = Flattened(Rows.Current.IfNone([]), shown, kept, culture);
        string strut = rows.Choose(static row => row.Item.ToOption()).Count.ToString(culture);
        Node divider = new Node.Box { Role = Role.Divider, Width = Sizing.Grow.Unit };
        Seq<TRow> totaled = Rows.Current.IfNone([]).Filter(kept);
        Seq<Node> footer = shown.Exists(static column => column.Footer.IsSome)
            ? [divider, new Node.Flex(PhysicalAxis.Horizontal, Gutter(None, strut, culture) + shown.Map(column => Framed(column, column.Footer.Map(total => total(totaled)).IfNone(static () => new Node.Box()), [], CellRole))) {
                Role = Role.ListRow,
                Width = Sizing.Grow.Unit,
            }]
            : [];
        return new Node.Scroll(PhysicalAxis.Horizontal, new Node.Flex(PhysicalAxis.Vertical,
            Seq(Header(shown, strut, culture), divider, Rows.Drawn(_ => Body(rows, shown, strut, culture), Unexpected)) + footer) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
        }) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            Conditions = [Condition.Borderless],
        };
    }

    // --- [ROWS]
    private Seq<Row<Either<IGrouping<Key, TRow>, TRow>>> Flattened(Seq<TRow> current, Seq<Column<TRow>> shown, Func<TRow, bool> kept, CultureInfo culture) {
        Comparison<TRow> order = Sort
            .Choose(entry => shown.Find(column => column.Key == entry.Column).Bind(column => column.Order | column.Text.Map(text => (Comparison<TRow>)((left, right) => culture.CompareInfo.Compare(text(left), text(right), CompareOptions.NumericOrdering))))
                .Map(compare => entry.Descending ? (left, right) => compare(right, left) : compare))
            .Fold((Comparison<TRow>)(static (_, _) => 0), static (prior, next) => (left, right) => prior(left, right) switch { 0 => next(left, right), var held => held });
        Seq<TRow> Sorted(Seq<TRow> rows) => toSeq(rows.Order(System.Collections.Generic.Comparer<TRow>.Create(order)));
        Seq<Either<IGrouping<Key, TRow>, TRow>> roots = Group.Match(
            Some: of => toSeq(Sorted(current).GroupBy(of)).Map(static group => Either.Left<IGrouping<Key, TRow>, TRow>(group)),
            None: () => Sorted(current).Map(static row => Either.Right<IGrouping<Key, TRow>, TRow>(row)));
        Seq<Either<IGrouping<Key, TRow>, TRow>> Below(Either<IGrouping<Key, TRow>, TRow> item) =>
            item.Match(Left: static group => toSeq(group), Right: row => Children.Map(children => Sorted(children(row))).IfNone([])).Map(static row => Either.Right<IGrouping<Key, TRow>, TRow>(row));
        Seq<Key> Every(Seq<Either<IGrouping<Key, TRow>, TRow>> items) => items.Bind(item => Itemized(item).Cons(Every(Below(item))));
        return Row<Either<IGrouping<Key, TRow>, TRow>>.Flatten(roots, Below, Itemized, Query.IsSome ? toHashSet(Every(roots)) : Open, item => item.ToOption().Exists(kept));
    }

    private Key Itemized(Either<IGrouping<Key, TRow>, TRow> item) => item.Match(Left: static group => group.Key, Right: Keyed);

    private Node.Scroll Body(Seq<Row<Either<IGrouping<Key, TRow>, TRow>>> rows, Seq<Column<TRow>> shown, string strut, CultureInfo culture) {
        HashMap<Key, int> numbers = toHashMap(rows.Filter(static row => row.Item.IsRight).Map(static (row, index) => (row.Key, index + 1)));
        Node Lined(Row<Either<IGrouping<Key, TRow>, TRow>> row) {
            Node line = new Node.Flex(PhysicalAxis.Horizontal, row.Item.Match(
                Left: group => Lead(row).Add(GroupFace.Match(Some: face => face(group.Key, toSeq(group)), None: () => Grouped(group, culture))),
                Right: datum => Gutter(numbers.Find(row.Key), strut, culture) + shown.Map((column, index) => Cell(column, datum, index is 0 ? Lead(row) : [], culture)))) {
                Key = row.Key,
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            };
            Accessible accessible = new() { Role = Children.IsSome ? AccessRole.TreeItem : AccessRole.Row, Expanded = row.Open };
            return Moved.Match(
                Some: moved => new Node.Flex(PhysicalAxis.Vertical, [line]) {
                    Width = Sizing.Grow.Unit,
                    Height = Sizing.Grow.Unit,
                    Accessible = accessible,
                    Drop = new Drop((transfer, target, placement) =>
                        from keys in Optional(transfer as Transfer.Keys)
                        from over in target
                        from effect in moved(keys.Paths.Choose(static path => path.Last), over, placement)
                        select effect) { Nests = Children.IsSome },
                },
                None: () => line with { Accessible = accessible });
        }
        return new Node.Scroll(PhysicalAxis.Vertical, Row<Either<IGrouping<Key, TRow>, TRow>>.Drawn(rows, Choice, Lined, Realized) with {
            Width = Sizing.Grow.Unit,
            Lift = Moved.Map(static _ => new Lift(static keys => IO.pure<Transfer>(new Transfer.Keys(keys.Map(static key => Seq(key)))))),
            Accessible = new Accessible { Role = Children.IsSome ? AccessRole.Tree : AccessRole.Grid },
        }) {
            Width = Sizing.Grow.Unit,
            Height = Sizing.Grow.Unit,
            KeepsEnd = KeepsEnd,
        };
    }

    private static Node.Flex Grouped(IGrouping<Key, TRow> group, CultureInfo culture) =>
        new(PhysicalAxis.Horizontal, [
            new Node.Text([group.Key.Identifier]) { MaxLines = 1, Align = UnitPoint.Center },
            new Node.Text([group.Count().ToString(culture)]) { Role = Role.Badge, Align = UnitPoint.Center },
        ]) {
            Width = Sizing.Grow.Unit,
        };

    private Seq<Node> Lead(Row<Either<IGrouping<Key, TRow>, TRow>> row) {
        Node spacer = new Node.Box { Role = Role.Chevron, Opacity = Fraction.MinValue };
        return Children.IsSome || Group.IsSome
            ? toSeq(Range(0, row.Depth)).Map(_ => spacer) + Seq(row.Open.Match(Some: open => Disclosed(row.Key, open), None: () => spacer))
            : [];
    }

    private Node Disclosed(Key key, bool open) {
        Node chevron = new Node.Box { Role = Role.Chevron, Conditions = open ? [Condition.Selected] : [], Accessible = new Accessible { Role = AccessRole.Disclosure, Expanded = open } };
        return Expanded.Map(expanded => new Part.Press(_ => expanded(Edit.Of(EditPhase.Commit, open ? Open.Remove(key) : Open.TryAdd(key)))).Drawn(chevron)).IfNone(chevron);
    }

    private Seq<Node> Gutter(Option<int> number, string strut, CultureInfo culture) =>
        Numbered
            ? [new Node.Overlay(new Node.Text([strut]) { Opacity = Fraction.MinValue }, number.Map(at => (Node)new Node.Text([at.ToString(culture)]) { Align = new UnitPoint(Fraction.MaxValue, Fraction.Half) }).ToSeq()) {
                Role = Role.Gutter,
                Height = Sizing.Grow.Unit,
                Sticks = PhysicalAxis.Horizontal,
            }]
            : [];

    // --- [CELLS]
    private Node Cell(Column<TRow> column, TRow row, Seq<Node> lead, CultureInfo culture) {
        Node content = column.Cell.Map(cell => cell(row)).IfNone(() => column.Text.Map(text => (Node)new Node.Text(Query.Map(query => query.Marked(text(row), culture)).IfNone(() => [text(row)])) {
            MaxLines = 1,
            Edited = column.Renamed.Map(renamed => (Func<Edit<string>, IO<Unit>>)(edit => renamed(row, edit))),
        }).IfNone(static () => new Node.Box()));
        return Framed(column, lead.IsEmpty ? content : new Node.Flex(PhysicalAxis.Horizontal, lead.Add(content)), [], CellRole);
    }

    private static Node Framed(Column<TRow> column, Node content, Seq<Node> above, Option<AccessRole> role) {
        Node aligned = content with { Align = new UnitPoint(column.Align, Fraction.Half) };
        return (column.Width is Sizing.Fit ? new Node.Flex(PhysicalAxis.Horizontal, [aligned]) : (Node)new Node.Overlay(new Node.Box(), aligned.Cons(above))) with {
            Key = column.Key,
            Role = Role.Cell,
            Width = column.Width,
            Height = Sizing.Grow.Unit,
            Sticks = column.Pinned ? Some(PhysicalAxis.Horizontal) : None,
            Accessible = new Accessible { Role = role },
        };
    }

    // --- [HEADER]
    private Node Header(Seq<Column<TRow>> shown, string strut, CultureInfo culture) {
        Seq<Key> keys = shown.Map(static column => column.Key);
        HashMap<Key, Column<TRow>> keyed = toHashMap(shown.Map(static column => (column.Key, column)));
        Node Lined(Seq<Node> cells) =>
            new Node.Flex(PhysicalAxis.Horizontal, Gutter(None, strut, culture) + cells) { Role = Role.ListRow, Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.Row } };
        Option<Key> lead = Sort.Head.Map(static entry => entry.Column);
        Node heads = Sorted.IsSome || Arranged.IsSome
            ? new Part.Choice(keys, new Selection.Many(toHashSet(Sort.Map(static entry => entry.Column))) { Anchor = lead, Focus = lead }, edit => Sorted.Filter(_ => edit.Phase != EditPhase.Step).Match(
                Some: sorted => sorted(new Edit<Seq<ColumnSort>>(edit.Phase, edit.Value.Map(next => Resorted(next, keys)))),
                None: static () => IO.pure(unit))) {
                Reorder = Arranged.Map(arranged => Arranging<Seq<Key>>(arranged, Reordered)),
            }.Drawn(key => Head(keyed[key], culture), Lined)
            : Lined(shown.Map<Node>(column => Head(column, culture)));
        return Sorted.IsSome || Arranged.IsSome
            ? new Node.Overlay(heads, [new Node.Layer(Some(Seq<Key>()), Menu) { Trigger = new Trigger.Secondary() }]) { Width = Sizing.Grow.Unit }
            : heads;
    }

    private Node.Flex Head(Column<TRow> column, CultureInfo culture) {
        Seq<Node> marks = column.Width is Sizing.Fit
            ? []
            : Sort.Map(static (entry, rank) => (Entry: entry, Rank: rank)).Find(held => held.Entry.Column == column.Key).Map(held =>
                Seq<Node>(new Node.Box { Role = Role.Sorter, Conditions = Direction(column.Key) })
                + (Sort.Count > 1 ? Seq<Node>(new Node.Text([(held.Rank + 1).ToString(culture)]) { Role = Role.Badge }) : [])).IfNone([]);
        Node cell = Framed(column, marks.IsEmpty ? column.Header : new Node.Flex(PhysicalAxis.Horizontal, column.Header.Cons(marks)), Arranged.Bind(arranged => Grip(column, arranged)).ToSeq(), None);
        return new Node.Flex(PhysicalAxis.Horizontal, [cell with { Key = None, Width = Sizing.Grow.Unit, Sticks = None }]) {
            Key = column.Key,
            Role = Role.Stack,
            Width = column.Width,
            Sticks = cell.Sticks,
            Accessible = new Accessible { Role = AccessRole.GridCell },
        };
    }

    private Option<Node> Grip(Column<TRow> column, Func<Edit<Seq<Column<TRow>>>, IO<Unit>> arranged) =>
        from width in Optional(column.Width as Sizing.Fixed)
        from domain in AxisRange.Validated(width.Limits.Min, width.Limits.Max).ToOption()
        let plane = new PlotPlane.Track(PhysicalAxis.Horizontal, new Axis.Linear(domain))
        select new Handles<float>(width.Extent, Points<float>.Scalar(plane, Lens.identity<float>()), Arranging<float>(arranged, extent => Changed(column.Key, held => held with { Width = new Sizing.Fixed(extent) { Limits = width.Limits } }))) {
            Gearing = Some(1f),
            Handle = (_, _) => new Node.Pin(new Vector2(domain.Low, 0f), new Node.Box { Width = Sizing.Grow.Unit, Height = Sizing.Grow.Unit }) { Span = new Vector2(domain.Span, 1f) },
        }.Drawn(plane, [new Node.Box { Role = Role.Grip }]) with {
            Width = Sizing.Fit.Content,
            Height = Sizing.Grow.Unit,
            Align = new UnitPoint(Fraction.MaxValue, Fraction.Half),
        };

    private Node.Flex Menu =>
        new(PhysicalAxis.Vertical, Columns.Map(column => (Node)new Node.Flex(PhysicalAxis.Horizontal,
            Seq(Arranged.Match(
                Some: arranged => new Toggle(column.Hidden ? Check.Off : Check.On, Arranging<Check>(arranged, check => Changed(column.Key, held => held with { Hidden = check == Check.Off }))) { Content = [column.Header] }.Drawn,
                None: () => column.Header) with {
                Width = Sizing.Grow.Unit,
            })
            + Sorted.Filter(_ => column.Order.IsSome || column.Text.IsSome).Map(sorted =>
                new Part.Press(_ => sorted(Edit.Of(EditPhase.Commit, Cycled(column.Key)))).Drawn(new Node.Box { Role = Role.Sorter, Conditions = Direction(column.Key) })).ToSeq()
            + Arranged.Map(arranged => new Toggle(column.Pinned ? Check.On : Check.Off, Arranging<Check>(arranged, check => Changed(column.Key, held => held with { Pinned = check == Check.On }))) {
                Role = Role.Button,
                Content = [new Node.Box { Role = Role.Pin }],
            }.Drawn).ToSeq()) {
            Key = column.Key,
            Width = Sizing.Grow.Unit,
        })) {
            Role = Role.Menu,
            Accessible = new Accessible { Role = AccessRole.List },
        };

    // --- [ARRANGE]
    private HashSet<Condition> Direction(Key key) => toHashSet(Sort.Find(entry => entry.Column == key).Map(static entry => entry.Descending ? Condition.Descending : Condition.Selected).ToSeq());

    private Seq<ColumnSort> Resorted(Selection next, Seq<Key> keys) =>
        keys.Filter(next.Holds) switch {
            var held => next.Lead.Filter(lead => held == Seq(lead)).Match(
                Some: only => Sort.ForAll(entry => entry.Column == only) ? Cycled(only) : [new ColumnSort(only, Descending: false)],
                None: () => Sort.Filter(entry => next.Holds(entry.Column)) + held.Filter(key => !Sort.Exists(entry => entry.Column == key)).Map(static key => new ColumnSort(key, Descending: false))),
        };

    private Seq<ColumnSort> Cycled(Key key) =>
        Sort.Find(entry => entry.Column == key).Match(
            Some: entry => entry.Descending ? Sort.Filter(held => held.Column != key) : Sort.Map(held => held.Column == key ? held with { Descending = true } : held),
            None: () => Sort.Add(new ColumnSort(key, Descending: false)));

    private Seq<Column<TRow>> Reordered(Seq<Key> order) => order.Choose(toHashMap(Columns.Map(static column => (column.Key, column))).Find) + Columns.Filter(static column => column.Hidden);

    private Seq<Column<TRow>> Changed(Key key, Func<Column<TRow>, Column<TRow>> change) => Columns.Map(column => column.Key == key ? change(column) : column);

    private static Func<Edit<T>, IO<Unit>> Arranging<T>(Func<Edit<Seq<Column<TRow>>>, IO<Unit>> arranged, Func<T, Seq<Column<TRow>>> columns) =>
        edit => arranged(new Edit<Seq<Column<TRow>>>(edit.Phase, edit.Value.Map(columns)));
}
