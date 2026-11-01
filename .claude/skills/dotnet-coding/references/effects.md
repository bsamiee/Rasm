# [EFFECTS]

Covers keeping effects at the boundary, from a pure core between injected reads and writes to lifting host calls and scoping what they acquire.

## [01]-[ISOLATION]

A function that both computes and writes splits into a pure function returning every value it computes and an `IO` query that reads, calls it once, and writes. An injected `IO<A>` runs again at every bind, so a value later steps read binds once and passes on as a value:

```csharp
internal sealed record Line(Guid Product, decimal Price, int Quantity);

internal static class Orders {
    public static (decimal Total, Seq<Line> Kept) Priced(Seq<Line> lines) =>
        (lines.Sum(static line => line.Price * line.Quantity), lines.Filter(static line => line.Quantity > 0));

    public static IO<decimal> Checkout(Func<Guid, IO<Seq<Line>>> read, Func<Guid, Seq<Line>, IO<Unit>> write, Guid order) =>
        from lines in read(order)
        let priced = Priced(lines)
        from _ in write(order, priced.Kept)
        select priced.Total;
}
```

## [02]-[LIFTING]

`Task<A>` starts its work when the call returns it, `IO<A>` holds the thunk and runs it again on each run, so retry, fallback, and timeout wrap the `IO` and a started task never enters one. A non-generic `Task` enters through `liftIO` and a `ValueTask` through `IO.liftVAsync`, a nullable answer leaves through `Optional` as absence, and a synchronous throwing call ends at the boundary as `Try.lift(...).Run()`:

```csharp
internal sealed record Quote(string Provider, decimal Price);

[JsonSerializable(typeof(Quote))]
internal sealed partial class Wire : JsonSerializerContext;

internal static class Quotes {
    public static IO<Option<Quote>> Fetch(HttpClient http, Uri source) =>
        IO.liftAsync(env => http.GetFromJsonAsync(source, Wire.Default.Quote, env.Token)).Map(Optional);
    public static IO<Unit> Write(Stream target, Quote quote) =>
        liftIO(env => JsonSerializer.SerializeAsync(target, quote, Wire.Default.Quote, env.Token));
    public static Fin<Option<Quote>> Decode(string json) =>
        Try.lift(() => Optional(JsonSerializer.Deserialize(json, Wire.Default.Quote))).Run();
}
```

## [03]-[SCOPES]

`Bracket(Use:, Fin:)` runs its `Fin` after the work on both paths, for an after-step that releases nothing. A member returning an acquired value leaves `.Bracket()` to the scope that consumes it, and a member owning the whole scope brackets it. The acquired value keys its release and equal values keep one release, so an attach yields its handler and never `unit`. One `.Bracket()` releases its resources in no fixed order, so a resource whose release needs another (a transaction its connection) closes at its own inner `.Bracket()`:

```csharp
internal static class Scopes {
    public static IO<A> Timed<A>(TimeProvider time, Histogram<double> latency, IO<A> work) =>
        IO.lift(time.GetTimestamp).Bracket(
            Use: _ => work,
            Fin: start => IO.lift(() => latency.Record(time.GetElapsedTime(start).TotalSeconds)));

    public static IO<A> Transacted<A>(DbDataSource source, Func<DbTransaction, IO<A>> work) =>
        use(IO.liftVAsync(env => source.OpenConnectionAsync(env.Token)))
            .Bind(connection => (from transaction in use(IO.liftVAsync(env => connection.BeginTransactionAsync(env.Token)))
                                 from value in work(transaction)
                                 from _ in liftIO(env => transaction.CommitAsync(env.Token))
                                 select value).Bracket())
            .Bracket();

    public static IO<Unit> Subscribed<THandler>(Action<THandler> add, Action<THandler> remove, THandler handler) where THandler : Delegate =>
        use(() => {
            add(handler);
            return handler;
        }, remove).Map(unit);
}
```

Nesting order decides what each scope surrounds: `Timed(time, latency, Transacted(source, work))` measures acquisition, statements, and commit, `Transacted(source, transaction => Timed(time, latency, work(transaction)))` measures the statements alone, and a failure before the commit leaves the transaction to roll back at its release.
