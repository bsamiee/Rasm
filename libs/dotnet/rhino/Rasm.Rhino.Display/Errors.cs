using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Display;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnsupportedSpace(ActiveSpace Space) : ValidationFailure("Conduit space filter does not support {Space}", ErrorOps.Code<UnsupportedSpace>());

public sealed record RenderRefused(RenderPipeline.RenderReturnCode ReturnCode) : Expected("Render returned {ReturnCode}", ErrorOps.Code<RenderRefused>());

public sealed record UnsupportedMark(WorldMark Mark) : Expected("{Mark} has no retained display form", ErrorOps.Code<UnsupportedMark>());

public sealed record UnknownEffect(Guid Id) : Expected("Post effect {Id} is not in the collection", ErrorOps.Code<UnknownEffect>());
