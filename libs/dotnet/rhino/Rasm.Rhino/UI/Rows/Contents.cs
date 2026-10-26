using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;

namespace Rasm.Rhino.UI.Rows;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentRows {
    // --- [COMPUTED]
    public static ControlRow Computed<TRequest, TValue>(string label, TRequest request, Func<TRequest, IO<TValue>> compute, Func<TValue, IO<Unit>> release, Func<TValue, Node> drawn)
        where TRequest : notnull
        where TValue : notnull =>
        new ControlRow.Bound<(TRequest Request, Func<TRequest, IO<TValue>> Compute, Func<TValue, IO<Unit>> Release, Func<TValue, Node> Drawn)>(label, RowShape.Block, (request, compute, release, drawn),
            static (latest, scope, retain) =>
                from held in atomIO(Option<(TRequest Request, Content<TValue> Content)>.None)
                from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
                from kept in DisposalOps.Acquire(IO.lift(() => new Retained<TValue>(values => latest.Bind(row => Callbacks.Each(values.Map(row.Row.Release))).Map(static _ => unit))), retain)
                from working in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
                let site = new CallbackSite(scope.Sink, typeof(ContentRows), nameof(Computed))
                let shown =
                    from row in latest
                    from current in held.ValueIO
                    from drew in view.Show(current.Map(static state => state.Content).IfNone(new Content<TValue>.Pending(None, None))
                        .Drawn(row.Row.Drawn, RowText.Localize("{0} unavailable", scope.Sink, arguments: [RowText.Localize(row.Label, scope.Sink).Local]).Local))
                    select drew
                from landed in Subscriptions.Idle<TRequest, Content<TValue>>(arrived => Landed(held, kept, latest, arrived).Bind(_ => shown))(site, retain)
                select new RowCells(view,
                    from row in latest
                    from prior in held.ValueIO
                    from changed in when(!prior.Exists(current => EqualityComparer<TRequest>.Default.Equals(current.Request, row.Row.Request)),
                        from drained in working.Drain
                        from pending in held.SwapIO(current => Some((row.Row.Request, current.Map(static state => state.Content.Requested(None)).IfNone(new Content<TValue>.Pending(None, None)))))
                        from forked in DisposalOps.Fork(
                            row.Row.Compute(row.Row.Request).Map(static value => (Content<TValue>)new Content<TValue>.Ready(value))
                                .Catch(static error => !error.Is(Errors.Cancelled), static error => IO.pure<Content<TValue>>(new Content<TValue>.Failed(error)))
                                .Bind(content => landed((row.Row.Request, content))),
                            working.Retain)
                        select unit).As()
                    from redrawn in shown
                    select unit));

    private static IO<Unit> Landed<TRequest, TValue>(
        Atom<Option<(TRequest Request, Content<TValue> Content)>> held, Retained<TValue> kept,
        IO<ControlRow.Bound<(TRequest Request, Func<TRequest, IO<TValue>> Compute, Func<TValue, IO<Unit>> Release, Func<TValue, Node> Drawn)>> latest, HashMap<TRequest, Content<TValue>> arrived)
        where TRequest : notnull
        where TValue : notnull =>
        toSeq(arrived.AsIterable()).TraverseM(entry =>
            from current in held.ValueIO
            from row in latest
            from placed in current.Exists(state => EqualityComparer<TRequest>.Default.Equals(state.Request, entry.Key))
                ? held.SwapIO(state => state.Map(prior => prior with { Content = entry.Value })).Bind(_ => entry.Value.Held.Traverse(kept.Replace).As()).Map(static _ => unit)
                : entry.Value.Held.Traverse(row.Row.Release).As().Map(static _ => unit)
            select unit).As().Map(static _ => unit);
}
