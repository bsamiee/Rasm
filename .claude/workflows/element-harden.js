export const meta = {
  name: 'element-harden',
  whenToUse: 'After element-architect-build; adversarial hardening of newly-built Rasm.Element design pages, one folder (or explicit page set) at a time.',
  description: 'Rebuild ALL target pages per-file FIRST, then a lib-wide union-find reconcile, then BATCHED critique -> redteam -> sweep (4 pages/agent) with a full-libs/ seam ripple in the sweep. Stacks BOTH the shared libs/csharp/.api tier AND each folder .api, preserves the section-4-RT architecture invariants. Per ELEMENT-REBUILD-PLAN.md. args = a folder name (Bim), an array of folders, or explicit .planning page paths; empty = all five element folders.',
  phases: [
    { title: 'Discover' },
    { title: 'Rebuild' },
    { title: 'Reconcile' },
    { title: 'Critique' },
    { title: 'Redteam' },
    { title: 'Sweep' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const BATCH = 4
const STAGGER_MS = 1500
const PLAN = 'ELEMENT-REBUILD-PLAN.md'
const ELEMENT_FOLDERS = ['libs/csharp/Rasm.Element', 'libs/csharp/Rasm.Bim', 'libs/csharp/Rasm.Materials', 'libs/csharp/Rasm.Persistence', 'libs/csharp/Rasm.Compute']

// --- [INPUTS] ----------------------------------------------------------------------------

const parsedArgs = typeof args === 'string' ? (() => { try { return JSON.parse(args) } catch { return args } })() : args
const rawArgs = Array.isArray(parsedArgs) ? parsedArgs : (typeof parsedArgs === 'string' ? [parsedArgs] : [])
const cleaned = rawArgs.map((x) => String(x).trim()).filter((x) => x && x.toUpperCase() !== 'ALL')
const isPagePath = (x) => x.indexOf('/.planning/') !== -1 && x.endsWith('.md')
const EXPLICIT_PAGES = cleaned.filter(isPagePath)
const folderNames = cleaned.filter((x) => !isPagePath(x))
const ownerOf = (p) => ELEMENT_FOLDERS.find((r) => p.indexOf(r + '/') === 0)
const FOLDERS = folderNames.length ? ELEMENT_FOLDERS.filter((r) => folderNames.some((w) => r.endsWith(w) || r.endsWith('Rasm.' + w) || r === w))
  : EXPLICIT_PAGES.length ? [...new Set(EXPLICIT_PAGES.map(ownerOf).filter(Boolean))]
  : ELEMENT_FOLDERS

// --- [MODELS] ----------------------------------------------------------------------------

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['pages', 'folders'],
  properties: { pages: { type: 'array', items: { type: 'string' } }, folders: { type: 'array', items: { type: 'string' } } } }
const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const FILE_FIXLOG = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'],
  properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const BATCH_FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'],
  properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['hardened', 'refined', 'clean'] }, extended: { type: 'string' }, aligned: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: the Rasm.Element rebuild. READ ' + PLAN + ' (repo root) in FULL FIRST — section 4-RT (RED-TEAM REVISIONS) is AUTHORITATIVE and OVERRIDES section ' +
    '4A-H on conflict. These are DESIGN-PAGE specs (.planning markdown). The NEW architecture (the Rasm.Element seam, the IElementProjection + IGraphConstraint ' +
    'contracts, the NEUTRAL edge algebra, the typed value vocabulary, Marten-as-append-substrate-beneath-the-CRDT-engine) is CANONICAL — harden TO it, never ' +
    'regress it. CLAUDE.md WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward; the amended strata ' +
    'adds Rasm.Element as the lowest-AEC sub-stratum).',
  'MANDATORY STANDARDS: docs/stacks/csharp/** is the FLOOR (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, ' +
    'system-apis + the relevant domain/ shard) — meet it then PUSH PAST to the strongest form. Cite only members verified via `uv run python -m tools.assay api`.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write; the returned fix-log REPORTS edits already made, never a to-do. Leave nothing ' +
    'behind except genuine cross-FILE items (report those in residual_high).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, redteam, sweep) is HOSTILE: assume the fence is NAIVE/SHALLOW/ILLUSORY until it survives an aggressive attack; ' +
    'the burden of proof is ON THE PAGE. "Mature"/"good enough"/a prior clean verdict are REJECTED. A no-edit verdict is earned ONLY after a genuine attack finds ' +
    'nothing — never a first-read concession, never to avoid work.',
  'ILLUSORY/FAKE CODE is the PRIMARY target — code that USES the doctrine vocabulary ([Union]/[SmartEnum]/[ValueObject]/Fold/the rails) and reads dense yet is ' +
    'HOLLOW: a name/signature/prose promising capability the body lacks; a thin slice of a rich concept (a 2-case union for a 20-case domain); a placeholder/stub ' +
    'dressed as finished; a cited .api/host member never verified (a phantom). Treat dense confident pages with MORE suspicion. Earn every clean verdict.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the named laws of docs/stacks/csharp/README.md held as fact: EXPRESSION_SPINE + BOUNDARY_ADMISSION; SHAPE_BUDGET (one concept owns ONE ' +
    'type, variants are cases) + DEEP_SURFACES + MODAL_ARITY + ANTICIPATORY_COLLAPSE; POLICY_VALUES + DERIVED_LOGIC/TYPES + SEMANTIC_NAMING; LIBRARY_DEPTH + ' +
    'DEFINITION_TIME_ASPECTS; ROOT_REBUILD + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION; INTERFACE_SEAM.',
  'COLLAPSE MANDATE: collapse >=3 parallel types / sibling factories / repeated switch arms / single-call helpers into ONE polymorphic owner IN THE SAME FILE via ' +
    '[Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/source-generated case families/Fold algebra/frozen tables — never extract a new file to ' +
    'reduce LOC, never delete capability.',
  'STACK CAPABILITY (CORRECTED — there IS a central tier now): FIRST mine BOTH the SHARED tier `libs/csharp/.api/**` AND the target folder`s own `.api/**` to full ' +
    'depth, layered with the universal Thinktecture/LanguageExt rails. Compose EVERY relevant host API + admitted NuGet member into single dense owners (generated ' +
    'owners, Fold algebra, data tables), not flat per-API uses; use the DEEPEST surface each package reaches; reject thin wrappers + BCL-first reflexes; verify ' +
    'novel members via `uv run python -m tools.assay api`. (The legacy cs-rebuild workflows wrongly said C# has no central .api tier — it does: libs/csharp/.api/.)',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam): structural collapse and .api-stacking are NECESSARY but NOT SUFFICIENT — a fully-collapsed owner ' +
    'can still model a NAIVE slice of its concept. Close the gap by GROWING the existing owner (a case in the closed family, a row/richer data on the smart-enum, a ' +
    'field/composed value-object, an operation, a policy value) per ROOT_REBUILD + COMPOSED_IMPLEMENTATION — never a parallel surface or a new file. Every extension ' +
    'cites exactly one source: a PACKAGE member, a DOMAIN attribute, or a CONSUMER contract. If the concept is genuinely complete, prove it by adding nothing.',
].join('\n')
const SEAM = [
  'SECTION-4-RT INVARIANTS (the new architecture — preserve, never regress): Relationship is a NEUTRAL edge algebra (Compose|Assign|Associate|Connect|Void + typed ' +
    'payload + Generic passthrough), NOT 17 typed IfcRel cases — the IFC relationship schema lives in Bim`s projector [C5]; PredefinedType is a typed Object field ' +
    'with a Bim egress gate [C6]; the Associate edge carries LayerSet/ProfileSet usage [C7]; MeasureValue uses a Dimension value-object + UnitsNet QuantityType [H2]; ' +
    'ElementGraph carries the incidence index + memoized Bake [H3] with a HAMT working graph vs Frozen read snapshot [H4]; rooted NodeId is a neutral kernel id, IFC ' +
    'GlobalId a Bim attribute [H6]; ONE canonical codec ToCanonicalBytes() on the Node union shared by NodeId-hash + diff [H7]; GeoReference (full tuple) only on ' +
    'Header/Coverage [M1]; RepresentationContentHash keyed map [M2]; TWO seam interfaces — IElementProjection + IGraphConstraint [M3]; Marten is the append substrate ' +
    'BENEATH the preserved op-log/CRDT/time-travel/StructuralMerge engine [H11]; topology is synchronous, AGE/DuckDB are async analytical lanes [C2/H5]. A page that ' +
    'contradicts a 4-RT invariant is a DEFECT to fix toward 4-RT.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely (no weak/erased types); no exception control flow in domain logic (LanguageExt rails/ROP); no imperative branching ' +
    'where a bounded vocabulary/frozen table/generated Switch/Fold owns the variation; no mutable accumulation. Total generated Switch with compile-time ' +
    'exhaustiveness (no runtime-silent _ arm). Typed receipts (no generic IReceipt). Fault = a closed [Union] deriving from Expected.',
  'Latest C# 14 on net10 to the metal (Nullable enable; primary constructors; collection expressions + spread; params collections incl ReadOnlySpan; list/slice/' +
    'relational/logical patterns; required members; file-scoped types; field accessors; extension blocks/operators; generic math / static abstract+virtual interface ' +
    'members; with-expressions; System.Threading.Lock; raw + u8 literals where they fit). Apply the file-organization + section-order law.',
  'BOUNDARY/STRATA: each owner stays on its stratum, depends strictly upward; geometry/mesh/IFC meet at the wire with one owner per runtime; no host-type leak into a ' +
    'host-neutral owner; SEMANTIC_NAMING (one bounded-context term per concept, one word default; no Get/GetMany/GetBy/List/Search families). Each libs/csharp package ' +
    'usable in ISOLATION yet ALIGNED-not-coupled (peers never reference each other; alignment via the Rasm.Element contracts).',
].join('\n')
const PROSE = [
  'PROSE: high-signal design-SPEC prose only. Lead each section with the controlling rule; one idea per paragraph; close on the consequence/boundary. Cut provenance, ' +
    'process narration, freshness disclaimers, report framing, empty hedges. Prose that ASSERTS capability the fence lacks is a defect.',
  'DESIGN-DOC OUTPUT LAW: REAL transcription-complete code fences — ZERO placeholder/stub/TODO, NO page/length cap; densify in place. BACKTICK every symbol/type/' +
    'member/path/package. ("Page craft" is a docs/stacks/csharp doctrine concept only; it does NOT govern these design docs.) COMMENT HYGIENE: keep the canonical ' +
    'section-divider headers; otherwise agent-facing comments only where intent is not obvious — default ZERO; no task/process/history narration.',
].join('\n')
const DOCTRINE = [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', SEAM, '', PATLAW, '', PROSE].join('\n')

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
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }
const base = (p) => p.split('/').pop()
const folderOf = (p) => { for (const r of ELEMENT_FOLDERS) { if (p.indexOf(r + '/') === 0) return r.split('/').pop() } return 'root' }
const inLibs = (p) => typeof p === 'string' && p.startsWith('libs/')
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const batchLabel = (stage, b) => stage + ':' + folderOf(b[0]) + ':' + base(b[0]) + (b.length > 1 ? '+' + (b.length - 1) : '')
const rebuildPrompt = (page) => [DOCTRINE, '', 'TASK: HOSTILE GROUND-UP REBUILD of ' + page + ' to the ULTRA-ADVANCED bar AND domain-complete capability. DISBELIEVE ' +
  'the page — rebuild to the strongest form the doctrine + the section-4-RT invariants admit. Read the page, its sibling pages, ' + PLAN + ' (sections 4/4-RT/5/6), ' +
  'docs/stacks/csharp + the relevant domain/ shard, and BOTH the SHARED libs/csharp/.api/ AND the folder .api/ catalogs it composes; verify every cited member via ' +
  '`uv run python -m tools.assay api`. Collapse parallel shapes into one owner IN THE SAME FILE; close the concept capability gaps in place (each addition citing a ' +
  'package/domain/consumer source); modern C#14/net10, all-backticked high-signal prose, real full code fences. Fix THIS page in place. Report collapsed (before->after) + ' +
  'extended (additions + cited sources); residual_high = each {files,claim} for a CROSS-FILE item only.'].join('\n')
const critiquePrompt = (files) => [DOCTRINE, '', 'TASK: ULTRA-HARSH DOCTRINAL + CAPABILITY-COMPLETENESS AUDIT + FIX IN PLACE of these ' + files.length + ' already-rebuilt pages:\n' +
  files.map((f) => '- ' + f).join('\n') + '\nProcess EACH page independently; assume a violation exists in every fence until proven otherwise; trust nothing the prose claims. ' +
  'Run the mechanical checklists per page and REPAIR every hit IN THAT PAGE: COLLAPSE_SCAN (3+ parallel shapes/sibling names/repeated arms -> one owner); OWNER_CHOOSER (re-derive ' +
  'each shape`s owner from the 5 discriminants); KNOB_TEST (delete each param; collapse flags to policy values/input-shape); ASPECTS (definition-time via source-gen, ' +
  'composition-time via effect transformers); RAILS (narrowest carrier; closed Expected fault; total Switch; no exception control flow); STRATA/MEMBERS/MODERN (depend upward; ' +
  'no phantom member; C#14/net10; full docs/stacks/csharp + domain shard; BOTH .api tiers maximized); CAPABILITY-COMPLETENESS + ILLUSION (a collapsed owner can still be a naive ' +
  'slice — grow it; delete decorative/speculative padding); and the SECTION-4-RT INVARIANTS (a page contradicting one is a defect). Read each page + ' + PLAN + ' + docs/stacks/csharp ' +
  '+ both .api tiers. EDIT each page in place to fix every hit; edit ONLY these ' + files.length + ' pages — anything spanning a file OUTSIDE this set goes to residual_high {files,claim}.'].join('\n')
const redteamPrompt = (files) => [DOCTRINE, '', 'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of these ' + files.length + ' pages — the LAST + most aggressive per-page pass; ' +
  'trust nothing the author/critique claimed:\n' + files.map((f) => '- ' + f).join('\n') + '\nFor EACH page: (A) COUNTERFACTUAL on the core choice — is the owner/algebra/dispatch ' +
  'categorically the strongest the doctrine admits, or does a denser owner / a deeper admitted-package primitive collapse the whole fence? Rebuild to it. (B) ANTICIPATORY_COLLAPSE — ' +
  'does the next case/dimension land as ONE case/row/policy value with every consumer broken LOUDLY at compile time (total Switch, no silent _)? (C) LONG-TAIL — attack every ' +
  'input/output/edge/failure mode; both ingress AND egress parameterized. (D) STRATA/BOUNDARY — no downward dep, no host-type leak, no concern owned twice, geometry/mesh/IFC meet ' +
  'at one wire owner. (E) PHANTOMS/SURFACE-SPRAWL — collapse hand-rolled code the .api tiers already own; delete unverifiable members. (F) CAPABILITY-COMPLETENESS + ILLUSION — ' +
  'attack the owner for domain-completeness independent of how collapsed it looks; grow it in place. (G) SECTION-4-RT FIDELITY — verify the page upholds every 4-RT invariant ' +
  'relevant to it. Repair every defect in place; edit ONLY these ' + files.length + ' pages — cross-file items go to residual_high {files,claim}. If a page`s strongest form is ' +
  'already present, prove it by finding nothing.'].join('\n')
const sweepPrompt = (files, residuals) => [DOCTRINE, '', 'TASK: FINAL SWEEP + FULL-LIBS RIPPLE/SEAM of these ' + files.length + ' pages:\n' + files.map((f) => '- ' + f).join('\n') +
  '\n(1) DEFERRED RESIDUALS — resolve the cross-file residuals below that touch THESE pages, fixing the part that lives in these pages in place (read every listed file for context):\n' +
  (residuals.length ? JSON.stringify(residuals, null, 1) : '(none for this batch)') +
  '\n(2) DRIFT SWEEP — re-read each page; fix any incoherence/drift the rebuild/critique/redteam left (dangling refs, a renamed owner a sibling still cites, a half-applied collapse). ' +
  '(3) FULL-LIBS RIPPLE — for EVERY cross-folder/cross-stack seam these pages expose, align THIS side of the seam in these pages against the shared shape the ENTIRE libs/ stack ' +
  'uses (the other C# folders: libs/csharp/Rasm kernel, Rasm.AppHost, Rasm.AppUi, Rasm.Fabrication, Rasm.Rhino, Rasm.Grasshopper, the AEC peers; AND the libs/python wire — ' +
  'decode-not-remint): IElementProjection/IGraphConstraint <-> projectors; the GeometryRef/RepresentationContentHash/ContentAddress content-key <-> Persistence <-> Rasm kernel; the ' +
  'typed Material/Property/Assessment/Classification wire vocabulary <-> libs/python decoders. READ the counterpart endpoints to learn the correct shared shape, then fix THESE ' +
  'pages` side + mirror the seam in THESE pages` ARCHITECTURE [02]-[SEAMS]. EDIT ONLY these ' + files.length + ' pages (+ their folder ARCHITECTURE/README) — every counterpart-side ' +
  'edit in another folder goes to residual_high {files,claim} so the counterpart folder`s run (or the reconcile) applies it. Return files + verdict + aligned + residual_high.'].join('\n')
const reconcileFix = (cl) => [LAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', SEAM, '', PATLAW, '', 'TASK: RECONCILE these cross-FILE residuals (no severity; every one is ' +
  'must-address). Blast radius is LIB-WIDE — read EVERY listed file across libs/ (csharp + py) and fix the real cross-file defect in place (unify the shared type/seam/contract, ' +
  'repair strata/boundary, extend a shared owner spanning files), preserving all capability; if a residual is factually wrong, leave it and say why. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [LAW, '', SEAM, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — read the named files from disk, classify fixed/invalid/open (default ' +
  'open on doubt). Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles touched: ' + JSON.stringify(fixFiles)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Discover')
const EXCLUDED_PAGE_NAMES = new Set(['README.md', 'ARCHITECTURE.md', 'IDEAS.md', 'TASKLOG.md'])
const isAllowedFolderFile = (p) => typeof p === 'string' && FOLDERS.some((root) => p === root || p.startsWith(root + '/'))
const isDesignPage = (p) => isAllowedFolderFile(p) && p.includes('/.planning/') && p.endsWith('.md') && !EXCLUDED_PAGE_NAMES.has(base(p))
const inv = EXPLICIT_PAGES.length ? null : await agent('Resolve the rebuilt Rasm.Element-campaign design pages to harden. For EACH folder root below, find every design page (repo-relative *.md) ' +
  'under `<root>/.planning/**` at ANY depth; EXCLUDE README.md/ARCHITECTURE.md/IDEAS.md/TASKLOG.md. Use find; do not cd; do not edit. Roots:\n' + JSON.stringify(FOLDERS, null, 1) +
  '\nReturn pages (the union of all design pages) + folders (the roots that had pages).', { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, model: 'sonnet', effort: 'low' })
const pages = [...new Set((EXPLICIT_PAGES.length ? EXPLICIT_PAGES : (inv && inv.pages || [])).filter(isDesignPage))]
log('Discover: ' + (EXPLICIT_PAGES.length ? 'explicit page targeting' : 'folder discovery') + ' -> ' + pages.length + ' page(s) across ' + FOLDERS.length + ' folder(s); CAP=' + CAP + ' BATCH=' + BATCH)
if (!pages.length) { log('No pages — run element-architect-build first, or pass a folder/page subset'); return { folders: FOLDERS, total: 0 } }

// --- [REBUILD]

phase('Rebuild')
const built = (await pool(pages, CAP, (p) => agent(rebuildPrompt(p), { label: 'rebuild:' + folderOf(p) + ':' + base(p), phase: 'Rebuild', schema: FILE_FIXLOG, effort: 'max', stallMs: 300000 }))).filter(Boolean)
log('Rebuild: ' + built.length + '/' + pages.length + ' pages rebuilt')

// --- [RECONCILE]

const norm = (x, page) => { const files = Array.isArray(x.files) ? x.files.filter(inLibs) : []; return { files: files.length ? files : [page], claim: x.claim } }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const r1 = dedup(built.flatMap((r) => (r.residual_high || []).map((x) => norm(x, r.file))))
const clusters = cluster(r1)
log('Reconcile: ' + r1.length + ' rebuild residuals -> ' + clusters.length + ' clusters (serialized, lib-wide)')
phase('Reconcile')
const reconciled = clusters.length ? (await pool(clusters, CAP, async (cl) => {
  const fix = await agent(reconcileFix(cl), { label: 'reconcile-fix', phase: 'Reconcile', schema: FIX_SCHEMA, effort: 'max', stallMs: 300000 })
  if (!fix) return null
  const verify = await agent(reconcileVerify(cl, fix.files), { label: 'reconcile-verify', phase: 'Reconcile', schema: VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
  return { fix, verify }
})).filter(Boolean) : []
const reconcileOpen = reconciled.flatMap((r) => (r.verify && r.verify.claims || []).filter((c) => c.status === 'open').map((c) => c.claim))

// --- [CRITIQUE]

phase('Critique')
const batches = chunk(pages, BATCH)
const crit = (await pool(batches, CAP, (b) => agent(critiquePrompt(b), { label: batchLabel('critique', b), phase: 'Critique', schema: BATCH_FIXLOG, effort: 'xhigh', stallMs: 420000 }))).filter(Boolean)

// --- [REDTEAM]

phase('Redteam')
const red = (await pool(batches, CAP, (b) => agent(redteamPrompt(b), { label: batchLabel('redteam', b), phase: 'Redteam', schema: BATCH_FIXLOG, effort: 'max', stallMs: 420000 }))).filter(Boolean)

// --- [SWEEP]

const critRed = dedup([...crit, ...red].flatMap((r) => (r.residual_high || []).map((x) => norm(x, (r.files && r.files[0]) || pages[0]))))
log('Sweep: ' + batches.length + ' batches; ' + critRed.length + ' critique/redteam residuals threaded to their touching batch')
phase('Sweep')
const swept = (await pool(batches, CAP, (b) => {
  const mine = critRed.filter((x) => x.files.some((f) => b.includes(f)))
  return agent(sweepPrompt(b, mine), { label: batchLabel('sweep', b), phase: 'Sweep', schema: BATCH_FIXLOG, effort: 'max', stallMs: 420000 })
})).filter(Boolean)

// counterpart-side + untouched-batch residuals reach the human / the next folder run / a later reconcile
const sweepResidual = dedup(swept.flatMap((r) => (r.residual_high || []).map((x) => norm(x, (r.files && r.files[0]) || pages[0]))))
const unaddressed = critRed.filter((x) => !x.files.some((f) => batches.some((b) => b.includes(f))))

return {
  folders: FOLDERS, pages: pages.length, rebuilt: built.length,
  reconcileClusters: clusters.length, reconcileOpen,
  critiqueBatches: crit.length, redteamBatches: red.length, sweepBatches: swept.length,
  openResidual: dedup([...sweepResidual, ...unaddressed]).map((x) => ({ files: x.files, claim: x.claim })),
}
