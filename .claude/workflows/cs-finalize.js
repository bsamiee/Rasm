export const meta = {
  name: 'cs-finalize',
  whenToUse: 'The final libs/csharp gap-closure + capability-completion + full-libs seam/boundary reconcile pass for the Rasm.Element campaign.',
  description: 'FINAL campaign pass over the touched libs/csharp folders (Element, Bim, Materials, Persistence, Compute). A real DISCOVERY phase — 1 opus agent per folder re-reads ELEMENT-REBUILD-PLAN.md (sections 4/4-RT/5/6) + the folder .planning/.api/README/.csproj + Directory.Packages.props and returns a STRUCTURED GAP LIST (missing central pins, missing/thin .api catalogs, admitted packages not yet composed into the design, missing newest-version confirmation, design/capability gaps, seam/boundary gaps, ARCHITECTURE/README untruths). Then PINS (one agent hand-edits Directory.Packages.props: every needed pin at the newest stable via the nuget MCP, net10 TFM, commercial-safe, grouped; dotnet restore to confirm) -> CLOSE (1 agent/folder, batched, parallel: author/deepen the .api catalogs via assay api, INTEGRATE each package capability into the owning design page per ROOT_REBUILD, update README roster + .csproj, close the design/capability gaps) -> CRITIQUE -> REDTEAM (adversarial per-folder, FIX in place, ultra-stack both .api tiers, corrective usage, universal docs/stacks/csharp conformance) -> SWEEP (FULL libs/ pass: every cross-folder/cross-stack seam + boundary corrected across ALL libs incl. AppHost/AppUi/Rhino/Grasshopper/Fabrication + the libs/python wire, design doc + code fence in any language, every folder ARCHITECTURE.md + README.md truthful — this ABSORBS the standalone boundary-ripple step) -> RESOLVE (terminal lib-wide no-defer drive-to-zero: every residual any phase surfaced is fixed + adversarially verified, looped until nothing is open). CAP 8. Takes no args.',
  phases: [
    { title: 'Discover' },
    { title: 'Pins' },
    { title: 'Close' },
    { title: 'Critique' },
    { title: 'Redteam' },
    { title: 'Sweep' },
    { title: 'Resolve' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 8
const STAGGER_MS = 1500
const STALL = 600000
const MAX_ROUNDS = 6
const PLAN = 'ELEMENT-REBUILD-PLAN.md'
const PROPS = 'Directory.Packages.props'
const FOLDERS = [
  { name: 'Element', root: 'libs/csharp/Rasm.Element' },
  { name: 'Bim', root: 'libs/csharp/Rasm.Bim' },
  { name: 'Materials', root: 'libs/csharp/Rasm.Materials' },
  { name: 'Persistence', root: 'libs/csharp/Rasm.Persistence' },
  { name: 'Compute', root: 'libs/csharp/Rasm.Compute' },
]
const LIBS_SWEEP = 'EVERY libs/ folder: the five above PLUS libs/csharp/Rasm (kernel), Rasm.AppHost, Rasm.AppUi, Rasm.Fabrication, Rasm.Rhino, Rasm.Grasshopper, and the libs/python wire (decode-not-remint)'

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const GAP = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['kind', 'detail'], properties: { kind: { type: 'string', enum: ['pin', 'api', 'integrate', 'design', 'capability', 'seam', 'doc'] }, package: { type: 'string' }, detail: { type: 'string' }, files: { type: 'array', items: { type: 'string' } } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'gaps', 'summary'], properties: { folder: { type: 'string' }, gaps: GAP, pinsNeeded: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package'], properties: { package: { type: 'string' }, current: { type: 'string' }, note: { type: 'string' } } } }, summary: { type: 'string' } } }
const PINS_SCHEMA = { type: 'object', additionalProperties: false, required: ['pinned', 'verdict', 'summary'], properties: { pinned: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'version'], properties: { package: { type: 'string' }, version: { type: 'string' } } } }, verdict: { type: 'string', enum: ['pinned', 'none'] }, restore: { type: 'string' }, summary: { type: 'string' } } }
const FOLDER_FIXLOG = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['closed', 'hardened', 'refined', 'clean'] }, integrated: { type: 'array', items: { type: 'string' } }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['aligned', 'clean'] }, aligned: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: the Rasm.Element rebuild — this is the FINAL gap-closure + capability-completion + full-libs reconcile pass. READ ' + PLAN + ' (repo root) in FULL FIRST — section 4-RT is AUTHORITATIVE over ' +
    '4A-H; section 5 is the PACKAGE ADMISSION REGISTER (each NEW package lands FULLY: central ' + PROPS + ' pin + folder README roster row + folder .csproj PackageReference + a FULL .api/ catalog; .api tier ' +
    'rule: cross-cutting SUBSTRATE -> shared libs/csharp/.api/, folder-specific -> that folder .api/); section 6 is the ripple/seam map. These are DESIGN-PAGE specs (.planning markdown) + their .api catalogs + ' +
    'the central manifest. The settled architecture (Rasm.Element seam, IElementProjection + IGraphConstraint, neutral edge algebra, typed value vocabulary, Marten-as-append-substrate-beneath-the-CRDT-engine) is ' +
    'CANONICAL. CLAUDE.md WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward).',
  'MANDATORY STANDARDS: docs/stacks/csharp/** is the FLOOR (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis + the relevant domain/ shard). EVERY stage ' +
    'ACTUALLY READS the core pages + the relevant domain/ shard(s) and conforms UNIVERSALLY (EXPRESSION_SPINE / BOUNDARY_ADMISSION / SHAPE_BUDGET / MODAL_ARITY / OWNER_CHOOSER / RAILS / two-weave ASPECTS / ' +
    'COLLAPSE_SCAN / DERIVED_LOGIC) — a GENERAL doctrinal harden, not just seam-alignment. Cite ONLY host/NuGet members verified via `uv run python -m tools.assay api` (a member you cannot verify is a phantom).',
  'WRITE-FULLY MANDATE: every gap/fix you identify you CLOSE NOW via Edit/Write; the returned log REPORTS edits already made, never a to-do. Leave nothing behind except genuine cross-FOLDER items (report those ' +
    'in residual_high for the terminal Resolve, which has NO scope cap).',
].join('\n')
const GAPLAW = [
  'GAP DISCIPLINE (the distinguishing mandate of this pass): the prior hardens were per-folder and time-boxed; this pass HUNTS the work that was NOT done. For EACH admitted package in section 5 + the ALREADY-PINNED ' +
    '& COMPOSED roster, verify the FULL admission landed: (1) PIN — present in ' + PROPS + ' at the newest stable (confirm via the nuget MCP `get_latest_package_version`/`get_package_context`); (2) .api — a FULL, ' +
    'non-phantom .api/ catalog in the correct tier (shared libs/csharp/.api/ for substrate, folder .api/ for folder-specific), every member assay-verified; (3) README roster row + .csproj PackageReference present; ' +
    '(4) INTEGRATED — the capability is actually COMPOSED into the owning design page (grown into the owner per ROOT_REBUILD), not merely catalogued or named in prose. A package pinned-but-uncomposed, an .api that ' +
    'is thin/phantom, or a design page that ignores an admitted capability the concept ADMITS is a GAP to close. PG server extensions (pg_duckdb/timescaledb-toolkit/postgis_raster/postgis_sfcgal) are provisioned ' +
    'server-side (not nuget) — document, do not pin. MagmaWorks is DEFERRED (not on nuget.org). Beyond packages, hunt design gaps (a naive/thin owner), seam gaps (a cross-folder contract one side does not honor), ' +
    'and doc untruths (an ARCHITECTURE.md codemap/[02]-[SEAMS] or README roster that no longer matches the pages).',
  'PINS land in ONE place: the central ' + PROPS + ' is hand-edited (label-grouped by owner, newest stable, net10 TFM, commercial-safe), NEVER `dotnet add`; confirm the graph with `dotnet restore`. Per-folder ' +
    'agents NEVER edit ' + PROPS + ' (the single Pins agent owns it — no concurrent manifest writes); folder agents own their .api/ + .planning/ + README + .csproj only.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY implementing stage (close, critique, redteam, sweep, resolve) is HOSTILE: assume the corpus is INCOMPLETE / NAIVE / ILLUSORY until it survives an aggressive attack; the burden of proof ' +
    'is ON THE PAGE. "Mature"/"good enough"/a prior clean verdict are REJECTED. A no-edit verdict is earned ONLY after a genuine attack + a real domain/package/consumer sweep finds nothing.',
  'ILLUSORY/FAKE is the PRIMARY target — a page that USES the doctrine vocabulary and reads dense yet is HOLLOW (a name/prose promising capability the body lacks; a thin slice of a rich concept; a cited but ' +
    'unverifiable .api/host member — a phantom; an admitted package named in prose but never composed into a fence). Treat dense confident pages with MORE suspicion; disbelieve every self-claim until verified.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the named laws of docs/stacks/csharp/README.md held as fact: EXPRESSION_SPINE + BOUNDARY_ADMISSION; SHAPE_BUDGET + DEEP_SURFACES + MODAL_ARITY + ANTICIPATORY_COLLAPSE + INTERFACE_SEAM; ' +
    'POLICY_VALUES + DERIVED_LOGIC/TYPES + SEMANTIC_NAMING; LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS; ROOT_REBUILD + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION.',
  'COLLAPSE MANDATE: collapse >=3 parallel types / sibling factories / repeated switch arms / single-call helpers into ONE polymorphic owner IN THE SAME FILE via [Union]/[SmartEnum<TKey>]/[ValueObject<T>]/' +
    '[ComplexValueObject]/source-generated case families/Fold algebra/frozen tables — never extract a new file to reduce LOC, never delete capability.',
  'MAX-STACK BOTH .api TIERS TO EXHAUSTION: mine the ENTIRE folder .api/ catalog set (for a large .api folder such as Rasm.Persistence ~80 catalogs: the event/store/columnar/index/wire/codec/compression/vector/geo/' +
    'messaging/kms families) AND the full shared libs/csharp/.api/ tier, layered with the universal Thinktecture/LanguageExt/QuikGraph/Riok.Mapperly/Generator.Equals substrate. Compose EVERY relevant admitted member ' +
    'into single dense owners (generated owners, Fold algebra, data tables) at the DEEPEST surface each package reaches; reject thin wrappers + BCL-first reflexes + a surface-level subset. A composed owner that leaves ' +
    'an admitted .api capability the concept ADMITS unexploited (ESPECIALLY a NEWLY-pinned package) is a CAPABILITY-INCOMPLETENESS defect to close. Verify novel members via `uv run python -m tools.assay api`.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam): structural collapse + .api-stacking are NECESSARY but NOT SUFFICIENT — a fully-collapsed owner can still model a NAIVE slice. Close the gap by GROWING ' +
    'the existing owner (a case in the closed family, a row/richer data on the smart-enum, a field/composed value-object, an operation, a policy value) per ROOT_REBUILD + COMPOSED_IMPLEMENTATION — never a parallel ' +
    'surface or a new file. Every extension cites ONE source: a PACKAGE member (assay-verified), a DOMAIN attribute, or a CONSUMER contract. CORRECTIVE USAGE: where a page composes a package member WRONGLY (wrong ' +
    'overload, a phantom signature, a misused rail), FIX the usage to the verified-correct surface — incorrect composition is as much a defect as a missing one.',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy; capability COVERAGE against the full domain + .api surface is the measure. A confident-LOOKING page is the PRIME suspect for a thin slice. Assess each owner against ' +
    'its concept INDEPENDENTLY of size and EXTEND every owner the concept under-realizes in place; if after a real sweep the concept is genuinely complete, prove it by adding nothing — never pad.',
].join('\n')
const SEAM = [
  'SECTION-4-RT INVARIANTS (preserve, never regress): Relationship is a NEUTRAL edge algebra (Compose|Assign|Associate|Connect|Void + typed payload + Generic passthrough), NOT 17 IfcRel cases (IFC schema lives in ' +
    'Bim`s projector); PredefinedType a typed Object field with a Bim egress gate; the Associate edge carries LayerSet/ProfileSet usage; MeasureValue is the 4-arg (QuantityType, Dimension, double Si, string ' +
    'CanonicalUnit) over a Dimension value-object + UnitsNet; ElementGraph carries the incidence index + memoized Bake with a HAMT working graph vs Frozen read snapshot; rooted NodeId is a neutral kernel id ' +
    '(NodeId.Rooted()), IFC GlobalId a Bim attribute; ONE canonical codec (ToCanonicalBytes/CanonicalWriter) shared by NodeId-hash + diff; content id is NodeId.Content(self-hash); TWO seam interfaces ' +
    'IElementProjection + IGraphConstraint; Marten is the append substrate BENEATH the preserved op-log/CRDT/time-travel/StructuralMerge engine; stream-per-MODEL + GraphDelta event body; topology synchronous ' +
    '(inline read-your-writes projection), AGE/DuckDB async analytical lanes; geometry object-store write-blob-first + one-txn identity+event. A page contradicting a 4-RT invariant is a defect to fix TOWARD 4-RT.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely (no weak/erased types); NO exception control flow in domain logic (LanguageExt Fin/Validation/Option/Eff rails / ROP); NO imperative branching where a bounded ' +
    'vocabulary / frozen table / generated Switch / Fold owns the variation; NO mutable accumulation; total generated Switch with compile-time exhaustiveness (no silent _); fault = a CLOSED [Union] deriving from the ' +
    'kernel Expected. Latest C# 14 on net10 to the metal. LanguageExt.Core 5.0.0-beta: Map/Choose/Filter/Fold/ToSeq exist ONLY on LanguageExt types — wrap a raw ImmutableArray/FrozenDictionary.Values in toSeq(...) ' +
    'before a combinator; `.HeadOrNone()` is removed (use the Seq.Head Option property).',
  'STRATA/BOUNDARY + SEMANTIC_NAMING: each owner stays on its stratum, depends strictly upward; geometry/mesh/IFC meet at the wire with one owner per runtime; no host-type leak into a host-neutral owner; one ' +
    'bounded-context term per concept; ONE_HOP_RESOLUTION (no alias chains / forwarding helpers / single-call helpers). Apply the docs/stacks/csharp file-organization + section-order law.',
].join('\n')
const PROSE = 'PROSE + FENCES: design-SPEC prose only — lead with the controlling rule, one idea per paragraph, close on the consequence; cut hedges/provenance/process narration. REAL transcription-complete code ' +
  'fences, ZERO placeholder/stub/TODO. BACKTICK every symbol/type/member/path/package. Keep the canonical section-divider headers; otherwise agent-facing comments only where intent is not obvious (default zero). ' +
  '("Page craft" is a docs/stacks/csharp concept ONLY; it does not govern these design docs.)'
const DOCTRINE = [LAW, '', GAPLAW, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', SEAM, '', PATLAW, '', PROSE].join('\n')

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
const discoverPrompt = (f) => [DOCTRINE, '', 'TASK: GAP DISCOVERY for `' + f.name + '` (folder `' + f.root + '/**`). READ-ONLY — investigate, do NOT edit. Re-read ' + PLAN + ' (sections 4/4-RT/5/6), EVERY ' +
  'design page under `' + f.root + '/.planning/**`, EVERY catalog under `' + f.root + '/.api/**` + the shared `libs/csharp/.api/**`, the folder `README.md`/`ARCHITECTURE.md`/`.csproj`, and `' + PROPS + '`. ' +
  'Produce the COMPLETE gap list for this folder: (a) PACKAGE admission gaps — for each section-5 package + the already-pinned roster this folder composes, is it PINNED (check ' + PROPS + '; confirm newest via the ' +
  'nuget MCP — ToolSearch `mcp__nuget__get_latest_package_version`/`get_package_context`), does it have a FULL non-phantom .api catalog in the right tier, a README roster row + .csproj reference, and is it actually ' +
  'COMPOSED into the owning design page (not just catalogued)?; (b) DESIGN gaps — a naive/thin owner, an admitted capability the concept ADMITS but the page ignores, a wrong/phantom .api usage (corrective); (c) SEAM ' +
  'gaps — a cross-folder contract (the Rasm.Element seam, the content-key wire, the projector interfaces) this folder does not honor; (d) DOC untruths — ARCHITECTURE.md codemap/[02]-[SEAMS] or README roster that no ' +
  'longer matches the pages. Verify novel members via `uv run python -m tools.assay api`. Return gaps (each {kind, package?, detail, files}) + pinsNeeded (packages missing/stale in ' + PROPS + ', with current version + ' +
  'note) + a summary of the dominant gap class.'].join('\n')
const pinsPrompt = (pins) => [LAW, '', GAPLAW, '', 'TASK: CENTRAL PIN ADMISSION. The discovery agents found these packages missing or stale in `' + PROPS + '`:\n' + JSON.stringify(pins, null, 1) + '\nFor EACH: ' +
  'confirm the NEWEST stable version via the nuget MCP (ToolSearch `mcp__nuget__get_latest_package_version` + `get_package_context` for net10.0 TFM + commercial-safe license), then HAND-EDIT `' + PROPS + '` to add/' +
  'update the `<PackageVersion>` in the correct label-grouped cluster (sorted, one-line maintenance comment only) — NEVER `dotnet add`. Skip PG server extensions (provisioned server-side, not nuget) and any package ' +
  'a discovery agent flagged DEFERRED/not-on-nuget.org. Then run `dotnet restore` and confirm the graph resolves (a restored assembly is required so the Close phase`s `assay api` can decompile it). Return pinned ' +
  '(each {package, version} you actually wrote) + verdict + the restore result + summary. Edit ONLY `' + PROPS + '`.'].join('\n')
const closePrompt = (f, gaps) => [DOCTRINE, '', 'TASK: CLOSE the discovered gaps for `' + f.name + '` (folder `' + f.root + '/**`; the central pins are already in ' + PROPS + ' from the Pins phase, assemblies ' +
  'restored). For EACH gap below: CLOSE it in place to the strongest clean/modern form. PACKAGE gaps -> author/deepen the FULL .api catalog in the correct tier (every member assay-verified, matching the existing ' +
  '.api convention), add the README roster row + .csproj PackageReference, and INTEGRATE the capability into the OWNING design page (grow the owner per ROOT_REBUILD + COMPOSED_IMPLEMENTATION + MAX-STACK — never a ' +
  'parallel surface). DESIGN/CAPABILITY gaps -> grow the owner / fix the wrong usage. Edit pages under `' + f.root + '/**` + the shared `libs/csharp/.api/**` substrate catalogs this folder owns; a fix REQUIRING ' +
  'another folder goes to residual_high {files, claim}. GAPS:\n' + JSON.stringify(gaps, null, 1) + '\nReturn folder + verdict + integrated (each package woven in + where) + extended + residual_high + summary.'].join('\n')
const critiquePrompt = (f) => [DOCTRINE, '', 'TASK: HOSTILE DOCTRINAL + CAPABILITY-COMPLETENESS + INTEGRATION AUDIT + FIX IN PLACE across `' + f.name + '` (`' + f.root + '/**`). You are an ULTRA-HARSH, UNAGREEABLE ' +
  'auditor AND an implementer — you REPAIR/COLLAPSE/EXTEND in place. Process the folder at full depth; assume a violation in EVERY fence until proven otherwise; trust nothing the prose claims. Read the folder pages, ' +
  'BOTH .api tiers, the operative docs/stacks/csharp pages + the relevant domain/ shard(s). Run the mechanical checklists and REPAIR every hit: (1) COLLAPSE_SCAN (3+ parallel shapes/sibling names/repeated arms -> one ' +
  'owner); (2) OWNER_CHOOSER per shape (kill parallel DTOs/one-field wrappers/nullable-as-failure/default-ghosts); (3) KNOB_TEST (flags -> policy values/input-shape; timeout/retry/token -> carrier/aspect); (4) ' +
  'two-weave ASPECTS; (5) RAILS (narrowest carrier, closed Expected fault, total Switch, no exception control flow, LanguageExt-v5 combinator correctness); (6) STRATA/MEMBERS/MODERN (depend upward, no phantom member, ' +
  'C#14/net10, FULL docs/stacks/csharp + domain shard, BOTH .api tiers MAXIMIZED); (7) INTEGRATION + CAPABILITY-COMPLETENESS + ILLUSION (EVERY admitted/newly-pinned package actually COMPOSED into a fence not just ' +
  'named; a collapsed owner can still be a naive slice — grow it; CORRECTIVE usage — fix wrong/phantom package composition; delete decorative/speculative padding); (8) SECTION-4-RT invariants. Edit the `' + f.name + ' ' +
  'pages to fix every hit; cross-folder items -> residual_high {files, claim}. Return folder + verdict + extended + residual_high + summary.'].join('\n')
const redteamPrompt = (f) => [DOCTRINE, '', 'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE across `' + f.name + '` (`' + f.root + '/**`) — the LAST + most aggressive per-folder pass; trust nothing the close/' +
  'critique claimed. Open BOTH .api tiers + the universal rails, the sibling pages, docs/stacks/csharp + the domain/ shard, ' + PLAN + '. (A) COUNTERFACTUAL on each core owner — is it categorically the strongest the ' +
  'doctrine admits, or does a denser owner / a DEEPER admitted-package primitive collapse the fence? rebuild to it. (B) ANTICIPATORY_COLLAPSE — the next case/dimension/provider lands as ONE case/row/policy value with ' +
  'consumers broken LOUDLY at compile time (total Switch, no silent _). (C) LONG-TAIL — every input/output/edge/failure mode; both ingress AND egress parameterized. (D) STRATA + BOUNDARY-INTEGRITY — no downward dep, ' +
  'no host-type leak, no concern owned twice, geometry/mesh/IFC at ONE wire owner, no coupling to a sibling INTERIOR. (E) SURFACE-SPRAWL + PHANTOMS — collapse hand-rolled code the .api tiers own; delete unverifiable ' +
  'members; an admitted package re-derived by hand is a defect. (F) INTEGRATION + CAPABILITY-COMPLETENESS — every newly-pinned package genuinely composed; attack each owner for domain-completeness; grow it in place. ' +
  '(G) SECTION-4-RT FIDELITY. ALSO a FULL COLD RE-REVIEW of every critique dimension. The folder MUST end objectively denser, MORE CAPABLE, more correct; if the strongest form is present, prove it by finding nothing. ' +
  'Edit ONLY `' + f.name + '` pages; cross-folder -> residual_high {files, claim}. Return folder + verdict + extended + residual_high + summary.'].join('\n')
const sweepPrompt = () => [DOCTRINE, '', 'TASK: FULL-LIBS SWEEP — the whole-stack seam/boundary alignment + truthfulness pass across ' + LIBS_SWEEP + '. This ABSORBS the boundary-folder ripple step. (1) SEAMS/WIRES — ' +
  'every cross-folder/cross-stack producer/consumer relationship the campaign created is aligned to ONE shared shape on BOTH endpoints and recorded in both folders ARCHITECTURE.md [02]-[SEAMS] with mirrored glyphs: ' +
  'IElementProjection/IGraphConstraint <-> the Bim/Materials projectors; the GeometryRef/RepresentationContentHash/ContentAddress content-key <-> Persistence <-> the Rasm kernel; the typed Material/Property/Assessment/' +
  'Classification wire vocabulary <-> the libs/python decoders (decode-not-remint, never re-mint the C# contract). (2) BOUNDARY FOLDERS — confirm Rasm.AppHost stays reference-light (no AEC-domain ref), Rasm.AppUi ' +
  'stays pure-UI (no AEC ref), Rasm.Rhino/Rasm.Grasshopper consume the seam Element/ElementGraph at the wire (not the retired BimElement/Materials.Element), Rasm.Fabrication references Rasm.Element; FIX any that ' +
  'consume a retired shape. (3) NO DUPLICATION / NO STRATA VIOLATION — a concept owned twice collapses to its rightful stratum owner; depend strictly upward; geometry/mesh/IFC at ONE wire owner per runtime. (4) DOC ' +
  'TRUTH — every touched folder ARCHITECTURE.md (codemap + [02]-[SEAMS]) + README roster is truthful to its pages; libs/.planning/architecture.md + libs/csharp/.planning/ARCHITECTURE.md reflect the Rasm.Element ' +
  'stratum. Fix in place across folders (design doc AND code fence, in any language — C#, Python); a counterpart edit you cannot complete goes to residual_high {files, claim}. Return verdict + aligned (each ' +
  'alignment, naming the folders + seam) + residual_high + summary.'].join('\n')
const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals the close/critique/redteam/sweep phases surfaced; NO severity, NO leftovers, NO deferral, NO scope ' +
  'cap. Read EVERY listed file across libs/ (csharp + py) + ' + PROPS + ' and FIX the real cross-file defect in place to the strongest clean/modern + 4-RT form (align the seam + every consumer in lockstep, repair ' +
  'strata/boundary, finish a package admission spanning files, make a doc truthful), preserving all capability — a token patch that leaves the seam misaligned is NOT a fix; if a residual is FACTUALLY WRONG, leave it ' +
  'and say why. If your fix surfaces a new cross-file need, report it in residual_high. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [LAW, '', SEAM, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and CONFIRM the fix is ACTUALLY made AND complete ' +
  '+ high-quality + clean/modern/4-RT-conformant, not a token/naive patch. ATTACK it: shallow, partial, a rename that left a sibling stale, a seam still misaligned? Classify each: "fixed" (real, complete, non-naive), ' +
  '"invalid" (claim factually wrong — cite why), or "open" (NOT fixed or fixed naively — redo). Default "open" on ANY doubt. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('cs-finalize: gap-discovery (' + FOLDERS.length + ' folders) -> pins -> close -> critique -> redteam -> full-libs sweep -> terminal resolve; CAP=' + CAP)

// --- [DISCOVER]
phase('Discover')
const discovered = (await pool(FOLDERS, CAP, (f) => agent(discoverPrompt(f), { label: 'discover:' + f.name, phase: 'Discover', schema: DISCOVER_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
const gapsByFolder = new Map(discovered.map((d) => [d.folder, d.gaps || []]))
const allPins = dedup(discovered.flatMap((d) => (d.pinsNeeded || []).map((p) => ({ files: [PROPS], claim: p.package + (p.current ? ' (current ' + p.current + ')' : '') + (p.note ? ' — ' + p.note : '') }))))
log('Discover: ' + discovered.reduce((n, d) => n + (d.gaps || []).length, 0) + ' gaps across ' + discovered.length + ' folders; ' + allPins.length + ' pin(s) needed')

// --- [PINS]
phase('Pins')
if (allPins.length) await agent(pinsPrompt(allPins.map((p) => p.claim)), { label: 'pins', phase: 'Pins', schema: PINS_SCHEMA, effort: 'max', stallMs: STALL })
else log('Pins: none needed (every admitted package already pinned)')

// --- [CLOSE]
phase('Close')
const closed = (await pool(FOLDERS, CAP, (f) => agent(closePrompt(f, gapsByFolder.get(f.name) || []), { label: 'close:' + f.name, phase: 'Close', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [CRITIQUE]
phase('Critique')
const crit = (await pool(FOLDERS, CAP, (f) => agent(critiquePrompt(f), { label: 'critique:' + f.name, phase: 'Critique', schema: FOLDER_FIXLOG, effort: 'xhigh', stallMs: STALL }))).filter(Boolean)

// --- [REDTEAM]
phase('Redteam')
const red = (await pool(FOLDERS, CAP, (f) => agent(redteamPrompt(f), { label: 'redteam:' + f.name, phase: 'Redteam', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [SWEEP]
phase('Sweep')
const swept = (await pool([0], 1, () => agent(sweepPrompt(), { label: 'sweep:full-libs', phase: 'Sweep', schema: SWEEP_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [RESOLVE]
const residualsOf = (rows, key) => rows.flatMap((r) => (r.residual_high || []).map((x) => norm(x, key(r))))
let pending = dedup([
  ...residualsOf(closed, (r) => 'libs/csharp/Rasm.' + r.folder),
  ...residualsOf(crit, (r) => 'libs/csharp/Rasm.' + r.folder),
  ...residualsOf(red, (r) => 'libs/csharp/Rasm.' + r.folder),
  ...residualsOf(swept, () => 'libs/csharp'),
])
const keyOf = (r) => r.files.slice().sort().join(',') + '|' + r.claim
const seen = new Set(pending.map(keyOf))
let invalid = []
let noFix = []
let round = 0
if (pending.length) {
  phase('Resolve')
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = cluster(pending)
    log('Resolve round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (lib-wide, no-defer)')
    const resolved = (await pool(clusters, CAP, async (cl) => {
      const fix = await agent(reconcileFix(cl), { label: 'resolve-fix:r' + round, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL })
      const touched = (fix && Array.isArray(fix.files) ? fix.files.filter(inLibs) : [])
      // No file-changing progress: the fix found nothing to change -> the cluster is resolved-or-phantom; skip the mandatory verify and drop it (recorded as noFix).
      if (!fix || touched.length === 0 || fix.verdict === 'clean') return { open: [], invalid: [], surfaced: [], dropped: cl, changed: false }
      const verify = await agent(reconcileVerify(cl, fix.files), { label: 'resolve-verify:r' + round, phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
      const claims = (verify && verify.claims) || []
      const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
      const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
      return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map((x) => norm(x, 'libs/csharp')), dropped: [], changed: true }
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
  if (pending.length) log('Resolve: ' + pending.length + ' STILL OPEN after ' + MAX_ROUNDS + ' rounds — REPORTED, never silently dropped')
  else log('Resolve: all residuals fixed + adversarially verified across ' + round + ' round(s)')
} else { log('Resolve: no residuals surfaced — clean') }

return {
  workflow: 'cs-finalize', folders: FOLDERS.map((f) => f.name),
  gaps: discovered.map((d) => ({ folder: d.folder, gaps: (d.gaps || []).length })),
  pinsNeeded: allPins.length, closeVerdicts: closed.map((r) => ({ folder: r.folder, verdict: r.verdict })),
  sweepVerdict: swept[0] && swept[0].verdict, resolveRounds: round,
  invalidClaims: invalid.map((x) => x.claim), noFix: noFix.map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: pending.map((x) => ({ files: x.files, claim: x.claim })),
}
