using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Grasshopper;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class GrasshopperReason {
    public static readonly GrasshopperReason Stated = new("rejected input");
    public static readonly GrasshopperReason Declined = new("declined by the host");
    public static readonly GrasshopperReason Invalid = new("rule violated");
    public static readonly GrasshopperReason Missing = new("missing");
    public static readonly GrasshopperReason Taken = new("taken");
    public static readonly GrasshopperReason Unfinished = new("did not complete");
    public static readonly GrasshopperReason Unloaded = new("not loaded");
    public static readonly GrasshopperReason Unreadable = new("unreadable");

    public string Message { get; }
}

public sealed record GrasshopperFault(string Subject, GrasshopperReason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(
        Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject} at index {at}"), () => $"{Reason.Message}: {Subject}"),
        0,
        Inner),
    IValidationError<GrasshopperFault> {
    public static GrasshopperFault Create(string message) => new(message, GrasshopperReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, GrasshopperReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new GrasshopperFault(string.Create(CultureInfo.InvariantCulture, $"{names} = {rejected}"), reason, index);
}
