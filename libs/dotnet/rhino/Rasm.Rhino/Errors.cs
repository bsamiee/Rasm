using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Rhino;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class RhinoReason {
    public static readonly RhinoReason Stated = new("rejected input");
    public static readonly RhinoReason Declined = new("host declined");
    public static readonly RhinoReason Missing = new("not found");
    public static readonly RhinoReason Invalid = new("fails validation");
    public static readonly RhinoReason Outside = new("out of range");
    public static readonly RhinoReason Repeated = new("appears more than once");
    public static readonly RhinoReason Mismatched = new("wrong type");
    public static readonly RhinoReason Unavailable = new("unavailable in its plug-in state");
    public static readonly RhinoReason Unsupported = new("not supported by the host");
    public static readonly RhinoReason Raised = new("script raised");
    public static readonly RhinoReason Nothing = new("command ended with nothing");
    public static readonly RhinoReason Exited = new("command ended with Rhino exiting");
    public static readonly RhinoReason Unknown = new("unknown command");
    public static readonly RhinoReason Taken = new("already in use");
    public static readonly RhinoReason Unwritable = new("cannot be written");
    public static readonly RhinoReason Unreadable = new("cannot be read");
    public static readonly RhinoReason Unqualified = new("malformed");

    public string Message { get; }
}

public sealed record RhinoFault(object? Subject, RhinoReason Reason, Option<int> Index = default, Option<Error> Inner = default, Option<string> Expression = default)
    : Expected(Reason.Message, 0, Inner),
    IValidationError<RhinoFault> {
    public object? Subject { get; } = Subject;
    public RhinoReason Reason { get; } = Reason;
    public Option<string> Expression { get; } = Expression;

    public override string Message =>
        string.Concat([
            Reason.Message, ": ", .. Expression.Map(static named => $"{named} = ").ToSeq(), string.Create(CultureInfo.InvariantCulture, $"{Subject}"),
            .. Index.Map(static at => string.Create(CultureInfo.InvariantCulture, $" at index {at}")).ToSeq(),
        ]);

    public static RhinoFault Create(string message) => new(message, RhinoReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, RhinoReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string? names = null) =>
        flag ? unit : new RhinoFault(rejected, reason, index, Expression: Optional(names));
}
