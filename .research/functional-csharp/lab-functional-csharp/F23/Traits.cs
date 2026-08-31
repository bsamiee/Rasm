namespace Lab.F23;

internal sealed record Rejected() : Expected("value rejected", 2301);

internal sealed record Settings(int Factor);

internal static class Traits {
    public static K<F, int> Doubled<F>(K<F, int> values) where F : Functor<F> => values.Map(static v => v * 2);

    public static K<F, int> Lifted<F>(int value) where F : Applicative<F> => F.Pure(value);
    public static K<F, int> Incremented<F>(K<F, int> value) where F : Applicative<F> => F.Apply(F.Pure<Func<int, int>>(static v => v + 1), value);
    public static K<F, int> Summed<F>(K<F, int> left, K<F, int> right) where F : Applicative<F> => (left, right).Apply(static (a, b) => a + b);

    public static K<M, int> Halved<M>(K<M, int> value, Func<int, K<M, int>> halve) where M : Monad<M> => value.Bind(halve);
    public static K<M, int> Total<M>(K<M, int> first, K<M, int> second) where M : Monad<M> =>
        from a in first
        from b in second
        select a + b;

    public static int Sum<T>(K<T, int> values) where T : Foldable<T> => values.Fold(0, static (total, v) => total + v);
    public static Seq<int> Reversed<T>(K<T, int> values) where T : Foldable<T> => values.FoldBack(Seq<int>(), static (acc, v) => acc.Add(v));
    public static bool AnyNegative<T>(K<T, int> values) where T : Foldable<T> => values.Exists(static v => v < 0);
    public static bool AllPositive<T>(K<T, int> values) where T : Foldable<T> => values.ForAll(static v => v > 0);
    public static Option<int> Second<T>(K<T, int> values) where T : Foldable<T> => values.At(1);

    public static K<Option, K<T, int>> ParseAll<T>(K<T, string> values) where T : Traversable<T> => T.Traverse(static s => parseInt(s), values);
    public static K<IO, K<T, int>> ReadAll<T>(K<T, string> values, Func<string, IO<int>> read) where T : Traversable<T> => T.TraverseM(read, values);

    public static K<F, int> Reject<F>() where F : Fallible<F> => F.Fail<int>(new Rejected());
    public static K<F, int> Recovered<F>(K<F, int> value) where F : Fallible<F>, Applicative<F> =>
        value.Catch(static error => error.HasCode(2301), static _ => F.Pure(0));

    public static K<M, int> Factor<M>() where M : Readable<M, Settings>, Monad<M> => Readable.ask<M, Settings>().Map(static s => s.Factor);
    public static K<M, int> Scaled<M>(int value) where M : Readable<M, Settings> => Readable.asks<M, Settings, int>(s => s.Factor * value);
    public static K<M, int> ScaledTwice<M>(K<M, int> operation) where M : Readable<M, Settings> =>
        Readable.local<M, Settings, int>(static s => s with { Factor = s.Factor * 2 }, operation);

    public static K<M, int> Tick<M>() where M : Stateful<M, int>, Monad<M> =>
        from current in Stateful.get<M, int>()
        from _ in Stateful.put<M, int>(current + 1)
        select current;
    public static K<M, Unit> Reset<M>() where M : Stateful<M, int> => Stateful.modify<M, int>(static _ => 0);
    public static K<M, int> Counted<M>() where M : Stateful<M, int>, Monad<M> => Stateful.state<M, int, int>(static n => (n, n + 1));
    public static K<M, int> Isolated<M>(K<M, int> operation) where M : Stateful<M, int>, Monad<M> => Stateful.local<M, int, int>(static n => n + 100, operation);

    public static K<M, Unit> Note<M>(string message) where M : Writable<M, Seq<string>> => Writable.tell<M, Seq<string>>(Seq(message));

    public static K<F, int> Nothing<F>() where F : Alternative<F> => F.Empty<int>();
    public static K<F, int> FirstOf<F>(K<F, int> first, K<F, int> second) where F : Alternative<F> => F.Choose(first, second);
    public static Option<int> Chosen(Option<int> first, Option<int> second) => first | second;
}
