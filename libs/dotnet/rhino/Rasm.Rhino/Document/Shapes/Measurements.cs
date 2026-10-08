namespace Rasm.Rhino.Document.Shapes;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate bool PrincipalRead(out double x, out Vector3d xAxis, out double y, out Vector3d yAxis, out double z, out Vector3d zAxis);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrincipalMoments(double X, Vector3d XAxis, double Y, Vector3d YAxis, double Z, Vector3d ZAxis) {
    public static IO<PrincipalMoments> Area(Seq<GeometryBase> parts) =>
        Of(() => AreaMassProperties.Compute(parts), static mass => mass.CentroidCoordinatesPrincipalMomentsOfInertia);

    public static IO<PrincipalMoments> Volume(Seq<GeometryBase> parts) =>
        Of(() => VolumeMassProperties.Compute(parts), static mass => mass.CentroidCoordinatesPrincipalMomentsOfInertia);

    public static IO<PrincipalMoments> Length(Seq<Curve> parts) =>
        Of(() => LengthMassProperties.Compute(parts), static mass => mass.CentroidCoordinatesPrincipalMomentsOfInertia);

    private static IO<PrincipalMoments> Of<TMass>(Func<TMass?> compute, Func<TMass, PrincipalRead> read) where TMass : class, IDisposable =>
        use(IO.lift(() => Missing.Unless(compute(), nameof(AreaMassProperties.Compute))))
            .Bind(mass => IO.lift(() => Refused.Unless(
                read(mass)(out double x, out Vector3d xAxis, out double y, out Vector3d yAxis, out double z, out Vector3d zAxis),
                new PrincipalMoments(x, xAxis, y, yAxis, z, zAxis),
                nameof(AreaMassProperties.CentroidCoordinatesPrincipalMomentsOfInertia))))
            .Bracket();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Measurements {
    // --- [CHANGE]
    public static uint Checksum<T>(Seq<T> parts) where T : GeometryBase =>
        parts.Fold(0u, static (remainder, part) => part.DataCRC(remainder));

    // --- [VALIDITY]
    public static Fin<T> Valid<T>(T geometry, string member) where T : GeometryBase =>
        geometry.IsValidWithLog(out string log) ? geometry : new InvalidGeometry(member, geometry.ObjectType, log);

    public static Fin<Seq<T>> Valid<T>(Seq<T?> geometries, string member) where T : GeometryBase =>
        Callbacks.Each(geometries, (geometry, index) => geometry is { } present ? Element(present, member, index) : new RefusedElement(member, index));

    public static Fin<Seq<Option<T>>> Valid<T>(Seq<Option<T>> geometries, string member) where T : GeometryBase =>
        Callbacks.Each(geometries, (geometry, index) => geometry.Traverse(present => Element(present, member, index)).As());

    private static Fin<T> Element<T>(T geometry, string member, int index) where T : GeometryBase =>
        geometry.IsValidWithLog(out string log) ? geometry : new InvalidGeometryElement(member, index, geometry.ObjectType, log);

    // --- [BOUNDS]
    public static Option<BoundingBox> Bounds<T>(Seq<T> parts, Func<T, BoundingBox> measure) =>
        Conversions.Present(parts.Map(measure).Fold(BoundingBox.Empty, BoundingBox.Union));
}
