export const meta = {
  name: 'hygiene-sweep',
  whenToUse: 'Whole-tree freshness, seam, and consistency sweep over a target root before a cold-verify gate.',
  description: 'Whole-tree freshness/seams/consistency/blocker-resolution over a target root: README <-> central dependency manifest <-> per-dependency API/evidence catalogs, ARCHITECTURE seams line-by-line, done-task verify-remove, blocker protocol, then a cold-verify gate at zero high-severity. Language-agnostic (each agent resolves the actual manifest/toolchain owner from CLAUDE.md + the repo). Surgical, fix-in-place, no new design pages. args = optional scope (e.g. "libs/python"); empty = all of libs.',
  phases: [
    { title: 'Hygiene-Discover', detail: 'list the package/area folders under the target' },
    { title: 'H1-Folder', detail: 'per folder: README<->manifest<->API consistency, ARCHITECTURE seams, done-task verify-remove, blockers' },
    { title: 'H2-Global', detail: 'global manifest<->README consistency + branch/lib refinement + tooling correctness' },
    { title: 'H3-ColdVerify', detail: 'fresh per-folder verify; fix residual; gate zero high-severity' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const STAGGER_MS = 1500

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folders'], properties: { folders: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'applied', 'verdict', 'summary'], properties: { file: { type: 'string' }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean', 'fail'] }, residual_high: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. CLAUDE.md governs (WORKSPACE_LAW, DEPENDENCY_POLICY, OWNER_ROUTING). This is a SURGICAL hygiene pass: refine/correct, NEVER ' +
    'explanatory bloat, NEVER a new design page. Cards: default refine-do-not-proliferate (a genuinely-new card only if truly appropriate). VERIFY ' +
    'against disk before removing any done-claimed card. De-bloat verbose ARCHITECTURE comment lines; use correct glyphs; stay concise. Resolve ' +
    'the language toolchain owner from CLAUDE.md OWNER_ROUTING and invoke its real metadata/resolve command; when that command fails or is ' +
    'unavailable (it is under active construction), verify through the fallback tiers instead — BOTH .api catalog tiers, the nuget MCP for NuGet ' +
    'feed truth / Context7 for API docs, exa/tavily against the package source. Fix-in-place.',
  'MANIFEST OWNER: central package/version ownership lives in ONE owning manifest per language (per DEPENDENCY_POLICY) — identify the one ' +
    'governing this root (the Python pyproject, the C# Directory.Packages.props, or the TS workspace manifest) and treat it as the single source ' +
    'of truth. Each consumed dependency may carry an API/evidence catalog (a .api/<pkg> entry where the root maintains one); a folder README lists ' +
    'the dependencies it consumes. These three must agree.',
  'WRITE-FULLY MANDATE: every fix/correction you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, ledger, or would/should hedge; leave nothing behind except genuine cross-FILE items ' +
    '(residual_high). If a file is already correct, return verdict=clean — never invent edits. The cold-verify stage must itself FIX any residual ' +
    'it finds, not just report it.',
  'EVIDENCE GUARD (deletion + capability): a failed, erroring, or absent toolchain resolve is an OUTAGE signal, never absence evidence. Declare ' +
    'a cited member or catalog entry a PHANTOM, and delete it, ONLY when a second independent tier corroborates the absence (the .api catalogs, ' +
    'the nuget MCP feed / Context7 docs, exa/tavily against the package source); an uncorroborated failure KEEPS the entry and lands in ' +
    'residual_high. Capability is HARDENED, never dropped: zero current consumers never lowers the bar — an admitted capability nothing exploits ' +
    'is a named gap on the owning card surface, never a removal.',
  'ADVERSARIAL STANCE: presume every audited surface drifted, stale, or illusory until an aggressive attack finds nothing; a prior clean verdict ' +
    'is a rejected self-assessment, and verdict=clean is EARNED by an attack that comes back empty, never conceded on first read. Every ' +
    'enumerated checklist in this prompt is a FLOOR, never the complete set — hunt defects past it: any repeated structure, parallel spelling, ' +
    'duplicated or contradictory statement, or drifted family in scope is a target you find yourself. Naivety is a defect on two axes, both ' +
    'intolerable: COVERAGE — the audit or fix covers a thin slice of the concern where the domain carries the full set; APPROACH — enumerated ' +
    'one-off instance fixes where one parameterized, family-wide correction should own the space (an instance roster is seed DATA feeding that ' +
    'one systematic pass, never the mechanism). A token or single-point patch where a root-level correction of the same files is available is ' +
    'itself a defect — repair to the root form.',
].join('\n')
const BLOCKER = [
  'BLOCKER PROTOCOL: for each [BLOCKED] card, classify. GENUINE-EXTERNAL (a dependency build/wheel/target absent for the runtime, upstream issue ' +
    'open, sibling card unmet, live-proof needed) -> keep [BLOCKED], sharpen the rationale. RESEARCH/PROBING-RESOLVABLE -> PROBE NOW (the ' +
    'toolchain resolve/metadata command — or, when it is down, the .api catalogs / nuget MCP / Context7 / package index — plus the sibling ' +
    'page) and: if TRIVIAL, resolve fully (fix/correct tooling, get the info, ' +
    'write it where it belongs: manifest/API-catalog/README/seam/card-body); if a BIG task, flip [BLOCKED]->[QUEUED] with a surgical correction ' +
    'plus a reproduce-the-needed-info procedure baked into the card (do NOT implement big work inside hygiene).',
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

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Hygiene-Discover')
const inv = await agent('SCOPE DISCOVERY under ' + SWEEP + ', resolved against REAL disk state: run find/ls NOW and enumerate the package/area ' +
  'folders that own a design corpus — the immediate child directories containing a .planning directory and/or a README.md + ARCHITECTURE.md — ' +
  'plus the branch-level .planning tier if present. Never memory, never a prior run: confirm every candidate by its on-disk markers before ' +
  'listing it. Completeness is the contract — a missed real corpus folder starves every downstream stage; an invented one wastes a whole agent. ' +
  'Return each as a repo-relative path; do not cd. The list is scope ground truth, an initial pointer never a ceiling — downstream agents ' +
  'full-read their folders themselves; it never licenses a skim.', { label: 'discover', phase: 'Hygiene-Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const FOLDERS = ((inv && inv.folders) || []).filter(Boolean)
log('Hygiene discover under ' + SWEEP + ': ' + FOLDERS.length + ' folders')

// --- [H1_FOLDER]
phase('H1-Folder')
const h1 = (await pool(FOLDERS, CAP, (f) => agent([
  LAW, '', BLOCKER, '',
  'TASK (folder hygiene, edit only within ' + f + '): full-read the folder at large FIRST — README, ARCHITECTURE, cards, .api entries, every ' +
    '.planning page — full files, never skims. (1) CONSISTENCY — every dependency in ' + f + '/README.md exists in the central dependency ' +
    'manifest with the correct version/marker/floor; every dependency a card claims/wants is admitted or correctly deferred-carded; enumerate ' +
    'BOTH .api tiers IN FULL with a real ls/find (the language-root .api/ and the folder-local .api/ where present, never memory) and read ' +
    'entries at operator depth (members, not titles): every admitted dependency the folder consumes has its catalog entry (CREATE a missing one ' +
    'NOW); an entry for a removed dependency is a phantom — DELETE it NOW; a cited member that fails the toolchain resolve is a phantom ONLY ' +
    'when a fallback tier corroborates the absence per the EVIDENCE GUARD (delete corroborated NOW; keep uncorroborated, named in residual_high); ' +
    'an admitted capability no page or card exploits is a named gap — record it on the owning card surface. README fresh vs the current ' +
    'manifest. (2) SEAMS — audit ' + f + '/ARCHITECTURE.md LINE BY LINE: every seam truthful (matches a real page/owner/cross-package ' +
    'contract), no missing real cross-folder seam, no stale/wrong info, codemap == the on-disk page set, correct glyphs, concise comment lines ' +
    '(de-bloat). (3) DONE-TASK — find COMPLETE/implied-done cards; VERIFY the named deliverable exists on disk (the fence/page/manifest line ' +
    'itself, never the card claim); remove only verified-done; REVERT a false-complete card to its truthful open status and correct its body ' +
    'NOW. (4) BLOCKERS — apply the blocker protocol to every [BLOCKED] card in this folder. Fix-in-place. Return the fix-log of edits already ' +
    'made.',
].join('\n'), { label: 'H1:' + f.split('/').pop(), phase: 'H1-Folder', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)
log('H1 folder hygiene done across ' + h1.length + ' folders')

// --- [H2_GLOBAL]
phase('H2-Global')
const h2 = await agent([
  LAW, '', BLOCKER, '',
  'TASK (global, scope ' + SWEEP + ', edit the central manifest comments / READMEs / branch + lib .planning as needed): (1) GLOBAL MANIFEST ' +
    'CONSISTENCY — diff the central dependency manifest against ALL folder README dependency sections from real full reads, never memory (no ' +
    'dependency in a README absent from the manifest; no admitted-and-consumed dependency missing from a README; versions/markers/floors ' +
    'consistent); correct drift surgically NOW. (2) BRANCH/LIB REFINEMENT — refine/correct/de-stale the cross-cutting content in the branch and ' +
    'lib .planning tiers (README/ARCHITECTURE freshness, truthful cross-package seams, correct anchors) to the current state; no new design ' +
    'pages. (3) TOOLING — prove every tooling reference (toolchain invocation, command catalogs, owner routing) against the live owner by ' +
    'resolving/invoking it, and correct drift NOW. Fix-in-place. Return the fix-log of edits already made.',
].join('\n'), { label: 'H2:global', phase: 'H2-Global', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: 300000 })
log('H2 global consistency + branch/lib refinement done')

// --- [H3_COLDVERIFY]
phase('H3-ColdVerify')
const h3 = (await pool(FOLDERS, CAP, (f) => agent([
  LAW, '', BLOCKER, '',
  'TASK (COLD VERIFY of ' + f + ' — adversarial and WRITING, no prior verdicts exist for you): re-derive the folder hygiene state from fresh ' +
    'full-file reads and attack it — README <-> central manifest <-> BOTH .api tiers fully consistent (no missing/orphan catalog entry, no ' +
    'phantom cited member — EVIDENCE GUARD corroboration required before any deletion, no manifest drift); ARCHITECTURE seams all truthful and ' +
    'codemap == the on-disk page set; no false-complete card; ' +
    'every [BLOCKED] card correctly classified per the protocol (genuine vs resolvable); cards fresh and bidirectional. Where the sweep already ' +
    'edited, PROVE each edit on disk, re-derive its necessity and form, and REPAIR any loose, weak, or token fix to its root form NOW. FIX any ' +
    'residual inconsistency in place whatever its severity (you own this folder). Gate: verdict=fixed when you repaired anything; verdict=clean ' +
    'ONLY when your attack found nothing; verdict=fail only for a defect genuinely unreachable from this folder, named in residual_high. ' +
    'Return the fix-log of edits already made.',
].join('\n'), { label: 'H3:' + f.split('/').pop(), phase: 'H3-ColdVerify', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: 300000 }))).filter(Boolean)
const failing = h3.filter((v) => v && v.verdict === 'fail')
log('H3 cold verify: ' + (h3.length - failing.length) + '/' + h3.length + ' clean; ' + failing.length + ' failing')

const crossResiduals = h1.flatMap((r) => r.residual_high || [])
let crossVerify = null
if (crossResiduals.length) {
  phase('Reconcile')
  await agent([LAW, '', BLOCKER, '', 'TASK: CROSS-FOLDER RECONCILE. The per-folder pass flagged these residuals that SPAN folders (a seam two ' +
    'folders must agree on, a cross-package contract, a manifest/README mismatch across packages) — per-folder H3 cannot own them. There is NO ' +
    'severity — address EVERY residual. Read every implicated file across folders and FIX each in place, consistently, regressing nothing. ' +
    'Residuals:\n' + JSON.stringify(crossResiduals, null, 1)].join('\n'), { label: 'cross-fix', phase: 'Reconcile', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: 300000 })
  crossVerify = await agent([LAW, '', BLOCKER, '', 'TASK: ADVERSARIAL WRITING VERIFY of the cross-folder residuals below — for each, re-derive ' +
    'from the spanned files whether the residual was real and what its ROOT resolution is, then PROVE on disk the fix landed properly and ' +
    'consistently in EVERY spanned file; REPAIR any loose, weak, or token fix to its root form NOW, and FIX any still-open residual in place. ' +
    'Verdict: fixed when you repaired; clean ONLY when the attack finds every residual truly resolved; fail on any doubt you cannot repair ' +
    'from the spanned files. ' +
    'Residuals:\n' + JSON.stringify(crossResiduals, null, 1)].join('\n'), { label: 'cross-verify', phase: 'Reconcile', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: 300000 })
}

return {
  scope: SWEEP,
  h1: h1.map((r) => ({ file: r.file, verdict: r.verdict, applied: (r.applied || []).length })),
  h2: { verdict: h2 && h2.verdict, applied: h2 && h2.applied ? h2.applied.length : 0 },
  gate: { clean: h3.length - failing.length, total: h3.length, failing: failing.map((v) => v.file) },
  cross_reconcile: { residuals: crossResiduals.length, verify: crossVerify && crossVerify.verdict },
}
