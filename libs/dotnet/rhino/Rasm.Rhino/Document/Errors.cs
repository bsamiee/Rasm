namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UndoRecordUnavailable = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UndoRecordUnavailable() : Expected("Running command holds no undo record", (int)Codes.UndoRecordUnavailable);
