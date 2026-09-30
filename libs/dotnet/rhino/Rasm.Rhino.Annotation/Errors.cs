namespace Rasm.Rhino.Annotation;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int MissingMember = 1700;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record MissingMember(Type Subject, string Member) : Expected("{Subject} has no {Member}", Codes.MissingMember);
