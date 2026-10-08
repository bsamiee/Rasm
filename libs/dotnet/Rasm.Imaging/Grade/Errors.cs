namespace Rasm.Imaging.Grade;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidGrade = 1,
    CorrectionUnreadable,
    CorrectionMalformed,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidGrade() : Expected("grade value outside its limits", (int)Codes.InvalidGrade), IValidationError<InvalidGrade> {
    public static InvalidGrade Create(string message) => new();
}

public sealed record CorrectionUnreadable : Expected {
    public CorrectionUnreadable(Error cause) : base("CDL text unreadable", (int)Codes.CorrectionUnreadable, cause) { }
}

public sealed record CorrectionMalformed(string Element) : Expected("CDL correction misstates an element", (int)Codes.CorrectionMalformed);
