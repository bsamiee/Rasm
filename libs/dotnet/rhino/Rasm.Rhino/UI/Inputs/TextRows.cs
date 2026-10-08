using System.Diagnostics;
using Eto;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Chrome;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Views;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TextLines : System.Numerics.IMinMaxValue<TextLines> {
    public static TextLines MinValue { get; } = new(1);
    public static TextLines MaxValue { get; } = new(short.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
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
    public static ControlRow Keyed<TRecord, TValue, TRaw, TError>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, EntryForm form, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Typed(source, field,
            record => StoredText.Capture<TValue, TRaw>(lens.Get(record)),
            (record, text) => StoredText.Recall<TValue, TRaw, TError>(text).Map(value => lens.Set(value, record)),
            form, None, rules);

    public static ControlRow Raw<TRecord, TRaw>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TRaw> lens, EntryForm form, RowRules rules)
        where TRecord : notnull
        where TRaw : notnull, ISpanParsable<TRaw> =>
        Typed(source, field,
            record => StoredText.Format(lens.Get(record)),
            (record, text) => StoredText.Parse<TRaw>(text).Map(raw => lens.Set(raw, record)),
            form, None, rules);

    public static ControlRow OptionalKeyed<TRecord, TValue, TRaw, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Option<TValue>> lens, EntryForm form, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        Typed(source, field,
            record => Conversions.Unset(lens.Get(record).Map(static value => StoredText.Capture<TValue, TRaw>(value))),
            (record, text) => Conversions.Present(text).Match(
                Some: present => StoredText.Recall<TValue, TRaw, TError>(present).Map(value => lens.Set(Some(value), record)),
                None: () => lens.Set(None, record)),
            form, None, rules);

    public static ControlRow OptionalRaw<TRecord, TRaw>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Option<TRaw>> lens, EntryForm form, RowRules rules)
        where TRecord : notnull
        where TRaw : notnull, ISpanParsable<TRaw> =>
        Typed(source, field,
            record => Conversions.Unset(lens.Get(record).Map(static raw => StoredText.Format(raw))),
            (record, text) => Conversions.Present(text).Match(
                Some: present => StoredText.Parse<TRaw>(present).Map(raw => lens.Set(Some(raw), record)),
                None: () => lens.Set(None, record)),
            form, None, rules);

    // --- [PATHS]
    public static ControlRow Path<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, string> lens, FileAction action, Seq<FileTypeRow> types, RowRules rules)
        where TRecord : notnull =>
        Typed(source, field, lens.Get, (record, text) => lens.Set(text, record), new EntryForm.SingleLine(), Some((action, types)), rules);

    public static ControlRow Loaded<TRecord, TValue, TKey, TRaw, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key,
        FileAction action, Seq<FileTypeRow> types, RowRules rules)
        where TRecord : notnull
        where TKey : IObjectFactory<TKey, TRaw, TError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw>
        where TError : Error, IValidationError<TError> =>
        ControlRow.Of(source, field, RowShape.Inline, scope => Entered(source, field,
            record => Conversions.Unset(lens.Get(record).Map(value => StoredText.Capture<TKey, TRaw>(key(value)))),
            None,
            (record, text) => Conversions.Present(text).Match(
                Some: present =>
                    from found in IO.lift(StoredText.Recall<TKey, TRaw, TError>(present))
                    from value in load(found)
                    select lens.Set(Some(value), record),
                None: () => IO.pure(lens.Set(None, record))),
            new EntryForm.SingleLine(), Some((action, types)), scope), rules);

    // --- [LIVE]
    public static ControlRow Readout<TState>(string caption, string help, ReadoutSource<TState> source, Func<TState, string> format, RowRules rules)
        where TState : notnull =>
        new ControlRow.Readout(RowShape.Inline, scope =>
            from live in Live(source,
                static _ =>
                    from label in IO.lift(static () => new Label { Wrap = WrapMode.None, TextAlignment = TextAlignment.Right })
                    from digits in IO.lift(() => label.Font = HostTheme.Digits(label.Font))
                    select label,
                (label, state) => Written(() => label.Text, text => label.Text = text, format(state)), scope)
            select new RowCells(live.Control, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None, live.Release),
            caption, help, rules);

    public static ControlRow Progress(string caption, string help, ReadoutSource<WorkState> source, RowRules rules) =>
        new ControlRow.Readout(RowShape.Inline, scope =>
            from live in Live(source, static _ => IO.lift(static () => new ProgressBar { MinValue = 0 }), Indicated, scope)
            select new RowCells(live.Control, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None, live.Release),
            caption, help, rules);

    internal static IO<(TControl Control, IDisposable Release)> Live<TState, TControl>(
        ReadoutSource<TState> source, Func<TState, IO<TControl>> build, Func<TControl, TState, IO<Unit>> show, RowScope scope)
        where TState : notnull
        where TControl : Control =>
        from control in Watched(source, scope).Bracket(
            Use: first =>
                from built in build(first.Value)
                from shown in show(built, first.Value)
                select built,
            Fin: static first => DisposalOps.Release(first.Release.ToSeq()))
        let site = new CallbackSite(scope.Sink, typeof(FrameClocks), nameof(FrameClocks.Sample))
        let edge = new CallbackSite(scope.Sink, control.GetType(), nameof(Control.UnLoad))
        let watch =
            from watched in Watched(source, scope)
            from sampled in DisposalOps.OnFailure(
                FrameClocks.Sample(control, watched.Read.Bind(current => current.Match(Some: state => show(control, state), None: static () => IO.pure(unit))), site),
                DisposalOps.Release(watched.Release.ToSeq()))
            select DisposalOps.Composite(watched.Release.ToSeq().Add(sampled), site)
        from held in IO.lift(static () => Atom((Held: Seq<IDisposable>(), Taken: Seq<IDisposable>())))
        let unload =
            from taken in held.SwapIO(static state => (Held: Seq<IDisposable>(), Taken: state.Held))
            from released in DisposalOps.Release(taken.Taken)
            select released
        from edges in DisposalOps.AcquireAll(
            Seq(Subscriptions.Attach(h => control.Load += h, h => control.Load -= h,
                    Callbacks.Handler<EventArgs>(_ =>
                        from opened in watch
                        from kept in held.SwapIO(state => state with { Held = state.Held.Add(opened) })
                        select unit, new CallbackSite(scope.Sink, control.GetType(), nameof(Control.Load)))),
                Subscriptions.Attach(h => control.UnLoad += h, h => control.UnLoad -= h, Callbacks.Handler<EventArgs>(_ => unload, edge))),
            DisposalOps.Release)
        select (control, DisposalOps.Composite(new Disposal<IO<Unit>>(unload, release => Callbacks.Succeeded(release, edge)).Cons(edges), edge));

    // --- [REALIZE]
    private static ControlRow Typed<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Func<TRecord, string> text, Func<TRecord, string, Fin<TRecord>> cross,
        EntryForm form, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Inline,
            scope => Entered(source, field, text, Some(cross), (record, typed) => IO.lift(cross(record, typed)), form, browse, scope), rules);

    private static IO<RowCells> Entered<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Func<TRecord, string> text, Option<Func<TRecord, string, Fin<TRecord>>> preview,
        Func<TRecord, string, IO<TRecord>> commit, EntryForm form, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse, RowScope scope)
        where TRecord : notnull =>
        from entry in IO.lift(() => form.Switch(
            singleLine: static _ => Line(),
            multiLine: static multi => Area(multi.Visible),
            rich: static rich => Rich(rich.Visible),
            secret: static _ => Secret()))
        let site = new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(TextControl.TextChanged))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            h => entry.Field.TextChanged += h, h => entry.Field.TextChanged -= h,
            edit => Callbacks.Handler<EventArgs>(_ => preview.Traverse(cross =>
                    from held in scope.Read(source)
                    from previewed in edit.Preview(cross(held, entry.Read()), Some(Accessors.TextChangingEventsDelay))
                    select previewed).As().Map(static _ => unit), site),
            value =>
                from marked in IO.lift(() => entry.Varied.Iter(mark => mark(value.IsNone)))
                from written in (value.Map(text) | entry.Blank).Traverse(shown => Written(entry.Read, entry.Write, shown)).As()
                select unit,
            scope, site)
        let committed = (Func<string, IO<Unit>>)(typed =>
            from held in scope.Read(source)
            from written in commit(held, typed)
                .Match(Succ: record => bound.Edit.Commit(record), Fail: error => error.IsExpected ? bound.Edit.Commit(error) : IO.fail<Unit>(error))
                .Flatten()
            select written)
        let answers = HashMap((entry.Commit, IO.lift(entry.Read).Bind(committed)), (Keys.Escape, bound.Edit.Cancel))
        from keys in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                Seq(Subscriptions.Attach(h => entry.Field.KeyDown += h, h => entry.Field.KeyDown -= h,
                        Callbacks.Handler<KeyEventArgs>(args => Pressed(args, answers), new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(Control.KeyDown)))),
                    Subscriptions.Attach(h => entry.Field.LostFocus += h, h => entry.Field.LostFocus -= h,
                        Callbacks.Handler<EventArgs>(_ => IO.lift(entry.Read).Bind(committed), new CallbackSite(scope.Sink, entry.Field.GetType(), nameof(Control.LostFocus))))),
                DisposalOps.Release),
            IO.lift(bound.Release.Dispose))
        from picked in DisposalOps.OnFailure(
            browse.Traverse(asked => Browsed(field.Caption, asked, entry.Field, committed, scope)).As(),
            DisposalOps.Release(bound.Release.Cons(keys)))
        select new RowCells(
            entry.Shown, None, picked.Map(static held => (Control)held.Pick), None, [], [], new RowHelp(None, Conversions.Present(text(source.Default))),
            bound.Edit.Shown, Some<RowEdit>(bound.Edit), Some(ChoiceRows.Context(entry.Shown, [], [], scope.Sink)),
            DisposalOps.Composite(bound.Release.Cons(keys) + picked.Map(static held => held.Release).ToSeq(), site));

    private static Entry Line() {
        TextBox box = new();
        return new(box, box, Keys.Enter, () => box.Text, text => box.Text = text,
            Some<Action<bool>>(varied => box.PlaceholderText = varied ? RowEdit.Varies : ""), Some(""));
    }

    private static Entry Area(TextLines visible) {
        TextArea area = new() { Wrap = true, AcceptsReturn = true, AcceptsTab = false, SpellCheck = false };
        area.Height = (int)Math.Ceiling(visible * area.Font.LineHeight);
        return new(area, area, Application.Instance.CommonModifier | Keys.Enter, () => area.Text, text => area.Text = text, None, Some(""));
    }

    private static Entry Rich(TextLines visible) {
        RichTextAreaWithAlternateText rich = new() { HideAlternateTextWhenValueChanges = true, SelectAllOnGotFocus = false };
        rich.Height = (int)Math.Ceiling(visible * rich.RichTextArea.Font.LineHeight);
        return new(rich, rich.RichTextArea, Application.Instance.CommonModifier | Keys.Enter, () => rich.RichTextArea.Rtf, rtf => rich.RichTextArea.Rtf = rtf,
            Some<Action<bool>>(varied => (rich.AlternateText, rich.ShowAlternateText) = (RowEdit.Varies, varied)), None);
    }

    private static Entry Secret() {
        PasswordBox box = new();
        return new(box, box, Keys.Enter, () => box.Text, text => box.Text = text, None, Some(""));
    }

    private static IO<Unit> Written(Func<string> read, Action<string> write, string shown) =>
        IO.lift(read).Bind(current => when(!string.Equals(current, shown, StringComparison.Ordinal), IO.lift(() => write(shown))).As());

    private static IO<Unit> Indicated(ProgressBar bar, WorkState state) =>
        from shown in IO.pure(state.Switch(
            inactive: static _ => (Visible: false, Indeterminate: false, Maximum: 1, Value: 0),
            indeterminate: static _ => (Visible: true, Indeterminate: true, Maximum: 1, Value: 0),
            counted: static counted => (Visible: true, Indeterminate: false, Maximum: counted.Work.Total, Value: counted.Work.Done)))
        from current in IO.lift(() => (bar.Visible, bar.Indeterminate, bar.MaxValue, bar.Value))
        from written in when(current != shown, IO.lift(() => { (bar.Visible, bar.Indeterminate, bar.MaxValue, bar.Value) = shown; })).As()
        select written;

    private static IO<Unit> Pressed(KeyEventArgs args, HashMap<Keys, IO<Unit>> answers) =>
        answers.Find(args.KeyData)
            .Traverse(answer => IO.lift(() => args.Handled = true).Bind(_ => answer)).As()
            .Map(static _ => unit);

    private static IO<(Button Pick, IDisposable Release)> Browsed(
        string caption, (FileAction Action, Seq<FileTypeRow> Types) browse, TextControl box, Func<string, IO<Unit>> commit, RowScope scope) =>
        from command in IO.lift(static () => new Command { ToolTip = RowText.Localize("Browse").Local })
        let site = new CallbackSite(scope.Sink, typeof(Command), nameof(Command.Executed))
        let title = RowText.Localize(caption, table: Some<object>(scope.Sink))
        from picked in ButtonRows.Pick(command, GlyphRole.Load, scope.Sink)
        from held in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                Seq(Subscriptions.Attach(h => command.Executed += h, h => command.Executed -= h,
                        Callbacks.Handler<EventArgs>(_ =>
                            from current in IO.lift(() => box.Text)
                            from path in Asked(browse, title, current, scope)
                            from committed in path.Match(Some: commit, None: static () => IO.pure(unit))
                            select committed, site)),
                    Transfer.Drops(box, browse.Types, commit, scope.Sink)),
                DisposalOps.Release),
            IO.lift(picked.Release.Dispose))
        select (picked.Control, DisposalOps.Composite(picked.Release.Cons(held), site));

    private static IO<Option<string>> Asked((FileAction Action, Seq<FileTypeRow> Types) browse, LocalizeStringPair title, string current, RowScope scope) =>
        (browse.Action switch {
            FileAction.OpenFile => HostDialogs.ShowOpenDialog(new global::Rhino.UI.OpenFileDialog { Title = title.Local, FileName = current, Filter = HostDialogs.Filter(browse.Types) })
                .Map(static picked => picked.Head),
            FileAction.SaveFile => HostDialogs.ShowSaveDialog(new global::Rhino.UI.SaveFileDialog {
                Title = title.Local, FileName = current, Filter = HostDialogs.Filter(browse.Types),
                DefaultExt = Conversions.Unset(browse.Types.Head.Bind(static row => row.Extensions.Head)),
            })
                .Map(static picked => Some(picked)),
            FileAction.SelectFolder =>
                from document in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
                from picked in HostDialogs.ShowSelectFolderDialog(document, title, Conversions.Present(current))
                select Some(picked),
            _ => throw new UnreachableException(nameof(FileAction)),
        }).Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(Option<string>.None));

    private static IO<(IO<Option<TState>> Read, TState Value, Option<IDisposable> Release)> Watched<TState>(ReadoutSource<TState> source, RowScope scope)
        where TState : notnull =>
        source.Switch(
            scope,
            model: static (held, model) =>
                from document in IO.lift(held.Document.ToFin(new Missing(nameof(RowScope.Document))))
                from watched in model.Read.Watch(document)
                from value in DisposalOps.OnFailure(
                    IO.lift(watched.Held.Map(static live => live.Value).ToFin(new Missing(nameof(LiveModel<>.Value)))),
                    IO.lift(watched.Dispose))
                select (IO.lift(() => watched.Held.Map(static live => live.Value)), value, Some<IDisposable>(watched)),
            cell: static (_, cell) =>
                IO.lift(() => (IO.lift(() => Some(cell.Held.Value)), cell.Held.Value, Option<IDisposable>.None)),
            scoped: static (held, reading) =>
                reading.Read(held).Map(value => (reading.Read(held).Map(static next => Some(next)), value, Option<IDisposable>.None)));

    private sealed record Entry(Control Shown, TextControl Field, Keys Commit, Func<string> Read, Action<string> Write, Option<Action<bool>> Varied, Option<string> Blank);
}
