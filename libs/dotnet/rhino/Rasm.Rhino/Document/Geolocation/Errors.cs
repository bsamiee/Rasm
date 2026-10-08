namespace Rasm.Rhino.Document.Geolocation;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    OffsetOutOfRange = 1,
    ReversedWindow,
    InvalidStudyStep,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record OffsetOutOfRange(double Hours) : Expected("Sun time zone {Hours} h is outside the offset range", (int)Codes.OffsetOutOfRange);

public sealed record ReversedWindow(LocalDateTime Start, LocalDateTime End) : Expected("Sun study starts at {Start} after its end {End}", (int)Codes.ReversedWindow);

public sealed record InvalidStudyStep(Period Step) : Expected("Sun study step {Step} is no positive count of minutes or days", (int)Codes.InvalidStudyStep);
