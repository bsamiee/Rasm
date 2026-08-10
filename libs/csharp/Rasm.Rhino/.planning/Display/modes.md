# [RASM_RHINO_DISPLAY_MODES]

`Modes.Configure` owns display-mode appearance, descriptor policy, table operations, viewport binding, mode-scoped capture, and built-in analysis attachment as one request algebra. Raw host editors remain inside the fold, every viewport touch stays leased, and every successful mutation returns detached mode evidence.

`ModeOp` is the display-mode table seam this page owns, `ViewportTarget` remains the viewport identity seam, and `CaptureArtifact` remains bitmap custody. `DisplayModeDescription`, `DisplayPipelineAttributes`, `RhinoViewport`, and `VisualAnalysisMode` never cross the receipt boundary.

## [01]-[INDEX]

- [02]-[APPEARANCE]: `Appearance` folds complete concern values over the live mode editor and reads the whole set back.
- [03]-[MODE_FAMILY]: `ModeKind`, `ModePolicy`, and `ModePlan` own identity, policy, and derivation.
- [04]-[MODE_TABLE]: `ModeOp` owns the display-mode table operation family.
- [05]-[CONFIGURE]: `ModeRequest` closes every mode, table, viewport, capture, and analysis modality behind `Modes.Configure`; `ModeSummary` is the detached descriptor projection every query answers with.

## [02]-[APPEARANCE]

- Owner: `Appearance` is the closed concern family; each case carries the whole state its host writer consumes.
- Entry: `Appearance.Write` and `Appearance.Of` are the only surfaces that receive `DisplayPipelineAttributes`.
- Auto: `Appearance.Write` traverses the immutable case sequence and captures host rejection on one rail.
- Law: the family is SYMMETRIC — `Of` projects the whole live attribute set back as one concern per case, in `Write`'s own dispatch order, so mode state round-trips: a commit compensates from a prior read, an inspect receipt carries the whole concern set, and a case whose writer has no reader is a defect neither half can hide. A concern that broadcasts one value across sibling host slots reads back from the slot the case names first, so a `Of`-then-`Write` round trip restores every slot the case owns.
- Law: each `Write` arm is a TOTAL host write of the state its case carries — a partial concern is not a state the attribute set can hold, so the straight-line settable assignment is the concern's granularity, not a missing decomposition. Every arm runs under its OWN `Op`, so a mid-arm host throw names the concern that was mid-write and the compensating read knows which concern to restore.
- Law: `ShineAxis` is the one shine/transparency regime owner — the attribute editor runs shine to `Material.MaxShine` and transparency to a 0..100 percentage while `DisplayMaterial` runs both on the unit interval — so a value admitted for one regime cannot silently satisfy the other; `ShadedFace` on the draw page composes the same owner.
- Law: the concern sequence admits one row per case — duplicate discriminants reject at request admission, so no later row silently overwrites an earlier host write.
- Law: the case set spans the WHOLE public attribute model, so an attribute family with no writer is a defect the case that owns its concern absorbs — the tangent, single-curve, and iso-colour band widens `Edges`, the shadow band rides one `ShadowBand` carrier on `Lighting`, the top-level grid, plane, and axes members widen `Grid`, the backface-material and per-face override band widens `Shading` over two `FaceOverride` carriers, and the two genuinely homeless scene concerns (`BoundingBoxMode`, `DynamicDisplayUsage`) seat on `Pipeline`; a sibling record beside the owning case is the deleted form.
- Law: a thickness-, colour-, or display-usage discriminant is a bounded row carrying ONE native column per host family — the four thickness families and the two colour families are distinct nested enums whose rosters diverge, and `BoundingBoxDisplayMode` is non-sequential, so an ordinal cast or a single native value fanned across four writers is unrepresentable.
- Receipt: appearance contributes only through the enclosing `ModeReceipt.Configured` case.
- Growth: a host appearance concern lands as one row or carrier field on the case that owns its concern, and only a genuinely new concern earns an `Appearance` case with its dispatch arm.
- Boundary: colors quantize once at the writer; raw host colors and attribute editors stay inside the boundary.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
// Host truth: the four thickness-use families are DISTINCT nested enums with divergent rosters, and each writer takes only
// its own, so one shared row carries a native column per family rather than one value cast across four signatures.
[SmartEnum<int>]
public sealed partial class WidthUse {
    public static readonly WidthUse Object = new(
        key: 0,
        curve: DisplayPipelineAttributes.CurveThicknessUse.ObjectWidth,
        surface: DisplayPipelineAttributes.SurfaceThicknessUse.ObjectWidth,
        naked: DisplayPipelineAttributes.SurfaceNakedEdgeThicknessUse.ObjectWidth,
        iso: DisplayPipelineAttributes.SurfaceIsoThicknessUse.ObjectWidth,
        subD: DisplayPipelineAttributes.SubDThicknessUse.ObjectWidth);
    public static readonly WidthUse Pixels = new(
        key: 1,
        curve: DisplayPipelineAttributes.CurveThicknessUse.Pixels,
        surface: DisplayPipelineAttributes.SurfaceThicknessUse.Pixels,
        naked: DisplayPipelineAttributes.SurfaceNakedEdgeThicknessUse.Pixels,
        iso: DisplayPipelineAttributes.SurfaceIsoThicknessUse.PixelsUV,
        subD: DisplayPipelineAttributes.SubDThicknessUse.Pixels);
    // The two rows above are the axes every family shares; these carry the extra row one family alone admits.
    public static readonly WidthUse InheritEdge = new(
        key: 2,
        curve: DisplayPipelineAttributes.CurveThicknessUse.ObjectWidth,
        surface: DisplayPipelineAttributes.SurfaceThicknessUse.ObjectWidth,
        naked: DisplayPipelineAttributes.SurfaceNakedEdgeThicknessUse.UseSurfaceEdgeSettings,
        iso: DisplayPipelineAttributes.SurfaceIsoThicknessUse.SingleWidthForAllCurves,
        subD: DisplayPipelineAttributes.SubDThicknessUse.ObjectWidth);

    internal DisplayPipelineAttributes.CurveThicknessUse Curve { get; }
    internal DisplayPipelineAttributes.SurfaceThicknessUse Surface { get; }
    internal DisplayPipelineAttributes.SurfaceNakedEdgeThicknessUse Naked { get; }
    internal DisplayPipelineAttributes.SurfaceIsoThicknessUse Iso { get; }
    internal DisplayPipelineAttributes.SubDThicknessUse SubD { get; }
}

// Host truth: the two shine/transparency regimes are NOT interchangeable. The display-mode attribute editor runs shine to
// `Material.MaxShine` and transparency as a 0..100 percentage; `DisplayMaterial` runs BOTH on the unit interval. One owner
// names each regime with its own ceiling, so a shine admitted for the editor cannot pass a material bracket's admission
// and a unit-interval transparency cannot pass the editor's — a bare `double` bound would let either cross unnoticed.
[SmartEnum<int>]
public sealed partial class ShineAxis {
    public static readonly ShineAxis Editor = new(key: 0, shineCeiling: Material.MaxShine, transparencyCeiling: 100.0);
    public static readonly ShineAxis Unit = new(key: 1, shineCeiling: 1.0, transparencyCeiling: 1.0);

    internal double ShineCeiling { get; }
    internal double TransparencyCeiling { get; }

    internal bool Shine(double value) => double.IsFinite(value) && value >= 0.0 && value <= ShineCeiling;

    internal bool Transparency(double value) => double.IsFinite(value) && value >= 0.0 && value <= TransparencyCeiling;
}

[SmartEnum<int>]
public sealed partial class IsoColorUse {
    public static readonly IsoColorUse ObjectColor = new(0, DisplayPipelineAttributes.SurfaceIsoColorUse.ObjectColor);
    public static readonly IsoColorUse SingleColor = new(1, DisplayPipelineAttributes.SurfaceIsoColorUse.SingleColorForAll);
    public static readonly IsoColorUse SpecifiedUv = new(2, DisplayPipelineAttributes.SurfaceIsoColorUse.SpecifiedUV);
    internal DisplayPipelineAttributes.SurfaceIsoColorUse Native { get; }
}

[SmartEnum<int>]
public sealed partial class EdgeColorUse {
    public static readonly EdgeColorUse ObjectColor = new(0, DisplayPipelineAttributes.SurfaceEdgeColorUse.ObjectColor);
    public static readonly EdgeColorUse IsocurveColor = new(1, DisplayPipelineAttributes.SurfaceEdgeColorUse.IsocurveColor);
    public static readonly EdgeColorUse SingleColor = new(2, DisplayPipelineAttributes.SurfaceEdgeColorUse.SingleColorForAll);
    internal DisplayPipelineAttributes.SurfaceEdgeColorUse Native { get; }
}

// Host truth: `BoundingBoxDisplayMode` is NON-SEQUENTIAL (`None = 0`, `OnAlways = 1`, `OnDuringDynamicDisplay = 2`), so the
// key is the declaration ordinal and the native row is carried, never cast.
[SmartEnum<int>]
public sealed partial class BoundsUse {
    public static readonly BoundsUse None = new(0, DisplayPipelineAttributes.BoundingBoxDisplayMode.None);
    public static readonly BoundsUse Always = new(1, DisplayPipelineAttributes.BoundingBoxDisplayMode.OnAlways);
    public static readonly BoundsUse Dynamic = new(2, DisplayPipelineAttributes.BoundingBoxDisplayMode.OnDuringDynamicDisplay);
    internal DisplayPipelineAttributes.BoundingBoxDisplayMode Native { get; }
}

[SmartEnum<int>]
public sealed partial class DynamicUse {
    public static readonly DynamicUse Application = new(0, DisplayPipelineAttributes.DynamicDisplayUse.UseAppSettings);
    public static readonly DynamicUse BoundingBox = new(1, DisplayPipelineAttributes.DynamicDisplayUse.DisplayObjectBoundingBox);
    internal DisplayPipelineAttributes.DynamicDisplayUse Native { get; }
}

[SmartEnum<int>]
public sealed partial class GridPlaneUse {
    public static readonly GridPlaneUse WithGrid = new(0, DisplayPipelineAttributes.GridPlaneVisibilityMode.ShowOnlyIfGridVisible);
    public static readonly GridPlaneUse Always = new(1, DisplayPipelineAttributes.GridPlaneVisibilityMode.AlwaysShow);
    internal DisplayPipelineAttributes.GridPlaneVisibilityMode Native { get; }
}

[SmartEnum<int>]
public sealed partial class AxesColorUse {
    public static readonly AxesColorUse Application = new(0, DisplayPipelineAttributes.WorldAxesIconColorUse.UseApplicationSettings);
    public static readonly AxesColorUse GridAxes = new(1, DisplayPipelineAttributes.WorldAxesIconColorUse.SameAsGridAxesColors);
    public static readonly AxesColorUse Custom = new(2, DisplayPipelineAttributes.WorldAxesIconColorUse.Custom);
    internal DisplayPipelineAttributes.WorldAxesIconColorUse Native { get; }
}

[SmartEnum<int>]
public sealed partial class ScopeUse {
    public static readonly ScopeUse Document = new(
        key: 0,
        ground: DisplayPipelineAttributes.GroundPlaneUsages.ByDocument,
        workflow: DisplayPipelineAttributes.LinearWorkflowUsages.ByDocument);
    public static readonly ScopeUse Custom = new(
        key: 1,
        ground: DisplayPipelineAttributes.GroundPlaneUsages.Custom,
        workflow: DisplayPipelineAttributes.LinearWorkflowUsages.Custom);

    internal DisplayPipelineAttributes.GroundPlaneUsages Ground { get; }
    internal DisplayPipelineAttributes.LinearWorkflowUsages Workflow { get; }
}

[SmartEnum<int>]
public sealed partial class BackfaceUse {
    public static readonly BackfaceUse Front = new(0, DisplayPipelineAttributes.BackfaceStyle.UseFrontFaceSettings);
    public static readonly BackfaceUse Cull = new(1, DisplayPipelineAttributes.BackfaceStyle.CullBackfaces);
    public static readonly BackfaceUse ObjectColor = new(2, DisplayPipelineAttributes.BackfaceStyle.UseObjectColor);
    public static readonly BackfaceUse Solid = new(3, DisplayPipelineAttributes.BackfaceStyle.SingleColorAllBackfaces);
    public static readonly BackfaceUse RenderMaterial = new(4, DisplayPipelineAttributes.BackfaceStyle.UseRenderMaterial);
    public static readonly BackfaceUse CustomMaterial = new(5, DisplayPipelineAttributes.BackfaceStyle.CustomMaterialAllBackfaces);
    internal DisplayPipelineAttributes.BackfaceStyle Native { get; }
}

[SmartEnum<int>]
public sealed partial class LightingUse {
    public static readonly LightingUse None = new(0, DisplayPipelineAttributes.LightingSchema.None);
    public static readonly LightingUse Default = new(1, DisplayPipelineAttributes.LightingSchema.DefaultLighting);
    public static readonly LightingUse Scene = new(2, DisplayPipelineAttributes.LightingSchema.SceneLighting);
    public static readonly LightingUse Custom = new(3, DisplayPipelineAttributes.LightingSchema.CustomLighting);
    public static readonly LightingUse AmbientOcclusion = new(4, DisplayPipelineAttributes.LightingSchema.AmbientOcclusion);
    internal DisplayPipelineAttributes.LightingSchema Native { get; }
}

[SmartEnum<int>]
public sealed partial class ClippingFillUse {
    public static readonly ClippingFillUse Viewport = new(0, DisplayPipelineAttributes.ClippingPlaneFillColorUse.ViewportColor);
    public static readonly ClippingFillUse RenderMaterial = new(1, DisplayPipelineAttributes.ClippingPlaneFillColorUse.RenderMaterialColor);
    public static readonly ClippingFillUse PlaneMaterial = new(2, DisplayPipelineAttributes.ClippingPlaneFillColorUse.PlaneMaterialColor);
    public static readonly ClippingFillUse Solid = new(3, DisplayPipelineAttributes.ClippingPlaneFillColorUse.SolidColor);
    internal DisplayPipelineAttributes.ClippingPlaneFillColorUse Native { get; }
}

[SmartEnum<int>]
public sealed partial class ClippingEdgeUse {
    public static readonly ClippingEdgeUse Plane = new(0, DisplayPipelineAttributes.ClippingEdgeColorUse.PlaneColor);
    public static readonly ClippingEdgeUse Solid = new(1, DisplayPipelineAttributes.ClippingEdgeColorUse.SolidColor);
    public static readonly ClippingEdgeUse Object = new(2, DisplayPipelineAttributes.ClippingEdgeColorUse.ObjectColor);
    internal DisplayPipelineAttributes.ClippingEdgeColorUse Native { get; }
}

[SmartEnum<int>]
public sealed partial class LockedUse {
    public static readonly LockedUse Object = new(0, DisplayPipelineAttributes.LockedObjectUse.UseObjectAttributes);
    public static readonly LockedUse Specified = new(1, DisplayPipelineAttributes.LockedObjectUse.SpecifyColor);
    public static readonly LockedUse Application = new(2, DisplayPipelineAttributes.LockedObjectUse.UseAppSettings);
    internal DisplayPipelineAttributes.LockedObjectUse Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Fill {
    private Fill() { }
    public sealed record Default : Fill;
    public sealed record Solid(PerceptualColor Color) : Fill;
    public sealed record Gradient(PerceptualColor Top, PerceptualColor Bottom) : Fill;
    public sealed record Corners(PerceptualColor TopLeft, PerceptualColor BottomLeft, PerceptualColor TopRight, PerceptualColor BottomRight) : Fill;
    public sealed record Bitmap : Fill;
    public sealed record Renderer : Fill;
    public sealed record Transparent : Fill;

    internal static Fin<Fill> Of(DisplayPipelineAttributes source, Op key) => key.Catch(() => {
        source.GetFill(out System.Drawing.Color topLeft, out System.Drawing.Color bottomLeft, out System.Drawing.Color topRight, out System.Drawing.Color bottomRight);
        return (Mode: source.FillMode, Colors: toSeq([topLeft, bottomLeft, topRight, bottomRight]));
    }).Bind(row => row.Colors.TraverseM(color => PerceptualColor.OfRgb(color.R, color.G, color.B, color.A, key)).As()
        .Bind(colors => row.Mode switch {
            DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor => Fin.Succ<Fill>(new Default()),
            DisplayPipelineAttributes.FrameBufferFillMode.SolidColor => Fin.Succ<Fill>(new Solid(colors[0])),
            DisplayPipelineAttributes.FrameBufferFillMode.Gradient2Color => Fin.Succ<Fill>(new Gradient(colors[0], colors[1])),
            DisplayPipelineAttributes.FrameBufferFillMode.Gradient4Color => Fin.Succ<Fill>(new Corners(colors[0], colors[1], colors[2], colors[3])),
            DisplayPipelineAttributes.FrameBufferFillMode.Bitmap => Fin.Succ<Fill>(new Bitmap()),
            DisplayPipelineAttributes.FrameBufferFillMode.Renderer => Fin.Succ<Fill>(new Renderer()),
            DisplayPipelineAttributes.FrameBufferFillMode.Transparent => Fin.Succ<Fill>(new Transparent()),
            _ => Fin.Fail<Fill>(key.InvalidResult())
        }));
}

// --- [MODELS] -------------------------------------------------------------------------------
// The per-face override band and the shadow band are products with no discriminant, so each is one carrier its owning
// `Appearance` case holds rather than a per-flag parameter tail on the case itself.
public readonly record struct FaceOverride(bool Color, bool Transparency, bool Reflectivity) {
    public static FaceOverride None { get; } = new(Color: false, Transparency: false, Reflectivity: false);
}

[ComplexValueObject]
public sealed partial class ShadowBand {
    public bool On { get; }
    public PerceptualColor Color { get; }
    public int Intensity { get; }
    public int MemoryUsage { get; }
    public int SkylightQuality { get; }
    public int SoftEdgeQuality { get; }
    public double EdgeBlur { get; }
    public double Bias { get; }
    public int TransparencyTolerance { get; }
    public float ClippingRadius { get; }
    public bool IgnoreUserClipping { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref bool on,
        ref PerceptualColor color,
        ref int intensity,
        ref int memoryUsage,
        ref int skylightQuality,
        ref int softEdgeQuality,
        ref double edgeBlur,
        ref double bias,
        ref int transparencyTolerance,
        ref float clippingRadius,
        ref bool ignoreUserClipping) =>
        validationError = intensity >= 0
            && memoryUsage >= 0
            && skylightQuality >= 0
            && softEdgeQuality >= 0
            && transparencyTolerance >= 0
            && double.IsFinite(edgeBlur) && edgeBlur >= 0.0
            && double.IsFinite(bias)
            && float.IsFinite(clippingRadius) && clippingRadius >= 0f
                ? null
                : new ValidationError(message: "Shadow band carries a negative or non-finite quantity.");
}

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Appearance {
    private Appearance() { }
    public sealed record Shading(bool Enabled, bool VertexColors, bool Flat, bool AssignedMaterial, Option<PerceptualColor> ObjectColor, BackfaceUse Backface, bool CullBackfaces, double Shine, double Transparency, PerceptualColor Diffuse, PerceptualColor BackDiffuse, Fill Fill, bool Highlight, bool CustomMaterial, bool CustomMaterialBackfaces, bool BackfaceMaterial, double BackShine, double BackTransparency, FaceOverride Front, FaceOverride Back) : Appearance;
    public sealed record Edges(bool Curves, bool Surfaces, bool Naked, bool Isocurves, bool TangentEdges, bool TangentSeams, int Width, WidthUse WidthUse, float Scale, PerceptualColor Color, int ReductionPercent, bool Pattern, Option<PerceptualColor> SingleCurveColor, EdgeColorUse EdgeColorUse, IsoColorUse IsoColorUse, PerceptualColor IsoUv, PerceptualColor IsoU, PerceptualColor IsoV) : Appearance;
    public sealed record Lighting(LightingUse Scheme, PerceptualColor Ambient, bool UseLightColor, bool ShowLights, bool CastShadows, ShadowBand Shadows) : Appearance;
    public sealed record Ground(ScopeUse Usage, bool Enabled, bool ShowUnderside, double Altitude, PerceptualColor Color, bool Shadows, bool AutoAltitude) : Appearance;
    public sealed record Grid(bool GridVisible, bool Axes, bool WorldAxes, bool Transparent, bool OnTop, int ThinFrequency, int ThickFrequency, PerceptualColor Thin, PerceptualColor Thick, PerceptualColor X, PerceptualColor Y, PerceptualColor Z, int GridTransparency, int PlaneTransparency, GridPlaneUse PlaneVisibility, PerceptualColor PlaneColor, bool PlaneUsesGridColor, int AxesSizePercent, AxesColorUse AxesColorUse) : Appearance;
    public sealed record SubD(bool Smooth, bool Creases, bool NonManifold, bool Boundary, int Width, WidthUse WidthUse, float Scale, PerceptualColor SmoothColor, PerceptualColor CreaseColor, PerceptualColor NonManifoldColor, PerceptualColor BoundaryColor) : Appearance;
    public sealed record Mesh(bool Wires, bool Naked, bool NonManifold, bool Vertices, int Width, int VertexSize, PerceptualColor WireColor, PerceptualColor NakedColor, PerceptualColor NonManifoldColor) : Appearance;
    public sealed record Clipping(bool Planes, bool Fills, bool Edges, bool SectionStyles, bool IntersectionSurfaces, bool IntersectionEdges, ClippingFillUse FillUse, ClippingEdgeUse EdgeUse, PerceptualColor FillColor, PerceptualColor EdgeColor, int EdgeWidth, int ShadeTransparency) : Appearance;
    public sealed record Technical(bool Hidden, bool Edges, bool Silhouettes, bool Creases, bool Seams, bool Intersections, bool Lighting) : Appearance;
    public sealed record Locked(LockedUse Usage, PerceptualColor Color, int Transparency, bool Behind, bool Ghost, bool LayersFollowLock) : Appearance;
    public sealed record Points(bool Visible, PointUse PointStyle, float PointRadius, bool Clouds, PointUse CloudStyle, float CloudRadius) : Appearance;
    public sealed record Grips(bool Visible, bool Polygon, PointUse Style, int WireWidth, int Size, Option<PerceptualColor> FixedColor) : Appearance;
    public sealed record Fade(PerceptualColor Color, float Amount) : Appearance;
    public sealed record Dither(float Amount) : Appearance;
    public sealed record Hatch(float Strength, float Width) : Appearance;
    public sealed record Pipeline(bool Xray, bool IgnoreHighlights, bool DisableConduits, bool DisableTransparency, bool Text, bool Annotations, ScopeUse Workflow, float PreGamma, float PostGamma, bool BakeTextures, int RealtimePasses, bool RealtimeProgress, BoundsUse Bounds, DynamicUse Dynamic) : Appearance;

    internal bool Valid => Switch(
        shading: static row => row.Backface is not null
            && row.Fill is not null
            && ShineAxis.Editor.Shine(row.Shine)
            && ShineAxis.Editor.Shine(row.BackShine)
            && ShineAxis.Editor.Transparency(row.Transparency)
            && ShineAxis.Editor.Transparency(row.BackTransparency),
        edges: static row => row.WidthUse is not null
            && row.EdgeColorUse is not null
            && row.IsoColorUse is not null
            && row.Width > 0
            && float.IsFinite(row.Scale)
            && row.Scale > 0f
            && row.ReductionPercent is >= 0 and <= 100,
        lighting: static row => row.Scheme is not null && row.Shadows is not null,
        ground: static row => row.Usage is not null && double.IsFinite(row.Altitude),
        grid: static row => row.ThinFrequency > 0
            && row.ThickFrequency > 0
            && row.PlaneVisibility is not null
            && row.AxesColorUse is not null
            && row.GridTransparency is >= 0 and <= 255
            && row.PlaneTransparency is >= 0 and <= 255
            && row.AxesSizePercent > 0,
        subD: static row => row.WidthUse is not null && row.Width > 0 && float.IsFinite(row.Scale) && row.Scale > 0f,
        mesh: static row => row.Width > 0 && row.VertexSize > 0,
        clipping: static row => row.FillUse is not null
            && row.EdgeUse is not null
            && row.EdgeWidth > 0
            && row.ShadeTransparency is >= 0 and <= 100,
        technical: static _ => true,
        locked: static row => row.Usage is not null && row.Transparency is >= 0 and <= 100,
        points: static row => row.PointStyle is not null
            && row.CloudStyle is not null
            && float.IsFinite(row.PointRadius)
            && row.PointRadius > 0f
            && float.IsFinite(row.CloudRadius)
            && row.CloudRadius > 0f,
        grips: static row => row.Style is not null && row.WireWidth > 0 && row.Size > 0,
        fade: static row => float.IsFinite(row.Amount) && row.Amount is >= 0f and <= 1f,
        dither: static row => float.IsFinite(row.Amount) && row.Amount is >= 0f and <= 1f,
        hatch: static row => float.IsFinite(row.Strength)
            && row.Strength is >= 0f and <= 1f
            && float.IsFinite(row.Width)
            && row.Width > 0f,
        pipeline: static row => row.Workflow is not null
            && row.Bounds is not null
            && row.Dynamic is not null
            && float.IsFinite(row.PreGamma)
            && row.PreGamma > 0f
            && float.IsFinite(row.PostGamma)
            && row.PostGamma > 0f
            && row.RealtimePasses > 0);

    // Each concern writes under its own `Op`, so a mid-arm host throw carries the concern name rather than the batch's.
    internal static Fin<Unit> Write(Seq<Appearance> concerns, DisplayPipelineAttributes target, Op key) =>
        concerns.TraverseM(concern => Op.Of(name: $"{key.Value}.{concern.GetType().Name}")
            .Catch(() => Fin.Succ(concern.Write(target)))).As()
            .Map(static _ => unit);

    // The read half, case for case with the dispatch below: one reader per `Appearance` case in `Write`'s own order, so a
    // written mode reads back as the concern sequence that produced it and a caller can restore what a failed commit
    // half-wrote. Concerns whose case broadcasts one value across sibling host slots read the slot the case names first.
    internal static Fin<Seq<Appearance>> Of(DisplayPipelineAttributes source, Op key) =>
        Optional(source).ToFin(key.InvalidInput())
            .Bind(live => Readers.TraverseM(read => Op.Of(name: $"{key.Value}.{read.Method.Name}")
                .Catch(() => read(live, key))).As());

    private static readonly Seq<Func<DisplayPipelineAttributes, Op, Fin<Appearance>>> Readers =
        Seq<Func<DisplayPipelineAttributes, Op, Fin<Appearance>>>(
            ReadShading, ReadEdges, ReadLighting, ReadGround, ReadGrid, ReadSubD, ReadMesh, ReadClipping,
            ReadTechnical, ReadLocked, ReadPoints, ReadGrips, ReadFade, ReadDither, ReadHatch, ReadPipeline);

    private static Fin<PerceptualColor> Ink(System.Drawing.Color value, Op key) =>
        PerceptualColor.OfRgb(value.R, value.G, value.B, value.A, key);

    // A host boolean gates each optional colour slot, so the absent case is the flag being clear, never a sentinel colour.
    private static Fin<Option<PerceptualColor>> Ink(bool used, System.Drawing.Color value, Op key) =>
        used ? Ink(value, key).Map(Some) : Fin.Succ(Option<PerceptualColor>.None);

    internal Unit Write(DisplayPipelineAttributes target) => Switch(
        target,
        shading: static (a, row) => Write(a, row),
        edges: static (a, row) => Write(a, row),
        lighting: static (a, row) => Write(a, row),
        ground: static (a, row) => Write(a, row),
        grid: static (a, row) => Write(a, row),
        subD: static (a, row) => Write(a, row),
        mesh: static (a, row) => Write(a, row),
        clipping: static (a, row) => Write(a, row),
        technical: static (a, row) => Op.Side(() => {
            (a.ShowHiddenLines, a.ShowEdges, a.ShowSilhouttes, a.ShowCreases) = (row.Hidden, row.Edges, row.Silhouettes, row.Creases);
            (a.ShowSeams, a.ShowIntersections, a.ShowLighting) = (row.Seams, row.Intersections, row.Lighting);
        }),
        locked: static (a, row) => Op.Side(() => {
            (a.LockedObjectUsage, a.LockedColor, a.LockedObjectTransparency) = (row.Usage.Native, Quant.Sys(row.Color), row.Transparency);
            (a.LockedObjectsDrawBehindOthers, a.GhostLockedObjects, a.LayersFollowLockUsage) = (row.Behind, row.Ghost, row.LayersFollowLock);
        }),
        points: static (a, row) => Op.Side(() => {
            (a.ShowPoints, a.PointStyle, a.PointRadius) = (row.Visible, row.PointStyle.Native, row.PointRadius);
            (a.ShowPointClouds, a.PointCloudStyle, a.PointCloudRadius) = (row.Clouds, row.CloudStyle.Native, row.CloudRadius);
        }),
        grips: static (a, row) => Op.Side(() => {
            (a.ShowGrips, a.ControlPolygonShow, a.ControlPolygonStyle) = (row.Visible, row.Polygon, row.Style.Native);
            (a.ControlPolygonWireThickness, a.ControlPolygonGripSize) = (row.WireWidth, row.Size);
            a.ControlPolygonUseFixedSingleColor = row.FixedColor.IsSome;
            _ = row.FixedColor.Iter(color => a.ControlPolygonColor = Quant.Sys(color));
        }),
        fade: static (a, row) => Op.Side(() => a.SetColorFadeEffect(Quant.Sys(row.Color), row.Amount)),
        dither: static (a, row) => Op.Side(() => a.SetDitherTransparencyEffect(row.Amount)),
        hatch: static (a, row) => Op.Side(() => a.SetDiagonalHatchEffect(row.Strength, row.Width)),
        pipeline: static (a, row) => Write(a, row));

    private static Unit Write(DisplayPipelineAttributes a, Shading row) {
        (a.ShadingEnabled, a.ShadeVertexColors, a.FrontFlatShaded) = (row.Enabled, row.VertexColors, row.Flat);
        (a.UseAssignedObjectMaterial, a.UseCustomObjectColor) = (row.AssignedMaterial, row.ObjectColor.IsSome);
        _ = row.ObjectColor.Iter(color => a.ObjectColor = Quant.Sys(color));
        (a.BackfaceDisplayStyle, a.CullBackfaces, a.HighlightSurfaces) = (row.Backface.Native, row.CullBackfaces, row.Highlight);
        (a.UseCustomObjectMaterial, a.UseCustomObjectMaterialBackfaces, a.UseBackfaceMaterial) =
            (row.CustomMaterial, row.CustomMaterialBackfaces, row.BackfaceMaterial);
        (a.FrontMaterialShine, a.FrontMaterialTransparency, a.FrontDiffuse, a.BackMaterialDiffuseColor) =
            (row.Shine, row.Transparency, Quant.Sys(row.Diffuse), Quant.Sys(row.BackDiffuse));
        (a.BackMaterialShine, a.BackMaterialTransparency) = (row.BackShine, row.BackTransparency);
        (a.FrontOverrideObjectColor, a.FrontOverrideObjectTransparency, a.FrontOverrideObjectReflectivity) =
            (row.Front.Color, row.Front.Transparency, row.Front.Reflectivity);
        // Host truth: the back face publishes no colour-override slot, so only two of the three axes cross.
        (a.BackOverrideObjectTransparency, a.BackOverrideObjectReflectivity) = (row.Back.Transparency, row.Back.Reflectivity);
        return row.Fill.Switch(
            a,
            @default: static (target, _) => Op.Side(() => target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor),
            solid: static (target, fill) => Op.Side(() => { target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.SolidColor; target.SetFill(Quant.Sys(fill.Color)); }),
            gradient: static (target, fill) => Op.Side(() => { target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Gradient2Color; target.SetFill(Quant.Sys(fill.Top), Quant.Sys(fill.Bottom)); }),
            corners: static (target, fill) => Op.Side(() => { target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Gradient4Color; target.SetFill(Quant.Sys(fill.TopLeft), Quant.Sys(fill.BottomLeft), Quant.Sys(fill.TopRight), Quant.Sys(fill.BottomRight)); }),
            bitmap: static (target, _) => Op.Side(() => target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Bitmap),
            renderer: static (target, _) => Op.Side(() => target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Renderer),
            transparent: static (target, _) => Op.Side(() => target.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Transparent));
    }

    private static Unit Write(DisplayPipelineAttributes a, Edges row) {
        (a.ShowCurves, a.ShowSurfaceEdges, a.ShowSurfaceNakedEdge, a.ShowIsoCurves) = (row.Curves, row.Surfaces, row.Naked, row.Isocurves);
        (a.ShowTangentEdges, a.ShowTangentSeams) = (row.TangentEdges, row.TangentSeams);
        (a.CurveThickness, a.SurfaceEdgeThickness, a.SurfaceNakedEdgeThickness, a.SurfaceIsoThickness) = (row.Width, row.Width, row.Width, row.Width);
        (a.CurveThicknessScale, a.SurfaceEdgeThicknessScale, a.SurfaceNakedEdgeThicknessScale, a.SurfaceIsoThicknessScale) = (row.Scale, row.Scale, row.Scale, row.Scale);
        // Each usage writer takes its own nested vocabulary, so the shared row hands each its own native column.
        a.SetCurveThicknessUsage(row.WidthUse.Curve); a.SetSurfaceEdgeThicknessUsage(row.WidthUse.Surface);
        a.SetSurfaceNakedEdgeThicknessUsage(row.WidthUse.Naked); a.SetSurfaceIsoThicknessUsage(row.WidthUse.Iso);
        a.UseSingleCurveColor = row.SingleCurveColor.IsSome;
        _ = row.SingleCurveColor.Iter(color => a.CurveColor = Quant.Sys(color));
        (a.SurfaceEdgeColor, a.SurfaceNakedEdgeColor, a.SurfaceEdgeColorUsage) = (Quant.Sys(row.Color), Quant.Sys(row.Color), row.EdgeColorUse.Native);
        (a.SurfaceEdgeColorReduction, a.SurfaceNakedEdgeColorReduction) = (row.ReductionPercent, row.ReductionPercent);
        (a.SurfaceIsoUVColor, a.SurfaceIsoUColor, a.SurfaceIsoVColor) = (Quant.Sys(row.IsoUv), Quant.Sys(row.IsoU), Quant.Sys(row.IsoV));
        a.SetSurfaceIsoColorUsage(row.IsoColorUse.Native);
        a.SetSurfaceIsoApplyPattern(row.Pattern, row.Pattern, row.Pattern);
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Lighting row) {
        (a.LightingScheme, a.AmbientLightingColor, a.UseLightColor, a.ShowLights) = (row.Scheme.Native, Quant.Sys(row.Ambient), row.UseLightColor, row.ShowLights);
        a.CastShadows = row.CastShadows;
        (a.ShadowsOn, a.ShadowColor, a.ShadowIntensity, a.ShadowMemoryUsage) =
            (row.Shadows.On, Quant.Sys(row.Shadows.Color), row.Shadows.Intensity, row.Shadows.MemoryUsage);
        (a.SkylightShadowQuality, a.ShadowSoftEdgeQuality, a.ShadowEdgeBlur, a.ShadowBiasX) =
            (row.Shadows.SkylightQuality, row.Shadows.SoftEdgeQuality, row.Shadows.EdgeBlur, row.Shadows.Bias);
        (a.ShadowTransparencyTolerance, a.ShadowClippingRadius, a.ShadowsIgnoreUserDefinedClippingPlanes) =
            (row.Shadows.TransparencyTolerance, row.Shadows.ClippingRadius, row.Shadows.IgnoreUserClipping);
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Ground row) {
        (a.GroundPlaneUsage, a.CustomGroundPlaneEnabled, a.CustomGroundPlaneShowUnderside) = (row.Usage.Ground, row.Enabled, row.ShowUnderside);
        (a.CustomGroundPlaneAltitude, a.CustomGroundPlaneColor, a.CustomGroundPlaneShadowOnly, a.CustomGroundPlaneAutomaticAltitude) = (row.Altitude, Quant.Sys(row.Color), row.Shadows, row.AutoAltitude);
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Grid row) {
        (a.ViewSpecificAttributes.ShowGrid, a.ViewSpecificAttributes.ShowGridAxes, a.ViewSpecificAttributes.ShowWorldAxes) = (row.GridVisible, row.Axes, row.WorldAxes);
        (a.ViewSpecificAttributes.GridIsTransparent, a.ViewSpecificAttributes.GridDrawOnTop) = (row.Transparent, row.OnTop);
        (a.ViewSpecificAttributes.ThinGridLineFrequency, a.ViewSpecificAttributes.ThickGridLineFrequency) = (row.ThinFrequency, row.ThickFrequency);
        (a.ViewSpecificAttributes.ThinGridLineColor, a.ViewSpecificAttributes.ThickGridLineColor) = (Quant.Sys(row.Thin), Quant.Sys(row.Thick));
        (a.ViewSpecificAttributes.WorldAxisColorX, a.ViewSpecificAttributes.WorldAxisColorY, a.ViewSpecificAttributes.WorldAxisColorZ) = (Quant.Sys(row.X), Quant.Sys(row.Y), Quant.Sys(row.Z));
        // The top-level grid band sits on the attribute set itself, not the view-specific nest, so both write from one case.
        (a.GridTransparency, a.GridPlaneTransparency, a.GridPlaneVisibility) = (row.GridTransparency, row.PlaneTransparency, row.PlaneVisibility.Native);
        (a.GridPlaneColor, a.PlaneUsesGridColor) = (Quant.Sys(row.PlaneColor), row.PlaneUsesGridColor);
        (a.AxesSizePercentage, a.WorldAxesIconColorUsage) = (row.AxesSizePercent, row.AxesColorUse.Native);
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, SubD row) {
        (a.ShowSubDEdges, a.ShowSubDCreases, a.ShowSubDNonmanifoldEdges, a.ShowSubDBoundary) = (row.Smooth, row.Creases, row.NonManifold, row.Boundary);
        (a.SubDSmoothInteriorEdgeThickness, a.SubDCreaseInteriorEdgeThickness, a.SubDNonManifoldEdgeThickness, a.SubDBoundaryEdgeThickness) = (row.Width, row.Width, row.Width, row.Width);
        (a.SubDSmoothInteriorThicknessUsage, a.SubDCreaseInteriorThicknessUsage, a.SubDNonManifoldThicknessUsage, a.SubDBoundaryThicknessUsage) = (row.WidthUse.SubD, row.WidthUse.SubD, row.WidthUse.SubD, row.WidthUse.SubD);
        (a.SubDSmoothInteriorThicknessScale, a.SubDCreaseInteriorThicknessScale, a.SubDNonManifoldThicknessScale, a.SubDBoundaryThicknessScale) = (row.Scale, row.Scale, row.Scale, row.Scale);
        (a.SubDSmoothInteriorEdgeColor, a.SubDCreaseInteriorEdgeColor, a.SubDNonManifoldEdgeColor, a.SubDBoundaryEdgeColor) = (Quant.Sys(row.SmoothColor), Quant.Sys(row.CreaseColor), Quant.Sys(row.NonManifoldColor), Quant.Sys(row.BoundaryColor));
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Mesh row) {
        (a.ShowMeshEdges, a.ShowMeshNakedEdges, a.ShowMeshNonmanifoldEdges) = (row.Wires, row.Naked, row.NonManifold);
        (a.MeshEdgeThickness, a.MeshNakedEdgeThickness, a.MeshNonmanifoldEdgeThickness) = (row.Width, row.Width, row.Width);
        (a.MeshEdgeColor, a.MeshNakedEdgeColor, a.MeshNonmanifoldEdgeColor) = (Quant.Sys(row.WireColor), Quant.Sys(row.NakedColor), Quant.Sys(row.NonManifoldColor));
        (a.MeshVertexSize, a.MeshSpecificAttributes.ShowMeshVertices) = (row.VertexSize, row.Vertices);
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Clipping row) {
        (a.ShowClippingPlanes, a.ShowClippingFills, a.ShowClippingEdges, a.UseSectionStyles) = (row.Planes, row.Fills, row.Edges, row.SectionStyles);
        (a.ShowClipIntersectionSurfaces, a.ShowClipIntersectionEdges) = (row.IntersectionSurfaces, row.IntersectionEdges);
        (a.ClippingPlaneFillColorUsage, a.ClippingFillColor, a.ClippingShadeTransparency) = (row.FillUse.Native, Quant.Sys(row.FillColor), row.ShadeTransparency);
        (a.ClippingEdgeColorUsage, a.ClippingEdgeColor, a.ClippingEdgeThickness) = (row.EdgeUse.Native, Quant.Sys(row.EdgeColor), row.EdgeWidth);
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Pipeline row) {
        (a.XrayAllObjects, a.IgnoreHighlights, a.DisableConduits, a.DisableTransparency) = (row.Xray, row.IgnoreHighlights, row.DisableConduits, row.DisableTransparency);
        (a.ShowText, a.ShowAnnotations, a.LinearWorkflowUsage) = (row.Text, row.Annotations, row.Workflow.Workflow);
        (a.PreProcessGamma, a.PostProcessGamma, a.BakeTextures) = (row.PreGamma, row.PostGamma, row.BakeTextures);
        (a.RealtimeRenderPasses, a.ShowRealtimeRenderProgressBar) = (row.RealtimePasses, row.RealtimeProgress);
        (a.BoundingBoxMode, a.DynamicDisplayUsage) = (row.Bounds.Native, row.Dynamic.Native);
        return unit;
    }

    // --- [READERS]
    private static Fin<Appearance> ReadShading(DisplayPipelineAttributes a, Op key) =>
        from objectColor in Ink(a.UseCustomObjectColor, a.ObjectColor, key)
        from backface in key.Row(BackfaceUse.Items, a.BackfaceDisplayStyle, static row => row.Native)
        from diffuse in Ink(a.FrontDiffuse, key)
        from backDiffuse in Ink(a.BackMaterialDiffuseColor, key)
        from fill in Fill.Of(a, key)
        select (Appearance)new Shading(
            Enabled: a.ShadingEnabled, VertexColors: a.ShadeVertexColors, Flat: a.FrontFlatShaded,
            AssignedMaterial: a.UseAssignedObjectMaterial, ObjectColor: objectColor, Backface: backface,
            CullBackfaces: a.CullBackfaces, Shine: a.FrontMaterialShine, Transparency: a.FrontMaterialTransparency,
            Diffuse: diffuse, BackDiffuse: backDiffuse, Fill: fill, Highlight: a.HighlightSurfaces,
            CustomMaterial: a.UseCustomObjectMaterial, CustomMaterialBackfaces: a.UseCustomObjectMaterialBackfaces,
            BackfaceMaterial: a.UseBackfaceMaterial, BackShine: a.BackMaterialShine, BackTransparency: a.BackMaterialTransparency,
            Front: new FaceOverride(a.FrontOverrideObjectColor, a.FrontOverrideObjectTransparency, a.FrontOverrideObjectReflectivity),
            // Host truth: the back face publishes no colour-override slot, so the read answers the `false` the write skipped.
            Back: new FaceOverride(Color: false, a.BackOverrideObjectTransparency, a.BackOverrideObjectReflectivity));

    // The naked-edge column is the only one whose roster separates all three width rows, so the reverse lookup keys on it.
    // The case broadcasts one width, scale, reduction, colour, and pattern flag across the four edge families, so the read
    // answers the curve/edge slots the case names first and a round trip restores every slot the concern owns.
    private static Fin<Appearance> ReadEdges(DisplayPipelineAttributes a, Op key) =>
        from widthUse in key.Row(WidthUse.Items, a.GetSurfaceNakedEdgeThicknessUsage(), static row => row.Naked)
        from edgeUse in key.Row(EdgeColorUse.Items, a.SurfaceEdgeColorUsage, static row => row.Native)
        from isoUse in key.Row(IsoColorUse.Items, a.GetSurfaceIsoColorUsage(), static row => row.Native)
        from color in Ink(a.SurfaceEdgeColor, key)
        from single in Ink(a.UseSingleCurveColor, a.CurveColor, key)
        from isoUv in Ink(a.SurfaceIsoUVColor, key)
        from isoU in Ink(a.SurfaceIsoUColor, key)
        from isoV in Ink(a.SurfaceIsoVColor, key)
        from pattern in key.Catch(() => {
            a.GetSurfaceIsoApplyPattern(out bool u, out _, out _);
            return Fin.Succ(u);
        })
        select (Appearance)new Edges(
            Curves: a.ShowCurves, Surfaces: a.ShowSurfaceEdges, Naked: a.ShowSurfaceNakedEdge, Isocurves: a.ShowIsoCurves,
            TangentEdges: a.ShowTangentEdges, TangentSeams: a.ShowTangentSeams, Width: a.CurveThickness, WidthUse: widthUse,
            Scale: a.CurveThicknessScale, Color: color, ReductionPercent: a.SurfaceEdgeColorReduction, Pattern: pattern,
            SingleCurveColor: single, EdgeColorUse: edgeUse, IsoColorUse: isoUse, IsoUv: isoUv, IsoU: isoU, IsoV: isoV);

    private static Fin<Appearance> ReadLighting(DisplayPipelineAttributes a, Op key) =>
        from scheme in key.Row(LightingUse.Items, a.LightingScheme, static row => row.Native)
        from ambient in Ink(a.AmbientLightingColor, key)
        from shadow in Ink(a.ShadowColor, key)
        select (Appearance)new Lighting(
            Scheme: scheme, Ambient: ambient, UseLightColor: a.UseLightColor, ShowLights: a.ShowLights,
            CastShadows: a.CastShadows,
            Shadows: ShadowBand.Create(
                on: a.ShadowsOn, color: shadow, intensity: a.ShadowIntensity, memoryUsage: a.ShadowMemoryUsage,
                skylightQuality: a.SkylightShadowQuality, softEdgeQuality: a.ShadowSoftEdgeQuality,
                edgeBlur: a.ShadowEdgeBlur, bias: a.ShadowBiasX, transparencyTolerance: a.ShadowTransparencyTolerance,
                clippingRadius: a.ShadowClippingRadius, ignoreUserClipping: a.ShadowsIgnoreUserDefinedClippingPlanes));

    private static Fin<Appearance> ReadGround(DisplayPipelineAttributes a, Op key) =>
        from usage in key.Row(ScopeUse.Items, a.GroundPlaneUsage, static row => row.Ground)
        from color in Ink(a.CustomGroundPlaneColor, key)
        select (Appearance)new Ground(
            Usage: usage, Enabled: a.CustomGroundPlaneEnabled, ShowUnderside: a.CustomGroundPlaneShowUnderside,
            Altitude: a.CustomGroundPlaneAltitude, Color: color, Shadows: a.CustomGroundPlaneShadowOnly,
            AutoAltitude: a.CustomGroundPlaneAutomaticAltitude);

    private static Fin<Appearance> ReadGrid(DisplayPipelineAttributes a, Op key) =>
        from planeVisibility in key.Row(GridPlaneUse.Items, a.GridPlaneVisibility, static row => row.Native)
        from axesColorUse in key.Row(AxesColorUse.Items, a.WorldAxesIconColorUsage, static row => row.Native)
        from thin in Ink(a.ViewSpecificAttributes.ThinGridLineColor, key)
        from thick in Ink(a.ViewSpecificAttributes.ThickGridLineColor, key)
        from x in Ink(a.ViewSpecificAttributes.WorldAxisColorX, key)
        from y in Ink(a.ViewSpecificAttributes.WorldAxisColorY, key)
        from z in Ink(a.ViewSpecificAttributes.WorldAxisColorZ, key)
        from planeColor in Ink(a.GridPlaneColor, key)
        select (Appearance)new Grid(
            GridVisible: a.ViewSpecificAttributes.ShowGrid, Axes: a.ViewSpecificAttributes.ShowGridAxes,
            WorldAxes: a.ViewSpecificAttributes.ShowWorldAxes, Transparent: a.ViewSpecificAttributes.GridIsTransparent,
            OnTop: a.ViewSpecificAttributes.GridDrawOnTop, ThinFrequency: a.ViewSpecificAttributes.ThinGridLineFrequency,
            ThickFrequency: a.ViewSpecificAttributes.ThickGridLineFrequency, Thin: thin, Thick: thick, X: x, Y: y, Z: z,
            GridTransparency: a.GridTransparency, PlaneTransparency: a.GridPlaneTransparency,
            PlaneVisibility: planeVisibility, PlaneColor: planeColor, PlaneUsesGridColor: a.PlaneUsesGridColor,
            AxesSizePercent: a.AxesSizePercentage, AxesColorUse: axesColorUse);

    // Host truth: the SubD usage column carries only the two axes every family shares, so `InheritEdge` and `Object` write
    // identical SubD state and the read answers `Object` — the round trip preserves the state, not a row the host never held.
    private static Fin<Appearance> ReadSubD(DisplayPipelineAttributes a, Op key) =>
        from widthUse in key.Row(WidthUse.Items, a.SubDSmoothInteriorThicknessUsage, static row => row.SubD)
        from smooth in Ink(a.SubDSmoothInteriorEdgeColor, key)
        from crease in Ink(a.SubDCreaseInteriorEdgeColor, key)
        from nonManifold in Ink(a.SubDNonManifoldEdgeColor, key)
        from boundary in Ink(a.SubDBoundaryEdgeColor, key)
        select (Appearance)new SubD(
            Smooth: a.ShowSubDEdges, Creases: a.ShowSubDCreases, NonManifold: a.ShowSubDNonmanifoldEdges,
            Boundary: a.ShowSubDBoundary, Width: a.SubDSmoothInteriorEdgeThickness, WidthUse: widthUse,
            Scale: a.SubDSmoothInteriorThicknessScale, SmoothColor: smooth, CreaseColor: crease,
            NonManifoldColor: nonManifold, BoundaryColor: boundary);

    private static Fin<Appearance> ReadMesh(DisplayPipelineAttributes a, Op key) =>
        from wire in Ink(a.MeshEdgeColor, key)
        from naked in Ink(a.MeshNakedEdgeColor, key)
        from nonManifold in Ink(a.MeshNonmanifoldEdgeColor, key)
        select (Appearance)new Mesh(
            Wires: a.ShowMeshEdges, Naked: a.ShowMeshNakedEdges, NonManifold: a.ShowMeshNonmanifoldEdges,
            Vertices: a.MeshSpecificAttributes.ShowMeshVertices, Width: a.MeshEdgeThickness, VertexSize: a.MeshVertexSize,
            WireColor: wire, NakedColor: naked, NonManifoldColor: nonManifold);

    private static Fin<Appearance> ReadClipping(DisplayPipelineAttributes a, Op key) =>
        from fillUse in key.Row(ClippingFillUse.Items, a.ClippingPlaneFillColorUsage, static row => row.Native)
        from edgeUse in key.Row(ClippingEdgeUse.Items, a.ClippingEdgeColorUsage, static row => row.Native)
        from fill in Ink(a.ClippingFillColor, key)
        from edge in Ink(a.ClippingEdgeColor, key)
        select (Appearance)new Clipping(
            Planes: a.ShowClippingPlanes, Fills: a.ShowClippingFills, Edges: a.ShowClippingEdges,
            SectionStyles: a.UseSectionStyles, IntersectionSurfaces: a.ShowClipIntersectionSurfaces,
            IntersectionEdges: a.ShowClipIntersectionEdges, FillUse: fillUse, EdgeUse: edgeUse, FillColor: fill,
            EdgeColor: edge, EdgeWidth: a.ClippingEdgeThickness, ShadeTransparency: a.ClippingShadeTransparency);

    private static Fin<Appearance> ReadTechnical(DisplayPipelineAttributes a, Op key) =>
        key.Catch(() => Fin.Succ<Appearance>(new Technical(
            Hidden: a.ShowHiddenLines, Edges: a.ShowEdges, Silhouettes: a.ShowSilhouttes, Creases: a.ShowCreases,
            Seams: a.ShowSeams, Intersections: a.ShowIntersections, Lighting: a.ShowLighting)));

    private static Fin<Appearance> ReadLocked(DisplayPipelineAttributes a, Op key) =>
        from usage in key.Row(LockedUse.Items, a.LockedObjectUsage, static row => row.Native)
        from color in Ink(a.LockedColor, key)
        select (Appearance)new Locked(
            Usage: usage, Color: color, Transparency: a.LockedObjectTransparency,
            Behind: a.LockedObjectsDrawBehindOthers, Ghost: a.GhostLockedObjects, LayersFollowLock: a.LayersFollowLockUsage);

    private static Fin<Appearance> ReadPoints(DisplayPipelineAttributes a, Op key) =>
        from style in key.Row(PointUse.Items, a.PointStyle, static row => row.Native)
        from cloud in key.Row(PointUse.Items, a.PointCloudStyle, static row => row.Native)
        select (Appearance)new Points(
            Visible: a.ShowPoints, PointStyle: style, PointRadius: a.PointRadius,
            Clouds: a.ShowPointClouds, CloudStyle: cloud, CloudRadius: a.PointCloudRadius);

    private static Fin<Appearance> ReadGrips(DisplayPipelineAttributes a, Op key) =>
        from style in key.Row(PointUse.Items, a.ControlPolygonStyle, static row => row.Native)
        from fixedColor in Ink(a.ControlPolygonUseFixedSingleColor, a.ControlPolygonColor, key)
        select (Appearance)new Grips(
            Visible: a.ShowGrips, Polygon: a.ControlPolygonShow, Style: style,
            WireWidth: a.ControlPolygonWireThickness, Size: a.ControlPolygonGripSize, FixedColor: fixedColor);

    private static Fin<Appearance> ReadFade(DisplayPipelineAttributes a, Op key) =>
        key.Catch(() => {
            a.GetColorFadeEffect(out System.Drawing.Color color, out float amount);
            return Fin.Succ((Color: color, Amount: amount));
        }).Bind(row => Ink(row.Color, key).Map(color => (Appearance)new Fade(Color: color, Amount: row.Amount)));

    private static Fin<Appearance> ReadDither(DisplayPipelineAttributes a, Op key) =>
        key.Catch(() => Fin.Succ<Appearance>(new Dither(Amount: a.GetDitherTransparencyEffect())));

    private static Fin<Appearance> ReadHatch(DisplayPipelineAttributes a, Op key) =>
        key.Catch(() => {
            a.GetDiagonalHatchEffect(out float strength, out float width);
            return Fin.Succ<Appearance>(new Hatch(Strength: strength, Width: width));
        });

    private static Fin<Appearance> ReadPipeline(DisplayPipelineAttributes a, Op key) =>
        from workflow in key.Row(ScopeUse.Items, a.LinearWorkflowUsage, static row => row.Workflow)
        from bounds in key.Row(BoundsUse.Items, a.BoundingBoxMode, static row => row.Native)
        from dynamic in key.Row(DynamicUse.Items, a.DynamicDisplayUsage, static row => row.Native)
        select (Appearance)new Pipeline(
            Xray: a.XrayAllObjects, IgnoreHighlights: a.IgnoreHighlights, DisableConduits: a.DisableConduits,
            DisableTransparency: a.DisableTransparency, Text: a.ShowText, Annotations: a.ShowAnnotations,
            Workflow: workflow, PreGamma: a.PreProcessGamma, PostGamma: a.PostProcessGamma, BakeTextures: a.BakeTextures,
            RealtimePasses: a.RealtimeRenderPasses, RealtimeProgress: a.ShowRealtimeRenderProgressBar,
            Bounds: bounds, Dynamic: dynamic);
}
```

## [03]-[MODE_FAMILY]

- Owner: `ModeKind` carries built-in identity; `ModePlan` distinguishes editing an existing descriptor from deriving and persisting a copy.
- Law: each `ModePolicy` case carries one descriptor decision; a request sequence composes any unique subset independently from appearance concerns.
- Law: `ModeOp.FindCase` yields a detached host copy; appearance and policy writes remain local until `ModeOp.UpdateCase` persists the descriptor.
- Law: copied modes always pass through `ModeOp.UpdateCase`; an in-memory copy never becomes a successful receipt.
- Law: `ModeSummary` is `ModePolicy`'s read half — its `Policies` projection re-states the whole band as the case sequence that wrote it, so a policy band round-trips exactly as a concern set does and a compensating write needs no second reader.
- Growth: a built-in mode is one `ModeKind` row; a descriptor property is one `ModePolicy` case.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct ModeId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Mode identity is empty.") : null;
}

[SmartEnum<int>]
public sealed partial class ModeKind {
    public static readonly ModeKind Wireframe = Row(0, static () => ModeId.Create(DisplayModeDescription.WireframeId));
    public static readonly ModeKind Shaded = Row(1, static () => ModeId.Create(DisplayModeDescription.ShadedId));
    public static readonly ModeKind Rendered = Row(2, static () => ModeId.Create(DisplayModeDescription.RenderedId));
    public static readonly ModeKind RenderedShadows = Row(3, static () => ModeId.Create(DisplayModeDescription.RenderedShadowsId));
    public static readonly ModeKind Ghosted = Row(4, static () => ModeId.Create(DisplayModeDescription.GhostedId));
    public static readonly ModeKind XRay = Row(5, static () => ModeId.Create(DisplayModeDescription.XRayId));
    public static readonly ModeKind Technical = Row(6, static () => ModeId.Create(DisplayModeDescription.TechId));
    public static readonly ModeKind Artistic = Row(7, static () => ModeId.Create(DisplayModeDescription.ArtisticId));
    public static readonly ModeKind Pen = Row(8, static () => ModeId.Create(DisplayModeDescription.PenId));
    public static readonly ModeKind Monochrome = Row(9, static () => ModeId.Create(DisplayModeDescription.MonochromeId));
    public static readonly ModeKind AmbientOcclusion = Row(10, static () => ModeId.Create(DisplayModeDescription.AmbientOcclusionId));
    public static readonly ModeKind Raytraced = Row(11, static () => ModeId.Create(DisplayModeDescription.RaytracedId));

    private static ModeKind Row(int key, Func<ModeId> id) => new(key, id);

    [UseDelegateFromConstructor]
    public partial ModeId Id();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModePolicy {
    private ModePolicy() { }
    public sealed record Name(string Value) : ModePolicy;
    public sealed record InMenu(bool Value) : ModePolicy;
    public sealed record ShadeCommand(bool Value) : ModePolicy;
    public sealed record Shading(bool Value) : ModePolicy;
    public sealed record ObjectAssignment(bool Value) : ModePolicy;
    public sealed record ShadedPipeline(bool Value) : ModePolicy;
    public sealed record WireframePipeline(bool Value) : ModePolicy;
    public sealed record PipelineLocked(bool Value) : ModePolicy;

    internal bool Valid => Switch(
        name: static row => !string.IsNullOrWhiteSpace(row.Value),
        inMenu: static _ => true,
        shadeCommand: static _ => true,
        shading: static _ => true,
        objectAssignment: static _ => true,
        shadedPipeline: static _ => true,
        wireframePipeline: static _ => true,
        pipelineLocked: static _ => true);

    private Unit Write(DisplayModeDescription mode) => Switch(
        mode,
        name: static (target, row) => Op.Side(() => target.EnglishName = row.Value),
        inMenu: static (target, row) => Op.Side(() => target.InMenu = row.Value),
        shadeCommand: static (target, row) => Op.Side(() => target.SupportsShadeCommand = row.Value),
        shading: static (target, row) => Op.Side(() => target.SupportsShading = row.Value),
        objectAssignment: static (target, row) => Op.Side(() => target.AllowObjectAssignment = row.Value),
        shadedPipeline: static (target, row) => Op.Side(() => target.ShadedPipelineRequired = row.Value),
        wireframePipeline: static (target, row) => Op.Side(() => target.WireframePipelineRequired = row.Value),
        pipelineLocked: static (target, row) => Op.Side(() => target.PipelineLocked = row.Value));

    internal static Fin<Unit> Write(Seq<ModePolicy> policies, DisplayModeDescription mode, Op key) =>
        policies.TraverseM(policy => key.Catch(() => Fin.Succ(policy.Write(mode)))).As().Map(static _ => unit);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModePlan {
    private ModePlan() { }
    public sealed record Existing(ModeId Id) : ModePlan;
    public sealed record Derived(ModeId Source, string Name) : ModePlan;

    internal bool Valid => Switch(
        existing: static row => row.Id.Value != Guid.Empty,
        derived: static row => row.Source.Value != Guid.Empty && !string.IsNullOrWhiteSpace(row.Name));
}
```

## [04]-[MODE_TABLE]

- Owner: `ModeOp` is the closed table request family consumed by `Modes.Configure`.
- Entry: `ModeOp.Apply` returns resolved descriptors for every operation, including host-minted identities.
- Law: every minted identifier re-resolves before egress; a dangling identifier never becomes a descriptor receipt.
- Law: every case has a reachable public ingress on `ModeRequest`, so the vocabulary and the entry family agree case for case; a table verb whose only argument is a live `DisplayModeDescription` has no admissible ingress across the receipt boundary and does not exist here — `AddDisplayMode(string)` mints through `BlankCase` and `UpdateDisplayMode` persists a descriptor the fold already resolved.
- Law: the host splits its table verbs by return shape — the four minting verbs answer a `Guid` that is `Guid.Empty` on refusal, and the three persisting verbs answer a `bool` — so `Mint` folds the identifier verbs onto one re-resolving rail and `Confirm` folds the boolean ones, and no arm reads a return the host does not publish.
- Growth: a table verb is one request case, one dispatch arm, and one `ModeRequest` entry case in the same pass.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record ModeOp {
    private ModeOp() { }
    internal sealed record CensusCase : ModeOp;
    internal sealed record FindCase(ModeId Mode) : ModeOp;
    internal sealed record NamedCase(string Name) : ModeOp;
    internal sealed record BlankCase(string Name) : ModeOp;
    internal sealed record UpdateCase(DisplayModeDescription Mode) : ModeOp;
    internal sealed record CopyCase(ModeId Source, string Name) : ModeOp;
    internal sealed record DeleteCase(ModeId Mode) : ModeOp;
    internal sealed record ImportCase(string Path, bool Interactive) : ModeOp;
    internal sealed record ExportCase(ModeId Mode, string Path) : ModeOp;

    internal Fin<Seq<DisplayModeDescription>> Apply(Op? key = null) {
        Op op = key.OrDefault();
        return Switch(
            op,
            censusCase: static (inner, _) => inner.Catch(() => Fin.Succ(toSeq(DisplayModeDescription.GetDisplayModes()))),
            findCase: static (inner, row) => Resolve(row.Mode, inner).Map(static mode => Seq(mode)),
            namedCase: static (inner, row) => inner.Catch(() =>
                Optional(DisplayModeDescription.FindByName(row.Name)).ToFin(inner.InvalidInput()).Map(static mode => Seq(mode))),
            blankCase: static (inner, row) => Mint(() => DisplayModeDescription.AddDisplayMode(row.Name), inner),
            updateCase: static (inner, row) => inner.Catch(() =>
                inner.Confirm(DisplayModeDescription.UpdateDisplayMode(row.Mode)).Map(_ => Seq(row.Mode))),
            copyCase: static (inner, row) => Mint(() => DisplayModeDescription.CopyDisplayMode(row.Source.Value, row.Name), inner),
            deleteCase: static (inner, row) => Resolve(row.Mode, inner)
                .Bind(mode => inner.Catch(() => inner.Confirm(DisplayModeDescription.DeleteDisplayMode(row.Mode.Value)).Map(_ => Seq(mode)))),
            importCase: static (inner, row) => Mint(() => DisplayModeDescription.ImportFromFile(row.Path, row.Interactive), inner),
            exportCase: static (inner, row) => Resolve(row.Mode, inner)
                .Bind(mode => inner.Catch(() => inner.Confirm(DisplayModeDescription.ExportToFile(mode, row.Path)).Map(_ => Seq(mode)))));
    }

    private static Fin<DisplayModeDescription> Resolve(ModeId id, Op key) =>
        key.Catch(() => Optional(DisplayModeDescription.GetDisplayMode(id.Value)).ToFin(key.InvalidInput()));

    private static Fin<Seq<DisplayModeDescription>> Mint(Func<Guid> mint, Op key) =>
        key.Catch(() => mint() is var id && id != Guid.Empty
            ? Resolve(ModeId.Create(id), key).Map(static mode => Seq(mode))
            : Fin.Fail<Seq<DisplayModeDescription>>(key.InvalidResult()));
}
```

## [05]-[CONFIGURE]

- Owner: `ModeRequest` is the complete ingress family and `ModeReceipt` is the detached egress family.
- Entry: `Modes.Configure` dispatches every modality; request shape carries singular, batch, query, and capture intent without flags or sibling verbs.
- Law: the ingress family covers every `ModeOp` case, so the table vocabulary and the public entry agree case for case — census, name lookup, blank mint, retire, `.ini` import, and `.ini` export each reach a consumer, and a table verb whose only argument is a live descriptor has no admissible ingress and does not exist.
- Law: a descriptor never crosses the receipt boundary; `ModeSummary` is the detached projection every query answers with, carrying identity, both names, and the whole policy band.
- Law: analysis attachment admits a unique requested set, separates requested and changed subjects in the receipt, and restores a failed prefix in reverse while retaining every cleanup fault.
- Growth: `AnalysisKind` carries every built-in host analysis identity as a table row.
- Law: `Apply` runs ONE write path for both plan cases and a written descriptor IS recoverable — `Appearance.Of` and `ModeSummary.Of` read the whole concern and policy state back, so the commit captures the prior state before its first write and replays it onto the live descriptor when any stage fails, folding a failed restoration onto the primary fault. A staging clone stays unspellable because the host publishes no `DisplayPipelineAttributes` clone or assign, and it would buy nothing the compensating read does not: a derived plan deletes its own minted copy, and an existing plan holds no such copy to delete.
- Boundary: UI adjustment and analysis-dialog requests demand dialog capability; bitmap custody exits only as `CaptureArtifact`.
- Growth: a new mode operation is one request case, one `ModeOp` case, and one receipt projection inside the existing dispatch.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct AnalysisId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Analysis identity is empty.") : null;
}

[SmartEnum<int>]
public sealed partial class AnalysisKind {
    public static readonly AnalysisKind Edge = Row(0, static () => AnalysisId.Create(VisualAnalysisMode.RhinoEdgeAnalysisModeId));
    public static readonly AnalysisKind CurvatureGraph = Row(1, static () => AnalysisId.Create(VisualAnalysisMode.RhinoCurvatureGraphAnalysisModeId));
    public static readonly AnalysisKind Zebra = Row(2, static () => AnalysisId.Create(VisualAnalysisMode.RhinoZebraStripeAnalysisModeId));
    public static readonly AnalysisKind Emap = Row(3, static () => AnalysisId.Create(VisualAnalysisMode.RhinoEmapAnalysisModeId));
    public static readonly AnalysisKind CurvatureColor = Row(4, static () => AnalysisId.Create(VisualAnalysisMode.RhinoCurvatureColorAnalyisModeId));
    public static readonly AnalysisKind DraftAngle = Row(5, static () => AnalysisId.Create(VisualAnalysisMode.RhinoDraftAngleAnalysisModeId));
    public static readonly AnalysisKind Thickness = Row(6, static () => AnalysisId.Create(VisualAnalysisMode.RhinoThicknessAnalysisModeId));
    public static readonly AnalysisKind EdgeContinuity = Row(7, static () => AnalysisId.Create(VisualAnalysisMode.RhinoEdgeContinuityAlalysisModeId));
    public static readonly AnalysisKind Direction = Row(8, static () => AnalysisId.Create(VisualAnalysisMode.RhinoDirectionAnalysisModeId));
    public static readonly AnalysisKind End = Row(9, static () => AnalysisId.Create(VisualAnalysisMode.RhinoEndAnalysisModeId));

    private static AnalysisKind Row(int key, Func<AnalysisId> id) => new(key, id);

    [UseDelegateFromConstructor]
    public partial AnalysisId Id();
}

[SmartEnum<int>]
public sealed partial class AnalysisState {
    public static readonly AnalysisState Detached = new(key: 0, enabled: false);
    public static readonly AnalysisState Attached = new(key: 1, enabled: true);

    internal bool Enabled { get; }
}

[SmartEnum<int>]
public sealed partial class CurvatureRange {
    public static readonly CurvatureRange Automatic = new(key: 0, apply: static () => Op.Side(VisualAnalysisMode.CurvatureColorAutoRange));
    public static readonly CurvatureRange Maximum = new(key: 1, apply: static () => Op.Side(VisualAnalysisMode.CurvatureColorMaxRange));

    [UseDelegateFromConstructor]
    internal partial Unit Apply();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalysisEdit {
    private AnalysisEdit() { }
    public sealed record Set(Seq<Guid> Objects, AnalysisKind Kind, AnalysisState State) : AnalysisEdit;
    public sealed record Census(Guid Object) : AnalysisEdit;
    public sealed record AdjustMeshes(AnalysisKind Kind) : AnalysisEdit;
    public sealed record UserInterface(AnalysisKind Kind, bool Visible) : AnalysisEdit;
    public sealed record Range(CurvatureRange Value) : AnalysisEdit;

    internal bool Valid => Switch(
        set: static row => !row.Objects.IsEmpty
            && row.Objects.ForAll(static id => id != Guid.Empty)
            && row.Objects.Distinct().Count == row.Objects.Count
            && row.Kind is not null
            && row.State is not null,
        census: static row => row.Object != Guid.Empty,
        adjustMeshes: static row => row.Kind is not null,
        userInterface: static row => row.Kind is not null,
        range: static row => row.Value is not null);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModeRequest {
    private ModeRequest() { }
    public sealed record Apply(ModePlan Plan, Seq<ModePolicy> Policies, Seq<Appearance> Concerns) : ModeRequest;
    public sealed record Bind(DocumentSession Session, ViewportTarget Target, ModeId Mode) : ModeRequest;
    public sealed record Inspect(DocumentSession Session, ViewportTarget Target) : ModeRequest;
    public sealed record Capture(DocumentSession Session, ViewportTarget Target, ModeId Mode, Option<Size2i> Extent) : ModeRequest;
    public sealed record Analyze(DocumentSession Session, AnalysisEdit Edit) : ModeRequest;
    public sealed record Census : ModeRequest;
    public sealed record Named(string Name) : ModeRequest;
    public sealed record Mint(string Name) : ModeRequest;
    public sealed record Retire(ModeId Mode) : ModeRequest;
    public sealed record Import(string Path, bool Interactive) : ModeRequest;
    public sealed record Export(ModeId Mode, string Path) : ModeRequest;

    internal bool Valid => Switch(
        apply: static row => row.Plan is not null
            && row.Plan.Valid
            && row.Policies.ForAll(static policy => policy is not null && policy.Valid)
            && Cases.Unique(row.Policies)
            && row.Concerns.ForAll(static concern => concern is not null && concern.Valid)
            && Cases.Unique(row.Concerns),
        bind: static row => row.Session is not null && row.Target is not null && row.Mode.Value != Guid.Empty,
        inspect: static row => row.Session is not null && row.Target is not null,
        capture: static row => row.Session is not null
            && row.Target is not null
            && row.Mode.Value != Guid.Empty
            && row.Extent.Match(Some: static size => size.IsValid, None: static () => true),
        analyze: static row => row.Session is not null && row.Edit is not null && row.Edit.Valid,
        census: static _ => true,
        named: static row => !string.IsNullOrWhiteSpace(row.Name),
        mint: static row => !string.IsNullOrWhiteSpace(row.Name),
        retire: static row => row.Mode.Value != Guid.Empty,
        import: static row => !string.IsNullOrWhiteSpace(row.Path),
        export: static row => row.Mode.Value != Guid.Empty && !string.IsNullOrWhiteSpace(row.Path));

}

// --- [MODELS] -------------------------------------------------------------------------------
// The detached descriptor projection: identity, both names, and the policy band, so a census answers without a host handle.
public readonly record struct ModeSummary(
    ModeId Id,
    string Name,
    string LocalName,
    bool InMenu,
    bool SupportsShadeCommand,
    bool SupportsShading,
    bool AllowObjectAssignment,
    bool ShadedPipelineRequired,
    bool WireframePipelineRequired,
    bool PipelineLocked) {
    internal static ModeSummary Of(DisplayModeDescription mode) => new(
        Id: ModeId.Create(mode.Id),
        Name: mode.EnglishName,
        LocalName: mode.LocalName,
        InMenu: mode.InMenu,
        SupportsShadeCommand: mode.SupportsShadeCommand,
        SupportsShading: mode.SupportsShading,
        AllowObjectAssignment: mode.AllowObjectAssignment,
        ShadedPipelineRequired: mode.ShadedPipelineRequired,
        WireframePipelineRequired: mode.WireframePipelineRequired,
        PipelineLocked: mode.PipelineLocked);

    // The read half of `ModePolicy`: the whole band re-stated as the case sequence that wrote it, so a compensating
    // write replays a summary directly and the localized name — which no policy case writes — stays out of it.
    internal Seq<ModePolicy> Policies => Seq<ModePolicy>(
        new ModePolicy.Name(Name),
        new ModePolicy.InMenu(InMenu),
        new ModePolicy.ShadeCommand(SupportsShadeCommand),
        new ModePolicy.Shading(SupportsShading),
        new ModePolicy.ObjectAssignment(AllowObjectAssignment),
        new ModePolicy.ShadedPipeline(ShadedPipelineRequired),
        new ModePolicy.WireframePipeline(WireframePipelineRequired),
        new ModePolicy.PipelineLocked(PipelineLocked));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModeReceipt : IDetachedDocumentResult {
    private ModeReceipt() { }
    public sealed record Configured(ModeId Mode) : ModeReceipt;
    public sealed record Bound(ModeId Mode) : ModeReceipt;
    public sealed record Inspected(ModeId Mode, Seq<Appearance> Concerns, Seq<ModePolicy> Policies) : ModeReceipt;
    public sealed record Captured(CaptureArtifact Artifact) : ModeReceipt;
    public sealed record Resolved(Seq<ModeSummary> Modes) : ModeReceipt;
    public sealed record Retired(ModeId Mode) : ModeReceipt;
    public sealed record Exported(ModeId Mode, string Path) : ModeReceipt;
    public sealed record AnalysisChanged(Seq<Guid> Requested, Seq<Guid> Changed, AnalysisId Mode, AnalysisState State) : ModeReceipt;
    public sealed record AnalysisCensus(Guid Object, Seq<AnalysisId> Active) : ModeReceipt;
    public sealed record AnalysisAdjusted(AnalysisId Mode) : ModeReceipt;
    public sealed record AnalysisInterface(AnalysisId Mode, bool Visible) : ModeReceipt;
    public sealed record AnalysisRange(CurvatureRange Value) : ModeReceipt;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Modes {
    public static Fin<ModeReceipt> Configure(ModeRequest request, Op? key = null) {
        Op op = key.OrDefault();
        return guard(request is not null && request.Valid, op.InvalidInput()).ToFin().Bind(_ => request.Switch(
            op,
            apply: static (op, row) => row.Plan.Switch(
                (Policies: row.Policies, Concerns: row.Concerns, Op: op),
                existing: static (held, plan) => Resolve(plan.Id, held.Op)
                    .Bind(mode => Commit(mode, held.Policies, held.Concerns, held.Op)),
                derived: static (held, plan) => new ModeOp.CopyCase(plan.Source, plan.Name).Apply(held.Op)
                    .Bind(modes => modes.Head.ToFin(held.Op.InvalidResult()))
                    .Bind(mode => Commit(mode, held.Policies, held.Concerns, held.Op)
                        .BindFail(failure => new ModeOp.DeleteCase(ModeId.Create(mode.Id)).Apply(held.Op).Match(
                            Succ: _ => Fin.Fail<ModeReceipt>(failure),
                            Fail: cleanup => Fin.Fail<ModeReceipt>(failure + cleanup))))),
            bind: static (op, row) => Resolve(row.Mode, op)
                .Bind(mode => ViewportLease.Of(row.Session, row.Target, op)
                    .Bind(lease => lease.Use(borrow => op.Catch(() => Fin.Succ((borrow.Viewport.DisplayMode = mode, unit).Item2)), op)))
                .Map(_ => (ModeReceipt)new ModeReceipt.Bound(row.Mode)),
            inspect: static (op, row) => ViewportLease.Of(row.Session, row.Target, op)
                .Bind(lease => lease.Use(borrow => op.Catch(() => Optional(borrow.Viewport.DisplayMode).ToFin(op.InvalidResult()))
                    .Bind(mode => Appearance.Of(mode.DisplayAttributes, op).Map(concerns => (
                        Mode: ModeId.Create(mode.Id),
                        Concerns: concerns,
                        Policies: ModeSummary.Of(mode).Policies))), op))
                .Map(state => (ModeReceipt)new ModeReceipt.Inspected(state.Mode, state.Concerns, state.Policies)),
            capture: static (op, row) => Resolve(row.Mode, op)
                .Bind(mode => ViewportLease.Of(row.Session, row.Target, op)
                    .Bind(lease => lease.Use(borrow => op.Catch(() => Optional(row.Extent.Match(
                        Some: size => borrow.View.CaptureToBitmap(size.Native, mode),
                        None: () => borrow.View.CaptureToBitmap(mode))).ToFin(op.InvalidResult())), op)))
                .Bind(bitmap => CaptureArtifact.Raster(bitmap, op))
                .Map(artifact => (ModeReceipt)new ModeReceipt.Captured(artifact)),
            analyze: static (op, row) => Analyze(row.Session, row.Edit, op),
            census: static (op, _) => Summarize(new ModeOp.CensusCase(), op),
            named: static (op, row) => Summarize(new ModeOp.NamedCase(row.Name), op),
            mint: static (op, row) => Summarize(new ModeOp.BlankCase(row.Name), op),
            retire: static (op, row) => new ModeOp.DeleteCase(row.Mode).Apply(op)
                .Map(_ => (ModeReceipt)new ModeReceipt.Retired(row.Mode)),
            import: static (op, row) => Summarize(new ModeOp.ImportCase(row.Path, row.Interactive), op),
            export: static (op, row) => new ModeOp.ExportCase(row.Mode, row.Path).Apply(op)
                .Map(_ => (ModeReceipt)new ModeReceipt.Exported(row.Mode, row.Path))));
    }

    private static Fin<DisplayModeDescription> Resolve(ModeId id, Op key) =>
        new ModeOp.FindCase(id).Apply(key).Bind(modes => modes.Head.ToFin(key.InvalidResult()));

    private static Fin<ModeReceipt> Summarize(ModeOp op, Op key) =>
        op.Apply(key).Bind(modes => key.Catch(() =>
            Fin.Succ<ModeReceipt>(new ModeReceipt.Resolved(modes.Map(ModeSummary.Of).Strict()))));

    // The read half is what makes the write recoverable: the prior concern set and policy band are captured before the
    // first write, so a failed appearance write, policy write, or persist replays them onto the live descriptor. A
    // failed replay folds onto the primary fault rather than replacing it, and the caller's next commit starts from a
    // descriptor it can read. One write path serves both plans; the derived plan's own minted copy is deleted above.
    private static Fin<ModeReceipt> Commit(DisplayModeDescription mode, Seq<ModePolicy> policies, Seq<Appearance> concerns, Op key) =>
        from prior in Appearance.Of(mode.DisplayAttributes, key)
        from band in key.Catch(() => Fin.Succ(ModeSummary.Of(mode).Policies))
        from receipt in (from _ in Appearance.Write(concerns, mode.DisplayAttributes, key)
                         from __ in ModePolicy.Write(policies, mode, key)
                         from ___ in new ModeOp.UpdateCase(mode).Apply(key)
                         select (ModeReceipt)new ModeReceipt.Configured(ModeId.Create(mode.Id)))
            .BindFail(failure => Restore(mode, prior, band, key).Match(
                Succ: _ => Fin.Fail<ModeReceipt>(failure),
                Fail: cleanup => Fin.Fail<ModeReceipt>(failure + cleanup)))
        select receipt;

    private static Fin<Unit> Restore(DisplayModeDescription mode, Seq<Appearance> concerns, Seq<ModePolicy> policies, Op key) =>
        Appearance.Write(concerns, mode.DisplayAttributes, key).Bind(_ => ModePolicy.Write(policies, mode, key));

    private static Fin<ModeReceipt> Analyze(DocumentSession session, AnalysisEdit edit, Op key) => edit.Switch(
        (Session: session, Op: key),
        set: static (ctx, row) => Set(ctx.Session, row.Objects, row.Kind, row.State, ctx.Op),
        census: static (ctx, row) => ctx.Session.Demand(
            document => ctx.Op.Catch(() => Optional(document.Objects.FindId(row.Object)).ToFin(ctx.Op.InvalidInput())
                .Map(subject => (ModeReceipt)new ModeReceipt.AnalysisCensus(
                    row.Object,
                    toSeq(subject.GetActiveVisualAnalysisModes()).Map(static mode => AnalysisId.Create(mode.Id))))),
            ctx.Op,
            [SessionNeed.Read]),
        adjustMeshes: static (ctx, row) => ctx.Session.Demand(
            document => Analysis(row.Kind, ctx.Op).Bind(mode => ctx.Op.Catch(() =>
                ctx.Op.Confirm(VisualAnalysisMode.AdjustAnalysisMeshes(document, mode.Id)))
                .Map(_ => (ModeReceipt)new ModeReceipt.AnalysisAdjusted(AnalysisId.Create(mode.Id)))),
            ctx.Op,
            [SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Dialog]),
        userInterface: static (ctx, row) => ctx.Session.Demand(
            _ => Analysis(row.Kind, ctx.Op)
                .Bind(mode => ctx.Op.Catch(() => Fin.Succ((
                    Op.Side(() => mode.EnableUserInterface(row.Visible)),
                    (ModeReceipt)new ModeReceipt.AnalysisInterface(AnalysisId.Create(mode.Id), row.Visible)).Item2))),
            ctx.Op,
            [SessionNeed.Dialog]),
        range: static (ctx, row) => ctx.Session.Demand(
            _ => ctx.Op.Catch(() => Fin.Succ((row.Value.Apply(), (ModeReceipt)new ModeReceipt.AnalysisRange(row.Value)).Item2)),
            ctx.Op,
            [SessionNeed.Dialog]));

    private static Fin<ModeReceipt> Set(DocumentSession session, Seq<Guid> objects, AnalysisKind kind, AnalysisState state, Op key) =>
        session.Demand(
            document => Analysis(kind, key).Bind(mode => objects.TraverseM(id => key.Catch(() =>
                    from subject in Optional(document.Objects.FindId(id)).ToFin(key.InvalidInput())
                    from _ in guard(mode.ObjectSupportsAnalysisMode(subject), key.InvalidInput()).ToFin()
                    let prior = toSeq(subject.GetActiveVisualAnalysisModes()).Exists(active => active.Id == mode.Id)
                    select (Id: id, Subject: subject, Prior: prior))).As()
                .Bind(subjects => subjects.Fold(
                    Fin.Succ(Seq<(Guid Id, RhinoObject Subject, bool Prior)>()),
                    (applied, row) => applied.Bind(done => row.Prior == state.Enabled
                        ? Fin.Succ(done)
                        : key.Catch(() => key.Confirm(row.Subject.EnableVisualAnalysisMode(mode, state.Enabled)))
                            .Map(_ => done.Add(row))
                            .BindFail(failure => Compensate(document, done, mode, failure, key)))))
                .Bind(touched => key.Catch(() => {
                    document.Views.Redraw();
                    return Fin.Succ<ModeReceipt>(new ModeReceipt.AnalysisChanged(
                        subjects.Map(static row => row.Id),
                        touched.Map(static row => row.Id),
                        AnalysisId.Create(mode.Id),
                        state));
                }))),
            key,
            [SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Redraw]);

    private static Fin<Seq<(Guid Id, RhinoObject Subject, bool Prior)>> Compensate(
        RhinoDoc document,
        Seq<(Guid Id, RhinoObject Subject, bool Prior)> applied,
        VisualAnalysisMode mode,
        Error primary,
        Op key) {
        Seq<Error> rollback = toSeq(applied.AsEnumerable().Reverse())
            .Choose(row => key.Catch(() => key.Confirm(row.Subject.EnableVisualAnalysisMode(mode, row.Prior))).Match(
                Succ: static _ => Option<Error>.None,
                Fail: static failure => Some(failure)))
            .Strict();
        Seq<Error> cleanup = rollback + key.Catch(() => { document.Views.Redraw(); return Fin.Succ(unit); }).Match(
            Succ: static _ => Seq<Error>(),
            Fail: static failure => Seq(failure));
        return Fin.Fail<Seq<(Guid Id, RhinoObject Subject, bool Prior)>>(cleanup.IsEmpty
            ? primary
            : primary + cleanup.Fold(Errors.None, static (folded, failure) => folded + failure));
    }

    private static Fin<VisualAnalysisMode> Analysis(AnalysisKind kind, Op key) =>
        key.Catch(() => Optional(VisualAnalysisMode.Find(kind.Id())).ToFin(key.InvalidResult()));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
