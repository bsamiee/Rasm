using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Imaging;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ImagingReason {
    public static readonly ImagingReason Stated = new("rejected input");
    public static readonly ImagingReason Range = new("value out of range");
    public static readonly ImagingReason Mismatch = new("mismatched counterpart");
    public static readonly ImagingReason Empty = new("no usable sample");
    public static readonly ImagingReason Miscounted = new("wrong sample count");
    public static readonly ImagingReason Degenerate = new("degenerate shape");
    public static readonly ImagingReason NoInverse = new("no exact inverse");
    public static readonly ImagingReason UnknownFormat = new("unknown format");
    public static readonly ImagingReason Unsupported = new("unsupported");
    public static readonly ImagingReason Malformed = new("malformed text");
    public static readonly ImagingReason Refused = new("refused by codec");
    public static readonly ImagingReason Unreadable = new("unreadable");
    public static readonly ImagingReason Oversize = new("over native limits");
    public static readonly ImagingReason Missing = new("missing");

    public string Message { get; }
}

public sealed record ImagingFault(object? Subject, ImagingReason Reason, Option<int> Index = default, Option<Error> Inner = default, string Expression = "")
    : Expected(Reason.Message, 0, Inner), IValidationError<ImagingFault> {
    public object? Subject { get; } = Subject;
    public ImagingReason Reason { get; } = Reason;
    public string Expression { get; } = Expression;

    public override string Message => string.Create(CultureInfo.InvariantCulture,
        $"{Reason.Message}: {(Expression.Length is 0 ? "" : $"{Expression} = ")}{Subject}{Index.Match(static at => string.Create(CultureInfo.InvariantCulture, $" at index {at}"), static () => "")}");

    public static ImagingFault Create(string message) => new(message, ImagingReason.Stated);

    public static ImagingFault Of<T>(ImagingReason reason, T rejected, Option<int> index = default, Option<Error> inner = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        new(rejected, reason, index, inner, names);

    public static Fin<Unit> Unless<T>(bool flag, ImagingReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : Of(reason, rejected, index, names: names);
}
