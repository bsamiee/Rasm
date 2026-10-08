using System.Runtime.CompilerServices;

namespace Rasm.Rhino;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CallbackSite(IPlugInSink Sink, Type Owner, string Member) {
    public static CallbackSite Of(object constructed, [CallerMemberName] string member = "") =>
        new(IPlugInSink.Of(constructed), constructed.GetType(), member);

    public static CallbackSite Of(IPlugInSink sink, [CallerMemberName] string member = "") =>
        new(sink, sink.GetType(), member);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Callbacks {
    // --- [ANSWERS]
    public static T Answer<T>(IO<T> effect, Func<T> failed, CallbackSite site) =>
        Answer(Try.lift(effect.Run).Run(), failed, site);

    public static T Answer<T>(Fin<T> answer, Func<T> failed, CallbackSite site) =>
        answer.IfFail(error => {
            site.Sink.Report(error, site.Owner, site.Member);
            return failed();
        });

    public static T Answer<TArgs, T>(TArgs args, Func<TArgs, IO<T>> effect, Func<T> failed, CallbackSite site) =>
        Answer(IO.pure(args).Bind(effect), failed, site);

    public static T Answer<T>(Option<IO<T>> effect, Func<T> absent, Func<T> failed, CallbackSite site) =>
        effect.Match(Some: run => Answer(run, failed, site), None: absent);

    public static bool Succeeded(IO<Unit> effect, CallbackSite site) =>
        Answer(effect.Map(static _ => true), static () => false, site);

    public static bool Succeeded(Option<IO<Unit>> effect, Func<bool> absent, CallbackSite site) =>
        Answer(effect.Map(static run => run.Map(static _ => true)), absent, static () => false, site);

    public static EventHandler<TArgs> Handler<TArgs>(Func<TArgs, IO<Unit>> deliver, CallbackSite site) =>
        (_, args) => _ = Answer(args, deliver, static () => unit, site);

    // --- [CALLS]
    public static Fin<T> Captured<T>(Action<Action<Fin<T>>> host, string member) {
        Option<Fin<T>> answer = None;
        host(value => answer = Some(value));
        return answer.IfNone(() => new Missing(member));
    }

    public static async Task<Fin<T>> CapturedAsync<T>(Func<Action<Fin<T>>, Task> host, string member) {
        Option<Fin<T>> answer = None;
        await host(value => answer = Some(value)).ConfigureAwait(false);
        return answer.IfNone(() => new Missing(member));
    }

    public static Fin<Seq<T>> Each<TItem, T>(Seq<TItem> items, Func<TItem, int, Fin<T>> element) =>
        items.Map((item, index) => element(item, index).ToValidation())
            .Strict()
            .Traverse(static answer => answer)
            .As()
            .ToFin();

    public static Fin<Unit> Each<TItem>(Seq<TItem> items, Func<TItem, bool> call, string member) =>
        Each(items, (item, index) => RefusedElement.Unless(call(item), member, index)).Map(static _ => unit);

    public static IO<Seq<T>> Each<T>(Seq<IO<T>> effects) =>
        effects.Map<K<IO, T>>(static effect => effect)
            .PartitionFallible()
            .As()
            .Bind(static parts => parts.Fails.IsEmpty ? IO.pure(parts.Succs) : IO.fail<Seq<T>>(Error.Many(parts.Fails)));

    public static IO<Unit> Each(Seq<IO<Unit>> effects) =>
        Each<Unit>(effects).Map(static _ => unit);

    public static Option<T> Found<T>(bool found, T value) =>
        found ? Some(value) : None;

    public static Fin<T> Thrown<TException, T>(Func<T> call, string member) where TException : Exception =>
        Try.lift(call).Run().BindFail(error => error.HasException<TException>() ? new Refused(member) : error);

    // --- [LOOKUPS]
    public static Validation<Error, Seq<TRow>> Unique<TRow, TKey>(Seq<TRow> rows, Func<TRow, TKey> key, string member) where TKey : notnull =>
        Unique(rows, key, EqualityComparer<TKey>.Default, member);

    public static Validation<Error, Seq<TRow>> Unique<TRow, TKey>(Seq<TRow> rows, Func<TRow, TKey> key, IEqualityComparer<TKey> comparer, string member) where TKey : notnull =>
        toSeq(rows.CountBy(key, comparer))
            .Filter(static counted => counted.Value > 1)
            .Traverse(counted => Validation.Fail<Error, Unit>(new Duplicate<TKey>(member, counted.Key, counted.Value)))
            .As()
            .Map(_ => rows);
}
