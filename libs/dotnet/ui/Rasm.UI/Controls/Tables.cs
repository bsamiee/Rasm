using System.Globalization;
using System.Numerics;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ColumnSort(Key Column, bool Descending);

public sealed record Column<TRow>(Key Key, View Header) where TRow : notnull {
    public Option<Func<TRow, string>> Text { get; init; }
    public Option<Func<TRow, View>> Cell { get; init; }
    public Option<Comparison<TRow>> Order { get; init; }
    public Option<Func<TRow, Edit<string>, IO<Unit>>> Renamed { get; init; }
    public Option<Func<Seq<TRow>, View>> Footer { get; init; }
    public Sizing Width { get; init; } = Sizing.Grow.Unit;
    public Fraction Align { get; init; } = Fraction.MinValue;
    public bool Hidden { get; init; }
    public bool Pinned { get; init; }

    public static Column<TRow> Toggled(Key key, string label, Key glyph, Func<TRow, Check> value, Func<TRow, Edit<Check>, IO<Unit>> edited) =>
        new(key, new View.Flex(PhysicalAxis.Horizontal, [Toggle.Glyphed(glyph, Check.On) with { Accessible = new Accessible { Role = AccessRole.Image, Label = label } }]) { Role = Roles.Button }) {
            Cell = Some<Func<TRow, View>>(row => value(row) switch {
                var check => new Toggle(check, edit => edited(row, edit)) { Kind = ToggleKind.Button, Content = [Toggle.Glyphed(glyph, check)] }.Drawn switch {
                    var drawn => drawn with { Conditions = drawn.Conditions.TryAdd(Condition.Borderless), Accessible = drawn.Accessible.Combine(new Accessible { Label = label }) },
                },
            }),
            Order = Some<Comparison<TRow>>((left, right) => Rank(value(left)).CompareTo(Rank(value(right)))),
            Width = Sizing.Fit.Content,
        };

    private static int Rank(Check check) => check.Map(off: 0, on: 2, mixed: 1);
}

public sealed record Table<TRow>(Content<Seq<TRow>> Rows, Func<TRow, Key> Keyed, Seq<Column<TRow>> Columns, Func<Seq<Key>, Part.Choice> Choice) where TRow : notnull {

    // --- [STATE]

    public HashSet<Key> Realized { get; init; }
    public Seq<ColumnSort> Sort { get; init; }
    public Option<Func<Edit<Seq<ColumnSort>>, IO<Unit>>> Sorted { get; init; }
    public Option<Func<Edit<Seq<Column<TRow>>>, IO<Unit>>> Arranged { get; init; }
    public Search Filter { get; init; } = Search.Everything;
    public Option<Func<TRow, Seq<TRow>>> Children { get; init; }
    public Option<Func<TRow, Key>> Group { get; init; }
    public Option<Func<Key, Seq<TRow>, View>> GroupFace { get; init; }
    public HashSet<Key> Open { get; init; }
    public Option<Func<Edit<HashSet<Key>>, IO<Unit>>> Expanded { get; init; }
    public Option<Func<Seq<Key>, Key, Placement, Option<IO<Unit>>>> Moved { get; init; }
    public bool Numbered { get; init; }
    public bool KeepsEnd { get; init; }

    private Option<AccessRole> CellRole => Children.IsSome ? None : Some(AccessRole.GridCell);

    // --- [DRAWING]

    public View Drawn(CultureInfo culture) {
        Seq<Column<TRow>> shown = toSeq(Columns.Filter(static column => !column.Hidden).OrderByDescending(static column => column.Pinned));
        Func<TRow, bool> kept = row => Filter.Terms.IsEmpty || shown.Exists(column => column.Text.Exists(text => Filter.Matches(text(row), culture)));
        Seq<Row<Either<IGrouping<Key, TRow>, TRow>>> rows = Flattened(Rows.Current.IfNone([]), shown, kept, culture);
        int count = rows.Choose(static row => row.Item.ToOption()).Count;
        Seq<TRow> totaled = Rows.Current.IfNone([]).Filter(kept);
        Seq<View> footer = shown.Exists(static column => column.Footer.IsSome)
            ? [View.Divider(PhysicalAxis.Vertical), new View.Flex(PhysicalAxis.Horizontal, Gutter(None, count, culture) + shown.Map(column => Framed(column, column.Footer.Map(total => total(totaled)).IfNone(static () => new View.Box()), [], CellRole))) {
                Role = Roles.ListRow,
                Width = Sizing.Grow.Unit,
            }]
            : [];
        return new View.Scroll(PhysicalAxis.Horizontal, new View.Flex(PhysicalAxis.Vertical,
            Seq(Header(shown, count, culture), View.Divider(PhysicalAxis.Vertical), Rows.Drawn(_ => Body(rows, shown, count, culture))) + footer).Grown) {
            Conditions = [Condition.Borderless],
        }.Grown;
    }

    // --- [ROWS]

    private Seq<Row<Either<IGrouping<Key, TRow>, TRow>>> Flattened(Seq<TRow> current, Seq<Column<TRow>> shown, Func<TRow, bool> kept, CultureInfo culture) {
        Comparison<TRow> order = Sort
            .Choose(entry => shown.Find(column => column.Key == entry.Column).Bind(column => column.Order || column.Text.Map(text => (Comparison<TRow>)((left, right) => culture.CompareInfo.Compare(text(left), text(right), CompareOptions.NumericOrdering))))
                .Map(compare => entry.Descending ? (left, right) => compare(right, left) : compare))
            .Fold((Comparison<TRow>)(static (_, _) => 0), static (prior, next) => (left, right) => prior(left, right) switch { 0 => next(left, right), var held => held });
        Seq<TRow> Sorted(Seq<TRow> rows) => toSeq(rows.Order(System.Collections.Generic.Comparer<TRow>.Create(order)));
        Seq<Either<IGrouping<Key, TRow>, TRow>> roots = Group.Match(
            Some: of => toSeq(Sorted(current).GroupBy(of)).Map(static group => Either.Left<IGrouping<Key, TRow>, TRow>(group)),
            None: () => Sorted(current).Map(static row => Either.Right<IGrouping<Key, TRow>, TRow>(row)));
        Seq<Either<IGrouping<Key, TRow>, TRow>> Below(Either<IGrouping<Key, TRow>, TRow> item) =>
            item.Match(Left: static group => toSeq(group), Right: row => Children.Map(children => Sorted(children(row))).IfNone([])).Map(static row => Either.Right<IGrouping<Key, TRow>, TRow>(row));
        return Row<Either<IGrouping<Key, TRow>, TRow>>.Flatten(roots, Below, Itemized, item => !Filter.Terms.IsEmpty || Open.Contains(Itemized(item)), item => item.ToOption().Exists(kept));
    }

    private Key Itemized(Either<IGrouping<Key, TRow>, TRow> item) => item.Match(Left: static group => group.Key, Right: Keyed);

    private View Body(Seq<Row<Either<IGrouping<Key, TRow>, TRow>>> rows, Seq<Column<TRow>> shown, int count, CultureInfo culture) {
        HashMap<Key, int> numbers = toHashMap(rows.Filter(static row => row.Item.IsRight).Map(static (row, index) => (row.Key, index + 1)));
        View Lined(Row<Either<IGrouping<Key, TRow>, TRow>> row) {
            Seq<View> lead = Children.IsSome || Group.IsSome ? row.Lead(Expanded.Map(expanded => expanded(Edit.Of(EditPhase.Commit, Open.SymmetricExcept([row.Key]))))) : [];
            View line = new View.Flex(PhysicalAxis.Horizontal, row.Item.Match(
                Left: group => lead.Add(GroupFace.Match(Some: face => face(group.Key, toSeq(group)), None: () => Grouped(group, culture))),
                Right: datum => Gutter(numbers.Find(row.Key), count, culture) + shown.Map((column, index) => Cell(column, datum, index is 0 ? lead : [], culture)))) {
                Key = row.Key,
            }.Grown;
            Accessible accessible = new() { Role = Children.IsSome ? AccessRole.TreeItem : AccessRole.Row, Expanded = row.Open };
            return Moved.Match(
                Some: moved => new View.Flex(PhysicalAxis.Vertical, [line]) {
                    Accessible = accessible,
                    Drop = new Drop((transfer, target, placement) =>
                        from keys in Optional(transfer as Transfer.Keys)
                        from over in target
                        from effect in moved(keys.Paths.Choose(static path => path.Last), over, placement)
                        select effect) { Nests = Children.IsSome },
                }.Grown,
                None: () => line with { Accessible = accessible });
        }
        return new View.Scroll(PhysicalAxis.Vertical, Row<Either<IGrouping<Key, TRow>, TRow>>.Drawn(rows, Choice, Lined, Realized) with {
            Width = Sizing.Grow.Unit,
            Lift = Moved.Map(static _ => Lift.Of(static keys => new Transfer.Keys(keys.Map(static key => Seq(key))))),
            Accessible = new Accessible { Role = Children.IsSome ? AccessRole.Tree : AccessRole.Grid },
        }) {
            KeepsEnd = KeepsEnd,
        }.Grown;
    }

    private static View.Flex Grouped(IGrouping<Key, TRow> group, CultureInfo culture) =>
        new(PhysicalAxis.Horizontal, [
            new View.Text([group.Key.Identifier]) { MaxLines = 1, Align = UnitPoint.Center },
            new View.Text([group.Count().ToString(culture)]) { Role = Roles.Badge, Align = UnitPoint.Center },
        ]) {
            Width = Sizing.Grow.Unit,
        };

    private Seq<View> Gutter(Option<int> number, int count, CultureInfo culture) =>
        Numbered ? [CodeView.Number(number, count, culture) with { Role = Roles.Gutter, Height = Sizing.Grow.Unit, Sticks = PhysicalAxis.Horizontal }] : [];

    // --- [CELLS]

    private View Cell(Column<TRow> column, TRow row, Seq<View> lead, CultureInfo culture) {
        View content = column.Cell.Map(cell => cell(row)).IfNone(() => column.Text.Map(text => (View)new View.Text(Filter.Marked(text(row), culture)) {
            MaxLines = 1,
            Edited = column.Renamed.Map(renamed => (Func<Edit<string>, IO<Unit>>)(edit => renamed(row, edit))),
        }).IfNone(static () => new View.Box()));
        return Framed(column, lead.IsEmpty ? content : new View.Flex(PhysicalAxis.Horizontal, lead.Add(content)), [], CellRole);
    }

    private static View Framed(Column<TRow> column, View content, Seq<View> above, Option<AccessRole> role) {
        View aligned = content with { Align = new UnitPoint(column.Align, Fraction.Half) };
        return (column.Width is Sizing.Fit ? new View.Flex(PhysicalAxis.Horizontal, [aligned]) : (View)new View.Overlay(new View.Box(), aligned.Cons(above))) with {
            Key = column.Key,
            Role = Roles.Cell,
            Width = column.Width,
            Height = Sizing.Grow.Unit,
            Sticks = column.Pinned ? Some(PhysicalAxis.Horizontal) : None,
            Accessible = new Accessible { Role = role },
        };
    }

    // --- [HEADER]

    private View Header(Seq<Column<TRow>> shown, int count, CultureInfo culture) {
        Seq<Key> keys = shown.Map(static column => column.Key);
        Seq<View> heads = shown.Map<View>(column => Head(column, culture));
        View Lined(Seq<View> cells) =>
            new View.Flex(PhysicalAxis.Horizontal, Gutter(None, count, culture) + cells) { Role = Roles.ListRow, Width = Sizing.Grow.Unit, Accessible = new Accessible { Role = AccessRole.Row } };
        Option<Key> lead = Sort.Head.Map(static entry => entry.Column);
        return Sorted.IsSome || Arranged.IsSome
            ? new View.Overlay(new Part.Choice(keys, new Selection.Many(toHashSet(Sort.Map(static entry => entry.Column))) { Anchor = lead, Focus = lead }, edit => Sorted.Filter(_ => edit.Phase != EditPhase.Step).Match(
                Some: sorted => sorted(edit.Map(next => Resorted(next, keys)).As()),
                None: static () => IO.pure(unit))) {
                Reorder = Arranged.Map(arranged => Arranging<Seq<Key>>(arranged, Reordered)),
            }.Drawn(heads, Lined), [new View.Layer(Menu) { Trigger = new Trigger.Secondary() }]) { Width = Sizing.Grow.Unit }
            : Lined(heads);
    }

    private View.Flex Head(Column<TRow> column, CultureInfo culture) {
        Seq<View> marks = column.Width is Sizing.Fit
            ? []
            : Sort.Map(static (entry, rank) => (Entry: entry, Rank: rank)).Find(held => held.Entry.Column == column.Key).Map(held =>
                Seq<View>(new View.Box { Role = Roles.Sorter, Conditions = Direction(column.Key) })
                + (Sort.Count > 1 ? Seq<View>(new View.Text([(held.Rank + 1).ToString(culture)]) { Role = Roles.Badge }) : [])).IfNone([]);
        View cell = Framed(column, marks.IsEmpty ? column.Header : new View.Flex(PhysicalAxis.Horizontal, column.Header.Cons(marks)), Arranged.Bind(arranged => Grip(column, arranged)).ToSeq(), None);
        return new View.Flex(PhysicalAxis.Horizontal, [cell with { Key = None, Width = Sizing.Grow.Unit, Sticks = None }]) {
            Key = column.Key,
            Role = Roles.Stack,
            Width = column.Width,
            Sticks = cell.Sticks,
            Accessible = new Accessible { Role = AccessRole.GridCell },
        };
    }

    private Option<View> Grip(Column<TRow> column, Func<Edit<Seq<Column<TRow>>>, IO<Unit>> arranged) =>
        from width in Optional(column.Width as Sizing.Fixed)
        from domain in AxisRange.Validated(width.Limits.Min, float.Min(width.Limits.Max, float.MaxValue)).ToOption()
        let plane = new PlotPlane.Track(PhysicalAxis.Horizontal, new Axis.Linear(domain))
        select new Handles<float>(width.Extent, Points<float>.Scalar(plane, Lens.identity<float>()), Arranging<float>(arranged, extent => Changed(column.Key, held => held with { Width = new Sizing.Fixed(extent) { Limits = width.Limits } }))) {
            Gearing = Some(1f),
            Handle = (_, _) => new View.Pin(new Vector2(domain.Low, 0f), new View.Box().Grown) { Span = new Vector2(domain.Span, 1f) },
        }.Drawn(plane, [new View.Box { Role = Roles.Grip }]) with {
            Width = Sizing.Fit.Content,
            Height = Sizing.Grow.Unit,
            Align = new UnitPoint(Fraction.MaxValue, Fraction.Half),
        };

    private View.Flex Menu =>
        new(PhysicalAxis.Vertical, Columns.Map(column => (View)new View.Flex(PhysicalAxis.Horizontal,
            Seq(Arranged.Match(
                Some: arranged => new Toggle(!column.Hidden, Arranging<bool>(arranged, shown => Changed(column.Key, held => held with { Hidden = !shown }))) { Content = [column.Header] }.Drawn,
                None: () => column.Header) with {
                Width = Sizing.Grow.Unit,
            })
            + Sorted.Filter(_ => column.Order.IsSome || column.Text.IsSome).Map(sorted =>
                new Part.Press(_ => sorted(Edit.Of(EditPhase.Commit, Cycled(column.Key)))).Drawn(new View.Box { Role = Roles.Sorter, Conditions = Direction(column.Key) })).ToSeq()
            + Arranged.Map(arranged => new Toggle(column.Pinned, Arranging<bool>(arranged, pinned => Changed(column.Key, held => held with { Pinned = pinned }))) {
                Kind = ToggleKind.Button,
                Content = [new View.Box { Role = Roles.Pin }],
            }.Drawn).ToSeq()) {
            Key = column.Key,
            Width = Sizing.Grow.Unit,
        })) {
            Role = Roles.Menu,
            Accessible = new Accessible { Role = AccessRole.List },
        };

    // --- [ARRANGEMENT]

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
        edit => arranged(edit.Map(columns).As());
}
