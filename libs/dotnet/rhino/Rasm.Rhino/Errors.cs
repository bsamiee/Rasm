using System.Runtime.InteropServices;
using Rhino.Commands;

namespace Rasm.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    Refused = 1,
    RefusedElement,
    Missing,
    InvalidAnswer,
    CountMismatch,
    IndexOutOfRange,
    WrongType,
    Duplicate,
    Taken,
    MissingGuid,
    Ended,
    InvalidRhinoValue,
    UnrepresentableSpan,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record Refused(string Member) : Expected("{Member} refused the call", (int)Codes.Refused) {
    public static Fin<Unit> Unless(bool accepted, string member) => accepted ? unit : new Refused(member);

    public static Fin<T> Unless<T>(bool accepted, T value, string member) => accepted ? value : new Refused(member);
}

public sealed record RefusedElement(string Member, int Index) : Expected("{Member} refused the element at index {Index}", (int)Codes.RefusedElement) {
    public static Fin<Unit> Unless(bool accepted, string member, int index) => accepted ? unit : new RefusedElement(member, index);

    public static Fin<T> Unless<T>(bool accepted, T value, string member, int index) => accepted ? value : new RefusedElement(member, index);
}

public sealed record Missing(string Member) : Expected("{Member} answered nothing", (int)Codes.Missing) {
    public static Fin<Unit> Unless(bool found, string member) => found ? unit : new Missing(member);

    public static Fin<T> Unless<T>(T? value, string member) where T : class => value is { } found ? found : new Missing(member);
}

public sealed record InvalidAnswer(string Member) : Expected("{Member} answered a value outside its contract", (int)Codes.InvalidAnswer) {
    public static Fin<T> Unless<T>(bool valid, T value, string member) => valid ? value : new InvalidAnswer(member);
}

public sealed record CountMismatch(string Member, int Required, int ItemCount) : Expected("{Member} requires {Required} items and received {ItemCount}", (int)Codes.CountMismatch) {
    public static Fin<Unit> Unless(int required, int itemCount, string member) => required == itemCount ? unit : new CountMismatch(member, required, itemCount);
}

public sealed record IndexOutOfRange(string Member, int Index, int ItemCount) : Expected("{Member} index {Index} is outside {ItemCount} items", (int)Codes.IndexOutOfRange) {
    public static Fin<Unit> Unless(int index, int itemCount, string member) => index >= 0 && index < itemCount ? unit : new IndexOutOfRange(member, index, itemCount);
}

public sealed record WrongType(Type Required, Type Actual) : Expected("{Actual} is given where {Required} is required", (int)Codes.WrongType) {
    public static Fin<T> Unless<T>(object value) => value is T typed ? typed : new WrongType(typeof(T), value.GetType());
}

public sealed record Duplicate<TKey>(string Member, TKey Key, int ItemCount) : Expected("{Member} received {Key} {ItemCount} times", (int)Codes.Duplicate);

public sealed record Taken(string Member, string Key) : Expected("{Member} already holds {Key}", (int)Codes.Taken) {
    public static Fin<Unit> Unless(bool free, string member, string key) => free ? unit : new Taken(member, key);
}

public sealed record MissingGuid(Type Subject) : Expected("{Subject} requires a Guid attribute", (int)Codes.MissingGuid) {
    public static Fin<Unit> Unless(Type subject) => Attribute.IsDefined(subject, typeof(GuidAttribute), inherit: false) ? unit : new MissingGuid(subject);
}

public sealed record Ended(string Member, Result Result) : Expected("{Member} ended with {Result}", (int)Codes.Ended);

public sealed record InvalidRhinoValue() : Expected("value out of range", (int)Codes.InvalidRhinoValue), IValidationError<InvalidRhinoValue> {
    public static InvalidRhinoValue Create(string message) => new();
}

public sealed record UnrepresentableSpan(Duration Span, Duration Unit) : Expected("{Span} is no whole count of {Unit}", (int)Codes.UnrepresentableSpan);
