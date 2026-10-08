namespace Rasm.Imaging.Filters.Detail;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidDetail = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidDetail() : Expected("detail value outside its limits", (int)Codes.InvalidDetail), IValidationError<InvalidDetail> {
    public static InvalidDetail Create(string message) => new();
}
