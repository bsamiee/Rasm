using Grasshopper2.Doc;
using Grasshopper2.Framework;

namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    PluginRefused = 1,
    RefusedDataType,
    DocumentNotFound,
    DocumentDamaged,
    ClusterRefused,
    InvalidWire,
    WireAbsent,
    WirePresent,
    SolutionUnfinished,
    InvalidGrasshopperValue,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record PluginRefused : Expected {
    public PluginRefused(string location, FailureKind reason, Option<Error> cause) : base("{Location} failed to load with {Reason}", (int)Codes.PluginRefused, cause) =>
        (Location, Reason) = (location, reason);

    public string Location { get; }
    public FailureKind Reason { get; }

    public static Fin<Unit> Unless(bool loaded, FailureInfo failure) =>
        loaded ? unit : new PluginRefused(failure.Location, failure.Kind, Optional(failure.Exception).Map(static exception => New(exception)));
}

public sealed record RefusedDataType(Type DataType, string Member) : Expected("{Member} refused the data type {DataType}", (int)Codes.RefusedDataType) {
    public static Fin<Unit> Unless(bool accepted, Type dataType, string member) => accepted ? unit : new RefusedDataType(dataType, member);
}

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

public sealed record InvalidGrasshopperValue() : Expected("value out of range", (int)Codes.InvalidGrasshopperValue), IValidationError<InvalidGrasshopperValue> {
    public static InvalidGrasshopperValue Create(string message) => new();
}
