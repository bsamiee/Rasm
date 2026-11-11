using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Imaging;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class ImagingReason {
    public static readonly ImagingReason Stated = new("rejected input");
    public static readonly ImagingReason Range = new("out of range");
    public static readonly ImagingReason Mismatch = new("mismatched counterpart");
    public static readonly ImagingReason Empty = new("no usable sample");
    public static readonly ImagingReason Miscounted = new("wrong sample count");
    public static readonly ImagingReason Degenerate = new("degenerate shape");
    public static readonly ImagingReason NoInverse = new("no exact inverse");
    public static readonly ImagingReason UnknownFormat = new("unknown format");
    public static readonly ImagingReason Unsupported = new("not supported");
    public static readonly ImagingReason Malformed = new("malformed text");
    public static readonly ImagingReason Refused = new("refused by the codec");
    public static readonly ImagingReason Unreadable = new("cannot be read");
    public static readonly ImagingReason Oversize = new("over native limits");
    public static readonly ImagingReason Missing = new("not found");

    public string Message { get; }
}

public sealed record ImagingFault(object? Subject, ImagingReason Reason, Option<int> Index = default, Option<Error> Inner = default, Option<string> Expression = default)
    : Expected(Reason.Message, 0, Inner), IValidationError<ImagingFault> {
    public object? Subject { get; } = Subject;
    public ImagingReason Reason { get; } = Reason;
    public Option<string> Expression { get; } = Expression;

    public override string Message =>
        string.Concat([
            Reason.Message, ": ", .. Expression.Map(static named => $"{named} = ").ToSeq(), string.Create(CultureInfo.InvariantCulture, $"{Subject}"),
            .. Index.Map(static at => string.Create(CultureInfo.InvariantCulture, $" at index {at}")).ToSeq(),
        ]);

    public static ImagingFault Create(string message) => new(message, ImagingReason.Stated);

    public static ImagingFault Of<T>(ImagingReason reason, T rejected, Option<int> index = default, Option<Error> inner = default, [CallerArgumentExpression(nameof(rejected))] string? names = null) =>
        new(rejected, reason, index, inner, Optional(names));

    public static Fin<Unit> Unless<T>(bool flag, ImagingReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string? names = null) =>
        flag ? unit : Of(reason, rejected, index, names: names);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class TextValidation {
    internal static ImagingFault? Parsed<T>(string? value, Func<string, Fin<T>> read, out T? item) where T : class {
        Fin<Option<T>> parsed = Optional(value).Traverse(read).As();
        item = parsed.ToOption().Bind(static held => held).ValueUnsafe();
        return parsed.Match(Succ: static _ => null, Fail: static error => error.Filter<ImagingFault>().Head as ImagingFault);
    }
}
