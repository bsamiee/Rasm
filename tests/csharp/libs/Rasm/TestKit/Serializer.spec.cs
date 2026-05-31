using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.TestKit;

// --- [LAWS] ---------------------------------------------------------------------------------
public sealed class GeometrySerializerLaws {
    private static readonly GeometrySerializer Serializer = new();

    [Fact]
    public void SupportIsLimitedToDeclaredGeometryValues() {
        (string Label, Func<bool> Probe, bool Expected)[] rows = [
            ("Point3d", static () => Serializer.IsSerializable(type: typeof(Point3d), value: Point3d.Origin, failureReason: out _), true),
            ("Vector3d", static () => Serializer.IsSerializable(type: typeof(Vector3d), value: Vector3d.XAxis, failureReason: out _), true),
            ("BoundingBox", static () => Serializer.IsSerializable(type: typeof(BoundingBox), value: BoundingBox.Empty, failureReason: out _), true),
            ("Plane", static () => Serializer.IsSerializable(type: typeof(Plane), value: Plane.WorldXY, failureReason: out _), false),
        ];
        Spec.SupportMatrix(rows: rows);
    }

    [Fact]
    public void PointRoundtripPreservesCoordinates() =>
        Spec.ForAll(gen: Gens.Point, property: static point => {
            object actual = Serializer.Deserialize(type: typeof(Point3d), serializedValue: Serializer.Serialize(value: point));
            Spec.Equal(left: Assert.IsType<Point3d>(@object: actual), right: point, what: "point serializer");
        });

    [Fact]
    public void VectorRoundtripPreservesCoordinates() =>
        Spec.ForAll(gen: Gens.Vec, property: static vector => {
            object actual = Serializer.Deserialize(type: typeof(Vector3d), serializedValue: Serializer.Serialize(value: vector));
            Spec.Equal(left: Assert.IsType<Vector3d>(@object: actual), right: vector, what: "vector serializer");
        });

    [Fact]
    public void BoundingBoxRoundtripPreservesCorners() =>
        Spec.ForAll(gen: Gens.NonEmptyBbox, property: static box => {
            BoundingBox actual = Assert.IsType<BoundingBox>(@object: Serializer.Deserialize(type: typeof(BoundingBox), serializedValue: Serializer.Serialize(value: box)));
            Spec.Equal(left: actual.Min, right: box.Min, what: "bounding box min");
            Spec.Equal(left: actual.Max, right: box.Max, what: "bounding box max");
        });

    [Fact]
    public void MalformedComponentsFailAsFormatErrors() =>
        _ = Assert.Throws<FormatException>(testCode: static () => Serializer.Deserialize(type: typeof(Point3d), serializedValue: "1;NaNish;3"));
}
