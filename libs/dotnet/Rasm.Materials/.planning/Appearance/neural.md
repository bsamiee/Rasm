# [MATERIALS_NEURAL]

Materials OWNS the photo-to-PBR stage vocabulary and SPECIFIES inference; `Rasm.Compute` EXECUTES it: one `ModelCard` frozen registry keyed by `ModelCardId` carries every admitted model as DATA — stage, licence class, weight policy, weight-artefact digest, tensor contract, shape buckets, execution-provider ladder, partition bound, residual ceiling, and the physical-channel prohibition — so admitting or retiring a model is a ROW and no surface moves. One `StageProduct` `[Union]` closes what a stage can emit AND what a caller demands: a frozen `Raster/set#TEXTURE_SET` `TextureChannel` that lands as a plane, or a `PriorField` intermediate that feeds another stage and never reaches a set. `PbrStage` rows declare only what they CONSUME and what they EMIT, so the dependency relation is DERIVED — one greedy cover over the requested products, one fixpoint closure over the consume-emit relation, one refinement pass keyed on the requested output extent — and `StagePlan.Plan` folds that relation into the dependency-ordered `Seq<StageStep>` the `[WIRE]` seam carries, each stage's input bound to its PRODUCER rather than to the source photo and each step carrying the settled result a `StageReplay` consult already holds. `LicenseClass` closes the grant vocabulary and its `Blocked` row REFUSES at request construction rather than at execution; `StageResult`/`StageOutput` carry every produced plane back with the provider the session reached, the graph partition count, the golden-output residual, and the digest of the weight bytes the executor loaded as typed evidence. `ModelCard` admits a new model as one ROW, `LicenseClass` a new licence posture, `PriorField` a new intermediate, and `PbrStage` a new inference stage declaring its consumed and emitted products — never a per-model type, a hardcoded model name inside a stage, a hand-listed pipeline order, or a boolean gate standing in for a grant. `ModelRegistry` and `StagePlan` compose the `Raster/set#TEXTURE_SET` `TextureChannel` roster as their channel vocabulary, the `Raster/plane#PLANE_FORMAT` `PlaneFormat`/`PlaneTransfer` bands, the seam `Rasm.Element` `ContentAddress` for every plane blob, the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail, and the kernel `Dimension`/`Op` atoms — re-minting no channel vocabulary, no plane format, no hash, and no fault. Height is NOT a stage and NOT a prior: it integrates from `geometry_normal` under a `PriorField.Depth` low-frequency anchor as a `Raster/filter#PLANE_OP`, pure math with no model. Tiling is NOT a stage: `Raster/tile#TILE_SYNTH` owns seam coherence procedurally. Text-to-material generation is NOT a stage: it is an external-service seam this page rules out of the in-process registry.

## [01]-[INDEX]

- [02]-[MODEL_REGISTRY]: `ModelRegistry` freezes the `ModelCard` row table with its selection fold and its accumulating `Census` declaration gate, `StageRelation` derives the consume-emit correspondence both directions read, over `PriorField`/`ScoreField`/`StageProduct` the emission vocabulary, `PbrStage` the stage family and its consume-emit declaration, `TensorContract` the graph-shape carrier with `StageBinding` its output rows, and the `LicenseClass`/`WeightPolicy`/`InferenceProvider`/`TensorPrecision`/`ProviderTrait`/`ModelTrait` bands.
- [03]-[STAGE_PLAN]: `StagePlan` resolves the `StageIntent`/`StagePolicy` request shapes through cover-closure-refine into the `StageStep` sequence carrying the `StageRequest`/`StageResult`/`StageOutput` wire records the `Rasm.Compute` execution seam moves, `StageInput` binding each stage to its producer, `InferenceTiling` its tiling, and `StageReplay` the settled-evidence consult.
- [04]-[RESEARCH]: open epistemic debt with its verification route.

## [02]-[MODEL_REGISTRY]

- Owner: `ModelRegistry` the frozen `ModelCard` table keyed by `ModelCardId` with its `Census` declaration gate; `StageRelation` the derived consume-emit index both directions read; `PbrStage`/`PriorField`/`ScoreField`/`StageSelection`/`LicenseClass`/`WeightPolicy`/`InferenceProvider`/`TensorPrecision`/`ProviderTrait`/`ModelTrait` `[SmartEnum]` bands; `StageProduct` `[Union]` the emission vocabulary; `TensorContract` the graph-shape carrier with `StageBinding` its output rows and `LatentInput` its seed-driven input; `ResidualBand` the divergence band; `ModelCard` the row.
- Cases: stage {`Delight`, `Albedo`, `Normals`, `Depth`, `Svbrdf`, `IntrinsicAppearance`, `SpectralReflectance`, `SuperResolve`, `Tileability`}; product {`Channel`, `Prior`, `Measure`}; prior {`Delit`, `Depth`, `Spectral`}; score {`Tileability`}; selection {`Cover`, `Refine`}; licence {`Permissive`, `Copyleft`, `OpenRail`, `Research`, `Blocked`}; weight {`Redistributable`, `CallerSupplied`}; provider {`Cpu`, `CoreMl`, `WebGpu`} over trait {`Terminal`, `PinsFormat`}; model trait {`PhysicalChannelForbidden`, `Stochastic`}; precision {`Fp32`, `Fp16`}.
- Entry: `public static Fin<ModelCard> Select(PbrStage stage, StagePolicy policy, Op key)` is the ONE selection fold — the requested stage, the caller's licence ceiling, and the provider preference resolve one card off the frozen rows, so a stage never names a model and a model swap is a row edit. `Select` rails `MaterialFault.Parameter` naming the stage on an unregistered stage, a stage whose every card exceeds the ceiling, or a `Blocked` card pinned explicitly — a stage with no admitted card is DECLARED absence, never a silent skip. `ModelRegistry.Census()` is the ONE declaration gate over the whole table, ACCUMULATING every breach so a broken row set names all of them at once; the type initializer is its first reader and the only construction on this page where the language offers no rail.
- Law: a stage declares CONSUMES and EMITS and NOTHING else. `StageRelation` derives every dependency edge from the relation between them — one type-init index over the roster, forward (emitters by product) and inverse (a stage's own emitted keys) on ONE owner — so the plan's cover, closure, refine, and binding folds all read one lookup rather than re-walking `Items` per query, the order is derived rather than authored, and a stage carrying a `Requires` list contradicting its own inputs is unrepresentable. Widening a model's graph to a second input moves one `TensorContract` column and one `Consumes` row, with no fold edit.
- Law: `Delight` emits `PriorField.Delit` and NOT `base_color` — a de-lit photograph is a de-shadowed sRGB observation still carrying view-dependent residue, so publishing it as a base-colour plane seats an intermediate the shading model then reads as measured reflectance. `Depth` emits `PriorField.Depth`, the monocular relative-inverse-depth prior anchoring the low-frequency ambiguity Frankot-Chellappa integration cannot recover; `height` stays the `Raster/filter#PLANE_OP` product `Raster/set#TEXTURE_SET` declares `Derived("geometry_normal")`, so no stage emits it and no card claims it. `SpectralReflectance` emits `PriorField.Spectral`, the per-wavelength `(η, k)` curve `surface#CONDUCTOR_IOR` grounds a metal from — a SET-LEVEL column beside `ConductorMetal`'s three-band `ComplexIor`, a substance fact one material carries and never a per-texel channel.
- Law: FOUR STAGES AND THE REFINE PATH ARE UNREACHABLE TODAY, and the registry says so in ROWS rather than in silence. `Albedo`, `SpectralReflectance`, `Svbrdf`, and `Tileability` carry only `Blocked` cards and both `SuperResolve` rows are `Blocked`, so `Serviceable` answers false at every ceiling and the cover fold plans no route through them. That is the honest capability statement: the vocabulary, the plan algebra, the contract, and the admission gate are complete and exercised by the granted stages, and each blocked stage arms as ONE row edit the moment its weight card publishes a licence.
- Law: `ModelCard.Artefact` and `ModelCard.Residual` fill by MEASUREMENT, never by estimate. No redistributable weight artefact ships yet, so every current row carries typed absence and the `WeightPolicy` census below proves that absence rather than leaving an unfilled column. Divergence is a property of a graph RUNNING on a provider, so each card's band carries its DECLARED ceiling until that card's first `Rasm.Compute` execution measures it — `StageResult.GoldenDelta` itself, one fixed input across every listed provider filling `Upper` at `ResidualBand.Ceiling` for a deterministic card, a seed sweep filling `Lower` at `ResidualBand.Point` for a stochastic one. Estimating a ceiling ahead of execution inverts the column's own law by writing an unmeasured number into the slot a measurement gates.
- Law: the weights gate is licence-class DATA, never code — anything an OSS project may freely use admits as a row (permissive, copyleft, OpenRAIL, research-class alike) and only a payment-gated model rejects outright, while a model whose WEIGHT card is silent about its licence enters as a `Blocked` row: the artefact is registered so the estate records what exists, and the row carries NO grant to run it. `LicenseClass.Grants` is the one predicate every gate reads; a boolean beside the class is a second truth two folds read differently.
- Law: `ModelTrait.PhysicalChannelForbidden` is the generative-super-resolution law as data — a card holding it may only emit `Channel(base_color)`, so a `superResolve` result naming any other product refuses at decode. Generative up-sampling invents plausible high-frequency detail; on an albedo that is acceptable authoring, on a normal, roughness, metalness, or height plane it is fabricated physics the shading model then integrates as if measured. The trait rides `CapabilitySet<ModelTrait>` beside `Stochastic` rather than as a bool per behaviour, so a third model property is one ROW and no signature moves.
- Law: `WeightPolicy` states whether the estate MAY carry an artefact and carries NO address — the app-root import boundary resolves `ModelCardId` to bytes for both rows alike, so the registry stays a vocabulary and never a distribution channel. That column GOVERNS rather than describes: `ModelRegistry.Census` proves the correspondence both ways, so a `Redistributable` row with an absent `Artefact` states a claim it has not filled and a `CallerSupplied` row carrying one forges a deployment fact the registry cannot know. Nothing reading a row leaves decorative density; this conjunct reads it.
- Packages: Rasm (project — `Dimension`/`Op`), Rasm.Element (project — `ContentAddress`), Rasm.Materials.Appearance.Bsdf (`MaterialFault` band 2450), Rasm.Materials.Raster (`TextureChannel`/`PlaneFormat`/`PlaneTransfer` — the frozen channel and plane vocabularies this page projects, never re-declares), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum]`/`[ValueObject<string>]` with the folder's `ComparerAccessors.StringOrdinal` key policy, and the `TryGet` lift onto `Option` the catalog's own guidance pins), LanguageExt.Core (`Fin`/`Seq`/`Option`), BCL inbox (`FrozenDictionary`). NO inference package is composed here — `Microsoft.ML.OnnxRuntime` is `Rasm.Compute`'s and this owner's strata rank forbids the reference.
- Growth: each new model lands one `ModelCard` row; a new licence posture one `LicenseClass` row with its `Grants` column; a new execution provider one `InferenceProvider` row with its refusal semantics; a new intermediate one `PriorField` row carrying its plane shape; a new stage one `PbrStage` row declaring its consumed and emitted products, with one card row per stage the type-initialization census demands. A stage carries as many cards as the estate admits: today `Svbrdf` and `SuperResolve` carry two rows each and the rest one, with `Blocked` rows recording artefacts that exist without a grant — the multi-card stages are what the selection fold discriminates and a richer census is row growth, never a fold edit.
- Boundary: text-to-material generation is an EXTERNAL-SERVICE SEAM, not a registry row. `Raster/tile#TILE_SYNTH` coherence gates every set the estate holds, and the one locally-runnable candidate loses its tileability at export, so a generated set fails that gate; a service-produced set therefore enters through `Raster/set#SET_INGEST` classification like any other third-party asset, carrying its provenance and its licence class as ingest evidence, and no stage, card, or provider row represents it.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Projection;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Raster;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PriorField {
    public static readonly PriorField Delit = new("delit", format: PlaneFormat.Rgba16, transfer: PlaneTransfer.Srgb);

    public static readonly PriorField Depth = new("depth", format: PlaneFormat.R32F, transfer: PlaneTransfer.Raw);

    public static readonly PriorField Spectral = new("spectral", format: PlaneFormat.R32F, transfer: PlaneTransfer.Raw);

    public PlaneFormat Format { get; }
    public PlaneTransfer Transfer { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScoreField {
    public static readonly ScoreField Tileability = new("tileability");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StageProduct {
    private StageProduct() { }

    public sealed record Channel(TextureChannel Field) : StageProduct;
    public sealed record Prior(PriorField Field) : StageProduct;
    public sealed record Measure(ScoreField Field) : StageProduct;

    public string Key => Switch(channel: static c => c.Field.Key, prior: static p => p.Field.Key, measure: static m => m.Field.Key);

    public Option<PlaneFormat> Format =>
        Switch(channel: static c => Some(Storage(c.Field)), prior: static p => Some(p.Field.Format), measure: static _ => Option<PlaneFormat>.None);
    public Option<PlaneTransfer> Transfer =>
        Switch(channel: static c => Some(c.Field.Transfer), prior: static p => Some(p.Field.Transfer), measure: static _ => Option<PlaneTransfer>.None);

    public static Option<StageProduct> Parse(string key) =>
        TextureChannel.TryGet(key, out TextureChannel? channel)
            ? Some<StageProduct>(new Channel(channel))
            : PriorField.TryGet(key, out PriorField? prior)
                ? Some<StageProduct>(new Prior(prior))
                : ScoreField.TryGet(key, out ScoreField? score)
                    ? Some<StageProduct>(new Measure(score))
                    : None;

    static PlaneFormat Storage(TextureChannel channel) =>
        PlaneFormat.For(channel.Components, ChannelDtype.Unorm16).IfNone(PlaneFormat.Rgba16);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StageSelection {
    public static readonly StageSelection Cover = new("cover");
    public static readonly StageSelection Refine = new("refine");
}

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
    public static readonly PbrStage Tileability = new("tileability", StageSelection.Cover, scale: 1,
        consumes: static () => Seq<StageProduct>(new StageProduct.Channel(TextureChannel.BaseColor)),
        emits: static () => Seq<StageProduct>(new StageProduct.Measure(ScoreField.Tileability)));

    public StageSelection Selection { get; }

    public int Scale { get; }

    [UseDelegateFromConstructor]
    public partial Seq<StageProduct> Consumes();
    [UseDelegateFromConstructor]
    public partial Seq<StageProduct> Emits();

    private static readonly Lazy<FrozenDictionary<PbrStage, int>> Order =
        new(static () => Items.Select(static (stage, index) => (stage, index)).ToFrozenDictionary(static e => e.stage, static e => e.index));

    public int Ordinal => Order.Value[this];
}

internal static class StageRelation {
    private static readonly Lazy<FrozenDictionary<string, Seq<PbrStage>>> Forward =
        new(static () => PbrStage.Items
            .SelectMany(static stage => stage.Emits().Map(product => (Key: product.Key, Stage: stage)))
            .GroupBy(static edge => edge.Key)
            .ToFrozenDictionary(static group => group.Key,
                static group => toSeq(group.OrderBy(static edge => edge.Stage.Ordinal).Select(static edge => edge.Stage)).Strict(),
                StringComparer.Ordinal));

    private static readonly Lazy<FrozenDictionary<PbrStage, FrozenSet<string>>> Inverse =
        new(static () => PbrStage.Items.ToFrozenDictionary(static stage => stage,
            static stage => stage.Emits().Map(static product => product.Key).ToFrozenSet(StringComparer.Ordinal)));

    public static Seq<PbrStage> Emitters(StageProduct product) =>
        Forward.Value.TryGetValue(product.Key, out Seq<PbrStage> stages) ? stages : Seq<PbrStage>();

    public static bool Emits(PbrStage stage, StageProduct product) => Inverse.Value[stage].Contains(product.Key);

    public static Seq<PbrStage> Covering(StageProduct product, StagePolicy policy) =>
        Emitters(product).Filter(stage => stage.Selection == StageSelection.Cover && ModelRegistry.Serviceable(stage, policy));
}

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

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WeightPolicy {
    public static readonly WeightPolicy Redistributable = new("redistributable");
    public static readonly WeightPolicy CallerSupplied = new("callerSupplied");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProviderTrait : ICapability<ProviderTrait> {
    public static readonly ProviderTrait Terminal = new("terminal");
    public static readonly ProviderTrait PinsFormat = new("pins-format");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InferenceProvider {
    public static readonly InferenceProvider Cpu    = new("cpu",    order: 0, traits: CapabilitySet<ProviderTrait>.Of(ProviderTrait.Terminal));
    public static readonly InferenceProvider CoreMl = new("coreMl", order: 1, traits: CapabilitySet<ProviderTrait>.Of(ProviderTrait.PinsFormat));
    public static readonly InferenceProvider WebGpu = new("webGpu", order: 2, traits: CapabilitySet<ProviderTrait>.None);
    public int Order { get; }
    public CapabilitySet<ProviderTrait> Traits { get; }
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
public readonly partial struct ModelCardId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = !string.IsNullOrWhiteSpace(value) && value.Length <= 128
            ? null
            : new ValidationError($"<model-card-id-blank-or-long:{value?.Length ?? 0}>");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct StageBinding(string Tensor, int Lane, StageProduct Product, Option<PlaneTransfer> Transfer, Option<PlaneFormat> Format);

public readonly record struct LatentInput(string Tensor, int Channels, int Downscale);

public sealed record TensorContract(
    Seq<string> Inputs, string Layout, Seq<(Dimension Width, Dimension Height)> Buckets, int Overlap,
    Seq<StageBinding> Outputs, Option<LatentInput> Latent) {

    public string TensorLayout => Layout;

    public Option<(Dimension Width, Dimension Height)> BucketFor(Dimension width, Dimension height) =>
        toSeq(Buckets.Filter(b => b.Width.Value >= width.Value && b.Height.Value >= height.Value)
                .OrderBy(static b => b.Width.Value * b.Height.Value))
            .Head
            .Match(Some, () => toSeq(Buckets.OrderByDescending(static b => b.Width.Value * b.Height.Value)).Head);
}

public readonly record struct ResidualBand(Option<double> Lower, double Upper) {
    public static ResidualBand Point(double ceiling) => new(Some(ceiling), ceiling);
    public static ResidualBand Ceiling(double upper) => new(Option<double>.None, upper);
    public bool Admits(double delta) => double.IsFinite(delta) && delta <= Upper;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModelTrait : ICapability<ModelTrait> {
    public static readonly ModelTrait PhysicalChannelForbidden = new("physical-channel-forbidden");
    public static readonly ModelTrait Stochastic = new("stochastic");
}

public sealed record ModelCard(
    ModelCardId Id, PbrStage Stage, LicenseClass License, string LicenseId, WeightPolicy Weights, Option<ContentAddress> Artefact,
    TensorContract Contract, Seq<InferenceProvider> Providers, TensorPrecision Precision, int PartitionBound,
    ResidualBand Residual, CapabilitySet<ModelTrait> Traits) {

    public bool Admits(LicenseClass ceiling) => License.Grants && License.Rank <= ceiling.Rank;

    public bool Permits(StageProduct product) =>
        (!Traits.Admits(ModelTrait.PhysicalChannelForbidden) || (product is StageProduct.Channel colour && colour.Field == TextureChannel.BaseColor))
        && Contract.Outputs.Exists(binding => binding.Product.Key == product.Key);

    public Option<(Option<PlaneTransfer> Transfer, Option<PlaneFormat> Format)> Shaped(StageProduct product) =>
        Contract.Outputs.Find(binding => binding.Product.Key == product.Key).Map(static b => (b.Transfer, b.Format));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ModelRegistry {
    public static readonly FrozenDictionary<ModelCardId, ModelCard> Rows = Seq(
        Card("stable-delight-yoso-v0-4-base", PbrStage.Delight, LicenseClass.Permissive, "apache-2.0", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["input"], [("latent", Prior(PriorField.Delit))], buckets: [(512, 512)], overlap: 16),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 2e-3, traits: CapabilitySet<ModelTrait>.Of(ModelTrait.Stochastic)),
        Card("supermat-albedo", PbrStage.Albedo, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["input"], [("albedo", Channel(TextureChannel.BaseColor))], buckets: [(512, 512)], overlap: 16),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 2e-3, traits: CapabilitySet<ModelTrait>.None),
        Card("lotus-d-normal-v1-1", PbrStage.Normals, LicenseClass.Permissive, "apache-2.0", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["image"], [("normal", Channel(TextureChannel.GeometryNormal))], buckets: [(512, 512)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 1e-3, traits: CapabilitySet<ModelTrait>.None),
        Card("depth-anything-v2-small", PbrStage.Depth, LicenseClass.Permissive, "apache-2.0", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["image"], [("depth", Prior(PriorField.Depth))], buckets: [(518, 518)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 5e-3, traits: CapabilitySet<ModelTrait>.None),
        Card("inria-unet-hf-svbrdf", PbrStage.Svbrdf, LicenseClass.Permissive, "mit", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["input"],
                [("albedo", Channel(TextureChannel.BaseColor)), ("roughness", Channel(TextureChannel.SpecularRoughness)),
                 ("metallic", Channel(TextureChannel.BaseMetalness)), ("normal", Channel(TextureChannel.GeometryNormal))],
                buckets: [(256, 256), (512, 512)], overlap: 32),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 1e-3, traits: CapabilitySet<ModelTrait>.None),
        Card("marigold-iid-appearance-v1-1", PbrStage.IntrinsicAppearance, LicenseClass.OpenRail, "openrail++-m", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["image"],
                [("albedo", Channel(TextureChannel.BaseColor)), ("material", Channel(TextureChannel.SpecularRoughness)),
                 ("material", Channel(TextureChannel.BaseMetalness))],
                buckets: [(768, 768)], overlap: 32, latent: new LatentInput("latent", Channels: 8, Downscale: 8)),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 2e-3, traits: CapabilitySet<ModelTrait>.Of(ModelTrait.Stochastic)),
        Card("realesr-general-x4v3", PbrStage.SuperResolve, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["input"], [("output", Channel(TextureChannel.BaseColor))], buckets: [(256, 256), (512, 512)], overlap: 16),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 2, ceiling: 5e-3, traits: CapabilitySet<ModelTrait>.Of(ModelTrait.PhysicalChannelForbidden)),
        Card("span-x4", PbrStage.SuperResolve, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["input"], [("output", Channel(TextureChannel.BaseColor))], buckets: [(256, 256)], overlap: 16),
            [InferenceProvider.CoreMl, InferenceProvider.Cpu], TensorPrecision.Fp16, partitions: 1, ceiling: 8e-3, traits: CapabilitySet<ModelTrait>.Of(ModelTrait.PhysicalChannelForbidden)),
        Card("spectral-reflectance-unshipped", PbrStage.SpectralReflectance, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["input"], [("spectrum", Prior(PriorField.Spectral))], buckets: [(512, 512)], overlap: 32),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 1e-3, traits: CapabilitySet<ModelTrait>.None),
        Card("textile-tileability", PbrStage.Tileability, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["input"], [("score", Measure(ScoreField.Tileability))], buckets: [(512, 512)], overlap: 16),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 1e-3, traits: CapabilitySet<ModelTrait>.None),
        Card("mate-unified", PbrStage.Svbrdf, LicenseClass.Blocked, "absent", WeightPolicy.CallerSupplied, artefact: None,
            Contract(["input"],
                [("albedo", Channel(TextureChannel.BaseColor)), ("roughness", Channel(TextureChannel.SpecularRoughness)),
                 ("metallic", Channel(TextureChannel.BaseMetalness)), ("normal", Channel(TextureChannel.GeometryNormal))],
                buckets: [(512, 512)], overlap: 32),
            [InferenceProvider.Cpu], TensorPrecision.Fp32, partitions: 1, ceiling: 1e-3, traits: CapabilitySet<ModelTrait>.None))
        .ToFrozenDictionary(static card => card.Id);

    public static Validation<Error, Unit> Census() =>
        toSeq(PbrStage.Items).Traverse(Stage).Map(static _ => unit)
            .Apply(toSeq(Rows.Values).Traverse(Card).Map(static _ => unit), static (_, _) => unit).As();

    static Validation<Error, Unit> Stage(PbrStage stage) =>
        Breach(Rows.Values.Any(card => card.Stage == stage), $"<stage-uncarded:{stage.Key}>")
            .Apply(Breach(stage.Consumes().ForAll(product =>
                    StageRelation.Emitters(product).Exists(prior =>
                        prior.Ordinal < stage.Ordinal && prior.Selection == StageSelection.Cover)),
                $"<stage-forward-consumes:{stage.Key}>"), static (_, _) => unit).As();

    static Validation<Error, Unit> Card(ModelCard card) =>
        Breach(card.Contract.Outputs.ForAll(binding => StageRelation.Emits(card.Stage, binding.Product)),
                $"<stage-contract-overemits:{card.Id.Value}>")
            .Apply(Breach(card.Contract.Inputs.Count == Math.Max(1, card.Stage.Consumes().Count),
                $"<stage-contract-arity:{card.Id.Value}:{card.Contract.Inputs.Count}>"), static (_, _) => unit).As()
            .Apply(Breach(card.Contract.Overlap is >= FeatherFloor and <= FeatherCeiling,
                $"<model-overlap-band:{card.Id.Value}:{card.Contract.Overlap}>"), static (_, _) => unit).As()
            .Apply(Breach(card.Contract.Buckets.Count <= 1
                    || !card.Contract.Outputs.Exists(static binding => binding.Product is StageProduct.Measure),
                $"<model-measure-multi-bucket:{card.Id.Value}:{card.Contract.Buckets.Count}>"), static (_, _) => unit).As()
            .Apply(Breach(card.Contract.Latent.ForAll(seeded =>
                    card.Contract.Buckets.ForAll(b => b.Width.Value % seeded.Downscale == 0 && b.Height.Value % seeded.Downscale == 0)),
                $"<model-latent-bucket:{card.Id.Value}>"), static (_, _) => unit).As()
            .Apply(Breach(card.Weights == WeightPolicy.Redistributable == card.Artefact.IsSome,
                $"<model-weight-artefact:{card.Id.Value}:{card.Weights.Key}>"), static (_, _) => unit).As();

    static Validation<Error, Unit> Breach(bool held, string token) =>
        held
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new MaterialFault.Parameter(Op.Of(name: "model-registry"), token));

    const int FeatherFloor = 8, FeatherCeiling = 32;

    static ModelRegistry() =>
        Census().IfFail(static faults => throw faults.ToException());

    public static Fin<ModelCard> Select(PbrStage stage, StagePolicy policy, Op key) =>
        policy.PinnedCard.Match(
            Some: id => Rows.TryGetValue(id, out ModelCard? pinned) && pinned.Stage == stage
                ? Admissible(pinned, policy, key)
                : new MaterialFault.Parameter(key, $"<model-card-unpinned:{id.Value}:{stage.Key}>"),
            None: () => toSeq(toSeq(Rows.Values)
                    .Filter(card => card.Stage == stage && card.Admits(policy.Ceiling))
                    .OrderBy(card => card.Providers.Exists(p => p == policy.Preferred) ? 0 : 1)
                    .ThenBy(static card => card.PartitionBound)
                    .ThenBy(static card => card.License.Rank))
                .Head
                .ToFin(new MaterialFault.Parameter(key, $"<model-stage-unserved:{stage.Key}@{policy.Ceiling.Key}>")));

    public static Option<PbrStage> EmitterOf(Seq<PbrStage> prefix, StageProduct product) =>
        prefix.Filter(stage => StageRelation.Emits(stage, product)).Last;

    public static bool Granted(PbrStage stage, StagePolicy policy) =>
        Rows.Values.Any(card => card.Stage == stage && card.Admits(policy.Ceiling));

    static readonly ConcurrentDictionary<LicenseClass, Lazy<FrozenDictionary<PbrStage, bool>>> ServiceTable = new();

    public static bool Serviceable(PbrStage stage, StagePolicy policy) =>
        ServiceTable.GetOrAdd(policy.Ceiling, static ceiling =>
            new Lazy<FrozenDictionary<PbrStage, bool>>(() => Close(ceiling))).Value[stage];

    static FrozenDictionary<PbrStage, bool> Close(LicenseClass ceiling) =>
        toSeq(PbrStage.Items.OrderBy(static s => s.Ordinal))
            .Fold(HashMap<PbrStage, bool>(), (table, stage) => table.Add(stage,
                Rows.Values.Any(card => card.Stage == stage && card.Admits(ceiling))
                    && stage.Consumes().ForAll(product => StageRelation.Emitters(product).Exists(emitter =>
                           emitter.Ordinal < stage.Ordinal && emitter.Selection == StageSelection.Cover
                           && table.Find(emitter).IfNone(false)))))
            .ToFrozenDictionary(static entry => entry.Key, static entry => entry.Value);

    static Fin<ModelCard> Admissible(ModelCard card, StagePolicy policy, Op key) =>
        card.Admits(policy.Ceiling)
            ? Fin.Succ(card)
            : new MaterialFault.Parameter(key, $"<model-card-ungranted:{card.Id.Value}:{card.License.Key}>");

    static ModelCard Card(
        string id, PbrStage stage, LicenseClass license, string licenseId, WeightPolicy weights, Option<ContentAddress> artefact,
        TensorContract contract, InferenceProvider[] providers, TensorPrecision precision, int partitions, double ceiling,
        CapabilitySet<ModelTrait> traits) =>
        new(ModelCardId.Create(id), stage, license, licenseId, weights, artefact, contract, toSeq(providers), precision, partitions,
            traits.Admits(ModelTrait.Stochastic) ? ResidualBand.Ceiling(ceiling) : ResidualBand.Point(ceiling), traits);

    static TensorContract Contract(
        string[] inputs, (string Tensor, StageProduct Product)[] outputs, (int W, int H)[] buckets, int overlap,
        LatentInput? latent = null) =>
        new(toSeq(inputs), "nchw",
            toSeq(buckets).Map(static b => (Dimension.Create(b.W), Dimension.Create(b.H))), overlap,
            Outputs(outputs), Optional(latent));

    static Seq<StageBinding> Outputs((string Tensor, StageProduct Product)[] outputs) =>
        toSeq(outputs)
            .Fold((Bound: Seq<StageBinding>(), Seen: HashMap<string, int>()),
                static (state, row) => state.Seen.Find(row.Tensor).IfNone(0) switch {
                    var lane => (state.Bound.Add(new StageBinding(row.Tensor, lane, row.Product, row.Product.Transfer, row.Product.Format)),
                                 state.Seen.AddOrUpdate(row.Tensor, lane + 1)),
                })
            .Bound;

    static StageProduct Channel(TextureChannel field) => new StageProduct.Channel(field);
    static StageProduct Prior(PriorField field) => new StageProduct.Prior(field);
    static StageProduct Measure(ScoreField field) => new StageProduct.Measure(field);
}
```

## [03]-[STAGE_PLAN]

- Owner: `StagePlan` the planning fold; `StageIntent`/`StagePolicy` the request shapes; `StageInput` the producer binding; `InferenceTiling` the fixed-bucket tiling; `StageReplay`/`StageStep` the replay consult and the planned step it carries; `StageRequest`/`StageResult`/`StageOutput` the seam records.
- Entry: `public static Fin<Seq<StageStep>> Plan(StageIntent intent, Op key, Option<StageReplay> replay = default)` resolves the requested `StageProduct` set into the dependency-ordered step sequence — one entry for the whole plan, because a per-stage entrypoint pushes the ordering, the input binding, the extent threading, and the replay consult onto every caller; `StageResult.Admit(StageResult, ModelCard, StageRequest, Op)` is the ONE ingestion gate every returned result crosses — card echo, `Op` echo, product permission, output completeness, extent congruence, partition bound, and residual ceiling in one rail — and `InferenceTiling.Of(width, height, contract, key)` derives the tiling from the card's own bucket roster.
- Law: resolution is COVER, then CLOSURE, then REFINE, and each pass reads row data AND the licence posture. `Cover` runs a greedy set cover over the SERVICEABLE `StageSelection.Cover` rows against the requested `StageProduct` set — one demand axis over channels AND priors, so the depth anchor and the spectral curve are requestable exactly as a channel is and a prior-emitting stage is reachable rather than orphaned. SERVICEABLE means the stage's whole consume-closure holds a granting card at the caller's ceiling, so the grant gate shapes the ROUTE instead of surfacing as a refusal three stages deep; the stage covering the most still-uncovered products wins and declaration order breaks every tie.
- Law: `Closure` walks the roster in REVERSE declaration order ONCE, pulling each selected stage's consumed products back to their serviceable emitters through `StageRelation` — a single reverse pass is exact because the census asserts at type initialization that a consumed product is emitted by an earlier row. `Refine` ACCUMULATES the refinement chain against the target-over-source factor: each granted refine row whose `Scale` divides the remainder appends in declaration order, an anisotropic or unreachable target REFUSES, and a chained refinement binds its predecessor through the prefix rather than the cover stage's original.
- Law: each stage's INPUT is its PRODUCER's output, never the source photo. `StageInput.Source` carries the intent's plane for a stage consuming nothing, and `StageInput.Produced` names the emitting stage and the product for a stage consuming one, which the executor resolves against the results it already holds. Handing every stage the same source blob runs a chain whose links never touch, its albedo estimator reading the raw photograph the delighting stage exists to replace.
- Law: extent threads THROUGH the plan. `InferenceTiling` derives from the extent a stage's input carries, and the stage's own `Scale` column produces the extent its consumers see, so a four-times up-sampler's downstream tiling is correct without a caller recomputing anything and a mismatched tile grid is unrepresentable.
- Law: `LicenseClass.Blocked` REFUSES at request construction, not at execution — an explicit pin reads a blocked card's metadata alone, `StageRequest.Of` refuses every request naming it, so the grant gate sits at the earliest point holding it rather than deep inside a runtime trusting a caller's word.
- Law: tiling is FIXED-SHAPE and reflect-padded. Dynamic input shapes fragment an ORT graph into many partitions and defeat memory-pattern reuse, so `InferenceTiling` selects the card's own bucket, pads by reflection, and feathers the declared overlap; the tile grid, the overlap, the pad mode, and the warm-up bucket key all ride the request, so the executor warms one session per bucket and never re-derives geometry the plan already fixed. `InferenceTiling.Of` counts the FIRST tile whole and steps the remainder by the stride, so an extent equal to its bucket is one tile rather than two.
- Law: the `StageInfer` REPLAY consult threads into this fold and nowhere else. `HookModality.Observe` points stay decorator-only and their owners name nothing, while a `Replay` verdict enters the owner's OWN rail — a decorator wrapping `Plan` sees the whole sequence and skips no stage inside it — so the port is a `StageReplay` argument the composition root binds, keyed on the minted `StageRequest` because that record IS the identity a retained typed envelope carries. `StageResult.Admit` gates a held result against the freshly-minted request exactly as it gates a live one, so a drifted card, a short output set, an out-of-extent typed envelope, or a breached residual refuses the PLAN rather than seating a prior run's planes under this plan's extent; absence is the live path, so a composition with no replay store issues every step.
- Law: the `PriorField.Depth` prior couples to the height integration as a POST-INTEGRATION AFFINE FIT, never a boundary condition and never a low-frequency additive blend. `PriorField.Depth` carries RELATIVE INVERSE depth — scale-and-shift ambiguous by construction — so it holds no value a boundary condition imposes, and `Raster/set#TEXTURE_SET` derives `height` under `HeightSolver.Spectral`, a periodic solve that HAS no boundary to impose one on. That solve leaves exactly two degrees of freedom free, the DC term and the low-frequency ramp normals cannot constrain, so the fit is one closed-form least squares for `(a, b)` in `a·h + b` against the prior's reciprocal over the valid mask — two unknowns for two free parameters, with the fit residual riding the `HeightEvidence` the forward direction already records. Frequency-domain blending buys a cutoff knob and mixes two fields whose scales disagree; a boundary condition demands metric depth the prior does not carry.
- Law: `ResidualBand` gates a NORMALIZED residual: `GoldenDelta` is the per-channel RMS difference against the card's own CPU-provider reference, divided by the output's declared extent, so one ceiling is extent-invariant and a 4x up-sampler's ceiling means the same thing as a 512-square normal field's. Un-normalized L2 distance grows with the square root of the pixel count, which makes a single ceiling a function of the bucket rather than of the provider divergence it exists to bound. The band's SHAPE follows `Stochastic`: a deterministic card varies on the provider axis alone and its floor is its ceiling, while a stochastic card re-draws per seed and carries an absent floor until a seed sweep measures it — `Admits` reads `Upper` either way, so an unmeasured floor never weakens the gate.
- Law: `StageResult` carries typed evidence, never a bare success — `ProviderUsed` records the execution provider AFTER any policy refusal so a silent degradation reads off the result, `PartitionCount` rails when the session fragmented past the card's declared bound, `GoldenDelta` carries the residual against the model's own CPU-provider reference output so a fast-but-wrong provider is caught by measurement rather than by trust, `Artefact` carries the digest of the weight bytes the executor loaded so two revisions of one card separate on the result, and `Op` echoes the request's own with `Admit` comparing the pair, so a failure correlates to the plan that issued it and a transposed result refuses.
- Law: a returned PLANE proves its DECLARED SHAPE. `StageResult.Admit` reads each output's transfer and format against the `StageBinding` row that named the product, so the `PriorField` and `TextureChannel` storage columns are load-bearing end to end rather than declarations nothing reads — an executor returning a delight prior at `srgb8` or a normal field at `unorm8` refuses at the boundary the planes cross, where a plane whose own header agrees with itself carries a quantization no later gate can see.
- Boundary: the `[WIRE]` seam is `Rasm.Materials/Appearance/neural` ↔ `Rasm.Compute/Model/inference`, recorded at BOTH folder `ARCHITECTURE.md` `[03]-[SEAMS]` maps. It is C#-interior and mints NO corpus contract entry — a cross-language entry for a branch-interior hop is the fabricated contract the cross-`libs/` ruling forecloses. `StageRequest` carries CONTENT ADDRESSES and vocabulary KEYS, never plane bytes: the source plane and every produced plane live in the write-once blob store the app root binds, so the wire stays small and the executor never marshals a raster through a message.
- Boundary: `StageResult` ingestion produces the `TextureSet` the `acquisition#ACQUISITION` `CaptureSource.NeuralPlanes` arm consumes. `Prior` and `Measure` outputs DROP at that boundary — priors feed another stage or a `Raster/filter#PLANE_OP` and measures grade rather than cover — `Channel` outputs become the set's planes through the frozen `StageResult.Planes` projection, and the arm returns the SET beside the averaged row so a photo becomes a shadeable material rather than only an encodable wire. Grades reach their consumer through `StageResult.Scores`; binding one to the `Raster/tile#TILE_GATE` `TilePolicy.Scorer` closure stays the APP ROOT's hop, since that contract is a delegate over a decoded tile and this owner mints no plane bytes and holds no executor.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record StagePolicy(LicenseClass Ceiling, InferenceProvider Preferred, Option<ModelCardId> PinnedCard, TensorPrecision Precision, ulong Seed) {
    public static readonly StagePolicy Default = new(LicenseClass.Research, InferenceProvider.Cpu, None, TensorPrecision.Fp32, 0UL);
}

public sealed record StageIntent(
    ContentAddress SourceKey, Dimension Width, Dimension Height, Dimension TargetWidth, Dimension TargetHeight,
    Seq<StageProduct> Requested, StagePolicy Policy) {

    public static Fin<StageIntent> Of(
        ContentAddress source, Dimension width, Dimension height, Seq<StageProduct> requested, StagePolicy policy, Op key,
        Option<(Dimension Width, Dimension Height)> target = default) =>
        requested.IsEmpty
            ? new MaterialFault.Parameter(key, "<stage-intent-no-products>")
            : target.IfNone((width, height)) is var (tw, th) && (tw.Value < width.Value || th.Value < height.Value)
                ? new MaterialFault.Parameter(key, $"<stage-target-below-source:{tw.Value}x{th.Value}<{width.Value}x{height.Value}>")
                : Fin.Succ(new StageIntent(source, width, height, tw, th, toSeq(requested.Distinct()), policy));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StageInput {
    private StageInput() { }

    public sealed record Source(ContentAddress Key) : StageInput;
    public sealed record Produced(PbrStage Stage, StageProduct Product) : StageInput;

    public (string Stage, string Role, string Key) Wire => Switch(
        source: static s => (string.Empty, string.Empty, s.Key.ToValue()),
        produced: static p => (p.Stage.Key, p.Product.Key, string.Empty));
}

public readonly record struct InferenceTiling(Dimension TileWidth, Dimension TileHeight, int Columns, int Rows, int Overlap, string PadMode, string Bucket) {
    public static Fin<InferenceTiling> Of(Dimension width, Dimension height, TensorContract contract, Op key) =>
        contract.BucketFor(width, height)
            .ToFin(new MaterialFault.Parameter(key, $"<stage-no-bucket:{width.Value}x{height.Value}>"))
            .Map(bucket => new InferenceTiling(bucket.Width, bucket.Height,
                Columns: Steps(width.Value, bucket.Width.Value, contract.Overlap),
                Rows: Steps(height.Value, bucket.Height.Value, contract.Overlap),
                Overlap: contract.Overlap, PadMode: "reflect",
                Bucket: $"{bucket.Width.Value}x{bucket.Height.Value}"));

    static int Steps(int extent, int bucket, int overlap) =>
        extent <= bucket ? 1 : 1 + (int)Math.Ceiling((double)(extent - bucket) / Math.Max(1, bucket - overlap));
}

public sealed record StageRequest(
    PbrStage Stage, ModelCardId ModelCardId, Option<ContentAddress> Artefact, LicenseClass LicenseClass, Seq<StageInput> Inputs,
    Dimension InputWidth, Dimension InputHeight, Dimension OutputWidth, Dimension OutputHeight,
    int TileWidth, int TileHeight, int Overlap, string PadMode, string Bucket, string Layout,
    InferenceProvider Provider, TensorPrecision Precision, ulong Seed, Op Op) {

    public static Fin<StageRequest> Of(
        ModelCard card, StageIntent intent, Seq<PbrStage> prefix, Dimension width, Dimension height, InferenceTiling tiles, Op key) =>
        from _ in guard(card.License.Grants, new MaterialFault.Parameter(key, $"<stage-license-blocked:{card.Id.Value}>"))
        from _seed in guard(!card.Traits.Admits(ModelTrait.Stochastic) || intent.Policy.Seed != 0UL,
                new MaterialFault.Parameter(key, $"<stage-stochastic-seed-unset:{card.Id.Value}>"))
        from inputs in Bind(card.Stage, intent, prefix, key)
        let resolved = Preferred(card, intent.Policy)
        select new StageRequest(card.Stage, card.Id, card.Artefact, card.License, inputs, width, height,
            Dimension.Create(width.Value * card.Stage.Scale), Dimension.Create(height.Value * card.Stage.Scale),
            tiles.TileWidth.Value, tiles.TileHeight.Value, tiles.Overlap, tiles.PadMode, tiles.Bucket, card.Contract.TensorLayout,
            resolved.Provider, resolved.Precision, card.Traits.Admits(ModelTrait.Stochastic) ? intent.Policy.Seed : 0UL, key);

    static Fin<Seq<StageInput>> Bind(PbrStage stage, StageIntent intent, Seq<PbrStage> prefix, Op key) =>
        stage.Consumes().IsEmpty
            ? Fin.Succ(Seq<StageInput>(new StageInput.Source(intent.SourceKey)))
            : stage.Consumes().Traverse(product =>
                  ModelRegistry.EmitterOf(prefix, product)
                      .Map(emitter => (StageInput)new StageInput.Produced(emitter, product))
                      .ToFin(new MaterialFault.Parameter(key, $"<stage-input-unemitted:{stage.Key}:{product.Key}>"))).As()
              .Map(static bound => bound.Strict());

    static (InferenceProvider Provider, TensorPrecision Precision) Preferred(ModelCard card, StagePolicy policy) =>
        (card.Providers.Exists(p => p == policy.Preferred)
            ? policy.Preferred
            : toSeq(card.Providers.OrderBy(static p => p.Order)).Head.IfNone(InferenceProvider.Cpu)) switch {
            var chosen when !chosen.Traits.Admits(ProviderTrait.PinsFormat) || policy.Precision == card.Precision => (chosen, policy.Precision),
            _ => (toSeq(card.Providers.Filter(static p => p.Traits.Admits(ProviderTrait.Terminal) || !p.Traits.Admits(ProviderTrait.PinsFormat)).OrderBy(static p => p.Order)).Head
                      .IfNone(InferenceProvider.Cpu), policy.Precision),
        };
}

public readonly record struct StageOutput(StageProduct Role, ContentAddress BlobKey, Dimension Width, Dimension Height, PlaneTransfer Transfer, PlaneFormat Format);

public readonly record struct StageScore(StageProduct Role, double Value);

public sealed record StageResult(
    PbrStage Stage, ModelCardId ModelCardId, ContentAddress Artefact, Seq<StageOutput> Outputs, Seq<StageScore> Scores,
    InferenceProvider ProviderUsed,
    int PartitionCount, double ElapsedMs, double GoldenDelta, bool ParityFresh, float Coverage, int TilesEmitted, Op Op) {

    public static Fin<StageResult> Admit(StageResult result, ModelCard card, StageRequest request, Op key) =>
        from _ in guard(result.ModelCardId == card.Id,
                new MaterialFault.Parameter(key, $"<stage-card-mismatch:{result.ModelCardId.Value}!={card.Id.Value}>"))
        from _op in guard(result.Op == request.Op,
                new MaterialFault.Parameter(key, $"<stage-op-mismatch:{card.Id.Value}>"))
        from _art in guard(request.Artefact.Map(declared => declared == result.Artefact).IfNone(true),
                new MaterialFault.Parameter(key, $"<stage-artefact-mismatch:{card.Id.Value}:{result.Artefact.ToValue()}>"))
        from __ in guard(result.Outputs.ForAll(output => card.Permits(output.Role))
                    && result.Scores.ForAll(score => card.Permits(score.Role)),
                new MaterialFault.Parameter(key, $"<stage-product-forbidden:{card.Id.Value}>"))
        from ___ in guard(card.Contract.Outputs.ForAll(binding => binding.Product is StageProduct.Measure
                    ? result.Scores.Exists(s => s.Role.Key == binding.Product.Key)
                    : result.Outputs.Exists(o => o.Role.Key == binding.Product.Key)),
                new MaterialFault.Parameter(key, $"<stage-outputs-short:{card.Id.Value}:{result.Outputs.Count}+{result.Scores.Count}>"))
        from _extent in guard(result.Outputs.Filter(static o => o.Role is StageProduct.Channel)
                    .ForAll(o => o.Width == request.OutputWidth && o.Height == request.OutputHeight),
                new MaterialFault.Parameter(key, $"<stage-extent-mismatch:{card.Id.Value}:{request.OutputWidth.Value}x{request.OutputHeight.Value}>"))
        from _shape in guard(result.Outputs.ForAll(output =>
                    card.Shaped(output.Role).ForAll(declared =>
                        declared.Transfer.ForAll(t => t == output.Transfer) && declared.Format.ForAll(f => f == output.Format))),
                new MaterialFault.Parameter(key, $"<stage-plane-shape:{card.Id.Value}>"))
        from ____ in guard(result.PartitionCount <= card.PartitionBound,
                new MaterialFault.Parameter(key, $"<stage-partition-bound:{result.PartitionCount}/{card.PartitionBound}>"))
        from _____ in guard(card.Residual.Admits(result.GoldenDelta),
                new MaterialFault.Parameter(key, $"<stage-golden-delta:{result.GoldenDelta:R}/{card.Residual.Upper:R}>"))
        select result;

    public Seq<StageOutput> Planes => Outputs.Filter(static output => output.Role is StageProduct.Channel);

}

public delegate Option<StageResult> StageReplay(StageRequest request);

public readonly record struct StageStep(StageRequest Request, Option<StageResult> Replayed);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StagePlan {
    public static Fin<Seq<StageStep>> Plan(StageIntent intent, Op key, Option<StageReplay> replay = default) =>
        from covered in Cover(intent.Requested, intent.Policy, key)
        from ordered in Refine(Closure(covered, intent.Policy), intent, key)
        from steps in Thread(ordered, intent, replay, key)
        select steps;

    static Fin<Seq<PbrStage>> Cover(Seq<StageProduct> requested, StagePolicy policy, Op key) =>
        requested.Filter(product => StageRelation.Covering(product, policy).IsEmpty) switch {
            var orphans when orphans.IsEmpty => Fin.Succ(Greedy(requested, policy, Seq<PbrStage>())),
            var orphans => new MaterialFault.Parameter(key,
                $"<stage-product-unproduced:{string.Join(',', orphans.Map(static p => p.Key))}@{policy.Ceiling.Key}>"),
        };

    static Seq<PbrStage> Greedy(Seq<StageProduct> uncovered, StagePolicy policy, Seq<PbrStage> chosen) =>
        uncovered.IsEmpty
            ? chosen
            : toSeq(toSeq(PbrStage.Items)
                    .Filter(stage => stage.Selection == StageSelection.Cover && !chosen.Exists(c => c == stage)
                                  && ModelRegistry.Serviceable(stage, policy))
                    .Map(stage => (Stage: stage, Gain: uncovered.Count(product => StageRelation.Emits(stage, product))))
                    .Filter(static candidate => candidate.Gain > 0)
                    .OrderByDescending(static candidate => candidate.Gain)
                    .ThenBy(static candidate => candidate.Stage.Ordinal))
                .Head
                .Match(
                    Some: best => Greedy(uncovered.Filter(product => !StageRelation.Emits(best.Stage, product)), policy, chosen.Add(best.Stage)),
                    None: () => chosen);

    static Seq<PbrStage> Closure(Seq<PbrStage> seeds, StagePolicy policy) =>
        toSeq(PbrStage.Items).Rev()
            .Fold(seeds, (reached, stage) =>
                reached.Exists(s => s == stage)
                    ? stage.Consumes().Fold(reached, (inner, product) =>
                          StageRelation.Covering(product, policy).Head
                              .Match(Some: emitter => inner.Exists(s => s == emitter) ? inner : inner.Add(emitter), None: () => inner))
                    : reached) switch {
                var reachable => toSeq(PbrStage.Items).Filter(stage => reachable.Exists(s => s == stage)).Strict(),
            };

    static Fin<Seq<PbrStage>> Refine(Seq<PbrStage> ordered, StageIntent intent, Op key) {
        (int needW, int needH) = (intent.TargetWidth.Value / intent.Width.Value, intent.TargetHeight.Value / intent.Height.Value);
        if (needW != needH || intent.TargetWidth.Value != intent.Width.Value * needW || intent.TargetHeight.Value != intent.Height.Value * needH) {
            return new MaterialFault.Parameter(key, $"<stage-target-anisotropic:{intent.TargetWidth.Value}x{intent.TargetHeight.Value}>");
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
            : new MaterialFault.Parameter(key, $"<stage-target-unreachable:{intent.TargetWidth.Value}x{intent.TargetHeight.Value}:x{remaining}>");
    }

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
