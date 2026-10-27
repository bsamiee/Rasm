using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Grasshopper;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class GrasshopperReason {
    public static readonly GrasshopperReason Stated = new("{0}");
    public static readonly GrasshopperReason Declined = new("{0} returned failure");
    public static readonly GrasshopperReason Outside = new("{0} is outside its range");
    public static readonly GrasshopperReason Unwired = new("{0} cannot hold a wire");
    public static readonly GrasshopperReason Repeated = new("{0} repeats");
    public static readonly GrasshopperReason Missing = new("{0} matches nothing");
    public static readonly GrasshopperReason Taken = new("{0} is taken");
    public static readonly GrasshopperReason Unfinished = new("{0} did not complete");
    public static readonly GrasshopperReason Unloaded = new("{0} failed to load");
    public static readonly GrasshopperReason Unreadable = new("{0} is unreadable");

    public string Template { get; }
}

public sealed record GrasshopperFault(string Subject, GrasshopperReason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(
        Index.Match(
            at => string.Create(CultureInfo.InvariantCulture, $"{string.Format(CultureInfo.InvariantCulture, Reason.Template, Subject)} at index {at}"),
            () => string.Format(CultureInfo.InvariantCulture, Reason.Template, Subject)),
        ErrorCodes.ValidationFailed,
        Inner),
    IValidationError<GrasshopperFault> {
    public static GrasshopperFault Create(string message) => new(message, GrasshopperReason.Stated);

    public static Fin<Unit> Unless(bool accepted, Option<int> index = default, [CallerArgumentExpression(nameof(accepted))] string call = "") =>
        accepted ? unit : new GrasshopperFault(call, GrasshopperReason.Declined, index);

    public static Fin<Unit> Unless(bool accepted, string report, [CallerArgumentExpression(nameof(accepted))] string call = "") =>
        accepted ? unit : new GrasshopperFault(call, GrasshopperReason.Declined, Inner: Error.New(report));
}
