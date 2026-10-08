using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChoiceItem<TModel>(TModel Value, string Caption, Option<IGlyph> Glyph) where TModel : notnull;

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
    internal static partial CheckMenuItem ToCheckMenuItem(MenuPick.Item item);
}

public static class ChoiceRows {
    // --- [ITEMS]
    public static Seq<ChoiceItem<TValue>> Items<TValue, TError>(Func<TValue, string> caption, Option<Func<TValue, IGlyph>> glyph, IPlugInSink plugIn)
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
        ControlRow.Of(source, field, RowShape.Inline, scope =>
            from listed in items(scope)
            let site = new CallbackSite(scope.Sink, typeof(DropDown), nameof(DropDown.SelectedValueChanged))
            from popup in Popped(listed, site)
            from bound in DisposalOps.OnFailure(
                RowEdit.Bind(source, IterableNE.create(field),
                    h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h,
                    edit => Callbacks.Handler<EventArgs>(_ =>
                        from picked in popup.Picked
                        from held in scope.Read(source)
                        from committed in picked.Traverse(pick => edit.Commit(lens.Set(pick, held))).As()
                        select unit, site),
                    value => popup.Write(value.Map(lens.Get)), scope, site),
                IO.lift(popup.Release.Dispose))
            from opening in DisposalOps.OnFailure(
                Subscriptions.Attach(h => popup.Control.DropDownOpening += h, h => popup.Control.DropDownOpening -= h,
                    Callbacks.Handler<EventArgs>(
                        _ => items(scope).Bind(popup.Relist).Bind(changed => when(changed, bound.Edit.Shown).As()),
                        new CallbackSite(scope.Sink, typeof(DropDown), nameof(DropDown.DropDownOpening)))),
                DisposalOps.Release(Seq(popup.Release, bound.Release)))
            select new RowCells(
                popup.Control, None, None, None, [], [],
                new RowHelp(None, listed.Find(item => EqualityComparer<TModel>.Default.Equals(item.Value, lens.Get(source.Default))).Map(static item => item.Caption)),
                bound.Edit.Shown, Some<RowEdit>(bound.Edit), Some(Labelled([], [], scope.Sink)),
                DisposalOps.Composite(Seq(popup.Release, bound.Release, opening), site)), rules);

    public static ControlRow Flags<TRecord>(
        RowSource<TRecord> source, string caption, string help, IterableNE<(RowField<TRecord> Field, Lens<TRecord, bool> Lens, IGlyph Glyph)> members, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, members.Map(static member => member.Field), caption, help, RowShape.Inline, scope =>
            from site in IO.pure(new CallbackSite(scope.Sink, typeof(SegmentedButton), nameof(SegmentedButton.SelectedItemsChanged)))
            from strip in Segmented(
                toSeq(members.Map(member => new ChoiceItem<RowField<TRecord>>(member.Field, Wording.English.Shown(member.Field.Caption, scope.Sink), Some(member.Glyph)))),
                SegmentedSelectionMode.Multiple, site)
            from bound in DisposalOps.OnFailure(
                RowEdit.Bind(source, members.Map(static member => member.Field),
                    h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h,
                    edit => Callbacks.Handler<EventArgs>(_ =>
                        from picked in strip.Picked
                        from held in scope.Read(source)
                        from committed in edit.Commit(members.Fold((record, member) => member.Lens.Set(picked.Exists(member.Field.Equals), record), held))
                        select committed, site),
                    value => strip.Write(value.Map(record => members.Filter(member => member.Lens.Get(record)).Map(static member => member.Field).ToSeq())), scope, site),
                IO.lift(strip.Release.Dispose))
            select new RowCells(
                strip.Control, None, None, None, [], [], new RowHelp(None, None), bound.Edit.Shown, Some<RowEdit>(bound.Edit),
                Some(Context(strip.Control, [], [], scope.Sink)),
                DisposalOps.Composite(Seq(strip.Release, bound.Release), site)), rules);

    // --- [CONTROLS]
    public static IO<(DropDown Control, Func<Option<TModel>, IO<Unit>> Show, IDisposable Release)> Popup<TModel>(
        Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInSink sink) where TModel : notnull =>
        from site in IO.pure(new CallbackSite(sink, typeof(DropDown), nameof(DropDown.SelectedValueChanged)))
        from popup in Popped(items, site)
        from bound in Commits(h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h, popup.Picked, popup.Write, commit, popup.Release, site)
        select (popup.Control, bound.Show, bound.Release);

    public static IO<(Control Control, Func<Option<TModel>, IO<Unit>> Show, IDisposable Release)> Strip<TModel>(
        Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInSink sink) where TModel : notnull {
        const int segmentLimit = 5;
        return items.Count <= segmentLimit
            ? from site in IO.pure(new CallbackSite(sink, typeof(SegmentedButton), nameof(SegmentedButton.SelectedItemsChanged)))
              from strip in Segmented(items, SegmentedSelectionMode.Single, site)
              from bound in Commits(h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h,
                  strip.Picked.Map(static picks => picks.Head), value => strip.Write(value.Map(static one => Seq(one))), commit, strip.Release, site)
              select ((Control)strip.Control, bound.Show, bound.Release)
            : Popup(items, commit, sink).Map(static popup => ((Control)popup.Control, popup.Show, popup.Release));
    }

    // --- [MENUS]
    public static IO<(ContextMenu Menu, IDisposable Release)> Menu(
        Seq<Seq<MenuEntry>> groups, Seq<IO<Unit>> refreshes, Option<IO<Seq<MenuPick>>> listing, IPlugInSink sink) =>
        from site in IO.pure(new CallbackSite(sink, typeof(ContextMenu), nameof(ContextMenu.Opening)))
        from entries in Entries(groups, site)
        let held = entries.Bind(static realized => realized)
        from menu in IO.lift(() => new ContextMenu(Joined(entries)))
        from opened in DisposalOps.OnFailure(Opened(menu.Items, h => menu.Opening += h, h => menu.Opening -= h, refreshes, listing, site), Free(held))
        select (menu, DisposalOps.Composite(new Disposal<ContextMenu>(menu, static owned => owned.Dispose()).Cons(held.Map(static entry => entry.Release)).Add(opened), site));

    public static Func<Option<Label>, Seq<Seq<Command>>, IO<Unit>, IO<IDisposable>> Context(Control control, Seq<Seq<MenuEntry>> own, Seq<IO<Unit>> refreshes, IPlugInSink sink) =>
        (_, groups, refresh) => Attached(control, own, refreshes, groups, refresh, sink);

    public static Func<Option<Label>, Seq<Seq<Command>>, IO<Unit>, IO<IDisposable>> Labelled(Seq<Seq<MenuEntry>> own, Seq<IO<Unit>> refreshes, IPlugInSink sink) =>
        (label, groups, refresh) => label.Match(
            Some: held => Attached(held, own, refreshes, groups, refresh, sink),
            None: static () => IO.pure(Thinktecture.Empty.Disposable()));

    private static IO<IDisposable> Attached(Control control, Seq<Seq<MenuEntry>> own, Seq<IO<Unit>> refreshes, Seq<Seq<Command>> groups, IO<Unit> refresh, IPlugInSink sink) =>
        from built in Menu(own + groups.Map(static cluster => cluster.Map(static command => (MenuEntry)command)), refreshes.Add(refresh), None, sink)
        from attached in DisposalOps.OnFailure(IO.lift(() => control.ContextMenu = built.Menu), IO.lift(built.Release.Dispose))
        select DisposalOps.Composite(
            Seq(built.Release, new Disposal<Control>(control, static held => held.ContextMenu = null)),
            new CallbackSite(sink, control.GetType(), nameof(Control.ContextMenu)));

    // --- [SEARCH]
    public static IO<(SearchBox Control, IDisposable Release)> Search(string placeholder, Func<Func<string, bool>, IO<Unit>> filter, IPlugInSink sink) =>
        from field in IO.lift(() => new SearchBox { PlaceholderText = placeholder })
        from attached in Subscriptions.Attach(h => field.TextChanged += h, h => field.TextChanged -= h,
            Callbacks.Handler<EventArgs>(
                _ => IO.lift(() => field.Text.Trim()).Bind(query => filter(text => RowText.Culture.CompareInfo.IndexOf(text, query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)),
                new CallbackSite(sink, typeof(SearchBox), nameof(SearchBox.TextChanged))))
        select (field, attached);

    // --- [REALIZE]
    private static IO<(DropDown Control, Func<Option<TModel>, IO<Unit>> Write, IO<Option<TModel>> Picked, Func<Seq<ChoiceItem<TModel>>, IO<bool>> Relist, IDisposable Release)> Popped<TModel>(
        Seq<ChoiceItem<TModel>> items, CallbackSite site) where TModel : notnull =>
        from popup in IO.lift(static () => new DropDown())
        from varies in IO.lift(static () => new ListItem { Text = RowEdit.Varies })
        from first in Listed(items, site)
        from held in IO.lift(() => Atom(first))
        select (
            popup,
            (Func<Option<TModel>, IO<Unit>>)(value => held.ValueIO.Bind(current => IO.lift(() => {
                (popup.DataStore, popup.SelectedValue) = value.Match(
                    Some: model => (current.Entries, current.Entry(model).ValueUnsafe()),
                    None: () => (varies.Cons(current.Entries), (object?)varies));
            }))),
            held.ValueIO.Bind(current => IO.lift(() => current.Value(popup.SelectedValue))),
            (Func<Seq<ChoiceItem<TModel>>, IO<bool>>)(next => held.ValueIO.Bind(current => current.Rows.Map(static row => row.Item).Equals(next)
                ? IO.pure(value: false)
                : from fresh in Listed(next, site)
                  from swapped in held.SwapIO(_ => fresh)
                  from released in IO.lift(current.Themed.Dispose)
                  select true)),
            (IDisposable)new Disposal<Atom<Listing<TModel>>>(held, static cell => cell.Value.Themed.Dispose()));

    private static IO<Listing<TModel>> Listed<TModel>(Seq<ChoiceItem<TModel>> items, CallbackSite site) where TModel : notnull =>
        from rows in IO.lift(() => items.Map(static item => (Item: item, Entry: new ImageListItem { Text = item.Caption })).Strict())
        from themed in DisposalOps.AcquireAll(
            rows.Bind(row => row.Item.Glyph.Map(glyph => Icons.Themed(site.Sink, glyph, IconSlot.DropDownItem, icon => row.Entry.Image = icon)).ToSeq()), DisposalOps.Release)
        select new Listing<TModel>(rows, DisposalOps.Composite(themed, site));

    private static IO<(SegmentedButton Control, Func<Option<Seq<TModel>>, IO<Unit>> Write, IO<Seq<TModel>> Picked, IDisposable Release)> Segmented<TModel>(
        Seq<ChoiceItem<TModel>> items, SegmentedSelectionMode mode, CallbackSite site) where TModel : notnull =>
        from rows in IO.lift(() => items.Map(static item => (Item: item, Segment: new ButtonSegmentedItem {
            Text = item.Glyph.Map(static _ => "").IfNone(item.Caption), ToolTip = item.Caption,
        })).Strict())
        from strip in IO.lift(() => new SegmentedButton { SelectionMode = mode })
        from added in IO.lift(() => strip.Items.AddRange(rows.Map(static row => (SegmentedItem)row.Segment)))
        from themed in DisposalOps.AcquireAll(
            rows.Bind(row => row.Item.Glyph.Map(glyph => Icons.Themed(site.Sink, glyph, IconSlot.PanelButton, icon => row.Segment.Image = icon)).ToSeq()), DisposalOps.Release)
        select (
            strip,
            (Func<Option<Seq<TModel>>, IO<Unit>>)(value => toHashSet(value.ToSeq().Flatten()) switch {
                var shown => IO.lift(() => rows.Iter(row => row.Segment.Selected = shown.Contains(row.Item.Value))),
            }),
            IO.lift(() => rows.Filter(static row => row.Segment.Selected).Map(static row => row.Item.Value).Strict()),
            DisposalOps.Composite(themed, site));

    private static IO<(Func<Option<TModel>, IO<Unit>> Show, IDisposable Release)> Commits<TModel>(
        Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> remove, IO<Option<TModel>> picked, Func<Option<TModel>, IO<Unit>> write,
        Func<TModel, IO<Unit>> commit, IDisposable release, CallbackSite site) where TModel : notnull =>
        from handler in IO.pure(Callbacks.Handler<EventArgs>(_ => picked.Bind(pick => pick.Match(Some: commit, None: static () => IO.pure(unit))), site))
        from attached in DisposalOps.OnFailure(Subscriptions.Attach(add, remove, handler), IO.lift(release.Dispose))
        select ((Func<Option<TModel>, IO<Unit>>)(value => Subscriptions.Detached(add, remove, handler, write(value))), DisposalOps.Composite(Seq(release, attached), site));

    private static IO<Seq<Seq<(MenuItem Item, IDisposable Release)>>> Entries(Seq<Seq<MenuEntry>> groups, CallbackSite site) =>
        DisposalOps.AcquireAll(
            from grouped in groups select DisposalOps.AcquireAll(from entry in grouped select Entry(entry, site), Free),
            static held => Free(held.Bind(static group => group)));

    private static IO<(MenuItem Item, IDisposable Release)> Entry(MenuEntry entry, CallbackSite site) =>
        entry.Switch(site,
            item: static (_, item) =>
                IO.lift(() => item.Command.CreateMenuItem()).Map(static created => (created, (IDisposable)new Disposal<MenuItem>(created, static held => held.Unbind()))),
            submenu: static (at, submenu) =>
                from children in Entries(submenu.Groups, at)
                from sub in IO.lift(() => new SubMenuItem([.. Joined(children)]) { Text = submenu.Caption })
                select ((MenuItem)sub, DisposalOps.Composite(children.Bind(static realized => realized).Map(static child => child.Release), at)),
            listed: static (at, listed) =>
                from sub in IO.lift(() => new SubMenuItem { Text = listed.Caption })
                from opened in Opened(sub.Items, h => sub.Opening += h, h => sub.Opening -= h, [], Some(listed.Read),
                    new CallbackSite(at.Sink, typeof(SubMenuItem), nameof(SubMenuItem.Opening)))
                select ((MenuItem)sub, opened));

    private static IO<(MenuItem Item, IDisposable Release)> Pick(MenuPick pick, CallbackSite site) =>
        pick.Switch(site,
            item: static (at, item) =>
                from check in IO.lift(() => MenuPickMapper.ToCheckMenuItem(item))
                from clicked in Subscriptions.Attach(h => check.Click += h, h => check.Click -= h,
                    Callbacks.Handler<EventArgs>(_ => item.Run, new CallbackSite(at.Sink, typeof(CheckMenuItem), nameof(CheckMenuItem.Click))))
                select ((MenuItem)check, DisposalOps.Composite(Seq(new Disposal<MenuItem>(check, static held => held.Dispose()), clicked), at)),
            group: static (at, nested) =>
                from picks in DisposalOps.AcquireAll(nested.Picks.Map(child => Pick(child, at)), Free)
                let children = picks.Map(static child => child.Item)
                from sub in IO.lift(() => new SubMenuItem([.. children]) { Text = nested.Caption })
                select ((MenuItem)sub, DisposalOps.Composite(
                    new Disposal<MenuItem>(sub, static held => held.Dispose()).Cons(picks.Map(static child => child.Release)), at)));

    private static IO<IDisposable> Opened(
        MenuItemCollection items, Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> remove,
        Seq<IO<Unit>> refreshes, Option<IO<Seq<MenuPick>>> listing, CallbackSite site) =>
        from shown in IO.lift(static () => Atom(new ListedPicks([], [])))
        from attached in Subscriptions.Attach(add, remove, Callbacks.Handler<EventArgs>(_ => Callbacks.Each(refreshes + (
            from read in listing
            select from taken in shown.SwapIO(static held => new ListedPicks([], held.Shown))
                   from removed in IO.lift(() => taken.Taken.Iter(stale => items.Remove(stale.Item)))
                   from released in Free(taken.Taken)
                   from picks in read
                   from placed in DisposalOps.AcquireAll(picks.Map(pick => Pick(pick, site)), Free)
                   from listed in IO.lift(() => placed.IsEmpty || items.Count == 0
                       ? placed
                       : placed.Add(new SeparatorMenuItem() switch { var line => (line, new Disposal<MenuItem>(line, static held => held.Dispose())) }))
                   from inserted in IO.lift(() => listed.Iter((index, entry) => items.Insert(index, entry.Item)))
                   from kept in shown.SwapIO(held => held with { Shown = listed })
                   select unit).ToSeq()).Map(static _ => unit), site))
        select DisposalOps.Composite(
            Seq(new Disposal<Atom<ListedPicks>>(shown, cell => _ = Callbacks.Answer(Free(cell.Value.Shown), static () => unit, site)), attached), site);

    private static IO<Unit> Free(Seq<(MenuItem Item, IDisposable Release)> held) =>
        DisposalOps.Release(held.Map(static entry => entry.Release));

    private static Seq<MenuItem> Joined(Seq<Seq<(MenuItem Item, IDisposable Release)>> groups) =>
        groups.Map(static group => group.Map(static entry => entry.Item))
            .Filter(static group => !group.IsEmpty)
            .Fold(Seq<MenuItem>(), static (joined, group) => joined.IsEmpty ? group : joined.Add(new SeparatorMenuItem()).Concat(group));

    private sealed record Listing<TModel>(Seq<(ChoiceItem<TModel> Item, ImageListItem Entry)> Rows, IDisposable Themed) where TModel : notnull {
        public Seq<object> Entries => Rows.Map(static row => (object)row.Entry);

        public Option<ImageListItem> Entry(TModel value) =>
            Rows.Find(row => EqualityComparer<TModel>.Default.Equals(row.Item.Value, value)).Map(static row => row.Entry);

        public Option<TModel> Value(object? selected) =>
            Rows.Find(row => ReferenceEquals(row.Entry, selected)).Map(static row => row.Item.Value);
    }

    private sealed record ListedPicks(Seq<(MenuItem Item, IDisposable Release)> Shown, Seq<(MenuItem Item, IDisposable Release)> Taken);
}
