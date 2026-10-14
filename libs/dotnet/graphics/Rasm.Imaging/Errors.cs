using TinyEXR;

namespace Rasm.Imaging;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    CodecRefused = 1,
    EmptyRegion,
    CameraRequired,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record CodecRefused : Expected {
    public CodecRefused(ExrResult status, Option<Exception> exception)
        : base("EXR codec refused the operation", (int)Codes.CodecRefused, exception.Map(static thrown => New(thrown))) => Status = status;

    public ExrResult Status { get; }

    public static Fin<T> Unless<T>(ReaderResult<T> result) where T : class => Optional(result.Value).ToFin(new CodecRefused(result.Status, Optional(result.Error)));

    public static Fin<Unit> Unless(WriterResult result) => result.IsSuccess ? unit : new CodecRefused(result.Status, Optional(result.Error));
}

public sealed record EmptyRegion((long Left, long Top, long Right, long Bottom) Region) : Expected("region holds no covered pixel of finite value", (int)Codes.EmptyRegion);

public sealed record CameraRequired() : Expected("operation requires the frame's camera", (int)Codes.CameraRequired);
