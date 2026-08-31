namespace Lab.F01;

internal static class FunctionalModel {
    public static (Seq<int> Source, Seq<int> Result) Tripled() {
        Func<int, int> triple = static x => x * 3;
        Seq<int> source = toSeq(Range(1, 3));
        Seq<int> result = source.Map(triple); // 3, 6, 9
        return (source, result);
    }

    public static List<int> SortedInPlace() {
        List<int> values = [7, 6, 1];
        values.Sort(); // values is now 1, 6, 7
        return values;
    }

    public static (Seq<int> Values, Seq<int> Sorted, Seq<int> Odd) Transformed() {
        Seq<int> values = Seq(7, 6, 1);
        Seq<int> sorted = toSeq(values.Order()); // values remains 7, 6, 1
        Seq<int> odd = values.Filter(static x => x % 2 == 1); // 7, 1
        return (values, sorted, odd);
    }
}
