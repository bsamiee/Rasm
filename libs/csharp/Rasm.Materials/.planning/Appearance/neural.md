# [MATERIALS_NEURAL]

Materials OWNS the photo-to-PBR stage vocabulary and SPECIFIES inference; `Rasm.Compute` EXECUTES it: one `ModelCard` frozen registry keyed by `ModelCardId` carries every admitted model as DATA — stage, licence class, weight policy, weight-artefact digest, tensor contract, shape buckets, execution-provider ladder, partition bound, residual ceiling, and the physical-channel prohibition — so admitting or retiring a model is a ROW and no surface moves. One `StageProduct` `[Union]` closes what a stage can emit AND what a caller demands: a frozen `Raster/set#TEXTURE_SET` `TextureChannel` that lands as a plane, or a `PriorField` intermediate that feeds another stage and never reaches a set. `PbrStage` rows declare only what they CONSUME and what they EMIT, so the dependency relation is DERIVED — one greedy cover over the requested products, one fixpoint closure over the consume-emit relation, one refinement pass keyed on the requested output extent — and `StagePlan.Plan` folds that relation into the dependency-ordered `Seq<StageStep>` the `[WIRE]` seam carries, each stage's input bound to its PRODUCER rather than to the source photo and each step carrying the settled result a `StageReplay` consult already holds. `LicenseClass` closes the grant vocabulary and its `Blocked` row REFUSES at request construction rather than at execution; `StageResult`/`StageOutput` carry every produced plane back with the provider the session reached, the graph partition count, the golden-output residual, and the digest of the weight bytes the executor loaded as typed evidence. `ModelCard` admits a new model as one ROW, `LicenseClass` a new licence posture, `PriorField` a new intermediate, and `PbrStage` a new inference stage declaring its consumed and emitted products — never a per-model type, a hardcoded model name inside a stage, a hand-listed pipeline order, or a boolean gate standing in for a grant. `ModelRegistry` and `StagePlan` compose the `Raster/set#TEXTURE_SET` `TextureChannel` roster as their channel vocabulary, the `Raster/plane#PLANE_FORMAT` `PlaneFormat`/`PlaneTransfer` bands, the seam `Rasm.Element` `ContentAddress` for every plane blob, the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail, and the kernel `Dimension`/`Op` atoms — re-minting no channel vocabulary, no plane format, no hash, and no fault. Height is NOT a stage and NOT a prior: it integrates from `geometry_normal` under a `PriorField.Depth` low-frequency anchor as a `Raster/filter#PLANE_OP`, pure math with no model. Tiling is NOT a stage: `Raster/tile#TILE_SYNTH` owns seam coherence procedurally. Text-to-material generation is NOT a stage: it is an external-service seam this page rules out of the in-process registry.

## [01]-[INDEX]

- [02]-[MODEL_REGISTRY]: `ModelRegistry` freezes the `ModelCard` row table with its selection fold, over `PriorField`/`StageProduct` the emission vocabulary, `PbrStage` the stage family and its consume-emit declaration, `TensorContract` the graph-shape carrier, and the `LicenseClass`/`WeightPolicy`/`InferenceProvider`/`TensorPrecision` bands.
- [03]-[STAGE_PLAN]: `StagePlan` resolves the `StageIntent`/`StagePolicy` request shapes through cover-closure-refine into the `StageStep` sequence carrying the `StageRequest`/`StageResult`/`StageOutput` wire records the `Rasm.Compute` execution seam moves, `StageInput` binding each stage to its producer, `InferenceTiling` its tiling, and `StageReplay` the settled-evidence consult.
- [04]-[RESEARCH]: open epistemic debt with its verification route.

## [02]-[MODEL_REGISTRY]

- Owner: `ModelRegistry` the frozen `ModelCard` table keyed by `ModelCardId`; `PbrStage`/`PriorField`/`StageSelection`/`LicenseClass`/`WeightPolicy`/`InferenceProvider`/`TensorPrecision` `[SmartEnum]` bands; `StageProduct` `[Union]` the emission vocabulary; `TensorContract` the graph-shape carrier; `ModelCard` the row.
- Cases: stage {`Delight`, `Albedo`, `Normals`, `Depth`, `Svbrdf`, `IntrinsicAppearance`, `SpectralReflectance`, `SuperResolve`, `Tileability`}; product {`Channel`, `Prior`, `Measure`}; prior {`Delit`, `Depth`, `Spectral`}; score {`Tileability`}; selection {`Cover`, `Refine`}; licence {`Permissive`, `Copyleft`, `OpenRail`, `Research`, `Blocked`}; weight {`Redistributable`, `CallerSupplied`}; provider {`Cpu`, `CoreMl`, `WebGpu`}; precision {`Fp32`, `Fp16`}.
- Entry: `public static Fin<ModelCard> Select(PbrStage stage, StagePolicy policy, Op key)` is the ONE selection fold — the requested stage, the caller's licence ceiling, and the provider preference resolve one card off the frozen rows, so a stage never names a model and a model swap is a row edit; `ModelRegistry.Rows` is the frozen table and `EmitterOf` the roster-order producer lookup the plan folds. `Select` rails `MaterialFault.Parameter` naming the stage on an unregistered stage, a stage whose every card exceeds the caller's licence ceiling, or a `Blocked` card pinned explicitly — a stage with no admitted card is DECLARED absence, never a silent skip.
- Law: a stage declares CONSUMES and EMITS and nothing else. `StagePlan` resolves every dependency edge from the relation between them against the SELECTED plan, so the order is derived rather than authored, a stage carrying a `Requires` list that contradicts its own inputs is unrepresentable, and a model whose graph gains a second input widens one `TensorContract` column and one `Consumes` row with no fold edit. `Delight` emits `PriorField.Delit` and NOT `base_color`: a de-lit photograph is a de-shadowed sRGB observation still carrying view-dependent residue, so publishing it as a base-colour plane seats an intermediate in a set the shading model then reads as measured reflectance. `Depth` emits `PriorField.Depth`, the monocular relative-inverse-depth prior that anchors the low-frequency ambiguity Frankot-Chellappa integration cannot recover — `height` remains the `Raster/filter#PLANE_OP` product `Raster/set#TEXTURE_SET` declares `Derived("geometry_normal")`, so no stage emits it and no card claims it. `SpectralReflectance` emits `PriorField.Spectral`, the per-wavelength `(η, k)` curve `surface#CONDUCTOR_IOR` grounds a metal from: it admits through `surface#SPECTRAL_UPSAMPLE` per `SpectralBand` and lands as a SET-LEVEL column beside `ConductorMetal`'s three-band `ComplexIor`, the three-band pair staying the fast path — a substance fact one material carries, never a per-texel channel.
- Law: FOUR STAGES AND THE REFINE PATH ARE UNREACHABLE TODAY, and the registry says so in rows rather than in silence. `Albedo`, `SpectralReflectance`, `Svbrdf`, and `Tileability` carry only `Blocked` cards, and both `SuperResolve` rows are `Blocked`, so `Serviceable` answers false for each at every ceiling and the cover fold plans no route through them — the estate knows the artefacts exist, records what each would need, and grants nothing. That is the honest capability statement: the vocabulary, the plan algebra, the contract, and the admission gate are all complete and exercised by the granted stages, and each blocked stage arms the moment its weight card publishes a licence, as ONE row edit. `ModelCard.Artefact` is likewise a declared shape carrying absence on every current row, because no REDISTRIBUTABLE weight artefact ships yet — the digest law stands as design and the column fills at the first redistributable admission, which is a row edit and not a surface one. `ModelCard.Residual` reads the same way and settles by the same mechanism: a divergence is a property of a graph RUNNING on a provider, so each card's band carries its DECLARED default until that card's first `Rasm.Compute` execution measures it, and the measurement is `StageResult.GoldenDelta` itself rather than a study — one fixed input across every provider the row lists fills `Upper` for a deterministic card at `ResidualBand.Ceiling`, while a `Stochastic` one takes that same `Upper` beside a seed sweep filling `Lower` at `ResidualBand.Point`. Estimating a ceiling ahead of execution would invert the column's own law by writing an unmeasured number into the slot a measurement gates; the two diffusers rows settle only once their exports exist, a band measured against an absent graph measuring nothing. [SPIKE]: each row's residual VALUES converge on that first per-card execution alone; the deterministic floor is the declared band defaults, the `Stochastic`-selected band shape, and `Admits` reading `Upper` either way, each total without them.
- Law: the weights gate is licence-class DATA, never code — anything an OSS project may freely use admits as a row (permissive, copyleft, OpenRAIL, research-class alike) and only a payment-gated model rejects outright, while a model whose WEIGHT card is silent about its licence enters as a `Blocked` row: the artefact is registered so the estate records what exists, and the row carries NO grant to run it. `LicenseClass.Grants` is the one predicate every gate reads; a boolean beside the class is a second truth two folds read differently.
- Law: `PhysicalChannelForbidden` is the generative-super-resolution law as data — a card carrying it may only emit `Channel(base_color)`, so a `superResolve` result naming any other product refuses at decode. Generative up-sampling invents plausible high-frequency detail; on an albedo that is acceptable authoring, on a normal, roughness, metalness, or height plane it is fabricated physics the shading model then integrates as if measured.
- Law: `WeightPolicy` states whether the estate MAY carry an artefact and carries no address — the app-root import boundary resolves `ModelCardId` to bytes for both rows alike, so the registry stays a vocabulary and never a distribution channel, and an address column every row fills with absence is the second truth this omission forecloses.
- Packages: Rasm (project — `Dimension`/`Op`), Rasm.Element (project — `ContentAddress`), Rasm.Materials.Appearance.Bsdf (`MaterialFault` band 2450), Rasm.Materials.Raster (`TextureChannel`/`PlaneFormat`/`PlaneTransfer` — the frozen channel and plane vocabularies this page projects, never re-declares), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[ValueObject<string>]` with the folder's `ComparerAccessors.StringOrdinal` key policy, and the `TryGet` lift onto `Option` the catalog's own guidance pins), LanguageExt.Core (`Fin`/`Seq`/`Option`), BCL inbox (`FrozenDictionary`). NO inference package is composed here — `Microsoft.ML.OnnxRuntime` is `Rasm.Compute`'s and this owner's strata rank forbids the reference.
- Growth: a new model is one `ModelCard` row; a new licence posture one `LicenseClass` row with its `Grants` column; a new execution provider one `InferenceProvider` row with its refusal semantics; a new intermediate one `PriorField` row carrying its plane shape; a new stage one `PbrStage` row declaring its consumed and emitted products, with one card row per stage the type-initialization census demands. A stage carries as many cards as the estate admits: today `Svbrdf` and `SuperResolve` carry two rows each and the rest one, with `Blocked` rows recording artefacts that exist without a grant — the multi-card stages are what the selection fold discriminates and a richer census is row growth, never a fold edit.
- Boundary: Materials owns the VOCABULARY and `Rasm.Compute` owns EXECUTION. `Rasm.Compute` ranks above `Rasm.Materials` in the branch strata with no reference in either direction, so nothing here reaches an ONNX session, an `OrtValue`, or a provider handle; the request crosses as a content-keyed `[WIRE]` recorded at both folder `ARCHITECTURE.md` `[03]-[SEAMS]` maps, Compute transcribes the stage, product, and licence keys into its own mirror, and the app root orchestrates the hop. That wire mints NO `tests/contracts/MANIFEST.md` entry — it never leaves the C# runtime, and a cross-language corpus entry for a branch-interior hop is the fabricated contract the cross-`libs/` ruling forecloses.
- Boundary: text-to-material generation is an EXTERNAL-SERVICE SEAM, not a registry row. `Raster/tile#TILE_SYNTH` coherence gates every set the estate holds, and the one locally-runnable candidate loses its tileability at export, so a generated set fails that gate; a service-produced set therefore enters through `Raster/set#SET_INGEST` classification like any other third-party asset, carrying its provenance and its licence class as ingest evidence, and no stage, card, or provider row represents it.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Concurrent;                 // ConcurrentDictionary — the per-ceiling Serviceable memo
using System.Collections.Frozen;
using System.Linq;                                   // the roster folds over Items
using LanguageExt;                                   // Fin, Seq, Option
using Rasm.Domain;                                   // Op
using Rasm.Drawing;                                  // ChannelDtype — the kernel storage-type roster the stage storage resolver names
using Rasm.Element.Projection;                       // ContentAddress (the seam content key)
using Rasm.Materials.Appearance.Bsdf;                // MaterialFault (band 2450)
using Rasm.Materials.Raster;                         // TextureChannel, PlaneFormat, PlaneTransfer
using Rasm.Numerics;                                 // Dimension
using Thinktecture;
using static LanguageExt.Prelude;

// Folder-root namespace beside acquisition#ACQUISITION, whose CaptureSource.NeuralPlanes arm consumes this page's
// StageResult set; the eventual source file is Appearance/Neural.cs.
namespace Rasm.Materials.Appearance;

// --- [TYPES] -------------------------------------------------------------------------------
// PriorField closes the intermediate planes a stage emits that no TextureSet carries. A later stage or a
// Raster/filter#PLANE_OP CONSUMES a prior and no set ever lands it, so its shape is declared here rather than borrowed
// from a channel row whose transfer, neutral, and mip law then describes a plane no shading rail reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PriorField {
    // Delit carries a de-lit photograph: shadows and specular highlights removed, still a DISPLAY-REFERRED observation
    // carrying view-dependent residue — the admitted artefact's own wrapper clips its decode to [-1,1], maps to [0,1],
    // and quantizes to 8-bit sRGB, so the transfer band is srgb by the artefact's own delivery. The estate admits the
    // PRE-quantization float decode and stores it at sixteen bits: the delighting stage's own 8-bit rounding would
    // otherwise bake into a prior two later estimators read as if it were the observation. It feeds the SVBRDF and
    // albedo estimators, never a base-colour plane.
    public static readonly PriorField Delit = new("delit", format: PlaneFormat.Rgba16, transfer: PlaneTransfer.Srgb);

    // Monocular relative inverse depth. It anchors the low-frequency shape Frankot-Chellappa integration of a normal
    // field cannot recover, so it feeds the height PlaneOp as a constraint and never becomes a channel of its own.
    public static readonly PriorField Depth = new("depth", format: PlaneFormat.R32F, transfer: PlaneTransfer.Raw);

    // A per-wavelength reflectance curve: one scalar lane per bsdf#SHADING_FRAME SpectralBand sample across the plane's
    // layer axis. It admits through surface#SPECTRAL_UPSAMPLE per band and lands as a SET-LEVEL column beside the
    // ConductorMetal row's three-band ComplexIor, never as a per-texel channel — a spectral curve is a substance fact
    // one material carries, so a TextureChannel row for it would mint a plane every consumer would read as reflectance.
    public static readonly PriorField Spectral = new("spectral", format: PlaneFormat.R32F, transfer: PlaneTransfer.Raw);

    public PlaneFormat Format { get; }
    public PlaneTransfer Transfer { get; }
}

// ScoreField closes the SCALAR measurements a model can emit — a grade over a whole tile rather than a field across
// it. A grade crosses BY VALUE: it carries no blob, no extent, and no transfer band, because nothing downstream
// samples it and a consumer reading one float out of an object store pays a round trip for a number the result
// already holds. That is the whole distinction from a plane product, and it is why this roster declares no plane
// shape at all — a shape column here would describe bytes no producer writes. A measurement lands in NO TextureSet
// and feeds no later stage's pixels.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScoreField {
    // The learned tileability grade over a candidate tile. Raster/tile#TILE_GATE remains the ONLY mint of a
    // TileProof — this row is a model's opinion, and the gate decides what a proof is — so the measurement crosses
    // as evidence a scorer produced rather than as a passing verdict.
    // NO AGGREGATION DIRECTION lands on this roster, and the omission is the law: a grade is a property of ONE
    // tile, so averaging or maximizing two would publish a number no model produced. The single-bucket admission
    // below is what keeps that from ever being asked.
    public static readonly ScoreField Tileability = new("tileability");
}

// What a stage emits or consumes: a frozen channel that lands in a set, a prior that feeds another stage, or a
// scalar measure that lands in neither. ONE union closes all three, so the plan folds one relation and a stage's
// inputs and outputs share a vocabulary. The third case is a genuinely distinct MODALITY rather than a plane wearing
// a small extent — a scorer and a classifier grade an input where an estimator produces a field, and the whole
// difference is that nothing downstream samples the result.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StageProduct {
    private StageProduct() { }

    public sealed record Channel(TextureChannel Field) : StageProduct;
    public sealed record Prior(PriorField Field) : StageProduct;
    public sealed record Measure(ScoreField Field) : StageProduct;

    public string Key => Switch(channel: static c => c.Field.Key, prior: static p => p.Field.Key, measure: static m => m.Field.Key);

    // The plane shape is TYPED ABSENCE on a measure, because a grade crosses by value and has no bytes to describe.
    // An Option here rather than a fabricated R32F row is what keeps a binding from declaring a storage lane no
    // producer writes and no reader fetches.
    public Option<PlaneFormat> Format =>
        Switch(channel: static c => Some(Storage(c.Field)), prior: static p => Some(p.Field.Format), measure: static _ => Option<PlaneFormat>.None);
    public Option<PlaneTransfer> Transfer =>
        Switch(channel: static c => Some(c.Field.Transfer), prior: static p => Some(p.Field.Transfer), measure: static _ => Option<PlaneTransfer>.None);

    // Parse resolves a declared key through ONE path in ROSTER PRECEDENCE: channels first (the frozen roster is the
    // larger and the canonical vocabulary), priors second, measures last. TryGet lifts onto Option per the
    // Thinktecture catalog's own guidance, so the throwing Get never reaches a rail and an unknown key is a typed
    // absence a decode gate names.
    public static Option<StageProduct> Parse(string key) =>
        TextureChannel.TryGet(key, out TextureChannel? channel)
            ? Some<StageProduct>(new Channel(channel))
            : PriorField.TryGet(key, out PriorField? prior)
                ? Some<StageProduct>(new Prior(prior))
                : ScoreField.TryGet(key, out ScoreField? score)
                    ? Some<StageProduct>(new Measure(score))
                    : None;

    // Storage width follows the channel row's semantic component count through the plane#PLANE_FORMAT shape
    // resolver, so a three-component channel resolves to the four-component row exactly as a pressed plane does and
    // no inference product mints a texel shape the arena cannot hold.
    static PlaneFormat Storage(TextureChannel channel) =>
        PlaneFormat.For(channel.Components, ChannelDtype.Unorm16).IfNone(PlaneFormat.Rgba16);
}

// Whether a stage answers a channel DEMAND or refines an already-produced product. A Refine row never satisfies a
// cover demand, and every input binds against the plan PREFIX — the stages already executed — so a stage that emits
// what it consumes chains onto its predecessor and can never close a cycle on itself.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StageSelection {
    public static readonly StageSelection Cover = new("cover");
    public static readonly StageSelection Refine = new("refine");
}

// PbrStage closes the inference stage family. Consumes and Emits are the WHOLE dependency declaration: the plan derives
// every edge from the relation between them, so a stage never carries a hand-authored predecessor list contradicting its
// own graph inputs. Declaration order is the preference ladder AND the topological order — a row's consumed products are
// emitted by an earlier row, which the registry asserts at type initialization.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PbrStage {
    public static readonly PbrStage Delight = new("delight", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(),
        emits: static () => Seq<StageProduct>(new StageProduct.Prior(PriorField.Delit)));
    public static readonly PbrStage Albedo = new("albedo", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(new StageProduct.Prior(PriorField.Delit)),
        emits: static () => Seq<StageProduct>(new StageProduct.Channel(TextureChannel.BaseColor)));
    public static readonly PbrStage Normals = new("normals", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(),
        emits: static () => Seq<StageProduct>(new StageProduct.Channel(TextureChannel.GeometryNormal)));
    public static readonly PbrStage Depth = new("depth", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(),
        emits: static () => Seq<StageProduct>(new StageProduct.Prior(PriorField.Depth)));
    public static readonly PbrStage Svbrdf = new("svbrdf", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(new StageProduct.Prior(PriorField.Delit)),
        emits: static () => Seq<StageProduct>(
            new StageProduct.Channel(TextureChannel.BaseColor), new StageProduct.Channel(TextureChannel.SpecularRoughness),
            new StageProduct.Channel(TextureChannel.BaseMetalness), new StageProduct.Channel(TextureChannel.GeometryNormal)));
    public static readonly PbrStage IntrinsicAppearance = new("intrinsicAppearance", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(),
        emits: static () => Seq<StageProduct>(
            new StageProduct.Channel(TextureChannel.BaseColor), new StageProduct.Channel(TextureChannel.SpecularRoughness),
            new StageProduct.Channel(TextureChannel.BaseMetalness)));
    public static readonly PbrStage SpectralReflectance = new("spectralReflectance", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(new StageProduct.Prior(PriorField.Delit)),
        emits: static () => Seq<StageProduct>(new StageProduct.Prior(PriorField.Spectral)));
    public static readonly PbrStage SuperResolve = new("superResolve", StageSelection.Refine, scale: 4,
        consumes: static () => Seq<StageProduct>(new StageProduct.Channel(TextureChannel.BaseColor)),
        emits: static () => Seq<StageProduct>(new StageProduct.Channel(TextureChannel.BaseColor)));
    // Tileability GRADES a produced base colour rather than estimating a field, and it is a COVER row for exactly
    // the reason every other cover row is one: a caller asks for the measure and this stage answers that demand, so
    // the greedy cover reaches it and the closure pulls whichever estimator produced the colour it reads. No third
    // StageSelection is owed — grading IS covering a demand, and the difference between this row and its siblings is
    // carried entirely by what it emits.
    public static readonly PbrStage Tileability = new("tileability", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(new StageProduct.Channel(TextureChannel.BaseColor)),
        emits: static () => Seq<StageProduct>(new StageProduct.Measure(ScoreField.Tileability)));

    public StageSelection Selection { get; }

    // Scale carries the extent multiplier a stage applies to its input. Every cover row is 1; a refine row's factor is
    // what the plan's target-extent test reads, so a 2x up-sampler is a row rather than a second refinement mechanism.
    public int Scale { get; }

    // Deferred columns: an eager field reference would capture null before the later rows materialize, the
    // forward-reference trap the vocabulary law names.
    [UseDelegateFromConstructor]
    public partial Seq<StageProduct> Consumes();
    [UseDelegateFromConstructor]
    public partial Seq<StageProduct> Emits();

    public bool EmitsProduct(StageProduct product) => Emits().Exists(p => p.Key == product.Key);

    // Declaration order as DATA. The cover tiebreak, the reverse-pass closure, and the forward-consumes invariant all
    // read it, so it materializes once off the generated roster rather than re-scanning Items per comparison.
    private static readonly Lazy<FrozenDictionary<PbrStage, int>> Order =
        new(static () => Items.Select(static (stage, index) => (stage, index)).ToFrozenDictionary(static e => e.stage, static e => e.index));

    public int Ordinal => Order.Value[this];
}

// LicenseClass closes the grant vocabulary. Grants is the ONE predicate every gate reads — a boolean beside the class is
// a second truth two folds read differently. Blocked is not a rejection: the artefact is REGISTERED so the estate
// records what exists, and the row carries no grant to run it, which is exactly what a silent weight card earns.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LicenseClass {
    public static readonly LicenseClass Permissive = new("permissive", grants: true,  rank: 0);
    public static readonly LicenseClass Copyleft   = new("copyleft",   grants: true,  rank: 1);
    public static readonly LicenseClass OpenRail   = new("openRail",   grants: true,  rank: 2);
    public static readonly LicenseClass Research   = new("research",   grants: true,  rank: 3);
    public static readonly LicenseClass Blocked    = new("blocked",    grants: false, rank: 4);
    public bool Grants { get; }

    // Rank orders the classes by how far a consumer's own posture must reach; a StagePolicy ceiling admits every
    // class at or below its rank, so a permissive-only consumer and a research-tolerant one read ONE column.
    public int Rank { get; }
}

// Whether the estate may CARRY the artefact. The card holds no address at all — the app-root import boundary
// resolves the id to bytes for both rows alike, so this column governs redistribution and nothing else.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeightPolicy {
    public static readonly WeightPolicy Redistributable = new("redistributable");
    public static readonly WeightPolicy CallerSupplied = new("callerSupplied");
}

// InferenceProvider ladders the execution providers as rows in preference order. Cpu is the guaranteed terminal — every
// other row may refuse at admission and degrade to the next with its reason on the receipt — and PinsFormat is the row's
// own pin rather than a caller knob, because a provider silently running an fp32 graph in fp16 is the trap the pin closes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InferenceProvider {
    public static readonly InferenceProvider Cpu    = new("cpu",    order: 0, terminal: true,  pinsFormat: false);
    public static readonly InferenceProvider CoreMl = new("coreMl", order: 1, terminal: false, pinsFormat: true);
    public static readonly InferenceProvider WebGpu = new("webGpu", order: 2, terminal: false, pinsFormat: false);
    public int Order { get; }
    public bool Terminal { get; }

    // CoreML pins its model format at session build; an unpinned graph silently executes at reduced precision with the
    // golden-output residual as its only signal, which is why the pin is a row column and not a caller argument.
    public bool PinsFormat { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TensorPrecision {
    public static readonly TensorPrecision Fp32 = new("fp32");
    public static readonly TensorPrecision Fp16 = new("fp16");
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<ModelCardFault>]
public readonly partial struct ModelCardId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = !string.IsNullOrWhiteSpace(value) && value.Length <= 128
            ? null
            : new ModelCardFault($"<model-card-id-blank-or-long:{value?.Length ?? 0}>");
}

// --- [MODELS] ------------------------------------------------------------------------------
// One graph output binding: the ONNX graph's OWN output tensor name, the COMPONENT LANE within it, the product it
// lands on, and the plane shape its bytes carry. Tensor and product are DISTINCT — a model names its outputs
// whatever its author chose, and collapsing the two would make the estate's channel roster the model's naming
// authority. Lane exists because a PACKED export names ONE tensor for several products: a graph emitting a single
// `material` tensor carrying roughness beside metalness binds two rows against it, and without the lane the second
// product would read the first's bytes.
// The shape pair is Option-typed for the same reason the product's own is: a MEASURE binding describes a value, not
// a lane, so absence there is the honest column rather than a fabricated storage row.
public readonly record struct OutputBinding(string Tensor, int Lane, StageProduct Product, Option<PlaneTransfer> Transfer, Option<PlaneFormat> Format);

// One graph input binding: the graph's own input tensor name and the product feeding it. The order matches the
// stage's Consumes sequence, so a two-input model binds two rows and the executor never guesses which is which.
public readonly record struct InputBinding(string Tensor, Option<StageProduct> Product);

// A SEED-DRIVEN graph input no upstream stage emits. A diffusion export cannot fold its latent into an initializer:
// the ensemble averages over independent draws, so a baked latent freezes every pass to one draw and silently
// destroys the variance the ensembling exists to reduce — the output stops being an ensemble and starts being a
// single sample wearing an ensemble's name. It therefore rides a CONTRACT column rather than an InputBinding, whose
// Product would have to be a fiction: nothing produces it, the executor synthesizes it from the request's own Seed,
// and its extent derives from the tile by the declared downscale rather than from a bucket of its own.
public readonly record struct LatentInput(string Tensor, int Channels, int Downscale);

// TensorContract carries the input bindings, the layout token, the fixed shape buckets the session warms per model, the
// reflect-pad width the tiler feathers over, the optional seed-driven latent, and the output bindings. Dynamic shapes
// fragment an ORT graph and defeat memory-pattern reuse, so a bucket roster is the shape and a free extent is
// unrepresentable.
public sealed record TensorContract(
    Seq<InputBinding> Inputs, string Layout, Seq<(Dimension Width, Dimension Height)> Buckets, int Overlap,
    Seq<OutputBinding> Outputs, Option<LatentInput> Latent) {

    // Layout is the graph's own dimension-order token (the ONNX convention its author exported under), and it binds
    // at the REQUEST: a tile's width and height mean nothing to an executor until it knows which axis of the input
    // tensor each fills, so StageRequest carries it beside the tile extents and the executor packs its buffer from
    // the token rather than from a convention it assumes. Declared here because the contract is the model's fact;
    // read there because packing is the execution's.
    public string TensorLayout => Layout;

    // BucketFor resolves the smallest bucket covering the extent, else the largest bucket the tiler walks the extent
    // with. Both arms return a bucket, so a plan never carries an untiled extent a session refuses.
    public Option<(Dimension Width, Dimension Height)> BucketFor(Dimension width, Dimension height) =>
        toSeq(Buckets.Filter(b => b.Width.Value >= width.Value && b.Height.Value >= height.Value)
                .OrderBy(static b => b.Width.Value * b.Height.Value))
            .Head
            .Match(Some, () => toSeq(Buckets.OrderByDescending(static b => b.Width.Value * b.Height.Value)).Head);
}

// A residual ceiling is a BAND, never a scalar, and ModelCard.Stochastic selects what the band was measured ACROSS.
// A DETERMINISTIC card re-runs its own graph identically, so its only divergence axis is the PROVIDER and the band
// collapses to a point the moment one sweep measures it — Lower and Upper are then the same number and saying so is
// a real claim rather than a filled column. A STOCHASTIC card re-draws per seed, so the same provider disagrees with
// itself and the band must span the seed sweep as well; pinning that to a scalar fixes the ceiling to whichever seed
// the measuring run happened to draw, which is the defect this shape forecloses. Lower is therefore typed absence
// until a sweep measures it, while Upper is the DECLARED ceiling Admit gates on — so one comparison serves both
// shapes and an unmeasured band still refuses a breach.
public readonly record struct ResidualBand(Option<double> Lower, double Upper) {
    // A deterministic card's band is a point BY CONSTRUCTION — one seed, so no seed spread exists to measure.
    public static ResidualBand Point(double ceiling) => new(Some(ceiling), ceiling);
    // A stochastic card declares its ceiling and leaves the floor absent until a seed sweep fills it.
    public static ResidualBand Ceiling(double upper) => new(Option<double>.None, upper);
    public bool Admits(double delta) => double.IsFinite(delta) && delta <= Upper;
}

// One registry row. Every axis a caller could otherwise hardcode is a column: which stage, what grant, whose
// weights, WHICH BYTES those weights are, which tensors, which providers in which order, what precision, how many graph partitions the session may
// fragment into before the receipt rails, what residual against the CPU reference the row tolerates, whether the
// model's products are admissible as physical channels, and whether the graph is STOCHASTIC — the column the plan's
// seed threading reads, so a diffusion-derived card replays under the policy seed and a deterministic card marks a
// caller-supplied seed inert at the source rather than at the executor's refusal. Artefact is the DIGEST of the weight
// file this card names — ModelCardId is a name a repository may re-publish, the artefact is what produced the plane —
// and it is an Option by the same law WeightPolicy states: a Redistributable row's bytes are estate data whose digest
// is registry truth, while a CallerSupplied row's bytes are a DEPLOYMENT fact the registry cannot know, so absence
// there is a declared absence and a literal would be a measurement no producer took. The card still carries no
// ADDRESS, so the app-root import boundary resolves bytes for both rows alike and the receipt still separates two
// revisions of one card by the digest the executor measured.
public sealed record ModelCard(
    ModelCardId Id, PbrStage Stage, LicenseClass License, string LicenseId, WeightPolicy Weights, Option<ContentAddress> Artefact,
    TensorContract Contract, Seq<InferenceProvider> Providers, TensorPrecision Precision, int PartitionBound,
    ResidualBand Residual, bool PhysicalChannelForbidden, bool Stochastic) {

    // Admits passes a card whose class grants at all and sits at or below the caller's ceiling.
    public bool Admits(LicenseClass ceiling) => License.Grants && License.Rank <= ceiling.Rank;

    // Permits lands base_color ALONE off a physical-channel-forbidden card; every other card lands exactly the products
    // its bindings declare. Plan-time selection and decode-time output admission BOTH read this predicate, so a
    // fabricated physical channel is unrepresentable rather than merely discouraged.
    public bool Permits(StageProduct product) =>
        (!PhysicalChannelForbidden || (product is StageProduct.Channel colour && colour.Field == TextureChannel.BaseColor))
        && Contract.Outputs.Exists(binding => binding.Product.Key == product.Key);
}

// --- [ERRORS] ------------------------------------------------------------------------------
// ModelCardFault carries the registry's own admission error, distinct from the band-2450 shading rail: a malformed card
// is a DECLARATION defect caught at type initialization, never a shade-time fault an Op key correlates.
public sealed record ModelCardFault(string Detail) : ValidationError(Detail);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ModelRegistry {
    // Rows freezes the table. The licence column reads the WEIGHT card and never the repository: a project licence
    // covers source, and a weight distributed with no declaration of its own is silent regardless of what its repo
    // carries. Both SuperResolve artefacts are exactly that — each ships from a release or an archive that states no
    // licence, and the permissive repo beside them grants nothing for the file — so both enter Blocked and a 4x target
    // REFUSES loudly rather than running an ungranted up-sampler. SuperMat carries no licence at all and enters the
    // same way. MatE is the watch row: a published architecture whose weights have not shipped, registered so its
    // arrival is a row flip rather than a redesign. The spectral-reflectance row is the same shape at the far end: no
    // artefact has shipped, so the stage exists, its consume-emit relation is declared, and the grant waits.
    public static readonly FrozenDictionary<ModelCardId, ModelCard> Rows = Seq(
        Card("stable-delight-yoso-v0-4-base", PbrStage.Delight, LicenseClass.Permissive, "apache-2.0", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["input:"], ["latent@delit"], buckets: [(512, 512)], overlap: 16),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 2e-3, stochastic: true),
        Card("supermat-albedo", PbrStage.Albedo, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["input:delit"], ["albedo@base_color"], buckets: [(512, 512)], overlap: 16),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 2e-3, stochastic: false),
        // The export contract is SETTLED: one input `image` at f32[1,3,H,W] to one `normal` output. The task and CLIP
        // embeddings the pipeline threads are constants of the checkpoint, so they fold to graph INITIALIZERS and no
        // second input survives the export — which is what makes this card a plain single-input row where the
        // diffusers sibling below is not.
        Card("lotus-d-normal-v1-1", PbrStage.Normals, LicenseClass.Permissive, "apache-2.0", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["image:"], ["normal@geometry_normal"], buckets: [(512, 512)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 1e-3, stochastic: false),
        Card("depth-anything-v2-small", PbrStage.Depth, LicenseClass.Permissive, "apache-2.0", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["image:"], ["depth@depth"], buckets: [(518, 518)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 5e-3, stochastic: false),
        Card("inria-unet-hf-svbrdf", PbrStage.Svbrdf, LicenseClass.Permissive, "mit", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["input:delit"],
                ["albedo@base_color", "roughness@specular_roughness", "metallic@base_metalness", "normal@geometry_normal"],
                buckets: [(256, 256), (512, 512)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 1e-3, stochastic: false),
        // The export contract is SETTLED and it is the one row carrying a LATENT. The graph takes `image` beside a
        // second `latent` input at f32[1,8,H/8,W/8] that CANNOT fold to an initializer: this card ensembles, and the
        // ensemble averages over independent latent draws, so baking one draw in freezes every pass to that draw and
        // the output stops being an ensemble while still reporting as one. It emits TWO tensors — `albedo` and a
        // PACKED `material` carrying roughness beside metalness — which is why two bindings name one tensor and the
        // derived lane separates them. Its extents cap at 768 and must be multiples of the latent downscale, both of
        // which the bucket roster and the Contract latent gate hold at declaration.
        Card("marigold-iid-appearance-v1-1", PbrStage.IntrinsicAppearance, LicenseClass.OpenRail, "openrail++-m", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["image:"], ["albedo@base_color", "material@specular_roughness", "material@base_metalness"],
                buckets: [(768, 768)], overlap: 32, latent: new LatentInput("latent", Channels: 8, Downscale: 8)),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 2e-3, stochastic: true),
        Card("realesr-general-x4v3", PbrStage.SuperResolve, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["input:base_color"], ["output@base_color"], buckets: [(256, 256), (512, 512)], overlap: 16),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 2, forbidden: true, ceiling: 5e-3, stochastic: false),
        Card("span-x4", PbrStage.SuperResolve, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["input:base_color"], ["output@base_color"], buckets: [(256, 256)], overlap: 16),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp16, partitions: 1, forbidden: true, ceiling: 8e-3, stochastic: false),
        Card("spectral-reflectance-unshipped", PbrStage.SpectralReflectance, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["input:delit"], ["spectrum@spectral"], buckets: [(512, 512)], overlap: 32),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 1e-3, stochastic: false),
        // TexTile clears the CONTRACT — a fixed NCHW f32[1,3,512,512] tile in, one scalar out, which is exactly the
        // shape the scorer modality admits — and fails the GRANT: its weights ship as a bare checkpoint with no
        // licence card of its own, and a repository licence never stands in for the weight card. So it enters
        // Blocked by the same law the two up-samplers enter under: the estate records that the artefact exists and
        // carries no permission to run it, and the day its card publishes a licence the row flips one column.
        Card("textile-tileability", PbrStage.Tileability, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["input:base_color"], ["score@tileability"], buckets: [(512, 512)], overlap: 16),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 1e-3, stochastic: false),
        Card("mate-unified", PbrStage.Svbrdf, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: null,
            Contract(["input:delit"],
                ["albedo@base_color", "roughness@specular_roughness", "metallic@base_metalness", "normal@geometry_normal"],
                buckets: [(512, 512)], overlap: 32),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 1e-3, stochastic: false))
        .ToFrozenDictionary(static card => card.Id);

    // Type-initialization invariants the plan's one-pass closure and one-pass ordering both stand on: every stage
    // carries at least one card, every card's bindings match its stage's declared products, and every consumed
    // product is emitted by an EARLIER row. A violated invariant is a declaration defect, so it fails the load
    // rather than reaching a rail — and it fails with the offending stage named, not as an opaque inner exception.
    static ModelRegistry() {
        string? breach = PbrStage.Items
            .Select(static stage =>
                !Rows.Values.Any(card => card.Stage == stage) ? $"<stage-uncarded:{stage.Key}>"
                : stage.Consumes().Exists(product => !PbrStage.Items.Any(prior =>
                      prior.Ordinal < stage.Ordinal && prior.Selection == StageSelection.Cover && prior.EmitsProduct(product)))
                    ? $"<stage-forward-consumes:{stage.Key}>"
                : Rows.Values.Where(card => card.Stage == stage).Any(card =>
                      !card.Contract.Outputs.ForAll(binding => stage.EmitsProduct(binding.Product))
                   || card.Contract.Inputs.Count != stage.Consumes().Count + (stage.Consumes().IsEmpty ? 1 : 0))
                    ? $"<stage-contract-mismatch:{stage.Key}>"
                : null)
            .FirstOrDefault(static reason => reason is not null);
        if (breach is not null) { throw new InvalidOperationException(breach); }
    }

    // ONE selection fold: the stage, the caller's licence ceiling, and the provider preference resolve a card off the
    // frozen rows. Ordering is grant-admissibility first, then provider preference match, then the smallest partition
    // bound — so a policy edit re-selects and a stage never names a model.
    public static Fin<ModelCard> Select(PbrStage stage, StagePolicy policy, Op key) =>
        policy.PinnedCard.Match(
            Some: id => Rows.TryGetValue(id, out ModelCard? pinned) && pinned.Stage == stage
                ? Admissible(pinned, policy, key)
                : MaterialFault.Parameter(key, $"<model-card-unpinned:{id.Value}:{stage.Key}>"),
            None: () => toSeq(toSeq(Rows.Values)
                    .Filter(card => card.Stage == stage && card.Admits(policy.Ceiling))
                    .OrderBy(card => card.Providers.Exists(p => p == policy.Preferred) ? 0 : 1)
                    .ThenBy(static card => card.PartitionBound)
                    .ThenBy(static card => card.License.Rank))
                .Head
                .ToFin(MaterialFault.Parameter(key, $"<model-stage-unserved:{stage.Key}@{policy.Ceiling.Key}>")));

    // EmitterOf resolves a product's producer within the EXECUTED PREFIX: the latest prior stage emitting it.
    // Resolution runs against the prefix and never the roster, so a refine stage binds the refinement it follows —
    // a chained up-sampler consumes its predecessor's output, never the cover stage's original — and a stage can
    // never bind itself because its prefix never contains it.
    public static Option<PbrStage> EmitterOf(Seq<PbrStage> prefix, StageProduct product) =>
        prefix.Filter(stage => stage.EmitsProduct(product)).Last;

    // Granted answers whether a stage carries ANY card admissible at the caller's ceiling; Serviceable closes it over
    // the consume relation, so the cover fold never selects a route whose upstream link has no grant — the licence
    // gate shapes the PLAN rather than surfacing as a refusal three stages deep.
    public static bool Granted(PbrStage stage, StagePolicy policy) =>
        Rows.Values.Any(card => card.Stage == stage && card.Admits(policy.Ceiling));

    // Serviceable MEMOIZES per (ceiling, stage) — the Ordinal Lazy pattern — because the recursion re-walks every
    // shared upstream link once per consumer: the cover fold asks it for every stage on every requested product, and
    // a diamond in the consume relation re-proves the same subtree exponentially in its depth. The key is the licence
    // CEILING and nothing else, since that is the only policy axis the answer reads; a Lazy per ceiling builds its
    // whole table once and every later ask is a lookup.
    static readonly ConcurrentDictionary<LicenseClass, Lazy<FrozenDictionary<PbrStage, bool>>> ServiceTable = new();

    public static bool Serviceable(PbrStage stage, StagePolicy policy) =>
        ServiceTable.GetOrAdd(policy.Ceiling, static ceiling =>
            new Lazy<FrozenDictionary<PbrStage, bool>>(() => Close(ceiling))).Value[stage];

    // Close builds the whole answer in ORDINAL order, so each stage's consume links are already settled when it is
    // reached and the fixpoint needs no recursion at all — the relation is acyclic by the ordinal law the roster
    // already enforces, which is exactly what makes one forward pass sufficient.
    static FrozenDictionary<PbrStage, bool> Close(LicenseClass ceiling) =>
        toSeq(PbrStage.Items.OrderBy(static s => s.Ordinal))
            .Fold(HashMap<PbrStage, bool>(), (table, stage) => table.Add(stage,
                Rows.Values.Any(card => card.Stage == stage && card.Admits(ceiling))
                    && stage.Consumes().ForAll(product => PbrStage.Items.Any(emitter =>
                           emitter.Ordinal < stage.Ordinal && emitter.Selection == StageSelection.Cover
                           && emitter.EmitsProduct(product) && table.Find(emitter).IfNone(false)))))
            .ToFrozenDictionary(static entry => entry.Key, static entry => entry.Value);

    static Fin<ModelCard> Admissible(ModelCard card, StagePolicy policy, Op key) =>
        card.Admits(policy.Ceiling)
            ? Fin.Succ(card)
            : MaterialFault.Parameter(key, $"<model-card-ungranted:{card.Id.Value}:{card.License.Key}>");

    static ModelCard Card(
        string id, PbrStage stage, LicenseClass license, string licenseId, WeightPolicy weights, string? artefact, TensorContract contract,
        InferenceProvider[] providers, TensorPrecision precision, int partitions, bool forbidden, double ceiling, bool stochastic) =>
        // The BAND SHAPE derives from the stochastic column rather than from a second row argument: a deterministic
        // card has no seed axis, so its floor is its ceiling and the point form states that; a stochastic one leaves
        // the floor absent until a seed sweep measures it. One declared ceiling, two shapes, zero authoring choice.
        new(ModelCardId.Create(id), stage, license, licenseId, weights, Digest(artefact, id), contract, toSeq(providers), precision, partitions,
            stochastic ? ResidualBand.Ceiling(ceiling) : ResidualBand.Point(ceiling), forbidden, stochastic);

    // A declared digest admits through the seam's OWN X32 wire factory, so a mistyped literal fails the LOAD with its
    // row named rather than seating a card whose artefact column addresses nothing; an absent one is the caller-supplied
    // row's structural absence, never a zero address standing in for an unmeasured file.
    static Option<ContentAddress> Digest(string? hex, string id) =>
        hex is null
            ? None
            : ContentAddress.Validate(hex, null, out ContentAddress? address) is null
                ? Some(address!)
                : throw new InvalidOperationException($"<model-artefact-digest:{id}:{hex}>");

    // Row spelling: an input is `tensor:product` with an EMPTY product naming the source photo, an output is
    // `tensor@product`. Two spellings for two directions keeps a transposed row unparseable rather than silently
    // valid, and the product key resolves against the FROZEN channel roster before the prior roster. The overlap
    // gate holds the frozen 8-32 feather band at declaration, so an out-of-band card fails the load, never a plan.
    static TensorContract Contract(string[] inputs, string[] outputs, (int W, int H)[] buckets, int overlap, LatentInput? latent = null) =>
        overlap is < 8 or > 32
            ? throw new InvalidOperationException($"<model-overlap-band:{overlap}>")
            // A MEASURE-EMITTING CARD IS SINGLE-BUCKET BY LAW. A scorer's contract is fixed-shape by definition — it
            // grades one tile at one extent — so a second bucket would put BucketFor's widest-bucket fallback in a
            // position to plan a multi-tile grid for a value that has no aggregation across tiles, and a grade is a
            // property of ONE tile. The gate sits HERE, where the buckets admit, so a card declaring the shape it
            // cannot honour fails the LOAD rather than reaching a plan that would then have to invent a reduction.
            : buckets.Length > 1
              && outputs.Any(static spec => spec.Split('@') is [_, string product] && ScoreField.TryGet(product, out _))
            ? throw new InvalidOperationException($"<model-measure-multi-bucket:{buckets.Length}>")
            // A latent's extent is the tile's own divided by the declared downscale, so a bucket the downscale does
            // not divide packs a fractional latent grid the session cannot bind. The gate is a DECLARATION gate for
            // the same reason the overlap band is: a row that cannot tile is a row nobody should be able to author.
            : latent is { } seeded && buckets.Any(b => b.W % seeded.Downscale != 0 || b.H % seeded.Downscale != 0)
            ? throw new InvalidOperationException($"<model-latent-bucket:{seeded.Tensor}:{seeded.Downscale}>")
            : new(toSeq(inputs).Map(static spec => spec.Split(':') switch {
                [string tensor, ""] => new InputBinding(tensor, None),
                [string tensor, string product] => new InputBinding(tensor, Some(Resolve(product, spec))),
                var malformed => throw new InvalidOperationException($"<model-input-spec:{string.Join(':', malformed)}>"),
            }),
            "nchw",
            toSeq(buckets).Map(static b => (Dimension.Create(b.W), Dimension.Create(b.H))), overlap,
            Outputs(outputs),
            Optional(latent));

    // Lane DERIVES from the tensor's repeat ordinal — a PACKED export names one tensor for several products, so the
    // nth binding of a repeated tensor reads its nth component group. Deriving it from the roster keeps a row's spec
    // one string per product and makes a transposed lane unspellable; an unrepeated tensor is lane zero, which is
    // every single-output row on the table.
    static Seq<OutputBinding> Outputs(string[] outputs) =>
        toSeq(outputs)
            .Map(static spec => spec.Split('@') switch {
                [string tensor, string product] => (Tensor: tensor, Product: Resolve(product, spec)),
                var malformed => throw new InvalidOperationException($"<model-output-spec:{string.Join('@', malformed)}>"),
            })
            .Fold((Bound: Seq<OutputBinding>(), Seen: HashMap<string, int>()),
                static (state, row) => state.Seen.Find(row.Tensor).IfNone(0) switch {
                    var lane => (state.Bound.Add(Bound(row.Tensor, lane, row.Product)), state.Seen.AddOrUpdate(row.Tensor, lane + 1)),
                })
            .Bound;

    // Bound reads the plane shape off the PRODUCT itself, never off a further spec field a row author could transpose
    // against the roster the product already carries.
    static OutputBinding Bound(string tensor, int lane, StageProduct product) => new(tensor, lane, product, product.Transfer, product.Format);

    // Resolve fails the load on an unknown product key with the offending spec named — a row is a DECLARATION — rather
    // than surfacing an opaque type-initialization inner chain a reader cannot attribute to a row.
    static StageProduct Resolve(string product, string spec) =>
        StageProduct.Parse(product).Match(
            Some: static resolved => resolved,
            None: () => throw new InvalidOperationException($"<model-product-unknown:{spec}>"));
}
```

## [03]-[STAGE_PLAN]

- Owner: `StagePlan` the planning fold; `StageIntent`/`StagePolicy` the request shapes; `StageInput` the producer binding; `InferenceTiling` the fixed-bucket tiling; `StageReplay`/`StageStep` the replay consult and the planned step it carries; `StageRequest`/`StageResult`/`StageOutput` the seam records.
- Entry: `public static Fin<Seq<StageStep>> Plan(StageIntent intent, Op key, Option<StageReplay> replay = default)` resolves the requested `StageProduct` set into the dependency-ordered step sequence — one entry for the whole plan, because a per-stage entrypoint pushes the ordering, the input binding, the extent threading, and the replay consult onto every caller; `StageResult.Admit(StageResult, ModelCard, StageRequest, Op)` is the ONE ingestion gate every returned result crosses — card echo, `Op` echo, product permission, output completeness, extent congruence, partition bound, and residual ceiling in one rail — and `InferenceTiling.Of(width, height, contract, key)` derives the tiling from the card's own bucket roster.
- Law: resolution is COVER, then CLOSURE, then REFINE, and each pass reads row data AND the licence posture. `Cover` runs a greedy set cover over the SERVICEABLE `StageSelection.Cover` rows against the requested `StageProduct` set — one demand axis over channels AND priors, so the depth anchor and the spectral curve are requestable exactly as a channel is and a prior-emitting stage is reachable rather than orphaned — serviceable means the stage's whole consume-closure holds a granting card at the caller's ceiling, so the grant gate shapes the route instead of surfacing as a refusal three stages deep, the stage covering the most still-uncovered requested products wins, declaration order breaking every tie. `Closure` walks `PbrStage.Items` in REVERSE declaration order once, pulling each selected stage's consumed products back to their serviceable emitters; a single reverse pass is exact because the registry asserts at type initialization that a consumed product is emitted by an earlier row. `Refine` ACCUMULATES the refinement chain against the target-over-source factor — each granted refine row whose `Scale` divides the remainder appends in declaration order, an anisotropic or unreachable target REFUSES, and a chained refinement binds its predecessor through the prefix rather than the cover stage's original.
- Law: each stage's INPUT is its PRODUCER's output, never the source photo. `StageInput.Source` carries the intent's plane for a stage consuming nothing, and `StageInput.Produced` names the emitting stage and the product for a stage consuming one, which the executor resolves against the results it already holds. Handing every stage the same source blob runs a chain whose links never touch, its albedo estimator reading the raw photograph the delighting stage exists to replace.
- Law: extent threads THROUGH the plan. `InferenceTiling` derives from the extent a stage's input carries, and the stage's own `Scale` column produces the extent its consumers see, so a four-times up-sampler's downstream tiling is correct without a caller recomputing anything and a mismatched tile grid is unrepresentable.
- Law: `LicenseClass.Blocked` REFUSES at request construction, not at execution — an explicit pin reads a blocked card's metadata alone, `StageRequest.Of` refuses every request naming it, so the grant gate sits at the earliest point holding it rather than deep inside a runtime trusting a caller's word.
- Law: tiling is FIXED-SHAPE and reflect-padded. Dynamic input shapes fragment an ORT graph into many partitions and defeat memory-pattern reuse, so `InferenceTiling` selects the card's own bucket, pads by reflection, and feathers the declared overlap; the tile grid, the overlap, the pad mode, and the warm-up bucket key all ride the request, so the executor warms one session per bucket and never re-derives geometry the plan already fixed. `InferenceTiling.Of` counts the FIRST tile whole and steps the remainder by the stride, so an extent equal to its bucket is one tile rather than two.
- Law: the `StageInfer` REPLAY consult threads into this fold and nowhere else. `HookModality.Observe` points stay decorator-only and their owners name nothing, while a `Replay` verdict enters the owner's OWN rail — a decorator wrapping `Plan` sees the whole sequence and skips no stage inside it — so the port is a `StageReplay` argument the composition root binds, keyed on the minted `StageRequest` because that record IS the identity a retained typed envelope carries. `StageResult.Admit` gates a held result against the freshly-minted request exactly as it gates a live one, so a drifted card, a short output set, an out-of-extent typed envelope, or a breached residual refuses the PLAN rather than seating a prior run's planes under this plan's extent; absence is the live path, so a composition with no replay store issues every step.
- Law: the `PriorField.Depth` prior couples to the height integration as a POST-INTEGRATION AFFINE FIT, never a boundary condition and never a low-frequency additive blend. `PriorField.Depth` carries RELATIVE INVERSE depth — scale-and-shift ambiguous by construction — so it holds no value a boundary condition imposes, and `Raster/set#TEXTURE_SET` derives `height` under `HeightSolver.Spectral`, a periodic solve that HAS no boundary to impose one on. That solve leaves exactly two degrees of freedom free, the DC term and the low-frequency ramp normals cannot constrain, so the fit is one closed-form least squares for `(a, b)` in `a·h + b` against the prior's reciprocal over the valid mask — two unknowns for two free parameters, with the fit residual riding the `HeightEvidence` the forward direction already records. Frequency-domain blending buys a cutoff knob and mixes two fields whose scales disagree; a boundary condition demands metric depth the prior does not carry.
- Law: `ResidualBand` gates a NORMALIZED residual: `GoldenDelta` is the per-channel RMS difference against the card's own CPU-provider reference, divided by the output's declared extent, so one ceiling is extent-invariant and a 4x up-sampler's ceiling means the same thing as a 512-square normal field's. Un-normalized L2 distance grows with the square root of the pixel count, which makes a single ceiling a function of the bucket rather than of the provider divergence it exists to bound. The band's SHAPE follows `Stochastic`: a deterministic card varies on the provider axis alone and its floor is its ceiling, while a stochastic card re-draws per seed and carries an absent floor until a seed sweep measures it — `Admits` reads `Upper` either way, so an unmeasured floor never weakens the gate.
- Law: `StageResult` carries typed evidence, never a bare success — `ProviderUsed` records the execution provider AFTER any policy refusal so a silent degradation reads off the receipt, `PartitionCount` rails when the session fragmented past the card's declared bound, `GoldenDelta` carries the residual against the model's own CPU-provider reference output so a fast-but-wrong provider is caught by measurement rather than by trust, `Artefact` carries the digest of the weight bytes the executor loaded so two revisions of one card separate on the receipt, and `Op` echoes the request's own with `Admit` comparing the pair, so a failure correlates to the plan that issued it and a transposed result refuses.
- Boundary: the `[WIRE]` seam is `Rasm.Materials/Appearance/neural` ↔ `Rasm.Compute/Model/inference`, recorded at BOTH folder `ARCHITECTURE.md` `[03]-[SEAMS]` maps. It is C#-interior and mints no corpus contract entry. `StageRequest` carries CONTENT ADDRESSES and vocabulary KEYS, never plane bytes — the source plane and every produced plane live in the write-once blob store the app root binds — so the wire stays small and the executor never marshals a raster through a message. `StageResult` ingestion produces the `TextureSet` the `acquisition#ACQUISITION` `CaptureSource.NeuralPlanes` arm consumes: `Prior` and `Measure` outputs are dropped at that boundary — a prior feeds another stage or a `Raster/filter#PLANE_OP` and a measure grades rather than covers, so neither belongs in a set — `Channel` outputs become the set's planes, and the arm returns the SET beside the averaged `MaterialParameters` row, so a photo becomes a shadeable material through `Raster/set#SET_BIND` rather than only an encodable wire. A `Measure` product reaches its consumer through `StageResult.Scores`; binding one to the `Raster/tile#TILE_GATE` `TilePolicy.Scorer` closure is the APP ROOT's hop, because the scorer contract is a delegate over a decoded tile and this owner mints no plane bytes and holds no executor.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// StagePolicy carries the caller's posture as ONE row: the licence ceiling admissible cards must sit at or below, the
// preferred provider, an optional pinned card overriding selection, the precision request, and the deterministic seed
// any stochastic stage threads. Every knob a signature could grow is a column here.
public sealed record StagePolicy(LicenseClass Ceiling, InferenceProvider Preferred, Option<ModelCardId> PinnedCard, TensorPrecision Precision, ulong Seed) {
    public static readonly StagePolicy Default = new(LicenseClass.Research, InferenceProvider.Cpu, None, TensorPrecision.Fp32, 0UL);
}

// What the caller wants: the source plane's blob and extent, the TARGET extent the products must reach, the PRODUCT
// set to produce, and the policy. Requested is the StageProduct union — the same vocabulary a stage emits — so a caller
// asks for a channel and for a prior through ONE demand axis: the depth anchor the height PlaneOp integrates against
// and the spectral curve the conductor row grounds from are requestable exactly as base_color is, where a channel-only
// demand made every prior-emitting stage unreachable by any plan. The requested products are the DISCRIMINANT the cover
// fold reads and the target extent the DISCRIMINANT the refine pass reads — a stage list or a super-resolve flag would
// push the resolution the plan owns back onto the caller.
public sealed record StageIntent(
    ContentAddress SourceKey, Dimension Width, Dimension Height, Dimension TargetWidth, Dimension TargetHeight,
    Seq<StageProduct> Requested, StagePolicy Policy) {

    public static Fin<StageIntent> Of(
        ContentAddress source, Dimension width, Dimension height, Seq<StageProduct> requested, StagePolicy policy, Op key,
        Option<(Dimension Width, Dimension Height)> target = default) =>
        requested.IsEmpty
            ? MaterialFault.Parameter(key, "<stage-intent-no-products>")
            : target.IfNone((width, height)) is var (tw, th) && (tw.Value < width.Value || th.Value < height.Value)
                ? MaterialFault.Parameter(key, $"<stage-target-below-source:{tw.Value}x{th.Value}<{width.Value}x{height.Value}>")
                : Fin.Succ(new StageIntent(source, width, height, tw, th, toSeq(requested.Distinct()), policy));
}

// Where a stage's tensor input comes from. Source is the intent's own plane; Produced names the stage and the
// product whose output the executor already holds. ONE union, so a chained stage cannot silently read the photo.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StageInput {
    private StageInput() { }

    public sealed record Source(ContentAddress Key) : StageInput;
    public sealed record Produced(PbrStage Stage, StageProduct Product) : StageInput;

    // Wire spells the three flat fields the C#-interior roster carries: an empty stage key means the source plane, and
    // its middle slot is `Role` on both the request and the result wires — one spelling for one product vocabulary, so
    // a reader binds an input row to its producer's output row without a name translation of its own.
    public (string Stage, string Role, string Key) Wire => Switch(
        source: static s => (string.Empty, string.Empty, s.Key.ToValue()),
        produced: static p => (p.Stage.Key, p.Product.Key, string.Empty));
}

// InferenceTiling carries a request's fixed-bucket tiling: the grid, the reflect-pad width, and the feather the executor blends
// seams over. It derives from the card's own bucket roster, so a caller cannot author a tiling the session refuses. The name is
// the ONNX bucket grid it is, never a tiling-SYNTHESIS plan — Raster/tile#TILE_SYNTH owns that word, and interchange#TEXTURE_EGRESS
// composes both namespaces in one compilation unit where a shared spelling resolves to neither.
public readonly record struct InferenceTiling(Dimension TileWidth, Dimension TileHeight, int Columns, int Rows, int Overlap, string PadMode, string Bucket) {
    public static Fin<InferenceTiling> Of(Dimension width, Dimension height, TensorContract contract, Op key) =>
        contract.BucketFor(width, height)
            .ToFin(MaterialFault.Parameter(key, $"<stage-no-bucket:{width.Value}x{height.Value}>"))
            .Map(bucket => new InferenceTiling(bucket.Width, bucket.Height,
                Columns: Steps(width.Value, bucket.Width.Value, contract.Overlap),
                Rows: Steps(height.Value, bucket.Height.Value, contract.Overlap),
                Overlap: contract.Overlap, PadMode: "reflect",
                Bucket: $"{bucket.Width.Value}x{bucket.Height.Value}"));

    // Steps counts the first tile as a whole bucket and advances each further tile by the stride, so an extent equal to
    // its bucket is exactly one tile; counting the whole extent against the stride emits a second empty tile.
    static int Steps(int extent, int bucket, int overlap) =>
        extent <= bucket ? 1 : 1 + (int)Math.Ceiling((double)(extent - bucket) / Math.Max(1, bucket - overlap));
}

// One inference request crossing the [WIRE] seam: the five tile fields ride FLAT (the executor re-derives its grid from extent, tile, and overlap deterministically; the
// producer-interior InferenceTiling keeps the grid for its own arithmetic and never crosses), and the member names are the
// mechanical casing of the frozen keys — ModelCardId, LicenseClass, Op — never a local synonym. The record carries
// content addresses and vocabulary KEYS, never plane bytes and never a live handle, so the executor decodes the keys
// into its own mirror and the two owners share a vocabulary rather than a type graph.
public sealed record StageRequest(
    PbrStage Stage, ModelCardId ModelCardId, Option<ContentAddress> Artefact, LicenseClass LicenseClass, Seq<StageInput> Inputs,
    Dimension InputWidth, Dimension InputHeight, Dimension OutputWidth, Dimension OutputHeight,
    int TileWidth, int TileHeight, int Overlap, string PadMode, string Bucket, string Layout,
    InferenceProvider Provider, TensorPrecision Precision, ulong Seed, Op Op) {

    // Construction IS the grant gate: a blocked card can be read from the registry but no request naming it exists.
    // Bind resolves the input bindings AGAINST THE EXECUTED PREFIX, so a chained stage cannot fall back to the
    // source plane and a chained refinement binds its predecessor. The seed columns cross only where the card
    // declares a stochastic graph — a deterministic card zeroes the seed at the source, so the executor's
    // seed-unbindable refusal marks a real defect rather than a defaulted knob.
    public static Fin<StageRequest> Of(
        ModelCard card, StageIntent intent, Seq<PbrStage> prefix, Dimension width, Dimension height, InferenceTiling tiles, Op key) =>
        from _ in guard(card.License.Grants, MaterialFault.Parameter(key, $"<stage-license-blocked:{card.Id.Value}>"))
        // A STOCHASTIC card at the ZERO DEFAULT SEED refuses HERE, the mirror of the deterministic law below it. A
        // deterministic card zeroes its seed at the source so the executor's seed-unbindable refusal marks a real
        // defect; a stochastic one cannot be handed a silently-minted nonzero default, because two runs of one plan
        // would then draw different latents while both receipts reported the same policy — replay forks and nothing
        // says so. The caller supplies its draw DELIBERATELY or the request does not exist.
        from _seed in guard(!card.Stochastic || intent.Policy.Seed != 0UL,
                MaterialFault.Parameter(key, $"<stage-stochastic-seed-unset:{card.Id.Value}>"))
        from inputs in Bind(card.Stage, intent, prefix, key)
        let resolved = Preferred(card, intent.Policy)
        select new StageRequest(card.Stage, card.Id, card.Artefact, card.License, inputs, width, height,
            Dimension.Create(width.Value * card.Stage.Scale), Dimension.Create(height.Value * card.Stage.Scale),
            tiles.TileWidth.Value, tiles.TileHeight.Value, tiles.Overlap, tiles.PadMode, tiles.Bucket, card.Contract.TensorLayout,
            resolved.Provider, resolved.Precision, card.Stochastic ? intent.Policy.Seed : 0UL, key);

    // Every consumed product resolves to the prefix's own emitter; a stage consuming nothing binds the source plane.
    // Bind rails an unresolvable product HERE as a plan defect rather than issuing a request the executor cannot satisfy.
    static Fin<Seq<StageInput>> Bind(PbrStage stage, StageIntent intent, Seq<PbrStage> prefix, Op key) =>
        stage.Consumes().IsEmpty
            ? Fin.Succ(Seq<StageInput>(new StageInput.Source(intent.SourceKey)))
            : stage.Consumes().Traverse(product =>
                  ModelRegistry.EmitterOf(prefix, product)
                      .Map(emitter => (StageInput)new StageInput.Produced(emitter, product))
                      .ToFin(MaterialFault.Parameter(key, $"<stage-input-unemitted:{stage.Key}:{product.Key}>"))).As()
              .Map(static bound => bound.Strict());

    // Preferred resolves the provider AND the precision TOGETHER, because they are not independent: a PinsFormat
    // provider fixes the graph's numeric format at session build, so pairing one with a policy precision it cannot
    // honour builds a session that runs at the pinned format and reports the requested one — a silent reduced-precision
    // execution whose only signal is the golden residual, exactly the trap the pin column exists to close. The pair
    // therefore resolves on one member: the card's ladder answers WHICH provider, and a pinned provider that cannot
    // honour the policy precision DEGRADES to the card's terminal row rather than carrying a precision it will not
    // keep. Returning the pair is what lets StageRequest.Of bind both from one decision — the prior shape resolved
    // the provider here and read the precision straight off the policy, so span-x4's Fp16 request bound a CoreML
    // provider pinned to its own format and the request's two columns disagreed by construction.
    static (InferenceProvider Provider, TensorPrecision Precision) Preferred(ModelCard card, StagePolicy policy) =>
        (card.Providers.Exists(p => p == policy.Preferred)
            ? policy.Preferred
            : toSeq(card.Providers.OrderBy(static p => p.Order)).Head.IfNone(InferenceProvider.Cpu)) switch {
            var chosen when !chosen.PinsFormat || policy.Precision == card.Precision => (chosen, policy.Precision),
            // A pinned provider whose format contradicts the card's own declared precision cannot serve this
            // request at all, so the fold walks past it to the terminal row the ladder guarantees rather than
            // issuing a request whose provider and precision columns describe two different executions.
            _ => (toSeq(card.Providers.Filter(static p => p.Terminal || !p.PinsFormat).OrderBy(static p => p.Order)).Head
                      .IfNone(InferenceProvider.Cpu), policy.Precision),
        };
}

// One produced plane: Role is the product the plane lands on (the wire lowers it to the canonical channel or prior
// key), then its blob, extent, and the transfer and format its bytes carry.
public readonly record struct StageOutput(StageProduct Role, ContentAddress BlobKey, Dimension Width, Dimension Height, PlaneTransfer Transfer, PlaneFormat Format);

// One MEASURED scalar. A grade carries its role and its value alone — no blob, no extent, no transfer band —
// because nothing downstream samples it and a consumer asking for a grade reads the number the result already holds
// rather than fetching four bytes out of a store. Fields and grades ride SEPARATE collections on the result because
// they are separate MODALITIES, not one collection with a small extent.
public readonly record struct StageScore(StageProduct Role, double Value);

// StageResult carries the executed result with its typed evidence. ProviderUsed is the
// provider AFTER any refusal, PartitionCount the graph fragmentation the session actually reached, GoldenDelta the
// residual against the model's CPU reference — so a fast-but-wrong provider is caught by measurement rather than trusted.
// Artefact is the digest of the WEIGHT BYTES the executor loaded, and it is unconditional here where the card's is an
// Option: a card may not know a caller-supplied artefact, but an execution always has one, so two revisions of one card
// separate on the receipt exactly where a durability consumer asks which bytes produced a plane.
// ParityFresh discriminates a MEASURED golden delta from a memoized one — the parity lease runs once per
// (card, provider, precision) triple, so only the run that took the measurement writes the histogram; Coverage is
// the tile-mosaic acceptance fraction the gate previously read once and discarded.
public sealed record StageResult(
    PbrStage Stage, ModelCardId ModelCardId, ContentAddress Artefact, Seq<StageOutput> Outputs, Seq<StageScore> Scores,
    InferenceProvider ProviderUsed,
    int PartitionCount, double ElapsedMs, double GoldenDelta, bool ParityFresh, float Coverage, int TilesEmitted, Op Op) {

    // Admit is the ONE ingestion gate. A physical-channel-forbidden card's result naming any product but base_color
    // refuses HERE, so the prohibition holds at the boundary the planes cross rather than as advice on the card; a
    // partition count past the card's declared bound and a non-finite or over-ceiling residual refuse the same way; a
    // result short of the card's declared outputs refuses rather than yielding a partial set a consumer completes
    // with neutrals it would then read as measured; and every channel output's extent proves against the REQUEST's
    // declared output extent — this page's own threads-through-the-plan law read back at the boundary the planes cross.
    public static Fin<StageResult> Admit(StageResult result, ModelCard card, StageRequest request, Op key) =>
        from _ in guard(result.ModelCardId == card.Id,
                MaterialFault.Parameter(key, $"<stage-card-mismatch:{result.ModelCardId.Value}!={card.Id.Value}>"))
        from _op in guard(result.Op == request.Op,
                MaterialFault.Parameter(key, $"<stage-op-mismatch:{card.Id.Value}>"))
        from _art in guard(request.Artefact.Map(declared => declared == result.Artefact).IfNone(true),
                MaterialFault.Parameter(key, $"<stage-artefact-mismatch:{card.Id.Value}:{result.Artefact.ToValue()}>"))
        // The permission gate reads BOTH collections: a forbidden product is forbidden whichever modality carries
        // it, and a grade slipping past because the sweep only walked the plane list is the hole this closes.
        from __ in guard(result.Outputs.ForAll(output => card.Permits(output.Role))
                    && result.Scores.ForAll(score => card.Permits(score.Role)),
                MaterialFault.Parameter(key, $"<stage-product-forbidden:{card.Id.Value}>"))
        // Completeness proves ACROSS BOTH collections, because a card's bindings span two modalities: a plane-shaped
        // binding lands in Outputs and a Measure-shaped one in Scores. One roster read against one collection
        // refuses every scorer result the moment grades stop riding the plane list, which is exactly what a
        // by-value grade does.
        from ___ in guard(card.Contract.Outputs.ForAll(binding => binding.Product is StageProduct.Measure
                    ? result.Scores.Exists(s => s.Role.Key == binding.Product.Key)
                    : result.Outputs.Exists(o => o.Role.Key == binding.Product.Key)),
                MaterialFault.Parameter(key, $"<stage-outputs-short:{card.Id.Value}:{result.Outputs.Count}+{result.Scores.Count}>"))
        from _extent in guard(result.Outputs.Filter(static o => o.Role is StageProduct.Channel)
                    .ForAll(o => o.Width == request.OutputWidth && o.Height == request.OutputHeight),
                MaterialFault.Parameter(key, $"<stage-extent-mismatch:{card.Id.Value}:{request.OutputWidth.Value}x{request.OutputHeight.Value}>"))
        from ____ in guard(result.PartitionCount <= card.PartitionBound,
                MaterialFault.Parameter(key, $"<stage-partition-bound:{result.PartitionCount}/{card.PartitionBound}>"))
        // The band's Upper is the gate whatever its floor: an unmeasured stochastic band still refuses a breach,
        // and its Lower fills from the seed sweep without moving this comparison.
        from _____ in guard(card.Residual.Admits(result.GoldenDelta),
                MaterialFault.Parameter(key, $"<stage-golden-delta:{result.GoldenDelta:R}/{card.Residual.Upper:R}>"))
        select result;

    // Planes projects the set-bound half of a result: a prior feeds the next stage and a measure is read by whoever
    // asked for it, so neither belongs in a TextureSet. The acquisition#ACQUISITION CaptureSource.NeuralPlanes arm
    // reads THIS projection — the frozen non-channel-drop site — and never filters the union itself.
    public Seq<StageOutput> Planes => Outputs.Filter(static output => output.Role is StageProduct.Channel);

}

// The StageInfer REPLAY consult. The point's modality is Replay because a settled inference is evidence a later run
// re-READS rather than re-earns, and re-reading is a FOLD decision no decorator can take: a decorator wrapping Plan
// sees the whole sequence and cannot skip one stage inside it. The port takes the MINTED request, because that record
// IS the identity a retained envelope carries — matching on it is structural where a re-derived key would be a second
// vocabulary — and Op is an operation identity rather than a run id, so a held envelope from a prior run matches the
// same request this plan mints. A composition with no replay store passes None and every stage issues.
public delegate Option<StageResult> StageReplay(StageRequest request);

// One planned stage: the request the executor issues, and the settled result a replay consult already holds. A
// replayed step crosses no wire — the executor skips it and reads the carried evidence — and it proves through the
// SAME StageResult.Admit a live result crosses, so a stale envelope refuses at the plan rather than seating a prior
// run's planes under this plan's extent.
public readonly record struct StageStep(StageRequest Request, Option<StageResult> Replayed);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class StagePlan {
    // ONE planning fold: cover the demand, close the dependencies, append the refinement chain, then thread the
    // extent through the ordered sequence binding each stage to its executed prefix and consulting the replay port.
    public static Fin<Seq<StageStep>> Plan(StageIntent intent, Op key, Option<StageReplay> replay = default) =>
        from covered in Cover(intent.Requested, intent.Policy, key)
        from ordered in Refine(Closure(covered, intent.Policy), intent, key)
        from steps in Thread(ordered, intent, replay, key)
        select steps;

    // GREEDY SET COVER over the SERVICEABLE cover rows — a stage counts only when its whole consume-closure holds a
    // granting card at the caller's ceiling, so the licence gate shapes the route: a base_color demand reaches the
    // de-light-then-SVBRDF chain where both links grant and the intrinsic-appearance estimator where they do not,
    // rather than refusing three stages deep on a chain the fold could see was ungranted. Each round takes the stage answering the most still-uncovered
    // channels, declaration order breaking ties. A channel no serviceable row emits is a DECLARED gap named with the
    // ceiling that closed it, never a silent short output set.
    static Fin<Seq<PbrStage>> Cover(Seq<StageProduct> requested, StagePolicy policy, Op key) {
        Seq<StageProduct> orphans = requested.Filter(product => !PbrStage.Items.Any(stage =>
            stage.Selection == StageSelection.Cover && ModelRegistry.Serviceable(stage, policy)
            && stage.EmitsProduct(product)));
        return orphans.IsEmpty
            ? Fin.Succ(Greedy(requested, policy, Seq<PbrStage>()))
            : MaterialFault.Parameter(key,
                  $"<stage-product-unproduced:{string.Join(',', orphans.Map(static p => p.Key))}@{policy.Ceiling.Key}>");
    }

    static Seq<PbrStage> Greedy(Seq<StageProduct> uncovered, StagePolicy policy, Seq<PbrStage> chosen) =>
        uncovered.IsEmpty
            ? chosen
            : toSeq(toSeq(PbrStage.Items)
                    .Filter(stage => stage.Selection == StageSelection.Cover && !chosen.Exists(c => c == stage)
                                  && ModelRegistry.Serviceable(stage, policy))
                    .Map(stage => (Stage: stage, Gain: uncovered.Count(stage.EmitsProduct)))
                    .Filter(static candidate => candidate.Gain > 0)
                    .OrderByDescending(static candidate => candidate.Gain)
                    .ThenBy(static candidate => candidate.Stage.Ordinal))
                .Head
                .Match(
                    Some: best => Greedy(uncovered.Filter(product => !best.Stage.EmitsProduct(product)), policy, chosen.Add(best.Stage)),
                    None: () => chosen);

    // ONE REVERSE PASS over the declaration order. A consumed product is emitted by an earlier row — the registry asserts
    // it at type initialization — so walking backwards and pulling each selected stage's consumed products to their
    // SERVICEABLE emitters reaches the transitive closure in a single sweep, and the forward filter that follows
    // yields the topological order directly. No visited set, no sort, no fixpoint loop for a graph the vocabulary orders.
    static Seq<PbrStage> Closure(Seq<PbrStage> seeds, StagePolicy policy) {
        Seq<PbrStage> reachable = toSeq(PbrStage.Items).Rev()
            .Fold(seeds, (reached, stage) =>
                reached.Exists(s => s == stage)
                    ? stage.Consumes().Fold(reached, (inner, product) =>
                          toSeq(toSeq(PbrStage.Items)
                                  .Filter(candidate => candidate.Selection == StageSelection.Cover
                                                    && candidate.EmitsProduct(product) && ModelRegistry.Serviceable(candidate, policy))
                                  .OrderBy(static candidate => candidate.Ordinal))
                              .Head
                              .Match(Some: emitter => inner.Exists(s => s == emitter) ? inner : inner.Add(emitter), None: () => inner))
                    : reached);
        return toSeq(PbrStage.Items).Filter(stage => reachable.Exists(s => s == stage)).Strict();
    }

    // Refine reads the TARGET extent, never a flag, and ACCUMULATES: the required factor is the target over the
    // source, each granted refine row whose Scale divides the remainder appends in declaration order, and a
    // remainder the roster cannot close REFUSES — a plan silently returning a fraction of the requested extent is
    // the defect this gate forecloses. Source-extent targets append nothing.
    static Fin<Seq<PbrStage>> Refine(Seq<PbrStage> ordered, StageIntent intent, Op key) {
        (int needW, int needH) = (intent.TargetWidth.Value / intent.Width.Value, intent.TargetHeight.Value / intent.Height.Value);
        if (needW != needH || intent.TargetWidth.Value != intent.Width.Value * needW || intent.TargetHeight.Value != intent.Height.Value * needH) {
            return MaterialFault.Parameter(key, $"<stage-target-anisotropic:{intent.TargetWidth.Value}x{intent.TargetHeight.Value}>");
        }
        (Seq<PbrStage> plan, int remaining) = toSeq(PbrStage.Items
            .Where(stage => stage.Selection == StageSelection.Refine && ModelRegistry.Granted(stage, intent.Policy))
            .Where(stage => stage.Consumes().ForAll(product => ModelRegistry.EmitterOf(ordered, product).IsSome))
            .OrderBy(static stage => stage.Ordinal))
            .Fold((Plan: ordered, Remaining: Math.Max(1, needW)), static (state, stage) =>
                state.Remaining > 1 && state.Remaining % stage.Scale == 0
                    ? (state.Plan.Add(stage), state.Remaining / stage.Scale)
                    : state);
        return remaining == 1
            ? Fin.Succ(plan)
            : MaterialFault.Parameter(key, $"<stage-target-unreachable:{intent.TargetWidth.Value}x{intent.TargetHeight.Value}:x{remaining}>");
    }

    // Extent threads through the ordered sequence: each stage tiles against the extent its input carries and
    // multiplies by its own Scale for its consumers, so a downstream tiling is correct with no caller arithmetic.
    // The fold hands each request the PREFIX of stages already threaded, so input binding and execution order are
    // one fact rather than a roster guess.
    // The replay consult sits HERE, between the mint and the append, because the request is the consult key and the
    // extent must thread identically whether a stage runs or replays — a held result is admitted through the same gate
    // a live one crosses, so a drifted card, a short output set, or an out-of-extent envelope refuses the PLAN.
    static Fin<Seq<StageStep>> Thread(Seq<PbrStage> ordered, StageIntent intent, Option<StageReplay> replay, Op key) =>
        ordered.Fold(
            Fin.Succ((Steps: Seq<StageStep>(), Prefix: Seq<PbrStage>(), Width: intent.Width, Height: intent.Height)),
            (state, stage) => state.Bind(carried =>
                from card in ModelRegistry.Select(stage, intent.Policy, key)
                from tiles in InferenceTiling.Of(carried.Width, carried.Height, card.Contract, key)
                from request in StageRequest.Of(card, intent, carried.Prefix, carried.Width, carried.Height, tiles, key)
                from held in Consulted(replay, request, card, key)
                select (Steps: carried.Steps.Add(new StageStep(request, held)), Prefix: carried.Prefix.Add(stage),
                        Width: request.OutputWidth, Height: request.OutputHeight)))
        .Map(static carried => carried.Steps.Strict());

    static Fin<Option<StageResult>> Consulted(Option<StageReplay> replay, StageRequest request, ModelCard card, Op key) =>
        replay.Bind(port => port(request))
            .Match(
                Some: held => StageResult.Admit(held, card, request, key).Map(Some),
                None: () => Fin.Succ(Option<StageResult>.None));
}
```

## [04]-[RESEARCH]

(none)
