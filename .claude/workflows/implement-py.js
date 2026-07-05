export const meta = {
  name: 'implement-py',
  whenToUse: 'Realize open IDEAS and TASKLOG cards into design-page code fences across the Python target folders.',
  description: 'Realize every open IDEAS/TASKLOG card across the Python target set (libs/python/artifacts, compute, data, geometry, runtime) into deep design-page code FENCES at the docs/stacks/python bar (with docs/stacks/csharp as the ambition floor), repair every ripple in-pass, and truthfully close the cards. One discovery agent maps cards + ripple classes + blockers; each target folder then runs ONE implement -> critique -> redteam pipeline, ALL folders concurrent under one pooled cap. Every stage WRITES and repairs the cross-file ripples its own work exposes in the same pass — in-scope seams aligned against current disk, 1-hop out-of-scope same-language counterpart fences realized directly — with BLOCKED probes and folder-local package admission inline. The redteam is each folder\'s terminal stage and sole card-status owner: it final-remediates weak realizations in place and closes only cards whose realization it verified strong on disk. The single permitted handoff is the central pyproject.toml pin: folder agents report exact rows + band markers, one terminal writer applies them serially. Card-driven (it implements ideas/tasks), NOT the in-isolation api-stacking of the rebuild engine. Python-only. args = a target path string, an array of paths, or empty for the five defaults. The language-wide libs/python/.planning is out of scope.',
  phases: [
    { title: 'Discover', detail: 'one agent: enumerate folders + both .api tiers + doctrine from disk, full-read each target IDEAS/TASKLOG and anchored pages; extract open cards (all tasks incl atomic + 1-3 ideas), sequence each folder, classify every ripple (in_scope / oos_samelang / cross_lang), record in-scope gates and malformed ripples' },
    { title: 'Realize', detail: 'all folder pipelines concurrent under one pooled cap: implement(max) -> critique(xhigh) -> redteam(max, terminal close); every stage writes, re-reads current disk before editing, repairs its own ripples in-pass, and the redteam closes only cards verified strong on disk' },
    { title: 'Pins', detail: 'one terminal writer applies every reported central pyproject.toml pin row + band marker serially; runs only when pins were reported' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10 // in-flight ceiling for the one pooled scheduler
// The launch gate spaces EVERY agent start >= STAGGER_MS apart for the pool's whole life: real
// (slow) work fans to CAP while a fast-fail cascade self-throttles to <= 1 launch / STAGGER_MS.
const STAGGER_MS = 1500
const STALL = 300000
const ROOT = 'libs/python'
const SHARED_API = 'libs/python/.api'
const CENTRAL = 'pyproject.toml'
const DEFAULT_TARGETS = ['libs/python/artifacts', 'libs/python/compute', 'libs/python/data', 'libs/python/geometry', 'libs/python/runtime']

// --- [INPUTS] ----------------------------------------------------------------------------
const norm = (t) => { const s = String(t).trim(); return s.indexOf('libs/') === 0 ? s : ROOT + '/' + s }
const TARGETS = Array.isArray(args) ? args.filter(Boolean).map(norm)
  : (args && typeof args === 'object' && Array.isArray(args.targets)) ? args.targets.filter(Boolean).map(norm)
  : (typeof args === 'string' && args.trim() && args.trim().toUpperCase() !== 'ALL') ? [norm(args)]
  : DEFAULT_TARGETS
const TARGET_NAMES = TARGETS.map((t) => '`' + (t.split('/').filter(Boolean).pop() || t) + '`').join(', ')

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['targets'], properties: {
  targets: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'order', 'tasks', 'ideas', 'ripples'], properties: {
    folder: { type: 'string' },
    order: { type: 'array', items: { type: 'string' } },
    tasks: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'status'], properties: { slug: { type: 'string' }, status: { type: 'string' }, atomic: { type: 'boolean' }, thesis: { type: 'string' } } } },
    ideas: { type: 'array', maxItems: 3, items: { type: 'object', additionalProperties: false, required: ['slug', 'status'], properties: { slug: { type: 'string' }, status: { type: 'string' }, thesis: { type: 'string' } } } },
    ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from_slug', 'klass', 'to_pkg', 'to_slug'], properties: { from_slug: { type: 'string' }, klass: { type: 'string', enum: ['in_scope', 'oos_samelang', 'cross_lang'] }, to_pkg: { type: 'string' }, to_slug: { type: 'string' } } } },
    gates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['blocked_slug', 'gated_by_slug', 'in_scope'], properties: { blocked_slug: { type: 'string' }, gated_by_slug: { type: 'string' }, in_scope: { type: 'boolean' } } } },
  } } },
  malformed_ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from_slug', 'raw'], properties: { from_slug: { type: 'string' }, raw: { type: 'string' } } } },
} }
// Required-but-possibly-empty `ripples`/`pins` are attestations: ripple repair ran in-pass and
// the central pin is the run's ONE handoff — an empty array attests none arose, never a skip.
const RIPPLES = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['counterpart', 'action'], properties: { counterpart: { type: 'string' }, action: { type: 'string' } } } }
const PINS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'row'], properties: { package: { type: 'string' }, row: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'ripples', 'pins', 'summary'], properties: {
  folder: { type: 'string' },
  verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
  realized: { type: 'array', items: { type: 'string' } },
  deferred: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  collapsed: { type: 'string' },
  ripples: RIPPLES,
  pins: PINS,
  summary: { type: 'string' },
} }
const REDTEAM_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'ripples', 'pins', 'closed', 'reopened', 'summary'], properties: {
  folder: { type: 'string' },
  verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
  realized: { type: 'array', items: { type: 'string' } },
  deferred: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  collapsed: { type: 'string' },
  ripples: RIPPLES,
  pins: PINS,
  closed: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'disposition', 'strength'], properties: { slug: { type: 'string' }, disposition: { type: 'string', enum: ['complete', 'dropped'] }, strength: { type: 'string', enum: ['strong', 'partial', 'weak'] } } } },
  reopened: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }
const PIN_SCHEMA = { type: 'object', additionalProperties: false, required: ['applied', 'rejected', 'summary'], properties: {
  applied: { type: 'array', items: { type: 'string' } },
  rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'reason'], properties: { package: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }

// --- [DOCTRINE] --------------------------------------------------------------------------
const FB = ' (the `.api` catalogs, Context7/exa/tavily for the official surface, and the `nuget` MCP for NuGet-side members own the fallback when assay is unavailable)'
const LAW = [
  'Rasm monorepo, libs/python planning corpus (markdown specs of intended Python module designs). CLAUDE.md manifest + WORKSPACE_LAW strata ' +
    'govern. The session targets are the libs/python package folders ' + TARGET_NAMES + '. Each holds `IDEAS.md` ' +
    '+ `TASKLOG.md` + `ARCHITECTURE.md` + `README.md` at its package ROOT, design pages at `<folder>/.planning/<subdomain>/*.md`, and a ' +
    'folder-specific `.api/*.md` catalog. The language-wide `libs/python/.planning` is OUT of scope this run. Read the package-root ' +
    '`ARCHITECTURE.md` (sub-domain map + `[02]-[SEAMS]`) and `README.md` (admitted-package roster) as governing context. Cross-folder repair ' +
    'lands at seams, counterpart cards, and consumer sites — never by rebuilding a sibling owner interior.',
  'STANDARD: docs/stacks/python/ is the route-owned law ' +
    '(README/language/shapes/surfaces-and-dispatch/rails-and-effects/algorithms/iteration/concurrency/boundaries/runtime/system-apis) — author ' +
    'Python as dense, polymorphic, and rich as that bar admits; docs/stacks/csharp/ is the density/ambition FLOOR (match its richness, never ' +
    'import C#-shaped idioms). READ the operative docs/stacks/python pages and conform exactly. Cite ONLY members confirmed in the .api catalogs; ' +
    'verify any novel member via `uv run python -m tools.assay api`' + FB + '.',
  'This is IMPLEMENT, not the in-isolation api-stacking rebuild: realize the folder SPECIFIC open IDEAS/TASKLOG cards into deep design-page ' +
    'FENCES. A FENCE is a markdown fenced code block inside a `.planning` design page — the work product itself, NEVER a `.py` source file. SCOPE ' +
    'per target: realize ALL open tasks (including `Atomic`-flagged minor tasks), then the 1-3 chosen open ideas, tasks first. Realize tied to the ' +
    'card charter (Capability/Shape/Unlocks/Anchors), composing the right admitted capability and crushing surface sprawl into fewer richer owners ' +
    'with zero functionality loss.',
  'TWO-TIER .api: every fence draws on BOTH the shared/universal catalogs at `' + SHARED_API + '/*.md` (anyio, expression, msgspec, pydantic, ' +
    'pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*, protobuf, grpcio, and siblings — the disk listing owns the ' +
    'roster) AND the folder-specific catalogs at ' +
    '`<folder>/.api/*.md`. The shared tier is SHARED capability you MUST consider and compose to realize the card properly — never re-derive by ' +
    'hand or settle for a thin folder-only subset; layer the shared rails (expression `Result`/`Option`, msgspec/pydantic discriminated models, ' +
    'beartype validation, stamina retry, structlog + opentelemetry spans, anyio structured concurrency) ON TOP OF the folder-specific domain ' +
    'packages. This is implement (use the capability the card needs), not rebuild (max-stack every catalog for its own sake).',
  'WRITE-FULLY + FIX-IT-NOW: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge. A cross-file ripple your edit exposes is YOURS in the ' +
    'same pass, wherever it lives: the seam counterpart on both ends, the consumer site, the stale sibling page, the 1-hop counterpart card fence ' +
    'in another libs/python folder — repaired now and recorded in `ripples` (an empty `ripples` attests your pass exposed none, never that repair ' +
    'was skipped). The ONE handoff this run permits is the central `' + CENTRAL + '` pin: report the exact row + band marker in `pins` for the ' +
    'run\'s single manifest writer. If after real investigation a fence is already correct, say so — never invent edits to look busy.',
].join('\n')
const CARD = [
  'CARD SCHEMA: open cards live in the target `IDEAS.md` (ideas — larger conceptual capability) and `TASKLOG.md` (tasks — concrete targeted work), ' +
    'under section `[01]-[OPEN]`; closed cards collapse under `[02]-[CLOSED]`. A card is `[ID]-[STATUS]: <thesis>` then the bullets `Capability:` ' +
    '/ `Shape:` / `Unlocks:` / `Anchors:` / `Tension:` (only when a constraint shapes it) / `Ripple:` (only on a cross-folder counterpart) / ' +
    '`Atomic:` (only on a minor task). Open statuses: `ACTIVE` / `QUEUED` / `BLOCKED`. Closed: `COMPLETE` / `DROPPED`. ALWAYS read the FULL card ' +
    'body (every bullet) from disk — the thesis alone is never enough to realize the charter.',
  'RIPPLE: `Ripple: <lang>:<pkg> [SLUG]` (or `<pkg> [SLUG]`) is a BIDIRECTIONAL cross-folder link — the counterpart card in the named pkg carries ' +
    'the mirror slug, and ripples are PART of scope, repaired in the pass that exposes them, never handed to a later stage. Three classes: ' +
    'IN-SCOPE (counterpart is another Python session target — its own pipeline realizes its card; you align your half of the seam to the ' +
    'counterpart page as it NOW stands on disk, and the later-landing side owns the final alignment), OUT-OF-SCOPE SAME-LANGUAGE (counterpart in ' +
    'a non-target libs/python folder — YOU realize the 1-hop counterpart card fence and align the seam on both ends in the same pass; the ' +
    'ripple\'s scope is that counterpart card and its seam, not the foreign folder\'s other cards), CROSS-LANGUAGE / LIB-WIDE (`libs/csharp`, ' +
    '`libs/typescript`, `libs/python/.planning`, `libs/.planning` — outside this Python-only run\'s language rail; land your half stating the ' +
    'wire contract, and the card stays open unless it is complete on your half alone).',
  'PROBE FREELY (nothing gates probing): EVERY agent in EVERY phase may — and should — probe to verify reality at any time, for ANY card or design ' +
    'decision, not only `[BLOCKED]` ones — `uv run python -m tools.assay api resolve|query` over Python distributions / host DLLs / NuGet / ' +
    'node_modules to confirm any member; `uv run python -m tools.assay provision check` (+ `pyproject.toml` + tools/assay/README.md) for a ' +
    'native/scientific/database/provisioning band (sanitized Rasm evidence — direct `forge-provision` is Forge-level debugging, not the normal ' +
    'entry); Rhino WIP (never Rhino 8) via the rhino-mcp skill or tools/rhino-bridge for live host behavior. tools/assay is under concurrent ' +
    'construction: when an assay invocation fails, the probe obligation stands and reroutes — the `.api` catalogs, Context7/exa/tavily for the ' +
    'official surface, the `nuget` MCP for NuGet-side members — and a blocker provable ONLY through downed assay is a legitimate out-of-run ' +
    'blocker, never a faked resolution. A `[BLOCKED]` card is REALIZED this turn whenever a probe resolves its blocker OR its gating work is in ' +
    'scope; a blocker is genuinely legitimate ONLY when it depends on work outside this run.',
  'PACKAGE ADMISSION (only when a card genuinely needs a not-yet-admitted package): the new dependency is exactly ONE of three bands, which ' +
    'decides the central `' + CENTRAL + '` marker AND the install/reflect path — (a) pure-Python wheel -> installs to `.venv`; (b) ' +
    'scientific/native (numpy/scipy-class, C/C++/Fortran build) -> NOT in `.venv`, the `forge-scientific-*` env, decided via `uv run python -m ' +
    'tools.assay provision check` (the package own wheel/build metadata via Context7/PyPI evidence decides the band when assay is unavailable); ' +
    '(c) companion-band -> pinned with a `; python_version<\'3.15\'` marker. Do the folder-local parts NOW — add the package to the correct group ' +
    'in the target `README.md` and author the target `.api/<package>.md` from `uv run python -m tools.assay api`' + FB + '. The central repo-root ' +
    '`' + CENTRAL + '` has exactly ONE in-run writer: report the exact dependency row + band marker in `pins` and never edit that file yourself. ' +
    'Never a per-folder `' + CENTRAL + '`.',
  'CARD CLOSURE (the folder red-team ONLY — implement and critique NEVER change card status): a genuinely-complete card moves to its file ' +
    '`[02]-[CLOSED]` section as a collapsed one-liner `[ID]-[COMPLETE]: <one-line disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`); ' +
    'update the target `ARCHITECTURE.md` `[02]-[SEAMS]` ONLY when a real cross-folder seam landed. A ripple-carrying card closes COMPLETE only ' +
    'when its seam is verified landed on BOTH ends on current disk; close only `strong` cards and honestly re-open the rest.',
].join('\n')
const BARHUNT = [
  'BAR — a high-value IMPLEMENT leaves every owner capturing the capability the card needs from the packages it admits, every sprawl collapsed ' +
    'into one denser owner with NO capability lost, and every fence transcription-complete against the verified `.api`. The critique guards ' +
    'capability conservation, charter completeness, and density; the red-team attacks every fence for a surface that could still collapse, a thin ' +
    'wrapper, a silent functionality drop during a refactor, a missed package capability the card needs, or a framework violation, and fixes each ' +
    'in place.',
  'HUNT (at implement, critique, and red-team alike): UNDER-CAPTURED CAPABILITY — an admitted package whose `.api` and code expose capability the ' +
    'CARD needs but no owner exploits is a gap, closed by deepening a fence. SURFACE SPRAWL — parallel types/functions/near-duplicate shapes ' +
    'collapse into one parameterized owner (closed family / `@tagged_union` / derived `frozendict` table / fold / stacked aspect) with no ' +
    'functionality removed. RAIL UNIFICATION — one entrypoint family per rail, one closed fault vocabulary per domain, total `match` + ' +
    '`assert_never`. OPTIMIZATION — correctness first, then allocation/vectorization (numpy)/dispatch shape/algorithmic complexity, not only ' +
    'line-count. NEW WORK SURFACED — api gaps and tasks the implementation exposes are realized the same turn.',
  'NAIVETY — a defect on two orthogonal axes, both intolerable at every stage: COVERAGE — the owner models a thin slice of its concept (the ' +
    'obvious three fields where the domain carries fifteen; a two-case family for a twenty-case domain); APPROACH — enumerated hardcoded ' +
    'instances where a parameterized algorithmic owner should generate the space (a fixed roster of styles/patterns/variants is seed DATA ' +
    'feeding ONE generator over named parameters, never the mechanism itself). Rebuild the owner to the generative form on either hit. COLLAPSE ' +
    'FREEDOM: every enumerated collapse-signal list in this prompt is a FLOOR, never the complete set — any repeated structure, parallel ' +
    'spelling, or enumerable family an algebra, table, fold, or generator can own is a collapse target you find yourself. Dense, ' +
    'confident-looking work is the prime suspect for hollowness: hold every fence naive until it survives attack.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE: docs/stacks/python/ is the route-owned law — READ `README.md` (the 16 laws + the 12-signal COLLAPSE_SCAN), `shapes.md` ' +
    '(OWNER_CHOOSER + the lifecycle), `surfaces-and-dispatch.md` (dispatch forms + ASPECTS), and `rails-and-effects.md` (rail/effect law) before ' +
    'realizing, and hold every fence to them as fact. docs/stacks/csharp/ is the density/ambition FLOOR — match its richness, never import ' +
    'C#-shaped idioms.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows `Raw -> Payload -> Canonical owner -> Rail -> Projection -> Egress`. Raw material is ' +
    'admitted EXACTLY ONCE into an evidence-carrying owner (Pydantic/`TypedDict` payload at ingress); interior code never re-validates, never sees ' +
    '`None`-as-failure, sentinels, or provider shapes; egress projects outward (`msgspec.Struct` wire) from the canonical owner. Parameterize BOTH ' +
    'ingress AND egress so the same owner sources and sinks across many providers/apps without touching its interior.',
  'SHAPE LAW: one concept owns exactly ONE type (SHAPE_BUDGET) — variants are cases in one closed family, never sibling types; one rich ' +
    'polymorphic surface over many shallow (DEEP_SURFACES); the owner is shaped for the family it will ABSORB (ANTICIPATORY_COLLAPSE) so the next ' +
    'case/dimension/modality lands as ONE declaration with every consumer untouched or broken loudly at type-check. Choose each owner by the ' +
    'OWNER_CHOOSER discriminants — admission (trusted/untrusted), identity regime (value/tag/key/reference), variant arity ' +
    '(one/closed-family/open), payload timing (def-time/runtime), openness (closed/semi/open) -> the right owner among `TypedDict`, Pydantic, ' +
    '`msgspec.Struct`, frozen dataclass, rich class, `StrEnum`/`Literal`, `sentinel`, `Option`/`Result`, `frozendict`/`Map`/`tuple`, `Protocol`. A ' +
    'misplaced shape traces to one mis-answered discriminant.',
  'ASPECT-FIRST (DEFINITION_TIME_ASPECTS): every CROSS-CUTTING capability — retry, telemetry/spans, validation, contracts, memoization, ' +
    'registration, receipts, fault rails — is a SIGNATURE- and RAIL-PRESERVING decorator (inline `**P` + `functools.wraps`) that materializes ' +
    'policy, STACKS in deterministic order (bottom-up at definition, top-down at call), and NEVER raises into domain flow (a failing aspect ' +
    'returns the rail `Error`). Co-occurring wrappers sharing an admission path collapse into ONE parameterized aspect factory. Code reads as STACKED ' +
    'DECORATORS over a thin pure core, never inline-repeated concerns or sibling helper functions; the domain transform itself stays a pure ' +
    'function/fold.',
  'DERIVATION + ARITY: cases sharing generative structure are DERIVED — one primary `frozendict` correspondence declared, every secondary map ' +
    'derived from it (DERIVED_LOGIC), or a fold/comprehension — never enumerated arms. Configuration enters as ONE behavior-carrying value ' +
    '(vocabulary member, tagged variant, frozen policy table), never flag sets the body re-derives (POLICY_VALUES). ONE entrypoint owns every ' +
    'modality (singular/plural/batch/stream), discriminating on the INPUT SHAPE (`T | Iterable[T]` normalized once at the head), never a name ' +
    'suffix or a `mode`/`batch` knob (MODAL_ARITY); a `timeout`/`retry`/`deadline` is an aspect or an `anyio` scope, never a signature param ' +
    '(KNOB_TEST). Capability exits through FEW dense unified entry points — one polymorphic entry per rail discriminating on input shape ' +
    '(single|batch|stream absorbed by input detection, forward and inverse directions on one surface), variation living in input shape, policy ' +
    'values, and table rows, never parallel exports or modality-named siblings; the surface narrows by absorption, never by omission.',
  'RAILS (rails-and-effects): the narrowest carrier that states the outcome, chosen ONCE at admission — `Option[T]` non-failing absence, ' +
    '`Result[T, E]` typed fallibility, `effect.result` do-notation for sequential `bind`, `Block`/`Map` immutable traversal, an `anyio` task group ' +
    'as the failure boundary (NEVER `asyncio.gather`), `stamina.retry` as the decorator (never a sleep-loop). The fault type `E` is a CLOSED ' +
    'vocabulary — `Literal` set, `StrEnum`, or `@tagged_union` family — NEVER a bare `str` for a multi-cause domain. Accumulate-vs-abort is a ' +
    'correctness decision fixed at the boundary: `map2`/accumulating-fold for independent operands (a `bind` chain over independents reports only ' +
    'the first failure), `bind` short-circuit for dependent steps. Cancellation is not failure; resource cleanup is `AsyncExitStack` + a shielded ' +
    'scope.',
  'STACK .api CAPABILITY: ENUMERATE BOTH catalog tiers IN FULL with a real listing (`ls`/`fd` of the shared `' + SHARED_API + '/*.md` AND the ' +
    'folder-specific `<folder>/.api/*.md`, from disk, never memory), then mine every catalog the card touches to OPERATOR DEPTH and compose the ' +
    'capability into single dense operations woven as ONE rail, layering the shared rails (expression `Result`/`Option`, msgspec/pydantic ' +
    'discriminated models, beartype validation, stamina retry, structlog + opentelemetry spans, anyio structured concurrency) ON TOP OF the ' +
    'folder-specific domain packages — e.g. `msgspec dec_hook` -> pydantic discriminated union -> stamina `retry_context` -> opentelemetry span ' +
    'around the domain op — NOT flat one-shot per-library uses. Use the DEEPEST primitive each package itself reaches (LIBRARY_DEPTH). An ' +
    'admitted capability the card charter needs that no owner exploits is a DEFECT the pass closes by deepening a fence; a cited member that ' +
    'cannot be verified in the catalogs or via `uv run python -m tools.assay api`' + FB + ' is a PHANTOM the pass deletes or corrects on sight. (Implement ' +
    'composes the capability the card needs; it does not max-stack every catalog for its own sake — that is rebuild.)',
  'PRESERVE all capability (densify, never delete functionality): capability is improved or extended, NEVER dropped for lack of a current ' +
    'consumer — zero consumers never lowers the bar; planned consumers are real design pressure. Where a fence is already dense, deepen; where ' +
    'it is flat/naive, rebuild ground-up. Never regress correctness or boundary law.',
].join('\n')
const PATLAW = [
  'PY-VERSION LAW: target Python 3.15 on the full modern band (3.11/3.12/3.13/3.14/3.15) — advanced patterns ONLY, zero legacy idioms, IDENTICAL ' +
    'conventions across every folder and package.',
  'NEVER write `from __future__ import annotations`. NEVER use legacy typing: use PEP 585 builtin generics (`list[T]`, `dict[K, V]`, `tuple[...]`, ' +
    '`set[T]`) NOT `typing.List/Dict/Tuple/Set`; PEP 604 unions (`X | None`, `A | B`) NOT `Optional`/`Union`; PEP 695 type parameters (`class ' +
    'C[T]:`, `def f[T](...)`, `type Alias[T] = ...`) NOT `TypeVar` + `Generic`. Use `Self`, `override`, `TypeIs`/`TypeGuard`, `assert_never`, ' +
    '`ReadOnly`, `TypedDict` + `NotRequired`/`Required`, `LiteralString`, `enum.StrEnum`/`IntEnum`, and `@dataclass(slots=True, frozen=True)` or ' +
    '`msgspec.Struct`/pydantic models where each best fits.',
  'PAYLOADS — NEWEST FORM: ingress payloads are static `TypedDict` contracts with `closed=True` or `extra_items=T` and per-key ' +
    '`Required[]`/`NotRequired[]`/`ReadOnly[T]`, admitted through a module-level `TypeAdapter`, with `Unpack[TypedDict]` at root keyword ' +
    'entrypoints (never forwarded through interiors); extension bands fold into `frozendict`/tuple evidence at materialization, and ' +
    '`msgspec.Struct(frozen=True)` owns wire/egress. NO `dict[str, Any]` bags, homogeneous `**kwargs`, or `Mapping[str, object]` payloads.',
  'FROZENDICT (py3.15 builtin): `from builtins import frozendict` is the owner for immutable map rows, dispatch/policy TABLES (one primary ' +
    '`frozendict[K, tuple[...]]`, secondary maps derived from it), payload `extra_items` extension bands, and immutable evidence — REJECT ' +
    '`MappingProxyType`, a module-level mutable `dict` used as a table, tuple-pair pseudo-maps, and mutate-then-freeze. Prefer total ' +
    '`match`/structural pattern matching over if-chains, walrus where it tightens, `assert_never` on closed unions, and PEP 750 t-strings / PEP ' +
    '749 deferred annotations where relevant. Keep every choice CONSISTENT across folders so the corpus reads as one ultra-advanced codebase.',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every folder owner strictly in its lane; internal code uses canonical names and shapes with mapping only at ' +
  'the edge; respect the dependency direction of the workspace strata. Cross-folder repair is seam-shaped: align counterparts, consumer sites, ' +
  'and counterpart cards — a concern owned twice across a runtime, a folder mixing unrelated concerns, or coupling to a sibling owner INTERIOR ' +
  '(vs its wire/seam) is a defect.'
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
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in backticks. Name ' +
    'the exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design ' +
    'content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
  'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY ' +
  'where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a ' +
  'comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no ' +
  'task/process/session/history/proof/review comments, no docstring bloat. Densify names and types so comments are rarely needed; cut every ' +
  'low-value comment.'
const DOCTRINE = [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', CURRENT, '', PROSE, '', COMMENTS].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const folderName = (p) => p.split('/').filter(Boolean).pop() || p
const discoverPrompt = (targets) => [LAW, '', CARD, '',
  'TASK: DISCOVER + SEQUENCE the open work across these Python session targets: ' + JSON.stringify(targets) + '. You are the read-only ' +
    'reconnaissance that grounds every downstream stage, and read-only is your ONLY concession: every row you return is grounded in REAL DISK ' +
    'STATE, never memory. FIRST enumerate with a real listing (`ls`/`fd`, from the source of truth): each target folder at large, its ' +
    '`.planning/**` pages, its `.api/*.md` catalogs, the shared `' + SHARED_API + '/*.md` tier, and the `docs/stacks/python/` doctrine inventory — ' +
    'resolve the target set against what actually exists (a missing folder, card file, or section is a finding to report, never a gap to paper ' +
    'over). THEN read each target package-root `IDEAS.md` + `TASKLOG.md` IN FULL — every open card body, every bullet — open the counterpart card ' +
    'file of every `Ripple:` to locate the mirror slug ON DISK, and FULL-READ the design pages each open card\'s `Anchors:` name — ground order, ' +
    'gates, and every klass call in real page state, never card text alone. For EACH target return: (1) folder — echo the target folder path ' +
    'exactly; (2) ' +
    'tasks — EVERY open card in `TASKLOG.md` (status ACTIVE/QUEUED/BLOCKED; carry the Atomic flag); (3) ideas — the 1-3 MOST actionable open cards ' +
    'in `IDEAS.md` (tasks-first doctrine: at most 3, the ones whose Anchors are most settled and whose ripples land on in-scope targets), HARD CAP ' +
    '3; (4) order — ONE sequenced slug list, ALL tasks first in dependency order then the chosen ideas; (5) ripples — for EVERY card carrying a ' +
    '`Ripple:` field, one row {from_slug, klass, to_pkg, to_slug}: klass=`in_scope` if to_pkg is one of these targets, `oos_samelang` if it is ' +
    'another libs/python folder, `cross_lang` if it points at `libs/csharp` / `libs/typescript` / `libs/python/.planning` / `libs/.planning`; (6) ' +
    'gates — for any [BLOCKED] card, {blocked_slug, gated_by_slug, in_scope} where in_scope is true iff the gating work is itself an open card in ' +
    'one of these targets. Also return malformed_ripples for any `Ripple:` line you cannot parse into a pkg+slug, or whose counterpart slug you ' +
    'cannot locate on disk. Classify from the FULL card body against the real files — never from the thesis, never from memory; list only slugs, ' +
    'files, and gates you verified on disk, never a phantom. Carry each task/idea row `thesis` as a one-line MAP hook: the charter\'s composed ' +
    'capability, the concrete verified `.api` members it stacks, and a hostile weak/strong call on the current fence state. Your product is a ' +
    'MAP, not a verdict, and it is an initial pointer, never a ceiling: downstream agents re-read everything and it licenses no skim. Return ' +
    'the structured map ONLY; edit nothing.'].join('\n')
const implementPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: IMPLEMENT — realize the open cards of `' + folder + '` into deep design-page FENCES at the doctrine bar. The sequenced worklist (slugs + ' +
    'ripple map; read each FULL card body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`, never the thesis alone):\n' + seq + '\nREAD: ' +
    'each card full body; every design page the card names under `' + folder + '/.planning/**`; the sibling pages it seams to, at their CURRENT ' +
    'on-disk state; the package-root `ARCHITECTURE.md` + `README.md`; the operative docs/stacks/python/ pages (docs/stacks/csharp/ as the ambition ' +
    'floor); BOTH .api tiers — the shared `' + SHARED_API + '/*.md` AND the folder `' + folder + '/.api/*.md` (stack them, the shared rails ' +
    'layered onto the folder packages); and verify any novel member via `uv run python -m tools.assay api`' + FB + '. Realize EVERY card in ' +
    '`order` (all tasks incl. Atomic, then the ideas) into deep fences in the `' + folder + '` design pages, in LIFECYCLE order (admit raw ONCE ' +
    'into a typed `TypedDict`/Pydantic payload -> materialize into the canonical owner the OWNER_CHOOSER discriminants select -> weave every ' +
    'cross-cutting concern (retry/telemetry/validation/contracts/memo/receipts) as a STACKED signature+rail-preserving aspect over a thin pure ' +
    'core -> compose the domain transform through ONE unified `Result`/`Option` rail with total `match` -> project + `msgspec.Struct` egress, ' +
    'BOTH ingress and egress parameterized). Collapse parallel shapes into one closed family/ADT (`@tagged_union`); drive cases with a derived ' +
    '`frozendict` table or fold; one polymorphic entrypoint per modality (`T | Iterable[T]` normalized once). py3.15-modern only (PEP ' +
    '585/604/695, `frozendict` builtin, newest payload forms; NO `from __future__ import annotations`, NO legacy typing, NO ' +
    '`asyncio`/`None`-as-failure). Resolve any [BLOCKED] card inline (assay api for members' + FB + '; `uv run python -m tools.assay provision ' +
    'check` for native/scientific/provisioning bands). RIPPLES ARE YOURS: repair every ripple your cards carry in this same pass per the RIPPLE ' +
    'law — align each in-scope seam to the counterpart page as it NOW stands on disk, realize each 1-hop out-of-scope same-language counterpart ' +
    'card fence and align both ends, land your half of each cross-language seam stating the wire contract — and record each repair in `ripples`. ' +
    'PACKAGE ADMISSION per the card law: decide the band, folder-local parts NOW, the central `' + CENTRAL + '` row + band marker reported in ' +
    '`pins`, never edited. Do NOT close any card — the folder red-team owns card status. High-signal prose all-backticked, comment hygiene, ' +
    'fix-in-place (read-then-extend, preserve capability). Return verdict + realized slugs + deferred (any card you could not realize, with ' +
    'reason) + collapsed (before->after counts) + ripples + pins + summary.'].join('\n')
const critiquePrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: DOCTRINAL-CONFORMANCE AUDIT + CHARTER-COMPLETENESS + FIX IN PLACE across `' + folder + '`. You are an ULTRA-HARSH, UNAGREEABLE auditor: ' +
    'assume a violation exists in every fence until you prove otherwise, and "good enough" is rejected. The cards realized this turn (read each ' +
    'FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`):\n' + seq + '\nREAD the realized pages under `' + folder + '/.planning/**`, ' +
    'the sibling pages at their CURRENT on-disk state, the operative docs/stacks/python/ pages, and BOTH .api tiers (shared `' + SHARED_API + '` + ' +
    'folder `' + folder + '/.api`). Run these MECHANICAL checklists line-by-line and REPAIR every hit in place (a fix, never a ledger note); the ' +
    'checklists are a FLOOR you hunt past, never the complete audit:',
  '(1) COLLAPSE_SCAN — apply the move for any of the 12 signals (shapes sharing an identity regime, an admission path, a payload timing, or a ' +
    'consumer collapse into ONE owner; a shape survives only on a genuinely distinct discriminant): sibling prefix/suffix names -> one ' +
    'modality-polymorphic entrypoint; same return rail differing only by arity -> input-shape discrimination; a `get`/`get_many`/`get_by_id` ' +
    'family -> one input-keyed entrypoint; functions differing only by a literal -> parameterize the literal as policy; a bool parameter selecting ' +
    'two bodies -> one derived body or policy value; a function calling exactly one other -> delete the hop; a class exposing one public method -> ' +
    'module function or fold-on-owner; parallel dispatch arms repeating structure -> a `frozendict` table or fold algebra; several types sharing ' +
    'fields for one concept -> one closed family; sibling module constants sharing one concept -> one `frozendict`/`StrEnum`; a wrapper renaming a ' +
    'package API -> use the package surface directly; co-occurring wrappers sharing an admission path -> one parameterized aspect factory. The 12 signals are a ' +
    'FLOOR — hunt collapse targets past them.',
  '(2) OWNER_CHOOSER — for EVERY shape re-derive the owner from the 5 discriminants (admission, identity regime, variant arity, payload timing, ' +
    'openness); if it is not the discriminant-correct owner (`TypedDict`/Pydantic/`msgspec.Struct`/frozen dataclass/rich ' +
    'class/`StrEnum`/`Literal`/`sentinel`/`Option`/`Result`/`frozendict`/`Map`/`tuple`/`Protocol`), replace it. Kill every parallel DTO, one-field ' +
    'wrapper, field-rename class, tag-only shape, and `None`-as-failure.',
  '(3) KNOB_TEST — delete each parameter: if the value already encodes what it carried, it was a knob — collapse a `strict: bool`/`mode`/`batch` ' +
    'flag into a policy value or input-shape discriminant, and move every `timeout`/`retry`/`deadline` out of the signature into an aspect or ' +
    '`anyio` scope.',
  '(4) ASPECTS — every cross-cutting concern (retry/telemetry/validation/contracts/memo/registration/receipts) MUST be a signature+rail-preserving ' +
    'STACKED decorator that never raises into domain flow; co-occurring wrappers sharing an admission path collapse into one aspect factory; deterministic stacking ' +
    'order verified. Inline-repeated concerns and sibling helper functions are defects.',
  '(5) RAILS — narrowest carrier chosen once; the fault type is a CLOSED `Literal`/`StrEnum`/`@tagged_union` (a bare `str` fault for a multi-cause ' +
    'domain is a defect); accumulate-vs-abort disposition correct (`map2`/fold for independents, `bind` for dependents); NO `asyncio`, NO ' +
    'hand-rolled retry loop, NO `None`-as-failure, NO exception control flow in domain logic.',
  '(6) PAYLOADS/FROZENDICT/PEP — payloads are `closed=`/`extra_items=` `TypedDict` via a module-level `TypeAdapter` with `Unpack[TypedDict]` at ' +
    'root entrypoints; `frozendict` (builtin) owns tables/evidence (no `MappingProxyType`/dict-table/tuple-pairs); PEP 585/604/695 only, no `from ' +
    '__future__ import annotations`, no legacy typing; total `match` + `assert_never`.',
  '(7) CHARTER-COMPLETENESS — for EVERY card in the worklist, verify the realized fences GENUINELY fulfill its `Capability`/`Shape`/`Unlocks` ' +
    '(read the full card from disk): a missing modality, an unrealized `Shape` clause, a stubbed/placeholder fence, or a capability the card ' +
    'promises but the fences do not deliver is a DEFECT — realize it NOW. A card whose fences are thin against its charter is not done. Hunt ' +
    'BOTH naivety axes per card: COVERAGE (a thin slice of the charter concept) and APPROACH (a hardcoded roster where ONE parameterized ' +
    'generator should own the space); rebuild to the generative form on either hit.',
  '(8) SEAMS — check every cross-page and cross-folder symbol these cards compose against the counterpart as it NOW stands on disk: a signature ' +
    'mismatch corrects at the weaker end, a conflict resolves to the stronger form, never a revert; a seam counterpart or consumer site your fix ' +
    'exposes is repaired in this same pass wherever it lives, recorded in `ripples`.',
  'Also enforce both-tier `.api` use (a thin folder-only subset ignoring the shared rails the card needs is a defect), cross-folder convention ' +
    'consistency, and prose + comment hygiene. FIX every hit NOW wherever it lives per WRITE-FULLY; report any central `' + CENTRAL + '` row in ' +
    '`pins`. Return verdict + realized + deferred + collapsed + ripples + pins + summary.'].join('\n')
const redteamPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE + TERMINAL CLOSE across `' + folder + '`. You are the LAST and MOST AGGRESSIVE pass: ' +
    'assume the author and critique missed things and that the chosen design is not the strongest until proven, with the burden of proof ON THE ' +
    'DESIGN. The cards realized this turn (read each FULL body):\n' + seq + '\nOpen BOTH .api tiers (shared `' + SHARED_API + '` + folder ' +
    '`' + folder + '/.api`), the sibling pages at their CURRENT on-disk state, and the operative docs/stacks/python/ pages. Attack from every ' +
    'direction and REPAIR every defect in place — no soft-pedalling, no could/should, a fix never a ledger.',
  'PRIMARY LENS — fundamental design, multi-faceted / multi-dimensional / multi-directional: (A) COUNTERFACTUAL on the core choice — is the owner, ' +
    'the algebra, and the dispatch form categorically the strongest the doctrine admits, or does a denser owner, a fold/derived-table algebra, or ' +
    'a DEEPER admitted-package primitive collapse the whole fence? If a fundamentally stronger design exists, rebuild to it — never defend the ' +
    'incumbent. (B) ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: when the next case/dimension/knob/modality/provider arrives, ' +
    'does it land as ONE declaration with every consumer untouched (or broken loudly at type-check)? If it would touch multiple sites, reshape so ' +
    'the growth axis is a case, row, policy value, or carrier swap. (C) LONG-TAIL + MULTI-DIMENSIONAL — attack every input/output/edge/failure ' +
    'mode (empty, singular, plural, stream, malformed, concurrent, cancelled, partial-failure, version-skew); is the accumulate-vs-abort ' +
    'disposition correct for the REAL boundary; are BOTH ingress AND egress parameterized so this owner sources and sinks across many apps without ' +
    'interior edits? (D) BOUNDARY-INTEGRITY — a concern owned twice in a runtime, a folder mixing concerns, a concern scattered across folders, ' +
    'coupling to a sibling owner INTERIOR (vs its wire/seam), OR a sibling planning page left STALE by this folder change even when no ripple card ' +
    'names it (ports/boundaries/wires/seams drift) is a defect: fix it NOW wherever it lives — the stale sibling page, the seam counterpart, the ' +
    'consumer site — and record the repair in `ripples`. (E) SURFACE-SPRAWL-IN-TIME — an admitted package whose `.api` exposes capability the ' +
    'card needs but the fence re-derives by hand, flat code below the operator depth the packages reach, a phantom `.api` member, or a thin ' +
    'wrapper: collapse to package depth and verify the member exists (via `assay api`' + FB + ').',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (every time, NOT only on a structural restructure): re-attack every conformance dimension with fresh ' +
    'hostile eyes, trusting nothing the prior passes claimed — the COLLAPSE_SCAN signals, OWNER_CHOOSER per shape, the KNOB_TEST per param, the ' +
    'ASPECT taxonomy, rail + closed-fault-vocabulary discipline, charter-completeness per card, BOTH naivety axes (COVERAGE thin-slice; ' +
    'APPROACH roster-where-a-generator-should-own-the-space), payload/`frozendict`/PEP conformance, both-tier ' +
    '`.api` use, py3.15-modern typing, and prose/comment hygiene — and fix every defect. Even absent a structural rebuild, the fences must end ' +
    'objectively denser, more correct, and more powerful than the critique left them; if the strongest form is genuinely already present, prove it ' +
    'by finding nothing — never invent churn.',
  'TERMINAL CLOSE — you are `' + folder + '`\'s LAST stage and the SOLE owner of its card status. For EVERY card in scope this run, re-read its ' +
    'FULL body and the realized fences on CURRENT disk, then ADVERSARIALLY VERIFY — the fences are naive until they survive your attack, a prior ' +
    'pass verdict a rejected self-assessment — that they genuinely fulfill the card `Capability`/`Shape`/`Unlocks` against the verified `.api` ' +
    '(verify novel members via `uv run python -m tools.assay api`' + FB + '). FINAL-remediate any weak or partial realization in place NOW, then ' +
    'assign each card a strength: `strong` (every charter clause delivered, fences transcription-complete against the verified `.api`), `partial` ' +
    '(most delivered, a clause still thin), `weak` (charter not met). CLOSE only `strong` cards per the CARD CLOSURE law; a ripple card whose ' +
    'seam you cannot verify landed on BOTH ends on current disk stays OPEN with that reason; honestly RE-OPEN every card you cannot bring to ' +
    '`strong`, with a one-line reason (a real out-of-run or cross-language dependency). The orchestrator DEMOTES any card closed below `strong`, ' +
    'so never inflate. Return verdict + realized + deferred + collapsed + ripples + pins + closed [{slug, disposition, strength}] + reopened ' +
    '[{slug, reason}] + summary.'].join('\n')
const pinPrompt = (pins) => [LAW, '', PROSE, '',
  'TASK: CENTRAL PIN APPLY — you are the run\'s SOLE writer for the repo-root `' + CENTRAL + '` and its LAST agent. Apply each reported ' +
    'dependency row + band marker below exactly once, preserving the existing group/marker order and deduping semantically identical rows. ' +
    'Verify each package, version, and band before applying — pure-Python wheel vs scientific/native (`uv run python -m tools.assay provision ' +
    'check`; the package wheel/build metadata via Context7/PyPI evidence when assay is unavailable) vs companion-band `; python_version<\'3.15\'` ' +
    'marker. Confirm the folder README group and `.api/<package>.md` catalog landed; repair a missing folder-local part in place. Reject an ' +
    'unverifiable or malformed pin with its reason — never apply it silently. PINS:\n' + JSON.stringify(pins, null, 1)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// The single scheduler for every agent-bearing task in the run: CAP tasks in flight, staggered launch.
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  let gate = Promise.resolve()
  const launch = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  const run = async () => { while (next < items.length) { const i = next++; await launch(); out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}

phase('Discover')
const disc = (await agent(discoverPrompt(TARGETS), { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, effort: 'medium', stallMs: STALL })) || { targets: [], malformed_ripples: [] }
const withCards = (disc.targets || []).filter((t) => (t.tasks && t.tasks.length) || (t.ideas && t.ideas.length))
const crossLang = (disc.targets || []).flatMap((t) => (t.ripples || []).filter((rp) => rp.klass === 'cross_lang')
  .map((rp) => folderName(t.folder) + ' [' + rp.from_slug + '] -> ' + rp.to_pkg + ' [' + rp.to_slug + ']'))
log('Discover: ' + TARGETS.length + ' targets; ' + withCards.length + ' with open cards; pooling at CAP=' + CAP)

phase('Realize')
const runFolder = async (t) => {
  const seq = JSON.stringify({ order: t.order, tasks: t.tasks, ideas: t.ideas, ripples: t.ripples, gates: t.gates || [] }, null, 1)
  const tag = folderName(t.folder)
  const impl = await agent(implementPrompt(t.folder, seq), { label: 'implement:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: STALL })
  if (!impl) return { folder: t.folder, failed: true, logs: [], red: null } // failure isolation: a dead implement skips its reviews
  const crit = await agent(critiquePrompt(t.folder, seq), { label: 'critique:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: STALL })
  const red = await agent(redteamPrompt(t.folder, seq), { label: 'redteam:' + tag, phase: 'Realize', schema: REDTEAM_SCHEMA, effort: 'max', stallMs: STALL })
  return { folder: t.folder, failed: false, logs: [impl, crit, red].filter(Boolean), red }
}
const done = (await pool(withCards, CAP, runFolder)).filter(Boolean)
const failed = done.filter((r) => r.failed).map((r) => r.folder)
const deferred = done.flatMap((r) => r.logs.flatMap((l) => (l.deferred || []).map((d) => ({ folder: r.folder, slug: d.slug, reason: d.reason }))))
const ripplesRepaired = done.flatMap((r) => r.logs.flatMap((l) => l.ripples || []))
const pinsReported = [...new Map(done.flatMap((r) => r.logs.flatMap((l) => l.pins || [])).map((p) => [p.package + '|' + p.row, p])).values()]
let closed_count = 0
const reopened = []
for (const r of done) {
  for (const c of ((r.red && r.red.closed) || [])) {
    if (c.disposition === 'complete' && c.strength !== 'strong') reopened.push({ folder: r.folder, slug: c.slug, reason: 'demoted: strength=' + c.strength })
    else closed_count++
  }
  for (const ro of ((r.red && r.red.reopened) || [])) reopened.push({ folder: r.folder, slug: ro.slug, reason: ro.reason })
}
log('Realize: ' + (done.length - failed.length) + '/' + withCards.length + ' folders; ' + closed_count + ' cards closed, ' + reopened.length +
  ' re-open/demoted; ' + ripplesRepaired.length + ' ripple repair(s); ' + pinsReported.length + ' pin(s) reported' +
  (failed.length ? '; failed: ' + failed.join(', ') : ''))

let pinlog = null
if (pinsReported.length) {
  phase('Pins')
  pinlog = await agent(pinPrompt(pinsReported), { label: 'pins', phase: 'Pins', schema: PIN_SCHEMA, effort: 'medium', stallMs: STALL })
}

return {
  root: ROOT,
  targets: TARGETS,
  realized_folders: done.filter((r) => !r.failed).map((r) => r.folder),
  realize_failed: failed,
  deferred,
  ripples_repaired: ripplesRepaired.length,
  closed_count,
  reopened,
  pins: { reported: pinsReported.length, applied: (pinlog && pinlog.applied) || [], rejected: (pinlog && pinlog.rejected) || [] },
  cross_language: crossLang,
  malformed_ripples: disc.malformed_ripples || [],
}
