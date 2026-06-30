export const meta = {
  name: 'bim-residual-triage',
  whenToUse: 'One-off: triage the 301 parked Bim reconcile residuals down to the TRUE unfixed set before the Bim fix step.',
  description: 'Deep re-validation of the 301 parked Bim residuals (logged under the OLD per-page harden, before WF-2 reconcile + the Bim/Compute ripple + the Element harden lib-wide reconcile + the element-residual-fix WF all touched Bim, and before the .planning sub-folders were renamed lowercase). 2 investigators split Bim by sub-area, read the parked list from disk, and re-classify EACH residual vs the CURRENT Bim pages + the settled seam contract: REAL (genuine unfixed defect, with current file:line evidence) / RESOLVED (disk already reflects the fix) / STALE (path/owner/structure gone). Then 1 adversarial judge critique/verifies the REAL candidates (default-dismiss on doubt — never waste the fix step on phantom/stale claims), dedups, and WRITES the true residual set to scratchpad/bim-residuals-triaged.json. Read-only except the judge writing the triage output. Takes no args.',
  phases: [
    { title: 'Investigate' },
    { title: 'Judge' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const STALL = 600000
const PLAN = 'ELEMENT-REBUILD-PLAN.md'
const SCRATCH = '/private/tmp/claude-501/-Users-bardiasamiee-Documents-99-Github-Rasm/44e213c2-1533-4e4d-90bd-be5de78fcee0/scratchpad'
const PARKED = SCRATCH + '/bim-residuals.json'
const TRIAGED = SCRATCH + '/bim-residuals-triaged.json'
const AREAS = [
  { name: 'exchange+model+projection', scope: 'residuals touching pages under `libs/csharp/Rasm.Bim/.planning/exchange/` (export, import, format, tessellation, reconstruct, wire), `model/` (elements, faults, query, structural, structure, systems, zones), and `Projection/` (semantic)' },
  { name: 'semantics+review+planning', scope: 'residuals touching pages under `libs/csharp/Rasm.Bim/.planning/semantics/` (appearance, classification, composition, connection, georeference, geospatial, properties), `review/` (coordination, diff, issues, validation, versioning), and `planning/` (cost, schedule)' },
]

// --- [MODELS] ----------------------------------------------------------------------------

const REAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim', 'evidence'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' }, evidence: { type: 'string' } } } }
const INVESTIGATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['area', 'real', 'resolved', 'stale', 'summary'], properties: { area: { type: 'string' }, real: REAL, resolved: { type: 'integer' }, stale: { type: 'integer' }, summary: { type: 'string' } } }
const JUDGE_SCHEMA = { type: 'object', additionalProperties: false, required: ['trueCount', 'dismissed', 'residuals', 'summary'], properties: { trueCount: { type: 'integer' }, dismissed: { type: 'integer' }, residuals: REAL, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CONTEXT: the Rasm.Element rebuild campaign. ' + PARKED + ' holds 301 PARKED Bim reconcile residuals (each {files, claim}) logged when Rasm.Bim was hardened with the OLD per-page 3x flow and PARKED. ' +
    'SINCE THEN, Bim was touched by: WF-2 reconcile, the Bim+Compute harden ripple, the Element harden lib-wide reconcile, AND the element-residual-fix WF (19 Bim files in its last commit); the `.planning` sub-folders were also RENAMED to lowercase (the json paths use old Capitalized folders). So MANY of the 301 are now RESOLVED (the fix already landed) or STALE (the path/owner/structure they cite no longer exists). The settled seam architecture (ELEMENT-REBUILD-PLAN.md section 4-RT — the Rasm.Element seam, neutral edge algebra, 4-arg MeasureValue, two seam interfaces, etc.) is AUTHORITATIVE.',
  'This is a TRIAGE/REVIEW, not a fix — produce the TRUE unfixed residual set so the later Bim fix step does not waste effort on phantom/stale claims. INVESTIGATE DEEPLY: never trust a parked claim at face value — re-read the CURRENT Bim page(s) on disk (match by page NAME, case-insensitive; resolve the lowercase folder rename), the settled Rasm.Element seam pages it references, and docs/stacks/csharp, then judge whether the defect STILL EXISTS on disk TODAY.',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------

const investigatePrompt = (a) => [LAW, '', 'TASK: TRIAGE the parked Bim residuals for the `' + a.name + '` sub-area. (1) READ ' + PARKED + ' (the 301-item JSON array {files, claim}). (2) FILTER to ' + a.scope +
  ' — match each residual to a CURRENT page by NAME (case-insensitive; the json paths use OLD Capitalized folders, the tree is lowercase). (3) For EACH filtered residual, RE-VALIDATE against the CURRENT disk: ' +
  'read the current Bim page(s), the Rasm.Element seam page(s) it references, ' + PLAN + ' (section 4-RT), and docs/stacks/csharp, then classify — REAL (the defect GENUINELY still exists on disk today; capture current file:line EVIDENCE), ' +
  'RESOLVED (the current disk already reflects the fix; cite where), or STALE (the cited path/owner/structure no longer exists, e.g. a deleted page, a renamed owner, a contract the seam already changed). Be rigorous: a compile-break claim is RESOLVED only if the current code genuinely compiles against the settled seam; a rename/anchor claim is STALE if the target was already repointed. READ-ONLY — do NOT edit any file. Return ONLY genuine REAL residuals in `real` (each {files (current paths), claim, evidence}) + the resolved/stale COUNTS + a summary of the dominant resolution reason.'].join('\n')
const judgePrompt = (candidates) => [LAW, '', 'TASK: ADVERSARIAL JUDGE + critique/verify of the Bim residual triage. The two investigators returned these REAL candidates (already filtered from the 301). For EACH candidate, RE-VERIFY against the ' +
  'CURRENT Bim disk that it is a GENUINE unfixed defect — re-read the cited current page(s) + the seam contract; be SKEPTICAL and DEFAULT TO DISMISS on any doubt that the defect still exists (the cost of a false-positive is wasted fix effort + churn on already-correct pages, so the bar for "true" is HIGH). Dedup candidates that name the same defect. Also do a quick scan for any GENUINE defect an investigator may have wrongly dismissed (only add one with hard current evidence). Then WRITE the TRUE residual set as a JSON array of {files (current repo-relative paths), claim, evidence} to ' + TRIAGED + ' (use the Write tool; this is the ONLY write you make). Return trueCount + dismissed (how many candidates you rejected as resolved/stale/duplicate) + residuals (the true set) + a summary. CANDIDATES:\n' + JSON.stringify(candidates, null, 1)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('bim-residual-triage: re-validating 301 parked Bim residuals -> true unfixed set (2 investigators by sub-area -> 1 adversarial judge)')

phase('Investigate')
const inv = (await parallel(AREAS.map((a) => () => agent(investigatePrompt(a), { label: 'investigate:' + a.name, phase: 'Investigate', schema: INVESTIGATE_SCHEMA, effort: 'high', stallMs: STALL })))).filter(Boolean)
const candidates = inv.flatMap((r) => r.real || [])
const resolved = inv.reduce((n, r) => n + (r.resolved || 0), 0)
const stale = inv.reduce((n, r) => n + (r.stale || 0), 0)
log('Investigate: ' + candidates.length + ' REAL candidate(s); ' + resolved + ' resolved, ' + stale + ' stale (of 301)')

phase('Judge')
const judge = candidates.length
  ? await agent(judgePrompt(candidates), { label: 'judge', phase: 'Judge', schema: JUDGE_SCHEMA, effort: 'max', stallMs: STALL })
  : { trueCount: 0, dismissed: 0, residuals: [], summary: 'No real candidates survived investigation — all 301 parked residuals are resolved or stale.' }
log('Judge: ' + (judge.trueCount || 0) + ' TRUE residual(s) of 301 (' + (judge.dismissed || 0) + ' dismissed by the judge); written to ' + TRIAGED)

return {
  workflow: 'bim-residual-triage', parked: 301,
  investigated: inv.map((r) => ({ area: r.area, real: (r.real || []).length, resolved: r.resolved, stale: r.stale })),
  realCandidates: candidates.length, trueCount: judge.trueCount, dismissedByJudge: judge.dismissed,
  trueResiduals: (judge.residuals || []).map((r) => ({ files: r.files, claim: r.claim })),
  triagedOut: TRIAGED,
}
