using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Components;
using Rhino;
using Rhino.UI;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OpenEdit<TRecord>(Seq<(ValueStore<TRecord> Store, Option<TRecord> Start)> Targets, TRecord Latest, bool Stepping, Option<ValueDiff> Diff, Option<IO<Unit>> Owed) where TRecord : notnull;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class RowEdit {
    // --- [MARKS]
    public static string Varies => Localization.LocalizeString("(varies)", typeof(ObjectPropertiesPage).Assembly, 2118);

    public abstract IO<bool> Modified { get; }

    public abstract IO<bool> Editing { get; }

    public abstract IO<Unit> Reset { get; }

    // --- [BINDING]
    private protected RowEdit() { }

    public static IO<RowEdit<TRecord>> Bind<TRecord, THandler>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields, Action<THandler> add, Action<THandler> remove,
        Func<RowEdit<TRecord>, THandler> handler, Func<Option<TRecord>, IO<Unit>> show, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TRecord : notnull
        where THandler : Delegate =>
        from signals in source.Signals(scope)
        from state in IO.lift(static () => Ref(Option<OpenEdit<TRecord>>.None))
        from timer in DisposalOps.Acquire(IO.lift(static () => new UITimer()), static timer => IO.lift(timer.Dispose), retain)
        let edit = new RowEdit<TRecord>(source, fields, scope, state, timer, made => handler(made) switch {
            var attached => (keep => Subscriptions.Attach(add, remove, attached, keep), value => Subscriptions.Detached(add, remove, attached, show(value))),
        })
        from attached in edit.Attached(retain)
        from idle in Subscriptions.Attach(
            static h => RhinoApp.Idle += h, static h => RhinoApp.Idle -= h,
            new EventHandler(Callbacks.Handler<EventArgs>(_ => edit.Flush, site).Invoke), retain)
        from elapsed in Subscriptions.Attach(
            h => timer.Elapsed += h, h => timer.Elapsed -= h,
            Callbacks.Handler<EventArgs>(_ => edit.Elapsed, site), retain)
        from heard in Callbacks.Each(signals.Map(signal => signal.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ => edit.Heard), site.Sink, retain)))
        from closing in DisposalOps.Acquire(IO.lift(() => new Disposal<RowEdit<TRecord>>(edit, static closed => closed.Released.Run())), static lease => IO.lift(lease.Dispose), retain)
        select edit;
}

public sealed class RowEdit<TRecord> : RowEdit where TRecord : notnull {
    // --- [STATE]
    private readonly RowSource<TRecord> source;
    private readonly Seq<RowField<TRecord>> fields;
    private readonly RowScope scope;
    private readonly Ref<Option<OpenEdit<TRecord>>> state;
    private readonly UITimer timer;
    private readonly Func<Option<TRecord>, IO<Unit>> show;

    internal RowEdit(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields, RowScope scope, Ref<Option<OpenEdit<TRecord>>> state, UITimer timer,
        Func<RowEdit<TRecord>, (Func<Action<IDisposable>, IO<Unit>> Attach, Func<Option<TRecord>, IO<Unit>> Show)> controls) {
        (this.source, this.fields, this.scope, this.state, this.timer) = (source, toSeq(fields), scope, state, timer);
        (Attached, show) = controls(this);
    }

    public Seq<EntryKey> Keys => fields.Bind(row => source.Keys(row.Parameter));

    // --- [BRACKET]
    public IO<Seq<TRecord>> Take<TValue>(Edit<TValue> edit, Func<TValue, TRecord, Fin<TRecord>> into) where TValue : notnull =>
        edit.Switch(
            (Edit: this, Into: into),
            preview: static (taken, preview) => taken.Edit.Opened.Bind(open => taken.Edit.Preview(taken.Into(preview.Value, open.Latest), None)).Map(static _ => Seq<TRecord>()),
            step: static (taken, step) => taken.Edit.Opened.Bind(open => taken.Edit.Preview(taken.Into(step.Value, open.Latest), Some(Accessors.StepEventsDelay))).Map(static _ => Seq<TRecord>()),
            commit: static (taken, commit) => taken.Edit.Opened.Bind(open => taken.Edit.Commit(taken.Into(commit.Value, open.Latest))),
            cancel: static (taken, _) => taken.Edit.Cancel.Map(static _ => Seq<TRecord>()));

    public IO<Unit> Preview(Fin<TRecord> value, Option<IO<double>> delay) =>
        value.Match(
            Succ: latest =>
                from opened in Opened
                from seconds in delay.Traverse(static read => read).As()
                from moved in atomicIO(() => state.Value = state.Value.Map(open => open with { Latest = latest, Stepping = seconds.IsSome }))
                from timed in IO.lift(() => seconds.Match(
                    Some: held => {
                        timer.Interval = held;
                        timer.Start();
                    },
                    None: timer.Stop))
                select unit,
            Fail: static _ => IO.pure(unit));

    public IO<Seq<TRecord>> Commit(Fin<TRecord> value) =>
        from latest in IO.lift(value)
        from opened in Opened
        from applied in Close(open => Committed(latest, open))
        from heard in Heard
        select applied;

    public IO<Unit> Cancel =>
        Close(Restored).Bind(_ => Heard);

    public override IO<Unit> Reset =>
        Opened.Bind(_ => Close(open => Committed(source.Default, open))).Bind(_ => Heard);

    // --- [MARKS]
    public override IO<bool> Editing => state.ValueIO.Map(static held => held.IsSome);

    public IO<Unit> Shown =>
        state.ValueIO.Bind(held => held.Match(Some: static _ => IO.pure(unit), None: () => Agreed.Bind(show)));

    public override IO<bool> Modified =>
        scope.Edited(source)
            .Bind(stores => Callbacks.Each(stores.Map(store => store.Read.Map(held => held.IfNone(source.Default)).Map(current => !store.Same(Taken(source.Default, current), current)))))
            .Map(static marks => marks.Exists(static mark => mark));

    private IO<Option<TRecord>> Agreed =>
        from stores in scope.Edited(source)
        from held in Callbacks.Each(stores.Map(store => store.Read.Map(value => (store.Same, Value: value.IfNone(source.Default)))))
        select held.Head.Match(
            Some: first => held.Tail.ForAll(next => first.Same(Taken(next.Value, first.Value), first.Value)) ? Some(first.Value) : None,
            None: () => Some(source.Default));

    // --- [PACING]
    internal IO<Unit> Flush =>
        from held in state.ValueIO
        from paced in scope.Paced
        from written in held.Filter(_ => paced).Traverse(Paced).As()
        select unit;

    internal IO<Unit> Elapsed =>
        Close(open => Committed(open.Latest, open)).Bind(_ => Heard);

    internal IO<Unit> Heard => scope.Changed(Keys);

    internal Func<Action<IDisposable>, IO<Unit>> Attached { get; }

    internal IO<Unit> Released =>
        state.ValueIO.Bind(held => held.Match(
            Some: open => (open.Stepping ? Close(stepped => Committed(stepped.Latest, stepped)) : Close(Restored)).Map(static _ => unit),
            None: static () => IO.pure(unit)));

    public IO<OpenEdit<TRecord>> Opened =>
        state.ValueIO.Bind(held => held.Match(Some: static open => IO.pure(open), None: () => Open));

    private IO<OpenEdit<TRecord>> Open =>
        from stores in scope.Edited(source)
        from targets in Callbacks.Each(stores.Map(static store => store.Read.Map(start => (Store: store, Start: start))))
        from records in scope.Records
        let diff = scope.History.Filter(_ => records).Map(static _ => ValueDiff.Empty)
        let open = new OpenEdit<TRecord>(targets, targets.Head.Bind(static target => target.Start).IfNone(source.Default), Stepping: false, diff, None)
        from opened in atomicIO(() => state.Value = Some(open))
        select open;

    private IO<Seq<TRecord>> Close(Func<OpenEdit<TRecord>, IO<Seq<TRecord>>> closing) =>
        envIO.Bind(environment =>
            state.ValueIO.Bind(held => held.Match(
                Some: open => Closed(open, closing, environment),
                None: static () => IO.pure(Seq<TRecord>()))).Uninterruptible());

    private IO<Seq<TRecord>> Closed(OpenEdit<TRecord> open, Func<OpenEdit<TRecord>, IO<Seq<TRecord>>> closing, EnvIO environment) =>
        from stopped in IO.lift(timer.Stop)
        from written in IO.liftVAsync(() => closing(open).RunAsync(environment))
        from closed in atomicIO(() => state.Value = None)
        select written;

    private IO<Seq<TRecord>> Committed(TRecord value, OpenEdit<TRecord> open) =>
        from applied in scope.Within(ValueStore.Commit(open.Targets.Map(target => Edit(target.Store, value)), open.Owed))
        from held in state.ValueIO
        from recorded in Recorded(held.Bind(static current => current.Diff))
        select applied;

    private IO<Seq<TRecord>> Restored(OpenEdit<TRecord> open) =>
        scope.Paced.Bind(paced => paced
            ? ValueStore.Commit(open.Targets.Map(target => Edit(target.Store, target.Start.IfNone(source.Default))), open.Owed)
            : IO.pure(Seq<TRecord>()));

    private IO<Unit> Recorded(Option<ValueDiff> applied) =>
        (from diff in applied
         from history in scope.History
         select history.Commit(diff))
        .Match(Some: static record => record.Map(static _ => unit), None: static () => IO.pure(unit));

    private IO<Unit> Paced(OpenEdit<TRecord> open) =>
        Callbacks.Each(open.Targets.Map(target => Edit(target.Store, open.Latest))).Map(static _ => unit);

    private IO<(TRecord Value, Option<IO<Unit>> Owed)> Edit(ValueStore<TRecord> store, TRecord value) =>
        envIO.Bind(environment =>
            (from accepted in IO.liftVAsync(() => store.Read.Bind(held => Taken(value, held.IfNone(source.Default)) switch {
                var written => store.Apply(Some(written).Filter(record => !store.Same(record, source.Default)))
                    .Map(applied => (Applied: applied, Value: (applied.Value.HasChanged ? applied.Value.ToOption() : held).IfNone(source.Default))),
            }).RunAsync(environment))
             let diff = ValueDiff.Of(accepted.Applied.Value, source.Default, source.Capture)
             from accumulated in atomicIO(() => state.Value = state.Value.Map(open => open with {
                 Diff = open.Diff.Map(earlier => earlier.Then(diff)),
                 Owed = open.Owed | accepted.Applied.Owed,
             }))
             select (accepted.Value, accepted.Applied.Owed)).Uninterruptible());

    private TRecord Taken(TRecord from, TRecord into) => fields.Fold(into, (held, field) => field.Taken(from, held));
}
