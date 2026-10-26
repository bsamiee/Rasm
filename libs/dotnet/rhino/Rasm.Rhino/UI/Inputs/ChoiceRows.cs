using System.Globalization;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChoiceItem<TModel>(TModel Value, string Caption, Option<GlyphSource> Glyph) where TModel : notnull;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ChoiceRows {
    // --- [ITEMS]
    public static Seq<ChoiceItem<TValue>> Items<TValue>(Option<Func<TValue, GlyphSource>> glyph, IPlugInSink plugIn)
        where TValue : ISmartEnum<string, TValue, ValidationError>, ICaptioned =>
        toSeq(TValue.Items).Map(item => new ChoiceItem<TValue>(item, RowText.Localize(item.Caption, plugIn).Local, glyph.Map(read => read(item)))).Strict();

    public static Seq<ChoiceItem<TEnum>> Members<TEnum>(Func<TEnum, string> caption, IPlugInSink plugIn) where TEnum : struct, Enum =>
        toSeq(Enum.GetValues<TEnum>()).Map(member => new ChoiceItem<TEnum>(member, RowText.Localize(caption(member), plugIn).Local, None)).Strict();

    public static Seq<ChoiceItem<Option<TModel>>> Optional<TModel>(string absent, Seq<ChoiceItem<TModel>> items, IPlugInSink plugIn) where TModel : notnull =>
        new ChoiceItem<Option<TModel>>(None, RowText.Localize(absent, plugIn).Local, None)
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
                edit.Shown, Some<RowEdit>(edit), Some(Labelled([], []))), rules);

    public static ControlRow Flags<TRecord>(
        RowSource<TRecord> source, string caption, string help, IterableNE<(RowField<TRecord> Field, Lens<TRecord, bool> Lens, GlyphSource Glyph)> members, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, members.Map(static member => member.Field), caption, help, RowShape.Inline, (scope, retain) =>
            from site in IO.pure(new CallbackSite(scope.Sink, typeof(SegmentedButton), nameof(SegmentedButton.SelectedItemsChanged)))
            from strip in Segmented(
                toSeq(members.Map(member => new ChoiceItem<RowField<TRecord>>(member.Field, RowText.Localize(member.Field.Caption, scope.Sink).Local, Some(member.Glyph)))),
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
                Some(Context(strip.Control, [], []))), rules);

    // --- [CONTROLS]
    public static IO<(DropDown Control, Func<Option<TModel>, IO<Unit>> Show)> Popup<TModel>(
        Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInSink sink, Action<IDisposable> retain) where TModel : notnull =>
        DisposalOps.Acquire<(DropDown Control, Func<Option<TModel>, IO<Unit>> Show)>(keep =>
            from site in IO.pure(new CallbackSite(sink, typeof(DropDown), nameof(DropDown.SelectedValueChanged)))
            from popup in Popped(items, site, keep)
            from show in Commits(h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h,
                popup.Picked, popup.Write, commit, site, keep)
            select (popup.Control, show), retain);

    public static IO<(Control Control, Func<Option<TModel>, IO<Unit>> Show)> Strip<TModel>(
        Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInSink sink, Action<IDisposable> retain) where TModel : notnull {
        const int segmentLimit = 5;
        return DisposalOps.Acquire<(Control Control, Func<Option<TModel>, IO<Unit>> Show)>(keep => items.Count <= segmentLimit
            ? from site in IO.pure(new CallbackSite(sink, typeof(SegmentedButton), nameof(SegmentedButton.SelectedItemsChanged)))
              from strip in Segmented(items, SegmentedSelectionMode.Single, site, keep)
              from show in Commits(h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h,
                  strip.Picked.Map(static picks => picks.Head), value => strip.Write(value.Map(static one => Seq(one))), commit, site, keep)
              select ((Control)strip.Control, show)
            : Popup(items, commit, sink, keep).Map(static popup => ((Control)popup.Control, popup.Show)), retain);
    }

    // --- [MENUS]
    public static Func<Seq<Seq<Command<RowRule, Invoke>>>, IO<Unit>, RowScope, Action<IDisposable>, IO<ContextMenu>> Context(
        Control control, Seq<Seq<Command<RowRule, Invoke>>> own, Seq<IO<Unit>> refreshes) =>
        (groups, refresh, scope, retain) => DisposalOps.Acquire(keep =>
            from menu in Labelled(own, refreshes)(groups, refresh, scope, keep)
            from attached in DisposalOps.Change(() => control.ContextMenu, value => control.ContextMenu = value, menu, keep)
            select menu, retain);

    public static Func<Seq<Seq<Command<RowRule, Invoke>>>, IO<Unit>, RowScope, Action<IDisposable>, IO<ContextMenu>> Labelled(
        Seq<Seq<Command<RowRule, Invoke>>> own, Seq<IO<Unit>> refreshes) =>
        (groups, refresh, scope, retain) => DisposalOps.Acquire(keep =>
            from made in CommandRows.Menu(own + groups, scope, keep)
            from opened in Subscriptions.Attach(h => made.Menu.Opening += h, h => made.Menu.Opening -= h,
                Callbacks.Handler<EventArgs>(_ => Callbacks.Each(refreshes.Add(refresh)).Map(unit), new CallbackSite(scope.Sink, typeof(ContextMenu), nameof(ContextMenu.Opening))), keep)
            select made.Menu, retain);

    // --- [SEARCH]
    public static IO<SearchBox> Search(string placeholder, Func<Func<string, bool>, IO<Unit>> filter, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from field in DisposalOps.Acquire(IO.lift(static () => new SearchBox()), keep)
            from faced in IO.lift(() => field.PlaceholderText = placeholder)
            from attached in Subscriptions.Attach(h => field.TextChanged += h, h => field.TextChanged -= h,
                Callbacks.Handler<EventArgs>(
                    _ => IO.lift(() => field.Text.Trim()).Bind(query => filter(text => RowText.Culture.CompareInfo.IndexOf(text, query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)),
                    new CallbackSite(sink, typeof(SearchBox), nameof(SearchBox.TextChanged))), keep)
            select field, retain);

    // --- [REALIZE]
    private static IO<(DropDown Control, Func<Option<TModel>, IO<Unit>> Write, IO<Option<TModel>> Picked, Func<Seq<ChoiceItem<TModel>>, IO<bool>> Relist)> Popped<TModel>(
        Seq<ChoiceItem<TModel>> items, CallbackSite site, Action<IDisposable> retain) where TModel : notnull =>
        from popup in DisposalOps.Acquire(IO.lift(static () => new DropDown()), retain)
        from varies in IO.lift(static () => new ListItem { Text = RowEdit.Varies })
        from held in IO.lift(static () => Ref(Option<Listing<TModel>>.None))
        from owned in DisposalOps.Acquire(IO.lift(() => new Disposal<Ref<Option<Listing<TModel>>>>(held, static cell =>
            atomicIO(() => { Option<Listing<TModel>> prior = cell.Value; cell.Value = None; return prior; })
                .Bind(static prior => IO.lift(() => prior.Iter(static listing => listing.Themed.Dispose()))).Uninterruptible().Run())), retain)
        let replace = (Func<Seq<ChoiceItem<TModel>>, IO<Unit>>)(roster => DisposalOps.Acquire(
            keep => Listed(popup, roster, site, keep),
            (rows, release) => atomicIO(() => {
                Option<Listing<TModel>> prior = held.Value;
                held.Value = owned.Held.Map(_ => new Listing<TModel>(rows, release)) | prior;
                return owned.Held.ToFin(Errors.Cancelled).Map(_ => prior);
            }).Bind(IO.lift).Bind(static prior => IO.lift(() => prior.Iter(static listing => listing.Themed.Dispose())))).Map(static _ => unit))
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
        from themed in DisposalOps.Acquire(keep => rows.TraverseM(row => row.Item.Glyph.Traverse(glyph =>
            Icons.Themed(site.Sink, glyph, IconSlot.DropDownItem, HostTheme.In(control), icon => row.Entry.Image = icon, keep)).As()).As(), retain)
        select rows;

    private static IO<(SegmentedButton Control, Func<Option<Seq<TModel>>, IO<Unit>> Write, IO<Seq<TModel>> Picked)> Segmented<TModel>(
        Seq<ChoiceItem<TModel>> items, SegmentedSelectionMode mode, CallbackSite site, Action<IDisposable> retain) where TModel : notnull =>
        from strip in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton()), retain)
        from selected in IO.lift(() => strip.SelectionMode = mode)
        from rows in items.TraverseM(item =>
            from segment in ButtonRows.Segment<ButtonSegmentedItem>(retain)
            from faced in IO.lift(() => { (segment.Text, segment.ToolTip) = (item.Glyph.Map(static _ => "").IfNone(item.Caption), item.Caption); })
            select (Item: item, Segment: segment)).As()
        from added in IO.lift(() => strip.Items.AddRange(rows.Map(static row => (SegmentedItem)row.Segment)))
        from themed in DisposalOps.Acquire(keep => rows.TraverseM(row => row.Item.Glyph.Traverse(glyph =>
            Icons.Themed(site.Sink, glyph, IconSlot.PanelButton, HostTheme.In(strip), icon => row.Segment.Image = icon, keep)).As()).As(), retain)
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

    private sealed record Listing<TModel>(Seq<(ChoiceItem<TModel> Item, ImageListItem Entry)> Rows, IDisposable Themed) where TModel : notnull {
        public Seq<object> Entries => Rows.Map(static row => (object)row.Entry);

        public Option<ImageListItem> Entry(TModel value) =>
            Rows.Find(row => EqualityComparer<TModel>.Default.Equals(row.Item.Value, value)).Map(static row => row.Entry);

        public Option<TModel> Value(object? selected) =>
            Rows.Find(row => ReferenceEquals(row.Entry, selected)).Map(static row => row.Item.Value);
    }
}
