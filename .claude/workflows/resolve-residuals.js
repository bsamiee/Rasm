export const meta = {
  name: 'resolve-residuals',
  whenToUse: 'Resolve cross-file hard residuals a rebuild run could not close, with research and adversarial verification.',
  description: 'Cross-file HARD-RESIDUAL resolver, language-agnostic, two write stages: cluster residuals by spanned files (union-find, atomic; path-less claims form one atomic cluster; oversized components sub-shard by lead file; LPT work-balanced packing into bounded buckets) -> RESOLVE per bucket (research + decide + edit in one pass: full-read spanned pages, enumerate both .api tiers, assay-verify members, make the strongest principled choice and implement it across every spanned page; one re-attempt for a dead bucket) -> VERIFY per bucket, pipelined with no barrier (adversarially re-derive each claim, prove on disk, repair weak fixes in place, one verdict per claim). args = an array, {residuals:[...]}, or a rebuild-* run\'s {hard_residual:[...]}; items are {files,claim}, {id,claim,hint}, or bare claim strings. Empty = no-op.',
  phases: [
    { title: 'Resolve', detail: 'per work-balanced bucket: research + decide + implement across all spanned pages in one write pass; dead buckets get one re-attempt' },
    { title: 'Verify', detail: 'per bucket the moment its resolve lands: adversarially re-derive, prove on disk, repair in place, one verdict per claim' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const TARGET_WORK = 10

// --- [INPUTS] ----------------------------------------------------------------------------
const input = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const RAW = Array.isArray(input) ? input : (input && Array.isArray(input.residuals)) ? input.residuals : (input && Array.isArray(input.hard_residual)) ? input.hard_residual : []
const rel = (f) => { const i = String(f).indexOf('libs/'); return i > 0 ? String(f).slice(i) : String(f) }
const toResidual = (x, i) => typeof x === 'string' ? { id: 'R' + (i + 1), claim: x, files: [], hint: '' } : { id: x.id || ('R' + (i + 1)), claim: x.claim || '', files: Array.isArray(x.files) ? x.files.filter(Boolean).map(rel) : [], hint: x.hint || '' }
const RESIDUALS = RAW.map(toResidual).filter((r) => r.claim)

// --- [MODELS] ----------------------------------------------------------------------------
const RESOLVE_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'items'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  verdict: { type: 'string', enum: ['resolved', 'partial'] },
  items: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['id', 'decision'], properties: {
    id: { type: 'string' }, decision: { type: 'string' }, files: { type: 'array', items: { type: 'string' } },
    open_question: { type: 'string' } } } },
  summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['claims'], properties: {
  claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['id', 'status'], properties: {
    id: { type: 'string' }, status: { type: 'string', enum: ['resolved', 'open'] }, evidence: { type: 'string' } } } },
  repaired_files: { type: 'array', items: { type: 'string' } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------
const DESIGN_BAR = [
  'Rasm monorepo planning corpus (markdown specs; code fences are the product, no source yet). CLAUDE.md manifest + WORKSPACE_LAW strata govern. ' +
    'LANGUAGE ROUTING — hold every edit to the OWNING language bar of the file you edit, by its root: `libs/csharp/**` -> docs/stacks/csharp/ + ' +
    'the relevant domain/ shard ([Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject] ADT collapse, LanguageExt ' +
    '`Fin`/`Validation`/`Option`/`Eff` rails, two-weave AOP, total generated `Switch`, the `tools/cs-analyzer` doctrine gate); `libs/python/**` -> ' +
    'docs/stacks/python/ (py3.15 PEP 585/604/695, `frozendict` builtin, `TypedDict`/Pydantic payloads, expression `Result`/`Option`, `anyio`, ' +
    '`stamina`, closed-fault `@tagged_union`); `libs/typescript/**` -> docs/stacks/typescript/ (Effect-TS rails, `Schema`-first ' +
    'boundaries, branded/nominal types, exhaustive discriminated unions, zero `any`/`throw`/`enum`). Read the operative pages for the file ' +
    'language before editing; every fence holds that bar as fact.',
  'ULTRA-STACK .api CAPABILITY by the file language — enumerate BOTH tiers IN FULL with a real `ls`/`fd` listing from disk (never memory) and ' +
    'mine them to operator depth: C# uses the per-package `<pkg>/.api/*.md` catalogs + the universal Thinktecture/LanguageExt rails (no central ' +
    'tier; Geometry catalogs live at `libs/csharp/Rasm/.api/`); Python mines the shared `libs/python/.api/*.md` AND the folder ' +
    '`libs/python/<folder>/.api/*.md`; TypeScript mines the shared `libs/typescript/.api/*.md` tier AND the folder ' +
    '`libs/typescript/<folder>/.api/*.md`. An admitted capability the resolution admits but no owner exploits is a defect to close; a cited member ' +
    'no catalog or `assay api` confirms is a phantom to DELETE, never to assert. Maximize the shared/universal rails, never only the folder set.',
  'PROSE + COMMENTS: high-signal design-spec prose per docs/standards/style-guide.md — lead with the controlling contract, one idea per paragraph, ' +
    'no hedges/provenance/process narration; BACKTICK every symbol/type/member/path. Keep canonical `# --- [LABEL]` section dividers; comment only ' +
    'where intent is not obvious; zero low-value comments. Prose that ASSERTS capability the fence does not implement is a defect.',
  'BOUNDARY LAW: keep every package/folder owner in its lane and on its stratum; internal code uses canonical names with mapping only at the edge; ' +
    'respect the workspace strata (depend strictly upward; no host-type leak into a host-neutral owner); do not trample a sibling owner while ' +
    'resolving a residual.',
].join('\n')
const ADVERSARIAL = 'HOSTILE STANCE: assume the residual is REAL and the spanned code is naive, junior, or ILLUSORY until proven otherwise — the ' +
  'burden of proof is on the code, never on you. "Already fine"/"mature"/"good enough" is rejected; treat dense confident-looking code as a prime ' +
  'suspect for hollow/decorative complexity, and disbelieve every claim the page makes about itself until verified against the real domain + the ' +
  'catalogued package surface. NAIVETY is a defect on two orthogonal axes, both intolerable: COVERAGE — the owner models a thin slice of its ' +
  'concept (the obvious three fields where the domain carries fifteen; a two-case family for a twenty-case domain); APPROACH — enumerated ' +
  'hardcoded instances where a parameterized algorithmic owner should generate the space (a fixed roster of styles, patterns, or variants is seed ' +
  'DATA feeding one generator over named parameters, never the mechanism itself). Any repeated structure, parallel spelling, or enumerable family ' +
  'an algebra, table, fold, or generator can own is a collapse target you find yourself — no enumerated signal list is ever the complete set. ' +
  'Resolve at the ROOT: where a single-point patch and a root-level dense reconstruction of the same pages are both available, the reconstruction ' +
  'is the resolution; a token patch is itself a defect.'
const RESIDUAL_LAW = [
  'YOU RESOLVE CROSS-FILE HARD RESIDUALS: each residual is a real, open, cross-FILE defect or deferred decision the per-page rebuild + in-run ' +
    'reconcile could not close. Resolve it PROPERLY and FULLY — add the functionality/feature/axis, unify the ambiguous contract, lock the ' +
    'signature, close the capability gap — across EVERY design page it spans, leaving the corpus internally consistent.',
  'DEFERRED DECISIONS ARE RESOLVED, NOT PUNTED: where a residual is a design decision (a new policy axis, a consumer wiring, a canonical contract, ' +
    'a capability extension), make the STRONGEST principled choice the domain + standards admit (research it; choose the most polymorphic, ' +
    'parameterized, future-ready form) and IMPLEMENT it. Only leave an `open_question` when the choice genuinely requires an operator product ' +
    'decision you cannot ground.',
  'REALIZATION-GATE ITEMS STAY IN THE DESIGN PAGE: where a residual notes an owner is "not yet realized as source", the resolution is to ' +
    'SIGNATURE-LOCK the exact contract on the owning DESIGN PAGE so consumers cite it consistently — NEVER create `.cs`/`.py`/`.ts` source (that ' +
    'is the deferred implement pass).',
  'RESEARCH MANDATE — no guessing: where a claim hinges on a real package/host member, VERIFY it via `uv run python -m tools.assay api` over the ' +
    'right artifact band (host DLLs / NuGet / Python distributions / node_modules; READ tools/assay/README.md FIRST for the api-arm contract and ' +
    'the correct invocation, and for a gated/companion Python band the correct interpreter). Where a claim catalogs missing members, WRITE them ' +
    'into the owning `.api/*.md` catalog — resolutions MAY edit `.api` catalog files as well as design pages. Where a claim hinges on a ' +
    'domain/math contract, research the real convention before deciding.',
  'WRITE-FULLY: every edit you identify you MUST make NOW via Edit/Write; the fix-log REPORTS edits already made. Leave behind only a genuine ' +
    'still-cross-file item or a true operator decision (open_question).',
].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const clusterResiduals = (rows) => {
  const parent = new Map()
  const find = (k) => { while (parent.get(k) !== k) { parent.set(k, parent.get(parent.get(k))); k = parent.get(k) } return k }
  const add = (k) => { if (!parent.has(k)) parent.set(k, k) }
  const anchored = rows.filter((r) => r.files.length)
  for (const r of anchored) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of anchored) { const root = find(r.files[0]); if (!by.has(root)) by.set(root, []); by.get(root).push(r) }
  const clusters = [...by.values()]
  // Path-less resolvers locate targets by search and could collide anywhere — ONE atomic cluster owns them all.
  const loose = rows.filter((r) => !r.files.length)
  if (loose.length) clusters.push(loose)
  return clusters
}
const clusterWork = (cl) => 2 * new Set(cl.flatMap((r) => r.files)).size + cl.length
// Atomicity is BUDGETED at the fair share: an over-budget component sub-shards by lead file (same-lead-file rows never
// split — the edit-collision floor); Verify owns the deliberate cross-shard seams.
const shardOversized = (cs) => {
  const cap = Math.max(TARGET_WORK, Math.ceil(cs.reduce((w, c) => w + clusterWork(c), 0) / CAP))
  return cs.flatMap((c) => {
    if (clusterWork(c) <= cap) return [c]
    const byFile = new Map()
    for (const r of c) { const k = r.files[0] || '~'; if (!byFile.has(k)) byFile.set(k, []); byFile.get(k).push(r) }
    const shards = []
    for (const g of [...byFile.values()].sort((a, b) => clusterWork(b) - clusterWork(a))) {
      const t = shards.find((s) => clusterWork(s.concat(g)) <= cap)
      if (t) t.push(...g); else shards.push([...g])
    }
    return shards
  })
}
const packClusters = (clusters) => {
  const sorted = clusters.slice().sort((a, b) => clusterWork(b) - clusterWork(a))
  const total = sorted.reduce((s, c) => s + clusterWork(c), 0)
  const n = Math.max(1, Math.min(CAP, sorted.length, Math.ceil(total / TARGET_WORK)))
  const buckets = Array.from({ length: n }, () => ({ w: 0, rows: [] }))
  for (const cl of sorted) { const b = buckets.reduce((m, x) => (x.w < m.w ? x : m)); b.w += clusterWork(cl); b.rows.push(...cl) }
  return buckets.filter((b) => b.rows.length).map((b) => b.rows)
}
const resolvePrompt = (rows) => [DESIGN_BAR, '', ADVERSARIAL, '', RESIDUAL_LAW, '', 'TASK (RESOLVE — research + decide + implement in ONE write ' +
  'pass): close every residual below. Per residual: FULL-read every page the claim spans AND the owning folder at large (siblings, the folder ' +
  'ARCHITECTURE seams) — resolve the true spanned-file set against real disk state, never against the claim text alone; enumerate BOTH `.api` ' +
  'tiers for the file language from disk and read every catalog the residual touches; verify every member claim and domain/math contract per ' +
  'the RESEARCH MANDATE; then DECIDE the canonical resolution (the strongest principled design the owning language bar admits) and IMPLEMENT it ' +
  'NOW via Edit/Write across every spanned page, unifying each cross-file contract/seam consistently. A concurrent sibling bucket may share a ' +
  'page with yours only through a deliberate cross-shard seam: edit any potentially shared page with surgical anchored Edits only — re-read and ' +
  're-apply on an edit conflict, never a whole-file rewrite. RESIDUALS:\n' + JSON.stringify(rows.map((r) => ({ id: r.id, claim: r.claim,
  files: r.files, hint: r.hint || undefined })), null, 1) + '\nReturn files (every page edited), verdict (resolved = every residual closed; ' +
  'partial otherwise), items (one per residual: id, decision = the canonical resolution implemented + a hostile weak/strong call on the code as ' +
  'it stood, files = the pages that residual touched, open_question ONLY for a true operator product decision), summary.'].join('\n')
const verifyPrompt = (rows, fix) => [DESIGN_BAR, '', ADVERSARIAL, '', RESIDUAL_LAW, '', 'TASK (WRITING VERIFY — adversarial, never a friendly ' +
  'confirmation; you EDIT): a resolver claims to have closed the residuals below. Per residual: (a) RE-DERIVE from the claim and the pages ' +
  'whether the claimed work was necessary at all; (b) read every spanned page from disk and PROVE the resolution is ACTUALLY done — ' +
  'implemented (not hedged or merely described), correct (math/domain/`.api`-grounded; re-check every cited member against the catalog/`assay ' +
  'api`; a cited member that fails verification is a phantom you DELETE from the page NOW), and CONSISTENT across every page it spans; (c) ' +
  'where the landed fix is loose, weak, token, or naive on either axis, REPAIR it in place NOW via Edit/Write to the objectively-best ' +
  'root-level form of the same files — never punt a strengthenable fix to `open`; (d) only then classify: `resolved` ONLY if genuinely ' +
  'complete, correct, and consistent ON DISK after your edits (cite the on-disk proof in evidence), else `open` with exactly what remains and ' +
  'why it is genuinely unreachable from these files. Default to `open` on doubt. One verdict per residual — a dropped id cannot validate. ' +
  'Prove against disk, never trust the resolver summary.\nRESIDUALS:\n' + JSON.stringify(rows.map((r) => ({ id: r.id, claim: r.claim,
  files: r.files })), null, 1) + '\nRESOLVER LOG:\n' + JSON.stringify({ files: fix.files, verdict: fix.verdict, items: fix.items,
  summary: fix.summary || '' }, null, 1) + '\nList every file you edited in repaired_files.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!RESIDUALS.length) { log('resolve-residuals: no residuals passed (args = array / {residuals:[...]} / a rebuild {hard_residual:[...]}). No-op.'); return { residuals: 0, resolved: [], open: [], failed: [], open_questions: [] } }

const buckets = packClusters(shardOversized(clusterResiduals(RESIDUALS)))
log(RESIDUALS.length + ' residual(s) -> ' + buckets.length + ' balanced bucket(s): [' + buckets.map((b) => b.length).join(', ') + '] claims, work [' +
  buckets.map(clusterWork).join(', ') + ']')

phase('Resolve')
const results = await pipeline(
  buckets,
  async (rows, _, i) => {
    // One bounded re-attempt: a transient agent death must never silently lose a bucket.
    const opts = { label: 'resolve:' + (i + 1) + ' (' + rows.length + ' claims)', phase: 'Resolve', schema: RESOLVE_SCHEMA, effort: 'max', stallMs: 300000 }
    const fix = (await agent(resolvePrompt(rows), opts)) || (await agent(resolvePrompt(rows), { ...opts, label: opts.label + ':retry' }))
    return fix ? { rows, fix } : null
  },
  (r, rows, i) => r ? agent(verifyPrompt(r.rows, r.fix), { label: 'verify:' + (i + 1), phase: 'Verify', schema: VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
    .then((v) => ({ rows: r.rows, fix: r.fix, verify: v })) : null,
)
const done = results.filter(Boolean)
const claims = done.flatMap((r) => (r.verify && r.verify.claims) || [])
const byId = new Map(claims.map((c) => [c.id, c]))
const failed = RESIDUALS.filter((r) => !byId.has(r.id)).map((r) => r.id)
const open = claims.filter((c) => c.status === 'open')
log('Verify: ' + (claims.length - open.length) + '/' + claims.length + ' resolved; ' + open.length + ' open' +
  (failed.length ? '; ' + failed.length + ' UNVERIFIED (bucket died — surfaced, not lost): ' + failed.join(',') : ''))

return {
  residuals: RESIDUALS.length,
  buckets: buckets.length,
  resolved: claims.filter((c) => c.status === 'resolved').map((c) => c.id),
  open: open.map((c) => ({ id: c.id, remains: c.evidence || '' })),
  failed,
  open_questions: done.flatMap((r) => (r.fix.items || []).filter((p) => p.open_question).map((p) => ({ id: p.id, q: p.open_question }))),
}
