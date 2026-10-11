using System.Runtime.InteropServices;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Plugin;
using Rhino.PlugIns;

[assembly: Guid("07134403-e825-43e4-b711-8fa2f330760a")]

namespace Arches;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class ArchPlugIn() : DefinedPlugIn(new PlugInDefinition {
    LoadTime = PlugInLoadTime.AtStartup,
    History = static _ => IO.pure(new HistoryLimits(10, Duration.FromSeconds(5))),
    Acquisitions = [AppSettings.NudgeFollowsActiveDocument(static (resolution, snap, grid) => (snap, 2 * resolution, grid))],
    Commands = [
        static sink => new ArchCommand(sink, new Guid(0x6d076643, 0x514b, 0x42b3, 0xa7, 0x16, 0xec, 0x15, 0x57, 0xa0, 0x4d, 0xec), "CircularArch", "Select circular arch type", IterableNE.create<ArchForm>(
            new ArchForm.Spanned("Semicircular", ArchCurves.Semicircular),
            new ArchForm.Raised("Segmental", ArchCurves.Circular, ArchCurves.UpToSemicircle))),
        static sink => new ArchCommand(sink, new Guid(0x5f756b0b, 0x0a5d, 0x44ba, 0xa4, 0xc6, 0xfa, 0x77, 0xe7, 0xf6, 0x7d, 0x5c), "GothicArch", "Select Gothic arch type", IterableNE.create<ArchForm>(
            new ArchForm.Spanned("Equilateral", ArchCurves.Equilateral),
            new ArchForm.Raised("Lancet", ArchCurves.Gothic, ArchCurves.LancetRise),
            new ArchForm.Raised("Depressed", ArchCurves.Gothic, ArchCurves.DepressedGothicRise))),
        static sink => new ArchCommand(sink, new Guid(0x7067d347, 0xeacc, 0x4a47, 0x89, 0xb1, 0xdc, 0xff, 0xcd, 0x69, 0x11, 0x8f), "ThreeCenteredArch", "Select three-centered arch type", IterableNE.create<ArchForm>(
            new ArchForm.Spanned("BasketHandle", ArchCurves.BasketHandle),
            new ArchForm.Raised("Depressed", ArchCurves.DepressedThreeCentered, ArchCurves.UpToSemicircle))),
        static sink => new ArchCommand(sink, new Guid(0xee54b6c4, 0x410c, 0x45c7, 0xb4, 0xe8, 0x66, 0x5f, 0x0d, 0x20, 0xc4, 0x2e), "FourCenteredArch", "Select four-centered arch type", IterableNE.create<ArchForm>(
            new ArchForm.Spanned("Persian", ArchCurves.Persian),
            new ArchForm.Spanned("Tudor", ArchCurves.Tudor),
            new ArchForm.Spanned("Keel", ArchCurves.Keel))),
        static sink => new ArchCommand(sink, new Guid(0xdd8a62b3, 0xe296, 0x4263, 0x85, 0xec, 0x17, 0x6a, 0xcf, 0xc7, 0x13, 0xa2), "HorseshoeArch", "Select horseshoe arch type", IterableNE.create<ArchForm>(
            new ArchForm.Raised("Rounded", ArchCurves.Circular, ArchCurves.FromSemicircle),
            new ArchForm.Raised("Pointed", ArchCurves.PointedHorseshoe, ArchCurves.PointedHorseshoeRise))),
        static sink => new ArchCommand(sink, new Guid(0x7fd6b2cd, 0x5a6c, 0x4c55, 0xbf, 0x34, 0x80, 0x34, 0xa0, 0x90, 0x8c, 0x8c), "OgeeArch", "Select ogee arch type", IterableNE.create<ArchForm>(
            new ArchForm.Raised("ThreeCenteredOgee", ArchCurves.ThreeCenteredOgee, ArchCurves.ThreeCenteredOgeeRise),
            new ArchForm.Spanned("ReverseOgee", ArchCurves.ReverseOgee),
            new ArchForm.Spanned("Tented", ArchCurves.TentedOgee),
            new ArchForm.Raised("FourCenteredOgee", ArchCurves.FourCenteredOgee, ArchCurves.FromSemicircle))),
        static sink => new ArchCommand(sink, new Guid(0xef5626af, 0xe4c8, 0x4dfe, 0xb6, 0xe9, 0x7a, 0x8a, 0x66, 0x36, 0xae, 0x9b), "MultifoilArch", "Select multifoil arch type", IterableNE.create<ArchForm>(
            new ArchForm.Counted("Multifoil", ArchCurves.Multifoil, InitialCount: 5),
            new ArchForm.Foiled("Cinquefoil", ArchCurves.Cinquefoil),
            new ArchForm.Foiled("Trefoil", ArchCurves.Trefoil))),
        static sink => new ArchCommand(sink, new Guid(0x7d37f77d, 0x297e, 0x4aa1, 0xa7, 0xad, 0xef, 0x90, 0xc5, 0x85, 0xb8, 0x10), "ConicArch", "Select conic arch type", IterableNE.create<ArchForm>(
            new ArchForm.Raised("Parabolic", ArchCurves.Parabolic, ArchCurves.PositiveRise),
            new ArchForm.Raised("Elliptical", ArchCurves.Elliptical, ArchCurves.UpToSemicircle))),
    ],
});
