using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record SupportSpace {
    private SupportSpace(object value) => Value = value;
    internal object Value { get; }
    internal Type SourceType => Value.GetType();
    internal bool CanClosestNormal => GeometryKernel.CanClosestNormal(type: SourceType);
    internal bool CanSignedDistance => GeometryKernel.CanSignedDistance(type: SourceType);
    internal bool AdmitsNormal(ClosestHit hit) =>
        CanClosestNormal && hit.Normal.IsSome;
    internal bool AdmitsTangent(ClosestHit hit) =>
        GeometryKernel.CanClosestTangent(type: SourceType) && hit.Tangent.IsSome;
    internal bool AdmitsFrame(ClosestHit hit) =>
        GeometryKernel.CanClosestFrame(type: SourceType) && hit.Frame.IsSome;
    internal bool AdmitsSignedDistance(ClosestHit hit) =>
        Value switch {
            Plane or Sphere or Box or BoundingBox => hit.Distance.IsSome,
            _ => CanSignedDistance && hit.Normal.IsSome,
        };
    internal bool AdmitsContainmentDistance(ClosestHit hit) =>
        Value switch {
            Brep { IsSolid: true } or Mesh { IsSolid: true } => hit.Distance.IsSome,
            Brep or Mesh => false,
            _ => AdmitsSignedDistance(hit: hit),
        };
    public static Fin<SupportSpace> Of(object? value, Op? key = null) {
        Op op = key.OrDefault();
        return value switch {
            VectorCloud.ClusterCase cluster => Fin.Succ(new SupportSpace(value: cluster)),
            _ => from source in Optional(value).ToFin(op.InvalidInput())
                 let type = source.GetType()
                 from _ in guard(type != typeof(object) && type != typeof(GeometryBase) && GeometryKernel.CanClosest(type: type), op.Unsupported(type, typeof(ClosestHit)))
                 select new SupportSpace(value: source),
        };
    }
    internal Fin<ClosestHit> Closest(Point3d sample, Op key) =>
        Value switch {
            VectorCloud.ClusterCase cluster => cluster.ClosestVertex(sample: sample, key: key),
            _ => GeometryKernel.ClosestOf(geometry: Value, target: sample, key: key),
        };
    internal Fin<double> SignedDistance(ClosestHit hit, Point3d sample, Op key) =>
        GeometryKernel.SignedDistanceOf(geometry: Value, hit: hit, sample: sample, key: key);
    internal Fin<double> ContainmentDistance(ClosestHit hit, Point3d sample, Context context, Op key) =>
        Value switch {
            Brep { IsSolid: true } brep => hit.Distance.ToFin(Fail: key.InvalidResult())
                .Map(distance => (brep.IsPointInside(sample, context.Absolute.Value, strictlyIn: false) ? -1.0 : 1.0) * distance),
            Mesh { IsSolid: true } mesh => hit.Distance.ToFin(Fail: key.InvalidResult())
                .Map(distance => (mesh.IsPointInside(sample, context.Absolute.Value, strictlyIn: false) ? -1.0 : 1.0) * distance),
            Brep or Mesh => Fin.Fail<double>(error: key.InvalidInput()),
            _ => SignedDistance(hit: hit, sample: sample, key: key),
        };
}
