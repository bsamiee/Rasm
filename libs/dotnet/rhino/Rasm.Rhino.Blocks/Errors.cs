namespace Rasm.Rhino.Blocks;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int DefinitionContained = 1200;
    public const int AlreadyLinked = 1201;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DefinitionContained(Guid Definition, Seq<Guid> Containers) : Expected("Definition {Definition} is nested in definitions {Containers}", Codes.DefinitionContained) {
    public static Fin<Unit> Unless(Guid definition, Seq<Guid> containers) => containers.IsEmpty ? unit : new DefinitionContained(definition, containers);
}

public sealed record AlreadyLinked(string Path, Guid Definition) : Expected("File {Path} is already linked by definition {Definition}", Codes.AlreadyLinked) {
    public static Fin<Unit> Unless(bool created, string path, Guid definition) => created ? unit : new AlreadyLinked(path, definition);
}
