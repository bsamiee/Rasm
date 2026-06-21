export const meta = {
  name: 'hygiene-sweep',
  description: 'Whole-tree freshness/seams/consistency/blocker-resolution over a target root: README <-> central dependency manifest <-> per-dependency API/evidence catalogs, ARCHITECTURE seams line-by-line, done-task verify-remove, blocker protocol, then a cold-verify gate at zero high-severity. Language-agnostic (each agent resolves the actual manifest/toolchain owner from CLAUDE.md + the repo). Surgical, fix-in-place, no new design pages. args = optional scope (e.g. "libs/python"); empty = all of libs.',
  phases: [
    { title: 'Hygiene-Discover', detail: 'list the package/area folders under the target' },
    { title: 'H1-Folder', detail: 'per folder: README<->manifest<->API consistency, ARCHITECTURE seams, done-task verify-remove, blockers' },
    { title: 'H2-Global', detail: 'global manifest<->README consistency + branch/lib refinement + tooling correctness' },
    { title: 'H3-ColdVerify', detail: 'fresh per-folder verify; fix residual; gate zero high-severity' },
  ],
}

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folders'], properties: { folders: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'applied', 'verdict', 'summary'], properties: { file: { type: 'string' }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean', 'fail'] }, residual_high: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [HARNESS] -- bounded worker pool: steady <=cap concurrent, no burst ----------------
const STAGGER_MS = 1500
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  const run = async (slot) => {
    if (slot) await new Promise((res) => setTimeout(res, slot * STAGGER_MS))
    while (next < items.length) { const i = next++; out[i] = await worker(items[i], i) }
  }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, (_, slot) => run(slot)))
  return out
}
const CAP = 10

// --- [INPUT] -- args = optional scope; empty/"ALL" = all of libs ---
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

const LAW = [
  'Rasm monorepo. CLAUDE.md governs (WORKSPACE_LAW, DEPENDENCY_POLICY, OWNER_ROUTING). This is a SURGICAL hygiene pass: refine/correct, NEVER explanatory bloat, NEVER a new design page. Cards: default refine-do-not-proliferate (a genuinely-new card only if truly appropriate). VERIFY against disk before removing any done-claimed card. De-bloat verbose ARCHITECTURE comment lines; use correct glyphs; stay concise. Resolve the language toolchain owner from CLAUDE.md OWNER_ROUTING and invoke its real metadata/resolve command. Fix-in-place. C# PLANNING-HOMING: under `libs/csharp/Rasm` the active planning effort is `Rasm/Geometry` — its design pages are at `libs/csharp/Rasm/Geometry/.planning/**` while the README/`.api/`/`ARCHITECTURE.md`/cards sit at the `Rasm/` package ROOT; treat the Rasm root plus its nested `Geometry/.planning` as ONE corpus for README<->manifest<->.api<->seam consistency. Mature siblings `Analysis`/`Domain`/`Vectors` are not planning targets.',
  'MANIFEST OWNER: central package/version ownership lives in ONE owning manifest per language (per DEPENDENCY_POLICY) — identify the one governing this root (the Python pyproject, the C# Directory.Packages.props, or the TS workspace manifest) and treat it as the single source of truth. Each consumed dependency may carry an API/evidence catalog (a .api/<pkg> entry where the root maintains one); a folder README lists the dependencies it consumes. These three must agree.',
  'WRITE-FULLY MANDATE: every fix/correction you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log is a REPORT of edits ALREADY MADE, never a to-do list, ledger, or would/should hedge; leave nothing behind except genuine cross-FILE items (residual_high). If a file is already correct, return verdict=clean — never invent edits. The cold-verify stage must itself FIX any residual high it finds, not just report it.',
].join('\n')
const BLOCKER = [
  'BLOCKER PROTOCOL: for each [BLOCKED] card, classify. GENUINE-EXTERNAL (a dependency build/wheel/target absent for the runtime, upstream issue open, sibling card unmet, live-proof needed) -> keep [BLOCKED], sharpen the rationale. RESEARCH/PROBING-RESOLVABLE -> PROBE NOW (the toolchain resolve/metadata command, the package index, the sibling page) and: if TRIVIAL, resolve fully (fix/correct tooling, get the info, write it where it belongs: manifest/API-catalog/README/seam/card-body); if a BIG task, flip [BLOCKED]->[QUEUED] with a surgical correction plus a reproduce-the-needed-info procedure baked into the card (do NOT implement big work inside hygiene).',
].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Hygiene-Discover')
const inv = await agent('List the package/area folders under ' + SWEEP + ' — the immediate child directories that own a design corpus (those containing a .planning directory and/or a README.md + ARCHITECTURE.md). Include the branch-level .planning tier if present. Return each as a repo-relative path. Use find/ls; do not cd.', { label: 'discover', phase: 'Hygiene-Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const FOLDERS = ((inv && inv.folders) || []).filter(Boolean)
log('Hygiene discover under ' + SWEEP + ': ' + FOLDERS.length + ' folders')

phase('H1-Folder')
const h1 = (await pool(FOLDERS, CAP, (f) => agent([
  LAW, '', BLOCKER, '',
  'TASK (folder hygiene, edit only within ' + f + '): (1) CONSISTENCY — every dependency in ' + f + '/README.md exists in the central dependency manifest with the correct version/marker/floor; every dependency a card claims/wants is admitted or correctly deferred-carded; every admitted dependency the folder consumes has its API/evidence catalog entry (flag+create a missing one; flag a stale entry for a removed dependency); README fresh vs the current manifest. (2) SEAMS — read all of ' + f + '/.planning/ + cards, then audit ' + f + '/ARCHITECTURE.md LINE BY LINE: every seam truthful (matches a real page/owner/cross-package contract), no missing real cross-folder seam, no stale/wrong info, codemap == the on-disk page set, correct glyphs, concise comment lines (de-bloat). (3) DONE-TASK — find COMPLETE/implied-done cards; VERIFY the deliverable exists on disk; mark/remove only verified-done; flag false-complete. (4) BLOCKERS — apply the blocker protocol to every [BLOCKED] card in this folder. Fix-in-place. Return the fix-log.',
].join('\n'), { label: 'H1:' + f.split('/').pop(), phase: 'H1-Folder', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)
log('H1 folder hygiene done across ' + h1.length + ' folders')

phase('H2-Global')
const h2 = await agent([
  LAW, '', BLOCKER, '',
  'TASK (global, scope ' + SWEEP + ', edit the central manifest comments / READMEs / branch + lib .planning as needed): (1) GLOBAL MANIFEST CONSISTENCY — confirm the central dependency manifest and ALL folder README dependency sections agree (no dependency in a README absent from the manifest; no admitted-and-consumed dependency missing from a README; versions/markers/floors consistent); correct drift surgically. (2) BRANCH/LIB REFINEMENT — refine/correct/de-stale the cross-cutting content in the branch and lib .planning tiers (README/ARCHITECTURE freshness, truthful cross-package seams, correct anchors) to the current state; no new design pages. (3) TOOLING — verify any tooling reference (toolchain invocation, command catalogs, owner routing) is correct and current. Fix-in-place. Return the fix-log.',
].join('\n'), { label: 'H2:global', phase: 'H2-Global', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: 300000 })
log('H2 global consistency + branch/lib refinement done')

phase('H3-ColdVerify')
const h3 = (await pool(FOLDERS, CAP, (f) => agent([
  LAW, '',
  'TASK (COLD VERIFY of ' + f + ', no prior verdicts): independently confirm — README <-> central manifest <-> per-dependency API/evidence catalogs fully consistent (no missing/stale catalog entry, no manifest drift); ARCHITECTURE seams all truthful and codemap == on-disk page set; no false-complete card; every [BLOCKED] card correctly classified (genuine vs resolved/flipped); cards fresh and bidirectional. If ANY residual high-severity inconsistency remains, FIX IT IN PLACE now (you own this folder). Gate: verdict=clean only if zero residual high; otherwise verdict=fail. Return the fix-log.',
].join('\n'), { label: 'H3:' + f.split('/').pop(), phase: 'H3-ColdVerify', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: 300000 }))).filter(Boolean)
const failing = h3.filter((v) => v && v.verdict === 'fail')
log('H3 cold verify: ' + (h3.length - failing.length) + '/' + h3.length + ' clean; ' + failing.length + ' failing')

// --- [RECONCILE] -- cross-folder residuals H1 flagged that per-folder H3 cannot own ---------
const crossResiduals = h1.flatMap((r) => r.residual_high || [])
let crossVerify = null
if (crossResiduals.length) {
  phase('Reconcile')
  await agent([LAW, '', BLOCKER, '', 'TASK: CROSS-FOLDER RECONCILE. The per-folder pass flagged these residuals that SPAN folders (a seam two folders must agree on, a cross-package contract, a manifest/README mismatch across packages) — per-folder H3 cannot own them. There is NO severity — address EVERY residual. Read every implicated file across folders and FIX each in place, consistently, regressing nothing. Residuals:\n' + JSON.stringify(crossResiduals, null, 1)].join('\n'), { label: 'cross-fix', phase: 'Reconcile', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: 300000 })
  crossVerify = await agent([LAW, '', 'TASK: ADVERSARIAL VERIFY the cross-folder residuals below are ACTUALLY resolved — read the implicated files from disk, default to fail on any doubt, and FIX any still-open in place. Residuals:\n' + JSON.stringify(crossResiduals, null, 1)].join('\n'), { label: 'cross-verify', phase: 'Reconcile', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: 300000 })
}

return {
  scope: SWEEP,
  h1: h1.map((r) => ({ file: r.file, verdict: r.verdict, applied: (r.applied || []).length })),
  h2: { verdict: h2 && h2.verdict, applied: h2 && h2.applied ? h2.applied.length : 0 },
  gate: { clean: h3.length - failing.length, total: h3.length, failing: failing.map((v) => v.file) },
  cross_reconcile: { residuals: crossResiduals.length, verify: crossVerify && crossVerify.verdict },
}
