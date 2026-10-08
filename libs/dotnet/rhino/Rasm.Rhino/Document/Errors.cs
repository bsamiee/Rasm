using Rhino.Display;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UndoRecordUnavailable = 1,
    NoActiveView,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UndoRecordUnavailable() : Expected("Running command holds no undo record", (int)Codes.UndoRecordUnavailable);

public sealed record NoActiveView() : Expected("Document has no active view", (int)Codes.NoActiveView) {
    public static Fin<RhinoView> Unless(RhinoView? view) => view is { } active ? active : new NoActiveView();
}
