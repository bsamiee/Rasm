export const meta = {
  name: 'rebuild-api',
  whenToUse: 'Rebuild every .api catalog under a target root to full integration-shaped capability.',
  description: 'Rebuild every .api catalog under a target root to FULL first-class, integration-shaped capability — document each package full advanced surface AND how packages STACK into single dense rails, verified against real members. Substrate-first PER LANGUAGE: each language runs as an independent concurrent lane in which the shared tier (libs/<lang>/.api/) is rebuilt before that language folder tiers — the barrier is language-local, so a python folder catalog never waits on csharp substrate; a failed substrate batch flags that language folder batches in the log and return instead of silently stacking onto stub hubs. Folder batches keep one folder per batch, pack small sibling-folder tails of the same language up to the batch size, and co-batch sibling families as the WORK PARTITION, never a write fence: every batch fixes any catalog its work exposes — either tier, in or out of its batch — in the same pass under the current-state law, so the run ends closed in one pass. Every catalog rebuild batch (substrate and folder tier alike) runs on gpt-5.5 dispatched through a sonnet codex wrapper in a workspace-write sandbox — batches are path-disjoint by construction (CODEX flag; false restores native opus batch agents); the discover stage stays sonnet. Language-agnostic: members verified via assay api over host DLLs / NuGet / Python distributions / node_modules, falling back to the nuget MCP / Context7 / source tier when reflection is blocked. args = optional scope (string, array of scopes, or {target|targets} — e.g. "libs/python" or "libs/csharp/Rasm.Bim"); empty = all of libs.',
  phases: [
    { title: 'API-Discover', detail: 'list every .api catalog under the target from disk; _tmp/archives excluded' },
    { title: 'API-Substrate', detail: 'per-language lanes on gpt-5.5 (codex wrappers, workspace-write): each language shared tier (libs/<lang>/.api/) rebuilt first inside its own lane — the hub rails that language folder tier stacks onto; a failed hub batch flags the lane' },
    { title: 'API-Rebuild', detail: 'folder-tier batches per language lane on gpt-5.5 (codex wrappers, workspace-write): one folder per batch, small sibling-folder tails packed up to the batch size; all lanes concurrent under CAP=14; every cross-catalog defect fixed in-pass' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 14
const BATCH = 4 // .api files per agent — deep enough per file, many agents for parallelism
const STAGGER_MS = 1500
const STALL = 300000
const CODEX = true // catalog rebuild batch lanes run on gpt-5.5 via the codex wrapper (workspace-write); false restores native opus lanes
const CODEX_DIR = '.claude/scratch/rebuild-api' // wrapper task/schema/report files, one triple per lane

// --- [INPUTS] ----------------------------------------------------------------------------

// args is structured data — a scope string, an array of scopes, or {target|targets}; empty = the full libs sweep.
const scopeRows = Array.isArray(args) ? args : typeof args === 'string' ? [args] : args && typeof args === 'object' ? [].concat(args.targets ?? args.target ?? []) : []
const scopes = scopeRows.map((s) => String(s).trim()).filter((s) => s && s !== 'ALL')
const SWEEP = scopes.length ? scopes.join(', ') : 'libs'

// --- [MODELS] ----------------------------------------------------------------------------

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'string' } } } }
// On-disk product schema: each batch writes its complete fix-log here; required-but-empty `beyondBatch` attests the cross-catalog hunt ran and every exposed defect landed in-pass.
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'beyondBatch', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, beyondBatch: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, summary: { type: 'string' } } }

// Thin wire receipt: the batch PRODUCT is the edited catalogs on disk plus the fix-log at `report`; only status + count + headline travel inline.
const RECEIPT = { type: 'object', additionalProperties: false, required: ['ok', 'report', 'entries', 'headline', 'failure'], properties: {
  ok: { type: 'boolean' }, report: { type: 'string' }, entries: { type: 'integer' },
  headline: { type: 'string' }, failure: { type: 'string' } } }

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
// The single run-wide scheduler: CAP agents in flight across every language lane, launches staggered; a freed slot passes to the next waiter.
let active = 0
let gate = Promise.resolve()
const waiters = []
const acquire = () => (active < CAP ? (active++, Promise.resolve()) : new Promise((res) => waiters.push(res)))
const release = () => { const w = waiters.shift(); if (w) w(); else active-- }
const stagger = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
const scheduled = async (fn) => { await acquire(); await stagger(); try { return await fn() } finally { release() } }
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }

// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// CODEX_DIR, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns a thin RECEIPT — the product (edited catalogs + fix-log)
// stays on disk. It never does, edits, judges, or relays the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  const rptPat = '[' + rpt.slice(0, 1) + ']' + rpt.slice(1) // self-excluding pgrep/pkill pattern
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and return a thin ' +
    'RECEIPT for its on-disk report. Never perform, edit, judge, soften, summarize, or RELAY the work itself.',
  '(1) Files FIRST, with the WRITE TOOL — never a shell heredoc and never a relative path (cwd drift and heredoc quoting land files where codex cannot find them, killing every launch on a missing schema file). From the repository root (your starting cwd): mkdir -p ' + CODEX_DIR + '; purge stale lane artifacts (a leftover report would READY instantly with last run\'s data): rm -f ' + base + '-report.json ' + base + '-stderr.log; Write the TASK block below verbatim to ' + base + '-task.md; Write this JSON ' +
    'Schema exactly to ' + base + '-schema.json — both paths resolved ABSOLUTE under the repository root: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call from the repo root, which FIRST verifies the files: test -s ' + base + '-task.md && test -s ' + base + '-schema.json || echo FILES-MISSING — on FILES-MISSING redo (1), NEVER launch without both. THEN the command below VERBATIM, never retyped or reflowed (every token matters: dropping </dev/null makes codex block forever on stdin, zero-CPU, no report): ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral -c mcp_servers={} ' +
    '--output-schema ' + base + '-schema.json -o ' + base + '-report.json "Do the task in ' + base + '-task.md ' +
    'from the repository root. Final message: JSON per the output schema." </dev/null >/dev/null 2>' + base + '-stderr.log &',
  '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
    'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
    'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' + base + '-report.json ] && break; ' +
    'pgrep -f "' + rptPat + '" >/dev/null || break; sleep 20; done; if [ -s ' + base + '-report.json ]; then echo ' +
    'READY; elif pgrep -f "' + rptPat + '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
    'while it prints RUNNING; stop on READY; on GONE go to (4). LIVENESS IS NOT HEALTH: after the 4th RUNNING poll (~20 min wall) the run is WEDGED, not slow — kill it (pkill -f "' + rptPat + '") and go to (4) as GONE. Cap at 7 poll calls total.',
  '(4) READY: do NOT relay the report body through your output — build the MECHANICAL headline with jq (never your own ' +
    'judgment): entries=$(jq \'.files | length\' ' + base + '-report.json); beyond=$(jq \'.beyondBatch | length\' ' + base + '-report.json); verdict=$(jq -r \'.verdict\' ' + base + '-report.json). ' +
    'Return the RECEIPT: ok=true, report=' + base + '-report.json, entries=that count, headline="<entries> catalogs | verdict:<verdict> | +<beyond> beyond", failure empty. ' +
    'GONE with no report: tail -5 ' + base + '-stderr.log FIRST — that tail IS the crash reason; relaunch the (2) command once (detached, never ' +
    'foreground) and resume polling; a second GONE returns ok=false, entries=0, report and headline empty, failure=the stderr tail in one line.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}

// Every catalog rebuild batch routes here: gpt-5.5 wrapper when CODEX, native opus otherwise. The roster row carries
// `scope` from the ORCHESTRATOR (the batch's assigned files) so a failed lane's territory is exact even when it died.
const recon = (task, o) => (CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: RECEIPT, stallMs: STALL })
  : agent(task + '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
    CODEX_DIR + '/' + fileTag(o.label) + '-report.json (Write tool, absolute path under the repo root): ' +
    JSON.stringify(o.schema) + ' — then return ONLY the receipt: ok, report path, entries count, one-line mechanical headline, failure empty.',
    { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: RECEIPT, stallMs: STALL })
).then((r) => ({ lane: o.label, scope: o.scope || [], ok: !!(r && r.ok && r.report), report: (r && r.report) || '',
  entries: (r && r.entries) || 0, headline: (r && r.headline) || '', failure: (r && r.failure) || (r ? '' : 'lane died') }))
const rel = (f) => { const i = String(f).indexOf('libs/'); return i > 0 ? String(f).slice(i) : String(f) }
const isSubstrate = (f) => /^libs\/[^/]+\/\.api\//.test(f)
const folderOf = (f) => f.slice(0, f.indexOf('/.api/'))
const langOf = (f) => f.split('/')[1] || ''

// Full folder chunks stay whole; sub-BATCH tails of sibling folders in one language pack together — never a 1-catalog agent per tiny folder.
const packFolders = (byFolder) => {
  const batches = []
  let tail = []
  for (const k of [...byFolder.keys()].sort()) {
    for (const c of chunk(byFolder.get(k).slice().sort(), BATCH)) {
      if (c.length === BATCH) batches.push({ files: c })
      else if (tail.length + c.length > BATCH) { batches.push({ files: tail }); tail = c }
      else tail = tail.concat(c)
    }
  }
  if (tail.length) batches.push({ files: tail })
  return batches
}

const rebuildPrompt = (files, tier, degraded) => [
  LAW, '',
  'TASK: REBUILD these .api catalogs to FULL first-class, integration-shaped capability (fix-in-place, read-then-extend; never shrink real ' +
    'content): ' + files.join(', '),
  tier === 'substrate'
    ? 'These are SHARED-TIER catalogs — the universal rails every folder tier in this language stacks onto. Beyond the package own advanced ' +
      'surface, document the ANCHOR members downstream catalogs compose against (the service tags, carriers, schema/codec entrypoints, ' +
      'layer/runtime constructors, and cross-package seams that make this the hub) at operator depth — a folder catalog written after you must ' +
      'find every rail it stacks onto already documented here. A defect this work exposes in a sibling substrate catalog outside your batch is ' +
      'yours per the FIX-IT-NOW + CURRENT-STATE laws.'
    : 'The shared substrate tier for this language was rebuilt EARLIER in this run' +
      (degraded && degraded.length
        ? ' — INCOMPLETELY: the hub batch(es) covering ' + degraded.join(', ') + ' FAILED, so those hubs may still be pre-rebuild. Trust NO ' +
          'hub as rebuilt: verify every anchor you stack onto against CURRENT disk content, and where a hub omits an anchor your stacking ' +
          'note composes against, extend that hub YOURSELF per the FIX-IT-NOW + CURRENT-STATE laws.'
        : '.') +
      ' Read the substrate catalogs your stacking notes ' +
      'compose against FROM DISK and verify every stacking claim against their REAL content — a stacking claim written from memory is ' +
      'a phantom; a hub that truly omits an anchor you stack onto is extended by YOU now per the FIX-IT-NOW + CURRENT-STATE laws, ' +
      'never noted for someone else. Your batch is one folder, or a PACKED set of small sibling folders in one language: unify the row ' +
      'grammar across sibling catalogs in the batch (same family, same ' +
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

const processBatch = (tier, degraded) => async (w) => recon(rebuildPrompt(w.files, tier, degraded),
  { label: 'api:' + w.files[0].split('/.api/')[0].split('/').pop() + ':b' + w.idx + '+' + (w.files.length - 1),
    phase: tier === 'substrate' ? 'API-Substrate' : 'API-Rebuild', schema: FIXLOG_SCHEMA, scope: w.files, writes: true })
const failedOf = (batches, res) => batches.filter((_, i) => !res[i] || !res[i].ok).flatMap((b) => b.files)
// One language lane: its substrate hubs land before its folder tier; a failed hub batch FLAGS the folder batches instead of failing silently.
const runLane = async (l) => {
  const subRes = l.sub.length ? await Promise.all(l.sub.map((w, bi) => scheduled(() => processBatch('substrate', [])({ ...w, idx: bi })).catch(() => null))) : []
  const subFailed = failedOf(l.sub, subRes)
  if (subFailed.length) log('Substrate DEGRADED [' + l.lang + ']: ' + subFailed.join(', ') + ' — ' + l.fold.length +
    ' folder batch(es) proceed FLAGGED: hubs verified from disk, never assumed rebuilt')
  else if (l.sub.length) log('Substrate [' + l.lang + ']: ' + subRes.filter((r) => r && r.ok).length + '/' + l.sub.length + ' hub batches landed')
  const foldRes = l.fold.length ? await Promise.all(l.fold.map((w, bi) => scheduled(() => processBatch('folder', subFailed)({ ...w, idx: bi })).catch(() => null))) : []
  return { lang: l.lang, fold: l.fold, subRes, foldRes, subFailed, foldFailed: failedOf(l.fold, foldRes) }
}

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
const LANGS = [...new Set([...T0, ...T1].map(langOf))].sort()
const lanes = LANGS.map((lang) => {
  const byFolder = new Map()
  for (const f of T1.filter((x) => langOf(x) === lang)) { const k = folderOf(f); if (!byFolder.has(k)) byFolder.set(k, []); byFolder.get(k).push(f) }
  return { lang, sub: chunk(T0.filter((f) => langOf(f) === lang), BATCH).map((files) => ({ files })), fold: packFolders(byFolder), folders: byFolder.size }
})
const totalFiles = T0.length + T1.length
const totalBatches = lanes.reduce((n, l) => n + l.sub.length + l.fold.length, 0)
log('API discover under ' + SWEEP + ': ' + totalFiles + ' catalogs (' + T0.length + ' substrate + ' + T1.length + ' folder-tier across ' +
  lanes.reduce((n, l) => n + l.folders, 0) + ' folders) in ' + totalBatches + ' batches across ' + LANGS.length + ' language lane(s); CAP=' + CAP)

phase('API-Substrate')

phase('API-Rebuild')

// Both groups open before launch: lanes interleave the two tiers across languages; each agent lands in the group its phase option names.
const laneOut = (await Promise.all(lanes.map(runLane))).filter(Boolean)
const done = laneOut.flatMap((l) => [...l.subRes, ...l.foldRes]).filter((r) => r && r.ok)
const FAILED = laneOut.flatMap((l) => [...l.subFailed, ...l.foldFailed])
const DEGRADED = laneOut.filter((l) => l.subFailed.length).map((l) => ({ lang: l.lang, substrateFailed: l.subFailed, flaggedFolderBatches: l.fold.length }))
const edits = done.reduce((n, r) => n + r.entries, 0) // catalog edits reported across batches; each batch's full fix-log stays on disk at its receipt's `report`
log('API rebuild: ' + done.length + '/' + totalBatches + ' batches landed (' + totalFiles + ' catalogs); ' + edits + ' catalog edits reported' +
  (DEGRADED.length ? ' — DEGRADED lane(s), folder batches ran against unverified hubs: ' + DEGRADED.map((d) => d.lang).join(', ') : '') +
  (FAILED.length ? ' — FAILED (reported, run continues): ' + FAILED.join(', ') : ''))

return { scope: SWEEP, catalogs: totalFiles, batches: totalBatches, complete: done.length, failed: FAILED, degraded: DEGRADED, edits, reports: done.map((r) => r.report) }
