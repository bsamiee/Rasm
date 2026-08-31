namespace Lab.F20;

internal static class Samples {
    private static readonly Seq<Func<Fin<Unit>>> Probes = [
        AgentProbe,
        AccountProbe,
    ];

    public static Fin<Unit> Run() {
        Seq<Error> failures = Probes.Choose(static probe => probe().Match(Succ: static _ => Option<Error>.None, Fail: Some));
        return failures.IsEmpty ? unit : Error.Many(failures);
    }

    private static Fin<Unit> Check(string probe, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{probe}: {string.Join(" | ", failed)}")).ToFin();
    }

    private static Fin<Unit> AgentProbe() {
        Conduit<Increment, Increment> inbox = Agent.Inbox<Increment>();
        Counter counter = new(inbox);
        IO<(int First, int Second, int Final)> session =
            from running in Agent.Start(inbox, 0, Counting.Process)
            from first in counter.IncrementBy(5)
            from second in counter.IncrementBy(7)
            from _ in inbox.Complete()
            from final in running.Await
            select (first, second, final);
        Conduit<int, int> summing = Agent.Inbox<int>();
        IO<int> pure =
            from running in Agent.Start(summing, 0, static (int total, int item) => total + item)
            from _ in Seq(1, 2, 3).TraverseM(summing.Post).As()
            from __ in summing.Complete()
            from final in running.Await
            select final;
        Conduit<int, int> stopping = Agent.Inbox<int>();
        IO<int> untilZero =
            from running in stopping.Reduce(0, static (total, item) => item == 0 ? Reduced.DoneIO(total) : Reduced.ContinueIO(total + item)).Fork()
            from _ in Seq(1, 2, 0, 9).TraverseM(stopping.Post).As()
            from final in running.Await
            select final;
        return Check(
            nameof(AgentProbe),
            ("Session", session.RunSafe() == Pure((5, 12, 12))),
            ("Pure", pure.RunSafe() == Pure(6)),
            ("UntilZero", untilZero.RunSafe() == Pure(3)));
    }

    private static Fin<Unit> AccountProbe() {
        Atom<Seq<Debited>> journal = Atom(Seq<Debited>());
        AtomHashMap<Guid, AccountProcess> processes = AtomHashMap<Guid, AccountProcess>();
        Guid id = Guid.NewGuid();
        Guid missing = Guid.NewGuid();
        Func<Debited, IO<Unit>> persist = debited => IO.lift(() => ignore(journal.Swap(events => events.Add(debited))));
        Func<Guid, OptionT<IO, AccountState>> load = static _ => OptionT.Some<IO, AccountState>(new AccountState(1000m, 500m));
        Func<Guid, OptionT<IO, AccountState>> absent = static _ => OptionT.None<IO, AccountState>();
        IO<AccountProcess> resolve = Registry.Require(Registry.Lookup(processes, load, persist, id));
        Fin<AccountState> accepted = resolve.Bind(static process => process.Debit(800m)).RunSafe();
        Fin<AccountState> rejected = resolve.Bind(static process => process.Debit(800m)).RunSafe();
        Fin<AccountProcess> unknown = Registry.Require(Registry.Lookup(processes, absent, persist, missing)).RunSafe();
        IO<(bool Same, AccountState Final)> closing =
            from process in resolve
            from other in AccountProcess.Start(persist, new AccountState(0m, 0m))
            from kept in IO.lift(() => processes.FindOrAdd(id, other))
            from _ in other.Inbox.Complete()
            from __ in process.Inbox.Complete()
            from final in process.Running.Await
            select (ReferenceEquals(kept, process), final);
        Fin<(bool Same, AccountState Final)> closed = closing.RunSafe();
        return Check(
            nameof(AccountProbe),
            ("Accepted", accepted == Pure(new AccountState(200m, 500m))),
            ("Rejected", rejected.Match(Succ: static _ => false, Fail: static error => error.HasCode(1001))),
            ("Unknown", unknown.Match(Succ: static _ => false, Fail: static error => error.HasCode(1002))),
            ("Journal", journal.Value == Seq(new Debited(800m))),
            ("Closed", closed == Pure((true, new AccountState(200m, 500m)))),
            ("Registered", processes.Count == 1));
    }
}
