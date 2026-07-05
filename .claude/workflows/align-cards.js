export const meta = {
  name: 'align-cards',
  whenToUse: 'Align and refine every IDEAS and TASKLOG card under a target root to the current corpus, after a build pass shifts the realized surface.',
  description: 'Align/refine every IDEAS + TASKLOG card under a target root to the current state of the corpus, with Ripple bidirectionality + completeness-vs-charter verification. Language-agnostic (cards are markdown governed by the card schema). NO new cards beyond a genuine completeness fill. Self-enclosing: every agent writes with full ripple authority — a defect its work exposes in any card file is fixed in the same pass, both ends, never reported for a later phase or session. One flat pool (CAP=10) schedules every agent: the align fan runs all card files concurrently, then the two verify lenses (bidirectionality, completeness) run concurrently over the whole set under the current-state law. args = optional scope (e.g. "libs/python" or "libs/csharp/Rasm.Bim"); empty = all of libs.',
  phases: [
    { title: 'Cards-Discover', detail: 'list every IDEAS.md + TASKLOG.md under the target', model: 'sonnet' },
    { title: 'Cards-Align', detail: 'per card file, all concurrent: re-align to current state, refine/correct/disposition, cross-file ripples fixed in-pass, no new cards' },
    { title: 'Cards-Verify', detail: 'two concurrent writing lenses over the whole set: Ripple bidirectionality + completeness vs each folder charter/plan band' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const STAGGER_MS = 1500
const STALL = 300000

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'string' } } } }
// Required-but-possibly-empty `repaired` is an attestation: ripple authority ran; empty attests no cross-file defect surfaced.
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'applied', 'verdict', 'repaired', 'summary'], properties: { file: { type: 'string' }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['aligned', 'clean'] }, repaired: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo. CLAUDE.md card law governs. READ libs/.planning/campaign-method.md for the role law and voice, and libs/.planning/README.md for ' +
    'the exact IDEAS/TASKLOG card schema. This is an ALIGNMENT/REFINEMENT pass: re-align each card to the CURRENT state of the folder it governs ' +
    '(status, anchors, what is now realized in-corpus vs still-deferred, a denser/better approach WITHIN the ideas/tasks lane, correct/relocate ' +
    'slugs and anchors). Make cards logical and aligned, NOT coupled. Apply the 7-point density rubric (polymorphic collapse; one-hop; growth-axis ' +
    'absorption; Result/Option rails; library-at-depth; policy-values not boolean knobs; greenfield in-place) as a FLOOR, never the complete ' +
    'set — any repeated structure, parallel spelling, or enumerable family a single algebra, table, fold, or generator can own is a target you ' +
    'hunt past the rubric yourself. NO NEW cards (ripple counterparts already exist; do not add more) — the single exception is ' +
    'the completeness lens, which MAY add exactly one missing genuinely-deferred card per real gap. Genuinely DEFERRED only; no test/meta/decision/create-file cards. Fix-in-place.',
  'ADVERSARIAL STANCE: every card, status, anchor, Ripple, and prior disposition is naive or illusory until it survives your attack; dense, ' +
    'confident-looking cards are the prime suspects, and verdict=clean is EARNED by an attack that finds nothing, never conceded on a first read. ' +
    'Naivety in a kept thesis is a defect on two axes: COVERAGE — the card defers a thin slice of the concept it names; APPROACH — the thesis ' +
    'enumerates hardcoded instances (a fixed roster of styles/patterns/variants) where one parameterized generator should own the space; a roster is ' +
    'seed DATA feeding that generator, never the mechanism. Repair a thesis failing either axis in place to the root form. ULTRA-STACKING: enumerate ' +
    'BOTH `.api` tiers with a real listing — folder-local `.api/` and language-root `libs/<lang>/.api/` — and read every catalog a thesis touches to ' +
    'operator depth; a cited member you cannot verify in a catalog or the corpus is a phantom — correct it to the verified spelling or disposition ' +
    'the card; an admitted capability neither realized in-corpus nor carded as deferred is a REAL gap, fillable only in the completeness lens. ' +
    'HARDENING: capability is improved or extended, NEVER dropped for lack of a current consumer — zero consumers never lowers a card\'s bar, and a card is dropped only against realized work or a proven falsehood on disk, never against missing demand.',
  'WRITE-FULLY + RIPPLE AUTHORITY: every alignment/correction you identify you make NOW via Edit/Write — the structured fix-log is a REPORT of ' +
    'edits ALREADY MADE, never a to-do list or hedge; the run hands nothing to a later phase, session, or workflow. Any card file your work exposes ' +
    'as defective — the far end of a Ripple you corrected, a sibling folder card carrying the same stale claim, a slug-collision partner — is YOURS ' +
    'in the same pass: fix it there, both ends, listing every file beyond your primary in `repaired`. An already-aligned file returns verdict=clean — never invent edits.',
  'CURRENT STATE: sibling agents run concurrently and may write the same card files. Before ANY edit, re-read the file\'s CURRENT on-disk state; ' +
    'a landed sibling edit is composed as found, never assumed or re-derived; a conflict resolves to the stronger form, never a revert.',
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

phase('Cards-Discover')
const inv = await agent('DISCOVERY: enumerate every card file under ' + SWEEP + ' — files named exactly IDEAS.md or TASKLOG.md, at BOTH tiers: ' +
  'package roots and .planning tiers. The listing MUST be a live find run against the working tree — never memory, a prior run, or an index — ' +
  'with the scope path first proven to exist on disk. Completeness is the product: a card file you miss silently escapes the whole pass; widen the ' +
  'search on any doubt and verify every returned path exists. Return sorted repo-relative paths. Read-only; use find; do not cd.',
  { label: 'discover', phase: 'Cards-Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const CARD_FILES = ((inv && inv.files) || []).filter(Boolean)
log('Cards discover under ' + SWEEP + ': ' + CARD_FILES.length + ' card files')

// --- [CARDS_ALIGN]
phase('Cards-Align')
const aligned = (await pool(CARD_FILES, CAP, (f) => agent([
  LAW, '',
  'TASK: ALIGN/REFINE the cards in ' + f + ' (your primary file) to the current state. Read the card file IN FULL, then read the folder it ' +
    'governs IN FULL — its .planning pages, ARCHITECTURE, README, and both .api tiers — and attack every card line-by-line against that reality: ' +
    'an IDEA/TASK now realized in-corpus is marked COMPLETE/DROPPED with a one-line disposition ONLY against the realized page or fence you ' +
    'located on disk, never against the claim the card itself makes; a stale status, a wrong or now better-placed anchor, a thesis the corpus now ' +
    'enables to be denser or that fails either naivety axis, and a phantom-cited member are each corrected in place. These checks are a FLOOR, ' +
    'never the complete set — hunt past them for any defect the card schema or the corpus exposes. Do NOT add new cards. Read the wider corpus as ' +
    'needed for cross-folder logic (aligned-not-coupled); a defect that read exposes in another card file is yours per RIPPLE AUTHORITY under CURRENT STATE. Return the fix-log of edits already made.',
].join('\n'), { label: 'align:' + f.split('/').slice(-2).join('/'), phase: 'Cards-Align', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: STALL }))).filter(Boolean)
log('Cards aligned across ' + aligned.length + ' files')

// --- [CARDS_VERIFY]
phase('Cards-Verify')
const verify = (await pool([
  () => agent([LAW, '', 'TASK: BIDIRECTIONALITY verify + FIX IN PLACE across ALL discovered card files (' + JSON.stringify(CARD_FILES) + '). This ' +
    'is an adversarial WRITING verify, never a confirmation: treat every Ripple and every disposition the align pass just made as suspect until ' +
    'proven on disk. For every Ripple, confirm the named counterpart card EXISTS on the other side and references back; repair any ' +
    'dangling/asymmetric/slug-collision Ripple (edit whichever side is wrong). Re-derive each COMPLETE/DROPPED disposition: the realized page or ' +
    'fence must actually exist in-corpus — revert or correct any disposition the disk does not prove; no in-corpus-done work stays carded and no ' +
    'card duplicates a realized page. A token, loose, or single-point align fix is itself a defect you rebuild to the root form before ' +
    'classifying. Return the fix-log of edits already made; set file to the first card file you edited and list every edited file in repaired.'].join('\n'),
    { label: 'verify:bidir', phase: 'Cards-Verify', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: STALL }),
  () => agent([LAW, '', 'TASK: COMPLETENESS verify + FIX IN PLACE. For each target folder, read its plan/charter/README IN FULL to learn its ' +
    'intended card band and scope — from disk, never memory — then attack its IDEAS/TASKLOG pool against the band: a folder short of band or a ' +
    'genuinely-deferred unit missing its card is a defect you CLOSE NOW by adding exactly that missing genuinely-deferred card (the one place a ' +
    'missing deferred card is filled), never a finding you flag. Mine the gap sources to operator depth: both .api tiers and the README registry ' +
    'are where the uncarded admitted capability hides. Prove pre-existing cards were correctly absorbed/dispositioned: a wrong, token, or loose ' +
    'disposition is repaired to the root card form, never noted. Never duplicate a realized page. Return the fix-log of edits already made; set ' +
    'file to a card file you edited and list every edited file in repaired.'].join('\n'), { label: 'verify:complete', phase: 'Cards-Verify', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: STALL }),
], CAP, (t) => t())).filter(Boolean)
log('Cards verify done: ' + verify.map((r) => r.verdict).join(', '))

return { scope: SWEEP, files: CARD_FILES.length, aligned: aligned.map((r) => ({ file: r.file, verdict: r.verdict, applied: (r.applied || []).length, repaired: (r.repaired || []).length })), verify: verify.map((r) => ({ file: r.file, verdict: r.verdict, repaired: (r.repaired || []).length })) }
