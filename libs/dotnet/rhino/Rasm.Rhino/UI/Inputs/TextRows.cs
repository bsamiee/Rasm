using Eto;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

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
        new ControlRow.Bound<(ReadoutSource<TState> Source, Func<TState, string> Format)>(label, RowShape.Inline, (source, format), static (latest, scope, retain) =>
            from row in latest
            from live in Live(row.Row.Source,
                (state, keep) => DisposalOps.Acquire(IO.lift(() => new SceneView(scope, Displayed(row.Row.Format(state)))), keep),
                (view, state) => latest.Bind(current => view.Show(Displayed(current.Row.Format(state)))), scope, retain)
            select new RowCells(live, IO.pure(unit)));

    public static ControlRow Progress<T>(string label, ReadoutSource<Content<T>> source) where T : notnull =>
        new ControlRow.Bound<ReadoutSource<Content<T>>>(label, RowShape.Inline, source, static (latest, scope, retain) =>
            from row in latest
            from live in Live(row.Row, static (_, keep) =>
                from bar in DisposalOps.Acquire(IO.lift(static () => new ProgressBar()), keep)
                from minimum in IO.lift(() => bar.MinValue = 0)
                select bar, Indicated, scope, retain)
            select new RowCells(live, IO.pure(unit)));

    internal static IO<TControl> Live<TState, TControl>(
        ReadoutSource<TState> source, Func<TState, Action<IDisposable>, IO<TControl>> build, Func<TControl, TState, IO<Unit>> show, RowScope scope, Action<IDisposable> retain)
        where TState : notnull
        where TControl : Control =>
        DisposalOps.Acquire(owned =>
            from control in DisposalOps.Within<IDisposable, TControl>(temporary =>
                from first in Watched(source, scope, temporary)
                from built in build(first.Value, owned)
                from shown in show(built, first.Value)
                select built, DisposalOps.Release)
            from active in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), owned)
            from loaded in Subscriptions.Attach(h => control.Load += h, h => control.Load -= h,
                Callbacks.Handler<EventArgs>(_ => DisposalOps.Acquire(keep => Followed(source, show, control, scope, keep), (_, lease) => active.Retain(lease)),
                    new CallbackSite(scope.Sink, control.GetType(), nameof(Control.Load))), owned)
            from unloaded in Subscriptions.Attach(h => control.UnLoad += h, h => control.UnLoad -= h,
                Callbacks.Handler<EventArgs>(_ => active.Drain, new CallbackSite(scope.Sink, control.GetType(), nameof(Control.UnLoad))), owned)
            select control, retain);

    private static IO<Unit> Followed<TState, TControl>(ReadoutSource<TState> source, Func<TControl, TState, IO<Unit>> show, TControl control, RowScope scope, Action<IDisposable> keep)
        where TState : notnull
        where TControl : Control =>
        from watched in Watched(source, scope, keep)
        from post in Subscriptions.Idle<Unit, TState>(changed => changed.Find(unit).Traverse(state => show(control, state)).As().Map(static _ => unit))(
            new CallbackSite(scope.Sink, control.GetType(), nameof(Live)), keep)
        from current in post((unit, watched.Value))
        from followed in watched.Changes.Inline(state => post((unit, state)), scope.Sink, keep)
        select unit;

    private static Node.Text Displayed(string text) => new([text]) { Role = PanelRoles.Readout, MaxLines = Some(1) };

    // --- [REALIZE]
    private static ControlRow.Bound<(string Value, EntryForm Form, Option<(FileAction Action, Seq<FileTypeRow> Types)> Browse, Func<Edit<string>, IO<Unit>> Edited)> Entered(
        string label, string value, EntryForm form, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse, Func<Edit<string>, IO<Unit>> edited) =>
        new(
            label, RowShape.Inline, (value, form, browse, edited), static (latest, scope, retain) =>
                from row in latest
                from entry in row.Row.Form.Switch(retain,
                    singleLine: static (keep, _) => Line(keep),
                    multiLine: static (keep, multi) => Area(multi.Visible, keep),
                    rich: static (keep, rich) => Rich(rich.Visible, keep),
                    secret: static (keep, _) => Secret(keep))
                let edit = (Func<Edit<string>, IO<Unit>>)(change => latest.Bind(current => current.Row.Edited(change)))
                let changed = Callbacks.Handler<EventArgs>(_ => IO.lift(entry.Read).Bind(typed => edit(new Edit<string>.Preview(typed))),
                    new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(TextControl.TextChanged)))
                from previewed in Subscriptions.Attach(h => entry.Field.TextChanged += h, h => entry.Field.TextChanged -= h, changed, retain)
                let shown = latest.Bind(current => Subscriptions.Detached(h => entry.Field.TextChanged += h, h => entry.Field.TextChanged -= h, changed,
                    IO.lift(entry.Read).Bind(text => when(!string.Equals(text, current.Row.Value, StringComparison.Ordinal), IO.lift(() => entry.Write(current.Row.Value))).As())))
                let committed = (Func<string, IO<Unit>>)(typed => edit(new Edit<string>.Commit(typed)))
                let answers = HashMap((entry.Commit, IO.lift(entry.Read).Bind(committed)), (Keys.Escape, edit(new Edit<string>.Cancel()).Bind(_ => shown)))
                from keys in Subscriptions.Attach(h => entry.Field.KeyDown += h, h => entry.Field.KeyDown -= h,
                    Callbacks.Synchronous<KeyEventArgs>(args => Pressed(args, answers), new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(Control.KeyDown))), retain)
                from lost in Subscriptions.Attach(h => entry.Field.LostFocus += h, h => entry.Field.LostFocus -= h,
                    Callbacks.Handler<EventArgs>(_ => IO.lift(entry.Read).Bind(committed), new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(Control.LostFocus))), retain)
                from picked in row.Row.Browse.Traverse(asked =>
                    Browsed(new LocalizeStringPair(row.Label, RowText.Localize(row.Label, scope.Sink).Local), asked, entry.Field, committed, scope, retain)).As()
                select new RowCells(entry.Shown, shown) { Pick = picked });

    private static IO<Entry> Line(Action<IDisposable> retain) =>
        from box in DisposalOps.Acquire(IO.lift(static () => new TextBox()), retain)
        select new Entry(box, box, Keys.Enter, () => box.Text, text => box.Text = text);

    private static IO<Entry> Area(int visible, Action<IDisposable> retain) =>
        from area in DisposalOps.Acquire(IO.lift(static () => new TextArea()), retain)
        from fitted in IO.lift(() => {
            (area.Wrap, area.AcceptsReturn, area.AcceptsTab, area.SpellCheck, area.Height) = (true, true, false, false, (int)MathF.Ceiling(area.Font.LineHeight * visible));
        })
        select new Entry(area, area, Application.Instance.CommonModifier | Keys.Enter, () => area.Text, text => area.Text = text);

    private static IO<Entry> Rich(int visible, Action<IDisposable> retain) =>
        from rich in DisposalOps.Acquire(IO.lift(static () => new RichTextAreaWithAlternateText()), retain)
        from fitted in IO.lift(() => {
            (rich.HideAlternateTextWhenValueChanges, rich.SelectAllOnGotFocus, rich.Height) = (true, false, (int)MathF.Ceiling(rich.RichTextArea.Font.LineHeight * visible));
        })
        select new Entry(rich, rich.RichTextArea, Application.Instance.CommonModifier | Keys.Enter, () => rich.RichTextArea.Rtf, rtf => rich.RichTextArea.Rtf = rtf);

    private static IO<Entry> Secret(Action<IDisposable> retain) =>
        from box in DisposalOps.Acquire(IO.lift(static () => new PasswordBox()), retain)
        select new Entry(box, box, Keys.Enter, () => box.Text, text => box.Text = text);

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

    private static IO<Unit> Pressed(KeyEventArgs args, HashMap<Keys, IO<Unit>> answers) =>
        answers.Find(args.KeyData)
            .Traverse(answer => IO.lift(() => args.Handled = true).Bind(_ => answer)).As()
            .Map(unit);

    private static IO<Control> Browsed(
        LocalizeStringPair title, (FileAction Action, Seq<FileTypeRow> Types) browse, TextControl box, Func<string, IO<Unit>> commit, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from picked in CommandRows.Button(
            new Command<RowRule, Invoke>.Run(new CommandFace("text-browse", "Browse", None, Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.OpenSvg))), None,
                new Invoke.Effect(held =>
                    from current in IO.lift(() => box.Text)
                    from path in Asked(browse, title, current, held)
                    from committed in path.Match(Some: commit, None: static () => IO.pure(unit))
                    select committed)),
            IconSlot.Small, scope, owned)
        from dropped in DataTransfer.Drops(box, browse.Types, commit, scope.Sink, owned)
        select picked.Control, retain);

    private static IO<Option<string>> Asked((FileAction Action, Seq<FileTypeRow> Types) browse, LocalizeStringPair title, string current, RowScope scope) =>
        HostDialogs.Filter(browse.Types.Map(static row => (row.Description, row.Extensions))) switch {
            var filter => (browse.Action switch {
                FileAction.OpenFile => HostDialogs.ShowOpenDialog(new global::Rhino.UI.OpenFileDialog { Title = title.Local, FileName = current, Filter = filter })
                    .Map(static picked => picked.Head),
                FileAction.SaveFile => HostDialogs.ShowSaveDialog(new global::Rhino.UI.SaveFileDialog {
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

    private static IO<(TState Value, HostEvent<TState> Changes)> Watched<TState>(ReadoutSource<TState> source, RowScope scope, Action<IDisposable> retain)
        where TState : notnull =>
        source.Switch((Scope: scope, Retain: retain),
            model: static (held, model) =>
                from document in held.Scope.DocumentIO
                from watched in model.Read.Watch(document, held.Retain)
                select (watched.Value, watched.Changed.Choose(static change => Some(change.After))),
            cell: static (_, cell) => IO.lift(() => (cell.Held.Value, Subscriptions.Changed(cell.Held))),
            scoped: static (held, reading) => reading.Read(held.Scope).Map(value => (value, new HostEvent<TState>(typeof(RowScope), nameof(RowScope.Listen),
                (deliver, _, keep) => held.Scope.Listen(_ => reading.Read(held.Scope).Bind(deliver), keep)))));

    private sealed record Entry(Control Shown, TextControl Field, Keys Commit, Func<string> Read, Action<string> Write);
}
