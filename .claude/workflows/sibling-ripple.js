export const meta = {
  name: 'sibling-ripple',
  whenToUse: 'After WF-1 (element-component) + WF-2 (bim-rebuild) land the Element/Materials/Bim changes — the TERMINAL cross-libs pass: re-bind the sibling consumer sites, harden the .api substrate, confirm the manifest graph, verify every seam mirrors on both endpoints, and drive every residual (incl the carried Properties residuals) to zero within a hard reconcile budget. Replaces the former separate seam-reconcile.',
  description: 'WF-3 sibling-ripple, now the TERMINAL cross-libs master pass (the former seam-reconcile is FOLDED IN). Seven sequential phases. DISCOVER + VALIDATE + REBIND re-bind the named sibling consumer sites (Compute Analysis, Persistence Element, the Fabrication seam rows INCLUDING the Construction/nesting to Fabrication/Nesting seam and the Properties to Fabrication/Process seam, and the python + typescript wire decoders) to the Component/detail-schema wire WF-1 + WF-2 landed, decode-not-remint, batched two ripples per agent. SUBSTRATE hardens the shared libs/csharp/.api substrate tier and performs the three named fills (the .api README roster, the LanguageExt.Core catalog gap, the protobuf-net vs MessagePack vs Nerdbank.MessagePack serializer redundancy). PINS confirms the central Directory.Packages.props graph (hand-edit only if a pin must change, never dotnet add, then dotnet restore + dotnet nuget why). SWEEP verifies every cross-folder and cross-stack seam mirrors on BOTH endpoints across all libs folders (csharp + python + typescript), including the just-rebound sibling seams and the nesting seam, and confirms every index doc is truthful. RESOLVE is a BUDGET-CAPPED reconcile over the union of every surfaced residual plus the two carried Properties residuals (the aluminium 6082-T6 roster gap and the properties.md prose-vs-data parity drift): the union is clustered by shared files and AT MOST 6 fix agents plus 6 verify agents run in ONE pass — never more, no round-chasing and no sanity re-audit loop — and every residual not closed within that budget is logged LOUDLY and handed to a downstream residual-fix run, never chased with more agents. Re-opens no Element/Materials/Bim design. Takes no args.',
  phases: [
    { title: 'Discover' },
    { title: 'Validate' },
    { title: 'Rebind' },
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
const FIX_CAP = 6
const VERIFY_CAP = 6
const IMPLEMENT_BATCH = 2
const SCOPE = 'RASM-REBUILD-SCOPE.md'
const PROPS = 'Directory.Packages.props'
const SUBSTRATE = 'libs/csharp/.api'
const CORE_CS = 'docs/stacks/csharp/{README,language,shapes,surfaces-and-dispatch,rails-and-effects,boundaries,algorithms,system-apis}.md'
const DOMAIN_ROSTER = 'docs/stacks/csharp/domain/<shard>.md ENUMERATED ROSTER (13): runtime, concurrency, diagnostics, ' +
  'validation, resilience, transport, persistence, durability, postgres, data-interchange, compute, visuals, ' +
  'interaction. MAP each page concern -> its required shard(s): IFC/glTF/STEP/wire codec -> transport + ' +
  'data-interchange; Pset/Qto + property/admission validation -> validation; spatial/graph/catalog persistence -> ' +
  'persistence + durability; clash/solve/numeric -> compute; telemetry/receipts -> diagnostics; retry/backoff -> ' +
  'resilience; free-threading/subinterpreter -> runtime + concurrency; SQL/RLS -> postgres; UI/interaction -> ' +
  'interaction; appearance/render -> visuals.'
const API_TIERS = 'BOTH `.api` TIERS: the SHARED substrate tier `' + SUBSTRATE + '/**` (universal cross-cutting catalogs + ' +
  'the Thinktecture / LanguageExt rails) AND the package FOLDER tier `<package>/.api/**` (domain catalogs). For the ' +
  'python/ts sites the substrate is the language-stack `.api` substrate where present plus the package folder catalogs.'
const LIBS_SWEEP = 'EVERY libs/ folder: libs/csharp/Rasm.Element, Rasm.Materials, Rasm.Bim, Rasm.Compute, Rasm.Persistence, Rasm.Fabrication, libs/csharp/Rasm (the geometry/analysis kernel), Rasm.AppHost, ' +
  'Rasm.AppUi, Rasm.Rhino, Rasm.Grasshopper, AND the libs/python + libs/typescript wire (decode-not-remint, never re-mint the C# contract)'
const STACK = {
  cs: 'docs/stacks/csharp/** (README + language + shapes + surfaces-and-dispatch + ' +
    'rails-and-effects + boundaries + algorithms + system-apis + the relevant domain/ shard)',
  py: 'docs/stacks/python/** (the core pages + the relevant domain/ shard)',
  ts: 'docs/stacks/typescript/** (the coding-ts route: core pages + the relevant domain shard)',
}
const MAT_PROPS = 'libs/csharp/Rasm.Materials/.planning/Properties/properties.md'
const MAT_SUST = 'libs/csharp/Rasm.Materials/.planning/Properties/sustainability.md'
// Carried Properties residuals — seeded into the terminal reconcile so the retired rebuild-handoff/ feed is deletable; each is a real cross-FILE lockstep edit the budgeted Resolve pass fixes + adversarially verifies, the claim carrying the full authoritative data.
const CARRIED_RESIDUALS = [
  { files: [MAT_PROPS, MAT_SUST], claim:
    'ROSTER GAP — add aluminium EN AW-6082-T6 (the most common European structural extrusion alloy; EN 1999-1-1 names it first) in ' +
    'LOCKSTEP to BOTH Properties pages, preserving the dual-key roster parity (91=91 -> 92=92). properties.md engineering row ' +
    '(EN 1999-1-1 Table 3.2b, 5<t<=25mm): rho 2710, E 70000 MPa, f0 260, fu 310, nu 0.33, alpha 23.1e-6, lambda ~170, c ~897, fire ' +
    'B-s1,d0. sustainability.md EPD row shares the EU-Al-Profile extrusion eco-profile (A1A3 ~5.73, as 6061-T6 per the page ' +
    'GWP-tracks-mass-not-alloy doctrine), Uniclass Pr_20_85_08_02. Update BOTH pages [FULL_ROSTER]/[CATALOGUE_DOMAIN_COVERAGE] prose ' +
    'that lists aluminium as 6061-T6/6063-T5/5083 to include 6082-T6. Secondary candidate 7020 (EN 1999 names it less frequently) is not forced.' },
  { files: [MAT_PROPS, MAT_SUST], claim:
    'PROSE-VS-DATA PARITY DRIFT in properties.md — the rosters are now EXACT parity (every engineering id resolves a lifecycle row), ' +
    'retiring the old required-superset asymmetry, but properties.md prose still describes that asymmetry: near line 315 it claims ' +
    'insulation.phenolic is omitted from the lifecycle roster (FALSE — sustainability.md carries Kingspan-Kooltherm-Phenolic) and near ' +
    'line 384 it claims the rebar generic aliases / ASTM-CSA grades are EPD-roster-optional (FALSE — sustainability.md carries ' +
    'gr40/60/75/80/60w/80w/400w/500w/metal.steel/metal.iron). Rewrite the prose to state EXACT parity; sustainability.md ' +
    '[FULL_ROSTER_DEPTH] is the correct side and must not be weakened. ALSO confirm no other Materials/Element catalogue page repeats ' +
    'the Classification-as-MaterialPropertySet-case mislabel (Classification egress is the generic Classification value-object riding ' +
    'the MaterialBinding to the Object node) and that the MeasureValue/PropertyValue/PropertyName composes-the-seam phantom (listing ' +
    'them as composed members where a folder only passes raw doubles through seam smart-constructors) is absent in sibling catalogue pages.' },
]
const SITES = [
  { name: 'Compute/Analysis', root: 'libs/csharp/Rasm.Compute/.planning/Analysis', lang: 'cs',
    bind: 're-bind the analysis consumers to the changed seam; honor scope [1].4 resolution-through-Component / ' +
      'type-resolved accessors and do NOT silently break Compute Op-free graph.SectionOf(member)' },
  { name: 'Persistence/Element', root: 'libs/csharp/Rasm.Persistence/.planning/Element', lang: 'cs',
    bind: 're-bind to the changed seam / Type-node (ObjectKind.Type) shape; surface the Type-node persistence ' +
      'per scope [3] M4 without re-opening the egress-owner decision' },
  { name: 'Fabrication/seam-rows', root: 'libs/csharp/Rasm.Fabrication/.planning', lang: 'cs',
    bind: 're-bind the Fabrication seam rows per scope [5], each a FIRST-CLASS both-endpoint seam mirrored in ' +
      'Fabrication`s ARCHITECTURE.md [2]-[SEAMS] against its Materials producer: (1) the Construction/nesting producer ' +
      '(Materials Construction/nesting.md) -> the Fabrication/Nesting consumer — verify BOTH endpoints carry the ' +
      'mirrored row, align the Fabrication body to it, never re-author the Materials producer; (2) the Properties ' +
      'producer -> the Fabrication/Process consumer' },
  { name: 'python/data/decode', root: 'libs/python/data/.planning', lang: 'py',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the data decoders' },
  { name: 'python/geometry/mesh', root: 'libs/python/geometry/.planning/mesh', lang: 'py',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the mesh decoders' },
  { name: 'typescript/interchange/codec', root: 'libs/typescript/interchange/.planning/Codec', lang: 'ts',
    bind: 'DECODE-NOT-REMINT the changed Component/detail-schema wire in the interchange codec' },
]

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const UNDERUTIL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'capability'], properties: { catalog: { type: 'string' }, capability: { type: 'string' } } } }
const PIN_NEED = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package'], properties: { package: { type: 'string' }, current: { type: 'string' }, note: { type: 'string' } } } }
const FOLDER_MAP = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder'], properties: { folder: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } }, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, domainShards: { type: 'array', items: { type: 'string' } } } } }
const RIPPLE = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['detail'], properties: { detail: { type: 'string' }, files: { type: 'array', items: { type: 'string' } }, pages: { type: 'array', items: { type: 'string' } }, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, domainShards: { type: 'array', items: { type: 'string' } } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['site', 'ripples', 'summary'], properties: { site: { type: 'string' }, ripples: RIPPLE, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, domainShards: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const VALIDATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['validated', 'summary'], properties: { validated: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['site', 'ripples'], properties: { site: { type: 'string' }, ripples: RIPPLE, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, domainShards: { type: 'array', items: { type: 'string' } } } } }, dropped: { type: 'array', items: { type: 'string' } }, summary: { type: 'string' } } }
const FOLDER_FIXLOG = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: { folder: { type: 'string' }, verdict: { type: 'string', enum: ['closed', 'hardened', 'refined', 'clean'] }, integrated: { type: 'array', items: { type: 'string' } }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const SUBSTRATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, decision: { type: 'string' }, integrated: { type: 'array', items: { type: 'string' } }, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, pinsNeeded: PIN_NEED, residual_high: RESIDUAL, summary: { type: 'string' } } }
const PINS_SCHEMA = { type: 'object', additionalProperties: false, required: ['pinned', 'verdict', 'summary'], properties: { pinned: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'version'], properties: { package: { type: 'string' }, version: { type: 'string' } } } }, verdict: { type: 'string', enum: ['pinned', 'none'] }, restore: { type: 'string' }, summary: { type: 'string' } } }
const SWEEP_SCHEMA = { type: 'object', additionalProperties: false, required: ['verdict', 'summary'], properties: { verdict: { type: 'string', enum: ['aligned', 'clean'] }, aligned: { type: 'array', items: { type: 'string' } }, folderMap: FOLDER_MAP, residual_high: RESIDUAL, summary: { type: 'string' } } }
const FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'CAMPAIGN: the Rasm Materials/Bim/Element rebuild — WF-3 sibling-ripple, now the TERMINAL cross-libs master pass ' +
    '(the former seam-reconcile is FOLDED IN, run LAST). READ `' + SCOPE + '` (repo root) in FULL FIRST; a prompt ' +
    'POINTS to a scope section by number and never restates scope. [1] is the unified Material/Component/Element ' +
    'paradigm + the canonical Element seam; [3] the captured WATCH residuals; [4] CATALOG_LAW (NO new NuGet pin is ' +
    'required — package work is the LanguageExt.Core .api gap + the `' + SUBSTRATE + '/` README roster + the ' +
    'protobuf-net/MessagePack/Nerdbank.MessagePack redundancy); [5] the semantic-rename + boundary law ' +
    '(ProfileRef/ProfileSet/ComputedSection STAY seam-canonical); [7] the two-tier .api mandate; [8] the manifest rule.',
  'CROSS-WORKFLOW CONTRACT: WF-1 (element-component) + WF-2 (bim-rebuild) ALREADY landed the Element/Materials/Bim ' +
    'design changes AND the seam-mirror rows. This WF (1) re-binds the named sibling consumer sites to the changed ' +
    'Component/detail-schema wire and records each site`s seam row in its OWN `ARCHITECTURE.md` `[2]-[SEAMS]`, (2) ' +
    'hardens the shared `' + SUBSTRATE + '` .api substrate + confirms the manifest graph, (3) verifies every ' +
    'cross-libs seam mirrors on BOTH endpoints, and (4) drives every surfaced residual to zero within a hard ' +
    'reconcile budget. Do NOT re-open Element/Materials/Bim design here — align a body to an already-landed row, ' +
    'never re-author the row.',
  'These are DESIGN-PAGE specs (`.planning` markdown) — code fences ARE the product (transcription-complete, ' +
    'decompile-verified, implementation-ready). This WF does NOT re-author design pages from scratch — it re-binds ' +
    'siblings, hardens the .api substrate, confirms the manifest, verifies + repairs cross-libs seam mirroring, then ' +
    'drives all residuals to zero. The settled architecture is CANONICAL; CLAUDE.md WORKSPACE_LAW strata govern ' +
    '(KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward).',
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
  'TWO-TIER API (scope `[7]`): cite ONLY the `.api/` catalogs the concept composes — the SHARED substrate tier `' + SUBSTRATE +
    '/**` (treated EQUALLY with the folder tier, used whenever any folder composes a shared dependency) AND the ' +
    'folder tier `<package>/.api/**` — never noise. Stack BOTH tiers to exhaustion at the DEEPEST operator/' +
    'combinator/generated surface each package reaches; reject thin wrappers, BCL/stdlib-first reflexes, and ' +
    'surface-level subsets.',
  'MEMBER TRUTH: cite ONLY host/NuGet members verified via `uv run python -m tools.assay api` (a member you cannot ' +
    'verify is a PHANTOM — drop it); assay api OWNS member-existence truth and WINS on conflict. Live NuGet feed ' +
    'routes through the nuget MCP (ToolSearch-load `mcp__nuget__get_latest_package_version` + ' +
    '`mcp__nuget__get_package_context` by exact name); internal/library API shape resolves through Context7 ' +
    '(ToolSearch-load `mcp__plugin_context7_context7__resolve-library-id` + `mcp__plugin_context7_context7__query-docs`).',
].join('\n')
const DECODE = [
  'DECODE-NOT-REMINT (the sibling-ripple KEY LAW, scope `[1].5` + `[5]`): C# owns the wire vocabulary; the python/ts ' +
    'decoders RE-BIND to the changed Component/detail-schema wire and NEVER re-mint the C# contract. The neutral ' +
    'detail schema is Element-declared over `PropertyBag` + the canonical `PropertyName` vocabulary (scope `[1].5`, ' +
    'NEUTRAL — no `Pset_*` IFC names in the seam).',
  '`ProfileRef`/`ProfileSet`/`ComputedSection` STAY seam-canonical (scope `[5]`, the M7 one-hop resolution) — ' +
    '`Component` composes them unchanged and the semantic-rename STOPS at the Materials folder boundary.',
  'C#-SIBLING RE-BIND (scope `[1].4`): the analysis/persistence consumers resolve through the Component ' +
    '(resolution-through-Component / type-resolved accessors) WITHOUT silently breaking Compute Op-free ' +
    '`graph.SectionOf(member)`; the Type-node (`ObjectKind.Type`) shape and the named Bake type->occurrence ' +
    'inheritance are the seam WF-1 landed, consumed here, not re-authored.',
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
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY implementing stage is HOSTILE: assume the page is NAIVE, SHALLOW, JUNIOR, STALE, or ' +
    'ILLUSORY until it survives an aggressive attack, REGARDLESS of how it looks or what any prior pass claimed; the ' +
    'burden of proof is ON THE PAGE, never on you. "Mature", "already strong", "good enough", "done", and a prior ' +
    'clean verdict are REJECTED self-assessments — a no-edit verdict is earned ONLY after a genuinely aggressive ' +
    'attack on the real domain + the verified package surface finds nothing. A confident edit that does not TRULY ' +
    're-bind to the changed wire is INCOMPLETE, never done. Aggression is DEPTH and RIGOR, never churn — every edit ' +
    'cites a source (a package member, a domain attribute, a consumer contract); churn-for-appearance is rejected.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code PRETENDS to be advanced: it uses the ' +
    'doctrine vocabulary, cites packages, reads dense and confident, yet is HOLLOW. Treat dense, confident-looking ' +
    'fences with MORE suspicion, not less, and DISBELIEVE every claim the page makes about itself until you verify ' +
    'it against the real domain + the catalogued package surface. HUNT: a name/signature/prose promising capability ' +
    'the body does not implement; a "rich" decoder that decodes a thin slice of the wire; a stale decode left ' +
    'mid-flight; a seam mirrored on ONE endpoint but not the other; a `.api`/host member cited but never verified (a ' +
    'phantom). Every such illusion is a DEFECT to rebuild — never invent churn to look busy.',
  'WRITE-FULLY: every re-bind/gap/defect you identify you MAKE NOW via Edit/Write; the returned log REPORTS edits ' +
    'already made, never a to-do. A fix spanning a FILE you do not own goes to `residual_high` as `{files:[...], ' +
    'claim}` (the resource slot is a LIST so a cross-file seam names EVERY spanned file) for the terminal RESOLVE.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the named docs/stacks laws held as fact: [FLOW] EXPRESSION_SPINE (domain logic is ' +
    'expression-shaped; dependent steps bind monadically, independent ones accumulate applicatively; the carrier ' +
    'selects the algebra) + BOUNDARY_ADMISSION (raw admitted EXACTLY ONCE into an evidence-carrying owner; interior ' +
    'never re-validates). [SHAPE] SHAPE_BUDGET (one concept owns ONE type; variants are cases in one closed family) + ' +
    'DEEP_SURFACES + MODAL_ARITY (one entrypoint owns every modality, discriminating on input shape) + ' +
    'ANTICIPATORY_COLLAPSE. [DERIVATION] POLICY_VALUES + DERIVED_LOGIC + DERIVED_TYPES + SEMANTIC_NAMING. [MATERIAL] ' +
    'LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD (weave new capability into the owner as if ' +
    'always present; no shims/aliases/migration layers) + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION.',
  'ULTRA-ADVANCED COLLAPSE MANDATE: COLLAPSE >=3 parallel types / sibling factory methods / repeated dispatch arms ' +
    '/ single-call private helpers into ONE polymorphic owner IN THE SAME FILE (the language-doctrine ADT/union/' +
    'smart-enum/value-object/source-generated case family / fold algebra / frozen data table) — never extract a new ' +
    'file to reduce LOC, never delete capability.',
  'STACK CAPABILITY: ' + API_TIERS + ' MINE both tiers and LAYER the universal substrate rails onto the domain ' +
    'packages, woven as ONE dense rail at the DEEPEST operator/combinator/generated surface each package reaches ' +
    '(LIBRARY_DEPTH) — NOT flat one-shot per-API uses, NOT a surface-level subset, NOT a thin rename wrapper, NOT a ' +
    'BCL/stdlib-first reflex. A decoder/owner that leaves an admitted `.api` capability the wire/concept ADMITS ' +
    'unexploited is a capability-incompleteness defect. PRESERVE all capability (densify, never delete functionality); ' +
    'regress no correctness or strata law.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — structural collapse + `.api`-stacking are NECESSARY ' +
    'but NOT SUFFICIENT: a fully-collapsed decoder can still re-bind a NAIVE slice of the changed wire (the obvious 3 ' +
    'fields where the wire carries fifteen; a 2-case match where the seam has twenty). Close the gap by GROWING the ' +
    'existing owner — a case in the closed family, a row/richer column on the smart-enum or frozen table, a field, an ' +
    'operation, a policy value — per ANTICIPATORY_COLLAPSE + ROOT_REBUILD, NEVER a parallel type, a new file, or flat ' +
    'appended code.',
  'GAP SOURCES (every extension cites exactly one): (a) PACKAGE — a member the admitted `.api`/host surface exposes ' +
    'that the wire ADMITS but the page IGNORES (the `apiUnderutilized` suggestions; verify via `assay api`); (b) ' +
    'DOMAIN — an attribute/field/sub-kind/relationship the REAL changed seam demands but the decoder omits; (c) ' +
    'CONSUMER — a contract a downstream owner requires that has no composed spelling here yet. JUSTIFIED, NOT RANDOM: ' +
    'if after a real domain + package sweep the re-bind is genuinely complete, prove it by adding nothing — every ' +
    'added case/field/operation is load-bearing and cites its source; preserve ALL existing capability.',
].join('\n')
const READMANDATE = [
  'DOWNSTREAM READ MANDATE — BEFORE editing, READ: (1) ALL root `' + CORE_CS + '` core pages — the full set, every ' +
    'stage, never a subset (the C# floor governs the wire vocabulary even for the python/ts decoders; the python/ts ' +
    'sites ALSO read their own language stack `' + STACK.py + '` / `' + STACK.ts + '`). (2) the work-item`s ' +
    '`domainShards` — and ONLY those required shards (focused, not all 13). (3) the work-item`s `apiUsed` catalogs at ' +
    'full operator depth AND the `apiUnderutilized` capability to STACK into the owner (closing the underutilization) ' +
    '— BOTH `.api` tiers. (4) `' + SCOPE + '` for the central goal. ' + DOMAIN_ROSTER,
].join('\n')
const DOCTRINE = [LAW, '', DUAL, '', API, '', DECODE, '', SEAM, '', ADVERSARIAL, '', ULTRA, '', EXTEND, '', READMANDATE].join('\n')

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
const dedup2 = (rs) => [...new Map(rs.map((r) => [(r.catalog || '') + '|' + (r.capability || ''), r])).values()]
const chunk = (xs, n) => { const out = []; for (let i = 0; i < xs.length; i += n) out.push(xs.slice(i, i + n)); return out }
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const SITE_BY = new Map(SITES.map((s) => [s.name, s]))
const residualsOf = (rows, key) => rows.flatMap((r) => (r.residual_high || []).map((x) => norm(x, key(r))))
const discoverPrompt = (s) => [DOCTRINE, '', 'TASK: RIPPLE DISCOVERY for the sibling site `' + s.name + '` (folder `' + s.root + '/**`, CODE stack `' + STACK[s.lang] + '`). ' +
  'READ-ONLY — investigate, do NOT edit. Re-read `' + SCOPE + '` then EVERY page under `' + s.root + '/**` AND the CHANGED seam shape WF-1+WF-2 landed (the Element `Graph`/`Component` ' +
  'seam, the one seam-declared NEUTRAL detail schema over `PropertyBag` + `PropertyName`, the `ObjectKind.Type` Type-node + named Bake inheritance). SITE RE-BIND CHARTER: ' + s.bind + '. ' +
  'ENUMERATE both `.api` tiers (' + API_TIERS + ') and the full domain roster (' + DOMAIN_ROSTER + '). For EACH discovered page, MAP its composed catalogs (`apiUsed`, BOTH tiers), the ' +
  'catalogs/members it SHOULD compose but does not (`apiUnderutilized` as {catalog, capability} — the underutilization the re-bind must close), and the REQUIRED domain shard(s) ' +
  '(`domainShards`) its concern demands; members verified-local via `uv run python -m tools.assay api`. ' +
  'Return the PRECISE ripple work-list for this site ONLY — which seam signatures / decoders shifted and what each page MUST change to re-bind (decode-not-remint for the python/ts ' +
  'decoders; resolution-through-Component for the C# consumers). Each ripple is {detail, files, pages (the design pages the ripple touches), apiUsed, apiUnderutilized, domainShards} ' +
  'so the per-ripple reading map travels with it. ALSO return the site-level rollup apiUsed + apiUnderutilized + domainShards across all ripples. Return site + ripples + apiUsed + ' +
  'apiUnderutilized + domainShards + a summary of the dominant ripple class.'].join('\n')
const validatePrompt = (discovered) => [DOCTRINE, '', 'TASK: VALIDATE the discovered ripple across ALL sites (single barrier pass). READ-ONLY — investigate, do NOT edit. For EACH ' +
  'discovered ripple: CONFIRM it is REAL — the seam shape WF-1+WF-2 landed ACTUALLY changed in the direction the ripple claims, and every cited member (including the apiUsed/apiUnderutilized ' +
  'catalogs) is verified via `assay api` / Context7. DROP any PHANTOM ripple (a member that does not exist, a ripple whose seam did not actually change, or one that would re-mint the ' +
  'C# contract rather than decode it), and DROP any phantom `apiUnderutilized` suggestion (a catalog/member that does not exist or the page does not actually admit). PRESERVE each surviving ' +
  'ripple`s {detail, files, pages, apiUsed, apiUnderutilized, domainShards} map intact AND the site-level apiUsed/apiUnderutilized/domainShards rollup so the reading map reaches Rebind. Return ' +
  'the VALIDATED re-bind list per site (site + the SURVIVING ripples + apiUsed + apiUnderutilized + domainShards) + dropped (the phantom ripples/suggestions, each with the one-line reason) + ' +
  'summary. DISCOVERED:\n' + JSON.stringify(discovered, null, 1)].join('\n')
const rebindPrompt = (s) => [DOCTRINE, '', 'TASK: HOSTILE RE-BIND of the sibling site `' + s.name + '` (folder `' + s.root + '/**`, CODE stack `' + STACK[s.lang] + '`) to the changed ' +
  'Component/detail-schema wire — IN PLACE, decode-not-remint for the python/ts decoders, resolution-through-Component for the C# consumers. DISBELIEVE every existing decoder — assume it is ' +
  'naive/stale/illusory until proven world-class; do NOT polish, REBUILD the re-bind to the strongest CLEAN/MODERN form the site language doctrine + the DECODE-NOT-REMINT law admit, treating ' +
  'dense confident-looking code as a prime suspect for hollow re-binding. READING MAP (read ALL before editing per the DOWNSTREAM READ MANDATE): apiUsed=' + JSON.stringify(s.apiUsed || []) +
  '; apiUnderutilized=' + JSON.stringify(s.apiUnderutilized || []) + '; domainShards=' + JSON.stringify(s.domainShards || []) + '. For EACH validated ripple below, MAKE the re-bind edit NOW, ' +
  'COMPOSE its apiUsed catalogs at full operator depth AND STACK the apiUnderutilized capability into the owner (closing the underutilization) across BOTH `.api` tiers, preserving ' +
  '`ProfileRef`/`ProfileSet`/`ComputedSection` seam-canonical and never re-minting the C# contract; record the site`s seam row in its `ARCHITECTURE.md` `[2]-[SEAMS]` where applicable. Do ' +
  'NOT re-open Element/Materials/Bim design. Every edit cites a source (a package member / domain attribute / consumer contract) — no churn-for-appearance. A re-bind ' +
  'requiring a FILE outside this site goes to residual_high {files, claim}. This is a FOCUSED 2-ripple BATCH of the site`s validated ripples (the rest of the site is re-bound by sibling batch ' +
  'agents in parallel) — re-bind THESE ripples completely + aggressively. VALIDATED RIPPLES IN THIS BATCH (' + s.ripples.length + ', each carrying its own pages/apiUsed/apiUnderutilized/domainShards):\n' +
  JSON.stringify(s.ripples, null, 1) + '\nReturn folder (the EXACT site name `' + s.name + '`) + verdict + integrated (each ripple re-bound + where) + extended + residual_high + summary.'].join('\n')
const substratePrompt = () => [DOCTRINE, '', 'TASK: SUBSTRATE — harden the SHARED `' + SUBSTRATE + '/**` .api tier (rebuild-api 3-lens per catalog: EXTRACT-FULL the advanced surface -> REFINE to integration-shaped, STACKED ' +
  'rails -> HARDEN adversarially, removing every phantom/naive/un-stacked framing; members verified via `uv run python -m tools.assay api`). Then the THREE named substrate fills of scope [4]/[7]: (1) FILL the `' + SUBSTRATE +
  '/README.md` ROSTER so every substrate catalog carries a truthful roster row reflecting the FINAL state; (2) AUTHOR/DEEPEN the LanguageExt.Core `.api` catalog gap to full first-class capability (the substrate-tier rail every ' +
  'folder composes — Fin/Validation/Option/Eff + Seq/Iterable/Arr/Lst/HashSet/Set combinators, traversal, the v5-beta surface, every member assay-verified); (3) RESOLVE the protobuf-net vs MessagePack vs Nerdbank.MessagePack ' +
  'REDUNDANCY — DECIDE the ONE canonical wire-serialization owner on real evidence (member surface via `assay api` + newest-stable/license/supply-chain via the nuget MCP), RECORD the decision in the owning catalog, and ' +
  'PRUNE/REALIGN the others (drop each loser`s catalog to a one-line superseded note pointing at the owner, and flag the manifest implication). DEFERRED TOOLS — load the EXACT names via ToolSearch FIRST before any call: ' +
  '`mcp__nuget__get_latest_package_version`, `mcp__nuget__get_package_context`; `mcp__plugin_context7_context7__resolve-library-id`, `mcp__plugin_context7_context7__query-docs`. Edit ONLY under `' + SUBSTRATE + '/**`; a ' +
  'realignment that REQUIRES a folder design page or the manifest goes to residual_high {files, claim} (the serialization decision`s consumer realignments + the manifest pin/prune belong there). ALSO emit the substrate ' +
  'discovery map for the downstream Sweep/Resolve: `apiUsed` (every substrate catalog the campaign folders actively compose) + `apiUnderutilized` ({catalog, capability} — substrate capability the folders ADMIT but IGNORE, the ' +
  'underutilization the sweep/resolve must close). Return files + verdict + ' +
  'decision (the chosen serialization owner + why) + integrated + apiUsed + apiUnderutilized + pinsNeeded (every package the substrate work touched or the decision changes, for the Pins phase to confirm or prune) + residual_high + summary.'].join('\n')
const pinsPrompt = (pins) => [LAW, '', API, '', 'TASK: CONFIRM THE CENTRAL PIN GRAPH. The substrate phase touched/decided these packages (an EMPTY list means a pure graph confirmation):\n' + JSON.stringify(pins, null, 1) +
  '\nFor EACH: confirm the NEWEST stable version via the nuget MCP — ToolSearch-load `mcp__nuget__get_latest_package_version` + `mcp__nuget__get_package_context` FIRST (net10.0 TFM + commercial-safe license). HAND-EDIT `' +
  PROPS + '` ONLY if a pin must change — add/update the `<PackageVersion>` in the correct label-grouped cluster (sorted, one-line maintenance comment only), and REMOVE a pin the substrate serialization decision pruned — NEVER ' +
  '`dotnet add`. Scope [4] expects NO NEW package; the likely edits are confirm-only + the serialization prune. Then run `dotnet restore` AND `dotnet nuget why` to confirm the graph resolves (a restored assembly is required so ' +
  'any downstream `assay api` can decompile it). If nothing must change, confirm-and-log (verdict=none). Return pinned (each {package, version} you actually wrote) + verdict + the restore/why result + summary. Edit ONLY `' +
  PROPS + '`.'].join('\n')
const sweepPrompt = () => [DOCTRINE, '', 'TASK: FULL-LIBS SWEEP — the whole-stack one-shape-BOTH-ENDPOINTS seam/boundary verifier + truthfulness pass across ' + LIBS_SWEEP + '. (1) SEAMS/WIRES — every cross-folder/cross-stack ' +
  'producer/consumer relationship the campaign created mirrors on BOTH endpoints to ONE shared shape and is recorded in BOTH folders` `ARCHITECTURE.md` [2]-[SEAMS] with MIRRORED rows (a seam present on one side but ' +
  'missing/divergent on the other is the PRIMARY defect): the Element seam (IElementProjection/IGraphConstraint, the neutral edge algebra, MeasureValue, the unified Object/ObjectKind regime) <-> the Materials Component ' +
  'projection <-> the Bim ingest/egress projectors; the GeometryRef/RepresentationContentHash/ContentAddress content-key <-> Persistence <-> the Rasm kernel; the typed Material/Property/Assessment/Classification/section wire ' +
  'vocabulary <-> the libs/python AND libs/typescript decoders (decode-not-remint, never re-mint the C# contract); AND the JUST-REBOUND sibling seams — Compute/Analysis resolution-through-Component, Persistence/Element ' +
  'Type-node, the Fabrication rows (the Construction/nesting.md producer <-> Fabrication/Nesting consumer AND the Properties producer <-> Fabrication/Process consumer), the python/ts decoders — each mirrored on BOTH endpoints. ' +
  '(2) BOUNDARY FOLDERS — Rasm.AppHost reference-light (no AEC-domain ref), Rasm.AppUi pure-UI, Rasm.Rhino/Rasm.Grasshopper consume the seam Element/ElementGraph at the wire, Rasm.Fabrication references Rasm.Element; FIX any ' +
  'that consume a retired shape. (3) NO DUPLICATION / NO STRATA VIOLATION — a concept owned twice collapses to its rightful stratum owner; depend strictly upward; geometry/mesh/IFC at ONE wire owner per runtime. (4) DOC TRUTH — ' +
  'every index doc (each folder `ARCHITECTURE.md` codemap + [02]-[SEAMS] + `README.md` roster; each branch index; the cross-libs `libs/.planning/architecture.md` + `libs/csharp/.planning/ARCHITECTURE.md`) is TRUTHFUL to its ' +
  'pages. FIX in place across folders (design doc AND code fence, in any language — C#, Python, TypeScript); a counterpart edit you cannot complete goes to residual_high {files, claim}. ALSO, as you sweep each folder, ENUMERATE ' +
  'both `.api` tiers (substrate `' + SUBSTRATE + '/**` + the folder tier `<package>/.api/**`) and the full domain roster (' + DOMAIN_ROSTER + ') and EMIT a per-folder discovery map `folderMap` — one {folder, pages (the seam ' +
  'pages swept), apiUsed (catalogs the folder composes, BOTH tiers), apiUnderutilized ({catalog, capability} the folder ADMITS but IGNORES), domainShards (the required shard(s) the folder concern demands)} row per swept folder ' +
  '— so any cross-file residual the Resolve phase fixes carries the page-keyed reading map. Return verdict + aligned (each alignment, naming the folders + seam) + folderMap + residual_high + summary.'].join('\n')
const mapFor = (cl) => { const fm = (swept[0] && swept[0].folderMap) || []; const files = [...new Set(cl.flatMap((r) => r.files || []))]; const hit = fm.filter((m) => m.folder && files.some((f) => f.indexOf(m.folder) === 0 || (m.folder.indexOf('libs/') === 0 && f.indexOf(m.folder.split('/.planning')[0]) === 0))); const sub = (substrate[0] && substrate[0].apiUnderutilized) || []; return { apiUsed: [...new Set(hit.flatMap((m) => m.apiUsed || []))], apiUnderutilized: dedup2(hit.flatMap((m) => m.apiUnderutilized || []).concat(sub)), domainShards: [...new Set(hit.flatMap((m) => m.domainShards || []))] } }
const reconcileFix = (cl) => [DOCTRINE, '', 'READING MAP for these residuals (read ALL per the DOWNSTREAM READ MANDATE): ' + JSON.stringify(mapFor(cl)) + '. ', 'TASK: TERMINAL RECONCILE — fix EVERY one of these cross-FILE residuals the rebind/substrate/sweep phases surfaced; NO severity, NO ' +
  'leftovers, NO deferral. Read EVERY listed file across libs/ (csharp + python + typescript) + `' + PROPS + '` and FIX the real cross-file defect in place to the strongest clean/modern + seam-contract + ' +
  'decode-not-remint form (align the seam + every consumer in lockstep on BOTH endpoints, preserve `ProfileRef`/`ProfileSet`/`ComputedSection` seam-canonical, never re-mint the C# contract, repair strata/boundary, finish a ' +
  'substrate decision spanning files, make an index doc truthful), COMPOSING the apiUsed catalogs at full operator depth + STACKING the apiUnderutilized capability across BOTH `.api` tiers, preserving all capability — a token ' +
  'patch that leaves the seam misaligned is NOT a fix; if a residual is FACTUALLY WRONG, leave it and say why. Fix EVERY residual in this cluster in ONE pass — this is the ONLY fix agent this cluster receives. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerify = (cl, fixFiles) => [LAW, '', SEAM, '', DECODE, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim — re-read the named files from disk and CONFIRM the fix is ACTUALLY made AND ' +
  'complete + clean/modern + seam-conformant + decode-not-remint, not a token/naive patch. ATTACK it: shallow, partial, a rename that left a sibling decoder stale, a seam mirrored on one endpoint but not the other, a re-minted ' +
  'C# contract? Classify each: "fixed" (real, complete, non-naive), "invalid" (claim PROVABLY WRONG — cite why), or "open" (NOT fixed, OR fixed naively — a confident edit that does not truly resolve the cross-file defect is ' +
  'open, never fixed). Default "open" on ANY doubt; "invalid" ONLY when provably wrong. Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

log('sibling-ripple (terminal cross-libs): discover(' + SITES.length + ' sites) -> validate -> rebind(decode-not-remint, 2/agent) -> substrate(.api harden + fills) -> pins(graph) -> sweep(full-libs both-endpoints) -> resolve(BUDGET-CAPPED: <=' + FIX_CAP + ' fix / <=' + VERIFY_CAP + ' verify, ' + CARRIED_RESIDUALS.length + ' carried seeded); CAP=' + CAP)

// --- [DISCOVER]
phase('Discover')
const discovered = (await pool(SITES, CAP, (s) => agent(discoverPrompt(s), { label: 'discover:' + s.name, phase: 'Discover', schema: DISCOVER_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean)
log('Discover: ' + discovered.reduce((n, d) => n + (d.ripples || []).length, 0) + ' ripple(s) across ' + discovered.length + ' sites')

// --- [VALIDATE]
phase('Validate')
const validation = await agent(validatePrompt(discovered), { label: 'validate', phase: 'Validate', schema: VALIDATE_SCHEMA, effort: 'xhigh', stallMs: STALL })
const validated = (validation && validation.validated) || []
const validatedBySite = new Map(validated.map((v) => [v.site, v]))
const discoveredBySite = new Map(discovered.map((d) => [d.site, d]))
log('Validate: ' + validated.reduce((n, v) => n + (v.ripples || []).length, 0) + ' real ripple(s); ' + (((validation && validation.dropped) || []).length) + ' phantom dropped')

// --- [REBIND]
// Thread the per-site reading map (apiUsed/apiUnderutilized/domainShards) from validate into each rebind agent,
// falling back to the discover-stage site rollup so the map survives even if validate omitted the rollup. The
// IMPLEMENT pass batches at IMPLEMENT_BATCH ripples per agent (no implement agent handles more than 2 files at
// once) — each site's validated ripples chunk into 2-ripple sub-batches, 1 agent per batch via the shared pool.
phase('Rebind')
const rebindBatches = SITES.flatMap((s) => {
  const v = validatedBySite.get(s.name) || {}
  const d = discoveredBySite.get(s.name) || {}
  const apiUsed = v.apiUsed || d.apiUsed || []
  const apiUnderutilized = v.apiUnderutilized || d.apiUnderutilized || []
  const domainShards = v.domainShards || d.domainShards || []
  return chunk(v.ripples || [], IMPLEMENT_BATCH).map((ripples, b) => ({ ...s, ripples, apiUsed, apiUnderutilized, domainShards, batch: b }))
})
const rebound = (await pool(rebindBatches, CAP, (s) => agent(rebindPrompt(s), { label: 'rebind:' + s.name + ':b' + s.batch, phase: 'Rebind', schema: FOLDER_FIXLOG, effort: 'max', stallMs: STALL }))).filter(Boolean)

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
// BUDGET-CAPPED reconcile: at most FIX_CAP fix agents + VERIFY_CAP verify agents TOTAL — ONE pass, NO round-chasing,
// NO sanity re-audit loop (that is what spawned fix agents doing nothing after the first few). Cluster the union by
// shared files (union-find), process AT MOST FIX_CAP clusters; each cluster gets ONE fix agent and (if it changed a
// file, budget permitting) ONE verify agent. Every residual not closed within the budget — overflow clusters,
// verify-open, or no-file-change — is logged LOUDLY and handed to a downstream residual-fix run, NEVER re-chased.
const fbOf = (r) => { const site = SITE_BY.get(r.folder); return site ? site.root : 'libs/csharp' }
const union = dedup([
  ...CARRIED_RESIDUALS.map((r) => norm(r, 'libs/csharp')),
  ...residualsOf(rebound, fbOf),
  ...residualsOf(substrate, () => SUBSTRATE),
  ...residualsOf(swept, () => 'libs/csharp'),
])
phase('Resolve')
const clusters = cluster(union)
const capped = clusters.slice(0, FIX_CAP)
const overflow = clusters.slice(FIX_CAP).flat()
let verifyLeft = VERIFY_CAP
let invalid = []
let noFix = []
let stillOpen = []
if (union.length) {
  log('Resolve: ' + union.length + ' residual(s) -> ' + clusters.length + ' cluster(s); ONE budgeted pass over ' + capped.length + ' cluster(s) (<=' + FIX_CAP + ' fix, <=' + VERIFY_CAP + ' verify)' + (overflow.length ? '; ' + overflow.length + ' residual(s) overflow the budget -> loud pending' : ''))
  const resolved = (await pool(capped, CAP, async (cl) => {
    const fix = await agent(reconcileFix(cl), { label: 'resolve-fix', phase: 'Resolve', schema: FIX_SCHEMA, effort: 'max', stallMs: STALL })
    const touched = (fix && Array.isArray(fix.files) ? fix.files.filter(inLibs) : [])
    // No file-changing progress: nothing to verify; record as noFix (phantom/already-resolved), never re-spawn a fix.
    if (!fix || touched.length === 0 || fix.verdict === 'clean') return { open: [], invalid: [], noFix: cl }
    // Verify only while the verify budget remains; once spent, trust the fix (spawn no more verify agents).
    if (verifyLeft <= 0) return { open: [], invalid: [], noFix: [] }
    verifyLeft--
    const verify = await agent(reconcileVerify(cl, fix.files), { label: 'resolve-verify', phase: 'Resolve', schema: VERIFY_SCHEMA, effort: 'max', stallMs: STALL })
    const claims = (verify && verify.claims) || []
    const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
    const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
    return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), noFix: [] }
  })).filter(Boolean)
  invalid = dedup(resolved.flatMap((r) => r.invalid))
  noFix = dedup(resolved.flatMap((r) => r.noFix))
  const invalidKeys = new Set(invalid.map((x) => x.claim))
  stillOpen = dedup([...resolved.flatMap((r) => r.open), ...overflow]).filter((r) => !invalidKeys.has(r.claim))
  if (stillOpen.length) log('Resolve: ' + stillOpen.length + ' residual(s) STILL OPEN after the ' + FIX_CAP + '-fix / ' + VERIFY_CAP + '-verify budget — logged LOUDLY, handed to a downstream residual-fix run, NEVER chased with more agents')
  else log('Resolve: all ' + union.length + ' surfaced residual(s) fixed + verified within the budget')
} else { log('Resolve: no residuals surfaced — clean') }

return {
  workflow: 'sibling-ripple', sites: SITES.map((s) => s.name),
  ripples: discovered.map((d) => ({ site: d.site, ripples: (d.ripples || []).length })),
  dropped: ((validation && validation.dropped) || []).length,
  rebindVerdicts: rebound.map((r) => ({ site: r.folder, verdict: r.verdict })),
  substrateVerdict: substrate[0] && substrate[0].verdict,
  pinsVerdict: pins && pins.verdict, pinned: ((pins && pins.pinned) || []).map((p) => p.package + '@' + p.version),
  sweepVerdict: swept[0] && swept[0].verdict,
  surfacedResiduals: union.length, fixBudget: FIX_CAP, verifyBudget: VERIFY_CAP,
  invalidClaims: invalid.map((x) => x.claim),
  noFix: noFix.map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: stillOpen.map((x) => ({ files: x.files, claim: x.claim })),
  downstreamResidualFix: stillOpen.length > 0,
}
