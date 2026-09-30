namespace Rasm.Rhino.Persistence;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int ReadOnlyKey = 1800;

    public const int TypeMismatch = 1801;

    public const int UnreadableVersion = 1802;

    public const int ArchiveFault = 1803;

    public const int NotAttachable = 1804;

    public const int ArchiveRejected = 1805;

    public const int FontUnavailable = 1806;

    public const int TranslucentColor = 1807;

    public const int UnknownThemeKey = 1808;

    public const int OddPadding = 1809;

    public const int UnrestorableEdit = 1810;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ReadOnlyKey(string Key) : Expected("Key {Key} is read-only", Codes.ReadOnlyKey) {
    public static Fin<Unit> Unless(bool writable, string key) => writable ? unit : new ReadOnlyKey(key);
}

public sealed record TypeMismatch(string Key, object Stored, Type Requested) : Expected("Key {Key} holds {Stored}, which is not a {Requested}", Codes.TypeMismatch);

public sealed record UnreadableVersion(uint TypeCode, int Major, int Minor) : Expected("Chunk {TypeCode} has version {Major}.{Minor}, which the reader rejects", Codes.UnreadableVersion);

public sealed record ArchiveFault(string Member, bool ErrorOccurred, Error Cause)
    : Expected("{Member} threw BinaryArchiveException with ErrorOccurred {ErrorOccurred}", Codes.ArchiveFault, Some(Cause));

public sealed record NotAttachable(Type UserDataType, Error Cause)
    : Expected("{UserDataType} is not a public class with a public parameterless constructor", Codes.NotAttachable, Some(Cause));

public sealed record ArchiveRejected(string Member, string Log) : Expected("{Member} rejected the archive: {Log}", Codes.ArchiveRejected);

public sealed record FontUnavailable(string Family, Error Cause) : Expected("Eto allocates no font of family {Family}", Codes.FontUnavailable, Some(Cause));

public sealed record TranslucentColor(string Member, byte Alpha) : Expected("{Member} drops alpha {Alpha} at the write", Codes.TranslucentColor) {
    public static Fin<Unit> Unless(bool opaque, string member, byte alpha) => opaque ? unit : new TranslucentColor(member, alpha);
}

public sealed record UnknownThemeKey(string Key) : Expected("No theme zone enumerates {Key}", Codes.UnknownThemeKey) {
    public static Fin<Unit> Unless(bool known, string key) => known ? unit : new UnknownThemeKey(key);
}

public sealed record OddPadding(int Row, int Glyph) : Expected("Row {Row} and glyph {Glyph} leave no whole padding", Codes.OddPadding) {
    public static Fin<Unit> Unless(bool even, int row, int glyph) => even ? unit : new OddPadding(row, glyph);
}

public sealed record UnrestorableEdit(string Property) : Expected("A layer state restore does not revert {Property}", Codes.UnrestorableEdit) {
    public static Fin<Unit> Unless(bool restorable, string property) => restorable ? unit : new UnrestorableEdit(property);
}
