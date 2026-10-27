using System.Collections.Generic;
using System.Globalization;
using Eto.Forms;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.UI;

namespace Rasm.RhinoCommon.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChoiceItem<TModel>(TModel Value, string Caption) where TModel : notnull {
    public Option<GlyphSource> Glyph { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ChoiceRows {
    // --- [ITEMS]
    public static Seq<ChoiceItem<Option<TModel>>> Optional<TModel>(string absent, Seq<ChoiceItem<TModel>> items, IPlugInLifetime owner) where TModel : notnull =>
        new ChoiceItem<Option<TModel>>(None, RowText.Localize(absent, owner).Local)
            .Cons(items.Map(static item => new ChoiceItem<Option<TModel>>(Some(item.Value), item.Caption) { Glyph = item.Glyph }));

    // --- [ROWS]
    public static ControlRow Choice<TModel>(string label, TModel value, Seq<ChoiceItem<TModel>> items, Func<Edit<TModel>, IO<Unit>> edited) where TModel : notnull =>
        new ControlRow.Bound<(TModel Value, Seq<ChoiceItem<TModel>> Items, Func<Edit<TModel>, IO<Unit>> Edited)>(label, RowShape.Inline, (value, items, edited), static (latest, scope) =>
            from row in latest
            from popup in Popped(row.Row.Items, scope.Owner)
            from change in ControlChange<EventArgs>.Attached(
                Subscriptions.Host<EventArgs>(h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h),
                _ =>
                    from picked in popup.Picked
                    from current in latest
                    from committed in picked.Traverse(pick => current.Row.Edited(Edit.Commit(pick))).As()
                    select unit,
                scope.Owner)
            select new RowCells(popup.Control, latest.Bind(current => change.Detached(popup.Relist(current.Row.Items).Bind(_ => popup.Write(current.Row.Value))))));

    public static ControlRow Flags<TModel>(string label, Seq<TModel> value, Seq<ChoiceItem<TModel>> items, Func<Edit<Seq<TModel>>, IO<Unit>> edited) where TModel : notnull =>
        new ControlRow.Bound<(Seq<TModel> Value, Seq<ChoiceItem<TModel>> Items, Func<Edit<Seq<TModel>>, IO<Unit>> Edited)>(label, RowShape.Inline, (value, items, edited), static (latest, scope) =>
            from row in latest
            from strip in Segmented(row.Row.Items, SegmentedSelectionMode.Multiple, scope.Owner)
            from change in ControlChange<EventArgs>.Attached(
                Subscriptions.Host<EventArgs>(h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h),
                _ =>
                    from picked in strip.Picked
                    from current in latest
                    from committed in current.Row.Edited(Edit.Commit(picked))
                    select unit,
                scope.Owner)
            select new RowCells(strip.Control, latest.Bind(current => change.Detached(strip.Write(current.Row.Value)))));

    // --- [CONTROLS]
    public static IO<(DropDown Control, Func<TModel, IO<Unit>> Show)> Popup<TModel>(Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInLifetime owner)
        where TModel : notnull =>
        from popup in Popped(items, owner)
        from show in Commits(Subscriptions.Host<EventArgs>(h => popup.Control.SelectedValueChanged += h, h => popup.Control.SelectedValueChanged -= h),
            popup.Picked, popup.Write, commit, owner)
        select (popup.Control, show);

    public static IO<(Control Control, Func<TModel, IO<Unit>> Show)> Strip<TModel>(Seq<ChoiceItem<TModel>> items, Func<TModel, IO<Unit>> commit, IPlugInLifetime owner)
        where TModel : notnull {
        const int segmentLimit = 5;
        return items.Count <= segmentLimit
            ? from strip in Segmented(items, SegmentedSelectionMode.Single, owner)
              from show in Commits(Subscriptions.Host<EventArgs>(h => strip.Control.SelectedItemsChanged += h, h => strip.Control.SelectedItemsChanged -= h),
                  strip.Picked.Map(static picks => picks.Head), value => strip.Write(Seq(value)), commit, owner)
              select ((Control)strip.Control, show)
            : Popup(items, commit, owner).Map(static popup => ((Control)popup.Control, popup.Show));
    }

    // --- [SEARCH]
    public static IO<SearchBox> Search(string placeholder, Func<Func<string, bool>, IO<Unit>> filter, IPlugInLifetime owner) =>
        from field in use(static () => new SearchBox())
        from faced in IO.lift(() => field.PlaceholderText = placeholder)
        from attached in Subscriptions.Host<EventArgs>(h => field.TextChanged += h, h => field.TextChanged -= h).Inline(
            _ => IO.lift(() => field.Text.Trim()).Bind(query => filter(text => RowText.Culture.CompareInfo.IndexOf(text, query, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)),
            owner)
        select field;

    // --- [REALIZE]
    private static IO<(DropDown Control, Func<TModel, IO<Unit>> Write, IO<Option<TModel>> Picked, Func<Seq<ChoiceItem<TModel>>, IO<bool>> Relist)> Popped<TModel>(
        Seq<ChoiceItem<TModel>> items, IPlugInLifetime owner) where TModel : notnull =>
        from popup in use(static () => new DropDown())
        from owned in use(static () => EnvIO.New(token: CancellationToken.None))
        from listed in IO.lift(static () => Ref(Option<Lease<Listing<TModel>>>.None))
        let replace = (Func<Seq<ChoiceItem<TModel>>, IO<Unit>>)(roster =>
            Products.Cleared(listed).Bind(_ => Products.Swapped(listed, Listed(popup, roster, owner))).Under(owned).Map(static _ => unit))
        from first in replace(items)
        select (
            popup,
            (Func<TModel, IO<Unit>>)(value => IO.lift(() => listed.Value.Iter(lease =>
                (popup.DataStore, popup.SelectedValue) = (lease.Value.Entries, lease.Value.Entry(value).ValueUnsafe())))),
            IO.lift(() => listed.Value.Bind(lease => lease.Value.Value(popup.SelectedValue))),
            (Func<Seq<ChoiceItem<TModel>>, IO<bool>>)(next => listed.ValueIO.Bind(current => current.Match(
                Some: lease => lease.Value.Rows.Map(static row => row.Item).Equals(next)
                    ? IO.pure(value: false) : replace(next).Map(value: true),
                None: static () => IO.pure(value: false)))));

    private static IO<Listing<TModel>> Listed<TModel>(Control control, Seq<ChoiceItem<TModel>> items, IPlugInLifetime owner) where TModel : notnull =>
        from rows in IO.lift(() => items.Map(static item => (Item: item, Entry: new ImageListItem { Text = item.Caption })).Strict())
        from themed in rows.TraverseM(row => row.Item.Glyph.Traverse(glyph =>
            Icons.Themed(owner, glyph, IconSlot.DropDownItem, HostTheme.In(control), icon => row.Entry.Image = icon)).As()).As()
        select new Listing<TModel>(rows);

    private static IO<(SegmentedButton Control, Func<Seq<TModel>, IO<Unit>> Write, IO<Seq<TModel>> Picked)> Segmented<TModel>(
        Seq<ChoiceItem<TModel>> items, SegmentedSelectionMode mode, IPlugInLifetime owner) where TModel : notnull =>
        from strip in use(static () => new SegmentedButton())
        from selected in IO.lift(() => strip.SelectionMode = mode)
        from rows in items.TraverseM(static item =>
            from segment in ButtonRows.Segment<ButtonSegmentedItem>()
            from faced in IO.lift(() => { (segment.Text, segment.ToolTip) = (item.Glyph.Map(static _ => "").IfNone(item.Caption), item.Caption); })
            select (Item: item, Segment: segment)).As()
        from added in IO.lift(() => strip.Items.AddRange(rows.Map(static row => (SegmentedItem)row.Segment)))
        from themed in rows.TraverseM(row => row.Item.Glyph.Traverse(glyph =>
            Icons.Themed(owner, glyph, IconSlot.PanelButton, HostTheme.In(strip), icon => row.Segment.Image = icon)).As()).As()
        select (
            strip,
            (Func<Seq<TModel>, IO<Unit>>)(value => toHashSet(value) switch {
                var shown => IO.lift(() => rows.Iter(row => row.Segment.Selected = shown.Contains(row.Item.Value))),
            }),
            IO.lift(() => rows.Filter(static row => row.Segment.Selected).Map(static row => row.Item.Value).Strict()));

    private static IO<Func<TModel, IO<Unit>>> Commits<TModel>(
        HostEvent<EventArgs> changes, IO<Option<TModel>> picked, Func<TModel, IO<Unit>> write, Func<TModel, IO<Unit>> commit, IPlugInLifetime owner) where TModel : notnull =>
        from change in ControlChange<EventArgs>.Attached(changes, _ => picked.Bind(pick => pick.Match(Some: commit, None: static () => IO.pure(unit))), owner)
        select (Func<TModel, IO<Unit>>)(value => change.Detached(write(value)));

    private sealed record Listing<TModel>(Seq<(ChoiceItem<TModel> Item, ImageListItem Entry)> Rows) where TModel : notnull {
        public Seq<object> Entries => Rows.Map(static row => (object)row.Entry);

        public Option<ImageListItem> Entry(TModel value) =>
            Rows.Find(row => EqualityComparer<TModel>.Default.Equals(row.Item.Value, value)).Map(static row => row.Entry);

        public Option<TModel> Value(object? selected) =>
            Rows.Find(row => ReferenceEquals(row.Entry, selected)).Map(static row => row.Item.Value);
    }
}
