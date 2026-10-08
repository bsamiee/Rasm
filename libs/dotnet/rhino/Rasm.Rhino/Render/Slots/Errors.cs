using Rhino.Render;

namespace Rasm.Rhino.Render.Slots;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    MalformedRendering = 1,
    MissingPlane,
    MalformedSidecar,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record MalformedRendering(string Path, RimagePart Part) : Expected("{Path} holds no readable {Part}", (int)Codes.MalformedRendering);

public sealed record MissingPlane(string Path, RenderWindow.StandardChannels Channel) : Expected("{Path} holds no {Channel} plane", (int)Codes.MissingPlane);

public sealed record MalformedSidecar : Expected {
    public MalformedSidecar(string path, Error cause) : base("{Path} holds no readable slot state", (int)Codes.MalformedSidecar, cause) => Path = path;

    public string Path { get; }
}
