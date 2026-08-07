# [MATERIALS_PRESS]

THE BAKE ENGINE. One `TexturePress.Press` fold drives a `PressSubject` — a compiled `graph#MATERIAL_GRAPH` node program, a raw `texture#TEXTURE_UV` procedural field, a `surface#OPENPBR_SLAB` parameter vector under a spatially-varying aging trajectory, or a `finish#FINISH` Kubelka-Munk pigment mix under a spatially-varying weight simplex — across a `PressPlan`'s texel grid and mints a `set#TEXTURE_SET` `TextureSet` of content-keyed `plane#TEXTURE_PLANE` pyramids. The four subjects compile ONCE into one `PressProgram`, and the row kernel dispatches on that program per BAND rather than per texel, so a plane costs one dispatch per partition and one allocation-free pass per row. The two spatially-varying subjects share one quantization law: a fallible per-texel admission is unaffordable at sixteen million points, so each compiles a LADDER of admitted vectors over its own declared rung columns — the aging ladder over the `(age, cavity)` PRODUCT the `weathering#WEATHERING` `CavityResponse` rows make irreducible to one scalar, the mix ladder over the barycentric weight simplex — and every texel indexes the cell its fields sampled, so a spatially-varying bake pays a declared cell count rather than a per-texel rail. Batching rides `graph#MATERIAL_GRAPH` `CompiledGraph.ShadeSpan`: the port environment resolves ONCE into an index-addressed scratch whose slot order IS the frozen compiled sort, so a plane never rebuilds an immutable map per node per texel — the difference between minutes and days at four thousand square; the integrator's per-point `Shade` re-enters the SAME rail over a one-element window, so the press and the integrator drive one evaluation law. The band fold rides `ParallelHelper.For` over a seeded struct `IAction`, and every per-texel jitter derives from the TEXEL COORDINATE and the plan seed rather than from a sequential stream, so a band partition can never reorder a draw and a re-press at one seed is byte-identical at any processor count.

Persisted plane bytes are ALWAYS CPU-minted. The `PressBackend.WebGpu` row is an accelerator and preview lane whose output is never content-addressed, and that is a STRUCTURAL veto rather than an empirical tolerance: `PressProduct` gives the GPU arm a `Preview` case carrying planes and a receipt but NO `TextureSet`, so a GPU-keyed set has no spelling and cannot be persisted, wired, or addressed by accident. GPU `f32` cannot reproduce the CPU `f64` procedural lattice, so a GPU-keyed plane forks the content key at its own preimage; the divergence the parity workload measures rides `PressReceipt.GpuDeltaMax` as TELEMETRY and never enters a key. Every measured column is a TYPED ABSENCE when nothing measured it, because a fabricated zero and an unmeasured pass are the two states a gate exists to separate — the graph key of a graphless subject, the parity delta of a single-lane press, the ladder coverage of an unaged run, and the wall time of an unclocked run all read absence rather than zero. The page composes `set#TEXTURE_SET` for the produced bundle, its channel roster, and each slot's `Read` projection, `plane#TEXTURE_PLANE` for the arena and the pyramid, `filter#PLANE_OP` for every post chain, every derived channel, and its `PlaneReceipt` evidence, `tile#TILE_SYNTH` for the in-fold tiling a plan requests, `gpu#PRESS_DEVICE` for the accelerator arm, `weathering#WEATHERING` for the aging ladder, `finish#FINISH` for the pigment-mix ladder, `surface#OPENPBR_SLAB` `ToneMap` for the one display egress a binding declares, the kernel `Deterministic` splitmix64 draw, `ContentHash` identity, and `ValidityClaim` receipt fold, `TimeProvider` for the one measured wall time, and CommunityToolkit.HighPerformance for every pooled arena and partitioned band — reminting no evaluator, no arena, no random source, no clock, no tone curve, and no identity.

## [01]-[INDEX]

- [02]-[PRESS_PLAN]: the `PressBackend` axis, the `PressSubject` union, the `ChannelBinding` row with its `DisplayEgress` column, the `LadderRungs` quantization carrier, the `PressPlan` record with its canonical plan key, and the binding-order law that seats derived channels after their sources and paired channels after their companions.
- [03]-[TEXTURE_PRESS]: the one `TexturePress.Press` entry, the `PressProgram` compiled subject, the `AgeLadder` two-dimensional aged-vector table and the `MixLadder` weight-simplex table, the `PressRows` band kernel over `ParallelHelper.For`, the coordinate-keyed jitter law, the paired mip resolution, the GPU lowering gate, and the post-fold and tiling composition.
- [04]-[PRESS_RECEIPT]: the `PressProduct` union that makes the content-identity veto structural, and the `PressReceipt` evidence row with its per-channel plane receipts, its `AgeCoverage` ladder-exercise column, and its typed absences.

## [02]-[PRESS_PLAN]

- Owner: `PressPlan` the bake request; `PressSubject` `[Union]` the thing being baked; `ChannelBinding` the per-channel request row; `DisplayEgress` the per-binding scene-to-display policy; `LadderRungs` the quantization carrier every ladder reads; `PressBackend` `[SmartEnum<string>]` the execution lane.
- Cases: subject {`Graph` (a `MaterialGraph` with the parameter row and conductor its sink resolves against), `Source` (one `TextureSource` sampled through a `SamplerState` into one channel), `Slab` (a `MaterialParameters` row lowered to the OpenPBR vector, aged per texel by the age, cavity, and curvature fields), `Mix` (a `finish#FINISH` pigment set resolved per texel through a `TextureSource` weight field per pigment), `Sky` (a frontier-supplied radiance closure over a world direction), `MeshSpace` (an already-flattened chart run carried as data, with its ray target)} · backend {`cpu` (content-authoritative), `webgpu` (accelerator, never content-authoritative)}.
- Law: binding ORDER is derived, never authored — `Of` sorts bindings by `TextureChannel.Origin` depth, then by pair dependency, then by `TextureChannel.Ordinal`, so every `Shaded` and `Geometric` channel seats before any `Derived` one, a normal seats before the roughness whose mip fold consumes its variance, and a plan requesting `occlusion` without `height` produces `height` as an intermediate rather than refusing. A caller never sequences the fold.
- Law: spatial cavity evidence enters the press as its OWN field, never as a derived channel. A `Slab` subject's own `occlusion` chain derives from `height`, which derives from `geometry_normal`, whose origin is a CONSTANT the shade point never reaches — so a slab press solves a flat height field and produces a uniform-1.0 occlusion plane by construction, and no binding order could rescue it because `Compile` is the first generator of the fold and nothing is landed when the ladder is read. `IDEAS.md [MESH_SPACE_BAKE]` is the card that makes a derived occlusion real; until it lands, the cavity field is caller-supplied evidence exactly as the age field is.
- Law: the cavity field carries the CAVITY scalar — `1.0` the fully occluded crevice — while the `set#TEXTURE_SET` `occlusion` channel stores VISIBILITY (`filter#PLANE_OP` deposits `open/rays` and the row's own neutral is `1.0` unoccluded), so an occlusion plane crosses into a cavity field through the landed `RemapCurve.Levels.Invert` row and never by a raw bind; the raw bind ages every `Crevice` effect on the open face and reads as a plausible plane rather than as a fault. An ABSENT cavity field reads `1.0` because a `Crevice` effect with no cavity evidence is the uniform aging the ladder already spells, while `0.0` would silently delete every crevice effect and run every exposed one at full age.
- Entry: `public static Fin<PressPlan> Of(PressPlanDraft draft, PressSubject subject, Op key)` is the ONE plan admission — extent, layer law, binding uniqueness, pack membership, format width, display egress, ladder rungs, subject arity, ladder cell budget, tile-guide coverage, and backend lowerability all gate here so the bake fold itself carries no re-check; the subject enters the admission because arity, cell budget, and lowerability are facts of the SUBJECT against the bindings, and deferring them to dispatch means a caller learns the veto after renting a device; a cyclic `Derived.From` chain refuses here through the roster-bounded depth walk, so the bake fold recurses on a proven-acyclic roster. `PlanKey` is the canonical content key the receipt records and the cache keys on.
- Packages: `set#TEXTURE_SET` (composed — `TextureChannel`/`ChannelPack`/`LayerLaw`/`TextureSet`/`SinkSlot`), `plane#TEXTURE_PLANE` (composed — `PlaneFormat`/`PlaneQuantity`/`MipPolicy`/`AlphaMode`), `filter#PLANE_OP` (composed — `PlaneOp` the post chain), `tile#TILE_SYNTH` (composed — `TilePolicy` the in-fold tiling request), `Rasm.Materials.Appearance.Graph` (composed — `MaterialGraph`/`MaterialParameters`), `Rasm.Materials.Appearance.Surface` (composed — `ConductorMetal`, `ToneOperator`/`DisplayEncoding` the display egress rows), `Rasm.Materials.Appearance.Texture` (composed — `TextureSource`/`SamplerState`/`UvFrame`), `Rasm.Materials.Appearance.Weathering` (composed — `WeatheringDose`/`AgeParameter`), `Rasm.Materials.Appearance.Finish` (composed — `FinishKind`/`FinishLayer`/`Pigment` the mix subject's own vocabulary), `Rasm.Element.Composition` (the SEAM `MaterialId`), `Rasm` (project — `ContentHash.Of` the one identity entry, `Dimension`, `UnitInterval`, `Op`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new bake subject is one `PressSubject` case with its `PressProgram` arm; a new execution lane is one `PressBackend` row carrying its authority column; a new per-channel request knob is one `ChannelBinding` column; a new quantized axis is one `LadderRungs` column read by the ladder that owns it, priced against the cell product it multiplies. There is NO `BakeGraph`/`BakeField`/`BakeSlab`/`BakeMix` family — the subject's own case discriminates, and a caller holding a subject calls one entry, which is why a dome and a chart set land as cases rather than as a second engine each.
- Boundary: `PressBackend` carries `ContentAuthoritative` as a ROW COLUMN rather than as a caller flag, so the content-identity law is data the plan admission reads and `[04]-[PRESS_RECEIPT]` enforces at the type level. Mip policy is a per-binding `Option<MipPolicy>` defaulting to the channel row's own law and spelling `MipPolicy.None` for a single-level plane — a plan-level `Mips` boolean beside a per-binding override is one knob selecting between two bodies, and the row already carries the answer. The plan key is `ContentHash.Of` over the plan's canonical bytes — extent, layer law, the ordered binding rows with their channels, formats, resolved mip policies, pack keys, post chains, and display egress rows, the backend key, the seed, the alpha mode, the height scale, the three `LadderRungs` columns, the tile policy, and the subject's own `UvFrame` digest — and it EXCLUDES the material id and the conductor, which name the subject rather than the bake, so two materials pressed under one plan share a plan key and the receipt separates them by graph key. The UV frame is the one subject-borne column the key ADMITS, and the discriminant is what the column does: a material id and a conductor NAME the subject, while a `UvFrame` offset, scale, or rotation SHAPES the bake — `texture#TEXTURE_UV` `TextureUv.Sample` applies it before the source dispatch, so a re-tiled bake is different bytes, and a frame outside the preimage is a cached plane a second tiling silently inherits. `UvFrame.Digest` reads EMPTY at identity by its owner's construction, so an untransformed bind keys byte-identically to a plan that never knew the axis existed and landing the column re-keys no blob already addressed. `Layers` and `LayerLaw` ride the plan so a cube map, a flipbook, and a volume are one bake shape at different rows; a UDIM set is N plans sharing a key, never one plan carrying a tile list, because a UDIM tile is an independent extent whose planes address independently — the per-tile products assemble at `set#TEXTURE_SET` `UdimSheet.Of`, the one owner proving the tiles agree. A binding naming a channel already inside a requested pack REFUSES at admission — the pack owns those slots and a standalone duplicate keys the set twice for one field — and a binding whose `Format` carries fewer components than its channel declares refuses for the same reason `set#TEXTURE_SET` refuses it later: a three-component normal in a two-component plane is a reconstruction the sampler cannot invert without evidence the plane does not carry. A `Source` subject binds EXACTLY ONE channel, because a procedural field has one value and a second bound channel would silently receive its neutral; a `Mix` subject binds one weight field PER PIGMENT, because a mix whose simplex is short one axis resolves a pigment nothing weights; a `Tile` policy whose guide channel no binding produces refuses, because the synthesizer would rail on a set that admitted cleanly. A binding whose channel carries an OPEN photometric scale — `PlaneQuantity.Light` beside a `ChannelUnit` other than `none`, which is `emission_luminance` alone on the landed roster — stored in a `PlaneFormat` the plane page reports `Normalized` with NO `Display` egress refuses at admission: an unbounded cd/m² value hard-clips at unity in a unorm lane with no tone curve, and a clipped emission plane is indistinguishable from an authored one downstream. The lane test reads `plane#PLANE_FORMAT` `Normalized`, the estate's one unorm-versus-float discriminant, because the kernel `ChannelDtype` roster the format seats onto carries no normalization column of its own. The `webgpu` backend gates TWO independent facts at admission, not at dispatch, and the split is what keeps each honest: the SUBJECT's lowerability is a fact of the subject alone, and the EXTENT is measured against `PressBackend.TexelCeiling`, the conformance FLOOR every device grants, so a bake no device could run refuses before one is rented. That ceiling is arithmetic, not a guess: `134217728` is exactly the guaranteed minimum for `maxStorageBufferBindingSize`, the lane binds ONE storage buffer per plane, and `134217728 / 16 = 8388608` texels — so a square accelerator preview tops out at `2048²` and a `4096²` request refuses at admission rather than at dispatch. A plan clearing the floor can still exceed what a particular adapter negotiated, and that refusal belongs at `gpu#PRESS_DEVICE`'s dispatch gate, which reads the device's own `DeviceGetLimits` block and quotes the granted value; collapsing the two would either rent a device to answer a plan question or assert a ceiling nothing measured. Lowerability itself: a `Source` subject over an `Image` case or a `Triplanar` whose projected source is not a solid `Noise` (the three-plane 2D blend has no kernel arm), or a `Slab`, `Mix`, or `MeshSpace` subject, has no kernel row on `gpu#WGSL_KERNEL` and refuses with the offending case named, so a caller learns the veto before renting a device rather than after. A `Graph` subject LOWERS as a KERNEL CHAIN — one dispatch per node in the compiled topological order over `gpu#KERNEL_CHAIN`'s ping-ponged slot pool, a field kernel per procedural `Texture` node, `mathFold` per `Math` node, `mixFold` per `Mix` node — and its verdict is the ALLOCATOR's rather than a case test: the chain plans by linear-scan live-range analysis over that order, its slot count is the DAG's maximum live width, and `slots × extent × 16` admits against the declared footprint or the accelerator refuses with the slot count and the budget named. Its refusals are NODE-grained, so a caller learns which node vetoed rather than that "the graph" did, and a refused chain refuses the accelerator alone — the CPU lane is content-authoritative anyway, so the recourse costs throughput and nothing else. The two LADDER subjects sit outside the question entirely: each compiles CPU-admitted cells a GPU arm could only rebuild at `f32`, forking the key the veto holds. A `Noise` source — planar OR solid — and a `Triplanar` over a solid `Noise` lower to `noiseField`: the solid family rides the row's `dimension` column, a triplanar's three planes sample one world point for a solid projected noise so the blend IS the 3D field and the world scale folds into the frequency word, and the widening moves no identity law — the accelerator product stays a `Preview`, CPU bytes stay canonical.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;                       // CultureInfo (the canonical plan projection)
using System.Text;                                // Encoding.UTF8 (the TOTAL preimage projection)
using System.Threading;                           // CancellationToken, Interlocked — the cancel rail and the fault tally
using CommunityToolkit.HighPerformance;           // Memory2D/Span2D/ReadOnlySpan2D — the staging plane views
using CommunityToolkit.HighPerformance.Buffers;   // MemoryOwner/SpanOwner — the pooled arenas the band fold rents
using CommunityToolkit.HighPerformance.Helpers;   // ParallelHelper, IAction — the allocation-free band partition
using LanguageExt;                                // Seq, Option, Fin, HashMap
using Rasm.Domain;                                // Op, ContentHash, Deterministic, ValidityClaim, IValidityEvidence
using Rasm.Drawing;                               // ChannelDtype — the kernel storage-type roster PlaneFormat seats onto
using Rasm.Element.Composition;                   // MaterialId — the SEAM identity
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault (band 2450), RgbSpectrum (the display-egress grade carrier)
using Rasm.Materials.Appearance;                  // Weathering, WeatheringDose, AgeParameter, Finish, FinishKind, FinishLayer — the folder-root appearance operators
using Rasm.Materials.Appearance.Graph;            // MaterialGraph, CompiledGraph (+ its Order/Operands compiled-sort reads the chain lowering walks), AppearanceNode, MaterialParameters, ShadePoint, SurfaceShade, PortValue, GraphContext
using Rasm.Materials.Appearance.Surface;          // OpenPbrSurface, ConductorMetal, ToneMap, ToneOperator, DisplayEncoding
using Rasm.Materials.Appearance.Texture;          // TextureSource, TextureUv, UvSample, SamplerState, UvFrame, ShadeVec4
using Rasm.Numerics;                              // Dimension, UnitInterval
using Rhino.Geometry;                             // Point3d, Vector3d — the shade point's host geometry edge
using Thinktecture;
using Rasm.Materials.Appearance.Finish;           // FinishPigment — the closed row FinishMix.Of admits, carrying its own Pigment reflectance
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// ContentAuthoritative is the content-identity law as ROW DATA: the plan admission reads it and the
// PressProduct union enforces it at the type level, so no caller flag can promote a GPU plane to a key.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PressBackend {
    public static readonly PressBackend Cpu    = new("cpu",    contentAuthoritative: true,  texelCeiling: long.MaxValue);
    public static readonly PressBackend WebGpu = new("webgpu", contentAuthoritative: false, texelCeiling: LoweredFloor / BytesPerTexel);
    public bool ContentAuthoritative { get; }

    // The extent a lane can lower, as a ROW rather than as a device read: gpu#PRESS_DEVICE owns the negotiated
    // ceilings and this page may not spell a WebGPU type, so the accelerator row carries the CONFORMANCE FLOOR
    // every device grants and the plan refuses a bake no device could run before renting one. A device granting
    // more than the floor still refuses at gpu#PRESS_DEVICE's own dispatch gate, which quotes the granted value;
    // the two gates answer different questions and neither stands for the other.
    public long TexelCeiling { get; }

    // Sixteen bytes per texel is the storage arrangement gpu#WGSL_KERNEL declares — four f32 lanes per RGBA
    // texel — so the texel ceiling and the buffer floor are one fact read at two scales.
    const long BytesPerTexel = 16;

    // 134217728 is EXACTLY the WebGPU conformance minimum for maxStorageBufferBindingSize, the guaranteed floor
    // every conformant device grants. The lane binds ONE storage buffer per plane, never a split, so the whole
    // plane must fit that binding and the plan refuses an oversize extent at admission rather than discovering it
    // at dispatch — which is what makes a split binding unreachable rather than merely unimplemented. The
    // arithmetic is the whole ceiling: 134217728 / 16 = 8388608 texels, so a square preview tops out at 2048².
    const long LoweredFloor = 134_217_728;
}

// Graph varies the five sink columns per texel and carries every other channel from its constant row; Slab varies
// the WHOLE OpenPBR vector through the aging trajectory an age FIELD and a cavity FIELD sample per texel; Mix
// varies it through the pigment-weight simplex one field per pigment samples; Source bakes one procedural field
// into one channel. The four are distinct evaluation shapes, not four names for one — and every spatial input is
// a TextureSource rather than a channel because nothing is landed when a ladder is read, Compile being the first
// generator of the fold. The cavity field is the scalar each WeatheringEffect's own CavityResponse maps to its
// age multiplier; an ABSENT field reads 1.0 — full cavity — because a Crevice effect with no cavity evidence is
// the uniform aging the ladder already spells, while 0.0 would silently delete every crevice effect and run every
// exposed one at full age. Both spatial fields ride the subject's ONE SamplerState, exactly as the age field does.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressSubject {
    private PressSubject() { }
    public sealed record Graph(MaterialGraph Program, MaterialParameters Row, ConductorMetal Conductor) : PressSubject;
    public sealed record Source(TextureSource Field, SamplerState Sampler, TextureChannel Target) : PressSubject;
    // CurvatureField is the second exposure axis beside CavityField and it is a SEPARATE field because the two are
    // independent: a crevice can sit on a convex arris and a gutter can be both concave and open, so one occlusion
    // scalar cannot answer both and a `Convex` effect row on a cavity-only surface is byte-identical to `Exposed`.
    // An ABSENT curvature field reads 0.0 — the flat extreme, at which every curvature-keyed row scales at unity
    // and the axis collapses to its own middle column, so a caller who never heard of the axis gets the aging it
    // always got. That is the same degradation law the cavity field takes, one axis over.
    public sealed record Slab(MaterialParameters Row, ConductorMetal Conductor, Seq<WeatheringDose> Aging,
        Option<TextureSource> AgeField, Option<TextureSource> CavityField, Option<TextureSource> CurvatureField,
        SamplerState Sampler) : PressSubject;
    public sealed record Mix(FinishKind Kind, Seq<FinishPigment> Pigments, Seq<TextureSource> WeightFields,
        Seq<FinishLayer> Stack, Option<MaterialParameters> Substrate, ConductorMetal Conductor, SamplerState Sampler) : PressSubject;

    // Sky bakes a synthesized DOME. The case carries a RADIANCE CLOSURE and names no sky type at all: the
    // appearance frontier is S2 and this owner is S1, so a `SkyModel` may not cross — exactly as `Source` carries
    // a `TextureSource` without the press knowing a noise basis. The frontier resolves its own model, its own
    // turbidity, and its own solar position, and hands down a function from a world direction to spectral
    // radiance; the press supplies the direction its `LayerLaw.CubeFaces` correspondence already derives per
    // texel, so a dome inherits the partitioning, cancellation, receipt, and accelerator lane every other bake has
    // instead of carrying a sweep of its own. The DIFFUSE field alone crosses here — the solar disc rides the
    // light row's own sampling arm, because folding disc radiance into the dome double-counts the sun.
    public sealed record Sky(Func<Vector3d, RgbSpectrum> Radiance, TextureChannel Target) : PressSubject;

    // MeshSpace bakes a SURFACE's own texture space. The chart set crosses as DATA — the kernel's already
    // flattened `ChartAtlas` product lowered to per-texel surface evidence — so the host-neutral boundary holds
    // exactly as it does everywhere else on this page: no host mesh type enters, no tessellator runs here, and
    // the press consumes a value the kernel produced. That is what makes occlusion, thickness, and curvature
    // MEASURED against real geometry rather than approximated off a height plane, and it is why the derived rows'
    // height-field folds stay the FALLBACK origin rather than being deleted — a slab subject still has no body to
    // trace against, and its approximations remain the honest answer for the subject that has no geometry.
    public sealed record MeshSpace(
        ReadOnlyMemory<ChartTexel> Charts, Dimension ChartWidth, MaterialParameters Row, ConductorMetal Conductor,
        double RayDistance, int Rays, int GutterRings) : PressSubject;

    // The one subject-borne fact the plan key admits: a UvFrame SHAPES the bake where a material id and a conductor
    // merely NAME the subject, so the digest crosses into the preimage while the naming columns stay excluded. A
    // Graph subject samples through its own node closures and declares no press-level frame.
    public Option<UvFrame> Frame => Switch(
        graph:     static _ => Option<UvFrame>.None,
        source:    static s => Some(s.Sampler.Frame),
        slab:      static b => Some(b.Sampler.Frame),
        mix:       static m => Some(m.Sampler.Frame),
        // A dome addresses a DIRECTION and a chart set addresses its own parameterization; neither reads a UV
        // frame at all, so both declare absence rather than a synthetic identity a preimage would then carry.
        sky:       static _ => Option<UvFrame>.None,
        meshSpace: static _ => Option<UvFrame>.None);

    // The case tag every refusal quotes, derived from the union's OWN dispatch. A GetType().Name read answers
    // the nested record's CLR name, so a rename silently re-spells the token inside every fault detail an
    // operator greps and every fixture that pins one, while the Switch arm is the same closed vocabulary the
    // plan gates already discriminate on and the generator proves total.
    public string Case => Switch(
        graph:     static _ => "graph",
        source:    static _ => "source",
        slab:      static _ => "slab",
        mix:       static _ => "mix",
        sky:       static _ => "sky",
        meshSpace: static _ => "meshSpace");
}

// One texel of an already-flattened chart, the DATA a mesh-space bake consumes in place of a mesh. Position and
// Frame are the surface point and its shading basis; Coverage separates a texel inside a chart from a gutter
// texel the dilation will fill, so the bake never traces from a point no chart occupies. This is the whole of the
// geometry that crosses into this folder, and it crosses as measured values the kernel's own flatten produced —
// which is exactly the confinement that keeps the host-neutral boundary intact while the fields become real.
public readonly record struct ChartTexel(Point3d Position, Vector3d Normal, Vector3d Tangent, bool Coverage);

// --- [MODELS] ------------------------------------------------------------------------------
// The quantization carrier every ladder reads: three declared axes on ONE plan column rather than three loose int
// knobs an admission would guard three times and a preimage would append three times. Each axis is the rung count
// over its own [0,1] parameter, Degenerate names the first axis below the two-rung floor a lerp needs, and Digest
// is the one preimage fragment — a quantization that moves the produced bytes enters the plan key exactly as the
// post chain and the tile fragment do. The canonical row prices the aging product it exists for: 16 age rungs
// against 8 cavity rungs is 128 fallible admissions per press against sixteen million texels at four thousand
// square, which is the same three-orders margin the one-dimensional ladder bought, one dimension wider.
public readonly record struct LadderRungs(int Age, int Cavity, int Curvature, int Mix) {
    // The THIRD AXIS IS PRICED, not assumed. A curvature dimension MULTIPLIES the aging cell product, so the
    // default row buys it at the cheapest rung count that still interpolates — `16 × 8 × 4` is 512 fallible
    // admissions per press against sixteen million texels at four thousand square, still three orders of margin
    // and four times the two-axis cost. `Curvature: 1` is the declared OPT-OUT and it is exempt from the two-rung
    // floor for exactly that reason: a single column means the axis is not sampled at all and the ladder is the
    // two-dimensional one it was, so a caller with no curvature field pays nothing for the axis existing.
    // The per-texel `Apply` escape — evaluating the aged vector at each texel's own exact triple — stays the
    // GROWTH LEG rather than the default: it is a fallible admission per texel, which is the three-orders cost the
    // ladder exists to refuse, and it earns its place only where a caller proves quantization visible.
    public static readonly LadderRungs Default = new(Age: 16, Cavity: 8, Curvature: 4, Mix: 8);
    const int Floor = 2;
    const int OptOut = 1;

    public Option<(string Axis, int Rungs)> Degenerate =>
        Age < Floor ? Some((nameof(Age), Age))
        : Cavity < Floor ? Some((nameof(Cavity), Cavity))
        : Curvature < OptOut ? Some((nameof(Curvature), Curvature))
        : Mix < Floor ? Some((nameof(Mix), Mix))
        : Option<(string, int)>.None;

    // The cell product the plan admission prices against its own ceiling: a third axis is a multiplier, so the
    // budget is read rather than reasoned about at each site that cares.
    public long AgeCells => (long)Age * Cavity * Curvature;

    public string Digest => string.Create(CultureInfo.InvariantCulture, $"rungs|{Age}|{Cavity}|{Curvature}|{Mix}");
}

// The scene-to-display policy a binding declares: the surface#OPENPBR_SLAB ToneOperator row grades the HDR
// radiance to display-linear, the DisplayEncoding row rebases that triple onto the target primaries and reads its
// transfer, and Exposure is the multiplicative stop the curve takes first. It is a binding COLUMN rather than a
// caller convention because the press owns the ONE staging-to-plane crossing, and its digest enters the plan key
// because a graded plane is different bytes under an otherwise identical request.
public readonly record struct DisplayEgress(ToneOperator Operator, DisplayEncoding Encoding, double Exposure) {
    public string Digest => string.Create(CultureInfo.InvariantCulture, $"display|{Operator.Key}|{Encoding.Key}|{Exposure:R}");
}

// Post is the filter#PLANE_OP chain applied to this channel's plane after the shade fold and before the mip
// chain builds — a level, a remap, a blur — so a post-processed plane still carries a coherent pyramid. Mip is
// a typed override of the channel row's own law: absence takes the row, MipPolicy.None spells a flat plane.
// Display is the typed absence that separates a scene-referred plane written raw from one graded for a display
// container: absence is the default for every parameter and reflectance channel, presence is required for an
// open photometric scale narrowed into an integer lane.
public sealed record ChannelBinding(
    TextureChannel Channel, PlaneFormat Format, Option<MipPolicy> Mip, Option<ChannelPack> Pack,
    Seq<PlaneOp> Post, Option<DisplayEgress> Display) {
    public MipPolicy Policy => Mip.IfNone(() => Channel.Mip);
}

public sealed record PressPlanDraft(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law, Seq<ChannelBinding> Bindings,
    PressBackend Backend, ulong Seed, AlphaMode Alpha, Option<double> HeightScaleMm, LadderRungs Rungs,
    Option<TilePolicy> Tile, Option<MaterialId> Material, Option<ConductorMetal> Conductor);

public sealed record PressPlan(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law, Seq<ChannelBinding> Bindings,
    PressBackend Backend, ulong Seed, AlphaMode Alpha, Option<double> HeightScaleMm, LadderRungs Rungs,
    Option<TilePolicy> Tile, Option<MaterialId> Material, Option<ConductorMetal> Conductor, UInt128 PlanKey) {

    // The ONE plan admission. Binding order is DERIVED here — sources before derivations, normals before the
    // roughness rows whose mip fold consumes their variance — so the fold never re-sorts and a caller never
    // sequences. Subject arity gates here too, so a Source plan cannot silently neutral-fill a second channel.
    public static Fin<PressPlan> Of(PressPlanDraft draft, PressSubject subject, Op key) =>
        Gates(draft, subject, key)
            .Fold(Fin.Succ(unit), static (admitted, gate) => admitted.Bind(_ => gate()))
            .Map(_ => Order(draft.Bindings))
            .Map(ordered => new PressPlan(draft.Width, draft.Height, draft.Layers, draft.Law, ordered, draft.Backend,
                draft.Seed, draft.Alpha, draft.HeightScaleMm, draft.Rungs, draft.Tile, draft.Material, draft.Conductor,
                Mint(draft, ordered, subject)));

    // --- [PLAN_ADMISSION]
    // The gates in ORDINAL order — the sequence IS the ordinal, and the fold stops at the first refusal, so a
    // caller always reads the narrowest true statement about its draft. Order is earned rather than incidental:
    // structural shape before roster walks, the roster's own acyclicity before anything that walks it, the
    // subject before the bindings it has to satisfy, and the lane gates last because a draft malformed for every
    // backend must not report as a backend problem. A new gate is one row at the position its specificity earns;
    // the positional bind ladder it replaces numbered its discards instead of naming its steps.
    static Seq<Func<Fin<Unit>>> Gates(PressPlanDraft draft, PressSubject subject, Op key) =>
        Seq<Func<Fin<Unit>>>(
            () => guard(!draft.Bindings.IsEmpty, MaterialFault.Parameter(key, "<press-plan-no-bindings>")),
            () => guard(draft.Law.Admits(draft.Layers.Value), MaterialFault.Parameter(key, $"<layer-law-rejects:{draft.Law.Key}:{draft.Layers.Value}>")),
            () => guard(draft.Bindings.Map(static b => b.Channel).Distinct().Count() == draft.Bindings.Count, MaterialFault.Parameter(key, "<press-binding-duplicate-channel>")),
            () => draft.Rungs.Degenerate
                .Map(bad => Fin.Fail<Unit>(MaterialFault.Parameter(key, $"<ladder-degenerate:{bad.Axis}:{bad.Rungs}>")))
                .IfNone(Fin.Succ(unit)),
            // A roster whose Derived.From chain cycles has no fold order — the bounded Depth walk names it here,
            // where an unbounded recursion inside the bake would stack-overflow instead of railing.
            () => guard(draft.Bindings.ForAll(static b => Depth(b.Channel) >= 0), MaterialFault.Parameter(key, "<derived-origin-cycle>")),
            () => AdmitSubject(draft, subject, key),
            () => draft.Tile
                .Map(policy => guard(draft.Bindings.Exists(b => b.Channel == policy.Guide), MaterialFault.Parameter(key, $"<tile-guide-unbound:{policy.Guide.Key}>")).ToFin())
                .IfNone(Fin.Succ(unit)),
            () => draft.Bindings.Fold(Fin.Succ(unit), (acc, b) => acc.Bind(_ => AdmitBinding(draft, b, key))),
            () => AdmitLane(draft, subject, key));

    // The accelerator lane vetoes on TWO independent facts and states both here, before a device is rented: the
    // SUBJECT must have a kernel chain to lower onto, and the EXTENT must fit the lane's own conformance-floor
    // ceiling. A plan clearing the floor can still exceed what a particular device negotiated, and that refusal
    // belongs at gpu#PRESS_DEVICE's dispatch gate where the granted value exists to quote — the floor gate
    // refuses what NO device runs, the device gate refuses what THIS one will not, and collapsing them into one
    // site would either rent a device to answer a plan question or assert a ceiling nothing measured.
    static Fin<Unit> AdmitLane(PressPlanDraft draft, PressSubject subject, Op key) =>
        draft.Backend.ContentAuthoritative
            ? Fin.Succ(unit)
            : !Lowerable(subject)
                ? Fin.Fail<Unit>(MaterialFault.Parameter(key, $"<gpu-unlowerable-subject:{subject.Case}>"))
                : (long)draft.Width.Value * draft.Height.Value * draft.Layers.Value is var texels && texels > draft.Backend.TexelCeiling
                    ? Fin.Fail<Unit>(MaterialFault.Parameter(key, $"<gpu-extent-over-floor:{texels}:{draft.Backend.TexelCeiling}>"))
                    // A Graph subject carries a THIRD fact past lowerability and extent: its chain's slot pool is
                    // resident all at once, so the footprint gates here beside them rather than at a dispatch that
                    // would already have rented a device and lowered every node.
                    : subject is PressSubject.Graph graph
                        ? AdmitChain(draft, graph, key)
                        : Fin.Succ(unit);

    // A procedural field has ONE value: a second bound channel would receive its neutral and read as baked. A mix
    // needs one weight field PER PIGMENT — a short simplex resolves a pigment nothing weights — and its ladder is
    // the barycentric lattice whose cell count grows combinatorially in the pigment count, so the CELL BUDGET is
    // the gate rather than the rung count alone: nineteen pigments at eight rungs is not a finer bake, it is an
    // unbuildable one, and naming it here is what keeps Compile total over an admitted plan.
    static Fin<Unit> AdmitSubject(PressPlanDraft draft, PressSubject subject, Op key) =>
        subject.Switch(
            state:  (Draft: draft, Key: key),
            graph:  static (s, _) => Fin.Succ(unit),
            slab:   static (s, _) => Fin.Succ(unit),
            source: static (s, f) => s.Draft.Bindings.Count is 1 && s.Draft.Bindings[0].Channel == f.Target
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(MaterialFault.Parameter(s.Key, $"<source-subject-binds-one-channel:{f.Target.Key}>")),
            // A dome has ONE radiance per direction, so it binds one channel exactly as a procedural field does —
            // a second bound channel would receive its neutral and read as baked. The layer law is the gate that
            // matters beyond arity: the correspondence a texel's direction derives from is `CubeFaces`, so a dome
            // pressed under any other law has no direction to evaluate and refuses here rather than at the band.
            sky:    static (s, k) => s.Draft.Bindings.Count is 1 && s.Draft.Bindings[0].Channel == k.Target
                ? guard(s.Draft.Law == LayerLaw.CubeFaces,
                    MaterialFault.Parameter(s.Key, $"<sky-subject-layer-law:{s.Draft.Law.Key}>")).ToFin()
                : Fin.Fail<Unit>(MaterialFault.Parameter(s.Key, $"<sky-subject-binds-one-channel:{k.Target.Key}>")),
            // The chart run must cover the plan's own grid exactly: a short run leaves texels with no surface
            // point to trace from, and a long one carries charts the bake will never address — both are a chart
            // set flattened against a different extent than the one being pressed.
            meshSpace: static (s, m) => (long)m.Charts.Length == (long)s.Draft.Width.Value * s.Draft.Height.Value
                && m.ChartWidth == s.Draft.Width
                ? guard(m.Rays > 0 && m.RayDistance > 0.0 && m.GutterRings > 0,
                    MaterialFault.Parameter(s.Key, $"<mesh-space-cast:{m.Rays}:{m.RayDistance:R}:{m.GutterRings}>")).ToFin()
                : Fin.Fail<Unit>(MaterialFault.Parameter(s.Key, $"<mesh-space-chart-extent:{m.Charts.Length}>")),
            mix:    static (s, m) => m.Pigments.IsEmpty || m.Pigments.Count != m.WeightFields.Count
                ? Fin.Fail<Unit>(MaterialFault.Parameter(s.Key, $"<mix-subject-weight-arity:{m.Pigments.Count}!={m.WeightFields.Count}>"))
                : MixLadder.Budget(s.Draft.Rungs.Mix, m.Pigments.Count) is var cells && cells <= MixLadder.CellCeiling
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(MaterialFault.Parameter(s.Key, $"<mix-ladder-over-budget:{cells}:{MixLadder.CellCeiling}>")));

    // An OPEN photometric scale — PlaneQuantity.Light beside a declared ChannelUnit — stored in a NORMALIZED lane
    // with no Display egress hard-clips at unity, and a clipped emission plane is indistinguishable downstream from
    // an authored one. A [0,1] weight or reflectance channel is unaffected: its Light quantity bounds at unity by
    // its own domain, which is why the unit column and not the quantity alone carries the discriminant. The lane
    // test reads plane#PLANE_FORMAT's own Normalized member — the ONE unorm-versus-float discriminant the estate
    // holds — never a depth-row probe, because the kernel ChannelDtype roster carries no normalization column.
    static Fin<Unit> AdmitBinding(PressPlanDraft draft, ChannelBinding binding, Op key) =>
        from _ in guard(binding.Format.Components >= binding.Channel.Components, MaterialFault.Parameter(key, $"<binding-format-narrow:{binding.Channel.Key}>"))
        from __ in guard(binding.Pack.Map(p => p.Slots.Contains(binding.Channel)).IfNone(true), MaterialFault.Parameter(key, $"<binding-pack-foreign:{binding.Channel.Key}>"))
        from ___ in guard(binding.Pack.IsSome || !draft.Bindings.Exists(other => other.Pack.Map(p => p.Slots.Contains(binding.Channel)).IfNone(false)),
                          MaterialFault.Parameter(key, $"<binding-both-packed-and-standalone:{binding.Channel.Key}>"))
        from ____ in guard(binding.Display.IsSome
                           || binding.Channel.Transfer.Quantity != PlaneQuantity.Light
                           || binding.Channel.Unit == ChannelUnit.None
                           || !binding.Format.Normalized,
                          MaterialFault.Parameter(key, $"<binding-open-light-clipped:{binding.Channel.Key}:{binding.Format.Key}>"))
        select unit;

    // Only a procedural Source over a field with a kernel arm reaches the accelerator: noise lowers at BOTH
    // dimensions (solid rides the noiseField dimension column), a triplanar over a solid noise collapses to
    // that same 3D field (its three planes sample one world point), and the graph, slab, mix, image, and
    // planar-triplanar arms have no kernel chain — the veto is stated once here rather than discovered after
    // a device is rented. The two ladder subjects are structurally unlowerable rather than merely unimplemented:
    // each compiles a table of CPU-admitted vectors, and a GPU arm rebuilding it at f32 forks the very key the
    // content-identity veto exists to hold.
    // A SKY lowers too: `gpu#WGSL_KERNEL` `equirectToCube` carries the face correspondence whole, so a preview dome
    // is a real image of the field the CPU mint will produce rather than a substitute for it. A GRAPH lowers as a
    // KERNEL CHAIN — one dispatch per node in the compiled order over `gpu#KERNEL_CHAIN`'s ping-ponged slot pool —
    // and its verdict is the ALLOCATOR's, not a case test, which is why it answers on its own arm below.
    // MESH-SPACE does NOT lower and its refusal is structural rather than unimplemented: the cast traces an
    // arbitrary chart set at f64, and a GPU arm rebuilding that at f32 forks the very key the content-identity
    // veto exists to hold, exactly as the two ladder subjects do.
    static bool Lowerable(PressSubject subject) =>
        subject is PressSubject.Sky or PressSubject.Graph
        || (subject is PressSubject.Source source && source.Field switch {
            TextureSource.Noise or TextureSource.Checker or TextureSource.Gradient => true,
            TextureSource.Triplanar { Projected: TextureSource.Noise { Solid: true } } => true,
            _ => false,
        });

    // THE GRAPH ARM of the accelerator verdict, and it is the THIRD instance of this page's own two-gate law
    // rather than a new kind of gate. What a PLAN can answer without renting a device is answered here: every node
    // of the graph carries a `WgslKernel` row to lower onto, and the resulting chain's SLOT FOOTPRINT — the
    // allocator's high-water mark times the extent's plane bytes — admits against the declared budget. What only a
    // COMPILE can answer waits for one, because the allocator's live-range scan runs over the TOPOLOGICAL ORDER
    // and that order is the compile's own product: re-deriving it here would compile the graph twice per press.
    // A refusal refuses the ACCELERATOR and never the bake — the caller's recourse is the CPU lane, which is
    // content-authoritative anyway, so falling back costs throughput and nothing else.
    static Fin<Unit> AdmitChain(PressPlanDraft draft, PressSubject.Graph subject, Op key) =>
        ChainNodes(subject, key)
            .Bind(nodes => ChainPlan.Of(nodes, key))
            .Bind(plan => plan.Admits((long)draft.Width.Value * draft.Height.Value * draft.Layers.Value, key));

    // THE GRAPH LOWERING: one `ChainNode` per node of the compiled program, in the compiled TOPOLOGICAL ORDER, so
    // the sequence the allocator scans is the order the evaluator already proved acyclic and answerable. A
    // procedural `Texture` node takes its field kernel, a `Math` node `mathFold`, a `Mix` node `mixFold`, and each
    // node's operands are the SLOT-FREE node indices of the ports it consumes — the same `Produces`/consumes
    // relation `MaterialGraph.Compile`'s own answerability proof reads, which is why no second dependency map
    // exists to drift from it. A node with no kernel row refuses BY NAME here, so a caller learns which node
    // vetoed the accelerator rather than that "the graph" did.
    // Uniform words ride the one `gpu#PRESS_DEVICE` `KernelUniform` writer in each row's declared order, exactly as
    // the single-dispatch `Stage` does — a chain changes what a dispatch reads FROM, never how a block is written.
    static Fin<Seq<ChainNode>> ChainNodes(PressSubject.Graph subject, Op key) =>
        subject.Program.Compile(key).Bind(compiled =>
            compiled.Order.Fold(Fin.Succ(Seq<ChainNode>()), (acc, node) =>
                acc.Bind(built => ChainKernel(node, key)
                    .Map(kernel => built.Add(new ChainNode(kernel, compiled.Operands(node), ChainWords(node, kernel)))))));

    // The one node-class-to-kernel-row correspondence. A `Graph` subject reaching the accelerator has already
    // passed `Lowerable`, so this arm's refusals name the NODE rather than the subject — an image-backed texture
    // node and every node class with no kernel row veto here, one name at a time.
    static Fin<WgslKernel> ChainKernel(AppearanceNode node, Op key) =>
        node switch {
            AppearanceNode.Texture { Source: TextureSource.Noise } => Fin.Succ(WgslKernel.NoiseField),
            AppearanceNode.Texture { Source: TextureSource.Checker } => Fin.Succ(WgslKernel.CheckerField),
            AppearanceNode.Texture { Source: TextureSource.Gradient } => Fin.Succ(WgslKernel.GradientField),
            AppearanceNode.Math => Fin.Succ(WgslKernel.MathFold),
            AppearanceNode.Mix => Fin.Succ(WgslKernel.MixFold),
            _ => MaterialFault.Parameter(key, $"<graph-node-unlowerable:{node.Kind}>"),
        };

    // The per-node uniform block, written through the ONE `gpu#PRESS_DEVICE` `KernelUniform` word writer in the
    // kernel row's own declared order — the same writer `Stage` uses for a single dispatch and the same one every
    // golden fixture builds its block with, so a chain step and a fixture cannot disagree about layout. A chain
    // changes what a dispatch reads FROM; it never changes how a block is written, which is why no second word
    // order exists here and a float-typed carrier writing into a `u32` slot stays as unrepresentable as it is
    // everywhere else on this rail.
    static ReadOnlyMemory<uint> ChainWords(AppearanceNode node, WgslKernel kernel);

    // Three ordering keys, one sort: derivation depth (a source before what folds from it), pair dependency (a
    // normal before the roughness whose variance fold reads it), then the roster ordinal for determinism.
    static Seq<ChannelBinding> Order(Seq<ChannelBinding> bindings) =>
        toSeq(bindings
            .OrderBy(static b => Depth(b.Channel))
            .ThenBy(static b => b.Channel.Pair.IsSome)
            .ThenBy(static b => b.Channel.Ordinal));

    // Bounded by the roster's own count, so a cyclic Derived.From chain returns the -1 sentinel the admission
    // gate rails on instead of recursing without a floor.
    static int Depth(TextureChannel channel) => Depth(channel, walked: 0);
    static int Depth(TextureChannel channel, int walked) =>
        walked > TextureChannel.Items.Count ? -1
        : channel.Origin switch {
            ChannelOrigin.Derived derived when TextureChannel.TryGet(derived.From, out TextureChannel? from) =>
                Depth(from!, walked + 1) is var depth && depth >= 0 ? depth + 1 : -1,
            ChannelOrigin.Derived => 1,
            _ => 0,
        };

    // The plan key names the BAKE, never the subject: material id and conductor are excluded so two materials
    // pressed under one plan share a plan key, and the receipt's graph key is what separates them. The subject's
    // UvFrame is the ONE exception and the discriminant is what the column DOES — a naming column identifies the
    // subject, a framing column moves the texels, and TextureUv.Sample applies the frame before every source
    // dispatch, so a re-tiled bake outside the preimage is a cached plane the second tiling silently inherits.
    // Every preimage piece appends as a WHOLE UTF-8 string — a fixed buffer under an unchecked TryWrite could
    // truncate silently and vanish an entry, which is an address fork no diagnostic names — and the POST
    // CHAIN enters whole per op: a count admits two different transform algebras under one key, and a cached
    // plane produced by a different chain is cache poisoning wearing a key. The DISPLAY row enters per binding
    // for the same reason a post op does: a graded plane is different bytes under an identical request. The tile
    // fragment carries EVERY column that moves the tiled bytes or the proof — strategy, guide, overlap, accept
    // score, grade edge, seed, colours — because a column outside the key is a silent quality fork between two
    // "identical" plans, and the ladder digest carries all three rung axes for the same reason.
    static UInt128 Mint(PressPlanDraft draft, Seq<ChannelBinding> ordered, PressSubject subject) =>
        ContentHash.Of((Draft: draft, Ordered: ordered, Frame: subject.Frame), static (source, digest) => {
            void Piece(string text) => digest.Append(Encoding.UTF8.GetBytes(text));
            Piece(string.Create(CultureInfo.InvariantCulture,
                $"{source.Draft.Width.Value}x{source.Draft.Height.Value}x{source.Draft.Layers.Value}|{source.Draft.Law.Key}|{source.Draft.Backend.Key}|{source.Draft.Seed:x16}|{source.Draft.Alpha.Key}"));
            // The height scale takes its OWN FRAMED FRAGMENT, exactly as the rungs, the tile policy, and each
            // display row do — riding inside the extent piece made it the one plan column whose value shared a
            // delimiter run with five unrelated facts, where every other column that moves the produced bytes is
            // separately framed and separately readable. ABSENCE IS SPELLED: a plan that declares no displacement
            // amplitude appends `hs:none` and one declaring 12.5 appends `hs:12.5`, so the two are distinguishable
            // in the address rather than colliding on the zero that used to mean both.
            Piece(string.Create(CultureInfo.InvariantCulture,
                $"hs:{source.Draft.HeightScaleMm.Map(static mm => mm.ToString("R", CultureInfo.InvariantCulture)).IfNone("none")}"));
            Piece(source.Draft.Rungs.Digest);
            foreach (ChannelBinding binding in source.Ordered) {
                Piece(string.Create(CultureInfo.InvariantCulture,
                    $"{binding.Channel.Key}|{binding.Format.Key}|{binding.Policy.Key}|{binding.Pack.Map(static p => p.Key).IfNone(string.Empty)}"));
                // filter#PLANE_OP `Digest` is the canonical per-op spelling: rename-stable case tokens, owned
                // SmartEnum keys, invariant numerics — a ToString fold here re-keyed every cached plane on a
                // case rename.
                foreach (PlaneOp op in binding.Post) { Piece(op.Digest); }
                binding.Display.Iter(egress => Piece(egress.Digest));
            }
            source.Draft.Tile.Iter(policy => Piece(string.Create(CultureInfo.InvariantCulture,
                $"{policy.Strategy.Key}|{policy.Guide.Key}|{policy.Overlap}|{policy.AcceptScore:R}|{policy.GradeEdge}|{policy.Seed:x16}|{policy.WangColors}")));
            source.Frame.Iter(frame => Piece(frame.Digest));
        });
}
```

## [03]-[TEXTURE_PRESS]

- Owner: `TexturePress` the bake fold; `PressProgram` `[Union]` the compiled subject; `AgeLadder` the two-dimensional aged-vector table the `Slab` program reads; `MixLadder` the barycentric weight-simplex table the `Mix` program reads; `PressRows` the struct `IAction` band partition.
- Entry: `public static Fin<PressProduct> Press(PressSubject subject, PressPlan plan, Op key, TimeProvider? clock = null, BakeGovernance governance = default)` is the ONE bake — it compiles the subject once, folds every direct binding, applies each binding's post chain, derives every derived channel from its landed source through the channel's OWN declared fold, composes every requested pack from the landed chains, builds the mip chains against their paired companions, tiles when the plan requests it, and admits the result through `set#TEXTURE_SET` `TextureSet.Of`; the caller composes a `PressProduct` and never orchestrates a stage, and the token cancels between bindings and inside every band onto the kernel cancel rail.
- Packages: `graph#MATERIAL_GRAPH` (composed — `MaterialGraph.Compile` ONCE per press, `CompiledGraph.ShadeSpan` the batched evaluator, `CompiledGraph.ScratchWidth` the per-band scratch the fold rents against, `ShadePoint`, `GraphContext.Tolerant`), CommunityToolkit.HighPerformance (`ParallelHelper.For<TAction>(int, int, in TAction)` over a SEEDED `struct IAction` so the partition allocates nothing, inlines, clamps to the processor count, and carries its state — the unseeded overload default-constructs the action and would lose every field the fold needs; `ParallelHelper.ForEach<TItem, TAction>(Memory<T>, in TAction, minimumActionsPerThread)` over a seeded `struct IRefAction<T>` the pack composition's per-level item fold rides, each worker taking its own `ref` job; `MemoryOwner<T>.Allocate` the per-binding staging arena, `SpanOwner<T>.Allocate` the per-band point/scratch/shade rentals, `Memory2D<T>`/`Span2D<T>` the plane views), `Rasm` (project — `Deterministic.Stream`/`NextUnit` the lane-exact coordinate-keyed per-texel draw), `set#TEXTURE_SET` (composed — `SinkSlot.Read` the per-slot `SurfaceShade` column reader, `ChannelOrigin` the per-channel production law), `filter#PLANE_OP` (composed — `PlaneOp.Apply(TexturePlane, Seq<PlaneOp>, Op, TimeProvider?)` and its `PlaneReceipt`, for every post chain and every derived channel), `tile#TILE_SYNTH` (composed — `TileSynth.Tileify` when the plan carries a policy), `gpu#PRESS_DEVICE` (composed — `PressDevice.Acquire`/`Dispatch` on the accelerator arm), `weathering#WEATHERING` (composed — `Weathering.Apply` at each cell of the age ladder, taking the cell's own `AgeParameter` and `UnitInterval` cavity scalar), `finish#FINISH` (composed — `FinishMix.Of` and `Finish.Resolve` at each cell of the mix ladder), `surface#OPENPBR_SLAB` (composed — `ToneMap.Apply`/`ToneMap.Encode` the one display egress a binding declares).
- Growth: a new evaluation shape is one `PressSubject` case and one `PressProgram` arm; a new post-processing step is one `filter#PLANE_OP` `PlaneOp` on a binding's chain; a new derived channel is one `ChannelOrigin.Derived` row on `set#TEXTURE_CHANNEL` carrying its own fold — the press discovers both the dependency and the operation from the roster and needs no edit.
- Law: the subject compiles ONCE into a `PressProgram` — a graph resolves its topological order into a frozen `CompiledGraph` and its constant OpenPBR vector, a slab builds its whole age ladder, a mix builds its whole simplex ladder, a field captures its sampler, a dome captures its radiance closure, a mesh-space subject resolves its per-channel measure — and the BAND kernel dispatches on that program once per PARTITION rather than once per texel, so a four-thousand-square plane pays four dispatches per core instead of sixteen million. `ShadeSpan` re-enters over the compiled order with the port environment resolved into an INDEX-ADDRESSED scratch whose slot order is that sort, so a plane costs one allocation-free pass per row instead of one immutable-map rebuild per node per texel; the integrator's per-point `Shade` re-enters this same rail over a one-element window, one evaluation law at two grains.
- Law: band parallelism rides `ParallelHelper.For` over a SEEDED `struct IAction` — the seeded overload copies the caller's action into each partition, where the unseeded one default-constructs it and would hand every band an empty program, an empty plan, and a null target. Each band rents its point, scratch, and shade spans ONCE and walks its own rows, so a partition rents once where a per-row action rents per row.
- Law: PER-TEXEL JITTER derives from the TEXEL COORDINATE and the plan seed through `Deterministic.Stream` over the `(x, y, layer, ordinal)` lanes with the FULL 64-bit seed as its own lane, and both axis draws advance that texel-local state through `NextUnit` — never a sequential stream. So a band partition cannot reorder a draw, two channels of one press do not share a jitter sequence, a re-press at one seed is byte-identical at any processor count, and `Position` agrees with the jittered UV so a subject reading both sees one point. The two LINE-INVARIANT reads — the channel ordinal, a frozen-index probe, and the seed cast — hoist out of the texel walk, so what remains inside is arithmetic over a stack-allocated lane span; deriving a texel state by advancing a line state instead would re-transcribe the stream owner's private gamma, which is the deleted form.
- Law: the LAYER axis is the plan's own `LayerLaw` evaluated per line — `cubeFaces` derives the frozen per-face direction into `Position` (the `faceDir` transcription both the CPU bake and the GPU kernel pin to the freeze), `volume` the slab-centre depth coordinate, `frames` the frame coordinate — so the layer rows are real evaluation shapes and a dome reads its direction off the same correspondence its preview kernel does.
- Law: the `Graph` program varies the FIVE sink columns per texel through each slot's own `SinkSlot.Read` projection and reads every other bound channel off its constant lowered vector, because the `graph#MATERIAL_GRAPH` `BsdfOutput` sink carries five columns — so a coat-roughness plane pressed from a graph subject is honestly constant rather than silently mis-projected from base colour.
- Law: the `Slab` program reads a THREE-DIMENSIONAL QUANTIZED AGE LADDER over the `(age, cavity, curvature)` PRODUCT, because the aged vector does not factor through any one scalar: each dose's effective age is its own row's `weathering#WEATHERING` `CavityResponse` scale over the exposure pair, so a dose set mixing a `Crevice` row with an `Exposed` row — or a `Convex` row with a `Concave` one — produces a fold no single parameter indexes, and the fold is order-dependent because each step lerps toward its own terminal. Cavity and curvature are INDEPENDENT axes rather than refinements of one: a crevice can sit on a convex arris, so on a single occlusion scalar `Convex` is byte-identical to `Exposed` and the pair only becomes distinguishable when the second field exists. `Weathering.Apply` runs once per CELL across the plan's own `LadderRungs.AgeCells` quantization and every texel indexes the cell its three fields sampled.
- Law: the THIRD LADDER AXIS IS PRICED. A curvature dimension MULTIPLIES the cell product, so the default row buys it at the cheapest count that still interpolates — `16 × 8 × 4` is 512 fallible admissions per press against sixteen million texels at four thousand square, four times the two-axis cost and still three orders of margin. `Curvature: 1` is the declared OPT-OUT, exempt from the two-rung floor precisely because one column means the axis is not sampled and the ladder is the two-dimensional one it was. The per-texel `Apply` escape — the exact triple at every texel — stays the GROWTH LEG rather than the default, because it is a fallible admission per texel, which is the cost the ladder exists to refuse.
- Law: the `Mix` program reads the SAME shape over the barycentric weight simplex: `Finish.Resolve` runs once per lattice point of the compositions of `LadderRungs.Mix − 1` into the pigment count, each texel's sampled weight vector normalizes and quantizes by largest remainder onto that lattice, and the cell budget gates at admission because the count grows COMBINATORIALLY in the pigment count where the aging product grows as a lattice. Every rung count is a declared plan column entering the plan key rather than a hidden approximation — a caller needing a continuous trajectory presses at a finer ladder, which is one column, not a different fold.
- Law: ABSENT SPATIAL FIELDS DEGRADE TO THE AGING A CALLER WHO NEVER HEARD OF THE AXIS EXPECTS, and each extreme is chosen for that and nothing else: no age field bakes UNAGED at `0.0` (the material as authored, where the silent fully-weathered default was an inversion a caller found only in the render), no cavity field reads `1.0` (the full-cavity extreme at which every `Crevice` row ages at the raw age and the axis collapses to its uniform column, where `0.0` would delete every crevice effect and run every exposed one at full age), and no curvature field reads `0.0` (the flat extreme at which every curvature-keyed row scales at unity).
- Law: the `Sky` subject is a bake like any other. Its radiance closure evaluates at each texel's own world direction and the dome inherits the press engine's partitioning, cancellation, receipt, and accelerator lane instead of carrying a sweep of its own; the DIFFUSE field alone crosses, because the solar disc rides the light row's own sampling arm and folding disc radiance into the dome double-counts the sun the moment the light samples it. It is the one subject beyond `Source` that LOWERS: `equirectToCube` carries the face correspondence whole, so a preview dome images the field the CPU mint will produce, and the CPU mint stays authoritative under the same veto.
- Law: the `MeshSpace` subject makes occlusion, thickness, and curvature MEASURED. Charts cross as DATA — the kernel's already-flattened product lowered to per-texel surface evidence — so no host mesh type enters and no tessellator runs here; a GUTTER texel writes its channel neutral and is traced from nothing, because a ray cast from a point not on the body measures absence rather than occlusion, and the subject's own `Dilate` rings then fill those texels from their own chart so the gutter never bleeds a neighbour's relief through the mip chain. It does NOT lower, and that refusal is structural: an f32 rebuild of an arbitrary chart cast forks the very key the content-identity veto holds. The `filter#PLANE_OP` height-field derivations stay the FALLBACK origin rather than being deleted — a slab subject has no body to trace against, and its approximations remain the honest answer for the subject that has no geometry.
- Law: STAGING IS `ShadeVec4` AND QUANTIZATION IS THE PLANE'S. The fold writes decoded four-lane texels into a `Memory2D<ShadeVec4>` arena and the plane's own row `Write` rail associates alpha, encodes the transfer, and narrows to the binding's `PlaneFormat`, so exactly one quantizer exists in the corpus and a press never encodes a texel itself. A binding declaring a `DisplayEgress` grades its row BEFORE that crossing — `ToneMap.Apply` per texel, then `ToneMap.Encode` over the whole row — so scene-referred radiance reaches an integer lane through the one tone-map owner rather than through the transfer clip a raw narrow performs; a binding declaring none takes its own walk and rents nothing, because a zero-length rental exists only to be skipped. Both lanes cross the same `Fill` rail, so the preview inherits the grade and the two differ in authority alone.
- Law: DERIVED CHANNELS fold AFTER their sources land, through the channel row's OWN `ChannelOrigin.Derived.Fold` step composed BEFORE the binding's post chain, so the derivation is roster data rather than a caller-supplied chain the press hopes contains the right operation; a source channel a plan did not request is produced as an intermediate and dropped unless bound. A PAIRED mip policy resolves its companion from the landed map and DOWNGRADES to `MipPolicy.Box` where the plan bound none, the receipt recording the channel by name — refusing the whole press for a quality floor the corpus already declares acceptable is the worse trade.
- Law: TILING IS IN-FOLD when the plan carries a policy, so every channel takes ONE plan and the resulting set re-keys; the set's `Tiled` proof stays the gate's own mint rather than the plan's request. A REQUESTED PACK builds AFTER the channels land — each lane composes PER LEVEL from its slot channel's own folded chain, an absent slot carries its neutral at every level, packed channels leave the standalone roster, and depth-divergent slots refuse by name; the compose rail carries its own `Rollback`, because a level that fails to rent mid-chain otherwise strands every level already minted in a carrier no caller receives.
- Law: THE ACCELERATOR ARM dispatches per binding against a device acquired once and released at the fold's close, runs no post chain and no derivation, and its product is a `Preview` — no set, no key, nothing addressable — so the content-identity law needs no runtime check downstream. `Lower` carries the PROVEN field forward beside its kernel row so `Stage` reads a `TextureSource` rather than re-testing a union the plan admission already closed; the uniform block writes through the one `KernelUniform` word writer in the row's declared order, colour columns crossing as `Vec4` appends because a scalar pair previews every authored ramp as grey. `Lift` widens the readback back into staging and crosses through the same `Fill` rail; a short readback RAILS rather than zero-filling its tail, because a truncated dispatch reading as a black band is the failure a preview is least likely to be checked for, and a `Coupled` policy downgrades to the box floor since the preview lane lands no companion to read a variance from.
- Law: FAILURE IS COUNTED EVIDENCE. A band-kernel failure fills the channel neutral AND tallies into `PressReceipt.Faulted` through one interlocked cell, so a plane whose evaluation died and dressed as neutral is separable at the receipt from a genuinely-neutral one. The `Aged` arm alone fills `PressReceipt.Aging`: one interlocked cell folds the visited rungs of every ladder axis as bitsets beside the sampled age extrema, so an over-quantized ladder and an unexercised axis both read off the receipt instead of off a second press.
- Boundary: GOVERNANCE rides the `filter#PLANE_OP` `BakeGovernance` carrier and never a token tail beside a sink tail — one value through the fold statics, `Opened` publishing-and-checking in one call, default-inert so an unwatched press pays one struct copy. The PROGRESS UNIT is the BINDING, the only boundary whose count the plan declares and whose cost is comparable across a press, so both fold passes and the preview lane walk one running ordinal over `plan.Bindings.Count` and the two backends stay comparable on the one surface a caller sees mid-run. CANCELLATION rides the kernel rail underneath: every band polls the token PER LINE, the fold checks between bindings, and a cancelled press rails `Fault.Cancelled` after DISPOSING every landed plane, the same discipline every failure arm holds through `Released`. Wall time rides the injected `TimeProvider` and every per-channel `PlaneReceipt` rides the same clock. The `[EXPRESSION_SPINE]` exemptions are the `PressRows` band arms, the two ladder builds with the simplex quantize-and-rank kernel, the pack `Compose`, and the `Fill` staging write — fixed-extent numeric folds over caller-owned buffers; every admission, dispatch, and egress surface is expression-bodied except the failure-disposal seams, which are resource boundaries.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit — the [02] prelude is in scope.)

// --- [TYPES] -------------------------------------------------------------------------------
// The COMPILED subject: one shape per evaluation law, resolved once per press. The band kernel dispatches on
// this union per PARTITION, so a four-thousand-square plane pays four dispatches per core rather than sixteen
// million, and each arm owns its own row loop.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressProgram {
    private PressProgram() { }
    public sealed record Shaded(CompiledGraph Graph, MaterialParameters Row, OpenPbrSurface Constant) : PressProgram;
    public sealed record Field(TextureSource Source, SamplerState Sampler, TextureChannel Target) : PressProgram;
    // Coverage rides the COMPILED program because the program is minted once per press and reaches every band of
    // every binding already — a per-band or per-binding cell would fragment the very census it exists to answer,
    // and a fifth carried tuple field would thread an instrument through four folds that never read it.
    public sealed record Aged(AgeLadder Ladder, Option<TextureSource> AgeField, Option<TextureSource> CavityField,
        Option<TextureSource> CurvatureField, SamplerState Sampler, LadderCoverage Coverage) : PressProgram;
    public sealed record Mixed(MixLadder Ladder, Seq<TextureSource> WeightFields, SamplerState Sampler) : PressProgram;
    // The dome carries the frontier's closure and nothing else: there is no model, no turbidity, and no solar
    // position on this side of the strata, so the compile step has nothing to resolve and the program IS the
    // closure paired with the channel it fills.
    public sealed record Dome(Func<Vector3d, RgbSpectrum> Radiance, TextureChannel Target) : PressProgram;
    // The surface program holds the chart run and the MEASURE the bound channel demands, resolved ONCE at compile
    // against the channel's own row rather than re-tested per texel: an occlusion channel casts the hemisphere, a
    // curvature channel reads the chart's own second fundamental form, a thickness channel casts inward, and every
    // other channel reads the constant lowered vector — so the band arm is one indexed call and a channel with no
    // geometric measure is answered at compile rather than discovered at the first texel.
    public sealed record Surface(
        ReadOnlyMemory<ChartTexel> Charts, MaterialParameters Row, OpenPbrSurface Constant,
        Func<ChartTexel, TextureChannel, ShadeVec4> Measure, int GutterRings) : PressProgram;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The quantized aging trajectory over the (age, cavity) PRODUCT. Weathering.Apply is fallible and runs once per
// CELL, never once per texel, so a spatially-aged 4k bake costs AgeRungs x CavityRungs admissions instead of
// sixteen million. The second dimension is not a refinement: each dose's effective age is its own row's
// CavityResponse.Scale(age, occlusion), so a dose set mixing a Crevice row with an Exposed row yields a fold no
// single scalar indexes. Cells are AGE-MAJOR — the age index runs fastest — so At's one multiply-add addresses
// the plane, and both rung counts are plan columns entering the plan key.
public sealed record AgeLadder(Seq<OpenPbrSurface> Cells, int AgeRungs, int CavityRungs, int CurvatureRungs) {
    public static Fin<AgeLadder> Of(
        MaterialParameters row, ConductorMetal conductor, Seq<WeatheringDose> aging, LadderRungs rungs, Op key) =>
        toSeq(Enumerable.Range(0, checked((int)rungs.AgeCells)))
            .Fold(Fin.Succ(Seq<OpenPbrSurface>()), (acc, cell) =>
                acc.Bind(built => Weathering.Apply(row, aging,
                        AgeParameter.Create((cell % rungs.Age) / (double)(rungs.Age - 1)),
                        // The exposure PAIR crosses whole, because the two axes are independent and a response row
                        // reads both: a single-rung curvature axis divides by its own opt-out floor and every cell
                        // reads the flat middle, which is the two-dimensional ladder exactly as it was.
                        new SurfaceExposure(
                            UnitInterval.Create(((cell / rungs.Age) % rungs.Cavity) / (double)(rungs.Cavity - 1)),
                            rungs.Curvature is 1 ? 0.0 : (((cell / rungs.Age) / rungs.Cavity) / (double)(rungs.Curvature - 1) * 2.0) - 1.0),
                        key)
                    .Map(aged => built.Add(OpenPbrSurface.Of(aged, conductor)))))
            .Map(built => new AgeLadder(built, rungs.Age, rungs.Cavity, rungs.Curvature));

    // AGE-MAJOR, then cavity, then curvature — one multiply-add per axis, so a texel's three sampled fields address
    // their cell arithmetically and the coverage census reads the same indices the fold wrote.
    public OpenPbrSurface At(double age, double cavity, double curvature) =>
        Cells[(((Rung(Signed(curvature), CurvatureRungs) * CavityRungs) + Rung(cavity, CavityRungs)) * AgeRungs) + Rung(age, AgeRungs)];

    // Curvature arrives SIGNED on [-1,1] — the `set#TEXTURE_CHANNEL` `curvature` row's own declared range — and the
    // rung quantizer works on the unit interval, so the lift is stated here rather than at each of the two sites
    // that would otherwise spell it and could disagree by half a rung.
    internal static double Signed(double curvature) => (Math.Clamp(curvature, -1.0, 1.0) + 1.0) * 0.5;

    // The ONE quantizer both axes and the coverage census read, so a visited-rung tally can never disagree with
    // the cell a texel actually took.
    public static int Rung(double t, int rungs) => Math.Clamp((int)(t * (rungs - 1) + 0.5), 0, rungs - 1);
}

// The quantized pigment-weight simplex, the SAME shape one dimension generalized: Finish.Resolve is fallible and
// runs once per LATTICE POINT of the compositions of `Rungs - 1` into the pigment count, and each texel's sampled
// weight vector normalizes and quantizes onto that lattice. The cell count grows COMBINATORIALLY in the pigment
// count where the aging product grows as a plane, which is why the plan admission gates the cell budget rather
// than the rung count alone — nineteen pigments at eight rungs is not a finer bake, it is an unbuildable one.
public sealed record MixLadder(Seq<OpenPbrSurface> Cells, int Pigments, int Rungs) {
    internal const int CellCeiling = 4096;

    // C(units + parts - 1, parts - 1) as the running-exact incremental product — every partial product is
    // divisible by its own step, so no factorial forms. EXACT, never saturating: Rank sums these terms to
    // address a cell, and a saturation sentinel inside that sum indexes nothing. Every call site sits behind an
    // admitted ladder, whose whole lattice already fits the budget, so each partial term is bounded by it.
    static int Compositions(int units, int parts) {
        long count = 1L;
        for (int i = 1; i < parts; i++) { count = count * (units + i) / i; }
        return (int)count;
    }

    // The ADMISSION probe, saturating ON PURPOSE and answering a different question: the plan gate asks whether
    // the lattice fits the budget, and a product already past the ceiling needs no exact value — it needs to
    // stop multiplying before it overflows. Splitting the probe from the exact count is what keeps the sentinel
    // out of rank arithmetic, where nineteen pigments once made every cell address int.MaxValue-sized garbage.
    internal static int Budget(int rungs, int pigments) {
        long count = 1L;
        for (int i = 1; i < pigments; i++) {
            count = count * (rungs - 1 + i) / i;
            if (count > CellCeiling) { return int.MaxValue; }
        }
        return (int)count;
    }

    public static Fin<MixLadder> Of(PressSubject.Mix mix, LadderRungs rungs, Op key) {
        int parts = mix.Pigments.Count, units = rungs.Mix - 1;
        int[] counts = new int[parts];
        counts[parts - 1] = units;
        Fin<Seq<OpenPbrSurface>> built = Fin.Succ(Seq<OpenPbrSurface>());
        do {
            built = built.Bind(cells => Resolve(mix, counts, key).Map(cells.Add));
        } while (built.IsSucc && Advance(counts, units));
        return built.Map(cells => new MixLadder(cells, parts, rungs.Mix));
    }

    // The counts ARE the weights: FinishMix.Of admits any non-negative vector with a positive sum and the
    // Kubelka-Munk constructor normalizes, so scaling them back onto [0,1] would only re-derive a ratio the
    // lattice already states exactly.
    static Fin<OpenPbrSurface> Resolve(PressSubject.Mix mix, int[] counts, Op key) =>
        FinishMix.Of(mix.Pigments, toSeq(counts).Map(static c => (double)c), key)
            .Bind(admitted => Finish.Resolve(mix.Kind, admitted, mix.Stack, key, mix.Substrate))
            .Map(resolved => OpenPbrSurface.Of(resolved.Row, mix.Conductor));

    // Lexicographic advance over the free axes with the last part carrying the remainder: increment the deepest
    // axis that still has slack, zero every axis after it, and re-seat the remainder. The enumeration order IS
    // the order Rank inverts, so the two derive from one law rather than agreeing by inspection.
    static bool Advance(Span<int> counts, int units) {
        for (int axis = counts.Length - 2; axis >= 0; axis--) {
            int prefix = 0;
            for (int i = 0; i < axis; i++) { prefix += counts[i]; }
            if (prefix + counts[axis] >= units) { continue; }
            counts[axis]++;
            for (int i = axis + 1; i < counts.Length - 1; i++) { counts[i] = 0; }
            counts[counts.Length - 1] = units - prefix - counts[axis];
            return true;
        }
        return false;
    }

    // The per-texel read: normalize, quantize by largest remainder into the caller's band-scoped counts, then rank.
    public OpenPbrSurface At(ReadOnlySpan<double> weights, Span<int> counts) {
        Quantize(weights, counts, Rungs - 1);
        return Cells[Rank(counts, Rungs - 1)];
    }

    // Largest-remainder quantization onto the lattice: the floor pass places most units, the residual pass hands
    // each remaining unit to the largest fractional part with the LOWEST index breaking ties, so one weight vector
    // always lands on one cell and a band partition cannot produce two answers. A zero-sum vector seats the whole
    // mass on the first pigment rather than dividing by zero.
    static void Quantize(ReadOnlySpan<double> weights, Span<int> counts, int units) {
        double total = 0.0;
        for (int i = 0; i < weights.Length; i++) { total += Math.Max(0.0, weights[i]); }
        if (!(total > 0.0)) { counts.Clear(); counts[0] = units; return; }
        int placed = 0;
        for (int i = 0; i < counts.Length; i++) {
            counts[i] = (int)(Math.Max(0.0, weights[i]) / total * units);
            placed += counts[i];
        }
        for (; placed < units; placed++) {
            int best = 0;
            double top = -1.0;
            for (int i = 0; i < counts.Length; i++) {
                double residual = (Math.Max(0.0, weights[i]) / total * units) - counts[i];
                if (residual > top) { (top, best) = (residual, i); }
            }
            counts[best]++;
        }
    }

    // The rank of a composition in Advance's own enumeration: every lattice point whose earlier axes are smaller
    // precedes it, and the count of those is the composition count over the axes that remain.
    static int Rank(ReadOnlySpan<int> counts, int units) {
        int rank = 0, remaining = units;
        for (int axis = 0; axis < counts.Length - 1; axis++) {
            for (int step = 0; step < counts[axis]; step++) { rank += Compositions(remaining - step, counts.Length - axis - 1); }
            remaining -= counts[axis];
        }
        return rank;
    }
}

// The band-shared ladder census: two visited-rung bitsets sized from the plan's own rung columns plus the sampled
// age extrema as raw bits, every word advanced with Interlocked so a band partition contributes without a lock —
// the same cell discipline the fault tally takes, one word wider per sixty-four rungs, so a ladder of any declared
// depth is counted exactly rather than saturated. An unvisited cell reports absence: the extrema seed at the
// infinities, so a program that never sampled reads a coverage nothing measured and the receipt carries None.
// The type is PUBLIC because the compiled PressProgram.Aged case is, and a public record cannot carry an internal
// member type; the mutating members stay internal, so a consumer reads the census and never advances it.
public sealed class LadderCoverage(LadderRungs rungs) {
    readonly ulong[] words = new ulong[Words(rungs.Age) + Words(rungs.Cavity) + Words(rungs.Curvature) + 2];
    readonly int cavityAt = Words(rungs.Age);
    readonly int curvatureAt = Words(rungs.Age) + Words(rungs.Cavity);
    readonly int minAt = Words(rungs.Age) + Words(rungs.Cavity) + Words(rungs.Curvature);

    static int Words(int count) => (count + 63) >> 6;

    internal LadderCoverage Seeded() {
        words[minAt] = BitConverter.DoubleToUInt64Bits(double.PositiveInfinity);
        words[minAt + 1] = BitConverter.DoubleToUInt64Bits(double.NegativeInfinity);
        return this;
    }

    internal void Visit(int ageRung, int cavityRung, int curvatureRung, double age) {
        Interlocked.Or(ref words[ageRung >> 6], 1UL << (ageRung & 63));
        Interlocked.Or(ref words[cavityAt + (cavityRung >> 6)], 1UL << (cavityRung & 63));
        Interlocked.Or(ref words[curvatureAt + (curvatureRung >> 6)], 1UL << (curvatureRung & 63));
        Extremum(ref words[minAt], age, lower: true);
        Extremum(ref words[minAt + 1], age, lower: false);
    }

    public Option<AgeCoverage> Read() =>
        BitConverter.UInt64BitsToDouble(Volatile.Read(ref words[minAt])) switch {
            var low when double.IsFinite(low) => Some(new AgeCoverage(rungs.Age, rungs.Cavity, rungs.Curvature,
                Visited(0, cavityAt), Visited(cavityAt, curvatureAt), Visited(curvatureAt, minAt), low,
                BitConverter.UInt64BitsToDouble(Volatile.Read(ref words[minAt + 1])))),
            _ => Option<AgeCoverage>.None,
        };

    int Visited(int from, int until) {
        int seen = 0;
        for (int word = from; word < until; word++) { seen += System.Numerics.BitOperations.PopCount(Volatile.Read(ref words[word])); }
        return seen;
    }

    // The extremum commits by compare-exchange over the raw bits, re-reading the loser and retrying — a bare read
    // then write loses every concurrent band but the last, which is a coverage span narrower than the press took.
    static void Extremum(ref ulong cell, double value, bool lower) {
        ulong candidate = BitConverter.DoubleToUInt64Bits(value);
        ulong seen = Volatile.Read(ref cell);
        while (lower ? BitConverter.UInt64BitsToDouble(seen) > value : BitConverter.UInt64BitsToDouble(seen) < value) {
            ulong prior = Interlocked.CompareExchange(ref cell, candidate, seen);
            if (prior == seen) { return; }
            seen = prior;
        }
    }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TexturePress {
    // Bands sized so one rent serves a partition rather than a row — the same grain filter#PLANE_FOLD takes,
    // because both fold the same planes through the same rail and a second sizing law would diverge.
    const int BandFloor = 16;

    public static Fin<PressProduct> Press(PressSubject subject, PressPlan plan, Op key, TimeProvider? clock = null, BakeGovernance governance = default) {
        TimeProvider ticks = clock ?? TimeProvider.System;
        long opened = ticks.GetTimestamp();
        return plan.Backend.ContentAuthoritative
            ? Mint(subject, plan, key, ticks, opened, governance)
            : Accelerate(subject, plan, key, ticks, opened, governance);
    }

    // THE PROGRESS UNIT IS THE BINDING, because that is the only boundary whose count the plan already declares and
    // whose cost is comparable across a press: every binding folds one whole plane through one band partition, and a
    // derived binding folds its source first. Both passes walk the SAME roster, so the fraction is one running
    // ordinal over plan.Bindings.Count and a chain of two hundred channels reports the same shape as a chain of one.
    // Publishing per band or per line would flood a sink that has one number to show, and the per-line token read
    // inside PressRows is what keeps a cancelled sixteen-million-texel pass from running to completion regardless.
    static Fin<T> Staged<T>(BakeGovernance governance, int done, int total, HashMap<TextureChannel, TexturePyramid> landed, Func<Fin<T>> body) =>
        governance.Opened(total <= 0 ? 1.0 : done / (double)total).Map(_ => Surrender<T>(landed)).IfNone(body);

    // The CPU lane: compile once, fold every direct binding, post-process, derive, PACK, mip against pairs,
    // tile, admit. Each binding's staging arena lives exactly one Finish and disposes at its fold step — a
    // press never holds N staging arenas at once — and the set is minted LAST so a failed stage never leaves
    // a half-keyed bundle behind. A DIRECT binding is a non-derived one OR the field subject's own target, so
    // a Source plan targeting `height` bakes the field INTO height rather than synthesizing a normal map and
    // integrating it back out. Every landed plane DISPOSES on the failure and cancellation arms — a refused
    // press leaks no arena — and cancellation rails the kernel `Fault.Cancelled` between bindings and inside
    // every band, surrendering no partial product. Intermediates a derivation pulled in but no binding
    // requested are dropped from the set and disposed with it.
    static Fin<PressProduct> Mint(PressSubject subject, PressPlan plan, Op key, TimeProvider ticks, long opened, BakeGovernance governance) =>
        from program in Compile(subject, plan, key)
        from folded in plan.Bindings.Filter(b => Direct(program, b))
            .Fold(Fin.Succ((Planes: HashMap<TextureChannel, TexturePyramid>.Empty, Evidence: HashMap<TextureChannel, PlaneReceipt>.Empty, Downgraded: Seq<TextureChannel>(), Faulted: HashMap<TextureChannel, ulong>.Empty, Done: 0)), (acc, binding) =>
                acc.Bind(carried => Staged(governance, carried.Done, plan.Bindings.Count, carried.Planes, () =>
                    Land(program, plan, binding, (carried.Planes, carried.Evidence, carried.Downgraded, carried.Faulted), key, ticks, governance.Cancel)
                        .Map(next => (next.Planes, next.Evidence, next.Downgraded, next.Faulted, Done: carried.Done + 1))
                        .MapFail(fault => Released(carried.Planes, fault)))))
        from derived in plan.Bindings.Filter(b => !Direct(program, b))
            .Fold(Fin.Succ(folded), (acc, binding) => acc.Bind(carried =>
                Staged(governance, carried.Done, plan.Bindings.Count, carried.Planes, () =>
                    Derive(program, plan, binding, (carried.Planes, carried.Evidence, carried.Downgraded, carried.Faulted), key, ticks, governance.Cancel)
                        .Map(next => (next.Planes, next.Evidence, next.Downgraded, next.Faulted, Done: carried.Done + 1))
                        .MapFail(fault => Released(carried.Planes, fault)))))
        from packed in Packed(plan, derived.Planes, key).MapFail(fault => Released(derived.Planes, fault))
        from set in TextureSet.Of(new TextureSetDraft(plan.Width, plan.Height, plan.Layers, plan.Law,
            NormalConvention.Gl, plan.Alpha, plan.HeightScaleMm, Option<TileProof>.None, Seq<UdimTile>(),
            packed.Channels.Filter((c, _) => plan.Bindings.Exists(b => b.Channel == c && b.Pack.IsNone)), packed.Packs,
            plan.Conductor, plan.Material), key).MapFail(fault => Released(derived.Planes, fault))
        from tiled in plan.Tile
            .Map(policy => TileSynth.Tileify(set, policy, key, ticks).Map(static pair => pair.Set))
            .IfNone(Fin.Succ(set))
        select (PressProduct)new PressProduct.Minted(tiled, new PressReceipt(plan.Backend, plan.PlanKey,
            GraphKey(program), plan.Seed, Texels(tiled), ticks.GetElapsedTime(opened).TotalMilliseconds,
            derived.Evidence, derived.Downgraded, derived.Faulted, GpuDeltaMax: Option<double>.None,
            Aging: Coverage(program)));

    // Only the aged program measures a ladder, so every other program reports absence rather than a zero span a
    // gate would read as a real one-cell census — the same typed-absence law GraphKey and GpuDeltaMax hold.
    static Option<AgeCoverage> Coverage(PressProgram program) =>
        program is PressProgram.Aged aged ? aged.Coverage.Read() : Option<AgeCoverage>.None;

    // A direct binding folds the program itself: every non-derived channel, plus the field subject's OWN
    // target even when that target is roster-derived — the field IS the channel's bytes there.
    static bool Direct(PressProgram program, ChannelBinding binding) =>
        binding.Channel.Origin is not ChannelOrigin.Derived
            || (program is PressProgram.Field field && field.Target == binding.Channel);

    // One binding's landing: fold the band kernel, cross into the plane substrate, thread the band tally.
    static Fin<(HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneReceipt> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted)> Land(
        PressProgram program, PressPlan plan, ChannelBinding binding,
        (HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneReceipt> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted) carried,
        Op key, TimeProvider ticks, CancellationToken cancel) =>
        Fold(program, plan, binding, key, cancel).Bind(fold => {
            try {
                return cancel.IsCancellationRequested
                    ? Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, HashMap<TextureChannel, PlaneReceipt>, Seq<TextureChannel>, HashMap<TextureChannel, ulong>)>(new Fault.Cancelled())
                    : Finish(plan, binding, fold.Arena.Memory.AsMemory2D(plan.Height.Value * plan.Layers.Value, plan.Width.Value), carried.Planes, key, ticks)
                        .Map(built => (
                            carried.Planes.Add(binding.Channel, built.Pyramid),
                            carried.Evidence.Add(binding.Channel, built.Receipt),
                            built.Downgraded ? carried.Downgraded.Add(binding.Channel) : carried.Downgraded,
                            fold.Faulted > 0 ? carried.Faulted.Add(binding.Channel, fold.Faulted) : carried.Faulted));
            } finally {
                fold.Arena.Dispose();
            }
        });

    static Fin<T> Surrender<T>(HashMap<TextureChannel, TexturePyramid> landed) {
        landed.Values.Iter(static pyramid => pyramid.Dispose());
        return Fin.Fail<T>(new Fault.Cancelled());
    }

    static Error Released(HashMap<TextureChannel, TexturePyramid> landed, Error fault) {
        landed.Values.Iter(static pyramid => pyramid.Dispose());
        return fault;
    }

    // The packs the plan requested become pack planes: packed channels leave the standalone roster, each lane
    // composes PER LEVEL from its slot channel's own already-folded chain — which is what makes the frozen
    // per-component mip law hold by construction — and an absent slot carries its channel's own neutral at
    // every level. Slots packed together must agree on chain depth; divergent per-slot mip overrides refuse
    // by name rather than truncating a chain silently. The carrier's Policy column records the box floor —
    // the per-component truth is the slot rows' own Mip columns, roster data every reader already holds.
    static Fin<(HashMap<TextureChannel, TexturePyramid> Channels, Seq<ChannelPackPlane> Packs)> Packed(
        PressPlan plan, HashMap<TextureChannel, TexturePyramid> landed, Op key) =>
        toSeq(plan.Bindings.Choose(static b => b.Pack).Distinct())
            .Fold(Fin.Succ((Channels: landed, Packs: Seq<ChannelPackPlane>())), (acc, pack) =>
                acc.Bind(carried => {
                    Seq<TextureChannel> present = pack.Slots.Filter(carried.Channels.ContainsKey);
                    Seq<int> depths = toSeq(present.Choose(slot => carried.Channels.Find(slot)).Map(static c => c.Levels.Count).Distinct());
                    ChannelDtype depth = plan.Bindings.Filter(b => b.Pack == Some(pack)).Map(static b => b.Format.Depth).Head.IfNone(ChannelDtype.Unorm8);
                    return present.IsEmpty
                        ? Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, Seq<ChannelPackPlane>)>(MaterialFault.Parameter(key, $"<pack-no-landed-slot:{pack.Key}>"))
                        : depths.Count > 1
                            ? Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, Seq<ChannelPackPlane>)>(MaterialFault.Parameter(key, $"<pack-slot-mip-divergent:{pack.Key}>"))
                            : from format in PlaneFormat.For(4, depth).ToFin(MaterialFault.Parameter(key, $"<pack-format-unresolved:{pack.Key}:{depth.Key}>"))
                              from levels in Compose(pack, carried.Channels, depths.Head.IfNone(1), format, key)
                              select (
                                  carried.Channels.Filter((c, _) => !pack.Slots.Contains(c)),
                                  carried.Packs.Add(new ChannelPackPlane(pack, new TexturePyramid(levels, MipPolicy.Box, Coupled: false), present)));
                }));

    // One pack level: read each present lane's own level row, seat absent lanes at their channel neutral,
    // write through the plane's own ShadeVec4 rail — raw transfer, no alpha, the frozen pack law. Lane index
    // IS slot order, the same correspondence ChannelPack.Lane publishes. The level roster is an ITEM fold —
    // every level target mints on the rail FIRST (the parallel walk is total), then ParallelHelper.ForEach
    // partitions the job Memory and hands each worker its own ref PackLevelJob, the sibling of the Compute
    // mosaic's IRefAction item fold: no worker indexes a captured array by slot number, the seeded struct
    // carries the slot planes and neutrals whole, and a single partition invokes inline.
    static Fin<Seq<TexturePlane>> Compose(ChannelPack pack, HashMap<TextureChannel, TexturePyramid> landed, int depth, PlaneFormat format, Op key) {
        Option<TexturePyramid> reference = pack.Slots.Choose(slot => landed.Find(slot)).Head;
        return reference.ToFin(MaterialFault.Parameter(key, $"<pack-no-landed-slot:{pack.Key}>")).Bind(head =>
            toSeq(Enumerable.Range(0, depth)).Fold(Fin.Succ(Seq<PackLevelJob>()), (acc, levelIndex) =>
                acc.Bind(jobs => {
                    TexturePlane extent = head.Levels[levelIndex];
                    return TexturePlane.Of(format, extent.Width, extent.Height, PlaneTransfer.Raw, AlphaMode.None, key, Some(extent.Layers))
                        .Map(target => jobs.Add(new PackLevelJob(
                            target,
                            [.. pack.Slots.Map(slot => landed.Find(slot).Case is TexturePyramid chain ? chain.Levels[levelIndex] : null)],
                            [.. pack.Slots.Map(static slot => slot.Neutral.X)])))
                        // Custody rides the rail's own Rollback: a level that fails to rent mid-chain leaves every
                        // level already minted holding a pooled arena the rail cannot see, and the composed carrier
                        // never reaches a caller to dispose. The disposer runs over the targets, not the sources —
                        // the landed channel chains belong to the fold above and are borrowed here.
                        .Rollback([.. jobs.Map(static job => job.Target)]);
                }))
            .Map(static jobs => {
                PackLevelJob[] roster = [.. jobs];
                PackCompose fold = new();
                ParallelHelper.ForEach<PackLevelJob, PackCompose>(roster.AsMemory(), in fold, minimumActionsPerThread: 1);
                return toSeq(roster).Map(static job => job.Target);
            }));
    }

    // One level job whole per worker: the target plane, the per-slot source level (null seats the neutral),
    // and the per-slot neutral scalar — every field the band needs rides the item, never a closure.
    readonly record struct PackLevelJob(TexturePlane Target, TexturePlane?[] Slots, double[] Neutrals);

    readonly struct PackCompose : IRefAction<PackLevelJob> {
        public void Invoke(ref PackLevelJob job) {
            using SpanOwner<ShadeVec4> lane = SpanOwner<ShadeVec4>.Allocate(job.Target.Width.Value);
            using SpanOwner<ShadeVec4> texels = SpanOwner<ShadeVec4>.Allocate(job.Target.Width.Value);
            for (int layer = 0; layer < job.Target.Layers.Value; layer++) {
                for (int row = 0; row < job.Target.Height.Value; row++) {
                    for (int x = 0; x < texels.Span.Length; x++) { texels.Span[x] = new ShadeVec4(0.0, 0.0, 0.0, 1.0); }
                    for (int slotIndex = 0; slotIndex < job.Slots.Length; slotIndex++) {
                        // NULL is the absent-slot signal and the probe is the read: the job carries a fixed-arity
                        // slot array because a worker takes its item whole and may capture nothing, and a
                        // `TexturePlane?[]` is the one shape that carries "this lane has no chain" inside a value a
                        // `ref` fold can hand across a partition — an `Option` array would box per slot per level.
                        if (job.Slots[slotIndex] is TexturePlane source) {
                            source.ReadShade(row, layer, lane.Span);
                            for (int x = 0; x < texels.Span.Length; x++) { texels.Span[x] = Seat(texels.Span[x], slotIndex, lane.Span[x].X); }
                        } else {
                            double neutral = job.Neutrals[slotIndex];
                            for (int x = 0; x < texels.Span.Length; x++) { texels.Span[x] = Seat(texels.Span[x], slotIndex, neutral); }
                        }
                    }
                    job.Target.WriteShade(row, layer, texels.Span);
                }
            }
        }
    }

    static ShadeVec4 Seat(ShadeVec4 texel, int lane, double value) =>
        lane switch { 0 => texel with { X = value }, 1 => texel with { Y = value }, _ => texel with { Z = value } };

    // The GPU lane returns a Preview: planes and a receipt, NO TextureSet, therefore NO key. The
    // content-identity veto is the union's shape, not a runtime check any consumer could skip. It runs no post
    // chain and no derivation — the accelerator lowers field kernels alone, which is exactly what the plan
    // admission already proved before a device was rented.
    static Fin<PressProduct> Accelerate(PressSubject subject, PressPlan plan, Op key, TimeProvider ticks, long opened, BakeGovernance governance) =>
        from lease in PressDevice.Acquire(DevicePolicy.Default, key)
        // The preview lane reports on the SAME binding unit the CPU lane does, so a caller watching a bake sees one
        // series whichever backend served it — an accelerator that reported a different unit would make the two
        // lanes incomparable on the one surface a caller can see mid-run.
        from planes in lease.Use((Subject: subject, Plan: plan, Key: key, Governance: governance), static (state, device) =>
            state.Plan.Bindings.Fold(Fin.Succ((Rows: HashMap<TextureChannel, TexturePyramid>.Empty, Done: 0)), (acc, binding) =>
                acc.Bind(carried => Staged(state.Governance, carried.Done, state.Plan.Bindings.Count, carried.Rows, () =>
                    Lower(state.Subject, binding, state.Key)
                        .Bind(lowered => device.Dispatch(lowered.Kernel, Stage(state.Plan, lowered.Kernel, lowered.Field), state.Key))
                        .Bind(receipt => Lift(state.Plan, binding, receipt, state.Key))
                        .Map(plane => (Rows: carried.Rows.Add(binding.Channel, plane), Done: carried.Done + 1))
                        .MapFail(fault => Released(carried.Rows, fault)))))
            .Map(static carried => carried.Rows))
        select (PressProduct)new PressProduct.Preview(planes, new PressReceipt(plan.Backend, plan.PlanKey,
            Option<UInt128>.None, plan.Seed, Texels(plan, planes), ticks.GetElapsedTime(opened).TotalMilliseconds,
            HashMap<TextureChannel, PlaneReceipt>.Empty, Seq<TextureChannel>(), HashMap<TextureChannel, ulong>.Empty,
            GpuDeltaMax: Option<double>.None, Aging: Option<AgeCoverage>.None));

    // One compile per press. The graph arm freezes its order AND its constant lowered vector, so an unslotted
    // channel reads a real OpenPBR column rather than a re-lowering per texel; the two ladder arms build their
    // whole tables here, so every cell's fallible admission is paid once and the band arms are TOTAL over an
    // admitted plan. The aged arm seats its coverage census with the table, so the census and the table it
    // measures are minted together and no band can observe one without the other.
    static Fin<PressProgram> Compile(PressSubject subject, PressPlan plan, Op key) =>
        subject.Switch(
            state:  (Plan: plan, Key: key),
            graph:  static (s, g) => g.Program.Compile(s.Key).Map(compiled => (PressProgram)new PressProgram.Shaded(compiled, g.Row, OpenPbrSurface.Of(g.Row, g.Conductor))),
            source: static (_, f) => Fin.Succ<PressProgram>(new PressProgram.Field(f.Field, f.Sampler, f.Target)),
            slab:   static (s, b) => AgeLadder.Of(b.Row, b.Conductor, b.Aging, s.Plan.Rungs, s.Key)
                .Map(ladder => (PressProgram)new PressProgram.Aged(
                    ladder, b.AgeField, b.CavityField, b.CurvatureField, b.Sampler, new LadderCoverage(s.Plan.Rungs).Seeded())),
            mix:    static (s, m) => MixLadder.Of(m, s.Plan.Rungs, s.Key)
                .Map(ladder => (PressProgram)new PressProgram.Mixed(ladder, m.WeightFields, m.Sampler)),
            // A dome resolves NOTHING at compile: the frontier already resolved its model, so the program is the
            // closure paired with its target and the compile step is the identity that says so.
            sky:    static (_, k) => Fin.Succ<PressProgram>(new PressProgram.Dome(k.Radiance, k.Target)),
            // The mesh-space arm resolves the per-channel MEASURE once against the bound roster, so the band arm
            // is one indexed call and a channel with no geometric producer is answered here — at compile, against
            // the plan's own bindings — rather than discovered at the first texel of the first band.
            meshSpace: static (s, m) => GeometricMeasure(m, s.Plan.Bindings, s.Key)
                .Map(measure => (PressProgram)new PressProgram.Surface(
                    m.Charts, m.Row, OpenPbrSurface.Of(m.Row, m.Conductor), measure, m.GutterRings)));

    // The per-channel geometric measure, resolved ONCE against the plan's own bindings. `ChannelOrigin.Geometric`
    // gains REAL producers here: occlusion casts the hemisphere against the chart set, curvature reads the chart's
    // own frame differential, and thickness casts inward — three measurements a height-field derivation can only
    // approximate. Every other bound channel reads the constant lowered vector, exactly as a graph subject's
    // unslotted channels do, so the arm is total over an admitted plan and no texel discovers a missing producer.
    static Fin<Func<ChartTexel, TextureChannel, ShadeVec4>> GeometricMeasure(
        PressSubject.MeshSpace subject, Seq<ChannelBinding> bindings, Op key) =>
        bindings.Exists(static b => b.Channel.Origin is ChannelOrigin.Geometric or ChannelOrigin.Derived)
            ? Fin.Succ<Func<ChartTexel, TextureChannel, ShadeVec4>>((texel, channel) => Measured(subject, texel, channel))
            : Fin.Succ<Func<ChartTexel, TextureChannel, ShadeVec4>>(static (_, channel) => channel.Neutral);

    // The cast itself. It reads the chart run the subject carries and NOTHING else — no host geometry, no
    // tessellator, no acceleration structure this folder would have to own — because the chart entry already
    // carries the surface point and its frame. A channel with no geometric meaning answers its own neutral rather
    // than a fabricated measurement, which is the same typed-absence discipline every measured column here takes.
    static ShadeVec4 Measured(PressSubject.MeshSpace subject, ChartTexel texel, TextureChannel channel) =>
        channel == TextureChannel.Occlusion ? Scalar(Hemisphere(subject, texel, inward: false))
        : channel == TextureChannel.Curvature ? Scalar(Bend(subject, texel))
        : channel == TextureChannel.Height ? Scalar(Hemisphere(subject, texel, inward: true))
        : channel.Neutral;

    static ShadeVec4 Scalar(double value) => new(value, 0.0, 0.0, 1.0);

    // The band fold. ParallelHelper.For over a SEEDED struct IAction allocates nothing, inlines, clamps to the
    // processor count, and invokes inline for a single partition; the unseeded overload default-constructs the
    // action and would hand every band an empty program. The arena is one pooled MemoryOwner per binding. The
    // anchor mints ONCE on the rail — a per-band `.IfFail(default)` forced a null into a class-typed cell and
    // hid the one refusal the fixed axes make unreachable — and the fault tally crosses the partition as one
    // Interlocked cell, so a neutral-filled band failure is COUNTED evidence, never a silent success.
    static Fin<(MemoryOwner<ShadeVec4> Arena, ulong Faulted)> Fold(PressProgram program, PressPlan plan, ChannelBinding binding, Op key, CancellationToken cancel) =>
        ShadePoint.Of(Point3d.Origin, Vector3d.ZAxis, Vector3d.ZAxis, Some(Vector3d.XAxis), 0.0, 0.0, GraphContext.Tolerant, key)
            .Map(anchor => {
                int rows = plan.Height.Value * plan.Layers.Value;
                MemoryOwner<ShadeVec4> arena = MemoryOwner<ShadeVec4>.Allocate(plan.Width.Value * rows, AllocationMode.Default);
                ulong[] faulted = new ulong[1];
                int band = Math.Max(BandFloor, rows / (Environment.ProcessorCount * 4));
                try {
                    ParallelHelper.For(0, (rows + band - 1) / band,
                        in new PressRows(program, plan, binding.Channel, anchor, arena.Memory.AsMemory2D(rows, plan.Width.Value), band, rows, faulted, cancel, key));
                } catch {
                    // The rental never outlives a partition throw; the rail carries every DOMAIN failure, so a
                    // throw here is host-level and rethrows after the arena returns to its pool.
                    arena.Dispose();
                    throw;
                }
                return (arena, faulted[0]);
            });

    // Staging crosses into the plane substrate ONCE, through the plane's own row Write rail: alpha
    // association, transfer encode, and depth narrowing all happen at their owner, so the press encodes no
    // texel itself. The post chain then runs as ONE filter#PLANE_OP Apply over the admitted plane, whose
    // PlaneReceipt reaches the press receipt with the height solver's residual intact. A paired mip policy
    // resolves its companion from the landed map, downgrading to Box (the declared quality floor) and naming
    // the channel when the plan bound none — refusing the whole press for a floor is the deleted response.
    static Fin<(TexturePyramid Pyramid, PlaneReceipt Receipt, bool Downgraded)> Finish(
        PressPlan plan, ChannelBinding binding, Memory2D<ShadeVec4> staging, HashMap<TextureChannel, TexturePyramid> landed, Op key, TimeProvider ticks) =>
        from blank in TexturePlane.Of(binding.Format, plan.Width, plan.Height, binding.Channel.Transfer,
            binding.Format.Alpha.Carries ? plan.Alpha : AlphaMode.None, key, Some(plan.Layers))
        let filled = Fill(blank, staging, binding.Display)
        from posted in PlaneOp.Apply(filled, binding.Post, key, ticks)
        let paired = Companion(binding.Channel, landed)
        let policy = binding.Policy.Coupled && paired.IsNone ? MipPolicy.Box : binding.Policy
        from chain in TexturePyramid.Of(posted.Plane, policy, key, paired)
        select (chain, posted.Receipt, policy != binding.Policy);

    static Option<TexturePyramid> Companion(TextureChannel channel, HashMap<TextureChannel, TexturePyramid> landed) =>
        channel.Pair.Bind(name => TextureChannel.TryGet(name, out TextureChannel? row) ? landed.Find(row) : Option<TexturePyramid>.None);

    // Row-wise write through the plane's OWN ShadeVec4 rail over EVERY layer, so the lane-to-register
    // correspondence has one owner and the press encodes no texel itself. A declared DisplayEgress grades the row
    // BEFORE that crossing — the surface#OPENPBR_SLAB ToneMap operator per texel at the row's exposure, then the
    // span encode into the target's own primaries and transfer — so scene-referred radiance reaches an integer
    // lane through the corpus's one tone-map owner rather than through the transfer clip a raw narrow performs.
    // Both lanes call this rail, so the preview inherits the grade and the two still differ only in authority.
    static TexturePlane Fill(TexturePlane plane, Memory2D<ShadeVec4> texels, Option<DisplayEgress> display) {
        ReadOnlySpan2D<ShadeVec4> source = texels.Span;
        // The PASS-THROUGH lane rents nothing at all and takes its own walk, because a zero-length rental is a
        // rental that exists only to be skipped — it declared two buffers a caller has to reason about and a
        // reader has to check the width of, to express "this binding grades nothing". The two walks share the row
        // read and diverge exactly where they differ, which is the whole of the difference.
        if (display.Case is not DisplayEgress egress) {
            for (int layer = 0; layer < plane.Layers.Value; layer++) {
                for (int row = 0; row < plane.Height.Value; row++) {
                    plane.WriteShade(row, layer, source.GetRowSpan((layer * plane.Height.Value) + row));
                }
            }
            return plane;
        }
        // The grade takes TWO buffers rather than aliasing one, so no unstated in-place contract binds ToneMap.
        using SpanOwner<ShadeVec4> toned = SpanOwner<ShadeVec4>.Allocate(plane.Width.Value);
        using SpanOwner<ShadeVec4> encoded = SpanOwner<ShadeVec4>.Allocate(plane.Width.Value);
        for (int layer = 0; layer < plane.Layers.Value; layer++) {
            for (int row = 0; row < plane.Height.Value; row++) {
                ReadOnlySpan<ShadeVec4> line = source.GetRowSpan((layer * plane.Height.Value) + row);
                for (int x = 0; x < line.Length; x++) {
                    RgbSpectrum scene = RgbSpectrum.Create(Lane(line[x].X), Lane(line[x].Y), Lane(line[x].Z));
                    RgbSpectrum graded = ToneMap.Apply(egress.Operator, scene, egress.Exposure);
                    toned.Span[x] = new ShadeVec4(graded.R, graded.G, graded.B, line[x].W);
                }
                ToneMap.Encode(toned.Span, egress.Encoding, encoded.Span);
                // COVERAGE never grades: the encode owns the colour lanes and the press restores the alpha it
                // staged, so an associated-alpha plane still associates once, at the plane's own Write rail.
                for (int x = 0; x < line.Length; x++) { encoded.Span[x] = encoded.Span[x] with { W = line[x].W }; }
                plane.WriteShade(row, layer, encoded.Span);
            }
        }
        return plane;
    }

    // The validated reflectance carrier REFUSES a non-finite or negative channel at Create, so the display seam
    // seats a dead lane at black rather than throwing past the rail — the band arm that produced it already
    // tallied into PressReceipt.Faulted, so the evidence survives where an escaping throw would take the press.
    static double Lane(double value) => double.IsFinite(value) ? Math.Max(0.0, value) : 0.0;

    // A derived channel folds from its LANDED source plane through the ROSTER's own declared step, composed
    // before the caller's post chain — the press never re-derives a normal integration, an occlusion sweep, or
    // a curvature stencil, and a caller cannot omit the operation that makes the channel what it is. Ensure
    // produces a MISSING source as an intermediate first — occlusion without height presses height from
    // geometry_normal, and geometry_normal itself folds off the shade point — so the [02] plan law "an unbound
    // source is produced, never refused" is real: the recursion grounds at the roster's non-derived origins and
    // an unbound intermediate is dropped at the set filter rather than keyed.
    static Fin<(HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneReceipt> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted)> Derive(
        PressProgram program, PressPlan plan, ChannelBinding binding,
        (HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneReceipt> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted) carried,
        Op key, TimeProvider ticks, CancellationToken cancel) =>
        binding.Channel.Origin is ChannelOrigin.Derived derived && TextureChannel.TryGet(derived.From, out TextureChannel? from)
            ? from sourced in Ensure(program, plan, from!, carried, key, ticks, cancel)
              from source in sourced.Planes.Find(from!).ToFin(MaterialFault.Parameter(key, $"<derived-source-absent:{binding.Channel.Key}:{derived.From}>"))
              from folded in PlaneOp.Apply(source.Base, derived.Fold.Cons(binding.Post), key, ticks)
              let paired = Companion(binding.Channel, sourced.Planes)
              let policy = binding.Policy.Coupled && paired.IsNone ? MipPolicy.Box : binding.Policy
              from chain in TexturePyramid.Of(folded.Plane, policy, key, paired)
              select (sourced.Planes.Add(binding.Channel, chain),
                      sourced.Evidence.Add(binding.Channel, folded.Receipt),
                      policy != binding.Policy ? sourced.Downgraded.Add(binding.Channel) : sourced.Downgraded,
                      sourced.Faulted)
            : Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, HashMap<TextureChannel, PlaneReceipt>, Seq<TextureChannel>, HashMap<TextureChannel, ulong>)>(
                MaterialFault.Parameter(key, $"<derived-origin-unresolved:{binding.Channel.Key}>"));

    // A missing source materializes through an IMPLICIT solver-grade binding — float storage, no pyramid, no
    // post, no display grade, because an intermediate feeds a derivation rather than a container — recursing
    // through Derive for a derived source and through the band fold for a shaded or geometric
    // one. The intermediate joins the landed map so a second consumer reuses it, and the set filter drops it.
    // The recursion needs no visited set of its own: the plan admission's Depth gate already refused a cyclic
    // Derived.From chain, so the walk grounds at a non-derived origin within the roster's own count.
    static Fin<(HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneReceipt> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted)> Ensure(
        PressProgram program, PressPlan plan, TextureChannel channel,
        (HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneReceipt> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted) carried,
        Op key, TimeProvider ticks, CancellationToken cancel) =>
        carried.Planes.ContainsKey(channel)
            ? Fin.Succ(carried)
            // The implicit format is a ROSTER READ on the rail, never a substitution: PlaneFormat.For answers
            // the float row for THIS channel's component count, and an absent answer is a roster gap the fold
            // names. The fallback it replaces was worse than unreachable — a one-component height intermediate
            // would have materialized as a four-component plane and every derivation reading it would have
            // folded three fabricated lanes, with the roster totality that makes the case dead standing one
            // roster edit away from no longer making it dead.
            : from format in PlaneFormat.For(channel.Components, ChannelDtype.Float32)
                  .ToFin(MaterialFault.Parameter(key, $"<implicit-format-absent:{channel.Key}:{channel.Components}>"))
              let implicitBinding = new ChannelBinding(channel, format, Some(MipPolicy.None),
                  Option<ChannelPack>.None, Seq<PlaneOp>(), Option<DisplayEgress>.None)
              from landed in channel.Origin is ChannelOrigin.Derived
                  ? Derive(program, plan, implicitBinding, carried, key, ticks, cancel)
                  : Land(program, plan, implicitBinding, carried, key, ticks, cancel)
              select landed;

    // The GPU lowering gate mirrors the plan admission exactly, so a caller that passed admission cannot fail
    // here for a reason admission could have named. It carries the PROVEN field forward beside the kernel: the
    // proof that this subject lowers is exactly the evidence Stage needs, and re-testing the union there would
    // mint an arm the admission already made unreachable.
    static Fin<(WgslKernel Kernel, TextureSource Field)> Lower(PressSubject subject, ChannelBinding binding, Op key) =>
        subject is PressSubject.Source source
            ? source.Field switch {
                TextureSource.Noise noise       => Fin.Succ((WgslKernel.NoiseField, (TextureSource)noise)),
                TextureSource.Checker checker   => Fin.Succ((WgslKernel.CheckerField, (TextureSource)checker)),
                TextureSource.Gradient gradient => Fin.Succ((WgslKernel.GradientField, (TextureSource)gradient)),
                TextureSource.Triplanar { Projected: TextureSource.Noise { Solid: true } } triplanar =>
                    Fin.Succ((WgslKernel.NoiseField, (TextureSource)triplanar)),
                _ => Fin.Fail<(WgslKernel, TextureSource)>(RasterFault.Device(key, $"<gpu-unlowerable-source:{binding.Channel.Key}>")),
            }
            : Fin.Fail<(WgslKernel, TextureSource)>(RasterFault.Device(key, $"<gpu-unlowerable-subject:{binding.Channel.Key}>"));

    // The uniform block is built through the ONE gpu#PRESS_DEVICE KernelUniform writer in the row's own declared
    // word order, so this dispatch and the row's golden fixture build the same layout and cannot disagree about
    // it. Every append names its own width: the noise block interleaves nine floats with nine integer codes
    // before its two colour vectors, and a float carrier over an octave-count slot hands the shader 0x40000000
    // read as a billion octaves. Every op code comes from WgslOpCode over the vocabulary's own key, so the
    // lowering never re-numbers a basis, a fractal, a metric, or a feature. Buffer POSITION is the @binding
    // index, so the sequence — uniform, then each read the row's Layout declares, then the one write sized by
    // the kernel's OWN WriteElements — IS the layout PressDevice.Guard checks against the row's roster; the
    // shared sizing derivation is what keeps a reduction row's buffer at groups x stride floats rather than a
    // texel-count formula that trips the storage ceiling at production extents.
    static KernelBinding Stage(PressPlan plan, WgslKernel kernel, TextureSource field) =>
        Seat(plan, kernel, Words(plan, field), Reads(field), kernel.Groups(plan.Width, plan.Height, plan.Layers));

    static KernelBinding Seat(PressPlan plan, WgslKernel kernel, KernelUniform words, Seq<ReadOnlyMemory<float>> reads, (uint X, uint Y, uint Z) groups) =>
        new(reads.Fold(Seq(words.Block), static (buffers, plane) => buffers.Add(new KernelBuffer.Read(plane)))
                .Add(new KernelBuffer.Write(kernel.WriteElements(plan.Width, plan.Height, plan.Layers))),
            groups.X, groups.Y, groups.Z);

    // The lowerable sources, each in its own Params order. Low and High cross as FOUR lanes because the CPU
    // arm lerps the source's own colours through ShadeVec4 — a scalar pair previews every authored ramp as
    // grey. Every code comes from WgslOpCode over the vocabulary's own key, so the lowering re-numbers no
    // basis, fractal, metric, or feature. The dimension word selects the noiseField 3D lattice: a solid Noise
    // crosses with the plan's layer count so the kernel derives the layer-centre depth the CPU volume law
    // reads, a triplanar over a solid Noise folds its world scale into the frequency word (its three planes
    // sample one world point, so the blend is the 3D field itself), and Vec4 pads to its own sixteen-byte
    // boundary so no caller counts the trailing words.
    static KernelUniform Words(PressPlan plan, TextureSource field) => field switch {
        TextureSource.Noise noise => NoiseWords(plan, noise, noise.Frequency, noise.Solid),
        TextureSource.Triplanar { Projected: TextureSource.Noise noise } triplanar =>
            NoiseWords(plan, noise, noise.Frequency * triplanar.Scale, solid: true),
        TextureSource.Checker checker =>
            KernelUniform.Empty.Extent(plan.Width, plan.Height).U32(checker.Repeats).Pad(1)
                .Vec4(ShadeVec4.FromColor(checker.Even)).Vec4(ShadeVec4.FromColor(checker.Odd)),
        TextureSource.Gradient gradient =>
            KernelUniform.Empty.Extent(plan.Width, plan.Height).U32(gradient.Lut.Count).U32(gradient.Vertical ? 1 : 0),
        _ => KernelUniform.Empty,
    };

    static KernelUniform NoiseWords(PressPlan plan, TextureSource.Noise noise, double frequency, bool solid) =>
        KernelUniform.Empty.Extent(plan.Width, plan.Height)
            .F32(frequency).F32(noise.Lacunarity).F32(noise.Gain).F32(noise.WeightedStrength)
            .F32(noise.PingPongStrength).F32(noise.Cellular.Jitter).F32(noise.Period.Value)
            .F32(noise.Warp.Amplitude).F32(noise.Warp.Frequency)
            .U32(noise.Octaves).I32(noise.Seed).Code(WgslOpCode.Of(noise.Base)).Code(WgslOpCode.Of(noise.Fractal))
            .Code(WgslOpCode.Of(noise.Cellular.Distance)).Code(WgslOpCode.Of(noise.Cellular.Return)).I32(noise.Warp.Seed)
            .U32(solid ? 1 : 0).U32(plan.Layers.Value)
            .Vec4(ShadeVec4.FromColor(noise.Low)).Vec4(ShadeVec4.FromColor(noise.High));

    // Only the gradient row is SAMPLED, and its read plane is the LUT the source already resolved in Oklch at
    // Gradient.Of — the perceptual hue path is priced once, host-side, and the shader reads an index lerp. The
    // flattening is four lanes per texel because a storage vec4<f32> imposes a sixteen-byte stride the host
    // unpacks anyway, so the buffer is a flat f32 run on both ends.
    static Seq<ReadOnlyMemory<float>> Reads(TextureSource field) {
        if (field is not TextureSource.Gradient gradient) { return Seq<ReadOnlyMemory<float>>(); }
        float[] lut = new float[gradient.Lut.Count * 4];
        for (int texel = 0; texel < gradient.Lut.Count; texel++) {
            ShadeVec4 stop = gradient.Lut[texel];
            (lut[texel * 4], lut[(texel * 4) + 1], lut[(texel * 4) + 2], lut[(texel * 4) + 3]) =
                ((float)stop.X, (float)stop.Y, (float)stop.Z, (float)stop.W);
        }
        return Seq<ReadOnlyMemory<float>>(lut);
    }

    // The readback widens back into the ShadeVec4 staging the CPU lane writes, so the preview crosses into the
    // plane substrate through exactly the same Fill rail and the plane's own Write quantizes it — the two lanes
    // differ in AUTHORITY, never in encoding. A short readback rails rather than filling the tail with zeros: a
    // truncated dispatch that reads as a black band is the failure a preview is least likely to be checked for.
    // A COUPLED mip policy downgrades to the box floor because the preview lane lands no companion plane to read
    // the variance from, which is the same floor the CPU lane records when a plan binds no companion.
    static Fin<TexturePyramid> Lift(PressPlan plan, ChannelBinding binding, KernelReceipt receipt, Op key) {
        int rows = plan.Height.Value * plan.Layers.Value;
        long texels = (long)plan.Width.Value * rows;
        if (receipt.Output.Length < texels * 4) {
            return Fin.Fail<TexturePyramid>(RasterFault.Device(key, $"<gpu-readback-short:{binding.Channel.Key}:{receipt.Output.Length}<{texels * 4}>"));
        }
        using MemoryOwner<ShadeVec4> staging = MemoryOwner<ShadeVec4>.Allocate(checked((int)texels), AllocationMode.Default);
        ReadOnlySpan<float> lanes = receipt.Output.Span;
        Span<ShadeVec4> decoded = staging.Span;
        for (int texel = 0; texel < decoded.Length; texel++) {
            int at = texel * 4;
            decoded[texel] = new ShadeVec4(lanes[at], lanes[at + 1], lanes[at + 2], lanes[at + 3]);
        }
        return TexturePlane.Of(binding.Format, plan.Width, plan.Height, binding.Channel.Transfer,
                binding.Format.Alpha.Carries ? plan.Alpha : AlphaMode.None, key, Some(plan.Layers))
            .Map(blank => Fill(blank, staging.Memory.AsMemory2D(rows, plan.Width.Value), binding.Display))
            .Bind(filled => TexturePyramid.Of(filled, binding.Policy.Coupled ? MipPolicy.Box : binding.Policy, key));
    }

    // The graph key is the COMPILED ORDER, absent for a graphless subject — a zero would read as a real key
    // shared by every field and slab press ever taken. Each node enters as its port id and its CASE TAG off
    // the union's own exhaustive Switch — a reflection Type.Name would silently re-key every graph ever
    // pressed on a case-record rename, and a trimmed/AOT rename would fork federation reproducibility; the
    // generated Switch instead breaks the build the day the union grows a case this tag fold does not name.
    static Option<UInt128> GraphKey(PressProgram program) =>
        program is PressProgram.Shaded shaded
            ? Some(ContentHash.Of(shaded.Graph, static (graph, digest) =>
                  graph.Order.Iter(node =>
                      digest.Append(Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{node.Id.Value}:{Tag(node)}"))))))
            : Option<UInt128>.None;

    static string Tag(AppearanceNode node) =>
        node.Switch(
            input:      static _ => "input",
            texture:    static _ => "texture",
            math:       static _ => "math",
            mix:        static _ => "mix",
            normal:     static _ => "normal",
            bsdfOutput: static _ => "bsdf-output");

    // Texels counts what was actually SHADED: every level of every pyramid at its own extent. Multiplying the
    // base extent by the level count over-counts a full chain by roughly three, and a throughput number built
    // on that reads faster than the press ran.
    static ulong Texels(TextureSet set) =>
        // HashMap<K,V> publishes no pair-shaped Fold — a 2-arg lambda binds the Foldable extension over VALUES
        // alone — so the pair walk re-enters through AsIterable.
        toSeq(set.Channels.AsIterable()).Fold(0UL, static (acc, pair) => acc + Levels(pair.Value)) + set.Packs.Fold(0UL, static (acc, pack) => acc + Levels(pack.Plane));

    static ulong Texels(PressPlan plan, HashMap<TextureChannel, TexturePyramid> planes) =>
        toSeq(planes.AsIterable()).Fold(0UL, static (acc, pair) => acc + Levels(pair.Value));

    static ulong Levels(TexturePyramid pyramid) =>
        pyramid.Levels.Fold(0UL, static (acc, level) => acc + ((ulong)level.Width.Value * (ulong)level.Height.Value * (ulong)level.Layers.Value));
}

// The band kernel: ONE dispatch on the compiled program per partition, then that arm's own row loop. Point,
// write, scratch, and shade spans all rent ONCE PER BAND — the program switch runs per band, so each arm owns
// its band loop and its rentals, and no row pays a pool round trip. Jitter mints a texel-local state through
// Deterministic.Stream over the (x, y, layer, ordinal) lanes and the FULL 64-bit plan seed, so partitioning
// cannot reorder a draw, two channels never share a jitter sequence, the whole seed reaches the draw lane-exact
// (a truncated half made two plans differing above bit 31 unreplayable from the receipt), and Position
// agrees with the jittered UV — a subject reading both sees ONE point. The LAYER axis is the plan's own
// LayerLaw evaluated per line: cubeFaces derives the frozen per-face DIRECTION into Position, volume the
// depth coordinate, frames the frame coordinate, so the layer rows are real evaluation shapes rather than
// near-identical copies wearing a law's name.
internal readonly struct PressRows(
    PressProgram program, PressPlan plan, TextureChannel channel, ShadePoint anchor,
    Memory2D<ShadeVec4> target, int band, int rows, ulong[] faulted, CancellationToken cancel, Op key) : IAction {
    public void Invoke(int slice) {
        int start = slice * band, end = Math.Min(rows, start + band);
        if (start >= end || cancel.IsCancellationRequested) { return; }
        using SpanOwner<ShadePoint> points = SpanOwner<ShadePoint>.Allocate(plan.Width.Value);
        using SpanOwner<ShadeVec4> write = SpanOwner<ShadeVec4>.Allocate(plan.Width.Value);
        switch (program) {
            case PressProgram.Shaded shaded: ShadedBand(shaded, start, end, points.Span, write.Span); break;
            case PressProgram.Field field: FieldBand(field, start, end, points.Span, write.Span); break;
            case PressProgram.Aged aged: AgedBand(aged, start, end, points.Span, write.Span); break;
            case PressProgram.Mixed mixed: MixedBand(mixed, start, end, points.Span, write.Span); break;
            case PressProgram.Dome dome: DomeBand(dome, start, end, points.Span, write.Span); break;
            case PressProgram.Surface surface: SurfaceBand(surface, start, end, start, write.Span); break;
            // The union is closed and every arm binds above; this tail exists only because the compiler cannot see
            // that closure. The arena is rented UNCLEARED, so an arm returning without writing would publish the
            // pool's last tenant as texels — the neutral fill is what makes the unreachable arm harmless.
            default: NeutralBand(start, end, write.Span); break;
        }
    }

    // The DOME band evaluates the frontier's radiance closure at each texel's own world direction. `Points` already
    // derived that direction into `Position` through the plan's `CubeFaces` law — the one frozen face
    // correspondence the CPU bake and the `equirectToCube` kernel both pin to — so this arm reads the point rather
    // than re-deriving a mapping, and the two lanes cannot disagree about which texel is which direction.
    void DomeBand(PressProgram.Dome dome, int start, int end, Span<ShadePoint> points, Span<ShadeVec4> write) {
        for (int line = start; line < end; line++) {
            if (cancel.IsCancellationRequested) { return; }
            Points(line, points);
            for (int x = 0; x < write.Length; x++) {
                RgbSpectrum radiance = dome.Radiance(new Vector3d(points[x].Position.X, points[x].Position.Y, points[x].Position.Z));
                write[x] = new ShadeVec4(radiance.R, radiance.G, radiance.B, 1.0);
            }
            Emit(line, write);
        }
    }

    // The SURFACE band traces the chart set. A gutter texel — one no chart covers — writes its channel NEUTRAL and
    // is traced from nothing, because a ray cast from a point that is not on the body measures the body's absence
    // rather than its occlusion; the `Dilate` rings the subject declares then fill those texels from their own
    // chart, which is what keeps the gutter from bleeding a neighbour's relief across a chart boundary at every
    // mip level. The cast itself reads the chart run directly rather than through `Points`, because a mesh-space
    // texel's position IS its chart entry and a jittered UV would sample a surface point it does not own.
    void SurfaceBand(PressProgram.Surface surface, int start, int end, int line, Span<ShadeVec4> write) {
        for (; line < end; line++) {
            if (cancel.IsCancellationRequested) { return; }
            ReadOnlySpan<ChartTexel> charts = surface.Charts.Span.Slice(line * write.Length, write.Length);
            for (int x = 0; x < write.Length; x++) {
                write[x] = charts[x].Coverage
                    ? surface.Measure(charts[x], channel)
                    : channel.Neutral;
            }
            Emit(line, write);
        }
    }

    void NeutralBand(int start, int end, Span<ShadeVec4> write) {
        Span2D<ShadeVec4> plane = target.Span;
        write.Fill(channel.Neutral);
        for (int line = start; line < end; line++) {
            for (int x = 0; x < write.Length; x++) { plane[line, x] = write[x]; }
        }
    }

    // The five sink columns vary per texel through the slot's OWN SurfaceShade reader; every other bound
    // channel reads the constant lowered vector, so a coat-roughness plane from a graph subject is honestly
    // constant rather than the base-colour column wearing another channel's name. A ShadeSpan failure fills
    // the row with the channel neutral AND tallies its texels — counted evidence, never a silent success.
    void ShadedBand(PressProgram.Shaded shaded, int start, int end, Span<ShadePoint> points, Span<ShadeVec4> write) {
        using SpanOwner<PortValue> scratch = SpanOwner<PortValue>.Allocate(shaded.Graph.ScratchWidth);
        using SpanOwner<SurfaceShade> shades = SpanOwner<SurfaceShade>.Allocate(points.Length);
        Span2D<ShadeVec4> plane = target.Span;
        // The slot probes ONCE per band, imperatively — a Match lambda cannot capture a stack-only SpanOwner,
        // so the arm branches on the probed row and the rentals stay band-scoped.
        SinkSlot? slot = channel.Slot.Case as SinkSlot;
        for (int line = start; line < end && !cancel.IsCancellationRequested; line++) {
            Points(line, points);
            if (slot is not null) {
                if (shaded.Graph.ShadeSpan(points, shaded.Row, scratch.Span, shades.Span, key).IsSucc) {
                    for (int x = 0; x < write.Length; x++) { write[x] = slot.Read(shades.Span[x]); }
                } else {
                    write.Fill(channel.Neutral);
                    Interlocked.Add(ref faulted[0], (ulong)write.Length);
                }
            } else {
                write.Fill(Constant(shaded.Constant, points[0]));
            }
            for (int x = 0; x < write.Length; x++) { plane[line, x] = write[x]; }
        }
    }

    // The field writes ONLY its own target — the plan admission binds a Source subject to one channel, and
    // this arm re-proves it per band so an Ensure intermediate for a DIFFERENT channel takes its origin law
    // (a geometric row reads the point's frame, anything else its neutral) instead of wearing the field's
    // bytes. A sample fault seats the neutral and tallies.
    void FieldBand(PressProgram.Field field, int start, int end, Span<ShadePoint> points, Span<ShadeVec4> write) {
        Span2D<ShadeVec4> plane = target.Span;
        bool targeted = channel == field.Target;
        for (int line = start; line < end && !cancel.IsCancellationRequested; line++) {
            Points(line, points);
            ulong faults = 0;
            for (int x = 0; x < write.Length; x++) {
                if (targeted) {
                    Fin<ShadeVec4> sampled = TextureUv.Sample(field.Source, Sample(points[x], 0.0), field.Sampler, key);
                    if (sampled.Case is ShadeVec4 texel) { write[x] = texel; } else { write[x] = channel.Neutral; faults++; }
                } else {
                    write[x] = channel.Origin is ChannelOrigin.Geometric geometric ? geometric.Read(points[x]) : channel.Neutral;
                }
            }
            if (faults > 0) { Interlocked.Add(ref faulted[0], faults); }
            for (int x = 0; x < write.Length; x++) { plane[line, x] = write[x]; }
        }
    }

    // The two spatial fields sample per texel and index the ladder's own CELL; the channel's lens then reads that
    // cell's OpenPBR column, so a spatially-aged bake pays one admission per cell and none per texel. An ABSENT
    // age field reads 0.0 — the UNAGED authored state — because a slab pressed without a spatial field is the
    // material as authored, and the silent fully-weathered default was the inverted intuition a caller discovers
    // only in the render; an ABSENT cavity field reads 1.0 — full cavity — because that is the column at which
    // every Crevice row ages at the raw age and the ladder collapses to the uniform trajectory a caller who never
    // bound the axis expects, where 0.0 would delete every crevice effect and run every exposed one at full age.
    // Both fields read Luminance, so a single-lane plane and an RGB plane both answer, and both share ONE fault
    // tally because a texel is one sample point whichever field refused. The arm stays TOTAL — the ladder's cells
    // were admitted at compile — and each visited cell folds into the coverage census beside the write.
    void AgedBand(PressProgram.Aged aged, int start, int end, Span<ShadePoint> points, Span<ShadeVec4> write) {
        Span2D<ShadeVec4> plane = target.Span;
        for (int line = start; line < end && !cancel.IsCancellationRequested; line++) {
            Points(line, points);
            ulong faults = 0;
            for (int x = 0; x < write.Length; x++) {
                double age = Field(aged.AgeField, aged.Sampler, points[x], fallback: 0.0, ref faults);
                double cavity = Field(aged.CavityField, aged.Sampler, points[x], fallback: 1.0, ref faults);
                // Curvature is the third field and its own absence extreme is FLAT — the middle of the signed
                // axis, at which every curvature-keyed row scales at unity and the axis contributes nothing.
                double curvature = Field(aged.CurvatureField, aged.Sampler, points[x], fallback: 0.0, ref faults);
                (double clampedAge, double clampedCavity) = (Math.Clamp(age, 0.0, 1.0), Math.Clamp(cavity, 0.0, 1.0));
                double clampedCurvature = Math.Clamp(curvature, -1.0, 1.0);
                aged.Coverage.Visit(AgeLadder.Rung(clampedAge, aged.Ladder.AgeRungs),
                    AgeLadder.Rung(clampedCavity, aged.Ladder.CavityRungs),
                    AgeLadder.Rung(AgeLadder.Signed(clampedCurvature), aged.Ladder.CurvatureRungs), clampedAge);
                write[x] = Constant(aged.Ladder.At(clampedAge, clampedCavity, clampedCurvature), points[x]);
            }
            if (faults > 0) { Interlocked.Add(ref faulted[0], faults); }
            for (int x = 0; x < write.Length; x++) { plane[line, x] = write[x]; }
        }
    }

    // The weight simplex samples one field PER PIGMENT into the band's own scratch, quantizes onto the barycentric
    // lattice, and reads the resolved cell — the same shape the age ladder takes, one dimension generalized. The
    // weights and counts rent ONCE PER BAND like every other span here, and a refused field seats a zero weight
    // and tallies, so a wholly-refused vector falls to the pure-first-pigment cell rather than a NaN mix.
    void MixedBand(PressProgram.Mixed mixed, int start, int end, Span<ShadePoint> points, Span<ShadeVec4> write) {
        Span2D<ShadeVec4> plane = target.Span;
        using SpanOwner<double> weights = SpanOwner<double>.Allocate(mixed.WeightFields.Count);
        using SpanOwner<int> counts = SpanOwner<int>.Allocate(mixed.WeightFields.Count);
        for (int line = start; line < end && !cancel.IsCancellationRequested; line++) {
            Points(line, points);
            ulong faults = 0;
            for (int x = 0; x < write.Length; x++) {
                for (int pigment = 0; pigment < weights.Span.Length; pigment++) {
                    weights.Span[pigment] = Field(Some(mixed.WeightFields[pigment]), mixed.Sampler, points[x], fallback: 0.0, ref faults);
                }
                write[x] = Constant(mixed.Ladder.At(weights.Span, counts.Span), points[x]);
            }
            if (faults > 0) { Interlocked.Add(ref faulted[0], faults); }
            for (int x = 0; x < write.Length; x++) { plane[line, x] = write[x]; }
        }
    }

    // The ONE spatial-field read both ladder arms take: an absent field answers its own declared fallback, a
    // refused sample answers the same fallback and tallies, and a landed sample answers its luminance — so the
    // absence law and the fault law are stated once rather than per field per arm.
    double Field(Option<TextureSource> source, SamplerState sampler, ShadePoint point, double fallback, ref ulong faults) {
        if (source.Case is not TextureSource field) { return fallback; }
        Fin<ShadeVec4> sampled = TextureUv.Sample(field, Sample(point, 0.0), sampler, key);
        if (sampled.Case is ShadeVec4 texel) { return texel.Luminance; }
        faults++;
        return fallback;
    }

    // One line of points: both-axis coordinate-keyed jitter, Position in agreement with the jittered UV, and
    // the LAYER coordinate the plan's own law derives. The per-texel state mints through the kernel
    // Deterministic.Stream over the (x, y, layer, ordinal) lanes and the FULL 64-bit plan seed — lane-exact,
    // so the receipt column replays the draw with no half-splitting across two int salts — and the two axis
    // draws advance that texel-local state through NextUnit; the state derives from the coordinate, never a
    // sequential stream, so a band partition cannot reorder a draw.
    void Points(int line, Span<ShadePoint> points) {
        int width = plan.Width.Value, height = plan.Height.Value;
        int layer = line / height, y = line % height;
        // The two LINE-INVARIANT reads hoist out of the texel walk. `Ordinal` is a frozen roster index behind a
        // lazy accessor — a dictionary probe — and the seed cast is a conversion; spelling either inside the loop
        // paid both on every one of sixteen million texels for values that cannot change across a row. What stays
        // inside is arithmetic alone: the lane run is a stack-allocated span the collection expression fills, so
        // the ingress allocates nothing, and the state derives from the FULL coordinate exactly as before. The
        // draw stays coordinate-keyed and the owner's stream ingress stays the one mint — deriving a per-texel
        // state by advancing a line state would re-transcribe the owner's private gamma, which is the deleted form.
        int ordinal = channel.Ordinal;
        long seed = unchecked((long)plan.Seed);
        for (int x = 0; x < width; x++) {
            ulong state = Deterministic.Stream([x, y, layer, ordinal], seed);
            double ju = Deterministic.NextUnit(ref state);
            double jv = Deterministic.NextUnit(ref state);
            double u = (x + ju) / width, v = (y + jv) / height;
            points[x] = plan.Law == LayerLaw.CubeFaces
                ? anchor with { U = u, V = v, Position = Face(layer, u, v) }
                : anchor with { U = u, V = v, Position = new Point3d(u, v, LayerCoord(layer)) };
        }
    }

    // volume: the depth coordinate at the layer's own slab centre; frames: the frame coordinate on [0,1];
    // array/none: the layer index normalized — three laws, one column, each a real fact of its row.
    double LayerCoord(int layer) =>
        plan.Law == LayerLaw.Volume ? (layer + 0.5) / plan.Layers.Value
        : plan.Layers.Value > 1 ? layer / (double)(plan.Layers.Value - 1)
        : 0.0;

    // The FROZEN cube-face mapping, transcribed from the gpu#WGSL_KERNEL faceDir fragment — both texts pin to
    // the freeze [06] equirect correspondence, so the CPU cube bake and the GPU cube kernel address one law.
    static Point3d Face(int face, double u, double v) {
        double s = (2.0 * u) - 1.0, t = (2.0 * v) - 1.0;
        (double x, double y, double z) = face switch {
            0 => (1.0, -s, -t),
            1 => (-1.0, s, -t),
            2 => (s, 1.0, t),
            3 => (s, -1.0, -t),
            4 => (s, -t, 1.0),
            _ => (-s, -t, -1.0),
        };
        double length = Math.Sqrt((x * x) + (y * y) + (z * z));
        return new Point3d(x / length, y / length, z / length);
    }

    // One channel-value projection over the roster's own origin law: a shaded row reads its lens, a geometric
    // row reads the point's frame, a derived row is produced by filter#PLANE_OP and never reaches this kernel.
    ShadeVec4 Constant(OpenPbrSurface vector, ShadePoint point) =>
        channel.Origin switch {
            ChannelOrigin.Shaded shaded => shaded.Lens.Read(vector),
            ChannelOrigin.Geometric geometric => geometric.Read(point),
            _ => channel.Neutral,
        };

    static UvSample Sample(ShadePoint point, double mip) =>
        new(UnitInterval.Create(Math.Clamp(point.U, 0.0, 1.0)), UnitInterval.Create(Math.Clamp(point.V, 0.0, 1.0)), Vector3d.Zero, Vector3d.ZAxis, mip);
}
```

## [04]-[PRESS_RECEIPT]

- Owner: `PressProduct` `[Union]` the content-identity veto made structural; `PressReceipt` the bake evidence.
- Cases: product {`Minted` (a CPU-pressed `TextureSet` with its key, wire-legal and persistable), `Preview` (GPU-pressed planes with no set, no key, and nothing addressable)}.
- Entry: the union IS the entry — a consumer matching on `PressProduct` reaches a `TextureSet` only through the `Minted` arm, so a GPU-keyed set is unrepresentable rather than merely forbidden.
- Receipt: `PressReceipt` carries the backend, the plan key, the graph key where one exists, the replay seed, the true shaded texel count, the measured wall time, the per-channel `filter#PLANE_OP` `PlaneReceipt` evidence, the channels whose paired mip policy downgraded, the per-channel neutral-fill fault tally, the parity delta a two-lane run measured, and the `AgeCoverage` census an aged press exercised.
- Packages: `Rasm` (project — `IValidityEvidence`/`ValidityClaim` the corpus evidence floor), `set#TEXTURE_SET` (composed — `TextureSet`/`TextureChannel`), `plane#TEXTURE_PLANE` (composed — `TexturePyramid`), `filter#PLANE_OP` (composed — `PlaneReceipt`), LanguageExt.Core.
- Growth: a new receipt column is one field the fold already computes, because the plan key names the request and the receipt names the run; a new product class is a new case, and the two that exist partition the authority question.
- Boundary: the `Preview` case carries planes and a receipt and NOTHING addressable — no set, no key, no digest — so a GPU result cannot be persisted, wired, or content-addressed by accident, and the structural veto costs no runtime check anywhere downstream. Every measured column is a TYPED ABSENCE where nothing measured it: `GraphKey` is absent for a field or slab subject rather than a zero every graphless press would share, `GpuDeltaMax` — the frozen `[04.4]` spelling, carried under the wire's own name so the `[Mapper]` projection stays mechanical — is absent for a single-lane press rather than a zero the parity gate reads as a perfect match, and `Aging` is absent for every program but the aged one rather than a zero-span census a gate reads as a one-cell ladder, exactly the forged-zero the corpus refuses on every tally, level, and receipt field; the declared-versus-visited pair on that census is what makes an over-quantized ladder and an unexercised cavity dimension legible from the receipt rather than from a second press; at the wire the interior lowers once, the `Option<double>` to the proto optional and the absent `GraphKey` to the empty string, mirroring `materialId`'s empty for an acquired set. `PressProduct.Parity` is the ONE producer that fills the delta — it folds the per-channel maximum over both lanes' base-level decoded rows and stamps the minted receipt — and the `Projection/benchmarks` parity workload COMPOSES it by pressing one plan on both lanes; a measurement fold living only inside a benchmark leaves the receipt column with no owner in the engine that declares it. `GraphKey` folds the COMPILED ORDER — each node's port id then its case name — because the frozen topological sort is what the bake evaluated, so a re-authored graph whose nodes reorder textually but compile to one order keys identically, and a graph whose evaluation order genuinely changed keys differently. `Texels` sums every level's own extent rather than multiplying the base by the level count, so texels-per-second is a throughput number rather than one inflated threefold by a full chain. `Planes` carries each channel's own `PlaneReceipt`, so the height solver's true relative residual reaches the benchmark rather than dying inside the fold — and `Residual` selects it DETERMINISTICALLY, the height channel first then roster order, where a hash-order enumeration handed a different channel's number per run; `Downgraded` names every channel whose paired mip policy fell back to the box floor, and `Faulted` every channel whose band kernel neutral-filled under a failure with its texel tally, so both quality decisions the press made silently become decisions the receipt reports. `IsValid` reads what the receipt alone can prove: `webgpu` beside a graph key or an aging census is invalid evidence, because the accelerator lane refuses both graph and ladder subjects at plan admission and a receipt contradicting that law was forged, and a census claiming more visited rungs than the plan declared or an inverted age span is a fabricated column rather than a measured one — the STRONGER Minted-authority gate lives where the set meets the wire, `interchange#TEXTURE_EGRESS` `TextureSetWire.Of` proving every press receipt content-authoritative, and this page names that owner rather than claiming a check the receipt's own columns cannot see.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit.)

// --- [MODELS] ------------------------------------------------------------------------------
// The content-identity veto AS A TYPE: the GPU arm has no TextureSet to return, so a GPU-keyed plane has no
// spelling. Amendment-grade law that costs zero runtime checks downstream.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressProduct {
    private PressProduct() { }
    public sealed record Minted(TextureSet Set, PressReceipt Receipt) : PressProduct;
    public sealed record Preview(HashMap<TextureChannel, TexturePyramid> Planes, PressReceipt Receipt) : PressProduct;

    public PressReceipt Evidence => Switch(minted: static m => m.Receipt, preview: static p => p.Receipt);

    // The ONE producer of GpuDeltaMax, so the column has an owner the moment both lanes of a plan exist rather
    // than a shape only a harness knows how to fill. The measure is the per-channel maximum absolute difference
    // over the BASE level's DECODED rows: the mip chain re-derives from that level on both lanes, so a coarser
    // divergence is the base's own restated, and the decoded rail measures the quantity the lanes disagree
    // about instead of a storage-format difference between an f32 preview and a unorm plane. A channel the
    // preview lane never produced contributes NOTHING — the accelerator answers one lowered source and the rest
    // are UNMEASURED, where a difference against a neutral plane reports full-scale divergence for a channel
    // nobody dispatched. An empty intersection therefore leaves the column ABSENT, the same typed absence a
    // single-lane press reads, and an extent disagreement REFUSES rather than resampling, because a delta
    // across two grids measures the resampler. The stamp lands on the MINTED receipt: the CPU lane is the
    // authority the delta is measured against, and the preview receipt carries no key to file the number under.
    public static Fin<PressReceipt> Parity(Minted minted, Preview preview, Op key) =>
        minted.Receipt.PlanKey != preview.Receipt.PlanKey
            ? MaterialFault.Parameter(key, $"<parity-plan-mismatch:{minted.Receipt.PlanKey:x}:{preview.Receipt.PlanKey:x}>")
            : toSeq(preview.Planes.AsIterable())
                .Fold(Fin.Succ(Option<double>.None), (worst, entry) => worst.Bind(carried =>
                    minted.Set.Channels.Find(entry.Key)
                        .Map(cpu => Divergence(cpu.Base, entry.Value.Base, key)
                            .Map(delta => Some(carried.Map(seen => Math.Max(seen, delta)).IfNone(delta))))
                        .IfNone(Fin.Succ(carried))))
                .Map(delta => minted.Receipt with { GpuDeltaMax = delta });

    // One streaming pass over both planes' decoded rows, no arena beyond the two row scratches, and no sampler
    // anywhere — the comparison is texel-for-texel over the same lattice or it is a refusal.
    static Fin<double> Divergence(TexturePlane cpu, TexturePlane gpu, Op key) {
        if (cpu.Width != gpu.Width || cpu.Height != gpu.Height || cpu.Layers != gpu.Layers || cpu.Lanes != gpu.Lanes) {
            return MaterialFault.Parameter(key, $"<parity-extent-mismatch:{cpu.Width.Value}x{cpu.Height.Value}x{cpu.Layers.Value}:{gpu.Width.Value}x{gpu.Height.Value}x{gpu.Layers.Value}>");
        }
        using SpanOwner<double> left = SpanOwner<double>.Allocate(cpu.RowScalars);
        using SpanOwner<double> right = SpanOwner<double>.Allocate(gpu.RowScalars);
        double worst = 0.0;
        for (int layer = 0; layer < cpu.Layers.Value; layer++) {
            for (int row = 0; row < cpu.Height.Value; row++) {
                cpu.Read(row, layer, left.Span);
                gpu.Read(row, layer, right.Span);
                for (int lane = 0; lane < left.Span.Length; lane++) { worst = Math.Max(worst, Math.Abs(left.Span[lane] - right.Span[lane])); }
            }
        }
        return Fin.Succ(worst);
    }
}

// Every measured column is a TYPED ABSENCE where nothing measured it — a graphless press has no graph key and
// a single-lane press measured no divergence, and a zero in either reads to a gate as a real value. The wire
// spelling is the frozen [04.4] gpuDeltaMax, so the member carries the SAME name and the [Mapper] projection
// stays mechanical; the interior Option lowers at WireCodec to the proto optional, and the graph key's
// absence lowers to the empty string exactly as materialId lowers an acquired set's absent family. Texels
// sums each level's own extent, so texels-per-second is throughput rather than a headline. Faulted names
// every channel whose band kernel neutral-filled under a failure with the texel tally — the one column that
// separates a genuinely-neutral plane from a plane whose evaluation died and dressed as one — and a caller
// gates on it exactly as it gates on Downgraded.
// The ladder census an aged press exercised: the three DECLARED rung counts beside the three VISITED counts and the
// sampled age span. Declared-versus-visited is the whole point — the ratio names an over-quantized ladder, a
// visited cavity count of one names a dimension the fields never drove, and a curvature visited count of one on a
// multi-rung declaration names a curvature field that never varied — so each pair travels together and neither
// half is derivable from the other.
public readonly record struct AgeCoverage(
    int AgeRungs, int CavityRungs, int CurvatureRungs, int AgeRungsVisited, int CavityRungsVisited,
    int CurvatureRungsVisited, double AgeMin, double AgeMax);

public sealed record PressReceipt(
    PressBackend Backend, UInt128 PlanKey, Option<UInt128> GraphKey, ulong Seed, ulong Texels, double ElapsedMs,
    HashMap<TextureChannel, PlaneReceipt> Planes, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted,
    Option<double> GpuDeltaMax, Option<AgeCoverage> Aging)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast((int)Math.Min(Texels, int.MaxValue), 1),
        ValidityClaim.Nonnegative(ElapsedMs),
        ValidityClaim.Of(GpuDeltaMax.ForAll(static d => double.IsFinite(d) && d >= 0.0)),
        ValidityClaim.Of(Backend.ContentAuthoritative || GraphKey.IsNone),
        // A census claiming more visited rungs than the plan declared, or an inverted age span, is a forged
        // column rather than a measured one — the two facts the receipt alone can prove about its own ladder.
        ValidityClaim.Of(Aging.ForAll(static c =>
            c.AgeRungsVisited is > 0 && c.AgeRungsVisited <= c.AgeRungs
            && c.CavityRungsVisited is > 0 && c.CavityRungsVisited <= c.CavityRungs
            && double.IsFinite(c.AgeMin) && c.AgeMin <= c.AgeMax)),
        ValidityClaim.Of(Backend.ContentAuthoritative || Aging.IsNone));

    public bool ContentAuthoritative => Backend.ContentAuthoritative;
    public double TexelsPerSecond => ElapsedMs > 0.0 ? Texels / (ElapsedMs / 1000.0) : 0.0;
    // DETERMINISTIC selection: the height solve is the residual that matters, so the height channel answers
    // first and the roster order breaks every remaining tie — hash-order enumeration handed a different
    // channel's residual per run, which is a benchmark reading noise as signal.
    public Option<double> Residual =>
        Planes.Find(TextureChannel.Height).Bind(static receipt => receipt.Residual)
            .Match(
                Some: Some,
                None: () => toSeq(TextureChannel.Items).Choose(c => Planes.Find(c).Bind(static r => r.Residual)).Head);
}
```

## [05]-[RESEARCH]

(none)
