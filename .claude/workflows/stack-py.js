export const meta = {
  name: 'stack-py',
  whenToUse: 'Harden the docs/stacks/python code doctrine in place across core, domain, and numerics shards.',
  description: 'Durable, reusable FULL-STACK hardening of the docs/stacks/python CODE DOCTRINE — the single standing python-stack WF (peer to stack-cs and stack-ts), run whenever to push the whole settled corpus (core concept pages + domain/ shards + numerics/ shards) to the 13/10 ultra-dense bar through a full initial -> critique -> redteam adversarial pass per page. NOT a from-scratch build and NOT a restructure (the file set is settled) — a hostile per-file harden in place. Every stage is HOSTILE: the page is naive/shallow/illusory until it survives an aggressive attack, the burden of proof on the page. CRITIQUE is the mechanical line-by-line doctrinal-conformance audit (COLLAPSE_SCAN, OWNER_CHOOSER, KNOB_TEST, AOP, rails, py3.15/payloads, citation tier, agnostic snippet law, page-craft, altitude, zero-meta, capability-completeness) fixed in place; REDTEAM is critique AND MORE — counterfactual on the core teaching shape, anticipatory-collapse of the next case, corpus-wide duplication, AOP + shape-budget maximization, substrate-depth + phantoms, capability-completeness, plus a full cold adversarial re-review. Phases: Inventory (atlas + domain/ + numerics/ routers -> the core + domain + numerics file set) -> Harden (1 agent/file, rebuild(max) -> critique(xhigh) -> redteam(max), pooled at CAP=12, 1500ms launch gate) -> Sweep (sequential atlas-order pass, implicit upward stacking, region ledger, zero duplicated snippets) -> Reconcile (union-find cross-file residuals -> fix(max) -> adversarial verify(xhigh); hard residuals hand off to resolve-residuals). The csharp doc set is the read-only FLOOR/reference; snippets agnostic; every cited .api member verified via assay api; every edit scoped to docs/stacks/python. Takes no args.',
  phases: [
    { title: 'Inventory', detail: 'parse the README atlas + the domain/ and numerics/ routers for the ordered core + domain + numerics file set + per-file state, emit the region ledger seed' },
    { title: 'Harden', detail: 'per file (1 agent/file): rebuild(max) -> critique(xhigh, mechanical conformance audit) -> redteam(max, critique AND MORE + full cold re-review), every stage ADVERSARIAL (naive/illusory-by-default), pooled at CAP=12' },
    { title: 'Sweep', detail: 'sequential atlas-order pass: each file reads finalized priors via the region ledger, routes altitude (no re-teach), removes duplicated snippet demonstrations' },
    { title: 'Reconcile', detail: 'union-find cluster cross-file residuals by shared file -> fix(max) -> adversarial verify(xhigh); hard residuals hand off to resolve-residuals' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 12
const STAGGER_MS = 1500
const STALL = 300000
const ROOT = 'docs/stacks/python'

// --- [MODELS] ----------------------------------------------------------------------------
const INVENTORY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'order'], properties: { path: { type: 'string' }, order: { type: 'integer' }, folder: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } }, map: { type: 'string' } } } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'owned_regions'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['routed', 'clean'] }, rerouted: { type: 'array', items: { type: 'string' } }, owned_regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } }, repaired: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'TARGET: docs/stacks/python/ is the route-owned Python CODE DOCTRINE — a doc set of AGNOSTIC teaching pages (core concept pages + a domain/ ' +
    'shard set + a numerics/ shard set) that legislate how all project Python is written. It is NOT a libs/python/.planning design corpus: a page ' +
    'teaches a coding LAW with one exemplary agnostic snippet, never a concrete module. The README owns routing + the 16 doctrine laws + the ' +
    'COLLAPSE_SCAN + page-craft; the domain/ and numerics/ READMEs are one-table routers whose shards compose the core laws and never re-open ' +
    'them; each concept page owns ONE disjoint layer and states doctrine as fact. READ docs/stacks/python/README.md sections [02]-[DOCTRINE], ' +
    '[03]-[COLLAPSE_SCAN], [05]-[PAGE_CRAFT], [06]-[CORPUS_LAW] + the sub-folder routers and hold them as law.',
  'QUALITY BAR: the PYTHON stack is the highest-rigor stack in the repo; docs/stacks/csharp/ (the README + shapes + surfaces-and-dispatch + the ' +
    'domain/ shards) is the read-only FLOOR/reference — NEVER edit a csharp file. Mirror its DENSITY — SHAPE_BUDGET, DEEP_SURFACES, MODAL_ARITY, ' +
    'ANTICIPATORY_COLLAPSE (one owner ready to replace 10+ loose things), POLICY_VALUES, OWNER_CHOOSER — in pure-Python idiom and push PAST it to ' +
    'the strongest form Python admits; NEVER import a C#/LanguageExt spelling (`[Union]`, `Fin`, `SmartEnum`) into a Python page. The csharp set ' +
    'is a floor for how dense the Python set must read, never how it must be spelled.',
  'WRITE-FULLY MANDATE, scoped to docs/stacks/python/** ONLY: every defect you identify you FIX NOW via Edit/Write directly in the file; the ' +
    'structured fix-log you return is a REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should hedge. Edit ONLY files under ' +
    'docs/stacks/python/; reading csharp/standards/.api files is allowed, editing anything outside docs/stacks/python/ is forbidden. Leave nothing ' +
    'behind except genuine cross-FILE items (report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, AND red-team) is HOSTILE: assume the page is NAIVE, SHALLOW, JUNIOR, or ILLUSORY until it ' +
    'survives an aggressive attack; the burden of proof is ON THE PAGE, never on you. A `finalized` state marker, "mature", "already strong", ' +
    '"good enough", and a prior `clean` grade are REJECTED self-assessments. Default to "this page is naive and must be rebuilt to the strongest ' +
    'form the doctrine admits" and MAKE that rebuild; a no-edit verdict is earned ONLY after a genuinely aggressive attack finds nothing.',
  'ILLUSORY / FAKE content is the PRIMARY target — the MOST dangerous content is what PRETENDS to be advanced: a snippet that READS dense and ' +
    'confident (uses `@tagged_union`/`frozendict`/the rails, cites packages) yet demonstrates a THIN slice; prose that ASSERTS richness the fence ' +
    'does not contain; a card field (`Use`/`Accept`/`Reject`/`Law`/`Boundary`) that decides nothing; a structurally-correct collapse that is ' +
    'semantically empty; a `.api` member cited but unverifiable (a PHANTOM — delete it). Treat dense, confident-looking fences with MORE ' +
    'suspicion, not less, and DISBELIEVE every claim the page makes about itself until verified.',
  'NAIVETY is a defect on TWO orthogonal axes, both intolerable: COVERAGE — the owner models a THIN SLICE of its concept (the obvious three ' +
    'fields where the domain carries fifteen; a two-case family for a twenty-case space); APPROACH — enumerated hardcoded instances where a ' +
    'parameterized, algorithmic owner should GENERATE the space (a fixed roster of styles, patterns, or variants is seed DATA feeding ONE ' +
    'generator over named parameters, never the mechanism itself). Attack both axes in every fence and repair on sight.',
  'CRITIQUE vs RED-TEAM (hold the distinction): CRITIQUE is the MECHANICAL, line-by-line doctrinal-conformance audit — run every checklist signal ' +
    'across every fence and FIX each hit in place. RED-TEAM is critique AND MORE — the LAST, MOST AGGRESSIVE pass that re-runs the entire critique ' +
    'with fresh hostile eyes AND adds the architect lenses (counterfactual on the core shape, anticipatory-collapse of the next case, corpus-wide ' +
    'duplication, AOP + shape-budget maximization, substrate-depth + phantoms, capability-completeness). Neither is a ledger of notes: both EDIT ' +
    'the page; the page must end objectively denser, more capable, and more doctrine-compliant than each stage found it.',
].join('\n')
const PYDOCTRINE16 = [
  'HOLD the README [02]-[DOCTRINE] 16 laws as fact, never restated on a concept page: [FLOW] EXPRESSION_SPINE (domain logic is expression-shaped; ' +
    'dependent steps `bind`, independent ones accumulate; statements survive only in a measured kernel that names the exemption) + ' +
    'BOUNDARY_ADMISSION (`Raw -> Payload -> Canonical owner -> Rail -> Projection -> Egress`; raw admitted EXACTLY ONCE into an evidence-carrying ' +
    'owner; interior never re-validates or sees `None`-as-failure/sentinel/provider shape). [SHAPE] SHAPE_BUDGET + DEEP_SURFACES + MODAL_ARITY + ' +
    'ANTICIPATORY_COLLAPSE. [DERIVATION] POLICY_VALUES + DERIVED_LOGIC + DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] ' +
    'LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION.',
  'RUN the README [03]-[COLLAPSE_SCAN] 12-signal table on every fence: any signal triggers the collapse move, 3+ instances make it mandatory. ' +
    'Every enumerated collapse/kill list in this doctrine is a FLOOR, never the complete set: any repeated structure, parallel spelling, or ' +
    'enumerable family an algebra, table, fold, or generator can own is a collapse target you hunt down yourself, listed or not. A page that ' +
    'demonstrates a coding law must itself obey every law it can reach.',
].join('\n')
const SHAPE_ADT = [
  'EXTREME SHAPE/TYPE DENSITY (the central mandate): one concept owns exactly ONE type as a dense CLOSED-FAMILY ADT — `@tagged_union` / `Literal` ' +
    'set / `StrEnum` / `msgspec.Struct` / frozen dataclass / `frozendict` table — chosen by the OWNER_CHOOSER 5 discriminants (admission, identity ' +
    'regime, variant arity, payload timing, openness). KILL on sight: loose 1-2-field classes, parallel DTOs, field-rename wrappers, tag-only ' +
    'shapes, and 3+ sibling module constants/types for one concept (collapse to one `frozendict`/`StrEnum`/closed family). No loose/weak type or ' +
    'constant spam survives.',
  'ANTICIPATORY_COLLAPSE: shape the owner for the family it WILL absorb so the next case/dimension/modality lands as ONE declaration with every ' +
    'consumer untouched or broken loudly at type-check — one owner READY TO REPLACE 10+ loose things. The exemplary snippet MUST show the owner at ' +
    'that scale with the growth axis visible, not an isolated minimum.',
].join('\n')
const AOP_FIRST = [
  'FULL AOP (ULTRA-CRITICAL): every CROSS-CUTTING concern — retry, telemetry/spans, validation, contracts, memoization, registration, receipts, ' +
    'fault rails — is OUR OWN signature- AND rail-preserving decorator (inline `**P` + `functools.wraps`) STACKED over a THIN PURE CORE; it ' +
    'materializes policy, stacks in deterministic order, and NEVER raises into domain flow (a failing aspect returns the rail `Error`). The domain ' +
    'transform itself stays a pure function/fold under the stack.',
  'AOP is NOT merely applying `@beartype`/`@msgspec` decorators — though `beartype` (contracts) and `msgspec` (struct/codec) MUST be fully ' +
    'integrated. It means our code SHAPED TOWARD decorators: MAXIMIZE how much functionality is expressed AS stacked decorators; 2-4 co-occurring ' +
    'wrappers collapse into ONE parameterized aspect factory; a page reads as stacked decorators over a pure core, never inline-repeated concerns ' +
    'or sibling helper functions. A concern open-coded where a decorator belongs is a defect.',
].join('\n')
const PARAM_POLY = [
  'HEAVY PARAMETERIZATION, ZERO HARDCODING/FRAGILE LOGIC, FULL POLYMORPHISM. ONE entrypoint owns every modality — `T | Iterable[T]` normalized ' +
    'ONCE at the head, discriminating on input SHAPE (type/tag/pattern/arity), never a name suffix or a `mode`/`batch`/`strict` knob (KNOB_TEST: ' +
    'delete each parameter; if the value reconstructs what it carried, it was a knob to collapse).',
  'Configuration enters as ONE behavior-carrying value — a vocabulary member, a tagged variant, a frozen policy table (POLICY_VALUES) — never a ' +
    'flag set the body re-derives. A `timeout`/`retry`/`deadline` is an aspect or an `anyio` scope, never a signature param. Cases sharing ' +
    'generative structure are DERIVED from one primary `frozendict` correspondence (DERIVED_LOGIC), never enumerated arms.',
].join('\n')
const CORE_LOGIC = [
  'WORLD-CLASS ALGORITHMIC BODIES (the function-body mandate, owner = iteration.md): every body that does real work is expression-shaped at full ' +
    'operator depth — a naive `for`-loop with mutable accumulation, a hand-rolled index counter, a `while`-mutator, or an intermediate materialized ' +
    'list where a fused form expresses it is a DEFECT. Collapse multi-accumulator scans into ONE `functools.reduce`/`itertools.accumulate` over a ' +
    'tuple/struct seed; fuse comprehensions and generators (no intermediate lists); reach the FULL `itertools`/`functools` combinator surface ' +
    '(`accumulate`/`groupby`/`pairwise`/`chain.from_iterable`/`product`/`starmap`/`takewhile`/`tee`/`islice`/`batched`/`compress` + ' +
    '`reduce`/`cache`/`partial`/`singledispatch`); walrus where it fuses test+bind; index/window/slice algebra and `zip(strict=True)` over manual ' +
    'indexing; structural `match` as data-shape dispatch; recursion or divide-and-conquer where it reads cleaner than iteration.',
  'ALTITUDE: iteration.md owns the NON-carrier, NON-numeric pure-computation algebra; rails-and-effects owns carrier-threading folds ' +
    '(`Block.fold`/`traverse`/`sequence`/`partition`); algorithms.md/numerics owns numeric folds. A body re-deriving line-by-line what a deeper ' +
    'composed form states once is surface-sprawl-in-time. Every snippet body across the corpus obeys this and composes iteration.md as settled ' +
    'supporting material, never re-teaching it.',
].join('\n')
const ASYNC_DEPTH = [
  'STRUCTURED-CONCURRENCY DEPTH (wherever a page touches async): one `anyio` failure boundary, NEVER naive `asyncio`. Task fan-out is a task group ' +
    'with a typed child carrier; cancellation is scope-based (`fail_after`/`move_on_after`/`current_effective_deadline`, level-triggered, ' +
    '`get_cancelled_exc_class()` re-raised under `CancelScope(shield=True)`, never railed into a `Result.Error`); offload is a `frozendict[Lane, ' +
    'Offload]` table over `to_thread`/`to_interpreter`/`to_process` + `CapacityLimiter`; a `BaseExceptionGroup` is partitioned via `except*` and ' +
    'reduced through the rail fault monoid; memory object streams + sync primitives own producer/consumer handoff; teardown is `AsyncExitStack` + a ' +
    'shielded close. Retry is `stamina` (`AsyncRetryingCaller`/`retry_context` + `RetryHook` spans) over raised transients only — never a loop.',
  'REJECT on sight: `asyncio.gather`/`create_task`/`wait_for`/`sleep`, `ThreadPoolExecutor`/`ProcessPoolExecutor` on the loop, bare `try`/`finally` ' +
    'teardown, wall-clock sleep in tests (use `trio` `MockClock(autojump_threshold=0)`/`wait_all_tasks_blocked`). The `trio` backend is the proven ' +
    'seam; a transport rail (`asyncssh` `SSHClientConnection.run(check=True)`) is a rail-CONSUMER example, never the subject of a core page. Stack ' +
    'the async surface at full depth — flat single-await code below the structured-concurrency operator depth is a defect.',
].join('\n')
const PAYLOAD_TAXONOMY = [
  'PAYLOAD TAXONOMY (closed, MINIMAL, each pushed PAST the limit — `frozendict` is ONE member, NEVER the default): the payload shape is chosen by ' +
    'discriminant, never settled on one form. EDGE keyword ingress -> closed `TypedDict` (`closed=True`/`extra_items=T`, per-key ' +
    '`Required`/`NotRequired`/`ReadOnly`) via a module-level `TypeAdapter`, `Unpack[TypedDict]` only at root entrypoints. UNTRUSTED ingress -> a ' +
    '`pydantic` model at the edge (discriminated unions via `Field(discriminator=...)`, `Annotated` `AfterValidator`/`WrapValidator`, ' +
    '`model_validator`, `TypeAdapter.validate_python`/`validate_json`). STRUCTURED domain payload -> `@tagged_union` `case()` carrying per-case ' +
    'data. WIRE/egress -> `msgspec.Struct(frozen=True)` at full depth (`tag`/`tag_field` tagged unions, `array_like=True`, `forbid_unknown_fields`, ' +
    '`msgspec.Meta` constraints). DEFERRED octets -> `msgspec.Raw`. IMMUTABLE table/evidence/extension-band -> `frozendict`.',
  'Each shape is mined to its DEEPEST API form and the family stays MINIMAL — adding a parallel shape for one concept is a defect, and so is forcing ' +
    'one shape (a `dict[str, Any]` bag, a `MappingProxyType`, a mutate-then-freeze, a tuple-pair pseudo-map, homogeneous `**kwargs`) where the ' +
    'discriminant selects another. Owners: shapes.md owns the taxonomy + admission, boundaries.md owns the wire/octet seam.',
].join('\n')
const PY315 = [
  'BLEEDING-EDGE Python 3.15 ONLY: PEP 585 builtin generics, PEP 604 unions, PEP 695 type parameters (`class C[T]`, `def f[T]`, `type Alias[T] = ' +
    '...`). NEVER `from __future__ import annotations`; NEVER legacy `typing.List`/`Optional`/`Union`/`TypeVar`+`Generic`. Use ' +
    '`Self`/`override`/`TypeIs`/`assert_never`/`ReadOnly`/`LiteralString`, total `match` + `assert_never` over closed unions, walrus where it ' +
    'tightens.',
  'PAYLOADS — newest form: ingress payloads are closed `TypedDict` (`closed=True` or `extra_items=T`, per-key `Required`/`NotRequired`/`ReadOnly`) ' +
    'admitted through a module-level `TypeAdapter`, with `Unpack[TypedDict]` at root keyword entrypoints (never forwarded through interiors); ' +
    '`msgspec.Struct(frozen=True)` owns wire/egress. `frozendict` (py3.15 builtin) owns immutable map rows, dispatch/policy TABLES, and evidence — ' +
    'REJECT `MappingProxyType`, a module-level mutable `dict` used as a table, tuple-pair pseudo-maps, mutate-then-freeze, `dict[str, Any]` bags, ' +
    'and homogeneous `**kwargs`.',
].join('\n')
const APISTACK_SUBSTRATE = [
  'CITATION TIER (load-bearing, doctrine-specific): the SHARED substrate catalogs — `expression`, `pydantic`, `pydantic-settings`, `beartype`, ' +
    '`msgspec`, `anyio`, `structlog`, `stamina`, `numpy`, `psutil`, `opentelemetry-*`, `protobuf`, `grpcio` — are cited by EVERY page. ENUMERATE ' +
    'the catalog set IN FULL with a real `ls`/`fd` listing at every stage — the catalog root is docs/stacks/python/.api/ when present, else the ' +
    'language-wide libs/python/.api/ — never a memory-recalled roster. A CORE page cites ONLY this shared substrate. A DOMAIN or NUMERICS shard ' +
    'cites the shared substrate PLUS the package-cluster tier (the `.api/` catalogs for the packages its concept composes) — a numerics shard ' +
    'additionally STACKS its scientific cluster at the deepest dense/sparse/iterative/structured primitives its concern reaches — and NOTHING ' +
    'outside its declared cluster. Mine BOTH tiers to their FULL advanced surface and STACK them as ONE dense rail — `expression` rails + ' +
    '`msgspec`/`pydantic` models + `beartype` contracts + `stamina` retry + `structlog`+OTel spans + `anyio` concurrency layered together, never ' +
    'flat one-shot per-library uses.',
  'Cite ONLY members confirmed in the actual .api catalog file; a member you cannot verify is a PHANTOM to delete (verify novel members via `uv ' +
    'run python -m tools.assay api`). An admitted capability the concept admits but NO owner exploits is a DEFECT the pass closes in place, ' +
    'citing the exact member. A domain/numerics shard composes the FINALIZED core laws as settled material (it never re-opens ' +
    'admission/shape/rail/dispatch/boundary decisions the core owns) and owns ONE closed vocabulary for its axis. Use the DEEPEST primitive each ' +
    'package reaches (LIBRARY_DEPTH); flat code below that operator depth is surface sprawl.',
].join('\n')
const PAGECRAFT = [
  'PAGE-CRAFT LAW (README [05]-[PAGE_CRAFT]): page grammar is a NARROW index table, then deep FAMILY CARDS, then ONE agnostic snippet beside the ' +
    'rule it proves; the page ends at its last card. CARD ECONOMY: cards are few, deep, evidence-dense; near-peer cards MERGE until each owns a ' +
    'decision cluster; a card line carries exactly ONE decision; a `Use`/`Accept`/`Reject`/`Law`/`Boundary` field appears only where it decides ' +
    'something — a field that decides nothing is DELETED, not filled. Tables enumerate, cards legislate (rows stay atomic, no prose cramming, no ' +
    'links in cells).',
  'REJECT columns are LOAD-BEARING: every `Use` names the spelling, wrapper, or local pattern it DELETES. CODE NAMES BEFORE PROSE: every member a ' +
    'card or snippet names is verified against the installed package before it is written; a nameable surface spelled as prose is a defect. ZERO ' +
    'META: no provenance, source trace, release narration, process state, freshness disclaimer, or tool/skill context — any such block POISONS ' +
    'every downstream generation that loads the page; a stale `capture-pending`/`research` block is deleted. The domain/ and numerics/ folders ' +
    'each carry their own one-table README router (pages compose root laws, never re-open them).',
].join('\n')
const AGNOSTIC_SNIPPETS = [
  'AGNOSTIC SNIPPET LAW (style-guide [07]-[PLACEHOLDER_LAW]): every snippet COMPILES under Python 3.15 with legal NEUTRAL identifiers — ' +
    '`Shape`/`RefinedShape`/`Variant`/`PRIMARY`/`Field`/`KEY`/`Row`/`ROW_A`/`TABLE`/`SELECTED` — and placeholder strings (`"<value-a>"`) appear ' +
    'ONLY inside literals. NO project, repo, host, customer, pricing, deployment, or business-domain noun anchors a snippet; a domain noun is ' +
    'context poison. (A domain/numerics doctrine page teaches a coding-axis or numerical LAW agnostically; it is NOT a concrete module or solver ' +
    'for one problem.)',
  'CORPUS-WIDE ZERO duplicated snippet demonstrations: each snippet exercises a surface region NO OTHER snippet in the corpus shows — the region ' +
    'is its spotlight; finalized surfaces composed as supporting material occupy no region and duplicate nothing. A duplicated region is repaired ' +
    'by ROUTING to its owner (compose it as supporting material), never by re-teaching. Snippets are doctrine-exemplary at full operator depth, ' +
    '~3-4x denser than ordinary code, at the scale a large system takes (admission + dispatch + rail + policy in one fence with the growth axis ' +
    'visible).',
].join('\n')
const OPINIONATED = [
  'HEAVILY-OPINIONATED PROJECT DOCTRINE, NOT a language survey. ZERO table-stakes is tolerated, ever: a card or snippet teaching something a ' +
    'competent Python developer already knows — rather than an opinionated, dense, project-specific CHOICE — is a DEFECT to delete or densify. No ' +
    'net-casting to "cover the language"; cover only the opinionated decisions the projects need, each at 13/10.',
  'LOC budget ~450 is a SOFT pressure signal toward DENSIFICATION, NOT a hard gate. The real metric is per-card and per-snippet density: every ' +
    'card and every snippet world-class, zero filler. NEVER strip snippet whitespace, remove design content, or fragment a coherent concept to hit ' +
    'a number; a coherent dense concept may exceed 450 when every card and snippet earns its place (the csharp algorithms.md monolith is the ' +
    'precedent). A split is justified ONLY by concept disjointness, never by line count.',
].join('\n')
const STYLE_PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md: lead each section with the controlling rule/contract; one idea per paragraph; close on the ' +
    'consequence or boundary. Cut hedges (`may`/`might`/`probably`/`generally`/`where possible`/`if needed`), provenance, process narration, and ' +
    'report framing. Prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. Prose that ' +
    'ASSERTS capability the fence lacks is a defect, not content.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in a code span; name ' +
    'the exact member instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE + FILE ORGANIZATION: code fences are agent-facing. Do NOT use section-divider headers in snippets (NO `# --- ' +
  '[LABEL] ---` lines and NO standalone `[LABEL]` section-header comments — they add LOC without value); organize PURELY by the canonical ' +
  'declaration ORDER (CLAUDE.md [08]-[FILE_ORGANIZATION]: imports + `TYPE_CHECKING` -> types -> constants -> models -> errors -> services -> ' +
  'operations -> composition -> exports; types before classes; owner blocks + dependency clusters kept intact; the Python overlay puts runtime ' +
  'decoders/registries/tables AFTER the symbols they inspect; runtime/dependency order wins so every fence loads top-to-bottom), and strip any ' +
  'existing divider/section-label line. Beyond that, comment ONLY where intent is not already obvious from names, types, and signatures: default ' +
  'ZERO comments; at most 1 line where a comment genuinely earns its place. No narration, no restating the code, no docstring bloat, no ' +
  'task/process/review comments.'
const DOCTRINE = [LAW, '', ADVERSARIAL, '', PYDOCTRINE16, '', SHAPE_ADT, '', AOP_FIRST, '', PARAM_POLY, '', CORE_LOGIC, '', ASYNC_DEPTH, '', PY315, '', PAYLOAD_TAXONOMY, '', APISTACK_SUBSTRATE, '', PAGECRAFT, '', AGNOSTIC_SNIPPETS, '', OPINIONATED, '', STYLE_PROSE, '', COMMENTS].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
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
const nameOf = (p) => p.indexOf(ROOT + '/') === 0 ? p.slice(ROOT.length + 1) : p
const authorPrompt = (page) => [DOCTRINE, '', 'TASK: HOSTILE HARDEN of ' + page + ' to the ULTRA-DENSE Python doctrine bar. DISBELIEVE the page — ' +
  'assume every fence is naive, junior, or illusory until proven 13/10, and treat dense confident-looking content as a prime suspect for ' +
  'hollow/decorative complexity. Read the page, the README atlas + doctrine sections + the domain/ and numerics/ routers, its sibling pages ' +
  '(cross-page unification), the csharp density FLOOR (READ-ONLY), the style-guide, and the .api catalogs it composes — enumerate BOTH tiers per ' +
  'the CITATION TIER law (a core page mines the SHARED substrate; a domain/numerics shard mines the substrate PLUS its declared package cluster ' +
  '— STACK them; VERIFY every cited member, delete phantoms). Construct in BOUNDARY_ADMISSION lifecycle order; collapse parallel shapes into ONE ' +
  'dense closed-family ADT chosen by OWNER_CHOOSER; express every cross-cutting concern as a stacked signature+rail-preserving decorator over a ' +
  'thin pure core (FULL AOP); parameterize fully, one polymorphic entrypoint per modality; py3.15-modern only. Make the exemplary snippet AGNOSTIC ' +
  '(neutral names, no domain noun), compiling, ~3-4x denser than ordinary code, showing one owner ready to replace 10+ loose things with the ' +
  'growth axis visible. A domain/numerics shard COMPOSES the finalized core laws as settled material and never re-opens them. Cut every ' +
  'table-stakes card/snippet; cut every loose 1-2-field type and constant-spam cluster by collapsing it into one owner. Apply the page-craft ' +
  'grammar (narrow index table -> deep cards -> one snippet per region) and the style/comment hygiene. Report what you collapsed (count ' +
  'before->after) in `collapsed`, what capability you extended (each addition + its cited source) in `extended`, and this page`s spotlight snippet ' +
  'REGIONS in `regions`. verdict is `rebuilt` unless the page genuinely survived untouched. Return residual_high — each a {files:[every ' +
  'repo-relative path the cross-file fix spans], claim} for any CROSS-FILE item you cannot fix from this one file.'].join('\n')
const critiquePrompt = (page) => [DOCTRINE, '',
  'TASK: HOSTILE DOCTRINAL-CONFORMANCE AUDIT + FIX IN PLACE of ' + page + '. You are an ULTRA-HARSH, UNAGREEABLE auditor: assume a violation ' +
    'exists in EVERY fence until you prove otherwise, trust NOTHING the author or the prose claims, and "good enough"/"mature" is rejected ' +
    'outright. Read the page, the README doctrine sections, its sibling pages, the csharp reference, the style-guide, and the .api catalogs it ' +
    'composes. Run these MECHANICAL checklists line-by-line and REPAIR every hit in place (a fix, never a ledger note); the checklists are a ' +
    'FLOOR you hunt past, never the boundary of the audit:',
  '(1) COLLAPSE_SCAN — apply the move for any of the README [03] 12 signals (3+ instances mandatory): sibling prefix/suffix -> one ' +
    'modality-polymorphic entrypoint; same rail differing by arity -> input-shape discrimination; a get/get_many/get_by family -> one input-keyed ' +
    'entrypoint; functions differing only by a literal -> parameterize as a POLICY_VALUE; a bool selecting two bodies -> one derived body/policy ' +
    'value; a function calling exactly one other -> delete the hop; a class exposing one public method -> module function/fold; parallel dispatch ' +
    'arms -> a `frozendict` table/fold; several types sharing fields -> one closed family; 3+ sibling constants -> one `frozendict`/`StrEnum`; a ' +
    'package-rename wrapper -> use the surface directly; 2-4 recurring wrappers -> one aspect factory. These 12 signals are a FLOOR — hunt ' +
    'collapse targets past them.',
  '(2) OWNER_CHOOSER — re-derive EVERY shape from the 5 discriminants and replace any non-discriminant-correct owner; KILL every parallel DTO, ' +
    'one-field wrapper, field-rename class, tag-only shape, and `None`-as-failure. (3) KNOB_TEST — delete each parameter; collapse any ' +
    '`strict`/`mode`/`batch` flag into a policy value or input-shape discriminant; move every `timeout`/`retry`/`deadline` off the signature into ' +
    'an aspect or `anyio` scope.',
  '(4) ASPECTS / AOP — every cross-cutting concern is a signature+rail-preserving STACKED decorator over a pure core that never raises into domain ' +
    'flow; 2-4 co-occurring wrappers collapse into one aspect factory; `beartype`/`msgspec` integrated; inline-repeated concerns and sibling ' +
    'helpers are defects. (5) RAILS — narrowest carrier chosen once; the fault type is a CLOSED `Literal`/`StrEnum`/`@tagged_union` (a bare `str` ' +
    'fault for a multi-cause domain is a defect); accumulate-vs-abort correct (`map2`/fold for independents, `bind` for dependents); NO `asyncio`, ' +
    'NO hand-rolled retry loop, NO `None`-as-failure, NO exception control flow in domain logic.',
  '(6) PY315 / PAYLOADS / FROZENDICT / PEP — closed `TypedDict` via module-level `TypeAdapter` with `Unpack` at root entrypoints; `frozendict` ' +
    'owns tables/evidence (no `MappingProxyType`/dict-table/tuple-pairs/`dict[str,Any]`); PEP 585/604/695 only, no `from __future__`, no legacy ' +
    'typing; total `match` + `assert_never`. (7) CITATION TIER — a CORE page cites ONLY the shared substrate; a DOMAIN/numerics shard cites the ' +
    'substrate PLUS its declared package cluster; EVERY cited member verified against the actual catalog (delete phantoms); the full advanced ' +
    'surface is STACKED, not a thin subset.',
  '(8) AGNOSTIC SNIPPET LAW — every snippet compiles, uses neutral structural names, placeholders only inside literals, NO domain/repo/host noun; ' +
    'the snippet shows the form at large-system scale with the growth axis visible. (9) PAGE GRAMMAR + CARD ECONOMY — narrow index table -> deep ' +
    'cards -> one snippet per region -> page ends at last card; card fields earned (delete a field that decides nothing); near-peer cards merged; ' +
    'every `Use` names what it deletes. (10) ALTITUDE / NO RE-TEACH — prose re-teaching a mechanic owned by a finalized prior page (or, for a ' +
    'shard, a core law) is repaired by routing to the owner.',
  '(11) ZERO META + STYLE + COMMENTS — no provenance/process/release/tool context (a stale `capture-pending`/`research` block is deleted); ' +
    'style-guide prose law applied; all code backticked; comment hygiene clean. (12) CAPABILITY-COMPLETENESS + ILLUSION — verify the body ' +
    'implements what names/prose promise; close any capability the substrate/cluster surface, the real concept, or a consumer contract admits that ' +
    'the owner OMITS (a case/row/field/operation), citing its source; reject speculative padding, decorative ceremony, and any TABLE-STAKES ' +
    'card/snippet (delete or densify it).',
  'EDIT the page to fix every hit. Report what you extended in `extended` and the page`s snippet REGIONS in `regions`. Return residual_high — each ' +
    'a {files:[...], claim} for any CROSS-FILE item you cannot fix here.'].join('\n')
const redteamPrompt = (page) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of ' + page + '. You are the LAST and MOST AGGRESSIVE pass: assume the author and critique ' +
    'missed things and that the chosen design is naive or illusory until PROVEN the strongest, the burden of proof ON THE PAGE. Red-team is ' +
    'critique AND MORE. Open the .api catalogs it composes, the sibling pages, the README doctrine, the csharp reference, and the style-guide. ' +
    'Attack from every direction and REPAIR every defect in place — no soft-pedalling, a fix never a ledger.',
  'PRIMARY LENSES, multi-faceted: (A) COUNTERFACTUAL on the page`s core TEACHING shape — is the owner, the algebra (`fold`/derived `frozendict` ' +
    'table), and the dispatch form categorically the strongest the doctrine admits for THIS concept, or does a denser owner or a DEEPER ' +
    'substrate/cluster primitive collapse the whole fence? Rebuild to it; never defend the incumbent. (B) ANTICIPATORY_COLLAPSE — compute the DIFF ' +
    'OF THE NEXT case/dimension/modality: does it land as ONE declaration with every consumer untouched or broken loudly at type-check? If it ' +
    'would touch multiple sites, reshape so the growth axis is a case/row/policy value, and the snippet SHOWS one owner ready to replace 10+ loose ' +
    'things.',
  '(C) CORPUS-WIDE DUPLICATION — attack this page`s snippet regions against the finalized priors: any snippet re-demonstrating a region another ' +
    'page owns is ROUTED to its owner (composed as supporting material), never re-taught. (D) AOP + SHAPE-BUDGET MAXIMIZATION — counterfactually ' +
    'attack how much MORE functionality could be expressed as stacked decorators over a thinner pure core, and whether any loose type/constant ' +
    'cluster could collapse into one closed family; push PAST the critique`s bar. (E) ALTITUDE + SUBSTRATE-DEPTH + PHANTOMS + CITATION-TIER — a ' +
    'mechanic owned by a finalized prior or a core law (route it); flat code below substrate/cluster operator depth (collapse to package depth); a ' +
    'phantom .api member (delete it); a page reaching the wrong .api tier (route out). (F) CAPABILITY-COMPLETENESS + LONG-TAIL + ILLUSION + ' +
    'TABLE-STAKES — name an omitted capability, edge case, or failure-mode row with a cite and extend the owner in place; delete any ' +
    'table-stakes/decorative/speculative card or snippet.',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (every time): re-attack every critique dimension (1-12) with fresh hostile eyes, trusting nothing the ' +
    'prior passes claimed. Even absent a structural rebuild the page must end objectively denser, MORE capable, more agnostic-compliant, and more ' +
    'opinionated than the critique left it; if the strongest form is genuinely already present, prove it by finding nothing — never invent churn. ' +
    'Report what you extended in `extended` and the page`s REGIONS in `regions`. Return residual_high — each a {files:[...], claim} for a ' +
    'CROSS-FILE item you cannot fix from one file.'].join('\n')
const sweepPrompt = (page, ledger) => [DOCTRINE, '',
  'TASK: SEQUENTIAL CORPUS-INTEGRATION SWEEP + FIX IN PLACE of ' + page + '. The prior pages in atlas order (core, then domain/ and numerics/ ' +
    'shards) are now FINALIZED. Your job is to make this page integrate them IMPLICITLY and own a disjoint region of the corpus. Read THIS page ' +
    'from disk in full; you are given the REGION LEDGER of every prior finalized page as data (NOT their bodies) — read a prior page`s full body ' +
    'ONLY when the ledger flags a candidate collision with a region you demonstrate.',
  'ENFORCE: (1) ALTITUDE ROUTING — if this page re-teaches a mechanic the ledger shows owned by an earlier finalized page (or, for a shard, a core ' +
    'law), repair by ROUTING to the owner (compose it as settled supporting material, delete the re-teach); vocabulary/owners/rails from earlier ' +
    'pages arrive settled and are never re-explained. (2) NO DUPLICATE SNIPPET — if a snippet here exercises a region the ledger marks owned ' +
    'upstream, DEMOTE it to supporting material and pick an UNOWNED region for the spotlight, or delete it if it adds nothing new. (3) DENSITY + ' +
    'OPINIONATED + PAGE-CRAFT — the page must end objectively denser, fully agnostic, table-stakes-free, within the soft ~450 LOC density signal ' +
    '(never split a coherent concept or strip snippet whitespace to hit it).',
  'REGION LEDGER (finalized priors):\n' + JSON.stringify(ledger, null, 1) + '\nEDIT this page to fix every hit. Return your page`s UPDATED ' +
    '`owned_regions` (the spotlight regions it now owns after routing), `rerouted` (regions you demoted/removed), verdict `routed` if you changed ' +
    'it else `clean`, and residual_high for any CROSS-FILE item you cannot fix here.'].join('\n')
const STAGES = [
  { key: 'rebuild', build: authorPrompt, effort: 'max' },
  { key: 'crit', build: critiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: redteamPrompt, effort: 'max' },
]
const processPage = async (page) => {
  const logs = {}
  for (const st of STAGES) {
    const r = await agent(st.build(page), { label: st.key + ':' + nameOf(page), phase: 'Harden', schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: STALL })
    if (r === null) break
    logs[st.key] = r
  }
  return { page, logs, ok: Object.keys(logs).length === STAGES.length }
}
const regionsOf = (logs) => { for (const st of ['redteam', 'crit', 'rebuild']) { const l = logs[st]; if (l && l.regions && l.regions.length) return l.regions } return [] }

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Inventory')
const inv = await agent('DISCOVERY — read-only is the ONLY concession of this stage; depth is not. Read ' + ROOT + '/README.md IN FULL and parse ' +
  'the [01]-[ATLAS] table, THEN recurse into every sub-folder README router it references (e.g. ' + ROOT + '/domain/README.md, ' + ROOT +
  '/numerics/README.md if present). Resolve scope against REAL disk state, never memory: run a real fd/find listing of every .md under ' + ROOT +
  ' AND a real ls of BOTH .api catalog tiers (shared substrate + package clusters; root ' + ROOT + '/.api/ when present, else libs/python/.api/); ' +
  'routers own ORDER, disk owns EXISTENCE. Read EVERY concept page IN FULL — no skimming, no memory-recalled inventory — plus the folder at ' +
  'large. Return the full ordered file set: every CONCEPT page that exists on disk under ' + ROOT + ' (the core pages first in atlas order, then ' +
  'each sub-folder`s pages in its router order), EXCLUDING every README.md AND algorithms.md (slated for numerics/ retirement — skip it). Each ' +
  'row {path (repo-relative, e.g. ' + ROOT + '/shapes.md), order (global integer), folder (the sub-folder name or empty for core), regions (its ' +
  'current snippet-demonstration region tags, one short tag per major snippet), map (a per-page reading MAP, never a bare verdict: composed ' +
  'capability; underutilized admitted capability with CONCRETE members read from the actual catalog files — verified members ONLY, a member you ' +
  'cannot find there is a phantom and is NEVER listed; contextual seams to sibling pages; stacking guidance; a hostile weak/strong call)}. The ' +
  'map is an INITIAL POINTER for downstream stages, never a ceiling — it licenses NO downstream skim. Use fd/ls/read; do not cd; do not edit ' +
  'anything.', { label: 'inventory', phase: 'Inventory', schema: INVENTORY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const ordered = ((inv && inv.files) || []).filter((f) => f && f.path).sort((a, b) => a.order - b.order).map((f) => f.path)
log('Inventory: ' + ordered.length + ' python doctrine pages (core + domain + numerics) to harden')

// --- [HARDEN]
phase('Harden')
const done = (await pool(ordered, CAP, (page) => processPage(page))).filter(Boolean)

// --- [SWEEP]
phase('Sweep')
const ledger = []
const sweepLogs = []
const seedRegions = (p) => { const d = done.find((r) => r.page === p); if (d) { const rr = regionsOf(d.logs); if (rr.length) return rr } const iv = (inv && inv.files || []).find((f) => f.path === p); return (iv && iv.regions) || [] }
for (let i = 0; i < ordered.length; i++) {
  const page = ordered[i]
  const r = await agent(sweepPrompt(page, ledger), { label: 'sweep:' + nameOf(page), phase: 'Sweep', schema: SWEEP_SCHEMA, effort: 'xhigh', stallMs: STALL })
  const regions = (r && r.owned_regions && r.owned_regions.length) ? r.owned_regions : seedRegions(page)
  ledger.push({ file: page, owned_regions: regions })
  if (r) sweepLogs.push(r)
}
log('Sweep: ' + sweepLogs.length + '/' + ordered.length + ' pages integrated in atlas order')

const norm = (x, page) => typeof x === 'string' ? { files: [page], claim: x } : { files: x.files && x.files.length ? x.files : [page], claim: x.claim }
const allRes = []
for (const r of done) for (const st of ['rebuild', 'crit', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, r.page)) }
for (const r of sweepLogs) if (r.residual_high) for (const x of r.residual_high) allRes.push(norm(x, r.file))
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Harden+Sweep done; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent([DOCTRINE, '', 'TASK: RECONCILE these cross-FILE residuals the per-page + sweep passes deferred. There is NO severity ' +
      '— treat EVERY residual as must-address. Read EVERY listed file. For each: if it is a real cross-file defect, FIX it in place (unify the ' +
      'shared owner/rail/region, repair the altitude/duplication issue, or extend the shared owner to close a gap that spans files), preserving ' +
      'all capability and regressing no file; if a residual is FACTUALLY INCORRECT, leave it and say why. Edit ONLY under ' + ROOT + '/. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: STALL })
    if (!fix) return null
    const verify = await agent([LAW, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL WRITING VERIFY of the reconcile fixes — never a friendly ' +
      'confirmation, never read-only. For EVERY claim: (a) RE-DERIVE necessity — decide from the files themselves whether the claimed defect was ' +
      'real and a fix warranted at all, trusting nothing the fixer reported; (b) PROVE ON DISK the fix was done properly — read every named file ' +
      'in full and check the landed edit against the doctrine, never against the fixer summary; (c) where the landed fix is loose, weak, or a ' +
      'token single-point patch and a root-level dense reconstruction of the same files is available, REPAIR it NOW to that objectively strongest ' +
      'form (a point patch where a root rebuild is available is itself a defect you fix) and record every file you edited in `repaired`; (d) only ' +
      'THEN classify each claim: "fixed" (real defect, now genuinely resolved — by the fixer or by your own repair), "invalid" (the claim is ' +
      'factually wrong — cite why), or "open" (real defect genuinely unreachable from the files at hand — NEVER a punt on a strengthenable fix; ' +
      'put the blocking reason in `evidence`). Edit ONLY under ' + ROOT + '/. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer ' +
      'touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: STALL })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const settled = new Set(reconciled.filter((r) => r.verify).flatMap((r) => r.cluster.map((c) => c.claim)))  // fix or verify died -> claims stay live
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const hard_residual = uniq.filter((r) => openClaims.has(r.claim) || !settled.has(r.claim))
log('Reconcile: ' + clusters.length + ' cluster(s); ' + hard_residual.length + ' open hard residual(s) -> resolve-residuals')
return { workflow: 'stack-py', root: ROOT, ordered: ordered, hardened: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, total: ordered.length, region_seed: ledger, clusters: clusters.length, hard_residual: hard_residual }
