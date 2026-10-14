namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    NativeOutputUnavailable = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record NativeOutputUnavailable(Guid Session, Seq<Guid> Effects)
    : Expected("Rendering {Session} cannot reconstruct the requested output around native effects {Effects}", (int)Codes.NativeOutputUnavailable);
