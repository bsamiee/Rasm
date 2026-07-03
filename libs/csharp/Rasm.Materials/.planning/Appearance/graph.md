# [MATERIALS_GRAPH]

THE NODE-GRAPH APPEARANCE ENGINE and THE POLYMORPHIC MATERIAL LIBRARY. One `AppearanceNode` `[Union]` closes the node-kind family — `Input`, `Texture`, `Math`, `Mix`, `Normal`, `BsdfOutput` — over the typed `PortValue` channel set; one `MaterialGraph.Compile` fold orders the node DAG ONCE on the shared QuikGraph substrate (`IsDirectedAcyclicGraph` gate, `SourceFirstTopologicalSort` order) and freezes it, and the per-sample `CompiledGraph.Shade` folds each node's arm into a `PortEnvironment` terminating at the single `BsdfOutput` node that assembles a `SurfaceShade`. One `MaterialParameters` record is the canonical Disney-principled parameter vector — sixteen positional columns plus the init-defaulted `Film` thin-film carrier — every measured material parameterizes, and one `MaterialLibrary` `FrozenDictionary<MaterialId, MaterialParameters>` is the catalog as DATA ROWS — metal, glass, liquid, gas, gem, stone, plastic, rubber, polymer, skin, fabric, paint, ceramic, wax, wood, coat — so a new material is `MaterialLibrary.Rows[MaterialId.Of("metal.titanium")] = new MaterialParameters(...)`, a row of values, NEVER a `TitaniumMaterial` type. The page OWNS the `PortId`/`MathOp`/`MixOp`/`PortValue`/`GraphContext` graph vocabulary (the `MixOp` table one `BlendMode` row per W3C compositing member, the blend behavior a DATA column, never sixteen delegates), the `ShadePoint`/`AppearanceNode`/`SurfaceShade`/`PortEnvironment`/`CompiledGraph`/`MaterialGraph` evaluation surfaces, the `SubsurfaceRadius` mean-free-path and `ThinFilm` interference carriers, the `MaterialParameters` row, and the `MaterialLibrary` catalog/admission/reference folds; it COMPOSES the SEAM `Rasm.Element` `MaterialId` identity (never re-minting a `family.name` key), the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail (never a second fault), the Rasm.Vectors `Direction`/`VectorFrame`/`Context` shading frame (never re-minting a vector or a tolerance), the `texture#TEXTURE_UV` `TextureUv.Port` closure for the `Texture` arm (never re-implementing sampling), QuikGraph as the one graph-algorithm substrate the whole stack folds transient graphs onto (never a hand-rolled Kahn walk), and Wacton.Unicolour directly as the scene-linear/spectral/compositing color owner under the one `Acescg` working space (never re-minting a `ColourSpace`). The terminal `SurfaceShade` is the resolved parameter snapshot the `surface#OPENPBR_SLAB` `SlabStack.ToLayered` lowers to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` the integrator shades — the graph resolves the parameters, the lobe math lives on the `bsdf`/`surface` pages and is never re-derived here. The masonry-assignment consumer generalizes through the `MaterialId` row: a masonry `Component` maps to a `MaterialId`, never to a component-specific material type.

## [01]-[INDEX]

- [01]-[MATERIAL_GRAPH]: the `PortId`/`MathOp`/`MixOp` graph vocabulary (`MixOp` the 16-row `BlendMode` table), the `PortValue` channel set, the `GraphContext` tolerant-`Context` carrier, the `AppearanceNode` node union, the `ShadePoint`/`PortEnvironment`/`CompiledGraph`/`MaterialGraph` QuikGraph-ordered evaluation fold, and the `SurfaceShade` sink.
- [02]-[MATERIAL_LIBRARY]: the `SubsurfaceRadius` mean-free-path and `ThinFilm` interference carriers, the `MaterialParameters` canonical row, the seam `MaterialId` key, the catalog table, the profile-assignment generalization, the `NearestChecker`/`HueConstant`/`Named` Datasets validation seam over the reflection-derived reference tables, the three physical-reproducibility gates by domain — display RGB (`SurfaceShade.InGamut`), Pointer real-surface (`PointerAdmit`/`MapToPointer`), and MacAdam spectral-limit (`SpectralAdmit`/`MapToSpectral`) — plus the `CvdPreview`/`Contrast`/`NearestIscc` accessibility and designation projections.

## [02]-[MATERIAL_GRAPH]

- Owner: `MaterialGraph`/`CompiledGraph` over `AppearanceNode`; the `PortId`/`MathOp`/`MixOp`/`PortValue` graph vocabulary; the `GraphContext` tolerant-`Context` carrier; the `ShadePoint`/`SurfaceShade`/`PortEnvironment` evaluation models.
- Cases: `Input` (constant/parameter source) · `Texture` (UV-sampled source — the `texture#TEXTURE_UV` `TextureUv.Port` closure) · `Math` (closed scalar/vector op over upstream ports) · `Mix` (parameterized `BlendMode` composite of two ports) · `Normal` (tangent-space perturbation of the shading frame) · `BsdfOutput` (the single sink assembling the closed lobe set into a `SurfaceShade`)
- Entry: `public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key)` is the ONE-SHOT convenience (Compile + Shade for a single sample); the PER-SAMPLE path `Compile` resolves the topological order ONCE into a frozen `CompiledGraph` and `Shade` re-enters over that compiled order per sample, so the integrator hot loop pays the sort once per material, never per ray, and `Evaluate`'s per-call re-sort never reaches the inner loop. `Fin<T>` aborts on a cyclic DAG (`MaterialFault.Graph`, key-correlated), a duplicate node id, an unbound port reference, a non-`BsdfOutput` sink, or an out-of-gamut assembled shade (a port-TYPE mismatch cannot fault — the `PortValue.AsScalar`/`AsColor`/`AsVector` projections are total by construction); `MaterialGraph.Default` is the canonical Disney-principled wiring every library row drives through.
- Packages: QuikGraph (composed — `AdjacencyGraph<PortId, SEdge<PortId>>` with `allowParallelEdges: false`, `AddVertexRange` admitting isolates, `AddVerticesAndEdge` per dependency edge, `AlgorithmExtensions.IsDirectedAcyclicGraph` the cheap cycle pre-gate, `AlgorithmExtensions.SourceFirstTopologicalSort` the Kahn order — the one graph-algorithm substrate `Rasm.Element`/`Rasm.Persistence`/`Rasm.Bim` already fold onto, admitted folder-locally against the central pin), Rasm (project — `Direction`/`VectorFrame`/`Context`/`Op`, `Rhino.Geometry.Point3d`/`Vector3d`/`Plane` at the host edge), Rasm.Element (the SEAM `MaterialId`, composed not re-declared), Rasm.Materials.Appearance.Bsdf (the `MaterialFault` band-2450 rail composed from `bsdf#SHADING_FRAME`), Wacton.Unicolour (color/spectral/compositing compose — `Mix`, `Blend(backdrop, BlendMode)`, the 16-member `BlendMode` vocabulary), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`). The `Texture` arm's closure is the `texture#TEXTURE_UV` `TextureUv.Port` product — `texture` COMPOSES this page's `PortValue`/`PortId`/`ShadePoint`, so graph stays the LOWER owner and the `Texture` case carries only a host-free `Func<double,double,PortValue>`, never a `texture`-namespace type (no cyclic namespace dependency).
- Growth: a new appearance operation is one `MathOp` row (the operation behavior rides the SmartEnum row's `[UseDelegateFromConstructor]` delegate — the roster spans arithmetic incl. floored `modulo`, the unary transcendentals `sqrt`/`abs`/`sin`/`cos`, min/max, the vector `dot`/`cross`/`normalize`, unit clamps, and the Schlick weight, each keyed to its MaterialX standard math category) or one `MixOp` row naming its `BlendMode` member (the blend behavior IS the `Mode` data column the ONE `Apply` derivation reads — never a new arm, never a hand-rolled channel composite); a genuinely new node KIND with no parameterization of the six is one `AppearanceNode` case; a new port channel is one `PortValue` case carrying its CLR carrier; a new lobe assembled at the sink is one `BsdfLobe` `[Union]` case on the `bsdf` page — never a per-effect graph variant and never a sibling node type. The `AppearanceNode` union and `PortValue` set are the MaterialX 1.39 node-graph alignment target the `interchange#MATERIALX_DOCUMENT` `NodeCategory`/`MtlxPort` map projects onto, framed at `[4]-[RESEARCH]`.
- Boundary: the node DAG is the only appearance-program shape — a per-material hand-written shade function is the deleted form; `PortValue` is the only inter-node channel and carries scalar/`Unicolour`/`Vector3d`/`VectorFrame` polarities so a node arm reads typed ports and never `object`; the `Color`→`Scalar` projection is the AP1 scene-linear luminance dot `0.2722287 R + 0.6740818 G + 0.0536895 B` (the AP1-primary luminance row consistent with the declared `Acescg` working space and the `bsdf#LOBE_FAMILY` `RgbSpectrum.Luminance` weights — a Rec709 weight on AP1-linear channels is the colorimetric defect, biasing a green-heavy mask), never a red-channel read, so a mask pulled from a color is photometrically weighted and cannot silently bias to red; the `Texture` arm carries the TOTAL `Func<double,double,PortValue>` closure the `texture#TEXTURE_UV` `TextureUv.Port(TextureSource, UvSample, SamplerState, Channel, Op)` mints — the node holds the delegate, the sampling fold lives on the `texture` page, and the arm never re-implements a sampler nor admits a raw caller-supplied lambda that bypasses the `Channel`-neutralized fault rail; the `Normal` arm perturbs the composed Rasm.Vectors `VectorFrame` (tangent·bitangent·normal) and never re-mints a basis; the `BsdfOutput` arm assembles the `SurfaceShade` parameter snapshot (resolved base color, metalness, roughness, perturbed shading frame, emission) the renderer reads — the lobe WEIGHTING is the downstream `surface#OPENPBR_SLAB` `SlabStack.ToLayered` lowering of the `MaterialParameters` row to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` the integrator shades, the graph sink being the resolved parameter shade and the lobe math living wholly on the `bsdf`/`surface` pages, never re-derived here, color resolved through the directly-consumed Wacton.Unicolour `RgbConfiguration.Acescg` scene-linear owner; the `BsdfOutput` sink resolves through `Assemble` behind a pattern-matched sink probe (a non-`BsdfOutput` sink rails `MaterialFault.Graph`, never an unchecked cast), never a port write, so the environment carries no dead entry under the sink id and a downstream node cannot read a phantom `Scalar(1.0)`; the `Math` arm folds over its `MathOp` SmartEnum by delegate row so a new operation is a row, never a new arm, and the `MathOp.Fresnel` row supplies only the Schlick angular weight `(1−cosθ)⁵` for a `Mix` lobe blend — the full Fresnel term lives on `bsdf#MICROFACET_KERNEL`, never re-derived here; the `Mix` arm dispatches `b.AsColor.Blend(a.AsColor, Mode)` — the W3C separable/non-separable compositing algebra Unicolour owns, `a` the backdrop, `b` the source, the factor the blend opacity lerped in scene-linear `RgbLinear` — so all sixteen W3C modes are one data column and the prior three-mode hand-rolled `ChannelCompose` channel math is the deleted form; the `Lerp` row IS `BlendMode.Normal` spelled as the HDR-safe scene-linear `Unicolour.Mix` (the blend algebra clips to the `[0,1]` W3C reflectance domain; an over-unity INTERMEDIATE — a scaled mask, a `Math` product — keeps its `>1` channels through the linear arm, while a sink-bound emission port is NORMALIZED chromaticity by construction, `MaterialParameters.EmissionLuminance` carrying the energy, so the `Assemble` `InGamut` gate holds); `Compile` folds the DAG onto the QuikGraph substrate ONCE — `AddVertexRange` admits every node so an isolate still orders, `AddVerticesAndEdge` adds one dependency→dependent `SEdge<PortId>` per KNOWN dependency (a dangling port reference stays out of the graph and faults at `Shade`'s `env.Read`, `allowParallelEdges: false` deduplicating an authored `Lhs == Rhs` double edge), `IsDirectedAcyclicGraph` pre-gates a cycle onto `MaterialFault.Graph` before `SourceFirstTopologicalSort` would throw `NonAcyclicGraphException`, and `Shade` re-enters over the frozen `CompiledGraph` order so per-sample evaluation never re-sorts — the prior hand-rolled indegree/`Queue`/`CollectionsMarshal` Kahn kernel and its `[EXPRESSION_SPINE]` exemption are DELETED, the mutable `AdjacencyGraph` build being the substrate's own catalogued construction seam, and the page now carries no imperative kernel; the `PortEnvironment` fold projects a new immutable `HashMap` per resolved node and never mutates a dictionary; `GraphContext.Tolerant` is the one tolerant `Context` the `Normal`/`ShadePoint` arms construct the `VectorFrame` through (a millimetre-scale model `Context` whose `Fin` admission the page resolves once, so a near-degenerate perturbation re-seeds a perpendicular tangent through the Vectors owner rather than faulting mid-shade); `MaterialGraph.Default` carries the geometric frame unperturbed through one `Normal` node at `Strength 0` whose identity tangent-space sample `(0.5,0.5,1.0)` decodes to `+Z`, so a library row is parameters evaluated through this one standard graph, never a per-row graph type; a cycle, a dangling port, a duplicate node id, or a non-`BsdfOutput` sink rails `Fin.Fail` and never propagates a NaN shade outward.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Reflection;                          // the definition-time Datasets field derivation (IsccNbs names, EbnerFairchild loci)
using LanguageExt;
using QuikGraph;                                  // AdjacencyGraph, SEdge — the shared graph substrate (folder-local admission, central pin)
using QuikGraph.Algorithms;                       // AlgorithmExtensions: IsDirectedAcyclicGraph, SourceFirstTopologicalSort
using Rasm.Domain;                                // Context, Op
using Rasm.Element;                               // MaterialId — the SEAM material-identity owner, composed not re-declared
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault (band 2450, the one appearance fault) composed from bsdf#SHADING_FRAME
using Rasm.Vectors;                               // Direction, VectorFrame
using Rhino;                                      // UnitSystem (the GraphContext.Tolerant model-unit seed)
using Rhino.Geometry;                             // Point3d, Vector3d, Plane (host geometry at the shading-frame edge)
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;                  // Macbeth, EbnerFairchild, IsccNbs — validation/reference tables only
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Graph;

// --- [TYPES] -------------------------------------------------------------------------------
// An int-keyed node port identity: NO comparer attribute — an int key uses the generated EqualityComparer<int>.Default
// (a string ComparerAccessors over an int key cannot bind), matching every sibling [SmartEnum<int>]/[ValueObject<int>].
[ValueObject<int>]
public readonly partial struct PortId {
    public static PortId Of(int value) => Create(value);   // the .Of factory the default-graph wiring and node authors compose ([ValueObject] generates Create, not Of)
}

// The MaterialX-aligned math roster: arithmetic (add/subtract/multiply/divide/scale/power/modulo), the unary
// transcendentals (sqrt/abs/sin/cos), min/max, the vector ops (dot/cross/normalize), the unit clamps, and the Schlick
// angular weight — each a delegate row, dispatch by data. TOTALITY CONVENTION, no fault channel: a zero divisor folds
// divide AND modulo to 0.0 (the MaterialX convention), a negative sqrt operand clamps to 0.0, a zero-length normalize
// returns the zero vector; modulo is FLOORED (the MaterialX/GLSL mod), never the CLR remainder.
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

// The FULL W3C compositing vocabulary as DATA — one row per Unicolour BlendMode member, ONE Apply derivation reading
// the Mode column (the prior four delegates plus the three-mode hand-rolled ChannelCompose are the deleted forms).
// Lerp IS the Normal row spelled as the HDR-safe scene-linear Mix (Blend clips to the [0,1] W3C reflectance domain;
// an over-unity intermediate keeps its >1 channels through the linear arm — sink-bound emission is normalized
// chromaticity, EmissionLuminance the energy); every named blend runs b.Blend(a, Mode) — a the backdrop, b the
// source — then the factor lerp as blend opacity. Mode is the per-row datum the interchange#MATERIALX_DOCUMENT
// category map reads to emit the real MaterialX node per blend.
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
    // The ONE scene-linear working space (AP1 primaries) every Appearance page composes — surface#SPECTRAL_UPSAMPLE,
    // photometric#PHOTOMETRIC, texture#TEXTURE_UV, finish#FINISH, and interchange#MATERIAL_WIRE all read PortValue.SceneLinear.
    internal static readonly Configuration SceneLinear = new(RgbConfiguration.Acescg);
}

// The one tolerant Context the Normal/ShadePoint arms construct a VectorFrame through. The kernel Context ctor is
// private and Context.Of(UnitSystem) returns Validation<Error,Context>; the page resolves that admission ONCE into a
// ready value (millimetre model tolerance), so a per-sample frame build never re-validates and a near-degenerate
// perturbation re-seeds a perpendicular tangent through the Vectors owner's own Fin rail rather than faulting mid-shade.
// The IfFail throw is statically unreachable — Context.Of(UnitSystem.Millimeters) is a total success (it routes the
// kernel Millimeters() row) — named the boundary construction exemption: a static field must hold a non-optional Context,
// and an Option<Context> field would force every call site to handle a None that cannot occur.
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
    // The Texture arm carries a host-free TOTAL (u,v)->PortValue closure — the texture#TEXTURE_UV TextureUv.Port bridge
    // (texture#GRAPH_NODE_BRIDGE owns it) mints that closure from a TextureSource + UvSample anchor + SamplerState +
    // Channel, folding a non-finite/undersized sample to the Channel neutral so the arm stays total, and the wiring site
    // composes `new Texture(id, TextureUv.Port(...))`. The case knows ONLY the Func (texture COMPOSES this page's
    // PortValue/PortId/ShadePoint, so graph is the lower owner and carries no texture-namespace type), never a raw caller
    // lambda re-implementing a sampler — the Sample delegate is ALWAYS a TextureUv.Port closure.
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
}

public sealed record SurfaceShade(Unicolour BaseColorLinear, double Metalness, double Roughness, VectorFrame ShadingFrame, Unicolour EmissionLinear) {
    public bool InGamut => BaseColorLinear.IsInRgbGamut && EmissionLinear.IsInRgbGamut;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed record PortEnvironment(HashMap<PortId, PortValue> Resolved) {
    public static readonly PortEnvironment Empty = new(HashMap<PortId, PortValue>.Empty);
    public Fin<PortValue> Read(PortId id, Op key) => Resolved.Find(id).ToFin(MaterialFault.Graph(key, $"<unbound-port:{id.Value}>"));
    public PortEnvironment With(PortId id, PortValue value) => this with { Resolved = Resolved.AddOrUpdate(id, value) };
}

public static class NodeEvaluator {
    public static Fin<Option<PortValue>> Evaluate(AppearanceNode node, ShadePoint point, MaterialParameters parameters, PortEnvironment env, Op key) =>
        node.Switch(
            input: i => Fin.Succ(Some(i.Pull(parameters))),
            texture: t => Fin.Succ(Some(t.Sample(point.U, point.V))),
            math: m => EvaluateMath(m, env, key).Map(Some),
            mix: x => EvaluateMix(x, env, key).Map(Some),
            normal: n => EvaluateNormal(n, point, env, key).Map(Some),
            bsdfOutput: _ => Fin.Succ(Option<PortValue>.None));

    static Fin<PortValue> EvaluateMath(AppearanceNode.Math m, PortEnvironment env, Op key) =>
        from lhs in env.Read(m.Lhs, key)
        from rhs in m.Rhs.Match(Some: r => env.Read(r, key), None: () => Fin.Succ<PortValue>(new PortValue.Scalar(0.0)))
        select m.Op.Apply(lhs, rhs);

    internal static double SchlickWeight(double cosTheta) { double m = System.Math.Clamp(1.0 - cosTheta, 0.0, 1.0); double m2 = m * m; return m2 * m2 * m; }

    static Fin<PortValue> EvaluateMix(AppearanceNode.Mix x, PortEnvironment env, Op key) =>
        from a in env.Read(x.A, key)
        from b in env.Read(x.B, key)
        from f in env.Read(x.Factor, key)
        select x.Op.Apply(a, b, System.Math.Clamp(f.AsScalar, 0.0, 1.0));

    static Fin<PortValue> EvaluateNormal(AppearanceNode.Normal n, ShadePoint point, PortEnvironment env, Op key) =>
        from sample in env.Read(n.Source, key)
        let raw = sample.AsVector
        let tangentSpace = new Vector3d((2.0 * raw.X - 1.0) * n.Strength, (2.0 * raw.Y - 1.0) * n.Strength, 2.0 * raw.Z - 1.0)
        let basis = point.Frame.Value
        let world = (basis.XAxis * tangentSpace.X) + (basis.YAxis * tangentSpace.Y) + (basis.ZAxis * tangentSpace.Z)
        from perturbed in VectorFrame.Of(origin: basis.Origin, normal: world, xHint: Some(basis.XAxis), context: GraphContext.Tolerant, key: key)
        select (PortValue)new PortValue.Frame(perturbed);
}

public sealed record MaterialGraph(Seq<AppearanceNode> Nodes, PortId Sink) {
    // The compile fold on the shared QuikGraph substrate: AddVertexRange admits every node so an isolate still orders,
    // AddVerticesAndEdge adds one dependency->dependent SEdge per KNOWN dependency (a dangling port stays out of the
    // graph and faults at Shade's env.Read; allowParallelEdges: false deduplicates an authored Lhs==Rhs double edge),
    // IsDirectedAcyclicGraph pre-gates a cycle onto MaterialFault.Graph before SourceFirstTopologicalSort (Kahn) would
    // throw NonAcyclicGraphException. A colliding node id faults FIRST — AddOrUpdate would otherwise silently drop the
    // earlier node's semantics from the compiled order. The hand-rolled indegree/Queue/CollectionsMarshal kernel is the
    // deleted form; the mutable AdjacencyGraph build is the substrate's own catalogued construction seam, not a page kernel.
    public Fin<CompiledGraph> Compile(Op key) {
        HashMap<PortId, AppearanceNode> byId = Nodes.Fold(HashMap<PortId, AppearanceNode>.Empty, static (m, n) => m.AddOrUpdate(n.Id, n));
        AdjacencyGraph<PortId, SEdge<PortId>> dag = new(allowParallelEdges: false);
        dag.AddVertexRange(Nodes.Map(static n => n.Id));
        Nodes.Iter(n => n.Dependencies.Filter(byId.ContainsKey).Iter(d => dag.AddVerticesAndEdge(new SEdge<PortId>(d, n.Id))));
        return byId.Count != Nodes.Count
            ? Fin.Fail<CompiledGraph>(MaterialFault.Graph(key, "<duplicate-node-id>"))
            : !byId.Find(Sink).Map(static n => n is AppearanceNode.BsdfOutput).IfNone(false)
                ? Fin.Fail<CompiledGraph>(MaterialFault.Graph(key, "<sink-not-bsdf-output>"))
                : !dag.IsDirectedAcyclicGraph()
                    ? Fin.Fail<CompiledGraph>(MaterialFault.Graph(key, "<cyclic-appearance-graph>"))
                    : Fin.Succ(new CompiledGraph(toSeq(dag.SourceFirstTopologicalSort()).Map(id => byId[id]), Sink, byId));
    }

    // The ONE-SHOT convenience: Compile + Shade in one call for a single sample (a preview, a wire-egress shade). It
    // RE-SORTS every call, so the per-sample integrator hot path NEVER routes here — it Compiles ONCE to a CompiledGraph
    // then calls compiled.Shade per sample (the frozen-order re-entry), so the sort is paid once per material, not per ray.
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

public sealed record CompiledGraph(Seq<AppearanceNode> Order, PortId Sink, HashMap<PortId, AppearanceNode> ById) {
    public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Op key) =>
        Order.Fold(Fin.Succ(PortEnvironment.Empty), (acc, node) => acc.Bind(env => NodeEvaluator.Evaluate(node, point, parameters, env, key).Map(value => value.Match(Some: v => env.With(node.Id, v), None: () => env))))
            .Bind(env => ById.Find(Sink).ToFin(MaterialFault.Graph(key, "<sink-missing>"))
                .Bind(sink => sink is AppearanceNode.BsdfOutput output
                    ? Assemble(output, point, env, key)
                    : Fin.Fail<SurfaceShade>(MaterialFault.Graph(key, "<sink-not-bsdf-output>"))));

    static Fin<SurfaceShade> Assemble(AppearanceNode.BsdfOutput sink, ShadePoint point, PortEnvironment env, Op key) =>
        from baseColor in env.Read(sink.BaseColor, key)
        from metalness in env.Read(sink.Metalness, key)
        from roughness in env.Read(sink.Roughness, key)
        from normalFrame in env.Read(sink.NormalFrame, key)
        from emission in env.Read(sink.Emission, key)
        let frame = normalFrame switch { PortValue.Frame f => f.Value, _ => point.Frame }
        let shade = new SurfaceShade(baseColor.AsColor, System.Math.Clamp(metalness.AsScalar, 0.0, 1.0), System.Math.Clamp(roughness.AsScalar, 0.0, 1.0), frame, emission.AsColor)
        from valid in guard(shade.InGamut, MaterialFault.Gamut(key, $"<shade-out-of-gamut:{shade.BaseColorLinear.Hex}>"))
        select shade;
}
```

## [03]-[MATERIAL_LIBRARY]

- Owner: `MaterialLibrary` over `MaterialParameters` keyed by the seam `MaterialId`; `SubsurfaceRadius` the validated mean-free-path carrier; `ThinFilm` the validated interference-film carrier (the OpenPBR `thin_film` group as one value object); `MaterialParameters` the canonical row — sixteen positional Disney-principled columns plus the init-defaulted `Film`.
- Cases: the transcribed seed spans sixteen material families across thirty-four DATA ROWS — `metal.gold`/`metal.copper`/`metal.aluminum`/`metal.titanium`/`metal.iron`/`metal.steel`/`metal.silver`/`metal.chrome`/`metal.brass` (`metal.steel` the galvanized/structural-steel render row the `Component/component#COMPONENT_OWNER` `Component.AppearanceId` resolves — a warm-grey conductor distinct from the bluer `metal.iron`), `glass.crown`/`glass.flint`, `liquid.water`/`liquid.oil`, `gas.cavity`, `gem.diamond`, `stone.jade`/`stone.marble`, `plastic.abs`/`plastic.pvc`, `rubber.matte`, `polymer.adhesive` (the amber structural-epoxy render row a bonded `Component/joint#JOINT_FAMILY` `AdhesiveClass` joint's `AppearanceId` resolves — a smooth IOR-1.55 dielectric, distinct from the `metal.steel` base-metal `SubstanceId`), `skin.caucasian`/`skin.deep`, `fabric.velvet`/`fabric.silk`/`fabric.denim`, `paint.car-metallic`/`paint.clearcoat`, `ceramic.glazed`/`ceramic.porcelain`, `wax.beeswax`/`wax.candle`, `wood.oak`, `coat.gold-leaf` — each a row of `MaterialParameters` values the catalog grows by pure data addition (a new measured material is one row, not a new type), ZERO per-material types.
- Entry: `public static Fin<MaterialParameters> Lookup(MaterialId id, Op key)` — `Fin<T>` aborts on an unregistered id (`MaterialFault.Parameter`, key-correlated); an ad-hoc parameter vector admits through `MaterialParameters.Of` directly — the ONE row validation catalog rows and measured imports share, never a library-level forwarding alias; `Assign` is the profile-generalization seam mapping a masonry `Component` `MaterialId` to a catalog row; `Named` re-bases a Datasets named colour into a row's scene-linear `BaseColor`; `NearestChecker` (metric-parameterized over the full `DeltaE` selector), `HueConstant` (the Ebner-Fairchild constant-hue witness), `PointerAdmit`/`SpectralAdmit`/`MapToPointer`/`MapToSpectral` the reproducibility gates and recoveries, and `CvdPreview`/`Contrast`/`NearestIscc` the accessibility and designation projections.
- Packages: Wacton.Unicolour (base-color/emission construction; the `IsInPointerGamut`/`MapToPointerGamut` Pointer real-surface gamut accessors, the `IsInMacAdamLimits`/`MapToMacAdamLimits`/`IsImaginary` MacAdam spectral-limit accessors, the full 12-member `DeltaE` selector through `Difference` (the drift gate dispatches `Ciede2000`/`Cam16`/`Hyab` by the caller's policy row), the `Contrast` WCAG ratio, and the `Simulate(Cvd, double severity)` colour-vision-deficiency projection over the `Cvd` 8-member selector), Wacton.Unicolour.Datasets (composed for `Macbeth.All` ColorChecker validation, `Css`/`Xkcd`/`Nord` named-colour resolution, the `EbnerFairchild` `AllHue0..AllHue336` constant-hue loci driving `HueConstant` (`HungBerns` the admitted alternate loci family), and the `IsccNbs` 267 designation centroids driving `NearestIscc` — validation/reference only), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `System.Reflection` for the one definition-time Datasets field derivation)
- Growth: a new material is one `MaterialLibrary.Rows` entry — a `MaterialId` key and a `MaterialParameters` value. A new appearance parameter shared by ALL materials is one column on `MaterialParameters` (init-defaulted so existing rows are unaffected — the `Film` thin-film carrier is exactly this growth, landed as the OpenPBR `thin_film` group's row source). A new gamut domain (the display RGB gate, the Pointer real-surface gate, the MacAdam spectral-limit gate) is one accessor predicate by domain, never a collapse of the three into one gate; a new drift metric is a `DeltaE` member the caller's policy row passes, never a second checker; a new reference dataset is one reflection-derived table over the admitted Datasets assembly; a new accessibility-preview is one read-only projection over the package's own selector, never a stored row. There is NO per-material type, NO `GoldMaterial`/`GlassMaterial` class, NO `MetalFactory`/`PlasticFactory`, and NO per-family graph variant — the named defect is a second material surface; the cure is a row. The conductor and dielectric rows are the measured-spectral grounding target framed at `[4]-[RESEARCH]`.
- Boundary: `MaterialParameters` is the single material concept — its columns are the closed Disney-principled parameter set (base color, metalness, roughness, specular tint, anisotropy, IOR, transmission, transmission roughness, sheen, sheen tint, clearcoat, clearcoat roughness, subsurface weight, subsurface radius, emission color, emission luminance) plus the init-defaulted `Film` interference carrier — the OpenPBR `thin_film` group's ROW SOURCE the `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` reads (retiring its hardcoded thin-film zeros) and the `finish#FINISH` pearlescent/anodized rows seed, validated once at `ThinFilm.Create` so a negative thickness, an out-of-unit weight, or a sub-unity film IOR is unrepresentable and every existing positional construction binds unchanged; base color and emission are constructed once through Wacton.Unicolour scene-linear `Acescg` so the table carries spectrally-grounded colors, never raw byte triples; the metalness/IOR pairing is the conductor/dielectric discriminant the `bsdf#LOBE_FAMILY` lobe weights read (metalness=1 selects the conductor lobe with the base color as F0, metalness=0 the dielectric lobe with IOR-derived F0), so a "metal" and a "plastic" differ only by the metalness, IOR, and roughness columns — never by type; transmission>0 with IOR selects the dielectric-transmission lobe so glass, water, the sealed IGU cavity gas (`gas.cavity`, IOR 1.0 so its transmissive interface carries no Fresnel and the `Component/glazing#GLAZING_FAMILY` cavity layers shade as a clear non-refracting fill rather than the `liquid.water` proxy), and gems are rows differing only in IOR and transmission roughness; subsurface weight>0 routes the subsurface lobe so skin, wax, jade, and marble are rows differing only in subsurface radius (the per-channel mean-free-path carried as the validated three-band `SubsurfaceRadius` `[ComplexValueObject]`, a negative or non-finite millimetre band unrepresentable at `Create` so the inline negative-mfp guard `MaterialParameters.Of` once carried is gone); sheen>0 routes the sheen lobe so velvet, silk, and denim are rows differing only in sheen and roughness; clearcoat>0 layers the clearcoat lobe so car paint and glazed ceramic are rows differing only in clearcoat and clearcoat roughness; the profile consumer generalizes through `Assign`, which maps a masonry `Component` to a `MaterialId` row and NEVER mints a profile-specific material — `Component/masonry#MASONRY_FAMILY` is the cross-section owner the engine reads, never modifies, and an unmapped key falls back to the neutral `ceramic.porcelain` row rather than a fault so the profile consumer always shades; the Wacton.Unicolour.Datasets composition is validation/reference only — `NearestChecker` gates a candidate against the nearest `Macbeth.All` ColorChecker patch by `Unicolour.Difference` under the CALLER'S `DeltaE` metric (a drift beyond tolerance rails `MaterialFault.Gamut`; the metric is a policy value on the finish row, never a hidden default), `HueConstant` anchors a REFERENCE to its nearest `EbnerFairchild` constant-hue locus and requires the candidate within tolerance of that SAME locus (a tint that walked off-hue rails the reused `Gamut` case), `NearestIscc` projects the nearest of the 267 ISCC-NBS centroids as the standardized designation a specification prints, and `Named` re-bases a passed `Css`/`Xkcd`/`Nord` named `Unicolour` into a row's scene-linear `BaseColor` through `ConvertToConfiguration(SceneLinear)` FIRST (so the read channels are genuinely AP1-linear, not an sRGB-linear triple mislabelled as AP1 — the same colorimetric boundary the AP1 luminance honors); the ISCC/loci tables are ONE definition-time reflection derivation over the admitted assembly's own public static fields (`SYMBOLIC_REFERENCE`: the names and groups travel as the assembly's identifiers, never a hand-keyed 267-row transcription that drifts), the observer CMFs/illuminant SPDs/reflectance staying on the main Wacton.Unicolour owner the Datasets package does not carry; there are THREE gamut gates BY DOMAIN, never one collapse — `SurfaceShade.InGamut` reads the display `IsInRgbGamut` (the preview-reproducibility gate every row evaluates through), `PointerAdmit` reads the Pointer real-surface `IsInPointerGamut` (the physical-reproducibility gate a pigment-mixed reflectance must pass, the predicate `Appearance/finish#FINISH` imports for its admission), and `SpectralAdmit` reads the MacAdam optimal-colour `IsInMacAdamLimits` (the absolute spectral-locus bound a reflectance physically reachable at its luminance must satisfy, a reflectance beyond the spectral locus first caught by `IsImaginary` so an imaginary colour rails before the MacAdam test), each domain-gate railing the SAME `MaterialFault.Gamut` case with its own domain reason string (the case is reused across all three, never a second fault) and `MapToPointer`/`MapToSpectral` returning the nearest in-gamut Pointer/MacAdam `Unicolour` for the recoverable path — the gate ladder runs display RGB inside Pointer real-surface inside the MacAdam spectral-locus bound, the three never collapsed because each names a distinct physical reproducibility domain (the `HueConstant` witness sits BESIDE the ladder as a constancy check, never a fourth gamut); the `CvdPreview` accessibility projection reads `Unicolour.Simulate(Cvd, double severity)` over the `Cvd` 8-member deficiency selector and `Contrast` reads the WCAG ratio into a typed threshold receipt (`4.5`/`3.0`/`7.0` — AA text, AA large/UI, AAA text) — READ-ONLY projections the color-specification seam consumes, never stored library columns; every row evaluates to an in-gamut `SurfaceShade` through the same `MaterialGraph`.

```csharp signature
// (Continues the Rasm.Materials.Appearance.Graph compilation unit — the [02] prelude's usings, including
// `using Rasm.Element;` and `using Wacton.Unicolour.Datasets;`, are in scope; no duplicate import block.)

// --- [TYPES] -------------------------------------------------------------------------------
// MaterialId is a CROSS-PACKAGE identity owned by the SEAM (`Rasm.Element` `Composition/material#MATERIAL_COMPOSITION`
// declares the one `[ValueObject<string>]` material key with `ComparerAccessors.StringOrdinalIgnoreCase`). The
// `Material`/`MaterialComposition`/`MaterialLayer`/`MaterialConstituent` seam types and every Materials catalogue key
// on it, so this page composes the seam type rather than declaring a parallel `family.name` identity — the prior local
// `MaterialId` is RETIRED. The seam key comparer (ordinal-ignore-case) travels with the type; the shipped
// `ComparerAccessors.StringOrdinal` is the ordinal string policy the local `MathOp`/`MixOp` string keys compose, NOT
// the material identity (and NOT the int-keyed `PortId`, which carries no comparer).

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

// The OpenPBR thin_film group as ONE validated carrier — interference weight, film thickness (nm), film IOR — the
// row source surface#OPENPBR_SLAB OpenPbrSurface.Of reads into ThinFilmWeight/ThinFilmThickness/ThinFilmIor and
// finish#FINISH pearlescent/anodized rows seed. None (weight 0) is the no-film algebra zero every row defaults to;
// an out-of-unit weight, a negative/non-finite thickness, or a sub-unity IOR is unrepresentable at Create.
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

    // The interference-film column: init-defaulted enrichment (the acquisition#ACQUISITION Provenance mechanic), so
    // all thirty-four positional rows and every sibling construction bind unchanged; a film-bearing row spells
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

    // The neutral fallback row an unmapped profile key shades with — a named policy value, never an inline literal.
    static readonly MaterialId Neutral = MaterialId.Of("ceramic.porcelain");

    // Datasets reference tables as ONE definition-time derivation over the admitted assembly's public static fields —
    // the 267 IsccNbs designation centroids keyed by the assembly's OWN field identifiers, the 15 EbnerFairchild
    // AllHue* constant-hue loci — SYMBOLIC_REFERENCE, never a hand-keyed transcription that drifts from the shipped data.
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

    // The drift witness under the CALLER'S DeltaE policy — Ciede2000 for pigment paints, Cam16 for effect finishes,
    // Hyab for large-difference composites — the metric a finish#FINISH FinishHandling row value, never a hidden default.
    public static Fin<(Unicolour Patch, double DeltaE)> NearestChecker(Unicolour candidate, double tolerance, DeltaE metric, Op key) =>
        toSeq(Macbeth.All)
            .Map(patch => (Patch: patch, DeltaE: candidate.Difference(patch, metric)))
            .Fold(Option<(Unicolour Patch, double DeltaE)>.None, static (best, row) => best.Filter(b => b.DeltaE <= row.DeltaE).IsSome ? best : Some(row))
            .ToFin(MaterialFault.Parameter(key, "<colorchecker-set-empty>"))
            .Bind(nearest => nearest.DeltaE <= tolerance
                ? Fin.Succ(nearest)
                : MaterialFault.Gamut(key, $"<colorchecker-drift:deltaE={nearest.DeltaE:R}>"));

    // The Ebner-Fairchild hue-constancy witness: anchor the REFERENCE to its nearest constant-hue locus (min Ciede2000
    // over each AllHue* group), then require the CANDIDATE within tolerance of that SAME locus — a tinted composite
    // that walked off the reference hue rails the reused Gamut case; finish#FINISH gates every composite through this.
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

    // The ISCC-NBS designation projection: the nearest of the 267 centroids by Ciede2000 — the standardized colour
    // NAME a finish schedule or specification prints, read-only beside CvdPreview/Contrast, never a stored row column.
    public static (string Name, Unicolour Centroid, double DeltaE) NearestIscc(Unicolour candidate) =>
        toSeq(IsccCentroids)
            .Map(row => (Name: row.Key, Centroid: row.Value, DeltaE: candidate.Difference(row.Value, DeltaE.Ciede2000)))
            .Fold((Name: "<unnamed>", Centroid: candidate, DeltaE: double.MaxValue), static (best, row) => row.DeltaE < best.DeltaE ? row : best);
}
```

## [04]-[RESEARCH]

- [MATERIALX_GRAPH_INTERCHANGE]: REALIZED at `interchange#MATERIALX_DOCUMENT` — the `AppearanceNode` union and `PortValue` set project onto the MaterialX 1.39 node-graph schema through the `interchange#MATERIALX_DOCUMENT` `NodeCategory` map and the `MtlxPort` typed-port axis so a `MaterialGraph` serializes to and from `.mtlx` (root `<materialx version="1.39">`, `<nodegraph>` of `<node>` with `<input>`/`<output>`), the six node cases mapping to the MaterialX categories through the interchange per-node category resolution (per-`MathOp`/per-`MixOp` category rows; `Mtlx.CategoryOf(TextureSource)` for the texture arm), the `PortValue` polarities onto the per-slot MaterialX typed ports (`Scalar`→`float`, `Color`→`color3`, `Vector`/`Frame`→`vector3`) rather than a blanket `color3`, the `texture#TEXTURE_UV` `TextureSource.MtlxCategory` resolving the `Texture` arm's real category, and the Standard-Surface-to-OpenPBR translation riding the `surface#OPENPBR_SLAB` lowering — the full `open_pbr_surface` input set, not a 12-input subset. The `Mix` arm carries its `MixOp.Mode` datum and the PAIRED interchange counterpart is REALIZED: each factor-bearing blend maps to its native MaterialX node (`mix`/`screen`/`overlay`/`dodge`/`burn`/`difference`), the factorless `Multiply`/`Darken`/`Lighten`/`HardLight` projections lower through the interchange `mix`-wrapper expansion so the `Factor` lerp survives the wire, the six HSL/soft modes with no stdlib node rail `MaterialFault.Graph` loud, and the widened `MathOp` roster (`subtract`/`divide`/`modulo`/`sqrt`/`abs`/`sin`/`cos`/`min`/`max`/`normalize`/`cross`) maps to its standard math categories with unary ops crossing on the stdlib `in` port — no flatten-to-`mix` mislabeling survives on either side. The node names derive from the `PortId` ordinal as `node{id}` so the DAG topology round-trips; the `System.Xml.Linq` `.mtlx` serialize/admit is the host-edge concern `interchange` owns, never a re-architecture of this evaluation fold.
- [MEASURED_SPECTRAL_LIBRARY]: the conductor grounding is REALIZED at `surface#CONDUCTOR_IOR` — the `ConductorMetal` rows carry the measured complex refractive index `(η, k)` per RGB band (gold/copper/aluminum/silver/iron/chromium/titanium/brass from the Johnson-Christy / `refractiveindex.info` tables) so a metal row's `bsdf#LOBE_FAMILY` `Conductor` lobe grounds from the measured Fresnel keyed by the row's `metal.<name>` `MaterialId`, the `BaseColor` the perceptual preview seed and the `(η, k)` the shading truth; `ConductorMetal.Resolve(family, name)` maps a `metal.*` id to its `ConductorMetal`. The remaining [UPSTREAM-BLOCKED] leg is the measured isotropic 195-wavelength spectral BRDF (EPFL RGL goniophotometer, brdf-loader `.bsdf` format) admitted through the `surface#SPECTRAL_UPSAMPLE` `ToSpd` per band, blocked on a vendored managed `.bsdf` reader at `acquisition#EPFL_RGL_BRDF_LOADER`; the per-band `(η, k)` table is the INTERNAL leg now landed, the spectral curve the host-edge extension.
- [SUBSURFACE_RADIUS_UNITS]: REALIZED — the `SubsurfaceRadius` mean-free-path is the validated three-band `[ComplexValueObject]` carrier over per-channel non-negative finite millimetre bands (`R`/`G`/`B`), authored from the published skin/marble/wax diffusion profiles, gating a negative or non-finite mean-free-path once at `Create` so a degenerate radius is unrepresentable at admission rather than caught by an inline `MaterialParameters.Of` guard. The `bsdf#LOBE_FAMILY` `Subsurface` lobe reads the carrier's `Magnitude` for the Burley diffusion radius and the `interchange#MATERIAL_WIRE` projection reads the `R`/`G`/`B` bands rather than a `Vector3d.X`/`Y`/`Z` decomposition; the `photometric#PHOTOMETRIC` admission coerces emission luminance through the in-folder `MaterialUnits` UnitsNet boundary (the illuminance row gated by `UnitsNet.Illuminance`, never a reach DOWN to the app-platform `Rasm.Compute` units owner) — no unit type crosses the interior signature, the band carrier the millimetre owner. The `ThinFilm` carrier is the SAME mechanic for the interference film: weight/thickness-nm/IOR validated once at `Create`, `None` the algebra zero, the row's `Film` column the OpenPBR `thin_film` source the `surface#OPENPBR_SLAB` lowering reads and the `finish#FINISH` pearlescent/anodized rows seed — the counterpart `OpenPbrSurface.Of` read (retiring its hardcoded `ThinFilmWeight: 0.0`) is the recorded surface-page residual.
- [GRAPH_CONTEXT]: REALIZED on-page — `GraphContext.Tolerant` is the one tolerant `Rasm.Domain.Context` the `Normal`/`ShadePoint` arms construct the Rasm.Vectors `VectorFrame` through, resolving the kernel `Context.Of(UnitSystem.Millimeters)` `Validation<Error,Context>` admission ONCE into a ready value (the kernel `Context` ctor is `private`, so the page cannot mint a bare `Context` — it composes the public factory) so a near-degenerate perturbation re-seeds a perpendicular tangent through the Vectors owner's own `Fin` rail rather than faulting mid-shade, and the per-sample frame build never re-validates. The carrier is the one `Context` the `interchange#MATERIAL_WIRE` `MaterialWire.Mint` `ShadePoint.Of(..., GraphContext.Tolerant, key)` preview evaluation also composes, so the seam egress and the shade share one tolerant context; the frame owner owns the basis, never a re-minted one here.
