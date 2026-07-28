# [MATERIALS_GRAPH]

THE NODE-GRAPH APPEARANCE ENGINE and THE POLYMORPHIC MATERIAL LIBRARY. One `AppearanceNode` `[Union]` closes the node-kind family — `Input`, `Texture`, `Math`, `Mix`, `Normal`, `BsdfOutput` — over the typed `PortValue` channel set; one `MaterialGraph.Compile` fold orders the node DAG ONCE on the shared QuikGraph substrate (`IsDirectedAcyclicGraph` gate, `SourceFirstTopologicalSort` order), resolves its sink and its two rentals, and freezes the whole result, and `CompiledGraph.ShadeSpan` re-enters that frozen order over a caller-rented index-addressed scratch whose slot order IS the sort — the per-sample `CompiledGraph.Shade` being that SAME rail over a one-element window, so ONE representation, ONE `NodeEvaluator.Apply` algebra over pre-resolved operands, and ONE `Assemble` gamut gate carry both the integrator and the bake, a plane pays one allocation-free pass per row, and the per-node immutable-map rebuild the per-point rail once paid is gone from the hot path too — and the node algebra is POINTWISE by construction with every neighbourhood kernel owned by `Raster/filter#PLANE_OP` over a whole plane. One `MaterialParameters` record is the canonical Disney-principled parameter vector — sixteen positional columns with the init-defaulted `Film` thin-film carrier — every measured material parameterizes, and one `MaterialLibrary` `FrozenDictionary<MaterialId, MaterialParameters>` is the catalog as DATA ROWS — metal, glass, liquid, gas, gem, stone, plastic, rubber, polymer, skin, fabric, paint, ceramic, wax, wood, coat — so a new material is `MaterialLibrary.Rows[MaterialId.Of("metal.titanium")] = new MaterialParameters(...)`, a row of values, NEVER a `TitaniumMaterial` type. `Rasm.Materials.Appearance.Graph` OWNS the `PortId`/`MathOp`/`MixOp`/`PortValue`/`GraphContext` graph vocabulary (the `MixOp` table one `BlendMode` row per W3C compositing member, the blend behavior a DATA column, never sixteen delegates), the `ShadePoint`/`AppearanceNode`/`SurfaceShade`/`PortSlot`/`CompiledGraph`/`MaterialGraph` evaluation surfaces, the `SubsurfaceRadius` mean-free-path and `ThinFilm` interference carriers, the `MaterialParameters` row, and the `MaterialLibrary` catalog/admission/reference folds; it COMPOSES the SEAM `Rasm.Element` `MaterialId` identity (never re-minting a `family.name` key), the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail (never a second fault), the Rasm.Numerics `Direction`/`VectorFrame`/`Context` shading frame (never re-minting a vector or a tolerance), the `texture#TEXTURE_UV` `TextureUv.Port` closure for the `Texture` arm (never re-implementing sampling), QuikGraph as the one graph-algorithm substrate the whole stack folds transient graphs onto (never a hand-rolled Kahn walk), and Wacton.Unicolour directly as the scene-linear/spectral/compositing color owner under the one `Acescg` working space (never re-minting a `ColourSpace`). `SurfaceShade` terminates the graph as the resolved parameter snapshot the `surface#OPENPBR_SLAB` `SlabStack.ToLayered` lowers to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` the integrator shades — the graph resolves the parameters, the lobe math living on the `bsdf`/`surface` pages, never re-derived here. `MaterialId` generalizes the masonry-assignment consumer: a masonry `Component` maps to a `MaterialId`, never to a component-specific material type.

## [01]-[INDEX]

- [02]-[MATERIAL_GRAPH]: `PortId`/`MathOp`/`MixOp` carry the graph vocabulary (`MixOp` the 16-row `BlendMode` table), `PortValue` the channel set, `GraphContext` the tolerant-`Context` carrier, `AppearanceNode` the node union over its `Produces` column, `ShadePoint`/`PortSlot`/`CompiledGraph`/`MaterialGraph` the QuikGraph-ordered evaluation fold over the one slot-addressed `ShadeSpan` rail its per-point `Shade` window re-enters, and `SurfaceShade` the sink.
- [03]-[MATERIAL_LIBRARY]: `MaterialLibrary` catalogs `MaterialParameters` rows under the seam `MaterialId` key over the `SubsurfaceRadius` mean-free-path and `ThinFilm` interference carriers, generalizes profile assignment, and gates through the `NearestChecker`/`HueConstant`/`Named` Datasets validation seam over the reflection-derived reference tables, the three physical-reproducibility gates by domain — display RGB (`SurfaceShade.InGamut`), Pointer real-surface (`PointerAdmit`/`MapToPointer`), and MacAdam spectral-limit (`SpectralAdmit`/`MapToSpectral`) — with the `CvdPreview`/`Contrast`/`NearestIscc` accessibility and designation projections.

## [02]-[MATERIAL_GRAPH]

- Owner: `MaterialGraph`/`CompiledGraph` over `AppearanceNode`; the `PortId`/`MathOp`/`MixOp`/`PortValue` graph vocabulary; the `GraphContext` tolerant-`Context` carrier; the `ShadePoint`/`SurfaceShade`/`PortSlot` evaluation models.
- Cases: `Input` (constant/parameter source) · `Texture` (UV-sampled source — the `texture#TEXTURE_UV` `TextureUv.Port` closure) · `Math` (closed scalar/vector op over upstream ports) · `Mix` (parameterized `BlendMode` composite of two ports) · `Normal` (tangent-space perturbation of the shading frame) · `BsdfOutput` (the single sink assembling the closed lobe set into a `SurfaceShade`)
- Entry: `public Fin<Unit> ShadeSpan(ReadOnlySpan<ShadePoint> points, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> shades, Op key)` is the ONE evaluation rail — `Raster/press#TEXTURE_PRESS` drives it per band, and the per-point `public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Op key)` the integrator holds re-enters it over a one-element window, so no second environment representation exists to drift from — with `ScratchWidth` and `OperandWidth` the two `Compile`-resolved rentals a batched caller sizes against; `public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key)` is the ONE-SHOT convenience (Compile + Shade for a single sample), while the per-sample path `Compile`s ONCE into a frozen `CompiledGraph` and re-enters it per sample, so the hot loop pays the sort once per material, never per ray. `Fin<T>` aborts at COMPILE on a cyclic DAG (`MaterialFault.Graph`, key-correlated), a duplicate node id, a dangling port reference, a dependency on a non-producing port, or a missing/non-`BsdfOutput` sink, and at SHADE on a short span rental, a degenerate frame perturbation, or an out-of-gamut assembled shade — each shade-time failure re-wrapped with the failing TEXEL INDEX, since a plane fails at one of sixteen million points that all ran the same program (a port-TYPE mismatch cannot fault at all — the `PortValue.AsScalar`/`AsColor`/`AsVector` projections are total by construction); `MaterialGraph.Default` is the canonical Disney-principled wiring every library row drives through.
- Packages: QuikGraph (composed — `AdjacencyGraph<PortId, SEdge<PortId>>` with `allowParallelEdges: false`, `AddVertexRange` admitting isolates, `AddVerticesAndEdge` per dependency edge, `AlgorithmExtensions.IsDirectedAcyclicGraph` the cheap cycle pre-gate, `AlgorithmExtensions.SourceFirstTopologicalSort` the Kahn order — the one graph-algorithm substrate `Rasm.Element`/`Rasm.Persistence`/`Rasm.Bim` already fold onto, admitted folder-locally against the central pin), Rasm (project — `Direction`/`VectorFrame`/`Context`/`Op`, `Rhino.Geometry.Point3d`/`Vector3d`/`Plane` at the host edge), Rasm.Element (the SEAM `MaterialId`, composed not re-declared), Rasm.Materials.Appearance.Bsdf (the `MaterialFault` band-2450 rail composed from `bsdf#SHADING_FRAME`), Wacton.Unicolour (color/spectral/compositing compose — `Mix`, `Blend(backdrop, BlendMode)`, the 16-member `BlendMode` vocabulary), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`). `texture#TEXTURE_UV` `TextureUv.Port` mints the `Texture` arm's closure — `texture` COMPOSES this page's `PortValue`/`PortId`/`ShadePoint`, so graph stays the LOWER owner and the `Texture` case carries only a host-free `Func<double,double,PortValue>`, never a `texture`-namespace type (no cyclic namespace dependency).
- Growth: a new appearance operation is one `MathOp` row (the operation behavior rides the SmartEnum row's `[UseDelegateFromConstructor]` delegate — the roster spans arithmetic incl. floored `modulo`, the unary transcendentals `sqrt`/`abs`/`sin`/`cos`, min/max, the vector `dot`/`cross`/`normalize`, unit clamps, and the Schlick weight, each keyed to its MaterialX standard math category) or one `MixOp` row naming its `BlendMode` member (the blend behavior IS the `Mode` data column the ONE `Apply` derivation reads — never a new arm, never a hand-rolled channel composite); a genuinely new node KIND with no parameterization of the six is one `AppearanceNode` case; a new port channel is one `PortValue` case carrying its CLR carrier; a new lobe assembled at the sink is one `BsdfLobe` `[Union]` case on the `bsdf` page — never a per-effect graph variant and never a sibling node type. `interchange#MATERIALX_DOCUMENT` projects its `NodeCategory`/`MtlxPort` map onto the `AppearanceNode` union and `PortValue` set, the MaterialX 1.39 node-graph alignment target.
- Boundary: the node DAG is the only appearance-program shape — a per-material hand-written shade function is the deleted form; `PortValue` is the only inter-node channel and carries scalar/`Unicolour`/`Vector3d`/`VectorFrame` polarities so a node arm reads typed ports and never `object`; the `Color`→`Scalar` projection is the AP1 scene-linear luminance dot `0.2722287 R + 0.6740818 G + 0.0536895 B` (the AP1-primary luminance row consistent with the declared `Acescg` working space and the `bsdf#LOBE_FAMILY` `RgbSpectrum.Luminance` weights — a Rec709 weight on AP1-linear channels is the colorimetric defect, biasing a green-heavy mask), never a red-channel read, so a mask pulled from a color is photometrically weighted and cannot silently bias to red; the `Texture` arm carries the TOTAL `Func<double,double,PortValue>` closure the `texture#TEXTURE_UV` `TextureUv.Port(TextureSource, UvSample, SamplerState, Channel, Op)` mints — the node holds the delegate, the sampling fold lives on the `texture` page, and the arm never re-implements a sampler nor admits a raw caller-supplied lambda that bypasses the `Channel`-neutralized fault rail; the `Normal` arm perturbs the composed Rasm.Numerics `VectorFrame` (tangent·bitangent·normal) and never re-mints a basis; the `BsdfOutput` arm assembles the `SurfaceShade` parameter snapshot (resolved base color, metalness, roughness, perturbed shading frame, emission) the renderer reads — the lobe WEIGHTING is the downstream `surface#OPENPBR_SLAB` `SlabStack.ToLayered` lowering of the `MaterialParameters` row to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` the integrator shades, the graph sink being the resolved parameter shade and the lobe math living wholly on the `bsdf`/`surface` pages, never re-derived here, color resolved through the directly-consumed Wacton.Unicolour `RgbConfiguration.Acescg` scene-linear owner; the `BsdfOutput` sink resolves through `Assemble` behind a pattern-matched sink probe (a non-`BsdfOutput` sink rails `MaterialFault.Graph`, never an unchecked cast), never a port write, so the environment carries no dead entry under the sink id and a downstream node cannot read a phantom `Scalar(1.0)`; the `Math` arm folds over its `MathOp` SmartEnum by delegate row so a new operation is a row, never a new arm, and the `MathOp.Fresnel` row supplies only the Schlick angular weight `(1−cosθ)⁵` for a `Mix` lobe blend — the full Fresnel term lives on `bsdf#MICROFACET_KERNEL`, never re-derived here; the `Mix` arm dispatches `b.AsColor.Blend(a.AsColor, Mode)` — the W3C separable/non-separable compositing algebra Unicolour owns, `a` the backdrop, `b` the source, the factor the blend opacity lerped in scene-linear `RgbLinear` — so all sixteen W3C modes are one data column and the prior three-mode hand-rolled `ChannelCompose` channel math is the deleted form; the `Lerp` row IS `BlendMode.Normal` spelled as the HDR-safe scene-linear `Unicolour.Mix` (the blend algebra clips to the `[0,1]` W3C reflectance domain; an over-unity INTERMEDIATE — a scaled mask, a `Math` product — keeps its `>1` channels through the linear arm, while a sink-bound emission port is NORMALIZED chromaticity by construction, `MaterialParameters.EmissionLuminance` carrying the energy, so the `Assemble` `InGamut` gate holds); the node algebra is POINTWISE by construction and the `AppearanceNode` union admits no neighbourhood operation — a blur, a normal-from-height integration, an ambient-occlusion sweep, or any other kernel reading a texel's neighbours lives at `Raster/filter#PLANE_OP` over a whole `Raster/plane#TEXTURE_PLANE`, because a DAG node evaluated per shading point has no neighbours to read, so a node kind pretending otherwise either fabricates them or forces every sample to carry a plane; the press bakes the DAG's pointwise field first and folds the plane algebra AFTER, so the two owners compose in one direction and neither re-implements the other; `Compile` folds the DAG onto the QuikGraph substrate ONCE — `AddVertexRange` admits every node so an isolate still orders, `AddVerticesAndEdge` adds one dependency→dependent `SEdge<PortId>` per KNOWN dependency (`allowParallelEdges: false` deduplicating an authored `Lhs == Rhs` double edge), and one `ANSWERABILITY` sweep railing `MaterialFault.Graph` at COMPILE over the two failures a slot-addressed read cannot distinguish at runtime — a port no node declares (`<dangling-port>`) and a port whose node `Produces` nothing (`<non-producing-port>`, the sink a dependent named) — because both read an UNWRITTEN scratch cell carrying the previous texel's value rather than faulting, where the per-point map read once railed cleanly, so the proof is what keeps one rail total and a per-texel liveness check is exactly the cost the frozen order exists to delete; `IsDirectedAcyclicGraph` pre-gates a cycle onto `MaterialFault.Graph` before `SourceFirstTopologicalSort` throws `NonAcyclicGraphException`, and the sink resolves to its `BsdfOutput` at COMPILE so no rail re-probes a cast per sample; `ShadeSpan` then re-enters the frozen order against a caller-rented `Span<PortValue>` scratch and `Shade` re-enters `ShadeSpan` over a one-element window, so ONE `NodeEvaluator.Apply` algebra over pre-resolved operands and ONE `Assemble` reading that scratch DIRECTLY close every evaluation at the same gamut gate — a `Func<PortId, Fin<PortValue>>` port reader cannot exist here at all, since a lambda may not capture a `Span<T>`, and a second environment shape minted to dodge that is the divergence this collapse forecloses; the prior hand-rolled indegree/`Queue`/`CollectionsMarshal` Kahn kernel is DELETED for the substrate's own catalogued `AdjacencyGraph` construction seam, and the page's ONE `[EXPRESSION_SPINE]` exemption is the `ShadeSpan` span kernel — a fixed-extent index walk over caller-owned buffers, the doctrine's named span-loop carve — while every admission, dispatch, and egress surface on the page is expression-bodied; `GraphContext.Tolerant` is the one tolerant `Context` the `Normal`/`ShadePoint` arms construct the `VectorFrame` through (a millimetre-scale model `Context` whose `Fin` admission the page resolves once, so a near-degenerate perturbation re-seeds a perpendicular tangent through the `Rasm.Numerics` owner rather than faulting mid-shade); `MaterialGraph.Default` carries the geometric frame unperturbed through one `Normal` node at `Strength 0` whose identity tangent-space sample `(0.5,0.5,1.0)` decodes to `+Z`, so a library row is parameters evaluated through this one standard graph, never a per-row graph type; a cycle, a dangling port, a duplicate node id, or a non-`BsdfOutput` sink rails `Fin.Fail` and never propagates a NaN shade outward.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Reflection;                          // the definition-time Datasets field derivation (IsccNbs names, EbnerFairchild loci)
using LanguageExt;
using QuikGraph;                                  // AdjacencyGraph, SEdge — the shared graph substrate (folder-local admission, central pin)
using QuikGraph.Algorithms;                       // AlgorithmExtensions: IsDirectedAcyclicGraph, SourceFirstTopologicalSort
using Rasm.Domain;                                // Context, Op
using Rasm.Element.Composition;                               // MaterialId — the SEAM material-identity owner, composed not re-declared
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault (band 2450, the one appearance fault) composed from bsdf#SHADING_FRAME
using Rasm.Numerics;                               // Direction, VectorFrame
using Rhino;                                      // UnitSystem (the GraphContext.Tolerant model-unit seed)
using Rhino.Geometry;                             // Point3d, Vector3d, Plane (host geometry at the shading-frame edge)
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;                  // Macbeth, EbnerFairchild, IsccNbs — validation/reference tables only
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Graph;

// --- [TYPES] -------------------------------------------------------------------------------
// PortId identifies a node port by int and carries NO comparer attribute — an int key uses the generated EqualityComparer<int>.Default
// (a string ComparerAccessors over an int key cannot bind), matching every sibling [SmartEnum<int>]/[ValueObject<int>].
[ValueObject<int>]
public readonly partial struct PortId {
    public static PortId Of(int value) => Create(value);   // the .Of factory the default-graph wiring and node authors compose ([ValueObject] generates Create, not Of)
}

// MathOp carries the MaterialX-aligned math roster: arithmetic (add/subtract/multiply/divide/scale/power/modulo), the unary transcendentals
// (sqrt/abs/sin/cos), min/max, the vector ops (dot/cross/normalize), the unit clamps, and the Schlick angular weight — each a delegate row,
// dispatch by data. TOTALITY CONVENTION, no fault channel: a zero divisor folds divide AND modulo to 0.0 (the MaterialX convention), a negative
// sqrt operand clamps to 0.0, a zero-length normalize returns the zero vector; modulo is FLOORED (the MaterialX/GLSL mod), never the CLR remainder.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MathOp {
    public static readonly MathOp Add = new("add", static (l, r) => new PortValue.Vector(l.AsVector + r.AsVector));
    public static readonly MathOp Subtract = new("subtract", static (l, r) => new PortValue.Vector(l.AsVector - r.AsVector));
    public static readonly MathOp Multiply = new("multiply", static (l, r) => new PortValue.Scalar(l.AsScalar * r.AsScalar));
    public static readonly MathOp Divide = new("divide", static (l, r) => new PortValue.Scalar(r.AsScalar is 0.0 ? 0.0 : l.AsScalar / r.AsScalar));
    public static readonly MathOp Modulo = new("modulo", static (l, r) => new PortValue.Scalar(r.AsScalar is 0.0 ? 0.0 : l.AsScalar - r.AsScalar * System.Math.Floor(l.AsScalar / r.AsScalar)));
    public static readonly MathOp Scale = new("scale", static (l, r) => new PortValue.Vector(l.AsVector * r.AsScalar));
    public static readonly MathOp Power = new("power", static (l, r) => new PortValue.Scalar(System.Math.Pow(l.AsScalar, r.AsScalar)));
    public static readonly MathOp Sqrt = new("sqrt", static (l, _) => new PortValue.Scalar(System.Math.Sqrt(System.Math.Max(0.0, l.AsScalar))));
    public static readonly MathOp Abs = new("abs", static (l, _) => new PortValue.Scalar(System.Math.Abs(l.AsScalar)));
    public static readonly MathOp Sin = new("sin", static (l, _) => new PortValue.Scalar(System.Math.Sin(l.AsScalar)));
    public static readonly MathOp Cos = new("cos", static (l, _) => new PortValue.Scalar(System.Math.Cos(l.AsScalar)));
    public static readonly MathOp Min = new("min", static (l, r) => new PortValue.Scalar(System.Math.Min(l.AsScalar, r.AsScalar)));
    public static readonly MathOp Max = new("max", static (l, r) => new PortValue.Scalar(System.Math.Max(l.AsScalar, r.AsScalar)));
    public static readonly MathOp DotProduct = new("dot", static (l, r) => new PortValue.Scalar(l.AsVector * r.AsVector));
    public static readonly MathOp CrossProduct = new("cross", static (l, r) => new PortValue.Vector(Vector3d.CrossProduct(l.AsVector, r.AsVector)));
    public static readonly MathOp Normalize = new("normalize", static (l, _) => new PortValue.Vector(l.AsVector is { Length: > 0.0 } v ? v / v.Length : l.AsVector));
    public static readonly MathOp Clamp01 = new("clamp01", static (l, _) => new PortValue.Scalar(System.Math.Clamp(l.AsScalar, 0.0, 1.0)));
    public static readonly MathOp OneMinus = new("one-minus", static (l, _) => new PortValue.Scalar(1.0 - l.AsScalar));
    public static readonly MathOp Fresnel = new("fresnel-weight", static (l, r) => new PortValue.Scalar(NodeEvaluator.SchlickWeight(System.Math.Clamp(l.AsVector * r.AsVector, 0.0, 1.0))));

    [UseDelegateFromConstructor]
    public partial PortValue Apply(PortValue lhs, PortValue rhs);
}

// MixOp carries the FULL W3C compositing vocabulary as DATA — one row per Unicolour BlendMode member, ONE Apply derivation reading its Mode
// column (the prior four delegates and the three-mode hand-rolled ChannelCompose are the deleted forms). Lerp IS the Normal row spelled as the
// HDR-safe scene-linear Mix (Blend clips to the [0,1] W3C reflectance domain; an over-unity intermediate keeps its >1 channels through the
// linear arm — sink-bound emission is normalized chromaticity, EmissionLuminance the energy); every named blend runs b.Blend(a, Mode) — a the
// backdrop, b the source — then the factor lerp as blend opacity. Mode is the per-row datum the interchange#MATERIALX_DOCUMENT category map
// reads to emit the real MaterialX node per blend.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MixOp {
    public static readonly MixOp Lerp       = new("lerp",       BlendMode.Normal);
    public static readonly MixOp Multiply   = new("multiply",   BlendMode.Multiply);
    public static readonly MixOp Screen     = new("screen",     BlendMode.Screen);
    public static readonly MixOp Overlay    = new("overlay",    BlendMode.Overlay);
    public static readonly MixOp Darken     = new("darken",     BlendMode.Darken);
    public static readonly MixOp Lighten    = new("lighten",    BlendMode.Lighten);
    public static readonly MixOp Dodge      = new("dodge",      BlendMode.ColourDodge);
    public static readonly MixOp Burn       = new("burn",       BlendMode.ColourBurn);
    public static readonly MixOp HardLight  = new("hard-light", BlendMode.HardLight);
    public static readonly MixOp SoftLight  = new("soft-light", BlendMode.SoftLight);
    public static readonly MixOp Difference = new("difference", BlendMode.Difference);
    public static readonly MixOp Exclusion  = new("exclusion",  BlendMode.Exclusion);
    public static readonly MixOp Hue        = new("hue",        BlendMode.Hue);
    public static readonly MixOp Saturation = new("saturation", BlendMode.Saturation);
    public static readonly MixOp Colour     = new("colour",     BlendMode.Colour);
    public static readonly MixOp Luminosity = new("luminosity", BlendMode.Luminosity);

    public BlendMode Mode { get; }

    public PortValue Apply(PortValue a, PortValue b, double t) =>
        new PortValue.Color(Mode is BlendMode.Normal
            ? a.AsColor.Mix(b.AsColor, ColourSpace.RgbLinear, t, premultiplyAlpha: false)
            : a.AsColor.Mix(b.AsColor.Blend(a.AsColor, Mode), ColourSpace.RgbLinear, t, premultiplyAlpha: false));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PortValue {
    private PortValue() { }

    public sealed record Scalar(double Value) : PortValue;
    public sealed record Color(Unicolour Linear) : PortValue;
    public sealed record Vector(Vector3d Value) : PortValue;
    public sealed record Frame(VectorFrame Value) : PortValue;

    public double AsScalar => Switch(scalar: static s => s.Value, color: static c => Luminance(c.Linear), vector: static v => v.Value.Length, frame: static _ => 1.0);

    // AP1 scene-linear luminance — the AP1-primary -> Y row consistent with the Acescg working space and the
    // bsdf#LOBE_FAMILY RgbSpectrum.Luminance weights; a Rec709 weight on AP1-linear channels mis-weights green.
    static double Luminance(Unicolour c) => 0.2722287 * c.RgbLinear.R + 0.6740818 * c.RgbLinear.G + 0.0536895 * c.RgbLinear.B;
    public Vector3d AsVector => Switch(scalar: static s => new Vector3d(s.Value, s.Value, s.Value), color: static c => RgbLinearVector(c.Linear), vector: static v => v.Value, frame: static f => f.Value.ZAxis);
    public Unicolour AsColor => Switch(scalar: static s => GreyLinear(s.Value), color: static c => c.Linear, vector: static v => VectorLinear(v.Value), frame: static f => VectorLinear(f.Value.ZAxis));

    static Vector3d RgbLinearVector(Unicolour c) => new(c.RgbLinear.R, c.RgbLinear.G, c.RgbLinear.B);
    static Unicolour GreyLinear(double g) => new(SceneLinear, ColourSpace.RgbLinear, g, g, g);
    static Unicolour VectorLinear(Vector3d v) => new(SceneLinear, ColourSpace.RgbLinear, v.X, v.Y, v.Z);
    // SceneLinear declares the ONE scene-linear working space (AP1 primaries) every Appearance page composes — surface#SPECTRAL_UPSAMPLE,
    // photometric#PHOTOMETRIC, texture#TEXTURE_UV, finish#FINISH, and interchange#MATERIAL_WIRE all read PortValue.SceneLinear.
    internal static readonly Configuration SceneLinear = new(RgbConfiguration.Acescg);
}

// GraphContext.Tolerant holds the one tolerant Context the Normal/ShadePoint arms construct a VectorFrame through. Context's kernel ctor is
// private and Context.Of(UnitSystem) returns Validation<Error,Context>; GraphContext resolves that admission ONCE into a ready value
// (millimetre model tolerance), so a per-sample frame build never re-validates and a near-degenerate perturbation re-seeds a perpendicular
// tangent through the Rasm.Numerics owner's own Fin rail rather than faulting mid-shade. IfFail guards a statically unreachable throw —
// Context.Of(UnitSystem.Millimeters) is a total success (it routes the kernel Millimeters() row) — the named boundary construction exemption:
// a static field must hold a non-optional Context, and an Option<Context> field forces every call site to handle a None that cannot occur.
public static class GraphContext {
    public static readonly Context Tolerant =
        Context.Of(UnitSystem.Millimeters).IfFail(_ => throw new InvalidOperationException("<graph-context-millimetres-unresolved>"));
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ShadePoint(Point3d Position, VectorFrame Frame, Vector3d ViewDirection, double U, double V) {
    public static Fin<ShadePoint> Of(Point3d position, Vector3d normal, Vector3d view, Option<Vector3d> tangentHint, double u, double v, Context context, Op key) =>
        from frame in VectorFrame.Of(origin: position, normal: normal, xHint: tangentHint, context: context, key: key)
        from outgoing in Direction.Of(value: view, context: context, key: key)
        select new ShadePoint(position, frame, outgoing.Value, u, v);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppearanceNode {
    private AppearanceNode(PortId id) => Id = id;
    public PortId Id { get; }

    public sealed record Input(PortId Id, Func<MaterialParameters, PortValue> Pull) : AppearanceNode(Id);
    // texture#TEXTURE_UV TextureUv.Port mints the host-free TOTAL (u,v)->PortValue closure the Texture arm carries — from a TextureSource +
    // UvSample anchor + SamplerState + Channel, folding a non-finite/undersized sample to the Channel neutral so the arm stays total, and the
    // wiring site composes `new Texture(id, TextureUv.Port(...))`. Texture knows ONLY the Func (texture COMPOSES this page's
    // PortValue/PortId/ShadePoint, so graph is the lower owner and carries no texture-namespace type), never a raw caller lambda
    // re-implementing a sampler — the Sample delegate is ALWAYS a TextureUv.Port closure.
    public sealed record Texture(PortId Id, Func<double, double, PortValue> Sample) : AppearanceNode(Id);
    public sealed record Math(PortId Id, MathOp Op, PortId Lhs, Option<PortId> Rhs) : AppearanceNode(Id);
    public sealed record Mix(PortId Id, MixOp Op, PortId A, PortId B, PortId Factor) : AppearanceNode(Id);
    public sealed record Normal(PortId Id, PortId Source, double Strength) : AppearanceNode(Id);
    public sealed record BsdfOutput(PortId Id, PortId BaseColor, PortId Metalness, PortId Roughness, PortId NormalFrame, PortId Emission) : AppearanceNode(Id);

    public Seq<PortId> Dependencies =>
        Switch(
            input: static _ => Seq<PortId>(),
            texture: static _ => Seq<PortId>(),
            math: static m => m.Rhs.Match(Some: r => Seq(m.Lhs, r), None: () => Seq(m.Lhs)),
            mix: static x => Seq(x.A, x.B, x.Factor),
            normal: static n => Seq(n.Source),
            bsdfOutput: static o => Seq(o.BaseColor, o.Metalness, o.Roughness, o.NormalFrame, o.Emission));

    // Whether the arm yields a port value a dependent can read. The sink alone does not — it consumes five ports and
    // writes nothing — so a node naming the sink as a dependency is a graph whose slot read has no cell to answer
    // from, and Compile refuses it against THIS column rather than against a hardcoded `is BsdfOutput` probe. A
    // second terminal kind therefore answers one column instead of growing a branch in the admission fold.
    public bool Produces =>
        Switch(
            input: static _ => true,
            texture: static _ => true,
            math: static _ => true,
            mix: static _ => true,
            normal: static _ => true,
            bsdfOutput: static _ => false);
}

public sealed record SurfaceShade(Unicolour BaseColorLinear, double Metalness, double Roughness, VectorFrame ShadingFrame, Unicolour EmissionLinear) {
    public bool InGamut => BaseColorLinear.IsInRgbGamut && EmissionLinear.IsInRgbGamut;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// PortSlot carries the per-node production: a value, or the SINK's typed absence — its Produced flag the runtime witness of the
// AppearanceNode.Produces column Compile already proved. An Option<PortValue> is unspellable on this rail: a Span-addressed scratch cell
// cannot hold one through the generated Switch's state parameter.
public readonly record struct PortSlot(bool Produced, PortValue Value) {
    // ZeroScalar declares BEFORE Sink: static field initializers run in declaration order, so the reversed order
    // seats a null Value inside the Sink sentinel — the type-init trap texture.md's own tables name.
    internal static readonly PortValue ZeroScalar = new PortValue.Scalar(0.0);
    public static readonly PortSlot Sink = new(false, ZeroScalar);
    public static PortSlot Of(PortValue value) => new(true, value);
}

public static class NodeEvaluator {
    // Apply is the ONE node algebra. Operands arrive PRE-RESOLVED in `Dependencies` order, so the evaluator reads no environment at all and the
    // rail alone owns the fill. Buffer type is PortValue[] rather than Span because the generated total Switch threads its state as a type
    // argument and a ref struct is not one; the rail rents it ONCE per fold, so no texel allocates. Arms stay static (the closure-free
    // per-sample law) and the Normal arm carries the only fault — every other production is total.
    public static Fin<PortSlot> Apply(AppearanceNode node, ShadePoint point, MaterialParameters parameters, PortValue[] operands, Op key) =>
        node.Switch(
            state:      (Point: point, Parameters: parameters, Operands: operands, Key: key),
            input:      static (s, i) => Fin.Succ(PortSlot.Of(i.Pull(s.Parameters))),
            texture:    static (s, t) => Fin.Succ(PortSlot.Of(t.Sample(s.Point.U, s.Point.V))),
            math:       static (s, m) => Fin.Succ(PortSlot.Of(m.Op.Apply(s.Operands[0], m.Rhs.IsSome ? s.Operands[1] : PortSlot.ZeroScalar))),
            mix:        static (s, x) => Fin.Succ(PortSlot.Of(x.Op.Apply(s.Operands[0], s.Operands[1], System.Math.Clamp(s.Operands[2].AsScalar, 0.0, 1.0)))),
            normal:     static (s, n) => Perturb(n, s.Point, s.Operands[0], s.Key).Map(PortSlot.Of),
            bsdfOutput: static (_, _) => Fin.Succ(PortSlot.Sink));

    internal static double SchlickWeight(double cosTheta) { double m = System.Math.Clamp(1.0 - cosTheta, 0.0, 1.0); double m2 = m * m; return m2 * m2 * m; }

    static Fin<PortValue> Perturb(AppearanceNode.Normal n, ShadePoint point, PortValue sample, Op key) =>
        from raw in Fin.Succ(sample.AsVector)
        let tangentSpace = new Vector3d((2.0 * raw.X - 1.0) * n.Strength, (2.0 * raw.Y - 1.0) * n.Strength, 2.0 * raw.Z - 1.0)
        let basis = point.Frame.Value
        let world = (basis.XAxis * tangentSpace.X) + (basis.YAxis * tangentSpace.Y) + (basis.ZAxis * tangentSpace.Z)
        from perturbed in VectorFrame.Of(origin: basis.Origin, normal: world, xHint: Some(basis.XAxis), context: GraphContext.Tolerant, key: key)
        select (PortValue)new PortValue.Frame(perturbed);
}

public sealed record MaterialGraph(Seq<AppearanceNode> Nodes, PortId Sink) {
    // Compile folds onto the shared QuikGraph substrate: AddVertexRange admits every node so an isolate still orders, AddVerticesAndEdge adds
    // one dependency->dependent SEdge per KNOWN dependency (allowParallelEdges: false deduplicates an authored Lhs==Rhs double edge),
    // IsDirectedAcyclicGraph pre-gates a cycle onto MaterialFault.Graph before SourceFirstTopologicalSort (Kahn) throws
    // NonAcyclicGraphException. A colliding node id faults FIRST — AddOrUpdate otherwise silently drops the earlier node's semantics from the
    // compiled order. Hand-rolled indegree/Queue/CollectionsMarshal kernels are the deleted form; the mutable AdjacencyGraph build is the
    // substrate's own catalogued construction seam, not a page kernel. Compile also resolves what the rail must never re-derive: the sink's
    // own BsdfOutput, the two rentals, and the node -> scratch-cell slot map at the frozen order.
    public Fin<CompiledGraph> Compile(Op key) {
        HashMap<PortId, AppearanceNode> byId = Nodes.Fold(HashMap<PortId, AppearanceNode>.Empty, static (m, n) => m.AddOrUpdate(n.Id, n));
        AdjacencyGraph<PortId, SEdge<PortId>> dag = new(allowParallelEdges: false);
        dag.AddVertexRange(Nodes.Map(static n => n.Id));
        Nodes.Iter(n => n.Dependencies.Filter(byId.ContainsKey).Iter(d => dag.AddVerticesAndEdge(new SEdge<PortId>(d, n.Id))));
        return from _ in guard(byId.Count == Nodes.Count, MaterialFault.Graph(key, "<duplicate-node-id>"))
               // ANSWERABILITY, one sweep over the two ways a slot read has no cell to answer from: a port no node
               // declares, and a port whose node PRODUCES nothing (the sink a dependent named). The rail addresses
               // its scratch by SLOT, so either would read the PREVIOUS texel's value in that cell rather than
               // railing, and a per-texel liveness re-check is exactly the cost the frozen order exists to delete.
               // Both reasons are distinct because they are distinct authoring mistakes, and the sweep reads the
               // Produces COLUMN, so a second non-producing node kind needs no edit here.
               from __ in Nodes.Bind(n => n.Dependencies.Map(d => (Consumer: n.Id, Port: d, Source: byId.Find(d))))
                   .Filter(static edge => edge.Source.Map(static node => node.Produces).IfNone(false) is false)
                   .HeadOrNone()
                   .Match(
                       Some: edge => Fin.Fail<Unit>(MaterialFault.Graph(key, edge.Source.IsSome
                           ? $"<non-producing-port:{edge.Consumer.Value}<-{edge.Port.Value}>"
                           : $"<dangling-port:{edge.Consumer.Value}<-{edge.Port.Value}>")),
                       None: static () => Fin.Succ(unit))
               // Compile resolves the sink to its OWN case ONCE, so no rail re-probes a cast per sample and the two sink
               // malformations keep the two reasons an author needs to tell them apart.
               from output in byId.Find(Sink).ToFin(MaterialFault.Graph(key, "<sink-missing>"))
                   .Bind(node => node is AppearanceNode.BsdfOutput sink
                       ? Fin.Succ(sink)
                       : Fin.Fail<AppearanceNode.BsdfOutput>(MaterialFault.Graph(key, "<sink-not-bsdf-output>")))
               from ___ in guard(dag.IsDirectedAcyclicGraph(), MaterialFault.Graph(key, "<cyclic-appearance-graph>"))
               let order = toSeq(dag.SourceFirstTopologicalSort()).Map(id => byId[id])
               // Slots freeze the node -> scratch-cell correspondence at the compiled order: the rail writes cell n
               // for order position n and reads an operand at Slots[dependency]. One sort, one index — and the
               // per-node OPERAND SLOT ARRAYS resolve here too, because Dependencies is a COMPUTED Seq whose
               // per-texel read would allocate millions of sequences across a 4k plane; the compiled int[] rows
               // are the allocation-free form the batched rail indexes.
               let slots = order.Map(static (node, index) => KeyValuePair.Create(node.Id, index)).ToFrozenDictionary()
               select new CompiledGraph(order, output, slots,
                   order.Map(node => node.Dependencies.Map(d => slots[d]).ToArray()).Strict());
    }

    // Evaluate is the ONE-SHOT convenience: Compile + Shade in one call for a single sample (a preview, a wire-egress shade). It RE-SORTS
    // every call, so the per-sample integrator hot path NEVER routes here — it Compiles ONCE to a CompiledGraph then calls compiled.Shade per
    // sample (the frozen-order re-entry), so the sort is paid once per material, not per ray.
    public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key) =>
        Compile(key).Bind(compiled => compiled.Shade(point, parameters, key));

    public static readonly MaterialGraph Default = BuildDefault();

    static MaterialGraph BuildDefault() {
        PortId baseColor = PortId.Of(1), metalness = PortId.Of(2), roughness = PortId.Of(3), normalSrc = PortId.Of(4), normal = PortId.Of(5), emission = PortId.Of(6), sink = PortId.Of(7);
        return new MaterialGraph(Seq<AppearanceNode>(
            new AppearanceNode.Input(baseColor, static p => new PortValue.Color(p.BaseColor)),
            new AppearanceNode.Input(metalness, static p => new PortValue.Scalar(p.Metalness)),
            new AppearanceNode.Input(roughness, static p => new PortValue.Scalar(p.Roughness)),
            new AppearanceNode.Input(normalSrc, static _ => new PortValue.Vector(new Vector3d(0.5, 0.5, 1.0))),
            new AppearanceNode.Input(emission, static p => new PortValue.Color(p.Emission)),
            new AppearanceNode.Normal(normal, normalSrc, Strength: 0.0),
            new AppearanceNode.BsdfOutput(sink, baseColor, metalness, roughness, normal, emission)), sink);
    }
}

public sealed record CompiledGraph(Seq<AppearanceNode> Order, AppearanceNode.BsdfOutput Output, FrozenDictionary<PortId, int> Slots, Seq<int[]> OperandSlots) {
    // ScratchWidth and OperandWidth are the two rentals a batched caller sizes against, RESOLVED AT COMPILE: one scratch cell per compiled
    // node, one operand cell per widest dependency list. Both derive from the frozen order, so a widened sink or a new node kind moves the
    // rental with no caller edit — a hardcoded five overflows the day BsdfOutput takes a sixth port — and neither re-folds the order on a
    // per-sample read the way a computed property does.
    public int ScratchWidth { get; } = Order.Count;
    public int OperandWidth { get; } = Order.Fold(1, static (widest, node) => System.Math.Max(widest, node.Dependencies.Count));

    // Shade re-enters the batched rail over a one-element window, so the integrator and the press fold ONE representation, ONE order, and ONE
    // gamut gate, and a semantic divergence between them is unrepresentable rather than guarded by review. Rentals are per-CALL and never
    // per-node — strictly less allocation than the persistent map the prior per-point rail rebuilt once per node per sample.
    public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Op key) {
        SurfaceShade[] shaded = new SurfaceShade[1];
        return ShadeSpan([point], parameters, new PortValue[ScratchWidth], shaded, key).Map(_ => shaded[0]);
    }

    // THE rail press#TEXTURE_PRESS drives per band: the port environment resolves into an index-addressed scratch
    // whose slot order IS the frozen compiled sort, so a plane costs one allocation-free pass per row — the
    // difference between minutes and days at four thousand square. Compile proved every dependency both KNOWN and
    // PRODUCING, so every slot read below is TOTAL and no per-texel liveness check exists. Scratch and shades are
    // CALLER-OWNED spans (the press rents them per band) and the one operand buffer is rented per CALL, never per
    // texel; a short rental refuses rather than truncating. The scratch needs no per-texel clear: the topological
    // order writes every producing cell before any dependent reads it. This walk is the page's named
    // [EXPRESSION_SPINE] span-kernel exemption — a Span may cross no lambda, so the fold has no expression form.
    public Fin<Unit> ShadeSpan(ReadOnlySpan<ShadePoint> points, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> shades, Op key) {
        if (scratch.Length < ScratchWidth || shades.Length < points.Length) {
            return Fin.Fail<Unit>(MaterialFault.Graph(key, $"<shade-span-rental-short:{scratch.Length}/{ScratchWidth}:{shades.Length}/{points.Length}>"));
        }
        PortValue[] operands = new PortValue[OperandWidth];
        for (int p = 0; p < points.Length; p++) {
            for (int n = 0; n < Order.Count; n++) {
                AppearanceNode node = Order[n];
                int[] sources = OperandSlots[n];
                for (int d = 0; d < sources.Length; d++) { operands[d] = scratch[sources[d]]; }
                Fin<PortSlot> produced = NodeEvaluator.Apply(node, points[p], parameters, operands, key);
                if (produced.IsFail) { return Indexed(produced, p, key).Map(static _ => unit); }
                PortSlot slot = produced.IfFail(PortSlot.Sink);
                if (slot.Produced) { scratch[n] = slot.Value; }
            }
            Fin<SurfaceShade> shade = Assemble(Output, points[p], scratch, Slots, key);
            if (shade.IsFail) { return Indexed(shade, p, key).Map(static _ => unit); }
            shades[p] = shade.IfFail(default(SurfaceShade)!);
        }
        return Fin.Succ(unit);
    }

    // Indexed names WHICH point failed — the arm's own reason with its window index appended — because a plane fails at ONE of sixteen million
    // points that all ran the same program. The re-wrap PRESERVES the fault's own case: a gamut refusal at texel
    // twelve million is still a gamut refusal, and flattening every span failure onto Graph would erase the one
    // discrimination the recovery vocabulary dispatches on.
    static Fin<T> Indexed<T>(Fin<T> failed, int index, Op key) =>
        failed.Match(
            Succ: static value => Fin.Succ(value),
            Fail: error => Fin.Fail<T>(error is MaterialFault.GamutCase gamut
                ? MaterialFault.Gamut(key, $"<shade-span-point:{index}:{gamut.Detail}>")
                : MaterialFault.Graph(key, $"<shade-span-point:{index}:{error.Message}>")));

    // Assemble reads the scratch DIRECTLY in ONE assembly. A Func<PortId, Fin<PortValue>> port reader is unspellable here — a lambda may not
    // capture a Span<T> — and minting a second environment shape to dodge that is exactly the divergence the single rail forecloses. Compile
    // proved each of the five sink ports known and producing but NOT their types, so the one non-projection read —
    // the normal frame — REFUSES a non-Frame production: a sink mis-wired onto a scalar node would otherwise shade
    // the whole plane unperturbed with nothing raised, the silent wrong answer the total projections cannot produce.
    static Fin<SurfaceShade> Assemble(AppearanceNode.BsdfOutput sink, ShadePoint point, ReadOnlySpan<PortValue> scratch, FrozenDictionary<PortId, int> slots, Op key) {
        if (scratch[slots[sink.NormalFrame]] is not PortValue.Frame frame) {
            return Fin.Fail<SurfaceShade>(MaterialFault.Graph(key, $"<sink-normal-not-frame:{sink.NormalFrame.Value}>"));
        }
        SurfaceShade shade = new(
            scratch[slots[sink.BaseColor]].AsColor,
            System.Math.Clamp(scratch[slots[sink.Metalness]].AsScalar, 0.0, 1.0),
            System.Math.Clamp(scratch[slots[sink.Roughness]].AsScalar, 0.0, 1.0),
            frame.Value,
            scratch[slots[sink.Emission]].AsColor);
        return shade.InGamut
            ? Fin.Succ(shade)
            : Fin.Fail<SurfaceShade>(MaterialFault.Gamut(key, $"<shade-out-of-gamut:{shade.BaseColorLinear.Hex}>"));
    }
}
```

## [03]-[MATERIAL_LIBRARY]

- Owner: `MaterialLibrary` over `MaterialParameters` keyed by the seam `MaterialId`; `SubsurfaceRadius` the validated mean-free-path carrier; `ThinFilm` the validated interference-film carrier (the OpenPBR `thin_film` group as one value object); `MaterialParameters` the canonical row — sixteen positional Disney-principled columns with the init-defaulted `Film`.
- Cases: the transcribed seed spans sixteen material families across thirty-four DATA ROWS — `metal.gold`/`metal.copper`/`metal.aluminum`/`metal.titanium`/`metal.iron`/`metal.steel`/`metal.silver`/`metal.chrome`/`metal.brass` (`metal.steel` the galvanized/structural-steel render row the `Component/component#COMPONENT_OWNER` `Component.AppearanceId` resolves — a warm-grey conductor distinct from the bluer `metal.iron`), `glass.crown`/`glass.flint`, `liquid.water`/`liquid.oil`, `gas.cavity`, `gem.diamond`, `stone.jade`/`stone.marble`, `plastic.abs`/`plastic.pvc`, `rubber.matte`, `polymer.adhesive` (the amber structural-epoxy render row a bonded `Component/joint#JOINT_FAMILY` `AdhesiveClass` joint's `AppearanceId` resolves — a smooth IOR-1.55 dielectric, distinct from the `metal.steel` base-metal `SubstanceId`), `skin.caucasian`/`skin.deep`, `fabric.velvet`/`fabric.silk`/`fabric.denim`, `paint.car-metallic`/`paint.clearcoat`, `ceramic.glazed`/`ceramic.porcelain`, `wax.beeswax`/`wax.candle`, `wood.oak`, `coat.gold-leaf` — each a row of `MaterialParameters` values the catalog grows by pure data addition (a new measured material is one row, not a new type), ZERO per-material types.
- Entry: `public static Fin<MaterialParameters> Lookup(MaterialId id, Op key)` — `Fin<T>` aborts on an unregistered id (`MaterialFault.Parameter`, key-correlated); an ad-hoc parameter vector admits through `MaterialParameters.Of` directly — the ONE row validation catalog rows and measured imports share, never a library-level forwarding alias; `Assign` is the profile-generalization seam mapping a masonry `Component` `MaterialId` to a catalog row; `Named` re-bases a Datasets named colour into a row's scene-linear `BaseColor`; `NearestChecker` (metric-parameterized over the full `DeltaE` selector), `HueConstant` (the Ebner-Fairchild constant-hue witness), `PointerAdmit`/`SpectralAdmit`/`MapToPointer`/`MapToSpectral` the reproducibility gates and recoveries, and `CvdPreview`/`Contrast`/`NearestIscc` the accessibility and designation projections.
- Packages: Wacton.Unicolour (base-color/emission construction; the `IsInPointerGamut`/`MapToPointerGamut` Pointer real-surface gamut accessors, the `IsInMacAdamLimits`/`MapToMacAdamLimits`/`IsImaginary` MacAdam spectral-limit accessors, the full 12-member `DeltaE` selector through `Difference` (the drift gate dispatches `Ciede2000`/`Cam16`/`Hyab` by the caller's policy row), the `Contrast` WCAG ratio, and the `Simulate(Cvd, double severity)` colour-vision-deficiency projection over the `Cvd` 8-member selector), Wacton.Unicolour.Datasets (composed for `Macbeth.All` ColorChecker validation, `Css`/`Xkcd`/`Nord` named-colour resolution, the `EbnerFairchild` `AllHue0..AllHue336` constant-hue loci driving `HueConstant` (`HungBerns` the admitted alternate loci family), and the `IsccNbs` 267 designation centroids driving `NearestIscc` — validation/reference only), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `System.Reflection` for the one definition-time Datasets field derivation)
- Growth: a new material is one `MaterialLibrary.Rows` entry — a `MaterialId` key and a `MaterialParameters` value; a new appearance parameter shared by ALL materials is one init-defaulted column on `MaterialParameters` (every existing row binds unchanged — the `Film` thin-film carrier is exactly this growth, landed as the OpenPBR `thin_film` group's row source); a new gamut domain (the display RGB gate, the Pointer real-surface gate, the MacAdam spectral-limit gate) is one accessor predicate by domain, never a collapse of the three into one gate; a new drift metric is a `DeltaE` member the caller's policy row passes, never a second checker; a new reference dataset is one reflection-derived table over the admitted Datasets assembly; a new accessibility-preview is one read-only projection over the package's own selector, never a stored row. There is NO per-material type, NO `GoldMaterial`/`GlassMaterial` class, NO `MetalFactory`/`PlasticFactory`, and NO per-family graph variant — the named defect is a second material surface; the repair is a row. Measured-spectral grounding targets the conductor and dielectric rows, framed at `[4]-[RESEARCH]`.
- Boundary: `MaterialParameters` is the single material concept — its columns are the closed Disney-principled parameter set (base color, metalness, roughness, specular tint, anisotropy, IOR, transmission, transmission roughness, sheen, sheen tint, clearcoat, clearcoat roughness, subsurface weight, subsurface radius, emission color, emission luminance) with the init-defaulted `Film` interference carrier — the OpenPBR `thin_film` group's ROW SOURCE the `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` reads (retiring its hardcoded thin-film zeros) and the `finish#FINISH` pearlescent/anodized rows seed, validated once at `ThinFilm.Create` so a negative thickness, an out-of-unit weight, or a sub-unity film IOR is unrepresentable and every existing positional construction binds unchanged; base color and emission are constructed once through Wacton.Unicolour scene-linear `Acescg` so the table carries spectrally-grounded colors, never raw byte triples; the metalness/IOR pairing is the conductor/dielectric discriminant the `bsdf#LOBE_FAMILY` lobe weights read (metalness=1 selects the conductor lobe with the base color as F0, metalness=0 the dielectric lobe with IOR-derived F0), so a "metal" and a "plastic" differ only by the metalness, IOR, and roughness columns — never by type; transmission>0 with IOR selects the dielectric-transmission lobe so glass, water, the sealed IGU cavity gas (`gas.cavity`, IOR 1.0 so its transmissive interface carries no Fresnel and the `Component/glazing#GLAZING_FAMILY` cavity layers shade as a clear non-refracting fill rather than the `liquid.water` proxy), and gems are rows differing only in IOR and transmission roughness; subsurface weight>0 routes the subsurface lobe so skin, wax, jade, and marble are rows differing only in subsurface radius (the per-channel mean-free-path carried as the validated three-band `SubsurfaceRadius` `[ComplexValueObject]`, a negative or non-finite millimetre band unrepresentable at `Create` so the inline negative-mfp guard `MaterialParameters.Of` once carried is gone); sheen>0 routes the sheen lobe so velvet, silk, and denim are rows differing only in sheen and roughness; clearcoat>0 layers the clearcoat lobe so car paint and glazed ceramic are rows differing only in clearcoat and clearcoat roughness; the profile consumer generalizes through `Assign`, which maps a masonry `Component` to a `MaterialId` row and NEVER mints a profile-specific material — `Component/masonry#MASONRY_FAMILY` is the cross-section owner the engine reads, never modifies, and an unmapped key falls back to the neutral `ceramic.porcelain` row rather than a fault so the profile consumer always shades; the Wacton.Unicolour.Datasets composition is validation/reference only — `NearestChecker` gates a candidate against the nearest `Macbeth.All` ColorChecker patch by `Unicolour.Difference` under the CALLER'S `DeltaE` metric (a drift beyond tolerance rails `MaterialFault.Gamut`; the metric is a policy value on the finish row, never a hidden default), `HueConstant` anchors a REFERENCE to its nearest `EbnerFairchild` constant-hue locus and requires the candidate within tolerance of that SAME locus (a tint that walked off-hue rails the reused `Gamut` case), `NearestIscc` projects the nearest of the 267 ISCC-NBS centroids as the standardized designation a specification prints, and `Named` re-bases a passed `Css`/`Xkcd`/`Nord` named `Unicolour` into a row's scene-linear `BaseColor` through `ConvertToConfiguration(SceneLinear)` FIRST (so the read channels are genuinely AP1-linear, not an sRGB-linear triple mislabelled as AP1 — the same colorimetric boundary the AP1 luminance honors); the ISCC/loci tables are ONE definition-time reflection derivation over the admitted assembly's own public static fields (`SYMBOLIC_REFERENCE`: the names and groups travel as the assembly's identifiers, never a hand-keyed 267-row transcription that drifts), the observer CMFs/illuminant SPDs/reflectance staying on the main Wacton.Unicolour owner the Datasets package does not carry; there are THREE gamut gates BY DOMAIN, never one collapse — `SurfaceShade.InGamut` reads the display `IsInRgbGamut` (the preview-reproducibility gate every row evaluates through), `PointerAdmit` reads the Pointer real-surface `IsInPointerGamut` (the physical-reproducibility gate a pigment-mixed reflectance must pass, the predicate `Appearance/finish#FINISH` imports for its admission), and `SpectralAdmit` reads the MacAdam optimal-colour `IsInMacAdamLimits` (the absolute spectral-locus bound a reflectance physically reachable at its luminance must satisfy, a reflectance beyond the spectral locus first caught by `IsImaginary` so an imaginary colour rails before the MacAdam test), each domain-gate railing the SAME `MaterialFault.Gamut` case with its own domain reason string (the case is reused across all three, never a second fault) and `MapToPointer`/`MapToSpectral` returning the nearest in-gamut Pointer/MacAdam `Unicolour` for the recoverable path — the gate ladder runs display RGB inside Pointer real-surface inside the MacAdam spectral-locus bound, the three never collapsed because each names a distinct physical reproducibility domain (the `HueConstant` witness sits BESIDE the ladder as a constancy check, never a fourth gamut); the `CvdPreview` accessibility projection reads `Unicolour.Simulate(Cvd, double severity)` over the `Cvd` 8-member deficiency selector and `Contrast` reads the WCAG ratio into a typed threshold receipt (`4.5`/`3.0`/`7.0` — AA text, AA large/UI, AAA text) — READ-ONLY projections the color-specification seam consumes, never stored library columns; every row evaluates to an in-gamut `SurfaceShade` through the same `MaterialGraph`.

```csharp signature
// (Continues the Rasm.Materials.Appearance.Graph compilation unit — the [02] prelude's usings, including
// `using Rasm.Element.Composition;` and `using Wacton.Unicolour.Datasets;`, are in scope; no duplicate import block.)

// --- [TYPES] -------------------------------------------------------------------------------
// MaterialId is a CROSS-PACKAGE identity owned by the SEAM (`Rasm.Element` `Composition/material#MATERIAL_COMPOSITION`
// declares the one `[ValueObject<string>]` material key with `ComparerAccessors.StringOrdinalIgnoreCase`). The
// `Material`/`MaterialComposition`/`MaterialLayer`/`MaterialConstituent` seam types and every Materials catalogue key
// on it, so this page composes the seam type rather than declaring a parallel `family.name` identity — the prior local
// `MaterialId` is RETIRED. The seam key comparer (ordinal-ignore-case) travels with the type; the shipped
// `ComparerAccessors.StringOrdinal` is the ordinal string policy the local `MathOp`/`MixOp` string keys compose, NOT the
// material identity (and NOT the int-keyed `PortId`, which carries no comparer).

[ComplexValueObject]
public readonly partial struct SubsurfaceRadius {
    public double R { get; }
    public double G { get; }
    public double B { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double r, ref double g, ref double b) {
        if (!double.IsFinite(r) || !double.IsFinite(g) || !double.IsFinite(b) || r < 0.0 || g < 0.0 || b < 0.0)
            validationError = new ValidationError($"<subsurface-radius-negative-mfp:{r:R},{g:R},{b:R}>");
    }

    public static readonly SubsurfaceRadius None = Create(0.0, 0.0, 0.0);
    public double Magnitude => Math.Sqrt(R * R + G * G + B * B);
}

// ThinFilm carries the OpenPBR thin_film group as ONE validated carrier — interference weight, film thickness (nm), film IOR — the row source
// surface#OPENPBR_SLAB OpenPbrSurface.Of reads into ThinFilmWeight/ThinFilmThickness/ThinFilmIor and finish#FINISH pearlescent/anodized rows
// seed. None (weight 0) is the no-film algebra zero every row defaults to; an out-of-unit weight, a negative/non-finite thickness, or a
// sub-unity IOR is unrepresentable at Create.
[ComplexValueObject]
public readonly partial struct ThinFilm {
    public double Weight { get; }
    public double ThicknessNm { get; }
    public double Ior { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double weight, ref double thicknessNm, ref double ior) {
        if (!double.IsFinite(weight) || weight is < 0.0 or > 1.0 || !double.IsFinite(thicknessNm) || thicknessNm < 0.0 || !double.IsFinite(ior) || ior < 1.0)
            validationError = new ValidationError($"<thin-film-out-of-range:{weight:R},{thicknessNm:R},{ior:R}>");
    }

    public static readonly ThinFilm None = Create(0.0, 0.0, 1.5);
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record MaterialParameters(
    Unicolour BaseColor,
    double Metalness,
    double Roughness,
    double SpecularTint,
    double Anisotropy,
    double Ior,
    double Transmission,
    double TransmissionRoughness,
    double Sheen,
    double SheenTint,
    double Clearcoat,
    double ClearcoatRoughness,
    double Subsurface,
    SubsurfaceRadius SubsurfaceRadius,
    Unicolour Emission,
    double EmissionLuminance) {

    // Film lands the interference column as init-defaulted enrichment (the acquisition#ACQUISITION Provenance mechanic), so all thirty-four
    // positional rows and every sibling construction bind unchanged; a film-bearing row spells
    // `new(...) { Film = ThinFilm.Create(w, nm, ior) }` and the carrier's Create owns the validation.
    public ThinFilm Film { get; init; } = ThinFilm.None;

    // ALL eleven unit-interval columns gate — a SheenTint of 7 or an Anisotropy of -2 is as unrepresentable as a
    // Metalness of 3; EmissionLuminance is the one open non-negative scale (cd/m², bounded by physics not by unit).
    public static Fin<MaterialParameters> Of(MaterialParameters candidate, Op key) =>
        from _ in guard(InUnit(candidate.Metalness) && InUnit(candidate.Roughness) && InUnit(candidate.SpecularTint) && InUnit(candidate.Anisotropy)
                && InUnit(candidate.Transmission) && InUnit(candidate.TransmissionRoughness) && InUnit(candidate.Sheen) && InUnit(candidate.SheenTint)
                && InUnit(candidate.Clearcoat) && InUnit(candidate.ClearcoatRoughness) && InUnit(candidate.Subsurface), MaterialFault.Parameter(key, "<weight-out-of-unit>"))
        from __ in guard(InIorRange(candidate.Ior, candidate.Metalness), MaterialFault.Parameter(key, $"<ior-out-of-range:{candidate.Ior}@metalness={candidate.Metalness}>"))
        from ___ in guard(double.IsFinite(candidate.EmissionLuminance) && candidate.EmissionLuminance >= 0.0, MaterialFault.Parameter(key, $"<emission-luminance-negative:{candidate.EmissionLuminance:R}>"))
        from ____ in guard(candidate.BaseColor.IsInRgbGamut && candidate.Emission.IsInRgbGamut, MaterialFault.Gamut(key, "<row-color-out-of-gamut>"))
        select candidate;

    static bool InUnit(double v) => double.IsFinite(v) && v is >= 0.0 and <= 1.0;
    static bool InIorRange(double ior, double metalness) => double.IsFinite(ior) && (metalness >= 1.0 ? ior is >= 0.1 and <= 3.0 : ior is >= 1.0 and <= 2.5);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialLibrary {
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
    static readonly Unicolour Black = Linear(0.0, 0.0, 0.0);
    static readonly SubsurfaceRadius NoScatter = SubsurfaceRadius.None;
    static SubsurfaceRadius Scatter(double r, double g, double b) => SubsurfaceRadius.Create(r, g, b);

    // Neutral names the fallback row an unmapped profile key shades with — a policy value, never an inline literal.
    static readonly MaterialId Neutral = MaterialId.Of("ceramic.porcelain");

    // Datasets reference tables land as ONE definition-time derivation over the admitted assembly's public static fields — 267 IsccNbs
    // designation centroids keyed by the assembly's OWN field identifiers, 15 EbnerFairchild AllHue* constant-hue loci — SYMBOLIC_REFERENCE,
    // never a hand-keyed transcription that drifts from the shipped data.
    static readonly FrozenDictionary<string, Unicolour> IsccCentroids = Fields<Unicolour>(typeof(IsccNbs), "");
    static readonly FrozenDictionary<string, IEnumerable<Unicolour>> HueLoci = Fields<IEnumerable<Unicolour>>(typeof(EbnerFairchild), "AllHue");

    static FrozenDictionary<string, T> Fields<T>(Type dataset, string prefix) =>
        dataset.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => typeof(T).IsAssignableFrom(f.FieldType) && f.Name.StartsWith(prefix, StringComparison.Ordinal))
            .ToFrozenDictionary(static f => f.Name, static f => (T)f.GetValue(null)!, StringComparer.Ordinal);

    public static readonly FrozenDictionary<MaterialId, MaterialParameters> Rows = new (MaterialId Id, MaterialParameters Row)[] {
        (MaterialId.Of("metal.gold"),      new(Linear(1.000, 0.766, 0.336), 1.0, 0.12, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.copper"),    new(Linear(0.955, 0.638, 0.538), 1.0, 0.18, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.aluminum"),  new(Linear(0.913, 0.922, 0.924), 1.0, 0.08, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.silver"),    new(Linear(0.972, 0.960, 0.915), 1.0, 0.05, 0.0,  0.0,  0.155, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.iron"),      new(Linear(0.560, 0.570, 0.580), 1.0, 0.35, 0.0,  0.0,  2.950, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.steel"),     new(Linear(0.560, 0.570, 0.577), 1.0, 0.40, 0.0,  0.0,  2.800, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.titanium"),  new(Linear(0.542, 0.497, 0.449), 1.0, 0.28, 0.0,  0.0,  2.740, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.chrome"),    new(Linear(0.550, 0.556, 0.554), 1.0, 0.02, 0.0,  0.0,  3.000, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.brass"),     new(Linear(0.887, 0.789, 0.434), 1.0, 0.22, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("glass.crown"),     new(Linear(0.960, 0.970, 0.980), 0.0, 0.02, 0.0,  0.0,  1.520, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("glass.flint"),     new(Linear(0.950, 0.945, 0.960), 0.0, 0.03, 0.0,  0.0,  1.620, 1.0, 0.05, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("liquid.water"),    new(Linear(0.980, 0.990, 0.995), 0.0, 0.0,  0.0,  0.0,  1.333, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("liquid.oil"),      new(Linear(0.920, 0.880, 0.620), 0.0, 0.04, 0.0,  0.0,  1.470, 0.9, 0.08, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("gas.cavity"),      new(Linear(0.998, 0.998, 0.998), 0.0, 0.0,  0.0,  0.0,  1.000, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("gem.diamond"),     new(Linear(0.990, 0.990, 0.995), 0.0, 0.0,  0.0,  0.0,  2.417, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("stone.jade"),      new(Linear(0.270, 0.560, 0.380), 0.0, 0.35, 0.0,  0.0,  1.660, 0.4, 0.30, 0.0, 0.0, 0.0, 0.0, 0.6, Scatter(4.0, 8.0, 5.0), Black, 0.0)),
        (MaterialId.Of("plastic.abs"),     new(Linear(0.800, 0.050, 0.050), 0.0, 0.30, 0.5,  0.0,  1.460, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("plastic.pvc"),     new(Linear(0.180, 0.380, 0.760), 0.0, 0.45, 0.4,  0.0,  1.520, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("rubber.matte"),    new(Linear(0.040, 0.040, 0.040), 0.0, 0.85, 0.0,  0.0,  1.519, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("polymer.adhesive"), new(Linear(0.250, 0.190, 0.110), 0.0, 0.35, 0.0,  0.0,  1.550, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("skin.caucasian"), new(Linear(0.640, 0.430, 0.370), 0.0, 0.45, 0.0,  0.0,  1.400, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, Scatter(3.67, 1.37, 0.68), Black, 0.0)),
        (MaterialId.Of("skin.deep"),       new(Linear(0.330, 0.180, 0.130), 0.0, 0.50, 0.0,  0.0,  1.400, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, Scatter(2.10, 0.80, 0.40), Black, 0.0)),
        (MaterialId.Of("fabric.velvet"),   new(Linear(0.380, 0.030, 0.080), 0.0, 0.90, 0.0,  0.0,  1.460, 0.0, 0.0, 1.0, 0.8, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("fabric.silk"),     new(Linear(0.700, 0.620, 0.480), 0.0, 0.35, 0.2,  0.6,  1.460, 0.0, 0.0, 0.6, 0.3, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("fabric.denim"),    new(Linear(0.150, 0.230, 0.380), 0.0, 0.80, 0.0,  0.0,  1.460, 0.0, 0.0, 0.4, 0.5, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("paint.car-metallic"), new(Linear(0.090, 0.020, 0.220), 0.85, 0.30, 0.0, 0.0, 1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("paint.clearcoat"), new(Linear(0.700, 0.700, 0.700), 0.0, 0.40, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.03, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("ceramic.glazed"),  new(Linear(0.880, 0.850, 0.780), 0.0, 0.10, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.9, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("ceramic.porcelain"), new(Linear(0.930, 0.920, 0.900), 0.0, 0.20, 0.0, 0.0, 1.504, 0.0, 0.0, 0.0, 0.0, 0.3, 0.10, 0.4, Scatter(5.0, 5.0, 5.0), Black, 0.0)),
        (MaterialId.Of("wax.beeswax"),     new(Linear(0.870, 0.700, 0.330), 0.0, 0.55, 0.0,  0.0,  1.443, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.8, Scatter(6.0, 4.0, 1.5), Black, 0.0)),
        (MaterialId.Of("wax.candle"),      new(Linear(0.940, 0.920, 0.850), 0.0, 0.60, 0.0,  0.0,  1.430, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.9, Scatter(8.0, 6.0, 4.0), Black, 0.0)),
        (MaterialId.Of("stone.marble"),    new(Linear(0.870, 0.860, 0.840), 0.0, 0.30, 0.0,  0.0,  1.486, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.5, Scatter(2.19, 2.62, 3.00), Black, 0.0)),
        (MaterialId.Of("wood.oak"),        new(Linear(0.430, 0.270, 0.140), 0.0, 0.55, 0.3,  0.4,  1.530, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("coat.gold-leaf"),  new(Linear(1.000, 0.766, 0.336), 1.0, 0.06, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 1.0, 0.02, 0.0, NoScatter, Black, 0.0)),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);   // seam MaterialId's generated equality (ordinal-ignore-case) keys the row table

    public static Fin<MaterialParameters> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out MaterialParameters? row) ? Fin.Succ(row!) : Fin.Fail<MaterialParameters>(MaterialFault.Parameter(key, $"<unregistered-material:{id.Value}>"));

    public static Fin<MaterialParameters> Assign(MaterialId appearanceId, Op key) =>
        Rows.TryGetValue(appearanceId, out MaterialParameters? row) ? Fin.Succ(row!) : Lookup(Neutral, key);

    // Re-base a passed named Unicolour (a Wacton.Unicolour.Datasets Css/Xkcd/Nord static, measured in ITS OWN working
    // space) into the row's scene-linear Acescg BaseColor: ConvertToConfiguration rebases the colour onto the AP1
    // working space FIRST, so the read .RgbLinear channels are genuinely AP1-linear — stuffing a Css sRGB-linear triple
    // straight into an Acescg Unicolour would mislabel sRGB primaries as AP1 (the same colorimetric defect the Rec709
    // luminance carried), so the working-space convert is the correctness boundary, not an optional rebase.
    public static MaterialParameters Named(Unicolour reference, MaterialParameters template) {
        ColourTriplet ap1 = reference.ConvertToConfiguration(PortValue.SceneLinear).RgbLinear.Triplet;
        return template with { BaseColor = new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, ap1.First, ap1.Second, ap1.Third) };
    }

    // NearestChecker witnesses drift under the CALLER'S DeltaE policy — Ciede2000 for pigment paints, Cam16 for effect finishes, Hyab for
    // large-difference composites — the metric a finish#FINISH FinishHandling row value, never a hidden default.
    public static Fin<(Unicolour Patch, double DeltaE)> NearestChecker(Unicolour candidate, double tolerance, DeltaE metric, Op key) =>
        toSeq(Macbeth.All)
            .Map(patch => (Patch: patch, DeltaE: candidate.Difference(patch, metric)))
            .Fold(Option<(Unicolour Patch, double DeltaE)>.None, static (best, row) => best.Filter(b => b.DeltaE <= row.DeltaE).IsSome ? best : Some(row))
            .ToFin(MaterialFault.Parameter(key, "<colorchecker-set-empty>"))
            .Bind(nearest => nearest.DeltaE <= tolerance
                ? Fin.Succ(nearest)
                : MaterialFault.Gamut(key, $"<colorchecker-drift:deltaE={nearest.DeltaE:R}>"));

    // HueConstant witnesses Ebner-Fairchild hue constancy: anchor the REFERENCE to its nearest constant-hue locus (min Ciede2000 over each
    // AllHue* group), then require the CANDIDATE within tolerance of that SAME locus — a tinted composite that walked off the reference hue
    // rails the reused Gamut case; finish#FINISH gates every composite through this.
    public static Fin<Unicolour> HueConstant(Unicolour candidate, Unicolour reference, double tolerance, Op key) =>
        toSeq(HueLoci)
            .Map(locus => (Locus: locus.Key, Anchor: LocusDelta(reference, locus.Value), Drift: LocusDelta(candidate, locus.Value)))
            .Fold(Option<(string Locus, double Anchor, double Drift)>.None, static (best, row) => best.Filter(b => b.Anchor <= row.Anchor).IsSome ? best : Some(row))
            .ToFin(MaterialFault.Parameter(key, "<constant-hue-loci-empty>"))
            .Bind(nearest => nearest.Drift <= tolerance
                ? Fin.Succ(candidate)
                : MaterialFault.Gamut(key, $"<hue-shifted-tint:{nearest.Locus}:deltaE={nearest.Drift:R}>"));

    static double LocusDelta(Unicolour colour, IEnumerable<Unicolour> locus) =>
        locus.Min(member => colour.Difference(member, DeltaE.Ciede2000));

    public static Fin<Unicolour> PointerAdmit(Unicolour reflectance, Op key) =>
        reflectance.IsInPointerGamut
            ? Fin.Succ(reflectance)
            : MaterialFault.Gamut(key, $"<pointer-unreproducible-reflectance:{reflectance.Hex}>");

    public static Unicolour MapToPointer(Unicolour reflectance) => reflectance.MapToPointerGamut();

    public static Fin<Unicolour> SpectralAdmit(Unicolour reflectance, Op key) =>
        reflectance.IsImaginary
            ? MaterialFault.Gamut(key, $"<imaginary-reflectance:{reflectance.Hex}>")
            : reflectance.IsInMacAdamLimits
                ? Fin.Succ(reflectance)
                : MaterialFault.Gamut(key, $"<macadam-unreproducible-reflectance:{reflectance.Hex}>");

    public static Unicolour MapToSpectral(Unicolour reflectance) => reflectance.MapToMacAdamLimits();

    public static Unicolour CvdPreview(Unicolour color, Cvd deficiency, double severity) => color.Simulate(deficiency, System.Math.Clamp(severity, 0.0, 1.0));

    // WCAG contrast as a typed threshold receipt beside CvdPreview — the ratio plus the 4.5/3.0/7.0 verdicts
    // (AA text · AA large/UI · AAA text) the color-specification seam reads; never a bare package-call rename.
    public static (double Ratio, bool AaText, bool AaLarge, bool AaaText) Contrast(Unicolour foreground, Unicolour background) =>
        foreground.Contrast(background) switch { var ratio => (ratio, ratio >= 4.5, ratio >= 3.0, ratio >= 7.0) };

    // NearestIscc projects the ISCC-NBS designation: the nearest of the 267 centroids by Ciede2000 — the standardized colour NAME a finish
    // schedule or specification prints, read-only beside CvdPreview/Contrast, never a stored row column.
    public static (string Name, Unicolour Centroid, double DeltaE) NearestIscc(Unicolour candidate) =>
        toSeq(IsccCentroids)
            .Map(row => (Name: row.Key, Centroid: row.Value, DeltaE: candidate.Difference(row.Value, DeltaE.Ciede2000)))
            .Fold((Name: "<unnamed>", Centroid: candidate, DeltaE: double.MaxValue), static (best, row) => row.DeltaE < best.DeltaE ? row : best);
}
```

## [04]-[RESEARCH]

- [MEASURED_SPECTRAL_LIBRARY]-[BLOCKED]: Which managed reader admits the measured isotropic 195-wavelength spectral BRDF (EPFL RGL goniophotometer, brdf-loader `.bsdf` format) per band through `surface#SPECTRAL_UPSAMPLE` `ToSpd` onto the conductor and dielectric rows?; verify a vendored managed `.bsdf` reader at `acquisition` `[EPFL_RGL_BRDF_LOADER]`.
