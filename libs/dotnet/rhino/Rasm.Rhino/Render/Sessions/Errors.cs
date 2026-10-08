using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    RenderRefused = 1,
    OverrideHeld,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record RenderRefused(RenderPipeline.RenderReturnCode ReturnCode) : Expected("Render returned {ReturnCode}", (int)Codes.RenderRefused);

public sealed record OverrideHeld(uint Document) : Expected("Document {Document} already holds a render override", (int)Codes.OverrideHeld) {
    public static Fin<uint> Unless(bool free, uint document) => free ? document : new OverrideHeld(document);
}
