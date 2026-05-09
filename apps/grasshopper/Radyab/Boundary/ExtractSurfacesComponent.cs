using Analysis;
using Grasshopper;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Radyab.Boundary;

// --- [COMPONENT] -------------------------------------------------------------------------------

[IoId("F51A09A8-A5A5-467A-ADBA-C950511A0020")]
public sealed class ExtractSurfacesComponent : AnalysisComponent<object> {
    protected override Seq<IBridgeOutput<object>> Outputs { get; } = Seq<IBridgeOutput<object>>(
        new BridgeOutput<object, Brep>(
            Name: "All Surfaces",
            Code: "AS",
            Description: "Every face of the input as a single-face Brep; preserves trims; SubD inputs are converted via SubDToBrepOptions.Default.",
            Query: Analysis.Query.Faces<object, Brep>(aspect: Faces.All)),
        new BridgeOutput<object, Brep>(
            Name: "Top Surface",
            Code: "TS",
            Description: "Face(s) with the maximum area-centroid world-Z; ties returned within absolute tolerance (e.g. both caps of a horizontal cylinder, stacked planar panels).",
            Query: Analysis.Query.Faces<object, Brep>(aspect: Faces.Top)),
        new BridgeOutput<object, Brep>(
            Name: "Bottom Surface",
            Code: "BS",
            Description: "Face(s) with the minimum area-centroid world-Z; ties returned within absolute tolerance.",
            Query: Analysis.Query.Faces<object, Brep>(aspect: Faces.Bottom)),
        new BridgeOutput<object, Brep>(
            Name: "Indexed Surface",
            Code: "IS",
            Description: "Face at the user-supplied Index input; clamped to [0, count-1]. Empty when the input has zero faces.",
            Query: Analysis.Query.Faces<object, Brep>(aspect: Faces.At())),
        new BridgeOutput<object, Plane>(
            Name: "UV Frame",
            Code: "UV",
            Description: "Orthonormal frame at the indexed face's area centroid: X aligns with du, Z points outward (orientation-corrected for closed Breps), Y completes the right-handed basis.",
            Query: Analysis.Query.Faces<object, Plane>(aspect: Faces.At())));

    protected override Option<IndexInputSpec> IndexInput =>
        Some(IndexInputSpec.Standard);

    public ExtractSurfacesComponent()
        : base(nomen: new Nomen(
            "Extract Surfaces",
            "All faces, top/bottom by world-Z centroid (ties returned), an index-clamped face, and the UV frame on the indexed face. Accepts Brep, BrepFace, Surface, SubD; rejects Mesh.",
            "Radyab",
            "Extraction")) { }

    public ExtractSurfacesComponent(IReader reader) : base(reader: reader) { }
}
