export const meta = {
  name: 'bim-rebuild',
  whenToUse: 'WF-2 of the Materials/Element/Bim rebuild: re-bind the Rasm.Bim seam signatures to the unified paradigm, fix the defect hit-list, apply the rename/split, mirror the seam rows into the page bodies, and drive every surfaced residual to zero.',
  description: 'Rasm.Bim seam RE-BIND (not teardown — 27 mostly world-class pages, KEEP every world-class page). DISCOVER (read-only, one agent per Bim area: Semantics, SemanticDetail, Geospatial, Exchange, ModelProjection, PlanningReview) returns the precise re-bind work-list. REBIND (one agent per area) re-threads every seam signature to the unified Material/Component/Element paradigm + the neutral detail schema + the Component Type representation, fixes the defect hit-list (import .ToError()/keyless CodecReject violating the bare-lift doctrine; format 13k-char Boundary paragraph + bogus ComparerAccessors Owner line), renames Model/structure.md to spatial.md, splits Projection/semantic.md into semantic/relations/egress, keeps geospatial isolated, reviews the ModelDiff.Encode/BcfWire second-wire drift, and writes in place (cross-file to residual_high). CRITIQUE then REDTEAM (pipeline, no barrier, batches of 4 re-bound pages, dual-axis, fix in place). SWEEP (batched, barrier) aligns each Bim page BODY to the seam-mirror rows WF-1 already landed in both ARCHITECTURE [02]-[SEAMS] endpoints and confirms the codemap + README roster are truthful, without re-authoring the rows. RESOLVE is the no-defer core: a reconcile fix-to-separate-adversarial-verify loop over the union-find clusters of every surfaced residual, then a SANITY re-audit force-closed per cluster until zero open, terminal pending logged loudly. WF-2 edits only Bim page bodies; the seam-mirror rows belong to WF-1. Takes no args.',
  phases: [
    { title: 'Discover' },
    { title: 'Rebind' },
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
const BIM = 'libs/csharp/Rasm.Bim'

// AREAS — the disjoint Bim re-bind partition (one Discover + one Rebind agent owns each; file-disjoint so a barrier'd pool never write-collides).
const AREAS = [
  { name: 'Semantics', pages: ['Semantics/composition.md', 'Semantics/connection.md'],
    focus: 'RE-THREAD the seam signatures (scope [1]): composition.md MaterialProjection now targets the NEUTRAL detail schema + the ' +
      'unified Type Object; connection.md ConnectionProjection READS the seam-declared detail schema (scope [1].5), never a hand-synced ' +
      'identical bag. The ONE Component projection mints the deterministic-rooted Type Object, stamps Classification/PredefinedType ' +
      '(validity stays the Bim AdmitPredefined egress gate), lowers the complete parametric data, binds occurrences via Assign.TypeDefinition.' },
  { name: 'SemanticDetail', pages: ['Semantics/properties.md', 'Semantics/classification.md', 'Semantics/appearance.md', 'Model/structural.md', 'Planning/cost.md'],
    focus: 'RE-BIND properties / classification / appearance / structural / cost to the unified paradigm + the neutral detail schema ' +
      '(scope [1]). Appearance reconciles at the owner-agnostic AppearanceKey — a FROZEN invariant (scope [5]); touch its cross-page ' +
      'anchors only, never perturb AppearanceKey/AppearanceSummary.Of.' },
  { name: 'Geospatial', pages: ['Semantics/geospatial.md', 'Semantics/georeference.md'],
    focus: 'KEEP geospatial in Bim, ISOLATED as its own sub-folder concern (scope [5]; NO new package). WATCH H8 (scope [3]) only if the ' +
      'ingress/emit site is touched: sniff FILE_SCHEMA / JSON schema_identifier before constructing DatabaseIfc; validate code+predefined ' +
      'against Header.ReleaseVersion at Emit.' },
  { name: 'Exchange', pages: ['Exchange/import.md', 'Exchange/format.md', 'Exchange/export.md', 'Exchange/reconstruct.md', 'Exchange/tessellation.md', 'Exchange/wire.md'],
    focus: 'DEFECT HIT-LIST: import.md `.ToError()` hops + keyless BimFault.CodecReject(detail) ctors VIOLATE the (Op key, string detail) ' +
      'bare-lift doctrine — fix to the keyed bare lift; format.md the ~13k-char single Boundary: paragraph (split into governed ' +
      'paragraphs/clusters) + the bogus ComparerAccessors Owner line — fix. WATCH H8 (scope [3]) only if the ingress/emit site is touched.' },
  { name: 'ModelProjection', pages: ['Model/elements.md', 'Model/faults.md', 'Model/query.md', 'Model/structure.md', 'Model/systems.md', 'Model/zones.md', 'Projection/semantic.md'],
    focus: 'RENAME Model/structure.md -> Model/spatial.md (breaks the structure/structural homonym; re-thread every inbound anchor — ' +
      'cross-file anchors -> residual_high). SPLIT Projection/semantic.md into semantic / relations / egress (one concern each). ' +
      'DECIDE IfcDomain/IfcSchema seam-vs-Bim (no strong signal — default Bim-internal). M4 (Ara3D.BimOpenSchema egress-owner) only if ' +
      'Type-node persistence forces it (scope [3]).' },
  { name: 'PlanningReview', pages: ['Planning/schedule.md', 'Review/coordination.md', 'Review/diff.md', 'Review/issues.md', 'Review/validation.md', 'Review/versioning.md'],
    focus: 'REVIEW the ModelDiff.Encode / BcfWire second-wire drift in Review/diff.md: the seam-graph snapshot wire is Persistence`s — keep ' +
      'Bim`s BCF/issue interchange wire, drop any duplicate graph-snapshot wire. KEEP every world-class page; align bodies, never teardown.' },
]

// PAGES — the post-rebind canonical page set (structure.md renamed to spatial.md; semantic.md split into semantic/relations/egress); batched for the page-scoped phases.
const PAGES = [
  'Semantics/appearance.md', 'Semantics/classification.md', 'Semantics/composition.md', 'Semantics/connection.md',
  'Semantics/georeference.md', 'Semantics/geospatial.md', 'Semantics/properties.md',
  'Exchange/export.md', 'Exchange/format.md', 'Exchange/import.md', 'Exchange/reconstruct.md', 'Exchange/tessellation.md', 'Exchange/wire.md',
  'Model/elements.md', 'Model/faults.md', 'Model/query.md', 'Model/spatial.md', 'Model/structural.md', 'Model/systems.md', 'Model/zones.md',
  'Projection/semantic.md', 'Projection/relations.md', 'Projection/egress.md',
  'Planning/cost.md', 'Planning/schedule.md',
  'Review/coordination.md', 'Review/diff.md', 'Review/issues.md', 'Review/validation.md', 'Review/versioning.md',
]

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const WORK = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'work'], properties: { page: { type: 'string' }, work: { type: 'string' } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['area', 'worklist', 'summary'], properties: { area: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } }, worklist: WORK, summary: { type: 'string' } } }
const FOLDER_FIXLOG = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['closed', 'hardened', 'refined', 'clean'] }, integrated: { type: 'array', items: { type: 'string' } }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['aligned', 'clean'] }, aligned: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SANITY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'items'], properties: { overall: { type: 'boolean' }, items: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'open'] }, evidence: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: bim-rebuild (WF-2) — the Rasm.Bim seam RE-BIND. READ ' + SCOPE + ' (repo root) in FULL FIRST; a prompt POINTS to its ' +
    'sections by number, NEVER restates scope.',
  'This is RE-BIND not teardown: Bim is 27 mostly world-class pages — KEEP every world-class page, align its body, never weaken ' +
    'capability. The unified Material/Component/Element paradigm + the NEUTRAL detail schema + the Component Type representation are ' +
    'scope [1]; the semantic-rename + Bim boundary law are scope [5]; the captured WATCH residuals are scope [3]; the catalog law is [4].',
  'BIM IS THE SOLE IFC SEMANTIC AUTHORITY: IFC names (Pset_*), GlobalId assignment, and the egress mapping stay in Bim`s ' +
    'SemanticProjector; a cross-peer invariant is a Bim-implemented IGraphConstraint, NEVER an IFC column in the seam (scope [1].5). ' +
    'The seam carries the NEUTRAL detail schema + the canonical PropertyName vocabulary only.',
].join('\n')
const STANDARDS = [
  'DUAL-AXIS MANDATE (scope [6]) — a page finalizes only when a COLD read against BOTH surfaces nothing.',
  'CODE doctrine: docs/stacks/csharp/** every core page by name (README, language, shapes, surfaces-and-dispatch, rails-and-effects, ' +
    'boundaries, algorithms, system-apis) + the relevant docs/stacks/csharp/domain/ shard — strategic emphasis for this rebuild: ' +
    'data-interchange, diagnostics, interaction, transport, validation.',
  'DOC-CRAFT: libs/.planning/README.md [PLANNING_STANDARD] + its [06] cold-grade REVIEW gate + the design-page grammar + the ' +
    'page#CLUSTER integration-point notation; the three docs/standards form standards (information-structure, formatting, style-guide); ' +
    'docs/standards/proof.md claim discipline.',
  'BANNED HEDGES (scope [6], word-boundary, page-wide): should/could/would/might/maybe/perhaps/likely/probably/propose/consider/' +
    'recommended/ideally/TBD/TODO/FIXME/we/our/you and the synonym forms (is expected to / can be / aims to / is designed to / in the ' +
    'future / eventually / as needed / if necessary); future tense is legal ONLY on a card growth line or a RESEARCH item.',
  'ZERO-PROVENANCE (scope [6]): no reader address, narration, process, source provenance, source-mining history, freshness ' +
    'disclaimers, checklist tails; on a design page no links, URLs, versions, dates, or session context.',
].join('\n')
const APITIER = [
  'TWO-TIER .api MANDATE (scope [7]): every page cites the `.api/` catalogs its concept composes — and ONLY those, never noise.',
  'The SUBSTRATE tier libs/csharp/.api/** (the cross-cutting external sources) is treated EQUALLY with the FOLDER tier ' + BIM +
    '/.api/** (the domain catalogues: the GeometryGym/IFC, IDS, BCF, units, geospatial CRS/vector/raster, glTF/USD/scene families). ' +
    'Members are verified-local truth via `uv run python -m tools.assay api`; a member you cannot verify is a PHANTOM — never cite it. ' +
    'Live NuGet feed facts route through the nuget MCP; assay api wins on conflict for member existence.',
].join('\n')
const ADVERSARIAL = 'ADVERSARIAL STANCE — every implementing stage (rebind, critique, redteam, sweep, resolve) is HOSTILE: assume the ' +
  'page is INCOMPLETE / NAIVE / ILLUSORY until it survives an aggressive attack; the burden of proof is ON THE PAGE. A prior clean ' +
  'verdict and confident dense prose are REJECTED — treat dense pages with MORE suspicion. ILLUSORY/FAKE is the primary target: a name ' +
  'promising capability the body lacks, a thin slice of a rich concept, a phantom member, a seam signature named but not actually re-bound.'
const SEAM = [
  'RE-BIND TARGETS (scope [1]): Semantics/composition MaterialProjection now targets the NEUTRAL detail schema + the unified Type ' +
    'Object; Semantics/connection ConnectionProjection READS the seam-declared detail schema, never a hand-synced identical bag; ' +
    're-bind properties / classification / appearance / structural / cost. The ONE Component projection mints the deterministic-rooted ' +
    'Type Object, stamps Classification/PredefinedType (validity stays the Bim AdmitPredefined egress gate), lowers the complete ' +
    'parametric data, and binds occurrences via Assign.TypeDefinition.',
  'BIM RENAMES (scope [5]): Model/structure.md -> Model/spatial.md (breaks the structure/structural homonym; re-thread every inbound ' +
    'anchor); SPLIT Projection/semantic.md into semantic / relations / egress. Semantics/geospatial STAYS in Bim, isolated as its own ' +
    'sub-folder concern (NO new package). DECIDE IfcDomain/IfcSchema seam-vs-Bim (default Bim-internal). REVIEW the ModelDiff.Encode / ' +
    'BcfWire second-wire drift (the seam-graph snapshot wire is Persistence`s).',
  'DEFECT HIT-LIST (scope [6] conformance): Exchange/import.md `.ToError()` hops + keyless BimFault.CodecReject(detail) ctors VIOLATE ' +
    'the (Op key, string detail) bare-lift doctrine — fix; Exchange/format.md the ~13k-char single Boundary: paragraph + the bogus ' +
    'ComparerAccessors Owner line — fix. WATCH (scope [3]) only if touched: H8 (sniff FILE_SCHEMA / JSON schema_identifier before ' +
    'constructing DatabaseIfc; validate code+predefined against Header.ReleaseVersion at Emit) if the ingress/emit site is touched; ' +
    'M4 (Ara3D.BimOpenSchema egress-owner) only if Type-node persistence forces it.',
  'CROSS-WF CONTRACT: WF-1 (element-component) landed the seam-mirror ROWS in BOTH ARCHITECTURE [02]-[SEAMS] endpoints. WF-2 edits ' +
    'ONLY ' + BIM + ' page BODIES — never Materials/Element/Compute/Persistence/python/ts, never re-author a seam-mirror row; align ' +
    'Bim`s body to the already-landed row.',
].join('\n')
const PROSE = 'PROSE + FENCES: design-SPEC prose only — lead with the controlling rule, one idea per paragraph, close on the ' +
  'consequence; cut hedges/provenance/process narration (scope [6]). REAL transcription-complete code fences, ZERO ' +
  'placeholder/stub/TODO. BACKTICK every symbol/type/member/path/package. Keep the canonical section-divider headers; agent-facing ' +
  'comments only where intent is not obvious.'
const DOCTRINE = [LAW, '', STANDARDS, '', APITIER, '', ADVERSARIAL, '', SEAM, '', PROSE].join('\n')

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
const chunk = (xs, n) => xs.reduce((acc, x, i) => { (i % n === 0 ? acc.push([x]) : acc[acc.length - 1].push(x)); return acc }, [])
const BATCHES = chunk(PAGES, BATCH)
const SWEEP_ITEMS = BATCHES.map((pages, i) => ({ kind: 'pages', i, pages })).concat([{ kind: 'codemap', i: -1 }])
const residualsOf = (rows, key) => rows.flatMap((r) => (r.residual_high || []).map((x) => norm(x, key(r))))

const discoverPrompt = (a) => [DOCTRINE, '', 'TASK: READ-ONLY RE-BIND DISCOVERY for the `' + a.name + '` area of `' + BIM + '/.planning` ' +
  '(pages: ' + a.pages.join(', ') + '). Investigate, do NOT edit. Read ' + SCOPE + ' in FULL, the listed pages, BOTH .api tiers, the ' +
  'index docs at ' + BIM + ' (ARCHITECTURE.md/README.md), and the seam endpoints the pages cite. AREA FOCUS: ' + a.focus + ' Produce the ' +
  'precise re-bind work-list: per page, the exact seam-signature re-thread / defect fix / rename / split to apply, grounded in ' +
  'assay-verified members. Return area + pages + worklist (each {page, work}) + a summary of the dominant work class.'].join('\n')
const rebindPrompt = (a, work) => [DOCTRINE, '', 'TASK: RE-BIND the `' + a.name + '` area (pages: ' + a.pages.join(', ') + ') IN PLACE. ' +
  'AREA FOCUS: ' + a.focus + ' For each page: re-thread every seam signature to the unified paradigm + the neutral detail schema + the ' +
  'Component Type representation (scope [1]); fix the named defects; apply the rename/split. WRITE-FULLY now — the returned log REPORTS ' +
  'edits already made, never a to-do. A fix that REQUIRES editing a page OUTSIDE this area goes to residual_high {files, claim} ' +
  '(especially the rename/split inbound-anchor re-threads). Edit ONLY `' + BIM + '` page bodies (WF-2 scope). DISCOVERED WORK-LIST:\n' +
  JSON.stringify(work, null, 1) + '\nReturn folder (the area name) + verdict + integrated + extended + residual_high + summary.'].join('\n')
const critiquePrompt = (batch, i) => [DOCTRINE, '', 'TASK: HOSTILE DUAL-AXIS CRITIQUE + FIX IN PLACE over these re-bound `' + BIM + '` ' +
  'pages (batch ' + i + '): ' + batch.join(', ') + '. Audit BOTH axes (scope [6]): CODE doctrine (the core docs/stacks/csharp/** pages + ' +
  'the relevant domain shard) AND DOC-CRAFT (the planning standard + the [06] cold-grade gate + the page grammar + the three form ' +
  'standards + proof.md). Run the mechanical checklists and REPAIR every hit: COLLAPSE_SCAN (3+ parallel shapes/sibling names/repeated ' +
  'arms -> one owner); OWNER_CHOOSER; KNOB_TEST; RAILS (closed Expected fault, total Switch, no exception control flow); two-weave ' +
  'ASPECTS; MEMBERS (no phantom, both .api tiers stacked); banned-hedges + zero-provenance. cross-file -> residual_high {files, claim}. ' +
  'Edit ONLY `' + BIM + '` page bodies. Return folder (batch ' + i + ') + verdict + extended + residual_high + summary.'].join('\n')
const redteamPrompt = (batch, i, crit) => [DOCTRINE, '', 'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE over these `' + BIM + '` pages ' +
  '(batch ' + i + '): ' + batch.join(', ') + ' — the LAST + most aggressive pass; trust nothing the critique claimed. COUNTERFACTUAL on ' +
  'each owner (does a denser owner / a deeper admitted-package primitive collapse the fence?); ANTICIPATORY_COLLAPSE (the next ' +
  'case/provider lands as ONE case/row/policy value, consumers broken LOUDLY at compile time); LONG-TAIL ingress AND egress; ' +
  'STRATA/BOUNDARY (IFC stays in SemanticProjector, geospatial isolated, no host-type leak); PHANTOMS; plus a FULL COLD re-review of ' +
  'every critique dimension on BOTH axes (scope [6]). CARRY FORWARD any unresolved cross-file residual_high from the critique result ' +
  'below and add your own. Edit ONLY `' + BIM + '` page bodies; cross-file -> residual_high {files, claim}. CRITIQUE RESULT:\n' +
  JSON.stringify((crit && { verdict: crit.verdict, residual_high: crit.residual_high || [] }) || {}, null, 1) +
  '\nReturn folder (batch ' + i + ') + verdict + extended + residual_high + summary.'].join('\n')
const sweepPrompt = (item) => item.kind === 'codemap'
  ? [DOCTRINE, '', 'TASK: TRUTHFULNESS SWEEP of `' + BIM + '/ARCHITECTURE.md` (codemap + [02]-[SEAMS]) + `' + BIM + '/README.md` roster. ' +
    'Confirm the codemap + the roster are TRUTHFUL to the re-bound pages: the renamed Model/spatial.md, the split ' +
    'Projection/{semantic,relations,egress}.md, the re-threaded seam signatures, the page set itself. Do NOT re-author the [02]-[SEAMS] ' +
    'mirror ROWS — WF-1 landed those on both endpoints; only correct an untruthful codemap/roster entry. cross-file -> residual_high ' +
    '{files, claim}. Return verdict + aligned + residual_high + summary.'].join('\n')
  : [DOCTRINE, '', 'TASK: SEAM-MIRROR BODY ALIGNMENT over these `' + BIM + '` pages: ' + item.pages.join(', ') + '. WF-1 ' +
    '(element-component) already landed the seam-mirror ROWS in BOTH `ARCHITECTURE.md` [02]-[SEAMS] endpoints; do NOT re-author those ' +
    'rows. ALIGN each page BODY to them: every seam signature, type name, and field the page references matches the landed mirror ' +
    'EXACTLY (the unified Object/ObjectKind, the neutral detail schema, the Component Type bind, the canonical PropertyName ' +
    'vocabulary). Edit ONLY these page bodies (`' + BIM + '`); a counterpart edit outside these pages -> residual_high {files, claim}. ' +
    'Return verdict + aligned + residual_high + summary.'].join('\n')
const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals the ' +
  'rebind/critique/redteam/sweep phases surfaced; NO severity, NO leftovers, NO deferral, NO scope cap WITHIN `' + BIM + '`. Read EVERY ' +
  'listed file and FIX the real cross-file defect in place to the strongest clean/modern form (re-thread the rename/split inbound ' +
  'anchors in lockstep, align every Bim consumer of the re-bound seam, make the codemap/roster truthful), preserving all capability — a ' +
  'token patch that leaves an anchor dangling or a seam misaligned is NOT a fix. WF-2 edits ONLY `' + BIM + '` page bodies (never ' +
  'Materials/Element/Compute/Persistence/python/ts; align Bim`s body to the already-landed seam row, never re-author the row). If a ' +
  'residual is FACTUALLY WRONG, leave it and say why. If your fix surfaces a new cross-file need, report it in residual_high. ' +
  'Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [LAW, '', SEAM, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — ' +
  're-read the named files from disk and CONFIRM the fix is ACTUALLY made AND complete + clean/modern, not a token/naive patch. ATTACK ' +
  'it: shallow, partial, a rename that left a sibling anchor stale, a seam still misaligned? Default `open` on ANY doubt — a confident ' +
  'edit that does not truly resolve the cross-file defect is `open`, never `fixed`. Classify each: `fixed` (real, complete, non-naive), ' +
  '`invalid` (claim PROVABLY wrong — cite why), or `open` (NOT fixed or fixed naively — redo). Claims:\n' + JSON.stringify(cl, null, 1) +
  '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')
const sanityPrompt = (items) => [DOCTRINE, '', 'TASK: SINGLE SANITY RE-AUDIT over ALL ' + items.length + ' surfaced re-bind residuals. ' +
  'For EACH item: re-read the cited file(s) from disk and CONFIRM the defect is GENUINELY + CLEANLY resolved on BOTH axes (scope [6]) — ' +
  'the seam signature re-bound, the rename/split anchors threaded, the defect-hit-list fix landed, no token patch, no sibling left ' +
  'stale, no new drift. Be adversarial: a confident-looking edit that does not truly resolve the defect is `open`. Classify each ' +
  '`fixed` or `open` with one-line evidence; default `open` on ANY doubt. The whole `' + BIM + '` corpus is in scope to read. ITEMS:\n' +
  items.map((it, i) => (i + 1) + '. [' + it.files.join(',') + '] ' + it.claim).join('\n') +
  '\nReturn overall (true only if ALL fixed) + items (per-item status + evidence) + summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('bim-rebuild: discover (' + AREAS.length + ' areas) -> rebind -> critique|redteam (pipeline, ' + BATCHES.length + ' batches) -> sweep -> resolve (reconcile + sanity); CAP=' + CAP)

// --- [DISCOVER]
phase('Discover')
const discovered = (await pool(AREAS, CAP, (a) => agent(discoverPrompt(a), { label: 'discover:' + a.name, phase: 'Discover', schema: DISCOVER_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
const workByArea = new Map(discovered.map((d) => [d.area, d.worklist || []]))
log('Discover: ' + discovered.reduce((n, d) => n + (d.worklist || []).length, 0) + ' work item(s) across ' + discovered.length + ' area(s)')

// --- [REBIND]
phase('Rebind')
const rebound = (await pool(AREAS, CAP, (a) => agent(rebindPrompt(a, workByArea.get(a.name) || []), { label: 'rebind:' + a.name, phase: 'Rebind', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [CRITIQUE]
// --- [REDTEAM]
phase('Critique')
phase('Redteam')
const reviewed = (await pipeline(BATCHES,
  (batch, _orig, i) => agent(critiquePrompt(batch, i), { label: 'critique:b' + i, phase: 'Critique', schema: FOLDER_FIXLOG, effort: 'xhigh', stallMs: STALL }),
  (crit, batch, i) => agent(redteamPrompt(batch, i, crit), { label: 'redteam:b' + i, phase: 'Redteam', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }),
)).filter(Boolean)

// --- [SWEEP]
phase('Sweep')
const swept = (await pool(SWEEP_ITEMS, CAP, (item) => agent(sweepPrompt(item), { label: 'sweep:' + (item.kind === 'codemap' ? 'codemap' : 'b' + item.i), phase: 'Sweep', schema: SWEEP_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [RESOLVE]
const allResiduals = dedup([
  ...residualsOf(rebound, () => BIM),
  ...residualsOf(reviewed, () => BIM),
  ...residualsOf(swept, () => BIM),
])
const filesByClaim = new Map(allResiduals.map((r) => [r.claim, r.files]))
const keyOf = (r) => r.files.slice().sort().join(',') + '|' + r.claim
const seen = new Set(allResiduals.map(keyOf))
let pending = allResiduals.slice()
let invalid = []
let noFix = []
let round = 0
let sanity = null
let sanityOpen = []
if (allResiduals.length) {
  phase('Resolve')
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = cluster(pending)
    log('Resolve round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (Bim-wide, no-defer)')
    const resolved = (await pool(clusters, CAP, async (cl) => {
      const fix = await agent(reconcileFix(cl), { label: 'resolve-fix:r' + round, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL })
      const touched = (fix && Array.isArray(fix.files) ? fix.files.filter(inLibs) : [])
      // No file-changing progress: the fix found nothing to change -> the cluster is resolved-or-phantom; skip the mandatory verify and drop it (recorded as noFix).
      if (!fix || touched.length === 0 || fix.verdict === 'clean') return { open: [], invalid: [], surfaced: [], dropped: cl, changed: false }
      const verify = await agent(reconcileVerify(cl, fix.files), { label: 'resolve-verify:r' + round, phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
      const claims = (verify && verify.claims) || []
      const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
      const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
      return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map((x) => norm(x, BIM)), dropped: [], changed: true }
    })).filter(Boolean)
    invalid = dedup([...invalid, ...resolved.flatMap((r) => r.invalid)])
    noFix = dedup([...noFix, ...resolved.flatMap((r) => r.dropped)])
    const invalidKeys = new Set(invalid.map((r) => r.claim))
    // Re-enter ONLY genuinely-new residuals: a key already queued this run cannot re-enter (stops a phantom re-feeding every round).
    const fresh = dedup([...resolved.flatMap((r) => r.open), ...resolved.flatMap((r) => r.surfaced)]).filter((r) => !invalidKeys.has(r.claim) && !seen.has(keyOf(r)))
    fresh.forEach((r) => seen.add(keyOf(r)))
    pending = fresh
    // NO-PROGRESS BREAK: no cluster changed a file this round -> the remaining residuals are phantom/unfixable; stop instead of grinding to MAX_ROUNDS.
    if (!resolved.some((r) => r.changed)) { log('Resolve round ' + round + ': no file-changing progress — ' + noFix.length + ' residual(s) had nothing to fix (phantom/resolved); breaking'); pending = []; break }
  }
  if (pending.length) log('Resolve reconcile: ' + pending.length + ' residual(s) still working after ' + MAX_ROUNDS + ' rounds — handed to the sanity drive-to-zero')
  else log('Resolve reconcile: all clustered residuals fixed + adversarially verified across ' + round + ' round(s)')
  // DRIVE TO ZERO: re-sanity over the union of all surfaced residuals after every force-close until NOTHING is open; SANITY_CAP is a runaway backstop, not a give-up.
  sanity = await agent(sanityPrompt(allResiduals), { label: 'sanity', phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
  sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
  let saneRound = 0
  while (sanityOpen.length && saneRound < SANITY_CAP) {
    saneRound++
    const prevOpen = sanityOpen.length
    const forceItems = dedup(sanityOpen.map((i) => ({ files: filesByClaim.get(i.claim) || [BIM], claim: i.claim })))
    const clusters = cluster(forceItems)
    log('Sanity round ' + saneRound + ': ' + sanityOpen.length + ' OPEN -> force-close per cluster (' + clusters.length + '), files preserved, then re-sanity; nothing leaves open')
    const forced = (await pool(clusters, CAP, (cl) => agent(reconcileFix(cl), { label: 'sanity-force-close:r' + saneRound, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
    const fcTouched = forced.flatMap((fc) => (fc && Array.isArray(fc.files) ? fc.files.filter(inLibs) : []))
    // The force-close changed nothing: the remaining items are phantom/unfixable; skip the re-audit and stop.
    if (fcTouched.length === 0) { log('Sanity round ' + saneRound + ': force-close changed no files — ' + sanityOpen.length + ' remaining item(s) phantom/unfixable; breaking'); break }
    sanity = await agent(sanityPrompt(allResiduals), { label: 'sanity:r' + saneRound, phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
    sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
    // No net decrease across the force-close -> drive-to-zero has stalled; stop instead of grinding to SANITY_CAP.
    if (sanityOpen.length >= prevOpen) { log('Sanity round ' + saneRound + ': no net progress (' + sanityOpen.length + ' open, was ' + prevOpen + ') — remaining item(s) phantom/unfixable; breaking'); break }
  }
  if (sanityOpen.length) log('Resolve: ' + sanityOpen.length + ' STILL OPEN after the force-close drive — HARD BLOCKER (likely an architectural decision), reported LOUDLY, never silently dropped')
  else log('Resolve: ALL ' + allResiduals.length + ' surfaced residual(s) CLOSED + sanity-verified')
} else { log('Resolve: no residuals surfaced — clean') }

return {
  workflow: 'bim-rebuild', areas: AREAS.map((a) => a.name), pages: PAGES.length, batches: BATCHES.length,
  rebindVerdicts: rebound.map((r) => ({ area: r.folder, verdict: r.verdict })),
  reviewVerdicts: reviewed.map((r, i) => ({ batch: i, verdict: r && r.verdict })),
  sweepVerdicts: swept.map((r) => r && r.verdict),
  surfacedResidual: allResiduals.length, resolveRounds: round,
  invalidClaims: invalid.map((x) => x.claim),
  noFix: noFix.map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: pending.map((x) => ({ files: x.files, claim: x.claim })),
  sanityOverall: sanity && sanity.overall,
  sanityOpen: sanityOpen.map((i) => ({ claim: i.claim, evidence: i.evidence })),
}
