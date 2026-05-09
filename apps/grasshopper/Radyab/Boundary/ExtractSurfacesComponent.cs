using Core.Domain;
using Grasshopper;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Radyab.Boundary;

// --- [COMPONENT] -------------------------------------------------------------------------------

[IoId("F51A09A8-A5A5-467A-ADBA-C950511A0020")]
public sealed class ExtractSurfacesComponent : AnalysisComponent<RhinoGeometry> {
    protected override Seq<IBridgeOutput<RhinoGeometry>> Outputs { get; } =
        toSeq(SurfaceAspect.Items.Select((SurfaceAspect aspect) => aspect.ToBridgeOutput()));

    protected override Option<IndexInputSpec> IndexInput =>
        Some(new IndexInputSpec(
            Name: "Index",
            Code: "I",
            Description: "Zero-based selector; clamped to the available range.",
            Default: 0,
            Requirement: Requirement.MayBeMissing));

    public ExtractSurfacesComponent()
        : base(nomen: new Nomen(
            "Extract Surfaces",
            "All faces, top/bottom by world-Z centroid (ties returned), an index-clamped face, and the UV frame on the indexed face. Accepts Brep, BrepFace, Surface, SubD; rejects Mesh.",
            "Radyab",
            "Extraction")) { }

    public ExtractSurfacesComponent(IReader reader) : base(reader: reader) { }
}
