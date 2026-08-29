# [RASM_RESULTS]

Kernel ROP substrate (`Rasm.Domain`). Every fallible kernel surface fails through one `Fault` band carrying one generated code, every retriable failure answers one `Retriability` discriminant, every lock-free transition returns one `Transition` verdict, every disposable crossing rides `Lease<T>`, and every result proves itself through the `IValidityEvidence` fold — the floor no kernel page compiles without.

`Rasm.Domain` rides `Directory.Build.props` as a branch-wide global using, so every stratum names this floor unqualified. `TelemetrySource` (`frame.md`) is the typed package roster this page's band owner column reads; `Dimension` (`Numerics/atoms.md`) is the bounded-budget carrier the transition owner reads — one assembly, so neither composition needs a reference edge. `Duration` resolves to LanguageExt's schedule-stream duration under the injected global using, never NodaTime's clock carrier, so a redrive delay reaches its schedule with no conversion.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: `FaultBand` + `Fault` + `FaultId` + `KernelFault` — the branch-wide code-space registry, the expected-error base, generated numeric identity, the closed kernel fault union, and `FaultExtensions.Owner`.
- [03]-[REDRIVE]: `Retriability` + `RedrivePolicy` + `Verdict` + `Redrive` — the one retriability discriminant and the one re-drive owner over `Schedule`.
- [04]-[RESOURCE_RESULT]: `Lease<T>` — Owned/Borrowed disposal discipline with `Use`/`Resource`/`Dispose` folds — and `Custody`, the all-attempted release algebra (`Release`, failure-arm `Rollback`, both-arms `Settled`/`Bracket`).
- [05]-[TRANSITION]: `Transition<TState>` + `Cell` — the verdict every lock-free `Atom` transition returns, and its four transition shapes.
- [06]-[VALIDITY_FOLD]: `IValidityEvidence` + `ValidityClaim` — the one result-validity fold; a result declares which claims hold, never how.
- [07]-[CARRIAGE_LAW]: `HostEdge` — `Eff<Env>` as runtime carriage, telemetry as a tap, one paradigm per operation, and the one host-crossing vocabulary.
- [08]-[CARRIER_CODEC]: `LanguageExtJsonConverterFactory` — the one carrier-space System.Text.Json owner every wire mint at every stratum registers.
- [09]-[DENSITY_BAR]: owner/concern/result partition across the substrate floor.

## [02]-[FAULT_BAND]

- Owner: `FaultBand` the branch-wide fault-code registry whose rows partition the WHOLE code space; `Fault` the abstract `Error` base sealing the expected protocol over generated identity and lowering into the LanguageExt exception protocol through `WrappedErrorExpectedException`; `FaultId` the cached identity one `[FaultCase]` leaf mints against its family's band row — the transported code beside the generated case token; `ICausedFault` the mandatory cause projection `Inner` folds; `KernelFault` the closed `[Union]` of every kernel-substrate failure; and `FaultExtensions` the `extension(Error)` block deriving allocating owner off the ledger.
- Cases: a `FaultBand` row is one allocated range — base `Key`, `Span`, `Kind` separating log-event allocation from fault allocation, and `Owner` the typed `TelemetrySource` package. Each `KernelFault` case carries its typed payload, renders its own `Message`, and states its identity as ONE `[FaultCase]` ordinal.
- Law: ONE registry holds every band in the branch. Disjointness, span containment, and the log-const mirror are FORCED by `Proof`, the one registry audit a host seats before it serves — it takes the assemblies whose `[FaultCase]` leaves it censuses, so a family reaches the proof by being loaded rather than by remembering to register, so a folder cannot collide by forgetting to pin a foreign neighborhood — the failure mode a per-folder registry with `Mirror` rows carries by construction, and a deferred proof no path touches runs never. NAMED LOSS accepted: a folder-local band mint becomes a kernel row, so a folder adding a band edits a kernel page; the gain is one `Proof` over the whole code space instead of five hand-mirrored registries that agree only by inspection. `Mirror` and `Page` columns both delete: one registry cannot collide with itself, and the `Owner` row already routes the provenance.
- Law: a row name is its bare concern; where two owners claim one concern the row takes the owner prefix on BOTH sides, so no reader resolves `Command` or `Identity` by neighborhood arithmetic. A fault row's `Span` equals its direct union leaf count and ordinals fill `0..Span-1`; `Proof` refuses a row whose declared `Span` cannot seat its family.
- Law: a `[LoggerMessage] EventId` argument must be a compile-time CONST while a registry row is an instance, so a log-event band publishes `public const int <Name>Base` beside its row with the SAME value — `Proof` reads the `<Name>Base` naming rule to pair the two and forces their agreement, so no log owner restates that half; an attribute literal computed from nothing is the drifting form. Row names never shadow a TYPE their consumers hold in scope; a shadowing concern renames with its owner prefix (`UiContext`, `UiSurface`, `StoreSchedule`, `StoreTopology`).
- Law: every kernel case carries one compact explicit ordinal. Recovery predicates match on the case or its numeric identity, never on rendered text.
- Law: payloads are evidence, never live resources — `InvalidGeometry` carries the failing `Type`, not the geometry reference, because coercion leases dispose before a fault surfaces, so a live payload hands consumers a disposed native object and retains host memory inside accumulating `Validation` results. A payload is the WHOLE of what a case transports: the label, the measured scalar, the requirement it missed, the failing type. `OutOfRange` is the one scalar-range refusal across the kernel and carries the rejected number beside its requirement; a range rejection never degrades to `InvalidInput`, which drops both. `InvalidValue` is its non-scalar sibling, the generated-factory rejection carrying the owner label and generated requirement text.
- Law: `InvalidContext` names an execution-context refusal — a main-thread-affinity guard, a released lease, a dead conduit or live-state gate — distinct from `MissingContext` (no model context supplied) and `InvalidInput` (the value itself is unsound); recovery differs by case (marshal or re-acquire versus repair the argument), so host thread and lifecycle gates raise `InvalidContext`.
- Law: the two-family split holds from the kernel side — a substrate failure is a `KernelFault` case, a robust-core geometry failure is a `Rasm.Numerics` `GeometryFault` binding `FaultBand.Geometry`, and neither absorbs the other; a page composing both families converts nothing, both already `Error`.
- Law: the category plane is DELETED — a fault states its identity as one number and its semantics as its case type, so no owner renders, stores, or wires a category text. Owner is a LOCAL derivation off the ledger through `FaultBand.OwnerOf`, never a stored column or a wire field, and a foreign `Error` gets no fabricated owner.
- Law: `FaultId.Case` is the generator's `nameof` for the leaf and stays LOCAL evidence — it tags a span, fills a log field, and rides `Domain/telemetry`'s `FaultObservation` in-process, while equality, hashing, recovery predicates, metric dimensions, and every wire and store column read the code alone. A column carrying the token past that line publishes a second discriminant peers then join on, so a case rename becomes a peer-wide re-spelling where today it is one compilation; the generated `FaultDetail` and `FaultObservation` therefore transport only the family-relative ordinal beside the family domain, `FaultBand.OwnerOf` still derives the owner, and the token answers only the question a number cannot — WHICH case, in an operator's own vocabulary, at the moment they read the trace. Two declarations at THIS owner enforce it rather than a rule each consumer remembers: the equality pair is declared so the record fold never enrols the token, and `[JsonIgnore]` keeps every codec off it, so a boundary carrying the token must ADD a column deliberately instead of inheriting one.
- Law: folder fault families bind ONE `FaultBand` row on the root and state one `[FaultCase]` ordinal per direct sealed leaf; the generator caches identity per case — the band-relative code and the leaf's own `nameof` in ONE `FaultId` — and diagnoses a missing, duplicate, or negative ordinal, a root missing `[Union]` or its band binding, an indirect case, and a hand-written member competing with generated identity. SPAN CONTAINMENT is `Proof`'s half of that burden — because Roslyn cannot evaluate the constructor arguments of a referenced `static readonly` band, and a mirror minting a generator-readable copy is the deleted form. NAMED LOSS: a mis-declared band surfaces at the host's startup audit rather than at the compilation that declared it; the rejected alternative surfaced it as `TypeInitializationException` at the first fault mint, which names neither the offending row nor the page that added it. A per-case band literal, a hand offset switch, a roster mirroring the union, raw `+ n` arithmetic, and a family reading another owner's row are the deleted forms this floor makes unspellable.
- Growth: a new case is one typed `[FaultCase]` leaf, followed by declaration-order ordinal compaction and the matching `Span` edit; a new family alone allocates one registry row and one `[Union]` root deriving `Fault` with that binding.
- Boundary: `KernelFault` crosses the in-process kernel, analysis runtime, and Grasshopper boundary as the one substrate failure vocabulary; an in-process subscriber receives only `Domain/telemetry`'s `FaultObservation` projection — the optional generated `FaultId`, recovery, bounded cause stamps — never the fault value or its rendered `Message`, and a wire lowering copies the `Code` half of that identity while the case token stays behind. This union therefore owes no JSON derived-type roster. Log-event-id bands register HERE under `BandKind.Event`; `Proof` permits event and fault rows to share a base while keeping rows of the same kind disjoint.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`, `[Union]`), LanguageExt.Core (`Error`, `Option`, `Unit`), BCL inbox (`CultureInfo`, `[JsonIgnore]`), RhinoCommon (`UnitSystem`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using Rhino;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
public enum BandKind { Event, Fault }

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FaultBand {
    public int Span { get; }
    public BandKind Kind { get; }
    public TelemetrySource Owner { get; }

    // --- [APPHOST_CORE]
    public static readonly FaultBand SpineEvents      = new(1000, 100, BandKind.Event, TelemetrySource.AppHost);
    public const int SpineEventsBase = 1000;
    public static readonly FaultBand Profile          = new(1100, 4, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Lifecycle        = new(1200, 3, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Update           = new(1300,  6, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand SupplyChain      = new(1320,  7, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand HostSchedule     = new(1340,  3, BandKind.Fault, TelemetrySource.AppHost);
    // --- [AEC_AND_KERNEL_GEOMETRY]
    public static readonly FaultBand Core             = new(2200,  30, BandKind.Fault, TelemetrySource.Compute);
    public static readonly FaultBand Component        = new(2300,  41, BandKind.Fault, TelemetrySource.Materials);
    public static readonly FaultBand Geometry         = new(2350,  60, BandKind.Fault, TelemetrySource.Kernel);
    public static readonly FaultBand Appearance       = new(2450,   3, BandKind.Fault, TelemetrySource.Materials);
    public static readonly FaultBand Raster           = new(2460,   7, BandKind.Fault, TelemetrySource.Materials);
    public static readonly FaultBand Projection       = new(2470,   3, BandKind.Fault, TelemetrySource.Materials);
    public static readonly FaultBand Element          = new(2500,   7, BandKind.Fault, TelemetrySource.Element);
    public static readonly FaultBand Bim              = new(2600,   2, BandKind.Fault, TelemetrySource.Bim);
    public static readonly FaultBand Fabrication      = new(2700,  66, BandKind.Fault, TelemetrySource.Fabrication);
    // --- [APPHOST_PLATFORM]
    public static readonly FaultBand Config           = new(4100, 5, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Hop              = new(4500,  14, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Wire             = new(4520,  13, BandKind.Fault, TelemetrySource.Compute);
    public static readonly FaultBand HostCoordination = new(4540,  9, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand HostCommand      = new(4600,  8, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Grant            = new(4620,  6, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand HostIdentity     = new(4630,  9, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Mcp              = new(4640,  9, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Sandbox          = new(4660,  6, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Feature          = new(4700,  4, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Solver           = new(4710,  4, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand LiveWire         = new(4720,  9, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Bus              = new(4730,  1, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Outbox           = new(4740,  5, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand LaneGuard        = new(4750,  6, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Replay           = new(4760,  4, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Orchestration    = new(4770,  6, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Secret           = new(4780,  3, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Pem              = new(4790,  3, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand HostFederation   = new(4800,  5, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Support          = new(4810,  4, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Drain            = new(4820,  2, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Benchmark        = new(4840,  5, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Grasshopper      = new(4850,   5, BandKind.Fault, TelemetrySource.Grasshopper);
    public static readonly FaultBand Companion        = new(4870,  8, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Telemetry        = new(4880,  5, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand Instrument       = new(4890,  3, BandKind.Fault, TelemetrySource.AppHost);
    public static readonly FaultBand GrasshopperLog   = new(4700,  20, BandKind.Event, TelemetrySource.Grasshopper);
    public const int GrasshopperLogBase = 4700;
    // --- [RHINO]
    public static readonly FaultBand HostDraft        = new(4900,   1, BandKind.Fault, TelemetrySource.Rhino);
    public static readonly FaultBand HostDocument     = new(4920,   3, BandKind.Fault, TelemetrySource.Rhino);
    public static readonly FaultBand HostExchange     = new(4930,   5, BandKind.Fault, TelemetrySource.Rhino);
    public static readonly FaultBand HostViewport     = new(4940,   1, BandKind.Fault, TelemetrySource.Rhino);
    public static readonly FaultBand HostRender       = new(4950,   4, BandKind.Fault, TelemetrySource.Rhino);
    public static readonly FaultBand HostPlugin       = new(4960,   5, BandKind.Fault, TelemetrySource.Rhino);
    public static readonly FaultBand HostPersistence  = new(4970,   4, BandKind.Fault, TelemetrySource.Rhino);
    // --- [PERSISTENCE]
    public static readonly FaultBand RemoteStore      = new(5400,  17, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Cache            = new(5500,  10, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Embedded         = new(7710,  10, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Sync             = new(8250,   9, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Commit           = new(8260,   5, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Egress           = new(8270,   6, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Retention        = new(8280,   3, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Recovery         = new(8290,   6, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Graph            = new(8300,   3, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Codec            = new(8310,   6, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand StoreIdentity    = new(8340,  10, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Columnar         = new(8350,  10, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Cypher           = new(8360,   5, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand StoreTopology    = new(8370,   3, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Server           = new(8380,  10, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Tabular          = new(8390,   7, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand StoreSchedule    = new(8400,   4, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Retrieval        = new(8410,   6, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand StoreFederation  = new(8420,   9, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand StoreCoordination = new(8430, 10, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand GeoIngest        = new(8440,   8, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Selection        = new(8460,   4, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand StoreIssue       = new(8470,   2, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Series           = new(8480,   8, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand StoreStat        = new(8490,   4, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Ingress          = new(8500,   6, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Contract         = new(8510,   7, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Scan             = new(8520,   4, BandKind.Fault, TelemetrySource.Persistence);
    public static readonly FaultBand Cesql            = new(8530,   7, BandKind.Fault, TelemetrySource.Persistence);
    // --- [APPUI_SHELL]
    public static readonly FaultBand UiSurface        = new(6000,  5, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Control          = new(6010,  6, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Layout           = new(6020,  3, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Virtual          = new(6030,  3, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Dialog           = new(6040,  9, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand InputDriver      = new(6050,  8, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Nav              = new(6060,  8, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand UiCommand        = new(6070,  6, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Screen           = new(6080,  7, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Accessibility    = new(6090,  4, BandKind.Fault, TelemetrySource.AppUi);
    // --- [APPUI_RENDER]
    public static readonly FaultBand Viewport         = new(6100,  5, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Shader           = new(6110,  5, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Immersive        = new(6120,  4, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Capture          = new(6130,  4, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand UiDraft          = new(6140,  4, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Animation        = new(6150,  7, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Visual           = new(6160,  8, BandKind.Fault, TelemetrySource.AppUi);
    // --- [APPUI_CHARTS]
    public static readonly FaultBand Chart            = new(6200,  20, BandKind.Fault, TelemetrySource.AppUi);
    // --- [APPUI_EDITING]
    public static readonly FaultBand Edit             = new(6300,  6, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Form             = new(6310,  8, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand History          = new(6320,  7, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Canvas           = new(6330,  8, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand LiveData         = new(6340,  6, BandKind.Fault, TelemetrySource.AppUi);
    // --- [APPUI_DOCUMENT]
    public static readonly FaultBand Notebook         = new(6400,  3, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Content          = new(6410,  4, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Export           = new(6420,  8, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Search           = new(6430,  4, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Board            = new(6440,  6, BandKind.Fault, TelemetrySource.AppUi);
    // --- [LOG_EVENT_IDS]
    public static readonly FaultBand MaterialsLog     = new(6400,  10, BandKind.Event, TelemetrySource.Materials);
    public const int MaterialsLogBase = 6400;
    public static readonly FaultBand HostObjectsLog   = new(6410,  10, BandKind.Event, TelemetrySource.Rhino);
    public const int HostObjectsLogBase = 6410;
    // --- [APPUI_COLLAB]
    public static readonly FaultBand Collab           = new(6500,  8, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand UiIssue          = new(6510,  7, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Tour             = new(6520,  2, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Session          = new(6530,  7, BandKind.Fault, TelemetrySource.AppUi);
    // --- [APPUI_THEME]
    public static readonly FaultBand Asset            = new(6600,  7, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Locale           = new(6610,  6, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Theme            = new(6620,  9, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Motion           = new(6630,  6, BandKind.Fault, TelemetrySource.AppUi);
    // --- [APPUI_DIAGNOSTICS]
    public static readonly FaultBand Proof            = new(6700,  12, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand DevLoop          = new(6720,  3, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Governor         = new(6730,  2, BandKind.Fault, TelemetrySource.AppUi);
    // --- [APPUI_VFX]
    public static readonly FaultBand Material         = new(6800,  8, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Effect           = new(6810,  7, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Compose          = new(6820,  7, BandKind.Fault, TelemetrySource.AppUi);
    // --- [APPUI_ANALYSIS]
    public static readonly FaultBand Layer            = new(6900,  8, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand Compare          = new(6910,  5, BandKind.Fault, TelemetrySource.AppUi);
    public static readonly FaultBand UiContext        = new(6920,  6, BandKind.Fault, TelemetrySource.AppUi);
    // --- [KERNEL]
    public static readonly FaultBand Kernel           = new(9100,  11, BandKind.Fault, TelemetrySource.Kernel);
    public static readonly FaultBand Interaction      = new(9200,  6, BandKind.Fault, TelemetrySource.Kernel);

    public int Code(int offset) => Key + offset;

    public bool Seats(int offset) => offset >= 0 && offset < Span;

    public static Option<FaultBand> OwnerOf(BandKind kind, int code) => toSeq(Items).Find(band => band.Kind == kind && code >= band.Key && code < band.Key + band.Span);

    public static Fin<Unit> Proof(params ReadOnlySpan<Assembly> carried) =>
        Overlaps().Append(Mirrors()).Append(Undersized(carried: Iterable.FromSpan(carried).ToSeq())) is { IsEmpty: false } faults
            ? Fin.Fail<Unit>(error: Error.Many(faults))
            : Fin.Succ(value: unit);

    static Seq<Error> Overlaps() =>
        toSeq(toSeq(Items).GroupBy(static band => band.Kind))
            .Bind(static space => toSeq(space.OrderBy(static band => band.Key)) is var ordered
                ? ordered.Zip(ordered.Skip(count: 1))
                    .Filter(static pair => pair.First.Key + pair.First.Span > pair.Second.Key)
                    .Map(static pair => (Error)new KernelFault.InvalidValue(
                        Label: string.Create(provider: CultureInfo.InvariantCulture, $"{pair.Second.Owner.Key}@{pair.Second.Key}"),
                        Requirement: string.Create(provider: CultureInfo.InvariantCulture,
                            $"a base at or above {pair.First.Key + pair.First.Span}, clear of {pair.First.Owner.Key}@{pair.First.Key}+{pair.First.Span}")))
                : Seq<Error>())
            .Strict();

    static Seq<Error> Mirrors() =>
        toSeq(typeof(FaultBand).GetFields(BindingFlags.Public | BindingFlags.Static))
            .Filter(static field => field.IsLiteral && field.FieldType == typeof(int) && field.Name.EndsWith("Base", StringComparison.Ordinal))
            .Choose(static field => TryGet(field.Name[..^"Base".Length], out FaultBand? row) && row is { } band
                ? (int)field.GetRawConstantValue()! == band.Key
                    ? Option<Error>.None
                    : Some((Error)new KernelFault.OutOfRange(
                        Label: field.Name,
                        Scalar: (int)field.GetRawConstantValue()!,
                        Requirement: string.Create(provider: CultureInfo.InvariantCulture, $"the {band.Key} its own row allocates")))
                : Some((Error)new KernelFault.InvalidValue(
                    Label: field.Name,
                    Requirement: "a band row named by the const it mirrors")))
            .Strict();

    static Seq<Error> Undersized(Seq<Assembly> carried) =>
        carried
            .Bind(static assembly => toSeq(assembly.GetTypes()))
            .Filter(static type => type.IsSealed && typeof(Fault).IsAssignableFrom(type) && type.IsDefined(typeof(FaultCaseAttribute), inherit: false))
            .Choose(static leaf => Optional(leaf.BaseType).Map(root => (Root: root, Leaf: leaf)))
            .GroupBy(static pair => pair.Root)
            .Choose(static family => Optional(family.Key
                    .GetField("FamilyBand", BindingFlags.NonPublic | BindingFlags.Static)?
                    .GetValue(obj: null) as FaultBand)
                .Map(band => (Band: band, Root: family.Key, Leaves: family.Count())))
            .Filter(static family => family.Leaves > family.Band.Span)
            .Map(static family => (Error)new KernelFault.OutOfRange(
                Label: string.Create(provider: CultureInfo.InvariantCulture, $"{family.Root.Name}@{family.Band.Key}"),
                Scalar: family.Band.Span,
                Requirement: string.Create(provider: CultureInfo.InvariantCulture, $"a span seating {family.Leaves} leaves")))
            .Strict();
}

// --- [ERRORS] --------------------------------------------------------------------------
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class FaultCaseAttribute(int offset) : Attribute {
    public int Offset { get; } = offset;
}

public sealed record FaultId {
    internal FaultId(FaultBand band, int offset, string @case) => (Code, Case) = (band.Code(offset: offset), @case);

    public int Code { get; }

    [JsonIgnore]
    public string Case { get; }

    public bool Equals(FaultId? other) => other is not null && Code == other.Code;

    public override int GetHashCode() => Code;
}

public interface ICausedFault {
    Error Cause { get; }
}

public abstract record Fault : Error {
    protected abstract FaultId IdentityCore { get; }
    protected static FaultId Identify(FaultBand band, int offset, string @case) => new(band, offset, @case);

    public FaultId Identity => IdentityCore;
    public sealed override int Code => Identity.Code;
    public sealed override bool IsExpected => true;
    public sealed override bool IsExceptional => false;
    public sealed override bool Is(Error error) => error is Fault fault && Identity == fault.Identity;
    public sealed override Option<Error> Inner => this is ICausedFault caused ? Some(caused.Cause) : None;
    public sealed override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);

    public virtual Retriability Retriability => Retriability.Terminal;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KernelFault : Fault {
    private KernelFault() { }
    private static readonly FaultBand FamilyBand = FaultBand.Kernel;

    [FaultCase(0)]
    public sealed partial record MissingContext : KernelFault { public override string Message => "An execution context is required."; }
    [FaultCase(1)]
    public sealed partial record InvalidContext : KernelFault { public override string Message => "Invoked outside a live execution context."; }
    [FaultCase(2)]
    public sealed partial record InvalidInput(Option<string> Axis = default) : KernelFault {
        public override string Message => $"Invalid input{Axis.Map(static a => $" ({a})").IfNone(string.Empty)}.";
    }
    [FaultCase(3)]
    public sealed partial record Unsupported(Type InputType, Type OutputType) : KernelFault {
        public override string Message => $"Input '{InputType.Name}' with output '{OutputType.Name}' is unsupported.";
    }
    [FaultCase(4)]
    public sealed partial record InvalidResult(Option<string> Detail = default) : KernelFault {
        public override string Message => $"No valid result{Detail.Map(static d => $": {d}").IfNone(static () => ".")}";
    }
    [FaultCase(5)]
    public sealed partial record Cancelled(Error Cause) : KernelFault, ICausedFault { public override string Message => "Cancelled."; }
    [FaultCase(6)]
    public sealed partial record MissingGeometry : KernelFault { public override string Message => "Geometry input is required."; }
    [FaultCase(7)]
    public sealed partial record InvalidGeometry(Type Shape, string Check, string Log) : KernelFault {
        public override string Message => string.IsNullOrWhiteSpace(value: Log)
            ? $"Geometry validation failed for {Shape.Name} under check '{Check}'."
            : $"Geometry validation failed for {Shape.Name} under check '{Check}': {Log}";
    }
    [FaultCase(8)]
    public sealed partial record InvalidValue(string Label, string Requirement) : KernelFault {
        public override string Message => $"Value '{Label}' is invalid: {Requirement}";
    }
    [FaultCase(9)]
    public sealed partial record OutOfRange(string Label, double Scalar, string Requirement) : KernelFault {
        public override string Message => string.Create(provider: CultureInfo.InvariantCulture, $"Value '{Label}' must be {Requirement}; actual={Scalar:R}.");
    }
    [FaultCase(10)]
    public sealed partial record InvalidUnitSystem(UnitSystem Units, string Requirement) : KernelFault { public override string Message => $"Model unit system must be {Requirement}; actual={Units}."; }
    [FaultCase(11)]
    public sealed partial record RankDeficient : KernelFault { public override string Message => "The numerical operator is rank deficient."; }
    [FaultCase(12)]
    public sealed partial record IterationLimit : KernelFault { public override string Message => "The numerical iteration limit was reached."; }
    [FaultCase(13)]
    public sealed partial record ResidualExceeded : KernelFault { public override string Message => "The numerical residual exceeded its tolerance."; }
}

public static class FaultExtensions {
    extension(Error error) {
        public Option<TelemetrySource> Owner =>
            error is Fault fault
                ? FaultBand.OwnerOf(BandKind.Fault, fault.Code).Map(static band => band.Owner)
                : None;
    }
}
```

## [03]-[REDRIVE]

- Owner: `Retriability` the branch-wide retriability discriminant carried as a virtual on `Fault` and the ONE posture spelling through its `Key` projection, `RedrivePolicy` the one re-drive policy value, `Verdict` the one durable re-drive answer, and `Redrive` the two-arm executor and the ONE cancellation reading.
- Cases: `Retriability` is a `[Union]` rather than a keyed roster because `Throttled` carries a server-stated delay the other two do not; `Verdict` splits a scheduled retry from an exhausted bound and a terminal refusal, and every case is consumed by `Settle`.
- Entry: a fault band overrides `Retriability` on its own cases and states nothing else — every kernel and folder band inherits `Terminal` by construction, so a case that never overrides is terminal without spelling it. `RedrivePolicy.Of(law, bound)` is the ONE mint name; the bound applies by derivation at `Curve`, so a stored curve cannot disagree with `Bound`, and `Exhausted` reads the same ordinal convention `Curve`'s stream counts — attempt zero is the first failure, and exactly `Bound` re-drives admit on both arms.
- Law: library tiers CLASSIFY and execute nothing — the discriminant rides the fault, the policy rides the runtime, and only a root-bound executor runs `Redrive`. Per-policy `Func<Error, bool>` classifiers are the deleted form: they re-decide at each policy what the fault already answers, which is exactly the split the branch retriability ruling forbids; `bool IsTransient` interfaces fall for the same reason, one axis short of the throttled case.
- Law: `Settle` is one verdict per pass and holds NO loop state, so a resumed workflow, a swept outbox row, and a cache-aged assessment each read the same predicate against their own durable ordinal. Exhaustion is a typed `Abandoned`, never a success-shaped fall-through, and a schedule that runs dry at the ordinal abandons rather than retrying forever.
- Law: `Key` is the ONE posture render and every consumer composes it — a metric dimension value, a span tag, a log field, and a board caption all read the same member, so the three-literal `Switch` a consumer once spelled for itself is deleted wherever it stood. The failure that form carries is silent: each copy is proved total by the generator and none is proved to AGREE with its siblings, so one renamed word splits a dashboard population and no build says so.
- Law: `Cancellation(Error)` is the ONE cancellation reading and answers the CAUSE, so a caller needing the evidence and a caller needing only the verdict compose the same member. It spans BOTH spellings by construction — `KernelFault.Cancelled` a cancellation `HostEdge.Captured` PROVED against a token, `Errors.Cancelled` the code `Try.lift` and every kernel refusal normalize onto — and folds `ManyErrors` membership like `Posture`. A site testing one spelling alone is the deleted form: it reads correct and silently misses every cancellation minted by the other funnel.
- Law: a foreign `Error` — anything not deriving `Fault` — is Terminal by construction, so an un-adopted third-party error cannot become silently retriable. `Settle` folds the `ManyErrors` MEMBERSHIP tree recursively, never `Inner` which is causal evidence: `AsIterable()` and `Count` read DIRECT children alone, so a nested aggregate otherwise hides a terminal leaf behind a wrapper.
- Exemption: `Redrive.Run` is the IN-PROCESS arm and carries the whole retry mechanism on `IO<T>.RetryWhile`; no hand attempt loop, no `Task.Delay` window, and no clock arithmetic exists at any consumer.
- Growth: a new retriability posture is one `Retriability` case with the `Settle` arm it selects and one word on `Key`, which every emitter then answers unedited; a new backoff shape is a `Schedule` composition at the policy mint, never a member here.
- Packages: LanguageExt.Core (`Schedule`, `ScheduleTransformer`, `IO`, `Duration`, `Iterable`, `Option`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record Retriability {
    private Retriability() { }
    public sealed record TerminalCase : Retriability;
    public sealed record TransientCase : Retriability;
    public sealed record ThrottledCase(Duration RetryAfter) : Retriability;
    public static Retriability Terminal { get; } = new TerminalCase();
    public static Retriability Transient { get; } = new TransientCase();
    public static Retriability Throttled(Duration retryAfter) => new ThrottledCase(RetryAfter: retryAfter);

    public string Key => Switch(
        terminalCase: static _ => "terminal",
        transientCase: static _ => "transient",
        throttledCase: static _ => "throttled");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RedrivePolicy(Schedule Law, int Bound) {
    public static readonly RedrivePolicy None = Of(law: Schedule.Never, bound: 0);
    public static RedrivePolicy Of(Schedule law, int bound) => new(Law: law, Bound: int.Max(bound, 0));
    public Schedule Curve => Law & Schedule.recurs(times: Bound);
    public bool Exhausted(int attempt) => attempt >= Bound;
    public Option<Duration> Next(int attempt) => Iterable.head(list: Curve.Run().Skip(amount: attempt));
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union]
public abstract partial record Verdict {
    private Verdict() { }
    public sealed record Deferred(int Attempt, Duration After) : Verdict;
    public sealed record Abandoned(Error Cause, int Attempt) : Verdict;
    public sealed record Terminal(Error Cause) : Verdict;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Redrive {
    public static IO<T> Run<T>(RedrivePolicy policy, IO<T> work) =>
        work.RetryWhile(schedule: policy.Curve, predicate: static error => Posture(error) is Retriability.TransientCase);

    public static Retriability Posture(Error fault) => fault switch {
        ManyErrors many => many.Errors
            .Fold(Option<Retriability>.None, static (state, child) =>
                Some(state.Match(
                    Some: held => Merge(held, Posture(child)),
                    None: () => Posture(child))))
            .IfNone(Retriability.Terminal),
        Fault expected => expected.Retriability,
        _ => Retriability.Terminal,
    };

    public static Option<Error> Cancellation(Error fault) => fault switch {
        KernelFault.Cancelled proven => Some(proven.Cause),
        ManyErrors many => many.Errors.Choose(Cancellation).Head,
        _ => fault.Is(Errors.Cancelled) ? Some(fault) : None,
    };

    public static Verdict Settle(RedrivePolicy policy, Error fault, int attempt) =>
        Posture(fault).Switch(
            terminalCase: _ => (Verdict)new Verdict.Terminal(Cause: fault),
            transientCase: _ => Defer(policy: policy, fault: fault, attempt: attempt, after: policy.Next(attempt: attempt)),
            throttledCase: throttled => Defer(policy: policy, fault: fault, attempt: attempt, after: Some(throttled.RetryAfter)));

    static Retriability Merge(Retriability left, Retriability right) => (left, right) switch {
        (Retriability.TerminalCase, _) or (_, Retriability.TerminalCase) => Retriability.Terminal,
        (Retriability.ThrottledCase a, Retriability.ThrottledCase b) =>
            a.RetryAfter.CompareTo(b.RetryAfter) >= 0 ? a : b,
        (Retriability.ThrottledCase throttled, _) => throttled,
        (_, Retriability.ThrottledCase throttled) => throttled,
        _ => Retriability.Transient,
    };

    static Verdict Defer(RedrivePolicy policy, Error fault, int attempt, Option<Duration> after) =>
        policy.Exhausted(attempt: attempt)
            ? new Verdict.Abandoned(Cause: fault, Attempt: attempt)
            : after.Match(
                Some: delay => (Verdict)new Verdict.Deferred(Attempt: attempt + 1, After: delay),
                None: () => new Verdict.Abandoned(Cause: fault, Attempt: attempt));
}
```

## [04]-[RESOURCE_RESULT]

- Owner: `Lease<T>` — the closed `[Union]` over disposal ownership for any `T : class, IDisposable`. `Owned` carries a value this case must dispose; `Borrowed` carries a value the host still owns. `Custody` — the release algebra over `Fin`: `Release` the all-attempted roster fold, `Dispose` its disposable-roster projection, `Rollback` failure-only compensation, and `Settled`/`Bracket` the both-arms postures split by whether the primary is already settled.
- Entry: `Acquire(mint)` is the fallible mint funnelling a throwing host mint into `Fin` through `Try.lift`; the fallible `Use(body)` runs a `Fin`-shaped projection through `Settled` and AGGREGATES a cleanup fault into the primary; `Use(project)` and the state-threaded `Use(state, project)` are the pure consumption gate; `Resource` reads the live value where the caller manages the extent, and `Dispose()` releases `Owned` and no-ops `Borrowed`. `Custody.Release` is the reverse-order, all-attempted fold over a `Fin`-shaped resource roster, preserving the whole cleanup set through `Error.Many`; `Custody.Dispose` is that same fold for an `IDisposable` roster; `fold.Rollback(held...)` releases the already-acquired handle span LIFO on the FAILURE arm alone; `fold.Settled(held, release)` runs a fallible roster release after an already-settled primary on BOTH arms; `Custody.Bracket(body, held...)` captures a body and disposes an `IDisposable` span on both arms; `Custody.Bracket(acquire, project)` lifts a produced-inside resource into `Fin`, projects it live, and releases unconditionally.
- Law: ownership is a case, never a flag — the coercion table (`normalization.md`) returns `Fin<Lease<Curve|Surface|Brep>>` deciding owned-versus-borrowed per recovery path, `Requirement`'s lease-aware checks (`validation.md`) thread it, and projection carriers ride `Lease<GeometryBase>`.
- Law: the state-threaded `Use` overload keeps projections closure-free — state rides the fold, lambdas stay `static`.
- Law: `Try.lift` is the ONE funnel a host disposal or host mint crosses on this owner, and `Released` its single-resource row every posture composes; a `try`/`catch` beside a release is the deleted form.
- Law: an acquire chain that must release what it already holds composes `Rollback` on the failure arm — a `try`/`catch` that disposes and rethrows is the deleted form, because the rethrow reaches no consumer this owner can name.
- Law: release brackets ACQUISITION here, not outcome — `using` scopes the value for the whole projection and runs on every exit path, failed result included, so the success-arm-release regression cannot occur and no failure branch owes cleanup evidence. Effect-typed brackets are the substrate form for an asynchronous or `Fin`-shaped acquisition; a synchronous host handle whose whole contract is one lexical window composes `using` directly and lifts nothing.
- Law: posture follows CUSTODY, never style — `Rollback` serves the acquire chain whose success value takes ownership; `Settled` serves an already-run primary still holding a fallible resource roster; `Bracket` captures scratch whose custody never transfers. Cleanup faults AGGREGATE into the primary outcome on every posture — a leaking release never silently replaces the fault that caused it, and a primary success under a failed release reads as the release fault. Folder-local release folds and domain-flow `try`/`finally` releases both delete onto this owner.
- Boundary: the owned arms of `Lease<T>.Use` hold the `using` boundary — the platform-forced disposal boundary.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record Lease<T> where T : class, IDisposable {
    private Lease() { }
    public sealed record Owned(T Value) : Lease<T>;
    public sealed record Borrowed(T Value) : Lease<T>;
    public static Fin<Lease<T>> Acquire(Func<T> mint) =>
        Try.lift(mint).Run().Map(static value => (Lease<T>)new Owned(Value: value));
    public Fin<TResult> Use<TResult>(Func<T, Fin<TResult>> body) =>
        Switch(state: body,
            owned: static (use, owned) => use(arg: owned.Value).Settled(release: () => Custody.Released(owned.Value)),
            borrowed: static (use, borrowed) => use(arg: borrowed.Value));
    public TResult Use<TResult>(Func<T, TResult> project) => Switch(state: project, owned: static (use, owned) => { using T resource = owned.Value; return use(resource); }, borrowed: static (use, borrowed) => use(borrowed.Value));
    public TResult Use<TState, TResult>(TState state, Func<TState, T, TResult> project) =>
        Switch(state: (State: state, Project: project), owned: static (use, owned) => { using T resource = owned.Value; return use.Project(use.State, resource); }, borrowed: static (use, borrowed) => use.Project(use.State, borrowed.Value));
    public T Resource => Switch(owned: static owned => owned.Value, borrowed: static borrowed => borrowed.Value);
    public Unit Dispose() => Switch(owned: static owned => { owned.Value.Dispose(); return unit; }, borrowed: static _ => unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Custody {
    public static Fin<Unit> Release(Seq<Func<Fin<Unit>>> releases) {
        (Seq<Error> fails, Seq<Unit> _) = releases.Map(static release => Try.lift(release).Run().Bind(static inner => inner)).Partition();
        return fails.IsEmpty ? Fin.Succ(unit) : Fin.Fail<Unit>(Error.Many(fails));
    }
    public static Fin<Unit> Release<THeld>(Seq<THeld> held, Func<THeld, Fin<Unit>> release) =>
        Release(held.Rev().Map(row => (Func<Fin<Unit>>)(() => release(row))).Strict());

    public static Fin<Unit> Dispose<THeld>(Seq<THeld> held) where THeld : class, IDisposable =>
        Release(held: held, release: static resource => Released(resource));

    public static Fin<Unit> Released(IDisposable resource) =>
        Try.lift(() => { resource.Dispose(); return unit; }).Run();

    public static Fin<T> Rollback<T>(this Fin<T> fold, params ReadOnlySpan<IDisposable?> held) =>
        fold.IsSucc ? fold : Combined(fold, Released(held));

    public static Fin<T> Rollback<T>(this Fin<T> fold, Func<Fin<Unit>> release) =>
        fold.IsSucc ? fold : Combined(fold, Release(Seq(release)));
    public static Fin<T> Rollback<T, THeld>(this Fin<T> fold, Seq<THeld> held, Func<THeld, Fin<Unit>> release) =>
        fold.IsSucc ? fold : Combined(fold, Release(held: held, release: release));

    public static Fin<T> Settled<T>(this Fin<T> primary, Func<Fin<Unit>> release) =>
        Combined(primary, Release(Seq(release)));
    public static Fin<T> Settled<T, THeld>(this Fin<T> primary, Seq<THeld> held, Func<THeld, Fin<Unit>> release) =>
        Combined(primary, Release(held: held, release: release));

    public static Fin<T> Bracket<T>(Func<Fin<T>> body, params ReadOnlySpan<IDisposable?> held) =>
        Combined(Try.lift(body).Run().Bind(static inner => inner), Released(held));

    public static Fin<T> Bracket<TResource, T>(Func<TResource> acquire, Func<TResource, Fin<T>> project)
        where TResource : class, IDisposable =>
        Try.lift(acquire).Run().Bind(held => Bracket(() => project(held), held));

    static Fin<T> Combined<T>(Fin<T> primary, Fin<Unit> released) =>
        released.Match(
            Succ: _ => primary,
            Fail: cleanup => primary.Match(Succ: _ => Fin.Fail<T>(cleanup), Fail: cause => Fin.Fail<T>(cause + cleanup)));

    static Fin<Unit> Released(ReadOnlySpan<IDisposable?> held) {
        Fin<Unit> outcome = Fin.Succ(unit);
        for (int slot = held.Length - 1; slot >= 0; slot--) {
            if (held[slot] is not { } handle) { continue; }
            outcome = Released(handle).Match(
                Succ: _ => outcome,
                Fail: fault => outcome.Match(Succ: _ => Fin.Fail<Unit>(fault), Fail: prior => Fin.Fail<Unit>(prior + fault)));
        }
        return outcome;
    }
}
```

## [05]-[TRANSITION]

- Owner: `Transition<TState>` the verdict every lock-free transition owes its caller, and `Cell` the four transition shapes every `Atom` consumer composes.
- Cases: `Committed` this caller's proposal landed, `Ceded` another contender held the seat, `Refused` the cell declined outright, `Contended` the attempt budget spent with nothing landed. `Current` projects the post-state off every case, so a caller reading only the state never branches; a stored `bool Won` is the deleted form because `is Committed` is the probe.
- Entry: `Claim` is first-writer-wins over a keyed table, `Seat` first-writer over a single slot, `Step` a guarded transition whose step declines with `None` and cedes when its observed state moved, `Commit` an expensive transition computed outside the cell and committed by snapshot comparison — budget-free at the kernel default `SwapBudget`, which is why no consumer mints a one-member budget shell of its own — `Take` the take-and-clear whose `Committed` payload is the DRAINED value, `Converge` the bounded ITERATE checking explicit settlement before the first step and after every commit, returning `Refused` when a next step declines, `Ceded` when another writer moves the state, and `Contended` when the budget exhausts — `Swap(_ => empty)` returns the empty post-state and is the retired spelling.
- Law: `Commit` bounds CAS ATTEMPTS, never iterations, while `Converge` bounds committed steps: completion is a committed state satisfying `settled`; `None` is a refusal, and every terminal verdict carries the state the caller reads.
- Law: the candidate mints ONCE outside the transition. Compare-and-swap bodies re-run on every contended retry, so a capsule minted, a rank incremented, or a tally swapped from inside one burns identity or counts attempts instead of commits — the hazard three host pages document by hand and this owner retires.
- Law: a swap returning only the new value reports success to every contender, so the losing writer proceeds on a decision it never won; the refusable swap is no better, answering the post-state rather than whether the step was taken. Every shape here returns the verdict beside the state and `ignore(cell.Swap(...))` is the deleted spelling corpus-wide.
- Law: `Commit` bounds its attempts with a `Dimension` and returns `Contended` — a TYPED exhaustion carrying the spent budget, never a success-shaped fall-through that certifies an uncommitted state as committed.
- Exemption: compare-and-swap bodies and the bounded `Commit`/`Converge` drivers are the mechanism's statement region — each pass reads a fresh verdict, contained HERE so no consumer writes one.
- Law: the keyed shape takes a whole-value `Atom<HashMap<TKey, TValue>>` rather than the key-grained cell, because that cell's find-or-add answers `Option<TValue>` — a `Some` that cannot distinguish the seater from the finder, which is the exact decision this owner exists to carry; consumers threading non-keyed transitions over the same cell also keep one cell type.
- Growth: a new transition shape is one `Cell` member over the same verdict; a new verdict is one `Transition` case with every consumer's `Switch` loudly broken.
- Packages: LanguageExt.Core (`Atom`, `HashMap`, `Option`), Thinktecture.Runtime.Extensions (`[Union]`), `Rasm.Numerics` (`Dimension`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Numerics;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record Transition<TState> {
    private Transition() { }
    public sealed record Committed(TState State) : Transition<TState>;
    public sealed record Ceded(TState State) : Transition<TState>;
    public sealed record Refused(TState State, Error Cause) : Transition<TState>;
    public sealed record Contended(TState State, Dimension Attempts) : Transition<TState>;
    public TState Current => Switch(
        committed: static row => row.State,
        ceded: static row => row.State,
        refused: static row => row.State,
        contended: static row => row.State);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cell {
    public static readonly Dimension SwapBudget = Dimension.Create(value: 8);

    public static Transition<HashMap<TKey, TValue>> Claim<TKey, TValue>(Atom<HashMap<TKey, TValue>> cell, TKey key, Func<TValue> mint) {
        TValue candidate = mint();
        bool seated = false;
        HashMap<TKey, TValue> settled = cell.Swap(held => {
            seated = !held.ContainsKey(key);
            return seated ? held.Add(key, candidate) : held;
        });
        return seated
            ? (Transition<HashMap<TKey, TValue>>)new Transition<HashMap<TKey, TValue>>.Committed(State: settled)
            : new Transition<HashMap<TKey, TValue>>.Ceded(State: settled);
    }

    public static Transition<Option<TValue>> Seat<TValue>(Atom<Option<TValue>> cell, Func<TValue> mint) {
        TValue candidate = mint();
        bool seated = false;
        Option<TValue> settled = cell.Swap(held => {
            seated = held.IsNone;
            return seated ? Some(candidate) : held;
        });
        return seated
            ? (Transition<Option<TValue>>)new Transition<Option<TValue>>.Committed(State: settled)
            : new Transition<Option<TValue>>.Ceded(State: settled);
    }

    public static (Transition<Option<TValue>> Transition, Option<TToken> Token) Seat<TValue, TToken>(Atom<Option<TValue>> cell, Func<(TValue Value, TToken Token)> mint) {
        (TValue candidate, TToken token) = mint();
        bool seated = false;
        Option<TValue> settled = cell.Swap(held => {
            seated = held.IsNone;
            return seated ? Some(candidate) : held;
        });
        return seated
            ? (new Transition<Option<TValue>>.Committed(State: settled), Some(token))
            : ((Transition<Option<TValue>>)new Transition<Option<TValue>>.Ceded(State: settled), Option<TToken>.None);
    }

    public static Transition<TState> Step<TState>(Atom<TState> cell, Func<TState, Option<TState>> step, Error declined) {
        TState seen = cell.Value;
        Option<TState> candidate = step(arg: seen);
        if (candidate.IsNone) { return new Transition<TState>.Refused(State: seen, Cause: declined); }

        bool landed = false;
        TState settled = cell.SwapMaybe(current => {
            landed = EqualityComparer<TState>.Default.Equals(x: current, y: seen);
            return landed ? candidate : None;
        });
        return landed
            ? (Transition<TState>)new Transition<TState>.Committed(State: settled)
            : new Transition<TState>.Ceded(State: settled);
    }

    public static Transition<Seq<TValue>> Take<TValue>(Atom<Seq<TValue>> cell) {
        Seq<TValue> drained = Seq<TValue>();
        ignore(cell.Swap(held => { drained = held; return Seq<TValue>(); }));
        return new Transition<Seq<TValue>>.Committed(State: drained);
    }
    public static Transition<Option<TValue>> Take<TValue>(Atom<Option<TValue>> cell) {
        Option<TValue> drained = None;
        ignore(cell.Swap(held => { drained = held; return None; }));
        return new Transition<Option<TValue>>.Committed(State: drained);
    }
    public static Transition<TState> Commit<TState>(Atom<TState> cell, Func<TState, TState> compute, Dimension budget) {
        for (int attempt = 0; attempt < budget.Value; attempt++) {
            TState seen = cell.Value;
            TState next = compute(arg: seen);
            bool landed = false;
            TState settled = cell.SwapMaybe(current => {
                landed = EqualityComparer<TState>.Default.Equals(x: current, y: seen);
                return landed ? Some(next) : None;
            });
            if (landed) { return new Transition<TState>.Committed(State: settled); }
        }
        return new Transition<TState>.Contended(State: cell.Value, Attempts: budget);
    }
    public static Transition<TState> Commit<TState>(Atom<TState> cell, Func<TState, TState> compute) =>
        Commit(cell: cell, compute: compute, budget: SwapBudget);

    public static Transition<TState> Converge<TState>(
        Atom<TState> cell,
        Func<TState, Option<TState>> step,
        Func<TState, bool> settled,
        Dimension budget,
        Error declined) {
        if (settled(cell.Value)) { return new Transition<TState>.Committed(State: cell.Value); }
        for (int attempt = 0; attempt < budget.Value; attempt++) {
            Transition<TState> transition = Step(cell: cell, step: step, declined: declined);
            if (transition is Transition<TState>.Refused refused) { return refused; }
            if (transition is Transition<TState>.Ceded ceded) { return ceded; }
            if (transition is Transition<TState>.Committed committed && settled(committed.State)) { return committed; }
        }
        return new Transition<TState>.Contended(State: cell.Value, Attempts: budget);
    }
}
```

## [06]-[VALIDITY_FOLD]

- Owner: `IValidityEvidence` the corpus-wide evidence floor (one member, `IsValid`) every kernel result and carrier implements, and `ValidityClaim` the claim vocabulary whose `All` fold is the one mechanism a result's `IsValid` body composes.
- Entry: a result spells `public bool IsValid => ValidityClaim.All(...)` over its claim rows and bare predicates alike — the implicit `bool -> ValidityClaim` conversion binds a comparison, a nested result's own fold, and a count test at every arity, so `ValidityClaim.Of(` is the deleted spelling corpus-wide and no call site wraps.
- Law: one claim vocabulary states each predicate once; a result declares which claims hold, never how a predicate is computed.
- Law: predicate policy is named once here — the scalar `Finite` is `RhinoMath.IsValidDouble`, screening both non-finite values and the host `RhinoMath.UnsetValue` sentinel because scalar fields on kernel results can carry host-read material; the span `Finite` is the vectorized `TensorPrimitives.IsFiniteAll` gate, correct for solver-produced arrays that never carry the host sentinel; the quantity `Finite` screens a `UnitsNet` carrier through its own scalar, and a decimal-backed quantity is finite by construction so the double projection loses nothing it measures. One generic numeric witness over all of these is the REJECTED collapse: it silently drops the sentinel screen the scalar arm declares load-bearing.
- Law: the host coordinate arms are RhinoCommon's own component-wise sentinel screen — `Point3d.IsValid`/`Vector3d.IsValid` read through ONE kernel arm per struct, so the predicate for a host coordinate keeps a single kernel site and a consumer never spells `.IsValid` beside the claim vocabulary; a fourth host struct is one more arm, never a generic witness that drops the sentinel screen.
- Law: absence is an `Option`, never a nullable result reference — `Evidence<T>(Option<T>)` reads an absent nested RESULT as non-falsifying and `WhenPresent<T>(Option<T>, claim)` is its general sibling over any facet claim, so no consumer hand-folds `facet.Map(...).IfNone(true)`. NAMED LOSS: a present result no longer reaches a named member; it spells `result.IsValid` through the conversion, which is the same fold with the kernel vocabulary dropped at that one site.
- Law: implementing `IValidityEvidence` registers a result with the acceptance oracle — `Acceptance.ValidityOf` (`validation.md`) probes the one `IValidityEvidence` arm ahead of every category default, so a result also inhabiting a blanket-admitted category still answers through its own fold and a new result reaches the oracle with zero oracle edits.
- Law: keyed distinctness folds ONCE. `Collisions` is the count-then-filter every "one row per key" refusal stands on — a roster freeze, a mount table, an arm table, an alert-namespace proof — so a site supplies its key projection and its own `KernelFault.InvalidValue` text and re-spells no fold; a hand-rolled count map beside a roster is the deleted form.
- Growth: a new predicate is one claim row — the scalar guard (`validation.md`) takes the claim, so the guard family widens HERE and no guard surface grows a member.
- Boundary: the fold is validity evidence, never admission — admission rejects raw material at the boundary with typed faults (`validation.md`), the fold answers whether an already-constructed result carries coherent evidence. `All`'s span loop is the named kernel exemption.
- Packages: RhinoCommon (`RhinoMath`), System.Numerics.Tensors (`TensorPrimitives`), UnitsNet (`IQuantity`, `QuantityValue`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics.Tensors;
using Rhino;
using UnitsNet;

namespace Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IValidityEvidence { public bool IsValid { get; } }

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct ValidityClaim(bool Holds) {
    public static ValidityClaim Finite(double value) => new(Holds: RhinoMath.IsValidDouble(x: value));
    public static ValidityClaim Finite<T>(T value) where T : IQuantity => new(Holds: double.IsFinite(d: (double)value.Value));
    public static ValidityClaim Finite(ReadOnlySpan<double> values) => new(Holds: TensorPrimitives.IsFiniteAll(values));
    public static ValidityClaim Finite(Point3d value) => new(Holds: value.IsValid);
    public static ValidityClaim Finite(Vector3d value) => new(Holds: value.IsValid);
    public static ValidityClaim Direction(Vector3d value) => new(Holds: value.IsValid && !value.IsZero);
    public static ValidityClaim Nonnegative(double value) => new(Holds: RhinoMath.IsValidDouble(x: value) && value >= 0.0);
    public static ValidityClaim Positive(double value) => new(Holds: RhinoMath.IsValidDouble(x: value) && value > 0.0);
    public static ValidityClaim UnitInterval(double value) => new(Holds: RhinoMath.IsValidDouble(x: value) && value is >= 0.0 and <= 1.0);
    public static ValidityClaim Ordered(double lower, double upper) => new(Holds: RhinoMath.IsValidDouble(x: lower) && RhinoMath.IsValidDouble(x: upper) && lower <= upper);
    public static ValidityClaim CountAtLeast(int count, int floor) => new(Holds: count >= floor);
    public static ValidityClaim CountExactly(int count, int expected) => new(Holds: count == expected);
    public static ValidityClaim Evidence<T>(Option<T> evidence) where T : IValidityEvidence => WhenPresent(evidence, static value => value.IsValid);
    public static ValidityClaim WhenPresent<T>(Option<T> facet, Func<T, ValidityClaim> claim) => facet.Map(claim).IfNone(true);
    public static ValidityClaim All(params ReadOnlySpan<ValidityClaim> claims) {
        foreach (ValidityClaim claim in claims) {
            if (!claim.Holds) { return new(Holds: false); }
        }
        return new(Holds: true);
    }
    public static implicit operator ValidityClaim(bool holds) => new(Holds: holds);
    public static implicit operator bool(ValidityClaim claim) => claim.Holds;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class RosterFold {
    internal static Seq<TKey> Collisions<T, TKey>(this Seq<T> rows, Func<T, TKey> key) where TKey : notnull =>
        rows.Fold(
                HashMap<TKey, int>(),
                (held, row) => held.AddOrUpdate(key(arg: row), static seats => seats + 1, static () => 1))
            .AsIterable().Filter(static seat => seat.Value > 1).Map(static seat => seat.Key).ToSeq().Strict();
}
```

## [07]-[CARRIAGE_LAW]

One carriage law rules every kernel page; no page re-decides it.

- Law: `Eff<Env>` is the runtime CARRIAGE — a pipeline needing tolerance context, progress, or cancellation is `Eff<Env, T>` composing `Env.Asks`/`Env.EnvAsks`.
- Law: below the `Eff` floor the synchronous owners thread `Context` and `CancellationToken` as explicit parameters (`Requirement.Apply(context, value, cancel)` is the canonical shape); at the floor and above, `Env` carries both. One operation is written in exactly one paradigm — a `Fin`/`Validation` body, or an `Eff<Env, T>` pipeline.
- Owner: `HostEdge` is the ONE crossing vocabulary between kernel carriers and a host that speaks `null`, `void`, and `ref`, plus `Captured`/`CapturedIO`, the ASYNCHRONOUS funnel LanguageExt's `Try` has no twin for. `Slot`/`Nullable` project `Option<T>` onto a host reference or nullable slot — `Slot` IS `ValueUnsafe`, which answers `null` only for a reference element, so the struct arm keeps its own `Match` — `NonEmpty` admits a host string back as `Option<string>` under a LENGTH predicate and no trim, which is what separates it from `validation.md`'s trimming `Acceptance.Text`; `Side` lifts a void host call onto `Unit` so a statement composes as an expression, and `Settle` writes a `Fin` success into a host `ref` slot and answers whether it landed. A conditional side effect spells `condition ? Side(f) : unit` at the site or an `if` at statement position — a second member for a ternary is the deleted form, and the `bool`-plus-`out` crossing belongs to `validation.md`'s `Admit.Probe`, which states a requirement this owner cannot.
- Law: optional context is `Option<T> x = default` consumed through `IfNone` against its policy owner's canonical row; `T? x = null` optional tails are the deleted form kernel-wide. `HostEdge.Slot`/`Nullable` are the ONE place `null` is a legal spelling — a host slot the domain never reads back — so no host-facing page hand-spells the `Option` → `null` projection.
- Law: `Captured` takes its token FIRST and REQUIRED — an optional tail defaults to `CancellationToken.None`, which silently retires the cancel arm and the deadline above it, and the corpus spells the token-carrying lift once as `CapturedIO`, reading `EnvIO.Token` itself so no consumer re-writes the two-lift sandwich or drops the token on the way through. `Try.lift(f).Run()` funnels a SYNCHRONOUS host call and `Captured` its awaited twin, and every other exception stays the exact captured `Exceptional` so a classifier composes AFTER the funnel and never inside it. `Captured` lands a PROVEN cancellation on `KernelFault.Cancelled` carrying the raised exception, because an unrequested or tokenless cancel is a library's own and never the caller's; `Try.lift` has no token to prove it with and normalizes onto the package `Errors.Cancelled` identity instead, so a recovery predicate reads the case and the code alike. The capture widens the caught exception to `Exception` before `Error.New`, which the two-argument overload requires statically.
- Boundary: this owner holds TWO rungs, and the second is named rather than exempted. The SHAPE rung — `Slot`, `Nullable`, `NonEmpty`, `Side`, `Settle` — is total, mints no fault, reads no policy, and answers `Option<T>` where a crossing can be absent, its refusal the caller's own typed fault. The CAPTURE rung — `Captured`/`CapturedIO` — mints `KernelFault.Cancelled`, reads the token, and lands `Fin`, because an awaited host call has no other place to become a carrier.
- Law: telemetry is a TAP, never a result — the `TelemetrySink` (`telemetry.md`) rides `Env` at the `Eff` floor or enters a synchronous gate point as one explicit trailing parameter beside `Context`/`CancellationToken`; facts publish through its one `Tap`, and an observe-side subscriber fault isolates onto the tap's own cell, never failing the tapped operation.
- Boundary: `Env` is `Analysis/query.md`'s frozen record — this page legislates the carriage law, that page owns the record and the pipeline shape.
- Packages: LanguageExt.Core (`Option`, `Fin`, `Unit`); BCL inbox.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt.UnsafeValueAccess;

namespace Rasm.Domain;

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class HostEdge {
    public static T? Slot<T>(Option<T> value) where T : class => value.ValueUnsafe();
    public static T? Nullable<T>(Option<T> value) where T : struct => value.Match(Some: static held => (T?)held, None: static () => (T?)null);
    public static Option<string> NonEmpty(string? value) => Optional(value).Filter(static text => text.Length > 0);
    public static Unit Side(Action action) { action(); return unit; }

    public static bool Settle<T>(ref T slot, Fin<T> outcome) {
        if (outcome.Case is T value) { slot = value; return true; }
        return false;
    }

    public static async ValueTask<Fin<T>> Captured<T>(CancellationToken token, Func<CancellationToken, ValueTask<Fin<T>>> body) {
        try { return await body(token).ConfigureAwait(false); }
        catch (OperationCanceledException raised) when (token.IsCancellationRequested) { return Fin.Fail<T>(new KernelFault.Cancelled(Error.New(raised.Message, (Exception)raised))); }
        catch (Exception raised) { return Fin.Fail<T>(Error.New(raised.Message, raised)); }
    }

    public static IO<Fin<T>> CapturedIO<T>(Func<CancellationToken, ValueTask<Fin<T>>> body) =>
        IO.liftVAsync(env => Captured(env.Token, body));
}
```

## [08]-[CARRIER_CODEC]

- Owner: `LanguageExtJsonConverterFactory` with its `CollectionJsonConverter<TCarrier, T>`/`OptionJsonConverter<T>`/`HashMapJsonConverter<K, V>` rows — the ONE carrier-space System.Text.Json owner for the LanguageExt collections any wire crosses. Homed at the kernel because every stratum carries `Rasm`: the S1 suite merge (`Rasm.AppHost/Runtime/ports#WIRE_LAW` `SuiteContracts.Wire`) and the S2 `Rasm.Persistence/Element/codec` `ElementJson` mint each REGISTER this one type; an S1 home left the S2 graph — whose reference set is `{Rasm, Rasm.Element}` — unable to name it, which is the strata violation that forced the move.
- Cases: admission is ONE `Carriers` table read by both `CanConvert` and `CreateConverter`, so a new carrier is one row and never a second predicate clause that drifts from what the mint produces. Every array-shaped carrier shares ONE converter: a set and a sequence differ in admission, not in wire form, so the row names the closing shape and no second class exists — `Seq`, `Set`, `Arr`, `Lst`, and `HashSet` all read `CarrierRow.Collection`.
- Auto: the element builder DERIVES from the carrier's own `[CollectionBuilder]` attribute, so the roster has no builder column to keep in step with the carrier set; the bind runs once per closed type in a generic static and a carrier without the attribute fails at type initialization rather than at the first decode.
- Law: the mint is memoized on the page's own claim transition (`[07]`), so `Activator.CreateInstance` runs once per closed carrier type rather than once per `CreateConverter` call, and a contended first mint seats exactly one instance.
- Law: every element round-trips through the SAME options the carrier was resolved from, so nesting composes — a `Seq<T>` whose rows carry `Instant` or generated owners still reaches the NodaTime and Thinktecture converters the registering mint carries, and a carrier converter re-implementing an element codec is the deleted form. Each container's wire shape is exactly what the underlying BCL member emits — array for the collection carriers, object for `HashMap`, the bare value for `Option` — so wrapping the elements in a message envelope is the rejected form.
- Law: `Option<T>` serves BOTH emission postures without a knob — under a resolver carrying an `OmitAbsent`-class modifier (the suite merge) the write leg never sees a `None`; under an explicit-null mint (the S2 graphs, defaulting `JsonIgnoreCondition.Never`) a `None` writes `null` and the read leg admits `null` symmetrically. One converter serves every mint, and the emission posture is the registering RESOLVER's contract, never this owner's.
- Boundary: registration is the mint's obligation and this owner registers nothing — a per-member `[JsonConverter]` attribute over these carriers and a second factory declaration are the deleted forms at every mint. LanguageExt ships no System.Text.Json support and its carriers are structurally unpopulatable by the serializer — each is an immutable readonly struct whose `[CollectionBuilder]` hook the serializer never reads and whose `Add` returns a NEW value — so without this factory the READ leg fails on every member while the write leg succeeds, the silent decode-failure shape.
- Packages: LanguageExt.Core; BCL inbox (`System.Text.Json`, `System.Collections.Frozen`, `System.Reflection`).
- Growth: a new carrier is ONE `Carriers` row — the array shapes name the shared collection row, a distinct wire shape names its own converter. `Map<,>` is the one carrier still outside the table: it is ordered-keyed, so it lands as its own `Shaped` row with a converter that rebuilds through `toMap` rather than borrowing `HashMap`'s.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.Domain;

// --- [TABLES] --------------------------------------------------------------------------
public sealed class LanguageExtJsonConverterFactory : JsonConverterFactory {
    static readonly FrozenDictionary<Type, CarrierRow> Carriers = new Dictionary<Type, CarrierRow> {
        [typeof(Seq<>)] = CarrierRow.Collection,
        [typeof(Set<>)] = CarrierRow.Collection,
        [typeof(Arr<>)] = CarrierRow.Collection,
        [typeof(Lst<>)] = CarrierRow.Collection,
        [typeof(LanguageExt.HashSet<>)] = CarrierRow.Collection,
        [typeof(Option<>)] = CarrierRow.Shaped(converter: typeof(OptionJsonConverter<>)),

        [typeof(HashMap<,>)] = CarrierRow.Shaped(converter: typeof(HashMapJsonConverter<,>)),
    }.ToFrozenDictionary();

    static readonly Atom<HashMap<Type, Lazy<JsonConverter>>> Minted = Atom(HashMap<Type, Lazy<JsonConverter>>());

    public override bool CanConvert(Type type) =>
        type.IsGenericType && Carriers.ContainsKey(type.GetGenericTypeDefinition());

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options) =>
        Cell.Claim(cell: Minted, key: type, mint: () => new(valueFactory: () => {
            CarrierRow row = Carriers[type.GetGenericTypeDefinition()];
            return (JsonConverter)Activator.CreateInstance(type: row.Converter.MakeGenericType(row.Close(type)))!;
        }, mode: LazyThreadSafetyMode.ExecutionAndPublication)).Current[type].Value;

    readonly record struct CarrierRow(Type Converter, Func<Type, Type[]> Close) {
        public static readonly CarrierRow Collection =
            new(Converter: typeof(CollectionJsonConverter<,>), Close: static type => [type, type.GetGenericArguments()[0]]);
        public static CarrierRow Shaped(Type converter) =>
            new(Converter: converter, Close: static type => type.GetGenericArguments());
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public delegate TCarrier CarrierBuild<TCarrier, T>(ReadOnlySpan<T> rows);

public sealed class CollectionJsonConverter<TCarrier, T> : JsonConverter<TCarrier> where TCarrier : IEnumerable<T> {
    static readonly CarrierBuild<TCarrier, T> Build = Bind();

    static CarrierBuild<TCarrier, T> Bind() {
        CollectionBuilderAttribute hook = typeof(TCarrier).GetCustomAttribute<CollectionBuilderAttribute>()!;
        return hook.BuilderType
            .GetMethod(name: hook.MethodName, genericParameterCount: 1, types: [typeof(ReadOnlySpan<>).MakeGenericType(Type.MakeGenericMethodParameter(0))])!
            .MakeGenericMethod(typeof(T))
            .CreateDelegate<CarrierBuild<TCarrier, T>>();
    }

    public override TCarrier Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        Build(JsonSerializer.Deserialize<T[]>(ref reader, options) ?? []);

    public override void Write(Utf8JsonWriter writer, TCarrier value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        foreach (T item in value) { JsonSerializer.Serialize(writer, item, options); }
        writer.WriteEndArray();
    }
}

public sealed class OptionJsonConverter<T> : JsonConverter<Option<T>> {
    public override Option<T> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        reader.TokenType is JsonTokenType.Null
            ? Option<T>.None
            : Optional(JsonSerializer.Deserialize<T>(ref reader, options));

    public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options) {
        foreach (T item in value.AsEnumerable()) { JsonSerializer.Serialize(writer, item, options); return; }
        writer.WriteNullValue();
    }
}

public sealed class HashMapJsonConverter<K, V> : JsonConverter<HashMap<K, V>> where K : notnull {
    public override HashMap<K, V> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        toHashMap((JsonSerializer.Deserialize<Dictionary<K, V>>(ref reader, options) ?? [])
            .Select(static entry => (entry.Key, entry.Value)));

    public override void Write(Utf8JsonWriter writer, HashMap<K, V> value, JsonSerializerOptions options) {
        JsonConverter<K> keys = (JsonConverter<K>)options.GetConverter(typeof(K));
        writer.WriteStartObject();
        foreach ((K key, V item) in value.AsIterable()) {
            keys.WriteAsPropertyName(writer, key, options);
            JsonSerializer.Serialize(writer, item, options);
        }
        writer.WriteEndObject();
    }
}
```

## [09]-[DENSITY_BAR]

One substrate floor; growth is a case, a band row, a claim row, or a carrier row, never a sibling owner.

| [INDEX] | [CONCERN]              | [OWNER]                               | [KIND]                                  | [RESULT]                    |
| :-----: | :--------------------- | :------------------------------------ | :-------------------------------------- | :-------------------------- |
|  [01]   | Fault code space       | `FaultBand`                           | `[SmartEnum<int>]` branch-wide registry | `band → int` + `int → band` |
|  [02]   | Substrate faults       | `Fault` + `KernelFault`               | typed payloads under generated identity | `Fault → Error` subtype     |
|  [03]   | Retriability + redrive | `Retriability` + `Redrive`            | `[Union]` discriminant + policy value   | `Error → Verdict`/`IO<T>`   |
|  [04]   | Resource ownership     | `Lease<T>` + `Custody`                | `[Union]` Owned/Borrowed + release fold | `Lease<T>.Use → TResult`    |
|  [05]   | Transition verdict     | `Transition<TState>` + `Cell`         | `[Union]` verdict + four CAS shapes     | `Atom<T> → Transition<T>`   |
|  [06]   | Result validity        | `IValidityEvidence` + `ValidityClaim` | evidence floor + claim fold             | `ValidityClaim.All → bool`  |
|  [07]   | Host crossing          | `HostEdge`                            | shape projection + awaited capture      | `Option<T> → T?`/`void → Unit`/`await → Fin` |
|  [08]   | Carrier codec          | `LanguageExtJsonConverterFactory`     | closed carrier-to-converter table       | mint registers, wire rides  |

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
