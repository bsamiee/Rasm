namespace Lab.F17;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        RecursionProbe,
        LoopProbe,
        StateProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> RecursionProbe() {
        const int limit = 100_000;
        return Check(
            nameof(RecursionProbe),
            ("Direct", Direct.Run(0, static x => x + 1, static x => x >= 10) == 10),
            ("Found", Positions.FirstPositionAtZero(Seq(2, -12, 9), 10, 0).Run() == Some(1)),
            ("Exhausted", Positions.FirstPositionAtZero(Seq(1, 1), 10, 0).Run().IsNone),
            ("DeepPosition", Positions.FirstPositionAtZero(toSeq(Range(0, limit)).Map(static _ => -1), limit, 0).Run() == Some(limit - 1)),
            ("RunUntil", Trampolined.RunUntil(0, static x => x >= limit, static x => x + 1).Run() == limit),
            ("Bound", Trampolined.RunUntil(0, static x => x >= limit, static x => x + 1).Bind(static x => Trampoline.Pure(x * 2)).Run() == limit * 2));
    }

    private static Fin<Unit> LoopProbe() {
        const int limit = 100_000;
        Atom<int> pending = Atom(3);
        Atom<int> moves = Atom(0);
        IO<int> readMove = IO.lift(() => moves.Swap(static n => n + 1)).Map(static _ => 1);
        Fin<Session> played = Sessions.Play(new Session(limit, HasExited: false), readMove).RunSafe();
        Fin<Session> unchanged = Sessions.Play(new Session(5, HasExited: true), readMove).RunSafe();
        Fin<int> counted = Try.lift(Deep.CountDown(IO.lift(static () => 1), limit).Run).Run();
        Fin<int> drained = Polling.Drain(IO.lift(() => pending.Swap(static n => n - 1))).RunSafe();
        return Check(
            nameof(LoopProbe),
            ("Played", played == Pure(new Session(0, HasExited: true))),
            ("Moves", moves.Value == limit),
            ("Unchanged", unchanged == Pure(new Session(5, HasExited: true))),
            ("Counted", counted == Pure(0)),
            ("Drained", drained == Pure(0) && pending.Value == 0));
    }

    private static Fin<Unit> StateProbe() {
        Atom<int> transitions = Atom(0);
        Session initial = new(3, HasExited: false);
        Seq<Session> states = States.Trace(initial, state => Advance(state, transitions));
        Option<Session> final = Consumption.Final(states);
        Seq<string> messages = Consumption.Messages(states);
        (Seq<string> Messages, Session State) report = Consumption.Report(states, initial);
        int afterOne = transitions.Value;
        Seq<Session> again = States.Trace(initial, state => Advance(state, transitions));
        Option<Session> head = again.Head;
        return Check(
            nameof(StateProbe),
            ("Remaining", states.Map(static state => state.Remaining) == Seq(2, 1, 0)),
            ("Final", final == Some(new Session(0, HasExited: true))),
            ("Messages", messages == Seq("remaining 2", "remaining 1", "remaining 0")),
            ("Report", report.Messages.Count == 3 && report.State == new Session(0, HasExited: true)),
            ("OnePass", afterOne == 3),
            ("Head", head == Some(new Session(2, HasExited: false))),
            ("Rerun", transitions.Value == 4),
            ("Empty", States.Trace(new Session(0, HasExited: true), state => Advance(state, transitions)).IsEmpty));
    }

    private static Session Advance(Session state, Atom<int> transitions) {
        _ = transitions.Swap(static n => n + 1);
        return new Session(state.Remaining - 1, state.Remaining <= 1);
    }
}
