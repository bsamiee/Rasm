using Rasm.Rhino.Document;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record WrongKind(RenderContentKind Required, RenderContentKind Actual) : Expected("Content is {Actual} where {Required} is required", ErrorOps.Code<WrongKind>());

public sealed record Unattached(Guid Content) : Expected("Content {Content} belongs to no document", ErrorOps.Code<Unattached>());

public sealed record SlotRejected(Guid TypeId, string ChildSlotName) : Expected("Type {TypeId} is not acceptable as a child in slot {ChildSlotName}", ErrorOps.Code<SlotRejected>()) {
    public static Fin<Unit> Unless(bool acceptable, Guid typeId, string childSlotName) => acceptable ? unit : new SlotRejected(typeId, childSlotName);
}

public sealed record UnsupportedField(string Field, Type FieldType) : Expected("Field {Field} is a {FieldType}, which no FieldValue case holds", ErrorOps.Code<UnsupportedField>());

public sealed record AlreadyRegistered(string Extension) : Expected("Extension {Extension} already has a serializer", ErrorOps.Code<AlreadyRegistered>()) {
    public static Fin<Unit> Unless(bool registered, string extension) => registered ? unit : new AlreadyRegistered(extension);
}
