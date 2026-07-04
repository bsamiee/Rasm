export const meta = {
  name: 'finalize',
  whenToUse: 'The finalization engine — the complement to rebuild: where rebuild improves/extends, finalize corrects and closes. Run it over a package (or folder subset) whose build legs have landed to kill split-brain, unify paradigms, adjudicate phantoms, collapse duplication/indirection in place, smooth logic flow end-to-end, and finish naive/mini surfaces — folder by folder in dependency order so the pass never chases itself.',
  description: 'Durable language-agnostic FINALIZATION pass over one libs/{csharp,python,typescript} package planning corpus. args = a package root, an array of planning sub-folders, or {targets}; language derives from the root and selects the doctrine root pages, both .api tiers, the manifest, and the member-verification rail. Plan (1 sonnet) enumerates sub-folders + pages. Strata (opus readers, 3 folders each) derive the folder dependency order from disk; the script toposorts. Then FOLDER BY FOLDER in that order: Discover — ONE opus agent PER PAGE fully understands the page + its related pages + the package README/ARCHITECTURE/manifest + every relevant .api catalog (both tiers), and returns the correction census (underutilized capability, hand-rolled reimplementation, phantoms classified forgotten-vs-lie, split-brain, unnecessary differentiation, naive fields, logic-flow breaks, stale references); Fix — ONE fable agent holding all the folder dossiers + the doctrine ROOT pages corrects the folder in place and may emit folder-tagged residuals the script routes into later folders\' fix prompts. Leftover residuals (backward or out-of-set) close in ONE terminal fable sweep — self-contained, no handoff.',
  phases: [
    { title: 'Plan', detail: 'one thin agent enumerates the planning sub-folders and their design pages', model: 'sonnet' },
    { title: 'Strata', detail: 'opus readers (3 folders each) read their folders + the package README/ARCHITECTURE and emit folder dependency edges; the script toposorts the processing order', model: 'opus' },
    { title: 'Discover', detail: 'per folder in order: 1 opus agent PER PAGE — full understanding of the page, its related pages, the manifest, and every relevant .api catalog; returns the correction census with anchors', model: 'opus' },
    { title: 'Fix', detail: 'per folder in order: ONE fable agent (doctrine root pages read in full) corrects the folder in place — unify, collapse, realize-or-delete phantoms, smooth flow, densify; emits folder-tagged residuals routed downstream', model: 'fable' },
    { title: 'Sweep', detail: 'ONE terminal fable agent closes any residual left pointing backward or out of the folder set; nothing hands off', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 8
const STAGGER_MS = 1500
const STALL = 480000
const STRATA_BATCH = 3

// --- [INPUTS] ----------------------------------------------------------------------------
const normTarget = (t) => String(t).trim().replace(/\/+$/, '').replace(/^\/+/, '')
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const rawTargets = Array.isArray(argsIn) ? argsIn
  : (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.targets)) ? argsIn.targets
  : (argsIn && typeof argsIn === 'object' && typeof argsIn.targets === 'string') ? [argsIn.targets]
  : (typeof argsIn === 'string' && argsIn.trim()) ? [argsIn]
  : []
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))]
const langOf = (t) => t.indexOf('libs/csharp') === 0 ? 'cs' : t.indexOf('libs/python') === 0 ? 'py' : t.indexOf('libs/typescript') === 0 ? 'ts' : null
const LANG_KEYS = [...new Set(TARGETS.map(langOf))]
const LANG_KEY = (LANG_KEYS.length === 1 && LANG_KEYS[0]) ? LANG_KEYS[0] : null

// --- [MODELS] ----------------------------------------------------------------------------
const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['root', 'folders'], properties: {
  root: { type: 'string' },
  folders: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'pages'], properties: {
    folder: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } } } } } }
const STRATA_SCHEMA = { type: 'object', additionalProperties: false, required: ['edges'], properties: {
  edges: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from', 'to'], properties: {
    from: { type: 'string' }, to: { type: 'string' }, why: { type: 'string' } } } },
  notes: { type: 'string' } } }
const DOSSIER_SCHEMA = { type: 'object', additionalProperties: false, required: ['page', 'findings'], properties: {
  page: { type: 'string' }, related: { type: 'array', items: { type: 'string' } }, catalogs: { type: 'array', items: { type: 'string' } },
  findings: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['kind', 'anchor', 'evidence'], properties: {
    kind: { type: 'string', enum: ['underutilized', 'handrolled', 'phantom_forgotten', 'phantom_lie', 'splitbrain', 'differentiation', 'naive', 'flow', 'stale'] },
    anchor: { type: 'string' }, evidence: { type: 'string' }, pointer: { type: 'string' } } } } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] },
  collapsed: { type: 'string' }, realized: { type: 'string' }, summary: { type: 'string' },
  residuals: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'claim'], properties: {
    folder: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LANG = {
  cs: { name: 'C#', root: 'libs/csharp', stack: 'docs/stacks/csharp', shared: 'libs/csharp/.api',
    manifest: 'the package `.csproj` and the central `Directory.Packages.props` block for this package',
    verify: '`uv run python -m tools.assay api` (assay blocked or unavailable: the `.api` catalogs + the nuget MCP for feed truth + Context7/exa/tavily own the fallback)',
    stackLaw: 'the docs/stacks/csharp ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis); the domain/ sub-folder is OUT of this read' },
  py: { name: 'Python', root: 'libs/python', stack: 'docs/stacks/python', shared: 'libs/python/.api',
    manifest: 'the root `pyproject.toml` rows this package consumes',
    verify: '`uv run python -m tools.assay api resolve <pkg>` (blocked or gated: the `.api` catalogs + PyPI feed truth + Context7/exa/tavily own the fallback)',
    stackLaw: 'the docs/stacks/python ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full in the README [01]-[ATLAS] order (language, shapes, iteration, surfaces-and-dispatch, rails-and-effects, concurrency, boundaries, algorithms, system-apis, runtime); the domain/ and numerics/ sub-folders are OUT of this read' },
  ts: { name: 'TypeScript', root: 'libs/typescript', stack: 'docs/stacks/typescript', shared: 'libs/typescript/.api',
    manifest: 'the `pnpm-workspace.yaml` / package manifest rows this area consumes',
    verify: 'the published types in node_modules (`uv run python -m tools.assay api` over node_modules declarations where a member is novel)',
    stackLaw: 'the docs/stacks/typescript ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full (README, language, derivation, values, computation, shapes, surfaces-and-dispatch, rails-and-effects, services-and-layers, concurrency, streams, boundaries)' },
}
const L = LANG[LANG_KEY] || LANG.cs // inert fallback: composition no-ops before any agent call when LANG_KEY is null

const LAW = [
  'Rasm monorepo, ' + L.name + ' planning corpus (markdown design pages; code fences are the product). This is a FINALIZATION pass, not a ' +
    'redesign: the rebuild engine improves and extends; THIS engine corrects and CLOSES. The targets are: split-brain and dual paradigms ' +
    'unified onto ONE rail/paradigm per the doctrine; unnecessary differentiation (parallel shapes, sibling name families, twin entries for ' +
    'one concept) collapsed into the polymorphic owner IN PLACE — never a new file; duplication and indirection reduced; PHANTOM code ' +
    'adjudicated — a cited member/type/seam that does not exist is either FORGOTTEN FUNCTIONALITY (intent evidence stands in seams, README ' +
    'rows, growth clauses, or sibling contracts — realize it NOW, properly and at full depth, as if it had always been there) or a LIE ' +
    '(no intent evidence — delete it); naive/mini fields and capability-thin owners grown to the real concept the domain demands; logic flow ' +
    'smoothed END-TO-END (admission once -> canonical owner -> unified rail -> projection -> egress; no dead ends, no rail breaches, no ' +
    'unreachable cases, no orphaned carriers); density/complexity/richness pushed and LOC reduced THROUGH density, never through capability ' +
    'loss. ALL capability is preserved — deletion exists only for proven lies and true duplicates.',
  'REMOVAL DISCIPLINE binds: a package admission, a capability, or a domain concept is never removed for underutilization — underutilization ' +
    'is an integration defect this pass CLOSES by wiring the capability into its owner. The only sanctioned deletions are phantom LIES and ' +
    'exact duplicates whose one surviving owner carries everything.',
  'WRITE-FULLY MANDATE: every fix identified is made NOW via Edit/Write; the fix-log REPORTS edits already made, never a to-do list.',
].join('\n')
const BOUNDARY = 'BOUNDARY LAW: cross-package and cross-language wire-canonical seam names are FROZEN — repair a seam recording, never rename ' +
  'the wire. The package README.md/ARCHITECTURE.md stay truthful in the same motion as any owner/entry change. Strata hold: no downward ' +
  'dependency, no host-type leak into a host-neutral owner.'

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
// Kahn toposort over folder edges ({from, to} = FROM depends on TO, so TO processes first); ties and cycles fall back to plan order.
const toposort = (folders, edges) => {
  const set = new Set(folders)
  const deps = new Map(folders.map((f) => [f, new Set()]))
  for (const e of edges) if (set.has(e.from) && set.has(e.to) && e.from !== e.to) deps.get(e.from).add(e.to)
  const order = []
  const placed = new Set()
  while (order.length < folders.length) {
    const ready = folders.filter((f) => !placed.has(f) && [...deps.get(f)].every((d) => placed.has(d)))
    if (!ready.length) { for (const f of folders) if (!placed.has(f)) { order.push(f); placed.add(f) } break } // cycle -> plan order
    for (const f of ready) { order.push(f); placed.add(f) }
  }
  return order
}

const planPrompt = () => ['Rasm monorepo. TASK: thin enumerate (read-only, do NOT edit). TARGETS (repo-relative): ' + JSON.stringify(TARGETS) +
  '. Each target is a package root (e.g. ' + L.root + '/<Package>) or a planning sub-folder (<root>/.planning/<Folder>). Resolve the ONE ' +
  'owning package root. Return root (the package root) and folders — one entry per planning sub-folder in scope (a package-root target ' +
  'expands to EVERY sub-folder under its .planning tree; explicit sub-folder targets restrict to exactly those): {folder: the sub-folder ' +
  'name, pages: repo-relative *.md paths under it}. EXCLUDE IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md and any _*.md campaign brief. Use ' +
  'find/ls; do NOT cd.'].join('\n')
const strataPrompt = (batch, allFolders, root) => [LAW, '', 'TASK: STRATA READ (read-only, do NOT edit). Read the package README.md + ' +
  'ARCHITECTURE.md at ' + root + ' IN FULL, then read EVERY page in your assigned folders IN FULL: ' + JSON.stringify(batch) + '. For each ' +
  'assigned folder, determine which OTHER folders in the full set ' + JSON.stringify(allFolders) + ' it depends on (composes their owners, ' +
  'cites their entries/types/seams, consumes their vocabularies). Return edges — {from: your folder, to: the folder it depends on, why: the ' +
  'concrete owner/seam that proves it} — one edge per real dependency, none for prose-only mentions. The edge direction is DEPENDS-ON: ' +
  'from needs to.'].join('\n')
const discoverPrompt = (page, folder, root) => [LAW, '', 'TASK: PER-PAGE DISCOVERY (read-only, do NOT edit) — you own ONE page: ' + page +
  ' (folder ' + folder + '). FULLY understand it: read the page top-to-bottom, then read EVERY related page it composes or is composed by ' +
  '(seam partners both directions, entry owners, vocabulary sources — full reads, never skims). Read the package README.md + ARCHITECTURE.md ' +
  'at ' + root + ' and ' + L.manifest + '. Enumerate BOTH .api tiers with a real ls (' + L.shared + '/ AND the package .api/) and READ every ' +
  'catalog relevant to this page. Verify every load-bearing cited member via ' + L.verify + ' — never memory. Return the correction census, ' +
  'one finding per defect with an exact anchor: `underutilized` (a catalog capability the page concept admits but no fence exploits — name ' +
  'the concrete member and where it integrates), `handrolled` (page logic re-deriving what an admitted package/API owns), `phantom_forgotten` ' +
  '(a cited member/type/seam that does not exist AND intent evidence stands — name the evidence; the fix realizes it), `phantom_lie` (cited, ' +
  'nonexistent, no intent evidence — the fix deletes it), `splitbrain` (one concern spelled two ways / dual paradigms on one rail), ' +
  '`differentiation` (parallel shapes/entries/name families one polymorphic owner should carry), `naive` (mini fields, capability-thin ' +
  'owners, enumerated rosters where a generator belongs), `flow` (rail breaches, dead ends, unreachable cases, admission done twice or never), ' +
  '`stale` (references to renamed/moved/dead owners). Evidence is disk-cited fact; `pointer` is an initial fix pointer, never a ceiling. An ' +
  'empty census is EARNED by the full read, never a first-pass concession.'].join('\n')
const fixPrompt = (folder, pages, dossiers, forwarded, processed, root) => [LAW, '', BOUNDARY, '', 'TASK: FOLDER FINALIZATION — you are the ' +
  'ONE writer for folder ' + folder + ' (pages: ' + pages.join(', ') + '). FIRST read ' + L.stackLaw + '. Then read every page of your folder ' +
  'IN FULL, the package README.md/ARCHITECTURE.md at ' + root + ', and ' + L.manifest + '. Your grounding: the per-page correction dossiers ' +
  'below (verify anything load-bearing — they point, you decide) plus the forwarded residuals earlier folders routed to you (close every one). ' +
  'Apply the finalization law to EVERY page: unify split-brain onto one paradigm; collapse differentiation and duplication IN PLACE; realize ' +
  'every phantom_forgotten properly and at full depth; delete every phantom_lie; grow naive owners to the real concept; smooth logic flow ' +
  'end-to-end; wire every underutilized capability into its owner; replace hand-rolled logic with the package surface (verify members via ' +
  L.verify + '). ALREADY-PROCESSED folders (' + (processed.length ? processed.join(', ') : 'none yet') + ') carry corrections that are LAW — ' +
  'compose them; a surgical edit there is allowed ONLY when your unification demands it. NOT-YET-PROCESSED folders are untouchable: a defect ' +
  'you find there becomes a residual {folder, files, claim} routed to its fixer — never an edit, never dropped. A defect outside the folder ' +
  'set entirely returns with folder "". WRITE-FULLY now; keep README/ARCHITECTURE truthful in the same motion. DOSSIERS:\n' +
  JSON.stringify(dossiers, null, 1) + (forwarded.length ? '\nFORWARDED RESIDUALS (close these):\n' + JSON.stringify(forwarded, null, 1) : '') +
  '\nReturn the fix-log: files (every page edited), verdict, collapsed (what merged), realized (phantoms made real), summary, residuals.'].join('\n')
const sweepPrompt = (leftovers, root) => [LAW, '', BOUNDARY, '', 'TASK: TERMINAL SWEEP — the run\'s last agent; nothing follows, nothing hands ' +
  'off. FIRST read ' + L.stackLaw + '. Close EVERY leftover residual below: each points backward at an already-finalized folder or outside ' +
  'the folder set — read every named file plus the owning folder context at ' + root + ', decide the canonical fix, and implement it NOW at ' +
  'the objectively-best root-level form (whole-package authority; wire-canonical seam names stay frozen). Where disk proves a claim factually ' +
  'wrong, cite the proof in the summary — never silently skip. RESIDUALS:\n' + JSON.stringify(leftovers, null, 1) +
  '\nReturn files, verdict, summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length) { log('No targets — pass a package root, an array of planning sub-folders, or {targets}. Empty args is a no-op.'); return { targets: [], folders: 0 } }
if (!LANG_KEY) { log('Targets must live under ONE language root (libs/csharp | libs/python | libs/typescript). Got: ' + JSON.stringify(TARGETS)); return { targets: TARGETS, folders: 0 } }

// --- [PLAN]
phase('Plan')
const plan = await agent(planPrompt(), { label: 'plan', phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN_SCHEMA, stallMs: STALL })
const ROOT = (plan && plan.root) || TARGETS[0]
const FOLDERS = ((plan && plan.folders) || []).filter((f) => f && f.folder && (f.pages || []).length)
const pagesOf = new Map(FOLDERS.map((f) => [f.folder, f.pages]))
const folderNames = FOLDERS.map((f) => f.folder)
log('Plan[' + LANG_KEY + ']: ' + folderNames.length + ' folder(s), ' + FOLDERS.reduce((n, f) => n + f.pages.length, 0) + ' page(s) at ' + ROOT)
if (!folderNames.length) { log('No folders resolved under the targets'); return { targets: TARGETS, language: LANG_KEY, folders: 0 } }

// --- [STRATA]
phase('Strata')
const strata = (await parallel(chunk(folderNames, STRATA_BATCH).map((batch, i) => () =>
  agent(strataPrompt(batch, folderNames, ROOT), { label: 'strata:b' + i, phase: 'Strata', model: 'opus', effort: 'max', schema: STRATA_SCHEMA, stallMs: STALL })))).filter(Boolean)
const ORDER = toposort(folderNames, strata.flatMap((s) => s.edges || []))
log('Strata: ' + strata.flatMap((s) => s.edges || []).length + ' edge(s) -> order [' + ORDER.join(' -> ') + ']')

// --- [DISCOVER]
// --- [FIX]
phase('Discover')
phase('Fix')
const routed = new Map(ORDER.map((f) => [f, []]))
const leftovers = []
const fixLogs = []
const processed = []
for (const folder of ORDER) {
  const pages = pagesOf.get(folder) || []
  const dossiers = (await pool(pages, CAP, (page) =>
    agent(discoverPrompt(page, folder, ROOT), { label: 'discover:' + folder + '/' + page.split('/').pop(), phase: 'Discover', model: 'opus', effort: 'max', schema: DOSSIER_SCHEMA, stallMs: STALL }))).filter(Boolean)
  // One bounded re-attempt: a transient agent death must never silently lose a folder.
  const opts = { label: 'fix:' + folder + ' (' + pages.length + ' pages)', phase: 'Fix', model: 'fable', effort: 'max', schema: FIX_SCHEMA, stallMs: STALL }
  const fix = (await agent(fixPrompt(folder, pages, dossiers, routed.get(folder), processed, ROOT), opts))
    || (await agent(fixPrompt(folder, pages, dossiers, routed.get(folder), processed, ROOT), { ...opts, label: opts.label + ':retry' }))
  processed.push(folder)
  if (!fix) { log('Fix ' + folder + ': agent died twice — folder census surfaced in the return'); fixLogs.push({ folder, verdict: 'UNFIXED', summary: 'fix agent died twice', files: [] }); continue }
  fixLogs.push({ folder, verdict: fix.verdict, summary: fix.summary, files: fix.files, collapsed: fix.collapsed, realized: fix.realized })
  for (const r of (fix.residuals || []).filter((x) => x && x.claim)) {
    const dest = routed.has(r.folder) && !processed.includes(r.folder) ? r.folder : null
    if (dest) routed.get(dest).push({ from: folder, files: r.files || [], claim: r.claim })
    else leftovers.push({ from: folder, folder: r.folder || '', files: r.files || [], claim: r.claim })
  }
  log('Fix ' + folder + ': ' + fix.verdict + '; ' + (fix.files || []).length + ' file(s); ' + (fix.residuals || []).length + ' residual(s) routed')
}

// --- [SWEEP]
let sweep = null
if (leftovers.length) {
  phase('Sweep')
  const opts = { label: 'sweep (' + leftovers.length + ' claims)', phase: 'Sweep', model: 'fable', effort: 'max', schema: SWEEP_SCHEMA, stallMs: STALL }
  sweep = (await agent(sweepPrompt(leftovers, ROOT), opts)) || (await agent(sweepPrompt(leftovers, ROOT), { ...opts, label: opts.label + ':retry' }))
}
log('finalize[' + LANG_KEY + ']: ' + processed.length + '/' + ORDER.length + ' folder(s); ' + leftovers.length + ' leftover residual(s)' +
  (leftovers.length ? (sweep ? ' swept (' + sweep.verdict + ')' : ' — SWEEP DIED TWICE, surfaced in the return') : ''))

return {
  targets: TARGETS, language: LANG_KEY, root: ROOT, order: ORDER,
  folders: fixLogs, leftovers: sweep ? [] : leftovers, sweep: sweep ? { verdict: sweep.verdict, files: sweep.files, summary: sweep.summary } : null,
}
