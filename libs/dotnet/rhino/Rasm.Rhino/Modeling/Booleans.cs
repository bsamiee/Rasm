namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Boolean<T> where T : GeometryBase {
    public sealed record Union(Seq<T> Operands) : Boolean<T>;

    public sealed record Intersection(Seq<T> First, Seq<T> Second) : Boolean<T>;

    public sealed record Difference(Seq<T> First, Seq<T> Second) : Boolean<T>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Booleans {
    // --- [OPERANDS]
    internal static IO<Seq<Grouped<T, int>>> PerFirst<T>(Seq<T> first, Func<T, IO<Seq<T>>> step) =>
        first.Map(static (operand, index) => (Operand: operand, Index: index))
            .TraverseM(at => step(at.Operand).Map(pieces => pieces.Map(piece => new Grouped<T, int>(piece, [at.Index]))))
            .As()
            .Map(static groups => groups.Bind(static group => group));

    internal static IO<Seq<Grouped<T, int>>> Meet<T>(Seq<T> first, Seq<T> second, Func<IO<Seq<T>>> union, Func<T, T, IO<Seq<T>>> binary) =>
        from pieces in second.Count switch {
            0 => IO.pure(Seq<T>()),
            1 => IO.pure(second),
            _ => union(),
        }
        from met in PerFirst(first, operand => pieces.TraverseM(piece => binary(operand, piece)).As().Map(static met => met.Bind(static held => held)))
        select met;
}
