namespace Rasm.Rhino.Document.Notation;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UnparsedText = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnparsedText(Type Owner, string Member, string Text) : Expected("{Owner}.{Member} parsed no whole value from {Text}", (int)Codes.UnparsedText);
