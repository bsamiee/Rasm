export const meta = {
  name: 'implement-py',
  whenToUse: 'Realize open IDEAS and TASKLOG cards into design-page code fences across the Python target folders.',
  description: 'Realize every open IDEAS/TASKLOG card across the Python target set (libs/python/artifacts, compute, data, geometry, runtime) into deep design-page code FENCES at the docs/stacks/python bar (with docs/stacks/csharp as the ambition floor), resolve all ripples, and truthfully close the cards. Per FOLDER, not per page: one discovery agent maps cards + ripple classes + blockers; each target folder is realized as ONE implement -> critique -> redteam cycle (all WRITE, both reviews adversarial, fix-in-place; BLOCKED probe + folder-local package admission inline, no prep phase); a bounded reconcile aligns in-scope seams, realizes 1-hop same-language counterparts, and applies the single central pyproject.toml pin serially; a final per-folder closeout verify-remediate-and-closes complete cards. Card-driven (it implements ideas/tasks), NOT the in-isolation api-stacking of rebuild-python. Disposable, Python-only. args = a target path string, an array of paths, or empty for the five defaults. The language-wide libs/python/.planning is out of scope.',
  phases: [
    { title: 'Discover', detail: 'one agent: enumerate folders + both .api tiers + doctrine from disk, full-read each target IDEAS/TASKLOG and anchored pages; extract open cards (all tasks incl atomic + 1-3 ideas) as map rows, sequence each folder, classify every ripple (in_scope / oos_samelang / cross_lang), record in-scope gates and malformed ripples' },
    { title: 'Realize', detail: 'per target folder, pooled at CAP: implement(max) -> critique(max, adversarial + charter-completeness) -> redteam(max, adversarial + staleness lens); all WRITE, fix-in-place, own-pages-only, cross-folder seams logged as residuals' },
    { title: 'Reconcile', detail: 'bounded single pass: cluster cross-folder residuals by shared file -> fix(max: align in-scope seam / realize 1-hop same-language counterpart / apply central pyproject.toml pin / defer cross-lang) -> writing verify(xhigh: re-derive necessity, prove on disk, repair weak fixes to root, then classify)' },
    { title: 'Closeout', detail: 'per folder: verify each card vs full charter, FINAL-remediate weak cards in place, close genuinely-complete (move to [02]-[CLOSED], collapse, update ARCHITECTURE [02]-[SEAMS]), honestly re-open the rest; strength-demotion makes closed mechanically truthful' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const STAGGER_MS = 1500
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
const TARGET_SET = new Set(TARGETS)

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
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: {
  folder: { type: 'string' },
  verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
  realized: { type: 'array', items: { type: 'string' } },
  deferred: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  collapsed: { type: 'string' },
  residual_ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, pkg: { type: 'string' }, slug: { type: 'string' }, mirror_slug: { type: 'string' }, claim: { type: 'string' } } } },
  summary: { type: 'string' },
} }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  verdict: { type: 'string', enum: ['fixed', 'clean'] },
  pairs: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['pkg', 'slug', 'mirror_slug', 'seam_landed'], properties: { pkg: { type: 'string' }, slug: { type: 'string' }, mirror_slug: { type: 'string' }, seam_landed: { type: 'boolean' } } } },
  admitted: { type: 'array', items: { type: 'string' } },
  deferred_legs: { type: 'array', items: { type: 'string' } },
  summary: { type: 'string' },
} }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, repaired_files: { type: 'array', items: { type: 'string' } }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const CLOSEOUT_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'summary'], properties: {
  folder: { type: 'string' },
  closed: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'disposition', 'strength'], properties: { slug: { type: 'string' }, disposition: { type: 'string', enum: ['complete', 'dropped'] }, strength: { type: 'string', enum: ['strong', 'partial', 'weak'] } } } },
  reopened: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo, libs/python planning corpus (markdown specs of intended Python module designs). CLAUDE.md manifest + WORKSPACE_LAW strata ' +
    'govern. The session targets are the libs/python package folders `artifacts`, `compute`, `data`, `geometry`, `runtime`. Each holds `IDEAS.md` ' +
    '+ `TASKLOG.md` + `ARCHITECTURE.md` + `README.md` at its package ROOT, design pages at `<folder>/.planning/<subdomain>/*.md`, and a ' +
    'folder-specific `.api/*.md` catalog. The language-wide `libs/python/.planning` is OUT of scope this run. Read the package-root ' +
    '`ARCHITECTURE.md` (sub-domain map + `[02]-[SEAMS]`) and `README.md` (admitted-package roster) as governing context; never trample a sibling ' +
    'folder owner.',
  'STANDARD: docs/stacks/python/ is the route-owned law ' +
    '(README/language/shapes/surfaces-and-dispatch/rails-and-effects/algorithms/type-system/boundaries/runtime/system-apis) — author Python as ' +
    'dense, polymorphic, and rich as that bar admits; docs/stacks/csharp/ is the density/ambition FLOOR (match its richness, never import ' +
    'C#-shaped idioms). READ the operative docs/stacks/python pages and conform exactly. Cite ONLY members confirmed in the .api catalogs; verify ' +
    'any novel member via `uv run python -m tools.assay api`.',
  'This is IMPLEMENT, not the in-isolation api-stacking rebuild: realize the folder SPECIFIC open IDEAS/TASKLOG cards into deep design-page ' +
    'FENCES. A FENCE is a markdown fenced code block inside a `.planning` design page — the work product itself, NEVER a `.py` source file. SCOPE ' +
    'per target: realize ALL open tasks (including `Atomic`-flagged minor tasks), then the 1-3 chosen open ideas, tasks first. Realize tied to the ' +
    'card charter (Capability/Shape/Unlocks/Anchors), composing the right admitted capability and crushing surface sprawl into fewer richer owners ' +
    'with zero functionality loss.',
  'TWO-TIER .api: every fence draws on BOTH the shared/universal catalogs at `libs/python/.api/*.md` (anyio, expression, msgspec, pydantic, ' +
    'pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*, protobuf, grpcio) AND the folder-specific catalogs at ' +
    '`<folder>/.api/*.md`. The shared tier is SHARED capability you MUST consider and compose to realize the card properly — never re-derive by ' +
    'hand or settle for a thin folder-only subset; layer the shared rails (expression `Result`/`Option`, msgspec/pydantic discriminated models, ' +
    'beartype validation, stamina retry, structlog + opentelemetry spans, anyio structured concurrency) ON TOP OF the folder-specific domain ' +
    'packages. This is implement (use the capability the card needs), not rebuild (max-stack every catalog for its own sake).',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FOLDER ' +
    'items (report those in residual_ripples). If after real investigation a fence is already correct, say so — never invent edits to look busy.',
].join('\n')
const CARD = [
  'CARD SCHEMA: open cards live in the target `IDEAS.md` (ideas — larger conceptual capability) and `TASKLOG.md` (tasks — concrete targeted work), ' +
    'under section `[01]-[OPEN]`; closed cards collapse under `[02]-[CLOSED]`. A card is `[ID]-[STATUS]: <thesis>` then the bullets `Capability:` ' +
    '/ `Shape:` / `Unlocks:` / `Anchors:` / `Tension:` (only when a constraint shapes it) / `Ripple:` (only on a cross-folder counterpart) / ' +
    '`Atomic:` (only on a minor task). Open statuses: `ACTIVE` / `QUEUED` / `BLOCKED`. Closed: `COMPLETE` / `DROPPED`. ALWAYS read the FULL card ' +
    'body (every bullet) from disk — the thesis alone is never enough to realize the charter.',
  'RIPPLE: `Ripple: <lang>:<pkg> [SLUG]` (or `<pkg> [SLUG]`) is a BIDIRECTIONAL cross-folder link — the counterpart card in the named pkg carries ' +
    'the mirror slug, and ripples are PART of scope. Three classes: IN-SCOPE (counterpart is another Python session target — each realizes its OWN ' +
    'half, the seam aligns in reconcile), OUT-OF-SCOPE SAME-LANGUAGE (counterpart in a non-target libs/python folder — reconcile realizes the ' +
    '1-hop counterpart), CROSS-LANGUAGE / LIB-WIDE (`libs/csharp`, `libs/typescript`, `libs/python/.planning`, `libs/.planning` — a deferred leg ' +
    'whose counterpart the other-language workflow realizes, NOT realized this Python-only run).',
  'PROBE FREELY (nothing gates probing): EVERY agent in EVERY phase may — and should — probe to verify reality at any time, for ANY card or design ' +
    'decision, not only `[BLOCKED]` ones — `uv run python -m tools.assay api resolve|query` over Python distributions / host DLLs / NuGet / ' +
    'node_modules to confirm any member; `uv run python -m tools.assay provision check` (+ `pyproject.toml` + tools/assay/README.md) for a ' +
    'native/scientific/database/provisioning band (sanitized Rasm evidence — direct `forge-provision` is Forge-level debugging, not the normal ' +
    'entry); Rhino WIP (never Rhino 8) via the rhino-mcp skill or tools/rhino-bridge for live host behavior. A `[BLOCKED]` card is REALIZED this ' +
    'turn whenever a probe resolves its blocker OR its gating work is in scope; a blocker is genuinely legitimate ONLY when it depends on work ' +
    'outside this run.',
  'PACKAGE ADMISSION (only when a card genuinely needs a not-yet-admitted package): the new dependency is exactly ONE of three bands, which ' +
    'decides the central `pyproject.toml` marker AND the install/reflect path — (a) pure-Python wheel -> installs to `.venv`; (b) ' +
    'scientific/native (numpy/scipy-class, C/C++/Fortran build) -> NOT in `.venv`, the `forge-scientific-*` env, decided via `uv run python -m ' +
    'tools.assay provision check`; (c) companion-band -> pinned with a `; python_version<\'3.15\'` marker. Pin the version + band marker in the ' +
    'ONE central repo-root `pyproject.toml` (a SHARED file — the reconcile pass owns it; you MUST NOT edit it from a folder agent; LOG it as a ' +
    'residual_ripple keyed on `pyproject.toml`), add the package to the correct group in the target `README.md` (folder-local), and author the ' +
    'target `.api/<package>.md` from `uv run python -m tools.assay api` (folder-local). Never a per-folder `pyproject.toml`.',
  'CLOSEOUT (the closeout pass ONLY): a genuinely-complete card moves to its file `[02]-[CLOSED]` section as a collapsed one-liner ' +
    '`[ID]-[COMPLETE]: <one-line disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`); update the target `ARCHITECTURE.md` ' +
    '`[02]-[SEAMS]` section ONLY when a real cross-folder seam landed. Realize/critique/redteam passes NEVER change card status.',
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
    'line-count. NEW WORK SURFACED — api gaps and tasks the implementation exposes are realized or recorded the same turn.',
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
    'returns the rail `Error`). Two-to-four wrappers that always co-occur collapse into ONE parameterized aspect factory. Code reads as STACKED ' +
    'DECORATORS over a thin pure core, never inline-repeated concerns or sibling helper functions; the domain transform itself stays a pure ' +
    'function/fold.',
  'DERIVATION + ARITY: cases sharing generative structure are DERIVED — one primary `frozendict` correspondence declared, every secondary map ' +
    'derived from it (DERIVED_LOGIC), or a fold/comprehension — never enumerated arms. Configuration enters as ONE behavior-carrying value ' +
    '(vocabulary member, tagged variant, frozen policy table), never flag sets the body re-derives (POLICY_VALUES). ONE entrypoint owns every ' +
    'modality (singular/plural/batch/stream), discriminating on the INPUT SHAPE (`T | Iterable[T]` normalized once at the head), never a name ' +
    'suffix or a `mode`/`batch` knob (MODAL_ARITY); a `timeout`/`retry`/`deadline` is an aspect or an `anyio` scope, never a signature param ' +
    '(KNOB_TEST).',
  'RAILS (rails-and-effects): the narrowest carrier that states the outcome, chosen ONCE at admission — `Option[T]` non-failing absence, ' +
    '`Result[T, E]` typed fallibility, `effect.result` do-notation for sequential `bind`, `Block`/`Map` immutable traversal, an `anyio` task group ' +
    'as the failure boundary (NEVER `asyncio.gather`), `stamina.retry` as the decorator (never a sleep-loop). The fault type `E` is a CLOSED ' +
    'vocabulary — `Literal` set, `StrEnum`, or `@tagged_union` family — NEVER a bare `str` for a multi-cause domain. Accumulate-vs-abort is a ' +
    'correctness decision fixed at the boundary: `map2`/accumulating-fold for independent operands (a `bind` chain over independents reports only ' +
    'the first failure), `bind` short-circuit for dependent steps. Cancellation is not failure; resource cleanup is `AsyncExitStack` + a shielded ' +
    'scope.',
  'STACK .api CAPABILITY: ENUMERATE BOTH catalog tiers IN FULL with a real listing (`ls`/`fd` of the shared `libs/python/.api/*.md` AND the ' +
    'folder-specific `<folder>/.api/*.md`, from disk, never memory), then mine every catalog the card touches to OPERATOR DEPTH and compose the ' +
    'capability into single dense operations woven as ONE rail, layering the shared rails (expression `Result`/`Option`, msgspec/pydantic ' +
    'discriminated models, beartype validation, stamina retry, structlog + opentelemetry spans, anyio structured concurrency) ON TOP OF the ' +
    'folder-specific domain packages — e.g. `msgspec dec_hook` -> pydantic discriminated union -> stamina `retry_context` -> opentelemetry span ' +
    'around the domain op — NOT flat one-shot per-library uses. Use the DEEPEST primitive each package itself reaches (LIBRARY_DEPTH). An ' +
    'admitted capability the card charter needs that no owner exploits is a DEFECT the pass closes by deepening a fence; a cited member that ' +
    'cannot be verified in the catalogs or via `uv run python -m tools.assay api` is a PHANTOM the pass deletes or corrects on sight. (Implement ' +
    'composes the capability the card needs; it does not max-stack every catalog for its own sake — that is rebuild.)',
  'PRESERVE all capability (densify, never delete functionality). Where a fence is already dense, deepen; where it is flat/naive, rebuild ' +
    'ground-up. Never regress correctness or boundary law.',
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
  'the edge; do not trample a sibling owner while densifying; respect the dependency direction of the workspace strata. Each target realizes ONLY ' +
  'its OWN cards into its OWN pages; a concern owned twice across a runtime, a folder mixing unrelated concerns, or coupling to a sibling owner ' +
  'INTERIOR (vs its wire/seam) is a defect.'
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
const DOCTRINE = [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const folderName = (p) => p.split('/').filter(Boolean).pop() || p
const pkgPath = (toPkg) => { const k = String(toPkg || '').trim().replace(/^python:/, ''); return k.indexOf('libs/') === 0 ? k : ROOT + '/' + k }
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
    'each card full body; every design page the card names under `' + folder + '/.planning/**`; the sibling pages it seams to; the package-root ' +
    '`ARCHITECTURE.md` + `README.md`; the operative docs/stacks/python/ pages (docs/stacks/csharp/ as the ambition floor); BOTH .api tiers — the ' +
    'shared `' + SHARED_API + '/*.md` AND the folder `' + folder + '/.api/*.md` (stack them, the shared rails layered onto the folder packages); ' +
    'and verify any novel member via `uv run python -m tools.assay api`. Realize EVERY card in `order` (all tasks incl. Atomic, then the ideas) ' +
    'into deep fences IN `' + folder + '` PAGES ONLY, in LIFECYCLE order (admit raw ONCE into a typed `TypedDict`/Pydantic payload -> materialize ' +
    'into the canonical owner the OWNER_CHOOSER discriminants select -> weave every cross-cutting concern ' +
    '(retry/telemetry/validation/contracts/memo/receipts) as a STACKED signature+rail-preserving aspect over a thin pure core -> compose the ' +
    'domain transform through ONE unified `Result`/`Option` rail with total `match` -> project + `msgspec.Struct` egress, BOTH ingress and egress ' +
    'parameterized). Collapse parallel shapes into one closed family/ADT (`@tagged_union`); drive cases with a derived `frozendict` table or fold; ' +
    'one polymorphic entrypoint per modality (`T | Iterable[T]` normalized once). py3.15-modern only (PEP 585/604/695, `frozendict` builtin, ' +
    'newest payload forms; NO `from __future__ import annotations`, NO legacy typing, NO `asyncio`/`None`-as-failure). Resolve any [BLOCKED] card ' +
    'inline (assay api for members; `uv run python -m tools.assay provision check` for native/scientific/provisioning bands). PACKAGE ADMISSION ' +
    '(only if a card needs a not-yet-admitted package): decide the band (pure-Python / scientific-native / companion), do the FOLDER-LOCAL parts ' +
    'NOW — add the package to the correct group in `' + folder + '/README.md` and author `' + folder + '/.api/<pkg>.md` from `assay api` — and LOG ' +
    'the central `pyproject.toml` pin + band marker as a residual_ripple with files including `pyproject.toml` (a single reconcile agent owns that ' +
    'shared file; you MUST NOT edit it). RIPPLES: realize ONLY `' + folder + '`\'s OWN half of every seam; NEVER edit another folder page. For ' +
    'each ripple your cards carry, log a residual_ripple {files:[your_page, counterpart_page], pkg, slug, mirror_slug, claim} stating the contract ' +
    'your half exposes (cross-language/lib-wide legs are deferred to the other-language workflow). Do NOT close any card — the closeout pass owns ' +
    'card status. High-signal prose all-backticked, comment hygiene, fix-in-place (read-then-extend, preserve capability). Return verdict + ' +
    'realized slugs + deferred (any card you could not realize, with reason) + collapsed (before->after counts) + residual_ripples + summary.'].join('\n')
const critiquePrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: DOCTRINAL-CONFORMANCE AUDIT + CHARTER-COMPLETENESS + FIX IN PLACE across `' + folder + '`. You are an ULTRA-HARSH, UNAGREEABLE auditor: ' +
    'assume a violation exists in every fence until you prove otherwise, and "good enough" is rejected. The cards realized this turn (read each ' +
    'FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`):\n' + seq + '\nREAD the realized pages under `' + folder + '/.planning/**`, ' +
    'the sibling pages, the operative docs/stacks/python/ pages, and BOTH .api tiers (shared `' + SHARED_API + '` + folder `' + folder + '/.api`). ' +
    'Run these MECHANICAL checklists line-by-line and REPAIR every hit in place (a fix, never a ledger note); the checklists are a FLOOR you ' +
    'hunt past, never the complete audit:',
  '(1) COLLAPSE_SCAN — apply the move for any of the 12 signals (3+ instances makes it mandatory): sibling prefix/suffix names -> one ' +
    'modality-polymorphic entrypoint; same return rail differing only by arity -> input-shape discrimination; a `get`/`get_many`/`get_by_id` ' +
    'family -> one input-keyed entrypoint; functions differing only by a literal -> parameterize the literal as policy; a bool parameter selecting ' +
    'two bodies -> one derived body or policy value; a function calling exactly one other -> delete the hop; a class exposing one public method -> ' +
    'module function or fold-on-owner; parallel dispatch arms repeating structure -> a `frozendict` table or fold algebra; several types sharing ' +
    'fields for one concept -> one closed family; 3+ sibling module constants for one concept -> one `frozendict`/`StrEnum`; a wrapper renaming a ' +
    'package API -> use the package surface directly; the same 2-4 wrappers recurring -> one parameterized aspect factory. The 12 signals are a ' +
    'FLOOR — hunt collapse targets past them.',
  '(2) OWNER_CHOOSER — for EVERY shape re-derive the owner from the 5 discriminants (admission, identity regime, variant arity, payload timing, ' +
    'openness); if it is not the discriminant-correct owner (`TypedDict`/Pydantic/`msgspec.Struct`/frozen dataclass/rich ' +
    'class/`StrEnum`/`Literal`/`sentinel`/`Option`/`Result`/`frozendict`/`Map`/`tuple`/`Protocol`), replace it. Kill every parallel DTO, one-field ' +
    'wrapper, field-rename class, tag-only shape, and `None`-as-failure.',
  '(3) KNOB_TEST — delete each parameter: if the value already encodes what it carried, it was a knob — collapse a `strict: bool`/`mode`/`batch` ' +
    'flag into a policy value or input-shape discriminant, and move every `timeout`/`retry`/`deadline` out of the signature into an aspect or ' +
    '`anyio` scope.',
  '(4) ASPECTS — every cross-cutting concern (retry/telemetry/validation/contracts/memo/registration/receipts) MUST be a signature+rail-preserving ' +
    'STACKED decorator that never raises into domain flow; 2-4 co-occurring wrappers collapse into one aspect factory; deterministic stacking ' +
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
  'Also enforce both-tier `.api` use (a thin folder-only subset ignoring the shared rails the card needs is a defect), cross-folder convention ' +
    'consistency, and prose + comment hygiene. EDIT the `' + folder + '` pages to fix every hit; realize ONLY `' + folder + '` pages and OVERRIDE ' +
    'any earlier residual you can now resolve; log any genuine cross-FOLDER item as a residual_ripple {files, pkg, slug, mirror_slug, claim}. ' +
    'Return verdict + realized + deferred + collapsed + residual_ripples + summary.'].join('\n')
const redteamPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE across `' + folder + '`. You are the LAST and MOST AGGRESSIVE pass: assume the author and ' +
    'critique missed things and that the chosen design is not the strongest until proven, with the burden of proof ON THE DESIGN. The cards ' +
    'realized this turn (read each FULL body):\n' + seq + '\nOpen BOTH .api tiers (shared `' + SHARED_API + '` + folder `' + folder + '/.api`), ' +
    'the sibling pages, and the operative docs/stacks/python/ pages. Attack from every direction and REPAIR every defect in place — no ' +
    'soft-pedalling, no could/should, a fix never a ledger.',
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
    'names it (ports/boundaries/wires/seams drift) is a defect: fix it within `' + folder + '`, or record it as a residual_ripple. (E) ' +
    'SURFACE-SPRAWL-IN-TIME — an admitted package whose `.api` exposes capability the card needs but the fence re-derives by hand, flat code below ' +
    'the operator depth the packages reach, a phantom `.api` member, or a thin wrapper: collapse to package depth and verify the member exists ' +
    '(via `assay api`).',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (every time, NOT only on a structural restructure): re-attack every conformance dimension with fresh ' +
    'hostile eyes, trusting nothing the prior passes claimed — the COLLAPSE_SCAN signals, OWNER_CHOOSER per shape, the KNOB_TEST per param, the ' +
    'ASPECT taxonomy, rail + closed-fault-vocabulary discipline, charter-completeness per card, BOTH naivety axes (COVERAGE thin-slice; ' +
    'APPROACH roster-where-a-generator-should-own-the-space), payload/`frozendict`/PEP conformance, both-tier ' +
    '`.api` use, py3.15-modern typing, and prose/comment hygiene — and fix every defect. Even absent a structural rebuild, the fences must end ' +
    'objectively denser, more correct, and more powerful than the critique left them; if the strongest form is genuinely already present, prove it ' +
    'by finding nothing — never invent churn. Realize ONLY `' + folder + '` pages; log cross-FOLDER items as residual_ripples. Return verdict + ' +
    'realized + deferred + collapsed + residual_ripples + summary.'].join('\n')
const reconcileFixPrompt = (cl) => [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '',
  'TASK: RECONCILE this cluster of cross-FOLDER residuals the per-folder passes deferred. There is NO severity — treat EVERY residual as ' +
    'must-address. Read EVERY listed file. Handle each residual by KIND: (a) IN-SCOPE SEAM (both halves already realized by their own target ' +
    'folders) — read both pages, ALIGN them to ONE shared contract, fix any mismatch, set `seam_landed` true; (b) OUT-OF-SCOPE SAME-LANGUAGE ' +
    'COUNTERPART (the counterpart card lives in a non-target libs/python folder) — realize that ONE counterpart card fence (its half only, NEVER ' +
    'the folder\'s other cards) at the same bar and align the seam; (c) CENTRAL PIN — apply every `pyproject.toml` version pin + band marker in this ' +
    'cluster (you are the ONLY agent that edits that shared file; apply them all serially, keeping the existing group/marker order) and list them ' +
    'in `admitted`; (d) CROSS-LANGUAGE / LIB-WIDE LEG — record it in `deferred_legs` and do NOT realize it (its counterpart is the other-language ' +
    'workflow concern). Preserve all capability, regress no file, never trample a sibling owner interior. For every ripple counterpart you touch, ' +
    'emit a `pairs` row {pkg, slug, mirror_slug, seam_landed}. If a residual is FACTUALLY INCORRECT or not a real defect, leave it and say why in ' +
    'the summary — never silently skip a real one. ' +
  'A concurrent sibling may share a page with your cluster (oversized components shard file-atomically): edit any potentially shared page with ' +
  'surgical anchored Edits only — re-read and re-apply on an edit conflict, never a whole-file rewrite. ' +
  'Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerifyPrompt = (cl, fix) => [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '',
  'TASK: ADVERSARIAL WRITING VERIFY over this reconcile cluster — never a friendly confirmation: hold every claimed fix loose, weak, or token ' +
    'until it survives your attack, one verdict per claim. Per claim: (1) RE-DERIVE necessity from the named files on disk — was the defect real, ' +
    'was the fix required at all; (2) PROVE on disk the fix was done properly (seam aligned on BOTH pages to ONE contract, counterpart fence ' +
    'realized at the doctrine bar, central `pyproject.toml` pin + band marker actually present); (3) REPAIR every loose, weak, or token fix IN ' +
    'PLACE via Edit/Write to the objectively best root-level form of the SAME files — a single-point patch where a root-level dense ' +
    'reconstruction of those files is available is itself a defect you repair NOW; confine repairs to the files the claims name, and list every ' +
    'file you touch in `repaired_files`; (4) only THEN classify: "fixed" (real defect genuinely resolved — by the fixer or by your own repair, ' +
    'cite the on-disk evidence), "invalid" (the claim is factually wrong / not a real defect — cite why), "open" (real defect whose resolution is ' +
    'genuinely unreachable from the files at hand — a cross-language or out-of-run leg — NEVER a strengthenable fix you punted). Mark "invalid" ' +
    'ONLY when you can show the claim is wrong. Set `overall` true only when zero claims remain "open". Claims:\n' + JSON.stringify(cl, null, 1) +
    '\nFiles the fixer touched: ' + JSON.stringify(fix.files) + '\nPairs the fixer reported: ' + JSON.stringify(fix.pairs || [])].join('\n')
const closeoutPrompt = (folder, seamJson) => [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '',
  'TASK: TRUTHFUL CLOSEOUT + FINAL REMEDIATION of `' + folder + '`. This is the SOLE owner of card status. For EVERY card that was in scope this ' +
    'run, read its FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md` and the realized fences under `' + folder + '/.planning/**`, ' +
    'then SANITY-VERIFY the fences genuinely fulfill the card `Capability`/`Shape`/`Unlocks` against the cited `.api` (verify novel members via ' +
    '`uv run python -m tools.assay api`). If a card is WEAK or PARTIAL, make a FINAL in-place REMEDIATION NOW (it already passed ' +
    'implement->critique->redteam this turn; deepen the fences under `' + folder + '` to genuinely complete the charter — a token single-point ' +
    'patch where a root-level deepening of the same fences is available is itself a defect, repair to the root form), then re-verify. Assign ' +
    'each card a strength: `strong` (every charter clause delivered, fences transcription-complete against the verified `.api`), `partial` (most ' +
    'delivered, a clause still thin), `weak` (charter not met). CLOSE only genuinely-complete cards: move them to the `[02]-[CLOSED]` section of ' +
    'their owning file (`' + folder + '/IDEAS.md` or `' + folder + '/TASKLOG.md`) as a collapsed one-liner `[ID]-[COMPLETE]: <disposition>; ' +
    'Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`), and update `' + folder + '/ARCHITECTURE.md` `[02]-[SEAMS]` ONLY when a real cross-folder ' +
    'seam landed. RIPPLE PAIRS: a card carrying a `Ripple:` closes COMPLETE only if its seam landed — this map gives seam_landed per slug (`false` ' +
    '= seam did NOT land, keep the card OPEN; `true`/absent = judge on your own half): ' + seamJson + '. Honestly RE-OPEN any card you cannot ' +
    'bring to `strong` this run, with a one-line reason (a real out-of-run or cross-language dependency). The orchestrator will DEMOTE any card ' +
    'you mark complete whose strength is not `strong`, so never inflate. Return closed [{slug, disposition, strength}] + reopened [{slug, reason}] ' +
    '+ summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
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
const disc = (await agent(discoverPrompt(TARGETS), { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, effort: 'medium', stallMs: 300000 })) || { targets: [], malformed_ripples: [] }
const withCards = (disc.targets || []).filter((t) => (t.tasks && t.tasks.length) || (t.ideas && t.ideas.length))
log('Discover: ' + TARGETS.length + ' targets; ' + withCards.length + ' with open cards; pooling at CAP=' + CAP)

// --- [REALIZE]
phase('Realize')
const realizeFolder = async (t) => {
  const seq = JSON.stringify({ order: t.order, tasks: t.tasks, ideas: t.ideas, ripples: t.ripples, gates: t.gates || [] }, null, 1)
  const tag = folderName(t.folder)
  const impl = await agent(implementPrompt(t.folder, seq), { label: 'implement:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  if (!impl) return { folder: t.folder, failed: true, logs: [] }
  const crit = await agent(critiquePrompt(t.folder, seq), { label: 'critique:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  const red = await agent(redteamPrompt(t.folder, seq), { label: 'redteam:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  return { folder: t.folder, failed: false, logs: [impl, crit, red].filter(Boolean) }
}
const realized = (await pool(withCards, CAP, realizeFolder)).filter(Boolean)
const failedFolders = new Set(realized.filter((r) => r.failed).map((r) => r.folder))
const deferredCards = realized.flatMap((r) => r.logs.flatMap((l) => (l.deferred || []).map((d) => ({ folder: r.folder, slug: d.slug, reason: d.reason }))))

const crossLang = []
for (const t of (disc.targets || [])) for (const rp of (t.ripples || [])) if (rp.klass === 'cross_lang') crossLang.push(t.folder + ' [' + rp.from_slug + '] ' +
  '-> ' + rp.to_pkg + ' [' + rp.to_slug + ']')
const allRes = []
for (const r of realized) if (!r.failed) for (const l of r.logs) for (const x of (l.residual_ripples || [])) allRes.push({ files: (x.files && x.files.length ? x.files : [r.folder]), pkg: x.pkg, slug: x.slug, mirror_slug: x.mirror_slug, claim: x.claim || '' })
for (const t of (disc.targets || [])) for (const rp of (t.ripples || [])) if (rp.klass !== 'cross_lang') allRes.push({ files: [t.folder, pkgPath(rp.to_pkg)], pkg: rp.to_pkg, slug: rp.from_slug, mirror_slug: rp.to_slug, claim: 'ripple ' +
  '' + rp.klass + ': ' + folderName(t.folder) + ' [' + rp.from_slug + '] <-> ' + rp.to_pkg + ' [' + rp.to_slug + ']' })
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
// Heaviest cluster first: a fixer's load is dominated by distinct files read + reconciled; under CAP the long pole must never launch last.
// Atomicity is BUDGETED at the fair share (totalWork/CAP): an over-budget component sub-shards by lead file (same-lead-file rows never split —
// the edit-collision floor); verify owns the deliberate cross-shard seams.
const clusterWork = (c) => { const files = new Set(); for (const r of c) for (const f of r.files) files.add(f); return files.size * 2 + c.length }
const shardOversized = (cs) => {
  const cap = Math.max(1, Math.ceil(cs.reduce((w, c) => w + clusterWork(c), 0) / CAP))
  return cs.flatMap((c) => {
    if (clusterWork(c) <= cap) return [c]
    const byFile = new Map()
    for (const r of c) { const k = r.files[0] || '~'; if (!byFile.has(k)) byFile.set(k, []); byFile.get(k).push(r) }
    const shards = []
    for (const g of [...byFile.values()].sort((a, b) => clusterWork(b) - clusterWork(a))) {
      const t = shards.find((s) => clusterWork(s.concat(g)) <= cap)
      if (t) t.push(...g); else shards.push([...g])
    }
    return shards
  })
}
const sharded = shardOversized(clusters); clusters.length = 0; clusters.push(...sharded)
clusters.sort((a, b) => clusterWork(b) - clusterWork(a) || (a[0].claim || '').localeCompare(b[0].claim || ''))
log('Realize: ' + realized.filter((r) => !r.failed).length + '/' + withCards.length + ' folders; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' ' +
  'clusters; work [' + clusters.map(clusterWork).join(', ') + '] (2*files+claims)' + (failedFolders.size ? '; ' + failedFolders.size + ' folder(s) failed' : ''))
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent(reconcileFixPrompt(cl), { label: 'reconcile-fix:' + i, phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: 300000 })
    if (!fix) return null
    const verify = await agent(reconcileVerifyPrompt(cl, fix), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const seamLanded = {}
for (const r of reconciled) for (const p of ((r.fix && r.fix.pairs) || [])) { seamLanded[p.slug] = p.seam_landed; seamLanded[p.mirror_slug] = p.seam_landed }
const oosFolders = new Set()
for (const r of reconciled) for (const p of ((r.fix && r.fix.pairs) || [])) { const fp = pkgPath(p.pkg); if (!TARGET_SET.has(fp) && fp.indexOf(ROOT + '/') === 0) oosFolders.add(fp) }
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_open = claimsAll.filter((c) => c.status === 'open').map((c) => c.claim)
const dropped_invalid = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
const admitted = [...new Set(reconciled.flatMap((r) => (r.fix && r.fix.admitted) || []))]
const deferred_legs = [...new Set([...crossLang, ...reconciled.flatMap((r) => (r.fix && r.fix.deferred_legs) || [])])]

// --- [CLOSEOUT]
phase('Closeout')
const closeoutFolders = [...withCards.map((t) => t.folder).filter((f) => !failedFolders.has(f)), ...oosFolders]
const seamJson = JSON.stringify(seamLanded, null, 1)
const closeouts = (await pool(closeoutFolders, CAP, (f) => agent(closeoutPrompt(f, seamJson), { label: 'closeout:' + folderName(f), phase: 'Closeout', schema: CLOSEOUT_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)
let closed_count = 0
const reopened = []
for (const c of closeouts) {
  for (const cc of (c.closed || [])) {
    if (cc.disposition === 'complete' && cc.strength !== 'strong') reopened.push({ folder: c.folder, slug: cc.slug, reason: 'demoted: strength=' + cc.strength })
    else closed_count++
  }
  for (const ro of (c.reopened || [])) reopened.push({ folder: c.folder, slug: ro.slug, reason: ro.reason })
}
log('Closeout: ' + closed_count + ' cards closed, ' + reopened.length + ' re-open/demoted across ' + closeoutFolders.length + ' folders')

return {
  root: ROOT,
  targets: TARGETS,
  realized_folders: realized.filter((r) => !r.failed).map((r) => r.folder),
  realize_failed: [...failedFolders],
  deferred: deferredCards,
  reconciled_clusters: clusters.length,
  seams_landed: Object.keys(seamLanded).filter((k) => seamLanded[k]).length,
  oos_counterparts_realized: [...oosFolders],
  admitted,
  closed_count,
  reopened,
  hard_open,
  dropped_invalid,
  cross_language_legs: deferred_legs,
  malformed_ripples: disc.malformed_ripples || [],
}
