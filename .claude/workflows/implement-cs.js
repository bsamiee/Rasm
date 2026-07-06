export const meta = {
  name: 'implement-cs',
  whenToUse: 'Realize open IDEAS and TASKLOG cards into design-page code fences across the C# target folders (default: AppHost, Compute, AppUi, Persistence).',
  description: 'Realize every open IDEAS/TASKLOG card across the C# target set (default: Rasm.AppHost, Rasm.Compute, Rasm.AppUi, Rasm.Persistence; any libs/csharp package via args) into deep design-page code FENCES at the docs/stacks/csharp bar, repair every ripple in-pass, and truthfully close the cards. Each target folder runs its OWN discover -> implement -> critique -> redteam chain, ALL chains concurrent under one pooled cap: a folder starts the moment its own discovery lands, a folder with no open cards no-ops after its own discovery, and a failed chain isolates without rejecting the pool. Discovery hands downstream stages navigation FACTS (paths, verified catalog members, seam targets) and never verdicts; it runs read-only on gpt-5.5 dispatched through a sonnet codex wrapper (CODEX flag; false restores the native inherit-model lane); when it proves page-disjoint card groups, the implement stage fans over them. Every stage WRITES and repairs the page-level ripples its own work exposes in the same pass — in-scope seams aligned against current disk, 1-hop out-of-scope C# counterpart fences realized directly — with BLOCKED probes and folder-local package admission inline. The redteam is each folder chain\'s terminal stage and sole card-status owner: it final-remediates weak realizations in place and closes only cards whose realization it verified strong on disk. Two handoffs route to the run\'s terminal single-writer, the central Directory.Packages.props pin and the package ARCHITECTURE.md [02]-[SEAMS] row: folder agents report exact rows, one terminal sonnet writer applies them serially. C#-only. args = a target path string, an array of target paths, or empty for the four defaults.',
  phases: [
    { title: 'Realize', detail: 'all folder chains concurrent under one pooled cap: discover(gpt-5.5 via codex wrapper, read-only navigation facts) -> implement(high; fans over discovery-proven page-disjoint card groups) -> critique(high) -> redteam(high, terminal close); a folder with no open cards no-ops after its own discovery; every writing stage re-reads current disk, repairs page-level ripples in-pass, and reports central pin rows + ARCHITECTURE.md [02]-[SEAMS] rows for the terminal single-writer instead of editing those surfaces' },
    { title: 'Pins', detail: 'one terminal single-writer applies every reported central Directory.Packages.props pin row at the symbol anchor and every reported ARCHITECTURE.md [02]-[SEAMS] row serially; runs only when rows were reported', model: 'sonnet' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 14 // concurrent folder-CHAIN ceiling — the default target sets run below it; it binds only when args name more folders than CAP
const IMPL_FAN = 3 // max implement agents fanned per folder, and only over discovery-proven page-disjoint card groups
// The launch gate spaces every pooled start (chain heads + implement-fan members) >= STAGGER_MS
// apart for the run's whole life: real (slow) work fans wide while a fast-fail cascade self-throttles.
const STAGGER_MS = 1500
const STALL = 300000
const ROOT = 'libs/csharp'
const SHARED_API = 'libs/csharp/.api'
const CENTRAL = 'Directory.Packages.props'
const DEFAULT_TARGETS = ['libs/csharp/Rasm.AppHost', 'libs/csharp/Rasm.Compute', 'libs/csharp/Rasm.AppUi', 'libs/csharp/Rasm.Persistence']
const CODEX = true // the discovery lane runs on gpt-5.5 via the codex wrapper; false restores the native inherit-model lane
const CODEX_DIR = '.claude/scratch/codex' // wrapper task/schema/report files, one triple per lane

// --- [INPUTS] ----------------------------------------------------------------------------
const norm = (t) => { const s = String(t).trim(); return s.indexOf('libs/') === 0 ? s : ROOT + '/' + s }
const TARGETS = Array.isArray(args) ? args.filter(Boolean).map(norm)
  : (args && typeof args === 'object' && Array.isArray(args.targets)) ? args.targets.filter(Boolean).map(norm)
  : (typeof args === 'string' && args.trim() && args.trim().toUpperCase() !== 'ALL') ? [norm(args)]
  : DEFAULT_TARGETS
const TARGET_NAMES = TARGETS.map((t) => '`' + (t.split('/').filter(Boolean).pop() || t) + '`').join(', ')

// --- [MODELS] ----------------------------------------------------------------------------
// Per-folder discovery: `pages` per card are disk-verified Anchors targets proving page-disjoint
// implement groups; `malformed_ripples` is a required attestation (empty = none found).
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'order', 'tasks', 'ideas', 'ripples', 'malformed_ripples'], properties: {
  folder: { type: 'string' },
  order: { type: 'array', items: { type: 'string' } },
  tasks: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'status', 'pages'], properties: { slug: { type: 'string' }, status: { type: 'string' }, atomic: { type: 'boolean' }, thesis: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } } } },
  ideas: { type: 'array', maxItems: 3, items: { type: 'object', additionalProperties: false, required: ['slug', 'status', 'pages'], properties: { slug: { type: 'string' }, status: { type: 'string' }, thesis: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } } } },
  ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from_slug', 'klass', 'to_pkg', 'to_slug'], properties: { from_slug: { type: 'string' }, klass: { type: 'string', enum: ['in_scope', 'oos_csharp', 'cross_lang'] }, to_pkg: { type: 'string' }, to_slug: { type: 'string' } } } },
  gates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['blocked_slug', 'gated_by_slug', 'in_scope'], properties: { blocked_slug: { type: 'string' }, gated_by_slug: { type: 'string' }, in_scope: { type: 'boolean' } } } },
  map: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'composed', 'underutilized', 'seams', 'stacking'], properties: { page: { type: 'string' }, composed: { type: 'string' }, underutilized: { type: 'string' }, seams: { type: 'string' }, stacking: { type: 'string' } } } },
  malformed_ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from_slug', 'raw'], properties: { from_slug: { type: 'string' }, raw: { type: 'string' } } } },
} }
// Required-but-possibly-empty `ripples`/`pins`/`seams` are attestations: ripple repair ran in-pass,
// and pin + seam rows are the run's only single-writer handoffs — empty attests none arose, never a skip.
const RIPPLES = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['counterpart', 'action'], properties: { counterpart: { type: 'string' }, action: { type: 'string' } } } }
const PINS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'row'], properties: { package: { type: 'string' }, row: { type: 'string' } } } }
const SEAMS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['file', 'row'], properties: { file: { type: 'string' }, row: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'ripples', 'pins', 'seams', 'summary'], properties: {
  folder: { type: 'string' },
  verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
  realized: { type: 'array', items: { type: 'string' } },
  deferred: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  collapsed: { type: 'string' },
  ripples: RIPPLES,
  pins: PINS,
  seams: SEAMS,
  summary: { type: 'string' },
} }
const REDTEAM_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'ripples', 'pins', 'seams', 'closed', 'reopened', 'summary'], properties: {
  folder: { type: 'string' },
  verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
  realized: { type: 'array', items: { type: 'string' } },
  deferred: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  collapsed: { type: 'string' },
  ripples: RIPPLES,
  pins: PINS,
  seams: SEAMS,
  closed: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'disposition', 'strength'], properties: { slug: { type: 'string' }, disposition: { type: 'string', enum: ['complete', 'dropped'] }, strength: { type: 'string', enum: ['strong', 'partial', 'weak'] } } } },
  reopened: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }
const PIN_SCHEMA = { type: 'object', additionalProperties: false, required: ['applied', 'seam_rows_applied', 'rejected', 'summary'], properties: {
  applied: { type: 'array', items: { type: 'string' } },
  seam_rows_applied: { type: 'array', items: { type: 'string' } },
  rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'reason'], properties: { target: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }

// --- [DOCTRINE] --------------------------------------------------------------------------
const FB = ' (the `.api` catalogs, the `nuget` MCP for feed truth, and Context7/exa/tavily for the official surface own the fallback when assay is unavailable)'
const LAW = [
  'Rasm monorepo, libs/csharp planning corpus (markdown specs of intended C# package designs). CLAUDE.md manifest + WORKSPACE_LAW strata govern ' +
    '(KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward; a host-neutral owner only where a non-Rhino runtime ' +
    'consumes the contract). The session targets are the libs/csharp packages ' + TARGET_NAMES + ', each on its canonical stratum; `Rasm.AppHost` ' +
    'is the host-neutral runtime spine `Compute`/`Persistence`/`AppUi` adapt to. Each target holds `IDEAS.md` + `TASKLOG.md` + ' +
    '`ARCHITECTURE.md` + `README.md` + `<pkg>.csproj` at the package ROOT, a deep `.api/api-*.md` capability catalog, and design pages at ' +
    '`<pkg>/.planning/<subdomain>/*.md`. Read the package-root `ARCHITECTURE.md` (sub-domain map + `[02]-[SEAMS]`), `README.md` (admitted-package ' +
    'roster), and `.api/` as the governing context and capability tier for that target. Cross-folder repair lands at seams, counterpart cards, and ' +
    'consumer sites — never by rebuilding a sibling owner interior.',
  'MANDATORY STANDARDS — docs/stacks/csharp/ is the FLOOR, not the ceiling: every fence MUST meet docs/stacks/csharp/ (README, language, shapes, ' +
    'surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) AND the specialized docs/stacks/csharp/domain/ shard(s) ' +
    'relevant to the page concern (compute, concurrency, data-interchange, diagnostics, durability, interaction, persistence, postgres, ' +
    'resilience, runtime, transport, validation, visuals), then PUSH PAST it to the objectively strongest form the doctrine admits. READ the ' +
    'relevant shard(s) and conform exactly — a hard gate enforced by the `tools/cs-analyzer` compiled-doctrine gate (a true positive is ' +
    'architecture pressure, fix the shape; a false positive is rule pressure, never a suppression). Cite only host/NuGet members confirmed via `uv ' +
    'run python -m tools.assay api`' + FB + '; back bridge claims with EvidenceCertificate + reviewed ReferenceEvidence.',
  'This is IMPLEMENT, not an untied page rebuild: realize the folder SPECIFIC open IDEAS/TASKLOG cards into deep design-page FENCES. A FENCE is a ' +
    'markdown fenced code block inside a `.planning` design page — the work product itself, NEVER a `.cs`/`.py`/`.ts` source file. SCOPE per ' +
    'target: realize ALL open tasks (including `Atomic`-flagged minor tasks), then the 1-3 chosen open ideas, tasks first. Realize tied to the ' +
    'card charter (Capability/Shape/Unlocks/Anchors), mining every admitted package to full capability and crushing surface sprawl into fewer ' +
    'richer owners with zero functionality loss.',
  'WRITE-FULLY + FIX-IT-NOW: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge. A cross-file ripple your edit exposes is YOURS in the ' +
    'same pass, wherever it lives: the seam counterpart on both ends, the consumer site, the stale sibling page, the 1-hop counterpart card fence ' +
    'in another libs/csharp package — repaired now and recorded in `ripples` (an empty `ripples` attests your pass exposed none, never that repair ' +
    'was skipped). TWO handoffs route to the run\'s terminal single-writer and are NEVER edited by a folder agent: the central `' + CENTRAL + '` ' +
    'pin (report the exact row in `pins`) and any package `ARCHITECTURE.md` `[02]-[SEAMS]` row (report {file, row} in `seams` — the ' +
    'highest-collision shared surface); every other page-level ripple stays yours, repaired distributed under the anchored-Edit discipline. If ' +
    'after real investigation a fence is already correct, say so — never invent edits to look busy.',
].join('\n')
const CARD = [
  'CARD SCHEMA: open cards live in `<pkg>/IDEAS.md` (ideas — larger conceptual capability) and `<pkg>/TASKLOG.md` (tasks — concrete targeted ' +
    'work), under section `[01]-[OPEN]`; closed cards collapse under `[02]-[CLOSED]`. A card is `[ID]-[STATUS]: <thesis>` then the bullets ' +
    '`Capability:` / `Shape:` / `Unlocks:` / `Anchors:` / `Tension:` (only when a constraint shapes it) / `Ripple:` (only on a cross-folder ' +
    'counterpart) / `Atomic:` (only on a minor task). Open statuses: `ACTIVE` (in-flight), `QUEUED` (next-up), `BLOCKED` (open but ' +
    'non-actionable). Closed: `COMPLETE` (finished) or `DROPPED` (abandoned). ALWAYS read the FULL card body (every bullet) from disk — the thesis ' +
    'alone is never enough to realize the charter.',
  'RIPPLE: `Ripple: <lang>:<pkg> [SLUG]` (or `<pkg> [SLUG]`) is a BIDIRECTIONAL cross-folder link — the counterpart card in the named pkg carries ' +
    'the mirror slug, and ripples are PART of scope, repaired in the pass that exposes them, never handed to a later stage. Three classes: ' +
    'IN-SCOPE (counterpart is another session target — its own pipeline realizes its card; you align your half of the seam to the counterpart ' +
    'page as it NOW stands on disk, and the later-landing side owns the final alignment), OUT-OF-SCOPE C# (counterpart in a non-target ' +
    'libs/csharp package — YOU realize the 1-hop counterpart card fence and align the seam on both ends in the same pass; the ripple\'s scope is ' +
    'that counterpart card and its seam, not the foreign folder\'s other cards), CROSS-LANGUAGE / LIB-WIDE (`libs/.planning`, `libs/typescript`, ' +
    '`libs/python` — outside this C#-only run\'s language rail; land your half stating the wire contract, and the card stays open unless it is ' +
    'complete on your half alone).',
  'PROBE FREELY (nothing gates probing): EVERY agent in EVERY phase may — and should — probe to verify reality at any time, for ANY card or design ' +
    'decision, not only `[BLOCKED]` ones — `uv run python -m tools.assay api resolve|query` over host DLLs / NuGet to confirm any member or ' +
    'signature; Rhino WIP (never Rhino 8) via the rhino-mcp skill or tools/rhino-bridge for live host/GH behavior; `uv run python -m tools.assay ' +
    'provision check` (+ tools/assay/README.md) for a native/scientific/database/provisioning band. tools/assay is under concurrent construction: ' +
    'when an assay invocation fails, the probe obligation stands and reroutes — the `.api` catalogs, the `nuget` MCP for feed truth, ' +
    'Context7/exa/tavily for the official surface — and a blocker provable ONLY through downed assay is a legitimate out-of-run blocker, never a ' +
    'faked resolution. A `[BLOCKED]` card is REALIZED this turn whenever a probe resolves its blocker OR its gating work is in scope; a blocker ' +
    'is genuinely legitimate ONLY when it depends on work outside this run.',
  'PACKAGE ADMISSION (only when a card genuinely needs a not-yet-admitted package): do the folder-local parts NOW — add `<PackageReference ' +
    'Include="..."/>` WITHOUT a version to `<pkg>/<pkg>.csproj`, add the package to the correct group in `<pkg>/README.md`, and author ' +
    '`<pkg>/.api/api-<pkg>.md` from `uv run python -m tools.assay api`' + FB + '. The central repo-root `' + CENTRAL + '` has exactly ONE in-run ' +
    'writer: report the exact `<PackageVersion Include="..." Version="..."/>` row in `pins` and never edit that file yourself. Never a per-folder ' +
    'version manifest; never re-pin a version outside `' + CENTRAL + '`.',
  'CARD CLOSURE (the folder red-team ONLY — implement and critique NEVER change card status): a genuinely-complete card moves to its file ' +
    '`[02]-[CLOSED]` section as a collapsed one-liner `[ID]-[COMPLETE]: <one-line disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`); ' +
    'report the owning `<pkg>/ARCHITECTURE.md` `[02]-[SEAMS]` row as {file, row} in `seams` ONLY when a real cross-folder seam landed (for a ' +
    'shared entry the owning-stratum folder\'s row wins — `Rasm.Persistence` owns durable-store seams, `Rasm.AppHost` owns host-neutral contract ' +
    'seams); the run\'s terminal single-writer applies it, never you. A ripple-carrying card closes COMPLETE only when its seam is verified landed ' +
    'on BOTH ends on current disk; close only `strong` cards and honestly re-open the rest.',
].join('\n')
const BARHUNT = [
  'BAR — a high-value IMPLEMENT leaves every owner capturing the FULL capability of every package it admits, every sprawl collapsed into one ' +
    'denser owner with NO capability lost, and every fence transcription-complete against the verified `.api`. The critique guards capability ' +
    'conservation, charter completeness, and density; the red-team attacks every fence for a surface that could still collapse, a thin wrapper, a ' +
    'silent functionality drop during a refactor, a missed package capability, or a framework violation, and fixes each in place.',
  'HUNT (at implement, critique, and red-team alike, from multiple facets): UNDER-CAPTURED CAPABILITY — an admitted package whose `.api` and code ' +
    'expose capability no owner exploits is a named gap, closed by deepening a fence or adding one. SURFACE SPRAWL — parallel ' +
    'types/enums/methods/near-duplicate shapes collapse into one parameterized owner in the C# collapse vocabulary ' +
    '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated case family/`Fold` algebra/frozen table) with no ' +
    'functionality removed. RAIL UNIFICATION — one entrypoint family per rail, one closed `Expected` fault family per package, total generated ' +
    '`Switch`. OPTIMIZATION — correctness first, then allocation/span/SoA layout/dispatch shape/algorithmic complexity, not only line-count. NEW ' +
    'WORK SURFACED — api gaps, stronger packages, and tasks the implementation exposes are realized the same turn (extend the canonical owner ' +
    'first, never a parallel surface).',
  'NAIVETY (two axes, both intolerable): COVERAGE — the owner models a thin slice of its concept (the obvious three fields where the domain ' +
    'carries fifteen; a two-case family for a twenty-case domain); APPROACH — enumerated hardcoded instances where a parameterized, algorithmic ' +
    'owner should generate the space (a fixed roster of styles/patterns/variants is seed DATA feeding one generator over named parameters, NEVER ' +
    'the mechanism itself). Every enumerated collapse-signal list in this prompt is a FLOOR, never the complete set: any repeated structure, ' +
    'parallel spelling, or enumerable family an algebra, table, fold, or generator can own is a collapse target you find yourself. A discovery ' +
    '`map` row in the worklist is an initial pointer, never a ceiling — re-read the pages in full and exceed it; a map never licenses a skim.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the 16 named laws of docs/stacks/csharp/README.md, held as fact: [FLOW] EXPRESSION_SPINE (domain logic is ' +
    'expression-shaped; dependent steps `Bind` monadically, independent ones accumulate applicatively; the carrier, never a flag, selects the ' +
    'algebra; statements survive only in measured `ref struct`/span kernels that name the exemption) + BOUNDARY_ADMISSION (raw admitted EXACTLY ' +
    'ONCE into an evidence-carrying owner; interior never re-validates or sees null/sentinel/provider shape). [SHAPE] SHAPE_BUDGET (one concept ' +
    'owns ONE type; variants are cases in one closed family) + DEEP_SURFACES + MODAL_ARITY (one entrypoint owns every modality, discriminating on ' +
    'input shape) + ANTICIPATORY_COLLAPSE (shape the owner for the family it will absorb). [DERIVATION] POLICY_VALUES + DERIVED_LOGIC + ' +
    'DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD (weave ' +
    'new capability into the owner as if always present; no shims/aliases/[Obsolete]/migration layers) + ONE_HOP_RESOLUTION + ' +
    'COMPOSED_IMPLEMENTATION.',
  'ULTRA-ADVANCED COLLAPSE MANDATE: parallel types / sibling factory methods / repeated switch arms / single-call private helpers sharing an ' +
    'identity regime, an admission path, a payload timing, or a consumer COLLAPSE into ONE polymorphic owner IN THE SAME FILE via `[Union]` / ' +
    '`[Union<T1,...>]` ad-hoc / `[SmartEnum<TKey>]` / `[SmartEnum]` keyless / `[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case ' +
    'families / `Fold` algebra / frozen data tables — a shape survives only on a genuinely distinct discriminant; never extract a new file to ' +
    'reduce LOC, never delete capability. Capability exits through FEW dense unified entry points — one polymorphic entry per rail discriminating ' +
    'on input shape (single|batch|stream absorbed by input detection, forward and inverse directions on one surface), variation living in input ' +
    'shape, policy values, and table rows, never parallel exports or modality-named siblings; the surface narrows by absorption, never by omission.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows raw -> admit ONCE (generated factory + validation partial admits/rejects; one rail ' +
    'bridge lifts the generated outcome into `Fin<T>` / `Validation<Error,T>`; `Option<T>` carries absence; exceptions convert at the owning ' +
    'boundary only) -> canonical owner -> unified rail -> projection -> egress. Interior code never re-validates, never sees `null`-as-failure, ' +
    'sentinels, or provider shapes; parameterize BOTH ingress AND egress so the same owner sources and sinks across many consumers without ' +
    'interior edits.',
  'ULTRA-STACK CAPABILITY: BOTH `.api` tiers are enumerated IN FULL with a real listing from disk (`ls`/`fd`, never memory) and mined to operator ' +
    'depth — the shared substrate tier `' + SHARED_API + '/` (Thinktecture, MathNet, CSparse, QuikGraph, Mapperly, and the other cross-package ' +
    'catalogs) AND the folder tier `' + ROOT + '/<Package>/.api/` (the curated, integration-shaped domain surface) — plus the universal ' +
    'Thinktecture (generated domain shape) / LanguageExt (rails, effects, schedules, immutable collections) rails and full docs/stacks/csharp ' +
    'doctrine, with MathNet / CSparse owning numeric algorithms. There is NO fixed package count: compose EVERY relevant host API + admitted NuGet ' +
    'package + catalog member into single dense owners woven as ONE rail (source-generated owners, `Fold` algebra, data tables), ALWAYS layering ' +
    'the universal Thinktecture/LanguageExt rails onto the domain packages, NOT flat one-shot per-API uses. Use the DEEPEST ' +
    'operator/combinator/generated surface each package itself reaches (LIBRARY_DEPTH); an admitted capability the concept admits but no owner ' +
    'exploits is a DEFECT you close, and a cited member you cannot verify via `uv run python -m tools.assay api`' + FB + ' is a PHANTOM you delete or ' +
    'correct, never leave standing; reject surface-level subsets, BCL-first reflexes, and thin rename wrappers.',
  'PRESERVE all capability (densify, never delete functionality): capability is improved or extended, NEVER dropped for lack of a current ' +
    'consumer — zero consumers never lowers the bar; planned consumers are real design pressure. Where a fence is already dense, deepen; where ' +
    'it is flat/naive, rebuild ground-up. Never regress correctness or boundary/strata law.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely — NEVER weak/unbounded/erased types where the language can express the domain; NEVER exception ' +
    'control flow in domain logic (use the LanguageExt typed rails / ROP and the route recovery patterns); NEVER imperative branching where a ' +
    'bounded vocabulary, frozen table, generated `Switch`, match, or `Fold` owns the variation; NEVER mutable accumulation for domain transforms ' +
    '(use immutable folds, projections, collection combinators). Total generated `Switch` with compile-time exhaustiveness (a new case breaks ' +
    'every dispatch site — NEVER a runtime-silent `_` arm). Typed algorithm receipts (NEVER a generic `IReceipt`/ledger) when fields carry ' +
    'route/status/sampling/solver/spectral/mesh/extraction/benchmark/host evidence. The fault type is a CLOSED `[Union]` family deriving from ' +
    '`Expected` (a bare exception or a generic untyped `Error` for a multi-cause domain is a defect).',
  'Latest stable C# 14 on `net10.0` to the metal (`Nullable enable`, NRT enforced): primary constructors, collection expressions with spread, ' +
    '`params` collections (incl. `params ReadOnlySpan<T>`), list/slice/relational/logical pattern matching, switch expressions, `required` ' +
    'members, `file`-scoped types, `field` accessors, extension blocks (`extension(Receiver)`) and extension operators, generic math / static ' +
    'abstract+virtual interface members, `with` expressions, `nameof` with unbound generics, `System.Threading.Lock`, raw string + `u8` literals ' +
    'where they fit. Treat analyzer diagnostics as architecture pressure (fix true positives, refine false positives, no ceremony suppressions). ' +
    'Apply the docs/stacks/csharp file-organization and section-order law (`[Union]`/`[SmartEnum]`/`[ValueObject]` and generated case families ' +
    'stay inside the declaring owner block; canonical section order TYPES -> CONSTANTS -> MODELS -> ERRORS -> SERVICES -> OPERATIONS -> ' +
    'COMPOSITION -> EXPORTS).',
  'Keep conventions IDENTICAL across every package; place each package on its canonical stratum and depend strictly upward; geometry/mesh/IFC meet ' +
    'at the wire with one owner per runtime; never leak a host type into a host-neutral owner. SEMANTIC_NAMING: one canonical bounded-context term ' +
    'per concept (one word default, three the ceiling); arity/filter/provider/modality live in request shape, case, or policy row, never parallel ' +
    '`Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` names; ONE_HOP_RESOLUTION (no alias chains, forwarding helpers, or util shells).',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package owner strictly in its lane and on its stratum; geometry/mesh/IFC meet at the wire with one ' +
  'owner per runtime; internal code uses canonical names and shapes with mapping only at the edge; never introduce a downward dependency or leak ' +
  'a host type into a host-neutral owner. Cross-folder repair is seam-shaped: align counterparts, consumer sites, and counterpart cards — a ' +
  'concern owned twice across a runtime, a folder mixing unrelated concerns, or coupling to a sibling owner INTERIOR (vs its seam/wire) is a defect.'
const CURRENT = 'CURRENT STATE — sibling folder pipelines land work concurrently with yours. Before ANY edit, re-read the CURRENT on-disk state ' +
  'of your pages AND every sibling page your pages compose or ripple into; landed sibling work is composed as found, never assumed from the ' +
  'discovery map. A seam counterpart a sibling pipeline landed is COMPOSED, not re-derived; a conflict between your design and a landed sibling ' +
  'resolves to the STRONGER form, never a revert. Edit any potentially shared page with surgical anchored Edits only — re-read and re-apply on an ' +
  'edit conflict, never a whole-file rewrite.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
    'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
    'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing ' +
    'contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, method, operator, package ID, path, command, flag, and literal value in backticks. Name the ' +
    'exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
  'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY ' +
  'where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a ' +
  'comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no ' +
  'task/process/session/history/proof/review comments, no XML-doc bloat. Densify names and types so comments are rarely needed; cut every ' +
  'low-value comment.'
const DOCTRINE = [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', CURRENT, '', PROSE, '', COMMENTS].join('\n')
const GROUPNOTE = 'CONCURRENT CARD GROUP: sibling implement agents realize this folder\'s OTHER page-disjoint card groups concurrently. Realize ' +
  'ONLY the cards in your worklist; touch any shared folder surface (README.md, a page outside your group\'s anchored pages, a sibling folder ' +
  'page) with surgical anchored Edits only, re-read and re-applied on conflict.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// One shared launch gate: chain heads and implement-fan members alike pass it, so every pooled start stays staggered.
let gate = Promise.resolve()
const stagger = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  const run = async () => { while (next < items.length) { const i = next++; await stagger(); out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}
// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// CODEX_DIR, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns that JSON verbatim. It never does, edits, or judges the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and relay ' +
    'its typed answer VERBATIM. Never perform, edit, judge, soften, or summarize the task yourself.',
  '(1) mkdir -p ' + CODEX_DIR + '; write the TASK block below verbatim to ' + base + '-task.md; write this JSON ' +
    'Schema exactly to ' + base + '-schema.json: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call that returns immediately: ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral ' +
    '--output-schema ' + base + '-schema.json -o ' + base + '-report.json "Do the task in ' + base + '-task.md ' +
    'from the repository root. Final message: JSON per the output schema." </dev/null >/dev/null 2>&1 &',
  '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
    'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
    'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' + base + '-report.json ] && break; ' +
    'pgrep -f "' + rpt + '" >/dev/null || break; sleep 20; done; if [ -s ' + base + '-report.json ]; then echo ' +
    'READY; elif pgrep -f "' + rpt + '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
    'while it prints RUNNING; stop on READY; on GONE go to (4). Cap at 7 poll calls.',
  '(4) READY: return the report-file JSON through your structured output VERBATIM, unchanged. GONE with no report: ' +
    'relaunch the (2) command once (detached, never foreground) and resume polling; a second GONE returns the ' +
    'schema shape with every array empty and each required string field set to CODEX-FAILED plus the one-line reason.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}
// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, the native inherit-model lane otherwise.
const recon = (task, o) => CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: o.schema, stallMs: STALL })
  : agent(task, { label: o.label, phase: o.phase, effort: 'high', schema: o.schema, stallMs: STALL })
const folderName = (p) => p.split('/').filter(Boolean).pop() || p
// Page-disjointness is PROVEN, never assumed: every ordered card must carry >=1 verified page,
// gate pairs merge, and components pack heaviest-first into <= IMPL_FAN buckets without splitting.
const cardGroups = (t) => {
  const inOrder = new Set(t.order || [])
  const cards = [...(t.tasks || []), ...(t.ideas || [])].filter((c) => inOrder.has(c.slug))
  if (cards.length < 2 || cards.some((c) => !(c.pages && c.pages.length))) return null
  const parent = new Map()
  const seed = (k) => { if (!parent.has(k)) parent.set(k, k) }
  const find = (k) => { let r = k; while (parent.get(r) !== r) r = parent.get(r); return r }
  const union = (a, b) => { seed(a); seed(b); const ra = find(a); const rb = find(b); if (ra !== rb) parent.set(ra, rb) }
  for (const c of cards) { seed('s:' + c.slug); for (const p of c.pages) union('s:' + c.slug, 'p:' + p) }
  for (const g of (t.gates || [])) if (inOrder.has(g.blocked_slug) && inOrder.has(g.gated_by_slug)) union('s:' + g.blocked_slug, 's:' + g.gated_by_slug)
  const comps = new Map()
  for (const c of cards) {
    const r = find('s:' + c.slug)
    if (!comps.has(r)) comps.set(r, { slugs: [], pages: new Set() })
    const g = comps.get(r); g.slugs.push(c.slug); for (const p of c.pages) g.pages.add(p)
  }
  if (comps.size < 2) return null
  const sorted = [...comps.values()].sort((a, b) => b.pages.size - a.pages.size || (a.slugs[0] < b.slugs[0] ? -1 : 1))
  const buckets = Array.from({ length: Math.min(IMPL_FAN, sorted.length) }, () => ({ slugs: [], pages: [] }))
  for (const c of sorted) { const b = buckets.reduce((m, x) => (x.pages.length < m.pages.length ? x : m)); b.slugs.push(...c.slugs); b.pages.push(...c.pages) }
  return buckets
}
const groupSeq = (t, g) => {
  const s = new Set(g.slugs)
  const p = new Set(g.pages)
  return JSON.stringify({
    order: (t.order || []).filter((x) => s.has(x)),
    tasks: (t.tasks || []).filter((c) => s.has(c.slug)),
    ideas: (t.ideas || []).filter((c) => s.has(c.slug)),
    ripples: (t.ripples || []).filter((r) => s.has(r.from_slug)),
    gates: (t.gates || []).filter((x) => s.has(x.blocked_slug)),
    map: (t.map || []).filter((m) => p.has(m.page)),
  }, null, 1)
}
const discoverPrompt = (folder) => [LAW, '', CARD, '',
  'TASK: DISCOVER + SEQUENCE + MAP the open work of the single session target `' + folder + '` (the full session target set, for ripple ' +
    'classification only: ' + JSON.stringify(TARGETS) + '). DISCOVERY is the reconnaissance this folder\'s downstream chain stands on, and ' +
    'read-only is its ONLY concession — full reads, never skims, never memory. FIRST enumerate from the source of truth with a REAL listing ' +
    '(`ls`/`fd`, never recall): BOTH `.api` tiers (`' + SHARED_API + '/` and `' + folder + '/.api/`), the doctrine inventory ' +
    '(`docs/stacks/csharp/` core + `domain/` shards), and the target folder at large (`.planning/**` pages, `ARCHITECTURE.md`, `README.md`, the ' +
    'csproj). THEN read IN FULL: `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md` (every card body, every bullet — the thesis alone is ' +
    'never enough), every design page an open card names, and the folder at large (the remaining `.planning/**` pages plus `ARCHITECTURE.md` + ' +
    '`README.md`) — seams, staleness, and unexploited capability hide in pages no card names. Resolve scope against real disk state: a named ' +
    'page, pkg, or slug you cannot find on disk is recorded (map row or malformed_ripples), never assumed. Return: (1) folder — echo ' +
    '`' + folder + '` exactly; (2) tasks — EVERY open card in `TASKLOG.md` (status ACTIVE/QUEUED/BLOCKED; carry the Atomic flag); (3) ideas — the ' +
    '1-3 MOST actionable open cards in `IDEAS.md` (tasks-first doctrine: pick at most 3, the ones whose Anchors are most settled and whose ' +
    'ripples land on in-scope targets), HARD CAP 3; on EVERY task/idea row also return pages — the repo-relative design pages under ' +
    '`' + folder + '/.planning/**` the card\'s `Anchors:` name and you VERIFIED exist on disk (empty when none verify): these rows prove ' +
    'page-disjoint card groups; (4) order — ONE sequenced slug list, ALL tasks first in dependency order (a card whose Anchors reference another ' +
    'card output comes after it), then the chosen ideas; (5) ripples — for EVERY card carrying a `Ripple:` field, one row {from_slug, klass, ' +
    'to_pkg, to_slug}: klass=`in_scope` if to_pkg is one of the session targets, `oos_csharp` if it is another libs/csharp package, `cross_lang` ' +
    'if it points at `libs/.planning` / `libs/typescript` / `libs/python`; (6) gates — for any [BLOCKED] card, {blocked_slug, gated_by_slug, ' +
    'in_scope} where in_scope is true iff the gating work is itself an open card in one of the session targets (that card becomes actionable ' +
    'this run once its gate lands); (7) map — one row per design page the open cards target: {page, composed: the capability the page already ' +
    'composes, underutilized: catalog-anchored member FACTS — the exact verified member spellings, each with its owning `.api` catalog, that the ' +
    'page does not yet compose (verified members ONLY — a member you cannot verify is a phantom and is NEVER listed; exact spellings and ' +
    'locations, never judgment wording), seams: the contextual cross-page/cross-folder seams the page meets, stacking: how the catalog ' +
    'capability stacks into the realization}. The map carries NAVIGATION FACTS — paths, verified member locations, seam targets — never ' +
    'verdicts; downstream agents treat it as an initial pointer, never a ceiling, and it licenses no skim. Also return malformed_ripples for any ' +
    '`Ripple:` line you cannot parse into a pkg+slug, or whose counterpart slug you cannot locate in the named pkg. Return the structured map ' +
    'ONLY; edit nothing — read-only is DISCOVERY\'s sole concession.'].join('\n')
const implementPrompt = (folder, seq, note) => [DOCTRINE, '',
  'TASK: IMPLEMENT — realize the open cards of `' + folder + '` into deep design-page FENCES at the ULTRA bar. The sequenced worklist (slugs + ' +
    'ripple map + discovery capability map; read each FULL card body from `' + folder + '/IDEAS.md` + ' +
    '`' + folder + '/TASKLOG.md`, never the thesis alone):\n' + seq + '\nREAD: ' +
    'each card full body; every design page the card names under `' + folder + '/.planning/**`; the sibling pages it seams to, at their CURRENT ' +
    'on-disk state; the package-root `ARCHITECTURE.md` + `README.md`; docs/stacks/csharp/ core + the relevant domain/ shard(s) for the card ' +
    'concern; BOTH `.api` tiers in full — `' + SHARED_API + '/` + `' + folder + '/.api/api-*.md`, enumerated by real listing, never memory — plus ' +
    'the admitted packages; and verify any novel host/NuGet member via `uv run python -m tools.assay api`' + FB + '. Realize EVERY card in `order` ' +
    '(all tasks incl. Atomic, then the ideas) into deep fences in the `' + folder + '` design pages, in LIFECYCLE order (admit raw ONCE through a ' +
    'generated factory + validation partial -> lift into the canonical owner the OWNER_CHOOSER discriminants select -> weave every cross-cutting ' +
    'concern as a definition-time source-generated aspect or composition-time effect transformer over a thin pure core -> compose through ONE ' +
    'unified `Fin`/`Validation`/`Option`/`Eff` rail with total generated `Switch` -> project + egress, BOTH ingress and egress parameterized). ' +
    'Collapse parallel shapes into one `[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated case family in the ' +
    'SAME file; drive cases with a `Fold` algebra or a frozen table; one polymorphic entrypoint per modality. Resolve any [BLOCKED] card inline ' +
    '(probe via `assay api`' + FB + ' / Forge band / Rhino WIP). RIPPLES ARE YOURS: repair every ripple your cards carry in this same pass per the ' +
    'RIPPLE law — align each in-scope seam to the counterpart page as it NOW stands on disk, realize each 1-hop out-of-scope C# counterpart card ' +
    'fence and align both ends, land your half of each cross-language seam stating the wire contract — and record each repair in `ripples`. ' +
    'PACKAGE ADMISSION per the card law: folder-local parts NOW, the central `' + CENTRAL + '` row reported in `pins`, never edited; a landed ' +
    'cross-folder seam\'s `ARCHITECTURE.md` `[02]-[SEAMS]` row reported in `seams`, never edited. Do NOT close any card — the folder red-team ' +
    'owns card status. Modern C# 14 / net10 to the metal, high-signal prose all-backticked, comment hygiene, fix-in-place (read-then-extend, ' +
    'preserve capability). Return verdict + realized slugs + deferred (any card you could not realize, with reason) + collapsed (before->after ' +
    'counts) + ripples + pins + seams + summary.' + (note ? '\n' + note : '')].join('\n')
const critiquePrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: DOCTRINAL-CONFORMANCE AUDIT + CHARTER-COMPLETENESS + FIX IN PLACE across `' + folder + '`. You are an ULTRA-HARSH, UNAGREEABLE auditor: ' +
    'assume a violation exists in every fence until you prove otherwise, and "good enough" is rejected. The cards realized this turn (read each ' +
    'FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`):\n' + seq + '\nREAD the realized pages, the sibling pages at their ' +
    'CURRENT on-disk state, docs/stacks/csharp/ core + the relevant domain/ shard(s), and BOTH `.api` tiers by real listing — `' + SHARED_API + '/` ' +
    'substrate + `' + folder + '/.api/` — plus the universal Thinktecture/LanguageExt rails. Run these MECHANICAL checklists line-by-line as a ' +
    'FLOOR you hunt past, never the complete audit, and REPAIR every hit in place (a fix, never a ledger note):',
  '(1) COLLAPSE_SCAN — apply the move for any signal (shapes sharing an identity regime, an admission path, a payload timing, or a consumer ' +
    'collapse into ONE owner; a shape survives only on a genuinely distinct discriminant): sibling prefix/suffix names -> one modality-polymorphic ' +
    'entrypoint; same return rail differing only by arity -> input-shape discrimination; functions differing only by a literal -> parameterize the ' +
    'literal as a POLICY_VALUE; a `bool`/`mode`/`batch` parameter selecting two bodies -> one derived body or policy value; a method calling ' +
    'exactly one other -> delete the hop (ONE_HOP_RESOLUTION); parallel dispatch arms repeating structure -> a `Fold` algebra or frozen table ' +
    '(DERIVED_LOGIC); several types sharing fields for one concept -> one closed family; a `Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` family -> ' +
    'one input-keyed polymorphic operation; a wrapper renaming a package API -> use the package surface directly (LIBRARY_DEPTH); parallel ' +
    'types / sibling factories / repeated switch arms / single-call helpers sharing an identity regime, admission path, payload timing, or ' +
    'consumer -> ONE `[Union]` / `[SmartEnum<TKey>]` / `[ValueObject<T>]` / ' +
    '`[ComplexValueObject]` / source-generated case family IN THE SAME FILE. These signals are a FLOOR, never the complete set — hunt past them.',
  '(2) OWNER_CHOOSER — for EVERY shape re-derive the owner from the 5 discriminants (admission, identity regime, variant arity, payload timing, ' +
    'openness), most-specific wins: invariant-bearing scalar -> `[ValueObject<TKey>]`; N-field one-concept product no discriminator -> ' +
    '`[ComplexValueObject]`; bounded vocabulary wire-keyed identity -> `[SmartEnum<TKey>]`; bounded vocabulary process-local behavior -> ' +
    '`[SmartEnum]` keyless; closed alternatives per-occurrence payload -> `[Union]`; one value over 2-5 unrelated types -> `[Union<T1,...>]` ' +
    'ad-hoc; interior product no invariant -> `record`/`readonly record struct`; combinable capability set -> a frozen set; cross-product/external ' +
    'policy key -> a frozen table; foreign wire enum / ABI bits / kernel ordinal -> a language `enum` AT THE SEAM ONLY. Kill every parallel DTO, ' +
    'one-field wrapper, field-rename shape, nullable-as-failure, and struct-`default` ghost.',
  '(3) KNOB_TEST — removal: delete each parameter; if the value reconstructs what it carried it was a knob -> collapse a ' +
    '`bool`/`mode`/`strict`/`batch` flag into a policy value or input-shape discriminant; a nullable flag tail -> one `Option<ContextRecord>`; the ' +
    'single optional form is `Option<T> x = default` consumed via `IfNone(canonical)`; move every `timeout`/`retry`/`deadline`/`CancellationToken` ' +
    'OFF the signature onto the carrier or a composition-time effect aspect.',
  '(4) ASPECTS — definition-time concerns (admission, identity, dispatch, serialization, grammar, logging) attach via attribute-directed SOURCE ' +
    'GENERATION in the fixed generator order; composition-time concerns attach as effect transformers in author order — retry as `Schedule`-driven ' +
    '`IO<T>.Retry(Schedule)`/`Prelude.retry`, recovery as named catch combinators (`@catch`/`catchOf`/`CatchM` composed via `|`), resource ' +
    'lifetime as `Bracket`/`BracketIO`/`Finally`; the two weaves meet at EXACTLY ONE seam, the admission rail bridge. Co-occurring wrappers ' +
    'sharing an admission path collapse into ONE aspect; an aspect NEVER raises into domain flow; deterministic stacking order verified. ' +
    'Inline-repeated concerns and sibling helper methods are defects.',
  '(5) RAILS — RAIL_CHOOSER, the narrowest carrier chosen ONCE at admission: `Option<T>` absence, `Fin<T>` synchronous fallibility, ' +
    '`Validation<Error,T>` independent accumulated faults, `Eff<RT,T>` runtime capability, `IO<T>` deferred boundary work, `Schedule` retry ' +
    'policy, `Seq<T>`/`Arr<T>`/`HashMap<K,V>` immutable traversal/lookup; the fault type is a CLOSED `[Union]` family deriving from `Expected` (a ' +
    'bare exception or generic untyped `Error` for a multi-cause domain is a defect; recovery identity via `Is`/`HasCode`/`IsType<E>`, never ' +
    '`==`); accumulate-vs-abort disposition correct (`Apply`/`&`/`.Traverse` for independents, `Bind`/`.TraverseM`/query for dependents); total ' +
    'generated `Switch` (NO `_` arm hiding a case); `.Fold`/`.Traverse`/`.Choose` with the mandatory `.As()` re-anchor; NO exception control flow ' +
    'in domain logic, NO mutable accumulation.',
  '(6) STRATA/MEMBERS/MODERN — strata correctness (depend strictly upward; NO downward dependency, NO host-type leak into a host-neutral owner; ' +
    'geometry/mesh/IFC meet at the wire with one owner per runtime); cite ONLY host/NuGet members confirmed in a `.api` catalog (verify novel ' +
    'members via `uv run python -m tools.assay api`' + FB + '; an unverifiable cited member is a PHANTOM — delete or correct it); latest modern C# 14 on ' +
    'net10 (primary ctors, collection expressions, `params` collections, list/relational/logical patterns, switch expressions, `required` ' +
    'members, `file` types, `field` accessors, extension blocks, generic math, static abstract members); FULL docs/stacks/csharp + the relevant ' +
    'domain/ shard conformance; BOTH `.api` tiers (`' + SHARED_API + '/` substrate + the folder catalogs) AND the universal Thinktecture/LanguageExt ' +
    'rails maximized to operator depth; the `tools/cs-analyzer` doctrine-gate clean.',
  '(7) CHARTER-COMPLETENESS — for EVERY card in the worklist, verify the realized fences GENUINELY fulfill its `Capability`/`Shape`/`Unlocks` ' +
    '(read the full card from disk): a missing modality, an unrealized `Shape` clause, a stubbed/placeholder fence, or a capability the card ' +
    'promises but the fences do not deliver is a DEFECT — realize it NOW. A card whose fences are thin against its charter is not done. Attack ' +
    'BOTH naivety axes per card: COVERAGE — fences modeling a thin slice of the domain the charter names — widen to the full concept; APPROACH — ' +
    'an enumerated roster of hardcoded instances where ONE parameterized generator should own the space — demote the roster to seed data feeding ' +
    'that generator.',
  '(8) SEAMS — check every cross-page and cross-folder symbol these cards compose against the counterpart as it NOW stands on disk: a signature ' +
    'mismatch corrects at the weaker end, a conflict resolves to the stronger form, never a revert; a seam counterpart or consumer site your fix ' +
    'exposes is repaired in this same pass wherever it lives, recorded in `ripples` (a package `ARCHITECTURE.md` `[02]-[SEAMS]` row change is ' +
    'reported in `seams` for the terminal single-writer, never edited directly).',
  'Also enforce the docs/stacks/csharp file-organization + section-order law, cross-package convention consistency, and prose + comment hygiene. ' +
    'FIX every hit NOW wherever it lives per WRITE-FULLY; report any central `' + CENTRAL + '` row in `pins`. Return verdict + realized + deferred ' +
    '+ collapsed + ripples + pins + seams + summary.'].join('\n')
const redteamPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE + TERMINAL CLOSE across `' + folder + '`. You are the LAST and MOST AGGRESSIVE pass: ' +
    'assume the author and critique missed things and that the chosen design is not the strongest until proven, with the burden of proof ON THE ' +
    'DESIGN. The cards realized this turn (read each FULL body):\n' + seq + '\nREAD BOTH `.api` tiers by real listing — `' + SHARED_API + '/` ' +
    'substrate + `' + folder + '/.api/` — plus the universal Thinktecture/LanguageExt rails, the sibling pages at their CURRENT on-disk state, ' +
    'docs/stacks/csharp/ + the relevant domain/ shard. Attack from every direction and REPAIR every defect in place — no soft-pedalling, no ' +
    'could/should, a fix never a ledger.',
  'PRIMARY LENS — fundamental design, multi-faceted: (A) COUNTERFACTUAL on the core choice — is the owner, the algebra (`Fold`/generated ' +
    '`Switch`/data table), and the dispatch form categorically the strongest the doctrine admits, or does a denser owner ' +
    '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated family), a data table, or a DEEPER admitted-package ' +
    'primitive (LanguageExt/Thinktecture/MathNet/CSparse) collapse the whole fence? If a fundamentally stronger design exists, rebuild to it — ' +
    'never defend the incumbent; an enumerated roster of hardcoded variants where a parameterized generator should own the space is an APPROACH ' +
    'defect — demote the roster to seed data feeding ONE generator. (B) ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: when the next ' +
    'case/dimension/knob/modality/provider arrives, does it land as ONE case/row/policy value with every consumer untouched or broken LOUDLY at ' +
    'compile time (total generated `Switch`, no silent `_`)? If it would touch multiple sites, reshape so the growth axis is a case, row, policy ' +
    'value, or carrier swap. (C) LONG-TAIL + DOMAIN COMPLETENESS — attack every input/output/edge/failure mode (empty, singular, plural, stream, ' +
    'malformed, concurrent, cancelled, partial-failure, version-skew); COVERAGE naivety — an owner modeling the obvious thin slice of its concept ' +
    'where the domain carries far more — is a defect: widen to the full concept; is the accumulate-vs-abort disposition correct for the REAL boundary; ' +
    'are BOTH ingress AND egress parameterized so this owner sources and sinks across hundreds of consumers without interior edits? (D) STRATA + ' +
    'BOUNDARY-INTEGRITY — a downward dependency, a host-type leak into a host-neutral owner, a concern owned twice in a runtime, a folder mixing ' +
    'concerns, geometry/mesh/IFC not meeting at ONE wire owner per runtime, coupling to a sibling owner INTERIOR (vs its seam/wire), OR a sibling ' +
    'planning page left STALE by this folder change even when no ripple card names it (ports/boundaries/wires/seams drift) is a defect: fix it ' +
    'NOW wherever it lives — the stale sibling page, the seam counterpart, the consumer site — and record the repair in `ripples` (a package ' +
    '`ARCHITECTURE.md` `[02]-[SEAMS]` row change is reported in `seams` for the terminal single-writer, never edited directly). (E) ' +
    'SURFACE-SPRAWL-IN-TIME — an admitted package whose `.api` or the universal rails expose capability the fence re-derives by hand, flat code ' +
    'below the operator depth the packages reach, a phantom `.api`/host member, or a thin wrapper: collapse to package depth, verify every cited ' +
    'member (via `assay api`' + FB + '), and DELETE or correct every phantom.',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (every time, NOT only on a structural restructure): re-attack every conformance dimension with fresh ' +
    'hostile eyes, trusting nothing the prior passes claimed — the COLLAPSE_SCAN signals, OWNER_CHOOSER per shape, the KNOB_TEST per param, the ' +
    'two-weave ASPECT taxonomy, rail + closed-`Expected`-fault discipline, charter-completeness per card, strata correctness, modern-C# 14 typing, ' +
    'docs/stacks/csharp + domain-shard conformance, `.api` + Thinktecture/LanguageExt maximization, the `tools/cs-analyzer` doctrine-gate, and ' +
    'prose/comment hygiene — and fix every defect. Even absent a structural rebuild, the fences must end objectively denser, more correct, and ' +
    'more powerful than the critique left them; if the strongest form is genuinely already present, prove it by finding nothing — never invent ' +
    'churn.',
  'TERMINAL CLOSE — you are `' + folder + '`\'s LAST stage and the SOLE owner of its card status. For EVERY card in scope this run, re-read its ' +
    'FULL body and the realized fences on CURRENT disk, then ADVERSARIALLY VERIFY — the fences are naive until they survive your attack, a prior ' +
    'pass verdict a rejected self-assessment — that they genuinely fulfill the card `Capability`/`Shape`/`Unlocks` against the verified `.api` ' +
    '(verify novel members via `uv run python -m tools.assay api`' + FB + '). FINAL-remediate any weak or partial realization in place NOW, then ' +
    'assign each card a strength: `strong` (every charter clause delivered, fences transcription-complete against the verified `.api`), `partial` ' +
    '(most delivered, a clause still thin), `weak` (charter not met). CLOSE only `strong` cards per the CARD CLOSURE law; a ripple card whose ' +
    'seam you cannot verify landed on BOTH ends on current disk stays OPEN with that reason; honestly RE-OPEN every card you cannot bring to ' +
    '`strong`, with a one-line reason (a real out-of-run or cross-language dependency). The orchestrator DEMOTES any card closed below `strong`, ' +
    'so never inflate. Return verdict + realized + deferred + collapsed + ripples + pins + seams + closed [{slug, disposition, strength}] + ' +
    'reopened [{slug, reason}] + summary.'].join('\n')
const pinPrompt = (pins, seams) => [LAW, '', PROSE, '',
  'TASK: TERMINAL SINGLE-WRITER — you are the run\'s SOLE writer for the repo-root `' + CENTRAL + '` and for every package `ARCHITECTURE.md` ' +
    '`[02]-[SEAMS]` section, and its LAST agent. PINS: apply each reported row below exactly once — hand-edit the grouped manifest at the SYMBOL ' +
    'anchor (never a line number), preserving label-group and alphabetical order, deduping semantically identical rows; verify each package + ' +
    'version via the `nuget` MCP or `uv run python -m tools.assay api`' + FB + ' before applying; confirm the owning `<pkg>.csproj` carries the ' +
    'versionless `<PackageReference/>` and the folder README/.api rows landed, repairing a missing folder-local part in place. SEAM ROWS: upsert ' +
    'each reported {file, row} into the named file\'s `[02]-[SEAMS]` section exactly once, preserving the section\'s row format and order and ' +
    'deduping semantically identical rows; a missing file or absent `[02]-[SEAMS]` section rejects the row. Reject any unverifiable or malformed ' +
    'row as {target, reason} — never apply it silently. PINS:\n' + JSON.stringify(pins, null, 1) + '\nSEAM ROWS:\n' +
    JSON.stringify(seams, null, 1)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

// --- [REALIZE]
phase('Realize')
log('Pooling ' + TARGETS.length + ' folder chain(s) at CAP=' + CAP)
const runFolder = async (target) => {
  const tag = folderName(target)
  try {
    const t = await recon(discoverPrompt(target), { label: 'discover:' + tag, phase: 'Realize', schema: DISCOVERY_SCHEMA })
    if (!t) return { folder: target, failed: true, empty: false, logs: [], red: null, cross_lang: [], malformed: [] }
    const cross = (t.ripples || []).filter((rp) => rp.klass === 'cross_lang').map((rp) => tag + ' [' + rp.from_slug + '] -> ' + rp.to_pkg + ' [' + rp.to_slug + ']')
    const malformed = t.malformed_ripples || []
    if (!((t.tasks && t.tasks.length) || (t.ideas && t.ideas.length))) return { folder: target, failed: false, empty: true, logs: [], red: null, cross_lang: cross, malformed }
    const seq = JSON.stringify({ order: t.order, tasks: t.tasks, ideas: t.ideas, ripples: t.ripples, gates: t.gates || [], map: t.map || [] }, null, 1)
    const groups = cardGroups(t)
    let impls
    if (groups) {
      log(tag + ': implement fan over ' + groups.length + ' page-disjoint group(s); page weights ' + groups.map((g) => g.pages.length).join('/'))
      impls = (await parallel(groups.map((g, gi) => async () => { await stagger(); return agent(implementPrompt(target, groupSeq(t, g), GROUPNOTE), { label: 'implement:' + tag + ':g' + gi, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: STALL }) }))).filter(Boolean)
    } else {
      const one = await agent(implementPrompt(target, seq, ''), { label: 'implement:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: STALL })
      impls = one ? [one] : []
    }
    if (!impls.length) return { folder: target, failed: true, empty: false, logs: [], red: null, cross_lang: cross, malformed } // failure isolation: a dead implement skips its reviews
    const crit = await agent(critiquePrompt(target, seq), { label: 'critique:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: STALL })
    const red = await agent(redteamPrompt(target, seq), { label: 'redteam:' + tag, phase: 'Realize', schema: REDTEAM_SCHEMA, effort: 'high', stallMs: STALL })
    return { folder: target, failed: false, empty: false, logs: [...impls, crit, red].filter(Boolean), red, cross_lang: cross, malformed }
  } catch (e) {
    return { folder: target, failed: true, empty: false, logs: [], red: null, cross_lang: [], malformed: [], error: String((e && e.message) || e) } // failure isolation: one thrown chain never rejects the pool
  }
}
const done = (await pool(TARGETS, CAP, runFolder)).filter(Boolean)
const failed = done.filter((r) => r.failed).map((r) => r.folder)
const emptyFolders = done.filter((r) => r.empty).map((r) => r.folder)
const active = done.filter((r) => !r.failed && !r.empty)
const crossLang = done.flatMap((r) => r.cross_lang || [])
const deferred = done.flatMap((r) => r.logs.flatMap((l) => (l.deferred || []).map((d) => ({ folder: r.folder, slug: d.slug, reason: d.reason }))))
const ripplesRepaired = done.flatMap((r) => r.logs.flatMap((l) => l.ripples || []))
const pinsReported = [...new Map(done.flatMap((r) => r.logs.flatMap((l) => l.pins || [])).map((p) => [p.package + '|' + p.row, p])).values()]
const seamsReported = [...new Map(done.flatMap((r) => r.logs.flatMap((l) => l.seams || [])).map((s) => [s.file + '|' + s.row, s])).values()]
let closed_count = 0
const reopened = []
for (const r of done) {
  for (const c of ((r.red && r.red.closed) || [])) {
    if (c.disposition === 'complete' && c.strength !== 'strong') reopened.push({ folder: r.folder, slug: c.slug, reason: 'demoted: strength=' + c.strength })
    else closed_count++
  }
  for (const ro of ((r.red && r.red.reopened) || [])) reopened.push({ folder: r.folder, slug: ro.slug, reason: ro.reason })
}
log('Realize: ' + active.length + '/' + TARGETS.length + ' folder(s) realized (' + emptyFolders.length + ' empty); ' + closed_count +
  ' cards closed, ' + reopened.length + ' re-open/demoted; ' + ripplesRepaired.length + ' ripple repair(s); ' + pinsReported.length +
  ' pin(s) + ' + seamsReported.length + ' seam row(s) reported' + (failed.length ? '; failed: ' + failed.join(', ') : ''))

// --- [PINS]
let pinlog = null
if (pinsReported.length || seamsReported.length) {
  phase('Pins')
  pinlog = await agent(pinPrompt(pinsReported, seamsReported), { label: 'pins', phase: 'Pins', schema: PIN_SCHEMA, model: 'sonnet', effort: 'high', stallMs: STALL })
}

return {
  root: ROOT,
  targets: TARGETS,
  realized_folders: active.map((r) => r.folder),
  empty_folders: emptyFolders,
  realize_failed: failed,
  deferred,
  ripples_repaired: ripplesRepaired.length,
  closed_count,
  reopened,
  pins: { reported: pinsReported.length, seams_reported: seamsReported.length, applied: (pinlog && pinlog.applied) || [], seam_rows_applied: (pinlog && pinlog.seam_rows_applied) || [], rejected: (pinlog && pinlog.rejected) || [] },
  cross_language: crossLang,
  malformed_ripples: done.flatMap((r) => r.malformed || []),
}
