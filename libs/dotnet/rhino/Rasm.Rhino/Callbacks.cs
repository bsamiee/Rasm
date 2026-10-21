using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Threading;

namespace Rasm.Rhino;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CallbackSite(IPlugInSink Sink, Type Owner, string Member) {
    public static CallbackSite Of(object constructed, [CallerMemberName] string member = "") =>
        new(IPlugInSink.Of(constructed), constructed.GetType(), member);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Callbacks {
    // --- [SETTLING]
    private static readonly JoinableTaskContext Threading = Eto.Forms.Application.Instance.Invoke(static () =>
        Eto.Forms.Application.Instance.Properties.Create(typeof(JoinableTaskContext), static () => new JoinableTaskContext()));

    private static readonly JoinableTaskCollection Pending = Threading.CreateCollection();

    public static Fin<T> Settled<T>(IO<T> effect, CancellationToken token) =>
        Try.lift(() => Threading.Factory.Run(async () => {
            using EnvIO environment = EnvIO.New(token: token, syncContext: SynchronizationContext.Current);
            return await effect.RunAsync(environment).ConfigureAwait(false);
        })).Run();

    // --- [ANSWERS]
    public static T Answer<T>(IO<T> effect, Func<T> failed, CallbackSite site) =>
        Answer(Settled(effect, CancellationToken.None), failed, site);

    public static T Answer<T>(Fin<T> answer, Func<T> failed, CallbackSite site) =>
        answer.IfFail(error => {
            site.Sink.Report(error, site.Owner, site.Member);
            return failed();
        });

    public static T Answer<TArgs, T>(TArgs args, Func<TArgs, IO<T>> effect, Func<T> failed, CallbackSite site) =>
        Answer(IO.pure(args).Bind(effect), failed, site);

    public static T Answer<T>(Option<IO<T>> effect, Func<T> absent, Func<T> failed, CallbackSite site) =>
        effect.Match(Some: run => Answer(run, failed, site), None: absent);

    public static bool Succeeded<T>(IO<T> effect, CallbackSite site) =>
        Answer(effect.Map(value: true), static () => false, site);

    public static bool Succeeded<T>(Fin<T> answer, CallbackSite site) =>
        Answer(answer.Map(static _ => true), static () => false, site);

    public static bool Succeeded<T>(Option<IO<T>> effect, Func<bool> absent, CallbackSite site) =>
        effect.Match(Some: run => Succeeded(run, site), None: absent);

    public static void Run<T>(IO<T> effect, CallbackSite site) =>
        Run(Settled(effect, CancellationToken.None), site);

    public static void Run<T>(Fin<T> answer, CallbackSite site) =>
        answer.IfFail(error => site.Sink.Report(error, site.Owner, site.Member));

    public static void Run<T>(Option<IO<T>> effect, CallbackSite site) =>
        effect.Iter(run => Run(run, site));

    public static void Started<T>(IO<T> effect, CallbackSite site) =>
        Started(effect.Map(unit).IfFail(error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member))));

    internal static void Started(IO<Unit> effect) =>
        Pending.Add(Threading.Factory.RunAsync(() => effect.RunAsync(CancellationToken.None).AsTask()));

    public static EventHandler<TArgs> Handler<TArgs>(Func<TArgs, IO<Unit>> deliver, CallbackSite site) =>
        (_, args) => Started(IO.pure(args).Bind(deliver), site);

    internal static IO<Unit> Drained =>
        IO.liftAsync(static environment => Pending.JoinTillEmptyAsync(environment.Token).ToUnit());

    // --- [CALLS]
    public static IO<T> Captured<T>(Func<Action<Fin<T>>, IO<Unit>> host, string member) =>
        CapturedOptional(host).Bind(answer => IO.lift(() => answer.ToFin(new Refused(member, Refused.Absent))));

    public static IO<Option<T>> CapturedOptional<T>(Func<Action<Fin<T>>, IO<Unit>> host) =>
        from answer in atomIO(Option<Fin<T>>.None)
        from _ in host(value => answer.Swap(_ => Some(value)))
        from value in answer.Value.Traverse(static result => IO.lift(() => result)).As()
        select value;

    public static IO<T> Captured<TArgs, T>(Action<Action<TArgs>> host, Func<TArgs, IO<T>> body, string member) =>
        Captured<T>(answer => IO.lift(() => host(args => answer(Settled(body(args), CancellationToken.None)))), member);

    public static Fin<Seq<T>> Each<TItem, T>(Seq<TItem> items, Func<TItem, int, Fin<T>> element) =>
        items.Map(element).Strict().Traverse(static answer => answer).As();

    public static Fin<Unit> Each<TItem>(Seq<TItem> items, Func<TItem, bool> call, string member) =>
        Each(items, (item, index) => Refused.Unless(call(item), member, index)).Map(static _ => unit);

    public static IO<Seq<T>> Each<T>(Seq<IO<T>> effects) =>
        effects.PartitionFallible().As().Bind(static parts => parts.Fails.IsEmpty ? IO.pure(parts.Succs) : IO.fail<Seq<T>>(Error.Many(parts.Fails)));

    public static Option<T> Found<T>(bool found, T value) =>
        found ? Some(value) : None;

    // --- [THREADS]
    extension<T>(IO<T> effect) {
        public IO<T> OnUiThread() =>
            IO.liftVAsync(async environment => {
                await Threading.Factory.SwitchToMainThreadAsync(environment.Token);
                return await effect.RunAsync(environment).ConfigureAwait(false);
            });
    }

    // --- [LOOKUPS]
    public static Fin<Seq<TRow>> Unique<TRow, TKey>(Seq<TRow> rows, Func<TRow, TKey> key, string member, IEqualityComparer<TKey>? comparer = null) where TKey : notnull =>
        toSeq(rows.CountBy(key, comparer))
            .Filter(static counted => counted.Value > 1)
            .Traverse(counted => Fin.Fail<Unit>(Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"{member} received {counted.Key} {counted.Value} times"))))
            .As()
            .Map(_ => rows);
}
