export const meta = {
  name: 'hygiene-sweep',
  whenToUse: 'Whole-tree freshness, seam, and consistency sweep over a target root before a cold-verify gate.',
  description: 'Whole-tree freshness/seams/consistency/blocker-resolution over a target root: README <-> central dependency manifest <-> per-dependency API/evidence catalogs, ARCHITECTURE seams line-by-line, done-task verify-remove, blocker protocol, then a per-folder cold-verify gate that FIXES what it finds — clean verdicts earned by an attack that comes back empty. Language-agnostic (each agent resolves the actual manifest/toolchain owner from CLAUDE.md + the repo). Surgical, fix-in-place, no new design pages. Self-enclosing: every agent carries full ripple authority — a defect its work exposes anywhere in scope is fixed in the same pass, both ends, under the current-state law; the central manifest is the single permitted handoff (folder agents report exact rows, the one global writer applies them once, with one bounded re-attempt). A fan agent that dies is surfaced as a skipped folder in the return, never silently dropped. One flat pool (CAP=14) schedules every fan; every worker runs on fable. args = optional scope (e.g. "libs/python"); empty = all of libs.',
  phases: [
    { title: 'Hygiene-Discover', detail: 'list the package/area folders under the target', model: 'sonnet' },
    { title: 'H1-Folder', detail: 'per folder, all concurrent: README<->manifest<->API consistency, ARCHITECTURE seams, done-task verify-remove, blockers; cross-folder ripples fixed in-pass; manifest rows reported', model: 'fable' },
    { title: 'H2-Global', detail: 'the single central-manifest writer, one bounded re-attempt: applies reported rows, global manifest<->README consistency, branch/lib refinement, tooling correctness', model: 'fable' },
    { title: 'H3-ColdVerify', detail: 'per folder, all concurrent: fresh adversarial re-derive that fixes every residual in place; gate = zero unrepaired defects', model: 'fable' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 14
const STAGGER_MS = 1500
const STALL = 300000

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folders'], properties: { folders: { type: 'array', items: { type: 'string' } } } }
// Required-but-possibly-empty `manifest_rows` is an attestation: the manifest single-writer law held — needed rows are reported, never hand-edited in a fan.
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'applied', 'verdict', 'manifest_rows', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean', 'fail'] }, manifest_rows: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['doc', 'row'], properties: { doc: { type: 'string' }, row: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. This is a SURGICAL hygiene pass: refine/correct, NEVER explanatory bloat, NEVER a new design page. Cards: default ' +
    'refine-do-not-proliferate (a genuinely-new card only if truly appropriate). VERIFY against disk before removing any done-claimed card. ' +
    'De-bloat verbose ARCHITECTURE comment lines; use correct glyphs; stay concise. Resolve the language toolchain owner per OWNER_ROUTING and ' +
    'invoke its real metadata/resolve command; when that command fails or is unavailable (it is under active construction), verify through the ' +
    'fallback tiers instead — BOTH .api catalog tiers, the nuget MCP for NuGet feed truth / Context7 for API docs, exa/tavily against the ' +
    'package source. Fix-in-place.',
  'MANIFEST OWNER: central package/version ownership lives in ONE owning manifest per language — identify the one governing this root per ' +
    'DEPENDENCY_POLICY and treat it as the single source of truth. Each consumed dependency may carry an API/evidence catalog (a .api/<pkg> ' +
    'entry where the root maintains one); a folder README lists ' +
    'the dependencies it consumes. These three must agree. The central manifest has exactly ONE in-run writer — the global stage: a fan agent ' +
    'that proves a manifest defect reports the exact correction as a {doc, row} in `manifest_rows` and never edits the manifest itself.',
  'WRITE-FULLY + RIPPLE AUTHORITY: every fix/correction you identify you MUST make NOW via Edit/Write directly in the file — the structured ' +
    'fix-log is a REPORT of edits ALREADY MADE, never a to-do list, ledger, or would/should hedge; the run hands nothing to a later phase, ' +
    'session, or workflow. A defect your work exposes OUTSIDE your primary folder — a seam two folders must agree on, a cross-package contract, ' +
    'a counterpart README or card the fix implicates — is YOURS in the same pass: fix it at both ends, consistently, regressing nothing; the ' +
    'central manifest alone routes through `manifest_rows`. If a file is already correct, return verdict=clean — never invent edits.',
  'CURRENT STATE: sibling agents sweep other folders concurrently and may land edits on the same shared surfaces — seam counterparts, branch ' +
    'docs, cross-package contracts. Before editing any file beyond your primary folder, re-read its CURRENT on-disk state; an edit a sibling ' +
    'landed is composed as found, never assumed or re-derived; a conflict between your correction and a landed sibling resolves to the stronger ' +
    'form, never a revert.',
  'EVIDENCE GUARD (deletion + capability): a failed, erroring, or absent toolchain resolve is an OUTAGE signal, never absence evidence. Declare ' +
    'a cited member or catalog entry a PHANTOM, and delete it, ONLY when a second independent tier corroborates the absence (the .api catalogs, ' +
    'the nuget MCP feed / Context7 docs, exa/tavily against the package source); an uncorroborated failure KEEPS the entry — keeping it IS the ' +
    'in-run resolution under outage, named in the fix-log summary. Capability is HARDENED, never dropped: zero current consumers never lowers ' +
    'the bar — an admitted capability nothing exploits is a named gap on the owning card surface, never a removal.',
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
    'write it where it belongs: manifest-row report/API-catalog/README/seam/card-body); if a BIG task, flip [BLOCKED]->[QUEUED] with a surgical ' +
    'correction plus a reproduce-the-needed-info procedure baked into the card (do NOT implement big work inside hygiene).',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// The single scheduler for every fan in the run: CAP tasks in flight, staggered launch.
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
const h1Raw = await pool(FOLDERS, CAP, (f) => agent([
  LAW, '', BLOCKER, '',
  'TASK (folder hygiene, primary folder ' + f + '): full-read the folder at large FIRST — README, ARCHITECTURE, cards, .api entries, every ' +
    '.planning page — full files, never skims. (1) CONSISTENCY — every dependency in ' + f + '/README.md exists in the central dependency ' +
    'manifest with the correct version/marker/floor (a manifest defect is an exact `manifest_rows` report, per MANIFEST OWNER); every dependency ' +
    'a card claims/wants is admitted or correctly deferred-carded; enumerate BOTH .api tiers IN FULL with a real ls/find (the language-root ' +
    '.api/ and the folder-local .api/ where present, never memory) and read entries at operator depth (members, not titles): every admitted ' +
    'dependency the folder consumes has its catalog entry (CREATE a missing one NOW); an entry for a removed dependency is a phantom — DELETE it ' +
    'NOW; a cited member that fails the toolchain resolve is a phantom ONLY when a fallback tier corroborates the absence per the EVIDENCE GUARD ' +
    '(delete corroborated NOW; keep uncorroborated, named in the summary); an admitted capability no page or card exploits is a named gap — ' +
    'record it on the owning card surface. README fresh vs the current manifest. (2) SEAMS — audit ' + f + '/ARCHITECTURE.md LINE BY LINE: every ' +
    'seam truthful (matches a real page/owner/cross-package contract), no missing real cross-folder seam, no stale/wrong info, codemap == the ' +
    'on-disk page set, correct glyphs, concise comment lines (de-bloat); a seam defect is repaired at BOTH ends per RIPPLE AUTHORITY under ' +
    'CURRENT STATE. (3) DONE-TASK — find COMPLETE/implied-done cards; VERIFY the named deliverable exists on disk (the fence/page/manifest line ' +
    'itself, never the card claim); remove only verified-done; REVERT a false-complete card to its truthful open status and correct its body ' +
    'NOW. (4) BLOCKERS — apply the blocker protocol to every [BLOCKED] card in this folder. Fix-in-place. Return the fix-log of edits already ' +
    'made.',
].join('\n'), { label: 'H1:' + f.split('/').pop(), phase: 'H1-Folder', schema: FIXLOG_SCHEMA, model: 'fable', effort: 'high', stallMs: STALL }))
const h1 = FOLDERS.map((f, i) => (h1Raw[i] ? { folder: f, ...h1Raw[i] } : null)).filter(Boolean)
const h1Skipped = FOLDERS.filter((f, i) => !h1Raw[i]) // dead agents surface as skipped folders, never silent losses
const ROWS = h1.flatMap((r) => r.manifest_rows || [])
log('H1 folder hygiene done across ' + h1.length + ' folders; ' + ROWS.length + ' manifest row(s) reported' +
  (h1Skipped.length ? '; SKIPPED (agent died): ' + h1Skipped.join(', ') : ''))

// --- [H2_GLOBAL]
phase('H2-Global')
const h2Prompt = [
  LAW, '',
  'TASK (global, scope ' + SWEEP + ' — you are the run\'s ONLY central-manifest writer): (1) MANIFEST ROWS — apply every reported row below to ' +
    'its owning manifest exactly once: re-verify each against the implicated README/catalog on CURRENT disk, dedupe semantically identical rows, ' +
    'hand-edit at the symbol anchor (never a line number) preserving label-group order; a row disk disproves is dropped, named in the summary. ' +
    'ROWS: ' + JSON.stringify(ROWS) + '. (2) GLOBAL MANIFEST CONSISTENCY — diff the central dependency manifest against ALL folder README ' +
    'dependency sections from real full reads, never memory (no dependency in a README absent from the manifest; no admitted-and-consumed ' +
    'dependency missing from a README; versions/markers/floors consistent); correct drift surgically NOW. (3) BRANCH/LIB REFINEMENT — ' +
    'refine/correct/de-stale the cross-cutting content in the branch and lib .planning tiers (README/ARCHITECTURE freshness, truthful ' +
    'cross-package seams, correct anchors) to the current state; no new design pages. (4) TOOLING — prove every tooling reference (toolchain ' +
    'invocation, command catalogs, owner routing) against the live owner by resolving/invoking it, and correct drift NOW. Fix-in-place. Return ' +
    'the fix-log of edits already made.',
].join('\n')
const h2Opts = { label: 'H2:global', phase: 'H2-Global', schema: FIXLOG_SCHEMA, model: 'fable', effort: 'high', stallMs: STALL }
// One bounded re-attempt: a dead H2 silently drops every folder's manifest rows.
const h2 = (await agent(h2Prompt, h2Opts)) || (await agent(h2Prompt, { ...h2Opts, label: 'H2:global:retry' }))
log(h2 ? 'H2 global consistency + manifest rows + branch/lib refinement done'
  : 'H2 WRITER DIED TWICE — ' + ROWS.length + ' manifest row(s) unapplied, surfaced in the return')

// --- [H3_COLDVERIFY]
phase('H3-ColdVerify')
const h3Raw = await pool(FOLDERS, CAP, (f) => agent([
  LAW, '', BLOCKER, '',
  'TASK (COLD VERIFY of ' + f + ' — adversarial and WRITING, no prior verdicts exist for you): re-derive the folder hygiene state from fresh ' +
    'full-file reads and attack it — README <-> central manifest <-> BOTH .api tiers fully consistent (no missing/orphan catalog entry, no ' +
    'phantom cited member — EVIDENCE GUARD corroboration required before any deletion, no manifest drift); ARCHITECTURE seams all truthful and ' +
    'codemap == the on-disk page set; no false-complete card; every [BLOCKED] card correctly classified per the protocol (genuine vs ' +
    'resolvable); cards fresh and bidirectional. Where the sweep already edited, PROVE each edit on disk, re-derive its necessity and form, and ' +
    'REPAIR any loose, weak, or token fix to its root form NOW. FIX every residual inconsistency in place WHEREVER it lives — full write ' +
    'authority under RIPPLE AUTHORITY and CURRENT STATE, both ends of every seam; the global manifest writer has finished, so a manifest defect ' +
    'you prove is yours: re-read the manifest immediately before editing and hand-edit surgically at the symbol anchor. Gate: verdict=fixed ' +
    'when you repaired anything; verdict=clean ONLY when your attack found nothing; verdict=fail ONLY for a defect whose truth this run cannot ' +
    'establish (toolchain outage with no corroborating tier) — the entry stays untouched, the fail names it in the summary. Return the fix-log ' +
    'of edits already made.',
].join('\n'), { label: 'H3:' + f.split('/').pop(), phase: 'H3-ColdVerify', schema: FIXLOG_SCHEMA, model: 'fable', effort: 'high', stallMs: STALL }))
const h3 = FOLDERS.map((f, i) => (h3Raw[i] ? { folder: f, ...h3Raw[i] } : null)).filter(Boolean)
const h3Skipped = FOLDERS.filter((f, i) => !h3Raw[i]) // dead agents surface as skipped folders, never silent losses
const failing = h3.filter((v) => v && v.verdict === 'fail')
log('H3 cold verify: ' + (h3.length - failing.length) + '/' + h3.length + ' clean or fixed; ' + failing.length + ' failing' +
  (h3Skipped.length ? '; SKIPPED (agent died): ' + h3Skipped.join(', ') : ''))

return {
  scope: SWEEP,
  h1: h1.map((r) => ({ folder: r.folder, files: (r.files || []).length, verdict: r.verdict, applied: (r.applied || []).length, manifest_rows: (r.manifest_rows || []).length })),
  h2: { verdict: (h2 && h2.verdict) || 'DEAD', applied: (h2 && h2.applied && h2.applied.length) || 0, rows: ROWS.length },
  gate: { clean: h3.length - failing.length, total: h3.length, failing: failing.map((v) => v.summary) },
  skipped: { h1: h1Skipped, h3: h3Skipped },
  unapplied_rows: h2 ? [] : ROWS,
}
