# [MATERIALS_OBSERVABILITY]

MATERIALS signal evidence starts with the closed `MaterialsFact` family. `MaterialsPoint` closes the point roster over the kernel `IHookRoster` floor and `MaterialsHooks.Live` mints the ONE kernel `HookSet<MaterialsPoint, MaterialsFact, TelemetrySource>` this folder composes.

`MaterialsInstrument` closes the `rasm.materials.<domain>.<measure>` roster with each row CARRYING its kernel `InstrumentSpec`, and `MaterialsTap` projects the fact stream onto it as a hook subscriber. `MaterialsLog` carries the fixed-severity fault projection beside the `LatencyPhase`/`LatencyMeasure` vocabularies `MaterialsLatency` contributes, and `MaterialsDescriptors` binds the board pack over the same roster.

Settled composition draws every mechanism from the kernel signal capsule — hook set with its seats, veto folding, replay retention, detach custody, and bounded `FaultCell`; instrument set with its bucket advice, tag projection, and level cells; package-identity band; tenancy frame; SLO algebra.

Fact payloads compose Component, Appearance, Properties, and contract results. Instrument names run dotted `rasm.materials.<domain>.<measure>` with UCUM units under the `TelemetrySource.Materials` scope the composing app admits by name, every work row carrying the kernel `rasm.tenant` partition.

## [01]-[INDEX]

- [02]-[FACT_FAMILY]: `MaterialsFact` closes the evidence union and projects each case's own `MaterialsPoint` row.
- [03]-[HOOKS]: `MaterialsPoint` realizes the kernel roster floor on a `CapabilitySet<HookModality>` column and `MaterialsHooks` mints the kernel hook set over it.
- [04]-[INSTRUMENT_TAP]: `MaterialsInstrument` closes the roster, and `MaterialsTap` binds the level probes and projects the fact stream.
- [05]-[EVIDENCE_RECORDS]: `MaterialsLog` carries the fixed-severity projection, `LatencyPhase` and `LatencyMeasure` close the bracket and quantity vocabularies over their `LatencyWrite` laws, and `MaterialsLatency` derives the contributed three-axis roster from them.
- [06]-[BOARD_PACK]: `MaterialsDescriptors` binds the kernel pack over that roster.

## [02]-[FACT_FAMILY]

- Owner: `MaterialsFact` — the closed evidence union every tap fires and every projection folds, its `At` column projecting the `[03]` roster row that owns each case.
- Cases: `CatalogueAdmit` (the row a veto gate transforms or refuses pre-freeze), `SectionSolve` (profile case, solved section, wall duration), `CapacityCheck` (the lifted `CapacityLift`, the `Utilisation` verdict, wall duration), `GraphCompile` (material, ordered node count, wall duration), `AcquisitionFit` (the measured `CaptureProvenance`, wall duration), `WireMint` (material, `WireProvenance`), `ProjectionGate` (the `GraphDelta` a veto refuses or admits pre-merge), `TexturePress` (the lifted `PressRun` and the material it baked for), `TileSynth` (strategy, guide channel, and the lifted `TileRun` — the guide rides beside the run for the reason `StageInfer` carries its request: an unmeasured run still names the channel it ran against), `TileGrade` (strategy, the `Evidence<TileProof>` probe outcome the gate's `Fin` lifts through `Evidence.Of`, wall duration — its own case because a grade runs without synthesis and an ingested set earns its proof having passed no synthesizer), `PyramidBuild` (channel, mip policy, level count, texel census, fold duration — the one texture construction every press, ingest, and decode pays per channel), `SetIngest` (the claimed-stem census, the typed refusal rows, and the resolved convention), `PlaneCodec` (container row, direction, stored bytes, wall duration), `StageInfer` (the issued `StageRequest` and the lifted `StageResult` — the request rides so the tap can see a provider DEGRADATION, which the result alone cannot show), `EnvironmentPrefilter` (light key, sky model, level count, wall duration).
- Entry: each composition-root decorator fires one case through `hooks.Fire(fact.At, fact)`; veto cases fire before catalogue freeze or graph merge.
- Auto: `At` is the PRIMARY CORRESPONDENCE between this union and the `[03]` roster — the generated total `Map` breaks at compile time on a case with no row or a row with no case, so no call site names a point and the pairing cannot drift. Elapsed columns derive from one injected clock at the decorator boundary.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new evidence shape is one `MaterialsFact` case, one `MaterialsPoint` row with its `At` arm, and one projection arm at `[04]`.
- Boundary: facts carry the results the owning pages already mint — `CapacityLift`, `CaptureProvenance`, `WireProvenance`, `ComputedSection`, `PressRun`, `TileRun`, `StageResult` — and never re-derive their scalars, so a bake's texel census, backend, and elapsed millisecond come off the press's own run, a tiling run's two independent signals off the gate's own score, and an inference's provider, partition count, and reference residual off the executor's own result. `PlaneCodec` and `EnvironmentPrefilter` own no result record, so each carries the four columns its arm reads and nothing more. `SetIngest` carries the manifest's own three columns because `SetManifest` is an accumulating monoid rather than a result record, and its refusal rows cross TYPED — a formatted token keys a counter on file stems and hands the roster an unbounded dimension it cannot close.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record MaterialsFact : IHookFact<MaterialsPoint> {
    private MaterialsFact() { }

    public sealed record CatalogueAdmit(ComponentRow Row) : MaterialsFact;
    public sealed record SectionSolve(string Profile, ComputedSection Section, Duration Elapsed) : MaterialsFact;
    public sealed record CapacityCheck(CapacityLift Lift, Utilisation Verdict, Duration Elapsed) : MaterialsFact;
    public sealed record GraphCompile(MaterialId Material, int Nodes, Duration Elapsed) : MaterialsFact;
    public sealed record AcquisitionFit(CaptureProvenance Provenance, Duration Elapsed) : MaterialsFact;
    public sealed record WireMint(MaterialId Material, WireProvenance Provenance) : MaterialsFact;
    public sealed record ProjectionGate(GraphDelta Delta) : MaterialsFact;

    public sealed record TexturePress(Option<MaterialId> Material, PressRun Run) : MaterialsFact;
    public sealed record TileSynth(TileStrategy Strategy, TextureChannel Guide, TileRun Run) : MaterialsFact;
    public sealed record TileGrade(TileStrategy Strategy, Evidence<TileProof> Proof, Duration Elapsed) : MaterialsFact;
    public sealed record PyramidBuild(TextureChannel Channel, MipPolicy Policy, int Levels, long Texels, Duration Elapsed) : MaterialsFact;
    public sealed record SetIngest(int Claimed, Seq<(IngestRefusal Reason, string Detail)> Unresolved, Option<NormalConvention> Convention) : MaterialsFact;
    public sealed record PlaneCodec(RasterFormat Format, bool Encoded, long Bytes, Duration Elapsed) : MaterialsFact;
    public sealed record StageInfer(StageRequest Request, StageResult Result) : MaterialsFact;
    public sealed record EnvironmentPrefilter(string LightKey, string SkyModel, int SpecularMips, Duration Elapsed) : MaterialsFact;

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

## [03]-[HOOKS]

- Owner: `MaterialsPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.materials.<domain>.<point>`, realizing the kernel `IHookRoster<MaterialsPoint>` floor with a `CapabilitySet<HookModality>` column; `MaterialsHooks` the composition entry minting the ONE kernel `HookSet<MaterialsPoint, MaterialsFact, TelemetrySource>`. The folder mints ZERO hook mechanism — seats, veto folding, bounded replay, fork-shielded isolation, detach custody, owner-scoped release, and the bounded `FaultCell` all ride the kernel hook set.
- Cases: `rasm.materials.catalogue.admit` veto, `rasm.materials.section.solve`, `rasm.materials.capacity.check`, `rasm.materials.graph.compile`, `rasm.materials.acquisition.fit` replay, `rasm.materials.wire.mint`, `rasm.materials.projection.project` veto, `rasm.materials.texture.press`, `rasm.materials.texture.tile`, `rasm.materials.texture.grade`, `rasm.materials.texture.pyramid`, `rasm.materials.texture.ingest`, `rasm.materials.texture.codec`, `rasm.materials.neural.infer` replay, `rasm.materials.environment.prefilter`. The two replay rows settle a costly external computation whose evidence a later run re-reads rather than re-earns.
- Entry: `MaterialsHooks.Live(key, gates, taps, cell)` mints the hook set once at composition, seating one kernel point per `MaterialsPoint` row from `Items` alone; `hooks.Fire(fact.At, fact, key)` is the emitter entry and `hooks.Points` the census a `HookRegistry` freezes at the app root; `HookMounts<MaterialsPoint, TelemetrySource>` carries any rider custody a host composition claims over these seats.
- Auto: every point admits `HookModality.Observe` beside whatever else it holds, because the `[04]` projection is ONE unscoped tap over a total `Switch` and a veto-only or replay-only set refuses it; the roster's `Id` and the hook set's seats both derive from the row key, so a `Live` seat cannot re-spell either.
- Packages: Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new point is one `MaterialsPoint` row, one `MaterialsFact` case, and one `At` arm; delivery semantics are the kernel modality rows.
- Law: NAMED LOSS from composing the kernel hook set — the per-point FACT TYPE. A subscriber to a named `HookPoint<MaterialsFact.TilePress>` field took no codec fact; under one hook set every point shares `MaterialsFact` and subscribers discriminate on the case. What survives is stronger: `At` fixes the case-to-row pairing at compile time, so the guarantee moved from a field's declaration onto a generated total map. WITNESS — the fifteen `HookPoint<MaterialsFact.*>` columns, the fifteen-line `Live()`, the fifteen-entry `Points` census, and the private `Seat<TFact>` mint all delete onto `MaterialsHooks.Of`.
- Boundary: ids and modalities live on the roster rows alone, so a Materials point joins any app-tier registry census unrenamed; a subscriber fault parks as `IsolatedFault` on the composition's own bounded cell and the emitter is untouched, the ring shedding oldest-first rather than growing for process lifetime. Veto points carry observe subscribers legally and the capsule dispatches them from the admitted fact alone, so a `[04]` arm on a veto point counts admitted rows and refusal volume rides the cell. Spans are absent by design: this folder's eager constructions carry the `[05]` checkpoint ledger instead, so `Plane` is `None` on every row, no `TraceScope` derives off these ids, and `Live` binds no `IHookSpan`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

using MaterialsGate = Rasm.Domain.HookGate<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;
using MaterialsObserver = Rasm.Domain.HookTap<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;
using MaterialsHooks = Rasm.Domain.HookSet<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Materials.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
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
    public static readonly MaterialsPoint TileGrade = new("rasm.materials.texture.grade", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint PyramidBuild = new("rasm.materials.texture.pyramid", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint SetIngest = new("rasm.materials.texture.ingest", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint PlaneCodec = new("rasm.materials.texture.codec", CapabilitySet<HookModality>.Of(HookModality.Observe));
    public static readonly MaterialsPoint StageInfer = new("rasm.materials.neural.infer", CapabilitySet<HookModality>.Of(HookModality.Replay, HookModality.Observe));
    public static readonly MaterialsPoint EnvironmentPrefilter = new("rasm.materials.environment.prefilter", CapabilitySet<HookModality>.Of(HookModality.Observe));

    static readonly Lazy<FrozenDictionary<MaterialsPoint, HookId>> Ids = new(
        static () => Items.ToFrozenDictionary(static row => row, static row => HookId.Create(value: row.Key)),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public CapabilitySet<HookModality> Modalities { get; }

    public HookId Id => Ids.Value[this];

    public Option<TraceScope> Plane => Option<TraceScope>.None;
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class MaterialsHooks {
    public static Fin<MaterialsHooks> Live(Seq<MaterialsGate> gates = default, Seq<MaterialsObserver> taps = default,
        Option<FaultCell> cell = default) =>
        MaterialsHooks.Of(gates, taps, Option<IHookSpan>.None, cell);
}
```

## [04]-[INSTRUMENT_TAP]

- Owner: `MaterialsInstrument` — the closed `rasm.materials.*` roster, a `[SmartEnum<string>]` whose every row CARRIES its kernel `InstrumentSpec` (kind, measurement form, UCUM unit, kernel `Buckets` advice, the closed dimension set) beside the one dotted slot and outcome-value block both the metric writes and the `[05]` pivots spell; `MaterialsTap` — the fact-to-write projection and the level-probe binder over the `InstrumentSet` the composing root materializes.
- Cases: the roster below IS the case list, and restating each row's dimensions here publishes a second roster that drifts on the first column edit. Rows split three ways by what produces them: PUSHED rows the `[02]` fact arms write on every occurrence; the two composition-supplied POPULATION levels a frozen catalogue and its material library answer; and the three HOOK-ANSWERED rows the evidence cell probes, since a shielded subscriber failure fires no fact and so has no write site at all.
- Entry: `MaterialsInstrument.Telemetry(version)` — the one contributor port, carrying the `[06]` board pack beside these rows so board and reliability policy travel downward with the roster they name; `MaterialsTap.Tap(set)` returns the ONE unscoped `MaterialsObserver` the composing root hands `MaterialsHooks.Live`; `MaterialsTap.Levels(set, hooks, key, supplied)` binds every pulled row — the hook set's own probes from `HookLevels` and the composition's catalogue and library readers from the span — and returns the scopes that retire them.
- Auto: `Rows` DERIVES from `Items` and construction proves each row's name against its key, so the const-name roster and the hand-listed sequence that mirrored it are ONE declaration; every write addresses the row (`InstrumentSet.Write(row.Row, …)`, the kernel's own write law) rather than a name, so an unmounted row and a form mismatch surface as typed refusals; every histogram row binds its named kernel `Buckets` row as explicit-bucket advice under the base2-exponential wire default, so no bound array is spelled here; every write materializes its tag set through `InstrumentSet.Tags`, which returns the stack-allocated `TagList` the kernel's `in TagList` overload consumes and folds the ambient `TenantContext` partition in beside the arm's own slots — an arm widening a shared set does it by COPY-THEN-ADD through the page's one `Keyed` widening, never by materializing a heap `KeyValuePair<string, object?>[]`; a multi-write arm reads `Enabled` ahead of its tag mint and ahead of any per-channel walk, and an unmounted name reads enabled so the gate never absorbs the refusal a write owes; each share indicator's outcome verdict rides the same write that counts the occurrence, so a good half can never miss an occurrence its denominator recorded.
- Packages: Rasm, LanguageExt.Core, BCL inbox (`System.Diagnostics.Metrics`).
- Growth: a histogram policy change is one kernel `Buckets` row reference; a new instrument is one `MaterialsInstrument` row carrying its own UCUM unit — `{texel}`, `By`, `{partition}`, `{inference}`, `{tile}`, `{stem}`, `{channel}` — and one write in the owning `Switch` arm, a new fact case breaking the tap at compile time; a new tileability signal is one `ScoreComponents` row and no arm edit; a new hook-answered level is one `HookLevels` row and a new supplied level one `Level` row with its reader at the call site, never a signature edit.
- Law: throughput rides MONOTONE COUNTERS in UCUM units and latency rides the histograms — a bake spans four orders of magnitude between a preview and a production plane, so a bucket ladder over texel or byte volume grades nothing while the counter's own derivative is exactly the rate a board reads.
- Law: a REFUSAL is counted and never measured — an unmeasured run enters its counter's own verdict partition while every histogram gates on the evidence that proves a measurement was taken, because a sentinel admitted into a distribution reads to a board as the best value in it.
- Law: `Levels` proves the supply a BIJECTION against the roster's own pulled column minus the hook-answered rows — a name outside that set refuses, a pulled row with no reader refuses, and a name supplied twice refuses before its second bind shadows the first. NAMED LOSS from composing the kernel probe: `InstrumentSet.Bind` takes `Func<double>` and the mounted row saturates each reading into its declared carrier at collection, so the folder's own whole-number domain gate deletes and a starved reader publishes a saturated value rather than refusing at bind.
- Law: NAMED LOSS on the fault series — the kernel `FaultCell` is a bounded ring publishing `Parked`, `Shed`, and `Lost` and raising no change event, so the monotone count the hook set's evidence once pushed has no producer. One level probes total parked Materials-owned depth beside two monotone ring tallies, and the ever-parked total reads as depth summed with shed rather than as a counter nothing writes.
- Boundary: `MaterialId` and the solved `ComputedSection` stay fact evidence with no arm — material identity is identifier-grade and belongs on typed results, never on a metric series. Tenancy is the kernel `TenantContext` projection every work-row write folds, so this page holds no tenant key, no baggage read, and no zero sentinel, while the two pulled POPULATION rows stay untenanted on ownership alone: a frozen catalogue and its material library are process-scoped reference data no tenant owns, so a tenant column there declares a key no reader can emit. Every projection arm returns the kernel write result and subscribes through the hook set's shielded tap, so a refused write parks as `IsolatedFault` beside every other tap fault and no folder-local lift aspect exists. Instrument custody stays the composing app's — this spine binds and subscribes against a mounted `InstrumentSet` and mints no meter.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Diagnostics;
using LanguageExt;
using Rasm.Domain;
using Rasm.Materials.Raster;
using static LanguageExt.Prelude;

using MaterialsObserver = Rasm.Domain.HookTap<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;
using MaterialsHooks = Rasm.Domain.HookSet<Rasm.Materials.Projection.MaterialsPoint, Rasm.Materials.Projection.MaterialsFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Materials.Projection;

// --- [TABLES] --------------------------------------------------------------------------
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
    public const string ChannelSlot = "rasm.materials.texture.channel";
    public const string VerdictSlot = "rasm.materials.texture.verdict";
    public const string StrategySlot = "rasm.materials.tile.strategy";
    public const string ComponentSlot = "rasm.materials.tile.component";
    public const string PolicySlot = "rasm.materials.texture.mip.policy";
    public const string ReasonSlot = "rasm.materials.ingest.reason";
    public const string ContainerSlot = "rasm.materials.plane.container";
    public const string DirectionSlot = "rasm.materials.plane.direction";
    public const string StageSlot = "rasm.materials.neural.stage";
    public const string ProviderSlot = "rasm.materials.neural.provider";
    public const string LicenceSlot = "rasm.materials.neural.licence";
    public const string FidelitySlot = "rasm.materials.neural.fidelity";
    public const string SkySlot = "rasm.materials.environment.sky";

    public const string Adequate = "adequate";
    public const string Inadequate = "inadequate";
    public const string FullRank = "full";
    public const string RankDeficient = "deficient";
    public const string Encode = "encode";
    public const string Decode = "decode";
    public const string Honoured = "honoured";
    public const string Degraded = "degraded";
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
            "capacity checks by lift kind, governing action, and adequacy verdict",
            Seq(TenantContext.TenantSlot, KindSlot, GoverningSlot, AdequacySlot), None, None, None));

    public static readonly MaterialsInstrument CapacityUtilisation = new(
        "rasm.materials.capacity.utilisation",
        InstrumentSpec.Create("rasm.materials.capacity.utilisation", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "governing utilisation ratio per capacity check",
            Seq(TenantContext.TenantSlot, KindSlot, GoverningSlot), Some(Buckets.GoverningRatio), None, None));

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

    public static readonly MaterialsInstrument PressRuns = new(
        "rasm.materials.texture.presses",
        InstrumentSpec.Create("rasm.materials.texture.presses", InstrumentKind.Count, MeasureForm.Whole, "{press}",
            "texture presses settled by backend", Seq(TenantContext.TenantSlot, BackendSlot), None, None, None));

    public static readonly MaterialsInstrument PressTexels = new(
        "rasm.materials.texture.texels",
        InstrumentSpec.Create("rasm.materials.texture.texels", InstrumentKind.Count, MeasureForm.Whole, "{texel}",
            "texels shaded across every channel and mip level, by backend", Seq(TenantContext.TenantSlot, BackendSlot), None, None, None));

    public static readonly MaterialsInstrument PressDuration = new(
        "rasm.materials.texture.press.duration",
        InstrumentSpec.Create("rasm.materials.texture.press.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
            "texture press wall duration by backend", Seq(TenantContext.TenantSlot, BackendSlot), Some(Buckets.DecodeSeconds), None, None));

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

    public static readonly MaterialsInstrument PressGpuDelta = new(
        "rasm.materials.texture.press.gpu.delta",
        InstrumentSpec.Create("rasm.materials.texture.press.gpu.delta", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "worst per-channel CPU-versus-GPU divergence on a two-lane press, by backend",
            Seq(TenantContext.TenantSlot, BackendSlot), Some(Buckets.DivergenceRatio), None, None));

    public static readonly MaterialsInstrument PressAgeCoverage = new(
        "rasm.materials.texture.press.aging.coverage",
        InstrumentSpec.Create("rasm.materials.texture.press.aging.coverage", InstrumentKind.Distribution, MeasureForm.Real, "1",
            "ladder rungs visited over rungs declared, by backend and ladder axis",
            Seq(TenantContext.TenantSlot, BackendSlot, AxisSlot), Some(Buckets.Fractions), None, None));

    public static readonly MaterialsInstrument TileRuns = new(
        "rasm.materials.texture.tiles",
        InstrumentSpec.Create("rasm.materials.texture.tiles", InstrumentKind.Count, MeasureForm.Whole, "{tile}",
            "tiling runs settled by strategy, guide channel, and grade verdict",
            Seq(TenantContext.TenantSlot, StrategySlot, ChannelSlot, VerdictSlot), None, None, None));

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

    public static readonly MaterialsInstrument InferResidual = new(
        "rasm.materials.neural.residual",
        InstrumentSpec.Create("rasm.materials.neural.residual", InstrumentKind.Distribution, MeasureForm.Real, "1",
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

    public static readonly MaterialsInstrument Faults = new(
        "rasm.materials.faults",
        InstrumentSpec.Create("rasm.materials.faults", InstrumentKind.Levels, MeasureForm.Whole, "{fault}",
            "parked Materials tap and veto faults held on the hook set's evidence cell",
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

    public static TelemetryContributorPort Telemetry(string version) =>
        new(Scope: TelemetrySource.Materials, Version: version, Instruments: Rows, Board: MaterialsDescriptors.Pack);

    static partial void ValidateConstructorArguments(ref string key, ref InstrumentSpec row) {
        if (!string.Equals(row.Name, StringComparison.Ordinal)) {
            throw new ArgumentException($"<materials-instrument:{key}>", nameof(row));
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MaterialsTap {
    public static MaterialsObserver Tap(InstrumentSet set) =>
        new(fact => Project(set, fact));

    static readonly Seq<(MaterialsInstrument Row, Func<MaterialsHooks, Seq<(Option<string> Partition, Func<double> Read)>> Probe)> HookLevels =
        Seq<(MaterialsInstrument, Func<MaterialsHooks, Seq<(Option<string>, Func<double>)>>)>(
            (MaterialsInstrument.Faults, static hooks => Seq<(Option<string>, Func<double>)>(
                (Option<string>.None, () => hooks.Faults.Parked.Count(fault =>
                    fault.Cause is Fault typed
                    && FaultBand.OwnerOf(BandKind.Fault, typed.Code).Exists(static band => band.Owner == TelemetrySource.Materials))))),
            (MaterialsInstrument.FaultsShed, static hooks => Seq<(Option<string>, Func<double>)>(
                (Option<string>.None, () => hooks.Faults.Shed))),
            (MaterialsInstrument.FaultsLost, static hooks => Seq<(Option<string>, Func<double>)>(
                (Option<string>.None, () => hooks.Faults.Lost))));

    public static Fin<Seq<IDisposable>> Levels(
        InstrumentSet set, MaterialsHooks hooks,
        params ReadOnlySpan<(MaterialsInstrument Row, Func<double> Read)> supplied) {
        Seq<(MaterialsInstrument Row, Func<double> Read)> offered = toSeq(supplied.ToArray());
        Seq<MaterialsInstrument> owed = toSeq(MaterialsInstrument.Items)
            .Filter(static row => row.Row.Kind.Pulled && !HookLevels.Exists(probe => probe.Row.Equals(row)))
            .Strict();
        return offered.Map(static row => row.Row).Collisions(static row => row) is { IsEmpty: false } twice
            ? Fin.Fail<Seq<IDisposable>>(new KernelFault.InvalidValue(
                Label: string.Join(", ", twice.Map(static row => row.Key)),
                Requirement: "one reader per pulled roster row"))
            : owed.Count != offered.Count || owed.Exists(row => !offered.Exists(entry => entry.Row.Equals(row)))
                ? Fin.Fail<Seq<IDisposable>>(new KernelFault.InvalidValue(
                    Label: TelemetrySource.Materials.Key,
                    Requirement: "exactly one supplied reader for every composition-owned pulled roster row"))
                : (HookLevels.Bind(probe => probe.Probe(hooks).Map(fan => (probe.Row, fan.Partition, fan.Read)))
                    + offered.Map(static row => (row.Row, Partition: Option<string>.None, row.Read)))
                    .TraverseM(entry => set.Bind(entry.Row.Row, entry.Read, Partitioned(entry.Row, entry.Partition)))
                    .As();
    }

    static TagList Partitioned(MaterialsInstrument row, Option<string> partition) =>
        (row.Row.Tag, partition) switch {
            ({ IsSome: true, Case: string slot }, { IsSome: true, Case: string value }) =>
                InstrumentSet.Tags(TenantContext.Current, (slot, value)),
            _ => InstrumentSet.Tags(TenantContext.Current),
        };

    static readonly Seq<(Func<TileScore, double> Read, string Value)> ScoreComponents =
        Seq<(Func<TileScore, double>, string)>(
            (static score => score.SeamRatio, "seam"),
            (static score => score.LatticeLeak, "lattice"),
            (static score => score.Value, "value"));

    static Fin<Unit> Project(InstrumentSet set, MaterialsFact fact) =>
        fact.Switch<(InstrumentSet Rows, TenantContext Tenant), Fin<Unit>>(
            state: (set, TenantContext.Current),
            catalogueAdmit: static (bind, f) => bind.Rows.Write(MaterialsInstrument.CatalogueAdmits.Row, 1L,
                InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.FamilySlot, f.Row.Item.Family.Key))),
            sectionSolve: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.SectionSolves, 1L, MaterialsInstrument.SectionDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.ProfileSlot, f.Profile))),
            capacityCheck: static (bind, f) => {
                TagList scope = InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.KindSlot, f.Lift.Kind),
                    (MaterialsInstrument.GoverningSlot, f.Verdict.Governing.Key));
                return bind.Rows.Write(MaterialsInstrument.CapacityChecks.Row, 1L,
                        Keyed(scope, MaterialsInstrument.AdequacySlot,
                            f.Verdict.Adequate ? MaterialsInstrument.Adequate : MaterialsInstrument.Inadequate))
                    .Bind(_ => f.Verdict.Ratio
                        .TraverseM(value => bind.Rows.Write(MaterialsInstrument.CapacityUtilisation.Row, value, scope))
                        .As()
                        .Map(static _ => unit));
            },
            graphCompile: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.GraphNodes, f.Nodes, MaterialsInstrument.GraphDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant)),
            acquisitionFit: static (bind, f) => {
                TagList method = InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.MethodSlot, f.Provenance.Method.Key));
                return f.Provenance.Assessment
                    .TraverseM(assessment => assessment.Switch(
                        state: (Rows: bind.Rows, Method: method),
                        fit: static (state, fit) => state.Rows.Write(MaterialsInstrument.AcquisitionFits.Row, 1L,
                                Keyed(state.Method, MaterialsInstrument.RankSlot,
                                    fit.Rank == fit.ParameterCount
                                        ? MaterialsInstrument.FullRank
                                        : MaterialsInstrument.RankDeficient))
                            .Bind(_ => state.Rows.Write(MaterialsInstrument.AcquisitionResidual.Row, fit.Residual, state.Method)),
                        inference: static (_, _) => Fin.Succ(unit))).As()
                    .Map(static _ => unit);
            },
            wireMint: static (bind, f) => bind.Rows.Write(MaterialsInstrument.WireMints.Row, 1L,
                InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.MethodSlot, f.Provenance.Method))),
            projectionGate: static (bind, _) => bind.Rows.Write(MaterialsInstrument.ProjectionAdmits.Row, 1L,
                InstrumentSet.Tags(bind.Tenant)),
            texturePress: static (bind, f) => {
                if (!bind.Rows.Enabled(Seq(
                        MaterialsInstrument.PressRuns.Row, MaterialsInstrument.PressTexels.Row,
                        MaterialsInstrument.PressDuration.Row, MaterialsInstrument.PressDowngraded.Row,
                        MaterialsInstrument.PressFaulted.Row, MaterialsInstrument.PressGpuDelta.Row,
                        MaterialsInstrument.PressAgeCoverage.Row))) { return Fin.Succ(unit); }
                TagList backend = InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.BackendSlot, f.Run.Backend.Key));
                return bind.Rows.Write(MaterialsInstrument.PressRuns.Row, 1L, backend)
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.PressTexels.Row,
                        (long)ulong.Min(f.Run.Texels, (ulong)long.MaxValue), backend))
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.PressDuration.Row, f.Run.ElapsedMs / 1000.0, backend))
                    .Bind(_ => f.Run.Downgraded.TraverseM(channel =>
                        bind.Rows.Write(MaterialsInstrument.PressDowngraded.Row, 1L,
                            Keyed(backend, MaterialsInstrument.ChannelSlot, channel.Key))).As())
                    .Bind(_ => f.Run.Faulted.AsIterable().TraverseM(row =>
                        bind.Rows.Write(MaterialsInstrument.PressFaulted.Row,
                            (long)ulong.Min(row.Value, (ulong)long.MaxValue),
                            Keyed(backend, MaterialsInstrument.ChannelSlot, row.Key.Key))).As())
                    .Bind(_ => f.Run.GpuDeltaMax
                        .TraverseM(delta => bind.Rows.Write(MaterialsInstrument.PressGpuDelta.Row, delta, backend))
                        .As()
                        .Map(static _ => unit))
                    .Bind(_ => f.Run.Aging
                        .TraverseM(coverage => bind.Rows.Write(MaterialsInstrument.PressAgeCoverage.Row,
                                coverage.AgeRungsVisited / (double)coverage.AgeRungs,
                                Keyed(backend, MaterialsInstrument.AxisSlot, "age"))
                            .Bind(_ => bind.Rows.Write(MaterialsInstrument.PressAgeCoverage.Row,
                                coverage.CavityRungsVisited / (double)coverage.CavityRungs,
                                Keyed(backend, MaterialsInstrument.AxisSlot, "cavity"))))
                        .As()
                        .Map(static _ => unit));
            },
            tileSynth: static (bind, f) => {
                TagList plan = InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.StrategySlot, f.Strategy.Key), (MaterialsInstrument.ChannelSlot, f.Guide.Key));
                Option<TileScore> score = f.Run.Score.Value();
                return bind.Rows.Write(MaterialsInstrument.TileRuns.Row, 1L,
                        Keyed(plan, MaterialsInstrument.VerdictSlot,
                            score.IsSome ? MaterialsInstrument.Measured : MaterialsInstrument.Unmeasured))
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.TileDuration.Row, f.Run.ElapsedMs / 1000.0,
                        InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.StrategySlot, f.Strategy.Key))))
                    .Bind(_ => score.TraverseM(measured => ScoreComponents.TraverseM(row =>
                            bind.Rows.Write(MaterialsInstrument.TileScoreSignal.Row, row.Read(measured),
                                Keyed(plan, MaterialsInstrument.ComponentSlot, row.Value))).As().Map(static _ => unit))
                        .As()
                        .Map(static _ => unit));
            },
            tileGrade: static (bind, f) => {
                TagList plan = InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.StrategySlot, f.Strategy.Key));
                Option<TileProof> proof = f.Proof.Value();
                return bind.Rows.Write(MaterialsInstrument.GradeRuns.Row, 1L,
                        Keyed(plan, MaterialsInstrument.VerdictSlot, proof.Match(
                            Some: static minted => minted.Accepted ? MaterialsInstrument.Accepted : MaterialsInstrument.Rejected,
                            None: static () => MaterialsInstrument.Unmeasured)))
                    .Bind(_ => proof
                        .TraverseM(minted => bind.Rows.Write(MaterialsInstrument.GradeScore.Row, minted.Score.Value, plan))
                        .As()
                        .Map(static _ => unit));
            },
            pyramidBuild: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.PyramidLevels, f.Levels, MaterialsInstrument.PyramidDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.ChannelSlot, f.Channel.Key), (MaterialsInstrument.PolicySlot, f.Policy.Key))),
            setIngest: static (bind, f) => {
                if (!bind.Rows.Enabled(Seq(MaterialsInstrument.IngestStems.Row))) { return Fin.Succ(unit); }
                TagList partition = InstrumentSet.Tags(bind.Tenant);
                TagList claimed = Keyed(
                    Keyed(partition, MaterialsInstrument.VerdictSlot, MaterialsInstrument.Claimed),
                    MaterialsInstrument.ReasonSlot, string.Empty);
                return bind.Rows.Write(MaterialsInstrument.IngestStems.Row, f.Claimed, claimed)
                    .Bind(_ => f.Unresolved
                        .Fold(HashMap<IngestRefusal, long>(), static (tally, row) =>
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
                TagList lane = InstrumentSet.Tags(bind.Tenant,
                    (MaterialsInstrument.StageSlot, f.Result.Stage.Key),
                    (MaterialsInstrument.ProviderSlot, f.Result.ProviderUsed.Key));
                TagList admitted = Keyed(
                    Keyed(lane, MaterialsInstrument.LicenceSlot, f.Request.LicenseClass.Key),
                    MaterialsInstrument.FidelitySlot,
                    f.Result.ProviderUsed == f.Request.Provider ? MaterialsInstrument.Honoured : MaterialsInstrument.Degraded);
                return bind.Rows.Write(MaterialsInstrument.InferRuns.Row, 1L, admitted)
                    .Bind(_ => bind.Rows.Write(MaterialsInstrument.InferPartitions.Row, (long)f.Result.PartitionCount, lane))
                    .Bind(_ => f.Result.ParityFresh
                        ? bind.Rows.Write(MaterialsInstrument.InferResidual.Row, f.Result.ReferenceDelta, lane)
                        : Fin.Succ(unit));
            },
            environmentPrefilter: static (bind, f) => Paired(bind.Rows,
                MaterialsInstrument.PrefilterRuns, 1L, MaterialsInstrument.PrefilterDuration, f.Elapsed,
                InstrumentSet.Tags(bind.Tenant, (MaterialsInstrument.SkySlot, f.SkyModel))));

    static TagList Keyed(TagList tags, string slot, object? value) {
        tags.Add(slot, value);
        return tags;
    }

    static Fin<Unit> Paired(
        InstrumentSet set, MaterialsInstrument census, long count,
        MaterialsInstrument duration, Duration elapsed, TagList tags) =>
        set.Write(census.Row, count, tags).Bind(_ => set.Write(duration.Row, elapsed.TotalSeconds, tags));
}
```

## [05]-[EVIDENCE_RECORDS]

- Owner: `MaterialsLog` — the fixed-severity generated emission grammar over the folder's banded faults and the hook set's isolated evidence; `LatencyPhase` — the bracketed-construction vocabulary, each row owning BOTH of its checkpoint names; `LatencyMeasure` — the accumulated-quantity vocabulary, each row owning its accumulation law; `LatencyWrite` — the two accumulation laws those rows carry; `PhaseBracket`/`MeasureSlot` — the resolved token carriers the ledger entries accept; `MaterialsLatency` — the folder's contributed three-axis name roster and the entries that write under it.
- Cases: `LatencyWrite.Accrue` sums a quantity one request reaches many times — texels across every channel of one press, bytes across every encoded plane; `LatencyWrite.Pin` states a quantity measured once — the landed plane census a fold reports at its close. `LatencyPhase` rows are the folder's three bracketed constructions and `LatencyMeasure` rows its three quantities, each binding its own law.
- Entry: `MaterialsLog.Logged` rides `MapFail` on any Materials result so a refusal logs once at the boundary that produced it; `MaterialsLog.Drain(ILogger, Seq<IsolatedFault>)` projects a snapshot of the hook set's parked evidence; `LatencyPhase.Resolve` and `LatencyMeasure.Resolve` are the ONE token-resolution pair a composition runs at boot; `MaterialsLatency.Measured(ILatencyContext, in PhaseBracket, Func<Fin<T>>)` brackets one eager construction; `Measure(ILatencyContext, in MeasureSlot, long)` folds one quantity under the slot's own law; `Attributed(ILatencyContext, TagToken, string)` stamps one pivot last-write-wins; `Sealed(ILatencyContext)` freezes the ledger and hands back its `LatencyData`.
- Auto: severity is declaration data on the attribute, so no call site chooses a level and no runtime switch over named severity verbs exists; `EventId` allocates from `FaultBand.MaterialsLogBase`, the const the kernel ledger publishes beside its own `BandKind.Event` row, and the type initializer PROVES that const against `FaultBand.MaterialsLog.Code(offset)` at load, so the two owners the dual-owner law names move as one edit or throw; the generated `IsEnabled` gate precedes payload construction; `Checkpoints` and `Measures` DERIVE from the two row families rather than restating them.
- Packages: Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new bracketed construction is one `LatencyPhase` row and its two names derive from the stem; a new accumulated quantity is one `LatencyMeasure` row naming its law; a new accumulation law is one `LatencyWrite` row; a new pivot is one entry in `Tags`; a new event family is one partial in this band with its offset added to the type-init proof.
- Boundary: this folder CONTRIBUTES a latency vocabulary and never registers one — `Checkpoints`, `Measures`, and `Tags` leave as one contributed roster the app root's single `LatencySpine.Register` fold folds beside every peer contributor's, so no package reaches `RegisterCheckpointNames` and splits the table. That registration arms `LatencyContextOptions.ThrowOnUnregisteredNames`, which makes an unregistered name a BOOT FAILURE rather than a positionless token whose writes drop unseen, so deriving both rosters from their row families is the structural half of that guarantee. Libraries take the logger and the ledger by injection and a logger-less composition binds `NullLogger.Instance`, never a nullable handle; the `Code` holes read the band ledger and the kernel `Error` projection, so a reader groups records by the same band a fault series groups by. The instrument rows at `[04]` and the records here are disjoint mandates over one refusal, never two shapes of one record; settlement records in `finally`, so failed or throwing constructions close the same bracket as successful ones. Pivots COMPOSE the `[04]` slot consts, so a ledger pivot and a metric dimension naming one axis are one string. `Freeze` seals at the composition edge after the last write and never inside a bracket a retry re-enters. Duration NEVER derives from a stamp difference — the checkpoint pair is the ledger's own elapsed.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Diagnostics.Latency;
using Microsoft.Extensions.Logging;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LatencyWrite {
    public static readonly LatencyWrite Accrue = new(static (ledger, token, value) => { ledger.AddMeasure(token, value); return unit; });
    public static readonly LatencyWrite Pin = new(static (ledger, token, value) => { ledger.RecordMeasure(token, value); return unit; });

    [UseDelegateFromConstructor]
    public partial Unit Apply(ILatencyContext ledger, MeasureToken token, long value);
}

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

    public PhaseBracket Resolve(ILatencyContextTokenIssuer issuer) =>
        new(issuer.GetCheckpointToken(Started), issuer.GetCheckpointToken(Settled));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LatencyMeasure {
    public static readonly LatencyMeasure TexelsShaded = new("rasm.materials.texture.texels.shaded", LatencyWrite.Accrue);
    public static readonly LatencyMeasure PlaneBytesEncoded = new("rasm.materials.texture.plane.bytes.encoded", LatencyWrite.Accrue);
    public static readonly LatencyMeasure PlanesLanded = new("rasm.materials.texture.planes.landed", LatencyWrite.Pin);

    public LatencyWrite Write { get; }

    public MeasureSlot Resolve(ILatencyContextTokenIssuer issuer) => new(Write, issuer.GetMeasureToken(Key));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct PhaseBracket(CheckpointToken Started, CheckpointToken Settled);

public readonly record struct MeasureSlot(LatencyWrite Write, MeasureToken Token);

// --- [SERVICES] ------------------------------------------------------------------------
public static partial class MaterialsLog {
    static MaterialsLog() {
        if (FaultBand.MaterialsLogBase != FaultBand.MaterialsLog.Code(0)
            || FaultBand.MaterialsLogBase + 1 != FaultBand.MaterialsLog.Code(1)) {
            throw new InvalidOperationException(
                $"<materials-log-band:{FaultBand.MaterialsLogBase}/{FaultBand.MaterialsLog.Key}>");
        }
    }

    [LoggerMessage(EventId = FaultBand.MaterialsLogBase, EventName = "MaterialsRefused", Level = LogLevel.Warning, Message = "materials {materials.op} refused")]
    public static partial void Refused(ILogger logger, [TagName("materials.op")] , int? faultCode, [LogProperties] Error fault);

    [LoggerMessage(EventId = FaultBand.MaterialsLogBase + 1, EventName = "MaterialsIsolated", Level = LogLevel.Warning, Message = "materials tap {materials.point} isolated")]
    public static partial void Isolated(ILogger logger, [TagName("materials.point")] HookId point, int? faultCode, [LogProperties] Error cause);

    extension<T>(Fin<T> step) {
        public Fin<T> Logged(ILogger logger) =>
            step.MapFail(error => {
                Refused(logger, error is Fault fault ? fault.Code : null, error);
                return error;
            });
    }

    public static Unit Drain(ILogger logger, Seq<IsolatedFault> held) =>
        ignore(held.Iter(fault => Isolated(logger, fault.Point, fault.Cause is Fault typed ? typed.Code : null, fault.Cause)));
}

public static class MaterialsLatency {
    public static readonly Seq<string> Checkpoints = LatencyPhase.Names;

    public static readonly Seq<string> Measures = toSeq(LatencyMeasure.Items).Map(static row => row.Key);

    public static readonly Seq<string> Tags = Seq(
        TenantContext.TenantSlot, MaterialsInstrument.BackendSlot, MaterialsInstrument.ChannelSlot);

    public static Fin<T> Measured<T>(ILatencyContext ledger, in PhaseBracket phase, Func<Fin<T>> body) {
        ledger.AddCheckpoint(phase.Started);
        try { return body(); }
        finally { ledger.AddCheckpoint(phase.Settled); }
    }

    public static Unit Measure(ILatencyContext ledger, in MeasureSlot slot, long value) =>
        slot.Write.Apply(ledger, slot.Token, value);

    public static Unit Attributed(ILatencyContext ledger, TagToken token, string value) {
        ledger.SetTag(token, value);
        return unit;
    }

    public static LatencyData Sealed(ILatencyContext ledger) {
        ledger.Freeze();
        return ledger.LatencyData;
    }
}
```

## [06]-[BOARD_PACK]

- Owner: `MaterialsDescriptors` — the folder's one kernel `BoardPack` value binding the panel rows and reliability objectives over the `[04]` roster.
- Entry: `MaterialsDescriptors.Pack` is the whole descriptor surface the IaC compile leg decodes under `materials.catalogue`, the provenance key the pack carries as its own first column — `Wire`, `Panels`, and `Objectives` are its columns, `Alerts` derives one `AlertSpec` per objective per burn row through the kernel fold, and `Pack.Admit(roster)` proves every panel instrument, every break key, every widget resolution, and every indicator series against the declaring port's own roster before a board compiles; the pack rides `[04]`'s contributor port outward, so the mounting root runs that proof and this folder exposes no second admission entry.
- Auto: panel and objective rows name instruments through the `[04]` roster's own rows rather than through a parallel const block, so a renamed instrument breaks this pack at compile time instead of at admission; a panel naming an instrument alone reads the kernel widget projection for that row's measurement shape, so only a deliberate reading spells a `PanelKind`; burn windows, factors, severities, and the budget share derive from the kernel table, and every objective omits its compliance window so kernel admission canonicalizes the one repo default.
- Packages: Rasm, LanguageExt.Core, NodaTime.
- Growth: a new board panel is one `PanelSpec` on the pack; a new reliability policy is one `Objective` row over an existing indicator shape, and a share over an already-fanned population needs no roster edit; a new indicator shape is a kernel `Sli` case breaking every compile leg at once.
- Boundary: dashboards, alert provisioning, tenancy, query dialects, the panel descriptor row, and the burn algebra are the kernel's and the IaC plane's — this page carries pack DATA behind the same `rasm.materials.*` names the instruments carry and never a descriptor type, query string, board JSON, or provider type. An objective binds only measures the observe hook set writes on every occurrence, so a veto-refused admission stays fault-cell evidence and never a denominator; a success share is a partition over the ONE counter its verdict dimension already fans, because a good-half twin doubles the mounted series and strands its denominator on the next arm edit, and `Ratio` stays reserved for genuinely independent counters. The catalogue and library populations override to `Stat` and carry no objective, because a frozen row count reads as a figure against no ceiling; the three fault rows carry none for the same reason.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using PanelKind = Rasm.Domain.PanelKind;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MaterialsDescriptors {
    public static readonly BoardPack Pack = new(
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
            PanelSpec.Of("Press quality decisions", MaterialsInstrument.PressDowngraded.Key, PanelKind.Table, MaterialsInstrument.ChannelSlot),
            PanelSpec.Of("Press faulted texels", MaterialsInstrument.PressFaulted.Key, PanelKind.Table, MaterialsInstrument.ChannelSlot),
            PanelSpec.Of("Tile verdicts", MaterialsInstrument.TileRuns.Key, PanelKind.Table, MaterialsInstrument.StrategySlot, MaterialsInstrument.VerdictSlot),
            PanelSpec.Of("Tileability signal", MaterialsInstrument.TileScoreSignal.Key, MaterialsInstrument.ComponentSlot),
            PanelSpec.Of("Ingest classification", MaterialsInstrument.IngestStems.Key, PanelKind.Table, MaterialsInstrument.VerdictSlot, MaterialsInstrument.ReasonSlot),
            PanelSpec.Of("Plane codec volume", MaterialsInstrument.PlaneBytes.Key, MaterialsInstrument.ContainerSlot, MaterialsInstrument.DirectionSlot),
            PanelSpec.Of("Inference mix by licence", MaterialsInstrument.InferRuns.Key, PanelKind.Table, MaterialsInstrument.LicenceSlot),
            PanelSpec.Of("Inference residual", MaterialsInstrument.InferResidual.Key, MaterialsInstrument.StageSlot),
            PanelSpec.Of("Inference partitions", MaterialsInstrument.InferPartitions.Key, MaterialsInstrument.ProviderSlot),
            PanelSpec.Of("Prefilter latency", MaterialsInstrument.PrefilterDuration.Key, MaterialsInstrument.SkySlot),
            PanelSpec.Of("Parked fault depth", MaterialsInstrument.Faults.Key, PanelKind.Stat),
            PanelSpec.Of("Evidence ring shedding", MaterialsInstrument.FaultsShed.Key, PanelKind.Stat),
            PanelSpec.Of("Evidence parks declined", MaterialsInstrument.FaultsLost.Key, PanelKind.Stat)),
        Objectives: Seq(
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
            Objective.Create(
                name: "materials.texture.press.latency",
                sli: new Sli.Latency(Metric: MaterialsInstrument.PressDuration.Key, Ceiling: Duration.FromSeconds(60), Quantile: 0.95d),
                target: 0.95d,
                window: default),
            Objective.Create(
                name: "materials.texture.tile",
                sli: new Sli.Partition(
                    Metric: MaterialsInstrument.GradeRuns.Key,
                    By: MaterialsInstrument.VerdictSlot,
                    Good: Seq(MaterialsInstrument.Accepted)),
                target: 0.90d,
                window: default),
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
