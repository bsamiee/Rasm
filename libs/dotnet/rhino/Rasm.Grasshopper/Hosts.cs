using System.Runtime.CompilerServices;
using Eto.Forms;
using Microsoft.VisualStudio.Threading;

namespace Rasm.Grasshopper;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Lease<T>(T Value, EnvIO Scope);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed partial class PluginSession(ILogger logger) {
    // --- [STATE]
    private static readonly JoinableTaskContext Threading = Application.Instance.Invoke(static () =>
        Application.Instance.Properties.Create(typeof(JoinableTaskContext), static () => new JoinableTaskContext()));

    private readonly EnvIO environment = EnvIO.New(token: CancellationToken.None);

    // --- [SCOPE]
    public IO<T> Held<T>(IO<T> effect) => effect.Under(environment);

    public IO<Unit> Open<TOwner>(TOwner owner, Seq<Func<TOwner, IO<Unit>>> rows) => Held(rows.TraverseM(row => row(owner)).As().Map(unit));

    public IO<Unit> Close => IO.lift(environment.Dispose);

    // --- [RUNS]
    public static Fin<T> Settled<T>(IO<T> effect, CancellationToken token) =>
        IO.lift(() => Threading.Factory.Run(async () => {
            using EnvIO scope = EnvIO.New(token: token, syncContext: SynchronizationContext.Current);
            return await effect.RunAsync(scope).ConfigureAwait(false);
        })).RunSafe();

    public T Answer<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") =>
        Settled(effect, CancellationToken.None).IfFail(error => {
            if (error.Count is 1 && error.Is(Errors.Cancelled)) Cancelled(logger, member);
            else Failed(logger, member, error.Message, toSeq(error.AsIterable()).Choose(static cause => cause.Exception) switch { [] => null, [var single] => single, var many => new AggregateException(many) });
            return failed;
        });

    public Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => Answer(effect.Map(unit), unit, member);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Member} failed: {Message}")]
    private static partial void Failed(ILogger logger, string member, string message, Exception? exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Member} cancelled")]
    private static partial void Cancelled(ILogger logger, string member);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Hosts {
    // --- [ANSWERS]
    public static Fin<T> Found<T>(T? value, [CallerArgumentExpression(nameof(value))] string call = "") where T : class =>
        Optional(value).ToFin(new GrasshopperFault(call, GrasshopperReason.Missing));

    public static Fin<T> Found<T>(Option<T> value, [CallerArgumentExpression(nameof(value))] string call = "") =>
        value.ToFin(new GrasshopperFault(call, GrasshopperReason.Missing));
}

public static class Hooks {
    // --- [EVENTS]
    public static IO<Unit> On(Action<EventHandler> add, Action<EventHandler> remove, IO<Unit> react, IGrasshopperPlugin owner, [CallerArgumentExpression(nameof(add))] string label = "") =>
        Attached((_, _) => owner.Session.Run(react, label), add, remove);

    public static IO<Unit> On<TArgs>(Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, Func<TArgs, IO<Unit>> react, IGrasshopperPlugin owner, [CallerArgumentExpression(nameof(add))] string label = "") =>
        Attached((_, args) => owner.Session.Run(react(args), label), add, remove);

    private static IO<Unit> Attached<THandler>(THandler handler, Action<THandler> add, Action<THandler> remove) where THandler : Delegate =>
        use(() => {
            add(handler);
            return handler;
        }, remove).Map(unit);
}

public static class Leases {
    // --- [SLOTS]
    public static IO<T> Swapped<T>(Ref<Option<Lease<T>>> slot, IO<T> body) =>
        from fresh in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
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
