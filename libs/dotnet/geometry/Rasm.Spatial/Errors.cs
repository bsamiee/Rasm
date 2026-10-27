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

    public string Message { get; }
}

public sealed record SpatialFault(string Subject, SpatialRule Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject} at index {at}"), () => $"{Reason.Message}: {Subject}"), 0, Inner) {
    public static Fin<Unit> Unless<T>(bool flag, SpatialRule reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new SpatialFault(string.Create(CultureInfo.InvariantCulture, $"{names} = {rejected}"), reason, index);
}
