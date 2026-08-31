namespace Lab.F00;

internal static class Loops {
    public static Fin<Unit> Probe() {
        const int limit = 100_000;
        int trampolined = CountTo(0, limit).Run();
        Fin<int> recurred = Monad.recur<IO, int, int>(0, static i => i >= limit ? IO.pure(Next.Done<int, int>(i)) : IO.pure(Next.Loop<int, int>(i + 1))).As().RunSafe();
        Fin<int> tailed = Try.lift(DeepCount(0, limit).Run).Run();
        Seq<int> unfolded = toSeq(LanguageExt.List.unfold(1, Step));
        return Verify.Check(
            nameof(Loops),
            ("trampolined == limit", trampolined == limit),
            ("recurred == Pure(limit)", recurred == Pure(limit)),
            ("tailed == Pure(limit)", tailed == Pure(limit)),
            ("unfolded == Seq(1, 2, 3)", unfolded == Seq(1, 2, 3)));
    }

    private static Trampoline<int> CountTo(int current, int limit) =>
        current >= limit ? Trampoline.Pure(current) : Trampoline.More(() => CountTo(current + 1, limit));

    private static IO<int> DeepCount(int current, int limit) =>
        current >= limit ? IO.pure(current) : IO.lift(() => current + 1).Bind(next => tail(DeepCount(next, limit)));

    private static Option<(int, int)> Step(int state) => state > 3 ? None : Some((state, state + 1));
}
