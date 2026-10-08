using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Components;
using Rhino;
using Rhino.UI;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OpenEdit<TRecord>(Option<TRecord> Start, TRecord Latest, bool Stepping, Option<ValueSet> Begun) where TRecord : notnull;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class RowEdit {
    // --- [MARKS]
    public static string Varies => Localization.LocalizeString("(varies)", typeof(ObjectPropertiesPage).Assembly, 2118);

    public abstract IO<bool> Modified { get; }

    public abstract IO<Unit> Reset { get; }

    // --- [BINDING]
    private protected RowEdit() { }

    public static IO<(RowEdit<TRecord> Edit, IDisposable Release)> Bind<TRecord, THandler>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields, Action<THandler> add, Action<THandler> remove,
        Func<RowEdit<TRecord>, THandler> handler, Func<Option<TRecord>, IO<Unit>> show, RowScope scope, CallbackSite site)
        where TRecord : notnull
        where THandler : Delegate =>
        from signals in source.Signals(scope)
        from state in IO.lift(static () => Atom(Option<OpenEdit<TRecord>>.None))
        from timer in IO.lift(static () => new UITimer())
        let edit = new RowEdit<TRecord>(source, fields, scope, state, timer, made => handler(made) switch {
            var attached => (Subscriptions.Attach(add, remove, attached), value => Subscriptions.Detached(add, remove, attached, show(value))),
        })
        from held in DisposalOps.AcquireAll(
            Seq(IO.pure<IDisposable>(timer),
                edit.Attached,
                Subscriptions.Attach<EventHandler>(
                    static h => RhinoApp.Idle += h, static h => RhinoApp.Idle -= h,
                    new EventHandler(Callbacks.Handler<EventArgs>(_ => edit.Flush, site).Invoke)),
                Subscriptions.Attach<EventHandler<EventArgs>>(
                    h => timer.Elapsed += h, h => timer.Elapsed -= h,
                    Callbacks.Handler<EventArgs>(_ => edit.Elapsed, site)))
            + signals.Map(signal => signal.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ => edit.Heard), site.Sink))
            + Seq(IO.lift<IDisposable>(() => new Disposal<RowEdit<TRecord>>(edit, closing => _ = Callbacks.Answer(closing.Released, static () => unit, site)))),
            DisposalOps.Release)
        select (edit, DisposalOps.Composite(held, site));
}

public sealed class RowEdit<TRecord> : RowEdit where TRecord : notnull {
    // --- [STATE]
    private readonly RowSource<TRecord> source;
    private readonly Seq<RowField<TRecord>> fields;
    private readonly RowScope scope;
    private readonly Atom<Option<OpenEdit<TRecord>>> state;
    private readonly UITimer timer;
    private readonly Func<Option<TRecord>, IO<Unit>> show;

    internal RowEdit(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields, RowScope scope, Atom<Option<OpenEdit<TRecord>>> state, UITimer timer,
        Func<RowEdit<TRecord>, (IO<IDisposable> Attach, Func<Option<TRecord>, IO<Unit>> Show)> controls) {
        (this.source, this.fields, this.scope, this.state, this.timer) = (source, toSeq(fields), scope, state, timer);
        (Attached, show) = controls(this);
    }

    public Seq<EntryKey> Keys => fields.Bind(field => source.Keys(field.Parameter));

    // --- [BRACKET]
    public IO<Unit> Take<TValue>(Edit<TValue> edit, Func<TValue, TRecord, Fin<TRecord>> into) where TValue : notnull =>
        Latest.Bind(latest => edit.Switch(
            (Edit: this, Latest: latest, Into: into),
            preview: static (taken, preview) => taken.Edit.Preview(taken.Into(preview.Value, taken.Latest), None),
            step: static (taken, step) => taken.Edit.Preview(taken.Into(step.Value, taken.Latest), Some(Accessors.StepEventsDelay)),
            commit: static (taken, commit) => taken.Edit.Commit(taken.Into(commit.Value, taken.Latest)),
            cancel: static (taken, _) => taken.Edit.Cancel));

    public IO<Unit> Preview(Fin<TRecord> value, Option<IO<double>> delay) =>
        value.Match(
            Succ: latest =>
                from opened in Opened
                from seconds in delay.Traverse(static read => read).As()
                from moved in state.SwapIO(held => held.Map(open => open with { Latest = latest, Stepping = seconds.IsSome }))
                from timed in IO.lift(() => seconds.Match(
                    Some: held => {
                        timer.Interval = held;
                        timer.Start();
                    },
                    None: timer.Stop))
                select unit,
            Fail: static _ => IO.pure(unit));

    public IO<Unit> Commit(Fin<TRecord> value) =>
        Opened.Bind(_ => Close(open => Committed(value.IfFail(_ => open.Latest), open))).Bind(_ => Heard);

    public IO<Unit> Cancel =>
        Close(Restored).Bind(_ => Heard);

    public override IO<Unit> Reset =>
        Opened.Bind(_ => Close(open => Committed(source.Default, open))).Bind(_ => Heard);

    // --- [MARKS]
    public IO<Unit> Shown =>
        state.ValueIO.Bind(held => held.Match(Some: static _ => IO.pure(unit), None: () => Agreed.Bind(show)));

    public override IO<bool> Modified =>
        scope.Edited(source)
            .Bind(stores => stores.TraverseM(store => store.Read.Map(held => held.IfNone(source.Default)).Map(current => !store.Same(Taken(source.Default, current), current))).As())
            .Map(static marks => marks.Exists(static mark => mark));

    private IO<Option<TRecord>> Agreed =>
        from stores in scope.Edited(source)
        from held in stores.TraverseM(store => store.Read.Map(value => (store.Same, Value: value.IfNone(source.Default)))).As()
        select held.Head.Match(
            Some: first => held.Tail.ForAll(next => first.Same(Taken(next.Value, first.Value), first.Value)) ? Some(first.Value) : None,
            None: () => Some(source.Default));

    // --- [PACING]
    internal IO<Unit> Flush =>
        from held in state.ValueIO
        from paced in scope.Paced
        from written in held.Filter(_ => paced).Traverse(open => Paced(open.Latest)).As()
        select unit;

    internal IO<Unit> Elapsed =>
        Close(open => Committed(open.Latest, open)).Bind(_ => Heard);

    internal IO<Unit> Heard => scope.Changed(Keys);

    internal IO<IDisposable> Attached { get; }

    internal IO<Unit> Released =>
        state.ValueIO.Bind(held => held.Match(
            Some: open => open.Stepping ? Close(stepped => Committed(stepped.Latest, stepped)) : Close(Restored),
            None: static () => IO.pure(unit)));

    private IO<TRecord> Latest =>
        state.ValueIO.Bind(held => held.Match(Some: static open => IO.pure(open.Latest), None: () => scope.Read(source)));

    private IO<Unit> Opened =>
        state.ValueIO.Bind(held => held.Match(Some: static _ => IO.pure(unit), None: () => Open));

    private IO<Unit> Open =>
        from stores in scope.Edited(source)
        from start in stores.Head.Match(Some: static store => store.Read, None: static () => IO.pure(Option<TRecord>.None))
        from records in scope.Records
        from begun in (from keys in Keys.AsIterableNE()
                       from history in scope.History.Filter(_ => records)
                       select history.Group.Capture.Map(set => set.Scoped(Some(keys)))).Traverse(static capture => capture).As()
        from opened in state.SwapIO(_ => Some(new OpenEdit<TRecord>(start, start.IfNone(source.Default), Stepping: false, begun)))
        select unit;

    private IO<Unit> Close(Func<OpenEdit<TRecord>, IO<Unit>> closing) =>
        state.ValueIO.Bind(held => held.Match(
            Some: open =>
                from stopped in IO.lift(timer.Stop)
                from closed in state.SwapIO(static _ => Option<OpenEdit<TRecord>>.None)
                from written in closing(open)
                select unit,
            None: static () => IO.pure(unit)));

    private IO<Unit> Committed(TRecord value, OpenEdit<TRecord> open) =>
        scope.Within(Written(value)).Bind(_ => Recorded(open));

    private IO<Unit> Restored(OpenEdit<TRecord> open) =>
        scope.Paced.Bind(paced => when(paced, Written(open.Start.IfNone(source.Default))).As());

    private IO<Unit> Recorded(OpenEdit<TRecord> open) =>
        (from begun in open.Begun
         from keys in Keys.AsIterableNE()
         from history in scope.History
         select history.Group.Capture.Bind(after => history.Commit(after.Scoped(Some(keys)), begun)))
        .Match(Some: static record => record.Map(static _ => unit), None: static () => IO.pure(unit));

    private IO<Unit> Written(TRecord value) =>
        scope.Edited(source).Bind(stores => ValueStore.Commit(stores.Map(store => Edit(store, value)).Add(IO.pure(stores.Choose(static store => store.Applied.Drain).Head))));

    private IO<Unit> Paced(TRecord value) =>
        scope.Edited(source).Bind(stores => stores.TraverseM(store => Edit(store, value)).As()).Map(static _ => unit);

    private IO<Option<IO<Unit>>> Edit(ValueStore<TRecord> store, TRecord value) =>
        store.Read.Bind(held => Taken(value, held.IfNone(source.Default)) switch {
            var written => store.Edit(Some(written).Filter(record => !store.Same(record, source.Default))),
        });

    private TRecord Taken(TRecord from, TRecord into) => fields.Fold(into, (held, field) => field.Taken(from, held));
}
