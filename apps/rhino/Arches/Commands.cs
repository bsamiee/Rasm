using System.Runtime.InteropServices;
using Arches.Interaction;
using Arches.Profiles;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Plugin;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.UI;

[assembly: Guid("07134403-e825-43e4-b711-8fa2f330760a")]

namespace Arches;

// --- [COMPOSITION] ---------------------------------------------------------------------
internal sealed class ArchCommand(Guid id, string englishName, string typePrompt, IterableNE<ArchType> types) : HostCommand {
    public override Guid Id => id;

    public override string EnglishName => englishName;

    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode) =>
        from tolerance in IO.lift(() => doc.ModelAbsoluteTolerance)
        from profile in Prompts.PickType(typePrompt, types, new Context(doc, tolerance, None))
        from added in DisposalOps.Using(
            profile.Joined(tolerance),
            curves => Commits.WithinRedraw(
                doc,
                new RedrawPolicy.AllViews(Deferred: true),
                TableOps.Apply(doc, new TableOp.Add(curves.Map(static curve => new GeometryPair(curve, None)), None, Reference: false))))
        select unit;
}

public sealed class ArchPlugIn() : CallbackPlugIn(new PlugInCallbacks {
    Subscriptions = [static reject => AppSettings.NudgeFollowsActiveDocument(static (resolution, grid) => (grid.SnapSpacing, 2 * resolution, grid.GridSpacing), reject)],
    Commands = [
        new ArchCommand(new Guid(0x6d076643, 0x514b, 0x42b3, 0xa7, 0x16, 0xec, 0x15, 0x57, 0xa0, 0x4d, 0xec), "CircularArch", LOC.STR("Select circular arch type"), IterableNE.create(
            ArchType.Of(LOC.CON("Semicircular"), Circular.Semicircular),
            ArchType.Of(LOC.CON("Segmental"), Circular.SingleCentered, RiseLimits.UpToSemicircle))),
        new ArchCommand(new Guid(0x5f756b0b, 0xa5d, 0x44ba, 0xa4, 0xc6, 0xfa, 0x77, 0xe7, 0xf6, 0x7d, 0x5c), "GothicArch", LOC.STR("Select Gothic arch type"), IterableNE.create(
            ArchType.Of(LOC.CON("Equilateral"), Gothic.Equilateral),
            ArchType.Of(LOC.CON("Lancet"), Gothic.Pointed, Gothic.LancetRise),
            ArchType.Of(LOC.CON("Depressed"), Gothic.Pointed, Gothic.DepressedRise))),
        new ArchCommand(new Guid(0x7067d347, 0xeacc, 0x4a47, 0x89, 0xb1, 0xdc, 0xff, 0xcd, 0x69, 0x11, 0x8f), "ThreeCenteredArch", LOC.STR("Select three-centered arch type"), IterableNE.create(
            ArchType.Of(LOC.CON("BasketHandle"), ThreeCentered.BasketHandle),
            ArchType.Of(LOC.CON("Depressed"), ThreeCentered.Depressed, RiseLimits.UpToSemicircle))),
        new ArchCommand(new Guid(0xee54b6c4, 0x410c, 0x45c7, 0xb4, 0xe8, 0x66, 0x5f, 0xd, 0x20, 0xc4, 0x2e), "FourCenteredArch", LOC.STR("Select four-centered arch type"), IterableNE.create(
            ArchType.Of(LOC.CON("Persian"), FourCentered.Persian),
            ArchType.Of(LOC.CON("Tudor"), FourCentered.Tudor),
            ArchType.Of(LOC.CON("Keel"), FourCentered.Keel))),
        new ArchCommand(new Guid(0xdd8a62b3, 0xe296, 0x4263, 0x85, 0xec, 0x17, 0x6a, 0xcf, 0xc7, 0x13, 0xa2), "HorseshoeArch", LOC.STR("Select horseshoe arch type"), IterableNE.create(
            ArchType.Of(LOC.CON("Rounded"), Circular.SingleCentered, RiseLimits.FromSemicircle),
            ArchType.Of(LOC.CON("Pointed"), Horseshoe.Pointed, Horseshoe.PointedRise))),
        new ArchCommand(new Guid(0x7fd6b2cd, 0x5a6c, 0x4c55, 0xbf, 0x34, 0x80, 0x34, 0xa0, 0x90, 0x8c, 0x8c), "OgeeArch", LOC.STR("Select ogee arch type"), IterableNE.create(
            ArchType.Of(LOC.CON("ThreeCenteredOgee"), Ogee.ThreeCentered, Ogee.ThreeCenteredRise),
            ArchType.Of(LOC.CON("ReverseOgee"), Ogee.Reverse),
            ArchType.Of(LOC.CON("Tented"), Ogee.Tented),
            ArchType.Of(LOC.CON("FourCenteredOgee"), Ogee.FourCentered, RiseLimits.FromSemicircle))),
        new ArchCommand(new Guid(0xef5626af, 0xe4c8, 0x4dfe, 0xb6, 0xe9, 0x7a, 0x8a, 0x66, 0x36, 0xae, 0x9b), "MultifoilArch", LOC.STR("Select multifoil arch type"), IterableNE.create(
            ArchType.Of(LOC.CON("Multifoil"), Multifoil.Foils),
            ArchType.Of(LOC.CON("Cinquefoil"), Multifoil.Cinquefoil),
            ArchType.Of(LOC.CON("Trefoil"), Multifoil.Trefoil))),
        new ArchCommand(new Guid(0x7d37f77d, 0x297e, 0x4aa1, 0xa7, 0xad, 0xef, 0x90, 0xc5, 0x85, 0xb8, 0x10), "ConicArch", LOC.STR("Select conic arch type"), IterableNE.create(
            ArchType.Of(LOC.CON("Parabolic"), Conic.Parabolic, RiseLimits.Positive),
            ArchType.Of(LOC.CON("Elliptical"), Conic.Elliptical, RiseLimits.UpToSemicircle))),
    ],
}) {
    public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;
}
