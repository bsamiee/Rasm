using System.Collections.Generic;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;

namespace Rasm.RhinoCommon.UI.Rows;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentRows {
    // --- [COMPUTED]
    public static ControlRow Computed<TRequest, TValue>(string label, TRequest request, Func<TRequest, IO<TValue>> compute, Func<TValue, IO<Unit>> release, Func<TValue, Node> drawn)
        where TRequest : notnull
        where TValue : notnull =>
        new ControlRow.Bound<(TRequest Request, Func<TRequest, IO<TValue>> Compute, Func<TValue, IO<Unit>> Release, Func<TValue, Node> Drawn)>(label, RowShape.Block, (request, compute, release, drawn),
            static (latest, scope) =>
                from held in atomIO(Option<(TRequest Request, Content<TValue> Content)>.None)
                from view in use(() => new SceneView(scope, new Node.Box()))
                from owned in use(static () => EnvIO.New(token: CancellationToken.None))
                from kept in IO.lift(static () => Ref(Option<Lease<TValue>>.None))
                from working in IO.lift(static () => Ref(Option<Lease<ForkIO<Unit>>>.None))
                let shown =
                    from row in latest
                    from current in held.ValueIO
                    from drew in view.Show(current.Map(static state => state.Content).IfNone(new Content<TValue>.Pending())
                        .Drawn(row.Row.Drawn, RowText.Localize("{0} unavailable", scope.Owner, arguments: [RowText.Localize(row.Label, scope.Owner).Local]).Local))
                    select drew
                from landed in Mailboxes.Idle<TRequest, Content<TValue>>(arrived => Landed(held, kept, owned, latest, arrived).Bind(_ => shown))(scope.Owner)
                select new RowCells(view,
                    from row in latest
                    from prior in held.ValueIO
                    from changed in when(!prior.Exists(current => EqualityComparer<TRequest>.Default.Equals(current.Request, row.Row.Request)),
                        from pending in held.SwapIO(current => Some((row.Row.Request, current.Map(static state => state.Content.Requested(None)).IfNone(new Content<TValue>.Pending()))))
                        from forked in Products.Cleared(working).Bind(_ => Products.Swapped(working, use(
                            row.Row.Compute(row.Row.Request).Map(static value => (Content<TValue>)new Content<TValue>.Ready(value))
                                .Catch(static error => !error.Is(Errors.Cancelled), static error => IO.pure<Content<TValue>>(new Content<TValue>.Failed(error)))
                                .Bind(content => landed(row.Row.Request, content))
                                .Fork(),
                            static fork => fork.Cancel))).Under(owned)
                        select unit).As()
                    from redrawn in shown
                    select unit));

    private static IO<Unit> Landed<TRequest, TValue>(
        Atom<Option<(TRequest Request, Content<TValue> Content)>> held, Ref<Option<Lease<TValue>>> kept, EnvIO owned,
        IO<ControlRow.Bound<(TRequest Request, Func<TRequest, IO<TValue>> Compute, Func<TValue, IO<Unit>> Release, Func<TValue, Node> Drawn)>> latest, HashMap<TRequest, Content<TValue>> arrived)
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
