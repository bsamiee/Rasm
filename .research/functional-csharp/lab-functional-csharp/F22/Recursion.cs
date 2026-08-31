namespace Lab.F22;

internal static class Recursion {
    public static IO<int> CountTo(int current, int limit) =>
        current >= limit ? IO.pure(current) : IO.lift(() => current + 1).Bind(next => tail(CountTo(next, limit)));

    public static IO<int> Recur(int limit) =>
        Monad.recur<IO, int, int>(0, i => i >= limit ? IO.pure(Next.Done<int, int>(i)) : IO.pure(Next.Loop<int, int>(i + 1))).As();

    public static IO<int> Poll(Atom<int> polls) =>
        IO.lift(() => polls.Swap(static n => n + 1)).RepeatUntil(static n => n >= 3);

    public static IO<int> Drain(Atom<int> pending) =>
        IO.lift(() => pending.Swap(static n => n - 1)).RepeatWhile(static n => n > 0);
}
