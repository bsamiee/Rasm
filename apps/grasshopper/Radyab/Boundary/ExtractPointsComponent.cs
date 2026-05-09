using Core.Domain;
using Grasshopper;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Radyab.Boundary;

// --- [COMPONENT] -------------------------------------------------------------------------------

[IoId("0C58A2D3-4709-4D74-85B1-48BC85FA1F69")]
[Nomen(
    name: "Extract Points",
    info: "Edge midpoints, intelligent center, vertices, bounding corners, and world-cardinal quadrants for any Rhino geometry.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractPointsComponent : AnalysisComponent<RhinoGeometry> {
    protected override Seq<IBridgeOutput<RhinoGeometry>> Outputs { get; } =
        toSeq(PointAspect.Items.Select((PointAspect aspect) => aspect.ToBridgeOutput()));

    public ExtractPointsComponent() : base(nomen: NomenOf<ExtractPointsComponent>()) { }

    public ExtractPointsComponent(IReader reader) : base(reader: reader) { }
}
