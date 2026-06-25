export const meta = {
  name: 'reconcile-python-stack',
  description: 'WF3 of the docs/stacks/python doctrine rebuild (run LAST, after harden-python-core then design-python-domain). Whole-corpus finalization of the entire docs/stacks/python doc set (core + domain/ + numerics/ if built): a cold context-free grade of every page, a corpus-wide duplicate-snippet detection across the full set, a union-find reconcile of every cross-file residual + duplication collision, then a finalize re-grade of touched files to a zero-edit clean pass. Holds the highest bar: every snippet 13/10, agnostic, table-stakes-free, implicitly stacking up the README chain, each snippet earning its place with zero duplicated demonstrations, no contradictions. The csharp doc set is the READ-ONLY density reference; every edit is scoped to docs/stacks/python. Phases: Rediscover (re-parse the now-restructured atlas + every sub-README router) -> Cold-grade (1 cold agent/file, fix in place, report regions) -> Corpus-dedup (1 barrier agent over the full region ledger detects cross-file duplicated demonstrations) -> Reconcile (union-find over residuals + collisions -> fix(max) -> adversarial verify(xhigh)) -> Finalize-grade (re-grade touched files to clean). Takes no args.',
  phases: [
    { title: 'Rediscover', detail: 're-parse the core README atlas + every sub-README router to the current full ordered file set (core + domain/ + numerics/)' },
    { title: 'Cold-grade', detail: '1 cold context-free agent per file: hostile finalization grade against the doctrine + page-craft + the csharp bar, fix in place, report snippet regions' },
    { title: 'Corpus-dedup', detail: '1 barrier agent over the full region ledger: detect duplicated snippet demonstrations across the entire corpus' },
    { title: 'Reconcile', detail: 'union-find cluster cross-file residuals + duplication collisions -> fix(max) -> adversarial verify(xhigh); hard residuals hand off to resolve-residuals' },
    { title: 'Finalize-grade', detail: 're-grade only the files the reconcile touched, converging to a zero-edit clean pass' },
  ],
}

// --- [SCHEMAS] ---------------------------------------------------------------------------
const INVENTORY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'order'], properties: { path: { type: 'string' }, order: { type: 'integer' }, folder: { type: 'string' }, regions: { type: 'array', items: { type: 'string' } } } } } } }
const COLD_GRADE_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'grade', 'regions'], properties: { file: { type: 'string' }, grade: { type: 'string', enum: ['clean', 'edited'] }, regions: { type: 'array', items: { type: 'string' } }, residual_high: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }
const DEDUP_SCHEMA = { type: 'object', additionalProperties: false, required: ['collisions'], properties: { collisions: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['region', 'files', 'owner'], properties: { region: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, owner: { type: 'string' } } } } } }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [HARNESS] -- steady bounded pool: <=cap in flight AND a serialized launch gate --------
const STAGGER_MS = 1500
const STALL = 300000
const CAP = 12
const ROOT = 'docs/stacks/python'
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

// --- [MODELS] -- the shared doctrine blocks (identical to design-python-domain) ------------
const LAW = [
  'TARGET: docs/stacks/python/ is the route-owned Python CODE DOCTRINE — a doc set of AGNOSTIC teaching pages that legislate how all project Python is written. It is NOT a libs/python/.planning design corpus: a page teaches a coding LAW with one exemplary agnostic snippet, never a concrete module. The README owns routing + the 16 doctrine laws + the COLLAPSE_SCAN; each concept page owns ONE disjoint layer and states doctrine as fact. READ docs/stacks/python/README.md sections [02]-[DOCTRINE], [03]-[COLLAPSE_SCAN], [05]-[PAGE_CRAFT], [06]-[CORPUS_LAW] and hold them as law.',
  'QUALITY BAR is docs/stacks/csharp/ (the README + shapes + surfaces-and-dispatch + the domain/ shards) — READ-ONLY: NEVER edit a csharp file. Mirror its DENSITY — SHAPE_BUDGET, DEEP_SURFACES, MODAL_ARITY, ANTICIPATORY_COLLAPSE (one owner ready to replace 10+ loose things), POLICY_VALUES, OWNER_CHOOSER — in pure-Python idiom; NEVER import a C#/LanguageExt spelling (`[Union]`, `Fin`, `SmartEnum`) into a Python page. The csharp set is how dense the Python set must read, never how it must be spelled.',
  'WRITE-FULLY MANDATE, scoped to docs/stacks/python/** ONLY: every defect you identify you FIX NOW via Edit/Write directly in the file; the structured fix-log you return is a REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should hedge. Edit ONLY files under docs/stacks/python/; reading csharp/standards/.api files is allowed, editing anything outside docs/stacks/python/ is forbidden. Leave nothing behind except genuine cross-FILE items (report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage is HOSTILE: assume the page is NAIVE, SHALLOW, JUNIOR, or ILLUSORY until it survives an aggressive attack; the burden of proof is ON THE PAGE, never on you. A `finalized` state marker, "mature", "already strong", "good enough", and a prior `clean` grade are REJECTED self-assessments. Default to "this page is naive and must be rebuilt to the strongest form the doctrine admits" and MAKE that rebuild; a no-edit verdict is earned ONLY after a genuinely aggressive attack finds nothing.',
  'ILLUSORY / FAKE content is the PRIMARY target — the MOST dangerous content is what PRETENDS to be advanced: a snippet that READS dense and confident (uses `@tagged_union`/`frozendict`/the rails, cites packages) yet demonstrates a THIN slice; prose that ASSERTS richness the fence does not contain; a card field (`Use`/`Accept`/`Reject`/`Law`/`Boundary`) that decides nothing; a structurally-correct collapse that is semantically empty; a `.api` member cited but unverifiable (a PHANTOM — delete it). Treat dense, confident-looking fences with MORE suspicion, not less, and DISBELIEVE every claim the page makes about itself until verified.',
].join('\n')
const PYDOCTRINE16 = [
  'HOLD the README [02]-[DOCTRINE] 16 laws as fact, never restated on a concept page: [FLOW] EXPRESSION_SPINE (domain logic is expression-shaped; dependent steps `bind`, independent ones accumulate; statements survive only in a measured kernel that names the exemption) + BOUNDARY_ADMISSION (`Raw -> Payload -> Canonical owner -> Rail -> Projection -> Egress`; raw admitted EXACTLY ONCE into an evidence-carrying owner; interior never re-validates or sees `None`-as-failure/sentinel/provider shape). [SHAPE] SHAPE_BUDGET + DEEP_SURFACES + MODAL_ARITY + ANTICIPATORY_COLLAPSE. [DERIVATION] POLICY_VALUES + DERIVED_LOGIC + DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION.',
  'RUN the README [03]-[COLLAPSE_SCAN] 12-signal table on every fence: any signal triggers the collapse move, 3+ instances make it mandatory. A page that demonstrates a coding law must itself obey every law it can reach.',
].join('\n')
const SHAPE_ADT = [
  'EXTREME SHAPE/TYPE DENSITY (the central mandate): one concept owns exactly ONE type as a dense CLOSED-FAMILY ADT — `@tagged_union` / `Literal` set / `StrEnum` / `msgspec.Struct` / frozen dataclass / `frozendict` table — chosen by the OWNER_CHOOSER 5 discriminants (admission, identity regime, variant arity, payload timing, openness). KILL on sight: loose 1-2-field classes, parallel DTOs, field-rename wrappers, tag-only shapes, and 3+ sibling module constants/types for one concept (collapse to one `frozendict`/`StrEnum`/closed family). No loose/weak type or constant spam survives.',
  'ANTICIPATORY_COLLAPSE: shape the owner for the family it WILL absorb so the next case/dimension/modality lands as ONE declaration with every consumer untouched or broken loudly at type-check — one owner READY TO REPLACE 10+ loose things. The exemplary snippet MUST show the owner at that scale with the growth axis visible, not an isolated minimum.',
].join('\n')
const AOP_FIRST = [
  'FULL AOP (ULTRA-CRITICAL): every CROSS-CUTTING concern — retry, telemetry/spans, validation, contracts, memoization, registration, receipts, fault rails — is OUR OWN signature- AND rail-preserving decorator (inline `**P` + `functools.wraps`) STACKED over a THIN PURE CORE; it materializes policy, stacks in deterministic order, and NEVER raises into domain flow (a failing aspect returns the rail `Error`). The domain transform itself stays a pure function/fold under the stack.',
  'AOP is NOT merely applying `@beartype`/`@msgspec` decorators — though `beartype` (contracts) and `msgspec` (struct/codec) MUST be fully integrated. It means our code SHAPED TOWARD decorators: MAXIMIZE how much functionality is expressed AS stacked decorators; 2-4 co-occurring wrappers collapse into ONE parameterized aspect factory; a page reads as stacked decorators over a pure core, never inline-repeated concerns or sibling helper functions. A concern open-coded where a decorator belongs is a defect.',
].join('\n')
const PARAM_POLY = [
  'HEAVY PARAMETERIZATION, ZERO HARDCODING/FRAGILE LOGIC, FULL POLYMORPHISM. ONE entrypoint owns every modality — `T | Iterable[T]` normalized ONCE at the head, discriminating on input SHAPE (type/tag/pattern/arity), never a name suffix or a `mode`/`batch`/`strict` knob (KNOB_TEST: delete each parameter; if the value reconstructs what it carried, it was a knob to collapse).',
  'Configuration enters as ONE behavior-carrying value — a vocabulary member, a tagged variant, a frozen policy table (POLICY_VALUES) — never a flag set the body re-derives. A `timeout`/`retry`/`deadline` is an aspect or an `anyio` scope, never a signature param. Cases sharing generative structure are DERIVED from one primary `frozendict` correspondence (DERIVED_LOGIC), never enumerated arms.',
].join('\n')
const PY315 = [
  'BLEEDING-EDGE Python 3.15 ONLY: PEP 585 builtin generics, PEP 604 unions, PEP 695 type parameters (`class C[T]`, `def f[T]`, `type Alias[T] = ...`). NEVER `from __future__ import annotations`; NEVER legacy `typing.List`/`Optional`/`Union`/`TypeVar`+`Generic`. Use `Self`/`override`/`TypeIs`/`assert_never`/`ReadOnly`/`LiteralString`, total `match` + `assert_never` over closed unions, walrus where it tightens.',
  'PAYLOADS — newest form: ingress payloads are closed `TypedDict` (`closed=True` or `extra_items=T`, per-key `Required`/`NotRequired`/`ReadOnly`) admitted through a module-level `TypeAdapter`, with `Unpack[TypedDict]` at root keyword entrypoints (never forwarded through interiors); `msgspec.Struct(frozen=True)` owns wire/egress. `frozendict` (py3.15 builtin) owns immutable map rows, dispatch/policy TABLES, and evidence — REJECT `MappingProxyType`, a module-level mutable `dict` used as a table, tuple-pair pseudo-maps, mutate-then-freeze, `dict[str, Any]` bags, and homogeneous `**kwargs`.',
].join('\n')
const APISTACK_SUBSTRATE = [
  'CITATION TIER (load-bearing, doctrine-specific): the SHARED substrate catalogs at docs/stacks/python/.api/*.md — `expression`, `pydantic`, `pydantic-settings`, `beartype`, `msgspec`, `anyio`, `structlog`, `stamina`, `numpy`, `psutil`, `opentelemetry-*`, `protobuf`, `grpcio` — are cited by EVERY page. A CORE page cites ONLY this shared substrate. A DOMAIN/numerics page cites the shared substrate PLUS the package `.api/` catalogs (wherever they live under docs/stacks/python/.api/) for the packages its concept composes, and NOTHING outside its declared cites. Mine each to its FULL advanced surface and STACK them as ONE dense rail — `expression` rails + `msgspec`/`pydantic` models + `beartype` contracts + `stamina` retry + `structlog`+OTel spans + `anyio` concurrency layered together, never flat one-shot per-library uses.',
  'Cite ONLY members confirmed in the actual .api catalog file; a member you cannot verify is a PHANTOM to delete (verify novel members via `uv run python -m tools.assay api`). A domain page composes the FINALIZED core laws as settled material (it never re-opens admission/shape/rail/dispatch/boundary decisions the core owns) and owns ONE closed vocabulary for its axis. Use the DEEPEST primitive each package reaches (LIBRARY_DEPTH); flat code below that operator depth is surface sprawl.',
].join('\n')
const PAGECRAFT = [
  'PAGE-CRAFT LAW (README [05]-[PAGE_CRAFT]): page grammar is a NARROW index table, then deep FAMILY CARDS, then ONE agnostic snippet beside the rule it proves; the page ends at its last card. CARD ECONOMY: cards are few, deep, evidence-dense; near-peer cards MERGE until each owns a decision cluster; a card line carries exactly ONE decision; a `Use`/`Accept`/`Reject`/`Law`/`Boundary` field appears only where it decides something — a field that decides nothing is DELETED, not filled. Tables enumerate, cards legislate (rows stay atomic, no prose cramming, no links in cells).',
  'REJECT columns are LOAD-BEARING: every `Use` names the spelling, wrapper, or local pattern it DELETES. CODE NAMES BEFORE PROSE: every member a card or snippet names is verified against the installed package before it is written; a nameable surface spelled as prose is a defect. ZERO META: no provenance, source trace, release narration, process state, freshness disclaimer, or tool/skill context — any such block POISONS every downstream generation that loads the page; a stale `capture-pending`/`research` block is deleted. A domain folder carries its own README router (a one-table router; pages compose root laws, never re-open them).',
].join('\n')
const AGNOSTIC_SNIPPETS = [
  'AGNOSTIC SNIPPET LAW (style-guide [07]-[PLACEHOLDER_LAW]): every snippet COMPILES under Python 3.15 with legal NEUTRAL identifiers — `Shape`/`RefinedShape`/`Variant`/`PRIMARY`/`Field`/`KEY`/`Row`/`ROW_A`/`TABLE`/`SELECTED` — and placeholder strings (`"<value-a>"`) appear ONLY inside literals. NO project, repo, host, customer, pricing, deployment, or business-domain noun anchors a snippet; a domain noun is context poison.',
  'CORPUS-WIDE ZERO duplicated snippet demonstrations: each snippet exercises a surface region NO OTHER snippet in the corpus shows — the region is its spotlight; finalized surfaces composed as supporting material occupy no region and duplicate nothing. A duplicated region is repaired by ROUTING to its owner (compose it as supporting material), never by re-teaching. Snippets are doctrine-exemplary at full operator depth, ~3-4x denser than ordinary code, at the scale a large system takes (admission + dispatch + rail + policy in one fence with the growth axis visible).',
].join('\n')
const OPINIONATED = [
  'HEAVILY-OPINIONATED PROJECT DOCTRINE, NOT a language survey. ZERO table-stakes is tolerated, ever: a card or snippet teaching something a competent Python developer already knows — rather than an opinionated, dense, project-specific CHOICE — is a DEFECT to delete or densify. No net-casting to "cover the language"; cover only the opinionated decisions the projects need, each at 13/10.',
  'LOC budget ~450 is a SOFT pressure signal toward DENSIFICATION, NOT a hard gate. The real metric is per-card and per-snippet density: every card and every snippet world-class, zero filler. NEVER strip snippet whitespace, remove design content, or fragment a coherent concept to hit a number; a coherent dense concept may exceed 450 when every card and snippet earns its place (the csharp algorithms.md monolith is the precedent). A split is justified ONLY by concept disjointness, never by line count.',
].join('\n')
const STYLE_PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md: lead each section with the controlling rule/contract; one idea per paragraph; close on the consequence or boundary. Cut hedges (`may`/`might`/`probably`/`generally`/`where possible`/`if needed`), provenance, process narration, and report framing. Prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph. Prose that ASSERTS capability the fence lacks is a defect, not content.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in a code span; name the exact member instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE + FILE ORGANIZATION: code fences are agent-facing. Do NOT use section-divider headers in snippets (NO `# --- [UPPERCASE_LABEL]` lines and NO standalone `[LABEL]` section-header comments — they add LOC without value); organize PURELY by the canonical declaration ORDER (CLAUDE.md [08]: imports -> types -> constants -> models -> errors -> services -> operations -> composition -> exports; types before classes; owner blocks + dependency clusters intact; the Python overlay puts runtime tables/decoders AFTER the symbols they inspect; runtime/dependency order wins so every fence loads top-to-bottom), and strip any existing divider/section-label line. Beyond that, comment ONLY where intent is not already obvious from names, types, and signatures: default ZERO comments; at most 1 line where a comment genuinely earns its place. No narration, no restating the code, no docstring bloat, no task/process/review comments.'
const DOCTRINE = [LAW, '', ADVERSARIAL, '', PYDOCTRINE16, '', SHAPE_ADT, '', AOP_FIRST, '', PARAM_POLY, '', PY315, '', APISTACK_SUBSTRATE, '', PAGECRAFT, '', AGNOSTIC_SNIPPETS, '', OPINIONATED, '', STYLE_PROSE, '', COMMENTS].join('\n')

// --- [OPERATIONS] -- prompt builders -----------------------------------------------------
const coldGradePrompt = (page) => [DOCTRINE, '', 'TASK: COLD CONTEXT-FREE FINALIZATION GRADE + FIX IN PLACE of ' + page + '. This is the whole-corpus finalization gate: read THIS page COLD (fresh hostile eyes, trusting nothing a prior pass claimed), grade it against the full doctrine, the page-craft law, and the csharp density bar, and FIX every defect in place — extreme shape/ADT density, full AOP, parameterization/polymorphism, py3.15-modern, correct citation tier with every member verified, agnostic compiling snippets at large-system scale, page-craft grammar + card economy, zero table-stakes, zero meta, style + comment hygiene. Also confirm the page implicitly stacks the README chain (composes finalized owners as settled material, never re-teaching). Report grade `clean` ONLY if a genuinely aggressive attack left it untouched, else `edited`; report this page`s spotlight snippet `regions`. Return residual_high — each a {files:[...], claim} for any CROSS-FILE item (a contradiction with another page, a duplicated demonstration, a shared owner that spans files) you cannot fix from this one file. Edit ONLY under ' + ROOT + '/.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Rediscover')
const inv = await agent('Read ' + ROOT + '/README.md and parse the [01]-[ATLAS] table, THEN recurse into every sub-folder README router it references (e.g. ' + ROOT + '/domain/README.md, ' + ROOT + '/numerics/README.md if present). Return the CURRENT full ordered file set: every CONCEPT page that exists on disk under ' + ROOT + ' (core pages first in atlas order, then each sub-folder`s pages in its router order), EXCLUDING every README.md. Each row {path (repo-relative), order (global integer), folder (the sub-folder name or empty for core)}. Use find/read; do not cd; do not edit anything.', { label: 'rediscover', phase: 'Rediscover', schema: INVENTORY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const files = ((inv && inv.files) || []).filter((f) => f && f.path).sort((a, b) => a.order - b.order).map((f) => f.path)
log('Rediscover: ' + files.length + ' concept pages across the full docs/stacks/python set')

phase('Cold-grade')
const graded = (await pool(files, CAP, (page) => agent(coldGradePrompt(page), { label: 'cold:' + nameOf(page), phase: 'Cold-grade', schema: COLD_GRADE_SCHEMA, effort: 'xhigh', stallMs: STALL }).then((r) => ({ page, r })))).filter((x) => x && x.r)
const ledger = graded.map((g) => ({ file: g.page, regions: (g.r.regions || []) }))

phase('Corpus-dedup')
const dedup = await agent([DOCTRINE, '', 'TASK: CORPUS-WIDE DUPLICATE-SNIPPET DETECTION (the one whole-corpus barrier; detection + naming only, no edits). The REGION LEDGER below lists every page and the snippet-demonstration regions it spotlights across the ENTIRE docs/stacks/python corpus. Detect every case where the SAME surface region is demonstrated as a SPOTLIGHT by more than one page (a duplicated demonstration — the corpus law forbids it; each region has exactly one owner, others compose it as supporting material). For each collision return {region, files (the pages that all spotlight it), owner (the page that should KEEP the spotlight by altitude — the page whose disjoint layer the region most belongs to)}. Read a page`s body only to disambiguate a genuine collision from two distinct regions that share a tag. REGION LEDGER:\n' + JSON.stringify(ledger, null, 1)].join('\n'), { label: 'corpus-dedup', phase: 'Corpus-dedup', schema: DEDUP_SCHEMA, effort: 'max', stallMs: STALL })
const collisions = (dedup && dedup.collisions) || []
log('Corpus-dedup: ' + collisions.length + ' duplicated-demonstration collision(s)')

// --- [RECONCILE] -- residuals from cold-grade UNION the dedup collisions ------------------
const norm = (x, page) => typeof x === 'string' ? { files: [page], claim: x } : { files: x.files && x.files.length ? x.files : [page], claim: x.claim }
const allRes = []
for (const g of graded) if (g.r.residual_high) for (const x of g.r.residual_high) allRes.push(norm(x, g.page))
for (const c of collisions) allRes.push({ files: (c.files && c.files.length ? c.files : [c.owner]).filter(Boolean), claim: 'duplicated snippet demonstration of region "' + c.region + '" — owner ' + c.owner + ' keeps the spotlight; route the other page(s) to compose it as supporting material, never re-teaching' })
const uniq = [...new Map(allRes.filter((r) => r.files.length).map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Cold-grade: ' + graded.filter((g) => g.r.grade === 'edited').length + ' edited; reconcile ' + uniq.length + ' residuals+collisions -> ' + clusters.length + ' clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent([DOCTRINE, '', 'TASK: RECONCILE these whole-corpus cross-FILE residuals + duplication collisions. NO severity — treat EVERY item as must-address. Read EVERY listed file. For each: if it is a real cross-file defect, FIX it in place (unify the shared owner/rail; for a duplication collision, route the non-owner page(s) to compose the region as settled supporting material and re-spotlight an UNOWNED region, leaving the owner page`s spotlight intact; repair any contradiction so the pages agree), preserving all capability and regressing no file; if an item is FACTUALLY INCORRECT, leave it and say why. Edit ONLY under ' + ROOT + '/. Items:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: STALL })
    if (!fix) return null
    const verify = await agent([LAW, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Read the named files from disk and classify each item: "fixed" (real defect, now genuinely resolved — for a duplication, exactly one page now spotlights the region), "invalid" (the claim is factually wrong — cite why), or "open" (real defect still NOT resolved). Default to "open" on any doubt. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: STALL })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const openClaims = new Set(claimsAll.filter((c) => c.status === 'open').map((c) => c.claim))
const hard_residual = uniq.filter((r) => openClaims.has(r.claim))
const touched = [...new Set(reconciled.flatMap((r) => (r.fix && r.fix.files) || []).filter((p) => typeof p === 'string' && p.indexOf(ROOT) === 0))]

let finalized = []
if (touched.length) {
  phase('Finalize-grade')
  finalized = (await pool(touched, CAP, (page) => agent(coldGradePrompt(page), { label: 'final:' + nameOf(page), phase: 'Finalize-grade', schema: COLD_GRADE_SCHEMA, effort: 'xhigh', stallMs: STALL }).then((r) => ({ page, grade: r && r.grade })))).filter(Boolean)
}
const stillEdited = finalized.filter((f) => f.grade === 'edited').map((f) => f.page)
log('Finalize-grade: ' + touched.length + ' touched re-graded; ' + stillEdited.length + ' still not clean; ' + hard_residual.length + ' open hard residual(s) -> resolve-residuals')
return { workflow: 'reconcile-python-stack', root: ROOT, total: files.length, edited: graded.filter((g) => g.r.grade === 'edited').length, collisions: collisions.length, clusters: clusters.length, touched: touched.length, not_clean: stillEdited, hard_residual: hard_residual }
