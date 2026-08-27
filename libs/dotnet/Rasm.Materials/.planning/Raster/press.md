# [MATERIALS_PRESS]

THE BAKE ENGINE. One `TexturePress.Press` fold drives a `PressSubject` — a compiled `graph#MATERIAL_GRAPH` node program, a raw `texture#TEXTURE_UV` procedural field, a `surface#OPENPBR_SLAB` parameter vector under a spatially-varying aging trajectory, or a `finish#FINISH` Kubelka-Munk pigment mix under a spatially-varying weight simplex — across a `PressPlan`'s texel grid and mints a `set#TEXTURE_SET` `TextureSet` of content-keyed `plane#TEXTURE_PLANE` pyramids. The four subjects compile ONCE into one `PressProgram`, and the row kernel dispatches on that program per BAND rather than per texel, so a plane costs one dispatch per partition and one allocation-free pass per row. The two spatially-varying subjects share one quantization law: a fallible per-texel admission is unaffordable at sixteen million points, so each compiles a LADDER of admitted vectors over its own declared rung columns — the aging ladder over the `(age, cavity)` PRODUCT the `weathering#WEATHERING` `CavityResponse` rows make irreducible to one scalar, the mix ladder over the barycentric weight simplex — and every texel indexes the cell its fields sampled, so a spatially-varying bake pays a declared cell count rather than a per-texel admission. Batching rides `graph#MATERIAL_GRAPH` `CompiledGraph.ShadeSpan`: the port environment resolves ONCE into an index-addressed scratch whose slot order IS the frozen compiled sort, so a plane never rebuilds an immutable map per node per texel — the difference between minutes and days at four thousand square; the integrator's per-point `Shade` re-enters the SAME path over a one-element window, so the press and the integrator drive one evaluation law. The band fold rides `ParallelHelper.For` over a seeded struct `IAction`, and every per-texel jitter derives from the TEXEL COORDINATE and the plan seed rather than from a sequential stream, so a band partition can never reorder a draw and a re-press at one seed is byte-identical at any processor count.

Persisted plane bytes are ALWAYS CPU-minted. The `PressBackend.WebGpu` row is an accelerator and preview lane whose output is never content-addressed, and that is a STRUCTURAL veto rather than an empirical tolerance: `PressProduct` gives the GPU arm a `Preview` case carrying planes and its `PressRun` but NO `TextureSet`, so a GPU-keyed set has no spelling and cannot be persisted, wired, or addressed by accident. GPU `f32` cannot reproduce the CPU `f64` procedural lattice, so a GPU-keyed plane forks the content key at its own preimage; the divergence the parity workload measures rides `PressRun.GpuDeltaMax` as TELEMETRY and never enters a key. Every measured column is a TYPED ABSENCE when nothing measured it, because a fabricated zero and an unmeasured pass are the two states a gate exists to separate — the graph key of a graphless subject, the parity delta of a single-lane press, the ladder coverage of an unaged run, and the wall time of an unclocked run all read absence rather than zero. The page composes `set#TEXTURE_SET` for the produced bundle, its channel roster, and each slot's `Read` projection, `plane#TEXTURE_PLANE` for the arena and the pyramid, `filter#PLANE_OP` for every post chain, every derived channel, and its `PlaneTrace` evidence, `tile#TILE_SYNTH` for the in-fold tiling a plan requests, `gpu#PRESS_DEVICE` for the accelerator arm, `weathering#WEATHERING` for the aging ladder, `finish#FINISH` for the pigment-mix ladder, `surface#OPENPBR_SLAB` `ToneMap` for the one display egress a binding declares, the kernel `Deterministic` splitmix64 draw, `ContentHash` identity, and `ValidityClaim` validity fold, `TimeProvider` for the one measured wall time, and CommunityToolkit.HighPerformance for every pooled arena and partitioned band — reminting no evaluator, no arena, no random source, no clock, no tone curve, and no identity.

## [01]-[INDEX]

- [02]-[PRESS_PLAN]: the `PressBackend` axis, the `PressSubject` union, the `ChannelBinding` row with its `DisplayEgress` column, the `LadderRungs` quantization carrier, the `PressPlan` record with its canonical plan key, and the binding-order law that seats derived channels after their sources and paired channels after their companions.
- [03]-[TEXTURE_PRESS]: the one `TexturePress.Press` entry, the `PressProgram` compiled subject, the `AgeLadder` two-dimensional aged-vector table and the `MixLadder` weight-simplex table, the `PressRows` band kernel over `ParallelHelper.For`, the coordinate-keyed jitter law, the paired mip resolution, the GPU lowering gate, and the post-fold and tiling composition.
- [04]-[PRESS_PRODUCT]: the `PressProduct` union that makes the content-identity veto structural, and the `PressRun` record with its per-channel `PlaneTrace` rows, its `AgeCoverage` ladder-exercise column, and its typed absences.

## [02]-[PRESS_PLAN]

- Owner: `PressPlan` the bake request; `PressSubject` `[Union]` the thing being baked; `ChannelBinding` the per-channel request row; `DisplayEgress` the per-binding scene-to-display policy; `LadderRungs` the quantization carrier every ladder reads; `PressBackend` `[SmartEnum<string>]` the execution lane.
- Cases: subject {`Graph` (a `MaterialGraph` with the parameter row and conductor its sink resolves against), `Source` (one `TextureSource` sampled through a `SamplerState` into one channel), `Slab` (a `MaterialParameters` row lowered to the OpenPBR vector, aged per texel by the age, cavity, and curvature fields), `Mix` (a `finish#FINISH` pigment set resolved per texel through a `TextureSource` weight field per pigment), `Sky` (a frontier-supplied radiance closure over a world direction), `MeshSpace` (an already-flattened chart run carried as data, with its ray target)} · backend {`cpu` (content-authoritative), `webgpu` (accelerator, never content-authoritative)}.
- Law: binding ORDER is derived, never authored — `Of` sorts bindings by `TextureChannel.Origin` depth, then by pair dependency, then by `TextureChannel.Ordinal`, so every `Shaded` and `Geometric` channel seats before any `Derived` one, a normal seats before the roughness whose mip fold consumes its variance, and a plan requesting `occlusion` without `height` produces `height` as an intermediate rather than refusing. A caller never sequences the fold.
- Law: spatial cavity evidence enters the press as its OWN field, never as a derived channel. A `Slab` subject's own `occlusion` chain derives from `height`, which derives from `geometry_normal`, whose origin is a CONSTANT the shade point never reaches — so a slab press solves a flat height field and produces a uniform-1.0 occlusion plane by construction, and no binding order could rescue it because `Compile` is the first generator of the fold and nothing is landed when the ladder is read.
- Law: the cavity field carries the CAVITY scalar — `1.0` the fully occluded crevice — while the `set#TEXTURE_SET` `occlusion` channel stores VISIBILITY (`filter#PLANE_OP` deposits `open/rays` and the row's own neutral is `1.0` unoccluded), so an occlusion plane crosses into a cavity field through the landed `RemapCurve.Levels.Invert` row and never by a raw bind; the raw bind ages every `Crevice` effect on the open face and reads as a plausible plane rather than as a fault. An ABSENT cavity field reads `1.0` because a `Crevice` effect with no cavity evidence is the uniform aging the ladder already spells, while `0.0` would silently delete every crevice effect and run every exposed one at full age.
- Entry: `public static Fin<PressPlan> Of(PressPlanDraft draft, PressSubject subject)` is the ONE plan admission — extent, layer law, binding uniqueness, pack membership, format width, display egress, ladder rungs, subject arity, ladder cell budget, tile-guide coverage, and backend lowerability all gate here so the bake fold itself carries no re-check; the subject enters the admission because arity, cell budget, and lowerability are facts of the SUBJECT against the bindings, and deferring them to dispatch means a caller learns the veto after renting a device; a cyclic `Derived.From` chain refuses here through the roster-bounded depth walk, so the bake fold recurses on a proven-acyclic roster. `PlanKey` is the canonical content key the run records and the cache keys on.
- Packages: `set#TEXTURE_SET` (composed — `TextureChannel`/`ChannelPack`/`LayerLaw`/`TextureSet`/`SinkSlot`), `plane#TEXTURE_PLANE` (composed — `PlaneFormat`/`PlaneQuantity`/`MipPolicy`/`AlphaMode`), `filter#PLANE_OP` (composed — `PlaneOp` the post chain), `tile#TILE_SYNTH` (composed — `TilePolicy` the in-fold tiling request), `Rasm.Materials.Appearance.Graph` (composed — `MaterialGraph`/`MaterialParameters`), `Rasm.Materials.Appearance.Surface` (composed — `ConductorMetal`, `ToneOperator`/`DisplayEncoding` the display egress rows), `Rasm.Materials.Appearance.Texture` (composed — `TextureSource`/`SamplerState`/`UvFrame`), `Rasm.Materials.Appearance.Weathering` (composed — `WeatheringDose`/`AgeParameter`), `Rasm.Materials.Appearance.Finish` (composed — `FinishKind`/`FinishLayer`/`Pigment` the mix subject's own vocabulary), `Rasm.Element.Composition` (the CONTRACT `MaterialId`), `Rasm` (project — `ContentHash.Of` the one identity entry, `Dimension`, `UnitInterval`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new bake subject is one `PressSubject` case with its `PressProgram` arm; a new execution lane is one `PressBackend` row carrying its authority column; a new per-channel request knob is one `ChannelBinding` column; a new quantized axis is one `LadderRungs` column read by the ladder that owns it, priced against the cell product it multiplies. There is NO `BakeGraph`/`BakeField`/`BakeSlab`/`BakeMix` family — the subject's own case discriminates, and a caller holding a subject calls one entry, which is why a dome and a chart set land as cases rather than as a second engine each.
- Boundary: `PressBackend` carries `ContentAuthoritative` as a ROW COLUMN rather than as a caller flag, so the content-identity law is data the plan admission reads and `[04]-[PRESS_PRODUCT]` enforces at the type level. Mip policy is a per-binding `Option<MipPolicy>` defaulting to the channel row's own law and spelling `MipPolicy.None` for a single-level plane — a plan-level `Mips` boolean beside a per-binding override is one knob selecting between two bodies, and the row already carries the answer. The plan key is `ContentHash.Of` over the plan's canonical bytes — extent, layer law, the ordered binding rows with their channels, formats, resolved mip policies, pack keys, post chains, and display egress rows, the backend key, the seed, the alpha mode, the height scale, the three `LadderRungs` columns, the tile policy, and the subject's own `UvFrame` digest — and it EXCLUDES the material id and the conductor, which name the subject rather than the bake, so two materials pressed under one plan share a plan key and the run separates them by graph key. The UV frame is the one subject-borne column the key ADMITS, and the discriminant is what the column does: a material id and a conductor NAME the subject, while a `UvFrame` offset, scale, or rotation SHAPES the bake — `texture#TEXTURE_UV` `TextureUv.Sample` applies it before the source dispatch, so a re-tiled bake is different bytes, and a frame outside the preimage is a cached plane a second tiling silently inherits. `UvFrame.Digest` reads EMPTY at identity by its owner's construction, so an untransformed bind keys byte-identically to a plan that never knew the axis existed and landing the column re-keys no blob already addressed. `Layers` and `LayerLaw` ride the plan so a cube map, a flipbook, and a volume are one bake shape at different rows; a UDIM set is N plans sharing a key, never one plan carrying a tile list, because a UDIM tile is an independent extent whose planes address independently — the per-tile products assemble at `set#TEXTURE_SET` `UdimSheet.Of`, the one owner proving the tiles agree. A binding naming a channel already inside a requested pack REFUSES at admission — the pack owns those slots and a standalone duplicate keys the set twice for one field — and a binding whose `Format` carries fewer components than its channel declares refuses for the same reason `set#TEXTURE_SET` refuses it later: a three-component normal in a two-component plane is a reconstruction the sampler cannot invert without evidence the plane does not carry. A `Source` subject binds EXACTLY ONE channel, because a procedural field has one value and a second bound channel would silently receive its neutral; a `Mix` subject binds one weight field PER PIGMENT, because a mix whose simplex is short one axis resolves a pigment nothing weights; a `Tile` policy whose guide channel no binding produces refuses, because the synthesizer would fail on a set that admitted cleanly. A binding whose channel carries an OPEN photometric scale — `PlaneQuantity.Light` beside a `ChannelUnit` other than `none`, which is `emission_luminance` alone on the landed roster — stored in a `PlaneFormat` the plane page reports `Normalized` with NO `Display` egress refuses at admission: an unbounded cd/m² value hard-clips at unity in a unorm lane with no tone curve, and a clipped emission plane is indistinguishable from an authored one downstream. The lane test reads `plane#PLANE_FORMAT` `Normalized`, the module's one unorm-versus-float discriminant, because the kernel `ChannelDtype` roster the format seats onto carries no normalization column of its own. The `webgpu` backend gates TWO independent facts at admission, not at dispatch, and the split is what keeps each honest: the SUBJECT's lowerability is a fact of the subject alone, and the EXTENT is measured against `PressBackend.TexelCeiling`, the conformance FLOOR every device grants, so a bake no device could run refuses before one is rented. That ceiling is arithmetic, not a guess: `134217728` is exactly the guaranteed minimum for `maxStorageBufferBindingSize`, the lane binds ONE storage buffer per plane, and `134217728 / 16 = 8388608` texels — so a square accelerator preview tops out at `2048²` and a `4096²` request refuses at admission rather than at dispatch. A plan clearing the floor can still exceed what a particular adapter negotiated, and that refusal belongs at `gpu#PRESS_DEVICE`'s dispatch gate, which reads the device's own `DeviceGetLimits` block and quotes the granted value; collapsing the two would either rent a device to answer a plan question or assert a ceiling nothing measured. Lowerability itself: a `Source` subject over an `Image` case or a `Triplanar` whose projected source is not a solid `Noise` (the three-plane 2D blend has no kernel arm), or a `Slab`, `Mix`, or `MeshSpace` subject, has no kernel row on `gpu#WGSL_KERNEL` and refuses with the offending case named, so a caller learns the veto before renting a device rather than after. A `Graph` subject LOWERS as a KERNEL CHAIN — one dispatch per node in the compiled topological order over `gpu#KERNEL_CHAIN`'s ping-ponged slot pool, a field kernel per procedural `Texture` node, `mathFold` per `Math` node, `mixFold` per `Mix` node — and its verdict is the ALLOCATOR's rather than a case test: the chain plans by linear-scan live-range analysis over that order, its slot count is the DAG's maximum live width, and `slots × extent × 16` admits against the declared footprint or the accelerator refuses with the slot count and the budget named. Its refusals are NODE-grained, so a caller learns which node vetoed rather than that "the graph" did, and a refused chain refuses the accelerator alone — the CPU lane is content-authoritative anyway, so the recourse costs throughput and nothing else. The two LADDER subjects sit outside the question entirely: each compiles CPU-admitted cells a GPU arm could only rebuild at `f32`, forking the key the veto holds. A `Noise` source — planar OR solid — and a `Triplanar` over a solid `Noise` lower to `noiseField`: the solid family rides the row's `dimension` column, a triplanar's three planes sample one world point for a solid projected noise so the blend IS the 3D field and the world scale folds into the frequency word, and the widening moves no identity law — the accelerator product stays a `Preview`, CPU bytes stay canonical.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using System.Numerics.Tensors;
using System.Text;
using System.Threading;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Composition;
using Rasm.Materials.Appearance;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Finish;
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Surface;
using Rasm.Materials.Appearance.Texture;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PressBackend {
    public static readonly PressBackend Cpu    = new("cpu",    contentAuthoritative: true,  texelCeiling: long.MaxValue);
    public static readonly PressBackend WebGpu = new("webgpu", contentAuthoritative: false, texelCeiling: LoweredFloor / BytesPerTexel);
    public bool ContentAuthoritative { get; }

    public long TexelCeiling { get; }

    const long BytesPerTexel = 16;

    const long LoweredFloor = 134_217_728;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressSubject {
    private PressSubject() { }
    public sealed record Graph(MaterialGraph Program, MaterialParameters Row, ConductorMetal Conductor) : PressSubject;
    public sealed record Source(TextureSource Field, SamplerState Sampler, TextureChannel Target) : PressSubject;
    public sealed record Slab(MaterialParameters Row, ConductorMetal Conductor, Seq<WeatheringDose> Aging,
        Option<TextureSource> AgeField, Option<TextureSource> CavityField, Option<TextureSource> CurvatureField,
        SamplerState Sampler) : PressSubject;
    public sealed record Mix(FinishKind Kind, Seq<FinishPigment> Pigments, Seq<TextureSource> WeightFields,
        Seq<FinishLayer> Stack, Option<MaterialParameters> Substrate, ConductorMetal Conductor, SamplerState Sampler) : PressSubject;

    public sealed record Sky(Func<Vector3d, RgbSpectrum> Radiance, TextureChannel Target) : PressSubject;

    public sealed record MeshSpace(
        ReadOnlyMemory<ChartTexel> Charts, Dimension ChartWidth, MaterialParameters Row, ConductorMetal Conductor,
        double RayDistance, int Rays, int GutterRings) : PressSubject;

    public Option<UvFrame> Frame => Switch(
        graph:     static _ => Option<UvFrame>.None,
        source:    static s => Some(s.Sampler.Frame),
        slab:      static b => Some(b.Sampler.Frame),
        mix:       static m => Some(m.Sampler.Frame),
        sky:       static _ => Option<UvFrame>.None,
        meshSpace: static _ => Option<UvFrame>.None);

    public string Case => Switch(
        graph:     static _ => "graph",
        source:    static _ => "source",
        slab:      static _ => "slab",
        mix:       static _ => "mix",
        sky:       static _ => "sky",
        meshSpace: static _ => "meshSpace");
}

public readonly record struct ChartTexel(Point3d Position, Vector3d Normal, Vector3d Tangent, bool Coverage);

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct LadderRungs(int Age, int Cavity, int Curvature, int Mix) {
    public static readonly LadderRungs Default = new(Age: 16, Cavity: 8, Curvature: 4, Mix: 8);
    const int Floor = 2;
    const int OptOut = 1;

    public Option<(string Axis, int Rungs)> Degenerate =>
        Age < Floor ? Some((nameof(Age), Age))
        : Cavity < Floor ? Some((nameof(Cavity), Cavity))
        : Curvature < OptOut ? Some((nameof(Curvature), Curvature))
        : Mix < Floor ? Some((nameof(Mix), Mix))
        : Option<(string, int)>.None;

    public long AgeCells => (long)Age * Cavity * Curvature;

    public string Digest => string.Create(CultureInfo.InvariantCulture, $"rungs|{Age}|{Cavity}|{Curvature}|{Mix}");
}

public readonly record struct DisplayEgress(ToneOperator Operator, DisplayEncoding Encoding, double Exposure) {
    public string Digest => string.Create(CultureInfo.InvariantCulture, $"display|{Operator.Key}|{Encoding.Key}|{Exposure:R}");
}

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

    public static Fin<PressPlan> Of(PressPlanDraft draft, PressSubject subject) =>
        Gates(draft, subject)
            .TraverseM(static gate => gate()).As()
            .Map(_ => Order(draft.Bindings))
            .Map(ordered => new PressPlan(draft.Width, draft.Height, draft.Layers, draft.Law, ordered, draft.Backend,
                draft.Seed, draft.Alpha, draft.HeightScaleMm, draft.Rungs, draft.Tile, draft.Material, draft.Conductor,
                Mint(draft, ordered, subject)));

    // --- [PLAN_ADMISSION]
    static Seq<Func<Fin<Unit>>> Gates(PressPlanDraft draft, PressSubject subject) =>
        Seq<Func<Fin<Unit>>>(
            () => guard(!draft.Bindings.IsEmpty, new MaterialFault.Parameter("<press-plan-no-bindings>")),
            () => guard(draft.Law.Admits(draft.Layers.Value), new MaterialFault.Parameter($"<layer-law-rejects:{draft.Law.Key}:{draft.Layers.Value}>")),
            () => guard(draft.Bindings.Map(static b => b.Channel).Distinct().Count() == draft.Bindings.Count, new MaterialFault.Parameter("<press-binding-duplicate-channel>")),
            () => draft.Rungs.Degenerate
                .TraverseM(bad => Fin.Fail<Unit>(new MaterialFault.Parameter($"<ladder-degenerate:{bad.Axis}:{bad.Rungs}>"))).As()
                .Map(_ => unit),
            () => guard(draft.Bindings.ForAll(static b => Depth(b.Channel) >= 0), new MaterialFault.Parameter("<derived-origin-cycle>")),
            () => AdmitSubject(draft, subject),
            () => draft.Tile
                .TraverseM(policy => guard(draft.Bindings.Exists(b => b.Channel == policy.Guide), new MaterialFault.Parameter($"<tile-guide-unbound:{policy.Guide.Key}>")).ToFin()).As()
                .Map(_ => unit),
            () => draft.Bindings.TraverseM(binding => AdmitBinding(draft, binding)).As().Map(static _ => unit),
            () => AdmitLane(draft, subject));

    static Fin<Unit> AdmitLane(PressPlanDraft draft, PressSubject subject) =>
        draft.Backend.ContentAuthoritative
            ? Fin.Succ(unit)
            : !Lowerable(subject)
                ? Fin.Fail<Unit>(new MaterialFault.Parameter($"<gpu-unlowerable-subject:{subject.Case}>"))
                : (long)draft.Width.Value * draft.Height.Value * draft.Layers.Value is var texels && texels > draft.Backend.TexelCeiling
                    ? Fin.Fail<Unit>(new MaterialFault.Parameter($"<gpu-extent-over-floor:{texels}:{draft.Backend.TexelCeiling}>"))
                    : subject is PressSubject.Graph graph
                        ? AdmitChain(draft, graph)
                        : Fin.Succ(unit);

    static Fin<Unit> AdmitSubject(PressPlanDraft draft, PressSubject subject) =>
        subject.Switch(
            state:  draft,
            graph:  static (s, _) => Fin.Succ(unit),
            slab:   static (s, _) => Fin.Succ(unit),
            source: static (s, f) => s.Bindings.Count is 1 && s.Bindings[0].Channel == f.Target
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new MaterialFault.Parameter(s.Key, $"<source-subject-binds-one-channel:{f.Target.Key}>")),
            sky:    static (s, k) => s.Bindings.Count is 1 && s.Bindings[0].Channel == k.Target
                ? guard(s.Law == LayerLaw.CubeFaces,
                    new MaterialFault.Parameter(s.Key, $"<sky-subject-layer-law:{s.Law.Key}>")).ToFin()
                : Fin.Fail<Unit>(new MaterialFault.Parameter(s.Key, $"<sky-subject-binds-one-channel:{k.Target.Key}>")),
            meshSpace: static (s, m) => (long)m.Charts.Length == (long)s.Width.Value * s.Height.Value
                && m.ChartWidth == s.Width
                ? guard(m.Rays > 0 && m.RayDistance > 0.0 && m.GutterRings > 0,
                    new MaterialFault.Parameter(s.Key, $"<mesh-space-cast:{m.Rays}:{m.RayDistance:R}:{m.GutterRings}>")).ToFin()
                : Fin.Fail<Unit>(new MaterialFault.Parameter(s.Key, $"<mesh-space-chart-extent:{m.Charts.Length}>")),
            mix:    static (s, m) => m.Pigments.IsEmpty || m.Pigments.Count != m.WeightFields.Count
                ? Fin.Fail<Unit>(new MaterialFault.Parameter(s.Key, $"<mix-subject-weight-arity:{m.Pigments.Count}!={m.WeightFields.Count}>"))
                : MixLadder.Budget(s.Rungs.Mix, m.Pigments.Count) is var cells && cells <= MixLadder.CellCeiling
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new MaterialFault.Parameter(s.Key, $"<mix-ladder-over-budget:{cells}:{MixLadder.CellCeiling}>")));

    static Fin<Unit> AdmitBinding(PressPlanDraft draft, ChannelBinding binding) =>
        from _ in guard(binding.Format.Components >= binding.Channel.Components, new MaterialFault.Parameter($"<binding-format-narrow:{binding.Channel.Key}>"))
        from __ in guard(binding.Pack.Map(p => p.Slots.Contains(binding.Channel)).IfNone(true), new MaterialFault.Parameter($"<binding-pack-foreign:{binding.Channel.Key}>"))
        from ___ in guard(binding.Pack.IsSome || !draft.Bindings.Exists(other => other.Pack.Map(p => p.Slots.Contains(binding.Channel)).IfNone(false)),
                          new MaterialFault.Parameter($"<binding-both-packed-and-standalone:{binding.Channel.Key}>"))
        from ____ in guard(binding.Display.IsSome
                           || binding.Channel.Transfer.Quantity != PlaneQuantity.Light
                           || binding.Channel.Unit == ChannelUnit.None
                           || !binding.Format.Normalized,
                          new MaterialFault.Parameter($"<binding-open-light-clipped:{binding.Channel.Key}:{binding.Format.Key}>"))
        select unit;

    static bool Lowerable(PressSubject subject) =>
        subject is PressSubject.Sky or PressSubject.Graph
        || (subject is PressSubject.Source source && source.Field switch {
            TextureSource.Noise or TextureSource.Checker or TextureSource.Gradient => true,
            TextureSource.Triplanar { Projected: TextureSource.Noise { Solid: true } } => true,
            _ => false,
        });

    static Fin<Unit> AdmitChain(PressPlanDraft draft, PressSubject.Graph subject) =>
        ChainNodes(subject)
            .Bind(nodes => ChainPlan.Of(nodes))
            .Bind(plan => plan.Admits((long)draft.Width.Value * draft.Height.Value * draft.Layers.Value));

    static Fin<Seq<ChainNode>> ChainNodes(PressSubject.Graph subject) =>
        subject.Program.Compile().Bind(compiled =>
            compiled.Order.TraverseM(node => ChainKernel(node)
                .Map(kernel => new ChainNode(kernel, compiled.Operands(node), ChainWords(node, kernel)))).As());

    static Fin<WgslKernel> ChainKernel(AppearanceNode node) =>
        node switch {
            AppearanceNode.Texture { Source: TextureSource.Noise } => Fin.Succ(WgslKernel.NoiseField),
            AppearanceNode.Texture { Source: TextureSource.Checker } => Fin.Succ(WgslKernel.CheckerField),
            AppearanceNode.Texture { Source: TextureSource.Gradient } => Fin.Succ(WgslKernel.GradientField),
            AppearanceNode.Math => Fin.Succ(WgslKernel.MathFold),
            AppearanceNode.Mix => Fin.Succ(WgslKernel.MixFold),
            _ => new MaterialFault.Parameter($"<graph-node-unlowerable:{node.Kind}>"),
        };

    static ReadOnlyMemory<uint> ChainWords(AppearanceNode node, WgslKernel kernel);

    static Seq<ChannelBinding> Order(Seq<ChannelBinding> bindings) =>
        toSeq(bindings
            .OrderBy(static b => Depth(b.Channel))
            .ThenBy(static b => b.Channel.Pair.IsSome)
            .ThenBy(static b => b.Channel.Ordinal));

    static int Depth(TextureChannel channel) => Depth(channel, walked: 0);
    static int Depth(TextureChannel channel, int walked) =>
        walked > TextureChannel.Items.Count ? -1
        : channel.Origin switch {
            ChannelOrigin.Derived derived when TextureChannel.TryGet(derived.From, out TextureChannel? from) =>
                Depth(from!, walked + 1) is var depth && depth >= 0 ? depth + 1 : -1,
            ChannelOrigin.Derived => 1,
            _ => 0,
        };

    static UInt128 Mint(PressPlanDraft draft, Seq<ChannelBinding> ordered, PressSubject subject) =>
        ContentHash.Of((Draft: draft, Ordered: ordered, Frame: subject.Frame), static (source, digest) => {
            void Piece(string text) => digest.Append(Encoding.UTF8.GetBytes(text));
            Piece(string.Create(CultureInfo.InvariantCulture,
                $"{source.Draft.Width.Value}x{source.Draft.Height.Value}x{source.Draft.Layers.Value}|{source.Draft.Law.Key}|{source.Draft.Backend.Key}|{source.Draft.Seed:x16}|{source.Draft.Alpha.Key}"));
            Piece(string.Create(CultureInfo.InvariantCulture,
                $"hs:{source.Draft.HeightScaleMm.Map(static mm => mm.ToString("R", CultureInfo.InvariantCulture)).IfNone("none")}"));
            Piece(source.Draft.Rungs.Digest);
            foreach (ChannelBinding binding in source.Ordered) {
                Piece(string.Create(CultureInfo.InvariantCulture,
                    $"{binding.Channel.Key}|{binding.Format.Key}|{binding.Policy.Key}|{binding.Pack.Map(static p => p.Key).IfNone(string.Empty)}"));
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
- Entry: `public static Fin<PressProduct> Press(PressSubject subject, PressPlan plan, TimeProvider? clock = null, BakeGovernance governance = default)` is the ONE bake — it compiles the subject once, folds every direct binding, applies each binding's post chain, derives every derived channel from its landed source through the channel's OWN declared fold, composes every requested pack from the landed chains, builds the mip chains against their paired companions, tiles when the plan requests it, and admits the result through `set#TEXTURE_SET` `TextureSet.Of`; the caller composes a `PressProduct` and never orchestrates a stage, and the token cancels between bindings and inside every band onto the kernel cancel channel.
- Packages: `graph#MATERIAL_GRAPH` (composed — `MaterialGraph.Compile` ONCE per press, `CompiledGraph.ShadeSpan` the batched evaluator, `CompiledGraph.ScratchWidth` the per-band scratch the fold rents against, `ShadePoint`, `Context.Canonical`), CommunityToolkit.HighPerformance (`ParallelHelper.For<TAction>(int, int, in TAction)` over a SEEDED `struct IAction` so the partition allocates nothing, inlines, clamps to the processor count, and carries its state — the unseeded overload default-constructs the action and would lose every field the fold needs; `ParallelHelper.ForEach<TItem, TAction>(Memory<T>, in TAction, minimumActionsPerThread)` over a seeded `struct IRefAction<T>` the pack composition's per-level item fold rides, each worker taking its own `ref` job; `MemoryOwner<T>.Allocate` the per-binding staging arena, `SpanOwner<T>.Allocate` the per-band point/scratch/shade rentals, `Memory2D<T>`/`Span2D<T>` the plane views), `Rasm` (project — `Deterministic.Stream`/`NextUnit` the lane-exact coordinate-keyed per-texel draw), `set#TEXTURE_SET` (composed — `SinkSlot.Read` the per-slot `SurfaceShade` column reader, `ChannelOrigin` the per-channel production law), `filter#PLANE_OP` (composed — `PlaneOp.Apply(TexturePlane, Seq<PlaneOp>, TimeProvider?)` and its `PlaneTrace`, for every post chain and every derived channel), `tile#TILE_SYNTH` (composed — `TileSynth.Tileify` when the plan carries a policy), `gpu#PRESS_DEVICE` (composed — `PressDevice.Acquire`/`Dispatch` on the accelerator arm), `weathering#WEATHERING` (composed — `Weathering.Apply` at each cell of the age ladder, taking the cell's own `AgeParameter` and `UnitInterval` cavity scalar), `finish#FINISH` (composed — `FinishMix.Of` and `Finish.Resolve` at each cell of the mix ladder), `surface#OPENPBR_SLAB` (composed — `ToneMap.Apply`/`ToneMap.Encode` the one display egress a binding declares).
- Growth: a new evaluation shape is one `PressSubject` case and one `PressProgram` arm; a new post-processing step is one `filter#PLANE_OP` `PlaneOp` on a binding's chain; a new derived channel is one `ChannelOrigin.Derived` row on `set#TEXTURE_CHANNEL` carrying its own fold — the press discovers both the dependency and the operation from the roster and needs no edit.
- Law: the subject compiles ONCE into a `PressProgram` — a graph resolves its topological order into a frozen `CompiledGraph` and its constant OpenPBR vector, a slab builds its whole age ladder, a mix builds its whole simplex ladder, a field captures its sampler, a dome captures its radiance closure, a mesh-space subject resolves its per-channel measure — and the BAND kernel dispatches on that program once per PARTITION rather than once per texel, so a four-thousand-square plane pays four dispatches per core instead of sixteen million. `ShadeSpan` re-enters over the compiled order with the port environment resolved into an INDEX-ADDRESSED scratch whose slot order is that sort, so a plane costs one allocation-free pass per row instead of one immutable-map rebuild per node per texel; the integrator's per-point `Shade` re-enters this same path over a one-element window, one evaluation law at two grains.
- Law: band parallelism rides `ParallelHelper.For` over a SEEDED `struct IAction` — the seeded overload copies the caller's action into each partition, where the unseeded one default-constructs it and would hand every band an empty program, an empty plan, and a null target. Each band rents its point, scratch, and shade spans ONCE and walks its own rows, so a partition rents once where a per-row action rents per row.
- Law: PER-TEXEL JITTER derives from the TEXEL COORDINATE and the plan seed through `Deterministic.Stream` over the `(x, y, layer, ordinal)` lanes with the FULL 64-bit seed as its own lane, and both axis draws advance that texel-local state through `NextUnit` — never a sequential stream. So a band partition cannot reorder a draw, two channels of one press do not share a jitter sequence, a re-press at one seed is byte-identical at any processor count, and `Position` agrees with the jittered UV so a subject reading both sees one point. The two LINE-INVARIANT reads — the channel ordinal, a frozen-index probe, and the seed cast — hoist out of the texel walk, so what remains inside is arithmetic over a stack-allocated lane span; deriving a texel state by advancing a line state instead would re-transcribe the stream owner's private gamma, which is the deleted form.
- Law: the LAYER axis is the plan's own `LayerLaw` evaluated per line — `cubeFaces` derives the frozen per-face direction into `Position` (the `faceDir` transcription both the CPU bake and the GPU kernel pin to the freeze), `volume` the slab-centre depth coordinate, `frames` the frame coordinate — so the layer rows are real evaluation shapes and a dome reads its direction off the same correspondence its preview kernel does.
- Law: the `Graph` program varies the FIVE sink columns per texel through each slot's own `SinkSlot.Read` projection and reads every other bound channel off its constant lowered vector, because the `graph#MATERIAL_GRAPH` `BsdfOutput` sink carries five columns — so a coat-roughness plane pressed from a graph subject is honestly constant rather than silently mis-projected from base colour.
- Law: the `Slab` program reads a THREE-DIMENSIONAL QUANTIZED AGE LADDER over the `(age, cavity, curvature)` PRODUCT, because the aged vector does not factor through any one scalar: each dose's effective age is its own row's `weathering#WEATHERING` `CavityResponse` scale over the exposure pair, so a dose set mixing a `Crevice` row with an `Exposed` row — or a `Convex` row with a `Concave` one — produces a fold no single parameter indexes, and the fold is order-dependent because each step lerps toward its own terminal. Cavity and curvature are INDEPENDENT axes rather than refinements of one: a crevice can sit on a convex arris, so on a single occlusion scalar `Convex` is byte-identical to `Exposed` and the pair only becomes distinguishable when the second field exists. `Weathering.Apply` runs once per CELL across the plan's own `LadderRungs.AgeCells` quantization and every texel indexes the cell its three fields sampled.
- Law: the THIRD LADDER AXIS IS PRICED. A curvature dimension MULTIPLIES the cell product, so the default row buys it at the cheapest count that still interpolates — `16 × 8 × 4` is 512 fallible admissions per press against sixteen million texels at four thousand square, four times the two-axis cost and still three orders of margin. `Curvature: 1` is the declared OPT-OUT, exempt from the two-rung floor precisely because one column means the axis is not sampled and the ladder is the two-dimensional one it was. The per-texel `Apply` escape — the exact triple at every texel — stays the GROWTH LEG rather than the default, because it is a fallible admission per texel, which is the cost the ladder exists to refuse.
- Law: the `Mix` program reads the SAME shape over the barycentric weight simplex: `Finish.Resolve` runs once per grid point of the compositions of `LadderRungs.Mix − 1` into the pigment count, each texel's sampled weight vector normalizes and quantizes by largest remainder onto that grid, and the cell budget gates at admission because the count grows COMBINATORIALLY in the pigment count where the aging product grows as a grid. Every rung count is a declared plan column entering the plan key rather than a hidden approximation — a caller needing a continuous trajectory presses at a finer ladder, which is one column, not a different fold.
- Law: ABSENT SPATIAL FIELDS DEGRADE TO THE AGING A CALLER WHO NEVER HEARD OF THE AXIS EXPECTS, and each extreme is chosen for that and nothing else: no age field bakes UNAGED at `0.0` (the material as authored, where the silent fully-weathered default was an inversion a caller found only in the render), no cavity field reads `1.0` (the full-cavity extreme at which every `Crevice` row ages at the raw age and the axis collapses to its uniform column, where `0.0` would delete every crevice effect and run every exposed one at full age), and no curvature field reads `0.0` (the flat extreme at which every curvature-keyed row scales at unity).
- Law: the `Sky` subject is a bake like any other. Its radiance closure evaluates at each texel's own world direction and the dome inherits the press engine's partitioning, cancellation, run record, and accelerator lane instead of carrying a sweep of its own; the DIFFUSE field alone crosses, because the solar disc rides the light row's own sampling arm and folding disc radiance into the dome double-counts the sun the moment the light samples it. It is the one subject beyond `Source` that LOWERS: `equirectToCube` carries the face correspondence whole, so a preview dome images the field the CPU mint will produce, and the CPU mint stays authoritative under the same veto.
- Law: the `MeshSpace` subject makes occlusion, thickness, and curvature MEASURED. Charts cross as DATA — the kernel's already-flattened product lowered to per-texel surface evidence — so no host mesh type enters and no tessellator runs here; a GUTTER texel writes its channel neutral and is traced from nothing, because a ray cast from a point not on the body measures absence rather than occlusion, and the subject's own `Dilate` rings then fill those texels from their own chart so the gutter never bleeds a neighbour's relief through the mip chain. It does NOT lower, and that refusal is structural: an f32 rebuild of an arbitrary chart cast forks the very key the content-identity veto holds. The `filter#PLANE_OP` height-field derivations stay the FALLBACK origin rather than being deleted — a slab subject has no body to trace against, and its approximations remain the honest answer for the subject that has no geometry.
- Law: STAGING IS `ShadeVec4` AND QUANTIZATION IS THE PLANE'S. The fold writes decoded four-lane texels into a `Memory2D<ShadeVec4>` arena and the plane's own row `Write` accessor associates alpha, encodes the transfer, and narrows to the binding's `PlaneFormat`, so exactly one quantizer exists in the corpus and a press never encodes a texel itself. A binding declaring a `DisplayEgress` grades its row BEFORE that crossing — `ToneMap.Apply` per texel, then `ToneMap.Encode` over the whole row — so scene-referred radiance reaches an integer lane through the one tone-map owner rather than through the transfer clip a raw narrow performs; a binding declaring none takes its own walk and rents nothing, because a zero-length rental exists only to be skipped. Both lanes cross the same `Fill` path, so the preview inherits the grade and the two differ in authority alone.
- Law: DERIVED CHANNELS fold AFTER their sources land, through the channel row's OWN `ChannelOrigin.Derived.Fold` step composed BEFORE the binding's post chain, so the derivation is roster data rather than a caller-supplied chain the press hopes contains the right operation; a source channel a plan did not request is produced as an intermediate and dropped unless bound. A PAIRED mip policy resolves its companion from the landed map and DOWNGRADES to `MipPolicy.Box` where the plan bound none, the run recording the channel by name — refusing the whole press for a quality floor the corpus already declares acceptable is the worse trade.
- Law: TILING IS IN-FOLD when the plan carries a policy, so every channel takes ONE plan and the resulting set re-keys; the set's `Tiled` proof stays the gate's own mint rather than the plan's request. A REQUESTED PACK builds AFTER the channels land — each lane composes PER LEVEL from its slot channel's own folded chain, an absent slot carries its neutral at every level, packed channels leave the standalone roster, and depth-divergent slots refuse by name; the compose pipeline carries its own `Rollback`, because a level that fails to rent mid-chain otherwise strands every level already minted in a carrier no caller receives.
- Law: THE ACCELERATOR ARM dispatches per binding against a device acquired once and released at the fold's close, runs no post chain and no derivation, and its product is a `Preview` — no set, no key, nothing addressable — so the content-identity law needs no runtime check downstream. `Lower` carries the PROVEN field forward beside its kernel row so `Stage` reads a `TextureSource` rather than re-testing a union the plan admission already closed; the uniform block writes through the one `KernelUniform` word writer in the row's declared order, colour columns crossing as `Vec4` appends because a scalar pair previews every authored ramp as grey. `Lift` widens the readback back into staging and crosses through the same `Fill` path; a short readback FAILS rather than zero-filling its tail, because a truncated dispatch reading as a black band is the failure a preview is least likely to be checked for, and a `Coupled` policy downgrades to the box floor since the preview lane lands no companion to read a variance from.
- Law: FAILURE IS COUNTED EVIDENCE. A band-kernel failure fills the channel neutral AND tallies into `PressRun.Faulted` through one interlocked cell, so a plane whose evaluation died and dressed as neutral is separable on the run from a genuinely-neutral one. The `Aged` arm alone fills `PressRun.Aging`: one interlocked cell folds the visited rungs of every ladder axis as bitsets beside the sampled age extrema, so an over-quantized ladder and an unexercised axis both read off the run instead of off a second press.
- Boundary: GOVERNANCE rides the `filter#PLANE_OP` `BakeGovernance` carrier and never a token tail beside a sink tail — one value through the fold statics, `Opened` publishing-and-checking in one call, default-inert so an unwatched press pays one struct copy. The PROGRESS UNIT is the BINDING, the only boundary whose count the plan declares and whose cost is comparable across a press, so both fold passes and the preview lane walk one running ordinal over `plan.Bindings.Count` and the two backends stay comparable on the one surface a caller sees mid-run. CANCELLATION rides the kernel channel underneath: every band polls the token PER LINE, the fold checks between bindings, and a cancelled press fails `Errors.Cancelled` after DISPOSING every landed plane, the same discipline every failure arm holds through `Released`. Wall time rides the injected `TimeProvider` and every per-channel `PlaneTrace` rides the same clock. The `[EXPRESSION_SPINE]` exemptions are NAMED PER FAMILY and nothing else on the page carries one: the `PressRows` band arms (each row writes a `Span2D` line a closure cannot cross and polls the cancellation token per line), the two ladder builds with the simplex quantize-and-rank kernel (a composition rank is a positional walk over a caller-owned count run), the pack `Compose` lane seat, and the `Fill` staging write — every one a fixed-extent index fold over a caller-owned buffer where a `Seq` operator would allocate per texel. A loop with an operator IS folded rather than exempted: the parity `Divergence` row comparison is one `TensorPrimitives.Subtract` and one `MaxMagnitude` over the row, its two surviving walks being the plane's own layer and row decode addressing. Every admission, dispatch, and egress surface is expression-bodied except the failure-disposal blocks, which are resource boundaries.

```csharp

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressProgram {
    private PressProgram() { }
    public sealed record Shaded(CompiledGraph Graph, MaterialParameters Row, OpenPbrSurface Constant) : PressProgram;
    public sealed record Field(TextureSource Source, SamplerState Sampler, TextureChannel Target) : PressProgram;
    public sealed record Aged(AgeLadder Ladder, Option<TextureSource> AgeField, Option<TextureSource> CavityField,
        Option<TextureSource> CurvatureField, SamplerState Sampler, LadderCoverage Coverage) : PressProgram;
    public sealed record Mixed(MixLadder Ladder, Seq<TextureSource> WeightFields, SamplerState Sampler) : PressProgram;
    public sealed record Dome(Func<Vector3d, RgbSpectrum> Radiance, TextureChannel Target) : PressProgram;
    public sealed record Surface(
        ReadOnlyMemory<ChartTexel> Charts, MaterialParameters Row, OpenPbrSurface Constant,
        Func<ChartTexel, TextureChannel, ShadeVec4> Measure, int GutterRings) : PressProgram;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AgeLadder(Seq<OpenPbrSurface> Cells, int AgeRungs, int CavityRungs, int CurvatureRungs) {
    public static Fin<AgeLadder> Of(
        MaterialParameters row, ConductorMetal conductor, Seq<WeatheringDose> aging, LadderRungs rungs) =>
        toSeq(Enumerable.Range(0, checked((int)rungs.AgeCells)))
            .TraverseM(cell => Weathering.Apply(row, aging,
                        AgeParameter.Create((cell % rungs.Age) / (double)(rungs.Age - 1)),
                        new SurfaceExposure(
                            UnitInterval.Create(((cell / rungs.Age) % rungs.Cavity) / (double)(rungs.Cavity - 1)),
                            rungs.Curvature is 1 ? 0.0 : (((cell / rungs.Age) / rungs.Cavity) / (double)(rungs.Curvature - 1) * 2.0) - 1.0))
                    .Map(aged => OpenPbrSurface.Of(aged, conductor))).As()
            .Map(cells => new AgeLadder(cells, rungs.Age, rungs.Cavity, rungs.Curvature));

    public OpenPbrSurface At(double age, double cavity, double curvature) =>
        Cells[(((Rung(Signed(curvature), CurvatureRungs) * CavityRungs) + Rung(cavity, CavityRungs)) * AgeRungs) + Rung(age, AgeRungs)];

    internal static double Signed(double curvature) => (Math.Clamp(curvature, -1.0, 1.0) + 1.0) * 0.5;

    public static int Rung(double t, int rungs) => Math.Clamp((int)(t * (rungs - 1) + 0.5), 0, rungs - 1);
}

public sealed record MixLadder(Seq<OpenPbrSurface> Cells, int Pigments, int Rungs) {
    internal const int CellCeiling = 4096;

    static int Compositions(int units, int parts) {
        long count = 1L;
        for (int i = 1; i < parts; i++) { count = count * (units + i) / i; }
        return (int)count;
    }

    internal static int Budget(int rungs, int pigments) {
        long count = 1L;
        for (int i = 1; i < pigments; i++) {
            count = count * (rungs - 1 + i) / i;
            if (count > CellCeiling) { return int.MaxValue; }
        }
        return (int)count;
    }

    public static Fin<MixLadder> Of(PressSubject.Mix mix, LadderRungs rungs) {
        int parts = mix.Pigments.Count, units = rungs.Mix - 1;
        int[] counts = new int[parts];
        counts[parts - 1] = units;
        Fin<Seq<OpenPbrSurface>> built = Fin.Succ(Seq<OpenPbrSurface>());
        do {
            built = built.Bind(cells => Resolve(mix, counts).Map(cells.Add));
        } while (built.IsSucc && Advance(counts, units));
        return built.Map(cells => new MixLadder(cells, parts, rungs.Mix));
    }

    static Fin<OpenPbrSurface> Resolve(PressSubject.Mix mix, int[] counts) =>
        FinishMix.Of(mix.Pigments, toSeq(counts).Map(static c => (double)c))
            .Bind(admitted => Finish.Resolve(mix.Kind, admitted, mix.Stack, mix.Substrate))
            .Map(resolved => OpenPbrSurface.Of(resolved.Row, mix.Conductor));

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

    public OpenPbrSurface At(ReadOnlySpan<double> weights, Span<int> counts) {
        Quantize(weights, counts, Rungs - 1);
        return Cells[Rank(counts, Rungs - 1)];
    }

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

    static int Rank(ReadOnlySpan<int> counts, int units) {
        int rank = 0, remaining = units;
        for (int axis = 0; axis < counts.Length - 1; axis++) {
            for (int step = 0; step < counts[axis]; step++) { rank += Compositions(remaining - step, counts.Length - axis - 1); }
            remaining -= counts[axis];
        }
        return rank;
    }
}

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TexturePress {
    const int BandFloor = 16;

    public static Fin<PressProduct> Press(PressSubject subject, PressPlan plan, TimeProvider? clock = null, BakeGovernance governance = default) {
        TimeProvider ticks = clock ?? TimeProvider.System;
        long opened = ticks.GetTimestamp();
        return plan.Backend.ContentAuthoritative
            ? Mint(subject, plan, ticks, opened, governance)
            : Accelerate(subject, plan, ticks, opened, governance);
    }

    static Fin<T> Staged<T>(BakeGovernance governance, int done, int total, HashMap<TextureChannel, TexturePyramid> landed, Func<Fin<T>> body) =>
        governance.Opened(total <= 0 ? 1.0 : done / (double)total).Map(_ => Surrender<T>(landed)).IfNone(body);

    static Fin<PressProduct> Mint(PressSubject subject, PressPlan plan, TimeProvider ticks, long opened, BakeGovernance governance) =>
        from program in Compile(subject, plan)
        from folded in plan.Bindings.Filter(b => Direct(program, b))
            .FoldM((Planes: HashMap<TextureChannel, TexturePyramid>(), Evidence: HashMap<TextureChannel, PlaneTrace>(), Downgraded: Seq<TextureChannel>(), Faulted: HashMap<TextureChannel, ulong>(), Done: 0), (carried, binding) =>
                Staged(governance, carried.Done, plan.Bindings.Count, carried.Planes, () =>
                    Land(program, plan, binding, (carried.Planes, carried.Evidence, carried.Downgraded, carried.Faulted), ticks, governance.Cancel)
                        .Map(next => (next.Planes, next.Evidence, next.Downgraded, next.Faulted, Done: carried.Done + 1))
                        .Rollback([.. carried.Planes.Values])))
            .As()
        from derived in plan.Bindings.Filter(b => !Direct(program, b))
            .FoldM(folded, (carried, binding) =>
                Staged(governance, carried.Done, plan.Bindings.Count, carried.Planes, () =>
                    Derive(program, plan, binding, (carried.Planes, carried.Evidence, carried.Downgraded, carried.Faulted), ticks, governance.Cancel)
                        .Map(next => (next.Planes, next.Evidence, next.Downgraded, next.Faulted, Done: carried.Done + 1))
                        .Rollback([.. carried.Planes.Values])))
            .As()
        from packed in Packed(plan, derived.Planes, governance.Cancel).Rollback([.. derived.Planes.Values])
        from set in TextureSet.Of(new TextureSetDraft(plan.Width, plan.Height, plan.Layers, plan.Law,
            NormalConvention.Gl, plan.Alpha, plan.HeightScaleMm, new Evidence<TileProof>.Absent(), Seq<UdimTile>(),
            packed.Channels.Filter((c, _) => plan.Bindings.Exists(b => b.Channel == c && b.Pack.IsNone)), packed.Packs,
            plan.Conductor, plan.Material)).Rollback([.. derived.Planes.Values])
        from tiled in plan.Tile
            .TraverseM(policy => TileSynth.Tileify(set, policy, ticks).Map(static pair => pair.Set)).As()
            .Map(tiled => tiled.IfNone(set))
        select (PressProduct)new PressProduct.Minted(tiled, new PressRun(plan.Backend, plan.PlanKey,
            GraphKey(program), plan.Seed, Texels(tiled), ticks.GetElapsedTime(opened).TotalMilliseconds,
            derived.Evidence, derived.Downgraded, derived.Faulted, GpuDeltaMax: Option<double>.None,
            Aging: Coverage(program)));

    static Option<AgeCoverage> Coverage(PressProgram program) =>
        program is PressProgram.Aged aged ? aged.Coverage.Read() : Option<AgeCoverage>.None;

    static bool Direct(PressProgram program, ChannelBinding binding) =>
        binding.Channel.Origin is not ChannelOrigin.Derived
            || (program is PressProgram.Field field && field.Target == binding.Channel);

    static Fin<(HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneTrace> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted)> Land(
        PressProgram program, PressPlan plan, ChannelBinding binding,
        (HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneTrace> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted) carried, TimeProvider ticks, CancellationToken cancel) =>
        Fold(program, plan, binding, cancel).Bind(fold =>
            Custody.Bracket(
                () => cancel.IsCancellationRequested
                    ? Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, HashMap<TextureChannel, PlaneTrace>, Seq<TextureChannel>, HashMap<TextureChannel, ulong>)>(Errors.Cancelled)
                    : Finish(plan, binding, fold.Arena.Memory.AsMemory2D(plan.Height.Value * plan.Layers.Value, plan.Width.Value), carried.Planes, ticks)
                        .Map(built => (
                            carried.Planes.Add(binding.Channel, built.Pyramid),
                            carried.Evidence.Add(binding.Channel, built.Trace),
                            built.Downgraded ? carried.Downgraded.Add(binding.Channel) : carried.Downgraded,
                            fold.Faulted > 0 ? carried.Faulted.Add(binding.Channel, fold.Faulted) : carried.Faulted)),
                fold.Arena));

    static Fin<T> Surrender<T>(HashMap<TextureChannel, TexturePyramid> landed) =>
        Fin.Fail<T>(Errors.Cancelled).Rollback([.. landed.Values]);

    static Fin<(HashMap<TextureChannel, TexturePyramid> Channels, Seq<ChannelPackPlane> Packs)> Packed(
        PressPlan plan, HashMap<TextureChannel, TexturePyramid> landed, CancellationToken cancel) =>
        cancel.IsCancellationRequested
            ? Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, Seq<ChannelPackPlane>)>(Errors.Cancelled)
            : toSeq(plan.Bindings.Choose(static b => b.Pack).Distinct())
            .FoldM((Channels: landed, Packs: Seq<ChannelPackPlane>()), (carried, pack) => {
                    Seq<TextureChannel> present = pack.Slots.Filter(carried.Channels.ContainsKey);
                    Seq<int> depths = toSeq(present.Choose(slot => carried.Channels.Find(slot)).Map(static c => c.Levels.Count).Distinct());
                    ChannelDtype depth = plan.Bindings.Filter(b => b.Pack == Some(pack)).Map(static b => b.Format.Depth).Head.IfNone(ChannelDtype.Unorm8);
                    return present.IsEmpty
                        ? Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, Seq<ChannelPackPlane>)>(new MaterialFault.Parameter($"<pack-no-landed-slot:{pack.Key}>"))
                        : depths.Count > 1
                            ? Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, Seq<ChannelPackPlane>)>(new MaterialFault.Parameter($"<pack-slot-mip-divergent:{pack.Key}>"))
                            : from format in PlaneFormat.For(4, depth).ToFin(new MaterialFault.Parameter($"<pack-format-unresolved:{pack.Key}:{depth.Key}>"))
                              from levels in Compose(pack, carried.Channels, depths.Head.IfNone(1), format, cancel)
                              select (
                                  carried.Channels.Filter((c, _) => !pack.Slots.Contains(c)),
                                  carried.Packs.Add(new ChannelPackPlane(pack, new TexturePyramid(levels, MipPolicy.Box, Coupled: false), present)));
                }).As();

    static Fin<Seq<TexturePlane>> Compose(
        ChannelPack pack, HashMap<TextureChannel, TexturePyramid> landed, int depth, PlaneFormat format, CancellationToken cancel) {
        Option<TexturePyramid> reference = pack.Slots.Choose(slot => landed.Find(slot)).Head;
        return reference.ToFin(new MaterialFault.Parameter($"<pack-no-landed-slot:{pack.Key}>")).Bind(head =>
            toSeq(Enumerable.Range(0, depth)).FoldM(Seq<PackLevelJob>(), (jobs, levelIndex) => {
                    TexturePlane extent = head.Levels[levelIndex];
                    return TexturePlane.Of(format, extent.Width, extent.Height, PlaneTransfer.Raw, AlphaMode.None, Some(extent.Layers))
                        .Map(target => jobs.Add(new PackLevelJob(
                            target,
                            [.. pack.Slots.Map(slot => landed.Find(slot).Case is TexturePyramid chain ? chain.Levels[levelIndex] : null)],
                            [.. pack.Slots.Map(static slot => slot.Neutral.X)])))
                        .Rollback([.. jobs.Map(static job => job.Target)]);
                }).As())
            .Bind(jobs => Try.lift(() => {
                    PackLevelJob[] roster = [.. jobs];
                    PackCompose fold = new(cancel);
                    ParallelHelper.ForEach<PackLevelJob, PackCompose>(roster.AsMemory(), in fold, minimumActionsPerThread: 1);
                    return cancel.IsCancellationRequested
                        ? Fin.Fail<Seq<TexturePlane>>(Errors.Cancelled)
                        : Fin.Succ(toSeq(roster).Map(static job => job.Target));
                }).Run().Bind(static inner => inner)
                .Rollback([.. jobs.Map(static job => job.Target)]));
    }

    readonly record struct PackLevelJob(TexturePlane Target, TexturePlane?[] Slots, double[] Neutrals);

    readonly struct PackCompose(CancellationToken cancel) : IRefAction<PackLevelJob> {
        public void Invoke(ref PackLevelJob job) {
            using SpanOwner<ShadeVec4> lane = SpanOwner<ShadeVec4>.Allocate(job.Target.Width.Value);
            using SpanOwner<ShadeVec4> texels = SpanOwner<ShadeVec4>.Allocate(job.Target.Width.Value);
            for (int layer = 0; layer < job.Target.Layers.Value && !cancel.IsCancellationRequested; layer++) {
                for (int row = 0; row < job.Target.Height.Value && !cancel.IsCancellationRequested; row++) {
                    for (int x = 0; x < texels.Span.Length; x++) { texels.Span[x] = new ShadeVec4(0.0, 0.0, 0.0, 1.0); }
                    for (int slotIndex = 0; slotIndex < job.Slots.Length; slotIndex++) {
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

    static Fin<PressProduct> Accelerate(PressSubject subject, PressPlan plan, TimeProvider ticks, long opened, BakeGovernance governance) =>
        from lease in PressDevice.Acquire(DevicePolicy.Default)
        from planes in lease.Use((Subject: subject, Plan: plan, Governance: governance), static (state, device) =>
            state.Plan.Bindings.FoldM((Rows: HashMap<TextureChannel, TexturePyramid>(), Done: 0), (carried, binding) =>
                Staged(state.Governance, carried.Done, state.Plan.Bindings.Count, carried.Rows, () =>
                    Lower(state.Subject, binding, state.Key)
                        .Bind(lowered => device.Dispatch(lowered.Kernel, Stage(state.Plan, lowered.Kernel, lowered.Field), state.Key))
                        .Bind(readback => Lift(state.Plan, binding, readback, state.Key))
                        .Map(plane => (Rows: carried.Rows.Add(binding.Channel, plane), Done: carried.Done + 1))
                        .Rollback([.. carried.Rows.Values])))
            .As()
            .Map(static carried => carried.Rows))
        select (PressProduct)new PressProduct.Preview(planes, new PressRun(plan.Backend, plan.PlanKey,
            Option<UInt128>.None, plan.Seed, Texels(plan, planes), ticks.GetElapsedTime(opened).TotalMilliseconds,
            HashMap<TextureChannel, PlaneTrace>(), Seq<TextureChannel>(), HashMap<TextureChannel, ulong>(),
            GpuDeltaMax: Option<double>.None, Aging: Option<AgeCoverage>.None));

    static Fin<PressProgram> Compile(PressSubject subject, PressPlan plan) =>
        subject.Switch(
            state:  plan,
            graph:  static (s, g) => g.Program.Compile(s.Key).Map(compiled => (PressProgram)new PressProgram.Shaded(compiled, g.Row, OpenPbrSurface.Of(g.Row, g.Conductor))),
            source: static (_, f) => Fin.Succ<PressProgram>(new PressProgram.Field(f.Field, f.Sampler, f.Target)),
            slab:   static (s, b) => AgeLadder.Of(b.Row, b.Conductor, b.Aging, s.Rungs, s.Key)
                .Map(ladder => (PressProgram)new PressProgram.Aged(
                    ladder, b.AgeField, b.CavityField, b.CurvatureField, b.Sampler, new LadderCoverage(s.Rungs).Seeded())),
            mix:    static (s, m) => MixLadder.Of(m, s.Rungs, s.Key)
                .Map(ladder => (PressProgram)new PressProgram.Mixed(ladder, m.WeightFields, m.Sampler)),
            sky:    static (_, k) => Fin.Succ<PressProgram>(new PressProgram.Dome(k.Radiance, k.Target)),
            meshSpace: static (s, m) => GeometricMeasure(m, s.Bindings, s.Key)
                .Map(measure => (PressProgram)new PressProgram.Surface(
                    m.Charts, m.Row, OpenPbrSurface.Of(m.Row, m.Conductor), measure, m.GutterRings)));

    static Fin<Func<ChartTexel, TextureChannel, ShadeVec4>> GeometricMeasure(
        PressSubject.MeshSpace subject, Seq<ChannelBinding> bindings) =>
        bindings.Exists(static b => b.Channel.Origin is ChannelOrigin.Geometric or ChannelOrigin.Derived)
            ? Fin.Succ<Func<ChartTexel, TextureChannel, ShadeVec4>>((texel, channel) => Measured(subject, texel, channel))
            : Fin.Succ<Func<ChartTexel, TextureChannel, ShadeVec4>>(static (_, channel) => channel.Neutral);

    static ShadeVec4 Measured(PressSubject.MeshSpace subject, ChartTexel texel, TextureChannel channel) =>
        channel == TextureChannel.Occlusion ? Scalar(Hemisphere(subject, texel, inward: false))
        : channel == TextureChannel.Curvature ? Scalar(Bend(subject, texel))
        : channel == TextureChannel.Height ? Scalar(Hemisphere(subject, texel, inward: true))
        : channel.Neutral;

    static ShadeVec4 Scalar(double value) => new(value, 0.0, 0.0, 1.0);

    static Fin<(MemoryOwner<ShadeVec4> Arena, ulong Faulted)> Fold(PressProgram program, PressPlan plan, ChannelBinding binding, CancellationToken cancel) =>
        Try.lift(() => ShadePoint.Of(Point3d.Origin, Vector3d.ZAxis, Vector3d.ZAxis, Some(Vector3d.XAxis), 0.0, 0.0, Context.Canonical)
            .Bind(anchor => {
                int rows = plan.Height.Value * plan.Layers.Value;
                MemoryOwner<ShadeVec4> arena = MemoryOwner<ShadeVec4>.Allocate(plan.Width.Value * rows, AllocationMode.Default);
                ulong[] faulted = new ulong[1];
                int band = Math.Max(BandFloor, rows / (Environment.ProcessorCount * 4));
                return Try.lift(() => {
                    ParallelHelper.For(0, (rows + band - 1) / band,
                        in new PressRows(program, plan, binding.Channel, anchor, arena.Memory.AsMemory2D(rows, plan.Width.Value), band, rows, faulted, cancel));
                    return Fin.Succ((arena, faulted[0]));
                }).Run().Bind(static inner => inner).Rollback(arena);
            })).Run().Bind(static inner => inner);

    static Fin<(TexturePyramid Pyramid, PlaneTrace Trace, bool Downgraded)> Finish(
        PressPlan plan, ChannelBinding binding, Memory2D<ShadeVec4> staging, HashMap<TextureChannel, TexturePyramid> landed, TimeProvider ticks) =>
        from blank in TexturePlane.Of(binding.Format, plan.Width, plan.Height, binding.Channel.Transfer,
            binding.Format.Alpha.Carries ? plan.Alpha : AlphaMode.None, Some(plan.Layers))
        let filled = Fill(blank, staging, binding.Display)
        from posted in PlaneOp.Apply(filled, binding.Post, ticks)
        let paired = Companion(binding.Channel, landed)
        let policy = binding.Policy.Coupled && paired.IsNone ? MipPolicy.Box : binding.Policy
        from chain in TexturePyramid.Of(posted.Plane, policy, paired)
        select (chain, posted.Trace, policy != binding.Policy);

    static Option<TexturePyramid> Companion(TextureChannel channel, HashMap<TextureChannel, TexturePyramid> landed) =>
        channel.Pair.Bind(name => TextureChannel.TryGet(name, out TextureChannel? row) ? landed.Find(row) : Option<TexturePyramid>.None);

    static TexturePlane Fill(TexturePlane plane, Memory2D<ShadeVec4> texels, Option<DisplayEgress> display) {
        ReadOnlySpan2D<ShadeVec4> source = texels.Span;
        if (display.Case is not DisplayEgress egress) {
            for (int layer = 0; layer < plane.Layers.Value; layer++) {
                for (int row = 0; row < plane.Height.Value; row++) {
                    plane.WriteShade(row, layer, source.GetRowSpan((layer * plane.Height.Value) + row));
                }
            }
            return plane;
        }
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
                for (int x = 0; x < line.Length; x++) { encoded.Span[x] = encoded.Span[x] with { W = line[x].W }; }
                plane.WriteShade(row, layer, encoded.Span);
            }
        }
        return plane;
    }

    static double Lane(double value) => double.IsFinite(value) ? Math.Max(0.0, value) : 0.0;

    static Fin<(HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneTrace> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted)> Derive(
        PressProgram program, PressPlan plan, ChannelBinding binding,
        (HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneTrace> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted) carried, TimeProvider ticks, CancellationToken cancel) =>
        binding.Channel.Origin is ChannelOrigin.Derived derived && TextureChannel.TryGet(derived.From, out TextureChannel? from)
            ? from sourced in Ensure(program, plan, from!, carried, ticks, cancel)
              from source in sourced.Planes.Find(from!).ToFin(new MaterialFault.Parameter($"<derived-source-absent:{binding.Channel.Key}:{derived.From}>"))
              from folded in PlaneOp.Apply(source.Base, derived.Fold.Cons(binding.Post), ticks)
              let paired = Companion(binding.Channel, sourced.Planes)
              let policy = binding.Policy.Coupled && paired.IsNone ? MipPolicy.Box : binding.Policy
              from chain in TexturePyramid.Of(folded.Plane, policy, paired)
              select (sourced.Planes.Add(binding.Channel, chain),
                      sourced.Evidence.Add(binding.Channel, folded.Trace),
                      policy != binding.Policy ? sourced.Downgraded.Add(binding.Channel) : sourced.Downgraded,
                      sourced.Faulted)
            : Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, HashMap<TextureChannel, PlaneTrace>, Seq<TextureChannel>, HashMap<TextureChannel, ulong>)>(
                new MaterialFault.Parameter($"<derived-origin-unresolved:{binding.Channel.Key}>"));

    static Fin<(HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneTrace> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted)> Ensure(
        PressProgram program, PressPlan plan, TextureChannel channel,
        (HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneTrace> Evidence, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted) carried, TimeProvider ticks, CancellationToken cancel) =>
        carried.Planes.ContainsKey(channel)
            ? Fin.Succ(carried)
            : from format in PlaneFormat.For(channel.Components, ChannelDtype.Float32)
                  .ToFin(new MaterialFault.Parameter($"<implicit-format-absent:{channel.Key}:{channel.Components}>"))
              let implicitBinding = new ChannelBinding(channel, format, Some(MipPolicy.None),
                  Option<ChannelPack>.None, Seq<PlaneOp>(), Option<DisplayEgress>.None)
              from landed in channel.Origin is ChannelOrigin.Derived
                  ? Derive(program, plan, implicitBinding, carried, ticks, cancel)
                  : Land(program, plan, implicitBinding, carried, ticks, cancel)
              select landed;

    static Fin<(WgslKernel Kernel, TextureSource Field)> Lower(PressSubject subject, ChannelBinding binding) =>
        subject is PressSubject.Source source
            ? source.Field switch {
                TextureSource.Noise noise       => Fin.Succ((WgslKernel.NoiseField, (TextureSource)noise)),
                TextureSource.Checker checker   => Fin.Succ((WgslKernel.CheckerField, (TextureSource)checker)),
                TextureSource.Gradient gradient => Fin.Succ((WgslKernel.GradientField, (TextureSource)gradient)),
                TextureSource.Triplanar { Projected: TextureSource.Noise { Solid: true } } triplanar =>
                    Fin.Succ((WgslKernel.NoiseField, (TextureSource)triplanar)),
                _ => Fin.Fail<(WgslKernel, TextureSource)>(new RasterFault.Device($"<gpu-unlowerable-source:{binding.Channel.Key}>")),
            }
            : Fin.Fail<(WgslKernel, TextureSource)>(new RasterFault.Device($"<gpu-unlowerable-subject:{binding.Channel.Key}>"));

    static KernelBinding Stage(PressPlan plan, WgslKernel kernel, TextureSource field) =>
        Seat(plan, kernel, Words(plan, field), Reads(field), kernel.Groups(plan.Width, plan.Height, plan.Layers));

    static KernelBinding Seat(PressPlan plan, WgslKernel kernel, KernelUniform words, Seq<ReadOnlyMemory<float>> reads, (uint X, uint Y, uint Z) groups) =>
        new(reads.Fold(Seq(words.Block), static (buffers, plane) => buffers.Add(new KernelBuffer.Read(plane)))
                .Add(new KernelBuffer.Write(kernel.WriteElements(plan.Width, plan.Height, plan.Layers))),
            groups.X, groups.Y, groups.Z);

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

    static Fin<TexturePyramid> Lift(PressPlan plan, ChannelBinding binding, KernelReadback readback) {
        int rows = plan.Height.Value * plan.Layers.Value;
        long texels = (long)plan.Width.Value * rows;
        if (readback.Output.Length < texels * 4) {
            return Fin.Fail<TexturePyramid>(new RasterFault.Device($"<gpu-readback-short:{binding.Channel.Key}:{readback.Output.Length}<{texels * 4}>"));
        }
        using MemoryOwner<ShadeVec4> staging = MemoryOwner<ShadeVec4>.Allocate(checked((int)texels), AllocationMode.Default);
        ReadOnlySpan<float> lanes = readback.Output.Span;
        Span<ShadeVec4> decoded = staging.Span;
        for (int texel = 0; texel < decoded.Length; texel++) {
            int at = texel * 4;
            decoded[texel] = new ShadeVec4(lanes[at], lanes[at + 1], lanes[at + 2], lanes[at + 3]);
        }
        return TexturePlane.Of(binding.Format, plan.Width, plan.Height, binding.Channel.Transfer,
                binding.Format.Alpha.Carries ? plan.Alpha : AlphaMode.None, Some(plan.Layers))
            .Map(blank => Fill(blank, staging.Memory.AsMemory2D(rows, plan.Width.Value), binding.Display))
            .Bind(filled => TexturePyramid.Of(filled, binding.Policy.Coupled ? MipPolicy.Box : binding.Policy));
    }

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

    static ulong Texels(TextureSet set) =>
        toSeq(set.Channels.AsIterable()).Fold(0UL, static (acc, pair) => acc + Levels(pair.Value)) + set.Packs.Fold(0UL, static (acc, pack) => acc + Levels(pack.Plane));

    static ulong Texels(PressPlan plan, HashMap<TextureChannel, TexturePyramid> planes) =>
        toSeq(planes.AsIterable()).Fold(0UL, static (acc, pair) => acc + Levels(pair.Value));

    static ulong Levels(TexturePyramid pyramid) =>
        pyramid.Levels.Fold(0UL, static (acc, level) => acc + ((ulong)level.Width.Value * (ulong)level.Height.Value * (ulong)level.Layers.Value));
}

internal readonly struct PressRows(
    PressProgram program, PressPlan plan, TextureChannel channel, ShadePoint anchor,
    Memory2D<ShadeVec4> target, int band, int rows, ulong[] faulted, CancellationToken cancel) : IAction {
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
            default: NeutralBand(start, end, write.Span); break;
        }
    }

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

    void ShadedBand(PressProgram.Shaded shaded, int start, int end, Span<ShadePoint> points, Span<ShadeVec4> write) {
        using SpanOwner<PortValue> scratch = SpanOwner<PortValue>.Allocate(shaded.Graph.ScratchWidth);
        using SpanOwner<SurfaceShade> shades = SpanOwner<SurfaceShade>.Allocate(points.Length);
        Span2D<ShadeVec4> plane = target.Span;
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

    void AgedBand(PressProgram.Aged aged, int start, int end, Span<ShadePoint> points, Span<ShadeVec4> write) {
        Span2D<ShadeVec4> plane = target.Span;
        for (int line = start; line < end && !cancel.IsCancellationRequested; line++) {
            Points(line, points);
            ulong faults = 0;
            for (int x = 0; x < write.Length; x++) {
                double age = Field(aged.AgeField, aged.Sampler, points[x], fallback: 0.0, ref faults);
                double cavity = Field(aged.CavityField, aged.Sampler, points[x], fallback: 1.0, ref faults);
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

    double Field(Option<TextureSource> source, SamplerState sampler, ShadePoint point, double fallback, ref ulong faults) {
        if (source.Case is not TextureSource field) { return fallback; }
        Fin<ShadeVec4> sampled = TextureUv.Sample(field, Sample(point, 0.0), sampler, key);
        if (sampled.Case is ShadeVec4 texel) { return texel.Luminance; }
        faults++;
        return fallback;
    }

    void Points(int line, Span<ShadePoint> points) {
        int width = plan.Width.Value, height = plan.Height.Value;
        int layer = line / height, y = line % height;
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

    double LayerCoord(int layer) =>
        plan.Law == LayerLaw.Volume ? (layer + 0.5) / plan.Layers.Value
        : plan.Layers.Value > 1 ? layer / (double)(plan.Layers.Value - 1)
        : 0.0;

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

## [04]-[PRESS_PRODUCT]

- Owner: `PressProduct` `[Union]` the content-identity veto made structural; `PressRun` the bake evidence.
- Cases: product {`Minted` (a CPU-pressed `TextureSet` with its key, wire-legal and persistable), `Preview` (GPU-pressed planes with no set, no key, and nothing addressable)}.
- Entry: the union IS the entry — a consumer matching on `PressProduct` reaches a `TextureSet` only through the `Minted` arm, so a GPU-keyed set is unrepresentable rather than merely forbidden.
- Packages: `Rasm` (project — `IValidityEvidence`/`ValidityClaim` the corpus evidence floor, whose implicit `bool` conversion is what lets a bare predicate sit beside a claim row so `ValidityClaim.Of(` appears nowhere), `set#TEXTURE_SET` (composed — `TextureSet`/`TextureChannel`), `plane#TEXTURE_PLANE` (composed — `TexturePyramid` and its decoded row accessor), `filter#PLANE_OP` (composed — `PlaneTrace`), System.Numerics.Tensors (composed — `TensorPrimitives.Subtract`/`MaxMagnitude`, the parity row fold), LanguageExt.Core.
- Growth: a new `PressRun` column is one field the fold already computes, because the plan key names the request and the run record names the run; a new product class is a new case, and the two that exist partition the authority question.
- Boundary: the `Preview` case carries planes and its run and NOTHING addressable — no set, no key, no digest — so a GPU result cannot be persisted, wired, or content-addressed by accident, and the structural veto costs no runtime check anywhere downstream. Every measured column is a TYPED ABSENCE where nothing measured it: `GraphKey` is absent for a field or slab subject rather than a zero every graphless press would share, `GpuDeltaMax` — the frozen `[04.4]` spelling, carried under the wire's own name so the `interchange#MATERIAL_WIRE` `[Mapper]` that projects this run needs no rename row and this page mints no mapper of its own — is absent for a single-lane press rather than a zero the parity gate reads as a perfect match, and `Aging` is absent for every program but the aged one rather than a zero-span census a gate reads as a one-cell ladder, exactly the forged-zero the corpus refuses on every tally, level, and run column; the declared-versus-visited pair on that census is what makes an over-quantized ladder and an unexercised cavity dimension legible from the run rather than from a second press; at the wire the interior lowers once, preserving both `GpuDeltaMax` and `GraphKey` as protobuf absence where their domain options are absent. `PressProduct.Parity` is the ONE producer that fills the delta — it folds the per-channel maximum over both lanes' base-level decoded rows and stamps the minted run — and the `Projection/benchmarks` parity workload COMPOSES it by pressing one plan on both lanes; a measurement fold living only inside a benchmark leaves the run column with no owner in the engine that declares it. `GraphKey` folds the COMPILED ORDER — each node's port id then its case name — because the frozen topological sort is what the bake evaluated, so a re-authored graph whose nodes reorder textually but compile to one order keys identically, and a graph whose evaluation order genuinely changed keys differently. `Texels` sums every level's own extent rather than multiplying the base by the level count, so texels-per-second is a throughput number rather than one inflated threefold by a full chain — and `TexelsPerSecond` is itself an `Option<double>`, because an unclocked run measured no rate and a `0.0` reads to a benchmark as sixteen million texels produced infinitely slowly, the same forged zero every other column here refuses. `Planes` carries each channel's own `PlaneTrace`, so the height solver's true relative residual reaches the benchmark rather than dying inside the fold — and `Residual` selects it DETERMINISTICALLY, the height channel first then roster order, where a hash-order enumeration handed a different channel's number per run; `Downgraded` names every channel whose paired mip policy fell back to the box floor, and `Faulted` every channel whose band kernel neutral-filled under a failure with its texel tally, so both quality decisions the press made silently become decisions the run reports. `IsValid` reads what the run alone can prove: `webgpu` beside a graph key or an aging census is invalid evidence, because the accelerator lane refuses both graph and ladder subjects at plan admission and a run contradicting that law was forged, and a census claiming more visited rungs than the plan declared or an inverted age span is a fabricated column rather than a measured one — the STRONGER Minted-authority gate lives where the set meets the wire, `interchange#MATERIAL_WIRE` `AppearanceEgress.Set` proving every press run content-authoritative, and this page names that owner rather than claiming a check the run's own columns cannot see.

```csharp

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressProduct {
    private PressProduct() { }
    public sealed record Minted(TextureSet Set, PressRun Run) : PressProduct;
    public sealed record Preview(HashMap<TextureChannel, TexturePyramid> Planes, PressRun Run) : PressProduct;

    public PressRun Run => Switch(minted: static m => m.Run, preview: static p => p.Run);

    public static Fin<PressRun> Parity(Minted minted, Preview preview) =>
        minted.Run.PlanKey != preview.Run.PlanKey
            ? new MaterialFault.Parameter($"<parity-plan-mismatch:{minted.Run.PlanKey:x}:{preview.Run.PlanKey:x}>")
            : toSeq(preview.Planes.AsIterable())
                .FoldM(Option<double>.None, (carried, entry) =>
                    minted.Set.Channels.Find(entry.Key)
                        .Map(cpu => Divergence(cpu.Base, entry.Value.Base)
                            .Map(delta => Some(carried.Map(seen => Math.Max(seen, delta)).IfNone(delta))))
                        .IfNone(Fin.Succ(carried))).As()
                .Map(delta => minted.Run with { GpuDeltaMax = delta });

    static Fin<double> Divergence(TexturePlane cpu, TexturePlane gpu) {
        if (cpu.Width != gpu.Width || cpu.Height != gpu.Height || cpu.Layers != gpu.Layers || cpu.Lanes != gpu.Lanes) {
            return new MaterialFault.Parameter($"<parity-extent-mismatch:{cpu.Width.Value}x{cpu.Height.Value}x{cpu.Layers.Value}:{gpu.Width.Value}x{gpu.Height.Value}x{gpu.Layers.Value}>");
        }
        using SpanOwner<double> left = SpanOwner<double>.Allocate(cpu.RowScalars);
        using SpanOwner<double> right = SpanOwner<double>.Allocate(gpu.RowScalars);
        using SpanOwner<double> delta = SpanOwner<double>.Allocate(cpu.RowScalars);
        double worst = 0.0;
        for (int layer = 0; layer < cpu.Layers.Value; layer++) {
            for (int row = 0; row < cpu.Height.Value; row++) {
                cpu.Read(row, layer, left.Span);
                gpu.Read(row, layer, right.Span);
                TensorPrimitives.Subtract<double>(left.Span, right.Span, delta.Span);
                worst = Math.Max(worst, Math.Abs(TensorPrimitives.MaxMagnitude<double>(delta.Span)));
            }
        }
        return Fin.Succ(worst);
    }
}

public readonly record struct AgeCoverage(
    int AgeRungs, int CavityRungs, int CurvatureRungs, int AgeRungsVisited, int CavityRungsVisited,
    int CurvatureRungsVisited, double AgeMin, double AgeMax);

public sealed record PressRun(
    PressBackend Backend, UInt128 PlanKey, Option<UInt128> GraphKey, ulong Seed, ulong Texels, double ElapsedMs,
    HashMap<TextureChannel, PlaneTrace> Planes, Seq<TextureChannel> Downgraded, HashMap<TextureChannel, ulong> Faulted,
    Option<double> GpuDeltaMax, Option<AgeCoverage> Aging)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast((int)Math.Min(Texels, int.MaxValue), 1),
        ValidityClaim.Nonnegative(ElapsedMs),
        GpuDeltaMax.ForAll(static d => double.IsFinite(d) && d >= 0.0),
        Backend.ContentAuthoritative || GraphKey.IsNone,
        Aging.ForAll(static c =>
            c.AgeRungsVisited is > 0 && c.AgeRungsVisited <= c.AgeRungs
            && c.CavityRungsVisited is > 0 && c.CavityRungsVisited <= c.CavityRungs
            && double.IsFinite(c.AgeMin) && c.AgeMin <= c.AgeMax),
        Backend.ContentAuthoritative || Aging.IsNone);

    public bool ContentAuthoritative => Backend.ContentAuthoritative;

    public Option<double> TexelsPerSecond => ElapsedMs > 0.0 ? Some(Texels / (ElapsedMs / 1000.0)) : None;
    public Option<double> Residual =>
        Planes.Find(TextureChannel.Height).Bind(static trace => trace.Residual)
            .Match(
                Some: Some,
                None: () => toSeq(TextureChannel.Items).Choose(c => Planes.Find(c).Bind(static r => r.Residual)).Head);
}
```

## [05]-[RESEARCH]

(none)
