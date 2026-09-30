using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int WrongKind = 1300;

    public const int Unattached = 1301;

    public const int SlotRejected = 1302;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record WrongKind(RenderContentKind Required, RenderContentKind Actual) : Expected("Content is {Actual} where {Required} is required", Codes.WrongKind);

public sealed record Unattached(Guid Content) : Expected("Content {Content} belongs to no document", Codes.Unattached);

public sealed record SlotRejected(Guid TypeId, string ChildSlotName) : Expected("Type {TypeId} is not acceptable as a child in slot {ChildSlotName}", Codes.SlotRejected) {
    public static Fin<Unit> Unless(bool acceptable, Guid typeId, string childSlotName) => acceptable ? unit : new SlotRejected(typeId, childSlotName);
}
