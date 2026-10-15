using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChoiceItem<TModel>(TModel Value, string Caption, Option<(IGlyph Glyph, int Master)> Glyph) where TModel : notnull;

[Union]
public abstract partial record MenuPick {
    public sealed record Item(string Caption, bool Marked, IO<Unit> Run) : MenuPick;

    public sealed record Group(string Caption, Seq<MenuPick> Picks) : MenuPick;
}

[Union]
public abstract partial record MenuEntry {
    public sealed record Item(Command Command) : MenuEntry;

    public sealed record Submenu(string Caption, Seq<Seq<MenuEntry>> Groups) : MenuEntry;

    public sealed record Listed(string Caption, IO<Seq<MenuPick>> Read) : MenuEntry;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class MenuPickMapper {
    [MapProperty(nameof(MenuPick.Item.Caption), nameof(CheckMenuItem.Text))]
    [MapProperty(nameof(MenuPick.Item.Marked), nameof(CheckMenuItem.Checked))]
    [MapperIgnoreSource(nameof(MenuPick.Item.Run), Justification = "The item's Click handler runs it")]
    internal static partial void Apply(MenuPick.Item item, CheckMenuItem target);
}

public static class ChoiceRows {
    // --- [ITEMS]
    public static Seq<ChoiceItem<TValue>> Items<TValue, TError>(Func<TValue, string> caption, Option<Func<TValue, (IGlyph Glyph, int Master)>> glyph, IPlugInSink plugIn)
        where TValue : ISmartEnum<string, TValue, TError>
        where TError : Error, IValidationError<TError> =>
        toSeq(TValue.Items).Map(item => new ChoiceItem<TValue>(item, Wording.English.Shown(caption(item), plugIn), glyph.Map(read => read(item)))).Strict();

    public static Seq<ChoiceItem<TEnum>> Members<TEnum>(Func<TEnum, string> caption, IPlugInSink plugIn) where TEnum : struct, Enum =>
        toSeq(Enum.GetValues<TEnum>()).Map(member => new ChoiceItem<TEnum>(member, Wording.English.Shown(caption(member), plugIn), None)).Strict();

    public static Seq<ChoiceItem<Option<TModel>>> Optional<TModel>(string absent, Seq<ChoiceItem<TModel>> items, IPlugInSink plugIn) where TModel : notnull =>
        new ChoiceItem<Option<TModel>>(None, Wording.English.Shown(absent, plugIn), None)
            .Cons(items.Map(static item => new ChoiceItem<Option<TModel>>(Some(item.Value), item.Caption, item.Glyph)));

    // --- [ROWS]
    public static ControlRow Choice<TRecord, TModel>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TModel> lens, Func<RowScope, IO<Seq<ChoiceItem<TModel>>>> items, RowRules rules)
        where TRecord : notnull
        where TModel : notnull =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) =>
            from listed in items(scope)
            let site = new CallbackSite(scope.Sink, typeof(DropDown), nameof(DropDown.SelectedValueChanged))
            from popup in Popped(listed, site, retain)
            from edit in RowEdit.Bind(source, IterableNE.create(field),
                h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h,
                bound => Callbacks.Handler<EventArgs>(_ =>
                    from picked in popup.Picked
                    from held in scope.Read(source)
                    from committed in picked.Traverse(pick => bound.Commit(lens.Set(pick, held))).As()
                    select unit, site),
                value => popup.Write(value.Map(lens.Get)), scope, site, retain)
            from opening in Subscriptions.Attach(h => popup.Control.DropDownOpening += h, h => popup.Control.DropDownOpening -= h,
                Callbacks.Handler<EventArgs>(_ => items(scope).Bind(popup.Relist).Bind(changed => when(changed, edit.Shown).As()),
                    new CallbackSite(scope.Sink, typeof(DropDown), nameof(DropDown.DropDownOpening))), retain)
            select new RowCells(
                popup.Control, None, None, None, [], [],
                new RowHelp(None, listed.Find(item => EqualityComparer<TModel>.Default.Equals(item.Value, lens.Get(source.Default))).Map(static item => item.Caption)),
                edit.Shown, Some<RowEdit>(edit), Some(Labelled([], [], scope.Sink))), rules);

    public static ControlRow Flags<TRecord>(
        RowSource<TRecord> source, string caption, string help, IterableNE<(RowField<TRecord> Field, Lens<TRecord, bool> Lens, (IGlyph Glyph, int Master) Glyph)> members, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, members.Map(static member => member.Field), caption, help, RowShape.Inline, (scope, retain) =>
            from site in IO.pure(new CallbackSite(scope.Sink, typeof(SegmentedButton), nameof(SegmentedButton.SelectedItemsChanged)))
            from strip in Segmented(
                toSeq(members.Map(member => new ChoiceItem<RowField<TRecord>>(member.Field, Wording.English.Shown(member.Field.Caption, scope.Sink), Some(member.Glyph)))),
                SegmentedSelectionMode.Multiple, site, retain)
            from edit in RowEdit.Bind(source, members.Map(static member => member.Field),
                h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h,
                bound => Callbacks.Handler<EventArgs>(_ =>
                    from picked in strip.Picked
                    from held in scope.Read(source)
                    from committed in bound.Commit(members.Fold((record, member) => member.Lens.Set(picked.Exists(member.Field.Equals), record), held))
                    select unit, site),
                value => strip.Write(value.Map(record => members.Filter(member => member.Lens.Get(record)).Map(static member => member.Field).ToSeq())), scope, site, retain)
            select new RowCells(strip.Control, None, None, None, [], [], new RowHelp(None, None), edit.Shown, Some<RowEdit>(edit),
                Some(Context(strip.Control, [], [], scope.Sink))), rules);

    // --- [CONTROLS]
    public static IO<(DropDown Control, Func<Option<TModel>, IO<Unit>> Show)> Popup<TModel>(
        Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInSink sink, Action<IDisposable> retain) where TModel : notnull =>
        DisposalOps.Acquire<(DropDown Control, Func<Option<TModel>, IO<Unit>> Show)>(keep =>
            from site in IO.pure(new CallbackSite(sink, typeof(DropDown), nameof(DropDown.SelectedValueChanged)))
            from popup in Popped(items, site, keep)
            from show in Commits(h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h,
                popup.Picked, popup.Write, commit, site, keep)
            select (popup.Control, show), (_, release) => retain(release));

    public static IO<(Control Control, Func<Option<TModel>, IO<Unit>> Show)> Strip<TModel>(
        Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInSink sink, Action<IDisposable> retain) where TModel : notnull {
        const int segmentLimit = 5;
        return DisposalOps.Acquire<(Control Control, Func<Option<TModel>, IO<Unit>> Show)>(keep => items.Count <= segmentLimit
            ? from site in IO.pure(new CallbackSite(sink, typeof(SegmentedButton), nameof(SegmentedButton.SelectedItemsChanged)))
              from strip in Segmented(items, SegmentedSelectionMode.Single, site, keep)
              from show in Commits(h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h,
                  strip.Picked.Map(static picks => picks.Head), value => strip.Write(value.Map(static one => Seq(one))), commit, site, keep)
              select ((Control)strip.Control, show)
            : Popup(items, commit, sink, keep).Map(static popup => ((Control)popup.Control, popup.Show)), (_, release) => retain(release));
    }

    // --- [MENUS]
    public static IO<ContextMenu> Menu(
        Seq<Seq<MenuEntry>> groups, Seq<IO<Unit>> refreshes, Option<IO<Seq<MenuPick>>> listing, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from menu in DisposalOps.Acquire(IO.lift(static () => new ContextMenu()), static held => IO.lift(held.Dispose), keep)
            from populated in Populate(menu, groups, refreshes, listing, sink, keep)
            select menu, (_, release) => retain(release));

    public static IO<Unit> Populate(
        ContextMenu menu, Seq<Seq<MenuEntry>> groups, Seq<IO<Unit>> refreshes, Option<IO<Seq<MenuPick>>> listing, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from site in IO.pure(new CallbackSite(sink, typeof(ContextMenu), nameof(ContextMenu.Opening)))
            from entries in Entries(groups, site, keep, menu.Items.Count is not 0)
            from inserted in IO.lift(() => {
                IDisposable owned = new Disposal<Seq<MenuItem>>(entries, held => Removed(menu.Items, held).Run());
                keep(owned);
                _ = entries.Iter(entry => menu.Items.Insert(menu.Items.Count, entry));
            }).Uninterruptible()
            from opened in Opened(menu.Items, h => menu.Opening += h, h => menu.Opening -= h, refreshes, listing, site, keep)
            select unit, (_, release) => retain(release));

    public static Func<Seq<Seq<Command>>, IO<Unit>, Action<IDisposable>, IO<ContextMenu>> Context(
        Control control, Seq<Seq<MenuEntry>> own, Seq<IO<Unit>> refreshes, IPlugInSink sink) =>
        (groups, refresh, retain) => DisposalOps.Acquire(keep =>
            from menu in Labelled(own, refreshes, sink)(groups, refresh, keep)
            from attached in DisposalOps.Change(() => control.ContextMenu, value => control.ContextMenu = value, menu, keep)
            select menu, (_, release) => retain(release));

    public static Func<Seq<Seq<Command>>, IO<Unit>, Action<IDisposable>, IO<ContextMenu>> Labelled(
        Seq<Seq<MenuEntry>> own, Seq<IO<Unit>> refreshes, IPlugInSink sink) =>
        (groups, refresh, retain) => Menu(own + groups.Map(static cluster => cluster.Map<MenuEntry>(static command => new MenuEntry.Item(command))), refreshes.Add(refresh), None, sink, retain);

    // --- [SEARCH]
    public static IO<SearchBox> Search(string placeholder, Func<Func<string, bool>, IO<Unit>> filter, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from field in DisposalOps.Acquire(IO.lift(static () => new SearchBox()), static held => IO.lift(held.Dispose), keep)
            from faced in IO.lift(() => field.PlaceholderText = placeholder)
            from attached in Subscriptions.Attach(h => field.TextChanged += h, h => field.TextChanged -= h,
                Callbacks.Handler<EventArgs>(
                    _ => IO.lift(() => field.Text.Trim()).Bind(query => filter(text => RowText.Culture.CompareInfo.IndexOf(text, query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)),
                    new CallbackSite(sink, typeof(SearchBox), nameof(SearchBox.TextChanged))), keep)
            select field, (_, release) => retain(release));

    // --- [REALIZE]
    private static IO<(DropDown Control, Func<Option<TModel>, IO<Unit>> Write, IO<Option<TModel>> Picked, Func<Seq<ChoiceItem<TModel>>, IO<bool>> Relist)> Popped<TModel>(
        Seq<ChoiceItem<TModel>> items, CallbackSite site, Action<IDisposable> retain) where TModel : notnull =>
        from popup in DisposalOps.Acquire(IO.lift(static () => new DropDown()), static held => IO.lift(held.Dispose), retain)
        from varies in IO.lift(static () => new ListItem { Text = RowEdit.Varies })
        from held in IO.lift(static () => Ref(Option<Listing<TModel>>.None))
        from owned in DisposalOps.Acquire(IO.lift(() => new Disposal<Ref<Option<Listing<TModel>>>>(held, static cell =>
            atomicIO(() => { Option<Listing<TModel>> prior = cell.Value; cell.Value = None; return prior; })
                .Bind(static prior => IO.lift(() => prior.Iter(static listing => listing.Themed.Dispose()))).Uninterruptible().Run())),
            static owner => IO.lift(owner.Dispose), retain)
        let replace = (Func<Seq<ChoiceItem<TModel>>, IO<Unit>>)(roster => DisposalOps.Replace(
            keep => Listed(popup, roster, site, keep),
            (rows, release) => atomic(() => {
                _ = owned.Held.ToFin(Errors.Cancelled).ThrowIfFail();
                Option<Listing<TModel>> prior = held.Value;
                held.Value = Some(new Listing<TModel>(rows, release));
                return IO.lift(() => prior.Iter(static listing => listing.Themed.Dispose()));
            })).Map(static _ => unit))
        from first in replace(items)
        select (
            popup,
            (Func<Option<TModel>, IO<Unit>>)(value => held.ValueIO.Bind(current => IO.lift(() => current.Iter(listing =>
                (popup.DataStore, popup.SelectedValue) = value.Match(
                    Some: model => (listing.Entries, listing.Entry(model).ValueUnsafe()),
                    None: () => (varies.Cons(listing.Entries), (object?)varies)))))),
            IO.lift(() => held.Value.Bind(current => current.Value(popup.SelectedValue))),
            (Func<Seq<ChoiceItem<TModel>>, IO<bool>>)(next => held.ValueIO.Bind(current => current.Match(
                Some: listing => listing.Rows.Map(static row => row.Item).Equals(next)
                    ? IO.pure(value: false) : replace(next).Map(value: true),
                None: static () => IO.pure(value: false)))));

    private static IO<Seq<(ChoiceItem<TModel> Item, ImageListItem Entry)>> Listed<TModel>(
        Control control, Seq<ChoiceItem<TModel>> items, CallbackSite site, Action<IDisposable> retain) where TModel : notnull =>
        from rows in IO.lift(() => items.Map(static item => (Item: item, Entry: new ImageListItem { Text = item.Caption })).Strict())
        from themed in DisposalOps.AcquireAll(
            rows.Bind(row => row.Item.Glyph.Map(glyph => (Func<Action<IDisposable>, IO<Unit>>)(keep => Icons.Themed(site.Sink, glyph, IconSlot.DropDownItem, HostTheme.In(control), icon => row.Entry.Image = icon, keep))).ToSeq()),
            DisposalOps.Release, (_, lease) => retain(lease))
        select rows;

    private static IO<(SegmentedButton Control, Func<Option<Seq<TModel>>, IO<Unit>> Write, IO<Seq<TModel>> Picked)> Segmented<TModel>(
        Seq<ChoiceItem<TModel>> items, SegmentedSelectionMode mode, CallbackSite site, Action<IDisposable> retain) where TModel : notnull =>
        from strip in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton()), static held => IO.lift(held.Dispose), retain)
        from selected in IO.lift(() => strip.SelectionMode = mode)
        from rows in items.TraverseM(item =>
            from segment in DisposalOps.Acquire(IO.lift(static () => new ButtonSegmentedItem()), ButtonRows.Released,
                held => retain(new Disposal<SegmentedItem>(held, static owned => ButtonRows.Released(owned).Run())))
            from faced in IO.lift(() => { (segment.Text, segment.ToolTip) = (item.Glyph.Map(static _ => "").IfNone(item.Caption), item.Caption); })
            select (Item: item, Segment: segment)).As()
        from added in IO.lift(() => strip.Items.AddRange(rows.Map(static row => (SegmentedItem)row.Segment)))
        from themed in DisposalOps.AcquireAll(
            rows.Bind(row => row.Item.Glyph.Map(glyph => (Func<Action<IDisposable>, IO<Unit>>)(keep => Icons.Themed(site.Sink, glyph, IconSlot.PanelButton, HostTheme.In(strip), icon => row.Segment.Image = icon, keep))).ToSeq()), DisposalOps.Release, (_, lease) => retain(lease))
        select (
            strip,
            (Func<Option<Seq<TModel>>, IO<Unit>>)(value => toHashSet(value.ToSeq().Flatten()) switch {
                var shown => IO.lift(() => rows.Iter(row => row.Segment.Selected = shown.Contains(row.Item.Value))),
            }),
            IO.lift(() => rows.Filter(static row => row.Segment.Selected).Map(static row => row.Item.Value).Strict()));

    private static IO<Func<Option<TModel>, IO<Unit>>> Commits<TModel>(
        Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> remove, IO<Option<TModel>> picked, Func<Option<TModel>, IO<Unit>> write,
        Func<TModel, IO<Unit>> commit, CallbackSite site, Action<IDisposable> retain) where TModel : notnull =>
        from handler in IO.pure(Callbacks.Handler<EventArgs>(_ => picked.Bind(pick => pick.Match(Some: commit, None: static () => IO.pure(unit))), site))
        from attached in Subscriptions.Attach(add, remove, handler, retain)
        select (Func<Option<TModel>, IO<Unit>>)(value => Subscriptions.Detached(add, remove, handler, write(value)));

    private static IO<Seq<MenuItem>> Entries(Seq<Seq<MenuEntry>> groups, CallbackSite site, Action<IDisposable> retain, bool preceding = false) =>
        DisposalOps.AcquireAll(groups.Filter(static group => !group.IsEmpty).Map((group, index) =>
            (preceding || index is not 0
                ? Seq<Func<Action<IDisposable>, IO<MenuItem>>>(static keep => DisposalOps.Acquire(IO.lift(static () => new SeparatorMenuItem()), static held => IO.lift(held.Dispose), keep).Map(static separator => (MenuItem)separator))
                : Seq<Func<Action<IDisposable>, IO<MenuItem>>>())
            .Concat(group.Map(entry => (Func<Action<IDisposable>, IO<MenuItem>>)(keep => Entry(entry, site, keep))))).Flatten(), DisposalOps.Release, (_, lease) => retain(lease));

    private static IO<MenuItem> Entry(MenuEntry entry, CallbackSite site, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep => entry.Switch((Site: site, Retain: keep),
            item: static (at, item) =>
                DisposalOps.Acquire(IO.lift(() => {
                    MenuItem created = item.Command.CreateMenuItem();
                    return (Item: created, Release: (IDisposable)new Disposal<MenuItem>(created, static held =>
                        Callbacks.Each(Seq(IO.lift(() => { held.Command = null; }), IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(unit).Run()));
                }), static held => IO.lift(held.Release.Dispose), held => at.Retain(held.Release)).Map(static held => held.Item),
            submenu: static (at, submenu) =>
                from sub in DisposalOps.Acquire(IO.lift(static () => new SubMenuItem()), static held => IO.lift(held.Dispose), at.Retain)
                from faced in IO.lift(() => sub.Text = submenu.Caption)
                from children in Entries(submenu.Groups, at.Site, at.Retain)
                from inserted in IO.lift(() => children.Iter(child => sub.Items.Insert(sub.Items.Count, child)))
                select (MenuItem)sub,
            listed: static (at, listed) =>
                from sub in DisposalOps.Acquire(IO.lift(static () => new SubMenuItem()), static held => IO.lift(held.Dispose), at.Retain)
                from faced in IO.lift(() => sub.Text = listed.Caption)
                from opened in Opened(sub.Items, h => sub.Opening += h, h => sub.Opening -= h, [], Some(listed.Read),
                    new CallbackSite(at.Site.Sink, typeof(SubMenuItem), nameof(SubMenuItem.Opening)), at.Retain)
                select (MenuItem)sub), (_, release) => retain(release));

    private static IO<MenuItem> Pick(MenuPick pick, CallbackSite site, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep => pick.Switch((Site: site, Retain: keep),
            item: static (at, item) =>
                from check in DisposalOps.Acquire(IO.lift(static () => new CheckMenuItem()), static held => IO.lift(held.Dispose), at.Retain)
                from faced in IO.lift(() => MenuPickMapper.Apply(item, check))
                from clicked in Subscriptions.Attach(h => check.Click += h, h => check.Click -= h,
                    Callbacks.Handler<EventArgs>(_ => item.Run, new CallbackSite(at.Site.Sink, typeof(CheckMenuItem), nameof(CheckMenuItem.Click))), at.Retain)
                select (MenuItem)check,
            group: static (at, nested) =>
                from sub in DisposalOps.Acquire(IO.lift(static () => new SubMenuItem()), static held => IO.lift(held.Dispose), at.Retain)
                from faced in IO.lift(() => sub.Text = nested.Caption)
                from children in DisposalOps.AcquireAll(nested.Picks.Map(child => (Func<Action<IDisposable>, IO<MenuItem>>)(held => Pick(child, at.Site, held))),
                    DisposalOps.Release, (_, lease) => at.Retain(lease))
                from inserted in IO.lift(() => children.Iter(child => sub.Items.Insert(sub.Items.Count, child)))
                select (MenuItem)sub), (_, release) => retain(release));

    private static IO<Unit> Opened(
        MenuItemCollection items, Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> remove,
        Seq<IO<Unit>> refreshes, Option<IO<Seq<MenuPick>>> listing, CallbackSite site, Action<IDisposable> retain) =>
        from shown in IO.lift(static () => Ref(Seq<IDisposable>()))
        let clear = atomicIO(() => {
            Seq<IDisposable> taken = shown.Value;
            shown.Value = [];
            return taken;
        }).Bind(DisposalOps.Release).Uninterruptible()
        from owned in DisposalOps.Acquire(IO.lift(() => new Disposal<Ref<Seq<IDisposable>>>(shown, _ => clear.Run())), static held => IO.lift(held.Dispose), retain)
        from attached in Subscriptions.Attach(add, remove, Callbacks.Handler<EventArgs>(_ => Callbacks.Each(refreshes + listing.Map(read =>
            clear.Bind(_ => read).Bind(picks => DisposalOps.AcquireAll(
                picks.Map(pick => (Func<Action<IDisposable>, IO<MenuItem>>)(keep => Pick(pick, site, keep)))
                    .Concat(!picks.IsEmpty && items.Count is not 0
                        ? Seq<Func<Action<IDisposable>, IO<MenuItem>>>(static keep =>
                            DisposalOps.Acquire(IO.lift(static () => new SeparatorMenuItem()), static held => IO.lift(held.Dispose), keep).Map(static separator => (MenuItem)separator))
                        : Seq<Func<Action<IDisposable>, IO<MenuItem>>>())
                    .Map((factory, index) => (Func<Action<IDisposable>, IO<Unit>>)(keep => Placed(items, factory, index, keep))),
                DisposalOps.Release, (_, lease) => atomic(() =>
                    owned.Held.ToFin(Errors.Cancelled).Map(_ => shown.Value = Seq(lease)).ThrowIfFail())))
                .Map(unit)).ToSeq()).Map(static _ => unit), site), retain)
        select unit;

    private static IO<Unit> Placed(MenuItemCollection items, Func<Action<IDisposable>, IO<MenuItem>> factory, int index, Action<IDisposable> keep) =>
        from item in factory(keep)
        from removal in DisposalOps.Acquire(IO.lift(() => new Disposal<MenuItem>(item, held => items.Remove(held))), static held => IO.lift(held.Dispose), keep)
        from inserted in IO.lift(() => items.Insert(index, item))
        select unit;

    private static IO<Unit> Removed(MenuItemCollection items, Seq<MenuItem> held) =>
        Callbacks.Each(held.Map(entry => IO.lift(() => items.Remove(entry)).Map(unit))).Map(static _ => unit).Uninterruptible();

    private sealed record Listing<TModel>(Seq<(ChoiceItem<TModel> Item, ImageListItem Entry)> Rows, IDisposable Themed) where TModel : notnull {
        public Seq<object> Entries => Rows.Map(static row => (object)row.Entry);

        public Option<ImageListItem> Entry(TModel value) =>
            Rows.Find(row => EqualityComparer<TModel>.Default.Equals(row.Item.Value, value)).Map(static row => row.Entry);

        public Option<TModel> Value(object? selected) =>
            Rows.Find(row => ReferenceEquals(row.Entry, selected)).Map(static row => row.Item.Value);
    }
}
