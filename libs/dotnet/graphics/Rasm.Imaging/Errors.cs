namespace Rasm.Imaging;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ImagingFault(string Message) : Expected(Message, 0), IValidationError<ImagingFault> {
    public static ImagingFault Create(string message) => new(message);
}
