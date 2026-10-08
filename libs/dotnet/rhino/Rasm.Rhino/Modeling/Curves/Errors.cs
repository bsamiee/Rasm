namespace Rasm.Rhino.Modeling.Curves;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UnorderedParameters = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnorderedParameters(string Member) : Expected("{Member} requires strictly increasing parameters", (int)Codes.UnorderedParameters);
