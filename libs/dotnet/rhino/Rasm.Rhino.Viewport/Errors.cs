using System.Drawing;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Viewport;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ProjectionMismatch(CameraProjection Required, CameraProjection Actual)
    : Expected("Pose projection {Required} differs from viewport projection {Actual}", ErrorOps.Code<ProjectionMismatch>()) {
    public static Fin<Unit> Unless(CameraProjection required, CameraProjection actual) => required == actual ? unit : new ProjectionMismatch(required, actual);
}

public sealed record Reopened(uint SnapshotSerial, uint DocumentSerial)
    : Expected("Snapshot belongs to document {SnapshotSerial} and the viewport to document {DocumentSerial}", ErrorOps.Code<Reopened>()) {
    public static Fin<Unit> Unless(uint snapshotSerial, uint documentSerial) => snapshotSerial == documentSerial ? unit : new Reopened(snapshotSerial, documentSerial);
}

public sealed record SampleOutsideExtent(System.Drawing.Point Pixel, Size Extent)
    : Expected("Sample pixel {Pixel} is outside viewport extent {Extent}", ErrorOps.Code<SampleOutsideExtent>()) {
    public static Fin<Unit> Unless(System.Drawing.Point pixel, Size extent) =>
        (pixel.X >= 0) && (pixel.X < extent.Width) && (pixel.Y >= 0) && (pixel.Y < extent.Height) ? unit : new SampleOutsideExtent(pixel, extent);
}

public sealed record WrongViewKind(string Required) : Expected("View is not a {Required}", ErrorOps.Code<WrongViewKind>());
