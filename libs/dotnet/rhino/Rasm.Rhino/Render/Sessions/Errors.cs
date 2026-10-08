using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    RenderRefused = 1,
    FrameUnformed,
    SceneUntapped,
    OverrideHeld,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record RenderRefused(RenderPipeline.RenderReturnCode ReturnCode) : Expected("Render returned {ReturnCode}", (int)Codes.RenderRefused);

public sealed record FrameUnformed(Guid Session) : Expected("Rendering {Session} holds no formed frame", (int)Codes.FrameUnformed);

public sealed record SceneUntapped(Guid Session) : Expected("Rendering {Session} holds no scene-linear frame", (int)Codes.SceneUntapped);

public sealed record OverrideHeld(uint Document) : Expected("Document {Document} already holds a render override", (int)Codes.OverrideHeld) {
    public static Fin<uint> Unless(bool free, uint document) => free ? document : new OverrideHeld(document);
}
