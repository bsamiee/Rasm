using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Document;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record EmptyGuid(string Member) : Expected("{Member} returned an empty Guid", ErrorOps.Code<EmptyGuid>());

public sealed record NegativeIndex(string Member) : Expected("{Member} returned a negative index", ErrorOps.Code<NegativeIndex>());

public sealed record Refused(string Member) : Expected("{Member} returned false", ErrorOps.Code<Refused>()) {
    public static Fin<Unit> Unless(bool accepted, string member) => accepted ? unit : new Refused(member);

    public static Fin<T> Unless<T>(bool accepted, T value, string member) => accepted ? value : new Refused(member);
}

public sealed record RefusedElement(string Member, int Index) : Expected("{Member} returned false for the element at index {Index}", ErrorOps.Code<RefusedElement>());

public sealed record Missing(string Member) : Expected("{Member} found nothing", ErrorOps.Code<Missing>()) {
    public static Fin<Unit> Unless(bool found, string member) => found ? unit : new Missing(member);

    public static Fin<T> Unless<T>(T? value, string member) where T : class => Optional(value).ToFin(new Missing(member));
}

public sealed record Canceled() : Expected("Canceled", ErrorOps.Code<Canceled>()) {
    public static Fin<Unit> Unless(bool accepted) => accepted ? unit : new Canceled();
}

public sealed record NothingEntered() : Expected("Prompt returned nothing", ErrorOps.Code<NothingEntered>());

public sealed record ExitRequested() : Expected("Rhino requested exit", ErrorOps.Code<ExitRequested>());

public sealed record UnknownCommand(string CommandName) : Expected("{CommandName} is not a known command", ErrorOps.Code<UnknownCommand>());

public sealed record NoActiveView() : Expected("Document has no active view", ErrorOps.Code<NoActiveView>());

public abstract record ValidationFailure : Expected, IValidationError<ValidationFailure> {
    protected ValidationFailure(string message, int code) : base(message, code) { }

    public static ValidationFailure Create(string message) => new FactoryRejected(message);
}

public sealed record Invalid(string Member) : ValidationFailure("Invalid value for {Member}", ErrorOps.Code<Invalid>()) {
    public static Fin<Unit> Unless(bool valid, string member) => valid ? unit : new Invalid(member);

    public static Fin<T> Unless<T>(bool valid, T value, string member) => valid ? value : new Invalid(member);
}

public sealed record FactoryRejected(string Reason) : ValidationFailure("{Reason}", ErrorOps.Code<FactoryRejected>());

public sealed record InvalidElement(string Member, int Index) : Expected("Invalid value for {Member} at index {Index}", ErrorOps.Code<InvalidElement>());

public sealed record InvalidOutput(string Member, int ItemCount) : Expected("{Member} returned {ItemCount} invalid items", ErrorOps.Code<InvalidOutput>());

public abstract record LimitViolation : Expected {
    protected LimitViolation(string message, int code) : base(message, code) { }
}

public sealed record BelowLowerLimit(string Subject, IFormattable Limit) : LimitViolation("{Subject} must be at least {Limit}", ErrorOps.Code<BelowLowerLimit>());

public sealed record NotGreaterThan(string Subject, IFormattable Limit) : LimitViolation("{Subject} must be greater than {Limit}", ErrorOps.Code<NotGreaterThan>());

public sealed record AboveUpperLimit(string Subject, IFormattable Limit) : LimitViolation("{Subject} must be at most {Limit}", ErrorOps.Code<AboveUpperLimit>());

public sealed record NotLessThan(string Subject, IFormattable Limit) : LimitViolation("{Subject} must be less than {Limit}", ErrorOps.Code<NotLessThan>());

public sealed record IndexOutOfRange(string Member, int Index, int ItemCount) : Expected("{Member} index {Index} is outside {ItemCount} items", ErrorOps.Code<IndexOutOfRange>()) {
    public static Fin<Unit> Unless(int index, int itemCount, string member) => (index >= 0) && (index < itemCount) ? unit : new IndexOutOfRange(member, index, itemCount);
}

public sealed record CountMismatch(string Member, int Required, int ItemCount) : Expected("{Member} requires {Required} items and received {ItemCount}", ErrorOps.Code<CountMismatch>()) {
    public static Fin<Unit> Unless(int required, int itemCount, string member) => required == itemCount ? unit : new CountMismatch(member, required, itemCount);
}

public sealed record UnexpectedResult(string Member, Result Result) : Expected("{Member} returned {Result}", ErrorOps.Code<UnexpectedResult>());

public sealed record CallbackSkipped(string Member) : Expected("{Member} returned without calling the callback", ErrorOps.Code<CallbackSkipped>());

public sealed record MissingGuid(Type Subject) : Expected("{Subject} requires a Guid attribute", ErrorOps.Code<MissingGuid>()) {
    public static Fin<Unit> Unless(Type type) => Attribute.IsDefined(type, typeof(GuidAttribute), inherit: false) ? unit : new MissingGuid(type);
}

public sealed record ExportMissingGuid(string Member) : Expected("{Member} found an exported class without a Guid attribute", ErrorOps.Code<ExportMissingGuid>());

public sealed record Mismatch(string Member) : Expected("{Member} read back a value other than the one written", ErrorOps.Code<Mismatch>()) {
    public static Fin<Unit> Unless(bool same, string member) => same ? unit : new Mismatch(member);
}

public sealed record DuplicateIndex(string Member, int Index) : Expected("{Member} received index {Index} more than once", ErrorOps.Code<DuplicateIndex>());

public sealed record Ambiguous(string Member, int Matched) : Expected("{Member} matched {Matched} items where one was required", ErrorOps.Code<Ambiguous>());

public sealed record UndoRecordUnavailable(bool InCommand, bool UndoRecordingIsActive)
    : Expected("Undo record unavailable with InCommand {InCommand} and UndoRecordingIsActive {UndoRecordingIsActive}", ErrorOps.Code<UndoRecordUnavailable>());

public sealed record UndoRecordOpen(uint Serial) : Expected("Undo record {Serial} is open", ErrorOps.Code<UndoRecordOpen>());

public sealed record UndoRecordInCommand(uint Serial) : Expected("Undo record {Serial} belongs to the running command", ErrorOps.Code<UndoRecordInCommand>());

public sealed record NotOwned() : Expected("Geometry is not owned by the handle", ErrorOps.Code<NotOwned>()) {
    public static Fin<Unit> Unless(bool owned) => owned ? unit : new NotOwned();
}

public sealed record WrongGeometry(Type Required, ObjectType Actual) : Expected("Geometry is {Actual} where {Required} is required", ErrorOps.Code<WrongGeometry>());

public sealed record LayerOrphan(Guid Layer, Guid Parent) : Expected("Layer {Layer} has parent {Parent}, which is not in the layer table", ErrorOps.Code<LayerOrphan>());

public sealed record LayerCycle(Guid Layer) : Expected("Layer {Layer} is its own ancestor", ErrorOps.Code<LayerCycle>());

public sealed record UnboundedChannel(EventKind Kind) : Expected("{Kind} raises once per frame and requires a bounded channel", ErrorOps.Code<UnboundedChannel>());

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ErrorOps {
    // --- [CODES]
    public static int Code<TError>() where TError : Expected =>
        typeof(TError).GUID.GetHashCode();

    // --- [MESSAGES]
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
