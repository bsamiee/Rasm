namespace Rasm.Rhino.Document.Geolocation;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    ReversedWindow = 1,
    InvalidStudyStep,
    SkippedLocalTime,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ReversedWindow(LocalDateTime Start, LocalDateTime End) : Expected("Sun study starts at {Start} after its end {End}", (int)Codes.ReversedWindow);

public sealed record InvalidStudyStep(Period Step) : Expected("Sun study step {Step} is no positive count of minutes or days", (int)Codes.InvalidStudyStep);

public sealed record SkippedLocalTime(LocalDateTime Local) : Expected("{Local} falls in a skipped hour of the computer's time zone", (int)Codes.SkippedLocalTime);
