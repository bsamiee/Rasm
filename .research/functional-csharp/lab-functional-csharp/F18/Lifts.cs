namespace Lab.F18;

internal static class Codes {
    public const int ProviderDown = 3001;
    public const int UnknownAccount = 3002;
    public const int InsufficientFunds = 3003;
}

internal sealed record Flight(string Airline, decimal Price);

internal sealed record Airline(string Name, IO<Seq<Flight>> Flights);

internal sealed record ProviderDown() : Expected("provider down", Codes.ProviderDown);

internal static class Lifts {
    public static IO<Flight> Known(Flight flight) =>
        IO.pure(flight);

    public static IO<Flight> Fetch(Func<Task<Flight>> request) =>
        IO.liftAsync(request);

    public static IO<Flight> FetchWithToken(Func<CancellationToken, Task<Flight>> request) =>
        IO.liftAsync(env => request(env.Token));

    public static IO<decimal> Price(IO<Flight> flight) =>
        flight.Map(static f => f.Price);

    public static IO<int> Seats(IO<Flight> flight, Func<Flight, IO<int>> availability) =>
        flight.Bind(availability);

    public static IO<decimal> Total(IO<Flight> outbound, IO<Flight> inbound) =>
        from o in outbound
        from i in inbound
        select o.Price + i.Price;
}
