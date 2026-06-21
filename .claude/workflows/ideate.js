export const meta = {
  name: 'ideate',
  description: 'Rebuild a folder IDEAS + TASKS card pool to world-class: survey the realized corpus and research the real domain, author the genuinely-deferred idea/task pool, then fix-in-place constructive critique + hostile adversarial redteam, with Ripple bidirectionality. Language-agnostic (cards are markdown governed by the card schema). Authors NO design pages (that is the rebuild-* workflows) and aligns nothing pre-existing for its own sake (that is align-cards) — this is the greenfield/expansion pool generator. args = optional scope (e.g. "libs/python/geometry"); empty = all of libs.',
  phases: [
    { title: 'Survey', detail: 'per folder: map realized capability + current pool + researched domain-completeness gaps + cross-folder seams' },
    { title: 'Ideate', detail: 'author/rebuild the IDEAS + TASKS pool grounded in the survey, genuinely-deferred only' },
    { title: 'Critique', detail: 'fix-in-place: pull the pool up — density, domain-completeness, anchors, ripples' },
    { title: 'Redteam', detail: 'fix-in-place hostile reviewer: attack redundancy, mis-carding, dangling ripples, under-ideation' },
  ],
}

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folders'], properties: { folders: { type: 'array', items: { type: 'string' } } } }
const SURVEY_SCHEMA = { type: 'object', additionalProperties: false, required: ['realized', 'gaps', 'notes'], properties: { realized: { type: 'array', items: { type: 'string' } }, gaps: { type: 'array', items: { type: 'string' } }, cross_folder: { type: 'array', items: { type: 'string' } }, existing_cards: { type: 'array', items: { type: 'string' } }, notes: { type: 'string' } } }
const CARDLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, applied: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'refined', 'hardened', 'clean'] }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }, summary: { type: 'string' } } }

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
const nameOf = (f) => f.split('/').pop() || f

const LAW = [
  'Rasm monorepo. CLAUDE.md card law governs. READ libs/.planning/campaign-method.md for the exact card schema and voice. You produce a folder IDEAS + TASKS card pool: IDEAS are conceptual/multi-step/higher-order deferred capability; TASKS are concrete/focused deferred units. Every card passes the 7-point density rubric (polymorphic collapse; one-hop; growth-axis absorption; Result/Option rails; library-at-depth; policy-values not boolean knobs; greenfield in-place). A card is ONLY for genuinely DEFERRED work — NEVER duplicate a realized design page, NEVER a test/meta/decision/unblock/create-file card. Cards are FLOORS, not ceilings. C# PLANNING-HOMING: under `libs/csharp/Rasm` the active planning effort is `Rasm/Geometry` — author its cards into `libs/csharp/Rasm/{IDEAS,TASKLOG}.md` (package root) while surveying its design pages at `libs/csharp/Rasm/Geometry/.planning/**`; the governing `.api/`/`ARCHITECTURE.md`/`README.md` are at the `Rasm/` root. Mature siblings `Analysis`/`Domain`/`Vectors` are not card targets.',
  'RIPPLE LAW: a cross-folder dependency is a Ripple that pairs two cards — each references the other by backticked owner + backticked [SLUG]. A Ripple to an EXISTING counterpart REFERENCES it (never re-creates the slug). One canonical slug per bounded concept; no collisions.',
  'RESEARCH MANDATE: never guess domain completeness. Read the folder realized corpus (its design pages + ARCHITECTURE + README) AND research the REAL domain to find genuinely-deferred capability the pool should hold. The pool must reflect the domain, not only what is already written.',
  'WRITE-FULLY MANDATE: every card you author/refine/repair you MUST write NOW via Edit/Write directly into the folder IDEAS.md / TASKLOG.md (create them if absent) — the structured log is a REPORT of edits ALREADY MADE, never a to-do list or hedge; leave nothing behind. If the pool is already world-class, return verdict=clean — never invent cards to look busy.',
].join('\n')

const surveyPrompt = (folder) => [LAW, '', 'TASK: SURVEY ' + folder + ' (read-only). Read its design pages (' + folder + '/.planning/**), ARCHITECTURE.md, README.md, and any existing ' + folder + '/IDEAS.md + TASKLOG.md. Produce a structured map: (realized) the capability the pages ALREADY deliver — these must NOT be carded; (gaps) genuinely-deferred capability the REAL domain needs beyond what is realized — research the domain, do not guess; (cross_folder) seams where a card should Ripple to a sibling owner; (existing_cards) the current card pool and its state. Return the structured survey.'].join('\n')
const ideatePrompt = (folder, ctx) => [LAW, '', 'SURVEY of ' + folder + ':\n' + ctx, '', 'TASK: author/rebuild the IDEAS + TASKS pool in ' + folder + '/IDEAS.md and ' + folder + '/TASKLOG.md, grounded in the survey above. Author IDEAS for the conceptual/multi-step gaps and TASKS for the concrete/deferred ones; each card dense at the 7-point rubric, genuinely-deferred ONLY (never a realized page from the survey), correctly anchored, with Ripple refs to existing cross-folder counterparts where the seam exists. Match the exact card schema/voice from campaign-method.md. Fix-in-place (write the files, create if absent). Return the card-log + residual — each a {files: [both folders IDEAS.md/TASKLOG.md the Ripple pairs], claim} object for any CROSS-FOLDER Ripple counterpart you cannot author from this folder (NO severity; the reconcile phase fixes all of them).'].join('\n')
const critiquePrompt = (folder) => [LAW, '', 'TASK: CONSTRUCTIVE CRITIQUE + FIX IN PLACE of the cards in ' + folder + '/IDEAS.md + TASKLOG.md. Pull the pool UP: denser/sharper theses, fuller domain-completeness (is a genuinely-deferred capability still missing?), better anchors, richer and correct Ripple refs, polymorphic/AOP framing in Capability/Shape. EDIT the card files. Return the card-log + residual — each a {files: [both folders IDEAS.md/TASKLOG.md the Ripple pairs], claim} object for any CROSS-FOLDER Ripple asymmetry you cannot fix from this folder (NO severity; the reconcile phase fixes all of them).'].join('\n')
const redteamPrompt = (folder) => [LAW, '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE of the cards in ' + folder + '/IDEAS.md + TASKLOG.md. You are a HOSTILE reviewer whose goal is to REJECT this pool — assume it is redundant, under-dense, or under-ideated until it proves otherwise. ATTACK and repair: any card that duplicates a realized design page (mis-carded in-pass work — delete/disposition); any test/meta/decision/unblock/create-file card; dangling/asymmetric/colliding Ripple (counterpart slug absent or not referencing back); weak/under-dense cards failing the 7-point rubric; non-deferred work wrongly carded. ALSO the FORWARD lens: genuinely-deferred domain capability MISSING from the pool (under-ideation), a pool that understates the real domain — author what is missing. Run COUNTERFACTUALS (is this the strongest set of deferred bets, or is there a denser/higher-leverage framing?). Repair every defect in place. Return the card-log + residual — each item a {files: [both folders IDEAS.md/TASKLOG.md the Ripple pairs], claim} object (genuine cross-folder items only; everything within this folder you fix yourself).'].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const ideateFolder = async (folder) => {
  const survey = await agent(surveyPrompt(folder), { label: 'survey:' + nameOf(folder), phase: 'Survey', schema: SURVEY_SCHEMA, effort: 'high' })
  if (survey === null) return { folder, logs: {}, ok: false }
  const authored = await agent(ideatePrompt(folder, JSON.stringify(survey)), { label: 'ideate:' + nameOf(folder), phase: 'Ideate', schema: CARDLOG_SCHEMA, effort: 'max' })
  if (authored === null) return { folder, logs: {}, ok: false }
  const crit = await agent(critiquePrompt(folder), { label: 'crit:' + nameOf(folder), phase: 'Critique', schema: CARDLOG_SCHEMA, effort: 'xhigh' })
  if (crit === null) return { folder, logs: { ideate: authored }, ok: false }
  const rt = await agent(redteamPrompt(folder), { label: 'redteam:' + nameOf(folder), phase: 'Redteam', schema: CARDLOG_SCHEMA, effort: 'max' })
  return { folder, logs: { ideate: authored, crit, redteam: rt }, ok: rt !== null }
}

// --- [COMPOSITION] -----------------------------------------------------------------------
phase('Survey')
const inv = await agent('List the package/area folders under ' + SWEEP + ' that own a design corpus (immediate child directories containing a .planning directory and/or a README.md + ARCHITECTURE.md). Include the branch-level tier if it owns cards. Return each as a repo-relative path. Use find/ls; do not cd.', { label: 'discover', phase: 'Survey', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const FOLDERS = ((inv && inv.folders) || []).filter(Boolean)
log('Ideate discover under ' + SWEEP + ': ' + FOLDERS.length + ' folders')

const done = (await pool(FOLDERS, CAP, (f) => ideateFolder(f))).filter(Boolean)

// --- [RECONCILE] -- repair cross-folder Ripple asymmetries the per-folder pass deferred ----
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const cardFiles = (folder) => [folder + '/IDEAS.md', folder + '/TASKLOG.md']
const norm = (x, folder) => typeof x === 'string' ? { files: cardFiles(folder), claim: x } : { files: x.files && x.files.length ? x.files : cardFiles(folder), claim: x.claim }
const allRes = []
for (const r of done) for (const st of ['ideate', 'crit', 'redteam']) { const l = r.logs && r.logs[st]; if (l && l.residual) for (const x of l.residual) allRes.push(norm(x, r.folder)) }
const uniq = [...new Map(allRes.map((r) => [r.files.join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Ideate: ' + done.length + '/' + FOLDERS.length + ' folder pools; reconcile ' + uniq.length + ' ripple residuals -> ' + clusters.length + ' clusters')
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pipeline(
    clusters,
    (cl) => agent([LAW, '', 'TASK: RECONCILE these cross-folder Ripple residuals (dangling/asymmetric/colliding pairings). There is NO severity — address EVERY residual. Read EVERY listed card file, repair BOTH sides so each Ripple references an existing counterpart that references back — author the missing counterpart card or correct the slug, never leave a dangling Ripple. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'), { label: 'reconcile-fix', phase: 'Reconcile', schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: 300000 }),
    (fix, cl, i) => fix ? agent([LAW, '', 'TASK: ADVERSARIAL COMPLETENESS VERIFY. A reconcile agent claims to have repaired the cross-folder Ripple residuals below. Read the named card files from disk and prove, per claim, whether the pairing is ACTUALLY bidirectional and collision-free now — status "fixed" (resolved), "invalid" (the claimed asymmetry is factually wrong — cite why), or "open" (still dangling/asymmetric). Default "open" on any doubt; "invalid" only when provably wrong. One verdict per claim plus overall. Claims:\n' + JSON.stringify(cl, null, 1) + '\nCard files touched: ' + JSON.stringify(fix.files)].join('\n'), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 }).then((v) => ({ cluster: cl, fix, verify: v })) : null,
  )).filter(Boolean)
}
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_residual = claimsAll.filter((c) => c.status === 'open').map((c) => c.claim)
const dropped = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
log('Reconcile: ' + clusters.length + ' clusters; ' + hard_residual.length + ' open (hard residual), ' + dropped.length + ' dropped as invalid')
return { scope: SWEEP, folders: done.map((r) => r.folder), clusters: clusters.length, hard_residual: hard_residual, dropped: dropped }
