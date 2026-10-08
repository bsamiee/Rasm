using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CellGlyph<TItem>(Seq<IGlyph> Glyphs, Func<TItem, IGlyph> Read) where TItem : notnull {
    public static CellGlyph<TItem> Of<TGlyph, TError>(Func<TItem, TGlyph> read)
        where TGlyph : class, IGlyph, ISmartEnum<string, TGlyph, TError>
        where TError : Error, IValidationError<TError> =>
        new(toSeq(TGlyph.Items).Map(static glyph => (IGlyph)glyph), item => read(item));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ListColumn<TItem> where TItem : notnull {
    private ListColumn(Option<string> caption) => Caption = caption;

    public Option<string> Caption { get; }

    public bool Renames => Switch(text: static text => text.Rename.IsSome, check: static _ => false, glyph: static _ => false, progress: static _ => false);

    public bool Edits => Switch(text: static text => text.Rename.IsSome, check: static _ => true, glyph: static _ => false, progress: static _ => false);

    public bool Stretches => Map(text: true, check: false, glyph: false, progress: true);

    public Seq<IGlyph> Glyphs =>
        Switch(
            text: static text => text.Icon.ToSeq().Bind(static icon => icon.Glyphs),
            check: static _ => Seq<IGlyph>(),
            glyph: static glyph => glyph.Icon.Glyphs,
            progress: static _ => Seq<IGlyph>());

    public static ListColumn<TItem> Named<TValue, TRaw, TError>(Option<string> caption, Lens<TItem, TValue> name, Option<CellGlyph<TItem>> icon)
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        new Text(
            caption,
            item => StoredText.Capture<TValue, TRaw>(name.Get(item)),
            icon,
            Some<Func<TItem, string, Fin<TItem>>>((item, text) => StoredText.Recall<TValue, TRaw, TError>(text).Map(value => name.Set(value, item))));

    public static ListColumn<TItem> Fraction<TValue, TKey>(Option<string> caption, Func<TItem, Option<TValue>> read)
        where TValue : System.Numerics.IMinMaxValue<TValue>, IConvertible<TKey>
        where TKey : System.Numerics.INumber<TKey> =>
        new Progress(caption, item => read(item).Map(static value => float.CreateChecked(
            (double.CreateChecked(value.ToValue()) - double.CreateChecked(TValue.MinValue.ToValue()))
            / (double.CreateChecked(TValue.MaxValue.ToValue()) - double.CreateChecked(TValue.MinValue.ToValue())))));

    public sealed record Text(Option<string> Caption, Func<TItem, string> Read, Option<CellGlyph<TItem>> Icon, Option<Func<TItem, string, Fin<TItem>>> Rename)
        : ListColumn<TItem>(Caption);

    public sealed record Check(Option<string> Caption, Lens<TItem, bool> Value) : ListColumn<TItem>(Caption);

    public sealed record Glyph(Option<string> Caption, CellGlyph<TItem> Icon) : ListColumn<TItem>(Caption);

    public sealed record Progress(Option<string> Caption, Func<TItem, Option<float>> Read) : ListColumn<TItem>(Caption);
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

public sealed record BrowserLayout(Option<TileSize> Size, Option<BrowserMode> Mode) : IStateRecord<BrowserLayout, BrowserLayoutParameter, InvalidRhinoValue> {
    public static BrowserLayout Default { get; } = new(None, None);
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BrowserLayoutParameter : IStateParameter<BrowserLayout> {
    public static readonly BrowserLayoutParameter Size = new("tile-size", new StateParameter<BrowserLayout>.OptionalKeyed<TileSize, float, InvalidRhinoValue>(
        Lens<BrowserLayout, Option<TileSize>>.New(static layout => layout.Size, static size => layout => layout with { Size = size })));
    public static readonly BrowserLayoutParameter Mode = new("mode", new StateParameter<BrowserLayout>.OptionalChoice<BrowserMode, InvalidRhinoValue>(
        Lens<BrowserLayout, Option<BrowserMode>>.New(static layout => layout.Mode, static mode => layout => layout with { Mode = mode })));

    public StateParameter<BrowserLayout> Kind { get; }
}

public sealed record BrowserRow<TItem, TKey>(
    Func<TItem, TKey> Key,
    Func<RowScope, TItem, string> Label,
    Func<RowScope, Control, TItem, Size, IO<Disposal<Thumbnail>>> Tile,
    Func<RowScope, ValueStore<Seq<TItem>>> Items,
    Func<RowScope, ValueStore<TKey>> Current,
    BrowserMode Mode,
    ReadoutSource<PixelExtent> Extent,
    string Empty)
    where TItem : notnull
    where TKey : notnull {
    public bool Numbered { get; init; }
    public Option<Func<TItem, string>> Ordered { get; init; }
    public Option<ListSearch<TItem>> Search { get; init; }
    public Option<Func<TItem, string, Fin<TItem>>> Rename { get; init; }
    public Option<Lens<TItem, bool>> Favourite { get; init; }
    public Option<Func<TItem, string>> File { get; init; }
    public Option<Func<RowScope, Option<TItem>, IO<Unit>>> Preview { get; init; }
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
        new ControlRow.Readout(RowShape.Filled, scope =>
            from grid in IO.lift(() => new TreeGridView {
                ShowHeader = toSeq(row.Columns).Exists(static column => column.Caption.IsSome),
                GridLines = GridLines.None,
                Border = BorderType.None,
                RowHeight = 20,
                AllowMultipleSelection = row.AllowMultipleSelection,
                AllowEmptySelection = true,
            })
            let disposed = IO.lift(fun(grid.Dispose))
            from list in IO.lift(static () => Atom(new ListState<TItem>(Seq<RowItem<TItem>>(), [], Seq<(RowItem<TItem> Node, TItem Row)>())))
            from keep in IO.lift(static () => Atom<Func<TItem, bool>>(static _ => true))
            from glyphs in DisposalOps.OnFailure(Icons.Check(scope.Sink, toSeq(row.Columns).Bind(static column => column.Glyphs).Distinct(), IconSlot.ListCell), disposed)
            from columns in DisposalOps.OnFailure(IO.lift(() => Columns(row, scope.Sink).Iter(grid.Columns.Add)), disposed)
            from commands in DisposalOps.OnFailure(
                Commanded(row.Pair, row.Commands(IO.lift(() => Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)).Map(static node => node.Row))), scope),
                disposed)
            let failed = Seq<IDisposable>(grid)
            from selection in DisposalOps.OnFailure(Selection(grid, list, row, scope), DisposalOps.Release(failed.Add(commands.Release)))
            from items in DisposalOps.OnFailure(Items(grid, list, keep, row, selection.Shown, scope), DisposalOps.Release(failed + Seq(commands.Release, selection.Release)))
            let bound = Seq(commands.Release, selection.Release, items.Release)
            from parts in DisposalOps.OnFailure(
                DisposalOps.AcquireAll(
                    row.Branches.Map(branches => Expansion(grid, list, row, branches, scope)).ToSeq()
                    + row.Current.Map(current => Watched(
                        current.Source, current.Source.Keys(current.Field.Parameter),
                        shown => IO.lift(() => Indices(row, Folded(row, list.Value.Nodes).Visible, toHashSet(shown.ToSeq())).Head.Iter(grid.ScrollToRow)),
                        scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.ScrollToRow)))).ToSeq(),
                    static held => DisposalOps.Release(held.Map(static part => part.Release))),
                DisposalOps.Release(failed + bound))
            let brackets = Seq((items.Keys, items.Shown), (selection.Keys, selection.Shown)) + parts.Map(static part => (part.Keys, part.Shown))
            from gestures in DisposalOps.OnFailure(Gestures(grid, list, row, commands.Keys, scope), DisposalOps.Release(failed + bound + parts.Map(static part => part.Release)))
            let attached = bound + parts.Map(static part => part.Release) + Seq(gestures)
            from content in DisposalOps.OnFailure(
                Searched(Paired(grid, commands.Pair), row.Search, row.Wording, kept => keep.SwapIO(_ => kept).Bind(_ => items.Shown), scope),
                DisposalOps.Release(failed + attached))
            from listened in DisposalOps.OnFailure(Listened(brackets + content.Brackets, scope), DisposalOps.Release(failed + attached + content.Held))
            select new RowCells(
                content.Control, None, None, None, [], [], new RowHelp(None, None),
                (brackets + content.Brackets).TraverseM(static bracket => bracket.Shown).As().Bind(_ => commands.Refresh),
                None,
                Some(ChoiceRows.Context(grid, commands.Menu, Seq(commands.Refresh), scope.Sink)),
                DisposalOps.Composite(attached + content.Held + Seq(listened), new CallbackSite(scope.Sink, typeof(ListRows), nameof(List)))),
            caption, help, rules) { Wording = row.Wording };

    // --- [BROWSERS]
    public static ControlRow Browser<TItem, TKey>(string caption, string help, BrowserRow<TItem, TKey> row, RowRules rules)
        where TItem : notnull
        where TKey : notnull =>
        (Items: RowSource.Opaque(caption, nameof(BrowserRow<,>.Items), Seq<TItem>(), row.Items),
         Current: RowSource.Opaque(caption, nameof(BrowserRow<,>.Current), Option<TKey>.None, scope => RowSource.Absent(row.Current(scope))),
         Layout: RowSource.Fields(
             caption, toSeq(BrowserLayoutParameter.Items).Map(item => ((IStateParameter<BrowserLayout>)item, caption)),
             static scope => IO.pure(scope.Settings.Map(static node => PlugInSettings.Record<BrowserLayout>(node.Child(Layouts), Applied.Live)).ToSeq()))) switch {
                 var sources => new ControlRow.Readout(RowShape.Filled, scope =>
                     from scroll in IO.lift(static () => new Scrollable { Border = BorderType.None })
                     from live in DisposalOps.OnFailure(
                         TextRows.Live(
                             row.Extent,
                             extent => IO.lift(() => new ThumbnailBrowser<TItem, TKey>(
                                 scope.Sink, Source(row, scope), new BrowserState<TItem, TKey>(Seq<TItem>(), None, None, row.Mode, extent))),
                             static (built, extent) => built.Extent(extent),
                             scope),
                         IO.lift(fun(scroll.Dispose)))
                     let browser = live.Control
                     let failed = Seq<IDisposable>(scroll, browser)
                     from placed in DisposalOps.OnFailure(IO.lift(() => scroll.Content = browser), DisposalOps.Release(failed.Add(live.Release)))
                     let site = new CallbackSite(scope.Sink, typeof(ThumbnailBrowser<TItem, TKey>), nameof(ThumbnailBrowser<,>.Edited))
                     from items in DisposalOps.OnFailure(Items(browser, sources.Items, row, scope, site), DisposalOps.Release(failed.Add(live.Release)))
                     from current in DisposalOps.OnFailure(Current(browser, sources.Current, scope, site), DisposalOps.Release(failed + Seq(live.Release, items.Release)))
                     from layout in DisposalOps.OnFailure(
                         Laid(sources.Layout, row.Mode, browser, scope),
                         DisposalOps.Release(failed + Seq(live.Release, items.Release, current.Release)))
                     let brackets = Seq((items.Keys, items.Shown), (current.Keys, current.Shown), (layout.Keys, layout.Shown))
                     let bound = Seq(live.Release, items.Release, current.Release, layout.Release)
                     from commands in DisposalOps.OnFailure(Commanded(None, row.Commands(IO.lift(() => browser.Selection)), scope), DisposalOps.Release(failed + bound))
                     from routes in DisposalOps.OnFailure(
                         DisposalOps.AcquireAll(
                             Seq(Subscriptions.Attach(h => browser.KeyDown += h, h => browser.KeyDown -= h,
                                     Callbacks.Handler<KeyEventArgs>(args => Keyed(commands.Keys, args.KeyData, args),
                                         new CallbackSite(scope.Sink, typeof(ThumbnailBrowser<TItem, TKey>), nameof(Control.KeyDown)))))
                             + row.Reloads.Map(reload => reload.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ => browser.Reload), scope.Sink)),
                             DisposalOps.Release),
                         DisposalOps.Release(failed + bound.Add(commands.Release)))
                     let attached = bound.Add(commands.Release) + routes
                     from content in DisposalOps.OnFailure(Searched(scroll, row.Search, row.Wording, browser.Filter, scope), DisposalOps.Release(failed + attached))
                     from listened in DisposalOps.OnFailure(Listened(brackets + content.Brackets, scope), DisposalOps.Release(failed + attached + content.Held))
                     select new RowCells(
                         content.Control, None, None, None, Seq(layout.Line), [], new RowHelp(None, None),
                         (brackets + content.Brackets).TraverseM(static bracket => bracket.Shown).As().Bind(_ => commands.Refresh),
                         None,
                         Some(ChoiceRows.Context(browser, commands.Menu, Seq(browser.TargetPointer, commands.Refresh), scope.Sink)),
                         DisposalOps.Composite(attached + content.Held + Seq(listened), new CallbackSite(scope.Sink, typeof(ListRows), nameof(Browser)))),
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
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown, IDisposable Release)> Selection<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(row.Selected.Source, IterableNE.create(row.Selected.Field),
                h => grid.SelectionChanged += h, h => grid.SelectionChanged -= h,
                edit => Callbacks.Handler<EventArgs>(
                    _ => from keys in IO.lift(() => toHashSet(from node in Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)) select row.Key(node.Row)))
                         from committed in edit.Commit(keys)
                         select committed,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectionChanged))),
                agreed => Reselected(grid, list, row, agreed),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.SelectedRows)))
            .Map(static bound => (bound.Edit.Keys, bound.Edit.Shown, bound.Release));

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
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown, IDisposable Release)> Items<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, Atom<Func<TItem, bool>> keep, ListRow<TItem, TKey> row, IO<Unit> reselect, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(row.Items.Source, IterableNE.create(row.Items.Field),
                h => grid.CellEdited += h, h => grid.CellEdited -= h,
                edit => Callbacks.Handler<GridViewCellEventArgs>(
                    args => from found in IO.lift(() => Resolved(row, list.Value.Nodes, args.Item))
                            from stored in scope.Read(row.Items.Source)
                            from committed in found.Match(Some: node => edit.Commit(Replaced(row.Key, row.Branches, stored, node.Row)), None: static () => IO.pure(unit))
                            select committed,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellEdited))),
                agreed => Fill(grid, list, keep, row, reselect, agreed.ToSeq().Flatten()),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.DataStore)))
            .Map(static bound => (bound.Edit.Keys, bound.Edit.Shown, bound.Release));

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

    private static Seq<GridColumn> Columns<TItem, TKey>(ListRow<TItem, TKey> row, IPlugInSink sink)
        where TItem : notnull
        where TKey : notnull =>
        toSeq(row.Columns).Map((column, index) => new GridColumn {
            HeaderText = Conversions.Unset(column.Caption.Map(caption => row.Wording.Shown(caption, sink))),
            DataCell = CellFor(column, sink),
            Editable = column.Edits,
            Resizable = false,
            Expand = column.Stretches && toSeq(row.Columns).Skip(index + 1).ForAll(static later => !later.Stretches),
        });

    // --- [EDITS]
    private static Seq<TItem> Replaced<TItem, TKey>(Func<TItem, TKey> key, Option<ListBranches<TItem, TKey>> branches, Seq<TItem> items, TItem edited)
        where TItem : notnull
        where TKey : notnull =>
        items.Map(item => EqualityComparer<TKey>.Default.Equals(key(item), key(edited))
            ? edited
            : branches.Fold(item, (held, tree) => tree.Children.Update(kids => Replaced(key, branches, kids, edited), held)));

    private static Cell CellFor<TItem>(ListColumn<TItem> column, IPlugInSink sink) where TItem : notnull =>
        column.Switch(
            sink,
            text: static (held, text) => text.Icon.Match<Cell>(
                Some: icon => new ImageTextCell { ImageBinding = Pictured(icon, held), TextBinding = Written(text) },
                None: () => new TextBoxCell { Binding = Written(text) }),
            check: static (_, check) => new CheckBoxCell {
                Binding = Binding.Delegate<RowItem<TItem>, bool?>(
                    node => check.Value.Get(node.Row),
                    (node, value) => node.Row = check.Value.Set(value is true, node.Row)),
            },
            glyph: static (held, glyph) => new ImageViewCell { Binding = Pictured(glyph.Icon, held) },
            progress: static (_, progress) => new ProgressCell {
                Binding = Binding.Delegate<RowItem<TItem>, float?>(node => progress.Read(node.Row).ToNullable()),
            });

    private static IndirectBinding<Image> Pictured<TItem>(CellGlyph<TItem> icon, IPlugInSink sink) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, Image>(node => Icons.Frame(sink, icon.Read(node.Row), IconSlot.ListCell));

    private static IndirectBinding<string> Written<TItem>(ListColumn<TItem>.Text text) where TItem : notnull =>
        Binding.Delegate<RowItem<TItem>, string>(
            node => text.Read(node.Row),
            (node, value) => _ = text.Rename.Bind(rename => rename(node.Row, value).ToOption()).Iter(item => node.Row = item));

    // --- [EXPANSION]
    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown, IDisposable Release)> Expansion<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, ListBranches<TItem, TKey> branches, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(branches.Expanded.Source, IterableNE.create(branches.Expanded.Field),
                h => {
                    grid.Expanded += h;
                    grid.Collapsed += h;
                },
                h => {
                    grid.Expanded -= h;
                    grid.Collapsed -= h;
                },
                edit => Callbacks.Handler<TreeGridViewItemEventArgs>(
                    _ => from state in list.ValueIO
                         from committed in edit.Commit(Folded(row, state.Nodes).Open)
                         select committed,
                    new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Expanded))),
                open => Expand(grid, list, row, open),
                scope, new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.ReloadItem)))
            .Map(static bound => (bound.Edit.Keys, bound.Edit.Shown, bound.Release));

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
    private static IO<IDisposable> Gestures<TItem, TKey>(
        TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Seq<(Command Command, Action Run)> keys, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        DisposalOps.AcquireAll(
                Seq(Subscriptions.Attach(h => grid.KeyDown += h, h => grid.KeyDown -= h,
                        Callbacks.Handler<KeyEventArgs>(args => Pressed(grid, list, row, keys, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.KeyDown)))),
                    Subscriptions.Attach(h => grid.MouseDown += h, h => grid.MouseDown -= h,
                        Callbacks.Handler<MouseEventArgs>(args => Pointed(grid, list, row, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(Control.MouseDown)))),
                    Subscriptions.Attach(h => grid.CellFormatting += h, h => grid.CellFormatting -= h,
                        Callbacks.Handler<GridCellFormatEventArgs>(args => Formatted(list, row, args), new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.CellFormatting)))))
                + row.Activate.Map(activate => Activated(grid, list, row, activate, scope)).ToSeq(),
                DisposalOps.Release)
            .Map(attached => DisposalOps.Composite(attached, new CallbackSite(scope.Sink, typeof(ListRows), nameof(Gestures))));

    private static IO<IDisposable> Activated<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, Func<TItem, IO<Unit>> activate, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        Subscriptions.Attach(h => grid.Activated += h, h => grid.Activated -= h,
            Callbacks.Handler<TreeGridViewItemEventArgs>(
                _ => from head in IO.lift(() => Chosen(Folded(row, list.Value.Nodes).Visible, toHashSet(grid.SelectedRows)).Head)
                     from ran in head.Match(Some: node => activate(node.Row), None: static () => IO.pure(unit))
                     select ran,
                new CallbackSite(scope.Sink, typeof(TreeGridView), nameof(TreeGridView.Activated))));

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
            action.Iter(static run => run());
            args.Handled = action.IsSome;
        }));

    private static IO<Unit> Pointed<TItem, TKey>(TreeGridView grid, Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, MouseEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        when(args.Buttons == MouseButtons.Alternate,
            from pressed in IO.lift(() => (grid.GetCellAt(args.Location).Item, Rows: toHashSet(grid.SelectedRows)))
            from found in IO.lift(() => Folded(row, list.Value.Nodes).Visible
                .Choose((index, node) => Callbacks.Found(ReferenceEquals(node, pressed.Item) && !pressed.Rows.Contains(index), index))
                .Head)
            from selected in IO.lift(() => found.Iter(at => grid.SelectedRows = [at]))
            select selected).As();

    private static IO<Unit> Formatted<TItem, TKey>(Atom<ListState<TItem>> list, ListRow<TItem, TKey> row, GridCellFormatEventArgs args)
        where TItem : notnull
        where TKey : notnull =>
        IO.lift(() => Resolved(row, list.Value.Nodes, args.Item).Iter(node =>
            (args.Font, args.ForegroundColor) = (
                node.Kids.IsEmpty ? args.Font : SystemFonts.Bold(args.Font.Size),
                row.Available.ForAll(available => available(node.Row)) ? args.ForegroundColor : PaintSlot.DisabledText.Read())));

    // --- [COMMANDS]
    private static IO<(Option<Control> Pair, Seq<Seq<MenuEntry>> Menu, Seq<(Command Command, Action Run)> Keys, IO<Unit> Refresh, IDisposable Release)> Commanded(
        Option<(CommandRow.Run Add, CommandRow.Run Remove)> pair, Seq<CommandRow> commands, RowScope scope) =>
        from site in IO.pure(new CallbackSite(scope.Sink, typeof(CommandRow), nameof(CommandRow.Menu)))
        from paired in pair.Traverse(held =>
            from add in held.Add.Realize(scope)
            from remove in DisposalOps.OnFailure(held.Remove.Realize(scope), IO.lift(fun(add.Release.Dispose)))
            from placed in DisposalOps.OnFailure(ButtonRows.AddRemove(add.Commands, remove.Commands), DisposalOps.Release(Seq(add.Release, remove.Release)))
            select (Control: (Control)placed.Control,
                Keys: Seq((Command: add.Commands, Run: add.Commands.Execute), (Command: remove.Commands, Run: (Action)remove.Commands.Execute)),
                Refresh: add.Refresh.Bind(_ => remove.Refresh),
                Release: DisposalOps.Composite(Seq(add.Release, remove.Release, placed.Release), site))).As()
        from realized in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(commands.Map(command => Projected(command, scope)), static held => DisposalOps.Release(held.Map(static command => command.Realized.Release))),
            DisposalOps.Release(paired.Map(static held => held.Release).ToSeq()))
        let menu = CommandRow.Menu(Seq(realized.Map(static command => command.Realized)), site)
        select (
            paired.Map(static held => held.Control),
            (paired.Map(static held => held.Keys.Map(static key => key.Command)).ToSeq() + menu.Groups).Map(static group => group.Map(static command => (MenuEntry)command)),
            paired.ToSeq().Bind(static held => held.Keys) + realized.Bind(static command => command.Keys),
            paired.Map(static held => held.Refresh).ToSeq().Add(menu.Joined.Refresh).TraverseM(static refresh => refresh).As().Map(static _ => unit),
            DisposalOps.Composite(paired.Map(static held => held.Release).ToSeq().Add(menu.Joined.Release), site));

    private static IO<(Realized<Seq<Command>> Realized, Seq<(Command Command, Action Run)> Keys)> Projected(CommandRow command, RowScope scope) =>
        command.Switch(
            scope,
            run: static (held, run) => run.Realize(held).Map(static done => (
                new Realized<Seq<Command>>(Seq(done.Commands), done.Refresh, done.Release),
                Seq((Command: done.Commands, Run: (Action)done.Commands.Execute)))),
            check: static (held, check) => check.Realize(held).Map(static done => (
                new Realized<Seq<Command>>(Seq<Command>(done.Commands), done.Refresh, done.Release),
                Seq((Command: (Command)done.Commands, Run: (Action)(() => done.Commands.Checked = !done.Commands.Checked))))),
            radio: static (held, radio) => radio.Realize(held).Map(static done => (
                new Realized<Seq<Command>>(done.Commands.Map(static member => (Command)member.Command), done.Refresh, done.Release),
                toSeq(from member in done.Commands select (Command: (Command)member.Command, Run: (Action)(() => member.Command.Checked = true))))));

    // --- [PLACEMENT]
    private static Control Paired(Control content, Option<Control> pair) =>
        pair.Match(
            Some: buttons => new StackLayout {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Panel),
                Items = { new StackLayoutItem(content, expand: true), new StackLayoutItem(buttons, HorizontalAlignment.Left) },
            },
            None: () => content);

    private static IO<(Control Control, Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)> Brackets, Seq<IDisposable> Held)> Searched<TItem>(
        Control content, Option<ListSearch<TItem>> search, Wording wording, Func<Func<TItem, bool>, IO<Unit>> keep, RowScope scope) where TItem : notnull =>
        search.Match(
            Some: held =>
                from field in ChoiceRows.Search(wording.Shown(held.Placeholder, scope.Sink), matches => keep(item => matches(held.Text(scope, item))), scope.Sink)
                from shown in DisposalOps.OnFailure(
                    held.Shown.Traverse(source => Watched(source, Named(source, scope), visible => IO.lift(fun(() => { field.Control.Visible = visible; })), scope,
                        new CallbackSite(scope.Sink, typeof(SearchBox), nameof(SearchBox.Visible)))).As(),
                    IO.lift(fun(field.Release.Dispose)))
                select ((Control)new StackLayout {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Panel),
                    Items = { field.Control, new StackLayoutItem(content, expand: true) },
                },
                    shown.Map(static watched => (watched.Keys, watched.Shown)).ToSeq(),
                    field.Release.Cons(shown.Map(static watched => watched.Release).ToSeq())),
            None: () => IO.pure(((Control)content, Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)>(), Seq<IDisposable>())));

    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown, IDisposable Release)> Watched<TValue>(
        RowSource<TValue> source, Seq<EntryKey> keys, Func<TValue, IO<Unit>> show, RowScope scope, CallbackSite site) where TValue : notnull =>
        from signals in source.Signals(scope)
        let shown = scope.Read(source).Bind(show)
        from held in DisposalOps.AcquireAll(
            signals.Map(signal => signal.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ => shown), site.Sink)),
            DisposalOps.Release)
        select (keys, shown, DisposalOps.Composite(held, site));

    private static Seq<EntryKey> Named(RowSource source, RowScope scope) =>
        source.Values(scope).Paths().Map(path => new EntryKey(source.Owner, path));

    private static IO<IDisposable> Listened(Seq<(Seq<EntryKey> Keys, IO<Unit> Shown)> brackets, RowScope scope) =>
        scope.Listen(changed => brackets
            .Filter(bracket => bracket.Keys.Exists(key => changed.Exists(held => held == key)))
            .TraverseM(static bracket => bracket.Shown)
            .As()
            .Map(static _ => unit));

    // --- [LAYOUT]
    private const string Layouts = "thumbnail-browser";

    private static IO<(RowLine Line, Seq<EntryKey> Keys, IO<Unit> Shown, IDisposable Release)> Laid<TItem, TKey>(
        RowSource<BrowserLayout> layout, BrowserMode mode, ThumbnailBrowser<TItem, TKey> browser, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        from slider in IO.lift(static () => new Eto.Forms.Slider { MinValue = (int)TileSize.MinValue.ToValue(), MaxValue = (int)TileSize.MaxValue.ToValue(), TickFrequency = 0 })
        let keys = Named(layout, scope)
        let written = (Func<Func<BrowserLayout, BrowserLayout>, IO<Unit>>)(next =>
            from held in scope.Read(layout)
            from stored in scope.Store(layout).Write(Some(next(held)))
            from changed in scope.Changed(keys)
            select unit)
        from modes in DisposalOps.OnFailure(
            ChoiceRows.Strip(
                toSeq(BrowserMode.Items).Map(static item => new ChoiceItem<BrowserMode>(item, item.Caption, Some<IGlyph>(item.Glyph))),
                static picked => written(held => held with { Mode = Some(picked) }), scope.Sink),
            IO.lift(fun(slider.Dispose)))
        let add = (Action<EventHandler<EventArgs>>)(static h => slider.ValueChanged += h)
        let remove = (Action<EventHandler<EventArgs>>)(static h => slider.ValueChanged -= h)
        let moved = Callbacks.Handler<EventArgs>(
            static _ => IO.lift(static () => TileSize.Clamped(slider.Value)).Bind(static size => written(held => held with { Size = Some(size) })),
            new CallbackSite(scope.Sink, typeof(Eto.Forms.Slider), nameof(Eto.Forms.Slider.ValueChanged)))
        let widths = IO.lift(() => browser.ImageWidth)
            .Bind(static width => Subscriptions.Detached(add, remove, moved, IO.lift(fun(() => slider.Value = (int)MathF.Round(width.ToValue())))))
        let failed = Seq<IDisposable>(slider, modes.Release)
        from watched in DisposalOps.OnFailure(
            Watched(layout, keys, held => held.Mode.IfNone(mode) switch {
                var shown =>
                    from sized in browser.TileWidth(held.Size)
                    from moded in browser.Mode(shown)
                    from picked in modes.Show(Some(shown))
                    from fitted in widths
                    select unit,
            },
                scope, new CallbackSite(scope.Sink, typeof(ThumbnailBrowser<TItem, TKey>), nameof(ThumbnailBrowser<,>.Mode))),
            DisposalOps.Release(failed))
        from routes in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                Seq(Subscriptions.Attach(add, remove, moved),
                    Subscriptions.Attach(h => browser.SizeChanged += h, h => browser.SizeChanged -= h,
                        Callbacks.Handler<EventArgs>(static _ => widths, new CallbackSite(scope.Sink, typeof(ThumbnailBrowser<TItem, TKey>), nameof(Control.SizeChanged))))),
                DisposalOps.Release),
            DisposalOps.Release(failed.Add(watched.Release)))
        select (
            new RowLine(None, new TableLayout(new TableRow(new TableCell(slider, true), modes.Control)) { Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) }, None, None),
            keys,
            watched.Shown,
            DisposalOps.Composite(Seq(modes.Release, watched.Release) + routes, new CallbackSite(scope.Sink, typeof(ListRows), nameof(Laid))));

    // --- [ROUTES]
    private static BrowserSource<TItem, TKey> Source<TItem, TKey>(BrowserRow<TItem, TKey> row, RowScope scope)
        where TItem : notnull
        where TKey : notnull =>
        new(row.Key, item => row.Label(scope, item), (view, item, size) => row.Tile(scope, view, item, size), row.Wording.Shown(row.Empty, scope.Sink)) {
            Numbered = row.Numbered,
            Favourite = row.Favourite.Map(static lens => lens.Get),
            Preview = row.Preview.Map(preview => (Func<Option<TItem>, IO<Unit>>)(item => preview(scope, item))),
            Export = row.File.Map(static file => (Func<Control, Seq<TItem>, Option<Thumbnail>, IO<Unit>>)(
                (view, items, thumbnail) => Transfer.Drag(view, items.Map(file), thumbnail.Bind(static held => held.Drag)))),
            Renames = row.Rename.IsSome,
            Reorders = row.Ordered.IsNone,
        };

    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown, IDisposable Release)> Items<TItem, TKey>(
        ThumbnailBrowser<TItem, TKey> browser, (RowSource<Seq<TItem>> Source, RowField<Seq<TItem>> Field) items, BrowserRow<TItem, TKey> row, RowScope scope, CallbackSite site)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(items.Source, IterableNE.create(items.Field),
                h => browser.Edited += h, h => browser.Edited -= h,
                edit => Callbacks.Handler<Edit<BrowserEdit<TItem, TKey>>>(change => ToItems(row, change, edit), site),
                shown => browser.Items(Ordered(row.Ordered, shown.ToSeq().Flatten())), scope, site)
            .Map(static bound => (bound.Edit.Keys, bound.Edit.Shown, bound.Release));

    private static IO<(Seq<EntryKey> Keys, IO<Unit> Shown, IDisposable Release)> Current<TItem, TKey>(
        ThumbnailBrowser<TItem, TKey> browser, (RowSource<Option<TKey>> Source, RowField<Option<TKey>> Field) current, RowScope scope, CallbackSite site)
        where TItem : notnull
        where TKey : notnull =>
        RowEdit.Bind(current.Source, IterableNE.create(current.Field),
                h => browser.Edited += h, h => browser.Edited -= h,
                edit => Callbacks.Handler<Edit<BrowserEdit<TItem, TKey>>>(change => ToCurrent(change, edit), site),
                shown => browser.Receive(shown.Bind(static key => key).Map<BrowserEdit<TItem, TKey>>(static key => new BrowserEdit<TItem, TKey>.Applied(key))),
                scope, site)
            .Map(static bound => (bound.Edit.Keys, bound.Edit.Shown, bound.Release));

    private static Option<Edit<TOut>> Mapped<TIn, TOut>(Edit<TIn> change, Func<TIn, Option<TOut>> map)
        where TIn : notnull
        where TOut : notnull =>
        change.Switch(
            map,
            preview: static (into, preview) => into(preview.Value).Map<Edit<TOut>>(static value => new Edit<TOut>.Preview(value)),
            step: static (into, step) => into(step.Value).Map<Edit<TOut>>(static value => new Edit<TOut>.Step(value)),
            commit: static (into, commit) => into(commit.Value).Map<Edit<TOut>>(static value => new Edit<TOut>.Commit(value)),
            cancel: static (_, _) => Some<Edit<TOut>>(new Edit<TOut>.Cancel()));

    private static IO<Unit> ToCurrent<TItem, TKey>(Edit<BrowserEdit<TItem, TKey>> change, RowEdit<Option<TKey>> edit)
        where TItem : notnull
        where TKey : notnull =>
        Mapped(change, static value => value.Switch(
                applied: static applied => Some(applied.Key),
                reordered: static _ => Option<TKey>.None,
                renamed: static _ => Option<TKey>.None,
                favourited: static _ => Option<TKey>.None))
            .Match(Some: picked => edit.Take(picked, static (key, _) => Fin.Succ(Some(key))), None: static () => IO.pure(unit));

    private static IO<Unit> ToItems<TItem, TKey>(BrowserRow<TItem, TKey> row, Edit<BrowserEdit<TItem, TKey>> change, RowEdit<Seq<TItem>> edit)
        where TItem : notnull
        where TKey : notnull =>
        Mapped(change, value => Into(row, value))
            .Match(Some: mapped => edit.Take(mapped, static (into, latest) => into(latest)), None: static () => IO.pure(unit));

    private static Option<Func<Seq<TItem>, Fin<Seq<TItem>>>> Into<TItem, TKey>(BrowserRow<TItem, TKey> row, BrowserEdit<TItem, TKey> value)
        where TItem : notnull
        where TKey : notnull =>
        value.Switch(
            row,
            applied: static (_, _) => Option<Func<Seq<TItem>, Fin<Seq<TItem>>>>.None,
            reordered: static (held, reordered) => Some<Func<Seq<TItem>, Fin<Seq<TItem>>>>(latest => Fin.Succ(toSeq(
                from key in reordered.Keys
                from item in latest
                where EqualityComparer<TKey>.Default.Equals(held.Key(item), key)
                select item))),
            renamed: static (held, renamed) => held.Rename.Map<Func<Seq<TItem>, Fin<Seq<TItem>>>>(rename => latest =>
                from item in rename(renamed.Item, renamed.Name)
                select Replaced(held.Key, None, latest, item)),
            favourited: static (held, favourited) => held.Favourite
                .Map(lens => lens.Update(static on => !on, favourited.Item))
                .Map<Func<Seq<TItem>, Fin<Seq<TItem>>>>(toggled => latest => Fin.Succ(Replaced(held.Key, None, latest, toggled))));
}
