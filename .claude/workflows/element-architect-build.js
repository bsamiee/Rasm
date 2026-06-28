export const meta = {
  name: 'element-architect-build',
  whenToUse: 'After element-api-authoring; stands up the Rasm.Element seam + rebuilds Bim/Materials/Persistence/Compute design corpora.',
  description: 'Per-folder BUILD of the Rasm.Element rebuild: author the new design pages (real full code fences, no placeholders), update the 3-level architecture docs, reconcile cross-folder deferrals, and align every seam. Per ELEMENT-REBUILD-PLAN.md; section 4-RT overrides section 4 on conflict. NOT a per-file 3-pass — that is the harden workflow.',
  phases: [
    { title: 'Build' },
    { title: 'Architecture' },
    { title: 'Reconcile' },
    { title: 'Align' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 6
const STAGGER_MS = 1500
const PLAN = 'ELEMENT-REBUILD-PLAN.md'

// --- [INPUTS] ----------------------------------------------------------------------------

const FOLDERS = [
  { name: 'Rasm.Element', root: 'libs/csharp/Rasm.Element', build:
    'NEW lowest-AEC seam package (refs ../Rasm ONLY). Author EVERY section-4D page (Graph/element+delta, Relations/relation, Classification/classification, ' +
    'Properties/property+quantity, Composition/material+acoustic, Assessment/assessment, Geospatial/coverage+reference, Projection/projection+address+fault) + ' +
    'governing README/ARCHITECTURE/IDEAS/TASKLOG + Rasm.Element.csproj (ProjectReference ../Rasm; PackageReference Thinktecture/LanguageExt/UnitsNet/QuikGraph/Mapperly/Generator.Equals). ' +
    'APPLY ALL section-4-RT overrides: NEUTRAL edge algebra Relationship (Compose|Assign|Associate|Connect|Void + typed payload + Generic passthrough, NOT 17 IfcRel cases) [C5]; ' +
    'PredefinedType is NOT here (Bim owns it); Object carries generic Classification(system,code) [C6 lives in Bim egress]; Associate edge carries LayerSet/ProfileSet usage payload [C7]; ' +
    'PropertySet/QuantitySet carry InheritanceMode [H1]; MeasureValue(quantityType, Si, canonicalUnit) over a Dimension value-object + UnitsNet QuantityType, named kinds as QTO accessors [H2]; ' +
    'ElementGraph carries the incidence index + memoized Bake [H3]; HAMT working graph vs Frozen read snapshot [H4]; rooted NodeId = neutral ULID/Guid (IFC GlobalId is a Bim attribute) [H6]; ' +
    'ONE canonical value codec ToCanonicalBytes() instance member on the Node union, shared by NodeId hash + diff [H7]; GeoReference full 12-tuple on Header/Coverage ONLY, dropped from Object [M1]; ' +
    'RepresentationContentHash keyed map (no IfcRepHash leak) [M2]; TWO interfaces — IElementProjection + IGraphConstraint [M3]; ContentAddress composes the kernel seed-zero XxHash128 [C7-hash].' },
  { name: 'Rasm.Bim', root: 'libs/csharp/Rasm.Bim', build:
    'REBUILD as projector + SOLE GeometryGym/IFC owner. Author Projection/semantic.md (SemanticProjector:IElementProjection — Project lowers GeometryGym->seam; Emit Bim-internal) + ' +
    'IGraphConstraint impl (IFC-semantic legality). KEEP IfcClass/IfcDomain/PredefinedType + the PropertyKey Pset/Qto roster + bSDD + QTO-override + base-qty; KEEP the FULL IfcRel* names/directionality/' +
    'inverse-semantics + the 8 stranded families in the projector (they ride the seam neutral payload) [C5]; PredefinedType egress gate -> BimFault.UnmappedClass [C6]; InheritanceMode stamped at ingress [H1]; ' +
    'IFC GlobalId stored as an Object attribute mapped 1:1 [H6]; IntroducedIn/RemovedIn schema-span + FILE_SCHEMA sniff [H8]; OwnerHistory + StepHeader [H9]; geospatial projector -> Object/Coverage + full GeoReference [M1]. ' +
    'RETIRE BimElement/BimModel + the stringly PropertyBinding/QuantityBinding + the seam-owned value half of Semantics/properties.md. csproj += Rasm.Element + Mapperly/Generator.Equals.' },
  { name: 'Rasm.Materials', root: 'libs/csharp/Rasm.Materials', build:
    'REBUILD as projector. Author Projection/material.md (MaterialProjector:IElementProjection, Project-only; authors element->material Associate edges WHEN given a non-empty element-NodeId set [H12]). ' +
    'Route the MaterialProperty unions into the seam Material/MaterialPropertySet + Assessment INPUT payloads; ProfileSet -> ProfileRef (one-hop section-property resolution [M7]); the intrinsic acoustic pure folds ' +
    'are now SEAM-owned (Rasm.Element/Composition/acoustic.md) — Materials references them, does not re-author; hand the multi-ply AssemblyAggregator to Compute; Appearance -> Appearance node content-keyed. ' +
    'RETIRE Materials.Element + MaterialAssignment. csproj += Rasm.Element.' },
  { name: 'Rasm.Persistence', root: 'libs/csharp/Rasm.Persistence', build:
    'FULL rebuild per section 4F + 4-RT. Marten is the APPEND SUBSTRATE BENEATH the PRESERVED op-log/CRDT/time-travel/StructuralMerge/causal-DAG engine — Version/ keeps that engine (re-keyed to NodeId/Relationship), ' +
    'NOT collapsed to ledger+merge [H11, operator Q1]. Stream-per-MODEL, event body = GraphDelta, inline SingleStreamProjection + AggregateSnapshot [C1]. Authoritative topology SYNCHRONOUS/co-txn; AGE/DuckDB async ' +
    'analytical lanes with staleness watermark; interactive-correctness queries block on WaitForNonStaleProjectionData [C2]; AGE optional self-hosted, DEFAULT topology = in-process QuikGraph + DuckDB columnar [H5]. ' +
    'Geometry object-store: write-blob-first + content-lineage/retention catalog + full-history GC + ONE txn owner for identity+event [H10]. SnapshotCodec composes the kernel XxHash128. Ingest/tabular.md = MiniExcel, ONE TabularSource. ' +
    'DuckDB INSTALL/LOAD documented in api-duckdb.md. csproj += Rasm.Element + Marten.' },
  { name: 'Rasm.Compute', root: 'libs/csharp/Rasm.Compute', build:
    'Author the C#-first ANALYSIS rail (new). Own the discipline-specific Assessment INPUT/RESULT shapes + the runners that read the concrete ElementGraph directly (above the seam, no interface): VividOrange + ' +
    'BriefFiniteElement/FEALiTE2D structural; NREL.OpenStudio 3.11 in-process model + EnergyPlus SUBPROCESS via a PARAMETERIZED discovery boundary (env-var -> configured-path -> bundled-fallback; dev/CI uses the ' +
    'Forge OPENSTUDIO_ENERGYPLUSDIR 25.2.0, NEVER the standalone 26.1.0) [section 4E]; the relocated multi-ply AssemblyAggregator (ISO 6946 series-U / ISO 12354 layered-STC / rule-of-mixtures); hand-rolled closed-form ' +
    'ISO/EN folds + EC3 REST + hand-rolled design-code checks. Write Assessment.Result nodes back content-keyed on (input,route). csproj += Rasm.Element.' },
]
const wanted = Array.isArray(args)
  ? args.map((x) => String(x).trim()).filter(Boolean)
  : (typeof args === 'string' && args.trim()) ? [args.trim()] : null
const normalizedWanted = wanted ? wanted.map((w) => w.toLowerCase()) : null
const TARGETS = !normalizedWanted || normalizedWanted.includes('all')
  ? FOLDERS
  : FOLDERS.filter((f) => normalizedWanted.some((w) => f.name.toLowerCase() === w || f.name.toLowerCase() === 'rasm.' + w))
if (!TARGETS.length) throw new Error('No folders matched args: ' + JSON.stringify(wanted))

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const BUILD_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'],
  properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['built', 'rebuilt', 'updated'] }, created: { type: 'array', items: { type: 'string' } }, retired: { type: 'array', items: { type: 'string' } }, residual: RESIDUAL, summary: { type: 'string' } } }
const ARCH_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'updated', 'summary'], properties: { verdict: { type: 'string', enum: ['updated', 'clean'] }, updated: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const ALIGN_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['aligned', 'clean'] }, aligned: { type: 'array', items: { type: 'string' } }, residual: RESIDUAL, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const BRIEF = [
  'CAMPAIGN: the Rasm.Element rebuild. READ ' + PLAN + ' (repo root) in FULL FIRST — it is the authoritative blueprint. Section 4-RT (RED-TEAM REVISIONS) ' +
    'OVERRIDES section 4A-H on any conflict; obey 4-RT. These are DESIGN-PAGE specs (.planning markdown), not compiled code.',
  'BINDING DOCTRINE: docs/stacks/csharp/** is the FLOOR (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, ' +
    'system-apis + the relevant domain/ shard). Thinktecture [Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject] + LanguageExt Fin/Validation/' +
    'Option/Eff are the established CORE rails. Apply the section/file-organization + section-order law.',
  'CAPABILITY SOURCES: mine BOTH the SHARED tier `libs/csharp/.api/**` AND the target folder`s own `.api/**` (the api-authoring workflow just filled the ' +
    'new ones) to full depth; compose external capability into canonical owners, never thin wrappers.',
  'LIBS DESIGN LAW (section 1A): lib-grade for countless consumers; each package usable in ISOLATION yet ALIGNED-not-coupled (peers never reference each ' +
    'other; alignment via the Rasm.Element contracts). Parameterize ingress AND egress. Python is a wire-aligned companion only.',
  'DESIGN-DOC OUTPUT LAW: author REAL, transcription-complete code fences — ZERO placeholder/stub/TODO, NO page/length cap; densify in place. Follow the ' +
    'conventions of sibling design pages + the canonical package-README structure BY EXAMPLE. "Page craft" is a docs/stacks/csharp doctrine concept ONLY and ' +
    'does NOT govern these design docs. ZERO spam, ZERO drift, no provenance/process/freshness prose, ground/root-up as if always-present.',
].join('\n')

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
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const buildPrompt = (f) => [BRIEF, '',
  'TASK: BUILD the design corpus of folder `' + f.name + '` (' + f.root + '). ' + f.build,
  'Read ' + PLAN + ' (sections 4, 4-RT, 5, 6 for this folder), docs/stacks/csharp, the SHARED libs/csharp/.api/, ' + f.root + '/.api/, and the CURRENT ' +
    f.root + '/.planning (the migration source). Then AUTHOR the new pages + retire the named owners + update this folder`s README roster + csproj. Write ' +
    'every file NOW. Edit ONLY files under ' + f.root + ' (cross-folder moves: author only YOUR side; log the other side as a residual). Return created + ' +
    'retired + residual {files,claim} for any genuine cross-folder seam you cannot resolve from this folder alone.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Build')
const built = (await pool(TARGETS, CAP, (f) => agent(buildPrompt(f), { label: 'build:' + f.name, phase: 'Build', schema: BUILD_SCHEMA, effort: 'max', stallMs: 600000 }))).filter(Boolean)
log('Build: ' + built.length + '/' + TARGETS.length + ' folders')

// --- [ARCHITECTURE]

phase('Architecture')
const architectureTargets = TARGETS.map((t) => t.root + '/ARCHITECTURE.md')
const arch = await agent([BRIEF, '',
  'TASK: update the architecture docs so the strata/seams/roster are TRUTHFUL for the new Rasm.Element sub-stratum + the rebuilt folders. Apply the section-4A ' +
    'STRATA AMENDMENT (Rasm.Element is a sanctioned new lowest-AEC sub-stratum; AEC peers now depend up on {Rasm, Rasm.Element}; alignment-without-coupling, ' +
    'the same shape as the kernel). EDIT: (1) libs/.planning/architecture.md (add Rasm.Element to the strata + package roster + the amended dependency clause); ' +
    '(2) libs/.planning/planning-targets.md (register Rasm.Element); (3) libs/csharp/.planning/ARCHITECTURE.md (C# stack map + [02]-[SEAMS]); (4) each rebuilt ' +
    'package ARCHITECTURE.md [02]-[SEAMS]: ' + architectureTargets.join(', ') + '. Mirror every cross-package seam in BOTH ' +
    'endpoints. Read ' + PLAN + ' sections 4A + 6. Write the edits NOW.',
  'BUILT FOLDERS:\n' + JSON.stringify(built.map((b) => ({ folder: b.folder, created: b.created })), null, 1)].join('\n'),
  { label: 'architecture', phase: 'Architecture', schema: ARCH_SCHEMA, effort: 'xhigh', stallMs: 300000 })

// --- [RECONCILE]

const rootsByFolder = new Map(FOLDERS.map((f) => [f.name, f.root]))
const norm = (x, folder) => ({ files: x.files && x.files.length ? x.files : [rootsByFolder.get(folder) ?? folder], claim: x.claim })
const allRes = built.flatMap((b) => (b.residual || []).map((x) => norm(x, b.folder)))
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = cluster(uniq)
log('Reconcile: ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters')
phase('Reconcile')
const reconciled = clusters.length ? (await pool(clusters, CAP, async (cl) => {
  const fix = await agent([BRIEF, '', 'TASK: RECONCILE these cross-folder build deferrals. There is NO severity — every residual is must-address. Read EVERY ' +
    'listed file; if it is a real cross-folder defect FIX it in place (unify the shared seam/type/contract, repair a strata/boundary issue, complete a ' +
    'cross-folder move), preserving all capability; if a residual is factually wrong, leave it and say why. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n'),
    { label: 'reconcile-fix', phase: 'Reconcile', schema: FIX_SCHEMA, effort: 'max', stallMs: 300000 })
  if (!fix) return null
  const verify = await agent([BRIEF, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — read the named files from disk and classify each residual fixed/' +
    'invalid/open (default open on doubt). Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles touched: ' + JSON.stringify(fix.files)].join('\n'),
    { label: 'reconcile-verify', phase: 'Reconcile', schema: VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
  return { fix, verify }
})).filter(Boolean) : []
const openClaims = reconciled.flatMap((r) => (r.verify && r.verify.claims || []).filter((c) => c.status === 'open').map((c) => c.claim))

// --- [ALIGN]

const fullRun = TARGETS.length === FOLDERS.length
phase('Align')
const SEAMS = fullRun ? [
  { key: 'projection-seam', focus: 'The IElementProjection + IGraphConstraint contracts in Rasm.Element/Projection vs their implementations: Bim SemanticProjector, Materials MaterialProjector (+ the Fabrication stub + the wire Assemble fold). Signatures, ProjectionContext shape, GraphDelta, the both-ids Associate rule [H12] — one shared shape, mirrored in both ARCHITECTURE [02]-[SEAMS].' },
  { key: 'content-key-seam', focus: 'The GeometryRef/RepresentationContentHash + ContentAddress content-key seam across Rasm (kernel seed-zero XxHash128 entry), Rasm.Element, Rasm.Persistence (Store/object-store + SnapshotCodec), and the canonical value codec [H7]. One hasher, one canonical-byte projection shared by NodeId hash + diff.' },
  { key: 'ripple-rename', focus: 'Surgical seam renames in the RIPPLE folders that consume the retired owners: Rasm.Fabrication (ref + later 3rd projector), Rasm.AppHost (stays reference-light + ArchUnitNET no-GeometryGym-below-seam rule), Rasm.AppUi (pure-UI, NO wire composition), Rasm.Rhino + Rasm.Grasshopper (consume seam Element/ElementGraph at the wire = the composition-root owners). No BimElement/Materials.Element references remain.' },
  { key: 'python-wire', focus: 'libs/python wire-alignment ONLY (no Materials/Bim/Persistence reference): align the IFC->IfcOpenShell->GLB companion + interchange decoders to the seam GeometryRef content-key (one XxHash128 seed) + the typed Material/Property/Assessment/Classification wire vocabulary. Decode, never re-mint. Add a float-bearing parity golden vector [H7].' },
] : []
const aligned = (await pool(SEAMS, CAP, (s) => agent([BRIEF, '',
  'TASK: ALIGN this cross-folder seam in place so both endpoints carry ONE shared shape with no drift; mirror it in both ARCHITECTURE [02]-[SEAMS] where applicable. ' +
    'SEAM: ' + s.focus + ' Read ' + PLAN + ' (sections 4, 4-RT, 6). Fix in place; do not trample owner interiors. Return aligned + any residual {files,claim}.'].join('\n'),
  { label: 'align:' + s.key, phase: 'Align', schema: ALIGN_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)

return {
  built: built.map((b) => ({ folder: b.folder, verdict: b.verdict, created: (b.created || []).length, retired: (b.retired || []).length })),
  architecture: arch && arch.updated, reconciled: reconciled.length, openClaims,
  aligned: aligned.map((a) => ({ verdict: a.verdict, aligned: a.aligned })),
}
