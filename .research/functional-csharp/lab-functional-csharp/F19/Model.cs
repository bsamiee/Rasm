namespace Lab.F19;

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

internal static class Model {
    public static Source<int> Observed => Source.lift(new Replay<int>(Seq(1, 2, 3)));

    public static IO<int> Total(Source<int> source) => source.Reduce(0, static (sum, item) => sum + item);
}
