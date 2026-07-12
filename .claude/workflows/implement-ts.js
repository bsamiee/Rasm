export const meta = {
    name: 'implement-ts',
    whenToUse: 'Realize open cards into design-page code fences across the TypeScript target folders.',
    description:
        "Realize every open IDEAS/TASKLOG card across the libs/typescript capability folders (core, security, data, runtime, ui, iac) into deep design-page code FENCES at the docs/stacks/typescript bar (Effect-TS rails, Schema-first boundaries, one canonical owner per concept, exhaustive discriminated unions, zero any/throw/enum), repair every ripple in-pass, and truthfully close the cards. Each target folder runs its OWN discover -> implement -> critique -> redteam chain, ALL chains concurrent under one pooled cap: a folder starts the moment its own discovery lands, a folder with no open cards no-ops after its own discovery, and a failed chain isolates without rejecting the pool. Discovery hands downstream stages navigation FACTS (paths, verified members, seam targets) and never verdicts; it runs read-only on gpt-5.6-terra dispatched through a sonnet codex wrapper (CODEX flag; false restores the native inherit-model lane), lands its full product on disk under .claude/scratch/implement-ts, and returns a thin receipt plus a jq-extracted structural skeleton on the wire; the implement stage reads the report IN FULL from disk, and when the skeleton proves page-disjoint card groups it fans over them. Every stage WRITES and repairs the page-level ripples its own work exposes in the same pass — in-scope seams aligned against current disk, 1-hop out-of-scope same-language counterpart fences realized directly — with BLOCKED probes and folder-local package admission inline. The redteam is each folder chain's terminal stage and sole card-status owner: it final-remediates weak realizations in place and closes only cards whose realization it verified strong on disk. Two handoffs route to the run's terminal single-writer, the central pnpm-workspace.yaml catalog pin (+ its catalog: manifest row) and the area ARCHITECTURE.md [02]-[SEAMS] row: folder agents report exact rows, one terminal opus writer applies them serially. Every writing stage also nominates generalizable lessons into a required-usually-empty harvest, folded forward through the redteam; the terminal stage is a DRAIN LOOP over the pooled deferred backlog plus orphaned critique fixlogs that also applies the central pins and ARCHITECTURE seam rows and re-feeds the still-open remainder under a round cap + no-shrinkage progress gate, then one fable doctrine lander adjudicates the pooled harvest against the docs/laws admission bar (land-nothing legal) before the run closes. Card-driven (it implements ideas/tasks), NOT the in-isolation api-stacking of rebuild-api. TypeScript-only. args = a target path string, an array of paths, or empty for all capability folders. The language-wide libs/typescript/.planning is out of scope.",
    phases: [
        {
            title: 'Realize',
            detail: 'all folder chains concurrent under one pooled cap: discover(gpt-5.6-terra via codex wrapper, read-only; full product to disk, thin receipt + structural skeleton on the wire) -> implement(fable; reads the discovery report from disk, fans over discovery-proven page-disjoint card groups) -> critique(ONE gpt-5.6-sol codex lane, workspace-write; fixlog to disk, receipt on the wire) -> redteam(fable, terminal close; reads the critique fixlog from disk as refutation targets and folds its surviving ripples/pins/seams/deferred rows into its own return); a folder with no open cards no-ops after its own discovery; every writing stage re-reads current disk, repairs page-level ripples in-pass, and reports central pin rows + ARCHITECTURE.md [02]-[SEAMS] rows for the terminal single-writer instead of editing those surfaces',
        },
        {
            title: 'Pins',
            detail: 'a terminal DRAIN LOOP: one serial opus single-writer per round applies every reported pnpm-workspace.yaml catalog pin + its catalog: row in the owning package.json and every reported ARCHITECTURE.md [02]-[SEAMS] row, drains the orphaned critique fixlogs (a folder whose redteam died) and the pooled deferred backlog against live disk, and re-feeds the still-open remainder under a round cap + no-shrinkage progress gate; then one fable doctrine lander adjudicates the pooled harvest nominations against the docs/laws admission bar (land-nothing legal). Runs only when pins, seams, orphans, backlog, or harvest exist',
            model: 'opus',
        },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAP = 14; // concurrent folder-CHAIN ceiling — the default target sets run below it; it binds only when args name more folders than CAP
const IMPL_FAN = 3; // max implement agents fanned per folder, and only over discovery-proven page-disjoint card groups
const STAGGER_MS = 1500;
const STALL = 300000;
const DRAIN_ROUNDS = 4; // terminal drain fixpoint cap; the no-shrinkage progress gate (no remaining shrinkage -> stop) is the real bound
const CODEX_STALL = 1500000; // wrapper stall sits above the xhigh blocking-call ceiling (1200s): a silent live MCP call is legal waiting, never a stall
const SOL_STALL = 2400000; // sol critique holds one long blocking MCP call at the operator-default tier; stall detection must outlast it
const ROOT = 'libs/typescript';
const SHARED_API = 'libs/typescript/.api';
const CENTRAL = 'pnpm-workspace.yaml';
const DEFAULT_TARGETS = [
    'libs/typescript/core',
    'libs/typescript/security',
    'libs/typescript/data',
    'libs/typescript/runtime',
    'libs/typescript/ui',
    'libs/typescript/iac',
];
const CODEX = true;
const CODEX_DIR = '.claude/scratch/implement-ts'; // per-lane MCP reports

// --- [INPUTS] --------------------------------------------------------------------------

const norm = (t) => {
    const s = String(t).trim();
    return s.indexOf('libs/') === 0 ? s : ROOT + '/' + s;
};
const TARGETS = Array.isArray(args)
    ? args.filter(Boolean).map(norm)
    : args && typeof args === 'object' && Array.isArray(args.targets)
      ? args.targets.filter(Boolean).map(norm)
      : typeof args === 'string' && args.trim() && args.trim().toUpperCase() !== 'ALL'
        ? [norm(args)]
        : DEFAULT_TARGETS;

// --- [MODELS] --------------------------------------------------------------------------

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const ANCHOR = {
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: { type: 'string' },
        line: { type: 'integer' },
        role: { type: 'string', enum: ['state', 'ruling', 'catalog', 'counterpart', 'absence'] },
        note: { type: 'string' },
    },
};

// Per-folder discovery PRODUCT (the on-disk report): `pages` per card are disk-verified Anchors
// targets proving page-disjoint implement groups; `malformed_ripples` is a required attestation
// (empty = none found); `coverage` is part of the product — requested vs actually-read scope.
const RIPPLE_ROW = {
    type: 'object',
    additionalProperties: false,
    required: ['from_slug', 'klass', 'to_pkg', 'to_slug'],
    properties: {
        from_slug: { type: 'string' },
        klass: { type: 'string', enum: ['in_scope', 'oos_samelang', 'cross_lang'] },
        to_pkg: { type: 'string' },
        to_slug: { type: 'string' },
    },
};
const GATE_ROW = {
    type: 'object',
    additionalProperties: false,
    required: ['blocked_slug', 'gated_by_slug', 'in_scope'],
    properties: { blocked_slug: { type: 'string' }, gated_by_slug: { type: 'string' }, in_scope: { type: 'boolean' } },
};
const MALFORMED_ROW = {
    type: 'object',
    additionalProperties: false,
    required: ['from_slug', 'raw'],
    properties: { from_slug: { type: 'string' }, raw: { type: 'string' } },
};
const CARD_REF = {
    type: 'object',
    additionalProperties: false,
    required: ['slug', 'pages'],
    properties: { slug: { type: 'string' }, pages: { type: 'array', items: { type: 'string' } } },
};

const DISCOVERY_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['folder', 'order', 'tasks', 'ideas', 'ripples', 'malformed_ripples', 'gates', 'map', 'coverage'],
    properties: {
        folder: { type: 'string' },
        order: { type: 'array', items: { type: 'string' } },
        tasks: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['slug', 'status', 'pages', 'atomic', 'thesis'],
                properties: {
                    slug: { type: 'string' },
                    status: { type: 'string' },
                    atomic: { type: 'boolean' },
                    thesis: { type: 'string' },
                    pages: { type: 'array', items: { type: 'string' } },
                },
            },
        },
        ideas: {
            type: 'array',
            maxItems: 3,
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['slug', 'status', 'pages', 'thesis'],
                properties: {
                    slug: { type: 'string' },
                    status: { type: 'string' },
                    thesis: { type: 'string' },
                    pages: { type: 'array', items: { type: 'string' } },
                },
            },
        },
        ripples: { type: 'array', items: RIPPLE_ROW },
        gates: { type: 'array', items: GATE_ROW },
        map: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'files', 'anchors', 'members', 'composed', 'underutilized', 'seams', 'stacking'],
                properties: {
                    page: { type: 'string' },
                    files: { type: 'array', items: { type: 'string' } }, // files the implementer must open for this row
                    anchors: { type: 'array', items: ANCHOR }, // exact coordinates backing the row's facts
                    members: { type: 'array', items: { type: 'string' } }, // verified member spellings backing `underutilized`
                    composed: { type: 'string' },
                    underutilized: { type: 'string' },
                    seams: { type: 'string' },
                    stacking: { type: 'string' },
                },
            },
        },
        malformed_ripples: { type: 'array', items: MALFORMED_ROW },
        coverage: {
            type: 'object',
            additionalProperties: false,
            required: ['requested', 'read', 'skipped', 'unverified'],
            properties: {
                requested: { type: 'array', items: { type: 'string' } },
                read: { type: 'array', items: { type: 'string' } },
                skipped: { type: 'array', items: { type: 'string' } },
                unverified: { type: 'array', items: { type: 'string' } },
            },
        },
    },
};

// Thin wire receipt + mechanically-extracted skeleton: the discovery PRODUCT (statuses, theses,
// coverage, the full navigation dossier) stays on disk at `report`; only status + count + headline
// plus the structural rows the orchestrator fans and seams over (order/cards/gates/ripples/malformed)
// travel inline. `cards` = {slug, pages} per open card — the page-disjointness proof, nothing more.
const RECEIPT = {
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'headline', 'failure', 'order', 'cards', 'gates', 'ripples', 'malformed'],
    properties: {
        ok: { type: 'boolean' },
        report: { type: 'string' },
        entries: { type: 'integer' },
        headline: { type: 'string' },
        failure: { type: 'string' },
        order: { type: 'array', items: { type: 'string' } },
        cards: { type: 'array', items: CARD_REF },
        gates: { type: 'array', items: GATE_ROW },
        ripples: { type: 'array', items: RIPPLE_ROW },
        malformed: { type: 'array', items: MALFORMED_ROW },
    },
};

// Thin wire receipt for the sol critique lane: its FIXLOG product stays on disk at `report`.
const LANE_RECEIPT = {
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'headline', 'failure'],
    properties: {
        ok: { type: 'boolean' },
        report: { type: 'string' },
        entries: { type: 'integer' },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};

// Required-but-possibly-empty `ripples`/`pins`/`seams` are attestations: ripple repair ran in-pass,
// and pin + seam rows are the run's only single-writer handoffs — empty attests none arose, never a skip.
const RIPPLES = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['counterpart', 'action'],
        properties: { counterpart: { type: 'string' }, action: { type: 'string' } },
    },
};
const PINS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['package', 'row'],
        properties: { package: { type: 'string' }, row: { type: 'string' } },
    },
};
const SEAMS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['file', 'row'],
        properties: { file: { type: 'string' }, row: { type: 'string' } },
    },
};

// The counted backlog: cards a folder chain could not realize, drained by the terminal loop and re-fed as `remaining`.
const DEFERRED = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['files', 'claim'],
        properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } },
    },
};

// Doctrine nominations — generalizable lessons only; the terminal doctrine lander adjudicates every row.
const HARVEST = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['altitude', 'lang', 'claim', 'anchors', 'existingClause'],
        properties: {
            altitude: { type: 'string', enum: ['stacks', 'reviewer', 'constitution', 'planning', 'readme', 'laws'] },
            lang: { type: 'string' },
            claim: { type: 'string' },
            anchors: { type: 'array', items: { type: 'string' } },
            existingClause: { type: 'string' },
        },
    },
};

const FIXLOG_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['folder', 'verdict', 'ripples', 'pins', 'seams', 'harvest', 'summary', 'realized', 'deferred', 'collapsed'],
    properties: {
        folder: { type: 'string' },
        verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
        realized: { type: 'array', items: { type: 'string' } },
        deferred: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['slug', 'reason'],
                properties: { slug: { type: 'string' }, reason: { type: 'string' } },
            },
        },
        collapsed: { type: 'string' },
        ripples: RIPPLES,
        pins: PINS,
        seams: SEAMS,
        harvest: HARVEST,
        summary: { type: 'string' },
    },
};

const REDTEAM_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['folder', 'verdict', 'ripples', 'pins', 'seams', 'harvest', 'closed', 'reopened', 'summary', 'realized', 'deferred', 'collapsed'],
    properties: {
        folder: { type: 'string' },
        verdict: { type: 'string', enum: ['realized', 'refined', 'clean'] },
        realized: { type: 'array', items: { type: 'string' } },
        deferred: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['slug', 'reason'],
                properties: { slug: { type: 'string' }, reason: { type: 'string' } },
            },
        },
        collapsed: { type: 'string' },
        ripples: RIPPLES,
        pins: PINS,
        seams: SEAMS,
        harvest: HARVEST,
        closed: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['slug', 'disposition', 'strength'],
                properties: {
                    slug: { type: 'string' },
                    disposition: { type: 'string', enum: ['complete', 'dropped'] },
                    strength: { type: 'string', enum: ['strong', 'partial', 'weak'] },
                },
            },
        },
        reopened: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['slug', 'reason'],
                properties: { slug: { type: 'string' }, reason: { type: 'string' } },
            },
        },
        summary: { type: 'string' },
    },
};

const PIN_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['applied', 'seam_rows_applied', 'rejected', 'remaining', 'harvest', 'summary'],
    properties: {
        applied: { type: 'array', items: { type: 'string' } },
        harvest: HARVEST,
        seam_rows_applied: { type: 'array', items: { type: 'string' } },
        rejected: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['target', 'reason'],
                properties: { target: { type: 'string' }, reason: { type: 'string' } },
            },
        },
        remaining: DEFERRED, // rows verified still-open and genuinely blocked; the drain loop re-feeds them until empty or no progress
        summary: { type: 'string' },
    },
};

const DOCTRINE_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['landed', 'refined', 'rejected', 'files', 'summary'],
    properties: {
        landed: { type: 'array', items: { type: 'string' } },
        refined: { type: 'array', items: { type: 'string' } },
        rejected: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['claim', 'reason'],
                properties: { claim: { type: 'string' }, reason: { type: 'string' } },
            },
        },
        files: { type: 'array', items: { type: 'string' } },
        summary: { type: 'string' },
    },
};

// --- [DOCTRINE] ------------------------------------------------------------------------

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
    'TWO-TIER .api: every fence draws on BOTH the shared/universal catalogs at `' +
        SHARED_API +
        '/*.md` (enumerated from disk, never from ' +
        'memory) AND the area-specific catalogs at ' +
        '`<area>/.api/*.md`. The shared Effect substrate tier is SHARED capability you MUST consider and compose to realize the card properly — ' +
        'never hand-roll `Promise`/`try`-`catch` glue or settle for a thin area-only subset; layer the shared Effect ecosystem ' +
        '(`Effect`/`Layer`/`Context`/`Schema`/`Stream`) end-to-end ON TOP OF the area-specific packages. This is implement (use the capability the ' +
        'card needs), not rebuild (max-stack every catalog for its own sake).',
    'WRITE-FULLY + FIX-IT-NOW: every fix you identify you MUST make NOW via Edit/Write directly in the file — the structured fix-log you return is a ' +
        'REPORT of edits ALREADY MADE, never a to-do list, a ledger, or a would/should-fix hedge. The writing is YOURS: never delegate authoring to ' +
        'another agent — a delegate may only fetch information. A cross-file ripple your edit exposes is YOURS in the ' +
        'same pass, wherever it lives: the seam counterpart on both ends, the consumer site, the stale sibling page, the 1-hop counterpart card fence ' +
        'in another libs/typescript area — repaired now and recorded in `ripples` (an empty `ripples` attests your pass exposed none, never that ' +
        "repair was skipped). TWO handoffs route to the run's terminal single-writer and are NEVER edited by a folder agent: the central " +
        '`' +
        CENTRAL +
        '` catalog pin + its `catalog:` manifest row (report the exact rows in `pins`) and any area `ARCHITECTURE.md` `[02]-[SEAMS]` ' +
        'row (report {file, row} in `seams` — the highest-collision shared surface); every other page-level ripple stays yours, repaired distributed ' +
        'under the anchored-Edit discipline. If after real investigation a fence is already correct, say so — never invent edits to look busy.',
].join('\n');

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
        "ripple's scope is that counterpart card and its seam, not the foreign area's other cards), CROSS-LANGUAGE / LIB-WIDE (`libs/csharp`, " +
        "`libs/python`, `libs/typescript/.planning`, `libs/.planning` — outside this TypeScript-only run's language rail; land your half stating the " +
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
        '`' +
        CENTRAL +
        "` catalog and the owning manifest's `catalog:` row (`libs/typescript/package.json` or the root `package.json`) have exactly " +
        'ONE in-run writer: report the exact catalog pin + `catalog:` row in `pins` and never edit those shared manifests yourself. Never a per-area ' +
        '`package.json`.',
    'CARD CLOSURE (the area red-team ONLY — implement and critique NEVER change card status): a genuinely-complete card moves to its file ' +
        '`[02]-[CLOSED]` section as a collapsed one-liner `[ID]-[COMPLETE]: <one-line disposition>; Ripple: <pkg> [SLUG]` (or `[DROPPED]: <reason>`); ' +
        "report the target `ARCHITECTURE.md` `[02]-[SEAMS]` row as {file, row} in `seams` ONLY when a real cross-folder seam landed; the run's " +
        'terminal single-writer applies it, never you. A ripple-carrying card closes COMPLETE only when its seam is verified landed on BOTH ends on ' +
        'current disk; close only `strong` cards and honestly re-open the rest.',
].join('\n');

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
].join('\n');

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
    'STACK CAPABILITY (ultra-stacking): ENUMERATE both catalog tiers IN FULL from a real listing — `ls` the shared `' +
        SHARED_API +
        '/` AND ' +
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
].join('\n');

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
].join('\n');

const BOUNDARIES =
    'BOUNDARY LAW: keep every area owner strictly in its lane; internal code uses canonical names and shapes with mapping (and ' +
    '`Schema` validation) only at the edge; respect the dependency direction of the workspace. Cross-folder repair is seam-shaped: align ' +
    'counterparts, consumer sites, and counterpart cards — a concern owned twice across a runtime, an area mixing unrelated concerns, or coupling ' +
    'to a sibling owner INTERIOR (vs its wire/seam) is a defect.';

const CURRENT =
    'CURRENT STATE — sibling area pipelines land work concurrently with yours. Before ANY edit, re-read the CURRENT on-disk state ' +
    'of your pages AND every sibling page your pages compose or ripple into; landed sibling work is composed as found, never assumed from the ' +
    'discovery map. A seam counterpart a sibling pipeline landed is COMPOSED, not re-derived; a conflict between your design and a landed sibling ' +
    'resolves to the STRONGER form, never a revert. Edit any potentially shared page with surgical anchored Edits only — re-read and re-apply on an ' +
    'edit conflict, never a whole-file rewrite.';

const PROSE = [
    'PROSE QUALITY — apply docs/standards/style-guide.md. The page is a design SPEC: high-signal prose ONLY. Lead each section with the controlling ' +
        'rule/contract; one idea per paragraph; close on the consequence or boundary. Cut noise: no provenance, process narration, freshness ' +
        'disclaimers, report framing, or empty hedges (may/might/probably/generally/where possible). Trim walls of explanation to the load-bearing ' +
        'contract, and prefer a table, a typed signature block, or a tight bullet wherever it carries the design better than a paragraph.',
    'BACKTICK ALL CODE: wrap every symbol, type, field, function, operator, package ID, path, command, flag, and literal value in backticks. Name ' +
        'the exact member/type/rail in backticks instead of paraphrasing behavior. Trimming prose MUST NOT reduce technical density or remove design ' +
        'content.',
].join('\n');

const COMMENTS =
    'COMMENT HYGIENE: code fences are agent-facing — comment for the next agent, never as a tutorial. KEEP the canonical ' +
    'section-divider headers (language-comment marker + space + `---` + bracketed `[UPPERCASE_LABEL]` + dash-fill). Beyond dividers, comment ONLY ' +
    'where intent is not already obvious from names, types, and signatures: default to ZERO comments on self-evident code; at most 1 line where a ' +
    'comment genuinely earns its place; 1-2 lines only for a truly subtle invariant, contract, or boundary. NO restating the code, no narration, no ' +
    'task/process/session/history/proof/review comments, no TSDoc bloat. Densify names and types so comments are rarely needed; cut every low-value ' +
    'comment.';

const LAWS =
    'LAWS — read `docs/laws/` IN FULL (README + topology + patterns + scars; short registry pages) before any edit: a topology row whose ' +
    '[SURFACE] your edits touch binds its obligated counterparts into the SAME pass, and every patterns row binds each branch it names.';

const HARVEST_LAW =
    'HARVEST (required key, usually empty): nominate ONLY findings that generalize beyond this folder — a collapse pattern reusable across ' +
    'folders, a naivety class no doctrine clause names, a review rule that would have caught a defect BEFORE review, a cross-surface coupling ' +
    'discovered the hard way. Each row: altitude (stacks|reviewer|constitution|planning|readme|laws), lang, claim (the generalized law, one ' +
    'sentence), anchors (file:line evidence), existingClause (the exact doctrine or reviewer clause it would harden, quoted with its path — or ' +
    '"absent" plus the surfaces searched). A card-local fix never nominates; an empty array is the normal verdict — the terminal doctrine ' +
    'lander refutes weak rows, so nominate substance, never volume.';

const DOCTRINE = [LAW, '', LAWS, '', CARD, '', BARHUNT, '', ULTRA, '', PATLAW, '', BOUNDARIES, '', CURRENT, '', PROSE, '', COMMENTS].join('\n');

const GROUPNOTE =
    "CONCURRENT CARD GROUP: sibling implement agents realize this area's OTHER page-disjoint card groups concurrently. Realize " +
    "ONLY the cards in your worklist; touch any shared area surface (README.md, a page outside your group's anchored pages, a sibling area " +
    'page) with surgical anchored Edits only, re-read and re-applied on conflict.';

const INFO_LAW =
    'You provide INFORMATION, never prescriptions: exact disk locations and anchors, the current shape at each page, verified member ' +
    'spellings, seam endpoints, gaps. Downstream stages decide how to build; a map row that tells them what to write instead of what is true is a ' +
    'defect. ROW FORM: `composed`/`underutilized`/`seams`/`stacking` are prose facts; `members` carries exact verified spellings; `anchors` carry ' +
    'one coordinate per row (role names what it proves; `note` is the shortest literal witness under 20 words, or empty when path+line suffice; an ' +
    '`absence` anchor names where the expected thing was searched and not found); `files` lists what the implementer must open for the row. ' +
    'COVERAGE is part of the product: `requested` = your assigned scope, `read` = what you actually full-read, `skipped`/`unverified` = what you ' +
    'did not reach — an honest skip beats a silent one.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms));
// One shared launch gate: chain heads and implement-fan members alike pass it, so every pooled start stays staggered.
let gate = Promise.resolve();
const stagger = () => {
    gate = gate.then(() => sleep(STAGGER_MS));
    return gate;
};
const pool = async (items, cap, worker) => {
    const out = new Array(items.length);
    let next = 0;
    const run = async () => {
        while (next < items.length) {
            const i = next++;
            await stagger();
            out[i] = await worker(items[i], i);
        }
    };
    await Promise.all(Array.from({ length: Math.min(cap, items.length) }, () => run()));
    return out;
};

// Codex dispatch: the sonnet wrapper makes one blocking Codex MCP call, writes the envelope's content
// to the lane report, and returns mechanical orchestration data. Lane law rides developer-instructions
// (role split, battery-validated); the prompt carries only the task; the output contract sits LAST.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const laneLaw = (schema, o) =>
    (o.fix
        ? '<persistence>\nComplete every named move before yielding; do not stop at analysis or a partial edit. If the chosen ' +
          'approach resists, pick the next-best one and proceed. Return without an applied edit only if the territory genuinely ' +
          'admits none.\n</persistence>\n\n<verification>\nAfter editing, re-read each changed file and confirm it is coherent ' +
          'and nothing it carried was lost. Fix what fails before yielding.\n</verification>'
        : '<context_gathering>\nTerritory: the exact files and directories the task names. Do not open files outside it, ' +
          'including skill or instruction files (.claude/, CLAUDE.md, AGENTS.md).\nBudget: at most ' +
          (o.calls || 60) +
          ' tool calls total. Read in small batches (a handful of files per command, line-capped); never concatenate the whole ' +
          'territory into one command - tool output truncates and the data is lost.\nStop as soon as the product is complete. ' +
          'If something is still uncertain at the budget, proceed and record the residue in the product gap/unverified field ' +
          'instead of re-reading.\n</context_gathering>\n\n<verification>\nBefore the final message, confirm every cited ' +
          'spelling appears verbatim in the cited file; anything unconfirmed is recorded as a gap, never asserted.\n' +
          '</verification>') +
    '\n\n<output_contract>\nYour final message is a single JSON object with exactly this shape: ' +
    JSON.stringify(schema) +
    '\n- JSON only: no prose before or after it, no code fences, no markdown.\n- Every key shown is required.\n' +
    '- Use null for a value you could not determine and [] for an empty list; never guess.\n</output_contract>';
const codexPrompt = (label, task, schema, o) => {
    const base = CODEX_DIR + '/' + fileTag(label);
    const root = '/Users/bardiasamiee/Documents/99.Github/Rasm';
    const report = root + '/' + base + '-report.json';
    const model = o.model || 'gpt-5.6-terra';
    return [
        'DISPATCH ROLE: ' +
            model +
            ' performs the complete TASK below through one blocking Codex MCP call. Follow exactly four steps; ' +
            'never perform, edit, judge, soften, summarize, or relay the task yourself.',
        '(1) Call ToolSearch with query "select:mcp__codex__codex".',
        '(2) Call the loaded mcp__codex__codex tool ONCE with model="' +
            model +
            '", sandbox=' +
            (o.writes ? '"workspace-write"' : '"read-only"') +
            ', cwd=' +
            JSON.stringify(root) +
            (o.codexEffort ? ', config={"model_reasoning_effort":"' + o.codexEffort + '"}' : '') +
            ', "developer-instructions" set to the LANE LAW block below VERBATIM, and prompt set to the TASK block below ' +
            'VERBATIM. If the call errors, retry the identical call ONCE; if the retry errors, skip step (3) and return the ' +
            'error through step (4).',
        'LANE LAW:\n\n' + laneLaw(schema, o),
        'TASK:\n\n' + task,
        '(3) The tool result is a JSON envelope {threadId, content} whose content field holds the final-message text. ' +
            'Write that CONTENT text (the product JSON, unescaped) — never the envelope — with the Write tool to this absolute path: ' +
            report +
            '. Do not normalize, reformat, summarize, or extract the text before writing it.',
        o.receipt(base),
    ].join('\n\n');
};
const twinOf = (m) => (/-sol/.test(m || '') ? 'fable' : /-luna/.test(m || '') ? 'sonnet' : 'opus');
const nativeLane = (task, o) =>
    agent(task + o.nativeTail(CODEX_DIR + '/' + fileTag(o.label) + '-report.json'), {
        label: o.label,
        phase: o.phase,
        model: o.nativeModel || twinOf(o.model),
        effort: 'high',
        schema: o.wire,
        stallMs: o.stallMs || STALL,
    });
// The discovery lane routes here: gpt-5.6-terra wrapper when CODEX (a read-only mapping lane — recon law,
// never fix), the native twin otherwise. The row carries `scope` from the ORCHESTRATOR (never the lane's
// self-report) so a failed lane's territory is exact even when the lane died before writing anything.
const discoveryReceipt = (base) =>
    '(4) Parse the tool result text only for mechanical orchestration data. Return ok=true, report=' +
    base +
    '-report.json, entries=(tasks.length + ideas.length), headline="<entries> open cards | <status tallies>", failure empty, and copy ' +
    'order, cards=(tasks + ideas projected to {slug,pages}), gates, ripples, and malformed from malformed_ripples verbatim from the ' +
    'result. On a second tool error return ok=false, entries=0, report and headline empty, every structural array empty, and failure ' +
    'equal to the error text VERBATIM.';
const discoveryTail = (report) =>
    '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
    report +
    ' (Write tool, absolute path under the repo root): ' +
    JSON.stringify(DISCOVERY_SCHEMA) +
    ' — then return ONLY the receipt: ok, report path, entries = open tasks + ideas count, ' +
    'one-line mechanical headline (card count + status counts), failure empty, plus order, cards ({slug, pages} per open ' +
    'card), gates, ripples, and malformed (= malformed_ripples) transcribed exactly from the product.';
const recon = (task, o) => {
    const opts = { ...o, writes: !!o.writes, receipt: discoveryReceipt, nativeTail: discoveryTail, wire: RECEIPT };
    return (
        CODEX
            ? agent(codexPrompt(o.label, task, DISCOVERY_SCHEMA, opts), {
                  label: 'terra:' + o.label,
                  phase: o.phase,
                  model: 'sonnet',
                  effort: 'low',
                  schema: RECEIPT,
                  stallMs: o.stallMs || CODEX_STALL,
              }).then((r) => (r && !r.ok && /usage|quota|limit/i.test(r.failure || '') ? nativeLane(task, opts) : r))
            : nativeLane(task, opts)
    ).then((r) => ({
        lane: o.label,
        scope: o.scope || [],
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'lane died'),
        order: (r && r.order) || [],
        cards: (r && r.cards) || [],
        gates: (r && r.gates) || [],
        ripples: (r && r.ripples) || [],
        malformed: (r && r.malformed) || [],
    }));
};
const folderName = (p) => p.split('/').filter(Boolean).pop() || p;

// Sol critique lane: one blocking Codex MCP call at the operator-default tier in a workspace-write sandbox — a FIX lane
// (persistence + post-edit verification law), FIXLOG product to disk, thin receipt on the wire; CODEX=false (or a quota
// fallback) restores the native fable twin writing the same product to the same path.
const critiqueReceipt = (base) =>
    '(4) Parse the tool result text only for mechanical orchestration data. Return ok=true, report=' +
    base +
    '-report.json, entries=the length of result["realized"], headline="<entries> realized | verdict <verdict>", and failure ' +
    'empty. On a second tool error return ok=false, entries=0, report and headline empty, and failure equal to the error text VERBATIM.';
const critiqueTail = (report) =>
    '\n\nPRODUCT TO DISK: write your COMPLETE fix-log as one JSON file matching this schema at ' +
    report +
    ' (Write tool, absolute path under the repo root): ' +
    JSON.stringify(FIXLOG_SCHEMA) +
    ' — then return ONLY the receipt: ok, report path, entries = realized count, one-line mechanical headline, failure empty.';
const solLane = (task, o) => {
    const opts = { ...o, model: 'gpt-5.6-sol', writes: true, fix: true, receipt: critiqueReceipt, nativeTail: critiqueTail, wire: LANE_RECEIPT };
    return (
        CODEX
            ? agent(codexPrompt(o.label, task, FIXLOG_SCHEMA, opts), {
                  label: 'sol:' + o.label,
                  phase: o.phase,
                  model: 'sonnet',
                  effort: 'low',
                  schema: LANE_RECEIPT,
                  stallMs: SOL_STALL,
              }).then((r) => (r && !r.ok && /usage|quota|limit/i.test(r.failure || '') ? nativeLane(task, opts) : r))
            : nativeLane(task, opts)
    )
        .then((r) => ({
            ok: !!(r && r.ok && r.report),
            report: (r && r.report) || '',
            entries: (r && r.entries) || 0,
            headline: (r && r.headline) || '',
            failure: (r && r.failure) || (r ? '' : 'lane died'),
        }))
        .catch(() => ({ ok: false, report: '', entries: 0, headline: '', failure: 'lane died' }));
};

// Page-disjointness is PROVEN, never assumed: every ordered card must carry >=1 verified page,
// gate pairs merge, and components pack heaviest-first into <= IMPL_FAN buckets without splitting.
const cardGroups = (t) => {
    const inOrder = new Set(t.order || []);
    const cards = (t.cards || []).filter((c) => inOrder.has(c.slug));
    if (cards.length < 2 || cards.some((c) => !(c.pages && c.pages.length))) return null;
    const parent = new Map();
    const seed = (k) => {
        if (!parent.has(k)) parent.set(k, k);
    };
    const find = (k) => {
        let r = k;
        while (parent.get(r) !== r) r = parent.get(r);
        return r;
    };
    const union = (a, b) => {
        seed(a);
        seed(b);
        const ra = find(a);
        const rb = find(b);
        if (ra !== rb) parent.set(ra, rb);
    };
    for (const c of cards) {
        seed('s:' + c.slug);
        for (const p of c.pages) union('s:' + c.slug, 'p:' + p);
    }
    for (const g of t.gates || [])
        if (inOrder.has(g.blocked_slug) && inOrder.has(g.gated_by_slug)) union('s:' + g.blocked_slug, 's:' + g.gated_by_slug);
    const comps = new Map();
    for (const c of cards) {
        const r = find('s:' + c.slug);
        if (!comps.has(r)) comps.set(r, { slugs: [], pages: new Set() });
        const g = comps.get(r);
        g.slugs.push(c.slug);
        for (const p of c.pages) g.pages.add(p);
    }
    if (comps.size < 2) return null;
    const sorted = [...comps.values()].sort((a, b) => b.pages.size - a.pages.size || (a.slugs[0] < b.slugs[0] ? -1 : 1));
    const buckets = Array.from({ length: Math.min(IMPL_FAN, sorted.length) }, () => ({ slugs: [], pages: [] }));
    for (const c of sorted) {
        const b = buckets.reduce((m, x) => (x.pages.length < m.pages.length ? x : m));
        b.slugs.push(...c.slugs);
        b.pages.push(...c.pages);
    }
    return buckets;
};

const groupSeq = (t, g) => {
    const s = new Set(g.slugs);
    return JSON.stringify(
        {
            order: (t.order || []).filter((x) => s.has(x)),
            cards: (t.cards || []).filter((c) => s.has(c.slug)),
            ripples: (t.ripples || []).filter((r) => s.has(r.from_slug)),
            gates: (t.gates || []).filter((x) => s.has(x.blocked_slug)),
        },
        null,
        1,
    );
};

const discoverPrompt = (folder) =>
    [
        LAW,
        '',
        CARD,
        '',
        INFO_LAW,
        '',
        'TASK: DISCOVER + SEQUENCE + MAP the open work of the single TypeScript session target `' +
            folder +
            '` (the full session target set, for ripple ' +
            'classification only: ' +
            JSON.stringify(TARGETS) +
            "). This is the read-only reconnaissance grounding this area's downstream chain, and " +
            'read-only is its ONLY concession: resolve scope against real disk state (a real listing of the target folder, never memory), then read ' +
            '`' +
            folder +
            '/IDEAS.md` + `' +
            folder +
            '/TASKLOG.md` IN FULL from disk — full-file reads, never a grep/skim; the design pages are ' +
            'downstream reading scope. Return: (1) folder — echo `' +
            folder +
            '` exactly; (2) tasks — EVERY open card in `TASKLOG.md` (status ' +
            'ACTIVE/QUEUED/BLOCKED; carry the Atomic flag); (3) ideas — the 1-3 MOST actionable open cards in `IDEAS.md` (tasks-first doctrine: at most ' +
            '3, the ones whose Anchors are most settled and whose ripples land on in-scope targets), HARD CAP 3; on EVERY task/idea row also return ' +
            'pages — the repo-relative design pages under `' +
            folder +
            "/.planning/**` the card's `Anchors:` bullet names, PLUS one row per ripple-counterpart page (the design page a `Ripple:` row's " +
            'counterpart card names in its own folder — read it and anchor its seam surface, so the writers reach every counterpart hot instead of ' +
            'cold-hunting it), each VERIFIED to exist on ' +
            'disk with a listing (existence only — the pages stay downstream reading scope; empty when none verify): these rows prove page-disjoint card ' +
            'groups; (4) order — ONE sequenced slug list, ALL tasks first in dependency order then the chosen ideas; (5) ripples — for EVERY card ' +
            'carrying a `Ripple:` field, one row {from_slug, klass, to_pkg, to_slug}: klass=`in_scope` if to_pkg is one of the session targets, ' +
            '`oos_samelang` if it is another libs/typescript area, `cross_lang` if it points at `libs/csharp` / `libs/python` / ' +
            '`libs/typescript/.planning` / `libs/.planning`; confirm every counterpart ON DISK — open the named pkg card file and locate the mirror ' +
            'slug, never assert it from memory; (6) gates — for any [BLOCKED] card, {blocked_slug, gated_by_slug, in_scope} where in_scope is true iff ' +
            'the gating work is itself an open card in one of the session targets; (7) map — one row per design page the open cards target, PLUS one ' +
            "row per ripple-counterpart page (the design page a `Ripple:` row's counterpart card names in its own folder — read it and anchor its " +
            'seam surface, so the writers reach every counterpart hot instead of cold-hunting it): {page, files: every file (the page, its cited ' +
            "catalogs, its seam counterparts) the implementer must open for this row, anchors: exact coordinates backing the row's facts per the ROW " +
            'FORM law, members: the exact verified member spellings backing underutilized, each verified against the published types in node_modules ' +
            'and its owning `.api` catalog — verify a member via `uv run python -m tools.assay api` over the node_modules declarations (assay ' +
            'unavailable: the `.d.ts` directly + both `.api` tiers + Context7/exa/tavily, never memory); verified members ONLY — a member you cannot ' +
            'verify is a phantom and is NEVER listed; exact spellings and locations, never judgment wording, composed: the capability the page ' +
            'already composes, underutilized: catalog-anchored member FACTS the page does not yet compose, seams: the contextual cross-page/' +
            'cross-area seams the page meets, stacking: how the catalog capability stacks into the realization}. ' +
            'Return malformed_ripples for any `Ripple:` line you cannot parse ' +
            'into a pkg+slug, or whose counterpart slug you cannot locate on disk — an unverified ripple row is a phantom that belongs in ' +
            'malformed_ripples, never in ripples; (8) coverage — part of the product: `requested` = your assigned scope (the two card files plus every ' +
            'counterpart card file the ripples point at), `read` = what you actually full-read, `skipped`/`unverified` = what you did not reach or ' +
            'could not confirm — an honest skip beats a silent one. Read the FULL card body (every bullet) to classify — never guess from the thesis. ' +
            'Your map carries NAVIGATION FACTS — paths, verified counterpart locations, seam targets — never verdicts, and it is an initial pointer, ' +
            'never a ceiling: downstream agents re-read every full card and page, and the map never licenses a downstream skim. Return the structured ' +
            'product ONLY; edit nothing (discovery is the sole read-only stage).',
    ].join('\n');

const implementPrompt = (folder, rpt, seq, note) =>
    [
        DOCTRINE,
        '',
        'TASK: IMPLEMENT — realize the open cards of `' +
            folder +
            '` into deep design-page FENCES at the ULTRA-ADVANCED bar. DISCOVERY REPORT: ' +
            'read ' +
            rpt +
            " IN FULL from disk FIRST — the folder's navigation-facts reconnaissance (every open card with status/thesis/verified " +
            'pages, ripple classifications, gates, malformed ripples, the per-page navigation map ({page, files, anchors, members, composed, ' +
            'underutilized, seams, stacking}), and the lane `coverage`): FACTS with jump-coordinate anchors, never verdicts and never a reading ' +
            'ceiling — spot-verify what you build on, re-open every anchor behind an edit, hunt past the map on your own authority, and give its ' +
            'coverage `skipped`/`unverified` scope your own cold read. The ' +
            'sequenced worklist skeleton ' +
            '(your slugs + verified pages + ripple rows; read each FULL card body from `' +
            folder +
            '/IDEAS.md` + `' +
            folder +
            '/TASKLOG.md`, never the skeleton alone):\n' +
            seq +
            '\nREAD: ' +
            'each card full body; every design page the card names under `' +
            folder +
            '/.planning/**`; the sibling pages it seams to, at their CURRENT ' +
            'on-disk state; the area-root `ARCHITECTURE.md` + `README.md`; the operative docs/stacks/typescript/ pages (docs/stacks/csharp/ as the ' +
            'ambition floor); BOTH .api tiers — the shared `' +
            SHARED_API +
            '/*.md` AND the area `' +
            folder +
            '/.api/*.md` (stack them, the shared ' +
            'Effect/Schema rails layered onto the area packages) — cross-checked against the published types in node_modules; verify a member via `uv ' +
            'run python -m tools.assay api` (assay unavailable: the `.d.ts` directly + both `.api` tiers + Context7/exa/tavily, never memory). ' +
            'Realize EVERY card in `order` (all tasks incl. Atomic, then the ideas) into deep fences in the `' +
            folder +
            '` design pages, in LIFECYCLE ' +
            'order (admit raw ONCE through a `Schema` parse at the boundary -> canonical owner -> weave every cross-cutting concern as an Effect ' +
            'combinator/layer/decorator over a thin pure core -> compose through ONE unified `Effect`/`Either`/`Option` rail -> project + egress, BOTH ' +
            'ingress and egress parameterized with generics at depth). Collapse parallel interfaces/types/classes into ONE tagged discriminated union with ' +
            'EXHAUSTIVE `Match.exhaustive` dispatch; Schema-refined brands where primitives are overloaded. ZERO `any`/implicit-any/unsafe `as`/non-null `!`, NO ' +
            'runtime `enum`, NO `throw` in domain logic, `import type` explicit. Resolve any [BLOCKED] card inline (read published types in node_modules / ' +
            '`assay api`). RIPPLES ARE YOURS: repair every ripple your cards carry in this same pass per the RIPPLE law — align each in-scope seam to the ' +
            'counterpart page as it NOW stands on disk, realize each 1-hop out-of-scope same-language counterpart card fence and align both ends, land ' +
            'your half of each cross-language seam stating the wire contract — and record each repair in `ripples`. PACKAGE ADMISSION per the card law: ' +
            'folder-local parts NOW, the central `' +
            CENTRAL +
            '` catalog pin + `catalog:` row reported in `pins`, never edited; a landed cross-folder ' +
            "seam's `ARCHITECTURE.md` `[02]-[SEAMS]` row reported in `seams`, never edited. Do NOT close any card — the area red-team owns card status. " +
            'High-signal prose all-backticked, comment hygiene, fix-in-place (read-then-extend, preserve capability). Return verdict + realized slugs + ' +
            'deferred (any card you could not realize, with reason) + collapsed (before->after counts) + ripples + pins + seams + harvest + ' +
            'summary. ' +
            HARVEST_LAW +
            (note ? '\n' + note : ''),
    ].join('\n');

const critiquePrompt = (folder, seq, report) =>
    [
        DOCTRINE,
        '',
        'TASK: DOCTRINAL-CONFORMANCE AUDIT + CHARTER-COMPLETENESS + FIX IN PLACE across `' +
            folder +
            '`. You are an ULTRA-HARSH, UNAGREEABLE auditor: ' +
            'is this TRULY ultra-advanced TS, or naive code in disguise? Assume a violation exists in every fence until you prove otherwise; "good enough" ' +
            'is rejected. The cards realized this turn (read each FULL body from `' +
            folder +
            '/IDEAS.md` + `' +
            folder +
            '/TASKLOG.md`):\n' +
            seq +
            '\nThe discovery report at `' +
            report +
            '` carries the navigation map from disk — facts with anchors, never verdicts; consult it for verified members, seam targets, and ' +
            'coverage gaps, and it licenses no skim.\nREAD ' +
            'the realized pages under `' +
            folder +
            '/.planning/**`, the sibling pages at their CURRENT on-disk state, docs/stacks/typescript/, and BOTH ' +
            '.api tiers (shared `' +
            SHARED_API +
            '` + area `' +
            folder +
            '/.api`). Run these MECHANICAL checklists line-by-line as a FLOOR you hunt PAST, ' +
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
            '`' +
            CENTRAL +
            '` row in `pins`. Return verdict + realized + deferred + collapsed + ripples + pins + seams + harvest + summary. ' +
            HARVEST_LAW,
    ].join('\n');

const redteamPrompt = (folder, seq, report, critRep) =>
    [
        DOCTRINE,
        '',
        'TASK: ADVERSARIAL RED-TEAM + FIX IN PLACE + TERMINAL CLOSE across `' +
            folder +
            '`. You are a HOSTILE principal reviewer whose explicit goal ' +
            'is to REJECT this design as naive TypeScript: assume it is junior, under-typed, or wrong until the fences prove otherwise; the burden of ' +
            'proof is ON THE DESIGN. The cards realized this turn (read each FULL body):\n' +
            seq +
            '\n' +
            (critRep
                ? 'PRIOR CLAIMS (UNVERIFIED): the sol critique fixlog is ON DISK at `' +
                  critRep +
                  '` — read it IN FULL from disk; its edits and verdicts are refutation targets you judge against CURRENT disk, never a settled ' +
                  'record. FOLD-FORWARD DUTY: its surviving `ripples`, `pins`, `seams`, `deferred`, and `harvest` rows are folded into YOUR return ' +
                  "(re-verified against current disk, deduped) — your return is the folder's consolidated record; a dropped critique row is a " +
                  'silent loss.'
                : 'PRIOR CLAIMS: the critique lane did not land — your cold attack is the only review this folder gets; judge from CURRENT disk alone.') +
            '\nThe discovery report at `' +
            report +
            '` carries the navigation map from disk — facts with anchors, never verdicts; consult it for verified members, seam targets, and ' +
            'coverage gaps, and it licenses no skim.\nRead docs/stacks/typescript/, the published ' +
            'library types, BOTH .api tiers (shared `' +
            SHARED_API +
            '` + area `' +
            folder +
            '/.api`), and the sibling pages at their CURRENT on-disk ' +
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
        'TERMINAL CLOSE — you are `' +
            folder +
            "`'s LAST stage and the SOLE owner of its card status. For EVERY card in scope this run, re-read its " +
            'FULL body and the realized fences on CURRENT disk, then ADVERSARIALLY VERIFY — the fences are naive until they survive your attack, a prior ' +
            'pass verdict a rejected self-assessment — that they genuinely fulfill the card `Capability`/`Shape`/`Unlocks` against the published types ' +
            '(verify a member via `uv run python -m tools.assay api`; assay unavailable — the published `.d.ts` + both `.api` tiers + ' +
            'Context7/exa/tavily). FINAL-remediate any weak or partial realization in place NOW, then assign each card a strength: `strong` (every ' +
            'charter clause delivered, fences transcription-complete and fully type-safe against the published types), `partial` (most delivered, a ' +
            'clause still thin), `weak` (charter not met). CLOSE only `strong` cards per the CARD CLOSURE law; a ripple card whose seam you cannot ' +
            'verify landed on BOTH ends on current disk stays OPEN with that reason; honestly RE-OPEN every card you cannot bring to `strong`, with a ' +
            'one-line reason (a real out-of-run or cross-language dependency). The orchestrator DEMOTES any card closed below `strong`, so never ' +
            'inflate. Return verdict + realized + deferred + collapsed + ripples + pins + seams + harvest + closed [{slug, disposition, strength}] + ' +
            'reopened [{slug, reason}] + summary. ' +
            HARVEST_LAW,
    ].join('\n');

const pinPrompt = (pins, seams, orphans, backlog, round) =>
    [
        round
            ? 'DRAIN ROUND ' +
              round +
              ' — every backlog row below was verified STILL-OPEN by the prior round; the pins, seam rows, and orphan fixlogs are already ' +
              'applied. Realize or repair each row at its root NOW; a row you genuinely cannot land carries its named blocker and owner in `remaining`.'
            : '',
        LAW,
        '',
        PROSE,
        '',
        "TASK: TERMINAL SINGLE-WRITER + BACKLOG DRAIN — you are the run's SOLE writer for the shared manifests (`" +
            CENTRAL +
            '` catalog + the owning `catalog:` ' +
            'manifest, `libs/typescript/package.json` or the root `package.json`) and for every area `ARCHITECTURE.md` `[02]-[SEAMS]` section, and its ' +
            'LAST agent. ORPHANED CRITIQUE FIXLOGS (folders whose redteam never landed, so these on-disk ' +
            "fixlogs' `pins`, `seams`, and `harvest` rows were never folded forward — read each IN FULL from disk, apply the pin and seam rows " +
            "under the same law as the reported rows below, and fold each fixlog's surviving harvest rows into your own `harvest` return, " +
            're-verified against current disk and deduped): ' +
            JSON.stringify(orphans) +
            '. PINS: apply each reported catalog pin + `catalog:` row below exactly once, preserving the existing group/order and deduping ' +
            'semantically identical rows; verify each package + version against the published registry (Context7/exa/tavily against the official package ' +
            'docs; `uv run python -m tools.assay api` over node_modules once installed) before applying; confirm the area README group and ' +
            '`.api/<package>.md` catalog landed, repairing a missing folder-local part in place. SEAM ROWS: upsert each reported {file, row} into the ' +
            "named file's `[02]-[SEAMS]` section exactly once, preserving the section's row format and order and deduping semantically identical rows; " +
            'a missing file or absent `[02]-[SEAMS]` section rejects the row. Reject any unverifiable or malformed row as {target, reason} — never apply ' +
            'it silently. BACKLOG DRAIN (deferred cards the folder chains could not realize — re-verify each {files, claim} on CURRENT disk, realize ' +
            'any whose gate landed this run in a sibling folder now that every chain has closed, reject what disk already resolved): ' +
            JSON.stringify(backlog, null, 1) +
            '. PINS:\n' +
            JSON.stringify(pins, null, 1) +
            '\nSEAM ROWS:\n' +
            JSON.stringify(seams, null, 1) +
            '\nReturn applied + seam_rows_applied + rejected + summary, and `remaining` carrying ONLY backlog rows verified still-open on current ' +
            'disk and genuinely blocked, each claim naming its blocker and owner; a row disk already resolved is culled with proof in `rejected`, and ' +
            'an empty `remaining` attests the drain closed. ' +
            HARVEST_LAW,
    ]
        .filter(Boolean)
        .join('\n');

const doctrinePrompt = (rows) =>
    'TASK: DOCTRINE LANDER — the durable-learning terminal of this run. Read `docs/laws/README.md` FIRST — it ' +
    'owns the corpus admission and page-shape law; obey it over any restatement. Load the `docgen` skill AND the `skill-writer` skill via the Skill tool BEFORE any durable edit; ' +
    'load `mermaid-diagramming` before touching any diagram. ' +
    "NOMINATIONS (unverified, biased toward their authors' own work — refute by default): " +
    JSON.stringify(rows) +
    '\nADJUDICATE each row per the admission bar: cold-read its target surface IN FULL, verify its anchors on CURRENT disk; LAND NOTHING is a ' +
    'first-class verdict.\nTOPOLOGY RE-PROOF: re-verify every `docs/laws/topology.md` row whose [SURFACE] this run touched — cull a row whose ' +
    'coupling no longer holds, land a coupling this run proved.\nGATE: run `uv run .claude/skills/docgen/scripts/prose_gate.py <every touched ' +
    '.md>` and repair to zero FAILs before returning. Return landed/refined/rejected (each rejection with its reason)/files/summary.';

// --- [COMPOSITION] ---------------------------------------------------------------------

phase('Realize');
log('Pooling ' + TARGETS.length + ' folder chain(s) at CAP=' + CAP);
const runFolder = async (target) => {
    const tag = folderName(target);
    try {
        const t = await recon(discoverPrompt(target), { label: 'discover:' + tag, phase: 'Realize', scope: [target] });
        if (!t.ok) return { folder: target, failed: true, empty: false, logs: [], red: null, cross_lang: [], malformed: [], error: t.failure };
        const cross = (t.ripples || [])
            .filter((rp) => rp.klass === 'cross_lang')
            .map((rp) => tag + ' [' + rp.from_slug + '] -> ' + rp.to_pkg + ' [' + rp.to_slug + ']');
        const malformed = t.malformed || [];
        if (!t.entries) return { folder: target, failed: false, empty: true, logs: [], red: null, cross_lang: cross, malformed };
        const seq = JSON.stringify({ order: t.order, cards: t.cards, ripples: t.ripples, gates: t.gates }, null, 1);
        const groups = cardGroups(t);
        let impls;
        if (groups) {
            log(
                tag +
                    ': implement fan over ' +
                    groups.length +
                    ' page-disjoint group(s); page weights ' +
                    groups.map((g) => g.pages.length).join('/'),
            );
            impls = (
                await parallel(
                    groups.map((g, gi) => async () => {
                        await stagger();
                        return agent(implementPrompt(target, t.report, groupSeq(t, g), GROUPNOTE), {
                            label: 'implement:' + tag + ':g' + gi,
                            phase: 'Realize',
                            schema: FIXLOG_SCHEMA,
                            model: 'fable',
                            effort: 'high',
                            stallMs: STALL,
                        });
                    }),
                )
            ).filter(Boolean);
        } else {
            const one = await agent(implementPrompt(target, t.report, seq, ''), {
                label: 'implement:' + tag,
                phase: 'Realize',
                schema: FIXLOG_SCHEMA,
                model: 'fable',
                effort: 'high',
                stallMs: STALL,
            });
            impls = one ? [one] : [];
        }
        if (!impls.length) return { folder: target, failed: true, empty: false, logs: [], red: null, cross_lang: cross, malformed }; // failure isolation: a dead implement skips its reviews
        // Sol critique: fixlog to disk, receipt on the wire; the redteam folds its rows forward,
        // and a folder whose redteam dies leaves the fixlog ORPHANED for the terminal single-writer.
        await stagger();
        const crit = await solLane(critiquePrompt(target, seq, t.report), { label: 'critique:' + tag, phase: 'Realize' });
        const critRep = crit.ok ? crit.report : '';
        await stagger();
        const red = await agent(redteamPrompt(target, seq, t.report, critRep), {
            label: 'redteam:' + tag,
            phase: 'Realize',
            schema: REDTEAM_SCHEMA,
            model: 'fable',
            effort: 'high',
            stallMs: STALL,
        });
        return {
            folder: target,
            failed: false,
            empty: false,
            logs: [...impls, red].filter(Boolean),
            red,
            critReport: critRep && !red ? critRep : '',
            cross_lang: cross,
            malformed,
        };
    } catch (e) {
        return {
            folder: target,
            failed: true,
            empty: false,
            logs: [],
            red: null,
            cross_lang: [],
            malformed: [],
            error: String((e && e.message) || e),
        }; // failure isolation: one thrown chain never rejects the pool
    }
};
const done = (await pool(TARGETS, CAP, runFolder)).filter(Boolean);
const failed = done.filter((r) => r.failed).map((r) => r.folder);
const emptyFolders = done.filter((r) => r.empty).map((r) => r.folder);
const active = done.filter((r) => !r.failed && !r.empty);
const crossLang = done.flatMap((r) => r.cross_lang || []);
const deferred = done.flatMap((r) => r.logs.flatMap((l) => (l.deferred || []).map((d) => ({ folder: r.folder, slug: d.slug, reason: d.reason }))));
const ripplesRepaired = done.flatMap((r) => r.logs.flatMap((l) => l.ripples || []));
const pinsReported = [...new Map(done.flatMap((r) => r.logs.flatMap((l) => l.pins || [])).map((p) => [p.package + '|' + p.row, p])).values()];
const seamsReported = [...new Map(done.flatMap((r) => r.logs.flatMap((l) => l.seams || [])).map((s) => [s.file + '|' + s.row, s])).values()];
let closed_count = 0;
const reopened = [];
for (const r of done) {
    for (const c of (r.red && r.red.closed) || []) {
        if (c.disposition === 'complete' && c.strength !== 'strong')
            reopened.push({ folder: r.folder, slug: c.slug, reason: 'demoted: strength=' + c.strength });
        else closed_count++;
    }
    for (const ro of (r.red && r.red.reopened) || []) reopened.push({ folder: r.folder, slug: ro.slug, reason: ro.reason });
}
log(
    'Realize: ' +
        active.length +
        '/' +
        TARGETS.length +
        ' folder(s) realized (' +
        emptyFolders.length +
        ' empty); ' +
        closed_count +
        ' cards closed, ' +
        reopened.length +
        ' re-open/demoted; ' +
        ripplesRepaired.length +
        ' ripple repair(s); ' +
        pinsReported.length +
        ' pin(s) + ' +
        seamsReported.length +
        ' seam row(s) reported' +
        (failed.length ? '; failed: ' + failed.join(', ') : ''),
);

// --- [PINS]

const ORPHANS = done.map((r) => r.critReport || '').filter(Boolean);
// The deferred cards a folder chain could not realize become the drain backlog in {files, claim} form.
const BACKLOG = deferred.map((d) => ({ files: [], claim: d.folder + ' [' + d.slug + ']: ' + d.reason }));
// Terminal DRAIN LOOP: one serial opus single-writer per round applies the reported pins + seam rows and the orphaned
// critique fixlogs once (round 0), then drains the pooled deferred backlog against live disk, re-feeding the still-open
// remainder; a round cap plus a no-shrinkage progress gate bound the loop, and the blocked remainder rides the run return.
let pinlog = null;
let pinHarvest = [];
let residuals = BACKLOG;
let orphanQueue = ORPHANS;
let lastOpen = Infinity;
if (pinsReported.length || seamsReported.length || ORPHANS.length || BACKLOG.length) {
    phase('Pins');
    for (let round = 0; round < DRAIN_ROUNDS; round++) {
        pinlog = await agent(pinPrompt(round ? [] : pinsReported, round ? [] : seamsReported, orphanQueue, residuals, round), {
            label: round ? 'pins:r' + round : 'pins',
            phase: 'Pins',
            schema: PIN_SCHEMA,
            model: 'opus',
            effort: 'high',
            stallMs: STALL,
        });
        if (!pinlog) break; // dead round: the fed-in residual and orphan sets survive to the run return, never zeroed by a lost closer
        pinHarvest = pinHarvest.concat(pinlog.harvest || []);
        const open = pinlog.remaining || [];
        orphanQueue = [];
        residuals = open;
        if (!open.length || open.length >= lastOpen) break;
        lastOpen = open.length;
    }
}
// DOCTRINE LANDER: the run's durable-learning terminal — pooled harvest nominations adjudicated against the live
// docs/laws surfaces; refutation-first, land-nothing legal, admission law owned by docs/laws. Fires only when non-empty.
const HARVEST_ROWS = done.flatMap((r) => r.logs.flatMap((l) => l.harvest || [])).concat(pinHarvest);
let doctrine = null;
if (HARVEST_ROWS.length) {
    phase('Pins');
    doctrine = await agent(doctrinePrompt(HARVEST_ROWS), {
        label: 'doctrine',
        phase: 'Pins',
        schema: DOCTRINE_SCHEMA,
        model: 'fable',
        effort: 'high',
        stallMs: STALL,
    });
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
    pins: {
        reported: pinsReported.length,
        seams_reported: seamsReported.length,
        applied: (pinlog && pinlog.applied) || [],
        seam_rows_applied: (pinlog && pinlog.seam_rows_applied) || [],
        rejected: (pinlog && pinlog.rejected) || [],
    },
    residuals,
    doctrine: doctrine && {
        nominated: HARVEST_ROWS.length,
        landed: (doctrine.landed || []).length,
        refined: (doctrine.refined || []).length,
        rejected: (doctrine.rejected || []).length,
        files: doctrine.files || [],
        summary: doctrine.summary,
    },
    cross_language: crossLang,
    malformed_ripples: done.flatMap((r) => r.malformed || []),
};
