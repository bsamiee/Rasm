namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    CyclicDefinitions = 1,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record CyclicDefinitions(Seq<Seq<Guid>> Components)
    : Expected("Definitions {Components} contain cycles", (int)Codes.CyclicDefinitions);
