using System.Diagnostics;
using Eto;
using PeterO.Numbers;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TextLines : System.Numerics.IMinMaxValue<TextLines> {
    public static TextLines MinValue { get; } = new(1);
    public static TextLines MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = ValidationResults.OutsideRange(value, MinValue._value, MaxValue._value);
}

[Union]
public abstract partial record EntryForm {
    public sealed record SingleLine : EntryForm;

    public sealed record MultiLine(TextLines Visible) : EntryForm;

    public sealed record Rich(TextLines Visible) : EntryForm;

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
    public static ControlRow Field<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, EntryForm form, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse, RowRules rules)
        where TRecord : notnull =>
        FieldTexts.Of(field.Parameter) switch {
            [var codec] => ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Entered(source, field,
                record => Conversions.Unset(codec.Capture(record).Filter(text => !codec.Absent.Exists(absent => string.Equals(absent, text, StringComparison.Ordinal)))),
                codec.File.IsNone,
                (record, text) => IO.lift(codec.Recall(Conversions.Present(text).IfNone(() => codec.Absent.IfNone(text)))).Bind(static step => step).Map(set => set(record)),
                form, browse, scope, retain), rules),
            var codecs => throw new UnreachableException($"{field.Parameter.Key} holds {codecs.Count} stored texts where an entry row holds one"),
        };

    // --- [PATHS]
    public static ControlRow Path<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, string> lens, FileAction action, Seq<FileTypeRow> types, RowRules rules)
        where TRecord : notnull =>
        Typed(source, field, lens.Get, (record, text) => lens.Set(text, record), new EntryForm.SingleLine(), Some((action, types)), rules);

    // --- [LIVE]
    public static ControlRow Readout<TState>(string caption, string help, ReadoutSource<TState> source, Func<TState, string> format, RowRules rules)
        where TState : notnull =>
        new ControlRow.Readout(RowShape.Inline, (scope, retain) =>
            from live in Live(source,
                static (_, keep) =>
                    from label in DisposalOps.Acquire(IO.lift(static () => new Eto.Forms.Label()), keep)
                    from faced in IO.lift(() => { (label.Wrap, label.TextAlignment, label.Font) = (Eto.Forms.WrapMode.None, Eto.Forms.TextAlignment.Right, HostTheme.Digits(label.Font)); })
                    select label,
                (label, state) => Written(() => label.Text, text => label.Text = text, format(state)), scope, retain)
            select new RowCells(live, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None),
            caption, help, rules);

    public static ControlRow Progress(string caption, string help, ReadoutSource<WorkState> source, RowRules rules) =>
        new ControlRow.Readout(RowShape.Inline, (scope, retain) =>
            from live in Live(source, static (_, keep) =>
                from bar in DisposalOps.Acquire(IO.lift(static () => new Eto.Forms.ProgressBar()), keep)
                from minimum in IO.lift(() => bar.MinValue = 0)
                select bar, Indicated, scope, retain)
            select new RowCells(live, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None),
            caption, help, rules);

    internal static IO<TControl> Live<TState, TControl>(
        ReadoutSource<TState> source, Func<TState, Action<IDisposable>, IO<TControl>> build, Func<TControl, TState, IO<Unit>> show, RowScope scope, Action<IDisposable> retain)
        where TState : notnull
        where TControl : Eto.Forms.Control =>
        DisposalOps.Acquire(owned =>
            from control in DisposalOps.Within<IDisposable, TControl>(temporary =>
                from first in Watched(source, scope, temporary)
                from built in build(first.Value, owned)
                from shown in show(built, first.Value)
                select built, DisposalOps.Release)
            let site = new CallbackSite(scope.Sink, typeof(FrameClocks), nameof(FrameClocks.Sample))
            from active in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), owned)
            from loaded in Subscriptions.Attach(h => control.Load += h, h => control.Load -= h,
                Callbacks.Handler<EventArgs>(_ => DisposalOps.Acquire(keep => Sampled(source, show, control, site, scope, keep), (_, lease) => active.Retain(lease)),
                    new CallbackSite(scope.Sink, control.GetType(), nameof(Eto.Forms.Control.Load))), owned)
            from unloaded in Subscriptions.Attach(h => control.UnLoad += h, h => control.UnLoad -= h,
                Callbacks.Handler<EventArgs>(_ => active.Drain, new CallbackSite(scope.Sink, control.GetType(), nameof(Eto.Forms.Control.UnLoad))), owned)
            select control, retain);

    private static IO<Unit> Sampled<TState, TControl>(
        ReadoutSource<TState> source, Func<TControl, TState, IO<Unit>> show, TControl control, CallbackSite site, RowScope scope, Action<IDisposable> keep)
        where TState : notnull
        where TControl : Eto.Forms.Control =>
        from watched in Watched(source, scope, keep)
        from sampled in FrameClocks.Sample(control, watched.Read.Bind(current => current.Match(Some: state => show(control, state), None: static () => IO.pure(unit))), site, keep)
        select unit;

    // --- [REALIZE]
    public static ControlRow Typed<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Func<TRecord, string> text, Func<TRecord, string, Fin<TRecord>> cross,
        EntryForm form, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Inline,
            (scope, retain) => Entered(source, field, text, previewed: true, (record, typed) => IO.lift(cross(record, typed)), form, browse, scope, retain), rules);

    private static IO<RowCells> Entered<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Func<TRecord, string> text, bool previewed,
        Func<TRecord, string, IO<TRecord>> commit, EntryForm form, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        DisposalOps.Acquire(owned =>
        from entry in form.Switch(owned,
            singleLine: static (keep, _) => Line(keep),
            multiLine: static (keep, multi) => Area(multi.Visible, keep),
            rich: static (keep, rich) => Rich(rich.Visible, keep),
            secret: static (keep, _) => Secret(keep))
        let site = new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(Eto.Forms.TextControl.TextChanged))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            h => entry.Field.TextChanged += h, h => entry.Field.TextChanged -= h,
            edit => Callbacks.Handler<EventArgs>(_ => when(previewed, Previewed(edit, commit, source, entry, scope)).As(), site),
            value =>
                from marked in IO.lift(() => entry.Varied.Iter(mark => mark(value.IsNone)))
                from written in (value.Map(text) | entry.Blank).Traverse(shown => Written(entry.Read, entry.Write, shown)).As()
                select unit,
            scope, site, owned)
        let committed = (Func<string, IO<Unit>>)(typed =>
            from held in scope.Read(source)
            from written in commit(held, typed)
                .Match(Succ: record => bound.Commit(record), Fail: error => error.IsExpected && !error.Is(Errors.Cancelled) ? bound.Commit(error) : IO.fail<Seq<TRecord>>(error))
                .Flatten()
            select unit)
        let answers = HashMap((entry.Commit, IO.lift(entry.Read).Bind(committed)), (Eto.Forms.Keys.Escape, bound.Cancel))
        from keys in Subscriptions.Attach(h => entry.Field.KeyDown += h, h => entry.Field.KeyDown -= h,
            Callbacks.Handler<Eto.Forms.KeyEventArgs>(args => Pressed(args, answers), new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(Eto.Forms.Control.KeyDown))), owned)
        from lost in Subscriptions.Attach(h => entry.Field.LostFocus += h, h => entry.Field.LostFocus -= h,
            Callbacks.Handler<EventArgs>(_ => IO.lift(entry.Read).Bind(committed), new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(Eto.Forms.Control.LostFocus))), owned)
        from picked in browse.Traverse(asked => Browsed(new LocalizeStringPair(field.Caption, field.Wording.Shown(field.Caption, scope.Sink)), asked, entry.Field, committed, scope, owned)).As()
        select new RowCells(
            entry.Shown, None, picked.Map(static held => (Eto.Forms.Control)held), None, [], [], new RowHelp(None, Conversions.Present(text(source.Default))),
            bound.Shown, Some<RowEdit>(bound), Some(ChoiceRows.Context(entry.Shown, [], [], scope.Sink))), retain);

    private static IO<Unit> Previewed<TRecord>(RowEdit<TRecord> edit, Func<TRecord, string, IO<TRecord>> cross, RowSource<TRecord> source, Entry entry, RowScope scope)
        where TRecord : notnull =>
        from held in scope.Read(source)
        from previewed in cross(held, entry.Read())
            .Match(
                Succ: record => edit.Preview(record, Some(Accessors.TextChangingEventsDelay)),
                Fail: error => error.IsExpected ? edit.Preview(error, Some(Accessors.TextChangingEventsDelay)) : IO.fail<Unit>(error))
            .Flatten()
        select previewed;

    private static IO<Entry> Line(Action<IDisposable> retain) =>
        from box in DisposalOps.Acquire(IO.lift(static () => new Eto.Forms.TextBox()), retain)
        select new Entry(box, box, Eto.Forms.Keys.Enter, () => box.Text, text => box.Text = text,
            Some<Action<bool>>(varied => box.PlaceholderText = varied ? RowEdit.Varies : ""), Some(""));

    private static IO<Entry> Area(TextLines visible, Action<IDisposable> retain) =>
        from area in DisposalOps.Acquire(IO.lift(static () => new Eto.Forms.TextArea()), retain)
        from height in Height(visible, area.Font.LineHeight)
        from fitted in IO.lift(() => {
            (area.Wrap, area.AcceptsReturn, area.AcceptsTab, area.SpellCheck, area.Height) = (true, true, false, false, height);
        })
        select new Entry(area, area, Eto.Forms.Application.Instance.CommonModifier | Eto.Forms.Keys.Enter, () => area.Text, text => area.Text = text, None, Some(""));

    private static IO<Entry> Rich(TextLines visible, Action<IDisposable> retain) =>
        from rich in DisposalOps.Acquire(IO.lift(static () => new RichTextAreaWithAlternateText()), retain)
        from height in Height(visible, rich.RichTextArea.Font.LineHeight)
        from fitted in IO.lift(() => {
            (rich.HideAlternateTextWhenValueChanges, rich.SelectAllOnGotFocus, rich.Height) = (true, false, height);
        })
        select new Entry(rich, rich.RichTextArea, Eto.Forms.Application.Instance.CommonModifier | Eto.Forms.Keys.Enter, () => rich.RichTextArea.Rtf, rtf => rich.RichTextArea.Rtf = rtf,
            Some<Action<bool>>(varied => (rich.AlternateText, rich.ShowAlternateText) = (RowEdit.Varies, varied)), None);

    private static IO<int> Height(TextLines visible, float lineHeight) =>
        IO.lift(() => EDecimal.FromSingle(lineHeight).Multiply((int)visible).RoundToExponent(0, ERounding.Ceiling).ToInt32Checked());

    private static IO<Entry> Secret(Action<IDisposable> retain) =>
        from box in DisposalOps.Acquire(IO.lift(static () => new Eto.Forms.PasswordBox()), retain)
        select new Entry(box, box, Eto.Forms.Keys.Enter, () => box.Text, text => box.Text = text, None, Some(""));

    private static IO<Unit> Written(Func<string> read, Action<string> write, string shown) =>
        IO.lift(read).Bind(current => when(!string.Equals(current, shown, StringComparison.Ordinal), IO.lift(() => write(shown))).As());

    private static IO<Unit> Indicated(Eto.Forms.ProgressBar bar, WorkState state) =>
        from shown in IO.pure(state.Switch(
            inactive: static _ => (Visible: false, Indeterminate: false, Maximum: 1, Value: 0),
            indeterminate: static _ => (Visible: true, Indeterminate: true, Maximum: 1, Value: 0),
            counted: static counted => counted.Meter switch {
                var meter => (Visible: true, Indeterminate: false, Maximum: meter.Total, Value: meter.Done),
            }))
        from written in IO.lift(() => { (bar.Visible, bar.Indeterminate, bar.MaxValue, bar.Value) = shown; })
        select written;

    private static IO<Unit> Pressed(Eto.Forms.KeyEventArgs args, HashMap<Eto.Forms.Keys, IO<Unit>> answers) =>
        answers.Find(args.KeyData)
            .Traverse(answer => IO.lift(() => args.Handled = true).Bind(_ => answer)).As()
            .Map(unit);

    private static IO<Eto.Forms.Button> Browsed(
        LocalizeStringPair title, (FileAction Action, Seq<FileTypeRow> Types) browse, Eto.Forms.TextControl box, Func<string, IO<Unit>> commit, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from command in IO.lift(static () => new Eto.Forms.Command())
        from faced in IO.lift(() => command.ToolTip = RowText.Localize("Browse").Local)
        from picked in ButtonRows.Pick(command, (GlyphRole.Load, IconSlot.PickButton.Master), scope.Sink, owned)
        from executed in Subscriptions.Attach(h => command.Executed += h, h => command.Executed -= h,
            Callbacks.Handler<EventArgs>(_ =>
                from current in IO.lift(() => box.Text)
                from path in Asked(browse, title, current, scope)
                from committed in path.Match(Some: commit, None: static () => IO.pure(unit))
                select committed, new CallbackSite(scope.Sink, typeof(Eto.Forms.Command), nameof(Eto.Forms.Command.Executed))), owned)
        from dropped in Transfer.Drops(box, browse.Types, commit, scope.Sink, owned)
        select picked, retain);

    private static IO<Option<string>> Asked((FileAction Action, Seq<FileTypeRow> Types) browse, LocalizeStringPair title, string current, RowScope scope) =>
        HostDialogs.Filter(browse.Types.Map(static row => (row.Description, row.Extensions))) switch {
            var filter => (browse.Action switch {
                FileAction.OpenFile => HostDialogs.ShowOpenDialog(new OpenFileDialog { Title = title.Local, FileName = current, Filter = filter })
                    .Map(static picked => picked.Head),
                FileAction.SaveFile => HostDialogs.ShowSaveDialog(new SaveFileDialog {
                    Title = title.Local, FileName = current, Filter = filter,
                    DefaultExt = Conversions.Unset(browse.Types.Head.Bind(static row => row.Extensions.Head)),
                })
                    .Map(static picked => Some(picked)),
                FileAction.SelectFolder =>
                    from document in scope.DocumentIO
                    from picked in HostDialogs.ShowSelectFolderDialog(document, title, Conversions.Present(current))
                    select Some(picked),
                _ => throw new UnreachableException(nameof(FileAction)),
            }).Catch(static error => error.Count is 1 && error.Is(Errors.Cancelled), static _ => IO.pure(Option<string>.None)),
        };

    private static IO<(IO<Option<TState>> Read, TState Value)> Watched<TState>(ReadoutSource<TState> source, RowScope scope, Action<IDisposable> retain)
        where TState : notnull =>
        source.Switch((Scope: scope, Retain: retain),
            model: static (held, model) =>
                from document in held.Scope.DocumentIO
                from watched in model.Read.Watch(document, held.Retain)
                select (IO.lift(() => Some(watched.Value)), watched.Value),
            cell: static (_, cell) => IO.lift(() => (IO.lift(() => Some(cell.Held.Value)), cell.Held.Value)),
            scoped: static (held, reading) => reading.Read(held.Scope).Map(value => (reading.Read(held.Scope).Map(static next => Some(next)), value)));

    private sealed record Entry(Eto.Forms.Control Shown, Eto.Forms.TextControl Field, Eto.Forms.Keys Commit, Func<string> Read, Action<string> Write, Option<Action<bool>> Varied, Option<string> Blank);
}
