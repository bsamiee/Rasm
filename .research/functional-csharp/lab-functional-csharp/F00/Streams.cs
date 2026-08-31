namespace Lab.F00;

internal sealed class ReplayObservable<A>(Seq<A> items) : IObservable<A> {
    public IDisposable Subscribe(IObserver<A> observer) {
        _ = items.Iter(observer.OnNext);
        observer.OnCompleted();
        return new Subscription();
    }

    private sealed class Subscription : IDisposable {
        public void Dispose() {
        }
    }
}

internal static class Streams {
    private static Action<int> onTick = static _ => { };

    public static Fin<Unit> Probe() =>
        SourceProbe()
            .Bind(static _ => InboxProbe());

    private static Fin<Unit> SourceProbe() {
        Source<int> observed = Source.lift(new ReplayObservable<int>(Seq(1, 2, 3)));
        Fin<int> summed = observed.Reduce(0, static (s, a) => s + a).RunSafe();
        Source<int> merged = Source.merge(Source.lift(Seq(1, 2)), Source.lift(Seq(3)));
        Fin<int> mergedSum = merged.Reduce(0, static (s, a) => s + a).RunSafe();
        Source<(int First, string Second)> zipped = Source.lift(Seq(1, 2)).Zip(Source.lift(Seq("a", "b")));
        Fin<Seq<(int First, string Second)>> zippedAll = zipped.Reduce(Seq<(int First, string Second)>(), static (s, a) => s.Add(a)).RunSafe();
        Event<int> ticks = Event.from(ref onTick);
        Fin<int> withReduced = observed.ReduceIO(0, static (s, a) => a == 2 ? Reduced.DoneIO(s + a) : Reduced.ContinueIO(s + a)).RunSafe();
        return Verify.Check(
            nameof(SourceProbe),
            ("summed == Pure(6)", summed == Pure(6)),
            ("mergedSum == Pure(6)", mergedSum == Pure(6)),
            ("zippedAll == Pure(Seq((1, \"a\"), (2, \"b\")))", zippedAll == Pure(Seq((1, "a"), (2, "b")))),
                        ("withReduced == Pure(3)", withReduced == Pure(3)),
                        ("ticks.Subscribe().RunSafe().IsSucc", ticks.Subscribe().RunSafe().IsSucc));
    }

    private static Fin<Unit> InboxProbe() {
        Conduit<int, int> inbox = Conduit.make(Buffer<int>.Unbounded);
        Conduit<int, int> replies = Conduit.make(Buffer<int>.Unbounded);
        IO<int> agent = inbox.Reduce(0, (state, message) =>
            from _ in replies.Post(state + message)
            select Reduced.Continue(state + message));
        IO<(int Reply, int Final)> session =
            from running in agent.Fork()
            from _ in inbox.Post(5)
            from reply in replies.Source.Take(1).Last()
            from __ in inbox.Complete()
            from final in running.Await
            select (reply, final);
        Fin<(int Reply, int Final)> outcome = session.RunSafe();
        Conduit<int, int> fresh = Conduit.make(Buffer<int>.Unbounded);
        Sink<string> lengths = fresh.Sink.Comap(static (string s) => s.Length);
        return Verify.Check(
            nameof(InboxProbe),
            ("outcome == Pure((5, 5))", outcome == Pure((5, 5))),
            ("lengths.Post(\"abc\").RunSafe().IsSucc", lengths.Post("abc").RunSafe().IsSucc));
    }
}
