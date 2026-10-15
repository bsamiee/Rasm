using Rhino.Render;

namespace Rasm.Rhino.Document.Tables;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UndoRecordOpen = 1,
    InvalidLayerSegment = 2,
    MissingLayerPath = 3,
    LayerCycle = 4,
    ForeignContent = 5,
    LayerPathTaken = 6,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UndoRecordOpen(uint Serial) : Expected("Undo record {Serial} is open", (int)Codes.UndoRecordOpen) {
    public static Fin<Unit> Unless(Option<uint> open) => open.Case is uint serial ? new UndoRecordOpen(serial) : unit;
}

public sealed record InvalidLayerSegment : Expected {
    public InvalidLayerSegment(int segment, Error cause) : base("Layer name at position {Segment} is refused", (int)Codes.InvalidLayerSegment, cause) => Segment = segment;

    public int Segment { get; }
}

public sealed record MissingLayerPath(string Path) : Expected("No layer has the path {Path}", (int)Codes.MissingLayerPath);

public sealed record LayerCycle(Guid Layer, Guid Parent) : Expected("Layer {Parent} is layer {Layer} or sits inside it", (int)Codes.LayerCycle) {
    public static Fin<Unit> Unless(bool acyclic, Guid layer, Guid parent) => acyclic ? unit : new LayerCycle(layer, parent);
}

public sealed record ForeignContent(Guid Content) : Expected("Render content {Content} belongs to another document", (int)Codes.ForeignContent) {
    public static Fin<T> Unless<T>(bool owned, T content) where T : RenderContent => owned ? content : new ForeignContent(content.Id);
}

public sealed record LayerPathTaken(string Path, Guid Occupant) : Expected("Layer path {Path} is held by layer {Occupant}", (int)Codes.LayerPathTaken);
