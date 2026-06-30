export const meta = {
  name: 'component-panel-build',
  whenToUse: 'Add the tenth panel/sheet-goods ComponentFamily to Rasm.Materials at full generative-data + docs/stacks/csharp depth, woven in place into the Component owner, projection, construction, and index.',
  description: 'BUILD the PANEL/SHEET-goods Component family — the gap stated in libs/csharp/Rasm.Materials/README.md — as a NEW ComponentFamily (the tenth, joining masonry/cmu/steel/timber/glazing/reinforcement/fastener/connector/joint) at FULL generative-data + docs/stacks/csharp doctrine depth, woven IN PLACE into the existing Component owner + projection + construction + index, never a parallel surface and never a tenth ad-hoc type. Clones the loop-fixed properties-seed-depth shape (its no-progress reconcile loop-fix, multi-pass adversarial structure, two-tier-.api STACK, full-seed-depth research mandate, REFACTOR alignment, and the project-wide progress-gated reconcile) and prepends a load-bearing DECIDE phase: a single opus agent researches the panel goods (gypsum/drywall ASTM C1396 / EN 520, wood structural panels plywood+OSB APA / EN 13986, cement board ASTM C1325, gypsum sheathing, fiberboard, steel deck SDI, rigid-board EPS/XPS/polyiso insulation) and emits the family DESIGN coherently — the ComponentFamily axis case name, the panel ComponentSection [Union] arm, the per-panel-kind generative field set (board length/width/thickness, edge profile, face/core LAYER STACK, fastening field+edge pattern, orientation, deck rib geometry), the ComponentClass primary/minor assignment, the IFC IfcElementType/PredefinedType mapping per kind (IfcCovering/IfcPlate/IfcSlab/IfcDeck/IfcMember verified via the Bim IfcClass/AdmitPredefined egress vocabulary + GeometryGym surface), the Construction layup model, and the per-file edit plan — so every per-file agent aligns to ONE shape. Then REBUILD (one agent per target file, authoring to the DECIDED design), RECONCILE-A, CRITIQUE-1, REDTEAM-1, RECONCILE-B, CRITIQUE-2, REDTEAM-2, RECONCILE-C. The board SUBSTANCE resolves to a Properties material row while the buildable BOARD is the Component — that split is preserved exactly; the axis grows by ONE row, the ComponentSection by ONE arm, the projection by ONE fold case, breaking every dispatch site loudly at compile time as intended. TWO-TIER .api (libs/csharp/.api substrate + libs/csharp/Rasm.Materials/.api folder) stacked at every stage; rosters researched via Exa/Tavily/Context7 with REAL standards-grounded rows, no placeholders or invented values; members verified via uv run python -m tools.assay api. Each reconcile is a no-defer, progress-gated, project-wide drive-to-zero over union-find clusters, MAX_ROUNDS-bounded, breaking early on a no-progress round and logging any terminal pending loudly. Takes no args.',
  phases: [
    { title: 'Decide' },
    { title: 'Rebuild' },
    { title: 'Reconcile-A' },
    { title: 'Critique-1' },
    { title: 'Redteam-1' },
    { title: 'Reconcile-B' },
    { title: 'Critique-2' },
    { title: 'Redteam-2' },
    { title: 'Reconcile-C' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const MAX_ROUNDS = 6
const STALL = 600000
const IMPLEMENT_BATCH = 2
const REVIEW_BATCH = 4
const SCOPE = 'RASM-REBUILD-SCOPE.md'
const DECISION = 'RASM-REBUILD-DECISION.md'
const README = 'libs/csharp/Rasm.Materials/README.md'
const CORE_CS = 'docs/stacks/csharp/{README,language,shapes,surfaces-and-dispatch,rails-and-effects,boundaries,algorithms,system-apis}.md'
const DOMAIN_ROSTER = 'docs/stacks/csharp/domain/<shard>.md ENUMERATED ROSTER (13): runtime, concurrency, diagnostics, ' +
  'validation, resilience, transport, persistence, durability, postgres, data-interchange, compute, visuals, ' +
  'interaction. MAP each page concern -> its required shard(s): IFC/PredefinedType/wire codec -> transport + ' +
  'data-interchange; admission/property validation -> validation; catalog/loader/layup persistence -> persistence + ' +
  'durability; nesting/bin-pack/numeric -> compute; receipts/telemetry -> diagnostics; appearance/render -> visuals.'
// The NEW panel/sheet ComponentFamily owner page (the tenth family). Authored from scratch to
// the DECIDED design; the family is woven IN PLACE into the existing owners below, never parallel.
const PANEL = 'libs/csharp/Rasm.Materials/.planning/Component/panel.md'
// Existing owners the panel family grows IN PLACE (axis case <-> section arm <-> projection arm <->
// construction layup <-> index). Paths hardcoded for orchestrator determinism (no glob); each agent
// resolves the real on-disk casing under libs/csharp/Rasm.Materials/.planning/.
const COMPONENT = 'libs/csharp/Rasm.Materials/.planning/Component/component.md'
const PROJECTION = 'libs/csharp/Rasm.Materials/.planning/Projection/component.md'
const ASSEMBLY = 'libs/csharp/Rasm.Materials/.planning/Construction/assembly.md'
const LAYOUT = 'libs/csharp/Rasm.Materials/.planning/Construction/layout.md'
const INDEX = [README, 'libs/csharp/Rasm.Materials/ARCHITECTURE.md']
const FILES = [PANEL, COMPONENT, PROJECTION, ASSEMBLY, LAYOUT, ...INDEX]
// Bim egress vocabulary (IfcClass/PredefinedType admission) the projection arm maps onto — READ-ONLY
// alignment surfaces, never re-opened by this build.
const SEAM = [
  'libs/csharp/Rasm.Bim/.planning/Exchange/egress.md',
  'libs/csharp/Rasm.Element/.planning/Composition/material.md',
]

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
// Batched implement/review agents handle 2 (implement) or 4 (review) files per agent and return one
// per-file fix-log row each; the composition FLATTENS `logs` back to the per-file shape the reconcile flow expects.
const BATCH_FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['logs'], properties: { logs: { type: 'array', items: FIXLOG_SCHEMA }, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }
// DECIDE emits the ONE family design every per-file agent aligns to: the axis case name, the panel
// ComponentSection [Union] arm, the per-kind generative field set + IFC mapping, the layup model, and
// the per-file edit plan (which existing owner gains which case/arm/row). `design` is the human-shaped
// blueprint prose; `kinds`/`editPlan` are the structured spine the reconcile checks alignment against.
const UNDERUTIL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'capability'], properties: { catalog: { type: 'string' }, capability: { type: 'string' } } } }
const DECIDE_SCHEMA = { type: 'object', additionalProperties: false, required: ['familyCase', 'design', 'kinds', 'editPlan'], properties: {
  familyCase: { type: 'string' },
  design: { type: 'string' },
  kinds: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['kind', 'standard', 'componentClass', 'ifcType', 'predefinedType', 'fields'], properties: { kind: { type: 'string' }, standard: { type: 'string' }, componentClass: { type: 'string', enum: ['primary', 'minor'] }, ifcType: { type: 'string' }, predefinedType: { type: 'string' }, fields: { type: 'string' } } } },
  editPlan: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['file', 'edit'], properties: { file: { type: 'string' }, edit: { type: 'string' } } } },
  // Per-target reading map: for EACH FILES entry, the .api catalogs it composes now (apiUsed, BOTH tiers), the admitted
  // capability it ignores (apiUnderutilized {catalog, capability}), and the required domain shard(s) its concern demands.
  fileMap: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['file'], properties: { file: { type: 'string' }, apiUsed: { type: 'array', items: { type: 'string' } }, apiUnderutilized: UNDERUTIL, domainShards: { type: 'array', items: { type: 'string' } } } } },
  residual_high: RESIDUAL,
} }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'Rasm monorepo, libs/csharp planning corpus (markdown specs of intended C# package designs). CLAUDE.md manifest + WORKSPACE_LAW strata govern ' +
    '(KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward). GOAL: BUILD the PANEL/SHEET-goods Component family — the ' +
    'gap stated in `' + README + '` — as a NEW `ComponentFamily` (the TENTH, joining masonry/cmu/steel/timber/glazing/reinforcement/fastener/' +
    'connector/joint) at FULL generative-data + docs/stacks/csharp depth, woven IN PLACE into the existing Component owner + projection + construction ' +
    '+ index. NEW owner page `' + PANEL + '`; the family grows IN PLACE in `' + COMPONENT + '` (axis case + `ComponentSection` arm + `ComponentClass`), ' +
    '`' + PROJECTION + '` (projection fold arm + IFC mapping), `' + ASSEMBLY + '`/`' + LAYOUT + '` (panel layup/coursing), and the Materials index docs ' +
    '`' + INDEX[0] + '`/`' + INDEX[1] + '`. Resolve the real on-disk casing robustly under `libs/csharp/Rasm.Materials/.planning/` (folders may be ' +
    'cased differently in git on a case-insensitive FS); DO NOT change folder casing (out of scope). This is a planning-stage DESIGN PAGE build — code ' +
    'fences are treated as REAL. NEVER a parallel surface, NEVER a tenth ad-hoc type.',
  'MANDATORY STANDARDS — docs/stacks/csharp/ is the FLOOR, not the ceiling: every fence MUST meet docs/stacks/csharp/ (README, language, shapes, ' +
    'surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) AND the relevant docs/stacks/csharp/domain/ shard(s) for this ' +
    'page concern (data-interchange for the wire/codec, validation for the admission rail, persistence for the catalog/loader store), then PUSH ' +
    'PAST it to the objectively strongest form the doctrine admits. READ the relevant shard(s) and conform exactly. Cite ONLY host/NuGet members ' +
    'confirmed via `uv run python -m tools.assay api` (a member you cannot verify is a phantom — delete it).',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE items ' +
    '(report those in residual_high {files, claim} for the project-wide reconcile, which has NO scope cap).',
].join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (rebuild, critique, AND red-team) is HOSTILE and COLD: read the page FRESH from disk and assume the existing ' +
    'fence is NAIVE, SHALLOW, JUNIOR, or ILLUSORY until it survives an aggressive attack, REGARDLESS of how it looks or what any prior pass ' +
    'claimed; the burden of proof is ON THE CODE, never on you. "Mature", "already strong", "good enough", "done", "polished", and a prior `clean` ' +
    'verdict are REJECTED self-assessments — default to "this fence is naive and must be rebuilt to the strongest form the doctrine admits" and MAKE ' +
    'that rebuild; a no-edit verdict is earned ONLY after a genuinely aggressive attack on the real domain + the verified package surface finds ' +
    'nothing. Reject "good enough" categorically.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code PRETENDS to be advanced: it uses the doctrine vocabulary (`[Union]`/' +
    '`[SmartEnum<TKey>]`/`[ValueObject]`/`Fold`/the rails), cites packages, reads dense and confident — yet is HOLLOW. Treat dense, ' +
    'confident-looking fences with MORE suspicion, not less, and DISBELIEVE every claim the page makes about itself until you verify it against the ' +
    'real domain and the catalogued package surface. HUNT: a name/signature/prose promising capability the body does not implement; a "rich" owner ' +
    'that is a thin slice (a panel-kind roster with a sample 2 boards for a wide goods family; the obvious 3 fields where a board carries fifteen); ' +
    'decorative density carrying no real capability; a placeholder/stub/sketch/invented number dressed as a finished design; a `.api`/host member ' +
    'cited but never verified (a phantom). Every such illusion is a DEFECT to rebuild. Where you genuinely cannot break the fence, earn it by ' +
    'finding nothing — never invent churn.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the named laws of docs/stacks/csharp/README.md held as fact: [FLOW] EXPRESSION_SPINE (domain logic is expression-shaped; ' +
    'dependent steps `Bind` monadically, independent ones accumulate applicatively; the carrier selects the algebra) + BOUNDARY_ADMISSION (raw ' +
    'admitted EXACTLY ONCE into an evidence-carrying owner; interior never re-validates). [SHAPE] SHAPE_BUDGET (one concept owns ONE type; variants ' +
    'are cases in one closed family) + DEEP_SURFACES + MODAL_ARITY (one entrypoint owns every modality, discriminating on input shape) + ' +
    'ANTICIPATORY_COLLAPSE. [DERIVATION] POLICY_VALUES + DERIVED_LOGIC + DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] ' +
    'LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD (weave new capability into the owner as if always present; no shims/aliases/' +
    '[Obsolete]/migration layers) + ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION.',
  'ULTRA-ADVANCED COLLAPSE MANDATE: COLLAPSE >=3 parallel types / sibling factory methods / repeated switch arms / single-call private helpers into ' +
    'ONE polymorphic owner IN THE SAME FILE via `[Union]` / `[Union<T1,...>]` ad-hoc / `[SmartEnum<TKey>]` / `[SmartEnum]` keyless / ' +
    '`[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case families / `Fold` algebra / frozen data tables — never extract a new file ' +
    'to reduce LOC, never delete capability.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows raw -> admit ONCE (generated factory + validation partial; one rail bridge lifts the ' +
    'generated outcome into `Fin<T>` / `Validation<Error,T>`; `Option<T>` carries absence; exceptions convert at the owning boundary only) -> ' +
    'canonical owner -> unified rail -> projection -> egress. Interior code never re-validates, never sees `null`-as-failure, sentinels, or provider ' +
    'shapes; parameterize BOTH ingress AND egress so the same owner sources and sinks across many consumers without interior edits.',
  'PRESERVE all capability (densify, never delete functionality). Where a fence is dense, deepen; where it is flat/naive, rebuild ground-up. Never ' +
    'regress correctness or boundary/strata law. The NINE existing families and their pages are untouched EXCEPT where the shared owner signature ' +
    'legitimately widens — a new arm in a closed `[Union]` or a new axis row breaks every dispatch site LOUDLY at compile time, which is CORRECT ' +
    '(total generated `Switch`, no silent `_` arm), never a regression.',
].join('\n')
const STACK = [
  'TWO-TIER .api — STACK BOTH TIERS TO EXHAUSTION at EVERY stage (C# DOES have a central .api tier — there are TWO): (1) the SUBSTRATE tier ' +
    '`libs/csharp/.api/**` (universal cross-cutting catalogs + the universal Thinktecture / LanguageExt rails — generated domain shape, rails, ' +
    'effects, schedules, immutable collections — with MathNet / CSparse owning numeric algorithms); (2) the FOLDER tier ' +
    '`libs/csharp/Rasm.Materials/.api/**` (UnitsNet, UnitsNet.Serialization, the VividOrange family, MathNet, MessagePack, Unicolour, ' +
    'RectangleBinPack.CSharp, and the rest). MINE both tiers and LAYER the universal substrate rails onto the folder domain packages, woven as ONE ' +
    'dense rail (source-generated owners, `Fold` algebra, frozen data tables) at the DEEPEST operator/combinator/generated surface each package ' +
    'reaches (LIBRARY_DEPTH) — NOT flat one-shot per-API uses, NOT a surface-level subset, NOT thin rename wrappers, NOT a BCL-first reflex. A ' +
    'composed owner that leaves an admitted .api capability the concept ADMITS unexploited is a CAPABILITY-INCOMPLETENESS defect. Verify every cited ' +
    'member via `uv run python -m tools.assay api`; an unverifiable member is a phantom to delete.',
  'CATALOG_LAW seam discipline (RASM-REBUILD-SCOPE.md [1]/[4]/[7]): VividOrange owns steel SECTION catalogues (geometry), NOT panel goods; no admitted ' +
    'package owns panel-goods dimension tables, so the panel roster is HAND-ROLLED IN-FENCE as the curated authoritative roster grounded in the cited ' +
    'panel standard (ASTM C1396 / EN 520 gypsum, APA / EN 13986 wood structural panels, ASTM C1325 cement board, SDI steel deck, the EPS/XPS/polyiso ' +
    'rigid-board standards). UnitsNet is leveraged THROUGH the seam `MeasureValue.Of` — raw doubles in the fence, NO hand-rolled unit kernel. The ' +
    'panel-kind roster is FULL-depth and load-bearing with REAL standards-grounded numbers, never a thin sample, never padded or invented.',
].join('\n')
const SEEDLAW = [
  'FULL GENERATIVE DEPTH — the family models PANEL/SHEET buildable goods, and each panel kind carries the COMPLETE data a generator lays panels over ' +
    'a frame from, NOT the scalars an analysis reads. Author the in-fence panel roster transcription-complete with REAL standards-grounded values — ' +
    'NO placeholders, NO sample subsets, NO invented numbers:',
  'PANEL KINDS (DECIDE finalizes the roster + the IFC mapping per kind): gypsum board / drywall / sheetrock (ASTM C1396 / EN 520 — width/length/' +
    'thickness, edge profile tapered/square/beveled/rounded, core regular/Type-X fire/moisture/abuse, face paper); wood structural panels / sheathing ' +
    '— plywood + OSB (APA / EN 13986 — span rating, thickness, grade, exposure/bond class); cement board (ASTM C1325); gypsum sheathing; fiberboard; ' +
    'steel deck (SDI — form/composite/roof, rib depth/pitch/profile, gauge); rigid-board insulation (EPS/XPS/polyiso — thickness, facer, R-value).',
  'GENERATIVE FIELDS per panel: board length/width/thickness, edge profile, face/core LAYER STACK, fastening pattern (field + edge spacing, fastener ' +
    'type, edge distance), orientation, and the deck rib geometry (form/composite/roof, rib depth/pitch/profile, gauge) where structural. A panel is a ' +
    'Component, NOT a Property: the board SUBSTANCE resolves to a `Properties` material row (gypsum already in the `Properties` catalogue as a ' +
    'substance; cement/OSB/etc. substances are `Properties` rows the deepened catalogue carries or grows), while the buildable BOARD is the Component ' +
    '— PRESERVE this split exactly. ComponentClass + IFC: space-bounding coverings (gypsum/sheathing) -> `IfcCovering`/`IfcPlate`; structural deck -> ' +
    '`IfcPlate`/`IfcSlab`/`IfcDeck`-as-`PredefinedType`/`IfcMember` (DECIDE researches the exact `IfcElementType` + `PredefinedType` per kind, verified ' +
    'via the Bim `IfcClass`/`AdmitPredefined` egress vocabulary `' + SEAM[0] + '` + the GeometryGym surface; confirm cited members via `assay api`).',
  'RESEARCH the authoritative rosters via the research rails (ToolSearch-LOAD each by EXACT tool name FIRST — they are DEFERRED): Exa ' +
    '(`mcp__exa__web_search_exa` / `mcp__exa__web_search_advanced_exa` / `mcp__exa__web_fetch_exa`), Tavily (`mcp__tavily__tavily_search` / ' +
    '`mcp__tavily__tavily_extract`), Context7 (`mcp__plugin_context7_context7__resolve-library-id` / `mcp__plugin_context7_context7__query-docs`). ' +
    'Ground every dimension/grade/rib-profile/R-value in the cited ASTM/EN/APA/SDI standard; a row you cannot ground is omitted, never invented.',
].join('\n')
const REFACTOR = [
  'REFACTOR ALIGNMENT — READ `' + SCOPE + '` ([1] paradigm: Material=pure-substance / Materials owns the substance catalog SOURCE and the buildable ' +
    'Component; [2] generative-data law; [4] CATALOG_LAW; [7] TWO-TIER .api) AND `' + DECISION + '` (the EXECUTED Material/Component/Element ' +
    'architecture) and `' + README + '` (the panel-gap framing) and ALIGN to the settled paradigm: the panel family is ONE `ComponentFamily` row whose ' +
    '`ComponentSection` arm carries the board generative data; the ONE `ComponentProjector.Project` lowers it onto the `Rasm.Element` seam, minting the ' +
    'deterministic-rooted Type `Object`, stamping `Classification`/`PredefinedType` off the `ComponentSection` egress. The panel family ALIGNS to the ' +
    'settled `ComponentProjector` -> seam Type-Object lowering; it NEVER re-opens the Element/Component seam interior (`' + SEAM[1] + '` is READ-ONLY). ' +
    'A page contradicting the settled paradigm, the generative-data law, or the seam contract is a defect to fix TOWARD the settled architecture.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — EXTEND IN PLACE per ANTICIPATORY_COLLAPSE + ROOT_REBUILD: the axis grows by ONE row, ' +
    'the `ComponentSection` by ONE arm, the projection by ONE fold case, the construction layup by ONE coursing case. Structural collapse + two-tier ' +
    '.api-stacking are NECESSARY but NOT SUFFICIENT: a fully-collapsed owner can still model a NAIVE slice (a panel roster of 2 boards where the ' +
    'standard admits a wide goods family; the obvious 3 fields where a board carries fifteen). Close the gap by GROWING the existing owner — a case in ' +
    'the closed family, a row / richer column on the smart-enum or frozen table, a field / composed `[ValueObject]`/`[ComplexValueObject]`, an ' +
    'operation, a policy value — NEVER a parallel type, a new file beyond the one NEW panel owner page, or flat appended code.',
  'GAP SOURCES (every extension cites exactly one): (a) PACKAGE — a member the admitted UnitsNet / VividOrange / MathNet / MessagePack / Unicolour / ' +
    'RectangleBinPack / substrate surface exposes that the concept ADMITS but the page IGNORES (verify via `assay api`); (b) DOMAIN — a panel kind, a ' +
    'generative field (edge profile, core type, span rating, rib geometry, fastening pattern, facer, R-value), a layup case, or an operation the REAL ' +
    'ASTM/EN/APA/SDI standard demands but the page omits — for the panel roster this is the dominant axis: the missing KINDS and the missing FIELDS ' +
    'are domain gaps to fill to FULL generative depth; (c) CONSUMER — a contract the `ComponentProjector` lowering, the IFC egress, or the Construction ' +
    'layup requires that has no composed spelling here yet (the law extends first, the feature lands second).',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy; capability COVERAGE against the full domain + both .api tiers is the measure. A confident-looking ' +
    'panel roster is the PRIME suspect for a thin slice. JUSTIFIED, NOT RANDOM: if after a real domain + package + consumer sweep the roster is ' +
    'genuinely complete, prove it by adding nothing — never invent a kind or a field to look busy; every added kind/field/operation is load-bearing, ' +
    'cites its source, and composes the existing rails; preserve ALL existing capability and the nine existing families intact.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely — NEVER weak/unbounded/erased types; NEVER exception control flow in domain logic (use the LanguageExt ' +
    'typed rails / ROP and the route recovery patterns); NEVER imperative branching where a bounded vocabulary, frozen table, generated `Switch`, ' +
    'match, or `Fold` owns the variation; NEVER mutable accumulation for domain transforms (use immutable folds, projections, collection ' +
    'combinators). Total generated `Switch` with compile-time exhaustiveness (a new case breaks every dispatch site — NEVER a runtime-silent `_` ' +
    'arm). Typed algorithm receipts (NEVER a generic `IReceipt`/ledger) when fields carry route/status/standard/IFC-mapping/host evidence. ' +
    'The fault type is a CLOSED `[Union]` family deriving from the kernel `Expected` (a bare exception or a generic untyped `Error` for a ' +
    'multi-cause domain is a defect).',
  'Latest stable C# 14 on `net10.0` to the metal (`Nullable enable`, NRT enforced): primary constructors, collection expressions with spread, ' +
    '`params` collections (incl. `params ReadOnlySpan<T>`), list/slice/relational/logical pattern matching, switch expressions, `required` members, ' +
    '`file`-scoped types, `field` accessors, extension blocks (`extension(Receiver)`) and extension operators, generic math / static abstract+virtual ' +
    'interface members, `with` expressions, `System.Threading.Lock`, raw string + `u8` literals where they fit. Treat analyzer diagnostics as ' +
    'architecture pressure. Apply the docs/stacks/csharp file-organization + section-order law (canonical section order TYPES -> CONSTANTS -> ' +
    'MODELS -> ERRORS -> SERVICES -> OPERATIONS -> COMPOSITION -> EXPORTS; generated case families stay inside the declaring owner block). ' +
    'LanguageExt.Core 5.0.0-beta: Map/Choose/Filter/Fold/ToSeq exist ONLY on LanguageExt types — wrap a raw ImmutableArray / FrozenDictionary.' +
    'Values in `toSeq(...)` before a combinator; `.HeadOrNone()` is removed (use the `Seq.Head` Option property).',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package owner strictly in its lane and on its stratum; geometry/mesh/IFC meet at the wire with one ' +
  'owner per runtime; internal code uses canonical names and shapes with mapping only at the edge; do not trample a sibling owner while ' +
  'densifying; never introduce a downward dependency or leak a host type into a host-neutral owner. The panel family stays on the AEC-DOMAIN ' +
  'stratum and lowers to the Element seam at the wire — it never re-opens the seam interior, and the panel-board SUBSTANCE stays a Properties row ' +
  'while the buildable BOARD stays the Component.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
    'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
    'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Prefer a table, a typed signature block, or a tight ' +
    'bullet wherever it carries the design better than a paragraph. Prose that ASSERTS capability the fence does not implement is a defect.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, method, operator, package ID, path, command, flag, standard code, and literal value in ' +
    'backticks. Name the exact member/type/rail in backticks instead of paraphrasing. Trimming prose MUST NOT reduce technical density or remove ' +
    'design content. REAL transcription-complete code fences, ZERO placeholder/stub/TODO.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
  'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY ' +
  'where intent is not already obvious from names, types, and signatures: default ZERO comments on self-evident code; at most 1 line where a ' +
  'comment genuinely earns its place; 1-2 lines only for a truly subtle invariant. NO restating the code, no narration, no task/process/session/' +
  'history/proof/review comments, no XML-doc bloat.'
const READMANDATE = 'DOWNSTREAM READ MANDATE — BEFORE editing, READ: (1) ALL root `' + CORE_CS + '` core pages — the full set, ' +
  'every stage, never a subset. (2) the target file`s required `domainShards` — and ONLY those (focused, not all 13). (3) ' +
  'the target file`s `apiUsed` catalogs at full operator depth AND the `apiUnderutilized` capability to STACK into the ' +
  'owner (closing the underutilization) — BOTH .api tiers (substrate `libs/csharp/.api/**` + folder ' +
  '`libs/csharp/Rasm.Materials/.api/**`). (4) `' + SCOPE + '` for the central goal. ' + DOMAIN_ROSTER
const DOCTRINE = [LAW, '', ADVERSARIAL, '', ULTRA, '', STACK, '', SEEDLAW, '', REFACTOR, '', EXTEND, '', READMANDATE, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS].join('\n')

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
const baseOf = (p) => { const seg = p.split('/'); return seg[seg.length - 1] }
const inProject = (p) => typeof p === 'string' && (p.startsWith('libs/') || p.indexOf('/libs/') !== -1 || p === SCOPE || p === DECISION || /\.props$/.test(p) || /(pyproject\.toml|pnpm-workspace\.yaml)$/.test(p))
const norm = (x, fallback) => { if (typeof x === 'string') return { files: [fallback], claim: x }; const files = Array.isArray(x.files) ? x.files.filter(inProject) : []; return { files: files.length ? files : [fallback], claim: x.claim } }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const chunk = (xs, n) => { const out = []; for (let i = 0; i < xs.length; i += n) out.push(xs.slice(i, i + n)); return out }
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}

const decidePrompt = () => [DOCTRINE, '', 'TASK: LOAD-BEARING COHERENCE STEP — research the panel/sheet goods and EMIT the family DESIGN so every ' +
  'per-file build agent aligns to ONE shape. This is design-decision work, not page-authoring: you produce the blueprint the Rebuild agents author ' +
  'to. Read `' + README + '` (the panel-gap framing), `' + COMPONENT + '` (the `ComponentFamily [SmartEnum<string>]` axis closed at NINE + the ' +
  '`ComponentSection [Union]` + the `ComponentClass` discriminant), `' + PROJECTION + '` (the `ComponentProjector` fold + the IFC `Classification`/' +
  '`PredefinedType` stamping), `' + ASSEMBLY + '`/`' + LAYOUT + '` (the construction layup + placement folds), the Bim egress vocabulary `' + SEAM[0] +
  '` (the `IfcClass`/`AdmitPredefined` admission surface — READ-ONLY), `' + SCOPE + '` + `' + DECISION + '` for the settled paradigm + generative-data ' +
  'law, the docs/stacks/csharp standards, and BOTH .api tiers. RESEARCH the authoritative panel rosters via the ToolSearch-loaded Exa/Tavily/Context7 ' +
  'rails (every dimension/grade/rib-profile/R-value standards-grounded + REAL, never invented). DECIDE and RETURN coherently: (1) `familyCase` — the ' +
  'ONE `ComponentFamily` axis case name (the tenth row); (2) `design` — the blueprint prose: the panel `ComponentSection [Union]` arm shape, the ' +
  'shared generative-field model (board length/width/thickness, edge profile, face/core LAYER STACK, fastening field+edge pattern, orientation, deck ' +
  'rib geometry), how the `ComponentProjector` fold arm lowers it, and the Construction layup model (boards laid over a frame with edge/field ' +
  'fastening); (3) `kinds` — one row per panel kind with `{kind, standard, componentClass(primary|minor), ifcType, predefinedType, fields}`, the IFC ' +
  'mapping verified against the Bim egress vocabulary + the GeometryGym surface (cite members via `assay api`); (4) `editPlan` — one `{file, edit}` ' +
  'row per target naming the EXACT existing-owner edit point (which axis case/section arm/projection fold case/layup case/index row each of `' + PANEL +
  '`, `' + COMPONENT + '`, `' + PROJECTION + '`, `' + ASSEMBLY + '`, `' + LAYOUT + '`, `' + INDEX[0] + '`, `' + INDEX[1] + '` gains; the `' + INDEX[1] +
  '` `[02]-[SEAMS]` row ONLY if a genuinely new seam edge appears); (5) `fileMap` — one {file, apiUsed, apiUnderutilized, domainShards} row per target FILE (' +
  PANEL + ', ' + COMPONENT + ', ' + PROJECTION + ', ' + ASSEMBLY + ', ' + LAYOUT + ', ' + INDEX[0] + ', ' + INDEX[1] + '): ENUMERATE both `.api` tiers ' +
  '(the SUBSTRATE `libs/csharp/.api/**` + the FOLDER `libs/csharp/Rasm.Materials/.api/**`) and the full domain roster (' + DOMAIN_ROSTER + '), then MAP per ' +
  'file its composed catalogs (`apiUsed`, BOTH tiers), the catalogs/members it SHOULD compose but does not (`apiUnderutilized` as {catalog, capability} — the ' +
  'underutilization the build must close), and the REQUIRED domain shard(s) (`domainShards`) its concern demands, members verified-local via `uv run python ' +
  '-m tools.assay api`, so each per-file build/critique/redteam agent receives its own page-keyed reading map. The design MUST preserve the Component/Property split, grow the axis by ONE row + ' +
  'the section by ONE arm + the projection by ONE fold case (the nine existing families untouched), and align to the settled `ComponentProjector` -> ' +
  'seam Type-Object lowering without re-opening the seam. This step WRITES no page; report any cross-file precondition in residual_high {files, claim}.'].join('\n')

const authorPrompt = (page, design, readMap) => [DOCTRINE, '', 'DECIDED FAMILY DESIGN (author to THIS shape — it is the coherence anchor every per-file agent ' +
  'shares; align exactly, deviate only with a cited domain/package/consumer reason):\n' + design, '', 'READING MAP for `' + page + '` (read ALL before ' +
  'editing per the DOWNSTREAM READ MANDATE): ' + readMap + '. ', 'TASK: BUILD the panel/sheet `ComponentFamily` ' +
  'into `' + page + '` to the ULTRA-ADVANCED bar, at FULL generative depth, AND to domain-complete capability, AUTHORING TO THE DECIDED DESIGN above. ' +
  'If `' + page + '` is the NEW owner page `' + PANEL + '`, author it ground-up as the tenth-family owner; if it is an existing owner, GROW the panel ' +
  'family IN PLACE per the editPlan (the axis by ONE row, the `ComponentSection` by ONE arm, the projection by ONE fold case, the layup by ONE case, ' +
  'the index by its router/codemap row) — NEVER a parallel type, NEVER trampling the nine existing families. DISBELIEVE any existing fence — assume it ' +
  'is naive, junior, or illusory until proven world-class; do NOT polish, REBUILD to the strongest form the doctrine admits, and treat dense ' +
  'confident-looking code as a prime suspect for hollow/decorative complexity. Read `' + page + '` FRESH from disk, its sibling Component pages + the ' +
  'DECIDED design (cross-page unification), the projection + seam pages `' + SEAM[0] + '`/`' + SEAM[1] + '` (READ-ONLY — align to them, never perturb ' +
  'them), `' + SCOPE + '` + `' + DECISION + '` + `' + README + '` for the settled paradigm, the docs/stacks/csharp standards PLUS the data-interchange/' +
  'validation/persistence domain shards, and BOTH .api tiers (the SUBSTRATE `libs/csharp/.api/**` and the FOLDER `libs/csharp/Rasm.Materials/.api/**`) ' +
  '— MINE + STACK them with the universal Thinktecture/LanguageExt rails and VERIFY every cited member via `uv run python -m tools.assay api` (a member ' +
  'you cannot verify is a phantom — delete it). Construct in LIFECYCLE order (BOUNDARY_ADMISSION): admit raw EXACTLY ONCE through a generated factory + ' +
  'validation partial -> lift into the canonical owner chosen by the OWNER_CHOOSER discriminants -> weave every cross-cutting concern as a ' +
  'definition-time source-generated aspect or a composition-time effect transformer -> compose through ONE unified rail (`Fin`/`Validation`/`Option`/' +
  '`Eff`) with total generated `Switch` -> project + egress, BOTH ingress and egress parameterized. Collapse parallel shapes into one `[Union]`/' +
  '`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated case family IN THE SAME FILE; drive cases with a `Fold` algebra or a ' +
  'frozen data table. PRIMARY TARGET — FULL GENERATIVE DEPTH: author the in-fence panel roster to the FULL authoritative depth per the SEEDLAW above ' +
  '(RESEARCH the ASTM/EN/APA/SDI rosters via the ToolSearch-loaded Exa/Tavily/Context7 rails; every number standards-grounded + REAL, never ' +
  'placeholder or invented; the panel roster hand-rolled in-fence per CATALOG_LAW, UnitsNet leveraged THROUGH `MeasureValue.Of` with raw doubles in ' +
  'the fence; the board SUBSTANCE stays a `Properties` row while the buildable BOARD is the Component). Latest modern C# 14 on net10. High-signal prose ' +
  'all-backticked. Fix-in-place (read-then-rebuild, preserve capability + the nine existing families). Report what you collapsed (count before->after) ' +
  'in `collapsed` and what generative depth + capability you authored (each addition + its cited source) in `extended`; verdict `rebuilt` unless the ' +
  'fence genuinely survived untouched. Return the fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans, ' +
  'ANYWHERE in the project], claim} object for any CROSS-FILE item you surface but cannot fix from this one file (NO severity; the project-wide ' +
  'reconcile fixes all).'].join('\n')

const critiquePrompt = (page, readMap) => [DOCTRINE, '', 'READING MAP for `' + page + '` (read ALL before editing per the DOWNSTREAM READ MANDATE): ' +
  readMap + '. ', 'TASK: HOSTILE COLD DOCTRINAL-CONFORMANCE + CAPABILITY-COMPLETENESS + FULL-GENERATIVE-DEPTH AUDIT + ' +
  'FIX IN PLACE of `' + page + '` (the panel-family build). Read the page FRESH from disk and do an INDEPENDENT COLD review — assume the page is ' +
  'naive/junior/illusory REGARDLESS of prior passes, trust NOTHING the author or the prose claims, and "good enough"/"mature" is rejected outright. ' +
  'Read the sibling Component pages, the projection + seam pages (READ-ONLY), the operative docs/stacks/csharp pages AND the data-interchange/' +
  'validation/persistence shards, and BOTH .api tiers (SUBSTRATE + FOLDER) with the universal Thinktecture/LanguageExt rails. Run these MECHANICAL ' +
  'checklists line-by-line and REPAIR every hit in place (a fix, never a ledger note): (1) COLLAPSE_SCAN — any 3+ signal (sibling prefix names, ' +
  'same-rail arity siblings, literal-only variants, a bool/mode flag selecting two bodies, a single-call hop, parallel dispatch arms, a Get/GetMany/' +
  'List/Search family) collapses to one polymorphic owner / `Fold` algebra / frozen table IN THE SAME FILE. (2) OWNER_CHOOSER — re-derive each shape ' +
  'from the 5 discriminants and select the row, most-specific wins; kill every parallel DTO, one-field wrapper, nullable-as-failure, enum-dictionary ' +
  'pair, struct-`default` ghost. (3) KNOB_TEST — delete each parameter; a reconstructable value was a knob (collapse a bool/mode/strict/batch flag to ' +
  'a policy value or input-shape discriminant; a nullable tail to one `Option<ContextRecord>`; move timeout/retry/deadline/token OFF the signature ' +
  'onto the carrier or an aspect). (4) ASPECTS — definition-time concerns via source generation, composition-time concerns as effect transformers ' +
  '(`Schedule`-driven retry, named catch combinators, `Bracket` lifetime); 2-4 co-occurring wrappers collapse to one aspect; an aspect NEVER raises ' +
  'into domain flow. (5) RAILS — narrowest carrier chosen ONCE at admission; the fault type a CLOSED `[Union]` deriving from `Expected`; ' +
  'accumulate-vs-abort disposition correct; total generated `Switch` (no `_` arm); NO exception control flow, NO mutable accumulation; LanguageExt v5 ' +
  'combinator correctness. (6) STRATA/MEMBERS/MODERN — strata correctness, ONLY assay-verified members (no phantom), C# 14 on net10, FULL ' +
  'docs/stacks/csharp + shard conformance, BOTH .api tiers maximized. (7) CAPABILITY-COMPLETENESS + FULL-GENERATIVE-DEPTH + ILLUSION — structural ' +
  'collapse and capability completeness are ORTHOGONAL: verify the body implements what the names/prose promise; ANY panel kind, generative field ' +
  '(edge profile, core type, span rating, rib geometry, fastening pattern, facer, R-value), layup case, or IFC mapping the standard / the package / a ' +
  'consumer admits that the panel roster OMITS is a DEFECT — fill it to FULL generative depth in place with REAL standards-grounded numbers (research ' +
  'via the ToolSearch-loaded Exa/Tavily/Context7 rails); reject the inverse (a padded kind, an invented number, decorative ceremony, prose asserting ' +
  'capability the fence lacks — delete it). Confirm the Component/Property split (board substance stays a `Properties` row), the axis grows by ONE row ' +
  '+ section by ONE arm + projection by ONE fold case, and the nine existing families regress NOTHING. Also enforce the file-organization + ' +
  'section-order law, cross-page convention consistency, and prose + comment hygiene. EDIT the page to fix every hit. Report what you extended in ' +
  '`extended`. Return the fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans, anywhere in the project], ' +
  'claim} object for any CROSS-FILE item you cannot fix from this one file (NO severity; the project-wide reconcile fixes all).'].join('\n')

const redteamPrompt = (page, readMap) => [DOCTRINE, '', 'READING MAP for `' + page + '` (read ALL before editing per the DOWNSTREAM READ MANDATE): ' +
  readMap + '. ', 'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of `' + page + '` (the panel-family build) — the ' +
  'MOST AGGRESSIVE COLD pass. Read the page FRESH from disk and do an INDEPENDENT COLD review: assume the author and critique missed things and that ' +
  'the chosen design is naive or illusory until PROVEN the strongest, the burden of proof ON THE DESIGN; trust nothing prior passes or the prose ' +
  'claimed. Open BOTH .api tiers (SUBSTRATE + FOLDER) + the universal rails, the sibling Component pages, the projection + seam pages (READ-ONLY), `' +
  SCOPE + '`/`' + DECISION + '`/`' + README + '`, the operative docs/stacks/csharp pages + the data-interchange/validation/persistence shards. Attack ' +
  'from every direction and REPAIR every defect in place: (A) COUNTERFACTUAL on the core choice — is the owner, the algebra, and the dispatch form ' +
  'categorically the strongest the doctrine admits, or does a denser owner / a DEEPER admitted-package primitive (LanguageExt/Thinktecture/MathNet/' +
  'UnitsNet/MessagePack/RectangleBinPack) collapse the whole fence? rebuild to it, never defend the incumbent. (B) ANTICIPATORY_COLLAPSE — the next ' +
  'panel kind/generative field/IFC mapping/provider lands as ONE case/row/policy value with every consumer untouched or broken LOUDLY at compile time ' +
  '(total `Switch`, no silent `_`). (C) LONG-TAIL + MULTI-DIMENSIONAL — every input/output/edge/failure mode (empty, singular, plural, malformed, ' +
  'version-skew); accumulate-vs-abort correct for the REAL boundary; BOTH ingress AND egress parameterized. (D) STRATA + BOUNDARY-INTEGRITY — no ' +
  'downward dependency, no host-type leak, no concern owned twice, no coupling to the seam INTERIOR (vs its wire), the board SUBSTANCE stays a ' +
  '`Properties` row while the BOARD stays the Component. (E) SURFACE-SPRAWL + PHANTOMS — an admitted package whose .api or the universal rails expose ' +
  'capability the fence re-derives by hand, flat code below the operator depth the packages reach, a phantom member (cited but unverifiable — delete ' +
  'it), a thin wrapper: collapse to package depth + verify via `assay api`. (F) CAPABILITY-COMPLETENESS + FULL-GENERATIVE-DEPTH + ILLUSION — ' +
  'counterfactually attack the panel roster for FULL completeness independently of how collapsed it looks: does the ASTM/EN/APA/SDI standard / the ' +
  'package / a consumer admit a panel kind, generative field, layup case, or IFC mapping this roster still OMITS? Name it with a cite, RESEARCH the ' +
  'real number via the ToolSearch-loaded Exa/Tavily/Context7 rails, and EXTEND the owner IN PLACE to FULL depth; conversely REJECT any padded kind, ' +
  'invented number, or parallel surface, and confirm the nine existing families regress NOTHING. ALSO a FULL COLD ADVERSARIAL RE-REVIEW of every ' +
  'conformance dimension with fresh hostile eyes. The fence must end objectively denser, MORE CAPABLE, deeper in generative depth, more correct than ' +
  'the critique left it; if the strongest form is genuinely present, prove it by finding nothing. Report what you extended in `extended`. Return the ' +
  'fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans, anywhere in the project], claim} object for a ' +
  'CROSS-FILE item you cannot fix from one file (NO severity; the project-wide reconcile fixes all).'].join('\n')

const mapFor = (cl) => { const files = [...new Set(cl.flatMap((r) => r.files || []))]; const hit = files.map((f) => READMAP.get(f)).filter(Boolean); const dd = (xs) => [...new Map(xs.map((x) => [(x.catalog || '') + '|' + (x.capability || ''), x])).values()]; return JSON.stringify({ apiUsed: [...new Set(hit.flatMap((m) => m.apiUsed || []))], apiUnderutilized: dd(hit.flatMap((m) => m.apiUnderutilized || [])), domainShards: [...new Set(hit.flatMap((m) => m.domainShards || []))] }) }
const reconcileFix = (cl) => [DOCTRINE, '', 'READING MAP for these residuals (read ALL per the DOWNSTREAM READ MANDATE): ' + mapFor(cl) + '. ', 'TASK: PROJECT-WIDE RECONCILE — fix EVERY one of these cross-FILE residuals the decide/rebuild/critique/' +
  'redteam passes surfaced. There is NO severity — treat EVERY residual as must-address; NO leftovers, NO deferral, NO scope cap. Your blast radius ' +
  'is the ENTIRE project: you MAY read and fix ANY file ANYWHERE (libs/csharp, libs/python, libs/typescript, the central manifests — anything a ' +
  'cross-file residual spans), not only the Materials folder. Read EVERY listed file. For each: if it is a real cross-file defect, FIX it in place ' +
  'to the strongest clean/modern form the doctrine admits (unify the shared type/seam/rail, repair the strata/boundary issue, extend the shared ' +
  'owner in place to close a capability gap that spans files, finish the panel-family wiring spanning files — the axis case <-> `ComponentSection` ' +
  'arm <-> projection fold case <-> construction layup case <-> index row alignment, the IFC mapping <-> Bim egress alignment — make a doc truthful), ' +
  'ALIGNING to the settled Component/Element/Material paradigm + the generative-data law + both .api tiers and PRESERVING the seam contract + the ' +
  'nine existing families — a token patch that leaves the family wiring misaligned is NOT a fix; if a residual is FACTUALLY INCORRECT or not a real ' +
  'defect, leave it and say why in the summary — never silently skip a real one. If your fix surfaces a NEW cross-file need, report it in ' +
  'residual_high {files, claim}. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')

const reconcileVerify = (cl, fixFiles) => [LAW, '', BOUNDARIES, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Re-read the ' +
  'named files from disk and CONFIRM the fix is ACTUALLY made AND complete + high-quality + clean/modern + paradigm-conformant + seam-preserving, ' +
  'not a token/naive patch. ATTACK it: shallow, partial, a rename that left a sibling stale, a family wiring still misaligned (axis case without its ' +
  '`ComponentSection` arm or projection fold case or layup case), an IFC mapping unverified, a panel row still missing or invented? Classify each ' +
  'claim: "fixed" (real defect, now genuinely + completely resolved), "invalid" (the claim is PROVABLY factually wrong / not a real defect — cite ' +
  'why), or "open" (real defect still NOT resolved, or fixed naively/incompletely — redo). Default to "open" on ANY doubt; mark "invalid" ONLY when ' +
  'you can show the claim is wrong, and it is SURFACED (never silently dropped). Claims:\n' + JSON.stringify(cl, null, 1) + '\nFiles the fixer ' +
  'touched: ' + JSON.stringify(fixFiles)].join('\n')

// Reusable project-wide no-defer drive-to-zero: union-find cluster -> fix(max) -> adversarial verify(xhigh),
// `open` re-enters a fresh round, MAX_ROUNDS-bounded; surfaces never silently dropped. Called 3x.
const keyOf = (r) => r.files.slice().sort().join(',') + '|' + r.claim
const reconcile = async (tag, seed) => {
  let pending = dedup(seed)
  const seen = new Set(pending.map(keyOf))
  let invalid = []
  let noFix = []
  let round = 0
  if (!pending.length) { log(tag + ': no residuals surfaced — clean'); return { rounds: 0, open: [], invalid: [], noFix: [] } }
  while (pending.length && round < MAX_ROUNDS) {
    round++
    const clusters = cluster(pending)
    log(tag + ' round ' + round + ': ' + pending.length + ' residual(s) -> ' + clusters.length + ' cluster(s) (project-wide, no-defer)')
    const resolved = (await pool(clusters, CAP, async (cl, i) => {
      const fix = await agent(reconcileFix(cl), { label: 'reconcile-fix:' + tag + ':r' + round + ':' + i, phase: tag, schema: RESIDUAL_FIX_SCHEMA, effort: 'max', stallMs: STALL })
      const touched = (fix && Array.isArray(fix.files) ? fix.files.filter(inProject) : [])
      // No file-changing progress: the fix found nothing to change -> the cluster is resolved-or-phantom; skip the mandatory verify and drop it (recorded as noFix).
      if (!fix || touched.length === 0 || fix.verdict === 'clean') return { open: [], invalid: [], surfaced: [], dropped: cl, changed: false }
      const verify = await agent(reconcileVerify(cl, fix.files), { label: 'reconcile-verify:' + tag + ':r' + round + ':' + i, phase: tag, schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: STALL })
      const claims = (verify && verify.claims) || []
      const ok = new Set(claims.filter((c) => c.status === 'fixed').map((c) => c.claim))
      const bad = new Set(claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
      return { open: cl.filter((r) => !ok.has(r.claim) && !bad.has(r.claim)), invalid: cl.filter((r) => bad.has(r.claim)), surfaced: (fix.residual_high || []).map((x) => norm(x, FILES[0])), dropped: [], changed: true }
    })).filter(Boolean)
    invalid = dedup([...invalid, ...resolved.flatMap((r) => r.invalid)])
    noFix = dedup([...noFix, ...resolved.flatMap((r) => r.dropped)])
    const invalidKeys = new Set(invalid.map((r) => r.claim))
    // Re-enter ONLY genuinely-new residuals: a key already queued this call cannot re-enter (stops a phantom re-feeding every round).
    const fresh = dedup([...resolved.flatMap((r) => r.open), ...resolved.flatMap((r) => r.surfaced)]).filter((r) => !invalidKeys.has(r.claim) && !seen.has(keyOf(r)))
    fresh.forEach((r) => seen.add(keyOf(r)))
    pending = fresh
    // NO-PROGRESS BREAK: no cluster changed a file this round -> the remaining residuals are phantom/unfixable; stop instead of grinding to MAX_ROUNDS.
    if (!resolved.some((r) => r.changed)) { log(tag + ' round ' + round + ': no file-changing progress — ' + noFix.length + ' residual(s) had nothing to fix (phantom/resolved); breaking'); pending = []; break }
  }
  if (pending.length) log(tag + ': ' + pending.length + ' STILL OPEN after ' + MAX_ROUNDS + ' rounds — REPORTED LOUDLY, never silently dropped')
  else log(tag + ': all residuals fixed + adversarially verified across ' + round + ' round(s)')
  return { rounds: round, open: pending, invalid, noFix }
}

// Per-file reading map (from DECIDE's fileMap) injected into each batched implement/review prompt; empty when DECIDE omits a file.
let READMAP = new Map()
const readMapFor = (page) => JSON.stringify(READMAP.get(page) || { apiUsed: [], apiUnderutilized: [], domainShards: [] })
// BATCH wrapper: one agent processes EACH file in its batch (2 for implement, 4 for review) at the per-file prompt depth,
// each file carrying its OWN reading map, returning one `logs[]` row per file. The implement batch additionally carries the DECIDED design.
const batchAuthorPrompt = (pages, design) => ['BATCHED IMPLEMENT — process EACH of these ' + pages.length + ' target file(s) as a FULL, INDEPENDENT, ' +
  'aggressive per-file build (no file gets less attention for being batched); return one `logs` row per file (the per-file fix-log) + a batch summary. ' +
  'FILE PROMPTS:\n\n' + pages.map((p, i) => '=== FILE ' + (i + 1) + ': ' + p + ' ===\n' + authorPrompt(p, design, readMapFor(p))).join('\n\n')].join('\n')
const batchReviewPrompt = (kind, pages) => ['BATCHED REVIEW — process EACH of these ' + pages.length + ' target file(s) as a FULL, INDEPENDENT, ' +
  'aggressive per-file ' + (kind === 'crit' ? 'critique' : 'red-team') + ' (no file gets less attention for being batched); return one `logs` row per ' +
  'file (the per-file fix-log) + a batch summary. FILE PROMPTS:\n\n' + pages.map((p, i) => '=== FILE ' + (i + 1) + ': ' + p + ' ===\n' +
  (kind === 'crit' ? critiquePrompt(p, readMapFor(p)) : redteamPrompt(p, readMapFor(p)))).join('\n\n')].join('\n')
const runImplement = async (design) => (await pool(chunk(FILES, IMPLEMENT_BATCH), CAP, (batch, b) => agent(batchAuthorPrompt(batch, design), { label: 'author:b' + b + ':' + batch.map(baseOf).join('+'), phase: 'Rebuild', schema: BATCH_FIXLOG_SCHEMA, effort: 'max', stallMs: STALL }))).filter(Boolean).flatMap((r) => r.logs || [])
const runStage = async (kind, effort, phaseTag) => (await pool(chunk(FILES, REVIEW_BATCH), CAP, (batch, b) => agent(batchReviewPrompt(kind, batch), { label: kind + ':b' + b + ':' + batch.map(baseOf).join('+'), phase: phaseTag, schema: BATCH_FIXLOG_SCHEMA, effort, stallMs: STALL }))).filter(Boolean).flatMap((r) => r.logs || [])
const residualsOf = (rows) => rows.flatMap((r) => (r.residual_high || []).map((x) => norm(x, r.file || FILES[0])))

// --- [COMPOSITION] -----------------------------------------------------------------------

log('component-panel-build: 9-phase panel-family ADDITION across ' + FILES.length + ' target page(s) (full generative depth + two-tier .api) -> ' +
  'decide -> rebuild -> reconcile-A -> critique-1 -> redteam-1 -> reconcile-B -> critique-2 -> redteam-2 -> reconcile-C; CAP=' + CAP + ' MAX_ROUNDS=' + MAX_ROUNDS)

// --- [DECIDE]
phase('Decide')
const decided = await agent(decidePrompt(), { label: 'decide:panel-family', phase: 'Decide', schema: DECIDE_SCHEMA, effort: 'max', stallMs: STALL })
const design = decided ? JSON.stringify({ familyCase: decided.familyCase, design: decided.design, kinds: decided.kinds, editPlan: decided.editPlan }, null, 1)
  : '(decide agent returned no design — author from the README panel-gap framing + the SEEDLAW roster directly)'
const decideResiduals = decided ? (decided.residual_high || []).map((x) => norm(x, PANEL)) : []
// Index DECIDE's per-file reading map by target file so every batched implement/review agent reads its files' apiUsed/apiUnderutilized/domainShards.
READMAP = new Map(((decided && decided.fileMap) || []).filter((m) => m && m.file).map((m) => [m.file, { apiUsed: m.apiUsed || [], apiUnderutilized: m.apiUnderutilized || [], domainShards: m.domainShards || [] }]))

// --- [REBUILD]
phase('Rebuild')
const rebuilt = await runImplement(design)

// --- [RECONCILE_A]
phase('Reconcile-A')
const recA = await reconcile('Reconcile-A', decideResiduals.concat(residualsOf(rebuilt)))

// --- [CRITIQUE_1]
phase('Critique-1')
const crit1 = await runStage('crit', 'xhigh', 'Critique-1')

// --- [REDTEAM_1]
phase('Redteam-1')
const red1 = await runStage('redteam', 'max', 'Redteam-1')

// --- [RECONCILE_B]
phase('Reconcile-B')
const recB = await reconcile('Reconcile-B', residualsOf(crit1).concat(residualsOf(red1)))

// --- [CRITIQUE_2]
phase('Critique-2')
const crit2 = await runStage('crit', 'xhigh', 'Critique-2')

// --- [REDTEAM_2]
phase('Redteam-2')
const red2 = await runStage('redteam', 'max', 'Redteam-2')

// --- [RECONCILE_C]
phase('Reconcile-C')
const recC = await reconcile('Reconcile-C', residualsOf(crit2).concat(residualsOf(red2)).concat(recA.open).concat(recB.open))
if (recC.open.length) log('Reconcile-C: TERMINAL ' + recC.open.length + ' residual(s) STILL OPEN after ' + MAX_ROUNDS + ' rounds — HARD BLOCKER, ' +
  'reported LOUDLY, never silently dropped')

return {
  workflow: 'component-panel-build', files: FILES,
  familyCase: decided ? decided.familyCase : null,
  decided: decided ? { kinds: (decided.kinds || []).map((k) => ({ kind: k.kind, ifcType: k.ifcType, predefinedType: k.predefinedType })), editPlan: decided.editPlan || [] } : null,
  rebuilt: rebuilt.map((r) => ({ file: r.file, verdict: r.verdict })),
  critique1: crit1.map((r) => ({ file: r.file, verdict: r.verdict })),
  redteam1: red1.map((r) => ({ file: r.file, verdict: r.verdict })),
  critique2: crit2.map((r) => ({ file: r.file, verdict: r.verdict })),
  redteam2: red2.map((r) => ({ file: r.file, verdict: r.verdict })),
  reconcileRounds: { A: recA.rounds, B: recB.rounds, C: recC.rounds },
  invalidClaims: dedup([...recA.invalid, ...recB.invalid, ...recC.invalid]).map((x) => x.claim),
  noFix: dedup([...recA.noFix, ...recB.noFix, ...recC.noFix]).map((x) => ({ files: x.files, claim: x.claim })),
  openResidual: recC.open.map((x) => ({ files: x.files, claim: x.claim })),
}
