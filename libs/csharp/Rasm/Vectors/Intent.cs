namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = new(key: 0), Direction = new(key: 1), Span = new(key: 2), Normal = new(key: 3), Distance = new(key: 4);
    public static readonly SupportProjection Parameter = new(key: 5), Uv = new(key: 6), Component = new(key: 7), MeshPoint = new(key: 8);
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record VectorIntent {
    public sealed record BetweenCase(Point3d Origin, SupportSpace Target, BoundarySense Sense) : VectorIntent;
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Frame) : VectorIntent;
    public sealed record AngularCase(Direction A, Direction B, Option<Plane> Frame) : VectorIntent;
    public sealed record SupportCase(SupportSpace Space, Point3d Sample, SupportProjection Projection) : VectorIntent;
    public sealed record FieldCase(VectorField Value, Point3d Sample) : VectorIntent;
    public sealed record RayCase(Point3d Origin, Direction Direction, RayPolicy Policy) : VectorIntent;
    public static VectorIntent Between(Point3d origin, SupportSpace target, BoundarySense? sense = null) =>
        new BetweenCase(Origin: origin, Target: target, Sense: sense ?? BoundarySense.Toward);
    public static VectorIntent Axis(SignedAxis axis, Plane? frame = null) =>
        new AxisCase(Value: axis, Frame: Optional(frame));
    public static VectorIntent Angular(Direction a, Direction b, Plane? frame = null) =>
        new AngularCase(A: a, B: b, Frame: Optional(frame));
    public static VectorIntent Support(SupportSpace space, Point3d sample, SupportProjection projection) =>
        new SupportCase(Space: space, Sample: sample, Projection: projection);
    public static VectorIntent Field(VectorField field, Point3d sample) =>
        new FieldCase(Value: field, Sample: sample);
    public static VectorIntent Ray(Point3d origin, Direction direction, RayPolicy? policy = null) =>
        new RayCase(Origin: origin, Direction: direction, Policy: policy ?? RayPolicy.Forward);
}
