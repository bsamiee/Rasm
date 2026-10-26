using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
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

    public static ListColumn<TItem> Named<TValue, TRaw>(Option<string> caption, Lens<TItem, TValue> name, Option<Func<TItem, GlyphSource>> icon)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        new Label(
            caption,
            item => StoredText.Format(name.Get(item).ToValue()),
            icon,
            Some<Func<TItem, string, Fin<TItem>>>((item, text) => StoredText.Parse<TRaw>(text).Bind(ValidationResults.Validated<TValue, TRaw>).Map(value => name.Set(value, item))));

    public static ListColumn<TItem> Fraction<TValue, TKey>(Option<string> caption, Func<TItem, Option<TValue>> read)
        where TValue : System.Numerics.IMinMaxValue<TValue>, IConvertible<TKey>
        where TKey : System.Numerics.INumber<TKey> =>
        new Progress(caption, item => read(item).Map(static value => float.CreateChecked(
            (double.CreateChecked(value.ToValue()) - double.CreateChecked(TValue.MinValue.ToValue()))
            / (double.CreateChecked(TValue.MaxValue.ToValue()) - double.CreateChecked(TValue.MinValue.ToValue())))));

    public sealed record Label(Option<string> Caption, Func<TItem, string> Read, Option<Func<TItem, GlyphSource>> Icon, Option<Func<TItem, string, Fin<TItem>>> Rename)
        : ListColumn<TItem>(Caption);

    public sealed record Check(Option<string> Caption, Lens<TItem, bool> Value) : ListColumn<TItem>(Caption);

    public sealed record Glyph(Option<string> Caption, Func<TItem, GlyphSource> Icon) : ListColumn<TItem>(Caption);

    public sealed record Progress(Option<string> Caption, Func<TItem, Option<float>> Read) : ListColumn<TItem>(Caption);
}

public sealed record ListSearch<TItem>(string Placeholder, Func<RowScope, TItem, string> Text, Option<RowSource<bool>> Shown) where TItem : notnull;

public sealed record ListBranches<TItem, TKey>(
    Lens<TItem, Seq<TItem>> Children,
    (RowSource<LanguageExt.HashSet<TKey>> Source, IStateParameter<LanguageExt.HashSet<TKey>> Parameter) Expanded)
    where TItem : notnull
    where TKey : notnull;

public sealed record ListRow<TItem, TKey>(
    IterableNE<ListColumn<TItem>> Columns,
    Func<TItem, TKey> Key,
    (RowSource<Seq<TItem>> Source, IStateParameter<Seq<TItem>> Parameter) Items,
    (RowSource<LanguageExt.HashSet<TKey>> Source, IStateParameter<LanguageExt.HashSet<TKey>> Parameter) Selected)
    where TItem : notnull
    where TKey : notnull {
    public Option<ListBranches<TItem, TKey>> Branches { get; init; }
    public Option<(RowSource<Option<TKey>> Source, IStateParameter<Option<TKey>> Parameter)> Current { get; init; }
    public Option<Func<TItem, string>> Ordered { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Option<Func<TItem, bool>> Available { get; init; }
    public Option<Func<TItem, IO<Unit>>> Activate { get; init; }
    public Option<string> Empty { get; init; }
    public Option<(Command<RowRule, Invoke>.Run Add, Command<RowRule, Invoke>.Run Remove)> Pair { get; init; }
    public Func<IO<Seq<TItem>>, Seq<Command<RowRule, Invoke>>> Commands { get; init; } = static _ => [];
    public bool AllowMultipleSelection { get; init; }
}

internal sealed record BrowserLayout(Option<Magnitude> Size, Option<TileArrangement> Arrangement) : IStateRecord<BrowserLayout, BrowserLayoutParameter> {
    public static BrowserLayout Default { get; } = new(None, None);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class BrowserLayoutParameter : IStateParameter<BrowserLayout> {
    public static readonly BrowserLayoutParameter Size = new("tile-size", new StateParameter<BrowserLayout>.OptionalKeyed<Magnitude, float>(
            Lens<BrowserLayout, Option<Magnitude>>.New(static layout => layout.Size, static size => layout => layout with { Size = size })),
        new("Tile size", "Thumbnail width, arrangement default when absent"));
    public static readonly BrowserLayoutParameter Arrangement = new("arrangement", new StateParameter<BrowserLayout>.OptionalChoice<TileArrangement>(
            Lens<BrowserLayout, Option<TileArrangement>>.New(static layout => layout.Arrangement, static arrangement => layout => layout with { Arrangement = arrangement }), "Default"),
        new("View", "Thumbnail arrangement, row default when absent"));

    public StateParameter<BrowserLayout> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record TileMark<TItem>(string Caption, GlyphSource On, GlyphSource Off, Lens<TItem, bool> Value) where TItem : notnull;

public sealed record BrowserRow<TItem>(
    Func<TItem, Key> Key,
    Func<RowScope, TItem, string> Label,
    Func<RowScope, TItem, SKSizeI, IO<SKImage>> Tile,
    Func<RowScope, ValueStore<Seq<TItem>>> Items,
    Func<RowScope, IO<Option<Key>>> Current,
    Func<RowScope, Key, IO<Unit>> Choose,
    TileArrangement Arrangement,
    ReadoutSource<Factor> Aspect,
    string Empty)
    where TItem : notnull {
    public bool Numbered { get; init; }
    public bool Reorders { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Option<Func<TItem, string, Fin<TItem>>> Rename { get; init; }
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
    public static ControlRow List<TItem, TKey>(ParameterText text, ListRow<TItem, TKey> row, RowRules rules)
        where TItem : notnull
        where TKey : notnull =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => DisposalOps.Acquire(owned =>
            from grid in DisposalOps.Acquire(IO.lift(static () => new TreeGridView()), owned)
            from faced in IO.lift(() => {
                (grid.ShowHeader, grid.GridLines, grid.Border, grid.AllowMultipleSelection, grid.AllowEmptySelection) =
                    (toSeq(row.Columns).Exists(static column => column.Caption.IsSome), GridLines.None, BorderType.None, row.AllowMultipleSelection, true);
            })
            from emphasis in IO.lift(static () => SystemFonts.Bold(SystemFonts.Label().Size))
            from list in IO.lift(static () => Atom(new ListState<TItem>(Seq<RowItem<TItem>>(), [], Seq<(RowItem<TItem> Node, TItem Row)>())))
            from keep in atomIO<Func<TItem, bool>>(static _ => true)
            from columns in Columns(row, grid, scope.Sink, owned)
            from commands in Commanded(row.Pair, row.Commands(IO.lift(() => Chosen<TItem>(grid).Map(static node => node.Row))), scope, owned)
            from selection in Selected(grid, list, row, scope, owned)
            from items in Items(grid, list, keep, row, selection.Shown, scope, owned)
            from expanded in row.Branches.Traverse(branches => Expansion(grid, list, row, branches, scope, owned)).As()
            from current in row.Current.Traverse(held => Watched(held.Source, held.Source.Keys(held.Parameter),
                shown => IO.lift(() => Indices(row, list.Value.Nodes.Bind(static node => node.Shown), toHashSet(shown.ToSeq())).Head.Iter(grid.ScrollToRow)),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.ScrollToRow)), owned)).As()
            let brackets = Seq((items.Keys, items.Shown), (selection.Keys, selection.Shown)) + expanded.ToSeq() + current.ToSeq()
            from gestures in Gestures(grid, list, row, emphasis, commands.Keyed, scope, owned)
            from emptied in row.Empty.Traverse(empty => Emptied(grid, list, RowText.Localize(empty, scope.Sink).Local, scope.Sink, owned)).As()
            from paired in Paired(emptied.IfNone(grid), commands.Pair, owned)
            from content in Searched(paired, row.Search, kept => writeIO(keep, kept).Bind(_ => items.Shown), scope, owned)
            from listened in Listened(brackets + content.Brackets, scope, owned)
            select new RowCells(content.Control, None, None, None, [], [], new RowHelp(None, None),
                Callbacks.Each((brackets + content.Brackets).Map(static bracket => bracket.Shown)).Bind(_ => commands.Refresh),
                None, Some(ChoiceRows.Context(grid, commands.Menu, Seq(commands.Refresh)))), retain),
            text, rules);

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

    // --- [SELECTION]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Selected<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(row.Selected.Source, IterableNE.create(row.Selected.Parameter),
                h => grid.SelectionChanged += h, h => grid.SelectionChanged -= h,
                edit => Callbacks.Handler<EventArgs>(
                    _ => IO.lift(() => toHashSet(Chosen<TItem>(grid).Map(node => row.Key(node.Row)))).Bind(keys => edit.Commit(keys)).Map(unit),
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectionChanged))),
                agreed => Reselected(grid, list, row, agreed),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectedRows)), retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<Unit> Reselected<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Option<LanguageExt.HashSet<TKey>> agreed)
        where TItem : notnull
        where TKey : notnull =>
        from state in list.ValueIO
        from placed in unless(ReferenceEquals(grid.DataStore, state.Store), IO.lift(fun(() => { grid.DataStore = state.Store; }))).As()
        from held in IO.lift(() => toHashSet(Chosen<TItem>(grid).Map(node => row.Key(node.Row))))
        from selected in agreed.Filter(keys => keys != held).Traverse(keys => IO.lift(() => { grid.SelectedRows = Indices(row, state.Nodes.Bind(static node => node.Shown), keys); })).As()
        select unit;

    // --- [FILL]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Items<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, Atom<Func<TItem, bool>> keep, ListRow<TItem, TKey> row, IO<Unit> reselect, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(row.Items.Source, IterableNE.create(row.Items.Parameter),
                h => grid.CellEdited += h, h => grid.CellEdited -= h,
                edit => Callbacks.Handler<GridViewCellEventArgs>(
                    args => from stored in scope.Read(row.Items.Source)
                            from committed in RowItem<TItem>.Of(args.Item).Traverse(node => edit.Commit(Replaced(row.Key, row.Branches, stored, node.Row))).As()
                            select unit,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellEdited))),
                agreed => Fill(grid, list, keep, row, reselect, agreed.ToSeq().Flatten()),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.DataStore)), retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<Unit> Fill<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, Atom<Func<TItem, bool>> keep, ListRow<TItem, TKey> row, IO<Unit> reselect, Seq<TItem> items)
        where TItem : notnull
        where TKey : notnull =>
        from kept in keep.ValueIO
        from next in list.SwapIO(state => Filled(row, state, items, kept))
        from reloaded in next.Reloads.TraverseM(reload => IO.lift(fun(() => {
            reload.Node.Row = reload.Row;
            grid.ReloadItem(reload.Node, reloadChildren: false);
        }))).As()
        from reselected in unless(ReferenceEquals(grid.DataStore, next.Store), reselect).As()
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
        from size in IconSlot.Small.Size()
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
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Expansion<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, ListBranches<TItem, TKey> branches, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(branches.Expanded.Source, IterableNE.create(branches.Expanded.Parameter),
                h => Callbacks.Each(Seq(IO.lift(() => grid.Expanded += h), IO.lift(() => grid.Collapsed += h))).Run(),
                h => Callbacks.Each(Seq(IO.lift(() => grid.Expanded -= h), IO.lift(() => grid.Collapsed -= h))).Run(),
                edit => Callbacks.Handler<TreeGridViewItemEventArgs>(
                    _ => list.ValueIO.Bind(state => edit.Commit(Open(row, state.Nodes))).Map(unit),
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Expanded))),
                open => Expand(grid, list, row, open),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.ReloadItem)), retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<Unit> Expand<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Option<LanguageExt.HashSet<TKey>> open)
        where TItem : notnull
        where TKey : notnull =>
        list.ValueIO.Bind(state => state.Nodes.Bind(static node => node.Preorder)
            .Filter(node => node.Expanded != open.Exists(keys => keys.Contains(row.Key(node.Row))))
            .TraverseM(node => IO.lift(fun(() => {
                node.Expanded = !node.Expanded;
                grid.ReloadItem(node, reloadChildren: true);
            })))
            .As()
            .Map(unit));

    // --- [GESTURES]
    private static IO<Unit> Gestures<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Font emphasis, Func<Keys, IO<bool>> keyed, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        Callbacks.Each(Seq(
            Subscriptions.Attach(h => grid.KeyDown += h, h => grid.KeyDown -= h,
                Callbacks.Synchronous<KeyEventArgs>(args => Pressed(grid, list, row, keyed, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.KeyDown))), retain),
            Subscriptions.Attach(h => grid.MouseDown += h, h => grid.MouseDown -= h,
                Callbacks.Synchronous<MouseEventArgs>(args => when(args.Buttons is MouseButtons.Alternate, IO.lift(() => RowItem<TItem>.Of(grid.GetCellAt(args.Location).Item)
                    .Filter(node => !Chosen<TItem>(grid).Exists(held => ReferenceEquals(held, node)))
                    .Iter(node => grid.SelectedItem = node))).As(), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.MouseDown))), retain),
            Subscriptions.Attach(h => grid.CellFormatting += h, h => grid.CellFormatting -= h,
                Callbacks.Synchronous<GridCellFormatEventArgs>(args => IO.lift(() => RowItem<TItem>.Of(args.Item).Iter(node =>
                    (args.Font, args.ForegroundColor) = (
                        node.Kids.IsEmpty ? args.Font : emphasis,
                        row.Available.ForAll(available => available(node.Row)) ? args.ForegroundColor : SystemColors.DisabledText))),
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellFormatting))), retain),
            HostTheme.In(grid).Inline(_ => IO.lift(grid.ReloadData), scope.Sink, retain))
            + row.Activate.Map(activate => Subscriptions.Attach(h => grid.Activated += h, h => grid.Activated -= h,
                Callbacks.Handler<TreeGridViewItemEventArgs>(_ => Chosen<TItem>(grid).Head.Traverse(node => activate(node.Row)).As().Map(unit),
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Activated))), retain)).ToSeq()).Map(unit);

    private static IO<Unit> Pressed<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Func<Keys, IO<bool>> keyed, KeyEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        from rows in IO.lift(() => toSeq(grid.SelectedRows).Strict())
        from shown in IO.lift(() => !list.Value.Nodes.IsEmpty)
        from handled in args.KeyData switch {
            Keys.F2 => Handled(args,
                from at in rows.Head
                from renaming in toSeq(row.Columns).Map(static (each, index) => (Column: each, Index: index)).Find(static entry => entry.Column.Renames)
                select (Action)(() => grid.BeginEdit(at, renaming.Index))),
            Keys.Up or Keys.Down when rows.IsEmpty && shown => Handled(args, Some(() => grid.SelectRow(0))),
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
        from label in DisposalOps.Acquire(IO.lift(() => new Eto.Forms.Label { Text = text, TextColor = SystemColors.DisabledText, TextAlignment = TextAlignment.Center }), retain)
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
            Spacing = RhinoLayout.StackedSpacing(Eto.Forms.Orientation.Vertical, RhinoLayout.SpacingType.Panel),
        }, retain).Map(static stack => (Control)stack);

    private static IO<(Control Control, Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)> Brackets)> Searched<TItem>(
        Control content, Option<ListSearch<TItem>> search, Func<Func<TItem, bool>, IO<Unit>> keep, RowScope scope, Action<IDisposable> retain) where TItem : notnull =>
        search.Match(Some: held =>
            from field in ChoiceRows.Search(RowText.Localize(held.Placeholder, scope.Sink).Local, matches => keep(item => matches(held.Text(scope, item))), scope.Sink, retain)
            from shown in held.Shown.Traverse(source => Watched(source, Named(source, scope), visible => IO.lift(() => { field.Visible = visible; }), scope,
                new CallbackSite(scope.Sink, typeof(SearchBox), nameof(SearchBox.Visible)), retain)).As()
            from stack in Stacked(retain, new StackLayoutItem(field), new StackLayoutItem(content, expand: true))
            select (stack, shown.ToSeq()),
            None: () => IO.pure((content, Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)>())));

    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Watched<TValue>(
        RowSource<TValue> source, Seq<EntryKey> keys, Func<TValue, IO<Unit>> show, RowScope scope, CallbackSite site, Action<IDisposable> retain) where TValue : notnull =>
        from signals in source.Signals(scope)
        let shown = scope.Read(source).Bind(show)
        from held in Callbacks.Each(signals.Map(signal => signal.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ => shown), site.Sink, retain)))
        select (keys, shown);

    private static Seq<EntryKey> Named(RowSource source, RowScope scope) =>
        source.Values(scope).Paths().Map(path => new EntryKey(source.Owner, path));

    private static IO<Unit> Listened(Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)> brackets, RowScope scope, Action<IDisposable> retain) =>
        scope.Listen(changed => Callbacks.Each(brackets.Filter(bracket => bracket.Keys.Exists(key => changed.Exists(held => held == key))).Map(static bracket => bracket.Shown)).Map(unit), retain);

    // --- [BROWSERS]
    private static readonly RowSource<BrowserLayout> Layout = RowSource.Fields(nameof(BrowserLayout), [.. BrowserLayoutParameter.Items],
        static scope => IO.pure(scope.Settings.Map(static node => PlugInSettings.Record<BrowserLayout>(node.Child(nameof(BrowserLayout)), Applied.Live)).ToSeq()));

    public static Command<RowRule, Invoke>.Choice Arrangement(TileArrangement initial) =>
        CommandRows.Choices<TileArrangement>(
            new CommandFace($"{nameof(BrowserLayout)}-{BrowserLayoutParameter.Arrangement.Key}", BrowserLayoutParameter.Arrangement.Text.Caption, Some(BrowserLayoutParameter.Arrangement.Text.Help), None),
            None,
            arrangement => RowRule.When(Layout, held => held.Arrangement.IfNone(initial) == arrangement, BrowserLayoutParameter.Arrangement),
            static (scope, arrangement) => Written(scope, held => held with { Arrangement = Some(arrangement) }),
            static arrangement => new CommandFace($"{nameof(BrowserLayout)}-{arrangement.Key}", ((ICaptioned)arrangement).Caption, None, Some<GlyphSource>(Icons.Embedded(typeof(ListRows).Assembly,
                arrangement.Map(grid: "grid.svg", list: "list.svg")))));

    public static ControlRow Browser<TItem>(ParameterText text, BrowserRow<TItem> row, RowRules rules) where TItem : notnull =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => DisposalOps.Acquire(owned =>
                from held in atomIO(new Browsing<TItem>([], Factor.Unit, BrowserLayout.Default, TileCache<TItem>.Empty))
                from wired in atomIO(Option<Wiring<TItem>>.None)
                from loads in Bin(owned)
                from dwell in Bin(owned)
                let site = new CallbackSite(scope.Sink, typeof(SceneView), nameof(Browser))
                from view in TextRows.Live(row.Aspect,
                    (aspect, keep) => held.SwapIO(state => state with { Aspect = aspect }).Bind(_ => DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), keep)),
                    (made, aspect) => Advanced(new Realized<TItem>(row, scope, made, held, wired, loads, dwell), state => state with { Aspect = aspect }),
                    scope, owned)
                let browser = new Realized<TItem>(row, scope, view, held, wired, loads, dwell)
                from released in DisposalOps.Acquire(IO.pure(unit), _ => held.ValueIO.Bind(static state => DisposalOps.Release(state.Tiles.Kept([]).Released)), owned)
                let listed = new ValueKey<Seq<TItem>>(nameof(BrowserRow<>.Items), new StateParameter<Seq<TItem>>.Opaque<Seq<TItem>>(Lens.identity<Seq<TItem>>()), text, Seq<TItem>())
                from edit in RowEdit.Bind<Seq<TItem>, Action>(RowSource.Of(text.Caption, listed, row.Items), IterableNE.create<IStateParameter<Seq<TItem>>>(listed),
                    static _ => { }, static _ => { }, static _ => static () => { },
                    shown => Filled(browser, shown.ToSeq().Flatten()), scope, site, owned)
                from current in Current(browser, site, owned)
                from laid in Watched(Layout, Named(Layout, scope), layout => Advanced(browser, state => state with { Layout = layout }), scope, site, owned)
                from commands in Commanded(None, row.Commands(held.ValueIO.Map(state => Picked(row, state))), scope, owned)
                from host in CommandRows.Host(scope.Hosted(view), owned)
                from bound in writeIO(wired, Some(new Wiring<TItem>(
                    change => edit.Take(change, static (rearrange, latest) => rearrange(latest)).Map(static _ => unit), current.Choose, host, commands.Menu)))
                from keys in Subscriptions.Attach(h => view.KeyDown += h, h => view.KeyDown -= h,
                    Callbacks.Synchronous<KeyEventArgs>(args => Pressed(browser, commands.Keyed, args), site with { Member = nameof(Control.KeyDown) }), owned)
                from sized in Subscriptions.Attach(h => view.SizeChanged += h, h => view.SizeChanged -= h, Callbacks.Handler<EventArgs>(_ => Placed(browser), site with { Member = nameof(Control.SizeChanged) }), owned)
                from loaded in Subscriptions.Attach(h => view.LoadComplete += h, h => view.LoadComplete -= h,
                    Callbacks.Handler<EventArgs>(_ => Placed(browser), site with { Member = nameof(Control.LoadComplete) }), owned)
                from reloads in Callbacks.Each(row.Reloads.Map(reload => reload.Choose(static _ => Some((unit, unit)))
                    .Through(Subscriptions.Idle<Unit, Unit>(_ => loads.Drain.Bind(_ => Advanced(browser, static state => state with { Tiles = state.Tiles.Reloaded })).Bind(_ => edit.Shown).Bind(_ => current.Shown)),
                        scope.Sink, owned)))
                from content in Searched(view, row.Search, keep => Advanced(browser, state => state with { Keep = keep }), scope, owned)
                let brackets = Seq((edit.Keys, edit.Shown), laid) + content.Brackets
                from listened in Listened(brackets, scope, owned)
                from placed in Placed(browser)
                select new RowCells(content.Control, None, None, None, [], [], new RowHelp(None, None),
                    Callbacks.Each(brackets.Map(static bracket => bracket.Shown)).Bind(_ => current.Shown).Bind(_ => commands.Refresh),
                    None, Some(ChoiceRows.Context(view, commands.Menu, Seq(commands.Refresh)))) {
                    Availability = Some<Func<bool, IO<Unit>>>(enabled => Advanced(browser, state => state with { Enabled = enabled })),
                }, retain),
            text, rules);

    private sealed record Browsing<TItem>(Seq<TItem> Items, Factor Aspect, BrowserLayout Layout, TileCache<TItem> Tiles) where TItem : notnull {
        public Func<TItem, bool> Keep { get; init; } = static _ => true;
        public Option<float> Resizing { get; init; }
        public Selection Picked { get; init; } = new Selection.Many([], None, None);
        public Option<Key> Current { get; init; }
        public Option<Key> Hovered { get; init; }
        public Option<Key> Renaming { get; init; }
        public LanguageExt.HashSet<Key> Viewed { get; init; }
        public Option<float> Scale { get; init; }
        public bool Enabled { get; init; } = true;
    }

    private sealed record Wiring<TItem>(
        Func<Edit<Func<Seq<TItem>, Fin<Seq<TItem>>>>, IO<Unit>> Items, Func<Edit<Key>, IO<Unit>> Choose, CommandHost<RowRule, Invoke> Host, Seq<Seq<Command<RowRule, Invoke>>> Menu)
        where TItem : notnull;

    private sealed record Realized<TItem>(
        BrowserRow<TItem> Row, RowScope Scope, SceneView View, Atom<Browsing<TItem>> State, Atom<Option<Wiring<TItem>>> Wired, Retained<IDisposable> Loads, Retained<IDisposable> Dwell)
        where TItem : notnull;

    private static IO<Retained<IDisposable>> Bin(Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain);

    // --- [GALLERY]
    private static Gallery<TItem> Laid<TItem>(Realized<TItem> browser, Browsing<TItem> state) where TItem : notnull =>
        new(state.Items.Filter(state.Keep), browser.Row.Key, item => browser.Row.Label(browser.Scope, item), state.Tiles, keys => new Part.Choice(keys, state.Picked, change => Selecting(browser, change)) {
            Reorder = browser.Row.Reorders ? Some<Func<Edit<Seq<Key>>, IO<Unit>>>(change => Edited(browser, change.Map(order => Rearranged(browser.Row.Key, order)).As())) : None,
            Exported = browser.Row.File.Map(file => (Func<Seq<Key>, IO<Unit>>)(moved => DataTransfer.Drag(browser.View, Listed(browser.Row, state, toHashSet(moved)).Choose(file), None))),
            Menu = Some<Func<Seq<Key>, SKRect, IO<Unit>>>((targeted, frame) => Menued(browser, targeted, frame)),
            Hover = Some<Func<Option<Key>, IO<Unit>>>(hovered => Hovered(browser, hovered)),
            Viewed = Some<Func<LanguageExt.HashSet<Key>, IO<Unit>>>(seen => Advanced(browser, current => current with { Viewed = seen })),
        }) {
            Arrangement = state.Layout.Arrangement.IfNone(browser.Row.Arrangement),
            TileSize = state.Resizing | state.Layout.Size.Map(static size => (float)size),
            Aspect = state.Aspect,
            Realized = state.Viewed,
            Hovered = state.Hovered,
            Current = state.Current,
            Numbered = browser.Row.Numbered,
            Marks = browser.Row.Marks.Map(mark => new GalleryMark<TItem>(RowText.Localize(mark.Caption, browser.Scope.Sink).Local, mark.On, mark.Off, mark.Value.Get,
                (item, on) => Revised(browser, mark.Value.Set(on, item)))),
            Renames = browser.Row.Rename.Map(_ => new GalleryRename(state.Renaming, key => Renaming(browser, Some(key)), (key, edit) => Renamed(browser, key, edit))),
            Resized = Some<Func<Edit<float>, IO<Unit>>>(change => Resized(browser, change)),
            Empty = Some(RowText.Localize(browser.Row.Empty, browser.Scope.Sink).Local),
        };

    private static IO<Unit> Advanced<TItem>(Realized<TItem> browser, Func<Browsing<TItem>, Browsing<TItem>> next) where TItem : notnull =>
        from moved in browser.State.SwapIO(state => Normalized(browser.Row, next(state)))
        let pruned = Laid(browser, moved).Pruned
        from kept in browser.State.SwapIO(state => state with { Tiles = pruned.Tiles })
        from released in DisposalOps.Release(pruned.Released)
        from shown in browser.View.Show(Laid(browser, kept).Drawn with { Conditions = kept.Enabled ? [] : [Condition.Disabled] })
        from requested in Requested(browser)
        select unit;

    private static Browsing<TItem> Normalized<TItem>(BrowserRow<TItem> row, Browsing<TItem> state) where TItem : notnull =>
        (Keys: toHashSet(state.Items.Map(row.Key)), Shown: toHashSet(state.Items.Filter(state.Keep).Map(row.Key))) switch {
            var held => state with {
                Picked = state.Picked.Switch(
                    held.Shown,
                    one: static (shown, one) => (Selection)new Selection.One(one.Picked.Filter(shown.Contains)),
                    many: static (shown, many) => new Selection.Many(many.Picked.Intersect(shown), many.Anchor.Filter(shown.Contains), many.Focus.Filter(shown.Contains))),
                Current = state.Current.Filter(held.Keys.Contains),
                Hovered = state.Hovered.Filter(held.Shown.Contains),
                Renaming = state.Renaming.Filter(held.Shown.Contains),
                Viewed = state.Viewed.Intersect(held.Shown),
            },
        };

    private static IO<Unit> Filled<TItem>(Realized<TItem> browser, Seq<TItem> items) where TItem : notnull =>
        browser.State.ValueIO.Bind(state => state.Items == items ? IO.pure(unit) : Advanced(browser, held => held with { Items = items }));

    private static IO<Unit> Placed<TItem>(Realized<TItem> browser) where TItem : notnull =>
        IO.lift(() => Optional(browser.View.ParentWindow).Map(static window => window.LogicalPixelSize)).Bind(scale => Advanced(browser, state => state with { Scale = scale }));

    private static Seq<TItem> Listed<TItem>(BrowserRow<TItem> row, Browsing<TItem> state, LanguageExt.HashSet<Key> keys) where TItem : notnull =>
        state.Items.Filter(item => keys.Contains(row.Key(item)));

    private static Seq<TItem> Picked<TItem>(BrowserRow<TItem> row, Browsing<TItem> state) where TItem : notnull =>
        state.Items.Filter(state.Keep).Filter(item => state.Picked.Holds(row.Key(item)));

    // --- [TILES]
    private static IO<Unit> Requested<TItem>(Realized<TItem> browser) where TItem : notnull =>
        from state in browser.State.ValueIO
        from loaded in state.Scale.Map(scale => Laid(browser, state).Requested(scale)).Filter(static asked => !asked.Wanted.IsEmpty).Traverse(asked =>
            from marked in browser.State.SwapIO(held => held with { Tiles = asked.Tiles })
            from forked in asked.Wanted.TraverseM(ask => DisposalOps.Fork(Reported(browser, Loaded(browser, ask.Key, ask.Tile)), browser.Loads.Retain)).As()
            select unit).As()
        select unit;

    private static IO<Unit> Loaded<TItem>(Realized<TItem> browser, Key key, Tile<TItem> tile) where TItem : notnull =>
        (browser.Row.Tile(browser.Scope, tile.Item, tile.Pixels).Map(static image => Fin.Succ(image))
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
        from moved in change.Carried.Traverse(picked => Advanced(browser, state => state with { Picked = picked })).As()
        from state in browser.State.ValueIO
        from chosen in change.Settled.Bind(static picked => picked.Lead.Filter(picked.Holds)).Filter(lead => state.Current != Some(lead))
            .Traverse(lead => browser.Wired.ValueIO.Bind(wired => wired.Traverse(held => held.Choose(change.Map(_ => lead).As())).As())).As()
        select unit;

    private static Func<Seq<TItem>, Fin<Seq<TItem>>> Rearranged<TItem>(Func<TItem, Key> key, Seq<Key> order) where TItem : notnull =>
        latest => (Moved: toHashSet(order), Items: toHashMap(latest.Map(item => (key(item), item)))) switch {
            var held => latest.Fold((Placed: Seq<TItem>(), Next: order), (placed, item) => held.Moved.Contains(key(item))
                    ? placed.Next.Head.Bind(held.Items.Find).Match(Some: next => (placed.Placed.Add(next), placed.Next.Tail), None: () => (placed.Placed.Add(item), placed.Next))
                    : (placed.Placed.Add(item), placed.Next))
                .Placed,
        };

    private static IO<Unit> Revised<TItem>(Realized<TItem> browser, Fin<TItem> item) where TItem : notnull =>
        Edited(browser, new Edit<Func<Seq<TItem>, Fin<Seq<TItem>>>>.Commit(latest => item.Map(held => Replaced(browser.Row.Key, None, latest, held))));

    private static IO<Unit> Edited<TItem>(Realized<TItem> browser, Edit<Func<Seq<TItem>, Fin<Seq<TItem>>>> change) where TItem : notnull =>
        browser.Wired.ValueIO.Bind(wired => wired.Traverse(held => held.Items(change)).As()).Map(unit);

    private static IO<Unit> Menued<TItem>(Realized<TItem> browser, Seq<Key> targeted, SKRect frame) where TItem : notnull =>
        from moved in Advanced(browser, state => state with { Picked = new Selection.Many(toHashSet(targeted), targeted.Head, targeted.Head) })
        from shown in browser.Wired.ValueIO.Bind(wired => wired.Traverse(held => held.Host.Menu(held.Menu, frame)).As())
        select unit;

    // --- [HOVER]
    private static IO<Unit> Hovered<TItem>(Realized<TItem> browser, Option<Key> key) where TItem : notnull =>
        from moved in Advanced(browser, state => state with { Hovered = key })
        from drained in browser.Dwell.Drain
        from dwelt in (from preview in browser.Row.Preview from held in key select (Preview: preview, Key: held)).Traverse(hover =>
            DisposalOps.Acquire(keep => DisposalOps.Fork(Reported(browser, Dwelled(browser, hover.Key, hover.Preview)), keep).Map(static _ => unit), (_, lease) => browser.Dwell.Retain(lease))).As()
        select unit;

    private static IO<Unit> Dwelled<TItem>(Realized<TItem> browser, Key key, Func<RowScope, TItem, Action<IDisposable>, IO<Unit>> preview) where TItem : notnull =>
        from delay in Accessors.StepEventsDelay
        from waited in IO.yieldFor(TimeSpan.FromSeconds(delay))
        from shown in (from state in browser.State.ValueIO
                       from visible in browser.Scope.Visibility
                       from previewed in (from item in Listed(browser.Row, state, [key]).Head where visible && state.Enabled && state.Hovered == Some(key) select item)
                           .Traverse(item => preview(browser.Scope, item, browser.Dwell.Retain)).As()
                       select unit).OnUiThread()
        select unit;

    // --- [RENAME]
    private static IO<Unit> Renaming<TItem>(Realized<TItem> browser, Option<Key> key) where TItem : notnull =>
        Advanced(browser, state => state with { Renaming = key.Filter(_ => browser.Row.Rename.IsSome) });

    private static IO<Unit> Renamed<TItem>(Realized<TItem> browser, Key key, Edit<string> edit) where TItem : notnull =>
        edit.Switch(
            (Browser: browser, Key: key),
            preview: static (_, _) => IO.pure(unit),
            step: static (_, _) => IO.pure(unit),
            commit: static (held, commit) =>
                from state in held.Browser.State.ValueIO
                from closed in Renaming(held.Browser, None)
                from committed in (from item in Listed(held.Browser.Row, state, [held.Key]).Head
                                   from rename in held.Browser.Row.Rename
                                   where !string.Equals(commit.Value, held.Browser.Row.Label(held.Browser.Scope, item), StringComparison.Ordinal)
                                   select Revised(held.Browser, rename(item, commit.Value))).Traverse(static revised => revised).As()
                select unit,
            cancel: static (held, _) => Renaming(held.Browser, None));

    // --- [KEYS]
    private static IO<Unit> Pressed<TItem>(Realized<TItem> browser, Func<Keys, IO<bool>> keyed, KeyEventArgs args) where TItem : notnull =>
        args.KeyData switch {
            Keys.F2 when browser.Row.Rename.IsSome => IO.lift(() => { args.Handled = true; }).Bind(_ => browser.State.ValueIO).Bind(state => Renaming(browser, state.Picked.Lead)),
            var key => Keyed(keyed, key, args),
        };

    // --- [SIZE]
    private static IO<Unit> Resized<TItem>(Realized<TItem> browser, Edit<float> change) where TItem : notnull =>
        change.Switch(
            browser,
            preview: static (held, preview) => Advanced(held, state => state with { Resizing = Some(preview.Value) }),
            step: static (held, step) => Stored(held, step.Value),
            commit: static (held, commit) => Stored(held, commit.Value),
            cancel: static (held, _) => Advanced(held, static state => state with { Resizing = None }));

    private static IO<Unit> Stored<TItem>(Realized<TItem> browser, float width) where TItem : notnull =>
        from size in IO.lift(ValidationResults.Validated<Magnitude, float>(width))
        from moved in Advanced(browser, state => state with { Resizing = None, Layout = state.Layout with { Size = Some(size) } })
        from written in Written(browser.Scope, held => held with { Size = Some(size) })
        select unit;

    private static IO<Unit> Written(RowScope scope, Func<BrowserLayout, BrowserLayout> next) =>
        from held in scope.Read(Layout)
        from stored in scope.Store(Layout).Write(Some(next(held)))
        from changed in scope.Changed(Named(Layout, scope))
        select unit;

    // --- [CURRENT]
    private static IO<(Func<Edit<Key>, IO<Unit>> Choose, IO<Unit> Shown)> Current<TItem>(Realized<TItem> browser, CallbackSite site, Action<IDisposable> retain) where TItem : notnull =>
        from pending in atomIO(Option<Key>.None)
        from timer in DisposalOps.Acquire(IO.lift(static () => new UITimer()), retain)
        let shown = browser.Row.Current(browser.Scope).Bind(current => Advanced(browser, state => state with { Current = current }))
        let settled = DisposalOps.Transacted(
                IO.lift(timer.Stop).Bind(_ => pending.ValueIO),
                held => held.Traverse(key => browser.Row.Choose(browser.Scope, key)).As(),
                static _ => unitIO,
                (held, _) => when(held.IsSome, writeIO(pending, Option<Key>.None).Map(unit)).As())
            .Bind(_ => shown)
        from closing in DisposalOps.Acquire(IO.pure(unit), _ => settled, retain)
        from elapsed in Subscriptions.Attach(h => timer.Elapsed += h, h => timer.Elapsed -= h, Callbacks.Handler<EventArgs>(_ => settled, site), retain)
        from changed in browser.Scope.Listen(_ => shown, retain)
        select ((Func<Edit<Key>, IO<Unit>>)(change => change.Switch(
            (Timer: timer, Pending: pending, Shown: shown, Settled: settled),
            preview: static (_, _) => IO.pure(unit),
            step: static (held, step) =>
                from delay in Accessors.StepEventsDelay
                from queued in writeIO(held.Pending, Some(step.Value))
                from started in IO.lift(() => {
                    held.Timer.Interval = delay;
                    held.Timer.Start();
                })
                select unit,
            commit: static (held, commit) => writeIO(held.Pending, Some(commit.Value)).Bind(_ => held.Settled),
            cancel: static (held, _) => IO.lift(held.Timer.Stop).Bind(_ => writeIO(held.Pending, Option<Key>.None)).Bind(_ => held.Shown))), shown);
}
