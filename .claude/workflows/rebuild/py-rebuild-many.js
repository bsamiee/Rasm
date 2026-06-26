export const meta = {
  name: 'py-rebuild-many',
  whenToUse: 'Hostile ground-up rebuild of many Python design-page folders to the py3.15 doctrine bar.',
  description: 'Per-FOLDER (not per-file) hostile ground-up rebuild of the libs/python design pages to the docs/stacks/python bar AND integration of every newly-admitted .api package into the design corpus. Marries plan-py quality/focus (py3.15 to the metal, ADT collapse via @tagged_union/frozen-dataclass/msgspec/pydantic owners, expression Result/Option rails, definition-time aspects/AOP, full both-tier .api stacking, justified in-place capability extension, the harsh adversarial stance) with implement-py folder dispersal: ONE agent per folder across any number of the Python planning folders, each folder run through a 3-step ADVERSARIAL cycle implement(max) -> critique(xhigh) -> redteam(max), every stage hostile (assume naive/illusory until it survives attack). Each agent integrates the folder newly-filled .api capabilities — the folder-specific catalogs AND the shared branch-substrate catalogs at libs/python/.api — into the EXISTING owning design page where one fits (grow the owner in place per the doctrine growth law), or authors a NEW design page (and sub-domain) where the concept is a genuinely new owner with no existing home, justified and built ground-up per docs/stacks/python and the .planning page grammar, updating ARCHITECTURE + README. Then a cross-folder reconcile, and a FINAL whole-stack 1-2-3 alignment pass (align -> critique -> redteam by one series of agents over all folders at once) that aligns every seam/wire/port/boundary, kills duplication and layering violations, and ensures every folder fully leverages the shared branch-substrate tier. args = optional folder path or array; empty = all five Python planning folders (artifacts, compute, data, geometry, runtime).',
  phases: [
    { title: 'Realize', detail: 'per folder (1 agent/folder, pooled): implement(max) -> critique(xhigh) -> redteam(max), every stage adversarial; integrate each new .api (folder + branch-substrate) into the existing owner or a new justified page; collapse + capability-extend the whole folder corpus, own-folder-only, cross-folder seams logged as residuals' },
    { title: 'Reconcile', detail: 'consume cross-folder residuals: union-find cluster by shared file -> fix(max) -> adversarial verify(xhigh); hard residuals reported for resolve-residuals' },
    { title: 'Final-Align', detail: 'one series of agents over ALL folders at once (align(max) -> critique(xhigh) -> redteam(max)): align every cross-folder seam/wire/port/boundary, kill duplication + layering violations, ensure every folder fully leverages the shared branch-substrate tier, catch gaps no single-folder pass could see' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const STAGGER_MS = 1500
const ALL_FOLDERS = [
  { name: 'artifacts', planning: 'libs/python/artifacts/.planning', api: 'libs/python/artifacts/.api', root: 'libs/python/artifacts', note: '' },
  { name: 'compute', planning: 'libs/python/compute/.planning', api: 'libs/python/compute/.api', root: 'libs/python/compute', note: '' },
  { name: 'data', planning: 'libs/python/data/.planning', api: 'libs/python/data/.api', root: 'libs/python/data', note: '' },
  { name: 'geometry', planning: 'libs/python/geometry/.planning', api: 'libs/python/geometry/.api', root: 'libs/python/geometry', note: '' },
  { name: 'runtime', planning: 'libs/python/runtime/.planning', api: 'libs/python/runtime/.api', root: 'libs/python/runtime', note: '' },
]

// --- [INPUTS] ----------------------------------------------------------------------------
const norm = (t) => { const s = String(t).trim(); return s.replace(/^libs\/python\//, '').replace(/\/$/, '') }
const wanted = Array.isArray(args) ? args.filter(Boolean).map(norm)
  : (typeof args === 'string' && args.trim() && args.trim().toUpperCase() !== 'ALL') ? [norm(args)]
  : null
const FOLDERS = wanted ? ALL_FOLDERS.filter((f) => wanted.some((w) => w === f.name)) : ALL_FOLDERS
const ROSTER = FOLDERS.map((f) => f.name + ' (' + f.root + ')').join('; ')

// --- [MODELS] ----------------------------------------------------------------------------
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, integrated: { type: 'array', items: { type: 'string' } }, newPages: { type: 'array', items: { type: 'string' } }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
// --- [FINAL-ALIGN] -- whole-stack cross-folder coherence prompts (one series of agents) -------
const ALIGN_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['aligned', 'clean'] }, aligned: { type: 'array', items: { type: 'string' } }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo, libs/python planning corpus (markdown specs of intended Python module designs). CLAUDE.md manifest law governs (respect the ' +
    'workspace strata and the dependency direction of the libs/python branch; a runtime-neutral owner only where a non-host consumer needs the ' +
    'contract). This pass works ONE FOLDER end-to-end: the whole folder design corpus under its `.planning/**` plus BOTH capability tiers — the ' +
    'folder-specific `.api/api-*.md` AND the shared branch-substrate catalogs at `libs/python/.api/*.md` (substrate packages catalogued ONCE at ' +
    'the branch and consumed per folder) — NOT a single page. All five Python folders use the STANDARD layout: design pages under ' +
    '`libs/python/<folder>/.planning/**`, capability catalogs under `libs/python/<folder>/.api/`, and the governing ' +
    '`ARCHITECTURE.md`/`README.md`/`IDEAS.md`/`TASKLOG.md` at the `libs/python/<folder>/` root.',
  'MANDATORY STANDARDS — docs/stacks/python/ is the FLOOR, not the ceiling: every fence MUST meet docs/stacks/python/ (README, language, shapes, ' +
    'surfaces-and-dispatch, rails-and-effects, concurrency, boundaries, algorithms, system-apis, runtime), then PUSH PAST it to the objectively ' +
    'strongest form the doctrine admits. READ the doctrine pages and conform exactly — a hard gate enforced by the Python lint/type/contract gate ' +
    '(`Ruff` lint, `ty`/`mypy` types, `beartype` runtime contracts, the `tools/py_analyzer` compiled-doctrine gate; a true positive is ' +
    'architecture pressure, fix the shape; a false positive is rule pressure, never a blanket ignore). Cite only members confirmed via `uv run ' +
    '--frozen python -m tools.assay api` (a member you cannot verify is a phantom — delete it). docs/stacks/csharp/ is the density/ambition FLOOR ' +
    '— match its richness, never import C#-shaped idioms.',
  'TWO JOBS, ONE PASS: (1) INTEGRATE every newly-admitted package — each folder `.api/api-*.md` catalog that a recent survey added and a focused ' +
    'rebuild-api filled, AND any newly-filled branch-substrate catalog at `libs/python/.api/` — into the folder design corpus so its capability is ' +
    'COMPOSED, not merely catalogued; (2) HOSTILE GROUND-UP REBUILD of the folder pages to the ULTRA bar (collapse surfaces/types, deepen ' +
    'bleeding-edge spellings, maximize both-tier admitted-library capability, close concept capability gaps). Both are untied to any card: improve ' +
    'the corpus objectively wherever the doctrine admits a stronger form.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FOLDER ' +
    'items (report those in residual_high).',
].join('\n')
const INTEGRATE = [
  'NEW-CAPABILITY INTEGRATION (the distinguishing mandate of this pass): for EVERY package newly admitted to this folder (read its filled ' +
    '`.api/api-*.md` catalog to full depth + verify members via `uv run --frozen python -m tools.assay api`), the capability MUST land in the ' +
    'design corpus as COMPOSED design, never a dangling catalog. PLACEMENT, most-specific wins: (a) GROW AN EXISTING OWNER — when an existing ' +
    'design page already owns the concept the package serves, weave the package capability INTO that owner in place (a case in the closed ' +
    '`@tagged_union`/`Literal`/`StrEnum` family, a row or richer data on the `frozendict` table, a field or composed value object on the existing ' +
    '`msgspec.Struct`/Pydantic model/frozen dataclass/`TypedDict`, an operation on the existing surface, a policy value), reshaped per the ' +
    'doctrine growth law + COMPOSED_IMPLEMENTATION as if it had always carried it — NEVER a parallel surface beside the owner; (b) AUTHOR A NEW ' +
    'PAGE — ONLY when the package introduces a genuinely NEW owner/concern that no existing page owns, author a new design page (and a new ' +
    '`.planning/<sub-domain>/` folder when the concern warrants its own sub-tree) at one-page-per-eventual-source-file granularity, built ' +
    'GROUND-UP to the full .planning page grammar (H1, lead paragraph, numbered cluster sections, card + transcription-complete signature fences) ' +
    'and the docs/stacks/python bar, then UPDATE the folder `ARCHITECTURE.md` codemap + `[02]-[SEAMS]` and the `README.md` router/package roster ' +
    'so the structure stays truthful.',
  'NEW-FILE DISCIPLINE: a new page is justified ONLY by a genuinely new domain owner — NEVER to dodge a collapse, reduce a page size, or split one ' +
    'concept across files. The collapse law still rules WITHIN every page: 3+ parallel shapes collapse into ONE owner in the SAME page. Before ' +
    'authoring a new page, prove no existing page is the rightful owner; a capability that belongs to an existing owner is grown there, not spun ' +
    'out. A new page that fragments an existing concept is a defect; a new page for a real new owner the folder was missing is correct.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (implement, critique, AND red-team) is HOSTILE: assume the existing fences are NAIVE, SHALLOW, JUNIOR, or ' +
    'ILLUSORY until they survive an aggressive attack; the burden of proof is ON THE CODE, never on you. "Mature", "already strong", "good ' +
    'enough", "done", "polished", and a prior `clean` verdict are REJECTED self-assessments — MOST of this corpus is naive, surface-level, ' +
    'old-style Python dressed in the right vocabulary, and it is NOT tolerable. Default to "this fence is naive and must be rebuilt to the ' +
    'strongest form the doctrine admits" and MAKE that rebuild; a no-edit verdict is reached ONLY after a genuinely aggressive attack on the real ' +
    'domain + the verified both-tier package surface finds nothing — never a first-read concession, never to avoid work. Reject "good enough" ' +
    'categorically.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code PRETENDS to be advanced: it uses the doctrine vocabulary ' +
    '(`@tagged_union`/`frozendict`/`Result`/`Option`/the rails), cites packages, reads dense and confident — yet is HOLLOW. Treat dense, ' +
    'confident-looking fences with MORE suspicion, and DISBELIEVE every claim the page makes about itself until you verify it against the real ' +
    'domain and the catalogued both-tier package surface. HUNT: a name/signature/prose PROMISING capability the body does not implement; a "rich" ' +
    'owner that is a thin slice of its concept (a 2-variant `@tagged_union` for a 20-case domain; the obvious 3 keys where the concept carries ' +
    'fifteen); decorative density and ceremony carrying no real capability; a placeholder/stub/sketch dressed as a finished design; a ' +
    'structurally-correct collapse that is semantically empty; a newly-admitted `.api` cited in prose but never actually composed into a fence; a ' +
    'phantom `.api` member cited but unverifiable. Every illusion is a DEFECT to rebuild, never a feature to preserve; never invent churn to look ' +
    'busy either.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — MANDATORY FULL READ BEFORE ANY EDIT: docs/stacks/python/ is the route-owned law, and you hold ONE FOLDER corpus with ample ' +
    'context budget, so there is NO excuse for a partial read. STEP 0 — LIST THE DIRECTORIES FIRST: enumerate `docs/stacks/python/`, ' +
    '`docs/stacks/python/.api/`, the folder `.api/`, AND the branch-substrate `libs/python/.api/` with a real `ls`/`find` to obtain the COMPLETE ' +
    'doctrine + both-tier catalog inventory before reading, so not one page is silently skipped and the routing order is taken from the source of ' +
    'truth, never from memory. THEN READ EVERY doctrine page IN FULL — top-to-bottom, every section, every family card, every snippet, never a ' +
    'partial, skim, grep-jump, or section-sample — STARTING AT THE README and proceeding in its `[01]-[ATLAS]` routing order: (1) ' +
    '`docs/stacks/python/README.md` (the 16 laws in 5 groups + the 12-signal COLLAPSE_SCAN + RULE_ENFORCEMENT + PAGE_CRAFT + CORPUS_LAW), then ' +
    'strictly in routing order (2) `docs/stacks/python/language.md`, (3) `docs/stacks/python/shapes.md` (the lifecycle + OWNER_CHOOSER + the ' +
    'closed-family/absence/variant law), (4) `docs/stacks/python/surfaces-and-dispatch.md` (dispatch forms + ASPECTS), (5) ' +
    '`docs/stacks/python/rails-and-effects.md` (rail/effect/fault/receipt law), (6) `docs/stacks/python/concurrency.md` (the anyio ' +
    'structured-concurrency rail), (7) `docs/stacks/python/boundaries.md` (host/wire boundary law), (8) `docs/stacks/python/algorithms.md` ' +
    '(numeric approach), (9) `docs/stacks/python/system-apis.md` (stdlib-replacement law), (10) `docs/stacks/python/runtime.md` (interpreter ' +
    'execution + isolation). The README `[STATE]` column marks each page finalized (binding law) or partial (operative-but-unfinalized context); ' +
    'read EVERY page regardless of state and hold every fence to them as fact — a partial, sampled, or skipped read of this doctrine is itself a ' +
    'process defect, not an efficiency. docs/stacks/csharp/ is the density/ambition FLOOR — match its richness, never import C#-shaped idioms.',
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
  'STACK .api CAPABILITY (load-bearing): FIRST inventory the COMPLETE catalog set available to this folder — BOTH the shared branch-substrate ' +
    'catalogs at `libs/python/.api/*.md` (anyio, expression, msgspec, pydantic, pydantic-settings, beartype, structlog, stamina, numpy, psutil, ' +
    'opentelemetry-*, protobuf, grpcio, numcodecs, zlib-ng, trio) AND every folder-specific catalog at `libs/python/<folder>/.api/*.md` — then ' +
    'mine them for the full ADVANCED surface of each package (combinators, hooks, native pipelines, discriminators, async mirrors) and how ' +
    'packages STACK. List BOTH `.api/` tier DIRECTORIES in full and DIFF that complete inventory against what the folder pages already cite: every ' +
    'admitted catalog whose domain a page admits but does NOT yet use is an ADOPTION TARGET — adopt it to depth on the owning page, or when it ' +
    'belongs on a sibling page surface it as a residual; a relevant admitted catalog left unadopted is a DEFECT, not an omission. There is NO ' +
    'fixed library count: compose EVERY relevant admitted library into single dense operations woven as ONE rail, and ALWAYS layer the ' +
    'shared/substrate rails (expression `Result`/`Option`, msgspec/pydantic discriminated models, beartype validation, stamina retry, structlog + ' +
    'opentelemetry spans, anyio structured concurrency, numpy) ON TOP OF the folder-specific domain packages — NOT flat one-shot per-library uses. ' +
    'Use the DEEPEST primitive each package itself reaches for (LIBRARY_DEPTH) — flat code below the operator depth the admitted packages reach is ' +
    'surface sprawl in time; reject surface-level single-feature subsets and any thin rename wrapper; verify novel members with `uv run --frozen ' +
    'python -m tools.assay api`.',
  'PRESERVE all capability (densify, never delete functionality). Where a fence is already dense, deepen; where it is flat/naive, rebuild ' +
    'ground-up. Never regress correctness or boundary/strata law.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — structural collapse and both-tier `.api`-stacking are NECESSARY but NOT ' +
    'SUFFICIENT. A page can be fully collapsed into one closed family/ADT and STILL be capability-thin: modeling a NAIVE, LIMITED slice of its ' +
    'domain concept — a flat id/member set where the concept owns geometry, metrics, attributes, topology, and operations; a 2-variant ' +
    '`@tagged_union` where the domain has twenty; a `TypedDict`/`Struct` with the obvious 3 keys where the concept carries fifteen. Structural ' +
    'completeness and CAPABILITY completeness are ORTHOGONAL. A FULL rebuild ALSO closes the capability gap so the page OWNS ITS DOMAIN CONCEPT ' +
    'COMPLETELY. Per COMPOSED_IMPLEMENTATION + the doctrine growth law (capability grows sublinearly; growth lands as cases/rows/policy-values ' +
    'INSIDE existing owners, never new surfaces beside them), every real missing concern lands as a CASE in the existing closed ' +
    '`@tagged_union`/`Literal`/`StrEnum` family, a ROW or richer data on the existing `frozendict` table, a FIELD on the existing ' +
    '`msgspec.Struct`/Pydantic model/frozen dataclass/`TypedDict`, an OPERATION on the existing surface, or a POLICY_VALUE on the existing ' +
    'vocabulary — reshaping the owner as if it had always carried it; NEVER a parallel type, a new file, a sibling shape, or flat appended code.',
  'GAP SOURCES (every extension MUST cite exactly one — justified, never speculative): (a) PACKAGE — a member the admitted package surface exposes ' +
    'that the concept ADMITS but the page IGNORES is a missing case in the owner law (BOTH tiers: the shared `libs/python/.api/` rails — numpy, ' +
    'pydantic, msgspec, anyio, expression, beartype, stamina, structlog, opentelemetry — AND the folder domain packages; stacking that full ' +
    'surface IS new functionality woven into the owner, not a denser spelling of the same call; verify the member exists) — this is the PRIMARY ' +
    'source for the newly-admitted .api packages this pass integrates. (b) DOMAIN — an attribute, metric, sub-kind, relationship, state, or ' +
    'operation the REAL concept demands but the page omits (a mesh owns its topology/normals/UV/attributes and validate/repair/query operations, ' +
    'not a vertex array alone; a data schema owns constraints/indexes/partitions/coverage, not naive columns; a geometry owner owns the full ' +
    'predicate/transform/measure family the domain needs). (c) CONSUMER — a contract a sibling or downstream owner will require that has no ' +
    'composed spelling here yet (a need with no spelling marks a missing case: the law extends first, the feature lands second).',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy — capability COVERAGE against the full domain + both-tier package surface is the real measure. A ' +
    'SMALL page modeling a rich concept is almost always under-built (give it the DEEPEST sweep), AND a LARGE, well-collapsed page can still be ' +
    'capability-SPARSE (an owner that indexes membership but models none of the concept geometry, metrics, attributes, topology, or analytics). ' +
    'Assess each owner against its domain independently of size and EXTEND every owner the concept under-realizes IN PLACE — integrated and ' +
    'unified into the one owner at full operator depth, every new field/case/operation composing the existing rails — never a new flat surface ' +
    'beside it.',
  'JUSTIFIED, NOT RANDOM: if after a real domain + package + consumer sweep the concept is genuinely complete, prove it by adding nothing — never ' +
    'invent capability to look busy or pad with flat fields. Every added case/row/field/operation is load-bearing, cites a package member / domain ' +
    'attribute / consumer contract, and composes the existing rails; preserve ALL existing capability — extension only deepens, never regresses.',
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
    '749 deferred annotations where relevant. Keep every choice CONSISTENT across folders so the corpus reads as one ultra-advanced codebase, and ' +
    'apply the canonical file-organization + section-order law (TYPES -> CONSTANTS -> MODELS -> ERRORS -> SERVICES -> OPERATIONS -> COMPOSITION -> ' +
    'EXPORTS, owner blocks kept intact).',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every folder owner strictly in its lane and respect the dependency direction of the libs/python branch ' +
  'strata; internal code uses canonical names and shapes with mapping only at the edge; do not trample a sibling owner while densifying; never ' +
  'introduce a cyclic/downward dependency or leak a host/provider type into a runtime-neutral owner. This pass realizes ONLY the current folder ' +
  'pages; a concern owned twice across folders, a folder mixing unrelated concerns, or coupling to a sibling owner INTERIOR (vs its seam/wire) is ' +
  'a defect to fix in-folder or log as a cross-folder residual.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
    'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
    'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Prefer a table, a typed signature block, or a ' +
    'tight bullet wherever it carries the design better than a paragraph. Prose that ASSERTS capability the fence does not implement is a defect.',
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
const DOCTRINE = [LAW, '', INTEGRATE, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS].join('\n')
const FINAL = [
  'WHOLE-STACK CROSS-FOLDER ALIGNMENT — this is the FINAL pass, after every folder ran its own per-folder implement -> critique -> redteam and the ' +
    'residual reconcile. Hold ALL these libs/python folders in view at once and align them as ONE coherent branch: ' + ROSTER + '. Read each ' +
    'folder ARCHITECTURE.md ([1]-[DOMAIN_MAP] + [2]-[SEAMS]) and README, the central manifest (pyproject.toml) + the branch-substrate ' +
    '`libs/python/.api/`, and every design page a cross-folder seam spans. This is the ONLY pass that edits ACROSS folders — but it aligns at the ' +
    'seam and never trespasses a folder owner interior.',
  'ALIGN OBJECTIVES (fix every one in place across the spanned folders): (1) SEAMS/WIRES — every cross-folder producer/consumer relationship is ' +
    'recorded in BOTH endpoint folders ARCHITECTURE [02]-[SEAMS] with mirrored glyphs and ONE shared shape; a stale, missing, or one-sided seam is ' +
    'a defect. (2) PORTS/BOUNDARIES — a capability a folder now needs from a sibling has a named port/boundary on both sides; add the new port ' +
    'where a per-folder pass created the need but could not reach the sibling. (3) NO DUPLICATION — a concept owned twice across the branch ' +
    'collapses to the ONE rightful owner; the sibling consumes the seam, never a second mint. (4) NO LAYERING VIOLATION — respect the libs/python ' +
    'dependency direction; no cyclic/downward dependency, no host/provider-type leak into a runtime-neutral owner, every wire meets at one owner ' +
    'per boundary. (5) SUBSTRATE-TIER LEVERAGE — every folder FULLY and PROPERLY leverages the shared branch-substrate `.api` tier (the universal ' +
    'rails: expression `Result`/`Option`, msgspec/pydantic discriminated models, beartype, anyio structured concurrency, stamina, structlog + ' +
    'opentelemetry, numpy) for BOTH existing and new functionality, NEVER re-implementing a substrate capability; a consumer hand-rolling what the ' +
    'substrate owns is a defect to rewire to the substrate. (6) GAPS/OVERSIGHTS — hunt the mistakes no single-folder pass could see from inside ' +
    'one folder, and fix them.',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const scopeLine = (f) => 'FOLDER: `' + f.name + '` — design pages under `' + f.planning + '/**`, capability catalogs under `' + f.api + '/` plus ' +
  'the shared branch-substrate `libs/python/.api/`, governing docs at `' + f.root + '`.' + (f.note ? ' ' + f.note : '')
const implementPrompt = (f) => [DOCTRINE, '',
  'TASK: PER-FOLDER HOSTILE REBUILD + NEW-.api INTEGRATION of `' + f.name + '`. ' + scopeLine(f),
  'Read EVERY design page under `' + f.planning + '/**`, the folder `.api/api-*.md` catalogs (especially the newly-admitted ones a recent survey ' +
    'added and rebuild-api filled) AND the shared branch-substrate catalogs at `libs/python/.api/*.md`, the governing `ARCHITECTURE.md` + ' +
    '`README.md`, the docs/stacks/python/ core pages in README routing order, and VERIFY every cited member via `uv run --frozen python -m ' +
    'tools.assay api` (a member you cannot verify is a phantom — delete it). Then, across the WHOLE folder corpus: (1) INTEGRATE every ' +
    'newly-admitted package capability (both tiers) into the design — GROW the existing owner page where one fits (a ' +
    'case/row/field/operation/policy-value per the doctrine growth law + COMPOSED_IMPLEMENTATION), or AUTHOR a NEW page (and sub-domain folder) ' +
    'ONLY where the package is a genuinely new owner with no existing home, built ground-up to the .planning page grammar + the docs/stacks/python ' +
    'bar, and UPDATE `ARCHITECTURE.md` codemap + `[02]-[SEAMS]` + `README.md`; (2) HOSTILE GROUND-UP REBUILD of the folder pages to the ULTRA bar ' +
    '— construct in LIFECYCLE order (admit raw ONCE into a typed `TypedDict`/Pydantic payload -> canonical owner via OWNER_CHOOSER -> every ' +
    'cross-cutting concern as a STACKED signature+rail-preserving aspect over a thin pure core -> ONE unified `Result`/`Option`/`effect.result` ' +
    'rail with closed-vocabulary faults -> project + egress via `msgspec.Struct`, both ingress and egress parameterized), collapse >=3 parallel ' +
    'shapes into one `@tagged_union`/closed family/`frozendict` table IN THE SAME page, drive cases with a derived `frozendict` table or fold, and ' +
    'CLOSE the concept capability gaps in place (each extension citing a package member / domain attribute / consumer contract). Realize ONLY `' + f.name + '` ' +
    'pages; never edit a sibling folder page. py3.15-modern only (PEP 585/604/695, `frozendict` builtin, newest payload forms), high-signal prose ' +
    'all-backticked, comment hygiene, fix-in-place. Return verdict + integrated (each new .api woven in, and where) + newPages (any new page ' +
    'authored, with its justification) + collapsed (before->after counts) + extended (each addition + its cited source) + residual_high (each ' +
    '{files:[every repo-relative path the cross-folder fix spans], claim} for a CROSS-FOLDER item) + summary.'].join('\n')
const critiquePrompt = (f) => [DOCTRINE, '',
  'TASK: HOSTILE DOCTRINAL-CONFORMANCE AUDIT + CAPABILITY-COMPLETENESS + INTEGRATION AUDIT + FIX IN PLACE across `' + f.name + '`. ' + scopeLine(f) + ' ' +
    'You are an ULTRA-HARSH, UNAGREEABLE auditor: assume a violation exists in EVERY fence until you prove otherwise, trust NOTHING the prior pass ' +
    'or the prose claims, and "good enough"/"mature" is rejected outright. Read the folder pages, BOTH `.api/` tiers (folder + branch-substrate), ' +
    'and the operative docs/stacks/python/ pages. Run these MECHANICAL checklists line-by-line and REPAIR every hit in place (a fix, never a ' +
    'ledger note):',
  '(1) COLLAPSE_SCAN — apply the move for any of the 12 signals (3+ instances makes it mandatory): sibling prefix/suffix names -> one ' +
    'modality-polymorphic entrypoint; same return rail differing only by arity -> input-shape discrimination; a `get`/`get_many`/`get_by_id` ' +
    'family -> one input-keyed entrypoint; functions differing only by a literal -> parameterize the literal as policy; a bool parameter selecting ' +
    'two bodies -> one derived body or policy value; a function calling exactly one other -> delete the hop; a class exposing one public method -> ' +
    'module function or fold-on-owner; parallel dispatch arms repeating structure -> a `frozendict` table or fold algebra; several types sharing ' +
    'fields for one concept -> one closed family; 3+ sibling module constants for one concept -> one `frozendict`/`StrEnum`; a wrapper renaming a ' +
    'package API -> use the package surface directly; the same 2-4 wrappers recurring -> one parameterized aspect factory.',
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
  '(6) PAYLOADS/FROZENDICT/PEP/STRATA/MEMBERS/MODERN — payloads are `closed=`/`extra_items=` `TypedDict` via a module-level `TypeAdapter` with ' +
    '`Unpack[TypedDict]` at root entrypoints; `frozendict` (builtin) owns tables/evidence (no `MappingProxyType`/dict-table/tuple-pairs); PEP ' +
    '585/604/695 only, no `from __future__ import annotations`, no legacy typing; total `match` + `assert_never`; respect the libs/python ' +
    'dependency direction (no cyclic/downward dependency, no host/provider-type leak into a runtime-neutral owner); cite ONLY members confirmed in ' +
    'the `.api/` catalogs (verify novel members via `uv run --frozen python -m tools.assay api`); FULL docs/stacks/python conformance; BOTH the ' +
    'folder `.api/` catalogs AND the branch-substrate rails maximized; the `tools/py_analyzer` lint/type/contract gate clean.',
  '(7) INTEGRATION-COMPLETENESS + CAPABILITY-COMPLETENESS + ILLUSION — for EVERY newly-admitted folder OR branch-substrate `.api` package, verify ' +
    'its capability is actually COMPOSED into a fence (not just named in prose); an admitted package whose `.api` exposes capability no owner ' +
    'exploits is a named gap to close NOW (grow the owner, or — only if a genuinely new owner — a new page). Independently, structural collapse ' +
    'and capability completeness are ORTHOGONAL: a fully-collapsed owner can still model a NAIVE slice of its concept — close it in place. Reject ' +
    'the inverse: a speculative/padding field, decorative ceremony, an UNJUSTIFIED new page that fragments an existing owner, or prose asserting ' +
    'capability the fence lacks is deleted/folded back.',
  'Also enforce the docs/stacks/python file-organization + section-order law, cross-folder convention consistency, and prose + comment hygiene. ' +
    'EDIT the `' + f.name + '` pages to fix every hit; OVERRIDE any earlier residual you can now resolve. Return verdict + integrated + newPages + ' +
    'collapsed + extended + residual_high + summary.'].join('\n')
const redteamPrompt = (f) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE across `' + f.name + '`. ' + scopeLine(f) + ' You are the LAST and MOST AGGRESSIVE pass: ' +
    'assume the implement and critique missed things and that the chosen design is naive or illusory until PROVEN the strongest, the burden of ' +
    'proof ON THE DESIGN; trust nothing the prior passes or the prose claimed. Open BOTH `.api/` tiers (folder + branch-substrate), the sibling ' +
    'pages, and the operative docs/stacks/python/ pages. Attack from every direction and REPAIR every defect in place — no soft-pedalling, a fix ' +
    'never a ledger.',
  'PRIMARY LENS: (A) COUNTERFACTUAL on each core choice — is the owner, the algebra (fold/derived-`frozendict`-table/closed family), and the ' +
    'dispatch form categorically the strongest the doctrine admits, or does a denser owner or a DEEPER admitted-package primitive ' +
    '(expression/msgspec/pydantic/anyio/numpy or a folder domain package) collapse the whole fence? Rebuild to the stronger form; never defend the ' +
    'incumbent. (B) ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: the next case/dimension/knob/modality/provider lands as ONE ' +
    'case/row/policy value with every consumer untouched or broken LOUDLY at type-check? If it would touch multiple sites, reshape the growth ' +
    'axis. (C) LONG-TAIL + MULTI-DIMENSIONAL — attack every input/output/edge/failure mode (empty, singular, plural, stream, malformed, ' +
    'concurrent, cancelled, partial-failure, version-skew); accumulate-vs-abort correct for the REAL boundary; BOTH ingress AND egress ' +
    'parameterized. (D) BOUNDARY-INTEGRITY — a cyclic/downward dependency, a host-type leak into a runtime-neutral owner, a concern owned twice ' +
    'across folders, a folder mixing concerns, coupling to a sibling owner INTERIOR (vs its seam/wire), OR a sibling page left STALE by this ' +
    'folder change (seam drift) is a defect: fix in-folder or log as a cross-folder residual. (E) SURFACE-SPRAWL-IN-TIME + PHANTOMS — an admitted ' +
    'package (either tier) whose `.api` exposes capability the fence re-derives by hand, flat code below the operator depth the packages reach, a ' +
    'phantom member, or a thin wrapper: collapse to package depth and verify the member (via `uv run --frozen python -m tools.assay api`). (F) ' +
    'INTEGRATION + CAPABILITY-COMPLETENESS — counterfactually attack each owner for domain-completeness AND verify every newly-admitted both-tier ' +
    '.api capability is genuinely composed into a fence; an UNJUSTIFIED new page (fragmenting an existing owner) is folded back, a MISSING owner ' +
    '(a real new concern with no page) is authored.',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (every time): re-attack every conformance dimension with fresh hostile eyes — the COLLAPSE_SCAN signals, ' +
    'OWNER_CHOOSER per shape, the KNOB_TEST per param, the ASPECT taxonomy, rail + closed-fault-vocabulary discipline, integration + ' +
    'capability-completeness per owner, payload/`frozendict`/PEP conformance, py3.15-modern typing, docs/stacks/python conformance, both-tier ' +
    '`.api` maximization, the `tools/py_analyzer` gate, and prose/comment hygiene — and fix every defect. The folder must end objectively denser, ' +
    'MORE CAPABLE, more correct than the critique left it; if the strongest form is genuinely already present, prove it by finding nothing — never ' +
    'invent churn. Realize ONLY `' + f.name + '` pages; log cross-FOLDER items as residual_high. Return verdict + integrated + newPages + ' +
    'collapsed + extended + residual_high + summary.'].join('\n')
const STAGES = [
  { key: 'implement', build: implementPrompt, effort: 'max' },
  { key: 'critique', build: critiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: redteamPrompt, effort: 'max' },
]
const processFolder = async (f) => {
  const logs = {}
  for (const st of STAGES) {
    const r = await agent(st.build(f), { label: st.key + ':' + f.name, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: 600000 })
    if (r === null) break
    logs[st.key] = r
  }
  return { folder: f.name, logs, ok: Object.keys(logs).length === STAGES.length }
}
const finalAlignPrompt = () => [DOCTRINE, '', FINAL, '', 'TASK: WHOLE-STACK ALIGN — walk every cross-folder seam/wire/port/boundary across all ' +
  'folders and ALIGN them in place. Record every seam in both endpoint ARCHITECTURE [02]-[SEAMS] with mirrored glyphs, add the new ' +
  'ports/boundaries the per-folder passes surfaced, collapse any concept owned twice to its rightful owner (the sibling consumes the seam), fix ' +
  'any layering violation or host-type leak, and rewire any consumer that hand-rolls a branch-substrate capability to leverage the substrate ' +
  'instead. Read the folders ARCHITECTURE + README + central pyproject.toml + the branch-substrate `libs/python/.api/` + the spanned design pages; ' +
  'verify any cited member via `uv run --frozen python -m tools.assay api`. Fix every defect in place across the spanned folders, regressing none. ' +
  'Return verdict + aligned (each alignment made, naming the folders + the seam/port) + residual (each {files, claim} for anything you could not ' +
  'fully resolve) + summary.'].join('\n')
const finalCritiquePrompt = () => [DOCTRINE, '', FINAL, '', 'TASK: ADVERSARIAL AUDIT of the whole-stack alignment + FIX IN PLACE. Assume a ' +
  'cross-folder defect remains until you prove otherwise; trust nothing the align pass claimed. Re-walk EVERY seam/wire/port/boundary across all ' +
  'folders: a one-sided or stale seam, a missing mirror glyph, a concept still owned twice, a residual layering violation or cyclic/downward ' +
  'dependency or host-type leak, a consumer still hand-rolling a branch-substrate capability, a new port the align pass missed, a gap between two ' +
  'folders. Repair every hit in place across the spanned folders. Return verdict + aligned + residual + summary.'].join('\n')
const finalRedteamPrompt = () => [DOCTRINE, '', FINAL, '', 'TASK: ADVERSARIAL RED-TEAM of the whole-stack alignment — the LAST and most aggressive ' +
  'whole-stack pass. Trust nothing the align/critique claimed. COUNTERFACTUALLY attack the cross-folder ownership: is each shared concept on the ' +
  'RIGHT owner; is each seam the strongest shared shape; does any consumer still under-leverage the branch-substrate tier or duplicate a sibling; ' +
  'is any layering edge, boundary, or wire still wrong; will the next cross-folder growth axis (a new shared case/port/provider) land cleanly ' +
  'without touching multiple interiors? Fix every defect in place across the spanned folders; if the branch is genuinely coherent, prove it by ' +
  'finding nothing — never invent churn. Return verdict + aligned + residual + summary.'].join('\n')
const ALIGN_STAGES = [
  { key: 'align', build: finalAlignPrompt, effort: 'max' },
  { key: 'critique', build: finalCritiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: finalRedteamPrompt, effort: 'max' },
]

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

log('py-rebuild-many: ' + FOLDERS.length + ' folders, per-folder implement -> critique -> redteam, pooled at CAP=' + Math.min(CAP, FOLDERS.length))
phase('Realize')
const done = (await pool(FOLDERS, CAP, (f) => processFolder(f))).filter(Boolean)

const allRes = []
for (const r of done) for (const st of ['implement', 'critique', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push({ files: (x.files && x.files.length ? x.files : [r.folder]), claim: x.claim }) }
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Realize: ' + done.filter((r) => r.ok).length + '/' + FOLDERS.length + ' folders complete; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' ' +
  'clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent([LAW, '', INTEGRATE, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', 'TASK: RECONCILE these ' +
      'cross-FOLDER residuals the implement/critique/redteam passes deferred. There is NO severity — treat EVERY residual as must-address. Read ' +
      'EVERY listed file. For each: if it is a real cross-folder defect, FIX it in place (unify the shared type/seam/rail, repair the ' +
      'dependency-direction/boundary issue, align a stale sibling page, or extend the shared owner in place to close a capability gap that spans ' +
      'folders), preserving all capability and regressing no file; if a residual is FACTUALLY INCORRECT, leave it and say why — never silently ' +
      'skip a real one. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix:' + i, phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 600000 })
    if (!fix) return null
    const verify = await agent([LAW, '', BOUNDARIES, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named files from disk and ' +
      'classify each residual: status "fixed" (real defect now genuinely resolved), "invalid" (the claim is factually wrong — cite why), or "open" ' +
      '(real defect still NOT resolved). Default "open" on any doubt; mark "invalid" ONLY when you can show the claim is wrong. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles ' +
      'the fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 600000 })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const hard_residual = uniq.filter((r) => openClaims.has(r.claim))
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
const integratedAll = done.flatMap((r) => Object.values(r.logs || {}).flatMap((l) => l.integrated || []))
const newPagesAll = done.flatMap((r) => Object.values(r.logs || {}).flatMap((l) => l.newPages || []))
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open (hard residual), ' + dropped.length + ' dropped as invalid')

// --- [FINAL_ALIGN]
phase('Final-Align')
const alignLogs = {}
for (const st of ALIGN_STAGES) {
  const r = await agent(st.build(), { label: 'final-' + st.key, phase: 'Final-Align', schema: ALIGN_SCHEMA, effort: st.effort, stallMs: 900000 })
  if (r === null) break
  alignLogs[st.key] = r
}
const alignedAll = Object.values(alignLogs).flatMap((l) => (l && l.aligned) || [])
const finalResidual = Object.values(alignLogs).flatMap((l) => (l && l.residual) || [])
log('Final-Align: ' + Object.keys(alignLogs).length + '/3 whole-stack passes; ' + alignedAll.length + ' alignments, ' + finalResidual.length + ' ' +
  'residual')

return { folders: FOLDERS.map((f) => f.name), complete: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, integrated: [...new Set(integratedAll)], newPages: [...new Set(newPagesAll)], clusters: clusters.length, hard_residual: hard_residual, dropped: dropped, finalAligned: alignedAll, finalResidual: finalResidual }
