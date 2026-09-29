using Rasm.Rhino.Document;

namespace Rasm.Rhino.Persistence;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ReadOnlyKey(string Key) : Expected("Key {Key} is read-only", ErrorOps.Code<ReadOnlyKey>()) {
    public static Fin<Unit> Unless(bool writable, string key) => writable ? unit : new ReadOnlyKey(key);
}

public sealed record TypeMismatch(string Key, Type Stored, Type Requested) : Expected("Key {Key} holds {Stored} where {Requested} was requested", ErrorOps.Code<TypeMismatch>());

public sealed record UnreadableVersion(uint TypeCode, int Major, int Minor) : Expected("Chunk {TypeCode} has version {Major}.{Minor}, which the reader rejects", ErrorOps.Code<UnreadableVersion>());

public sealed record ArchiveFault(string Member, bool ErrorOccurred) : Expected("{Member} threw BinaryArchiveException with ErrorOccurred {ErrorOccurred}", ErrorOps.Code<ArchiveFault>());

public sealed record NotAttachable(Type UserDataType) : Expected("{UserDataType} is not a public class with a public parameterless constructor", ErrorOps.Code<NotAttachable>());

public sealed record ArchiveRejected(string Member, string Log) : Expected("{Member} rejected the archive: {Log}", ErrorOps.Code<ArchiveRejected>());

public sealed record DuplicateTarget(string Target, int Matched) : Expected("{Matched} embedded files write the same target {Target}", ErrorOps.Code<DuplicateTarget>());

public sealed record FontUnavailable(string Family) : Expected("Eto allocates no font of family {Family}", ErrorOps.Code<FontUnavailable>());

public sealed record TranslucentColor(string Member, byte Alpha) : Expected("{Member} drops alpha {Alpha} at the write", ErrorOps.Code<TranslucentColor>()) {
    public static Fin<Unit> Unless(bool opaque, string member, byte alpha) => opaque ? unit : new TranslucentColor(member, alpha);
}

public sealed record UnknownThemeKey(string Key) : Expected("No theme zone enumerates {Key}", ErrorOps.Code<UnknownThemeKey>()) {
    public static Fin<Unit> Unless(bool known, string key) => known ? unit : new UnknownThemeKey(key);
}

public sealed record OddPadding(int Row, int Glyph) : Expected("Row {Row} and glyph {Glyph} leave no whole padding", ErrorOps.Code<OddPadding>()) {
    public static Fin<Unit> Unless(bool even, int row, int glyph) => even ? unit : new OddPadding(row, glyph);
}
