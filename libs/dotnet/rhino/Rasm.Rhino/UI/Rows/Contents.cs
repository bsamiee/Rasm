using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;

namespace Rasm.Rhino.UI.Rows;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentRows {
    // --- [COMPUTED]
    public static ControlRow Computed<TRequest, TValue>(
        ParameterText text, Func<RowScope, IO<Seq<HostEvent<Unit>>>> signals, Func<RowScope, IO<TRequest>> request, Func<TRequest, IO<TValue>> compute,
        Func<TValue, IO<Unit>> release, Func<TValue, Node> drawn, RowRules rules)
        where TRequest : notnull
        where TValue : notnull =>
        new ControlRow.Readout(RowShape.Block, (scope, retain) =>
            from held in atomIO(Option<(TRequest Request, Content<TValue> Content)>.None)
            from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
            from kept in DisposalOps.Acquire(IO.lift(() => new Retained<TValue>(values => Callbacks.Each(values.Map(release)).Map(static _ => unit))), retain)
            from working in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
            let site = new CallbackSite(scope.Sink, typeof(ContentRows), nameof(Computed))
            let unexpected = RowText.Localize("{0} unavailable", scope.Sink, arguments: [RowText.Localize(text.Caption, scope.Sink).Local]).Local
            let shown = held.ValueIO.Bind(current => view.Show(current.Map(static state => state.Content).IfNone(new Content<TValue>.Pending(None, None)).Drawn(drawn, unexpected)))
            from landed in Subscriptions.Idle<TRequest, Content<TValue>>(arrived => Landed(held, kept, release, arrived).Bind(_ => shown))(site, retain)
            let asked =
                from wanted in request(scope)
                from prior in held.ValueIO
                from changed in when(!prior.Exists(current => EqualityComparer<TRequest>.Default.Equals(current.Request, wanted)),
                    from drained in working.Drain
                    from pending in held.SwapIO(current => Some((wanted, current.Map(static state => state.Content.Requested(None)).IfNone(new Content<TValue>.Pending(None, None)))))
                    from redrawn in shown
                    from forked in DisposalOps.Fork(
                        compute(wanted).Map(static value => (Content<TValue>)new Content<TValue>.Ready(value))
                            .Catch(static error => !error.Is(Errors.Cancelled), static error => IO.pure<Content<TValue>>(new Content<TValue>.Failed(error)))
                            .Bind(content => landed((wanted, content))),
                        working.Retain)
                    select unit).As()
                select unit
            from posted in Subscriptions.Idle<Unit, Unit>(_ => asked)(site, retain)
            from raised in signals(scope)
            from subscribed in raised.TraverseM(signal => signal.Inline(each => posted((each, each)), scope.Sink, retain)).As()
            from heard in scope.Listen(_ => posted((unit, unit)), retain)
            select new RowCells(view, None, None, None, [], [], new RowHelp(None, None), asked.Bind(_ => shown), None, None),
            text, rules);

    private static IO<Unit> Landed<TRequest, TValue>(
        Atom<Option<(TRequest Request, Content<TValue> Content)>> held, Retained<TValue> kept, Func<TValue, IO<Unit>> release, HashMap<TRequest, Content<TValue>> arrived)
        where TRequest : notnull
        where TValue : notnull =>
        toSeq(arrived.AsIterable()).TraverseM(entry =>
            from current in held.ValueIO
            from placed in current.Exists(state => EqualityComparer<TRequest>.Default.Equals(state.Request, entry.Key))
                ? held.SwapIO(state => state.Map(prior => prior with { Content = entry.Value })).Bind(_ => entry.Value.Held.Traverse(kept.Replace).As()).Map(static _ => unit)
                : entry.Value.Held.Traverse(release).As().Map(static _ => unit)
            select unit).As().Map(static _ => unit);

    // --- [CONTEXTUAL]
    public static ControlRow Contextual<TRecord, TContext>(
        RowSource<TRecord> source, ParameterText text, Func<RowScope, IO<Option<TContext>>> context, Func<RowScope, IO<Seq<HostEvent<Unit>>>> signals,
        Func<TContext, Seq<ControlRow>> rows, RowRules rules)
        where TRecord : notnull
        where TContext : notnull =>
        source.Fields.Bind(source.Keys) switch {
            var keys => new ControlRow.Field(source, keys, RowShape.Nested, (scope, retain) => Contextualized(context, signals, rows, keys, scope, retain), text, rules),
        };

    private static IO<RowCells> Contextualized<TContext>(
        Func<RowScope, IO<Option<TContext>>> context, Func<RowScope, IO<Seq<HostEvent<Unit>>>> signals, Func<TContext, Seq<ControlRow>> rows, Seq<EntryKey> keys,
        RowScope scope, Action<IDisposable> retain)
        where TContext : notnull =>
        from held in atomIO(Option<(Option<TContext> Context, RowGrid Grid)>.None)
        from owned in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        from panel in Containers.Frame(static () => new Panel(), retain)
        let refresh =
            from current in context(scope)
            from prior in held.ValueIO
            from editing in prior.Traverse(static shown => shown.Grid.Editing).As()
            from replaced in editing.Contains(true) || prior.Exists(shown => shown.Context.Equals(current))
                ? prior.Traverse(shown => shown.Grid.Refresh(keys)).As().Map(static _ => unit)
                : Replaced(held, owned, panel, current, current.Map(rows).IfNone(Seq<ControlRow>()), scope)
            select replaced
        from post in Subscriptions.Idle<Unit, Unit>(_ => refresh)(new CallbackSite(scope.Sink, typeof(ContentRows), nameof(Contextual)), retain)
        from raised in signals(scope)
        from subscribed in raised.TraverseM(change => change.Inline(each => post((each, each)), scope.Sink, retain)).As()
        from heard in scope.Listen(_ => post((unit, unit)), retain)
        from initial in refresh
        select new RowCells(panel, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None) {
            NestedEditing = held.ValueIO.Bind(static current => current.Traverse(static shown => shown.Grid.Editing).As()).Map(static editing => editing.IfNone(noneValue: false)),
        };

    private static IO<Unit> Replaced<TContext>(
        Atom<Option<(Option<TContext> Context, RowGrid Grid)>> held, Retained<IDisposable> owned, Panel panel, Option<TContext> context, Seq<ControlRow> children, RowScope scope)
        where TContext : notnull =>
        DisposalOps.Acquire(
            keep => RowGrid.Realize(children, scope, keep),
            (grid, release) =>
                from published in DisposalOps.Published(owned, (Control?)grid.View, release, () => panel.Content, content => panel.Content = content)
                from shown in writeIO(held, Some((context, grid)))
                select unit)
            .Map(static _ => unit);
}
