using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Document;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int Refused = 1000;

    public const int RefusedElement = 1001;

    public const int Missing = 1002;

    public const int Canceled = 1003;

    public const int NothingEntered = 1004;

    public const int ExitRequested = 1005;

    public const int UnknownCommand = 1006;

    public const int NoActiveView = 1007;

    public const int Invalid = 1008;

    public const int FactoryRejected = 1009;

    public const int InvalidElement = 1010;

    public const int BelowLowerLimit = 1011;

    public const int NotGreaterThan = 1012;

    public const int AboveUpperLimit = 1013;

    public const int NotLessThan = 1014;

    public const int IndexOutOfRange = 1015;

    public const int CountMismatch = 1016;

    public const int CallbackSkipped = 1017;

    public const int MissingGuid = 1018;

    public const int ExportMissingGuid = 1019;

    public const int Duplicate = 1020;

    public const int Ambiguous = 1021;

    public const int UndoRecordUnavailable = 1022;

    public const int UndoRecordOpen = 1023;

    public const int WrongType = 1024;

    public const int LayerCycle = 1025;

    public const int LayerUnreachable = 1026;

    public const int InvalidAnswer = 1027;

    public const int LayerPathTaken = 1028;

    public const int CurrentLayerPruned = 1029;

    public const int Taken = 1030;

    public const int MissingComponent = 1031;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record Refused(string Member) : Expected("{Member} refused the call", Codes.Refused) {
    public static Fin<Unit> Unless(bool accepted, string member) => accepted ? unit : new Refused(member);

    public static Fin<T> Unless<T>(bool accepted, T value, string member) => accepted ? value : new Refused(member);
}

public sealed record RefusedElement(string Member, int Index) : Expected("{Member} returned false for the element at index {Index}", Codes.RefusedElement) {
    public static Fin<Unit> Unless(bool accepted, string member, int index) => accepted ? unit : new RefusedElement(member, index);

    public static Fin<T> Unless<T>(bool accepted, T value, string member, int index) => accepted ? value : new RefusedElement(member, index);
}

public sealed record Missing(string Member) : Expected("{Member} found nothing", Codes.Missing) {
    public static Fin<Unit> Unless(bool found, string member) => found ? unit : new Missing(member);

    public static Fin<T> Unless<T>(T? value, string member) where T : class => Optional(value).ToFin(new Missing(member));
}

public sealed record Canceled() : Expected("Canceled", Codes.Canceled) {
    public static Fin<T> Unless<T>(bool accepted, T value) => accepted ? value : new Canceled();
}

public sealed record NothingEntered() : Expected("Prompt returned nothing", Codes.NothingEntered);

public sealed record ExitRequested() : Expected("Rhino requested exit", Codes.ExitRequested);

public sealed record UnknownCommand(string CommandName) : Expected("{CommandName} is not a known command", Codes.UnknownCommand);

public sealed record NoActiveView() : Expected("Document has no active view", Codes.NoActiveView);

public abstract record ValidationFailure : Expected, IValidationError<ValidationFailure> {
    protected ValidationFailure(string message, int code) : base(message, code) { }

    public static ValidationFailure Create(string message) => new FactoryRejected(message);
}

public sealed record Invalid(string Member) : ValidationFailure("Invalid value for {Member}", Codes.Invalid) {
    public static Fin<Unit> Unless(bool valid, string member) => valid ? unit : new Invalid(member);

    public static Fin<T> Unless<T>(bool valid, T value, string member) => valid ? value : new Invalid(member);
}

public sealed record FactoryRejected(string Reason) : ValidationFailure("{Reason}", Codes.FactoryRejected);

public sealed record InvalidElement(string Member, int Index) : Expected("Invalid value for {Member} at index {Index}", Codes.InvalidElement);

public abstract record LimitViolation : ValidationFailure {
    protected LimitViolation(string message, int code) : base(message, code) { }
}

public sealed record BelowLowerLimit(string Subject, IFormattable Limit) : LimitViolation("{Subject} must be at least {Limit}", Codes.BelowLowerLimit);

public sealed record NotGreaterThan(string Subject, IFormattable Limit) : LimitViolation("{Subject} must be greater than {Limit}", Codes.NotGreaterThan);

public sealed record AboveUpperLimit(string Subject, IFormattable Limit) : LimitViolation("{Subject} must be at most {Limit}", Codes.AboveUpperLimit);

public sealed record NotLessThan(string Subject, IFormattable Limit) : LimitViolation("{Subject} must be less than {Limit}", Codes.NotLessThan);

public sealed record IndexOutOfRange(string Member, int Index, int ItemCount) : Expected("{Member} index {Index} is outside {ItemCount} items", Codes.IndexOutOfRange) {
    public static Fin<Unit> Unless(int index, int itemCount, string member) => (index >= 0) && (index < itemCount) ? unit : new IndexOutOfRange(member, index, itemCount);
}

public sealed record CountMismatch(string Member, int Required, int ItemCount) : Expected("{Member} requires {Required} items and received {ItemCount}", Codes.CountMismatch) {
    public static Fin<Unit> Unless(int required, int itemCount, string member) => required == itemCount ? unit : new CountMismatch(member, required, itemCount);
}

public sealed record CallbackSkipped(string Member) : Expected("{Member} returned without calling the callback", Codes.CallbackSkipped);

public sealed record MissingGuid(Type Subject) : Expected("{Subject} requires a Guid attribute", Codes.MissingGuid) {
    public static Fin<Unit> Unless(Type type) => Attribute.IsDefined(type, typeof(GuidAttribute), inherit: false) ? unit : new MissingGuid(type);
}

public sealed record ExportMissingGuid(string Member) : Expected("{Member} found an exported class without a Guid attribute", Codes.ExportMissingGuid);

public sealed record Duplicate<TKey>(string Member, TKey Key, int ItemCount) : Expected("{Member} received {Key} {ItemCount} times", Codes.Duplicate);

public sealed record Ambiguous(string Member, int Matched) : Expected("{Member} matched {Matched} items where one was required", Codes.Ambiguous);

public sealed record UndoRecordUnavailable() : Expected("Undo record unavailable: a command is running without undo recording", Codes.UndoRecordUnavailable);

public sealed record UndoRecordOpen(uint Serial) : Expected("Undo record {Serial} is open", Codes.UndoRecordOpen);

public sealed record WrongType(Type Required, Type Actual) : Expected("{Actual} is given where {Required} is required", Codes.WrongType);

public sealed record LayerCycle(Guid Layer) : Expected("Layer {Layer} is its own ancestor", Codes.LayerCycle) {
    public static Fin<Unit> Unless(bool acyclic, Guid layer) => acyclic ? unit : new LayerCycle(layer);
}

public sealed record LayerUnreachable(Seq<Guid> Layers) : Expected("Layers {Layers} are not reachable from a root layer", Codes.LayerUnreachable);

public sealed record InvalidAnswer(string Member) : Expected("{Member} returned a value outside its contract", Codes.InvalidAnswer) {
    public static Fin<T> Unless<T>(bool valid, T value, string member) => valid ? value : new InvalidAnswer(member);
}

public sealed record LayerPathTaken(string Path, Guid Occupant) : Expected("Layer path {Path} is held by layer {Occupant}", Codes.LayerPathTaken);

public sealed record CurrentLayerPruned(Guid Layer) : Expected("Pruning deletes current layer {Layer} and no current layer is named", Codes.CurrentLayerPruned);

public sealed record Taken(string Member, string Key) : Expected("{Member} already holds {Key}", Codes.Taken) {
    public static Fin<Unit> Unless(bool free, string member, string key) => free ? unit : new Taken(member, key);
}

public sealed record MissingComponent(ModelComponentType Type, ComponentRef Address) : Expected("No {Type} matches {Address}", Codes.MissingComponent);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ErrorOps {
    public static string Localize(Error error) =>
        string.Join(System.Environment.NewLine, error.AsIterable().Map(static leaf => leaf is Expected expected ? Filled(expected) : leaf.Message));

    public static void Report(Error error) => RhinoApp.WriteLine(Localize(error));

    private static string Filled(Expected error) =>
        toSeq(error.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)).Fold(
            Localization.LocalizeString(error.Message, error, -1),
            (text, property) => text.Replace(
                $"{{{property.Name}}}",
                string.Format(CultureInfo.GetCultureInfo(Localization.CurrentLanguageId), "{0}", property.GetValue(error)),
                StringComparison.Ordinal));
}
