using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
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
        from followed in ViewOps.WhileLoaded(control,
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
                from digits in HostTheme.Applied(made, HostPlatform.Current.Digits(made))
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
        from held in IO.lift(static () => Ref(Option<(TRequest Request, Loading<TValue> Content)>.None))
        from frame in Containers.Frame(static () => new Panel())
        from owned in use(static () => EnvIO.New(token: CancellationToken.None))
        from kept in IO.lift(static () => Ref(Option<Owned<TValue>>.None))
        from body in IO.lift(static () => Ref(Option<Owned<Control>>.None))
        from working in IO.lift(static () => Ref(Option<Owned<ForkIO<Unit>>>.None))
        let shown =
            from row in latest
            from current in atomicIO(() => held.Value)
            from control in Products.Replace(body, Placed(current.Map(static state => state.Content).IfNone(new Loading<TValue>.Pending(None)), row.Shown,
                LocalText.Of("{0} unavailable", label.Local).Local)).Scoped(owned)
            from seated in IO.lift(() => { frame.Content = control; })
            select unit
        from landed in Mailboxes.Idle<TRequest, Loading<TValue>>(arrived => Landed(held, kept, owned, latest, arrived).Bind(_ => shown))
        select new RowCells(frame,
            from row in latest
            from fresh in atomicIO(() => {
                Option<(TRequest Request, Loading<TValue> Content)> prior = held.Value;
                bool changed = !prior.Exists(current => EqualityComparer<TRequest>.Default.Equals(current.Request, row.Request));
                held.Value = changed ? Some((row.Request, (Loading<TValue>)new Loading<TValue>.Pending(None))) : prior;
                return changed;
            })
            from forked in when(fresh, Products.Clear(working).Bind(_ => Products.Replace(working, use(
                row.Compute(row.Request).Map(static value => (Loading<TValue>)new Loading<TValue>.Ready(value))
                    .Catch(static error => !error.Is(Errors.Cancelled), static error => IO.pure<Loading<TValue>>(new Loading<TValue>.Failed(error)))
                    .Bind(content => landed(row.Request, content))
                    .Fork(),
                static fork => fork.Cancel))).Scoped(owned).Map(unit)).As()
            from redrawn in shown
            select unit);

    private static IO<Control> Placed(Loading<TValue> content, Func<TValue, IO<Control>> shown, string unavailable) =>
        content.Switch(
            (Shown: shown, Unavailable: unavailable),
            pending: static (_, pending) =>
                from bar in use(() => new ProgressBar { Indeterminate = pending.Work.IsNone })
                from ranged in IO.lift(() => pending.Work.Iter(progress => (bar.MaxValue, bar.Value) = progress.Positions))
                select (Control)bar,
            ready: static (at, ready) => at.Shown(ready.Value),
            failed: static (at, _) => use(() => new Label { Text = at.Unavailable, TextColor = SystemColors.DisabledText }).Map(static label => (Control)label));

    private static IO<Unit> Landed(
        Ref<Option<(TRequest Request, Loading<TValue> Content)>> held, Ref<Option<Owned<TValue>>> kept, EnvIO owned, IO<ComputedRow<TRequest, TValue>> latest, HashMap<TRequest, Loading<TValue>> arrived) =>
        toSeq(arrived.AsIterable()).TraverseM(entry =>
            from current in atomicIO(() => {
                Option<(TRequest Request, Loading<TValue> Content)> prior = held.Value;
                bool matched = prior.Exists(state => EqualityComparer<TRequest>.Default.Equals(state.Request, entry.Key));
                held.Value = matched ? prior.Map(state => state with { Content = entry.Value }) : prior;
                return matched;
            })
            from row in latest
            from placed in current
                ? entry.Value.Held.Traverse(value => Products.Clear(kept).Bind(_ => Products.Replace(kept, use(IO.pure(value), stored => latest.Bind(last => last.Release(stored))))).Scoped(owned)).As().Map(unit)
                : entry.Value.Held.Traverse(row.Release).As().Map(unit)
            select unit).As().Map(unit);
}
