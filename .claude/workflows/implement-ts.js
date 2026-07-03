export const meta = {
  name: 'implement-ts',
  whenToUse: 'Realize open cards into design-page code fences across the TypeScript target folders.',
  description: 'Realize every open IDEAS/TASKLOG card across the TypeScript target set (libs/typescript/interchange, platform, projection, services, ui) into deep design-page code FENCES at the docs/stacks/typescript + coding-ts bar (Effect-TS rails, Schema-first boundaries, branded types, exhaustive discriminated unions, zero any/throw/enum), resolve all ripples, and truthfully close the cards. Per FOLDER, not per page: one discovery agent maps cards + ripple classes + blockers; each target folder is realized as ONE implement -> critique -> redteam cycle (all WRITE, both reviews adversarial, fix-in-place; BLOCKED probe + folder-local package admission inline, no prep phase); a bounded reconcile aligns in-scope seams, realizes 1-hop same-language counterparts, and applies the single central package.json pin serially; a final per-folder closeout verify-remediate-and-closes complete cards. Card-driven (it implements ideas/tasks), NOT the in-isolation api-stacking of rebuild-typescript. TS is the weakest lib: discard naive idioms wholesale. Disposable, TypeScript-only. args = a target path string, an array of paths, or empty for the five defaults. The language-wide libs/typescript/.planning is out of scope.',
  phases: [
    { title: 'Discover', detail: 'one agent: resolve scope against a real disk listing, full-read each target IDEAS/TASKLOG (cards only; design pages are downstream scope); extract open cards (all tasks incl atomic + 1-3 ideas), sequence each folder, classify every ripple (in_scope / oos_samelang / cross_lang) with every counterpart confirmed on disk, record in-scope gates and malformed ripples' },
    { title: 'Realize', detail: 'per target folder, pooled at CAP: implement(max) -> critique(max, adversarial + charter-completeness) -> redteam(max, adversarial + staleness lens); all WRITE, fix-in-place, own-pages-only, cross-folder seams logged as residuals' },
    { title: 'Reconcile', detail: 'bounded single pass: cluster cross-folder residuals by shared file -> fix(max: align in-scope seam / realize 1-hop same-language counterpart / apply central package.json pin / defer cross-lang) -> adversarial WRITING verify(xhigh: proves fixes on disk, repairs weak fixes to root, then classifies)' },
    { title: 'Closeout', detail: 'per folder: verify each card vs full charter, FINAL-remediate weak cards in place, close genuinely-complete (move to [02]-[CLOSED], collapse, update ARCHITECTURE [02]-[SEAMS]), honestly re-open the rest; strength-demotion makes closed mechanically truthful' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------
const CAP = 10
const STAGGER_MS = 1500
const ROOT = 'libs/typescript'
const SHARED_API = 'libs/typescript/.api'
const CENTRAL = 'package.json'
const DEFAULT_TARGETS = ['libs/typescript/interchange', 'libs/typescript/platform', 'libs/typescript/projection', 'libs/typescript/services', 'libs/typescript/ui']

// --- [INPUTS] ----------------------------------------------------------------------------
const norm = (t) => { const s = String(t).trim(); return s.indexOf('libs/') === 0 ? s : ROOT + '/' + s }
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
    ripples: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['from_slug', 'klass', 'to_pkg', 'to_slug'], properties: { from_slug: { type: 'string' }, klass: { type: 'string', enum: ['in_scope', 'oos_samelang', 'cross_lang'] }, to_pkg: { type: 'string' }, to_slug: { type: 'string' } } } },
    gates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['blocked_slug', 'gated_by_slug', 'in_scope'], properties: { blocked_slug: { type: 'string' }, gated_by_slug: { type: 'string' }, in_scope: { type: 'boolean' } } } },
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
const RECONCILE_VERIFY_SCHEMA = { type: 'object', additionalProperties: false, required: ['overall', 'claims'], properties: {
  overall: { type: 'boolean' },
  repaired_files: { type: 'array', items: { type: 'string' } },
  claims: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['claim', 'status'], properties: { claim: { type: 'string' }, status: { type: 'string', enum: ['fixed', 'invalid', 'open'] }, evidence: { type: 'string' } } } },
} }
const CLOSEOUT_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'summary'], properties: {
  folder: { type: 'string' },
  closed: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'disposition', 'strength'], properties: { slug: { type: 'string' }, disposition: { type: 'string', enum: ['complete', 'dropped'] }, strength: { type: 'string', enum: ['strong', 'partial', 'weak'] } } } },
  reopened: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }

// --- [DOCTRINE] --------------------------------------------------------------------------
const LAW = [
  'Rasm monorepo, libs/typescript planning corpus (markdown specs of intended TypeScript module designs). The current TypeScript code quality is ' +
    'POOR; this is a TRUE modernization to ultra-advanced TS, not a polish pass — discard naive idioms wholesale. CLAUDE.md manifest + ' +
    'WORKSPACE_LAW strata govern. The session targets are the libs/typescript area folders `interchange`, `platform`, `projection`, `services`, ' +
    '`ui`. Each holds `IDEAS.md` + `TASKLOG.md` + `ARCHITECTURE.md` + `README.md` at its area ROOT, design pages at ' +
    '`<area>/.planning/<subdomain>/*.md`, and an area-specific `.api/*.md` catalog. The language-wide `libs/typescript/.planning` is OUT of scope ' +
    'this run. Read the area-root `ARCHITECTURE.md` (sub-domain map + `[02]-[SEAMS]`) and `README.md` (admitted-package roster) as governing ' +
    'context; never trample a sibling area owner.',
  'STANDARD: docs/stacks/typescript/ is the route-owned law (README, language, shapes, surfaces-and-dispatch, rails-and-effects, boundaries, ' +
    'system-apis) plus the coding-ts standard — author TypeScript as dense, type-safe, and rich as that bar admits; docs/stacks/csharp/ is the ' +
    'density/ambition FLOOR. READ the operative docs/stacks/typescript pages + coding-ts and conform exactly. Cite ONLY real members of admitted ' +
    'packages, cross-checked against the published types in node_modules; verify a member via `uv run python -m tools.assay api` over the ' +
    'node_modules declarations.',
  'This is IMPLEMENT, not the in-isolation api-stacking rebuild: realize the area SPECIFIC open IDEAS/TASKLOG cards into deep design-page FENCES. ' +
    'A FENCE is a markdown fenced code block inside a `.planning` design page — the work product itself, NEVER a `.ts`/`.tsx` source file. SCOPE ' +
    'per target: realize ALL open tasks (including `Atomic`-flagged minor tasks), then the 1-3 chosen open ideas, tasks first. Realize tied to the ' +
    'card charter (Capability/Shape/Unlocks/Anchors), composing the right admitted capability and crushing surface sprawl into fewer richer owners ' +
    'with zero functionality loss.',
  'TWO-TIER .api: every fence draws on BOTH the shared/universal catalogs at `libs/typescript/.api/*.md` (effect, effect-platform, ' +
    'effect-opentelemetry, effect-atom, react, react-dom, clsx, isomorphic-dompurify, otplib) AND the area-specific catalogs at ' +
    '`<area>/.api/*.md`. The shared Effect/Schema/React tier is SHARED capability you MUST consider and compose to realize the card properly — ' +
    'never hand-roll `Promise`/`try`-`catch` glue or settle for a thin area-only subset; layer the shared Effect ecosystem ' +
    '(`Effect`/`Layer`/`Context`/`Schema`/`Stream`) end-to-end ON TOP OF the area-specific packages. This is implement (use the capability the ' +
    'card needs), not rebuild (max-stack every catalog for its own sake).',
  'WRITE-FULLY MANDATE: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge; leave nothing behind except genuine cross-FOLDER ' +
    'items (report those in residual_ripples). If after real investigation a fence is already correct, say so — never invent edits to look busy.',
].join('\n')
const CARD = [
  'CARD SCHEMA: open cards live in the target `IDEAS.md` (ideas — larger conceptual capability) and `TASKLOG.md` (tasks — concrete targeted work), ' +
    'under section `[01]-[OPEN]`; closed cards collapse under `[02]-[CLOSED]`. A card is `[ID]-[STATUS]: <thesis>` then the bullets `Capability:` ' +
    '/ `Shape:` / `Unlocks:` / `Anchors:` / `Tension:` (only when a constraint shapes it) / `Ripple:` (only on a cross-folder counterpart) / ' +
    '`Atomic:` (only on a minor task). Open statuses: `ACTIVE` / `QUEUED` / `BLOCKED`. Closed: `COMPLETE` / `DROPPED`. ALWAYS read the FULL card ' +
    'body (every bullet) from disk — the thesis alone is never enough to realize the charter.',
  'RIPPLE: `Ripple: <lang>:<pkg> [SLUG]` (or `<pkg> [SLUG]`) is a BIDIRECTIONAL cross-folder link — the counterpart card in the named pkg carries ' +
    'the mirror slug, and ripples are PART of scope. Three classes: IN-SCOPE (counterpart is another TypeScript session target — each realizes its ' +
    'OWN half, the seam aligns in reconcile), OUT-OF-SCOPE SAME-LANGUAGE (counterpart in a non-target libs/typescript area — reconcile realizes ' +
    'the 1-hop counterpart), CROSS-LANGUAGE / LIB-WIDE (`libs/csharp`, `libs/python`, `libs/typescript/.planning`, `libs/.planning` — a deferred ' +
    'leg whose counterpart the other-language workflow realizes, NOT realized this TypeScript-only run).',
  'PROBE FREELY (nothing gates probing): EVERY agent in EVERY phase may — and should — probe to verify reality at any time, for ANY card or design ' +
    'decision, not only `[BLOCKED]` ones — read the published types in `node_modules` and/or `uv run python -m tools.assay api` over the ' +
    'node_modules declarations to confirm any member or type; Rhino WIP (never Rhino 8) via the rhino-mcp skill or tools/rhino-bridge if a live ' +
    'host fact is needed. A `[BLOCKED]` card is REALIZED this turn whenever a probe resolves its blocker OR its gating work is in scope (a ' +
    'sibling-area contract resolves at the seam); a blocker is genuinely legitimate ONLY when it depends on work outside this run.',
  'PACKAGE ADMISSION (only when a card genuinely needs a not-yet-admitted package): add the dependency + version to the ONE central repo-root ' +
    '`package.json` (and the `pnpm-workspace.yaml` catalog if the version is catalog-managed) — a SHARED manifest the reconcile pass owns; you ' +
    'MUST NOT edit it from a folder agent; LOG it as a residual_ripple keyed on `package.json`. Add the package to the correct group in the target ' +
    '`README.md` (folder-local) and author the target `.api/<package>.md` from the published types via `uv run python -m tools.assay api` ' +
    '(folder-local). Never a per-area `package.json`.',
  'CLOSEOUT (the closeout pass ONLY): a genuinely-complete card moves to its file `[02]-[CLOSED]` section as a collapsed one-liner ' +
    '`[ID]-[COMPLETE]: <one-line disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`); update the target `ARCHITECTURE.md` ' +
    '`[02]-[SEAMS]` section ONLY when a real cross-folder seam landed. Realize/critique/redteam passes NEVER change card status.',
].join('\n')
const BARHUNT = [
  'BAR — a high-value IMPLEMENT leaves every owner capturing the capability the card needs from the packages it admits, every sprawl collapsed ' +
    'into one denser owner with NO capability lost, and every fence transcription-complete against the published types. The critique guards ' +
    'capability conservation, charter completeness, and type-density; the red-team attacks every fence for a surface that could still collapse, a ' +
    'thin wrapper, a silent functionality drop during a refactor, a missed package capability the card needs, or a framework violation, and fixes ' +
    'each in place.',
  'HUNT (at implement, critique, and red-team alike): UNDER-CAPTURED CAPABILITY — an admitted package whose `.api`/types expose capability the ' +
    'CARD needs but no owner exploits is a gap, closed by deepening a fence. SURFACE SPRAWL — parallel interfaces/types/classes modelling one ' +
    'concept collapse into ONE polymorphic surface (tagged discriminated union + exhaustive `match`/`assertNever`) with no functionality removed. ' +
    'RAIL UNIFICATION — one entrypoint family per rail, one typed-error channel per domain (`Effect`/`Either`), total exhaustive handling. ' +
    'OPTIMIZATION — correctness first, then type-precision (branded/nominal, template-literal, conditional/mapped types) and runtime shape, not ' +
    'only line-count. NEW WORK SURFACED — api gaps and tasks the implementation exposes are realized or recorded the same turn.',
  'NAIVETY (two orthogonal axes, both intolerable at every stage): COVERAGE — the owner models a thin slice of its concept (the obvious three ' +
    'fields where the domain carries fifteen; a two-case family for a twenty-case domain) — deepen to the full concept NOW. APPROACH — enumerated ' +
    'hardcoded instances (a fixed roster of styles/patterns/variants spelled one-by-one) where a parameterized algorithmic owner should GENERATE ' +
    'the space — the roster is seed DATA feeding ONE generator over named parameters, never the mechanism itself; rebuild it so. COLLAPSE FLOOR — ' +
    'every enumerated hunt/collapse-signal list in this prompt is a FLOOR, never the complete set: any repeated structure, parallel spelling, or ' +
    'enumerable family an algebra, table, fold, or generator can own is a collapse target you find beyond the list.',
].join('\n')
const ULTRA = [
  'OPERATIVE DOCTRINE: docs/stacks/typescript/ + coding-ts is the route-owned law — hold every fence to it as fact; docs/stacks/csharp/ is the ' +
    'density/ambition FLOOR. COLLAPSE >=3 parallel interfaces/types/classes modelling one concept into ONE polymorphic surface (tagged ' +
    'discriminated union + exhaustive match), never parallel names. AOP: cross-cutting concerns (retry, telemetry, validation, caching, receipts, ' +
    'fault rails) as Effect combinators/layers/decorators, not repeated inline. UNIFIED rails + UNIFIED pipelines + feature-arms-as-cases (never ' +
    'loose separate). Parameterize inputs AND outputs with generics at depth; no stringy/weak typing.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows raw -> admit ONCE through a `Schema` parse at the boundary (parse, never trust input) ' +
    '-> canonical owner -> unified `Effect`/`Either`/`Option` rail -> projection -> egress, BOTH ingress and egress parameterized so the same ' +
    'owner sources and sinks across consumers without interior edits. Interior code never re-validates, never sees raw/untyped/provider shapes.',
  'STACK CAPABILITY (ultra-stacking): ENUMERATE both catalog tiers IN FULL from a real listing — `ls` the shared `libs/typescript/.api/` AND ' +
    'the area `<area>/.api/`, never a memory-recall inventory — read every catalog the card touches, and mine each to OPERATOR DEPTH: the MOST ' +
    'powerful combinators each package reaches, composed into single dense rails, the shared Effect ecosystem ' +
    '(`Effect`/`Layer`/`Context`/`Schema`/`Stream`) layered end-to-end ON TOP OF the area-specific packages, NOT naive `Promise`/`try`-`catch` ' +
    'glue. An admitted capability the card needs that NO owner exploits is a defect closed by deepening a fence; capability the card needs ' +
    're-derived by hand is a defect; a cited member that cannot be verified against the published types is a PHANTOM — delete it and rebuild ' +
    'the fence on verified spellings. (Implement uses the capability the card needs; it does not max-stack every catalog for its own sake — ' +
    'that is rebuild.)',
  'PRESERVE all intended capability (densify, never delete functionality). Where a fence is already strong, deepen; where it is flat/naive, ' +
    'rebuild ground-up. Never regress correctness or boundary law.',
].join('\n')
const PATLAW = [
  'TS PATTERN LAW (ultra-advanced ONLY; do not preserve the naive idioms of the existing code): ZERO `any`, zero implicit `any`, zero unsafe `as`, ' +
    'zero non-null `!`; model with branded/nominal types, exact discriminated unions with EXHAUSTIVE handling (`assertNever` on the default), ' +
    '`readonly`/`as const`, template-literal types, conditional/mapped types, and the `satisfies` operator. NO runtime `enum` — use `const` unions ' +
    'or `Schema`/Effect.',
  'Domain logic runs on typed-error rails — `Effect`/`Either`/`Option`, NEVER `throw` in domain code; boundaries validate through `Schema` (parse, ' +
    'never trust input). `import type`/`export type` are explicit; side-effect/value imports preserve runtime order. Per the ' +
    'docs/stacks/typescript file-organization overlay: `Effect.Service` owners are SERVICES, `Layer`/runtime wiring is COMPOSITION, runtime ' +
    'schemas/classes are MODELS, and catalog/registry rows stay after the owners they reference.',
  'Keep conventions IDENTICAL across every area so the corpus reads as one ultra-advanced codebase. One canonical semantic name per bounded ' +
    'concept; discriminate on input shape rather than proliferating `get`/`getMany`/`getById` names.',
].join('\n')
const BOUNDARIES = 'BOUNDARY LAW: keep every area owner strictly in its lane; internal code uses canonical names and shapes with mapping (and ' +
  '`Schema` validation) only at the edge; do not trample a sibling owner while densifying; respect the dependency direction of the workspace. Each ' +
  'target realizes ONLY its OWN cards into its OWN pages; a concern owned twice across a runtime, an area mixing unrelated concerns, or coupling ' +
  'to a sibling owner INTERIOR (vs its wire/seam) is a defect.'
const PROSE = [
  'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
    'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
    'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing ' +
    'contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph.',
  'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in backticks. Name ' +
    'the exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design ' +
    'content.',
].join('\n')
const COMMENTS = 'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
  'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY ' +
  'where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a ' +
  'comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no ' +
  'task/process/session/history/proof/review comments, no TSDoc bloat. Densify names and types so comments are rarely needed; cut every low-value ' +
  'comment.'
const DOCTRINE = [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS].join('\n')

// --- [OPERATIONS] ------------------------------------------------------------------------
const folderName = (p) => p.split('/').filter(Boolean).pop() || p
const pkgPath = (toPkg) => { const k = String(toPkg || '').trim().replace(/^typescript:/, ''); return k.indexOf('libs/') === 0 ? k : ROOT + '/' + k }
const discoverPrompt = (targets) => [LAW, '', CARD, '',
  'TASK: DISCOVER + SEQUENCE the open work across these TypeScript session targets: ' + JSON.stringify(targets) + '. This is the read-only ' +
    'reconnaissance grounding every downstream stage, and read-only is its ONLY concession: resolve scope against real disk state (a real ' +
    'listing of each target folder, never memory), then read each target area-root `IDEAS.md` + `TASKLOG.md` IN FULL from disk — full-file ' +
    'reads, never a grep/skim; the design pages are downstream scope. For EACH target return: (1) folder — echo the target folder path exactly; ' +
    '(2) tasks — EVERY open card in `TASKLOG.md` (status ACTIVE/QUEUED/BLOCKED; carry the Atomic flag); (3) ideas — the 1-3 MOST actionable open ' +
    'cards in `IDEAS.md` (tasks-first doctrine: at most 3, the ones whose Anchors are most settled and whose ripples land on in-scope targets), ' +
    'HARD CAP 3; (4) order — ONE sequenced slug list, ALL tasks first in dependency order then the chosen ideas; (5) ripples — for EVERY card ' +
    'carrying a `Ripple:` field, one row {from_slug, klass, to_pkg, to_slug}: klass=`in_scope` if to_pkg is one of these targets, `oos_samelang` ' +
    'if it is another libs/typescript area, `cross_lang` if it points at `libs/csharp` / `libs/python` / `libs/typescript/.planning` / ' +
    '`libs/.planning`; confirm every counterpart ON DISK — open the named pkg card file and locate the mirror slug, never assert it from memory; ' +
    '(6) gates — for any [BLOCKED] card, {blocked_slug, gated_by_slug, in_scope} where in_scope is true iff the gating work is itself an open ' +
    'card in one of these targets. Return malformed_ripples for any `Ripple:` line you cannot parse into a pkg+slug, or whose counterpart slug ' +
    'you cannot locate on disk — an unverified ripple row is a phantom that belongs in malformed_ripples, never in ripples. Read the FULL card ' +
    'body (every bullet) to classify — never guess from the thesis. Your map is an initial pointer, never a ceiling: downstream agents re-read ' +
    'every full card and page, and the map never licenses a downstream skim. Return the structured map ONLY; edit nothing (discovery is the ' +
    'sole read-only stage).'].join('\n')
const implementPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: IMPLEMENT — realize the open cards of `' + folder + '` into deep design-page FENCES at the ULTRA-ADVANCED bar. The sequenced worklist ' +
    '(slugs + ripple map; read each FULL card body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`, never the thesis alone):\n' + seq + '\nREAD: ' +
    'each card full body; every design page the card names under `' + folder + '/.planning/**`; the sibling pages it seams to; the area-root ' +
    '`ARCHITECTURE.md` + `README.md`; the operative docs/stacks/typescript/ pages + coding-ts (docs/stacks/csharp/ as the ambition floor); BOTH ' +
    '.api tiers — the shared `' + SHARED_API + '/*.md` AND the area `' + folder + '/.api/*.md` (stack them, the shared Effect/Schema rails layered ' +
    'onto the area packages) — cross-checked against the published types in node_modules; verify a member via `uv run python -m tools.assay api`. ' +
    'Realize EVERY card in `order` (all tasks incl. Atomic, then the ideas) into deep fences IN `' + folder + '` PAGES ONLY, in LIFECYCLE order ' +
    '(admit raw ONCE through a `Schema` parse at the boundary -> canonical owner -> weave every cross-cutting concern as an Effect ' +
    'combinator/layer/decorator over a thin pure core -> compose through ONE unified `Effect`/`Either`/`Option` rail -> project + egress, BOTH ' +
    'ingress and egress parameterized with generics at depth). Collapse parallel interfaces/types/classes into ONE tagged discriminated union with ' +
    'EXHAUSTIVE `match`/`assertNever`; brand/nominal types where primitives are overloaded. ZERO `any`/implicit-any/unsafe `as`/non-null `!`, NO ' +
    'runtime `enum`, NO `throw` in domain logic, `import type` explicit. Resolve any [BLOCKED] card inline (read published types in node_modules / ' +
    '`assay api`). PACKAGE ADMISSION (only if a card needs a not-yet-admitted package): do the FOLDER-LOCAL parts NOW — add the package to the ' +
    'correct group in `' + folder + '/README.md` and author `' + folder + '/.api/<pkg>.md` from the published types via `assay api` — and LOG the ' +
    'central `package.json` (+ `pnpm-workspace.yaml` catalog) dependency add as a residual_ripple with files including `package.json` (a single ' +
    'reconcile agent owns that shared manifest; you MUST NOT edit it). RIPPLES: realize ONLY `' + folder + '`\'s OWN half of every seam; NEVER ' +
    'edit another folder page. For each ripple your cards carry, log a residual_ripple {files:[your_page, counterpart_page], pkg, slug, ' +
    'mirror_slug, claim} stating the contract your half exposes (cross-language/lib-wide legs are deferred to the other-language workflow). Do NOT ' +
    'close any card — the closeout pass owns card status. High-signal prose all-backticked, comment hygiene, fix-in-place (read-then-extend, ' +
    'preserve capability). Return verdict + realized slugs + deferred (any card you could not realize, with reason) + collapsed (before->after ' +
    'counts) + residual_ripples + summary.'].join('\n')
const critiquePrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: DOCTRINAL-CONFORMANCE AUDIT + CHARTER-COMPLETENESS + FIX IN PLACE across `' + folder + '`. You are an ULTRA-HARSH, UNAGREEABLE auditor: ' +
    'is this TRULY ultra-advanced TS, or naive code in disguise? Assume a violation exists in every fence until you prove otherwise; "good enough" ' +
    'is rejected. The cards realized this turn (read each FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`):\n' + seq + '\nREAD ' +
    'the realized pages under `' + folder + '/.planning/**`, the sibling pages, docs/stacks/typescript/ + coding-ts, and BOTH .api tiers (shared `' + SHARED_API + '` ' +
    '+ area `' + folder + '/.api`). Run these MECHANICAL checklists line-by-line as a FLOOR you hunt PAST, and REPAIR every hit in place (a fix, ' +
    'never a ledger note):',
  '(1) COLLAPSE_SCAN — sibling prefix/suffix names -> one input-shape-discriminating entrypoint; >=3 parallel interfaces/types/classes for one ' +
    'concept -> ONE tagged discriminated union with exhaustive `match`; a `get`/`getMany`/`getById` family -> one input-keyed entrypoint; ' +
    'functions differing only by a literal -> parameterize as policy; a bool selecting two bodies -> one derived body/policy value; a function ' +
    'calling exactly one other -> delete the hop; parallel dispatch arms repeating structure -> a table or fold; the same 2-4 wrappers recurring ' +
    '-> one Effect combinator/layer aspect. These signals are a FLOOR, never the complete set — hunt collapse targets beyond them per the ' +
    'COLLAPSE FLOOR law.',
  '(2) SHAPE/BRANDING — model each concept as ONE polymorphic surface; brand/nominal types where a primitive is overloaded (ids, units, tokens); ' +
    'exact discriminated unions with a discriminant tag; `readonly`/`as const`/`satisfies`; template-literal/conditional/mapped types where they ' +
    'tighten. Kill every parallel interface, one-field wrapper, field-rename type, and stringly-typed shape.',
  '(3) KNOB_TEST — delete each parameter: if the value already encodes what it carried, it was a knob — collapse a `mode`/`strict`/`batch` flag ' +
    'into a policy value or input-shape discriminant; move every `timeout`/`retry`/`deadline` into an Effect combinator/layer, never a signature ' +
    'param.',
  '(4) ASPECTS — every cross-cutting concern (retry/telemetry/validation/caching/receipts/fault rails) MUST be an Effect ' +
    'combinator/`Layer`/decorator that never throws into domain flow; 2-4 co-occurring wrappers collapse into one aspect; deterministic layer ' +
    'order verified. Inline-repeated concerns and sibling helper functions are defects.',
  '(5) RAILS/BOUNDARIES — domain logic on `Effect`/`Either`/`Option`, NEVER `throw`; every boundary validates through `Schema` (parse, never trust ' +
    'input); the error channel is a typed union, not `unknown`/`Error`; accumulate-vs-abort disposition correct; `Layer`/`Context` for dependency ' +
    'wiring, `Stream` for streaming — no naive `Promise`/`async` glue where Effect belongs.',
  '(6) TYPING/MODERN — ZERO `any`/implicit-any/unsafe `as`/non-null `!`; NO runtime `enum` (use `const` unions or `Schema`); EXHAUSTIVE union ' +
    'handling with `assertNever`; `import type`/`export type` explicit; the file-organization overlay honored (`Effect.Service` -> SERVICES, ' +
    '`Layer` -> COMPOSITION, schemas/classes -> MODELS, catalogs after their owners); members cross-checked against the published node_modules types.',
  '(7) CHARTER-COMPLETENESS — for EVERY card in the worklist, verify the realized fences GENUINELY fulfill its `Capability`/`Shape`/`Unlocks` ' +
    '(read the full card from disk): a missing modality, an unrealized `Shape` clause, a stubbed/placeholder fence, or a capability the card ' +
    'promises but the fences do not deliver is a DEFECT — realize it NOW. A card whose fences are thin against its charter is not done.',
  '(8) NAIVETY — both axes per fence: a COVERAGE thin-slice owner (a fraction of the concept modeled where the domain carries far more) is ' +
    'deepened to the full concept NOW; an APPROACH enumerated roster (fixed instances spelled one-by-one where ONE parameterized generator ' +
    'should own the space) is rebuilt as seed data feeding one generator.',
  'Also enforce the ultra-stacking law (both `.api` tiers enumerated in full; a thin area-only subset ignoring the shared Effect/Schema rails ' +
    'the card needs is a defect; a cited member you cannot verify against the published types is a phantom — delete it), cross-area convention ' +
    'consistency per coding-ts, and prose + comment hygiene. EDIT the `' + folder + '` pages to fix every hit; realize ONLY `' + folder + '` ' +
    'pages and OVERRIDE any earlier residual you can now resolve; log any genuine cross-FOLDER item as a residual_ripple {files, pkg, slug, ' +
    'mirror_slug, claim}. Return verdict + realized + deferred + collapsed + residual_ripples + summary.'].join('\n')
const redteamPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE across `' + folder + '`. You are a HOSTILE principal reviewer whose explicit goal is to REJECT this ' +
    'design as naive TypeScript: assume it is junior, under-typed, or wrong until the fences prove otherwise; the burden of proof is ON THE ' +
    'DESIGN. The cards realized this turn (read each FULL body):\n' + seq + '\nRead docs/stacks/typescript/ + coding-ts, the published library ' +
    'types, BOTH .api tiers (shared `' + SHARED_API + '` + area `' + folder + '/.api`), and the sibling pages. Attack relentlessly and REPAIR ' +
    'every defect in place — no soft-pedalling, no could/should, a fix never a ledger.',
  'PRIMARY LENS — fundamental design, multi-faceted: (A) COUNTERFACTUAL on the core choice — is the owner, the union algebra, and the dispatch ' +
    'form categorically the strongest the doctrine admits, or does a denser tagged-union owner, a branded type, or a DEEPER Effect/Schema ' +
    'primitive collapse the whole fence? If a fundamentally stronger design exists, rebuild to it — never defend the incumbent. (B) ' +
    'ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: when the next case/dimension/modality/provider arrives, does it land as ONE ' +
    'union case with every consumer broken loudly at type-check (exhaustive `assertNever`)? If it would touch multiple sites, reshape so the ' +
    'growth axis is a case, row, policy value, or layer swap. (C) LONG-TAIL + MULTI-DIMENSIONAL — attack every input/output/edge/failure mode ' +
    '(empty, singular, plural, stream, malformed, concurrent, cancelled, partial-failure, version-skew); is the boundary `Schema`-parsed; are BOTH ' +
    'ingress AND egress parameterized so this owner sources and sinks across consumers without interior edits? (D) BOUNDARY-INTEGRITY — a concern ' +
    'owned twice in a runtime, an area mixing concerns, coupling to a sibling owner INTERIOR (vs its wire/seam), OR a sibling planning page left ' +
    'STALE by this folder change even when no ripple card names it (ports/boundaries/wires/seams drift) is a defect: fix it within `' + folder + '`, ' +
    'or record it as a residual_ripple. (E) SURFACE-SPRAWL-IN-TIME — `any`/implicit-any/unsafe `as`/non-null `!`; `throw` in domain logic instead ' +
    'of `Effect`/`Either`; non-exhaustive union handling; runtime `enum`; missing branded types; loose `import` where `import type` is required; ' +
    'naive `Promise`/`async` glue where `Effect`/`Layer` belongs; a thin area-only `.api` subset ignoring the shared Effect/Schema rails the card ' +
    'needs; an unvalidated boundary (no `Schema` parse); a phantom member: for EACH, state the concrete failure (what breaks, under which ' +
    'input/edge) and repair it in place. (F) NAIVETY-AXES — attack COVERAGE (the owner models a thin slice of its concept where the domain ' +
    'carries far more — deepen to the full concept) and APPROACH (a fixed enumerated roster of instances where ONE parameterized generator ' +
    'should own the space — demote the roster to seed DATA feeding one generator; the roster is never the mechanism).',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (every time): re-attack every conformance dimension with fresh hostile eyes, trusting nothing the prior ' +
    'passes claimed — COLLAPSE_SCAN, shape/branding, the KNOB_TEST per param, the Effect-aspect taxonomy, rail + Schema-boundary discipline, ' +
    'charter-completeness per card, the two NAIVETY axes, the zero-any/throw/enum typing law, both-tier `.api` use at operator depth (phantom ' +
    'members deleted), the file-organization overlay, and prose/comment hygiene — a FLOOR list you hunt past — and fix every defect. Even ' +
    'absent a structural rebuild, the fences must end objectively denser, more type-safe, and more powerful ' +
    'than the critique left them; if the strongest form is genuinely already present, prove it by finding nothing — never invent churn. Realize ' +
    'ONLY `' + folder + '` pages; log cross-FOLDER items as residual_ripples. Return verdict + realized + deferred + collapsed + residual_ripples ' +
    '+ summary.'].join('\n')
const reconcileFixPrompt = (cl) => [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '',
  'TASK: RECONCILE this cluster of cross-FOLDER residuals the per-folder passes deferred. There is NO severity — treat EVERY residual as ' +
    'must-address. Read EVERY listed file. Handle each residual by KIND: (a) IN-SCOPE SEAM (both halves already realized by their own target ' +
    'folders) — read both pages, ALIGN them to ONE shared `Schema`/type contract, fix any mismatch, set `seam_landed` true; (b) OUT-OF-SCOPE ' +
    'SAME-LANGUAGE COUNTERPART (the counterpart card lives in a non-target libs/typescript area) — realize that ONE counterpart card fence (its ' +
    'half only, NEVER the area other cards) at the same bar and align the seam; (c) CENTRAL PIN — apply every `package.json` (+ ' +
    '`pnpm-workspace.yaml` catalog) dependency add in this cluster (you are the ONLY agent that edits that shared manifest; apply them all ' +
    'serially, keeping the existing group/order) and list them in `admitted`; (d) CROSS-LANGUAGE / LIB-WIDE LEG — record it in `deferred_legs` and ' +
    'do NOT realize it (its counterpart is the other-language workflow concern). Preserve all capability, regress no file, never trample a sibling ' +
    'owner interior. For every ripple counterpart you touch, emit a `pairs` row {pkg, slug, mirror_slug, seam_landed}. If a residual is FACTUALLY ' +
    'INCORRECT or not a real defect, leave it and say why in the summary — never silently skip a real one. Residuals:\n' + JSON.stringify(cl, null, 1)].join('\n')
const reconcileVerifyPrompt = (cl, fix) => [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '',
  'TASK: ADVERSARIAL WRITING VERIFY of the reconcile fixes — never a friendly confirmation; the fixer\'s claims are illusory until proven on ' +
    'disk, and a verdict issued without a disk proof is itself a defect. For EACH residual: (1) RE-DERIVE necessity — read every named file ' +
    'from disk and re-derive whether the claimed defect was real and whether the applied resolution is the RIGHT one, trusting nothing the ' +
    'fixer reported; (2) PROVE on disk the work was done properly — the seam genuinely aligned on BOTH pages to ONE shared `Schema`/type ' +
    'contract, the counterpart fence genuinely realized at the bar, the central pin genuinely applied; (3) REPAIR TO ROOT — a loose, weak, or ' +
    'token fix (a single-point patch where a root-level dense reconstruction of the SAME files is available; a seam aligned in prose but not in ' +
    'types; a thin counterpart fence) is itself a defect YOU repair NOW by editing those same files to the objectively-best form, then count ' +
    'the claim fixed; (4) only then classify each claim: status "fixed" (real defect now genuinely resolved — by the fixer or by YOUR repair), ' +
    '"invalid" (the claim is factually wrong / not a real defect — cite why), or "open" (real defect genuinely unreachable from the files at ' +
    'hand — never to punt a strengthenable fix). Default to "open" over a hollow "fixed"; mark "invalid" ONLY when you can show the claim is ' +
    'wrong; set overall true only when NO claim remains open. List every file you edited in repaired_files. Claims:\n' +
    JSON.stringify(cl, null, 1) + '\nFiles the fixer touched: ' + JSON.stringify(fix.files) +
    '\nPairs the fixer reported: ' + JSON.stringify(fix.pairs || [])].join('\n')
const closeoutPrompt = (folder, seamJson) => [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', PROSE, '', COMMENTS, '',
  'TASK: TRUTHFUL CLOSEOUT + FINAL REMEDIATION of `' + folder + '`. This is the SOLE owner of card status. For EVERY card that was in scope this ' +
    'run, read its FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md` and the realized fences under `' + folder + '/.planning/**`, ' +
    'then SANITY-VERIFY the fences genuinely fulfill the card `Capability`/`Shape`/`Unlocks` against the published types (verify a member via `uv ' +
    'run python -m tools.assay api`). If a card is WEAK or PARTIAL, make a FINAL in-place REMEDIATION NOW (it already passed ' +
    'implement->critique->redteam this turn; deepen the fences under `' + folder + '` to genuinely complete the charter), then re-verify. Assign ' +
    'each card a strength: `strong` (every charter clause delivered, fences transcription-complete and fully type-safe against the published ' +
    'types), `partial` (most delivered, a clause still thin), `weak` (charter not met). CLOSE only genuinely-complete cards: move them to the ' +
    '`[02]-[CLOSED]` section of their owning file (`' + folder + '/IDEAS.md` or `' + folder + '/TASKLOG.md`) as a collapsed one-liner ' +
    '`[ID]-[COMPLETE]: <disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`), and update `' + folder + '/ARCHITECTURE.md` `[02]-[SEAMS]` ' +
    'ONLY when a real cross-folder seam landed. RIPPLE PAIRS: a card carrying a `Ripple:` closes COMPLETE only if its seam landed — this map gives ' +
    'seam_landed per slug (`false` = seam did NOT land, keep the card OPEN; `true`/absent = judge on your own half): ' + seamJson + '. Honestly ' +
    'RE-OPEN any card you cannot bring to `strong` this run, with a one-line reason (a real out-of-run or cross-language dependency). The ' +
    'orchestrator will DEMOTE any card you mark complete whose strength is not `strong`, so never inflate. Return closed [{slug, disposition, ' +
    'strength}] + reopened [{slug, reason}] + summary.'].join('\n')

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
  const seq = JSON.stringify({ order: t.order, tasks: t.tasks, ideas: t.ideas, ripples: t.ripples, gates: t.gates || [] }, null, 1)
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
for (const r of reconciled) for (const p of ((r.fix && r.fix.pairs) || [])) { const fp = pkgPath(p.pkg); if (!TARGET_SET.has(fp) && fp.indexOf(ROOT + '/') === 0) oosFolders.add(fp) }
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
  root: ROOT,
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
