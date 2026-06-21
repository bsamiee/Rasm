export const meta = {
  name: 'rebuild-and-reconcile',
  description: 'Fan out one edit-in-place worker per file, then reconcile the cross-file fixes each worker had to defer: cluster deferrals by shared file, fix each cluster, adversarially verify per claim.',
  whenToUse: 'A refactor where each file is fixed independently but some fixes span files (a shared type, a seam two files must agree on)',
  phases: [{ title: 'Edit' }, { title: 'Reconcile' }],
}

// args = JSON array of file paths. The defaults keep the example runnable as-is.
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const FILES = Array.isArray(input) && input.length ? input : ['src/a.ts', 'src/b.ts', 'src/c.ts']

const EDIT = { type: 'object', additionalProperties: false, required: ['file', 'verdict'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['edited', 'clean'] }, residual: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } } } }
const FIXED = { type: 'object', additionalProperties: false, required: ['files', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const VERIFY = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'resolved'], properties: { claim: { type: 'string' }, resolved: { type: 'boolean' }, evidence: { type: 'string' } } } } } }

// Steady worker pool — better than parallel(thunks) for many long chains (pattern #11).
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let i = 0
  const run = async () => { while (i < items.length) { const k = i++; out[k] = await worker(items[k], k) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, run))
  return out
}

// --- Edit: one worker per file; fix what it can alone, DEFER cross-file work as DATA (a file LIST). ---
phase('Edit')
const done = (await pool(FILES, 8, (f) => agent(
  'Refactor ' + f + ' in place. Fix everything you can within this ONE file. Anything that also requires editing OTHER files (a shared type, a seam two files must agree on) you MUST NOT touch here — instead report it in residual as {files: [every file the fix touches], claim}.',
  { label: 'edit:' + f, phase: 'Edit', schema: EDIT }))).filter(Boolean)

// --- Reconcile: BARRIER (dedup + cluster by shared file via union-find), then PIPELINE fix -> verify. ---
const all = done.flatMap((d) => d.residual || [])
const uniq = [...new Map(all.map((r) => [r.files.join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map()
  const find = (x) => { let p = x; while (parent.get(p) !== p) p = parent.get(p); return p }
  const add = (x) => { if (!parent.has(x)) parent.set(x, x) }
  for (const r of uniq) { r.files.forEach(add); for (let j = 1; j < r.files.length; j++) parent.set(find(r.files[j]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
log('Reconcile: ' + uniq.length + ' deferrals -> ' + clusters.length + ' disjoint clusters')

let hard = []
if (clusters.length) {
  phase('Reconcile')
  // Disjoint clusters write non-overlapping files, so per-cluster fixers run concurrently with no collision (no worktree).
  const out = (await pipeline(
    clusters,
    (cl) => agent('Fix these cross-file deferrals in place. Read EVERY listed file; make the shared fix once, consistently, regress nothing.\n' + JSON.stringify(cl, null, 1), { label: 'fix', phase: 'Reconcile', schema: FIXED }),
    (fix, cl, i) => fix ? agent('Adversarially verify each claim is ACTUALLY resolved — read the named files from disk, default resolved=false on any doubt. Return one verdict per claim.\nClaims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files), { label: 'verify:' + i, phase: 'Reconcile', schema: VERIFY }).then((v) => ({ cl, v })) : null,
  )).filter(Boolean)
  // A separate verifier with one-verdict-per-claim PROVES completeness: a dropped claim cannot validate.
  hard = out.flatMap((o) => ((o.v && o.v.claims) || []).filter((c) => !c.resolved).map((c) => c.claim))
}
return { files: FILES.length, clusters: clusters.length, hard_residual: hard }
