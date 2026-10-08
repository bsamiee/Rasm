namespace Rasm.Rhino.Document.Shapes;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate bool PrincipalRead(out double x, out Vector3d xAxis, out double y, out Vector3d yAxis, out double z, out Vector3d zAxis);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrincipalMoments(double X, Vector3d XAxis, double Y, Vector3d YAxis, double Z, Vector3d ZAxis) {
    public static Option<PrincipalMoments> Of(PrincipalRead read) =>
        Callbacks.Found(read(out double x, out Vector3d xAxis, out double y, out Vector3d yAxis, out double z, out Vector3d zAxis), new PrincipalMoments(x, xAxis, y, yAxis, z, zAxis));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Measurements {
    // --- [VALIDITY]
    public static Fin<T> Valid<T>(T geometry, string member) where T : GeometryBase =>
        geometry.IsValidWithLog(out string log) ? geometry : new InvalidGeometry(member, geometry.ObjectType, log);

    public static Fin<Seq<T>> Valid<T>(Seq<T?> geometries, string member) where T : GeometryBase =>
        Callbacks.Each(geometries, (geometry, index) => geometry is { } present ? Element(present, member, index) : new RefusedElement(member, index));

    public static Fin<Seq<Option<T>>> Valid<T>(Seq<Option<T>> geometries, string member) where T : GeometryBase =>
        Callbacks.Each(geometries, (geometry, index) => geometry.Traverse(present => Element(present, member, index)).As());

    private static Fin<T> Element<T>(T geometry, string member, int index) where T : GeometryBase =>
        geometry.IsValidWithLog(out string log) ? geometry : new InvalidGeometryElement(member, index, geometry.ObjectType, log);

    // --- [PARTS]
    public static uint Checksum<T>(Seq<T> parts) where T : GeometryBase =>
        parts.Fold(0u, static (remainder, part) => part.DataCRC(remainder));

    public static Option<BoundingBox> Bounds(Seq<BoundingBox> boxes) =>
        Conversions.Present(boxes.Fold(BoundingBox.Empty, BoundingBox.Union));
}
