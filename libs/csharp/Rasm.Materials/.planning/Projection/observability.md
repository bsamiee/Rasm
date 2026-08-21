# [MATERIALS_OBSERVABILITY]

MATERIALS signal evidence starts with the closed `MaterialsFact` family. `MaterialsPoint` closes the point roster over the kernel `IHookRoster` floor and `MaterialsHooks.Live` mints the ONE kernel `HookRail<MaterialsPoint, MaterialsFact, TelemetrySource>` this folder composes.

`MaterialsInstrument` closes the `rasm.materials.<domain>.<measure>` roster with each row CARRYING its kernel `InstrumentSpec`, and `MaterialsTap` projects the fact stream onto it as a rail subscriber. `MaterialsLog` carries the fixed-severity fault projection beside the `LatencyPhase`/`LatencyMeasure` vocabularies `MaterialsLatency` contributes, and `MaterialsDescriptors` binds the board pack over the same roster.

Settled composition draws every mechanism from the kernel signal capsule — hook rail with its seats, veto folding, replay retention, detach custody, and bounded `FaultCell`; instrument set with its bucket advice, tag projection, and level cells; package-identity band; tenancy frame; SLO algebra.

Fact payloads compose Component, Appearance, Properties, and seam receipts. Instrument names run dotted `rasm.materials.<domain>.<measure>` with UCUM units under the `TelemetrySource.Materials` scope the composing app admits by name, every work row carrying the kernel `rasm.tenant` partition.

## [01]-[INDEX]

- [02]-[FACT_FAMILY]: `MaterialsFact` closes the evidence union and projects each case's own `MaterialsPoint` row.
- [03]-[HOOK_RAIL]: `MaterialsPoint` realizes the kernel roster floor on a `CapabilitySet<HookModality>` column and `MaterialsHooks` mints the kernel rail over it.
- [04]-[INSTRUMENT_TAP]: `MaterialsInstrument` closes the roster, and `MaterialsTap` binds the level probes and projects the fact stream.
- [05]-[EVIDENCE_RECORDS]: `MaterialsLog` carries the fixed-severity projection, `LatencyPhase` and `LatencyMeasure` close the bracket and quantity vocabularies over their `LatencyWrite` laws, and `MaterialsLatency` derives the contributed three-axis roster from them.
- [06]-[BOARD_PACK]: `MaterialsDescriptors` binds the kernel pack over that roster.

## [02]-[FACT_FAMILY]

- Owner: `MaterialsFact` — the closed evidence union every tap fires and every projection folds, its `At` column projecting the `[03]` roster row that owns each case.
- Cases: `CatalogueAdmit` (the row a veto gate transforms or refuses pre-freeze), `SectionSolve` (profile case, solved section, wall duration), `CapacityCheck` (the lifted `CapacityReceipt`, the `Utilisation` verdict, wall duration), `GraphCompile` (material, ordered node count, wall duration), `AcquisitionFit` (the measured `CaptureProvenance` receipt, wall duration), `WireMint` (material, `WireProvenance` receipt), `ProjectionGate` (the `GraphDelta` a veto refuses or admits pre-merge), `TexturePress` (the lifted `PressReceipt` and the material it baked for), `TileSynth` (strategy, guide channel, and the lifted `TileReceipt` — the guide rides beside the receipt for the reason `StageInfer` carries its request: an unmeasured run still names the channel it ran against), `TileGrade` (strategy, the `Evidence<TileProof>` probe outcome the gate's `Fin` lifts through `Evidence.Of`, wall duration — its own case because a grade runs without synthesis and an ingested set earns its proof having passed no synthesizer), `PyramidBuild` (channel, mip policy, level count, texel census, fold duration — the one texture construction every press, ingest, and decode pays per channel), `SetIngest` (the claimed-stem census, the typed refusal rows, and the resolved convention), `PlaneCodec` (container row, direction, stored bytes, wall duration), `StageInfer` (the issued `StageRequest` and the lifted `StageResult` — the request rides so the tap can see a provider DEGRADATION, which the result alone cannot show), `EnvironmentPrefilter` (light key, sky model, level count, wall duration).
- Entry: each composition-root decorator fires one case through `rail.Fire(fact.At, fact, key)`; veto cases fire before catalogue freeze or graph merge.
- Auto: `At` is the PRIMARY CORRESPONDENCE between this union and the `[03]` roster — the generated total `Map` breaks at compile time on a case with no row or a row with no case, so no call site names a point and the pairing cannot drift. Elapsed columns derive from one injected clock at the decorator boundary.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new evidence shape is one `MaterialsFact` case, one `MaterialsPoint` row with its `At` arm, and one projection arm at `[04]`.
- Boundary: facts carry receipts the owning pages already mint — `CapacityReceipt`, `CaptureProvenance`, `WireProvenance`, `ComputedSection`, `PressReceipt`, `TileReceipt`, `StageResult` — and never re-derive their scalars, so a bake's texel census, backend, and elapsed millisecond come off the press's own receipt, a tiling run's two independent signals off the gate's own score, and an inference's provider, partition count, and golden residual off the executor's own result. `PlaneCodec` and `EnvironmentPrefilter` own no receipt, so each carries the four columns its arm reads and nothing more. `SetIngest` carries the manifest's own three columns because `SetManifest` is an accumulating monoid rather than a receipt, and its refusal rows cross TYPED — a formatted token keys a counter on file stems and hands the roster an unbounded dimension it cannot close.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Materials.Appearance;
using Rasm.Materials.Appearance.Interchange;
using Rasm.Materials.Component;
using Rasm.Materials.Raster;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [MODELS] -------------------------------------------------------------------------------
[Union]
public abstract partial record MaterialsFact : IHookFact<MaterialsPoint> {
    private MaterialsFact() { }

    public sealed record CatalogueAdmit(ComponentRow Row) : MaterialsFact;
    public sealed record SectionSolve(Op Key, string Profile, ComputedSection Section, Duration Elapsed) : MaterialsFact;
    public sealed record CapacityCheck(Op Key, CapacityReceipt Receipt, Utilisation Verdict, Duration Elapsed) : MaterialsFact;
    public sealed record GraphCompile(Op Key, MaterialId Material, int Nodes, Duration Elapsed) : MaterialsFact;
    public sealed record AcquisitionFit(Op Key, CaptureProvenance Receipt, Duration Elapsed) : MaterialsFact;
    public sealed record WireMint(Op Key, MaterialId Material, WireProvenance Receipt) : MaterialsFact;
    public sealed record ProjectionGate(GraphDelta Delta) : MaterialsFact;

    // Texture facts lift the receipt each owner already minted rather than re-measuring: PressReceipt carries
    // backend, texels, elapsed, and the CPU-versus-GPU divergence, and StageResult carries the provider used, the
    // partition count, the golden residual, and the tiles inferred — so this family adds evidence SHAPES and no
    // arithmetic. Encoded is the binary direction axis the [04] arm publishes as two dimension values.
    public sealed record TexturePress(Op Key, Option<MaterialId> Material, PressReceipt Receipt) : MaterialsFact;
    public sealed record TileSynth(Op Key, TileStrategy Strategy, TextureChannel Guide, TileReceipt Receipt) : MaterialsFact;
    // Grading lifts the kernel EVIDENCE the gate's Fin folds onto (Evidence.Of at the fire site), never a
    // flattened boolean: a below-bar plane still MINTS its proof, a refused spectral band carries its own cause,
    // and the fact preserves all three states even where the metric arm's closed verdict vocabulary collapses
    // the two non-measured ones. Strategy is the POLICY's, because a graded ingest declares what it was graded
    // against even where nothing synthesized it.
    public sealed record TileGrade(Op Key, TileStrategy Strategy, Evidence<TileProof> Proof, Duration Elapsed) : MaterialsFact;
    public sealed record PyramidBuild(Op Key, TextureChannel Channel, MipPolicy Policy, int Levels, long Texels, Duration Elapsed) : MaterialsFact;
    public sealed record SetIngest(Op Key, int Claimed, Seq<(IngestRefusal Reason, string Detail)> Unresolved, Option<NormalConvention> Convention) : MaterialsFact;
    public sealed record PlaneCodec(Op Key, RasterFormat Format, bool Encoded, long Bytes, Duration Elapsed) : MaterialsFact;
    public sealed record StageInfer(Op Key, StageRequest Request, StageResult Result) : MaterialsFact;
    public sealed record EnvironmentPrefilter(Op Key, string LightKey, string SkyModel, int SpecularMips, Duration Elapsed) : MaterialsFact;

    // The ONE fact-to-point correspondence: every fire site hands `fact.At` to the rail, so a point spelling never
    // reaches a call site and the roster's totality is the compiler's rather than a convention's. Seats is the
    // kernel IHookFact seating correspondence HookRail constrains TFact on — derived off this same generated Map.
    public bool Seats(MaterialsPoint at) => at == At;

    public MaterialsPoint At => Map(
        catalogueAdmit:        MaterialsPoint.CatalogueAdmit,
        sectionSolve:          MaterialsPoint.SectionSolve,
        capacityCheck:         MaterialsPoint.CapacityCheck,
        graphCompile:          MaterialsPoint.GraphCompile,
        acquisitionFit:        MaterialsPoint.AcquisitionFit,
        wireMint:              MaterialsPoint.WireMint,
        projectionGate:        MaterialsPoint.ProjectionGate,
        texturePress:          MaterialsPoint.TexturePress,
        tileSynth:             MaterialsPoint.TileSynth,
        tileGrade:             MaterialsPoint.TileGrade,
        pyramidBuild:          MaterialsPoint.PyramidBuild,
        setIngest:             MaterialsPoint.SetIngest,
        planeCodec:            MaterialsPoint.PlaneCodec,
        stageInfer:            MaterialsPoint.StageInfer,
        environmentPrefilter:  MaterialsPoint.EnvironmentPrefilter);
}
```

## [03]-[HOOK_RAIL]

- Owner: `MaterialsPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.materials.<domain>.<point>`, realizing the kernel `IHookRoster<MaterialsPoint>` floor with a `CapabilitySet<HookModality>` column; `MaterialsHooks` the composition entry minting the ONE kernel `HookRail<MaterialsPoint, MaterialsFact, TelemetrySource>`. The folder mints ZERO rail mechanism — seats, veto folding, bounded replay, fork-shielded isolation, detach custody, owner-scoped release, and the bounded `FaultCell` all ride the kernel rail.
- Cases: `rasm.materials.catalogue.admit` veto, `rasm.materials.section.solve`, `rasm.materials.capacity.check`, `rasm.materials.graph.compile`, `rasm.materials.acquisition.fit` replay, `rasm.materials.wire.mint`, `rasm.materials.projection.project` veto, `rasm.materials.texture.press`, `rasm.materials.texture.tile`, `rasm.materials.texture.grade`, `rasm.materials.texture.pyramid`, `rasm.materials.texture.ingest`, `rasm.materials.texture.codec`, `rasm.materials.neural.infer` replay, `rasm.materials.environment.prefilter`. The two replay rows settle a costly external computation whose evidence a later run re-reads rather than re-earns.
- Entry: `MaterialsHooks.Live(key, gates, taps, cell)` mints the rail once at composition, seating one kernel point per `MaterialsPoint` row from `Items` alone; `rail.Fire(fact.At, fact, key)` is the emitter entry and `rail.Points` the census a `HookRegistry` freezes at the app root; `HookMounts<MaterialsPoint, TelemetrySource>` carries any rider custody a host composition claims over these seats.
- Auto: every point admits `HookModality.Observe` beside whatever else it holds, because the `[04]` projection is ONE unscoped tap over a total `Switch` and a veto-only or replay-only set refuses it; the roster's `Id` and the rail's seats both derive from the row key, so a `Live` seat cannot re-spell either.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new point is one `MaterialsPoint` row, one `MaterialsFact` case, and one `At` arm; delivery semantics are the kernel modality rows.
- Law: NAMED LOSS from composing the kernel rail — the per-point FACT TYPE. A subscriber to a named `HookPoint<MaterialsFact.TilePress>` field took no codec fact; under one rail every point shares `MaterialsFact` and subscribers discriminate on the case. What survives is stronger: `At` fixes the case-to-row pairing at compile time, so the guarantee moved from a field's declaration onto a generated total map. WITNESS — the fifteen `HookPoint<MaterialsFact.*>` columns, the fifteen-line `Live()`, the fifteen-entry `Points` census, and the private `Seat<TFact>` mint all delete onto `MaterialsRail.Of`.
- Boundary: ids and modalities live on the roster rows alone, so a Materials point joins any app-tier registry census unrenamed; a subscriber fault parks as `IsolatedFault` on the composition's own bounded cell and the emitter is untouched, the ring shedding oldest-first rather than growing for process lifetime. Veto points carry observe subscribers legally and the capsule dispatches them from the admitted fact alone, so a `[04]` arm on a veto point counts admitted rows and refusal volume rides the cell. Spans are absent by design: this folder's eager constructions carry the `[05]` checkpoint ledger instead, so `Plane` is `None` on every row, no `TraceScope` derives off these ids, and `Live` binds no `IHookSpan`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

// The kernel rail closed over this folder's roster/fact/owner triple — one alias set so every signature reads the
// domain name, never the three-parameter spelling.
using MaterialsGate = Rasm.Domain.HookGate<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;
using MaterialsObserver = Rasm.Domain.HookTap<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;
using MaterialsRail = Rasm.Domain.HookRail<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Materials.Projection;

// --- [TYPES] ----------------------------------------------------------------------------------
// Point roster keyed rasm.materials.<domain>.<point> — the kernel HookId four-segment grammar. Realizing
// IHookRoster<MaterialsPoint> is what lets the ONE kernel HookRail take this roster as its type parameter and mint
// seats from Items alone, so an inline HookId.Create literal at a call site stops compiling. Modality is the kernel
// capability column deciding veto admission and replay retention.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaterialsPoint : IHookRoster<MaterialsPoint> {
    public static readonly MaterialsPoint CatalogueAdmit = new("rasm.materials.catalogue.admit", CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe));
    public static readonly MaterialsPoint SectionSolve = new("rasm.materials.section.solve", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint CapacityCheck = new("rasm.materials.capacity.check", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint GraphCompile = new("rasm.materials.graph.compile", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint AcquisitionFit = new("rasm.materials.acquisition.fit", CapabilitySet<HookModality>.Of(HookModality.Replay, HookModality.Observe));
    public static readonly MaterialsPoint WireMint = new("rasm.materials.wire.mint", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint ProjectionGate = new("rasm.materials.projection.project", CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe));
    public static readonly MaterialsPoint TexturePress = new("rasm.materials.texture.press", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint TileSynth = new("rasm.materials.texture.tile", CapabilitySet<HookModality>.Of(HookModality.Observe));
    // Grading is its OWN point because it runs without synthesis: an ingested third-party set is graded to earn its
    // TileProof and never passes through the synthesizer, so a grade folded into the synthesis point is invisible
    // for exactly the population whose tileability nothing else measures.
    public static readonly MaterialsPoint TileGrade = new("rasm.materials.texture.grade", CapabilitySet<HookModality>.Of(HookModality.Observe));
    // The pyramid fold is the one texture construction every press, ingest, and decode pays per channel, so a
    // mip-policy choice that costs a fold has no series that shows it without this point.
    public static readonly MaterialsPoint PyramidBuild = new("rasm.materials.texture.pyramid", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint SetIngest = new("rasm.materials.texture.ingest", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint PlaneCodec = new("rasm.materials.texture.codec", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint StageInfer = new("rasm.materials.neural.infer", CapabilitySet<HookModality>.Of(HookModality.Replay, HookModality.Observe));
    public static readonly MaterialsPoint EnvironmentPrefilter = new("rasm.materials.environment.prefilter", CapabilitySet<HookModality>.Of(HookModality.Observe));

    // One materialized index answers the floor's Id read, so a fire pays a lookup rather than re-parsing the key
    // through HookId.Create. Lazy, never a static readonly fold: the generator fills Items from its own static
    // constructor, so an eager field would freeze an EMPTY roster.
    static readonly Lazy<FrozenDictionary<MaterialsPoint, HookId>> Ids = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => HookId.Create(value: row.Key)),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public CapabilitySet<HookModality> Modalities { get; }

    public HookId Id => Ids.Value[this];

    // This folder brackets nothing: the [05] checkpoint ledger carries its eager constructions, so no roster row
    // publishes a trace plane and the rail's own Traced fold degenerates to the body.
    public Option<TraceScope> Plane => Option<TraceScope>.None;
}

// --- [SERVICES] -----------------------------------------------------------------------------
// The composition entry preserves the exact rail refusal; the hook owner has already classified and attributed it.
public static class MaterialsHooks {
    public static Fin<MaterialsRail> Live(
        Op key, Seq<MaterialsGate> gates = default, Seq<MaterialsObserver> taps = default,
        Option<FaultCell> cell = default) =>
        MaterialsRail.Of(key, gates, taps, Option<IHookSpan>.None, cell);
}
```

## [04]-[INSTRUMENT_TAP]

- Owner: `MaterialsInstrument` — the closed `rasm.materials.*` roster, a `[SmartEnum<string>]` whose every row CARRIES its kernel `InstrumentSpec` (kind, measurement form, UCUM unit, kernel `Buckets` advice, the closed dimension set) beside the one dotted slot and outcome-value block both the metric writes and the `[05]` pivots spell; `MaterialsTap` — the fact-to-write projection and the level-probe binder over the `InstrumentSet` the composing root materializes.
- Cases: the roster below IS the case list, and restating each row's dimensions here publishes a second roster that drifts on the first column edit. Rows split three ways by what produces them: PUSHED rows the `[02]` fact arms write on every occurrence; the two composition-supplied POPULATION levels a frozen catalogue and its material library answer; and the three RAIL-ANSWERED rows the evidence cell probes, since a shielded subscriber failure fires no fact and so has no write site at all.
- Entry: `MaterialsInstrument.Telemetry(version)` — the one contributor port, carrying the `[06]` board pack beside these rows so board and reliability policy travel downward with the roster they name; `MaterialsTap.Tap(set)` returns the ONE unscoped `MaterialsObserver` the composing root hands `MaterialsHooks.Live`; `MaterialsTap.Levels(set, rail, key, supplied)` binds every pulled row — the rail's own probes from `RailLevels` and the composition's catalogue and library readers from the span — and returns the scopes that retire them.
- Auto: `Rows` DERIVES from `Items` and construction proves each row's name against its key, so the const-name roster and the hand-listed sequence that mirrored it are ONE declaration; every write addresses the row (`InstrumentSet.Write(row.Row, …)`, the kernel's own write law) rather than a name, so an unmounted row and a form mismatch surface as typed refusals; every histogram row binds its named kernel `Buckets` row as explicit-bucket advice under the base2-exponential wire default, so no bound array is spelled here; every write materializes its tag set through `InstrumentSet.Tags`, which returns the stack-allocated `TagList` the kernel's `in TagList` overload consumes and folds the ambient `TenantContext` partition in beside the arm's own slots — an arm widening a shared set does it by COPY-THEN-ADD through the page's one `Keyed` widening, never by materializing a heap `KeyValuePair<string, object?>[]`; a multi-write arm reads `Enabled` ahead of its tag mint and ahead of any receipt-collection walk, and an unmounted name reads enabled so the gate never absorbs the refusal a write owes; each share indicator's outcome verdict rides the same write that counts the occurrence, so a good half can never miss an occurrence its denominator recorded.
- Packages: Rasm, LanguageExt.Core, BCL inbox (`System.Diagnostics.Metrics`).
- Growth: a histogram policy change is one kernel `Buckets` row reference; a new instrument is one `MaterialsInstrument` row carrying its own UCUM unit — `{texel}`, `By`, `{partition}`, `{inference}`, `{tile}`, `{stem}`, `{channel}` — and one write in the owning `Switch` arm, a new fact case breaking the tap at compile time; a new tileability signal is one `ScoreComponents` row and no arm edit; a new rail-answered level is one `RailLevels` row and a new supplied level one `Level` row with its reader at the call site, never a signature edit.
- Law: throughput rides MONOTONE COUNTERS in UCUM units and latency rides the histograms — a bake spans four orders of magnitude between a preview and a production plane, so a bucket ladder over texel or byte volume grades nothing while the counter's own derivative is exactly the rate a board reads.
- Law: a REFUSAL is counted and never measured — an unmeasured run enters its counter's own verdict partition while every histogram gates on the evidence that proves a measurement was taken, because a sentinel admitted into a distribution reads to a board as the best value in it.
- Law: `Levels` proves the supply a BIJECTION against the roster's own pulled column minus the rail-answered rows — a name outside that set refuses, a pulled row with no reader refuses, and a name supplied twice refuses before its second bind shadows the first. NAMED LOSS from composing the kernel probe: `InstrumentSet.Bind` takes `Func<double>` and the mounted row saturates each reading into its declared carrier at collection, so the folder's own whole-number domain gate deletes and a starved reader publishes a saturated value rather than refusing at bind.
- Law: NAMED LOSS on the fault series — the kernel `FaultCell` is a bounded ring publishing `Parked`, `Shed`, and `Lost` and raising no change event, so the monotone count the rail's evidence once pushed has no producer. One level probes total parked Materials-owned depth beside two monotone ring tallies, and the ever-parked total reads as depth summed with shed rather than as a counter nothing writes.
- Boundary: `MaterialId` and the solved `ComputedSection` stay fact evidence with no arm — material identity is identifier-grade and belongs on typed receipts, never on a metric series. Tenancy is the kernel `TenantContext` projection every work-row write folds, so this page holds no tenant key, no baggage read, and no zero sentinel, while the two pulled POPULATION rows stay untenanted on ownership alone: a frozen catalogue and its material library are process-scoped reference data no tenant owns, so a tenant column there declares a key no reader can emit. Every projection arm returns the kernel write rail and subscribes through the rail's shielded tap, so a refused write parks as `IsolatedFault` beside every other tap fault and no folder-local lift aspect exists. Instrument custody stays the composing app's — this spine binds and subscribes against a mounted `InstrumentSet` and mints no meter.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System;
using System.Diagnostics;
using LanguageExt;
using Rasm.Domain;
using Rasm.Materials.Raster;
using static LanguageExt.Prelude;

using MaterialsObserver = Rasm.Domain.HookTap<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;
using MaterialsRail = Rasm.Domain.HookRail<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Materials.Projection;

// --- [TABLES] ---------------------------------------------------------------------------------
// Each row CARRIES its declaration, so `Rows` derives from `Items` and the two-declaration shape — a const-name
// block beside a hand-listed Seq — collapses to one. The write plane addresses by ROW (the kernel instrument law)
// and construction proves the row's name against its key.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaterialsInstrument {
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
    // graded and the press channel that degraded are one axis under one key, so a board joins a tile verdict to a
    // press downgrade on the channel column; a second guide-named key forks that join the day either moves.
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

    // Outcome values publish beside their slots because a partitioned counter's good half is read TWICE — the tap
    // arm stamps it and the [06] indicator names it as the partition's good set — so a value literal at either site
    // forks that share the moment the other moves. Each pair fans ONE counter; neither half earns its own.
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
    // one absence the gate answers. Folding rejected into unmeasured reports a healthy estate whose tilings were
    // never graded. The synthesis population carries only the measured/unmeasured half, since a synth receipt
    // records what it produced and the acceptance bar rides the proof.
    public const string Accepted = "accepted";
    public const string Rejected = "rejected";
    public const string Measured = "measured";
    public const string Unmeasured = "unmeasured";
    public const string Claimed = "claimed";
    public const string Unresolved = "unresolved";

    public static readonly MaterialsInstrument CatalogueAdmits = new(
        "rasm.materials.catalogue.admits",
        InstrumentSpec.Create("rasm.materials.catalogue.admits", InstrumentKind.Count, MeasureForm.Whole, "{row}",
            "catalogue rows admitted through the freeze veto by family", Seq(TenantContext.TenantSlot, FamilySlot), None, None, None));

    // The two POPULATION rows carry no tenant slot for ONE reason: a frozen catalogue and its material library are
    // process-scoped reference data no tenant owns, so a tenant column there declares a key no reader can emit.
    public static readonly MaterialsInstrument CatalogueRows = new(
        "rasm.materials.catalogue.rows",
        InstrumentSpec.Create("rasm.materials.catalogue.rows", InstrumentKind.Level, MeasureForm.Whole, "{row}",
            "frozen catalogue row population", Seq<string>(), None, None, None));

    public static readonly MaterialsInstrument LibraryRows = new(
        "rasm.materials.library.rows",
        InstrumentSpec.Create("rasm.materials.library.rows", InstrumentKind.Level, MeasureForm.Whole, "{row}",
            "admitted material-library row population", Seq<string>(), None, None, None));

    public static readonly MaterialsInstrument SectionSolves = new(
        "rasm.materials.section.solves",
        InstrumentSpec.Create("rasm.materials.section.solves", InstrumentKind.Count, MeasureForm.Whole, "{solve}",
            "profile section solves by profile case", Seq(TenantContext.TenantSlot, ProfileSlot), None, None, None));

    public static readonly MaterialsInstrument SectionDuration = new(
        "rasm.materials.section.duration",
        InstrumentSpec.Create("rasm.materials.section.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "profile section solve wall duration", Seq(TenantContext.TenantSlot, ProfileSlot), Some(Buckets.SolveSeconds), None, None));

    public static readonly MaterialsInstrument CapacityChecks = new(
        "rasm.materials.capacity.checks",
        InstrumentSpec.Create("rasm.materials.capacity.checks", InstrumentKind.Count, MeasureForm.Whole, "{check}",
            "capacity checks by receipt kind, governing action, and adequacy verdict",
            Seq(TenantContext.TenantSlot, KindSlot, GoverningSlot, AdequacySlot), None, None, None));

    public static readonly MaterialsInstrument CapacityUtilisation = new(
        "rasm.materials.capacity.utilisation",
        InstrumentSpec.Create("rasm.materials.capacity.utilisation", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "governing utilisation ratio per capacity check",
            Seq(TenantContext.TenantSlot, KindSlot, GoverningSlot), Some(Buckets.GoverningRatio), None, None));

    // Compile cost reads against graph size, so the node census rides the same fact its duration does — a duration
    // alone cannot separate a slow compiler from a large graph.
    public static readonly MaterialsInstrument GraphNodes = new(
        "rasm.materials.graph.nodes",
        InstrumentSpec.Create("rasm.materials.graph.nodes", InstrumentKind.Distribution, MeasureForm.Whole, "{node}",
            "ordered node count per material graph compile", Seq(TenantContext.TenantSlot), Some(Buckets.GraphCounts), None, None));

    public static readonly MaterialsInstrument GraphDuration = new(
        "rasm.materials.graph.duration",
        InstrumentSpec.Create("rasm.materials.graph.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "material graph compile wall duration", Seq(TenantContext.TenantSlot), Some(Buckets.CompileSeconds), None, None));

    public static readonly MaterialsInstrument AcquisitionFits = new(
        "rasm.materials.acquisition.fits",
        InstrumentSpec.Create("rasm.materials.acquisition.fits", InstrumentKind.Count, MeasureForm.Whole, "{fit}",
            "acquisition fits settled by capture method and parameter-rank verdict",
            Seq(TenantContext.TenantSlot, MethodSlot, RankSlot), None, None, None));

    public static readonly MaterialsInstrument AcquisitionResidual = new(
        "rasm.materials.acquisition.residual",
        InstrumentSpec.Create("rasm.materials.acquisition.residual", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "acquisition fit RMS residual by capture method",
            Seq(TenantContext.TenantSlot, MethodSlot), Some(Buckets.ResidualDecades), None, None));

    public static readonly MaterialsInstrument WireMints = new(
        "rasm.materials.wire.mints",
        InstrumentSpec.Create("rasm.materials.wire.mints", InstrumentKind.Count, MeasureForm.Whole, "{wire}",
            "appearance wire mints by capture method", Seq(TenantContext.TenantSlot, MethodSlot), None, None, None));

    public static readonly MaterialsInstrument ProjectionAdmits = new(
        "rasm.materials.projection.admits",
        InstrumentSpec.Create("rasm.materials.projection.admits", InstrumentKind.Count, MeasureForm.Whole, "{delta}",
            "graph deltas the projection veto admitted", Seq(TenantContext.TenantSlot), None, None, None));

    // Texture-plane throughput rides MONOTONE COUNTERS in UCUM units — `{texel}` shaded and `By` stored — because a
    // bake's magnitude spans four orders between a 256 preview and a 16k production plane; the per-run DURATION
    // beside it carries the distribution that does grade.
    public static readonly MaterialsInstrument PressRuns = new(
        "rasm.materials.texture.presses",
        InstrumentSpec.Create("rasm.materials.texture.presses", InstrumentKind.Count, MeasureForm.Whole, "{press}",
            "texture presses settled by backend", Seq(TenantContext.TenantSlot, BackendSlot), None, None, None));

    public static readonly MaterialsInstrument PressTexels = new(
        "rasm.materials.texture.texels",
        InstrumentSpec.Create("rasm.materials.texture.texels", InstrumentKind.Count, MeasureForm.Whole, "{texel}",
            "texels shaded across every channel and mip level, by backend", Seq(TenantContext.TenantSlot, BackendSlot), None, None, None));

    // DecodeSeconds (10 ms – 300 s) is the ladder a bake demands: a 256 preview lands sub-second and a 16k plane
    // runs past the 60 s objective, so the 2 s-ceiling compile ladder and the 250 ms-ceiling solve ladder both
    // saturate their top bucket exactly where the distribution matters.
    public static readonly MaterialsInstrument PressDuration = new(
        "rasm.materials.texture.press.duration",
        InstrumentSpec.Create("rasm.materials.texture.press.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "texture press wall duration by backend", Seq(TenantContext.TenantSlot, BackendSlot), Some(Buckets.DecodeSeconds), None, None));

    // Channel keys the two quality decisions the press makes silently, so an operator reads which plane degraded
    // rather than that something did. Faulted counts TEXELS off the per-channel tally the receipt carries, since a
    // channel count grades a one-texel fault and a whole-plane fault as one event.
    public static readonly MaterialsInstrument PressDowngraded = new(
        "rasm.materials.texture.press.downgraded",
        InstrumentSpec.Create("rasm.materials.texture.press.downgraded", InstrumentKind.Count, MeasureForm.Whole, "{channel}",
            "channels whose paired mip policy fell back to the box floor, by backend and channel",
            Seq(TenantContext.TenantSlot, BackendSlot, ChannelSlot), None, None, None));

    public static readonly MaterialsInstrument PressFaulted = new(
        "rasm.materials.texture.press.faulted",
        InstrumentSpec.Create("rasm.materials.texture.press.faulted", InstrumentKind.Count, MeasureForm.Whole, "{texel}",
            "texels neutral-filled by a failed band kernel, by backend and channel",
            Seq(TenantContext.TenantSlot, BackendSlot, ChannelSlot), None, None, None));

    // The divergence ladder, never the residual decades: a CPU-versus-GPU delta is a RATIO that matters between one
    // percent and two, where a decade ladder spends eight of its ten buckets below 1e-3 grading noise.
    public static readonly MaterialsInstrument PressGpuDelta = new(
        "rasm.materials.texture.press.gpu.delta",
        InstrumentSpec.Create("rasm.materials.texture.press.gpu.delta", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "worst per-channel CPU-versus-GPU divergence on a two-lane press, by backend",
            Seq(TenantContext.TenantSlot, BackendSlot), Some(Buckets.DivergenceRatio), None, None));

    // Aging coverage is ABSENT for every non-Aged program, so the arm gates on presence exactly as the gpu delta
    // does; the value is visited-over-declared per ladder axis, and an under-exercised dimension reads as a share
    // below one rather than as a silent re-press question.
    public static readonly MaterialsInstrument PressAgeCoverage = new(
        "rasm.materials.texture.press.aging.coverage",
        InstrumentSpec.Create("rasm.materials.texture.press.aging.coverage", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "ladder rungs visited over rungs declared, by backend and ladder axis",
            Seq(TenantContext.TenantSlot, BackendSlot, AxisSlot), Some(Buckets.Fractions), None, None));

    // Tileability is TWO independent measurements against one verdict, so the signal fans on a COMPONENT dimension
    // rather than publishing the product alone: a seam ratio alone passes a blurred border and a lattice leak alone
    // passes a sharp-but-quiet seam. The counter partitions every run; only a graded run reaches the histogram.
    public static readonly MaterialsInstrument TileRuns = new(
        "rasm.materials.texture.tiles",
        InstrumentSpec.Create("rasm.materials.texture.tiles", InstrumentKind.Count, MeasureForm.Whole, "{tile}",
            "tiling runs settled by strategy, guide channel, and grade verdict",
            Seq(TenantContext.TenantSlot, StrategySlot, ChannelSlot, VerdictSlot), None, None, None));

    // A tileability signal is a UNIT-INTERVAL score, so it takes the kernel's own fraction ladder: the residual
    // decades grade a quantity approaching zero, where every one of these values lives between 0 and 1.
    public static readonly MaterialsInstrument TileScoreSignal = new(
        "rasm.materials.texture.tile.score",
        InstrumentSpec.Create("rasm.materials.texture.tile.score", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "tileability signal by strategy and score component",
            Seq(TenantContext.TenantSlot, StrategySlot, ComponentSlot), Some(Buckets.Fractions), None, None));

    public static readonly MaterialsInstrument TileDuration = new(
        "rasm.materials.texture.tile.duration",
        InstrumentSpec.Create("rasm.materials.texture.tile.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "tiling synthesis wall duration by strategy",
            Seq(TenantContext.TenantSlot, StrategySlot), Some(Buckets.DecodeSeconds), None, None));

    // Grading is measured SEPARATELY from synthesis because the two populations differ: every synthesized plane is
    // graded, and so is every ingested set nothing synthesized, so a grade counter folded into the synthesis one
    // loses exactly the third-party population whose tileability nothing else reports.
    public static readonly MaterialsInstrument GradeRuns = new(
        "rasm.materials.texture.grades",
        InstrumentSpec.Create("rasm.materials.texture.grades", InstrumentKind.Count, MeasureForm.Whole, "{grade}",
            "tileability grades settled by strategy and proof verdict",
            Seq(TenantContext.TenantSlot, StrategySlot, VerdictSlot), None, None, None));

    public static readonly MaterialsInstrument GradeScore = new(
        "rasm.materials.texture.grade.score",
        InstrumentSpec.Create("rasm.materials.texture.grade.score", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "combined tileability verdict of every minted proof by strategy",
            Seq(TenantContext.TenantSlot, StrategySlot), Some(Buckets.Fractions), None, None));

    // Levels ride a counter rather than a histogram — a level count is a small bounded integer whose sum over a run
    // is the fold volume; the policy is the dimension that makes the duration actionable.
    public static readonly MaterialsInstrument PyramidLevels = new(
        "rasm.materials.texture.pyramid.levels",
        InstrumentSpec.Create("rasm.materials.texture.pyramid.levels", InstrumentKind.Count, MeasureForm.Whole, "{level}",
            "pyramid levels folded by channel and mip policy",
            Seq(TenantContext.TenantSlot, ChannelSlot, PolicySlot), None, None, None));

    public static readonly MaterialsInstrument PyramidDuration = new(
        "rasm.materials.texture.pyramid.duration",
        InstrumentSpec.Create("rasm.materials.texture.pyramid.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "pyramid fold wall duration by channel and mip policy",
            Seq(TenantContext.TenantSlot, ChannelSlot, PolicySlot), Some(Buckets.DecodeSeconds), None, None));

    // Refusal reasons cross as BOUNDED rows rather than a formatted token, so one series answers both operator
    // questions an asset-library ingest raises — how much of a vendor set this estate's alias table claims, and why
    // the remainder did not classify.
    public static readonly MaterialsInstrument IngestStems = new(
        "rasm.materials.texture.ingest.stems",
        InstrumentSpec.Create("rasm.materials.texture.ingest.stems", InstrumentKind.Count, MeasureForm.Whole, "{stem}",
            "ingested plane stems by classification verdict and refusal reason",
            Seq(TenantContext.TenantSlot, VerdictSlot, ReasonSlot), None, None, None));

    public static readonly MaterialsInstrument PlaneBytes = new(
        "rasm.materials.texture.plane.bytes",
        InstrumentSpec.Create("rasm.materials.texture.plane.bytes", InstrumentKind.Count, MeasureForm.Whole, "By",
            "plane bytes encoded or decoded by container and direction",
            Seq(TenantContext.TenantSlot, ContainerSlot, DirectionSlot), None, None, None));

    public static readonly MaterialsInstrument CodecDuration = new(
        "rasm.materials.texture.codec.duration",
        InstrumentSpec.Create("rasm.materials.texture.codec.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "plane codec wall duration by container and direction",
            Seq(TenantContext.TenantSlot, ContainerSlot, DirectionSlot), Some(Buckets.DecodeSeconds), None, None));

    // Licence rides the inference POPULATION because a fleet operator's first question about a model estate is
    // which grant class its running inferences fall under — a research-class row appearing in production is a
    // posture change no duration or residual would surface.
    public static readonly MaterialsInstrument InferRuns = new(
        "rasm.materials.neural.infers",
        InstrumentSpec.Create("rasm.materials.neural.infers", InstrumentKind.Count, MeasureForm.Whole, "{inference}",
            "photo-to-PBR inferences settled by stage, provider, licence class, and provider fidelity",
            Seq(TenantContext.TenantSlot, StageSlot, ProviderSlot, LicenceSlot, FidelitySlot), None, None, None));

    public static readonly MaterialsInstrument InferPartitions = new(
        "rasm.materials.neural.partitions",
        InstrumentSpec.Create("rasm.materials.neural.partitions", InstrumentKind.Distribution, MeasureForm.Whole, "{partition}",
            "ONNX graph partitions reached per inference by stage and provider",
            Seq(TenantContext.TenantSlot, StageSlot, ProviderSlot), Some(Buckets.GraphCounts), None, None));

    public static readonly MaterialsInstrument InferGolden = new(
        "rasm.materials.neural.golden",
        InstrumentSpec.Create("rasm.materials.neural.golden", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "inference residual against the model's CPU-reference output by stage and provider",
            Seq(TenantContext.TenantSlot, StageSlot, ProviderSlot), Some(Buckets.ResidualDecades), None, None));

    public static readonly MaterialsInstrument PrefilterRuns = new(
        "rasm.materials.environment.prefilters",
        InstrumentSpec.Create("rasm.materials.environment.prefilters", InstrumentKind.Count, MeasureForm.Whole, "{prefilter}",
            "IBL prefilters settled by sky model", Seq(TenantContext.TenantSlot, SkySlot), None, None, None));

    public static readonly MaterialsInstrument PrefilterDuration = new(
        "rasm.materials.environment.prefilter.duration",
        InstrumentSpec.Create("rasm.materials.environment.prefilter.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "IBL prefilter wall duration by sky model",
            Seq(TenantContext.TenantSlot, SkySlot), Some(Buckets.DecodeSeconds), None, None));

    // The rail's evidence cell answers all three fault rows: a shielded subscriber failure never fires a fact, so a
    // pushed row would have no write site at all. Depth counts Materials-owned faults; shed and declined are the
    // ring's own bounded-eviction tallies, without which a tap storm reads as a quiet estate.
    public static readonly MaterialsInstrument Faults = new(
        "rasm.materials.faults",
        InstrumentSpec.Create("rasm.materials.faults", InstrumentKind.Levels, MeasureForm.Whole, "{fault}",
            "parked Materials tap and veto faults held on the rail's evidence cell",
            Seq<string>(), None, None, None));

    public static readonly MaterialsInstrument FaultsShed = new(
        "rasm.materials.faults.shed",
        InstrumentSpec.Create("rasm.materials.faults.shed", InstrumentKind.Total, MeasureForm.Whole, "{fault}",
            "parked faults the bounded evidence ring evicted oldest-first", Seq<string>(), None, None, None));

    public static readonly MaterialsInstrument FaultsLost = new(
        "rasm.materials.faults.lost",
        InstrumentSpec.Create("rasm.materials.faults.lost", InstrumentKind.Total, MeasureForm.Whole, "{fault}",
            "parks the evidence ring declined under contention", Seq<string>(), None, None, None));

    public InstrumentSpec Row { get; }

    public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Row).Strict();

    // Rows and the [06] pack leave as ONE downward fact, so the mounting root proves the pack in one fold binding
    // these handles. Forward reach is safe by construction: the pack reads THIS roster's rows, and this factory is
    // a method the pack's own initializer never calls, so no initialization cycle exists in either direction.
    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: TelemetrySource.Materials, Version: version, Instruments: Rows, Board: MaterialsDescriptors.Pack);

    static partial void ValidateConstructorArguments(ref string key, ref InstrumentSpec row) {
        if (!string.Equals(key, row.Name, StringComparison.Ordinal)) {
            throw new ArgumentException($"<materials-instrument:{key}>", nameof(row));
        }
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// Fact-to-write projection and level binding over the composition's InstrumentSet — no minted state, so provider
// disposal owns instrument lifetime and this owner holds nothing to dispose.
public static class MaterialsTap {
    // ONE unscoped rail subscription: the projection owns a TOTAL Switch, so it wants every point and a scope row
    // would be a second, drift-prone statement of the totality the compiler already enforces. The typed refusal
    // rides straight out, so the rail's shield parks it as point-attributed evidence.
    public static MaterialsObserver Tap(InstrumentSet set) =>
        new(Op.Of(name: "rasm.materials.instruments"), fact => Project(set, fact));

    // The rail-ANSWERED pulled rows and their probe fans — one authority, from which the composition-supplied set
    // derives as the complement, so no hand-kept mirror of "which levels the caller owes" exists to drift. A row's
    // fan is a Seq because a keyed family answers one probe per partition and a scalar row answers exactly one.
    static readonly Seq<(MaterialsInstrument Row, Func<MaterialsRail, Seq<(Option<string> Partition, Func<double> Read)>> Probe)> RailLevels =
        Seq<(MaterialsInstrument, Func<MaterialsRail, Seq<(Option<string>, Func<double>)>>)>(
            (MaterialsInstrument.Faults, static rail => Seq<(Option<string>, Func<double>)>(
                (Option<string>.None, () => rail.Faults.Parked.Count(fault =>
                    fault.Cause is Fault typed
                    && FaultBand.OwnerOf(BandKind.Fault, typed.Code).Exists(static band => band.Owner == TelemetrySource.Materials))))),
            (MaterialsInstrument.FaultsShed, static rail => Seq<(Option<string>, Func<double>)>(
                (Option<string>.None, () => rail.Faults.Shed))),
            (MaterialsInstrument.FaultsLost, static rail => Seq<(Option<string>, Func<double>)>(
                (Option<string>.None, () => rail.Faults.Lost))));

    // Pulled populations arrive as composition-supplied readers: only the composing app holds the frozen catalogue
    // and its material library. The roster's own Pulled column, minus the rail-answered rows, is the completeness
    // proof in EVERY direction — a stray name, a starved gauge, and a name supplied twice all refuse here rather
    // than reporting a silent zero for the process lifetime or leaving one cell holding whichever reader bound last.
    // Tenant resolves ONCE at bind, which is correct for a composition-scoped probe where a per-read resolve is not.
    public static Fin<Seq<IDisposable>> Levels(
        InstrumentSet set, MaterialsRail rail, Op key,
        params ReadOnlySpan<(MaterialsInstrument Row, Func<double> Read)> supplied) {
        Seq<(MaterialsInstrument Row, Func<double> Read)> offered = toSeq(supplied.ToArray());
        Seq<MaterialsInstrument> owed = toSeq(MaterialsInstrument.Items)
            .Filter(static row => row.Row.Kind.Pulled && !RailLevels.Exists(probe => probe.Row.Equals(row)))
            .Strict();
        return offered.Map(static row => row.Row).Collisions(static row => row) is { IsEmpty: false } twice
            ? Fin.Fail<Seq<IDisposable>>(new KernelFault.InvalidValue(
                Label: string.Join(", ", twice.Map(static row => row.Key)),
                Requirement: "one reader per pulled roster row"))
            : owed.Count != offered.Count || owed.Exists(row => !offered.Exists(entry => entry.Row.Equals(row)))
                ? Fin.Fail<Seq<IDisposable>>(new KernelFault.InvalidValue(
                    Label: TelemetrySource.Materials.Key,
                    Requirement: "exactly one supplied reader for every composition-owned pulled roster row"))
                : (RailLevels.Bind(probe => probe.Probe(rail).Map(fan => (probe.Row, fan.Partition, fan.Read)))
                    + offered.Map(static row => (row.Row, Partition: Option<string>.None, row.Read)))
                    .TraverseM(entry => set.Bind(entry.Row.Row, entry.Read, key, Partitioned(entry.Row, entry.Partition)))
                    .As();
    }

    // The family's own declared Tag is the slot a partitioned probe reports under, so the key and the dimension it
    // fills are ONE declaration and a scalar row mints no dimension at all.
    static TagList Partitioned(MaterialsInstrument row, Option<string> partition) =>
        (row.Row.Tag, partition) switch {
            ({ IsSome: true, Case: string slot }, { IsSome: true, Case: string value }) =>
                InstrumentSet.Tags(TenantContext.Current, (slot, value)),
            _ => InstrumentSet.Tags(TenantContext.Current),
        };

    // Tileability components fan ONE histogram off one row table rather than three named writes: the score's own
    // column set is the dimension, so a fourth signal lands as a row here and no arm grows an arm.
    static readonly Seq<(Func<TileScore, double> Read, string Value)> ScoreComponents =
        Seq<(Func<TileScore, double>, string)>(
            (static score => score.SeamRatio, "seam"),
            (static score => score.LatticeLeak, "lattice"),
            (static score => score.Value, "value"));

    // Total generated dispatch — a new MaterialsFact case breaks this tap at compile time, so an unprojected fact
    // is a build error. Project resolves the ambient partition ONCE per fact and threads it as state: TenantContext
    // is the kernel's AsyncLocal slot, so a per-write read lets two writes of ONE fact land under two partitions
    // when a flow re-enters mid-projection.
    static Fin<Unit> Project(InstrumentSet set, MaterialsFact fact) =>
        fact.Switch<(InstrumentSet Rows, TenantContext Tenant), Fin<Unit>>(
            state: (set, TenantContext.Current),
            catalogueAdmit: static (bind, f) => bind.Rows.Write(MaterialsInstrument.CatalogueAdmits.Row, 1L,
                InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.FamilySlot, f.Row.Item.Family.Key))),
            sectionSolve: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.SectionSolves, 1L, MaterialsInstrument.SectionDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.ProfileSlot, f.Profile))),
            capacityCheck: static (bind, f) => {
                // Scope tags key both rows; adequacy rides the population alone, because a bounded ratio already
                // carries the verdict a second dimension on the histogram would only restate. The verdict tag is a
                // COPY-THEN-ADD off the shared scope — TagList is a struct, so the extension costs no heap.
                TagList scope = InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.KindSlot, f.Receipt.Kind),
                    (MaterialsInstrument.GoverningSlot, f.Verdict.Governing.Key));
                return bind.Rows.Write(MaterialsInstrument.CapacityChecks.Row, 1L,
                        Keyed(scope, MaterialsInstrument.AdequacySlot,
                            f.Verdict.Adequate ? MaterialsInstrument.Adequate : MaterialsInstrument.Inadequate))
                    // Unbounded carries no bounded ratio, so the verdict counts and records nothing; the capacity
                    // owner projects which cases hold one, so this arm never re-enumerates its case set.
                    .Bind(_ => f.Verdict.Ratio.Match(
                        Some: value => bind.Rows.Write(MaterialsInstrument.CapacityUtilisation.Row, value, scope),
                        None: static () => Fin.Succ(unit)));
            },
            graphCompile: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.GraphNodes, f.Nodes, MaterialsInstrument.GraphDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant)),
            acquisitionFit: static (bind, f) => {
                TagList method = InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.MethodSlot, f.Receipt.Method.Key));
                // Rank deficiency reads as a non-finite condition number, the Svd contract the receipt carries, so
                // the fit population stamps its own rank verdict and the [06] full-rank share partitions that series.
                return bind.Rows.Write(MaterialsInstrument.AcquisitionFits.Row, 1L,
                        Keyed(method, MaterialsInstrument.RankSlot,
                            double.IsFinite(f.Receipt.FitConditionNumber)
                                ? MaterialsInstrument.FullRank
                                : MaterialsInstrument.RankDeficient))
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.AcquisitionResidual.Row, f.Receipt.FitResidual, method));
            },
            wireMint: static (bind, f) => bind.Rows.Write(MaterialsInstrument.WireMints.Row, 1L,
                InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.MethodSlot, f.Receipt.Method))),
            projectionGate: static (bind, _) => bind.Rows.Write(MaterialsInstrument.ProjectionAdmits.Row, 1L,
                InstrumentSet.Tags(bind.Tenant)),
            // Backend keys every press row: a CPU-minted set and a GPU preview differ in what their bytes MEAN
            // (only the CPU lane is content-authoritative), so folding their throughput onto one series grades an
            // accelerator's speed as if it were the estate's own bake rate.
            texturePress: static (bind, f) => {
                // The press arm is the page's heaviest: eight writes plus two receipt-collection walks per bake.
                // One listened row admits the whole fold, so the gate skips the walks outright where nothing is
                // subscribed and never absorbs the mount refusal a write owes on a rostered row.
                if (!bind.Rows.Enabled(Seq(
                        MaterialsInstrument.PressRuns.Row, MaterialsInstrument.PressTexels.Row,
                        MaterialsInstrument.PressDuration.Row, MaterialsInstrument.PressDowngraded.Row,
                        MaterialsInstrument.PressFaulted.Row, MaterialsInstrument.PressGpuDelta.Row,
                        MaterialsInstrument.PressAgeCoverage.Row))) { return Fin.Succ(unit); }
                TagList backend = InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.BackendSlot, f.Receipt.Backend.Key));
                return bind.Rows.Write(MaterialsInstrument.PressRuns.Row, 1L, backend)
                    // Saturation rides the UNSIGNED minimum: `Math.Min` over a ulong and a long binds the double
                    // overload, so a census past 2^53 rounds before it is ever counted.
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.PressTexels.Row,
                        (long)ulong.Min(f.Receipt.Texels, (ulong)long.MaxValue), backend))
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.PressDuration.Row, f.Receipt.ElapsedMs / 1000.0, backend))
                    .Bind(_ => f.Receipt.Downgraded.TraverseM(channel =>
                        bind.Rows.Write(MaterialsInstrument.PressDowngraded.Row, 1L,
                            Keyed(backend, MaterialsInstrument.ChannelSlot, channel.Key))).As())
                    .Bind(_ => f.Receipt.Faulted.AsIterable().TraverseM(row =>
                        bind.Rows.Write(MaterialsInstrument.PressFaulted.Row,
                            (long)ulong.Min(row.Value, (ulong)long.MaxValue),
                            Keyed(backend, MaterialsInstrument.ChannelSlot, row.Key.Key))).As())
                    // GpuDeltaMax is a TYPED ABSENCE on a single-lane press, never a zero the parity gate would read
                    // as a perfect match, so presence alone admits the write.
                    .Bind(_ => f.Receipt.GpuDeltaMax.Match(
                        Some: delta => bind.Rows.Write(MaterialsInstrument.PressGpuDelta.Row, delta, backend),
                        None: static () => Fin.Succ(unit)))
                    // Aging rides the same typed-absence gate: only the Aged program mints a coverage census, and
                    // each axis writes its own share so an unexercised cavity dimension is attributable by key.
                    .Bind(_ => f.Receipt.Aging.Match(
                        Some: coverage => bind.Rows.Write(MaterialsInstrument.PressAgeCoverage.Row,
                                coverage.AgeRungsVisited / (double)coverage.AgeRungs,
                                Keyed(backend, MaterialsInstrument.AxisSlot, "age"))
                            .Bind(_ => bind.Rows.Write(MaterialsInstrument.PressAgeCoverage.Row,
                                coverage.CavityRungsVisited / (double)coverage.CavityRungs,
                                Keyed(backend, MaterialsInstrument.AxisSlot, "cavity"))),
                        None: static () => Fin.Succ(unit)));
            },
            tileSynth: static (bind, f) => {
                TagList plan = InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.StrategySlot, f.Strategy.Key), (MaterialsInstrument.ChannelSlot, f.Guide.Key));
                // Verdict recovers STRUCTURALLY off the receipt's own EVIDENCE through the stated Value()
                // collapse: the instrument verdict vocabulary is closed, so a refused band and a never-run grade
                // both tag Unmeasured here while the receipt itself keeps them apart, and the acceptance threshold
                // never lands here as a literal a caller's TilePolicy re-tuning would fork. Every run COUNTS and
                // only a measured run reaches the histogram — a zero in its place reads to a board as the worst
                // tiling in the estate rather than as no measurement. Duration measures on both halves.
                Option<TileScore> score = f.Receipt.Score.Value();
                return bind.Rows.Write(MaterialsInstrument.TileRuns.Row, 1L,
                        Keyed(plan, MaterialsInstrument.VerdictSlot,
                            score.IsSome ? MaterialsInstrument.Measured : MaterialsInstrument.Unmeasured))
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.TileDuration.Row, f.Receipt.ElapsedMs / 1000.0,
                        InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.StrategySlot, f.Strategy.Key))))
                    .Bind(_ => score.Match(
                        Some: measured => ScoreComponents.TraverseM(row =>
                            bind.Rows.Write(MaterialsInstrument.TileScoreSignal.Row, row.Read(measured),
                                Keyed(plan, MaterialsInstrument.ComponentSlot, row.Value))).As().Map(static _ => unit),
                        None: static () => Fin.Succ(unit)));
            },
            // Every grade COUNTS under its three-way verdict and every MINTED proof measures. A proof carries the
            // score it earned together with the bar it was graded against, so `Accepted` is the proof's own
            // predicate over that pair and never a threshold re-spelled here. The fact's Evidence keeps a refused
            // band apart from a never-run grade; the metric arm takes the stated Value() collapse because the
            // closed verdict vocabulary tags both as Unmeasured. The histogram takes EVERY proof: the population
            // a quality board reads is how far the estate's tilings land from the bar, which a roster admitting
            // only the passing half cannot show.
            tileGrade: static (bind, f) => {
                TagList plan = InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.StrategySlot, f.Strategy.Key));
                Option<TileProof> proof = f.Proof.Value();
                return bind.Rows.Write(MaterialsInstrument.GradeRuns.Row, 1L,
                        Keyed(plan, MaterialsInstrument.VerdictSlot, proof.Match(
                            Some: static minted => minted.Accepted ? MaterialsInstrument.Accepted : MaterialsInstrument.Rejected,
                            None: static () => MaterialsInstrument.Unmeasured)))
                    .Bind(_ => proof.Match(
                        Some: minted => bind.Rows.Write(MaterialsInstrument.GradeScore.Row, minted.Score.Value, plan),
                        None: static () => Fin.Succ(unit)));
            },
            pyramidBuild: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.PyramidLevels, f.Levels, MaterialsInstrument.PyramidDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.ChannelSlot, f.Channel.Key), (MaterialsInstrument.PolicySlot, f.Policy.Key))),
            setIngest: static (bind, f) => {
                // The refusal tally folds every unresolved stem before its first write, so the gate goes ahead of
                // the fold rather than ahead of the write alone — a vendor library dropping four hundred stems
                // costs one boolean in a process subscribed to nothing.
                if (!bind.Rows.Enabled(Seq(MaterialsInstrument.IngestStems.Row))) { return Fin.Succ(unit); }
                TagList partition = InstrumentSet.Tags(bind.Tenant);
                // Claimed stems carry the EMPTY reason the SkySlot precedent established, since a synthesized
                // "none" value mints a second vocabulary this arm keeps aligned with the refusal roster forever.
                // Refusals fold to ONE write per reason carrying its own tally, so a vendor library dropping four
                // hundred stems for one cause is four hundred on one series point.
                TagList claimed = Keyed(
                    Keyed(partition, MaterialsInstrument.VerdictSlot, MaterialsInstrument.Claimed),
                    MaterialsInstrument.ReasonSlot, string.Empty);
                return bind.Rows.Write(MaterialsInstrument.IngestStems.Row, f.Claimed, claimed)
                    .Bind(_ => f.Unresolved
                        .Fold(HashMap<IngestRefusal, long>.Empty, static (tally, row) =>
                            tally.AddOrUpdate(row.Reason, static held => held + 1L, 1L))
                        .AsIterable()
                        .TraverseM(row => bind.Rows.Write(MaterialsInstrument.IngestStems.Row, row.Value,
                            Keyed(Keyed(partition, MaterialsInstrument.VerdictSlot, MaterialsInstrument.Unresolved),
                                MaterialsInstrument.ReasonSlot, row.Key.Key))).As()
                        .Map(static _ => unit));
            },
            planeCodec: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.PlaneBytes, f.Bytes, MaterialsInstrument.CodecDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.ContainerSlot, f.Format.Key),
                    (MaterialsInstrument.DirectionSlot, f.Encoded ? MaterialsInstrument.Encode : MaterialsInstrument.Decode))),
            stageInfer: static (bind, f) => {
                // ProviderUsed, never the requested provider: the executor may refuse a policy row and degrade, and
                // a series keyed on the ASK would attribute a CPU fallback's latency to the accelerator.
                TagList lane = InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.StageSlot, f.Result.Stage.Key),
                    (MaterialsInstrument.ProviderSlot, f.Result.ProviderUsed.Key));
                // Fidelity partitions the ONE population on whether the executor honoured the requested provider:
                // a CoreML request that degraded to CPU is correct and slow, and only this dimension distinguishes
                // a healthy accelerator estate from one silently running every inference on the guaranteed floor.
                TagList admitted = Keyed(
                    Keyed(lane, MaterialsInstrument.LicenceSlot, f.Request.LicenseClass.Key),
                    MaterialsInstrument.FidelitySlot,
                    f.Result.ProviderUsed == f.Request.Provider ? MaterialsInstrument.Honoured : MaterialsInstrument.Degraded);
                return bind.Rows.Write(MaterialsInstrument.InferRuns.Row, 1L, admitted)
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.InferPartitions.Row, (long)f.Result.PartitionCount, lane))
                    // ParityFresh gates the histogram write: the golden delta is memoized per triple while the tap
                    // fires per inference, so only the run that TOOK the measurement writes it.
                    .Bind(_ => f.Result.ParityFresh
                        ? bind.Rows.Write(MaterialsInstrument.InferGolden.Row, f.Result.GoldenDelta, lane)
                        : Fin.Succ(unit));
            },
            // INGESTED domes carry no sky model, so the series keys on the empty string the environment row itself
            // publishes rather than on a synthesized "none" this arm keeps aligned.
            environmentPrefilter: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.PrefilterRuns, 1L, MaterialsInstrument.PrefilterDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.SkySlot, f.SkyModel))));

    // ONE widening over the kernel's stack-allocated tag projection: TagList is a struct, so the parameter is
    // already the caller's copy and the extension never reaches the shared base set. Without it, a per-row widening
    // inside a Traverse re-spells copy-then-Add at every call site, and materializing a heap
    // KeyValuePair<string, object?>[] beside the kernel's `in TagList` overload re-mints the one allocation the
    // projection exists to avoid.
    static TagList Keyed(TagList tags, string slot, object? value) {
        tags.Add(slot, value);
        return tags;
    }

    // FIVE arms take exactly one shape — a tag set, a whole-number census, and that run's wall duration under the
    // SAME tags — so the pair folds through one entry. The two writes share a tag set by construction here, which
    // is the property that kept drifting when each arm spelled it: an arm widening its census tags and forgetting
    // the duration published two series a board could no longer join.
    static Fin<Unit> Paired(
        InstrumentSet set, MaterialsInstrument census, long count,
        MaterialsInstrument duration, Duration elapsed, TagList tags) =>
        set.Write(census.Row, count, tags).Bind(_ => set.Write(duration.Row, elapsed.TotalSeconds, tags));
}
```

## [05]-[EVIDENCE_RECORDS]

- Owner: `MaterialsLog` — the fixed-severity generated emission grammar over the folder's banded faults and the rail's isolated evidence; `LatencyPhase` — the bracketed-construction vocabulary, each row owning BOTH of its checkpoint names; `LatencyMeasure` — the accumulated-quantity vocabulary, each row owning its accumulation law; `LatencyWrite` — the two accumulation laws those rows carry; `PhaseBracket`/`MeasureSlot` — the resolved token carriers the ledger entries accept; `MaterialsLatency` — the folder's contributed three-axis name roster and the entries that write under it.
- Cases: `LatencyWrite.Accrue` sums a quantity one request reaches many times — texels across every channel of one press, bytes across every encoded plane; `LatencyWrite.Pin` states a quantity measured once — the landed plane census a fold reports at its close. `LatencyPhase` rows are the folder's three bracketed constructions and `LatencyMeasure` rows its three quantities, each binding its own law.
- Entry: `MaterialsLog.Logged` rides `MapFail` on any Materials rail so a refusal logs once at the seam that produced it; `MaterialsLog.Drain(ILogger, Seq<IsolatedFault>)` projects a snapshot of the rail's parked evidence; `LatencyPhase.Resolve` and `LatencyMeasure.Resolve` are the ONE token-resolution pair a composition runs at boot; `MaterialsLatency.Measured(ILatencyContext, in PhaseBracket, Func<Fin<T>>)` brackets one eager construction; `Measure(ILatencyContext, in MeasureSlot, long)` folds one quantity under the slot's own law; `Attributed(ILatencyContext, TagToken, string)` stamps one pivot last-write-wins; `Sealed(ILatencyContext)` freezes the ledger and hands back its `LatencyData`.
- Auto: severity is declaration data on the attribute, so no call site chooses a level and no runtime switch over named severity verbs exists; `EventId` allocates from `FaultBand.MaterialsLogBase`, the const the kernel ledger publishes beside its own `BandKind.Event` row, and the type initializer PROVES that const against `FaultBand.MaterialsLog.Code(offset)` at load, so the two owners the dual-owner law names move as one edit or throw; the generated `IsEnabled` gate precedes payload construction; `Checkpoints` and `Measures` DERIVE from the two row families rather than restating them.
- Packages: Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new bracketed construction is one `LatencyPhase` row and its two names derive from the stem; a new accumulated quantity is one `LatencyMeasure` row naming its law; a new accumulation law is one `LatencyWrite` row; a new pivot is one entry in `Tags`; a new event family is one partial in this band with its offset added to the type-init proof.
- Boundary: this folder CONTRIBUTES a latency vocabulary and never registers one — `Checkpoints`, `Measures`, and `Tags` leave as one contributed roster the app root's single `LatencySpine.Register` fold folds beside every peer contributor's, so no package reaches `RegisterCheckpointNames` and splits the table. That registration arms `LatencyContextOptions.ThrowOnUnregisteredNames`, which makes an unregistered name a BOOT FAILURE rather than a positionless token whose writes drop unseen, so deriving both rosters from their row families is the structural half of that guarantee. Libraries take the logger and the ledger by injection and a logger-less composition binds `NullLogger.Instance`, never a nullable handle; the `Code` holes read the band ledger and the kernel `Error` projection, so a reader groups records by the same band a fault series groups by. The instrument rows at `[04]` and the records here are disjoint mandates over one refusal, never two shapes of one record; settlement records in `finally`, so failed or throwing constructions close the same bracket as successful ones. Pivots COMPOSE the `[04]` slot consts, so a ledger pivot and a metric dimension naming one axis are one string. `Freeze` seals at the composition edge after the last write and never inside a bracket a retry re-enters. Duration NEVER derives from a stamp difference — the checkpoint pair is the ledger's own elapsed.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Diagnostics.Latency;
using Microsoft.Extensions.Logging;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [TYPES] ----------------------------------------------------------------------------------
// Rows carry the ledger's two accumulation laws, each holding its own write rather than a call-site branch:
// AddMeasure sums across a request and RecordMeasure states one value absolutely, and choosing between them with a
// bool at every fold re-derives what the row already encodes.
[SmartEnum]
public sealed partial class LatencyWrite {
    public static readonly LatencyWrite Accrue = new(static (ledger, token, value) => { ledger.AddMeasure(token, value); return unit; });
    public static readonly LatencyWrite Pin = new(static (ledger, token, value) => { ledger.RecordMeasure(token, value); return unit; });

    [UseDelegateFromConstructor]
    public partial Unit Apply(ILatencyContext ledger, MeasureToken token, long value);
}

// Each phase row owns BOTH of its checkpoint names, derived from the stem so the pair cannot drift apart. Two loose
// name constants let a bracket open on the press and settle on the catalogue build with nothing raised, publishing
// an elapsed that spans two unrelated constructions and still reads measured.
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
    // a pair the vocabulary minted rather than two arguments a call site chose.
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
// phase row issued and a quantity reaches it beside the law its own row declares.
public readonly record struct PhaseBracket(CheckpointToken Started, CheckpointToken Settled);

public readonly record struct MeasureSlot(LatencyWrite Write, MeasureToken Token);

// --- [SERVICES] -----------------------------------------------------------------------------
public static partial class MaterialsLog {
    // The dual-owner invariant the [LoggerMessage] attribute forces: an EventId argument must be a compile-time
    // CONST while the band row is an instance, so the const lives beside the row at the kernel and this type
    // initializer proves every declared offset against the band's own Code fold. A drift throws at load rather than
    // publishing event ids inside a neighbour's span, and Code itself refuses an offset past the declared width.
    static MaterialsLog() {
        if (FaultBand.MaterialsLogBase != FaultBand.MaterialsLog.Code(0)
            || FaultBand.MaterialsLogBase + 1 != FaultBand.MaterialsLog.Code(1)) {
            throw new InvalidOperationException(
                $"<materials-log-band:{FaultBand.MaterialsLogBase}/{FaultBand.MaterialsLog.Key}>");
        }
    }

    // The fault crosses typed under [LogProperties], while faultCode is present only for the project Fault base.
    [LoggerMessage(EventId = FaultBand.MaterialsLogBase, EventName = "MaterialsRefused", Level = LogLevel.Warning, Message = "materials {materials.op} refused")]
    public static partial void Refused(ILogger logger, [TagName("materials.op")] Op op, int? faultCode, [LogProperties] Error fault);

    [LoggerMessage(EventId = FaultBand.MaterialsLogBase + 1, EventName = "MaterialsIsolated", Level = LogLevel.Warning, Message = "materials tap {materials.point} isolated")]
    public static partial void Isolated(ILogger logger, [TagName("materials.point")] HookId point, int? faultCode, [LogProperties] Error cause);

    extension<T>(Fin<T> step) {
        // MapFail keeps the rail intact, so the record is evidence beside the refusal rather than a second exit; a
        // generated emission returns void by the attribute's own contract, so the projection is a statement lambda.
        public Fin<T> Logged(ILogger logger, Op key) =>
            step.MapFail(error => {
                Refused(logger, key, error is Fault fault ? fault.Code : null, error);
                return error;
            });
    }

    public static Unit Drain(ILogger logger, Seq<IsolatedFault> held) =>
        ignore(held.Iter(fault => Isolated(logger, fault.Point, fault.Cause is Fault typed ? typed.Code : null, fault.Cause)));
}

public static class MaterialsLatency {
    // Both rosters DERIVE from the two row families and the [04] slot consts, so a phase or measure added to a
    // vocabulary reaches the one registration by construction. Under the app root's boot-strict fold a name this
    // folder stamps and never contributed is a composition FAILURE, so a hand-listed roster drifting behind its own
    // vocabulary is exactly the shape this derivation deletes.
    public static readonly Seq<string> Checkpoints = LatencyPhase.Names;

    public static readonly Seq<string> Measures = toSeq(LatencyMeasure.Items).Map(static row => row.Key);

    // Pivots COMPOSE the [04] slot consts: a ledger tag and a metric dimension naming one axis under two strings
    // strand every join from a slow request to the series that explains it.
    public static readonly Seq<string> Tags = Seq(
        TenantContext.TenantSlot, MaterialsInstrument.BackendSlot, MaterialsInstrument.ChannelSlot);

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

    // Seals at the composition edge: a frozen ledger refuses every later checkpoint, measure, and tag silently, so
    // freezing inside a retried bracket loses the attempt the retry exists to record.
    public static LatencyData Sealed(ILatencyContext ledger) {
        ledger.Freeze();
        return ledger.LatencyData;
    }
}
```

## [06]-[BOARD_PACK]

- Owner: `MaterialsDescriptors` — the folder's one kernel `BoardPack` value binding the panel rows and reliability objectives over the `[04]` roster.
- Entry: `MaterialsDescriptors.Pack` is the whole descriptor surface the IaC compile leg decodes under `materials.catalogue`, the provenance key the pack carries as its own first column — `Wire`, `Panels`, and `Objectives` are its columns, `Alerts` derives one `AlertSpec` per objective per burn row through the kernel fold, and `Pack.Admit(roster)` proves every panel instrument, every break key, every widget resolution, and every indicator series against the declaring port's own roster before a board compiles; the pack rides `[04]`'s contributor port outward, so the mounting root runs that proof and this folder exposes no second admission entry.
- Auto: panel and objective rows name instruments through the `[04]` roster's own rows rather than through a parallel const block, so a renamed instrument breaks this pack at compile time instead of at admission; a panel naming an instrument alone reads the kernel widget projection for that row's measurement shape, so only a deliberate reading spells a `PanelKind`; burn windows, factors, severities, and the budget share derive from the kernel table, and every objective omits its compliance window so kernel admission canonicalizes the one estate default.
- Packages: Rasm, LanguageExt.Core, NodaTime.
- Growth: a new board panel is one `PanelSpec` on the pack; a new reliability policy is one `Objective` row over an existing indicator shape, and a share over an already-fanned population needs no roster edit; a new indicator shape is a kernel `Sli` case breaking every compile leg at once.
- Boundary: dashboards, alert provisioning, tenancy, query dialects, the panel descriptor row, and the burn algebra are the kernel's and the IaC plane's — this page carries pack DATA behind the same `rasm.materials.*` names the instruments carry and never a descriptor type, query string, board JSON, or provider type. An objective binds only measures the observe rail writes on every occurrence, so a veto-refused admission stays fault-cell evidence and never a denominator; a success share is a partition over the ONE counter its verdict dimension already fans, because a good-half twin doubles the mounted series and strands its denominator on the next arm edit, and `Ratio` stays reserved for genuinely independent counters. The catalogue and library populations override to `Stat` and carry no objective, because a frozen row count reads as a figure against no ceiling; the three fault rows carry none for the same reason.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using PanelKind = Rasm.Domain.PanelKind;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class MaterialsDescriptors {
    public static readonly BoardPack Pack = new(
        // The provenance key the deploy tuple admits this projection under; pack and key are one value.
        Wire: "materials.catalogue",
        Panels: Seq(
            PanelSpec.Of("Catalogue admissions", MaterialsInstrument.CatalogueAdmits.Key, MaterialsInstrument.FamilySlot),
            PanelSpec.Of("Catalogue rows", MaterialsInstrument.CatalogueRows.Key, PanelKind.Stat),
            PanelSpec.Of("Library rows", MaterialsInstrument.LibraryRows.Key, PanelKind.Stat),
            PanelSpec.Of("Section solve rate", MaterialsInstrument.SectionSolves.Key, MaterialsInstrument.ProfileSlot),
            PanelSpec.Of("Section solve latency", MaterialsInstrument.SectionDuration.Key, MaterialsInstrument.ProfileSlot),
            PanelSpec.Of("Capacity verdicts", MaterialsInstrument.CapacityChecks.Key, MaterialsInstrument.KindSlot, MaterialsInstrument.AdequacySlot),
            PanelSpec.Of("Governing action mix", MaterialsInstrument.CapacityChecks.Key, PanelKind.Table, MaterialsInstrument.GoverningSlot),
            PanelSpec.Of("Capacity utilisation", MaterialsInstrument.CapacityUtilisation.Key, MaterialsInstrument.GoverningSlot),
            PanelSpec.Of("Graph node census", MaterialsInstrument.GraphNodes.Key),
            PanelSpec.Of("Graph compile latency", MaterialsInstrument.GraphDuration.Key),
            PanelSpec.Of("Acquisition fit residual", MaterialsInstrument.AcquisitionResidual.Key, MaterialsInstrument.MethodSlot),
            PanelSpec.Of("Fit rank by method", MaterialsInstrument.AcquisitionFits.Key, MaterialsInstrument.MethodSlot, MaterialsInstrument.RankSlot),
            PanelSpec.Of("Wire mints by method", MaterialsInstrument.WireMints.Key, MaterialsInstrument.MethodSlot),
            PanelSpec.Of("Projection admissions", MaterialsInstrument.ProjectionAdmits.Key),
            PanelSpec.Of("Press throughput", MaterialsInstrument.PressTexels.Key, MaterialsInstrument.BackendSlot),
            PanelSpec.Of("Press latency", MaterialsInstrument.PressDuration.Key, MaterialsInstrument.BackendSlot),
            // The press's two quality decisions are DIFFERENT FAILURES and each earns its own panel: a downgrade is
            // a policy fallback the plane survives, while a neutral-filled texel is output the band kernel could
            // not produce, and the faulted row is the one an operator escalates on.
            PanelSpec.Of("Press quality decisions", MaterialsInstrument.PressDowngraded.Key, PanelKind.Table, MaterialsInstrument.ChannelSlot),
            PanelSpec.Of("Press faulted texels", MaterialsInstrument.PressFaulted.Key, PanelKind.Table, MaterialsInstrument.ChannelSlot),
            PanelSpec.Of("Tile verdicts", MaterialsInstrument.TileRuns.Key, PanelKind.Table, MaterialsInstrument.StrategySlot, MaterialsInstrument.VerdictSlot),
            PanelSpec.Of("Tileability signal", MaterialsInstrument.TileScoreSignal.Key, MaterialsInstrument.ComponentSlot),
            PanelSpec.Of("Ingest classification", MaterialsInstrument.IngestStems.Key, PanelKind.Table, MaterialsInstrument.VerdictSlot, MaterialsInstrument.ReasonSlot),
            PanelSpec.Of("Plane codec volume", MaterialsInstrument.PlaneBytes.Key, MaterialsInstrument.ContainerSlot, MaterialsInstrument.DirectionSlot),
            PanelSpec.Of("Inference mix by licence", MaterialsInstrument.InferRuns.Key, PanelKind.Table, MaterialsInstrument.LicenceSlot),
            PanelSpec.Of("Inference residual", MaterialsInstrument.InferGolden.Key, MaterialsInstrument.StageSlot),
            PanelSpec.Of("Inference partitions", MaterialsInstrument.InferPartitions.Key, MaterialsInstrument.ProviderSlot),
            PanelSpec.Of("Prefilter latency", MaterialsInstrument.PrefilterDuration.Key, MaterialsInstrument.SkySlot),
            // Depth beside the ring's own eviction tallies: a shed count climbing while depth sits at the cap is the
            // tap storm a depth panel alone renders as a healthy plateau.
            PanelSpec.Of("Parked fault depth", MaterialsInstrument.Faults.Key, PanelKind.Stat),
            PanelSpec.Of("Evidence ring shedding", MaterialsInstrument.FaultsShed.Key, PanelKind.Stat),
            PanelSpec.Of("Evidence parks declined", MaterialsInstrument.FaultsLost.Key, PanelKind.Stat)),
        Objectives: Seq(
            // Every share partitions ONE mounted population on the verdict dimension its [04] arm already stamps,
            // so a denominator and its good half are one series on one write path and the good value is the axis
            // const the roster publishes rather than a literal spelled a second time here. Every row omits its
            // window, so kernel admission canonicalizes the one estate compliance default: restating that default
            // as a calendar literal reads as folder policy and survives the day the kernel moves it. Target and
            // ceiling stay folder policy — the figures this pack exists to carry.
            Objective.Create(
                name: "materials.capacity.adequate",
                sli: new Sli.Partition(
                    Metric: MaterialsInstrument.CapacityChecks.Key,
                    By: MaterialsInstrument.AdequacySlot,
                    Good: Seq(MaterialsInstrument.Adequate)),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "materials.acquisition.rank",
                sli: new Sli.Partition(
                    Metric: MaterialsInstrument.AcquisitionFits.Key,
                    By: MaterialsInstrument.RankSlot,
                    Good: Seq(MaterialsInstrument.FullRank)),
                target: 0.98d,
                window: default),
            Objective.Create(
                name: "materials.section.latency",
                sli: new Sli.Latency(Metric: MaterialsInstrument.SectionDuration.Key, Ceiling: Duration.FromMilliseconds(250), Quantile: 0.99d),
                target: 0.99d,
                window: default),
            Objective.Create(
                name: "materials.graph.latency",
                sli: new Sli.Latency(Metric: MaterialsInstrument.GraphDuration.Key, Ceiling: Duration.FromSeconds(2), Quantile: 0.95d),
                target: 0.99d,
                window: default),
            // Presses do BUILD work, so this ceiling stays generous and its target modest — a bake breaching a
            // minute at the tail is a capacity signal rather than an outage, and an aggressive objective here burns
            // budget on the 16k plane the estate deliberately admits.
            Objective.Create(
                name: "materials.texture.press.latency",
                sli: new Sli.Latency(Metric: MaterialsInstrument.PressDuration.Key, Ceiling: Duration.FromSeconds(60), Quantile: 0.95d),
                target: 0.95d,
                window: default),
            // The one texture OUTCOME the estate owns end to end is ACCEPTANCE rather than measurement, so this
            // binds the GRADE population, whose verdict dimension carries the proof's own accept predicate — a
            // share over the synthesis counter would pass every plane the gate measured and fell short on. The
            // grade population is also the wider one, since an ingested set is graded without being synthesized.
            // NO objective binds the ingest counter: a vendor library's alias coverage is a property of the
            // library, so a share over it grades a population against no ceiling this estate controls.
            Objective.Create(
                name: "materials.texture.tile",
                sli: new Sli.Partition(
                    Metric: MaterialsInstrument.GradeRuns.Key,
                    By: MaterialsInstrument.VerdictSlot,
                    Good: Seq(MaterialsInstrument.Accepted)),
                target: 0.90d,
                window: default),
            // PROVIDER FIDELITY, never latency and never the residual: an admitted result already cleared its
            // card's residual ceiling at the ingestion gate, so a residual objective grades a population that
            // cannot fail, while a fleet silently degrading every accelerator request to the CPU floor passes every
            // latency target and is exactly the regression worth alerting on.
            Objective.Create(
                name: "materials.neural.provider",
                sli: new Sli.Partition(
                    Metric: MaterialsInstrument.InferRuns.Key,
                    By: MaterialsInstrument.FidelitySlot,
                    Good: Seq(MaterialsInstrument.Honoured)),
                target: 0.90d,
                window: default)));
}
```

## [07]-[RESEARCH]

(none)
