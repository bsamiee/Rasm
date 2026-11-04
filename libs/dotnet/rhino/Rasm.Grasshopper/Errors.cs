using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Grasshopper;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class GrasshopperReason {
    public static readonly GrasshopperReason Stated = new("rejected input");
    public static readonly GrasshopperReason Declined = new("host declined");
    public static readonly GrasshopperReason Invalid = new("rule violated");
    public static readonly GrasshopperReason Missing = new("missing");
    public static readonly GrasshopperReason Taken = new("taken");
    public static readonly GrasshopperReason Unfinished = new("did not complete");
    public static readonly GrasshopperReason Unloaded = new("not loaded");
    public static readonly GrasshopperReason Unreadable = new("unreadable");

    public string Message { get; }
}

public sealed record GrasshopperFault(object? Subject, GrasshopperReason Reason, Option<int> Index = default, Option<Error> Inner = default, string Expression = "")
    : Expected(Reason.Message, 0, Inner),
    IValidationError<GrasshopperFault> {
    public object? Subject { get; } = Subject;
    public GrasshopperReason Reason { get; } = Reason;
    public string Expression { get; } = Expression;

    public override string Message {
        get {
            string subject = Expression.Length is 0 ? string.Create(CultureInfo.InvariantCulture, $"{Subject}") : string.Create(CultureInfo.InvariantCulture, $"{Expression} = {Subject}");
            return Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {subject} at index {at}"), () => $"{Reason.Message}: {subject}");
        }
    }

    public static GrasshopperFault Create(string message) => new(message, GrasshopperReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, GrasshopperReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new GrasshopperFault(rejected, reason, index, Expression: names);
}
