namespace Lab.F22;

internal sealed class Replay<A>(Seq<A> items) : IObservable<A> {
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
    public static Source<int> Observed => Source.lift(new Replay<int>(Seq(1, 2, 3)));

    public static Source<int> Merged => Source.merge(Source.lift(Seq(1, 2)), Source.lift(Seq(3)));

    public static Source<(int First, string Second)> Zipped => Source.lift(Seq(1, 2)).Zip(Source.lift(Seq("a", "b")));

    public static IO<int> Sum(Source<int> source) => source.Reduce(0, static (total, item) => total + item);

    public static IO<int> UntilTwo(Source<int> source) =>
        source.ReduceIO(0, static (total, item) => item == 2 ? Reduced.DoneIO(total + item) : Reduced.ContinueIO(total + item));

    public static IO<Unit> PostLength(Sink<int> sink, string text) => sink.Comap(static (string s) => s.Length).Post(text);

    public static IO<Unit> Closed(Sink<int> sink) =>
        from _ in sink.Complete()
        from __ in sink.Post(1)
        select unit;

    public static IO<int> Retained(Buffer<int> buffer, Seq<int> items) {
        Conduit<int, int> queue = Conduit.make(buffer);
        return
            from _ in items.TraverseM(queue.Post).As()
            from __ in queue.Complete()
            from total in queue.Reduce(0, static (sum, item) => Reduced.ContinueIO(sum + item))
            select total;
    }

    public static IO<(int Reply, int Final)> Session(Conduit<int, int> inbox, Conduit<int, int> replies) =>
        from running in inbox.Reduce(0, (state, message) => replies.Post(state + message).Map(_ => Reduced.Continue(state + message))).Fork()
        from _ in inbox.Post(5)
        from reply in replies.Source.Take(1).Last()
        from __ in inbox.Complete()
        from final in running.Await
        select (reply, final);

    public static ProducerT<int, IO, Unit> Numbers => ProducerT.yieldAll<IO, int>(Seq(1, 2, 3));

    public static PipeT<int, int, IO, Unit> Doubled => PipeT.map<IO, int, int>(static x => x * 2);

    public static ConsumerT<int, IO, Unit> Accumulate(Atom<int> total) =>
        ConsumerT.repeat(ConsumerT.awaiting<IO, int>().Bind(x => IO.lift(() => ignore(total.Swap(n => n + x)))));

    public static IO<int> Pipeline(Atom<int> total) =>
        (Numbers | Doubled | Accumulate(total)).Run().As().Map(_ => total.Value);
}
