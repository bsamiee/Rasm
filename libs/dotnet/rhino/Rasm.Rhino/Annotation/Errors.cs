namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    MissingMember = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record MissingMember(Type Subject, string Member) : Expected("{Subject} has no {Member}", (int)Codes.MissingMember);
