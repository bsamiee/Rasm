namespace Lab.F18;

internal static partial class Traversals {
    public static Option<Seq<double>> ParseAll(Seq<string> values) =>
        values.Traverse(static s => parseDouble(s)).As();
}

internal static partial class Traversals {
    public static Validation<Error, Seq<int>> ValidateAll(Seq<string> values, Func<string, Validation<Error, int>> validate) =>
        values.Traverse(validate).As();

    public static Validation<Error, Seq<int>> ValidateUntilFirstFailure(Seq<string> values, Func<string, Validation<Error, int>> validate) =>
        values.TraverseM(validate).As();
}

internal static partial class Traversals {
    public static IO<Seq<Flight>> SearchParallel(Seq<Airline> airlines) =>
        airlines.Traverse(static airline => airline.Flights).As().Map(static groups => groups.Flatten());

    public static IO<Seq<Flight>> SearchSerial(Seq<Airline> airlines) =>
        airlines.TraverseM(static airline => airline.Flights).As().Map(static groups => groups.Flatten());

    public static IO<Seq<Flight>> SearchBestEffort(Seq<Airline> airlines) =>
        airlines.Map<K<IO, Seq<Flight>>>(static airline => airline.Flights).PartitionFallible().As().Map(static parts => parts.Succs.Flatten());
}

internal static class Layers {
    public static Option<Validation<Error, int>> Parse(Validation<Error, string> validated) =>
        validated.Traverse(static s => parseInt(s)).As();

    public static Option<Validation<Error, int>> Swap(Validation<Error, Option<int>> stacked) =>
        stacked.Traverse(static option => option).As();
}
