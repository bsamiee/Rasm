using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    NativeOutputUnavailable = 1,
    EmptySlot = 2,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record NativeOutputUnavailable(Guid Session, Seq<Guid> Effects)
    : Expected("Rendering {Session} cannot reconstruct the requested output around native effects {Effects}", (int)Codes.NativeOutputUnavailable);

public sealed record EmptySlot(Guid Parent, string ChildSlotName) : Expected("Content {Parent} holds no child in slot {ChildSlotName}", (int)Codes.EmptySlot) {
    public static Fin<RenderContent> Unless(RenderContent? child, Guid parent, string childSlotName) => child is { } found ? found : new EmptySlot(parent, childSlotName);
}
