# [MATERIALS_NEURAL]

Materials OWNS the photo-to-PBR stage vocabulary and SPECIFIES inference; `Rasm.Compute` EXECUTES it: one `ModelCard` frozen registry keyed by `ModelCardId` carries every admitted model as DATA — stage, licence class, weight policy, tensor contract, shape buckets, execution-provider ladder, partition bound, residual ceiling, and the physical-channel prohibition — so admitting or retiring a model is a ROW and no surface moves. One `StageProduct` `[Union]` closes what a stage can emit: a frozen `Raster/set#TEXTURE_SET` `TextureChannel` that lands as a plane, or a `PriorField` intermediate that feeds another stage and never reaches a set. `PbrStage` rows declare only what they CONSUME and what they EMIT, so the dependency relation is DERIVED — one greedy cover over the requested channels, one fixpoint closure over the consume-emit relation, one refinement pass keyed on the requested output extent — and `StagePlan.Plan` folds that relation into the dependency-ordered `Seq<StageRequest>` the `[WIRE]` seam carries with each stage's input bound to its PRODUCER rather than to the source photo. `LicenseClass` closes the grant vocabulary and its `Blocked` row REFUSES at request construction rather than at execution; `StageResult`/`StageOutput` carry every produced plane back with the provider the session reached, the graph partition count, and the golden-output residual as typed evidence. `ModelCard` admits a new model as one ROW, `LicenseClass` a new licence posture, `PriorField` a new intermediate, and `PbrStage` a new inference stage declaring its consumed and emitted products — never a per-model type, a hardcoded model name inside a stage, a hand-listed pipeline order, or a boolean gate standing in for a grant. `ModelRegistry` and `StagePlan` compose the `Raster/set#TEXTURE_SET` `TextureChannel` roster as their channel vocabulary, the `Raster/plane#PLANE_FORMAT` `PlaneFormat`/`PlaneTransfer` bands, the seam `Rasm.Element` `ContentAddress` for every plane blob, the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail, and the kernel `Dimension`/`Op` atoms — re-minting no channel vocabulary, no plane format, no hash, and no fault. Height is NOT a stage and NOT a prior: it integrates from `geometry_normal` under a `PriorField.Depth` low-frequency anchor as a `Raster/filter#PLANE_OP`, pure math with no model. Tiling is NOT a stage: `Raster/tile#TILE_SYNTH` owns seam coherence procedurally. Text-to-material generation is NOT a stage: it is an external-service seam this page rules out of the in-process registry.

## [01]-[INDEX]

- [02]-[MODEL_REGISTRY]: `ModelRegistry` freezes the `ModelCard` row table with its selection fold, over `PriorField`/`StageProduct` the emission vocabulary, `PbrStage` the stage family and its consume-emit declaration, `TensorContract` the graph-shape carrier, and the `LicenseClass`/`WeightPolicy`/`InferenceProvider`/`TensorPrecision` bands.
- [03]-[STAGE_PLAN]: `StagePlan` resolves the `StageIntent`/`StagePolicy` request shapes through cover-closure-refine into the `StageRequest`/`StageResult`/`StageOutput` wire records the `Rasm.Compute` execution seam carries, `StageInput` binding each stage to its producer and `TilePlan` its tiling.
- [04]-[RESEARCH]: open epistemic debt with its verification route.

## [02]-[MODEL_REGISTRY]

- Owner: `ModelRegistry` the frozen `ModelCard` table keyed by `ModelCardId`; `PbrStage`/`PriorField`/`StageSelection`/`LicenseClass`/`WeightPolicy`/`InferenceProvider`/`TensorPrecision` `[SmartEnum]` bands; `StageProduct` `[Union]` the emission vocabulary; `TensorContract` the graph-shape carrier; `ModelCard` the row.
- Cases: stage {`Delight`, `Albedo`, `Normals`, `Depth`, `Svbrdf`, `IntrinsicAppearance`, `SuperResolve`}; product {`Channel`, `Prior`}; prior {`Delit`, `Depth`}; selection {`Cover`, `Refine`}; licence {`Permissive`, `Copyleft`, `OpenRail`, `Research`, `Blocked`}; weight {`Redistributable`, `CallerSupplied`}; provider {`Cpu`, `CoreMl`, `WebGpu`}; precision {`Fp32`, `Fp16`}.
- Entry: `public static Fin<ModelCard> Select(PbrStage stage, StagePolicy policy, Op key)` is the ONE selection fold — the requested stage, the caller's licence ceiling, and the provider preference resolve one card off the frozen rows, so a stage never names a model and a model swap is a row edit; `ModelRegistry.Rows` is the frozen table and `EmitterOf` the roster-order producer lookup the plan folds. `Select` rails `MaterialFault.Parameter` naming the stage on an unregistered stage, a stage whose every card exceeds the caller's licence ceiling, or a `Blocked` card pinned explicitly — a stage with no admitted card is DECLARED absence, never a silent skip.
- Law: a stage declares CONSUMES and EMITS and nothing else. `StagePlan` resolves every dependency edge from the relation between them against the SELECTED plan, so the order is derived rather than authored, a stage carrying a `Requires` list that contradicts its own inputs is unrepresentable, and a model whose graph gains a second input widens one `TensorContract` column and one `Consumes` row with no fold edit. `Delight` emits `PriorField.Delit` and NOT `base_color`: a de-lit photograph is a de-shadowed sRGB observation still carrying view-dependent residue, so publishing it as a base-colour plane seats an intermediate in a set the shading model then reads as measured reflectance. `Depth` emits `PriorField.Depth`, the monocular relative-inverse-depth prior that anchors the low-frequency ambiguity Frankot-Chellappa integration cannot recover — `height` remains the `Raster/filter#PLANE_OP` product `Raster/set#TEXTURE_SET` declares `Derived("geometry_normal")`, so no stage emits it and no card claims it.
- Law: the weights gate is licence-class DATA, never code — anything an OSS project may freely use admits as a row (permissive, copyleft, OpenRAIL, research-class alike) and only a payment-gated model rejects outright, while a model whose WEIGHT card is silent about its licence enters as a `Blocked` row: the artefact is registered so the estate records what exists, and the row carries NO grant to run it. `LicenseClass.Grants` is the one predicate every gate reads; a boolean beside the class is a second truth two folds read differently.
- Law: `PhysicalChannelForbidden` is the generative-super-resolution law as data — a card carrying it may only emit `Channel(base_color)`, so a `superResolve` result naming any other product refuses at decode. Generative up-sampling invents plausible high-frequency detail; on an albedo that is acceptable authoring, on a normal, roughness, metalness, or height plane it is fabricated physics the shading model then integrates as if measured.
- Law: `WeightPolicy` states whether the estate MAY carry an artefact and carries no address — the app-root import boundary resolves `ModelCardId` to bytes for both rows alike, so the registry stays a vocabulary and never a distribution channel, and an address column every row fills with absence is the second truth this omission forecloses.
- Packages: Rasm (project — `Dimension`/`Op`), Rasm.Element (project — `ContentAddress`), Rasm.Materials.Appearance.Bsdf (`MaterialFault` band 2450), Rasm.Materials.Raster (`TextureChannel`/`PlaneFormat`/`PlaneTransfer` — the frozen channel and plane vocabularies this page projects, never re-declares), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[ValueObject<string>]` with the folder's `ComparerAccessors.StringOrdinal` key policy, and the `TryGet` lift onto `Option` the catalog's own guidance pins), LanguageExt.Core (`Fin`/`Seq`/`Option`), BCL inbox (`FrozenDictionary`). NO inference package is composed here — `Microsoft.ML.OnnxRuntime` is `Rasm.Compute`'s and this owner's strata rank forbids the reference.
- Growth: a new model is one `ModelCard` row; a new licence posture one `LicenseClass` row with its `Grants` column; a new execution provider one `InferenceProvider` row with its refusal semantics; a new intermediate one `PriorField` row carrying its plane shape; a new stage one `PbrStage` row declaring its consumed and emitted products. A stage carries as many cards as the estate admits: today `Svbrdf` and `SuperResolve` carry two rows each and the rest one, with `Blocked` rows recording artefacts that exist without a grant — the multi-card stages are what the selection fold discriminates and a richer census is row growth, never a fold edit.
- Boundary: Materials owns the VOCABULARY and `Rasm.Compute` owns EXECUTION. `Rasm.Compute` ranks above `Rasm.Materials` in the branch strata with no reference in either direction, so nothing here reaches an ONNX session, an `OrtValue`, or a provider handle; the request crosses as a content-keyed `[WIRE]` recorded at both folder `ARCHITECTURE.md` `[03]-[SEAMS]` maps, Compute transcribes the stage, product, and licence keys into its own mirror, and the app root orchestrates the hop. That wire mints NO `tests/contracts/MANIFEST.md` entry — it never leaves the C# runtime, and a cross-language corpus entry for a branch-interior hop is the fabricated contract the cross-`libs/` ruling forecloses.
- Boundary: text-to-material generation is an EXTERNAL-SERVICE SEAM, not a registry row. `Raster/tile#TILE_SYNTH` coherence gates every set the estate holds, and the one locally-runnable candidate loses its tileability at export, so a generated set fails that gate; a service-produced set therefore enters through `Raster/set#SET_INGEST` classification like any other third-party asset, carrying its provenance and its licence class as ingest evidence, and no stage, card, or provider row represents it.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;                                   // the roster folds over Items
using LanguageExt;                                   // Fin, Seq, Option
using Rasm.Domain;                                   // Op
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
    // Delit carries a de-lit photograph: shadows and specular highlights removed, still a display-referred observation
    // carrying view-dependent residue. It feeds the SVBRDF and albedo estimators, never a base-colour plane.
    public static readonly PriorField Delit = new("delit", format: PlaneFormat.Rgba16, transfer: PlaneTransfer.Srgb);

    // Monocular relative inverse depth. It anchors the low-frequency shape Frankot-Chellappa integration of a normal
    // field cannot recover, so it feeds the height PlaneOp as a constraint and never becomes a channel of its own.
    public static readonly PriorField Depth = new("depth", format: PlaneFormat.R32F, transfer: PlaneTransfer.Raw);

    public PlaneFormat Format { get; }
    public PlaneTransfer Transfer { get; }
}

// What a stage emits or consumes: a frozen channel that lands in a set, or a prior that feeds another stage. ONE
// union closes both, so the plan folds one relation and a stage's inputs and outputs share a vocabulary.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StageProduct {
    private StageProduct() { }

    public sealed record Channel(TextureChannel Field) : StageProduct;
    public sealed record Prior(PriorField Field) : StageProduct;

    public string Key => Switch(channel: static c => c.Field.Key, prior: static p => p.Field.Key);
    public PlaneFormat Format => Switch(channel: static c => Storage(c.Field), prior: static p => p.Field.Format);
    public PlaneTransfer Transfer => Switch(channel: static c => c.Field.Transfer, prior: static p => p.Field.Transfer);

    // Parse resolves a declared key through ONE path: channels first (the frozen roster is the larger and the canonical
    // vocabulary), priors second. TryGet lifts onto Option per the Thinktecture catalog's own guidance, so the throwing
    // Get never reaches a rail and an unknown key is a typed absence a decode gate names.
    public static Option<StageProduct> Parse(string key) =>
        TextureChannel.TryGet(key, out TextureChannel? channel)
            ? Some<StageProduct>(new Channel(channel))
            : PriorField.TryGet(key, out PriorField? prior)
                ? Some<StageProduct>(new Prior(prior))
                : None;

    // Storage width follows the channel row's semantic component count through the plane#PLANE_FORMAT shape
    // resolver, so a three-component channel resolves to the four-component row exactly as a pressed plane does and
    // no inference product mints a texel shape the arena cannot hold.
    static PlaneFormat Storage(TextureChannel channel) =>
        PlaneFormat.For(channel.Components, PlaneDepth.U16).IfNone(PlaneFormat.Rgba16);
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
    public static readonly PbrStage SuperResolve = new("superResolve", StageSelection.Refine, scale: 4,
        consumes: static () => Seq<StageProduct>(new StageProduct.Channel(TextureChannel.BaseColor)),
        emits: static () => Seq<StageProduct>(new StageProduct.Channel(TextureChannel.BaseColor)));

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
// One graph output binding: the ONNX graph's OWN output tensor name, the product it lands on, and the plane shape
// its bytes carry. Tensor and product are DISTINCT — a model names its outputs whatever its author chose, and
// collapsing the two would make the estate's channel roster the model's naming authority.
public readonly record struct OutputBinding(string Tensor, StageProduct Product, PlaneTransfer Transfer, PlaneFormat Format);

// One graph input binding: the graph's own input tensor name and the product feeding it. The order matches the
// stage's Consumes sequence, so a two-input model binds two rows and the executor never guesses which is which.
public readonly record struct InputBinding(string Tensor, Option<StageProduct> Product);

// TensorContract carries the input bindings, the layout token, the fixed shape buckets the session warms per model, the
// reflect-pad width the tiler feathers over, and the output bindings. Dynamic shapes fragment an ORT graph and defeat
// memory-pattern reuse, so a bucket roster is the shape and a free extent is unrepresentable.
public sealed record TensorContract(
    Seq<InputBinding> Inputs, string Layout, Seq<(Dimension Width, Dimension Height)> Buckets, int Overlap, Seq<OutputBinding> Outputs) {

    // BucketFor resolves the smallest bucket covering the extent, else the largest bucket the tiler walks the extent
    // with. Both arms return a bucket, so a plan never carries an untiled extent a session refuses.
    public Option<(Dimension Width, Dimension Height)> BucketFor(Dimension width, Dimension height) =>
        Buckets.Filter(b => b.Width.Value >= width.Value && b.Height.Value >= height.Value)
            .OrderBy(static b => b.Width.Value * b.Height.Value)
            .HeadOrNone()
            .Match(Some, () => Buckets.OrderByDescending(static b => b.Width.Value * b.Height.Value).HeadOrNone());
}

// One registry row. Every axis a caller could otherwise hardcode is a column: which stage, what grant, whose
// weights, which tensors, which providers in which order, what precision, how many graph partitions the session may
// fragment into before the receipt rails, what residual against the CPU reference the row tolerates, whether the
// model's products are admissible as physical channels, and whether the graph is STOCHASTIC — the column the plan's
// seed threading reads, so a diffusion-derived card replays under the policy seed and a deterministic card marks a
// caller-supplied seed inert at the source rather than at the executor's refusal.
public sealed record ModelCard(
    ModelCardId Id, PbrStage Stage, LicenseClass License, string LicenseId, WeightPolicy Weights,
    TensorContract Contract, Seq<InferenceProvider> Providers, TensorPrecision Precision, int PartitionBound,
    double ResidualCeiling, bool PhysicalChannelForbidden, bool Stochastic) {

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
    // Rows freezes the table. StableDelight enters Blocked because its weight card is silent about a licence while its
    // repository is permissive — the artefact is recorded, the grant withheld. SuperMat is the fastest known albedo
    // estimator and carries no licence at all, so it enters the same way. MatE is the watch row: a published
    // architecture whose weights have not shipped, registered so its arrival is a row flip rather than a redesign.
    // Two SuperResolve cards prove the table is data — a quality card and a CC0 fast card selected by policy.
    public static readonly FrozenDictionary<ModelCardId, ModelCard> Rows = Seq(
        Card("stable-delight-yoso-v0-4-base", PbrStage.Delight, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied,
            Contract(["input:"], ["latent@delit"], buckets: [(512, 512)], overlap: 16),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 2e-3, stochastic: true),
        Card("supermat-albedo", PbrStage.Albedo, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied,
            Contract(["input:delit"], ["albedo@base_color"], buckets: [(512, 512)], overlap: 16),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 2e-3, stochastic: false),
        Card("lotus-d-normal-v1-1", PbrStage.Normals, LicenseClass.Permissive, "apache-2.0", WeightPolicy.CallerSupplied,
            Contract(["input:"], ["normal@geometry_normal"], buckets: [(512, 512)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 1e-3, stochastic: false),
        Card("depth-anything-v2-small", PbrStage.Depth, LicenseClass.Permissive, "apache-2.0", WeightPolicy.CallerSupplied,
            Contract(["image:"], ["depth@depth"], buckets: [(518, 518)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 5e-3, stochastic: false),
        Card("inria-unet-hf-svbrdf", PbrStage.Svbrdf, LicenseClass.Permissive, "mit", WeightPolicy.CallerSupplied,
            Contract(["input:delit"],
                ["albedo@base_color", "roughness@specular_roughness", "metallic@base_metalness", "normal@geometry_normal"],
                buckets: [(256, 256), (512, 512)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 1e-3, stochastic: false),
        Card("marigold-iid-appearance-v1-1", PbrStage.IntrinsicAppearance, LicenseClass.OpenRail, "openrail++-m", WeightPolicy.CallerSupplied,
            Contract(["input:"], ["albedo@base_color", "roughness@specular_roughness", "metallic@base_metalness"],
                buckets: [(512, 512)], overlap: 32),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, forbidden: false, ceiling: 2e-3, stochastic: true),
        Card("realesr-general-x4v3", PbrStage.SuperResolve, LicenseClass.Permissive, "bsd-3-clause", WeightPolicy.CallerSupplied,
            Contract(["input:base_color"], ["output@base_color"], buckets: [(256, 256), (512, 512)], overlap: 16),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 2, forbidden: true, ceiling: 5e-3, stochastic: false),
        Card("span-x4", PbrStage.SuperResolve, LicenseClass.Permissive, "cc0-1.0", WeightPolicy.Redistributable,
            Contract(["input:base_color"], ["output@base_color"], buckets: [(256, 256)], overlap: 16),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp16, partitions: 1, forbidden: true, ceiling: 8e-3, stochastic: false),
        Card("mate-unified", PbrStage.Svbrdf, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied,
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
            None: () => toSeq(Rows.Values)
                .Filter(card => card.Stage == stage && card.Admits(policy.Ceiling))
                .OrderBy(card => card.Providers.Exists(p => p == policy.Preferred) ? 0 : 1)
                .ThenBy(static card => card.PartitionBound)
                .ThenBy(static card => card.License.Rank)
                .HeadOrNone()
                .ToFin(MaterialFault.Parameter(key, $"<model-stage-unserved:{stage.Key}@{policy.Ceiling.Key}>")));

    // EmitterOf resolves a product's producer within the EXECUTED PREFIX: the latest prior stage emitting it.
    // Resolution runs against the prefix and never the roster, so a refine stage binds the refinement it follows —
    // a chained up-sampler consumes its predecessor's output, never the cover stage's original — and a stage can
    // never bind itself because its prefix never contains it.
    public static Option<PbrStage> EmitterOf(Seq<PbrStage> prefix, StageProduct product) =>
        prefix.Filter(stage => stage.EmitsProduct(product)).LastOrNone();

    // Granted answers whether a stage carries ANY card admissible at the caller's ceiling; Serviceable closes it over
    // the consume relation, so the cover fold never selects a route whose upstream link has no grant — the licence
    // gate shapes the PLAN rather than surfacing as a refusal three stages deep.
    public static bool Granted(PbrStage stage, StagePolicy policy) =>
        Rows.Values.Any(card => card.Stage == stage && card.Admits(policy.Ceiling));

    public static bool Serviceable(PbrStage stage, StagePolicy policy) =>
        Granted(stage, policy)
        && stage.Consumes().ForAll(product => PbrStage.Items.Any(emitter =>
               emitter.Ordinal < stage.Ordinal && emitter.Selection == StageSelection.Cover
               && emitter.EmitsProduct(product) && Serviceable(emitter, policy)));

    static Fin<ModelCard> Admissible(ModelCard card, StagePolicy policy, Op key) =>
        card.Admits(policy.Ceiling)
            ? Fin.Succ(card)
            : MaterialFault.Parameter(key, $"<model-card-ungranted:{card.Id.Value}:{card.License.Key}>");

    static ModelCard Card(
        string id, PbrStage stage, LicenseClass license, string licenseId, WeightPolicy weights, TensorContract contract,
        InferenceProvider[] providers, TensorPrecision precision, int partitions, bool forbidden, double ceiling, bool stochastic) =>
        new(ModelCardId.Create(id), stage, license, licenseId, weights, contract, toSeq(providers), precision, partitions, ceiling, forbidden, stochastic);

    // Row spelling: an input is `tensor:product` with an EMPTY product naming the source photo, an output is
    // `tensor@product`. Two spellings for two directions keeps a transposed row unparseable rather than silently
    // valid, and the product key resolves against the FROZEN channel roster before the prior roster. The overlap
    // gate holds the frozen 8-32 feather band at declaration, so an out-of-band card fails the load, never a plan.
    static TensorContract Contract(string[] inputs, string[] outputs, (int W, int H)[] buckets, int overlap) =>
        overlap is < 8 or > 32
            ? throw new InvalidOperationException($"<model-overlap-band:{overlap}>")
            : new(toSeq(inputs).Map(static spec => spec.Split(':') switch {
                [string tensor, ""] => new InputBinding(tensor, None),
                [string tensor, string product] => new InputBinding(tensor, Some(Resolve(product, spec))),
                var malformed => throw new InvalidOperationException($"<model-input-spec:{string.Join(':', malformed)}>"),
            }),
            "nchw",
            toSeq(buckets).Map(static b => (Dimension.Create(b.W), Dimension.Create(b.H))), overlap,
            toSeq(outputs).Map(static spec => spec.Split('@') switch {
                [string tensor, string product] => Bound(tensor, Resolve(product, spec)),
                var malformed => throw new InvalidOperationException($"<model-output-spec:{string.Join('@', malformed)}>"),
            }));

    // Bound reads the plane shape off the PRODUCT itself, never off a fourth spec field a row author could transpose
    // against the roster the product already carries.
    static OutputBinding Bound(string tensor, StageProduct product) => new(tensor, product, product.Transfer, product.Format);

    // Resolve fails the load on an unknown product key with the offending spec named — a row is a DECLARATION — rather
    // than surfacing an opaque type-initialization inner chain a reader cannot attribute to a row.
    static StageProduct Resolve(string product, string spec) =>
        StageProduct.Parse(product).Match(
            Some: static resolved => resolved,
            None: () => throw new InvalidOperationException($"<model-product-unknown:{spec}>"));
}
```

## [03]-[STAGE_PLAN]

- Owner: `StagePlan` the planning fold; `StageIntent`/`StagePolicy` the request shapes; `StageInput` the producer binding; `TilePlan` the fixed-bucket tiling; `StageRequest`/`StageResult`/`StageOutput` the seam records.
- Entry: `public static Fin<Seq<StageRequest>> Plan(StageIntent intent, Op key)` resolves the requested `TextureChannel` set into the dependency-ordered request sequence — one entry for the whole plan, because a per-stage entrypoint pushes the ordering, the input binding, and the extent threading onto every caller; `StageResult.Admit(StageResult, ModelCard, StageRequest, Op)` is the ONE ingestion gate every returned result crosses — card echo, `Op` echo, product permission, output completeness, extent congruence, partition bound, and residual ceiling in one rail — and `TilePlan.Of(width, height, contract, key)` derives the tiling from the card's own bucket roster.
- Law: resolution is COVER, then CLOSURE, then REFINE, and each pass reads row data AND the licence posture. `Cover` runs a greedy set cover over the SERVICEABLE `StageSelection.Cover` rows — serviceable means the stage's whole consume-closure holds a granting card at the caller's ceiling, so the grant gate shapes the route instead of surfacing as a refusal three stages deep — the stage covering the most still-uncovered requested channels wins, declaration order breaking every tie. `Closure` walks `PbrStage.Items` in REVERSE declaration order once, pulling each selected stage's consumed products back to their serviceable emitters; a single reverse pass is exact because the registry asserts at type initialization that a consumed product is emitted by an earlier row. `Refine` ACCUMULATES the refinement chain against the target-over-source factor — each granted refine row whose `Scale` divides the remainder appends in declaration order, an anisotropic or unreachable target REFUSES, and a chained refinement binds its predecessor through the prefix rather than the cover stage's original.
- Law: each stage's INPUT is its PRODUCER's output, never the source photo. `StageInput.Source` carries the intent's plane for a stage consuming nothing, and `StageInput.Produced` names the emitting stage and the product for a stage consuming one, which the executor resolves against the results it already holds. Handing every stage the same source blob runs a chain whose links never touch, its albedo estimator reading the raw photograph the delighting stage exists to replace.
- Law: extent threads THROUGH the plan. `TilePlan` derives from the extent a stage's input carries, and the stage's own `Scale` column produces the extent its consumers see, so a four-times up-sampler's downstream tiling is correct without a caller recomputing anything and a mismatched tile grid is unrepresentable.
- Law: `LicenseClass.Blocked` REFUSES at request construction, not at execution — an explicit pin reads a blocked card's metadata alone, `StageRequest.Of` refuses every request naming it, so the grant gate sits at the earliest point holding it rather than deep inside a runtime trusting a caller's word.
- Law: tiling is FIXED-SHAPE and reflect-padded. Dynamic input shapes fragment an ORT graph into many partitions and defeat memory-pattern reuse, so `TilePlan` selects the card's own bucket, pads by reflection, and feathers the declared overlap; the tile grid, the overlap, the pad mode, and the warm-up bucket key all ride the request, so the executor warms one session per bucket and never re-derives geometry the plan already fixed. `TilePlan.Of` counts the FIRST tile whole and steps the remainder by the stride, so an extent equal to its bucket is one tile rather than two.
- Law: `StageResult` carries typed evidence, never a bare success — `ProviderUsed` records the execution provider AFTER any policy refusal so a silent degradation reads off the receipt, `PartitionCount` rails when the session fragmented past the card's declared bound, `GoldenDelta` carries the residual against the model's own CPU-provider reference output so a fast-but-wrong provider is caught by measurement rather than by trust, and `Op` echoes the request's own with `Admit` comparing the pair, so a failure correlates to the plan that issued it and a transposed result refuses.
- Boundary: the `[WIRE]` seam is `Rasm.Materials/Appearance/neural` ↔ `Rasm.Compute/Model/inference`, recorded at BOTH folder `ARCHITECTURE.md` `[03]-[SEAMS]` maps. It is C#-interior and mints no corpus contract entry. `StageRequest` carries CONTENT ADDRESSES and vocabulary KEYS, never plane bytes — the source plane and every produced plane live in the write-once blob store the app root binds — so the wire stays small and the executor never marshals a raster through a message. `StageResult` ingestion produces the `TextureSet` the `acquisition#ACQUISITION` `CaptureSource.NeuralPlanes` arm consumes: `Prior` outputs are dropped at that boundary because a prior feeds another stage or a `Raster/filter#PLANE_OP` and belongs in no set, `Channel` outputs become the set's planes, and the arm returns the SET beside the averaged `MaterialParameters` row, so a photo becomes a shadeable material through `Raster/set#SET_BIND` rather than only an encodable wire.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// StagePolicy carries the caller's posture as ONE row: the licence ceiling admissible cards must sit at or below, the
// preferred provider, an optional pinned card overriding selection, the precision request, and the deterministic seed
// any stochastic stage threads. Every knob a signature could grow is a column here.
public sealed record StagePolicy(LicenseClass Ceiling, InferenceProvider Preferred, Option<ModelCardId> PinnedCard, TensorPrecision Precision, ulong Seed) {
    public static readonly StagePolicy Default = new(LicenseClass.Research, InferenceProvider.Cpu, None, TensorPrecision.Fp32, 0UL);
}

// What the caller wants: the source plane's blob and extent, the TARGET extent the products must reach, the channel
// set to produce, and the policy. The requested channels are the DISCRIMINANT the cover fold reads and the target
// extent the DISCRIMINANT the refine pass reads — a stage list or a super-resolve flag would push the resolution the
// plan owns back onto the caller.
public sealed record StageIntent(
    ContentAddress SourceKey, Dimension Width, Dimension Height, Dimension TargetWidth, Dimension TargetHeight,
    Seq<TextureChannel> Requested, StagePolicy Policy) {

    public static Fin<StageIntent> Of(
        ContentAddress source, Dimension width, Dimension height, Seq<TextureChannel> requested, StagePolicy policy, Op key,
        Option<(Dimension Width, Dimension Height)> target = default) =>
        requested.IsEmpty
            ? MaterialFault.Parameter(key, "<stage-intent-no-channels>")
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

// TilePlan carries a request's fixed-bucket tiling: the grid, the reflect-pad width, and the feather the executor blends
// seams over. It derives from the card's own bucket roster, so a caller cannot author a tiling the session refuses.
public readonly record struct TilePlan(Dimension TileWidth, Dimension TileHeight, int Columns, int Rows, int Overlap, string PadMode, string Bucket) {
    public static Fin<TilePlan> Of(Dimension width, Dimension height, TensorContract contract, Op key) =>
        contract.BucketFor(width, height)
            .ToFin(MaterialFault.Parameter(key, $"<stage-no-bucket:{width.Value}x{height.Value}>"))
            .Map(bucket => new TilePlan(bucket.Width, bucket.Height,
                Columns: Steps(width.Value, bucket.Width.Value, contract.Overlap),
                Rows: Steps(height.Value, bucket.Height.Value, contract.Overlap),
                Overlap: contract.Overlap, PadMode: "reflect",
                Bucket: $"{bucket.Width.Value}x{bucket.Height.Value}"));

    // Steps counts the first tile as a whole bucket and advances each further tile by the stride, so an extent equal to
    // its bucket is exactly one tile; counting the whole extent against the stride emits a second empty tile.
    static int Steps(int extent, int bucket, int overlap) =>
        extent <= bucket ? 1 : 1 + (int)Math.Ceiling((double)(extent - bucket) / Math.Max(1, bucket - overlap));
}

// One inference request crossing the [WIRE] seam, spelling the frozen [07.1] roster COLUMN FOR COLUMN: the five
// tile fields ride FLAT (the executor re-derives its grid from extent, tile, and overlap deterministically; the
// producer-interior TilePlan keeps the grid for its own arithmetic and never crosses), and the member names are the
// mechanical casing of the frozen keys — ModelCardId, LicenseClass, Op — never a local synonym. The record carries
// content addresses and vocabulary KEYS, never plane bytes and never a live handle, so the executor decodes the keys
// into its own mirror and the two owners share a vocabulary rather than a type graph.
public sealed record StageRequest(
    PbrStage Stage, ModelCardId ModelCardId, LicenseClass LicenseClass, Seq<StageInput> Inputs,
    Dimension InputWidth, Dimension InputHeight, Dimension OutputWidth, Dimension OutputHeight,
    int TileWidth, int TileHeight, int Overlap, string PadMode, string Bucket,
    InferenceProvider Provider, TensorPrecision Precision, ulong Seed, Op Op) {

    // Construction IS the grant gate: a blocked card can be read from the registry but no request naming it exists.
    // Bind resolves the input bindings AGAINST THE EXECUTED PREFIX, so a chained stage cannot fall back to the
    // source plane and a chained refinement binds its predecessor. The seed columns cross only where the card
    // declares a stochastic graph — a deterministic card zeroes the seed at the source, so the executor's
    // seed-unbindable refusal marks a real defect rather than a defaulted knob.
    public static Fin<StageRequest> Of(
        ModelCard card, StageIntent intent, Seq<PbrStage> prefix, Dimension width, Dimension height, TilePlan tiles, Op key) =>
        from _ in guard(card.License.Grants, MaterialFault.Parameter(key, $"<stage-license-blocked:{card.Id.Value}>"))
        from inputs in Bind(card.Stage, intent, prefix, key)
        select new StageRequest(card.Stage, card.Id, card.License, inputs, width, height,
            Dimension.Create(width.Value * card.Stage.Scale), Dimension.Create(height.Value * card.Stage.Scale),
            tiles.TileWidth.Value, tiles.TileHeight.Value, tiles.Overlap, tiles.PadMode, tiles.Bucket,
            Preferred(card, intent.Policy), intent.Policy.Precision, card.Stochastic ? intent.Policy.Seed : 0UL, key);

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

    // Preferred picks the provider the request asks for: the caller's preference when the card lists it, otherwise the
    // card's own highest-ordered row; the executor may still refuse and degrade, which the result reports rather than hides.
    static InferenceProvider Preferred(ModelCard card, StagePolicy policy) =>
        card.Providers.Exists(p => p == policy.Preferred)
            ? policy.Preferred
            : card.Providers.OrderBy(static p => p.Order).HeadOrNone().IfNone(InferenceProvider.Cpu);
}

// One produced plane spelling the frozen [07.3] roster: Role is the product the plane lands on (the wire lowers it
// to the canonical channel or prior key), then its blob, extent, and the transfer and format its bytes carry.
public readonly record struct StageOutput(StageProduct Role, ContentAddress BlobKey, Dimension Width, Dimension Height, PlaneTransfer Transfer, PlaneFormat Format);

// StageResult carries the executed result with its typed evidence under the frozen [07.2] names. ProviderUsed is the
// provider AFTER any refusal, PartitionCount the graph fragmentation the session actually reached, GoldenDelta the
// residual against the model's CPU reference — so a fast-but-wrong provider is caught by measurement rather than trusted.
public sealed record StageResult(
    PbrStage Stage, ModelCardId ModelCardId, Seq<StageOutput> Outputs, InferenceProvider ProviderUsed,
    int PartitionCount, double ElapsedMs, double GoldenDelta, int TilesEmitted, Op Op) {

    // Admit is the ONE ingestion gate. A physical-channel-forbidden card's result naming any product but base_color
    // refuses HERE, so the prohibition holds at the boundary the planes cross rather than as advice on the card; a
    // partition count past the card's declared bound and a non-finite or over-ceiling residual refuse the same way; a
    // result short of the card's declared outputs refuses rather than yielding a partial set a consumer completes
    // with neutrals it would then read as measured; and every channel output's extent proves against the REQUEST's
    // declared output extent — the freeze's threads-through-the-plan law read back at the boundary the planes cross.
    public static Fin<StageResult> Admit(StageResult result, ModelCard card, StageRequest request, Op key) =>
        from _ in guard(result.ModelCardId == card.Id,
                MaterialFault.Parameter(key, $"<stage-card-mismatch:{result.ModelCardId.Value}!={card.Id.Value}>"))
        from _op in guard(result.Op == request.Op,
                MaterialFault.Parameter(key, $"<stage-op-mismatch:{card.Id.Value}>"))
        from __ in guard(result.Outputs.ForAll(output => card.Permits(output.Role)),
                MaterialFault.Parameter(key, $"<stage-product-forbidden:{card.Id.Value}>"))
        from ___ in guard(card.Contract.Outputs.ForAll(binding => result.Outputs.Exists(o => o.Role.Key == binding.Product.Key)),
                MaterialFault.Parameter(key, $"<stage-outputs-short:{card.Id.Value}:{result.Outputs.Count}>"))
        from _extent in guard(result.Outputs.Filter(static o => o.Role is StageProduct.Channel)
                    .ForAll(o => o.Width == request.OutputWidth && o.Height == request.OutputHeight),
                MaterialFault.Parameter(key, $"<stage-extent-mismatch:{card.Id.Value}:{request.OutputWidth.Value}x{request.OutputHeight.Value}>"))
        from ____ in guard(result.PartitionCount <= card.PartitionBound,
                MaterialFault.Parameter(key, $"<stage-partition-bound:{result.PartitionCount}/{card.PartitionBound}>"))
        from _____ in guard(double.IsFinite(result.GoldenDelta) && result.GoldenDelta <= card.ResidualCeiling,
                MaterialFault.Parameter(key, $"<stage-golden-delta:{result.GoldenDelta:R}/{card.ResidualCeiling:R}>"))
        select result;

    // Planes projects the set-bound half of a result: priors feed the next stage and belong in no TextureSet. The
    // acquisition#ACQUISITION CaptureSource.NeuralPlanes arm reads THIS projection — the frozen prior-drop site —
    // and never filters the union itself.
    public Seq<StageOutput> Planes => Outputs.Filter(static output => output.Role is StageProduct.Channel);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class StagePlan {
    // ONE planning fold: cover the demand, close the dependencies, append the refinement chain, then thread the
    // extent through the ordered sequence binding each stage to its executed prefix.
    public static Fin<Seq<StageRequest>> Plan(StageIntent intent, Op key) =>
        from covered in Cover(intent.Requested, intent.Policy, key)
        from ordered in Refine(Closure(covered, intent.Policy), intent, key)
        from requests in Thread(ordered, intent, key)
        select requests;

    // GREEDY SET COVER over the SERVICEABLE cover rows — a stage counts only when its whole consume-closure holds a
    // granting card at the caller's ceiling, so the licence gate shapes the route: with the delighting artefacts
    // blocked, a base_color demand reaches the intrinsic-appearance estimator rather than refusing three stages deep
    // on a chain the fold could see was ungranted. Each round takes the stage answering the most still-uncovered
    // channels, declaration order breaking ties. A channel no serviceable row emits is a DECLARED gap named with the
    // ceiling that closed it, never a silent short output set.
    static Fin<Seq<PbrStage>> Cover(Seq<TextureChannel> requested, StagePolicy policy, Op key) {
        Seq<TextureChannel> orphans = requested.Filter(channel => !PbrStage.Items.Any(stage =>
            stage.Selection == StageSelection.Cover && ModelRegistry.Serviceable(stage, policy)
            && stage.EmitsProduct(new StageProduct.Channel(channel))));
        return orphans.IsEmpty
            ? Fin.Succ(Greedy(requested, policy, Seq<PbrStage>()))
            : MaterialFault.Parameter(key,
                  $"<stage-channel-unproduced:{string.Join(',', orphans.Map(static c => c.Key))}@{policy.Ceiling.Key}>");
    }

    static Seq<PbrStage> Greedy(Seq<TextureChannel> uncovered, StagePolicy policy, Seq<PbrStage> chosen) =>
        uncovered.IsEmpty
            ? chosen
            : toSeq(PbrStage.Items)
                .Filter(stage => stage.Selection == StageSelection.Cover && !chosen.Exists(c => c == stage)
                              && ModelRegistry.Serviceable(stage, policy))
                .Map(stage => (Stage: stage, Gain: uncovered.Count(channel => stage.EmitsProduct(new StageProduct.Channel(channel)))))
                .Filter(static candidate => candidate.Gain > 0)
                .OrderByDescending(static candidate => candidate.Gain)
                .ThenBy(static candidate => candidate.Stage.Ordinal)
                .HeadOrNone()
                .Match(
                    Some: best => Greedy(uncovered.Filter(channel => !best.Stage.EmitsProduct(new StageProduct.Channel(channel))), policy, chosen.Add(best.Stage)),
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
                          toSeq(PbrStage.Items)
                              .Filter(candidate => candidate.Selection == StageSelection.Cover
                                                && candidate.EmitsProduct(product) && ModelRegistry.Serviceable(candidate, policy))
                              .OrderBy(static candidate => candidate.Ordinal)
                              .HeadOrNone()
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
        (Seq<PbrStage> plan, int remaining) = toSeq(PbrStage.Items)
            .Filter(stage => stage.Selection == StageSelection.Refine && ModelRegistry.Granted(stage, intent.Policy))
            .Filter(stage => stage.Consumes().ForAll(product => ModelRegistry.EmitterOf(ordered, product).IsSome))
            .OrderBy(static stage => stage.Ordinal)
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
    static Fin<Seq<StageRequest>> Thread(Seq<PbrStage> ordered, StageIntent intent, Op key) =>
        ordered.Fold(
            Fin.Succ((Requests: Seq<StageRequest>(), Prefix: Seq<PbrStage>(), Width: intent.Width, Height: intent.Height)),
            (state, stage) => state.Bind(carried =>
                from card in ModelRegistry.Select(stage, intent.Policy, key)
                from tiles in TilePlan.Of(carried.Width, carried.Height, card.Contract, key)
                from request in StageRequest.Of(card, intent, carried.Prefix, carried.Width, carried.Height, tiles, key)
                select (Requests: carried.Requests.Add(request), Prefix: carried.Prefix.Add(stage),
                        Width: request.OutputWidth, Height: request.OutputHeight)))
        .Map(static carried => carried.Requests.Strict());
}
```

## [04]-[RESEARCH]

- [MODEL_TENSOR_NAMES]-[OPEN]: what input and output tensor names, layout tokens, and fixed shape buckets does each registered ONNX artefact publish, and does any card need a second input beyond its declared one?; verify by reading each artefact's own graph metadata at the app-root import boundary and bake the answers into the `tensor:product` and `tensor@product` row specs.
- [DELIT_PLANE_SHAPE]-[OPEN]: does the delighting artefact emit a display-referred sRGB image or a scene-linear plane, and is sixteen-bit integer the depth its output warrants against a float carrier?; verify against the artefact's own output tensor dtype and range on a fixed input, and correct the `PriorField.Delit` format and transfer columns.
- [SUPER_RESOLVE_LICENSE]-[OPEN]: which licence do the `realesr-general-x4v3` and `span-x4` weight cards declare, as distinct from their source repositories?; verify against the published weight cards and correct the `LicenseId` and `LicenseClass` columns, moving either to `Blocked` if its card is silent.
- [GOLDEN_RESIDUAL_CEILING]-[OPEN]: do the per-card `ResidualCeiling` values hold against measured CPU-versus-accelerator divergence, given that a delighting model, a depth model, and an up-sampler produce outputs on different scales?; verify by measuring each card's residual on a fixed input and replace each row's value.
- [DEPTH_PRIOR_COUPLING]-[OPEN]: which constraint form does the `Raster/filter#PLANE_OP` `HeightFromNormal` case take for a `PriorField.Depth` plane — a low-frequency additive anchor, a boundary condition on the Poisson solve, or a post-integration affine fit?; verify against the landed filter page and bind the prior at that page's own signature in the same pass.
