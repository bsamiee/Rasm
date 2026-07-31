# [MATERIALS_GRAPH]

THE NODE-GRAPH APPEARANCE ENGINE and THE POLYMORPHIC MATERIAL LIBRARY. One `AppearanceNode` `[Union]` closes the node-kind family — `Input`, `Texture`, `Math`, `Mix`, `Normal`, `BsdfOutput` — over the typed `PortValue` channel set; one `MaterialGraph.Compile` fold orders the node DAG ONCE on the shared QuikGraph substrate (`IsDirectedAcyclicGraph` gate, `SourceFirstTopologicalSort` order), resolves its sink and its two rentals, and freezes the whole result, and `CompiledGraph.ShadeSpan` re-enters that frozen order over a caller-rented index-addressed scratch whose slot order IS the sort — the per-sample `CompiledGraph.Shade` being that SAME rail over a one-element window, so ONE representation, ONE `NodeEvaluator.Apply` algebra over pre-resolved operands, and ONE `Assemble` gamut gate carry both the integrator and the bake — the rail's own buffers allocate nothing and the per-node immutable-map rebuild the per-point rail once paid is gone from the hot path, while each node production still mints its `PortValue` and `Fin` envelope per texel, the stated bound the class-shaped union carries — and the node algebra is POINTWISE by construction with every neighbourhood kernel owned by `Raster/filter#PLANE_OP` over a whole plane. One `MaterialParameters` record is the canonical Disney-principled parameter vector — a closed positional core beside a widening band of init-defaulted enrichment columns (the OpenPBR `thin_film` carrier, the three tint colours, the diffuse-roughness and anisotropy-rotation axes, the emission unit receipt) — every measured material parameterizes, and one `MaterialLibrary` `FrozenDictionary<MaterialId, MaterialParameters>` is the catalog as DATA ROWS — metal, glass, liquid, gas, gem, stone, plastic, rubber, polymer, skin, fabric, paint, ceramic, wax, wood, coat — so a new material is `MaterialLibrary.Rows[MaterialId.Of("metal.titanium")] = new MaterialParameters(...)`, a row of values, NEVER a `TitaniumMaterial` type. `Rasm.Materials.Appearance.Graph` OWNS the `PortId`/`MathOp`/`MixOp`/`PortValue`/`GraphContext` graph vocabulary (the `MixOp` table one `BlendMode` row per W3C compositing member, the blend behavior a DATA column, never sixteen delegates), the `GraphEdit`/`ShadeChannel` authoring vocabulary the one `MaterialGraph.Author` fold consumes (the producer that MINTS the `Math` and `Mix` kinds the union declares, the arity gate proves, the wire projects, and the WGSL lowering opcodes — its product an ordinary `MaterialGraph`, so authoring and evaluation share one representation and one admission predicate), the `ShadePoint`/`AppearanceNode`/`SurfaceShade`/`PortSlot`/`CompiledGraph`/`MaterialGraph` evaluation surfaces, the `SubsurfaceRadius` mean-free-path and `ThinFilm` interference carriers, the `MaterialParameters` row, and the `MaterialLibrary` catalog/admission/reference folds; it COMPOSES the SEAM `Rasm.Element` `MaterialId` identity (never re-minting a `family.name` key), the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail (never a second fault), the Rasm.Numerics `Direction`/`VectorFrame`/`Context` shading frame (never re-minting a vector or a tolerance), the `texture#TEXTURE_UV` `TextureUv.Port` closure for the `Texture` arm (never re-implementing sampling), QuikGraph as the one graph-algorithm substrate the whole stack folds transient graphs onto (never a hand-rolled Kahn walk), and Wacton.Unicolour directly as the scene-linear/spectral/compositing color owner under the one `Acescg` working space (never re-minting a `ColourSpace`). `SurfaceShade` terminates the graph as the resolved parameter snapshot the `surface#OPENPBR_SLAB` `SlabStack.ToLayered` lowers to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` the integrator shades — the graph resolves the parameters, the lobe math living on the `bsdf`/`surface` pages, never re-derived here. `MaterialId` generalizes the masonry-assignment consumer: a masonry `Component` maps to a `MaterialId`, never to a component-specific material type.

## [01]-[INDEX]

- [02]-[MATERIAL_GRAPH]: `PortId`/`MathOp`/`MixOp` carry the graph vocabulary (`MixOp` the 16-row `BlendMode` table), `PortValue` the channel set, `GraphContext` the tolerant-`Context` carrier, `AppearanceNode` the node union over its `Produces` column, `GraphEdit`/`ShadeChannel` the authoring request family and sink-port vocabulary `MaterialGraph.Author` folds, `ShadePoint`/`PortSlot`/`CompiledGraph`/`MaterialGraph` the QuikGraph-ordered evaluation fold over the one slot-addressed `ShadeSpan` rail its per-point `Shade` window re-enters, and `SurfaceShade` the sink.
- [03]-[MATERIAL_LIBRARY]: `MaterialLibrary` catalogs `MaterialParameters` rows under the seam `MaterialId` key over the `SubsurfaceRadius` mean-free-path and `ThinFilm` interference carriers, generalizes profile assignment, and gates through the `NearestChecker`/`HueConstant`/`Named` Datasets validation seam over the reflection-derived reference tables, the three reproducibility gates by domain — the AP1 working-space bound (`SurfaceShade.InGamut`), Pointer real-surface (`PointerAdmit`), and MacAdam spectral-limit (`SpectralAdmit`) — each railing the kernel `GamutPolicy` row's own containment, with the `Contrast`/`NearestIscc` accessibility and designation projections.

## [02]-[MATERIAL_GRAPH]

- Owner: `MaterialGraph`/`CompiledGraph` over `AppearanceNode`; the `PortId`/`MathOp`/`MixOp`/`PortValue` graph vocabulary; the `GraphContext` tolerant-`Context` carrier; the `GraphEdit` authoring request union and the `ShadeChannel` sink-port roster carrying each channel's read and re-seat; the `ShadePoint`/`SurfaceShade`/`PortSlot` evaluation models.
- Cases: `Input` (constant/parameter source) · `Texture` (UV-sampled source — the `texture#TEXTURE_UV` `TextureUv.Port` closure) · `Math` (closed scalar/vector op over upstream ports) · `Mix` (parameterized `BlendMode` composite of two ports) · `Normal` (tangent-space perturbation of the shading frame) · `BsdfOutput` (the single sink assembling the closed lobe set into a `SurfaceShade`); authoring edit {`Node` (a node at a FREE port), `Seat` (a replacement at a TAKEN port), `Route` (a `ShadeChannel` re-seat onto an existing port)}; shade channel {base-color, metalness, roughness, normal-frame, emission}.
- Entry: `public Fin<Unit> ShadeSpan(ReadOnlySpan<ShadePoint> points, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> shades, Op key)` is the ONE evaluation rail — `Raster/press#TEXTURE_PRESS` drives it per band, and the per-point `public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> window, Op key)` the integrator holds re-enters it over a one-element window, so no second environment representation exists to drift from and the rail's own buffers allocate nothing — a per-point entry renting its own scratch and its own window prices two heap arrays per ray to spare a caller two arguments; the per-node `PortValue`/`Fin` productions remain the union's heap cost, deleted only by a value-shaped union this page does not mint — with `ScratchWidth` and `OperandWidth` the two `Compile`-resolved rentals a caller sizes against; `public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key)` is the ONE-SHOT convenience (Compile + Shade for a single sample), while the per-sample path `Compile`s ONCE into a frozen `CompiledGraph` and re-enters it per sample, so the hot loop pays the sort once per material, never per ray. `Fin<T>` aborts at COMPILE on a cyclic DAG (`MaterialFault.Graph`, key-correlated), a duplicate node id, a dangling port reference, a dependency on a non-producing port, or a missing/non-`BsdfOutput` sink, and at SHADE on a short span rental, a degenerate frame perturbation, or an out-of-gamut assembled shade — each shade-time failure re-wrapped with the failing TEXEL INDEX, since a plane fails at one of sixteen million points that all ran the same program (a port-TYPE mismatch cannot fault at all — the `PortValue.AsScalar`/`AsColor`/`AsVector` projections are total by construction); `MaterialGraph.Default` is the canonical Disney-principled wiring every library row drives through; `public Fin<MaterialGraph> Author(Seq<GraphEdit> edits, Op key)` is the ONE producer entry a caller composes a layered or masked appearance through — folding the closed `GraphEdit` request family over `Default` (or any compiled-clean graph) with the node `Admit` predicate and the sink re-seat proofs run at ADMISSION so the product is compile-clean by construction, `public Seq<PortId> Ports(int count)` the fresh id block a session names new wiring with, and `public Fin<PortId> PortOf(ShadeChannel channel, Op key)` the read that tells a composer where a channel is ALREADY wired, so lowering onto a standing graph transcribes no port integer.
- Packages: QuikGraph (composed — `AdjacencyGraph<PortId, SEdge<PortId>>` with `allowParallelEdges: false`, `AddVertexRange` admitting isolates, `AddVerticesAndEdge` per dependency edge, `AlgorithmExtensions.IsDirectedAcyclicGraph` the cheap cycle pre-gate, `AlgorithmExtensions.SourceFirstTopologicalSort` the Kahn order — the one graph-algorithm substrate `Rasm.Element`/`Rasm.Persistence`/`Rasm.Bim` already fold onto, admitted folder-locally against the central pin), Rasm (project — `Direction`/`VectorFrame`/`Context`/`Op`, `Rhino.Geometry.Point3d`/`Vector3d`/`Plane` at the host edge), Rasm.Element (the SEAM `MaterialId`, composed not re-declared), Rasm.Materials.Appearance.Bsdf (the `MaterialFault` band-2450 rail composed from `bsdf#SHADING_FRAME`), Wacton.Unicolour (color/spectral/compositing compose — `Mix`, `Blend(backdrop, BlendMode)`, the 16-member `BlendMode` vocabulary), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`; `System.Buffers.ArrayPool<PortValue>.Shared` for the one operand buffer a `Span` cannot carry through the generated `Switch` state, rented per fold and returned cleared). `texture#TEXTURE_UV` `TextureUv.Port` mints the `Texture` arm's closure — `texture` COMPOSES this page's `PortValue`/`PortId`/`ShadePoint`, so graph stays the LOWER owner and the `Texture` case carries only a host-free `Func<double,double,PortValue>`, never a `texture`-namespace type (no cyclic namespace dependency).
- Growth: a new appearance operation is one `MathOp` row (the operation behavior rides the SmartEnum row's `[UseDelegateFromConstructor]` delegate — the roster spans arithmetic incl. floored `modulo`, the unary transcendentals `sqrt`/`abs`/`sin`/`cos`, min/max, the vector `dot`/`cross`/`normalize`, unit clamps, and the Schlick weight, each keyed to its MaterialX standard math category) or one `MixOp` row naming its `BlendMode` member (the blend behavior IS the `Mode` data column the ONE `Apply` derivation reads — never a new arm, never a hand-rolled channel composite); a genuinely new node KIND with no parameterization of the six is one `AppearanceNode` case; a new port channel is one `PortValue` case carrying its CLR carrier; a new lobe assembled at the sink is one `BsdfLobe` `[Union]` case on the `bsdf` page — never a per-effect graph variant and never a sibling node type. A new AUTHORING move is one `GraphEdit` case the generated `Switch` forces every fold site to route, and a sixth sink port is one `ShadeChannel` row carrying its read and its re-seat — never a per-op `Multiply`/`Screen`/`Lerp` factory family re-spelling the thirty-five `MathOp`/`MixOp` rows the declaration already closes, and never a per-channel `RouteBaseColor`/`RouteEmission` entrypoint. `interchange#MATERIALX_DOCUMENT` projects its `NodeCategory`/`MtlxPort` map onto the `AppearanceNode` union and `PortValue` set, the MaterialX 1.39 node-graph alignment target.
- Boundary: the node DAG is the only appearance-program shape — a per-material hand-written shade function is the deleted form; `PortValue` is the only inter-node channel and carries scalar/`Unicolour`/`Vector3d`/`VectorFrame` polarities so a node arm reads typed ports and never `object`; the `Color`→`Scalar` projection is the AP1 scene-linear luminance dot the `bsdf#LOBE_FAMILY` owner derives from the working space's own chromaticities (the AP1-primary luminance row consistent with the declared `Acescg` working space and the `bsdf#LOBE_FAMILY` `RgbSpectrum.Luminance` weights — a Rec709 weight on AP1-linear channels is the colorimetric defect, biasing a green-heavy mask), never a red-channel read, so a mask pulled from a color is photometrically weighted and cannot silently bias to red; the `Texture` arm carries the TOTAL `Func<double,double,PortValue>` closure the `texture#TEXTURE_UV` `TextureUv.Port(TextureSource, UvSample, SamplerState, Channel, Op)` mints — the node holds the delegate, the sampling fold lives on the `texture` page, and the arm never re-implements a sampler nor admits a raw caller-supplied lambda that bypasses the `Channel`-neutralized fault rail; the `Normal` arm perturbs the composed Rasm.Numerics `VectorFrame` (tangent·bitangent·normal) and never re-mints a basis; the `BsdfOutput` arm assembles the `SurfaceShade` parameter snapshot (resolved base color, metalness, roughness, perturbed shading frame, emission) the renderer reads — the lobe WEIGHTING is the downstream `surface#OPENPBR_SLAB` `SlabStack.ToLayered` lowering of the `MaterialParameters` row to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` the integrator shades, the graph sink being the resolved parameter shade and the lobe math living wholly on the `bsdf`/`surface` pages, never re-derived here, color resolved through the directly-consumed Wacton.Unicolour `RgbConfiguration.Acescg` scene-linear owner; the `BsdfOutput` sink resolves through `Assemble` behind a pattern-matched sink probe (a non-`BsdfOutput` sink rails `MaterialFault.Graph`, never an unchecked cast), never a port write, so the environment carries no dead entry under the sink id and a downstream node cannot read a phantom `Scalar(1.0)`; the `Math` arm folds over its `MathOp` SmartEnum by delegate row so a new operation is a row, never a new arm, and the `MathOp.Fresnel` row supplies only the Schlick angular weight `(1−cosθ)⁵` for a `Mix` lobe blend — the full Fresnel term lives on `bsdf#MICROFACET_KERNEL`, never re-derived here; the `Mix` arm dispatches `b.AsColor.Blend(a.AsColor, Mode)` — the W3C separable/non-separable compositing algebra Unicolour owns, `a` the backdrop, `b` the source, the factor the blend opacity lerped in scene-linear `RgbLinear` — so all sixteen W3C modes are one data column and the prior three-mode hand-rolled `ChannelCompose` channel math is the deleted form; the `Lerp` row IS `BlendMode.Normal` spelled as the HDR-safe scene-linear `Unicolour.Mix` (the blend algebra clips to the `[0,1]` W3C reflectance domain; an over-unity INTERMEDIATE — a scaled mask, a `Math` product — keeps its `>1` channels through the linear arm, while a sink-bound emission port is NORMALIZED chromaticity by construction, `MaterialParameters.EmissionLuminance` carrying the energy, so the `Assemble` `InGamut` gate holds); the node algebra is POINTWISE by construction and the `AppearanceNode` union admits no neighbourhood operation — a blur, a normal-from-height integration, an ambient-occlusion sweep, or any other kernel reading a texel's neighbours lives at `Raster/filter#PLANE_OP` over a whole `Raster/plane#TEXTURE_PLANE`, because a DAG node evaluated per shading point has no neighbours to read, so a node kind pretending otherwise either fabricates them or forces every sample to carry a plane; the press bakes the DAG's pointwise field first and folds the plane algebra AFTER, so the two owners compose in one direction and neither re-implements the other; `Compile` folds the DAG onto the QuikGraph substrate ONCE — `AddVertexRange` admits every node so an isolate still orders, `AddVerticesAndEdge` adds one dependency→dependent `SEdge<PortId>` per KNOWN dependency (`allowParallelEdges: false` deduplicating an authored `Lhs == Rhs` double edge), and one `ANSWERABILITY` sweep railing `MaterialFault.Graph` at COMPILE over the two failures a slot-addressed read cannot distinguish at runtime — a port no node declares (`<dangling-port>`) and a port whose node `Produces` nothing (`<non-producing-port>`, the sink a dependent named) — because both read an UNWRITTEN scratch cell carrying the previous texel's value rather than faulting, where the per-point map read once railed cleanly, so the proof is what keeps one rail total and a per-texel liveness check is exactly the cost the frozen order exists to delete; `IsDirectedAcyclicGraph` pre-gates a cycle onto `MaterialFault.Graph` before `SourceFirstTopologicalSort` throws `NonAcyclicGraphException`, and the sink resolves to its `BsdfOutput` at COMPILE so no rail re-probes a cast per sample; `ShadeSpan` then re-enters the frozen order against a caller-rented `Span<PortValue>` scratch and `Shade` re-enters `ShadeSpan` over a one-element window, so ONE `NodeEvaluator.Apply` algebra over pre-resolved operands and ONE `Assemble` reading that scratch DIRECTLY close every evaluation at the same gamut gate — a `Func<PortId, Fin<PortValue>>` port reader cannot exist here at all, since a lambda may not capture a `Span<T>`, and a second environment shape minted to dodge that is the divergence this collapse forecloses; the prior hand-rolled indegree/`Queue`/`CollectionsMarshal` Kahn kernel is DELETED for the substrate's own catalogued `AdjacencyGraph` construction seam, and the page's ONE `[EXPRESSION_SPINE]` exemption is the `ShadeSpan` span kernel — a fixed-extent index walk over caller-owned buffers, the doctrine's named span-loop carve — while every admission, dispatch, and egress surface on the page is expression-bodied; `GraphContext.Tolerant` is the one tolerant `Context` the `Normal`/`ShadePoint` arms construct the `VectorFrame` through (a millimetre-scale model `Context` whose `Fin` admission the page resolves once, so a near-degenerate perturbation re-seeds a perpendicular tangent through the `Rasm.Numerics` owner rather than faulting mid-shade); `MaterialGraph.Default` carries the geometric frame unperturbed through one `Normal` node at `Strength 0` whose identity tangent-space sample `(0.5,0.5,1.0)` decodes to `+Z`, so a library row is parameters evaluated through this one standard graph, never a per-row graph type; a cycle, a dangling port, a duplicate node id, or a non-`BsdfOutput` sink rails `Fin.Fail` and never propagates a NaN shade outward; AUTHORING is the same algebra read backwards and shares its proof — `MaterialGraph.Author` folds the closed `GraphEdit` family through the SAME `Admit` predicate `Compile` runs, the only difference being the KNOWN-SET each hands it (`Compile` the whole node map, so a dependency declared later is legal; `Author` the nodes admitted so far, so an unresolved dependency IS a forward reference an incremental fold cannot have), and a second copy of the arity or answerability sweep beside it is the fork this sharing forecloses; the authoring product is an ordinary `MaterialGraph` that `Compile`s, `ShadeSpan`s, and lowers to WGSL through the one frozen-order rail, so no authored-graph representation, builder type, or mutable node bag exists to diverge from the evaluated one; `Author` mints no `BsdfOutput` — the graph terminates ONCE and a caller layers onto `Default`'s sink through `Route`, so a second terminal is unrepresentable rather than resolved by an id compare at `Compile`; and a caller never hand-types a port integer, because `Ports` allocates above every authored id and `PortOf` reads whatever the sink already wires, which is what makes a mask blended against the standing base colour a two-edit sequence instead of a transcription of `Default`'s own wiring. LOWERING onto `Default` is `Seat`, never `Node` plus `Route`: a composer that owns the whole channel — the `Raster/set#SET_BIND` `Program` arm binding a texture set is the standing one — replaces the default's Input at that channel's OWN port, because authoring the covered channel at a fresh port instead leaves the default node orphaned in the compiled order, and an isolate the sort still admits pays a `PortValue` production per texel for a scratch cell nothing reads. `Seat` therefore refuses an absent port and a `Produces` flip, so a lowering carries `Default`'s topology BY CONSTRUCTION rather than by a second hand-wiring that must be re-checked against it every time either side widens.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Buffers;                             // ArrayPool — the one operand buffer the generated Switch state cannot carry as a span
using System.Collections.Frozen;
using System.Linq;                                // the definition-time table folds (Fields, the two static-ctor admission sweeps) + Enumerable.Range for the authoring port block
using System.Reflection;                          // the definition-time Datasets field derivation (IsccNbs names, EbnerFairchild loci)
using LanguageExt;
using QuikGraph;                                  // AdjacencyGraph, SEdge — the shared graph substrate (folder-local admission, central pin)
using QuikGraph.Algorithms;                       // AlgorithmExtensions: IsDirectedAcyclicGraph, SourceFirstTopologicalSort
using Rasm.Domain;                                // Context, Op
using Rasm.Element.Composition;                               // MaterialId — the SEAM material-identity owner, composed not re-declared
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault (band 2450, the one appearance fault) + RgbSpectrum.LuminanceWeights (the derived AP1 triple), composed from bsdf#SHADING_FRAME/#LOBE_FAMILY
using Rasm.Materials.Appearance.Photometric;      // EmissionInput — the admitted-emission RECEIPT this row carries as data (its UnitEvidence the unit half); the admission itself stays photometric#PHOTOMETRIC's, so no operation here names that owner
using Rasm.Numerics;                               // Direction, VectorFrame, RgbProfile (the working-space roster PortValue.SceneLinear reads its Configuration off), GamutPolicy (the reproducibility rows every gate rails)
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
// dispatch by data. SHAPE-PRESERVING: the elementwise arithmetic rows fold scalar⊕scalar to Scalar and any wider pair componentwise to
// Vector through ONE Zip helper, so Add(Scalar(1), Scalar(2)) is Scalar(3) and never a broadcast whose AsScalar reads √3 too large — the
// asymmetric scalar-Multiply/vector-Add fork is the deleted form. TOTALITY CONVENTION, no fault channel: a zero divisor folds divide AND
// modulo to 0.0 per component (the MaterialX convention), a negative sqrt operand clamps to 0.0, a zero-length normalize returns the zero
// vector; modulo is FLOORED (the MaterialX/GLSL mod), never the CLR remainder. Arity is a ROW COLUMN Compile gates a Math node against —
// a binary row with an absent Rhs or a unary row carrying one is a compile fault, never a silent ZeroScalar operand.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MathOp {
    public static readonly MathOp Add = new("add", arity: 2, static (l, r) => Zip(l, r, static (a, b) => a + b));
    public static readonly MathOp Subtract = new("subtract", arity: 2, static (l, r) => Zip(l, r, static (a, b) => a - b));
    public static readonly MathOp Multiply = new("multiply", arity: 2, static (l, r) => Zip(l, r, static (a, b) => a * b));
    public static readonly MathOp Divide = new("divide", arity: 2, static (l, r) => Zip(l, r, static (a, b) => b is 0.0 ? 0.0 : a / b));
    public static readonly MathOp Modulo = new("modulo", arity: 2, static (l, r) => Zip(l, r, static (a, b) => b is 0.0 ? 0.0 : a - b * System.Math.Floor(a / b)));
    public static readonly MathOp Scale = new("scale", arity: 2, static (l, r) => new PortValue.Vector(l.AsVector * r.AsScalar));
    public static readonly MathOp Power = new("power", arity: 2, static (l, r) => new PortValue.Scalar(System.Math.Pow(l.AsScalar, r.AsScalar)));
    public static readonly MathOp Sqrt = new("sqrt", arity: 1, static (l, _) => new PortValue.Scalar(System.Math.Sqrt(System.Math.Max(0.0, l.AsScalar))));
    public static readonly MathOp Abs = new("abs", arity: 1, static (l, _) => new PortValue.Scalar(System.Math.Abs(l.AsScalar)));
    public static readonly MathOp Sin = new("sin", arity: 1, static (l, _) => new PortValue.Scalar(System.Math.Sin(l.AsScalar)));
    public static readonly MathOp Cos = new("cos", arity: 1, static (l, _) => new PortValue.Scalar(System.Math.Cos(l.AsScalar)));
    public static readonly MathOp Min = new("min", arity: 2, static (l, r) => Zip(l, r, System.Math.Min));
    public static readonly MathOp Max = new("max", arity: 2, static (l, r) => Zip(l, r, System.Math.Max));
    public static readonly MathOp DotProduct = new("dot", arity: 2, static (l, r) => new PortValue.Scalar(l.AsVector * r.AsVector));
    public static readonly MathOp CrossProduct = new("cross", arity: 2, static (l, r) => new PortValue.Vector(Vector3d.CrossProduct(l.AsVector, r.AsVector)));
    public static readonly MathOp Normalize = new("normalize", arity: 1, static (l, _) => new PortValue.Vector(l.AsVector is { Length: > 0.0 } v ? v / v.Length : l.AsVector));
    public static readonly MathOp Clamp01 = new("clamp01", arity: 1, static (l, _) => new PortValue.Scalar(System.Math.Clamp(l.AsScalar, 0.0, 1.0)));
    public static readonly MathOp OneMinus = new("one-minus", arity: 1, static (l, _) => new PortValue.Scalar(1.0 - l.AsScalar));
    public static readonly MathOp Fresnel = new("fresnel-weight", arity: 2, static (l, r) => new PortValue.Scalar(NodeEvaluator.SchlickWeight(System.Math.Clamp(l.AsVector * r.AsVector, 0.0, 1.0))));

    public int Arity { get; }

    [UseDelegateFromConstructor]
    public partial PortValue Apply(PortValue lhs, PortValue rhs);

    // ONE elementwise fold: two scalars stay a Scalar, any wider pair folds componentwise through the AsVector
    // projection — shape follows the WIDER operand, never a broadcast that re-widens a scalar result.
    static PortValue Zip(PortValue l, PortValue r, Func<double, double, double> fold) =>
        l is PortValue.Scalar ls && r is PortValue.Scalar rs
            ? new PortValue.Scalar(fold(ls.Value, rs.Value))
            : new PortValue.Vector(new Vector3d(
                  fold(l.AsVector.X, r.AsVector.X), fold(l.AsVector.Y, r.AsVector.Y), fold(l.AsVector.Z, r.AsVector.Z)));
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
    public static readonly MixOp Lerp       = new("lerp",       BlendMode.Normal,     linearArm: true);
    public static readonly MixOp Multiply   = new("multiply",   BlendMode.Multiply,   linearArm: false);
    public static readonly MixOp Screen     = new("screen",     BlendMode.Screen,     linearArm: false);
    public static readonly MixOp Overlay    = new("overlay",    BlendMode.Overlay,    linearArm: false);
    public static readonly MixOp Darken     = new("darken",     BlendMode.Darken,     linearArm: false);
    public static readonly MixOp Lighten    = new("lighten",    BlendMode.Lighten,    linearArm: false);
    public static readonly MixOp Dodge      = new("dodge",      BlendMode.ColourDodge, linearArm: false);
    public static readonly MixOp Burn       = new("burn",       BlendMode.ColourBurn, linearArm: false);
    public static readonly MixOp HardLight  = new("hard-light", BlendMode.HardLight,  linearArm: false);
    public static readonly MixOp SoftLight  = new("soft-light", BlendMode.SoftLight,  linearArm: false);
    public static readonly MixOp Difference = new("difference", BlendMode.Difference, linearArm: false);
    public static readonly MixOp Exclusion  = new("exclusion",  BlendMode.Exclusion,  linearArm: false);
    public static readonly MixOp Hue        = new("hue",        BlendMode.Hue,        linearArm: false);
    public static readonly MixOp Saturation = new("saturation", BlendMode.Saturation, linearArm: false);
    public static readonly MixOp Colour     = new("colour",     BlendMode.Colour,     linearArm: false);
    public static readonly MixOp Luminosity = new("luminosity", BlendMode.Luminosity, linearArm: false);

    public BlendMode Mode { get; }

    // LinearArm is the ROW DATUM the one Apply derivation reads — Lerp alone takes the HDR-safe linear Mix while every
    // named blend routes Blend-then-Mix; a `Mode is Normal` type test here is the branch-on-a-row the page's own law
    // deletes, and a new blend row states its arm as data.
    public bool LinearArm { get; }

    public PortValue Apply(PortValue a, PortValue b, double t) =>
        new PortValue.Color(LinearArm
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

    // AP1 scene-linear luminance READ off the one owner — bsdf#LOBE_FAMILY RgbSpectrum.LuminanceWeights derives the
    // triple at type init from this working space's own chromaticities, so a working-space change moves every reader
    // and nothing here restates three decimals a reader cannot check; a Rec709 weight on AP1-linear channels
    // mis-weights green, which is exactly the drift a hand-typed copy could not be forced to notice.
    static double Luminance(Unicolour c) =>
        RgbSpectrum.LuminanceWeights switch { var w => (w.R * c.RgbLinear.R) + (w.G * c.RgbLinear.G) + (w.B * c.RgbLinear.B) };
    public Vector3d AsVector => Switch(scalar: static s => new Vector3d(s.Value, s.Value, s.Value), color: static c => RgbLinearVector(c.Linear), vector: static v => v.Value, frame: static f => f.Value.ZAxis);
    public Unicolour AsColor => Switch(scalar: static s => GreyLinear(s.Value), color: static c => c.Linear, vector: static v => VectorLinear(v.Value), frame: static f => VectorLinear(f.Value.ZAxis));

    static Vector3d RgbLinearVector(Unicolour c) => new(c.RgbLinear.R, c.RgbLinear.G, c.RgbLinear.B);
    static Unicolour GreyLinear(double g) => new(SceneLinear, ColourSpace.RgbLinear, g, g, g);
    static Unicolour VectorLinear(Vector3d v) => new(SceneLinear, ColourSpace.RgbLinear, v.X, v.Y, v.Z);
    // SceneLinear is the folder's NAME for the kernel Acescg working space every Appearance page composes —
    // surface#SPECTRAL_UPSAMPLE, photometric#PHOTOMETRIC, texture#TEXTURE_UV, finish#FINISH, and
    // interchange#MATERIAL_WIRE all read PortValue.SceneLinear. It reads the instance off the kernel RgbProfile row
    // rather than minting one: the Configuration instance IS the colour-space identity, and the texture gradient
    // already crosses this folder through PerceptualColor.OfRgb/ToRgb on RgbProfile.Acescg, so a folder-local mint
    // put two AP1 spaces in one pipeline — a spurious chromatic adaptation per crossing and two conversion caches.
    // SceneLinearDegree10 is the ONE space this folder still declares: a distinct CIE observer IS a distinct
    // tristimulus integration, the large-field readout photometric#PHOTOMETRIC selects cannot share the Degree2
    // instance, and no other package states colour against it, so it seats beside its consumer rather than widening
    // the kernel roster with an observer column every other row leaves at the default.
    internal static readonly Configuration SceneLinear = RgbProfile.Acescg.Configuration;
    internal static readonly Configuration SceneLinearDegree10 =
        new(RgbConfiguration.Acescg, new XyzConfiguration(Illuminant.D65, Observer.Degree10, "<acescg-degree10>"));

    // The two named scene-linear anchors every row, seed, and init-default reads instead of re-minting a Unicolour
    // triple at each site — one spelling for the algebra zero and one for the neutral tint.
    internal static readonly Unicolour Black = GreyLinear(0.0);
    internal static readonly Unicolour White = GreyLinear(1.0);
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

// --- [AUTHORING]
// ShadeChannel closes the sink's five named ports as DATA — each row carrying the port it reads and the re-seat that
// writes it — so routing an authored chain into base colour is a row read, a layering author blends against whatever
// the graph ALREADY wires there instead of hand-typing Default's own port integers, and a sixth sink port is one row
// rather than an arm in the authoring fold. The row pair is why authoring needs no per-channel entrypoint.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShadeChannel {
    public static readonly ShadeChannel BaseColor = new("base-color", static s => s.BaseColor, static (s, p) => s with { BaseColor = p });
    public static readonly ShadeChannel Metalness = new("metalness", static s => s.Metalness, static (s, p) => s with { Metalness = p });
    public static readonly ShadeChannel Roughness = new("roughness", static s => s.Roughness, static (s, p) => s with { Roughness = p });
    public static readonly ShadeChannel NormalFrame = new("normal-frame", static s => s.NormalFrame, static (s, p) => s with { NormalFrame = p });
    public static readonly ShadeChannel Emission = new("emission", static s => s.Emission, static (s, p) => s with { Emission = p });

    [UseDelegateFromConstructor]
    public partial PortId Port(AppearanceNode.BsdfOutput sink);

    [UseDelegateFromConstructor]
    public partial AppearanceNode.BsdfOutput Route(AppearanceNode.BsdfOutput sink, PortId port);
}

// GraphEdit is the AUTHORING REQUEST family — the one closed vocabulary MaterialGraph.Author folds, so the producer
// has ONE entrypoint and a new authoring move is a case the generated Switch forces rather than a sibling factory.
// Each case carries an already-constructed AppearanceNode or a channel row (the union IS the node vocabulary and the
// MathOp/MixOp rows ARE the operation vocabulary, so a per-op Multiply/Screen/Lerp factory family would re-spell
// thirty-five rows the declaration already closes). The three differ by their REFUSAL contract, which is the whole
// of their distinction: Node asserts the port is FREE (a collision refuses, which is what makes Ports-allocated
// authoring safe), Seat asserts the port is TAKEN and replaces what stands there (an absent port refuses, so a
// mis-targeted seat cannot silently become an add), and Route re-seats one sink channel onto an existing port. All
// three refuse against the same predicate Compile runs, so an authoring mistake names its own edit rather than
// surfacing later as a whole-graph fault the caller must bisect.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphEdit {
    private GraphEdit() { }

    public sealed record Node(AppearanceNode Authored) : GraphEdit;
    public sealed record Seat(AppearanceNode Replacement) : GraphEdit;
    public sealed record Route(ShadeChannel Channel, PortId Port) : GraphEdit;
}

public sealed record SurfaceShade(Unicolour BaseColorLinear, double Metalness, double Roughness, VectorFrame ShadingFrame, Unicolour EmissionLinear) {
    public bool InGamut => GamutPolicy.Perceptual.Contains(BaseColorLinear) && GamutPolicy.Perceptual.Contains(EmissionLinear);
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
    // argument and a ref struct is not one; the rail takes it from the shared ArrayPool ONCE per fold, so neither a texel nor a per-sample
    // re-entry allocates and the pool never retains a PortValue past the return. Arms stay static (the closure-free
    // per-sample law) and the Normal arm carries the only fault — every other production is total.
    public static Fin<PortSlot> Apply(AppearanceNode node, ShadePoint point, MaterialParameters parameters, PortValue[] operands, Op key) =>
        node.Switch(
            state:      (Point: point, Parameters: parameters, Operands: operands, Key: key),
            input:      static (s, i) => Fin.Succ(PortSlot.Of(i.Pull(s.Parameters))),
            texture:    static (s, t) => Fin.Succ(PortSlot.Of(t.Sample(s.Point.U, s.Point.V))),
            // Compile's arity gate proved Rhs presence against the op row, so the ZeroScalar here is only the ignored
            // placeholder a unary delegate's second parameter never reads — never a silent binary operand.
            math:       static (s, m) => Fin.Succ(PortSlot.Of(m.Op.Apply(s.Operands[0], m.Op.Arity == 2 ? s.Operands[1] : PortSlot.ZeroScalar))),
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
    // ByPort is the id index BOTH the compile fold and the authoring admission read — one fold expression, never a
    // second index shape drifting from this one. Compile binds it locally so the walk pays the fold once.
    HashMap<PortId, AppearanceNode> ByPort =>
        Nodes.Fold(HashMap<PortId, AppearanceNode>.Empty, static (m, n) => m.AddOrUpdate(n.Id, n));

    // Admit is the ONE node-declaration predicate, returning the refusal reason or None. ARITY reads the op's own row
    // column: a binary row with an absent Rhs once filled a silent ZeroScalar operand — Multiply-by-nothing shaded a
    // black plane wearing a success — and a unary row carrying an Rhs is an authored operand the fold would ignore.
    // ANSWERABILITY covers the two ways a slot read has no cell to answer from: a port the known-set does not declare,
    // and a port whose node PRODUCES nothing (the sink a dependent named). The rail addresses its scratch by SLOT, so
    // either would read the PREVIOUS texel's value in that cell rather than railing, and a per-texel liveness re-check
    // is exactly the cost the frozen order exists to delete. The sweep reads the Produces COLUMN, so a second
    // non-producing node kind needs no edit here; the two reasons stay distinct because they are distinct authoring
    // mistakes. The KNOWN-SET is the caller's discriminant and the whole reason one predicate serves both admissions.
    static Option<string> Admit(AppearanceNode node, HashMap<PortId, AppearanceNode> known) =>
        node is AppearanceNode.Math math && (math.Op.Arity == 2) != math.Rhs.IsSome
            ? Some($"<math-arity:{math.Id.Value}:{math.Op.Key}>")
            : node.Dependencies
                .Filter(d => known.Find(d).Map(static n => n.Produces).IfNone(false) is false)
                .Head
                .Map(d => known.ContainsKey(d)
                    ? $"<non-producing-port:{node.Id.Value}<-{d.Value}>"
                    : $"<dangling-port:{node.Id.Value}<-{d.Value}>");

    // Compile folds onto the shared QuikGraph substrate: AddVertexRange admits every node so an isolate still orders, AddVerticesAndEdge adds
    // one dependency->dependent SEdge per KNOWN dependency (allowParallelEdges: false deduplicates an authored Lhs==Rhs double edge),
    // IsDirectedAcyclicGraph pre-gates a cycle onto MaterialFault.Graph before SourceFirstTopologicalSort (Kahn) throws
    // NonAcyclicGraphException. A colliding node id faults FIRST — AddOrUpdate otherwise silently drops the earlier node's semantics from the
    // compiled order. Hand-rolled indegree/Queue/CollectionsMarshal kernels are the deleted form; the mutable AdjacencyGraph build is the
    // substrate's own catalogued construction seam, not a page kernel. Compile also resolves what the rail must never re-derive: the sink's
    // own BsdfOutput, the two rentals, and the node -> scratch-cell slot map at the frozen order.
    public Fin<CompiledGraph> Compile(Op key) {
        HashMap<PortId, AppearanceNode> byId = ByPort;
        AdjacencyGraph<PortId, SEdge<PortId>> dag = new(allowParallelEdges: false);
        dag.AddVertexRange(Nodes.Map(static n => n.Id));
        Nodes.Iter(n => n.Dependencies.Filter(byId.ContainsKey).Iter(d => dag.AddVerticesAndEdge(new SEdge<PortId>(d, n.Id))));
        return from _ in guard(byId.Count == Nodes.Count, MaterialFault.Graph(key, "<duplicate-node-id>"))
               // ONE declaration sweep over the shared Admit predicate — the ARITY gate against the op's own row
               // column and the ANSWERABILITY pair — reading the whole node map, so a dependency declared LATER is
               // legal here where the incremental author has no forward reference to admit. Compile and Author read
               // the SAME predicate against two known-sets, so a new refusal reason lands once.
               from _admitted in Nodes.Choose(n => Admit(n, byId)).Head.Match(
                   Some: reason => Fin.Fail<Unit>(MaterialFault.Graph(key, reason)),
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
        Compile(key).Bind(compiled => compiled.Shade(
            point, parameters, new PortValue[compiled.ScratchWidth], new SurfaceShade[1], key));

    // --- [AUTHORING]
    // Ports allocates a fresh contiguous block ABOVE every authored id, so an authoring session names its own wiring
    // without hand-typing an integer Default already claims — the collision the duplicate-id refusal would otherwise
    // raise at admission. It is a READ, never an ingress: the graph gains nodes through Author alone.
    public Seq<PortId> Ports(int count) =>
        toSeq(Enumerable.Range(Nodes.Fold(0, static (highest, n) => System.Math.Max(highest, n.Id.Value)) + 1, count)).Map(PortId.Of);

    // Author is the ONE producer for the node algebra — the fold that finally MINTS the Math and Mix kinds the union
    // declares, the arity gate proves, the wire projects, and the WGSL lowering opcodes. A layered or masked
    // appearance is a Seq of edits over MaterialGraph.Default, so a caller composes `Author(Seq(
    //   new GraphEdit.Node(new AppearanceNode.Texture(mask, TextureUv.Port(...))),
    //   new GraphEdit.Node(new AppearanceNode.Mix(tinted, MixOp.Multiply, ShadeChannel.BaseColor.Port(sink), mask, weight)),
    //   new GraphEdit.Route(ShadeChannel.BaseColor, tinted)), key)` and the product Compiles, shades, and lowers
    // through the SAME frozen-order rail Default takes — there is no authored-graph representation to diverge from.
    // The fold is a Seq rather than a `params ReadOnlySpan<GraphEdit>` because the page spends its one
    // [EXPRESSION_SPINE] span-kernel exemption on ShadeSpan, and a span may cross no lambda, so an arity-absorbing
    // span here would buy one call shape at the cost of a second imperative walk on an authoring path that shades
    // nothing. Each edit refuses on the typed rail with its OWN reason, so the fold short-circuits at the first
    // malformed move rather than handing Compile an unbisectable graph.
    public Fin<MaterialGraph> Author(Seq<GraphEdit> edits, Op key) =>
        edits.Fold(Fin.Succ(this), (graph, edit) => graph.Bind(g => g.Apply(edit, key)));

    Fin<MaterialGraph> Apply(GraphEdit edit, Op key) =>
        edit.Switch(
            state: (Graph: this, Key: key),
            node: static (s, n) => s.Graph.Admitted(n.Authored, s.Key),
            seat: static (s, n) => s.Graph.Seated(n.Replacement, s.Key),
            route: static (s, r) => s.Graph.Routed(r.Channel, r.Port, s.Key));

    // PortOf reads the port the SINK currently names for a channel — the public form of the ShadeChannel row's own
    // read delegate, so a composer layering onto a standing graph asks it where a channel is wired instead of
    // transcribing the integer Default happens to use. Without this the read column is declared and unreachable,
    // and every composer re-authors the wiring it should be reading.
    public Fin<PortId> PortOf(ShadeChannel channel, Op key) =>
        ByPort.Find(Sink).ToFin(MaterialFault.Graph(key, "<sink-missing>"))
            .Bind(node => node is AppearanceNode.BsdfOutput sink
                ? Fin.Succ(channel.Port(sink))
                : Fin.Fail<PortId>(MaterialFault.Graph(key, "<sink-not-bsdf-output>")));

    // Admitted is the node half: a colliding id, a SECOND sink (the graph terminates once, and a second BsdfOutput
    // would leave Compile choosing between two terminals on an id compare), an arity breach, or a dependency the
    // graph does not yet answer each refuse HERE. The known-set is the nodes authored SO FAR, so an unresolved
    // dependency IS a forward reference — which an incremental fold cannot have — and reads as dangling.
    Fin<MaterialGraph> Admitted(AppearanceNode node, Op key) =>
        ByPort switch { var known =>
            known.ContainsKey(node.Id)
                ? Fin.Fail<MaterialGraph>(MaterialFault.Graph(key, $"<authored-duplicate-node-id:{node.Id.Value}>"))
                : node is AppearanceNode.BsdfOutput
                    ? Fin.Fail<MaterialGraph>(MaterialFault.Graph(key, $"<authored-second-sink:{node.Id.Value}>"))
                    : Admit(node, known).Match(
                        Some: reason => Fin.Fail<MaterialGraph>(MaterialFault.Graph(key, reason)),
                        None: () => Fin.Succ(this with { Nodes = Nodes.Add(node) })) };

    // Seated REPLACES the node standing at an id — the move a composer lowering a texture set onto the default
    // wiring makes, where authoring the covered channel at a FRESH port would leave the default's own Input node
    // orphaned in the compiled order, paying a PortValue production per texel for a cell nothing reads. It refuses
    // an ABSENT port (a seat is a replacement, and silently becoming an add hides a mis-targeted id) and refuses a
    // PRODUCES flip, because a replacement that stops producing strands every dependent the old node answered —
    // the one answerability break a whole-graph sweep would report against the wrong node. The known-set is the
    // WHOLE map: a seat lands on a complete graph, so a dependency declared later is legal exactly as at Compile.
    Fin<MaterialGraph> Seated(AppearanceNode node, Op key) =>
        ByPort switch { var known =>
            known.Find(node.Id).ToFin(MaterialFault.Graph(key, $"<seated-port-absent:{node.Id.Value}>"))
                .Bind(standing => standing.Produces == node.Produces
                    ? Fin.Succ(standing)
                    : Fin.Fail<AppearanceNode>(MaterialFault.Graph(key, $"<seated-produces-flip:{node.Id.Value}>")))
                .Bind(_ => Admit(node, known).Match(
                    Some: reason => Fin.Fail<MaterialGraph>(MaterialFault.Graph(key, reason)),
                    None: () => Fin.Succ(this with { Nodes = Nodes.Map(n => n.Id == node.Id ? node : n) }))) };

    // Routed is the sink half: the channel row carries the re-seat, so this fold owns the two proofs a re-seat needs
    // — the sink resolving to its own BsdfOutput case, and the incoming port being both KNOWN and PRODUCING — and
    // never a per-channel body. Routing onto a port the graph cannot answer is refused at the edit rather than
    // shading a plane off an unwritten scratch cell.
    Fin<MaterialGraph> Routed(ShadeChannel channel, PortId port, Op key) =>
        ByPort switch { var known =>
            known.Find(Sink).ToFin(MaterialFault.Graph(key, "<sink-missing>"))
                .Bind(node => node is AppearanceNode.BsdfOutput sink
                    ? Fin.Succ(sink)
                    : Fin.Fail<AppearanceNode.BsdfOutput>(MaterialFault.Graph(key, "<sink-not-bsdf-output>")))
                .Bind(sink => known.Find(port).Map(static n => n.Produces).IfNone(false)
                    ? Fin.Succ(this with { Nodes = Nodes.Map(n => n.Id == Sink ? channel.Route(sink, port) : n) })
                    : Fin.Fail<MaterialGraph>(MaterialFault.Graph(key, $"<route-unanswerable-port:{channel.Key}<-{port.Value}>"))) };

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
    // gamut gate, and a semantic divergence between them is unrepresentable rather than guarded by review. The scratch and the window are the
    // CALLER'S: an integrator rents them once per material and re-enters per ray, so the per-sample rail allocates nothing — a rail minting
    // its own scratch and its own one-element window prices two heap arrays per sample on the hottest loop in the renderer to spare the caller
    // two arguments, and the window is exactly where the caller reads its result from anyway. A one-shot caller rents inline at the call site,
    // which is what one-shot means. The window read is a total probe: the rail wrote it or the rail failed.
    public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> window, Op key) =>
        ShadeSpan([point], parameters, scratch, window, key)
            .Bind(_ => window[0] is SurfaceShade shaded
                ? Fin.Succ(shaded)
                : Fin.Fail<SurfaceShade>(MaterialFault.Graph(key, "<shade-window-unwritten>")));

    // THE rail press#TEXTURE_PRESS drives per band: the port environment resolves into an index-addressed scratch
    // whose slot order IS the frozen compiled sort — the per-node HashMap rebuild the per-point rail once paid per
    // texel is gone, which is the difference between minutes and days at four thousand square; the remaining heap
    // traffic is each arm's own PortValue/Fin production, a bound the union's class shape carries, never this walk's.
    // Compile proved every dependency both KNOWN and
    // PRODUCING, so every slot read below is TOTAL and no per-texel liveness check exists. Scratch and shades are
    // CALLER-OWNED spans (the press rents them per band, the integrator once per material); a short rental refuses
    // rather than truncating. The operand buffer is the one array the fold cannot take as a span — the generated
    // Switch threads its state as a type argument and a ref struct is not one — so it comes from the shared
    // ArrayPool and returns CLEARED, which keeps the rail's buffer churn off the GC without stranding
    // PortValue references inside the pool. The scratch needs no per-texel clear: the topological order writes
    // every producing cell before any dependent reads it. Each step probes its own outcome ONCE — a fail-test
    // followed by a defaulted unwrap reads the rail twice and needs a sentinel for a branch that already returned,
    // and `default(SurfaceShade)!` is a null forced into a class-typed cell to satisfy it. This walk is the page's
    // named [EXPRESSION_SPINE] span-kernel exemption — a Span may cross no lambda, so the fold has no expression form.
    public Fin<Unit> ShadeSpan(ReadOnlySpan<ShadePoint> points, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> shades, Op key) {
        if (scratch.Length < ScratchWidth || shades.Length < points.Length) {
            return Fin.Fail<Unit>(MaterialFault.Graph(key, $"<shade-span-rental-short:{scratch.Length}/{ScratchWidth}:{shades.Length}/{points.Length}>"));
        }
        PortValue[] operands = ArrayPool<PortValue>.Shared.Rent(OperandWidth);
        try {
            for (int p = 0; p < points.Length; p++) {
                for (int n = 0; n < Order.Count; n++) {
                    AppearanceNode node = Order[n];
                    int[] sources = OperandSlots[n];
                    for (int d = 0; d < sources.Length; d++) { operands[d] = scratch[sources[d]]; }
                    Fin<PortSlot> produced = NodeEvaluator.Apply(node, points[p], parameters, operands, key);
                    if (produced.Case is not PortSlot slot) { return Indexed(produced, p, key).Map(static _ => unit); }
                    if (slot.Produced) { scratch[n] = slot.Value; }
                }
                Fin<SurfaceShade> shade = Assemble(Output, points[p], scratch, Slots, key);
                if (shade.Case is not SurfaceShade assembled) { return Indexed(shade, p, key).Map(static _ => unit); }
                shades[p] = assembled;
            }
            return Fin.Succ(unit);
        }
        finally { ArrayPool<PortValue>.Shared.Return(operands, clearArray: true); }
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

- Owner: `MaterialLibrary` over `MaterialParameters` keyed by the seam `MaterialId`; `SubsurfaceRadius` the validated mean-free-path carrier; `ThinFilm` the validated interference-film carrier (the OpenPBR `thin_film` group as one value object); `MaterialParameters` the canonical row — the closed positional Disney core beside its init-defaulted enrichment band.
- Cases: the transcribed seed spans sixteen material families across thirty-four DATA ROWS — `metal.gold`/`metal.copper`/`metal.aluminum`/`metal.titanium`/`metal.iron`/`metal.steel`/`metal.silver`/`metal.chrome`/`metal.brass` (`metal.steel` the galvanized/structural-steel render row the `Component/component#COMPONENT_OWNER` `Component.AppearanceId` resolves — a warm-grey conductor distinct from the bluer `metal.iron`), `glass.crown`/`glass.flint`, `liquid.water`/`liquid.oil`, `gas.cavity`, `gem.diamond`, `stone.jade`/`stone.marble`, `plastic.abs`/`plastic.pvc`, `rubber.matte`, `polymer.adhesive` (the amber structural-epoxy render row a bonded `Component/joint#JOINT_FAMILY` `AdhesiveClass` joint's `AppearanceId` resolves — a smooth IOR-1.55 dielectric, distinct from the `metal.steel` base-metal `SubstanceId`), `skin.caucasian`/`skin.deep`, `fabric.velvet`/`fabric.silk`/`fabric.denim`, `paint.car-metallic`/`paint.clearcoat`, `ceramic.glazed`/`ceramic.porcelain`, `wax.beeswax`/`wax.candle`, `wood.oak`, `coat.gold-leaf` — each a row of `MaterialParameters` values the catalog grows by pure data addition (a new measured material is one row, not a new type), ZERO per-material types.
- Entry: `public static Fin<MaterialParameters> Lookup(MaterialId id, Op key)` — `Fin<T>` aborts on an unregistered id (`MaterialFault.Parameter`, key-correlated); an ad-hoc parameter vector admits through `MaterialParameters.Of` directly — the ONE row validation catalog rows and measured imports share, never a library-level forwarding alias; `Assign` is the profile-generalization seam mapping a masonry `Component` `MaterialId` to a catalog row; `Named` re-bases a Datasets named colour into a row's scene-linear `BaseColor`; `NearestChecker` (metric-parameterized over the full `DeltaE` selector), `HueConstant` (the Ebner-Fairchild constant-hue witness), `PointerAdmit`/`SpectralAdmit` the two railed reproducibility gates over the kernel `GamutPolicy` rows whose `Bound` is the recovery, and `Contrast`/`NearestIscc` the accessibility and designation projections.
- Packages: Rasm (project — the `Numerics/atoms#SCALAR_FLOOR` `RgbProfile.Acescg.Configuration` scene-linear instance `PortValue.SceneLinear` names, and the `GamutPolicy` `Pointer`/`MacAdam`/`Perceptual` reproducibility rows whose `Contains`/`Bound` pair this page's gates rail and recover through), Wacton.Unicolour (base-color/emission construction; the `IsImaginary` spectral-locus pre-test, the full 12-member `DeltaE` selector through `Difference` (the drift gate dispatches `Ciede2000`/`Cam16`/`Hyab` by the caller's policy row), and the `Contrast` WCAG ratio), Wacton.Unicolour.Datasets (composed for `Macbeth.All` ColorChecker validation, `Css`/`Xkcd`/`Nord` named-colour resolution, the `EbnerFairchild` `AllHue0..AllHue336` constant-hue loci driving `HueConstant` (`HungBerns` the admitted alternate loci family), and the `IsccNbs` 267 designation centroids driving `NearestIscc` — validation/reference only), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `System.Reflection` for the one definition-time Datasets field derivation)
- Growth: a new material is one `MaterialLibrary.Rows` entry — a `MaterialId` key and a `MaterialParameters` value; a new appearance parameter shared by ALL materials is one init-defaulted column on `MaterialParameters` (every existing row binds unchanged — the `Film` thin-film carrier is exactly this growth, landed as the OpenPBR `thin_film` group's row source); a new gamut domain (the display RGB gate, the Pointer real-surface gate, the MacAdam spectral-limit gate) is one accessor predicate by domain, never a collapse of the three into one gate; a new drift metric is a `DeltaE` member the caller's policy row passes, never a second checker; a new reference dataset is one reflection-derived table over the admitted Datasets assembly; a new accessibility-preview is one read-only projection over the package's own selector, never a stored row. There is NO per-material type, NO `GoldMaterial`/`GlassMaterial` class, NO `MetalFactory`/`PlasticFactory`, and NO per-family graph variant — the named defect is a second material surface; the repair is a row. Measured-spectral grounding targets the conductor and dielectric rows, framed at `[04]-[RESEARCH]`.
- Boundary: `MaterialParameters` is the single material concept — its positional core is the closed Disney-principled parameter set (base color, metalness, roughness, specular tint, anisotropy, IOR, transmission, transmission roughness, sheen, sheen tint, clearcoat, clearcoat roughness, subsurface weight, subsurface radius, emission color, emission luminance) and every later axis lands as an INIT-DEFAULTED column so each catalogue row and sibling construction binds unchanged: the `Film` interference carrier (the OpenPBR `thin_film` group's ROW SOURCE the `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` reads and the `finish#FINISH` pearlescent/anodized rows seed, validated once at `ThinFilm.Create` so a negative thickness, an out-of-unit weight, or a sub-unity film IOR is unrepresentable), the three OpenPBR tint colours `CoatColor`/`SpecularColor`/`FuzzColor` neutral at `PortValue.White` (so the `weathering#WEATHERING` `CoatColorTo`/`FuzzColorTo` trajectories, the tinted `finish#FINISH` rows, and the `Raster/set#TEXTURE_SET` `coat_color`/`specular_color`/`fuzz_color` planes each write a REAL column instead of a lens that collapsed a three-band tint to one luminance scalar), `BaseDiffuseRoughness` as the Oren-Nayar axis distinct from the specular roughness beside it, `AnisotropyRotation` as the unit-convention grain reference the `bsdf#LOBE_FAMILY` anisotropic lobes turn by, `ThinWalled` as the OpenPBR `geometry_thin_walled` double-sided-shell flag (a set-level boolean the `foliage.leaf`, `paper.sheet`, `fabric.silk`, and `fabric.denim` rows set, the wire's `GeometryThinWalled` column carries, and the `surface#OPENPBR_SLAB` `Slab.Base` reads to transmit at unit index — the texture roster correctly excludes it as no per-texel field), and `EmissionProvenance` as the typed-absence `EmissionInput` receipt an ADMITTED emission magnitude carries WHOLE — the unit witness beside the chromaticity, CCT+Duv, relative-luminance, and gamut-map evidence the resolve took, so `interchange#MATERIAL_WIRE` mirrors the photometric measurement the way it already mirrors the `acquisition#ACQUISITION` capture measurement rather than stranding it at the resolve; base color and emission are constructed once through Wacton.Unicolour scene-linear `Acescg` so the table carries spectrally-grounded colors, never raw byte triples; `Metalness` is the conductor/dielectric PARTITION the `bsdf#LOBE_FAMILY` lobe weights read and `Ior` is the dielectric arm's own interface index at every row — the conductor arm grounds from the `surface#CONDUCTOR_IOR` `ConductorMetal` row the id names, so a "metal" and a "plastic" differ by the metalness, IOR, and roughness columns and by which measured metal the id resolves, never by type; a conductor's own `(η, k)` never enters this column, which is why the admitted IOR band is total rather than keyed on metalness; transmission>0 with IOR selects the dielectric-transmission lobe so glass, water, the sealed IGU cavity gas (`gas.cavity`, IOR 1.0 so its transmissive interface carries no Fresnel and the `Component/glazing#GLAZING_FAMILY` cavity layers shade as a clear non-refracting fill rather than the `liquid.water` proxy), and gems are rows differing only in IOR and transmission roughness; subsurface weight>0 routes the subsurface lobe so skin, wax, jade, and marble are rows differing only in subsurface radius (the per-channel mean-free-path carried as the validated three-band `SubsurfaceRadius` `[ComplexValueObject]`, a negative or non-finite millimetre band unrepresentable at `Create` so the inline negative-mfp guard `MaterialParameters.Of` once carried is gone); sheen>0 routes the sheen lobe so velvet, silk, and denim are rows differing only in sheen and roughness; clearcoat>0 layers the clearcoat lobe so car paint and glazed ceramic are rows differing only in clearcoat and clearcoat roughness; the profile consumer generalizes through `Assign`, which maps a masonry `Component` to a `MaterialId` row and NEVER mints a profile-specific material — `Component/masonry#MASONRY_FAMILY` is the cross-section owner the engine reads, never modifies, and an unmapped key falls back to the neutral `ceramic.porcelain` row rather than a fault so the profile consumer always shades; the Wacton.Unicolour.Datasets composition is validation/reference only — `NearestChecker` gates a candidate against the nearest `Macbeth.All` ColorChecker patch by `Unicolour.Difference` under the CALLER'S `DeltaE` metric (a drift beyond tolerance rails `MaterialFault.Gamut`; the metric is a policy value on the finish row, never a hidden default), `HueConstant` anchors a REFERENCE to its nearest `EbnerFairchild` constant-hue locus and requires the candidate within tolerance of that SAME locus (a tint that walked off-hue rails the reused `Gamut` case), `NearestIscc` projects the nearest of the 267 ISCC-NBS centroids as the standardized designation a specification prints, and `Named` re-bases a passed `Css`/`Xkcd`/`Nord` named `Unicolour` into a row's scene-linear `BaseColor` through `ConvertToConfiguration(SceneLinear)` FIRST (so the read channels are genuinely AP1-linear, not an sRGB-linear triple mislabelled as AP1 — the same colorimetric boundary the AP1 luminance honors); the ISCC/loci tables are ONE definition-time reflection derivation over the admitted assembly's own public static fields (`SYMBOLIC_REFERENCE`: the names and groups travel as the assembly's identifiers, never a hand-keyed 267-row transcription that drifts), the observer CMFs/illuminant SPDs/reflectance staying on the main Wacton.Unicolour owner the Datasets package does not carry; there are THREE gamut gates BY DOMAIN, never one collapse and never a nesting — `SurfaceShade.InGamut` reads `GamutPolicy.Perceptual.Contains` against the AP1 WORKING SPACE the `SceneLinear` configuration declares (the working-space bound every row evaluates through; ACEScg is wider than any display and is NOT contained in the Pointer volume, so a containment ladder over the three is the false claim this sentence deletes), `PointerAdmit` reads `GamutPolicy.Pointer` (the physical-reproducibility gate a pigment-mixed reflectance must pass, the predicate `Appearance/finish#FINISH` imports for its admission), and `SpectralAdmit` reads `GamutPolicy.MacAdam` (the absolute spectral-locus bound a reflectance physically reachable at its luminance must satisfy, a reflectance beyond the spectral locus first caught by `IsImaginary` so an imaginary colour rails before the MacAdam test), each domain-gate railing the SAME `MaterialFault.Gamut` case with its own domain reason string (the case is reused across all three, never a second fault) while the RECOVERY is the same kernel row's `Bound` — three INDEPENDENT domains named once in the kernel vocabulary, each carrying its predicate and its nearest-in-domain projection together, so a Materials-side projection rename over `MapToPointerGamut`/`MapToMacAdamLimits` is the deleted form (the `HueConstant` witness sits BESIDE them as a constancy check, never a fourth gamut); the accessibility projection is the kernel `PerceptualColor.Simulate(Cvd, UnitInterval)` — a folder-local preview that clamped a raw severity double instead of admitting it was the deleted form — and `Contrast` reads the WCAG ratio into a typed threshold receipt (`4.5`/`3.0`/`7.0` — AA text, AA large/UI, AAA text) — READ-ONLY projections the color-specification seam consumes, never stored library columns; every row evaluates to an in-gamut `SurfaceShade` through the same `MaterialGraph`.

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
    public double Magnitude => System.Math.Sqrt(R * R + G * G + B * B);
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

    // Every column below lands as init-defaulted ENRICHMENT (the acquisition#ACQUISITION Provenance mechanic), so
    // every positional catalogue row and every sibling construction binds unchanged and a row carrying one spells
    // `new(...) { Column = value }`. Film is the OpenPBR thin_film carrier whose Create owns its own validation.
    public ThinFilm Film { get; init; } = ThinFilm.None;

    // Three OpenPBR colour columns feed the surface#OPENPBR_SLAB vector and the Raster/set#TEXTURE_SET roster
    // bakes. White is the neutral every construction already held, so a row that authors none is behaviour-identical
    // — and the three-band tint an ingest, a bake, a weathering trajectory, or a tinted FinishKind writes now has a
    // column to land on instead of being synthesized at the lowering or collapsed to one luminance scalar at bind.
    public Unicolour CoatColor { get; init; } = PortValue.White;
    public Unicolour SpecularColor { get; init; } = PortValue.White;
    public Unicolour FuzzColor { get; init; } = PortValue.White;

    // The row's colour columns as ONE sequence, so a gamut gate, a drift census, a designation sweep, or a working-
    // space rebase folds a predicate over the set instead of re-spelling five member reads that silently skip
    // whichever column a later axis adds — the admission gate, the wire projection, and the library validators all
    // read this one projection.
    public Seq<Unicolour> Colours => Seq(BaseColor, Emission, CoatColor, SpecularColor, FuzzColor);

    // base_diffuse_roughness is the Oren-Nayar term, a DISTINCT axis from the specular Roughness beside it: the
    // OpenPBR default is Lambertian, so 0.0 is the honest default and the prior aliasing of Roughness onto both
    // columns is what made the bakeable base_diffuse_roughness plane contradict the shade it lowered to.
    public double BaseDiffuseRoughness { get; init; }

    // AnisotropyRotation states the grain azimuth on the OpenPBR/`.mtlx` UNIT convention (1 is a half turn), gated with
    // the other weights; surface#OPENPBR_SLAB Lower converts it to radians once. A scalar rotation is the carrier a
    // Box mip preserves, where averaging two opposed tangent VECTORS cancels.
    public double AnisotropyRotation { get; init; }

    // ThinWalled is the OpenPBR geometry_thin_walled double-sided-shell flag — a SET-LEVEL boolean the foliage,
    // paper, and drapery-fabric rows set, not a per-texel field, so it rides this row and the wire's GeometryThinWalled
    // column while the Raster/set#TEXTURE_SET roster correctly excludes it. False is the OpenPBR default: a closed
    // solid whose transmission interface refracts; true renders both faces as one infinitesimally thin sheet.
    public bool ThinWalled { get; init; }

    // EmissionProvenance witnesses an ADMITTED emission magnitude — typed absence, because a row that authored
    // EmissionLuminance directly and a row whose value crossed photometric#PHOTOMETRIC Admit are different facts and
    // an empty receipt would forge the second. Photometric.WithEmission is the only writer. It carries the WHOLE
    // EmissionInput payload rather than its UnitEvidence half alone: the chromaticity, CCT+Duv, relative-luminance,
    // and gamut-map columns are the measurement the admission TOOK, and a later weathering or finish trajectory
    // rewrites Emission/EmissionLuminance while leaving what was admitted standing — so the receipt is the snapshot
    // and the two colour columns are the current shade. Truncating it to the unit witness stranded that evidence at
    // the resolve, where the acquisition#ACQUISITION sibling receipt reaches interchange#MATERIAL_WIRE whole.
    public Option<EmissionInput> EmissionProvenance { get; init; }

    // Every unit-interval column gates — a SheenTint of 7 or an AnisotropyRotation of -2 is as unrepresentable as a
    // Metalness of 3; EmissionLuminance is the one open non-negative scale, its magnitude bounded by physics and its
    // UNIT proven at photometric#PHOTOMETRIC MaterialUnits.Admit rather than asserted here.
    public static Fin<MaterialParameters> Of(MaterialParameters candidate, Op key) =>
        from _ in guard(InUnit(candidate.Metalness) && InUnit(candidate.Roughness) && InUnit(candidate.SpecularTint) && InUnit(candidate.Anisotropy)
                && InUnit(candidate.AnisotropyRotation) && InUnit(candidate.BaseDiffuseRoughness)
                && InUnit(candidate.Transmission) && InUnit(candidate.TransmissionRoughness) && InUnit(candidate.Sheen) && InUnit(candidate.SheenTint)
                && InUnit(candidate.Clearcoat) && InUnit(candidate.ClearcoatRoughness) && InUnit(candidate.Subsurface), MaterialFault.Parameter(key, "<weight-out-of-unit>"))
        from __ in guard(InIorRange(candidate.Ior), MaterialFault.Parameter(key, $"<ior-out-of-range:{candidate.Ior}>"))
        from ___ in guard(double.IsFinite(candidate.EmissionLuminance) && candidate.EmissionLuminance >= 0.0, MaterialFault.Parameter(key, $"<emission-luminance-negative:{candidate.EmissionLuminance:R}>"))
        from ____ in guard(candidate.Colours.ForAll(GamutPolicy.Perceptual.Contains), MaterialFault.Gamut(key, "<row-color-out-of-gamut>"))
        select candidate;

    static bool InUnit(double v) => double.IsFinite(v) && v is >= 0.0 and <= 1.0;
    // Ior is the DIELECTRIC interface index at every row, so its band is TOTAL and reads no second column. Its
    // metalness-keyed predecessor admitted a conductor's eta into this column on a fully-metallic row, and its
    // step at metalness == 1.0 then refused the row the instant a weathering trajectory de-metalized it — the copper
    // patina and iron oxidation campaigns both failed at every non-zero age, on values the shading path never reads
    // for a metal anyway: surface#OPENPBR_SLAB SlabStack.LowerBase grounds the conductor lobe from the named
    // ConductorMetal row's measured (eta, k) bands and reads SpecularIor on the dielectric arms alone.
    static bool InIorRange(double ior) => double.IsFinite(ior) && ior is >= 1.0 and <= 2.5;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialLibrary {
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
    static readonly Unicolour Black = PortValue.Black;   // the ONE scene-linear zero, read off its owner rather than re-minted per table
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

    // Every conductor row carries the OpenPBR DIELECTRIC specular_ior default in the Ior column. Its measured
    // complex index is the surface#CONDUCTOR_IOR ConductorMetal row's (eta, k) bands, which is what
    // SlabStack.LowerBase grounds the Conductor lobe from; the Ior column reaches shading on the dielectric arms
    // alone and crosses as the interchange#MATERIAL_WIRE specular_ior port a peer reads as an interface index.
    // Transcribing a metal's eta-red here shipped 0.470 for gold and 3.000 for chrome onto that port, forked the
    // grounding against the ConductorMetal bands beside it, and drove every weathered row out of the admitted band
    // as soon as metalness fell below one.
    public static readonly FrozenDictionary<MaterialId, MaterialParameters> Rows = new (MaterialId Id, MaterialParameters Row)[] {
        (MaterialId.Of("metal.gold"),      new(Linear(1.000, 0.766, 0.336), 1.0, 0.12, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.copper"),    new(Linear(0.955, 0.638, 0.538), 1.0, 0.18, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.aluminum"),  new(Linear(0.913, 0.922, 0.924), 1.0, 0.08, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.silver"),    new(Linear(0.972, 0.960, 0.915), 1.0, 0.05, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.iron"),      new(Linear(0.560, 0.570, 0.580), 1.0, 0.35, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.steel"),     new(Linear(0.560, 0.570, 0.577), 1.0, 0.40, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.titanium"),  new(Linear(0.542, 0.497, 0.449), 1.0, 0.28, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.chrome"),    new(Linear(0.550, 0.556, 0.554), 1.0, 0.02, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.brass"),     new(Linear(0.887, 0.789, 0.434), 1.0, 0.22, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
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
        (MaterialId.Of("fabric.silk"),     new(Linear(0.700, 0.620, 0.480), 0.0, 0.35, 0.2,  0.6,  1.460, 0.0, 0.0, 0.6, 0.3, 0.0, 0.0, 0.0, NoScatter, Black, 0.0) { ThinWalled = true }),
        (MaterialId.Of("fabric.denim"),    new(Linear(0.150, 0.230, 0.380), 0.0, 0.80, 0.0,  0.0,  1.460, 0.0, 0.0, 0.4, 0.5, 0.0, 0.0, 0.0, NoScatter, Black, 0.0) { ThinWalled = true }),
        // The two SHELL rows the geometry_thin_walled flag exists for: a single-surface sheet whose two
        // interfaces sit one wall apart, transmitting without refracting and lit from behind. A leaf carries
        // its waxy cuticle as the coat and its mesophyll as the scatter; a sheet carries its fibre bulk as the
        // scatter alone. Both drive the surface#OPENPBR_SLAB unit-index transmissive arm.
        (MaterialId.Of("foliage.leaf"),    new(Linear(0.090, 0.220, 0.060), 0.0, 0.55, 0.0,  0.0,  1.420, 0.35, 0.60, 0.0, 0.0, 0.15, 0.25, 0.4, Scatter(2.0, 3.5, 1.5), Black, 0.0) { ThinWalled = true }),
        (MaterialId.Of("paper.sheet"),     new(Linear(0.780, 0.770, 0.740), 0.0, 0.75, 0.0,  0.0,  1.500, 0.30, 0.85, 0.0, 0.0, 0.0, 0.0, 0.25, Scatter(3.0, 3.0, 3.0), Black, 0.0) { ThinWalled = true }),
        (MaterialId.Of("paint.car-metallic"), new(Linear(0.090, 0.020, 0.220), 0.85, 0.30, 0.0, 0.0, 1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("paint.clearcoat"), new(Linear(0.700, 0.700, 0.700), 0.0, 0.40, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.03, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("ceramic.glazed"),  new(Linear(0.880, 0.850, 0.780), 0.0, 0.10, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.9, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("ceramic.porcelain"), new(Linear(0.930, 0.920, 0.900), 0.0, 0.20, 0.0, 0.0, 1.504, 0.0, 0.0, 0.0, 0.0, 0.3, 0.10, 0.4, Scatter(5.0, 5.0, 5.0), Black, 0.0)),
        (MaterialId.Of("wax.beeswax"),     new(Linear(0.870, 0.700, 0.330), 0.0, 0.55, 0.0,  0.0,  1.443, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.8, Scatter(6.0, 4.0, 1.5), Black, 0.0)),
        (MaterialId.Of("wax.candle"),      new(Linear(0.940, 0.920, 0.850), 0.0, 0.60, 0.0,  0.0,  1.430, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.9, Scatter(8.0, 6.0, 4.0), Black, 0.0)),
        (MaterialId.Of("stone.marble"),    new(Linear(0.870, 0.860, 0.840), 0.0, 0.30, 0.0,  0.0,  1.486, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.5, Scatter(2.19, 2.62, 3.00), Black, 0.0)),
        (MaterialId.Of("wood.oak"),        new(Linear(0.430, 0.270, 0.140), 0.0, 0.55, 0.3,  0.4,  1.530, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("coat.gold-leaf"),  new(Linear(1.000, 0.766, 0.336), 1.0, 0.06, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.02, 0.0, NoScatter, Black, 0.0)),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);   // seam MaterialId's generated equality (ordinal-ignore-case) keys the row table

    // Type-initialization admission — the ModelRegistry static-ctor pattern: every catalog row runs the SAME
    // MaterialParameters.Of gate an ad-hoc vector takes, so a transposed column or an out-of-unit weight fails the
    // load with its row named rather than shading a wrong material behind a frozen table the gate never saw.
    static MaterialLibrary() {
        string? breach = Rows
            .Select(static entry => MaterialParameters.Of(entry.Value, Op.Of(name: "material-library-admit")).Match(
                Succ: static _ => (string?)null,
                Fail: fault => $"<library-row-inadmissible:{entry.Key.Value}:{fault.Message}>"))
            .FirstOrDefault(static reason => reason is not null);
        if (breach is not null) { throw new InvalidOperationException(breach); }
    }

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

    // The two domain gates carry the fault rail, the domain reason, and — on the spectral side — the imaginary
    // pre-test the optimal-limit predicate cannot distinguish; the RECOVERY beside each is the kernel GamutPolicy
    // row's own Bound, so this owner adds a rail and never a projection rename. A consumer needing the nearest
    // reproducible colour reads GamutPolicy.Pointer.Bound or GamutPolicy.MacAdam.Bound directly.
    public static Fin<Unicolour> PointerAdmit(Unicolour reflectance, Op key) =>
        GamutPolicy.Pointer.Contains(reflectance)
            ? Fin.Succ(reflectance)
            : MaterialFault.Gamut(key, $"<pointer-unreproducible-reflectance:{reflectance.Hex}>");

    public static Fin<Unicolour> SpectralAdmit(Unicolour reflectance, Op key) =>
        reflectance.IsImaginary
            ? MaterialFault.Gamut(key, $"<imaginary-reflectance:{reflectance.Hex}>")
            : GamutPolicy.MacAdam.Contains(reflectance)
                ? Fin.Succ(reflectance)
                : MaterialFault.Gamut(key, $"<macadam-unreproducible-reflectance:{reflectance.Hex}>");

    // WCAG contrast as a typed threshold receipt — the ratio plus the 4.5/3.0/7.0 verdicts
    // (AA text · AA large/UI · AAA text) the color-specification seam reads; never a bare package-call rename.
    public static (double Ratio, bool AaText, bool AaLarge, bool AaaText) Contrast(Unicolour foreground, Unicolour background) =>
        foreground.Contrast(background) switch { var ratio => (ratio, ratio >= 4.5, ratio >= 3.0, ratio >= 7.0) };

    // NearestIscc projects the ISCC-NBS designation: the nearest of the 267 centroids by Ciede2000 — the standardized colour NAME a finish
    // schedule or specification prints, read-only beside Contrast, never a stored row column.
    public static (string Name, Unicolour Centroid, double DeltaE) NearestIscc(Unicolour candidate) =>
        toSeq(IsccCentroids)
            .Map(row => (Name: row.Key, Centroid: row.Value, DeltaE: candidate.Difference(row.Value, DeltaE.Ciede2000)))
            .Fold((Name: "<unnamed>", Centroid: candidate, DeltaE: double.MaxValue), static (best, row) => row.DeltaE < best.DeltaE ? row : best);
}
```

## [04]-[RESEARCH]

- [MEASURED_SPECTRAL_ROWS]-[OPEN]: which measured spectral source grounds the conductor and dielectric `MaterialLibrary.Rows` base colours, so a row's scene-linear triple is an integrated reflectance rather than a transcribed RGB literal; route the candidate spectra through `surface#SPECTRAL_UPSAMPLE` and compare each resulting `BaseColor` against the current row by `Difference(row, DeltaE.Ciede2000)` under `PortValue.SceneLinear`.
- [SHADE_CHANNEL_DELEGATE_ORDER]-[OPEN]: does the `[SmartEnum<string>]` generator emit a two-delegate constructor in partial-method DECLARATION order (`Port` then `Route`), or in another order the row constructions here would silently transpose; route `uv run python -m tools.assay api --key thinktecture-runtime-extensions` over the generated `ShadeChannel` constructor after the first build, since `libs/csharp/.api/api-thinktecture-runtime-extensions.md` `[03]-[ENTRYPOINTS]` states only that each `[UseDelegateFromConstructor]` delegate lands LAST.
