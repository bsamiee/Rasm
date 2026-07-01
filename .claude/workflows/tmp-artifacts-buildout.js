export const meta = {
  name: 'tmp-artifacts-buildout',
  whenToUse: 'Run 3x (args aec | media | visual) to build out one big new artifacts capability plane granularly while cold-improving the whole folder.',
  description: 'Parameterized specific build-out over libs/python/artifacts/.planning, run 3x (topic = aec | media | visual). Plan (1 thin agent) classifies every folder page by per-page kind (new | rebuild | improve) from the in-script FOCUS_SETS seed. Discover (1 agent per 4 pages) deep-reads each page + the folder + BOTH .api tiers (the shared libs/python/.api + the folder libs/python/artifacts/.api) and emits a per-page reading map: the catalogs the page CURRENTLY composes (apiUsed), the admitted catalogs/members the concept ADMITS but ignores (apiUnderutilized, real analysis), the contextual relation, and initial api-stacking guidance. Implement (1 agent per 2 pages, kind-aware) authors new pages ground-up / hostile-rebuilds existing / cold-improves the rest, threading the reading map as an INITIAL POINTER it must ultra-deep-read and confirm. Critique then Redteam (1 agent per 4 pages each, ultra-harsh, fix in place). Reconcile (pool-capped union-find). The three focus sets are encoded in-script (FOCUS_SETS) so the run survives args-drop on scriptPath re-runs; args selects the topic (a string or {topic}). Reads the campaign brief libs/python/artifacts/.planning/_REBUILD_BRIEF.md. Every phase is pooled at CAP=8 (max 8 concurrent agents; reconcile is 6). Temporary campaign workflow.',
  phases: [
    { title: 'Plan', detail: 'one thin agent classifies every folder page by kind (new | rebuild | improve) from FOCUS_SETS[topic] + the folder listing' },
    { title: 'Discover', detail: '1 agent per 4 pages: deep-read each page + folder + BOTH .api tiers; emit per-page apiUsed / apiUnderutilized / context / stacking-guidance reading map' },
    { title: 'Implement', detail: '1 agent per 2 pages, kind-aware: author new ground-up / hostile-rebuild / cold-improve, composing the reading map as initial pointer' },
    { title: 'Critique', detail: '1 agent per 4 pages, xhigh: ultra-harsh doctrinal-conformance + capability-completeness audit, fix in place' },
    { title: 'Redteam', detail: '1 agent per 4 pages, max: adversarial architect red-team, fix in place' },
    { title: 'Reconcile', detail: 'ONE encompassing pass — residuals consolidated into <=6 buckets, 6 fix + 6 critique-grade verify+repair, NO re-entry loop; blast radius = artifacts folder + cross-folder libs/python + C# wire seams + index docs' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 8
const STAGGER_MS = 1500
const STALL = 300000
const DISCOVER_BATCH = 4
const IMPL_BATCH = 2
const REVIEW_BATCH = 4
const RECON_CAP = 6 // reconcile is ONE encompassing pass: residuals consolidate into <=6 buckets -> 6 fix + 6 verify, never a re-entry loop
const ROOT = 'libs/python/artifacts/.planning'
const BRIEF = ROOT + '/_REBUILD_BRIEF.md'
const API_SHARED = 'libs/python/.api'
const API_FOLDER = 'libs/python/artifacts/.api'
const FOCUS_SETS = {
  aec: {
    new: ['drawing/standard.md', 'drawing/dimension.md', 'drawing/annotate.md', 'drawing/symbol.md', 'drawing/detail.md', 'drawing/schedule.md', 'specification/section.md', 'specification/classify.md', 'delivery/register.md', 'delivery/transmittal.md', 'export/dxf.md'],
    existing: ['composition/sheet.md', 'visualization/table.md'],
    delete: [],
  },
  media: {
    new: ['media/container.md', 'media/filtergraph.md', 'media/timeline.md', 'media/subtitle.md', 'media/analysis.md', 'media/synthesis.md'],
    existing: ['media/audio.md'],
    delete: ['media/video.md'],
    absorb: { 'media/container.md': 'media/video.md' },
  },
  visual: {
    new: ['visualization/diagram/node_link.md', 'visualization/diagram/er.md', 'visualization/diagram/flowchart.md', 'visualization/diagram/sankey.md', 'visualization/diagram/section_callout.md'],
    existing: ['visualization/diagram/layout.md', 'visualization/diagram/draw.md', 'visualization/diagram/glyphset.md', 'graphic/vector.md', 'graphic/marks/encode.md', 'graphic/marks/decode.md', 'export/layered.md', 'typography/shape.md', 'typography/font.md'],
    delete: [],
  },
}

// --- [INPUTS] ----------------------------------------------------------------------------

const TOPIC = (() => {
  const raw = (typeof args === 'string') ? args.trim() : (args && typeof args === 'object' && args.topic) ? String(args.topic).trim() : ''
  const t = raw.toLowerCase()
  return FOCUS_SETS[t] ? t : ''
})()
const SEED = TOPIC ? FOCUS_SETS[TOPIC] : null
const rel = (p) => ROOT + '/' + p
const SEED_NEW = SEED ? SEED.new.map(rel) : []
const SEED_EXISTING = SEED ? SEED.existing.map(rel) : []
const SEED_DELETE = SEED ? (SEED.delete || []).map(rel) : []
const SEED_ABSORB = SEED && SEED.absorb ? Object.fromEntries(Object.entries(SEED.absorb).map(([k, v]) => [rel(k), rel(v)])) : {}

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const UNDERUTIL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'capability'], properties: { catalog: { type: 'string' }, capability: { type: 'string' } } } }
const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages'], properties: { pages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'kind'], properties: { page: { type: 'string' }, kind: { type: 'string', enum: ['new', 'rebuild', 'improve'] }, absorb: { type: 'string' } } } }, deletePages: { type: 'array', items: { type: 'string' } } } }
// WORK item — each carries the per-page reading map Discover hands every downstream stage.
const WORK = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'kind', 'apiUsed', 'apiUnderutilized', 'contextNote', 'stackingGuidance'], properties: { page: { type: 'string' }, kind: { type: 'string', enum: ['new', 'rebuild', 'improve'] }, absorb: { type: 'string' }, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, contextNote: { type: 'string' }, stackingGuidance: { type: 'string' } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['worklist'], properties: { worklist: WORK, summary: { type: 'string' } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const REVIEW_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'Rasm monorepo, libs/python/artifacts planning corpus (markdown specs of intended Python module designs). CLAUDE.md manifest law governs. DENSITY BAR: ' +
    'docs/stacks/python/ (README/language/shapes/iteration/surfaces-and-dispatch/rails-and-effects/concurrency/boundaries/algorithms/system-apis/runtime) — author Python ' +
    'as dense, polymorphic, and rich as that bar. Cite ONLY members confirmed in the .api catalogs — and MINE BOTH TIERS fully: the shared/universal branch catalogs ' +
    '`libs/python/.api/*.md` AND the folder-specific `libs/python/artifacts/.api/*.md`. Maximize the shared/universal catalogs wherever relevant, never only the folder set.',
  'This is a FUNDAMENTAL GROUND-UP REBUILD (or, for a `new` page, a ground-up AUTHORING) of a planning-stage DESIGN PAGE, not a polish pass. Improve the page objectively: ' +
    'collapse surfaces/types, deepen bleeding-edge spellings, maximize admitted-library capability, AND close the concept capability gaps.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a REPORT of edits ALREADY MADE, ' +
    'never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE items (report those in residual_high).',
].join('\n')
const CAMPAIGN = [
  'CAMPAIGN — READ ' + BRIEF + ' FIRST as binding scope. UNIFIED TELOS: artifacts is a publication/print-production engine that is SIMULTANEOUSLY the foundation of a ' +
    'high-end AEC documentation engine — one body, not two arms; grade every owner against BOTH planes at once (a journal-grade pub primitive AND a drawing-sheet / ISO ' +
    'documentation primitive), and the bar is their UNION, never the lesser plane. The brief fixes the admit/replace package roster (categorical-best per concern, NO ' +
    'overlaps, supersede-not-accrete — flag a superseded package for the final pyproject reconciliation), the new-domain structure + the 7-page media restructure + the ' +
    'capability-detection media-filter contract, the AEC owned-vocabulary set (ISO 128/129-1/5455/5457/7200/13567/3098, NCS, MasterFormat/UniFormat/OmniClass authored to ' +
    'exact published cardinalities), the entry/receipt-seam unification target (ONE core/plan production entry, ONE core/receipt family — every domain incl. the new ones ' +
    'contributes a CASE, never a parallel entry or receipt rail), and the C#-side out-of-scope boundary (Rasm.Bim owns IFC; visualization/table consumes QTO/schedule rows ' +
    'via data/tabular, never re-authoring IFC).',
  'COVERAGE OVER SIZE: there is NO line budget and the prior ~450-LOC look is DROPPED — a fence is exactly as large as its owned concern requires (doctrine fences run ' +
    '3-4x denser than ordinary code). Grade capability COVERAGE against the full domain + both-tier package surface, never byte count: a small page modeling a rich concept ' +
    'is under-built, and a large well-collapsed page can still be capability-sparse.',
  'AUTHOR NEW PAGES IN FULL: a page of kind `new` does not exist yet (and may open a NEW folder — minimum two files) — AUTHOR it ground-up to the full doctrine + ' +
    'domain-complete capability bar, in the same code-fence-first design-page form as its mature siblings, wired into core/plan + core/receipt + the ARCHITECTURE [01]/[02] ' +
    'and README [01]/[02] maps. A page of kind `rebuild` is hostile-rebuilt in place; a page of kind `improve` is cold-pass improved. When a new page ABSORBS an old one ' +
    '(its reading map names the absorbed page), MOVE the real content over and then DELETE the absorbed page so no duplicate owner remains.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, AND red-team) is HOSTILE: assume the existing fence is NAIVE, SHALLOW, JUNIOR, or ILLUSORY until it survives an ' +
    'aggressive attack; the burden of proof is ON THE CODE, never on you. "Mature", "already strong", "good enough", "done", and a prior `clean` verdict are REJECTED ' +
    'self-assessments — MOST of this corpus is naive, surface-level, old-style Python dressed in the right vocabulary, and it is NOT tolerable. Default to "this fence is ' +
    'naive and must be rebuilt to the strongest form the doctrine admits" and MAKE that rebuild; a no-edit verdict is reached ONLY after a genuinely aggressive attack on ' +
    'the real domain + the verified both-tier package surface finds nothing — never a first-read concession, never to avoid work. Reject "good enough" categorically.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code is the code that PRETENDS to be advanced: it uses the doctrine vocabulary ' +
    '(`@tagged_union`/`frozendict`/`Result`/`Option`/the rails), cites packages, reads dense and confident — yet is HOLLOW. Treat dense, confident-looking fences with MORE ' +
    'suspicion, not less, and DISBELIEVE every claim the page makes about itself until you verify it against the real domain and the catalogued both-tier package surface. ' +
    'HUNT: a name/signature/prose that PROMISES capability the body does not implement; a "rich" owner that is a thin slice of its concept (a 2-variant union for a 20-case ' +
    'domain; the obvious 3 keys where the concept carries fifteen); decorative density and ceremony carrying no real capability; a placeholder/stub/sketch dressed as a ' +
    'finished design; prose that ASSERTS richness the fence does not contain; a structurally-correct collapse that is semantically empty; a `.api` member cited but never ' +
    'verified (a phantom). Every such illusion is a DEFECT to rebuild, not a feature to preserve; never invent churn to look busy either.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — MANDATORY FULL READ BEFORE ANY EDIT: docs/stacks/python/ is the route-owned law, and you hold a small batch with ample context budget, so there ' +
    'is NO excuse for a partial read. STEP 0 — LIST THE DIRECTORY FIRST: enumerate `docs/stacks/python/` AND `docs/stacks/python/.api/` with a real `ls`/`find` to obtain ' +
    'the COMPLETE doctrine + catalog inventory before reading, so not one page is silently skipped and the routing order is taken from the source of truth, never from ' +
    'memory. THEN READ EVERY doctrine page IN FULL — top-to-bottom, every section, every family card, every snippet, never a partial, skim, grep-jump, or section-sample — ' +
    'STARTING AT THE README and proceeding in its `[01]-[ATLAS]` routing order: (1) `docs/stacks/python/README.md` (the laws + the COLLAPSE_SCAN signals + RULE_ENFORCEMENT ' +
    '+ PAGE_CRAFT + CORPUS_LAW), then strictly in routing order language.md, shapes.md, iteration.md, surfaces-and-dispatch.md, rails-and-effects.md, concurrency.md, ' +
    'boundaries.md, algorithms.md, system-apis.md, runtime.md. READ EVERY ROOT `*.md` THE STEP-0 `ls` RETURNS AND FULLY INTERNALIZE IT BEFORE ANY EDIT — this enumeration ' +
    'is the floor, not the ceiling: a root page present on disk but absent from this list is STILL mandatory law, while the sub-folders `domain/` and `numerics/` are OUT of ' +
    'this read. The README `[STATE]` column marks each page finalized (binding law) or partial (operative-but-unfinalized context); read EVERY page regardless of state and ' +
    'hold every fence to them as fact — a partial, sampled, or skipped read of this doctrine is itself a process defect, not an efficiency. docs/stacks/csharp/ is the ' +
    'density/ambition FLOOR — match its richness, never import C#-shaped idioms.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows `Raw -> Payload -> Canonical owner -> Rail -> Projection -> Egress`. Raw material is admitted EXACTLY ONCE into ' +
    'an evidence-carrying owner (Pydantic/`TypedDict` payload at ingress); interior code never re-validates, never sees `None`-as-failure, sentinels, or provider shapes; ' +
    'egress projects outward (`msgspec.Struct` wire) from the canonical owner. Parameterize BOTH ingress AND egress so the same owner sources and sinks across many ' +
    'providers/apps without touching its interior.',
  'SHAPE LAW: one concept owns exactly ONE type (SHAPE_BUDGET) — variants are cases in one closed family, never sibling types; one rich polymorphic surface over many ' +
    'shallow (DEEP_SURFACES); the owner is shaped for the family it will ABSORB (ANTICIPATORY_COLLAPSE) so the next case/dimension/modality lands as ONE declaration with ' +
    'every consumer untouched or broken loudly at type-check. Choose each owner by the OWNER_CHOOSER discriminants — admission (trusted/untrusted), identity regime ' +
    '(value/tag/key/reference), variant arity (one/closed-family/open), payload timing (def-time/runtime), openness (closed/semi/open) -> the right owner among ' +
    '`TypedDict`, Pydantic, `msgspec.Struct`, frozen dataclass, rich class, `StrEnum`/`Literal`, `sentinel`, `Option`/`Result`, `frozendict`/`Map`/`tuple`, `Protocol`. A ' +
    'misplaced shape traces to one mis-answered discriminant.',
  'ASPECT-FIRST (DEFINITION_TIME_ASPECTS): every CROSS-CUTTING capability — retry, telemetry/spans, validation, contracts, memoization, registration, receipts, fault ' +
    'rails — is a SIGNATURE- and RAIL-PRESERVING decorator (inline `**P` + `functools.wraps`) that materializes policy, STACKS in deterministic order (bottom-up at ' +
    'definition, top-down at call), and NEVER raises into domain flow (a failing aspect returns the rail `Error`). Two-to-four wrappers that always co-occur collapse into ' +
    'ONE parameterized aspect factory. Code reads as STACKED DECORATORS over a thin pure core, never inline-repeated concerns or sibling helper functions; the domain ' +
    'transform itself stays a pure function/fold.',
  'DERIVATION + ARITY: cases sharing generative structure are DERIVED — one primary `frozendict` correspondence declared, every secondary map derived from it ' +
    '(DERIVED_LOGIC), or a fold/comprehension — never enumerated arms. Configuration enters as ONE behavior-carrying value (vocabulary member, tagged variant, frozen ' +
    'policy table), never flag sets the body re-derives (POLICY_VALUES). ONE entrypoint owns every modality (singular/plural/batch/stream), discriminating on the INPUT ' +
    'SHAPE (`T | Iterable[T]` normalized once at the head), never a name suffix or a `mode`/`batch` knob (MODAL_ARITY); a `timeout`/`retry`/`deadline` is an aspect or an ' +
    '`anyio` scope, never a signature param (KNOB_TEST).',
  'RAILS (rails-and-effects): the narrowest carrier that states the outcome, chosen ONCE at admission — `Option[T]` non-failing absence, `Result[T, E]` typed fallibility, ' +
    '`effect.result` do-notation for sequential `bind`, `Block`/`Map` immutable traversal, an `anyio` task group as the failure boundary (NEVER `asyncio.gather`), ' +
    '`stamina.retry` as the decorator (never a sleep-loop). The fault type `E` is a CLOSED vocabulary — `Literal` set, `StrEnum`, or `@tagged_union` family — NEVER a bare ' +
    '`str` for a multi-cause domain. Accumulate-vs-abort is a correctness decision fixed at the boundary: `map2`/accumulating-fold for independent operands, `bind` ' +
    'short-circuit for dependent steps. Cancellation is not failure; resource cleanup is `AsyncExitStack` + a shielded scope.',
  'STACK .api CAPABILITY (load-bearing): FIRST inventory the COMPLETE catalog set available to this page — BOTH the shared/universal branch catalogs at `libs/python/.api/*.md` ' +
    '(anyio, expression, msgspec, pydantic, pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*) AND every folder-specific catalog at ' +
    '`libs/python/artifacts/.api/*.md` — then mine them for the full ADVANCED surface of each package (combinators, hooks, native pipelines, discriminators, async mirrors) ' +
    'and how packages STACK. List BOTH `.api/` tier DIRECTORIES in full and DIFF that complete inventory against what the page already cites: every admitted catalog whose ' +
    'domain the page admits but does NOT yet use is an ADOPTION TARGET — adopt it to depth here, or when it belongs on a sibling page surface it as a residual; a relevant ' +
    'admitted catalog left unadopted is a DEFECT. There is NO fixed library count: compose EVERY relevant admitted library into single dense operations woven as ONE rail, ' +
    'and ALWAYS layer the shared/universal rails ON TOP OF the folder-specific domain packages — NOT flat one-shot per-library uses. Use the DEEPEST primitive each ' +
    'package itself reaches for (LIBRARY_DEPTH); reject surface-level single-feature subsets and any thin rename wrapper.',
  'PRESERVE all capability (densify, never delete functionality). Where a page is already dense, refine; where it is flat/naive, rebuild ground-up. Never regress ' +
    'correctness or boundary law.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — structural collapse and both-tier `.api`-stacking are NECESSARY but NOT SUFFICIENT. A page can be fully ' +
    'collapsed into one closed family/ADT and STILL be capability-thin: modeling a NAIVE, LIMITED slice of its domain concept — a flat id/member set where the concept owns ' +
    'geometry, metrics, attributes, topology, and operations; a 2-variant `@tagged_union` where the domain has twenty; a `TypedDict`/`Struct` with the obvious 3 keys where ' +
    'the concept carries fifteen. Structural completeness and CAPABILITY completeness are ORTHOGONAL. A FULL rebuild ALSO closes the capability gap so the page OWNS ITS ' +
    'DOMAIN CONCEPT COMPLETELY. Per COMPOSED_IMPLEMENTATION + the doctrine growth law (capability grows sublinearly; growth lands as cases/rows/policy-values INSIDE ' +
    'existing owners, never new surfaces beside them), every real missing concern lands as a CASE in the existing closed `@tagged_union`/`Literal`/`StrEnum` family, a ROW ' +
    'or richer data on the existing `frozendict` table, a FIELD on the existing `msgspec.Struct`/Pydantic model/frozen dataclass/`TypedDict`, an OPERATION on the existing ' +
    'surface, or a POLICY_VALUE on the existing vocabulary — reshaping the owner as if it had always carried it; NEVER a parallel type, a new file, a sibling shape, or ' +
    'flat appended code.',
  'GAP SOURCES (every extension MUST cite exactly one — justified, never speculative): (a) PACKAGE — a member the admitted package surface exposes that the concept ADMITS ' +
    'but the page IGNORES is a missing case in the owner law (BOTH tiers; the `apiUnderutilized` reading map is the first place to look; stacking that full surface IS new ' +
    'functionality woven into the owner, not a denser spelling of the same call; verify the member exists). (b) DOMAIN — an attribute, metric, sub-kind, relationship, ' +
    'state, or operation the REAL concept demands but the page omits (a dimension owner owns the full ISO 129-1 linear/aligned/angular/radial/diameter/ordinate/chain/' +
    'baseline + tolerance family, not a single linear case; a layer codec owns the full ISO 13567 + NCS discipline/major/minor/status structure, not a flat string). (c) ' +
    'CONSUMER — a contract a sibling or downstream owner will require that has no composed spelling here yet (a need with no spelling marks a missing case: the law extends ' +
    'first, the feature lands second).',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy — capability COVERAGE against the full domain + both-tier package surface is the real measure. A SMALL page modeling a ' +
    'rich concept is almost always under-built (give it the DEEPEST sweep), AND a LARGE, well-collapsed page can still be capability-SPARSE. Assess each owner against its ' +
    'domain independently of size and EXTEND every owner the concept under-realizes IN PLACE — integrated and unified into the one owner at full operator depth, every new ' +
    'field/case/operation composing the existing rails — never a new flat surface beside it.',
  'JUSTIFIED, NOT RANDOM: if after a real domain + package + consumer sweep the concept is genuinely complete, prove it by adding nothing — never invent capability to look ' +
    'busy or pad with flat fields. Every added case/row/field/operation is load-bearing, cites a package member / domain attribute / consumer contract, and composes the ' +
    'existing rails; preserve ALL existing capability — extension only deepens, never regresses.',
].join('\n')
const MECHANICS = [
  'MECHANICAL EXECUTABILITY — a design-page fence is a SIGNATURE-AND-IMPLEMENTATION CONTRACT, never a sketch: every fence MUST parse under the active py3.15 surface AND ' +
    'type-check against the REAL cross-page canonical owners it imports, because the corpus is ONE body (CORPUS_LAW three-layer inheritance) and a fence reading a ' +
    'field/case/attribute a sibling owner does not declare is a runtime DEFECT, not a design liberty. Mentally COMPILE and TYPE-CHECK each fence before accepting it. Find ' +
    'each class below BY NAME and FIX it in place by growing the EXISTING owner (a case/field/operation/row), never a new file:',
  'FENCE-PARSES (language.md CLOSED_MATCH_SITE) — every `match`/structural pattern, `for`-target, comprehension, and t-string parses: an OR-pattern whose alternatives bind ' +
    'DIFFERENT names, an invalid iterable-unpacking or starred target, or a malformed pattern is a NON-COMPILING fence and an automatic rebuild target.',
  'MODEL-COHERENCE (CORPUS_LAW) — every attribute, field, case tag, method, and imported symbol a fence reads off a canonical owner declared on ANOTHER page (or earlier in ' +
    'this one) MUST exist on the real declaration of that owner: verify each cross-owner read against the sibling owner before writing it, reconcile to the ONE canonical ' +
    'name, never invent a field the owner does not carry, and surface an un-reconcilable cross-page name as a residual.',
  'TOTAL-DISPATCH (shapes.md families) — `assert_never(unreachable)` is an exhaustiveness WITNESS, valid ONLY when every member of the FULL closed family is handled before ' +
    'it: enumerate the complete case set and prove NO valid case routes to `assert_never`; a parallel dispatch map keyed by a closed family must be TOTAL over it. A partial ' +
    '`match`/map dressed as total is a DEFECT.',
  'SINGLE-FACT EVIDENCE (rails-and-effects.md STATE_RECEIPTS; boundaries.md BYTE_IDENTITY) — the bytes, the content key, and the receipt evidence derive from ONE computed ' +
    'fact stored once on the stepped owner: the producer computes the fact, the receipt/contribute path READS the stored fact, never re-renders. A path that recomputes a ' +
    'render/placement/native-mutation a second time to mint receipt evidence is a DOUBLE-RENDER defect.',
  'LOOP-OFFLOAD (concurrency.md OFFLOAD_LANE) — synchronous CPU-bound or GIL-hostile provider work (rendering, parsing, native FFI sweeps, codec encode) NEVER runs on the ' +
    'event loop, NOR as an argument expression evaluated before the offload call: it crosses on exactly one arm — `anyio.to_thread.run_sync` for a GIL-releasing native ' +
    'call or blocking I/O, `to_interpreter.run_sync` for pure-Python isolate-safe CPU work, `to_process.run_sync` for a GIL-hostile or not-isolate-safe native call — each ' +
    'bounded by an explicit `CapacityLimiter`. A heavy synchronous call inside an async body is an event-loop-starvation DEFECT.',
  'HANDLE-LIFETIME (boundaries.md CAPSULE_OWNER) — every native/FFI handle a provider opens (a `*.open(...)` returning a C-backed document, container, plotter, cursor, or ' +
    'pinned buffer) closes DETERMINISTICALLY through an `AsyncExitStack.enter_async_context`, a `with` bracket, or a capsule registering release via `weakref.finalize` ' +
    'under a shielded teardown — never left for the GC to reap. An opened handle with no deterministic close is a LEAK defect; callers receive detached values or rails.',
  'BINARY-KERNEL (boundaries.md CAPSULE_OWNER) — a multi-megabyte binary mutated across N steps (a PDF, a container, a layered raster) is ONE imperative measured kernel ' +
    'threading ONE owned handle mutated in place, NOT a functional fold that rebinds and recopies the whole buffer per step; the kernel lives inside the shielded resource ' +
    'bracket, returns the rail `Result`, and carries one `# Exemption:` line naming the platform-forced in-place-mutation seam.',
  'IDENTITY-REGIME (boundaries.md MEMO_KEY) — a content-addressed key indexes by CONTENT, so two structurally-distinct siblings carrying identical content collide and ' +
    'silently overwrite in a `Map[ContentKey, _]`. Where an index/diff must distinguish identical-content siblings, the key joins a STRUCTURAL discriminant to the content ' +
    'digest — a content-only key under a structural index is a CORRUPTION defect.',
  'TEMPLATE-SAFETY (language.md TEMPLATE_STRUCTURE_SITE) — structured-text and markup egress (SVG, XML, Typst, HTML, query strings) built from dynamic or untrusted input ' +
    'uses PEP 750 t-strings / `string.templatelib.Template` processors or a structured builder (`xml.etree.ElementTree`), NEVER f-string interpolation with a hand-rolled ' +
    'escape. An f-string splicing a value into markup is an INJECTION defect.',
  'STREAM-OVER-MATERIALIZE (iteration.md LAZY_COMBINATORS) — a large or unbounded extraction (every word of a 500-page document, every node of a corpus tree, every frame ' +
    'of a video) is a lazy `itertools`/generator pipeline or a `yield from` fusion typed `Iterator[T]`, never an eagerly allocated `tuple`/`Block` of the whole result held ' +
    'in RAM and materialized only at the persistence/egress edge.',
  'NO-EXCEPTION-HOTLOOP (rails-and-effects.md EXPRESSION_SPINE) — a per-element `try`/`except` driving control flow inside a fold over a large collection is BOTH a ' +
    'domain-logic violation AND a throughput defect: a total predicate or a non-raising `Option`-returning parse replaces the per-element raise, and the boundary `catch` ' +
    'trap stays at the boundary, never in the hot fold.',
  'DERIVED-NOT-PARALLEL + PER-MODE PAYLOADS (DERIVED_LOGIC) — a secondary map hand-synced to a primary (a `_KEYS` tuple-table parallel to a `@tagged_union` case-payload ' +
    'tail) is a DERIVATION defect: declare ONE primary correspondence and DERIVE every secondary by comprehension. A monolithic typed bag whose fields are irrelevant for ' +
    'most modes (one `Spec` carrying the fields of every backend) is a permissive-bag DEFECT even when fully typed: collapse it into a discriminated per-mode ' +
    '`@tagged_union` whose each case carries ONLY the fields of its own mode — WITHOUT splitting the owner into new files.',
].join('\n')
const PATLAW = [
  'PY-VERSION LAW: target Python 3.15 on the full modern band (3.11/3.12/3.13/3.14/3.15) — advanced patterns ONLY, zero legacy idioms, IDENTICAL conventions across every ' +
    'folder and package.',
  'NEVER write `from __future__ import annotations`. NEVER use legacy typing: use PEP 585 builtin generics (`list[T]`, `dict[K, V]`, `tuple[...]`, `set[T]`) NOT ' +
    '`typing.List/Dict/Tuple/Set`; PEP 604 unions (`X | None`, `A | B`) NOT `Optional`/`Union`; PEP 695 type parameters (`class C[T]:`, `def f[T](...)`, `type Alias[T] = ' +
    '...`) NOT `TypeVar` + `Generic`. Use `Self`, `override`, `TypeIs`/`TypeGuard`, `assert_never`, `ReadOnly`, `TypedDict` + `NotRequired`/`Required`, `LiteralString`, ' +
    '`enum.StrEnum`/`IntEnum`, and `@dataclass(slots=True, frozen=True)` or `msgspec.Struct`/pydantic models where each best fits.',
  'PAYLOADS — NEWEST FORM: ingress payloads are static `TypedDict` contracts with `closed=True` or `extra_items=T` and per-key `Required[]`/`NotRequired[]`/`ReadOnly[T]`, ' +
    'admitted through a module-level `TypeAdapter`, with `Unpack[TypedDict]` at root keyword entrypoints; extension bands fold into `frozendict`/tuple evidence at ' +
    'materialization, and `msgspec.Struct(frozen=True)` owns wire/egress. NO `dict[str, Any]` bags, homogeneous `**kwargs`, or `Mapping[str, object]` payloads.',
  'FROZENDICT (py3.15 builtin): `from builtins import frozendict` is the owner for immutable map rows, dispatch/policy TABLES (one primary `frozendict[K, tuple[...]]`, ' +
    'secondary maps derived from it), payload `extra_items` extension bands, and immutable evidence — REJECT `MappingProxyType`, a module-level mutable `dict` used as a ' +
    'table, tuple-pair pseudo-maps, and mutate-then-freeze. Prefer total `match`/structural pattern matching over if-chains, walrus where it tightens, and `assert_never` ' +
    'on closed unions ONLY where it is genuinely unreachable over the FULL case set; PEP 750 t-strings are MANDATORY for all dynamic or untrusted structured-text and markup ' +
    'egress. Keep every choice CONSISTENT across folders so the corpus reads as one ultra-advanced codebase.',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package/folder owner strictly in its lane; internal code uses canonical names and shapes with mapping only at the edge; do ' +
  'not trample a sibling owner while densifying; respect the dependency direction of the workspace strata. The C# boundary is law: Rasm.Bim owns IFC semantics; the ' +
  'artifacts corpus composes QTO/schedule/credential facts at the wire, never re-authoring the IFC model.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling rule/contract; one idea ' +
    'per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness disclaimers, report framing, or empty hedges. Trim walls ' +
    'of explanation to the load-bearing contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. ' +
    'Prose that ASSERTS capability the fence does not implement is a defect, not content.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in backticks. Name the exact member/type/rail ' +
    'in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical section-divider headers (language-comment ' +
  'marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY where intent is not already obvious from names, types, and ' +
  'signatures: default to ZERO comments on self-evident code; at most 1 line where a comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, ' +
  'contract, or boundary. NO restating the code, no narration, no task/process/session/history/proof/review comments, no docstring bloat.'
const PRE = [LAW, '', CAMPAIGN, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', MECHANICS, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, ''].join('\n')

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
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
// packClusters — consolidate the union-find clusters into AT MOST n encompassing buckets (greedy smallest-bucket by residual count),
// so reconcile is a single bounded pass (<=n fix + <=n verify), never one agent per cluster ballooning out of hand.
const packClusters = (clusters, n) => {
  if (clusters.length <= n) return clusters
  const buckets = Array.from({ length: n }, () => [])
  const sorted = clusters.slice().sort((a, b) => b.length - a.length || ((a[0] && a[0].claim) || '').localeCompare((b[0] && b[0].claim) || ''))
  for (const c of sorted) { let mi = 0; for (let i = 1; i < n; i++) if (buckets[i].length < buckets[mi].length) mi = i; buckets[mi].push(...c) }
  return buckets.filter((b) => b.length)
}
const folderOf = (p) => { const head = p.split('/.planning/')[0].split('/'); return head[head.length - 1] || 'root' }
const subOf = (p) => p.split('/.planning/').pop()

// MAP_BY_PAGE — the per-page reading map (apiUsed/apiUnderutilized/contextNote/stackingGuidance/kind/absorb) Discover surfaced, indexed for every downstream stage.
let MAP_BY_PAGE = new Map()
const mapFor = (p) => MAP_BY_PAGE.get(p.page || p) || { page: p.page || p, kind: p.kind || 'improve', apiUsed: [], apiUnderutilized: [], contextNote: '', stackingGuidance: '' }
const mapsFor = (pgs) => pgs.map((p) => mapFor(p))
const READ_MANDATE = (maps) => 'READING MAP (the per-page grounding Discover surfaced — an INITIAL POINTER, never the ceiling):\n' + JSON.stringify(maps, null, 1) + '\n' +
  'BEFORE editing, ULTRA-DEEP-READ: (1) ALL docs/stacks/python/ root pages in README atlas order (the FULL set, NEVER a subset); (2) the campaign brief ' + BRIEF + ' in ' +
  'FULL; (3) BOTH .api tiers — list `' + API_SHARED + '/` AND `' + API_FOLDER + '/` in full and read every catalog relevant to these pages. The map POINTS; you VERIFY and ' +
  'EXCEED it: COMPOSE every `apiUsed` catalog at full operator depth, STACK every `apiUnderutilized` {catalog, capability} INTO the owning page (closing the ' +
  'underutilization is new functionality woven into the owner — a case/row/field/operation), AND independently CONFIRM no other relevant admitted catalog (either tier) is ' +
  'missing or unconsidered — the discovery agent only pointed, you do the exhaustive read. Verify every cited member exists (`uv run --frozen python -m tools.assay api ' +
  'resolve <pkg>`); a member you cannot verify is a phantom to delete.'

const planPrompt = () => ['Rasm monorepo, libs/python/artifacts/.planning. TASK: thin enumerate + classify (read-only, do NOT edit). Read ' + BRIEF + ' for the structure. The ' +
  'build-out topic is `' + TOPIC + '`. SEED pages to AUTHOR new (may open a new folder): ' + JSON.stringify(SEED_NEW) + '. SEED pages to REBUILD (existing focus): ' + JSON.stringify(SEED_EXISTING) +
  '. SEED pages to DELETE after absorption: ' + JSON.stringify(SEED_DELETE) + '. Absorb map (new-page <- absorbed-page): ' + JSON.stringify(SEED_ABSORB) + '. Use find/ls; do NOT cd. ' +
  'List EVERY design page under ' + ROOT + '/** (repo-relative *.md), EXCLUDING IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md, the campaign brief _REBUILD_BRIEF.md, and the ' +
  'SEED delete pages. Classify EACH page: kind `new` (a SEED-new page confirmed ABSENT on disk — author ground-up), `rebuild` (a SEED-existing focus page, OR a SEED-new ' +
  'page that already exists), `improve` (every OTHER existing folder page — the cold-pass set). Carry the absorb target on the owning new page. Return pages (each {page, ' +
  'kind, absorb?}) + deletePages (the SEED delete pages confirmed present). The owning package is artifacts.'].join('\n')
const discoverPrompt = (batch) => [LAW, '', CAMPAIGN, '', ADVERSARIAL, '', 'TASK: READ-ONLY DISCOVERY for these ' + batch.length + ' pages (investigate, do NOT edit): ' +
  batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') + '. Read ' + BRIEF + ' for the per-page concern + owned vocabulary/roster. For a kind=`improve`/`rebuild` page READ ' +
  'the page IN FULL; for a kind=`new` page (it does not exist yet) read its concept in the brief + its nearest SIBLING pages + the structural plan. ALSO read the folder ' +
  'at large (the sibling pages each composes, the index docs ' + ROOT.replace('/.planning', '') + '/ARCHITECTURE.md + README.md, and core/plan + core/receipt). Then ENUMERATE ' +
  'BOTH .api tiers IN FULL with a real `ls`: the SHARED `' + API_SHARED + '/` AND the FOLDER `' + API_FOLDER + '/`. For EACH of these pages produce its reading map: (a) ' +
  '`apiUsed` — the .api catalogs the page CURRENTLY composes, BOTH tiers (for a new page, the catalogs its concept WILL compose); (b) `apiUnderutilized` — each {catalog, ' +
  'capability}: an admitted catalog or member (either tier) the page concept ADMITS but the page IGNORES — REAL analysis against the verified .api inventory + the brief ' +
  'roster, never a guess; a concrete capability the implement MUST stack to close the underutilization (e.g. a `skia-pathops` boolean/offset op a vector page never calls, ' +
  'a `pyelk` ports/nesting layout a diagram page ignores, an `expression` Result rail a flat page skips); (c) `contextNote` — the page contextual relation: which sibling ' +
  'owners/seams it composes, where it sits in the folder, which core/plan + core/receipt case it contributes; (d) `stackingGuidance` — the INITIAL pointer on what api ' +
  'stacking + capability extension the implement should add (the implement confirms + deepens it). Members are verified-local truth via `uv run --frozen python -m ' +
  'tools.assay api resolve` where the package is installed (a gated/uninstalled package falls back to its catalog/official surface); never list a phantom. Return worklist ' +
  '(each {page, kind, absorb?, apiUsed, apiUnderutilized, contextNote, stackingGuidance}).'].join('\n')
const implementPrompt = (batch) => [PRE, READ_MANDATE(mapsFor(batch)), '', 'TASK: HOSTILE IMPLEMENT of these ' + batch.length + ' pages IN PLACE, each per its kind: ' +
  batch.map((p) => p.page + ' [' + (mapFor(p).kind) + ']').join(', ') + '. kind=`new`: GROUND-UP AUTHOR the page (it does not exist) to the full doctrine + domain-complete ' +
  'capability bar, in the code-fence-first design-page form of its mature siblings, wired into core/plan + core/receipt as a CASE + the ARCHITECTURE/README maps; if its ' +
  'reading map names an absorb target, MOVE that page content over then DELETE the absorbed page. kind=`rebuild`: HOSTILE GROUND-UP REBUILD in place. kind=`improve`: ' +
  'general cold-pass improve. EVERY page: DISBELIEVE it (assume naive/junior/illusory until it survives an aggressive attack; dense confident code is the PRIME suspect for ' +
  'hollowness); construct in LIFECYCLE order (admit raw ONCE into a typed payload -> canonical owner by OWNER_CHOOSER -> stacked rail/aspect over a thin pure core -> ' +
  'projection -> egress, BOTH parameterized); collapse parallel shapes into one closed family/ADT; drive cases with a derived `frozendict` table or fold; one polymorphic ' +
  'entrypoint per modality. COMPOSE the reading-map `apiUsed` at full operator depth + STACK every `apiUnderutilized` into the owner + CONFIRM no other admitted catalog is ' +
  'missing (the map is the initial pointer; you ultra-deep-read both .api tiers yourself). CLOSE the concept capability gaps by growing the EXISTING owner in place ' +
  '(case/row/field/operation/policy-value, each citing a package member / domain attribute / consumer contract). py3.15-modern only; high-signal all-backticked prose. ' +
  'WRITE-FULLY now (the fix-log REPORTS edits already made). Cross-file items -> residual_high {files, claim}. Return fix-log (files = pages you touched) + collapsed + ' +
  'extended + residual_high + summary.'].join('\n')
const critiquePrompt = (batch, i) => [PRE, READ_MANDATE(mapsFor(batch)), '', 'TASK: HOSTILE DOCTRINAL-CONFORMANCE + CAPABILITY-COMPLETENESS AUDIT + FIX IN PLACE over these ' +
  batch.length + ' pages (batch ' + i + ', audit EACH independently, fix EACH in place): ' + batch.map((p) => p.page).join(', ') + '. You are an ULTRA-HARSH, UNAGREEABLE auditor: ' +
  'assume a violation exists in EVERY fence until you prove otherwise, trust NOTHING the author or prose claims, treat dense confident code as the PRIME suspect for ' +
  'hollowness, and "good enough"/"mature"/a prior clean verdict is rejected outright. For EACH page run the MECHANICAL checklists line-by-line and REPAIR every hit in ' +
  'place (a fix, never a ledger note): (1) COLLAPSE_SCAN (12 signals, 3+ instances mandatory): sibling prefix/suffix names -> one modality-polymorphic entrypoint; same ' +
  'return rail differing only by arity -> input-shape discrimination; a get/get_many/get_by family -> one input-keyed entrypoint; functions differing only by a literal -> ' +
  'parameterize as policy; a bool selecting two bodies -> one derived body; a one-call hop -> delete it; a one-public-method class -> module function or fold-on-owner; ' +
  'parallel dispatch arms -> a `frozendict` table or fold; several types sharing fields -> one closed family; 3+ sibling constants -> one `frozendict`/`StrEnum`; a wrapper ' +
  'renaming a package API -> use the package directly; recurring 2-4 wrappers -> one aspect factory. (2) OWNER_CHOOSER — re-derive every shape from the 5 discriminants and ' +
  'replace any non-discriminant-correct owner; kill every parallel DTO, one-field wrapper, field-rename class, tag-only shape, and None-as-failure. (3) KNOB_TEST — delete ' +
  'each parameter the value already encodes; move every timeout/retry/deadline out of the signature into an aspect/`anyio` scope. (4) ASPECTS — every cross-cutting concern ' +
  'is a signature+rail-preserving STACKED decorator that never raises into domain flow; co-occurring wrappers collapse into one factory. (5) RAILS — narrowest carrier ' +
  'chosen once; CLOSED fault vocabulary (a bare `str` fault is a defect); accumulate-vs-abort correct; NO asyncio, hand-rolled retry, None-as-failure, or exception control ' +
  'flow in domain logic. (6) PAYLOADS/FROZENDICT/PEP — `closed=`/`extra_items=` `TypedDict` via module-level `TypeAdapter` + `Unpack`; `frozendict` builtin owns tables; ' +
  'PEP 585/604/695 only; total `match` + `assert_never`. (7) CAPABILITY-COMPLETENESS + ILLUSION — structural collapse and capability completeness are ORTHOGONAL: verify ' +
  'the body implements what the names/prose promise; ANY capability the reading-map `apiUnderutilized` / the both-tier admitted-package surface / the real domain / a ' +
  'consumer contract admits that the owner OMITS (a flat id/member set where the concept owns geometry/metrics/attributes/topology/operations; a 2-variant union where the ' +
  'domain has twenty; the obvious 3 keys where the concept carries fifteen) is a DEFECT — close it in place by growing the EXISTING owner, citing its source; delete ' +
  'speculative/padding/decorative additions. Enforce both-tier `.api` maximization, py3.15-modern typing, and prose + comment hygiene. EDIT to fix every hit. Return the ' +
  'batched fix-log (files = pages touched) + extended + residual_high — each a {files:[...], claim} object for CROSS-FILE items only.'].join('\n')
const redteamPrompt = (batch, i, crit) => [PRE, READ_MANDATE(mapsFor(batch)), '', 'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE over these ' + batch.length + ' pages ' +
  '(batch ' + i + ', red-team EACH independently, fix EACH in place): ' + batch.map((p) => p.page).join(', ') + '. You are the LAST and MOST AGGRESSIVE pass: assume the author and ' +
  'critique missed things and the chosen design is naive or illusory until PROVEN the strongest, burden of proof on the design. For EACH page: (A) COUNTERFACTUAL on the ' +
  'core owner/algebra/dispatch — rebuild to a fundamentally stronger form (denser owner / fold-derived-table / DEEPER admitted-package primitive — an `apiUnderutilized` ' +
  'member is the first place to look) if one exists; (B) ANTICIPATORY_COLLAPSE — the next case/dimension/modality lands as ONE declaration with every consumer untouched; ' +
  '(C) LONG-TAIL multi-dimensional attack (empty/singular/plural/stream/malformed/concurrent/cancelled/partial-failure/version-skew), accumulate-vs-abort correct, BOTH ' +
  'ingress AND egress parameterized; (D) BOUNDARY-INTEGRITY — a concern owned twice, a folder mixing concerns, a concern scattered across folders, or coupling to a sibling ' +
  'owner INTERIOR (vs its wire/seam) is a defect; the C# Rasm.Bim IFC boundary is law; (E) SURFACE-SPRAWL + PHANTOMS — an admitted package whose .api exposes capability ' +
  'the fence re-derives by hand, flat code below the operator depth the packages reach, a phantom .api member (delete it), or a thin wrapper: collapse to package depth and ' +
  'verify the member exists; (F) CAPABILITY-COMPLETENESS + ILLUSION — counterfactually attack the owner for DOMAIN-COMPLETENESS independently of how collapsed it looks; ' +
  'name any omitted capability (reading-map / domain / consumer) with a cite and EXTEND THE OWNER IN PLACE; conversely REJECT any flat-spam/speculative/parallel-surface ' +
  'extension. ALSO run a FULL COLD ADVERSARIAL RE-REVIEW of every conformance dimension with fresh hostile eyes. Even absent a structural rebuild the fence must end ' +
  'objectively denser, MORE CAPABLE, more correct than the critique left it; if the strongest form is genuinely present, prove it by finding nothing. CARRY FORWARD any ' +
  'unresolved cross-file residual from the critique result below and add your own. CRITIQUE RESULT:\n' + JSON.stringify((crit && { verdict: crit.verdict, residual_high: crit.residual_high || [] }) || {}, null, 1) +
  '\nReturn the batched fix-log (files = pages touched) + extended + residual_high — each a {files:[...], claim} object for CROSS-FILE items only.'].join('\n')
const reconcilePrompt = (cl) => [PRE, 'TASK: RECONCILE these cross-FILE residuals the build-out pass deferred. There is NO severity — treat EVERY residual as ' +
  'must-address. Your blast radius is WIDE: the whole libs/python/artifacts/.planning folder, the cross-folder libs/python siblings these seams touch (data/tabular, ' +
  'runtime, compute, geometry), the index docs (libs/python/artifacts/ARCHITECTURE.md [01]-[DOMAIN_MAP] + [02]-[SEAMS], README.md [01]-[ROUTER] + [02]-[DOMAIN_PACKAGES]), ' +
  'and the core/plan + core/receipt seam owners — read and fix ANY of them to keep seams consistent with the built pages. The C# wire seams (csharp:Rasm.Bim, ' +
  'csharp:Rasm.Persistence) are recorded as ALIGNED seams in the artifacts ARCHITECTURE [02]-[SEAMS], never coupled to a C# interior; fix a Python-side seam mismatch and ' +
  'record the wire contract, never edit C#. Read EVERY listed file. For each: if it is a real cross-file defect, FIX it in place (unify the shared type/seam/rail, add the ' +
  'depended-on case/field, repair the strata/boundary issue, update the index-doc maps for new domains/roster); if a residual is FACTUALLY INCORRECT, leave it and say why ' +
  'in the summary. Preserve all capability, regress nothing. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const verifyPrompt = (cl, fixFiles) => [PRE, 'TASK: RECONCILE VERIFY — CRITIQUE-GRADE + FIX IN PLACE, TERMINAL (no further spawning, no new rounds). A reconcile-fix ' +
  'agent claims to have resolved the cross-file residuals below. Re-read EVERY named file from disk and hold each claim to the CRITIQUE bar: is the fix the OBJECTIVELY ' +
  'BEST cross-file reconciliation — complete, dense, doctrine-conformant, BOTH seam endpoints aligned, index-doc maps truthful, all capability preserved — or is it ' +
  'LOOSE/WEAK/token/partial, and did it MISS a stronger reconciliation of these SAME files? Where the fix is loose/weak or a better reconciliation of these files exists, ' +
  'REPAIR it IN PLACE NOW to the objectively-best form — this is the LAST pass on these files, you FIX rather than defer. Then classify each claim: "fixed" (real, ' +
  'complete, objectively-best after your repair), "invalid" (the claim is provably wrong — cite why), or "open" (a GENUINE external blocker needing an architecture ' +
  'decision outside these files; describe it). Mark "open" ONLY for a true external blocker; a fix you could strengthen you STRENGTHEN, never mark open to punt. Do NOT ' +
  'surface new unrelated cross-file work and do NOT request another round — this pass is bounded to these files and is terminal. Claims:\n' + JSON.stringify(cl, null, 1) +
  '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TOPIC) { log('No topic resolved — pass args "aec" | "media" | "visual" (a string or {topic}). FOCUS_SETS keys: ' + Object.keys(FOCUS_SETS).join(', ')); return { root: ROOT, topic: null, total: 0 } }

phase('Plan')
const plan = await agent(planPrompt(), { label: 'plan', phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN_SCHEMA, stallMs: STALL })
const PAGES = [...new Map(((plan && plan.pages) || []).filter((p) => p && p.page && p.page !== BRIEF).map((p) => [p.page, { page: p.page, kind: p.kind || 'improve', absorb: p.absorb }])).values()]
const deletePages = [...new Set(((plan && plan.deletePages) || []).filter(Boolean))]
log('Plan[' + TOPIC + ']: ' + PAGES.length + ' pages (' + PAGES.filter((p) => p.kind === 'new').length + ' new, ' + PAGES.filter((p) => p.kind === 'rebuild').length + ' rebuild, ' +
  PAGES.filter((p) => p.kind === 'improve').length + ' improve), ' + deletePages.length + ' delete; CAP=' + CAP)
if (!PAGES.length) { log('No pages resolved for topic ' + TOPIC); return { root: ROOT, topic: TOPIC, total: 0 } }

// --- [DISCOVER]
phase('Discover')
const discovered = (await pool(chunk(PAGES, DISCOVER_BATCH), CAP, (batch, i) => agent(discoverPrompt(batch), { label: 'discover:b' + i, phase: 'Discover', model: 'opus', effort: 'max', schema: DISCOVER_SCHEMA, stallMs: STALL }))).filter(Boolean)
MAP_BY_PAGE = new Map(discovered.flatMap((d) => (d.worklist || []).filter((w) => w && w.page).map((w) => [w.page, w])))
log('Discover: reading map for ' + MAP_BY_PAGE.size + '/' + PAGES.length + ' pages across ' + discovered.length + ' batch(es)')

// --- [IMPLEMENT]
phase('Implement')
const built = (await pool(chunk(PAGES, IMPL_BATCH), CAP, (batch, i) => agent(implementPrompt(batch), { label: 'impl:b' + i + ':' + folderOf(batch[0].page), phase: 'Implement', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [CRITIQUE]
// --- [REDTEAM]
phase('Critique')
phase('Redteam')
const reviewed = (await pool(chunk(PAGES, REVIEW_BATCH), CAP, async (batch, i) => {
  const crit = await agent(critiquePrompt(batch, i), { label: 'critique:b' + i, phase: 'Critique', schema: REVIEW_SCHEMA, effort: 'xhigh', stallMs: STALL })
  const redteam = await agent(redteamPrompt(batch, i, crit), { label: 'redteam:b' + i, phase: 'Redteam', schema: REVIEW_SCHEMA, effort: 'max', stallMs: STALL })
  return { batch, crit, redteam }
})).filter(Boolean)

// --- [RECONCILE]
const inLibs = (p) => typeof p === 'string' && (p.startsWith('libs/') || p.indexOf('/libs/') !== -1)
const norm = (x, page) => { const files = Array.isArray(x.files) ? x.files.filter(inLibs) : []; return { files: files.length ? files : [page], claim: x.claim } }
const allRes = []
for (const r of built) if (r && r.residual_high) for (const x of r.residual_high) allRes.push(norm(x, (r.files && r.files[0]) || ROOT))
for (const r of reviewed) for (const k of ['crit', 'redteam']) { const l = r[k]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, (l.files && l.files[0]) || ROOT)) }
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
const buckets = packClusters(clusters, RECON_CAP)
log('Build-out[' + TOPIC + ']: ' + built.length + ' implement batch(es), ' + reviewed.length + ' review batch(es); reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters -> ' + buckets.length + ' bucket(s) (cap ' + RECON_CAP + ', ONE encompassing pass, no re-entry)')
let reconciled = []
if (buckets.length) {
  phase('Reconcile')
  reconciled = (await pool(buckets, RECON_CAP, async (bucket, i) => {
    const fix = await agent(reconcilePrompt(bucket), { label: 'reconcile-fix:' + i, phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: STALL })
    if (!fix) return null
    const verify = await agent(verifyPrompt(bucket, fix.files), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: STALL })
    return { fix, verify }
  })).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const surfaced = reconciled.flatMap((r) => (r.fix && r.fix.residual_high) || []).map((x) => norm(x, ROOT))
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
const hard_residual = dedup([...uniq.filter((r) => openClaims.has(r.claim)), ...surfaced])
log('Reconcile[' + TOPIC + ']: ' + buckets.length + ' bucket(s) reconciled (<=6 fix + <=6 verify, no re-spawn); ' + hard_residual.length + ' hard residual -> resolve-residuals, ' + dropped.length + ' dropped as invalid')
return { root: ROOT, topic: TOPIC, pages: PAGES.length, kinds: { new: PAGES.filter((p) => p.kind === 'new').length, rebuild: PAGES.filter((p) => p.kind === 'rebuild').length, improve: PAGES.filter((p) => p.kind === 'improve').length }, deletePages: deletePages, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped }
