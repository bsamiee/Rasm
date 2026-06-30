export const meta = {
  name: 'tmp-artifacts-rebuild',
  whenToUse: 'The comprehensive capstone deep pass over the whole libs/python/artifacts corpus after the three build-outs land.',
  description: 'General capstone: a full 7-phase hostile deep pass over the entire libs/python/artifacts/.planning corpus after the three build-outs complete it. Discover (regional, 4 opus agents decide the final page set + which pages are still weak) -> Rebuild (1 agent/file, max, only weak pages) -> Reconcile (union-find cross-folder residuals -> fix -> verify) -> Critique (batched 4, xhigh: docs/stacks/python conformance + both-tier .api ultra-stacking + no phantoms) -> Redteam (batched 4, max: folder-scope collapse/counterfactual + package rationalization) -> Full-Sweep (batched, max: line-by-line, no naive vocabularies, deepen cross-file stacking, resolve prior residuals) -> Resolve (terminal no-defer fix->verify loop until dry; hard residuals hand off to resolve-residuals). Reads the campaign brief libs/python/artifacts/.planning/_REBUILD_BRIEF.md. Temporary campaign workflow; takes no args.',
  phases: [
    { title: 'Discover', detail: 'regional (4 opus agents): list + classify every page weak/strong, decide the final page set' },
    { title: 'Rebuild', detail: 'per WEAK page (1 agent/file, max): hostile ground-up rebuild only where still weak' },
    { title: 'Reconcile', detail: 'union-find cluster cross-file residuals -> fix(max) -> adversarial verify(xhigh), cross-folder blast radius' },
    { title: 'Critique', detail: 'batched (4 pages/agent, xhigh): docs/stacks/python conformance + both-tier .api ultra-stacking + no phantoms, fix in place' },
    { title: 'Redteam', detail: 'batched (4 pages/agent, max): folder-scope collapse/counterfactual + package rationalization, fix in place' },
    { title: 'Full-Sweep', detail: 'batched (max): line-by-line, no naive vocabularies, deepen cross-file stacking, resolve prior residuals' },
    { title: 'Resolve', detail: 'terminal no-defer fix->verify loop until dry; hard residuals -> resolve-residuals' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const CRIT_BATCH = 4
const SWEEP_BATCH = 7
const RESOLVE_ROUNDS = 3
const ROOT = 'libs/python/artifacts/.planning'
const BRIEF = ROOT + '/_REBUILD_BRIEF.md'
const REGIONS = [
  { name: 'doc-core-exchange', prefixes: ['document/', 'core/', 'exchange/', 'specification/', 'delivery/'] },
  { name: 'visual-graphic-type', prefixes: ['visualization/', 'graphic/', 'typography/', 'drawing/'] },
  { name: 'compose-export-package', prefixes: ['composition/', 'export/', 'package/'] },
  { name: 'media-scene', prefixes: ['media/', 'scene/'] },
]

// --- [INPUTS] ----------------------------------------------------------------------------

// whole-folder capstone; args are ignored (campaign-locked target)

// --- [MODELS] ----------------------------------------------------------------------------

const REGION_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages'], properties: { pages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'weak'], properties: { page: { type: 'string' }, weak: { type: 'boolean' }, why: { type: 'string' } } } }, note: { type: 'string' } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const REVIEW_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, extended: { type: 'string' }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'Rasm monorepo, libs/python/artifacts planning corpus (markdown specs of intended Python module designs). CLAUDE.md manifest law governs. DENSITY BAR: ' +
    'docs/stacks/python/ (README/language/shapes/iteration/surfaces-and-dispatch/rails-and-effects/concurrency/boundaries/algorithms/system-apis/runtime) — author Python ' +
    'as dense, polymorphic, and rich as that bar. Cite ONLY members confirmed in the .api catalogs — and MINE BOTH TIERS fully: the shared/universal branch catalogs ' +
    '`libs/python/.api/*.md` AND the folder-specific `libs/python/artifacts/.api/*.md`. Maximize the shared/universal catalogs wherever relevant, never only the folder set.',
  'This is a FUNDAMENTAL GROUND-UP REBUILD of a planning-stage DESIGN PAGE, not a polish pass. Improve the page objectively: collapse surfaces/types, deepen bleeding-edge ' +
    'spellings, maximize admitted-library capability, AND close the concept capability gaps.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a REPORT of edits ALREADY MADE, ' +
    'never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE items (report those in residual_high).',
].join('\n')
const CAMPAIGN = [
  'CAMPAIGN CAPSTONE — READ ' + BRIEF + ' FIRST as binding scope. The three build-outs (AEC documentation plane, temporal media, visual authoring & diagramming) have ' +
    'completed the corpus; THIS is the comprehensive final deep pass over the WHOLE folder, holding the highest bar of the campaign. UNIFIED TELOS: artifacts is a ' +
    'publication/print-production engine that is SIMULTANEOUSLY the foundation of a high-end AEC documentation engine — one body, not two arms; grade every owner against ' +
    'BOTH planes at once and the bar is their UNION. The brief fixes the admit/replace package roster (categorical-best per concern, NO overlaps, supersede-not-accrete — ' +
    'this pass is the place to rationalize the package set and flag every residual overlap for the final pyproject reconciliation), the new-domain structure + the 7-page ' +
    'media restructure + the capability-detection media-filter contract, the AEC owned-vocabulary set (ISO 128/129-1/5455/5457/7200/13567/3098, NCS, MasterFormat/' +
    'UniFormat/OmniClass), the entry/receipt-seam unification target (ONE core/plan production entry, ONE core/receipt family — every domain contributes a CASE), and the ' +
    'C#-side out-of-scope boundary (Rasm.Bim owns IFC).',
  'COVERAGE OVER SIZE: there is NO line budget and the prior ~450-LOC look is DROPPED — a fence is exactly as large as its owned concern requires (doctrine fences run ' +
    '3-4x denser than ordinary code). Grade capability COVERAGE against the full domain + both-tier package surface, never byte count.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage is HOSTILE: assume the existing fence is NAIVE, SHALLOW, JUNIOR, or ILLUSORY until it survives an aggressive attack; the burden of ' +
    'proof is ON THE CODE, never on you. "Mature", "already strong", "good enough", "done", and a prior `clean` verdict are REJECTED self-assessments. Default to "this ' +
    'fence is naive and must be rebuilt to the strongest form the doctrine admits" and MAKE that rebuild; a no-edit verdict is reached ONLY after a genuinely aggressive ' +
    'attack on the real domain + the verified both-tier package surface finds nothing. Reject "good enough" categorically.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code is the code that PRETENDS to be advanced: it uses the doctrine vocabulary, cites packages, reads ' +
    'dense and confident — yet is HOLLOW. Treat dense, confident-looking fences with MORE suspicion, and DISBELIEVE every claim the page makes about itself until you ' +
    'verify it against the real domain and the catalogued both-tier package surface. HUNT: a name/signature/prose that PROMISES capability the body does not implement; a ' +
    '"rich" owner that is a thin slice of its concept; decorative density carrying no real capability; a placeholder dressed as a finished design; a `.api` member cited ' +
    'but never verified (a phantom). Every such illusion is a DEFECT to rebuild; never invent churn to look busy.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — MANDATORY FULL READ BEFORE ANY EDIT: docs/stacks/python/ is the route-owned law. STEP 0 — LIST THE DIRECTORY FIRST: enumerate ' +
    '`docs/stacks/python/` AND `docs/stacks/python/.api/` with a real `ls`/`find` to obtain the COMPLETE doctrine + catalog inventory before reading, so not one page is ' +
    'silently skipped and the routing order is taken from the source of truth. THEN READ EVERY doctrine page IN FULL — top-to-bottom, never a partial/skim/grep-jump — ' +
    'STARTING AT THE README and proceeding in its `[01]-[ATLAS]` routing order: README (laws + COLLAPSE_SCAN + RULE_ENFORCEMENT + PAGE_CRAFT + CORPUS_LAW), then language.md, ' +
    'shapes.md, iteration.md, surfaces-and-dispatch.md, rails-and-effects.md, concurrency.md, boundaries.md, algorithms.md, system-apis.md, runtime.md. READ EVERY ROOT ' +
    '`*.md` THE STEP-0 `ls` RETURNS AND FULLY INTERNALIZE IT BEFORE ANY EDIT — this enumeration is the floor, not the ceiling; the sub-folders `domain/` and `numerics/` ' +
    'are OUT of this read. A partial, sampled, or skipped read of this doctrine is itself a process defect. docs/stacks/csharp/ is the density/ambition FLOOR — match its ' +
    'richness, never import C#-shaped idioms.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows `Raw -> Payload -> Canonical owner -> Rail -> Projection -> Egress`. Raw material is admitted EXACTLY ONCE into ' +
    'an evidence-carrying owner (Pydantic/`TypedDict` payload at ingress); interior code never re-validates, never sees `None`-as-failure, sentinels, or provider shapes; ' +
    'egress projects outward (`msgspec.Struct` wire) from the canonical owner. Parameterize BOTH ingress AND egress so the same owner sources and sinks across many ' +
    'providers/apps without touching its interior.',
  'SHAPE LAW: one concept owns exactly ONE type (SHAPE_BUDGET) — variants are cases in one closed family, never sibling types; one rich polymorphic surface over many ' +
    'shallow (DEEP_SURFACES); the owner is shaped for the family it will ABSORB (ANTICIPATORY_COLLAPSE). Choose each owner by the OWNER_CHOOSER discriminants — admission, ' +
    'identity regime, variant arity, payload timing, openness -> the right owner among `TypedDict`, Pydantic, `msgspec.Struct`, frozen dataclass, rich class, ' +
    '`StrEnum`/`Literal`, `sentinel`, `Option`/`Result`, `frozendict`/`Map`/`tuple`, `Protocol`. A misplaced shape traces to one mis-answered discriminant.',
  'ASPECT-FIRST (DEFINITION_TIME_ASPECTS): every CROSS-CUTTING capability — retry, telemetry/spans, validation, contracts, memoization, registration, receipts, fault ' +
    'rails — is a SIGNATURE- and RAIL-PRESERVING decorator (inline `**P` + `functools.wraps`) that materializes policy, STACKS in deterministic order, and NEVER raises ' +
    'into domain flow. Two-to-four wrappers that always co-occur collapse into ONE parameterized aspect factory. Code reads as STACKED DECORATORS over a thin pure core.',
  'DERIVATION + ARITY: cases sharing generative structure are DERIVED — one primary `frozendict` correspondence declared, every secondary map derived from it, or a ' +
    'fold/comprehension — never enumerated arms. Configuration enters as ONE behavior-carrying value (POLICY_VALUES). ONE entrypoint owns every modality, discriminating on ' +
    'the INPUT SHAPE (`T | Iterable[T]` normalized once); a `timeout`/`retry`/`deadline` is an aspect or an `anyio` scope, never a signature param (KNOB_TEST).',
  'RAILS (rails-and-effects): the narrowest carrier that states the outcome, chosen ONCE at admission — `Option[T]` non-failing absence, `Result[T, E]` typed fallibility, ' +
    '`effect.result` do-notation, `Block`/`Map` immutable traversal, an `anyio` task group as the failure boundary (NEVER `asyncio.gather`), `stamina.retry` as the ' +
    'decorator. The fault type `E` is a CLOSED vocabulary — NEVER a bare `str` for a multi-cause domain. Accumulate-vs-abort fixed at the boundary: `map2`/accumulating-fold ' +
    'for independents, `bind` short-circuit for dependents. Cancellation is not failure; cleanup is `AsyncExitStack` + a shielded scope.',
  'STACK .api CAPABILITY (load-bearing): FIRST inventory the COMPLETE catalog set — BOTH the shared/universal `libs/python/.api/*.md` rails (anyio, expression, msgspec, ' +
    'pydantic, pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*) AND every folder catalog at `libs/python/artifacts/.api/*.md` — mine them ' +
    'for the full ADVANCED surface and how packages STACK. List BOTH `.api/` tier DIRECTORIES in full and DIFF against what the page cites: a relevant admitted catalog left ' +
    'unadopted is a DEFECT. Compose EVERY relevant admitted library into single dense operations woven as ONE rail, layering the shared/universal rails ON TOP OF the ' +
    'folder-specific domain packages. Use the DEEPEST primitive each package reaches (LIBRARY_DEPTH); reject surface-level subsets and thin rename wrappers.',
  'PRESERVE all capability (densify, never delete functionality). Where a page is already dense, refine; where it is flat/naive, rebuild ground-up. Never regress ' +
    'correctness or boundary law.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — structural collapse and both-tier `.api`-stacking are NECESSARY but NOT SUFFICIENT. A page can be fully ' +
    'collapsed into one closed family/ADT and STILL be capability-thin: a flat id/member set where the concept owns geometry, metrics, attributes, topology, and ' +
    'operations; a 2-variant `@tagged_union` where the domain has twenty; the obvious 3 keys where the concept carries fifteen. Structural completeness and CAPABILITY ' +
    'completeness are ORTHOGONAL. A FULL rebuild ALSO closes the capability gap so the page OWNS ITS DOMAIN CONCEPT COMPLETELY: every real missing concern lands as a CASE ' +
    'in the existing closed family, a ROW on the existing `frozendict` table, a FIELD on the existing model, an OPERATION on the existing surface, or a POLICY_VALUE on the ' +
    'existing vocabulary — NEVER a parallel type, a new file, a sibling shape, or flat appended code.',
  'GAP SOURCES (every extension MUST cite exactly one): (a) PACKAGE — a member the admitted package surface exposes that the concept ADMITS but the page IGNORES (BOTH ' +
    'tiers; verify the member exists). (b) DOMAIN — an attribute, metric, sub-kind, relationship, state, or operation the REAL concept demands but the page omits. (c) ' +
    'CONSUMER — a contract a sibling or downstream owner will require that has no composed spelling here yet.',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy — capability COVERAGE against the full domain + both-tier package surface is the real measure. A SMALL page modeling a ' +
    'rich concept is almost always under-built; a LARGE well-collapsed page can still be capability-SPARSE. Assess each owner against its domain independently of size and ' +
    'EXTEND every owner the concept under-realizes IN PLACE — integrated and unified into the one owner at full operator depth.',
  'JUSTIFIED, NOT RANDOM: if after a real domain + package + consumer sweep the concept is genuinely complete, prove it by adding nothing — never invent capability to ' +
    'look busy. Every added case/row/field/operation is load-bearing, cites a source, and composes the existing rails; preserve ALL existing capability.',
].join('\n')
const MECHANICS = [
  'MECHANICAL EXECUTABILITY — a design-page fence is a SIGNATURE-AND-IMPLEMENTATION CONTRACT, never a sketch: every fence MUST parse under the active py3.15 surface AND ' +
    'type-check against the REAL cross-page canonical owners it imports (CORPUS_LAW). Mentally COMPILE and TYPE-CHECK each fence before accepting it. Find each class below ' +
    'BY NAME and FIX it in place by growing the EXISTING owner, never a new file:',
  'FENCE-PARSES — every `match`/structural pattern, `for`-target, comprehension, and t-string parses; an OR-pattern binding DIFFERENT names, an invalid unpacking/starred ' +
    'target, or a malformed pattern is a NON-COMPILING fence and an automatic rebuild target.',
  'MODEL-COHERENCE (CORPUS_LAW) — every attribute/field/case-tag/method/imported-symbol a fence reads off a canonical owner declared on ANOTHER page MUST exist on the real ' +
    'declaration; verify each cross-owner read, reconcile to the ONE canonical name, never invent a field, surface an un-reconcilable name as a residual.',
  'TOTAL-DISPATCH — `assert_never(unreachable)` is valid ONLY when every member of the FULL closed family is handled before it; a parallel dispatch map keyed by a closed ' +
    'family must be TOTAL. A partial `match`/map dressed as total is a DEFECT.',
  'SINGLE-FACT EVIDENCE (STATE_RECEIPTS; BYTE_IDENTITY) — the bytes, content key, and receipt evidence derive from ONE computed fact stored once; the receipt/contribute ' +
    'path READS the stored fact, never re-renders. A path recomputing a render/placement to mint receipt evidence is a DOUBLE-RENDER defect.',
  'LOOP-OFFLOAD (OFFLOAD_LANE) — synchronous CPU-bound or GIL-hostile provider work (rendering, parsing, native FFI sweeps, codec encode) crosses on exactly one arm — ' +
    '`anyio.to_thread.run_sync` / `to_interpreter.run_sync` / `to_process.run_sync` — bounded by an explicit `CapacityLimiter`; never on the event loop nor as the arg ' +
    'expression before the offload.',
  'HANDLE-LIFETIME (CAPSULE_OWNER) — every native/FFI handle a provider opens closes DETERMINISTICALLY through `AsyncExitStack.enter_async_context`, a `with` bracket, or a ' +
    'capsule registering release via `weakref.finalize` under a shielded teardown — never left for the GC. An opened handle with no deterministic close is a LEAK defect.',
  'BINARY-KERNEL (CAPSULE_OWNER) — a multi-megabyte binary mutated across N steps is ONE imperative measured kernel threading ONE owned handle mutated in place, NOT a fold ' +
    'rebinding the whole buffer per step; it lives in the shielded resource bracket, returns the rail `Result`, and carries one `# Exemption:` line.',
  'IDENTITY-REGIME (MEMO_KEY) — a content-addressed key indexes by CONTENT; where an index/diff must distinguish identical-content siblings the key joins a STRUCTURAL ' +
    'discriminant to the content digest. A content-only key under a structural index is a CORRUPTION defect.',
  'TEMPLATE-SAFETY (TEMPLATE_STRUCTURE_SITE) — structured-text/markup egress (SVG, XML, Typst, HTML, query strings) from dynamic input uses PEP 750 t-strings / ' +
    '`string.templatelib.Template` processors or a structured builder (`xml.etree.ElementTree`), NEVER f-string interpolation with a hand-rolled escape.',
  'STREAM-OVER-MATERIALIZE (LAZY_COMBINATORS) — a large/unbounded extraction (every word of a 500-page document, every frame of a video) is a lazy `itertools`/generator ' +
    'pipeline or `yield from` fusion typed `Iterator[T]`, never an eagerly allocated whole-result `tuple`/`Block` held in RAM.',
  'NO-EXCEPTION-HOTLOOP (EXPRESSION_SPINE) — a per-element `try`/`except` driving control flow inside a fold over a large collection is BOTH a domain-logic violation AND a ' +
    'throughput defect: a total predicate or non-raising `Option`-returning parse replaces the per-element raise; the boundary `catch` stays at the boundary.',
  'DERIVED-NOT-PARALLEL + PER-MODE PAYLOADS (DERIVED_LOGIC) — a secondary map hand-synced to a primary is a DERIVATION defect: declare ONE primary correspondence and ' +
    'DERIVE every secondary by comprehension. A monolithic typed bag whose fields are irrelevant for most modes is a permissive-bag DEFECT: collapse into a discriminated ' +
    'per-mode `@tagged_union` whose each case carries ONLY its own fields — WITHOUT splitting the owner into new files.',
].join('\n')
const PATLAW = [
  'PY-VERSION LAW: target Python 3.15 on the full modern band — advanced patterns ONLY, zero legacy idioms, IDENTICAL conventions across every folder.',
  'NEVER write `from __future__ import annotations`. NEVER use legacy typing: PEP 585 builtin generics NOT `typing.List/Dict/Tuple/Set`; PEP 604 unions NOT ' +
    '`Optional`/`Union`; PEP 695 type parameters NOT `TypeVar` + `Generic`. Use `Self`, `override`, `TypeIs`/`TypeGuard`, `assert_never`, `ReadOnly`, `TypedDict` + ' +
    '`NotRequired`/`Required`, `LiteralString`, `enum.StrEnum`/`IntEnum`, and `@dataclass(slots=True, frozen=True)` or `msgspec.Struct`/pydantic models where each best fits.',
  'PAYLOADS — NEWEST FORM: ingress payloads are static `TypedDict` contracts with `closed=True` or `extra_items=T` and per-key `Required[]`/`NotRequired[]`/`ReadOnly[T]`, ' +
    'admitted through a module-level `TypeAdapter`, with `Unpack[TypedDict]` at root keyword entrypoints; `msgspec.Struct(frozen=True)` owns wire/egress. NO `dict[str, ' +
    'Any]` bags, homogeneous `**kwargs`, or `Mapping[str, object]` payloads.',
  'FROZENDICT (py3.15 builtin): `from builtins import frozendict` owns immutable map rows, dispatch/policy TABLES (one primary, secondary derived), and immutable evidence ' +
    '— REJECT `MappingProxyType`, a module-level mutable `dict` used as a table, tuple-pair pseudo-maps, and mutate-then-freeze. Prefer total `match` over if-chains; ' +
    'PEP 750 t-strings are MANDATORY for all dynamic or untrusted structured-text and markup egress. Keep every choice CONSISTENT across folders.',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package/folder owner strictly in its lane; internal code uses canonical names and shapes with mapping only at the edge; do ' +
  'not trample a sibling owner while densifying; respect the dependency direction of the workspace strata. The C# boundary is law: Rasm.Bim owns IFC semantics; the ' +
  'artifacts corpus composes QTO/schedule/credential facts at the wire, never re-authoring the IFC model.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling rule/contract; one idea ' +
    'per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness disclaimers, report framing, or empty hedges. Prefer a ' +
    'table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. Prose that ASSERTS capability the fence does not implement ' +
    'is a defect, not content.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in backticks. Name the exact member/type/rail ' +
    'in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical section-divider headers (language-comment ' +
  'marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY where intent is not already obvious from names, types, and ' +
  'signatures: default to ZERO comments on self-evident code; at most 1 line where a comment genuinely earns its place. NO restating the code, no narration, no ' +
  'task/process/session/history/proof/review comments, no docstring bloat.'

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
const clustersOf = (residuals) => {
  const uniq = [...new Map(residuals.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return { uniq, clusters: [...by.values()] }
}
const folderOf = (p) => { const head = p.split('/.planning/')[0].split('/'); return head[head.length - 1] || 'root' }
const subOf = (p) => p.split('/.planning/').pop()
const PRE = [LAW, '', CAMPAIGN, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', MECHANICS, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, ''].join('\n')
const regionPrompt = (rg) => [LAW, '', CAMPAIGN, '', ADVERSARIAL, '', 'TASK: REGIONAL DISCOVERY over the artifacts region `' + rg.name + '` (the sub-folders ' + JSON.stringify(rg.prefixes) +
  ' under ' + ROOT + '/). Read ' + BRIEF + ' for the intended structure. List EVERY design page under those sub-folders (repo-relative *.md, EXCLUDING IDEAS.md/TASKLOG.md/' +
  'README.md/ARCHITECTURE.md and the _REBUILD_BRIEF.md). For EACH page, READ it and classify `weak` true|false under the HOSTILE stance: weak = naive/illusory/hollow, ' +
  'under-collapsed, capability-thin against its domain, mis-stacked on the .api tiers, or carrying any mechanical/model-coherence defect — DEFAULT to weak unless the page ' +
  'genuinely survives an aggressive read; strong = only a page that is already world-class. Also decide the FINAL page set for this region (flag a page that should merge ' +
  'into a sibling or split, in `why`). Use find/ls + read; do NOT cd; do NOT edit anything. Return pages (each {page, weak, why}) + an optional region note.'].join('\n')
const rebuildPrompt = (page) => [PRE, 'TASK: HOSTILE GROUND-UP REBUILD of ' + page + ' to the doctrine AND to domain-complete capability — this page was flagged STILL WEAK ' +
  'after the build-outs. DISBELIEVE the page; assume every fence is naive, junior, or illusory until proven world-class; do NOT polish, REBUILD to the strongest form the ' +
  'doctrine admits. Read the page, its sibling pages, the operative docs/stacks/python/ pages, and BOTH .api tiers; MINE the full advanced surface, ADD any shared/' +
  'universal rail the page under-uses, STACK both tiers; VERIFY every cited member (a phantom is deleted). Construct in LIFECYCLE order, collapse parallel shapes into one ' +
  'closed family/ADT, drive cases with a derived `frozendict` table or fold, one polymorphic entrypoint per modality, weave cross-cutting concerns as STACKED aspects over ' +
  'a thin pure core, parameterize BOTH ingress and egress. CLOSE THE CONCEPT CAPABILITY GAPS by extending the EXISTING owner in place (case/row/field/operation/' +
  'policy-value), each addition citing a package member / domain attribute / consumer contract. Wire into core/plan + core/receipt as a CASE. py3.15-modern only; ' +
  'high-signal all-backticked prose. Report `collapsed` + `extended`; verdict `rebuilt` unless the fence genuinely survived untouched. Return the fix-log + residual_high ' +
  '— each a {files:[...], claim} object for CROSS-FILE items only.'].join('\n')
const critiquePrompt = (pages) => [PRE, 'TASK: BATCHED HOSTILE DOCTRINAL-CONFORMANCE AUDIT + FIX IN PLACE over these pages (audit EACH independently, fix EACH in ' +
  'place):\n' + JSON.stringify(pages, null, 1) + '\nYou are ULTRA-HARSH: assume a violation exists in EVERY fence until proven otherwise, trust NOTHING the prose claims. ' +
  'For EACH page run the mechanical checklists line-by-line: (1) COLLAPSE_SCAN (12 signals, 3+ instances mandatory), (2) OWNER_CHOOSER (5 discriminants, replace any ' +
  'non-correct owner, kill parallel DTOs/one-field wrappers/None-as-failure), (3) KNOB_TEST (delete encoded params, move timeout/retry/deadline to aspects), (4) ASPECTS ' +
  '(signature+rail-preserving stacked decorators that never raise into domain flow), (5) RAILS (narrowest carrier once, CLOSED fault vocabulary, accumulate-vs-abort ' +
  'correct, NO asyncio/hand-rolled retry/None-failure/exception-control-flow), (6) PAYLOADS/FROZENDICT/PEP (`closed=`/`extra_items=` `TypedDict` via `TypeAdapter` + ' +
  '`Unpack`, `frozendict` builtin tables, PEP 585/604/695, total `match` + `assert_never`). CRITICAL FOR THIS CAPSTONE: (a) docs/stacks/python conformance to the LETTER, ' +
  '(b) BOTH-TIER .api ULTRA-STACKING — diff the FULL shared `libs/python/.api` + folder `libs/python/artifacts/.api` inventory against what each page cites and adopt every ' +
  'relevant unadopted catalog to depth, (c) NO PHANTOMS — every cited member verified against the .api tiers, unverifiable members deleted, (d) CAPABILITY-COMPLETENESS + ' +
  'ILLUSION — close any omitted domain/package/consumer capability by growing the EXISTING owner, delete speculative/decorative padding. REPAIR every hit in place. Return ' +
  'the batched fix-log (files = pages touched) + residual_high — each a {files:[...], claim} object for CROSS-FILE items only.'].join('\n')
const redteamPrompt = (pages) => [PRE, 'TASK: BATCHED ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE over these pages (red-team EACH independently, fix EACH in ' +
  'place):\n' + JSON.stringify(pages, null, 1) + '\nYou are the MOST AGGRESSIVE pass: assume the design is naive or illusory until PROVEN the strongest, burden of proof on ' +
  'the design. For EACH page: (A) COUNTERFACTUAL on the core owner/algebra/dispatch — rebuild to a fundamentally stronger form (denser owner / fold-derived-table / DEEPER ' +
  'admitted-package primitive) if one exists; (B) ANTICIPATORY_COLLAPSE — the next case/dimension/modality lands as ONE declaration; (C) LONG-TAIL multi-dimensional attack ' +
  '(empty/singular/plural/stream/malformed/concurrent/cancelled/partial-failure/version-skew), accumulate-vs-abort correct, BOTH ingress AND egress parameterized; (D) ' +
  'BOUNDARY-INTEGRITY at FOLDER SCOPE — a concern owned twice across the folder, a folder mixing concerns, a concern scattered across folders, or coupling to a sibling ' +
  'INTERIOR (vs its wire/seam) is a defect; the C# Rasm.Bim IFC boundary is law; (E) SURFACE-SPRAWL + PHANTOMS — collapse hand-rolled code to package depth, delete ' +
  'unverifiable members; (F) PACKAGE RATIONALIZATION — enforce the brief categorical-best / no-overlap roster: where two packages own one concern, route through the ' +
  'categorical-best owner and FLAG the superseded package for the final pyproject reconciliation (in residual_high); (G) CAPABILITY-COMPLETENESS + ILLUSION — extend the ' +
  'owner in place for any omitted capability, reject flat spam. Hold the highest bar; every defect you raise you REPAIR. Return the batched fix-log + residual_high — each ' +
  'a {files:[...], claim} object for CROSS-FILE items only.'].join('\n')
const sweepPrompt = (pages) => [PRE, 'TASK: BATCHED FULL-SWEEP (max) over these pages — the terminal line-by-line densification pass before resolve (sweep EACH, fix EACH ' +
  'in place):\n' + JSON.stringify(pages, null, 1) + '\nThis is the deepest pass: read EACH page line-by-line and (1) PURGE NAIVE VOCABULARIES — no flat `str`/`int` where a ' +
  'closed `StrEnum`/`Literal`/`@tagged_union` owns the axis, no `dict[str, Any]` bag, no naive id/member set where the concept owns geometry/metrics/topology/operations; ' +
  '(2) DEEPEN CROSS-FILE STACKING — verify every seam to a sibling owner reads a real declared field/case, and stack the both-tier .api capability the page still under-' +
  'uses; (3) RESOLVE PRIOR RESIDUALS that fall within these pages; (4) re-run the full conformance lens (COLLAPSE_SCAN, OWNER_CHOOSER, KNOB_TEST, ASPECTS, RAILS, ' +
  'PAYLOADS/FROZENDICT/PEP, capability-completeness, illusion-hunt, prose + comment hygiene) with fresh hostile eyes. Even absent a structural rebuild each fence must end ' +
  'objectively denser, MORE CAPABLE, and more correct. REPAIR every hit in place. Return the batched fix-log (files = pages touched) + residual_high — each a {files:[...], ' +
  'claim} object for CROSS-FILE items only.'].join('\n')
const reconcileFixPrompt = (cl) => [PRE, 'TASK: RECONCILE these cross-FILE residuals. There is NO severity — treat EVERY residual as must-address. Your blast radius is ' +
  'WIDE: the whole libs/python/artifacts/.planning folder, the cross-folder libs/python siblings the seams touch (data/tabular, runtime, compute, geometry), the index ' +
  'docs (libs/python/artifacts/ARCHITECTURE.md [01]-[DOMAIN_MAP] + [02]-[SEAMS], README.md [01]-[ROUTER] + [02]-[DOMAIN_PACKAGES]), and the core/plan + core/receipt seam ' +
  'owners. The C# wire seams (csharp:Rasm.Bim, csharp:Rasm.Persistence) are recorded as ALIGNED seams, never coupled to a C# interior — fix the Python-side seam and record ' +
  'the wire contract, never edit C#. Read EVERY listed file. For each: if a real cross-file defect, FIX it in place (unify the shared type/seam/rail, add the depended-on ' +
  'case/field, update the index-doc maps, flag a package overlap for the final pyproject reconciliation); if FACTUALLY INCORRECT, leave it and say why. Preserve all ' +
  'capability. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const verifyPrompt = (cl, fix, tag) => [PRE, 'TASK: ADVERSARIAL VERIFY (' + tag + '), one verdict per claim. Read the named files from disk and classify each residual: ' +
  'status "fixed" (real defect, now genuinely resolved), "invalid" (the claim is factually wrong — cite why), or "open" (real defect still NOT resolved). Default "open" on ' +
  'any doubt; "invalid" only when provably wrong. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files)].join('\n')
const reviewResiduals = (results, acc) => { for (const r of results) if (r && r.residual_high) for (const x of r.residual_high) acc.push(typeof x === 'string' ? { files: [ROOT], claim: x } : { files: x.files && x.files.length ? x.files : [ROOT], claim: x.claim }) }

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Discover')
const regionResults = (await pool(REGIONS, CAP, (rg) => agent(regionPrompt(rg), { label: 'discover:' + rg.name, phase: 'Discover', model: 'opus', effort: 'high', schema: REGION_SCHEMA, stallMs: 300000 }))).filter(Boolean)
const allPageRows = regionResults.flatMap((r) => (r && r.pages) || []).filter((p) => p && p.page)
const ALL_PAGES = [...new Set(allPageRows.map((p) => p.page))]
const WEAK = [...new Set(allPageRows.filter((p) => p.weak).map((p) => p.page))]
log('Discover: ' + ALL_PAGES.length + ' pages across ' + regionResults.length + ' regions; ' + WEAK.length + ' weak -> rebuild; CAP=' + CAP)
if (!ALL_PAGES.length) { log('No pages discovered under ' + ROOT); return { root: ROOT, total: 0 } }

// --- [REBUILD]
phase('Rebuild')
const rebuilt = WEAK.length ? (await pool(WEAK, CAP, (p) => agent(rebuildPrompt(p), { label: 'rebuild:' + folderOf(p) + ':' + subOf(p), phase: 'Rebuild', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 }))).filter(Boolean) : []

// --- [RECONCILE]
const recResiduals = []
for (const r of rebuilt) if (r && r.residual_high) for (const x of r.residual_high) recResiduals.push(typeof x === 'string' ? { files: [ROOT], claim: x } : { files: x.files && x.files.length ? x.files : [ROOT], claim: x.claim })
const recC = clustersOf(recResiduals)
let recOpen = []
if (recC.clusters.length) {
  phase('Reconcile')
  const reconciled = (await pool(recC.clusters, CAP, async (cl, i) => {
    const fix = await agent(reconcileFixPrompt(cl), { label: 'reconcile-fix:' + i, phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 })
    if (!fix) return null
    const verify = await agent(verifyPrompt(cl, fix, 'reconcile'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
  const open = new Set(reconciled.flatMap((r) => (r.verify && r.verify.claims) || []).filter((c) => c.status === 'open').map((c) => c.claim))
  recOpen = recC.uniq.filter((r) => open.has(r.claim))
}
log('Rebuild: ' + rebuilt.length + '/' + WEAK.length + ' weak rebuilt; reconcile ' + recC.clusters.length + ' clusters -> ' + recOpen.length + ' open')

// --- [CRITIQUE]
phase('Critique')
const critResiduals = []
const critRes = (await pool(chunk(ALL_PAGES, CRIT_BATCH), CAP, (b) => agent(critiquePrompt(b), { label: 'critique', phase: 'Critique', schema: REVIEW_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)
reviewResiduals(critRes, critResiduals)

// --- [REDTEAM]
phase('Redteam')
const redteamResiduals = []
const redteamRes = (await pool(chunk(ALL_PAGES, CRIT_BATCH), CAP, (b) => agent(redteamPrompt(b), { label: 'redteam', phase: 'Redteam', schema: REVIEW_SCHEMA, effort: 'max', stallMs: 300000 }))).filter(Boolean)
reviewResiduals(redteamRes, redteamResiduals)

// --- [FULL_SWEEP]
phase('Full-Sweep')
const sweepResiduals = []
const sweepRes = (await pool(chunk(ALL_PAGES, SWEEP_BATCH), CAP, (b) => agent(sweepPrompt(b), { label: 'sweep', phase: 'Full-Sweep', schema: REVIEW_SCHEMA, effort: 'max', stallMs: 300000 }))).filter(Boolean)
reviewResiduals(sweepRes, sweepResiduals)

// --- [RESOLVE]
phase('Resolve')
let pending = [...recOpen, ...critResiduals, ...redteamResiduals, ...sweepResiduals]
let round = 0
let hard_residual = []
let prevOpen = Infinity
while (pending.length && round < RESOLVE_ROUNDS) {
  round++
  const { uniq, clusters } = clustersOf(pending)
  const resolved = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent(reconcileFixPrompt(cl), { label: 'resolve-fix:r' + round + ':' + i, phase: 'Resolve', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 })
    if (!fix) return null
    const verify = await agent(verifyPrompt(cl, fix, 'resolve r' + round), { label: 'resolve-verify:r' + round + ':' + i, phase: 'Resolve', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
  const open = new Set(resolved.flatMap((r) => (r.verify && r.verify.claims) || []).filter((c) => c.status === 'open').map((c) => c.claim))
  hard_residual = uniq.filter((r) => open.has(r.claim))
  log('Resolve round ' + round + ': ' + clusters.length + ' clusters -> ' + hard_residual.length + ' still open')
  if (hard_residual.length >= prevOpen) { log('Resolve made no progress this round; handing ' + hard_residual.length + ' residual(s) to resolve-residuals'); break }
  prevOpen = hard_residual.length
  pending = hard_residual
}
log('Capstone complete: ' + ALL_PAGES.length + ' pages, ' + WEAK.length + ' rebuilt; ' + hard_residual.length + ' hard residual after ' + round + ' resolve round(s) -> resolve-residuals')
return { root: ROOT, pages: ALL_PAGES.length, weak: WEAK.length, rebuilt: rebuilt.length, resolveRounds: round, hard_residual: hard_residual }
