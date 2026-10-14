using Rasm.Imaging.Quantization;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    RenderRefused = 1,
    FrameUnformed,
    SceneUntapped,
    OverrideHeld,
    MalformedRendering,
    MissingPlane,
    MalformedSidecar,
    NoDitherLevels,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record RenderRefused(RenderPipeline.RenderReturnCode ReturnCode) : Expected("Render returned {ReturnCode}", (int)Codes.RenderRefused);

public sealed record FrameUnformed(Guid Session) : Expected("Rendering {Session} holds no formed frame", (int)Codes.FrameUnformed);

public sealed record SceneUntapped(Guid Session) : Expected("Rendering {Session} holds no scene-linear frame", (int)Codes.SceneUntapped);

public sealed record OverrideHeld(uint Document) : Expected("Document {Document} already holds a render override", (int)Codes.OverrideHeld);

public sealed record MalformedRendering(string Path, RimagePart Part) : Expected("{Path} holds no readable {Part}", (int)Codes.MalformedRendering);

public sealed record MissingPlane(string Path, RenderWindow.StandardChannels Channel) : Expected("{Path} holds no {Channel} plane", (int)Codes.MissingPlane);

public sealed record MalformedSidecar : Expected {
    public MalformedSidecar(string path, Error cause) : base("{Path} holds no readable rendering state", (int)Codes.MalformedSidecar, cause) => Path = path;

    public string Path { get; }
}

public sealed record NoDitherLevels(Dither Dithering) : Expected("{Dithering} holds no levels to dither at a float depth", (int)Codes.NoDitherLevels);
