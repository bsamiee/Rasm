namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record SupportSpace {
    public sealed record PointCase(Point3d Value) : SupportSpace;
    public sealed record CurveCase(Curve Value) : SupportSpace;
    public sealed record SurfaceCase(Surface Value) : SupportSpace;
    public sealed record BrepCase(Brep Value) : SupportSpace;
    public sealed record MeshCase(Mesh Value) : SupportSpace;
    public sealed record PlaneCase(Plane Value) : SupportSpace;
    public sealed record BoxCase(Box Value) : SupportSpace;
    public sealed record BoundsCase(BoundingBox Value) : SupportSpace;
    public sealed record SphereCase(Sphere Value) : SupportSpace;
    public static SupportSpace Point(Point3d value) => new PointCase(Value: value);
    public static SupportSpace Curve(Curve value) => new CurveCase(Value: value);
    public static SupportSpace Surface(Surface value) => new SurfaceCase(Value: value);
    public static SupportSpace Brep(Brep value) => new BrepCase(Value: value);
    public static SupportSpace Mesh(Mesh value) => new MeshCase(Value: value);
    public static SupportSpace Plane(Plane value) => new PlaneCase(Value: value);
    public static SupportSpace Box(Box value) => new BoxCase(Value: value);
    public static SupportSpace Bounds(BoundingBox value) => new BoundsCase(Value: value);
    public static SupportSpace Sphere(Sphere value) => new SphereCase(Value: value);
    internal Fin<ClosestHit> Closest(Point3d sample, Op key) => Switch(
        state: (Sample: sample, Key: key),
        pointCase: static (s, p) => Fin.Succ(ClosestHit.At(target: s.Sample, point: p.Value)),
        curveCase: static (s, c) => GeometryKernel.ClosestOf(geometry: c.Value, target: s.Sample, key: s.Key),
        surfaceCase: static (s, sf) => GeometryKernel.ClosestOf(geometry: sf.Value, target: s.Sample, key: s.Key),
        brepCase: static (s, b) => GeometryKernel.ClosestOf(geometry: b.Value, target: s.Sample, key: s.Key),
        meshCase: static (s, m) => GeometryKernel.ClosestOf(geometry: m.Value, target: s.Sample, key: s.Key),
        planeCase: static (s, p) => GeometryKernel.ClosestOf(geometry: p.Value, target: s.Sample, key: s.Key),
        boxCase: static (s, b) => GeometryKernel.ClosestOf(geometry: b.Value, target: s.Sample, key: s.Key),
        boundsCase: static (s, b) => GeometryKernel.ClosestOf(geometry: b.Value, target: s.Sample, key: s.Key),
        sphereCase: static (s, sphere) => GeometryKernel.ClosestOf(geometry: sphere.Value, target: s.Sample, key: s.Key));
}
