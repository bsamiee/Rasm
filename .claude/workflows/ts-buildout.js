export const meta = {
  name: 'ts-buildout',
  description: 'Realize every one-line page stub across the libs/typescript folders into deep, decision-complete design pages at the rebuilt docs/stacks/typescript bar. Plan (1 opus agent expands targets to folders + stub inventories in the BLUEPRINT build order) -> per folder, pooled: Explore (1 opus agent reads BOTH .api tiers IN FULL + the BLUEPRINT section + DECISION leg + index docs + every stub, emits the API-capture reading map) -> Build (ONE fable agent owns the WHOLE folder: composes docs/stacks/typescript in full, realizes every page ground-up with .api ultra-stacking, re-reading catalogues as needed). No per-page batching, no critique/redteam followers, no in-run reconcile: cross-folder work is DEFERRED as {files, claim} residuals returned as data for one operator-launched alignment agent. args = a folder path, an array, or the string all; empty = no-op. Ephemeral: delete after the build-out lands.',
  whenToUse: 'Campaign stage 5, after the doctrine rebuild lands: fill the stood-up TS folder stubs with high depth, folder by folder or all at once.',
  phases: [
    { title: 'Plan', detail: '1 opus agent resolves targets to folders + per-folder page-stub inventories in BLUEPRINT build order' },
    { title: 'Explore', detail: 'per folder: 1 opus API-capture agent over BOTH .api tiers in full, BLUEPRINT section, DECISION leg, index docs, every stub -> the dense reading map' },
    { title: 'Build', detail: 'per folder: ONE fable agent, docs/stacks/typescript composed in full, every page realized ground-up, residuals deferred as data' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const TS = 'libs/typescript'
const TSD = 'docs/stacks/typescript'
const BLUEPRINT = 'libs/typescript/BLUEPRINT.md'
const DECISION = 'RASM-TS-PLATFORM-DECISION.md'
const FOLDER_CAP = 4
const STAGGER_MS = 2000

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const rawTargets = Array.isArray(argsIn) ? argsIn
  : (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.targets)) ? argsIn.targets
  : (typeof argsIn === 'string' && argsIn.trim()) ? [argsIn.trim()]
  : []
const ALL = rawTargets.some((t) => String(t).trim().toLowerCase() === 'all' || String(t).trim().replace(/\/+$/, '') === TS)
const TARGETS = ALL ? [] : [...new Set(rawTargets.map((t) => String(t).trim().replace(/\/+$/, '')).filter(Boolean)
  .map((t) => t.indexOf(TS + '/') === 0 ? t : TS + '/' + t.replace(/^\/+/, '')))]

// --- [MODELS] ----------------------------------------------------------------------------
const PLAN = { type: 'object', additionalProperties: false, required: ['folders'], properties: {
  folders: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'root', 'order', 'pages'], properties: {
    name: { type: 'string' }, root: { type: 'string' }, order: { type: 'number' },
    pages: { type: 'array', items: { type: 'string' } } } } } } }
const MAP = { type: 'object', additionalProperties: false, required: ['map'], properties: { map: { type: 'string' } } }
const BUILD = { type: 'object', additionalProperties: false, required: ['pages'], properties: {
  pages: { type: 'array', items: { type: 'string' } },
  residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: {
    files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = 'Rasm monorepo, TS catch-up campaign stage 5 (build-out). The ' + TS + ' folders were stood up per ' + DECISION + ' + ' + BLUEPRINT +
  ': authored README/ARCHITECTURE, EMPTY IDEAS/TASKLOG placeholders (NEVER open or fill them), one-line page stubs under <folder>/.planning/ ' +
  'mirroring the BLUEPRINT trees, and rebuilt .api catalogues at both tiers. ' + TSD + '/ is the rebuilt route-owned doctrine: the build agent ' +
  'composes it IN FULL (README plus language, derivation, values, computation, shapes, surfaces-and-dispatch, rails-and-effects, ' +
  'services-and-layers, concurrency, streams, boundaries) before writing a single fence, and holds every law as fact. THE ' +
  'PRODUCT: each one-line stub becomes a decision-complete design page - H1 [<PKG>_<PAGE>], one declarative lead, section [1] the cluster ' +
  'index, cluster cards (owner, packages, growth, earned boundary/receipt/entry lines), transcription-complete signature fences an implementer ' +
  'copies verbatim (bodies where the body is the law, ZERO comments in fences, invariants on the card), at most one mermaid per cluster; ' +
  'fences obey the EXPORT LAW - no in-body exports, declarations unexported with ONE end-of-file `// --- [EXPORTS]` block carrying the ' +
  'complete public surface; the block is itself a collapse target (sibling exports for one concept merge into ONE polymorphic export, ' +
  'modality discriminated inside, companions riding the same name via declaration merging; 1-2 exports per module the bar). ' +
  'ULTRA-STACKING: BOTH .api tiers (' + TS + '/.api/ substrate + <folder>/.api/ domain) are mined to operator depth; an admitted capability ' +
  'the concept admits but no owner exploits is a defect; an external member is written ONLY at a spelling an .api catalogue verifies - an ' +
  'unverifiable member is a RESEARCH item, never settled fence code. LAW SOURCES (read, adhere, never restate): the folder section of ' +
  BLUEPRINT + ' + the folder leg of ' + DECISION + ' (binding per-file intent), the folder README + ARCHITECTURE, ' +
  'libs/.planning/{architecture,campaign-method,README}.md where a boundary question arises. HARD CONSTRAINTS: lib-not-app; zero-YAML IaC; ' +
  'no-migration event-sourced persistence; sql lanes pg + sqlite; alignment-never-coupling (C# owns the wire vocabulary, XxHash128 content-key ' +
  'parity, GLB consumed, TS owns no geometry). DEFERRAL LAW: cross-folder or cross-language work you cannot complete inside your folder is ' +
  'DEFERRED as a residual {files: [both endpoint files], claim} returned as data - never half-edit a sibling folder, never drop the item silently.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const sleep = (ms) => new Promise((r) => setTimeout(r, ms))
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  let gate = Promise.resolve()
  const launch = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  const run = async () => { while (next < items.length) { const i = next++; await launch(); out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}
const planPrompt = () => [LAW, '', 'TASK: PLAN. Resolve the build-out scope against real disk state. TARGETS: ' +
  (TARGETS.length ? JSON.stringify(TARGETS) : '(all folders)') + '. Read the build-order section of ' + BLUEPRINT + ' for the folder order; ' +
  'for each in-scope folder, fd/ls its .planning/ tree and list every design-page stub (relative .md paths). A page already realized (more ' +
  'than ~5 lines) is listed too - the build agent deepens rather than re-stubs. Return folders sorted by build order, each {name, root ' +
  '(' + TS + '/<name>), order, pages}. Folders with zero pages are omitted.'].join('\n')
const explorePrompt = (f) => [LAW, '', 'TASK: EXPLORE (read-only API capture) for ' + f.root + '. Read IN FULL: EVERY catalogue in ' + TS +
  '/.api/ AND EVERY catalogue in ' + f.root + '/.api/ (ls both tiers first - enumerate completely, then read each catalogue whole), the ' +
  f.name + ' section of ' + BLUEPRINT + ', the ' + f.name + ' leg of ' + DECISION + ', the folder README.md + ARCHITECTURE.md, and every page ' +
  'stub: ' + JSON.stringify(f.pages) + '. Emit ONE dense reading map built to make the build agent FAST AND COMPLETE: per page {the owned ' +
  'concern and BLUEPRINT intent, the api members to stack with catalogue anchors (exact spellings, the stacking rails they compose, the ' +
  'advanced surface the naive path would miss), seams it touches (both endpoints), packages it composes, watch-fors}; plus folder-wide facts ' +
  '(shared vocabulary, the folder rail/fault owners, the substrate rails every page composes, index-doc expectations). The map is a pointer, ' +
  'never a ceiling - the build agent re-reads and exceeds it. Verified members only; a member you cannot anchor to a catalogue line is marked ' +
  'RESEARCH in the map.'].join('\n')
const buildPrompt = (f, mapText) => [LAW, '', 'FOLDER READING MAP (API capture):\n' + mapText, '', 'TASK: BUILD the WHOLE folder ' + f.root +
  ' - you own EVERY page: ' + JSON.stringify(f.pages) + '. FIRST compose ' + TSD + '/ IN FULL (README plus language, derivation, values, computation, shapes, ' +
  'surfaces-and-dispatch, rails-and-effects, services-and-layers, concurrency, streams, boundaries), then the folder law sources. Then ' +
  'realize every stub ground-up into a decision-complete design page at the doctrine bar: dense ' +
  'polymorphic owners, one entrypoint family per rail, total dispatch, policy-as-values, fences transcription-complete with every ' +
  'generated-owner knob, union case, and entrypoint signature spelled exactly; .api ultra-stacking per the map AND beyond it - re-open the ' +
  'catalogues whenever a page needs depth the map did not carry; one shared folder vocabulary across all pages (rails, fault owners, receipts ' +
  'consistent page to page); anticipate 5x demand - never a thin slice. Build in dependency order (vocabulary-setting pages before consuming ' +
  'pages) and hold each finished page as settled law for the later ones. Before returning, cold-re-read the folder as one body and repair any ' +
  'cross-page drift you find - you are the only agent that will touch this folder. Cross-folder needs become DEFERRED residuals per the ' +
  'deferral law. Return pages (paths written) + residual.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length && !ALL) {
  log('ts-buildout: pass a folder path, an array, or "all" (args = "libs/typescript/<name>" | [...] | "all"). No-op.')
  return { folders: 0, pages: 0, residual: [] }
}

phase('Plan')
const plan = await agent(planPrompt(), { label: 'plan', phase: 'Plan', model: 'opus', effort: 'low', schema: PLAN, stallMs: STALL })
const folders = ((plan && plan.folders) || []).filter((f) => (f.pages || []).length).sort((a, b) => a.order - b.order)
if (!folders.length) { log('Plan resolved zero folders with pages - nothing to build.'); return { folders: 0, pages: 0, residual: [] } }
log('Plan: ' + folders.length + ' folder(s), ' + folders.reduce((n, f) => n + f.pages.length, 0) + ' page(s)')

const buildFolder = async (f) => {
  const disc = await agent(explorePrompt(f), { label: 'explore:' + f.name, phase: 'Explore', model: 'opus', effort: 'max', schema: MAP, stallMs: STALL })
  const mapText = (disc && disc.map) || '(exploration skipped - derive the map yourself from the .api tiers and law sources)'
  const built = await agent(buildPrompt(f, mapText), { label: 'build:' + f.name + ' (' + f.pages.length + ' pages)', phase: 'Build', effort: 'max', schema: BUILD, stallMs: STALL })
  if (!built) { log('Folder ' + f.name + ': build agent died - folder needs a re-run'); return { name: f.name, pages: 0, failed: true, residual: [] } }
  log('Folder ' + f.name + ': ' + (built.pages || []).length + ' page(s) built, ' + (built.residual || []).length + ' deferred residual(s)')
  return { name: f.name, pages: (built.pages || []).length, failed: false, residual: built.residual || [] }
}

const results = (await pool(folders, FOLDER_CAP, buildFolder)).filter(Boolean)
const residual = [...new Map(results.flatMap((r) => r.residual).map((r) => [(r.files || []).slice().sort().join(',') + '|' + r.claim, r])).values()]
const failed = results.filter((r) => r.failed).map((r) => r.name)
log('Build-out complete: ' + results.filter((r) => !r.failed).length + '/' + folders.length + ' folders; ' + residual.length + ' residual(s)' +
  (failed.length ? '; FAILED (re-run these): ' + failed.join(', ') : ''))

return { folders: results.filter((r) => !r.failed).length, pages: results.reduce((n, r) => n + r.pages, 0),
  perFolder: results.map((r) => ({ name: r.name, pages: r.pages })), failed, residual }
