using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;

namespace Rasm.Rhino.UI.Rows;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentRows {
    // --- [COMPUTED]
    public static ControlRow Computed<TRequest, TValue>(string label, TRequest request, Func<TRequest, IO<TValue>> compute, Func<TValue, IO<Unit>> release, Func<TValue, IO<Control>> shown)
        where TRequest : notnull
        where TValue : notnull =>
        new ControlRow.Bound<(TRequest Request, Func<TRequest, IO<TValue>> Compute, Func<TValue, IO<Unit>> Release, Func<TValue, IO<Control>> Shown)>(label, RowShape.Block, (request, compute, release, shown),
            static (latest, scope) =>
                from held in atomIO(Option<(TRequest Request, Loading<TValue> Content)>.None)
                from frame in Containers.Frame(static () => new Panel())
                from owned in use(static () => EnvIO.New(token: CancellationToken.None))
                from kept in IO.lift(static () => Ref(Option<Lease<TValue>>.None))
                from body in IO.lift(static () => Ref(Option<Lease<Control>>.None))
                from working in IO.lift(static () => Ref(Option<Lease<ForkIO<Unit>>>.None))
                let shown =
                    from row in latest
                    from current in held.ValueIO
                    from control in Products.Swapped(body, Placed(current.Map(static state => state.Content).IfNone(new Loading<TValue>.Pending(None)), row.Row.Shown,
                        RowText.Localize("{0} unavailable", scope.Owner, arguments: [RowText.Localize(row.Label, scope.Owner).Local]).Local)).Under(owned)
                    from seated in IO.lift(() => { frame.Content = control; })
                    select unit
                from landed in Mailboxes.Idle<TRequest, Loading<TValue>>(arrived => Landed(held, kept, owned, latest, arrived).Bind(_ => shown))(scope.Owner)
                select new RowCells(frame,
                    from row in latest
                    from prior in held.ValueIO
                    from changed in when(!prior.Exists(current => EqualityComparer<TRequest>.Default.Equals(current.Request, row.Row.Request)),
                        from pending in held.SwapIO(current => Some((row.Row.Request, current.Map(static state => state.Content.Requested(None)).IfNone(new Loading<TValue>.Pending(None)))))
                        from forked in Products.Cleared(working).Bind(_ => Products.Swapped(working, use(
                            row.Row.Compute(row.Row.Request).Map(static value => (Loading<TValue>)new Loading<TValue>.Ready(value))
                                .Catch(static error => !error.Is(Errors.Cancelled), static error => IO.pure<Loading<TValue>>(new Loading<TValue>.Failed(error)))
                                .Bind(content => landed(row.Row.Request, content))
                                .Fork(),
                            static fork => fork.Cancel))).Under(owned)
                        select unit).As()
                    from redrawn in shown
                    select unit));

    private static IO<Control> Placed<TValue>(Loading<TValue> content, Func<TValue, IO<Control>> shown, string unavailable) where TValue : notnull =>
        content.Switch(
            (Shown: shown, Unavailable: unavailable),
            pending: static (_, pending) =>
                from bar in use(() => new ProgressBar { Indeterminate = pending.Work.IsNone })
                from ranged in IO.lift(() => pending.Work.Iter(progress => (bar.MaxValue, bar.Value) = progress.Positions))
                select (Control)bar,
            ready: static (at, ready) => at.Shown(ready.Value),
            failed: static (at, _) => use(() => new Label { Text = at.Unavailable, TextColor = SystemColors.DisabledText }).Map(static label => (Control)label));

    private static IO<Unit> Landed<TRequest, TValue>(
        Atom<Option<(TRequest Request, Loading<TValue> Content)>> held, Ref<Option<Lease<TValue>>> kept, EnvIO owned,
        IO<ControlRow.Bound<(TRequest Request, Func<TRequest, IO<TValue>> Compute, Func<TValue, IO<Unit>> Release, Func<TValue, IO<Control>> Shown)>> latest, HashMap<TRequest, Loading<TValue>> arrived)
        where TRequest : notnull
        where TValue : notnull =>
        toSeq(arrived.AsIterable()).TraverseM(entry =>
            from current in held.ValueIO
            from row in latest
            from placed in current.Exists(state => EqualityComparer<TRequest>.Default.Equals(state.Request, entry.Key))
                ? held.SwapIO(state => state.Map(prior => prior with { Content = entry.Value }))
                    .Bind(_ => entry.Value.Held.Traverse(value => Products.Cleared(kept).Bind(_ => Products.Swapped(kept, use(IO.pure(value), stored => latest.Bind(last => last.Row.Release(stored))))).Under(owned)).As())
                    .Map(static _ => unit)
                : entry.Value.Held.Traverse(row.Row.Release).As().Map(static _ => unit)
            select unit).As().Map(static _ => unit);
}
