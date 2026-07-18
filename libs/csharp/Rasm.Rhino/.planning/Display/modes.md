# [RASM_RHINO_DISPLAY_MODES]

`Modes.Configure` owns display-mode appearance, descriptor policy, viewport binding, mode-scoped capture, and built-in analysis attachment as one request algebra. Raw host editors remain inside the fold, every viewport touch stays leased, and every successful mutation returns detached mode evidence.

`ModeOp` remains the display-mode table seam, `ViewportTarget` remains the viewport identity seam, and `CaptureArtifact` remains bitmap custody. `DisplayModeDescription`, `DisplayPipelineAttributes`, `RhinoViewport`, and `VisualAnalysisMode` never cross the receipt boundary.

## [01]-[INDEX]

- [01]-[APPEARANCE]: `Appearance` folds complete concern values over the live mode editor.
- [02]-[MODE_FAMILY]: `ModeKind`, `ModePolicy`, and `ModePlan` own identity, policy, and derivation.
- [03]-[CONFIGURE]: `ModeRequest` closes every mode, viewport, capture, and analysis modality behind `Modes.Configure`.

## [02]-[APPEARANCE]

- Owner: `Appearance` is the closed concern family; each case carries the whole state its host writer consumes.
- Entry: `Appearance.Write` is the only surface that receives `DisplayPipelineAttributes`.
- Auto: `Appearance.Write` traverses the immutable case sequence and captures host rejection on one rail.
- Law: the concern sequence admits one row per case — duplicate discriminants reject at request admission, so no later row silently overwrites an earlier host write.
- Receipt: appearance contributes only through the enclosing `ModeReceipt.Configured` case.
- Growth: a host appearance concern lands as one `Appearance` case and one total dispatch arm.
- Boundary: colors quantize once at the writer; raw host colors and attribute editors stay inside the boundary.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class WidthUse {
    public static readonly WidthUse Object = new(
        key: 0,
        curve: DisplayPipelineAttributes.CurveThicknessUse.ObjectWidth,
        surface: DisplayPipelineAttributes.SurfaceThicknessUse.ObjectWidth,
        subD: DisplayPipelineAttributes.SubDThicknessUse.ObjectWidth);
    public static readonly WidthUse Pixels = new(
        key: 1,
        curve: DisplayPipelineAttributes.CurveThicknessUse.Pixels,
        surface: DisplayPipelineAttributes.SurfaceThicknessUse.Pixels,
        subD: DisplayPipelineAttributes.SubDThicknessUse.Pixels);

    internal DisplayPipelineAttributes.CurveThicknessUse Curve { get; }
    internal DisplayPipelineAttributes.SurfaceThicknessUse Surface { get; }
    internal DisplayPipelineAttributes.SubDThicknessUse SubD { get; }
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

    internal static Fin<Fill> Read(DisplayPipelineAttributes source, Op key) => key.Catch(() => {
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Appearance {
    private const double MinimumMaterialValue = 0.0;
    private const double MaximumMaterialTransparency = 100.0;
    private Appearance() { }
    public sealed record Shading(bool Enabled, bool VertexColors, bool Flat, bool AssignedMaterial, Option<PerceptualColor> ObjectColor, BackfaceUse Backface, bool CullBackfaces, double Shine, double Transparency, PerceptualColor Diffuse, PerceptualColor BackDiffuse, Fill Fill) : Appearance;
    public sealed record Edges(bool Curves, bool Surfaces, bool Naked, bool Isocurves, int Width, WidthUse WidthUse, float Scale, PerceptualColor Color, int ReductionPercent, bool Pattern) : Appearance;
    public sealed record Lighting(LightingUse Scheme, PerceptualColor Ambient, bool UseLightColor, bool ShowLights, bool CastShadows, int SkylightShadowQuality) : Appearance;
    public sealed record Ground(ScopeUse Usage, bool Enabled, bool ShowUnderside, double Altitude, PerceptualColor Color, bool Shadows, bool AutoAltitude) : Appearance;
    public sealed record Grid(bool GridVisible, bool Axes, bool WorldAxes, bool Transparent, bool OnTop, int ThinFrequency, int ThickFrequency, PerceptualColor Thin, PerceptualColor Thick, PerceptualColor X, PerceptualColor Y, PerceptualColor Z) : Appearance;
    public sealed record SubD(bool Smooth, bool Creases, bool NonManifold, bool Boundary, int Width, WidthUse WidthUse, float Scale, PerceptualColor SmoothColor, PerceptualColor CreaseColor, PerceptualColor NonManifoldColor, PerceptualColor BoundaryColor) : Appearance;
    public sealed record Mesh(bool Wires, bool Naked, bool NonManifold, bool Vertices, int Width, PerceptualColor WireColor, PerceptualColor NakedColor, PerceptualColor NonManifoldColor) : Appearance;
    public sealed record Clipping(bool Planes, bool Fills, bool Edges, bool SectionStyles, ClippingFillUse FillUse, ClippingEdgeUse EdgeUse, PerceptualColor FillColor, PerceptualColor EdgeColor, int EdgeWidth) : Appearance;
    public sealed record Technical(bool Hidden, bool Edges, bool Silhouettes, bool Creases, bool Seams, bool Intersections, bool Lighting) : Appearance;
    public sealed record Locked(LockedUse Usage, PerceptualColor Color, int Transparency, bool Behind, bool Ghost) : Appearance;
    public sealed record Points(bool Visible, PointUse PointStyle, float PointRadius, bool Clouds, PointUse CloudStyle, float CloudRadius) : Appearance;
    public sealed record Grips(bool Visible, bool Polygon, PointUse Style, int WireWidth, int Size, Option<PerceptualColor> FixedColor) : Appearance;
    public sealed record Fade(PerceptualColor Color, float Amount) : Appearance;
    public sealed record Dither(float Amount) : Appearance;
    public sealed record Hatch(float Strength, float Width) : Appearance;
    public sealed record Pipeline(bool Xray, bool IgnoreHighlights, bool DisableConduits, bool DisableTransparency, bool Text, bool Annotations, ScopeUse Workflow, float PreGamma, float PostGamma, bool BakeTextures, int RealtimePasses, bool RealtimeProgress) : Appearance;

    internal bool Valid => Switch(
        shading: static row => row.Backface is not null
            && row.Fill is not null
            && row.Shine >= MinimumMaterialValue
            && row.Shine <= Material.MaxShine
            && row.Transparency >= MinimumMaterialValue
            && row.Transparency <= MaximumMaterialTransparency,
        edges: static row => row.WidthUse is not null
            && row.Width > 0
            && float.IsFinite(row.Scale)
            && row.Scale > 0f
            && row.ReductionPercent is >= 0 and <= 100,
        lighting: static row => row.Scheme is not null && row.SkylightShadowQuality >= 0,
        ground: static row => row.Usage is not null && double.IsFinite(row.Altitude),
        grid: static row => row.ThinFrequency > 0 && row.ThickFrequency > 0,
        subD: static row => row.WidthUse is not null && row.Width > 0 && float.IsFinite(row.Scale) && row.Scale > 0f,
        mesh: static row => row.Width > 0,
        clipping: static row => row.FillUse is not null && row.EdgeUse is not null && row.EdgeWidth > 0,
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
            && float.IsFinite(row.PreGamma)
            && row.PreGamma > 0f
            && float.IsFinite(row.PostGamma)
            && row.PostGamma > 0f
            && row.RealtimePasses > 0);

    internal static Fin<Unit> Write(Seq<Appearance> concerns, DisplayPipelineAttributes target, Op key) =>
        concerns.TraverseM(concern => key.Catch(() => Fin.Succ(concern.Write(target)))).As().Map(static _ => unit);

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
            (a.LockedObjectsDrawBehindOthers, a.GhostLockedObjects) = (row.Behind, row.Ghost);
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
        (a.BackfaceDisplayStyle, a.CullBackfaces) = (row.Backface.Native, row.CullBackfaces);
        (a.FrontMaterialShine, a.FrontMaterialTransparency, a.FrontDiffuse, a.BackMaterialDiffuseColor) =
            (row.Shine, row.Transparency, Quant.Sys(row.Diffuse), Quant.Sys(row.BackDiffuse));
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
        (a.CurveThickness, a.SurfaceEdgeThickness, a.SurfaceNakedEdgeThickness, a.SurfaceIsoThickness) = (row.Width, row.Width, row.Width, row.Width);
        (a.CurveThicknessScale, a.SurfaceEdgeThicknessScale, a.SurfaceNakedEdgeThicknessScale, a.SurfaceIsoThicknessScale) = (row.Scale, row.Scale, row.Scale, row.Scale);
        a.SetCurveThicknessUsage(row.WidthUse.Curve); a.SetSurfaceEdgeThicknessUsage(row.WidthUse.Surface);
        a.SetSurfaceNakedEdgeThicknessUsage(row.WidthUse.Surface); a.SetSurfaceIsoThicknessUsage(row.WidthUse.Surface);
        (a.SurfaceEdgeColor, a.SurfaceNakedEdgeColor) = (Quant.Sys(row.Color), Quant.Sys(row.Color));
        (a.SurfaceEdgeColorReduction, a.SurfaceNakedEdgeColorReduction) = (row.ReductionPercent, row.ReductionPercent);
        a.SetSurfaceIsoApplyPattern(row.Pattern, row.Pattern, row.Pattern);
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Lighting row) {
        (a.LightingScheme, a.AmbientLightingColor, a.UseLightColor, a.ShowLights) = (row.Scheme.Native, Quant.Sys(row.Ambient), row.UseLightColor, row.ShowLights);
        (a.CastShadows, a.SkylightShadowQuality) = (row.CastShadows, row.SkylightShadowQuality);
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
        a.MeshSpecificAttributes.ShowMeshVertices = row.Vertices;
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Clipping row) {
        (a.ShowClippingPlanes, a.ShowClippingFills, a.ShowClippingEdges, a.UseSectionStyles) = (row.Planes, row.Fills, row.Edges, row.SectionStyles);
        (a.ClippingPlaneFillColorUsage, a.ClippingFillColor) = (row.FillUse.Native, Quant.Sys(row.FillColor));
        (a.ClippingEdgeColorUsage, a.ClippingEdgeColor, a.ClippingEdgeThickness) = (row.EdgeUse.Native, Quant.Sys(row.EdgeColor), row.EdgeWidth);
        return unit;
    }

    private static Unit Write(DisplayPipelineAttributes a, Pipeline row) {
        (a.XrayAllObjects, a.IgnoreHighlights, a.DisableConduits, a.DisableTransparency) = (row.Xray, row.IgnoreHighlights, row.DisableConduits, row.DisableTransparency);
        (a.ShowText, a.ShowAnnotations, a.LinearWorkflowUsage) = (row.Text, row.Annotations, row.Workflow.Workflow);
        (a.PreProcessGamma, a.PostProcessGamma, a.BakeTextures) = (row.PreGamma, row.PostGamma, row.BakeTextures);
        (a.RealtimeRenderPasses, a.ShowRealtimeRenderProgressBar) = (row.RealtimePasses, row.RealtimeProgress);
        return unit;
    }
}
```

## [03]-[MODE_FAMILY]

- Owner: `ModeKind` carries built-in identity; `ModePlan` distinguishes editing an existing descriptor from deriving and persisting a copy.
- Policy: each `ModePolicy` case carries one descriptor decision; a request sequence composes any unique subset independently from appearance concerns.
- Law: `ModeOp.FindCase` yields a detached host copy; appearance and policy writes remain local until `ModeOp.UpdateCase` persists the descriptor.
- Law: copied modes always pass through `ModeOp.UpdateCase`; an in-memory copy never becomes a successful receipt.
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

## [04]-[CONFIGURE]

- Owner: `ModeRequest` is the complete ingress family and `ModeReceipt` is the detached egress family.
- Entry: `Modes.Configure` dispatches every modality; request shape carries singular, batch, query, and capture intent without flags or sibling verbs.
- Law: analysis attachment admits a unique requested set, separates requested and changed subjects in the receipt, and restores a failed prefix in reverse while retaining every cleanup fault.
- Growth: `AnalysisKind` carries every built-in host analysis identity as a table row.
- Law: `Apply` stages by plan case — an existing descriptor proves every concern and policy write on a host-minted staging copy first, deleted after the proof, because `DisplayPipelineAttributes` admits no external clone; a derived plan's registered copy is itself the stage and deletes on a failed commit.
- Boundary: UI adjustment and analysis-dialog requests demand dialog capability; bitmap custody exits only as `CaptureArtifact`.
- Growth: a new mode operation is one request case and one receipt projection inside the existing dispatch.

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

    internal bool Valid => Switch(
        apply: static row => row.Plan is not null
            && row.Plan.Valid
            && row.Policies.ForAll(static policy => policy is not null && policy.Valid)
            && Unique(row.Policies)
            && row.Concerns.ForAll(static concern => concern is not null && concern.Valid)
            && Unique(row.Concerns),
        bind: static row => row.Session is not null && row.Target is not null && row.Mode.Value != Guid.Empty,
        inspect: static row => row.Session is not null && row.Target is not null,
        capture: static row => row.Session is not null
            && row.Target is not null
            && row.Mode.Value != Guid.Empty
            && row.Extent.Match(Some: static size => size.IsValid, None: static () => true),
        analyze: static row => row.Session is not null && row.Edit is not null && row.Edit.Valid);

    private static bool Unique<T>(Seq<T> rows) where T : class =>
        rows.Map(static row => row.GetType()).Distinct().Count == rows.Count;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModeReceipt : IDetachedDocumentResult {
    private ModeReceipt() { }
    public sealed record Configured(ModeId Mode) : ModeReceipt;
    public sealed record Bound(ModeId Mode) : ModeReceipt;
    public sealed record Inspected(ModeId Mode, Fill Fill) : ModeReceipt;
    public sealed record Captured(CaptureArtifact Artifact) : ModeReceipt;
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
                    .Bind(mode => Staged(mode, held.Policies, held.Concerns, held.Op)
                        .Bind(_ => Commit(mode, held.Policies, held.Concerns, held.Op))),
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
                    .Bind(mode => Fill.Read(mode.DisplayAttributes, op).Map(fill => (Mode: ModeId.Create(mode.Id), Fill: fill))), op))
                .Map(state => (ModeReceipt)new ModeReceipt.Inspected(state.Mode, state.Fill)),
            capture: static (op, row) => Resolve(row.Mode, op)
                .Bind(mode => ViewportLease.Of(row.Session, row.Target, op)
                    .Bind(lease => lease.Use(borrow => op.Catch(() => Optional(row.Extent.Match(
                        Some: size => borrow.View.CaptureToBitmap(size.Native, mode),
                        None: () => borrow.View.CaptureToBitmap(mode))).ToFin(op.InvalidResult())), op)))
                .Bind(bitmap => CaptureArtifact.Raster(bitmap, op))
                .Map(artifact => (ModeReceipt)new ModeReceipt.Captured(artifact)),
            analyze: static (op, row) => Analyze(row.Session, row.Edit, op)));
    }

    private static Fin<DisplayModeDescription> Resolve(ModeId id, Op key) =>
        new ModeOp.FindCase(id).Apply(key).Bind(modes => modes.Head.ToFin(key.InvalidResult()));

    private static Fin<ModeReceipt> Commit(DisplayModeDescription mode, Seq<ModePolicy> policies, Seq<Appearance> concerns, Op key) =>
        Appearance.Write(concerns, mode.DisplayAttributes, key)
            .Bind(_ => ModePolicy.Write(policies, mode, key))
            .Bind(_ => new ModeOp.UpdateCase(mode).Apply(key))
            .Map(_ => (ModeReceipt)new ModeReceipt.Configured(ModeId.Create(mode.Id)));

    private static Fin<Unit> Staged(DisplayModeDescription target, Seq<ModePolicy> policies, Seq<Appearance> concerns, Op key) =>
        new ModeOp.CopyCase(ModeId.Create(target.Id), $"staging-{Guid.NewGuid():N}").Apply(key)
            .Bind(modes => modes.Head.ToFin(key.InvalidResult()))
            .Bind(probe => DeleteAfter(
                Appearance.Write(concerns, probe.DisplayAttributes, key)
                    .Bind(_ => ModePolicy.Write(policies, probe, key)),
                ModeId.Create(probe.Id),
                key));

    private static Fin<Unit> DeleteAfter(Fin<Unit> primary, ModeId temporary, Op key) {
        Fin<Unit> cleanup = new ModeOp.DeleteCase(temporary).Apply(key).Map(static _ => unit);
        return primary.Match(
            Succ: _ => cleanup,
            Fail: failure => cleanup.Match(
                Succ: _ => Fin.Fail<Unit>(failure),
                Fail: secondary => Fin.Fail<Unit>(failure + secondary)));
    }

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
