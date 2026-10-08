namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    OpenBoundary = 1,
    VariationalPatchFailed,
    OutOfDomain,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record OpenBoundary(string Member) : Expected("{Member} must join into closed loops", (int)Codes.OpenBoundary) {
    public static Fin<Unit> Unless(bool closed, string member) => closed ? unit : new OpenBoundary(member);
}

public sealed record VariationalPatchFailed(string Reason) : Expected("Variational patch failed with reason {Reason}", (int)Codes.VariationalPatchFailed);

public sealed record OutOfDomain(string Member, double Parameter) : Expected("{Member} parameter {Parameter} is outside the domain", (int)Codes.OutOfDomain) {
    public static Fin<Unit> Unless(Interval domain, double parameter, string member) => domain.IncludesParameter(parameter) ? unit : new OutOfDomain(member, parameter);
}
