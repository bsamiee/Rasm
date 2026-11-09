using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Geometry;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class GeometryRule {
    public static readonly GeometryRule Stated = new("rejected input");
    public static readonly GeometryRule Count = new("wrong element count");
    public static readonly GeometryRule Order = new("out of order");
    public static readonly GeometryRule Shape = new("wrong shape");
    public static readonly GeometryRule Range = new("out of range");
    public static readonly GeometryRule Degenerate = new("degenerate extent");
    public static readonly GeometryRule Singular = new("singular map");
    public static readonly GeometryRule Distortion = new("map distorts distance unevenly");

    public string Message { get; }
}

public sealed record GeometryFault(object? Subject, GeometryRule Reason, Option<int> Index = default, Option<Error> Inner = default, Option<string> Expression = default)
    : Expected(Reason.Message, 0, Inner),
    IValidationError<GeometryFault> {
    public object? Subject { get; } = Subject;
    public GeometryRule Reason { get; } = Reason;
    public Option<string> Expression { get; } = Expression;

    public override string Message =>
        string.Concat([
            Reason.Message, ": ", .. Expression.Map(static named => $"{named} = ").ToSeq(), string.Create(CultureInfo.InvariantCulture, $"{Subject}"),
            .. Index.Map(static at => string.Create(CultureInfo.InvariantCulture, $" at index {at}")).ToSeq(),
        ]);

    public static GeometryFault Create(string message) => new(message, GeometryRule.Stated);

    public static Fin<Unit> Unless<T>(bool flag, GeometryRule reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string? names = null) =>
        flag ? unit : new GeometryFault(rejected, reason, index, Expression: Optional(names));
}
