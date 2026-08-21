# [COMPUTE_STAGE]

`StageRun` executes the photo-to-PBR wire: a dependency-ordered request sequence folds into results, each request resolving every consumed plane against a producer already held or against its own blob key, stacking them along the channel axis into one bound tensor, leasing a session at a parity-graded acceleration decision, building one `Model/tiling#TILE_PLAN` grid, synthesizing the seeded latent the graph declares, running that grid once through `Model/tiling#TILE_FOLD`, and writing every produced plane back through the injected port while every grade crosses by value.

`StageRequest` and `StageResult` transcribe the frozen `Rasm.Materials` wire as LOWERED PRIMITIVES — opaque keys and content addresses cross injected ports with no strata reference in either direction. Parity measures the candidate against a full-precision floor ONCE per acceleration decision, memoizes the verdict across the process and across restarts under one key, and DEMOTES a run whose measured residual leaves its card's live band.

## [01]-[INDEX]

- [02]-[STAGE_WIRE]: the lowered-primitive request and result records over a grant gate, the licence, residual, and latent mirrors, the accumulating decode admission, the executor-synthesized deterministic latent draw, the layout-memoized channel stack, and the port set the app root binds.
- [03]-[STAGE_FOLD]: dependency-ordered execution with a per-row producer-extent gate at resolution, a single-construction tile plan, lease-side artefact, layout, and latent gates, an accumulating attempt admission, and a one-demotion band gate inside the lease that reads the card's live envelope.
- [04]-[PARITY]: a horizon-bounded, capacity-capped, decision-keyed floor-provider residual memo over one lock-free keyed cell that survives a restart through its artifact port.
- [05]-[RESEARCH]: open questions.

## [02]-[STAGE_WIRE]

- Owner: `StageRequest`/`StageInput`/`StageOutput`/`StageScore`/`StageResult` transcribe the frozen wire records; `LicenseClass` enforces the grant vocabulary; `GridProduct` carries one executed grid's own field set beside its grade set; `ResidualBand` carries the card's parity band and `LatentInput`/`LatentDraw` its declared and synthesized seed tensor; `StageDraw` declares the reproducible draw lane the kernel `Deterministic` owner keys on; `PlaneStack` folds the channel sum once and memoizes the stacked bound tensor per layout row across every lease one stage takes; `StageSession` carries the model-derived facts a request cannot know; `StagePorts` carries the plane read, plane write, output-description, session-open, and parity-custody legs the app root binds; `StageRefusal` names this owner's shared contract refusals without a string-key roster.
- Cases: `LicenseClass` rows `permissive`, `copyleft`, `openRail`, `research`, `blocked`; `StageInput` an empty-stage source row or a named-producer chained row; the two `TileProduct` modalities the lease reports.
- Entry: `public static Fin<StageRequest> Admit(StageRequest request)` is the decode gate and `public Fin<TilePlan> Plan(int sourceChannels, Seq<TileProduct> products, TileAdmission admission, TileBlend blend, TileLayout layout)` the one plan construction.
- Law: this end binds LOWERED PRIMITIVES alone. Specifying ends author the wire in their own types; the strata forbid naming one of them here, so every column lands as the value the codec wrote — an enum as its roster string resolved through the roster THIS package owns, a content address as its hex32 string, a correlation key as a string echoed verbatim, an extent as the `int` every tile derivation and span index downstream already runs in (the wire's `uint32` widens losslessly and a negative never crosses). Opaque-key erasure is the deliberate consumer shape: a resolution that fails REFUSES rather than degrading, so a licence spelling this roster cannot honour never runs under a typo, and re-minting a rich value from a key is the drift a second vocabulary opens.
- Law: the DECODE-ONLY MIRROR is complete at 19 request columns and 13 result columns, and the producing roster is authoritative. Each record mirrors its `Rasm.Materials` `Appearance/neural#…` counterpart column for column — declaration order diverges only on `Artefact`, which the crossing's own slot-ordinal digest makes lawful — and a column the producer carries lands here in the SAME change as its `Runtime/wire#CONTRACT_EVOLUTION` slot row, whose arity probe then forces the pair. The `Layout` request column and the `Scores` result column are carried at both ends and owe their slot rows at that crossing; until both land the boot probe reads 19-against-18 and 13-against-12 and refuses, which is the probe doing its job rather than a defect at either record.
- Law: Materials SPECIFIES and Compute EXECUTES. Stage, model-card, and role identities cross as OPAQUE KEYS and this end dispatches on none of them, so admitting a model, a stage, or an intermediate at the specifying end moves no surface here.
- Law: `Scale` DERIVES from the extents, never a column. Wire records thread both extents while a stage publishes `inputWidth × scale`, so a carried scale only ever contradicts them; `StageRequest.Scale` answers `None` for a fractional or anisotropic ratio and admission refuses there rather than at a bind reporting a shape mismatch it cannot explain.
- Law: decode admission ACCUMULATES. Five independent columns — grant spelling, grant verdict, precision spelling, the frozen pad pin, and the bucket-versus-tile agreement — each carry their own constraint, so a request breaking three of them names three where an abort-first ternary ladder named the first and left the caller to re-submit twice.
- Law: the EXECUTOR synthesizes the latent; nothing upstream produces it. `StageSession.Latent` carries the card's own declaration — the graph's second input tensor, the channel depth of the draw, and the factor its extent divides the tile by — and this end mints the standard-normal tensor from the request's `Seed` at session bind, because a diffusion export cannot bake its latent into an initializer without freezing every pass to one draw.
- Law: the draw is the kernel's, and the collapse is a CORRECTNESS FIX rather than thrift. The mixer this page transcribed was a verbatim copy of `Rasm/Domain/identity#DETERMINISTIC_DERIVATION` `Deterministic`'s private splitmix64 finalizer differing in ONE place — the unit projection, which subtracted a reconstructed mantissa where the owner takes the top 53 bits and clamps to the open interval — so the two produced DIFFERENT doubles from the same state and a replay across the two spellings diverged. Composing the owner's lane-keyed draw makes `(seed, index, dimension)` the whole determinant, keeps Box-Muller here where no kernel member covers it, and puts the lane on a DECLARED ordinal so a row rename never silently re-keys a stored campaign.
- Law: SEED and LATENT are one joint discriminant, and both mismatches REFUSE. The specifying end zeroes the seed on a deterministic card, so a latent-declaring card arriving at the zero sentinel and a nonzero seed arriving at a card declaring no latent are the two halves of one contradiction — a request whose draw nothing synthesizes and a replay column the executor silently drops. One pattern over the pair refuses both; a graph that binds no latent runs at any seed it was never handed.
- Law: the LEASE binds tensor lanes to ROLE keys. Requests carry no output roster; the leased session reports one `TileProduct` row per card binding — the graph's own output tensor, the component lane inside it, and the opaque role key the product publishes under — so a PACKED export naming one tensor for several products lands each lane under its own role, and the executor never reads a role off a tensor's name.
- Law: a tiling plan has ONE construction. `StageRequest.Plan` folds the request's own extent, bucket, overlap, pad, and derived scale columns into `TilePlan`, whose gate roster alone spells the fixed-bucket law, and that same value then seats the bound flow and drives the fold.
- Law: a GRADE leaves as a value, never as a blob. A `Measure` product is rank-0 — no content address, no plane write, no mosaic arena — so it crosses on `StageResult.Scores` beside the plane outputs and never enters the produced-output map, because nothing downstream samples a grade and a stage binding one would be binding a number as a tensor. Writing four bytes through the plane port to hand back one float mints a blob the specifying end must fetch to read a value the result already holds.
- Auto: `Admit` proves everything provable WITHOUT a model — extent, bucket, and pad legality prove here so a malformed request never reaches a session lease, while the plan itself builds after the lease because only the model names its own products. The stacked bound tensor memoizes per LAYOUT ROW on one lock-free keyed cell, because layout is the only property of a lease the placement reads and one stage holds up to three leases whose layouts may differ; a single-source stage fills the bound buffer directly and a multi-source stage hands each filler to the layout's own stack row, so a planar plane lands in its contiguous slice with no intermediate and a fill failure rides the rail out with no torn entry seatable.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Domain.Deterministic`/`Domain.IDrawLane`)
- Growth: a new stage column is one record field here landing in the same change as its `Runtime/wire#CONTRACT_EVOLUTION` slot row, whose arity probe then forces the pair; a new grant posture is one `LicenseClass` row; a new decode invariant is one `IConstraint<StageRequest>` conformance the accumulating fold already reads; a further reproducible draw is one `StageDraw` row carrying its own declared lane; a new wire column is one record field transcribed from the frozen roster at both ends in one change; a further execution backend is one `Model/providers#EP_AXIS` row declaring one `WireKey`, never a translation table here and never a second stage owner.
- Boundary: `StagePorts` is the ONLY route to a plane and the only route to durable parity custody. Compute holds no blob store, no artifact index, no codec, and no channel vocabulary — it reads and writes float planes and parity verdicts through injected legs the app root binds against the Persistence object and artifact lanes, exactly as `Model/sessions#SESSION_CAPSULE` binds its warm-artifact leg; the read leg is the index-keyed span filler and the change is Compute-local by construction — the port is Compute-declared, the strata forbid a reference either way, and the filler is a delegate the ROOT binds (a blob copy, or a `Runtime/archive#HDF_ARCHIVE` hyperslab fill for an archive-resident plane), so an archive-resident chained input re-enters without rehydrating whole and no PureHDF member lands on a Compute signature; the parity legs carry no rail outward because the root that owns the artifact write also owns the evidence cell its refusal parks on, and a read answering nothing degrades to the cold measurement the process memo already prices. Provider and precision spellings resolve at `Model/providers#EP_AXIS`, whose rows carry their own wire keys, so this record holds no translation table and a roster landing there crosses without an edit here. `StageSession.Flow` takes the built plan and the synthesized draw, so the bound shapes, the bound draw, and the fold's shapes have one source and the root binds bytes rather than re-deriving a distribution. `GridProduct` is NOT a wire mirror: the specifying end's `StageProduct` is an emission `[Union]` naming the role type on its own output rows, and this carrier is an internal per-grid field-and-grade pair — genuinely distinct concepts, so this end keeps a distinct name rather than a same-named twin reaching one S4 consumer. This wire mints no `tests/contracts/MANIFEST.md` entry — it never leaves the C# runtime.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// This roster and the specifying registry's licence table are BOTH transcriptions of the frozen five-key
// vocabulary — cross-branch equality tests the wire key, the strata forbid sharing the type, and each end carries
// only the columns its own dispatch reads (a grant verdict here, an admission rank there); a merged shape is
// exactly the strata reference this wire exists to avoid.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LicenseClass {
    public static readonly LicenseClass Permissive = new("permissive", grants: true);
    public static readonly LicenseClass Copyleft = new("copyleft", grants: true);
    public static readonly LicenseClass OpenRail = new("openRail", grants: true);
    public static readonly LicenseClass Research = new("research", grants: true);
    // Silent-licence models reach the registry as this row and carry no grant to run; the executing end re-checks it.
    public static readonly LicenseClass Blocked = new("blocked", grants: false);

    private LicenseClass(string key, bool grants) : this(key) => Grants = grants;

    public bool Grants { get; }

    // Wire records carry the roster STRING and this roster owns the resolution, so a spelling no row claims REFUSES
    // rather than degrading — unlike a provider, a grant has no report column a caller could read a substitution
    // off, so a defaulted licence would run an unknown model under a typo.
    public static Option<LicenseClass> FromWire(string wire) => TryGet(wire, out LicenseClass? row) ? Some(row!) : None;
}

// The reproducible-draw lanes this owner mints, as DECLARED ordinals off the kernel `IDrawLane` axis — the shape
// the branch determinism scar demands, so no folder invents a positional constant of its own and a row rename
// never re-keys a stored campaign.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StageDraw : IDrawLane<StageDraw> {
    public static readonly StageDraw Latent = new("latent", lane: 0L);

    private StageDraw(string key, long lane) : this(key) => Lane = lane;

    public long Lane { get; }

    static IReadOnlyList<StageDraw> IDrawLane<StageDraw>.Items => Items;
}

// --- [MODELS] ------------------------------------------------------------------------------
// One consumed product. An empty Stage names the intent's own plane and carries the blob key; a named Stage names
// its producer and role, whose already-held output the fold resolves.
public readonly record struct StageInput(string Stage, string Role, string Key);

// One produced plane, named by the ROLE key the lease's binding roster carried — the graph's tensor name is the
// executor's business and never reaches the wire, because a packed tensor names several of these rows at once.
public readonly record struct StageOutput(string Role, string BlobKey, int Width, int Height, string Transfer, string Format);

// One MEASURED scalar. A grade carries its value and its role key alone — no blob, no extent, no transfer band —
// because nothing downstream samples it and a consumer asking for a grade reads the number the result already
// holds instead of fetching four bytes out of a store.
public readonly record struct StageScore(string Role, double Value);

// Every column is the LOWERED primitive the codec wrote: the producer's own types name none of this record, the
// vocabulary keys resolve through the rosters THIS package owns, and `Op` is a correlation string echoed verbatim
// onto the result rather than a value re-minted from a key. Extents land `int` because every grid derivation and
// span index downstream runs in that domain and the wire's `uint32` widens losslessly. Nineteen columns mirror
// the producing record's nineteen; declaration order diverges on `Artefact` alone, which the crossing's
// slot-ordinal digest makes lawful.
public sealed record StageRequest(
    string Stage, string ModelCardId, string License, Seq<StageInput> Inputs,
    int InputWidth, int InputHeight, int OutputWidth, int OutputHeight,
    int TileWidth, int TileHeight, int Overlap, string Pad, string Bucket,
    string Layout, // the producer graph's dimension-order token (NCHW/NHWC) — the binder seats tensors by it
    string Provider, string Precision, ulong Seed, string Op, string Artefact) {

    // Wire spellings resolve at the ROSTER that owns the rows, so this record holds no translation table and a
    // provider, precision, or licence landing there crosses without an edit here. The asymmetry is deliberate: a
    // substituted provider is reported on `ProviderUsed`, a substituted precision or grant is reported nowhere, so
    // one degrades and the other two refuse. The interior column shortens to `License` because a `LicenseClass`
    // member would SHADOW the `LicenseClass` type inside this record exactly as `PadMode` would — the same
    // interior-versus-wire split every other flat column takes, and the wire projection restores `licenseClass`.
    // PROVIDER resolution is deliberately NOT a property here: it answers what this host can run, which is a
    // property of the runtime rather than of the request, so the executor resolves it once against the frozen
    // provider census and threads that answer — a per-read property invites two reads of one decision.
    public Option<ModelPrecision> SelectedPrecision => ModelPrecision.FromWire(Precision);

    public Option<LicenseClass> SelectedLicense => LicenseClass.FromWire(License);

    // Scale is DERIVED from the extents the wire already threads: a stage publishes `inputWidth × scale`, so a
    // carried column could only ever contradict them, and a fractional or anisotropic ratio is a grid nothing builds.
    public Option<int> Scale =>
        InputWidth > 0 && InputHeight > 0
        && OutputWidth % InputWidth is 0 && OutputHeight % InputHeight is 0
        && OutputWidth / InputWidth == OutputHeight / InputHeight
            ? Some(OutputWidth / InputWidth)
            : None;

    // ONE plan construction lives here. Extent, bucket, overlap, pad, and scale come off this record; channels,
    // roster, layout, and blend come off the leased session, the only surface knowing the model. `TilePlan`'s own
    // gate roster then owns the fixed-bucket law, so no predicate here restates it and no later compare exists to
    // catch two spellings drifting. The wire's `padMode` field lands as the `Pad` column: a same-named `PadMode`
    // property SHADOWS the `PadMode` type inside this record (simple-name lookup binds the string member and
    // `string.TryGet` is CS1061; the static gate below hits CS0120), so the interior spelling shortens and the
    // wire projection restores `padMode` — the same interior-versus-wire split every other flat tile column takes.
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

    // DECODE gate: everything provable WITHOUT a model, accumulated so a request breaking three columns names
    // three. The plan itself builds after the lease, because only the model names its own products; `StageRun`
    // re-proves the grant alone, the one column an executing end never takes on trust.
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

    // Frozen wire records PIN `padMode` at `reflect`; the `PadMode` family stays the general tiling vocabulary,
    // and this boundary — the one carrying the wire — is where the pin enforces.
    sealed class PadPinned : IConstraint<StageRequest> {
        public Validation<Error, StageRequest> Check(StageRequest request) =>
            StringComparer.Ordinal.Equals(request.Pad, PadMode.Reflect.Key)
                ? request
                : StageRefusal.PadPinned.Fault();
    }

    sealed class BucketAgrees : IConstraint<StageRequest> {
        public Validation<Error, StageRequest> Check(StageRequest request) =>
            request.Scale.IsSome
            && StringComparer.Ordinal.Equals(request.Bucket, $"{request.TileWidth}x{request.TileHeight}")
                ? request
                : StageRefusal.Shape.Fault();
    }
}

// `ParityFresh` and `Coverage` are the two columns that make the other measured columns readable. Decorators at
// the consuming end see every result identically and cannot know a memo answered it, so a residual histogram keyed
// on `GoldenDelta` alone reads N observations where ONE measurement was taken — the discriminant therefore rides
// the RESULT, set true only by the arm that actually leased a floor session and ran both probes, false on a memo
// hit and false on the floor identity where the zero is a definition rather than an observation. `Coverage`
// carries the measured overlap-add weight floor: the mosaic gates on it once and a reassembly at 0.001 publishes
// as healthy without it. `Artefact` is the third: the digest of the weight bytes the leased session actually
// LOADED, measured at this end because only the lease observes which bytes reached the runtime, so the specifying
// end's card gate proves an observation rather than trusting a request round-tripped. Its slot ordinal sits past
// `Op` on the wire while the column reads beside `ModelCardId` here — the crossing folds slot ordinals and wire
// names alone, so declaration order is not contract identity. The specifying package owns this wire record; this
// fence transcribes the owner's shape and adds no column of its own.
public sealed record StageResult(
    string Stage, string ModelCardId, string Artefact, Seq<StageOutput> Outputs, Seq<StageScore> Scores,
    string ProviderUsed, int PartitionCount, double ElapsedMs, double GoldenDelta, bool ParityFresh, float Coverage,
    int TilesEmitted, string Op);

// What ONE executed grid produced, measured columns and the lease's own observations together. Fields and grades
// ride separate collections because they are separate MODALITIES, not one collection with a small extent: a field
// carries a blob key and an output extent, a grade carries a number. The demotion arm and the production arm
// answer this same shape, which is what lets a breach re-run at the floor and return through the same seam
// instead of forking the result construction. The name is DELIBERATELY not the specifying end's `StageProduct`,
// which is an emission `[Union]` naming the role type on its own rows — a genuinely different concept whose
// same-named twin would reach one S4 consumer under two meanings.
public readonly record struct GridProduct(
    Seq<StageOutput> Products, Seq<StageScore> Scores, int Partitions, float Coverage, int Tiles, string Artefact);

// The card's parity envelope, mirrored at the shape the specifying end declares it in. `Upper` is the gate every
// comparison reads; `Lower` is the DECLARED not-a-point state — a deterministic card diverges on the provider axis
// alone and its floor is its ceiling, while a stochastic card's band spans a seed sweep no single run performs, so
// absence there states an unmeasured floor rather than a weakened gate. `Admits` folds finiteness into the same
// read, so a non-finite residual can never pass as within envelope.
public readonly record struct ResidualBand(Option<double> Lower, double Upper) {
    public static ResidualBand Point(double ceiling) => new(Some(ceiling), ceiling);

    public static ResidualBand Ceiling(double upper) => new(Option<double>.None, upper);

    public bool Admits(double delta) => double.IsFinite(delta) && delta <= Upper;
}

// The seed-driven graph input NO upstream stage emits: the graph's own tensor, the channel depth of the draw, and
// the factor the tile extent divides by. It rides the lease rather than an input binding because nothing produces
// it — the executor mints it — and a card declaring none answers absence, at which point no draw exists to bind.
public readonly record struct LatentInput(string Tensor, int Channels, int Downscale) {
    // Extent divides by the declared downscale; the specifying end gates that at its own declaration, and this
    // fold proves it once more rather than packing a fractional grid the session would reject as a shape fault.
    public Fin<LatentDraw> Draw(TilePlan plan, ulong seed) =>
        Channels > 0 && Downscale > 0 && plan.TileWidth % Downscale is 0 && plan.TileHeight % Downscale is 0
            ? Fin.Succ(new LatentDraw(
                Tensor,
                plan.Layout.Shape(Channels, plan.TileHeight / Downscale, plan.TileWidth / Downscale),
                Normal(seed, Channels * (plan.TileHeight / Downscale) * (plan.TileWidth / Downscale))))
            : StageRefusal.LatentGrid.Fault<LatentDraw>();

    // A standard-normal draw the SEED alone determines. The uniform stream is the KERNEL's: the mixer transcribed
    // here was the owner's own splitmix64 finalizer copied verbatim except for the unit projection, so the two
    // spellings produced different doubles from one state and a replay across them diverged — this composition is
    // a correctness fix, not thrift. The lane-keyed STATELESS draw keys `(lane, index, dimension)` directly rather
    // than advancing a state, so the draw at a given index is a pure function of the seed on every host and no
    // partition order can reorder it; the open-interval clamp is the owner's, which is what keeps the logarithm
    // below defined. Box-Muller stays HERE because no kernel member covers the polar pair, and one rotation fills
    // two texels with no rejection loop, so stream position never depends on the values drawn. The bound `Draw`
    // carrier is declined at this site: its `At` materializes an `ImmutableArray` per texel where the span-keyed
    // entry allocates nothing across a latent of tens of thousands.
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

// The synthesized latent: the graph's own input tensor, its bound shape in the session's layout, and the draw
// filling it. The executor hands the ROOT bytes rather than a seed, so the distribution has exactly one
// implementation and a binder cannot re-derive a second one that replays differently.
public sealed record LatentDraw(string Tensor, long[] Shape, ReadOnlyMemory<float> Values);

// The plane read answers extent AND filler in ONE resolution: a separate stat leg would let two answers about one
// blob disagree, and a materializing byte read is the double copy the filler seam deletes.
public sealed record PlaneSource(int Width, int Height, int Channels, PlaneFill Fill);

// Multi-input stages bind ONE tensor: the wire's row order IS the channel-stack order, and the session's layout
// row owns the placement — planar appends whole channel planes, interleaved interleaves each texel's channel run
// — so a single-input stage crosses untouched and no second bound value exists to drift from the warm shape. The
// carrier exists because a stage holds up to three leases (the candidate probe, the floor probe, the production
// run) whose layouts may differ: the channel sum folds ONCE at the mint and each distinct layout stacks at most
// once, where a per-lease stack re-traversed the whole plane sequence four times for one stage. The memo keys on
// the layout row itself because layout is the only property of a lease the placement reads, and it rides a
// KEY-GRAINED lock-free cell rather than a mutable dictionary — the sibling memo on the parity owner is guarded
// and this one was not, two disciplines for one problem under a class whose own comment names three concurrent
// leases.
public sealed class PlaneStack {
    private readonly Seq<PlaneSource> sources;
    private readonly AtomHashMap<TileLayout, ReadOnlyMemory<float>> stacked = AtomHashMap<TileLayout, ReadOnlyMemory<float>>();
    private readonly int texels;

    private PlaneStack(Seq<PlaneSource> sources, int channels, int texels) =>
        (this.sources, Channels, this.texels) = (sources, channels, texels);

    public int Channels { get; }

    public static PlaneStack Of(Seq<PlaneSource> sources, StageRequest request) =>
        new(sources, sources.Sum(static source => source.Channels), request.InputWidth * request.InputHeight);

    // Single-source stages fill the bound tensor DIRECTLY — one buffer, one fill, layout-independent, exactly the
    // one materialization the byte read used to pay, now landing where it is consumed (and archive-fillable).
    // Multi-source stages hand each filler to the layout's own stack row, so a planar plane lands in its
    // contiguous slice with no intermediate and every distinct layout stacks at most once. A fill failure rides
    // the rail out — a torn memo entry is unrepresentable because seating happens only after every fill lands, and
    // the seat is one keyed CAS rather than a read followed by a write another lease could interleave.
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

// --- [ERRORS] ------------------------------------------------------------------------------
public static class StageRefusal {
    public static readonly ContractRefusal License = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Blocked = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Precision = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Pad = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal PadPinned = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Scale = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal Shape = new(ComputeArea.Model, ComputeContract.Compatible);
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
    public static readonly ContractRefusal LayoutMismatch = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal Band = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal GoldenShape = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal GoldenNonFinite = new(ComputeArea.Model, ComputeContract.Valid);

}

// --- [SERVICES] ----------------------------------------------------------------------------
// Everything about a run that only the LEASE knows: the card's binding roster naming each product's graph tensor,
// component lane, role key, and width, the tensor layout its graph emits, the taper profile its class of estimator
// wants, the partition count that bucket's warm-up measured, the bound input's own CHANNEL width (`InputMetadata`
// again — the gate the stacked input sum proves against), the card's parity `ResidualBand`, the digest of the
// weight bytes it loaded, and the LATENT its card declares — absent on a deterministic graph, which is what makes
// a seed nothing can bind refusable rather than silently dropped. `Flow` takes the BUILT plan and the SYNTHESIZED
// draw and binds every value from them, so shapes the flow holds, the draw it runs, and shapes the fold writes
// have one source. Holding the lease as this record's own disposable keeps its session alive across the whole grid.
public sealed record StageSession(
    Seq<TileProduct> Products, TileLayout Layout, TileBlend Blend, TileAdmission Admission, Option<int> Partitions, int PartitionCap,
    int InputChannels, ResidualBand Residual, string Artefact,
    Option<LatentInput> Latent, Func<TilePlan, Option<LatentDraw>, Fin<BoundFlow>> Flow, IDisposable Hold);

// Every plane crosses as a content address the host resolves; Compute holds no store, no codec, and no
// vocabulary. `Read` resolves a key to its declared extent and an index-keyed `PlaneFill` — the filler fills a
// caller-owned span, so plane bytes land where they are consumed and the app root binds an archive-resident plane
// to a hyperslab fill without rehydrating whole; the port itself still names no PureHDF member. `Lease` takes the
// resolved precision because a posture admitted at the wire and dropped before the session is a column the receipt
// then reports without anything having executed it. `Describe` takes the ROSTER ROW rather than a role string: the
// specifying end declares transfer and format per `(tensor, lane)` binding, so a packed export's two products
// describe apart and a role alone could not tell the port which binding it was answering for.
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
- Law: EVERY input row binds or the request refuses. Frozen-wire records carry one `StageInput` row per consumed product in the card's own binding order, and the executor resolves them ALL — a chained row against its producer's held output, an empty-stage row against its blob key — then STACKS the planes along the channel axis in that order into the one bound tensor, the session's own `InputMetadata` channel width proving the sum. Head-taking that silently drops `inputs[1..]` runs the card without the photograph its estimator consumes, and nothing rails.
- Law: a chained input RE-ENTERS through the port and never bypasses it. Producer planes leave through `StagePorts.Write` at the transfer and format `Describe` chose, and the host alone knows whether that crossing is lossless. Device-resident handoff is unreachable here for a second and independent reason: the mosaic overlap-adds every field into pooled HOST planes and `TileMosaic` owns those rentals, so no producer `OrtValue` survives a grid for a consumer to bind, and this fold reaches no `SessionPlacement` readback to compare residency with. Device-to-device copies belong where a bound output stays resident — one `Tensor/residency#ORT_BRIDGE` relay over a `BoundFlow` pair — never on this fold.
- Law: every input row of one stage shares ONE extent. The request declares `InputWidth`/`InputHeight` and every consumed plane matches it — a chained row proves against its producer's published extent at RESOLUTION, before the blob read and before the lease, and a source row proves against its own bytes at read-back — because the channel stack lays every plane into one bound tensor over one texel count, so a second extent has nowhere to go. Both refusals name both extents, where a bound session's shape fault three ports later names neither stage that disagreed.
- Law: the ARTEFACT pins at the lease, not at the far end. `StageRequest.Artefact` carries the weight digest the model card declared and `StageSession.Artefact` the digest the lease loaded, so the ONE seam every lease crosses proves them equal before a grid runs — comparing only where the result lands pays a whole mosaic, and worse, grades a parity residual against weights nobody asked for and seats that verdict in the memo. `StageResult.Artefact` then reports the MEASURED value rather than echoing the request. The LAYOUT column crosschecks at that same seam and for the same reason: the wire carries the producer graph's dimension-order row key so a binder can seat tensors before a lease returns, and the lease reports the layout the model actually emits — a column the wire carries and nothing compares is a claim rather than a contract.
- Law: the attempt's independent guards ACCUMULATE. The stacked channel sum, the measured partition cap, and the seed-and-latent pairing are three facts about one lease that do not depend on one another, so a lease breaking all three names all three; only the partition READ is sequenced before its cap, because a cap over an absent measurement has nothing to compare.
- Law: evidence publishes MEASURED or refuses. `PartitionCount` reads the per-bucket warm evidence the session capsule measured once, never a zero standing in for an unmeasured run; a request whose bucket carries no partition measurement refuses rather than minting a result whose evidence column reads as observed. Registration seats that bucket under its `WarmKey` with an ABSENT count and only the trace-reading `Model/run#RUN_MODES` `WarmPulse` fills it, so the two surfaces divide cleanly: the composition registers the shapes it will run, and the pulse measures how the graph partitioned for each.
- Law: the residual GATES, never merely reports, and the BAND rides the lease rather than the memo. Every lease carries the card's `ResidualBand` into `StageSession`, the run's own lease grades the measured delta against the band's `Upper`, and a breach DEMOTES to the floor at full precision — one demotion, `ProviderUsed` reporting the substitution, `GoldenDelta` keeping the measured breach — so an accelerated run outside its card's band never publishes as if it were inside. Freezing the band into the verdict at measurement time is the rejected form: a card widening its band keeps demoting against the frozen one until a re-measurement of an unchanged residual clears it.
- Auto: `Fold` threads a produced-OUTPUT map so a binding naming a producer resolves against results already held. `Execute` resolves the provider once against the frozen census, resolves and reads EVERY input row, runs the horizon-gated memoized parity measurement, then leases at that decision, proves the artefact digest and the layout row, accumulates the attempt guards, builds the plan against the session's binding roster, synthesizes the draw from the request's seed, opens ONE bound flow at that plan and that draw, runs the grid once, writes every produced plane through the port, carries every grade out by value, and folds the elapsed span off the timeline. A breach answers `None` inside that lease and the run re-leases once at the floor, so the demotion costs exactly one extra lease and only on the runs that earned it.
- Receipt: each executed stage emits one `ComputeReceipt.ModelRun` with the tiled mode key and the mosaic's tile count as `BatchSize` — one grid ran, so one receipt mints whatever the roster's width; the stage-level evidence rides `StageResult` across the wire, never a second receipt case, because the specifying end owns the admission that reads it.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, `Parametric.MonotonicTimeline`)
- Growth: a further attempt invariant is one `IConstraint<StageAttempt>` conformance; a stage emitting more products is more `TileProduct` rows the lease reports, a stage PACKING two products into one tensor is one more row at that tensor's next lane, a stage GRADING its input is one `TileProduct.Measure` row landing on `StageResult.Scores`, and a stage CONSUMING more products is one more wire input row widening the channel stack — no surface move on any of them.
- Boundary: the `GridProduct`→`StageResult` projection is NOT a Mapperly correspondence and no `[Mapper]` is owed here: both shapes are this package's own, the crossing folds three independently measured columns (the timeline span, the graded delta, the freshness discriminant) that no generated transcription can produce, and the pure columns it does carry are one owner's carrier feeding its own result rather than an owner↔DTO rename. The `[Mapper]`-earning correspondence on this wire lives at the specifying end, which authors the rich records this one transcribes.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The lifted candidate the attempt's accumulating gates read. Lifting once is what lets three independent facts
// fold applicatively where each inline term would otherwise re-read the same four values.
public readonly record struct StageAttempt(StageRequest Request, StageSession Session, TilePlan Plan, int Partitions);

// --- [OPERATIONS] --------------------------------------------------------------------------
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
        // The span is a monotone PAIR whose stamps carry their own timeline identity, so the subtraction proves
        // both marks came from one source — a stored raw tick belongs to no timeline and cannot.
        from opened in timeline.Capture()
        // Grants re-check HERE even when `Admit` already ran at decode: any host edge reaches this end, and the
        // caller's SPELLING resolves against the roster again rather than a resolved row riding in on the record.
        from licensed in request.SelectedLicense.ToFin(StageRefusal.License.Fault())
        from _ in guard(licensed.Grants, (Error)StageRefusal.Blocked.Fault()).ToFin()
        from precision in request.SelectedPrecision.ToFin(StageRefusal.Precision.Fault())
        from keys in Sources(request, produced)
        from planes in keys.Traverse(key => ports.Read(key).ToValidation()).As().ToFin()
        // Chained rows already proved their extents against their PRODUCERS at resolution; this guard proves every
        // source's DECLARED extent against the request's declaration — the whole gate for a source plane no stage
        // made, and the congruence every stacked row must share; the bytes themselves prove at fill, where the
        // filler's own destination length is the arithmetic witness.
        from __ in planes
            .Traverse(plane => guard(
                    plane.Width == request.InputWidth && plane.Height == request.InputHeight,
                    (Error)StageRefusal.ExtentMismatch.Fault())
                .ToFin().ToValidation())
            .As().ToFin()
        // One stack per stage: the parity probes and the production run share it, so the plane sequence is
        // traversed once for the sum and once per DISTINCT layout rather than once per lease.
        let stack = PlaneStack.Of(planes, request)
        // Provider resolution answers what this HOST can run, so it resolves once against the frozen census and
        // threads forward; precision already refused an unrostered spelling above.
        let requested = ExecutionProvider.FromWire(request.Provider)
        // Parity measures BEFORE the grid and decides nothing about the envelope: the card's live band rides the
        // lease the run itself takes, so a breach demotes there and the result reports the demotion on
        // `ProviderUsed` while `GoldenDelta` keeps the measured breach.
        from verdict in Assured(request, ports, stack, requested, precision, options, scope, clock)
        from outputs in Run(request, ports, stack, verdict.Verdict, requested, precision, options, scope)
        from closed in timeline.Capture()
        from elapsed in timeline.Elapsed(opened, closed)
        select new StageResult(
            request.Stage, request.ModelCardId, outputs.Product.Artefact, outputs.Product.Products,
            outputs.Product.Scores, outputs.Provider.ReportKey, outputs.Product.Partitions,
            elapsed.TotalMilliseconds, verdict.Verdict.Delta, verdict.Fresh,
            outputs.Product.Coverage, outputs.Product.Tiles, request.Op);

    // Chained stages NEVER carry the source plane: a binding naming a producer resolves against results already
    // held, so a pipeline whose links never touch is unrepresentable rather than merely discouraged. EVERY row
    // resolves — the frozen wire carries one row per consumed product in the card's binding order, and that order
    // is the channel-stack order the bound tensor takes. Each producer's PUBLISHED extent proves against this
    // request's declared input extent HERE — before the blob read and before the lease — because a plan defect
    // caught by a bound session's shape mismatch names a port rather than the two stages that disagree.
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

    // ONE lease, ONE plan, ONE grid, and the band gate INSIDE that lease. Leasing reports the model's binding
    // roster and the card's live `ResidualBand`, the request folds the roster into a plan, that plan seats the
    // flow, and the grid runs once for every plane the model emits. `None` is the DEMOTION signal and nothing
    // else: the lease that reads the card's authority is the same lease that would have run the grid, so the
    // decision happens where the authority lives, the floor re-lease is paid only by a run that was about to
    // publish outside its envelope, and a widened band re-grades an already-measured residual on the very next
    // request with nothing re-measured. The demoted PRECISION needs no column of its own: the floor row at full
    // precision is the one demotion this fold performs, so `providerUsed == cpu` already names it.
    static Fin<(GridProduct Product, ExecutionProvider Provider)> Run(
        StageRequest request, StagePorts ports, PlaneStack stack, ParityVerdict verdict,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope) =>
        Attempt(request, ports, stack, verdict, selected, precision, options, scope)
            .Bind(attempted => attempted.Case is GridProduct produced
                ? Fin.Succ((produced, selected))
                : Attempt(request, ports, stack, ParityVerdict.Identity, ExecutionProvider.Floor, ModelPrecision.Full, options, scope)
                    .Bind(floored => floored.Case is GridProduct onFloor
                        ? Fin.Succ((onFloor, ExecutionProvider.Floor))
                        // The floor arm grades the IDENTITY delta against the same band, so only a card whose
                        // `Upper` is itself negative or non-finite reaches here — a demotion loop is
                        // unrepresentable rather than merely unlikely.
                        : StageRefusal.Band.Fault<(GridProduct, ExecutionProvider)>()));

    // The three independent facts about one lease accumulate; only the partition READ sequences before its cap,
    // because a cap over an absent measurement has nothing to compare. REGISTRATION IS NOT MEASUREMENT: the
    // composition registers each bucket through `ModelSessions.Warm` under its `WarmKey`, which seats
    // `WarmEvidence` with an absent partition count, and only the trace-reading warm pulse fills it — so an
    // unmeasured bucket names a pulse the composition has not injected, never a caller error.
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

    // Stacked channel sums prove against the width the MODEL's own `InputMetadata` declares — the one surface
    // knowing how wide the bound tensor is — so a plan missing an input row or carrying a stray one refuses by
    // arithmetic before any texel moves.
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

    // Only the leased session knows whether the graph takes a draw at all, so the seed-and-latent gate sits here —
    // ONE pattern over the pair, because the two refusals are the two halves of one contradiction: a latent
    // nothing seeds and a seed nothing binds.
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

    // Lease and plan form ONE bracket: the hold releases inside the bind that took it, so a plan refusing never
    // strands a session and a fault never leaves a resident held. Artefact and LAYOUT pin HERE, at the one seam
    // every lease crosses — the production run and both parity probes — so a session holding weights the request
    // never named refuses before a grid runs rather than after the specifying end rejects a whole mosaic, a
    // residual graded on the wrong weights never enters the memo, and a dimension-order disagreement surfaces at
    // the end that holds both spellings instead of as a shape fault three ports later naming neither.
    static Fin<T> Leased<T>(
        StageRequest request, StagePorts ports, ExecutionProvider provider, ModelPrecision precision,
        int sourceChannels, Func<StageSession, TilePlan, Fin<T>> use) =>
        ports.Lease(request, provider, precision).Bind(session => {
            using (session.Hold) {
                return guard(
                        StringComparer.Ordinal.Equals(session.Artefact, request.Artefact),
                        (Error)StageRefusal.Artefact.Fault()).ToFin()
                    .Bind(_ => guard(
                            TileLayout.TryGet(request.Layout, out TileLayout? declared)
                            && ReferenceEquals(declared, session.Layout),
                            (Error)StageRefusal.LayoutMismatch.Fault()).ToFin())
                    .Bind(_ => request.Plan(sourceChannels, session.Products, session.Admission, session.Blend, session.Layout))
                    .Bind(plan => use(session, plan));
            }
        });

    // GRADES bypass both ports: no `Describe` because a number carries no transfer band, no `Write` because a blob
    // the specifying end must fetch to read one float is a store round trip for a value the result already holds.
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
                                     produced.Product.Role, key, plan.OutputWidth, plan.OutputHeight,
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

    // ONE synthesis site for the production grid and both parity probes. The draw is a pure function of the
    // request's seed and the plan's own tile extent, so the canary and the grid bind the SAME latent and the
    // residual grades the provider rather than two independent draws.
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
- Boundary: `ParityPort` carries no rail outward: the root that owns the artifact write also owns the evidence cell its refusal parks on, so a failed artifact write never fails an inference whose measurement succeeded, and a read answering nothing degrades to the cold measurement the memo already prices. Memo capacity is a POLICY VALUE on the port beside `Horizon` rather than a constant this fold reads — "bounded by the key product" only bounds anything if the product does, and a long-lived root serving a growing model registry grows it without ceiling.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Parity verdicts travel as VALUES and carry only what was MEASURED. Residuals are a property of
// `(card, provider, precision, runtime, device, host)` rather than of a process, so one shape keys the in-process
// memo and the durable row and neither tier can key differently. `MeasuredAt` is the second measured column and
// the reason the first stays trustworthy: the key names the silicon but not the driver or firmware stack running
// on it, and a driver revision changes a residual without changing one term of that key — so age, not a key term,
// is what retires a verdict. The BAND is deliberately absent: it belongs to the model card, rides the lease, and
// is read live at every gate, where freezing it here would keep demoting against an envelope the card has since
// widened.
public readonly record struct ParityVerdict(double Delta, Instant MeasuredAt) {
    // The floor at full precision answers itself, so the delta is zero by DEFINITION rather than by measurement
    // and the instant is the epoch nothing observed — `ParityFresh` on the result is what tells a reader which.
    public static readonly ParityVerdict Identity = new(0d, Instant.MinValue);
}

// --- [SERVICES] ----------------------------------------------------------------------------
// Parity's DURABLE half injects exactly as every plane crosses. Compute holds no artifact store: the app root
// binds these legs against the Persistence artifact lane and supplies the running `HostFingerprint`, so the
// durable key and the process key are ONE derivation. `Read` answering `None` is a miss rather than a fault — an
// unbound lane costs exactly the two leases and two probes the memo would have saved — and `Write` returns `Unit`
// by contract because the composing root parks its own write refusal on its own evidence cell. `Horizon` and
// `Capacity` ride here because this is the record the composing root builds: a verdict older than the horizon
// reads as ABSENT and re-measures, and the capacity is a policy value a root serving a growing registry raises.
public sealed record ParityPort(
    HostFingerprint Host,
    Duration Horizon,
    int Capacity,
    Func<string, Option<ParityVerdict>> Read,
    Func<string, ParityVerdict, Unit> Write) {
    // Thirty days bounds a verdict against the cadence a host's driver stack actually moves at, and it is the
    // composition's value rather than a constant this fold reads — a root serving volatile silicon shortens it.
    public static readonly Duration CanonicalHorizon = Duration.FromDays(30);

    public const int CanonicalCapacity = 512;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class StageRun {
    readonly record struct ParityMemo(ParityVerdict Verdict, long Read);

    // Key-grained and lock-free: seating, touching, and trimming are all key-local transitions over an immutable
    // map, and neither runs a native effect — which is the whole reason the sibling capsule's `Lock` is right
    // there and wrong here. `ParityTick` rides the same cell family rather than a bare mutable static.
    static readonly AtomHashMap<string, ParityMemo> Parity = AtomHashMap<string, ParityMemo>();
    static readonly Atom<long> ParityTick = Atom(0L);

    // ONE key across both tiers: the card names the model at its checksum, the provider's own `ResultKey` folds
    // the runtime version, the precision, every behavior option that shaped the compiled graph, and the RANKED
    // device's fingerprint, and the host fingerprint pins the machine whose silicon produced the residual — a
    // verdict measured on other silicon is another host's verdict, and reading it here would gate an acceleration
    // nothing on this machine graded. The device comes off `AutoSelect`'s own rank rather than off a lease,
    // because the key must resolve BEFORE any session opens and the rank is what the lease will take.
    static string ParityKey(StageRequest request, ExecutionProvider provider, ModelPrecision precision, ParityPort parity) =>
        $"{request.ModelCardId}:{provider.ResultKey(OrtEnv.Instance().GetVersionString(), precision, provider.AutoSelect.HeadOrNone())}:{parity.Host}";

    // Floor-at-full answering itself is an IDENTITY, so nothing was observed and `Fresh` is false — the
    // discriminant a reader needs is `providerUsed == cpu` at `fp32`, never a zero posing as a measurement.
    static Fin<(ParityVerdict Verdict, bool Fresh)> Assured(
        StageRequest request, StagePorts ports, PlaneStack stack,
        ExecutionProvider selected, ModelPrecision precision, RunOptions options, CancelScope scope, IClock clock) =>
        selected.IsFloor && ReferenceEquals(precision, ModelPrecision.Full)
            ? Fin.Succ((ParityVerdict.Identity, false))
            : Measured(request, ports, stack, selected, precision, options, scope, clock);

    // A hit measures nothing, so `Fresh` is false — which is what keeps a residual histogram counting
    // observations rather than requests; a per-inference tap cannot see this branch. Each lease releases inside
    // the bind that took it, and both probes run at the request's own seed.
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

    // Process memo answers first and the durable row second, and a durable hit SEATS the memo so a cold start pays
    // one artifact read per decision rather than one per request. Both tiers answer through ONE staleness gate: a
    // verdict past the port's horizon reads as ABSENT and re-measures, because the parity key names the silicon
    // but not the driver stack on top of it.
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

    // Eviction is least-recently-read and one PASS: the sorted-then-head form re-ranked every row on every insert
    // past the cap to discard all but one of the ranks it computed.
    static ParityVerdict Seat(string at, ParityVerdict verdict, ParityPort parity) {
        Parity.SwapKey(at, _ => Some(new ParityMemo(verdict, ParityTick.Swap(static tick => tick + 1L))));
        if (Parity.Count > parity.Capacity) {
            Parity.ToSeq()
                .Fold(Option<(string Key, long Read)>.None, static (coldest, row) =>
                    coldest.Case is (string _, long read) && read <= row.Value.Read ? coldest : Some((row.Key, row.Value.Read)))
                .Iter(oldest => Parity.Remove(oldest.Key));
        }
        return verdict;
    }

    static Fin<float[]> Probe(
        StageSession session, TilePlan plan, ulong seed, ReadOnlyMemory<float> source, RunOptions options, CancelScope scope) =>
        Drawn(session, plan, seed).Bind(latent => session.Flow(plan, latent)).Bind(flow => {
            using (flow) { return flow.Canary(options, scope, plan, source); }
        });

    // One fold serves both modalities: a GRADED canary's arrays hold exactly one element, so the max-magnitude
    // read IS the scalar absolute difference and a second residual arm would restate one arithmetic under a
    // modality name.
    static Fin<double> Residual(float[] candidate, float[] reference) {
        if (candidate.Length != reference.Length) {
            return StageRefusal.GoldenShape.Fault<double>();
        }
        using SpanOwner<float> difference = SpanOwner<float>.Allocate(candidate.Length);
        TensorPrimitives.Subtract(candidate, reference, difference.Span);
        float residual = TensorPrimitives.MaxMagnitude<float>(difference.Span);
        return float.IsFinite(residual)
            ? Fin.Succ((double)Math.Abs(residual))
            : StageRefusal.GoldenNonFinite.Fault<double>();
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
