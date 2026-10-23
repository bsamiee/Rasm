using Arches;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Modeling.Curves;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Commands;
using Rhino.PlugIns;

[assembly: RuiFile<ArchFamily>]

namespace Arches;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record ArchGlyph(string Key) : IGlyph;

internal sealed record ArchFamily(Guid Id, string Command, string Caption, string Prompt, string Tooltip, IterableNE<ArchChoice> Choices) : IRuiSource {
    // --- [FAMILIES]
    public static readonly Seq<Seq<ArchFamily>> Clusters = [
        [
            new(new Guid(0x6d076643, 0x514b, 0x42b3, 0xa7, 0x16, 0xec, 0x15, 0x57, 0xa0, 0x4d, 0xec), "CircularArch", "Circular Arch", "Select circular arch type",
                "Circular arch: semicircular or segmental", IterableNE.create<ArchChoice>(
                    new ArchChoice.Spanned("Semicircular", "Semicircular", "Semicircular arch: half circle on the span", SpannedArch.Semicircular),
                    new ArchChoice.Raised("Segmental", "Segmental", "Segmental arch: circle segment rising less than half the span", RaisedArch.Segmental))),
            new(new Guid(0xdd8a62b3, 0xe296, 0x4263, 0x85, 0xec, 0x17, 0x6a, 0xcf, 0xc7, 0x13, 0xa2), "HorseshoeArch", "Horseshoe Arch", "Select horseshoe arch type",
                "Horseshoe arch: arcs reaching wider than the span", IterableNE.create<ArchChoice>(
                    new ArchChoice.Raised("Rounded", "Rounded", "Horseshoe arch: circle arc reaching wider than the span", RaisedArch.Horseshoe),
                    new ArchChoice.Raised("Pointed", "Pointed", "Pointed horseshoe arch: two arcs reaching wider than the span and meeting in a point", RaisedArch.PointedHorseshoe))),
        ],
        [
            new(new Guid(0x5f756b0b, 0x0a5d, 0x44ba, 0xa4, 0xc6, 0xfa, 0x77, 0xe7, 0xf6, 0x7d, 0x5c), "GothicArch", "Gothic Arch", "Select Gothic arch type",
                "Gothic arch: two arcs meeting in a point", IterableNE.create<ArchChoice>(
                    new ArchChoice.Spanned("Equilateral", "Equilateral", "Equilateral arch: two arcs with radius equal to the span", SpannedArch.Equilateral),
                    new ArchChoice.Raised("Lancet", "Lancet", "Lancet arch: pointed arch rising above the equilateral arch", RaisedArch.Lancet),
                    new ArchChoice.Raised("Drop", "Drop", "Drop arch: pointed arch with radius shorter than the span", RaisedArch.Drop))),
            new(new Guid(0x7fd6b2cd, 0x5a6c, 0x4c55, 0xbf, 0x34, 0x80, 0x34, 0xa0, 0x90, 0x8c, 0x8c), "OgeeArch", "Ogee Arch", "Select ogee arch type",
                "Ogee arch: S-curves rising to the crown", IterableNE.create<ArchChoice>(
                    new ArchChoice.Raised("ThreeCenteredOgee", "Three-Centered Ogee", "Three-centered ogee arch: semicircle haunches reversing into concave arcs meeting in a point", RaisedArch.ThreeCenteredOgee),
                    new ArchChoice.Spanned("ReverseOgee", "Reverse Ogee", "Reverse ogee arch: concave then convex arcs meeting in a rounded crown", SpannedArch.ReverseOgee),
                    new ArchChoice.Spanned("Tented", "Tented", "Tented arch: four concave arcs rising to a point", SpannedArch.TentedOgee),
                    new ArchChoice.Raised("FourCenteredOgee", "Four-Centered Ogee", "Four-centered ogee arch: haunch arcs of a third of the span reversing into concave arcs meeting in a point", RaisedArch.FourCenteredOgee))),
        ],
        [
            new(new Guid(0x7067d347, 0xeacc, 0x4a47, 0x89, 0xb1, 0xdc, 0xff, 0xcd, 0x69, 0x11, 0x8f), "ThreeCenteredArch", "Three-Centered Arch", "Select three-centered arch type",
                "Three-centered arch: three arcs flatter than a semicircle", IterableNE.create<ArchChoice>(
                    new ArchChoice.Spanned("BasketHandle", "Basket Handle", "Basket-handle arch: three 60° arcs rising about a third of the span", SpannedArch.BasketHandle),
                    new ArchChoice.Raised("Depressed", "Depressed", "Depressed three-centered arch: three arcs at a chosen rise below half the span", RaisedArch.DepressedThreeCentered))),
            new(new Guid(0xee54b6c4, 0x410c, 0x45c7, 0xb4, 0xe8, 0x66, 0x5f, 0x0d, 0x20, 0xc4, 0x2e), "FourCenteredArch", "Four-Centered Arch", "Select four-centered arch type",
                "Four-centered arch: four arcs meeting in a point", IterableNE.create<ArchChoice>(
                    new ArchChoice.Spanned("Tudor", "Tudor", "Tudor arch: four arcs, haunches turning 45° into a low pointed crown", SpannedArch.Tudor),
                    new ArchChoice.Spanned("Persian", "Persian", "Persian arch: four arcs, haunches turning 60° into a flat, barely pointed crown", SpannedArch.Persian),
                    new ArchChoice.Spanned("Keel", "Keel", "Keel arch: flared springing and convex arcs meeting in a point", SpannedArch.Keel))),
        ],
        [
            new(new Guid(0xef5626af, 0xe4c8, 0x4dfe, 0xb6, 0xe9, 0x7a, 0x8a, 0x66, 0x36, 0xae, 0x9b), "MultifoilArch", "Multifoil Arch", "Select multifoil arch type",
                "Multifoil arch: lobed arcs from springing to crown", IterableNE.create<ArchChoice>(
                    new ArchChoice.Foiled("Cinquefoil", "Cinquefoil", "Cinquefoil arch: five lobes", FoiledArch.Cinquefoil),
                    new ArchChoice.Foiled("Trefoil", "Trefoil", "Trefoil arch: three lobes", FoiledArch.Trefoil),
                    new ArchChoice.Counted("Multifoil", "Multifoil", "Multifoil arch: a chosen number of lobes", FoilCount.Create(7)))),
        ],
        [
            new(new Guid(0x7d37f77d, 0x297e, 0x4aa1, 0xa7, 0xad, 0xef, 0x90, 0xc5, 0x85, 0xb8, 0x10), "ConicArch", "Conic Arch", "Select conic arch type",
                "Conic arch: parabola or half ellipse", IterableNE.create<ArchChoice>(
                    new ArchChoice.Raised("Parabolic", "Parabolic", "Parabolic arch: parabola at a chosen rise", RaisedArch.Parabolic),
                    new ArchChoice.Raised("Elliptical", "Elliptical", "Elliptical arch: half ellipse rising less than half the span", RaisedArch.Elliptical))),
        ],
    ];

    public static RuiFile Rui { get; } = new(
        [new ToolbarRow("Arches", new ArchGlyph("GothicArch_Equilateral.svg"), Clusters.Map(static cluster => cluster.Map(static family => family.Button))
            .Reduce(static (left, right) => left.Add(new ToolbarItemRow.Separator()) + right))],
        []);

    // --- [COMMANDS]
    public Func<IPlugInSink, Command> Registered => sink => new ArchCommand(sink, Id, Command, Prompt, Choices);

    // --- [TOOLBAR]
    private ArchGlyph Glyph(ArchChoice choice) => new($"{Command}_{choice.Option}.svg");

    private Macro Macro(Seq<ScriptStep> steps, string caption, string tooltip, ArchChoice shown) =>
        new CommandScript(Command, steps) switch {
            var script => new(script, new CommandFace(script.Text, caption, tooltip, Some<IGlyph>(Glyph(shown)), None)),
        };

    private Macro FromMidpoint(Seq<ScriptStep> steps, string caption, string tooltip, ArchChoice shown) =>
        Macro(steps.Add(new ScriptStep.Assign(ArchPrompts.BothSides, LabelPair.NoYes.On)), $"{caption}, From Midpoint", $"{tooltip}, from the span midpoint", shown);

    private ToolbarItemRow Button =>
        new ToolbarItemRow.Button(Macro([], Caption, Tooltip, Choices.Head)) {
            Right = Some(FromMidpoint([new ScriptStep.Pause()], Caption, Tooltip, Choices.Head)),
            Flyout = Some(new Flyout(new ToolbarRow(Caption, Glyph(Choices.Head), toSeq(Choices).Map(choice => (ToolbarItemRow)new ToolbarItemRow.Button(
                Macro([new ScriptStep.Pick(choice.Option)], $"{Caption}: {choice.Label}", choice.Tooltip, choice)) {
                Right = Some(FromMidpoint([new ScriptStep.Pick(choice.Option)], $"{Caption}: {choice.Label}", choice.Tooltip, choice)),
            })), FlyoutStyle.Normal)),
        };
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class ArchesPlugIn() : DefinedPlugIn(new PlugInDefinition {
    LoadTime = PlugInLoadTime.AtStartup,
    Clock = TimeProvider.System,
    Acquisitions = [AppSettings.NudgeFollowsActiveDocument(static (resolution, snap, grid) => (snap, 2 * resolution, grid))],
    Commands = ArchFamily.Clusters.Bind(static cluster => cluster.Map(static family => family.Registered)),
});
