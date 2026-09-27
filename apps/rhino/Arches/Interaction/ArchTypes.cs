using Arches.Profiles;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.UI;

namespace Arches.Interaction;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Anchor(Point3d Point, Vector3d Normal);

public sealed record Context(RhinoDoc Doc, double Tolerance, Option<Anchor> Start);

public sealed record ArchType(LocalizeStringPair Name, Func<Context, IO<ArchProfile>> Run);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ArchTypes {
    private const int InitialFoilCount = 5;

    public static ArchType TwoPoint(LocalizeStringPair name, Func<Span, Fin<ArchProfile>> build) =>
        new(name, context =>
            from span in Prompts.PickSpan(context, flip: true, (display, picked) => ProfileMarks.Profile(display, picked, build(picked)))
            from profile in IO.lift(build(span))
            select profile);

    public static ArchType ThreePoint(LocalizeStringPair name, Func<Span, Point3d, Fin<ArchProfile>> build, Func<Span, Fin<Limits<double>>> rise) =>
        new(name, context =>
            from span in Prompts.PickSpan(context, flip: false, static (display, picked) => ProfileMarks.Ends(display, picked.Start, picked.End))
            from limits in IO.lift(rise(span))
            from apex in Prompts.PickApex(context.Doc, span, limits, (display, candidate) => ProfileMarks.Profile(display, span, build(span, candidate)))
            from profile in IO.lift(build(span, apex))
            select profile);

    public static ArchType Foil(LocalizeStringPair name, Func<Span, bool, Fin<ArchProfile>> build) =>
        new(name, context =>
            from picked in Prompts.PickFoilSpan(context, (display, span, pointed) => ProfileMarks.Profile(display, span, build(span, pointed)))
            from profile in IO.lift(build(picked.Span, picked.Pointed))
            select profile);

    public static ArchType FoilCount(LocalizeStringPair name, Func<Span, bool, int, Fin<ArchProfile>> build) =>
        new(name, context =>
            from picked in Prompts.PickFoilSpan(context, (display, span, pointed) => ProfileMarks.Profile(display, span, build(span, pointed, InitialFoilCount)))
            from count in Prompts.PickFoilCount(picked.Span, InitialFoilCount, (display, candidate) => ProfileMarks.Profile(display, picked.Span, build(picked.Span, picked.Pointed, candidate)))
            from profile in IO.lift(build(picked.Span, picked.Pointed, count))
            select profile);
}
