using Eto;
using Eto.Forms;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.RhinoCommon.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record EntryForm {
    public sealed record SingleLine : EntryForm;

    public sealed record MultiLine(int Visible) : EntryForm;

    public sealed record Rich(int Visible) : EntryForm;

    public sealed record Secret : EntryForm;
}

[Union]
public abstract partial record ReadoutSource<TValue> where TValue : notnull {
    public sealed record Model(ReadModel<TValue> Read) : ReadoutSource<TValue>;

    public sealed record Cell(Atom<TValue> Held) : ReadoutSource<TValue>;

    public sealed record Scoped(Func<RowScope, IO<TValue>> Read) : ReadoutSource<TValue>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TextRows {
    // --- [ENTRIES]
    public static ControlRow Field(string label, string value, EntryForm form, Func<Edit<string>, IO<Unit>> edited) => Entered(label, value, form, None, edited);

    public static ControlRow Path(string label, string value, FileAction action, Seq<FileTypeRow> types, Func<Edit<string>, IO<Unit>> edited) =>
        Entered(label, value, new EntryForm.SingleLine(), Some((action, types)), edited);

    // --- [LIVE]
    public static ControlRow Readout<TState>(string label, ReadoutSource<TState> source, Func<TState, string> format) where TState : notnull =>
        new ControlRow.Bound<(ReadoutSource<TState> Source, Func<TState, string> Format)>(label, RowShape.Inline, (source, format), static (latest, scope) =>
            from row in latest
            from live in Live(row.Row.Source,
                state => use(() => new SceneView(scope, Displayed(row.Row.Format(state)))),
                (view, state) => latest.Bind(current => view.Show(Displayed(current.Row.Format(state)))), scope)
            select new RowCells(live, IO.pure(unit)));

    public static ControlRow Progress<T>(string label, ReadoutSource<Content<T>> source) where T : notnull =>
        new ControlRow.Bound<ReadoutSource<Content<T>>>(label, RowShape.Inline, source, static (latest, scope) =>
            from row in latest
            from live in Live(row.Row, static _ =>
                from bar in use(static () => new ProgressBar())
                from minimum in IO.lift(() => bar.MinValue = 0)
                select bar, Indicated, scope)
            select new RowCells(live, IO.pure(unit)));

    internal static IO<TControl> Live<TState, TControl>(ReadoutSource<TState> source, Func<TState, IO<TControl>> build, Func<TControl, TState, IO<Unit>> show, RowScope scope)
        where TState : notnull
        where TControl : Control =>
        from first in Watched(source, scope).Map(static watched => watched.Value).Bracket()
        from control in build(first)
        from shown in show(control, first)
        from owned in use(static () => EnvIO.New(token: CancellationToken.None))
        from active in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
        from loaded in Subscriptions.Host<EventArgs>(h => control.Load += h, h => control.Load -= h)
            .Inline(_ => Products.Cleared(active).Bind(_ => Products.Swapped(active, Followed(source, show, control, scope))).Under(owned), scope.Owner)
        from unloaded in Subscriptions.Host<EventArgs>(h => control.UnLoad += h, h => control.UnLoad -= h).Inline(_ => Products.Cleared(active).Under(owned), scope.Owner)
        select control;

    private static IO<Unit> Followed<TState, TControl>(ReadoutSource<TState> source, Func<TControl, TState, IO<Unit>> show, TControl control, RowScope scope)
        where TState : notnull
        where TControl : Control =>
        from watched in Watched(source, scope)
        from post in Mailboxes.Idle<Unit, TState>(changed => changed.Find(unit).Traverse(state => show(control, state)).As().Map(static _ => unit))(scope.Owner)
        from current in post(unit, watched.Value)
        from followed in watched.Changes.Inline(state => post(unit, state), scope.Owner)
        select unit;

    private static Node.Text Displayed(string text) => new([text]) { Role = PanelRoles.Readout, MaxLines = Some(1) };

    // --- [REALIZE]
    private static ControlRow.Bound<(string Value, EntryForm Form, Option<(FileAction Action, Seq<FileTypeRow> Types)> Browse, Func<Edit<string>, IO<Unit>> Edited)> Entered(
        string label, string value, EntryForm form, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse, Func<Edit<string>, IO<Unit>> edited) =>
        new(
            label, RowShape.Inline, (value, form, browse, edited), static (latest, scope) =>
                from row in latest
                from entry in row.Row.Form.Switch(
                    singleLine: static _ => HostInput.Line(),
                    multiLine: static multi => HostInput.Area(Some(multi.Visible)),
                    rich: static rich => Rich(rich.Visible),
                    secret: static _ => Secret())
                from shown in HostInput.Bound(entry, latest.Map(static current => (current.Row.Value, current.Row.Edited)), scope.Owner)
                from picked in row.Row.Browse.Traverse(asked =>
                    Browsed(new LocalizeStringPair(row.Label, RowText.Localize(row.Label, scope.Owner).Local), asked, entry.Field,
                        typed => latest.Bind(current => current.Row.Edited(Edit.Commit(typed))), scope)).As()
                select new RowCells(entry.Shown, shown) { Pick = picked });

    private static IO<HostInput.Entry> Rich(int visible) =>
        from rich in use(static () => new RichTextAreaWithAlternateText())
        from fitted in IO.lift(() => {
            (rich.HideAlternateTextWhenValueChanges, rich.SelectAllOnGotFocus, rich.Height) = (true, false, (int)MathF.Ceiling(rich.RichTextArea.Font.LineHeight * visible));
        })
        select new HostInput.Entry(rich, rich.RichTextArea, Application.Instance.CommonModifier | Keys.Enter, () => rich.RichTextArea.Rtf, rtf => rich.RichTextArea.Rtf = rtf);

    private static IO<HostInput.Entry> Secret() =>
        from box in use(static () => new PasswordBox())
        select new HostInput.Entry(box, box, Keys.Enter, () => box.Text, text => box.Text = text);

    private static IO<Unit> Indicated<T>(ProgressBar bar, Content<T> work) where T : notnull =>
        IO.lift(() => {
            (bar.Visible, bar.Indeterminate, bar.MaxValue, bar.Value) = work.Switch(
                pending: static pending => pending.Work.Match(
                    Some: static progress => (true, false, progress.Positions.MaxValue, progress.Positions.Value),
                    None: static () => (true, true, 1, 0)),
                ready: static _ => (false, false, 1, 0),
                empty: static _ => (false, false, 1, 0),
                failed: static _ => (false, false, 1, 0));
        });

    private static IO<Control> Browsed(
        LocalizeStringPair title, (FileAction Action, Seq<FileTypeRow> Types) browse, TextControl box, Func<string, IO<Unit>> commit, RowScope scope) =>
        from picked in CommandRows.Button(
            new Command<RowRule, Invoke>.Run(new CommandFace("text-browse", "Browse") { Glyph = Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.OpenSvg)) }, None,
                new Invoke.Effect(held =>
                    from current in IO.lift(() => box.Text)
                    from path in Asked(browse, title, current, held)
                    from committed in path.Match(Some: commit, None: static () => IO.pure(unit))
                    select committed)),
            IconSlot.Small, scope)
        from dropped in DataTransfer.Drops(box, browse.Types, commit, scope.Owner)
        select picked.Control;

    private static IO<Option<string>> Asked((FileAction Action, Seq<FileTypeRow> Types) browse, LocalizeStringPair title, string current, RowScope scope) =>
        HostDialogs.Filter(browse.Types.Map(static row => (row.Description, row.Extensions))) switch {
            var filter => (browse.Action switch {
                FileAction.OpenFile => HostDialogs.ShowOpenDialog(new Rhino.UI.OpenFileDialog { Title = title.Local, FileName = current, Filter = filter })
                    .Map(static picked => picked.Head),
                FileAction.SaveFile => HostDialogs.ShowSaveDialog(new Rhino.UI.SaveFileDialog {
                    Title = title.Local, FileName = current, Filter = filter,
                    DefaultExt = Conversions.Unset(browse.Types.Head.Bind(static row => row.Extensions.Head)),
                })
                    .Map(static picked => Some(picked)),
                FileAction.SelectFolder =>
                    from document in scope.DocumentIO
                    from picked in HostDialogs.ShowSelectFolderDialog(document, title, Conversions.Present(current))
                    select Some(picked),
            }).Catch(ErrorOps.Cancellation, static _ => IO.pure(Option<string>.None)),
        };

    private static IO<(TState Value, HostEvent<TState> Changes)> Watched<TState>(ReadoutSource<TState> source, RowScope scope) where TState : notnull =>
        source.Switch(scope,
            model: static (held, model) =>
                from document in held.DocumentIO
                from watched in model.Read.Watch(document)
                select (watched.Value, watched.Changed.Choose(static change => Some(change.After))),
            cell: static (_, cell) => IO.lift(() => (cell.Held.Value, Subscriptions.Changed(cell.Held))),
            scoped: static (held, reading) => reading.Read(held).Map(value => (value, held.Listened(reading.Read))));
}
