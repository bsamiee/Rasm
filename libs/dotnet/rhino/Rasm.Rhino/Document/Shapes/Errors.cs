using Rhino.DocObjects;

namespace Rasm.Rhino.Document.Shapes;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum Codes {
    InvalidGeometry = 1,
    InvalidGeometryElement,
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record InvalidGeometry(string Member, ObjectType ObjectType, string Log) : Expected("{Member} answered an invalid {ObjectType}: {Log}", (int)Codes.InvalidGeometry);

public sealed record InvalidGeometryElement(string Member, int Index, ObjectType ObjectType, string Log) : Expected("{Member} answered an invalid {ObjectType} at index {Index}: {Log}", (int)Codes.InvalidGeometryElement);
