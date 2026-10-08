namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    DefinitionContained = 1,
    AlreadyLinked,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DefinitionContained(Guid Definition, Seq<Guid> Containers)
    : Expected("Definition {Definition} is nested in definitions {Containers}", (int)Codes.DefinitionContained) {
    public static Fin<Unit> Unless(Guid definition, Seq<Guid> containers) => containers.IsEmpty ? unit : new DefinitionContained(definition, containers);
}

public sealed record AlreadyLinked(string Path, Guid Definition)
    : Expected("File {Path} is already linked by definition {Definition}", (int)Codes.AlreadyLinked) {
    public static Fin<Unit> Unless(bool created, string path, Guid definition) => created ? unit : new AlreadyLinked(path, definition);
}
