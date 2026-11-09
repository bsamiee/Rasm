using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Grasshopper;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class GrasshopperReason {
    public static readonly GrasshopperReason Stated = new("rejected input");
    public static readonly GrasshopperReason Declined = new("host declined");
    public static readonly GrasshopperReason Invalid = new("fails validation");
    public static readonly GrasshopperReason Missing = new("not found");
    public static readonly GrasshopperReason Taken = new("already in use");
    public static readonly GrasshopperReason Unfinished = new("did not complete");
    public static readonly GrasshopperReason Unloaded = new("not loaded");
    public static readonly GrasshopperReason Unreadable = new("cannot be read");

    public string Message { get; }
}

public sealed record GrasshopperFault(object? Subject, GrasshopperReason Reason, Option<int> Index = default, Option<Error> Inner = default, Option<string> Expression = default)
    : Expected(Reason.Message, 0, Inner),
    IValidationError<GrasshopperFault> {
    public object? Subject { get; } = Subject;
    public GrasshopperReason Reason { get; } = Reason;
    public Option<string> Expression { get; } = Expression;

    public override string Message =>
        string.Concat([
            Reason.Message, ": ", .. Expression.Map(static named => $"{named} = ").ToSeq(), string.Create(CultureInfo.InvariantCulture, $"{Subject}"),
            .. Index.Map(static at => string.Create(CultureInfo.InvariantCulture, $" at index {at}")).ToSeq(),
        ]);

    public static GrasshopperFault Create(string message) => new(message, GrasshopperReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, GrasshopperReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string? names = null) =>
        flag ? unit : new GrasshopperFault(rejected, reason, index, Expression: Optional(names));
}
