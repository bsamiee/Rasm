using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rasm.RhinoCommon;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Lease<T>(T Value, EnvIO Scope);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Products {
    // --- [PRODUCTS]
    public static IO<T> Made<T>(T? product, [CallerArgumentExpression(nameof(product))] string call = "") where T : class, IDisposable =>
        Held(product, call).Bind(held => IO.lift(held.ToFin(new RhinoFault(call, RhinoReason.Absent))));

    public static IO<Seq<T>> Made<T>(IEnumerable<T?>? products, [CallerArgumentExpression(nameof(products))] string call = "") where T : class, IDisposable =>
        products switch {
            null => IO.fail<Seq<T>>(new RhinoFault(call, RhinoReason.Absent)),
            _ => toSeq(products).Strict() switch {
                var rows => rows.Choose(static product => Optional(product)).TraverseM(static product => use(() => product)).As()
                    .Bind(_ => IO.lift(rows.Map((product, index) => Valid(product, call, index)).Traverse(static held => held).As())),
            },
        };

    public static IO<Option<T>> Held<T>(T? product, [CallerArgumentExpression(nameof(product))] string call = "") where T : class, IDisposable =>
        Optional(product).Traverse(held => use(() => held).Bind(used => IO.lift(Valid(used, call, None)))).As();

    private static Fin<T> Valid<T>(T? product, string call, Option<int> index) where T : class =>
        product switch {
            null => new RhinoFault(call, RhinoReason.Absent, index),
            CommonObject geometry when !geometry.IsValidWithLog(out string log) => new RhinoFault(call, RhinoReason.Invalid, index, Error.New(log)),
            { } held => held,
        };

    // --- [RELEASES]
    public static IO<Unit> Released(Action release) => use(() => new StrongBox<Action>(release), static box => box.Value!()).Map(unit);

    public static IO<Unit> Changed<T>(Func<T> read, Action<T> write, T value) =>
        IO.lift(read).Bind(prior => Released(() => write(prior)) >> IO.lift(() => write(value)));

    // --- [LEASES]
    public static IO<T> Swapped<T>(Ref<Option<Lease<T>>> slot, IO<T> body) =>
        from fresh in IO.lift(static () => EnvIO.New())
        from value in body.Under(fresh).Catch(error => IO.lift(fresh.Dispose) >> IO.fail<T>(error))
        from owned in use(() => fresh)
        from prior in atomicIO(() => {
            Option<Lease<T>> held = slot.Value;
            slot.Value = Some(new Lease<T>(value, fresh));
            return held;
        })
        from released in prior.Traverse(static lease => release(lease.Scope)).As()
        select value;

    public static IO<Unit> Cleared<T>(Ref<Option<Lease<T>>> slot) =>
        atomicIO(() => {
            Option<Lease<T>> held = slot.Value;
            slot.Value = None;
            return held;
        }).Bind(static prior => prior.Traverse(static lease => release(lease.Scope)).As()).Map(unit);

    // --- [SCOPES]
    extension<T>(IO<T> effect) {
        public IO<T> Under(EnvIO scope) => IO.liftVAsync(_ => effect.RunAsync(scope));
    }
}
