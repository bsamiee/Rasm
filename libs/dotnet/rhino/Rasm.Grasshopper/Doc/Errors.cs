using Grasshopper2.Doc;

namespace Rasm.Grasshopper.Doc;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    DocumentNotFound = 1,
    DocumentDamaged,
    ClusterRefused,
    InvalidWire,
    WireAbsent,
    WirePresent,
    SolutionUnfinished,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DocumentNotFound : Expected {
    public DocumentNotFound(string path, Error cause) : base("{Path} was not found", (int)Codes.DocumentNotFound, cause) => Path = path;

    public string Path { get; }
}

public sealed record DocumentDamaged : Expected {
    public DocumentDamaged(string path, Error cause) : base("{Path} is damaged and cannot be read", (int)Codes.DocumentDamaged, cause) => Path = path;

    public string Path { get; }
}

public sealed record ClusterRefused(GraphTopology Topology) : Expected("{Topology} objects form no cluster", (int)Codes.ClusterRefused);

public sealed record InvalidWire(WireEnds Wire) : Expected("{Wire} is not a valid wire", (int)Codes.InvalidWire);

public sealed record WireAbsent(WireEnds Wire) : Expected("{Wire} is not connected", (int)Codes.WireAbsent) {
    public static Fin<Unit> Unless(bool connected, WireEnds wire) => connected ? unit : new WireAbsent(wire);
}

public sealed record WirePresent(WireEnds Wire) : Expected("{Wire} is already connected", (int)Codes.WirePresent) {
    public static Fin<Unit> Unless(bool absent, WireEnds wire) => absent ? unit : new WirePresent(wire);
}

public sealed record SolutionUnfinished(SolutionId Id, SolutionPhase Culmination) : Expected("Solution {Id} ended {Culmination}", (int)Codes.SolutionUnfinished);
