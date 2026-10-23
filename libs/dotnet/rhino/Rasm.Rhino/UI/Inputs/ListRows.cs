using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Numeric;
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

    public sealed record Label(Option<string> Caption, Func<TItem, string> Read, Option<Func<TItem, IGlyph>> Icon, Option<Func<TItem, string, Fin<TItem>>> Rename)
        : ListColumn<TItem>(Caption);

    public sealed record Check(Option<string> Caption, Lens<TItem, bool> Value) : ListColumn<TItem>(Caption);

    public sealed record Glyph(Option<string> Caption, Func<TItem, IGlyph> Icon) : ListColumn<TItem>(Caption);

    public sealed record Progress(Option<string> Caption, Func<TItem, Option<float>> Read) : ListColumn<TItem>(Caption);
}

public static class ListColumn {
    public static ListColumn<TItem> Named<TItem, TValue, TRaw>(Option<string> caption, Lens<TItem, TValue> name, Option<Func<TItem, IGlyph>> icon)
        where TItem : notnull
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        new ListColumn<TItem>.Label(
            caption,
            item => StoredText.Capture<TValue, TRaw>(name.Get(item)),
            icon,
            Some<Func<TItem, string, Fin<TItem>>>((item, text) => StoredText.Recall<TValue, TRaw>(text).Map(value => name.Set(value, item))));

    public static ListColumn<TItem> Fraction<TItem, TValue, TKey>(Option<string> caption, Func<TItem, Option<TValue>> read)
        where TItem : notnull
        where TValue : System.Numerics.IMinMaxValue<TValue>, IConvertible<TKey>
        where TKey : System.Numerics.INumber<TKey> =>
        new ListColumn<TItem>.Progress(caption, item => read(item).Map(static value => float.CreateChecked(
            (double.CreateChecked(value.ToValue()) - double.CreateChecked(TValue.MinValue.ToValue()))
            / (double.CreateChecked(TValue.MaxValue.ToValue()) - double.CreateChecked(TValue.MinValue.ToValue())))));
}

public sealed record ListSearch<TItem>(string Placeholder, Func<RowScope, TItem, string> Text, Option<RowSource<bool>> Shown) where TItem : notnull;

public sealed record ListBranches<TItem, TKey>(
    Lens<TItem, Seq<TItem>> Children,
    (RowSource<LanguageExt.HashSet<TKey>> Source, RowField<LanguageExt.HashSet<TKey>> Field) Expanded)
    where TItem : notnull
    where TKey : notnull;

public sealed record ListRow<TItem, TKey>(
    IterableNE<ListColumn<TItem>> Columns,
    Func<TItem, TKey> Key,
    (RowSource<Seq<TItem>> Source, RowField<Seq<TItem>> Field) Items,
    (RowSource<LanguageExt.HashSet<TKey>> Source, RowField<LanguageExt.HashSet<TKey>> Field) Selected)
    where TItem : notnull
    where TKey : notnull {
    public Option<ListBranches<TItem, TKey>> Branches { get; init; }
    public Option<(RowSource<Option<TKey>> Source, RowField<Option<TKey>> Field)> Current { get; init; }
    public Option<Func<TItem, string>> Ordered { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Option<Func<TItem, bool>> Available { get; init; }
    public Option<Func<TItem, IO<Unit>>> Activate { get; init; }
    public Option<(CommandRow.Run Add, CommandRow.Run Remove)> Pair { get; init; }
    public Func<IO<Seq<TItem>>, Seq<CommandRow>> Commands { get; init; } = static _ => [];
    public bool AllowMultipleSelection { get; init; }
    public Wording Wording { get; init; } = Wording.English;
}

public sealed record BrowserLayout(Option<TileSize> Size, Option<BrowserMode> Mode) : IStateRecord<BrowserLayout, BrowserLayoutParameter> {
    public static BrowserLayout Default { get; } = new(None, None);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BrowserLayoutParameter : IStateParameter<BrowserLayout> {
    public static readonly BrowserLayoutParameter Size = new("tile-size", new StateParameter<BrowserLayout>.OptionalKeyed<TileSize, float>(
            Lens<BrowserLayout, Option<TileSize>>.New(static layout => layout.Size, static size => layout => layout with { Size = size })),
        new("Tile size", "Thumbnail width, fitted to browser when absent"));
    public static readonly BrowserLayoutParameter Mode = new("mode", new StateParameter<BrowserLayout>.OptionalChoice<BrowserMode>(
            Lens<BrowserLayout, Option<BrowserMode>>.New(static layout => layout.Mode, static mode => layout => layout with { Mode = mode }), "Default"),
        new("View", "Thumbnail arrangement, row default when absent"));

    public StateParameter<BrowserLayout> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record BrowserRow<TItem, TKey>(
    Func<TItem, TKey> Key,
    Func<RowScope, TItem, string> Label,
    Func<RowScope, Control, TItem, Size, Action<Disposal<Thumbnail>>, IO<Disposal<Thumbnail>>> Tile,
    Func<RowScope, ValueStore<Seq<TItem>>> Items,
    Func<RowScope, IO<Option<TKey>>> Current,
    Func<RowScope, TKey, IO<Unit>> Choose,
    BrowserMode Mode,
    ReadoutSource<Size> Extent,
    string Empty)
    where TItem : notnull
    where TKey : notnull {
    public bool Numbered { get; init; }
    public bool Reorders { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Option<Func<TItem, string, Fin<TItem>>> Rename { get; init; }
    public Option<Lens<TItem, bool>> Favorite { get; init; }
    public Option<Func<TItem, Option<string>>> File { get; init; }
    public Option<Func<RowScope, TItem, Action<IDisposable>, IO<Unit>>> Preview { get; init; }
    public Seq<HostEvent<Unit>> Reloads { get; init; }
    public Func<IO<Seq<TItem>>, Seq<CommandRow>> Commands { get; init; } = static _ => [];
    public Wording Wording { get; init; } = Wording.English;
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class RowItem<TItem> : TreeGridItem where TItem : notnull {
    internal RowItem(TItem row, int depth, Seq<RowItem<TItem>> kids) : base(kids.Map<ITreeGridItem>(static kid => kid)) =>
        (Row, Depth, Kids) = (row, depth, kids);

    public TItem Row { get; internal set; }
    public int Depth { get; }
    public Seq<RowItem<TItem>> Kids { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ListRows {
    // --- [LISTS]
    public static ControlRow List<TItem, TKey>(string caption, string help, ListRow<TItem, TKey> row, RowRules rules)
        where TItem : notnull
        where TKey : notnull =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => DisposalOps.Acquire(owned =>
            from grid in DisposalOps.Acquire(IO.lift(static () => new TreeGridView()), owned)
            from faced in IO.lift(() => {
                (grid.ShowHeader, grid.GridLines, grid.Border, grid.RowHeight, grid.AllowMultipleSelection, grid.AllowEmptySelection) =
                    (toSeq(row.Columns).Exists(static column => column.Caption.IsSome), GridLines.None, BorderType.None, 20, row.AllowMultipleSelection, true);
            })
            from emphasis in IO.lift(static () => SystemFonts.Bold(SystemFonts.Label().Size))
            from list in IO.lift(static () => Atom(new ListState<TItem>(Seq<RowItem<TItem>>(), [], Seq<(RowItem<TItem> Node, TItem Row)>())))
            from keep in atomIO<Func<TItem, bool>>(static _ => true)
            from columns in Columns(row, grid, scope.Sink, owned)
            from commands in Commanded(row.Pair, row.Commands(IO.lift(() => Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)).Map(static node => node.Row))), scope, owned)
            from selection in Selection(grid, list, row, scope, owned)
            from items in Items(grid, list, keep, row, selection.Shown, scope, owned)
            from expanded in row.Branches.Traverse(branches => Expansion(grid, list, row, branches, scope, owned)).As()
            from current in row.Current.Traverse(held => Watched(held.Source, held.Source.Keys(held.Field.Parameter),
                shown => IO.lift(() => Indices(row, Folded(row, list.Value.Nodes).Visible, toHashSet(shown.ToSeq())).Head.Iter(grid.ScrollToRow)),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.ScrollToRow)), owned)).As()
            let brackets = Seq((items.Keys, items.Shown), (selection.Keys, selection.Shown)) + expanded.ToSeq() + current.ToSeq()
            from gestures in Gestures(grid, list, row, emphasis, commands.Keys, scope, owned)
            from paired in Paired(grid, commands.Pair, owned)
            from content in Searched(paired, row.Search, row.Wording, kept => writeIO(keep, kept).Bind(_ => items.Shown), scope, owned)
            from listened in Listened(brackets + content.Brackets, scope, owned)
            select new RowCells(content.Control, None, None, None, [], [], new RowHelp(None, None),
                Callbacks.Each((brackets + content.Brackets).Map(static bracket => bracket.Shown)).Bind(_ => commands.Refresh),
                None, Some(ChoiceRows.Context(grid, commands.Menu, Seq(commands.Refresh), scope.Sink))), retain),
            caption, help, rules) { Wording = row.Wording };

    // --- [BROWSERS]
    public static ControlRow Browser<TItem, TKey>(string caption, string help, BrowserRow<TItem, TKey> row, RowRules rules)
        where TItem : notnull
        where TKey : notnull =>
        (Items: RowSource.Opaque(caption, nameof(BrowserRow<,>.Items), Seq<TItem>(), row.Items),
         Layout: RowSource.Fields(caption, [.. BrowserLayoutParameter.Items],
             static scope => IO.pure(scope.Settings.Map(static node => PlugInSettings.Record<BrowserLayout>(node.Child(Layouts), Applied.Live)).ToSeq()))) switch {
                 var sources => new ControlRow.Readout(RowShape.Filled, (scope, retain) => DisposalOps.Acquire(owned =>
                     from scroll in DisposalOps.Acquire(IO.lift(static () => new Scrollable()), owned)
                     from bordered in IO.lift(() => scroll.Border = BorderType.None)
                     from browser in TextRows.Live(row.Extent,
                         (extent, keep) => DisposalOps.Acquire(IO.lift(() => new ThumbnailBrowser<TItem, TKey>(scope.Sink, Source(row, scope), new BrowserState<TItem, TKey>(Seq<TItem>(), None, None, row.Mode, extent))), keep),
                         static (built, extent) => built.Extent(extent), scope, owned)
                     from placed in IO.lift(() => scroll.Content = browser)
                     from visibility in scope.OnVisibility(browser.Visibility, owned)
                     let site = new CallbackSite(scope.Sink, typeof(ThumbnailBrowser<TItem, TKey>), nameof(ThumbnailBrowser<,>.Edited))
                     from items in Items(browser, sources.Items, row, scope, site, owned)
                     from current in Current(browser, row, scope, site, owned)
                     from layout in Laid(sources.Layout, row.Mode, browser, scope, owned)
                     let brackets = Seq((items.Keys, items.Shown), (layout.Keys, layout.Shown))
                     from commands in Commanded(None, row.Commands(IO.lift(() => browser.Selection)), scope, owned)
                     from keys in Subscriptions.Attach(h => browser.KeyDown += h, h => browser.KeyDown -= h,
                         Callbacks.Handler<KeyEventArgs>(args => Keyed(commands.Keys, args.KeyData, args), new CallbackSite(scope.Sink, typeof(ThumbnailBrowser<TItem, TKey>), nameof(Control.KeyDown))), owned)
                     from reloads in Callbacks.Each(row.Reloads.Map(reload => reload.Choose(static _ => Some((unit, unit)))
                         .Through(Subscriptions.Idle<Unit, Unit>(_ => browser.Reload.Bind(_ => items.Shown).Bind(_ => current)), scope.Sink, owned)))
                     from content in Searched(scroll, row.Search, row.Wording, browser.Filter, scope, owned)
                     from listened in Listened(brackets + content.Brackets, scope, owned)
                     select new RowCells(content.Control, None, None, None, Seq(layout.Line), [], new RowHelp(None, None),
                         Callbacks.Each((brackets + content.Brackets).Map(static bracket => bracket.Shown)).Bind(_ => current).Bind(_ => commands.Refresh),
                         None, Some(ChoiceRows.Context(browser, commands.Menu, Seq(browser.TargetPointer, commands.Refresh), scope.Sink))) {
                         Availability = Some(browser.Available),
                     }, retain),
                     caption, help, rules) { Wording = row.Wording },
             };

    // --- [NODES]
    private sealed record ListState<TItem>(Seq<RowItem<TItem>> Nodes, TreeGridItemCollection Store, Seq<(RowItem<TItem> Node, TItem Row)> Reloads) where TItem : notnull;

    private static Seq<RowItem<TItem>> Built<TItem, TKey>(
        ListRow<TItem, TKey> row, Seq<TItem> items, LanguageExt.HashSet<TKey> open, Func<TItem, bool> keep, int depth)
        where TItem : notnull
        where TKey : notnull =>
        Ordered(row.Ordered, items)
            .Map(item => (Item: item, Kids: Built(row, row.Branches.ToSeq().Bind(branches => branches.Children.Get(item)), open, keep, depth + 1)))
            .Filter(pair => !pair.Kids.IsEmpty || keep(pair.Item))
            .Map(pair => new RowItem<TItem>(pair.Item, depth, pair.Kids) { Expanded = open.Contains(row.Key(pair.Item)) });

    private static (Seq<RowItem<TItem>> Preorder, Seq<RowItem<TItem>> Visible, Seq<(TKey Key, int Depth)> Outline, LanguageExt.HashSet<TKey> Open) Folded<TItem, TKey>(
        ListRow<TItem, TKey> row, Seq<RowItem<TItem>> nodes)
        where TItem : notnull
        where TKey : notnull =>
        nodes.Fold(
            (Preorder: Seq<RowItem<TItem>>(), Visible: Seq<RowItem<TItem>>(), Outline: Seq<(TKey Key, int Depth)>(), Open: LanguageExt.HashSet<TKey>.Empty),
            (held, node) => Folded(row, node.Kids) switch {
                var kids => (
                    held.Preorder.Add(node) + kids.Preorder,
                    held.Visible.Add(node) + (node.Expanded ? kids.Visible : Seq<RowItem<TItem>>()),
                    held.Outline.Add((row.Key(node.Row), node.Depth)) + kids.Outline,
                    (node.Expanded ? held.Open.Add(row.Key(node.Row)) : held.Open).Union(kids.Open)),
            });

    private static Seq<TItem> Ordered<TItem>(Option<Func<TItem, string>> ordered, Seq<TItem> items) =>
        ordered.Match(Some: name => toSeq(items.OrderBy(name, Comparer<string>.Create(Localization.LogicalSort))), None: () => items);

    private static Seq<RowItem<TItem>> Chosen<TItem>(Seq<RowItem<TItem>> visible, LanguageExt.HashSet<int> rows) where TItem : notnull =>
        visible.Choose((index, node) => Callbacks.Found(rows.Contains(index), node));

    private static Seq<int> Indices<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> visible, LanguageExt.HashSet<TKey> keys)
        where TItem : notnull
        where TKey : notnull =>
        visible.Choose((index, node) => Callbacks.Found(keys.Contains(row.Key(node.Row)), index));

    private static Option<RowItem<TItem>> Resolved<TItem, TKey>(ListRow<TItem, TKey> row, Seq<RowItem<TItem>> nodes, object item)
        where TItem : notnull
        where TKey : notnull =>
        Folded(row, nodes).Preorder.Find(node => ReferenceEquals(node, item));

    // --- [SELECTION]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Selection<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(row.Selected.Source, IterableNE.create(row.Selected.Field),
                h => grid.SelectionChanged += h, h => grid.SelectionChanged -= h,
                edit => Callbacks.Handler<EventArgs>(
                    _ => from keys in IO.lift(() => toHashSet(from node in Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)) select row.Key(node.Row)))
                         from committed in edit.Commit(keys)
                         select unit,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectionChanged))),
                agreed => Reselected(grid, list, row, agreed),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectedRows)), retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<Unit> Reselected<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Option<LanguageExt.HashSet<TKey>> agreed)
        where TItem : notnull
        where TKey : notnull =>
        from state in list.ValueIO
        from stored in IO.lift(() => ReferenceEquals(grid.DataStore, state.Store))
        from placed in unless(stored, IO.lift(fun(() => { grid.DataStore = state.Store; }))).As()
        from rows in IO.lift(() => toHashSet(grid.SelectedRows))
        from wanted in IO.lift(() => agreed.ToSeq().Bind(keys => Indices(row, Folded(row, state.Nodes).Visible, keys)))
        from selected in when(toHashSet(wanted) != rows, IO.lift(fun(() => { grid.SelectedRows = wanted; }))).As()
        select unit;

    // --- [FILL]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Items<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, Atom<Func<TItem, bool>> keep, ListRow<TItem, TKey> row, IO<Unit> reselect, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(row.Items.Source, IterableNE.create(row.Items.Field),
                h => grid.CellEdited += h, h => grid.CellEdited -= h,
                edit => Callbacks.Handler<GridViewCellEventArgs>(
                    args => from found in IO.lift(() => Resolved(row, list.Value.Nodes, args.Item))
                            from stored in scope.Read(row.Items.Source)
                            from committed in found.Match(Some: node => edit.Commit(Replaced(row.Key, row.Branches, stored, node.Row)).Map(unit), None: static () => IO.pure(unit))
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
        from stored in IO.lift(() => ReferenceEquals(grid.DataStore, next.Store))
        from reselected in unless(stored, reselect).As()
        select unit;

    private static ListState<TItem> Filled<TItem, TKey>(ListRow<TItem, TKey> row, ListState<TItem> state, Seq<TItem> items, Func<TItem, bool> keep)
        where TItem : notnull
        where TKey : notnull =>
        Folded(row, state.Nodes) switch {
            var before => Built(row, items, before.Open, keep, 0) switch {
                var after => Folded(row, after) switch {
                    var shaped when shaped.Outline == before.Outline => state with {
                        Reloads = before.Preorder.Zip(shaped.Preorder)
                            .Filter(static pair => !EqualityComparer<TItem>.Default.Equals(pair.First.Row, pair.Second.Row))
                            .Map(static pair => (pair.First, pair.Second.Row)),
                    },
                    _ => new ListState<TItem>(after, new TreeGridItemCollection(after.Map<ITreeGridItem>(static node => node)), []),
                },
            },
        };

    private static IO<Unit> Columns<TItem, TKey>(ListRow<TItem, TKey> row, TreeGridView grid, IPlugInSink sink, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        toSeq(row.Columns).Map(static (column, index) => (Column: column, Index: index)).TraverseM(entry =>
            from cell in CellFor(entry.Column, sink, retain)
            from column in DisposalOps.Acquire(IO.lift(static () => new GridColumn()), retain)
            from added in IO.lift(() => {
                (column.HeaderText, column.DataCell, column.Editable, column.Resizable, column.Expand) =
                    (Conversions.Unset(entry.Column.Caption.Map(caption => row.Wording.Shown(caption, sink))), cell, entry.Column.Edits, false,
                     entry.Column.Stretches && toSeq(row.Columns).Skip(entry.Index + 1).ForAll(static later => !later.Stretches));
                grid.Columns.Add(column);
            })
            select unit).As().Map(unit);

    // --- [EDITS]
    private static Seq<TItem> Replaced<TItem, TKey>(Func<TItem, TKey> key, Option<ListBranches<TItem, TKey>> branches, Seq<TItem> items, TItem edited)
        where TItem : notnull
        where TKey : notnull =>
        items.Map(item => EqualityComparer<TKey>.Default.Equals(key(item), key(edited))
            ? edited
            : branches.Fold(item, (held, tree) => tree.Children.Update(kids => Replaced(key, branches, kids, edited), held)));

    private static IO<Cell> CellFor<TItem>(ListColumn<TItem> column, IPlugInSink sink, Action<IDisposable> retain) where TItem : notnull =>
        IconSlot.ListCell.Size().Bind(size => column.Switch((Sink: sink, Retain: retain, Size: new Size(size, size)),
            label: static (held, label) => label.Icon.Match(
                Some: icon =>
                    from cell in DisposalOps.Acquire(IO.lift(static () => new ImageTextCell()), held.Retain)
                    from bound in IO.lift(() => { (cell.ImageBinding, cell.TextBinding) = (Pictured(icon, held.Size), Written(label)); })
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
                from bound in IO.lift(() => cell.Binding = Pictured(glyph.Icon, held.Size))
                select (Cell)cell,
            progress: static (held, progress) =>
                from cell in DisposalOps.Acquire(IO.lift(static () => new ProgressCell()), held.Retain)
                from bound in IO.lift(() => cell.Binding = Binding.Delegate<RowItem<TItem>, float?>(node => progress.Read(node.Row).ToNullable()))
                select (Cell)cell));

    private static IndirectBinding<Image> Pictured<TItem>(Func<TItem, IGlyph> icon, Size logicalSize) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, Image>(node => Icons.Frame(icon(node.Row), logicalSize));

    private static IndirectBinding<string> Written<TItem>(ListColumn<TItem>.Label label) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, string>(
            node => label.Read(node.Row),
            (node, value) => _ = label.Rename.Bind(rename => rename(node.Row, value).ToOption()).Iter(item => node.Row = item));

    // --- [EXPANSION]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Expansion<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, ListBranches<TItem, TKey> branches, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(branches.Expanded.Source, IterableNE.create(branches.Expanded.Field),
                h => Callbacks.Each(Seq(IO.lift(() => grid.Expanded += h), IO.lift(() => grid.Collapsed += h))).Run(),
                h => Callbacks.Each(Seq(IO.lift(() => grid.Expanded -= h), IO.lift(() => grid.Collapsed -= h))).Run(),
                edit => Callbacks.Handler<TreeGridViewItemEventArgs>(
                    _ => from state in list.ValueIO
                         from committed in edit.Commit(Folded(row, state.Nodes).Open)
                         select unit,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Expanded))),
                open => Expand(grid, list, row, open),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.ReloadItem)), retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<Unit> Expand<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Option<LanguageExt.HashSet<TKey>> open)
        where TItem : notnull
        where TKey : notnull =>
        from state in list.ValueIO
        from toggled in Folded(row, state.Nodes).Preorder
            .Filter(node => node.Expanded != open.Exists(keys => keys.Contains(row.Key(node.Row))))
            .TraverseM(node => IO.lift(fun(() => {
                node.Expanded = !node.Expanded;
                grid.ReloadItem(node, reloadChildren: true);
            })))
            .As()
        select unit;

    // --- [GESTURES]
    private static IO<Unit> Gestures<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Font emphasis, Seq<(Command Command, Action Run)> keys, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        Callbacks.Each(Seq(
            Subscriptions.Attach(h => grid.KeyDown += h, h => grid.KeyDown -= h,
                Callbacks.Handler<KeyEventArgs>(args => Pressed(grid, list, row, keys, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.KeyDown))), retain),
            Subscriptions.Attach(h => grid.MouseDown += h, h => grid.MouseDown -= h,
                Callbacks.Handler<MouseEventArgs>(args => Pointed(grid, list, row, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.MouseDown))), retain),
            Subscriptions.Attach(h => grid.CellFormatting += h, h => grid.CellFormatting -= h,
                Callbacks.Handler<GridCellFormatEventArgs>(args => Formatted(list, row, emphasis, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellFormatting))), retain),
            HostTheme.In(grid).Inline(_ => IO.lift(grid.ReloadData), scope.Sink, retain))
            + row.Activate.Map(activate => Subscriptions.Attach(h => grid.Activated += h, h => grid.Activated -= h,
                Callbacks.Handler<TreeGridViewItemEventArgs>(_ =>
                    from head in IO.lift(() => Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)).Head)
                    from ran in head.Match(Some: node => activate(node.Row), None: static () => IO.pure(unit))
                    select ran, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Activated))), retain)).ToSeq()).Map(unit);

    private static IO<Unit> Pressed<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Seq<(Command Command, Action Run)> keys, KeyEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        from rows in IO.lift(() => toSeq(grid.SelectedRows).Strict())
        from shown in IO.lift(() => !Folded(row, list.Value.Nodes).Visible.IsEmpty)
        from handled in args.KeyData switch {
            Keys.F2 => Handled(args,
                from at in rows.Head
                from renaming in toSeq(row.Columns).Map(static (each, index) => (Column: each, Index: index)).Find(static entry => entry.Column.Renames)
                select (Action)(() => grid.BeginEdit(at, renaming.Index))),
            Keys.Up or Keys.Down when rows.IsEmpty && shown => Handled(args, Some(() => grid.SelectRow(0))),
            var key => Keyed(keys, key, args),
        }
        select handled;

    private static IO<Unit> Keyed(Seq<(Command Command, Action Run)> keys, Keys key, KeyEventArgs args) =>
        Handled(args, keys.Find(pair => pair.Command.Enabled && pair.Command.Shortcut == key).Map(static pair => pair.Run));

    private static IO<Unit> Handled(KeyEventArgs args, Option<Action> action) =>
        IO.lift(fun(() => {
            _ = action.Iter(static run => run());
            args.Handled = action.IsSome;
        }));

    private static IO<Unit> Pointed<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, MouseEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        when(args.Buttons is MouseButtons.Alternate,
            from pressed in IO.lift(() => (grid.GetCellAt(args.Location).Item, Rows: toHashSet(grid.SelectedRows)))
            from found in IO.lift(() => Folded(row, list.Value.Nodes).Visible
                .Choose((index, node) => Callbacks.Found(ReferenceEquals(node, pressed.Item) && !pressed.Rows.Contains(index), index))
                .Head)
            from selected in IO.lift(() => found.Iter(at => grid.SelectedRows = [at]))
            select selected).As();

    private static IO<Unit> Formatted<TItem, TKey>(Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Font emphasis, GridCellFormatEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        IO.lift(() => Resolved(row, list.Value.Nodes, args.Item).Iter(node =>
            (args.Font, args.ForegroundColor) = (
                node.Kids.IsEmpty ? args.Font : emphasis,
                row.Available.ForAll(available => available(node.Row)) ? args.ForegroundColor : PaintSlot.TextDisabled.Read())));

    // --- [COMMANDS]
    private static IO<(Option<Control> Pair, Seq<Seq<MenuEntry>> Menu, Seq<(Command Command, Action Run)> Keys, IO<Unit> Refresh)> Commanded(
        Option<(CommandRow.Run Add, CommandRow.Run Remove)> pair, Seq<CommandRow> commands, RowScope scope, Action<IDisposable> retain) =>
        from paired in pair.Traverse(held =>
            from add in held.Add.Realize(scope, retain)
            from remove in held.Remove.Realize(scope, retain)
            from placed in ButtonRows.AddRemove(add.Commands, remove.Commands, retain)
            select (Control: (Control)placed,
                Keys: Seq((Command: add.Commands, Run: add.Commands.Execute), (Command: remove.Commands, Run: (Action)remove.Commands.Execute)),
                Refresh: Callbacks.Each(Seq(add.Refresh, remove.Refresh)).Map(unit))).As()
        from realized in commands.TraverseM(command => command.Commands(scope, retain)).As()
        let menu = CommandRow.Menu(Seq(realized))
        select (paired.Map(static held => held.Control),
            (paired.Map(static held => held.Keys.Map(static key => key.Command)).ToSeq() + menu.Groups).Map(static cluster => cluster.Map<MenuEntry>(static command => new MenuEntry.Item(command))),
            paired.ToSeq().Bind(static held => held.Keys) + realized.Bind(static held => held.Commands).Map(static member => (member.Command, Run: Invoked(member.Command))),
            Callbacks.Each(paired.Map(static held => held.Refresh).ToSeq().Add(menu.Refresh)).Map(unit));

    private static Action Invoked(Command command) =>
        command switch {
            RadioCommand radio => () => {
                radio.Checked = true;
                radio.Execute();
            }
            ,
            CheckCommand check => () => {
                check.Checked = !check.Checked;
                check.Execute();
            }
            ,
            _ => command.Execute,
        };

    // --- [PLACEMENT]
    private static IO<Control> Paired(Control content, Option<Control> pair, Action<IDisposable> retain) =>
        pair.Match(Some: buttons =>
            from stack in DisposalOps.Acquire(IO.lift(static () => new StackLayout()), retain)
            from added in IO.lift(() => {
                (stack.HorizontalContentAlignment, stack.Spacing) = (HorizontalAlignment.Stretch, RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Panel));
                stack.Items.Add(new StackLayoutItem(content, expand: true));
                stack.Items.Add(new StackLayoutItem(buttons, HorizontalAlignment.Left));
            })
            select (Control)stack,
            None: () => IO.pure(content));

    private static IO<(Control Control, Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)> Brackets)> Searched<TItem>(
        Control content, Option<ListSearch<TItem>> search, Wording wording, Func<Func<TItem, bool>, IO<Unit>> keep, RowScope scope, Action<IDisposable> retain) where TItem : notnull =>
        search.Match(Some: held =>
            from field in ChoiceRows.Search(wording.Shown(held.Placeholder, scope.Sink), matches => keep(item => matches(held.Text(scope, item))), scope.Sink, retain)
            from shown in held.Shown.Traverse(source => Watched(source, Named(source, scope), visible => IO.lift(() => { field.Visible = visible; }), scope,
                new CallbackSite(scope.Sink, typeof(SearchBox), nameof(SearchBox.Visible)), retain)).As()
            from stack in DisposalOps.Acquire(IO.lift(static () => new StackLayout()), retain)
            from placed in IO.lift(() => {
                (stack.HorizontalContentAlignment, stack.Spacing) = (HorizontalAlignment.Stretch, RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Panel));
                stack.Items.Add(field);
                stack.Items.Add(new StackLayoutItem(content, expand: true));
            })
            select ((Control)stack, shown.ToSeq()),
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

    // --- [LAYOUT]
    private const string Layouts = "thumbnail-browser";

    private static IO<(RowLine Line, Seq<EntryKey> Keys, IO<Unit> Shown)> Laid<TItem, TKey>(
        RowSource<BrowserLayout> layout, BrowserMode mode, ThumbnailBrowser<TItem, TKey> browser, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        from slider in DisposalOps.Acquire(IO.lift(() => new ParameterSlider<TileSize, float>(
            scope.Sink, NumberText.Scalar(NumericDomain.Bounded<TileSize, float>(TileSize.Presentation), ""), RowText.Localize(BrowserLayoutParameter.Size.Text.Caption).Local)), retain)
        let keys = Named(layout, scope)
        let written = (Func<Func<BrowserLayout, BrowserLayout>, IO<Unit>>)(next =>
            from held in scope.Read(layout)
            from stored in scope.Store(layout).Write(Some(next(held)))
            from changed in scope.Changed(keys)
            select unit)
        from modes in ChoiceRows.Strip(ChoiceRows.Items(Some<Func<BrowserMode, IGlyph>>(static item => item.Glyph), scope.Sink),
            picked => written(held => held with { Mode = Some(picked) }), scope.Sink, retain)
        let moved = Callbacks.Handler<EditEventArgs<TileSize>>(args => args.Edit.Carried.Traverse(size => written(held => held with { Size = Some(size) })).As().Map(unit),
            new CallbackSite(scope.Sink, typeof(ParameterSlider<TileSize, float>), nameof(ParameterSlider<,>.Edited)))
        let widths = IO.lift(() => browser.ImageWidth).Bind(width => slider.Receive(Some(TileSize.Clamped(width))))
        from watched in Watched(layout, keys, held =>
            from sized in browser.TileWidth(held.Size)
            from moded in browser.Mode(held.Mode.IfNone(mode))
            from picked in modes.Show(Some(held.Mode.IfNone(mode)))
            from fitted in widths
            select unit,
            scope, new CallbackSite(scope.Sink, typeof(ThumbnailBrowser<TItem, TKey>), nameof(ThumbnailBrowser<,>.Mode)), retain)
        from movedRoute in Subscriptions.Attach(h => slider.Edited += h, h => slider.Edited -= h, moved, retain)
        from sizedRoute in Subscriptions.Attach(h => browser.SizeChanged += h, h => browser.SizeChanged -= h,
            Callbacks.Handler<EventArgs>(_ => widths, new CallbackSite(scope.Sink, typeof(ThumbnailBrowser<TItem, TKey>), nameof(Control.SizeChanged))), retain)
        from table in DisposalOps.Acquire(IO.lift(static () => new TableLayout()), retain)
        from placed in IO.lift(() => {
            table.Rows.Add(new TableRow(new TableCell(slider, scaleWidth: true), modes.Control));
            table.Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table);
        })
        select (new RowLine(None, table, None, None), keys, watched.Shown);

    // --- [ROUTES]
    private static BrowserSource<TItem, TKey> Source<TItem, TKey>(BrowserRow<TItem, TKey> row, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        new(row.Key, item => row.Label(scope, item), (view, item, size, retain) => row.Tile(scope, view, item, size, retain), row.Wording.Shown(row.Empty, scope.Sink)) {
            Numbered = row.Numbered,
            Favorite = row.Favorite.Map(static lens => lens.Get),
            Preview = row.Preview.Map(preview => (Func<TItem, Action<IDisposable>, IO<Unit>>)((item, retain) => preview(scope, item, retain))),
            Export = row.File.Map(static file => (Func<Control, Seq<TItem>, Option<Thumbnail>, IO<Unit>>)(
                (view, items, thumbnail) => Transfer.Drag(view, items.Choose(file), thumbnail.Bind(static held => held.Drag)))),
            Renames = row.Rename.IsSome,
            Reorders = row.Reorders,
        };

    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown)> Items<TItem, TKey>(
        ThumbnailBrowser<TItem, TKey> browser, (RowSource<Seq<TItem>> Source, RowField<Seq<TItem>> Field) items, BrowserRow<TItem, TKey> row, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(items.Source, IterableNE.create(items.Field),
                h => browser.Edited += h, h => browser.Edited -= h,
                edit => Callbacks.Handler<EditEventArgs<BrowserEdit<TItem, TKey>>>(args => ToItems(row, args.Edit, edit), site),
                shown => browser.Items(shown.ToSeq().Flatten()), scope, site, retain)
            .Map(static bound => (bound.Keys, bound.Shown));

    private static IO<IO<Unit>> Current<TItem, TKey>(
        ThumbnailBrowser<TItem, TKey> browser, BrowserRow<TItem, TKey> row, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TItem : notnull
        where TKey : notnull =>
        from pending in IO.lift(static () => Ref(Option<TKey>.None))
        from timer in DisposalOps.Acquire(IO.lift(static () => new UITimer()), retain)
        let shown = row.Current(scope).Bind(value => browser.Receive(value.Map<BrowserEdit<TItem, TKey>>(static key => new BrowserEdit<TItem, TKey>.Applied(key))))
        let settled = Settled(row, scope, timer, pending).Bind(_ => shown)
        let applied = (Func<BrowserEdit<TItem, TKey>, Option<TKey>>)(static value => value.Switch(
            applied: static held => Some(held.Key), reordered: static _ => None, renamed: static _ => None, favorited: static _ => None))
        let stepped = (Func<BrowserEdit<TItem, TKey>, IO<Unit>>)(value => applied(value).Match(
            Some: key =>
                from delay in Accessors.StepEventsDelay
                from queued in atomicIO(() => pending.Value = Some(key))
                from started in IO.lift(() => { timer.Interval = delay; timer.Start(); })
                select unit,
            None: static () => IO.pure(unit)))
        from closing in DisposalOps.Acquire(IO.lift(() => new Disposal<Unit>(unit, _ => settled.Run())), retain)
        from elapsed in Subscriptions.Attach(h => timer.Elapsed += h, h => timer.Elapsed -= h, Callbacks.Handler<EventArgs>(_ => settled, site), retain)
        from edited in Subscriptions.Attach(h => browser.Edited += h, h => browser.Edited -= h,
            Callbacks.Handler<EditEventArgs<BrowserEdit<TItem, TKey>>>(args => args.Edit.Switch(
                (Stepped: stepped, Timer: timer, Pending: pending, Shown: shown, Applied: applied, Settled: settled),
                preview: static (held, preview) => held.Stepped(preview.Value),
                step: static (held, step) => held.Stepped(step.Value),
                commit: static (held, committed) => held.Applied(committed.Value).Match(
                    Some: key => atomicIO(() => held.Pending.Value = Some(key)).Bind(_ => held.Settled), None: static () => IO.pure(unit)),
                cancel: static (held, _) => IO.lift(held.Timer.Stop).Bind(_ => atomicIO(() => held.Pending.Value = None)).Bind(_ => held.Shown)), site), retain)
        from changed in scope.Listen(_ => shown, retain)
        select shown;

    private static IO<Unit> Settled<TItem, TKey>(BrowserRow<TItem, TKey> row, RowScope scope, UITimer timer, Ref<Option<TKey>> pending)
        where TItem : notnull
        where TKey : notnull =>
        DisposalOps.Transacted(
            IO.lift(timer.Stop).Bind(_ => pending.ValueIO),
            held => held.Traverse(key => row.Choose(scope, key)).As(),
            static _ => unitIO,
            (held, _) => when(held.IsSome, atomicIO(() => pending.Value = None).Map(unit)).As());

    private static Option<Edit<TOut>> Mapped<TIn, TOut>(Edit<TIn> change, Func<TIn, Option<TOut>> map)
        where TIn : notnull
        where TOut : notnull =>
        change.Switch(
            map,
            preview: static (into, preview) => into(preview.Value).Map<Edit<TOut>>(static value => new Edit<TOut>.Preview(value)),
            step: static (into, step) => into(step.Value).Map<Edit<TOut>>(static value => new Edit<TOut>.Step(value)),
            commit: static (into, commit) => into(commit.Value).Map<Edit<TOut>>(static value => new Edit<TOut>.Commit(value)),
            cancel: static (_, _) => Some<Edit<TOut>>(new Edit<TOut>.Cancel()));

    private static IO<Unit> ToItems<TItem, TKey>(BrowserRow<TItem, TKey> row, Edit<BrowserEdit<TItem, TKey>> change, RowEdit<Seq<TItem>> edit)
        where TItem : notnull
        where TKey : notnull =>
        Mapped(change, value => Into(row, value))
            .Match(Some: mapped => edit.Take(mapped, static (into, latest) => into(latest)).Map(unit), None: static () => IO.pure(unit));

    private static Option<Func<Seq<TItem>, Fin<Seq<TItem>>>> Into<TItem, TKey>(BrowserRow<TItem, TKey> row, BrowserEdit<TItem, TKey> value)
        where TItem : notnull
        where TKey : notnull =>
        value.Switch(
            row,
            applied: static (_, _) => Option<Func<Seq<TItem>, Fin<Seq<TItem>>>>.None,
            reordered: static (held, reordered) => Some<Func<Seq<TItem>, Fin<Seq<TItem>>>>(latest => toSeq(
                from key in reordered.Keys
                from item in latest
                where EqualityComparer<TKey>.Default.Equals(held.Key(item), key)
                select item)),
            renamed: static (held, renamed) => held.Rename.Map<Func<Seq<TItem>, Fin<Seq<TItem>>>>(rename => latest =>
                from item in rename(renamed.Item, renamed.Name)
                select Replaced(held.Key, None, latest, item)),
            favorited: static (held, favorited) => held.Favorite
                .Map(lens => lens.Update(static on => !on, favorited.Item))
                .Map<Func<Seq<TItem>, Fin<Seq<TItem>>>>(toggled => latest => Replaced(held.Key, None, latest, toggled)));
}
