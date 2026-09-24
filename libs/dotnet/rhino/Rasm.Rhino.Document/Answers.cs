using System.Reflection;
using Rhino.Commands;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Answers {
    // --- [VALUES]
    [UserMapping]
    public static Option<Guid> Present(Guid id) =>
        Some(id).Filter(static value => value != Guid.Empty);

    public static Option<int> Present(int index) =>
        Some(index).Filter(static value => value >= 0);

    public static Option<uint> Present(uint serial) =>
        Some(serial).Filter(static value => value != 0u);

    [UserMapping]
    public static Option<string> Present(string? text) =>
        Optional(text).Filter(static value => value.Length > 0);

    [UserMapping]
    public static Seq<T> Present<T>(IEnumerable<T?>? rows) where T : class =>
        toSeq(rows).Choose(static row => Optional(row)).Strict();

    public static Option<T> Found<T>(bool found, T value) =>
        found ? Some(value) : Option<T>.None;

    public static Fin<Guid> NonEmpty(Guid id, string member) =>
        Present(id).ToFin(new EmptyGuid(member));

    public static Fin<Seq<TRow>> NonEmpty<TRow>(Seq<TRow> rows, string member) =>
        rows.IsEmpty ? new Missing(member) : rows;

    public static Fin<int> NonNegative(int index, string member) =>
        Present(index).ToFin(new NegativeIndex(member));

    public static Fin<Unit> InRange(Seq<int> indices, int itemCount, string member) =>
        indices.TraverseM(index => IndexOutOfRange.Unless(index, itemCount, member)).As().Map(static _ => unit);

    public static Validation<Error, Seq<Unit>> Unique<TKey>(Seq<TKey> keys, Func<TKey, int, Error> duplicate) where TKey : notnull =>
        toSeq(keys.CountBy(static key => key))
            .Filter(static counted => counted.Value > 1)
            .Traverse(counted => Validation.Fail<Error, Unit>(duplicate(counted.Key, counted.Value)))
            .As();

    // --- [RESULTS]
    public static Fin<Unit> FromResult(Result result, string member) =>
        result switch {
            Result.Success => unit,
            Result.Cancel or Result.CancelModelessDialog => new Canceled(),
            Result.Nothing => new NothingEntered(),
            Result.ExitRhino => new ExitRequested(),
            Result.UnknownCommand => new UnknownCommand(member),
            _ => new UnexpectedResult(member, result),
        };

    public static Result ToResult(Error error) =>
        error.IsType<ExitRequested>() ? Result.ExitRhino
            : error.IsType<Canceled>() ? Result.Cancel
            : error.IsType<NothingEntered>() ? Result.Nothing
            : error.IsType<UnknownCommand>() ? Result.UnknownCommand
            : Result.Failure;

    public static Result ToResult(Fin<Unit> answer, Action<Error> reject) =>
        answer.Match(
            Succ: static _ => Result.Success,
            Fail: error => {
                Result known = ToResult(error);
                if (known == Result.Failure)
                    reject(error);
                return known;
            });

    // --- [CALLBACKS]
    public static TValue Answer<TValue>(IO<TValue> effect, Action<Error> reject, TValue fallback) =>
        Answer(Some(effect), reject, () => fallback);

    public static TValue Answer<TValue>(Option<IO<TValue>> effect, Action<Error> reject, Func<TValue> fallback) =>
        effect.Match(
            Some: run => run.RunSafe().IfFail(error => {
                reject(error);
                return fallback();
            }),
            None: fallback);

    public static TValue Answer<TValue>(Option<IO<TValue>> effect, Action<Error> reject, TValue refused, Func<TValue> absent) =>
        effect.Match(Some: run => Answer(run, reject, refused), None: absent);

    public static bool Succeeded(IO<Unit> effect, Action<Error> reject) =>
        Answer(effect.Map(static _ => true), reject, fallback: false);

    public static bool Succeeded(Option<IO<Unit>> effect, Action<Error> reject, Func<bool> absent) =>
        effect.Match(Some: run => Succeeded(run, reject), None: absent);

    public static EventHandler<TArgs> Handler<TArgs>(Func<TArgs, IO<Unit>> deliver, Action<Error> reject) =>
        (_, args) => _ = Answer(deliver(args), reject, unit);

    public static Fin<T> Captured<T>(Action<Action<Fin<T>>> host, string member) {
        Option<Fin<T>> answer = None;
        host(value => answer = Some(value));
        return answer.IfNone(new CallbackSkipped(member));
    }

    public static async Task<Fin<T>> CapturedAsync<T>(Func<Action<Fin<T>>, Task> host, string member) {
        Option<Fin<T>> answer = None;
        await host(value => answer = Some(value)).ConfigureAwait(false);
        return answer.IfNone(new CallbackSkipped(member));
    }

    // --- [REGISTRY]
    public static IO<Seq<T>> Registered<T>(Func<Assembly, Guid, T[]?> register, Assembly assembly, Guid plugInId, string member) =>
        from id in IO.lift(() => NonEmpty(plugInId, member))
        from registered in IO.lift(() => Missing.Unless(register(assembly, id), member).Map(static types => toSeq(types)))
            .Catch(static error => error.HasException<InvalidDataException>(), _ => IO.fail<Seq<T>>(new ExportMissingGuid(member)))
        select registered;

    // --- [PATHS]
    public static IO<string> QualifiedPath(string path) =>
        IO.lift(() => Invalid.Unless(Path.IsPathFullyQualified(path), path, nameof(Path.IsPathFullyQualified)));

    public static IO<string> ExistingPath(string path) =>
        from qualified in QualifiedPath(path)
        from found in IO.lift(() => Missing.Unless(File.Exists(qualified), nameof(File.Exists)))
        select qualified;
}
