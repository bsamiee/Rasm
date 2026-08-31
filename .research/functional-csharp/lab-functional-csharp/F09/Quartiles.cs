namespace Lab.F09;

internal static class Quartiles {
    public static Seq<Person> RichestQuartile(this Seq<Person> population) =>
        toSeq(population.OrderByDescending(static p => p.Earnings)).Take(population.Count / 4);

    public static decimal AverageEarnings(this IEnumerable<Person> population) => population.Average(static p => p.Earnings);

    public static decimal Result(Seq<Person> population) =>
        population
            .RichestQuartile()
            .AverageEarnings();
}
