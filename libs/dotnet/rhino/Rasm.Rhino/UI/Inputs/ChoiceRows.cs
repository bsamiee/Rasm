using System.Globalization;
using Eto.Forms;
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
    public static Seq<ChoiceItem<Option<TModel>>> Optional<TModel>(string absent, Seq<ChoiceItem<TModel>> items, IPlugInSink plugIn) where TModel : notnull =>
        new ChoiceItem<Option<TModel>>(None, RowText.Localize(absent, plugIn).Local, None)
            .Cons(items.Map(static item => new ChoiceItem<Option<TModel>>(Some(item.Value), item.Caption, item.Glyph)));

    // --- [ROWS]
    public static ControlRow Choice<TModel>(string label, TModel value, Seq<ChoiceItem<TModel>> items, Func<Edit<TModel>, IO<Unit>> edited) where TModel : notnull =>
        new ControlRow.Bound<(TModel Value, Seq<ChoiceItem<TModel>> Items, Func<Edit<TModel>, IO<Unit>> Edited)>(label, RowShape.Inline, (value, items, edited), static (latest, scope, retain) =>
            from row in latest
            let site = new CallbackSite(scope.Sink, typeof(DropDown), nameof(DropDown.SelectedValueChanged))
            from popup in Popped(row.Row.Items, site, retain)
            let handler = Callbacks.Handler<EventArgs>(_ =>
                from picked in popup.Picked
                from current in latest
                from committed in picked.Traverse(pick => current.Row.Edited(new Edit<TModel>.Commit(pick))).As()
                select unit, site)
            from attached in Subscriptions.Attach(h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h, handler, retain)
            select new RowCells(popup.Control, latest.Bind(current => Subscriptions.Detached(
                h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h, handler,
                popup.Relist(current.Row.Items).Bind(_ => popup.Write(current.Row.Value))))));

    public static ControlRow Flags<TModel>(string label, Seq<TModel> value, Seq<ChoiceItem<TModel>> items, Func<Edit<Seq<TModel>>, IO<Unit>> edited) where TModel : notnull =>
        new ControlRow.Bound<(Seq<TModel> Value, Seq<ChoiceItem<TModel>> Items, Func<Edit<Seq<TModel>>, IO<Unit>> Edited)>(label, RowShape.Inline, (value, items, edited), static (latest, scope, retain) =>
            from row in latest
            let site = new CallbackSite(scope.Sink, typeof(SegmentedButton), nameof(SegmentedButton.SelectedItemsChanged))
            from strip in Segmented(row.Row.Items, SegmentedSelectionMode.Multiple, site, retain)
            let handler = Callbacks.Handler<EventArgs>(_ =>
                from picked in strip.Picked
                from current in latest
                from committed in current.Row.Edited(new Edit<Seq<TModel>>.Commit(picked))
                select unit, site)
            from attached in Subscriptions.Attach(h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h, handler, retain)
            select new RowCells(strip.Control, latest.Bind(current => Subscriptions.Detached(
                h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h, handler, strip.Write(current.Row.Value)))));

    // --- [CONTROLS]
    public static IO<(DropDown Control, Func<TModel, IO<Unit>> Show)> Popup<TModel>(
        Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInSink sink, Action<IDisposable> retain) where TModel : notnull =>
        DisposalOps.Acquire<(DropDown Control, Func<TModel, IO<Unit>> Show)>(keep =>
            from site in IO.pure(new CallbackSite(sink, typeof(DropDown), nameof(DropDown.SelectedValueChanged)))
            from popup in Popped(items, site, keep)
            from show in Commits(h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h,
                popup.Picked, popup.Write, commit, site, keep)
            select (popup.Control, show), retain);

    public static IO<(Control Control, Func<TModel, IO<Unit>> Show)> Strip<TModel>(
        Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInSink sink, Action<IDisposable> retain) where TModel : notnull {
        const int segmentLimit = 5;
        return DisposalOps.Acquire<(Control Control, Func<TModel, IO<Unit>> Show)>(keep => items.Count <= segmentLimit
            ? from site in IO.pure(new CallbackSite(sink, typeof(SegmentedButton), nameof(SegmentedButton.SelectedItemsChanged)))
              from strip in Segmented(items, SegmentedSelectionMode.Single, site, keep)
              from show in Commits(h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h,
                  strip.Picked.Map(static picks => picks.Head), value => strip.Write(Seq(value)), commit, site, keep)
              select ((Control)strip.Control, show)
            : Popup(items, commit, sink, keep).Map(static popup => ((Control)popup.Control, popup.Show)), retain);
    }

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
    private static IO<(DropDown Control, Func<TModel, IO<Unit>> Write, IO<Option<TModel>> Picked, Func<Seq<ChoiceItem<TModel>>, IO<bool>> Relist)> Popped<TModel>(
        Seq<ChoiceItem<TModel>> items, CallbackSite site, Action<IDisposable> retain) where TModel : notnull =>
        from popup in DisposalOps.Acquire(IO.lift(static () => new DropDown()), retain)
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
            (Func<TModel, IO<Unit>>)(value => held.ValueIO.Bind(current => IO.lift(() => current.Iter(listing =>
                (popup.DataStore, popup.SelectedValue) = (listing.Entries, listing.Entry(value).ValueUnsafe()))))),
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

    private static IO<(SegmentedButton Control, Func<Seq<TModel>, IO<Unit>> Write, IO<Seq<TModel>> Picked)> Segmented<TModel>(
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
            (Func<Seq<TModel>, IO<Unit>>)(value => toHashSet(value) switch {
                var shown => IO.lift(() => rows.Iter(row => row.Segment.Selected = shown.Contains(row.Item.Value))),
            }),
            IO.lift(() => rows.Filter(static row => row.Segment.Selected).Map(static row => row.Item.Value).Strict()));

    private static IO<Func<TModel, IO<Unit>>> Commits<TModel>(
        Action<EventHandler<EventArgs>> add, Action<EventHandler<EventArgs>> remove, IO<Option<TModel>> picked, Func<TModel, IO<Unit>> write,
        Func<TModel, IO<Unit>> commit, CallbackSite site, Action<IDisposable> retain) where TModel : notnull =>
        from handler in IO.pure(Callbacks.Handler<EventArgs>(_ => picked.Bind(pick => pick.Match(Some: commit, None: static () => IO.pure(unit))), site))
        from attached in Subscriptions.Attach(add, remove, handler, retain)
        select (Func<TModel, IO<Unit>>)(value => Subscriptions.Detached(add, remove, handler, write(value)));

    private sealed record Listing<TModel>(Seq<(ChoiceItem<TModel> Item, ImageListItem Entry)> Rows, IDisposable Themed) where TModel : notnull {
        public Seq<object> Entries => Rows.Map(static row => (object)row.Entry);

        public Option<ImageListItem> Entry(TModel value) =>
            Rows.Find(row => EqualityComparer<TModel>.Default.Equals(row.Item.Value, value)).Map(static row => row.Entry);

        public Option<TModel> Value(object? selected) =>
            Rows.Find(row => ReferenceEquals(row.Entry, selected)).Map(static row => row.Item.Value);
    }
}
