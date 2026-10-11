using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    RenderRefused = 1,
    FrameUnformed,
    SceneUntapped,
    OverrideHeld,
    NativeOutputUnavailable,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record RenderRefused(RenderPipeline.RenderReturnCode ReturnCode) : Expected("Render returned {ReturnCode}", (int)Codes.RenderRefused);

public sealed record FrameUnformed(Guid Session) : Expected("Rendering {Session} holds no formed frame", (int)Codes.FrameUnformed);

public sealed record SceneUntapped(Guid Session) : Expected("Rendering {Session} holds no scene-linear frame", (int)Codes.SceneUntapped);

public sealed record NativeOutputUnavailable(Guid Session, Seq<Guid> Effects)
    : Expected("Rendering {Session} cannot reconstruct the requested output around native effects {Effects}", (int)Codes.NativeOutputUnavailable);

public sealed record OverrideHeld(uint Document) : Expected("Document {Document} already holds a render override", (int)Codes.OverrideHeld);
