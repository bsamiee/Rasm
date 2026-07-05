export const meta = {
  name: 'cold-verify',
  whenToUse: 'Campaign closure gate: after a rebuild campaign lands, verify the whole target corpus against its root DECISION/brief and fix every miss in place. args = {doc, root} or an array of such pairs; campaigns verify in parallel lanes. A clean close is earned by attack; findings resolve in-run, never as a report.',
  description: 'Cold-verify pass over one or more landed campaigns. Per campaign: one sonnet plan partitions the target folder into balanced verification slices; opus verifiers fan out, each reading the root doc IN FULL plus its slice pages IN FULL, hunting missing/wrong/faked/naive work with typed anchored findings (one verifier owns the governance lane: index docs, manifest rows, csproj/README registries, .api anchors, acceptance traces, rider receipts); one fable resolver re-verifies every finding on disk and fixes ALL of it in place with full write authority, pushing touched pages past the ruling per the floor law; one fable cold close re-attacks the resolved corpus and fixes residuals itself — a clean verdict is earned by an attack that finds nothing.',
  phases: [
    { title: 'Plan', detail: 'per campaign: enumerate pages, partition into balanced slices', model: 'sonnet' },
    { title: 'Verify', detail: 'per campaign: opus slice verifiers + one governance verifier, read-only, typed anchored findings', model: 'opus' },
    { title: 'Resolve', detail: 'per campaign: one fable fixes every validated finding in place, floor law on touched pages', model: 'fable' },
    { title: 'Close', detail: 'per campaign: one fable cold adversarial re-read, residuals fixed in place', model: 'fable' },
  ],
}

// --- [CONSTANTS] ---
const SLICES = 4
const STALL = 300000

// --- [INPUTS] ---
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const CAMPS = (Array.isArray(argsIn) ? argsIn : [argsIn]).filter((c) => c && c.doc && c.root)
if (!CAMPS.length) { log('No campaigns — pass {doc, root} or an array of pairs.'); return { campaigns: 0 } }

// --- [MODELS] ---
const PLAN = { type: 'object', additionalProperties: false, required: ['slices', 'governance'], properties: {
  slices: { type: 'array', items: { type: 'array', items: { type: 'string' } } },
  governance: { type: 'array', items: { type: 'string' } } } }
const FINDINGS = { type: 'object', additionalProperties: false, required: ['findings', 'summary'], properties: {
  findings: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'class', 'finding', 'anchor'], properties: {
    target: { type: 'string' }, class: { type: 'string', enum: ['missing', 'wrong', 'faked', 'naive', 'drift', 'phantom'] },
    finding: { type: 'string' }, anchor: { type: 'string' }, fix: { type: 'string' } } } },
  summary: { type: 'string' } } }
const FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'resolved', 'rejected', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  resolved: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: {
    target: { type: 'string' }, action: { type: 'string' } } } },
  rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['finding', 'reason'], properties: {
    finding: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' } } }
const CLOSE = { type: 'object', additionalProperties: false, required: ['verdict', 'files', 'summary'], properties: {
  verdict: { type: 'string', enum: ['clean', 'fixed'] }, files: { type: 'array', items: { type: 'string' } },
  residual: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }

// --- [SHARED_BLOCKS] ---
const CTX = (c) => 'Rasm monorepo, planning phase. The campaign over ' + c.root + ' is LANDED; ' + c.doc + ' at the repo ' +
  'root is the binding ruling it executed. This pass earns the cold close: the corpus is presumed defective until an ' +
  'attack finds nothing. All .md prose follows docs/standards/style-guide.md — declarative agent-facing law, no ' +
  'provenance, no process narration, no hedges. Never run git commit.'
const HUNT = 'HUNT CLASSES: missing (a ruled motion, page, owner, case, band, seam row, or rider with no landed ' +
  'counterpart), wrong (landed but contradicting the ruling or the doctrine), faked (claimed done — prose asserts what ' +
  'the fence body omits, a receipt without the edit, a re-anchor to a dead target), naive (landed thin — a slice of the ' +
  'ruled capability, an underutilized admitted package, a ceiling-read of a floor ruling), drift (two landed surfaces ' +
  'disagreeing — page vs index doc vs manifest vs .api), phantom (a cited member, page, or anchor that does not exist). ' +
  'Every finding carries a file anchor and, where the fix is derivable, the exact fix. Verify cited external members ' +
  'against the .api catalogs; never trust page prose about itself.'

// --- [COMPOSITION] ---
const lanes = await parallel(CAMPS.map((c) => async () => {
  const tag = c.root.split('/').pop()
  const plan = await agent(CTX(c) + '\n\nTASK: thin enumerate (read-only). List every design page under ' + c.root +
    '/.planning/ (real ls/find, never memory) and partition them into ' + SLICES + ' balanced slices grouped by ' +
    'sub-folder in dependency order. Return `governance` as the campaign governance surface: the package README.md and ' +
    'ARCHITECTURE.md, the .csproj or language manifest rows for this package, the .api folder path, and ' + c.doc + '.',
    { label: 'plan:' + tag, phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN, stallMs: STALL })
  const slices = ((plan && plan.slices) || []).filter((s) => s && s.length)
  const gov = (plan && plan.governance) || []
  const verifyTasks = slices.map((pages, i) => () => agent(CTX(c) + '\n\n' + HUNT + '\n\nTASK: HOSTILE READ-ONLY VERIFY, ' +
    'slice ' + i + '. Read ' + c.doc + ' IN FULL — every ruling, page row, band, seam, rider, and acceptance trace that ' +
    'touches your pages. Then read each of these pages IN FULL plus every .api catalog its fences cite: ' +
    JSON.stringify(pages) + '. Verify each ruling landed PROPERLY — integrated as if always designed that way, at the ' +
    'ruled band/signature/charter, frozen names byte-identical — and attack past the ruling: the floor law means a ' +
    'page that merely met its disposition without depth is a naive finding. Return typed anchored findings.',
    { label: 'verify:' + tag + ':s' + i, phase: 'Verify', model: 'opus', effort: 'high', schema: FINDINGS, stallMs: STALL }))
  verifyTasks.push(() => agent(CTX(c) + '\n\n' + HUNT + '\n\nTASK: HOSTILE READ-ONLY GOVERNANCE VERIFY. Read ' + c.doc +
    ' IN FULL, then audit the governance surface end to end: ' + JSON.stringify(gov) + '. Every acceptance trace ' +
    'resolves on disk (page exists, entry carries the ruled signature, seam anchor present); every rider has its landed ' +
    'receipt; the README router/package groups, ARCHITECTURE codemap + seams ledger (canonical [KIND] tags, mirrored ' +
    'endpoints), central manifest rows, and .api anchors agree with the landed page set — a disagreement between any ' +
    'two surfaces is a drift finding. Return typed anchored findings.',
    { label: 'verify:' + tag + ':gov', phase: 'Verify', model: 'opus', effort: 'high', schema: FINDINGS, stallMs: STALL }))
  const found = (await parallel(verifyTasks)).filter(Boolean)
  const all = found.flatMap((f) => f.findings || [])
  log(tag + ': ' + all.length + ' finding(s) from ' + found.length + ' verifier(s)')
  const fix = await agent(CTX(c) + '\n\nTASK: RESOLVE EVERYTHING (WRITER — full authority over ' + c.root + ', its ' +
    'manifest rows, and ' + c.doc + ' where a finding proves the doc itself wrong). For each finding below: re-verify ' +
    'it on disk (a finding without a verifiable anchor is rejected with reason); fix every validated finding IN PLACE ' +
    'at its root — grow the owning page at its bar, repair both ends of a drifted seam, correct the governance surface ' +
    'that lied. The floor law governs every page you touch: exceed the ruling with denser, deeper, more capable form; ' +
    'a single-point patch where a root-level reconstruction is available is itself a defect. Frozen signatures and ' +
    'wire names stay byte-identical. FINDINGS: ' + JSON.stringify(all),
    { label: 'resolve:' + tag, phase: 'Resolve', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL })
  const close = await agent(CTX(c) + '\n\n' + HUNT + '\n\nTASK: COLD ADVERSARIAL CLOSE (WRITER). You run after the ' +
    'resolver; assume it missed things and introduced ripples. Re-read every file it touched — ' +
    JSON.stringify((fix && fix.files) || []) + ' — plus a hostile sample of the untouched corpus and the governance ' +
    'surface. Attack every conformance dimension cold; FIX every residual yourself in place. `clean` is earned only by ' +
    'an attack that finds nothing; `residual` carries only what is genuinely beyond surgical reach, with anchors.',
    { label: 'close:' + tag, phase: 'Close', model: 'fable', effort: 'high', schema: CLOSE, stallMs: STALL })
  return { campaign: c.root, findings: all.length, resolved: (fix && fix.resolved && fix.resolved.length) || 0,
    rejected: (fix && fix.rejected && fix.rejected.length) || 0, verdict: (close && close.verdict) || 'unknown',
    residual: (close && close.residual) || [], summary: (close && close.summary) || '' }
}))
return { campaigns: lanes.filter(Boolean) }
