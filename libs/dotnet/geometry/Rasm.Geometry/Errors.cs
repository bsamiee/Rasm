using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Geometry;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class GeometryRule {
    public static readonly GeometryRule Stated = new("rejected input");
    public static readonly GeometryRule Count = new("wrong element count");
    public static readonly GeometryRule Order = new("values out of order");
    public static readonly GeometryRule Shape = new("wrong shape");
    public static readonly GeometryRule Range = new("value out of range");
    public static readonly GeometryRule Degenerate = new("degenerate extent");
    public static readonly GeometryRule Singular = new("singular map");

    public string Message { get; }
}

public sealed record GeometryFault(object? Subject, GeometryRule Reason, Option<int> Index = default, Option<Error> Inner = default, string Expression = "")
    : Expected(Reason.Message, 0, Inner),
    IValidationError<GeometryFault> {
    public object? Subject { get; } = Subject;
    public GeometryRule Reason { get; } = Reason;
    public string Expression { get; } = Expression;

    public override string Message {
        get {
            string subject = Expression.Length is 0 ? string.Create(CultureInfo.InvariantCulture, $"{Subject}") : string.Create(CultureInfo.InvariantCulture, $"{Expression} = {Subject}");
            return Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {subject} at index {at}"), () => $"{Reason.Message}: {subject}");
        }
    }

    public static GeometryFault Create(string message) => new(message, GeometryRule.Stated);

    public static Fin<Unit> Unless<T>(bool flag, GeometryRule reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new GeometryFault(rejected, reason, index, Expression: names);
}
