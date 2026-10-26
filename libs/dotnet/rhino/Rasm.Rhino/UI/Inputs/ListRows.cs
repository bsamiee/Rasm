using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using Rhino.UI;
using Rhino.UI.Controls;
using SkiaSharp;

namespace Rasm.Rhino.UI.Inputs;

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

public sealed record ListBranches<TItem, TKey>(Lens<TItem, Seq<TItem>> Children, LanguageExt.HashSet<TKey> Expanded, Func<Edit<LanguageExt.HashSet<TKey>>, IO<Unit>> Edited)
    where TItem : notnull
    where TKey : notnull;

public sealed record ListRow<TItem, TKey>(IterableNE<ListColumn<TItem>> Columns, Func<TItem, TKey> Key, Seq<TItem> Items, LanguageExt.HashSet<TKey> Selected)
    where TItem : notnull
    where TKey : notnull {
    public Func<Edit<Seq<TItem>>, IO<Unit>> Edited { get; init; } = static _ => IO.pure(unit);
    public Func<Edit<LanguageExt.HashSet<TKey>>, IO<Unit>> Picked { get; init; } = static _ => IO.pure(unit);
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
    public Option<Func<RowScope, TItem, Action<IDisposable>, IO<Unit>>> Preview { get; init; }
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
        new ControlRow.Bound<ListRow<TItem, TKey>>(label, RowShape.Filled, row, static (latest, scope, retain) =>
            from first in latest
            from grid in DisposalOps.Acquire(IO.lift(static () => new TreeGridView()), retain)
            from faced in IO.lift(() => {
                (grid.ShowHeader, grid.GridLines, grid.Border, grid.AllowMultipleSelection, grid.AllowEmptySelection) =
                    (toSeq(first.Row.Columns).Exists(static column => column.Caption.IsSome), GridLines.None, BorderType.None, first.Row.AllowMultipleSelection, true);
            })
            from emphasis in IO.lift(static () => SystemFonts.Bold(SystemFonts.Label().Size))
            from list in IO.lift(static () => Atom(new ListState<TItem>(Seq<RowItem<TItem>>(), [], Seq<(RowItem<TItem> Node, TItem Row)>())))
            from keep in atomIO<Func<TItem, bool>>(static _ => true)
            from columns in Columns(first.Row, grid, scope.Sink, retain)
            from commands in Commanded(first.Row.Pair, first.Row.Commands(IO.lift(() => Chosen<TItem>(grid).Map(static node => node.Row))), scope, retain)
            let selected = Callbacks.Handler<EventArgs>(_ =>
                from current in latest
                from keys in IO.lift(() => toHashSet(Chosen<TItem>(grid).Map(node => current.Row.Key(node.Row))))
                from picked in when(keys != current.Row.Selected, current.Row.Picked(Edit.Commit(keys))).As()
                select unit, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectionChanged)))
            from selecting in Subscriptions.Attach(h => grid.SelectionChanged += h, h => grid.SelectionChanged -= h, selected, retain)
            from editing in Subscriptions.Attach(h => grid.CellEdited += h, h => grid.CellEdited -= h,
                Callbacks.Handler<GridViewCellEventArgs>(args =>
                    from current in latest
                    from committed in RowItem<TItem>.Of(args.Item).Traverse(node => current.Row.Edited(Edit.Commit(Replaced(current.Row.Key, current.Row.Branches, current.Row.Items, node.Row)))).As()
                    select unit, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellEdited))), retain)
            let expanded = Callbacks.Handler<TreeGridViewItemEventArgs>(_ =>
                from current in latest
                from state in list.ValueIO
                from opened in current.Row.Branches.Traverse(branches => branches.Edited(Edit.Commit(Open(current.Row, state.Nodes)))).As()
                select unit, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Expanded)))
            from expanding in Subscriptions.Attach(h => grid.Expanded += h, h => grid.Expanded -= h, expanded, retain)
            from collapsing in Subscriptions.Attach(h => grid.Collapsed += h, h => grid.Collapsed -= h, expanded, retain)
            let shown =
                from current in latest
                from filled in Fill(grid, list, keep, current.Row)
                from opened in current.Row.Branches.Traverse(branches => Expand(grid, list, current.Row, branches.Expanded)).As()
                from reselected in Subscriptions.Detached(h => grid.SelectionChanged += h, h => grid.SelectionChanged -= h, selected, Reselected(grid, list, current.Row))
                from scrolled in IO.lift(() => current.Row.Current.Iter(key => Indices(current.Row, list.Value.Nodes.Bind(static node => node.Shown), [key]).Head.Iter(grid.ScrollToRow)))
                select unit
            from gestures in Gestures(grid, latest, emphasis, commands.Keyed, scope, retain)
            from emptied in first.Row.Empty.Traverse(empty => Emptied(grid, list, RowText.Localize(empty, scope.Sink).Local, scope.Sink, retain)).As()
            from paired in Paired(emptied.IfNone(grid), commands.Pair, retain)
            from content in Searched(paired, first.Row.Search, kept => writeIO(keep, kept).Bind(_ => shown), scope, retain)
            select new RowCells(content.Control, shown.Bind(_ => latest).Bind(current => content.Shown(current.Row.Search)).Bind(_ => commands.Refresh)) { Menu = commands.Menu });

    private sealed record ListState<TItem>(Seq<RowItem<TItem>> Nodes, TreeGridItemCollection Store, Seq<(RowItem<TItem> Node, TItem Row)> Reloads) where TItem : notnull;

    private static Seq<RowItem<TItem>> Built<TItem, TKey>(ListRow<TItem, TKey> row, Seq<TItem> items, LanguageExt.HashSet<TKey> open, Func<TItem, bool> keep, int depth)
        where TItem : notnull
        where TKey : notnull =>
        row.Ordered.Match(Some: name => toSeq(items.OrderBy(name, Comparer<string>.Create(Localization.LogicalSort))), None: () => items)
            .Map(item => (Item: item, Kids: Built(row, row.Branches.ToSeq().Bind(branches => branches.Children.Get(item)), open, keep, depth + 1)))
            .Filter(pair => !pair.Kids.IsEmpty || keep(pair.Item))
            .Map(pair => new RowItem<TItem>(pair.Item, depth, pair.Kids) { Expanded = open.Contains(row.Key(pair.Item)) });

    private static LanguageExt.HashSet<TKey> Open<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> nodes) where TItem : notnull where TKey : notnull =>
        toHashSet(nodes.Bind(static node => node.Preorder).Filter(static node => node.Expanded).Map(node => row.Key(node.Row)));

    private static Seq<RowItem<TItem>> Chosen<TItem>(TreeGridView grid) where TItem : notnull =>
        toSeq(grid.SelectedItems).Choose(RowItem<TItem>.Of);

    private static Seq<int> Indices<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> shown, LanguageExt.HashSet<TKey> keys)
        where TItem : notnull
        where TKey : notnull =>
        shown.Choose((index, node) => Callbacks.Found(keys.Contains(row.Key(node.Row)), index));

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
                        .Filter(static pair => !EqualityComparer<TItem>.Default.Equals(pair.First.Row, pair.Second.Row))
                        .Map(static pair => (pair.First, pair.Second.Row)),
                },
                _ => new ListState<TItem>(built.After, new TreeGridItemCollection(built.After.Map<ITreeGridItem>(static node => node)), []),
            },
        };

    private static IO<Unit> Columns<TItem, TKey>(ListRow<TItem, TKey> row, TreeGridView grid, IPlugInSink sink, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        from size in IconSlot.Small.Extent()
        from pixels in IconSlot.Small.Pixels
        from added in toSeq(row.Columns).Map(static (column, index) => (Column: column, Index: index)).TraverseM(entry =>
            from cell in CellFor(entry.Column, new Size(size, size), (float)pixels.Width / size, retain)
            from column in DisposalOps.Acquire(IO.lift(static () => new GridColumn()), retain)
            from placed in IO.lift(() => {
                (column.HeaderText, column.DataCell, column.Editable, column.Resizable, column.Expand) =
                    (Conversions.Unset(entry.Column.Caption.Map(caption => RowText.Localize(caption, sink).Local)), cell, entry.Column.Edits, false,
                     entry.Column.Stretches && toSeq(row.Columns).Skip(entry.Index + 1).ForAll(static later => !later.Stretches));
                grid.Columns.Add(column);
            })
            select unit).As()
        select unit;

    private static IO<Cell> CellFor<TItem>(ListColumn<TItem> column, Size logical, float scale, Action<IDisposable> retain) where TItem : notnull =>
        column.Switch((Retain: retain, Logical: logical, Scale: scale),
            label: static (held, label) => label.Icon.Match(
                Some: icon =>
                    from cell in DisposalOps.Acquire(IO.lift(static () => new ImageTextCell()), held.Retain)
                    from bound in IO.lift(() => { (cell.ImageBinding, cell.TextBinding) = (Pictured(icon, held.Logical, held.Scale), Written(label)); })
                    select (Cell)cell,
                None: () =>
                    from cell in DisposalOps.Acquire(IO.lift(static () => new TextBoxCell()), held.Retain)
                    from bound in IO.lift(() => cell.Binding = Written(label))
                    select (Cell)cell),
            check: static (held, check) =>
                from cell in DisposalOps.Acquire(IO.lift(static () => new CheckBoxCell()), held.Retain)
                from bound in IO.lift(() => cell.Binding = Binding.Delegate<RowItem<TItem>, bool?>(node => check.Value.Get(node.Row), (node, value) => node.Row = check.Value.Set(value is true, node.Row)))
                select (Cell)cell,
            glyph: static (held, glyph) =>
                from cell in DisposalOps.Acquire(IO.lift(static () => new ImageViewCell()), held.Retain)
                from bound in IO.lift(() => cell.Binding = Pictured(glyph.Icon, held.Logical, held.Scale))
                select (Cell)cell,
            progress: static (held, progress) =>
                from cell in DisposalOps.Acquire(IO.lift(static () => new ProgressCell()), held.Retain)
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
        items.Map(item => EqualityComparer<TKey>.Default.Equals(key(item), key(edited))
            ? edited
            : branches.Fold(item, (held, tree) => tree.Children.Update(kids => Replaced(key, branches, kids, edited), held)));

    // --- [EXPANSION]
    private static IO<Unit> Expand<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, LanguageExt.HashSet<TKey> open)
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
    private static IO<Unit> Gestures<TItem, TKey>(
        TreeGridView grid, IO<ControlRow.Bound<ListRow<TItem, TKey>>> latest, Font emphasis, Func<Keys, IO<bool>> keyed, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        Callbacks.Each(Seq(
            Subscriptions.Attach(h => grid.KeyDown += h, h => grid.KeyDown -= h,
                Callbacks.Synchronous<KeyEventArgs>(args => latest.Bind(current => Pressed(grid, current.Row, keyed, args)), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.KeyDown))), retain),
            Subscriptions.Attach(h => grid.MouseDown += h, h => grid.MouseDown -= h,
                Callbacks.Synchronous<MouseEventArgs>(args => when(args.Buttons is MouseButtons.Alternate, IO.lift(() => RowItem<TItem>.Of(grid.GetCellAt(args.Location).Item)
                    .Filter(node => !Chosen<TItem>(grid).Exists(held => ReferenceEquals(held, node)))
                    .Iter(node => grid.SelectedItem = node))).As(), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.MouseDown))), retain),
            Subscriptions.Attach(h => grid.CellFormatting += h, h => grid.CellFormatting -= h,
                Callbacks.Synchronous<GridCellFormatEventArgs>(args => latest.Bind(current => IO.lift(() => RowItem<TItem>.Of(args.Item).Iter(node =>
                    (args.Font, args.ForegroundColor) = (
                        node.Kids.IsEmpty ? args.Font : emphasis,
                        current.Row.Available.ForAll(available => available(node.Row)) ? args.ForegroundColor : SystemColors.DisabledText)))),
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellFormatting))), retain),
            Subscriptions.Attach(h => grid.Activated += h, h => grid.Activated -= h,
                Callbacks.Handler<TreeGridViewItemEventArgs>(_ => latest.Bind(current => (from activate in current.Row.Activate from node in Chosen<TItem>(grid).Head select activate(node.Row))
                    .Traverse(static activated => activated).As()).Map(unit),
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Activated))), retain),
            HostTheme.In(grid).Inline(_ => IO.lift(grid.ReloadData), scope.Sink, retain))).Map(unit);

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
        Option<(Command<RowRule, Invoke>.Run Add, Command<RowRule, Invoke>.Run Remove)> pair, Seq<Command<RowRule, Invoke>> commands, RowScope scope, Action<IDisposable> retain) =>
        from paired in pair.Traverse(held => ButtonRows.AddRemove(held.Add, held.Remove, scope, retain)).As()
        let menu = pair.Map(static held => Seq<Command<RowRule, Invoke>>(held.Add, held.Remove)).ToSeq().Add(commands)
        from keyed in CommandRows.Keyed(menu, scope, retain)
        select (paired.Map(static held => (Control)held.Control), menu, keyed, paired.Map(static held => held.Refresh).IfNone(IO.pure(unit)));

    // --- [PLACEMENT]
    private static IO<Control> Emptied<TItem>(TreeGridView grid, Atom<ListState<TItem>> list, string text, IPlugInSink sink, Action<IDisposable> retain) where TItem : notnull =>
        from label in DisposalOps.Acquire(IO.lift(() => new Label { Text = text, TextColor = SystemColors.DisabledText, TextAlignment = TextAlignment.Center }), retain)
        from stack in Containers.Frame(static () => new StackLayout { HorizontalContentAlignment = HorizontalAlignment.Stretch }, retain)
        let shown = fun((ListState<TItem> state) => IO.lift(() => (grid.Visible, label.Visible) = (!state.Nodes.IsEmpty, state.Nodes.IsEmpty)).Map(unit))
        from placed in IO.lift(() => {
            stack.Items.Add(new StackLayoutItem(grid, expand: true));
            stack.Items.Add(label);
        })
        from heard in Subscriptions.Changed(list, nameof(list.Change)).Inline(shown, sink, retain)
        from current in list.ValueIO.Bind(shown)
        select (Control)stack;

    private static IO<Control> Paired(Control content, Option<Control> pair, Action<IDisposable> retain) =>
        pair.Match(Some: buttons => Stacked(retain, new StackLayoutItem(content, expand: true), new StackLayoutItem(buttons, HorizontalAlignment.Left)), None: () => IO.pure(content));

    private static IO<Control> Stacked(Action<IDisposable> retain, params Seq<StackLayoutItem> items) =>
        Containers.Frame(() => new StackLayout([.. items]) {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Panel),
        }, retain).Map(static stack => (Control)stack);

    private static IO<(Control Control, Func<Option<ListSearch<TItem>>, IO<Unit>> Shown)> Searched<TItem>(
        Control content, Option<ListSearch<TItem>> search, Func<Func<TItem, bool>, IO<Unit>> keep, RowScope scope, Action<IDisposable> retain) where TItem : notnull =>
        search.Match(Some: held =>
            from field in ChoiceRows.Search(RowText.Localize(held.Placeholder, scope.Sink).Local, matches => keep(item => matches(held.Text(item))), scope.Sink, retain)
            from stack in Stacked(retain, new StackLayoutItem(field), new StackLayoutItem(content, expand: true))
            select (stack, (Func<Option<ListSearch<TItem>>, IO<Unit>>)(current => IO.lift(() => { field.Visible = current.Exists(static shown => shown.Shown); }))),
            None: () => IO.pure((content, (Func<Option<ListSearch<TItem>>, IO<Unit>>)(static _ => IO.pure(unit)))));

    // --- [BROWSERS]
    public static ControlRow Browser<TItem>(string label, BrowserRow<TItem> row) where TItem : notnull =>
        new ControlRow.Bound<BrowserRow<TItem>>(label, RowShape.Filled, row, static (latest, scope, retain) =>
            from first in latest
            from held in atomIO(new Browsing<TItem>(TileCache<TItem>.Empty))
            from loads in Bin(retain)
            from dwell in Bin(retain)
            let site = new CallbackSite(scope.Sink, typeof(SceneView), nameof(Browser))
            from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
            from released in DisposalOps.Acquire(IO.pure(unit), _ => held.ValueIO.Bind(static state => DisposalOps.Release(state.Tiles.Kept([]).Released)), retain)
            from commands in Commanded(None, first.Row.Commands(from current in latest from state in held.ValueIO select Picked(current.Row, state)), scope, retain)
            from host in CommandRows.Host(scope.Hosted(view), retain)
            let browser = new Realized<TItem>(latest, scope, view, held, loads, dwell, host, commands.Menu)
            from keys in Subscriptions.Attach(h => view.KeyDown += h, h => view.KeyDown -= h,
                Callbacks.Synchronous<KeyEventArgs>(args => Keyed(commands.Keyed, args.KeyData, args), site with { Member = nameof(Control.KeyDown) }), retain)
            from sized in Subscriptions.Attach(h => view.SizeChanged += h, h => view.SizeChanged -= h, Callbacks.Handler<EventArgs>(_ => Placed(browser), site with { Member = nameof(Control.SizeChanged) }), retain)
            from loaded in Subscriptions.Attach(h => view.LoadComplete += h, h => view.LoadComplete -= h,
                Callbacks.Handler<EventArgs>(_ => Placed(browser), site with { Member = nameof(Control.LoadComplete) }), retain)
            from reloads in Callbacks.Each(first.Row.Reloads.Map(reload => reload.Choose(static _ => Some((unit, unit)))
                .Through(Subscriptions.Idle<Unit, Unit>(_ => loads.Drain.Bind(_ => Advanced(browser, static state => state with { Tiles = state.Tiles.Reloaded }))), scope.Sink, retain)))
            from content in Searched(view, first.Row.Search, keep => Advanced(browser, state => state with { Keep = keep }), scope, retain)
            select new RowCells(content.Control, latest.Bind(current => content.Shown(current.Row.Search)).Bind(_ => Placed(browser)).Bind(_ => commands.Refresh)) { Menu = commands.Menu });

    private sealed record Browsing<TItem>(TileCache<TItem> Tiles) where TItem : notnull {
        public Func<TItem, bool> Keep { get; init; } = static _ => true;
        public Option<float> Resizing { get; init; }
        public Selection Picked { get; init; } = new Selection.Many([]);
        public Option<Key> Hovered { get; init; }
        public LanguageExt.HashSet<Key> Viewed { get; init; }
        public Option<float> Scale { get; init; }
    }

    private sealed record Realized<TItem>(
        IO<ControlRow.Bound<BrowserRow<TItem>>> Latest, RowScope Scope, SceneView View, Atom<Browsing<TItem>> State, Retained<IDisposable> Loads, Retained<IDisposable> Dwell,
        CommandHost<RowRule, Invoke> Host, Seq<Seq<Command<RowRule, Invoke>>> Menu)
        where TItem : notnull;

    private static IO<Retained<IDisposable>> Bin(Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain);

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
                    Accessible = AccessRole.CheckBox.Marked(new Accessible { Role = AccessRole.CheckBox, Label = RowText.Localize(mark.Caption, browser.Scope.Sink).Local }, on ? Check.On : Check.Off),
                }),
            }),
            Resized = current.Row.Resized.Map(resized => (RowText.Localize("Tile size", browser.Scope.Sink).Local, (Func<Edit<float>, IO<Unit>>)(change => Resized(browser, resized, change)))),
            Empty = Some(RowText.Localize(current.Row.Empty, browser.Scope.Sink).Local),
        };

    private static IO<Unit> Advanced<TItem>(Realized<TItem> browser, Func<Browsing<TItem>, Browsing<TItem>> next) where TItem : notnull =>
        from current in browser.Latest
        from moved in browser.State.SwapIO(state => Normalized(current.Row, next(state)))
        let pruned = Laid(browser, current, moved).Pruned
        from kept in browser.State.SwapIO(state => state with { Tiles = pruned.Tiles })
        from released in DisposalOps.Release(pruned.Released)
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

    private static Seq<TItem> Listed<TItem>(BrowserRow<TItem> row, LanguageExt.HashSet<Key> keys) where TItem : notnull =>
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
            from forked in asked.Wanted.TraverseM(ask => DisposalOps.Fork(Reported(browser, Loaded(browser, current.Row, ask.Key, ask.Tile)), browser.Loads.Retain)).As()
            select unit).As()
        select unit;

    private static IO<Unit> Loaded<TItem>(Realized<TItem> browser, BrowserRow<TItem> row, Key key, Tile<TItem> tile) where TItem : notnull =>
        (row.Tile(browser.Scope, tile.Item, tile.Pixels).Map(static image => Fin.Succ(image))
         | @catch(static error => !ErrorOps.Cancellation(error), static error => IO.pure(Fin.Fail<SKImage>(error))))
            .Bind(arrival => (
                from state in browser.State.ValueIO
                let arrived = state.Tiles.Arrived(key, tile, arrival)
                from stored in browser.State.SwapIO(held => held with { Tiles = arrived.Cache })
                from released in DisposalOps.Release(arrived.Released)
                from shown in Advanced(browser, static held => held)
                select unit).OnUiThread());

    private static IO<Unit> Reported<TItem>(Realized<TItem> browser, IO<Unit> work) where TItem : notnull =>
        work | @catch(static error => !ErrorOps.Cancellation(error), error => IO.lift(() => browser.Scope.Sink.Report(error, typeof(ListRows), nameof(Browser))));

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
        from drained in browser.Dwell.Drain
        from current in browser.Latest
        from dwelt in (from preview in current.Row.Preview from held in key select (Preview: preview, Key: held)).Traverse(hover =>
            DisposalOps.Acquire(keep => DisposalOps.Fork(Reported(browser, Dwelled(browser, hover.Key, hover.Preview)), keep).Map(static _ => unit), (_, lease) => browser.Dwell.Retain(lease))).As()
        select unit;

    private static IO<Unit> Dwelled<TItem>(Realized<TItem> browser, Key key, Func<RowScope, TItem, Action<IDisposable>, IO<Unit>> preview) where TItem : notnull =>
        from delay in Accessors.StepEventsDelay
        from waited in IO.yieldFor(TimeSpan.FromSeconds(delay))
        from shown in (from current in browser.Latest
                       from state in browser.State.ValueIO
                       from visible in browser.Scope.Visibility
                       from previewed in (from item in Listed(current.Row, [key]).Head where visible && current.Enabled && state.Hovered == Some(key) select item)
                           .Traverse(item => preview(browser.Scope, item, browser.Dwell.Retain)).As()
                       select unit).OnUiThread()
        select unit;

    // --- [SIZE]
    private static IO<Unit> Resized<TItem>(Realized<TItem> browser, Func<Edit<float>, IO<Unit>> resized, Edit<float> change) where TItem : notnull =>
        from moved in Advanced(browser, state => state with { Resizing = change.Value.Filter(_ => change.Phase == EditPhase.Preview) })
        from applied in change.Applied.Traverse(_ => resized(change)).As()
        select unit;
}
