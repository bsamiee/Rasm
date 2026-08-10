# [MATERIALS_OBSERVABILITY]

MATERIALS signal evidence starts with the closed `MaterialsFact` family: `MaterialsHooks` composes the kernel signal capsule into the folder's point rail, `MaterialsInstruments` projects the fact stream onto `rasm.materials.<domain>.<measure>` instruments as a rail subscriber, `MaterialsLog` carries the fixed-severity fault projection beside the `LatencyPhase`/`LatencyMeasure` vocabularies `MaterialsLatency` contributes as one checkpoint, measure, and pivot roster, and `MaterialsDescriptors` binds the folder's board pack over that roster.

Settled composition draws every mechanism from the kernel signal capsule — hook capsule, instrument capsule with its bucket advice and level cells, package-identity band, tenancy frame, and SLO algebra with its board-pack carrier; each fence prelude names the exact rows it binds, and fact payloads compose Component, Appearance, Properties, and seam receipts. Instrument names run dotted `rasm.materials.<domain>.<measure>` with UCUM units under the `TelemetrySource.Materials` scope the composing app admits by name, every work row carrying the kernel `rasm.tenant` partition.

## [01]-[INDEX]

- [02]-[FACT_FAMILY]: `MaterialsFact` closes the evidence union.
- [03]-[HOOK_RAIL]: `MaterialsPoint` closes the point vocabulary on its kernel `Modality` column and `MaterialsHooks` composes that roster over the kernel capsule.
- [04]-[INSTRUMENT_TAP]: `MaterialsInstruments` mounts the roster, the level bindings, the contributor port, and the rail projection.
- [05]-[EVIDENCE_RECORDS]: `MaterialsLog` carries the fixed-severity projection, `LatencyPhase` and `LatencyMeasure` close the bracket and quantity vocabularies over their `LatencyWrite` laws, and `MaterialsLatency` derives the contributed three-axis roster from them.
- [06]-[BOARD_PACK]: `MaterialsDescriptors` binds the kernel pack over that roster.

## [02]-[FACT_FAMILY]

- Owner: `MaterialsFact` — the closed evidence union every tap fires and every projection folds.
- Cases: `CatalogueAdmit` (the row a veto gate transforms or refuses pre-freeze), `SectionSolve` (profile case, solved section, wall duration), `CapacityCheck` (the lifted `CapacityReceipt`, the `Utilisation` verdict, wall duration), `GraphCompile` (material, ordered node count, wall duration), `AcquisitionFit` (the measured `Provenance` receipt, wall duration), `WireMint` (material, `WireProvenance` receipt), `ProjectionGate` (the `GraphDelta` a veto refuses or admits pre-merge), `TexturePress` (the lifted `PressReceipt` and the material it baked for), `TileSynth` (strategy, guide channel, and the lifted `TileReceipt` — the guide rides beside the receipt for the reason `StageInfer` carries its request: an unmeasured run still names the channel it ran against), `TileGrade` (strategy, the `Option<TileProof>` the gate answered with, wall duration — its own case because a grade runs without synthesis and an ingested set earns its proof having passed no synthesizer), `PyramidBuild` (channel, mip policy, level count, texel census, fold duration — the one texture construction every press, ingest, and decode pays per channel), `SetIngest` (the claimed-stem census, the typed refusal rows, and the resolved convention — the three columns `SetManifest` already carries), `PlaneCodec` (container row, direction, stored bytes, wall duration), `StageInfer` (the issued `StageRequest` and the lifted `StageResult` — the request rides so the tap can see a provider DEGRADATION, which the result alone cannot show, and it already carries the grant class, so a second licence column is two carriers for one value), `EnvironmentPrefilter` (light key, sky model, level count, wall duration).
- Entry: each composition-root decorator fires one case after the owning entrypoint settles; veto cases fire before catalogue freeze or graph merge.
- Auto: elapsed columns derive from one injected clock at the decorator boundary.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new evidence shape is one `MaterialsFact` case, one point row at `[03]`, and one projection arm at `[04]`.
- Boundary: facts carry receipts the owning pages already mint — `CapacityReceipt`, `Provenance`, `WireProvenance`, `ComputedSection`, `PressReceipt`, `TileReceipt`, `StageResult` — and never re-derive their scalars, so a bake's texel census, backend, and elapsed millisecond come off the press's own receipt, a tiling run's two independent signals off the gate's own score, and an inference's provider, partition count, and golden residual off the executor's own result rather than off a second measurement this tap keeps honest. `PlaneCodec` and `EnvironmentPrefilter` own no receipt, so each carries the four columns its arm reads and nothing more. `SetIngest` carries the manifest's own three columns because `SetManifest` is an accumulating monoid rather than a receipt, and its refusal rows cross TYPED, since a formatted token keys a counter on file stems and hands the roster an unbounded dimension it cannot close.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;                              // Op
using Rasm.Element.Composition;                 // MaterialId
using Rasm.Element.Graph;                       // GraphDelta
using Rasm.Materials.Appearance;                // Provenance
using Rasm.Materials.Appearance.Interchange;    // WireProvenance
using Rasm.Materials.Component;                 // ComponentRow, ComputedSection, CapacityReceipt, Utilisation
using Rasm.Materials.Raster;                    // PressReceipt, RasterFormat, TileReceipt, TileProof, TileStrategy, TextureChannel, MipPolicy,
                                                // IngestRefusal, NormalConvention — the bake, tiling, and ingest vocabularies the texture facts lift
// Rasm.Materials.Appearance already in scope: StageRequest, StageResult, LicenseClass — the inference request, result, and grant class
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [MODELS] -------------------------------------------------------------------------------
[Union]
public abstract partial record MaterialsFact {
    private MaterialsFact() { }

    public sealed record CatalogueAdmit(ComponentRow Row) : MaterialsFact;
    public sealed record SectionSolve(Op Key, string Profile, ComputedSection Section, Duration Elapsed) : MaterialsFact;
    public sealed record CapacityCheck(Op Key, CapacityReceipt Receipt, Utilisation Verdict, Duration Elapsed) : MaterialsFact;
    public sealed record GraphCompile(Op Key, MaterialId Material, int Nodes, Duration Elapsed) : MaterialsFact;
    public sealed record AcquisitionFit(Op Key, Provenance Receipt, Duration Elapsed) : MaterialsFact;
    public sealed record WireMint(Op Key, MaterialId Material, WireProvenance Receipt) : MaterialsFact;
    public sealed record ProjectionGate(GraphDelta Delta) : MaterialsFact;

    // Texture-generation facts lift the receipt each owner already minted rather than re-measuring: PressReceipt carries
    // backend, texels, elapsed, and the CPU-versus-GPU divergence, and StageResult carries the provider used, the graph
    // partition count, the golden residual, and the tiles inferred — so this family adds evidence SHAPES and no
    // arithmetic. Encoded is the binary direction axis the [04] arm publishes as its own two dimension values, the same
    // shape the adequacy verdict already takes.
    public sealed record TexturePress(Op Key, Option<MaterialId> Material, PressReceipt Receipt) : MaterialsFact;
    public sealed record TileSynth(Op Key, TileStrategy Strategy, TextureChannel Guide, TileReceipt Receipt) : MaterialsFact;
    // Grading lifts the OPTION the gate answers with, never a flattened boolean: the gate is total and answers
    // absence for a plane that tiles badly, so the fact carries what the gate produced and the arm partitions on
    // presence. Strategy is the POLICY's, because a graded ingest declares the strategy it was graded against
    // even where nothing synthesized it.
    public sealed record TileGrade(Op Key, TileStrategy Strategy, Option<TileProof> Proof, Duration Elapsed) : MaterialsFact;
    public sealed record PyramidBuild(Op Key, TextureChannel Channel, MipPolicy Policy, int Levels, long Texels, Duration Elapsed) : MaterialsFact;
    public sealed record SetIngest(Op Key, int Claimed, Seq<(IngestRefusal Reason, string Detail)> Unresolved, Option<NormalConvention> Convention) : MaterialsFact;
    public sealed record PlaneCodec(Op Key, RasterFormat Format, bool Encoded, long Bytes, Duration Elapsed) : MaterialsFact;
    public sealed record StageInfer(Op Key, StageRequest Request, StageResult Result) : MaterialsFact;
    public sealed record EnvironmentPrefilter(Op Key, string LightKey, string SkyModel, int SpecularMips, Duration Elapsed) : MaterialsFact;
}
```

## [03]-[HOOK_RAIL]

- Owner: `MaterialsPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.materials.<domain>.<point>` with the kernel `HookModality` column; `MaterialsHooks` the per-composition point roster composing the kernel capsule — one `HookPoint<TFact>` field per row, one shared `IsolatedFault` evidence cell, no process-global registry, since Materials holds no plugin-identity grant custody; every declared point carries a projection arm at `[04]`, so a point firing into nothing has no landing.
- Cases: point roster rows — `rasm.materials.catalogue.admit` veto (`CatalogueAdmit`), `rasm.materials.section.solve` observe (`SectionSolve`), `rasm.materials.capacity.check` observe (`CapacityCheck`), `rasm.materials.graph.compile` observe (`GraphCompile`), `rasm.materials.acquisition.fit` replay (`AcquisitionFit`), `rasm.materials.wire.mint` observe (`WireMint`), `rasm.materials.projection.project` veto (`ProjectionGate`), `rasm.materials.texture.press` observe (`TexturePress`), `rasm.materials.texture.tile` observe (`TileSynth`), `rasm.materials.texture.grade` observe (`TileGrade`), `rasm.materials.texture.pyramid` observe (`PyramidBuild`), `rasm.materials.texture.ingest` observe (`SetIngest`), `rasm.materials.texture.codec` observe (`PlaneCodec`), `rasm.materials.neural.infer` replay (`StageInfer`), `rasm.materials.environment.prefilter` observe (`EnvironmentPrefilter`). `rasm.materials.neural.infer` takes REPLAY for the reason `rasm.materials.acquisition.fit` does: both settle a costly external computation whose evidence a later run re-reads rather than re-earns.
- Entry: `MaterialsHooks.Live()` mints the roster once at composition by seating one kernel point per `MaterialsPoint` row; a decorator fires its declared point value, so a name-resolved lookup surface never exists; `Points` hands the point set to `HookRegistry.Mount` at the app root; the capsule's `Veto`/`Observe`/`Drain` are the subscriber entries.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new point is one `MaterialsPoint` row, one `MaterialsHooks` field with its `Live()` seat and `Points` entry, and one `MaterialsFact` case; delivery semantics are the kernel modality rows.
- Boundary: ids and modalities live on the roster rows alone, so a `Live()` seat re-spelling either is the forked-vocabulary defect and a Materials point joins any app-tier registry census unrenamed; fire order, veto folding, bounded replay, and fork-shielded isolation are the capsule's — a subscriber fault parks as `IsolatedFault` on the composition's cell and the emitter is untouched; one synchronous `Fire`, so an effect-composed decorator lifts at its own seam. Veto points carry observe subscribers legally and the capsule dispatches them from the admitted fact alone, so a `[04]` arm on a veto point counts admitted rows alone; refusal volume rides the shared fault cell that already records every veto verdict. Spans are absent by design: this folder's eager constructions carry the `[05]` checkpoint ledger instead, so no `TraceScope` plane derives off these ids and the roster admits no scope into any band.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                              // HookId, HookModality, HookPoint, IHookPoint, IsolatedFault
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [TYPES] ----------------------------------------------------------------------------------
// Point roster keyed rasm.materials.<domain>.<point> — the kernel HookId four-segment grammar. Modality is the
// kernel column deciding veto admission and replay retention, so id and delivery semantics belong to the row and
// a `Live()` seat re-spelling either is the forked-vocabulary defect a construction literal invites.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaterialsPoint {
    public static readonly MaterialsPoint CatalogueAdmit = new("rasm.materials.catalogue.admit", modality: HookModality.Veto);
    public static readonly MaterialsPoint SectionSolve = new("rasm.materials.section.solve", modality: HookModality.Observe);
    public static readonly MaterialsPoint CapacityCheck = new("rasm.materials.capacity.check", modality: HookModality.Observe);
    public static readonly MaterialsPoint GraphCompile = new("rasm.materials.graph.compile", modality: HookModality.Observe);
    public static readonly MaterialsPoint AcquisitionFit = new("rasm.materials.acquisition.fit", modality: HookModality.Replay);
    public static readonly MaterialsPoint WireMint = new("rasm.materials.wire.mint", modality: HookModality.Observe);
    public static readonly MaterialsPoint ProjectionGate = new("rasm.materials.projection.project", modality: HookModality.Veto);
    public static readonly MaterialsPoint TexturePress = new("rasm.materials.texture.press", modality: HookModality.Observe);
    public static readonly MaterialsPoint TileSynth = new("rasm.materials.texture.tile", modality: HookModality.Observe);
    // Grading is its OWN point because it runs without synthesis: an ingested third-party set is graded to earn
    // its TileProof and never passes through the synthesizer, so a grade folded into the synthesis point is
    // invisible for exactly the population whose tileability nothing else measures.
    public static readonly MaterialsPoint TileGrade = new("rasm.materials.texture.grade", modality: HookModality.Observe);
    // The pyramid fold is the one texture construction with no point at all, and it is the one every press,
    // ingest, and decode pays per channel — so a mip-policy choice that costs a fold has no series that shows it.
    public static readonly MaterialsPoint PyramidBuild = new("rasm.materials.texture.pyramid", modality: HookModality.Observe);
    public static readonly MaterialsPoint SetIngest = new("rasm.materials.texture.ingest", modality: HookModality.Observe);
    public static readonly MaterialsPoint PlaneCodec = new("rasm.materials.texture.codec", modality: HookModality.Observe);
    public static readonly MaterialsPoint StageInfer = new("rasm.materials.neural.infer", modality: HookModality.Replay);
    public static readonly MaterialsPoint EnvironmentPrefilter = new("rasm.materials.environment.prefilter", modality: HookModality.Observe);

    public HookModality Modality { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record MaterialsHooks(
    HookPoint<MaterialsFact.CatalogueAdmit> CatalogueAdmit,
    HookPoint<MaterialsFact.SectionSolve> SectionSolve,
    HookPoint<MaterialsFact.CapacityCheck> CapacityCheck,
    HookPoint<MaterialsFact.GraphCompile> GraphCompile,
    HookPoint<MaterialsFact.AcquisitionFit> AcquisitionFit,
    HookPoint<MaterialsFact.WireMint> WireMint,
    HookPoint<MaterialsFact.ProjectionGate> ProjectionGate,
    HookPoint<MaterialsFact.TexturePress> TexturePress,
    HookPoint<MaterialsFact.TileSynth> TileSynth,
    HookPoint<MaterialsFact.TileGrade> TileGrade,
    HookPoint<MaterialsFact.PyramidBuild> PyramidBuild,
    HookPoint<MaterialsFact.SetIngest> SetIngest,
    HookPoint<MaterialsFact.PlaneCodec> PlaneCodec,
    HookPoint<MaterialsFact.StageInfer> StageInfer,
    HookPoint<MaterialsFact.EnvironmentPrefilter> EnvironmentPrefilter,
    Atom<Seq<IsolatedFault>> Faults) {
    public static MaterialsHooks Live() {
        Atom<Seq<IsolatedFault>> faults = Atom(Seq<IsolatedFault>());
        return new(
            Seat<MaterialsFact.CatalogueAdmit>(MaterialsPoint.CatalogueAdmit, faults),
            Seat<MaterialsFact.SectionSolve>(MaterialsPoint.SectionSolve, faults),
            Seat<MaterialsFact.CapacityCheck>(MaterialsPoint.CapacityCheck, faults),
            Seat<MaterialsFact.GraphCompile>(MaterialsPoint.GraphCompile, faults),
            Seat<MaterialsFact.AcquisitionFit>(MaterialsPoint.AcquisitionFit, faults),
            Seat<MaterialsFact.WireMint>(MaterialsPoint.WireMint, faults),
            Seat<MaterialsFact.ProjectionGate>(MaterialsPoint.ProjectionGate, faults),
            Seat<MaterialsFact.TexturePress>(MaterialsPoint.TexturePress, faults),
            Seat<MaterialsFact.TileSynth>(MaterialsPoint.TileSynth, faults),
            Seat<MaterialsFact.TileGrade>(MaterialsPoint.TileGrade, faults),
            Seat<MaterialsFact.PyramidBuild>(MaterialsPoint.PyramidBuild, faults),
            Seat<MaterialsFact.SetIngest>(MaterialsPoint.SetIngest, faults),
            Seat<MaterialsFact.PlaneCodec>(MaterialsPoint.PlaneCodec, faults),
            Seat<MaterialsFact.StageInfer>(MaterialsPoint.StageInfer, faults),
            Seat<MaterialsFact.EnvironmentPrefilter>(MaterialsPoint.EnvironmentPrefilter, faults),
            faults);
    }

    // Mount table the app root audits every registered point through, folded into the one frozen `HookRegistry`
    // beside every sibling roster — duplicate ids across the whole composition are structurally fatal there.
    public Seq<IHookPoint> Points => Seq<IHookPoint>(
        CatalogueAdmit, SectionSolve, CapacityCheck, GraphCompile, AcquisitionFit, WireMint, ProjectionGate,
        TexturePress, TileSynth, TileGrade, PyramidBuild, SetIngest, PlaneCodec, StageInfer, EnvironmentPrefilter);

    private static HookPoint<TFact> Seat<TFact>(MaterialsPoint row, Atom<Seq<IsolatedFault>> faults) =>
        new(id: HookId.Create(value: row.Key), modality: row.Modality, faults: faults);
}
```

## [04]-[INSTRUMENT_TAP]

- Owner: `MaterialsInstruments` — the `rasm.materials.*` `InstrumentSpec` roster, the contributor port, and the rail-subscribed projection; the roster is composition-free data, so one declaration binds against any meter and any cells.
- Cases: tileability grades by strategy and proof verdict with the combined verdict of every minted proof off `TileGrade`; pyramid levels folded and fold duration by channel and mip policy off `PyramidBuild`; catalogue admissions by family off `CatalogueAdmit`; solve counts and duration off `SectionSolve`; capacity checks by adequacy verdict and governing utilisation off `CapacityCheck`; compile node census and duration off `GraphCompile`; fits by parameter-rank verdict and the residual off `AcquisitionFit`; wire mints by capture method off `WireMint`; projection admissions off `ProjectionGate`; press runs, texels, duration, downgraded channels, faulted texels, the gated GPU divergence, and the gated aging-ladder coverage per ladder axis by backend and channel off `TexturePress`; tiling runs by strategy, guide channel, and grade verdict with the three-component tileability signal and the synthesis duration off `TileSynth`; ingested stems by classification verdict and typed refusal reason off `SetIngest`; plane bytes and codec duration by container and direction off `PlaneCodec`; inference runs, partitions, and golden residual by stage, provider, licence, and fidelity off `StageInfer`; prefilter runs and duration by sky model off `EnvironmentPrefilter`; catalogue and library row levels off composition-bound readers; fault counts off the rail's `IsolatedFault` cell banded by kernel category.
- Entry: `MaterialsInstruments.Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl)` — the one contributor port, carrying the `[06]` board pack beside these rows so board and reliability policy travel downward with the roster they name; `MaterialsInstruments.Tap(MaterialsHooks hooks, InstrumentSet set, params ReadOnlySpan<(string Name, Func<double> Read)> levels)` binds the pulled readers and mounts the observe subscriptions at composition, so create and write calls live only inside this spine.
- Auto: every histogram row binds its named kernel `Buckets` row as the explicit-bucket fallback under the base2-exponential wire default, so no bound array is spelled here; every dimension key is a declared slot const carried on its own row's `Dimensions` column, so the governance leg derives view tag keys from the mounted roster rather than a second table; every write materializes its tag set through the kernel `InstrumentSet.Tags` entry, which returns the stack-allocated `TagList` the kernel's own `in TagList` write overload consumes and folds the ambient `TenantContext` partition in beside the arm's own slots, so a partitioned host attributes every work row and a single-tenant one mints no dimension at all — an arm widening a shared set for one row does it by COPY-THEN-ADD off that struct through the page's one `Keyed` widening, never by materializing a heap `KeyValuePair<string, object?>[]` beside the projection that exists to avoid it; a multi-write arm reads `Enabled` ahead of its tag mint and ahead of any receipt-collection walk it would otherwise fold for a listener that does not exist, and an unmounted name reads enabled so the gate never absorbs the refusal a write owes; each share indicator's outcome verdict rides the same write that counts the occurrence, so a good half can never miss an occurrence its denominator recorded and no second counter carries the numerator; a supplied reader binds through its own row's declared `MeasureForm`, so one supply shape serves a whole-count population and a real-valued level.
- Packages: Rasm, LanguageExt.Core, BCL inbox (`System.Diagnostics.Metrics`).
- Growth: a histogram policy change is one kernel `Buckets` row reference; a new instrument is one `InstrumentSpec` row and one tap arm carrying its own UCUM unit string — `{texel}`, `By`, `{partition}`, `{inference}`, `{tile}`, `{stem}`, `{channel}` — so a magnitude states what it counts and a board never infers a scale from an instrument name; a new tileability signal is one `ScoreComponents` row and no arm edit at all; a new pulled level is one `Level` row and one reader at the call site, never a signature edit.
- Boundary: throughput rides MONOTONE COUNTERS in UCUM units and latency rides the histograms — a bake spans four orders of magnitude between a preview and a production plane, so a bucket ladder over texel or byte volume grades nothing while the counter's own derivative is exactly the rate a board reads. `MaterialId` and the solved `ComputedSection` stay fact evidence with no arm — material identity is identifier-grade and belongs on spans and typed receipts, never on a metric series, and a solved section's column set is receipt truth the owning page already mints; tenancy is the kernel `TenantContext` projection every work-row write folds and every work row declares, so this page holds no tenant key, no baggage read, and no zero sentinel, while the two pulled population rows stay untenanted on ownership alone — a frozen catalogue and its library are process-scoped reference data no tenant owns — since the kernel's optional level key and its `Bind` registration both report a pulled reading under tags, so cadence stopped being a reason the moment a tagged pulled reading became spellable; every projection arm returns the kernel write rail and subscribes through the capsule's rail-shaped `Observe`, so a refused write parks as `IsolatedFault` beside every other tap fault and no folder-local lift aspect exists; level readers are composition-supplied and bound through the kernel `LevelCells`, so app-scoped isolation holds by construction; `Tap` proves the supply a bijection against the roster's own `Pulled` column — a name outside the pulled rows refuses, a pulled row with no reader refuses, and a name supplied twice refuses before its second bind shadows the first — so a population gauge reads the live catalogue and a cell nobody writes has no construction path; a whole-number level crosses the domain gate its cell declares, so a non-finite or out-of-range reading refuses rather than casting to an undefined value the series carries as a population; a REFUSAL is counted and never measured — an unmeasured run enters its counter's own verdict partition while every histogram gates on the evidence that proves a measurement was taken (a finite seam ratio, a present parity delta), because a sentinel admitted into a distribution reads to a board as the best value in it; live facts and replayed message envelopes remain mutually exclusive evidence paths at the composition root; instrument custody stays the composing app's — this spine binds and subscribes against a mounted `InstrumentSet` and mints no meter, so the fan that materializes the port is the one creation site.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Diagnostics;                       // TagList — the kernel tag projection's own carrier
using LanguageExt;
using Rasm.Domain;                              // Buckets, Fault, HookDetacher, HookPoint, InstrumentSet,
                                                // InstrumentSpec, IsolatedFault, KernelInstruments, MeasureForm,
                                                // TelemetryContributorPort, TelemetryIdentity, TelemetrySource,
                                                // TenantContext, FaultExtensions extension property Category
using Rasm.Materials.Raster;                    // TileScore, IngestRefusal — the two fanned dimensions this tap reads
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class MaterialsInstruments {
    public const string FamilySlot = "rasm.materials.family";
    public const string ProfileSlot = "rasm.materials.profile";
    public const string KindSlot = "rasm.materials.capacity.kind";
    public const string GoverningSlot = "rasm.materials.capacity.governing";
    public const string AdequacySlot = "rasm.materials.capacity.adequacy";
    public const string RankSlot = "rasm.materials.acquisition.rank";
    public const string MethodSlot = "rasm.materials.capture.method";
    public const string BackendSlot = "rasm.materials.press.backend";
    public const string AxisSlot = "rasm.materials.press.axis";
    // ONE texture-channel dimension across every texture row that names a channel — the tiling guide the gate
    // graded and the press channel that degraded are one axis under one key, so a board joins a tile verdict to
    // a press downgrade on the channel column; a second guide-named key forks that join the day either moves.
    public const string ChannelSlot = "rasm.materials.texture.channel";
    public const string VerdictSlot = "rasm.materials.texture.verdict";
    public const string StrategySlot = "rasm.materials.tile.strategy";
    public const string ComponentSlot = "rasm.materials.tile.component";
    // The mip POLICY the pyramid fold ran, because fold cost is a function of the policy and not of the channel
    // alone — a Kaiser fold and a box fold over one plane differ by an order of magnitude.
    public const string PolicySlot = "rasm.materials.texture.mip.policy";
    public const string ReasonSlot = "rasm.materials.ingest.reason";
    public const string ContainerSlot = "rasm.materials.plane.container";
    public const string DirectionSlot = "rasm.materials.plane.direction";
    public const string StageSlot = "rasm.materials.neural.stage";
    public const string ProviderSlot = "rasm.materials.neural.provider";
    public const string LicenceSlot = "rasm.materials.neural.licence";
    public const string FidelitySlot = "rasm.materials.neural.fidelity";
    public const string SkySlot = "rasm.materials.environment.sky";

    // Outcome values publish beside their slots because a partitioned counter's good half is read TWICE — the tap arm
    // stamps it and the [06] indicator names it as the partition's good set — so a value literal at either site forks
    // that share the moment the other moves. Each pair fans ONE counter; neither half earns a counter of its own.
    public const string Adequate = "adequate";
    public const string Inadequate = "inadequate";
    public const string FullRank = "full";
    public const string RankDeficient = "deficient";
    public const string Encode = "encode";
    public const string Decode = "decode";
    public const string Honoured = "honoured";
    public const string Degraded = "degraded";
    // Tiling verdicts are THREE outcomes, not two, because the gate publishes a grade whenever it measures one:
    // `accepted` cleared the proof's own AcceptBar, `rejected` was measured and fell short, and `unmeasured` is the
    // one absence the gate answers — its second signal could not be taken. Folding rejected into unmeasured is the
    // defect this vocabulary deletes: a plane graded at half the bar is evidence an operator acts on, while an
    // unmeasured plane is evidence nobody has yet, and a board that renders them alike reports a healthy estate
    // whose tilings were never graded. The synthesis population carries only the measured/unmeasured half, since a
    // synth receipt records what it produced and the acceptance bar rides the proof.
    public const string Accepted = "accepted";
    public const string Rejected = "rejected";
    public const string Measured = "measured";
    public const string Unmeasured = "unmeasured";
    public const string Claimed = "claimed";
    public const string Unresolved = "unresolved";

    public const string CatalogueAdmits = "rasm.materials.catalogue.admits";
    public const string CatalogueRows = "rasm.materials.catalogue.rows";
    public const string LibraryRows = "rasm.materials.library.rows";
    public const string SectionSolves = "rasm.materials.section.solves";
    public const string SectionDuration = "rasm.materials.section.duration";
    public const string CapacityChecks = "rasm.materials.capacity.checks";
    public const string CapacityUtilisation = "rasm.materials.capacity.utilisation";
    public const string GraphNodes = "rasm.materials.graph.nodes";
    public const string GraphDuration = "rasm.materials.graph.duration";
    public const string AcquisitionFits = "rasm.materials.acquisition.fits";
    public const string AcquisitionResidual = "rasm.materials.acquisition.residual";
    public const string WireMints = "rasm.materials.wire.mints";
    public const string ProjectionAdmits = "rasm.materials.projection.admits";
    public const string PressRuns = "rasm.materials.texture.presses";
    public const string PressTexels = "rasm.materials.texture.texels";
    public const string PressDuration = "rasm.materials.texture.press.duration";
    public const string PressDowngraded = "rasm.materials.texture.press.downgraded";
    public const string PressFaulted = "rasm.materials.texture.press.faulted";
    public const string PressGpuDelta = "rasm.materials.texture.press.gpu.delta";
    public const string PressAgeCoverage = "rasm.materials.texture.press.aging.coverage";
    public const string TileRuns = "rasm.materials.texture.tiles";
    public const string TileScore = "rasm.materials.texture.tile.score";
    public const string TileDuration = "rasm.materials.texture.tile.duration";
    public const string GradeRuns = "rasm.materials.texture.grades";
    public const string GradeScore = "rasm.materials.texture.grade.score";
    public const string PyramidLevels = "rasm.materials.texture.pyramid.levels";
    public const string PyramidDuration = "rasm.materials.texture.pyramid.duration";
    public const string IngestStems = "rasm.materials.texture.ingest.stems";
    public const string PlaneBytes = "rasm.materials.texture.plane.bytes";
    public const string CodecDuration = "rasm.materials.texture.codec.duration";
    public const string InferRuns = "rasm.materials.neural.infers";
    public const string InferPartitions = "rasm.materials.neural.partitions";
    public const string InferGolden = "rasm.materials.neural.golden";
    public const string PrefilterRuns = "rasm.materials.environment.prefilters";
    public const string PrefilterDuration = "rasm.materials.environment.prefilter.duration";
    public const string Faults = "rasm.materials.faults";

    // Share indicators partition ONE mounted population on the outcome dimension its own row declares, so every [06]
    // objective resolves a single Count row through Slo.Admit and a good-half twin has no roster seat to take.
    // Every WORK row leads on the kernel tenant slot: a section solve, a capacity check, a graph compile, a
    // capture fit, a wire mint, and a graph-delta admission are all per-project work a multi-tenant host runs
    // for one tenant at a time, and the root row's empty projection makes the dimension free in a single-tenant
    // process. The two POPULATION rows carry none for ONE reason: a frozen catalogue and its material library are
    // process-scoped reference data no tenant owns, so a tenant column there would declare a key no reader can
    // ever emit. The cadence half of that reasoning is retired — the kernel's optional level key and its Bind
    // registration both report a pulled reading under tags, so "a scalar level carries no call-site tag" no
    // longer distinguishes these rows from any other; only the ownership does.
    public static readonly Seq<InstrumentSpec> Rows = Seq(
        InstrumentSpec.Count(CatalogueAdmits, "{row}", "catalogue rows admitted through the freeze veto by family", MeasureForm.Whole, TenantContext.TenantSlot, FamilySlot),
        InstrumentSpec.Level(CatalogueRows, "{row}", "frozen catalogue row population", MeasureForm.Whole),
        InstrumentSpec.Level(LibraryRows, "{row}", "admitted material-library row population", MeasureForm.Whole),
        InstrumentSpec.Count(SectionSolves, "{solve}", "profile section solves by profile case", MeasureForm.Whole, TenantContext.TenantSlot, ProfileSlot),
        InstrumentSpec.Advised(SectionDuration, "s", "profile section solve wall duration", MeasureForm.Real, Buckets.SolveSeconds, TenantContext.TenantSlot, ProfileSlot),
        InstrumentSpec.Count(CapacityChecks, "{check}", "capacity checks by receipt kind, governing action, and adequacy verdict", MeasureForm.Whole, TenantContext.TenantSlot, KindSlot, GoverningSlot, AdequacySlot),
        InstrumentSpec.Advised(CapacityUtilisation, "1", "governing utilisation ratio per capacity check", MeasureForm.Real, Buckets.GoverningRatio, TenantContext.TenantSlot, KindSlot, GoverningSlot),
        // Compile cost reads against graph size, so the node census rides the same fact its duration does — a
        // duration alone cannot separate a slow compiler from a large graph.
        InstrumentSpec.Advised(GraphNodes, "{node}", "ordered node count per material graph compile", MeasureForm.Whole, Buckets.GraphCounts, TenantContext.TenantSlot),
        InstrumentSpec.Advised(GraphDuration, "s", "material graph compile wall duration", MeasureForm.Real, Buckets.CompileSeconds, TenantContext.TenantSlot),
        InstrumentSpec.Count(AcquisitionFits, "{fit}", "acquisition fits settled by capture method and parameter-rank verdict", MeasureForm.Whole, TenantContext.TenantSlot, MethodSlot, RankSlot),
        InstrumentSpec.Advised(AcquisitionResidual, "1", "acquisition fit RMS residual by capture method", MeasureForm.Real, Buckets.ResidualDecades, TenantContext.TenantSlot, MethodSlot),
        InstrumentSpec.Count(WireMints, "{wire}", "appearance wire mints by capture method", MeasureForm.Whole, TenantContext.TenantSlot, MethodSlot),
        InstrumentSpec.Count(ProjectionAdmits, "{delta}", "graph deltas the projection veto admitted", MeasureForm.Whole, TenantContext.TenantSlot),
        // Texture-plane throughput rides MONOTONE COUNTERS in UCUM units — `{texel}` shaded and `By` stored — rather than
        // histograms, because a bake's magnitude spans four orders between a 256 preview and a 16k production plane and a
        // bucket ladder over that range grades nothing; the rate a board reads is the counter's own derivative, and the
        // per-run DURATION beside it carries the distribution that does grade. Every texture row leads on the tenant
        // slot: a bake, an encode, an inference, and a prefilter are all per-project work a multi-tenant host runs for
        // one tenant at a time.
        InstrumentSpec.Count(PressRuns, "{press}", "texture presses settled by backend", MeasureForm.Whole, TenantContext.TenantSlot, BackendSlot),
        InstrumentSpec.Count(PressTexels, "{texel}", "texels shaded across every channel and mip level, by backend", MeasureForm.Whole, TenantContext.TenantSlot, BackendSlot),
        // DecodeSeconds (10 ms – 300 s) is the ladder a bake's own comment demands: a 256 preview lands sub-second
        // and a 16k production plane runs past the 60 s objective, so the 2 s-ceiling compile ladder and the
        // 250 ms-ceiling solve ladder both saturate their top bucket exactly where the distribution matters.
        InstrumentSpec.Advised(PressDuration, "s", "texture press wall duration by backend", MeasureForm.Real, Buckets.DecodeSeconds, TenantContext.TenantSlot, BackendSlot),
        // Channel keys the two quality decisions the press makes silently, so an operator reads which plane
        // degraded rather than that something did. Faulted counts TEXELS off the per-channel tally the receipt
        // carries, since a channel count grades a one-texel fault and a whole-plane fault as one event.
        // GpuDeltaMax is ABSENT for a single-lane press, so its histogram arm gates on presence and an
        // unconditional write publishes a perfect parity match no lane measured.
        InstrumentSpec.Count(PressDowngraded, "{channel}", "channels whose paired mip policy fell back to the box floor, by backend and channel", MeasureForm.Whole, TenantContext.TenantSlot, BackendSlot, ChannelSlot),
        InstrumentSpec.Count(PressFaulted, "{texel}", "texels neutral-filled by a failed band kernel, by backend and channel", MeasureForm.Whole, TenantContext.TenantSlot, BackendSlot, ChannelSlot),
        // The divergence ladder, never the residual decades: a CPU-versus-GPU delta is a RATIO that matters between
        // one percent and two, where a decade ladder spends eight of its ten buckets below 1e-3 grading noise and
        // collapses every actionable divergence into its top two.
        InstrumentSpec.Advised(PressGpuDelta, "1", "worst per-channel CPU-versus-GPU divergence on a two-lane press, by backend", MeasureForm.Real, Buckets.DivergenceRatio, TenantContext.TenantSlot, BackendSlot),
        // Aging coverage is ABSENT for every non-Aged program, so the arm gates on presence exactly as the gpu
        // delta does; the value is visited-over-declared per ladder axis, and an under-exercised dimension reads
        // as a share below one rather than as a silent re-press question.
        InstrumentSpec.Advised(PressAgeCoverage, "1", "ladder rungs visited over rungs declared, by backend and ladder axis", MeasureForm.Real, Buckets.Fractions, TenantContext.TenantSlot, BackendSlot, AxisSlot),
        // Tileability is TWO independent measurements against one verdict, so the signal fans on a COMPONENT
        // dimension rather than publishing the product alone: a seam ratio alone passes a blurred border and a
        // lattice leak alone passes a sharp-but-quiet seam, and an operator told only that a tiling failed cannot
        // read which half failed. The counter partitions every run; only a graded run reaches the histogram.
        InstrumentSpec.Count(TileRuns, "{tile}", "tiling runs settled by strategy, guide channel, and grade verdict", MeasureForm.Whole, TenantContext.TenantSlot, StrategySlot, ChannelSlot, VerdictSlot),
        // A tileability signal is a UNIT-INTERVAL score, so it takes the kernel's own fraction ladder: the residual
        // decades grade a quantity approaching zero, where every one of these values lives between 0 and 1 and the
        // whole population would pile into the top bucket. The kernel owns the ladder — a folder-local bound array
        // beside it is the forked-policy defect the one advice holder exists to prevent.
        InstrumentSpec.Advised(TileScore, "1", "tileability signal by strategy and score component", MeasureForm.Real, Buckets.Fractions, TenantContext.TenantSlot, StrategySlot, ComponentSlot),
        InstrumentSpec.Advised(TileDuration, "s", "tiling synthesis wall duration by strategy", MeasureForm.Real, Buckets.DecodeSeconds, TenantContext.TenantSlot, StrategySlot),
        // Grading is measured SEPARATELY from synthesis because the two populations differ: every synthesized
        // plane is graded, and so is every ingested set nothing synthesized, so a grade counter folded into the
        // synthesis one loses exactly the third-party population whose tileability nothing else reports. The
        // combined value alone reaches the histogram — the component fan already rides TileScore for the
        // synthesized half, and a grade with no proof carries no value to bucket.
        InstrumentSpec.Count(GradeRuns, "{grade}", "tileability grades settled by strategy and proof verdict", MeasureForm.Whole, TenantContext.TenantSlot, StrategySlot, VerdictSlot),
        InstrumentSpec.Advised(GradeScore, "1", "combined tileability verdict of every minted proof by strategy", MeasureForm.Real, Buckets.Fractions, TenantContext.TenantSlot, StrategySlot),
        // The pyramid fold is per-CHANNEL work every press, ingest, and decode pays, and its cost is a function
        // of the mip POLICY: a Kaiser fold and a box fold over one plane differ by an order of magnitude, so the
        // policy is the dimension that makes the duration actionable. Levels ride a counter rather than a
        // histogram — a level count is a small bounded integer whose sum over a run is the fold volume.
        InstrumentSpec.Count(PyramidLevels, "{level}", "pyramid levels folded by channel and mip policy", MeasureForm.Whole, TenantContext.TenantSlot, ChannelSlot, PolicySlot),
        InstrumentSpec.Advised(PyramidDuration, "s", "pyramid fold wall duration by channel and mip policy", MeasureForm.Real, Buckets.DecodeSeconds, TenantContext.TenantSlot, ChannelSlot, PolicySlot),
        // Refusal reasons cross as BOUNDED rows rather than the formatted token the manifest once carried, so one
        // series answers both operator questions an asset-library ingest raises — how much of a vendor set this
        // estate's alias table claims, and why the remainder did not classify.
        InstrumentSpec.Count(IngestStems, "{stem}", "ingested plane stems by classification verdict and refusal reason", MeasureForm.Whole, TenantContext.TenantSlot, VerdictSlot, ReasonSlot),
        InstrumentSpec.Count(PlaneBytes, "By", "plane bytes encoded or decoded by container and direction", MeasureForm.Whole, TenantContext.TenantSlot, ContainerSlot, DirectionSlot),
        InstrumentSpec.Advised(CodecDuration, "s", "plane codec wall duration by container and direction", MeasureForm.Real, Buckets.DecodeSeconds, TenantContext.TenantSlot, ContainerSlot, DirectionSlot),
        // Licence rides the inference POPULATION because a fleet operator's first question about a model estate is
        // which grant class its running inferences fall under — a research-class row appearing in production is a
        // posture change no duration or residual would ever surface.
        InstrumentSpec.Count(InferRuns, "{inference}", "photo-to-PBR inferences settled by stage, provider, licence class, and provider fidelity", MeasureForm.Whole, TenantContext.TenantSlot, StageSlot, ProviderSlot, LicenceSlot, FidelitySlot),
        InstrumentSpec.Advised(InferPartitions, "{partition}", "ONNX graph partitions reached per inference by stage and provider", MeasureForm.Whole, Buckets.GraphCounts, TenantContext.TenantSlot, StageSlot, ProviderSlot),
        InstrumentSpec.Advised(InferGolden, "1", "inference residual against the model's CPU-reference output by stage and provider", MeasureForm.Real, Buckets.ResidualDecades, TenantContext.TenantSlot, StageSlot, ProviderSlot),
        InstrumentSpec.Count(PrefilterRuns, "{prefilter}", "IBL prefilters settled by sky model", MeasureForm.Whole, TenantContext.TenantSlot, SkySlot),
        InstrumentSpec.Advised(PrefilterDuration, "s", "IBL prefilter wall duration by sky model", MeasureForm.Real, Buckets.DecodeSeconds, TenantContext.TenantSlot, SkySlot),
        InstrumentSpec.Count(Faults, "{fault}", "veto refusals and isolated tap faults by category", MeasureForm.Whole, TenantContext.TenantSlot, KernelInstruments.CategorySlot));

    // Rows and the `[06]` pack over them leave as ONE downward fact, so the mounting root proves the pack in
    // one fold binding these handles. Forward reach stays safe by construction: the pack reads consts, which
    // trigger no static construction, while this factory is a method the pack's own init never calls.
    public static TelemetryContributorPort Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl) =>
        new(Scope: TelemetrySource.Materials.Key, Version: version, Instruments: Rows, SchemaUrl: schemaUrl,
            Board: MaterialsDescriptors.Pack);

    // First double above the long range, derived from the exactly-representable minimum rather than a decimal
    // literal a binary double rounds: (double)long.MinValue is -2^63 exact, so its negation is the exclusive
    // ceiling and the admitted band carries the long domain's own asymmetry.
    const double WholeCeiling = -(double)long.MinValue;

    // Pulled populations arrive as composition-supplied readers: only the composing app holds the frozen catalogue
    // and its material library, so bound readers are the levels and a cell nobody writes is the deleted form. The
    // roster's own Pulled column is the completeness proof in every direction — a stray name, a starved gauge, and
    // a name supplied twice all refuse here rather than reporting a silent zero for the process lifetime or leaving
    // one cell holding whichever of two readers bound last, a shadowing the roster census cannot see.
    public static Fin<Seq<IDisposable>> Tap(
        MaterialsHooks hooks, InstrumentSet set, params ReadOnlySpan<(string Name, Func<double> Read)> levels) =>
        toSeq(levels.ToArray())
            .Fold(Fin.Succ(Seq<string>()), (state, row) => state.Bind(bound => bound.Exists(name => name == row.Name)
                ? Fin.Fail<Seq<string>>(new Fault.InvalidValue(Label: row.Name, Requirement: "one reader per pulled roster row"))
                : Bound(set, row).Map(_ => bound.Add(row.Name))))
            .Bind(bound => Rows.Filter(static row => row.Kind.Pulled).ForAll(row => bound.Exists(name => name == row.Name))
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new Fault.InvalidValue(
                    Label: TelemetrySource.Materials.Key,
                    Requirement: "a supplied reader for every pulled roster row")))
            .Map(_ => Mounted(hooks, set));

    // Measurement type is the row's own declaration, so one supply shape closes both forms and each reader lands
    // in the cell keyed by the type its own gauge reads back. The whole-number form crosses the domain gate rather
    // than a bare cast: an unchecked (long) of a non-finite or out-of-range double has no defined image, so a
    // starved reader would publish long.MinValue as a row population indistinguishable from a real one. The supply
    // admits its own current reading here, and the bound cell re-admits every later one against the same gate,
    // holding the admitted reading where a drifted one has no whole-number image.
    static Fin<Unit> Bound(InstrumentSet set, (string Name, Func<double> Read) row) =>
        set.Rows.TryGetValue(row.Name, out InstrumentSpec? declared) && declared.Kind.Pulled
            ? declared.Form.Switch(
                state: (Set: set, Row: row),
                whole: static bind => Whole(bind.Row.Name, bind.Row.Read())
                    .Bind(seed => bind.Set.Bind(bind.Row.Name, () => Whole(bind.Row.Name, bind.Row.Read()).IfFail(seed))),
                real: static bind => bind.Set.Bind(bind.Row.Name, bind.Row.Read))
            : Fin.Fail<Unit>(new Fault.InvalidValue(Label: row.Name, Requirement: "a mounted pulled roster row"));

    // Whole-number admission over the double domain as ONE gate: double.IsInteger already excludes NaN and both
    // infinities, and the band closes the magnitudes the conversion cannot name.
    static Fin<long> Whole(string name, double reading) =>
        double.IsInteger(reading) && reading >= long.MinValue && reading < WholeCeiling
            ? Fin.Succ((long)reading)
            : Fin.Fail<long>(new Fault.InvalidValue(Label: name, Requirement: "a whole-number reading inside the long range"));

    // Every arm returns the kernel write rail, so the capsule's own rail-shaped Observe lifts a refusal onto the IO
    // error channel and its shield parks it as IsolatedFault — a folder-local lift aspect has nothing left to add.
    // Every tag set materializes through the kernel `InstrumentSet.Tags` entry, which appends the ambient
    // tenancy partition — empty on the root row, so a single-tenant composition mints no `rasm.tenant` entry
    // and a partitioned one stamps it on every work row uniformly. A hand-spelled `KeyValuePair` array beside
    // it re-mints the one materialization the capsule owns and drops the partition on the arm that forgets it.
    // Tileability components fan ONE histogram off one row table rather than three named writes: the score's own
    // column set is the dimension, so a fourth signal lands as a row here and no arm grows an arm.
    static readonly Seq<(Func<TileScore, double> Read, string Value)> ScoreComponents =
        Seq<(Func<TileScore, double>, string)>(
            (static score => score.SeamRatio, "seam"),
            (static score => score.LatticeLeak, "lattice"),
            (static score => score.Value, "value"));

    static Seq<IDisposable> Mounted(MaterialsHooks hooks, InstrumentSet set) {
        // Re-parking a refused fault-count write on the rail cell would re-enter this handler, so the refusal is
        // discarded here and nowhere else on the page.
        AtomChangedEvent<Seq<IsolatedFault>> rejected = held => held.Last.Iter(fault =>
            ignore(set.Enabled(Faults)
                ? set.Write(Faults, 1L,
                    InstrumentSet.Tags(TenantContext.Current, (KernelInstruments.CategorySlot, fault.Cause.Category)))
                : Fin.Succ(unit)));
        hooks.Faults.Change += rejected;
        return Seq<IDisposable>(
            hooks.CatalogueAdmit.Observe(fact =>
                set.Write(CatalogueAdmits, 1L, InstrumentSet.Tags(TenantContext.Current, (FamilySlot, fact.Row.Item.Family.Key)))),
            hooks.SectionSolve.Observe(fact => Paired(set, SectionSolves, 1L, SectionDuration, fact.Elapsed,
                InstrumentSet.Tags(TenantContext.Current, (ProfileSlot, fact.Profile)))),
            hooks.CapacityCheck.Observe(fact => {
                // Scope tags key both rows; adequacy rides the population alone, because a bounded ratio already
                // carries the verdict a second dimension on the histogram would only restate. The verdict tag is a
                // COPY-THEN-ADD off the shared scope — TagList is a struct, so the extended set costs no heap and
                // the scope the ratio writes under stays exactly the two dimensions its row declares.
                TagList scope = InstrumentSet.Tags(TenantContext.Current,
                    (KindSlot, fact.Receipt.Kind), (GoverningSlot, fact.Verdict.Governing.Key));
                TagList verdict = scope;
                verdict.Add(AdequacySlot, fact.Verdict.Adequate ? Adequate : Inadequate);
                return set.Write(CapacityChecks, 1L, verdict)
                    // Unbounded carries no bounded ratio, so the verdict counts and records nothing; the capacity
                    // owner projects which cases hold one, so this arm never re-enumerates its case set.
                    .Bind(_ => fact.Verdict.Ratio.Match(
                        Some: value => set.Write(CapacityUtilisation, value, scope),
                        None: static () => Fin.Succ(unit)));
            }),
            hooks.GraphCompile.Observe(fact => Paired(set, GraphNodes, fact.Nodes, GraphDuration, fact.Elapsed,
                InstrumentSet.Tags(TenantContext.Current))),
            hooks.AcquisitionFit.Observe(fact => {
                TagList method = InstrumentSet.Tags(TenantContext.Current, (MethodSlot, fact.Receipt.Method.Key));
                // Rank deficiency reads as a non-finite condition number, the Svd contract the receipt carries, so the
                // fit population stamps its own rank verdict and the [06] full-rank share partitions that one series.
                TagList ranked = method;
                ranked.Add(RankSlot, double.IsFinite(fact.Receipt.FitConditionNumber) ? FullRank : RankDeficient);
                return set.Write(AcquisitionFits, 1L, ranked)
                    .Bind(_ => set.Write(AcquisitionResidual, fact.Receipt.FitResidual, method));
            }),
            hooks.WireMint.Observe(fact =>
                set.Write(WireMints, 1L, InstrumentSet.Tags(TenantContext.Current, (MethodSlot, fact.Receipt.Method)))),
            hooks.ProjectionGate.Observe(_ => set.Write(ProjectionAdmits, 1L, InstrumentSet.Tags(TenantContext.Current))),
            // Backend keys every press row: a CPU-minted set and a GPU preview differ in what their bytes MEAN
            // (only the CPU lane is content-authoritative), so folding their throughput onto one series would grade
            // an accelerator's speed as if it were the estate's own bake rate.
            hooks.TexturePress.Observe(fact => {
                // The press arm is the page's heaviest: eight writes plus two receipt-collection walks per bake.
                // One listened row admits the whole fold, so the gate skips the walks outright where nothing is
                // subscribed and never absorbs the mount refusal a write owes on a rostered name.
                if (!set.Enabled(PressRuns, PressTexels, PressDuration, PressDowngraded,
                        PressFaulted, PressGpuDelta, PressAgeCoverage)) { return Fin.Succ(unit); }
                TagList backend = InstrumentSet.Tags(TenantContext.Current, (BackendSlot, fact.Receipt.Backend.Key));
                return set.Write(PressRuns, 1L, backend)
                    // Saturation rides the UNSIGNED minimum: `Math.Min` over a ulong and a long binds the double
                    // overload, so a census past 2^53 rounds before it is ever counted.
                    .Bind(_ => set.Write(PressTexels, (long)ulong.Min(fact.Receipt.Texels, (ulong)long.MaxValue), backend))
                    .Bind(_ => set.Write(PressDuration, fact.Receipt.ElapsedMs / 1000.0, backend))
                    .Bind(_ => fact.Receipt.Downgraded.TraverseM(channel =>
                        set.Write(PressDowngraded, 1L, Keyed(backend, ChannelSlot, channel.Key))).As())
                    .Bind(_ => fact.Receipt.Faulted.AsIterable().TraverseM(row =>
                        set.Write(PressFaulted, (long)ulong.Min(row.Value, (ulong)long.MaxValue),
                            Keyed(backend, ChannelSlot, row.Key.Key))).As())
                    // GpuDeltaMax is a TYPED ABSENCE on a single-lane press, never a zero the parity gate would
                    // read as a perfect match, so presence alone admits the write.
                    .Bind(_ => fact.Receipt.GpuDeltaMax.Match(
                        Some: delta => set.Write(PressGpuDelta, delta, backend),
                        None: static () => Fin.Succ(unit)))
                    // Aging rides the same typed-absence gate: only the Aged program mints a coverage census, and
                    // each axis writes its own share so an unexercised cavity dimension is attributable by key.
                    .Bind(_ => fact.Receipt.Aging.Match(
                        Some: coverage => set.Write(PressAgeCoverage, coverage.AgeRungsVisited / (double)coverage.AgeRungs,
                                Keyed(backend, AxisSlot, "age"))
                            .Bind(_ => set.Write(PressAgeCoverage, coverage.CavityRungsVisited / (double)coverage.CavityRungs,
                                Keyed(backend, AxisSlot, "cavity"))),
                        None: static () => Fin.Succ(unit)));
            }),
            hooks.TileSynth.Observe(fact => {
                TagList plan = InstrumentSet.Tags(TenantContext.Current,
                    (StrategySlot, fact.Strategy.Key), (ChannelSlot, fact.Guide.Key));
                // Verdict recovers STRUCTURALLY off the receipt's own OPTION: the gate answers absence only where a
                // signal could not be measured, so presence is the whole discriminant and the acceptance threshold
                // never lands here as a literal a caller's `TilePolicy` re-tuning would fork. Every run COUNTS and
                // only a measured run reaches the histogram — an unmeasured run has no value to bucket, and a zero
                // in its place reads to a board as the worst tiling in the estate rather than as no measurement.
                // Duration measures on both halves, so it writes on both.
                return set.Write(TileRuns, 1L,
                        Keyed(plan, VerdictSlot, fact.Receipt.Score.IsSome ? Measured : Unmeasured))
                    .Bind(_ => set.Write(TileDuration, fact.Receipt.ElapsedMs / 1000.0,
                        InstrumentSet.Tags(TenantContext.Current, (StrategySlot, fact.Strategy.Key))))
                    .Bind(_ => fact.Receipt.Score.Match(
                        Some: score => ScoreComponents.TraverseM(row => set.Write(TileScore, row.Read(score),
                            Keyed(plan, ComponentSlot, row.Value))).As().Map(static _ => unit),
                        None: static () => Fin.Succ(unit)));
            }),
            // Every grade COUNTS under its three-way verdict and every MINTED proof measures. A proof carries the
            // score it earned together with the bar it was graded against, so `Accepted` is the proof's own
            // predicate over that pair and never a threshold re-spelled here. The histogram takes EVERY proof,
            // accepted or not: the population a quality board reads is how far the estate's tilings land from the
            // bar, which a roster admitting only the passing half cannot show. Absence alone withholds a value,
            // because absence is the one state where nothing was measured.
            hooks.TileGrade.Observe(fact => {
                TagList plan = InstrumentSet.Tags(TenantContext.Current, (StrategySlot, fact.Strategy.Key));
                return set.Write(GradeRuns, 1L, Keyed(plan, VerdictSlot, fact.Proof.Match(
                        Some: static proof => proof.Accepted ? Accepted : Rejected,
                        None: static () => Unmeasured)))
                    .Bind(_ => fact.Proof.Match(
                        Some: proof => set.Write(GradeScore, proof.Score.Value, plan),
                        None: static () => Fin.Succ(unit)));
            }),
            hooks.PyramidBuild.Observe(fact => Paired(set, PyramidLevels, fact.Levels, PyramidDuration, fact.Elapsed,
                InstrumentSet.Tags(TenantContext.Current, (ChannelSlot, fact.Channel.Key), (PolicySlot, fact.Policy.Key)))),
            hooks.SetIngest.Observe(fact => {
                // The refusal tally folds every unresolved stem before its first write, so the gate goes ahead of
                // the fold rather than ahead of the write alone — a vendor library dropping four hundred stems
                // costs one boolean in a process subscribed to nothing.
                if (!set.Enabled(IngestStems)) { return Fin.Succ(unit); }
                TagList partition = InstrumentSet.Tags(TenantContext.Current);
                // Claimed stems carry the EMPTY reason the SkySlot precedent already established, since a
                // synthesized "none" value mints a second vocabulary this arm keeps aligned with the refusal
                // roster forever. Refusals fold to ONE write per reason carrying its own tally, so a vendor
                // library dropping four hundred stems for one cause is four hundred on one series point.
                TagList claimed = partition;
                claimed.Add(VerdictSlot, Claimed);
                claimed.Add(ReasonSlot, string.Empty);
                return set.Write(IngestStems, fact.Claimed, claimed)
                    .Bind(_ => fact.Unresolved
                        .Fold(HashMap<IngestRefusal, long>.Empty, static (tally, row) =>
                            tally.AddOrUpdate(row.Reason, static held => held + 1L, 1L))
                        .AsIterable()
                        .TraverseM(row => set.Write(IngestStems, row.Value,
                            Keyed(Keyed(partition, VerdictSlot, Unresolved), ReasonSlot, row.Key.Key))).As()
                        .Map(static _ => unit));
            }),
            hooks.PlaneCodec.Observe(fact => Paired(set, PlaneBytes, fact.Bytes, CodecDuration, fact.Elapsed,
                InstrumentSet.Tags(TenantContext.Current,
                    (ContainerSlot, fact.Format.Key), (DirectionSlot, fact.Encoded ? Encode : Decode)))),
            hooks.StageInfer.Observe(fact => {
                // ProviderUsed, never the requested provider: the executor may refuse a policy row and degrade, and
                // a series keyed on the ASK would attribute a CPU fallback's latency to the accelerator.
                TagList lane = InstrumentSet.Tags(TenantContext.Current,
                    (StageSlot, fact.Result.Stage.Key), (ProviderSlot, fact.Result.ProviderUsed.Key));
                // Fidelity partitions the ONE population on whether the executor honoured the requested provider:
                // a CoreML request that degraded to CPU is correct and slow, and only this dimension distinguishes
                // a healthy accelerator estate from one silently running every inference on the guaranteed floor.
                TagList admitted = lane;
                admitted.Add(LicenceSlot, fact.Request.LicenseClass.Key);
                admitted.Add(FidelitySlot, fact.Result.ProviderUsed == fact.Request.Provider ? Honoured : Degraded);
                return set.Write(InferRuns, 1L, admitted)
                    .Bind(_ => set.Write(InferPartitions, (long)fact.Result.PartitionCount, lane))
                    // ParityFresh gates the histogram write: the golden delta is memoized per triple while the tap
                    // fires per inference, so only the run that TOOK the measurement writes it — N-observations-
                    // where-one-was-taken is the forged-measure form this gate deletes.
                    .Bind(_ => fact.Result.ParityFresh ? set.Write(InferGolden, fact.Result.GoldenDelta, lane) : Fin.Succ(unit));
            }),
            // INGESTED domes carry no sky model, so the series keys on the empty string the environment row itself
            // publishes rather than on a synthesized "none" this arm keeps aligned.
            hooks.EnvironmentPrefilter.Observe(fact => Paired(set, PrefilterRuns, 1L, PrefilterDuration, fact.Elapsed,
                InstrumentSet.Tags(TenantContext.Current, (SkySlot, fact.SkyModel)))),
            new HookDetacher(() => hooks.Faults.Change -= rejected));
    }

    // ONE widening over the kernel's stack-allocated tag projection: TagList is a struct, so the parameter is
    // already the caller's copy and the extension never reaches the shared base set. This exists because a
    // per-row widening inside a Traverse would otherwise re-spell the copy-then-Add at every call site, and
    // materializing a heap `KeyValuePair<string, object?>[]` beside the kernel's own `in TagList` write overload
    // re-mints the one allocation the projection exists to avoid.
    static TagList Keyed(TagList tags, string slot, object? value) {
        tags.Add(slot, value);
        return tags;
    }

    // FIVE arms take exactly one shape — a tag set, a whole-number census, and that run's wall duration under the
    // SAME tags — so the pair folds through one entry instead of re-spelling the tag local and the Bind at each.
    // The two writes share a tag set by construction here, which is the property that kept drifting when each arm
    // spelled it: an arm widening its census tags and forgetting the duration published two series a board could
    // no longer join. TagList is a struct, so the parameter is already this fold's own copy.
    static Fin<Unit> Paired(
        InstrumentSet set, string census, long count, string duration, Duration elapsed, TagList tags) =>
        set.Write(census, count, tags).Bind(_ => set.Write(duration, elapsed.TotalSeconds, tags));
}
```

## [05]-[EVIDENCE_RECORDS]

- Owner: `MaterialsLog` — the fixed-severity generated emission grammar over the folder's banded faults and the rail's isolated evidence; `LatencyPhase` — the bracketed-construction vocabulary, each row owning BOTH of its checkpoint names; `LatencyMeasure` — the accumulated-quantity vocabulary, each row owning its accumulation law; `LatencyWrite` — the two accumulation laws those rows carry; `PhaseBracket`/`MeasureSlot` — the resolved token carriers the ledger entries accept; `MaterialsLatency` — the folder's contributed three-axis name roster and the entries that write under it.
- Cases: `LatencyWrite.Accrue` sums a quantity one request reaches many times — texels across every channel of one press, bytes across every encoded plane; `LatencyWrite.Pin` states a quantity measured once — the landed plane census a fold reports at its close. `LatencyPhase` rows are the folder's three bracketed constructions (catalogue build, interaction solve, texture press) and `LatencyMeasure` rows its three quantities, each binding its own law.
- Entry: `MaterialsLog.Logged` rides `MapFail` on any Materials rail so a refusal logs once at the seam that produced it; `MaterialsLog.Drain(ILogger, Seq<IsolatedFault>)` projects a snapshot of the rail's parked evidence; `LatencyPhase.Resolve(ILatencyContextTokenIssuer)` and `LatencyMeasure.Resolve(ILatencyContextTokenIssuer)` are the ONE token-resolution pair a composition runs at boot; `MaterialsLatency.Measured(ILatencyContext, in PhaseBracket, Func<Fin<T>>)` brackets one eager construction; `MaterialsLatency.Measure(ILatencyContext, in MeasureSlot, long)` folds one quantity under the slot's own law; `MaterialsLatency.Attributed(ILatencyContext, TagToken, string)` stamps one pivot last-write-wins; `MaterialsLatency.Sealed(ILatencyContext)` freezes the ledger and hands back its `LatencyData` for the composition's exporter.
- Auto: severity is declaration data on the attribute, so no call site chooses a level and no runtime switch over named severity verbs exists; `EventId` allocates from this owner's declared band and `EventName` is the dashboard-stable half; the generated `IsEnabled` gate precedes payload construction; `Checkpoints` and `Measures` DERIVE from the two row families rather than restating them, so a phase or quantity added to a vocabulary reaches the contributed roster by construction and a hand-listed roster cannot drift behind the names its own folds stamp.
- Packages: Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new bracketed construction is one `LatencyPhase` row and its two names derive from the stem; a new accumulated quantity is one `LatencyMeasure` row naming its law; a new accumulation law is one `LatencyWrite` row; a new pivot is one entry in `Tags`; a new event family is one partial in this band, never a per-fault-family verb — no roster edit accompanies any of them.
- Boundary: this folder CONTRIBUTES a latency vocabulary and never registers one — `Checkpoints`, `Measures`, and `Tags` leave as one contributed roster the app root's single `LatencySpine.Register` fold folds beside its own and every peer contributor's, so no package reaches `RegisterCheckpointNames` and splits the table. That registration arms `LatencyContextOptions.ThrowOnUnregisteredNames`, which makes an unregistered name a BOOT FAILURE rather than a positionless token whose writes drop unseen: a name this folder stamps and never contributed refuses the composition, so the derivation of both rosters from their row families is the structural half of that guarantee and a hand-listed roster is the failure mode it forecloses. Libraries take the logger and the latency ledger by injection and a logger-less composition binds `NullLogger.Instance`, never a nullable handle; the `Code` and `Category` holes read the `Rasm.Element` band registry and the kernel `Error` projection, so a reader groups records by the same band a fault counter groups its series by and no fault text is re-formatted here; the instrument counts at `[04]` and the records here are disjoint mandates over one refusal, never two shapes of one record; settlement records in `finally`, so failed or throwing constructions close the same bracket as successful constructions. Pivots COMPOSE the `[04]` slot consts rather than re-spelling them, so a ledger pivot and a metric dimension naming one axis are one string and a join from a slow request to its own series holds; `Freeze` seals, so it runs at the composition edge after the last write and never inside a bracket a retry re-enters, where a frozen context refuses every later checkpoint, measure, and tag in silence. Duration NEVER derives from a stamp difference — the checkpoint pair is the ledger's own elapsed, and the causal stamp orders events without measuring them.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using Microsoft.Extensions.Diagnostics.Latency; // CheckpointToken, ILatencyContext, ILatencyContextTokenIssuer,
                                                // LatencyData, MeasureToken, TagToken
using LanguageExt.Common;                       // Error — the typed rail fault the property walk projects
using Microsoft.Extensions.Logging;             // ILogger, LogLevel, LoggerMessageAttribute, LogPropertiesAttribute, TagNameAttribute
using Rasm.Domain;                              // HookId, IsolatedFault, Op, TenantContext, FaultExtensions extension property Category
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [TYPES] ----------------------------------------------------------------------------------
// Rows carry the ledger's two accumulation laws, each holding its own write rather than a call-site branch:
// AddMeasure sums across a request and RecordMeasure states one value absolutely, and choosing between them with
// a bool at every fold re-derives what the row already encodes. Both members return void, so each row's body
// takes the named statement seam this file's generated-emission projection already carries.
[SmartEnum]
public sealed partial class LatencyWrite {
    public static readonly LatencyWrite Accrue = new(static (ledger, token, value) => { ledger.AddMeasure(token, value); return unit; });
    public static readonly LatencyWrite Pin = new(static (ledger, token, value) => { ledger.RecordMeasure(token, value); return unit; });

    [UseDelegateFromConstructor]
    public partial Unit Apply(ILatencyContext ledger, MeasureToken token, long value);
}

// Each phase row owns BOTH of its checkpoint names, derived from the stem so the pair cannot drift apart. Two
// loose name constants let a bracket open on the press and settle on the catalogue build with nothing raised,
// publishing an elapsed that spans two unrelated constructions and still reads measured.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LatencyPhase {
    public static readonly LatencyPhase Catalogue = Bracketed("rasm.materials.catalogue.build");
    public static readonly LatencyPhase Interaction = Bracketed("rasm.materials.interaction.solve");
    public static readonly LatencyPhase Press = Bracketed("rasm.materials.texture.press");

    public string Started { get; }
    public string Settled { get; }

    private static LatencyPhase Bracketed(string stem) => new(stem, $"{stem}.started", $"{stem}.settled");

    public static Seq<string> Names => toSeq(Items).Bind(static row => Seq(row.Started, row.Settled));

    // Tokens resolve ONCE per composition and the row hands back BOTH at once, so the bracket a call site holds is
    // a pair the vocabulary minted rather than two arguments a call site chose. A resolution per invocation is the
    // deleted form — the issuer's lookup is a dictionary read the ledger exists to avoid on a measured path.
    public PhaseBracket Resolve(ILatencyContextTokenIssuer issuer) =>
        new(issuer.GetCheckpointToken(Started), issuer.GetCheckpointToken(Settled));
}

// Each measure row owns its accumulation LAW, because the law belongs to the quantity and never to the call:
// texels and encoded bytes accrue across every channel of one request, while the landed plane census states once.
// Passing the law beside the token lets one fold Pin an accruing quantity and publish one channel's tally as the
// whole request's.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LatencyMeasure {
    public static readonly LatencyMeasure TexelsShaded = new("rasm.materials.texture.texels.shaded", LatencyWrite.Accrue);
    public static readonly LatencyMeasure PlaneBytesEncoded = new("rasm.materials.texture.plane.bytes.encoded", LatencyWrite.Accrue);
    public static readonly LatencyMeasure PlanesLanded = new("rasm.materials.texture.planes.landed", LatencyWrite.Pin);

    public LatencyWrite Write { get; }

    public MeasureSlot Resolve(ILatencyContextTokenIssuer issuer) => new(Write, issuer.GetMeasureToken(Key));
}

// --- [MODELS] --------------------------------------------------------------------------------
// Both resolved carriers spell what the ledger entries accept: a bracket reaches a fold as the token PAIR one
// phase row issued and a quantity reaches it beside the law its own row declares. Compositions resolve each once
// at boot and hold the value; a fold assembling either from loose tokens is the deleted form.
public readonly record struct PhaseBracket(CheckpointToken Started, CheckpointToken Settled);

public readonly record struct MeasureSlot(LatencyWrite Write, MeasureToken Token);

// --- [SERVICES] -----------------------------------------------------------------------------
public static partial class MaterialsLog {
    // This owner allocates its event ids from Band; fault-band integers stay the Rasm.Element FaultBand registry's.
    private const int Band = 6400;

    // The fault crosses TYPED under [LogProperties]: the generator walks its public properties into structured
    // tags, so the band, the discriminant, and every column the concrete case carries reach the sink as separate
    // fields a query filters on — where three pre-flattened strings reach it as three opaque values and the
    // fourth column a new case adds reaches it not at all. [TagName] PINS each identifier's tag name, because the
    // parameter name is the generator's default and a rename would silently re-key every board filtering on it.
    // Category rides its own parameter because it is an extension property on the fault rail rather than a
    // declared one, so the property walk cannot see it.
    [LoggerMessage(EventId = Band, EventName = "MaterialsRefused", Level = LogLevel.Warning,
        Message = "materials {materials.op} refused")]
    public static partial void Refused(ILogger logger, [TagName("materials.op")] Op op, string category, [LogProperties] Error fault);

    [LoggerMessage(EventId = Band + 1, EventName = "MaterialsIsolated", Level = LogLevel.Warning,
        Message = "materials tap {materials.point} isolated")]
    public static partial void Isolated(ILogger logger, [TagName("materials.point")] HookId point, string category, [LogProperties] Error cause);

    extension<T>(Fin<T> step) {
        // MapFail keeps the rail intact, so the record is evidence beside the refusal rather than a second exit; a
        // generated emission returns void by the attribute's own contract, so the projection is a statement lambda
        // and the tuple-sequenced expression every Unit-returning sibling uses has no spelling here.
        public Fin<T> Logged(ILogger logger, Op key) =>
            step.MapFail(error => {
                Refused(logger, key, error.Category, error);
                return error;
            });
    }

    public static Unit Drain(ILogger logger, Seq<IsolatedFault> held) =>
        ignore(held.Iter(fault => Isolated(logger, fault.Point, fault.Cause.Category, fault.Cause)));
}

public static class MaterialsLatency {
    // Both rosters DERIVE from the two row families and the [04] slot consts, so a phase or measure
    // added to a roster reaches the one registration by construction. Under the app root's boot-strict fold a name
    // this folder stamps and never contributed is a composition FAILURE rather than a positionless token whose
    // writes drop, so a hand-listed roster drifting behind its own vocabulary is the shape this derivation deletes.
    public static readonly Seq<string> Checkpoints = LatencyPhase.Names;

    public static readonly Seq<string> Measures = toSeq(LatencyMeasure.Items).Map(static row => row.Key);

    // Pivots COMPOSE the [04] slot consts: a ledger tag and a metric dimension naming one axis under two strings
    // strand every join from a slow request to the series that explains it.
    public static readonly Seq<string> Tags = Seq(
        TenantContext.TenantSlot, MaterialsInstruments.BackendSlot, MaterialsInstruments.ChannelSlot);

    public static Fin<T> Measured<T>(ILatencyContext ledger, in PhaseBracket phase, Func<Fin<T>> body) {
        ledger.AddCheckpoint(phase.Started);
        try { return body(); }
        finally { ledger.AddCheckpoint(phase.Settled); }
    }

    public static Unit Measure(ILatencyContext ledger, in MeasureSlot slot, long value) =>
        slot.Write.Apply(ledger, slot.Token, value);

    // Last write wins per pivot, so a degraded press stamps the backend it FELL BACK to and the ledger keeps
    // whichever lane produced the latency rather than whichever lane the caller asked for.
    public static Unit Attributed(ILatencyContext ledger, TagToken token, string value) {
        ledger.SetTag(token, value);
        return unit;
    }

    // Seals at the composition edge: a frozen ledger refuses every later checkpoint, measure, and tag silently,
    // so freezing inside a retried bracket loses the attempt the retry exists to record.
    public static LatencyData Sealed(ILatencyContext ledger) {
        ledger.Freeze();
        return ledger.LatencyData;
    }
}
```

## [06]-[BOARD_PACK]

- Owner: `MaterialsDescriptors` — the folder's one kernel `BoardPack` value binding the panel rows and reliability objectives over the `[04]` roster.
- Entry: `MaterialsDescriptors.Pack` is the whole descriptor surface the IaC compile leg decodes under `materials.catalogue`, the provenance key the pack carries as its own first column — `Wire`, `Panels`, and `Objectives` are its columns, `Alerts` derives one `AlertSpec` per objective per burn row through the kernel fold, and `Pack.Admit(roster)` proves every panel instrument, every break key, every widget resolution, and every indicator series against the declaring port's own roster before a board compiles; the pack rides `[04]`'s contributor port outward, so the mounting root runs that proof and this folder exposes no second admission entry.
- Auto: a panel naming an instrument alone reads the kernel widget projection for that row's measurement shape, so only a deliberate reading spells a `PanelKind`; every descriptor names an instrument on the `[04]` roster and every break key names one of that row's declared dimensions, so a renamed instrument or a dropped dimension refuses at composition rather than rendering an empty panel; burn windows, factors, severities, and the budget share derive from the kernel table, and every objective omits its compliance window so kernel admission canonicalizes the one estate default — no threshold and no calendar literal lands here.
- Packages: Rasm, LanguageExt.Core, NodaTime.
- Growth: a new board panel is one `PanelSpec` on the pack; a new reliability policy is one `Objective` row over an existing indicator shape, and a share over an already-fanned population needs no roster edit at all; a new indicator shape is a kernel `Sli` case breaking every compile leg at once.
- Boundary: dashboards, alert provisioning, tenancy, query dialects, the panel descriptor row, and the burn algebra are the kernel's and the IaC plane's — this page carries pack DATA behind the same `rasm.materials.*` names the instruments carry and never a descriptor type, query string, board JSON, or provider type; an objective binds only measures the observe rail writes on every occurrence, so a veto-refused admission stays fault-counter evidence and never a denominator; a success share is a partition over the ONE counter its verdict dimension already fans, because a good-half twin doubles the mounted series and strands its denominator on the next arm edit, and `Ratio` stays reserved for genuinely independent counters; the catalogue and library populations override to `Stat` and carry no objective, because a frozen row count reads as a figure against no ceiling and a population has no reliability target.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;                              // BoardPack, KernelInstruments, Objective, PanelSpec, Sli
using PanelKind = Rasm.Domain.PanelKind;        // the kernel board vocabulary, pinned clear of the sheet-good
                                                // Component/panel#PANEL_FAMILY roster this file's [02] prelude imports
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class MaterialsDescriptors {
    public static readonly BoardPack Pack = new(
        Wire: "materials.catalogue", // the provenance key the deploy tuple admits this projection under; pack and key are one value
        Panels: Seq(
            PanelSpec.Of("Catalogue admissions", MaterialsInstruments.CatalogueAdmits, MaterialsInstruments.FamilySlot),
            PanelSpec.Of("Catalogue rows", MaterialsInstruments.CatalogueRows, PanelKind.Stat),
            PanelSpec.Of("Library rows", MaterialsInstruments.LibraryRows, PanelKind.Stat),
            PanelSpec.Of("Section solve rate", MaterialsInstruments.SectionSolves, MaterialsInstruments.ProfileSlot),
            PanelSpec.Of("Section solve latency", MaterialsInstruments.SectionDuration, MaterialsInstruments.ProfileSlot),
            PanelSpec.Of("Capacity verdicts", MaterialsInstruments.CapacityChecks, MaterialsInstruments.KindSlot, MaterialsInstruments.AdequacySlot),
            PanelSpec.Of("Governing action mix", MaterialsInstruments.CapacityChecks, PanelKind.Table, MaterialsInstruments.GoverningSlot),
            PanelSpec.Of("Capacity utilisation", MaterialsInstruments.CapacityUtilisation, MaterialsInstruments.GoverningSlot),
            PanelSpec.Of("Graph node census", MaterialsInstruments.GraphNodes),
            PanelSpec.Of("Graph compile latency", MaterialsInstruments.GraphDuration),
            PanelSpec.Of("Acquisition fit residual", MaterialsInstruments.AcquisitionResidual, MaterialsInstruments.MethodSlot),
            PanelSpec.Of("Fit rank by method", MaterialsInstruments.AcquisitionFits, MaterialsInstruments.MethodSlot, MaterialsInstruments.RankSlot),
            PanelSpec.Of("Wire mints by method", MaterialsInstruments.WireMints, MaterialsInstruments.MethodSlot),
            PanelSpec.Of("Projection admissions", MaterialsInstruments.ProjectionAdmits),
            PanelSpec.Of("Press throughput", MaterialsInstruments.PressTexels, MaterialsInstruments.BackendSlot),
            PanelSpec.Of("Press latency", MaterialsInstruments.PressDuration, MaterialsInstruments.BackendSlot),
            PanelSpec.Of("Press quality decisions", MaterialsInstruments.PressDowngraded, PanelKind.Table, MaterialsInstruments.ChannelSlot),
            // The press's two quality decisions are DIFFERENT FAILURES and each earns its own panel: a downgrade is a
            // policy fallback the plane survives, while a neutral-filled texel is output the band kernel could not
            // produce. Folding them onto one panel reads a channel that degraded and a channel that faulted as one
            // condition, and the faulted row is the one an operator escalates on.
            PanelSpec.Of("Press faulted texels", MaterialsInstruments.PressFaulted, PanelKind.Table, MaterialsInstruments.ChannelSlot),
            PanelSpec.Of("Tile verdicts", MaterialsInstruments.TileRuns, PanelKind.Table, MaterialsInstruments.StrategySlot, MaterialsInstruments.VerdictSlot),
            PanelSpec.Of("Tileability signal", MaterialsInstruments.TileScore, MaterialsInstruments.ComponentSlot),
            PanelSpec.Of("Ingest classification", MaterialsInstruments.IngestStems, PanelKind.Table, MaterialsInstruments.VerdictSlot, MaterialsInstruments.ReasonSlot),
            PanelSpec.Of("Plane codec volume", MaterialsInstruments.PlaneBytes, MaterialsInstruments.ContainerSlot, MaterialsInstruments.DirectionSlot),
            PanelSpec.Of("Inference mix by licence", MaterialsInstruments.InferRuns, PanelKind.Table, MaterialsInstruments.LicenceSlot),
            PanelSpec.Of("Inference residual", MaterialsInstruments.InferGolden, MaterialsInstruments.StageSlot),
            PanelSpec.Of("Inference partitions", MaterialsInstruments.InferPartitions, MaterialsInstruments.ProviderSlot),
            PanelSpec.Of("Prefilter latency", MaterialsInstruments.PrefilterDuration, MaterialsInstruments.SkySlot),
            PanelSpec.Of("Fault rate", MaterialsInstruments.Faults, KernelInstruments.CategorySlot)),
        Objectives: Seq(
            // Both shares partition ONE mounted population on the verdict dimension its `[04]` arm already stamps, so a
            // denominator and its good half are one series on one write path and the good value is the axis const the
            // roster publishes rather than a literal spelled a second time here. Every row omits its window, so kernel
            // admission canonicalizes the one estate compliance default: restating that default as a calendar literal
            // reads as folder policy and survives the day the kernel moves it, leaving `Slo.Share` publishing two
            // budget spends for one burn row. Target and ceiling stay folder policy — the figures this pack exists
            // to carry — and the window is estate discipline the kernel owns.
            Objective.Create(
                name: "materials.capacity.adequate",
                sli: new Sli.Partition(
                    Metric: MaterialsInstruments.CapacityChecks,
                    By: MaterialsInstruments.AdequacySlot,
                    Good: Seq(MaterialsInstruments.Adequate)),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "materials.acquisition.rank",
                sli: new Sli.Partition(
                    Metric: MaterialsInstruments.AcquisitionFits,
                    By: MaterialsInstruments.RankSlot,
                    Good: Seq(MaterialsInstruments.FullRank)),
                target: 0.98d,
                window: default),
            Objective.Create(
                name: "materials.section.latency",
                sli: new Sli.Latency(Metric: MaterialsInstruments.SectionDuration, Ceiling: Duration.FromMilliseconds(250), Quantile: 0.99d),
                target: 0.99d,
                window: default),
            Objective.Create(
                name: "materials.graph.latency",
                sli: new Sli.Latency(Metric: MaterialsInstruments.GraphDuration, Ceiling: Duration.FromSeconds(2), Quantile: 0.95d),
                target: 0.99d,
                window: default),
            // Presses do BUILD work, so this ceiling stays generous and its target modest — a bake breaching a minute at the
            // tail is a capacity signal rather than an outage, and an aggressive objective here burns budget on the 16k
            // plane the estate deliberately admits.
            Objective.Create(
                name: "materials.texture.press.latency",
                sli: new Sli.Latency(Metric: MaterialsInstruments.PressDuration, Ceiling: Duration.FromSeconds(60), Quantile: 0.95d),
                target: 0.95d,
                window: default),
            // materials.texture.tile grades the one texture OUTCOME the estate owns end to end, and that outcome is
            // ACCEPTANCE rather than measurement: it binds the GRADE population, whose verdict dimension carries the
            // proof's own accept predicate, because a share over the synthesis counter would pass every plane the
            // gate measured and fell short on. The grade population is also the wider one — an ingested set is
            // graded without ever being synthesized — so the objective covers exactly the planes whose tileability
            // the estate claims. The target sits at the provider-fidelity figure because both grade a decision the
            // estate can actually move: a tiling that misses the bar is a synthesizer or a source problem, never a
            // caller's. NO objective binds the ingest counter: a vendor library's alias coverage is a property of
            // the library, so a share over it grades a population against no ceiling this estate controls, the same
            // reason the catalogue and library populations carry none.
            Objective.Create(
                name: "materials.texture.tile",
                sli: new Sli.Partition(
                    Metric: MaterialsInstruments.GradeRuns,
                    By: MaterialsInstruments.VerdictSlot,
                    Good: Seq(MaterialsInstruments.Accepted)),
                target: 0.90d,
                window: default),
            // materials.neural.provider grades PROVIDER FIDELITY, never latency and never the residual: an admitted result
            // already cleared its card's residual ceiling at the ingestion gate, so a residual objective grades a
            // population that cannot fail, while a fleet silently degrading every accelerator request to the CPU floor
            // passes every latency target and is exactly the regression worth alerting on. Sli.Partition splits the ONE
            // mounted inference population on the verdict dimension its arm stamps.
            Objective.Create(
                name: "materials.neural.provider",
                sli: new Sli.Partition(
                    Metric: MaterialsInstruments.InferRuns,
                    By: MaterialsInstruments.FidelitySlot,
                    Good: Seq(MaterialsInstruments.Honoured)),
                target: 0.90d,
                window: default)));
}
```

## [07]-[RESEARCH]

(none)
