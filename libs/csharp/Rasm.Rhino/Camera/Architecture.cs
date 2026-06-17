namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ArchitecturalStyle {
    public static readonly ArchitecturalStyle
        TwoPointElevation = new(key: 0, build: BuildTwoPoint),
        ParallelPlan = new(key: 1, build: BuildParallelPlan),
        ParallelAxonometric = new(key: 2, build: BuildAxon),
        PerspectiveTopView = new(key: 3, build: BuildPerspectiveTop),
        PerspectiveSection = new(key: 4, build: BuildPerspectiveSection),
        ReflectedCeiling = new(key: 5, build: BuildReflectedCeiling);

    [UseDelegateFromConstructor] internal partial Fin<Seq<CameraEdit>> Build(CameraSubject subject, Option<Vector3d> lockedUp);

    private static Fin<Seq<CameraEdit>> BuildTwoPoint(CameraSubject subject, Option<Vector3d> _) =>
        subject.BoundsOf(op: Op.Of()).Map(bounds => {
            Point3d center = bounds.Center;
            double distance = bounds.Diagonal.Length;
            return Seq<CameraEdit>(
                new CameraEdit.Location(Value: center + (Vector3d.XAxis * distance), Couple: new CameraCouple.Free()),
                new CameraEdit.Target(Value: center, Couple: new CameraCouple.Free()),
                new CameraEdit.Project(Projection: new CameraProjection.TwoPointPerspective(LensLength: CameraDefaults.LensLength)),
                new CameraEdit.Lock(Camera: new CameraLock.Architectural()),
                new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric));
        });

    // the up-lock (2nd param) is discarded: a parallel view's up follows from the Location->Target direction;
    // the architectural up-lock is applied via CameraEdit.Lock(CameraLock.Architectural()).
    private static Fin<Seq<CameraEdit>> BuildParallelPlan(CameraSubject subject, Option<Vector3d> _) =>
        subject.BoundsOf(op: Op.Of()).Map(bounds => Seq<CameraEdit>(
            new CameraEdit.Location(Value: bounds.Center + (Vector3d.ZAxis * (bounds.Diagonal.Length * 1.5)), Couple: new CameraCouple.Free()),
            new CameraEdit.Target(Value: bounds.Center, Couple: new CameraCouple.Free()),
            new CameraEdit.Project(Projection: new CameraProjection.Parallel(SymmetricFrustum: true)),
            new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric)));

    private static Fin<Seq<CameraEdit>> BuildAxon(CameraSubject subject, Option<Vector3d> _) =>
        subject.BoundsOf(op: Op.Of()).Map(bounds => {
            Vector3d iso = new Vector3d(x: 1.0, y: 1.0, z: 1.0) * (bounds.Diagonal.Length * 1.5 / Math.Sqrt(3.0));
            return Seq<CameraEdit>(
                new CameraEdit.Location(Value: bounds.Center + iso, Couple: new CameraCouple.Free()),
                new CameraEdit.Target(Value: bounds.Center, Couple: new CameraCouple.Free()),
                new CameraEdit.Project(Projection: new CameraProjection.Parallel(SymmetricFrustum: true)),
                new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric));
        });

    // Perspective looking straight down from above; the Architectural lock holds the up-axis so the
    // descended view keeps verticals vertical instead of free-tumbling on the next interaction.
    private static Fin<Seq<CameraEdit>> BuildPerspectiveTop(CameraSubject subject, Option<Vector3d> _) =>
        subject.BoundsOf(op: Op.Of()).Map(bounds => Seq<CameraEdit>(
            new CameraEdit.NavigateLookAt(From: bounds.Center + (Vector3d.ZAxis * (bounds.Diagonal.Length * 2.0)), At: bounds.Center),
            new CameraEdit.Project(Projection: new CameraProjection.Perspective(LensLength: CameraDefaults.LensLength)),
            new CameraEdit.Lock(Camera: new CameraLock.Architectural()),
            new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric)));

    // A perspective cut along the bounds' shallowest axis: the camera sits back along +X looking through
    // the subject, NearFar clamps the frustum to the box depth so the front face reads as a section cut.
    private static Fin<Seq<CameraEdit>> BuildPerspectiveSection(CameraSubject subject, Option<Vector3d> _) =>
        subject.BoundsOf(op: Op.Of()).Map(bounds => Seq<CameraEdit>(
            new CameraEdit.NavigateLookAt(From: bounds.Center + (Vector3d.XAxis * (bounds.Diagonal.Length * 1.5)), At: bounds.Center),
            new CameraEdit.Project(Projection: new CameraProjection.Perspective(LensLength: CameraDefaults.LensLength)),
            new CameraEdit.Lock(Camera: new CameraLock.Architectural()),
            new CameraEdit.NearFar(Source: new CameraSubject.InBounds(Value: bounds)),
            new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric)));

    // Wires the otherwise-unreachable CameraProjection.ParallelReflected: a mirrored plan looking up at the
    // ceiling plane (reflected-ceiling-plan convention). Camera/target unlocked, frustum left untouched.
    private static Fin<Seq<CameraEdit>> BuildReflectedCeiling(CameraSubject subject, Option<Vector3d> _) =>
        subject.BoundsOf(op: Op.Of()).Map(bounds => Seq<CameraEdit>(
            new CameraEdit.Location(Value: bounds.Center + (Vector3d.ZAxis * (bounds.Diagonal.Length * 1.5)), Couple: new CameraCouple.Free()),
            new CameraEdit.Target(Value: bounds.Center, Couple: new CameraCouple.Free()),
            new CameraEdit.Project(Projection: new CameraProjection.ParallelReflected()),
            new CameraEdit.SubjectFrame(Subject: subject, Mode: FramePaddingMode.Symmetric)));
}
