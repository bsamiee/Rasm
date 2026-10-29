using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rhino.Runtime;

namespace Rasm.Rhino;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Owned<T>(T Value, EnvIO Scope);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Products {
    // --- [PRODUCTS]
    public static IO<T> Of<T>(Func<T?> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
        Present(call, Label(member)).Bind(found => IO.lift(found.ToFin(new RhinoFault(Label(member), RhinoReason.Missing))));

    public static IO<Option<T>> Maybe<T>(Func<T?> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : class => Present(call, Label(member));

    public static IO<Seq<T>> Many<T>(Func<IEnumerable<T?>?> call, bool required = true, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
        IO.lift(call).Bind(products => Listed(products, Label(member), required));

    public static IO<(T Value, TOut Out)> Of<T, TOut>(Func<(T? Value, TOut Out)> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
        IO.lift(call).Bind(answered => Optional(answered.Value).Match(
            Some: held => Owning(held, Label(member), None).Map(owned => (owned, answered.Out)),
            None: () => IO.fail<(T, TOut)>(new RhinoFault(Label(member), RhinoReason.Missing))));

    public static IO<TOut> Accepted<TOut>(Func<(bool Accepted, TOut Out)> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        IO.lift(Fin<TOut> () => call() switch {
            (true, var value) => value,
            _ => new RhinoFault(Label(member), RhinoReason.Declined),
        });

    public static IO<Unit> Accepted(Func<bool> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        IO.lift(() => call() ? Fin.Succ(unit) : new RhinoFault(Label(member), RhinoReason.Declined));

    private static IO<Option<T>> Present<T>(Func<T?> call, string label) where T : class =>
        IO.lift(call).Bind(product => Optional(product).Traverse(held => Owning(held, label, None)).As());

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

    private static string Label(string member) =>
        Seq("static () => ", "() => ").Find(prefix => member.StartsWith(prefix, StringComparison.Ordinal)).Map(prefix => member[prefix.Length..]).IfNone(member);

    // --- [CALLBACKS]
    public static IO<T> Captured<T>(Func<Action<Fin<T>>, IO<Unit>> host, [CallerArgumentExpression(nameof(host))] string call = "") =>
        from answer in atomIO(Option<Fin<T>>.None)
        from hosted in host(value => answer.Swap(_ => Some(value)))
        from value in IO.lift(() => answer.Value.ToFin(new RhinoFault(call, RhinoReason.Missing)).Bind(static result => result))
        select value;

    public static IO<T> Captured<TArgs, T>(Action<Action<TArgs>> host, Func<TArgs, IO<T>> body, [CallerArgumentExpression(nameof(host))] string call = "") =>
        Captured<T>(answer => IO.lift(() => host(args => answer(Session.RunSafe(body(args), CancellationToken.None)))), call);

    // --- [TRAVERSALS]
    public static Fin<Seq<T>> Each<TItem, T>(Seq<TItem> items, Func<TItem, int, Fin<T>> element) =>
        items.Map(element).Strict().Traverse(static answer => answer).As();

    public static IO<Seq<T>> Each<T>(Seq<IO<T>> effects) =>
        effects.Map<K<IO, T>>(static effect => effect).PartitionFallible().As()
            .Bind(static settled => settled.Fails.IsEmpty ? IO.pure(settled.Succs) : IO.fail<Seq<T>>(Error.Many(settled.Fails)));

    // --- [RELEASES]
    public static IO<Unit> Released(Action release) => use(() => new StrongBox<Action>(release), static box => box.Value!()).Map(unit);

    public static IO<Unit> Changed<T>(Func<T> read, Action<T> write, T value) =>
        IO.lift(read).Bind(prior => Released(() => write(prior)) >> IO.lift(() => write(value)));

    // --- [OWNERSHIP]
    public static IO<TOut> Transfer<T, TOut>(T value, Func<T, IO<TOut>> to) where T : IDisposable =>
        to(value).Catch(error => IO.lift(value.Dispose) >> IO.fail<TOut>(error)).As();

    public static IO<TOut> Transfer<T, TOut>(Func<T?> make, Func<T, IO<TOut>> to, [CallerArgumentExpression(nameof(make))] string member = "") where T : class, IDisposable =>
        IO.lift(make).Bind(made => Optional(made).Match(Some: held => Transfer(held, to), None: () => IO.fail<TOut>(new RhinoFault(Label(member), RhinoReason.Missing))));

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
        from fresh in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
        from value in Transfer(fresh, body.Scoped)
        from kept in use(() => fresh)
        from prior in atomicIO(() => swap(new Owned<T>(value, fresh)))
        from released in Release(prior)
        select value;

    private static IO<Unit> Release<T>(Option<Owned<T>> prior) => prior.Traverse(static owned => release(owned.Scope)).As().Map(unit);
}
