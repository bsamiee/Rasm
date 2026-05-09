using Analysis;
using Core.Domain;
using Grasshopper;
using Rhino.Geometry;
using Thinktecture;

namespace Radyab.Components;

// --- [MODELS] ----------------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class SurfaceAspect {
    public static readonly SurfaceAspect AllSurfaces = new(
        key: nameof(AllSurfaces),
        displayName: "All Surfaces",
        code: "AS",
        description: "Every face of the input as a single-face Brep; preserves trims; SubD inputs are converted via SubDToBrepOptions.Default.",
        factory: static () => new BridgeOutput<RhinoGeometry, Brep>(
            Name: "All Surfaces",
            Code: "AS",
            Description: "Every face of the input as a single-face Brep; preserves trims; SubD inputs are converted via SubDToBrepOptions.Default.",
            Query: Query.Faces<object, Brep>(aspect: Faces.All)));
    public static readonly SurfaceAspect TopSurface = new(
        key: nameof(TopSurface),
        displayName: "Top Surface",
        code: "TS",
        description: "Face(s) with the maximum area-centroid world-Z; ties returned within absolute tolerance (e.g. both caps of a horizontal cylinder, stacked planar panels).",
        factory: static () => new BridgeOutput<RhinoGeometry, Brep>(
            Name: "Top Surface",
            Code: "TS",
            Description: "Face(s) with the maximum area-centroid world-Z; ties returned within absolute tolerance (e.g. both caps of a horizontal cylinder, stacked planar panels).",
            Query: Query.Faces<object, Brep>(aspect: Faces.Top)));
    public static readonly SurfaceAspect BottomSurface = new(
        key: nameof(BottomSurface),
        displayName: "Bottom Surface",
        code: "BS",
        description: "Face(s) with the minimum area-centroid world-Z; ties returned within absolute tolerance.",
        factory: static () => new BridgeOutput<RhinoGeometry, Brep>(
            Name: "Bottom Surface",
            Code: "BS",
            Description: "Face(s) with the minimum area-centroid world-Z; ties returned within absolute tolerance.",
            Query: Query.Faces<object, Brep>(aspect: Faces.Bottom)));
    public static readonly SurfaceAspect IndexedSurface = new(
        key: nameof(IndexedSurface),
        displayName: "Indexed Surface",
        code: "IS",
        description: "Face at the user-supplied Index input; clamped to [0, count-1]. Empty when the input has zero faces.",
        factory: static () => new BridgeOutput<RhinoGeometry, Brep>(
            Name: "Indexed Surface",
            Code: "IS",
            Description: "Face at the user-supplied Index input; clamped to [0, count-1]. Empty when the input has zero faces.",
            Query: Query.Faces<object, Brep>(aspect: Faces.At())));
    public static readonly SurfaceAspect UVFrame = new(
        key: nameof(UVFrame),
        displayName: "UV Frame",
        code: "UV",
        description: "Orthonormal frame at the indexed face's area centroid: X aligns with du, Z points outward (orientation-corrected for closed Breps), Y completes the right-handed basis.",
        factory: static () => new BridgeOutput<RhinoGeometry, Plane>(
            Name: "UV Frame",
            Code: "UV",
            Description: "Orthonormal frame at the indexed face's area centroid: X aligns with du, Z points outward (orientation-corrected for closed Breps), Y completes the right-handed basis.",
            Query: Query.Faces<object, Plane>(aspect: Faces.At())));
    public string DisplayName { get; }
    public string Code { get; }
    public string Description { get; }
    private Func<IBridgeOutput<RhinoGeometry>> Factory { get; }

    // --- [OPERATIONS] --------------------------------------------------------------------------

    public IBridgeOutput<RhinoGeometry> ToBridgeOutput() =>
        Factory();
}
