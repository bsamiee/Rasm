using System.Globalization;
using Rhino.Commands;

namespace Rasm.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
public enum FileReason {
    Taken,
    Missing,
    Unwritable,
    Unreadable,
    Unqualified,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record Refused(string Member, string Reason) : Expected($"{Member} {Reason}", 0) {
    public const string Declined = "returned failure";
    public const string Absent = "returned nothing";
    public const string Invalid = "returned invalid value";

    public Refused(string member, string reason, int index) : this(member, string.Create(CultureInfo.InvariantCulture, $"{reason} at index {index}")) { }

    public static Fin<Unit> Unless(bool accepted, string member) => accepted ? unit : new Refused(member, Declined);

    public static Fin<T> Unless<T>(bool accepted, T value, string member) => accepted ? value : new Refused(member, Declined);

    public static Fin<Unit> Unless(bool accepted, string member, int index) => accepted ? unit : new Refused(member, Declined, index);

    public static Fin<T> Unless<T>(bool accepted, T value, string member, int index) => accepted ? value : new Refused(member, Declined, index);

    public static Fin<Unit> UnlessFound(bool found, string member) => found ? unit : new Refused(member, Absent);

    public static Fin<T> UnlessFound<T>(T? value, string member) where T : class => value is { } found ? found : new Refused(member, Absent);

    public static Fin<T> UnlessFound<T>(T? value, string member) where T : struct => value is { } found ? found : new Refused(member, Absent);

    public static Fin<T> UnlessValid<T>(bool valid, T value, string member) => valid ? value : new Refused(member, Invalid);
}

public sealed record RhinoFault(string Message) : Expected(Message, 0), IValidationError<RhinoFault> {
    public static RhinoFault Create(string message) => new(message);
}

public sealed record Ended(string Member, Result Result) : Expected($"{Member} ended with {Result}", 0);

public sealed record NotFound(string Kind, string Key) : Expected($"no {Kind} matches {Key}", 0);

public sealed record FileRefused(string Path, FileReason Reason, Option<Error> Cause = default) : Expected($"{Path} refused as {Reason}", 0, Cause);
