export const meta = {
  name: 'tidy-planning-docs',
  whenToUse: 'Surgical comment and prose hygiene pass over a .planning design corpus, after a rebuild lands.',
  description: 'Surgical comment + prose HYGIENE pass over every .planning design corpus under a libs/ scope. NOT a rebuild: it changes ONLY fenced-code comments and page prose, never a code fence design, signature, type, case, field, body, or design decision. One work unit per .planning-owning package folder — a folder above 12 design pages splits into two page-slice units so per-stage full-corpus reads stop scaling with folder size — every agent call scheduled by one agent-level slot scheduler (CAP=14), each unit run through a true 3-step ADVERSARIAL pipeline: tidy -> critique -> redteam (each stage consumes the prior stage\'s product). Every comment is treated as agent-facing framing that exists ONLY to help a future rebuild-* agent understand the why/intent/invariant: noise/restatement/process comments are deleted, every kept comment is refined to 1-2 (max 3) high-signal lines, and prose is trimmed of stale/wrong/noise content toward a ~20-25% reduction WHERE possible without losing any load-bearing context. Self-enclosing: a same-kind defect a pipeline exposes in ANOTHER folder\'s .planning page is fixed there in the same pass under the current-state law, never handed to a later run. Edits are scoped to .planning markdown; governing docs (ARCHITECTURE/README/IDEAS/TASKLOG/.api), docs/standards/style-guide.md, and CLAUDE.md [08] divider grammar are read for context only. args = optional libs scope (e.g. libs/csharp, libs/python, libs/typescript); empty = all of libs.',
  phases: [
    { title: 'Discover', detail: 'list every .planning-owning package folder under the scope with its design-page count, plus any language/branch-level .planning tier', model: 'sonnet' },
    { title: 'Tidy', detail: 'per unit (a folder, or one of two page-slices when it exceeds 12 pages): tidy(high) -> critique(high) -> redteam, every stage ADVERSARIAL, comments + prose only, all units concurrent under agent-level slots at CAP=14' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 14
const PAGE_SPLIT = 12
const STAGGER_MS = 1500
const STALL = 300000

// --- [INPUTS] ----------------------------------------------------------------------------
const raw = (typeof args === 'string') ? args.trim() : (args && typeof args === 'object' && args.target) ? String(args.target).trim() : ''
const SCOPE = raw || 'libs'

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['units'], properties: { units: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'planning', 'context_root', 'pages'], properties: { folder: { type: 'string' }, planning: { type: 'string' }, context_root: { type: 'string' }, pages: { type: 'integer' } } } } } }
const HYGIENE_LOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['trimmed', 'refined', 'clean'] }, pages: { type: 'integer' }, comments_cut: { type: 'integer' }, reduction_pct: { type: 'number' }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'SCOPE: this is a SURGICAL HYGIENE pass over ONE planning FOLDER`s design corpus — fenced-code COMMENTS and page PROSE ONLY. It is NOT a rebuild ' +
    'and NOT a design pass: do NOT change any code fence`s design, signatures, types, members, cases, fields, bodies, structure, ordering, or the ' +
    'design decisions a page makes; do NOT add, remove, or reorder design content, cards, signature fences, or tables. Every edit lands in ' +
    '.planning markdown — this folder`s tree is your primary surface.',
  'READ (never edit) the folder`s governing docs for CONTEXT — `ARCHITECTURE.md`, `README.md`, `IDEAS.md`, `TASKLOG.md` — so every trim is ' +
    'informed by what the folder owns and where it integrates. Enumerate BOTH `.api` tiers with a REAL disk listing, never from memory: the ' +
    '`.api/` catalogs at the folder`s context root AND the language-root catalogs at `libs/<lang>/.api/`. The code fences plus both tiers are ' +
    'the VERIFICATION GROUND for every prose/comment claim about a member, type, package, or capability: a claim neither ground verifies is a ' +
    'PHANTOM the pass corrects from the catalogs or deletes, never leaves standing, and never verifies from memory.',
  'READ `docs/standards/style-guide.md` IN FULL and apply it to all prose. READ `CLAUDE.md` section [08]-[FILE_ORGANIZATION] for the OFFICIAL ' +
    'canonical section-divider labels and the divider grammar. Governing docs and `.api` catalogs stay read-only context — this pass never edits them.',
  'RIPPLE + CURRENT STATE: a comments-and-prose defect your verification exposes in ANOTHER folder`s .planning page — the far end of a broken ' +
    'cross-reference, the same phantom claim or noise family — is YOURS in the same pass under the identical comments-and-prose-only law; never a ' +
    'note for a later run. Sibling pipelines run concurrently: before editing any page outside your tree, re-read its CURRENT on-disk state, ' +
    'compose landed sibling edits as found, and resolve a conflict to the stronger form, never a revert.',
].join('\n')
const DIVIDERS = [
  'DIVIDER GRAMMAR (CLAUDE.md [08]): a CANONICAL top-level section divider is the comment marker + space + `---` + a bracketed UPPERCASE_SNAKE ' +
    'label + dash-fill to the language width (e.g. `// --- [TYPES] -------...`, `# --- [CONSTANTS] -------...`), using a canonical label ' +
    '(`TYPES`/`CONSTANTS`/`MODELS`/`ERRORS`/`SERVICES`/`OPERATIONS`/`COMPOSITION`/`EXPORTS`) or a precise domain extension ' +
    '(`[TABLES]`/`[BOUNDARIES]`/`[REPOSITORIES]`/`[POLICIES]`/`[INDEXES]`/`[MIDDLEWARE]`/`[ENTRY]`).',
  'A SUB-SECTION divider inside a large owner uses the LOOSER form — comment marker + `---` + `[LABEL]` with NO trailing dash-fill and freer ' +
    'labeling (e.g. `// --- [VECTOR_HEAT]`, `# --- [NORMAL_ESTIMATION]`). KEEP every divider; repair a malformed one to its correct form; NEVER ' +
    'invent a divider, NEVER convert a loose sub-section divider into a dash-filled canonical one or vice versa, and NEVER relabel a canonical ' +
    'section to a drift label. Dividers are STRUCTURE, exempt from the comment-trim mandate.',
].join('\n')
const COMMENTS = [
  'COMMENT HYGIENE (the core mandate): EVERY comment inside a code fence is AGENT-FACING framing — it exists ONLY to help a FUTURE coding agent ' +
    '(one that rebuilds this page via the rebuild-* workflows) grasp the WHY, intent, invariant, contract, or boundary that the names, types, and ' +
    'signatures do NOT already convey. A comment that restates the code, narrates, or carries ' +
    'task/session/subagent/review-label/proof/history/process framing is NOISE — DELETE it (CLAUDE.md forbids these outright).',
  'REFINE every comment that survives — even an already-1-2-line one — for maximum signal per line; verify it still states something the code does ' +
    'not. A comment of 2+ lines is reduced to the smallest reasonable line count (5 -> 3, 4 -> 2-3), targeting 1-3 lines with 1-2 ideal; only a ' +
    'genuinely subtle multi-part invariant, contract, or boundary keeps 3. The net effect per fence: fewer comments, each load-bearing and tight.',
].join('\n')
const PROSE = [
  'PROSE HYGIENE: improve ALL page prose — lead paragraphs, card lines, section text, bullets, and table cells. REMOVE stale, wrong, outdated, ' +
    'contradicted, or superseded statements; remove noise, empty hedges, redundancy, and report framing per the style-guide; tighten very long ' +
    'line items and walls of explanation down to the load-bearing contract; RAISE quality where a sentence is correct but weak.',
  'Drive the TOTAL prose DOWN by roughly 20-25% WHERE genuinely possible WITHOUT losing any load-bearing content — if a folder is already tight, a ' +
    'smaller reduction (or none) is correct. NEVER pad to look thorough and NEVER cut signal to hit a number; the percentage is a target for noise ' +
    'removal, not a quota over meaning.',
].join('\n')
const PRESERVE = [
  'LOAD-BEARING PRESERVATION (CRITICAL): these pages are FAR from done — the rebuild-* workflows will later rebuild them and RELY on the prose + ' +
    'comments to carry the context, intent, rationale, integration knowledge, and cross-references the code fences alone do not hold. The goal is ' +
    'HIGHER SIGNAL, not less context. Cut ONLY noise / stale / redundant / restating material; PRESERVE every load-bearing explanation, invariant, ' +
    'boundary, rationale, integration point, RESEARCH marker, and cross-reference a future rebuild agent needs.',
  'When unsure whether a line is load-bearing, KEEP it (refined), never delete it. Removing real context to hit a reduction target is a defect ' +
    'WORSE than leaving slight bloat. The pass adds signal and removes noise; it never strips the folder of the framing that makes a future ' +
    'rebuild possible.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE (every stage): the corpus is naive until it survives attack, and dense confident-looking prose is the prime suspect. ' +
    'Assume the state handed to you BOTH under-trimmed (noise/stale/bloat survives) AND over-trimmed (a load-bearing comment, explanation, ' +
    'invariant, or integration point was cut — restore it, refined, from the context-root docs). "good enough" and a prior clean verdict are ' +
    'rejected self-assessments; a no-edit verdict is EARNED by an attack that finds nothing, never conceded on first read. Every surviving ' +
    'comment and prose line must earn its place as high-signal agent-facing framing, and NO code-fence design may have been altered.',
  'NAIVETY is a defect on two axes, both intolerable. COVERAGE: the pass worked a thin slice — short obvious pages, page tops, the first screen ' +
    'of long fences — while deep sections, long pages, bottom-half fences, and table cells kept their noise. APPROACH: enumerated spot-fixes ' +
    'where one rule owns the space — a recurring hedge, restatement, framing, or divider defect is a FAMILY swept corpus-wide with one rule; ' +
    'found instances are pointers to the family, never the whole fix.',
  'Every enumerated defect list in this prompt is a FLOOR, never the complete set: hunt past it — any repeated framing, parallel spelling, or ' +
    'recurring noise family that one tighter rule can own is a target you find yourself. Every stage WRITES: repair each hit in place the ' +
    'moment it is found; output is a fix-log of edits ALREADY MADE, never a ledger, a to-do list, or a would/should hedge.',
].join('\n')
const STYLE = 'PROSE QUALITY — apply docs/standards/style-guide.md: lead each section with the controlling rule; one idea per paragraph; close on ' +
  'the consequence or boundary. Cut hedges (may/might/probably/generally/where possible/if needed) and report framing; preserve contract scope ' +
  'qualifiers (optional/if present/when configured). Backtick every code symbol, type, member, path, command, and literal value.'
const DOCTRINE = [LAW, '', DIVIDERS, '', COMMENTS, '', PROSE, '', PRESERVE, '', STYLE].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// Agent-level slot scheduler: every agent() call takes one slot, so unit chains launch freely via Promise.all while true in-flight agents stay at CAP.
const makeSlots = (cap) => {
  let active = 0
  let gate = Promise.resolve()
  const waiters = []
  const stagger = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
  return async (fn) => {
    if (active >= cap) await new Promise((res) => waiters.push(res))
    active++
    await stagger()
    try { return await fn() } finally { active--; const next = waiters.shift(); if (next) next() }
  }
}
const slot = makeSlots(CAP)
// A folder past PAGE_SPLIT design pages becomes two page-slice units, bounding each stage's full-corpus read.
const splitUnits = (list) => list.flatMap((u) => (u.pages > PAGE_SPLIT)
  ? [1, 2].map((part) => ({ folder: u.folder, planning: u.planning, context_root: u.context_root, pages: u.pages, slice: part }))
  : [{ folder: u.folder, planning: u.planning, context_root: u.context_root, pages: u.pages || 0, slice: 0 }])
const sliceCtx = (u) => !u.slice ? '' : '\nPAGE SLICE ' + u.slice + ' of 2 (this folder holds ' + u.pages + ' design pages; the corpus is split so ' +
  'per-stage reads stay bounded — every whole-corpus instruction above NARROWS to your slice): enumerate every markdown design page under the ' +
  'planning tree sorted lexicographically by path; your pages are ' + (u.slice === 1 ? 'the FIRST half of that ordering (1..ceil(N/2))' :
  'the SECOND half of that ordering (after ceil(N/2))') + '. PROCESS and EDIT only your slice pages — the sibling slice agent owns the rest ' +
  'concurrently; a FAMILY sweep runs across your slice; the RIPPLE + CURRENT-STATE law still governs an out-of-tree far end.'
const ctx = (u) => '\nFOLDER: ' + u.folder + '\nPLANNING TREE (your primary surface): ' + u.planning + '\nCONTEXT ROOT (read-only ' +
  'governing docs + .api): ' + u.context_root + sliceCtx(u)
const tidyPrompt = (u) => [DOCTRINE, '', ADVERSARIAL, '', 'TASK: SURGICAL HYGIENE PASS over EVERY design page under this folder`s .planning tree. ' +
  'First read the context-root governing docs + BOTH .api tiers + the style-guide + CLAUDE.md [08] divider grammar, then process EVERY ' +
  'markdown design page under the planning tree: apply the COMMENT hygiene (trim/refine/delete per the mandate), the PROSE hygiene (remove ' +
  'stale/wrong/noise, tighten, ~20-25% reduction where possible), and the DIVIDER grammar, preserving ALL design/code and ALL load-bearing ' +
  'context for future rebuild agents. Touch comments + prose ONLY — never a code fence`s design, signatures, structure, or content. Report ' +
  'the folder, pages touched, comments cut, an approximate prose reduction_pct, and a one-line summary. verdict `trimmed` unless the corpus ' +
  'was already maximally tight (then `refined`/`clean`).' + ctx(u)].join('\n')
const critiquePrompt = (u) => [DOCTRINE, '', ADVERSARIAL, '', 'TASK: HOSTILE HYGIENE AUDIT + FIX IN PLACE over EVERY page under this folder`s ' +
  '.planning tree. Trust NOTHING the tidy pass claims; audit every page line by line and REPAIR every hit in place — the numbered checks are a ' +
  'FLOOR hunted past, never the whole audit: (1) any noise / stale / wrong / restating / process comment the pass MISSED -> delete it; (2) any ' +
  'OVER-trim — a load-bearing comment, explanation, invariant, boundary, rationale, integration point, or RESEARCH marker the pass wrongly cut ' +
  '-> RESTORE it (refined, from the context-root docs if needed); (3) any comment still over 1-2 lines without a genuinely subtle multi-part ' +
  'reason -> reduce it (max 3); (4) any malformed/invented/mis-converted divider -> fix to the correct canonical-vs-loose form; (5) any prose ' +
  'still bloated, hedged, or report-framed -> tighten per the style-guide; (6) any prose/comment claim about a member, type, package, or ' +
  'capability that neither the fence nor either .api tier verifies -> correct it from the catalogs or delete it (PHANTOM); (7) ANY code fence ' +
  'whose design/signatures/types/structure/content was altered by the tidy pass -> REVERT that change (hygiene is comments + prose ONLY). Every ' +
  'hit is a FAMILY: when a check lands, sweep the same defect pattern across the whole corpus — both naivety axes attacked. The report is the ' +
  'fix-log of edits made: folder, pages, reduction_pct, summary; verdict `trimmed` when this attack cut or restored material, `refined` for ' +
  'quality-only edits, `clean` ONLY when a full attack found nothing.' + ctx(u)].join('\n')
const redteamPrompt = (u) => [DOCTRINE, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE over this folder`s .planning tree — the ' +
  'LAST and MOST AGGRESSIVE pass; red-team is the critique AND MORE. Re-attack every page COLD, trusting nothing the prior passes claimed — ' +
  'every critique dimension re-run in full — plus these lenses, each a floor: (COUNTERFACTUAL) read each page as the cold rebuild agent that ' +
  'must work from it alone; where the surviving prose + comments no longer carry the why/invariant/integration the fences cannot, the trim went ' +
  'too deep — restore it, refined, from the context-root docs; (LONG-TAIL) attack where prior passes fade — deep sections, long pages, ' +
  'bottom-half fences, table cells, the last pages of the tree: the subtle surviving noise, the still-bloated line item, the stale or ' +
  'contradicted fact, the comment that does not earn its place; (PHANTOMS) any prose/comment claim or cross-reference that the fence, both .api ' +
  'tiers, and the sibling pages do not verify -> correct or delete it — a defective far end in another folder`s page is fixed there per RIPPLE ' +
  '+ CURRENT STATE; (FAMILY) every defect found is a family swept corpus-wide, never a spot-fix — both naivety axes attacked; (COMPLETENESS) ' +
  'every page under the tree is attacked, none skipped. Confirm: every comment is high-signal agent-facing framing at 1-2 (max 3) lines; every ' +
  'divider is correct; the prose reduction held (~20-25%, or correctly less for an already-tight folder) WITHOUT losing signal; and NO code ' +
  'fence design/signature/structure was touched anywhere. The folder must end objectively higher-signal, leaner, and fully load-bearing. Fix ' +
  'every defect in place; a page already at the bar is proven by an attack that finds nothing, never conceded — never invent churn. The report ' +
  'is the fix-log of edits made: folder, pages, reduction_pct, summary; verdict `clean` ONLY when the full cold attack found nothing.' + ctx(u)].join('\n')
const STAGES = [
  { key: 'tidy', build: tidyPrompt, effort: 'high' },
  { key: 'crit', build: critiquePrompt, effort: 'high' },
  { key: 'redteam', build: redteamPrompt, effort: 'high' },
]
const processUnit = async (u) => { // true per-unit pipeline: each stage consumes the prior stage's on-disk product; a dead stage ends its unit's chain
  const logs = {}
  for (const st of STAGES) {
    const r = await slot(() => agent(st.build(u), { label: st.key + ':' + u.folder + (u.slice ? '#' + u.slice : ''), phase: 'Tidy', schema: HYGIENE_LOG_SCHEMA, effort: st.effort, stallMs: STALL }))
    if (r === null) break
    logs[st.key] = r
  }
  return { folder: u.folder, slice: u.slice, logs, ok: Object.keys(logs).length === STAGES.length }
}

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Discover')
const inv = await agent('List every PACKAGE FOLDER under ' + SCOPE + ' that owns design pages in a `.planning/` tree. For each, return {folder ' +
  '(the package dir, repo-relative, e.g. libs/csharp/Rasm.AppHost), planning (the .planning dir that holds the design pages, e.g. ' +
  'libs/csharp/Rasm.AppHost/.planning), context_root (the dir whose ARCHITECTURE.md/README.md/IDEAS.md/TASKLOG.md + .api/ govern that planning)}. ' +
  'For a normal package folder=context_root=the package dir and planning=<package>/.planning. Also ' +
  'emit a unit for any language-wide or branch-level .planning under the scope (e.g. libs/<lang>/.planning) with context_root the same dir. ' +
  'EXCLUDE folders with no .planning (host-bound source packages such as Rasm.Rhino/Rasm.Grasshopper). This is DISCOVERY and read-only is its ' +
  'ONLY concession: enumerate with a REAL find/fd listing from disk, NEVER from memory of the repo layout, and VERIFY every unit against disk ' +
  'state — the planning tree exists and holds at least one .md design page (return that count as pages), and the context_root actually holds ' +
  'the governing docs + .api/. The unit list is scope resolution, an initial pointer: downstream agents full-read every page under each ' +
  'planning tree themselves, so the list never caps what they process. Do not cd; do not edit anything.', { label: 'discover', phase: 'Discover', schema: DISCOVER_SCHEMA, model: 'sonnet', effort: 'low', stallMs: STALL })
const units = ((inv && inv.units) || []).filter((u) => u && u.folder && u.planning)
const workUnits = splitUnits(units)
log('Discover under ' + SCOPE + ': ' + units.length + ' planning folders -> ' + workUnits.length + ' work units; agent-level slots at CAP=' + CAP)

// --- [TIDY]
phase('Tidy')
const done = (await Promise.all(workUnits.map((u) => processUnit(u)))).filter(Boolean)
const complete = done.filter((r) => r.ok)
const lastLog = (r) => r.logs.redteam || r.logs.crit || r.logs.tidy || null
const commentsCut = done.reduce((n, r) => n + STAGES.reduce((m, st) => m + ((r.logs[st.key] && r.logs[st.key].comments_cut) || 0), 0), 0)
const pcts = done.map(lastLog).filter((l) => l && typeof l.reduction_pct === 'number').map((l) => l.reduction_pct)
const reduction = pcts.length ? Math.round((pcts.reduce((a, b) => a + b, 0) / pcts.length) * 10) / 10 : 0
log('Tidy: ' + complete.length + '/' + workUnits.length + ' units fully passed (tidy -> critique -> redteam); comments cut ' + commentsCut + '; mean prose reduction ' + reduction + '%')
return { workflow: 'tidy-planning-docs', scope: SCOPE, folders: units.length, units: workUnits.length, complete: complete.length, incomplete: done.filter((r) => !r.ok).length, comments_cut: commentsCut, reduction_pct_mean: reduction, results: done.map((r) => ({ folder: r.folder, slice: r.slice, log: lastLog(r) })) }
