namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    TypeMismatch = 1,
    UnreadText,
    UnstorableKind,
    UnreadableRecord,
    ArchiveRejected,
    ArchiveFault,
    NotAttachable,
    UnrestorableEdit,
    UnknownSnapshot,
    SnapshotNotApplied,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record TypeMismatch(string Key, Type Stored, Type Requested) : Expected("Key {Key} holds a {Stored} where a {Requested} is read", (int)Codes.TypeMismatch) {
    public static Fin<T> Unless<T>(object value, string key) where T : notnull => value is T typed ? typed : new TypeMismatch(key, value.GetType(), typeof(T));
}

public sealed record UnreadText(string Text, Type Requested, Option<Error> Inner = default) : Expected("Text {Text} reads as no {Requested}", (int)Codes.UnreadText, Inner);

public sealed record UnstorableKind(string Key, Type Kind) : Expected("ArchivableDictionary.Set takes no {Kind} for key {Key}", (int)Codes.UnstorableKind);

public sealed record UnreadableRecord(string Name, int Version, int Supported) : Expected("Record {Name} has version {Version}, newer than version {Supported}", (int)Codes.UnreadableRecord);

public sealed record ArchiveRejected(string Member, string Log) : Expected("{Member} rejected the archive: {Log}", (int)Codes.ArchiveRejected);

public sealed record ArchiveFault : Expected {
    public ArchiveFault(string member, Error cause) : base("{Member} threw BinaryArchiveException", (int)Codes.ArchiveFault, cause) => Member = member;

    public string Member { get; }
}

public sealed record NotAttachable : Expected {
    public NotAttachable(Type userDataType, Error cause) : base("{UserDataType} is not a public top-level class", (int)Codes.NotAttachable, cause) => UserDataType = userDataType;

    public Type UserDataType { get; }
}

public sealed record UnrestorableEdit(string Member) : Expected("A layer state restore does not revert {Member}", (int)Codes.UnrestorableEdit) {
    public static Fin<Unit> Unless(bool restorable, string member) => restorable ? unit : new UnrestorableEdit(member);
}

public sealed record UnknownSnapshot(SnapshotName Name) : Expected("No snapshot is named {Name}", (int)Codes.UnknownSnapshot);

public sealed record SnapshotNotApplied(SnapshotChange Change) : Expected("{Change} left the snapshot names unchanged", (int)Codes.SnapshotNotApplied) {
    public static Fin<Unit> Unless(bool applied, SnapshotChange change) => applied ? unit : new SnapshotNotApplied(change);
}
