namespace Rasm.Imaging;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ImagingFault(string Message, Option<Error> Inner = default) : Expected(Message, 0, Inner), IValidationError<ImagingFault> {
    public static ImagingFault Create(string message) => new(message);
}
