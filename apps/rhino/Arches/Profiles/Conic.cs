using Rasm.Rhino.Document;

namespace Arches.Profiles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Conic {
    // --- [LIMITS]
    public static readonly Limits<double> ParabolicRise = Limits.Above(0.0);

    // --- [PROFILES]
    public static Fin<ArchProfile> Parabolic(Span span, Point3d apex) => new ArchProfile.Parabolic(span, apex);

    public static Fin<ArchProfile> Elliptical(Span span, Point3d apex) {
        Ellipse ellipse = new(new Plane(span.Midpoint, -span.Direction, apex - span.Midpoint), span.HalfSpan, apex.DistanceTo(span.Midpoint));
        return Invalid.Unless<ArchProfile>(ellipse.IsValid, new ArchProfile.Elliptical(span, ellipse), nameof(Ellipse));
    }
}
