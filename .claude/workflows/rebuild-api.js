export const meta = {
  name: 'rebuild-api',
  whenToUse: 'Rebuild every .api catalog under a target root to full integration-shaped capability.',
  description: 'Rebuild every .api catalog under a target root to FULL first-class, integration-shaped capability — document each package full advanced surface AND how packages STACK into single dense rails, verified against real members. Substrate-first: the shared tier (libs/<lang>/.api/) is rebuilt BEFORE folder tiers — the one true barrier, so folder catalogs stack onto real rebuilt hubs, never stubs; within each side every batch runs concurrently under one pool cap. Folder batches never span folders and co-batch sibling families as the WORK PARTITION, never a write fence: every batch fixes any catalog its work exposes — either tier, in or out of its batch — in the same pass under the current-state law, so the run ends closed in one pass. Language-agnostic: members verified via assay api over host DLLs / NuGet / Python distributions / node_modules, falling back to the nuget MCP / Context7 / source tier when reflection is blocked. args = optional scope (e.g. "libs/python" or "libs/csharp/Rasm.Bim"); empty = all of libs.',
  phases: [
    { title: 'API-Discover', detail: 'list every .api catalog under the target from disk; _tmp/archives excluded' },
    { title: 'API-Substrate', detail: 'shared-tier catalogs (libs/<lang>/.api/) rebuilt first — the hub rails every folder tier stacks onto' },
    { title: 'API-Rebuild', detail: 'folder-tier batches, never spanning folders, sibling families co-batched, pooled at CAP=10; every cross-catalog defect fixed in-pass' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const BATCH = 4 // .api files per agent — deep enough per file, many agents for parallelism
const STAGGER_MS = 1500
const STALL = 300000

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'string' } } } }
// Required-but-possibly-empty `beyondBatch` is an attestation: the cross-catalog hunt ran and every exposed defect landed in-pass.
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'beyondBatch', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, beyondBatch: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. .api catalogs are agent-facing declarative records of a package useful surface that DESIGN PAGES compose against. CLAUDE.md ' +
    'DEPENDENCY_POLICY: mine each admitted package to its FULL useful capability; prefer ecosystem primitives over reinvention; internalize ' +
    'capability into canonical owners; treat dependencies as first-class implementation surfaces. House .api format: header (package / version / ' +
    'license / build-floor / marker or target), then member sections grouped by concern, backticked symbols + signatures + a consumer/boundary ' +
    'note. NO provenance/process narration, NO freshness tails. Cite REAL members only — verify via `uv run --frozen python -m tools.assay api ' +
    'resolve <pkg>` (assay api owns external-artifact reflection over host DLLs, NuGet, installed Python distributions, and node_modules ' +
    'declarations per CLAUDE.md OWNER_ROUTING); when reflection is blocked or assay is unavailable, verify through the fallback tier instead ' +
    '— the nuget MCP for NuGet feed truth, Context7 for official API docs, exa/tavily for the package source/official surface — never from ' +
    'memory. Before driving assay, READ tools/assay/README.md for the api-arm contract (its resolve/decompile/reflection invocation, ' +
    'supported artifact kinds, and JSON output shape) so you drive it correctly rather than guessing flags.',
  'MANDATE — INTEGRATION-SHAPED, NOT SURFACE-LEVEL: a rebuilt .api documents (a) the package full ADVANCED surface (combinators, hooks, native ' +
    'pipelines, discriminators, async mirrors — not just the basic members), AND (b) the INTEGRATION patterns the dense design should compose — ' +
    'how this library STACKS with the other admitted libs into single rails (e.g. a decode hook feeding a discriminated model under a retry ' +
    'context with a telemetry span) — INCLUDING the SHARED/UNIVERSAL catalog tier (`libs/python/.api/` for Python; ' +
    '`libs/typescript/.api/` for TypeScript; `libs/csharp/.api/` for C# — the Thinktecture/LanguageExt substrate), so a folder/area catalog ' +
    'documents stacking ONTO those universal rails, not only its sibling-folder ' +
    'libs. The catalog GUIDES the rebuild toward first-class, stacked usage. Reject surface-level member lists.',
  'ADVERSARIAL LAW: every catalog is naive, shallow, or illusory until it survives attack — dense confident-looking catalogs are the prime ' +
    'suspect. Naivety is a defect on two orthogonal axes, both intolerable: COVERAGE — the catalog documents a thin slice of its package, the ' +
    'obvious members where the real surface carries far more; APPROACH — enumerated hardcoded instances where one parameterized pattern should ' +
    'own the space (a fixed roster of recipes, variants, or styles is seed DATA feeding one documented parameterized pattern, never the ' +
    'mechanism itself). Every defect list and capability-kind list in this prompt is a FLOOR, never the complete set — hunt past it: any ' +
    'repeated structure, parallel spelling, or enumerable family that one pattern, table, or parameterized rail can own is a collapse target ' +
    'you find yourself. ULTRA-STACKING: enumerate BOTH .api tiers in full from disk and mine each package to operator depth; an admitted ' +
    'capability the package carries but its catalog omits is a defect you close NOW; a cited member that cannot be verified is a phantom you ' +
    'delete NOW.',
  'FIX-IT-NOW LAW: a cross-catalog contradiction or gap your work exposes is YOURS in the same pass, wherever it lives — a hub omitting an ' +
    'anchor your stacking note composes against, a sibling catalog with divergent row grammar, a stale or contradicting claim in a catalog ' +
    'outside your batch: edit THAT catalog directly, either tier, under the CURRENT-STATE law. The batch is a work partition, never a write ' +
    'fence. Package admission is not this pass\'s surface: catalogs document the admitted set as it stands — a genuinely missing package is ' +
    'stated as fact in your summary, never admitted here.',
  'CURRENT-STATE LAW: sibling batches land catalog work concurrently with yours. Before editing any catalog outside your batch — a substrate ' +
    'hub or a sibling folder catalog — re-read its CURRENT on-disk state and compose landed sibling work as found; a conflict between your fix ' +
    'and a landed sibling resolves to the stronger form, never a revert, never a shrink of real content.',
  'WRITE-FULLY MANDATE: every correction you identify you MUST make NOW via Edit/Write directly in the .api file — the structured fix-log is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list or would/should hedge; leave nothing behind. `files` lists every catalog you edited; ' +
    '`beyondBatch` lists those outside your assigned batch — empty attests the cross-catalog hunt found nothing, never that it did not run. ' +
    'Verdict=clean is EARNED by an attack that finds nothing, never conceded on first read — and never invent edits to force a verdict.',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// The single scheduler for every agent-bearing task in the run: CAP tasks in flight, staggered launch.
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
      'find every rail it stacks onto already documented here. A defect this work exposes in a sibling substrate catalog outside your batch is ' +
      'yours per the FIX-IT-NOW + CURRENT-STATE laws.'
    : 'The shared substrate tier for this language has ALREADY been rebuilt in this run. Read the substrate catalogs your stacking notes ' +
      'compose against FROM DISK and verify every stacking claim against their REAL rebuilt content — a stacking claim written from memory is ' +
      'a phantom; a hub that, as rebuilt, truly omits an anchor you stack onto is extended by YOU now per the FIX-IT-NOW + CURRENT-STATE laws, ' +
      'never noted for someone else. Your batch is one folder: unify the row grammar across sibling catalogs in the batch (same family, same ' +
      'shape — provider rows, client/layer/config spellings, asymmetry columns) so siblings read as one family, never divergent one-offs — and ' +
      'a divergent sibling outside your batch is equally yours.',
  'For EACH file run the same 3-lens write: (1) EXTRACT-FULL — confirm the package and document its full useful ADVANCED surface ' +
    '(combinators/hooks/async mirrors/discriminators/native pipelines — a floor, not the set), not the basic subset; (2) REFINE/REFACTOR — ' +
    'restructure to integration-shaped, documenting how this lib STACKS with the universal-tier rails AND sibling admitted libs into single ' +
    'dense rails; (3) HARDEN — the terminal, most aggressive review: attack BOTH naivety axes (COVERAGE thin-slice, APPROACH ' +
    'enumerated-instances-where-one-parameterized-pattern-owns-the-space), then remove every phantom member, wrong floor/marker/target, ' +
    'surface-level framing, missing license/ABI/runtime flag, and un-stacked single-feature framing — a defect list you hunt past — and end ' +
    'with a full cold re-read of each finished catalog. Verify members via `uv run --frozen python -m tools.assay api resolve` (blocked: the ' +
    'nuget MCP / Context7 / exa-tavily source tier owns the fallback). Also close any ' +
    'gap a consuming design page genuinely needs (a specific member/signature the design composes). Return the fix-log: `files` = every ' +
    'catalog you edited, `beyondBatch` = those outside your assigned batch.',
].join('\n')
const processBatch = (tier) => async (w) => {
  const r = await agent(rebuildPrompt(w.files, tier), { label: 'api:' + w.files[0].split('/.api/')[0].split('/').pop() + '+' + (w.files.length - 1), phase: tier === 'substrate' ? 'API-Substrate' : 'API-Rebuild', schema: FIXLOG_SCHEMA, model: 'opus', effort: 'max', stallMs: STALL })
  return r ? { files: w.files, log: r } : null
}
const failedOf = (batches, res) => batches.filter((_, i) => !res[i]).flatMap((b) => b.files)

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('API-Discover')
const inv = await agent('Enumerate every .api catalog file under ' + SWEEP + ' from REAL disk state — one find listing over */.api/*.md (and any ' +
  'nested .api subdirs), never a memory-recall inventory: BOTH tiers the scope contains, the shared/universal tier (libs/<lang>/.api/) AND every ' +
  'folder tier (libs/<lang>/<folder>/.api/). EXCLUDE archive and scratch trees: any path segment _tmp, _archive, or node_modules. Return each as ' +
  'a repo-relative path — this listing is the ground truth downstream batches resolve against, an initial pointer never a ceiling. If none ' +
  'exist, return an empty list. Use find; do not cd.', { label: 'discover', phase: 'API-Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
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
// The one true barrier: folder tiers stack onto the substrate hubs, so the hubs land first.
let t0Res = []
if (t0Batches.length) {
  phase('API-Substrate')
  t0Res = await pool(t0Batches, CAP, processBatch('substrate'))
  log('Substrate wave: ' + t0Res.filter(Boolean).length + '/' + t0Batches.length + ' batches (' + T0.length + ' hub catalogs)')
}

// --- [API_REBUILD]
phase('API-Rebuild')
const t1Res = t1Batches.length ? await pool(t1Batches, CAP, processBatch('folder')) : []
const done = [...t0Res, ...t1Res].filter(Boolean)
const FAILED = [...failedOf(t0Batches, t0Res), ...failedOf(t1Batches, t1Res)]
const touched = [...new Set(done.flatMap((r) => r.log.files || []))]
const beyond = [...new Set(done.flatMap((r) => r.log.beyondBatch || []))]
log('API rebuild: ' + done.length + '/' + totalBatches + ' batches landed (' + totalFiles + ' catalogs); ' + touched.length + ' catalogs touched (' +
  beyond.length + ' via beyond-batch fixes)' + (FAILED.length ? ' — FAILED (reported, run continues): ' + FAILED.join(', ') : ''))
return { scope: SWEEP, catalogs: totalFiles, batches: totalBatches, complete: done.length, failed: FAILED, filesTouched: touched.length, beyondBatch: beyond }
