using Rhino.Render;

namespace Rasm.Rhino.Render.Content;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    Unattached = 1,
    SlotRejected,
    EmptySlot,
    ContentFileFailed,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record Unattached(Guid Content) : Expected("Content {Content} belongs to no document", (int)Codes.Unattached) {
    public static Fin<Unit> Unless(bool attached, Guid content) => attached ? unit : new Unattached(content);
}

public sealed record SlotRejected(Guid TypeId, string ChildSlotName) : Expected("Slot {ChildSlotName} does not accept content type {TypeId}", (int)Codes.SlotRejected) {
    public static Fin<Unit> Unless(bool accepted, Guid typeId, string childSlotName) => accepted ? unit : new SlotRejected(typeId, childSlotName);
}

public sealed record EmptySlot(Guid Parent, string ChildSlotName) : Expected("Content {Parent} holds no child in slot {ChildSlotName}", (int)Codes.EmptySlot) {
    public static Fin<RenderContent> Unless(RenderContent? child, Guid parent, string childSlotName) => child is { } found ? found : new EmptySlot(parent, childSlotName);
}

public sealed record ContentFileFailed : Expected {
    public ContentFileFailed(string path, Error cause) : base("{Path} failed", (int)Codes.ContentFileFailed, cause) => Path = path;

    public string Path { get; }
}
