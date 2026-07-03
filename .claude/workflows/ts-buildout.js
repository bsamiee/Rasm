export const meta = {
  name: 'ts-buildout',
  description: 'Realize every one-line page stub across the libs/typescript folders into deep, decision-complete design pages at the rebuilt docs/stacks/typescript bar. Plan (1 agent expands targets to folders + stub inventories in the BLUEPRINT build order) -> per folder, pooled: Discover (1 opus agent reads the BLUEPRINT section + DECISION leg + index docs + BOTH .api tiers + every stub, emits the reading map) -> Implement (1 fable agent per 2 pages, ground-up authoring with .api ultra-stacking) -> Critique (1 per 3 pages, xhigh, mechanical fix-in-place) -> Redteam (1 per 3 pages, max, six lenses + folder seam coherence, fix-in-place). Build-first: NO cross-folder reconcile - cross-folder work is DEFERRED as {files, claim} residuals returned for the later ts-align pass. args = a folder path, an array, or the string all; empty = no-op. Ephemeral: delete after the build-out lands.',
  whenToUse: 'Campaign stage 5, after the doctrine rebuild lands: fill the stood-up TS folder stubs with high depth, folder by folder or all at once.',
  phases: [
    { title: 'Plan', detail: '1 agent resolves targets to folders + per-folder page-stub inventories in BLUEPRINT build order' },
    { title: 'Discover', detail: 'per folder: 1 opus reading-map agent over BLUEPRINT section, DECISION leg, index docs, both .api tiers, every stub' },
    { title: 'Implement', detail: 'per folder: 1 fable max agent per 2 pages, ground-up authoring, verified members only' },
    { title: 'Critique', detail: 'per folder: 1 fable xhigh agent per 3 pages, mechanical doctrinal audit fixed in place' },
    { title: 'Redteam', detail: 'per folder: 1 fable max agent per 3 pages, six lenses + folder seam coherence, fixed in place' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const TS = 'libs/typescript'
const TSD = 'docs/stacks/typescript'
const BLUEPRINT = 'libs/typescript/BLUEPRINT.md'
const DECISION = 'RASM-TS-PLATFORM-DECISION.md'
const FOLDER_CAP = 3
const STAGGER_MS = 2000
const IMPL_BATCH = 2
const REVIEW_BATCH = 3

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
const IMPL = { type: 'object', additionalProperties: false, required: ['pages'], properties: {
  pages: { type: 'array', items: { type: 'string' } },
  residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: {
    files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = 'Rasm monorepo, TS catch-up campaign stage 5 (build-out). The ' + TS + ' folders were stood up per ' + DECISION + ' + ' + BLUEPRINT +
  ': authored README/ARCHITECTURE, EMPTY IDEAS/TASKLOG placeholders (NEVER open or fill them), one-line page stubs under <folder>/.planning/ ' +
  'mirroring the BLUEPRINT trees, and rebuilt .api catalogues at both tiers. ' + TSD + '/ is the rebuilt route-owned doctrine: any agent that ' +
  'writes fences composes it IN FULL (README + every concept page) before writing. THE PRODUCT: each one-line stub becomes a decision-complete ' +
  'design page - H1 [<PKG>_<PAGE>], one declarative lead, section [1] the cluster index, cluster cards (owner, packages, growth, earned ' +
  'boundary/receipt/entry lines), transcription-complete signature fences an implementer copies verbatim (bodies where the body is the law, ' +
  'ZERO comments in fences, invariants on the card), at most one mermaid per cluster. ULTRA-STACKING: BOTH .api tiers (' + TS + '/.api/ ' +
  'substrate + <folder>/.api/ domain) are enumerated in full and mined to operator depth; an admitted capability the concept admits but no ' +
  'owner exploits is a defect; an external member is written ONLY at a spelling an .api catalogue verifies - an unverifiable member is a ' +
  'RESEARCH item, never settled fence code. LAW SOURCES (read, adhere, never restate): the folder section of ' + BLUEPRINT + ' + the folder ' +
  'leg of ' + DECISION + ' (binding per-file intent), the folder README + ARCHITECTURE, libs/.planning/{architecture,campaign-method,README}.md ' +
  'where a boundary question arises. HARD CONSTRAINTS: lib-not-app; zero-YAML IaC; no-migration event-sourced persistence; sql lanes pg + ' +
  'sqlite; alignment-never-coupling (C# owns the wire vocabulary, XxHash128 content-key parity, GLB consumed, TS owns no geometry). ' +
  'DEFERRAL LAW: cross-folder or cross-language work you cannot complete inside your folder is DEFERRED as a residual {files: [both endpoint ' +
  'files], claim} returned as data for the later ts-align pass - never half-edit a sibling folder, never drop the item silently.'

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
const chunk = (arr, n) => { const out = []; for (let i = 0; i < arr.length; i += n) out.push(arr.slice(i, i + n)); return out }
const planPrompt = () => [LAW, '', 'TASK: PLAN. Resolve the build-out scope against real disk state. TARGETS: ' +
  (TARGETS.length ? JSON.stringify(TARGETS) : '(all folders)') + '. Read the build-order section of ' + BLUEPRINT + ' for the folder order; ' +
  'for each in-scope folder, fd/ls its .planning/ tree and list every design-page stub (relative .md paths). A page already realized (more ' +
  'than ~5 lines) is listed too - the implement agent deepens rather than re-stubs. Return folders sorted by build order, each {name, root ' +
  '(' + TS + '/<name>), order, pages}. Folders with zero pages are omitted.'].join('\n')
const discoverPrompt = (f) => [LAW, '', 'TASK: DISCOVER (read-only reading map) for ' + f.root + '. Read IN FULL: the ' + f.name + ' section ' +
  'of ' + BLUEPRINT + ', the ' + f.name + ' leg of ' + DECISION + ', the folder README.md + ARCHITECTURE.md, EVERY catalogue in ' + TS +
  '/.api/ and ' + f.root + '/.api/ (ls first - enumerate both tiers completely), and every page stub: ' + JSON.stringify(f.pages) + '. Emit ' +
  'ONE dense reading map: per page {the owned concern and BLUEPRINT intent, the api members to stack with catalogue anchors, seams it touches ' +
  '(both endpoints), packages it composes, watch-fors}; plus folder-wide facts (shared vocabulary, the folder rail/fault owners, index-doc ' +
  'expectations). The map is a pointer, never a ceiling - implementers re-read and exceed it. Verified members only; a member you cannot ' +
  'anchor to a catalogue line is marked RESEARCH in the map.'].join('\n')
const implPrompt = (f, mapText, pages) => [LAW, '', 'FOLDER READING MAP:\n' + mapText, '', 'TASK: IMPLEMENT ' + pages.length + ' page(s) of ' +
  f.root + ' GROUND-UP: ' + JSON.stringify(pages) + '. Compose ' + TSD + '/ IN FULL first (README + every concept page), then the folder law ' +
  'sources. Each stub becomes a complete design page at the doctrine bar: dense polymorphic owners, one entrypoint family per rail, total ' +
  'dispatch, policy-as-values, fences transcription-complete with every generated-owner knob, union case, and entrypoint signature spelled ' +
  'exactly; .api ultra-stacking per the map and beyond it; growth axes on every cluster card. Anticipate 5x demand - never a thin slice. ' +
  'Cross-folder needs become DEFERRED residuals per the deferral law. Return pages (paths written) + residual.'].join('\n')
const critiquePrompt = (f, pages) => [LAW, '', 'TASK: CRITIQUE ' + pages.length + ' page(s) of ' + f.root + ': ' + JSON.stringify(pages) +
  ' - the mechanical line-by-line doctrinal audit; every hit FIXED IN PLACE via Edit. Compose ' + TSD + '/ in full first. Checklist floor ' +
  '(hunt past it): collapse scan (parallel shapes, arity twins, literal-differing functions, one-hop wrappers); owner choice and shape budget; ' +
  'knob test (a parallel parameter re-describing the input is smuggled arity); rails unified (one fault family, Option/typed errors, zero ' +
  'throw in domain flow); fence truthfulness against BOTH .api tiers (spot-verify every external member - an unverified member becomes a ' +
  'RESEARCH item); page grammar and card economy; BLUEPRINT intent honored per file; zero meta/provenance. Cross-folder hits become DEFERRED ' +
  'residuals. Return pages + residual.'].join('\n')
const redteamPrompt = (f, pages) => [LAW, '', 'TASK: RED-TEAM ' + pages.length + ' page(s) of ' + f.root + ': ' + JSON.stringify(pages) +
  ' - the terminal aggressive review; every defect FIXED IN PLACE via Edit. Six lenses: COUNTERFACTUAL on the core owner/algebra/dispatch of ' +
  'each page (categorically stronger form available under the doctrine?); DIFF-OF-NEXT-FEATURE (the next case/modality lands as one row with ' +
  'every consumer untouched or loudly broken at type-check); LONG-TAIL failure modes (backpressure, cancellation, partial failure, tenancy, ' +
  'retention); BOUNDARY integrity (folder charter respected, edge-ledger imports only, wire law absolute); SURFACE SPRAWL + phantom members ' +
  '(re-verify novel members; kill sprawl by collapse, never by capability loss); DOMAIN COMPLETENESS (the concept at 5x demand); then a cold ' +
  're-review of every critique dimension + folder coherence (shared vocabulary consistent across the folder pages, README router intents ' +
  'still true - fix the router line when a page outgrew it). Cross-folder hits become DEFERRED residuals. Return pages + residual.'].join('\n')

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
  const disc = await agent(discoverPrompt(f), { label: 'discover:' + f.name, phase: 'Discover', model: 'opus', effort: 'max', schema: MAP, stallMs: STALL })
  const mapText = (disc && disc.map) || '(discovery skipped - derive the map yourself from the law sources)'
  const implOut = (await parallel(chunk(f.pages, IMPL_BATCH).map((pages, i) => () =>
    agent(implPrompt(f, mapText, pages), { label: 'impl:' + f.name + ':' + (i + 1), phase: 'Implement', effort: 'max', schema: IMPL, stallMs: STALL })))).filter(Boolean)
  const critOut = (await parallel(chunk(f.pages, REVIEW_BATCH).map((pages, i) => () =>
    agent(critiquePrompt(f, pages), { label: 'crit:' + f.name + ':' + (i + 1), phase: 'Critique', effort: 'xhigh', schema: IMPL, stallMs: STALL })))).filter(Boolean)
  const redOut = (await parallel(chunk(f.pages, REVIEW_BATCH).map((pages, i) => () =>
    agent(redteamPrompt(f, pages), { label: 'red:' + f.name + ':' + (i + 1), phase: 'Redteam', effort: 'max', schema: IMPL, stallMs: STALL })))).filter(Boolean)
  const residual = [...implOut, ...critOut, ...redOut].flatMap((r) => r.residual || [])
  log('Folder ' + f.name + ': ' + f.pages.length + ' page(s) built, ' + residual.length + ' deferred residual(s)')
  return { name: f.name, pages: f.pages.length, residual }
}

const results = (await pool(folders, FOLDER_CAP, buildFolder)).filter(Boolean)
const residual = [...new Map(results.flatMap((r) => r.residual).map((r) => [(r.files || []).slice().sort().join(',') + '|' + r.claim, r])).values()]
log('Build-out complete: ' + results.length + '/' + folders.length + ' folders; ' + residual.length + ' residual(s) for ts-align')

return { folders: results.length, pages: results.reduce((n, r) => n + r.pages, 0), perFolder: results.map((r) => ({ name: r.name, pages: r.pages })),
  residual }
