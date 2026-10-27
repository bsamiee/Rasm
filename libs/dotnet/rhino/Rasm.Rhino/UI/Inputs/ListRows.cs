using Eto.Drawing;
using Eto.Forms;
using LanguageExt.ClassInstances;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ListColumn<TItem> where TItem : notnull {
    private ListColumn(Option<string> caption) => Caption = caption;

    public Option<string> Caption { get; }

    public bool Renames => Switch(label: static label => label.Rename.IsSome, check: static _ => false, glyph: static _ => false, progress: static _ => false);

    public bool Edits => Switch(label: static label => label.Rename.IsSome, check: static _ => true, glyph: static _ => false, progress: static _ => false);

    public bool Stretches => Map(label: true, check: false, glyph: false, progress: true);

    public sealed record Label(Option<string> Caption, Func<TItem, string> Read, Option<Func<TItem, IconResource>> Icon, Option<Func<TItem, string, Fin<TItem>>> Rename)
        : ListColumn<TItem>(Caption);

    public sealed record Check(Option<string> Caption, Lens<TItem, bool> Value) : ListColumn<TItem>(Caption);

    public sealed record Glyph(Option<string> Caption, Func<TItem, IconResource> Icon) : ListColumn<TItem>(Caption);

    public sealed record Progress(Option<string> Caption, Func<TItem, Option<float>> Read) : ListColumn<TItem>(Caption);
}

public sealed record ListSearch<TItem>(string Placeholder, Func<TItem, string> Text) where TItem : notnull {
    public bool Shown { get; init; } = true;
}

public sealed record ListBranches<TItem, TKey>(Lens<TItem, Seq<TItem>> Children, HashSet<TKey> Expanded, Func<Edit<HashSet<TKey>>, IO<Unit>> Edited)
    where TItem : notnull
    where TKey : notnull;

public sealed record ListRow<TItem, TKey>(IterableNE<ListColumn<TItem>> Columns, Func<TItem, TKey> Key, Seq<TItem> Items, HashSet<TKey> Selected)
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
}

public sealed record BrowserRow<TItem, TKey>(Func<TItem, TKey> Key, Func<TItem, string> Label, Func<RowScope, TItem, Size, IO<Image>> Tile, Seq<TItem> Items, Size TileSize, string Empty)
    where TItem : notnull
    where TKey : notnull {
    public Option<TKey> Current { get; init; }
    public Func<Edit<TKey>, IO<Unit>> Chosen { get; init; } = static _ => IO.pure(unit);
    public Option<ListSearch<TItem>> Search { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<Invoke>>> Commands { get; init; } = static _ => [];
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ListRows {
    // --- [LISTS]
    public static ControlRow List<TItem, TKey>(string label, ListRow<TItem, TKey> row)
        where TItem : notnull
        where TKey : notnull =>
        new ControlRow.Bound<ListRow<TItem, TKey>>(label, RowShape.Filled, row, static (latest, scope) =>
            from first in latest
            from grid in use(static () => new TreeGridView())
            from faced in IO.lift(() => {
                (grid.ShowHeader, grid.GridLines, grid.Border, grid.AllowMultipleSelection, grid.AllowEmptySelection) =
                    (toSeq(first.Row.Columns).Exists(static column => column.Caption.IsSome), GridLines.None, BorderType.None, first.Row.AllowMultipleSelection, true);
            })
            from emphasis in IO.lift(static () => SystemFonts.Bold(SystemFonts.Label().Size))
            from list in IO.lift(static () => Atom(new ListState<TItem>(Seq<RowItem<TItem>>(), [], Seq<(RowItem<TItem> Node, TItem Row)>())))
            from keep in atomIO<Func<TItem, bool>>(static _ => true)
            from columns in Columns(first.Row, grid, scope.Owner)
            from commands in Commanded(first.Row.Pair, first.Row.Commands(IO.lift(() => Chosen<TItem>(grid).Map(static node => node.Row))), scope)
            from selection in ControlChange<EventArgs>.Attached(
                Subscriptions.Host<EventArgs>(h => grid.SelectionChanged += h, h => grid.SelectionChanged -= h),
                _ =>
                    from current in latest
                    from keys in IO.lift(() => toHashSet(Chosen<TItem>(grid).Map(node => current.Row.Key(node.Row))))
                    from picked in when(keys != current.Row.Selected, current.Row.Picked(Edits.Commit(keys))).As()
                    select unit,
                scope.Owner)
            from editing in Subscriptions.Host<GridViewCellEventArgs>(h => grid.CellEdited += h, h => grid.CellEdited -= h).Inline(args =>
                from current in latest
                from committed in RowItem<TItem>.Of(args.Item).Traverse(node => current.Row.Edited(Edits.Commit(Replaced(current.Row.Key, current.Row.Branches, current.Row.Items, node.Row)))).As()
                select unit, scope.Owner)
            from expanding in Subscriptions.Merged(
                    Subscriptions.Host<TreeGridViewItemEventArgs>(h => grid.Expanded += h, h => grid.Expanded -= h),
                    Subscriptions.Host<TreeGridViewItemEventArgs>(h => grid.Collapsed += h, h => grid.Collapsed -= h))
                .Inline(_ =>
                    from current in latest
                    from state in list.ValueIO
                    from opened in current.Row.Branches.Traverse(branches => branches.Edited(Edits.Commit(Open(current.Row, state.Nodes)))).As()
                    select unit, scope.Owner)
            let shown =
                from current in latest
                from filled in Fill(grid, list, keep, current.Row)
                from opened in current.Row.Branches.Traverse(branches => Expand(grid, list, current.Row, branches.Expanded)).As()
                from reselected in selection.Detached(Reselected(grid, list, current.Row))
                from scrolled in IO.lift(() => current.Row.Current.Iter(key => Indices(current.Row, list.Value.Nodes.Bind(static node => node.Shown), [key]).Head.Iter(grid.ScrollToRow)))
                select unit
            from gestures in Gestures(grid, latest, emphasis, commands.Keyed, scope)
            from emptied in first.Row.Empty.Traverse(empty => Emptied(grid, list, RowText.Localize(empty, scope.Owner).Local, scope.Owner)).As()
            from paired in Paired(emptied.IfNone(grid), commands.Pair)
            from content in Searched(paired, first.Row.Search, kept => writeIO(keep, kept).Bind(_ => shown), scope)
            select new RowCells(content.Control, shown.Bind(_ => latest).Bind(current => content.Shown(current.Row.Search)).Bind(_ => commands.Refresh)) { Menu = commands.Menu });

    private sealed record ListState<TItem>(Seq<RowItem<TItem>> Nodes, TreeGridItemCollection Store, Seq<(RowItem<TItem> Node, TItem Row)> Reloads) where TItem : notnull;

    private static Seq<RowItem<TItem>> Built<TItem, TKey>(ListRow<TItem, TKey> row, Seq<TItem> items, HashSet<TKey> open, Func<TItem, bool> keep, int depth)
        where TItem : notnull
        where TKey : notnull =>
        row.Ordered.Match(Some: name => Sorted(items, name), None: () => items)
            .Map(item => (Item: item, Kids: Built(row, row.Branches.ToSeq().Bind(branches => branches.Children.Get(item)), open, keep, depth + 1)))
            .Filter(pair => !pair.Kids.IsEmpty || keep(pair.Item))
            .Map(pair => new RowItem<TItem>(pair.Item, depth, pair.Kids) { Expanded = open.Contains(row.Key(pair.Item)) });

    private static Seq<TItem> Sorted<TItem>(Seq<TItem> items, Func<TItem, string> name) {
        TItem[] sorted = [.. items];
        sorted.AsSpan().Sort((left, right) => Localization.LogicalSort(name(left), name(right)));
        return toSeq(sorted);
    }

    private static HashSet<TKey> Open<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> nodes) where TItem : notnull where TKey : notnull =>
        toHashSet(nodes.Bind(static node => node.Preorder).Filter(static node => node.Expanded).Map(node => row.Key(node.Row)));

    private static Seq<RowItem<TItem>> Chosen<TItem>(TreeGridView grid) where TItem : notnull =>
        toSeq(grid.SelectedItems).Choose(RowItem<TItem>.Of);

    private static Seq<int> Indices<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> shown, HashSet<TKey> keys)
        where TItem : notnull
        where TKey : notnull =>
        shown.Choose((index, node) => Conversions.Found(keys.Contains(row.Key(node.Row)), index));

    private static IO<Unit> Reselected<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row)
        where TItem : notnull
        where TKey : notnull =>
        from state in list.ValueIO
        from placed in unless(ReferenceEquals(grid.DataStore, state.Store), IO.lift(fun(() => { grid.DataStore = state.Store; }))).As()
        from held in IO.lift(() => toHashSet(Chosen<TItem>(grid).Map(node => row.Key(node.Row))))
        from selected in when(row.Selected != held, IO.lift(() => { grid.SelectedRows = Indices(row, state.Nodes.Bind(static node => node.Shown), row.Selected); })).As()
        select unit;

    // --- [FILL]
    private static IO<Unit> Fill<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, Atom<Func<TItem, bool>> keep, ListRow<TItem, TKey> row)
        where TItem : notnull
        where TKey : notnull =>
        from kept in keep.ValueIO
        from next in list.SwapIO(state => Filled(row, state, row.Items, kept))
        from reloaded in next.Reloads.TraverseM(reload => IO.lift(fun(() => {
            reload.Node.Row = reload.Row;
            grid.ReloadItem(reload.Node, reloadChildren: false);
        }))).As()
        select unit;

    private static ListState<TItem> Filled<TItem, TKey>(ListRow<TItem, TKey> row, ListState<TItem> state, Seq<TItem> items, Func<TItem, bool> keep)
        where TItem : notnull
        where TKey : notnull =>
        (Before: state.Nodes.Bind(static node => node.Preorder), After: Built(row, items, Open(row, state.Nodes), keep, 0)) switch {
            var built => built.After.Bind(static node => node.Preorder) switch {
                var after when after.Map(node => (row.Key(node.Row), node.Depth)) == built.Before.Map(node => (row.Key(node.Row), node.Depth)) => state with {
                    Reloads = built.Before.Zip(after)
                        .Filter(static pair => !EqDefault<TItem>.Equals(pair.First.Row, pair.Second.Row))
                        .Map(static pair => (pair.First, pair.Second.Row)),
                },
                _ => new ListState<TItem>(built.After, new TreeGridItemCollection(built.After.Map<ITreeGridItem>(static node => node)), []),
            },
        };

    private static IO<Unit> Columns<TItem, TKey>(ListRow<TItem, TKey> row, TreeGridView grid, IPlugInLifetime owner)
        where TItem : notnull
        where TKey : notnull =>
        from size in IconSlot.Small.Extent()
        from added in toSeq(row.Columns).Map(static (column, index) => (Column: column, Index: index)).TraverseM(entry =>
            from cell in CellFor(entry.Column, new Size(size, size), owner)
            from column in use(static () => new GridColumn())
            from placed in IO.lift(() => {
                (column.HeaderText, column.DataCell, column.Editable, column.Resizable, column.Expand) =
                    (Conversions.Unset(entry.Column.Caption.Map(caption => RowText.Localize(caption, owner).Local)), cell, entry.Column.Edits, false,
                     entry.Column.Stretches && toSeq(row.Columns).Skip(entry.Index + 1).ForAll(static later => !later.Stretches));
                grid.Columns.Add(column);
            })
            select unit).As()
        select unit;

    private static IO<Cell> CellFor<TItem>(ListColumn<TItem> column, Size logical, IPlugInLifetime owner) where TItem : notnull =>
        column.Switch((Logical: logical, Owner: owner),
            label: static (held, label) => label.Icon.Match(
                Some: icon =>
                    from cell in use(static () => new ImageTextCell())
                    from bound in IO.lift(() => { (cell.ImageBinding, cell.TextBinding) = (Pictured(icon, held.Logical, held.Owner), Written(label)); })
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
                from bound in IO.lift(() => cell.Binding = Pictured(glyph.Icon, held.Logical, held.Owner))
                select (Cell)cell,
            progress: static (_, progress) =>
                from cell in use(static () => new ProgressCell())
                from bound in IO.lift(() => cell.Binding = Binding.Delegate<RowItem<TItem>, float?>(node => progress.Read(node.Row).ToNullable()))
                select (Cell)cell);

    private static IndirectBinding<Image?> Pictured<TItem>(Func<TItem, IconResource> icon, Size logical, IPlugInLifetime owner) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, Image?>(node => owner.Answer(Icons.Frames(icon(node.Row), logical).Map(static Image? (frames) => frames), failed: null));

    private static IndirectBinding<string> Written<TItem>(ListColumn<TItem>.Label label) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, string>(
            node => label.Read(node.Row),
            (node, value) => _ = label.Rename.Bind(rename => rename(node.Row, value).ToOption()).Iter(item => node.Row = item));

    private static Seq<TItem> Replaced<TItem, TKey>(Func<TItem, TKey> key, Option<ListBranches<TItem, TKey>> branches, Seq<TItem> items, TItem edited)
        where TItem : notnull
        where TKey : notnull =>
        items.Map(item => EqDefault<TKey>.Equals(key(item), key(edited))
            ? edited
            : branches.Fold(item, (held, tree) => tree.Children.Update(kids => Replaced(key, branches, kids, edited), held)));

    // --- [EXPANSION]
    private static IO<Unit> Expand<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, HashSet<TKey> open)
        where TItem : notnull
        where TKey : notnull =>
        list.ValueIO.Bind(state => state.Nodes.Bind(static node => node.Preorder)
            .Filter(node => node.Expanded != open.Contains(row.Key(node.Row)))
            .TraverseM(node => IO.lift(fun(() => {
                node.Expanded = !node.Expanded;
                grid.ReloadItem(node, reloadChildren: true);
            })))
            .As())
            .Map(unit);

    // --- [GESTURES]
    private static IO<Unit> Gestures<TItem, TKey>(TreeGridView grid, IO<ControlRow.Bound<ListRow<TItem, TKey>>> latest, Font emphasis, Func<Keys, IO<bool>> keyed, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        Callbacks.Each(Seq(
            Subscriptions.During<KeyEventArgs>(h => grid.KeyDown += h, h => grid.KeyDown -= h)
                .Inline(args => latest.Bind(current => Pressed(grid, current.Row, keyed, args)), scope.Owner),
            Subscriptions.During<MouseEventArgs>(h => grid.MouseDown += h, h => grid.MouseDown -= h)
                .Inline(args => when(args.Buttons is MouseButtons.Alternate, IO.lift(() => RowItem<TItem>.Of(grid.GetCellAt(args.Location).Item)
                    .Filter(node => !Chosen<TItem>(grid).Exists(held => ReferenceEquals(held, node)))
                    .Iter(node => grid.SelectedItem = node))).As(), scope.Owner),
            Subscriptions.During<GridCellFormatEventArgs>(h => grid.CellFormatting += h, h => grid.CellFormatting -= h)
                .Inline(args => latest.Bind(current => IO.lift(() => RowItem<TItem>.Of(args.Item).Iter(node =>
                    (args.Font, args.ForegroundColor) = (
                        node.Kids.IsEmpty ? args.Font : emphasis,
                        current.Row.Available.ForAll(available => available(node.Row)) ? args.ForegroundColor : SystemColors.DisabledText)))), scope.Owner),
            Subscriptions.Host<TreeGridViewItemEventArgs>(h => grid.Activated += h, h => grid.Activated -= h)
                .Inline(_ => latest.Bind(current => (from activate in current.Row.Activate from node in Chosen<TItem>(grid).Head select activate(node.Row))
                    .Traverse(static activated => activated).As()).Map(unit), scope.Owner),
            HostTheme.In(grid).Inline(_ => IO.lift(grid.ReloadData), scope.Owner))).Map(unit);

    private static IO<Unit> Pressed<TItem, TKey>(TreeGridView grid, ListRow<TItem, TKey> row, Func<Keys, IO<bool>> keyed, KeyEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        from rows in IO.lift(() => toSeq(grid.SelectedRows).Strict())
        from handled in args.KeyData switch {
            Keys.F2 => Handled(args,
                from at in rows.Head
                from renaming in toSeq(row.Columns).Map(static (each, index) => (Column: each, Index: index)).Find(static entry => entry.Column.Renames)
                select (Action)(() => grid.BeginEdit(at, renaming.Index))),
            Keys.Up or Keys.Down when rows.IsEmpty && !row.Items.IsEmpty => Handled(args, Some(() => grid.SelectRow(0))),
            var key => Keyed(keyed, key, args),
        }
        select handled;

    private static IO<Unit> Keyed(Func<Keys, IO<bool>> keyed, Keys key, KeyEventArgs args) =>
        keyed(key).Bind(handled => IO.lift(() => { args.Handled = handled; }));

    private static IO<Unit> Handled(KeyEventArgs args, Option<Action> action) =>
        IO.lift(fun(() => {
            _ = action.Iter(static run => run());
            args.Handled = action.IsSome;
        }));

    // --- [COMMANDS]
    private static IO<(Option<Control> Pair, Seq<Seq<Command<Invoke>>> Menu, Func<Keys, IO<bool>> Keyed, IO<Unit> Refresh)> Commanded(
        Option<(Command<Invoke>.Run Add, Command<Invoke>.Run Remove)> pair, Seq<Command<Invoke>> commands, RowScope scope) =>
        from paired in pair.Traverse(held => ButtonRows.AddRemove(held.Add, held.Remove, scope)).As()
        let menu = pair.Map(static held => Seq<Command<Invoke>>(held.Add, held.Remove)).ToSeq().Add(commands)
        from keyed in CommandRows.Keyed(menu, scope)
        select (paired.Map(static held => (Control)held.Control), menu, keyed, paired.Map(static held => held.Refresh).IfNone(IO.pure(unit)));

    // --- [PLACEMENT]
    private static IO<Control> Emptied<TItem>(TreeGridView grid, Atom<ListState<TItem>> list, string text, IPlugInLifetime owner) where TItem : notnull =>
        from label in use(() => new Label { Text = text, TextColor = SystemColors.DisabledText, TextAlignment = TextAlignment.Center })
        from stack in Containers.Frame(static () => new StackLayout { HorizontalContentAlignment = HorizontalAlignment.Stretch })
        let shown = fun((ListState<TItem> state) => IO.lift(() => (grid.Visible, label.Visible) = (!state.Nodes.IsEmpty, state.Nodes.IsEmpty)).Map(unit))
        from placed in IO.lift(() => {
            stack.Items.Add(new StackLayoutItem(grid, expand: true));
            stack.Items.Add(label);
        })
        from heard in Subscriptions.Changed(list).Inline(shown, owner)
        from current in list.ValueIO.Bind(shown)
        select (Control)stack;

    private static IO<Control> Paired(Control content, Option<Control> pair) =>
        pair.Match(Some: buttons => Stacked(new StackLayoutItem(content, expand: true), new StackLayoutItem(buttons, HorizontalAlignment.Left)), None: () => IO.pure(content));

    private static IO<Control> Stacked(params Seq<StackLayoutItem> items) =>
        Containers.Frame(() => new StackLayout([.. items]) {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Panel),
        }).Map(static stack => (Control)stack);

    private static IO<(Control Control, Func<Option<ListSearch<TItem>>, IO<Unit>> Shown)> Searched<TItem>(
        Control content, Option<ListSearch<TItem>> search, Func<Func<TItem, bool>, IO<Unit>> keep, RowScope scope) where TItem : notnull =>
        search.Match(Some: held =>
            from field in ChoiceRows.Search(RowText.Localize(held.Placeholder, scope.Owner).Local, matches => keep(item => matches(held.Text(item))), scope.Owner)
            from stack in Stacked(new StackLayoutItem(field), new StackLayoutItem(content, expand: true))
            select (stack, (Func<Option<ListSearch<TItem>>, IO<Unit>>)(current => IO.lift(() => { field.Visible = current.Exists(static shown => shown.Shown); }))),
            None: () => IO.pure((content, (Func<Option<ListSearch<TItem>>, IO<Unit>>)(static _ => IO.pure(unit)))));

    // --- [BROWSERS]
    public static ControlRow Browser<TItem, TKey>(string label, BrowserRow<TItem, TKey> row)
        where TItem : notnull
        where TKey : notnull =>
        new ControlRow.Bound<BrowserRow<TItem, TKey>>(label, RowShape.Filled, row, static (latest, scope) =>
            from first in latest
            from tiles in IO.lift(static () => Ref(HashMap<TKey, Image>()))
            from keep in atomIO<Func<TItem, bool>>(static _ => true)
            from owned in use(static () => EnvIO.New(token: CancellationToken.None))
            from loads in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
            from seated in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
            from grid in Containers.Frame(static () => new ControlGridLayout { Wrap = true, GridWrapMode = GridWrapMode.Horizontal, ItemPadding = RhinoLayout.Padding(RhinoLayout.PaddingType.ButtonRow) })
            from scroll in Containers.Frame(static () => new Scrollable { Border = BorderType.None, ExpandContentWidth = true, ExpandContentHeight = false })
            from empty in use(() => new Label { Text = RowText.Localize(first.Row.Empty, scope.Owner).Local, TextColor = SystemColors.DisabledText, TextAlignment = TextAlignment.Center })
            from stack in Containers.Frame(static () => new StackLayout { HorizontalContentAlignment = HorizontalAlignment.Stretch })
            from placed in IO.lift(() => {
                scroll.Content = grid;
                stack.Items.Add(new StackLayoutItem(scroll, expand: true));
                stack.Items.Add(empty);
            })
            from commands in Commanded(None, first.Row.Commands(from current in latest from kept in keep.ValueIO select Listed(current.Row, kept)), scope)
            let shown =
                from current in latest
                from kept in keep.ValueIO
                let listed = Listed(current.Row, kept)
                let size = current.Row.TileSize
                from loaded in Products.Cleared(loads).Bind(_ => Products.Swapped(loads, Loaded(latest, tiles, listed, size, scope))).Under(owned)
                from reseated in Products.Cleared(seated).Bind(_ => Products.Swapped(seated, Seated(grid, latest, tiles, listed, size, commands.Menu, scope))).Under(owned)
                from emptied in IO.lift(() => { (scroll.Visible, empty.Visible, grid.ItemSize) = (!listed.IsEmpty, listed.IsEmpty, size); })
                select unit
            from keys in Subscriptions.During<KeyEventArgs>(h => grid.KeyDown += h, h => grid.KeyDown -= h).Inline(args => CommandRows.Pressed(commands.Keyed, args), scope.Owner)
            from content in Searched(stack, first.Row.Search, kept => writeIO(keep, kept).Bind(_ => shown), scope)
            select new RowCells(content.Control, latest.Bind(current => content.Shown(current.Row.Search)).Bind(_ => shown).Bind(_ => commands.Refresh)) { Menu = commands.Menu });

    private static Seq<TItem> Listed<TItem, TKey>(BrowserRow<TItem, TKey> row, Func<TItem, bool> keep) where TItem : notnull where TKey : notnull =>
        row.Items.Filter(keep);

    private static IO<Unit> Seated<TItem, TKey>(
        ControlGridLayout grid, IO<ControlRow.Bound<BrowserRow<TItem, TKey>>> latest, Ref<HashMap<TKey, Image>> tiles, Seq<TItem> listed, Size size, Seq<Seq<Command<Invoke>>> menu, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        from current in latest
        from held in tiles.ValueIO
        from buttons in listed.TraverseM(item => current.Row.Key(item) switch {
            var key =>
                from button in use(() => new ImageButton {
                    Size = size,
                    ToolTip = current.Row.Label(item),
                    Image = held.Find(key).ValueUnsafe(),
                    BackgroundColor = current.Row.Current == Some(key) ? SystemColors.Highlight : Colors.Transparent,
                })
                from pressed in Subscriptions.Host<EventArgs>(h => button.Click += h, h => button.Click -= h)
                    .Inline(_ => latest.Bind(last => last.Row.Chosen(Edits.Commit(key))), scope.Owner)
                from popup in unless(menu.IsEmpty, CommandRows.Menu(menu, scope).Bind(made => IO.lift(() => { button.ContextMenu = made.Menu; }))).As()
                select (Control)button,
        }).As()
        from released in Products.Released(grid.Items.Clear)
        from added in IO.lift(() => buttons.Iter(button => grid.Items.Add(button)))
        select unit;

    private static IO<Unit> Loaded<TItem, TKey>(IO<ControlRow.Bound<BrowserRow<TItem, TKey>>> latest, Ref<HashMap<TKey, Image>> tiles, Seq<TItem> listed, Size size, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        from current in latest
        let wanted = toHashSet(listed.Map(current.Row.Key))
        from pruned in atomicIO(() => (Before: tiles.Value, After: tiles.Value = tiles.Value.Filter((key, _) => wanted.Contains(key))))
        from disposed in IO.lift(() => pruned.Before.Filter((key, _) => !wanted.Contains(key)).Values.Iter(static image => image.Dispose()))
        from forked in listed.Filter(item => !pruned.After.ContainsKey(current.Row.Key(item))).TraverseM(item =>
            use(((from made in current.Row.Tile(scope, item, size)
                  from image in use(() => made)
                  from cached in atomicIO(() => tiles.Value = tiles.Value.AddOrUpdate(current.Row.Key(item), image))
                  select unit)
                    | @catch(static error => !ErrorOps.Cancellation(error), error => IO.lift(() => PlugInLog.Of(scope.Owner).Failed(error, scope.Owner.GetType(), nameof(Browser)))))
                .Fork(), static fork => fork.Cancel)).As()
        select unit;
}
