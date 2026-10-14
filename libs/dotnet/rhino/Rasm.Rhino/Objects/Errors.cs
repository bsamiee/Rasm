namespace Rasm.Rhino.Objects;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidPlane = 1,
    InvalidLight = 2,
    MissingHistoryInput = 3,
    StaleHistory = 4,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidPlane(string Member) : Expected("{Member} received a plane that is not valid", (int)Codes.InvalidPlane);

public sealed record InvalidLight : Expected {
    public InvalidLight(Guid light, Error cause) : base("Light {Light} holds a value outside its type", (int)Codes.InvalidLight, cause) => Light = light;

    public Guid Light { get; }
}

public sealed record MissingHistoryInput(int Id) : Expected("History record holds no input under id {Id}", (int)Codes.MissingHistoryInput) {
    public static Fin<T> Unless<T>(bool found, T value, int id) => found ? value : new MissingHistoryInput(id);
}

public sealed record StaleHistory(int Recorded, int Current) : Expected("History record version {Recorded} differs from command version {Current}", (int)Codes.StaleHistory);
