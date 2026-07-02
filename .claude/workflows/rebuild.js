export const meta = {
  name: 'rebuild',
  whenToUse: 'The standing rebuild engine for any libs/ planning corpus: pass targets (file / sub-folder / package root, any number) and optionally a campaign brief; it plans, discovers, hostile-implements, critiques, red-teams, and reconciles at the owning-language doctrine bar.',
  description: 'Durable language-agnostic rebuild engine over libs/{csharp,python,typescript} planning corpora. args = a target path, an array, or {targets, brief}; empty = no-op; language derives from the target root and selects the doctrine (cs/py/ts), both .api tiers, casing, and the member-verification rail. Plan (1 sonnet agent) expands targets to pages and, when a brief is given, classifies kind (new/rebuild/improve) + deletions + absorb pairs FROM THE BRIEF against real disk state. Discover (1 agent per 4 pages, opus) deep-reads pages + folder + BOTH .api tiers + the language doctrine and emits per-page reading maps. Implement (1 agent per 2 pages, fable, kind-aware) authors new pages ground-up / hostile-rebuilds / cold-improves with absorb-then-delete mechanics. Critique (1 per 3 pages, fable xhigh, mechanical checklists) then Redteam (1 per 3 pages, fable max, six lenses + cold re-review) fix in place, redteam paired after its critique batch. Reconcile: union-find residual clusters packed into at most 6 buckets -> parallel opus fixers -> exactly ONE terminal fable verify agent (skipped when no fixer changed a file), critique-grade repair-in-place, unreachable claims returned as hard_residual for resolve-residuals — NO re-entry loop.',
  phases: [
    { title: 'Plan', detail: 'one thin agent expands the targets into the page list; with a brief, classifies kind + deletePages + absorb pairs from the brief against real disk state', model: 'sonnet' },
    { title: 'Discover', detail: '1 agent per 4 pages: deep-read each page + the folder at large + BOTH .api tiers + the language doctrine; emit per-page apiUsed / apiUnderutilized / context / stacking / weak reading maps', model: 'opus' },
    { title: 'Implement', detail: '1 agent per 2 pages, kind-aware: ground-up author / hostile rebuild / cold improve, absorb-then-delete, write-fully; plus one brief-deletion executor when the brief drops pages outright' },
    { title: 'Critique', detail: '1 agent per 3 pages: mechanical line-by-line checklists (COLLAPSE_SCAN / OWNER_CHOOSER / KNOB_TEST / ASPECTS / RAILS / language-modernity / CAPABILITY-COMPLETENESS + ILLUSION), fix in place' },
    { title: 'Redteam', detail: '1 agent per 3 pages, paired after its critique batch: six adversarial lenses + a full cold re-review, fix in place' },
    { title: 'Reconcile', detail: 'union-find residual clusters -> at most 6 buckets -> parallel opus fixers -> ONE terminal verify agent (guarded: skipped when no fixer changed a file); open claims return as hard_residual, no re-entry loop', model: 'opus' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 10
const STAGGER_MS = 1500
const STALL = 300000
const DISCOVER_BATCH = 4
const IMPL_BATCH = 2
const REVIEW_BATCH = 3
const RECON_CAP = 6 // reconcile is ONE encompassing pass: residuals pack into <=6 buckets -> <=6 fixers -> ONE terminal verify, never a re-entry loop

// --- [INPUTS] ----------------------------------------------------------------------------

const normTarget = (t) => String(t).trim().replace(/\/+$/, '').replace(/^\/+/, '')
const rawTargets = Array.isArray(args) ? args
  : (args && typeof args === 'object' && Array.isArray(args.targets)) ? args.targets
  : (args && typeof args === 'object' && typeof args.targets === 'string') ? [args.targets]
  : (typeof args === 'string' && args.trim()) ? [args]
  : []
const BRIEF = (args && typeof args === 'object' && !Array.isArray(args) && typeof args.brief === 'string' && args.brief.trim()) ? args.brief.trim() : ''
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))]
const langOf = (t) => t.indexOf('libs/csharp') === 0 ? 'cs' : t.indexOf('libs/python') === 0 ? 'py' : t.indexOf('libs/typescript') === 0 ? 'ts' : null
const LANG_KEYS = [...new Set(TARGETS.map(langOf))]
const LANG_KEY = (LANG_KEYS.length === 1 && LANG_KEYS[0]) ? LANG_KEYS[0] : null

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const UNDERUTIL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['catalog', 'capability'], properties: { catalog: { type: 'string' }, capability: { type: 'string' } } } }
const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['packages', 'pages'], properties: {
  packages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['name', 'root', 'planning', 'api'], properties: { name: { type: 'string' }, root: { type: 'string' }, planning: { type: 'string' }, api: { type: 'string' }, note: { type: 'string' } } } },
  pages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'kind'], properties: { page: { type: 'string' }, kind: { type: 'string', enum: ['new', 'rebuild', 'improve'] }, absorb: { type: 'string' } } } },
  deletePages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'capturedIn'], properties: { page: { type: 'string' }, capturedIn: { type: 'string' } } } } } }
// WORK item — the per-page reading map Discover hands every downstream stage.
const WORK = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'kind', 'apiUsed', 'apiUnderutilized', 'contextNote', 'stackingGuidance', 'weak'], properties: {
  page: { type: 'string' }, kind: { type: 'string', enum: ['new', 'rebuild', 'improve'] }, absorb: { type: 'string' }, apiUsed: { type: 'array', items: { type: 'string' } },
  apiUnderutilized: UNDERUTIL, contextNote: { type: 'string' }, stackingGuidance: { type: 'string' }, weak: { type: 'boolean' } } } }
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['worklist'], properties: { worklist: WORK, summary: { type: 'string' } } }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['authored', 'rebuilt', 'refined', 'clean'] }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const REVIEW_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const DELETE_SCHEMA = { type: 'object', additionalProperties: false, required: ['deleted', 'summary'], properties: { deleted: { type: 'array', items: { type: 'string' } }, kept: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'reason'], properties: { page: { type: 'string' }, reason: { type: 'string' } } } }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const RESIDUAL_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: { files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['fixed', 'clean'] }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, repaired: { type: 'array', items: { type: 'string' } }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LANG = {
  cs: {
    key: 'cs', name: 'C#', root: 'libs/csharp', stack: 'docs/stacks/csharp', casing: 'PascalCase',
    corpus: 'libs/csharp planning corpus (markdown specs of intended C# package designs)',
    strata: 'CLAUDE.md manifest + WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly ' +
      'upward; a host-neutral owner only where a non-Rhino runtime consumes the contract).',
    stackLaw: 'MANDATORY STANDARDS — docs/stacks/csharp/ is the FLOOR, not the ceiling: every fence MUST meet docs/stacks/csharp/ (README, ' +
      'language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) AND the specialized ' +
      'docs/stacks/csharp/domain/ shard(s) relevant to the page concern, then PUSH PAST it to the objectively strongest form the doctrine admits ' +
      '— the tools/cs-analyzer compiled-doctrine gate enforces it (a true positive is architecture pressure, fix the shape; a false positive is ' +
      'rule pressure, never a suppression). Cite only host/NuGet members confirmed via `uv run python -m tools.assay api`; back bridge claims ' +
      'with EvidenceCertificate + reviewed ReferenceEvidence.',
    homing: 'C# PLANNING-HOMING: under libs/csharp/Rasm the active planning effort is Rasm/Geometry — its design pages live at ' +
      'libs/csharp/Rasm/Geometry/.planning/** while its governing ARCHITECTURE.md/IDEAS.md/TASKLOG.md/README.md and .api/ catalogs live at the ' +
      'libs/csharp/Rasm ROOT (one level UP from Geometry/); home any Rasm or Rasm/Geometry target there ({name: "Geometry", root: ' +
      '"libs/csharp/Rasm", planning: "libs/csharp/Rasm/Geometry/.planning", api: "libs/csharp/Rasm/.api"}) and NEVER touch the mature siblings ' +
      'Analysis/Domain/Vectors.',
    apiTiers: 'MINE BOTH capability tiers: the SHARED substrate catalogs `libs/csharp/.api/*.md` (Thinktecture generated owners, LanguageExt ' +
      'rails/effects/schedules/immutable collections, QuikGraph, Mapperly and siblings) AND the folder catalogs `<package>/.api/*.md` ' +
      '(Rasm/Geometry pages read `libs/csharp/Rasm/.api/`); ALWAYS layer the universal Thinktecture/LanguageExt rails onto the domain packages, ' +
      'never only the folder set.',
    readLaw: 'docs/stacks/csharp/ in FULL — README plus language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, ' +
      'system-apis — PLUS the docs/stacks/csharp/domain/ shard(s) relevant to each page concern (READ the shard; conformance to it is a hard gate)',
    verify: '`uv run python -m tools.assay api`',
    vocab: '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject]`/`Fold`/the rails)',
    slur: 'naive, surface-level code dressed in the right vocabulary',
    illusion: 'a `.api`/host member cited but never verified (a phantom)',
    ultra: [
      'OPERATIVE DOCTRINE — the 16 named laws of docs/stacks/csharp/README.md, held as fact: [FLOW] EXPRESSION_SPINE (domain logic is ' +
        'expression-shaped; dependent steps `Bind` monadically, independent ones accumulate applicatively; the carrier, never a flag, selects ' +
        'the algebra; statements survive only in measured `ref struct`/span kernels that name the exemption) + BOUNDARY_ADMISSION (raw admitted ' +
        'EXACTLY ONCE into an evidence-carrying owner; interior never re-validates or sees null/sentinel/provider shape). [SHAPE] SHAPE_BUDGET ' +
        '(one concept owns ONE type; variants are cases in one closed family) + DEEP_SURFACES + MODAL_ARITY (one entrypoint owns every modality, ' +
        'discriminating on input shape) + ANTICIPATORY_COLLAPSE (shape the owner for the family it will absorb). [DERIVATION] POLICY_VALUES + ' +
        'DERIVED_LOGIC + DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ' +
        'ROOT_REBUILD (weave new capability into the owner as if always present; no shims/aliases/[Obsolete]/migration layers) + ' +
        'ONE_HOP_RESOLUTION + COMPOSED_IMPLEMENTATION.',
      'ULTRA-ADVANCED COLLAPSE MANDATE: COLLAPSE >=3 parallel types / sibling factory methods / repeated switch arms / single-call private ' +
        'helpers into ONE polymorphic owner IN THE SAME FILE via `[Union]` / `[Union<T1,...>]` ad-hoc / `[SmartEnum<TKey>]` / `[SmartEnum]` ' +
        'keyless / `[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case families / `Fold` algebra / frozen data tables — never ' +
        'extract a new file to reduce LOC, never delete capability.',
      'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows raw -> admit ONCE (generated factory + validation partial admits/rejects; one ' +
        'rail bridge lifts the generated outcome into `Fin<T>` / `Validation<Error,T>`; `Option<T>` carries absence; exceptions convert at the ' +
        'owning boundary only) -> canonical owner -> unified rail -> projection -> egress. Interior code never re-validates, never sees ' +
        '`null`-as-failure, sentinels, or provider shapes; parameterize BOTH ingress AND egress so the same owner sources and sinks across many ' +
        'consumers without interior edits.',
      'STACK CAPABILITY: C# has NO central runtime `.api/` tier beyond the shared substrate catalogs — the universals are Thinktecture ' +
        '(generated domain shape) / LanguageExt (rails, effects, schedules, immutable collections) plus the full docs/stacks/csharp doctrine, ' +
        'with MathNet / CSparse owning numeric algorithms. There is NO fixed package count: compose EVERY relevant host API + admitted NuGet ' +
        'package + catalog member into single dense owners woven as ONE rail (source-generated owners, `Fold` algebra, data tables), ALWAYS ' +
        'layering the universal Thinktecture/LanguageExt rails onto the domain packages, NOT flat one-shot per-API uses. Use the DEEPEST ' +
        'operator/combinator/generated surface each package itself reaches (LIBRARY_DEPTH); reject surface-level subsets, BCL-first reflexes, ' +
        'and thin rename wrappers; verify novel members with `uv run python -m tools.assay api`.',
      'PRESERVE all capability (densify, never delete functionality). Where a fence is already dense, deepen; where it is flat/naive, rebuild ' +
        'ground-up. Never regress correctness or boundary/strata law.',
    ].join('\n'),
    mechanics: '',
    patlaw: [
      'C# PATTERN LAW: model the domain precisely — NEVER weak/unbounded/erased types where the language can express the domain; NEVER exception ' +
        'control flow in domain logic (use the LanguageExt typed rails / ROP and the route recovery patterns); NEVER imperative branching where ' +
        'a bounded vocabulary, frozen table, generated `Switch`, match, or `Fold` owns the variation; NEVER mutable accumulation for domain ' +
        'transforms (use immutable folds, projections, collection combinators). Total generated `Switch` with compile-time exhaustiveness (a new ' +
        'case breaks every dispatch site — NEVER a runtime-silent `_` arm). Typed algorithm receipts (NEVER a generic `IReceipt`/ledger) when ' +
        'fields carry route/status/sampling/solver/spectral/mesh/extraction/benchmark/host evidence. The fault type is a CLOSED `[Union]` family ' +
        'deriving from `Expected` (a bare exception or a generic untyped `Error` for a multi-cause domain is a defect).',
      'Latest stable C# 14 on `net10.0` to the metal (`Nullable enable`, NRT enforced): primary constructors, collection expressions with ' +
        'spread, `params` collections (incl. `params ReadOnlySpan<T>`), list/slice/relational/logical pattern matching, switch expressions, ' +
        '`required` members, `file`-scoped types, `field` accessors, extension blocks (`extension(Receiver)`) and extension operators, generic ' +
        'math / static abstract+virtual interface members, `with` expressions, `nameof` with unbound generics, `System.Threading.Lock`, raw ' +
        'string + `u8` literals where they fit. Treat analyzer diagnostics as architecture pressure. Apply the docs/stacks/csharp ' +
        'file-organization and section-order law (`[Union]`/`[SmartEnum]`/`[ValueObject]` and generated case families stay inside the declaring ' +
        'owner block; canonical section order TYPES -> CONSTANTS -> MODELS -> ERRORS -> SERVICES -> OPERATIONS -> COMPOSITION -> EXPORTS).',
      'Keep conventions IDENTICAL across every package; place each package on its canonical stratum and depend strictly upward; ' +
        'geometry/mesh/IFC meet at the wire with one owner per runtime; never leak a host type into a host-neutral owner. SEMANTIC_NAMING: one ' +
        'canonical bounded-context term per concept (one word default, three the ceiling); arity/filter/provider/modality live in request shape, ' +
        'case, or policy row, never parallel `Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` names; ONE_HOP_RESOLUTION (no alias chains, forwarding ' +
        'helpers, or util shells).',
    ].join('\n'),
    boundaries: 'BOUNDARY LAW: keep every package owner strictly in its lane and on its stratum; geometry/mesh/IFC meet at the wire with one ' +
      'owner per runtime; internal code uses canonical names and shapes with mapping only at the edge; do not trample a sibling owner while ' +
      'densifying; never introduce a downward dependency or leak a host type into a host-neutral owner. Cross-language wire seams are recorded ' +
      'as ALIGNED seams at the wire contract — a page pass fixes the local side and records the wire; a counterpart mirror (same-language ' +
      'sibling or cross-language decode side) is repaired only by the reconcile stage under the brief consumer-ripple rules, never ad hoc ' +
      'from a page pass.',
    docBloat: 'XML-doc',
    collapseInto: 'ONE `[Union]` / `[SmartEnum<TKey>]` / `[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case family IN THE SAME FILE',
    gapPkg: 'LIBRARY_DEPTH: e.g. an IFC schema gives a zone its quantities, space boundaries, and properties the page never reads — stacking ' +
      'that full surface IS new functionality woven into the owner, not a denser spelling of the same call',
    gapDomain: 'a BIM zone owns its boundary/area/volume, per-kind attributes — a fire compartment a rating, a thermal zone a setpoint, a load ' +
      'group its combinations, an MEP system its medium/flow/pressure — adjacency/nesting topology, and coverage/aggregation/spatial-query ' +
      'operations, not a flat member-id set alone; a profile owns section properties, grade, fabrication + code-check inputs, not width/height; ' +
      'a durable store owns its constraints, indexes, partitions, RLS, migration, and lifecycle, not naive columns',
    ownerGrammar: 'a CASE in the existing closed family, a ROW or richer data on the existing smart-enum, a FIELD or a composed ' +
      '`[ValueObject]`/`[ComplexValueObject]` on the existing record, an OPERATION on the existing surface, or a POLICY_VALUE on the existing vocabulary',
    ownerChooser: '(2) OWNER_CHOOSER — for EVERY shape re-derive the owner from the 5 discriminants (admission, identity regime, variant arity, ' +
      'payload timing, openness) and select the OWNER_CHOOSER row, most-specific wins: invariant-bearing scalar -> `[ValueObject<TKey>]`; ' +
      'N-field one-concept product, no discriminator -> `[ComplexValueObject]`; bounded vocabulary with wire-keyed identity -> ' +
      '`[SmartEnum<TKey>]`; bounded vocabulary with process-local behavior -> `[SmartEnum]` keyless; closed alternatives with per-occurrence ' +
      'payload -> `[Union]`; one value over 2-5 unrelated types -> `[Union<T1,...>]` ad-hoc; interior product, no invariant/admission -> ' +
      '`record`/`readonly record struct`; combinable capability set -> a frozen set; cross-product or external policy key -> a frozen table; ' +
      'foreign wire enum / ABI bits / kernel ordinal -> a language `enum` AT THE SEAM ONLY (re-closed at conversion). Kill every parallel DTO, ' +
      'one-field wrapper, field-rename shape, nullable payload bag, enum-dictionary pair, protocol shadow, nullable-as-failure, and ' +
      'struct-`default` ghost.',
    knob: '(3) KNOB_TEST — removal: delete each parameter; if the value reconstructs what it carried, it was a knob -> collapse a ' +
      '`bool`/`mode`/`strict`/`batch` flag into a policy value or input-shape discriminant; a nullable flag tail (`T? a = null, ..., bool x = ' +
      'false`) -> one `Option<ContextRecord>`; the single optional form is `Option<T> x = default` consumed via `IfNone(canonical)`; move every ' +
      '`timeout`/`retry`/`deadline`/`CancellationToken` OFF the signature onto the carrier or a composition-time effect aspect.',
    aspects: '(4) ASPECTS — the two-weave taxonomy: definition-time concerns (admission, identity, dispatch, serialization, grammar, logging) ' +
      'attach via attribute-directed SOURCE GENERATION in the fixed generator-owned order; composition-time concerns attach as effect ' +
      'transformers in author order — retry as `Schedule`-driven `IO<T>.Retry(Schedule)`/`Prelude.retry`, recovery as named catch combinators ' +
      '(`@catch`/`catchOf`/`CatchM` composed via `|`), resource lifetime as `Bracket`/`BracketIO`/`Finally`; the two weaves meet at EXACTLY ONE ' +
      'seam, the admission rail bridge. 2-4 co-occurring wrappers collapse into one aspect; an aspect NEVER raises into domain flow; ' +
      'deterministic stacking order verified. Inline-repeated concerns and sibling helper methods are defects.',
    rails: '(5) RAILS — RAIL_CHOOSER, the narrowest carrier chosen ONCE at admission: `Option<T>` absence, `Fin<T>` synchronous fallibility, ' +
      '`Validation<Error,T>` independent accumulated faults, `Eff<RT,T>` runtime capability, `IO<T>` deferred boundary work, `Schedule` retry ' +
      'policy, `Seq<T>`/`Arr<T>`/`HashMap<K,V>` immutable traversal/lookup; the fault type is a CLOSED `[Union]` family deriving from `Expected` ' +
      '(recovery identity via `Is`/`HasCode`/`IsType<E>`, never `==`); accumulate-vs-abort disposition correct (`Apply`/`&`/`.Traverse` for ' +
      'independents, `Bind`/`.TraverseM`/query expressions for dependents); total generated `Switch` with compile-time exhaustiveness (NO `_` ' +
      'arm hiding a case); `.Fold`/`.Traverse`/`.Choose` traversal with the mandatory `.As()` re-anchor; NO exception control flow in domain ' +
      'logic, NO mutable accumulation.',
    modernity: '(6) STRATA/MEMBERS/MODERN — strata correctness (depend strictly upward; NO downward dependency, NO host-type leak into a ' +
      'host-neutral owner; geometry/mesh/IFC meet at the wire with one owner per runtime); cite ONLY host/NuGet members confirmed in the package ' +
      '`.api/` catalog (no phantom member; verify novel members via `uv run python -m tools.assay api`); latest modern C# 14 on net10; FULL ' +
      'docs/stacks/csharp + the relevant domain/ shard conformance (READ the shard); BOTH the package `.api/` catalogs AND the universal ' +
      'Thinktecture/LanguageExt rails maximized, not a surface-level subset; the tools/cs-analyzer compiled-doctrine gate clean.',
    deepPkgs: 'LanguageExt/Thinktecture/MathNet/CSparse',
    exhaust: 'total generated `Switch`, no silent `_` arm',
    modern: 'Latest modern C# 14 on net10',
  },
  py: {
    key: 'py', name: 'Python', root: 'libs/python', stack: 'docs/stacks/python', casing: 'snake_case',
    corpus: 'libs/python planning corpus (markdown specs of intended Python module designs)',
    strata: 'CLAUDE.md manifest law governs.',
    stackLaw: 'DENSITY BAR: docs/stacks/python/ — author Python as dense, polymorphic, and rich as that bar; docs/stacks/csharp/ is the ' +
      'density/ambition FLOOR (match its richness, never import C#-shaped idioms). Cite ONLY members confirmed in the .api catalogs, mining ' +
      'BOTH tiers fully.',
    homing: '',
    apiTiers: 'MINE BOTH tiers fully: the SHARED/universal branch catalogs `libs/python/.api/*.md` (anyio, expression, msgspec, pydantic, ' +
      'pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*) AND the folder-specific `<package>/.api/*.md`; maximize ' +
      'the shared/universal catalogs wherever relevant, never only the folder set, ALWAYS layering the shared/universal rails ON TOP OF the ' +
      'folder-specific domain packages.',
    readLaw: 'docs/stacks/python/ in FULL, atlas order: STEP 0 — enumerate docs/stacks/python/ with a real ls/find to obtain the COMPLETE root ' +
      'inventory (the sub-folders domain/ and numerics/ are OUT of this read), THEN read EVERY root page top-to-bottom starting at the README ' +
      'and proceeding in its [01]-[ATLAS] routing order (language, shapes, iteration, surfaces-and-dispatch, rails-and-effects, concurrency, ' +
      'boundaries, algorithms, system-apis, runtime) — never a partial, skim, grep-jump, or section-sample; a root page present on disk but ' +
      'absent from this list is STILL mandatory law',
    verify: '`uv run --frozen python -m tools.assay api resolve <pkg>` (a gated/uninstalled package falls back to its catalog/official surface)',
    vocab: '(`@tagged_union`/`frozendict`/`Result`/`Option`/the rails)',
    slur: 'naive, surface-level, old-style Python dressed in the right vocabulary',
    illusion: 'a `.api` member cited but never verified (a phantom)',
    ultra: [
      'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows `Raw -> Payload -> Canonical owner -> Rail -> Projection -> Egress`. Raw ' +
        'material is admitted EXACTLY ONCE into an evidence-carrying owner (Pydantic/`TypedDict` payload at ingress); interior code never ' +
        're-validates, never sees `None`-as-failure, sentinels, or provider shapes; egress projects outward (`msgspec.Struct` wire) from the ' +
        'canonical owner. Parameterize BOTH ingress AND egress so the same owner sources and sinks across many providers/apps without touching ' +
        'its interior.',
      'SHAPE LAW: one concept owns exactly ONE type (SHAPE_BUDGET) — variants are cases in one closed family, never sibling types; one rich ' +
        'polymorphic surface over many shallow (DEEP_SURFACES); the owner is shaped for the family it will ABSORB (ANTICIPATORY_COLLAPSE) so the ' +
        'next case/dimension/modality lands as ONE declaration with every consumer untouched or broken loudly at type-check. Choose each owner ' +
        'by the OWNER_CHOOSER discriminants — admission (trusted/untrusted), identity regime (value/tag/key/reference), variant arity ' +
        '(one/closed-family/open), payload timing (def-time/runtime), openness (closed/semi/open) -> the right owner among `TypedDict`, ' +
        'Pydantic, `msgspec.Struct`, frozen dataclass, rich class, `StrEnum`/`Literal`, `sentinel`, `Option`/`Result`, ' +
        '`frozendict`/`Map`/`tuple`, `Protocol`. A misplaced shape traces to one mis-answered discriminant.',
      'ASPECT-FIRST (DEFINITION_TIME_ASPECTS): every CROSS-CUTTING capability — retry, telemetry/spans, validation, contracts, memoization, ' +
        'registration, receipts, fault rails — is a SIGNATURE- and RAIL-PRESERVING decorator (inline `**P` + `functools.wraps`) that ' +
        'materializes policy, STACKS in deterministic order (bottom-up at definition, top-down at call), and NEVER raises into domain flow (a ' +
        'failing aspect returns the rail `Error`). Two-to-four wrappers that always co-occur collapse into ONE parameterized aspect factory. ' +
        'Code reads as STACKED DECORATORS over a thin pure core, never inline-repeated concerns or sibling helper functions; the domain ' +
        'transform itself stays a pure function/fold.',
      'DERIVATION + ARITY: cases sharing generative structure are DERIVED — one primary `frozendict` correspondence declared, every secondary ' +
        'map derived from it (DERIVED_LOGIC), or a fold/comprehension — never enumerated arms. Configuration enters as ONE behavior-carrying ' +
        'value (vocabulary member, tagged variant, frozen policy table), never flag sets the body re-derives (POLICY_VALUES). ONE entrypoint ' +
        'owns every modality (singular/plural/batch/stream), discriminating on the INPUT SHAPE (`T | Iterable[T]` normalized once at the head), ' +
        'never a name suffix or a `mode`/`batch` knob (MODAL_ARITY); a `timeout`/`retry`/`deadline` is an aspect or an `anyio` scope, never a ' +
        'signature param (KNOB_TEST).',
      'RAILS (rails-and-effects): the narrowest carrier that states the outcome, chosen ONCE at admission — `Option[T]` non-failing absence, ' +
        '`Result[T, E]` typed fallibility, `effect.result` do-notation for sequential `bind`, `Block`/`Map` immutable traversal, an `anyio` ' +
        'task group as the failure boundary (NEVER `asyncio.gather`), `stamina.retry` as the decorator (never a sleep-loop). The fault type `E` ' +
        'is a CLOSED vocabulary — `Literal` set, `StrEnum`, or `@tagged_union` family — NEVER a bare `str` for a multi-cause domain. ' +
        'Accumulate-vs-abort is a correctness decision fixed at the boundary: `map2`/accumulating-fold for independent operands, `bind` ' +
        'short-circuit for dependent steps. Cancellation is not failure; resource cleanup is `AsyncExitStack` + a shielded scope.',
      'STACK .api CAPABILITY (load-bearing): FIRST inventory the COMPLETE catalog set available to this page — BOTH tiers — then mine them for ' +
        'the full ADVANCED surface of each package (combinators, hooks, native pipelines, discriminators, async mirrors) and how packages ' +
        'STACK. List BOTH `.api/` tier DIRECTORIES in full and DIFF that complete inventory against what the page already cites: every admitted ' +
        'catalog whose domain the page admits but does NOT yet use is an ADOPTION TARGET — adopt it to depth here, or when it belongs on a ' +
        'sibling page surface it as a residual; a relevant admitted catalog left unadopted is a DEFECT. There is NO fixed library count: ' +
        'compose EVERY relevant admitted library into single dense operations woven as ONE rail. Use the DEEPEST primitive each package itself ' +
        'reaches for (LIBRARY_DEPTH); reject surface-level single-feature subsets and any thin rename wrapper.',
      'PRESERVE all capability (densify, never delete functionality). Where a page is already dense, refine; where it is flat/naive, rebuild ' +
        'ground-up. Never regress correctness or boundary law.',
    ].join('\n'),
    mechanics: [
      'MECHANICAL EXECUTABILITY — a design-page fence is a SIGNATURE-AND-IMPLEMENTATION CONTRACT, never a sketch: every fence MUST parse under ' +
        'the active py3.15 surface AND type-check against the REAL cross-page canonical owners it imports, because the corpus is ONE body ' +
        '(CORPUS_LAW three-layer inheritance) and a fence reading a field/case/attribute a sibling owner does not declare is a runtime DEFECT, ' +
        'not a design liberty. Mentally COMPILE and TYPE-CHECK each fence before accepting it. Find each class below BY NAME and FIX it in ' +
        'place by growing the EXISTING owner (a case/field/operation/row), never a new file:',
      'FENCE-PARSES (language.md CLOSED_MATCH_SITE) — every `match`/structural pattern, `for`-target, comprehension, and t-string parses: an ' +
        'OR-pattern whose alternatives bind DIFFERENT names, an invalid iterable-unpacking or starred target, or a malformed pattern is a ' +
        'NON-COMPILING fence and an automatic rebuild target.',
      'MODEL-COHERENCE (CORPUS_LAW) — every attribute, field, case tag, method, and imported symbol a fence reads off a canonical owner ' +
        'declared on ANOTHER page (or earlier in this one) MUST exist on the real declaration of that owner: verify each cross-owner read ' +
        'against the sibling owner before writing it, reconcile to the ONE canonical name, never invent a field the owner does not carry, and ' +
        'surface an un-reconcilable cross-page name as a residual.',
      'TOTAL-DISPATCH (shapes.md families) — `assert_never(unreachable)` is an exhaustiveness WITNESS, valid ONLY when every member of the FULL ' +
        'closed family is handled before it: enumerate the complete case set and prove NO valid case routes to `assert_never`; a parallel ' +
        'dispatch map keyed by a closed family must be TOTAL over it. A partial `match`/map dressed as total is a DEFECT.',
      'SINGLE-FACT EVIDENCE (rails-and-effects.md STATE_RECEIPTS; boundaries.md BYTE_IDENTITY) — the bytes, the content key, and the receipt ' +
        'evidence derive from ONE computed fact stored once on the stepped owner: the producer computes the fact, the receipt/contribute path ' +
        'READS the stored fact, never re-renders. A path that recomputes a render/placement/native-mutation a second time to mint receipt ' +
        'evidence is a DOUBLE-RENDER defect.',
      'LOOP-OFFLOAD (concurrency.md OFFLOAD_LANE) — synchronous CPU-bound or GIL-hostile provider work NEVER runs on the event loop, NOR as an ' +
        'argument expression evaluated before the offload call: it crosses on exactly one arm — `anyio.to_thread.run_sync` for a GIL-releasing ' +
        'native call or blocking I/O, `to_interpreter.run_sync` for pure-Python isolate-safe CPU work, `to_process.run_sync` for a GIL-hostile ' +
        'or not-isolate-safe native call — each bounded by an explicit `CapacityLimiter`. A heavy synchronous call inside an async body is an ' +
        'event-loop-starvation DEFECT.',
      'HANDLE-LIFETIME (boundaries.md CAPSULE_OWNER) — every native/FFI handle a provider opens closes DETERMINISTICALLY through an ' +
        '`AsyncExitStack.enter_async_context`, a `with` bracket, or a capsule registering release via `weakref.finalize` under a shielded ' +
        'teardown — never left for the GC to reap. An opened handle with no deterministic close is a LEAK defect; callers receive detached ' +
        'values or rails.',
      'BINARY-KERNEL (boundaries.md CAPSULE_OWNER) — a multi-megabyte binary mutated across N steps is ONE imperative measured kernel ' +
        'threading ONE owned handle mutated in place, NOT a functional fold that rebinds and recopies the whole buffer per step; the kernel ' +
        'lives inside the shielded resource bracket, returns the rail `Result`, and carries one `# Exemption:` line naming the platform-forced ' +
        'in-place-mutation seam.',
      'IDENTITY-REGIME (boundaries.md MEMO_KEY) — a content-addressed key indexes by CONTENT, so two structurally-distinct siblings carrying ' +
        'identical content collide and silently overwrite in a `Map[ContentKey, _]`. Where an index/diff must distinguish identical-content ' +
        'siblings, the key joins a STRUCTURAL discriminant to the content digest — a content-only key under a structural index is a CORRUPTION ' +
        'defect.',
      'TEMPLATE-SAFETY (language.md TEMPLATE_STRUCTURE_SITE) — structured-text and markup egress (SVG, XML, Typst, HTML, query strings) built ' +
        'from dynamic or untrusted input uses PEP 750 t-strings / `string.templatelib.Template` processors or a structured builder ' +
        '(`xml.etree.ElementTree`), NEVER f-string interpolation with a hand-rolled escape. An f-string splicing a value into markup is an ' +
        'INJECTION defect.',
      'STREAM-OVER-MATERIALIZE (iteration.md LAZY_COMBINATORS) — a large or unbounded extraction is a lazy `itertools`/generator pipeline or a ' +
        '`yield from` fusion typed `Iterator[T]`, never an eagerly allocated `tuple`/`Block` of the whole result held in RAM and materialized ' +
        'only at the persistence/egress edge.',
      'NO-EXCEPTION-HOTLOOP (rails-and-effects.md EXPRESSION_SPINE) — a per-element `try`/`except` driving control flow inside a fold over a ' +
        'large collection is BOTH a domain-logic violation AND a throughput defect: a total predicate or a non-raising `Option`-returning ' +
        'parse replaces the per-element raise, and the boundary `catch` trap stays at the boundary, never in the hot fold.',
      'DERIVED-NOT-PARALLEL + PER-MODE PAYLOADS (DERIVED_LOGIC) — a secondary map hand-synced to a primary is a DERIVATION defect: declare ONE ' +
        'primary correspondence and DERIVE every secondary by comprehension. A monolithic typed bag whose fields are irrelevant for most modes ' +
        'is a permissive-bag DEFECT even when fully typed: collapse it into a discriminated per-mode `@tagged_union` whose each case carries ' +
        'ONLY the fields of its own mode — WITHOUT splitting the owner into new files.',
    ].join('\n'),
    patlaw: [
      'PY-VERSION LAW: target Python 3.15 on the full modern band (3.11/3.12/3.13/3.14/3.15) — advanced patterns ONLY, zero legacy idioms, ' +
        'IDENTICAL conventions across every folder and package.',
      'NEVER write `from __future__ import annotations`. NEVER use legacy typing: use PEP 585 builtin generics (`list[T]`, `dict[K, V]`, ' +
        '`tuple[...]`, `set[T]`) NOT `typing.List/Dict/Tuple/Set`; PEP 604 unions (`X | None`, `A | B`) NOT `Optional`/`Union`; PEP 695 type ' +
        'parameters (`class C[T]:`, `def f[T](...)`, `type Alias[T] = ...`) NOT `TypeVar` + `Generic`. Use `Self`, `override`, ' +
        '`TypeIs`/`TypeGuard`, `assert_never`, `ReadOnly`, `TypedDict` + `NotRequired`/`Required`, `LiteralString`, `enum.StrEnum`/`IntEnum`, ' +
        'and `@dataclass(slots=True, frozen=True)` or `msgspec.Struct`/pydantic models where each best fits.',
      'PAYLOADS — NEWEST FORM: ingress payloads are static `TypedDict` contracts with `closed=True` or `extra_items=T` and per-key ' +
        '`Required[]`/`NotRequired[]`/`ReadOnly[T]`, admitted through a module-level `TypeAdapter`, with `Unpack[TypedDict]` at root keyword ' +
        'entrypoints; extension bands fold into `frozendict`/tuple evidence at materialization, and `msgspec.Struct(frozen=True)` owns ' +
        'wire/egress. NO `dict[str, Any]` bags, homogeneous `**kwargs`, or `Mapping[str, object]` payloads.',
      'FROZENDICT (py3.15 builtin): `from builtins import frozendict` is the owner for immutable map rows, dispatch/policy TABLES (one primary ' +
        '`frozendict[K, tuple[...]]`, secondary maps derived from it), payload `extra_items` extension bands, and immutable evidence — REJECT ' +
        '`MappingProxyType`, a module-level mutable `dict` used as a table, tuple-pair pseudo-maps, and mutate-then-freeze. Prefer total ' +
        '`match`/structural pattern matching over if-chains, walrus where it tightens, and `assert_never` on closed unions ONLY where it is ' +
        'genuinely unreachable over the FULL case set; PEP 750 t-strings are MANDATORY for all dynamic or untrusted structured-text and markup ' +
        'egress. Keep every choice CONSISTENT across folders so the corpus reads as one ultra-advanced codebase.',
    ].join('\n'),
    boundaries: 'BOUNDARY LAW: keep every package/folder owner strictly in its lane; internal code uses canonical names and shapes with mapping ' +
      'only at the edge; do not trample a sibling owner while densifying; respect the dependency direction of the workspace strata. ' +
      'Cross-language wire seams are recorded as ALIGNED seams at the wire contract (the C# boundary is law: Rasm.Bim owns IFC semantics) — ' +
      'a page pass fixes the local side and records the wire; a counterpart mirror (same-language sibling or cross-language decode side) is ' +
      'repaired only by the reconcile stage under the brief consumer-ripple rules, never ad hoc from a page pass.',
    docBloat: 'docstring',
    collapseInto: 'one closed `@tagged_union`/`Literal`/`StrEnum` family, a derived `frozendict` table, or a fold IN THE SAME FILE',
    gapPkg: 'BOTH tiers; stacking that full surface IS new functionality woven into the owner, not a denser spelling of the same call',
    gapDomain: 'a dimension owner owns the full ISO 129-1 linear/aligned/angular/radial/diameter/ordinate/chain/baseline + tolerance family, ' +
      'not a single linear case; a layer codec owns the full ISO 13567 + NCS discipline/major/minor/status structure, not a flat string',
    ownerGrammar: 'a CASE in the existing closed `@tagged_union`/`Literal`/`StrEnum` family, a ROW or richer data on the existing `frozendict` ' +
      'table, a FIELD on the existing `msgspec.Struct`/Pydantic model/frozen dataclass/`TypedDict`, an OPERATION on the existing surface, or a ' +
      'POLICY_VALUE on the existing vocabulary',
    ownerChooser: '(2) OWNER_CHOOSER — re-derive every shape from the 5 discriminants (admission, identity regime, variant arity, payload ' +
      'timing, openness) and replace any non-discriminant-correct owner; kill every parallel DTO, one-field wrapper, field-rename class, ' +
      'tag-only shape, and None-as-failure.',
    knob: '(3) KNOB_TEST — delete each parameter the value already encodes; move every timeout/retry/deadline out of the signature into an ' +
      'aspect/`anyio` scope.',
    aspects: '(4) ASPECTS — every cross-cutting concern is a signature+rail-preserving STACKED decorator that never raises into domain flow; ' +
      'co-occurring wrappers collapse into one factory.',
    rails: '(5) RAILS — narrowest carrier chosen once; CLOSED fault vocabulary (a bare `str` fault is a defect); accumulate-vs-abort correct; ' +
      'NO asyncio, hand-rolled retry, None-as-failure, or exception control flow in domain logic.',
    modernity: '(6) PAYLOADS/FROZENDICT/PEP — `closed=`/`extra_items=` `TypedDict` via module-level `TypeAdapter` + `Unpack`; `frozendict` ' +
      'builtin owns tables; PEP 585/604/695 only; total `match` + `assert_never`; plus the full MECHANICAL EXECUTABILITY sweep (fence-parses, ' +
      'model-coherence, total-dispatch, single-fact evidence, loop-offload, handle-lifetime, identity-regime, template-safety, ' +
      'stream-over-materialize, no-exception-hotloop, derived-not-parallel).',
    deepPkgs: 'the admitted both-tier catalogs (expression/msgspec/pydantic/anyio + the folder domain packages)',
    exhaust: 'total `match` + `assert_never` over the FULL case set',
    modern: 'py3.15-modern only',
  },
  ts: {
    key: 'ts', name: 'TypeScript', root: 'libs/typescript', stack: 'docs/stacks/typescript', casing: 'camelCase',
    corpus: 'libs/typescript planning corpus (markdown specs of intended TypeScript module designs)',
    strata: 'CLAUDE.md manifest law governs.',
    stackLaw: 'DENSITY BAR + STANDARD: docs/stacks/typescript/ (README, language, shapes, surfaces-and-dispatch, rails-and-effects, ' +
      'boundaries, system-apis) and the coding-ts standard. The historical TS corpus quality is POOR; treat this as a TRUE modernization to ' +
      'ultra-advanced TS, discarding naive idioms wholesale. Cite only real members of admitted packages, cross-checked against the published ' +
      'types in node_modules.',
    homing: '',
    apiTiers: 'MINE BOTH catalog tiers: the SHARED/universal `libs/typescript/.api/*.md` (effect, effect-platform, effect-opentelemetry, ' +
      'effect-atom, react, react-dom, clsx) AND the area-specific `<area>/.api/*.md`, cross-checked against the published types in ' +
      'node_modules; maximize the shared/universal Effect/Schema/React rails wherever relevant, never only the area set.',
    readLaw: 'docs/stacks/typescript/ in FULL (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, system-apis) ' +
      'plus the coding-ts standard',
    verify: 'the published types in node_modules (`uv run python -m tools.assay api` over node_modules declarations where a member is novel)',
    vocab: '(tagged unions, `Schema`, `Effect`/`Layer`, branded types)',
    slur: 'naive JavaScript-in-TypeScript dressed in the right vocabulary',
    illusion: '`any`/unsafe `as`/non-null `!` smuggled under a confident surface; a member cited but unverifiable against node_modules (a phantom)',
    ultra: [
      'ULTRA-ADVANCED REBUILD MANDATE: COLLAPSE >=3 parallel interfaces/types/classes modelling one concept into ONE polymorphic surface ' +
        '(tagged discriminated union + exhaustive match), never parallel names. AOP: cross-cutting concerns (retry, telemetry, validation, ' +
        'caching, receipts, fault rails) as Effect combinators/layers/decorators, not repeated inline. UNIFIED rails + UNIFIED pipelines + ' +
        'feature-arms-as-cases (never loose separate). Parameterize inputs AND outputs with generics at depth; no stringy/weak typing.',
      'STACK CAPABILITY: FIRST inventory the COMPLETE catalog set — BOTH tiers — then compose the admitted libraries into single dense rails. ' +
        'There is NO fixed library count: weave EVERY relevant library, ALWAYS layering the shared/universal Effect ecosystem ' +
        '(`Effect`/`Layer`/`Context`/`Schema`/`Stream`) end-to-end ON TOP OF the area-specific packages, NOT naive `Promise`/`try`-`catch` ' +
        'glue. Use the MOST powerful combinators; reject surface-level single-feature uses.',
      'PRESERVE all intended capability (densify, never delete functionality). Where a page is already strong, refine; where it is flat/naive, ' +
        'rebuild ground-up. Never regress correctness or boundary law.',
    ].join('\n'),
    mechanics: '',
    patlaw: [
      'TS PATTERN LAW (ultra-advanced ONLY; do not preserve the naive idioms of the existing code): ZERO `any`, zero implicit `any`, zero ' +
        'unsafe `as`, zero non-null `!`; model with branded/nominal types, exact discriminated unions with EXHAUSTIVE handling (`assertNever` ' +
        'on the default), `readonly`/`as const`, template-literal types, conditional/mapped types, and the `satisfies` operator. NO runtime ' +
        '`enum` — use `const` unions or `Schema`/Effect.',
      'Domain logic runs on typed-error rails — `Effect`/`Either`/`Option`, NEVER `throw` in domain code; boundaries validate through `Schema` ' +
        '(parse, never trust input). `import type`/`export type` are explicit; side-effect/value imports preserve runtime order. Per the ' +
        'docs/stacks/typescript file-organization overlay: `Effect.Service` owners are SERVICES, `Layer`/runtime wiring is COMPOSITION, ' +
        'runtime schemas/classes are MODELS, and catalog or registry rows stay after the owners they reference.',
      'Keep conventions IDENTICAL across every area so the corpus reads as one ultra-advanced codebase. One canonical semantic name per ' +
        'bounded concept; discriminate on input shape rather than proliferating `get`/`getMany`/`getById` names.',
    ].join('\n'),
    boundaries: 'BOUNDARY LAW: keep every area owner strictly in its lane; internal code uses canonical names and shapes with mapping (and ' +
      '`Schema` validation) only at the edge; do not trample a sibling owner while densifying; respect the dependency direction of the ' +
      'workspace. Cross-language wire seams are recorded as ALIGNED seams at the wire contract — fix the local side and record the wire, never ' +
      'edit the counterpart language from this run.',
    docBloat: 'TSDoc',
    collapseInto: 'ONE tagged discriminated union + exhaustive match (with `Schema`/branded owners) IN THE SAME FILE',
    gapPkg: 'BOTH tiers: the shared `libs/typescript/.api/` Effect/Schema/React rails AND the area domain packages, cross-checked against the ' +
      'published node_modules types; stacking that full surface IS new functionality woven into the owner, not naive Promise/try-catch glue',
    gapDomain: 'a chart owns scale/axis/series/interaction/annotation families and zoom/brush/tooltip/legend operations, not two naive ' +
      'renders; a service owns retry/telemetry/validation/cache layers, not a bare fetch; a projection owns the full transform/diff/patch ' +
      'family the domain needs',
    ownerGrammar: 'a CASE in the existing tagged discriminated union, a FIELD on the existing `Schema`/`Struct`/branded record, a member on ' +
      'the existing `Effect.Service`, a ROW in the existing const-union/table, or a POLICY value on the existing vocabulary',
    ownerChooser: '(2) OWNER_CHOOSER — branded/nominal types where primitives are overloaded; exact discriminated unions for closed ' +
      'alternatives; `Schema`/`Struct` for validated boundary shapes; `Effect.Service` for capability owners; `as const` union tables for ' +
      'vocabularies; kill every parallel interface/type, one-field wrapper, field-rename shape, and thrown-error control flow.',
    knob: '(3) KNOB_TEST — delete each parameter the value already encodes; a `bool`/`mode`/`batch` flag selecting bodies collapses into a ' +
      'policy value or input-shape discriminant; every timeout/retry/deadline moves off the signature into Effect combinators/layers.',
    aspects: '(4) ASPECTS — cross-cutting concerns (retry, telemetry, validation, caching, receipts, fault rails) are Effect ' +
      'combinators/layers/decorators stacked over a thin pure core, never repeated inline; co-occurring wrappers collapse into one combinator.',
    rails: '(5) RAILS — domain logic on typed-error rails (`Effect`/`Either`/`Option`), NEVER `throw` in domain code; boundaries parse through ' +
      '`Schema` (never trust input); exhaustive union handling with `assertNever`; accumulate-vs-abort correct at the boundary; no naive ' +
      '`Promise`/`async` glue where `Effect`/`Layer` belongs.',
    modernity: '(6) TS-MODERNITY — ZERO `any`/implicit-`any`/unsafe `as`/non-null `!`; no runtime `enum` (const unions or `Schema`); ' +
      '`readonly`/`as const`; template-literal/conditional/mapped types + `satisfies` where they tighten; `import type`/`export type` explicit; ' +
      'the docs/stacks/typescript file-organization overlay respected (`Effect.Service` owners are SERVICES, `Layer` wiring is COMPOSITION, ' +
      'runtime schemas are MODELS, registry rows after their owners).',
    deepPkgs: 'the Effect ecosystem (`Effect`/`Layer`/`Context`/`Schema`/`Stream`) + the area packages',
    exhaust: 'exhaustive match + `assertNever`',
    modern: 'ultra-advanced modern TS only',
  },
}
const L = LANG[LANG_KEY] || LANG.cs // inert fallback: composition no-ops before any agent call when LANG_KEY is null

const LAW = [
  'Rasm monorepo, ' + L.corpus + '. ' + L.strata + ' ' + L.stackLaw + (L.homing ? ' ' + L.homing : '') + ' ' + L.apiTiers,
  'This is a FUNDAMENTAL GROUND-UP REBUILD (or, for a `new` page, a ground-up AUTHORING) of a planning-stage DESIGN PAGE, not a polish pass. ' +
    'Improve the page objectively: collapse surfaces/types, deepen bleeding-edge spellings, maximize admitted-library capability, AND close ' +
    'the concept capability gaps.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FILE ' +
    'items (report those in residual_high).',
].concat(BRIEF ? [
  'CAMPAIGN BRIEF — READ ' + BRIEF + ' FIRST as binding scope: the SHARED-LAW HEAD plus every section covering the targeted folder(s) (each ' +
    'leg reads head + own section only). The brief overrides page-local precedent on paradigm, rename, absorb, and delete decisions; frozen ' +
    'invariants the brief names are byte-identical law; seam-canonical wire names stay frozen unless the brief explicitly amends them AND ' +
    'assigns the counterpart work; a brief RESEARCH item is resolved by verification, never asserted.',
] : []).join('\n')
const ADVERSARIAL = [
  'ADVERSARIAL STANCE — EVERY stage (author, critique, AND red-team) is HOSTILE: assume the existing fence is NAIVE, SHALLOW, JUNIOR, or ' +
    'ILLUSORY until it survives an aggressive attack; the burden of proof is ON THE CODE, never on you. "Mature", "already strong", "good ' +
    'enough", "done", "polished", and a prior `clean` verdict are REJECTED self-assessments and prior-author claims — MOST of this corpus is ' +
    L.slur + ', and it is NOT tolerable. Default to "this fence is naive and must be rebuilt to the strongest form the doctrine admits" and ' +
    'MAKE that rebuild; a no-edit verdict is reached ONLY after a genuinely aggressive attack on the real domain + the verified package ' +
    'surface finds nothing — never a first-read concession, never to avoid work. Reject "good enough" categorically.',
  'NAIVETY is a defect on TWO orthogonal axes, both intolerable: COVERAGE — the owner models a thin slice of its concept; APPROACH — ' +
    'enumerated hardcoded instances where a parameterized, algorithmic owner should GENERATE the space: a fixed roster of ' +
    'styles/patterns/variants/arms is seed DATA feeding ONE generator over named parameters, NEVER the mechanism itself. COLLAPSE FREEDOM: ' +
    'every enumerated collapse-signal list in these prompts is a FLOOR, never the complete set — any repeated structure, parallel spelling, ' +
    'or enumerable family an algebra, table, fold, or generator can own is a collapse target you find yourself.',
  'ILLUSORY / FAKE CODE is the PRIMARY target — the MOST dangerous code is the code that PRETENDS to be advanced: it uses the doctrine ' +
    'vocabulary ' + L.vocab + ', cites packages, reads dense and confident — yet is HOLLOW. Treat dense, confident-looking fences with MORE ' +
    'suspicion, not less, and DISBELIEVE every claim the page makes about itself until you verify it against the real domain and the ' +
    'catalogued package surface. HUNT: a name/signature/prose that PROMISES capability the body does not implement; an owner naive on ' +
    'EITHER axis (a COVERAGE thin slice — a 2-case union for a 20-case domain, the obvious 3 fields where the concept carries fifteen; an ' +
    'APPROACH roster of enumerated instances where a generator belongs); decorative ' +
    'density, ceremony, and vocabulary carrying no real capability; a placeholder/stub/sketch dressed as a finished design; prose that ' +
    'ASSERTS richness the fence does not contain; a structurally-correct collapse that is semantically empty; ' + L.illusion + '. Every such ' +
    'illusion is a DEFECT to rebuild, not a feature to preserve; where you genuinely cannot break the fence, say so by finding nothing — but ' +
    'earn it; never invent churn to look busy either.',
].join('\n')
const EXTEND = [
  'CAPABILITY EXTENSION (justified, in-place, never flat spam) — structural collapse and .api-stacking are NECESSARY but NOT SUFFICIENT. A ' +
    'page can be fully collapsed into one polymorphic owner and STILL be capability-thin: modeling a NAIVE, LIMITED slice of its domain ' +
    'concept — a flat membership/id set where the concept owns geometry, metrics, per-kind attributes, topology, and operations; a 2-case ' +
    'vocabulary where the domain has twenty; a record with the obvious 3 fields where the concept carries fifteen. Structural completeness ' +
    'and CAPABILITY completeness are ORTHOGONAL. A FULL rebuild ALSO closes the capability gap so the page OWNS ITS DOMAIN CONCEPT ' +
    'COMPLETELY. Per COMPOSED_IMPLEMENTATION + the doctrine growth law (capability grows sublinearly; growth lands as cases/rows/policy-values ' +
    'INSIDE existing owners, never new surfaces beside them), every real missing concern lands as ' + L.ownerGrammar + ' — reshaping the owner ' +
    'as if it had always carried it; NEVER a parallel type, a new file, a sibling shape, or flat appended code.',
  'GAP SOURCES (every extension MUST cite exactly one — justified, never speculative): (a) PACKAGE — a member the admitted package/host ' +
    'surface exposes that the concept ADMITS but the page IGNORES is a missing case in the owner law (' + L.gapPkg + '; the apiUnderutilized ' +
    'reading map is the first place to look; verify the member via ' + L.verify + '). (b) DOMAIN — an attribute, metric, sub-kind, ' +
    'relationship, state, or operation the REAL concept demands but the page omits (' + L.gapDomain + '). (c) CONSUMER — a contract a sibling ' +
    'or downstream owner will require that has no composed spelling here yet (a need with no spelling marks a missing case: the law extends ' +
    'first, the feature lands second).',
  'COVERAGE OVER SIZE: byte-count is a WEAK proxy — capability COVERAGE against the full domain + both-tier package surface is the real ' +
    'measure. A SMALL page modeling a rich concept is almost always under-built (give it the DEEPEST sweep), AND a LARGE, well-collapsed page ' +
    'can still be capability-SPARSE. Assess each owner against its domain independently of size and EXTEND every owner the concept ' +
    'under-realizes IN PLACE — integrated and unified into the one owner at full operator depth, every new field/case/operation composing the ' +
    'existing rails — never a new flat surface beside it.',
  'JUSTIFIED, NOT RANDOM: if after a real domain + package + consumer sweep the concept is genuinely complete, prove it by adding nothing — ' +
    'never invent capability to look busy or pad with flat fields. Every added case/row/field/operation is load-bearing, cites a package ' +
    'member / domain attribute / consumer contract, and composes the existing rails; preserve ALL existing capability — extension only ' +
    'deepens, never regresses.',
].join('\n')
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the ' +
    'controlling rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, ' +
    'freshness disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the ' +
    'load-bearing contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a ' +
    'paragraph. Prose that ASSERTS capability the fence does not implement is a defect, not content.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in backticks. ' +
    'Name the exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or ' +
    'remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
  'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ' +
  'ONLY where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line ' +
  'where a comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no ' +
  'narration, no task/process/session/history/proof/review comments, no ' + L.docBloat + ' bloat. Densify names and types so comments are ' +
  'rarely needed; cut every low-value comment.'
const PRE = [LAW, '', ADVERSARIAL, '', L.ultra, '', EXTEND].concat(L.mechanics ? ['', L.mechanics] : [])
  .concat(['', L.patlaw, '', L.boundaries, '', PROSE, '', COMMENTS, '']).join('\n')

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
const chunk = (arr, n) => { const o = []; for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n)); return o }
const dedup = (rs) => [...new Map(rs.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
// packClusters — consolidate the union-find clusters into AT MOST n buckets (greedy smallest-bucket) so reconcile stays one bounded pass.
const packClusters = (clusters, n) => {
  if (clusters.length <= n) return clusters
  const buckets = Array.from({ length: n }, () => [])
  const sorted = clusters.slice().sort((a, b) => b.length - a.length || ((a[0] && a[0].claim) || '').localeCompare((b[0] && b[0].claim) || ''))
  for (const c of sorted) { let mi = 0; for (let i = 1; i < n; i++) if (buckets[i].length < buckets[mi].length) mi = i; buckets[mi].push(...c) }
  return buckets.filter((b) => b.length)
}
const unionFind = (uniq) => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
}
const folderOf = (p) => { const head = p.split('/.planning/')[0].split('/'); return head[head.length - 1] || 'root' }

// MAP_BY_PAGE — the per-page reading map Discover surfaced, indexed for every downstream stage.
let MAP_BY_PAGE = new Map()
const mapFor = (p) => MAP_BY_PAGE.get(p.page || p) || { page: p.page || p, kind: p.kind || 'rebuild', absorb: p.absorb, apiUsed: [], apiUnderutilized: [], contextNote: '', stackingGuidance: '', weak: true }
const mapsFor = (pgs) => pgs.map((p) => mapFor(p))
const READ_MANDATE = (maps) => 'READING MAP (the per-page grounding Discover surfaced — an INITIAL POINTER, never the ceiling):\n' +
  JSON.stringify(maps, null, 1) + '\nBEFORE editing, ULTRA-DEEP-READ: (1) the language doctrine — ' + L.readLaw + ' — never a subset; ' +
  (BRIEF ? '(2) the campaign brief ' + BRIEF + ' (the shared-law head + every section covering these folders) in FULL; ' : '') +
  '(' + (BRIEF ? '3' : '2') + ') BOTH .api tiers — ' + L.apiTiers + ' — list both tier directories in full and read every catalog relevant to ' +
  'these pages. The map POINTS; you VERIFY and EXCEED it: COMPOSE every `apiUsed` catalog at full operator depth, STACK every ' +
  '`apiUnderutilized` {catalog, capability} INTO the owning page (closing the underutilization is new functionality woven into the owner — a ' +
  'case/row/field/operation), AND independently CONFIRM no other relevant admitted catalog (either tier) is missing or unconsidered — an ' +
  'admitted capability the concept admits that NO owner exploits is a DEFECT to close, and the map never licenses a skim: the discovery ' +
  'agent only pointed, you do the exhaustive read. Verify every cited member via ' + L.verify + '; a member you cannot verify is a ' +
  'phantom to delete.'

const planPrompt = (pkgHint) => ['Rasm monorepo. TASK: thin enumerate + classify (read-only, do NOT edit). TARGETS (repo-relative): ' +
  JSON.stringify(TARGETS) + '. Each TARGET is a package/area ROOT (e.g. ' + L.root + '/<Package>), a SUB-FOLDER under .planning at ANY depth, ' +
  'or a specific design FILE. The OWNING FOLDER of a target is the path BEFORE "/.planning/", or the target itself when it has no ' +
  '"/.planning/" segment. ' + (L.homing ? L.homing + ' ' : '') + 'Use find/ls; do NOT cd. Return packages — one entry per DISTINCT owning ' +
  'folder: {name: the LAST path segment of the owning root, root, planning: "<root>/.planning" (the homing special case overrides), api: ' +
  '"<root>/.api"}. PAGES: expand each target — a ROOT target expands to EVERY design page under its planning tree (repo-relative *.md); a ' +
  'SUB-FOLDER target to every page under it at ANY depth; a FILE target to itself; union + dedup; EXCLUDE ' +
  'IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md and any _*.md campaign brief from the page list.',
  BRIEF
    ? 'CAMPAIGN BRIEF: READ ' + BRIEF + ' — the shared-law head plus every section covering the targeted folder(s). Classify each page FROM ' +
      'THE BRIEF against REAL DISK STATE: a brief-NEW page confirmed ABSENT on disk -> kind `new` (it may open a NEW sub-folder; carry ' +
      '`absorb` when the brief names an existing page it absorbs); a brief-new page that ALREADY EXISTS on disk -> kind `rebuild`; a page the ' +
      'brief names for rebuild/paradigm work -> kind `rebuild`; every OTHER existing page under the targets -> kind `improve` (the cold-pass ' +
      'set). deletePages: ONLY brief-declared outright deletions confirmed PRESENT on disk whose content the brief maps to a destination ' +
      'OUTSIDE the page set (report each as {page, capturedIn: the brief-named destination}); an absorbed page whose absorber is IN the page ' +
      'set travels via `absorb` on the absorber instead, never in deletePages; a brief-delete page already ABSENT on disk is a no-op — ' +
      'exclude it entirely.'
    : 'No campaign brief: every page gets kind `rebuild`; deletePages is empty; no absorb pairs.',
  pkgHint].filter(Boolean).join('\n')
const discoverPrompt = (batch) => [LAW, '', ADVERSARIAL, '', 'TASK: READ-ONLY DISCOVERY for these ' + batch.length + ' pages (investigate, do ' +
  'NOT edit): ' + batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') + '. ' +
  (BRIEF ? 'Read ' + BRIEF + ' (head + the sections covering these folders) for the per-page concern. ' : '') +
  'For a kind=`improve`/`rebuild` page READ the page IN FULL; for a kind=`new` page (it does not exist yet) read its concept ' +
  (BRIEF ? 'in the brief' : 'from the folder charter') + ' + its nearest SIBLING pages. ALSO read the folder at large — the sibling pages ' +
  'each composes and the owning-folder index docs (ARCHITECTURE.md + README.md) — as FULL-FILE reads, never a skim, a section-sample, or a ' +
  'memory-recall inventory. Then ENUMERATE BOTH .api tiers IN FULL with a real `ls` — ' + L.apiTiers + ' — AND the doctrine inventory IN ' +
  'FULL with a real ls/find from the source of truth (never memory), holding the language doctrine (' + L.readLaw + ') as the bar. For EACH ' +
  'page produce its reading map: (a) `apiUsed` — the ' +
  '.api catalogs the page CURRENTLY composes, BOTH tiers (for a new page, the catalogs its concept WILL compose); (b) `apiUnderutilized` — ' +
  'each {catalog, capability}: an admitted catalog or member (either tier) the page concept ADMITS but the page IGNORES — REAL analysis ' +
  'against the verified .api inventory, never a guess, naming the concrete capability the implement MUST stack; (c) `contextNote` — the page ' +
  'contextual relation: which sibling owners/seams it composes, where it sits in the folder, which folder entry/receipt seam it contributes ' +
  'to; (d) `stackingGuidance` — the INITIAL pointer on what api stacking + capability extension the implement should add (the implement ' +
  'confirms + deepens it); (e) `weak` — true when the page is naive/thin/illusory relative to the doctrine bar (a hostility verdict, never a ' +
  'courtesy). Your product is a MAP, never a bare verdict — the initial pointer every downstream stage verifies and EXCEEDS, never a ' +
  'ceiling, never a license for a downstream skim. Members are verified via ' + L.verify + '; never list a phantom. Return worklist (each ' +
  '{page, kind, absorb?, apiUsed, apiUnderutilized, contextNote, stackingGuidance, weak}).'].join('\n')
const implementPrompt = (batch) => [PRE, READ_MANDATE(mapsFor(batch)), '', 'TASK: HOSTILE IMPLEMENT of these ' + batch.length + ' pages IN ' +
  'PLACE, each per its kind: ' + batch.map((p) => p.page + ' [' + mapFor(p).kind + ']').join(', ') + '. kind=`new`: GROUND-UP AUTHOR the page ' +
  '(it does not exist; it may open a NEW sub-folder) to the full doctrine + domain-complete capability bar, in the code-fence-first ' +
  'design-page form of its mature siblings, wired into the owning-folder ARCHITECTURE.md + README.md maps and the folder entry/receipt seam ' +
  'owners where the folder has them; if its reading map names an absorb target, MOVE the real content over and then DELETE the absorbed page ' +
  'so no duplicate owner remains. kind=`rebuild`: HOSTILE GROUND-UP REBUILD in place. kind=`improve`: general cold-pass improve. EVERY page: ' +
  'DISBELIEVE it (assume naive/junior/illusory until it survives an aggressive attack; dense confident code is the PRIME suspect for ' +
  'hollowness); construct in LIFECYCLE order (admit raw ONCE -> canonical owner by OWNER_CHOOSER -> stacked rail/aspect over a thin pure core ' +
  '-> projection -> egress, BOTH parameterized); collapse parallel shapes into ' + L.collapseInto + '; one polymorphic entrypoint per ' +
  'modality. COMPOSE the reading-map `apiUsed` at full operator depth + STACK every `apiUnderutilized` into the owner + CONFIRM no other ' +
  'admitted catalog is missing (the map is the initial pointer; you ultra-deep-read both .api tiers yourself). CLOSE the concept capability ' +
  'gaps by growing the EXISTING owner in place (case/row/field/operation/policy-value, each citing a package member / domain attribute / ' +
  'consumer contract). ' + (BRIEF ? 'Every paradigm, rename, absorb, and delete decision follows the brief; frozen invariants the brief names ' +
  'stay byte-identical. ' : '') + L.modern + '; high-signal all-backticked prose. Keep the owning-folder index docs truthful for every page ' +
  'you add, absorb, or delete. WRITE-FULLY now (the fix-log REPORTS edits already made). Cross-file items -> residual_high {files, claim}. ' +
  'Return the fix-log (files = pages you touched) + collapsed + extended + residual_high + summary.'].join('\n')
const deletePrompt = (dels) => [LAW, '', L.boundaries, '', 'TASK: BRIEF-DECLARED PAGE DELETIONS (verify-then-delete, zero content loss). The ' +
  'brief maps the content of each page below to a destination OUTSIDE the page set. For EACH page: read it IN FULL, read the brief-named ' +
  'destination (capturedIn), and VERIFY the page design intent is genuinely captured there — every load-bearing owner, seam row, and real ' +
  'standards value accounted for. Where captured, DELETE the file and update the owning-folder ARCHITECTURE.md + README.md maps (and any ' +
  'sibling page that still routes to it). Where NOT fully captured, do NOT delete — report the exact gap as residual_high {files: [page, ' +
  'destination], claim}. Pages:\n' + JSON.stringify(dels, null, 1)].join('\n')
const critiquePrompt = (batch, i) => [PRE, READ_MANDATE(mapsFor(batch)), '', 'TASK: HOSTILE DOCTRINAL-CONFORMANCE + CAPABILITY-COMPLETENESS ' +
  'AUDIT + FIX IN PLACE over these ' + batch.length + ' pages (batch ' + i + ', audit EACH independently, fix EACH in place): ' +
  batch.map((p) => p.page).join(', ') + '. You are an ULTRA-HARSH, UNAGREEABLE auditor: assume a violation exists in EVERY fence until you ' +
  'prove otherwise, trust NOTHING the author or prose claims, treat dense confident code as the PRIME suspect for hollowness, and "good ' +
  'enough"/"mature"/a prior clean verdict is rejected outright. For EACH page run the MECHANICAL checklists line-by-line — each checklist is ' +
  'a FLOOR you hunt past, never the complete audit — and REPAIR every hit in place (a fix, never a ledger note): (1) COLLAPSE_SCAN (12 ' +
  'signals, 3+ instances mandatory; the 12 are a FLOOR — hunt collapse targets beyond them): sibling prefix/suffix names -> one ' +
  'modality-polymorphic entrypoint; same return rail differing only by arity -> input-shape discrimination; a get/get-many/get-by family -> ' +
  'one input-keyed entrypoint; functions differing only by a literal -> parameterize as policy; a bool selecting two bodies -> one derived ' +
  'body; a one-call hop -> delete it; a one-public-method class -> module function or fold-on-owner; parallel dispatch arms -> a derived ' +
  'table or fold; several types sharing fields -> one closed family; 3+ sibling constants -> one vocabulary/table owner; a wrapper renaming ' +
  'a package API -> use the package directly; recurring 2-4 wrappers -> one aspect factory; >=3 parallel shapes -> ' + L.collapseInto + '. ' +
  L.ownerChooser + ' ' + L.knob + ' ' + L.aspects + ' ' + L.rails + ' ' + L.modernity + ' (7) CAPABILITY-COMPLETENESS + ILLUSION — structural ' +
  'collapse and capability completeness are ORTHOGONAL: verify the body implements what the names/prose promise; ANY capability the ' +
  'reading-map `apiUnderutilized` / the both-tier admitted-package surface / the real domain / a consumer contract admits that the owner ' +
  'OMITS is a DEFECT — close it in place by growing the EXISTING owner, citing its source; attack BOTH naivety axes — COVERAGE (a thin ' +
  'slice of the concept) AND APPROACH (a roster of parallel arms/rows/values where ONE parameterized generator belongs — demote the roster ' +
  'to seed DATA feeding that generator); delete speculative/padding/decorative additions. ' +
  'Also enforce the ' + L.stack + ' file-organization + section-order law, cross-package convention consistency, and prose + comment ' +
  'hygiene. EDIT to fix every hit; OVERRIDE any earlier residual you can now resolve. Return the batched fix-log (files = pages touched) + ' +
  'extended + residual_high — each a {files:[...], claim} object for CROSS-FILE items only.'].join('\n')
const redteamPrompt = (batch, i, crit) => [PRE, READ_MANDATE(mapsFor(batch)), '', 'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE over ' +
  'these ' + batch.length + ' pages (batch ' + i + ', red-team EACH independently, fix EACH in place): ' + batch.map((p) => p.page).join(', ') +
  '. You are the LAST and MOST AGGRESSIVE pass: assume the author and critique missed things and the chosen design is naive or illusory ' +
  'until PROVEN the strongest, burden of proof ON THE DESIGN; trust nothing the prior passes or the prose claimed. For EACH page: (A) ' +
  'COUNTERFACTUAL on the core owner/algebra/dispatch — is it categorically the strongest form the doctrine admits, or does a denser owner ' +
  '(' + L.collapseInto + '), a derived data table, a parameterized GENERATOR owning an enumerated space (the roster demoted to seed DATA ' +
  'over named parameters), or a DEEPER admitted-package primitive (' + L.deepPkgs + '; an `apiUnderutilized` member is the first place to ' +
  'look) collapse the whole fence? If a fundamentally stronger design exists, rebuild to it — never defend the ' +
  'incumbent. (B) ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: the next case/dimension/knob/modality/provider lands as ONE ' +
  'case/row/policy value with every consumer untouched or broken LOUDLY at type-check (' + L.exhaust + '); if it would touch multiple sites, ' +
  'reshape so the growth axis is a case, row, policy value, or carrier swap. (C) LONG-TAIL multi-dimensional attack ' +
  '(empty/singular/plural/stream/malformed/concurrent/cancelled/partial-failure/version-skew); accumulate-vs-abort correct for the REAL ' +
  'boundary; BOTH ingress AND egress parameterized so the owner sources and sinks across hundreds of consumers without interior edits. (D) ' +
  'BOUNDARY/STRATA-INTEGRITY — a concern owned twice, a folder mixing concerns, a concern scattered across folders, a downward dependency, ' +
  'or coupling to a sibling owner INTERIOR (vs its wire/seam) is a defect; fix it or record it as a cross-file residual. (E) SURFACE-SPRAWL ' +
  '+ PHANTOMS — an admitted package whose .api exposes capability the fence re-derives by hand, flat code below the operator depth the ' +
  'packages reach, a phantom member (cited but unverifiable via ' + L.verify + ' — delete it), or a thin wrapper: collapse to package depth ' +
  'and verify the member exists. (F) CAPABILITY-COMPLETENESS + ILLUSION — counterfactually attack the owner for DOMAIN-COMPLETENESS ' +
  'independently of how collapsed or confident it looks; name any omitted capability (reading-map / domain / consumer) with a cite and ' +
  'EXTEND THE OWNER IN PLACE; conversely REJECT any flat-spam/speculative/parallel-surface extension. ALSO run a FULL COLD ADVERSARIAL ' +
  'RE-REVIEW of every conformance dimension with fresh hostile eyes — the COLLAPSE_SCAN signals (a FLOOR — hunt collapse targets beyond ' +
  'them), OWNER_CHOOSER correctness per shape, the KNOB_TEST per param, the aspect taxonomy, rail + closed-fault discipline, ' +
  'capability-completeness + illusion + BOTH naivety axes (COVERAGE + APPROACH) per owner, ' + L.modern + ' ' +
  'typing, full ' + L.stack + ' conformance, both-tier .api maximization, and prose/comment hygiene — and fix every defect. Even absent a ' +
  'structural rebuild the fence must end objectively denser, MORE CAPABLE, more correct than the critique left it; if the strongest form is ' +
  'genuinely present, prove it by finding nothing — never invent churn. CARRY FORWARD any unresolved cross-file residual from the critique ' +
  'result below and add your own. CRITIQUE RESULT:\n' + JSON.stringify((crit && { verdict: crit.verdict, residual_high: crit.residual_high || [] }) || {}, null, 1) +
  '\nReturn the batched fix-log (files = pages touched) + extended + residual_high — each a {files:[...], claim} object for CROSS-FILE ' +
  'items only.'].join('\n')
const reconcilePrompt = (bucket, pkgs) => [PRE, 'TASK: RECONCILE these cross-FILE residuals the build pass deferred. There is NO severity — ' +
  'treat EVERY residual as must-address. Your blast radius is the OWNING FOLDER(S) of this run (' + pkgs + ') PLUS every file a residual ' +
  'names, wherever it lives under libs/: any sibling page under the owning folders, the owning-folder index docs (ARCHITECTURE.md + ' +
  'README.md), the folder entry/receipt seam owners, AND the out-of-target seam counterparts — a sibling folder in the SAME language whose ' +
  'seam rows, consumer sites, or index docs the built pages disturbed, and the cross-LANGUAGE seam mirrors (the counterpart ARCHITECTURE ' +
  'seam records and decode-side vocabulary) — read and fix ANY of them so BOTH endpoints of every touched seam stay mirrored. Out-of-target ' +
  'edits are SEAM-SCOPED: repair only the seam/consumer drift the build caused (rename mirrors, seam-row records, decoded vocabulary, index ' +
  'routing), never rebuild a foreign folder interior; seam-canonical wire names stay frozen unless the brief explicitly amends them. ' +
  (BRIEF ? 'The brief (' + BRIEF + ') consumer-ripple rules govern every seam-name decision and every counterpart edit. ' : '') + 'Read ' +
  'EVERY listed file. For each residual: if it is a real cross-file defect, FIX it in place (unify the shared type/seam/rail, add the ' +
  'depended-on case/field, repair the strata/boundary issue, update the index-doc maps); if it is FACTUALLY INCORRECT, leave it and say why ' +
  'in the summary — never silently skip a real one to avoid work. Preserve all capability, regress nothing. Residuals:\n' +
  JSON.stringify(bucket, null, 1)].join('\n')
const verifyPrompt = (work) => [PRE, 'TASK: TERMINAL RECONCILE VERIFY — ADVERSARIAL, CRITIQUE-GRADE, WRITING (never a friendly ' +
  'confirmation) + FIX IN PLACE, the LAST pass (no further spawning, no new rounds). Fix agents claim to have resolved the residual buckets ' +
  'below. Work BUCKET-BY-BUCKET — never skim a bucket: re-read EVERY named file from disk, RE-DERIVE from the residual + the files whether ' +
  'the claimed work was NECESSARY, PROVE on disk it was done properly, and hold each claim to the CRITIQUE bar — is the fix the OBJECTIVELY ' +
  'BEST cross-file reconciliation (complete, dense, doctrine-conformant, BOTH seam endpoints aligned, index-doc maps truthful, all ' +
  'capability preserved) or is it LOOSE/WEAK/token/partial? Where the fix is loose/weak or a better reconciliation of these SAME files ' +
  'exists, REPAIR it IN PLACE NOW to the objectively-best ROOT-LEVEL form — a single-point patch where a root-level dense reconstruction of ' +
  'the same files is available is ITSELF a defect you repair; you FIX rather than defer. Then classify EVERY claim (ONE verdict per claim; ' +
  'a dropped claim cannot validate): "fixed" (real, complete, objectively-best after your repair), "invalid" (provably wrong or genuinely ' +
  'unnecessary — cite why), or "open" (a claim you genuinely CANNOT reach from these files — an external blocker needing an out-of-scope ' +
  'decision; describe it; NEVER mark open to punt a fix you could strengthen). Do NOT surface new unrelated cross-file work and do NOT ' +
  'request another round. Report every file you repaired in `repaired`. BUCKETS (each with its residual claims and the files its fixer ' +
  'touched):\n' + JSON.stringify(work, null, 1)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length) { log('No targets — pass a target path, an array, or {targets, brief}. Empty args is a no-op.'); return { targets: [], total: 0 } }
if (!LANG_KEY) { log('Targets must live under ONE language root (libs/csharp | libs/python | libs/typescript). Got: ' + JSON.stringify(TARGETS)); return { targets: TARGETS, total: 0 } }

// --- [PLAN]
phase('Plan')
const plan = await agent(planPrompt(''), { label: 'plan', phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN_SCHEMA, stallMs: STALL })
const packages = ((plan && plan.packages) || []).filter((p) => p && p.name)
const PAGES = [...new Map(((plan && plan.pages) || []).filter((p) => p && p.page).map((p) => [p.page, { page: p.page, kind: p.kind || 'rebuild', absorb: p.absorb }])).values()]
  .sort((a, b) => a.page.localeCompare(b.page))
const deletePages = ((plan && plan.deletePages) || []).filter((d) => d && d.page)
const kindCount = (k) => PAGES.filter((p) => p.kind === k).length
log('Plan[' + LANG_KEY + (BRIEF ? '|brief' : '') + ']: ' + PAGES.length + ' pages (' + kindCount('new') + ' new, ' + kindCount('rebuild') + ' rebuild, ' +
  kindCount('improve') + ' improve) + ' + deletePages.length + ' outright deletion(s) across ' + packages.length + ' folder(s) [' +
  packages.map((p) => p.name).join(', ') + ']; CAP=' + CAP)
if (!PAGES.length && !deletePages.length) { log('No pages resolved under the targets'); return { targets: TARGETS, language: LANG_KEY, brief: BRIEF, total: 0 } }

// --- [DISCOVER]
phase('Discover')
const discovered = (await pool(chunk(PAGES, DISCOVER_BATCH), CAP, (batch, i) =>
  agent(discoverPrompt(batch), { label: 'discover:b' + i, phase: 'Discover', model: 'opus', effort: 'max', schema: DISCOVER_SCHEMA, stallMs: STALL }))).filter(Boolean)
MAP_BY_PAGE = new Map(discovered.flatMap((d) => (d.worklist || []).filter((w) => w && w.page).map((w) => [w.page, w])))
log('Discover: reading map for ' + MAP_BY_PAGE.size + '/' + PAGES.length + ' pages (' +
  [...MAP_BY_PAGE.values()].filter((w) => w.weak).length + ' marked weak) across ' + discovered.length + ' batch(es)')

// --- [IMPLEMENT]
phase('Implement')
const implWork = chunk(PAGES, IMPL_BATCH).map((batch) => ({ kind: 'impl', batch }))
if (deletePages.length) implWork.push({ kind: 'delete', batch: deletePages })
const builtRaw = await pool(implWork, CAP, (w, i) => w.kind === 'delete'
  ? agent(deletePrompt(w.batch), { label: 'delete:brief', phase: 'Implement', model: 'opus', effort: 'high', schema: DELETE_SCHEMA, stallMs: STALL })
  : agent(implementPrompt(w.batch), { label: 'impl:b' + i + ':' + folderOf(w.batch[0].page), phase: 'Implement', model: 'fable', effort: 'max', schema: FIXLOG_SCHEMA, stallMs: STALL }))
const deleteLog = deletePages.length ? builtRaw[implWork.length - 1] : null // pool preserves item order; the delete executor is the last work item
const builtAll = builtRaw.filter(Boolean)
const built = builtAll.filter((r) => Array.isArray(r.files))
const deleted = (deleteLog && deleteLog.deleted) || []

// --- [CRITIQUE]
// --- [REDTEAM]
phase('Critique')
phase('Redteam')
const reviewed = (await pool(chunk(PAGES, REVIEW_BATCH), CAP, async (batch, i) => {
  const crit = await agent(critiquePrompt(batch, i), { label: 'critique:b' + i, phase: 'Critique', model: 'fable', effort: 'xhigh', schema: REVIEW_SCHEMA, stallMs: STALL })
  const redteam = await agent(redteamPrompt(batch, i, crit), { label: 'redteam:b' + i, phase: 'Redteam', model: 'fable', effort: 'max', schema: REVIEW_SCHEMA, stallMs: STALL })
  return { batch, crit, redteam }
})).filter(Boolean)

// --- [RECONCILE]
const inLibs = (p) => typeof p === 'string' && (p.indexOf('libs/') === 0 || p.indexOf('/libs/') !== -1)
const norm = (x, page) => { const files = Array.isArray(x.files) ? x.files.filter(inLibs) : []; return { files: files.length ? files : [page], claim: x.claim } }
const fallbackPage = (PAGES[0] && PAGES[0].page) || (packages[0] && packages[0].planning) || L.root
const allRes = []
for (const r of builtAll) if (r && r.residual_high) for (const x of r.residual_high) allRes.push(norm(x, (r.files && r.files[0]) || fallbackPage))
for (const r of reviewed) for (const k of ['crit', 'redteam']) { const l = r[k]; if (l && l.residual_high) for (const x of l.residual_high) allRes.push(norm(x, (l.files && l.files[0]) || fallbackPage)) }
const uniq = dedup(allRes.filter((r) => r && r.claim))
const clusters = unionFind(uniq)
const buckets = packClusters(clusters, RECON_CAP)
log('Build: ' + built.length + ' implement batch(es), ' + reviewed.length + ' review pair(s), ' + deleted.length + ' page(s) deleted; ' +
  'reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' clusters -> ' + buckets.length + ' bucket(s) (cap ' + RECON_CAP + ', ONE pass, no re-entry)')
let hard_residual = []
let dropped = []
let repaired = []
if (buckets.length) {
  phase('Reconcile')
  const pkgs = packages.map((p) => p.planning || p.root).join(', ')
  const fixes = await pool(buckets, RECON_CAP, (bucket, i) =>
    agent(reconcilePrompt(bucket, pkgs), { label: 'reconcile-fix:' + i, phase: 'Reconcile', model: 'opus', effort: 'max', schema: RESIDUAL_FIX_SCHEMA, stallMs: STALL }))
  const work = buckets.map((bucket, i) => ({ bucket, fix: fixes[i] }))
  for (const w of work) {
    if (!w.fix) { hard_residual.push(...w.bucket) } // fixer skipped/died — its residuals stay live
    else if (w.fix.residual_high) hard_residual.push(...w.fix.residual_high.map((x) => norm(x, fallbackPage))) // newly surfaced: NO re-entry, hand off
  }
  const changed = work.filter((w) => w.fix && w.fix.verdict === 'fixed' && (w.fix.files || []).length)
  const cleaned = work.filter((w) => w.fix && !(w.fix.verdict === 'fixed' && (w.fix.files || []).length))
  dropped = cleaned.flatMap((w) => w.bucket.map((r) => r.claim + ' [fixer verdict clean: ' + (w.fix.summary || 'judged not a real defect') + ']'))
  if (changed.length) {
    const verify = await agent(verifyPrompt(changed.map((w) => ({ residuals: w.bucket, fixerTouched: w.fix.files, fixerSummary: w.fix.summary }))),
      { label: 'reconcile-verify', phase: 'Reconcile', model: 'fable', effort: 'xhigh', schema: VERIFY_SCHEMA, stallMs: STALL })
    const claims = (verify && verify.claims) || []
    const openClaims = new Set(claims.filter((c) => c.status === 'open').map((c) => c.claim))
    hard_residual.push(...changed.flatMap((w) => w.bucket).filter((r) => openClaims.has(r.claim)))
    dropped.push(...claims.filter((c) => c.status === 'invalid').map((c) => c.claim))
    repaired = (verify && verify.repaired) || []
    if (!verify) hard_residual.push(...changed.flatMap((w) => w.bucket)) // verify skipped/died — claims stay live
  } else { log('Reconcile: no fixer changed a file — terminal verify skipped (guard)') }
  hard_residual = dedup(hard_residual)
}
log('Reconcile: ' + buckets.length + ' bucket(s); ' + hard_residual.length + ' hard residual -> resolve-residuals, ' + dropped.length +
  ' dropped, verify repaired ' + repaired.length + ' file(s)')
return { targets: TARGETS, language: LANG_KEY, brief: BRIEF, packages: packages.map((p) => p.name), pages: PAGES.length,
  kinds: { new: kindCount('new'), rebuild: kindCount('rebuild'), improve: kindCount('improve') }, deleted: deleted,
  deleteKept: (deleteLog && deleteLog.kept) || [], clusters: clusters.length, buckets: buckets.length, hard_residual: hard_residual, dropped: dropped, repaired: repaired }
