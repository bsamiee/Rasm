using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using LanguageExt.ClassInstances;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence.Settings;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.RhinoCommon.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using Rhino.UI;
using Rhino.UI.Controls;
using SkiaSharp;

namespace Rasm.RhinoCommon.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ListColumn<TItem> where TItem : notnull {
    private ListColumn(Option<string> caption) => Caption = caption;

    public Option<string> Caption { get; }

    public bool Renames => Switch(label: static label => label.Rename.IsSome, check: static _ => false, glyph: static _ => false, progress: static _ => false);

    public bool Edits => Switch(label: static label => label.Rename.IsSome, check: static _ => true, glyph: static _ => false, progress: static _ => false);

    public bool Stretches => Map(label: true, check: false, glyph: false, progress: true);

    public sealed record Label(Option<string> Caption, Func<TItem, string> Read, Option<Func<TItem, GlyphSource>> Icon, Option<Func<TItem, string, Fin<TItem>>> Rename)
        : ListColumn<TItem>(Caption);

    public sealed record Check(Option<string> Caption, Lens<TItem, bool> Value) : ListColumn<TItem>(Caption);

    public sealed record Glyph(Option<string> Caption, Func<TItem, GlyphSource> Icon) : ListColumn<TItem>(Caption);

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
    public Option<(Command<RowRule, Invoke>.Run Add, Command<RowRule, Invoke>.Run Remove)> Pair { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<RowRule, Invoke>>> Commands { get; init; } = static _ => [];
    public bool AllowMultipleSelection { get; init; }
}

public sealed record TileMark<TItem>(string Caption, GlyphSource On, GlyphSource Off, Lens<TItem, bool> Value) where TItem : notnull;

public sealed record BrowserRow<TItem>(Func<TItem, Key> Key, Func<TItem, string> Label, Func<RowScope, TItem, SKSizeI, IO<SKImage>> Tile, Seq<TItem> Items, string Empty)
    where TItem : notnull {
    public Func<Edit<Seq<TItem>>, IO<Unit>> Edited { get; init; } = static _ => IO.pure(unit);
    public Option<Key> Current { get; init; }
    public Func<Edit<Key>, IO<Unit>> Chosen { get; init; } = static _ => IO.pure(unit);
    public Factor Aspect { get; init; } = Factor.Unit;
    public Option<float> TileSize { get; init; }
    public Option<Func<Edit<float>, IO<Unit>>> Resized { get; init; }
    public bool Numbered { get; init; }
    public bool Reorders { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Seq<TileMark<TItem>> Marks { get; init; }
    public Option<Func<TItem, Option<string>>> File { get; init; }
    public Option<Func<RowScope, TItem, IO<Unit>>> Preview { get; init; }
    public Seq<HostEvent<Unit>> Reloads { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<RowRule, Invoke>>> Commands { get; init; } = static _ => [];
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
                    from picked in when(keys != current.Row.Selected, current.Row.Picked(Edit.Commit(keys))).As()
                    select unit,
                scope.Owner)
            from editing in Subscriptions.Host<GridViewCellEventArgs>(h => grid.CellEdited += h, h => grid.CellEdited -= h).Inline(args =>
                from current in latest
                from committed in RowItem<TItem>.Of(args.Item).Traverse(node => current.Row.Edited(Edit.Commit(Replaced(current.Row.Key, current.Row.Branches, current.Row.Items, node.Row)))).As()
                select unit, scope.Owner)
            from expanding in Subscriptions.Merged(
                    Subscriptions.Host<TreeGridViewItemEventArgs>(h => grid.Expanded += h, h => grid.Expanded -= h),
                    Subscriptions.Host<TreeGridViewItemEventArgs>(h => grid.Collapsed += h, h => grid.Collapsed -= h))
                .Inline(_ =>
                    from current in latest
                    from state in list.ValueIO
                    from opened in current.Row.Branches.Traverse(branches => branches.Edited(Edit.Commit(Open(current.Row, state.Nodes)))).As()
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
        from pixels in IconSlot.Small.Pixels
        from added in toSeq(row.Columns).Map(static (column, index) => (Column: column, Index: index)).TraverseM(entry =>
            from cell in CellFor(entry.Column, new Size(size, size), (float)pixels.Width / size)
            from column in use(static () => new GridColumn())
            from placed in IO.lift(() => {
                (column.HeaderText, column.DataCell, column.Editable, column.Resizable, column.Expand) =
                    (Conversions.Unset(entry.Column.Caption.Map(caption => RowText.Localize(caption, owner).Local)), cell, entry.Column.Edits, false,
                     entry.Column.Stretches && toSeq(row.Columns).Skip(entry.Index + 1).ForAll(static later => !later.Stretches));
                grid.Columns.Add(column);
            })
            select unit).As()
        select unit;

    private static IO<Cell> CellFor<TItem>(ListColumn<TItem> column, Size logical, float scale) where TItem : notnull =>
        column.Switch((Logical: logical, Scale: scale),
            label: static (held, label) => label.Icon.Match(
                Some: icon =>
                    from cell in use(static () => new ImageTextCell())
                    from bound in IO.lift(() => { (cell.ImageBinding, cell.TextBinding) = (Pictured(icon, held.Logical, held.Scale), Written(label)); })
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
                from bound in IO.lift(() => cell.Binding = Pictured(glyph.Icon, held.Logical, held.Scale))
                select (Cell)cell,
            progress: static (_, progress) =>
                from cell in use(static () => new ProgressCell())
                from bound in IO.lift(() => cell.Binding = Binding.Delegate<RowItem<TItem>, float?>(node => progress.Read(node.Row).ToNullable()))
                select (Cell)cell);

    private static IndirectBinding<Image> Pictured<TItem>(Func<TItem, GlyphSource> icon, Size logical, float scale) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, Image>(node => Icons.Frame(icon(node.Row), logical, scale));

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
    private static IO<(Option<Control> Pair, Seq<Seq<Command<RowRule, Invoke>>> Menu, Func<Keys, IO<bool>> Keyed, IO<Unit> Refresh)> Commanded(
        Option<(Command<RowRule, Invoke>.Run Add, Command<RowRule, Invoke>.Run Remove)> pair, Seq<Command<RowRule, Invoke>> commands, RowScope scope) =>
        from paired in pair.Traverse(held => ButtonRows.AddRemove(held.Add, held.Remove, scope)).As()
        let menu = pair.Map(static held => Seq<Command<RowRule, Invoke>>(held.Add, held.Remove)).ToSeq().Add(commands)
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
    public static ControlRow Browser<TItem>(string label, BrowserRow<TItem> row) where TItem : notnull =>
        new ControlRow.Bound<BrowserRow<TItem>>(label, RowShape.Filled, row, static (latest, scope) =>
            from first in latest
            from held in atomIO(new Browsing<TItem>(TileCache<TItem>.Empty))
            from owned in use(static () => EnvIO.New(token: CancellationToken.None))
            from loads in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
            from dwell in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
            from loading in Products.Swapped(loads, unitIO).Under(owned)
            from view in use(() => new SceneView(scope, new Node.Box()))
            from commands in Commanded(None, first.Row.Commands(from current in latest from state in held.ValueIO select Picked(current.Row, state)), scope)
            from host in CommandRows.Host(scope.Hosted(view))
            let browser = new Realized<TItem>(latest, scope, view, held, owned, loads, dwell, host, commands.Menu)
            from keys in Subscriptions.During<KeyEventArgs>(h => view.KeyDown += h, h => view.KeyDown -= h).Inline(args => Keyed(commands.Keyed, args.KeyData, args), scope.Owner)
            from placed in Subscriptions.Merged(
                    Subscriptions.Host<EventArgs>(h => view.SizeChanged += h, h => view.SizeChanged -= h),
                    Subscriptions.Host<EventArgs>(h => view.LoadComplete += h, h => view.LoadComplete -= h))
                .Inline(_ => Placed(browser), scope.Owner)
            from post in Mailboxes.Idle<Unit, Unit>(_ =>
                Products.Cleared(loads).Bind(_ => Products.Swapped(loads, unitIO)).Under(owned)
                    .Bind(_ => Advanced(browser, static state => state with { Tiles = state.Tiles.Reloaded })))(scope.Owner)
            from reloads in Callbacks.Each(first.Row.Reloads.Map(reload => reload.Inline(_ => post(unit, unit), scope.Owner)))
            from content in Searched(view, first.Row.Search, keep => Advanced(browser, state => state with { Keep = keep }), scope)
            select new RowCells(content.Control, latest.Bind(current => content.Shown(current.Row.Search)).Bind(_ => Placed(browser)).Bind(_ => commands.Refresh)) { Menu = commands.Menu });

    private sealed record Browsing<TItem>(TileCache<TItem> Tiles) where TItem : notnull {
        public Func<TItem, bool> Keep { get; init; } = static _ => true;
        public Option<float> Resizing { get; init; }
        public Selection Picked { get; init; } = new Selection.Many([]);
        public Option<Key> Hovered { get; init; }
        public HashSet<Key> Viewed { get; init; }
        public Option<float> Scale { get; init; }
    }

    private sealed record Realized<TItem>(
        IO<ControlRow.Bound<BrowserRow<TItem>>> Latest, RowScope Scope, SceneView View, Atom<Browsing<TItem>> State, EnvIO Owned,
        Ref<Option<Lease<Unit>>> Loads, Ref<Option<Lease<Unit>>> Dwell, CommandHost<RowRule, Invoke> Host, Seq<Seq<Command<RowRule, Invoke>>> Menu)
        where TItem : notnull;

    private static IO<Unit> Released<TItem>(Realized<TItem> browser, Seq<SKImage> images) where TItem : notnull =>
        images.TraverseM(static image => release(image)).As().Under(browser.Owned).Map(unit);

    // --- [GALLERY]
    private static Gallery<TItem> Laid<TItem>(Realized<TItem> browser, ControlRow.Bound<BrowserRow<TItem>> current, Browsing<TItem> state) where TItem : notnull =>
        new(current.Row.Items.Filter(state.Keep), current.Row.Key, current.Row.Label, state.Tiles, keys => new Part.Choice(keys, state.Picked, change => Selecting(browser, change)) {
            Reorder = change => current.Row.Reorders ? Some(current.Row.Edited(change.Map(order => Rearranged(current.Row.Key, current.Row.Items, order)).As())) : None,
            Exported = moved => current.Row.File.Map(file => DataTransfer.Drag(browser.View, Listed(current.Row, toHashSet(moved)).Choose(file), None)),
            Menu = (targeted, frame) => Some(Menued(browser, targeted, frame)),
            Hover = hovered => Some(Hovered(browser, hovered)),
            Viewed = seen => Some(Advanced(browser, held => held with { Viewed = seen })),
        }) {
            TileSize = state.Resizing | current.Row.TileSize,
            Aspect = current.Row.Aspect,
            Realized = state.Viewed,
            Current = current.Row.Current,
            Numbered = current.Row.Numbered,
            Overlay = item => current.Row.Marks.Map(mark => mark.Value.Get(item) switch {
                var on => new Part.Press(_ => current.Row.Edited(Edit.Commit(Replaced(current.Row.Key, current.Row.Items, mark.Value.Set(!on, item))))).Drawn(new Node.Glyph(on ? mark.On : mark.Off) {
                    Accessible = AccessRole.CheckBox.Marked(new Accessible { Role = AccessRole.CheckBox, Label = RowText.Localize(mark.Caption, browser.Scope.Owner).Local }, on ? Check.On : Check.Off),
                }),
            }),
            Resized = current.Row.Resized.Map(resized => (RowText.Localize("Tile size", browser.Scope.Owner).Local, (Func<Edit<float>, IO<Unit>>)(change => Resized(browser, resized, change)))),
            Empty = Some(RowText.Localize(current.Row.Empty, browser.Scope.Owner).Local),
        };

    private static IO<Unit> Advanced<TItem>(Realized<TItem> browser, Func<Browsing<TItem>, Browsing<TItem>> next) where TItem : notnull =>
        from current in browser.Latest
        from moved in browser.State.SwapIO(state => Normalized(current.Row, next(state)))
        let pruned = Laid(browser, current, moved).Pruned
        from kept in browser.State.SwapIO(state => state with { Tiles = pruned.Tiles })
        from released in Released(browser, pruned.Released)
        from shown in browser.View.Show(Laid(browser, current, kept).Drawn with { Conditions = current.Enabled ? [] : [Condition.Disabled] })
        from requested in Requested(browser)
        select unit;

    private static Browsing<TItem> Normalized<TItem>(BrowserRow<TItem> row, Browsing<TItem> state) where TItem : notnull =>
        toHashSet(row.Items.Filter(state.Keep).Map(row.Key)) switch {
            var shown => state with {
                Picked = state.Picked.Switch(
                    shown,
                    one: static (keys, one) => (Selection)new Selection.One(one.Picked.Filter(keys.Contains)),
                    many: static (keys, many) => new Selection.Many(many.Picked.Intersect(keys)) { Anchor = many.Anchor.Filter(keys.Contains), Focus = many.Focus.Filter(keys.Contains) }),
                Hovered = state.Hovered.Filter(shown.Contains),
                Viewed = state.Viewed.Intersect(shown),
            },
        };

    private static IO<Unit> Placed<TItem>(Realized<TItem> browser) where TItem : notnull =>
        IO.lift(() => Optional(browser.View.ParentWindow).Map(static window => window.LogicalPixelSize)).Bind(scale => Advanced(browser, state => state with { Scale = scale }));

    private static Seq<TItem> Listed<TItem>(BrowserRow<TItem> row, HashSet<Key> keys) where TItem : notnull =>
        row.Items.Filter(item => keys.Contains(row.Key(item)));

    private static Seq<TItem> Picked<TItem>(BrowserRow<TItem> row, Browsing<TItem> state) where TItem : notnull =>
        row.Items.Filter(state.Keep).Filter(item => state.Picked.Holds(row.Key(item)));

    private static Seq<TItem> Replaced<TItem>(Func<TItem, Key> key, Seq<TItem> items, TItem edited) where TItem : notnull =>
        items.Map(item => key(item) == key(edited) ? edited : item);

    // --- [TILES]
    private static IO<Unit> Requested<TItem>(Realized<TItem> browser) where TItem : notnull =>
        from current in browser.Latest
        from state in browser.State.ValueIO
        from loaded in state.Scale.Map(scale => Laid(browser, current, state).Requested(scale)).Filter(static asked => !asked.Wanted.IsEmpty).Traverse(asked =>
            from marked in browser.State.SwapIO(held => held with { Tiles = asked.Tiles })
            from forked in asked.Wanted.TraverseM(ask => browser.Loads.Value.Traverse(lease =>
                use(Reported(browser, Loaded(browser, current.Row, ask.Key, ask.Tile)).Fork(), static fork => fork.Cancel).Under(lease.Scope)).As()).As()
            select unit).As()
        select unit;

    private static IO<Unit> Loaded<TItem>(Realized<TItem> browser, BrowserRow<TItem> row, Key key, Tile<TItem> tile) where TItem : notnull =>
        (row.Tile(browser.Scope, tile.Item, tile.Pixels).Bind(image => use(() => image).Under(browser.Owned)).Map(static image => Fin.Succ(image))
         | @catch(static error => !ErrorOps.Cancellation(error), static error => IO.pure(Fin.Fail<SKImage>(error))))
            .Bind(arrival => (
                from state in browser.State.ValueIO
                let arrived = state.Tiles.Arrived(key, tile, arrival)
                from stored in browser.State.SwapIO(held => held with { Tiles = arrived.Cache })
                from released in Released(browser, arrived.Released)
                from shown in Advanced(browser, static held => held)
                select unit).OnUiThread());

    private static IO<Unit> Reported<TItem>(Realized<TItem> browser, IO<Unit> work, [CallerMemberName] string member = "") where TItem : notnull =>
        work | @catch(static error => !ErrorOps.Cancellation(error), error => IO.lift(() => PlugInLog.Of(browser.Scope.Owner).Failed(error, browser.Scope.Owner.GetType(), member)));

    // --- [CHOICE]
    private static IO<Unit> Selecting<TItem>(Realized<TItem> browser, Edit<Selection> change) where TItem : notnull =>
        from moved in change.Value.Traverse(picked => Advanced(browser, state => state with { Picked = picked })).As()
        from current in browser.Latest
        from chosen in (from picked in change.Applied from lead in picked.Lead where picked.Holds(lead) select lead).Filter(lead => current.Row.Current != Some(lead))
            .Traverse(lead => current.Row.Chosen(change.Map(_ => lead).As())).As()
        select unit;

    private static Seq<TItem> Rearranged<TItem>(Func<TItem, Key> key, Seq<TItem> latest, Seq<Key> order) where TItem : notnull =>
        (Moved: toHashSet(order), Items: toHashMap(latest.Map(item => (key(item), item)))) switch {
            var held => latest.Fold((Placed: Seq<TItem>(), Next: order), (placed, item) => held.Moved.Contains(key(item))
                    ? placed.Next.Head.Bind(held.Items.Find).Match(Some: next => (placed.Placed.Add(next), placed.Next.Tail), None: () => (placed.Placed.Add(item), placed.Next))
                    : (placed.Placed.Add(item), placed.Next))
                .Placed,
        };

    private static IO<Unit> Menued<TItem>(Realized<TItem> browser, Seq<Key> targeted, SKRect frame) where TItem : notnull =>
        from moved in Advanced(browser, state => state with { Picked = new Selection.Many(toHashSet(targeted)) { Anchor = targeted.Head, Focus = targeted.Head } })
        from shown in browser.Host.Menu(browser.Menu, frame)
        select unit;

    // --- [HOVER]
    private static IO<Unit> Hovered<TItem>(Realized<TItem> browser, Option<Key> key) where TItem : notnull =>
        from moved in Advanced(browser, state => state with { Hovered = key })
        from current in browser.Latest
        from dwelt in Products.Cleared(browser.Dwell).Bind(_ => (from preview in current.Row.Preview from held in key select (Preview: preview, Key: held)).Traverse(hover =>
            Products.Swapped(browser.Dwell,
                from shown in use(static () => EnvIO.New(token: CancellationToken.None))
                from forked in use(Reported(browser, Dwelled(browser, hover.Key, hover.Preview, shown)).Fork(), static fork => fork.Cancel)
                select unit)).As()).Under(browser.Owned)
        select unit;

    private static IO<Unit> Dwelled<TItem>(Realized<TItem> browser, Key key, Func<RowScope, TItem, IO<Unit>> preview, EnvIO shown) where TItem : notnull =>
        from delay in Accessors.StepEventsDelay
        from waited in IO.yieldFor(TimeSpan.FromSeconds(delay))
        from previewed in (from current in browser.Latest
                           from state in browser.State.ValueIO
                           from visible in browser.Scope.Visibility
                           from opened in (from item in Listed(current.Row, [key]).Head where visible && current.Enabled && state.Hovered == Some(key) select item)
                               .Traverse(item => preview(browser.Scope, item).Under(shown)).As()
                           select unit).OnUiThread()
        select unit;

    // --- [SIZE]
    private static IO<Unit> Resized<TItem>(Realized<TItem> browser, Func<Edit<float>, IO<Unit>> resized, Edit<float> change) where TItem : notnull =>
        from moved in Advanced(browser, state => state with { Resizing = change.Value.Filter(_ => change.Phase == EditPhase.Preview) })
        from applied in change.Applied.Traverse(_ => resized(change)).As()
        select unit;
}
