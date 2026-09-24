using System.Runtime.InteropServices;
using Arches.Interaction;
using Arches.Profiles;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.UI;

[assembly: Guid("07134403-e825-43e4-b711-8fa2f330760a")]

namespace Arches;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class ArchPlugIn : PlugIn;

public abstract class ArchCommand(string typePrompt, IterableNE<ArchType> types) : HostCommand {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode) =>
        from units in DocumentUnits.Read(doc, DocumentSpace.Model)
        from profile in Prompts.PickType(typePrompt, types, new Context(doc, ActiveNormal(doc), None))
        from added in Disposal.Using(
            profile.Joined(units.Absolute),
            curves => Commits.WithinRedraw(
                doc,
                new RedrawPolicy.AllViews(Deferred: true),
                TableOps.Apply(doc, new TableOp.Add(curves.Map(static curve => new GeometryPair(curve, None)), None, Reference: false))))
        select unit;

    private static IO<Vector3d> ActiveNormal(RhinoDoc doc) =>
        from row in Viewports.ResolveViewport(doc, new ViewportTarget.Active())
        from cplane in Cameras.GetConstructionPlane(row.Viewport)
        select cplane.Plane.Normal;
}

public sealed class CircularArch() : ArchCommand(
    LOC.STR("Select circular arch type"),
    IterableNE.create(
        ArchTypes.TwoPoint(LOC.CON("Semicircular"), Circular.Semicircular),
        ArchTypes.ThreePoint(LOC.CON("Segmental"), Circular.SingleCentered, RiseLimits.UpToSemicircle)));

public sealed class GothicArch() : ArchCommand(
    LOC.STR("Select Gothic arch type"),
    IterableNE.create(
        ArchTypes.TwoPoint(LOC.CON("Equilateral"), Gothic.Equilateral),
        ArchTypes.ThreePoint(LOC.CON("Lancet"), Gothic.Pointed, Gothic.LancetRise),
        ArchTypes.ThreePoint(LOC.CON("Depressed"), Gothic.Pointed, Gothic.DepressedRise)));

public sealed class ThreeCenteredArch() : ArchCommand(
    LOC.STR("Select three-centered arch type"),
    IterableNE.create(
        ArchTypes.TwoPoint(LOC.CON("BasketHandle"), ThreeCentered.BasketHandle),
        ArchTypes.ThreePoint(LOC.CON("Depressed"), ThreeCentered.Depressed, RiseLimits.UpToSemicircle)));

public sealed class FourCenteredArch() : ArchCommand(
    LOC.STR("Select four-centered arch type"),
    IterableNE.create(
        ArchTypes.TwoPoint(LOC.CON("Persian"), FourCentered.Persian),
        ArchTypes.TwoPoint(LOC.CON("Tudor"), FourCentered.Tudor),
        ArchTypes.TwoPoint(LOC.CON("Keel"), FourCentered.Keel)));

public sealed class HorseshoeArch() : ArchCommand(
    LOC.STR("Select horseshoe arch type"),
    IterableNE.create(
        ArchTypes.ThreePoint(LOC.CON("Rounded"), Circular.SingleCentered, RiseLimits.FromSemicircle),
        ArchTypes.ThreePoint(LOC.CON("Pointed"), Horseshoe.Pointed, Horseshoe.PointedRise)));

public sealed class OgeeArch() : ArchCommand(
    LOC.STR("Select ogee arch type"),
    IterableNE.create(
        ArchTypes.ThreePoint(LOC.CON("ThreeCenteredOgee"), Ogee.ThreeCentered, Ogee.ThreeCenteredRise),
        ArchTypes.TwoPoint(LOC.CON("ReverseOgee"), Ogee.Reverse),
        ArchTypes.TwoPoint(LOC.CON("Tented"), Ogee.Tented),
        ArchTypes.ThreePoint(LOC.CON("FourCenteredOgee"), Ogee.FourCentered, RiseLimits.FromSemicircle)));

public sealed class MultifoilArch() : ArchCommand(
    LOC.STR("Select multifoil arch type"),
    IterableNE.create(
        ArchTypes.FoilCount(LOC.CON("Multifoil"), Multifoil.Foils),
        ArchTypes.Foil(LOC.CON("Cinquefoil"), Multifoil.Cinquefoil),
        ArchTypes.Foil(LOC.CON("Trefoil"), Multifoil.Trefoil)));

public sealed class ConicArch() : ArchCommand(
    LOC.STR("Select conic arch type"),
    IterableNE.create(
        ArchTypes.ThreePoint(LOC.CON("Parabolic"), Conic.Parabolic, static _ => Conic.ParabolicRise),
        ArchTypes.ThreePoint(LOC.CON("Elliptical"), Conic.Elliptical, RiseLimits.UpToSemicircle)));
