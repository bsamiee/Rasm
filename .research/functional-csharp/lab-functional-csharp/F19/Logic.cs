namespace Lab.F19;

internal static class Transitions {
    public static Source<(A Previous, A Current)> PairWithPrevious<A>(this Source<A> source) =>
        source.Zip(source.Skip(1)).Map(static pair => (Previous: pair.First, Current: pair.Second));
}

internal static class Backpressure {
    public static Source<decimal> BalanceInUsd(Source<decimal> euroBalance, Source<decimal> eurUsdRate) =>
        euroBalance.Zip(eurUsdRate).Map(static pair => pair.First * pair.Second);

    public static IO<Seq<decimal>> Retained(Buffer<decimal> buffer, Seq<decimal> rates) {
        Conduit<decimal, decimal> quotes = Conduit.make(buffer);
        return
            from _ in rates.TraverseM(quotes.Post).As()
            from __ in quotes.Complete()
            from kept in quotes.Reduce(Seq<decimal>(), static (kept, rate) => Reduced.ContinueIO(kept.Add(rate)))
            select kept;
    }

    public static IO<Seq<decimal>> Drained(Buffer<decimal> buffer, Seq<decimal> rates) {
        Conduit<decimal, decimal> quotes = Conduit.make(buffer);
        return
            from running in quotes.Reduce(Seq<decimal>(), static (kept, rate) => Reduced.ContinueIO(kept.Add(rate))).Fork()
            from _ in rates.TraverseM(quotes.Post).As()
            from __ in quotes.Complete()
            from kept in running.Await
            select kept;
    }

    public static IO<Unit> PostLength(Sink<int> sink, string text) => sink.Comap(static (string s) => s.Length).Post(text);
}

internal sealed record Transaction(Guid AccountId, decimal Amount);

internal static class Ledger {
    public static IO<Seq<Guid>> Overdrawn(Source<Transaction> transactions) =>
        transactions
            .Reduce(HashMap<Guid, Seq<decimal>>(), static (ledger, transaction) =>
                ledger.AddOrUpdate(transaction.AccountId, amounts => amounts.Add(transaction.Amount), Seq(transaction.Amount)))
            .Map(static ledger => toSeq(ledger.Filter(Crossed).Keys));

    private static bool Crossed(Seq<decimal> amounts) {
        Seq<decimal> balances = amounts.Scan(0m, static (balance, amount) => balance + amount);
        return balances.Zip(balances.Tail).Exists(static step => step.First >= 0m && step.Second < 0m);
    }
}
