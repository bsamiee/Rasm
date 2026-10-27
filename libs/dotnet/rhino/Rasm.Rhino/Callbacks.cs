using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Threading;

namespace Rasm.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Callbacks {
    // --- [JOINING]
    private static readonly JoinableTaskContext Threading = Eto.Forms.Application.Instance.Invoke(static () =>
        Eto.Forms.Application.Instance.Properties.Create(typeof(JoinableTaskContext), static () => new JoinableTaskContext()));

    private static readonly JoinableTaskCollection Pending = Threading.CreateCollection();

    public static Fin<T> Settled<T>(IO<T> effect, CancellationToken token) =>
        Try.lift(() => Threading.Factory.Run(async () => {
            using EnvIO environment = EnvIO.New(token: token, syncContext: SynchronizationContext.Current);
            return await effect.RunAsync(environment).ConfigureAwait(false);
        })).Run();

    internal static Unit Started(IO<Unit> effect) {
        Pending.Add(Threading.Factory.RunAsync(() => effect.RunAsync(CancellationToken.None).AsTask()));
        return unit;
    }

    internal static IO<Unit> Drained =>
        IO.liftAsync(static environment => Pending.JoinTillEmptyAsync(environment.Token).ToUnit());

    // --- [THREADS]
    extension<T>(IO<T> effect) {
        public IO<T> OnUiThread() =>
            IO.liftVAsync(async environment => {
                await Threading.Factory.SwitchToMainThreadAsync(environment.Token);
                return await effect.RunAsync(environment).ConfigureAwait(false);
            });
    }

    // --- [OWNERS]
    extension(object owner) {
        public T Answer<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") =>
            Settled(effect, CancellationToken.None).IfFail(error => {
                PlugInLog.Of(owner).Failed(error, owner.GetType(), member);
                return failed;
            });

        public Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => owner.Answer(effect.Map(unit), unit, member);

        public Unit Start<T>(IO<T> effect, [CallerMemberName] string member = "") =>
            Started(effect.Map(unit).IfFail(error => IO.lift(() => PlugInLog.Of(owner).Failed(error, owner.GetType(), member))));

        public async Task<Fin<T>> Awaited<T>(IO<T> effect) {
            try {
                return await effect.RunSafeAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException exception) {
                return Fin.Fail<T>(Error.New(exception));
            }
        }
    }

    // --- [CALLS]
    public static IO<T> Captured<T>(Func<Action<Fin<T>>, IO<Unit>> host, [CallerArgumentExpression(nameof(host))] string call = "") =>
        from answer in atomIO(Option<Fin<T>>.None)
        from hosted in host(value => answer.Swap(_ => Some(value)))
        from value in IO.lift(() => answer.Value.ToFin(new RhinoFault(call, RhinoReason.Absent)).Bind(static result => result))
        select value;

    public static IO<T> Captured<TArgs, T>(Action<Action<TArgs>> host, Func<TArgs, IO<T>> body, [CallerArgumentExpression(nameof(host))] string call = "") =>
        Captured<T>(answer => IO.lift(() => host(args => answer(Settled(body(args), CancellationToken.None)))), call);

    // --- [TRAVERSALS]
    public static Fin<Seq<T>> Each<TItem, T>(Seq<TItem> items, Func<TItem, int, Fin<T>> element) =>
        items.Map(element).Strict().Traverse(static answer => answer).As();

    public static IO<Seq<T>> Each<T>(Seq<IO<T>> effects) =>
        effects.TraverseM(static effect => effect.Map(Fin.Succ<T>).Catch(static error => IO.pure(Fin.Fail<T>(error)))).As()
            .Bind(static settled => IO.lift(settled.Traverse(static answer => answer).As()));
}
