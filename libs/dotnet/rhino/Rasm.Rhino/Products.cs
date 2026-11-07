using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Threading;
using Rhino.Runtime;

namespace Rasm.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class Owned<T> {
    internal Owned(T value, EnvIO scope, Resources owner) => (Value, Scope, this.owner) = (value, scope, owner);

    private readonly Resources owner;

    public T Value { get; }

    public EnvIO Scope { get; }

    public IO<Unit> Release => IO.liftVAsync(environment => owner.Release(this).RunAsync(environment));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Products {

    // --- [PRODUCTS]

    public static IO<T> Of<T>(Func<T?> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
        Maybe(call, member).Bind(found => IO.lift(found.ToFin(new RhinoFault(member, RhinoReason.Missing))));

    public static IO<Option<T>> Maybe<T>(Func<T?> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
        IO.lift(call).Bind(product => Optional(product).Traverse(held => Owning(held, member, None)).As());

    public static IO<Seq<T>> Many<T>(Func<IEnumerable<T?>?> call, bool required = true, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
        IO.lift(call).Bind(products => Listed(products, member, required));

    public static IO<(Seq<T> Values, TOut Out)> Many<T, TOut>(Func<(IEnumerable<T?>? Values, TOut Out)> call, bool required = true, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
        IO.lift(call).SelectMany(answered => Listed(answered.Values, member, required), static (answered, values) => (values, answered.Out));

    public static IO<(T Value, TOut Out)> Of<T, TOut>(Func<(T? Value, TOut Out)> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
        IO.lift(call).Bind(answered => Optional(answered.Value).Match(
            Some: held => Owning(held, member, None).Map(owned => (owned, answered.Out)),
            None: () => IO.fail<(T, TOut)>(new RhinoFault(member, RhinoReason.Missing))));

    public static IO<TOut> Accepted<TOut>(Func<(bool Accepted, TOut Out)> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        IO.lift(Fin<TOut> () => call() switch {
            (true, var value) => value,
            _ => new RhinoFault(member, RhinoReason.Declined),
        });

    public static IO<Unit> Accepted(Func<bool> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        IO.lift(() => call() ? Fin.Succ(unit) : new RhinoFault(member, RhinoReason.Declined));

    public static IO<T> Copied<T>(T source, [CallerArgumentExpression(nameof(source))] string member = "") where T : GeometryBase =>
        use(IO.lift(() => Conversions.Required(source.Duplicate() as T, call: member)));

    private static IO<Seq<T>> Listed<T>(IEnumerable<T?>? products, string label, bool required) where T : class =>
        Optional(products).Map(static rows => toSeq(rows).Strict()).Filter(rows => !required || !rows.IsEmpty).Match(
            Some: rows => Each(rows.Map((row, index) => Optional(row).Match(
                Some: held => Owning(held, label, index),
                None: () => IO.fail<T>(new RhinoFault(label, RhinoReason.Missing, index))))),
            None: () => IO.fail<Seq<T>>(new RhinoFault(label, RhinoReason.Missing)));

    private static IO<T> Owning<T>(T product, string label, Option<int> index) where T : class =>
        when(product is IDisposable, use(() => (IDisposable)product).Map(unit)).As() >> IO.lift(() => product switch {
            CommonObject geometry when !geometry.IsValidWithLog(out string log) => Fin.Fail<T>(new RhinoFault(label, RhinoReason.Invalid, index, Error.New(log))),
            var held => held,
        });

    // --- [SCOPES]

    internal static IO<T> Scoped<T>(
        Func<T> call, [CallerArgumentExpression(nameof(call))] string member = "", [CallerFilePath] string file = "", [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0) =>
        (from risky in use(() => new RiskyAction(member, file, caller, line))
         from value in IO.lift(call)
         select value).Bracket();

    internal static IO<Unit> Scoped(
        Action call, [CallerArgumentExpression(nameof(call))] string member = "", [CallerFilePath] string file = "", [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0) =>
        Scoped(fun(call), member, file, caller, line);

    // --- [CALLBACKS]

    public static IO<T> Captured<T>(Func<Action<Fin<T>>, IO<Unit>> host, [CallerArgumentExpression(nameof(host))] string call = "") =>
        from answer in atomIO(Option<Fin<T>>.None)
        from hosted in host(value => answer.Swap(_ => Some(value)))
        from value in IO.lift(answer.Value.ToFin(new RhinoFault(call, RhinoReason.Missing)).Bind(static result => result))
        select value;

    public static IO<T> Captured<TArgs, T>(Action<Action<TArgs>> host, Func<TArgs, IO<T>> body, [CallerArgumentExpression(nameof(host))] string call = "") =>
        Captured<T>(answer => IO.lift(() => host(args => answer(Session.RunSafe(body(args), CancellationToken.None)))), call);

    // --- [TRAVERSALS]

    public static Fin<Seq<T>> Each<TItem, T>(Seq<TItem> items, Func<TItem, int, Fin<T>> element) =>
        items.Map(element).Strict().Traverse(static answer => answer).As();

    public static IO<Seq<T>> Each<T>(Seq<IO<T>> effects) =>
        effects.Map<K<IO, T>>(static effect => effect).PartitionFallible().As()
            .Bind(static outcome => outcome.Fails.IsEmpty ? IO.pure(outcome.Succs) : IO.fail<Seq<T>>(Error.Many(outcome.Fails)));

    // --- [RELEASES]

    public static IO<Unit> Released(Action release) => use(static () => new object(), _ => IO.lift(release)).Map(unit);

    public static IO<Unit> Released(IO<Unit> release) => use(static () => new object(), _ => release).Map(unit);

    public static IO<Unit> Changed<T>(Func<T> read, Action<T> write, T value) =>
        IO.lift(read).Bind(prior => Released(() => write(prior)) >> IO.lift(() => write(value)));

    public static IO<Unit> Changed<T>(Func<T> read, Action<T> write, Func<T, T> change) =>
        IO.lift(read).Bind(prior => Released(() => write(prior)) >> IO.lift(() => write(change(prior))));

    // --- [OWNERSHIP]

    public static IO<T> Acquired<T>(IO<T> acquire) where T : IDisposable => Acquired(acquire, static held => IO.lift(held.Dispose));

    public static IO<T> Acquired<T>(IO<T> acquire, Func<T, IO<Unit>> release) =>
        IO.env.Bind(environment => use(acquire.Scoped(environment), release).Uninterruptible());

    public static IO<Owned<T>> Owned<T>(IO<T> body) =>
        from parent in IO.env
        from owned in Acquired(
            (from scope in IO.env
             from value in body
             select new Owned<T>(value, scope, parent.Resources)).BracketFail(),
            static held => IO.lift(held.Scope.Dispose))
        select owned;

    public static IO<ForkIO<T>> Forked<T>(IO<T> body) =>
        IO.env.Bind(parent => Owned(use(Transfer(
                () => EnvIO.New(token: parent.Token),
                scope => body.Scoped(scope).Fork().Map(worker => (Scope: scope, Worker: worker))), static run =>
                    Each(Seq(
                            IO.liftAsync(() => run.Scope.Source.CancelAsync().ToUnit()),
                            run.Worker.Await.Map(unit) | @catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit))))
                        .Map(unit).Finally(IO.lift(run.Scope.Dispose)).Uninterruptible()))
            .Map(static held => new ForkIO<T>(held.Release, held.Value.Worker.Await.Uninterruptible())).Uninterruptible());

    public static IO<T> Serialized<T>(Func<Func<IO<Unit>, IO<Unit>>, IO<(T Value, IO<Unit> Close)>> open) =>
        use(Transfer(static () => new AsyncSemaphore(1), gate =>
                Transfer(static () => new CancellationTokenSource(), closing =>
                    Transfer(static () => EnvIO.New(token: CancellationToken.None), scope =>
                        from token in IO.lift(() => closing.Token)
                        from opened in open(body => Acquired(IO.liftAsync(() => gate.EnterAsync(token))).Bind(_ => body.Scoped(scope.Resources)).Bracket()).Scoped(scope)
                        select (opened.Value, opened.Close, Scope: scope, Gate: gate, Closing: closing)))),
                static held => IO.liftAsync(() => held.Closing.CancelAsync().ToUnit()).Finally(
                    (from entered in Acquired(IO.liftAsync(() => held.Gate.EnterAsync(CancellationToken.None)))
                     from closed in held.Close.Scoped(held.Scope)
                     select unit).Bracket()
                        .Finally(IO.lift(held.Scope.Dispose))
                        .Finally(IO.lift(held.Gate.Dispose))
                        .Finally(IO.lift(held.Closing.Dispose))).Uninterruptible())
            .Map(static held => held.Value).Uninterruptible();

    public static IO<TOut> Transfer<T, TOut>(Func<T?> make, Func<T, IO<TOut>> to, [CallerArgumentExpression(nameof(make))] string member = "") where T : class, IDisposable =>
        use(IO.lift(() => Conversions.Required(make(), call: member))).Bind(to).BracketFail();

    public static IO<T> Replace<T>(Ref<Option<Owned<T>>> slot, IO<T> body) =>
        Replaced(body, owned => {
            Option<Owned<T>> held = slot.Value;
            slot.Value = Some(owned);
            return held;
        });

    public static IO<T> Replace<TKey, T>(Ref<HashMap<TKey, Owned<T>>> slots, TKey key, IO<T> body) =>
        Replaced(body, owned => {
            Option<Owned<T>> held = slots.Value.Find(key);
            slots.Value = slots.Value.AddOrUpdate(key, owned);
            return held;
        });

    public static IO<Unit> Clear<T>(Ref<Option<Owned<T>>> slot) =>
        atomicIO(() => {
            Option<Owned<T>> held = slot.Value;
            slot.Value = None;
            return held;
        }).Bind(Release);

    public static IO<Unit> Clear<TKey, T>(Ref<HashMap<TKey, Owned<T>>> slots, TKey key) =>
        atomicIO(() => {
            Option<Owned<T>> held = slots.Value.Find(key);
            slots.Value = slots.Value.Remove(key);
            return held;
        }).Bind(Release);

    private static IO<T> Replaced<T>(IO<T> body, Func<Owned<T>, Option<Owned<T>>> swap) =>
        from fresh in Owned(body)
        from prior in atomicIO(() => swap(fresh))
        from released in Release(prior)
        select fresh.Value;

    private static IO<Unit> Release<T>(Option<Owned<T>> prior) => prior.Traverse(static owned => owned.Release).As().Map(unit);
}
