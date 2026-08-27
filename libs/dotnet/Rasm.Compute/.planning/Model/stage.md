# [COMPUTE_STAGE]

`StageRun` executes the photo-to-PBR wire: a dependency-ordered request sequence folds into results, each request resolving every consumed plane against a producer already held or against its own blob key, stacking them along the channel axis into one bound tensor, leasing a session at a parity-graded acceleration decision, building one `Model/tiling#TILE_PLAN` grid, synthesizing the seeded latent the graph declares, running that grid once through `Model/tiling#TILE_FOLD`, and writing every produced plane back through the injected port while every grade crosses by value.

## [01]-[INDEX]

- [02]-[STAGE_WIRE]: the `StageWireMap` crossing against the corpus stage family, the lowered-primitive request and result records over a grant gate, the licence, residual, and latent mirrors, the accumulating decode admission, the executor-synthesized deterministic latent draw, the layout-memoized channel stack, and the port set the app root binds.
- [03]-[STAGE_FOLD]: dependency-ordered execution with a per-row producer-extent gate at resolution, a single-construction tile plan, lease-side artefact and latent gates, an accumulating attempt admission, and a one-demotion band gate inside the lease that reads the card's live envelope.
- [04]-[PARITY]: a horizon-bounded, capacity-capped, decision-keyed floor-provider residual memo over one lock-free keyed cell that survives a restart through its artifact port.

## [02]-[STAGE_WIRE]

- Cases: `LicenseClass` rows `permissive`, `copyleft`, `openRail`, `research`, `blocked`; `StageInput` an empty-stage source row or a named-producer chained row; the two `TileProduct` modalities the lease reports.
- Entry: `public static Fin<StageRequest> StageWireMap.Admit(ReadOnlySequence<byte> payload)` is the wire door — size gate, bounded parse, corpus rules, lowering, then the constraint fold — and `public static Fin<StageResultWire> StageWireMap.Result(StageResult result)` mints the answer; `public static Fin<StageRequest> StageRequest.Admit(StageRequest request)` is the accumulating constraint fold and `public Fin<TilePlan> Plan(int sourceChannels, Seq<TileProduct> products, TileAdmission admission, TileBlend blend, TileLayout layout)` the one plan construction.
- Law: this end binds LOWERED PRIMITIVES alone. The corpus family is the wire vocabulary and the specifying end authors the rich records; the strata forbid naming one of them here, so every column lands as the value THIS package can read — a closed roster as its camelCase key string resolved through the roster this package owns, a content key as its hex32 string, a correlation key as a string echoed verbatim, an extent as the `int` every tile derivation and span index downstream already runs in. Opaque-key erasure is the deliberate consumer shape: a resolution that fails REFUSES rather than degrading, so a licence spelling this roster cannot honour never runs under a typo, and re-minting a rich value from a key is the drift a second vocabulary opens.
- Law: enum columns lower through ONE fold. `Runtime/wire#PROTO_VOCABULARY` `WireKeys.Camel` is the single derivation from a generated enum member to this end's key string, so `Stage`, `License`, `Provider`, `Precision`, and `Pad` all read one rule and no `(enum)` table exists to drift. The outbound direction inverts by NAME under `IgnoreCase`, which is exact because every key this end publishes differs from its generated member only in casing.
- Law: `uint32` widens into `int` losslessly for every extent a grid can address, and the one value that cannot refuses. The checked fold overflows past `int.MaxValue` and `Admit` captures it onto the error channel, where an unchecked narrowing would hand every downstream span index a negative extent it reads as legal.
- Law: identity crosses as SIXTEEN BYTES and this end renders it once. `ContentHash.Wire`/`ContentHash.Admit` are the byte projection and its inverse, and `ContentHash.Hex` the lowercase spelling every `StagePorts` blob key carries — one alphabet in both directions, so a key read inbound and written back outbound is byte-identical and `ContentHash.Admit` refusing uppercase is the proof no second spelling entered through the port. The absent artefact is the EMPTY string: the `bytes.len = 16` field rule refuses a present-but-empty column, so emptiness is absence and never a fabricated digest.
- Law: Materials SPECIFIES and Compute EXECUTES. Stage, model-card, and role identities cross as OPAQUE KEYS and this end dispatches on none of them, so admitting a model, a stage, or an intermediate at the specifying end moves no surface here.
- Law: `Scale` DERIVES from the extents, never a column. Wire records thread both extents while a stage publishes `inputWidth × scale`, so a carried scale only ever contradicts them; `StageRequest.Scale` answers `None` for a fractional or anisotropic ratio and admission refuses there rather than at a bind reporting a shape mismatch it cannot explain. The corpus proves the same isotropy as a message CEL rule, and the domain check stays because `Scale` is DERIVED here and every later gate reads the derivation rather than the rule.
- Law: decode admission ACCUMULATES. Five independent columns — grant spelling, grant verdict, precision spelling, the frozen pad pin, and the bucket-versus-tile agreement — each carry their own constraint, so a request breaking three of them names three where an abort-first ternary ladder named the first and left the caller to re-submit twice.
- Law: the EXECUTOR synthesizes the latent; nothing upstream produces it. `StageSession.Latent` carries the card's own declaration — the graph's second input tensor, the channel depth of the draw, and the factor its extent divides the tile by — and this end mints the standard-normal tensor from the request's `Seed` at session bind, because a diffusion export cannot bake its latent into an initializer without freezing every pass to one draw.
- Law: the draw is the kernel's, and the collapse is a CORRECTNESS FIX rather than thrift. The mixer this page transcribed was a verbatim copy of `Rasm/Domain/identity#DETERMINISTIC_DERIVATION` `Deterministic`'s private splitmix64 finalizer differing in ONE place — the unit projection, which subtracted a reconstructed mantissa where the owner takes the top 53 bits and clamps to the open interval — so the two produced DIFFERENT doubles from the same state and a replay across the two spellings diverged. Composing the owner's lane-keyed draw makes `(seed, index, dimension)` the whole determinant, keeps Box-Muller here where no kernel member covers it, and puts the lane on a DECLARED ordinal so a row rename never silently re-keys a stored campaign.
- Law: SEED and LATENT are one joint discriminant, and both mismatches REFUSE. The specifying end zeroes the seed on a deterministic card, so a latent-declaring card arriving at the zero sentinel and a nonzero seed arriving at a card declaring no latent are the two halves of one contradiction — a request whose draw nothing synthesizes and a replay column the executor silently drops. One pattern over the pair refuses both; a graph that binds no latent runs at any seed it was never handed.
- Law: the LEASE binds tensor lanes to ROLE keys. Requests carry no output roster; the leased session reports one `TileProduct` row per card binding — the graph's own output tensor, the component lane inside it, and the opaque role key the product publishes under — so a PACKED export naming one tensor for several products lands each lane under its own role, and the executor never reads a role off a tensor's name.
- Law: a tiling plan has ONE construction. `StageRequest.Plan` folds the request's own extent, bucket, overlap, pad, and derived scale columns into `TilePlan`, whose gate roster alone spells the fixed-bucket law, and that same value then seats the bound flow and drives the fold.
- Law: a GRADE leaves as a value, never as a blob. A `Measure` product is rank-0 — no content address, no plane write, no mosaic arena — so it crosses on `StageResult.Scores` beside the plane outputs and never enters the produced-output map, because nothing downstream samples a grade and a stage binding one would be binding a number as a tensor. Writing four bytes through the plane port to hand back one float mints a blob the specifying end must fetch to read a value the result already holds.
- Auto: `Admit` proves everything provable WITHOUT a model — the descriptor rules first, then extent, bucket, and pad legality — so a malformed request never reaches a session lease, while the plan itself builds after the lease because only the model names its own products and its own layout. The stacked bound tensor memoizes per LAYOUT ROW on one lock-free keyed cell, because layout is the only property of a lease the placement reads and one stage holds up to three leases whose layouts may differ; a single-source stage fills the bound buffer directly and a multi-source stage hands each filler to the layout's own stack row, so a planar plane lands in its contiguous slice with no intermediate and a fill failure rides the error channel out with no torn entry seatable.
- Growth: a new stage COLUMN is one numbered field on `stage.proto` and one record column here, which the RMG completeness diagnostics force in the same change at both ends; a new grant posture is one `LicenseClass` row beside its corpus enum value; a new decode invariant is one `IConstraint<StageRequest>` conformance the accumulating fold already reads; a further reproducible draw is one `StageDraw` row carrying its own declared lane; a further execution backend is one `Model/providers#EP_AXIS` row declaring one `WireKey` beside one corpus enum value, never a translation table here and never a second stage owner.
- Boundary: `StagePorts` is the ONLY route to a plane and the only route to durable parity custody. Compute holds no blob store, no artifact index, no codec, and no channel vocabulary — it reads and writes float planes and parity verdicts through injected legs the app root binds against the Persistence object and artifact lanes, exactly as `Model/sessions#SESSION_CAPSULE` binds its warm-artifact leg; the read leg is the index-keyed span filler and the change is Compute-local by construction — the port is Compute-declared, the strata forbid a reference either way, and the filler is a delegate the ROOT binds (a blob copy, or a `Runtime/archive#HDF_ARCHIVE` hyperslab fill for an archive-resident plane), so an archive-resident chained input re-enters without rehydrating whole and no PureHDF member lands on a Compute signature; the parity legs carry no error channel outward because the root that owns the artifact write also owns the evidence cell its refusal parks on, and a read answering nothing degrades to the cold measurement the process memo already prices. Every blob key the port answers is the kernel `ContentHash.Hex` spelling, since the outbound half re-admits it through `ContentHash.Admit`. Provider and precision spellings resolve at `Model/providers#EP_AXIS`, whose rows carry their own wire keys, so this record holds no translation table and a roster landing there crosses without an edit here. `StageSession.Flow` takes the built plan and the synthesized draw, so the bound shapes, the bound draw, and the fold's shapes have one source and the root binds bytes rather than re-deriving a distribution. `GridProduct` is NOT a wire mirror: the specifying end's `StageProduct` is an emission `[Union]` naming the role type on its own output rows, and this carrier is an internal per-grid field-and-grade pair — genuinely distinct concepts, so this end keeps a distinct name rather than a same-named twin reaching one S4 consumer. The channel half of a role key stays an opaque string BOTH ways: the appearance channel roster is Materials-owned and open, so this end tags a key it cannot resolve as `channel` and the specifying end admits it through `TextureChannel.TryGet`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Collections;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using NodaTime.Serialization.Protobuf;
using Rasm.Compute.Runtime;
using Rasm.Domain;
using Riok.Mapperly.Abstractions;
using Thinktecture;
using static LanguageExt.Prelude;
// Contracts are retired from this logic.
using WireDuration = Google.Protobuf.WellKnownTypes.Duration;

namespace Rasm.Compute.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LicenseClass {
    public static readonly LicenseClass Permissive = new("permissive", grants: true);
    public static readonly LicenseClass Copyleft = new("copyleft", grants: true);
    public static readonly LicenseClass OpenRail = new("openRail", grants: true);
    public static readonly LicenseClass Research = new("research", grants: true);
    public static readonly LicenseClass Blocked = new("blocked", grants: false);

    private LicenseClass(string key, bool grants) : this(key) => Grants = grants;

    public bool Grants { get; }
}

[SmartEnum<long>(KeyMemberName = nameof(IDrawLane<StageDraw>.Lane))]
public sealed partial class StageDraw : IDrawLane<StageDraw> {
    public static readonly StageDraw Latent = new(0L);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct TileBucket(int Width, int Height);

public readonly record struct StageInput(string Stage, string Role, string Key);

public readonly record struct StageOutput(string Role, string BlobKey, int Width, int Height, string Transfer, string Format);

public readonly record struct StageScore(string Role, double Value);

public sealed record StageRequest(
    string Stage, string ModelCardId, string License, Seq<StageInput> Inputs,
    int InputWidth, int InputHeight, int OutputWidth, int OutputHeight,
    int TileWidth, int TileHeight, int Overlap, string Pad, TileBucket Bucket,
    string Provider, string Precision, ulong Seed, string Op, string Artefact) {

    public Option<ModelPrecision> SelectedPrecision => ModelPrecision.FromWire(Precision);

    public Option<LicenseClass> SelectedLicense =>
        LicenseClass.TryGet(License, out LicenseClass? row) ? Some(row!) : None;

    public Option<int> Scale =>
        InputWidth > 0 && InputHeight > 0
        && OutputWidth % InputWidth is 0 && OutputHeight % InputHeight is 0
        && OutputWidth / InputWidth == OutputHeight / InputHeight
            ? Some(OutputWidth / InputWidth)
            : None;

    public Fin<TilePlan> Plan(int sourceChannels, Seq<TileProduct> products, TileAdmission admission, TileBlend blend, TileLayout layout) =>
        from scale in Scale.ToFin(StageRefusal.Scale.Fault())
        from pad in (PadMode.TryGet(Pad, out PadMode? row) ? Some(row!) : None)
            .ToFin(StageRefusal.Pad.Fault())
        from built in TilePlan.Validate(
                sourceWidth: InputWidth, sourceHeight: InputHeight, channels: sourceChannels, products: products,
                tileWidth: TileWidth, tileHeight: TileHeight, overlap: Overlap, scale: scale,
                admission: admission, pad: pad, blend: blend, layout: layout, out TilePlan? plan) is { } fault
            ? Fin.Fail<TilePlan>(fault)
            : Fin.Succ(plan!)
        select built;

    static readonly Seq<IConstraint<StageRequest>> Decode = Seq<IConstraint<StageRequest>>(
        new LicenseRostered(), new LicenseGrants(), new PrecisionRostered(), new PadPinned(), new BucketAgrees());

    public static Fin<StageRequest> Admit(StageRequest request) =>
        Decode.Traverse(gate => gate.Check(request)).As().Map(_ => request).ToFin();

    sealed class LicenseRostered : IConstraint<StageRequest> {
        public Validation<Error, StageRequest> Check(StageRequest request) =>
            request.SelectedLicense.IsSome ? request : StageRefusal.License.Fault();
    }

    sealed class LicenseGrants : IConstraint<StageRequest> {
        public Validation<Error, StageRequest> Check(StageRequest request) =>
            request.SelectedLicense.Map(static row => row.Grants).IfNone(true)
                ? request
                : StageRefusal.Blocked.Fault();
    }

    sealed class PrecisionRostered : IConstraint<StageRequest> {
        public Validation<Error, StageRequest> Check(StageRequest request) =>
            request.SelectedPrecision.IsSome ? request : StageRefusal.Precision.Fault();
    }

    sealed class PadPinned : IConstraint<StageRequest> {
        public Validation<Error, StageRequest> Check(StageRequest request) =>
            StringComparer.Ordinal.Equals(request.Pad, PadMode.Reflect.Key)
                ? request
                : StageRefusal.PadPinned.Fault();
    }

    sealed class BucketAgrees : IConstraint<StageRequest> {
        public Validation<Error, StageRequest> Check(StageRequest request) =>
            request.Scale.IsSome && request.Bucket == new TileBucket(request.TileWidth, request.TileHeight)
                ? request
                : StageRefusal.Shape.Fault();
    }
}

public sealed record StageResult(
    string Stage, string ModelCardId, string Artefact, Seq<StageOutput> Outputs, Seq<StageScore> Scores,
    string ProviderUsed, int PartitionCount, double ElapsedMs, double ResidualDelta, bool ParityFresh, float Coverage,
    int TilesEmitted, string Op);

public readonly record struct GridProduct(
    Seq<StageOutput> Products, Seq<StageScore> Scores, int Partitions, float Coverage, int Tiles, string Artefact);

public readonly record struct ResidualBand(Option<double> Lower, double Upper) {
    public static ResidualBand Point(double ceiling) => new(Some(ceiling), ceiling);

    public static ResidualBand Ceiling(double upper) => new(Option<double>.None, upper);

    public bool Admits(double delta) => double.IsFinite(delta) && delta <= Upper;
}

public readonly record struct LatentInput(string Tensor, int Channels, int Downscale) {
    public Fin<LatentDraw> Draw(TilePlan plan, ulong seed) =>
        Channels > 0 && Downscale > 0 && plan.TileWidth % Downscale is 0 && plan.TileHeight % Downscale is 0
            ? Fin.Succ(new LatentDraw(
                Tensor,
                plan.Layout.Shape(Channels, plan.TileHeight / Downscale, plan.TileWidth / Downscale),
                Normal(seed, Channels * (plan.TileHeight / Downscale) * (plan.TileWidth / Downscale))))
            : StageRefusal.LatentGrid.Fault<LatentDraw>();

    static ReadOnlyMemory<float> Normal(ulong seed, int count) {
        float[] draw = new float[count];
        long lane = StageDraw.Latent.Lane;
        long keyed = unchecked((long)seed);
        for (int index = 0; index < count; index += 2) {
            double radius = Math.Sqrt(-2d * Math.Log(Deterministic.Unit(lanes: [lane, index, 0L], seed: keyed)));
            double angle = 2d * Math.PI * Deterministic.Unit(lanes: [lane, index, 1L], seed: keyed);
            draw[index] = (float)(radius * Math.Cos(angle));
            if (index + 1 < count) { draw[index + 1] = (float)(radius * Math.Sin(angle)); }
        }
        return draw;
    }
}

public sealed record LatentDraw(string Tensor, long[] Shape, ReadOnlyMemory<float> Values);

public sealed record PlaneSource(int Width, int Height, int Channels, PlaneFill Fill);

public sealed class PlaneStack {
    private readonly Seq<PlaneSource> sources;
    private readonly AtomHashMap<TileLayout, ReadOnlyMemory<float>> stacked = AtomHashMap<TileLayout, ReadOnlyMemory<float>>();
    private readonly int texels;

    private PlaneStack(Seq<PlaneSource> sources, int channels, int texels) =>
        (this.sources, Channels, this.texels) = (sources, channels, texels);

    public int Channels { get; }

    public static PlaneStack Of(Seq<PlaneSource> sources, StageRequest request) =>
        new(sources, sources.Sum(static source => source.Channels), request.InputWidth * request.InputHeight);

    public Fin<ReadOnlyMemory<float>> For(TileLayout layout) =>
        stacked.Find(layout).Match(
            Some: Fin.Succ,
            None: () => Build(layout).Map(built => {
                stacked.SwapKey(layout, held => held.IfNone(built));
                return stacked.Find(layout).IfNone(built);
            }));

    Fin<ReadOnlyMemory<float>> Build(TileLayout layout) {
        float[] buffer = new float[(long)Channels * texels];
        if (sources.Count is 1) {
            return sources[0].Fill(buffer).Map(_ => (ReadOnlyMemory<float>)buffer);
        }
        return sources
            .Fold(
                Fin.Succ(0),
                (offset, source) => offset.Bind(at =>
                    layout.Stack(source.Fill, buffer, source.Channels, at, Channels, texels)
                        .Map(_ => at + source.Channels)))
            .Map(_ => (ReadOnlyMemory<float>)buffer);
    }
}

// --- [ERRORS] --------------------------------------------------------------------------
public static class StageRefusal {
    public static readonly ContractRefusal License = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Blocked = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Precision = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Pad = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal PadPinned = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Scale = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Shape = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal Extent = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal Vocabulary = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal NoInput = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Unresolved = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal ExtentChain = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal ExtentMismatch = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal InputChannels = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal PartitionsUnmeasured = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal Partitions = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal LatentGrid = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal LatentUnseeded = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal SeedUnbindable = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Artefact = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Band = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal ResidualShape = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal ResidualNonFinite = new(ComputeArea.Model, ComputeContract.Valid);

}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class StageWireMap {
    // --- [REQUEST_INGRESS]
    public static Fin<StageRequest> Admit(ReadOnlySequence<byte> payload) =>
        ParseGuard.Read(StageRequestWire.Parser, payload, WireLimits.Inbound).Bind(Lowered).Bind(StageRequest.Admit);

    static Fin<StageRequest> Lowered(StageRequestWire wire) =>
        Try.lift(() => Fin.Succ(ToDomain(wire))).Run().Bind(static inner => inner)
            .MapFail(static _ => (Error)StageRefusal.Extent.Fault());

    [MapProperty(nameof(StageRequestWire.Stage), nameof(StageRequest.Stage), Use = nameof(StageKey))]
    [MapProperty(nameof(StageRequestWire.License), nameof(StageRequest.License), Use = nameof(LicenceKey))]
    [MapProperty(nameof(StageRequestWire.Provider), nameof(StageRequest.Provider), Use = nameof(ProviderKey))]
    [MapProperty(nameof(StageRequestWire.Precision), nameof(StageRequest.Precision), Use = nameof(PrecisionKey))]
    [MapProperty(nameof(StageRequestWire.Pad), nameof(StageRequest.Pad), Use = nameof(PadKey))]
    [MapProperty(nameof(StageRequestWire.Inputs), nameof(StageRequest.Inputs), Use = nameof(Rows))]
    [MapProperty(nameof(StageRequestWire.InputWidth), nameof(StageRequest.InputWidth), Use = nameof(Whole))]
    [MapProperty(nameof(StageRequestWire.InputHeight), nameof(StageRequest.InputHeight), Use = nameof(Whole))]
    [MapProperty(nameof(StageRequestWire.OutputWidth), nameof(StageRequest.OutputWidth), Use = nameof(Whole))]
    [MapProperty(nameof(StageRequestWire.OutputHeight), nameof(StageRequest.OutputHeight), Use = nameof(Whole))]
    [MapProperty(nameof(StageRequestWire.TileWidth), nameof(StageRequest.TileWidth), Use = nameof(Whole))]
    [MapProperty(nameof(StageRequestWire.TileHeight), nameof(StageRequest.TileHeight), Use = nameof(Whole))]
    [MapProperty(nameof(StageRequestWire.Overlap), nameof(StageRequest.Overlap), Use = nameof(Whole))]
    [MapProperty(nameof(StageRequestWire.Bucket), nameof(StageRequest.Bucket), Use = nameof(Bucket))]
    [MapProperty(nameof(StageRequestWire.Artefact), nameof(StageRequest.Artefact), Use = nameof(Hex))]
    public static partial StageRequest ToDomain(StageRequestWire wire);

    // --- [RESULT_EGRESS]
    public static Fin<StageResultWire> Result(StageResult result) =>
        Try.lift(() => Fin.Succ(ToWire(result))).Run().Bind(static inner => inner)
            .MapFail(static _ => (Error)StageRefusal.Vocabulary.Fault());

    [MapProperty(nameof(StageResult.Stage), nameof(StageResultWire.Stage), Use = nameof(StageRow))]
    [MapProperty(nameof(StageResult.ProviderUsed), nameof(StageResultWire.ProviderUsed), Use = nameof(ProviderRow))]
    [MapProperty(nameof(StageResult.Artefact), nameof(StageResultWire.Artefact), Use = nameof(Bytes))]
    [MapProperty(nameof(StageResult.ElapsedMs), nameof(StageResultWire.Elapsed), Use = nameof(Elapsed))]
    [MapProperty(nameof(StageResult.PartitionCount), nameof(StageResultWire.PartitionCount), Use = nameof(Unsigned))]
    [MapProperty(nameof(StageResult.TilesEmitted), nameof(StageResultWire.TilesEmitted), Use = nameof(Unsigned))]
    public static partial StageResultWire ToWire(StageResult result);

    // --- [VOCABULARY]
    [UserMapping] static string StageKey(PbrStageWire wire) => WireKeys.Camel(wire);
    [UserMapping] static string LicenceKey(LicenseClassWire wire) => WireKeys.Camel(wire);
    [UserMapping] static string ProviderKey(InferenceProviderWire wire) => WireKeys.Camel(wire);
    [UserMapping] static string PrecisionKey(TensorPrecisionWire wire) => WireKeys.Camel(wire);
    [UserMapping] static string PadKey(PadModeWire wire) => WireKeys.Camel(wire);

    [MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true)]
    private static partial PbrStageWire StageRow(string key);

    [MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true)]
    private static partial InferenceProviderWire ProviderRow(string key);

    [MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true)]
    private static partial PlaneTransferWire TransferRow(string key);

    [MapEnum(EnumMappingStrategy.ByName, IgnoreCase = true)]
    private static partial PlaneFormatWire FormatRow(string key);

    // --- [SCALARS]
    [UserMapping] static int Whole(uint value) => checked((int)value);
    [UserMapping] static uint Unsigned(int value) => checked((uint)value);

    [UserMapping] static string Hex(ByteString bytes) => bytes.IsEmpty ? string.Empty : ContentHash.Hex(Digest(bytes));
    [UserMapping] static ByteString Bytes(string hex) => ContentHash.Wire(ContentHash.Admit(hex).ThrowIfFail());
    static UInt128 Digest(ByteString bytes) => ContentHash.Admit(bytes.Span).ThrowIfFail();

    [UserMapping] static TileBucket Bucket(BucketWire wire) => new(Whole(wire.Width), Whole(wire.Height));

    [UserMapping] static WireDuration Elapsed(double milliseconds) => Duration.FromMilliseconds(milliseconds).ToProtobufDuration();

    // --- [PRODUCTS]
    [UserMapping] static Seq<StageInput> Rows(RepeatedField<StageInputWire> rows) => toSeq(rows).Map(Row).Strict();

    static StageInput Row(StageInputWire wire) =>
        wire.KindCase switch {
            StageInputWire.KindOneofCase.Source => new StageInput(string.Empty, string.Empty, ContentHash.Hex(Digest(wire.Source.Key))),
            StageInputWire.KindOneofCase.Produced => new StageInput(WireKeys.Camel(wire.Produced.Stage), Product(wire.Produced.Product), string.Empty),
            _ => new StageInput(string.Empty, string.Empty, string.Empty),
        };

    static string Product(StageProductWire role) =>
        role.RoleCase switch {
            StageProductWire.RoleOneofCase.Channel => role.Channel,
            StageProductWire.RoleOneofCase.Prior => WireKeys.Camel(role.Prior),
            StageProductWire.RoleOneofCase.Measure => WireKeys.Camel(role.Measure),
            _ => string.Empty,
        };

    [UserMapping] static StageProductWire Role(string key) =>
        Named(out PriorFieldWire prior)
            ? new StageProductWire { Prior = prior }
            : Named(out ScoreFieldWire measure)
                ? new StageProductWire { Measure = measure }
                : new StageProductWire { Channel = key };

    static bool Named<TEnum>(string key, out TEnum row) where TEnum : struct, Enum {
        row = default;
        return key.Length > 0 && char.IsAsciiLetter(key[0])
            && Enum.TryParse(ignoreCase: true, out row)
            && !EqualityComparer<TEnum>.Default.Equals(row, default);
    }

    [MapProperty(nameof(StageOutput.Role), nameof(StageOutputWire.Role), Use = nameof(Role))]
    [MapProperty(nameof(StageOutput.BlobKey), nameof(StageOutputWire.Blob), Use = nameof(Bytes))]
    [MapProperty(nameof(StageOutput.Width), nameof(StageOutputWire.Width), Use = nameof(Unsigned))]
    [MapProperty(nameof(StageOutput.Height), nameof(StageOutputWire.Height), Use = nameof(Unsigned))]
    [MapProperty(nameof(StageOutput.Transfer), nameof(StageOutputWire.Transfer), Use = nameof(TransferRow))]
    [MapProperty(nameof(StageOutput.Format), nameof(StageOutputWire.Format), Use = nameof(FormatRow))]
    private static partial StageOutputWire Plane(StageOutput output);

    [MapProperty(nameof(StageScore.Role), nameof(StageScoreWire.Role), Use = nameof(Role))]
    private static partial StageScoreWire Grade(StageScore score);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record StageSession(
    Seq<TileProduct> Products, TileLayout Layout, TileBlend Blend, TileAdmission Admission, Option<int> Partitions, int PartitionCap,
    int InputChannels, ResidualBand Residual, string Artefact,
    Option<LatentInput> Latent, Func<TilePlan, Option<LatentDraw>, Fin<BoundFlow>> Flow, IDisposable Hold);

public sealed record StagePorts(
    Func<string, Fin<PlaneSource>> Read,
    Func<ReadOnlyMemory<float>, int, int, int, Fin<string>> Write,
    Func<StageRequest, TileProduct, Fin<(string Transfer, string Format)>> Describe,
    Func<StageRequest, ExecutionProvider, ModelPrecision, Fin<StageSession>> Lease,
    ParityPort Parity);
```

## [03]-[STAGE_FOLD]

- Owner: `StageRun` folds a dependency-ordered request sequence into results; `StageAttempt` is the lifted candidate its accumulating admission reads.
- Entry: `public static Fin<Seq<StageResult>> Fold(Seq<StageRequest> plan, StagePorts ports, RunOptions options, CancelScope scope, IClock clock, MonotonicTimeline timeline)` — one entry for the whole plan, because per-request entry pushes producer-output resolution onto the caller and re-opens the chained-stage defect where every stage reads the source photograph.
- Law: ONE clock capability per question, and the ELAPSED question is the timeline's. Verdict age is a wall `Instant` off `IClock`; the run's own span is a monotone pair off the kernel `MonotonicTimeline`, whose stamps carry their own timeline identity — a raw `long` mark threaded across a whole query belongs to no timeline and cannot prove the pair it is subtracted against came from the same source.
- Law: EVERY input row binds or the request refuses. The admitted request carries one `StageInput` row per consumed product in the card's own binding order, and the executor resolves them ALL — a chained row against its producer's held output, an empty-stage row against its blob key — then STACKS the planes along the channel axis in that order into the one bound tensor, the session's own `InputMetadata` channel width proving the sum. Head-taking that silently drops `inputs[1..]` runs the card without the photograph its estimator consumes, and nothing fails.
- Law: a chained input RE-ENTERS through the port and never bypasses it. Producer planes leave through `StagePorts.Write` at the transfer and format `Describe` chose, and the host alone knows whether that crossing is lossless. Device-resident handoff is unreachable here for a second and independent reason: the mosaic overlap-adds every field into pooled HOST planes and `TileMosaic` owns those rentals, so no producer `OrtValue` survives a grid for a consumer to bind, and this fold reaches no `SessionPlacement` readback to compare residency with. Device-to-device copies belong where a bound output stays resident — one `Tensor/residency#ORT_BRIDGE` relay over a `BoundFlow` pair — never on this fold.
- Law: every input row of one stage shares ONE extent. The request declares `InputWidth`/`InputHeight` and every consumed plane matches it — a chained row proves against its producer's published extent at RESOLUTION, before the blob read and before the lease, and a source row proves against its own bytes at read-back — because the channel stack lays every plane into one bound tensor over one texel count, so a second extent has nowhere to go. Both refusals name both extents, where a bound session's shape fault three ports later names neither stage that disagreed.
- Law: the ARTEFACT pins at the lease, not at the far end. `StageRequest.Artefact` carries the weight digest the model card declared and `StageSession.Artefact` the digest the lease loaded, so the ONE boundary every lease crosses proves them equal before a grid runs — comparing only where the result lands pays a whole mosaic, and worse, grades a parity residual against weights nobody asked for and seats that verdict in the memo. `StageResult.Artefact` then reports the MEASURED value rather than echoing the request. The LAYOUT is the lease's alone: `StageSession.Layout` is the dimension order the leased model card declares, the plan and the channel stack read it off the session, and no request column restates it — a column the wire carried and the lease overrode was a claim rather than a contract.
- Law: the attempt's independent guards ACCUMULATE. The stacked channel sum, the measured partition cap, and the seed-and-latent pairing are three facts about one lease that do not depend on one another, so a lease breaking all three names all three; only the partition READ is sequenced before its cap, because a cap over an absent measurement has nothing to compare.
- Law: evidence publishes MEASURED or refuses. `PartitionCount` reads the per-bucket warm evidence the session capsule measured once, never a zero standing in for an unmeasured run; a request whose bucket carries no partition measurement refuses rather than minting a result whose evidence column reads as observed. Registration seats that bucket under its `WarmKey` with an ABSENT count and only the trace-reading `Model/run#RUN_MODES` `WarmPulse` fills it, so the two surfaces divide cleanly: the composition registers the shapes it will run, and the pulse measures how the graph partitioned for each.
- Law: the residual GATES, never merely reports, and the BAND rides the lease rather than the memo. Every lease carries the card's `ResidualBand` into `StageSession`, the run's own lease grades the measured delta against the band's `Upper`, and a breach DEMOTES to the floor at full precision — one demotion, `ProviderUsed` reporting the substitution, `ResidualDelta` keeping the measured breach — so an accelerated run outside its card's band never publishes as if it were inside. Freezing the band into the verdict at measurement time is the rejected form: a card widening its band keeps demoting against the frozen one until a re-measurement of an unchanged residual clears it.
- Auto: `Fold` threads a produced-OUTPUT map so a binding naming a producer resolves against results already held. `Execute` resolves the provider once against the frozen census, resolves and reads EVERY input row, runs the horizon-gated memoized parity measurement, then leases at that decision, proves the artefact digest, accumulates the attempt guards, builds the plan against the session's binding roster, synthesizes the draw from the request's seed, opens ONE bound flow at that plan and that draw, runs the grid once, writes every produced plane through the port, carries every grade out by value, and folds the elapsed span off the timeline. A breach answers `None` inside that lease and the run re-leases once at the floor, so the demotion costs exactly one extra lease and only on the runs that earned it.
- Result: each executed stage returns one `StageResult` carrying its outputs, scores, provider, partition count, artifact digest, residual delta, tile count, and elapsed time.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Parametric.MonotonicTimeline`)
- Growth: a further attempt invariant is one `IConstraint<StageAttempt>` conformance; a stage emitting more products is more `TileProduct` rows the lease reports, a stage PACKING two products into one tensor is one more row at that tensor's next lane, a stage GRADING its input is one `TileProduct.Measure` row landing on `StageResult.Scores`, and a stage CONSUMING more products is one more wire input row widening the channel stack — no surface move on any of them.
- Boundary: the `GridProduct`→`StageResult` projection is NOT a Mapperly correspondence and no mapping method is owed for it: both shapes are this package's own, the crossing folds three independently measured columns (the timeline span, the graded delta, the freshness discriminant) that no generated transcription can produce, and the pure columns it does carry are one owner's carrier feeding its own result rather than an owner↔DTO rename. The `[Mapper]`-earning correspondence is `[02]-[STAGE_WIRE]` `StageWireMap`, which crosses these records against the corpus family and nothing else.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct StageAttempt(StageRequest Request, StageSession Session, TilePlan Plan, int Partitions);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class StageRun {
    public static Fin<Seq<StageResult>> Fold(
        Seq<StageRequest> plan, StagePorts ports, RunOptions options, CancelScope scope, IClock clock, MonotonicTimeline timeline) =>
        plan.Fold(
            Fin.Succ((Results: Seq<StageResult>(), Produced: HashMap<(string Stage, string Role), StageOutput>())),
            (state, request) => state.Bind(carried =>
                from admitted in StageRequest.Admit(request)
                from result in Execute(admitted, carried.Produced, ports, options, scope, clock, timeline)
                select (
                    Results: carried.Results.Add(result),
                    Produced: result.Outputs.Fold(
                        carried.Produced,
                        (map, output) => map.AddOrUpdate((request.Stage, output.Role), output)))))
            .Map(static carried => carried.Results);

    static Fin<StageResult> Execute(
        StageRequest request, HashMap<(string Stage, string Role), StageOutput> produced, StagePorts ports,
        RunOptions options, CancelScope scope, IClock clock, MonotonicTimeline timeline) =>
        from opened in timeline.Capture()
        from licensed in request.SelectedLicense.ToFin(StageRefusal.License.Fault())
        from _ in guard(licensed.Grants, (Error)StageRefusal.Blocked.Fault())
        from precision in request.SelectedPrecision.ToFin(StageRefusal.Precision.Fault())
        from keys in Sources(request, produced)
        from planes in keys.Traverse(key => ports.Read().ToValidation()).As().ToFin()
        from __ in planes
            .Traverse(plane => guard(
                    plane.Width == request.InputWidth && plane.Height == request.InputHeight,
                    (Error)StageRefusal.ExtentMismatch.Fault())
                .ToFin().ToValidation())
            .As().ToFin()
        let stack = PlaneStack.Of(planes, request)
        let requested = ExecutionProvider.FromWire(request.Provider)
        from verdict in Assured(request, ports, stack, requested, precision, options, scope, clock)
        from outputs in Run(request, ports, stack, verdict.Verdict, requested, precision, options, scope)
        from closed in timeline.Capture()
        from elapsed in timeline.Elapsed(opened, closed)
        select new StageResult(
            request.Stage, request.ModelCardId, outputs.Product.Artefact, outputs.Product.Products,
            outputs.Product.Scores, outputs.Provider.ReportKey, outputs.Product.Partitions,
            elapsed.TotalMilliseconds, verdict.Verdict.Delta, verdict.Fresh,
            outputs.Product.Coverage, outputs.Product.Tiles);

    static Fin<Seq<string>> Sources(StageRequest request, HashMap<(string Stage, string Role), StageOutput> produced) =>
        request.Inputs.IsEmpty
            ? StageRefusal.NoInput.Fault<Seq<string>>()
            : request.Inputs
                .Traverse(binding => (binding.Stage.Length is 0
                        ? Fin.Succ(binding.Key)
                        : produced.Find((binding.Stage, binding.Role))
                            .ToFin(StageRefusal.Unresolved.Fault())
                            .Bind(upstream =>
                                upstream.Width == request.InputWidth && upstream.Height == request.InputHeight
                                    ? Fin.Succ(upstream.BlobKey)
                                    : StageRefusal.ExtentChain.Fault<string>()))
                    .ToValidation())
                .As()
                .ToFin();

    static Fin<(GridProduct Product, ExecutionProvider Provider)> Run(
        StageRequest request, StagePorts ports, PlaneStack stack, ParityVerdict verdict,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope) =>
        Attempt(request, ports, stack, verdict, selected, precision, options, scope)
            .Bind(attempted => attempted.Case is GridProduct produced
                ? Fin.Succ((produced, selected))
                : Attempt(request, ports, stack, ParityVerdict.Identity, ExecutionProvider.Floor, ModelPrecision.Full, options, scope)
                    .Bind(floored => floored.Case is GridProduct onFloor
                        ? Fin.Succ((onFloor, ExecutionProvider.Floor))
                        : StageRefusal.Band.Fault<(GridProduct, ExecutionProvider)>()));

    static readonly Seq<IConstraint<StageAttempt>> Gates = Seq<IConstraint<StageAttempt>>(
        new ChannelSum(), new PartitionCap(), new SeedLatent());

    static Fin<Option<GridProduct>> Attempt(
        StageRequest request, StagePorts ports, PlaneStack stack, ParityVerdict verdict,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope) =>
        Leased(request, ports, selected, precision, stack.Channels, (session, plan) =>
            !session.Residual.Admits(verdict.Delta)
                ? Fin.Succ(Option<GridProduct>.None)
                : from partitions in session.Partitions
                      .ToFin(StageRefusal.PartitionsUnmeasured.Fault())
                  let candidate = new StageAttempt(request, session, plan, partitions)
                  from _ in Gates.Traverse(gate => gate.Check(candidate)).As().Map(static _ => unit).ToFin()
                  from source in stack.For(session.Layout)
                  from emitted in Emit(request, ports, session, plan, source, options, scope)
                  select Some(new GridProduct(
                      emitted.Products, emitted.Scores, partitions, emitted.Coverage, emitted.Tiles, session.Artefact)));

    sealed class ChannelSum : IConstraint<StageAttempt> {
        public Validation<Error, StageAttempt> Check(StageAttempt candidate) =>
            candidate.Plan.Channels == candidate.Session.InputChannels
                ? candidate
                : StageRefusal.InputChannels.Fault();
    }

    sealed class PartitionCap : IConstraint<StageAttempt> {
        public Validation<Error, StageAttempt> Check(StageAttempt candidate) =>
            candidate.Partitions <= candidate.Session.PartitionCap
                ? candidate
                : StageRefusal.Partitions.Fault();
    }

    sealed class SeedLatent : IConstraint<StageAttempt> {
        public Validation<Error, StageAttempt> Check(StageAttempt candidate) =>
            (candidate.Request.Seed, candidate.Session.Latent.Case) switch {
                (0UL, LatentInput declared) =>
                    StageRefusal.LatentUnseeded.Fault(),
                (not 0UL, not LatentInput) =>
                    StageRefusal.SeedUnbindable.Fault(),
                _ => candidate,
            };
    }

    static Fin<T> Leased<T>(
        StageRequest request, StagePorts ports, ExecutionProvider provider, ModelPrecision precision,
        int sourceChannels, Func<StageSession, TilePlan, Fin<T>> use) =>
        ports.Lease(request, provider, precision).Bind(session => {
            using (session.Hold) {
                return guard(
                        StringComparer.Ordinal.Equals(session.Artefact, request.Artefact),
                        (Error)StageRefusal.Artefact.Fault()).ToFin()
                    .Bind(_ => request.Plan(sourceChannels, session.Products, session.Admission, session.Blend, session.Layout))
                    .Bind(plan => use(session, plan));
            }
        });

    static Fin<(Seq<StageOutput> Products, Seq<StageScore> Scores, float Coverage, int Tiles)> Emit(
        StageRequest request, StagePorts ports, StageSession session, TilePlan plan, ReadOnlyMemory<float> source,
        RunOptions options, CancelScope scope) =>
        Drawn(session, plan, request.Seed).Bind(latent => session.Flow(plan, latent)).Bind(flow => {
            using (flow) {
                return flow.InferTiled(options, scope, plan, source).Bind(assembled => {
                    using (assembled) {
                        return assembled.Planes
                            .Traverse(produced =>
                                (from shape in ports.Describe(request, produced.Product)
                                 from key in ports.Write(
                                     produced.Plane.Memory, plan.OutputWidth, plan.OutputHeight, produced.Product.Channels)
                                 select new StageOutput(
                                     produced.Product.Role, plan.OutputWidth, plan.OutputHeight,
                                     shape.Transfer, shape.Format)).ToValidation())
                            .As()
                            .ToFin()
                            .Map(products => (
                                Products: products,
                                Scores: assembled.Grades.Map(static grade => new StageScore(grade.Role, grade.Value)),
                                assembled.Coverage, assembled.Tiles));
                    }
                });
            }
        });

    static Fin<Option<LatentDraw>> Drawn(StageSession session, TilePlan plan, ulong seed) =>
        session.Latent.Match(
            Some: declared => declared.Draw(plan, seed).Map(Some),
            None: static () => Fin.Succ(Option<LatentDraw>.None));
}
```

## [04]-[PARITY]

- Owner: `ParityVerdict` carries one measured residual beside the instant it was measured at; `ParityPort` carries the durable custody legs and the retirement horizon the composing root sets; `ParityMemo` is the process tier's row.
- Entry: the memoized measurement is interior to `StageRun`; the composing root binds `ParityPort` alone.
- Law: parity measures the CANDIDATE against a FULL-precision floor, ONCE per acceleration decision, BEFORE the grid runs. The canary tile runs on the requested provider and on `ExecutionProvider.Floor` at `ModelPrecision.Full` — `cpu` is the floor row's REPORT key, the spelling a reader discriminates on, never the lease selector — so the residual grades the whole decision, provider and precision together, rather than comparing two runs that already agreed to lower precision. Both probes run at the REQUEST'S seed: comparing two stochastic draws grades noise, not the provider.
- Law: residuals are a property of `(card, provider, precision, runtime, DEVICE, host)`. The canary grades the graph an EP compiled at a precision on ONE adapter, so the key folds the provider's own `ResultKey` — which now carries the ranked device's fingerprint — and a dual-GPU host cannot publish one verdict for two adapters. Measuring per request prices two extra leases, two extra flows, and two extra runs on every stage of every plan for a verdict that cannot move between them; a verdict living only in process memory re-prices exactly that on every cold app root, so the memo's durable half rides `ParityPort` under the same key and a restart READS what the last process measured.
- Law: on the floor at full precision the delta is 0 by IDENTITY, not by measurement. `providerUsed == cpu` at `fp32` names the discrimination — every accelerated run carries a memoized measurement, the floor carries the identity, and no zero reads as an unmeasured observation.
- Law: a parity verdict EXPIRES. `ParityPort.Horizon` bounds every hit at both tiers: the key names the card, the provider result key, and the host fingerprint, but not the driver or firmware stack the silicon runs under — and a driver revision moves a residual without moving one term of that key. Age is therefore the only honest retirement, a verdict past the horizon reads as ABSENT and re-measures, and the canonical thirty days is the composing root's value rather than a constant this fold reads.
- Law: the process tier is a KEY-GRAINED lock-free cell. Seating a verdict, touching a read ordinal, and trimming past the cap are all key-local transitions, so a `Lock` around a whole immutable map serialized every parity read on this process against every other; the guarded form was defensible only where the transition wrapped a native effect, and neither seating nor trimming runs one. Eviction is least-recently-read and costs ONE pass over the rows rather than a full sort per insert past the cap, and it costs one durable read on the next request rather than a re-measurement, because the port holds the same verdict.
- Auto: the memo answers first and the durable row second, and a durable hit SEATS the memo so a cold start pays one artifact read per decision rather than one per request. Both tiers answer through ONE staleness gate.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, LanguageExt.Core, NodaTime
- Growth: a further parity axis is one term folded into the key, which re-keys both the process memo and the durable row in one edit because they read one derivation; a further capacity or retirement posture is one `ParityPort` column.
- Boundary: `ParityPort` carries no error channel outward: the root that owns the artifact write also owns the evidence cell its refusal parks on, so a failed artifact write never fails an inference whose measurement succeeded, and a read answering nothing degrades to the cold measurement the memo already prices. Memo capacity is a POLICY VALUE on the port beside `Horizon` rather than a constant this fold reads — "bounded by the key product" only bounds anything if the product does, and a long-lived root serving a growing model registry grows it without ceiling.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ParityVerdict(double Delta, Instant MeasuredAt) {
    public static readonly ParityVerdict Identity = new(0d, Instant.MinValue);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record ParityPort(
    HostFingerprint Host,
    Duration Horizon,
    int Capacity,
    Func<string, Option<ParityVerdict>> Read,
    Func<string, ParityVerdict, Unit> Write) {
    public static readonly Duration CanonicalHorizon = Duration.FromDays(30);

    public const int CanonicalCapacity = 512;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class StageRun {
    readonly record struct ParityMemo(ParityVerdict Verdict, long Read);

    static readonly AtomHashMap<string, ParityMemo> Parity = AtomHashMap<string, ParityMemo>();
    static readonly Atom<long> ParityTick = Atom(0L);

    static string ParityKey(StageRequest request, ExecutionProvider provider, ModelPrecision precision, ParityPort parity) =>
        $"{request.ModelCardId}:{provider.ResultKey(OrtEnv.Instance().GetVersionString(), precision, provider.AutoSelect.Head)}:{parity.Host}";

    static Fin<(ParityVerdict Verdict, bool Fresh)> Assured(
        StageRequest request, StagePorts ports, PlaneStack stack,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope, IClock clock) =>
        selected.IsFloor && ReferenceEquals(precision, ModelPrecision.Full)
            ? Fin.Succ((ParityVerdict.Identity, false))
            : Measured(request, ports, stack, selected, precision, options, scope, clock);

    static Fin<(ParityVerdict Verdict, bool Fresh)> Measured(
        StageRequest request, StagePorts ports, PlaneStack stack,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope, IClock clock) =>
        (Key: ParityKey(request, selected, precision, ports.Parity), Now: clock.GetCurrentInstant()) switch {
            var at => Remembered(at.Key, ports.Parity, at.Now).Case is ParityVerdict held
                ? Fin.Succ((held, false))
                : Leased(request, ports, selected, precision, stack.Channels, (candidate, plan) =>
                    Leased(request, ports, ExecutionProvider.Floor, ModelPrecision.Full, stack.Channels, (reference, truthPlan) =>
                        from fastSource in stack.For(candidate.Layout)
                        from fast in Probe(candidate, plan, request.Seed, fastSource, options, scope)
                        from truthSource in stack.For(reference.Layout)
                        from truth in Probe(reference, truthPlan, request.Seed, truthSource, options, scope)
                        from delta in Residual(fast, truth)
                        select (Remember(at.Key, new ParityVerdict(delta, at.Now), ports.Parity), true))),
        };

    static Option<ParityVerdict> Remembered(string at, ParityPort parity, Instant now) =>
        Parity.Find(at).Filter(seated => Fresh(seated.Verdict, parity, now)) is { IsSome: true } memo
            ? memo.Map(seated => { Parity.SwapKey(at, held => held.Map(row => row with { Read = ParityTick.Swap(static tick => tick + 1L) })); return seated.Verdict; })
            : parity.Read(at).Filter(durable => Fresh(durable, parity, now)).Map(durable => Seat(at, durable, parity));

    static bool Fresh(ParityVerdict verdict, ParityPort parity, Instant now) =>
        now - verdict.MeasuredAt <= parity.Horizon;

    static ParityVerdict Remember(string at, ParityVerdict verdict, ParityPort parity) {
        parity.Write(at, verdict);
        return Seat(at, verdict, parity);
    }

    static ParityVerdict Seat(string at, ParityVerdict verdict, ParityPort parity) {
        Parity.SwapKey(at, _ => Some(new ParityMemo(verdict, ParityTick.Swap(static tick => tick + 1L))));
        if (Parity.Count > parity.Capacity) {
            Parity.AsIterable()
                .Fold(Option<(string Key, long Read)>.None, static (coldest, row) =>
                    coldest is { IsSome: true, Case: (string _, long read) } && read <= row.Value.Read ? coldest : Some((row.Key, row.Value.Read)))
                .Iter(oldest => Parity.Remove(oldest.Key));
        }
        return verdict;
    }

    static Fin<float[]> Probe(
        StageSession session, TilePlan plan, ulong seed, ReadOnlyMemory<float> source, RunOptions options, CancelScope scope) =>
        Drawn(session, plan, seed).Bind(latent => session.Flow(plan, latent)).Bind(flow => {
            using (flow) { return flow.Canary(options, scope, plan, source); }
        });

    static Fin<double> Residual(float[] candidate, float[] reference) {
        if (candidate.Length != reference.Length) {
            return StageRefusal.ResidualShape.Fault<double>();
        }
        using SpanOwner<float> difference = SpanOwner<float>.Allocate(candidate.Length);
        TensorPrimitives.Subtract(candidate, reference, difference.Span);
        float residual = TensorPrimitives.MaxMagnitude<float>(difference.Span);
        return float.IsFinite(residual)
            ? Fin.Succ((double)Math.Abs(residual))
            : StageRefusal.ResidualNonFinite.Fault<double>();
    }
}
```
