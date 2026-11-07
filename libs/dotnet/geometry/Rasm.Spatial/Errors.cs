using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Spatial;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class SpatialRule {
    public static readonly SpatialRule Stated = new("rejected input");
    public static readonly SpatialRule Count = new("wrong element count");
    public static readonly SpatialRule Order = new("values out of order");
    public static readonly SpatialRule Shape = new("wrong shape");
    public static readonly SpatialRule Range = new("value out of range");
    public static readonly SpatialRule Degenerate = new("degenerate extent");
    public static readonly SpatialRule Singular = new("singular map");

    public string Message { get; }
}

public sealed record SpatialFault(object? Subject, SpatialRule Reason, Option<int> Index = default, Option<Error> Inner = default, string Expression = "")
    : Expected(Reason.Message, 0, Inner),
    IValidationError<SpatialFault> {
    public object? Subject { get; } = Subject;
    public SpatialRule Reason { get; } = Reason;
    public string Expression { get; } = Expression;

    public override string Message {
        get {
            string subject = Expression.Length is 0 ? string.Create(CultureInfo.InvariantCulture, $"{Subject}") : string.Create(CultureInfo.InvariantCulture, $"{Expression} = {Subject}");
            return Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {subject} at index {at}"), () => $"{Reason.Message}: {subject}");
        }
    }

    public static SpatialFault Create(string message) => new(message, SpatialRule.Stated);

    public static Fin<Unit> Unless<T>(bool flag, SpatialRule reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new SpatialFault(rejected, reason, index, Expression: names);
}
