using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record EntryForm {
    public sealed record SingleLine : EntryForm;

    public sealed record MultiLine(int Visible, bool Rich) : EntryForm;

    public sealed record Secret : EntryForm;
}

[Union]
public abstract partial record ReadoutSource<TValue> where TValue : notnull {
    public sealed record Model(ReadModel<TValue> Read) : ReadoutSource<TValue>;

    public sealed record Cell(Atom<TValue> Held) : ReadoutSource<TValue>;

    public sealed record Scoped(Func<RowScope, IO<TValue>> Read) : ReadoutSource<TValue>;
}

public sealed record Entry(Control Shown, TextControl Field, Keys Commit, Func<string> Read, Action<string> Write);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TextRows {
    // --- [ENTRIES]
    public static ControlRow Field(string label, string value, EntryForm form, Func<Edit<string>, IO<Unit>> edited) => Entered(label, value, form, None, edited);

    public static ControlRow Path(string label, string value, FileBrowse action, Seq<FileTypeRow> types, Func<Edit<string>, IO<Unit>> edited) =>
        Entered(label, value, new EntryForm.SingleLine(), Some((action, types)), edited);

    public static IO<Entry> Line() =>
        from box in use(static () => new TextBox())
        select new Entry(box, box, Keys.Enter, () => box.Text, text => box.Text = text);

    public static IO<Entry> Area(Option<int> visible) =>
        from area in use(static () => new TextArea())
        from fitted in IO.lift(() => {
            (area.Wrap, area.AcceptsReturn, area.AcceptsTab, area.SpellCheck) = (true, true, false, false);
            _ = visible.Iter(lines => area.Height = (int)MathF.Ceiling(area.Font.LineHeight * lines));
        })
        select new Entry(area, area, Application.Instance.CommonModifier | Keys.Enter, () => area.Text, text => area.Text = text);

    private static IO<Entry> Rich(int visible) =>
        from rich in use(static () => new RichTextAreaWithAlternateText())
        from fitted in IO.lift(() => {
            (rich.HideAlternateTextWhenValueChanges, rich.SelectAllOnGotFocus, rich.Height) = (true, false, (int)MathF.Ceiling(rich.RichTextArea.Font.LineHeight * visible));
        })
        select new Entry(rich, rich.RichTextArea, Application.Instance.CommonModifier | Keys.Enter, () => rich.RichTextArea.Rtf, rtf => rich.RichTextArea.Rtf = rtf);

    private static IO<Entry> Secret() =>
        from box in use(static () => new PasswordBox())
        select new Entry(box, box, Keys.Enter, () => box.Text, text => box.Text = text);

    public static IO<IO<Unit>> Bound(Entry entry, IO<(string Value, Func<Edit<string>, IO<Unit>> Edited)> latest, IPlugInLifetime owner) =>
        from scope in IO.env
        from changing in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
        let previewing = Products.Swapped(changing, Subscriptions.Host<EventArgs>(h => entry.Field.TextChanged += h, h => entry.Field.TextChanged -= h)
            .Inline(_ => IO.lift(entry.Read).Bind(typed => Edited(latest, Edits.Preview(typed))), owner)).Under(scope)
        from previewed in previewing
        let shown = latest.Bind(current => (Products.Cleared(changing).Under(scope)
            >> IO.lift(entry.Read).Bind(text => when(!string.Equals(text, current.Value, StringComparison.Ordinal), IO.lift(() => entry.Write(current.Value))).As()))
            .Finally(previewing))
        let committed = IO.lift(entry.Read).Bind(typed => Edited(latest, Edits.Commit(typed)))
        let answers = HashMap((entry.Commit, committed), (Keys.Escape, Edited(latest, Edits.Cancel<string>()).Bind(_ => shown)))
        from keys in Subscriptions.During<KeyEventArgs>(h => entry.Field.KeyDown += h, h => entry.Field.KeyDown -= h).Inline(args => Answered(args, answers), owner)
        from lost in Subscriptions.Host<EventArgs>(h => entry.Field.LostFocus += h, h => entry.Field.LostFocus -= h).Inline(_ => committed, owner)
        select shown;

    private static IO<Unit> Edited(IO<(string Value, Func<Edit<string>, IO<Unit>> Edited)> latest, Edit<string> edit) => latest.Bind(current => current.Edited(edit));

    private static IO<Unit> Answered(KeyEventArgs args, HashMap<Keys, IO<Unit>> answers) =>
        answers.Find(args.KeyData)
            .Traverse(answer => IO.lift(() => args.Handled = true).Bind(_ => answer)).As()
            .Map(unit);

    // --- [LIVE]
    public static ControlRow Readout<TState>(string label, ReadoutSource<TState> source, Func<TState, string> format) where TState : notnull =>
        new ControlRow.Bound<(ReadoutSource<TState> Source, Func<TState, string> Format)>(label, RowShape.Inline, (source, format), static (latest, scope) =>
            from row in latest
            from live in Live(row.Row.Source,
                state =>
                    from label in use(() => new Label { Text = row.Row.Format(state), TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Wrap = WrapMode.None })
                    from faced in IO.lift(() => { label.Font = HostPlatform.Current.Digits(label.Font); })
                    select label,
                (label, state) => latest.Bind(current => IO.lift(() => { label.Text = current.Row.Format(state); })), scope)
            select new RowCells(live, IO.pure(unit)));

    public static ControlRow Progress<T>(string label, ReadoutSource<Loading<T>> source) where T : notnull =>
        new ControlRow.Bound<ReadoutSource<Loading<T>>>(label, RowShape.Inline, source, static (latest, scope) =>
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
        from followed in ViewOps.WhileLoaded(control, Followed(source, show, control, scope), scope.Owner)
        select control;

    private static IO<Unit> Followed<TState, TControl>(ReadoutSource<TState> source, Func<TControl, TState, IO<Unit>> show, TControl control, RowScope scope)
        where TState : notnull
        where TControl : Control =>
        from watched in Watched(source, scope)
        from post in Mailboxes.Idle<Unit, TState>(changed => changed.Find(unit).Traverse(state => show(control, state)).As().Map(static _ => unit))(scope.Owner)
        from current in post(unit, watched.Value)
        from followed in watched.Changes.Inline(state => post(unit, state), scope.Owner)
        select unit;

    // --- [REALIZE]
    private static ControlRow.Bound<(string Value, EntryForm Form, Option<(FileBrowse Action, Seq<FileTypeRow> Types)> Browse, Func<Edit<string>, IO<Unit>> Edited)> Entered(
        string label, string value, EntryForm form, Option<(FileBrowse Action, Seq<FileTypeRow> Types)> browse, Func<Edit<string>, IO<Unit>> edited) =>
        new(
            label, RowShape.Inline, (value, form, browse, edited), static (latest, scope) =>
                from row in latest
                from entry in row.Row.Form.Switch(
                    singleLine: static _ => Line(),
                    multiLine: static multi => multi.Rich ? Rich(multi.Visible) : Area(Some(multi.Visible)),
                    secret: static _ => Secret())
                from shown in Bound(entry, latest.Map(static current => (current.Row.Value, current.Row.Edited)), scope.Owner)
                from picked in row.Row.Browse.Traverse(asked =>
                    Browsed(new LocalizeStringPair(row.Label, RowText.Localize(row.Label, scope.Owner).Local), asked, entry.Field,
                        typed => latest.Bind(current => current.Row.Edited(Edits.Commit(typed))), scope)).As()
                select new RowCells(entry.Shown, shown) { Pick = picked });

    private static IO<Unit> Indicated<T>(ProgressBar bar, Loading<T> work) where T : notnull =>
        IO.lift(() => {
            (bar.Visible, bar.Indeterminate, bar.MaxValue, bar.Value) = work.Switch(
                pending: static pending => pending.Work.Match(
                    Some: static progress => (true, false, progress.Positions.MaxValue, progress.Positions.Value),
                    None: static () => (true, true, 1, 0)),
                ready: static _ => (false, false, 1, 0),
                failed: static _ => (false, false, 1, 0));
        });

    private static IO<Control> Browsed(
        LocalizeStringPair title, (FileBrowse Action, Seq<FileTypeRow> Types) browse, TextControl box, Func<string, IO<Unit>> commit, RowScope scope) =>
        from picked in CommandRows.Button(
            new Command<Invoke>.Run(new CommandFace("text-browse", "Browse") { Icon = Some(new IconResource(Assets.Rhino.Assembly, ResourceIds.OpenSvg)) }, None,
                new Invoke.Effect(held =>
                    from current in IO.lift(() => box.Text)
                    from path in Asked(browse, title, current, held)
                    from committed in path.Match(Some: commit, None: static () => IO.pure(unit))
                    select committed)),
            IconSlot.Small, scope)
        from dropped in DataTransfer.Drops(box, browse.Types, commit, scope.Owner)
        select picked.Control;

    private static IO<Option<string>> Asked((FileBrowse Action, Seq<FileTypeRow> Types) browse, LocalizeStringPair title, string current, RowScope scope) =>
        browse.Action.Asked(title, current, browse.Types, scope).Catch(ErrorOps.Cancellation, static _ => IO.pure(Option<string>.None));

    private static IO<(TState Value, HostEvent<TState> Changes)> Watched<TState>(ReadoutSource<TState> source, RowScope scope) where TState : notnull =>
        source.Switch(scope,
            model: static (held, model) =>
                from document in held.DocumentIO
                from watched in model.Read.Watch(document)
                select (watched.Value, watched.Changed.Choose(static change => Some(change.After))),
            cell: static (_, cell) => IO.lift(() => (cell.Held.Value, Subscriptions.Changed(cell.Held))),
            scoped: static (held, reading) => reading.Read(held).Map(value => (value, held.Listened(reading.Read))));
}
