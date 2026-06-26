export const meta = {
  name: 'align-cards',
  whenToUse: 'Align and refine every IDEAS and TASKLOG card under a target root to the current corpus, after a build pass shifts the realized surface.',
  description: 'Align/refine every IDEAS + TASKLOG card under a target root to the current state of the corpus, with Ripple bidirectionality + completeness-vs-charter verification. Language-agnostic (cards are markdown governed by the card schema). NO new cards beyond a genuine completeness fill. args = optional scope (e.g. "libs/python" or "libs/csharp/Rasm.Bim"); empty = all of libs.',
  phases: [
    { title: 'Cards-Discover', detail: 'list every IDEAS.md + TASKLOG.md under the target' },
    { title: 'Cards-Align', detail: 'per card file: re-align to current state, refine/correct/disposition, no new cards' },
    { title: 'Cards-Verify', detail: 'Ripple bidirectionality + completeness vs each folder charter/plan band' },
  ],
}

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'applied', 'verdict', 'summary'], properties: { file: { type: 'string' }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['aligned', 'clean'] }, residual: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [HARNESS] -- bounded worker pool: steady <=cap concurrent, no burst ----------------
const STAGGER_MS = 1500
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
const CAP = 10

// --- [INPUT] -- args = optional scope; empty/"ALL" = all of libs ---
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawScope = (typeof input === 'string') ? input.trim() : (input && typeof input === 'object' && input.target) ? String(input.target).trim() : ''
const SWEEP = (!rawScope || rawScope === 'ALL') ? 'libs' : rawScope

const LAW = [
  'Rasm monorepo. CLAUDE.md card law governs. READ libs/.planning/campaign-method.md for the exact card schema and voice. This is an ALIGNMENT/REFINEMENT pass: re-align each card to the CURRENT state of the folder it governs (status, anchors, what is now realized in-corpus vs still-deferred, a denser/better approach WITHIN the ideas/tasks lane, correct/relocate slugs and anchors). Make cards logical and aligned, NOT coupled. Apply the 7-point density rubric. NO NEW cards (ripple counterparts already exist; do not add more) — the single exception is the completeness stage, which MAY add exactly one missing genuinely-deferred card per real gap. Genuinely DEFERRED only; no test/meta/decision/create-file cards. Fix-in-place; edit ONLY your assigned file. C# PLANNING-HOMING: cards at `libs/csharp/Rasm/{IDEAS,TASKLOG}.md` govern the `Rasm/Geometry` planning effort — its governed design pages are nested at `libs/csharp/Rasm/Geometry/.planning/**` (NOT directly under `Rasm/`), while the `.api/` + `ARCHITECTURE.md` + `README.md` sit at the `Rasm/` root; read the corpus there. Mature siblings `Analysis`/`Domain`/`Vectors` are not card targets.',
  'WRITE-FULLY MANDATE: every alignment/correction you identify you MUST make NOW via Edit/Write directly in the card file — the structured fix-log is a REPORT of edits ALREADY MADE, never a to-do list or hedge; leave nothing behind. If a card file is already aligned and correct, return verdict=clean — never invent edits.',
].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Cards-Discover')
const inv = await agent('List every card file under ' + SWEEP + ' — files named exactly IDEAS.md or TASKLOG.md (at package roots and at .planning tiers). Return each as a repo-relative path. Use find; do not cd.', { label: 'discover', phase: 'Cards-Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const CARD_FILES = ((inv && inv.files) || []).filter(Boolean)
log('Cards discover under ' + SWEEP + ': ' + CARD_FILES.length + ' card files')

phase('Cards-Align')
const aligned = (await pool(CARD_FILES, CAP, (f) => agent([
  LAW, '',
  'TASK: ALIGN/REFINE the cards in ' + f + ' to the current state. Read the file, then read the folder it governs (its .planning pages + ARCHITECTURE + README) to check each card against reality: is an IDEA/TASK now realized in-corpus (mark COMPLETE/DROPPED with a one-line disposition)? is a status stale? is an anchor wrong or now better-placed? does a thesis need refining for a denser/better approach the corpus now enables? Correct/refine/disposition in place. Do NOT add new cards here. Read the wider corpus as needed for cross-folder logic (aligned-not-coupled). Return the fix-log.',
].join('\n'), { label: 'align:' + f.split('/').slice(-2).join('/'), phase: 'Cards-Align', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: 300000 }))).filter(Boolean)
log('Cards aligned across ' + aligned.length + ' files')

phase('Cards-Verify')
const filesList = JSON.stringify(CARD_FILES)
const verify = (await parallel([
  () => agent([LAW, '', 'TASK: BIDIRECTIONALITY verify + FIX IN PLACE across ALL discovered card files (' + filesList + '). For every Ripple, confirm the named counterpart card EXISTS on the other side and references back; repair any dangling/asymmetric/slug-collision Ripple (edit whichever side is wrong). Confirm no in-corpus-done work is still carded and no card duplicates a realized page. Return fix-log; set file to the first card file you edited (or the bidirectionality root).'].join('\n'), { label: 'verify:bidir', phase: 'Cards-Verify', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: 300000 }),
  () => agent([LAW, '', 'TASK: COMPLETENESS verify + FIX IN PLACE. For each target folder, read its plan/charter/README to learn its intended card band and scope, then confirm its IDEAS/TASKLOG pool matches that band; flag any folder short of band or any genuinely-deferred unit missing a card. Confirm pre-existing cards were correctly absorbed/dispositioned. Where a genuinely-deferred unit is missing a card, you MAY add exactly that card here (the one place a missing deferred card is filled) — never duplicate a realized page, never add test/meta/decision cards. Return fix-log; set file to a card file you edited.'].join('\n'), { label: 'verify:complete', phase: 'Cards-Verify', schema: FIXLOG_SCHEMA, effort: 'xhigh', stallMs: 300000 }),
])).filter(Boolean)
log('Cards verify done')

return { scope: SWEEP, files: CARD_FILES.length, aligned: aligned.map((r) => ({ file: r.file, verdict: r.verdict, applied: (r.applied || []).length })), verify: verify.map((r) => ({ file: r.file, verdict: r.verdict })) }
