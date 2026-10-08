namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidPlane = 1,
    InvalidLight,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidPlane(string Member) : Expected("{Member} received a plane that is not valid", (int)Codes.InvalidPlane) {
    public static Fin<Plane> Unless(Plane plane, string member) => plane.IsValid ? plane : new InvalidPlane(member);
}

public sealed record InvalidLight : Expected {
    public InvalidLight(Guid light, Error cause) : base("Light {Light} holds a value outside its type", (int)Codes.InvalidLight, cause) => Light = light;

    public Guid Light { get; }
}
