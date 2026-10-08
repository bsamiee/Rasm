namespace Rasm.Rhino.Modeling.Curves;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    UnorderedParameters = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record UnorderedParameters() : Expected($"{nameof(Curve.GetPerpendicularFrames)} requires strictly increasing parameters", (int)Codes.UnorderedParameters);
