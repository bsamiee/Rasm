export const meta = {
  name: 'implement-ts',
  whenToUse: 'Realize open cards into design-page code fences across the TypeScript target folders.',
  description: 'Realize every open IDEAS/TASKLOG card across the libs/typescript capability folders (core, security, data, runtime, ui, iac) into deep design-page code FENCES at the docs/stacks/typescript bar (Effect-TS rails, Schema-first boundaries, one canonical owner per concept, exhaustive discriminated unions, zero any/throw/enum), repair every ripple in-pass, and truthfully close the cards. Each target folder runs its OWN discover -> implement -> critique -> redteam chain, ALL chains concurrent under one pooled cap: a folder starts the moment its own discovery lands, a folder with no open cards no-ops after its own discovery, and a failed chain isolates without rejecting the pool. Discovery hands downstream stages navigation FACTS (paths, verified members, seam targets) and never verdicts; it runs read-only on gpt-5.5 dispatched through a sonnet codex wrapper (CODEX flag; false restores the native inherit-model lane), lands its full product on disk under .claude/scratch/implement-ts, and returns a thin receipt plus a jq-extracted structural skeleton on the wire; the implement stage reads the report IN FULL from disk, and when the skeleton proves page-disjoint card groups it fans over them. Every stage WRITES and repairs the page-level ripples its own work exposes in the same pass — in-scope seams aligned against current disk, 1-hop out-of-scope same-language counterpart fences realized directly — with BLOCKED probes and folder-local package admission inline. The redteam is each folder chain\'s terminal stage and sole card-status owner: it final-remediates weak realizations in place and closes only cards whose realization it verified strong on disk. Two handoffs route to the run\'s terminal single-writer, the central pnpm-workspace.yaml catalog pin (+ its catalog: manifest row) and the area ARCHITECTURE.md [02]-[SEAMS] row: folder agents report exact rows, one terminal sonnet writer applies them serially. Card-driven (it implements ideas/tasks), NOT the in-isolation api-stacking of rebuild-api. TypeScript-only. args = a target path string, an array of paths, or empty for all capability folders. The language-wide libs/typescript/.planning is out of scope.',
  phases: [
    { title: 'Realize', detail: 'all folder chains concurrent under one pooled cap: discover(gpt-5.5 via codex wrapper, read-only; full product to disk, thin receipt + structural skeleton on the wire) -> implement(high; reads the discovery report from disk, fans over discovery-proven page-disjoint card groups) -> critique(high) -> redteam(high, terminal close); a folder with no open cards no-ops after its own discovery; every writing stage re-reads current disk, repairs page-level ripples in-pass, and reports central pin rows + ARCHITECTURE.md [02]-[SEAMS] rows for the terminal single-writer instead of editing those surfaces' },
    { title: 'Pins', detail: 'one terminal single-writer applies every reported pnpm-workspace.yaml catalog pin + its catalog: row in the owning package.json and every reported ARCHITECTURE.md [02]-[SEAMS] row serially; runs only when rows were reported', model: 'sonnet' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 14     // concurrent folder-CHAIN ceiling — the default target sets run below it; it binds only when args name more folders than CAP
const IMPL_FAN = 3 // max implement agents fanned per folder, and only over discovery-proven page-disjoint card groups
const STAGGER_MS = 1500
const STALL = 300000
const ROOT = 'libs/typescript'
const SHARED_API = 'libs/typescript/.api'
const CENTRAL = 'pnpm-workspace.yaml'
const DEFAULT_TARGETS = ['libs/typescript/core', 'libs/typescript/security', 'libs/typescript/data', 'libs/typescript/runtime', 'libs/typescript/ui', 'libs/typescript/iac']
const CODEX = true
const CODEX_DIR = '.claude/scratch/implement-ts' // wrapper task/schema/report files, one triple per lane

// --- [INPUTS] ----------------------------------------------------------------------------

const norm = (t) => { const s = String(t).trim(); return s.indexOf('libs/') === 0 ? s : ROOT + '/' + s }
const TARGETS = Array.isArray(args) ? args.filter(Boolean).map(norm)
  : (args && typeof args === 'object' && Array.isArray(args.targets)) ? args.targets.filter(Boolean).map(norm)
  : (typeof args === 'string' && args.trim() && args.trim().toUpperCase() !== 'ALL') ? [norm(args)]
  : DEFAULT_TARGETS

// --- [MODELS] ----------------------------------------------------------------------------

// Per-folder discovery PRODUCT (the on-disk report): `pages` per card are disk-verified Anchors
// targets proving page-disjoint implement groups; `malformed_ripples` is a required attestation
// (empty = none found); `coverage` is part of the product — requested vs actually-read scope.
const RIPPLE_ROW = { type: 'object', additionalProperties: false, required: ['from_slug', 'klass', 'to_pkg', 'to_slug'], properties: { from_slug: { type: 'string' }, klass: { type: 'string', enum: ['in_scope', 'oos_samelang', 'cross_lang'] }, to_pkg: { type: 'string' }, to_slug: { type: 'string' } } }
const GATE_ROW = { type: 'object', additionalProperties: false, required: ['blocked_slug', 'gated_by_slug', 'in_scope'], properties: { blocked_slug: { type: 'string' }, gated_by_slug: { type: 'string' }, in_scope: { type: 'boolean' } } }
const MALFORMED_ROW = { type: 'object', additionalProperties: false, required: ['from_slug', 'raw'], properties: { from_slug: { type: 'string' }, raw: { type: 'string' } } }
const CARD_REF = { type: 'object', additionalProperties: false, required: ['slug', 'pages'], properties: { slug: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } } }

const DISCOVERY_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'order', 'tasks', 'ideas', 'ripples', 'malformed_ripples', 'gates', 'coverage'], properties: {
  folder: { type: 'string' },
  order: { type: 'array', items: { type: 'string' } },
  tasks: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'status', 'pages', 'atomic', 'thesis'], properties: { slug: { type: 'string' }, status: { type: 'string' }, atomic: { type: 'boolean' }, thesis: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } } } },
  ideas: { type: 'array', maxItems: 3, items: { type: 'object', additionalProperties: false, required: ['slug', 'status', 'pages', 'thesis'], properties: { slug: { type: 'string' }, status: { type: 'string' }, thesis: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } } } },
  ripples: { type: 'array', items: RIPPLE_ROW },
  gates: { type: 'array', items: GATE_ROW },
  malformed_ripples: { type: 'array', items: MALFORMED_ROW },
  coverage: { type: 'object', additionalProperties: false, required: ['requested', 'read', 'skipped', 'unverified'], properties: {
    requested: { type: 'array', items: { type: 'string' } },
    read: { type: 'array', items: { type: 'string' } },
    skipped: { type: 'array', items: { type: 'string' } },
    unverified: { type: 'array', items: { type: 'string' } } } },
} }

// Thin wire receipt + mechanically-extracted skeleton: the discovery PRODUCT (statuses, theses,
// coverage, the full navigation dossier) stays on disk at `report`; only status + count + headline
// plus the structural rows the orchestrator fans and seams over (order/cards/gates/ripples/malformed)
// travel inline. `cards` = {slug, pages} per open card — the page-disjointness proof, nothing more.
const RECEIPT = { type: 'object', additionalProperties: false, required: ['ok', 'report', 'entries', 'headline', 'failure', 'order', 'cards', 'gates', 'ripples', 'malformed'], properties: {
  ok: { type: 'boolean' }, report: { type: 'string' }, entries: { type: 'integer' },
  headline: { type: 'string' }, failure: { type: 'string' },
  order: { type: 'array', items: { type: 'string' } },
  cards: { type: 'array', items: CARD_REF },
  gates: { type: 'array', items: GATE_ROW },
  ripples: { type: 'array', items: RIPPLE_ROW },
  malformed: { type: 'array', items: MALFORMED_ROW } } }

// Required-but-possibly-empty `ripples`/`pins`/`seams` are attestations: ripple repair ran in-pass,
// and pin + seam rows are the run's only single-writer handoffs — empty attests none arose, never a skip.
const RIPPLES = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['counterpart', 'action'], properties: { counterpart: { type: 'string' }, action: { type: 'string' } } } }
const PINS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['package', 'row'], properties: { package: { type: 'string' }, row: { type: 'string' } } } }
const SEAMS = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['file', 'row'], properties: { file: { type: 'string' }, row: { type: 'string' } } } }

const FIXLOG_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'ripples', 'pins', 'seams', 'summary', 'realized', 'deferred', 'collapsed'], properties: {
  folder: { type: 'string' },
  verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
  realized: { type: 'array', items: { type: 'string' } },
  deferred: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  collapsed: { type: 'string' },
  ripples: RIPPLES,
  pins: PINS,
  seams: SEAMS,
  summary: { type: 'string' },
} }

const REDTEAM_SCHEMA = { type: 'object', additionalProperties: false, required: ['folder', 'verdict', 'ripples', 'pins', 'seams', 'closed', 'reopened', 'summary', 'realized', 'deferred', 'collapsed'], properties: {
  folder: { type: 'string' },
  verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
  realized: { type: 'array', items: { type: 'string' } },
  deferred: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  collapsed: { type: 'string' },
  ripples: RIPPLES,
  pins: PINS,
  seams: SEAMS,
  closed: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'disposition', 'strength'], properties: { slug: { type: 'string' }, disposition: { type: 'string', enum: ['complete', 'dropped'] }, strength: { type: 'string', enum: ['strong', 'partial', 'weak'] } } } },
  reopened: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['slug', 'reason'], properties: { slug: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }

const PIN_SCHEMA = { type: 'object', additionalProperties: false, required: ['applied', 'seam_rows_applied', 'rejected', 'summary'], properties: {
  applied: { type: 'array', items: { type: 'string' } },
  seam_rows_applied: { type: 'array', items: { type: 'string' } },
  rejected: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['target', 'reason'], properties: { target: { type: 'string' }, reason: { type: 'string' } } } },
  summary: { type: 'string' },
} }

// --- [DOCTRINE] --------------------------------------------------------------------------

const LAW = [
  'Rasm monorepo, libs/typescript planning corpus (markdown specs of intended TypeScript module designs). This is ultra-advanced TS realization, ' +
    'never a polish pass — discard naive idioms wholesale. CLAUDE.md manifest + ' +
    'WORKSPACE_LAW strata govern. The session targets are libs/typescript capability folders (core, security, data, runtime, ui, iac; ' +
    'test infrastructure lives under tests/, never in the branch). Each holds ' +
    '`IDEAS.md` + `TASKLOG.md` + `ARCHITECTURE.md` + ' +
    '`README.md` at its area ROOT, design pages at ' +
    '`<area>/.planning/<subdomain>/*.md`, and an area-specific `.api/*.md` catalog. The language-wide `libs/typescript/.planning` is OUT of scope ' +
    'this run. Read the area-root `ARCHITECTURE.md` (sub-domain map + `[02]-[SEAMS]`) and `README.md` (admitted-package roster) as governing ' +
    'context. Cross-folder repair lands at seams, counterpart cards, and consumer sites — never by rebuilding a sibling owner interior.',
  'STANDARD: docs/stacks/typescript/ is the route-owned law, composed IN FULL — README plus language, derivation, values, computation, ' +
    'shapes, surfaces-and-dispatch, rails-and-effects, services-and-layers, concurrency, streams, boundaries — author TypeScript as dense, ' +
    'type-safe, and rich as that bar admits; the doctrine is the FLOOR the work pushes past, never the ceiling. READ every page and conform exactly. ' +
    'Cite ONLY real members of admitted ' +
    'packages, cross-checked against the published types in node_modules; verify a member via `uv run python -m tools.assay api` over the ' +
    'node_modules declarations — and when assay is unavailable (it is under concurrent construction), the published `.d.ts` read directly, ' +
    'BOTH `.api` tiers, and Context7/exa/tavily against the official package docs are the verification rail, never memory.',
  'This is IMPLEMENT, not the in-isolation api-stacking rebuild: realize the area SPECIFIC open IDEAS/TASKLOG cards into deep design-page FENCES. ' +
    'A FENCE is a markdown fenced code block inside a `.planning` design page — the work product itself, NEVER a `.ts`/`.tsx` source file. SCOPE ' +
    'per target: realize ALL open tasks (including `Atomic`-flagged minor tasks), then the 1-3 chosen open ideas, tasks first. Realize tied to the ' +
    'card charter (Capability/Shape/Unlocks/Anchors), composing the right admitted capability and crushing surface sprawl into fewer richer owners ' +
    'with zero functionality loss.',
  'TWO-TIER .api: every fence draws on BOTH the shared/universal catalogs at `' + SHARED_API + '/*.md` (enumerated from disk, never from ' +
    'memory) AND the area-specific catalogs at ' +
    '`<area>/.api/*.md`. The shared Effect substrate tier is SHARED capability you MUST consider and compose to realize the card properly — ' +
    'never hand-roll `Promise`/`try`-`catch` glue or settle for a thin area-only subset; layer the shared Effect ecosystem ' +
    '(`Effect`/`Layer`/`Context`/`Schema`/`Stream`) end-to-end ON TOP OF the area-specific packages. This is implement (use the capability the ' +
    'card needs), not rebuild (max-stack every catalog for its own sake).',
  'WRITE-FULLY + FIX-IT-NOW: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
    'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge. A cross-file ripple your edit exposes is YOURS in the ' +
    'same pass, wherever it lives: the seam counterpart on both ends, the consumer site, the stale sibling page, the 1-hop counterpart card fence ' +
    'in another libs/typescript area — repaired now and recorded in `ripples` (an empty `ripples` attests your pass exposed none, never that ' +
    'repair was skipped). TWO handoffs route to the run\'s terminal single-writer and are NEVER edited by a folder agent: the central ' +
    '`' + CENTRAL + '` catalog pin + its `catalog:` manifest row (report the exact rows in `pins`) and any area `ARCHITECTURE.md` `[02]-[SEAMS]` ' +
    'row (report {file, row} in `seams` — the highest-collision shared surface); every other page-level ripple stays yours, repaired distributed ' +
    'under the anchored-Edit discipline. If after real investigation a fence is already correct, say so — never invent edits to look busy.',
].join('\n')

const CARD = [
  'CARD SCHEMA: open cards live in the target `IDEAS.md` (ideas — larger conceptual capability) and `TASKLOG.md` (tasks — concrete targeted work), ' +
    'under section `[01]-[OPEN]`; closed cards collapse under `[02]-[CLOSED]`. A card is `[ID]-[STATUS]: <thesis>` then the bullets `Capability:` ' +
    '/ `Shape:` / `Unlocks:` / `Anchors:` / `Tension:` (only when a constraint shapes it) / `Ripple:` (only on a cross-folder counterpart) / ' +
    '`Atomic:` (only on a minor task). Open statuses: `ACTIVE` / `QUEUED` / `BLOCKED`. Closed: `COMPLETE` / `DROPPED`. ALWAYS read the FULL card ' +
    'body (every bullet) from disk — the thesis alone is never enough to realize the charter.',
  'RIPPLE: `Ripple: <lang>:<pkg> [SLUG]` (or `<pkg> [SLUG]`) is a BIDIRECTIONAL cross-folder link — the counterpart card in the named pkg carries ' +
    'the mirror slug, and ripples are PART of scope, repaired in the pass that exposes them, never handed to a later stage. Three classes: ' +
    'IN-SCOPE (counterpart is another TypeScript session target — its own pipeline realizes its card; you align your half of the seam to the ' +
    'counterpart page as it NOW stands on disk, and the later-landing side owns the final alignment), OUT-OF-SCOPE SAME-LANGUAGE (counterpart in ' +
    'a non-target libs/typescript area — YOU realize the 1-hop counterpart card fence and align the seam on both ends in the same pass; the ' +
    'ripple\'s scope is that counterpart card and its seam, not the foreign area\'s other cards), CROSS-LANGUAGE / LIB-WIDE (`libs/csharp`, ' +
    '`libs/python`, `libs/typescript/.planning`, `libs/.planning` — outside this TypeScript-only run\'s language rail; land your half stating the ' +
    'wire contract, and the card stays open unless it is complete on your half alone).',
  'PROBE FREELY (nothing gates probing): EVERY agent in EVERY phase may — and should — probe to verify reality at any time, for ANY card or design ' +
    'decision, not only `[BLOCKED]` ones — read the published types in `node_modules` and/or `uv run python -m tools.assay api` over the ' +
    'node_modules declarations to confirm any member or type (assay unavailable: the direct `.d.ts` read + both `.api` tiers + Context7/exa/' +
    'tavily own the proof); Rhino WIP (never Rhino 8) via the rhino-mcp skill or tools/rhino-bridge if a live ' +
    'host fact is needed. A `[BLOCKED]` card is REALIZED this turn whenever a probe resolves its blocker OR its gating work is in scope (a ' +
    'sibling-area contract resolves at the seam); a blocker is genuinely legitimate ONLY when it depends on work outside this run.',
  'PACKAGE ADMISSION (only when a card genuinely needs a not-yet-admitted package): do the folder-local parts NOW — add the package to the ' +
    'correct group in the target `README.md` and author the target `.api/<package>.md` from the published types via `uv run python -m ' +
    'tools.assay api` (or the package `.d.ts` in node_modules plus Context7/exa/tavily docs when assay is unavailable). The central ' +
    '`' + CENTRAL + '` catalog and the owning manifest\'s `catalog:` row (`libs/typescript/package.json` or the root `package.json`) have exactly ' +
    'ONE in-run writer: report the exact catalog pin + `catalog:` row in `pins` and never edit those shared manifests yourself. Never a per-area ' +
    '`package.json`.',
  'CARD CLOSURE (the area red-team ONLY — implement and critique NEVER change card status): a genuinely-complete card moves to its file ' +
    '`[02]-[CLOSED]` section as a collapsed one-liner `[ID]-[COMPLETE]: <one-line disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`); ' +
    'report the target `ARCHITECTURE.md` `[02]-[SEAMS]` row as {file, row} in `seams` ONLY when a real cross-folder seam landed; the run\'s ' +
    'terminal single-writer applies it, never you. A ripple-carrying card closes COMPLETE only when its seam is verified landed on BOTH ends on ' +
    'current disk; close only `strong` cards and honestly re-open the rest.',
].join('\n')

const BARHUNT = [
  'BAR — a high-value IMPLEMENT leaves every owner capturing the capability the card needs from the packages it admits, every sprawl collapsed ' +
    'into one denser owner with NO capability lost, and every fence transcription-complete against the published types. The critique guards ' +
    'capability conservation, charter completeness, and type-density; the red-team attacks every fence for a surface that could still collapse, a ' +
    'thin wrapper, a silent functionality drop during a refactor, a missed package capability the card needs, or a framework violation, and fixes ' +
    'each in place.',
  'HUNT (at implement, critique, and red-team alike): UNDER-CAPTURED CAPABILITY — an admitted package whose `.api`/types expose capability the ' +
    'CARD needs but no owner exploits is a gap, closed by deepening a fence. SURFACE SPRAWL — parallel interfaces/types/classes modelling one ' +
    'concept collapse into ONE polymorphic surface (tagged discriminated union + exhaustive `Match.exhaustive` dispatch) with no functionality removed. ' +
    'RAIL UNIFICATION — one entrypoint family per rail, one typed-error channel per domain (`Effect`/`Either`), total exhaustive handling. ' +
    'OPTIMIZATION — correctness first, then type-precision (branded/nominal, template-literal, conditional/mapped types) and runtime shape, not ' +
    'only line-count. NEW WORK SURFACED — api gaps and tasks the implementation exposes are realized the same turn.',
  'NAIVETY (two orthogonal axes, both intolerable at every stage): COVERAGE — the owner models a thin slice of its concept (the obvious three ' +
    'fields where the domain carries fifteen; a two-case family for a twenty-case domain) — deepen to the full concept NOW. APPROACH — enumerated ' +
    'hardcoded instances (a fixed roster of styles/patterns/variants spelled one-by-one) where a parameterized algorithmic owner should GENERATE ' +
    'the space — the roster is seed DATA feeding ONE generator over named parameters, never the mechanism itself; rebuild it so. COLLAPSE FLOOR — ' +
    'every enumerated hunt/collapse-signal list in this prompt is a FLOOR, never the complete set: any repeated structure, parallel spelling, or ' +
    'enumerable family an algebra, table, fold, or generator can own is a collapse target you find beyond the list.',
].join('\n')

const ULTRA = [
  'OPERATIVE DOCTRINE: docs/stacks/typescript/ is the route-owned law — hold every fence to it as fact; docs/stacks/csharp/ is the ' +
    'density/ambition FLOOR. Parallel interfaces/types/classes modelling one concept and sharing an identity regime, an admission path, a payload ' +
    'timing, or a consumer COLLAPSE into ONE polymorphic surface (tagged discriminated union + exhaustive match), never parallel names — a shape ' +
    'survives only on a genuinely distinct discriminant. Capability exits through FEW dense unified entry points — one polymorphic entry per rail ' +
    'discriminating on input shape (single|batch|stream absorbed by input detection, forward and inverse directions on one surface), variation ' +
    'living in input shape, policy values, and table rows, never parallel exports or modality-named siblings; the surface narrows by absorption, ' +
    'never by omission. AOP: cross-cutting concerns (retry, telemetry, validation, caching, receipts, ' +
    'fault rails) as Effect combinators/layers/decorators, not repeated inline. UNIFIED rails + UNIFIED pipelines + feature-arms-as-cases (never ' +
    'loose separate). Parameterize inputs AND outputs with generics at depth; no stringy/weak typing.',
  'LIFECYCLE SPINE (BOUNDARY_ADMISSION): every fence flows raw -> admit ONCE through a `Schema` parse at the boundary (parse, never trust input) ' +
    '-> canonical owner -> unified `Effect`/`Either`/`Option` rail -> projection -> egress, BOTH ingress and egress parameterized so the same ' +
    'owner sources and sinks across consumers without interior edits. Interior code never re-validates, never sees raw/untyped/provider shapes.',
  'STACK CAPABILITY (ultra-stacking): ENUMERATE both catalog tiers IN FULL from a real listing — `ls` the shared `' + SHARED_API + '/` AND ' +
    'the area `<area>/.api/`, never a memory-recall inventory — read every catalog the card touches, and mine each to OPERATOR DEPTH: the MOST ' +
    'powerful combinators each package reaches, composed into single dense rails, the shared Effect ecosystem ' +
    '(`Effect`/`Layer`/`Context`/`Schema`/`Stream`) layered end-to-end ON TOP OF the area-specific packages, NOT naive `Promise`/`try`-`catch` ' +
    'glue. An admitted capability the card needs that NO owner exploits is a defect closed by deepening a fence; capability the card needs ' +
    're-derived by hand is a defect; a cited member that cannot be verified against the published types is a PHANTOM — delete it and rebuild ' +
    'the fence on verified spellings. (Implement uses the capability the card needs; it does not max-stack every catalog for its own sake — ' +
    'that is rebuild.)',
  'PRESERVE all intended capability (densify, never delete functionality): capability is improved or extended, NEVER dropped for lack of a ' +
    'current consumer — zero consumers never lowers the bar; planned consumers are real design pressure. Where a fence is already strong, ' +
    'deepen; where it is flat/naive, rebuild ground-up. Never regress correctness or boundary law.',
].join('\n')

const PATLAW = [
  'TS PATTERN LAW (ultra-advanced ONLY; do not preserve the naive idioms of the existing code): ZERO `any`, zero implicit `any`, zero unsafe `as`, ' +
    'zero non-null `!`; model with exact discriminated unions under EXHAUSTIVE handling (`Match.exhaustive` or a checked `never` sink), ' +
    '`readonly`/`as const`, template-literal types, conditional/mapped types, and the `satisfies` operator. NO runtime `enum` — use `const` unions ' +
    'or `Schema`/Effect.',
  'COLLAPSE MANDATES: one `as const satisfies <Contract>` table is the single source of truth for a vocabulary — `typeof`/`keyof typeof`/' +
    'indexed-access derive the types FROM the value; a hand-written union a value table could derive is a defect. A concept exports ONE name ' +
    'serving value and type (declaration merging, `Schema.Class` owners, const+type pairing) — a consumer forced to alias, re-instantiate, or ' +
    '`typeof` at the call site is a defect; const type parameters, `NoInfer`, and instantiation expressions pre-solve inference at the owner. ' +
    'Wrapping/injection/decoration attach INLINE at the owner declaration, never as loose intermediate consts. One entrypoint owns every call ' +
    'modality via overload signatures or a discriminated input union; `Function.dual` gives pipe + direct call in one function. SCHEMA ' +
    'AUTHORITY: static types derive (`Schema.Schema.Type`/`Schema.Schema.Encoded`, `X.Type`), wire twins derive through `Schema.transform` — a ' +
    'hand-declared parallel interface/DTO beside a Schema owner is a defect. SHAPE BUDGET: one deep nested owner replaces 5+ loose ' +
    'interfaces/aliases/DTOs/brands; brands live INSIDE rich owners as Schema refinements, never free-floating. EXPORT LAW: no in-body ' +
    'exports — declarations are unexported and the file ends with ONE `// --- [EXPORTS]` block (`export { A, B }` + `export type { T, U }`) ' +
    'carrying the complete public surface; the block is itself a collapse target — sibling exports for one concept merge into ONE polymorphic ' +
    'export (modality discriminated inside on input shape; companions ride the same name via declaration merging), 1-2 exports per module the bar.',
  'Domain logic runs on typed-error rails — `Effect`/`Either`/`Option`, NEVER `throw` in domain code; boundaries validate through `Schema` (parse, ' +
    'never trust input). `import type`/`export type` are explicit; side-effect/value imports preserve runtime order. Per the ' +
    'docs/stacks/typescript file-organization overlay: `Effect.Service` owners are SERVICES, `Layer`/runtime wiring is COMPOSITION, runtime ' +
    'schemas/classes are MODELS, and catalog/registry rows stay after the owners they reference.',
  'Keep conventions IDENTICAL across every area so the corpus reads as one ultra-advanced codebase. One canonical semantic name per bounded ' +
    'concept; discriminate on input shape rather than proliferating `get`/`getMany`/`getById` names.',
].join('\n')

const BOUNDARIES = 'BOUNDARY LAW: keep every area owner strictly in its lane; internal code uses canonical names and shapes with mapping (and ' +
  '`Schema` validation) only at the edge; respect the dependency direction of the workspace. Cross-folder repair is seam-shaped: align ' +
  'counterparts, consumer sites, and counterpart cards — a concern owned twice across a runtime, an area mixing unrelated concerns, or coupling ' +
  'to a sibling owner INTERIOR (vs its wire/seam) is a defect.'

const CURRENT = 'CURRENT STATE — sibling area pipelines land work concurrently with yours. Before ANY edit, re-read the CURRENT on-disk state ' +
  'of your pages AND every sibling page your pages compose or ripple into; landed sibling work is composed as found, never assumed from the ' +
  'discovery map. A seam counterpart a sibling pipeline landed is COMPOSED, not re-derived; a conflict between your design and a landed sibling ' +
  'resolves to the STRONGER form, never a revert. Edit any potentially shared page with surgical anchored Edits only — re-read and re-apply on an ' +
  'edit conflict, never a whole-file rewrite.'

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

const DOCTRINE = [LAW, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', CURRENT, '', PROSE, '', COMMENTS].join('\n')

const GROUPNOTE = 'CONCURRENT CARD GROUP: sibling implement agents realize this area\'s OTHER page-disjoint card groups concurrently. Realize ' +
  'ONLY the cards in your worklist; touch any shared area surface (README.md, a page outside your group\'s anchored pages, a sibling area ' +
  'page) with surgical anchored Edits only, re-read and re-applied on conflict.'

// --- [OPERATIONS] ------------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms))
// One shared launch gate: chain heads and implement-fan members alike pass it, so every pooled start stays staggered.
let gate = Promise.resolve()
const stagger = () => { gate = gate.then(() => sleep(STAGGER_MS)); return gate }
const pool = async (items, cap, worker) => {
  const out = new Array(items.length)
  let next = 0
  const run = async () => { while (next < items.length) { const i = next++; await stagger(); out[i] = await worker(items[i], i) } }
  await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()))
  return out
}

// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// CODEX_DIR, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns a thin RECEIPT + jq-extracted skeleton — the product
// stays on disk for the folder's implement stage. It never does, edits, judges, or relays the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-')
const codexPrompt = (label, task, schema, writes) => {
  const base = CODEX_DIR + '/' + fileTag(label)
  const rpt = fileTag(label) + '-report.json' // unique per lane; pgrep matches the -o path on the codex cmdline
  const rptPat = '[' + rpt.slice(0, 1) + ']' + rpt.slice(1) // self-excluding pgrep/pkill pattern
  return ['DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and return a thin ' +
    'RECEIPT (plus the jq-extracted structural skeleton) for its on-disk report. Never perform, edit, judge, soften, ' +
    'summarize, or RELAY the work itself.',
  '(1) Files FIRST, with the WRITE TOOL — never a shell heredoc and never a relative path (cwd drift and heredoc quoting land files where codex cannot find them, killing every launch on a missing schema file). From the repository root (your starting cwd): mkdir -p ' + CODEX_DIR + '; purge stale lane artifacts (a leftover report would READY instantly with last run\'s data): rm -f ' + base + '-report.json ' + base + '-stderr.log; Write the TASK block below verbatim to ' + base + '-task.md; Write this JSON ' +
    'Schema exactly to ' + base + '-schema.json — both paths resolved ABSOLUTE under the repository root: ' + JSON.stringify(schema),
  '(2) Launch codex DETACHED from the repo root — ONE Bash call from the repo root, which FIRST verifies the files: test -s ' + base + '-task.md && test -s ' + base + '-schema.json || echo FILES-MISSING — on FILES-MISSING redo (1), NEVER launch without both. THEN the command below VERBATIM, never retyped or reflowed (every token matters: dropping </dev/null makes codex block forever on stdin, zero-CPU, no report): ' +
    'codex exec -s ' + (writes ? 'workspace-write' : 'read-only') + ' --skip-git-repo-check --ephemeral -c mcp_servers={} ' +
    '--output-schema ' + base + '-schema.json -o ' + base + '-report.json "Do the task in ' + base + '-task.md ' +
    'from the repository root. Final message: JSON per the output schema." </dev/null >/dev/null 2>' + base + '-stderr.log &',
  '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
    'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
    'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' + base + '-report.json ] && break; ' +
    'pgrep -f "' + rptPat + '" >/dev/null || break; sleep 20; done; if [ -s ' + base + '-report.json ]; then echo ' +
    'READY; elif pgrep -f "' + rptPat + '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
    'while it prints RUNNING; stop on READY; on GONE go to (4). LIVENESS IS NOT HEALTH: after the 4th RUNNING poll (~20 min wall) the run is WEDGED, not slow — kill it (pkill -f "' + rptPat + '") and go to (4) as GONE. Cap at 7 poll calls total.',
  '(4) READY: do NOT relay the report body through your output — build the MECHANICAL headline and the structural skeleton ' +
    'with jq (never your own judgment or reading): entries=$(jq \'(.tasks | length) + (.ideas | length)\' ' + base + '-report.json); ' +
    'statuses=$(jq -r \'[.tasks[].status, .ideas[].status] | group_by(.) | map("\\(.[0])x\\(length)") | join(",")\' ' + base + '-report.json); ' +
    'plan=$(jq -c \'{order, cards: ((.tasks + .ideas) | map({slug, pages})), gates, ripples, malformed: .malformed_ripples}\' ' + base + '-report.json). ' +
    'Return the RECEIPT: ok=true, report=' + base + '-report.json, entries=that count, headline="<entries> open cards | <statuses>", ' +
    'failure empty, and order/cards/gates/ripples/malformed transcribed EXACTLY from the $plan JSON — a mechanical copy, never your ' +
    'own reading of the report. GONE with no report: tail -5 ' + base + '-stderr.log FIRST — that tail IS the crash reason; relaunch ' +
    'the (2) command once (detached, never foreground) and resume polling; a second GONE returns ok=false, entries=0, report and ' +
    'headline empty, failure=the stderr tail in one line, and order/cards/gates/ripples/malformed all empty.',
  'TASK — write verbatim to the task file, then dispatch:',
  task].join('\n\n')
}

// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, the native inherit-model lane otherwise.
// The roster row carries `scope` from the ORCHESTRATOR (never the lane's self-report) so a failed lane's
// territory is exact even when the lane died before writing anything.
const recon = (task, o) => (CODEX
  ? agent(codexPrompt(o.label, task, o.schema, !!o.writes),
    { label: 'gpt-5.5:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: RECEIPT, stallMs: STALL })
  : agent(task + '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
    CODEX_DIR + '/' + fileTag(o.label) + '-report.json (Write tool, absolute path under the repo root): ' +
    JSON.stringify(o.schema) + ' — then return ONLY the receipt: ok, report path, entries = open tasks + ideas count, ' +
    'one-line mechanical headline (card count + status counts), failure empty, plus order, cards ({slug, pages} per open ' +
    'card), gates, ripples, and malformed (= malformed_ripples) transcribed exactly from the product.',
    { label: o.label, phase: o.phase, effort: 'high', schema: RECEIPT, stallMs: STALL })
).then((r) => ({ lane: o.label, scope: o.scope || [], ok: !!(r && r.ok && r.report), report: (r && r.report) || '',
  entries: (r && r.entries) || 0, headline: (r && r.headline) || '', failure: (r && r.failure) || (r ? '' : 'lane died'),
  order: (r && r.order) || [], cards: (r && r.cards) || [], gates: (r && r.gates) || [],
  ripples: (r && r.ripples) || [], malformed: (r && r.malformed) || [] }))
const folderName = (p) => p.split('/').filter(Boolean).pop() || p

// Page-disjointness is PROVEN, never assumed: every ordered card must carry >=1 verified page,
// gate pairs merge, and components pack heaviest-first into <= IMPL_FAN buckets without splitting.
const cardGroups = (t) => {
  const inOrder = new Set(t.order || [])
  const cards = (t.cards || []).filter((c) => inOrder.has(c.slug))
  if (cards.length < 2 || cards.some((c) => !(c.pages && c.pages.length))) return null
  const parent = new Map()
  const seed = (k) => { if (!parent.has(k)) parent.set(k, k) }
  const find = (k) => { let r = k; while (parent.get(r) !== r) r = parent.get(r); return r }
  const union = (a, b) => { seed(a); seed(b); const ra = find(a); const rb = find(b); if (ra !== rb) parent.set(ra, rb) }
  for (const c of cards) { seed('s:' + c.slug); for (const p of c.pages) union('s:' + c.slug, 'p:' + p) }
  for (const g of (t.gates || [])) if (inOrder.has(g.blocked_slug) && inOrder.has(g.gated_by_slug)) union('s:' + g.blocked_slug, 's:' + g.gated_by_slug)
  const comps = new Map()
  for (const c of cards) {
    const r = find('s:' + c.slug)
    if (!comps.has(r)) comps.set(r, { slugs: [], pages: new Set() })
    const g = comps.get(r); g.slugs.push(c.slug); for (const p of c.pages) g.pages.add(p)
  }
  if (comps.size < 2) return null
  const sorted = [...comps.values()].sort((a, b) => b.pages.size - a.pages.size || (a.slugs[0] < b.slugs[0] ? -1 : 1))
  const buckets = Array.from({ length: Math.min(IMPL_FAN, sorted.length) }, () => ({ slugs: [], pages: [] }))
  for (const c of sorted) { const b = buckets.reduce((m, x) => (x.pages.length < m.pages.length ? x : m)); b.slugs.push(...c.slugs); b.pages.push(...c.pages) }
  return buckets
}

const groupSeq = (t, g) => {
  const s = new Set(g.slugs)
  return JSON.stringify({
    order: (t.order || []).filter((x) => s.has(x)),
    cards: (t.cards || []).filter((c) => s.has(c.slug)),
    ripples: (t.ripples || []).filter((r) => s.has(r.from_slug)),
    gates: (t.gates || []).filter((x) => s.has(x.blocked_slug)),
  }, null, 1)
}

const discoverPrompt = (folder) => [LAW, '', CARD, '',
  'TASK: DISCOVER + SEQUENCE the open work of the single TypeScript session target `' + folder + '` (the full session target set, for ripple ' +
    'classification only: ' + JSON.stringify(TARGETS) + '). This is the read-only reconnaissance grounding this area\'s downstream chain, and ' +
    'read-only is its ONLY concession: resolve scope against real disk state (a real listing of the target folder, never memory), then read ' +
    '`' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md` IN FULL from disk — full-file reads, never a grep/skim; the design pages are ' +
    'downstream reading scope. Return: (1) folder — echo `' + folder + '` exactly; (2) tasks — EVERY open card in `TASKLOG.md` (status ' +
    'ACTIVE/QUEUED/BLOCKED; carry the Atomic flag); (3) ideas — the 1-3 MOST actionable open cards in `IDEAS.md` (tasks-first doctrine: at most ' +
    '3, the ones whose Anchors are most settled and whose ripples land on in-scope targets), HARD CAP 3; on EVERY task/idea row also return ' +
    'pages — the repo-relative design pages under `' + folder + '/.planning/**` the card\'s `Anchors:` bullet names, each VERIFIED to exist on ' +
    'disk with a listing (existence only — the pages stay downstream reading scope; empty when none verify): these rows prove page-disjoint card ' +
    'groups; (4) order — ONE sequenced slug list, ALL tasks first in dependency order then the chosen ideas; (5) ripples — for EVERY card ' +
    'carrying a `Ripple:` field, one row {from_slug, klass, to_pkg, to_slug}: klass=`in_scope` if to_pkg is one of the session targets, ' +
    '`oos_samelang` if it is another libs/typescript area, `cross_lang` if it points at `libs/csharp` / `libs/python` / ' +
    '`libs/typescript/.planning` / `libs/.planning`; confirm every counterpart ON DISK — open the named pkg card file and locate the mirror ' +
    'slug, never assert it from memory; (6) gates — for any [BLOCKED] card, {blocked_slug, gated_by_slug, in_scope} where in_scope is true iff ' +
    'the gating work is itself an open card in one of the session targets. Return malformed_ripples for any `Ripple:` line you cannot parse ' +
    'into a pkg+slug, or whose counterpart slug you cannot locate on disk — an unverified ripple row is a phantom that belongs in ' +
    'malformed_ripples, never in ripples; (7) coverage — part of the product: `requested` = your assigned scope (the two card files plus every ' +
    'counterpart card file the ripples point at), `read` = what you actually full-read, `skipped`/`unverified` = what you did not reach or ' +
    'could not confirm — an honest skip beats a silent one. Read the FULL card body (every bullet) to classify — never guess from the thesis. ' +
    'Your map carries NAVIGATION FACTS — paths, verified counterpart locations, seam targets — never verdicts, and it is an initial pointer, ' +
    'never a ceiling: downstream agents re-read every full card and page, and the map never licenses a downstream skim. Return the structured ' +
    'map ONLY; edit nothing (discovery is the sole read-only stage).'].join('\n')

const implementPrompt = (folder, rpt, seq, note) => [DOCTRINE, '',
  'TASK: IMPLEMENT — realize the open cards of `' + folder + '` into deep design-page FENCES at the ULTRA-ADVANCED bar. DISCOVERY REPORT: ' +
    'read ' + rpt + ' IN FULL from disk FIRST — the folder\'s navigation-facts reconnaissance (every open card with status/thesis/verified ' +
    'pages, ripple classifications, gates, malformed ripples, coverage): FACTS and jump coordinates, never verdicts and never a reading ' +
    'ceiling — spot-verify what you build on, hunt past it, and give its coverage `skipped`/`unverified` scope your own cold read. The ' +
    'sequenced worklist skeleton ' +
    '(your slugs + verified pages + ripple rows; read each FULL card body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`, never the skeleton alone):\n' + seq + '\nREAD: ' +
    'each card full body; every design page the card names under `' + folder + '/.planning/**`; the sibling pages it seams to, at their CURRENT ' +
    'on-disk state; the area-root `ARCHITECTURE.md` + `README.md`; the operative docs/stacks/typescript/ pages (docs/stacks/csharp/ as the ' +
    'ambition floor); BOTH .api tiers — the shared `' + SHARED_API + '/*.md` AND the area `' + folder + '/.api/*.md` (stack them, the shared ' +
    'Effect/Schema rails layered onto the area packages) — cross-checked against the published types in node_modules; verify a member via `uv ' +
    'run python -m tools.assay api` (assay unavailable: the `.d.ts` directly + both `.api` tiers + Context7/exa/tavily, never memory). ' +
    'Realize EVERY card in `order` (all tasks incl. Atomic, then the ideas) into deep fences in the `' + folder + '` design pages, in LIFECYCLE ' +
    'order (admit raw ONCE through a `Schema` parse at the boundary -> canonical owner -> weave every cross-cutting concern as an Effect ' +
    'combinator/layer/decorator over a thin pure core -> compose through ONE unified `Effect`/`Either`/`Option` rail -> project + egress, BOTH ' +
    'ingress and egress parameterized with generics at depth). Collapse parallel interfaces/types/classes into ONE tagged discriminated union with ' +
    'EXHAUSTIVE `Match.exhaustive` dispatch; Schema-refined brands where primitives are overloaded. ZERO `any`/implicit-any/unsafe `as`/non-null `!`, NO ' +
    'runtime `enum`, NO `throw` in domain logic, `import type` explicit. Resolve any [BLOCKED] card inline (read published types in node_modules / ' +
    '`assay api`). RIPPLES ARE YOURS: repair every ripple your cards carry in this same pass per the RIPPLE law — align each in-scope seam to the ' +
    'counterpart page as it NOW stands on disk, realize each 1-hop out-of-scope same-language counterpart card fence and align both ends, land ' +
    'your half of each cross-language seam stating the wire contract — and record each repair in `ripples`. PACKAGE ADMISSION per the card law: ' +
    'folder-local parts NOW, the central `' + CENTRAL + '` catalog pin + `catalog:` row reported in `pins`, never edited; a landed cross-folder ' +
    'seam\'s `ARCHITECTURE.md` `[02]-[SEAMS]` row reported in `seams`, never edited. Do NOT close any card — the area red-team owns card status. ' +
    'High-signal prose all-backticked, comment hygiene, fix-in-place (read-then-extend, preserve capability). Return verdict + realized slugs + ' +
    'deferred (any card you could not realize, with reason) + collapsed (before->after counts) + ripples + pins + seams + summary.' +
    (note ? '\n' + note : '')].join('\n')

const critiquePrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: DOCTRINAL-CONFORMANCE AUDIT + CHARTER-COMPLETENESS + FIX IN PLACE across `' + folder + '`. You are an ULTRA-HARSH, UNAGREEABLE auditor: ' +
    'is this TRULY ultra-advanced TS, or naive code in disguise? Assume a violation exists in every fence until you prove otherwise; "good enough" ' +
    'is rejected. The cards realized this turn (read each FULL body from `' + folder + '/IDEAS.md` + `' + folder + '/TASKLOG.md`):\n' + seq + '\nREAD ' +
    'the realized pages under `' + folder + '/.planning/**`, the sibling pages at their CURRENT on-disk state, docs/stacks/typescript/, and BOTH ' +
    '.api tiers (shared `' + SHARED_API + '` + area `' + folder + '/.api`). Run these MECHANICAL checklists line-by-line as a FLOOR you hunt PAST, ' +
    'and REPAIR every hit in place (a fix, never a ledger note):',
  '(1) COLLAPSE_SCAN — sibling prefix/suffix names -> one input-shape-discriminating entrypoint; parallel interfaces/types/classes for one ' +
    'concept sharing an identity regime, admission path, payload timing, or consumer -> ONE tagged discriminated union with exhaustive `match` ' +
    '(a shape survives only on a genuinely distinct discriminant); a `get`/`getMany`/`getById` family -> one input-keyed entrypoint; ' +
    'functions differing only by a literal -> parameterize as policy; a bool selecting two bodies -> one derived body/policy value; a function ' +
    'calling exactly one other -> delete the hop; parallel dispatch arms repeating structure -> a table or fold; co-occurring wrappers sharing ' +
    'an admission path -> one Effect combinator/layer aspect. These signals are a FLOOR, never the complete set — hunt collapse targets beyond them per the ' +
    'COLLAPSE FLOOR law.',
  '(2) SHAPE/BRANDING — model each concept as ONE polymorphic surface; brand/nominal types where a primitive is overloaded (ids, units, tokens); ' +
    'exact discriminated unions with a discriminant tag; `readonly`/`as const`/`satisfies`; template-literal/conditional/mapped types where they ' +
    'tighten. Kill every parallel interface, one-field wrapper, field-rename type, and stringly-typed shape.',
  '(3) KNOB_TEST — delete each parameter: if the value already encodes what it carried, it was a knob — collapse a `mode`/`strict`/`batch` flag ' +
    'into a policy value or input-shape discriminant; move every `timeout`/`retry`/`deadline` into an Effect combinator/layer, never a signature ' +
    'param.',
  '(4) ASPECTS — every cross-cutting concern (retry/telemetry/validation/caching/receipts/fault rails) MUST be an Effect ' +
    'combinator/`Layer`/decorator that never throws into domain flow; co-occurring wrappers sharing an admission path collapse into one aspect; deterministic layer ' +
    'order verified. Inline-repeated concerns and sibling helper functions are defects.',
  '(5) RAILS/BOUNDARIES — domain logic on `Effect`/`Either`/`Option`, NEVER `throw`; every boundary validates through `Schema` (parse, never trust ' +
    'input); the error channel is a typed union, not `unknown`/`Error`; accumulate-vs-abort disposition correct; `Layer`/`Context` for dependency ' +
    'wiring, `Stream` for streaming — no naive `Promise`/`async` glue where Effect belongs.',
  '(6) TYPING/MODERN — ZERO `any`/implicit-any/unsafe `as`/non-null `!`; NO runtime `enum` (use `const` unions or `Schema`); EXHAUSTIVE union ' +
    'handling via `Match.exhaustive`/checked `never` sink; `import type`/`export type` explicit; the file-organization overlay honored ' +
    '(`Effect.Service` -> SERVICES, ' +
    '`Layer` -> COMPOSITION, schemas/classes -> MODELS, catalogs after their owners); members cross-checked against the published node_modules types.',
  '(7) CHARTER-COMPLETENESS — for EVERY card in the worklist, verify the realized fences GENUINELY fulfill its `Capability`/`Shape`/`Unlocks` ' +
    '(read the full card from disk): a missing modality, an unrealized `Shape` clause, a stubbed/placeholder fence, or a capability the card ' +
    'promises but the fences do not deliver is a DEFECT — realize it NOW. A card whose fences are thin against its charter is not done.',
  '(8) NAIVETY — both axes per fence: a COVERAGE thin-slice owner (a fraction of the concept modeled where the domain carries far more) is ' +
    'deepened to the full concept NOW; an APPROACH enumerated roster (fixed instances spelled one-by-one where ONE parameterized generator ' +
    'should own the space) is rebuilt as seed data feeding one generator.',
  '(9) SEAMS — check every cross-page and cross-area symbol these cards compose against the counterpart as it NOW stands on disk: a signature ' +
    'mismatch corrects at the weaker end, a conflict resolves to the stronger form, never a revert; a seam counterpart or consumer site your fix ' +
    'exposes is repaired in this same pass wherever it lives, recorded in `ripples` (an area `ARCHITECTURE.md` `[02]-[SEAMS]` row change is ' +
    'reported in `seams` for the terminal single-writer, never edited directly).',
  'Also enforce the ultra-stacking law (both `.api` tiers enumerated in full; a thin area-only subset ignoring the shared Effect/Schema rails ' +
    'the card needs is a defect; a cited member you cannot verify against the published types is a phantom — delete it), cross-area convention ' +
    'consistency per the doctrine, and prose + comment hygiene. FIX every hit NOW wherever it lives per WRITE-FULLY; report any central ' +
    '`' + CENTRAL + '` row in `pins`. Return verdict + realized + deferred + collapsed + ripples + pins + seams + summary.'].join('\n')

const redteamPrompt = (folder, seq) => [DOCTRINE, '',
  'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE + TERMINAL CLOSE across `' + folder + '`. You are a HOSTILE principal reviewer whose explicit goal ' +
    'is to REJECT this design as naive TypeScript: assume it is junior, under-typed, or wrong until the fences prove otherwise; the burden of ' +
    'proof is ON THE DESIGN. The cards realized this turn (read each FULL body):\n' + seq + '\nRead docs/stacks/typescript/, the published ' +
    'library types, BOTH .api tiers (shared `' + SHARED_API + '` + area `' + folder + '/.api`), and the sibling pages at their CURRENT on-disk ' +
    'state. Attack relentlessly and REPAIR every defect in place — no soft-pedalling, no could/should, a fix never a ledger.',
  'PRIMARY LENS — fundamental design, multi-faceted: (A) COUNTERFACTUAL on the core choice — is the owner, the union algebra, and the dispatch ' +
    'form categorically the strongest the doctrine admits, or does a denser tagged-union owner, a branded type, or a DEEPER Effect/Schema ' +
    'primitive collapse the whole fence? If a fundamentally stronger design exists, rebuild to it — never defend the incumbent. (B) ' +
    'ANTICIPATORY_COLLAPSE — compute the DIFF OF THE NEXT FEATURE: when the next case/dimension/modality/provider arrives, does it land as ONE ' +
    'union case with every consumer broken loudly at type-check (exhaustive dispatch)? If it would touch multiple sites, reshape so the ' +
    'growth axis is a case, row, policy value, or layer swap. (C) LONG-TAIL + MULTI-DIMENSIONAL — attack every input/output/edge/failure mode ' +
    '(empty, singular, plural, stream, malformed, concurrent, cancelled, partial-failure, version-skew); is the boundary `Schema`-parsed; are BOTH ' +
    'ingress AND egress parameterized so this owner sources and sinks across consumers without interior edits? (D) BOUNDARY-INTEGRITY — a concern ' +
    'owned twice in a runtime, an area mixing concerns, coupling to a sibling owner INTERIOR (vs its wire/seam), OR a sibling planning page left ' +
    'STALE by this area change even when no ripple card names it (ports/boundaries/wires/seams drift) is a defect: fix it NOW wherever it lives — ' +
    'the stale sibling page, the seam counterpart, the consumer site — and record the repair in `ripples` (an area `ARCHITECTURE.md` ' +
    '`[02]-[SEAMS]` row change is reported in `seams` for the terminal single-writer, never edited directly). (E) SURFACE-SPRAWL-IN-TIME — ' +
    '`any`/implicit-any/unsafe `as`/non-null `!`; `throw` in domain logic instead of `Effect`/`Either`; non-exhaustive union handling; runtime ' +
    '`enum`; missing branded types; loose `import` where `import type` is required; naive `Promise`/`async` glue where `Effect`/`Layer` belongs; ' +
    'a thin area-only `.api` subset ignoring the shared Effect/Schema rails the card needs; an unvalidated boundary (no `Schema` parse); a ' +
    'phantom member: for EACH, state the concrete failure (what breaks, under which input/edge) and repair it in place. (F) NAIVETY-AXES — attack ' +
    'COVERAGE (the owner models a thin slice of its concept where the domain carries far more — deepen to the full concept) and APPROACH (a fixed ' +
    'enumerated roster of instances where ONE parameterized generator should own the space — demote the roster to seed DATA feeding one ' +
    'generator; the roster is never the mechanism).',
  'ALSO — FULL COLD ADVERSARIAL RE-REVIEW (every time): re-attack every conformance dimension with fresh hostile eyes, trusting nothing the prior ' +
    'passes claimed — COLLAPSE_SCAN, shape/branding, the KNOB_TEST per param, the Effect-aspect taxonomy, rail + Schema-boundary discipline, ' +
    'charter-completeness per card, the two NAIVETY axes, the zero-any/throw/enum typing law, both-tier `.api` use at operator depth (phantom ' +
    'members deleted), the file-organization overlay, and prose/comment hygiene — a FLOOR list you hunt past — and fix every defect. Even ' +
    'absent a structural rebuild, the fences must end objectively denser, more type-safe, and more powerful than the critique left them; if the ' +
    'strongest form is genuinely already present, prove it by finding nothing — never invent churn.',
  'TERMINAL CLOSE — you are `' + folder + '`\'s LAST stage and the SOLE owner of its card status. For EVERY card in scope this run, re-read its ' +
    'FULL body and the realized fences on CURRENT disk, then ADVERSARIALLY VERIFY — the fences are naive until they survive your attack, a prior ' +
    'pass verdict a rejected self-assessment — that they genuinely fulfill the card `Capability`/`Shape`/`Unlocks` against the published types ' +
    '(verify a member via `uv run python -m tools.assay api`; assay unavailable — the published `.d.ts` + both `.api` tiers + ' +
    'Context7/exa/tavily). FINAL-remediate any weak or partial realization in place NOW, then assign each card a strength: `strong` (every ' +
    'charter clause delivered, fences transcription-complete and fully type-safe against the published types), `partial` (most delivered, a ' +
    'clause still thin), `weak` (charter not met). CLOSE only `strong` cards per the CARD CLOSURE law; a ripple card whose seam you cannot ' +
    'verify landed on BOTH ends on current disk stays OPEN with that reason; honestly RE-OPEN every card you cannot bring to `strong`, with a ' +
    'one-line reason (a real out-of-run or cross-language dependency). The orchestrator DEMOTES any card closed below `strong`, so never ' +
    'inflate. Return verdict + realized + deferred + collapsed + ripples + pins + seams + closed [{slug, disposition, strength}] + reopened ' +
    '[{slug, reason}] + summary.'].join('\n')

const pinPrompt = (pins, seams) => [LAW, '', PROSE, '',
  'TASK: TERMINAL SINGLE-WRITER — you are the run\'s SOLE writer for the shared manifests (`' + CENTRAL + '` catalog + the owning `catalog:` ' +
    'manifest, `libs/typescript/package.json` or the root `package.json`) and for every area `ARCHITECTURE.md` `[02]-[SEAMS]` section, and its ' +
    'LAST agent. PINS: apply each reported catalog pin + `catalog:` row below exactly once, preserving the existing group/order and deduping ' +
    'semantically identical rows; verify each package + version against the published registry (Context7/exa/tavily against the official package ' +
    'docs; `uv run python -m tools.assay api` over node_modules once installed) before applying; confirm the area README group and ' +
    '`.api/<package>.md` catalog landed, repairing a missing folder-local part in place. SEAM ROWS: upsert each reported {file, row} into the ' +
    'named file\'s `[02]-[SEAMS]` section exactly once, preserving the section\'s row format and order and deduping semantically identical rows; ' +
    'a missing file or absent `[02]-[SEAMS]` section rejects the row. Reject any unverifiable or malformed row as {target, reason} — never apply ' +
    'it silently. PINS:\n' + JSON.stringify(pins, null, 1) + '\nSEAM ROWS:\n' + JSON.stringify(seams, null, 1)].join('\n')

// --- [COMPOSITION] -----------------------------------------------------------------------

phase('Realize')
log('Pooling ' + TARGETS.length + ' folder chain(s) at CAP=' + CAP)
const runFolder = async (target) => {
  const tag = folderName(target)
  try {
    const t = await recon(discoverPrompt(target), { label: 'discover:' + tag, phase: 'Realize', schema: DISCOVERY_SCHEMA, scope: [target] })
    if (!t.ok) return { folder: target, failed: true, empty: false, logs: [], red: null, cross_lang: [], malformed: [], error: t.failure }
    const cross = (t.ripples || []).filter((rp) => rp.klass === 'cross_lang').map((rp) => tag + ' [' + rp.from_slug + '] -> ' + rp.to_pkg + ' [' + rp.to_slug + ']')
    const malformed = t.malformed || []
    if (!t.entries) return { folder: target, failed: false, empty: true, logs: [], red: null, cross_lang: cross, malformed }
    const seq = JSON.stringify({ order: t.order, cards: t.cards, ripples: t.ripples, gates: t.gates }, null, 1)
    const groups = cardGroups(t)
    let impls
    if (groups) {
      log(tag + ': implement fan over ' + groups.length + ' page-disjoint group(s); page weights ' + groups.map((g) => g.pages.length).join('/'))
      impls = (await parallel(groups.map((g, gi) => async () => { await stagger(); return agent(implementPrompt(target, t.report, groupSeq(t, g), GROUPNOTE), { label: 'implement:' + tag + ':g' + gi, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: STALL }) }))).filter(Boolean)
    } else {
      const one = await agent(implementPrompt(target, t.report, seq, ''), { label: 'implement:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: STALL })
      impls = one ? [one] : []
    }
    if (!impls.length) return { folder: target, failed: true, empty: false, logs: [], red: null, cross_lang: cross, malformed } // failure isolation: a dead implement skips its reviews
    const crit = await agent(critiquePrompt(target, seq), { label: 'critique:' + tag, phase: 'Realize', schema: FIXLOG_SCHEMA, effort: 'high', stallMs: STALL })
    const red = await agent(redteamPrompt(target, seq), { label: 'redteam:' + tag, phase: 'Realize', schema: REDTEAM_SCHEMA, effort: 'high', stallMs: STALL })
    return { folder: target, failed: false, empty: false, logs: [...impls, crit, red].filter(Boolean), red, cross_lang: cross, malformed }
  } catch (e) {
    return { folder: target, failed: true, empty: false, logs: [], red: null, cross_lang: [], malformed: [], error: String((e && e.message) || e) } // failure isolation: one thrown chain never rejects the pool
  }
}
const done = (await pool(TARGETS, CAP, runFolder)).filter(Boolean)
const failed = done.filter((r) => r.failed).map((r) => r.folder)
const emptyFolders = done.filter((r) => r.empty).map((r) => r.folder)
const active = done.filter((r) => !r.failed && !r.empty)
const crossLang = done.flatMap((r) => r.cross_lang || [])
const deferred = done.flatMap((r) => r.logs.flatMap((l) => (l.deferred || []).map((d) => ({ folder: r.folder, slug: d.slug, reason: d.reason }))))
const ripplesRepaired = done.flatMap((r) => r.logs.flatMap((l) => l.ripples || []))
const pinsReported = [...new Map(done.flatMap((r) => r.logs.flatMap((l) => l.pins || [])).map((p) => [p.package + '|' + p.row, p])).values()]
const seamsReported = [...new Map(done.flatMap((r) => r.logs.flatMap((l) => l.seams || [])).map((s) => [s.file + '|' + s.row, s])).values()]
let closed_count = 0
const reopened = []
for (const r of done) {
  for (const c of ((r.red && r.red.closed) || [])) {
    if (c.disposition === 'complete' && c.strength !== 'strong') reopened.push({ folder: r.folder, slug: c.slug, reason: 'demoted: strength=' + c.strength })
    else closed_count++
  }
  for (const ro of ((r.red && r.red.reopened) || [])) reopened.push({ folder: r.folder, slug: ro.slug, reason: ro.reason })
}
log('Realize: ' + active.length + '/' + TARGETS.length + ' folder(s) realized (' + emptyFolders.length + ' empty); ' + closed_count +
  ' cards closed, ' + reopened.length + ' re-open/demoted; ' + ripplesRepaired.length + ' ripple repair(s); ' + pinsReported.length +
  ' pin(s) + ' + seamsReported.length + ' seam row(s) reported' + (failed.length ? '; failed: ' + failed.join(', ') : ''))

// --- [PINS]

let pinlog = null
if (pinsReported.length || seamsReported.length) {
  phase('Pins')
  pinlog = await agent(pinPrompt(pinsReported, seamsReported), { label: 'pins', phase: 'Pins', schema: PIN_SCHEMA, model: 'sonnet', effort: 'high', stallMs: STALL })
}

return {
  root: ROOT,
  targets: TARGETS,
  realized_folders: active.map((r) => r.folder),
  empty_folders: emptyFolders,
  realize_failed: failed,
  deferred,
  ripples_repaired: ripplesRepaired.length,
  closed_count,
  reopened,
  pins: { reported: pinsReported.length, seams_reported: seamsReported.length, applied: (pinlog && pinlog.applied) || [], seam_rows_applied: (pinlog && pinlog.seam_rows_applied) || [], rejected: (pinlog && pinlog.rejected) || [] },
  cross_language: crossLang,
  malformed_ripples: done.flatMap((r) => r.malformed || []),
}
