using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Spatial;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class SpatialRule {
    public static readonly SpatialRule Count = new("wrong element count");
    public static readonly SpatialRule Order = new("values out of order");
    public static readonly SpatialRule Shape = new("wrong shape");
    public static readonly SpatialRule Range = new("value out of range");
    public static readonly SpatialRule Degenerate = new("degenerate extent");
    public static readonly SpatialRule Singular = new("singular map");
    public static readonly SpatialRule Stated = new("rule its factory states");

    public string Message { get; }
}
public sealed record SpatialFault(string Subject, SpatialRule Rule, Option<int> Index = default)
    : Expected(Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Subject} fails: {Rule.Message} at index {at}"), () => $"{Subject} fails: {Rule.Message}"), 0), IValidationError<SpatialFault> {
    public static SpatialFault Create(string message) => new(message, SpatialRule.Stated);

    public static Fin<Unit> Unless(bool flag, SpatialRule rule, Option<int> index = default, [CallerArgumentExpression(nameof(flag))] string subject = "") => flag ? unit : new SpatialFault(subject, rule, index);
}
