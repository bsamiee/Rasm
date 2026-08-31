namespace Lab.F09;

internal static class DataFlow {
    public static decimal AverageEarningsOfRichestQuartile(Seq<Person> population) =>
        population
            .OrderByDescending(static p => p.Earnings) // reorder, preserve elements
            .Take(population.Count / 4)                // reduce cardinality
            .Average(static p => p.Earnings);          // project and collapse to a scalar
}
