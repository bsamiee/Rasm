namespace Lab.F10;

internal sealed record Location(string Name);

[Union]
internal abstract partial record CustomerOffering {
    public required int Id { get; init; }

    internal sealed record Holiday : CustomerOffering {
        public required Location Destination { get; init; }
        public required Location DepartureAirport { get; init; }
        public required DateTime StartDate { get; init; }
        public required int DurationOfStay { get; init; }
    }
    internal sealed record DayTrip : CustomerOffering {
        public required DateTime DateOfTrip { get; init; }
        public required Location Attraction { get; init; }
        public required bool CoachTripRequired { get; init; }
    }
}

internal static class Offerings {
    public static string Format(CustomerOffering offering) =>
        offering.Switch(
            holiday: static x => string.Create(CultureInfo.InvariantCulture, $"{x.Destination.Name}, {x.DurationOfStay} nights"),
            dayTrip: static x => x.Attraction.Name);
}
