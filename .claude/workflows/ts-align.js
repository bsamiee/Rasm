export const meta = {
  name: 'ts-align',
  description: 'One lean pass combining the full-TS cross-folder reconcile/alignment/smoothing with the csharp/python seam smoothing. Census (4 parallel opus agents: intra-TS seam+duplication sweep over the folder halves, cross-language seam verification both-endpoint, catalog/allocation/doctrine drift) -> pure-JS merge/dedup/union-find clustering into at most 6 disjoint buckets -> Fix (parallel fable max writers, one per bucket) -> Verify (1 terminal fable xhigh adversarial verifier, repair-in-place, skipped when no fixer changed a file). Accepts the ts-buildout residual list via args and merges it with its own census. About 11 agents, peak concurrency 6. args = optional {residuals: [{files, claim}]} or a bare residual array; empty = full-branch census. Ephemeral: delete after the alignment lands.',
  whenToUse: 'Campaign stage 6-7, after the folder build-out: smooth every cross-folder seam inside libs/typescript and every cross-language seam to csharp/python in one pass, consuming the build-out deferrals.',
  phases: [
    { title: 'Census', detail: '4 parallel sweeps: TS folder halves (seams, duplication, vocabulary drift), cross-language rows both-endpoint, catalog/allocation/doctrine drift' },
    { title: 'Fix', detail: 'union-find clusters packed into at most 6 disjoint buckets, one fable max writer each' },
    { title: 'Verify', detail: '1 terminal xhigh adversarial verifier over every claim, repairing in place; unreachable claims returned as hard residuals' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const STALL = 480000
const TS = 'libs/typescript'
const TSD = 'docs/stacks/typescript'
const BLUEPRINT = 'libs/typescript/BLUEPRINT.md'
const DECISION = 'RASM-TS-PLATFORM-DECISION.md'
const MAX_BUCKETS = 6

// --- [INPUTS] ----------------------------------------------------------------------------
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = (typeof args === 'string' && /^\s*[\[{]/.test(args)) ? JSON.parse(args) : args
const seedResiduals = (Array.isArray(argsIn) ? argsIn
  : (argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.residuals)) ? argsIn.residuals
  : []).filter((r) => r && Array.isArray(r.files) && r.files.length && typeof r.claim === 'string' && r.claim.trim())

// --- [MODELS] ----------------------------------------------------------------------------
const CENSUS = { type: 'object', additionalProperties: false, required: ['residual'], properties: {
  residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: {
    files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }
const FIXED = { type: 'object', additionalProperties: false, required: ['files'], properties: {
  files: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const VERIFY = { type: 'object', additionalProperties: false, required: ['claims'], properties: {
  claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'resolved'], properties: {
    claim: { type: 'string' }, resolved: { type: 'boolean' }, note: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = 'Rasm monorepo, TS catch-up campaign alignment pass. The ' + TS + ' folders carry realized design pages built against ' + DECISION +
  ' + ' + BLUEPRINT + ' at the ' + TSD + ' doctrine bar. THIS PASS smooths what per-folder builds cannot: cross-folder seams inside the ' +
  'branch, cross-language seams to csharp/python, and branch-wide consistency - fixing at the ROOT (a dense reconstruction of the touched ' +
  'cluster beats a point patch) without re-litigating the settled 14-folder roster or any DECISION verdict. LAWS: one canonical name + one ' +
  'owner per shared concept; every seam recorded on BOTH endpoint files with mirrored glyphs; C# owns the wire vocabulary, XxHash128 ' +
  'content-key parity is bit-identical, TS consumes GLB and owns no geometry; the edge ledger of ' + DECISION + ' governs every import claim; ' +
  'fences stay transcription-complete with .api-verified members only (unverifiable = RESEARCH item); IDEAS/TASKLOG are empty placeholders - ' +
  'never open or fill them; zero meta/provenance in any artifact. A residual is DATA: {files: [every real endpoint file], claim}.'

// --- [OPERATIONS] ------------------------------------------------------------------------
const censusPrompt = (scope) => [LAW, '', 'TASK: READ-ONLY CENSUS. ' + scope, '', 'Return residual: every defect as {files (every endpoint ' +
  'file that must change - list ALL of them), claim (the defect + the canonical resolution direction, decisive, one line)}. Real findings ' +
  'only - a claim you cannot anchor to read evidence is not returned.'].join('\n')
const CENSUS_LANES = [
  { lane: 'intra-a', scope: 'Read the build-order section of ' + BLUEPRINT + ', take the FIRST HALF of the folder order. For each folder: ' +
    'ARCHITECTURE.md seam ledger + every design page fence. Sweep: every cross-folder reference verified against the counterpart page (both ' +
    'directions - declared-unwired AND wired-undeclared); duplicate mechanisms across folders (two folders minting one concept); vocabulary ' +
    'drift (one concept, two spellings); edge-ledger violations in import claims.' },
  { lane: 'intra-b', scope: 'Same sweep as intra-a over the SECOND HALF of the ' + BLUEPRINT + ' folder order, plus the branch docs: ' + TS +
    '/.planning/{README,ARCHITECTURE}.md codemap/seam tables verified against folder reality.' },
  { lane: 'cross-lang', scope: 'The cross-language surface: every seam row in the ' + DECISION + ' re-mirror map and in every TS folder ' +
    'ARCHITECTURE verified BOTH-ENDPOINT on disk (the csharp file must carry the mirrored row verbatim; the TS owner must exist); the wire ' +
    'law audited everywhere (C# wire vocabulary consumed never re-minted, XxHash128 parity, GLB rail, graduation evidence); ' +
    'libs/.planning/architecture.md + planning-targets.md rows involving typescript verified current.' },
  { lane: 'catalog', scope: 'Branch-wide truth surfaces: pnpm-workspace.yaml catalog <-> folder README package registries <-> BOTH .api tiers ' +
    '(every claimed package has its catalogue at the right tier, no orphan catalogues, no tier duplication) <-> the ' + BLUEPRINT +
    ' allocation section; doctrine-conformance drift (pages contradicting a ' + TSD + ' law - sample 3 pages per folder, prioritize fence-' +
    'bearing clusters).' },
]
const fixPrompt = (cluster) => [LAW, '', 'TASK: FIX this disjoint residual cluster IN PLACE - read every listed file in full first, then ' +
  'repair at the ROOT: reconstruct the touched clusters densely rather than point-patching; seams land on both endpoints with mirrored ' +
  'glyphs; vocabulary unifies to the canonical owner; capability is conserved. ' +
  'A concurrent sibling may share a page with your cluster (oversized components shard file-atomically): edit any potentially shared page with ' +
  'surgical anchored Edits only — re-read and re-apply on an edit conflict, never a whole-file rewrite. ' +
  'THE CLUSTER:\n' + JSON.stringify(cluster, null, 1) +
  '\nReturn files (every file you actually changed) + a one-line summary.'].join('\n')
const verifyPrompt = (rows, touched) => [LAW, '', 'TASK: TERMINAL ADVERSARIAL VERIFY - you WRITE. For EVERY claim below: re-derive whether ' +
  'the fix was necessary, prove ON DISK it was done properly at both endpoints, and IMPROVE the solution to the root where a stronger ' +
  'reconstruction is available (a surviving point-patch is itself a defect you repair). One verdict per claim - a dropped claim cannot ' +
  'validate. Files the fixers touched: ' + JSON.stringify(touched) + '. THE CLAIMS:\n' + JSON.stringify(rows, null, 1) +
  '\nReturn claims: [{claim, resolved, note}] - resolved=false ONLY when genuinely unreachable from the files at hand, with the blocking ' +
  'reason in note.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Census')
const censused = (await parallel(CENSUS_LANES.map((l) => () =>
  agent(censusPrompt(l.scope), { label: 'census:' + l.lane, phase: 'Census', model: 'opus', effort: 'max', schema: CENSUS, stallMs: STALL })))).filter(Boolean)
const all = [...seedResiduals, ...censused.flatMap((c) => c.residual || [])]
  .filter((r) => r && Array.isArray(r.files) && r.files.length && r.claim)
const uniq = [...new Map(all.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
log('Census: ' + censused.length + '/4 lanes; ' + seedResiduals.length + ' seeded + ' + (all.length - seedResiduals.length) + ' found -> ' + uniq.length + ' unique residual(s)')
if (!uniq.length) { log('Nothing to align - the branch is smooth.'); return { residuals: 0, fixed: 0, hard: [] } }

// Union-find by shared file, then pack clusters into at most MAX_BUCKETS disjoint buckets.
const parent = {}
const find = (k) => { while (parent[k] !== k) { parent[k] = parent[parent[k]]; k = parent[k] } return k }
const union = (a, b) => { const ra = find(a), rb = find(b); if (ra !== rb) parent[ra] = rb }
uniq.forEach((r, i) => { const key = 'r' + i; parent[key] = key; r.files.forEach((f) => { if (parent[f] === undefined) parent[f] = f; union(key, f) }) })
const groups = {}
uniq.forEach((r, i) => { const root = find('r' + i); (groups[root] = groups[root] || []).push(r) })
// Balance by WORK (distinct files dominate a fixer's load), never count, and CAP atomicity at the fair share: an over-budget
// connected component sub-shards by lead file (same-lead-file rows never split — the edit-collision floor); the terminal
// verifier owns the deliberate cross-shard seams.
const clusterWork = (c) => { const files = new Set(); for (const r of c) for (const f of r.files) files.add(f); return files.size * 2 + c.length }
const clusters = Object.values(groups)
const cap = Math.max(1, Math.ceil(clusters.reduce((w, c) => w + clusterWork(c), 0) / MAX_BUCKETS))
const shards = clusters.flatMap((c) => {
  if (clusterWork(c) <= cap) return [c]
  const byFile = new Map()
  for (const r of c) { const k = r.files[0] || '~'; if (!byFile.has(k)) byFile.set(k, []); byFile.get(k).push(r) }
  const out = []
  for (const g of [...byFile.values()].sort((a, b) => clusterWork(b) - clusterWork(a))) {
    const t = out.find((s) => clusterWork(s.concat(g)) <= cap)
    if (t) t.push(...g); else out.push([...g])
  }
  return out
})
const buckets = Array.from({ length: Math.min(MAX_BUCKETS, shards.length) }, () => ({ work: 0, rows: [] }))
for (const c of shards.slice().sort((a, b) => clusterWork(b) - clusterWork(a))) {
  let mi = 0; for (let i = 1; i < buckets.length; i++) if (buckets[i].work < buckets[mi].work) mi = i
  buckets[mi].rows.push(...c); buckets[mi].work += clusterWork(c)
}
const packed = buckets.filter((b) => b.rows.length).map((b) => b.rows)
log('Clustered: ' + clusters.length + ' cluster(s) -> ' + shards.length + ' shard(s) into ' + packed.length + ' bucket(s); work [' + packed.map(clusterWork).join(', ') + ']')

phase('Fix')
const fixed = (await parallel(packed.map((bucket, i) => () =>
  agent(fixPrompt(bucket), { label: 'fix:' + (i + 1), phase: 'Fix', effort: 'max', schema: FIXED, stallMs: STALL })))).filter(Boolean)
const touched = [...new Set(fixed.flatMap((f) => f.files || []))]
log('Fix: ' + fixed.length + '/' + packed.length + ' buckets; ' + touched.length + ' file(s) changed')

phase('Verify')
let hard = []
if (touched.length) {
  const verdicts = await agent(verifyPrompt(uniq, touched), { label: 'verify', phase: 'Verify', effort: 'xhigh', schema: VERIFY, stallMs: STALL })
  hard = ((verdicts && verdicts.claims) || []).filter((c) => !c.resolved).map((c) => c.claim + (c.note ? ' [' + c.note + ']' : ''))
} else { log('No fixer changed a file - skipping verify; every residual returned as hard.'); hard = uniq.map((r) => r.claim) }
log('Verify: ' + (uniq.length - hard.length) + '/' + uniq.length + ' resolved; ' + hard.length + ' hard residual(s)')

return { residuals: uniq.length, fixed: uniq.length - hard.length, buckets: packed.length, touched: touched.length, hard }
