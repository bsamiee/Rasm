export const meta = {
  name: 'properties-seed-depth',
  whenToUse: 'Ultra-harsh adversarial ground-up rebuild of the 2 Materials Properties design pages to full seed depth + the docs/stacks/csharp bar.',
  description: 'ULTRA-HARSH adversarial GROUND-UP rebuild of the 2 Rasm.Materials Properties design pages (properties.md + sustainability.md) to FULL SEED DEPTH and the docs/stacks/csharp doctrine bar. Clones the canonical 1-2-3 adversarial doctrine from cs-rebuild-single (the hostile naive/junior/illusory cold-review stance, the [Union]/[SmartEnum]/[ValueObject] collapse mandate, the LanguageExt Fin/Validation/Option/Eff rails, the in-place capability-extension law) but expands it into an 8-phase multi-pass structure (Rebuild, Reconcile-A, Critique-1, Redteam-1, Reconcile-B, Critique-2, Redteam-2, Reconcile-C) with a PROJECT-WIDE reconcile blast radius. TWO-TIER .api: both the SUBSTRATE tier libs/csharp/.api/ and the FOLDER tier libs/csharp/Rasm.Materials/.api/ (19 catalogs incl. UnitsNet, the VividOrange family, MathNet, MessagePack, Unicolour) are ultra-stacked at every stage, layered onto the docs/stacks/csharp core pages + the data-interchange/validation/persistence domain shards. The PRIMARY capability-extension target is FULL SEED DEPTH: the dense in-fence seed tables grow to the full authoritative roster CATALOG_LAW admits with REAL standards-grounded numbers (EN 10025/10088 steels, EN 1992/206 concrete, EN 10080 rebar, EN 338/14080 timber, EN 1999 aluminium, EN 771 masonry, EN 572 glass, EN 13162-167 insulation; matching EN 15978 per-module StageGwp/biogenic/EoL EPD rows), researched via Exa/Tavily/Context7, never placeholders or invented values. The rebuild ALIGNS to the settled Material/Component/Element paradigm (Properties is the substance + EPD SOURCE the Component projection lowers onto the Element MaterialPropertySet seam) and PRESERVES the seam contract, never re-opening it. Each reconcile is a no-defer fix-then-verify drive-to-zero over union-find clusters, project-wide, MAX_ROUNDS-bounded, with a terminal pending logged loudly. Takes no args.',
  phases: [
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
const SCOPE = 'RASM-REBUILD-SCOPE.md'
const DECISION = 'RASM-REBUILD-DECISION.md'
// Folder is `Properties` on disk but `properties` in git on a case-insensitive FS; casing is a
// SEPARATE deferred concern, out of scope. Paths hardcoded for orchestrator determinism (no glob);
// the rebuild agent resolves the real on-disk casing under libs/csharp/Rasm.Materials/.planning/.
const FILES = [
  'libs/csharp/Rasm.Materials/.planning/Properties/properties.md',
  'libs/csharp/Rasm.Materials/.planning/Properties/sustainability.md',
]
const SEAM = [
  'libs/csharp/Rasm.Materials/.planning/Projection/component.md',
  'libs/csharp/Rasm.Element/.planning/Composition/material.md',
]

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['file', 'verdict', 'summary'], properties: { file: { type: 'string' }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'Rasm monorepo, libs/csharp planning corpus (markdown specs of intended C# package designs). CLAUDE.md manifest + WORKSPACE_LAW strata govern ' +
    '(KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward). TARGET: the TWO Rasm.Materials Properties design ' +
    'pages `' + FILES[0] + '` + `' + FILES[1] + '`. The folder is `Properties` on disk but `properties` in git on a case-insensitive FS — resolve ' +
    'the real on-disk casing robustly (glob `*propert*` under `libs/csharp/Rasm.Materials/.planning/`); DO NOT change the folder casing (a SEPARATE ' +
    'deferred concern, out of scope). This is a planning-stage DESIGN PAGE rebuild — code fences are treated as REAL.',
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
    'that is a thin slice (a seed table with a sample 5 grades for a 60-grade roster; the obvious 3 columns where the concept carries fifteen); ' +
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
    'regress correctness or boundary/strata law.',
].join('\n')
const STACK = [
  'TWO-TIER .api — STACK BOTH TIERS TO EXHAUSTION at EVERY stage (C# DOES have a central .api tier — there are TWO): (1) the SUBSTRATE tier ' +
    '`libs/csharp/.api/**` (universal cross-cutting catalogs + the universal Thinktecture / LanguageExt rails — generated domain shape, rails, ' +
    'effects, schedules, immutable collections — with MathNet / CSparse owning numeric algorithms); (2) the FOLDER tier ' +
    '`libs/csharp/Rasm.Materials/.api/**` (the 19 folder catalogs: UnitsNet, UnitsNet.Serialization, the VividOrange family, MathNet, MessagePack, ' +
    'Unicolour, and the rest). MINE both tiers and LAYER the universal substrate rails onto the folder domain packages, woven as ONE dense rail ' +
    '(source-generated owners, `Fold` algebra, frozen data tables) at the DEEPEST operator/combinator/generated surface each package reaches ' +
    '(LIBRARY_DEPTH) — NOT flat one-shot per-API uses, NOT a surface-level subset, NOT thin rename wrappers, NOT a BCL-first reflex. A composed ' +
    'owner that leaves an admitted .api capability the concept ADMITS unexploited is a CAPABILITY-INCOMPLETENESS defect. Verify every cited member ' +
    'via `uv run python -m tools.assay api`; an unverifiable member is a phantom to delete.',
  'CATALOG_LAW seam discipline (RASM-REBUILD-SCOPE.md [1]/[4]/[7]): VividOrange owns steel SECTION catalogues (geometry); Properties owns material ' +
    'GRADES (substance properties), hand-rolled in-fence as the curated authoritative roster. UnitsNet is leveraged THROUGH the seam `MeasureValue.' +
    'Of` — raw doubles in the fence, NO hand-rolled unit kernel. The seed is the FULL-depth curated roster, not a thin sample, and every row is ' +
    'load-bearing and REAL (standards-grounded numbers), never padded or invented.',
].join('\n')
const SEEDLAW = [
  'FULL SEED DEPTH — the PRIMARY capability-extension target for these 2 pages (the one open axis the prior assessment flagged). GROW the dense ' +
    'in-fence seed tables to the FULL authoritative roster CATALOG_LAW (RASM-REBUILD-SCOPE.md [4]) admits, transcription-complete with REAL ' +
    'standards-grounded numbers — NO placeholders, NO sample subsets, NO invented values:',
  '`' + FILES[0] + '` (properties.md) — every common structural-material GRADE with full mechanical/thermal/acoustic/fire columns: EN 10025 carbon ' +
    'steels (S235/S275/S355/S420/S460/S690) + EN 10088 stainless (1.4301/1.4307/1.4401/1.4404/1.4571 + duplex 1.4462), EN 1992/EN 206 concrete ' +
    'strength classes (C12/15...C90/105 + LC lightweight), EN 10080 rebar (B500A/B/C), EN 338 sawn timber (C14...C50, D18...D70) + EN 14080 glulam ' +
    '(GL24h/c...GL32h/c), EN 1999 aluminium alloys (6061-T6/6063-T5/5083), EN 771 masonry units (clay/calcium-silicate/AAC/aggregate), EN 572 ' +
    'glass, EN 13162-167 insulation (mineral wool/EPS/XPS/PIR/PUR/wood-fibre), gypsum boards, membranes (EPDM/PVC/TPO).',
  '`' + FILES[1] + '` (sustainability.md) — matching EPD/GWP rows for that FULL roster: per-EN-15978-module `StageGwp` vectors (A1-A3/A4/A5/B/C1-C4/' +
    'D), signed biogenic carbon, recycled + EoL fractions, declared-quantity basis (PerKg/PerM2/PerM3), supply/install/lifecycle cost, and a real ' +
    'classification code (Uniclass 2015 / OmniClass).',
  'RESEARCH the authoritative rosters via the research rails (ToolSearch-LOAD each by EXACT tool name FIRST — they are DEFERRED): Exa ' +
    '(`mcp__exa__web_search_exa` / `mcp__exa__web_search_advanced_exa` / `mcp__exa__web_fetch_exa`), Tavily (`mcp__tavily__tavily_search` / ' +
    '`mcp__tavily__tavily_extract`), Context7 (`mcp__plugin_context7_context7__resolve-library-id` / `mcp__plugin_context7_context7__query-docs`). ' +
    'Ground every number in the cited EN/EPD standard; a row you cannot ground is omitted, never invented.',
].join('\n')
const REFACTOR = [
  'REFACTOR ALIGNMENT — READ `' + SCOPE + '` ([1] Material=pure-substance / Materials owns the substance catalog SOURCE; [4] CATALOG_LAW; [7] ' +
    'TWO-TIER .api) AND `' + DECISION + '` (the EXECUTED Material/Component/Element architecture) and ALIGN to the settled paradigm: Properties is ' +
    'the Material-substance + EPD SOURCE that the ONE Component projection (`' + SEAM[0] + '`) lowers onto the Element seam `MaterialPropertySet` ' +
    '(`' + SEAM[1] + '`). The SEAM owns the property-TYPE union; Properties owns the substance DATA + the `Admit`/`Lower`/`Lookup` loader rail + the ' +
    'seed tables. HARDEN this alignment and PRESERVE the just-rebuilt seam contract — NEVER re-open or perturb the Element/Component seam, only ' +
    'deepen the Properties source and keep the lowering coherent. A page contradicting the settled paradigm or the seam contract is a defect to fix ' +
    'TOWARD the settled architecture.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — structural collapse + two-tier .api-stacking are NECESSARY but NOT SUFFICIENT: a ' +
    'fully-collapsed owner can still model a NAIVE slice (a seed roster of 5 grades where the standard admits 60; the obvious 3 columns where the ' +
    'concept carries fifteen). Structural completeness and CAPABILITY completeness are ORTHOGONAL. Close the gap by GROWING the existing owner — a ' +
    'case in the closed family, a row / richer column on the smart-enum or frozen table, a field / composed `[ValueObject]`/`[ComplexValueObject]`, ' +
    'an operation, a policy value — per ANTICIPATORY_COLLAPSE + COMPOSED_IMPLEMENTATION + ROOT_REBUILD, NEVER a parallel type, a new file, or flat ' +
    'appended code.',
  'GAP SOURCES (every extension cites exactly one): (a) PACKAGE — a member the admitted UnitsNet / VividOrange / MathNet / MessagePack / Unicolour / ' +
    'substrate surface exposes that the concept ADMITS but the page IGNORES (verify via `assay api`); (b) DOMAIN — a mechanical / thermal / ' +
    'acoustic / fire / EPD attribute, a material grade, a lifecycle module, or an operation the REAL standard demands but the page omits — for the ' +
    'SEED tables this is the dominant axis: the missing GRADES and the missing COLUMNS are domain gaps to fill to FULL roster depth; (c) CONSUMER — ' +
    'a contract the Component projection or the Element seam requires that has no composed spelling here yet (the law extends first, the feature ' +
    'lands second).',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy; capability COVERAGE against the full domain + both .api tiers is the measure. A confident-looking ' +
    'seed table is the PRIME suspect for a thin slice. JUSTIFIED, NOT RANDOM: if after a real domain + package + consumer sweep the roster is ' +
    'genuinely complete, prove it by adding nothing — never invent a grade or a column to look busy; every added grade/column/field/operation is ' +
    'load-bearing, cites its source, and composes the existing rails; preserve ALL existing capability.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely — NEVER weak/unbounded/erased types; NEVER exception control flow in domain logic (use the LanguageExt ' +
    'typed rails / ROP and the route recovery patterns); NEVER imperative branching where a bounded vocabulary, frozen table, generated `Switch`, ' +
    'match, or `Fold` owns the variation; NEVER mutable accumulation for domain transforms (use immutable folds, projections, collection ' +
    'combinators). Total generated `Switch` with compile-time exhaustiveness (a new case breaks every dispatch site — NEVER a runtime-silent `_` ' +
    'arm). Typed algorithm receipts (NEVER a generic `IReceipt`/ledger) when fields carry route/status/sampling/standard/EPD-module/host evidence. ' +
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
  'densifying; never introduce a downward dependency or leak a host type into a host-neutral owner. Properties stays on the AEC-DOMAIN stratum and ' +
  'lowers to the Element seam at the wire — it never re-opens the seam interior.'
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
const DOCTRINE = [LAW, '', ADVERSARIAL, '', ULTRA, '', STACK, '', SEEDLAW, '', REFACTOR, '', EXTEND, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS].join('\n')

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
const cluster = (residuals) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of residuals) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of residuals) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}

const authorPrompt = (page) => [DOCTRINE, '', 'TASK: ULTRA-HARSH HOSTILE GROUND-UP REBUILD of `' + page + '` to the ULTRA-ADVANCED bar, to FULL ' +
  'SEED DEPTH, AND to domain-complete capability. DISBELIEVE the page — assume every fence is naive, junior, or illusory until proven world-class; ' +
  'do NOT polish what is there, REBUILD it to the strongest form the doctrine admits, and treat dense confident-looking code as a prime suspect for ' +
  'hollow/decorative complexity. Read the page FRESH from disk, its sibling Properties page (cross-page unification), the seam pages `' + SEAM[0] + ' ' +
  '`/`' + SEAM[1] + '` (READ-ONLY — align to them, never perturb them), `' + SCOPE + '` + `' + DECISION + '` for the settled paradigm, the ' +
  'docs/stacks/csharp standards PLUS the data-interchange/validation/persistence domain shards, and BOTH .api tiers (the SUBSTRATE ' +
  '`libs/csharp/.api/**` and the FOLDER `libs/csharp/Rasm.Materials/.api/**`) — MINE + STACK them with the universal Thinktecture/LanguageExt rails ' +
  'and VERIFY every cited member via `uv run python -m tools.assay api` (a member you cannot verify is a phantom — delete it). Construct in ' +
  'LIFECYCLE order (BOUNDARY_ADMISSION): admit raw EXACTLY ONCE through a generated factory + validation partial -> lift into the canonical owner ' +
  'chosen by the OWNER_CHOOSER discriminants -> weave every cross-cutting concern as a definition-time source-generated aspect or a ' +
  'composition-time effect transformer -> compose through ONE unified rail (`Fin`/`Validation`/`Option`/`Eff`) with total generated `Switch` -> ' +
  'project + egress, BOTH ingress and egress parameterized. Collapse parallel shapes into one `[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/' +
  '`[ComplexValueObject]`/source-generated case family IN THE SAME FILE; drive cases with a `Fold` algebra or a frozen data table. PRIMARY TARGET — ' +
  'FULL SEED DEPTH: grow the dense in-fence seed tables to the FULL authoritative roster per the SEEDLAW above (RESEARCH the EN/EPD rosters via the ' +
  'ToolSearch-loaded Exa/Tavily/Context7 rails; every number standards-grounded + REAL, never placeholder or invented; Properties owns GRADES ' +
  'hand-rolled in-fence, UnitsNet leveraged THROUGH `MeasureValue.Of` with raw doubles in the fence). Latest modern C# 14 on net10. High-signal ' +
  'prose all-backticked. Fix-in-place (read-then-rebuild, preserve capability). Report what you collapsed (count before->after) in `collapsed` and ' +
  'what capability + seed depth you extended (each addition + its cited source) in `extended`; verdict `rebuilt` unless the fence genuinely ' +
  'survived untouched. Return the fix-log + residual_high — each a {files: [every repo-relative path the cross-file fix spans, ANYWHERE in the ' +
  'project], claim} object for any CROSS-FILE item you surface but cannot fix from this one file (NO severity; the project-wide reconcile fixes all).'].join('\n')

const critiquePrompt = (page) => [DOCTRINE, '', 'TASK: HOSTILE COLD DOCTRINAL-CONFORMANCE + CAPABILITY-COMPLETENESS + FULL-SEED-DEPTH AUDIT + FIX ' +
  'IN PLACE of `' + page + '`. Read the page FRESH from disk and do an INDEPENDENT COLD review — assume the page is naive/junior/illusory REGARDLESS ' +
  'of prior passes, trust NOTHING the author or the prose claims, and "good enough"/"mature" is rejected outright. Read the sibling Properties page, ' +
  'the seam pages (READ-ONLY), the operative docs/stacks/csharp pages AND the data-interchange/validation/persistence shards, and BOTH .api tiers ' +
  '(SUBSTRATE + FOLDER) with the universal Thinktecture/LanguageExt rails. Run these MECHANICAL checklists line-by-line and REPAIR every hit in ' +
  'place (a fix, never a ledger note): (1) COLLAPSE_SCAN — any 3+ signal (sibling prefix names, same-rail arity siblings, literal-only variants, a ' +
  'bool/mode flag selecting two bodies, a single-call hop, parallel dispatch arms, a Get/GetMany/List/Search family) collapses to one polymorphic ' +
  'owner / `Fold` algebra / frozen table IN THE SAME FILE. (2) OWNER_CHOOSER — re-derive each shape from the 5 discriminants and select the row, ' +
  'most-specific wins; kill every parallel DTO, one-field wrapper, nullable-as-failure, enum-dictionary pair, struct-`default` ghost. (3) KNOB_TEST ' +
  '— delete each parameter; a reconstructable value was a knob (collapse a bool/mode/strict/batch flag to a policy value or input-shape ' +
  'discriminant; a nullable tail to one `Option<ContextRecord>`; move timeout/retry/deadline/token OFF the signature onto the carrier or an aspect). ' +
  '(4) ASPECTS — definition-time concerns via source generation, composition-time concerns as effect transformers (`Schedule`-driven retry, named ' +
  'catch combinators, `Bracket` lifetime); 2-4 co-occurring wrappers collapse to one aspect; an aspect NEVER raises into domain flow. (5) RAILS — ' +
  'narrowest carrier chosen ONCE at admission; the fault type a CLOSED `[Union]` deriving from `Expected`; accumulate-vs-abort disposition correct; ' +
  'total generated `Switch` (no `_` arm); NO exception control flow, NO mutable accumulation; LanguageExt v5 combinator correctness. (6) STRATA/' +
  'MEMBERS/MODERN — strata correctness, ONLY assay-verified members (no phantom), C# 14 on net10, FULL docs/stacks/csharp + shard conformance, BOTH ' +
  '.api tiers maximized. (7) CAPABILITY-COMPLETENESS + FULL-SEED-DEPTH + ILLUSION — structural collapse and capability completeness are ORTHOGONAL: ' +
  'verify the body implements what the names/prose promise; ANY grade, column, lifecycle module, or EPD attribute the standard / the package / a ' +
  'consumer admits that the seed roster OMITS is a DEFECT — fill it to FULL roster depth in place with REAL standards-grounded numbers (research via ' +
  'the ToolSearch-loaded Exa/Tavily/Context7 rails); reject the inverse (a padded grade, an invented number, decorative ceremony, prose asserting ' +
  'capability the fence lacks — delete it). Also enforce the file-organization + section-order law, cross-page convention consistency, and prose + ' +
  'comment hygiene. EDIT the page to fix every hit. Report what you extended in `extended`. Return the fix-log + residual_high — each a {files: ' +
  '[every repo-relative path the cross-file fix spans, anywhere in the project], claim} object for any CROSS-FILE item you cannot fix from this one ' +
  'file (NO severity; the project-wide reconcile fixes all).'].join('\n')

const redteamPrompt = (page) => [DOCTRINE, '', 'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE of `' + page + '` — the MOST AGGRESSIVE COLD ' +
  'pass. Read the page FRESH from disk and do an INDEPENDENT COLD review: assume the author and critique missed things and that the chosen design is ' +
  'naive or illusory until PROVEN the strongest, the burden of proof ON THE DESIGN; trust nothing prior passes or the prose claimed. Open BOTH .api ' +
  'tiers (SUBSTRATE + FOLDER) + the universal rails, the sibling Properties page, the seam pages (READ-ONLY), `' + SCOPE + '`/`' + DECISION + '`, ' +
  'the operative docs/stacks/csharp pages + the data-interchange/validation/persistence shards. Attack from every direction and REPAIR every defect ' +
  'in place: (A) COUNTERFACTUAL on the core choice — is the owner, the algebra, and the dispatch form categorically the strongest the doctrine ' +
  'admits, or does a denser owner / a DEEPER admitted-package primitive (LanguageExt/Thinktecture/MathNet/UnitsNet/MessagePack) collapse the whole ' +
  'fence? rebuild to it, never defend the incumbent. (B) ANTICIPATORY_COLLAPSE — the next grade/column/lifecycle-module/provider lands as ONE case/' +
  'row/policy value with every consumer untouched or broken LOUDLY at compile time (total `Switch`, no silent `_`). (C) LONG-TAIL + ' +
  'MULTI-DIMENSIONAL — every input/output/edge/failure mode (empty, singular, plural, malformed, version-skew); accumulate-vs-abort correct for the ' +
  'REAL boundary; BOTH ingress AND egress parameterized. (D) STRATA + BOUNDARY-INTEGRITY — no downward dependency, no host-type leak, no concern ' +
  'owned twice, no coupling to the seam INTERIOR (vs its wire). (E) SURFACE-SPRAWL + PHANTOMS — an admitted package whose .api or the universal ' +
  'rails expose capability the fence re-derives by hand, flat code below the operator depth the packages reach, a phantom member (cited but ' +
  'unverifiable — delete it), a thin wrapper: collapse to package depth + verify via `assay api`. (F) CAPABILITY-COMPLETENESS + FULL-SEED-DEPTH + ' +
  'ILLUSION — counterfactually attack the seed roster for FULL-ROSTER completeness independently of how collapsed it looks: does the EN/EPD ' +
  'standard / the package / a consumer admit a grade, column, lifecycle module, or attribute this roster still OMITS? Name it with a cite, RESEARCH ' +
  'the real number via the ToolSearch-loaded Exa/Tavily/Context7 rails, and EXTEND the owner IN PLACE to FULL depth; conversely REJECT any padded ' +
  'grade, invented number, or parallel surface. ALSO a FULL COLD ADVERSARIAL RE-REVIEW of every conformance dimension with fresh hostile eyes. The ' +
  'fence must end objectively denser, MORE CAPABLE, deeper-seeded, more correct than the critique left it; if the strongest form is genuinely ' +
  'present, prove it by finding nothing. Report what you extended in `extended`. Return the fix-log + residual_high — each a {files: [every ' +
  'repo-relative path the cross-file fix spans, anywhere in the project], claim} object for a CROSS-FILE item you cannot fix from one file (NO ' +
  'severity; the project-wide reconcile fixes all).'].join('\n')

const reconcileFix = (cl) => [DOCTRINE, '', 'TASK: PROJECT-WIDE RECONCILE — fix EVERY one of these cross-FILE residuals the rebuild/critique/' +
  'redteam passes surfaced. There is NO severity — treat EVERY residual as must-address; NO leftovers, NO deferral, NO scope cap. Your blast radius ' +
  'is the ENTIRE project: you MAY read and fix ANY file ANYWHERE (libs/csharp, libs/python, libs/typescript, the central manifests — anything a ' +
  'cross-file residual spans), not only the Materials folder. Read EVERY listed file. For each: if it is a real cross-file defect, FIX it in place ' +
  'to the strongest clean/modern form the doctrine admits (unify the shared type/seam/rail, repair the strata/boundary issue, extend the shared ' +
  'owner in place to close a capability gap that spans files, finish a seed-depth or wire alignment spanning files, make a doc truthful), ALIGNING ' +
  'to the settled Component/Element/Material paradigm + the SEEDLAW + both .api tiers and PRESERVING the seam contract — a token patch that leaves ' +
  'the seam misaligned is NOT a fix; if a residual is FACTUALLY INCORRECT or not a real defect, leave it and say why in the summary — never silently ' +
  'skip a real one. If your fix surfaces a NEW cross-file need, report it in residual_high {files, claim}. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')

const reconcileVerify = (cl, fixFiles) => [LAW, '', BOUNDARIES, '', ADVERSARIAL, '', 'TASK: ADVERSARIAL VERIFY, one verdict per claim. Re-read the ' +
  'named files from disk and CONFIRM the fix is ACTUALLY made AND complete + high-quality + clean/modern + paradigm-conformant + seam-preserving, ' +
  'not a token/naive patch. ATTACK it: shallow, partial, a rename that left a sibling stale, a seam still misaligned, a seed row still missing or ' +
  'invented? Classify each claim: "fixed" (real defect, now genuinely + completely resolved), "invalid" (the claim is PROVABLY factually wrong / ' +
  'not a real defect — cite why), or "open" (real defect still NOT resolved, or fixed naively/incompletely — redo). Default to "open" on ANY doubt; ' +
  'mark "invalid" ONLY when you can show the claim is wrong, and it is SURFACED (never silently dropped). Claims:\n' + JSON.stringify(cl, null, 1) + ' ' +
  '\nFiles the fixer touched: ' + JSON.stringify(fixFiles)].join('\n')

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

const STAGES = { author: authorPrompt, crit: critiquePrompt, redteam: redteamPrompt }
const runStage = async (kind, effort, phaseTag) => (await pool(FILES.map((f) => ({ page: f })), CAP, (w) => agent(STAGES[kind](w.page), { label: kind + ':' + baseOf(w.page), phase: phaseTag, schema: FIXLOG_SCHEMA, effort, stallMs: STALL }))).filter(Boolean)
const residualsOf = (rows) => rows.flatMap((r) => (r.residual_high || []).map((x) => norm(x, r.file || FILES[0])))

// --- [COMPOSITION] -----------------------------------------------------------------------

log('properties-seed-depth: 8-phase ultra-harsh rebuild of ' + FILES.length + ' Properties pages (full seed depth + two-tier .api) -> ' +
  'rebuild -> reconcile-A -> critique-1 -> redteam-1 -> reconcile-B -> critique-2 -> redteam-2 -> reconcile-C; CAP=' + CAP + ' MAX_ROUNDS=' + MAX_ROUNDS)

// --- [REBUILD]
phase('Rebuild')
const rebuilt = await runStage('author', 'max', 'Rebuild')

// --- [RECONCILE_A]
phase('Reconcile-A')
const recA = await reconcile('Reconcile-A', residualsOf(rebuilt))

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
  workflow: 'properties-seed-depth', files: FILES,
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
