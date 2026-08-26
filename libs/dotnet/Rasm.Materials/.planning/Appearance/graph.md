# [MATERIALS_GRAPH]

THE NODE-GRAPH APPEARANCE ENGINE and THE POLYMORPHIC MATERIAL LIBRARY. One `AppearanceNode` `[Union]` closes the node-kind family — `Input`, `Texture`, `Math`, `Mix`, `Normal`, `BsdfOutput` — over the typed `PortValue` channel set; one `MaterialGraph.Compile` fold orders the node DAG ONCE on the shared QuikGraph substrate (`IsDirectedAcyclicGraph` gate, `SourceFirstTopologicalSort` order), resolves its sink and its two rentals, and freezes the whole result, and `CompiledGraph.ShadeSpan` re-enters that frozen order over a caller-rented index-addressed scratch whose slot order IS the sort — the per-sample `CompiledGraph.Shade` being that SAME path over a one-element window, so ONE representation, ONE `NodeEvaluator.Apply` algebra over pre-resolved operands, and ONE `Assemble` gamut gate carry both the integrator and the bake — the path's own buffers allocate nothing and the per-node immutable-map rebuild the per-point path once paid is gone from the hot path, while each node production still mints its `PortValue` and its `Fin` typed envelope per texel, the stated bound the class-shaped union carries — and the node algebra is POINTWISE by construction with every neighbourhood kernel owned by `Raster/filter#PLANE_OP` over a whole plane. One `MaterialParameters` record is the canonical Disney-principled parameter vector — a closed positional core beside a widening band of init-defaulted enrichment columns (the OpenPBR `thin_film` carrier, the three tint colours, the diffuse-roughness and anisotropy-rotation axes, the emission unit evidence) — every measured material parameterizes, and one `MaterialLibrary` `FrozenDictionary<MaterialId, MaterialParameters>` is the catalog as DATA ROWS, so a new material is `MaterialLibrary.Rows[MaterialId.Create("metal.titanium")] = new MaterialParameters(...)`, a row of values, NEVER a `TitaniumMaterial` type. `Rasm.Materials.Appearance.Graph` OWNS the `PortId`/`MathOp`/`MixOp`/`PortValue` graph vocabulary (the `MixOp` table one `BlendMode` row per W3C compositing member, the blend behavior a DATA column, never sixteen delegates), the `GraphEdit`/`ShadeChannel` authoring vocabulary the one `MaterialGraph.Author` fold consumes (the producer that MINTS the `Math` and `Mix` kinds the union declares, the arity gate proves, the wire projects, and the WGSL lowering opcodes — its product an ordinary `MaterialGraph`, so authoring and evaluation share one representation and one admission predicate), the `ShadePoint`/`AppearanceNode`/`SurfaceShade`/`PortSlot`/`CompiledGraph`/`MaterialGraph` evaluation surfaces, the `SubsurfaceRadius` mean-free-path and `ThinFilm` interference carriers, the `MaterialParameters` row, and the `MaterialLibrary` catalog/admission/reference folds; it COMPOSES the CONTRACT `Rasm.Element` `MaterialId` identity (never re-minting a `family.name` key), the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 channel (never a second fault), the Rasm.Numerics `Direction`/`VectorFrame`/`Context` shading frame (never re-minting a vector or a tolerance), the `texture#TEXTURE_UV` `TextureUv.Port` closure and `UvSample.Parameter` driven lane for the `Texture` arm (never re-implementing sampling), QuikGraph as the one graph-algorithm substrate the whole stack folds transient graphs onto (never a hand-rolled Kahn walk), and Wacton.Unicolour directly as the scene-linear/spectral/compositing color owner under the one `Acescg` working space (never re-minting a `ColourSpace`). `SurfaceShade` terminates the graph as the resolved parameter snapshot the `surface#OPENPBR_SLAB` `SlabStack.ToLayered` lowers to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` the integrator shades — the graph resolves the parameters, the lobe math living on the `bsdf`/`surface` pages, never re-derived here. `MaterialId` generalizes the masonry-assignment consumer: a masonry `Component` maps to a `MaterialId`, never to a component-specific material type.

## [01]-[INDEX]

- [02]-[MATERIAL_GRAPH]: `PortId`/`MathOp`/`MixOp` carry the graph vocabulary (`MixOp` the 16-row `BlendMode` table), `PortValue` the channel set, `AppearanceNode` the node union over its `Produces` column, `GraphEdit`/`ShadeChannel` the authoring request family and sink-port vocabulary `MaterialGraph.Author` folds, `ShadePoint`/`PortSlot`/`CompiledGraph`/`MaterialGraph` the QuikGraph-ordered evaluation fold over the one slot-addressed `ShadeSpan` path its per-point `Shade` window re-enters, and `SurfaceShade` the sink.
- [03]-[MATERIAL_LIBRARY]: `MaterialLibrary` catalogs `MaterialParameters` rows under the contract `MaterialId` key over the `SubsurfaceRadius` mean-free-path and `ThinFilm` interference carriers, generalizes profile assignment, and gates through the `NearestChecker`/`HueConstant`/`Named` Datasets validation boundary over the reflection-derived reference tables, the three reproducibility gates by domain — the AP1 working-space bound (`SurfaceShade.InGamut`), Pointer real-surface (`PointerAdmit`), and MacAdam spectral-limit (`SpectralAdmit`) — each enforcing the kernel `GamutPolicy` row's own containment, with the `Contrast`/`NearestIscc` accessibility and designation projections.

## [02]-[MATERIAL_GRAPH]

- Owner: `MaterialGraph`/`CompiledGraph` over `AppearanceNode`; the `PortId`/`MathOp`/`MixOp`/`PortValue` graph vocabulary; the `GraphEdit` authoring request union and the `ShadeChannel` sink-port roster carrying each channel's read and re-seat; the `ShadePoint`/`SurfaceShade`/`PortSlot` evaluation models.
- Cases: `Input` (constant/parameter source) · `Texture` (UV-sampled source — the `texture#TEXTURE_UV` `TextureUv.Port` closure) · `Math` (closed scalar/vector op over upstream ports) · `Mix` (parameterized `BlendMode` composite of two ports) · `Normal` (tangent-space perturbation of the shading frame) · `BsdfOutput` (the single sink assembling the closed lobe set into a `SurfaceShade`); authoring edit {`Node` (a node at a FREE port), `Seat` (a replacement at a TAKEN port), `Route` (a `ShadeChannel` re-seat onto an existing port)}; shade channel {base-color, metalness, roughness, normal-frame, emission}.
- Entry: `public Fin<Unit> ShadeSpan(ReadOnlySpan<ShadePoint> points, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> shades, Op key)` is the ONE evaluation path — `Raster/press#TEXTURE_PRESS` drives it per band, and the per-point `public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> window, Op key)` the integrator holds re-enters it over a one-element window, so no second environment representation exists to drift from and the path's own buffers allocate nothing — a per-point entry renting its own scratch and its own window prices two heap arrays per ray to spare a caller two arguments; the per-node `PortValue`/`Fin` productions remain the union's heap cost, deleted only by a value-shaped union this page does not mint — with `ScratchWidth` and `OperandWidth` the two `Compile`-resolved rentals a caller sizes against; `public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key)` is the ONE-SHOT convenience (Compile + Shade for a single sample), while the per-sample path `Compile`s ONCE into a frozen `CompiledGraph` and re-enters it per sample, so the hot loop pays the sort once per material, never per ray. `Fin<T>` aborts at COMPILE on a cyclic DAG (`MaterialFault.Graph`, key-correlated), a duplicate node id, a dangling port reference, a dependency on a non-producing port, or a missing/non-`BsdfOutput` sink, and at SHADE on a short span rental, a degenerate frame perturbation, or an out-of-gamut assembled shade — each shade-time failure re-wrapped with the failing TEXEL INDEX, since a plane fails at one of sixteen million points that all ran the same program (a port-TYPE mismatch cannot fault at all — the `PortValue.AsScalar`/`AsColor`/`AsVector` projections are total by construction); `MaterialGraph.Default` is the canonical Disney-principled wiring every library row drives through; `public Fin<MaterialGraph> Author(Seq<GraphEdit> edits, Op key)` is the ONE producer entry a caller composes a layered or masked appearance through — folding the closed `GraphEdit` request family over `Default` (or any compiled-clean graph) with the node `Admit` predicate and the sink re-seat proofs run at ADMISSION so the product is compile-clean by construction, `public Seq<PortId> Ports(int count)` the fresh id block a session names new wiring with, and `public Fin<PortId> PortOf(ShadeChannel channel, Op key)` the read that tells a composer where a channel is ALREADY wired, so lowering onto a standing graph transcribes no port integer.
- Packages: QuikGraph (composed — `AdjacencyGraph<PortId, SEdge<PortId>>` with `allowParallelEdges: false`, `AddVertexRange` admitting isolates, `AddVerticesAndEdge` per dependency edge, `AlgorithmExtensions.IsDirectedAcyclicGraph` the cheap cycle pre-gate, `AlgorithmExtensions.SourceFirstTopologicalSort` the Kahn order — the one graph-algorithm substrate `Rasm.Element`/`Rasm.Persistence`/`Rasm.Bim` already fold onto, admitted folder-locally against the central pin), Rasm (project — `Direction`/`VectorFrame`/`Context`/`Op`, `Rhino.Geometry.Point3d`/`Vector3d`/`Plane` at the host edge), Rasm.Element (the CONTRACT `MaterialId`, composed not re-declared), Rasm.Materials.Appearance.Bsdf (the `MaterialFault` band-2450 channel composed from `bsdf#SHADING_FRAME`), Wacton.Unicolour (color/spectral/compositing compose — `Mix`, `Blend(backdrop, BlendMode)`, the 16-member `BlendMode` vocabulary), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`; `System.Buffers.ArrayPool<PortValue>.Shared` for the one operand buffer a `Span` cannot carry through the generated `Switch` state, rented per fold and returned cleared). `texture#TEXTURE_UV` `TextureUv.Port` mints the `Texture` arm's closure — `texture` COMPOSES this page's `PortValue`/`PortId`/`ShadePoint`, so graph stays the LOWER owner and the `Texture` case carries only a host-free `Func<double,double,PortValue>`, never a `texture`-namespace type (no cyclic namespace dependency).
- Growth: a new appearance operation is one `MathOp` row (the operation behavior rides the SmartEnum row's `[UseDelegateFromConstructor]` delegate — the roster spans arithmetic incl. floored `modulo`, the unary transcendental and rounding family, min/max, the vector ops, unit clamps, the Schlick weight, the `smoothstep`/`remap`/`range`/`contrast` signal curves, and the `if-greater`/`if-equal`/`switch` conditionals, each keyed to its MaterialX standard math category and each admitting its own operand count through its `Accepts` predicate column) or one `MixOp` row naming its `BlendMode` member (the blend behavior IS the `Mode` data column the ONE `Apply` derivation reads — never a new arm, never a hand-rolled channel composite); a genuinely new node KIND with no parameterization of the six is one `AppearanceNode` case; a new port channel is one `PortValue` case carrying its CLR carrier; a new lobe assembled at the sink is one `BsdfLobe` `[Union]` case on the `bsdf` page — never a per-effect graph variant and never a sibling node type. A new AUTHORING move is one `GraphEdit` case the generated `Switch` forces every fold site to route, and a sixth sink port is one `ShadeChannel` row carrying its read and its re-seat — never a per-op `Multiply`/`Screen`/`Lerp` factory family re-spelling the `MathOp`/`MixOp` rows the declaration already closes, and never a per-channel `RouteBaseColor`/`RouteEmission` entrypoint. `interchange#MATERIALX_DOCUMENT` projects its `NodeCategory`/`MtlxPort` map onto the `AppearanceNode` union and `PortValue` set, the MaterialX 1.39 node-graph alignment target.
- Boundary: the node DAG is the only appearance-program shape — a per-material hand-written shade function is the deleted form; `PortValue` is the only inter-node channel and carries scalar/`Unicolour`/`Vector3d`/`VectorFrame` polarities so a node arm reads typed ports and never `object`; the `Color`→`Scalar` projection is the AP1 scene-linear luminance dot the `bsdf#LOBE_FAMILY` owner derives from the working space's own chromaticities (the AP1-primary luminance row consistent with the declared `Acescg` working space and the `bsdf#LOBE_FAMILY` `RgbSpectrum.Luminance` weights — a Rec709 weight on AP1-linear channels is the colorimetric defect, biasing a green-heavy mask), never a red-channel read, so a mask pulled from a color is photometrically weighted and cannot silently bias to red; the `Texture` arm carries the TOTAL `Func<double,double,Option<double>,PortValue>` closure the `texture#TEXTURE_UV` `TextureUv.Port(TextureSource, UvSample, SamplerState, Channel, Op)` mints and an `Option<PortId> Parameter` naming the upstream port that DRIVES it, so a field-parameterized ramp is one wired dependency rather than a source case — the node holds the delegate, the sampling fold lives on the `texture` page, and the arm never re-implements a sampler nor admits a raw caller-supplied lambda that bypasses the `Channel`-neutralized fault path; the `Normal` arm perturbs the composed Rasm.Numerics `VectorFrame` (tangent·bitangent·normal) and never re-mints a basis; the `BsdfOutput` arm assembles the `SurfaceShade` parameter snapshot (resolved base color, metalness, roughness, perturbed shading frame, emission) the renderer reads after `Assemble` probes ALL FIVE sink ports against the `ShadeChannel` row's own `PortKind` column and admits its weights through the SAME `MaterialParameters.InUnit` predicate a row admission takes — the lobe WEIGHTING is the downstream `surface#OPENPBR_SLAB` `SlabStack.ToLayered` lowering of the `MaterialParameters` row to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` the integrator shades, the graph sink being the resolved parameter shade and the lobe math living wholly on the `bsdf`/`surface` pages, never re-derived here, color resolved through the directly-consumed Wacton.Unicolour `RgbConfiguration.Acescg` scene-linear owner; the `BsdfOutput` sink resolves through `Assemble` behind a pattern-matched sink probe (a non-`BsdfOutput` sink fails `MaterialFault.Graph`, never an unchecked cast), never a port write, so the environment carries no dead entry under the sink id and a downstream node cannot read a phantom `Scalar(1.0)`; the `Math` arm folds over its `MathOp` SmartEnum by delegate row so a new operation is a row, never a new arm, and the `MathOp.Fresnel` row supplies only the Schlick angular weight `(1−cosθ)⁵` for a `Mix` lobe blend — the full Fresnel term lives on `bsdf#MICROFACET_KERNEL`, never re-derived here; the `Mix` arm dispatches `b.AsColor.Blend(a.AsColor, Mode)` — the W3C separable/non-separable compositing algebra Unicolour owns, `a` the backdrop, `b` the source, the factor the blend opacity lerped in scene-linear `RgbLinear` — so all sixteen W3C modes are one data column and the prior three-mode hand-rolled `ChannelCompose` channel math is the deleted form; the `Lerp` row IS `BlendMode.Normal` spelled as the HDR-safe scene-linear `Unicolour.Mix` (the blend algebra clips to the `[0,1]` W3C reflectance domain; an over-unity INTERMEDIATE — a scaled mask, a `Math` product — keeps its `>1` channels through the linear arm, while a sink-bound emission port is NORMALIZED chromaticity by construction, `MaterialParameters.EmissionLuminance` carrying the energy, so the `Assemble` `InGamut` gate holds); the node algebra is POINTWISE by construction and the `AppearanceNode` union admits no neighbourhood operation — a blur, a normal-from-height integration, an ambient-occlusion sweep, or any other kernel reading a texel's neighbours lives at `Raster/filter#PLANE_OP` over a whole `Raster/plane#TEXTURE_PLANE`, because a DAG node evaluated per shading point has no neighbours to read, so a node kind pretending otherwise either fabricates them or forces every sample to carry a plane; the press bakes the DAG's pointwise field first and folds the plane algebra AFTER, so the two owners compose in one direction and neither re-implements the other; `Compile` folds the DAG onto the QuikGraph substrate ONCE — `AddVertexRange` admits every node so an isolate still orders, `AddVerticesAndEdge` adds one dependency→dependent `SEdge<PortId>` per KNOWN dependency (`allowParallelEdges: false` deduplicating an operand list naming one port twice), and one `ANSWERABILITY` sweep failing `MaterialFault.Graph` at COMPILE over the two failures a slot-addressed read cannot distinguish at runtime — a port no node declares (`<dangling-port>`) and a port whose node `Produces` nothing (`<non-producing-port>`, the sink a dependent named) — because both read an UNWRITTEN scratch cell carrying the previous texel's value rather than faulting, where the per-point map read once failed cleanly, so the proof is what keeps one path total and a per-texel liveness check is exactly the cost the frozen order exists to delete; `IsDirectedAcyclicGraph` pre-gates a cycle onto `MaterialFault.Graph` before `SourceFirstTopologicalSort` throws `NonAcyclicGraphException`, and the sink resolves to its `BsdfOutput` at COMPILE so no path re-probes a cast per sample; `ShadeSpan` then re-enters the frozen order against a caller-rented `Span<PortValue>` scratch and `Shade` re-enters `ShadeSpan` over a one-element window, so ONE `NodeEvaluator.Apply` algebra over pre-resolved operands and ONE `Assemble` reading that scratch DIRECTLY close every evaluation at the same gamut gate — a `Func<PortId, Fin<PortValue>>` port reader cannot exist here at all, since a lambda may not capture a `Span<T>`, and a second environment shape minted to dodge that is the divergence this collapse forecloses; the prior hand-rolled indegree/`Queue`/`CollectionsMarshal` Kahn kernel is DELETED for the substrate's own catalogued `AdjacencyGraph` construction API, and the page's ONE `[EXPRESSION_SPINE]` exemption is the `ShadeSpan` span kernel — a fixed-extent index walk over caller-owned buffers, the doctrine's named span-loop carve — while every admission, dispatch, and egress surface on the page is expression-bodied; `Context.Canonical` is the one tolerant `Context` the `Normal`/`ShadePoint` arms construct the `VectorFrame` through (a millimetre-scale model `Context` whose `Fin` admission the page resolves once, so a near-degenerate perturbation re-seeds a perpendicular tangent through the `Rasm.Numerics` owner rather than faulting mid-shade); `MaterialGraph.Default` carries the geometric frame unperturbed through one `Normal` node at `Strength 0` whose identity tangent-space sample `(0.5,0.5,1.0)` decodes to `+Z`, so a library row is parameters evaluated through this one standard graph, never a per-row graph type; a cycle, a dangling port, a duplicate node id, or a non-`BsdfOutput` sink returns `Fin.Fail` and never propagates a NaN shade outward; AUTHORING is the same algebra read backwards and shares its proof — `MaterialGraph.Author` folds the closed `GraphEdit` family through the SAME `Admit` predicate `Compile` runs, the only difference being the KNOWN-SET each hands it (`Compile` the whole node map, so a dependency declared later is legal; `Author` the nodes admitted so far, so an unresolved dependency IS a forward reference an incremental fold cannot have), and a second copy of the arity or answerability sweep beside it is the fork this sharing forecloses; the authoring product is an ordinary `MaterialGraph` that `Compile`s, `ShadeSpan`s, and lowers to WGSL through the one frozen-order path, so no authored-graph representation, builder type, or mutable node bag exists to diverge from the evaluated one; `Author` mints no `BsdfOutput` — the graph terminates ONCE and a caller layers onto `Default`'s sink through `Route`, so a second terminal is unrepresentable rather than resolved by an id compare at `Compile`; and a caller never hand-types a port integer, because `Ports` allocates above every authored id and `PortOf` reads whatever the sink already wires, which is what makes a mask blended against the standing base colour a two-edit sequence instead of a transcription of `Default`'s own wiring. LOWERING onto `Default` is `Seat`, never `Node` plus `Route`: a composer that owns the whole channel — the `Raster/set#SET_BIND` `Program` arm binding a texture set is the standing one — replaces the default's Input at that channel's OWN port, because authoring the covered channel at a fresh port instead leaves the default node orphaned in the compiled order, and an isolate the sort still admits pays a `PortValue` production per texel for a scratch cell nothing reads. `Seat` therefore refuses an absent port and a `Produces` flip, so a lowering carries `Default`'s topology BY CONSTRUCTION rather than by a second hand-wiring that must be re-checked against it every time either side widens.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers;
using System.Collections.Frozen;
using System.Linq;
using System.Reflection;
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Photometric;
using Rasm.Numerics;
using Rhino;
using Rhino.Geometry;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Graph;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<int>]
public readonly partial struct PortId { }

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MathOp {
    // --- [ARITHMETIC]
    public static readonly MathOp Add = new("add", Fixed(2), static (o, _) => Zip(o[0], o[1], static (a, b) => a + b));
    public static readonly MathOp Subtract = new("subtract", Fixed(2), static (o, _) => Zip(o[0], o[1], static (a, b) => a - b));
    public static readonly MathOp Multiply = new("multiply", Fixed(2), static (o, _) => Zip(o[0], o[1], static (a, b) => a * b));
    public static readonly MathOp Divide = new("divide", Fixed(2), static (o, _) => Zip(o[0], o[1], static (a, b) => b is 0.0 ? 0.0 : a / b));
    public static readonly MathOp Modulo = new("modulo", Fixed(2), static (o, _) => Zip(o[0], o[1], static (a, b) => b is 0.0 ? 0.0 : a - (b * System.Math.Floor(a / b))));
    public static readonly MathOp Power = new("power", Fixed(2), static (o, _) => Zip(o[0], o[1], System.Math.Pow));
    public static readonly MathOp Scale = new("scale", Fixed(2), static (o, _) => new PortValue.Vector(o[0].AsVector * o[1].AsScalar));
    public static readonly MathOp Min = new("min", Fixed(2), static (o, _) => Zip(o[0], o[1], System.Math.Min));
    public static readonly MathOp Max = new("max", Fixed(2), static (o, _) => Zip(o[0], o[1], System.Math.Max));
    public static readonly MathOp Atan2 = new("atan2", Fixed(2), static (o, _) => Zip(o[0], o[1], System.Math.Atan2));

    // --- [UNARY]
    public static readonly MathOp Sqrt = new("sqrt", Fixed(1), static (o, _) => Lift(o[0], static x => System.Math.Sqrt(System.Math.Max(0.0, x))));
    public static readonly MathOp Abs = new("abs", Fixed(1), static (o, _) => Lift(o[0], System.Math.Abs));
    public static readonly MathOp Sign = new("sign", Fixed(1), static (o, _) => Lift(o[0], static x => System.Math.Sign(x)));
    public static readonly MathOp Floor = new("floor", Fixed(1), static (o, _) => Lift(o[0], System.Math.Floor));
    public static readonly MathOp Ceil = new("ceil", Fixed(1), static (o, _) => Lift(o[0], System.Math.Ceiling));
    public static readonly MathOp Round = new("round", Fixed(1), static (o, _) => Lift(o[0], static x => System.Math.Round(x, MidpointRounding.AwayFromZero)));
    public static readonly MathOp Exp = new("exp", Fixed(1), static (o, _) => Lift(o[0], System.Math.Exp));
    public static readonly MathOp Ln = new("ln", Fixed(1), static (o, _) => Lift(o[0], static x => x > 0.0 ? System.Math.Log(x) : 0.0));
    public static readonly MathOp Sin = new("sin", Fixed(1), static (o, _) => Lift(o[0], System.Math.Sin));
    public static readonly MathOp Cos = new("cos", Fixed(1), static (o, _) => Lift(o[0], System.Math.Cos));
    public static readonly MathOp Clamp01 = new("clamp01", Fixed(1), static (o, _) => Lift(o[0], static x => System.Math.Clamp(x, 0.0, 1.0)));
    public static readonly MathOp OneMinus = new("one-minus", Fixed(1), static (o, _) => Lift(o[0], static x => 1.0 - x));

    // --- [VECTOR]
    public static readonly MathOp DotProduct = new("dot", Fixed(2), static (o, _) => new PortValue.Scalar(o[0].AsVector * o[1].AsVector));
    public static readonly MathOp CrossProduct = new("cross", Fixed(2), static (o, _) => new PortValue.Vector(Vector3d.CrossProduct(o[0].AsVector, o[1].AsVector)));
    public static readonly MathOp Normalize = new("normalize", Fixed(1), static (o, _) => new PortValue.Vector(o[0].AsVector is { Length: > 0.0 } v ? v / v.Length : o[0].AsVector));
    public static readonly MathOp Magnitude = new("magnitude", Fixed(1), static (o, _) => new PortValue.Scalar(o[0].AsVector.Length));
    public static readonly MathOp Distance = new("distance", Fixed(2), static (o, _) => new PortValue.Scalar((o[0].AsVector - o[1].AsVector).Length));
    public static readonly MathOp Fresnel = new("fresnel-weight", Fixed(2), static (o, _) => new PortValue.Scalar(NodeEvaluator.SchlickWeight(System.Math.Clamp(o[0].AsVector * o[1].AsVector, 0.0, 1.0))));

    // --- [REMAP]
    public static readonly MathOp Smoothstep = new("smoothstep", Fixed(3), static (o, _) =>
        new PortValue.Scalar(Ramp(o[0].AsScalar, o[1].AsScalar, o[2].AsScalar) switch { var t => t * t * (3.0 - (2.0 * t)) }));
    public static readonly MathOp Contrast = new("contrast", Fixed(3), static (o, _) =>
        new PortValue.Scalar(((o[0].AsScalar - o[2].AsScalar) * o[1].AsScalar) + o[2].AsScalar));
    public static readonly MathOp Remap = new("remap", Fixed(5), static (o, _) =>
        new PortValue.Scalar(Between(Span(o[0].AsScalar, o[1].AsScalar, o[2].AsScalar), o[3].AsScalar, o[4].AsScalar)));
    public static readonly MathOp Range = new("range", Fixed(6), static (o, _) =>
        new PortValue.Scalar(Between(
            System.Math.Pow(Ramp(o[0].AsScalar, o[1].AsScalar, o[2].AsScalar), System.Math.Max(1e-6, o[3].AsScalar)),
            o[4].AsScalar, o[5].AsScalar)));

    // --- [CONDITIONAL]
    public static readonly MathOp IfGreater = new("if-greater", Fixed(4), static (o, _) => o[0].AsScalar > o[1].AsScalar ? o[2] : o[3]);
    public static readonly MathOp IfEqual = new("if-equal", Fixed(4), static (o, _) => o[0].AsScalar == o[1].AsScalar ? o[2] : o[3]);

    public static readonly MathOp Pick = new("switch", static count => count >= 2, static (o, count) =>
        o[1 + System.Math.Clamp((int)System.Math.Floor(o[0].AsScalar), 0, count - 2)]);

    [UseDelegateFromConstructor]
    public partial bool Accepts(int operands);

    [UseDelegateFromConstructor]
    public partial PortValue Apply(PortValue[] operands, int count);

    static Func<int, bool> Fixed(int arity) => count => count == arity;

    static PortValue Zip(PortValue l, PortValue r, Func<double, double, double> fold) =>
        l is PortValue.Scalar ls && r is PortValue.Scalar rs
            ? new PortValue.Scalar(fold(ls.Value, rs.Value))
            : new PortValue.Vector(new Vector3d(
                  fold(l.AsVector.X, r.AsVector.X), fold(l.AsVector.Y, r.AsVector.Y), fold(l.AsVector.Z, r.AsVector.Z)));

    static PortValue Lift(PortValue v, Func<double, double> fold) =>
        v is PortValue.Scalar s
            ? new PortValue.Scalar(fold(s.Value))
            : new PortValue.Vector(new Vector3d(fold(v.AsVector.X), fold(v.AsVector.Y), fold(v.AsVector.Z)));

    static double Ramp(double x, double low, double high) => System.Math.Clamp(Span(x, low, high), 0.0, 1.0);
    static double Span(double x, double low, double high) => high - low is var d && d != 0.0 ? (x - low) / d : 0.0;
    static double Between(double t, double low, double high) => low + (t * (high - low));
}

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

    public bool LinearArm { get; }

    public PortValue Apply(PortValue a, PortValue b, double t) =>
        new PortValue.Color(LinearArm
            ? a.AsColor.Mix(b.AsColor, ColourSpace.RgbLinear, t, premultiplyAlpha: false)
            : a.AsColor.Mix(b.AsColor.Blend(a.AsColor, Mode), ColourSpace.RgbLinear, t, premultiplyAlpha: false));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PortKind {
    public static readonly PortKind Scalar = new("scalar");
    public static readonly PortKind Color = new("color");
    public static readonly PortKind Vector = new("vector");
    public static readonly PortKind Frame = new("frame");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PortValue {
    private PortValue() { }

    public sealed record Scalar(double Value) : PortValue;
    public sealed record Color(Unicolour Linear) : PortValue;
    public sealed record Vector(Vector3d Value) : PortValue;
    public sealed record Frame(VectorFrame Value) : PortValue;

    public PortKind Kind => Switch(
        scalar: static _ => PortKind.Scalar, color: static _ => PortKind.Color,
        vector: static _ => PortKind.Vector, frame: static _ => PortKind.Frame);

    public double AsScalar => Switch(scalar: static s => s.Value, color: static c => Luminance(c.Linear), vector: static v => v.Value.Length, frame: static _ => 1.0);

    static double Luminance(Unicolour c) =>
        RgbSpectrum.LuminanceWeights switch { var w => (w.R * c.RgbLinear.R) + (w.G * c.RgbLinear.G) + (w.B * c.RgbLinear.B) };
    public Vector3d AsVector => Switch(scalar: static s => new Vector3d(s.Value, s.Value, s.Value), color: static c => RgbLinearVector(c.Linear), vector: static v => v.Value, frame: static f => f.Value.ZAxis);
    public Unicolour AsColor => Switch(scalar: static s => GreyLinear(s.Value), color: static c => c.Linear, vector: static v => VectorLinear(v.Value), frame: static f => VectorLinear(f.Value.ZAxis));

    static Vector3d RgbLinearVector(Unicolour c) => new(c.RgbLinear.R, c.RgbLinear.G, c.RgbLinear.B);
    static Unicolour GreyLinear(double g) => new(SceneLinear, ColourSpace.RgbLinear, g, g, g);
    static Unicolour VectorLinear(Vector3d v) => new(SceneLinear, ColourSpace.RgbLinear, v.X, v.Y, v.Z);
    internal static readonly Configuration SceneLinear = RgbProfile.Acescg.Configuration;

    internal static readonly Configuration SceneLinearDegree10 =
        new(SceneLinear.Rgb, new XyzConfiguration(Illuminant.D65, Observer.Degree10, "<acescg-degree10>"),
            SceneLinear.Ybr, SceneLinear.Cam, SceneLinear.DynamicRange, SceneLinear.Icc);

    internal static readonly Unicolour Black = GreyLinear(0.0);
    internal static readonly Unicolour White = GreyLinear(1.0);
}

// --- [MODELS] --------------------------------------------------------------------------
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
    public sealed record Texture(PortId Id, Option<PortId> Parameter, Func<double, double, Option<double>, PortValue> Sample)
        : AppearanceNode(Id);
    public sealed record Math(PortId Id, MathOp Op, Seq<PortId> Operands) : AppearanceNode(Id);
    public sealed record Mix(PortId Id, MixOp Op, PortId A, PortId B, PortId Factor) : AppearanceNode(Id);
    public sealed record Normal(PortId Id, PortId Source, double Strength) : AppearanceNode(Id);
    public sealed record BsdfOutput(PortId Id, PortId BaseColor, PortId Metalness, PortId Roughness, PortId NormalFrame, PortId Emission) : AppearanceNode(Id);

    public Seq<PortId> Dependencies =>
        Switch(
            input: static _ => Seq<PortId>(),
            texture: static t => t.Parameter.Map(static p => Seq(p)).IfNone(Seq<PortId>()),
            math: static m => m.Operands,
            mix: static x => Seq(x.A, x.B, x.Factor),
            normal: static n => Seq(n.Source),
            bsdfOutput: static o => Seq(o.BaseColor, o.Metalness, o.Roughness, o.NormalFrame, o.Emission));

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
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShadeChannel {
    public static readonly ShadeChannel BaseColor = new("base-color", PortKind.Color, static s => s.BaseColor, static (s, p) => s with { BaseColor = p });
    public static readonly ShadeChannel Metalness = new("metalness", PortKind.Scalar, static s => s.Metalness, static (s, p) => s with { Metalness = p });
    public static readonly ShadeChannel Roughness = new("roughness", PortKind.Scalar, static s => s.Roughness, static (s, p) => s with { Roughness = p });
    public static readonly ShadeChannel NormalFrame = new("normal-frame", PortKind.Frame, static s => s.NormalFrame, static (s, p) => s with { NormalFrame = p });
    public static readonly ShadeChannel Emission = new("emission", PortKind.Color, static s => s.Emission, static (s, p) => s with { Emission = p });

    public PortKind Kind { get; }

    [UseDelegateFromConstructor]
    public partial PortId Port(AppearanceNode.BsdfOutput sink);

    [UseDelegateFromConstructor]
    public partial AppearanceNode.BsdfOutput Route(AppearanceNode.BsdfOutput sink, PortId port);
}

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public readonly record struct PortSlot(bool Produced, PortValue Value) {
    internal static readonly PortValue ZeroScalar = new PortValue.Scalar(0.0);
    public static readonly PortSlot Sink = new(false, ZeroScalar);
    public static PortSlot Of(PortValue value) => new(true, value);
}

public static class NodeEvaluator {
    public static Fin<PortSlot> Apply(AppearanceNode node, ShadePoint point, MaterialParameters parameters, PortValue[] operands, Op key) =>
        node.Switch(
            state:      (Point: point, Parameters: parameters, Operands: operands, Key: key),
            input:      static (s, i) => Fin.Succ(PortSlot.Of(i.Pull(s.Parameters))),
            texture:    static (s, t) => Fin.Succ(PortSlot.Of(t.Sample(s.Point.U, s.Point.V,
                            t.Parameter.Map(_ => s.Operands[0].AsScalar)))),
            math:       static (s, m) => Fin.Succ(PortSlot.Of(m.Op.Apply(s.Operands, m.Operands.Count))),
            mix:        static (s, x) => Fin.Succ(PortSlot.Of(x.Op.Apply(s.Operands[0], s.Operands[1], System.Math.Clamp(s.Operands[2].AsScalar, 0.0, 1.0)))),
            normal:     static (s, n) => Perturb(n, s.Point, s.Operands[0], s.Key).Map(PortSlot.Of),
            bsdfOutput: static (_, _) => Fin.Succ(PortSlot.Sink));

    internal static double SchlickWeight(double cosTheta) { double m = System.Math.Clamp(1.0 - cosTheta, 0.0, 1.0); double m2 = m * m; return m2 * m2 * m; }

    static Fin<PortValue> Perturb(AppearanceNode.Normal n, ShadePoint point, PortValue sample, Op key) =>
        from raw in Fin.Succ(sample.AsVector)
        let tangentSpace = new Vector3d((2.0 * raw.X - 1.0) * n.Strength, (2.0 * raw.Y - 1.0) * n.Strength, 2.0 * raw.Z - 1.0)
        let basis = point.Frame.Value
        let world = (basis.XAxis * tangentSpace.X) + (basis.YAxis * tangentSpace.Y) + (basis.ZAxis * tangentSpace.Z)
        from perturbed in VectorFrame.Of(origin: basis.Origin, normal: world, xHint: Some(basis.XAxis), context: Context.Canonical, key: key)
        select (PortValue)new PortValue.Frame(perturbed);
}

public sealed record MaterialGraph(Seq<AppearanceNode> Nodes, PortId Sink) {
    HashMap<PortId, AppearanceNode> ByPort =>
        Nodes.Fold(HashMap<PortId, AppearanceNode>.Empty, static (m, n) => m.AddOrUpdate(n.Id, n));

    static Option<string> Admit(AppearanceNode node, HashMap<PortId, AppearanceNode> known) =>
        node is AppearanceNode.Math math && !math.Op.Accepts(math.Operands.Count)
            ? Some($"<math-arity:{math.Id.Value}:{math.Op.Key}:{math.Operands.Count}>")
            : node.Dependencies
                .Filter(d => !Answers(known, d))
                .Head
                .Map(d => known.ContainsKey(d)
                    ? $"<non-producing-port:{node.Id.Value}<-{d.Value}>"
                    : $"<dangling-port:{node.Id.Value}<-{d.Value}>");

    static bool Answers(HashMap<PortId, AppearanceNode> known, PortId port) =>
        known.Find(port).Map(static n => n.Produces).IfNone(false);

    static Fin<AppearanceNode.BsdfOutput> SinkOf(HashMap<PortId, AppearanceNode> known, PortId sink, Op key) =>
        known.Find(sink).ToFin(new MaterialFault.Graph(key, "<sink-missing>"))
            .Bind(node => node is AppearanceNode.BsdfOutput output
                ? Fin.Succ(output)
                : Fin.Fail<AppearanceNode.BsdfOutput>(new MaterialFault.Graph(key, "<sink-not-bsdf-output>")));

    public Fin<CompiledGraph> Compile(Op key) => key.Catch(() => {
        HashMap<PortId, AppearanceNode> byId = ByPort;
        AdjacencyGraph<PortId, SEdge<PortId>> dag = new(allowParallelEdges: false);
        dag.AddVertexRange(Nodes.Map(static n => n.Id));
        Nodes.Iter(n => n.Dependencies.Filter(byId.ContainsKey).Iter(d => dag.AddVerticesAndEdge(new SEdge<PortId>(d, n.Id))));
        return from _ in guard(byId.Count == Nodes.Count, new MaterialFault.Graph(key, "<duplicate-node-id>"))
               from _admitted in Nodes.Choose(n => Admit(n, byId)).Head
                   .TraverseM(reason => Fin.Fail<Unit>(new MaterialFault.Graph(key, reason))).As()
               from output in SinkOf(byId, Sink, key)
               from ___ in guard(dag.IsDirectedAcyclicGraph(), new MaterialFault.Graph(key, "<cyclic-appearance-graph>"))
               let order = toSeq(dag.SourceFirstTopologicalSort()).Map(id => byId[id])
               let slots = order.Map(static (node, index) => KeyValuePair.Create(node.Id, index)).ToFrozenDictionary()
               select new CompiledGraph(order, output, slots,
                   order.Map(node => node.Dependencies.Map(d => slots[d]).ToArray()).Strict());
    });

    public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key) =>
        Compile(key).Bind(compiled => compiled.Shade(
            point, parameters, new PortValue[compiled.ScratchWidth], new SurfaceShade[1], key));

    // --- [AUTHORING]
    public Seq<PortId> Ports(int count) =>
        toSeq(Enumerable.Range(Nodes.Fold(0, static (highest, n) => System.Math.Max(highest, n.Id.Value)) + 1, count)).Map(PortId.Create);

    public Fin<MaterialGraph> Author(Seq<GraphEdit> edits, Op key) =>
        edits.FoldM(this, (graph, edit) => graph.Apply(edit, key)).As();

    Fin<MaterialGraph> Apply(GraphEdit edit, Op key) =>
        edit.Switch(
            state: (Graph: this, Key: key),
            node: static (s, n) => s.Graph.Admitted(n.Authored, s.Key),
            seat: static (s, n) => s.Graph.Seated(n.Replacement, s.Key),
            route: static (s, r) => s.Graph.Routed(r.Channel, r.Port, s.Key));

    public Fin<PortId> PortOf(ShadeChannel channel, Op key) =>
        SinkOf(ByPort, Sink, key).Map(channel.Port);

    Fin<MaterialGraph> Admitted(AppearanceNode node, Op key) =>
        ByPort switch { var known =>
            known.ContainsKey(node.Id)
                ? Fin.Fail<MaterialGraph>(new MaterialFault.Graph(key, $"<authored-duplicate-node-id:{node.Id.Value}>"))
                : node is AppearanceNode.BsdfOutput
                    ? Fin.Fail<MaterialGraph>(new MaterialFault.Graph(key, $"<authored-second-sink:{node.Id.Value}>"))
                    : Admit(node, known)
                        .Map(reason => Fin.Fail<MaterialGraph>(new MaterialFault.Graph(key, reason)))
                        .IfNone(() => Fin.Succ(this with { Nodes = Nodes.Add(node) })) };

    Fin<MaterialGraph> Seated(AppearanceNode node, Op key) =>
        ByPort switch { var known =>
            known.Find(node.Id).ToFin(new MaterialFault.Graph(key, $"<seated-port-absent:{node.Id.Value}>"))
                .Bind(standing => standing.Produces == node.Produces
                    ? Fin.Succ(standing)
                    : Fin.Fail<AppearanceNode>(new MaterialFault.Graph(key, $"<seated-produces-flip:{node.Id.Value}>")))
                .Bind(_ => Admit(node, known)
                    .Map(reason => Fin.Fail<MaterialGraph>(new MaterialFault.Graph(key, reason)))
                    .IfNone(() => Fin.Succ(this with { Nodes = Nodes.Map(n => n.Id == node.Id ? node : n) }))) };

    Fin<MaterialGraph> Routed(ShadeChannel channel, PortId port, Op key) =>
        ByPort switch { var known =>
            SinkOf(known, Sink, key)
                .Bind(sink => Answers(known, port)
                    ? Fin.Succ(this with { Nodes = Nodes.Map(n => n.Id == Sink ? channel.Route(sink, port) : n) })
                    : Fin.Fail<MaterialGraph>(new MaterialFault.Graph(key, $"<route-unanswerable-port:{channel.Key}<-{port.Value}>"))) };

    public static readonly MaterialGraph Default = BuildDefault();

    static MaterialGraph BuildDefault() {
        PortId baseColor = PortId.Create(1), metalness = PortId.Create(2), roughness = PortId.Create(3), normalSrc = PortId.Create(4), normal = PortId.Create(5), emission = PortId.Create(6), sink = PortId.Create(7);
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
    public int ScratchWidth { get; } = Order.Count;
    public int OperandWidth { get; } = Order.Fold(1, static (widest, node) => System.Math.Max(widest, node.Dependencies.Count));

    public ReadOnlySpan<int> Operands(int position) => OperandSlots[position];

    public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> window, Op key) =>
        ShadeSpan([point], parameters, scratch, window, key).Case is Error abandoned
            ? Fin.Fail<SurfaceShade>(abandoned)
            : window[0] is SurfaceShade shaded
                ? Fin.Succ(shaded)
                : Fin.Fail<SurfaceShade>(new MaterialFault.Graph(key, "<shade-window-unwritten>"));

    public Fin<Unit> ShadeSpan(ReadOnlySpan<ShadePoint> points, MaterialParameters parameters, Span<PortValue> scratch, Span<SurfaceShade> shades, Op key) {
        if (scratch.Length < ScratchWidth || shades.Length < points.Length) {
            return Fin.Fail<Unit>(new MaterialFault.Graph(key, $"<shade-span-rental-short:{scratch.Length}/{ScratchWidth}:{shades.Length}/{points.Length}>"));
        }
        PortValue[] operands = ArrayPool<PortValue>.Shared.Rent(OperandWidth);
        try {
            for (int p = 0; p < points.Length; p++) {
                for (int n = 0; n < Order.Count; n++) {
                    AppearanceNode node = Order[n];
                    int[] sources = OperandSlots[n];
                    for (int d = 0; d < sources.Length; d++) { operands[d] = scratch[sources[d]]; }
                    Fin<PortSlot> produced = NodeEvaluator.Apply(node, points[p], parameters, operands, key);
                    if (produced.Case is not PortSlot slot) { return produced.Map(static _ => unit); }
                    if (slot.Produced) { scratch[n] = slot.Value; }
                }
                Fin<SurfaceShade> shade = Assemble(Output, points[p], scratch, Slots, key);
                if (shade.Case is not SurfaceShade assembled) { return shade.Map(static _ => unit); }
                shades[p] = assembled;
            }
            return Fin.Succ(unit);
        }
        finally { ArrayPool<PortValue>.Shared.Return(operands, clearArray: true); }
    }

    static Fin<SurfaceShade> Assemble(AppearanceNode.BsdfOutput sink, ShadePoint point, ReadOnlySpan<PortValue> scratch, FrozenDictionary<PortId, int> slots, Op key) {
        foreach (ShadeChannel channel in ShadeChannel.Items) {
            PortValue produced = scratch[slots[channel.Port(sink)]];
            if (!ReferenceEquals(produced.Kind, channel.Kind)) {
                return Fin.Fail<SurfaceShade>(new MaterialFault.Graph(key, $"<sink-port-kind:{channel.Key}:{produced.Kind.Key}>"));
            }
        }
        if (scratch[slots[sink.NormalFrame]] is not PortValue.Frame frame) {
            return Fin.Fail<SurfaceShade>(new MaterialFault.Graph(key, $"<sink-normal-not-frame:{sink.NormalFrame.Value}>"));
        }
        double metalness = scratch[slots[sink.Metalness]].AsScalar, roughness = scratch[slots[sink.Roughness]].AsScalar;
        if (!MaterialParameters.InUnit(metalness) || !MaterialParameters.InUnit(roughness)) {
            return Fin.Fail<SurfaceShade>(new MaterialFault.Parameter(key, $"<shade-weight-out-of-unit:{metalness:R},{roughness:R}>"));
        }
        SurfaceShade shade = new(
            scratch[slots[sink.BaseColor]].AsColor, metalness, roughness, frame.Value,
            scratch[slots[sink.Emission]].AsColor);
        return shade.InGamut
            ? Fin.Succ(shade)
            : Fin.Fail<SurfaceShade>(new MaterialFault.Gamut(key, $"<shade-out-of-gamut:{shade.BaseColorLinear.Hex}>"));
    }
}
```

## [03]-[MATERIAL_LIBRARY]

- Owner: `MaterialLibrary` over `MaterialParameters` keyed by the contract `MaterialId`; `SubsurfaceRadius` the validated mean-free-path carrier; `ThinFilm` the validated interference-film carrier (the OpenPBR `thin_film` group as one value object); `ContrastGrade` the WCAG rung roster conforming the kernel `ICapability<TSelf>`; `MaterialParameters` the canonical row — the closed positional Disney core beside its init-defaulted enrichment band.
- Cases: the `MaterialLibrary.Rows` fence IS the seed roster and the only roster — `metal.gold`/`metal.copper`/`metal.aluminum`/`metal.titanium`/`metal.iron`/`metal.steel`/`metal.silver`/`metal.chrome`/`metal.brass` (`metal.steel` the galvanized/structural-steel render row the `Component/component#COMPONENT_OWNER` `Component.AppearanceId` resolves — a warm-grey conductor distinct from the bluer `metal.iron`), `glass.crown`/`glass.flint`, `liquid.water`/`liquid.oil`, `gas.cavity`, `gem.diamond`, `stone.jade`/`stone.marble`, `plastic.abs`/`plastic.pvc`, `rubber.matte`, `polymer.adhesive` (the amber structural-epoxy render row a bonded `Component/joint#JOINT_FAMILY` `AdhesiveClass` joint's `AppearanceId` resolves — a smooth IOR-1.55 dielectric, distinct from the `metal.steel` base-metal `SubstanceId`), `skin.caucasian`/`skin.deep`, `fabric.velvet`/`fabric.silk`/`fabric.denim`, `paint.car-metallic`/`paint.clearcoat`, `ceramic.glazed`/`ceramic.porcelain`, `wax.beeswax`/`wax.candle`, `wood.oak`, `coat.gold-leaf` — each a row of `MaterialParameters` values the catalog grows by pure data addition (a new measured material is one row, not a new type), ZERO per-material types.
- Entry: `public static Fin<MaterialParameters> Lookup(MaterialId id, Op key)` — `Fin<T>` aborts on an unregistered id and on the catalogue's own `Admission` census, the `Validation`-accumulated proof that every seed row clears `MaterialParameters.Of`, resolved once at type init and bound by both reads (`MaterialFault.Parameter`, key-correlated); an ad-hoc parameter vector admits through `MaterialParameters.Of` directly — the ONE row validation catalog rows and measured imports share, never a library-level forwarding alias; `Assign` is the profile-generalization entry mapping a masonry `Component` `MaterialId` to a catalog row through that same admission; `Named` re-bases a Datasets named colour into a row's scene-linear `BaseColor`; `NearestChecker` (metric-parameterized over the full `DeltaE` selector) and `HueConstant` (the Ebner-Fairchild constant-hue witness) both take the kernel `Tolerance` carrier on `ToleranceLane.Spectral`, so a gate's band is proved before it is read and the refusal names its lane; `PointerAdmit`/`SpectralAdmit` are the two fallible reproducibility gates over the kernel `GamutPolicy` rows whose `Bound` is the recovery; `Contrast`/`Requires`/`NearestIscc` the accessibility and designation projections over the `ContrastGrade` capability roster.
- Packages: Rasm (project — the `Numerics/atoms#SCALAR_FLOOR` `RgbProfile.Acescg.Configuration` scene-linear instance `PortValue.SceneLinear` names, and the `GamutPolicy` `Pointer`/`MacAdam`/`Perceptual` reproducibility rows whose `Contains`/`Bound` pair this page's gates fail and recover through), Wacton.Unicolour (base-color/emission construction; the `IsImaginary` spectral-locus pre-test, the full 12-member `DeltaE` selector through `Difference` (the drift gate dispatches `Ciede2000`/`Cam16`/`Hyab` by the caller's policy row), and the `Contrast` WCAG ratio), Wacton.Unicolour.Datasets (composed for `Macbeth.All` ColorChecker validation, `Css`/`Xkcd`/`Nord` named-colour resolution, the `EbnerFairchild` `AllHue0..AllHue336` constant-hue loci driving `HueConstant` (`HungBerns` the admitted alternate loci family), and the `IsccNbs` 267 designation centroids driving `NearestIscc` — validation/reference only), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `System.Reflection` for the one definition-time Datasets field derivation)
- Growth: a new material is one `MaterialLibrary.Rows` entry — a `MaterialId` key and a `MaterialParameters` value; a new appearance parameter shared by ALL materials is one init-defaulted column on `MaterialParameters` (every existing row binds unchanged — the `Film` thin-film carrier is exactly this growth, landed as the OpenPBR `thin_film` group's row source); a new gamut domain (the display RGB gate, the Pointer real-surface gate, the MacAdam spectral-limit gate) is one accessor predicate by domain, never a collapse of the three into one gate; a new drift metric is a `DeltaE` member the caller's policy row passes, never a second checker; a new reference dataset is one reflection-derived table over the admitted Datasets assembly; a new accessibility-preview is one read-only projection over the package's own selector, never a stored row. There is NO per-material type, NO `GoldMaterial`/`GlassMaterial` class, NO `MetalFactory`/`PlasticFactory`, and NO per-family graph variant — the named defect is a second material surface; the repair is a row. Measured-spectral grounding is settled per ROW rather than per family — the index corpus grounds the conductors and transparent dielectrics, the angular archive the woven and coated-paint rows, the reflectance libraries five named rows across stone, rubber, foliage, and skin — and every remaining row reads `Authored` as a measured verdict.
- Boundary: `MaterialParameters` is the single material concept — its positional core is the closed Disney-principled parameter set (base color, metalness, roughness, specular tint, anisotropy, IOR, transmission, transmission roughness, sheen, sheen tint, clearcoat, clearcoat roughness, subsurface weight, subsurface radius, emission color, emission luminance) and every later axis lands as an INIT-DEFAULTED column so each catalogue row and sibling construction binds unchanged: the `Film` interference carrier (the OpenPBR `thin_film` group's ROW SOURCE the `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` reads and the `finish#FINISH` pearlescent/anodized rows seed, validated once at `ThinFilm.Create` so a negative thickness, an out-of-unit weight, or a sub-unity film IOR is unrepresentable), the three OpenPBR tint colours `CoatColor`/`SpecularColor`/`FuzzColor` neutral at `PortValue.White` (so the `weathering#WEATHERING` `CoatColorTo`/`FuzzColorTo` trajectories, the tinted `finish#FINISH` rows, and the `Raster/set#TEXTURE_SET` `coat_color`/`specular_color`/`fuzz_color` planes each write a REAL column instead of a lens that collapsed a three-band tint to one luminance scalar), `BaseDiffuseRoughness` as the Oren-Nayar axis distinct from the specular roughness beside it, `AnisotropyRotation` as the unit-convention grain reference the `bsdf#LOBE_FAMILY` anisotropic lobes turn by, `ThinWalled` as the OpenPBR `geometry_thin_walled` double-sided-shell flag (a set-level boolean the `foliage.leaf`, `paper.sheet`, `fabric.silk`, and `fabric.denim` rows set, the wire's `GeometryThinWalled` column carries, and the `surface#OPENPBR_SLAB` `Slab.Base` reads to transmit at unit index — the texture roster correctly excludes it as no per-texel field), and `EmissionProvenance` as the typed-absence `EmissionInput` an ADMITTED emission magnitude carries WHOLE — the unit witness beside the chromaticity, CCT+Duv, relative-luminance, and gamut-map evidence the resolve took, so `interchange#MATERIAL_WIRE` mirrors the photometric measurement the way it already mirrors the `acquisition#ACQUISITION` capture measurement rather than stranding it at the resolve; base color and emission are constructed once through Wacton.Unicolour scene-linear `Acescg` so the table carries spectrally-grounded colors, never raw byte triples; `Metalness` is the conductor/dielectric PARTITION the `bsdf#LOBE_FAMILY` lobe weights read and `Ior` is the dielectric arm's own interface index at every row — the conductor arm grounds from the `surface#CONDUCTOR_IOR` `ConductorMetal` row the id names, so a "metal" and a "plastic" differ by the metalness, IOR, and roughness columns and by which measured metal the id resolves, never by type; a conductor's own `(η, k)` never enters this column, which is why the admitted IOR band is total rather than keyed on metalness; transmission>0 with IOR selects the dielectric-transmission lobe so glass, water, the sealed IGU cavity gas (`gas.cavity`, IOR 1.0 so its transmissive interface carries no Fresnel and the `Component/glazing#GLAZING_FAMILY` cavity layers shade as a clear non-refracting fill rather than the `liquid.water` proxy), and gems are rows differing only in IOR and transmission roughness; subsurface weight>0 routes the subsurface lobe so skin, wax, jade, and marble are rows differing only in subsurface radius (the per-channel mean-free-path carried as the validated three-band `SubsurfaceRadius` `[ComplexValueObject]`, a negative or non-finite millimetre band unrepresentable at `Create` so the inline negative-mfp guard `MaterialParameters.Of` once carried is gone); sheen>0 routes the sheen lobe so velvet, silk, and denim are rows differing only in sheen and roughness; clearcoat>0 layers the clearcoat lobe so car paint and glazed ceramic are rows differing only in clearcoat and clearcoat roughness; the profile consumer generalizes through `Assign`, which maps a masonry `Component` to a `MaterialId` row and NEVER mints a profile-specific material — `Component/masonry#MASONRY_FAMILY` is the cross-section owner the engine reads, never modifies, and an unmapped key falls back to the neutral `ceramic.porcelain` row rather than a fault so the profile consumer always shades; the Wacton.Unicolour.Datasets composition is validation/reference only — `NearestChecker` gates a candidate against the nearest `Macbeth.All` ColorChecker patch by `Unicolour.Difference` under the CALLER'S `DeltaE` metric (a drift beyond tolerance fails `MaterialFault.Gamut`; the metric is a policy value on the finish row, never a hidden default), `HueConstant` anchors a REFERENCE to its nearest `EbnerFairchild` constant-hue locus and requires the candidate within tolerance of that SAME locus (a tint that walked off-hue fails the reused `Gamut` case), `NearestIscc` projects the nearest of the 267 ISCC-NBS centroids as the standardized designation a specification prints, and `Named` re-bases a passed `Css`/`Xkcd`/`Nord` named `Unicolour` into a row's scene-linear `BaseColor` through `ConvertToConfiguration(SceneLinear)` FIRST (so the read channels are genuinely AP1-linear, not an sRGB-linear triple mislabelled as AP1 — the same colorimetric boundary the AP1 luminance honors); the ISCC/loci tables are ONE definition-time reflection derivation over the admitted assembly's own public static fields (`SYMBOLIC_REFERENCE`: the names and groups travel as the assembly's identifiers, never a hand-keyed 267-row transcription that drifts), the observer CMFs/illuminant SPDs/reflectance staying on the main Wacton.Unicolour owner the Datasets package does not carry; there are THREE gamut gates BY DOMAIN, never one collapse and never a nesting — `SurfaceShade.InGamut` reads `GamutPolicy.Perceptual.Contains` against the AP1 WORKING SPACE the `SceneLinear` configuration declares (the working-space bound every row evaluates through; ACEScg is wider than any display and is NOT contained in the Pointer volume, so a containment ladder over the three is the false claim this sentence deletes), `PointerAdmit` reads `GamutPolicy.Pointer` (the physical-reproducibility gate a pigment-mixed reflectance must pass, the predicate `Appearance/finish#FINISH` imports for its admission), and `SpectralAdmit` reads `GamutPolicy.MacAdam` (the absolute spectral-locus bound a reflectance physically reachable at its luminance must satisfy, a reflectance beyond the spectral locus first caught by `IsImaginary` so an imaginary colour fails before the MacAdam test), each domain-gate failing the SAME `MaterialFault.Gamut` case with its own domain reason string (the case is reused across all three, never a second fault) while the RECOVERY is the same kernel row's `Bound` — three INDEPENDENT domains named once in the kernel vocabulary, each carrying its predicate and its nearest-in-domain projection together, so a Materials-side projection rename over `MapToPointerGamut`/`MapToMacAdamLimits` is the deleted form (the `HueConstant` witness sits BESIDE them as a constancy check, never a fourth gamut); the accessibility projection is the kernel `PerceptualColor.Simulate(Cvd, UnitInterval)` — a folder-local preview that clamped a raw severity double instead of admitting it was the deleted form — and `Contrast` reads the WCAG ratio beside the `CapabilitySet<ContrastGrade>` of rungs it clears, each rung's published threshold living on its own row and the nesting carried by a `CapabilityLaw` a bool triple could not state (its eight corners spelled four reachable states), with `Requires` the entry a specification states its demanded rung through, refusing through the kernel `CapabilitySet.Require` door whose refuse arm receives the `Missing` complement, so a short pair names WHICH rungs it failed — READ-ONLY projections the color-specification boundary consumes, never stored library columns; every row evaluates to an in-gamut `SurfaceShade` through the same `MaterialGraph`.

```csharp

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColourBasis {
    public static readonly ColourBasis Authored = new("authored", spectral: false);
    public static readonly ColourBasis Reflectance = new("spectral-reflectance", spectral: true);
    public static readonly ColourBasis Refractive = new("refractive-index", spectral: true);
    public static readonly ColourBasis Goniometric = new("goniometric-brdf", spectral: true);

    public bool Spectral { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContrastGrade : ICapability<ContrastGrade> {
    public static readonly ContrastGrade AaLarge = new("aa-large", ratio: 3.0);
    public static readonly ContrastGrade AaText = new("aa-text", ratio: 4.5);
    public static readonly ContrastGrade AaaText = new("aaa-text", ratio: 7.0);

    public double Ratio { get; }

    public static readonly CapabilityLaw<ContrastGrade> Nested = new(Seq(
        CapabilitySet<ContrastGrade>.None,
        CapabilitySet<ContrastGrade>.Of(AaLarge),
        CapabilitySet<ContrastGrade>.Of(AaLarge, AaText),
        CapabilitySet<ContrastGrade>.All));
}

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

// --- [MODELS] --------------------------------------------------------------------------
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

    public ThinFilm Film { get; init; } = ThinFilm.None;

    public double ClearcoatAnisotropy { get; init; }
    public double ClearcoatAnisotropyRotation { get; init; }

    public Unicolour CoatColor { get; init; } = PortValue.White;
    public Unicolour SpecularColor { get; init; } = PortValue.White;
    public Unicolour FuzzColor { get; init; } = PortValue.White;

    public Seq<Unicolour> Colours => Seq(BaseColor, Emission, CoatColor, SpecularColor, FuzzColor);

    public double BaseDiffuseRoughness { get; init; }

    public double AnisotropyRotation { get; init; }

    public bool ThinWalled { get; init; }

    public Option<EmissionInput> EmissionProvenance { get; init; }

    public static Fin<MaterialParameters> Of(MaterialParameters candidate, Op key) =>
        from _ in guard(InUnit(candidate.Metalness) && InUnit(candidate.Roughness) && InUnit(candidate.SpecularTint) && InUnit(candidate.Anisotropy)
                && InUnit(candidate.AnisotropyRotation) && InUnit(candidate.BaseDiffuseRoughness)
                && InUnit(candidate.Transmission) && InUnit(candidate.TransmissionRoughness) && InUnit(candidate.Sheen) && InUnit(candidate.SheenTint)
                && InUnit(candidate.Clearcoat) && InUnit(candidate.ClearcoatRoughness) && InUnit(candidate.Subsurface), new MaterialFault.Parameter(key, "<weight-out-of-unit>"))
        from __ in guard(InIorRange(candidate.Ior), new MaterialFault.Parameter(key, $"<ior-out-of-range:{candidate.Ior}>"))
        from ___ in guard(double.IsFinite(candidate.EmissionLuminance) && candidate.EmissionLuminance >= 0.0, new MaterialFault.Parameter(key, $"<emission-luminance-negative:{candidate.EmissionLuminance:R}>"))
        from ____ in guard(candidate.Colours.ForAll(GamutPolicy.Perceptual.Contains), new MaterialFault.Gamut(key, "<row-color-out-of-gamut>"))
        select candidate;

    internal static bool InUnit(double v) => double.IsFinite(v) && v is >= 0.0 and <= 1.0;
    static bool InIorRange(double ior) => double.IsFinite(ior) && ior is >= 1.0 and <= 2.5;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MaterialLibrary {
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
    static readonly Unicolour Black = PortValue.Black;
    static readonly SubsurfaceRadius NoScatter = SubsurfaceRadius.None;

    static readonly MaterialId Neutral = MaterialId.Create("ceramic.porcelain");

    static readonly FrozenDictionary<string, ColourBasis> BasisKeys = new Dictionary<string, ColourBasis> {
        ["metal"] = ColourBasis.Refractive,
        ["coat"] = ColourBasis.Refractive,
        ["glass"] = ColourBasis.Refractive,
        ["liquid"] = ColourBasis.Refractive,
        ["gas"] = ColourBasis.Refractive,
        ["gem"] = ColourBasis.Refractive,
        ["fabric"] = ColourBasis.Goniometric,
        ["paint"] = ColourBasis.Goniometric,
        ["stone.marble"] = ColourBasis.Reflectance,
        ["rubber.matte"] = ColourBasis.Reflectance,
        ["foliage.leaf"] = ColourBasis.Reflectance,
        ["skin.caucasian"] = ColourBasis.Reflectance,
        ["skin.deep"] = ColourBasis.Reflectance,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    public static ColourBasis Basis(MaterialId id) =>
        BasisKeys.TryGetValue(id.Value, out ColourBasis? row) ? row!
            : BasisKeys.TryGetValue(id.Value.Split('.') is [var family, ..] ? family : id.Value, out ColourBasis? shared) ? shared!
            : ColourBasis.Authored;

    static readonly FrozenDictionary<string, Unicolour> IsccCentroids = Fields<Unicolour>(typeof(IsccNbs), "");
    static readonly FrozenDictionary<string, IEnumerable<Unicolour>> HueLoci = Fields<IEnumerable<Unicolour>>(typeof(EbnerFairchild), "AllHue");

    static FrozenDictionary<string, T> Fields<T>(Type dataset, string prefix) =>
        dataset.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => typeof(T).IsAssignableFrom(f.FieldType) && f.Name.StartsWith(prefix, StringComparison.Ordinal))
            .ToFrozenDictionary(static f => f.Name, static f => (T)f.GetValue(null)!, StringComparer.Ordinal);

    public static readonly FrozenDictionary<MaterialId, MaterialParameters> Rows = new (MaterialId Id, MaterialParameters Row)[] {
        (MaterialId.Create("metal.gold"),      new(Linear(1.000, 0.766, 0.336), 1.0, 0.12, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("metal.copper"),    new(Linear(0.955, 0.638, 0.538), 1.0, 0.18, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("metal.aluminum"),  new(Linear(0.913, 0.922, 0.924), 1.0, 0.08, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("metal.silver"),    new(Linear(0.972, 0.960, 0.915), 1.0, 0.05, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("metal.iron"),      new(Linear(0.560, 0.570, 0.580), 1.0, 0.35, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("metal.steel"),     new(Linear(0.560, 0.570, 0.577), 1.0, 0.40, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("metal.titanium"),  new(Linear(0.542, 0.497, 0.449), 1.0, 0.28, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("metal.chrome"),    new(Linear(0.550, 0.556, 0.554), 1.0, 0.02, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("metal.brass"),     new(Linear(0.887, 0.789, 0.434), 1.0, 0.22, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("glass.crown"),     new(Linear(0.960, 0.970, 0.980), 0.0, 0.02, 0.0,  0.0,  1.520, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("glass.flint"),     new(Linear(0.950, 0.945, 0.960), 0.0, 0.03, 0.0,  0.0,  1.620, 1.0, 0.05, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("liquid.water"),    new(Linear(0.980, 0.990, 0.995), 0.0, 0.0,  0.0,  0.0,  1.333, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("liquid.oil"),      new(Linear(0.920, 0.880, 0.620), 0.0, 0.04, 0.0,  0.0,  1.470, 0.9, 0.08, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("gas.cavity"),      new(Linear(0.998, 0.998, 0.998), 0.0, 0.0,  0.0,  0.0,  1.000, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("gem.diamond"),     new(Linear(0.990, 0.990, 0.995), 0.0, 0.0,  0.0,  0.0,  2.417, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("stone.jade"),      new(Linear(0.270, 0.560, 0.380), 0.0, 0.35, 0.0,  0.0,  1.660, 0.4, 0.30, 0.0, 0.0, 0.0, 0.0, 0.6, SubsurfaceRadius.Create(4.0, 8.0, 5.0), Black, 0.0)),
        (MaterialId.Create("plastic.abs"),     new(Linear(0.800, 0.050, 0.050), 0.0, 0.30, 0.5,  0.0,  1.460, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("plastic.pvc"),     new(Linear(0.180, 0.380, 0.760), 0.0, 0.45, 0.4,  0.0,  1.520, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("rubber.matte"),    new(Linear(0.040, 0.040, 0.040), 0.0, 0.85, 0.0,  0.0,  1.519, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("polymer.adhesive"), new(Linear(0.250, 0.190, 0.110), 0.0, 0.35, 0.0,  0.0,  1.550, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("skin.caucasian"), new(Linear(0.640, 0.430, 0.370), 0.0, 0.45, 0.0,  0.0,  1.400, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, SubsurfaceRadius.Create(3.67, 1.37, 0.68), Black, 0.0)),
        (MaterialId.Create("skin.deep"),       new(Linear(0.330, 0.180, 0.130), 0.0, 0.50, 0.0,  0.0,  1.400, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, SubsurfaceRadius.Create(2.10, 0.80, 0.40), Black, 0.0)),
        (MaterialId.Create("fabric.velvet"),   new(Linear(0.380, 0.030, 0.080), 0.0, 0.90, 0.0,  0.0,  1.460, 0.0, 0.0, 1.0, 0.8, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("fabric.silk"),     new(Linear(0.700, 0.620, 0.480), 0.0, 0.35, 0.2,  0.6,  1.460, 0.0, 0.0, 0.6, 0.3, 0.0, 0.0, 0.0, NoScatter, Black, 0.0) { ThinWalled = true }),
        (MaterialId.Create("fabric.denim"),    new(Linear(0.150, 0.230, 0.380), 0.0, 0.80, 0.0,  0.0,  1.460, 0.0, 0.0, 0.4, 0.5, 0.0, 0.0, 0.0, NoScatter, Black, 0.0) { ThinWalled = true }),
        (MaterialId.Create("foliage.leaf"),    new(Linear(0.090, 0.220, 0.060), 0.0, 0.55, 0.0,  0.0,  1.420, 0.35, 0.60, 0.0, 0.0, 0.15, 0.25, 0.4, SubsurfaceRadius.Create(2.0, 3.5, 1.5), Black, 0.0) { ThinWalled = true }),
        (MaterialId.Create("paper.sheet"),     new(Linear(0.780, 0.770, 0.740), 0.0, 0.75, 0.0,  0.0,  1.500, 0.30, 0.85, 0.0, 0.0, 0.0, 0.0, 0.25, SubsurfaceRadius.Create(3.0, 3.0, 3.0), Black, 0.0) { ThinWalled = true }),
        (MaterialId.Create("paint.car-metallic"), new(Linear(0.090, 0.020, 0.220), 0.85, 0.30, 0.0, 0.0, 1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("paint.clearcoat"), new(Linear(0.700, 0.700, 0.700), 0.0, 0.40, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.03, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("ceramic.glazed"),  new(Linear(0.880, 0.850, 0.780), 0.0, 0.10, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.9, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("ceramic.porcelain"), new(Linear(0.930, 0.920, 0.900), 0.0, 0.20, 0.0, 0.0, 1.504, 0.0, 0.0, 0.0, 0.0, 0.3, 0.10, 0.4, SubsurfaceRadius.Create(5.0, 5.0, 5.0), Black, 0.0)),
        (MaterialId.Create("wax.beeswax"),     new(Linear(0.870, 0.700, 0.330), 0.0, 0.55, 0.0,  0.0,  1.443, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.8, SubsurfaceRadius.Create(6.0, 4.0, 1.5), Black, 0.0)),
        (MaterialId.Create("wax.candle"),      new(Linear(0.940, 0.920, 0.850), 0.0, 0.60, 0.0,  0.0,  1.430, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.9, SubsurfaceRadius.Create(8.0, 6.0, 4.0), Black, 0.0)),
        (MaterialId.Create("stone.marble"),    new(Linear(0.870, 0.860, 0.840), 0.0, 0.30, 0.0,  0.0,  1.486, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.5, SubsurfaceRadius.Create(2.19, 2.62, 3.00), Black, 0.0)),
        (MaterialId.Create("wood.oak"),        new(Linear(0.430, 0.270, 0.140), 0.0, 0.55, 0.3,  0.4,  1.530, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Create("coat.gold-leaf"),  new(Linear(1.000, 0.766, 0.336), 1.0, 0.06, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.02, 0.0, NoScatter, Black, 0.0)),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row);

    static readonly Fin<Unit> Admission =
        toSeq(Rows).Traverse(static entry =>
                MaterialParameters.Of(entry.Value, Op.Of(name: $"material-library-admit:{entry.Key.ToValue()}"))
                    .ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit);

    public static Fin<MaterialParameters> Lookup(MaterialId id, Op key) =>
        from _ in Admission
        from row in Rows.TryGetValue(id, out MaterialParameters? found)
            ? Fin.Succ(found!)
            : Fin.Fail<MaterialParameters>(new MaterialFault.Parameter(key, $"<unregistered-material:{id.ToValue()}>"))
        select row;

    public static Fin<MaterialParameters> Assign(MaterialId appearanceId, Op key) =>
        Rows.ContainsKey(appearanceId) ? Lookup(appearanceId, key) : Lookup(Neutral, key);

    public static MaterialParameters Named(Unicolour reference, MaterialParameters template) {
        ColourTriplet ap1 = reference.ConvertToConfiguration(PortValue.SceneLinear).RgbLinear.Triplet;
        return template with { BaseColor = new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, ap1.First, ap1.Second, ap1.Third) };
    }

    static Option<T> MinBy<T>(Seq<T> rows, Func<T, double> measure) =>
        rows.Fold(Option<(T Row, double Score)>.None,
                (best, row) => measure(row) switch {
                    var score => best.Filter(b => b.Score <= score).IsSome ? best : Some((Row: row, Score: score)),
                })
            .Map(static best => best.Row);

    public static Fin<(Unicolour Patch, double DeltaE)> NearestChecker(Unicolour candidate, Tolerance tolerance, DeltaE metric, Op key) =>
        MinBy(toSeq(Macbeth.All).Map(patch => (Patch: patch, DeltaE: candidate.Difference(patch, metric))), static row => row.DeltaE)
            .ToFin(new MaterialFault.Parameter(key, "<colorchecker-set-empty>"))
            .Bind(nearest => nearest.DeltaE <= tolerance.Value
                ? Fin.Succ(nearest)
                : new MaterialFault.Gamut(key, $"<colorchecker-drift:{tolerance.Lane.Key}:deltaE={nearest.DeltaE:R}>"));

    public static Fin<Unicolour> HueConstant(Unicolour candidate, Unicolour reference, Tolerance tolerance, Op key) =>
        MinBy(toSeq(HueLoci).Map(locus => (Locus: locus.Key, Anchor: LocusDelta(reference, locus.Value), Drift: LocusDelta(candidate, locus.Value))),
                static row => row.Anchor)
            .ToFin(new MaterialFault.Parameter(key, "<constant-hue-loci-empty>"))
            .Bind(nearest => nearest.Drift <= tolerance.Value
                ? Fin.Succ(candidate)
                : new MaterialFault.Gamut(key, $"<hue-shifted-tint:{nearest.Locus}:deltaE={nearest.Drift:R}>"));

    static double LocusDelta(Unicolour colour, IEnumerable<Unicolour> locus) =>
        locus.Min(member => colour.Difference(member, DeltaE.Ciede2000));

    public static Fin<Unicolour> PointerAdmit(Unicolour reflectance, Op key) =>
        GamutPolicy.Pointer.Contains(reflectance)
            ? Fin.Succ(reflectance)
            : new MaterialFault.Gamut(key, $"<pointer-unreproducible-reflectance:{reflectance.Hex}>");

    public static Fin<Unicolour> SpectralAdmit(Unicolour reflectance, Op key) =>
        reflectance.IsImaginary
            ? new MaterialFault.Gamut(key, $"<imaginary-reflectance:{reflectance.Hex}>")
            : GamutPolicy.MacAdam.Contains(reflectance)
                ? Fin.Succ(reflectance)
                : new MaterialFault.Gamut(key, $"<macadam-unreproducible-reflectance:{reflectance.Hex}>");

    public static (double Ratio, CapabilitySet<ContrastGrade> Cleared) Contrast(Unicolour foreground, Unicolour background) =>
        foreground.Contrast(background) switch {
            var ratio => (ratio, CapabilitySet<ContrastGrade>.Of(toSeq(ContrastGrade.Items).Filter(rung => ratio >= rung.Ratio).ToArray())),
        };

    public static Fin<double> Requires(Unicolour foreground, Unicolour background, CapabilitySet<ContrastGrade> required, Op key) =>
        from admitted in ContrastGrade.Nested.Admit(required)
        let measured = Contrast(foreground, background)
        from _ in measured.Cleared.Require(admitted, missing =>
            new MaterialFault.Gamut(key, $"<contrast-short:{measured.Ratio:R}:missing={missing.Wire}>"))
        select measured.Ratio;

    public static (string Name, Unicolour Centroid, double DeltaE) NearestIscc(Unicolour candidate) =>
        MinBy(toSeq(IsccCentroids).Map(row => (Name: row.Key, Centroid: row.Value, DeltaE: candidate.Difference(row.Value, DeltaE.Ciede2000))),
                static row => row.DeltaE)
            .IfNone((Name: "<unnamed>", Centroid: candidate, DeltaE: double.MaxValue));
}
```

## [04]-[RESEARCH]

(none)
