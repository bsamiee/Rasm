# [MATERIALS_PRESS]

THE BAKE ENGINE. One `TexturePress.Press` fold drives a `PressSubject` — a compiled `graph#MATERIAL_GRAPH` node program, a raw `texture#TEXTURE_UV` procedural field, or a `surface#OPENPBR_SLAB` parameter vector under a spatially-varying aging trajectory — across a `PressPlan`'s texel grid and mints a `set#TEXTURE_SET` `TextureSet` of content-keyed `plane#TEXTURE_PLANE` pyramids. The three subjects compile ONCE into one `PressProgram`, and the row kernel dispatches on that program per BAND rather than per texel, so a plane costs one dispatch per partition and one allocation-free pass per row. Batching rides `graph#MATERIAL_GRAPH` `CompiledGraph.ShadeSpan`: the port environment resolves ONCE into an index-addressed scratch whose slot order IS the frozen compiled sort, so a plane never rebuilds an immutable map per node per texel — the difference between minutes and days at four thousand square. The per-point `Shade` rail is untouched, the band fold rides `ParallelHelper.For` over a seeded struct `IAction`, and every per-texel jitter derives from the TEXEL COORDINATE and the plan seed rather than from a sequential stream, so a band partition can never reorder a draw and a re-press at one seed is byte-identical at any processor count.

Persisted plane bytes are ALWAYS CPU-minted. The `PressBackend.WebGpu` row is an accelerator and preview lane whose output is never content-addressed, and that is a STRUCTURAL veto rather than an empirical tolerance: `PressProduct` gives the GPU arm a `Preview` case carrying planes and a receipt but NO `TextureSet`, so a GPU-keyed set has no spelling and cannot be persisted, wired, or addressed by accident. GPU `f32` cannot reproduce the CPU `f64` procedural lattice, so a GPU-keyed plane forks the content key at its own preimage; the divergence the parity workload measures rides `PressReceipt.GpuDelta` as TELEMETRY and never enters a key. Every measured column is a TYPED ABSENCE when nothing measured it, because a fabricated zero and an unmeasured pass are the two states a gate exists to separate — the graph key of a graphless subject, the parity delta of a single-lane press, and the wall time of an unclocked run all read absence rather than zero. The page composes `set#TEXTURE_SET` for the produced bundle, its channel roster, and each slot's `Read` projection, `plane#TEXTURE_PLANE` for the arena and the pyramid, `filter#PLANE_OP` for every post chain, every derived channel, and its `PlaneReceipt` evidence, `tile#TILE_SYNTH` for the in-fold tiling a plan requests, `gpu#PRESS_DEVICE` for the accelerator arm, `weathering#WEATHERING` for the aging ladder, the kernel `Deterministic` splitmix64 draw, `ContentHash` identity, and `ValidityClaim` receipt fold, `TimeProvider` for the one measured wall time, and CommunityToolkit.HighPerformance for every pooled arena and partitioned band — reminting no evaluator, no arena, no random source, no clock, and no identity.

## [01]-[INDEX]

- [02]-[PRESS_PLAN]: the `PressBackend` axis, the `PressSubject` union, the `ChannelBinding` row, the `PressPlan` record with its canonical plan key, and the binding-order law that seats derived channels after their sources and paired channels after their companions.
- [03]-[TEXTURE_PRESS]: the one `TexturePress.Press` entry, the `PressProgram` compiled subject, the `AgeLadder` quantized aged-vector table, the `PressRows` band kernel over `ParallelHelper.For`, the coordinate-keyed jitter law, the paired mip resolution, the GPU lowering gate, and the post-fold and tiling composition.
- [04]-[PRESS_RECEIPT]: the `PressProduct` union that makes the content-identity veto structural, and the `PressReceipt` evidence row with its per-channel plane receipts and typed absences.

## [02]-[PRESS_PLAN]

- Owner: `PressPlan` the bake request; `PressSubject` `[Union]` the thing being baked; `ChannelBinding` the per-channel request row; `PressBackend` `[SmartEnum<string>]` the execution lane.
- Cases: subject {`Graph` (a `MaterialGraph` with the parameter row and conductor its sink resolves against), `Source` (one `TextureSource` sampled through a `SamplerState` into one channel), `Slab` (a `MaterialParameters` row lowered to the OpenPBR vector, aged per texel by a `TextureSource` age field)} · backend {`cpu` (content-authoritative), `webgpu` (accelerator, never content-authoritative)}.
- Law: binding ORDER is derived, never authored — `Of` sorts bindings by `TextureChannel.Origin` depth, then by pair dependency, then by `TextureChannel.Ordinal`, so every `Shaded` and `Geometric` channel seats before any `Derived` one, a normal seats before the roughness whose mip fold consumes its variance, and a plan requesting `occlusion` without `height` produces `height` as an intermediate rather than refusing. A caller never sequences the fold.
- Entry: `public static Fin<PressPlan> Of(PressPlanDraft draft, PressSubject subject, Op key)` is the ONE plan admission — extent, layer law, binding uniqueness, pack membership, format width, subject arity, tile-guide coverage, and backend lowerability all gate here so the bake fold itself carries no re-check; the subject enters the admission because arity and lowerability are facts of the SUBJECT against the bindings, and deferring them to dispatch means a caller learns the veto after renting a device. `PlanKey` is the canonical content key the receipt records and the cache keys on.
- Packages: `set#TEXTURE_SET` (composed — `TextureChannel`/`ChannelPack`/`LayerLaw`/`TextureSet`/`SinkSlot`), `plane#TEXTURE_PLANE` (composed — `PlaneFormat`/`MipPolicy`/`AlphaMode`), `filter#PLANE_OP` (composed — `PlaneOp` the post chain), `tile#TILE_SYNTH` (composed — `TilePolicy` the in-fold tiling request), `Rasm.Materials.Appearance.Graph` (composed — `MaterialGraph`/`MaterialParameters`), `Rasm.Materials.Appearance.Surface` (composed — `ConductorMetal`), `Rasm.Materials.Appearance.Texture` (composed — `TextureSource`/`SamplerState`), `Rasm.Materials.Appearance.Weathering` (composed — `WeatheringDose`/`AgeParameter`), `Rasm.Element.Composition` (the SEAM `MaterialId`), `Rasm` (project — `ContentHash.Of` the one identity entry, `Dimension`, `Op`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new bake subject is one `PressSubject` case with its `PressProgram` arm; a new execution lane is one `PressBackend` row carrying its authority column; a new per-channel request knob is one `ChannelBinding` column. There is NO `BakeGraph`/`BakeField`/`BakeSlab` family — the subject's own case discriminates, and a caller holding a subject calls one entry.
- Boundary: `PressBackend` carries `ContentAuthoritative` as a ROW COLUMN rather than as a caller flag, so the content-identity law is data the plan admission reads and `[04]-[PRESS_RECEIPT]` enforces at the type level. Mip policy is a per-binding `Option<MipPolicy>` defaulting to the channel row's own law and spelling `MipPolicy.None` for a single-level plane — a plan-level `Mips` boolean beside a per-binding override is one knob selecting between two bodies, and the row already carries the answer. The plan key is `ContentHash.Of` over the plan's canonical bytes — extent, layer law, the ordered binding rows with their channels, formats, resolved mip policies and pack keys, the backend key, the seed, the alpha mode, the height scale, the age-ladder rung count, and the tile policy — and it EXCLUDES the material id and the conductor, which name the subject rather than the bake, so two materials pressed under one plan share a plan key and the receipt separates them by graph key. `Layers` and `LayerLaw` ride the plan so a cube map, a flipbook, and a volume are one bake shape at different rows; a UDIM set is N plans sharing a key, never one plan carrying a tile list, because a UDIM tile is an independent extent whose planes address independently. A binding naming a channel already inside a requested pack REFUSES at admission — the pack owns those slots and a standalone duplicate keys the set twice for one field — and a binding whose `Format` carries fewer components than its channel declares refuses for the same reason `set#TEXTURE_SET` refuses it later: a three-component normal in a two-component plane is a reconstruction the sampler cannot invert without evidence the plane does not carry. A `Source` subject binds EXACTLY ONE channel, because a procedural field has one value and a second bound channel would silently receive its neutral; a `Tile` policy whose guide channel no binding produces refuses, because the synthesizer would rail on a set that admitted cleanly. The `webgpu` backend gates LOWERABILITY at admission, not at dispatch: a `Source` subject over an `Image` or `Triplanar` case, or a `Graph` or `Slab` subject, has no kernel row on `gpu#WGSL_KERNEL` and refuses with the offending case named, so a caller learns the veto before renting a device rather than after.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;                       // CultureInfo (the canonical plan projection)
using System.Text.Unicode;                        // Utf8.TryWrite (the canonical plan projection)
using CommunityToolkit.HighPerformance;           // Memory2D/Span2D/ReadOnlySpan2D — the staging plane views
using CommunityToolkit.HighPerformance.Buffers;   // MemoryOwner/SpanOwner — the pooled arenas the band fold rents
using CommunityToolkit.HighPerformance.Helpers;   // ParallelHelper, IAction — the allocation-free band partition
using LanguageExt;                                // Seq, Option, Fin, HashMap
using Rasm.Domain;                                // Op, ContentHash, Deterministic, ValidityClaim, IValidityEvidence
using Rasm.Element.Composition;                   // MaterialId — the SEAM identity
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault (band 2450)
using Rasm.Materials.Appearance.Graph;            // MaterialGraph, CompiledGraph, MaterialParameters, ShadePoint, SurfaceShade, PortValue, GraphContext
using Rasm.Materials.Appearance.Surface;          // OpenPbrSurface, ConductorMetal
using Rasm.Materials.Appearance.Texture;          // TextureSource, TextureUv, UvSample, SamplerState, ShadeVec4
using Rasm.Materials.Appearance.Weathering;       // Weathering, WeatheringDose, AgeParameter
using Rasm.Numerics;                              // Dimension, UnitInterval
using Rhino.Geometry;                             // Point3d, Vector3d — the shade point's host geometry edge
using Thinktecture;
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

// Graph varies the five sink columns per texel and carries every other channel from its constant row; Slab
// varies the WHOLE OpenPBR vector through the aging trajectory an age FIELD samples per texel; Source bakes one
// procedural field into one channel. The three are distinct evaluation shapes, not three names for one — and
// the age field is a TextureSource rather than a channel because nothing is landed when the ladder is read.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PressSubject {
    private PressSubject() { }
    public sealed record Graph(MaterialGraph Program, MaterialParameters Row, ConductorMetal Conductor) : PressSubject;
    public sealed record Source(TextureSource Field, SamplerState Sampler, TextureChannel Target) : PressSubject;
    public sealed record Slab(MaterialParameters Row, ConductorMetal Conductor, Seq<WeatheringDose> Aging, Option<TextureSource> AgeField, SamplerState Sampler) : PressSubject;
}

// --- [MODELS] ------------------------------------------------------------------------------
// Post is the filter#PLANE_OP chain applied to this channel's plane after the shade fold and before the mip
// chain builds — a level, a remap, a blur — so a post-processed plane still carries a coherent pyramid. Mip is
// a typed override of the channel row's own law: absence takes the row, MipPolicy.None spells a flat plane.
public sealed record ChannelBinding(TextureChannel Channel, PlaneFormat Format, Option<MipPolicy> Mip, Option<ChannelPack> Pack, Seq<PlaneOp> Post) {
    public MipPolicy Policy => Mip.IfNone(() => Channel.Mip);
}

public sealed record PressPlanDraft(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law, Seq<ChannelBinding> Bindings,
    PressBackend Backend, ulong Seed, AlphaMode Alpha, double HeightScaleMm, int AgeRungs,
    Option<TilePolicy> Tile, Option<MaterialId> Material, Option<ConductorMetal> Conductor);

public sealed record PressPlan(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law, Seq<ChannelBinding> Bindings,
    PressBackend Backend, ulong Seed, AlphaMode Alpha, double HeightScaleMm, int AgeRungs,
    Option<TilePolicy> Tile, Option<MaterialId> Material, Option<ConductorMetal> Conductor, UInt128 PlanKey) {

    // The ONE plan admission. Binding order is DERIVED here — sources before derivations, normals before the
    // roughness rows whose mip fold consumes their variance — so the fold never re-sorts and a caller never
    // sequences. Subject arity gates here too, so a Source plan cannot silently neutral-fill a second channel.
    public static Fin<PressPlan> Of(PressPlanDraft draft, PressSubject subject, Op key) =>
        from _ in guard(!draft.Bindings.IsEmpty, MaterialFault.Parameter(key, "<press-plan-no-bindings>"))
        from __ in guard(draft.Law.Admits(draft.Layers.Value), MaterialFault.Parameter(key, $"<layer-law-rejects:{draft.Law.Key}:{draft.Layers.Value}>"))
        from ___ in guard(draft.Bindings.Map(static b => b.Channel).Distinct().Count() == draft.Bindings.Count, MaterialFault.Parameter(key, "<press-binding-duplicate-channel>"))
        from ____ in guard(draft.AgeRungs >= 2, MaterialFault.Parameter(key, $"<age-ladder-degenerate:{draft.AgeRungs}>"))
        from _____ in AdmitSubject(draft, subject, key)
        from ______ in draft.Tile.Match(
            Some: policy => guard(draft.Bindings.Exists(b => b.Channel == policy.Guide), MaterialFault.Parameter(key, $"<tile-guide-unbound:{policy.Guide.Key}>")),
            None: () => Fin.Succ(unit))
        from _______ in draft.Bindings.Fold(Fin.Succ(unit), (acc, b) => acc.Bind(_ => AdmitBinding(draft, b, key)))
        from ________ in guard(draft.Backend.ContentAuthoritative || Lowerable(subject), MaterialFault.Parameter(key, $"<gpu-unlowerable-subject:{subject.GetType().Name}>"))
        let ordered = Order(draft.Bindings)
        select new PressPlan(draft.Width, draft.Height, draft.Layers, draft.Law, ordered, draft.Backend,
            draft.Seed, draft.Alpha, draft.HeightScaleMm, draft.AgeRungs, draft.Tile, draft.Material, draft.Conductor,
            Mint(draft, ordered));

    // A procedural field has ONE value: a second bound channel would receive its neutral and read as baked.
    static Fin<Unit> AdmitSubject(PressPlanDraft draft, PressSubject subject, Op key) =>
        subject.Switch(
            state:  (Draft: draft, Key: key),
            graph:  static (s, _) => Fin.Succ(unit),
            slab:   static (s, _) => Fin.Succ(unit),
            source: static (s, f) => s.Draft.Bindings.Count is 1 && s.Draft.Bindings[0].Channel == f.Target
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(MaterialFault.Parameter(s.Key, $"<source-subject-binds-one-channel:{f.Target.Key}>")));

    static Fin<Unit> AdmitBinding(PressPlanDraft draft, ChannelBinding binding, Op key) =>
        from _ in guard(binding.Format.Components >= binding.Channel.Components, MaterialFault.Parameter(key, $"<binding-format-narrow:{binding.Channel.Key}>"))
        from __ in guard(binding.Pack.Match(Some: p => p.Slots.Contains(binding.Channel), None: static () => true), MaterialFault.Parameter(key, $"<binding-pack-foreign:{binding.Channel.Key}>"))
        from ___ in guard(binding.Pack.IsSome || !draft.Bindings.Exists(other => other.Pack.Match(Some: p => p.Slots.Contains(binding.Channel), None: static () => false)),
                          MaterialFault.Parameter(key, $"<binding-both-packed-and-standalone:{binding.Channel.Key}>"))
        select unit;

    // Only a procedural Source over a lowerable field row reaches the accelerator; the graph and slab arms have
    // no kernel chain, so the veto is stated once here rather than discovered after a device is rented.
    static bool Lowerable(PressSubject subject) =>
        subject is PressSubject.Source { Field: TextureSource.Noise or TextureSource.Checker or TextureSource.Gradient };

    // Three ordering keys, one sort: derivation depth (a source before what folds from it), pair dependency (a
    // normal before the roughness whose variance fold reads it), then the roster ordinal for determinism.
    static Seq<ChannelBinding> Order(Seq<ChannelBinding> bindings) =>
        toSeq(bindings
            .OrderBy(static b => Depth(b.Channel))
            .ThenBy(static b => b.Channel.Pair.IsSome)
            .ThenBy(static b => b.Channel.Ordinal));

    static int Depth(TextureChannel channel) =>
        channel.Origin switch {
            ChannelOrigin.Derived derived => TextureChannel.TryGet(derived.From, out TextureChannel? from) ? Depth(from) + 1 : 1,
            _ => 0,
        };

    // The plan key names the BAKE, never the subject: material id and conductor are excluded so two materials
    // pressed under one plan share a plan key, and the receipt's graph key is what separates them. The fold
    // rides the ONE kernel identity entry and writes its canonical text into a stack buffer per row.
    static UInt128 Mint(PressPlanDraft draft, Seq<ChannelBinding> ordered) =>
        ContentHash.Of((Draft: draft, Ordered: ordered), static (source, digest) => {
            Span<byte> slot = stackalloc byte[192];
            _ = Utf8.TryWrite(slot, CultureInfo.InvariantCulture,
                $"{source.Draft.Width.Value}x{source.Draft.Height.Value}x{source.Draft.Layers.Value}|{source.Draft.Law.Key}|{source.Draft.Backend.Key}|{source.Draft.Seed:x16}|{source.Draft.Alpha.Key}|{source.Draft.HeightScaleMm:R}|{source.Draft.AgeRungs}",
                out int header);
            digest.Append(slot[..header]);
            foreach (ChannelBinding binding in source.Ordered) {
                _ = Utf8.TryWrite(slot, CultureInfo.InvariantCulture,
                    $"{binding.Channel.Key}|{binding.Format.Key}|{binding.Policy.Key}|{binding.Pack.Match(Some: static p => p.Key, None: static () => string.Empty)}|{binding.Post.Count}",
                    out int row);
                digest.Append(slot[..row]);
            }
            source.Draft.Tile.Iter(policy => {
                _ = Utf8.TryWrite(slot, CultureInfo.InvariantCulture, $"{policy.Strategy.Key}|{policy.Guide.Key}|{policy.Overlap}|{policy.Seed:x16}|{policy.WangColors}", out int tile);
                digest.Append(slot[..tile]);
            });
        });
}
```

## [03]-[TEXTURE_PRESS]

- Owner: `TexturePress` the bake fold; `PressProgram` `[Union]` the compiled subject; `AgeLadder` the quantized aged-vector table the `Slab` program reads; `PressRows` the struct `IAction` band partition.
- Entry: `public static Fin<PressProduct> Press(PressSubject subject, PressPlan plan, Op key, TimeProvider? clock = null)` is the ONE bake — it compiles the subject once, folds every binding, applies each binding's post chain, derives every derived channel from its landed source through the channel's OWN declared fold, builds the mip chains against their paired companions, tiles when the plan requests it, and admits the result through `set#TEXTURE_SET` `TextureSet.Of`; the caller composes a `PressProduct` and never orchestrates a stage.
- Packages: `graph#MATERIAL_GRAPH` (composed — `MaterialGraph.Compile` ONCE per press, `CompiledGraph.ShadeSpan` the batched evaluator, `CompiledGraph.ScratchWidth` the per-band scratch the fold rents against, `ShadePoint`, `GraphContext.Tolerant`), CommunityToolkit.HighPerformance (`ParallelHelper.For<TAction>(int, int, in TAction)` over a SEEDED `struct IAction` so the partition allocates nothing, inlines, clamps to the processor count, and carries its state — the unseeded overload default-constructs the action and would lose every field the fold needs; `MemoryOwner<T>.Allocate` the per-binding staging arena, `SpanOwner<T>.Allocate` the per-band point/scratch/shade rentals, `Memory2D<T>`/`Span2D<T>` the plane views), `Rasm` (project — `Deterministic.UnitInterval(Point3d, int, int)` the coordinate-keyed per-texel draw), `set#TEXTURE_SET` (composed — `SinkSlot.Read` the per-slot `SurfaceShade` column reader, `ChannelOrigin` the per-channel production law), `filter#PLANE_OP` (composed — `PlaneOp.Apply(TexturePlane, Seq<PlaneOp>, Op, TimeProvider?)` and its `PlaneReceipt`, for every post chain and every derived channel), `tile#TILE_SYNTH` (composed — `TileSynth.Tileify` when the plan carries a policy), `gpu#PRESS_DEVICE` (composed — `PressDevice.Acquire`/`Dispatch` on the accelerator arm), `Rasm.Materials.Appearance.Weathering` (composed — `Weathering.Apply` at each rung of the age ladder).
- Growth: a new evaluation shape is one `PressSubject` case and one `PressProgram` arm; a new post-processing step is one `filter#PLANE_OP` `PlaneOp` on a binding's chain; a new derived channel is one `ChannelOrigin.Derived` row on `set#TEXTURE_CHANNEL` carrying its own fold — the press discovers both the dependency and the operation from the roster and needs no edit.
- Boundary: the subject compiles ONCE into a `PressProgram` — a graph resolves its topological order into a frozen `CompiledGraph` and its constant OpenPBR vector, a slab builds its whole age ladder, a field captures its sampler — and the BAND kernel dispatches on that program once per partition rather than once per texel, so a four-thousand-square plane pays four dispatches per core instead of sixteen million. `ShadeSpan` re-enters over the compiled order with the port environment resolved into an INDEX-ADDRESSED scratch whose slot order is that sort, so a plane costs one allocation-free pass per row instead of one immutable-map rebuild per node per texel; the per-point `Shade` rail is untouched and the integrator keeps it. Band parallelism rides `ParallelHelper.For` over a SEEDED `struct IAction`: the seeded overload copies the caller's action into each partition, where the unseeded overload default-constructs it and would hand every band an empty program, an empty plan, and a null target. Each band rents its point, scratch, and shade spans ONCE from `SpanOwner<T>` and walks its own rows, so a partition rents once where a per-row action rents per row. Per-texel jitter derives from the TEXEL COORDINATE and the plan seed through `Deterministic.UnitInterval` with the CHANNEL's roster ordinal as the salt — never from a sequential stream — so a band partition cannot reorder a draw, two channels of one press do not share a jitter sequence, a re-press at one seed is byte-identical regardless of processor count, and the receipt's seed genuinely replays the plane. The `Graph` program varies the FIVE sink columns per texel through each slot's own `SinkSlot.Read` projection and reads every other bound channel off its constant lowered vector — the `graph#MATERIAL_GRAPH` `BsdfOutput` sink carries five columns, so a coat-roughness plane pressed from a graph subject is honestly constant rather than silently mis-projected from base colour. The `Slab` program reads a QUANTIZED AGE LADDER: `Weathering.Apply` runs once per rung across the plan's own `AgeRungs` quantization and every texel indexes the rung its age field sampled, so a spatially-aged bake costs `AgeRungs` fallible parameter admissions rather than one per texel, and the rung count is a declared plan column entering the plan key rather than a hidden approximation — a caller needing continuous aging presses at a finer ladder, which is one column, not a different fold. Staging is `ShadeVec4` and quantization is `plane#TEXTURE_PLANE`'s: the fold writes decoded four-lane texels into a `Memory2D<ShadeVec4>` arena and the plane's own row `Write` rail associates alpha, encodes the transfer, and narrows to the binding's `PlaneFormat`, so exactly one quantizer exists in the corpus and a press never encodes a texel itself. Derived channels fold AFTER their sources land, through the channel row's OWN `ChannelOrigin.Derived.Fold` step composed BEFORE the binding's post chain — `height` from `geometry_normal` by the spectral height inversion, `occlusion` and `curvature` from `height` — so the derivation is roster data rather than a caller-supplied post chain the press hopes contains the right operation, and a source channel a plan did not request is produced as an intermediate and then dropped unless bound. A PAIRED mip policy resolves its companion from the landed map through `TextureChannel.Pair`; where the plan bound no companion the policy DOWNGRADES to `MipPolicy.Box` and the receipt records the channel by name, because the alternative — passing an unpaired paired policy to `TexturePyramid.Of` — refuses the whole press for a quality floor the corpus already declares acceptable. Tiling is IN-FOLD when the plan carries a policy: `tile#TILE_SYNTH` `TileSynth.Tileify` runs over the admitted set so every channel takes ONE plan and the resulting set re-keys, and the set's `Tiled` proof is the gate's own mint rather than the plan's request. The GPU arm dispatches per binding against a device acquired once for the whole press and released at the fold's close, runs no post chain and no derivation, and its product is a `Preview` — no set, no key, nothing addressable — so the content-identity law needs no runtime check anywhere downstream. Wall time rides the injected `TimeProvider` and every per-channel `PlaneReceipt` rides the same clock, so the press receipt carries measured elapsed and the height solver's true residual rather than a literal zero. The `[EXPRESSION_SPINE]` exemptions are the `PressRows.Invoke` band kernel, the `AgeLadder` build, and the `Fill` staging write: fixed-extent numeric folds over caller-owned buffers; every admission, dispatch, and egress surface is expression-bodied.

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
    public sealed record Aged(AgeLadder Ladder, Option<TextureSource> AgeField, SamplerState Sampler) : PressProgram;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The quantized aging trajectory: Weathering.Apply is fallible and runs once per RUNG, never once per texel, so
// a spatially-aged 4k bake costs AgeRungs admissions instead of sixteen million. Rung count is a plan column
// entering the plan key, so a finer ladder is a re-key rather than a silent quality change.
public sealed record AgeLadder(Seq<OpenPbrSurface> Rungs) {
    public static Fin<AgeLadder> Of(MaterialParameters row, ConductorMetal conductor, Seq<WeatheringDose> aging, int rungs, Op key) =>
        toSeq(Enumerable.Range(0, rungs))
            .Fold(Fin.Succ(Seq<OpenPbrSurface>()), (acc, rung) =>
                acc.Bind(built => Weathering.Apply(row, aging, AgeParameter.Create(rung / (double)(rungs - 1)), key)
                    .Map(aged => built.Add(OpenPbrSurface.Of(aged, conductor)))))
            .Map(static built => new AgeLadder(built));

    public OpenPbrSurface At(double age) => Rungs[Math.Clamp((int)(age * (Rungs.Count - 1) + 0.5), 0, Rungs.Count - 1)];
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class TexturePress {
    // Bands sized so one rent serves a partition rather than a row — the same grain filter#PLANE_FOLD takes,
    // because both fold the same planes through the same rail and a second sizing law would diverge.
    const int BandFloor = 16;

    public static Fin<PressProduct> Press(PressSubject subject, PressPlan plan, Op key, TimeProvider? clock = null) {
        TimeProvider ticks = clock ?? TimeProvider.System;
        long opened = ticks.GetTimestamp();
        return plan.Backend.ContentAuthoritative
            ? Mint(subject, plan, key, ticks, opened)
            : Accelerate(subject, plan, key, ticks, opened);
    }

    // The CPU lane: compile once, fold every non-derived binding, post-process, derive, mip against pairs, tile,
    // admit. The set is minted LAST so a failed stage never leaves a half-keyed bundle behind.
    static Fin<PressProduct> Mint(PressSubject subject, PressPlan plan, Op key, TimeProvider ticks, long opened) =>
        from program in Compile(subject, plan, key)
        from staged in plan.Bindings.Filter(static b => b.Channel.Origin is not ChannelOrigin.Derived)
            .Fold(Fin.Succ(HashMap<TextureChannel, Memory2D<ShadeVec4>>.Empty), (acc, binding) =>
                acc.Map(map => map.Add(binding.Channel, Fold(program, plan, binding, key))))
        from posted in staged.Fold(Fin.Succ((Planes: HashMap<TextureChannel, TexturePyramid>.Empty, Evidence: HashMap<TextureChannel, PlaneReceipt>.Empty, Downgraded: Seq<TextureChannel>())), (acc, pair) =>
            acc.Bind(carried => Finish(plan, pair.Key, pair.Value, carried.Planes, key, ticks).Map(built => (
                Planes: carried.Planes.Add(pair.Key, built.Pyramid),
                Evidence: carried.Evidence.Add(pair.Key, built.Receipt),
                Downgraded: built.Downgraded ? carried.Downgraded.Add(pair.Key) : carried.Downgraded))))
        from derived in plan.Bindings.Filter(static b => b.Channel.Origin is ChannelOrigin.Derived)
            .Fold(Fin.Succ(posted), (acc, binding) => acc.Bind(carried => Derive(plan, binding, carried, key, ticks)))
        from set in TextureSet.Of(new TextureSetDraft(plan.Width, plan.Height, plan.Layers, plan.Law,
            NormalConvention.Gl, plan.Alpha, plan.HeightScaleMm, Option<TileProof>.None, Seq<UdimTile>(),
            derived.Planes.Filter((c, _) => plan.Bindings.Exists(b => b.Channel == c)), Seq<ChannelPackPlane>(),
            plan.Conductor, plan.Material), key)
        from tiled in plan.Tile.Match(
            Some: policy => TileSynth.Tileify(set, policy, key, ticks).Map(static pair => pair.Set),
            None: () => Fin.Succ(set))
        select (PressProduct)new PressProduct.Minted(tiled, new PressReceipt(plan.Backend, plan.PlanKey,
            GraphKey(program), plan.Seed, Texels(tiled), ticks.GetElapsedTime(opened).TotalMilliseconds,
            derived.Evidence, derived.Downgraded, GpuDelta: Option<double>.None));

    // The GPU lane returns a Preview: planes and a receipt, NO TextureSet, therefore NO key. The
    // content-identity veto is the union's shape, not a runtime check any consumer could skip. It runs no post
    // chain and no derivation — the accelerator lowers field kernels alone, which is exactly what the plan
    // admission already proved before a device was rented.
    static Fin<PressProduct> Accelerate(PressSubject subject, PressPlan plan, Op key, TimeProvider ticks, long opened) =>
        from lease in PressDevice.Acquire(DevicePolicy.Default, key)
        from planes in lease.Use((Subject: subject, Plan: plan, Key: key), static (state, device) =>
            state.Plan.Bindings.Fold(Fin.Succ(HashMap<TextureChannel, TexturePyramid>.Empty), (acc, binding) =>
                acc.Bind(rows => Lower(state.Subject, binding, state.Key)
                    .Bind(kernel => device.Dispatch(kernel, Stage(state.Plan, kernel, state.Subject), state.Key))
                    .Bind(receipt => Lift(state.Plan, binding, receipt, state.Key))
                    .Map(plane => rows.Add(binding.Channel, plane)))))
        select (PressProduct)new PressProduct.Preview(planes, new PressReceipt(plan.Backend, plan.PlanKey,
            Option<UInt128>.None, plan.Seed, Texels(plan, planes), ticks.GetElapsedTime(opened).TotalMilliseconds,
            HashMap<TextureChannel, PlaneReceipt>.Empty, Seq<TextureChannel>(), GpuDelta: Option<double>.None));

    // One compile per press. The graph arm freezes its order AND its constant lowered vector, so an unslotted
    // channel reads a real OpenPBR column rather than a re-lowering per texel; the slab arm builds its whole
    // ladder here, so every rung's fallible admission is paid once.
    static Fin<PressProgram> Compile(PressSubject subject, PressPlan plan, Op key) =>
        subject.Switch(
            state:  (Plan: plan, Key: key),
            graph:  static (s, g) => g.Program.Compile(s.Key).Map(compiled => (PressProgram)new PressProgram.Shaded(compiled, g.Row, OpenPbrSurface.Of(g.Row, g.Conductor))),
            source: static (_, f) => Fin.Succ<PressProgram>(new PressProgram.Field(f.Field, f.Sampler, f.Target)),
            slab:   static (s, b) => AgeLadder.Of(b.Row, b.Conductor, b.Aging, s.Plan.AgeRungs, s.Key).Map(ladder => (PressProgram)new PressProgram.Aged(ladder, b.AgeField, b.Sampler)));

    // The band fold. ParallelHelper.For over a SEEDED struct IAction allocates nothing, inlines, clamps to the
    // processor count, and invokes inline for a single partition; the unseeded overload default-constructs the
    // action and would hand every band an empty program. The arena is one pooled MemoryOwner per binding.
    static Memory2D<ShadeVec4> Fold(PressProgram program, PressPlan plan, ChannelBinding binding, Op key) {
        int rows = plan.Height.Value * plan.Layers.Value;
        MemoryOwner<ShadeVec4> arena = MemoryOwner<ShadeVec4>.Allocate(plan.Width.Value * rows, AllocationMode.Default);
        Memory2D<ShadeVec4> target = arena.Memory.AsMemory2D(rows, plan.Width.Value);
        int band = Math.Max(BandFloor, rows / (Environment.ProcessorCount * 4));
        ParallelHelper.For(0, (rows + band - 1) / band, in new PressRows(program, plan, binding.Channel, target, band, rows, key));
        return target;
    }

    // Staging crosses into the plane substrate ONCE, through the plane's own row Write rail: alpha
    // association, transfer encode, and depth narrowing all happen at their owner, so the press encodes no
    // texel itself. The post chain then runs as ONE filter#PLANE_OP Apply over the admitted plane, whose
    // PlaneReceipt reaches the press receipt with the height solver's residual intact. A paired mip policy
    // resolves its companion from the landed map, downgrading to Box (the declared quality floor) and naming
    // the channel when the plan bound none — refusing the whole press for a floor is the deleted response.
    static Fin<(TexturePyramid Pyramid, PlaneReceipt Receipt, bool Downgraded)> Finish(
        PressPlan plan, TextureChannel channel, Memory2D<ShadeVec4> staging, HashMap<TextureChannel, TexturePyramid> landed, Op key, TimeProvider ticks) =>
        from binding in plan.Bindings.Find(b => b.Channel == channel).ToFin(MaterialFault.Parameter(key, $"<press-binding-lost:{channel.Key}>"))
        from blank in TexturePlane.Of(binding.Format, plan.Width, plan.Height, channel.Transfer, plan.Alpha, key, Some(plan.Layers))
        let filled = Fill(blank, staging)
        from posted in PlaneOp.Apply(filled, binding.Post, key, ticks)
        let paired = Companion(channel, landed)
        let policy = binding.Policy.Paired && paired.IsNone ? MipPolicy.Box : binding.Policy
        from chain in TexturePyramid.Of(posted.Plane, policy, key, paired)
        select (chain, posted.Receipt, policy != binding.Policy);

    static Option<TexturePyramid> Companion(TextureChannel channel, HashMap<TextureChannel, TexturePyramid> landed) =>
        channel.Pair.Bind(name => TextureChannel.TryGet(name, out TextureChannel? row) ? landed.Find(row) : Option<TexturePyramid>.None);

    // Row-wise write through the plane's OWN rail over EVERY layer; scratch and staging are caller-owned per
    // call, never plane-held, so a parallel band fold never serializes on a shared buffer.
    static TexturePlane Fill(TexturePlane plane, Memory2D<ShadeVec4> texels) {
        using SpanOwner<float> scratch = SpanOwner<float>.Allocate(plane.RowScalars);
        using SpanOwner<ShadeVec4> staging = SpanOwner<ShadeVec4>.Allocate(plane.Width.Value);
        ReadOnlySpan2D<ShadeVec4> source = texels.Span;
        for (int layer = 0; layer < plane.Layers.Value; layer++) {
            for (int row = 0; row < plane.Height.Value; row++) { plane.Write(layer, row, scratch.Span, source.GetRowSpan((layer * plane.Height.Value) + row), staging.Span); }
        }
        return plane;
    }

    // A derived channel folds from its LANDED source plane through the ROSTER's own declared step, composed
    // before the caller's post chain — the press never re-derives a normal integration, an occlusion sweep, or
    // a curvature stencil, and a caller cannot omit the operation that makes the channel what it is.
    static Fin<(HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneReceipt> Evidence, Seq<TextureChannel> Downgraded)> Derive(
        PressPlan plan, ChannelBinding binding, (HashMap<TextureChannel, TexturePyramid> Planes, HashMap<TextureChannel, PlaneReceipt> Evidence, Seq<TextureChannel> Downgraded) carried, Op key, TimeProvider ticks) =>
        binding.Channel.Origin is ChannelOrigin.Derived derived && TextureChannel.TryGet(derived.From, out TextureChannel? from)
            ? from source in carried.Planes.Find(from).ToFin(MaterialFault.Parameter(key, $"<derived-source-absent:{binding.Channel.Key}:{derived.From}>"))
              from folded in PlaneOp.Apply(source.Base, derived.Fold.Cons(binding.Post), key, ticks)
              let paired = Companion(binding.Channel, carried.Planes)
              let policy = binding.Policy.Paired && paired.IsNone ? MipPolicy.Box : binding.Policy
              from chain in TexturePyramid.Of(folded.Plane, policy, key, paired)
              select (carried.Planes.Add(binding.Channel, chain),
                      carried.Evidence.Add(binding.Channel, folded.Receipt),
                      policy != binding.Policy ? carried.Downgraded.Add(binding.Channel) : carried.Downgraded)
            : Fin.Fail<(HashMap<TextureChannel, TexturePyramid>, HashMap<TextureChannel, PlaneReceipt>, Seq<TextureChannel>)>(
                MaterialFault.Parameter(key, $"<derived-origin-unresolved:{binding.Channel.Key}>"));

    // The GPU lowering gate mirrors the plan admission exactly, so a caller that passed admission cannot fail
    // here for a reason admission could have named.
    static Fin<WgslKernel> Lower(PressSubject subject, ChannelBinding binding, Op key) =>
        subject is PressSubject.Source field
            ? field.Field switch {
                TextureSource.Noise    => Fin.Succ(WgslKernel.NoiseField),
                TextureSource.Checker  => Fin.Succ(WgslKernel.CheckerField),
                TextureSource.Gradient => Fin.Succ(WgslKernel.GradientField),
                _ => Fin.Fail<WgslKernel>(RasterFault.Device(key, $"<gpu-unlowerable-source:{binding.Channel.Key}>")),
            }
            : Fin.Fail<WgslKernel>(RasterFault.Device(key, $"<gpu-unlowerable-subject:{binding.Channel.Key}>"));

    static KernelBinding Stage(PressPlan plan, WgslKernel kernel, PressSubject subject) { /* fill the row's KernelUniform block from the plan extent and the TextureSource's own parameters in the kernel's declared word order, seat the read buffers the row's Layout declares, size the write buffer at w*h*4 floats, and take the workgroup counts from WgslKernel.Groups(plan.Width, plan.Height, plan.Layers) — the [EXPRESSION_SPINE] uniform-write exemption. */ throw new NotImplementedException(); }

    static Fin<TexturePyramid> Lift(PressPlan plan, ChannelBinding binding, KernelReceipt receipt, Op key) { /* widen the f32 readback into a ShadeVec4 staging plane, then TexturePlane.Of + Fill + TexturePyramid.Of exactly as the CPU lane does — the preview shares the plane substrate, only its authority differs. */ throw new NotImplementedException(); }

    // The graph key is the COMPILED ORDER, absent for a graphless subject — a zero would read as a real key
    // shared by every field and slab press ever taken.
    static Option<UInt128> GraphKey(PressProgram program) =>
        program is PressProgram.Shaded shaded
            ? Some(ContentHash.Of(shaded.Graph, static (graph, digest) => {
                  Span<byte> slot = stackalloc byte[32];
                  graph.Order.Iter(node => {
                      _ = Utf8.TryWrite(slot, CultureInfo.InvariantCulture, $"{node.Id.Value}:{node.GetType().Name}", out int written);
                      digest.Append(slot[..written]);
                  });
              }))
            : Option<UInt128>.None;

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
// scratch, and shade spans rent once per band. Jitter reads the TEXEL COORDINATE with the CHANNEL's roster
// ordinal as its salt, so partitioning cannot reorder a draw, two channels never share a jitter sequence, and
// a re-press at one seed is byte-identical at any processor count.
internal readonly struct PressRows(PressProgram program, PressPlan plan, TextureChannel channel, Memory2D<ShadeVec4> target, int band, int rows, Op key) : IAction {
    public void Invoke(int slice) {
        int width = plan.Width.Value, height = plan.Height.Value;
        using SpanOwner<ShadePoint> points = SpanOwner<ShadePoint>.Allocate(width);
        using SpanOwner<ShadeVec4> write = SpanOwner<ShadeVec4>.Allocate(width);
        ShadePoint anchor = ShadePoint.Of(Point3d.Origin, Vector3d.ZAxis, Vector3d.ZAxis, Some(Vector3d.XAxis), 0.0, 0.0, GraphContext.Tolerant, key)
            .IfFail(static _ => default);   // the anchor admits ONCE per band; a degenerate frame is unreachable from the fixed axes
        Span2D<ShadeVec4> plane = target.Span;
        for (int offset = 0; offset < band; offset++) {
            int line = (slice * band) + offset;
            if (line >= rows) { return; }
            int y = line % height;
            for (int x = 0; x < width; x++) {
                double jitter = Deterministic.UnitInterval(new Point3d(x, y, line / height), salt: channel.Ordinal, seed: unchecked((int)plan.Seed));
                points.Span[x] = anchor with {
                    U = (x + jitter) / width,
                    V = (y + 0.5) / height,
                    Position = new Point3d((x + 0.5) / width, (y + 0.5) / height, line / (double)height),
                };
            }
            Row(points.Span, write.Span);
            for (int x = 0; x < width; x++) { plane[line, x] = write.Span[x]; }
        }
    }

    // One dispatch per band, never per texel: each arm owns its row loop and its own scratch shape.
    void Row(ReadOnlySpan<ShadePoint> points, Span<ShadeVec4> write) =>
        program.Switch(
            shaded: s => Shaded(s, points, write),
            field:  f => Field(f, points, write),
            aged:   a => Aged(a, points, write));

    // The five sink columns vary per texel through the slot's OWN SurfaceShade reader; every other bound
    // channel reads the constant lowered vector, so a coat-roughness plane from a graph subject is honestly
    // constant rather than the base-colour column wearing another channel's name.
    Unit Shaded(PressProgram.Shaded program, ReadOnlySpan<ShadePoint> points, Span<ShadeVec4> write) {
        using SpanOwner<PortValue> scratch = SpanOwner<PortValue>.Allocate(program.Graph.ScratchWidth);
        using SpanOwner<SurfaceShade> shades = SpanOwner<SurfaceShade>.Allocate(points.Length);
        return channel.Slot.Match(
            Some: slot => program.Graph.ShadeSpan(points, program.Row, scratch.Span, shades.Span, key)
                .Match(Succ: _ => { for (int x = 0; x < write.Length; x++) { write[x] = slot.Read(shades.Span[x]); } return unit; },
                       Fail: _ => { write.Fill(channel.Neutral); return unit; }),
            None: () => { ShadeVec4 constant = Constant(program.Constant, points[0]); write.Fill(constant); return unit; });
    }

    Unit Field(PressProgram.Field program, ReadOnlySpan<ShadePoint> points, Span<ShadeVec4> write) {
        for (int x = 0; x < write.Length; x++) {
            write[x] = TextureUv.Sample(program.Source, Sample(points[x], 0.0), program.Sampler, key).IfFail(_ => channel.Neutral);
        }
        return unit;
    }

    // The age field samples per texel and indexes the ladder's own rung; the channel's lens then reads that
    // rung's OpenPBR column, so a spatially-aged bake pays one admission per rung and none per texel.
    Unit Aged(PressProgram.Aged program, ReadOnlySpan<ShadePoint> points, Span<ShadeVec4> write) {
        for (int x = 0; x < write.Length; x++) {
            double age = program.AgeField.Match(
                Some: source => TextureUv.Sample(source, Sample(points[x], 0.0), program.Sampler, key).Map(static v => v.Luminance).IfFail(0.0),
                None: () => 1.0);
            write[x] = Constant(program.Ladder.At(Math.Clamp(age, 0.0, 1.0)), points[x]);
        }
        return unit;
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
- Receipt: `PressReceipt` carries the backend, the plan key, the graph key where one exists, the replay seed, the true shaded texel count, the measured wall time, the per-channel `filter#PLANE_OP` `PlaneReceipt` evidence, the channels whose paired mip policy downgraded, and the parity delta a two-lane run measured.
- Packages: `Rasm` (project — `IValidityEvidence`/`ValidityClaim` the corpus evidence floor), `set#TEXTURE_SET` (composed — `TextureSet`/`TextureChannel`), `plane#TEXTURE_PLANE` (composed — `TexturePyramid`), `filter#PLANE_OP` (composed — `PlaneReceipt`), LanguageExt.Core.
- Growth: a new receipt column is one field the fold already computes, because the plan key names the request and the receipt names the run; a new product class is a new case, and the two that exist partition the authority question.
- Boundary: the `Preview` case carries planes and a receipt and NOTHING addressable — no set, no key, no digest — so a GPU result cannot be persisted, wired, or content-addressed by accident, and the structural veto costs no runtime check anywhere downstream. Every measured column is a TYPED ABSENCE where nothing measured it: `GraphKey` is absent for a field or slab subject rather than a zero every graphless press would share, and `GpuDelta` is absent for a single-lane press rather than a zero the parity gate reads as a perfect match — which is exactly the forged-zero the corpus refuses on every tally, level, and receipt field. The `Projection/benchmarks` parity workload is the surface that fills the delta by pressing both lanes over one plan and folding the per-channel maximum. `GraphKey` folds the COMPILED ORDER — each node's port id then its case name — because the frozen topological sort is what the bake evaluated, so a re-authored graph whose nodes reorder textually but compile to one order keys identically, and a graph whose evaluation order genuinely changed keys differently. `Texels` sums every level's own extent rather than multiplying the base by the level count, so texels-per-second is a throughput number rather than one inflated threefold by a full chain. `Planes` carries each channel's own `PlaneReceipt`, so the height solver's true relative residual — the one correctness signal that survives preconditioning and cancellation — reaches the benchmark rather than dying inside the fold, and `Downgraded` names every channel whose paired mip policy fell back to the box floor, so a quality decision the press made silently becomes a quality decision the receipt reports. `IsValid` reads the authority invariant rather than restating the fields: a `Minted` product's receipt must name a content-authoritative backend, and a receipt whose backend is `webgpu` beside a minted set is invalid evidence the moment it is constructed — which is the assertion the union already makes unrepresentable and the receipt carries anyway, because a receipt crossing a wire arrives without its union.

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
// a single-lane press measured no divergence, and a zero in either reads to a gate as a real value. Texels
// sums each level's own extent, so texels-per-second is throughput rather than a headline.
public sealed record PressReceipt(
    PressBackend Backend, UInt128 PlanKey, Option<UInt128> GraphKey, ulong Seed, ulong Texels, double ElapsedMs,
    HashMap<TextureChannel, PlaneReceipt> Planes, Seq<TextureChannel> Downgraded, Option<double> GpuDelta)
    : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast((int)Math.Min(Texels, int.MaxValue), 1),
        ValidityClaim.Nonnegative(ElapsedMs),
        ValidityClaim.Of(GpuDelta.ForAll(static d => double.IsFinite(d) && d >= 0.0)),
        ValidityClaim.Of(Backend.ContentAuthoritative || GraphKey.IsNone));

    public bool ContentAuthoritative => Backend.ContentAuthoritative;
    public double TexelsPerSecond => ElapsedMs > 0.0 ? Texels / (ElapsedMs / 1000.0) : 0.0;
    public Option<double> Residual => Planes.Values.Choose(static receipt => receipt.Residual).HeadOrNone();
}
```

## [05]-[RESEARCH]

- [SHADESPAN_SIGNATURE]-[OPEN]: does `graph#MATERIAL_GRAPH` mint `CompiledGraph.ShadeSpan(ReadOnlySpan<ShadePoint>, MaterialParameters, Span<PortValue>, Span<SurfaceShade>, Op) -> Fin<Unit>` beside `int ScratchWidth => Order.Count`; verification route is the landed `graph.md` `[02]-[MATERIAL_GRAPH]` fence, and `PressRows.Shaded` binds whatever the owner declares.
- [GPU_GRAPH_LOWERING]-[OPEN]: a `Graph` subject lowers to a KERNEL CHAIN — one field kernel per procedural `Texture` node, one `mathFold` per `Math` node, one `mixFold` per `Mix` node, dispatched in the compiled order over ping-ponged storage buffers — which needs a per-node buffer allocator and a live-range analysis the current arm does not carry; the deterministic floor is the CPU lane, and both the plan admission and `Lower` refuse until the chain lands rather than silently falling back.
