using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using LanguageExt.ClassInstances;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ListColumn<TItem> where TItem : notnull {
    private ListColumn(Option<string> caption, bool renames, bool edits, bool stretches, Option<Comparison<TItem>> order) =>
        (Caption, Renames, Edits, Stretches, Order) = (caption, renames, edits, stretches, order);

    public Option<string> Caption { get; }
    public bool Renames { get; }
    public bool Edits { get; }
    public bool Stretches { get; }
    public Option<Comparison<TItem>> Order { get; }

    public sealed record Label(Option<string> Caption, Func<TItem, string> Read, Option<Func<TItem, IconResource>> Icon, Option<Func<TItem, string, Fin<TItem>>> Rename)
        : ListColumn<TItem>(Caption, Rename.IsSome, Rename.IsSome, stretches: true, Some<Comparison<TItem>>((left, right) => Localization.LogicalSort(Read(left), Read(right))));

    public sealed record Check(Option<string> Caption, Lens<TItem, bool> Value)
        : ListColumn<TItem>(Caption, renames: false, edits: true, stretches: false, Some<Comparison<TItem>>((left, right) => Value.Get(left).CompareTo(Value.Get(right))));

    public sealed record Glyph(Option<string> Caption, Func<TItem, IconResource> Icon) : ListColumn<TItem>(Caption, renames: false, edits: false, stretches: false, None);

    public sealed record Progress(Option<string> Caption, Func<TItem, Option<float>> Read)
        : ListColumn<TItem>(Caption, renames: false, edits: false, stretches: true, Some<Comparison<TItem>>((left, right) => Read(left).IfNone(-1f).CompareTo(Read(right).IfNone(-1f))));
}

public sealed record ListSearch<TItem>(string Placeholder, Func<TItem, string> Text) where TItem : notnull {
    public bool Shown { get; init; } = true;
}

public sealed record ListBranches<TItem, TKey>(Lens<TItem, Seq<TItem>> Children, HashSet<TKey> Expanded, Func<Edit<HashSet<TKey>>, IO<Unit>> Edited)
    where TItem : notnull
    where TKey : notnull;

public sealed record ListRow<TItem, TKey>(IterableNE<ListColumn<TItem>> Columns, Func<TItem, TKey> Key, Seq<TItem> Items, HashSet<TKey> Selected) : IRow<ListRow<TItem, TKey>>
    where TItem : notnull
    where TKey : notnull {
    public Func<Edit<Seq<TItem>>, IO<Unit>> Edited { get; init; } = static _ => IO.pure(unit);
    public Func<Edit<HashSet<TKey>>, IO<Unit>> Picked { get; init; } = static _ => IO.pure(unit);
    public Option<ListBranches<TItem, TKey>> Branches { get; init; }
    public Option<TKey> Current { get; init; }
    public Option<Func<TItem, string>> Ordered { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Option<Func<TItem, bool>> Available { get; init; }
    public Option<Func<TItem, IO<Unit>>> Activate { get; init; }
    public Option<string> Empty { get; init; }
    public Option<(Command<Invoke>.Run Add, Command<Invoke>.Run Remove)> Pair { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<Invoke>>> Commands { get; init; } = static _ => [];
    public bool AllowMultipleSelection { get; init; }
    public bool Reorderable { get; init; }

    public RowShape Shape { get; init; } = RowShape.Filled;

    public static IO<RowCells> Realize(IO<ListRow<TItem, TKey>> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from grid in use(static () => new TreeGridView { GridLines = GridLines.None, Border = BorderType.None, AllowEmptySelection = true })
        from faced in IO.lift(() => { (grid.ShowHeader, grid.AllowMultipleSelection) = (toSeq(first.Columns).Exists(static column => column.Caption.IsSome), first.AllowMultipleSelection); })
        from emphasis in IO.lift(static () => SystemFonts.Bold(SystemFonts.Label().Size))
        from list in IO.lift(static () => Atom(new ListState<TItem>([], [], [], [], None)))
        from keep in atomIO<Func<TItem, bool>>(static _ => true)
        from columns in Lists.Columns(first, grid, list)
        from commands in Lists.Commanded(first.Pair, first.Commands(IO.lift(() => Lists.Chosen<TItem>(grid).Map(static node => node.Row))), scope)
        let shown =
            from current in latest
            from filled in Lists.Fill(grid, list, keep, current)
            from opened in current.Branches.Traverse(branches => Lists.Expand(grid, list, current, branches.Expanded)).As()
            from reselected in Lists.Reselected(grid, list)
            from scrolled in IO.lift(() => current.Current.Iter(key => Lists.Indices(current, list.Value.Nodes.Bind(static node => node.Shown), [key]).Head.Iter(grid.ScrollToRow)))
            select unit
        from selection in Values.Bound(grid, new BindableBinding<TreeGridView, Option<HashSet<TKey>>>(grid,
                held => Some(toHashSet(Lists.Chosen<TItem>(held).Map(node => first.Key(node.Row)))),
                (held, keys) => keys.Iter(chosen => held.SelectedRows = Lists.Indices(first, atomic(() => list.Value).Nodes.Bind(static node => node.Shown), chosen)),
                static (held, handler) => held.SelectionChanged += handler,
                static (held, handler) => held.SelectionChanged -= handler),
            static _ => false, None, Some(first.Selected), edit => latest.Bind(current => current.Picked(edit)))
        from editing in Subscriptions.On<GridViewCellEventArgs>(h => grid.CellEdited += h, h => grid.CellEdited -= h).Attach(args =>
            from current in latest
            from committed in RowItem<TItem>.Of(args.Item).Traverse(node => current.Edited(EditPhase.Commit.Of(Lists.Replaced(current.Key, current.Branches, current.Items, node.Row)))).As()
            select unit)
        from expanding in Subscriptions.Merged(
                Subscriptions.On<TreeGridViewItemEventArgs>(h => grid.Expanded += h, h => grid.Expanded -= h),
                Subscriptions.On<TreeGridViewItemEventArgs>(h => grid.Collapsed += h, h => grid.Collapsed -= h)).Attach(_ =>
            from current in latest
            from state in atomicIO(() => list.Value)
            from opened in current.Branches.Traverse(branches => branches.Edited(EditPhase.Commit.Of(Lists.Open(current, state.Nodes)))).As()
            select unit)
        from sorting in Subscriptions.On<GridColumnEventArgs>(h => grid.ColumnHeaderClick += h, h => grid.ColumnHeaderClick -= h).Attach(args =>
            list.SwapIO(state => state with {
                Sorted = Some((Column: grid.Columns.IndexOf(args.Column), Descending: state.Sorted.Exists(held => held.Column == grid.Columns.IndexOf(args.Column) && !held.Descending))),
            }).Bind(_ => shown))
        from gestures in Lists.Gestures(grid, latest, emphasis, commands.Keyed, list, shown)
        from emptied in first.Empty.Traverse(empty => Lists.Emptied(grid, list, LocalText.Of(empty).Local)).As()
        from paired in Lists.Paired(emptied.IfNone(grid), commands.Pair)
        from content in Lists.Searched(paired, first.Search, kept => writeIO(keep, kept).Bind(_ => shown))
        select new RowCells(content.Control, shown.Bind(_ => latest).Bind(current => content.Shown(current.Search) >> selection(Some(current.Selected))).Bind(_ => commands.Refresh)) {
            Menu = commands.Menu,
        };
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class RowItem<TItem> : TreeGridItem where TItem : notnull {
    internal RowItem(TItem row, int depth, Seq<RowItem<TItem>> kids) : base(kids.Map<ITreeGridItem>(static kid => kid)) =>
        (Row, Depth, Kids) = (row, depth, kids);

    public TItem Row { get; internal set; }
    public int Depth { get; }
    public Seq<RowItem<TItem>> Kids { get; }
    public Seq<RowItem<TItem>> Preorder => this.Cons(Kids.Bind(static kid => kid.Preorder));
    public Seq<RowItem<TItem>> Shown => this.Cons(Expanded ? Kids.Bind(static kid => kid.Shown) : []);

    public static Option<RowItem<TItem>> Of(object? item) => item is RowItem<TItem> node ? Some(node) : None;
}

internal sealed record ListState<TItem>(Seq<RowItem<TItem>> Nodes, TreeGridItemCollection Store, Seq<(RowItem<TItem> Node, TItem Row)> Reloads, HashMap<IconResource, Icon> Icons, Option<(int Column, bool Descending)> Sorted)
    where TItem : notnull;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Lists {
    // --- [STATE]
    internal static Seq<RowItem<TItem>> Chosen<TItem>(TreeGridView grid) where TItem : notnull => toSeq(grid.SelectedItems).Choose(RowItem<TItem>.Of);

    internal static Seq<int> Indices<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> shown, HashSet<TKey> keys) where TItem : notnull where TKey : notnull =>
        shown.Choose((index, node) => Conversions.Present(keys.Contains(row.Key(node.Row)), index));

    internal static HashSet<TKey> Open<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> nodes) where TItem : notnull where TKey : notnull =>
        toHashSet(nodes.Bind(static node => node.Preorder).Filter(static node => node.Expanded).Map(node => row.Key(node.Row)));

    internal static IO<Unit> Reselected<TItem>(TreeGridView grid, Atom<ListState<TItem>> list) where TItem : notnull =>
        list.ValueIO.Bind(state => unless(ReferenceEquals(grid.DataStore, state.Store), IO.lift(fun(() => { grid.DataStore = state.Store; }))).As());

    // --- [FILL]
    internal static IO<Unit> Fill<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, Atom<Func<TItem, bool>> keep, ListRow<TItem, TKey> row) where TItem : notnull where TKey : notnull =>
        from kept in keep.ValueIO
        from icons in toSeq(toHashSet(
                from column in toSeq(row.Columns)
                from icon in column.Switch(
                    label: static label => label.Icon.ToSeq(),
                    check: static _ => Seq<Func<TItem, IconResource>>(),
                    glyph: static glyph => Seq(glyph.Icon),
                    progress: static _ => Seq<Func<TItem, IconResource>>())
                from item in Flattened(row, row.Items)
                select icon(item)))
            .TraverseM(static icon => Icons.Frames(icon, IconSlot.Small).Map(frames => (icon, frames))).As()
        from next in list.SwapIO(state => Filled(row, state with { Icons = toHashMap(icons) }, kept))
        from reloaded in next.Reloads.TraverseM(reload => IO.lift(fun(() => {
            reload.Node.Row = reload.Row;
            grid.ReloadItem(reload.Node, reloadChildren: false);
        }))).As()
        select unit;

    private static Seq<TItem> Flattened<TItem, TKey>(ListRow<TItem, TKey> row, Seq<TItem> items) where TItem : notnull where TKey : notnull =>
        items.Bind(item => item.Cons(row.Branches.ToSeq().Bind(branches => Flattened(row, branches.Children.Get(item)))));

    private static ListState<TItem> Filled<TItem, TKey>(ListRow<TItem, TKey> row, ListState<TItem> state, Func<TItem, bool> keep) where TItem : notnull where TKey : notnull =>
        (Before: state.Nodes.Bind(static node => node.Preorder), After: Built(row, state.Sorted, row.Items, Open(row, state.Nodes), keep, 0)) switch {
            var built => built.After.Bind(static node => node.Preorder) switch {
                var after when after.Map(node => (row.Key(node.Row), node.Depth)) == built.Before.Map(node => (row.Key(node.Row), node.Depth)) => state with {
                    Reloads = built.Before.Zip(after).Filter(static pair => !EqDefault<TItem>.Equals(pair.First.Row, pair.Second.Row)).Map(static pair => (pair.First, pair.Second.Row)),
                },
                _ => state with { Nodes = built.After, Store = new TreeGridItemCollection(built.After.Map<ITreeGridItem>(static node => node)), Reloads = [] },
            },
        };

    private static Seq<RowItem<TItem>> Built<TItem, TKey>(ListRow<TItem, TKey> row, Option<(int Column, bool Descending)> sorted, Seq<TItem> items, HashSet<TKey> open, Func<TItem, bool> keep, int depth)
        where TItem : notnull
        where TKey : notnull =>
        Sorted(items, (from held in sorted
                       from column in toSeq(row.Columns).At(held.Column)
                       from order in column.Order
                       select held.Descending ? (left, right) => order(right, left) : order)
                | row.Ordered.Map(static name => (Comparison<TItem>)((left, right) => Localization.LogicalSort(name(left), name(right)))))
            .Map(item => (Item: item, Kids: Built(row, sorted, row.Branches.ToSeq().Bind(branches => branches.Children.Get(item)), open, keep, depth + 1)))
            .Filter(pair => !pair.Kids.IsEmpty || keep(pair.Item))
            .Map(pair => new RowItem<TItem>(pair.Item, depth, pair.Kids) { Expanded = open.Contains(row.Key(pair.Item)) });

    private static Seq<TItem> Sorted<TItem>(Seq<TItem> items, Option<Comparison<TItem>> order) =>
        order.Match(
            Some: compare => {
                TItem[] sorted = [.. items];
                sorted.AsSpan().Sort(compare);
                return toSeq(sorted);
            },
            None: () => items);

    internal static Seq<TItem> Replaced<TItem, TKey>(Func<TItem, TKey> key, Option<ListBranches<TItem, TKey>> branches, Seq<TItem> items, TItem edited) where TItem : notnull where TKey : notnull =>
        items.Map(item => EqDefault<TKey>.Equals(key(item), key(edited))
            ? edited
            : branches.Fold(item, (held, tree) => tree.Children.Update(kids => Replaced(key, branches, kids, edited), held)));

    private static Seq<TItem> Reordered<TItem, TKey>(ListRow<TItem, TKey> row, Seq<TItem> items, HashSet<TKey> moved, TKey target, bool after) where TItem : notnull where TKey : notnull =>
        items.Exists(item => EqDefault<TKey>.Equals(row.Key(item), target)) && items.Filter(item => moved.Contains(row.Key(item))).Count == moved.Count
            ? items.Filter(item => !moved.Contains(row.Key(item))) switch {
                var rest => rest.Map(static (item, at) => (Item: item, At: at)).Find(held => EqDefault<TKey>.Equals(row.Key(held.Item), target)).Map(held => held.At + (after ? 1 : 0)).IfNone(rest.Count) switch {
                    var place => rest.Take(place) + items.Filter(item => moved.Contains(row.Key(item))) + rest.Skip(place),
                },
            }
            : items.Map(item => row.Branches.Fold(item, (held, tree) => tree.Children.Update(kids => Reordered(row, kids, moved, target, after), held)));

    // --- [COLUMNS]
    internal static IO<Unit> Columns<TItem, TKey>(ListRow<TItem, TKey> row, TreeGridView grid, Atom<ListState<TItem>> list) where TItem : notnull where TKey : notnull =>
        toSeq(row.Columns).Map(static (column, index) => (Column: column, Index: index)).TraverseM(entry =>
            from cell in CellFor(entry.Column, list)
            from column in use(static () => new GridColumn())
            from placed in IO.lift(() => {
                (column.HeaderText, column.DataCell, column.Editable, column.Resizable, column.Sortable, column.Expand) =
                    (Conversions.Unset(entry.Column.Caption.Map(static caption => LocalText.Of(caption).Local)), cell, entry.Column.Edits, false, entry.Column.Order.IsSome,
                     entry.Column.Stretches && toSeq(row.Columns).Skip(entry.Index + 1).ForAll(static later => !later.Stretches));
                grid.Columns.Add(column);
            })
            select unit).As().Map(unit);

    private static IO<Cell> CellFor<TItem>(ListColumn<TItem> column, Atom<ListState<TItem>> list) where TItem : notnull =>
        column.Switch(list,
            label: static (held, label) => label.Icon.Match(
                Some: icon =>
                    from cell in use(static () => new ImageTextCell())
                    from bound in IO.lift(() => { (cell.ImageBinding, cell.TextBinding) = (Pictured(icon, held), Written(label)); })
                    select (Cell)cell,
                None: () =>
                    from cell in use(static () => new TextBoxCell())
                    from bound in IO.lift(() => cell.Binding = Written(label))
                    select (Cell)cell),
            check: static (_, check) =>
                from cell in use(static () => new CheckBoxCell())
                from bound in IO.lift(() => cell.Binding = Binding.Delegate<RowItem<TItem>, bool?>(node => check.Value.Get(node.Row), (node, value) => node.Row = check.Value.Set(value is true, node.Row)))
                select (Cell)cell,
            glyph: static (held, glyph) =>
                from cell in use(static () => new ImageViewCell())
                from bound in IO.lift(() => cell.Binding = Pictured(glyph.Icon, held))
                select (Cell)cell,
            progress: static (_, progress) =>
                from cell in use(static () => new ProgressCell())
                from bound in IO.lift(() => cell.Binding = Binding.Delegate<RowItem<TItem>, float?>(node => progress.Read(node.Row).ToNullable()))
                select (Cell)cell);

    private static IndirectBinding<Image?> Pictured<TItem>(Func<TItem, IconResource> icon, Atom<ListState<TItem>> list) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, Image?>(node => list.Value.Icons.Find(icon(node.Row)).ValueUnsafe());

    private static IndirectBinding<string> Written<TItem>(ListColumn<TItem>.Label label) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, string>(
            node => label.Read(node.Row),
            (node, value) => _ = label.Rename.Bind(rename => rename(node.Row, value).ToOption()).Iter(item => node.Row = item));

    // --- [EXPANSION]
    internal static IO<Unit> Expand<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, HashSet<TKey> open) where TItem : notnull where TKey : notnull =>
        list.ValueIO.Bind(state => state.Nodes.Bind(static node => node.Preorder)
            .Filter(node => node.Expanded != open.Contains(row.Key(node.Row)))
            .TraverseM(node => IO.lift(fun(() => {
                node.Expanded = !node.Expanded;
                grid.ReloadItem(node, reloadChildren: true);
            })))
            .As())
            .Map(unit);

    // --- [GESTURES]
    internal static IO<Unit> Gestures<TItem, TKey>(TreeGridView grid, IO<ListRow<TItem, TKey>> latest, Font emphasis, Func<Keys, IO<bool>> keyed, Atom<ListState<TItem>> list, IO<Unit> shown)
        where TItem : notnull
        where TKey : notnull =>
        from dragging in IO.lift(static () => Ref(HashSet<TKey>()))
        from gestures in Products.Each(Seq(
            Subscriptions.Inline<KeyEventArgs>(h => grid.KeyDown += h, h => grid.KeyDown -= h).Attach(args => latest.Bind(current => Pressed(grid, current, keyed, args))),
            Subscriptions.Inline<MouseEventArgs>(h => grid.MouseDown += h, h => grid.MouseDown -= h).Attach(args => when(args.Buttons is MouseButtons.Alternate, IO.lift(() => RowItem<TItem>.Of(grid.GetCellAt(args.Location).Item)
                .Filter(node => !Chosen<TItem>(grid).Exists(held => ReferenceEquals(held, node)))
                .Iter(node => grid.SelectedItem = node))).As()),
            Subscriptions.Inline<GridCellFormatEventArgs>(h => grid.CellFormatting += h, h => grid.CellFormatting -= h).Attach(args => latest.Bind(current => IO.lift(() => RowItem<TItem>.Of(args.Item).Iter(node =>
                (args.Font, args.ForegroundColor) = (
                    node.Kids.IsEmpty ? args.Font : emphasis,
                    current.Available.ForAll(available => available(node.Row)) ? args.ForegroundColor : SystemColors.DisabledText))))),
            Subscriptions.On<TreeGridViewItemEventArgs>(h => grid.Activated += h, h => grid.Activated -= h).Attach(_ => latest.Bind(current => (from activate in current.Activate from node in Chosen<TItem>(grid).Head select activate(node.Row))
                .Traverse(static activated => activated).As()).Map(unit)),
            Subscriptions.Inline<MouseEventArgs>(h => grid.MouseMove += h, h => grid.MouseMove -= h).Attach(args => latest.Bind(current =>
                when(current.Reorderable && args.Buttons is MouseButtons.Primary && atomic(() => dragging.Value).IsEmpty,
                    from moved in atomicIO(() => dragging.Value = toHashSet(Chosen<TItem>(grid).Map(node => current.Key(node.Row))))
                    from dropped in when(!moved.IsEmpty, use(static () => new DataObject { Text = "" }).Bind(data => IO.lift(() => grid.DoDragDrop(data, DragEffects.Move))).Map(unit)).As()
                    select unit).As())),
            Subscriptions.Inline<DragEventArgs>(h => grid.DragOver += h, h => grid.DragOver -= h).Attach(args => IO.lift(() => {
                grid.GetDragInfo(args)?.RestrictToInsert();
                args.Effects = atomic(() => dragging.Value).IsEmpty ? DragEffects.None : DragEffects.Move;
            })),
            Subscriptions.Inline<DragEventArgs>(h => grid.DragDrop += h, h => grid.DragDrop -= h).Attach(args =>
                from current in latest
                from moved in atomicIO(() => {
                    HashSet<TKey> held = dragging.Value;
                    dragging.Value = [];
                    return held;
                })
                from info in IO.lift(() => Optional(grid.GetDragInfo(args)))
                from sent in (from drop in info
                              from node in RowItem<TItem>.Of(drop.Item)
                              where !moved.IsEmpty
                              select (Target: current.Key(node.Row), After: drop.Position is GridDragPosition.After))
                    .Traverse(drop => current.Edited(EditPhase.Commit.Of(Reordered(current, current.Items, moved, drop.Target, drop.After)))).As()
                select unit),
            Subscriptions.Inline<EventArgs>(h => grid.DragEnd += h, h => grid.DragEnd -= h).Attach(_ => atomicIO(() => dragging.Value = []).Map(unit)),
            Products.Changed(() => grid.AllowDrop, value => grid.AllowDrop = value, value: true),
            HostTheme.Applied(grid, list.SwapIO(static state => state with { Icons = [] }).Bind(_ => shown).Bind(_ => IO.lift(grid.ReloadData))))).Map(unit)
        select unit;

    private static IO<Unit> Pressed<TItem, TKey>(TreeGridView grid, ListRow<TItem, TKey> row, Func<Keys, IO<bool>> keyed, KeyEventArgs args) where TItem : notnull where TKey : notnull =>
        from rows in IO.lift(() => toSeq(grid.SelectedRows).Strict())
        from handled in args.KeyData switch {
            Keys.F2 => Handled(args,
                from at in rows.Head
                from renaming in toSeq(row.Columns).Map(static (each, index) => (Column: each, Index: index)).Find(static entry => entry.Column.Renames)
                select (Action)(() => grid.BeginEdit(at, renaming.Index))),
            Keys.Up or Keys.Down when rows.IsEmpty && !row.Items.IsEmpty => Handled(args, Some(() => grid.SelectRow(0))),
            _ => CommandRows.Pressed(keyed, args),
        }
        select handled;

    private static IO<Unit> Handled(KeyEventArgs args, Option<Action> action) =>
        IO.lift(fun(() => {
            _ = action.Iter(static run => run());
            args.Handled = action.IsSome;
        }));

    // --- [COMMANDS]
    internal static IO<(Option<Control> Pair, Seq<Seq<Command<Invoke>>> Menu, Func<Keys, IO<bool>> Keyed, IO<Unit> Refresh)> Commanded(
        Option<(Command<Invoke>.Run Add, Command<Invoke>.Run Remove)> pair, Seq<Command<Invoke>> commands, RowScope scope) =>
        from paired in pair.Traverse(held => CommandRows.AddRemove(held.Add, held.Remove, scope)).As()
        let menu = pair.Map(static held => Seq<Command<Invoke>>(held.Add, held.Remove)).ToSeq().Add(commands)
        from keyed in CommandRows.Keyed(menu, scope)
        select (paired.Map(static held => held.Control), menu, keyed, paired.Map(static held => held.Refresh).IfNone(IO.pure(unit)));

    // --- [PLACEMENT]
    internal static IO<Control> Emptied<TItem>(TreeGridView grid, Atom<ListState<TItem>> list, string text) where TItem : notnull =>
        from label in use(() => new Label { Text = text, TextColor = SystemColors.DisabledText, TextAlignment = TextAlignment.Center })
        from stack in Containers.Frame(static () => new StackLayout { HorizontalContentAlignment = HorizontalAlignment.Stretch })
        let shown = fun((ListState<TItem> state) => IO.lift(() => (grid.Visible, label.Visible) = (!state.Nodes.IsEmpty, state.Nodes.IsEmpty)).Map(unit))
        from placed in IO.lift(() => {
            stack.Items.Add(new StackLayoutItem(grid, expand: true));
            stack.Items.Add(label);
        })
        from heard in Subscriptions.Changed(list).Attach(shown)
        from current in list.ValueIO.Bind(shown)
        select (Control)stack;

    internal static IO<Control> Paired(Control content, Option<Control> pair) =>
        pair.Match(Some: buttons => Stacked(new StackLayoutItem(content, expand: true), new StackLayoutItem(buttons, HorizontalAlignment.Left)), None: () => IO.pure(content));

    private static IO<Control> Stacked(params Seq<StackLayoutItem> items) =>
        Containers.Frame(() => new StackLayout([.. items]) {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Panel),
        }).Map(static stack => (Control)stack);

    internal static IO<(Control Control, Func<Option<ListSearch<TItem>>, IO<Unit>> Shown)> Searched<TItem>(Control content, Option<ListSearch<TItem>> search, Func<Func<TItem, bool>, IO<Unit>> keep) where TItem : notnull =>
        search.Match(
            Some: held =>
                from field in use(() => new SearchBox { PlaceholderText = LocalText.Of(held.Placeholder).Local })
                from typed in Subscriptions.On<EventArgs>(h => field.TextChanged += h, h => field.TextChanged -= h).Attach(_ =>
                    IO.lift(() => field.Text.Trim()).Bind(query => keep(item => LocalText.Culture.CompareInfo.IndexOf(held.Text(item), query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)))
                from stack in Stacked(new StackLayoutItem(field), new StackLayoutItem(content, expand: true))
                select (stack, (Func<Option<ListSearch<TItem>>, IO<Unit>>)(current => IO.lift(() => { field.Visible = current.Exists(static shown => shown.Shown); }))),
            None: () => IO.pure((content, (Func<Option<ListSearch<TItem>>, IO<Unit>>)(static _ => IO.pure(unit)))));
}
