namespace Lab.F17;

internal sealed record Session(int Remaining, bool HasExited);

internal static class Sessions {
    public static IO<Session> Play(Session initial, IO<int> readMove) =>
        Monad.recur<IO, Session, Session>(initial, state =>
            state.HasExited
                ? IO.pure(Next.Done<Session, Session>(state))
                : readMove.Map(move => Next.Loop<Session, Session>(Apply(state, move)))).As();

    private static Session Apply(Session state, int move) {
        int remaining = state.Remaining - move;
        return new Session(remaining, remaining <= 0);
    }
}

internal static class Deep {
    public static IO<int> CountDown(IO<int> step, int remaining) =>
        remaining <= 0
            ? IO.pure(remaining)
            : step.Bind(move => tail(CountDown(step, remaining - move)));
}

internal static class Polling {
    public static IO<int> Drain(IO<int> step) =>
        step.RepeatUntil(Schedule.spaced(TimeSpan.FromMilliseconds(1)), static remaining => remaining <= 0);
}
