using System.Collections.Generic;
using Eto.Forms;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Telemetry;
using Rhino.UI;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------

[Union]
public abstract partial record ReadoutSource<TValue> where TValue : notnull {
    public sealed record Model(ReadModel<TValue> Read) : ReadoutSource<TValue>;

    public sealed record Cell(Atom<TValue> Held) : ReadoutSource<TValue>;

    public sealed record Scoped(Func<RowScope, IO<TValue>> Read) : ReadoutSource<TValue>;

    internal IO<(TValue Value, HostEvent<TValue> Changes)> Watched(RowScope scope) =>
        Switch(scope,
            model: static (held, model) =>
                from document in held.DocumentIO
                from watched in model.Read.Watch(document)
                select (watched.Value, watched.Changed.Choose(static change => Some(change.After))),
            cell: static (_, cell) => IO.lift(() => (cell.Held.Value, Subscriptions.Changed(cell.Held))),
            scoped: static (held, reading) => reading.Read(held).Map(value => (value, held.Listened(reading.Read))));

    internal IO<TControl> Live<TControl>(Func<TValue, IO<TControl>> build, Func<TControl, TValue, IO<Unit>> show, RowScope scope) where TControl : Control =>
        from first in Watched(scope).Map(static watched => watched.Value).Bracket()
        from control in build(first)
        from shown in show(control, first)
        from followed in ViewOps.Loaded(control,
            from watched in Watched(scope)
            from post in Mailboxes.Idle<Unit, TValue>(changed => changed.Find(unit).Traverse(state => show(control, state)).As().Map(static _ => unit))
            from current in post(unit, watched.Value)
            from attached in watched.Changes.Attach(state => post(unit, state))
            select unit)
        select control;
}

public sealed record ReadoutRow<TValue>(ReadoutSource<TValue> Source, Func<TValue, string> Format) : IRow<ReadoutRow<TValue>> where TValue : notnull {
    public RowShape Shape { get; init; } = RowShape.Inline;

    public static IO<RowCells> Realize(IO<ReadoutRow<TValue>> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from live in first.Source.Live(
            state =>
                from made in use(() => new Label { Text = first.Format(state), TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Wrap = WrapMode.None })
                from digits in HostTheme.Applied(made, IHostPlatform.Current.Digits(made))
                select made,
            (made, state) => latest.Bind(current => IO.lift(() => { made.Text = current.Format(state); })), scope)
        select new RowCells(live, IO.pure(unit));
}

public sealed record ProgressRow<TValue>(ReadoutSource<Loading<TValue>> Source) : IRow<ProgressRow<TValue>> where TValue : notnull {
    public RowShape Shape { get; init; } = RowShape.Inline;

    public static IO<RowCells> Realize(IO<ProgressRow<TValue>> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from live in first.Source.Live(static _ => use(static () => new ProgressBar { MinValue = 0 }), Indicated, scope)
        select new RowCells(live, IO.pure(unit));

    private static IO<Unit> Indicated(ProgressBar bar, Loading<TValue> work) =>
        IO.lift(() => {
            (bar.Visible, bar.Indeterminate, bar.MaxValue, bar.Value) = work.Switch(
                pending: static pending => pending.Work.Match(
                    Some: static progress => (true, false, progress.Positions.MaxValue, progress.Positions.Value),
                    None: static () => (true, true, 1, 0)),
                ready: static _ => (false, false, 1, 0),
                failed: static _ => (false, false, 1, 0));
        });
}

public sealed record ComputedRow<TRequest, TValue>(TRequest Request, Func<TRequest, IO<TValue>> Compute, Func<TValue, IO<Unit>> Release, Func<TValue, IO<Control>> Shown)
    : IRow<ComputedRow<TRequest, TValue>>
    where TRequest : notnull
    where TValue : notnull {
    public RowShape Shape { get; init; } = RowShape.Block;

    public static IO<RowCells> Realize(IO<ComputedRow<TRequest, TValue>> latest, LocalizeStringPair label, RowScope scope) =>
        Products.Serialized(run =>
        from owned in IO.env
        from held in IO.lift(static () => Ref(Option<(TRequest Request, Guid Run, Loading<TValue> Content)>.None))
        from body in IO.lift(static () => Ref(Option<Owned<Control>>.None))
        from working in IO.lift(static () => Ref(Option<Owned<ForkIO<Unit>>>.None))
        from frame in Containers.Frame(static () => new Panel())
        let shown =
            from row in latest
            from current in atomicIO(() => held.Value)
            from control in Products.Replace(body, Placed(current.Map(static state => state.Content).IfNone(new Loading<TValue>.Pending(None)), row.Shown)).Scoped(owned)
            from placed in IO.lift(() => { frame.Content = control; })
            select unit
        from received in Mailboxes.Idle<Guid, Loading<TValue>>(arrived => run(
            atomicIO(() => held.Swap(prior => prior.Map(state => arrived.Find(state.Run).Map(content => state with { Content = content }).IfNone(state))))
                .Bind(_ => shown)))
        select (new RowCells(frame, run(
            from row in latest
            from fresh in atomicIO(() => !held.Value.Exists(current => EqualityComparer<TRequest>.Default.Equals(current.Request, row.Request)))
            let forking = when(fresh,
                from id in IO.lift(Guid.NewGuid)
                from pending in atomicIO(() => held.Value = Some((row.Request, id, (Loading<TValue>)new Loading<TValue>.Pending(None))))
                from stopped in (IO.lift(() => { frame.Content = null; }) >> Products.Clear(body).Scoped(owned))
                    .Finally(Products.Clear(working).Scoped(owned))
                let worker = Products.Forked(
                    (Products.Acquired(row.Compute(row.Request), row.Release).Map(static value => (Loading<TValue>)new Loading<TValue>.Ready(value))
                        | @catch(static error => !error.IsCancellation, static error => IO.lift(() => Session.Failed(error, nameof(ComputedRow<,>))).Map(_ => (Loading<TValue>)new Loading<TValue>.Failed(error))))
                    .Bind(content => received(id, content)))
                from started in Products.Replace(working, worker).Scoped(owned)
                select unit).As()
            from forked in forking
            from redrawn in shown
            select unit)),
            (atomicIO(() => { held.Value = None; }) >> IO.lift(() => { frame.Content = null; }) >> Products.Clear(body))
                .Finally(Products.Clear(working))));

    private static IO<Control> Placed(Loading<TValue> content, Func<TValue, IO<Control>> shown) =>
        content.Switch(
            shown,
            pending: static (_, pending) =>
                from bar in use(() => new ProgressBar { Indeterminate = pending.Work.IsNone })
                from ranged in IO.lift(() => pending.Work.Iter(progress => (bar.MaxValue, bar.Value) = progress.Positions))
                select (Control)bar,
            ready: static (held, ready) => held(ready.Value),
            failed: static (_, _) => use(static () => new Panel()).Map(static panel => (Control)panel));

}
