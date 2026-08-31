namespace Lab.F19;

internal static class Queries {
    private static readonly HashMap<string, Seq<decimal>> History = HashMap(("EURUSD", Seq(1.1m, 1.2m)), ("GBPUSD", Seq(1.3m)));

    public static Source<decimal> Quotes(string pair) => Source.lift(History.Find(pair).IfNone(Seq<decimal>()));

    public static Source<decimal> Rates(Source<string> currencyPairs) =>
        from pair in currencyPairs
        from rate in Quotes(pair)
        select rate;
}

internal static class Branches {
    public static (Source<A> Passed, Source<A> Failed) Partition<A>(this Source<A> source, Func<A, bool> predicate) =>
        (source.Filter(predicate), source.Filter(value => !predicate(value)));

    public static IO<int> Rejoined(Source<int> source) {
        (Source<int> passed, Source<int> failed) = source.Partition(static item => item > 1);
        return Source.merge(passed.Map(static item => item * 10), failed).Reduce(0, static (sum, item) => sum + item);
    }
}
