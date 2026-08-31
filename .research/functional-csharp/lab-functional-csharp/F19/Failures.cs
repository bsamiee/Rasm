namespace Lab.F19;

internal sealed record UnknownPair() : Expected("unknown currency pair", 404);

internal static class Failures {
    private static readonly HashMap<string, decimal> Table = HashMap(("EURUSD", 1.1m), ("GBPUSD", 1.3m));

    public static Fin<decimal> Rate(string pair) => Table.Find(pair).ToFin(new UnknownPair());

    public static Source<Fin<decimal>> Outcomes(Source<string> pairs) => pairs.Map(Rate);

    public static Source<string> Outputs(Source<string> pairs) =>
        Source.pure("Enter a currency pair").Combine(
            Outcomes(pairs).Map(static outcome => outcome.Match(
                Succ: static rate => rate.ToString(CultureInfo.InvariantCulture),
                Fail: static error => error.Message)));

    public static IO<(Seq<decimal> Rates, Seq<Error> Errors)> Partitioned(Source<string> pairs) =>
        Outcomes(pairs).Reduce(
            (Rates: Seq<decimal>(), Errors: Seq<Error>()),
            static (state, outcome) => outcome.Match(
                Succ: rate => (state.Rates.Add(rate), state.Errors),
                Fail: error => (state.Rates, state.Errors.Add(error))));
}
