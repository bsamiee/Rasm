export const meta = {
  name: 'implement-cs',
  whenToUse: 'Realize open cards into design-page code fences for the first C# target set (AppHost, Compute, AppUi, Persistence).',
  description: 'Realize every open IDEAS/TASKLOG card across the four C# session targets (Rasm.AppHost, Rasm.Compute, Rasm.AppUi, Rasm.Persistence) into deep design-page code FENCES at the docs/stacks/csharp 11/10 bar, resolve all ripples, and truthfully close the cards. One discovery agent maps cards + ripple classes + blockers; each target folder is realized as ONE implement -> critique -> redteam cycle (per folder, all WRITE, both reviews adversarial, fix-in-place; BLOCKED probe + folder-local package admission inline, no prep phase); a bounded reconcile aligns in-scope seams, realizes 1-hop out-of-scope C# ripple counterparts, and applies the single central package-pin serially; a final per-folder closeout verify-remediate-and-closes complete cards. Disposable, C#-only. args = a target path string, an array of target paths, or empty for the four defaults.',
  phases: [
    { title: 'Discover', detail: 'one agent: real-listing enumeration of both .api tiers + the doctrine inventory, full reads of the card files, every page the open cards name, and the folder at large; extract open cards (all tasks incl atomic + 1-3 ideas), sequence each folder, classify every ripple (in_scope / oos_csharp / cross_lang), record in-scope gates, malformed ripples, and a per-page capability map downstream stages treat as a pointer never a ceiling' },
    { title: 'Realize', detail: 'per target folder, pooled at CAP: implement(max) -> critique(max, adversarial + charter-completeness) -> redteam(max, adversarial + staleness lens); all WRITE, fix-in-place, own-pages-only, cross-folder seams logged as residuals' },
    { title: 'Reconcile', detail: 'bounded single pass: cluster cross-folder residuals by shared file -> fix(max: align in-scope seam / realize 1-hop oos counterpart / apply central pin / defer cross-lang) -> WRITING verify(xhigh): re-derive necessity, prove on disk, repair weak fixes to the root, then classify' },
    { title: 'Closeout', detail: 'per folder: verify each card vs full charter, FINAL-remediate weak cards in place, close genuinely-complete (move to [02]-[CLOSED], collapse, update ARCHITECTURE [02]-[SEAMS]), honestly re-open the rest; strength-demotion makes closed mechanically truthful' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10 // in-flight ceiling for every pooled stage; with the launch gate, real work fans to CAP while a fast-fail cascade never exceeds 1 launch / STAGGER_MS
// The gate spaces EVERY agent start >= STAGGER_MS apart for the pool's whole life, so a
// synchronized batch of fast failures (a rate-limit cascade) cannot free every worker at once and
// re-fire a thundering-herd of launches. Real (slow) work still fans out to CAP since each worker
// runs far longer than STAGGER; a fast-fail cascade self-throttles to <= 1 launch / STAGGER_MS.
const STAGGER_MS = 1500
const DEFAULT_TARGETS = ['libs/csharp/Rasm.AppHost', 'libs/csharp/Rasm.Compute', 'libs/csharp/Rasm.AppUi', 'libs/csharp/Rasm.Persistence']

// --- [INPUTS] ----------------------------------------------------------------------------
const norm = (t) => { const s = String(t).trim(); return s.indexOf('libs/') === 0 ? s : 'libs/csharp/' + s }
const TARGETS = Array.isArray(args) ? args.filter(Boolean).map(norm)
  : (args && typeof args === 'object' && Array.isArray(args.targets)) ? args.targets.filter(Boolean).map(norm)
  : (typeof args === 'string' && args.trim() && args.trim().toUpperCase() !== 'ALL') ? [norm(args)]
  : DEFAULT_TARGETS
const TARGET_SET = new Set(TARGETS)

// --- [MODELS] ----------------------------------------------------------------------------
const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['targets'], properties: {
  targets: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['folder', 'order', 'tasks', 'ideas', 'ripples'], properties: {
    folder: { type: 'string' },
    order: { type: 'array', items: { type: 'string' } },
    tasks: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'status'], properties: { slug: { type: 'string' }, status: { type: 'string' }, atomic: { type: 'boolean' }, thesis: { type: 'string' } } } },
    ideas: { type: 'array', maxItems: 3, items: { type: 'object', additionalProperties: false, required: ['slug', 'status'], properties: { slug: { type: 'string' }, status: { type: 'string' }, thesis: { type: 'string' } } } },
    ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from_slug', 'klass', 'to_pkg', 'to_slug'], properties: { from_slug: { type: 'string' }, klass: { type: 'string', enum: ['in_scope', 'oos_csharp', 'cross_lang'] }, to_pkg: { type: 'string' }, to_slug: { type: 'string' } } } },
    gates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['blocked_slug', 'gated_by_slug', 'in_scope'], properties: { blocked_slug: { type: 'string' }, gated_by_slug: { type: 'string' }, in_scope: { type: 'boolean' } } } },
    map: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'composed', 'underutilized', 'seams', 'stacking', 'call'], properties: { page: { type: 'string' }, composed: { type: 'string' }, underutilized: { type: 'string' }, seams: { type: 'string' }, stacking: { type: 'string' }, call: { type: 'string', enum: ['weak', 'strong'] } } } },
  } } },
  malformed_ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from_slug', 'raw'], properties: { from_slug: { type: 'string' }, raw: { type: 'string' } } } },
} }
const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'summary'], properties: {
  folder: { type: 'string' },
  verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
  realized: { type: 'array', items: { type: 'string' } },
  deferred: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  collapsed: { type: 'string' },
  residual_ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, pkg: { type: 'string' }, slug: { type: 'string' }, mirror_slug: { type: 'string' }, claim: { type: 'string' } } } },
  summary: { type: 'string' },
} }
const RECONCILE_FIX_SCHEMA = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } },
  verdict: { type: 'string', enum: ['fixed', 'clean'] },
  pairs: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['pkg', 'slug', 'mirror_slug', 'seam_landed'], properties: { pkg: { type: 'string' }, slug: { type: 'string' }, mirror_slug: { type: 'string' }, seam_landed: { type: 'boolean' } } } },
  admitted: { type: 'array', items: { type: 'string' } },
  deferred_legs: { type: 'array', items: { type: 'string' } },
  summary: { type: 'string' },
} }
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: { overall: { type: 'boolean' }, claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } }, repaired_files: { type: 'array', items: { type: 'string' } } } }
const CLOSEOUT_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'summary'], properties: {
  folder: { type: 'string' },
  closed: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'disposition', 'strength'], properties: { slug: { type: 'string' }, disposition: { type: 'string', enum: ['complete', 'dropped'] }, strength: { type: 'string', enum: ['strong', 'partial', 'weak'] } } } },
  reopened: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo, libs/csharp planning corpus (markdown specs of intended C# package designs). CLAUDE.md manifest + WORKSPACE_LAW strata govern ' +
    '(KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; depend strictly upward; a host-neutral owner only where a non-Rhino runtime ' +
    'consumes the contract). The session targets are the APP-PLATFORM / HOST-BOUNDARY packages `Rasm.AppHost` (the host-neutral runtime spine ' +
    '`Compute`/`Persistence`/`AppUi` adapt to), `Rasm.Compute`, `Rasm.AppUi`, `Rasm.Persistence`. Each target holds `IDEAS.md` + `TASKLOG.md` + ' +
    '`ARCHITECTURE.md` + `README.md` + `<pkg>.csproj` at the package ROOT, a deep `.api/api-*.md` capability catalog, and design pages at ' +
    '`<pkg>/.planning/<subdomain>/*.md`. Read the package-root `ARCHITECTURE.md` (sub-domain map + `[02]-[SEAMS]`), `README.md` (admitted-package ' +
    'roster), and `.api/` as the governing context and capability tier for that target. Never trample a sibling package owner.',
  'MANDATORY STANDARDS — docs/stacks/csharp/ is the FLOOR, not the ceiling: every fence MUST meet docs/stacks/csharp/ (README, language, shapes, ' +
    'surfaces-and-dispatch, rails-and-effects, boundaries, algorithms, system-apis) AND the specialized docs/stacks/csharp/domain/ shard(s) ' +
    'relevant to the page concern (compute, concurrency, data-interchange, diagnostics, durability, interaction, persistence, postgres, ' +
    'resilience, runtime, transport, validation, visuals), then PUSH PAST it to the objectively strongest form the doctrine admits. READ the ' +
    'relevant shard(s) and conform exactly — a hard gate enforced by the `tools/cs-analyzer` compiled-doctrine gate (a true positive is ' +
    'architecture pressure, fix the shape; a false positive is rule pressure, never a suppression). Cite only host/NuGet members confirmed via `uv ' +
    'run python -m tools.assay api`; back bridge claims with EvidenceCertificate + reviewed ReferenceEvidence.',
  'This is IMPLEMENT, not an untied page rebuild: realize the folder SPECIFIC open IDEAS/TASKLOG cards into deep design-page FENCES. A FENCE is a ' +
    'markdown fenced code block inside a `.planning` design page — the work product itself, NEVER a `.cs`/`.py`/`.ts` source file. SCOPE per ' +
    'target: realize ALL open tasks (including `Atomic`-flagged minor tasks), then the 1-3 chosen open ideas, tasks first. Realize tied to the ' +
    'card charter (Capability/Shape/Unlocks/Anchors), mining every admitted package to full capability and crushing surface sprawl into fewer ' +
    'richer owners with zero functionality loss.',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FOLDER ' +
    'items (report those in residual_ripples). If after real investigation a fence is already correct, say so — never invent edits to look busy.',
].join('\n')
const CARD = [
  'CARD SCHEMA: open cards live in `<pkg>/IDEAS.md` (ideas — larger conceptual capability) and `<pkg>/TASKLOG.md` (tasks — concrete targeted ' +
    'work), under section `[01]-[OPEN]`; closed cards collapse under `[02]-[CLOSED]`. A card is `[ID]-[STATUS]: <thesis>` then the bullets ' +
    '`Capability:` / `Shape:` / `Unlocks:` / `Anchors:` / `Tension:` (only when a constraint shapes it) / `Ripple:` (only on a cross-folder ' +
    'counterpart) / `Atomic:` (only on a minor task). Open statuses: `ACTIVE` (in-flight), `QUEUED` (next-up), `BLOCKED` (open but ' +
    'non-actionable). Closed: `COMPLETE` (finished) or `DROPPED` (abandoned). ALWAYS read the FULL card body (every bullet) from disk — the thesis ' +
    'alone is never enough to realize the charter.',
  'RIPPLE: `Ripple: <lang>:<pkg> [SLUG]` (or `<pkg> [SLUG]`) is a BIDIRECTIONAL cross-folder link — the counterpart card in the named pkg carries ' +
    'the mirror slug, and ripples are PART of scope. Three classes: IN-SCOPE (counterpart is another session target — each target realizes its OWN ' +
    'half, the seam aligns in reconcile), OUT-OF-SCOPE C# (counterpart in a non-target libs/csharp package — reconcile realizes the 1-hop ' +
    'counterpart), CROSS-LANGUAGE / LIB-WIDE (`libs/.planning`, `libs/typescript`, `libs/python` — a deferred leg, NOT realized this C#-only run).',
  'PROBE FREELY (nothing gates probing): EVERY agent in EVERY phase may — and should — probe to verify reality at any time, for ANY card or design ' +
    'decision, not only `[BLOCKED]` ones — `uv run python -m tools.assay api resolve|query` over host DLLs / NuGet to confirm any member or ' +
    'signature; Rhino WIP (never Rhino 8) via the rhino-mcp skill or tools/rhino-bridge for live host/GH behavior; `uv run python -m tools.assay ' +
    'provision check` (+ tools/assay/README.md) for a native/scientific/database/provisioning band. A `[BLOCKED]` card is REALIZED this turn ' +
    'whenever a probe resolves its blocker OR its gating work is in scope; a blocker is genuinely legitimate ONLY when it depends on work outside ' +
    'this run.',
  'PACKAGE ADMISSION (only when a card genuinely needs a not-yet-admitted package): pin the version in the ONE central repo-root ' +
    '`Directory.Packages.props` (a SHARED file — the reconcile pass owns it; you MUST NOT edit it from a folder agent), add `<PackageReference ' +
    'Include="..."/>` WITHOUT a version to `<pkg>/<pkg>.csproj` (folder-local), add the package to the correct group in `<pkg>/README.md` ' +
    '(folder-local), and author `<pkg>/.api/api-<pkg>.md` from `uv run python -m tools.assay api` (folder-local). Never a per-folder version ' +
    'manifest; never re-pin a version outside `Directory.Packages.props`.',
  'CLOSEOUT (the closeout pass ONLY): a genuinely-complete card moves to its file `[02]-[CLOSED]` section as a collapsed one-liner ' +
    '`[ID]-[COMPLETE]: <one-line disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`); update the owning `<pkg>/ARCHITECTURE.md` ' +
    '`[02]-[SEAMS]` section ONLY when a real cross-folder seam landed. Realize/critique/redteam passes NEVER change card status.',
].join('\n')
const BARHUNT = [
  'BAR — a high-value IMPLEMENT leaves every owner capturing the FULL capability of every package it admits, every sprawl collapsed into one ' +
    'denser owner with NO capability lost, and every fence transcription-complete against the verified `.api`. The critique guards capability ' +
    'conservation, charter completeness, and density; the red-team attacks every fence for a surface that could still collapse, a thin wrapper, a ' +
    'silent functionality drop during a refactor, a missed package capability, or a framework violation, and fixes each in place.',
  'HUNT (at implement, critique, and red-team alike, from multiple facets): UNDER-CAPTURED CAPABILITY — an admitted package whose `.api` and code ' +
    'expose capability no owner exploits is a named gap, closed by deepening a fence or adding one. SURFACE SPRAWL — parallel ' +
    'types/enums/methods/near-duplicate shapes collapse into one parameterized owner in the C# collapse vocabulary ' +
    '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated case family/`Fold` algebra/frozen table) with no ' +
    'functionality removed. RAIL UNIFICATION — one entrypoint family per rail, one closed `Expected` fault family per package, total generated ' +
    '`Switch`. OPTIMIZATION — correctness first, then allocation/span/SoA layout/dispatch shape/algorithmic complexity, not only line-count. NEW ' +
    'WORK SURFACED — api gaps, stronger packages, and tasks the implementation exposes are realized or recorded the same turn (extend the ' +
    'canonical owner first, never a parallel surface).',
  'NAIVETY (two axes, both intolerable): COVERAGE — the owner models a thin slice of its concept (the obvious three fields where the domain ' +
    'carries fifteen; a two-case family for a twenty-case domain); APPROACH — enumerated hardcoded instances where a parameterized, algorithmic ' +
    'owner should generate the space (a fixed roster of styles/patterns/variants is seed DATA feeding one generator over named parameters, NEVER ' +
    'the mechanism itself). Every enumerated collapse-signal list in this prompt is a FLOOR, never the complete set: any repeated structure, ' +
    'parallel spelling, or enumerable family an algebra, table, fold, or generator can own is a collapse target you find yourself. A discovery ' +
    '`map` row in the worklist is an initial pointer, never a ceiling — re-read the pages in full and exceed it; a map never licenses a skim.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE — the 16 named laws of docs/stacks/csharp/README.md, held as fact: [FLOW] EXPRESSION_SPINE (domain logic is ' +
    'expression-shaped; dependent steps `Bind` monadically, independent ones accumulate applicatively; the carrier, never a flag, selects the ' +
    'algebra; statements survive only in measured `ref struct`/span kernels that name the exemption) + BOUNDARY_ADMISSION (raw admitted EXACTLY ' +
    'ONCE into an evidence-carrying owner; interior never re-validates or sees null/sentinel/provider shape). [SHAPE] SHAPE_BUDGET (one concept ' +
    'owns ONE type; variants are cases in one closed family) + DEEP_SURFACES + MODAL_ARITY (one entrypoint owns every modality, discriminating on ' +
    'input shape) + ANTICIPATORY_COLLAPSE (shape the owner for the family it will absorb). [DERIVATION] POLICY_VALUES + DERIVED_LOGIC + ' +
    'DERIVED_TYPES + SYMBOLIC_REFERENCE + SEMANTIC_NAMING. [MATERIAL] LIBRARY_DEPTH + DEFINITION_TIME_ASPECTS. [INTEGRATION] ROOT_REBUILD (weave ' +
    'new capability into the owner as if always present; no shims/aliases/[Obsolete]/migration layers) + ONE_HOP_RESOLUTION + ' +
    'COMPOSED_IMPLEMENTATION.',
  'ULTRA-ADVANCED COLLAPSE MANDATE: COLLAPSE >=3 parallel types / sibling factory methods / repeated switch arms / single-call private helpers ' +
    'into ONE polymorphic owner IN THE SAME FILE via `[Union]` / `[Union<T1,...>]` ad-hoc / `[SmartEnum<TKey>]` / `[SmartEnum]` keyless / ' +
    '`[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case families / `Fold` algebra / frozen data tables — never extract a new file ' +
    'to reduce LOC, never delete capability.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows raw -> admit ONCE (generated factory + validation partial admits/rejects; one rail ' +
    'bridge lifts the generated outcome into `Fin<T>` / `Validation<Error,T>`; `Option<T>` carries absence; exceptions convert at the owning ' +
    'boundary only) -> canonical owner -> unified rail -> projection -> egress. Interior code never re-validates, never sees `null`-as-failure, ' +
    'sentinels, or provider shapes; parameterize BOTH ingress AND egress so the same owner sources and sinks across many consumers without ' +
    'interior edits.',
  'ULTRA-STACK CAPABILITY: BOTH `.api` tiers are enumerated IN FULL with a real listing from disk (`ls`/`fd`, never memory) and mined to operator ' +
    'depth — the shared substrate tier `libs/csharp/.api/` (Thinktecture, MathNet, CSparse, QuikGraph, Mapperly, and the other cross-package ' +
    'catalogs) AND the folder tier `libs/csharp/<Package>/.api/` (the curated, integration-shaped domain surface) — plus the universal ' +
    'Thinktecture (generated domain shape) / LanguageExt (rails, effects, schedules, immutable collections) rails and full docs/stacks/csharp ' +
    'doctrine, with MathNet / CSparse owning numeric algorithms. There is NO fixed package count: compose EVERY relevant host API + admitted NuGet ' +
    'package + catalog member into single dense owners woven as ONE rail (source-generated owners, `Fold` algebra, data tables), ALWAYS layering ' +
    'the universal Thinktecture/LanguageExt rails onto the domain packages, NOT flat one-shot per-API uses. Use the DEEPEST ' +
    'operator/combinator/generated surface each package itself reaches (LIBRARY_DEPTH); an admitted capability the concept admits but no owner ' +
    'exploits is a DEFECT you close, and a cited member you cannot verify via `uv run python -m tools.assay api` is a PHANTOM you delete or ' +
    'correct, never leave standing; reject surface-level subsets, BCL-first reflexes, and thin rename wrappers.',
  'PRESERVE all capability (densify, never delete functionality). Where a fence is already dense, deepen; where it is flat/naive, rebuild ' +
    'ground-up. Never regress correctness or boundary/strata law.',
].join('\n')
const PATLAW = [
  'C# PATTERN LAW: model the domain precisely — NEVER weak/unbounded/erased types where the language can express the domain; NEVER exception ' +
    'control flow in domain logic (use the LanguageExt typed rails / ROP and the route recovery patterns); NEVER imperative branching where a ' +
    'bounded vocabulary, frozen table, generated `Switch`, match, or `Fold` owns the variation; NEVER mutable accumulation for domain transforms ' +
    '(use immutable folds, projections, collection combinators). Total generated `Switch` with compile-time exhaustiveness (a new case breaks ' +
    'every dispatch site — NEVER a runtime-silent `_` arm). Typed algorithm receipts (NEVER a generic `IReceipt`/ledger) when fields carry ' +
    'route/status/sampling/solver/spectral/mesh/extraction/benchmark/host evidence. The fault type is a CLOSED `[Union]` family deriving from ' +
    '`Expected` (a bare exception or a generic untyped `Error` for a multi-cause domain is a defect).',
  'Latest stable C# 14 on `net10.0` to the metal (`Nullable enable`, NRT enforced): primary constructors, collection expressions with spread, ' +
    '`params` collections (incl. `params ReadOnlySpan<T>`), list/slice/relational/logical pattern matching, switch expressions, `required` ' +
    'members, `file`-scoped types, `field` accessors, extension blocks (`extension(Receiver)`) and extension operators, generic math / static ' +
    'abstract+virtual interface members, `with` expressions, `nameof` with unbound generics, `System.Threading.Lock`, raw string + `u8` literals ' +
    'where they fit. Treat analyzer diagnostics as architecture pressure (fix true positives, refine false positives, no ceremony suppressions). ' +
    'Apply the docs/stacks/csharp file-organization and section-order law (`[Union]`/`[SmartEnum]`/`[ValueObject]` and generated case families ' +
    'stay inside the declaring owner block; canonical section order TYPES -> CONSTANTS -> MODELS -> ERRORS -> SERVICES -> OPERATIONS -> ' +
    'COMPOSITION -> EXPORTS).',
  'Keep conventions IDENTICAL across every package; place each package on its canonical stratum and depend strictly upward; geometry/mesh/IFC meet ' +
    'at the wire with one owner per runtime; never leak a host type into a host-neutral owner. SEMANTIC_NAMING: one canonical bounded-context term ' +
    'per concept (one word default, three the ceiling); arity/filter/provider/modality live in request shape, case, or policy row, never parallel ' +
    '`Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` names; ONE_HOP_RESOLUTION (no alias chains, forwarding helpers, or util shells).',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every package owner strictly in its lane and on its stratum; geometry/mesh/IFC meet at the wire with one ' +
  'owner per runtime; internal code uses canonical names and shapes with mapping only at the edge; do not trample a sibling owner while ' +
  'densifying; never introduce a downward dependency or leak a host type into a host-neutral owner. Each target realizes ONLY its OWN cards into ' +
  'its OWN pages; a concern owned twice across a runtime, a folder mixing unrelated concerns, or coupling to a sibling owner INTERIOR (vs its ' +
  'seam/wire) is a defect.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
    'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
    'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing ' +
    'contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, method, operator, package ID, path, command, flag, and literal value in backticks. Name the ' +
    'exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
  'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY ' +
  'where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a ' +
  'comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no ' +
  'task/process/session/history/proof/review comments, no XML-doc bloat. Densify names and types so comments are rarely needed; cut every ' +
  'low-value comment.'
const DOCTRINE = [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const folderName = (p) => p.split('/').filter(Boolean).pop() || p
const pkgPath = (toPkg) => { let p = String(toPkg || '').trim().replace(/^csharp:/, ''); return p.indexOf('libs/') === 0 ? p : 'libs/csharp/' + p }
const discoverPrompt = (targets) => [LAW, '', CARD, '',
  'TASK: DISCOVER + SEQUENCE + MAP the open work across these C# session targets: ' + JSON.stringify(targets) + '. DISCOVERY is the reconnaissance ' +
    'every downstream stage stands on, and read-only is its ONLY concession — full reads, never skims, never memory. FIRST enumerate from the ' +
    'source of truth with a REAL listing (`ls`/`fd`, never recall): BOTH `.api` tiers (`libs/csharp/.api/` and each target `<pkg>/.api/`), the ' +
    'doctrine inventory (`docs/stacks/csharp/` core + `domain/` shards), and each target folder at large (`.planning/**` pages, `ARCHITECTURE.md`, ' +
    '`README.md`, `<pkg>.csproj`). THEN read IN FULL: each target `IDEAS.md` + `TASKLOG.md` (every card body, every bullet — the thesis alone is ' +
    'never enough), every design page an open card names, and the folder at large (the remaining `.planning/**` pages plus `ARCHITECTURE.md` + ' +
    '`README.md`) — seams, staleness, and unexploited capability hide in pages no card names. Resolve scope against real disk state: a named ' +
    'page, pkg, or slug you cannot find on disk is recorded (map row or malformed_ripples), never assumed. For EACH target return: (1) tasks — ' +
    'EVERY open card in `TASKLOG.md` (status ' +
    'ACTIVE/QUEUED/BLOCKED; carry the Atomic flag); (2) ideas — the 1-3 MOST actionable open cards in `IDEAS.md` (tasks-first doctrine: pick at ' +
    'most 3, the ones whose Anchors are most settled and whose ripples land on in-scope targets), HARD CAP 3; (3) order — ONE sequenced slug list, ' +
    'ALL tasks first in dependency order (a card whose Anchors reference another card output comes after it), then the chosen ideas; (4) ripples — ' +
    'for EVERY card carrying a `Ripple:` field, one row {from_slug, klass, to_pkg, to_slug}: klass=`in_scope` if to_pkg is one of these targets, ' +
    '`oos_csharp` if it is another libs/csharp package, `cross_lang` if it points at `libs/.planning` / `libs/typescript` / `libs/python`; (5) ' +
    'gates — for any [BLOCKED] card, {blocked_slug, gated_by_slug, in_scope} where in_scope is true iff the gating work is itself an open card in ' +
    'one of these targets (that card becomes actionable this run once its gate lands); (6) map — one row per design page the open cards target: ' +
    '{page, composed: the capability the page already composes, underutilized: admitted capability the page leaves unexploited with CONCRETE ' +
    'members you verified in a `.api` catalog (verified members ONLY — a member you cannot verify is a phantom and is NEVER listed), seams: the ' +
    'contextual cross-page/cross-folder seams the page meets, stacking: how the catalog capability stacks into the realization, call: a hostile ' +
    'weak/strong verdict — `strong` is earned by an attack that finds nothing, never conceded on first read}. The map is real analysis against the ' +
    'verified inventory, never a guess; downstream agents treat it as an initial pointer, never a ceiling. Also return malformed_ripples for any ' +
    '`Ripple:` line you cannot parse into a pkg+slug, or whose counterpart slug you cannot locate in the named pkg. Return the structured map ' +
    'ONLY; edit nothing — read-only is DISCOVERY\'s sole concession.'].join('\n')
const implementPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: IMPLEMENT — realize the open cards of `' + folder + '` into deep design-page FENCES at the ULTRA bar. The sequenced worklist (slugs + ' +
    'ripple map + discovery capability map; read each FULL card body from `' + folder + '/IDEAS.md` + ' +
    '`' + folder + '/TASKLOG.md`, never the thesis alone):\n' + seq + '\nREAD: ' +
    'each card full body; every design page the card names under `' + folder + '/.planning/**`; the sibling pages it seams to; the package-root ' +
    '`ARCHITECTURE.md` + `README.md`; docs/stacks/csharp/ core + the relevant domain/ shard(s) for the card concern; BOTH `.api` tiers in full — ' +
    '`libs/csharp/.api/` + `' + folder + '/.api/api-*.md`, enumerated by real listing, never memory — plus the admitted packages; and verify any ' +
    'novel host/NuGet member via `uv run python -m tools.assay api`. Realize EVERY card in `order` (all ' +
    'tasks incl. Atomic, then the ideas) into deep fences IN `' + folder + '` PAGES ONLY, in LIFECYCLE order (admit raw ONCE through a generated ' +
    'factory + validation partial -> lift into the canonical owner the OWNER_CHOOSER discriminants select -> weave every cross-cutting concern as ' +
    'a definition-time source-generated aspect or composition-time effect transformer over a thin pure core -> compose through ONE unified ' +
    '`Fin`/`Validation`/`Option`/`Eff` rail with total generated `Switch` -> project + egress, BOTH ingress and egress parameterized). Collapse ' +
    'parallel shapes into one `[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated case family in the SAME ' +
    'file; drive cases with a `Fold` algebra or a frozen table; one polymorphic entrypoint per modality. Resolve any [BLOCKED] card inline (probe ' +
    'via `assay api` / Forge band / Rhino WIP). PACKAGE ADMISSION (only if a card needs a not-yet-admitted package): do the FOLDER-LOCAL parts NOW ' +
    '— add `<PackageReference Include="..."/>` (no version) to `' + folder + '/' + folderName(folder) + '.csproj`, add the package to the correct ' +
    'group in `' + folder + '/README.md`, and author `' + folder + '/.api/api-<pkg>.md` from `assay api` — and LOG the central ' +
    '`Directory.Packages.props` version pin as a residual_ripple with files including `Directory.Packages.props` (a single reconcile agent owns ' +
    'that shared file; you MUST NOT edit it). RIPPLES: realize ONLY `' + folder + '`\'s OWN half of every seam; NEVER edit another folder page. ' +
    'For each ripple your cards carry, log a residual_ripple {files:[your_page, counterpart_page], pkg, slug, mirror_slug, claim} stating the ' +
    'contract your half exposes (reconcile aligns both halves). Do NOT close any card — the closeout pass owns card status. Modern C# 14 / net10 ' +
    'to the metal, high-signal prose all-backticked, comment hygiene, fix-in-place (read-then-extend, preserve capability). Return verdict + ' +
    'realized slugs + deferred (any card you could not realize, with reason) + collapsed (before->after counts) + residual_ripples + summary.'].join('\n')
const critiquePrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: DOCTRINAL-CONFORMANCE AUDIT + CHARTER-COMPLETENESS + FIX IN PLACE across `' + folder + '`. You are an ULTRA-HARSH, UNAGREEABLE auditor: ' +
    'assume a violation exists in every fence until you prove otherwise, and "good enough" is rejected. The cards realized this turn (read each ' +
    'FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`):\n' + seq + '\nREAD the realized pages, the sibling pages, ' +
    'docs/stacks/csharp/ core + the relevant domain/ shard(s), and BOTH `.api` tiers by real listing — `libs/csharp/.api/` substrate + ' +
    '`' + folder + '/.api/` — plus the universal Thinktecture/LanguageExt rails. Run these MECHANICAL checklists line-by-line as a FLOOR you hunt ' +
    'past, never the complete audit, and REPAIR every hit in place (a fix, never a ledger note):',
  '(1) COLLAPSE_SCAN — apply the move for any signal (3+ instances makes it mandatory): sibling prefix/suffix names -> one modality-polymorphic ' +
    'entrypoint; same return rail differing only by arity -> input-shape discrimination; functions differing only by a literal -> parameterize the ' +
    'literal as a POLICY_VALUE; a `bool`/`mode`/`batch` parameter selecting two bodies -> one derived body or policy value; a method calling ' +
    'exactly one other -> delete the hop (ONE_HOP_RESOLUTION); parallel dispatch arms repeating structure -> a `Fold` algebra or frozen table ' +
    '(DERIVED_LOGIC); several types sharing fields for one concept -> one closed family; a `Get`/`GetMany`/`GetBy<Key>`/`List`/`Search` family -> ' +
    'one input-keyed polymorphic operation; a wrapper renaming a package API -> use the package surface directly (LIBRARY_DEPTH); 3+ parallel ' +
    'types / sibling factories / repeated switch arms / single-call helpers -> ONE `[Union]` / `[SmartEnum<TKey>]` / `[ValueObject<T>]` / ' +
    '`[ComplexValueObject]` / source-generated case family IN THE SAME FILE. These signals are a FLOOR, never the complete set — hunt past them.',
  '(2) OWNER_CHOOSER — for EVERY shape re-derive the owner from the 5 discriminants (admission, identity regime, variant arity, payload timing, ' +
    'openness), most-specific wins: invariant-bearing scalar -> `[ValueObject<TKey>]`; N-field one-concept product no discriminator -> ' +
    '`[ComplexValueObject]`; bounded vocabulary wire-keyed identity -> `[SmartEnum<TKey>]`; bounded vocabulary process-local behavior -> ' +
    '`[SmartEnum]` keyless; closed alternatives per-occurrence payload -> `[Union]`; one value over 2-5 unrelated types -> `[Union<T1,...>]` ' +
    'ad-hoc; interior product no invariant -> `record`/`readonly record struct`; combinable capability set -> a frozen set; cross-product/external ' +
    'policy key -> a frozen table; foreign wire enum / ABI bits / kernel ordinal -> a language `enum` AT THE SEAM ONLY. Kill every parallel DTO, ' +
    'one-field wrapper, field-rename shape, nullable-as-failure, and struct-`default` ghost.',
  '(3) KNOB_TEST — removal: delete each parameter; if the value reconstructs what it carried it was a knob -> collapse a ' +
    '`bool`/`mode`/`strict`/`batch` flag into a policy value or input-shape discriminant; a nullable flag tail -> one `Option<ContextRecord>`; the ' +
    'single optional form is `Option<T> x = default` consumed via `IfNone(canonical)`; move every `timeout`/`retry`/`deadline`/`CancellationToken` ' +
    'OFF the signature onto the carrier or a composition-time effect aspect.',
  '(4) ASPECTS — definition-time concerns (admission, identity, dispatch, serialization, grammar, logging) attach via attribute-directed SOURCE ' +
    'GENERATION in the fixed generator order; composition-time concerns attach as effect transformers in author order — retry as `Schedule`-driven ' +
    '`IO<T>.Retry(Schedule)`/`Prelude.retry`, recovery as named catch combinators (`@catch`/`catchOf`/`CatchM` composed via `|`), resource ' +
    'lifetime as `Bracket`/`BracketIO`/`Finally`; the two weaves meet at EXACTLY ONE seam, the admission rail bridge. 2-4 co-occurring wrappers ' +
    'collapse into one aspect; an aspect NEVER raises into domain flow; deterministic stacking order verified. Inline-repeated concerns and ' +
    'sibling helper methods are defects.',
  '(5) RAILS — RAIL_CHOOSER, the narrowest carrier chosen ONCE at admission: `Option<T>` absence, `Fin<T>` synchronous fallibility, ' +
    '`Validation<Error,T>` independent accumulated faults, `Eff<RT,T>` runtime capability, `IO<T>` deferred boundary work, `Schedule` retry ' +
    'policy, `Seq<T>`/`Arr<T>`/`HashMap<K,V>` immutable traversal/lookup; the fault type is a CLOSED `[Union]` family deriving from `Expected` (a ' +
    'bare exception or generic untyped `Error` for a multi-cause domain is a defect; recovery identity via `Is`/`HasCode`/`IsType<E>`, never ' +
    '`==`); accumulate-vs-abort disposition correct (`Apply`/`&`/`.Traverse` for independents, `Bind`/`.TraverseM`/query for dependents); total ' +
    'generated `Switch` (NO `_` arm hiding a case); `.Fold`/`.Traverse`/`.Choose` with the mandatory `.As()` re-anchor; NO exception control flow ' +
    'in domain logic, NO mutable accumulation.',
  '(6) STRATA/MEMBERS/MODERN — strata correctness (depend strictly upward; NO downward dependency, NO host-type leak into a host-neutral owner; ' +
    'geometry/mesh/IFC meet at the wire with one owner per runtime); cite ONLY host/NuGet members confirmed in a `.api` catalog (verify novel ' +
    'members via `uv run python -m tools.assay api`; an unverifiable cited member is a PHANTOM — delete or correct it); latest modern C# 14 on ' +
    'net10 (primary ctors, collection expressions, `params` collections, list/relational/logical patterns, switch expressions, `required` ' +
    'members, `file` types, `field` accessors, extension blocks, generic math, static abstract members); FULL docs/stacks/csharp + the relevant ' +
    'domain/ shard conformance; BOTH `.api` tiers (`libs/csharp/.api/` substrate + the folder catalogs) AND the universal Thinktecture/LanguageExt ' +
    'rails maximized to operator depth; the `tools/cs-analyzer` doctrine-gate clean.',
  '(7) CHARTER-COMPLETENESS — for EVERY card in the worklist, verify the realized fences GENUINELY fulfill its `Capability`/`Shape`/`Unlocks` ' +
    '(read the full card from disk): a missing modality, an unrealized `Shape` clause, a stubbed/placeholder fence, or a capability the card ' +
    'promises but the fences do not deliver is a DEFECT — realize it NOW. A card whose fences are thin against its charter is not done. Attack ' +
    'BOTH naivety axes per card: COVERAGE — fences modeling a thin slice of the domain the charter names — widen to the full concept; APPROACH — ' +
    'an enumerated roster of hardcoded instances where ONE parameterized generator should own the space — demote the roster to seed data feeding ' +
    'that generator.',
  'Also enforce the docs/stacks/csharp file-organization + section-order law, cross-package convention consistency, and prose + comment hygiene. ' +
    'EDIT the `' + folder + '` pages to fix every hit; realize ONLY `' + folder + '` pages and OVERRIDE any earlier residual you can now resolve; ' +
    'log any genuine cross-FOLDER item as a residual_ripple {files, pkg, slug, mirror_slug, claim}. Return verdict + realized + deferred + ' +
    'collapsed + residual_ripples + summary.'].join('\n')
const redteamPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: ADVERSARIAL ARCHITECT RED-TEAM + FIX IN PLACE across `' + folder + '`. You are the LAST and MOST AGGRESSIVE pass: assume the author and ' +
    'critique missed things and that the chosen design is not the strongest until proven, with the burden of proof ON THE DESIGN. The cards ' +
    'realized this turn (read each FULL body):\n' + seq + '\nREAD BOTH `.api` tiers by real listing — `libs/csharp/.api/` substrate + ' +
    '`' + folder + '/.api/` — plus the universal Thinktecture/LanguageExt rails, the sibling pages, docs/stacks/csharp/ + the relevant domain/ ' +
    'shard. Attack from every direction and REPAIR every defect in place — no soft-pedalling, no could/should, a fix never a ledger.',
  'PRIMARY LENS — fundamental design, multi-faceted: (A) COUNTERFACTUAL on the core choice — is the owner, the algebra (`Fold`/generated ' +
    '`Switch`/data table), and the dispatch form categorically the strongest the doctrine admits, or does a denser owner ' +
    '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject<T>]`/`[ComplexValueObject]`/source-generated family), a data table, or a DEEPER admitted-package ' +
    'primitive (LanguageExt/Thinktecture/MathNet/CSparse) collapse the whole fence? If a fundamentally stronger design exists, rebuild to it — ' +
    'never defend the incumbent; an enumerated roster of hardcoded variants where a parameterized generator should own the space is an APPROACH ' +
    'defect — demote the roster to seed data feeding ONE generator. (B) ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: when the next ' +
    'case/dimension/knob/modality/provider arrives, does it land as ONE case/row/policy value with every consumer untouched or broken LOUDLY at ' +
    'compile time (total generated `Switch`, no silent `_`)? If it would touch multiple sites, reshape so the growth axis is a case, row, policy ' +
    'value, or carrier swap. (C) LONG-TAIL + DOMAIN COMPLETENESS — attack every input/output/edge/failure mode (empty, singular, plural, stream, ' +
    'malformed, concurrent, cancelled, partial-failure, version-skew); COVERAGE naivety — an owner modeling the obvious thin slice of its concept ' +
    'where the domain carries far more — is a defect: widen to the full concept; is the accumulate-vs-abort disposition correct for the REAL boundary; ' +
    'are BOTH ingress AND egress parameterized so this owner sources and sinks across hundreds of consumers without interior edits? (D) STRATA + ' +
    'BOUNDARY-INTEGRITY — a downward dependency, a host-type leak into a host-neutral owner, a concern owned twice in a runtime, a folder mixing ' +
    'concerns, geometry/mesh/IFC not meeting at ONE wire owner per runtime, coupling to a sibling owner INTERIOR (vs its seam/wire), OR a sibling ' +
    'planning page left STALE by this folder change even when no ripple card names it (ports/boundaries/wires/seams drift) is a defect: fix it ' +
    'within `' + folder + '`, or record it as a residual_ripple. (E) SURFACE-SPRAWL-IN-TIME — an admitted package whose `.api` or the universal ' +
    'rails expose capability the fence re-derives by hand, flat code below the operator depth the packages reach, a phantom `.api`/host member, or ' +
    'a thin wrapper: collapse to package depth, verify every cited member (via `assay api`), and DELETE or correct every phantom.',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (every time, NOT only on a structural restructure): re-attack every conformance dimension with fresh ' +
    'hostile eyes, trusting nothing the prior passes claimed — the COLLAPSE_SCAN signals, OWNER_CHOOSER per shape, the KNOB_TEST per param, the ' +
    'two-weave ASPECT taxonomy, rail + closed-`Expected`-fault discipline, charter-completeness per card, strata correctness, modern-C# 14 typing, ' +
    'docs/stacks/csharp + domain-shard conformance, `.api` + Thinktecture/LanguageExt maximization, the `tools/cs-analyzer` doctrine-gate, and ' +
    'prose/comment hygiene — and fix every defect. Even absent a structural rebuild, the fences must end objectively denser, more correct, and ' +
    'more powerful than the critique left them; if the strongest form is genuinely already present, prove it by finding nothing — never invent ' +
    'churn. Realize ONLY `' + folder + '` pages; log cross-FOLDER items as residual_ripples. Return verdict + realized + deferred + collapsed + ' +
    'residual_ripples + summary.'].join('\n')
const reconcileFixPrompt = (cl) => [DOCTRINE, '',
  'TASK: RECONCILE this cluster of cross-FOLDER residuals the per-folder passes deferred. There is NO severity — treat EVERY residual as ' +
    'must-address. Read EVERY listed file. Handle each residual by KIND: (a) IN-SCOPE SEAM (both halves already realized by their own target ' +
    'folders) — read both pages, ALIGN them to ONE shared contract, fix any mismatch, set `seam_landed` true; (b) OUT-OF-SCOPE C# COUNTERPART (the ' +
    'counterpart card lives in a non-target libs/csharp package) — realize that ONE counterpart card fence (its half only, NEVER the folder other ' +
    'cards) at the same ULTRA bar and align the seam; (c) CENTRAL PIN — apply every `Directory.Packages.props` version pin in this cluster (you ' +
    'are the ONLY agent that edits that shared file; apply them all serially, keeping alphabetical/group order) and list them in `admitted`; (d) ' +
    'CROSS-LANGUAGE / LIB-WIDE LEG — record it in `deferred_legs` and do NOT realize it (out of this C#-only run). Preserve all capability, ' +
    'regress no file, never trample a sibling owner interior. For every ripple counterpart you touch, emit a `pairs` row {pkg, slug, mirror_slug, ' +
    'seam_landed}. If a residual is FACTUALLY INCORRECT or not a real defect, leave it and say why in the summary — never silently skip a real ' +
    'one. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerifyPrompt = (cl, fix) => [DOCTRINE, '',
  'TASK: ADVERSARIAL WRITING VERIFY of the reconcile fixes — never a friendly confirmation, never read-only. Hold every fixer claim naive or ' +
    'illusory until it survives your attack. For EVERY claim: (1) RE-DERIVE necessity — was the claimed fix needed at all, and is the claim itself ' +
    'factually right? (2) PROVE ON DISK — read every named file IN FULL and prove the fix genuinely landed (seam aligned on BOTH pages to ONE ' +
    'shared contract / counterpart fence realized at the ULTRA bar / central pin applied), never trusting the fixer report. (3) REPAIR TO THE ROOT ' +
    '— a loose, weak, or token fix is a defect YOU repair NOW via Edit/Write in the same files: a single-point patch where a root-level dense ' +
    'reconstruction of the same files is available is itself a defect this verify fixes; leave the files objectively stronger, never merely ' +
    'confirmed (a `Directory.Packages.props` repair is in scope ONLY when this cluster\'s claims name that file). THEN classify each claim: status ' +
    '"fixed" (genuinely resolved on disk — including because YOU repaired it; cite the evidence), "invalid" (the claim is factually wrong / not a ' +
    'real defect — cite the disproof), "open" (a real defect genuinely unreachable from the files at hand — an out-of-run dependency — NEVER a ' +
    'punt on a fix you could strengthen yourself). Default a real-looking defect you cannot repair to "open"; mark "invalid" ONLY when you can ' +
    'show the claim is wrong. List every file you edited in repaired_files. Claims:\n' + JSON.stringify(cl, null, 1) +
    '\nFiles the fixer touched: ' + JSON.stringify(fix.files) + '\nPairs the fixer reported: ' + JSON.stringify(fix.pairs || [])].join('\n')
const closeoutPrompt = (folder, seamJson) => [DOCTRINE, '',
  'TASK: TRUTHFUL CLOSEOUT + FINAL REMEDIATION of `' + folder + '`. This is the SOLE owner of card status. For EVERY card that was in scope this ' +
    'run, read its FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md` and the realized fences under `' + folder + '/.planning/**`, ' +
    'then ADVERSARIALLY VERIFY — the fences are naive until they survive your attack, a prior pass verdict a rejected self-assessment — that the ' +
    'fences genuinely fulfill the card `Capability`/`Shape`/`Unlocks` against the cited `.api` (verify novel members via ' +
    '`uv run python -m tools.assay api`). If a card is WEAK or PARTIAL, make a FINAL in-place REMEDIATION NOW (it already passed ' +
    'implement->critique->redteam this turn; deepen the fences to genuinely complete the charter), then re-verify. Assign each card a strength: ' +
    '`strong` (every charter clause delivered, fences transcription-complete against the verified `.api`), `partial` (most delivered, a clause ' +
    'still thin), `weak` (charter not met). CLOSE only genuinely-complete cards: move them to the `[02]-[CLOSED]` section of their owning file as ' +
    'a collapsed one-liner `[ID]-[COMPLETE]: <disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`), and update `' + folder + '/ARCHITECTURE.md` ' +
    '`[02]-[SEAMS]` ONLY when a real cross-folder seam landed. RIPPLE PAIRS: a card carrying a `Ripple:` closes COMPLETE only if its seam landed — ' +
    'this map gives seam_landed per slug (`false` = seam did NOT land, keep the card OPEN; `true`/absent = judge on your own half): ' + seamJson + '. ' +
    'For a shared `[02]-[SEAMS]` entry, only the owning-stratum folder writes it (`Rasm.Persistence` owns durable-store seams; `Rasm.AppHost` owns ' +
    'host-neutral contract seams). Honestly RE-OPEN any card you cannot bring to `strong` this run, with a one-line reason (a real out-of-run ' +
    'dependency). The orchestrator will DEMOTE any card you mark complete whose strength is not `strong`, so never inflate. Return closed [{slug, ' +
    'disposition, strength}] + reopened [{slug, reason}] + summary.'].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

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

phase('Discover')
const disc = (await agent(discoverPrompt(TARGETS), { label: 'discover', phase: 'Discover', schema: DISCOVERY_SCHEMA, effort: 'medium', stallMs: 300000 })) || { targets: [], malformed_ripples: [] }
const withCards = (disc.targets || []).filter((t) => (t.tasks && t.tasks.length) || (t.ideas && t.ideas.length))
log('Discover: ' + TARGETS.length + ' targets; ' + withCards.length + ' with open cards; pooling at CAP=' + CAP)

// --- [REALIZE]
phase('Realize')
const realizeFolder = async (t) => {
  const seq = JSON.stringify({ order: t.order, tasks: t.tasks, ideas: t.ideas, ripples: t.ripples, gates: t.gates || [], map: t.map || [] }, null, 1)
  const tag = folderName(t.folder)
  const impl = await agent(implementPrompt(t.folder, seq), { label: 'implement:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  if (!impl) return { folder: t.folder, failed: true, logs: [] }
  const crit = await agent(critiquePrompt(t.folder, seq), { label: 'critique:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  const red = await agent(redteamPrompt(t.folder, seq), { label: 'redteam:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'max', stallMs: 300000 })
  return { folder: t.folder, failed: false, logs: [impl, crit, red].filter(Boolean) }
}
const realized = (await pool(withCards, CAP, realizeFolder)).filter(Boolean)
const failedFolders = new Set(realized.filter((r) => r.failed).map((r) => r.folder))
const deferredCards = realized.flatMap((r) => r.logs.flatMap((l) => (l.deferred || []).map((d) => ({ folder: r.folder, slug: d.slug, reason: d.reason }))))

const crossLang = []
for (const t of (disc.targets || [])) for (const rp of (t.ripples || [])) if (rp.klass === 'cross_lang') crossLang.push(t.folder + ' [' + rp.from_slug + '] ' +
  '-> ' + rp.to_pkg + ' [' + rp.to_slug + ']')
const allRes = []
for (const r of realized) if (!r.failed) for (const l of r.logs) for (const x of (l.residual_ripples || [])) allRes.push({ files: (x.files && x.files.length ? x.files : [r.folder]), pkg: x.pkg, slug: x.slug, mirror_slug: x.mirror_slug, claim: x.claim || '' })
for (const t of (disc.targets || [])) for (const rp of (t.ripples || [])) if (rp.klass !== 'cross_lang') allRes.push({ files: [t.folder, pkgPath(rp.to_pkg)], pkg: rp.to_pkg, slug: rp.from_slug, mirror_slug: rp.to_slug, claim: 'ripple ' +
  '' + rp.klass + ': ' + folderName(t.folder) + ' [' + rp.from_slug + '] <-> ' + rp.to_pkg + ' [' + rp.to_slug + ']' })
const uniq = [...new Map(allRes.map((r) => [r.files.slice().sort().join(',') + '|' + r.claim, r])).values()]
const clusters = (() => {
  const parent = new Map(); const find = (f) => { let p = f; while (parent.get(p) !== p) p = parent.get(p); return p }; const add = (f) => { if (!parent.has(f)) parent.set(f, f) }
  for (const r of uniq) { r.files.forEach(add); for (let i = 1; i < r.files.length; i++) parent.set(find(r.files[i]), find(r.files[0])) }
  const by = new Map()
  for (const r of uniq) { const root = r.files.length ? find(r.files[0]) : '__none__'; (by.get(root) || by.set(root, []).get(root)).push(r) }
  return [...by.values()]
})()
// Heaviest cluster first: a fixer's load is dominated by distinct files read + reconciled; under CAP the long pole must never launch last.
const clusterWork = (c) => { const files = new Set(); for (const r of c) for (const f of r.files) files.add(f); return files.size * 2 + c.length }
clusters.sort((a, b) => clusterWork(b) - clusterWork(a) || (a[0].claim || '').localeCompare(b[0].claim || ''))
log('Realize: ' + realized.filter((r) => !r.failed).length + '/' + withCards.length + ' folders; reconcile ' + uniq.length + ' residuals -> ' + clusters.length + ' ' +
  'clusters; work [' + clusters.map(clusterWork).join(', ') + '] (2*files+claims)' + (failedFolders.size ? '; ' + failedFolders.size + ' folder(s) failed' : ''))
let reconciled = []
if (clusters.length) {
  phase('Reconcile')
  reconciled = (await pool(clusters, CAP, async (cl, i) => {
    const fix = await agent(reconcileFixPrompt(cl), { label: 'reconcile-fix:' + i, phase: 'Reconcile', schema: RECONCILE_FIX_SCHEMA, effort: 'max', stallMs: 300000 })
    if (!fix) return null
    const verify = await agent(reconcileVerifyPrompt(cl, fix), { label: 'reconcile-verify:' + i, phase: 'Reconcile', schema: RECONCILE_VERIFY_SCHEMA, effort: 'xhigh', stallMs: 300000 })
    return { cluster: cl, fix, verify }
  })).filter(Boolean)
}
const seamLanded = {}
for (const r of reconciled) for (const p of ((r.fix && r.fix.pairs) || [])) { seamLanded[p.slug] = p.seam_landed; seamLanded[p.mirror_slug] = p.seam_landed }
const oosFolders = new Set()
for (const r of reconciled) for (const p of ((r.fix && r.fix.pairs) || [])) { const fp = pkgPath(p.pkg); if (!TARGET_SET.has(fp) && fp.indexOf('libs/csharp/') === 0) oosFolders.add(fp) }
const claimsAll = reconciled.flatMap((r) => (r.verify && r.verify.claims) || [])
const hard_open = claimsAll.filter((c) => c.status === 'open').map((c) => c.claim)
const dropped_invalid = claimsAll.filter((c) => c.status === 'invalid').map((c) => c.claim)
const admitted = [...new Set(reconciled.flatMap((r) => (r.fix && r.fix.admitted) || []))]
const deferred_legs = [...new Set([...crossLang, ...reconciled.flatMap((r) => (r.fix && r.fix.deferred_legs) || [])])]

// --- [CLOSEOUT]
phase('Closeout')
const closeoutFolders = [...withCards.map((t) => t.folder).filter((f) => !failedFolders.has(f)), ...oosFolders]
const seamJson = JSON.stringify(seamLanded, null, 1)
const closeouts = (await pool(closeoutFolders, CAP, (f) => agent(closeoutPrompt(f, seamJson), { label: 'closeout:' + folderName(f), phase: 'Closeout', schema: CLOSEOUT_SCHEMA, effort: 'xhigh', stallMs: 300000 }))).filter(Boolean)
let closed_count = 0
const reopened = []
for (const c of closeouts) {
  for (const cc of (c.closed || [])) {
    if (cc.disposition === 'complete' && cc.strength !== 'strong') reopened.push({ folder: c.folder, slug: cc.slug, reason: 'demoted: strength=' + cc.strength })
    else closed_count++
  }
  for (const ro of (c.reopened || [])) reopened.push({ folder: c.folder, slug: ro.slug, reason: ro.reason })
}
log('Closeout: ' + closed_count + ' cards closed, ' + reopened.length + ' re-open/demoted across ' + closeoutFolders.length + ' folders')

return {
  targets: TARGETS,
  realized_folders: realized.filter((r) => !r.failed).map((r) => r.folder),
  realize_failed: [...failedFolders],
  deferred: deferredCards,
  reconciled_clusters: clusters.length,
  seams_landed: Object.keys(seamLanded).filter((k) => seamLanded[k]).length,
  oos_counterparts_realized: [...oosFolders],
  admitted,
  closed_count,
  reopened,
  hard_open,
  dropped_invalid,
  cross_language_legs: deferred_legs,
  malformed_ripples: disc.malformed_ripples || [],
}
