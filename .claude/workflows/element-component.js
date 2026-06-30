export const meta = {
  name: 'element-component',
  description: 'WF-1 of the Rasm Material/Component/Element rebuild — the atomic Element plus Materials concern. Collapses the Materials Profiles and Connection sub-domains into ONE Component owner (cross-section as a Component field), refines the Element seam (ObjectKind Type/Occurrence, deterministic-rooted Type identity, owner-mints-its-identity, named Bake type-to-occurrence inheritance, the one neutral detail schema, the MaterialBinding/BakedMaterial/TypeBinding rename), captures the complete generative data per component family, applies the verbatim rename map, lands the seam-mirror rows at both endpoints, and drives every surfaced residual to zero through an adversarial reconcile then sanity loop. Design-doc work only; the sibling body re-binds are WF-2/WF-3.',
  whenToUse: 'The first leg of the Material/Component/Element rebuild — atomic Element plus Materials, one concern.',
  phases: [
    { title: 'Discover' },
    { title: 'Research' },
    { title: 'Validate' },
    { title: 'Decide' },
    { title: 'Pins' },
    { title: 'Rebuild' },
    { title: 'Critique' },
    { title: 'Redteam' },
    { title: 'Sweep' },
    { title: 'Resolve' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const STALL = 600000
const MAX_ROUNDS = 6
const SANITY_CAP = 6
const BATCH = 4
const SCOPE = 'RASM-REBUILD-SCOPE.md'
const PROPS = 'Directory.Packages.props'
const MAT = 'libs/csharp/Rasm.Materials'
const ELEM = 'libs/csharp/Rasm.Element'
const PROJECTION_PAGE = MAT + '/.planning/Projection/material.md'
const CONSTRUCTION_ROOT = MAT + '/.planning/Construction'
const API_DIR = MAT + '/.api'
const AREAS = [
  { key: 'profiles-connection', scope: 'Materials Profiles/* + Connection/* (the collapse SOURCE that becomes Component/) under ' + MAT + '/.planning' },
  { key: 'properties-projection-construction', scope: 'Materials Properties/* + Projection/material.md + Construction/* under ' + MAT + '/.planning' },
  { key: 'element-seam', scope: 'the Element seam touchpoints Graph/element.md, Relations/relation.md, Properties/property.md, ' +
    'Composition/material.md, Projection/projection.md under ' + ELEM + '/.planning' },
  { key: 'appearance-anchors', scope: 'the Materials Appearance/* 5 cross-page anchors into the collapsing pages under ' + MAT + '/.planning' },
]
const FAMILIES = ['CMU', 'brick/masonry', 'glazing IGU', 'fastener', 'rebar', 'headed stud', 'weld/joint', 'steel-sections + timber']

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['area', 'pages', 'summary'], properties: { area: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } }, gaps: { type: 'array', items: { type: 'string' } }, anchorsToPreserve: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const RESEARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['family', 'fields', 'summary'], properties: { family: { type: 'string' }, fields: { type: 'array', items: { type: 'string' } }, seedRows: { type: 'string' }, verifiedMembers: { type: 'array', items: { type: 'string' } }, pinsNeeded: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package'], properties: { package: { type: 'string' }, note: { type: 'string' } } } }, summary: { type: 'string' } } }
const VALIDATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['family', 'summary'], properties: { family: { type: 'string' }, validatedSpec: { type: 'string' }, phantoms: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const DECIDE_SCHEMA = { type: 'object', additionalProperties: false, required: ['componentPages', 'elementSeamRefinements', 'renameMap', 'retirements', 'summary'], properties: { componentPages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'skeleton'], properties: { path: { type: 'string' }, skeleton: { type: 'string' } } } }, elementSeamRefinements: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['path', 'refinement'], properties: { path: { type: 'string' }, refinement: { type: 'string' } } } }, renameMap: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['old', 'new', 'kind'], properties: { old: { type: 'string' }, new: { type: 'string' }, kind: { type: 'string' } } } }, retirements: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const PINS_SCHEMA = { type: 'object', additionalProperties: false, required: ['pinned', 'verdict', 'summary'], properties: { pinned: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'version'], properties: { package: { type: 'string' }, version: { type: 'string' } } } }, verdict: { type: 'string', enum: ['pinned', 'none'] }, restore: { type: 'string' }, summary: { type: 'string' } } }
const FOLDER_FIXLOG = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['closed', 'hardened', 'refined', 'clean'] }, integrated: { type: 'array', items: { type: 'string' } }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['aligned', 'clean'] }, aligned: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SANITY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'items'], properties: { overall: { type: 'boolean' }, items: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'open'] }, evidence: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const SCOPELAW = [
  'CAMPAIGN: the unified Material/Component/Element rebuild (WF-1, the atomic Element+Materials concern). READ ' + SCOPE + ' (repo root) in FULL FIRST; the ' +
    'named section governs the stage. All work is .planning design-doc work — code fences ARE the product (transcription-complete, decompile-verified, ' +
    'implementation-ready); edits to manifests, .api, tools, tests outside libs are real. The Materials index docs + .api live at the package root ' +
    'Rasm.Materials/, NOT inside .planning.',
  'CROSS-WORKFLOW CONTRACT: WF-1 LANDS the seam-mirror ROWS at BOTH endpoints (Element ARCHITECTURE.md [2]-[SEAMS] and Materials ARCHITECTURE.md ' +
    '[2]-[SEAMS]). The sibling Bim/Compute/Persistence/python/ts BODY re-binds are WF-2/WF-3 — do NOT edit those bodies here. Resolve a captured WATCH ' +
    'residual per ' + SCOPE + ' [3] ONLY when this rebuild touches its site.',
].join('\n')
const AXES = [
  'DUAL-AXIS READ (every page on BOTH; finalizes only when a cold read surfaces nothing per ' + SCOPE + ' [6]). CODE doctrine: docs/stacks/csharp/ every ' +
    'core page by name (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) PLUS the relevant ' +
    'docs/stacks/csharp/domain/ shard — for this campaign especially data-interchange, diagnostics, interaction, transport, validation.',
  'DOC-CRAFT axis: libs/.planning/README.md [PLANNING_STANDARD] incl. its [06] cold-grade gate, the design-page grammar, and the page#CLUSTER ' +
    'integration-point notation; the three docs/standards/ form standards (information-structure, formatting, style-guide); docs/standards/proof.md claim ' +
    'discipline. The README cold-grade is the doc-finalization gate.',
].join('\n')
const APILAW = 'TWO-TIER .api (per ' + SCOPE + ' [7]): cite BOTH the shared substrate libs/csharp/.api/ AND the folder tier Rasm.Materials/.api/ — and ONLY ' +
  'the catalogs the concept composes, never noise. Members are verified-local truth via `uv run python -m tools.assay api`; a member you cannot verify is a ' +
  'PHANTOM — never introduce it. Live NuGet feed intelligence routes through the nuget MCP; assay api wins on conflict for member existence.'
const HEDGELAW = 'BANNED HEDGES + ZERO-PROVENANCE (word-boundary, page-wide, per ' + SCOPE + ' [6]): no should, could, would, might, maybe, perhaps, likely, ' +
  'probably, propose, consider, recommended, ideally, TBD, TODO, FIXME, we, our, you, or the synonym forms (is expected to, can be, aims to, is designed to, ' +
  'in the future, eventually, as needed, if necessary). On a design page: no reader address, narration, process, provenance, source-mining history, freshness ' +
  'disclaimers, checklist tails, links, URLs, versions, dates, or session context. Future tense is legal only on a card growth line and a RESEARCH item.'
const STANCE = 'ADVERSARIAL + WRITE-FULLY: assume the page is incomplete, naive, or illusory until it survives an aggressive attack; the burden of proof is on ' +
  'the page. Close every gap NOW via Edit/Write — the returned log REPORTS edits already made, never a to-do. Only a genuine cross-FILE item goes to ' +
  'residual_high as {files, claim} for the terminal Resolve (which has no scope cap).'
const DOCTRINE = [SCOPELAW, '', AXES, '', APILAW, '', HEDGELAW, '', STANCE].join('\n')

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
const inLibs = (p) => typeof p === 'string' && (p.startsWith('libs/') || p.indexOf('/libs/') !== -1 || p === PROPS)
const norm = (x, fallback) => { const files = Array.isArray(x.files) ? x.files.filter(inLibs) : []; return { files: files.length ? files : [fallback], claim: x.claim } }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const chunk = (xs, n) => { const out = []; for (let i = 0; i < xs.length; i += n) out.push(xs.slice(i, i + n)); return out }
const REBUILD_KIND = {
  component: 'Capture the VALIDATED complete generative data per ' + SCOPE + ' [2] + the parameterized catalog loader rail + DENSE in-fence seed ' +
    'tables + the VividOrange/hand-roll split per [4]; cross-section is a Component FIELD.',
  seam: 'Realize the Element seam refinement mechanisms per ' + SCOPE + ' [1] (ObjectKind Type/Occurrence, deterministic-rooted Type ' +
    'identity, owner-mints-its-identity, named Bake inheritance + TypeId, the ONE neutral detail schema, the MaterialBinding trio rename).',
  projection: 'Author the ONE Component projection collapsing MaterialProjector + ConnectionProjector into one Project fold discriminating ' +
    'primary/minor + type/occurrence; mint the deterministic-rooted Type Object and bind occurrences via Assign.TypeDefinition.',
  construction: 'Keep + HARDEN Construction so it consumes Component; collapse any naive owner; align every anchor to the rename map.',
  index: 'Re-derive the four Materials index docs at the package root (README.md, ARCHITECTURE.md, IDEAS.md, TASKLOG.md) truthful to ' +
    'the rebuilt pages incl. the [2]-[SEAMS] rows.',
  api: 'Author the folder .api catalog inline for the admitted package (every member assay-verified, matching the existing .api convention).',
}
const discoverPrompt = (a) => [DOCTRINE, '', 'TASK: READ-ONLY DISCOVERY for the `' + a.key + '` area — ' + a.scope + '. Per ' + SCOPE + ' [1] (the unified ' +
  'paradigm) and [5] (the rename + boundary law), inventory the CURRENT page list, the gap/defect inventory (naive owners, scalar smears, hardcodes, phantom ' +
  'members, dangling anchors, dual-paradigm coupling), and the anchors this area must PRESERVE. FROZEN: Appearance AppearanceKey + AppearanceSummary.Of ' +
  '(tolerance 0.0, full IEEE precision) and the 5 Appearance cross-page anchors are anchor-only — never perturb a signature. Materials index docs + .api live ' +
  'at the package root Rasm.Materials/, not inside .planning. Return area + pages + gaps + anchorsToPreserve + summary.'].join('\n')
const researchPrompt = (f) => [DOCTRINE, '', 'TASK: COMPLETE GENERATIVE-DATA RESEARCH for the `' + f + '` family per ' + SCOPE + ' [2] (the cited standards ' +
  'ground each field) and [4] (CATALOG_LAW: schema + polymorphic loader rail + DENSE in-fence seed tables + the VividOrange-owns vs hand-roll split). Gather ' +
  'the COMPLETE parametric field set + authoritative seed-table rows grounded in the [2] standards named for this family, and confirm which VividOrange ' +
  'typed-designation + SteelStiffness members exist. Research via Exa (mcp__exa__web_search_exa, mcp__exa__web_search_advanced_exa, mcp__exa__web_fetch_exa) ' +
  '+ Tavily (mcp__tavily__tavily_search, mcp__tavily__tavily_extract); for live library/API shape ToolSearch-load Context7 FIRST by exact name ' +
  'mcp__plugin_context7_context7__resolve-library-id + mcp__plugin_context7_context7__query-docs before first use. Verify every member via ' +
  '`uv run python -m tools.assay api` (assay api wins on member existence). Per ' + SCOPE + ' [4] NO new NuGet package is required — flag pinsNeeded ONLY if a ' +
  'family genuinely admits one. Return family + fields + seedRows + verifiedMembers + pinsNeeded + summary.'].join('\n')
const validatePrompt = (p) => [DOCTRINE, '', 'TASK: VALIDATE the `' + p.family + '` research capture against ground truth. Cross-check EVERY field + member ' +
  'against `uv run python -m tools.assay api` (the member verifiably exists) and against the ' + SCOPE + ' [4] split (VividOrange owns steel section ' +
  'catalogues + EN grade factories + elastic/concrete section properties; hand-roll in-fence everything VividOrange does not own — the Wpl/It/Iw/Av gap via ' +
  'SteelStiffness, CMU, brick, bolt+nut+washer, rebar rib geometry, headed stud, IGU stacks, timber + AISI grades). Flag any PHANTOM member or any field the ' +
  'cited [2] standard does not support. ToolSearch-load any deferred MCP tool by exact name before first use. RESEARCH CAPTURE:\n' +
  JSON.stringify(p.research, null, 1) + '\nReturn family + validatedSpec + phantoms + summary.'].join('\n')
const decidePrompt = (disc, val, anchors) => [DOCTRINE, '', 'TASK: DECIDE the coherent architecture (single agent — the topology must be coherent). Per ' +
  SCOPE + ' [1] + [5]: (a) the new Component/ topology collapsing Profiles + Connection into ONE polymorphic owner over a primary/minor ' +
  'discriminant, cross-section as a Component FIELD (the ParametricSection + ComputedSection machinery become Component-internal) — ' +
  'emit the page set with per-page skeletons; (b) the Element seam refinement spec: ObjectKind in {Type, Occurrence} with ' +
  'deterministic-rooted Type identity via Object.ToCanonicalBytes EXCLUDING Representations, the owner-mints-its-identity ' +
  'projection law, the named Bake type->occurrence inheritance (a NAMED dimension, NOT an InheritanceMode extension which is ' +
  'PropertyBag-only) plus TypeId on the baked Element, the ONE neutral detail schema declared in Element Properties/property.md + ' +
  'Composition/material.md + new [2]-[SEAMS] rows, and the MaterialBinding->BakedMaterial(Element)/MaterialBinding(Materials)/' +
  'TypeBinding(new) rename; (c) the VERBATIM old->new symbol/file/anchor RENAME MAP (~428 anchors: Profiles+Connection->Component; ' +
  'hanger->connector; JointSection/NominalMm disambiguation; cmu WebThicknessMm->CrossWebMm/EndWebMm; GroutFraction->' +
  'GroutedCellFraction; resolve the Coring cross-family leak; the MaterialBinding trio); (d) the RETIREMENT list; (e) per-new-page ' +
  'skeletons. REUSE ObjectKind.Type (Graph/element.md [02]-[NODE_MODEL]) + AssignKind.TypeDefinition (Relations/relation.md L57) — ' +
  'NEVER a parallel DefinesByType case. ProfileRef/ProfileSet/ComputedSection STAY seam-canonical; the semantic-rename STOPS at ' +
  'the Materials folder boundary; Appearance signatures are OUT OF SCOPE (anchor-only). DISCOVERY:\n' +
  JSON.stringify(disc.map((d) => ({ area: d.area, pages: d.pages, gaps: d.gaps, anchors: d.anchorsToPreserve, summary: d.summary })), null, 1) +
  '\nVALIDATED CAPTURE:\n' + JSON.stringify(val, null, 1) + '\nPRESERVE ANCHORS:\n' + JSON.stringify(anchors) +
  '\nReturn componentPages + elementSeamRefinements + renameMap + retirements + summary.'].join('\n')
const pinsPrompt = (pins) => [SCOPELAW, '', APILAW, '', 'TASK: CENTRAL PIN ADMISSION (guarded — ' + SCOPE + ' [4] expects NONE; run only because a family ' +
  'flagged one). Packages flagged:\n' + JSON.stringify(pins, null, 1) + '\nFor EACH: confirm the NEWEST stable via the nuget MCP — ToolSearch-load FIRST by ' +
  'exact name mcp__nuget__get_latest_package_version + mcp__nuget__get_package_context (net10.0 TFM + commercial-safe license) — then HAND-EDIT `' + PROPS +
  '` to add/update the <PackageVersion> in the correct label-grouped cluster (sorted, one-line maintenance comment), NEVER `dotnet add`. Then `dotnet ' +
  'restore` + `dotnet nuget why` to confirm the graph. Edit ONLY `' + PROPS + '`. Return pinned + verdict + restore + summary.'].join('\n')
const rebuildPrompt = (it, rmap, anchors) => [DOCTRINE, '', 'TASK: REBUILD the DECIDED page `' + it.path + '` (kind: ' + it.kind + '). ' + REBUILD_KIND[it.kind] +
  ' Apply the rename map to every anchor this page owns; keep ProfileRef/ProfileSet/ComputedSection seam-canonical; never perturb a frozen Appearance ' +
  'signature. WRITE the page fully in place to the strongest clean/modern docs/stacks/csharp form — REAL transcription-complete fences, ZERO ' +
  'placeholder/stub/TODO; a cross-FILE need goes to residual_high {files, claim}. SKELETON/INTENT:\n' + (it.skeleton || '(derive from the decided topology)') +
  '\nRENAME MAP:\n' + JSON.stringify(rmap, null, 1) + '\nPRESERVE ANCHORS:\n' + JSON.stringify(anchors) +
  '\nReturn folder + verdict + integrated + extended + residual_high + summary.'].join('\n')
const critiquePrompt = (batch, i) => [DOCTRINE, '', 'TASK: DUAL-AXIS HOSTILE CRITIQUE + FIX IN PLACE across this batch of rebuilt pages (batch ' + i + '). ' +
  'Assume a violation in EVERY fence until proven otherwise. Run the CODE-axis mechanical checks (COLLAPSE_SCAN, OWNER_CHOOSER, KNOB_TEST, two-weave ASPECTS, ' +
  'RAILS, STRATA/MEMBERS, both .api tiers MAXIMIZED, no phantom member) AND the DOC-CRAFT axis (the [06] cold-grade, page grammar, banned hedges, ' +
  'zero-provenance) and REPAIR every hit in place; a cross-FILE item goes to residual_high {files, claim}. PAGES:\n' + JSON.stringify(batch, null, 1) +
  '\nReturn folder + verdict + integrated + extended + residual_high + summary.'].join('\n')
const redteamPrompt = (batch, crit, i) => [DOCTRINE, '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE across this batch (batch ' + i + ') — the LAST, most ' +
  'aggressive pass; trust nothing the critique claimed. COUNTERFACTUAL on each owner (is it the densest the doctrine admits?), ANTICIPATORY_COLLAPSE of the ' +
  'next case, LONG-TAIL ingress AND egress, STRATA + BOUNDARY-INTEGRITY, SURFACE-SPRAWL + PHANTOM kill, plus a FULL COLD re-review of every critique dimension ' +
  'on BOTH axes. The batch MUST end denser, more capable, more correct. Edit in place; cross-FILE -> residual_high {files, claim}. PAGES:\n' +
  JSON.stringify(batch, null, 1) + '\nPRIOR CRITIQUE:\n' + JSON.stringify(crit, null, 1) +
  '\nReturn folder + verdict + integrated + extended + residual_high + summary.'].join('\n')
const sweepPrompt = (batch, rmap, anchors, i) => [DOCTRINE, '', 'TASK: SEAM-MIRROR + RENAME SWEEP over this batch of rebuilt pages (batch ' + i + '). Apply ' +
  'the ATOMIC rename map across every anchor these pages own, and the Appearance ANCHOR-ONLY fixups (the 5 anchors; never touch an Appearance signature; ' +
  'AppearanceKey + AppearanceSummary.Of FROZEN). ' + (i === 0 ? 'THIS BATCH (index 0) ALSO owns the cross-cutting seam: LAND the seam-mirror ROWS at BOTH ' +
  'endpoints (Element ARCHITECTURE.md [2]-[SEAMS] <-> Materials ARCHITECTURE.md [2]-[SEAMS]) with mirrored glyphs, and DELETE the intra-Component [BOUNDARY] ' +
  'seam rows (an in-package relation is codemap, never a seam). ' : '') + 'WF-1 lands the rows at BOTH endpoints; the Bim/Compute/Persistence/python/ts BODY ' +
  're-binds are WF-2/WF-3 — do NOT edit those bodies. A counterpart edit you cannot complete -> residual_high {files, claim}. PAGES:\n' +
  JSON.stringify(batch, null, 1) + '\nRENAME MAP:\n' + JSON.stringify(rmap, null, 1) + '\nReturn verdict + aligned + residual_high + summary.'].join('\n')
const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals the rebuild/critique/redteam/sweep phases ' +
  'surfaced; NO severity, NO leftovers, NO deferral, NO scope cap. Read EVERY listed file across libs (csharp + py + ts) + ' + PROPS + ' and FIX the real ' +
  'cross-file defect in place to the strongest clean/modern form (align the seam + every consumer in lockstep, repair strata/boundary, finish a spanning ' +
  'admission, make a doc truthful), preserving all capability — a token patch that leaves the seam misaligned is NOT a fix; if a residual is FACTUALLY WRONG, ' +
  'leave it and say why. A new cross-file need -> residual_high. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [SCOPELAW, '', STANCE, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and ' +
  'CONFIRM the fix is ACTUALLY made AND complete + high-quality + clean/modern, not a token/naive patch. ATTACK it: shallow, partial, a rename that left a ' +
  'sibling stale, a seam still misaligned? Classify each "fixed" (real, complete, non-naive), "invalid" (claim factually wrong — cite why), or "open" (NOT ' +
  'fixed or fixed naively — redo). Default "open" on ANY doubt; a confident edit that does not truly resolve the cross-file defect is "open", never "fixed". ' +
  'Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')
const sanityPrompt = (items) => [DOCTRINE, '', 'TASK: SINGLE SANITY PASS over ALL ' + items.length + ' surfaced residuals (the snapshotted union). For EACH: ' +
  're-read the cited file(s) from disk and CONFIRM the defect is GENUINELY + CLEANLY fixed — the seam holds on BOTH endpoints, the fix is clean/modern per ' +
  'docs/stacks/csharp, no token patch, no sibling left stale, no new drift. Be adversarial: a confident-looking edit that does not truly resolve the defect is ' +
  '"open". Classify each "fixed" or "open" with one-line evidence; default "open" on ANY doubt. The full libs corpus is in scope to read. ITEMS:\n' +
  items.map((it, n) => (n + 1) + '. ' + it.claim).join('\n') + '\nReturn overall + items + summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('element-component: discover -> research -> validate -> decide -> pins -> rebuild -> critique/redteam -> sweep -> resolve; CAP=' + CAP)

// --- [DISCOVER]
phase('Discover')
const discovered = (await pool(AREAS, CAP, (a) => agent(discoverPrompt(a), { label: 'discover:' + a.key, phase: 'Discover', schema: DISCOVER_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
const anchors = [...new Set(discovered.flatMap((d) => d.anchorsToPreserve || []))]
log('Discover: ' + discovered.length + ' area(s); ' + anchors.length + ' anchor(s) to preserve')

// --- [RESEARCH]
phase('Research')
const researched = await pool(FAMILIES, CAP, (f) => agent(researchPrompt(f), { label: 'research:' + f, phase: 'Research', schema: RESEARCH_SCHEMA, effort: 'max', stallMs: STALL }))
const researchPairs = FAMILIES.map((f, i) => ({ family: f, research: researched[i] })).filter((p) => p.research)
const pinList = [...new Set(researched.filter(Boolean).flatMap((r) => (r.pinsNeeded || []).map((p) => p.package)))]

// --- [VALIDATE]
phase('Validate')
const validated = (await pool(researchPairs, CAP, (p) => agent(validatePrompt(p), { label: 'validate:' + p.family, phase: 'Validate', schema: VALIDATE_SCHEMA, effort: 'xhigh', stallMs: STALL }))).filter(Boolean)

// --- [DECIDE]
phase('Decide')
const decided = await agent(decidePrompt(discovered, validated, anchors), { label: 'decide', phase: 'Decide', schema: DECIDE_SCHEMA, model: 'opus', effort: 'max', stallMs: STALL })
const componentPages = (decided && decided.componentPages) || []
const seamRefinements = (decided && decided.elementSeamRefinements) || []
const renameMap = (decided && decided.renameMap) || []
const retirements = (decided && decided.retirements) || []
log('Decide: ' + componentPages.length + ' component page(s), ' + seamRefinements.length + ' seam refinement(s), ' + renameMap.length + ' rename(s)')

// --- [PINS]
phase('Pins')
if (pinList.length) await agent(pinsPrompt(pinList), { label: 'pins', phase: 'Pins', schema: PINS_SCHEMA, effort: 'max', stallMs: STALL })
else log('Pins: none needed — scope [4] admits no new package; generative data is hand-rolled in-fence')

// --- [REBUILD]
phase('Rebuild')
const rebuildItems = [
  ...componentPages.map((p) => ({ path: p.path, skeleton: p.skeleton, kind: 'component' })),
  ...seamRefinements.map((r) => ({ path: r.path, skeleton: r.refinement, kind: 'seam' })),
  { path: PROJECTION_PAGE, skeleton: '', kind: 'projection' },
  { path: CONSTRUCTION_ROOT, skeleton: '', kind: 'construction' },
  { path: MAT, skeleton: '', kind: 'index' },
]
if (pinList.length) rebuildItems.push({ path: API_DIR, skeleton: '', kind: 'api' })
const rebuilt = (await pool(rebuildItems, CAP, (it, i) => agent(rebuildPrompt(it, renameMap, anchors), { label: 'rebuild:' + it.kind + ':' + i, phase: 'Rebuild', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }))).filter(Boolean)
const rebuiltPages = rebuildItems.map((it) => it.path)
const batches = chunk(rebuiltPages, BATCH)
log('Rebuild: ' + rebuilt.length + '/' + rebuildItems.length + ' page-units written; ' + batches.length + ' review batch(es)')

// --- [CRITIQUE]
phase('Critique')

// --- [REDTEAM]
phase('Redteam')
const reviewed = (await pipeline(batches,
  (batch, _batch, i) => agent(critiquePrompt(batch, i), { label: 'critique:b' + i, phase: 'Critique', schema: FOLDER_FIXLOG, effort: 'xhigh', stallMs: STALL }),
  (crit, batch, i) => agent(redteamPrompt(batch, crit, i), { label: 'redteam:b' + i, phase: 'Redteam', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }).then((rt) => ({ critique: crit, redteam: rt })),
)).filter(Boolean)

// --- [SWEEP]
phase('Sweep')
const swept = (await pool(batches, CAP, (batch, i) => agent(sweepPrompt(batch, renameMap, anchors, i), { label: 'sweep:b' + i, phase: 'Sweep', schema: SWEEP_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [RESOLVE]
phase('Resolve')
const surfaced = dedup([
  ...rebuilt.flatMap((r) => (r.residual_high || []).map((x) => norm(x, MAT))),
  ...reviewed.flatMap((r) => ((r.critique && r.critique.residual_high) || []).map((x) => norm(x, MAT))),
  ...reviewed.flatMap((r) => ((r.redteam && r.redteam.residual_high) || []).map((x) => norm(x, MAT))),
  ...swept.flatMap((r) => (r.residual_high || []).map((x) => norm(x, MAT))),
])
const keyOf = (r) => r.files.slice().sort().join(',') + '|' + r.claim
const seen = new Set(surfaced.map(keyOf))
let pending = surfaced
let invalid = []
let noFix = []
let round = 0
while (pending.length && round < MAX_ROUNDS) {
  round++
  const clusters = cluster(pending)
  log('Resolve reconcile round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (lib-wide, no-defer)')
  const resolved = (await pool(clusters, CAP, async (cl) => {
    const fix = await agent(reconcileFix(cl), { label: 'resolve-fix:r' + round, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL })
    const touched = (fix && Array.isArray(fix.files) ? fix.files.filter(inLibs) : [])
    // No file-changing progress: the fix found nothing to change -> the cluster is resolved-or-phantom; skip the mandatory verify and drop it (recorded as noFix).
    if (!fix || touched.length === 0 || fix.verdict === 'clean') return { open: [], invalid: [], surfaced: [], dropped: cl, changed: false }
    const verify = await agent(reconcileVerify(cl, fix.files), { label: 'resolve-verify:r' + round, phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
    const claims = (verify && verify.claims) || []
    const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
    const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
    return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map((x) => norm(x, MAT)), dropped: [], changed: true }
  })).filter(Boolean)
  invalid = dedup([...invalid, ...resolved.flatMap((r) => r.invalid)])
  noFix = dedup([...noFix, ...resolved.flatMap((r) => r.dropped)])
  const invalidKeys = new Set(invalid.map((r) => r.claim))
  // Re-enter ONLY genuinely-new residuals: a key already queued this run cannot re-enter (stops a phantom re-feeding every round).
  const fresh = dedup([...resolved.flatMap((r) => r.open), ...resolved.flatMap((r) => r.surfaced)]).filter((r) => !invalidKeys.has(r.claim) && !seen.has(keyOf(r)))
  fresh.forEach((r) => seen.add(keyOf(r)))
  pending = fresh
  // NO-PROGRESS BREAK: no cluster changed a file this round -> the remaining residuals are phantom/unfixable; stop instead of grinding to MAX_ROUNDS.
  if (!resolved.some((r) => r.changed)) { log('Resolve reconcile round ' + round + ': no file-changing progress — ' + noFix.length + ' residual(s) had nothing to fix (phantom/resolved); breaking'); pending = []; break }
}
if (round && pending.length) log('Resolve reconcile: ' + pending.length + ' open after ' + MAX_ROUNDS + ' rounds — re-audited in the sanity drive-to-zero')

// DRIVE TO ZERO: re-audit the whole snapshotted residual universe after EVERY force-close until nothing is open. The cap is a runaway backstop, not a give-up.
const universe = surfaced
let sanity = universe.length ? await agent(sanityPrompt(universe), { label: 'sanity', phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL }) : null
let openClaims = new Set(((sanity && sanity.items) || []).filter((i) => i.status === 'open').map((i) => i.claim))
let sanityOpen = universe.filter((r) => openClaims.has(r.claim))
let saneRound = 0
while (sanityOpen.length && saneRound < SANITY_CAP) {
  saneRound++
  const prevOpen = sanityOpen.length
  const clusters = cluster(sanityOpen)
  log('Resolve sanity round ' + saneRound + ': ' + sanityOpen.length + ' OPEN -> force-close per cluster (' + clusters.length + ') + re-audit; nothing leaves open')
  const forced = (await pool(clusters, CAP, (cl) => agent(reconcileFix(cl), { label: 'sanity-force-close:r' + saneRound, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
  const fcTouched = forced.flatMap((fc) => (fc && Array.isArray(fc.files) ? fc.files.filter(inLibs) : []))
  // The force-close changed nothing: the remaining items are phantom/unfixable; skip the re-audit and stop.
  if (fcTouched.length === 0) { log('Resolve sanity round ' + saneRound + ': force-close changed no files — ' + sanityOpen.length + ' remaining item(s) phantom/unfixable; breaking'); break }
  sanity = await agent(sanityPrompt(universe), { label: 'sanity:r' + saneRound, phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
  openClaims = new Set(((sanity && sanity.items) || []).filter((i) => i.status === 'open').map((i) => i.claim))
  sanityOpen = universe.filter((r) => openClaims.has(r.claim))
  // No net decrease across the force-close -> drive-to-zero has stalled; stop instead of grinding to SANITY_CAP.
  if (sanityOpen.length >= prevOpen) { log('Resolve sanity round ' + saneRound + ': no net progress (' + sanityOpen.length + ' open, was ' + prevOpen + ') — remaining item(s) phantom/unfixable; breaking'); break }
}
if (sanityOpen.length) log('Resolve SANITY: ' + sanityOpen.length + ' STILL OPEN after the force-close drive — HARD BLOCKER, reported LOUDLY, never silently dropped')
else log('Resolve SANITY: all ' + universe.length + ' surfaced residual(s) CLOSED + verified across ' + saneRound + ' force-close round(s)')

return {
  workflow: 'element-component',
  areas: AREAS.map((a) => a.key),
  families: researched.filter(Boolean).map((r) => r.family),
  decided: decided ? { componentPages: componentPages.length, seamRefinements: seamRefinements.length, renames: renameMap.length, retirements: retirements.length } : null,
  pinsNeeded: pinList,
  rebuilt: rebuilt.length,
  batches: batches.length,
  resolveRounds: round,
  sanityRounds: saneRound,
  invalidClaims: invalid.map((x) => x.claim),
  noFix: noFix.map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: sanityOpen.map((x) => ({ files: x.files, claim: x.claim })),
}
