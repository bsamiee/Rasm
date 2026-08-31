namespace Lab.F16;

internal sealed record RateUnavailable() : Expected("rate unavailable", 1600);

internal static class RateCache {
    public static State<HashMap<string, decimal>, decimal> GetRate(Func<string, decimal> fetch, string currencyPair) =>
        from cached in State.gets<HashMap<string, decimal>, Option<decimal>>(cache => cache.Find(currencyPair))
        from rate in cached.Match(
            Some: static hit => State.pure<HashMap<string, decimal>, decimal>(hit),
            None: () => Store(fetch(currencyPair), currencyPair))
        select rate;

    private static State<HashMap<string, decimal>, decimal> Store(decimal rate, string currencyPair) =>
        State.modify<HashMap<string, decimal>>(cache => cache.Add(currencyPair, rate)).Map(_ => rate);
}

internal static class EffectfulRateCache {
    public static StateT<HashMap<string, decimal>, IO, decimal> GetRate(Func<string, IO<decimal>> fetch, string currencyPair) =>
        from cache in StateT.get<IO, HashMap<string, decimal>>()
        from rate in cache.Find(currencyPair).Match(
            Some: static hit => StateT.pure<HashMap<string, decimal>, IO, decimal>(hit),
            None: () => Fetch(fetch, currencyPair, cache))
        select rate;

    private static StateT<HashMap<string, decimal>, IO, decimal> Fetch(Func<string, IO<decimal>> fetch, string currencyPair, HashMap<string, decimal> cache) =>
        from rate in StateT.liftIO<HashMap<string, decimal>, IO, decimal>(fetch(currencyPair))
        from _ in StateT.put<IO, HashMap<string, decimal>>(cache.Add(currencyPair, rate))
        select rate;
}
