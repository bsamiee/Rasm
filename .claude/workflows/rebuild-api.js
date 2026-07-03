export const meta = {
  name: 'rebuild-api',
  whenToUse: 'Rebuild every .api catalog under a target root to full integration-shaped capability.',
  description: 'Rebuild every .api catalog under a target root to FULL first-class, integration-shaped capability — document each package full advanced surface AND how packages STACK into single dense rails, verified against real members. Substrate-first: the shared tier (libs/<lang>/.api/) is rebuilt as a wave BEFORE folder tiers, so folder catalogs stack onto real rebuilt hubs, never stubs; folder batches never span folders and co-batch sibling families. Residual reconcile normalizes paths, clusters by shared file, packs work-balanced buckets, then fix -> adversarial verify. Language-agnostic: members verified via assay api over host DLLs / NuGet / Python distributions / node_modules. args = optional scope (e.g. "libs/python" or "libs/csharp/Rasm.Bim"); empty = all of libs.',
  phases: [
    { title: 'API-Discover', detail: 'list every .api catalog under the target from disk; _tmp/archives excluded' },
    { title: 'API-Substrate', detail: 'shared-tier catalogs (libs/<lang>/.api/) rebuilt first — the hub rails every folder tier stacks onto' },
    { title: 'API-Rebuild', detail: 'folder-tier batches, never spanning folders, sibling families co-batched, pooled at CAP=10' },
    { title: 'Reconcile', detail: 'residuals normalized -> clustered by shared file -> work-balance packed; fix -> adversarial verify per bucket' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const BATCH = 4 // .api files per agent — deep enough per file, many agents for parallelism
const STAGGER_MS = 1500
const TARGET_WORK = 10

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } }, repaired_files: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. .api catalogs are agent-facing declarative records of a package useful surface that DESIGN PAGES compose against. CLAUDE.md ' +
    'DEPENDENCY_POLICY: mine each admitted package to its FULL useful capability; prefer ecosystem primitives over reinvention; internalize ' +
    'capability into canonical owners; treat dependencies as first-class implementation surfaces. House .api format: header (package / version / ' +
    'license / build-floor / marker or target), then member sections grouped by concern, backticked symbols + signatures + a consumer/boundary ' +
    'note. NO provenance/process narration, NO freshness tails. Cite REAL members only — verify via `uv run --frozen python -m tools.assay api ' +
    'resolve <pkg>` (assay api owns external-artifact reflection over host DLLs, NuGet, installed Python distributions, and node_modules ' +
    'declarations per CLAUDE.md OWNER_ROUTING); fall back to the package source/official surface only if reflection is blocked, tagging the ' +
    'catalog accordingly. READ tools/assay/README.md FIRST for the assay api-arm contract (its resolve/decompile/reflection invocation, supported ' +
    'artifact kinds, and JSON output shape) so you drive it correctly rather than guessing flags.',
  'MANDATE — INTEGRATION-SHAPED, NOT SURFACE-LEVEL: a rebuilt .api documents (a) the package full ADVANCED surface (combinators, hooks, native ' +
    'pipelines, discriminators, async mirrors — not just the basic members), AND (b) the INTEGRATION patterns the dense design should compose — ' +
    'how this library STACKS with the other admitted libs into single rails (e.g. a decode hook feeding a discriminated model under a retry ' +
    'context with a telemetry span) — INCLUDING the SHARED/UNIVERSAL catalog tier where the language has one (`libs/python/.api/` for Python; ' +
    '`libs/typescript/.api/` for TypeScript), so a folder/area catalog documents stacking ONTO those universal rails, not only its sibling-folder ' +
    'libs. C# has NO central tier — its universals are Thinktecture/LanguageExt, and the `libs/csharp/Rasm/.api/` catalogs serve the ' +
    '`Rasm/Geometry` planning effort. The catalog GUIDES the rebuild toward first-class, stacked usage. Reject surface-level member lists.',
  'ADVERSARIAL LAW: every catalog is naive, shallow, or illusory until it survives attack — dense confident-looking catalogs are the prime ' +
    'suspect. Naivety is a defect on two orthogonal axes, both intolerable: COVERAGE — the catalog documents a thin slice of its package, the ' +
    'obvious members where the real surface carries far more; APPROACH — enumerated hardcoded instances where one parameterized pattern should ' +
    'own the space (a fixed roster of recipes, variants, or styles is seed DATA feeding one documented parameterized pattern, never the ' +
    'mechanism itself). Every defect list and capability-kind list in this prompt is a FLOOR, never the complete set — hunt past it: any ' +
    'repeated structure, parallel spelling, or enumerable family that one pattern, table, or parameterized rail can own is a collapse target ' +
    'you find yourself. ULTRA-STACKING: enumerate BOTH .api tiers in full from disk and mine each package to operator depth; an admitted ' +
    'capability the package carries but its catalog omits is a defect you close NOW; a cited member that cannot be verified is a phantom you ' +
    'delete NOW.',
  'RESIDUAL DISCIPLINE: a residual is ONLY a cross-CATALOG contradiction or gap you cannot close by editing the files in YOUR batch — never a ' +
    'note about a file outside the .api tiers, never a to-do restating work already done, never a proposal to admit a new package (that is a ' +
    'survey-packages item you name in your summary instead). Each residual names EVERY catalog endpoint it spans as a repo-relative path.',
  'WRITE-FULLY MANDATE: every correction you identify you MUST make NOW via Edit/Write directly in the .api file — the structured fix-log is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list or would/should hedge; leave nothing behind. Verdict=clean is EARNED by an attack that ' +
    'finds nothing, never conceded on first read — and never invent edits to force a verdict.',
].join('\n')

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
const rel = (f) => { const i = String(f).indexOf('libs/'); return i > 0 ? String(f).slice(i) : String(f) }
const isSubstrate = (f) => /^libs\/[^/]+\/\.api\//.test(f)
const folderOf = (f) => f.slice(0, f.indexOf('/.api/'))
const rebuildPrompt = (files, tier) => [
  LAW, '',
  'TASK: REBUILD these .api catalogs to FULL first-class, integration-shaped capability (fix-in-place, read-then-extend; never shrink real ' +
    'content): ' + files.join(', '),
  tier === 'substrate'
    ? 'These are SHARED-TIER catalogs — the universal rails every folder tier in this language stacks onto. Beyond the package own advanced ' +
      'surface, document the ANCHOR members downstream catalogs compose against (the service tags, carriers, schema/codec entrypoints, ' +
      'layer/runtime constructors, and cross-package seams that make this the hub) at operator depth — a folder catalog written after you must ' +
      'find every rail it stacks onto already documented here.'
    : 'The shared substrate tier for this language has ALREADY been rebuilt in this run. Read the substrate catalogs your stacking notes ' +
      'compose against FROM DISK and verify every stacking claim against their REAL rebuilt content — a stacking claim written from memory is ' +
      'a phantom; a residual proposing "the hub must document X" is valid ONLY when the hub, as rebuilt, truly omits X. Your batch is one ' +
      'folder: unify the row grammar across sibling catalogs in the batch (same family, same shape — provider rows, client/layer/config ' +
      'spellings, asymmetry columns) so siblings read as one family, never divergent one-offs.',
  'For EACH file run the same 3-lens write: (1) EXTRACT-FULL — confirm the package and document its full useful ADVANCED surface ' +
    '(combinators/hooks/async mirrors/discriminators/native pipelines — a floor, not the set), not the basic subset; (2) REFINE/REFACTOR — ' +
    'restructure to integration-shaped, documenting how this lib STACKS with the universal-tier rails AND sibling admitted libs into single ' +
    'dense rails; (3) HARDEN — the terminal, most aggressive review: attack BOTH naivety axes (COVERAGE thin-slice, APPROACH ' +
    'enumerated-instances-where-one-parameterized-pattern-owns-the-space), then remove every phantom member, wrong floor/marker/target, ' +
    'surface-level framing, missing license/ABI/runtime flag, and un-stacked single-feature framing — a defect list you hunt past — and end ' +
    'with a full cold re-read of each finished catalog. Verify members via `uv run --frozen python -m tools.assay api resolve`. Also close any ' +
    'gap a consuming design page genuinely needs (a specific member/signature the design composes). Return the fix-log (files + verdict + ' +
    'residual per the RESIDUAL DISCIPLINE; everything within these catalogs you fix yourself).',
].join('\n')
const processBatch = (tier) => async (w) => {
  const r = await agent(rebuildPrompt(w.files, tier), { label: 'api:' + w.files[0].split('/.api/')[0].split('/').pop() + '+' + (w.files.length - 1), phase: tier === 'substrate' ? 'API-Substrate' : 'API-Rebuild', schema: FIXLOG_SCHEMA, model: 'opus', effort: 'max', stallMs: 300000 })
  return r ? { files: w.files, log: r } : null
}
const clusterWork = (cl) => 2 * new Set(cl.flatMap((r) => r.files)).size + cl.length
const packClusters = (clusters) => {
  const sorted = clusters.slice().sort((a, b) => clusterWork(b) - clusterWork(a))
  const total = sorted.reduce((s, c) => s + clusterWork(c), 0)
  const n = Math.max(1, Math.min(CAP, sorted.length, Math.ceil(total / TARGET_WORK)))
  const buckets = Array.from({ length: n }, () => ({ w: 0, rows: [] }))
  for (const cl of sorted) { const b = buckets.reduce((m, x) => (x.w < m.w ? x : m)); b.w += clusterWork(cl); b.rows.push(...cl) }
  return buckets.filter((b) => b.rows.length).map((b) => b.rows)
}

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('API-Discover')
const inv = await agent('Enumerate every .api catalog file under ' + SWEEP + ' from REAL disk state — one find listing over */.api/*.md (and any ' +
  'nested .api subdirs), never a memory-recall inventory: BOTH tiers the scope contains, the shared/universal tier (libs/<lang>/.api/) AND every ' +
  'folder tier (libs/<lang>/<folder>/.api/). EXCLUDE archive and scratch trees: any path segment _tmp, _archive, or node_modules. Return each as ' +
  'a repo-relative path — this listing is the ground truth downstream batches resolve against, an initial pointer never a ceiling. If none ' +
  'exist, return an empty list. Use find; do not cd.', { label: 'discover', phase: 'API-Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const FILES = [...new Set(((inv && inv.files) || []).filter(Boolean).map(rel))].filter((f) => !/(^|\/)(_tmp|_archive|node_modules)\//.test(f))
const T0 = FILES.filter(isSubstrate).sort()
const T1 = FILES.filter((f) => !isSubstrate(f) && f.includes('/.api/'))
const byFolder = new Map()
for (const f of T1) { const k = folderOf(f); if (!byFolder.has(k)) byFolder.set(k, []); byFolder.get(k).push(f) }
// Folder batches never span folders; sorted filenames keep sibling families (shared-prefix rows) adjacent in one batch.
const t0Batches = chunk(T0, BATCH).map((files) => ({ files }))
const t1Batches = [...byFolder.keys()].sort().flatMap((k) => chunk(byFolder.get(k).sort(), BATCH).map((files) => ({ files })))
const totalFiles = T0.length + T1.length
const totalBatches = t0Batches.length + t1Batches.length
log('API discover under ' + SWEEP + ': ' + totalFiles + ' catalogs (' + T0.length + ' substrate + ' + T1.length + ' folder-tier across ' +
  byFolder.size + ' folders) in ' + totalBatches + ' batches; pooling at CAP=' + CAP)

// --- [API_SUBSTRATE]
// Barrier by construction: folder tiers stack onto the substrate hubs, so the hubs land first.
let doneT0 = []
if (t0Batches.length) {
  phase('API-Substrate')
  doneT0 = (await pool(t0Batches, CAP, processBatch('substrate'))).filter(Boolean)
  log('Substrate wave: ' + doneT0.length + '/' + t0Batches.length + ' batches (' + T0.length + ' hub catalogs)')
}

// --- [API_REBUILD]
phase('API-Rebuild')
const doneT1 = (await pool(t1Batches, CAP, processBatch('folder'))).filter(Boolean)
const done = [...doneT0, ...doneT1]

// --- [RECONCILE]
const allRes = []
for (const r of done) if (r.log && r.log.residual) for (const x of r.log.residual) {
  const raw = typeof x === 'string' ? { files: r.files, claim: x } : { files: (x.files && x.files.length) ? x.files : r.files, claim: x.claim }
  const files = raw.files.map(rel).filter((f) => !/(^|\/)(_tmp|_archive|node_modules)\//.test(f))
  if (raw.claim) allRes.push({ files, claim: raw.claim })
}
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  const anchored = uniq.filter((r) => r.files.length)
  for (const r of anchored) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of anchored) { const root = find(r.files[0]); if (!by.has(root)) by.set(root, []); by.get(root).push(r) }
  const out = [...by.values()]
  const loose = uniq.filter((r) => !r.files.length)
  if (loose.length) out.push(loose)
  return out
})()
const buckets = packClusters(clusters)
log('API rebuild: ' + done.length + '/' + totalBatches + ' batches (' + totalFiles + ' catalogs); reconcile ' + uniq.length + ' residuals -> ' +
  clusters.length + ' clusters -> ' + buckets.length + ' balanced buckets [' + buckets.map((b) => b.length).join(', ') + ']')
let reconciled = []
if (buckets.length) {
  phase('Reconcile')
  reconciled = (await pipeline(
    buckets,
    (cl) => agent([LAW, '', 'TASK: RECONCILE these cross-CATALOG residuals the per-batch pass deferred. There is NO severity — address EVERY ' +
      'residual. Read EVERY listed .api catalog IN FULL, make the cross-catalog fix in place at its ROOT — the objectively-best form of the same ' +
      'catalogs, never a token alignment (add the depended-on member/signature, align the stacking note across catalogs), verify members via ' +
      '`uv run --frozen python -m tools.assay api resolve`, never shrink real content. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, model: 'opus', effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', 'TASK: ADVERSARIAL WRITING VERIFY — never a friendly confirmation, never read-only. A reconcile agent ' +
      'claims to have fixed the cross-catalog residuals below. Per claim: (a) re-derive from the catalogs whether the claimed fix was necessary ' +
      'at all; (b) read every named catalog from disk and PROVE the fix landed properly, re-verifying every cited member via `uv run --frozen ' +
      'python -m tools.assay api resolve`; (c) REPAIR every loose, weak, or token fix in place NOW via Edit/Write to the objectively-best ' +
      'root-level form of the same catalogs — a single-point patch where a root-level denser reconstruction is available is itself a defect you ' +
      'repair; (d) only then classify: status "fixed" (real defect, resolved on disk — your own repair counts), "invalid" (the claim is ' +
      'factually wrong — cite why, only when provably wrong), or "open" (RESERVED for claims genuinely unreachable from the catalogs at hand — ' +
      'never a punt on a strengthenable fix you could make yourself). Prove against disk, never trust the fixer summary. One verdict per claim ' +
      'plus overall; list every catalog you edited in repaired_files. Claims:\n' + JSON.stringify(cl, null, 1) + '\nCatalogs touched by the fixer: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, model: 'opus', effort: 'xhigh', stallMs: 300000 }).then((v) => ({ cluster: cl, fix, verify: v })) : null,
  )).filter(Boolean)
}
const spanOf = new Map(uniq.map((r) => [r.claim, r.files]))
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_residual = claimsAll.filter((c) => c.status === 'open').map((c) => ({ files: spanOf.get(c.claim) || [], claim: c.claim }))
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
log('Reconcile: ' + buckets.length + ' buckets; ' + hard_residual.length + ' open (hard residual), ' + dropped.length + ' dropped as invalid')
return { scope: SWEEP, catalogs: totalFiles, batches: totalBatches, complete: done.length, clusters: clusters.length, hard_residual: hard_residual, dropped: dropped }
