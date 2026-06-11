# [DOMAIN_CHARTER]

Build charter for the seven `domain/` concept pages. This file is the planning carrier for the subfolder: everything a build session needs to start without rediscovery. Each page's scope moves into the page when it is authored; when all seven pages are active, this file reduces to routing. The charter is fixed — a light user confirmation precedes the first build, and there is no re-scoping at that gate.

## [1]-[BUILD_ORDER]

Strict dependency order; each later page implicitly carries the earlier law and never re-teaches it. Build starts only after every root page in the parent folder is finalized through the corpus sweep and compile harness.

| [INDEX] | [PAGE]           | [OWNS]                                             | [PRIMARY_PACKAGES]                                      |
| :-----: | :--------------- | :------------------------------------------------- | :------------------------------------------------------ |
|   [1]   | `runtime.md`     | hosting, DI composition, options, caching, time    | Scrutor, Hosting/Options, HybridCache, NodaTime         |
|   [2]   | `concurrency.md` | threading, channels, reactive streams, parallelism | Channels, System.Reactive, DynamicData                  |
|   [3]   | `diagnostics.md` | logging, traces, metrics, redaction, correlation   | Serilog, OpenTelemetry, Extensions.Telemetry, Redaction |
|   [4]   | `validation.md`  | boundary-shape validation ownership                | FluentValidation                                        |
|   [5]   | `resilience.md`  | transport and boundary resilience pipelines        | Polly, Http.Resilience                                  |
|   [6]   | `persistence.md` | relational doctrine, provider-polymorphic          | EF Core, Npgsql, SQLite stack, BulkExtensions           |
|   [7]   | `compute.md`     | tensors, measured dispatch, remote compute lanes   | Numerics.Tensors, Grpc.Net.Client, Protobuf             |

## [2]-[CUMULATIVE_LAW]

The order is load-bearing because pages are layers of one body: every page is authored from the full established law of every page finalized before it — all root pages first, then each earlier domain page — so later pages spend zero lines re-deriving and every line on their own layer. Cumulative depth is what makes a 300-line page sufficient.

[PROSE]:
- A page consumes earlier layers as given material: admitted owners, typed rails, policy values, boundary capsules, the runtime spine arrive settled. It never re-teaches a prior layer, never names a sibling page, and never hedges a rule a prior layer decided — the reader is assumed to hold the whole stack beneath.
- No external sources, ever: pages carry zero links, citations, version narration, or source mentions. External truth is verified in the workspace — API maps, installed package source, current docs — before a claim lands; the page then states the law as fact.

[SNIPPETS]:
- A snippet first fully captures its own card — every load-bearing clause has a line. Its supporting material is then drawn from layers already finalized: a resilience snippet rides a typed rail and a policy value as given; a persistence snippet persists time through the calendar vocabulary the runtime layer established. The composition is silent — no comment narrates what came from where.
- The region map is corpus-wide and cumulative: a domain snippet demonstrates a surface region no root or earlier domain snippet owns, while freely using earlier regions' surfaces as background material. New region in the spotlight, established law in the supporting cast — that is the difference between stacking and duplication.

## [3]-[PER_PAGE_SCOPE]

[RUNTIME]:
- Hosting lifecycle; DI composition with assembly scanning and decoration (Scrutor) and keyed services; options and configuration with AOT-safe options validation; HybridCache; one process cancellation spine; time as clock abstraction (TimeProvider) plus calendar vocabulary (NodaTime).
- Establishes the spine every later page assumes as given: one cancellation root with derived child scopes, one clock, one calendar vocabulary, options as policy values.
- Assumes boundary state law and effect rails; never re-teaches state cells or carrier choice.

[CONCURRENCY]:
- Threading law, structured concurrency, `System.Threading.Channels`, reactive streams (System.Reactive, DynamicData) admitted only where they change the design choice, atomic/STM state cells at concurrency scope, parallelism policy.
- Disjoint: the boundaries page owns host-thread marshaling; the rails page owns the effect rails concurrency composes.

[DIAGNOSTICS]:
- One file: structured logging (Serilog + hosting integration over Logging.Abstractions), traces and metrics (OpenTelemetry + hosting), sampling and enrichment governance (Microsoft.Extensions.Telemetry), PII redaction (Compliance.Redaction), one correlation spine across all signals.
- Seam with boundaries: in-process observability is write-only through platform primitives; exporters live at out-of-process roots — this page owns what signals carry, the boundaries page owns where exporters may live.
- Extended-cap candidate (see §7).

[VALIDATION]:
- FluentValidation owns wire-DTO, options, and input validation. The triad law is the page's spine: generated validation partials admit value objects; `Validation<E,T>` rails own domain accumulation; the boundary validator owns wire shapes — which validator owns which seam is the decision the page exists to make.
- Seam with runtime: runtime owns the options-binding and startup-validation mechanism; this page owns which validator rules an options shape and how its failures join the typed rail.
- Assumes shapes and rails; never re-teaches them.

[RESILIENCE]:
- Polly `ResiliencePipeline` for non-HTTP seams (database, RPC, native) and Microsoft.Extensions.Http.Resilience for HTTP.
- Seam law stated as doctrine: domain-internal retry and repeat is `Schedule` policy on effect rails; transport and boundary resilience is a pipeline at the seam — never both on one seam. Lower layers detecting a second retry owner emit conflict evidence.

[PERSISTENCE]:
- EF Core doctrine, provider-polymorphic across SQLite and PostgreSQL (Npgsql with NodaTime mapping; SQLite temporal via value converters) as one case axis inside ONE file — provider variance is never a file split.
- Compiled models, JSON columns, complex types, interceptors, naming conventions, migrations, bulk movement (EFCore.BulkExtensions), integrity, snapshots, retention.
- Composes the boundaries page's lifecycle law for store open, drain, corruption fencing, and newer-schema rejection; storage naming stays internal — public code addresses entity kinds, never physical names.
- Extended-cap candidate (see §7).

[COMPUTE]:
- Tensors at application scope (System.Numerics.Tensors, generic math); measured dispatch with typed receipts; remote compute lanes over typed contracts (Grpc.Net.Client + Google.Protobuf); a model-inference lane.
- The inference runtime is UNVERIFIED — research could not confirm a current ONNX Runtime .NET release; re-verify at this page's research stage and scope the lane to what installed source proves.
- Disjoint: the algorithms page owns numeric route law; this page owns application-scope compute composition.

## [4]-[PACKAGE_RESIDENCY]

Every external library has exactly one doctrine home; a page names a package only where it changes the implementation choice. Cross-references between pages are implicit — only README files link.

| [INDEX] | [PACKAGE]                                                | [HOME]      | [QUEUED_VERSION] |
| :-----: | :------------------------------------------------------- | :---------- | :--------------- |
|   [1]   | Scrutor                                                  | runtime     | 7.0.0            |
|   [2]   | Microsoft.Extensions.Caching.Hybrid                      | runtime     | 10.7.0           |
|   [3]   | NodaTime (+ TimeProvider law)                            | runtime     | admitted         |
|   [4]   | System.Threading.Channels                                | concurrency | BCL              |
|   [5]   | System.Reactive                                          | concurrency | admitted         |
|   [6]   | DynamicData                                              | concurrency | admitted         |
|   [7]   | Serilog + Serilog.Extensions.Hosting                     | diagnostics | 4.3.1 / 10.0.0   |
|   [8]   | OpenTelemetry + OpenTelemetry.Extensions.Hosting         | diagnostics | 1.15.3 / 1.15.3  |
|   [9]   | Microsoft.Extensions.Telemetry                           | diagnostics | 10.7.0           |
|  [10]   | Compliance.Redaction + Logging.Abstractions              | diagnostics | admitted         |
|  [11]   | FluentValidation (+ DI extensions)                       | validation  | 12.1.1           |
|  [12]   | Polly                                                    | resilience  | 8.6.6            |
|  [13]   | Microsoft.Extensions.Http.Resilience                     | resilience  | 10.7.0           |
|  [14]   | EF Core + SQLite provider + SQLitePCLRaw + NamingConventions | persistence | admitted     |
|  [15]   | Npgsql.EntityFrameworkCore.PostgreSQL + Npgsql.NodaTime  | persistence | 10.0.2 / latest  |
|  [16]   | EFCore.BulkExtensions                                    | persistence | 10.0.1           |
|  [17]   | NodaTime.Serialization.SystemTextJson, System.IO.Hashing | persistence | admitted         |
|  [18]   | System.Numerics.Tensors                                  | compute     | 10.0.9           |
|  [19]   | Grpc.Net.Client + Google.Protobuf                        | compute     | admitted         |
|  [20]   | ONNX Runtime .NET                                        | compute     | UNVERIFIED       |

- Queued versions were verified 2026-06; re-verify at each page's research stage. Once a package is admitted, `Directory.Packages.props` is truth and its row here expires.
- Scrutor explicitly does NOT belong to the root boundaries page — DI composition is runtime law; boundaries owns host, native, and wire seams only.
- `reactive-ui` is deliberately unowned: will be implemented after all other domain categories are finalized.

## [5]-[PER_PAGE_P0]

Run in this order for every page, before any research lane launches:

1. Light user gate (first page only): confirm this charter plus any adjacency-discovered package additions. No re-scoping.
2. Admit the page's queued packages to `Directory.Packages.props`, run `dotnet restore`, then `uv run python -m tools.assay api doctor` and `resolve` to confirm the surfaces are extractable. Docs lead admission — the package enters the manifest so research can verify installed source, before any production code uses it.
3. API capture: one agent per package family writes `.reports/<page>/_api/<package>.md` — the full public capability inventory (namespaces, key types, options and DI integration points, source-generated surfaces, extension seams) via `uv run python -m tools.assay api query|show`, which is decompiled installed truth; verified-fresh official docs only where the tool cannot reach (analyzer or generator behavior invisible in IL). Agents execute the assay CLI; they never read `tools/` source.
4. Adjacency discovery: the same agents report companion packages worth admitting (hosting integrations, instrumentation, serialization adapters); discoveries surface at the gate for user decision.
5. App-library architecture scouts + sanitizer (the application libraries' ARCHITECTURE and ROADMAP documents only); sanitized pressure reaches research agents exclusively through orchestrator-written lane briefs — research agents never read scout maps.
6. Lane design (orchestrator): 4-6 disjoint lanes per page, at least one package-integration lane per major package family; every lane brief carries an OWNS/ASSUMES disjointness statement derived from the parent roadmap entry, plus the API-map digest and scout pressure inline. Lane agents see no other lane's brief.
7. Research lanes launch with the `_api/` maps added to every lane agent's sanctioned read list.

## [6]-[PIPELINE]

- Each page runs the full greenfield pipeline: research stages — initial (4 waves), hygiene, consolidate-to-bedrock (with cold-grade and targeted-fix loop), divergent (facet-planned: a planner assigns three disjoint facets per lane, then all divergent agents run in one parallel burst with keep-out lists), distill. There is no post-consolidate refine stage.
- Authoring stages — draft (≤225 lines, code-free), ten-lens panel + rewrite (≤250), enrich (<300), optimize (exit ceiling ≤260 for 4 planned snippets / ≤240 for 5-6), snippets (region map written BEFORE code; baseline + 10 isolated attempts + synthesis), snippet refinement to a zero-edit pass.
- Research is parallelizable across pages; authoring is sequential in build order, with page N+1's prose stages overlapping once page N clears its prose red-team loop; snippet stages serialize strictly on the corpus region map.
- Red-team cadence: exactly two loops per page — one on cards/prose after optimize, one as close-out after snippet refinement. Six lenses: meta/hedging/sourcing; context poisoning and placeholder neutrality; intra-page contradiction and unearned card fields; cross-corpus contradiction and region duplication (repairs ROUTE material to the owning page, never delete or duplicate); doctrine-law violations and flat spellings; page grammar and standards compliance. Findings require exact line + exact repair; quiescence = zero accepted findings plus a clean verification pass.
- Bars: 9.2 min-axis at stage admission; 9.5 min-axis snippet finalization, re-checked at the corpus sweep. Grading rubric: `../.reports/_grading.md`. Snippet dedup ledger: `../.reports/_region-map.md`. Page-craft arbiters: the formatting, information-structure, and style-guide standards under `docs/standards/` — tables decompose past 20 rows into axis-led sibling tables.
- Model policy: fast-tier workers for initial/hygiene/consolidate; top-tier for divergent, distill, every cold grade, all authoring, and all red-team lenses.
- Verification budget (binding on every research agent): at most 3 probes per claim, then the claim is unverified — correct to what source proves or delete; decompile an assembly once to a temp file and grep that, never re-decompile per probe.
- Quarantine: research, authoring, snippet, and red-team agents never read `libs/`, `apps/`, `tools/`, `tests/`, `.claude/`, any repository `*.cs`/`*.csproj`, or manifests; the NuGet cache and official docs are their ground truth. Commit per stage: `docs(csharp/domain-<page>): stage-N <name>`.

## [7]-[LOC_AND_CAPS]

- Target 300-350 lines per page; hard maximum 400. Below 300 is acceptable when content-complete — never pad.
- `persistence.md` and `diagnostics.md` are pre-identified extended-cap candidates: when the optimize stage proves the concern irreducible at 400 without losing signal, the cap extends to 480 for that page. Preference order: densify, then extend cap, then split — a split is last resort, must produce concern-disjoint files, and is decided at that page's optimize stage with explicit line arithmetic, never upfront.

## [8]-[KNOWN_RISKS]

- API hallucination on fresh package surfaces is the dominant failure mode; the `_api/` capture maps and the compile harness (active and retroactive from the root corpus sweep) are the arbiters — never trust memory over installed source.
- The assay api rail resolves fuzzy package keys; run `doctor` after every admission batch rather than assuming resolution.
- Version drift: the queued versions above age; each page's research stage re-verifies before admission.
- Ownership bleed: the root pages' red-team lens routes domain-owned material here by name — check the parent README roadmap entries for scope already promised to these pages before re-deciding anything.
- Verification spirals: lanes dense with precise API assertions verify slowest; an agent repeating full-assembly decompiles with zero file edits is spiraling — stop it and relaunch with a bounded brief pointing at the existing `_api/` maps and temp decompiles.
