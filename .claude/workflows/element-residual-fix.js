export const meta = {
  name: 'element-residual-fix',
  whenToUse: 'One-off: fix the 29 cross-file residuals the Element harden (old defer-flow) logged but did not fix.',
  description: 'Fix all 28 openResidual + 1 reconcileOpen from the Element harden wf_5196ae81-1e2 (many COMPILE-BREAK level). FOLDER-SCOPED batches in parallel (each owns one folder, no write-conflicts, aligns to the seam contract) -> a TERMINAL no-defer reconcile loop over cross-folder leftovers (fix -> adversarial verify -> loop until dry) -> ONE sanity-pass agent over ALL 29 items (re-read from disk, confirm each genuinely + cleanly fixed), whose open items feed one more reconcile round. Clean/modern C# 14/net10 per docs/stacks/csharp + the section-4-RT invariants; both .api tiers stacked; members verified via assay api. NO CodeRabbit. Takes no args.',
  phases: [
    { title: 'Fix' },
    { title: 'Reconcile' },
    { title: 'Sanity' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 8
const STAGGER_MS = 1500
const STALL = 600000
const MAX_ROUNDS = 5
const SANITY_CAP = 6
const PLAN = 'ELEMENT-REBUILD-PLAN.md'

// --- [INPUTS] ----------------------------------------------------------------------------
// Residuals embedded (deterministic; no discovery). Each batch owns ONE folder -> folder-disjoint, conflict-free.
// Full source-of-truth list: scratchpad/element-harden-residuals.md.

const BATCHES = [
  { name: 'Element', root: 'libs/csharp/Rasm.Element', claims: [
    'DANGLING ANCHOR `Composition/material#MATERIAL_NODE` (6+ occ: assessment.md 3x, quantity.md 1x, element.md ~13, relation.md ~12, coverage.md, acoustic.md) — material.md defines only `[02]-[MATERIAL_COMPOSITION]`/`[03]-[MATERIAL_PROPERTY]`, no MATERIAL_NODE. Owner fix in material.md: expose a MATERIAL_NODE anchor OR repoint all refs -> `#MATERIAL_COMPOSITION` (the section owning MaterialId/MaterialComposition). Pick ONE folder-wide.',
    'DANGLING ANCHOR `Projection/address#CANONICAL_WRITER` (12 refs corpus-wide; element.md, delta.md, material.md + 5 siblings) — address.md exposes only `[02]-[CONTENT_ADDRESS]` (CanonicalWriter is a [SERVICES] sub-block). CanonicalWriter is a distinct 12x-referenced codec owner co-equal with ContentAddress (README [14]-[ADDRESS] lists both): PREFER splitting address.md to expose `[03]-[CANONICAL_WRITER]` and retarget all citing pages; record the chosen convention in residual_high so consumer folders (Bim/Compute/Materials/Persistence) align. (Acceptable alternative: repoint all -> `#CONTENT_ADDRESS`.)',
    'LANGUAGEEXT v5 COMBINATOR COMPILE-BREAK (corpus-wide, owned by the Graph pages): LanguageExt.Core 5.0.0-beta-77 provides Map/Choose/Filter/Fold/ToSeq ONLY on its own types (Seq/Iterable/Arr/Lst/HashSet/Set) — NOT on raw ImmutableArray / FrozenDictionary.Values / IReadOnlyList; NO implicit ImmutableArray->Seq; `.HeadOrNone()` removed (use the `Seq.Head` Option property). element.md (Edges: ImmutableArray<Relationship>, Nodes: FrozenDictionary) calls `.Choose().ToSeq()`/`.Fold()`; delta.md `Edges.Filter/.Map/.Fold`; address.md `Nodes.Values.Map`/`.ToSeq()`; classification/material/property similar. RESOLVE uniformly in the type-owning Graph pages: PREFER exposing `Seq<Relationship>`/`Seq<Node>` typed read-accessors on `ElementGraph` (so consumers never combinator over the raw BCL surface; update api-quikgraph.md + flag the Persistence consumer in residual_high) — else wrap every BCL receiver in `toSeq(...)` before the combinator and use `.Head`.',
    'CONTENT-ADDRESS internals: (a) `ContentAddress.ByteOrder` is declared `private` (classification.md ~38) but delta.md `GraphDelta.ToCanonicalBytes` composes it to sort edge canonical bytes with the SAME comparer `ContentAddress.OfGraph` uses — change to `internal static readonly` (same-assembly), do NOT duplicate the comparer (DERIVED_LOGIC/one-owner). (b) The model-Header canonical-byte projection is duplicated byte-identically: `ContentAddress.HeaderBytes(w,h)` (address.md) + the `Header.IfSome` block (delta.md `GraphDelta.ToCanonicalBytes`). Collapse to ONE owner: add `public void CanonicalBytes(CanonicalWriter w)` to the `Header` record in element.md (writes Schema/View/Tolerance + `Reference.CanonicalBytes(w)`), have address.md `OfGraph` call `graph.Header.CanonicalBytes(w)` (delete the single-call private `HeaderBytes` helper), delta.md call `Header.IfSome(h => h.CanonicalBytes(w))`. Atomic across element.md + address.md + delta.md.',
    'FAULT FEDERATION list (Projection/address.md or fault.md prose) omits the Materials `ProjectionFault` (2470) and `ConnectionFault` (2360) that Materials material.md declares — add them to the federation enumeration for completeness. SEPARATELY, the kernel GeometryFault band collision (2450/2500 overlapping MaterialFault/ElementFault) is fixed in the KERNEL batch; Element fault.md correctly states the intended clean federation (2300/2350/2400/2450/2500/2600) and needs NO change once the kernel conforms.',
    'ASSESSMENT node-id contract (element.md + assessment.md, seam side): the `NodeId.OfContent` doc-comment in element.md must DROP the example claiming an Assessment InputKey "mints an assessment node id without re-hashing" — `OfContent` is valid only when its ContentAddress == ContentAddress.Of(node.ToCanonicalBytes()); an InputKey is a FOREIGN key, not the node self-hash. assessment.md must document the mint as `NodeId.Content(node.ToCanonicalBytes(tolerance))` (self-hash, computable pre-run) so a Compute-authored Assessment node passes ContentAddress.Verify on rehydrate. (Compute WriteBack consumer-side is the COMPUTE batch.)',
    'ENVIRONMENTAL DOUBLE-STORE contract (Composition/material.md, seam side): `MaterialPropertySet.Environmental` stores the A1-A3 GWP TWICE (the `GlobalWarmingPotential` MeasureValue field read by `Gwp`, AND `StageGwp[A1A3.Index]` read by `StageAt`/`WholeLifeGwp`) with no enforced equality — a DERIVED_LOGIC violation (acoustic.md derives every rating from its vector with no stored scalar). FIX: derive `Gwp => StageAt(LifecycleStage.A1A3)`, DROP the `GlobalWarmingPotential` field + its `Empty` arg + the CanonicalBytes `.Measure(...)` write + the `gwpKgCo2e` param on `OfEnvironmental`, add a `StageGwp` finiteness guard. Changing `OfEnvironmental`s signature breaks the Materials sustainability.md + Compute lifecycle.md call sites -> report those in residual_high so the consumer batches align in lockstep.',
    'SEAM SectionProperties (Composition/material.md) is the 17-field record with the warping constant `Iw` 5th (Area, Iyy, Izz, J, Iw, Wely, Welz, Wply, Wplz, AvY, AvZ, RadMajor, RadMinor, Depth, Width, HeatedPerimeter, AxisDistance) per EN 1993-1-1 §6.3.2 / AISC 360 Ch.F LTB input. CONFIRM the seam declares 17 fields with Iw 5th (the contract); the Materials `SeamSection` 16-arg builder is fixed in the MATERIALS batch to match. (If the seam is wrong, fix it here; if right, this is the contract the Materials batch aligns to.)',
    'reconcileOpen SYSTEMIC: confirm the inline `Object.BoundaryPolygon: Seq<Vector3>` + `Object.Axis: Option<AxisCurve>` analytical channel is dropped from element.md (already applied in the element.md rebuild) and that the Object node carries analytical axis/footprint ONLY by content key (Representations["Axis"]/["FootPrint"]); any seam page still referencing the inline channel is a defect.',
  ] },
  { name: 'Bim', root: 'libs/csharp/Rasm.Bim', claims: [
    'COMPILE-BREAK (Projection/semantic.md): `PropertyLowering.LowerValue`/`MeasureOpt`/`Measure` construct a 3-arg `new MeasureValue(dim, value, symbol)` — the seam `MeasureValue` is the 4-arg record `(QuantityType Type, Dimension Dimension, double Si, string CanonicalUnit)` (Element Properties/quantity.md, H2). The IFC quantity value is already SI-base: construct via `MeasureValue.OfSi(QuantityType, Dimension, double)` — map `IfcQuantityLength/Area/Volume/Weight/Time` -> `QuantityType.Length/Area/Volume/Mass/Duration`, generic `IfcMeasureValue` measures -> `QuantityType.Create(<ifc-measure-type-name>)` (or dimension-only `OfSi(dim, si)` when untyped).',
    'COMPILE-BREAK (Projection/semantic.md): `SemanticProjector.Project` mints rooted ids via `ctx.Rooted()`, but the seam `ProjectionContext` exposes only `For`/`Owns` and projection.md states the mint path is the kernel static `NodeId.Rooted()` (a ctx.Rooted() pass-through would be a doctrine-forbidden thin wrapper). Change both sites (`mints a NEUTRAL rooted NodeId per IfcRoot through ctx.Rooted()` prose + `map.AddOrUpdate(root.GlobalId, ctx.Rooted())`) to `NodeId.Rooted()`.',
    'BIM EGRESS GAP (the IfcRelKind reverse index): `ByNeutral` has Assign rows only for sub-kinds type-definition (DefinesByType) and group (AssignsToGroup), so `ForNeutral(Assign, "assessment")` returns None and a Compute-authored Assign/AssignKind.Assessment edge cannot be re-authored to IFC at Emit (silently dropped). Add an IfcRelKind row mapping the seam Assign/Assessment edge to an IFC objectified-assessment relation (e.g. IfcRelAssignsToControl over IfcPerformanceHistory), OR document assessment edges as non-IFC-native + intentionally dropped at egress.',
    'IfcComplexProperty LOWERING: the Bim properties projector must lower `IfcComplexProperty` onto the seam `PropertyValue.Complex` arm (UsageName + named sub-property map) at IFC ingest, recursing the seam `PropertyValue.Of` admission (IfcComplexProperty.HasProperties -> Complex.Properties keyed by each sub-property PropertyName; IfcComplexProperty.UsageName -> Complex.UsageName) — else nested/grouped IFC properties (layered glazing, multi-component ratings, bSDD complex templates) are dropped.',
    'GEOREFERENCE MapUnit: verify/ensure the Bim georeference projector reads `IfcProjectedCRS.MapUnit` and composes the model-unit<->CRS-unit factor when building the rigid map-conversion transform from the seam GeoReference. The seam tuple carries the resolved CRS native linear unit (Element reference.md) but NOT the MapUnit (M1), so a non-metre projected CRS (US-survey-foot State Plane) is mislocated unless the projector reads MapUnit at ingest.',
  ] },
  { name: 'Compute', root: 'libs/csharp/Rasm.Compute', claims: [
    'PHANTOM seam member (Analysis/aggregator.md ~179): `props.Fire.Map(static f => f.ResistanceMinutes)` reads a member that does NOT exist — the seam `MaterialPropertySet.Fire` is a typed `FireResistance R/E/I`, not a scalar ("a single resistance scalar is the deleted form"). The worst-ply fire envelope must read `f.Resistance.LoadBearingMinutes` (the R criterion).',
    'RENAMED-OWNER drift: the seam collapsed per-rating `Acoustic.StcContourFit` into the polymorphic `RatingContour.Fit` family (Stc/Rw rows by data). Compute consumers still cite the DELETED `Acoustic.StcContourFit` (aggregator ~9, physics ~6, Compute ARCHITECTURE ~2). Repoint every `Acoustic.StcContourFit(vector)` -> `RatingContour.Stc.Fit(vector)` (the public seam contour-fit kernel); fix the mass-law-band description "octave centres" -> "one-third-octave centres" (AcousticBand = 17 one-third-octave bands).',
    'ASSESSMENT node-id + private-ctor (Compute WriteBack, consumer-side): (a) the node id is minted `NodeId.OfContent(key)` where key=InputKey over (route,targets,policy); ContentAddress.Verify recomputes hash(ToCanonicalBytes(...)) != InputKey so every Compute-authored Assessment node fails Verify on rehydrate — mint via `NodeId.Content(node.ToCanonicalBytes(graph.Header.Tolerance))` (self-hash, computable pre-run) for BOTH the WriteBack mint AND the CacheFirst lookup in Assess. (b) WriteBack constructs `new AssessmentPayload(...)` against the seam PRIVATE ctor (inaccessible cross-assembly; internal Seed also inaccessible) — use the public `AssessmentPayload.Computed(discipline, route, inputKey, results, resultBlob, provenance, ctx.Key)` factory (returns Fin) and bind the Fin.',
    'ENVIRONMENTAL double-store consumer-side (Analysis/lifecycle.md:236/250): once the seam `OfEnvironmental` drops the `gwpKgCo2e` headline param + `GlobalWarmingPotential` field (Element batch), align the lifecycle call sites — `RunCarbon`/`EnrichCarbon` read the A1-A3 GWP from the StageGwp vector (`norm.PerUnit[LifecycleStage.A1A3.Index]`), not a separate stored scalar.',
    'ANALYSISROUTE cache-key contract: the AnalysisRoute token OR the InputKey MUST fold the solver tool+version, so a solver-version bump (EnergyPlus 25.2.0->26.1.0, a closed-form revision) re-keys to a fresh Assessment node rather than false-hitting a prior version cached Computed result. Encode solver tool+version in the route token or fold it into the XxHash128 InputKey derivation (Compute owns the roster + InputKey).',
  ] },
  { name: 'Materials', root: 'libs/csharp/Rasm.Materials', claims: [
    'SEAMSECTION positional COMPILE-BREAK (Projection/material.md): the seam `SectionProperties` is 17 fields with `Iw` 5th, but Materials `SeamSection` constructs only 16 args (no Iw) -> every field after position 4 lands in the wrong slot (Sx->Iw, etc.). Add an `IwMm6` column to `ComputedSection` and a `Warping(double mm6) => MeasureValue.OfSi(QuantityType.Create("WarpingConstant"), Dimension.Create(6,0,0,0,0,0,0), mm6*1e-18)` mapping at position 5, per the seam material.md Iw contract.',
    'RENAMED-OWNER drift: repoint every Materials reference to the deleted `Acoustic.StcContourFit` -> `RatingContour.Stc.Fit` (properties ~5, material/ARCHITECTURE/README ~1 each).',
    'ENVIRONMENTAL double-store consumer-side (properties/sustainability.md:52/83): once the seam `OfEnvironmental` drops the `GlobalWarmingPotential` field + `gwpKgCo2e` param (Element batch), align the Materials projector that authors the baked `Environmental` PerM3 declaration to the new signature (no headline GWP scalar; the A1-A3 value rides the StageGwp vector).',
    'FAULT-BAND COMPILE-BREAK: Materials `ConstructionFault`(2350)/`MaterialFault`(2450)/`ProfileFault`(2300) declare `: Expected, IValidationError<TError>` with per-case `override string Category` yet call `: base(detail, NNNN, None)` — that 3-arg ctor exists only on LanguageExt.Common.Expected (no Category to override), while the kernel `Rasm.Domain.Expected` (which HAS virtual Category) exposes only a parameterless ctor. Migrate all three to the kernel-Expected pattern ElementFault(2500)/BimFault(2600) use: `private Ctor(Op, string) : base()` + `override int Code => NNNN` + Switch-/per-case-projected Message/Category, so `FaultExtensions.Category(error)` reports the real category. (Also confirm the Materials band assignment vs the kernel re-band from the KERNEL batch.)',
  ] },
  { name: 'Persistence', root: 'libs/csharp/Rasm.Persistence', claims: [
    'graph.md `[02]-[STREAM_GRAIN]` Cases prose lists the seam `Header` as `(ReleaseVersion/ModelView/GeoReference/Tolerance/Instant/StepHeader/OwnerHistory)`, but the seam `Header` (Element Graph/element.md) is `(ReleaseVersion Schema, ModelView View, GeoReference Reference, double Tolerance, Instant At, StepHeader Step)` and carries NO OwnerHistory (it rides the Object node, H9). Drop `OwnerHistory` from the Header field-list in the prose.',
  ] },
  { name: 'Kernel', root: 'libs/csharp/Rasm', claims: [
    'FAULT-BAND COLLISION (federation-wide, kernel is the outlier): kernel `GeometryFault` sub-bands run 2401-2519 (DegenerateArrangement=2450, DecimationFault=2500, EncodingFault=2510) which OVERLAP the AEC bands MaterialFault=2450 and ElementFault=2500 -> `HasCode(2450)`/`HasCode(2500)` conflates a kernel geometry fault with a Material/Element fault. The clean federation is 2300/2350/2400/2450/2500/2600. FIX in the kernel: re-band the geometry sub-bands into 2400-2449 (compress to <=4 codes/cluster across its 12 sibling clusters), OR — if 50 codes cannot hold geometry — re-plan the federation to give geometry a full century (2400-2499) and shift the AEC bands up, updating every AEC fault page that asserts its band. Pick the option that keeps the federation collision-free and the AEC pages truthful (the Element/Materials/Bim fault pages assert the clean plan).',
  ] },
  { name: 'Python', root: 'libs/python', claims: [
    'CROSS-STACK golden-vector parity (runtime/evidence/identity.md `MATERIAL_LAYER_GOLDEN`, pinned on the C# Element address content-key): the fixture describes the per-layer ThicknessMm `Measure` canonical bytes as "the IEEE-754 LE Si magnitude + the 7 SI Dimension exponent ordinals" but OMITS the leading length-prefixed `QuantityType` discriminator token that C# `CanonicalWriter.Measure` writes FIRST (`String(q.Type.Value)` before Si+exponents). Align the golden-vector Measure byte description to include the leading length-prefixed QuantityType token so the seed-zero XxHash128 digest matches C# byte-for-byte (prose-shape pin; the fixture carries no bytes yet — decode-not-remint, never re-mint the C# contract).',
  ] },
  { name: 'SharedApi', root: 'libs/csharp/.api', claims: [
    'api-unitsnet.md UNDER-COVERAGE: the shared catalog tabulates only ~15 quantity/unit families but the Element corpus composes uncatalogued REAL members — `Density`/`DensityUnit.KilogramPerCubicMeter`, `ThermalConductivity`/`WattPerMeterKelvin`, `SpecificEntropy`/`JoulePerKilogramKelvin`, `HeatTransferCoefficient`/`WattPerSquareMeterKelvin`, `Level`/`Decibel` (SoundPressureLevel), `Temperature` — plus the open `QuantityType.Create(name)` derived-name pattern (WarpingConstant/SectionModulus/SecondMomentOfArea/TorsionConstant). All assay-verified real in UnitsNet 5.75.0. Deepen api-unitsnet.md to enumerate these families + document the derived-quantity-name composition pattern so future harden passes do not mis-flag them as phantoms.',
  ] },
]
const RECONCILE_OPEN = 'reconcileOpen SYSTEMIC: the inline Object.BoundaryPolygon/Axis analytical channel must be gone everywhere; analytical axis/footprint resolve by content key (Representations["Axis"]/["FootPrint"]) from the blob store, not an inline seam field.'
const ALL_CLAIMS = BATCHES.flatMap((b) => b.claims.map((c) => ({ folder: b.name, claim: c }))).concat([{ folder: 'Element', claim: RECONCILE_OPEN }])

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['fixed', 'partial', 'clean'] }, fixed: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SANITY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'items'], properties: { overall: { type: 'boolean' }, items: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'open'] }, evidence: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: the Rasm.Element rebuild. READ ' + PLAN + ' (repo root) in FULL FIRST — section 4-RT (RED-TEAM REVISIONS) is AUTHORITATIVE and OVERRIDES 4A-H on conflict. These are DESIGN-PAGE specs ' +
    '(.planning markdown). You are FIXING cross-file residuals the Element harden logged but did NOT fix (the old defer-flow). MANY are COMPILE-BREAK level. INVESTIGATE before you edit: read the cited ' +
    'files AND the seam contract pages they reference; a residual claim may be slightly imprecise — fix the REAL defect, never a literal token-patch of a misread claim.',
  'MANDATORY STANDARDS: docs/stacks/csharp/** is the FLOOR (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis + the relevant domain/ shard) — every fix ' +
    'lands at the strongest CLEAN/MODERN form the doctrine admits, never a patch that merely silences the defect. Stack BOTH the SHARED libs/csharp/.api/ tier AND the folder .api/ to full depth. Cite ONLY ' +
    'members verified via `uv run python -m tools.assay api` (a member you cannot verify is a phantom — do not introduce it).',
  'WRITE-FULLY MANDATE: every fix you identify you MAKE NOW via Edit/Write; the returned log REPORTS edits already made, never a to-do. A fix that REQUIRES editing a file OUTSIDE your folder scope goes to ' +
    'residual_high {files, claim} for the terminal reconcile (which has NO scope cap) — never leave it unfixed.',
].join('\n')
const SEAM = [
  'SECTION-4-RT INVARIANTS (preserve, never regress while fixing): Relationship is a NEUTRAL edge algebra (Compose|Assign|Associate|Connect|Void + typed payload + Generic passthrough), NOT 17 typed IfcRel ' +
    'cases — IFC relationship schema lives in Bim`s projector; PredefinedType is a typed Object field with a Bim egress gate; the Associate edge carries LayerSet/ProfileSet usage; MeasureValue is the 4-arg ' +
    '`(QuantityType, Dimension, double Si, string CanonicalUnit)` record over a Dimension value-object + UnitsNet; ElementGraph carries the incidence index + memoized Bake with a HAMT working graph vs Frozen ' +
    'read snapshot; rooted NodeId is a NEUTRAL kernel id (NodeId.Rooted()), IFC GlobalId a Bim attribute; ONE canonical codec (ToCanonicalBytes / CanonicalWriter) shared by NodeId-hash + diff; content-id is ' +
    'NodeId.Content(self-hash), OfContent only when ContentAddress == ContentAddress.Of(node bytes); TWO seam interfaces IElementProjection + IGraphConstraint; Marten is the append substrate BENEATH the ' +
    'preserved op-log/CRDT/time-travel engine; topology is synchronous, AGE/DuckDB async analytical lanes. A page contradicting a 4-RT invariant is a defect to fix TOWARD 4-RT.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW (clean/modern/advanced): model the domain precisely (no weak/erased types); NO exception control flow in domain logic (LanguageExt Fin/Validation/Option/Eff rails / ROP); NO imperative ' +
    'branching where a bounded vocabulary / frozen table / generated Switch / Fold owns the variation; NO mutable accumulation; total generated Switch with compile-time exhaustiveness (no silent _ arm); the ' +
    'fault type is a CLOSED [Union] deriving from the kernel Expected. Latest C# 14 on net10 to the metal. LanguageExt.Core 5.0.0-beta: Map/Choose/Filter/Fold/ToSeq exist ONLY on LanguageExt types (Seq/Iterable/' +
    'Arr/Lst/HashSet/Set); wrap a raw ImmutableArray/FrozenDictionary.Values in `toSeq(...)` (or expose a Seq read-accessor) before a combinator; `.HeadOrNone()` is removed (use the `Seq.Head` Option property).',
  'BOUNDARY/STRATA + SEMANTIC_NAMING: each owner stays on its stratum, depends strictly upward; one bounded-context term per concept; ONE_HOP_RESOLUTION (no alias chains / forwarding helpers / single-call ' +
    'helpers — a ctx pass-through to a kernel static is a forbidden thin wrapper). Apply the docs/stacks/csharp file-organization + section-order law. DERIVED_LOGIC: one canonical owner per fact (no double-store, ' +
    'no duplicated codec/comparer); a value stored twice with no enforced equality is a defect to collapse to a derivation.',
].join('\n')
const PROSE = 'PROSE + FENCES: design-SPEC prose only — lead with the controlling rule, one idea per paragraph, close on the consequence; cut hedges/provenance/process narration. REAL transcription-complete ' +
  'code fences, ZERO placeholder/stub/TODO. BACKTICK every symbol/type/member/path/package. COMMENT HYGIENE: keep the canonical section-divider headers; otherwise agent-facing comments only where intent is not ' +
  'obvious (default zero). ("Page craft" is a docs/stacks/csharp concept ONLY; it does not govern these design docs.)'
const DOCTRINE = [LAW, '', SEAM, '', PATLAW, '', PROSE].join('\n')

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
const norm = (x) => { const files = Array.isArray(x.files) ? x.files.filter(inLibs) : []; return { files: files.length ? files : ['libs/csharp'], claim: x.claim } }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const fixPrompt = (b) => [DOCTRINE, '', 'TASK: FIX the Element-harden residuals owned by `' + b.name + '` (folder `' + b.root + '/**`). For EACH residual: INVESTIGATE the cited files (read them + the seam ' +
  'contract pages they reference under libs/csharp/Rasm.Element/.planning), confirm the REAL defect, and FIX it in place to the strongest CLEAN/MODERN form docs/stacks/csharp + the 4-RT invariants admit — a ' +
  'token/literal patch that merely silences the symptom is NOT acceptable. Edit pages under `' + b.root + '/**` (+ its ARCHITECTURE/README); READ counterpart pages in other folders for the seam contract, but ' +
  'if a fix REQUIRES editing another folder, report it in residual_high {files, claim} for the terminal reconcile. Align every fix to the seam contract the 4-RT invariants + the Element seam pages define ' +
  '(SeamSection is 17 fields w/ Iw 5th; MeasureValue is the 4-arg QuantityType record; NodeId.Rooted()/NodeId.Content(self-hash); the neutral edge algebra; etc.). RESIDUALS (' + b.claims.length + '):\n' +
  b.claims.map((c, i) => (i + 1) + '. ' + c).join('\n') + '\nReturn folder + verdict + fixed (each residual you resolved) + residual_high (cross-folder items) + summary.'].join('\n')
const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals the folder batches deferred (cross-folder seam alignments). NO severity, NO leftovers, NO ' +
  'scope cap. Read EVERY listed file across libs/ (csharp + py) and FIX the real cross-file defect in place to the strongest clean/modern + 4-RT form (align the seam contract + every consumer in lockstep, ' +
  'repair strata/boundary), preserving all capability — a token patch that leaves the seam misaligned is NOT a fix; if a residual is FACTUALLY WRONG, leave it and say why (verify marks it invalid). If your fix ' +
  'surfaces a new cross-file need, report it in residual_high. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const verifyPrompt = (cl, fixFiles) => [LAW, '', SEAM, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and CONFIRM the fix was ACTUALLY made AND is COMPLETE + ' +
  'HIGH-QUALITY + clean/modern/4-RT-conformant, not a token or naive patch. ATTACK the fix: is it shallow, partial, a rename that left a sibling stale, or does it leave the cross-file seam misaligned? Classify ' +
  'each: "fixed" (confirmed real, complete, non-naive), "invalid" (claim factually wrong — cite why), or "open" (NOT fixed, OR fixed naively/incompletely — must be redone). Default "open" on ANY doubt. ' +
  'Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')
const sanityPrompt = (items) => [DOCTRINE, '', 'TASK: SINGLE SANITY PASS over ALL ' + items.length + ' original Element-harden residuals. For EACH item: re-read the cited file(s) from disk and CONFIRM the ' +
  'defect is GENUINELY + CLEANLY fixed — the compile-break is gone, the fix is clean/modern per docs/stacks/csharp, the 4-RT invariants hold, no token patch, no sibling left stale, no new drift introduced. Be ' +
  'adversarial: a confident-looking edit that does not truly resolve the defect is "open". Classify each item "fixed" or "open" with one-line evidence; default "open" on ANY doubt. The full corpus (every ' +
  'libs/csharp/Rasm.* + libs/python folder) is in scope to read. ITEMS:\n' + items.map((it, i) => (i + 1) + '. [' + it.folder + '] ' + it.claim).join('\n') +
  '\nReturn overall (true only if ALL fixed) + items (per-item status + evidence) + summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('element-residual-fix: ' + ALL_CLAIMS.length + ' residuals across ' + BATCHES.length + ' folder batches; fix -> reconcile(loop) -> sanity; CAP=' + CAP)

// --- [FIX]
phase('Fix')
const fixed = (await pool(BATCHES, CAP, (b) => agent(fixPrompt(b), { label: 'fix:' + b.name, phase: 'Fix', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
log('Fix: ' + fixed.filter((r) => r.verdict !== 'clean').length + '/' + BATCHES.length + ' batches reported edits')

// --- [RECONCILE]
let pending = dedup(fixed.flatMap((r) => (r.residual_high || []).map(norm)))
let invalid = []
let round = 0
if (pending.length) {
  phase('Reconcile')
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = cluster(pending)
    log('Reconcile round ' + round + ': ' + pending.length + ' cross-folder residual(s) -> ' + clusters.length + ' cluster(s) (lib-wide, no-defer)')
    const resolved = (await pool(clusters, CAP, async (cl) => {
      const fix = await agent(reconcileFix(cl), { label: 'reconcile-fix:r' + round, phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: STALL })
      if (!fix) return { open: cl, invalid: [], surfaced: [] }
      const verify = await agent(verifyPrompt(cl, fix.files), { label: 'reconcile-verify:r' + round, phase: 'Reconcile', schema: VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
      const claims = (verify && verify.claims) || []
      const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
      const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
      return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map(norm) }
    })).filter(Boolean)
    invalid = dedup([...invalid, ...resolved.flatMap((r) => r.invalid)])
    const invalidKeys = new Set(invalid.map((r) => r.claim))
    pending = dedup([...resolved.flatMap((r) => r.open), ...resolved.flatMap((r) => r.surfaced)]).filter((r) => !invalidKeys.has(r.claim))
  }
}

// --- [SANITY]
// DRIVE TO ZERO: re-sanity after every force-close until NOTHING is open. The cap is a runaway backstop, not a give-up.
phase('Sanity')
let sanity = await agent(sanityPrompt(ALL_CLAIMS), { label: 'sanity', phase: 'Sanity', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
let sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
let saneRound = 0
while (sanityOpen.length && saneRound < SANITY_CAP) {
  saneRound++
  log('Sanity round ' + saneRound + ': ' + sanityOpen.length + ' OPEN -> FORCE-CLOSE (lib-wide max pass over every open item) + re-sanity; nothing leaves open')
  phase('Reconcile')
  await agent(reconcileFix(sanityOpen.map((i) => ({ files: ['libs/csharp'], claim: i.claim }))), { label: 'sanity-force-close:r' + saneRound, phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: STALL })
  phase('Sanity')
  sanity = await agent(sanityPrompt(ALL_CLAIMS), { label: 'sanity:r' + saneRound, phase: 'Sanity', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
  sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
}
if (sanityOpen.length) log('Sanity: ' + sanityOpen.length + ' STILL OPEN after ' + SANITY_CAP + ' force-close rounds — HARD BLOCKER (likely an architectural decision), reported LOUDLY, never silently dropped')
else log('Sanity: ALL ' + ALL_CLAIMS.length + ' residuals CLOSED + verified across ' + saneRound + ' force-close round(s)')

return {
  workflow: 'element-residual-fix', totalResiduals: ALL_CLAIMS.length, batches: BATCHES.map((b) => b.name),
  fixVerdicts: fixed.map((r) => ({ folder: r.folder, verdict: r.verdict })),
  reconcileRounds: round, invalidClaims: invalid.map((x) => x.claim),
  sanityOverall: sanity && sanity.overall, sanityOpen: sanityOpen.map((i) => ({ claim: i.claim, evidence: i.evidence })),
}
