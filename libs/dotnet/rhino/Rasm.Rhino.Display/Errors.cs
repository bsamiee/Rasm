using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Display;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int UnsupportedSpace = 1900;

    public const int RenderRefused = 1901;

    public const int UnknownEffect = 1902;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnsupportedSpace(ActiveSpace Space) : ValidationFailure("Conduit space filter does not support {Space}", Codes.UnsupportedSpace);

public sealed record RenderRefused(RenderPipeline.RenderReturnCode ReturnCode) : Expected("Render returned {ReturnCode}", Codes.RenderRefused);

public sealed record UnknownEffect(Guid Id) : Expected("Post effect {Id} is not in the collection", Codes.UnknownEffect);
