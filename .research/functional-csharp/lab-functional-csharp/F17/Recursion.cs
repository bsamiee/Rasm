namespace Lab.F17;

internal static class Direct {
    public static S Run<S>(S state, Func<S, S> advance, Func<S, bool> isFinished) =>
        isFinished(state)
            ? state
            : Run(advance(state), advance, isFinished);
}

internal static class Positions {
    public static Trampoline<Option<int>> FirstPositionAtZero(Seq<int> deltas, int currentValue, int nextIndex) =>
        currentValue == 0
            ? Trampoline.Pure(Some(nextIndex - 1))
            : deltas.At(nextIndex).Match(
                Some: delta => Trampoline.More(() => FirstPositionAtZero(deltas, currentValue + delta, nextIndex + 1)),
                None: static () => Trampoline.Pure(Option<int>.None));
}

internal static class Trampolined {
    public static Trampoline<S> RunUntil<S>(S state, Func<S, bool> stop, Func<S, S> next) =>
        stop(state)
            ? Trampoline.Pure(state)
            : Trampoline.More(() => RunUntil(next(state), stop, next));
}
