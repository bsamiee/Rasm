export const meta = {
  name: 'sibling-ripple',
  description: 'WF-3 sibling-ripple — re-bind the named sibling sites (Compute Analysis, Persistence Element, the Fabrication seam rows, the python and typescript wire decoders) to the Component/detail-schema wire WF-1 + WF-2 landed: a thin per-site ripple-discover, a single validate barrier that drops phantom ripples, an in-place decode-not-remint rebind, then a no-defer reconcile fix-verify loop plus a sanity drive-to-zero. Re-opens no Element/Materials/Bim design.',
  whenToUse: 'After WF-1 (element-component) + WF-2 (bim-rebuild) land the Element/Materials/Bim changes; re-bind only the sibling consumer sites to the changed seam.',
  phases: [
    { title: 'Discover' },
    { title: 'Validate' },
    { title: 'Rebind' },
    { title: 'Resolve' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const STALL = 600000
const MAX_ROUNDS = 6
const SANITY_CAP = 6
const SCOPE = 'RASM-REBUILD-SCOPE.md'
const STACK = {
  cs: 'docs/stacks/csharp/** (README + language + shapes + surfaces-and-dispatch + ' +
    'rails-and-effects + boundaries + algorithms + system-apis + the relevant domain/ shard)',
  py: 'docs/stacks/python/** (the core pages + the relevant domain/ shard)',
  ts: 'docs/stacks/typescript/** (the coding-ts route: core pages + the relevant domain shard)',
}
const SITES = [
  { name: 'Compute/Analysis', root: 'libs/csharp/Rasm.Compute/.planning/Analysis', lang: 'cs',
    bind: 're-bind the analysis consumers to the changed seam; honor scope [1].4 resolution-through-Component / ' +
      'type-resolved accessors and do NOT silently break Compute Op-free graph.SectionOf(member)' },
  { name: 'Persistence/Element', root: 'libs/csharp/Rasm.Persistence/.planning/Element', lang: 'cs',
    bind: 're-bind to the changed seam / Type-node (ObjectKind.Type) shape; surface the Type-node persistence ' +
      'per scope [3] M4 without re-opening the egress-owner decision' },
  { name: 'Fabrication/seam-rows', root: 'libs/csharp/Rasm.Fabrication/.planning', lang: 'cs',
    bind: 're-bind the Fabrication seam rows per scope [5]: the Construction/nesting consumer -> Fabrication/Nesting ' +
      'and the Properties consumer -> Fabrication/Process' },
  { name: 'python/data/decode', root: 'libs/python/data/.planning', lang: 'py',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the data decoders' },
  { name: 'python/geometry/mesh', root: 'libs/python/geometry/.planning/mesh', lang: 'py',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the mesh decoders' },
  { name: 'typescript/interchange/codec', root: 'libs/typescript/interchange/.planning/Codec', lang: 'ts',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the interchange codec' },
]

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const RIPPLE = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['detail'], properties: { detail: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['site', 'ripples', 'summary'], properties: { site: { type: 'string' }, ripples: RIPPLE, summary: { type: 'string' } } }
const VALIDATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['validated', 'summary'], properties: { validated: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['site', 'ripples'], properties: { site: { type: 'string' }, ripples: RIPPLE } } }, dropped: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const FOLDER_FIXLOG = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['closed', 'hardened', 'refined', 'clean'] }, integrated: { type: 'array', items: { type: 'string' } }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SANITY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'items'], properties: { overall: { type: 'boolean' }, items: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'open'] }, evidence: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: the Rasm Materials/Bim/Element rebuild — WF-3 sibling-ripple. READ `' + SCOPE + '` (repo root) in ' +
    'FULL FIRST; a prompt POINTS to a scope section by number and never restates scope.',
  'CROSS-WORKFLOW CONTRACT: WF-1 (element-component) + WF-2 (bim-rebuild) ALREADY landed the Element/Materials/Bim ' +
    'design changes AND the seam-mirror rows; WF-3 ONLY re-binds the named sibling consumer sites to the changed ' +
    'Component/detail-schema wire and records each site`s seam row in its OWN `ARCHITECTURE.md` `[2]-[SEAMS]` where ' +
    'applicable. Do NOT re-open Element/Materials/Bim design here.',
  'These are DESIGN-PAGE specs (`.planning` markdown) — code fences ARE the product (transcription-complete, ' +
    'decompile-verified, implementation-ready).',
].join('\n')
const DUAL = [
  'DUAL-AXIS READ (scope `[6]`): every implementing stage reads BOTH axes and finalizes a page only when a cold ' +
    'read surfaces nothing. CODE doctrine — the per-site language stack pages by name (the injected `STACK` ' +
    'pointer). DOC-CRAFT — `libs/.planning/README.md` `[PLANNING_STANDARD]` (the doc-set, the four index templates, ' +
    'the design-page grammar, the `page#CLUSTER` integration notation) + its `[06]` cold-grade REVIEW gate (the ' +
    'doc-finalization gate); the three `docs/standards/` form standards + `docs/standards/proof.md` claim discipline.',
  'BANNED HEDGES (scope `[6]`, word-boundary, page-wide): should/could/would/might/maybe/perhaps/likely/probably/' +
    'propose/consider/recommended/ideally/TBD/TODO/FIXME/we/our/you + the synonym forms (is expected to, can be, ' +
    'aims to, is designed to, in the future, eventually, as needed, if necessary); future tense is legal only on a ' +
    'card growth line + a RESEARCH item.',
  'ZERO-PROVENANCE (scope `[6]`): no reader address, narration, process, source provenance, source-mining history, ' +
    'freshness disclaimers, checklist tails — and on a design page no links, URLs, versions, dates, or session context.',
].join('\n')
const API = [
  'TWO-TIER API (scope `[7]`): cite ONLY the `.api/` catalogs the concept composes — the SHARED substrate tier AND ' +
    'the folder tier — never noise. Members are verified-local truth via `uv run python -m tools.assay api` (a ' +
    'member you cannot verify is a PHANTOM — drop it).',
  'Live NuGet feed routes through the nuget MCP (ToolSearch-load `mcp__nuget__get_latest_package_version` + ' +
    '`mcp__nuget__get_package_context` by exact name before use); internal/library API shape resolves through ' +
    'Context7 (ToolSearch-load `mcp__plugin_context7_context7__resolve-library-id` + ' +
    '`mcp__plugin_context7_context7__query-docs`). Verified-local wins on conflict.',
].join('\n')
const DECODE = [
  'DECODE-NOT-REMINT (the WF-3 KEY LAW, scope `[1].5` + `[5]`): C# owns the wire vocabulary; the python/ts decoders ' +
    'RE-BIND to the changed Component/detail-schema wire and NEVER re-mint the C# contract. The neutral detail ' +
    'schema is Element-declared over `PropertyBag` + the canonical `PropertyName` vocabulary (scope `[1].5`, ' +
    'NEUTRAL — no `Pset_*` IFC names in the seam).',
  '`ProfileRef`/`ProfileSet`/`ComputedSection` STAY seam-canonical (scope `[5]`, the M7 one-hop resolution) — ' +
    '`Component` composes them unchanged and the semantic-rename STOPS at the Materials folder boundary.',
  'C#-SIBLING RE-BIND (scope `[1].4`): the analysis/persistence consumers resolve through the Component ' +
    '(resolution-through-Component / type-resolved accessors) WITHOUT silently breaking Compute Op-free ' +
    '`graph.SectionOf(member)`; the Type-node (`ObjectKind.Type`) shape and the named Bake type->occurrence ' +
    'inheritance are the seam WF-1 landed, consumed here, not re-authored.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE: every implementing stage is HOSTILE — assume the page is NAIVE/STALE/ILLUSORY until it ' +
    'survives an aggressive attack; the burden of proof is ON THE PAGE. A confident edit that does not TRULY ' +
    're-bind to the changed wire is INCOMPLETE, never done.',
  'WRITE-FULLY: every re-bind you identify you MAKE NOW via Edit/Write; the returned log REPORTS edits already ' +
    'made, never a to-do. A re-bind spanning a FILE you do not own goes to `residual_high` as `{files:[...], ' +
    'claim}` (the resource slot is a LIST so a cross-file seam names EVERY spanned file) for the terminal RESOLVE, ' +
    'which has NO scope cap.',
].join('\n')
const DOCTRINE = [LAW, '', DUAL, '', API, '', DECODE, '', ADVERSARIAL].join('\n')

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
const inLibs = (p) => typeof p === 'string' && (p.startsWith('libs/') || p.indexOf('/libs/') !== -1)
const norm = (x, fallback) => { const files = Array.isArray(x.files) ? x.files.filter(inLibs) : []; return { files: files.length ? files : [fallback], claim: x.claim } }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const SITE_BY = new Map(SITES.map((s) => [s.name, s]))
const discoverPrompt = (s) => [DOCTRINE, '', 'TASK: RIPPLE DISCOVERY for the sibling site `' + s.name + '` (folder `' + s.root + '/**`, CODE stack `' + STACK[s.lang] + '`). ' +
  'READ-ONLY — investigate, do NOT edit. Re-read `' + SCOPE + '` then EVERY page under `' + s.root + '/**` AND the CHANGED seam shape WF-1+WF-2 landed (the Element `Graph`/`Component` ' +
  'seam, the one seam-declared NEUTRAL detail schema over `PropertyBag` + `PropertyName`, the `ObjectKind.Type` ' + 'Type-node + named Bake inheritance). SITE RE-BIND CHARTER: ' + s.bind + '. ' +
  'Return the PRECISE ripple work-list for this site ONLY — which seam signatures / decoders shifted and what each page MUST ' + 'change to re-bind (decode-not-remint for the python/ts ' +
  'decoders; resolution-through-Component for the C# consumers). Each ' + 'ripple is {detail, files} carrying the spanned files. Return site + ripples + a summary of the dominant ripple class.'].join('\n')
const validatePrompt = (discovered) => [DOCTRINE, '', 'TASK: VALIDATE the discovered ripple across ALL sites (single barrier pass). READ-ONLY — investigate, do NOT edit. For EACH ' +
  'discovered ripple: CONFIRM it is REAL — the seam shape WF-1+WF-2 landed ACTUALLY changed ' + 'in the direction the ripple claims, and every cited member is verified via `assay api` / ' +
  'Context7. DROP any PHANTOM ripple (a member that does not exist, a ripple whose seam ' + 'did not actually change, or one that would re-mint the C# contract rather than decode it). Return ' +
  'the VALIDATED re-bind list per site (site + the SURVIVING ripples) + dropped (the phantom ripples, each with the one-line reason) + summary. DISCOVERED:\n' + JSON.stringify(discovered, null, 1)].join('\n')
const rebindPrompt = (s) => [DOCTRINE, '', 'TASK: RE-BIND the sibling site `' + s.name + '` (folder `' + s.root + '/**`, CODE stack `' + STACK[s.lang] + '`) to the changed ' +
  'Component/detail-schema wire — IN PLACE, decode-not-remint for the python/ts decoders, ' + 'resolution-through-Component for the C# consumers. For EACH validated ripple below, MAKE the ' +
  're-bind edit NOW to the strongest CLEAN/MODERN form the site language doctrine + ' + 'the DECODE-NOT-REMINT law admit, preserving `ProfileRef`/`ProfileSet`/`ComputedSection` seam-canonical ' +
  'and never re-minting the C# contract; record the site`s ' + 'seam row in its `ARCHITECTURE.md` `[2]-[SEAMS]` where applicable. Do NOT re-open Element/Materials/Bim design. A re-bind ' +
  'requiring a FILE outside this site goes to residual_high {files, claim}. VALIDATED RIPPLES (' + s.ripples.length + '):\n' + JSON.stringify(s.ripples, null, 1) + '\nReturn folder (the ' +
  'site name) + verdict + integrated (each ripple re-bound + where) + extended + residual_high + summary.'].join('\n')
const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE re-bind residuals the discover/validate/rebind phases surfaced; NO severity, NO ' +
  'leftovers, NO deferral, NO scope cap. Read EVERY listed file across the sibling sites (csharp + python + ts) ' + 'and FIX the real cross-file defect in place to the strongest clean/modern + ' +
  'decode-not-remint form (align the changed wire + every consumer in lockstep, ' + 'preserve `ProfileRef`/`ProfileSet`/`ComputedSection` seam-canonical, never re-mint the C# contract), ' +
  'preserving all capability — a token patch that leaves the wire misaligned ' + 'is NOT a fix; if a residual is FACTUALLY WRONG, leave it and say why. If your fix surfaces a new cross-file ' +
  'need, report it in residual_high. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [LAW, '', DECODE, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and CONFIRM the ' +
  're-bind is ACTUALLY made AND complete + clean/modern/decode-not-remint, ' + 'not a token/naive patch. ATTACK it: shallow, partial, a rename that left a sibling decoder stale, the wire still ' +
  'misaligned, a re-minted C# contract? ' + 'Classify each: "fixed" (real, complete, non-naive), "invalid" (claim PROVABLY WRONG — cite why), or "open" (NOT fixed, OR fixed naively — a ' +
  'confident edit that does not truly resolve the cross-file defect is open, never fixed). Default "open" on ANY doubt. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer ' +
  'touched: ' + JSON.stringify(fixFiles)].join('\n')
const sanityPrompt = (items) => [DOCTRINE, '', 'TASK: SANITY RE-AUDIT over the UNION of ALL ' + items.length + ' surfaced re-bind residuals. For EACH item: re-read the cited file(s) ' +
  'from disk and CONFIRM the re-bind is GENUINELY + CLEANLY done — the changed wire is honored on BOTH endpoints, the decode-not-remint law holds, ' +
  '`ProfileRef`/`ProfileSet`/`ComputedSection` stay seam-canonical, no token patch, ' + 'no sibling decoder left stale, no C# contract re-minted. Be adversarial: a confident-looking edit that ' +
  'does not truly resolve the cross-file defect is "open". Classify each item "fixed" or "open" with one-line evidence; default "open" on ANY doubt. ITEMS:\n' +
  items.map((it, i) => (i + 1) + '. [' + it.files.join(', ') + '] ' + it.claim).join('\n') + '\nReturn overall (true only if ALL fixed) + items (per-item status + evidence) + summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('sibling-ripple: ripple-discover (' + SITES.length + ' sibling sites) -> validate -> rebind (decode-not-remint) -> terminal resolve (reconcile + sanity drive-to-zero); CAP=' + CAP)

// --- [DISCOVER]
phase('Discover')
const discovered = (await pool(SITES, CAP, (s) => agent(discoverPrompt(s), { label: 'discover:' + s.name, phase: 'Discover', schema: DISCOVER_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
log('Discover: ' + discovered.reduce((n, d) => n + (d.ripples || []).length, 0) + ' ripple(s) across ' + discovered.length + ' sites')

// --- [VALIDATE]
phase('Validate')
const validation = await agent(validatePrompt(discovered), { label: 'validate', phase: 'Validate', schema: VALIDATE_SCHEMA, effort: 'xhigh', stallMs: STALL })
const validated = (validation && validation.validated) || []
const validatedBySite = new Map(validated.map((v) => [v.site, v.ripples || []]))
log('Validate: ' + validated.reduce((n, v) => n + (v.ripples || []).length, 0) + ' real ripple(s); ' + (((validation && validation.dropped) || []).length) + ' phantom dropped')

// --- [REBIND]
phase('Rebind')
const rebindSites = SITES.map((s) => ({ ...s, ripples: validatedBySite.get(s.name) || [] }))
const rebound = (await pool(rebindSites, CAP, (s) => agent(rebindPrompt(s), { label: 'rebind:' + s.name, phase: 'Rebind', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [RESOLVE]
const fbOf = (r) => { const site = SITE_BY.get(r.folder); return site ? site.root : 'libs/csharp' }
let union = dedup(rebound.flatMap((r) => (r.residual_high || []).map((x) => norm(x, fbOf(r)))))
let pending = union.slice()
let invalid = []
let round = 0
if (pending.length) {
  phase('Resolve')
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = cluster(pending)
    log('Resolve round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (sibling-wide, no-defer)')
    const resolved = (await pool(clusters, CAP, async (cl) => {
      const fix = await agent(reconcileFix(cl), { label: 'resolve-fix:r' + round, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL })
      if (!fix) return { open: cl, invalid: [], surfaced: [] }
      const verify = await agent(reconcileVerify(cl, fix.files), { label: 'resolve-verify:r' + round, phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
      const claims = (verify && verify.claims) || []
      const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
      const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
      return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map((x) => norm(x, 'libs/csharp')) }
    })).filter(Boolean)
    invalid = dedup([...invalid, ...resolved.flatMap((r) => r.invalid)])
    const invalidKeys = new Set(invalid.map((r) => r.claim))
    const surfaced = resolved.flatMap((r) => r.surfaced)
    union = dedup([...union, ...surfaced]).filter((r) => !invalidKeys.has(r.claim))
    pending = dedup([...resolved.flatMap((r) => r.open), ...surfaced]).filter((r) => !invalidKeys.has(r.claim))
  }
  if (pending.length) log('Resolve: ' + pending.length + ' still open after ' + MAX_ROUNDS + ' reconcile rounds -> handed to the sanity drive-to-zero')
}

// --- [SANITY]
// DRIVE TO ZERO: re-audit the UNION of every surfaced residual; force-close PER union-find cluster (files preserved) + re-audit until nothing is open. The cap is a runaway backstop.
const fileOf = new Map(union.map((r) => [r.claim, r.files]))
let sanity = null
let sanityOpen = []
let saneRound = 0
if (union.length) {
  sanity = await agent(sanityPrompt(union), { label: 'sanity', phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
  sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
  while (sanityOpen.length && saneRound < SANITY_CAP) {
    saneRound++
    const openRes = sanityOpen.map((i) => ({ files: fileOf.get(i.claim) || ['libs/csharp'], claim: i.claim }))
    const clusters = cluster(openRes)
    log('Sanity round ' + saneRound + ': ' + sanityOpen.length + ' OPEN -> ' + clusters.length + ' cluster(s) FORCE-CLOSE (files preserved) + re-audit; nothing leaves open')
    await pool(clusters, CAP, (cl) => agent(reconcileFix(cl), { label: 'sanity-force-close:r' + saneRound, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL }))
    sanity = await agent(sanityPrompt(union), { label: 'sanity:r' + saneRound, phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
    sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
  }
  if (sanityOpen.length) log('Sanity: ' + sanityOpen.length + ' STILL OPEN after ' + SANITY_CAP + ' force-close rounds — HARD BLOCKER, reported LOUDLY, never silently dropped')
  else log('Sanity: ALL ' + union.length + ' surfaced residual(s) CLOSED + verified across ' + saneRound + ' force-close round(s)')
} else { log('Resolve: no residuals surfaced — clean') }

return {
  workflow: 'sibling-ripple', sites: SITES.map((s) => s.name),
  ripples: discovered.map((d) => ({ site: d.site, ripples: (d.ripples || []).length })),
  dropped: ((validation && validation.dropped) || []).length,
  rebindVerdicts: rebound.map((r) => ({ site: r.folder, verdict: r.verdict })),
  resolveRounds: round, saneRounds: saneRound,
  invalidClaims: invalid.map((x) => x.claim),
  reconcilePending: pending.map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: sanityOpen.map((i) => ({ claim: i.claim, evidence: i.evidence })),
}
