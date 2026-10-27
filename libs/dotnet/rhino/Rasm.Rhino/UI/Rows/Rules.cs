namespace Rasm.RhinoCommon.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RowRule(Func<RowScope, IO<bool>> Holds) {
    public static RowRule operator &(RowRule left, RowRule right) => BitwiseAnd(left, right);

    public static RowRule operator |(RowRule left, RowRule right) => BitwiseOr(left, right);

    public static RowRule operator !(RowRule rule) => LogicalNot(rule);

    public static RowRule BitwiseAnd(RowRule left, RowRule right) => new(scope => left.Holds(scope).Bind(held => held ? right.Holds(scope) : IO.pure(value: false)));

    public static RowRule BitwiseOr(RowRule left, RowRule right) => new(scope => left.Holds(scope).Bind(held => held ? IO.pure(value: true) : right.Holds(scope)));

    public static RowRule LogicalNot(RowRule rule) => new(scope => rule.Holds(scope).Map(static held => !held));
}
