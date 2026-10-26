namespace Rasm.Imaging;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ImagingReason {
    public static readonly ImagingReason Range = new("lies outside its valid range");
    public static readonly ImagingReason Mismatch = new("differs from its counterpart");
    public static readonly ImagingReason Empty = new("holds no usable sample");
    public static readonly ImagingReason Miscounted = new("holds the wrong sample count");
    public static readonly ImagingReason Degenerate = new("is degenerate");
    public static readonly ImagingReason Uninvertible = new("has no exact inverse");
    public static readonly ImagingReason Unrooted = new("brackets no root");
    public static readonly ImagingReason Unnamed = new("names no supported format");
    public static readonly ImagingReason Unsupported = new("is unsupported");
    public static readonly ImagingReason Unparsed = new("did not parse");
    public static readonly ImagingReason Refused = new("was refused by its codec");
    public static readonly ImagingReason Unreadable = new("is unreadable");
    public static readonly ImagingReason Oversize = new("exceeds native limits");
    public static readonly ImagingReason Uncamera = new("needs the frame camera");
    public static readonly ImagingReason Repeated = new("repeats");
    public static readonly ImagingReason Stated = new("fails its factory rule");

    public string Message { get; }
}

public sealed record ImagingFault(string Subject, ImagingReason Reason, Option<Error> Cause = default) : Expected($"{Subject} {Reason.Message}", 0, Cause), IValidationError<ImagingFault> {
    public static ImagingFault Create(string message) => new(message, ImagingReason.Stated);
}
