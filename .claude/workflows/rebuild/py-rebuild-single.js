export const meta = {
  name: 'py-rebuild-single',
  description: 'Granular hostile ground-up rebuild of libs/python design pages to world-class py3.15 (ADT collapse, AOP, unified rails, full both-tier .api-stacking) AND justified IN-PLACE capability extension. TARGETS are granular: a package root (libs/python/artifacts), one or more sub-folders at ANY depth (libs/python/artifacts/.planning/graphic/marks), or specific files (any number) — passed as a string, an array, or {targets:[...]}. Per TARGETED design page, 1 agent per file in a 3-step ADVERSARIAL pipeline — rebuild(max) -> critique(xhigh) -> redteam(max), every stage hostile: assume the fence is naive/junior/illusory until it survives attack, never accept "mature", hunt the fake/decorative code that reads advanced but is hollow, collapse + stack both .api tiers, AND close the concept capability gaps by growing the existing owner in place. Then a FOLDER-WIDE reconcile: residual union-find fix/verify (blast radius = the owning folder, not just the targeted files) PLUS a per-package sibling-seam drift sweep so the whole folder stays coherent even where the targeted rebuild did not reach. The whole-folder collapse/unification pass is OWNED BY py-rebuild-many (run it scoped to one folder). args = a target path, an array of target paths, or {targets:[...]}; empty = no-op.',
  phases: [
    { title: 'Discover', detail: 'resolve the targets (file / sub-folder / package, any number) into the targeted page set + owning packages + the folder-wide page set' },
    { title: 'Rebuild', detail: 'per TARGETED page (1 agent/file): rebuild(max) -> critique(xhigh, 6-checklist + capability-completeness) -> redteam(max, counterfactual + cold re-review), every stage ADVERSARIAL (naive/illusory-by-default) and every stage first LISTS then FULLY reads the entire docs/stacks/python/ doctrine in README routing order, pooled at CAP' },
    { title: 'Reconcile', detail: 'FOLDER-WIDE: union-find cluster cross-file residuals -> fix(max) -> adversarial verify(xhigh) with the owning folder as blast radius; then a per-package sibling-seam drift sweep so the whole folder stays coherent even outside the targeted set; hard residuals hand off to resolve-residuals' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
// for a gradual roll-out; each stage is a journaled agent() call, so a paused or usage-limited run resumes from the journal
const STAGGER_MS = 1500
const ROOT = 'libs/python'

// --- [INPUTS] ----------------------------------------------------------------------------

const normTarget = (t) => { const s = String(t).trim().replace(/\/+$/, ''); return (s === ROOT || s.indexOf(ROOT + '/') === 0) ? s : ROOT + '/' + s.replace(/^\/+/, '') }
const rawTargets = Array.isArray(args) ? args
  : (args && typeof args === 'object' && Array.isArray(args.targets)) ? args.targets
  : (args && typeof args === 'object' && args.target) ? [args.target]
  : (typeof args === 'string' && args.trim()) ? [args]
  : []
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))]

// --- [MODELS] ----------------------------------------------------------------------------

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['packages', 'rebuildPages', 'folderPages'], properties: { packages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'planning', 'api', 'root'], properties: { name: { type: 'string' }, planning: { type: 'string' }, api: { type: 'string' }, root: { type: 'string' }, note: { type: 'string' } } } }, rebuildPages: { type: 'array', items: { type: 'string' } }, folderPages: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SEAM_SCHEMA = { type: 'object', additionalProperties: false, required: ['package', 'verdict', 'summary'], properties: { package: { type: 'string' }, verdict: { type: 'string', enum: ['repaired', 'clean'] }, repaired: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'Rasm monorepo, libs/python planning corpus (markdown specs of intended Python module designs). CLAUDE.md manifest law governs. DENSITY BAR: ' +
    'docs/stacks/python/ (README/language/shapes/surfaces-and-dispatch/rails-and-effects/algorithms/type-system/boundaries/runtime/system-apis) — ' +
    'author Python as dense, polymorphic, and rich as that bar. Cite ONLY members confirmed in the .api catalogs — and MINE BOTH TIERS fully: the ' +
    'shared/universal branch catalogs `libs/python/.api/*.md` AND the folder-specific `libs/python/<folder>/.api/*.md`. Maximize the ' +
    'shared/universal catalogs wherever relevant, never only the folder set.',
  'This is a FUNDAMENTAL GROUND-UP REBUILD of a planning-stage DESIGN PAGE, not a polish pass. Improve the page objectively: collapse ' +
    'surfaces/types, deepen bleeding-edge spellings, maximize admitted-library capability, AND close the concept capability gaps.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE items ' +
    '(report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, AND red-team) is HOSTILE: assume the existing fence is NAIVE, SHALLOW, JUNIOR, or ILLUSORY ' +
    'until it survives an aggressive attack; the burden of proof is ON THE CODE, never on you. "Mature", "already strong", "good enough", "done", ' +
    'and a prior `clean` verdict are REJECTED self-assessments — MOST of this corpus is naive, surface-level, old-style Python dressed in the ' +
    'right vocabulary, and it is NOT tolerable. Default to "this fence is naive and must be rebuilt to the strongest form the doctrine admits" and ' +
    'MAKE that rebuild; a no-edit verdict is reached ONLY after a genuinely aggressive attack on the real domain + the verified both-tier package ' +
    'surface finds nothing — never a first-read concession, never to avoid work. Reject "good enough" categorically.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code is the code that PRETENDS to be advanced: it uses the doctrine vocabulary ' +
    '(`@tagged_union`/`frozendict`/`Result`/`Option`/the rails), cites packages, reads dense and confident — yet is HOLLOW. Treat dense, ' +
    'confident-looking fences with MORE suspicion, not less, and DISBELIEVE every claim the page makes about itself until you verify it against ' +
    'the real domain and the catalogued both-tier package surface. HUNT: a name/signature/prose that PROMISES capability the body does not ' +
    'implement; a "rich" owner that is a thin slice of its concept (a 2-variant union for a 20-case domain; the obvious 3 keys where the concept ' +
    'carries fifteen); decorative density and ceremony carrying no real capability; a placeholder/stub/sketch dressed as a finished design; prose ' +
    'that ASSERTS richness the fence does not contain; a structurally-correct collapse that is semantically empty; a `.api` member cited but never ' +
    'verified (a phantom). Every such illusion is a DEFECT to rebuild, not a feature to preserve; never invent churn to look busy either.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — MANDATORY FULL READ BEFORE ANY EDIT: docs/stacks/python/ is the route-owned law, and you hold exactly ONE target file with ' +
    'ample context budget, so there is NO excuse for a partial read. STEP 0 — LIST THE DIRECTORY FIRST: enumerate `docs/stacks/python/` AND ' +
    '`docs/stacks/python/.api/` with a real `ls`/`find` to obtain the COMPLETE doctrine + catalog inventory before reading, so not one page is ' +
    'silently skipped and the routing order is taken from the source of truth, never from memory. THEN READ EVERY doctrine page IN FULL — ' +
    'top-to-bottom, every section, every family card, every snippet, never a partial, skim, grep-jump, or section-sample — STARTING AT THE README ' +
    'and proceeding in its `[01]-[ATLAS]` routing order: (1) `docs/stacks/python/README.md` (the 16 laws in 5 groups + the 12-signal COLLAPSE_SCAN ' +
    '+ RULE_ENFORCEMENT + PAGE_CRAFT + CORPUS_LAW), then strictly in routing order (2) `docs/stacks/python/language.md`, (3) ' +
    '`docs/stacks/python/shapes.md` (the lifecycle + OWNER_CHOOSER + the closed-family/absence/variant law), (4) ' +
    '`docs/stacks/python/iteration.md` (the pure carrier-free computation algebra — seed folds, lazy itertools/generator pipelines, ' +
    'streaming-over-eager-materialization, structural recursion at depth), (5) `docs/stacks/python/surfaces-and-dispatch.md` (dispatch forms + ' +
    'ASPECTS), (6) `docs/stacks/python/rails-and-effects.md` (rail/effect/fault + the RECEIPT single-fact law), (7) ' +
    '`docs/stacks/python/concurrency.md` (the anyio structured-concurrency rail + the to_thread/to_interpreter/to_process CPU-offload arms under ' +
    'CapacityLimiter), (8) `docs/stacks/python/boundaries.md` (host/wire boundary, FFI capsule lifetime + deterministic close, identity-regime ' +
    'law), (9) `docs/stacks/python/algorithms.md` (numeric approach), (10) `docs/stacks/python/system-apis.md` (stdlib-replacement law), (11) ' +
    '`docs/stacks/python/runtime.md` (interpreter execution + isolation, the lazy-import runtime surface). READ EVERY ROOT `*.md` THE STEP-0 `ls` ' +
    'RETURNS AND FULLY INTERNALIZE IT BEFORE ANY EDIT — this enumeration is the floor, not the ceiling: a root page present on disk but absent from ' +
    'this list is STILL mandatory law, while the sub-folders `domain/` and `numerics/` are OUT of this read. The ' +
    'README `[STATE]` column marks each page finalized (binding law) or partial (operative-but-unfinalized context); read EVERY page regardless of ' +
    'state and hold every fence to them as fact — a partial, sampled, or skipped read of this doctrine is itself a process defect, not an ' +
    'efficiency. docs/stacks/csharp/ is the density/ambition FLOOR — match its richness, never import C#-shaped idioms.',
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
  'STACK .api CAPABILITY (load-bearing): FIRST inventory the COMPLETE catalog set available to this page — BOTH the shared/universal branch ' +
    'catalogs at `libs/python/.api/*.md` (anyio, expression, msgspec, pydantic, pydantic-settings, beartype, structlog, stamina, numpy, psutil, ' +
    'opentelemetry-*, protobuf, grpcio) AND every folder-specific catalog at `libs/python/<folder>/.api/*.md` — then mine them for the full ' +
    'ADVANCED surface of each package (combinators, hooks, native pipelines, discriminators, async mirrors) and how packages STACK. List BOTH ' +
    '`.api/` tier DIRECTORIES in full and DIFF that complete inventory against what the page already cites: every admitted catalog whose domain ' +
    'the page admits but does NOT yet use is an ADOPTION TARGET — adopt it to depth here, or when it belongs on a sibling page surface it as a ' +
    'residual; a relevant admitted catalog left unadopted is a DEFECT, not an omission. There is NO fixed library count: compose EVERY relevant ' +
    'admitted library into single dense operations woven as ONE rail, and ALWAYS layer the shared/universal rails (expression `Result`/`Option`, ' +
    'msgspec/pydantic discriminated models, beartype validation, stamina retry, structlog + opentelemetry spans, anyio structured concurrency) ON ' +
    'TOP OF the folder-specific domain packages — NOT flat one-shot per-library uses. Use the DEEPEST primitive each package itself reaches for ' +
    '(LIBRARY_DEPTH) — flat code below the operator depth the admitted packages reach is surface sprawl in time; reject surface-level ' +
    'single-feature subsets and any thin rename wrapper.',
  'PRESERVE all capability (densify, never delete functionality). Where a page is already dense, refine; where it is flat/naive, rebuild ' +
    'ground-up. Never regress correctness or boundary law.',
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
    'surface IS new functionality woven into the owner, not a denser spelling of the same call; verify the member exists). (b) DOMAIN — an ' +
    'attribute, metric, sub-kind, relationship, state, or operation the REAL concept demands but the page omits (a mesh owns its ' +
    'topology/normals/UV/attributes and validate/repair/query operations, not a vertex array alone; a data schema owns ' +
    'constraints/indexes/partitions/coverage, not naive columns; a geometry owner owns the full predicate/transform/measure family the domain ' +
    'needs). (c) CONSUMER — a contract a sibling or downstream owner will require that has no composed spelling here yet (a need with no spelling ' +
    'marks a missing case: the law extends first, the feature lands second).',
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
const MECHANICS = [
  'MECHANICAL EXECUTABILITY (the burden the collapse + capability laws do NOT carry) — a design-page fence is a SIGNATURE-AND-IMPLEMENTATION ' +
    'CONTRACT, never a sketch: every fence MUST parse under the active py3.15 surface AND type-check against the REAL cross-page canonical owners it ' +
    'imports, because the corpus is ONE body (CORPUS_LAW three-layer inheritance) and a fence reading a field/case/attribute a sibling owner does ' +
    'not declare is a runtime DEFECT, not a design liberty. Mentally COMPILE and TYPE-CHECK each fence before accepting it — structural collapse that ' +
    'does not execute is illusory density, the prime suspect this stance hunts. Find each class below BY NAME and FIX it in place by growing the ' +
    'EXISTING owner (a case/field/operation/row), never a new file:',
  'FENCE-PARSES (language.md CLOSED_MATCH_SITE; README snippet law) — every `match`/structural pattern, `for`-target, comprehension, and t-string ' +
    'parses: an OR-pattern whose alternatives bind DIFFERENT names (`A(x) | B()` where the wildcard slot is a capture, not the `_` the law names), ' +
    'an invalid iterable-unpacking or starred `for`/return target, or a malformed pattern is a NON-COMPILING fence and an automatic rebuild target.',
  'MODEL-COHERENCE (CORPUS_LAW) — every attribute, field, case tag, method, and imported symbol a fence reads off a canonical owner declared on ' +
    'ANOTHER page (or earlier in this one) MUST exist on the real declaration of that owner: verify each cross-owner read against the sibling owner ' +
    'before writing it (a `run.rtl` read against a `RunNode` that declares `direction` is model drift — a runtime `AttributeError`; an import of a ' +
    'symbol a sibling collapsed or renamed is an `ImportError`; a call passing a kwarg the owner does not accept is a `TypeError`), reconcile to the ' +
    'ONE canonical name, never invent a field the owner does not carry, and surface an un-reconcilable cross-page name as a residual.',
  'TOTAL-DISPATCH (language.md CLOSED_MATCH_SITE; shapes.md families) — `assert_never(unreachable)` is an exhaustiveness WITNESS, valid ONLY when ' +
    'every member of the FULL closed family is handled before it: enumerate the complete case set of the owner and prove NO valid case routes to ' +
    '`assert_never` (a `match` covering 5 of 8 valid cases that `assert_never`s the rest is a reachable trap, not a totality proof); a parallel ' +
    'dispatch map keyed by a closed family must be TOTAL over it (a missing key is a runtime `KeyError`). A partial `match`/map dressed as total is a DEFECT.',
  'SINGLE-FACT EVIDENCE (rails-and-effects.md STATE_RECEIPTS; boundaries.md BYTE_IDENTITY) — the bytes, the content key, and the receipt evidence ' +
    'derive from ONE computed fact stored once on the stepped owner: the producer computes the fact, and the receipt/contribute path READS the ' +
    'stored fact, never re-renders. A path that recomputes a render/placement/native-mutation a second time to mint receipt evidence is a ' +
    'DOUBLE-RENDER defect, and a placeholder/empty-byte key for a real arm is an evidence hole — step the owner with one fact, store it, harvest it.',
  'LOOP-OFFLOAD (concurrency.md OFFLOAD_LANE + SCOPE_CHOOSER; runtime.md) — synchronous CPU-bound or GIL-hostile provider work (rendering, parsing, ' +
    'native FFI sweeps) NEVER runs on the event loop, NOR as an argument expression evaluated before the offload call: it crosses on exactly one arm ' +
    '— `anyio.to_thread.run_sync` for a GIL-releasing native call or blocking I/O, `to_interpreter.run_sync` for pure-Python isolate-safe CPU work, ' +
    '`to_process.run_sync` for a GIL-hostile or not-isolate-safe native call — each bounded by an explicit `CapacityLimiter`. A heavy synchronous ' +
    'call inside an async body (or passed as the arg to the offload) is an event-loop-starvation DEFECT.',
  'HANDLE-LIFETIME (boundaries.md CAPSULE_OWNER; concurrency.md RESOURCE_BRACKET) — every native/FFI handle a provider opens (a `*.open(...)` ' +
    'returning a C-backed document, plotter, cursor, or pinned buffer) closes DETERMINISTICALLY through an `AsyncExitStack.enter_async_context`, a ' +
    '`with` bracket, or a capsule registering release via `weakref.finalize` under a shielded teardown — never left for the GC to reap. An opened ' +
    'handle with no deterministic close is a LEAK defect; callers receive detached values or rails, never the live handle.',
  'BINARY-KERNEL (boundaries.md CAPSULE_OWNER; EXPRESSION_SPINE exemption) — a multi-megabyte binary mutated across N steps is ONE imperative ' +
    'measured kernel threading ONE owned handle mutated in place, NOT a functional fold that rebinds and recopies the whole buffer per step (an ' +
    'O(N*size) copy the platform makes prohibitive); the kernel lives inside the shielded resource bracket, returns the rail `Result`, and carries ' +
    'one `# Exemption:` line naming the platform-forced in-place-mutation seam. The per-step buffer recopy is the rejected form.',
  'IDENTITY-REGIME (boundaries.md MEMO_KEY; shapes.md OWNER_CHOOSER identity discriminant) — a content-addressed key indexes by CONTENT, so two ' +
    'structurally-distinct siblings carrying identical content collide and silently overwrite in a `Map[ContentKey, _]`. Where an index/diff must ' +
    'distinguish identical-content siblings, the key joins a STRUCTURAL discriminant (a path-vector, sibling ordinal, or owner identity) to the ' +
    'content digest — structural identity and content identity are distinct contracts, and a content-only key under a structural index is a CORRUPTION defect.',
  'TEMPLATE-SAFETY (language.md TEXT_AND_TEMPLATE_FORMS + TEMPLATE_STRUCTURE_SITE; system-apis.md) — structured-text and markup egress (SVG, XML, ' +
    'Typst, HTML, query strings) built from dynamic or untrusted input uses PEP 750 t-strings / `string.templatelib.Template` processors or a ' +
    'structured builder (`xml.etree.ElementTree`), NEVER f-string interpolation with a hand-rolled escape. An f-string splicing a value into markup ' +
    'is an INJECTION defect; the Template/processor carries the per-destination escaping the grammar requires.',
  'STREAM-OVER-MATERIALIZE (iteration.md LAZY_COMBINATORS + GENERATOR_FUSION) — a large or unbounded extraction (every word of a 500-page document, ' +
    'every node of a corpus tree) is a lazy `itertools`/generator pipeline or a `yield from` fusion typed `Iterator[T]`, never an eagerly allocated ' +
    '`tuple`/`Block` of the whole result held in RAM and materialized only at the persistence/egress edge; the eager collection minted only to be ' +
    'iterated once is the rejected materialization.',
  'NO-EXCEPTION-HOTLOOP (rails-and-effects.md EXPRESSION_SPINE) — a per-element `try`/`except` driving control flow inside a fold over a large ' +
    'collection (a per-cell coerce over tens of thousands of cells) is BOTH a domain-logic violation AND a throughput defect: a total predicate or a ' +
    'non-raising `Option`-returning parse replaces the per-element raise, and the boundary `catch` trap stays at the boundary, never in the hot fold.',
  'DERIVED-NOT-PARALLEL + PER-MODE PAYLOADS (DERIVED_LOGIC; shapes.md OWNER_CHOOSER) — a secondary map hand-synced to a primary (a `_KEYS` ' +
    'tuple-table parallel to a `@tagged_union` case-payload tail, drift-caught only late by `zip(strict=True)`) is a DERIVATION defect: declare ONE ' +
    'primary correspondence and DERIVE every secondary by comprehension. A monolithic typed bag whose fields are irrelevant for most modes (one ' +
    '`Spec` carrying the fields of every backend) is a permissive-bag DEFECT even when fully typed: collapse it into a discriminated per-mode ' +
    '`@tagged_union` whose each case carries ONLY the fields of its own mode — WITHOUT splitting the owner into new files.',
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
    '`match`/structural pattern matching over if-chains, walrus where it tightens, and `assert_never` on closed unions ONLY where it is genuinely ' +
    'unreachable over the FULL case set; PEP 750 t-strings / `string.templatelib.Template` are MANDATORY for all dynamic or untrusted ' +
    'structured-text and markup egress (never f-string interpolation of dynamic input), and PEP 749 deferred annotations apply where relevant. Keep ' +
    'every choice CONSISTENT across folders so the corpus reads as one ultra-advanced codebase.',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package/folder owner strictly in its lane; internal code uses canonical names and shapes with mapping ' +
  'only at the edge; do not trample a sibling owner while densifying; respect the dependency direction of the workspace strata.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
    'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
    'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing ' +
    'contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. Prose that ' +
    'ASSERTS capability the fence does not implement is a defect, not content.',
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

// --- [OPERATIONS] ------------------------------------------------------------------------

const folderOf = (p) => { const head = p.split('/.planning/')[0].split('/'); return head[head.length - 1] || 'root' }
const subOf = (p) => p.split('/.planning/').pop()
const authorPrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', MECHANICS, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '', 'TASK: ' +
  'HOSTILE GROUND-UP REBUILD of ' + page + ' to the doctrine AND to domain-complete capability. DISBELIEVE the page — assume every fence is naive, ' +
  'junior, or illusory until proven world-class; do NOT polish what is there, REBUILD it to the strongest form the doctrine admits, and treat ' +
  'dense confident-looking code as a prime suspect for hollow/decorative complexity. Read the page, its sibling pages (cross-page unification), ' +
  'the operative docs/stacks/python/ pages, and BOTH .api tiers (mine the full advanced surface, ADD any shared/universal rail the page ' +
  'under-uses, STACK them; VERIFY every cited member — a member you cannot verify is a phantom to delete). Construct in LIFECYCLE order: admit raw ' +
  'ONCE into a typed `TypedDict`/Pydantic payload -> materialize into the canonical owner chosen by the OWNER_CHOOSER discriminants -> weave every ' +
  'cross-cutting concern (retry/telemetry/validation/contracts/memo/receipts) as a STACKED signature+rail-preserving aspect over a thin pure core ' +
  '-> compose the domain transform through ONE unified rail -> project + egress, with BOTH ingress and egress parameterized. Collapse parallel ' +
  'shapes into one closed family/ADT; drive cases with a derived `frozendict` table or fold; one polymorphic entrypoint per modality (`T | ' +
  'Iterable[T]` normalized once). BEYOND collapse + both-tier `.api` maximization, CLOSE THE CONCEPT CAPABILITY GAPS so the page OWNS ITS DOMAIN ' +
  'CONCEPT COMPLETELY: run your OWN aggressive domain + package sweep, find where the owner models a NAIVE/thin slice of its concept, and extend ' +
  'the EXISTING owner in place (a case/row/field/operation/policy-value per ANTICIPATORY_COLLAPSE + COMPOSED_IMPLEMENTATION), each addition citing ' +
  'a package member / domain attribute / consumer contract — never a parallel surface, a new file, or flat spam. py3.15-modern only (PEP ' +
  '585/604/695, `frozendict` builtin, newest payload forms). High-signal prose all-backticked. Fix-in-place (read-then-rebuild, preserve ' +
  'capability). Report what you collapsed (count before->after) in `collapsed` and what capability you extended (each addition + its cited source) ' +
  'in `extended`; verdict is `rebuilt` unless the fence genuinely survived the hostile rebuild untouched. Return the fix-log + residual_high — ' +
  'each a {files: [every repo-relative path the cross-file fix spans], claim} object for any CROSS-FILE item you surface but cannot fix from this ' +
  'one file (NO severity; the reconcile phase fixes all of them).'].join('\n')
const critiquePrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', MECHANICS, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '',
  'TASK: HOSTILE DOCTRINAL-CONFORMANCE AUDIT + CAPABILITY-COMPLETENESS + FIX IN PLACE of ' + page + '. You are an ULTRA-HARSH, UNAGREEABLE ' +
    'auditor: assume a violation exists in EVERY fence until you prove otherwise, trust NOTHING the author or the prose claims, and "good ' +
    'enough"/"mature" is rejected outright. Run these MECHANICAL checklists line-by-line and REPAIR every hit in place (a fix, never a ledger note):',
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
  '(6) PAYLOADS/FROZENDICT/PEP — payloads are `closed=`/`extra_items=` `TypedDict` via a module-level `TypeAdapter` with `Unpack[TypedDict]` at ' +
    'root entrypoints; `frozendict` (builtin) owns tables/evidence (no `MappingProxyType`/dict-table/tuple-pairs); PEP 585/604/695 only, no `from ' +
    '__future__ import annotations`, no legacy typing; total `match` + `assert_never`.',
  '(7) CAPABILITY-COMPLETENESS + ILLUSION — structural collapse and capability completeness are ORTHOGONAL: a fully-collapsed owner can still ' +
    'model a NAIVE, LIMITED slice of its concept, and dense confident code is the prime suspect for hollowness. DISBELIEVE the page about its own ' +
    'richness: verify the body actually implements what the names/prose promise; any capability the both-tier admitted-package surface / the real ' +
    'domain concept / a consumer contract admits that the owner OMITS (a flat id/member set where the concept owns ' +
    'geometry/metrics/attributes/topology/operations; a 2-variant union where the domain has twenty; the obvious 3 keys where the concept carries ' +
    'fifteen) is a DEFECT — close it in place now by growing the EXISTING owner (case/row/field/operation), citing its source. Reject the inverse: ' +
    'a speculative/padding field, decorative ceremony, or prose asserting capability the fence lacks is deleted.',
  'Also enforce both-tier `.api` maximization (a thin folder-only subset ignoring the shared rails is a defect) and prose + comment hygiene. EDIT ' +
    'the page to fix every hit. Report what you extended in `extended`. Return the fix-log + residual_high — each a {files: [every repo-relative ' +
    'path the cross-file fix spans], claim} object for any CROSS-FILE item you cannot fix here (NO severity; the reconcile phase fixes all of them).'].join('\n')
const redteamPrompt = (page) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', MECHANICS, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of ' + page + '. You are the LAST and MOST AGGRESSIVE pass: assume the author and critique ' +
    'missed things and that the chosen design is naive or illusory until PROVEN the strongest, with the burden of proof on the design, never on ' +
    'you; trust nothing the prior passes or the prose claimed. Open BOTH .api tiers, the sibling pages, and the operative docs/stacks/python/ ' +
    'pages. Attack from every direction and REPAIR every defect in place — no soft-pedalling, no could/should, a fix never a ledger.',
  'PRIMARY LENS — fundamental design, multi-faceted / multi-dimensional / multi-directional: (A) COUNTERFACTUAL on the core choice — is the owner, ' +
    'the algebra, and the dispatch form categorically the strongest the doctrine admits, or does a denser owner, a fold/derived-table algebra, or ' +
    'a DEEPER admitted-package primitive collapse the whole fence? If a fundamentally stronger design exists, rebuild to it — never defend the ' +
    'incumbent. (B) ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: when the next case/dimension/knob/modality/provider arrives, ' +
    'does it land as ONE declaration with every consumer untouched (or broken loudly at type-check)? If it would touch multiple sites, reshape so ' +
    'the growth axis is a case, row, policy value, or carrier swap. (C) LONG-TAIL + MULTI-DIMENSIONAL — attack every input/output/edge/failure ' +
    'mode (empty, singular, plural, stream, malformed, concurrent, cancelled, partial-failure, version-skew); is the accumulate-vs-abort ' +
    'disposition correct for the REAL boundary; are BOTH ingress AND egress parameterized so this owner sources and sinks across hundreds of apps ' +
    'without interior edits? (D) BOUNDARY-INTEGRITY — a concern owned twice in a runtime, a folder mixing concerns, a concern scattered across ' +
    'folders, or any coupling to a sibling owner\'s INTERIOR (vs its wire/seam) is a defect: fix it, or record it as a cross-file residual. (E) ' +
    'SURFACE-SPRAWL-IN-TIME + PHANTOMS — an admitted package whose .api exposes capability the fence re-derives by hand, flat code below the ' +
    'operator depth the packages reach, a phantom .api member (cited but unverifiable — delete it), or a thin wrapper: collapse to package depth ' +
    'and verify the member exists. (F) CAPABILITY-COMPLETENESS + ILLUSION — counterfactually attack the owner for DOMAIN-COMPLETENESS ' +
    'independently of how collapsed or confident it looks: does the both-tier admitted-package surface, the real-world concept, or a consumer ' +
    'contract admit a capability this owner still OMITS (a flat membership/id set where the concept owns ' +
    'geometry/metrics/attributes/topology/operations; a 2-variant union where the domain has twenty; the obvious 3 keys where the concept carries ' +
    'fifteen; a name/prose promising capability the body lacks)? Name it with a cite and EXTEND THE OWNER IN PLACE (a case/row/field/operation, ' +
    'per ANTICIPATORY_COLLAPSE) — a structurally-perfect but capability-sparse or illusory owner is a DEFECT, not a finished page. Conversely, ' +
    'REJECT any flat-spam/speculative/parallel-surface extension.',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (run this every time, NOT only when an architectural restructure is warranted): re-attack every ' +
    'conformance dimension with fresh hostile eyes, trusting nothing the prior passes claimed — the COLLAPSE_SCAN signals, OWNER_CHOOSER ' +
    'correctness per shape, the KNOB_TEST per param, the ASPECT taxonomy, rail + closed-fault-vocabulary discipline, capability-completeness + ' +
    'illusion per owner, payload/`frozendict`/PEP conformance, both-tier `.api` maximization, py3.15-modern typing, and prose/comment hygiene — ' +
    'and fix every defect. Even absent a structural rebuild, the fence must end objectively denser, MORE CAPABLE, more correct, and more powerful ' +
    'than the critique left it; if the strongest form is genuinely already present, prove it by finding nothing — never invent churn.',
  'Hold the highest bar of any stage; reject "good enough"; every defect you raise you REPAIR. Report what you extended in `extended`. Return the ' +
    'fix-log + residual_high — each item a {files: [every repo-relative path the cross-file fix spans], claim} object for a CROSS-FILE item you ' +
    'cannot fix from one file (NO severity — every finding counts equally and the reconcile phase addresses all; every single-file fix you make ' +
    'yourself).'].join('\n')
const STAGES = [
  { key: 'rebuild', build: authorPrompt, effort: 'max' },
  { key: 'crit', build: critiquePrompt, effort: 'xhigh' },
  { key: 'redteam', build: redteamPrompt, effort: 'max' },
]
const processPage = async (w) => {
  const logs = {}
  for (const st of STAGES) {
    const r = await agent(st.build(w.page), { label: st.key + ':' + folderOf(w.page) + ':' + subOf(w.page), phase: 'Rebuild-' + folderOf(w.page), schema: FIXLOG_SCHEMA, effort: st.effort, stallMs: 300000 })
    if (r === null) break
    logs[st.key] = r
  }
  return { page: w.page, logs, ok: Object.keys(logs).length === STAGES.length }
}
const seamPrompt = (pkg, rebuilt) => [LAW, '', MECHANICS, '', BOUNDARIES, '', 'TASK: FOLDER-WIDE SEAM CHECK of package `' + pkg.name + '` after a TARGETED per-file ' +
  'rebuild touched ONLY these pages:\n' + JSON.stringify(rebuilt, null, 1) + '\nThe owning folder is `' + pkg.planning + '/**`. Read each rebuilt page; ' +
  'for every shape/owner/rail/seam/payload it changed, find the SIBLING pages in the SAME folder (OUTSIDE the targeted set) that consume it and that the ' +
  'rebuild left STALE, read ONLY those affected siblings, and FIX the drift in place so the folder stays coherent (a renamed/reshaped owner, a changed ' +
  'payload, a moved capability, a stale seam). Do NOT rebuild the siblings — repair ONLY the seam the targeted rebuild disturbed, preserving all ' +
  'capability and regressing nothing. Edit in place. Return verdict (`repaired` if you changed any sibling, else `clean`), repaired (each sibling ' +
  'repo-relative path you fixed), and summary.'].join('\n')

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
const inv = await agent('Resolve these rebuild TARGETS into the page set + owning packages for ' + ROOT + '. Each TARGET (repo-relative) is a PACKAGE ' +
  'root (e.g. ' + ROOT + '/artifacts), a SUB-FOLDER under .planning at ANY depth (e.g. ' + ROOT + '/artifacts/.planning/exchange OR ' + ROOT + '/artifacts/.planning/graphic/marks), ' +
  'or a specific design FILE (e.g. ' + ROOT + '/artifacts/.planning/export/indesign.md). TARGETS:\n' + JSON.stringify(TARGETS, null, 1) + '\nThe OWNING ' +
  'PACKAGE of a target is the path BEFORE "/.planning/", or the target itself when it has no "/.planning/" segment (a package root). Use find; do not ' +
  'cd; do not edit anything. Return: (1) packages — one entry per DISTINCT owning package: {name: the LAST path segment of the package root, planning: ' +
  '"<package>/.planning", api: "<package>/.api", root: "<package>"}. (2) rebuildPages — the TARGETED page subset (repo-relative *.md): a PACKAGE-root ' +
  'target expands to EVERY page under "<package>/.planning/**"; a SUB-FOLDER target to EVERY page under that sub-folder at ANY depth; a FILE target to ' +
  'itself; union all targets and dedup. (3) folderPages — EVERY design page under EVERY owning package "<package>/.planning/**" (the full folder set, ' +
  'the reconcile blast radius). Exclude IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md from BOTH rebuildPages and folderPages.', { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const packages = ((inv && inv.packages) || []).filter((p) => p && p.name)
const rebuildPages = [...new Set(((inv && inv.rebuildPages) || []).filter(Boolean))]
const folderPages = [...new Set(((inv && inv.folderPages) || []).filter(Boolean))]
const pending = rebuildPages.map((p) => ({ page: p }))
const total = pending.length
log('Discover: ' + total + ' targeted page(s) across ' + packages.length + ' package(s) [' + packages.map((p) => p.name).join(', ') + ']; folder-wide ' +
  'set ' + folderPages.length + '; CAP=' + CAP + ', ' + STAGGER_MS + 'ms stagger')
if (!total) { log('No targets resolved — pass a file, sub-folder, or package path as args (a string, an array, or {targets:[...]})'); return { root: ROOT, targets: TARGETS, total: 0, packages: packages.map((p) => p.name) } }

// --- [REBUILD]

phase('Rebuild')
const done = (await pool(pending, CAP, processPage)).filter(Boolean)

// --- [RECONCILE]

const norm = (x, page) => typeof x === 'string' ? { files: [page], claim: x } : { files: x.files && x.files.length ? x.files : [page], claim: x.claim }
const allRes = []
for (const r of done) for (const st of ['rebuild', 'crit', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, r.page)) }
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Rebuild: ' + done.length + '/' + total + ' pages; reconcile ' + uniq.length + ' residuals (crit+redteam, deduped) -> ' + clusters.length + ' ' +
  'clusters')
phase('Reconcile')
let reconciled = []
if (clusters.length) {
  reconciled = (await pipeline(
    clusters,
    (cl) => agent([LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', MECHANICS, '', PATLAW, '', BOUNDARIES, '', 'TASK: RECONCILE these cross-FILE residuals the ' +
      'critique AND red-team passes deferred. There is NO severity — treat EVERY residual as must-address. Your blast radius is the OWNING FOLDER(S): ' +
      'you MAY read and fix ANY sibling page under them to keep seams consistent with the rebuilt pages, not only the listed files. Read EVERY listed ' +
      'file. For each: if it is a real cross-file defect, FIX it in place (unify the shared type/seam/rail, repair the strata/boundary issue, or extend ' +
      'the shared owner in place to close a capability gap that spans files), preserving all capability and regressing no file; if a residual is ' +
      'FACTUALLY INCORRECT or not a real defect, leave it and say why in the summary — never silently skip a real one to avoid work. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', BOUNDARIES, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named files from disk and ' +
      'classify each residual: status "fixed" (real defect, now genuinely resolved), "invalid" (the claim is factually wrong / not a real defect — ' +
      'cite why), or "open" (real defect still NOT resolved). Default to "open" on any doubt for a real-looking defect; mark "invalid" ONLY when ' +
      'you can show the claim is wrong. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((v) => ({ cluster: cl, fix, verify: v })) : null,
  )).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const hard_residual = uniq.filter((r) => openClaims.has(r.claim))
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
const seamTargets = packages.map((pkg) => { const rebuilt = rebuildPages.filter((p) => p.indexOf(pkg.root + '/') === 0); const fall = folderPages.filter((p) => p.indexOf(pkg.root + '/') === 0); return { pkg, rebuilt, hasSiblings: rebuilt.length > 0 && rebuilt.length < fall.length } }).filter((x) => x.hasSiblings)
const seamResults = (await pool(seamTargets, CAP, (x) => agent(seamPrompt(x.pkg, x.rebuilt), { label: 'seam:' + x.pkg.name, phase: 'Reconcile', schema: SEAM_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)
const seamRepaired = seamResults.flatMap((s) => (s && s.repaired) || [])
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open (hard residual -> resolve-residuals), ' + dropped.length + ' ' +
  'dropped as invalid; folder-seam sweep repaired ' + seamRepaired.length + ' sibling(s)')

return { root: ROOT, targets: TARGETS, packages: packages.map((p) => p.name), complete: done.filter((r) => r.ok).length, incomplete: done.filter((r) => !r.ok).length, total: total, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped, seamRepaired: seamRepaired }
