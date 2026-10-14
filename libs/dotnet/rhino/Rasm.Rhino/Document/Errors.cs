namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UndoRecordUnavailable = 1,
    ReversedWindow = 2,
    InvalidStudyStep = 3,
    SkippedLocalTime = 4,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UndoRecordUnavailable() : Expected("Running command holds no undo record", (int)Codes.UndoRecordUnavailable);

public sealed record ReversedWindow(LocalDateTime Start, LocalDateTime End) : Expected("Sun study starts at {Start} after its end {End}", (int)Codes.ReversedWindow);

public sealed record InvalidStudyStep(Period Step) : Expected("Sun study step {Step} is no positive count of minutes or days", (int)Codes.InvalidStudyStep);

public sealed record SkippedLocalTime(LocalDateTime Local) : Expected("{Local} falls in a skipped hour of the computer's time zone", (int)Codes.SkippedLocalTime);
