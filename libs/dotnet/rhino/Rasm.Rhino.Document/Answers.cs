using System.Drawing;
using System.Reflection;
using Rhino.Commands;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Rasm.Rhino.Document.Answers))]

namespace Rasm.Rhino.Document;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Answers {
    // --- [VALUES]
    [UserMapping]
    public static Option<Guid> Present(Guid id) =>
        Some(id).Filter(static value => value != Guid.Empty);

    [UserMapping]
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

    public static Guid Unset(Option<Guid> id) =>
        id.IfNone(Guid.Empty);

    public static int Unset(Option<int> index) =>
        index.IfNone(-1);

    [UserMapping]
    public static string Unset(Option<string> text) =>
        text.IfNone("");

    public static Fin<Guid> Required(Guid id, string member) =>
        Present(id).ToFin(new Refused(member));

    public static Fin<int> Required(int index, string member) =>
        Present(index).ToFin(new Refused(member));

    public static Fin<uint> Required(uint serial, string member) =>
        Present(serial).ToFin(new Refused(member));

    public static bool Same(Color held, Color wanted) =>
        held.ToArgb() == wanted.ToArgb();

    public static Option<T> Found<T>(bool found, T value) =>
        found ? Some(value) : Option<T>.None;

    public static Fin<Seq<TRow>> NonEmpty<TRow>(Seq<TRow> rows, string member) =>
        rows.IsEmpty ? new Missing(member) : rows;

    public static Fin<T> Validated<T, TValue>(TValue value) where T : IObjectFactory<T, TValue, ValidationFailure> where TValue : notnull =>
        T.Validate(value, provider: null, out T? item) is { } error ? error : item!;

    // --- [CHECKS]
    public static ValidationFailure? FirstInvalid(params (bool Failed, string Member)[] checks) =>
        toSeq(checks).Find(static check => check.Failed).Map<ValidationFailure>(static check => new Invalid(check.Member)).ValueUnsafe();

    public static Validation<Error, Seq<Unit>> Unique<TKey>(Seq<TKey> keys, string member) where TKey : notnull =>
        Unique(keys, EqualityComparer<TKey>.Default, member);

    public static Validation<Error, Seq<Unit>> Unique<TKey>(Seq<TKey> keys, IEqualityComparer<TKey> comparer, string member) where TKey : notnull =>
        toSeq(keys.CountBy(static key => key, comparer))
            .Filter(static counted => counted.Value > 1)
            .Traverse(counted => Validation.Fail<Error, Unit>(new Duplicate<TKey>(member, counted.Key, counted.Value)))
            .As();

    public static Fin<Unit> Each<T>(Seq<T> items, Func<T, bool> call, string member) =>
        items.Map((item, index) => RefusedElement.Unless(call(item), member, index)).Traverse(identity).As().Map(static _ => unit);

    // --- [RESULTS]
    public static Fin<Unit> FromResult(Result result, string member) =>
        result switch {
            Result.Success => unit,
            Result.Cancel or Result.CancelModelessDialog => new Canceled(),
            Result.Nothing => new NothingEntered(),
            Result.ExitRhino => new ExitRequested(),
            Result.UnknownCommand => new UnknownCommand(member),
            Result.Failure => new Refused(member),
        };

    public static Result ToResult(Error error) =>
        error.IsType<ExitRequested>() ? Result.ExitRhino
            : error.IsType<Canceled>() ? Result.Cancel
            : error.IsType<NothingEntered>() ? Result.Nothing
            : error.IsType<UnknownCommand>() ? Result.UnknownCommand
            : Result.Failure;

    public static Result ToResult(Fin<Unit> answer) =>
        answer.Match(Succ: static _ => Result.Success, Fail: ToResult);

    public static Result ToResult(Fin<Unit> answer, Action<Error> reject) {
        Result known = ToResult(answer);
        if (known == Result.Failure)
            _ = answer.IfFail(reject);
        return known;
    }

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

    // --- [LOOKUPS]
    public static IO<Seq<T>> Registered<T>(Func<Assembly, Guid, T[]?> register, Assembly assembly, Guid plugInId, string member) =>
        from id in IO.lift(() => Invalid.Unless(plugInId != Guid.Empty, plugInId, nameof(plugInId)))
        from registered in IO.lift(() => Missing.Unless(register(assembly, id), member).Map(static types => toSeq(types)))
            .Catch(static error => error.HasException<InvalidDataException>(), _ => IO.fail<Seq<T>>(new ExportMissingGuid(member)))
        select registered;

    public static IO<string> QualifiedPath(string path) =>
        IO.lift(() => Invalid.Unless(Path.IsPathFullyQualified(path), path, nameof(Path.IsPathFullyQualified)));

    public static IO<string> ExistingPath(string path) =>
        from qualified in QualifiedPath(path)
        from found in IO.lift(() => Missing.Unless(File.Exists(qualified), nameof(File.Exists)))
        select qualified;
}
