using Rhino.DocObjects;

namespace Rasm.Rhino.Modeling;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    OpenBoundary = 1,
    VariationalPatchFailed = 2,
    OutOfDomain = 3,
    InvalidGeometry = 4,
    InvalidGeometryElement = 5,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record OpenBoundary(string Member) : Expected("{Member} must join into closed loops", (int)Codes.OpenBoundary) {
    public static Fin<Unit> Unless(bool closed, string member) => closed ? unit : new OpenBoundary(member);
}

public sealed record VariationalPatchFailed(string Reason) : Expected("Variational patch failed with reason {Reason}", (int)Codes.VariationalPatchFailed);

public sealed record OutOfDomain(string Member, double Parameter) : Expected("{Member} parameter {Parameter} is outside the domain", (int)Codes.OutOfDomain) {
    public static Fin<double> Unless(Interval domain, double parameter, string member) => domain.IncludesParameter(parameter) ? parameter : new OutOfDomain(member, parameter);
}

public sealed record InvalidGeometry(string Member, ObjectType ObjectType, string Log) : Expected("{Member} answered an invalid {ObjectType}: {Log}", (int)Codes.InvalidGeometry);

public sealed record InvalidGeometryElement(string Member, int Index, ObjectType ObjectType, string Log) : Expected("{Member} answered an invalid {ObjectType} at index {Index}: {Log}", (int)Codes.InvalidGeometryElement);
