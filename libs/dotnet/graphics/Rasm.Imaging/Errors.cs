namespace Rasm.Imaging;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class ImagingReason : IFaultReason {
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

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class TextValidation {
    internal static Fault<ImagingReason>? Parsed<T>(string? value, Func<string, Fin<T>> read, out T? item) where T : class {
        Fin<Option<T>> parsed = Optional(value).Traverse(read).As();
        item = parsed.ToOption().Bind(static held => held).ValueUnsafe();
        return parsed.Match(Succ: static _ => null, Fail: static error => error.Filter<Fault<ImagingReason>>().Head as Fault<ImagingReason>);
    }
}
