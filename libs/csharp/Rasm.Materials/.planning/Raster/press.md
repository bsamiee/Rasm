# [MATERIALS_PRESS]

THE BAKE ENGINE. One `TexturePress.Press` fold drives a `PressSubject` — a compiled `graph#MATERIAL_GRAPH` node program, a raw `texture#TEXTURE_UV` procedural field, a `surface#OPENPBR_SLAB` parameter vector under a spatially-varying aging trajectory, or a `finish#FINISH` Kubelka-Munk pigment mix under a spatially-varying weight simplex — across a `PressPlan`'s texel grid and mints a `set#TEXTURE_SET` `TextureSet` of content-keyed `plane#TEXTURE_PLANE` pyramids. The four subjects compile ONCE into one `PressProgram`, and the row kernel dispatches on that program per BAND rather than per texel, so a plane costs one dispatch per partition and one allocation-free pass per row. The two spatially-varying subjects share one quantization law: a fallible per-texel admission is unaffordable at sixteen million points, so each compiles a LADDER of admitted vectors over its own declared rung columns — the aging ladder over the `(age, cavity)` PRODUCT the `weathering#WEATHERING` `CavityResponse` rows make irreducible to one scalar, the mix ladder over the barycentric weight simplex — and every texel indexes the cell its fields sampled, so a spatially-varying bake pays a declared cell count rather than a per-texel rail. Batching rides `graph#MATERIAL_GRAPH` `CompiledGraph.ShadeSpan`: the port environment resolves ONCE into an index-addressed scratch whose slot order IS the frozen compiled sort, so a plane never rebuilds an immutable map per node per texel — the difference between minutes and days at four thousand square; the integrator's per-point `Shade` re-enters the SAME rail over a one-element window, so the press and the integrator drive one evaluation law. The band fold rides `ParallelHelper.For` over a seeded struct `IAction`, and every per-texel jitter derives from the TEXEL COORDINATE and the plan seed rather than from a sequential stream, so a band partition can never reorder a draw and a re-press at one seed is byte-identical at any processor count.

Persisted plane bytes are ALWAYS CPU-minted. The `PressBackend.WebGpu` row is an accelerator and preview lane whose output is never content-addressed, and that is a STRUCTURAL veto rather than an empirical tolerance: `PressProduct` gives the GPU arm a `Preview` case carrying planes and a receipt but NO `TextureSet`, so a GPU-keyed set has no spelling and cannot be persisted, wired, or addressed by accident. GPU `f32` cannot reproduce the CPU `f64` procedural lattice, so a GPU-keyed plane forks the content key at its own preimage; the divergence the parity workload measures rides `PressReceipt.GpuDeltaMax` as TELEMETRY and never enters a key. Every measured column is a TYPED ABSENCE when nothing measured it, because a fabricated zero and an unmeasured pass are the two states a gate exists to separate — the graph key of a graphless subject, the parity delta of a single-lane press, the ladder coverage of an unaged run, and the wall time of an unclocked run all read absence rather than zero. The page composes `set#TEXTURE_SET` for the produced bundle, its channel roster, and each slot's `Read` projection, `plane#TEXTURE_PLANE` for the arena and the pyramid, `filter#PLANE_OP` for every post chain, every derived channel, and its `PlaneReceipt` evidence, `tile#TILE_SYNTH` for the in-fold tiling a plan requests, `gpu#PRESS_DEVICE` for the accelerator arm, `weathering#WEATHERING` for the aging ladder, `finish#FINISH` for the pigment-mix ladder, `surface#OPENPBR_SLAB` `ToneMap` for the one display egress a binding declares, the kernel `Deterministic` splitmix64 draw, `ContentHash` identity, and `ValidityClaim` receipt fold, `TimeProvider` for the one measured wall time, and CommunityToolkit.HighPerformance for every pooled arena and partitioned band — reminting no evaluator, no arena, no random source, no clock, no tone curve, and no identity.

## [01]-[INDEX]

- [02]-[PRESS_PLAN]: the `PressBackend` axis, the `PressSubject` union, the `ChannelBinding` row with its `DisplayEgress` column, the `LadderRungs` quantization carrier, the `PressPlan` record with its canonical plan key, and the binding-order law that seats derived channels after their sources and paired channels after their companions.
- [03]-[TEXTURE_PRESS]: the one `TexturePress.Press` entry, the `PressProgram` compiled subject, the `AgeLadder` two-dimensional aged-vector table and the `MixLadder` weight-simplex table, the `PressRows` band kernel over `ParallelHelper.For`, the coordinate-keyed jitter law, the paired mip resolution, the GPU lowering gate, and the post-fold and tiling composition.
- [04]-[PRESS_RECEIPT]: the `PressProduct` union that makes the content-identity veto structural, and the `PressReceipt` evidence row with its per-channel plane receipts, its `AgeCoverage` ladder-exercise column, and its typed absences.

## [02]-[PRESS_PLAN]

- Owner: `PressPlan` the bake request; `PressSubject` `[Union]` the thing being baked; `ChannelBinding` the per-channel request row; `DisplayEgress` the per-binding scene-to-display policy; `LadderRungs` the quantization carrier every ladder reads; `PressBackend` `[SmartEnum<string>]` the execution lane.
- Cases: subject {`Graph` (a `MaterialGraph` with the parameter row and conductor its sink resolves against), `Source` (one `TextureSource` sampled through a `SamplerState` into one channel), `Slab` (a `MaterialParameters` row lowered to the OpenPBR vector, aged per texel by a `TextureSource` age field and a `TextureSource` cavity field), `Mix` (a `finish#FINISH` pigment set resolved per texel through a `TextureSource` weight field per pigment)} · backend {`cpu` (content-authoritative), `webgpu` (accelerator, never content-authoritative)}.
- Law: binding ORDER is derived, never authored — `Of` sorts bindings by `TextureChannel.Origin` depth, then by pair dependency, then by `TextureChannel.Ordinal`, so every `Shaded` and `Geometric` channel seats before any `Derived` one, a normal seats before the roughness whose mip fold consumes its variance, and a plan requesting `occlusion` without `height` produces `height` as an intermediate rather than refusing. A caller never sequences the fold.
- Law: spatial cavity evidence enters the press as its OWN field, never as a derived channel. A `Slab` subject's own `occlusion` chain derives from `height`, which derives from `geometry_normal`, whose origin is a CONSTANT the shade point never reaches — so a slab press solves a flat height field and produces a uniform-1.0 occlusion plane by construction, and no binding order could rescue it because `Compile` is the first generator of the fold and nothing is landed when the ladder is read. `IDEAS.md [MESH_SPACE_BAKE]` is the card that makes a derived occlusion real; until it lands, the cavity field is caller-supplied evidence exactly as the age field is.
- Law: the cavity field carries the CAVITY scalar — `1.0` the fully occluded crevice — while the `set#TEXTURE_SET` `occlusion` channel stores VISIBILITY (`filter#PLANE_OP` deposits `open/rays` and the row's own neutral is `1.0` unoccluded), so an occlusion plane crosses into a cavity field through the landed `RemapCurve.Levels.Invert` row and never by a raw bind; the raw bind ages every `Crevice` effect on the open face and reads as a plausible plane rather than as a fault. An ABSENT cavity field reads `1.0` because a `Crevice` effect with no cavity evidence is the uniform aging the ladder already spells, while `0.0` would silently delete every crevice effect and run every exposed one at full age.
- Entry: `public static Fin<PressPlan> Of(PressPlanDraft draft, PressSubject subject, Op key)` is the ONE plan admission — extent, layer law, binding uniqueness, pack membership, format width, display egress, ladder rungs, subject arity, ladder cell budget, tile-guide coverage, and backend lowerability all gate here so the bake fold itself carries no re-check; the subject enters the admission because arity, cell budget, and lowerability are facts of the SUBJECT against the bindings, and deferring them to dispatch means a caller learns the veto after renting a device; a cyclic `Derived.From` chain refuses here through the roster-bounded depth walk, so the bake fold recurses on a proven-acyclic roster. `PlanKey` is the canonical content key the receipt records and the cache keys on.
- Packages: `set#TEXTURE_SET` (composed — `TextureChannel`/`ChannelPack`/`LayerLaw`/`TextureSet`/`SinkSlot`), `plane#TEXTURE_PLANE` (composed — `PlaneFormat`/`PlaneQuantity`/`MipPolicy`/`AlphaMode`), `filter#PLANE_OP` (composed — `PlaneOp` the post chain), `tile#TILE_SYNTH` (composed — `TilePolicy` the in-fold tiling request), `Rasm.Materials.Appearance.Graph` (composed — `MaterialGraph`/`MaterialParameters`), `Rasm.Materials.Appearance.Surface` (composed — `ConductorMetal`, `ToneOperator`/`DisplayEncoding` the display egress rows), `Rasm.Materials.Appearance.Texture` (composed — `TextureSource`/`SamplerState`/`UvFrame`), `Rasm.Materials.Appearance.Weathering` (composed — `WeatheringDose`/`AgeParameter`), `Rasm.Materials.Appearance.Finish` (composed — `FinishKind`/`FinishLayer`/`Pigment` the mix subject's own vocabulary), `Rasm.Element.Composition` (the SEAM `MaterialId`), `Rasm` (project — `ContentHash.Of` the one identity entry, `Dimension`, `UnitInterval`, `Op`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new bake subject is one `PressSubject` case with its `PressProgram` arm; a new execution lane is one `PressBackend` row carrying its authority column; a new per-channel request knob is one `ChannelBinding` column; a new quantized axis is one `LadderRungs` column read by the ladder that owns it. There is NO `BakeGraph`/`BakeField`/`BakeSlab`/`BakeMix` family — the subject's own case discriminates, and a caller holding a subject calls one entry.
- Boundary: `PressBackend` carries `ContentAuthoritative` as a ROW COLUMN rather than as a caller flag, so the content-identity law is data the plan admission reads and `[04]-[PRESS_RECEIPT]` enforces at the type level. Mip policy is a per-binding `Option<MipPolicy>` defaulting to the channel row's own law and spelling `MipPolicy.None` for a single-level plane — a plan-level `Mips` boolean beside a per-binding override is one knob selecting between two bodies, and the row already carries the answer. The plan key is `ContentHash.Of` over the plan's canonical bytes — extent, layer law, the ordered binding rows with their channels, formats, resolved mip policies, pack keys, post chains, and display egress rows, the backend key, the seed, the alpha mode, the height scale, the three `LadderRungs` columns, the tile policy, and the subject's own `UvFrame` digest — and it EXCLUDES the material id and the conductor, which name the subject rather than the bake, so two materials pressed under one plan share a plan key and the receipt separates them by graph key. The UV frame is the one subject-borne column the key ADMITS, and the discriminant is what the column does: a material id and a conductor NAME the subject, while a `UvFrame` offset, scale, or rotation SHAPES the bake — `texture#TEXTURE_UV` `TextureUv.Sample` applies it before the source dispatch, so a re-tiled bake is different bytes, and a frame outside the preimage is a cached plane a second tiling silently inherits. `UvFrame.Digest` reads EMPTY at identity by its owner's construction, so an untransformed bind keys byte-identically to a plan that never knew the axis existed and landing the column re-keys no blob already addressed. `Layers` and `LayerLaw` ride the plan so a cube map, a flipbook, and a volume are one bake shape at different rows; a UDIM set is N plans sharing a key, never one plan carrying a tile list, because a UDIM tile is an independent extent whose planes address independently — the per-tile products assemble at `set#TEXTURE_SET` `UdimSheet.Of`, the one owner proving the tiles agree. A binding naming a channel already inside a requested pack REFUSES at admission — the pack owns those slots and a standalone duplicate keys the set twice for one field — and a binding whose `Format` carries fewer components than its channel declares refuses for the same reason `set#TEXTURE_SET` refuses it later: a three-component normal in a two-component plane is a reconstruction the sampler cannot invert without evidence the plane does not carry. A `Source` subject binds EXACTLY ONE channel, because a procedural field has one value and a second bound channel would silently receive its neutral; a `Mix` subject binds one weight field PER PIGMENT, because a mix whose simplex is short one axis resolves a pigment nothing weights; a `Tile` policy whose guide channel no binding produces refuses, because the synthesizer would rail on a set that admitted cleanly. A binding whose channel carries an OPEN photometric scale — `PlaneQuantity.Light` beside a `ChannelUnit` other than `none`, which is `emission_luminance` alone on the landed roster — stored in a `PlaneFormat` the plane page reports `Normalized` with NO `Display` egress refuses at admission: an unbounded cd/m² value hard-clips at unity in a unorm lane with no tone curve, and a clipped emission plane is indistinguishable from an authored one downstream. The lane test reads `plane#PLANE_FORMAT` `Normalized`, the estate's one unorm-versus-float discriminant, because the kernel `ChannelDtype` roster the format seats onto carries no normalization column of its own. The `webgpu` backend gates LOWERABILITY at admission, not at dispatch: a `Source` subject over an `Image` case or a `Triplanar` whose projected source is not a solid `Noise` (the three-plane 2D blend has no kernel arm), or a `Graph`, `Slab`, or `Mix` subject, has no kernel row on `gpu#WGSL_KERNEL` and refuses with the offending case named, so a caller learns the veto before renting a device rather than after. A `Noise` source — planar OR solid — and a `Triplanar` over a solid `Noise` lower to `noiseField`: the solid family rides the row's `dimension` column, a triplanar's three planes sample one world point for a solid projected noise so the blend IS the 3D field and the world scale folds into the frequency word, and the widening moves no identity law — the accelerator product stays a `Preview`, CPU bytes stay canonical.

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
using Rasm.Materials.Appearance.Graph;            // MaterialGraph, CompiledGraph, MaterialParameters, ShadePoint, SurfaceShade, PortValue, GraphContext
using Rasm.Materials.Appearance.Surface;          // OpenPbrSurface, ConductorMetal, ToneMap, ToneOperator, DisplayEncoding
using Rasm.Materials.Appearance.Texture;          // TextureSource, TextureUv, UvSample, SamplerState, UvFrame, ShadeVec4
using Rasm.Numerics;                              // Dimension, UnitInterval
using Rhino.Geometry;                             // Point3d, Vector3d — the shade point's host geometry edge
using Thinktecture;
using Wacton.Unicolour;                           // Pigment — the finish#FINISH Kubelka-Munk reflectance carrier the mix ladder resolves
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// ContentAuthoritative is the content-identity law as ROW DATA: the plan admission reads it and the
// PressProduct union enforces it at the type level, so no caller flag can promote a GPU plane to a key.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PressBackend {
    public static readonly PressBackend Cpu    = new("cpu",    contentAuthoritative: true);
    public static readonly PressBackend WebGpu = new("webgpu", contentAuthoritative: false);
    public bool ContentAuthoritative { get; }
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
    public sealed record Slab(MaterialParameters Row, ConductorMetal Conductor, Seq<WeatheringDose> Aging,
        Option<TextureSource> AgeField, Option<TextureSource> CavityField, SamplerState Sampler) : PressSubject;
    public sealed record Mix(FinishKind Kind, Seq<Pigment> Pigments, Seq<TextureSource> WeightFields,
        Seq<FinishLayer> Stack, Option<MaterialParameters> Substrate, ConductorMetal Conductor, SamplerState Sampler) : PressSubject;

    // The one subject-borne fact the plan key admits: a UvFrame SHAPES the bake where a material id and a conductor
    // merely NAME the subject, so the digest crosses into the preimage while the naming columns stay excluded. A
    // Graph subject samples through its own node closures and declares no press-level frame.
    public Option<UvFrame> Frame => Switch(
        graph:  static _ => Option<UvFrame>.None,
        source: static s => Some(s.Sampler.Frame),
        slab:   static b => Some(b.Sampler.Frame),
        mix:    static m => Some(m.Sampler.Frame));
}

// --- [MODELS] ------------------------------------------------------------------------------
// The quantization carrier every ladder reads: three declared axes on ONE plan column rather than three loose int
// knobs an admission would guard three times and a preimage would append three times. Each axis is the rung count
// over its own [0,1] parameter, Degenerate names the first axis below the two-rung floor a lerp needs, and Digest
// is the one preimage fragment — a quantization that moves the produced bytes enters the plan key exactly as the
// post chain and the tile fragment do. The canonical row prices the aging product it exists for: 16 age rungs
// against 8 cavity rungs is 128 fallible admissions per press against sixteen million texels at four thousand
// square, which is the same three-orders margin the one-dimensional ladder bought, one dimension wider.
public readonly record struct LadderRungs(int Age, int Cavity, int Mix) {
    public static readonly LadderRungs Default = new(Age: 16, Cavity: 8, Mix: 8);
    const int Floor = 2;

    public Option<(string Axis, int Rungs)> Degenerate =>
        Age < Floor ? Some((nameof(Age), Age))
        : Cavity < Floor ? Some((nameof(Cavity), Cavity))
        : Mix < Floor ? Some((nameof(Mix), Mix))
        : Option<(string, int)>.None;

    public string Digest => string.Create(CultureInfo.InvariantCulture, $"rungs|{Age}|{Cavity}|{Mix}");
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
    PressBackend Backend, ulong Seed, AlphaMode Alpha, double HeightScaleMm, LadderRungs Rungs,
    Option<TilePolicy> Tile, Option<MaterialId> Material, Option<ConductorMetal> Conductor);

public sealed record PressPlan(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law, Seq<ChannelBinding> Bindings,
    PressBackend Backend, ulong Seed, AlphaMode Alpha, double HeightScaleMm, LadderRungs Rungs,
    Option<TilePolicy> Tile, Option<MaterialId> Material, Option<ConductorMetal> Conductor, UInt128 PlanKey) {

    // The ONE plan admission. Binding order is DERIVED here — sources before derivations, normals before the
    // roughness rows whose mip fold consumes their variance — so the fold never re-sorts and a caller never
    // sequences. Subject arity gates here too, so a Source plan cannot silently neutral-fill a second channel.
    public static Fin<PressPlan> Of(PressPlanDraft draft, PressSubject subject, Op key) =>
        from _ in guard(!draft.Bindings.IsEmpty, MaterialFault.Parameter(key, "<press-plan-no-bindings>"))
        from __ in guard(draft.Law.Admits(draft.Layers.Value), MaterialFault.Parameter(key, $"<layer-law-rejects:{draft.Law.Key}:{draft.Layers.Value}>"))
        from ___ in guard(draft.Bindings.Map(static b => b.Channel).Distinct().Count() == draft.Bindings.Count, MaterialFault.Parameter(key, "<press-binding-duplicate-channel>"))
        from ____ in draft.Rungs.Degenerate.Match(
            Some: bad => Fin.Fail<Unit>(MaterialFault.Parameter(key, $"<ladder-degenerate:{bad.Axis}:{bad.Rungs}>")),
            None: () => Fin.Succ(unit))
        // A roster whose Derived.From chain cycles has no fold order — the bounded Depth walk names it here,
        // where an unbounded recursion inside the bake would stack-overflow instead of railing.
        from _____ in guard(draft.Bindings.ForAll(static b => Depth(b.Channel) >= 0), MaterialFault.Parameter(key, "<derived-origin-cycle>"))
        from ______ in AdmitSubject(draft, subject, key)
        from _______ in draft.Tile.Match(
            Some: policy => guard(draft.Bindings.Exists(b => b.Channel == policy.Guide), MaterialFault.Parameter(key, $"<tile-guide-unbound:{policy.Guide.Key}>")),
            None: () => Fin.Succ(unit))
        from ________ in draft.Bindings.Fold(Fin.Succ(unit), (acc, b) => acc.Bind(_ => AdmitBinding(draft, b, key)))
        from _________ in guard(draft.Backend.ContentAuthoritative || Lowerable(subject), MaterialFault.Parameter(key, $"<gpu-unlowerable-subject:{subject.GetType().Name}>"))
        let ordered = Order(draft.Bindings)
        select new PressPlan(draft.Width, draft.Height, draft.Layers, draft.Law, ordered, draft.Backend,
            draft.Seed, draft.Alpha, draft.HeightScaleMm, draft.Rungs, draft.Tile, draft.Material, draft.Conductor,
            Mint(draft, ordered, subject));

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
            mix:    static (s, m) => m.Pigments.IsEmpty || m.Pigments.Count != m.WeightFields.Count
                ? Fin.Fail<Unit>(MaterialFault.Parameter(s.Key, $"<mix-subject-weight-arity:{m.Pigments.Count}!={m.WeightFields.Count}>"))
                : MixLadder.Cells(s.Draft.Rungs.Mix, m.Pigments.Count) is var cells && cells <= MixLadder.CellCeiling
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
        from __ in guard(binding.Pack.Match(Some: p => p.Slots.Contains(binding.Channel), None: static () => true), MaterialFault.Parameter(key, $"<binding-pack-foreign:{binding.Channel.Key}>"))
        from ___ in guard(binding.Pack.IsSome || !draft.Bindings.Exists(other => other.Pack.Match(Some: p => p.Slots.Contains(binding.Channel), None: static () => false)),
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
    static bool Lowerable(PressSubject subject) =>
        subject is PressSubject.Source source && source.Field switch {
            TextureSource.Noise or TextureSource.Checker or TextureSource.Gradient => true,
            TextureSource.Triplanar { Projected: TextureSource.Noise { Solid: true } } => true,
            _ => false,
        };

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
                $"{source.Draft.Width.Value}x{source.Draft.Height.Value}x{source.Draft.Layers.Value}|{source.Draft.Law.Key}|{source.Draft.Backend.Key}|{source.Draft.Seed:x16}|{source.Draft.Alpha.Key}|{source.Draft.HeightScaleMm:R}"));
            Piece(source.Draft.Rungs.Digest);
            foreach (ChannelBinding binding in source.Ordered) {
                Piece(string.Create(CultureInfo.InvariantCulture,
                    $"{binding.Channel.Key}|{binding.Format.Key}|{binding.Policy.Key}|{binding.Pack.Match(Some: static p => p.Key, None: static () => string.Empty)}"));
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
- Entry: `public static Fin<PressProduct> Press(PressSubject subject, PressPlan plan, Op key, TimeProvider? clock = null, CancellationToken cancel = default)` is the ONE bake — it compiles the subject once, folds every direct binding, applies each binding's post chain, derives every derived channel from its landed source through the channel's OWN declared fold, composes every requested pack from the landed chains, builds the mip chains against their paired companions, tiles when the plan requests it, and admits the result through `set#TEXTURE_SET` `TextureSet.Of`; the caller composes a `PressProduct` and never orchestrates a stage, and the token cancels between bindings and inside every band onto the kernel cancel rail.
- Packages: `graph#MATERIAL_GRAPH` (composed — `MaterialGraph.Compile` ONCE per press, `CompiledGraph.ShadeSpan` the batched evaluator, `CompiledGraph.ScratchWidth` the per-band scratch the fold rents against, `ShadePoint`, `GraphContext.Tolerant`), CommunityToolkit.HighPerformance (`ParallelHelper.For<TAction>(int, int, in TAction)` over a SEEDED `struct IAction` so the partition allocates nothing, inlines, clamps to the processor count, and carries its state — the unseeded overload default-constructs the action and would lose every field the fold needs; `ParallelHelper.ForEach<TItem, TAction>(Memory<T>, in TAction, minimumActionsPerThread)` over a seeded `struct IRefAction<T>` the pack composition's per-level item fold rides, each worker taking its own `ref` job; `MemoryOwner<T>.Allocate` the per-binding staging arena, `SpanOwner<T>.Allocate` the per-band point/scratch/shade rentals, `Memory2D<T>`/`Span2D<T>` the plane views), `Rasm` (project — `Deterministic.Stream`/`NextUnit` the lane-exact coordinate-keyed per-texel draw), `set#TEXTURE_SET` (composed — `SinkSlot.Read` the per-slot `SurfaceShade` column reader, `ChannelOrigin` the per-channel production law), `filter#PLANE_OP` (composed — `PlaneOp.Apply(TexturePlane, Seq<PlaneOp>, Op, TimeProvider?)` and its `PlaneReceipt`, for every post chain and every derived channel), `tile#TILE_SYNTH` (composed — `TileSynth.Tileify` when the plan carries a policy), `gpu#PRESS_DEVICE` (composed — `PressDevice.Acquire`/`Dispatch` on the accelerator arm), `weathering#WEATHERING` (composed — `Weathering.Apply` at each cell of the age ladder, taking the cell's own `AgeParameter` and `UnitInterval` cavity scalar), `finish#FINISH` (composed — `FinishMix.Of` and `Finish.Resolve` at each cell of the mix ladder), `surface#OPENPBR_SLAB` (composed — `ToneMap.Apply`/`ToneMap.Encode` the one display egress a binding declares).
- Growth: a new evaluation shape is one `PressSubject` case and one `PressProgram` arm; a new post-processing step is one `filter#PLANE_OP` `PlaneOp` on a binding's chain; a new derived channel is one `ChannelOrigin.Derived` row on `set#TEXTURE_CHANNEL` carrying its own fold — the press discovers both the dependency and the operation from the roster and needs no edit.
- Boundary: the subject compiles ONCE into a `PressProgram` — a graph resolves its topological order into a frozen `CompiledGraph` and its constant OpenPBR vector, a slab builds its whole age ladder, a mix builds its whole simplex ladder, a field captures its sampler — and the BAND kernel dispatches on that program once per partition rather than once per texel, so a four-thousand-square plane pays four dispatches per core instead of sixteen million. `ShadeSpan` re-enters over the compiled order with the port environment resolved into an INDEX-ADDRESSED scratch whose slot order is that sort, so a plane costs one allocation-free pass per row instead of one immutable-map rebuild per node per texel; the integrator's per-point `Shade` re-enters this same rail over a one-element window, one evaluation law at two grains. Band parallelism rides `ParallelHelper.For` over a SEEDED `struct IAction`: the seeded overload copies the caller's action into each partition, where the unseeded overload default-constructs it and would hand every band an empty program, an empty plan, and a null target. Each band rents its point, scratch, and shade spans ONCE from `SpanOwner<T>` and walks its own rows, so a partition rents once where a per-row action rents per row. Per-texel jitter derives from the TEXEL COORDINATE and the plan seed through `Deterministic.Stream` over the `(x, y, layer, ordinal)` lanes with the FULL 64-bit seed as its own lane — the receipt column replays the draw with no two-int salt split — and both axis draws advance that texel-local state through `NextUnit`, never a sequential stream; a band partition cannot reorder a draw, two channels of one press do not share a jitter sequence, a re-press at one seed is byte-identical regardless of processor count, and `Position` agrees with the jittered UV so a subject reading both sees one point. The LAYER axis is the plan's own `LayerLaw` evaluated per line — `cubeFaces` derives the frozen per-face direction into `Position` (the faceDir transcription both the CPU bake and the GPU kernel pin to the freeze), `volume` the slab-centre depth coordinate, `frames` the frame coordinate — so the layer rows are real evaluation shapes. The `Graph` program varies the FIVE sink columns per texel through each slot's own `SinkSlot.Read` projection and reads every other bound channel off its constant lowered vector — the `graph#MATERIAL_GRAPH` `BsdfOutput` sink carries five columns, so a coat-roughness plane pressed from a graph subject is honestly constant rather than silently mis-projected from base colour. The `Slab` program reads a TWO-DIMENSIONAL QUANTIZED AGE LADDER over the `(age, cavity)` PRODUCT, because the aged vector does not factor through one scalar: each dose's effective age is its own row's `weathering#WEATHERING` `CavityResponse.Scale(age, occlusion)`, so a dose set mixing a `Crevice` row with an `Exposed` row produces a fold no single parameter indexes, and the fold is order-dependent because each step lerps toward its own terminal. `Weathering.Apply` therefore runs once per CELL across the plan's own `LadderRungs.Age × LadderRungs.Cavity` quantization and every texel indexes the cell its two fields sampled, so a spatially-aged bake costs the declared cell count in fallible parameter admissions rather than one per texel; the canonical `16 × 8` is 128 admissions against sixteen million texels at four thousand square, the same three-orders margin the one-dimensional ladder bought, one dimension wider. The `Mix` program reads the SAME shape over the barycentric weight simplex: `Finish.Resolve` runs once per lattice point of the compositions of `LadderRungs.Mix − 1` into the pigment count, each texel's sampled weight vector normalizes and quantizes by largest remainder onto that lattice, and the cell budget gates at admission because the count grows combinatorially in the pigment count where the aging product grows as a plane. Both rung counts are declared plan columns entering the plan key rather than hidden approximations — a caller needing a continuous trajectory presses at a finer ladder, which is one column, not a different fold. Staging is `ShadeVec4` and quantization is `plane#TEXTURE_PLANE`'s: the fold writes decoded four-lane texels into a `Memory2D<ShadeVec4>` arena and the plane's own row `Write` rail associates alpha, encodes the transfer, and narrows to the binding's `PlaneFormat`, so exactly one quantizer exists in the corpus and a press never encodes a texel itself. A binding declaring a `DisplayEgress` grades its staging row BEFORE that crossing — `ToneMap.Apply` per texel at the row's exposure and operator, then `ToneMap.Encode` over the whole row into the target encoding — so scene-referred radiance reaches an integer lane through the corpus's one tone-map owner and never through the transfer clip a raw narrow performs; the transform runs at the staging seam alone, so the GPU preview lane inherits it through the same `Fill` rail and the two lanes still differ in authority rather than in encoding. Derived channels fold AFTER their sources land, through the channel row's OWN `ChannelOrigin.Derived.Fold` step composed BEFORE the binding's post chain — `height` from `geometry_normal` by the spectral height inversion, `occlusion` and `curvature` from `height` — so the derivation is roster data rather than a caller-supplied post chain the press hopes contains the right operation, and a source channel a plan did not request is produced as an intermediate and then dropped unless bound. A PAIRED mip policy resolves its companion from the landed map through `TextureChannel.Pair`; where the plan bound no companion the policy DOWNGRADES to `MipPolicy.Box` and the receipt records the channel by name, because the alternative — passing an unpaired paired policy to `TexturePyramid.Of` — refuses the whole press for a quality floor the corpus already declares acceptable. Tiling is IN-FOLD when the plan carries a policy: `tile#TILE_SYNTH` `TileSynth.Tileify` runs over the admitted set so every channel takes ONE plan and the resulting set re-keys, and the set's `Tiled` proof is the gate's own mint rather than the plan's request. The GPU arm dispatches per binding against a device acquired once for the whole press and released at the fold's close, runs no post chain and no derivation, and its product is a `Preview` — no set, no key, nothing addressable — so the content-identity law needs no runtime check anywhere downstream. `Lower` carries the PROVEN field forward beside its kernel row, so `Stage` reads a `TextureSource` rather than re-testing the subject union and minting an arm the plan admission already made unreachable; the uniform block then writes through the one `gpu#PRESS_DEVICE` `KernelUniform` word writer in the row's own declared order, colour columns crossing as `Vec4` appends because the CPU arm lerps the source's own `Low`/`High` through `ShadeVec4` and a scalar pair previews every authored ramp as grey. `Lift` widens the `f32` readback back into `ShadeVec4` staging and crosses into the plane through the SAME `Fill` rail the CPU lane takes, so the two lanes differ in authority and never in encoding; a short readback rails rather than zero-filling its tail, because a truncated dispatch reading as a black band is the failure a preview is least likely to be checked for, and a `Coupled` mip policy downgrades to the box floor since the preview lane lands no companion plane to read a variance from. A REQUESTED PACK is built AFTER the channels land: each lane composes PER LEVEL from its slot channel's own already-folded chain — the frozen per-component mip law holding by construction — an absent slot carries its channel neutral at every level, the packed channels leave the standalone roster, and slots whose chains disagree on depth refuse by name; the composed carrier records the box floor while the per-component truth stays the slot rows' own `Mip` columns. A band-kernel FAILURE is counted evidence: the failing rows fill the channel neutral AND tally into `PressReceipt.Faulted` through one interlocked cell, so a plane whose evaluation died and dressed as neutral is separable from a genuinely-neutral plane at the receipt. CANCELLATION rides the one kernel rail: `Press` takes a `CancellationToken`, every band polls it per line, the fold checks it between bindings, and a cancelled press rails the kernel `Fault.Cancelled` after DISPOSING every landed plane — no partial product survives and no arena leaks, the same discipline every failure arm holds through `Released`. A `Slab` with no age field bakes UNAGED (`0.0`) — the material as authored — because the silent fully-weathered default was the inverted intuition a caller discovers only in the render; a `Slab` with no CAVITY field reads `1.0` instead, the full-cavity extreme at which every `Crevice` row ages at the raw age and the whole ladder collapses to its uniform column, so an absent cavity field degrades to the aging a caller who never heard of the axis expects. The `Aged` arm alone fills `PressReceipt.Aging`: one interlocked cell folds the visited age and cavity rungs as bitsets beside the sampled age extrema, so an over-quantized ladder (thirty-two rungs visiting two) and an unexercised cavity dimension (a whole axis at one column) read off the receipt instead of off a second press. Wall time rides the injected `TimeProvider` and every per-channel `PlaneReceipt` rides the same clock, so the press receipt carries measured elapsed and the height solver's true residual rather than a literal zero. The `[EXPRESSION_SPINE]` exemptions are the `PressRows` band arms, the `AgeLadder` and `MixLadder` builds with the simplex quantize-and-rank kernel, the pack `Compose`, and the `Fill` staging write: fixed-extent numeric folds over caller-owned buffers; every admission, dispatch, and egress surface is expression-bodied except the failure-disposal seams, which are resource boundaries.

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
        SamplerState Sampler, LadderCoverage Coverage) : PressProgram;
    public sealed record Mixed(MixLadder Ladder, Seq<TextureSource> WeightFields, SamplerState Sampler) : PressProgram;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The quantized aging trajectory over the (age, cavity) PRODUCT. Weathering.Apply is fallible and runs once per
// CELL, never once per texel, so a spatially-aged 4k bake costs AgeRungs x CavityRungs admissions instead of
// sixteen million. The second dimension is not a refinement: each dose's effective age is its own row's
// CavityResponse.Scale(age, occlusion), so a dose set mixing a Crevice row with an Exposed row yields a fold no
// single scalar indexes. Cells are AGE-MAJOR — the age index runs fastest — so At's one multiply-add addresses
// the plane, and both rung counts are plan columns entering the plan key.
public sealed record AgeLadder(Seq<OpenPbrSurface> Cells, int AgeRungs, int CavityRungs) {
    public static Fin<AgeLadder> Of(
        MaterialParameters row, ConductorMetal conductor, Seq<WeatheringDose> aging, LadderRungs rungs, Op key) =>
        toSeq(Enumerable.Range(0, rungs.Cavity * rungs.Age))
            .Fold(Fin.Succ(Seq<OpenPbrSurface>()), (acc, cell) =>
                acc.Bind(built => Weathering.Apply(row, aging,
                        AgeParameter.Create((cell % rungs.Age) / (double)(rungs.Age - 1)),
                        UnitInterval.Create((cell / rungs.Age) / (double)(rungs.Cavity - 1)), key)
                    .Map(aged => built.Add(OpenPbrSurface.Of(aged, conductor)))))
            .Map(built => new AgeLadder(built, rungs.Age, rungs.Cavity));

    public OpenPbrSurface At(double age, double cavity) => Cells[(Rung(cavity, CavityRungs) * AgeRungs) + Rung(age, AgeRungs)];

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
    // divisible by its own step, so no factorial is formed and nothing overflows before the ceiling saturates.
    internal static int Cells(int rungs, int pigments) {
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
            for (int step = 0; step < counts[axis]; step++) { rank += Cells(remaining - step + 1, counts.Length - axis - 1); }
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
    readonly ulong[] words = new ulong[Words(rungs.Age) + Words(rungs.Cavity) + 2];
    readonly int cavityAt = Words(rungs.Age);
    readonly int minAt = Words(rungs.Age) + Words(rungs.Cavity);

    static int Words(int count) => (count + 63) >> 6;

    internal LadderCoverage Seeded() {
        words[minAt] = BitConverter.DoubleToUInt64Bits(double.PositiveInfinity);
        words[minAt + 1] = BitConverter.DoubleToUInt64Bits(double.NegativeInfinity);
        return this;
    }

    internal void Visit(int ageRung, int cavityRung, double age) {
        Interlocked.Or(ref words[ageRung >> 6], 1UL << (ageRung & 63));
        Interlocked.Or(ref words[cavityAt + (cavityRung >> 6)], 1UL << (cavityRung & 63));
        Extremum(ref words[minAt], age, lower: true);
        Extremum(ref words[minAt + 1], age, lower: false);
    }

    public Option<AgeCoverage> Read() =>
        BitConverter.UInt64BitsToDouble(Volatile.Read(ref words[minAt])) switch {
            var low when double.IsFinite(low) => Some(new AgeCoverage(rungs.Age, rungs.Cavity,
                Visited(0, cavityAt), Visited(cavityAt, minAt), low,
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

    public static Fin<PressProduct> Press(PressSubject subject, PressPlan plan, Op key, TimeProvider? clock = null, CancellationToken cancel = default) {
        TimeProvider ticks = clock ?? TimeProvider.System;
        long opened = ticks.GetTimestamp();
        return plan.Backend.ContentAuthoritative
            ? Mint(subject, plan, key, ticks, opened, cancel)
            : Accelerate(subject, plan, key, ticks, opened, cancel);
    }

    // The CPU lane: compile once, fold every direct binding, post-process, derive, PACK, mip against pairs,
    // tile, admit. Each binding's staging arena lives exactly one Finish and disposes at its fold step — a
    // press never holds N staging arenas at once — and the set is minted LAST so a failed stage never leaves
    // a half-keyed bundle behind. A DIRECT binding is a non-derived one OR the field subject's own target, so
    // a Source plan targeting `height` bakes the field INTO height rather than synthesizing a normal map and
    // integrating it back out. Every landed plane DISPOSES on the failure and cancellation arms — a refused
    // press leaks no arena — and cancellation rails the kernel `Fault.Cancelled` between bindings and inside
    // every band, surrendering no partial product. Intermediates a derivation pulled in but no binding
    // requested are dropped from the set and disposed with it.
    static Fin<PressProduct> Mint(PressSubject subject, PressPlan plan, Op key, TimeProvider ticks, long opened, CancellationToken cancel) =>
        from program in Compile(subject, plan, key)
        from folded in plan.Bindings.Filter(b => Direct(program, b))
            .Fold(Fin.Succ((Planes: HashMap<TextureChannel, TexturePyramid>.Empty, Evidence: HashMap<TextureChannel, PlaneReceipt>.Empty, Downgraded: Seq<TextureChannel>(), Faulted: HashMap<TextureChannel, ulong>.Empty)), (acc, binding) =>
                acc.Bind(carried => cancel.IsCancellationRequested
                    ? Surrender<(HashMap<TextureChannel, TexturePyramid>, HashMap<TextureChannel, PlaneReceipt>, Seq<TextureChannel>, HashMap<TextureChannel, ulong>)>(carried.Planes)
                    : Land(program, plan, binding, carried, key, ticks, cancel).MapFail(fault => Released(carried.Planes, fault))))
        from derived in plan.Bindings.Filter(b => !Direct(program, b))
            .Fold(Fin.Succ(folded), (acc, binding) => acc.Bind(carried => cancel.IsCancellationRequested
                ? Surrender<(HashMap<TextureChannel, TexturePyramid>, HashMap<TextureChannel, PlaneReceipt>, Seq<TextureChannel>, HashMap<TextureChannel, ulong>)>(carried.Planes)
                : Derive(program, plan, binding, carried, key, ticks, cancel).MapFail(fault => Released(carried.Planes, fault))))
        from packed in Packed(plan, derived.Planes, key).MapFail(fault => Released(derived.Planes, fault))
        from set in TextureSet.Of(new TextureSetDraft(plan.Width, plan.Height, plan.Layers, plan.Law,
            NormalConvention.Gl, plan.Alpha, plan.HeightScaleMm, Option<TileProof>.None, Seq<UdimTile>(),
            packed.Channels.Filter((c, _) => plan.Bindings.Exists(b => b.Channel == c && b.Pack.IsNone)), packed.Packs,
            plan.Conductor, plan.Material), key).MapFail(fault => Released(derived.Planes, fault))
        from tiled in plan.Tile.Match(
            Some: policy => TileSynth.Tileify(set, policy, key, ticks).Map(static pair => pair.Set),
            None: () => Fin.Succ(set))
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
                            [.. pack.Slots.Map(static slot => slot.Neutral.X)])));
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
    static Fin<PressProduct> Accelerate(PressSubject subject, PressPlan plan, Op key, TimeProvider ticks, long opened, CancellationToken cancel) =>
        from lease in PressDevice.Acquire(DevicePolicy.Default, key)
        from planes in lease.Use((Subject: subject, Plan: plan, Key: key, Cancel: cancel), static (state, device) =>
            state.Plan.Bindings.Fold(Fin.Succ(HashMap<TextureChannel, TexturePyramid>.Empty), (acc, binding) =>
                acc.Bind(rows => state.Cancel.IsCancellationRequested
                    ? Surrender<HashMap<TextureChannel, TexturePyramid>>(rows)
                    : Lower(state.Subject, binding, state.Key)
                        .Bind(lowered => device.Dispatch(lowered.Kernel, Stage(state.Plan, lowered.Kernel, lowered.Field), state.Key))
                        .Bind(receipt => Lift(state.Plan, binding, receipt, state.Key))
                        .Map(plane => rows.Add(binding.Channel, plane))
                        .MapFail(fault => Released(rows, fault)))))
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
                .Map(ladder => (PressProgram)new PressProgram.Aged(ladder, b.AgeField, b.CavityField, b.Sampler, new LadderCoverage(s.Plan.Rungs).Seeded())),
            mix:    static (s, m) => MixLadder.Of(m, s.Plan.Rungs, s.Key)
                .Map(ladder => (PressProgram)new PressProgram.Mixed(ladder, m.WeightFields, m.Sampler)));

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
        // A pass-through binding rents zero-length buffers rather than growing a second write body: the branch is
        // per ROW, the pool answers a zero request with the shared empty array, and one rail keeps the layer walk.
        // The grade takes two buffers rather than aliasing one, so no unstated in-place contract binds ToneMap.
        int width = display.IsSome ? plane.Width.Value : 0;
        using SpanOwner<ShadeVec4> toned = SpanOwner<ShadeVec4>.Allocate(width);
        using SpanOwner<ShadeVec4> encoded = SpanOwner<ShadeVec4>.Allocate(width);
        for (int layer = 0; layer < plane.Layers.Value; layer++) {
            for (int row = 0; row < plane.Height.Value; row++) {
                ReadOnlySpan<ShadeVec4> line = source.GetRowSpan((layer * plane.Height.Value) + row);
                if (display.Case is not DisplayEgress egress) { plane.WriteShade(row, layer, line); continue; }
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
        Op key, TimeProvider ticks, CancellationToken cancel) {
        if (carried.Planes.ContainsKey(channel)) { return Fin.Succ(carried); }
        ChannelBinding implicitBinding = new(channel,
            PlaneFormat.For(channel.Components, ChannelDtype.Float32).IfNone(PlaneFormat.Rgba32F),
            Some(MipPolicy.None), Option<ChannelPack>.None, Seq<PlaneOp>(), Option<DisplayEgress>.None);
        return channel.Origin is ChannelOrigin.Derived
            ? Derive(program, plan, implicitBinding, carried, key, ticks, cancel)
            : Land(program, plan, implicitBinding, carried, key, ticks, cancel);
    }

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
    // it. Every append names its own width: the noise block interleaves nine floats with seven integer codes
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
        set.Channels.Fold(0UL, static (acc, pair) => acc + Levels(pair.Value)) + set.Packs.Fold(0UL, static (acc, pack) => acc + Levels(pack.Plane));

    static ulong Texels(PressPlan plan, HashMap<TextureChannel, TexturePyramid> planes) => planes.Fold(0UL, static (acc, pair) => acc + Levels(pair.Value));

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
            default: break;
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
                (double clampedAge, double clampedCavity) = (Math.Clamp(age, 0.0, 1.0), Math.Clamp(cavity, 0.0, 1.0));
                aged.Coverage.Visit(AgeLadder.Rung(clampedAge, aged.Ladder.AgeRungs),
                    AgeLadder.Rung(clampedCavity, aged.Ladder.CavityRungs), clampedAge);
                write[x] = Constant(aged.Ladder.At(clampedAge, clampedCavity), points[x]);
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
        for (int x = 0; x < width; x++) {
            ulong state = Deterministic.Stream([x, y, layer, channel.Ordinal], unchecked((long)plan.Seed));
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
- Boundary: the `Preview` case carries planes and a receipt and NOTHING addressable — no set, no key, no digest — so a GPU result cannot be persisted, wired, or content-addressed by accident, and the structural veto costs no runtime check anywhere downstream. Every measured column is a TYPED ABSENCE where nothing measured it: `GraphKey` is absent for a field or slab subject rather than a zero every graphless press would share, `GpuDeltaMax` — the frozen `[04.4]` spelling, carried under the wire's own name so the `[Mapper]` projection stays mechanical — is absent for a single-lane press rather than a zero the parity gate reads as a perfect match, and `Aging` is absent for every program but the aged one rather than a zero-span census a gate reads as a one-cell ladder, exactly the forged-zero the corpus refuses on every tally, level, and receipt field; the declared-versus-visited pair on that census is what makes an over-quantized ladder and an unexercised cavity dimension legible from the receipt rather than from a second press; at the wire the interior lowers once, the `Option<double>` to the proto optional and the absent `GraphKey` to the empty string, mirroring `materialId`'s empty for an acquired set. The `Projection/benchmarks` parity workload is the surface that fills the delta by pressing both lanes over one plan and folding the per-channel maximum. `GraphKey` folds the COMPILED ORDER — each node's port id then its case name — because the frozen topological sort is what the bake evaluated, so a re-authored graph whose nodes reorder textually but compile to one order keys identically, and a graph whose evaluation order genuinely changed keys differently. `Texels` sums every level's own extent rather than multiplying the base by the level count, so texels-per-second is a throughput number rather than one inflated threefold by a full chain. `Planes` carries each channel's own `PlaneReceipt`, so the height solver's true relative residual reaches the benchmark rather than dying inside the fold — and `Residual` selects it DETERMINISTICALLY, the height channel first then roster order, where a hash-order enumeration handed a different channel's number per run; `Downgraded` names every channel whose paired mip policy fell back to the box floor, and `Faulted` every channel whose band kernel neutral-filled under a failure with its texel tally, so both quality decisions the press made silently become decisions the receipt reports. `IsValid` reads what the receipt alone can prove: `webgpu` beside a graph key or an aging census is invalid evidence, because the accelerator lane refuses both graph and ladder subjects at plan admission and a receipt contradicting that law was forged, and a census claiming more visited rungs than the plan declared or an inverted age span is a fabricated column rather than a measured one — the STRONGER Minted-authority gate lives where the set meets the wire, `interchange#TEXTURE_EGRESS` `TextureSetWire.Of` proving every press receipt content-authoritative, and this page names that owner rather than claiming a check the receipt's own columns cannot see.

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
// The ladder census an aged press exercised: the two DECLARED rung counts beside the two VISITED counts and the
// sampled age span. Declared-versus-visited is the whole point — the ratio names an over-quantized ladder and a
// visited cavity count of one names a dimension the fields never drove — so the pair travels together and neither
// half is derivable from the other.
public readonly record struct AgeCoverage(
    int AgeRungs, int CavityRungs, int AgeRungsVisited, int CavityRungsVisited, double AgeMin, double AgeMax);

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

- [GPU_GRAPH_LOWERING]-[OPEN]: a `Graph` subject lowers to a KERNEL CHAIN — one field kernel per procedural `Texture` node, one `mathFold` per `Math` node, one `mixFold` per `Mix` node, dispatched in the compiled order over ping-ponged storage buffers — which needs a per-node buffer allocator and a live-range analysis the current arm does not carry; the deterministic floor is the CPU lane, and both the plan admission and `Lower` refuse until the chain lands. The two LADDER subjects sit outside the question: each compiles CPU-admitted cells a GPU arm rebuilds only at `f32`, forking the key the veto holds.
