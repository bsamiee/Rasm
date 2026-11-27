using System.Globalization;

namespace Rasm.Telemetry.Api;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IFaultReason {
    public string Message { get; }
}

// --- [ERRORS] --------------------------------------------------------------------------

public sealed record Fault<TReason>(object? Subject, Option<TReason> Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected("", 0, Inner),
    IValidationError<Fault<TReason>>
    where TReason : IFaultReason {
    public object? Subject { get; } = Subject;
    public Option<TReason> Reason { get; } = Reason;

    public override string Message =>
        string.Join(' ', [
            .. Optional(Subject).Map(static held => held is "" ? "\"\"" : string.Create(CultureInfo.InvariantCulture, $"{held}")).ToSeq(),
            .. Reason.Map(static reason => reason.Message).ToSeq(),
            .. Index.Map(static at => string.Create(CultureInfo.InvariantCulture, $"at index {at}")).ToSeq(),
        ]);

    public static Fault<TReason> Create(string message) => new(message, None);
}

public static class Fault {
    public static Fault<TReason> Of<TReason>(TReason reason, object? subject, Option<int> index = default, Option<Error> inner = default) where TReason : IFaultReason =>
        new(subject, reason, index, inner);

    public static Fin<Unit> Unless<TReason>(bool flag, TReason reason, object? subject, Option<int> index = default) where TReason : IFaultReason =>
        flag ? unit : Of(reason, subject, index);
}
