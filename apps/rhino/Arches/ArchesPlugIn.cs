using System.Runtime.InteropServices;
using Rasm.Rhino.Modeling.Curves;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.PlugIns;
using Rhino.PlugIns;

[assembly: Guid("07134403-e825-43e4-b711-8fa2f330760a")]

namespace Arches;

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class ArchesPlugIn() : DefinedPlugIn(new PlugInDefinition {
    LoadTime = PlugInLoadTime.AtStartup,
    Acquisitions = [AppSettings.NudgeFollowsActiveDocument(static (resolution, snap, grid) => (snap, 2 * resolution, grid))],
    Commands = [
        static sink => new ArchCommand(sink, new Guid(0x6d076643, 0x514b, 0x42b3, 0xa7, 0x16, 0xec, 0x15, 0x57, 0xa0, 0x4d, 0xec), "CircularArch", "Select circular arch type", IterableNE.create<ArchChoice>(
            new ArchChoice.Spanned("Semicircular", SpannedArch.Semicircular),
            new ArchChoice.Raised("Segmental", RaisedArch.Segmental))),
        static sink => new ArchCommand(sink, new Guid(0x5f756b0b, 0x0a5d, 0x44ba, 0xa4, 0xc6, 0xfa, 0x77, 0xe7, 0xf6, 0x7d, 0x5c), "GothicArch", "Select Gothic arch type", IterableNE.create<ArchChoice>(
            new ArchChoice.Spanned("Equilateral", SpannedArch.Equilateral),
            new ArchChoice.Raised("Lancet", RaisedArch.Lancet),
            new ArchChoice.Raised("Depressed", RaisedArch.DepressedGothic))),
        static sink => new ArchCommand(sink, new Guid(0x7067d347, 0xeacc, 0x4a47, 0x89, 0xb1, 0xdc, 0xff, 0xcd, 0x69, 0x11, 0x8f), "ThreeCenteredArch", "Select three-centered arch type", IterableNE.create<ArchChoice>(
            new ArchChoice.Spanned("BasketHandle", SpannedArch.BasketHandle),
            new ArchChoice.Raised("Depressed", RaisedArch.DepressedThreeCentered))),
        static sink => new ArchCommand(sink, new Guid(0xee54b6c4, 0x410c, 0x45c7, 0xb4, 0xe8, 0x66, 0x5f, 0x0d, 0x20, 0xc4, 0x2e), "FourCenteredArch", "Select four-centered arch type", IterableNE.create<ArchChoice>(
            new ArchChoice.Spanned("Persian", SpannedArch.Persian),
            new ArchChoice.Spanned("Tudor", SpannedArch.Tudor),
            new ArchChoice.Spanned("Keel", SpannedArch.Keel))),
        static sink => new ArchCommand(sink, new Guid(0xdd8a62b3, 0xe296, 0x4263, 0x85, 0xec, 0x17, 0x6a, 0xcf, 0xc7, 0x13, 0xa2), "HorseshoeArch", "Select horseshoe arch type", IterableNE.create<ArchChoice>(
            new ArchChoice.Raised("Rounded", RaisedArch.Horseshoe),
            new ArchChoice.Raised("Pointed", RaisedArch.PointedHorseshoe))),
        static sink => new ArchCommand(sink, new Guid(0x7fd6b2cd, 0x5a6c, 0x4c55, 0xbf, 0x34, 0x80, 0x34, 0xa0, 0x90, 0x8c, 0x8c), "OgeeArch", "Select ogee arch type", IterableNE.create<ArchChoice>(
            new ArchChoice.Raised("ThreeCenteredOgee", RaisedArch.ThreeCenteredOgee),
            new ArchChoice.Spanned("ReverseOgee", SpannedArch.ReverseOgee),
            new ArchChoice.Spanned("Tented", SpannedArch.TentedOgee),
            new ArchChoice.Raised("FourCenteredOgee", RaisedArch.FourCenteredOgee))),
        static sink => new ArchCommand(sink, new Guid(0xef5626af, 0xe4c8, 0x4dfe, 0xb6, 0xe9, 0x7a, 0x8a, 0x66, 0x36, 0xae, 0x9b), "MultifoilArch", "Select multifoil arch type", IterableNE.create<ArchChoice>(
            new ArchChoice.Counted("Multifoil", FoilCount.Create(5)),
            new ArchChoice.Foiled("Cinquefoil", FoiledArch.Cinquefoil),
            new ArchChoice.Foiled("Trefoil", FoiledArch.Trefoil))),
        static sink => new ArchCommand(sink, new Guid(0x7d37f77d, 0x297e, 0x4aa1, 0xa7, 0xad, 0xef, 0x90, 0xc5, 0x85, 0xb8, 0x10), "ConicArch", "Select conic arch type", IterableNE.create<ArchChoice>(
            new ArchChoice.Raised("Parabolic", RaisedArch.Parabolic),
            new ArchChoice.Raised("Elliptical", RaisedArch.Elliptical))),
    ],
});
