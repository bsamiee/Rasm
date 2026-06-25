export const meta = {
  name: 'organize-python-snippets',
  description: 'FINAL organization-only pass over the docs/stacks/python CODE DOCTRINE — runs LAST, after every content WF has finalized the corpus. ONE organizer agent per file re-orders the code INSIDE every Python fence into the canonical DECLARATION ORDER (imports/TYPE_CHECKING -> type aliases/protocols/enums/unions -> dependency-free constants -> models -> typed error rails -> service contracts -> operations -> composition -> exports; types before classes; owner blocks + dependency clusters kept intact; the Python overlay: runtime decoders/registries/tables AFTER the symbols they inspect, runtime/dependency order winning over role order) and STRIPS every section-divider / section-label comment line out of the fences (the doctrine snippets carry NO section dividers — they add LOC without value; organization is expressed only by ORDER). Then ONE critique/validate agent per file (write-capable) verifies every reorganized snippet still loads top-to-bottom with no forward-reference, the order is correct, ZERO section-divider lines remain, and ZERO content/prose/card/table/name/semantics changed, fixing every defect in place. MOVE/ORDER/STRIP-DIVIDER ONLY — never a rewrite, never a content edit; where canonical order would break execution the working order wins. Per-file organize -> validate, pooled at CAP=12 with a 1500ms launch gate, sonnet only. Edits ONLY docs/stacks/python. Takes no args.',
  phases: [
    { title: 'Inventory', detail: 'list every concept page under docs/stacks/python that carries Python fences (core + domain + numerics), excluding README routers', model: 'sonnet' },
    { title: 'Organize', detail: 'per file: organize(sonnet) re-orders every fence by canonical declaration order + strips all section-divider lines, then validate(sonnet, write) verifies load-order, divider-free, preservation and fixes in place', model: 'sonnet' },
  ],
}

// --- [SCHEMAS] ---------------------------------------------------------------------------
const INVENTORY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'order'], properties: { path: { type: 'string' }, order: { type: 'integer' }, folder: { type: 'string' } } } } } }
const ORG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['organized', 'conformant'] }, fences: { type: 'integer' }, dividers_removed: { type: 'integer' }, exemptions: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const VALIDATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'preserved'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['clean', 'fixed'] }, preserved: { type: 'boolean' }, fixes: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

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

// --- [MODELS] -- the shared organization-law blocks woven into both prompts ----------------
const LAW = [
  'TARGET: docs/stacks/python/ is the FINALIZED Python CODE DOCTRINE (agnostic teaching pages, each with one exemplary Python snippet per region). This pass is ORGANIZATION-ONLY: re-order the CODE INSIDE each ```python fence into the canonical DECLARATION ORDER and STRIP every section-divider / section-label comment line, changing NOTHING else. It is NOT a content rebuild, NOT a doctrine pass, NOT a densification — the doctrine is settled; you only re-order declarations and remove divider noise.',
  'AUTHORITY + SCOPE: READ CLAUDE.md section [08]-[FILE_ORGANIZATION] for the canonical ORDER, the ordering ladder, the owner-block + dependency-cluster rules, and (load-bearing) the Python language overlay — but IGNORE its section-separator FORMATTING: this pass adds NO dividers and removes the ones that exist. Edit ONLY the ONE target file named in the task, and within it ONLY the code inside ```python fences. The markdown AROUND every fence — prose, family cards, decision tables, `## [NN]-[LABEL]` headers, bracketed prose labels, the H1 + lead — stays BYTE-IDENTICAL. NEVER rename a symbol, alter a snippet`s logic, add or remove a declaration, touch an agnostic name or placeholder literal, or edit another file. MOVE / ORDER / STRIP-DIVIDER only; if a fence is already correctly ordered and divider-free, leave it untouched.',
].join('\n')
const ORDER = [
  'CANONICAL DECLARATION ORDER (omit unused roles): imports + `TYPE_CHECKING` + import-time gates -> type aliases/protocols/enums/unions -> dependency-free constants -> models (records/classes/value objects/receipts) -> typed error rails -> service contracts -> operations (pure transforms/rail pipelines/algorithms) -> composition (decorators/wiring/composition roots) -> exports. Types before classes; leaf operations before orchestration. This order is expressed PURELY by the SEQUENCE of declarations — never by a section-label comment.',
  'ORDERING LADDER, applied in order: `role` -> `owner block` -> `runtime/declaration dependency` -> `semantic rank` -> `kind` -> `smaller-to-larger` -> `alphabetical`. Keep an OWNER BLOCK intact — a closed family / `StrEnum` / value object / schema-model family / `frozendict` table / dispatcher is ONE owner; sort WITHIN it, never flatten its members into separate roles. Keep DEPENDENCY CLUSTERS adjacent — a declaration that must follow the symbol it derives from, inspects, registers, decodes, wraps, or composes stays next to it. Use smaller-to-larger then alphabetical ONLY as the final tiebreakers among equivalent declarations.',
  'PYTHON OVERLAY (load-bearing — module-level assignments execute immediately): imports + `TYPE_CHECKING` + import-time gates precede everything; runtime decoders/encoders/registries/dispatch tables FOLLOW the models or functions they inspect (a `TypeAdapter(Shape)`, a `frozendict` policy table keyed on an enum, a registry referencing a class MUST come AFTER that symbol); an `Annotated` validator function sits between the immutable constants and the dependent aliases that reference the real validator object. RUNTIME/DEPENDENCY ORDER WINS over the role order wherever they conflict — every snippet must still load top-to-bottom with no forward reference / `NameError`; where the canonical role order would break execution, keep the working order and record it as an exemption.',
  'NO SECTION DIVIDERS (the explicit point of this pass): do NOT add ANY `# --- [LABEL] ---` section-divider line or any standalone `[LABEL]` section-header comment inside a fence — they add LOC without value. REMOVE every existing section-divider and section-label comment line from every fence (the corpus currently carries ~56 such lines across `algorithms.md`, `rails-and-effects.md`, `runtime.md`, `concurrency.md` — strip them ALL). Organization is conveyed ONLY by the ORDER of declarations. Leave a genuine intent-bearing inline comment (the rare 1-liner explaining a subtle invariant) untouched; strip ONLY the section-divider / section-label lines.',
].join('\n')
const DOCTRINE = [LAW, '', ORDER].join('\n')

// --- [OPERATIONS] -- prompt builders -----------------------------------------------------
const organizePrompt = (page) => [DOCTRINE, '', 'TASK: ORGANIZE every Python fence in ' + page + ' by canonical DECLARATION ORDER and STRIP every section-divider line. FIRST read CLAUDE.md [08]-[FILE_ORGANIZATION] (for the ORDER + ordering ladder + Python overlay; IGNORE its divider-separator formatting — this pass adds NO dividers) and ' + page + ' in full. For EACH ```python fence: (a) REMOVE every `# --- [LABEL] ---` section-divider line and every standalone `[LABEL]` section-header comment; (b) re-order its declarations into the canonical role order under the ordering ladder + the Python overlay (types before classes; owner blocks and dependency clusters kept intact; runtime tables/decoders/registries after the symbols they inspect), honoring runtime/dependency order so the fence still loads top-to-bottom. This is MOVE/ORDER/STRIP-DIVIDER ONLY — preserve every declaration, name, line of logic, and placeholder exactly, keep every snippet agnostic, and leave ALL markdown (prose, cards, tables, headers) byte-identical. Edit ' + page + ' in place. Report `fences` (count), `dividers_removed` (count of divider/section-label lines stripped), any `exemptions` (a fence kept in working order because canonical order would break execution), verdict `organized` (you changed it) or `conformant` (already correctly ordered and divider-free).'].join('\n')
const validatePrompt = (page) => [DOCTRINE, '', 'TASK: CRITIQUE + VALIDATE + FIX IN PLACE the organization of ' + page + ' — the organizer just re-ordered its fences and stripped dividers; you are the write-capable check. Read CLAUDE.md [08]-[FILE_ORGANIZATION] and ' + page + ' FRESH and verify, fence by fence: (1) LOAD ORDER — every snippet still loads top-to-bottom with NO forward reference, NO `NameError`, no runtime table/decoder/registry moved before the symbol it inspects, no owner block or dependency cluster split; (2) ORDER — the canonical declaration order + ordering ladder + Python overlay are applied correctly (types before classes; leaf before orchestration); (3) NO DIVIDERS — ZERO `# --- [LABEL] ---` section-divider lines and ZERO standalone `[LABEL]` section-header comments remain in ANY fence (strip any that survived); (4) PRESERVATION — ZERO content changed vs the doctrine: every declaration, symbol name, line of logic, placeholder, card, table, prose line, and markdown header is intact, and every snippet is still agnostic. FIX every defect in place — move an execution-breaking or misordered declaration into the correct working order, strip any surviving divider/section-label line, restore anything dropped or renamed, revert any content/prose drift. Set `preserved` true only if the pass is genuinely organization + divider-strip with no semantic or content change. verdict `fixed` (you edited) or `clean` (organizer left it correct). Report `fixes`.'].join('\n')

const processPage = async (page) => {
  const organize = await agent(organizePrompt(page), { label: 'organize:' + nameOf(page), phase: 'Organize', schema: ORG_SCHEMA, model: 'sonnet', effort: 'high', stallMs: STALL })
  const validate = organize ? await agent(validatePrompt(page), { label: 'validate:' + nameOf(page), phase: 'Organize', schema: VALIDATE_SCHEMA, model: 'sonnet', effort: 'high', stallMs: STALL }) : null
  return { page, organize, validate, ok: !!(organize && validate) }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Inventory')
const inv = await agent('Read ' + ROOT + '/README.md and parse the [01]-[ATLAS] table, THEN recurse into every sub-folder README router it references (e.g. ' + ROOT + '/domain/README.md, ' + ROOT + '/numerics/README.md if present). Return EVERY concept page that exists on disk under ' + ROOT + ' and CONTAINS at least one ```python fence — core pages first in atlas order, then each sub-folder`s pages in router order — EXCLUDING every README.md (the routers carry no code). Each row {path (repo-relative, e.g. ' + ROOT + '/shapes.md), order (global integer), folder (the sub-folder name or empty for core)}. Use find/read; do not cd; do not edit anything.', { label: 'inventory', phase: 'Inventory', schema: INVENTORY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const files = ((inv && inv.files) || []).filter((f) => f && f.path).sort((a, b) => a.order - b.order).map((f) => f.path)
log('Inventory: ' + files.length + ' concept pages with Python fences under ' + ROOT)

phase('Organize')
const done = (await pool(files, CAP, (page) => processPage(page))).filter(Boolean)
const organized = done.filter((r) => r.organize && r.organize.verdict === 'organized').length
const stripped = done.reduce((n, r) => n + ((r.organize && r.organize.dividers_removed) || 0), 0)
const fixed = done.filter((r) => r.validate && r.validate.verdict === 'fixed').length
const unpreserved = done.filter((r) => r.validate && r.validate.preserved === false).map((r) => r.page)
log('Organize: ' + done.length + '/' + files.length + ' pages processed; ' + organized + ' re-ordered; ' + stripped + ' divider lines stripped; ' + fixed + ' validate-fixed; ' + unpreserved.length + ' preservation-flagged')
return { workflow: 'organize-python-snippets', root: ROOT, total: files.length, processed: done.filter((r) => r.ok).length, organized, dividers_removed: stripped, fixed, unpreserved }
