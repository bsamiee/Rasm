export const meta = {
  name: 'cold-verify',
  whenToUse: 'Campaign closure gate: after a rebuild campaign lands, verify the whole target corpus against its root DECISION/brief and fix every miss in place. args = {doc, root} or an array of such pairs; campaigns verify in parallel lanes. The resolver is the terminal finalizer — findings resolve in-run, never as a report, and no phase follows it.',
  description: 'Cold-verify pass over one or more landed campaigns. Per campaign: one sonnet plan partitions the target folder into balanced verification slices; opus verifiers fan out, each reading the root doc IN FULL plus its slice pages IN FULL, hunting missing/wrong/faked/naive work with typed anchored findings (one verifier owns the governance lane: index docs, manifest rows, csproj/README registries, .api anchors, acceptance traces, rider receipts); ONE terminal fable resolver then finalizes the campaign — verifier findings are SIGNALS, not law: it re-verifies each on disk, implements the strongest fix where a suggestion was weak or short-sighted, hunts and fixes what the verifiers missed on its own authority, resolves every ripple its edits expose, and pushes touched pages past the ruling per the floor law. No phase follows the resolver.',
  phases: [
    { title: 'Plan', detail: 'per campaign: enumerate pages, partition into balanced slices', model: 'sonnet' },
    { title: 'Verify', detail: 'per campaign: opus slice verifiers + one governance verifier, read-only, typed anchored findings', model: 'opus' },
    { title: 'Resolve', detail: 'per campaign: one terminal fable finalizer — findings as signals, own hunt beyond them, every ripple resolved in-run', model: 'fable' },
  ],
}

// --- [CONSTANTS] ---
const SLICES = 4
const STALL = 300000

// --- [INPUTS] ---
// Campaign row: {doc, root, pages?, governance?}. `pages` scopes the lane to an explicit landed
// subset (in-flight sibling pages, index docs, .api, manifests, and the root doc become a WRITE
// FENCE — findings there are recorded residuals, never edits) and skips the plan agent.
// `governance: false` skips the governance verifier (in-flight tails own those surfaces).
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const CAMPS = (Array.isArray(argsIn) ? argsIn : [argsIn]).filter((c) => c && c.doc && c.root)
if (!CAMPS.length) { log('No campaigns — pass {doc, root} or an array of pairs.'); return { campaigns: 0 } }
const chunk = (a, n) => { const o = []; for (let i = 0; i < a.length; i += n) o.push(a.slice(i, i + n)); return o }

// --- [MODELS] ---
const PLAN = { type: 'object', additionalProperties: false, required: ['slices', 'governance'], properties: {
  slices: { type: 'array', items: { type: 'array', items: { type: 'string' } } },
  governance: { type: 'array', items: { type: 'string' } } } }
const FINDINGS = { type: 'object', additionalProperties: false, required: ['findings', 'summary'], properties: {
  findings: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'class', 'finding', 'anchor'], properties: {
    target: { type: 'string' }, class: { type: 'string', enum: ['missing', 'wrong', 'faked', 'naive', 'drift', 'phantom'] },
    finding: { type: 'string' }, anchor: { type: 'string' }, fix: { type: 'string' } } } },
  summary: { type: 'string' } } }
// Required-but-possibly-empty `beyond` is an attestation: the resolver's own hunt ran, not only the signal list.
const FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'resolved', 'beyond', 'rejected', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  resolved: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: {
    target: { type: 'string' }, action: { type: 'string' } } } },
  beyond: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: {
    target: { type: 'string' }, action: { type: 'string' } } } },
  rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['finding', 'reason'], properties: {
    finding: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' } } }

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
  const scoped = Array.isArray(c.pages) && c.pages.length > 0
  const plan = scoped ? null : await agent(CTX(c) + '\n\nTASK: thin enumerate (read-only). List every design page under ' +
    c.root + '/.planning/ (real ls/find, never memory) and partition them into ' + SLICES + ' balanced slices grouped by ' +
    'sub-folder in dependency order. Return `governance` as the campaign governance surface: the package README.md and ' +
    'ARCHITECTURE.md, the .csproj or language manifest rows for this package, the .api folder path, and ' + c.doc + '.',
    { label: 'plan:' + tag, phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN, stallMs: STALL })
  const slices = scoped ? chunk(c.pages, Math.ceil(c.pages.length / SLICES))
    : ((plan && plan.slices) || []).filter((s) => s && s.length)
  const gov = scoped ? [] : ((plan && plan.governance) || [])
  const FENCE = scoped ? 'WRITE FENCE — the campaign is still IN FLIGHT: write ONLY within the scoped pages ' +
    JSON.stringify(c.pages) + '. Index docs, .api catalogs, central manifests, the root doc, and every sibling page ' +
    'outside the scope are FENCED — a validated finding or ripple landing there is RECORDED in `rejected` with reason ' +
    '`fenced` (target + exact fix), never edited; the post-completion pass executes it. A scoped page that reads a ' +
    'doc-frozen vocabulary owned by an in-flight page verifies against the doc\'s frozen contract, never the in-flight ' +
    'page. ' : ''
  const verifyTasks = slices.map((pages, i) => () => agent(CTX(c) + '\n\n' + HUNT + '\n\nTASK: HOSTILE READ-ONLY VERIFY, ' +
    'slice ' + i + '. Read ' + c.doc + ' IN FULL — every ruling, page row, band, seam, rider, and acceptance trace that ' +
    'touches your pages. Then read each of these pages IN FULL plus every .api catalog its fences cite: ' +
    JSON.stringify(pages) + '. Verify each ruling landed PROPERLY — integrated as if always designed that way, at the ' +
    'ruled band/signature/charter, frozen names byte-identical — and attack past the ruling: the floor law means a ' +
    'page that merely met its disposition without depth is a naive finding. Return typed anchored findings.',
    { label: 'verify:' + tag + ':s' + i, phase: 'Verify', model: 'opus', effort: 'high', schema: FINDINGS, stallMs: STALL }))
  if (c.governance !== false && !scoped) verifyTasks.push(() => agent(CTX(c) + '\n\n' + HUNT + '\n\nTASK: HOSTILE READ-ONLY GOVERNANCE VERIFY. Read ' + c.doc +
    ' IN FULL, then audit the governance surface end to end: ' + JSON.stringify(gov) + '. Every acceptance trace ' +
    'resolves on disk (page exists, entry carries the ruled signature, seam anchor present); every rider has its landed ' +
    'receipt; the README router/package groups, ARCHITECTURE codemap + seams ledger (canonical [KIND] tags, mirrored ' +
    'endpoints), central manifest rows, and .api anchors agree with the landed page set — a disagreement between any ' +
    'two surfaces is a drift finding. Return typed anchored findings.',
    { label: 'verify:' + tag + ':gov', phase: 'Verify', model: 'opus', effort: 'high', schema: FINDINGS, stallMs: STALL }))
  const found = (await parallel(verifyTasks)).filter(Boolean)
  const all = found.flatMap((f) => f.findings || [])
  log(tag + ': ' + all.length + ' finding(s) from ' + found.length + ' verifier(s)')
  const fix = await agent(CTX(c) + '\n\n' + HUNT + '\n\n' + FENCE + 'TASK: TERMINAL FINALIZE (WRITER — ' +
    (scoped ? 'write authority scoped by the fence above' : 'full authority over ' + c.root +
    ', its manifest rows, and ' + c.doc + ' where a finding proves the doc itself wrong') + '; you are the run\'s LAST agent, ' +
    'no phase follows you). Read ' + c.doc + ' IN FULL first. The verifier findings below are SIGNALS, not law: do not ' +
    're-litigate a correct finding — re-verify each on disk, then implement the STRONGEST resolution, which is the ' +
    'suggested fix only when that fix is already the root-level form; where a suggestion is weak, short-sighted, or a ' +
    'single-point patch, implement the denser root-level reconstruction instead (a finding without a verifiable anchor ' +
    'is rejected with reason). Then hunt PAST the signal list on your own authority — the hunt classes above over the ' +
    'corpus and governance surface as you work it — and fix what the verifiers missed; `beyond` enumerates those fixes, ' +
    'and an empty `beyond` attests your own hunt found nothing, never that it did not run. Every ripple an edit exposes ' +
    'is YOURS in the same pass — seam counterparts both ends, consumer sites, index docs, manifest rows, .api anchors — ' +
    'within your write authority; a ripple landing on a fenced surface is recorded with its exact fix, never silently ' +
    'dropped. The run ends finalized. The floor law governs every page you touch: exceed the ruling with ' +
    'denser, deeper, more capable form. Frozen signatures and wire names stay byte-identical. ' +
    'FINDINGS: ' + JSON.stringify(all),
    { label: 'resolve:' + tag, phase: 'Resolve', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL })
  return { campaign: c.root, findings: all.length, resolved: (fix && fix.resolved && fix.resolved.length) || 0,
    beyond: (fix && fix.beyond && fix.beyond.length) || 0, rejected: (fix && fix.rejected && fix.rejected.length) || 0,
    summary: (fix && fix.summary) || '' }
}))
return { campaigns: lanes.filter(Boolean) }
