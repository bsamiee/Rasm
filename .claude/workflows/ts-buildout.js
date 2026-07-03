export const meta = {
  name: 'ts-buildout',
  description: 'Realize every one-line page stub across the libs/typescript folders into deep, decision-complete design pages at the rebuilt docs/stacks/typescript bar. All-fable, phase-barriered waves: Scout (1 agent resolves folders + stub inventories in BLUEPRINT build order) -> Build (1 agent per folder: composes the doctrine in full, mines BOTH .api tiers itself, realizes every page ground-up) -> Critique (1 agent per folder: hostile conformance audit + fix in place) -> Redteam (1 agent per folder: most aggressive attack + fix in place) -> Harden (batches of 2-3 folders per agent: cross-folder unification, extension, terminal cold read). No in-run reconcile: cross-batch work is DEFERRED as {files, claim} residuals returned as data for one operator-launched alignment agent. args = a folder path, an array, or the string all; empty = no-op. Ephemeral: delete after the build-out lands.',
  whenToUse: 'Campaign stage 5, after the doctrine rebuild lands: fill the stood-up TS folder stubs with high depth, folder by folder or all at once.',
  phases: [
    { title: 'Scout', detail: '1 fable agent resolves targets to folders + per-folder page-stub inventories in BLUEPRINT build order' },
    { title: 'Build', detail: 'wave: 1 fable agent per folder — doctrine composed in full, BOTH .api tiers mined to operator depth, every page realized ground-up' },
    { title: 'Critique', detail: 'wave: 1 fable agent per folder — hostile doctrinal-conformance audit, every hit fixed in place, folder-scoped writes' },
    { title: 'Redteam', detail: 'wave: 1 fable agent per folder — most aggressive architect attack, fixes in place, folder-scoped writes' },
    { title: 'Harden', detail: 'wave: batches of 2-3 folders per fable agent — cross-folder unification, capability extension, terminal cold read' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const TS = 'libs/typescript'
const TSD = 'docs/stacks/typescript'
const BLUEPRINT = 'libs/typescript/BLUEPRINT.md'
const DECISION = 'RASM-TS-PLATFORM-DECISION.md'
const CAP = 6
const BATCH = 3
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
const SCOUT = { type: 'object', additionalProperties: false, required: ['folders'], properties: {
  folders: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'root', 'order', 'pages'], properties: {
    name: { type: 'string' }, root: { type: 'string' }, order: { type: 'number' },
    pages: { type: 'array', items: { type: 'string' } } } } } } }
const FIXLOG = { type: 'object', additionalProperties: false, required: ['pages', 'summary'], properties: {
  pages: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' },
  residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: {
    files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = 'Rasm monorepo, TS catch-up campaign stage 5 (build-out). The ' + TS + ' folders were stood up per ' + DECISION + ' + ' + BLUEPRINT +
  ': authored README/ARCHITECTURE, EMPTY IDEAS/TASKLOG placeholders (NEVER open or fill them), one-line page stubs under <folder>/.planning/ ' +
  'mirroring the BLUEPRINT trees, and rebuilt .api catalogues at both tiers. ' + TSD + '/ is the rebuilt route-owned doctrine: every writing ' +
  'agent composes it IN FULL (README plus language, derivation, values, computation, shapes, surfaces-and-dispatch, rails-and-effects, ' +
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
  'parity, GLB consumed, TS owns no geometry). DEFERRAL LAW: cross-folder or cross-language work you cannot complete inside your scope is ' +
  'DEFERRED as a residual {files: [both endpoint files], claim} returned as data - never half-edit a foreign folder, never drop the item silently.'

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
const batch = (arr, size) => {
  const n = Math.max(1, Math.ceil(arr.length / size))
  const base = Math.floor(arr.length / n)
  const rem = arr.length % n
  const out = []
  let at = 0
  for (let i = 0; i < n; i++) { const len = base + (i < rem ? 1 : 0); out.push(arr.slice(at, at + len)); at += len }
  return out.filter((b) => b.length)
}
const readList = (f) => 'Read IN FULL, in this order: ' + TSD + '/ (every page enumerated above), EVERY catalogue in ' + TS + '/.api/ AND ' +
  'EVERY catalogue in ' + f.root + '/.api/ (ls both tiers first - enumerate completely, then read each catalogue whole), the ' + f.name +
  ' section of ' + BLUEPRINT + ', the ' + f.name + ' leg of ' + DECISION + ', the folder README.md + ARCHITECTURE.md, and every page: ' +
  JSON.stringify(f.pages) + '.'
const scoutPrompt = () => [LAW, '', 'TASK: SCOUT. Resolve the build-out scope against real disk state. TARGETS: ' +
  (TARGETS.length ? JSON.stringify(TARGETS) : '(all folders)') + '. Read the build-order section of ' + BLUEPRINT + ' for the folder order; ' +
  'for each in-scope folder, fd/ls its .planning/ tree and list every design-page stub (relative .md paths). A page already realized (more ' +
  'than ~5 lines) is listed too - the build agent deepens rather than re-stubs. Return folders sorted by build order, each {name, root ' +
  '(' + TS + '/<name>), order, pages}. Folders with zero pages are omitted. Read-only; do not edit.'].join('\n')
const buildPrompt = (f) => [LAW, '', 'TASK: BUILD the WHOLE folder ' + f.root + ' - you own EVERY page: ' + JSON.stringify(f.pages) + '. ' +
  readList(f) + ' Then realize every stub ground-up into a decision-complete design page at the doctrine bar: dense polymorphic owners, one ' +
  'entrypoint family per rail, total dispatch, policy-as-values, fences transcription-complete with every generated-owner knob, union case, ' +
  'and entrypoint signature spelled exactly; .api ultra-stacking to operator depth - re-open the catalogues whenever a page needs more; one ' +
  'shared folder vocabulary across all pages (rails, fault owners, receipts consistent page to page); anticipate 5x demand - never a thin ' +
  'slice. Build in dependency order (vocabulary-setting pages before consuming pages) and hold each finished page as settled law for the ' +
  'later ones. Before returning, cold-re-read the folder as one body and repair any cross-page drift. Edit ONLY under ' + f.root + '/. ' +
  'Cross-folder needs become DEFERRED residuals per the deferral law. Return pages (paths written), summary, residual.'].join('\n')
const critiquePrompt = (f) => [LAW, '', 'TASK: CRITIQUE the folder ' + f.root + ' - HOSTILE doctrinal-conformance audit + FIX IN PLACE. ' +
  'ULTRA-HARSH, UNAGREEABLE: assume a violation exists on EVERY page; trust NOTHING the pages claim about themselves; "good enough" is ' +
  'rejected. ' + readList(f) + ' Sibling folders under ' + TS + '/ are context (read their pages where a seam touches them) but you EDIT ' +
  'ONLY under ' + f.root + '/ - a genuinely cross-folder defect is a residual, never a sibling edit. Audit and repair every hit: doctrine ' +
  'conformance page by page (shape budget, schema authority, export law + export-surface collapse, effect rails, value-derived vocabulary, ' +
  'inference pre-solving); fence-vs-catalogue truth (every named member anchored to an .api line - an unanchored member is a PHANTOM you ' +
  'delete or mark RESEARCH); BLUEPRINT intent conformance per page; transcription-completeness (an implementer copies the fence verbatim - ' +
  'no elided signatures, no hand-waved bodies where the body is the law); folder-wide vocabulary consistency; density without spam. Every ' +
  'hit is a fix, never a note. Return pages (paths edited), summary, residual.'].join('\n')
const redteamPrompt = (f) => [LAW, '', 'TASK: RED-TEAM the folder ' + f.root + ' - the MOST AGGRESSIVE per-folder stage; critique AND MORE; ' +
  'the burden of proof is ON THE FOLDER; trust NOTHING the prior waves claimed. ' + readList(f) + ' EDIT ONLY under ' + f.root + '/. ' +
  'LENSES: (A) COUNTERFACTUAL - does a denser canonical owner, a richer generated family, or a deeper admitted primitive collapse a whole ' +
  'page`s design? rebuild to it. (B) ANTICIPATORY growth - does the next case/variant/modality land as ONE row/tag/policy value with ' +
  'consumers broken loudly? reshape until it does. (C) NAIVETY on two axes - COVERAGE (the owner models a thin slice of its concept: extend ' +
  'to the full family the BLUEPRINT + .api capability admits) and APPROACH (enumerated hardcoded instances where a parameterized generator ' +
  'should own the space: roster becomes seed data). (D) ILLUSION - prose asserting capability the fence lacks, confident-looking fences ' +
  'demonstrating thin slices, phantom members: rebuild or delete. (E) SEAM TRUTH - every seam names both endpoints and the endpoint ' +
  'spelling is real (verify against the sibling page; a mismatch inside your folder you fix, a mismatch in the sibling is a residual). ' +
  'Fix everything in place; if the strongest form is genuinely present, prove it by finding nothing - never invent churn. Return pages ' +
  '(paths edited), summary, residual.'].join('\n')
const hardenPrompt = (fs) => [LAW, '', 'TASK: HARDEN the folder batch ' + JSON.stringify(fs.map((f) => f.root)) + ' - the terminal ' +
  'improvement/extension sweep; you own EVERY page in EVERY listed folder and you are the only agent touching them. FIRST compose ' + TSD +
  '/ IN FULL, then per folder its law sources (' + BLUEPRINT + ' section, ' + DECISION + ' leg, README + ARCHITECTURE, both .api tiers) and ' +
  'every page. THEN: (1) UNIFY across the batch - one shared vocabulary for rails, fault owners, receipts, and policy forms; every ' +
  'cross-folder seam INSIDE the batch recorded on both endpoint pages with matching spellings; altitude clean (an owner teaches its ' +
  'mechanic once, consumers compose it). (2) EXTEND - close every capability gap the concept or the .api catalogues admit that the pages ' +
  'omit; deepen thin owners to the 5x-demand shape; harden fences to transcription-complete at full operator depth. (3) COLD READ - final ' +
  'hostile pass over every page as one body: fix residual weakness, hedges, meta, thin cards, under-dense fences; never invent churn where ' +
  'the bar is genuinely met. Edit ONLY under the listed folder roots. Cross-BATCH needs become DEFERRED residuals per the deferral law. ' +
  'Return pages (paths edited), summary, residual.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length && !ALL) {
  log('ts-buildout: pass a folder path, an array, or "all" (args = "libs/typescript/<name>" | [...] | "all"). No-op.')
  return { folders: 0, pages: 0, residual: [] }
}

phase('Scout')
const scout = await agent(scoutPrompt(), { label: 'scout', phase: 'Scout', model: 'fable', effort: 'low', schema: SCOUT, stallMs: STALL })
const folders = ((scout && scout.folders) || []).filter((f) => (f.pages || []).length).sort((a, b) => a.order - b.order)
if (!folders.length) { log('Scout resolved zero folders with pages - nothing to build.'); return { folders: 0, pages: 0, residual: [] } }
log('Scout: ' + folders.length + ' folder(s), ' + folders.reduce((n, f) => n + f.pages.length, 0) + ' page(s)')

// --- [BUILD]
phase('Build')
const builds = await pool(folders, CAP, (f) =>
  agent(buildPrompt(f), { label: 'build:' + f.name + ' (' + f.pages.length + ' pages)', phase: 'Build', model: 'fable', effort: 'max', schema: FIXLOG, stallMs: STALL })
    .then((r) => ({ f, log: r })))
const failed = builds.filter((b) => !b.log).map((b) => b.f.name)
const live = builds.filter((b) => b.log).map((b) => b.f)
log('Build wave: ' + live.length + '/' + folders.length + ' folders built' + (failed.length ? '; FAILED (re-run these): ' + failed.join(', ') : ''))

// --- [CRITIQUE]
phase('Critique')
const crits = await pool(live, CAP, (f) =>
  agent(critiquePrompt(f), { label: 'critique:' + f.name, phase: 'Critique', model: 'fable', effort: 'xhigh', schema: FIXLOG, stallMs: STALL })
    .then((r) => ({ f, log: r })))
log('Critique wave: ' + crits.filter((c) => c.log).length + '/' + live.length + ' folders audited')

// --- [REDTEAM]
phase('Redteam')
const reds = await pool(live, CAP, (f) =>
  agent(redteamPrompt(f), { label: 'redteam:' + f.name, phase: 'Redteam', model: 'fable', effort: 'max', schema: FIXLOG, stallMs: STALL })
    .then((r) => ({ f, log: r })))
log('Redteam wave: ' + reds.filter((c) => c.log).length + '/' + live.length + ' folders attacked')

// --- [HARDEN]
phase('Harden')
const batches = batch(live, BATCH)
const hardens = await pool(batches, CAP, (fs) =>
  agent(hardenPrompt(fs), { label: 'harden:' + fs.map((f) => f.name).join('+'), phase: 'Harden', model: 'fable', effort: 'max', schema: FIXLOG, stallMs: STALL })
    .then((r) => ({ fs, log: r })))
log('Harden wave: ' + hardens.filter((h) => h.log).length + '/' + batches.length + ' batches hardened')

const logs = [...builds, ...crits, ...reds, ...hardens].map((x) => x.log).filter(Boolean)
const residual = [...new Map(logs.flatMap((l) => l.residual || []).filter((r) => r && r.claim)
  .map((r) => [(r.files || []).slice().sort().join(',') + '|' + r.claim, r])).values()]
const pagesBuilt = builds.filter((b) => b.log).reduce((n, b) => n + (b.log.pages || []).length, 0)
log('Build-out complete: ' + live.length + '/' + folders.length + ' folders; ' + pagesBuilt + ' pages; ' + residual.length + ' residual(s)' +
  (failed.length ? '; FAILED (re-run these): ' + failed.join(', ') : ''))

return { folders: live.length, pages: pagesBuilt, perFolder: builds.filter((b) => b.log).map((b) => ({ name: b.f.name, pages: (b.log.pages || []).length })),
  failed, residual }
