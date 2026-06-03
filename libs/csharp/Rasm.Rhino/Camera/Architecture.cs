namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ArchitecturalStyle {
    public static readonly ArchitecturalStyle
        TwoPointElevation = new(key: 0, build: BuildTwoPoint),
        ParallelPlan = new(key: 1, build: BuildParallelPlan),
        ParallelAxonometric = new(key: 2, build: BuildAxon);

    [UseDelegateFromConstructor] internal partial Fin<Seq<CameraEdit>> Build(CameraSubject subject, Option<Vector3d> lockedUp);

    private static Fin<Seq<CameraEdit>> BuildTwoPoint(CameraSubject subject, Option<Vector3d> lockedUp) =>
        subject.BoundsOf(op: Op.Of(name: nameof(BuildTwoPoint))).Map(bounds => {
            Point3d center = bounds.Center;
            double distance = bounds.Diagonal.Length;
            Vector3d up = lockedUp.IfNone(Vector3d.ZAxis);
            return Seq<CameraEdit>(
                new CameraEdit.Location(Value: center + (Vector3d.XAxis * distance), UpdateTarget: false),
                new CameraEdit.Target(Value: center, UpdateLocation: false),
                new CameraEdit.Project(Projection: new CameraProjection.TwoPointPerspective(
                    LensLength: CameraDefaults.LensLength, Target: Some(value: (Up: up, TargetDistance: distance)))),
                new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric));
        });

    // the up-lock (2nd param) is intentionally discarded for plan/axon: a parallel view's up follows from the
    // Location->Target direction; only the TwoPointPerspective elevation threads a locked up (HIGH-3).
    private static Fin<Seq<CameraEdit>> BuildParallelPlan(CameraSubject subject, Option<Vector3d> _) =>
        subject.BoundsOf(op: Op.Of(name: nameof(BuildParallelPlan))).Map(bounds => Seq<CameraEdit>(
            new CameraEdit.Location(Value: bounds.Center + (Vector3d.ZAxis * (bounds.Diagonal.Length * 1.5)), UpdateTarget: false),
            new CameraEdit.Target(Value: bounds.Center, UpdateLocation: false),
            new CameraEdit.Project(Projection: new CameraProjection.Parallel(SymmetricFrustum: true)),
            new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric)));

    private static Fin<Seq<CameraEdit>> BuildAxon(CameraSubject subject, Option<Vector3d> _) =>
        subject.BoundsOf(op: Op.Of(name: nameof(BuildAxon))).Map(bounds => {
            Vector3d iso = new Vector3d(x: 1.0, y: 1.0, z: 1.0) * (bounds.Diagonal.Length * 1.5 / Math.Sqrt(3.0));
            return Seq<CameraEdit>(
                new CameraEdit.Location(Value: bounds.Center + iso, UpdateTarget: false),
                new CameraEdit.Target(Value: bounds.Center, UpdateLocation: false),
                new CameraEdit.Project(Projection: new CameraProjection.Parallel(SymmetricFrustum: true)),
                new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric));
        });
}
