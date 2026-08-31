namespace Lab.F23;

internal static class StackSafety {
    public static Trampoline<long> SumTo(int current, int limit, long total) =>
        current > limit ? Trampoline.Pure(total) : Trampoline.More(() => SumTo(current + 1, limit, total + current));

    public static IO<int> Drain(Atom<int> pending) =>
        Monad.recur<IO, int, int>(0, drained => IO.lift(() => pending.Swap(static n => n - 1)).Map(left => left > 0 ? Next.Loop<int, int>(drained + 1) : Next.Done<int, int>(drained + 1))).As();

    public static IO<int> CountTo(int current, int limit) =>
        current >= limit ? IO.pure(current) : IO.lift(() => current + 1).Bind(next => tail(CountTo(next, limit)));

    public static Fin<int> Exit(IO<int> counted) => Try.lift(counted.Run).Run();
}
