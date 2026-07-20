export const meta = {
    name: 'slice-implement',
    description:
        'Theme-sliced implement engine over the whole libs/ card estate — one run realizes ONE functionality set at full depth, never unrelated work: discovery and per-folder plan lanes ingest the FULL card pool + census rows + research debt, then SELECT into typed work units only the theme-relevant cards and pages judged per folder against the run charter (borderline includes; non-theme cards stay untouched as expected leftovers, rostered with future-slice hints; cross-libs cards decompose into per-folder ripple executions landed in the same run); per unit a read-only codex sol lane writes a fact map + defect census + research-row census, blocked cards get a codex guidance leg then a stronger unblocking writer, a tiered writer (fable ground-up, opus surgical, codex mechanical volume) realizes cards into transcription-complete fences and drains research rows (theme-pertinent rows resolve fully; easy rows in touched files resolve opportunistically; hard non-pertinent rows stay, counted for their owning slice), a mid-run admission lane executes the full package-admission chain per discovered missing package (single-writer per central manifest), a critique repairs in place, and a conditional compose pass integrates critique-admitted packages into the unit fences; per-language drains apply cross-folder rows, one terminal fable red-team seals each libs tree with the four-surface drift closure, a theme-gated tests-estate branch with its own recon legs rebuilds tests/ + the root test spine to a gate-green state, and a slice-closure audit proves every theme-selected card terminal — non-theme leftovers are never failures. args = {camp: absolute campaign home (required), theme?: {title, charter, include, tests} — omitted runs the built-in slice #1: the complete observability set + full IaC realization, ruled inseparable; scope?: folder subset; tests?: boolean}; products land under camp/implement/ in a per-theme directory.',
    whenToUse: 'One run per functionality slice over the libs/ card estate — slice #1 is observability+IaC; later slices pass their own theme',
    phases: [
        { title: 'Plan', detail: 'per-folder + cross-libs card-pool ingestion into typed, dependency-ordered work units with blocker rows' },
        { title: 'Map', detail: 'per-unit codex sol fact map + defect and research census (read-only) and blocker guidance legs' },
        { title: 'Implement', detail: 'tiered writers realize cards into fences, drain research rows, and close every card in-run' },
        { title: 'Admit', detail: 'full admission chain per discovered missing package, single-writer per central manifest' },
        { title: 'Critique', detail: 'per-unit conformance + capability audit, repaired in place' },
        { title: 'Compose', detail: 'conditional integration pass composing critique-admitted packages into the unit fences' },
        { title: 'Drain', detail: 'per-language cross-folder row application, refute-first' },
        { title: 'Redteam', detail: 'one terminal fable lane per language tree + four-surface drift closure, legged for volume', model: 'fable' },
        {
            title: 'Tests',
            detail: 'tests-estate build-out per language with its own recon legs: _testkits, harness rails, root spine, gate to green',
        },
        {
            title: 'Audit',
            detail: 'slice-closure proof over theme-selected cards: zero open selected cards; leftovers + research counted, never failures',
        },
    ],
};

// --- [CONSTANTS] ---------------------------------------------------------------------

const REPO = '/Users/bardiasamiee/Documents/99.Github/Rasm';
const CAP = 14; // true in-flight agent ceiling — wrappers, writers, and reviewers all take one slot
const STAGGER_MS = 1500;
const STALL_MS = 900000; // blocking codex MCP legs and fable writers run many minutes without visible progress
const PLAN_MEDIUM_FLOOR = 5; // open-card count from which a plan lane runs medium effort
const PLAN_HIGH_FLOOR = 12; // open-card count from which a plan lane runs high effort — decomposition quality gates the whole chain
const SHORT = { csharp: 'cs', python: 'py', typescript: 'ts', cross: 'x' };
const LANGS = [
    { key: 'csharp', root: 'libs/csharp' },
    { key: 'python', root: 'libs/python' },
    { key: 'typescript', root: 'libs/typescript' },
];
const GATES = { csharp: 'dotnet build', python: 'pytest --collect-only', typescript: 'pnpm install then pnpm typecheck' };
const AXES = {
    csharp:
        'deterministic doubles; fake/in-memory telemetry capture rails (MetricCollector-class collectors, in-memory OTLP exporters, fake ' +
        'collectors, deterministic TimeProvider clocks); transport harnesses for the kafka/grpc/nats seams; BenchmarkDotNet corpus-gate wiring',
    python:
        'deterministic doubles; in-memory OTLP/metric capture rails and fake collectors; deterministic clocks; transport harnesses for the ' +
        'grpc/nats seams; the python bench tier',
    typescript:
        'deterministic doubles; in-memory OTLP/metric capture rails and fake collectors; deterministic TestClock rails; transport harnesses ' +
        'for the kafka/grpc/nats seams; vitest bench readiness',
};
const DEFAULT_THEME = {
    // slice #1 — the launch theme when args.theme is omitted
    title: 'observability-and-iac',
    charter:
        'The complete observability fabric and its full IaC realization, ruled inseparable — per-project deployment is the telemetry story ' +
        'final leg. In scope: four-signal telemetry (metrics, logging, tracing, profiling) and health rails; receipt-as-truth projection ' +
        'through the per-branch instrument owners; hook registries and telemetry-as-tap; the wire law (resource triple, UCUM metric names, ' +
        'OTLP/HTTP+protobuf egress, W3C composite propagation, trace exemplars, base2-exponential histograms, tenant baggage promotion); ' +
        'transport instrumentation (gRPC, Kafka, NATS manual W3C carriers, MQTT v5, CloudEvents); storage and lake harvest surfaces ' +
        '(Npgsql/psycopg/sqlite instrumentation, DuckDB and SQLite profile harvest, pg_stat receipt slots, Flight SQL trace injection, ' +
        'lake-engine profile parity); benchmarking receipt families and corpus gates; dashboards and alerting from the SLO burn-rate ' +
        'algebra; and the full IaC leg — backend store rows (Mimir-shaped axis), collector gateway and queue, log/trace/profile stores, ' +
        'alerting, tenancy, provisioning, deploy annotations, deploy-plane self-telemetry, and StackOutputs endpoints.',
    include: [
        'observability',
        'telemetry',
        'metrics',
        'logging',
        'tracing',
        'profiling',
        'health',
        'hooks',
        'instrumentation',
        'otlp',
        'otel',
        'exemplars',
        'baggage',
        'grpc',
        'kafka',
        'nats',
        'mqtt',
        'cloudevents',
        'collector',
        'pyroscope',
        'benchmark',
        'dashboards',
        'alerting',
        'slo',
        'iac',
        'mimir',
        'tempo',
        'loki',
        'victoriametrics',
        'victorialogs',
        'grafana',
        'deploy',
        'stackspec',
        'stackoutputs',
    ],
    tests: true,
};

// --- [INPUTS] ------------------------------------------------------------------------

// Tool-boundary normalization: the Workflow tool's untyped args input can deliver a JSON-encoded string.
const ARGS = (() => {
    if (typeof args === 'string') {
        try {
            return JSON.parse(args) || {};
        } catch {
            return {};
        }
    }
    return args && typeof args === 'object' ? args : {};
})();
const CAMP = typeof ARGS.camp === 'string' && ARGS.camp.trim() ? ARGS.camp.trim().replace(/\/+$/, '') : '';
const SCOPE = Array.isArray(ARGS.scope)
    ? ARGS.scope.filter(Boolean).map(String)
    : typeof ARGS.scope === 'string' && ARGS.scope.trim()
      ? [ARGS.scope.trim()]
      : [];
const slugOf = (s) =>
    String(s)
        .toLowerCase()
        .replace(/[^a-z0-9.-]+/g, '-');
const THEME = (() => {
    const t = ARGS.theme;
    return t && typeof t === 'object'
        ? {
              title: String(t.title || ''),
              charter: String(t.charter || ''),
              include: Array.isArray(t.include) ? t.include.filter(Boolean).map(String) : [],
              tests: t.tests === true,
          }
        : DEFAULT_THEME;
})();
const TESTS_ON = ARGS.tests === true || (!SCOPE.length && THEME.tests === true); // theme-gated: the tests estate rides the themes that claim it
const OUT = CAMP + '/implement/' + slugOf(THEME.title || 'slice'); // per-theme product directory — successive slices never mix receipts

// --- [MODELS] ------------------------------------------------------------------------

const DISCOVERY = {
    type: 'object',
    additionalProperties: false,
    required: ['folders'],
    properties: {
        folders: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['path', 'language', 'openCards'],
                properties: {
                    path: { type: 'string' },
                    language: { type: 'string', enum: ['csharp', 'python', 'typescript', 'cross'] },
                    openCards: { type: 'integer' },
                },
            },
        },
    },
};

const UNITS = {
    type: 'object',
    additionalProperties: false,
    required: ['units', 'leftovers', 'note'],
    properties: {
        leftovers: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['item', 'hint'],
                properties: { item: { type: 'string' }, hint: { type: 'string' } },
            },
        },
        units: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['order', 'title', 'tier', 'heavy', 'cards', 'pages', 'anchors', 'packages', 'rippleSurface', 'blockers'],
                properties: {
                    order: { type: 'integer' },
                    title: { type: 'string' },
                    tier: { type: 'string', enum: ['fable', 'opus', 'codex'] },
                    heavy: { type: 'boolean' },
                    cards: { type: 'array', items: { type: 'string' } },
                    pages: { type: 'array', items: { type: 'string' } },
                    anchors: { type: 'array', items: { type: 'string' } },
                    packages: { type: 'array', items: { type: 'string' } },
                    rippleSurface: {
                        type: 'array',
                        items: {
                            type: 'object',
                            additionalProperties: false,
                            required: ['endpoint', 'source'],
                            properties: { endpoint: { type: 'string' }, source: { type: 'string' } },
                        },
                    },
                    blockers: {
                        type: 'array',
                        items: {
                            type: 'object',
                            additionalProperties: false,
                            required: ['card', 'question', 'route'],
                            properties: { card: { type: 'string' }, question: { type: 'string' }, route: { type: 'string' } },
                        },
                    },
                },
            },
        },
        note: { type: 'string' },
    },
};

const PKG_ROW = {
    type: 'object',
    additionalProperties: false,
    required: ['package', 'language', 'reason'],
    properties: { package: { type: 'string' }, language: { type: 'string' }, reason: { type: 'string' } },
};

const RECEIPT = {
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'research', 'headline', 'failure'],
    properties: {
        ok: { type: 'boolean' },
        report: { type: 'string' },
        entries: { type: 'integer' },
        research: { type: 'integer' },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};

const STAGE = {
    type: 'object',
    additionalProperties: false,
    required: [
        'folder',
        'stage',
        'ok',
        'report',
        'filesTouched',
        'crossFolderRows',
        'cardsClosed',
        'researchResolved',
        'packagesMissing',
        'residuals',
        'headline',
        'failure',
    ],
    properties: {
        folder: { type: 'string' },
        stage: { type: 'string' },
        ok: { type: 'boolean' },
        report: { type: 'string' },
        filesTouched: { type: 'array', items: { type: 'string' } },
        crossFolderRows: { type: 'integer' },
        cardsClosed: { type: 'integer' },
        researchResolved: { type: 'integer' },
        packagesMissing: { type: 'array', items: PKG_ROW },
        residuals: { type: 'integer' },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};

const ADMITR = {
    type: 'object',
    additionalProperties: false,
    required: ['package', 'language', 'ok', 'admitted', 'report', 'filesTouched', 'headline', 'failure'],
    properties: {
        package: { type: 'string' },
        language: { type: 'string' },
        ok: { type: 'boolean' },
        admitted: { type: 'boolean' },
        report: { type: 'string' },
        filesTouched: { type: 'array', items: { type: 'string' } },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};

const DRAINR = {
    type: 'object',
    additionalProperties: false,
    required: ['language', 'ok', 'report', 'applied', 'residualRows', 'headline', 'failure'],
    properties: {
        language: { type: 'string' },
        ok: { type: 'boolean' },
        report: { type: 'string' },
        applied: { type: 'integer' },
        residualRows: { type: 'array', items: { type: 'string' } },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};

const REDTEAMR = {
    type: 'object',
    additionalProperties: false,
    required: ['language', 'ok', 'report', 'filesTouched', 'defectsFixed', 'cardsReopened', 'residualRows', 'headline', 'failure'],
    properties: {
        language: { type: 'string' },
        ok: { type: 'boolean' },
        report: { type: 'string' },
        filesTouched: { type: 'array', items: { type: 'string' } },
        defectsFixed: { type: 'integer' },
        cardsReopened: { type: 'integer' },
        residualRows: { type: 'array', items: { type: 'string' } },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};

const TESTSR = {
    type: 'object',
    additionalProperties: false,
    required: ['language', 'ok', 'report', 'filesTouched', 'gate', 'gateClean', 'residualRows', 'headline', 'failure'],
    properties: {
        language: { type: 'string' },
        ok: { type: 'boolean' },
        report: { type: 'string' },
        filesTouched: { type: 'array', items: { type: 'string' } },
        gate: { type: 'string' },
        gateClean: { type: 'boolean' },
        residualRows: { type: 'array', items: { type: 'string' } },
        headline: { type: 'string' },
        failure: { type: 'string' },
    },
};

const AUDITR = {
    type: 'object',
    additionalProperties: false,
    required: ['openCards', 'blockedNoTrigger', 'blockedCards', 'researchRows', 'researchByLanguage'],
    properties: {
        openCards: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['file', 'card'],
                properties: { file: { type: 'string' }, card: { type: 'string' } },
            },
        },
        blockedNoTrigger: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['file', 'card'],
                properties: { file: { type: 'string' }, card: { type: 'string' } },
            },
        },
        blockedCards: { type: 'integer' },
        researchRows: { type: 'integer' },
        researchByLanguage: { type: 'array', items: { type: 'string' } },
    },
};

// --- [DOCTRINE] ----------------------------------------------------------------------

const PATH_LAW =
    'Every path you write, cite, or pass is ABSOLUTE from ' +
    REPO +
    ' — never a relative path or bare basename; a relative product path mints a stray directory, a run defect. Run no git command in any ' +
    'form — staging custody belongs to the user after the run.';

const THEME_LAW =
    'THEME SLICE "' +
    THEME.title +
    '" — this run realizes ONE functionality set at full depth and does no unrelated work. CHARTER: ' +
    THEME.charter +
    (THEME.include.length ? ' Axis hints (guidance for judgment, never a keyword filter): ' + THEME.include.join(', ') + '.' : '');

const STANCE =
    'Hold the work hostile: it was authored by another engineer and is naive, shallow, or illusory until it survives your attack — dense, ' +
    'confident, idiom-fluent output is the prime suspect. Naivety on both axes is intolerable: COVERAGE (a thin slice of the concept where ' +
    'the domain carries the full family) and APPROACH (enumerated hardcoded instances where one parameterized owner generates the space). ' +
    'No churn: an edit requires a named violated law and the concrete case that breaks it; a clean verdict earned by an attack that finds ' +
    'nothing is a first-class result. Emptiness is not evidence: a probe returning nothing proves absence only after the probe re-runs in a ' +
    'second form.';

const FENCE_LAW =
    'Fences are the work product: capability conserved absolutely, density through polymorphic collapse never deletion; new capability lands ' +
    'as a row, case, field, operation, or policy value inside the existing owner before any new surface appears; doctrine is the floor ' +
    'pushed past, never the ceiling. A page or sub-folder minted this pass meets the full density and coverage bar of a mature page the ' +
    'moment it lands — a stub, skeleton, or placeholder is a defect. Where a card assumes a file, folder, or owner: confirm it on disk ' +
    'first; absent, CREATE it at that full bar and extend it as if it had existed from the start — never tacked-on. A page or sub-folder ' +
    'minted this pass joins its folder README registry and ARCHITECTURE codemap/[SEAMS] in the same unit — an unregistered mint is a defect.';

const CARD_LAW =
    'Every card in the unit reaches a terminal disposition IN ITS CARD FILE this pass: a realized card moves to [02]-[CLOSED] as ' +
    '[ID]-[COMPLETE] with a one-line disposition naming the landed owner; a card current disk proves already realized closes [COMPLETE] ' +
    'citing the landed fence; a refuted card closes [DROPPED] with the refuting citation; genuinely-future work re-cards in place to ' +
    '[BLOCKED] with an explicit arming trigger (the condition that activates it). An [ACTIVE] or [QUEUED] card may not survive your pass ' +
    'unchanged. Both ends of a Ripple pair move together only when both folders are yours — a foreign end is a crossFolderRows row. A ' +
    'cross-libs card closes at the cross tier only when every folder ripple is landed on disk. A non-theme open card sharing a card file ' +
    'with unit cards stays byte-untouched — an expected leftover for a future slice, never dispositioned, never reworded. RIPPLE ' +
    'DISPOSITION: a card closes ONLY with its ripple surface dispositioned — every unit rippleSurface endpoint its card names ends (a) ' +
    'LANDED in your territory this pass, (b) DEFERRED as a crossFolderRows row when outside your territory and theme-relevant, or (c) ' +
    'OUT-OF-THEME recorded in the receipt rippleLedger with the future-slice hint; an endpoint with none of the three makes the close ' +
    'incomplete — splash the endpoint exposes (a consumer of the consumer) joins the same ledger under the same three-way rule.';

const ROW_LAW =
    'Cross-folder needs are DATA, never foreign edits: a change any file outside your write territory needs lands as a crossFolderRows row ' +
    '{targetFile (absolute), language, change, origin} in your receipt document. A missing or version-short package is NEVER installed by ' +
    'you and NEVER edits a central manifest (Directory.Packages.props, pyproject.toml, pnpm-workspace.yaml) from your lane: it lands as a ' +
    'packagesMissing row {package, language, reason} — a dedicated admission lane owns the chain; language names the owning manifest ' +
    'language (csharp|python|typescript), never cross.';

const RESEARCH_LAW =
    'Research debt on unit pages and every file you touch — the map [RESEARCH] census is the queue, plus your own sweep of each unit page ' +
    'terminal [RESEARCH] section: (a) a row PERTINENT to the theme resolves fully via its stated route — member truth through tools.assay ' +
    'api, the .api catalogs, or the named live doc; a confirmed spelling bakes into the owning fence and the row DELETES whole (no ' +
    'tombstones, no resolved notes); a refuted assumption corrects at its root; a genuinely unresolvable row SHARPENS in place with a ' +
    'better question and exact route. (b) an EASY row in ANY file you touch — answerable by one verified read (an .api catalog row, a ' +
    'signature check, one assay probe) — resolves the same way regardless of theme; never leave an easy row standing in a touched file. ' +
    '(c) a HARD non-pertinent row stays byte-untouched and counts as left in your receipt. One ' +
    'carve-out: a row about python version floors or package support gates DELETES as resolved by standing ruling — py315 fully supports ' +
    'every admitted package; never emit python-version gating, markers, or prose anywhere — that is context poison, not a resolution.';

const TESTKIT_LAW =
    'Where the unit touches the testing surface, read ' +
    REPO +
    '/tests/README.md first and execute tests-INFRASTRUCTURE ripples only — root spine, _testkits, imports — never test authoring.';

const MAP_LAW =
    '<role>\nYou are a read-only recon mapper for one work unit of the Rasm planning corpus.\nTreat the task-listed unit pages and the ' +
    'required owner, catalog, and seam sources below as read territory.\nEdit nothing.\nRun no git command.\n</role>\n\n' +
    '<completion_bar>\nDone requires one complete markdown map as the final message.\nObservable completion requires every output-contract ' +
    'section in order, an on-disk source and anchor for every settled row, and [COVERAGE] accounting for every required read.\nPlace every ' +
    'unverified member, page, claim, or seam in [COVERAGE] with the failed probe.\nExclude unverified claims from settled sections.\n' +
    '</completion_bar>\n\n' +
    '<context_gathering>\nRead fully through EOF in this order:\n1. Every unit page the task lists.\n2. Each unit page folder README.md and ' +
    'ARCHITECTURE.md.\n3. Each named package .api catalog, folder tier first and language-root tier second.\n4. Only the anchors explicitly ' +
    'named at each seam-counterpart page.\n\nTreat explicit seam references as the only authority for expanding beyond the unit.\nDo not ' +
    'follow inferred counterparts or adjacent pages.\nRecord an absent task-named or required path in [COVERAGE] with its absolute attempted ' +
    'path and filesystem probe.\nDo not substitute a likely path for a missing path.\nCount a row under a unit page terminal [RESEARCH] ' +
    'section as live when it remains on disk without an explicit resolved, closed, or removed marker.\nRecord a required research field ' +
    'absent from its source as "missing on disk".\nUse at most 60 tool calls across discovery and verification.\nCount every tool invocation ' +
    'against the single budget.\nRead in small batches that preserve complete output.\nDo not concatenate the territory into one command.\n' +
    'Reserve enough calls to re-open every cited anchor during verification.\nStop discovery when the map is complete.\nWhen the remaining ' +
    'budget is required for verification, route unresolved discovery to [COVERAGE] instead of re-reading.\n</context_gathering>\n\n' +
    '<verification>\nRe-open every source anchor cited by a settled row.\nRe-check every cited member against both applicable .api tiers by ' +
    'exact textual spelling.\nTreat a substring, fuzzy match, or paraphrase as unverified.\nClassify a cited member absent from both .api ' +
    'tiers as a phantom in [DEFECTS].\nExclude every phantom from [FACTS].\nVerify each absence claim against the complete census boundary ' +
    'named by its row.\nAdmit a seam to [SEAMS] only when both endpoints, the kind, and the label re-confirm on disk.\nRoute a one-ended or ' +
    'mismatched seam to [DEFECTS] and [COVERAGE].\nRemove any settled row whose evidence fails re-confirmation.\nRecord the removed claim ' +
    'and failed probe in [COVERAGE].\n</verification>\n\n' +
    '<output_contract>\nReturn one markdown document with nothing before or after it.\nUse exactly these H2 sections in this order.\nAdd no ' +
    'other H2 section.\n\n' +
    '## [FACTS]\nUse one "- " row per page capability composed by a fence.\nShape each row as: page: <absolute path> | anchor: <symbol or ' +
    'heading> | owner: <owner> | fence: <fence identifier> | fact: <verified capability>.\n\n' +
    '## [DEFECTS]\nUse one "- " row per verified defect.\nAdmit these classes: facade, seam mirror missing, seam mirror mismatched, registry ' +
    'drift, phantom member, stale card claim.\nShape each row as: class: <class> | source: <absolute path and anchor> | evidence: <observed ' +
    'mismatch> | law: <violated planning law>.\nName the defect and its law.\nDo not state resulting code or remediation.\n\n' +
    '## [UNEXPLOITED]\nUse one "- " row per qualifying .api member by exact name.\nRequire evidence that the unit concept admits the member ' +
    'and that no unit-page fence composes it.\nShape each row as: member: <exact name> | catalog: <absolute .api path and anchor> | ' +
    'admitted-by: <absolute unit-page path and anchor> | absence-boundary: <fences checked>.\n\n' +
    '## [RESEARCH]\nUse one "- " row per live row under any unit page terminal [RESEARCH] section.\nShape each row as: page: <absolute ' +
    'path> | anchor: <row anchor> | question: <source text> | route: <source text or "missing on disk">.\n\n' +
    '## [SEAMS]\nUse one "- " row per verified cross-folder or cross-language seam.\nShape each row as: endpoint-a: <absolute path and ' +
    'anchor> | endpoint-b: <absolute path and anchor> | kind: <exact disk spelling> | label: <exact disk spelling>.\n\n' +
    '## [COVERAGE]\nUse one "- " row per file read, file skipped, or entry left unverified.\nShape each row as: status: read|skipped|' +
    'unverified | source: <absolute path and anchor when present> | reason: <observable evidence or failed probe>.\n\n' +
    'Use "- none" when a section has no qualifying rows.\nKeep every row factual and anchored.\nDo not prescribe changes.\n</output_contract>';

const GUIDE_LAW =
    '<role>\nYou are a blocker-resolution researcher for one BLOCKED planning card.\nTreat the repo and the verification routes named by the ' +
    'task as read territory.\nEdit nothing.\nRun no git command.\nPropose no code.\n</role>\n\n' +
    '<completion_bar>\nDone requires one guidance section resolving the blocker into decision inputs.\nObservable completion requires every ' +
    'output-contract heading in order, one supported verdict, an evidence-backed answer for every route question, and a source for every ' +
    'decision-critical fact.\nPreserve exact member spellings.\nName the decision the unblocking writer must take and the verified ' +
    'constraints that determine it.\nKeep guidance at the level of facts, constraints, and decision boundaries.\nPlace every unverified ' +
    'fact under [UNRESOLVED] with the exact failed probe.\nInclude no fence, snippet, or proposed declaration.\n</completion_bar>\n\n' +
    '<context_gathering>\nRead or execute the required evidence in this order:\n1. Read the blocked card and every page it anchors fully ' +
    'through EOF.\n2. Follow each route named by the task.\n3. Follow only the evidence dependencies required to answer that route.\n\n' +
    'For a .api route, read the applicable catalog entry in context and preserve its exact spelling.\nFor an assay route, run the exact ' +
    'member target named by the task and retain the decisive output.\nFor a live-doc route, search current primary vendor or project ' +
    'documentation and retain the canonical URL plus the decisive section.\nFor a seam-owner route, read the named owner page at the cited ' +
    'anchor.\nTreat verified route evidence as authority for member and external-API facts.\nTreat the planning card as authority for the ' +
    'decision it requires.\nWhen two sources governing the same fact conflict, record both under [UNRESOLVED].\nDo not select a preferred ' +
    'value without route evidence establishing precedence.\nRecord a missing path, unavailable document, blocked network request, or failed ' +
    'assay target as an exact failed probe.\nDo not substitute an inferred route for a missing named route.\nUse at most 40 tool calls ' +
    'across research and verification.\nCount every tool invocation against the single budget.\nStop research when every decision-critical ' +
    'route is answered or when the remaining uncertainty proves the blocker externally blocked.\nDo not broaden into adjacent design ' +
    'questions.\n</context_gathering>\n\n' +
    '<verification>\nRe-open every repo, catalog, seam-owner, and live-document source cited in the guidance.\nRe-run every decisive assay ' +
    'probe cited in the guidance.\nMatch every named member by exact textual spelling.\nTreat a substring, fuzzy result, or paraphrase as ' +
    'unverified.\nVerify that every route question appears once under [ROUTE_ANSWERS].\nAssign RESOLVABLE only when every decision-critical ' +
    'fact has route evidence.\nAssign EXTERNALLY-BLOCKED only when a decision-critical fact remains unavailable after every named route has ' +
    'been attempted.\nSupport EXTERNALLY-BLOCKED with the exact failed probe and its observed result.\nRemove every fact that fails ' +
    're-confirmation.\nMove the removed fact and failed probe to [UNRESOLVED].\n</verification>\n\n' +
    '<output_contract>\nReturn one markdown section with nothing before or after it.\nUse exactly these headings in this order.\nAdd no ' +
    'other H2 or H3 heading.\n\n## [BLOCKER] <card id>\n\n### [VERDICT]\nWrite one line in this shape: RESOLVABLE|EXTERNALLY-BLOCKED — ' +
    '<deciding fact> | source: <verification source>.\n\n### [FACTS]\nUse one "- " row per verified fact.\nShape each row as: fact: ' +
    '<verified statement with exact member spelling> | source: <absolute repo path and anchor, catalog path and anchor, assay target and ' +
    'decisive output, or canonical URL and section>.\n\n### [ROUTE_ANSWERS]\nUse one "- " row per route question.\nShape each row as: ' +
    'question: <route question> | answer: <verified answer> | evidence: <verification source>.\n\n### [DECISION_INPUTS]\nUse one "- " row ' +
    'per required decision input.\nShape each row as: decision: <decision the writer must take> | constraint: <verified determining ' +
    'constraint> | admitted boundary: <verified value or limit> | evidence: <verification source>.\nState facts and constraints.\nDo not ' +
    'state design, code, snippets, fences, or declarations.\n\n### [UNRESOLVED]\nUse one "- " row per unverified item.\nShape each row as: ' +
    'item: <decision-critical fact> | probe: <exact failed probe> | result: <observed failure>.\nWrite "none" when every decision-critical ' +
    'fact is verified.\n</output_contract>';

const IMPL_LAW =
    '<role>\nYou are a design-page writer for one work unit of the Rasm planning corpus. Work at the design layer: markdown design pages ' +
    'are the product, and their code fences carry the real, compilable design. Your write territory is exactly the files under the ' +
    'territory the task names plus its IDEAS.md/TASKLOG.md card files. Edit no source-tree file, central manifest (Directory.Packages.props, ' +
    'pyproject.toml, pnpm-workspace.yaml), or path outside that territory; run no git command.\n</role>\n\n' +
    '<completion_bar>\nDone means all of these observable states hold:\n1. Each realized card has a transcription-complete code fence on its ' +
    'owning page containing the real declaration, members, cases, fields, signatures, and bodies in exact verified spellings; its card file ' +
    'carries [COMPLETE] with a one-line disposition naming the landed owner.\n2. Each card proven already realized by current disk carries ' +
    '[COMPLETE] with a one-line disposition citing the landed fence.\n3. Each card refuted by current disk carries [DROPPED] with a ' +
    'one-line disposition citing the refuting file and anchor.\n4. Each genuinely future card carries [BLOCKED] with a one-line disposition ' +
    'stating the explicit condition that arms it.\n5. Each unit-page [RESEARCH] row reaches one task-defined outcome: a theme-pertinent ' +
    'row resolves via its route — a confirmed spelling lands in the owning fence and the row is removed, a refuted assumption is corrected ' +
    'at its owner and the obsolete row is removed, an unresolvable row is sharpened in place with a better question and exact verification ' +
    'route; an easy row in any touched file — answerable by one verified read — resolves the same way regardless of theme, never left ' +
    'standing in a touched file; a hard non-pertinent row stays untouched and is counted as left; a ' +
    'python version-floor or package-support-gate row is removed as resolved by standing ruling (py315 supports every admitted package; ' +
    'emit no python-version gating, marker, or prose anywhere).\n6. The receipt document exists at the exact absolute path the task names ' +
    'and reports the landed files, card ' +
    'dispositions, research outcomes, routed work, missing packages, and residuals.\n\nImplement exactly and only the task cards. Resolve ' +
    'ambiguity through the simplest interpretation consistent with current disk, the doctrine, and the owning page charter; do not ask ' +
    'questions. Do not land a partial edit for a deliverable that lacks enough evidence for a coherent landing; record one residuals row ' +
    'naming the blocked claim, affected files, and owner. Route each required out-of-territory change to crossFolderRows. Route each ' +
    'missing package to packagesMissing and perform no installation or manifest edit.\n</completion_bar>\n\n' +
    '<context_gathering>\nComplete this read ladder before the first edit:\n1. Read every doctrine root file the task names in full; each ' +
    'is the floor for every fence.\n2. Read every unit page in full.\n3. Read the task-named fact map in full when present; treat each ' +
    'entry as a lead and re-verify its anchor before use.\n4. Read the task-named guidance dossier in full when present; treat its facts ' +
    'and constraints as decision inputs, not design.\n5. Before writing a member, read its exact .api catalog row at the folder tier, then ' +
    'at the language-root tier.\n\nBudget: at most 90 tool calls total across the lane. Read in bounded batches whose outputs do not ' +
    'truncate; never concatenate the territory into one command. Stop gathering when the completion bar is met. At the budget, stop ' +
    'searching: keep every member absent from both .api tiers out of settled fence code, create or sharpen its [RESEARCH] row with the ' +
    'unresolved question and exact route, mark a dependent unrealizable card [BLOCKED] with its verification trigger, and record the ' +
    'blocked claim in residuals.\n</context_gathering>\n\n' +
    '<capability_mandate>\nLand new capability as a row, case, field, operation, or policy value on an existing owner before creating a ' +
    'surface. Create a surface only when current pages prove that every existing owner would violate its charter by carrying the card. ' +
    'Where a card assumes a file, folder, or owner, confirm it on disk first; absent, create it at the full bar and extend it as if it had ' +
    'existed from the start — a minted page meets the same density and coverage bar as a mature page the moment it lands, never a stub or ' +
    'skeleton. Mine both task-named .api tiers to operator depth for the unit concepts; an admitted capability the concept admits and no ' +
    'fence composes is a defect: compose it or record one residuals row with the ruling. ' +
    'Collapse flat repeated shapes and repeated case handling into one denser polymorphic owner. Parameterize repeated literals and ' +
    'per-site spellings as policy rows on that owner. Keep one monadic spine per concern. Preserve every behavior through densification: ' +
    'before removing a repeated shape, map each member, path, and precedence rule it carries to its surviving owner; an unmapped behavior ' +
    'blocks the collapse and lands in residuals.\n</capability_mandate>\n\n' +
    '<verification>\nRe-read each changed region immediately after landing its edit and compare it with the card it realizes. Accept a ' +
    'member spelling only when an exact row in the folder-tier or language-root .api catalog confirms it; use the task-named routes to ' +
    'resolve facts those catalogs delegate. Run no build, test, or compile command against the planning pages. Where the pass minted a ' +
    'new markdown file, run the docgen gate script at ' +
    REPO +
    '/.claude/skills/docgen/scripts/prose_gate.py (invocation per its --help) batched once over the new files after the final edit and ' +
    'repair the FAILs it reports on those files.\n\nAfter the final content ' +
    'edit, re-open every dispositioned card and its cited owner region. Confirm that each marker belongs to [COMPLETE], [DROPPED], or ' +
    '[BLOCKED], that its one-line disposition matches the landed fence, refuting evidence, or arming trigger, and that no task card remains ' +
    'without one terminal disposition. Re-open each unit page terminal [RESEARCH] section and confirm that every original row is removed, ' +
    'corrected at its owner, sharpened with an exact route, or counted as left (hard non-pertinent rows only, byte-untouched). Reconcile ' +
    'filesTouched against the design-page and card-file edits, ' +
    'cardsClosed against the task-card roster, researchResolved against the final research outcomes, and every routing, package, and ' +
    'residual row against current disk before writing the receipt.\n</verification>\n\n' +
    '<output_contract>\nWrite the receipt document to the exact absolute path the task names as the final filesystem action before the ' +
    'final message. Write one JSON object with exactly these keys and no others: {folder, stage, filesTouched, crossFolderRows, ' +
    'cardsClosed, packagesMissing, researchResolved, residuals, summary}.\n\nfolder and stage use the exact task values. filesTouched is ' +
    'the actual design-page and card-file edit roster as absolute paths. crossFolderRows is [{targetFile, language, change, origin}]. ' +
    'cardsClosed is [{file, card, disposition}], with disposition using only [COMPLETE], [DROPPED], or [BLOCKED] and carrying the same ' +
    'citation or arming trigger recorded in the card file. packagesMissing is [{package, language, reason}]. researchResolved is {resolved, ' +
    'sharpened, left}, with nonnegative counts matching the final research outcomes. residuals is [{claim, files, owner}]. summary is one ' +
    'evidence-grounded account of the landed outcome. Every path value is absolute. Use [] for an empty list; do not guess.\n\nYour final ' +
    'message is the same JSON object only, with no prose outside it and no code fence.\n</output_contract>';

const TMAP_LAW =
    '<role>\nYou are a read-only recon mapper for one language tests estate.\nTreat the paths named by the task and the required ' +
    'registry-depth sources below as read territory.\nEdit nothing.\nRun no git command.\n</role>\n\n' +
    '<completion_bar>\nDone requires one complete markdown map as the final message.\nObservable completion requires every output-contract ' +
    'section in order, an on-disk source and anchor for every settled row, and [COVERAGE] accounting for every required read.\nPlace every ' +
    'unverified file, capability, drift claim, or spine claim in [COVERAGE] with the failed probe.\nExclude unverified claims from settled ' +
    'sections.\n</completion_bar>\n\n' +
    '<context_gathering>\nRead fully through EOF in this order:\n1. Read the tests README, inventory the language tests tree named by the ' +
    'task, and read every file in that tree.\n2. Read every root spine file named by the task.\n3. Read each applicable language-libs ' +
    'README package-registry section, the testing-relevant .api catalogs the task names, and every observability page the task names.\n\n' +
    'Treat the named tests tree, named spine files, package registries, named catalogs, and named observability pages as the census ' +
    'boundary.\nDo not expand into unlisted implementation pages.\nRecord an absent task-named or required path in [COVERAGE] with its ' +
    'absolute attempted path and filesystem probe.\nDo not substitute a likely path for a missing path.\nUse at most 60 tool calls across ' +
    'discovery and verification.\nCount every tool invocation against the single budget.\nRead in small batches that preserve complete ' +
    'output.\nDo not concatenate the territory into one command.\nReserve enough calls to re-open every cited anchor during verification.\n' +
    'Stop discovery when the estate map is complete.\nWhen the remaining budget is required for verification, route unresolved discovery to ' +
    '[COVERAGE] instead of re-reading.\n</context_gathering>\n\n' +
    '<verification>\nRe-open every source anchor cited by a settled row.\nRe-confirm each _testkit and harness fact against its owning ' +
    'file.\nVerify each [GAPS] absence claim across the complete named tests-tree boundary.\nName the build-out axis governing each ' +
    'verified gap.\nVerify each [DRIFT] claim by comparing the tests-estate claim with the current libs registry or observability anchor.\n' +
    'Verify each [SPINE] claim against both the root-config row and the affected tests-estate evidence.\nTreat a substring, inferred ' +
    'relationship, or unmatched name as unverified.\nRemove any settled row whose evidence fails re-confirmation.\nRecord the removed ' +
    'claim and failed probe in [COVERAGE].\n</verification>\n\n' +
    '<output_contract>\nReturn one markdown document with nothing before or after it.\nUse exactly these H2 sections in this order.\nAdd ' +
    'no other H2 section.\n\n' +
    '## [FACTS]\nUse one "- " row per verified _testkit or harness inventory fact.\nShape each row as: source: <absolute path> | anchor: ' +
    '<symbol or heading> | owner: <_testkit or harness owner> | fact: <verified current capability>.\n\n' +
    '## [GAPS]\nUse one "- " row per verified capability gap against a build-out axis named by the task.\nShape each row as: axis: <exact ' +
    'task spelling> | gap: <absent capability> | census-boundary: <files or tree checked> | evidence: <observable absence evidence>.\n\n' +
    '## [DRIFT]\nUse one "- " row per stale or misaligned tests-estate content item.\nShape each row as: tests-source: <absolute path and ' +
    'anchor> | drift: <stale or misaligned claim> | libs-source: <absolute registry or observability path and anchor> | current-fact: ' +
    '<verified libs fact>.\n\n' +
    '## [SPINE]\nUse one "- " row per root-config test row misaligned with the estate.\nShape each row as: spine-source: <absolute path and ' +
    'anchor> | row: <exact row identity> | estate-source: <absolute path and anchor> | mismatch: <verified misalignment>.\n\n' +
    '## [COVERAGE]\nUse one "- " row per file read, file skipped, or entry left unverified.\nShape each row as: status: read|skipped|' +
    'unverified | source: <absolute path and anchor when present> | reason: <observable evidence or failed probe>.\n\n' +
    'Use "- none" when a section has no qualifying rows.\nKeep every row factual and anchored.\nDo not prescribe changes.\n</output_contract>';

// --- [OPERATIONS] --------------------------------------------------------------------

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const makeSlots = (cap) => {
    let active = 0;
    let gate = Promise.resolve();
    const waiters = [];
    const stagger = () => {
        gate = gate.then(() => sleep(STAGGER_MS));
        return gate;
    };
    return async (fn) => {
        if (active >= cap) await new Promise((res) => waiters.push(res));
        active++;
        await stagger();
        try {
            return await fn();
        } finally {
            active--;
            const next = waiters.shift();
            if (next) next();
        }
    };
};
const slot = makeSlots(CAP);
const guard = (p) => p.catch(() => null);
const sum = (xs, f) => xs.reduce((a, x) => a + (f(x) || 0), 0);

const baseOf = (f) => {
    const segs = f.path.replace(/\/+$/, '').split('/');
    const tail = segs[segs.length - 1];
    return tail === '.planning' ? (f.language === 'cross' ? 'libs' : 'branch') : tail;
};
const fid = (f) => (SHORT[f.language] || 'xx') + '-' + slugOf(baseOf(f));
const uid = (f, i) => fid(f) + '-u' + (i + 1);
const planningRootOf = (f) => (f.path.endsWith('/.planning') ? f.path : f.path + '/.planning');
const apiTiersOf = (f) =>
    f.language === 'cross'
        ? REPO + '/libs/csharp/.api, ' + REPO + '/libs/python/.api, and ' + REPO + '/libs/typescript/.api'
        : (f.path.endsWith('/.planning') ? '' : f.path + '/.api and ') + REPO + '/libs/' + f.language + '/.api';
const doctrineOf = (f) =>
    f.language === 'cross'
        ? REPO +
          '/libs/.planning/README.md, ' +
          REPO +
          '/libs/.planning/architecture.md, and ' +
          REPO +
          '/libs/.planning/campaign-method.md (the cross-libs law), plus for every fence you touch the owning language doctrine — the root ' +
          'files of ' +
          REPO +
          '/docs/stacks/<language>/ for each language whose pages the unit edits'
        : 'every root file of ' + REPO + '/docs/stacks/' + f.language + '/ (the doctrine floor)';
const censusOf = (f) =>
    f.language === 'cross'
        ? CAMP + '/synthesis.md ([08]-[CROSS_TRACK_RULES] and [09]-[RESIDUALS]) and ' + CAMP + '/residuals.md'
        : CAMP + '/realize/drain-' + f.language + '.md (escalated rows and unreviewed-chain sections) and ' + CAMP + '/residuals.md';
const territoryOf = (f) =>
    f.language === 'cross'
        ? 'libs-wide for this unit — every touchpoint page its cards name across ' +
          REPO +
          '/libs plus ' +
          f.path +
          '; both ends of every touched seam land in the same pass, and a cross-libs card closes only when every folder ripple is on disk'
        : f.path + ' only';
const memoryClause = (f) =>
    f.language === 'csharp' || f.language === 'cross'
        ? ', and the memory index at /Users/bardiasamiee/.claude/projects/-Users-bardiasamiee-Documents-99-Github-Rasm/memory/MEMORY.md — ' +
          'open every reference_* entry naming a surface this unit composes (RhinoCommon, GH2, Eto, LanguageExt, Thinktecture, telemetry traps)'
        : '';
const pyGuard = (k) =>
    k === 'python'
        ? ' Standing ruling: py315 supports every admitted package — never emit python-version gating, floor markers, or version prose ' +
          'anywhere; such content found on any surface is a defect you delete on sight.'
        : '';
const mapClause = (mapR) =>
    mapR && mapR.ok
        ? 'at ' +
          mapR.report +
          ' IN FULL from disk — its settled rows arrive twice-verified: compose them directly, re-opening only a row whose spelling a fence ' +
          'composes as settled code; its [UNEXPLOITED] rows are your composition queue and its [DEFECTS] rows your repair queue.'
        : '— NONE landed for this unit; your own full territory read replaces it.';
const guideClause = (u, guideR) =>
    u.blockers.length
        ? ' BLOCKED CARDS: read the guidance dossier ' +
          (guideR && guideR.ok ? 'at ' + guideR.report + ' IN FULL from disk' : '— the guidance leg failed; run its verification routes yourself') +
          '; guidance is decision input (verified facts, spellings, route answers), never design — the design is yours. Implement each unblock; ' +
          'a blocker proven genuinely unresolvable (external dependency, frozen wire) closes its card back to [BLOCKED] with a sharpened arming ' +
          'condition citing the dossier path; every other blocked card resolves and realizes in this pass.'
        : '';
const admitClause = (admitted) =>
    admitted && admitted.length
        ? ' ADMITTED MID-RUN for this unit: ' +
          JSON.stringify(admitted.map((a) => ({ package: a.package, language: a.language, admitted: !!(a.ok && a.admitted), catalog: a.report }))) +
          ' — compose each admitted package catalog members where the unit concept admits them; a rejected admission stays out of the fences.'
        : '';
const receiptDoc = (id, stage) =>
    'Write your COMPLETE receipt document as one JSON object to ' +
    OUT +
    '/' +
    id +
    '-' +
    stage +
    '.json (Write tool, absolute path): {folder, stage, filesTouched: [absolute paths], crossFolderRows: [{targetFile, language, change, ' +
    'origin}], rippleLedger: [{endpoint, disposition: "landed"|"deferred"|"out-of-theme", evidence}] — one row per unit rippleSurface ' +
    'endpoint plus every splash endpoint your work exposed, cardsClosed: [{file, card, disposition}], packagesMissing: [{package, ' +
    'language, reason}], researchResolved: {resolved, ' +
    'sharpened, left}, residuals: [{claim, files, owner}], summary}.';
const thinReceipt = (f, stage) =>
    ' Then return the thin receipt: folder ' +
    f.path +
    ', stage "' +
    stage +
    '", ok, report, filesTouched (absolute), crossFolderRows/cardsClosed/researchResolved/residuals counts, packagesMissing rows copied ' +
    'verbatim from your receipt document, one-line headline, failure empty.';

const discoverPrompt = () =>
    'Resolve the card-owning folder roster from disk. Folders: every package directory directly under ' +
    REPO +
    '/libs/csharp, ' +
    REPO +
    '/libs/python, and ' +
    REPO +
    '/libs/typescript that carries IDEAS.md or TASKLOG.md at its root (skip node_modules and dot-directories); plus each branch tier ' +
    REPO +
    '/libs/{csharp,python,typescript}/.planning where card files exist; plus the cross-libs tier ' +
    REPO +
    '/libs/.planning. Per folder count OPEN cards: lines matching ^\\[[A-Za-z0-9_-]+\\]-\\[(ACTIVE|QUEUED|BLOCKED)\\] inside the [01]-[OPEN] ' +
    'section of IDEAS.md plus TASKLOG.md ("(none)" is zero). Return folders as {path (absolute), language, openCards}; language derives from ' +
    'the path segment under libs/, and libs/.planning is language "cross".' +
    (SCOPE.length ? ' Restrict the roster to folders matching any of: ' + JSON.stringify(SCOPE) + '.' : '');

const planPrompt = (f) =>
    'POOL INGESTION for ' +
    f.path +
    ' (' +
    f.language +
    '). ' +
    PATH_LAW +
    ' Read from disk, in order: (1) ' +
    f.path +
    '/IDEAS.md and ' +
    f.path +
    '/TASKLOG.md — the [01]-[OPEN] sections are the pool; a card leader is [ID]-[ACTIVE|QUEUED|BLOCKED]. (2) Census findings: ' +
    censusOf(f) +
    ' — rows naming this folder join the pool as work. (3) ' +
    f.path +
    '/README.md (registry) and ' +
    f.path +
    '/ARCHITECTURE.md ([SEAMS]) for the packages and seams units compose. (4) Research debt: sweep the terminal [RESEARCH] section of every ' +
    'design page under ' +
    planningRootOf(f) +
    ' and note pages carrying live rows. The reads are the FULL pool — ingestion never narrows by theme; selection does. ' +
    THEME_LAW +
    ' SELECTION: judge every open card and census row against the charter for THIS folder — nuanced per-folder judgment, never keyword ' +
    'matching; borderline relevance INCLUDES. EMIT typed WORK UNITS covering the theme-relevant pool: every theme-relevant open card joins ' +
    'exactly one unit — none excluded, none deferred; a non-theme card joins leftovers, never a unit, and stays untouched. Pages with ' +
    'theme-pertinent [RESEARCH] rows join the unit owning those pages, and theme-pertinent rows on pages outside every card unit form ' +
    'one final research-drain unit ordered last; a non-pertinent row on a page no unit touches stays. A unit is a folder-scoped card ' +
    'cluster one writer lands in one pass: cluster cards sharing ' +
    'pages or one owner surface; never one unit per card when cards share a page, never one mega-unit over page-disjoint clusters. Size ' +
    'units for one writer single pass — roughly 3 to 8 pages of real work: split a folder-swallowing mega-unit at its page-disjoint ' +
    'cluster boundaries and merge card-sized fragments up; agent spin-up overhead swamps a tiny unit, and an oversized unit starves its ' +
    'critique. Per unit: ' +
    'order (dependency position, 1 first — a unit consuming another unit fence output orders after it, and a unit that MINTS a page or ' +
    'sub-folder orders before every unit extending it); title; tier; heavy (true when the ' +
    'unit spans 3+ pages or creates any page); cards (exact "FILE [ID]" spellings); pages (ABSOLUTE page paths the unit edits, index docs ' +
    'included when touched); anchors (owner symbols and fence names); packages (admitted package names the unit composes); rippleSurface — ' +
    'the unit ripple contract: for each selected card, every consumer, unlock endpoint, or counterpart the card NAMES in its Unlocks, ' +
    'Shape, Ripple, or thesis lands one row {endpoint (the folder or page it names, absolute where resolvable — cross-folder and ' +
    'cross-language included), source ("FILE [ID] Unlocks|Shape|Ripple")} — [] only when no selected card names one; blockers — every ' +
    'BLOCKED card in the unit lands one row {card, question (the blocker restated as an answerable question), route (the resolution route: ' +
    'an assay api member target, a .api catalog, a live doc, a seam-owner page)} — [] when none. TIER is decision weight, never modality: ' +
    '"fable" = design synthesis — new owners, ground-up or multi-page authoring, contested judgment; "opus" = surgical realization on ' +
    'settled owners — fence edits, card closes, conformance; "codex" = transcription at scale where every decision is already on the page. ' +
    'A unit carrying blockers tiers "fable" or "opus", never "codex".' +
    (f.language === 'cross'
        ? ' CROSS-LIBS DECOMPOSITION: a cross-libs card is a multi-folder work unit — decompose it into per-folder ripple executions inside ' +
          'ONE unit: pages list every touchpoint page across libs/ grouped by folder, anchors name both ends of every seam, the title names ' +
          'the folders spanned; the card closes at this tier only when every folder ripple lands.'
        : '') +
    ' LEFTOVERS: every non-theme OPEN card lands one row {item (exact "FILE [ID]" spelling), hint (one short phrase naming the future ' +
    'functionality set it belongs to)}; a hard non-pertinent research cluster on pages no unit touches lands {item: "RESEARCH <absolute ' +
    'page path>", hint}. Zero theme-relevant work = units [] (leftovers still rostered) with note naming what you checked. Units are ' +
    'navigation facts, never verdicts or designs.';

const mapTask = (f, u) =>
    'Map unit "' +
    u.title +
    '" of ' +
    f.path +
    '. UNIT PAGES (read in full): ' +
    JSON.stringify(u.pages) +
    '. CARDS: ' +
    JSON.stringify(u.cards) +
    '. ANCHORS: ' +
    JSON.stringify(u.anchors) +
    '. PACKAGES: ' +
    JSON.stringify(u.packages) +
    '. API TIERS: ' +
    apiTiersOf(f) +
    '. FOLDER: ' +
    f.path +
    '.';

const mapWrapPrompt = (f, u) => {
    const report = OUT + '/' + u.id + '-map.md';
    return (
        'DISPATCH ROLE: codex performs the complete TASK below through one blocking codex MCP call; never perform, edit, judge, or relay the ' +
        'work yourself. (1) ToolSearch "select:mcp__codex__codex". (2) Call mcp__codex__codex ONCE with model="gpt-5.6-sol", cwd="' +
        REPO +
        '", sandbox="read-only", approval-policy="never", ' +
        (u.heavy ? '' : 'config={"model_reasoning_effort":"medium"}, ') +
        '"developer-instructions" = the LANE LAW block below VERBATIM, prompt = the TASK block below VERBATIM. On a tool error retry ONCE ' +
        'with a sharpened prompt — your whole recovery budget. (3) The tool result is a JSON envelope {threadId, content}; Write the CONTENT ' +
        'text (never the envelope) unmodified to ' +
        report +
        ' (delete any leftover file there first). (4) Verify with one Bash call: grep -oE "^## \\[(FACTS|DEFECTS|UNEXPLOITED|RESEARCH|' +
        'SEAMS|COVERAGE)\\]" ' +
        report +
        ' | sort -u | wc -l — a count under 6 means a missing contract section, a malformed product: rewrite once from the tool result, ' +
        'then return ok=false with the miss. ' +
        '(5) Return ok, report, entries = the "- " row count under [DEFECTS], research = the "- " row count under [RESEARCH] (Read the file ' +
        'to count per section; "- none" counts zero), headline = "<defects>d <unexploited>u <research>r", failure empty — or ok=false with ' +
        'the error text VERBATIM.\n\nLANE LAW:\n\n' +
        MAP_LAW +
        '\n\nTASK:\n\n' +
        mapTask(f, u)
    );
};

const guideWrapPrompt = (f, u) => {
    const report = OUT + '/' + u.id + '-guidance.md';
    return (
        'DISPATCH ROLE: run ' +
        u.blockers.length +
        ' SEQUENTIAL blocking codex MCP calls, one per BLOCKER row below; never research, judge, or answer any blocker yourself. (1) ' +
        'ToolSearch "select:mcp__codex__codex" once. (2) Per row call mcp__codex__codex with model="gpt-5.6-sol", cwd="' +
        REPO +
        '", sandbox="read-only", approval-policy="never", config={"model_reasoning_effort":"medium","web_search":"live"}, ' +
        '"developer-instructions" = the LANE LAW block below VERBATIM, prompt = "Resolve this blocker into decision inputs. CARD: <card>. ' +
        'QUESTION: <question>. ROUTE: <route>. FOLDER: ' +
        f.path +
        '." with the row values substituted. On a tool error retry that row ONCE, then record it failed and continue. (3) Each tool result ' +
        'is a JSON envelope {threadId, content}; concatenate the CONTENT texts (never envelopes) in row order and Write the result to ' +
        report +
        ' (delete any leftover file first). (4) Verify with one Bash call: grep -c "^## \\[BLOCKER\\]" ' +
        report +
        ' — the count must equal ' +
        u.blockers.length +
        '; on a mismatch rewrite once from the tool results, then return ok=false with the miss. (5) Return ok, report, entries = the ' +
        'RESOLVABLE verdict count, research = 0, headline = "<resolvable>/<total> resolvable", failure = the failed row cards or empty.' +
        '\n\nLANE LAW:\n\n' +
        GUIDE_LAW +
        '\n\nBLOCKER ROWS:\n\n' +
        JSON.stringify(u.blockers)
    );
};

const implPrompt = (f, u, mapR, guideR) =>
    'IMPLEMENT unit "' +
    u.title +
    '" of ' +
    f.path +
    ' at the campaign bar. ' +
    THEME_LAW +
    ' Write territory: ' +
    territoryOf(f) +
    '. ' +
    PATH_LAW +
    ' Read AT SOURCE, in full, before any edit: ' +
    doctrineOf(f) +
    memoryClause(f) +
    '. Then run your OWN blind pass over the unit pages — defect list and design rulings from disk FIRST; then read the fact map ' +
    mapClause(mapR) +
    guideClause(u, guideR) +
    ' Realize every card into transcription-complete code FENCES inside the owning design pages — the real declaration with members, cases, ' +
    'fields, signatures, and bodies in exact verified spellings; a member unverifiable against a .api tier lands as a RESEARCH row, never ' +
    'settled fence code. ' +
    FENCE_LAW +
    ' ' +
    CARD_LAW +
    ' ' +
    ROW_LAW +
    ' ' +
    RESEARCH_LAW +
    ' ' +
    TESTKIT_LAW +
    ' ULTRA-STACKING: mine BOTH .api altitudes (' +
    apiTiersOf(f) +
    ') to operator depth for the unit concepts — an admitted capability the concept admits but no fence composes is a named defect: ' +
    'compose it or record it in residuals with the ruling. Where your pass mints a NEW markdown file, run the docgen gate script at ' +
    REPO +
    '/.claude/skills/docgen/scripts/prose_gate.py (invocation per its --help) batched once over the new files after the final edit, ' +
    'repaired to zero FAIL. UNIT: cards ' +
    JSON.stringify(u.cards) +
    '; pages ' +
    JSON.stringify(u.pages) +
    '; anchors ' +
    JSON.stringify(u.anchors) +
    '; packages ' +
    JSON.stringify(u.packages) +
    '. ' +
    receiptDoc(u.id, 'implement') +
    thinReceipt(f, 'implement');

const codexImplTask = (f, u, mapR, guideR) =>
    'Goal: realize unit "' +
    u.title +
    '" of ' +
    f.path +
    ' — every card below into transcription-complete fences, every card to a terminal disposition, every unit-page [RESEARCH] row to its ' +
    'outcome.\n' +
    'Theme: ' +
    THEME_LAW +
    '\n' +
    'Context: write territory ' +
    territoryOf(f) +
    '; doctrine root files ' +
    doctrineOf(f) +
    '; unit pages ' +
    JSON.stringify(u.pages) +
    '; cards ' +
    JSON.stringify(u.cards) +
    '; anchors ' +
    JSON.stringify(u.anchors) +
    '; packages ' +
    JSON.stringify(u.packages) +
    '; api tiers ' +
    apiTiersOf(f) +
    '; fact map ' +
    (mapR && mapR.ok
        ? mapR.report + ' (twice-verified navigation facts — compose directly; re-open only a spelling a fence settles)'
        : 'none — your own read replaces it') +
    (u.blockers.length
        ? '; guidance dossier ' +
          (guideR && guideR.ok
              ? guideR.report +
                ' (decision inputs, never design; an EXTERNALLY-BLOCKED verdict closes its card back to [BLOCKED] with a sharpened arming condition citing the dossier)'
              : 'failed — run its routes yourself')
        : '') +
    '.\nResearch step: a theme-pertinent row verifies via its stated route (tools.assay api, .api catalogs, named docs) — bake confirmed ' +
    'spellings into the owning fence and DELETE the row whole, correct refuted assumptions at the root, sharpen unresolvable rows in ' +
    'place; an easy row in any touched file (one verified read answers it) resolves the same way regardless of theme; a hard ' +
    'non-pertinent row stays untouched, counted as left; delete python version-floor and ' +
    'package-support-gate rows as resolved by standing ruling (py315 supports every admitted package — emit no version gating, marker, or ' +
    'prose anywhere).\n' +
    'Constraints: absolute paths everywhere; no source files; no manifest edits; no package installs.\n' +
    'Done when: every listed card dispositioned, fences landed, research drained, and the receipt document written to ' +
    OUT +
    '/' +
    u.id +
    '-implement.json.';

const codexImplWrapPrompt = (f, u, mapR, guideR) => {
    const report = OUT + '/' + u.id + '-implement.json';
    return (
        'DISPATCH ROLE: codex performs the complete TASK below through one blocking codex MCP call; never perform, edit, judge, or relay the ' +
        'work yourself. (1) ToolSearch "select:mcp__codex__codex". (2) Call mcp__codex__codex ONCE with model="gpt-5.6-sol", cwd="' +
        REPO +
        '", sandbox="workspace-write", approval-policy="never", "developer-instructions" = the LANE LAW block below VERBATIM, prompt = the ' +
        'TASK block below VERBATIM. On a tool error retry ONCE with a sharpened prompt — your whole recovery budget. (3) Codex writes its ' +
        'own receipt document to ' +
        report +
        ' as its final act; verify with one Bash call: jq -e ".filesTouched and .cardsClosed" ' +
        report +
        ' >/dev/null — on a miss, Write the tool-result CONTENT (the final-message JSON, never the envelope) to that path and re-probe; a ' +
        'second miss returns ok=false with the probe error. (4) Return the thin receipt: folder ' +
        f.path +
        ', stage "implement", ok, report, filesTouched = jq ".filesTouched", crossFolderRows/cardsClosed/residuals = the jq array lengths, ' +
        'researchResolved = jq ".researchResolved.resolved", packagesMissing = jq ".packagesMissing" rows verbatim, one-line headline, ' +
        'failure empty — or ok=false with the error text VERBATIM.\n\nLANE LAW:\n\n' +
        IMPL_LAW +
        '\n\nTASK:\n\n' +
        codexImplTask(f, u, mapR, guideR)
    );
};

const admitPrompt = (row, origin) =>
    'ADMISSION LANE for package "' +
    row.package +
    '" (' +
    row.language +
    '), requested by ' +
    origin +
    ': ' +
    row.reason +
    '. ' +
    PATH_LAW +
    ' You are the SINGLE writer on the ' +
    row.language +
    ' central manifest for this call. Execute the FULL admission chain: (1) LIVE-VERIFY the newest stable version — nuget MCP ' +
    'get_latest_package_version for csharp, the live registry for python/typescript; supersession-only rejection: reject ONLY when an ' +
    'already-admitted package supersedes the capability, returning admitted=false with the ruling. (2) Land the central manifest row in its ' +
    'owning group (Directory.Packages.props label group / pyproject.toml lean unpinned / pnpm-workspace.yaml cluster). (3) Prove the ' +
    'install gate green (dotnet restore over the consuming closure / uv sync / pnpm install), self-healing or reverting what cannot ' +
    'resolve — a reverted admission returns admitted=false with the resolver evidence. (4) Author the .api catalog at the correct tier — ' +
    'folder overlay vs language-root substrate per the two-tier law — with verified members only. (5) Land the README registry row at the ' +
    'consuming folder. (6) Land the csproj reference where consumed (csharp). (7) Run the docgen gate script at ' +
    REPO +
    '/.claude/skills/docgen/scripts/prose_gate.py (invocation per its --help) over the new files, repaired to zero FAIL. Write your full ' +
    'admission report to ' +
    OUT +
    '/admit-' +
    slugOf(row.package) +
    '.md, then return: package, language, ok, admitted, report, filesTouched, headline, failure.';

const critPrompt = (f, u, impl, mapR, admitted) =>
    'CRITIQUE unit "' +
    u.title +
    '" of ' +
    f.path +
    ' — the clause-by-clause doctrinal-conformance and capability-completeness audit, repaired IN PLACE with the writer full authority. ' +
    THEME_LAW +
    ' A non-theme open card stays byte-untouched — an expected leftover for a future slice. Write territory: ' +
    territoryOf(f) +
    '. ' +
    STANCE +
    ' COLD PASS FIRST: derive your own defect list from the unit pages on disk before any prior claim; the implement stage touched ' +
    JSON.stringify((impl && impl.filesTouched) || []) +
    ' (navigation only — look here first, bound nothing). Then read ' +
    (mapR && mapR.ok ? mapR.report + ' and ' : '') +
    ((impl && impl.report) || '(no implement receipt)') +
    ' IN FULL from disk as unverified prior claims to refute against current disk. Doctrine at source first: ' +
    doctrineOf(f) +
    '.' +
    admitClause(admitted) +
    ' Run the named checklists as a FLOOR and hunt past them: collapse scan (repeated shapes, parallel spellings, enumerable families an ' +
    'algebra, table, fold, or generator can own); owner choice; knob test (entry-point sprawl, boolean knobs); rail unification; language ' +
    'modernity; capability + illusion (a name promising capability the fence omits); fence transcription-completeness; member truth against ' +
    'both .api tiers (a cited member that fails verification is a phantom you delete or convert to a RESEARCH row); card-closure truth (a ' +
    'closed card whose fence is partial REOPENS; a [BLOCKED] re-card without an arming trigger gains one); ripple-ledger truth (every unit ' +
    'rippleSurface endpoint carries a rippleLedger row in the implement receipt whose disposition holds against disk — a landed endpoint ' +
    'proven, a deferred endpoint present as a crossFolderRows row, an out-of-theme endpoint carrying its hint; a silent or false endpoint ' +
    'is a defect you land or ledger yourself); research-row discipline (a ' +
    'resolved row deleted whole; a sharpened row carrying question and route; a hard non-pertinent row left byte-untouched, never ' +
    'dispositioned; any emitted python-version gating is a defect you remove — ' +
    'py315 supports every admitted package); .api ultra-stacking (both tiers — ' +
    apiTiersOf(f) +
    ' — enumerated for the unit packages; an unexploited admitted capability the concept admits is a defect you compose in); fresh-page ' +
    'depth (a page minted this run meets the mature-page density and coverage bar); card-assumed existence (an owner a card assumed exists ' +
    'on disk at the full bar — a missing or stubbed assumed owner is a defect you fix). Every hit is a FIX landed in this pass, never a ' +
    'note; the work ends objectively denser than ' +
    'implement left it, or your attack proves the strongest form present by finding nothing. ' +
    ROW_LAW +
    ' ' +
    PATH_LAW +
    ' ' +
    receiptDoc(u.id, 'critique') +
    thinReceipt(f, 'critique');

const composePrompt = (f, u, landed) =>
    'COMPOSE pass for unit "' +
    u.title +
    '" of ' +
    f.path +
    ' — packages admitted AFTER the critique: ' +
    JSON.stringify(landed.map((a) => ({ package: a.package, language: a.language, catalog: a.report }))) +
    '. Write territory: ' +
    territoryOf(f) +
    '. ' +
    PATH_LAW +
    ' Read each admitted package catalog at its landed tier IN FULL, then compose its members into the unit owning fences wherever the ' +
    'unit concept admits them — as rows, cases, fields, operations, or policy values on existing owners, at the campaign bar. ' +
    FENCE_LAW +
    ' A package whose capability the unit concept does not admit records one residuals row naming the ruling; edit nothing the admitted ' +
    'catalogs do not motivate. ' +
    ROW_LAW +
    ' UNIT pages: ' +
    JSON.stringify(u.pages) +
    '. ' +
    receiptDoc(u.id, 'compose') +
    thinReceipt(f, 'compose');

const drainPrompt = (L, receiptPaths) =>
    'DRAIN ' +
    L.key +
    ' — terminal cross-folder reconciler. Write territory: EXACTLY ' +
    REPO +
    '/' +
    L.root +
    '; never a sibling language tree, never a central manifest, never ' +
    REPO +
    '/libs/.planning. ' +
    PATH_LAW +
    ' Stage receipt documents (each one JSON object carrying a crossFolderRows array): ' +
    JSON.stringify(receiptPaths) +
    '. Roster the rows with ONE batched jq sweep over those files first, then read IN FULL each receipt carrying a row whose targetFile ' +
    'lies under your territory before applying it — rows originate from every language and the cross ' +
    'tier. Per row, REFUTE FIRST against current disk (the row predates later stages): a row already satisfied records verified-satisfied ' +
    'with the disk citation; a row conflicting with a landed page law records not-applied with the citation; the remainder APPLIES at root ' +
    '— land the change, mirror every touched seam edge at both endpoint ARCHITECTUREs with identical [KIND] and direction, and repair what ' +
    'the edit exposes in the same pass. Rows targeting outside your territory RETURN as residualRows ("targetFile :: change"), never apply — ' +
    'and a NEW cross-territory need an application exposes (second-order splash) joins residualRows the same way, never dropped. ' +
    pyGuard(L.key) +
    ' Write your full drain document (drained / verified-satisfied / not-applied-with-reason / escalated) to ' +
    OUT +
    '/drain-' +
    L.key +
    '.md, then return: language "' +
    L.key +
    '", ok, report, applied count, residualRows, headline, failure.';

const rtPrompt = (L, drainR, receiptPaths, admitRows, residualRows) =>
    'TERMINAL RED-TEAM over ' +
    REPO +
    '/' +
    L.root +
    ' — the whole language tree, the campaign last adversarial stage. Write territory: that tree, plus the ' +
    L.key +
    ' central manifest for closure repairs only. THEME SLICE "' +
    THEME.title +
    '": this run realized one functionality set; non-theme open cards are expected leftovers — never close, edit, or attack them; ' +
    'card-closure truth applies to the cards the stage receipts disposition. ' +
    STANCE +
    ' Pre-mortem that REBUILDS rather than annotates, every fix landed in place: (1) COUNTERFACTUAL on core owners the campaign touched — ' +
    'remove the chosen owner kind, hand-enumerated space, call-site dispatch, or hand-rolled kernel; derive the form without it; build the ' +
    'stronger form where one exists (the current shape also working is never a refutation). (2) DIFF-OF-NEXT-FEATURE — the next case, ' +
    'dimension, or modality lands as one row with every consumer untouched or loudly broken; where it cannot, restructure the owner. (3) ' +
    'PHANTOM HUNT — campaign-cited members verified against both .api tiers; an unverifiable member is deleted or converted to a RESEARCH ' +
    'row. (4) BOUNDARY INTEGRITY — every concern graded against ' +
    REPO +
    '/' +
    L.root +
    '/.planning/ARCHITECTURE.md: a concern owned twice, scattered, or coupled to a sibling interior instead of a [SEAMS] edge is fixed or ' +
    'recorded. (5) .API ULTRA-STACKING — both .api tiers enumerated in full; an admitted capability the concept admits but no owner ' +
    'exploits is a defect you close. (6) FOUR-SURFACE CLOSURE — bidirectional drift proof across the central manifest <-> the .api catalogs ' +
    '(both tiers) <-> the README registries <-> the project manifests (csproj rows for csharp): every admitted package present on every ' +
    'owning surface, no orphan rows, no missing rows, no duplicates; a folder .api file duplicating a branch-substrate catalog collapses to ' +
    'a folder-specific overlay or deletes per the two-tier law. Closure repairs on the central manifest are yours (orphan and duplicate ' +
    'rows only, surgical anchored Edits — a tests-estate lane may hold test rows in the same file); a NEW admission routes as a residual ' +
    'row. (7) DOCGEN-ZERO PROOF — run the docgen gate script at ' +
    REPO +
    '/.claude/skills/docgen/scripts/prose_gate.py (invocation per its --help) batched over every index doc your pass touched, repaired to ' +
    'zero FAIL. (8) FULL COLD RE-REVIEW of every conformance dimension by name, and card-closure truth: a closed card whose realization ' +
    'fails your attack REOPENS with the defect named; count it in cardsReopened. (9) LINGERING-UNLOCK SWEEP — every closed card ripple ' +
    'surface re-proven from the stage-receipt rippleLedger rows: a landed endpoint verified on disk, a deferred endpoint verified applied ' +
    '(drain document or disk), an out-of-theme endpoint verified hinted; a lingering endpoint you land or return in residualRows. ' +
    'Doctrine at source first: every root file of ' +
    REPO +
    '/docs/stacks/' +
    L.key +
    '/. Consume from disk as unverified claims to refute: the drain document at ' +
    ((drainR && drainR.report) || '(no drain document)') +
    ' and the stage receipts ' +
    JSON.stringify(receiptPaths) +
    '. MID-RUN ADMISSIONS to compose-check under (5) and (6): ' +
    JSON.stringify(admitRows) +
    ' — an admitted row whose catalog capability no owner composes is an ultra-stacking defect; a rejected row stays out of every fence. ' +
    'DRAIN RESIDUALS routed to your tree (rows no drain could apply, second-order splash included) — apply refute-first with your closure ' +
    'authority; one you cannot land returns in residualRows with its blocker: ' +
    JSON.stringify(residualRows) +
    '.' +
    pyGuard(L.key) +
    ' DISPATCH LEGS where the sweep exceeds one context — legs read and report, every write is yours: opus legs through the Agent tool ' +
    '(subagent_type "general-purpose", model opus) for deep folder mapping and strata integration checks; codex legs through ' +
    'mcp__codex__codex (ToolSearch "select:mcp__codex__codex" first; model "gpt-5.6-sol", config={"model_reasoning_effort":"medium"}, ' +
    'sandbox "read-only", approval-policy "never", cwd "' +
    REPO +
    '") for .api ultra-stacking member-verification volume. ' +
    ROW_LAW +
    ' ' +
    PATH_LAW +
    ' Write your full red-team document to ' +
    OUT +
    '/redteam-' +
    L.key +
    '.md, then return: language "' +
    L.key +
    '", ok, report, filesTouched, defectsFixed, cardsReopened, residualRows, headline, failure.';

const testsMapWrapPrompt = (L) => {
    const report = OUT + '/tests-' + L.key + '-map.md';
    return (
        'DISPATCH ROLE: codex performs the complete TASK below through one blocking codex MCP call; never perform, edit, judge, or relay the ' +
        'work yourself. (1) ToolSearch "select:mcp__codex__codex". (2) Call mcp__codex__codex ONCE with model="gpt-5.6-sol", cwd="' +
        REPO +
        '", sandbox="read-only", approval-policy="never", config={"model_reasoning_effort":"medium"}, "developer-instructions" = the LANE ' +
        'LAW block below VERBATIM, prompt = the TASK block below VERBATIM. On a tool error retry ONCE with a sharpened prompt. (3) The tool ' +
        'result is a JSON envelope {threadId, content}; Write the CONTENT text (never the envelope) unmodified to ' +
        report +
        ' (delete any leftover file first). (4) Verify with one Bash call: grep -oE "^## \\[(FACTS|GAPS|DRIFT|SPINE|COVERAGE)\\]" ' +
        report +
        ' | sort -u | wc -l — a count under 5 means a missing contract section, a malformed product: rewrite once from the tool result, ' +
        'then return ok=false with the miss. ' +
        '(5) Return ok, report, entries = the "- " row count under [GAPS] ("- none" counts zero), research = 0, headline = "<gaps>g ' +
        '<drift>d <spine>s", failure empty — or ok=false with the error text VERBATIM.\n\nLANE LAW:\n\n' +
        TMAP_LAW +
        '\n\nTASK:\n\n' +
        'Map the ' +
        L.key +
        ' tests estate. Territory: ' +
        REPO +
        '/tests/README.md, the ' +
        L.key +
        ' tests tree under ' +
        REPO +
        '/tests (resolve the exact directory from the README), the root test spine (' +
        (L.key === 'csharp'
            ? 'Directory.Build.props, Directory.Build.targets, global.json test rows'
            : L.key === 'python'
              ? 'pyproject.toml test tier and pytest configuration'
              : 'vitest/vite/playwright/stryker configs and package-manager test script rows') +
        ', ' +
        REPO +
        '/.config where test-owned), the testing-relevant .api catalogs (packages the tests spine cites, at their owning tiers under ' +
        apiTiersOf({ path: REPO + '/' + L.root, language: L.key }) +
        '), and the ' +
        L.key +
        ' libs surface at registry depth (' +
        REPO +
        '/' +
        L.root +
        ' README registries and observability pages). BUILD-OUT AXES to grade [GAPS] against: ' +
        AXES[L.key] +
        '.'
    );
};

const testsImplPrompt = (L, mapR) =>
    'TESTS-ESTATE build-out for ' +
    L.key +
    ' — ASSUME libs/ is fully made: build what tests/ must be for a world-class, powerful, ready-to-author-tests estate. INFRASTRUCTURE ' +
    'ONLY — never author tests. Write territory: the ' +
    L.key +
    ' tests tree under ' +
    REPO +
    '/tests (resolve the exact directory from ' +
    REPO +
    '/tests/README.md) plus the ' +
    L.key +
    ' rows of the root test spine (' +
    (L.key === 'csharp'
        ? 'Directory.Build.props/targets test rows and tests-level project files'
        : L.key === 'python'
          ? 'pyproject.toml test tier and pytest configuration rows'
          : 'vitest/vite/playwright/stryker configs and package-manager test script rows') +
    '). Root-manifest test rows are alignment territory — reshape existing test configuration; a NEW package remains a residual row, never ' +
    'an install or manifest add. Shared root surfaces (tests/README.md, ' +
    REPO +
    '/.config, and the central manifests where your spine rows live) sequence LAST, surgical anchored Edits only, full re-read immediately ' +
    'before the first edit — sibling language lanes and the per-language seal lanes run beside you. ' +
    STANCE +
    ' Read AT SOURCE in full first: ' +
    REPO +
    '/tests/README.md, every root file of ' +
    REPO +
    '/docs/stacks/' +
    L.key +
    '/, the .api catalogs of the testing packages the spine cites (your own reads — never guessed from pool receipts), then the tests map ' +
    mapClause(mapR) +
    ' Build-out axes (a floor, hunted past): ' +
    AXES[L.key] +
    '; shared logic and capabilities consolidated into the _testkits; drift and stale content fixed against the post-implementation libs ' +
    'surface; the root spine aligned. ' +
    PATH_LAW +
    ' GATE: after the final edit run ' +
    GATES[L.key] +
    ' (exact invocation resolved from tests/README.md), batched ONCE, and repair what it reports to green before returning; gateClean is ' +
    'true only from a green final run. Run the docgen gate script at ' +
    REPO +
    '/.claude/skills/docgen/scripts/prose_gate.py (invocation per its --help) batched once over every markdown file you touched, repaired ' +
    'to zero FAIL.' +
    pyGuard(L.key) +
    ' Write your full receipt document (files, capabilities landed, drift fixed, spine rows, gate output ' +
    'tail) to ' +
    OUT +
    '/tests-' +
    L.key +
    '.md, then return: language "' +
    L.key +
    '", ok, report, filesTouched, gate (the exact command run), gateClean, residualRows (missing packages as "package@' +
    L.key +
    ': reason" plus genuinely blocked items), headline, failure.';

const auditPrompt = (roots, selected, residuals) =>
    'SLICE-CLOSURE AUDIT for theme "' +
    THEME.title +
    '" — count, fix nothing, edit nothing. SELECTED CARDS (this slice contract, exact "FILE [ID]" rows): ' +
    JSON.stringify(selected) +
    '. Verify each selected card CURRENT leader marker in its named card file: roster every one still [ACTIVE] or [QUEUED] as openCards ' +
    '{file (absolute), card}; roster every selected [BLOCKED] card whose body states no arming trigger or condition as blockedNoTrigger; ' +
    'count all remaining selected [BLOCKED] cards as blockedCards. Non-selected open cards are OUT of scope — expected leftovers for ' +
    'future slices, never a failure, never rostered. Then count live "- " rows under terminal [RESEARCH] sections across every design ' +
    'page under these roots: ' +
    JSON.stringify(roots) +
    ' — remaining rows are expected (future slices own non-pertinent debt); report total and per language as "language: N". RIPPLE PROOF: ' +
    'sweep the rippleLedger arrays across the stage receipt documents under ' +
    OUT +
    ' — a "deferred" endpoint no drain or red-team document shows applied and current disk does not satisfy joins openCards as {file: the ' +
    'endpoint, card: "ripple: " + its source}. UNCONSUMED DRAIN RESIDUALS (no language tree consumed them): ' +
    JSON.stringify(residuals) +
    ' — verify each against current disk; one still unapplied joins openCards as {file: its targetFile, card: "residual: " + its change}. ' +
    'Counts only — no edits, no verdicts beyond the rosters.';

// --- [COMPOSITION] -------------------------------------------------------------------

if (!CAMP) return { skipped: true, reason: 'args.camp (absolute campaign home) is required' };

const disco = await guard(slot(() => agent(discoverPrompt(), { label: 'discover', phase: 'Plan', model: 'opus', effort: 'low', schema: DISCOVERY })));
let folders = (disco?.folders ?? []).filter((f) => f.path && f.language);
if (SCOPE.length)
    folders = folders.filter((f) => SCOPE.some((s) => f.path === s || f.path.endsWith('/' + s.replace(/\/+$/, '')) || f.path.includes(s)));
if (!folders.length) return { skipped: true, reason: 'no card-owning folders resolved', scope: SCOPE };
log(folders.length + ' folder(s), ' + sum(folders, (f) => f.openCards) + ' open card(s)');

// --- [FOLDER_CHAINS]

const manifestQueue = {};
const admitSeen = {}; // one admission chain per package@language — later requesters share the first receipt
const admit = (row, origin) => {
    const seenKey = row.language + ':' + slugOf(row.package);
    if (admitSeen[seenKey]) return admitSeen[seenKey];
    const run = () =>
        guard(
            slot(() =>
                agent(admitPrompt(row, origin), {
                    label: 'admit:' + slugOf(row.package),
                    phase: 'Admit',
                    model: 'opus',
                    effort: 'high',
                    schema: ADMITR,
                    stallMs: STALL_MS,
                }),
            ),
        );
    const p = (manifestQueue[row.language] || Promise.resolve()).then(run);
    manifestQueue[row.language] = p.then(
        () => undefined,
        () => undefined,
    );
    admitSeen[seenKey] = p;
    return p;
};
const runAdmissions = async (rec, origin) => {
    const rows = ((rec && rec.ok && rec.packagesMissing) || []).filter((r) => r && r.package && LANGS.some((L) => L.key === r.language));
    return (await Promise.all(rows.map((r) => admit(r, origin)))).filter(Boolean);
};

const runImplement = (f, u, mapR, guideR) =>
    u.tier === 'codex'
        ? guard(
              slot(() =>
                  agent(codexImplWrapPrompt(f, u, mapR, guideR), {
                      label: 'sol:impl:' + u.id,
                      phase: 'Implement',
                      model: 'sonnet',
                      effort: 'low',
                      schema: STAGE,
                      stallMs: STALL_MS,
                  }),
              ),
          )
        : guard(
              slot(() =>
                  agent(implPrompt(f, u, mapR, guideR), {
                      label: 'impl:' + u.id,
                      phase: 'Implement',
                      model: u.tier,
                      effort: 'high',
                      schema: STAGE,
                      stallMs: STALL_MS,
                  }),
              ),
          );

const runFolderChain = async (f, plan) => {
    const units = (plan?.units ?? [])
        .slice()
        .sort((a, b) => a.order - b.order)
        .map((u, i) => ({ ...u, id: uid(f, i) }));
    if (!units.length) {
        log((plan ? 'no-op' : 'PLAN FAILED') + ': ' + f.path);
        return { folder: f.path, language: f.language, planOk: !!plan, units: [], leftovers: plan?.leftovers ?? [] };
    }
    const out = [];
    for (const u of units) {
        const [mapR, guideR] = await Promise.all([
            guard(
                slot(() =>
                    agent(mapWrapPrompt(f, u), {
                        label: 'sol:map:' + u.id,
                        phase: 'Map',
                        model: 'sonnet',
                        effort: 'low',
                        schema: RECEIPT,
                        stallMs: STALL_MS,
                    }),
                ),
            ),
            u.blockers.length
                ? guard(
                      slot(() =>
                          agent(guideWrapPrompt(f, u), {
                              label: 'sol:guide:' + u.id,
                              phase: 'Map',
                              model: 'sonnet',
                              effort: 'low',
                              schema: RECEIPT,
                              stallMs: STALL_MS,
                          }),
                      ),
                  )
                : Promise.resolve(null),
        ]);
        const impl = await runImplement(f, u, mapR, guideR);
        const admittedImpl = await runAdmissions(impl, f.path);
        const crit =
            impl && impl.ok
                ? await guard(
                      slot(() =>
                          agent(critPrompt(f, u, impl, mapR, admittedImpl), {
                              label: 'crit:' + u.id,
                              phase: 'Critique',
                              model: u.heavy ? 'fable' : 'opus',
                              effort: 'high',
                              schema: STAGE,
                              stallMs: STALL_MS,
                          }),
                      ),
                  )
                : null;
        const admittedCrit = await runAdmissions(crit, f.path);
        const landedCrit = admittedCrit.filter((a) => a && a.ok && a.admitted);
        const comp = landedCrit.length
            ? await guard(
                  slot(() =>
                      agent(composePrompt(f, u, landedCrit), {
                          label: 'compose:' + u.id,
                          phase: 'Compose',
                          model: 'opus',
                          effort: 'high',
                          schema: STAGE,
                          stallMs: STALL_MS,
                      }),
                  ),
              )
            : null;
        const admittedComp = await runAdmissions(comp, f.path);
        out.push({
            unit: u.id,
            cards: u.cards,
            blockers: u.blockers.length,
            mapR,
            guideR,
            impl,
            crit,
            comp,
            admissions: admittedImpl.concat(admittedCrit, admittedComp),
        });
    }
    return { folder: f.path, language: f.language, planOk: true, units: out, leftovers: plan?.leftovers ?? [] };
};

const planLane = (f) =>
    guard(
        slot(() =>
            agent(planPrompt(f), {
                label: 'plan:' + fid(f),
                phase: 'Plan',
                model: 'opus',
                effort: f.language === 'cross' || f.openCards >= PLAN_HIGH_FLOOR ? 'high' : f.openCards >= PLAN_MEDIUM_FLOOR ? 'medium' : 'low',
                schema: UNITS,
            }),
        ),
    );

const folderRecs = folders.filter((f) => f.language !== 'cross');
const crossRec = folders.find((f) => f.language === 'cross') || null;
const crossPlanP = crossRec ? planLane(crossRec) : Promise.resolve(null);
const chains = (await pipeline(folderRecs, planLane, (plan, f) => runFolderChain(f, plan))).filter(Boolean);
const crossChain = crossRec ? await runFolderChain(crossRec, await crossPlanP) : null; // cross-libs units run after every folder chain — their touchpoints span the trees the folder writers just closed
const allChains = crossChain ? chains.concat([crossChain]) : chains;

const unitRecs = allChains.flatMap((c) => c.units.map((x) => ({ ...x, folder: c.folder, language: c.language })));
const receiptPaths = unitRecs.flatMap((x) =>
    [x.impl && x.impl.ok && x.impl.report, x.crit && x.crit.ok && x.crit.report, x.comp && x.comp.ok && x.comp.report].filter(Boolean),
);
const admissions = unitRecs.flatMap((x) => x.admissions || []);
log(unitRecs.length + ' unit(s), ' + receiptPaths.length + ' receipt(s), ' + admissions.length + ' admission(s)');

// --- [SEAL_AND_TESTS]

let drainUnrouted = [];
const sealP = receiptPaths.length
    ? (async () => {
          // Barrier is genuine: drain residualRows route ACROSS languages, so every drain must land before any redteam starts.
          const drainRs = await parallel(
              LANGS.map(
                  (L) => () =>
                      guard(
                          slot(() =>
                              agent(drainPrompt(L, receiptPaths), {
                                  label: 'drain:' + L.key,
                                  phase: 'Drain',
                                  model: 'opus',
                                  effort: 'high',
                                  schema: DRAINR,
                                  stallMs: STALL_MS,
                              }),
                          ),
                      ),
              ),
          );
          const allResiduals = drainRs.flatMap((dr) => (dr && dr.residualRows) || []);
          const residualFor = (L) => allResiduals.filter((r) => String(r).includes('/' + L.root + '/'));
          const routed = new Set(LANGS.flatMap((L) => residualFor(L)));
          drainUnrouted = allResiduals.filter((r) => !routed.has(r));
          if (drainUnrouted.length) log(drainUnrouted.length + ' drain residual(s) target no language tree — carried to the audit');
          return await parallel(
              LANGS.map((L, i) => () => {
                  const dr = drainRs[i];
                  const res = residualFor(L);
                  const hadUnits = allChains.some((c) => (c.language === L.key || c.language === 'cross') && c.units.length);
                  if (!hadUnits && !(dr && dr.applied > 0) && !res.length) return Promise.resolve({ language: L.key, drain: dr, redteam: null });
                  const admitRows = admissions
                      .filter((a) => a && a.language === L.key)
                      .map((a) => ({ package: a.package, admitted: !!(a.ok && a.admitted), catalog: a.report || '' }));
                  return guard(
                      slot(() =>
                          agent(rtPrompt(L, dr, receiptPaths, admitRows, res), {
                              label: 'rt:' + L.key,
                              phase: 'Redteam',
                              model: 'fable',
                              effort: 'max',
                              schema: REDTEAMR,
                              stallMs: STALL_MS,
                          }),
                      ),
                  ).then((rt) => ({ language: L.key, drain: dr, redteam: rt }));
              }),
          );
      })()
    : Promise.resolve([]);

const testsP = TESTS_ON
    ? pipeline(
          LANGS,
          (L) =>
              guard(
                  slot(() =>
                      agent(testsMapWrapPrompt(L), {
                          label: 'sol:tmap:' + L.key,
                          phase: 'Tests',
                          model: 'sonnet',
                          effort: 'low',
                          schema: RECEIPT,
                          stallMs: STALL_MS,
                      }),
                  ),
              ),
          (tm, L) =>
              guard(
                  slot(() =>
                      agent(testsImplPrompt(L, tm), {
                          label: 'tests:' + L.key,
                          phase: 'Tests',
                          model: 'fable',
                          effort: 'high',
                          schema: TESTSR,
                          stallMs: STALL_MS,
                      }),
                  ),
              ),
      )
    : Promise.resolve([]);

const [sealedRaw, testsRaw] = await Promise.all([sealP, testsP]);
const sealed = sealedRaw.filter(Boolean);
const tests = testsRaw.filter(Boolean);

// --- [AUDIT]

const selectedCards = unitRecs.flatMap((x) => x.cards || []);
const audit = await guard(
    slot(() =>
        agent(
            auditPrompt(
                folders.map((f) => f.path),
                selectedCards,
                drainUnrouted,
            ),
            {
                label: 'audit',
                phase: 'Audit',
                model: 'opus',
                effort: 'low',
                schema: AUDITR,
            },
        ),
    ),
);
const unclosed = audit ? audit.openCards.concat(audit.blockedNoTrigger) : [];
const plansFailed = allChains.filter((c) => !c.planOk).map((c) => c.folder);
const sliceClosed = !!audit && unclosed.length === 0 && !plansFailed.length;
if (!sliceClosed)
    log(
        'SLICE NOT CLOSED: ' +
            (audit
                ? unclosed.length +
                  ' selected card(s) without a terminal disposition' +
                  (plansFailed.length ? '; ' + plansFailed.length + ' plan lane(s) failed' : '')
                : 'audit lane failed'),
    );

// --- [CLOSE]

return {
    theme: THEME.title,
    folders: allChains.length,
    plansFailed,
    units: unitRecs.length,
    selectedCards: selectedCards.length,
    implemented: unitRecs.filter((x) => x.impl && x.impl.ok).length,
    critiqued: unitRecs.filter((x) => x.crit && x.crit.ok).length,
    composed: unitRecs.filter((x) => x.comp && x.comp.ok).length,
    failedUnits: unitRecs.filter((x) => !(x.impl && x.impl.ok)).map((x) => x.unit),
    cardsClosed: sum(
        unitRecs,
        (x) => ((x.impl && x.impl.cardsClosed) || 0) + ((x.crit && x.crit.cardsClosed) || 0) + ((x.comp && x.comp.cardsClosed) || 0),
    ),
    crossFolderRows: sum(
        unitRecs,
        (x) => ((x.impl && x.impl.crossFolderRows) || 0) + ((x.crit && x.crit.crossFolderRows) || 0) + ((x.comp && x.comp.crossFolderRows) || 0),
    ),
    researchBefore: sum(unitRecs, (x) => (x.mapR && x.mapR.research) || 0),
    researchResolved: sum(
        unitRecs,
        (x) => ((x.impl && x.impl.researchResolved) || 0) + ((x.crit && x.crit.researchResolved) || 0) + ((x.comp && x.comp.researchResolved) || 0),
    ),
    researchAfter: audit ? audit.researchRows : -1,
    researchByLanguage: audit ? audit.researchByLanguage : [],
    admissions: admissions.map((a) => ({ package: a.package, language: a.language, admitted: !!(a.ok && a.admitted), report: a.report || '' })),
    drains: sealed.map((s) => ({
        language: s.language,
        ok: !!(s.drain && s.drain.ok),
        applied: (s.drain && s.drain.applied) || 0,
        report: (s.drain && s.drain.report) || '',
    })),
    redteams: sealed
        .filter((s) => s.redteam)
        .map((s) => ({
            language: s.language,
            ok: !!s.redteam.ok,
            defectsFixed: s.redteam.defectsFixed || 0,
            cardsReopened: s.redteam.cardsReopened || 0,
            report: s.redteam.report || '',
        })),
    tests: tests.map((t) => ({ language: t.language, ok: !!t.ok, gate: t.gate || '', gateClean: !!t.gateClean, report: t.report || '' })),
    residuals: sealed
        .flatMap((s) => [...((s.drain && s.drain.residualRows) || []), ...((s.redteam && s.redteam.residualRows) || [])])
        .concat(tests.flatMap((t) => t.residualRows || []))
        .concat(
            admissions
                .filter((a) => !(a.ok && a.admitted))
                .map((a) => 'admission failed or rejected: ' + a.package + '@' + a.language + ' :: ' + (a.failure || a.headline || '')),
        ),
    sliceClosed,
    unclosed,
    futureSlices: allChains.flatMap((c) => (c.leftovers || []).map((l) => ({ folder: c.folder, item: l.item, hint: l.hint }))),
    blockedCards: audit ? audit.blockedCards : -1,
    reports: receiptPaths
        .concat(unitRecs.flatMap((x) => [x.mapR && x.mapR.ok && x.mapR.report, x.guideR && x.guideR.ok && x.guideR.report].filter(Boolean)))
        .concat(admissions.map((a) => a.report).filter(Boolean))
        .concat(sealed.flatMap((s) => [s.drain && s.drain.report, s.redteam && s.redteam.report].filter(Boolean)))
        .concat(tests.map((t) => t.report).filter(Boolean)),
    failure: sliceClosed
        ? ''
        : 'SLICE NOT CLOSED: ' +
          (audit
              ? unclosed.length +
                ' selected card(s) left without a terminal disposition' +
                (plansFailed.length ? '; ' + plansFailed.length + ' plan lane(s) failed' : '')
              : 'audit lane failed'),
};
