# [RASM_RHINO_DISPLAY_MODES]

Display-mode appearance model (`Rasm.Rhino.Display`). `DisplayPipelineAttributes` — the full how-a-viewport-looks model behind every display mode — becomes one `DisplayProfile` patch of concern schemas: shading with the object paint source, frame-buffer fill, and backface style; curve/surface/naked/iso edges; lighting and the shadow block; ground plane; grid and world axes; SubD and mesh edge classes as writer-column tables; clipping fills/edges/shades; the Rhino 9 technical toggles; locked-object ghosting; points, grips, and dynamic-display effects; and the pipeline scene flags. A populated schema writes its whole concern onto the host attribute object, and every color quantizes from the kernel `PerceptualColor.ToRgb()` at the write. Host attribute objects carry no public constructor — one is reached only through `DisplayModeDescription.DisplayAttributes` or handed in by host hooks — so the profile is a write patch persisted by `UpdateDisplayMode`, never a free-standing attribute value, and `CopyDisplayMode` registers in memory alone, so mode derivation is copy → configure → persist as one entry. `ModePolicy` folds the descriptor's pipeline policy flags, `BuiltinMode` keys the host's special-mode ids, `ViewportModes` binds a mode to a leased viewport and captures through the mode-scoped `CaptureToBitmap` overloads, and `BuiltinAnalysis` plus `AnalysisOp` own built-in visual-analysis attachment over `RhinoObject.EnableVisualAnalysisMode`. Descriptor table CRUD stays conduit.md's `ModeOp` — this page composes its cases and re-mints no table verb — and custom analysis registration stays conduit.md's `AnalysisProgram`.

## [01]-[INDEX]

- [02]-[APPEARANCE_SCHEMAS]: `WidthSource`/`ScopeSource` collapse rows, `FillSpec`/`ObjectPaint` unions, `SubDEdgeClass`/`MeshEdgeClass` writer tables, the concern schemas, and the `DisplayProfile` patch fold.
- [03]-[MODE_MANAGEMENT]: `BuiltinMode` rows, `ModePolicy` descriptor flags, and the `Modes` configure/derive rail over conduit.md's `ModeOp`.
- [04]-[VIEWPORT_ASSIGNMENT]: `ViewportModes` — leased per-viewport mode binding and mode-scoped capture.
- [05]-[BUILTIN_ANALYSIS]: `BuiltinAnalysis` rows and the `AnalysisOp` attachment rail over object enablement.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[APPEARANCE_SCHEMAS]

- Owner: `WidthSource` `[SmartEnum<int>]` — the one object-width-versus-pixels vocabulary whose columns lower onto the three isomorphic host enums (`CurveThicknessUse`, `SurfaceThicknessUse`, `SubDThicknessUse`), serving the per-class edge writers and the global `SubDThicknessUsage` slot alike; `ScopeSource` — the by-document-versus-custom pair lowering onto `GroundPlaneUsages` and `LinearWorkflowUsages`. `FillSpec` `[Union]` — the frame-buffer fill: default, solid, two-color gradient, four-corner gradient, bitmap, renderer, transparent — each arm writes `FillMode` plus the matching `SetFill` overload. `ObjectPaint` `[Union]` — the object paint source: assigned material, custom color, custom material — one arm sets the three mutually exclusive host bools together. `SubDEdgeClass` and `MeshEdgeClass` `[SmartEnum<int>]` — the edge-class writer tables: each row carries `[UseDelegateFromConstructor]` writer columns (show, paint, paint-use, reduction, width, width-use, scale, pattern for SubD; show, paint, reduction, width for mesh), so the four SubD and three mesh per-class member families collapse to one spec record applied through the row. Concern schemas — `ShadingSchema`, `EdgeSchema` (curve/edge/naked/iso faces), `LightSchema` with `ShadowSpec`, `GroundSchema`, `GridSchema`, `SubDSchema`, `MeshSchema`, `ClipSchema`, `TechnicalSchema`, `LockedSchema`, `PointSchema`, `GripSchema`, `EffectSchema`, `PipelineSchema` — are complete concern rows; `DisplayProfile` holds one `Option` slot per schema and folds populated slots onto the attribute object.
- Law: the profile is a patch at schema granularity — a populated schema writes its whole concern, an absent slot leaves the mode's stored concern untouched; per-field surgery rides the descriptor's own live editor, never a second diff shape.
- Law: every color slot is kernel `PerceptualColor` quantized once through the draw page's `Quant.Sys` at the write; a `System.Drawing.Color` field on a schema is the deleted form.
- Law: host alias slots take one writer — `CastShadows` shares `ShadowsOn`'s native bool so `ShadowSpec.On` owns it, `UseCustomObjectMaterialBackfaces` shares `UseBackfaceMaterial`'s, `SurfaceEdgeColorReduction` and the host-misspelled `SurfaceNakedAdgeColorReduction` alias their `*Percent` masters, and the raw iso bools (`SurfaceIsoSingleColor`, `SurfaceIsoColorsUsed`, `SurfaceIsoThicknessUsed`) are the slots `SetSurfaceIsoColorUsage`/`SetSurfaceIsoThicknessUsage` compose — the usage enum is the one write; `ShowSurfaceEdges` and the 8.6 singular `ShowSurfaceEdge` are distinct native slots one `Show` value writes together.
- Law: the technical axis is the seven `Show*` toggles (`ShowHiddenLines`, `ShowEdges`, `ShowSilhouttes` — host spelling — `ShowCreases`, `ShowSeams`, `ShowIntersections`, `ShowLighting`); the host's technical color/thickness members and its technical-parameter mask are internal, so the toggles are the whole reachable technical surface.
- Law: grid and viewport-scale members live on the nested `ViewSpecificAttributes` editor and mesh-wire members on `MeshSpecificAttributes`; the two schemas reach through the accessors and no other schema touches a nested editor.
- Boundary: `DisplayPipelineAttributes` never crosses into a consumer — the profile value is the crossing shape, and the host object is touched only inside the fold; render.md's `SettingsChanged` hook and conduit.md's `AnalysisProgram.SetupAttributes` receive the live host object from the host and read facts off it directly.
- Growth: a new appearance member is one field plus one write line in its owning schema; a new concern is one schema record plus one `DisplayProfile` slot; a new edge class is one writer row.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class WidthSource {
    public static readonly WidthSource ObjectWidth = new(
        key: 0,
        curve: DisplayPipelineAttributes.CurveThicknessUse.ObjectWidth,
        surface: DisplayPipelineAttributes.SurfaceThicknessUse.ObjectWidth,
        subD: DisplayPipelineAttributes.SubDThicknessUse.ObjectWidth);
    public static readonly WidthSource Pixels = new(
        key: 1,
        curve: DisplayPipelineAttributes.CurveThicknessUse.Pixels,
        surface: DisplayPipelineAttributes.SurfaceThicknessUse.Pixels,
        subD: DisplayPipelineAttributes.SubDThicknessUse.Pixels);

    public DisplayPipelineAttributes.CurveThicknessUse Curve { get; }
    public DisplayPipelineAttributes.SurfaceThicknessUse Surface { get; }
    public DisplayPipelineAttributes.SubDThicknessUse SubD { get; }
}

[SmartEnum<int>]
public sealed partial class ScopeSource {
    public static readonly ScopeSource ByDocument = new(
        key: 0,
        ground: DisplayPipelineAttributes.GroundPlaneUsages.ByDocument,
        workflow: DisplayPipelineAttributes.LinearWorkflowUsages.ByDocument);
    public static readonly ScopeSource Custom = new(
        key: 1,
        ground: DisplayPipelineAttributes.GroundPlaneUsages.Custom,
        workflow: DisplayPipelineAttributes.LinearWorkflowUsages.Custom);

    public DisplayPipelineAttributes.GroundPlaneUsages Ground { get; }
    public DisplayPipelineAttributes.LinearWorkflowUsages Workflow { get; }
}

[SmartEnum<int>]
public sealed partial class SubDEdgeClass {
    public static readonly SubDEdgeClass SmoothInterior = new(key: 0,
        show: static (a, on) => a.ShowSubDEdges = on,
        paint: static (a, c) => a.SubDSmoothInteriorEdgeColor = c,
        paintUse: static (a, u) => a.SubDSmoothInteriorEdgeColorUsage = u,
        reduction: static (a, r) => a.SubDSmoothInteriorColorReduction = r,
        width: static (a, w) => a.SubDSmoothInteriorEdgeThickness = w,
        widthUse: static (a, u) => a.SubDSmoothInteriorThicknessUsage = u,
        widthScale: static (a, s) => a.SubDSmoothInteriorThicknessScale = s,
        pattern: static (a, p) => a.SubDSmoothInteriorApplyPattern = p);
    public static readonly SubDEdgeClass CreaseInterior = new(key: 1,
        show: static (a, on) => a.ShowSubDCreases = on,
        paint: static (a, c) => a.SubDCreaseInteriorEdgeColor = c,
        paintUse: static (a, u) => a.SubDCreaseInteriorEdgeColorUsage = u,
        reduction: static (a, r) => a.SubDCreaseInteriorColorReduction = r,
        width: static (a, w) => a.SubDCreaseInteriorEdgeThickness = w,
        widthUse: static (a, u) => a.SubDCreaseInteriorThicknessUsage = u,
        widthScale: static (a, s) => a.SubDCreaseInteriorThicknessScale = s,
        pattern: static (a, p) => a.SubDCreaseInteriorApplyPattern = p);
    public static readonly SubDEdgeClass NonManifold = new(key: 2,
        show: static (a, on) => a.ShowSubDNonmanifoldEdges = on,
        paint: static (a, c) => a.SubDNonManifoldEdgeColor = c,
        paintUse: static (a, u) => a.SubDNonManifoldEdgeColorUsage = u,
        reduction: static (a, r) => a.SubDNonManifoldColorReduction = r,
        width: static (a, w) => a.SubDNonManifoldEdgeThickness = w,
        widthUse: static (a, u) => a.SubDNonManifoldThicknessUsage = u,
        widthScale: static (a, s) => a.SubDNonManifoldThicknessScale = s,
        pattern: static (a, p) => a.SubDNonManifoldApplyPattern = p);
    public static readonly SubDEdgeClass Boundary = new(key: 3,
        show: static (a, on) => a.ShowSubDBoundary = on,
        paint: static (a, c) => a.SubDBoundaryEdgeColor = c,
        paintUse: static (a, u) => a.SubDBoundaryEdgeColorUsage = u,
        reduction: static (a, r) => a.SubDBoundaryColorReduction = r,
        width: static (a, w) => a.SubDBoundaryEdgeThickness = w,
        widthUse: static (a, u) => a.SubDBoundaryThicknessUsage = u,
        widthScale: static (a, s) => a.SubDBoundaryThicknessScale = s,
        pattern: static (a, p) => a.SubDBoundaryApplyPattern = p);

    [UseDelegateFromConstructor]
    internal partial void Show(DisplayPipelineAttributes attributes, bool on);
    [UseDelegateFromConstructor]
    internal partial void Paint(DisplayPipelineAttributes attributes, System.Drawing.Color color);
    [UseDelegateFromConstructor]
    internal partial void PaintUse(DisplayPipelineAttributes attributes, DisplayPipelineAttributes.SubDEdgeColorUse use);
    [UseDelegateFromConstructor]
    internal partial void Reduction(DisplayPipelineAttributes attributes, int percent);
    [UseDelegateFromConstructor]
    internal partial void Width(DisplayPipelineAttributes attributes, float width);
    [UseDelegateFromConstructor]
    internal partial void WidthUse(DisplayPipelineAttributes attributes, DisplayPipelineAttributes.SubDThicknessUse use);
    [UseDelegateFromConstructor]
    internal partial void WidthScale(DisplayPipelineAttributes attributes, float scale);
    [UseDelegateFromConstructor]
    internal partial void Pattern(DisplayPipelineAttributes attributes, bool apply);
}

[SmartEnum<int>]
public sealed partial class MeshEdgeClass {
    public static readonly MeshEdgeClass Interior = new(key: 0,
        show: static (a, on) => a.ShowMeshEdges = on,
        paint: static (a, c) => a.MeshEdgeColor = c,
        reduction: static (a, r) => a.MeshEdgeColorReduction = r,
        width: static (a, w) => a.MeshEdgeThickness = w);
    public static readonly MeshEdgeClass Naked = new(key: 1,
        show: static (a, on) => a.ShowMeshNakedEdges = on,
        paint: static (a, c) => a.MeshNakedEdgeColor = c,
        reduction: static (a, r) => a.MeshNakedEdgeColorReduction = r,
        width: static (a, w) => a.MeshNakedEdgeThickness = w);
    public static readonly MeshEdgeClass NonManifold = new(key: 2,
        show: static (a, on) => a.ShowMeshNonmanifoldEdges = on,
        paint: static (a, c) => a.MeshNonmanifoldEdgeColor = c,
        reduction: static (a, r) => a.MeshNonmanifoldEdgeColorReduction = r,
        width: static (a, w) => a.MeshNonmanifoldEdgeThickness = w);

    [UseDelegateFromConstructor]
    internal partial void Show(DisplayPipelineAttributes attributes, bool on);
    [UseDelegateFromConstructor]
    internal partial void Paint(DisplayPipelineAttributes attributes, System.Drawing.Color color);
    [UseDelegateFromConstructor]
    internal partial void Reduction(DisplayPipelineAttributes attributes, int percent);
    [UseDelegateFromConstructor]
    internal partial void Width(DisplayPipelineAttributes attributes, int width);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FillSpec {
    private FillSpec() { }
    public sealed record DefaultCase : FillSpec;
    public sealed record SolidCase(PerceptualColor Color) : FillSpec;
    public sealed record GradientCase(PerceptualColor Top, PerceptualColor Bottom) : FillSpec;
    public sealed record CornersCase(PerceptualColor TopLeft, PerceptualColor BottomLeft, PerceptualColor TopRight, PerceptualColor BottomRight) : FillSpec;
    public sealed record BitmapCase : FillSpec;
    public sealed record RendererCase : FillSpec;
    public sealed record TransparentCase : FillSpec;

    internal Unit Write(DisplayPipelineAttributes attributes) =>
        Switch(
            state: attributes,
            defaultCase: static (a, _) => Op.Side(() => a.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor),
            solidCase: static (a, fill) => Op.Side(() => {
                a.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.SolidColor;
                a.SetFill(singleColor: Quant.Sys(fill.Color));
            }),
            gradientCase: static (a, fill) => Op.Side(() => {
                a.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Gradient2Color;
                a.SetFill(gradientTop: Quant.Sys(fill.Top), gradientBottom: Quant.Sys(fill.Bottom));
            }),
            cornersCase: static (a, fill) => Op.Side(() => {
                a.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Gradient4Color;
                a.SetFill(
                    gradientTopLeft: Quant.Sys(fill.TopLeft), gradientBottomLeft: Quant.Sys(fill.BottomLeft),
                    gradientTopRight: Quant.Sys(fill.TopRight), gradientBottomRight: Quant.Sys(fill.BottomRight));
            }),
            bitmapCase: static (a, _) => Op.Side(() => a.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Bitmap),
            rendererCase: static (a, _) => Op.Side(() => a.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Renderer),
            transparentCase: static (a, _) => Op.Side(() => a.FillMode = DisplayPipelineAttributes.FrameBufferFillMode.Transparent));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectPaint {
    private ObjectPaint() { }
    public sealed record AssignedMaterialCase : ObjectPaint;
    public sealed record CustomColorCase(PerceptualColor Color) : ObjectPaint;
    public sealed record CustomMaterialCase : ObjectPaint;

    internal Unit Write(DisplayPipelineAttributes attributes) =>
        Switch(
            state: attributes,
            assignedMaterialCase: static (a, _) => Op.Side(() =>
                (a.UseAssignedObjectMaterial, a.UseCustomObjectColor, a.UseCustomObjectMaterial) = (true, false, false)),
            customColorCase: static (a, paint) => Op.Side(() => {
                (a.UseAssignedObjectMaterial, a.UseCustomObjectColor, a.UseCustomObjectMaterial) = (false, true, false);
                a.ObjectColor = Quant.Sys(paint.Color);
            }),
            customMaterialCase: static (a, _) => Op.Side(() =>
                (a.UseAssignedObjectMaterial, a.UseCustomObjectColor, a.UseCustomObjectMaterial) = (false, false, true)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ShadingSchema(
    bool Enabled, bool VertexColors, bool FlatShaded, ObjectPaint Paint, FillSpec Fill,
    DisplayPipelineAttributes.BackfaceStyle Backface, bool CullBackfaces, bool HighlightSurfaces,
    (bool Distinct, bool ObjectMaterial, bool Custom, bool OverrideObjectColor) Backfaces,
    (double Shine, double Transparency, PerceptualColor Diffuse, bool OverrideColor, bool OverrideTransparency, bool OverrideReflectivity) Front,
    (double Shine, double Transparency, PerceptualColor Diffuse, bool OverrideTransparency, bool OverrideReflectivity) Back) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShadingEnabled, target.ShadeVertexColors, target.FrontFlatShaded) = (Enabled, VertexColors, FlatShaded);
        _ = Paint.Write(attributes: target);
        _ = Fill.Write(attributes: target);
        (target.BackfaceDisplayStyle, target.CullBackfaces, target.HighlightSurfaces) = (Backface, CullBackfaces, HighlightSurfaces);
        (target.UseBackfaceMaterial, target.UseObjectBackfaceMaterial) = (Backfaces.Distinct, Backfaces.ObjectMaterial);
        (target.UseCustomBackface, target.UseCustomObjectColorBackfaces) = (Backfaces.Custom, Backfaces.OverrideObjectColor);
        (target.FrontMaterialShine, target.FrontMaterialTransparency, target.FrontDiffuse) = (Front.Shine, Front.Transparency, Quant.Sys(Front.Diffuse));
        (target.FrontOverrideObjectColor, target.FrontOverrideObjectTransparency, target.FrontOverrideObjectReflectivity) = (Front.OverrideColor, Front.OverrideTransparency, Front.OverrideReflectivity);
        (target.BackMaterialShine, target.BackMaterialTransparency, target.BackMaterialDiffuseColor) = (Back.Shine, Back.Transparency, Quant.Sys(Back.Diffuse));
        (target.BackOverrideObjectTransparency, target.BackOverrideObjectReflectivity) = (Back.OverrideTransparency, Back.OverrideReflectivity);
        return unit;
    }
}

public sealed record CurveFace(bool Show, int Width, WidthSource WidthUse, float Scale, Option<PerceptualColor> SingleColor) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShowCurves, target.CurveThickness, target.CurveThicknessScale) = (Show, Width, Scale);
        target.SetCurveThicknessUsage(WidthUse.Curve);
        target.UseSingleCurveColor = SingleColor.IsSome;
        _ = SingleColor.Iter(color => target.CurveColor = Quant.Sys(color));
        return unit;
    }
}

public sealed record SurfaceEdgeFace(
    bool Show, int Width, WidthSource WidthUse, float Scale, PerceptualColor Color,
    DisplayPipelineAttributes.SurfaceEdgeColorUse ColorUse, int ReductionPercent, bool Pattern) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShowSurfaceEdges, target.ShowSurfaceEdge, target.SurfaceEdgeThickness, target.SurfaceEdgeThicknessScale) = (Show, Show, Width, Scale);
        target.SetSurfaceEdgeThicknessUsage(WidthUse.Surface);
        (target.SurfaceEdgeColorUsage, target.SurfaceEdgeColor) = (ColorUse, Quant.Sys(Color));
        (target.SurfaceEdgeColorReductionPercent, target.SurfaceEdgeApplyPattern) = (ReductionPercent, Pattern);
        return unit;
    }
}

public sealed record NakedEdgeFace(
    bool Show, int Width, DisplayPipelineAttributes.SurfaceNakedEdgeThicknessUse WidthUse, float Scale, PerceptualColor Color,
    DisplayPipelineAttributes.SurfaceNakedEdgeColorUse ColorUse, int ReductionPercent, bool Pattern) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShowSurfaceNakedEdge, target.SurfaceNakedEdgeThickness, target.SurfaceNakedEdgeThicknessScale) = (Show, Width, Scale);
        target.SetSurfaceNakedEdgeThicknessUsage(WidthUse);
        (target.SurfaceNakedEdgeColorUsage, target.SurfaceNakedEdgeColor) = (ColorUse, Quant.Sys(Color));
        (target.SurfaceNakedEdgeColorReductionPercent, target.SurfaceNakedEdgeApplyPattern) = (ReductionPercent, Pattern);
        return unit;
    }
}

public sealed record IsoFace(
    bool Show, bool OnFlatFaces, DisplayPipelineAttributes.SurfaceIsoColorUse ColorUse,
    PerceptualColor ColorUV, PerceptualColor ColorU, PerceptualColor ColorV,
    DisplayPipelineAttributes.SurfaceIsoThicknessUse WidthUse, int Width, int WidthU, int WidthV,
    (float U, float V, float W) Scale, bool PatternU, bool PatternV, bool PatternW) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShowIsoCurves, target.SurfaceIsoShowForFlatFaces) = (Show, OnFlatFaces);
        target.SetSurfaceIsoColorUsage(ColorUse);
        (target.SurfaceIsoUVColor, target.SurfaceIsoUColor, target.SurfaceIsoVColor) = (Quant.Sys(ColorUV), Quant.Sys(ColorU), Quant.Sys(ColorV));
        target.SetSurfaceIsoThicknessUsage(WidthUse);
        (target.SurfaceIsoThickness, target.SurfaceIsoUThickness, target.SurfaceIsoVThickness) = (Width, WidthU, WidthV);
        (target.SurfaceIsoThicknessUScale, target.SurfaceIsoThicknessVScale, target.SurfaceIsoThicknessWScale) = (Scale.U, Scale.V, Scale.W);
        target.SetSurfaceIsoApplyPattern(PatternU, PatternV, PatternW);
        return unit;
    }
}

public sealed record EdgeSchema(CurveFace Curves, SurfaceEdgeFace Edges, NakedEdgeFace Naked, IsoFace Iso, bool TangentEdges, bool TangentSeams) {
    internal Unit Write(DisplayPipelineAttributes target) {
        _ = Curves.Write(target: target);
        _ = Edges.Write(target: target);
        _ = Naked.Write(target: target);
        _ = Iso.Write(target: target);
        (target.ShowTangentEdges, target.ShowTangentSeams) = (TangentEdges, TangentSeams);
        return unit;
    }
}

public sealed record ShadowSpec(
    bool On, int Intensity, PerceptualColor Color, int MemoryUsage, int SkylightQuality, int SoftEdgeQuality,
    double EdgeBlur, double BiasX, int TransparencyTolerance, float ClippingRadius, bool IgnoreClippingPlanes) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShadowsOn, target.ShadowIntensity, target.ShadowColor) = (On, Intensity, Quant.Sys(Color));
        (target.ShadowMemoryUsage, target.SkylightShadowQuality, target.ShadowSoftEdgeQuality) = (MemoryUsage, SkylightQuality, SoftEdgeQuality);
        (target.ShadowEdgeBlur, target.ShadowBiasX, target.ShadowTransparencyTolerance) = (EdgeBlur, BiasX, TransparencyTolerance);
        (target.ShadowClippingRadius, target.ShadowsIgnoreUserDefinedClippingPlanes) = (ClippingRadius, IgnoreClippingPlanes);
        return unit;
    }
}

public sealed record LightSchema(
    DisplayPipelineAttributes.LightingSchema Scheme, PerceptualColor Ambient, bool UseLightColor,
    bool ShowLights, Option<ShadowSpec> Shadows) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.LightingScheme, target.AmbientLightingColor, target.UseLightColor) = (Scheme, Quant.Sys(Ambient), UseLightColor);
        target.ShowLights = ShowLights;
        _ = Shadows.Iter(spec => spec.Write(target: target));
        return unit;
    }
}

public sealed record GroundSchema(ScopeSource Usage, bool On, bool ShadowOnly, double Altitude, bool AutomaticAltitude) {
    internal Unit Write(DisplayPipelineAttributes target) {
        target.GroundPlaneUsage = Usage.Ground;
        (target.CustomGroundPlaneOn, target.CustomGroundPlaneShadowOnly) = (On, ShadowOnly);
        (target.CustomGroundPlaneAltitude, target.CustomGroundPlaneAutomaticAltitude) = (Altitude, AutomaticAltitude);
        return unit;
    }
}

public sealed record GridSchema(
    bool UseDocumentGrid, bool DrawGrid, bool DrawGridAxes, bool DrawZAxis, bool DrawWorldAxes,
    bool ShowOnTop, bool Blend, bool TransparentPlane, float Fade, float CornerRadius, float BoundaryThickness,
    PerceptualColor AxisX, PerceptualColor AxisY, PerceptualColor AxisZ, (double Horizontal, double Vertical) Scale,
    int Transparency, int PlaneTransparency, DisplayPipelineAttributes.GridPlaneVisibilityMode PlaneVisibility,
    DisplayPipelineAttributes.WorldAxesIconColorUse IconColors, bool PlaneUsesGridColor, PerceptualColor PlaneColor, int AxesSizePercent) {
    internal Unit Write(DisplayPipelineAttributes target) {
        DisplayPipelineAttributes.ViewDisplayAttributes view = target.ViewSpecificAttributes;
        (view.UseDocumentGrid, view.DrawGrid, view.DrawGridAxes, view.DrawZAxis, view.DrawWorldAxes) = (UseDocumentGrid, DrawGrid, DrawGridAxes, DrawZAxis, DrawWorldAxes);
        (view.ShowGridOnTop, view.BlendGrid, view.DrawTransparentGridPlane) = (ShowOnTop, Blend, TransparentPlane);
        (view.GridFade, view.GridCornerRadius, view.GridBoundaryThickness) = (Fade, CornerRadius, BoundaryThickness);
        (view.WorldAxisColorX, view.WorldAxisColorY, view.WorldAxisColorZ) = (Quant.Sys(AxisX), Quant.Sys(AxisY), Quant.Sys(AxisZ));
        (view.HorizontalViewportScale, view.VerticalViewportScale) = (Scale.Horizontal, Scale.Vertical);
        (target.GridTransparency, target.GridPlaneTransparency, target.GridPlaneVisibility) = (Transparency, PlaneTransparency, PlaneVisibility);
        (target.WorldAxesIconColorUsage, target.PlaneUsesGridColor) = (IconColors, PlaneUsesGridColor);
        (target.GridPlaneColor, target.AxesSizePercentage) = (Quant.Sys(PlaneColor), AxesSizePercent);
        return unit;
    }
}

public sealed record SubDEdgeSpec(
    bool Show, PerceptualColor Color, DisplayPipelineAttributes.SubDEdgeColorUse ColorUse, int Reduction,
    float Width, WidthSource WidthUse, float Scale, bool Pattern) {
    internal Unit Write(SubDEdgeClass edgeClass, DisplayPipelineAttributes target) {
        edgeClass.Show(attributes: target, on: Show);
        edgeClass.Paint(attributes: target, color: Quant.Sys(Color));
        edgeClass.PaintUse(attributes: target, use: ColorUse);
        edgeClass.Reduction(attributes: target, percent: Reduction);
        edgeClass.Width(attributes: target, width: Width);
        edgeClass.WidthUse(attributes: target, use: WidthUse.SubD);
        edgeClass.WidthScale(attributes: target, scale: Scale);
        edgeClass.Pattern(attributes: target, apply: Pattern);
        return unit;
    }
}

public sealed record SubDSchema(
    Seq<(SubDEdgeClass Class, SubDEdgeSpec Spec)> Edges,
    WidthSource WidthUse,
    Option<(bool Preview, PerceptualColor Plane, DisplayPipelineAttributes.SubDReflectionPlaneColorUse PlaneUse, int PlaneReduction, bool AxisOn, PerceptualColor AxisColor, float AxisWidth)> Reflection) {
    internal Unit Write(DisplayPipelineAttributes target) {
        target.SubDThicknessUsage = WidthUse.SubD;
        _ = Edges.Iter(row => row.Spec.Write(edgeClass: row.Class, target: target));
        _ = Reflection.Iter(plane => {
            (target.ShowSubDReflectionPlanePreview, target.SubDReflectionPlaneColor) = (plane.Preview, Quant.Sys(plane.Plane));
            (target.SubDReflectionPlaneColorUsage, target.SubDReflectionPlaneColorReduction) = (plane.PlaneUse, plane.PlaneReduction);
            (target.SubDReflectionPlaneAxisLineOn, target.SubDReflectionAxisLineColor, target.SubDReflectionAxisLineThickness) = (plane.AxisOn, Quant.Sys(plane.AxisColor), plane.AxisWidth);
        });
        return unit;
    }
}

public sealed record MeshEdgeSpec(bool Show, PerceptualColor Color, int Reduction, int Width) {
    internal Unit Write(MeshEdgeClass edgeClass, DisplayPipelineAttributes target) {
        edgeClass.Show(attributes: target, on: Show);
        edgeClass.Paint(attributes: target, color: Quant.Sys(Color));
        edgeClass.Reduction(attributes: target, percent: Reduction);
        edgeClass.Width(attributes: target, width: Width);
        return unit;
    }
}

public sealed record MeshSchema(
    Seq<(MeshEdgeClass Class, MeshEdgeSpec Spec)> Edges, bool Highlight, bool ShowWires,
    PerceptualColor WireColor, int WireWidth, bool ShowVertices, int VertexSize) {
    internal Unit Write(DisplayPipelineAttributes target) {
        _ = Edges.Iter(row => row.Spec.Write(edgeClass: row.Class, target: target));
        DisplayPipelineAttributes.MeshDisplayAttributes mesh = target.MeshSpecificAttributes;
        (mesh.HighlightMeshes, mesh.ShowMeshWires, mesh.AllMeshWiresColor) = (Highlight, ShowWires, Quant.Sys(WireColor));
        (mesh.MeshWireThickness, mesh.ShowMeshVertices, target.MeshVertexSize) = (WireWidth, ShowVertices, VertexSize);
        return unit;
    }
}

public sealed record ClipSchema(
    bool ShowPlanes, bool IntersectionSurfaces, bool IntersectionEdges, bool Fills, bool Edges,
    bool SelectionHighlight, bool ShadeSelectedPlane, bool SectionStyles,
    DisplayPipelineAttributes.ClippingPlaneFillColorUse FillUse, PerceptualColor FillColor,
    DisplayPipelineAttributes.ClippingEdgeColorUse EdgeUse, PerceptualColor EdgeColor, int EdgeWidth,
    DisplayPipelineAttributes.ClippingShadeColorUse ShadeUse, PerceptualColor ShadeColor, int ShadeTransparency) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShowClippingPlanes, target.ShowClipIntersectionSurfaces, target.ShowClipIntersectionEdges) = (ShowPlanes, IntersectionSurfaces, IntersectionEdges);
        (target.ShowClippingFills, target.ShowClippingEdges) = (Fills, Edges);
        (target.ClipSelectionHighlight, target.ClippingShadeSelectedPlane, target.UseSectionStyles) = (SelectionHighlight, ShadeSelectedPlane, SectionStyles);
        (target.ClippingPlaneFillColorUsage, target.ClippingFillColor) = (FillUse, Quant.Sys(FillColor));
        (target.ClippingEdgeColorUsage, target.ClippingEdgeColor, target.ClippingEdgeThickness) = (EdgeUse, Quant.Sys(EdgeColor), EdgeWidth);
        (target.ClippingShadeColorUsage, target.ClippingShadeColor, target.ClippingShadeTransparency) = (ShadeUse, Quant.Sys(ShadeColor), ShadeTransparency);
        return unit;
    }
}

public sealed record TechnicalSchema(bool HiddenLines, bool Edges, bool Silhouettes, bool Creases, bool Seams, bool Intersections, bool Lighting) {
    public static TechnicalSchema Drafting { get; } =
        new(HiddenLines: true, Edges: true, Silhouettes: true, Creases: true, Seams: true, Intersections: true, Lighting: false);

    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShowHiddenLines, target.ShowEdges, target.ShowSilhouttes, target.ShowCreases) = (HiddenLines, Edges, Silhouettes, Creases);
        (target.ShowSeams, target.ShowIntersections, target.ShowLighting) = (Seams, Intersections, Lighting);
        return unit;
    }
}

public sealed record LockedSchema(DisplayPipelineAttributes.LockedObjectUse Usage, PerceptualColor Color, int Transparency, bool DrawBehind, bool Ghost, bool LayersFollow) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.LockedObjectUsage, target.LockedColor, target.LockedObjectTransparency) = (Usage, Quant.Sys(Color), Transparency);
        (target.LockedObjectsDrawBehindOthers, target.GhostLockedObjects, target.LayersFollowLockUsage) = (DrawBehind, Ghost, LayersFollow);
        return unit;
    }
}

public sealed record PointSchema(bool Show, PointStyle Style, float Radius, bool ShowClouds, PointStyle CloudStyle, float CloudRadius) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShowPoints, target.PointStyle, target.PointRadius) = (Show, Style, Radius);
        (target.ShowPointClouds, target.PointCloudStyle, target.PointCloudRadius) = (ShowClouds, CloudStyle, CloudRadius);
        return unit;
    }
}

public sealed record GripSchema(
    bool ShowGrips, bool ShowPolygon, PointStyle Style, int WireWidth, int GripSize, bool SolidLines,
    Option<PerceptualColor> FixedColor, bool ShowPoints, bool ShowSurface, bool Highlight) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.ShowGrips, target.ControlPolygonShow, target.ControlPolygonStyle) = (ShowGrips, ShowPolygon, Style);
        (target.ControlPolygonWireThickness, target.ControlPolygonGripSize, target.ControlPolygonUseSolidLines) = (WireWidth, GripSize, SolidLines);
        target.ControlPolygonUseFixedSingleColor = FixedColor.IsSome;
        _ = FixedColor.Iter(color => target.ControlPolygonColor = Quant.Sys(color));
        (target.ControlPolygonShowPoints, target.ControlPolygonShowSurface, target.ControlPolygonHighlight) = (ShowPoints, ShowSurface, Highlight);
        return unit;
    }
}

public sealed record EffectSchema(Option<(PerceptualColor Color, float Amount)> Fade, Option<float> Dither, Option<(float Strength, float Width)> Hatch) {
    internal Unit Write(DisplayPipelineAttributes target) {
        _ = Fade.Iter(fade => target.SetColorFadeEffect(Quant.Sys(fade.Color), fade.Amount));
        _ = Dither.Iter(amount => target.SetDitherTransparencyEffect(amount));
        _ = Hatch.Iter(hatch => target.SetDiagonalHatchEffect(hatch.Strength, hatch.Width));
        return unit;
    }
}

public sealed record PipelineSchema(
    bool XrayAll, bool IgnoreHighlights, bool DisableConduits, bool DisableTransparency, bool ShowText, bool ShowAnnotations,
    DisplayPipelineAttributes.BoundingBoxDisplayMode BoundingBox, DisplayPipelineAttributes.DynamicDisplayUse DynamicDisplay,
    ScopeSource Workflow, bool PreColors, bool PreTextures, float PreGamma, bool PostFrameBuffer, float PostGamma,
    bool BakeTextures, int RealtimePasses, bool RealtimeProgress) {
    internal Unit Write(DisplayPipelineAttributes target) {
        (target.XrayAllObjects, target.IgnoreHighlights, target.DisableConduits, target.DisableTransparency) = (XrayAll, IgnoreHighlights, DisableConduits, DisableTransparency);
        (target.ShowText, target.ShowAnnotations, target.BoundingBoxMode, target.DynamicDisplayUsage) = (ShowText, ShowAnnotations, BoundingBox, DynamicDisplay);
        target.LinearWorkflowUsage = Workflow.Workflow;
        (target.PreProcessColors, target.PreProcessTextures, target.PreProcessGamma) = (PreColors, PreTextures, PreGamma);
        (target.PostProcessFrameBuffer, target.PostProcessGamma) = (PostFrameBuffer, PostGamma);
        (target.BakeTextures, target.RealtimeRenderPasses, target.ShowRealtimeRenderProgressBar) = (BakeTextures, RealtimePasses, RealtimeProgress);
        return unit;
    }
}

public sealed record DisplayProfile(
    Option<ShadingSchema> Shading = default,
    Option<EdgeSchema> Edges = default,
    Option<LightSchema> Lighting = default,
    Option<GroundSchema> Ground = default,
    Option<GridSchema> Grid = default,
    Option<SubDSchema> SubD = default,
    Option<MeshSchema> Mesh = default,
    Option<ClipSchema> Clipping = default,
    Option<TechnicalSchema> Technical = default,
    Option<LockedSchema> Locked = default,
    Option<PointSchema> Points = default,
    Option<GripSchema> Grips = default,
    Option<EffectSchema> Effects = default,
    Option<PipelineSchema> Pipeline = default) {
    public static DisplayProfile Empty { get; } = new();

    internal Unit Write(DisplayPipelineAttributes attributes) {
        _ = Shading.Iter(row => row.Write(target: attributes));
        _ = Edges.Iter(row => row.Write(target: attributes));
        _ = Lighting.Iter(row => row.Write(target: attributes));
        _ = Ground.Iter(row => row.Write(target: attributes));
        _ = Grid.Iter(row => row.Write(target: attributes));
        _ = SubD.Iter(row => row.Write(target: attributes));
        _ = Mesh.Iter(row => row.Write(target: attributes));
        _ = Clipping.Iter(row => row.Write(target: attributes));
        _ = Technical.Iter(row => row.Write(target: attributes));
        _ = Locked.Iter(row => row.Write(target: attributes));
        _ = Points.Iter(row => row.Write(target: attributes));
        _ = Grips.Iter(row => row.Write(target: attributes));
        _ = Effects.Iter(row => row.Write(target: attributes));
        _ = Pipeline.Iter(row => row.Write(target: attributes));
        return unit;
    }
}
```

## [03]-[MODE_MANAGEMENT]

- Owner: `BuiltinMode` `[SmartEnum<int>]` — the host's special-mode roster (`Wireframe`, `Shaded`, `Rendered`, `RenderedShadows`, `Ghosted`, `XRay`, `Tech`, `Artistic`, `Pen`, `Monochrome`, `AmbientOcclusion`, `Raytraced`), each row's `ModeId()` column deferring to the matching `DisplayModeDescription` id static. `ModePolicy` — the descriptor's pipeline policy patch: `Option` slots over `InMenu`, `SupportsShadeCommand`, `SupportsShading`, `AllowObjectAssignment`, `ShadedPipelineRequired`, `WireframePipelineRequired`, `PipelineLocked`, plus the `EnglishName` rename. `Modes` — the entry rail: `Configure` resolves the descriptor through conduit.md's `ModeOp.FindCase`, folds the policy onto the descriptor and the profile onto its `DisplayAttributes` editor, and persists through `ModeOp.UpdateCase`; `Derive` runs `ModeOp.CopyCase` then `Configure`, so a custom mode built from a built-in is one call.
- Law: `CopyDisplayMode` registers the new mode in memory only — `UpdateDisplayMode` persists it to disk, and an unpersisted mode silently vanishes when Rhino reloads the mode list — so every configure and derive terminates in `UpdateCase`, never a bare copy.
- Law: `DisplayAttributes` hands back the descriptor's own live attribute object, so the profile fold mutates the resolved descriptor in place and `UpdateCase` lands exactly the object it configured — no attribute value is copied, detached, or re-resolved between fold and persist.
- Boundary: table CRUD — census, find, named, add, update, import, export, copy, delete — is conduit.md's `ModeOp`; this page composes its cases and adds no parallel table verb. A built-in mode's own appearance is host-owned: `Configure` against a built-in id edits the user's mode definition exactly as the options dialog does, and `Derive` is the non-destructive route.
- Growth: a new special-mode id is one `BuiltinMode` row; a new descriptor flag is one `ModePolicy` slot; a richer derivation (import-then-configure) composes existing `ModeOp` cases inside `Modes`, never a new entry family.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class BuiltinMode {
    public static readonly BuiltinMode Wireframe = new(key: 0, modeId: static () => DisplayModeDescription.WireframeId);
    public static readonly BuiltinMode Shaded = new(key: 1, modeId: static () => DisplayModeDescription.ShadedId);
    public static readonly BuiltinMode Rendered = new(key: 2, modeId: static () => DisplayModeDescription.RenderedId);
    public static readonly BuiltinMode RenderedShadows = new(key: 3, modeId: static () => DisplayModeDescription.RenderedShadowsId);
    public static readonly BuiltinMode Ghosted = new(key: 4, modeId: static () => DisplayModeDescription.GhostedId);
    public static readonly BuiltinMode XRay = new(key: 5, modeId: static () => DisplayModeDescription.XRayId);
    public static readonly BuiltinMode Tech = new(key: 6, modeId: static () => DisplayModeDescription.TechId);
    public static readonly BuiltinMode Artistic = new(key: 7, modeId: static () => DisplayModeDescription.ArtisticId);
    public static readonly BuiltinMode Pen = new(key: 8, modeId: static () => DisplayModeDescription.PenId);
    public static readonly BuiltinMode Monochrome = new(key: 9, modeId: static () => DisplayModeDescription.MonochromeId);
    public static readonly BuiltinMode AmbientOcclusion = new(key: 10, modeId: static () => DisplayModeDescription.AmbientOcclusionId);
    public static readonly BuiltinMode Raytraced = new(key: 11, modeId: static () => DisplayModeDescription.RaytracedId);

    [UseDelegateFromConstructor]
    public partial Guid ModeId();
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ModePolicy(
    Option<string> Rename = default,
    Option<bool> InMenu = default,
    Option<bool> ShadeCommand = default,
    Option<bool> Shading = default,
    Option<bool> ObjectAssignment = default,
    Option<bool> ShadedPipeline = default,
    Option<bool> WireframePipeline = default,
    Option<bool> Locked = default) {
    internal Unit Write(DisplayModeDescription mode) {
        _ = Rename.Iter(value => mode.EnglishName = value);
        _ = InMenu.Iter(value => mode.InMenu = value);
        _ = ShadeCommand.Iter(value => mode.SupportsShadeCommand = value);
        _ = Shading.Iter(value => mode.SupportsShading = value);
        _ = ObjectAssignment.Iter(value => mode.AllowObjectAssignment = value);
        _ = ShadedPipeline.Iter(value => mode.ShadedPipelineRequired = value);
        _ = WireframePipeline.Iter(value => mode.WireframePipelineRequired = value);
        _ = Locked.Iter(value => mode.PipelineLocked = value);
        return unit;
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Modes {
    public static Fin<DisplayModeDescription> Configure(Guid modeId, Option<ModePolicy> policy = default, Option<DisplayProfile> profile = default, Op? key = null) {
        Op op = key.OrDefault();
        return from resolved in new ModeOp.FindCase(ModeId: modeId).Apply(key: op).Bind(modes => modes.Head.ToFin(Fail: op.InvalidResult()))
               from _ in op.Catch(() => {
                   _ = policy.Iter(row => row.Write(mode: resolved));
                   _ = profile.Iter(row => row.Write(attributes: resolved.DisplayAttributes));
                   return Fin.Succ(value: unit);
               })
               from __ in new ModeOp.UpdateCase(Mode: resolved).Apply(key: op)
               select resolved;
    }

    public static Fin<DisplayModeDescription> Derive(Guid sourceId, string name, Option<ModePolicy> policy = default, Option<DisplayProfile> profile = default, Op? key = null) {
        Op op = key.OrDefault();
        return from copied in new ModeOp.CopyCase(SourceId: sourceId, Name: name).Apply(key: op).Bind(modes => modes.Head.ToFin(Fail: op.InvalidResult()))
               from configured in Configure(modeId: copied.Id, policy: policy, profile: profile, key: op)
               select configured;
    }
}
```

## [04]-[VIEWPORT_ASSIGNMENT]

- Owner: `ViewportModes` — the per-viewport binding rail: `Assign` resolves the descriptor and sets `RhinoViewport.DisplayMode` under the viewport lease, `Assigned` reads the bound mode id off the leased viewport, and `Capture` renders one frame under a passed mode through the `RhinoView.CaptureToBitmap(DisplayModeDescription)` overloads, with `Size2i` selecting the sized overload and the frame delivered as Viewport/capture.md's leased `CaptureArtifact.RasterCase` — a bare host bitmap never crosses.
- Law: assignment resolves through `GetDisplayMode` before the lease opens, so a dangling mode id refuses without touching the viewport; the `DisplayMode` set is a host property assignment inside the borrow.
- Law: mode-scoped capture never mutates the viewport's assigned mode — the overload renders the frame under the passed descriptor and the binding survives untouched.
- Law: the attribute-typed `CaptureToBitmap` overloads collapse into the mode-typed pair — a public attribute object exists only through a descriptor, so the mode id is the one capture discriminant and an attributes parameter would re-describe it.
- Boundary: settings-driven capture — media size, layout, decorations, scale, printer — is Viewport/capture.md's `CapturePlan`/`Captures.Run`; this entry owns only the display-mode-scoped frame and mints it through that page's `CaptureArtifact.Raster` factory.
- Growth: a per-detail or broadcast assignment is arity on `ViewportTarget` resolution, never a sibling entry.

```csharp
// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ViewportModes {
    public static Fin<Unit> Assign(DocumentSession session, ViewportTarget target, Guid modeId, Op? key = null) {
        Op op = key.OrDefault();
        return from mode in Optional(DisplayModeDescription.GetDisplayMode(id: modeId)).ToFin(Fail: op.InvalidInput())
               from lease in ViewportLease.Of(session: session, target: target, key: op)
               from _ in lease.Use(borrow: row => op.Catch(() => Fin.Succ(value: Op.Side(() => row.Viewport.DisplayMode = mode))), key: op)
               select unit;
    }

    public static Fin<Guid> Assigned(DocumentSession session, ViewportTarget target, Op? key = null) {
        Op op = key.OrDefault();
        return from lease in ViewportLease.Of(session: session, target: target, key: op)
               from bound in lease.Use(borrow: row => op.Catch(() =>
                   Optional(row.Viewport.DisplayMode).ToFin(Fail: op.InvalidResult()).Map(static mode => mode.Id)), key: op)
               select bound;
    }

    public static Fin<CaptureArtifact> Capture(DocumentSession session, ViewportTarget target, Guid modeId, Option<Size2i> extent = default, Op? key = null) {
        Op op = key.OrDefault();
        return from mode in Optional(DisplayModeDescription.GetDisplayMode(id: modeId)).ToFin(Fail: op.InvalidInput())
               from lease in ViewportLease.Of(session: session, target: target, key: op)
               from shot in lease.Use(borrow: row => op.Catch(() => Optional(extent.Match(
                       Some: size => row.View.CaptureToBitmap(size: size.Native, mode: mode),
                       None: () => row.View.CaptureToBitmap(mode: mode))).ToFin(Fail: op.InvalidResult())), key: op)
               from artifact in CaptureArtifact.Raster(bitmap: shot, key: op)
               select artifact;
    }
}
```

## [05]-[BUILTIN_ANALYSIS]

- Owner: `BuiltinAnalysis` `[SmartEnum<int>]` — the built-in visual-analysis roster: `Edge`, `CurvatureGraph`, `Zebra`, `Emap`, `CurvatureColor`, `DraftAngle`, `Thickness`, `EdgeContinuity`, `Direction`, `End` — each row's `ModeId()` column deferring to the host id static (the curvature-color and edge-continuity statics carry the host's own misspellings, `RhinoCurvatureColorAnalyisModeId` and `RhinoEdgeContinuityAlalysisModeId`, cited verbatim), and `Resolve` returning the live `VisualAnalysisMode` through `Find(Guid)` — the resolved mode carries `Style` (`AnalysisStyle`: wireframe, texture, false-color) and `Name` as host facts. `AnalysisReceipt` — the typed result: touched object ids plus active mode ids. `AnalysisOp` `[Union]` — the attachment rail: `AttachCase`/`DetachCase` toggle a mode across an object set through `RhinoObject.EnableVisualAnalysisMode`, `CensusCase` projects one object's `GetActiveVisualAnalysisModes`, `AdjustMeshesCase` runs the interactive `AdjustAnalysisMeshes` density adjustment, `UiCase` toggles the resolved mode's `EnableUserInterface` dialog surface, and `CurvatureAutoRangeCase`/`CurvatureMaxRangeCase` drive the Rhino 9 curvature-range statics.
- Law: attach gates each object on `ObjectSupportsAnalysisMode` before `EnableVisualAnalysisMode`, so an unsupported object is a typed refusal, never a silent no-op; the redraw lands once per operation through `Views.Redraw`, never per object.
- Law: census returns mode ids as facts — custom-registered mode ids ride the same `Seq<Guid>` as built-ins, so the receipt never forces a lossy row lookup.
- Boundary: custom analysis-mode registration, draw hooks, and `Register(Type)` lifecycle are conduit.md's `AnalysisProgram`; this page owns built-in attachment and the range controls, and the two compose on one object without overlap.
- Growth: a new built-in id is one row; a new document-level control is one stateless case; a per-object analysis parameter surface lands as case payload, never a flag on the entry.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class BuiltinAnalysis {
    public static readonly BuiltinAnalysis Edge = new(key: 0, modeId: static () => VisualAnalysisMode.RhinoEdgeAnalysisModeId);
    public static readonly BuiltinAnalysis CurvatureGraph = new(key: 1, modeId: static () => VisualAnalysisMode.RhinoCurvatureGraphAnalysisModeId);
    public static readonly BuiltinAnalysis Zebra = new(key: 2, modeId: static () => VisualAnalysisMode.RhinoZebraStripeAnalysisModeId);
    public static readonly BuiltinAnalysis Emap = new(key: 3, modeId: static () => VisualAnalysisMode.RhinoEmapAnalysisModeId);
    public static readonly BuiltinAnalysis CurvatureColor = new(key: 4, modeId: static () => VisualAnalysisMode.RhinoCurvatureColorAnalyisModeId);
    public static readonly BuiltinAnalysis DraftAngle = new(key: 5, modeId: static () => VisualAnalysisMode.RhinoDraftAngleAnalysisModeId);
    public static readonly BuiltinAnalysis Thickness = new(key: 6, modeId: static () => VisualAnalysisMode.RhinoThicknessAnalysisModeId);
    public static readonly BuiltinAnalysis EdgeContinuity = new(key: 7, modeId: static () => VisualAnalysisMode.RhinoEdgeContinuityAlalysisModeId);
    public static readonly BuiltinAnalysis Direction = new(key: 8, modeId: static () => VisualAnalysisMode.RhinoDirectionAnalysisModeId);
    public static readonly BuiltinAnalysis End = new(key: 9, modeId: static () => VisualAnalysisMode.RhinoEndAnalysisModeId);

    [UseDelegateFromConstructor]
    public partial Guid ModeId();

    public Fin<VisualAnalysisMode> Resolve(Op? key = null) =>
        Optional(VisualAnalysisMode.Find(id: ModeId())).ToFin(Fail: key.OrDefault().InvalidResult());
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record AnalysisReceipt(Seq<Guid> Objects, Seq<Guid> Active);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalysisOp {
    private AnalysisOp() { }
    public sealed record AttachCase(Seq<Guid> Objects, BuiltinAnalysis Analysis) : AnalysisOp;
    public sealed record DetachCase(Seq<Guid> Objects, BuiltinAnalysis Analysis) : AnalysisOp;
    public sealed record CensusCase(Guid Target) : AnalysisOp;
    public sealed record AdjustMeshesCase(BuiltinAnalysis Analysis) : AnalysisOp;
    public sealed record UiCase(BuiltinAnalysis Analysis, bool Visible) : AnalysisOp;
    public sealed record CurvatureAutoRangeCase : AnalysisOp;
    public sealed record CurvatureMaxRangeCase : AnalysisOp;

    public Fin<AnalysisReceipt> Apply(DocumentSession session, Op? key = null) {
        Op op = key.OrDefault();
        return Switch(
            state: (Session: session, Op: op),
            attachCase: static (ctx, request) => Toggle(session: ctx.Session, objects: request.Objects, analysis: request.Analysis, enable: true, key: ctx.Op),
            detachCase: static (ctx, request) => Toggle(session: ctx.Session, objects: request.Objects, analysis: request.Analysis, enable: false, key: ctx.Op),
            censusCase: static (ctx, request) => ctx.Session.Demand(
                use: document => ctx.Op.Catch(() =>
                    from subject in Optional(document.Objects.FindId(request.Target)).ToFin(Fail: ctx.Op.InvalidInput())
                    select new AnalysisReceipt(
                        Objects: [request.Target],
                        Active: toSeq(subject.GetActiveVisualAnalysisModes()).Map(static mode => mode.Id))),
                key: ctx.Op,
                needs: [SessionNeed.Read]),
            adjustMeshesCase: static (ctx, request) => ctx.Session.Demand(
                use: document => request.Analysis.Resolve(key: ctx.Op).Bind(mode =>
                    ctx.Op.Confirm(success: VisualAnalysisMode.AdjustAnalysisMeshes(doc: document, analysisModeId: mode.Id))
                        .Map(_ => new AnalysisReceipt(Objects: [], Active: [mode.Id]))),
                key: ctx.Op,
                needs: [SessionNeed.Read, SessionNeed.Dialog]),
            uiCase: static (ctx, request) => request.Analysis.Resolve(key: ctx.Op).Bind(mode =>
                ctx.Op.Catch(() => Fin.Succ((Op.Side(() => mode.EnableUserInterface(on: request.Visible)), new AnalysisReceipt(Objects: [], Active: [mode.Id])).Item2))),
            curvatureAutoRangeCase: static (ctx, _) => ctx.Op.Catch(() =>
                Fin.Succ((Op.Side(VisualAnalysisMode.CurvatureColorAutoRange), new AnalysisReceipt(Objects: [], Active: [])).Item2)),
            curvatureMaxRangeCase: static (ctx, _) => ctx.Op.Catch(() =>
                Fin.Succ((Op.Side(VisualAnalysisMode.CurvatureColorMaxRange), new AnalysisReceipt(Objects: [], Active: [])).Item2)));
    }

    private static Fin<AnalysisReceipt> Toggle(DocumentSession session, Seq<Guid> objects, BuiltinAnalysis analysis, bool enable, Op key) =>
        session.Demand(
            use: document => analysis.Resolve(key: key).Bind(mode =>
                objects.TraverseM(id =>
                        from subject in Optional(document.Objects.FindId(id)).ToFin(Fail: key.InvalidInput())
                        from _ in guard(mode.ObjectSupportsAnalysisMode(subject), key.InvalidInput()).ToFin()
                        from __ in key.Confirm(success: subject.EnableVisualAnalysisMode(mode, enable))
                        select id).As()
                    .Map(touched => (Op.Side(() => document.Views.Redraw()), new AnalysisReceipt(Objects: touched, Active: [mode.Id])).Item2)),
            key: key,
            needs: [SessionNeed.Read, SessionNeed.Redraw]);
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]                 | [OWNER]                          | [FORM]                                                     | [ENTRY]                    |
| :-----: | :------------------------ | :------------------------------- | :--------------------------------------------------------- | :------------------------- |
|  [01]   | appearance patch          | `DisplayProfile`                 | `Option`-sloted concern schemas folded onto the editor     | `Modes.Configure`          |
|  [02]   | width/scope discriminants | `WidthSource`, `ScopeSource`     | isomorphic host-enum collapse rows                         | schema fields              |
|  [03]   | frame-buffer fill         | `FillSpec`                       | one union over `FillMode` plus the `SetFill` overloads     | `ShadingSchema.Fill`       |
|  [04]   | object paint source       | `ObjectPaint`                    | three-way source union over the exclusive host bools       | `ShadingSchema.Paint`      |
|  [05]   | SubD/mesh edge classes    | `SubDEdgeClass`, `MeshEdgeClass` | writer-column `[SmartEnum]` tables over per-class members  | `SubDSchema`/`MeshSchema`  |
|  [06]   | descriptor policy flags   | `ModePolicy`                     | `Option`-sloted descriptor patch plus rename               | `Modes.Configure`          |
|  [07]   | built-in mode roster      | `BuiltinMode`                    | id rows deferring to the host special-mode statics         | `Modes.Derive` source      |
|  [08]   | mode configure/derive     | `Modes`                          | `ModeOp` find/copy fold, profile write, update persist     | `Modes.Configure`/`Derive` |
|  [09]   | per-viewport binding      | `ViewportModes`                  | leased `DisplayMode` assignment and read-back              | `ViewportModes.Assign`     |
|  [10]   | mode-scoped capture       | `ViewportModes`                  | descriptor-typed `CaptureToBitmap` under the lease         | `ViewportModes.Capture`    |
|  [11]   | built-in analysis roster  | `BuiltinAnalysis`                | id rows plus live `Find` resolution                        | `AnalysisOp` payloads      |
|  [12]   | analysis attachment       | `AnalysisOp`                     | one union over enablement, census, mesh/UI/range controls  | `AnalysisOp.Apply`         |
