export const meta = {
  name: 'seam-reconcile',
  whenToUse: 'The campaign TERMINAL cross-libs master pass + .api substrate harden for the Rasm Material/Component/Element rebuild — run LAST.',
  description: 'TERMINAL cross-libs master pass + .api substrate harden for the Rasm Material/Component/Element rebuild, run LAST after element-component, bim-rebuild, and sibling-ripple. Four phases. SUBSTRATE: a single agent hardens the shared libs/csharp/.api substrate tier (rebuild-api 3-lens extract/refine/harden) and performs the three named fills of scope [4]/[7] — fill the libs/csharp/.api README roster, author/deepen the LanguageExt.Core catalog gap, and resolve the protobuf-net vs MessagePack vs Nerdbank.MessagePack redundancy to one canonical wire-serialization owner; members verified via assay api, nuget + Context7 ToolSearch-loaded. PINS: a single agent confirms the central Directory.Packages.props graph, hand-edits only if a pin must change or the serialization decision prunes one (never dotnet add), then dotnet restore + dotnet nuget why. SWEEP: a single full-libs agent verifies every cross-folder/cross-stack seam mirrors on BOTH endpoints across Element, Materials, Bim, Compute, Persistence, Fabrication, the Rasm kernel, AppHost, AppUi, Rhino, Grasshopper, and the python + typescript wire (decode-not-remint), each recorded in both folders ARCHITECTURE.md [2]-[SEAMS] with mirrored rows, confirms every index doc is truthful, and fixes in place across folders. RESOLVE: the no-defer terminal drive-to-zero — a fix then adversarial-verify reconcile loop over every surfaced residual (open re-enters, invalid surfaced never filtered), then a sanity re-audit over the union of all surfaced residuals, force-closed per union-find cluster and re-audited after every force-close until zero open; the round caps are runaway backstops and any terminal open is logged loudly and flagged for a downstream residual-fix run. CAP 10. Takes no args.',
  phases: [
    { title: 'Substrate' },
    { title: 'Pins' },
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
const SCOPE = 'RASM-REBUILD-SCOPE.md'
const PROPS = 'Directory.Packages.props'
const SUBSTRATE = 'libs/csharp/.api'
const LIBS_SWEEP = 'EVERY libs/ folder: libs/csharp/Rasm.Element, Rasm.Materials, Rasm.Bim, Rasm.Compute, Rasm.Persistence, Rasm.Fabrication, libs/csharp/Rasm (the geometry/analysis kernel), Rasm.AppHost, ' +
  'Rasm.AppUi, Rasm.Rhino, Rasm.Grasshopper, AND the libs/python + libs/typescript wire (decode-not-remint, never re-mint the C# contract)'

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const PIN_NEED = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package'], properties: { package: { type: 'string' }, current: { type: 'string' }, note: { type: 'string' } } } }
const SUBSTRATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, decision: { type: 'string' }, integrated: { type: 'array', items: { type: 'string' } }, pinsNeeded: PIN_NEED, residual_high: RESIDUAL, summary: { type: 'string' } } }
const PINS_SCHEMA = { type: 'object', additionalProperties: false, required: ['pinned', 'verdict', 'summary'], properties: { pinned: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'version'], properties: { package: { type: 'string' }, version: { type: 'string' } } } }, verdict: { type: 'string', enum: ['pinned', 'none'] }, restore: { type: 'string' }, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['aligned', 'clean'] }, aligned: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
const SANITY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'items'], properties: { overall: { type: 'boolean' }, items: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'open'] }, evidence: { type: 'string' } } } }, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: the Rasm Material/Component/Element rebuild — seam-reconcile is the TERMINAL cross-libs master pass + .api substrate harden, run LAST (AFTER element-component + bim-rebuild + sibling-ripple). READ `' + SCOPE +
    '` (repo root) in FULL FIRST; the prompts POINT to its numbered sections, they do NOT restate it. [1] is the unified Material/Component/Element paradigm + the canonical Element seam; [2] the per-Component generative data; ' +
    '[3] the captured WATCH residuals (resolve only those the rebuild triggers); [4] CATALOG_LAW (schema + polymorphic loader + dense in-fence seed tables + maximal external leverage; NO new NuGet pin is required — package work ' +
    'is the LanguageExt.Core .api gap + the `' + SUBSTRATE + '/` README roster + the protobuf-net/MessagePack/Nerdbank.MessagePack redundancy); [5] the semantic-rename + boundary law (ProfileRef/ProfileSet/ComputedSection STAY ' +
    'seam-canonical); [7] the two-tier .api mandate; [8] the manifest rule. The settled architecture is CANONICAL; CLAUDE.md WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend ' +
    'strictly upward). This WF does NOT re-author design pages from scratch — it hardens/extracts/refines the .api substrate, confirms the manifest graph, and verifies + repairs cross-libs seam mirroring, then drives all ' +
    'residuals to zero.',
  'WRITE-FULLY MANDATE: every gap/defect you identify you CLOSE NOW via Edit/Write; the returned log REPORTS edits already made, never a to-do. Leave nothing behind except genuine cross-FILE items — report those in residual_high ' +
    'as {files:[...], claim} (a cross-file item names EVERY spanned file) for the terminal Resolve, which has NO scope cap.',
].join('\n')
const DUALAXIS = [
  'DUAL-AXIS READ (scope [6]) — every implementing stage reads BOTH axes and finalizes a page only when a cold read against both surfaces nothing. CODE DOCTRINE: `docs/stacks/csharp/**` every core page by name (README, ' +
    'language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) + the relevant `domain/` shard(s) — strategic emphasis data-interchange, diagnostics, interaction, transport, validation; ' +
    '`docs/stacks/python/**` for the python wire + the ts route for the typescript wire. DOC-CRAFT: `libs/.planning/README.md` [PLANNING_STANDARD] (the index-doc templates, the design-page grammar, the `page#CLUSTER` ' +
    'integration-point notation, the [06] cold-grade REVIEW gate that is the doc-finalization gate, the [07] .api tiering) + `libs/.planning/campaign-method.md`; the THREE `docs/standards/` form standards ' +
    'information-structure.md / formatting.md / style-guide.md; `docs/standards/proof.md` claim discipline.',
  'BANNED HEDGES (word-boundary, page-wide, scope [6]): should, could, would, might, maybe, perhaps, likely, probably, propose, consider, recommended, ideally, TBD, TODO, FIXME, we, our, you, and the synonym forms (is expected ' +
    'to, can be, aims to, is designed to, in the future, eventually, as needed, if necessary) — future tense legal ONLY on a card growth line or a RESEARCH item. ZERO-PROVENANCE (style-guide.md): no reader address, narration, ' +
    'process, source provenance, source-mining history, freshness disclaimers, checklist tails; on a design page no links, URLs, versions, dates, or session context.',
].join('\n')
const APITIER = [
  'TWO-TIER .api MANDATE (scope [7]): every page cites the .api catalogs its concept composes — and ONLY those, never noise. SUBSTRATE tier `' + SUBSTRATE + '/` is the universally-used cross-cutting catalogue, treated EQUALLY ' +
    'with the folder tier and used whenever any folder composes a shared dependency; FOLDER tier `libs/csharp/<package>/.api/` carries domain catalogues + folder-specific overlays. Stack BOTH tiers to exhaustion: compose every ' +
    'relevant admitted member into single dense owners at the DEEPEST surface each package reaches; reject thin wrappers, BCL-first reflexes, and surface-level subsets.',
  'MEMBER TRUTH: cite ONLY host/NuGet members verified via `uv run python -m tools.assay api` (decompile/reflection over the restored assembly) — a member you cannot verify is a PHANTOM. assay api OWNS member-existence truth and ' +
    'WINS on conflict with live nuget feed intelligence; the nuget MCP answers newest-stable/license/supply-chain only. READ tools/assay/README.md FIRST for its api-arm invocation + JSON output shape.',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY implementing stage (substrate, sweep, resolve) is HOSTILE: assume the corpus is INCOMPLETE / NAIVE / ILLUSORY until it survives an aggressive attack; the burden of proof is ON THE PAGE. "Mature"/' +
    '"good enough"/a prior clean verdict are REJECTED. A no-edit verdict is earned ONLY after a genuine attack + a real domain/package/consumer sweep finds nothing.',
  'ILLUSORY/FAKE is the PRIMARY target — a page that USES the doctrine vocabulary and reads dense yet is HOLLOW (a name/prose promising capability the body lacks; a thin slice of a rich concept; a cited but unverifiable .api/host ' +
    'member — a phantom; a seam mirrored on ONE endpoint but not the other). Treat dense confident pages with MORE suspicion; disbelieve every self-claim until verified.',
].join('\n')
const SEAM = [
  'SEAM CONTRACT (scope [1] + [5], preserve + verify mirrored on BOTH endpoints): the unified Material(substance) / Component(placement-free TYPE owning the cross-section as a FIELD) / Element(sited Occurrence) paradigm over ONE ' +
    '`Object` node with `ObjectKind in {Type, Occurrence}` and one rooted identity regime (a Type Object`s NodeId deterministically derived from the Component`s canonical content, `Object.ToCanonicalBytes` excluding the volatile ' +
    'Representations for the Type seed); owner-mints-its-identity projection (Materials owns Types -> the ONE Component projection mints the rooted Type Object + stamps neutral Classification/PredefinedType + lowers parametric ' +
    'data + binds occurrences); the NEUTRAL edge algebra (Compose|Assign|Associate|Connect|Void + typed payload + Generic passthrough — IFC schema lives in Bim`s projector, never the seam); MeasureValue the 4-arg (QuantityType, ' +
    'Dimension, double Si, string CanonicalUnit); ONE canonical codec (ToCanonicalBytes/CanonicalWriter) shared by NodeId-hash + diff; the `MaterialBinding` trio disambiguated BakedMaterial / MaterialBinding / TypeBinding; ' +
    'ProfileRef/ProfileSet/ComputedSection STAY seam-canonical (the M7 one-hop resolution spanning Bim/Compute/python/ts).',
  'WIRE + STRATA LAW: the libs/python + libs/typescript wire DECODES the canonical C# contract, never RE-MINTS it (decode-not-remint); a golden-vector/byte-shape pin mirrors the C# CanonicalWriter byte order exactly. Each owner ' +
    'stays on its stratum and depends strictly upward; geometry/mesh/IFC meet at ONE wire owner per runtime; no host-type leak into a host-neutral owner; Rasm.AppHost stays reference-light (no AEC-domain ref), Rasm.AppUi pure-UI; ' +
    'Rasm.Rhino/Rasm.Grasshopper consume the seam Element/ElementGraph at the wire; Rasm.Fabrication references Rasm.Element. A page contradicting this contract is a defect to fix TOWARD it.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely (no weak/erased types); NO exception control flow in domain logic (LanguageExt Fin/Validation/Option/Eff rails / ROP); NO imperative branching where a bounded vocabulary / frozen ' +
    'table / generated Switch / Fold owns the variation; NO mutable accumulation; total generated Switch with compile-time exhaustiveness (no silent _); fault = a CLOSED [Union] deriving from the kernel Expected. Latest C# 14 on ' +
    'net10 to the metal. LanguageExt.Core 5.0.0-beta: Map/Choose/Filter/Fold/ToSeq exist ONLY on LanguageExt types — wrap a raw ImmutableArray/FrozenDictionary.Values in `toSeq(...)` before a combinator; `.HeadOrNone()` is ' +
    'removed (use the `Seq.Head` Option property).',
  'COLLAPSE MANDATE: collapse >=3 parallel types / sibling factories / repeated switch arms / single-call helpers into ONE polymorphic owner IN THE SAME FILE via [Union]/[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/' +
    'source-generated case families/Fold algebra/frozen tables — never extract a new file to reduce LOC, never delete capability. ONE_HOP_RESOLUTION: no alias chains, forwarding helpers, or single-call helpers; one bounded-context ' +
    'term per concept.',
].join('\n')
const PROSE = 'PROSE + FENCES: design-SPEC prose only — lead with the controlling rule, one idea per paragraph, close on the consequence; cut hedges/provenance/process narration. REAL transcription-complete code fences, ZERO ' +
  'placeholder/stub/TODO. BACKTICK every symbol/type/member/path/package. Keep the canonical section-divider headers; otherwise agent-facing comments only where intent is not obvious (default zero). ("Page craft" is a ' +
  'docs/stacks/csharp concept governing the doctrine pages, NOT these .planning design docs.)'
const DOCTRINE = [LAW, '', DUALAXIS, '', APITIER, '', ADVERSARIAL, '', SEAM, '', PATLAW, '', PROSE].join('\n')

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
const substratePrompt = () => [DOCTRINE, '', 'TASK: SUBSTRATE — harden the SHARED `' + SUBSTRATE + '/**` .api tier (rebuild-api 3-lens per catalog: EXTRACT-FULL the advanced surface -> REFINE to integration-shaped, STACKED ' +
  'rails -> HARDEN adversarially, removing every phantom/naive/un-stacked framing; members verified via `uv run python -m tools.assay api`). Then the THREE named substrate fills of scope [4]/[7]: (1) FILL the `' + SUBSTRATE +
  '/README.md` ROSTER so every substrate catalog carries a truthful roster row reflecting the FINAL state; (2) AUTHOR/DEEPEN the LanguageExt.Core `.api` catalog gap to full first-class capability (the substrate-tier rail every ' +
  'folder composes — Fin/Validation/Option/Eff + Seq/Iterable/Arr/Lst/HashSet/Set combinators, traversal, the v5-beta surface, every member assay-verified); (3) RESOLVE the protobuf-net vs MessagePack vs Nerdbank.MessagePack ' +
  'REDUNDANCY — DECIDE the ONE canonical wire-serialization owner on real evidence (member surface via `assay api` + newest-stable/license/supply-chain via the nuget MCP), RECORD the decision in the owning catalog, and ' +
  'PRUNE/REALIGN the others (drop each loser`s catalog to a one-line superseded note pointing at the owner, and flag the manifest implication). DEFERRED TOOLS — load the EXACT names via ToolSearch FIRST before any call: ' +
  '`mcp__nuget__get_latest_package_version`, `mcp__nuget__get_package_context`; `mcp__plugin_context7_context7__resolve-library-id`, `mcp__plugin_context7_context7__query-docs`. Edit ONLY under `' + SUBSTRATE + '/**`; a ' +
  'realignment that REQUIRES a folder design page or the manifest goes to residual_high {files, claim} (the serialization decision`s consumer realignments + the manifest pin/prune belong there). Return files + verdict + ' +
  'decision (the chosen serialization owner + why) + integrated + pinsNeeded (every package the substrate work touched or the decision changes, for the Pins phase to confirm or prune) + residual_high + summary.'].join('\n')
const pinsPrompt = (pins) => [LAW, '', APITIER, '', 'TASK: CONFIRM THE CENTRAL PIN GRAPH. The substrate phase touched/decided these packages (an EMPTY list means a pure graph confirmation):\n' + JSON.stringify(pins, null, 1) +
  '\nFor EACH: confirm the NEWEST stable version via the nuget MCP — ToolSearch-load `mcp__nuget__get_latest_package_version` + `mcp__nuget__get_package_context` FIRST (net10.0 TFM + commercial-safe license). HAND-EDIT `' +
  PROPS + '` ONLY if a pin must change — add/update the `<PackageVersion>` in the correct label-grouped cluster (sorted, one-line maintenance comment only), and REMOVE a pin the substrate serialization decision pruned — NEVER ' +
  '`dotnet add`. Scope [4] expects NO NEW package; the likely edits are confirm-only + the serialization prune. Then run `dotnet restore` AND `dotnet nuget why` to confirm the graph resolves (a restored assembly is required so ' +
  'any downstream `assay api` can decompile it). If nothing must change, confirm-and-log (verdict=none). Return pinned (each {package, version} you actually wrote) + verdict + the restore/why result + summary. Edit ONLY `' +
  PROPS + '`.'].join('\n')
const sweepPrompt = () => [DOCTRINE, '', 'TASK: FULL-LIBS SWEEP — the whole-stack one-shape-BOTH-ENDPOINTS seam/boundary verifier + truthfulness pass across ' + LIBS_SWEEP + '. (1) SEAMS/WIRES — every cross-folder/cross-stack ' +
  'producer/consumer relationship the campaign created mirrors on BOTH endpoints to ONE shared shape and is recorded in BOTH folders` `ARCHITECTURE.md` [2]-[SEAMS] with MIRRORED rows (a seam present on one side but ' +
  'missing/divergent on the other is the PRIMARY defect): the Element seam (IElementProjection/IGraphConstraint, the neutral edge algebra, MeasureValue, the unified Object/ObjectKind regime) <-> the Materials Component ' +
  'projection <-> the Bim ingest/egress projectors; the GeometryRef/RepresentationContentHash/ContentAddress content-key <-> Persistence <-> the Rasm kernel; the typed Material/Property/Assessment/Classification/section wire ' +
  'vocabulary <-> the libs/python AND libs/typescript decoders (decode-not-remint, never re-mint the C# contract). (2) BOUNDARY FOLDERS — Rasm.AppHost reference-light (no AEC-domain ref), Rasm.AppUi pure-UI, ' +
  'Rasm.Rhino/Rasm.Grasshopper consume the seam Element/ElementGraph at the wire, Rasm.Fabrication references Rasm.Element; FIX any that consume a retired shape. (3) NO DUPLICATION / NO STRATA VIOLATION — a concept owned twice ' +
  'collapses to its rightful stratum owner; depend strictly upward; geometry/mesh/IFC at ONE wire owner per runtime. (4) DOC TRUTH — every index doc (each folder `ARCHITECTURE.md` codemap + [02]-[SEAMS] + `README.md` roster; ' +
  'each branch index; the cross-libs `libs/.planning/architecture.md` + `libs/csharp/.planning/ARCHITECTURE.md`) is TRUTHFUL to its pages. FIX in place across folders (design doc AND code fence, in any language — C#, Python, ' +
  'TypeScript); a counterpart edit you cannot complete goes to residual_high {files, claim}. Return verdict + aligned (each alignment, naming the folders + seam) + residual_high + summary.'].join('\n')
const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals the substrate/sweep phases (and prior reconcile rounds) surfaced; NO severity, NO leftovers, NO deferral, NO ' +
  'scope cap. Read EVERY listed file across libs/ (csharp + python + typescript) + `' + PROPS + '` and FIX the real cross-file defect in place to the strongest clean/modern + seam-contract form (align the seam + every consumer ' +
  'in lockstep on BOTH endpoints, repair strata/boundary, finish a substrate decision spanning files, make an index doc truthful), preserving all capability — a token patch that leaves the seam misaligned is NOT a fix; if a ' +
  'residual is FACTUALLY WRONG, leave it and say why. If your fix surfaces a new cross-file need, report it in residual_high. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [LAW, '', SEAM, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and CONFIRM the fix is ACTUALLY made AND complete + ' +
  'high-quality + clean/modern/seam-conformant, not a token/naive patch. ATTACK it: shallow, partial, a rename that left a sibling stale, a seam mirrored on one endpoint but not the other? Classify each: "fixed" (real, ' +
  'complete, non-naive), "invalid" (claim PROVABLY wrong — cite why), or "open" (NOT fixed or fixed naively — redo). Default "open" on ANY doubt; "invalid" ONLY when provably wrong. Claims:\n' + JSON.stringify(cl, null, 1) +
  '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')
const sanityPrompt = (items) => [DOCTRINE, '', 'TASK: TERMINAL SANITY RE-AUDIT over the union of ALL ' + items.length + ' cross-libs residuals any phase surfaced. For EACH item: re-read the cited file(s) from disk and CONFIRM ' +
  'the defect is GENUINELY + CLEANLY resolved — the seam mirrors on BOTH endpoints, the fix is clean/modern per docs/stacks/csharp + the seam contract, every index doc is truthful, no token patch, no sibling left stale, no new ' +
  'drift introduced. Be adversarial: a confident-looking edit that does not truly resolve the cross-file defect is "open", never "fixed". Classify each item "fixed" or "open" with one-line evidence; default "open" on ANY doubt. ' +
  'The full corpus (every libs/csharp/Rasm.* + libs/python + libs/typescript folder) is in scope to read. ITEMS:\n' + items.map((it, i) => (i + 1) + '. [' + (it.files || []).join(', ') + '] ' + it.claim).join('\n') +
  '\nReturn overall (true only if ALL fixed) + items (per-item status + evidence) + summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('seam-reconcile: substrate (.api harden + roster/LanguageExt/serializer fills) -> pins (graph confirm) -> full-libs seam sweep -> terminal resolve (fix->verify loop + sanity drive-to-zero); CAP=' + CAP)

// --- [SUBSTRATE]
phase('Substrate')
const substrate = (await pool([0], 1, () => agent(substratePrompt(), { label: 'substrate:' + SUBSTRATE, phase: 'Substrate', schema: SUBSTRATE_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
const subPins = dedup(((substrate[0] && substrate[0].pinsNeeded) || []).map((p) => ({ files: [PROPS], claim: p.package + (p.current ? ' (current ' + p.current + ')' : '') + (p.note ? ' — ' + p.note : '') })))
log('Substrate: verdict ' + ((substrate[0] && substrate[0].verdict) || 'n/a') + '; ' + subPins.length + ' package(s) flagged for pin confirmation')

// --- [PINS]
phase('Pins')
const pins = await agent(pinsPrompt(subPins.map((p) => p.claim)), { label: 'pins', phase: 'Pins', schema: PINS_SCHEMA, effort: 'max', stallMs: STALL })
log('Pins: graph verdict ' + ((pins && pins.verdict) || 'n/a'))

// --- [SWEEP]
phase('Sweep')
const swept = (await pool([0], 1, () => agent(sweepPrompt(), { label: 'sweep:full-libs', phase: 'Sweep', schema: SWEEP_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)

// --- [RESOLVE]
const residualsOf = (rows, key) => rows.flatMap((r) => (r.residual_high || []).map((x) => norm(x, key(r))))
const surfaced = dedup([
  ...residualsOf(substrate, () => SUBSTRATE),
  ...residualsOf(swept, () => 'libs/csharp'),
])
const filesOf = new Map(surfaced.map((r) => [r.claim, r.files]))
const keyOf = (r) => r.files.slice().sort().join(',') + '|' + r.claim
const seen = new Set(surfaced.map(keyOf))
let invalid = []
let noFix = []
let pending = surfaced.slice()
let round = 0
phase('Resolve')
if (pending.length) {
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = cluster(pending)
    log('Resolve round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (lib-wide fix->verify, no-defer)')
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
    for (const r of resolved.flatMap((r) => r.surfaced)) if (!filesOf.has(r.claim)) filesOf.set(r.claim, r.files)
    // Re-enter ONLY genuinely-new residuals: a key already queued this run cannot re-enter (stops a phantom re-feeding every round).
    const fresh = dedup([...resolved.flatMap((r) => r.open), ...resolved.flatMap((r) => r.surfaced)]).filter((r) => !invalidKeys.has(r.claim) && !seen.has(keyOf(r)))
    fresh.forEach((r) => seen.add(keyOf(r)))
    pending = fresh
    // NO-PROGRESS BREAK: no cluster changed a file this round -> the remaining residuals are phantom/unfixable; stop instead of grinding to MAX_ROUNDS.
    if (!resolved.some((r) => r.changed)) { log('Resolve round ' + round + ': no file-changing progress — ' + noFix.length + ' residual(s) had nothing to fix (phantom/resolved); breaking'); pending = []; break }
  }
  if (pending.length) log('Resolve fix->verify: ' + pending.length + ' still open after ' + MAX_ROUNDS + ' rounds -> handed to the sanity drive-to-zero')
  else log('Resolve fix->verify: all surfaced residuals fixed + adversarially verified across ' + round + ' round(s)')
} else { log('Resolve: no residuals surfaced by substrate/sweep — fix->verify loop skipped; terminal sanity confirms') }

// --- [SANITY]
// DRIVE TO ZERO: re-audit the union of all surfaced residuals; force-close PER union-find cluster (files preserved) + re-audit after every force-close until NOTHING is open. The cap is a runaway backstop, not a give-up.
const auditInvalid = new Set(invalid.map((r) => r.claim))
const auditClaims = [...filesOf.entries()].filter(([c]) => !auditInvalid.has(c)).map(([claim, files]) => ({ files, claim }))
let sanity = null
let sanityOpen = []
let saneRound = 0
if (auditClaims.length) {
  sanity = await agent(sanityPrompt(auditClaims), { label: 'sanity', phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
  sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
  while (sanityOpen.length && saneRound < SANITY_CAP) {
    saneRound++
    const prevOpen = sanityOpen.length
    const openWithFiles = sanityOpen.map((i) => ({ files: filesOf.get(i.claim) || ['libs/csharp'], claim: i.claim }))
    const sclusters = cluster(openWithFiles)
    log('Sanity round ' + saneRound + ': ' + sanityOpen.length + ' OPEN -> ' + sclusters.length + ' cluster(s) FORCE-CLOSE + re-audit; nothing leaves open')
    const forced = (await pool(sclusters, CAP, (cl) => agent(reconcileFix(cl), { label: 'sanity-force-close:r' + saneRound, phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
    const fcTouched = forced.flatMap((fc) => (fc && Array.isArray(fc.files) ? fc.files.filter(inLibs) : []))
    // The force-close changed nothing: the remaining items are phantom/unfixable; skip the re-audit and stop.
    if (fcTouched.length === 0) { log('Sanity round ' + saneRound + ': force-close changed no files — ' + sanityOpen.length + ' remaining item(s) phantom/unfixable; breaking'); break }
    sanity = await agent(sanityPrompt(auditClaims), { label: 'sanity:r' + saneRound, phase: 'Resolve', schema: SANITY_SCHEMA, effort: 'max', stallMs: STALL })
    sanityOpen = ((sanity && sanity.items) || []).filter((i) => i.status === 'open')
    // No net decrease across the force-close -> drive-to-zero has stalled; stop instead of grinding to SANITY_CAP.
    if (sanityOpen.length >= prevOpen) { log('Sanity round ' + saneRound + ': no net progress (' + sanityOpen.length + ' open, was ' + prevOpen + ') — remaining item(s) phantom/unfixable; breaking'); break }
  }
  if (sanityOpen.length) log('Sanity: ' + sanityOpen.length + ' STILL OPEN after the force-close drive — HARD BLOCKER, reported LOUDLY + flagged for a downstream residual-fix run, never silently dropped')
  else log('Sanity: ALL ' + auditClaims.length + ' surfaced residuals CLOSED + verified across ' + saneRound + ' force-close round(s)')
} else { log('Resolve: clean — no cross-libs residual surfaced across the campaign') }

return {
  workflow: 'seam-reconcile',
  substrate: { verdict: substrate[0] && substrate[0].verdict, decision: substrate[0] && substrate[0].decision },
  pinsVerdict: pins && pins.verdict, pinned: ((pins && pins.pinned) || []).map((p) => p.package + '@' + p.version),
  sweepVerdict: swept[0] && swept[0].verdict,
  surfacedResiduals: surfaced.length, resolveRounds: round, sanityRounds: saneRound,
  invalidClaims: invalid.map((x) => x.claim),
  noFix: noFix.map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: pending.map((x) => ({ files: x.files, claim: x.claim })),
  sanityOpen: sanityOpen.map((i) => ({ claim: i.claim, evidence: i.evidence })),
  downstreamResidualFix: (pending.length + sanityOpen.length) > 0,
}
