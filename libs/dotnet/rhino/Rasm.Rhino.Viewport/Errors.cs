using System.Drawing;

namespace Rasm.Rhino.Viewport;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class Codes {
    public const int ProjectionMismatch = 1500;

    public const int SampleOutsideExtent = 1501;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record ProjectionMismatch(CameraProjection Required, CameraProjection Actual)
    : Expected("Pose projection {Required} differs from viewport projection {Actual}", Codes.ProjectionMismatch) {
    public static Fin<Unit> Unless(CameraProjection required, CameraProjection actual) => required == actual ? unit : new ProjectionMismatch(required, actual);
}

public sealed record SampleOutsideExtent(System.Drawing.Point Pixel, Size Extent)
    : Expected("Sample pixel {Pixel} is outside viewport extent {Extent}", Codes.SampleOutsideExtent) {
    public static Fin<Unit> Unless(System.Drawing.Point pixel, Size extent) =>
        new Rectangle(System.Drawing.Point.Empty, extent).Contains(pixel) ? unit : new SampleOutsideExtent(pixel, extent);
}
