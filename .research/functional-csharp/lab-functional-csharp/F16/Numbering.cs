namespace Lab.F16;

internal abstract record Tree<T> {
    public abstract State<int, Tree<(int Number, T Value)>> Number();
}

internal sealed record Leaf<T>(T Value) : Tree<T> {
    public override State<int, Tree<(int Number, T Value)>> Number() =>
        Numbering.GetAndIncrement.Map(count => Tree.Leaf((count, Value)));
}

internal sealed record Branch<T>(Tree<T> Left, Tree<T> Right) : Tree<T> {
    public override State<int, Tree<(int Number, T Value)>> Number() =>
        from left in Left.Number()
        from right in Right.Number()
        select Tree.Branch(left, right);
}

internal static class Tree {
    public static Tree<T> Leaf<T>(T value) => new Leaf<T>(value);

    public static Tree<T> Branch<T>(Tree<T> left, Tree<T> right) => new Branch<T>(left, right);
}

internal static class Numbering {
    public static State<int, int> GetAndIncrement { get; } = new(static count => (count, count + 1));

    public static Tree<(int Number, T Value)> Numbered<T>(Tree<T> tree) => tree.Number().Run(0).Value;
}
