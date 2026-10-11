namespace Rasm.Rhino.Persistence.Stores;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    RefusedEntry = 1,
    MissingField,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record RefusedEntry : Expected {
    public RefusedEntry(EntryKey key, string text, Error cause) : base("Entry {Key} refuses text {Text}", (int)Codes.RefusedEntry, cause) => (Key, Text) = (key, text);

    public EntryKey Key { get; }

    public string Text { get; }
}

public sealed record MissingField(Type ValueType) : Expected("No text was stored for {ValueType}", (int)Codes.MissingField);
