namespace Rasm.Rhino.Render.Effects;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UnknownEffect = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnknownEffect(Guid Id) : Expected("Post effect {Id} is not in the collection", (int)Codes.UnknownEffect);
