using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Rhino;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class RhinoReason {
    public static readonly RhinoReason Stated = new("rejected input");
    public static readonly RhinoReason Declined = new("host declined");
    public static readonly RhinoReason Missing = new("nothing found");
    public static readonly RhinoReason Invalid = new("invalid");
    public static readonly RhinoReason Outside = new("out of range");
    public static readonly RhinoReason Repeated = new("repeated");
    public static readonly RhinoReason Mismatched = new("wrong type");
    public static readonly RhinoReason Unavailable = new("unavailable in its plug-in state");
    public static readonly RhinoReason Unsupported = new("unsupported by the host");
    public static readonly RhinoReason Raised = new("script raised");
    public static readonly RhinoReason Nothing = new("command ended with nothing");
    public static readonly RhinoReason Exited = new("command ended with Rhino exiting");
    public static readonly RhinoReason Unknown = new("unknown command");
    public static readonly RhinoReason Taken = new("already taken");
    public static readonly RhinoReason Unwritable = new("not writable");
    public static readonly RhinoReason Unreadable = new("not readable");
    public static readonly RhinoReason Unqualified = new("not qualified");

    public string Message { get; }
}

public sealed record RhinoFault(string Subject, RhinoReason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(
        Index.Match(
            at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject} at index {at}"),
            () => $"{Reason.Message}: {Subject}"),
        0,
        Inner),
    IValidationError<RhinoFault> {
    public static RhinoFault Create(string message) => new(message, RhinoReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, RhinoReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{names} = {rejected}"), reason, index);
}
