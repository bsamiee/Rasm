# [MATERIALS_OBSERVABILITY]

MATERIALS signal evidence starts with the closed `MaterialsFact` family: `MaterialsHooks` composes the kernel signal capsule into the folder's point rail, `MaterialsInstruments` projects the fact stream onto `rasm.materials.<domain>.<measure>` instruments as a rail subscriber, `MaterialsLog` and `MaterialsLatency` carry the fixed-severity fault projection and the checkpoint ledger, and `MaterialsDescriptors` binds the folder's board pack over that roster.

Settled composition draws every mechanism from the kernel signal capsule — hook capsule, instrument capsule with its bucket advice and level cells, package-identity band, tenancy frame, and SLO algebra with its board-pack carrier; each fence prelude names the exact rows it binds, and fact payloads compose Component, Appearance, Properties, and seam receipts. Instrument names run dotted `rasm.materials.<domain>.<measure>` with UCUM units under the `TelemetrySource.Materials` scope the composing app admits by name, every work row carrying the kernel `rasm.tenant` partition.

## [01]-[INDEX]

- [02]-[FACT_FAMILY]: `MaterialsFact` closes the evidence union.
- [03]-[HOOK_RAIL]: `MaterialsPoint` closes the point vocabulary on its kernel `Modality` column and `MaterialsHooks` composes that roster over the kernel capsule.
- [04]-[INSTRUMENT_TAP]: `MaterialsInstruments` mounts the roster, the level bindings, the contributor port, and the rail projection.
- [05]-[EVIDENCE_RECORDS]: `MaterialsLog` and `MaterialsLatency` carry the fixed-severity projection and the checkpoint ledger.
- [06]-[BOARD_PACK]: `MaterialsDescriptors` binds the kernel pack over that roster.

## [02]-[FACT_FAMILY]

- Owner: `MaterialsFact` — the closed evidence union every tap fires and every projection folds.
- Cases: `CatalogueAdmit` (the row a veto gate may transform or refuse pre-freeze), `SectionSolve` (profile case, solved section, wall duration), `CapacityCheck` (the lifted `CapacityReceipt`, the `Utilisation` verdict, wall duration), `GraphCompile` (material, ordered node count, wall duration), `AcquisitionFit` (the measured `Provenance` receipt, wall duration), `WireMint` (material, `WireProvenance` receipt), `ProjectionGate` (the `GraphDelta` a veto may refuse pre-merge), `TexturePress` (the lifted `PressReceipt` and the material it baked for), `PlaneCodec` (container row, direction, stored bytes, wall duration), `StageInfer` (the issued `StageRequest`, the lifted `StageResult`, and the card's licence class — the request rides so the tap can see a provider DEGRADATION, which the result alone cannot show), `EnvironmentPrefilter` (light key, sky model, level count, wall duration).
- Entry: each composition-root decorator fires one case after the owning entrypoint settles; veto cases fire before catalogue freeze or graph merge.
- Auto: elapsed columns derive from one injected clock at the decorator boundary.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new evidence shape is one `MaterialsFact` case, one point row at `[03]`, and one projection arm at `[04]`.
- Boundary: facts carry receipts the owning pages already mint — `CapacityReceipt`, `Provenance`, `WireProvenance`, `ComputedSection`, `PressReceipt`, `StageResult` — and never re-derive their scalars, so a bake's texel census, backend, and elapsed millisecond come off the press's own receipt and an inference's provider, partition count, and golden residual off the executor's own result rather than off a second measurement this tap keeps honest. `PlaneCodec` and `EnvironmentPrefilter` own no receipt, so each carries the four columns its arm reads and nothing more.

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
using Rasm.Materials.Raster;                    // PressReceipt, RasterFormat — the bake and container receipts the texture facts lift
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
    public sealed record PlaneCodec(Op Key, RasterFormat Format, bool Encoded, long Bytes, Duration Elapsed) : MaterialsFact;
    public sealed record StageInfer(Op Key, StageRequest Request, StageResult Result, LicenseClass License) : MaterialsFact;
    public sealed record EnvironmentPrefilter(Op Key, string LightKey, string SkyModel, int SpecularMips, Duration Elapsed) : MaterialsFact;
}
```

## [03]-[HOOK_RAIL]

- Owner: `MaterialsPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.materials.<domain>.<point>` with the kernel `HookModality` column; `MaterialsHooks` the per-composition point roster composing the kernel capsule — one `HookPoint<TFact>` field per row, one shared `IsolatedFault` evidence cell, no process-global registry, since Materials holds no plugin-identity grant custody; every declared point carries a projection arm at `[04]`, so a point firing into nothing has no landing.
- Cases: point roster rows — `rasm.materials.catalogue.admit` veto (`CatalogueAdmit`), `rasm.materials.section.solve` observe (`SectionSolve`), `rasm.materials.capacity.check` observe (`CapacityCheck`), `rasm.materials.graph.compile` observe (`GraphCompile`), `rasm.materials.acquisition.fit` replay (`AcquisitionFit`), `rasm.materials.wire.mint` observe (`WireMint`), `rasm.materials.projection.project` veto (`ProjectionGate`), `rasm.materials.texture.press` observe (`TexturePress`), `rasm.materials.texture.codec` observe (`PlaneCodec`), `rasm.materials.neural.infer` replay (`StageInfer`), `rasm.materials.environment.prefilter` observe (`EnvironmentPrefilter`). `rasm.materials.neural.infer` takes REPLAY for the reason `rasm.materials.acquisition.fit` does: both settle a costly external computation whose evidence a later run re-reads rather than re-earns.
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
            Seat<MaterialsFact.PlaneCodec>(MaterialsPoint.PlaneCodec, faults),
            Seat<MaterialsFact.StageInfer>(MaterialsPoint.StageInfer, faults),
            Seat<MaterialsFact.EnvironmentPrefilter>(MaterialsPoint.EnvironmentPrefilter, faults),
            faults);
    }

    // Mount table the app root audits every registered point through, folded into the one frozen `HookRegistry`
    // beside every sibling roster — duplicate ids across the whole composition are structurally fatal there.
    public Seq<IHookPoint> Points => Seq<IHookPoint>(
        CatalogueAdmit, SectionSolve, CapacityCheck, GraphCompile, AcquisitionFit, WireMint, ProjectionGate,
        TexturePress, PlaneCodec, StageInfer, EnvironmentPrefilter);

    private static HookPoint<TFact> Seat<TFact>(MaterialsPoint row, Atom<Seq<IsolatedFault>> faults) =>
        new(id: HookId.Create(value: row.Key), modality: row.Modality, faults: faults);
}
```

## [04]-[INSTRUMENT_TAP]

- Owner: `MaterialsInstruments` — the `rasm.materials.*` `InstrumentSpec` roster, the contributor port, and the rail-subscribed projection; the roster is composition-free data, so one declaration binds against any meter and any cells.
- Cases: catalogue admissions by family off `CatalogueAdmit`; solve counts and duration off `SectionSolve`; capacity checks by adequacy verdict and governing utilisation off `CapacityCheck`; compile node census and duration off `GraphCompile`; fits by parameter-rank verdict and the residual off `AcquisitionFit`; wire mints by capture method off `WireMint`; projection admissions off `ProjectionGate`; catalogue and library row levels off composition-bound readers; fault counts off the rail's `IsolatedFault` cell banded by kernel category.
- Entry: `MaterialsInstruments.Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl)` — the one contributor port, carrying the `[06]` board pack beside these rows so board and reliability policy travel downward with the roster they name; `MaterialsInstruments.Tap(MaterialsHooks hooks, InstrumentSet set, params ReadOnlySpan<(string Name, Func<double> Read)> levels)` binds the pulled readers and mounts the observe subscriptions at composition, so create and write calls live only inside this spine.
- Auto: every histogram row binds its named kernel `Buckets` row as the explicit-bucket fallback under the base2-exponential wire default, so no bound array is spelled here; every dimension key is a declared slot const carried on its own row's `Dimensions` column, so the governance leg derives view tag keys from the mounted roster rather than a second table; every write materializes its tag set through the kernel `InstrumentSet.Tags` entry, which folds the ambient `TenantContext` partition in beside the arm's own slots, so a partitioned host attributes every work row and a single-tenant one mints no dimension at all; each share indicator's outcome verdict rides the same write that counts the occurrence, so a good half can never miss an occurrence its denominator recorded and no second counter carries the numerator; a supplied reader binds through its own row's declared `MeasureForm`, so one supply shape serves a whole-count population and a real-valued level.
- Packages: Rasm, LanguageExt.Core, BCL inbox (`System.Diagnostics.Metrics`).
- Growth: a histogram policy change is one kernel `Buckets` row reference; a new instrument is one `InstrumentSpec` row and one tap arm carrying its own UCUM unit string — `{texel}`, `By`, `{partition}`, `{inference}` — so a magnitude states what it counts and a board never infers a scale from an instrument name; a new pulled level is one `Level` row and one reader at the call site, never a signature edit.
- Boundary: throughput rides MONOTONE COUNTERS in UCUM units and latency rides the histograms — a bake spans four orders of magnitude between a preview and a production plane, so a bucket ladder over texel or byte volume grades nothing while the counter's own derivative is exactly the rate a board reads. `MaterialId` and the solved `ComputedSection` stay fact evidence with no arm — material identity is identifier-grade and belongs on spans and typed receipts, never on a metric series, and a solved section's column set is receipt truth the owning page already mints; tenancy is the kernel `TenantContext` projection every work-row write folds and every work row declares, so this page holds no tenant key, no baggage read, and no zero sentinel, while the two pulled population rows stay untenanted because a scalar level carries no call-site tag and a frozen catalogue is process-scoped reference data no tenant owns; every projection arm returns the kernel write rail and subscribes through the capsule's rail-shaped `Observe`, so a refused write parks as `IsolatedFault` beside every other tap fault and no folder-local lift aspect exists; level readers are composition-supplied and bound through the kernel `LevelCells`, so app-scoped isolation holds by construction; `Tap` proves the supply a bijection against the roster's own `Pulled` column — a name outside the pulled rows refuses, a pulled row with no reader refuses, and a name supplied twice refuses before its second bind shadows the first — so a population gauge reads the live catalogue and a cell nobody writes has no construction path; a whole-number level crosses the domain gate its cell declares, so a non-finite or out-of-range reading refuses rather than casting to an undefined value the series carries as a population; live facts and replay envelopes remain mutually exclusive evidence paths at the composition root; instrument custody stays the composing app's — this spine binds and subscribes against a mounted `InstrumentSet` and mints no meter, so the fan that materializes the port is the one creation site.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                              // Buckets, Fault, HookDetacher, HookPoint, InstrumentSet,
                                                // InstrumentSpec, IsolatedFault, KernelInstruments, MeasureForm,
                                                // TelemetryContributorPort, TelemetryIdentity, TelemetrySource,
                                                // TenantContext, FaultExtensions extension property Category
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
    // process. The two POPULATION rows carry none — a scalar pulled level is read at collection cadence with no
    // call-site tag, and a frozen catalogue and its material library are process-scoped reference data no
    // tenant owns, so a tenant column there would declare a key no reader can ever emit.
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
        InstrumentSpec.Advised(PressDuration, "s", "texture press wall duration by backend", MeasureForm.Real, Buckets.CompileSeconds, TenantContext.TenantSlot, BackendSlot),
        InstrumentSpec.Count(PlaneBytes, "By", "plane bytes encoded or decoded by container and direction", MeasureForm.Whole, TenantContext.TenantSlot, ContainerSlot, DirectionSlot),
        InstrumentSpec.Advised(CodecDuration, "s", "plane codec wall duration by container and direction", MeasureForm.Real, Buckets.SolveSeconds, TenantContext.TenantSlot, ContainerSlot, DirectionSlot),
        // Licence rides the inference POPULATION because a fleet operator's first question about a model estate is
        // which grant class its running inferences fall under — a research-class row appearing in production is a
        // posture change no duration or residual would ever surface.
        InstrumentSpec.Count(InferRuns, "{inference}", "photo-to-PBR inferences settled by stage, provider, licence class, and provider fidelity", MeasureForm.Whole, TenantContext.TenantSlot, StageSlot, ProviderSlot, LicenceSlot, FidelitySlot),
        InstrumentSpec.Advised(InferPartitions, "{partition}", "ONNX graph partitions reached per inference by stage and provider", MeasureForm.Whole, Buckets.GraphCounts, TenantContext.TenantSlot, StageSlot, ProviderSlot),
        InstrumentSpec.Advised(InferGolden, "1", "inference residual against the model's CPU-reference output by stage and provider", MeasureForm.Real, Buckets.ResidualDecades, TenantContext.TenantSlot, StageSlot, ProviderSlot),
        InstrumentSpec.Count(PrefilterRuns, "{prefilter}", "IBL prefilters settled by sky model", MeasureForm.Whole, TenantContext.TenantSlot, SkySlot),
        InstrumentSpec.Advised(PrefilterDuration, "s", "IBL prefilter wall duration by sky model", MeasureForm.Real, Buckets.CompileSeconds, TenantContext.TenantSlot, SkySlot),
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
    static Seq<IDisposable> Mounted(MaterialsHooks hooks, InstrumentSet set) {
        // Re-parking a refused fault-count write on the rail cell would re-enter this handler, so the refusal is
        // discarded here and nowhere else on the page.
        AtomChangedEvent<Seq<IsolatedFault>> rejected = held => held.Last.Iter(fault =>
            ignore(set.Write(Faults, 1L,
                InstrumentSet.Tags(TenantContext.Current, (KernelInstruments.CategorySlot, fault.Cause.Category)))));
        hooks.Faults.Change += rejected;
        return Seq<IDisposable>(
            hooks.CatalogueAdmit.Observe(fact =>
                set.Write(CatalogueAdmits, 1L, InstrumentSet.Tags(TenantContext.Current, (FamilySlot, fact.Row.Item.Family.Key)))),
            hooks.SectionSolve.Observe(fact => {
                KeyValuePair<string, object?>[] profile = InstrumentSet.Tags(TenantContext.Current, (ProfileSlot, fact.Profile));
                return set.Write(SectionSolves, 1L, profile)
                    .Bind(_ => set.Write(SectionDuration, fact.Elapsed.TotalSeconds, profile));
            }),
            hooks.CapacityCheck.Observe(fact => {
                // Scope tags key both rows; adequacy rides the population alone, because a bounded ratio already
                // carries the verdict a second dimension on the histogram would only restate.
                KeyValuePair<string, object?>[] scope = InstrumentSet.Tags(TenantContext.Current,
                    (KindSlot, fact.Receipt.Kind), (GoverningSlot, fact.Verdict.Governing.Key));
                return set.Write(CapacityChecks, 1L, [.. scope, new(AdequacySlot, fact.Verdict.Adequate ? Adequate : Inadequate)])
                    // Overcapacity carries no bounded ratio, so the verdict counts and records nothing; the capacity
                    // owner projects which cases hold one, so this arm never re-enumerates its case set.
                    .Bind(_ => fact.Verdict.Ratio.Match(
                        Some: value => set.Write(CapacityUtilisation, value, scope),
                        None: static () => Fin.Succ(unit)));
            }),
            hooks.GraphCompile.Observe(fact => {
                KeyValuePair<string, object?>[] partition = InstrumentSet.Tags(TenantContext.Current);
                return set.Write(GraphNodes, (long)fact.Nodes, partition)
                    .Bind(_ => set.Write(GraphDuration, fact.Elapsed.TotalSeconds, partition));
            }),
            hooks.AcquisitionFit.Observe(fact => {
                KeyValuePair<string, object?>[] method = InstrumentSet.Tags(TenantContext.Current, (MethodSlot, fact.Receipt.Method.Key));
                // Rank deficiency reads as a non-finite condition number, the Svd contract the receipt carries, so the
                // fit population stamps its own rank verdict and the [06] full-rank share partitions that one series.
                return set.Write(AcquisitionFits, 1L,
                        [.. method, new(RankSlot, double.IsFinite(fact.Receipt.FitConditionNumber) ? FullRank : RankDeficient)])
                    .Bind(_ => set.Write(AcquisitionResidual, fact.Receipt.FitResidual, method));
            }),
            hooks.WireMint.Observe(fact =>
                set.Write(WireMints, 1L, InstrumentSet.Tags(TenantContext.Current, (MethodSlot, fact.Receipt.Method)))),
            hooks.ProjectionGate.Observe(_ => set.Write(ProjectionAdmits, 1L, InstrumentSet.Tags(TenantContext.Current))),
            // Backend keys all three press rows: a CPU-minted set and a GPU preview differ in what their bytes MEAN
            // (only the CPU lane is content-authoritative), so folding their throughput onto one series would grade
            // an accelerator's speed as if it were the estate's own bake rate.
            hooks.TexturePress.Observe(fact => {
                KeyValuePair<string, object?>[] backend = InstrumentSet.Tags(TenantContext.Current, (BackendSlot, fact.Receipt.Backend.Key));
                return set.Write(PressRuns, 1L, backend)
                    .Bind(_ => set.Write(PressTexels, (long)Math.Min(fact.Receipt.Texels, long.MaxValue), backend))
                    .Bind(_ => set.Write(PressDuration, fact.Receipt.ElapsedMs / 1000.0, backend));
            }),
            hooks.PlaneCodec.Observe(fact => {
                KeyValuePair<string, object?>[] container = InstrumentSet.Tags(TenantContext.Current,
                    (ContainerSlot, fact.Format.Key), (DirectionSlot, fact.Encoded ? Encode : Decode));
                return set.Write(PlaneBytes, fact.Bytes, container)
                    .Bind(_ => set.Write(CodecDuration, fact.Elapsed.TotalSeconds, container));
            }),
            hooks.StageInfer.Observe(fact => {
                // ProviderUsed, never the requested provider: the executor may refuse a policy row and degrade, and
                // a series keyed on the ASK would attribute a CPU fallback's latency to the accelerator.
                KeyValuePair<string, object?>[] lane = InstrumentSet.Tags(TenantContext.Current,
                    (StageSlot, fact.Result.Stage.Key), (ProviderSlot, fact.Result.ProviderUsed.Key));
                // Fidelity partitions the ONE population on whether the executor honoured the requested provider:
                // a CoreML request that degraded to CPU is correct and slow, and only this dimension distinguishes
                // a healthy accelerator estate from one silently running every inference on the guaranteed floor.
                return set.Write(InferRuns, 1L, [.. lane,
                        new(LicenceSlot, fact.License.Key),
                        new(FidelitySlot, fact.Result.ProviderUsed == fact.Request.Provider ? Honoured : Degraded)])
                    .Bind(_ => set.Write(InferPartitions, (long)fact.Result.PartitionCount, lane))
                    .Bind(_ => set.Write(InferGolden, fact.Result.GoldenDelta, lane));
            }),
            // INGESTED domes carry no sky model, so the series keys on the empty string the environment row itself
            // publishes rather than on a synthesized "none" this arm keeps aligned.
            hooks.EnvironmentPrefilter.Observe(fact => {
                KeyValuePair<string, object?>[] sky = InstrumentSet.Tags(TenantContext.Current, (SkySlot, fact.SkyModel));
                return set.Write(PrefilterRuns, 1L, sky)
                    .Bind(_ => set.Write(PrefilterDuration, fact.Elapsed.TotalSeconds, sky));
            }),
            new HookDetacher(() => hooks.Faults.Change -= rejected));
    }
}
```

## [05]-[EVIDENCE_RECORDS]

- Owner: `MaterialsLog` — the fixed-severity generated emission grammar over the folder's banded faults and the rail's isolated evidence; `MaterialsLatency` — the checkpoint vocabulary and measured bracket over eager constructions.
- Entry: `MaterialsLog.Logged` rides `MapFail` on any Materials rail so a refusal logs once at the seam that produced it; `MaterialsLog.Drain(ILogger, Seq<IsolatedFault>)` projects a snapshot of the rail's parked evidence; `MaterialsLatency.Measured(ILatencyContext ledger, CheckpointToken started, CheckpointToken settled, Func<Fin<T>> body)` brackets one eager construction between two checkpoints.
- Auto: severity is declaration data on the attribute, so no call site chooses a level and no runtime switch over named severity verbs exists; `EventId` allocates from this owner's declared band and `EventName` is the dashboard-stable half; the generated `IsEnabled` gate precedes payload construction; checkpoint names register once at the app root through `RegisterCheckpointNames` and tokens resolve once through `ILatencyContextTokenIssuer.GetCheckpointToken`, so an unregistered name is a composition-time refusal.
- Packages: Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Telemetry.Abstractions, LanguageExt.Core.
- Growth: a new eager construction is one checkpoint pair in `Checkpoints`; a new event family is one partial in this band, never a per-fault-family verb.
- Boundary: libraries take the logger and the latency ledger by injection and a logger-less composition binds `NullLogger.Instance`, never a nullable handle; the `Code` and `Category` holes read the `Rasm.Element` band registry and the kernel `Error` projection, so a reader groups records by the same band a fault counter groups its series by and no fault text is re-formatted here; the instrument counts at `[04]` and the records here are disjoint mandates over one refusal, never two shapes of one record; settlement records in `finally`, so failed or throwing constructions close the same bracket as successful constructions.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using Microsoft.Extensions.Diagnostics.Latency; // CheckpointToken, ILatencyContext
using Microsoft.Extensions.Logging;             // ILogger, LogLevel, LoggerMessageAttribute
using Rasm.Domain;                              // IsolatedFault, Op, FaultExtensions extension property Category
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [SERVICES] -----------------------------------------------------------------------------
public static partial class MaterialsLog {
    // This owner allocates its event ids from Band; fault-band integers stay the Rasm.Element FaultBand registry's.
    private const int Band = 6400;

    [LoggerMessage(EventId = Band, EventName = "MaterialsRefused", Level = LogLevel.Warning,
        Message = "materials {Op} refused at band {Code}")]
    public static partial void Refused(ILogger logger, string op, int code, string category, string detail);

    [LoggerMessage(EventId = Band + 1, EventName = "MaterialsIsolated", Level = LogLevel.Warning,
        Message = "materials tap {Point} isolated at band {Code}")]
    public static partial void Isolated(ILogger logger, string point, int code, string category, string detail);

    extension<T>(Fin<T> step) {
        // MapFail keeps the rail intact, so the record is evidence beside the refusal rather than a second exit; a
        // generated emission returns void by the attribute's own contract, so the projection is a statement lambda
        // and the tuple-sequenced expression every Unit-returning sibling uses has no spelling here.
        public Fin<T> Logged(ILogger logger, Op key) =>
            step.MapFail(error => {
                Refused(logger, key.ToString(), error.Code, error.Category, error.Message);
                return error;
            });
    }

    public static Unit Drain(ILogger logger, Seq<IsolatedFault> held) =>
        ignore(held.Iter(fault =>
            Isolated(logger, fault.Point.ToString(), fault.Cause.Code, fault.Cause.Category, fault.Cause.Message)));
}

public static class MaterialsLatency {
    public const string CatalogueBuildStarted = "rasm.materials.catalogue.build.started";
    public const string CatalogueBuildSettled = "rasm.materials.catalogue.build.settled";
    public const string InteractionSolveStarted = "rasm.materials.interaction.solve.started";
    public const string InteractionSolveSettled = "rasm.materials.interaction.solve.settled";

    // RegisterCheckpointNames rows the app root registers before any token resolves.
    public static readonly Seq<string> Checkpoints =
        Seq(CatalogueBuildStarted, CatalogueBuildSettled, InteractionSolveStarted, InteractionSolveSettled);

    public static Fin<T> Measured<T>(ILatencyContext ledger, CheckpointToken started, CheckpointToken settled, Func<Fin<T>> body) {
        ledger.AddCheckpoint(started);
        try { return body(); }
        finally { ledger.AddCheckpoint(settled); }
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
