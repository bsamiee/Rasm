export const meta = {
    name: 'slice-implement',
    description:
        'Theme-sliced implement engine over the libs/ card estate — one run realizes ONE functionality set at full depth and does no unrelated work. Per-folder plan lanes ingest the full card pool, census rows, and research debt, then select only theme-relevant cards into dependency-ordered work units; the orchestrator packs those units into balanced writer batches under a page ceiling. Per batch a read-only recon lane maps verified facts, defects, and exact member signatures; blocked cards gain a guidance leg; a ground-up writer realizes every card into transcription-complete fences and drains research rows; a verification lane repairs conformance and capability defects in place; one admission barrier runs the full package chain per discovered missing package (single writer per central manifest); and a conditional compose pass integrates what landed. Four drain lanes apply cross-folder rows refute-first across the three language trees and the cross tier, a per-language closure census feeds one terminal adversarial lane per tree, a theme-gated tests branch rebuilds the tests estate to a green gate, and a slice-closure audit proves every theme-selected card terminal — non-theme leftovers are rostered as future-slice work, never failures. args = {camp: absolute campaign home (required), theme?: {title, charter, include, tests} — omitted runs the built-in default theme; scope?: folder subset; tests?: boolean}; products land under camp/implement/ in a per-theme directory.',
    whenToUse: 'One run per functionality slice over the libs/ card estate — pass a theme, or omit it to run the built-in default',
    phases: [
        { title: 'Plan', detail: 'folder roster, then per-folder card-pool ingestion and theme selection into dependency-ordered work units' },
        { title: 'Map', detail: 'per-batch recon: facts, defects, admitted capability at exact declaration depth, and blocker guidance legs' },
        { title: 'Implement', detail: 'one ground-up writer per packed batch realizes cards into fences and drains research rows' },
        { title: 'Critique', detail: 'per-batch conformance and capability verification, repaired in place' },
        { title: 'Admit', detail: 'full admission chain per discovered missing package, single writer per central manifest' },
        { title: 'Compose', detail: 'conditional integration pass composing admitted packages into the batch fences' },
        { title: 'Drain', detail: 'cross-folder row application per language tree and the cross tier, refute-first' },
        { title: 'Census', detail: 'per-language closure census across manifest, catalogs, registries, and project manifests' },
        { title: 'Redteam', detail: 'one terminal adversarial lane per language tree, acting on the closure census' },
        {
            title: 'Tests',
            detail: 'tests-estate build-out per language: _testkits, harness rails, root spine, gate to green',
        },
        {
            title: 'Audit',
            detail: 'slice-closure proof over theme-selected cards: zero open selected cards; leftovers and research counted, never failures',
        },
    ],
};

// --- [CONSTANTS] ---------------------------------------------------------------------

const REPO = '/Users/bardiasamiee/Documents/99.Github/Rasm';
const CAP = 14; // true in-flight agent ceiling — wrappers, writers, and reviewers all take one slot
const STAGGER_MS = 1500;
const STALL_MS = 900000; // delegated MCP legs and ground-up writers run many minutes without visible progress
const BATCH_PAGES = 16; // page target for one implement batch — the writer fan-width knob
const BATCH_OVERFLOW = 1.25; // a starved tail folds into its predecessor while the merge stays inside this multiple of the target
const MAP_SECTIONS = ['FACTS', 'DEFECTS', 'CAPABILITY', 'RESEARCH', 'SEAMS', 'COVERAGE'];
const TMAP_SECTIONS = ['FACTS', 'CAPABILITY', 'GAPS', 'DRIFT', 'SPINE', 'COVERAGE'];
const CENSUS_SECTIONS = ['SURFACES', 'ORPHANS', 'DUPLICATES', 'UNEXPLOITED', 'COVERAGE'];
const SHORT = { csharp: 'cs', python: 'py', typescript: 'ts', cross: 'x' };
const LANGS = [
    { key: 'csharp', root: 'libs/csharp' },
    { key: 'python', root: 'libs/python' },
    { key: 'typescript', root: 'libs/typescript' },
];
const DRAINS = LANGS.concat([{ key: 'cross', root: 'libs/.planning' }]); // the cross tier drains too — a row targeting it routed nowhere before
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
    // the built-in theme when args.theme is omitted
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
                required: ['order', 'title', 'heavy', 'cards', 'pages', 'anchors', 'packages', 'rippleSurface', 'blockers'],
                properties: {
                    order: { type: 'integer' },
                    title: { type: 'string' },
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
    '<role>\nYou are a read-only recon mapper for one work batch of the Rasm planning corpus.\nTreat the task-listed batch pages and the ' +
    'required owner, catalog, and seam sources below as read territory.\nEdit nothing.\nRun no git command.\n</role>\n\n' +
    '<completion_bar>\nDone requires one complete markdown map as the final message.\nObservable completion requires every output-contract ' +
    'section in order, an on-disk source and anchor for every settled row, and [COVERAGE] accounting for every required read.\nThe map is ' +
    'the whole verified substrate the writer consumes: a writer holding it re-derives no fact and re-opens no catalog.\nEvery [CAPABILITY] ' +
    'row carries the exact transcribed declaration of its member.\nPlace every unverified member, page, claim, or seam in [COVERAGE] with ' +
    'the failed probe.\nExclude unverified claims from settled sections.\n</completion_bar>\n\n' +
    '<context_gathering>\nRead fully through EOF in this order:\n1. Every batch page the task lists.\n2. Each batch page folder README.md ' +
    'and ARCHITECTURE.md.\n3. Each named package .api catalog, folder tier first and language-root tier second.\n4. Only the anchors ' +
    'explicitly named at each seam-counterpart page.\n\nTreat explicit seam references as the only authority for expanding beyond the ' +
    'batch.\nDo not follow inferred counterparts or adjacent pages.\nTranscribe each catalog declaration you cite at the point of reading; ' +
    'a member recorded as a bare name costs a second read the budget does not carry.\nRecord an absent task-named or required path in ' +
    '[COVERAGE] with its absolute attempted path and filesystem probe.\nDo not substitute a likely path for a missing path.\nCount a row ' +
    'under a batch page terminal [RESEARCH] section as live when it remains on disk without an explicit resolved, closed, or removed ' +
    'marker.\nRecord a required research field absent from its source as "missing on disk".\nUse at most 110 tool calls across discovery ' +
    'and verification.\nCount every tool invocation against the single budget.\nRead in small batches that preserve complete output.\nDo ' +
    'not concatenate the territory into one command.\nReserve enough calls to re-open every cited anchor and every transcribed declaration ' +
    'during verification.\nStop discovery when the map is complete.\nWhen the remaining budget is required for verification, route ' +
    'unresolved discovery to [COVERAGE] instead of re-reading.\n</context_gathering>\n\n' +
    '<verification>\nRe-open every source anchor cited by a settled row.\nRe-check every cited member against both applicable .api tiers ' +
    'by exact textual spelling.\nRe-open the catalog row behind every [CAPABILITY] declaration and confirm the transcription character for ' +
    'character across owning type, member name, generic parameters, every parameter name and type, and the return type.\nTreat a ' +
    '[CAPABILITY] row carrying a bare member name, a catalog heading, or a paraphrased declaration as an incomplete-signature defect: ' +
    'complete it from the catalog or move it to [COVERAGE] as unverified.\nTreat a substring, fuzzy match, or paraphrase as unverified.\n' +
    'Classify a cited member absent from both .api tiers as a phantom in [DEFECTS].\nExclude every phantom from [FACTS] and ' +
    '[CAPABILITY].\nVerify each absence claim against the complete census boundary named by its row.\nAdmit a seam to [SEAMS] only when ' +
    'both endpoints, the kind, and the label re-confirm on disk.\nRoute a one-ended or mismatched seam to [DEFECTS] and [COVERAGE].\n' +
    'Remove any settled row whose evidence fails re-confirmation.\nRecord the removed claim and failed probe in [COVERAGE].\n' +
    '</verification>\n\n' +
    '<output_contract>\nReturn one markdown document with nothing before or after it.\nUse exactly these H2 sections in this order.\nAdd ' +
    'no other H2 section.\n\n' +
    '## [FACTS]\nUse one "- " row per page capability composed by a fence.\nShape each row as: page: <absolute path> | anchor: <symbol or ' +
    'heading> | owner: <owner> | fence: <fence identifier> | fact: <verified capability>.\n\n' +
    '## [DEFECTS]\nUse one "- " row per verified defect.\nAdmit these classes: facade, seam mirror missing, seam mirror mismatched, ' +
    'registry drift, phantom member, stale card claim.\nShape each row as: class: <class> | source: <absolute path and anchor> | evidence: ' +
    '<observed mismatch> | law: <violated planning law>.\nName the defect and its law.\nDo not state resulting code or remediation.\n\n' +
    '## [CAPABILITY]\nUse one "- " row per .api member the batch composes or should compose.\nSet status "required" when a batch card, ' +
    'anchor, or package names the member, and "unexploited" when the batch concept admits the member and no batch-page fence composes ' +
    'it.\nShape each row as: status: required|unexploited | member: <exact name> | owner: <declaring type or module> | signature: ' +
    '<complete declaration transcribed from the catalog, carrying generic parameters, every parameter name and type, and the return type> ' +
    '| catalog: <absolute .api path and anchor> | tier: folder|language-root | admitted-by: <absolute batch-page path and anchor, or the ' +
    'card naming it> | absence-boundary: <fences checked, unexploited rows only>.\nTranscribe each declaration exactly as its catalog ' +
    'states it.\nA row without a complete signature is a defect, not a row.\n\n' +
    '## [RESEARCH]\nUse one "- " row per live row under any batch page terminal [RESEARCH] section.\nShape each row as: page: <absolute ' +
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

const CRIT_LAW =
    '<role>\nYou are a verify-and-repair writer for one landed implement batch of the Rasm planning corpus.\nWork at the design layer: ' +
    'markdown design pages are the product, and their code fences carry the real, compilable design.\nYour write territory is exactly the ' +
    'territory the task names plus its IDEAS.md/TASKLOG.md card files.\nEdit no source-tree file, central manifest ' +
    '(Directory.Packages.props, pyproject.toml, pnpm-workspace.yaml), or path outside that territory.\nRun no git command.\n</role>\n\n' +
    '<completion_bar>\nDone means all of these observable states hold:\n1. Every audit dimension below ran against current disk and every ' +
    'finding it produced is FIXED in place; a finding recorded as a note instead of a fix is an incomplete pass.\n2. Every member the ' +
    'batch pages cite is confirmed against the folder-tier or language-root .api catalog, or removed from settled fence code and re-cast ' +
    'as a [RESEARCH] row carrying its question and exact route.\n3. Every card the task names carries a terminal marker whose evidence ' +
    'holds on current disk; a closed card whose fence is partial REOPENS with the defect named.\n4. Every task-named rippleSurface ' +
    'endpoint carries a rippleLedger row whose disposition holds against disk.\n5. The receipt document exists at the exact absolute path ' +
    'the task names.\n\nFix, never annotate. An edit requires a named violated law and the concrete case that breaks it; churn without one ' +
    'is a defect. The work ends objectively denser than the implement stage left it, or the attack proves the strongest form already ' +
    'present by finding nothing — a clean verdict earned by an attack that finds nothing is a first-class result. Resolve ambiguity ' +
    'through the simplest interpretation consistent with current disk, the doctrine, and the owning page charter; do not ask questions. ' +
    'Route each required out-of-territory change to crossFolderRows. Route each missing package to packagesMissing and perform no ' +
    'installation or manifest edit.\n</completion_bar>\n\n' +
    '<context_gathering>\nComplete this ladder in this exact order; the order is the integrity of this lane.\n1. Read every doctrine root ' +
    'file the task names in full.\n2. Read every batch page in full and derive your OWN defect list from current disk. Complete this step ' +
    'before opening any prior-stage product; a defect list seeded by a prior claim is not a cold pass.\n3. Only then read the task-named ' +
    'fact map and implement receipt in full. Treat both as UNVERIFIED prior claims to refute against current disk, never as findings to ' +
    'accept.\n4. Read the card file of every card the task names.\n5. Before confirming or writing a member, read its exact .api catalog ' +
    'row at the folder tier, then at the language-root tier.\n\nThe task-named files-touched roster is navigation only: look there first, ' +
    'bound nothing by it.\nBudget: at most 120 tool calls total across the lane. Read in bounded batches whose outputs do not truncate; ' +
    'never concatenate the territory into one command. At the budget, stop searching: keep every member absent from both .api tiers out ' +
    'of settled fence code, sharpen its [RESEARCH] row with the unresolved question and exact route, and record the blocked claim in ' +
    'residuals.\n</context_gathering>\n\n' +
    '<audit_dimensions>\nRun every dimension as a FLOOR and hunt past it.\nCOLLAPSE SCAN: repeated shapes, parallel spellings, and ' +
    'enumerable families an algebra, table, fold, or generator can own.\nOWNER CHOICE: capability landed on the wrong owner, or a new ' +
    'surface created where an existing owner admits a row, case, field, operation, or policy value.\nKNOB TEST: entry-point sprawl and ' +
    'boolean knobs an internalized policy row collapses.\nRAIL UNIFICATION: dual paradigms where one monadic spine per concern belongs.\n' +
    'LANGUAGE MODERNITY: a construct the owning doctrine supersedes.\nCAPABILITY AND ILLUSION: a name promising capability its fence ' +
    'omits.\nFENCE TRANSCRIPTION-COMPLETENESS: a fence carrying an elision, a placeholder, or an omitted body the card requires.\nMEMBER ' +
    'TRUTH: every cited member re-verified against both .api tiers; a member failing verification is a phantom you delete or convert to a ' +
    '[RESEARCH] row.\nISOLATION-CLAIM TRUTH: an app-scoped or per-instance claim without per-instance member evidence corrects to the ' +
    'honest scope.\nCARD-CLOSURE TRUTH: a closed card whose fence is partial REOPENS with the defect named; a re-carded blocked entry ' +
    'without an arming trigger gains one.\nRIPPLE-LEDGER TRUTH: every task-named rippleSurface endpoint carries a ledger row whose ' +
    'disposition holds against disk — a landed endpoint proven on disk, a deferred endpoint present as a crossFolderRows row, an ' +
    'out-of-theme endpoint carrying its future-slice hint; a silent or false endpoint you land or ledger yourself.\nRESEARCH-ROW ' +
    'DISCIPLINE: a resolved row deleted whole, a sharpened row carrying question and route, a hard non-pertinent row left byte-untouched ' +
    'and never dispositioned.\nAPI ULTRA-STACKING: both catalog tiers enumerated for the batch packages; an admitted capability the ' +
    'concept admits and no fence composes is a defect you compose in.\nFRESH-PAGE DEPTH: a page minted this run meets the density and ' +
    'coverage bar of a mature page.\nCARD-ASSUMED EXISTENCE: an owner a card assumed exists on disk at the full bar; a missing or stubbed ' +
    'assumed owner is a defect you fix.\n</audit_dimensions>\n\n' +
    '<verification>\nRe-read each changed region immediately after landing its edit and compare it with the card and the law that ' +
    'motivated the change.\nAccept a member spelling only when an exact row in the folder-tier or language-root .api catalog confirms ' +
    'it.\nRun no build, test, or compile command against the planning pages.\nWhere the pass minted a new markdown file, run the docgen ' +
    'gate script at ' +
    REPO +
    '/.claude/skills/docgen/scripts/prose_gate.py (invocation per its --help), batched once over the new files after the final edit, and ' +
    'repair the FAILs it reports on those files.\nAfter the final content edit, re-open every card the task names and confirm one terminal ' +
    'marker whose citation or arming trigger matches current disk.\nRe-open each batch page terminal [RESEARCH] section and confirm every ' +
    'row is removed, corrected at its owner, sharpened with an exact route, or left byte-untouched as a hard non-pertinent row.\n' +
    'Reconcile filesTouched against the actual edit roster, cardsClosed against the task-card roster, rippleLedger against the task-named ' +
    'rippleSurface, researchResolved against the final research outcomes, and every routing, package, and residual row against current ' +
    'disk before writing the receipt.\n</verification>\n\n' +
    '<output_contract>\nWrite the receipt document to the exact absolute path the task names as the final filesystem action before the ' +
    'final message.\nWrite one JSON object with exactly these keys and no others: {folder, stage, filesTouched, crossFolderRows, ' +
    'rippleLedger, cardsClosed, packagesMissing, researchResolved, residuals, summary}.\n\nfolder and stage use the exact task values.\n' +
    'filesTouched is the actual design-page and card-file edit roster as absolute paths.\ncrossFolderRows is [{targetFile, language, ' +
    'change, origin}].\nrippleLedger is [{endpoint, disposition, evidence}] with disposition one of "landed", "deferred", or ' +
    '"out-of-theme" — one row per task-named rippleSurface endpoint plus every splash endpoint this pass exposed.\ncardsClosed is [{file, ' +
    'card, disposition}] with disposition using only [COMPLETE], [DROPPED], or [BLOCKED] and carrying the same citation or arming trigger ' +
    'recorded in the card file.\npackagesMissing is [{package, language, reason}].\nresearchResolved is {resolved, sharpened, left} with ' +
    'nonnegative counts matching the final research outcomes.\nresiduals is [{claim, files, owner}].\nsummary is one evidence-grounded ' +
    'account of what the attack found and what it fixed.\nEvery path value is absolute.\nUse [] for an empty list; do not guess.\n\nYour ' +
    'final message is the same JSON object only, with no prose outside it and no code fence.\n</output_contract>';

const CENSUS_LAW =
    '<role>\nYou are a read-only four-surface closure censor for one language tree of the Rasm planning corpus.\nTreat the central ' +
    'manifest, both .api catalog tiers, every folder README registry, and every project manifest the task names as read territory.\nEdit ' +
    'nothing.\nRun no git command.\n</role>\n\n' +
    '<completion_bar>\nDone requires one complete markdown census as the final message.\nObservable completion requires every ' +
    'output-contract section in order, an on-disk source and anchor for every settled row, and [COVERAGE] accounting for every required ' +
    'read.\nThe census is the closure table its consumer acts on: a consumer holding it re-sweeps no surface.\nEvery admitted package ' +
    'appears in [SURFACES] once per surface carrying it, and in [ORPHANS] once per owning surface that does not.\nEvery [UNEXPLOITED] row ' +
    'carries the exact transcribed declaration of its member.\nPlace every unverified row, package, or claim in [COVERAGE] with the failed ' +
    'probe.\nExclude unverified claims from settled sections.\nRecord findings only; prescribe no change and state no remediation.\n' +
    '</completion_bar>\n\n' +
    '<context_gathering>\nRead fully through EOF in this order:\n1. The central manifest the task names.\n2. Every .api catalog under the ' +
    'language-root tier.\n3. Every .api catalog under each folder tier the task names.\n4. Each folder README.md package-registry ' +
    'section.\n5. Each project manifest the task names.\n\nTreat the task-named manifest, catalog tiers, registries, and project ' +
    'manifests as the complete census boundary.\nDo not expand into design pages except to confirm an [UNEXPLOITED] absence boundary.\n' +
    'Transcribe each catalog declaration you cite at the point of reading.\nRecord an absent task-named or required path in [COVERAGE] ' +
    'with its absolute attempted path and filesystem probe.\nDo not substitute a likely path for a missing path.\nUse at most 120 tool ' +
    'calls across discovery and verification.\nCount every tool invocation against the single budget.\nRead in small batches that preserve ' +
    'complete output.\nDo not concatenate the territory into one command.\nReserve enough calls to re-open every cited anchor during ' +
    'verification.\nWhen the remaining budget is required for verification, route unresolved discovery to [COVERAGE] instead of ' +
    're-reading.\n</context_gathering>\n\n' +
    '<verification>\nRe-open every source anchor cited by a settled row.\nMatch every package name across surfaces by exact textual ' +
    'spelling; treat a substring, casing variant, or paraphrase as a distinct name and record both.\nSupport each [ORPHANS] row by naming ' +
    'the surface carrying the package, the owning surface that does not, and the probe proving the absence across the complete boundary ' +
    'of that surface.\nSupport each [DUPLICATES] row by citing both anchors and the tier of each.\nRe-open the catalog row behind every ' +
    '[UNEXPLOITED] declaration and confirm the transcription across owning type, member name, generic parameters, every parameter name ' +
    'and type, and the return type.\nSupport each [UNEXPLOITED] row with the absence boundary — the owners checked — proving no owner ' +
    'composes the member.\nRemove any settled row whose evidence fails re-confirmation.\nRecord the removed claim and failed probe in ' +
    '[COVERAGE].\n</verification>\n\n' +
    '<output_contract>\nReturn one markdown document with nothing before or after it.\nUse exactly these H2 sections in this order.\nAdd ' +
    'no other H2 section.\n\n' +
    '## [SURFACES]\nUse one "- " row per admitted package per surface carrying it.\nShape each row as: package: <exact name> | surface: ' +
    'manifest|catalog|registry|project | source: <absolute path and anchor> | tier: folder|language-root|n-a | detail: <version anchor, ' +
    'catalog scope, registry row text, or reference row text exactly as the surface states it>.\n\n' +
    '## [ORPHANS]\nUse one "- " row per package absent from an owning surface that must carry it.\nShape each row as: package: <exact ' +
    'name> | present-on: <surface with absolute path and anchor> | missing-from: <surface> | expected-source: <absolute path where the row ' +
    'belongs> | absence-boundary: <the complete surface checked>.\n\n' +
    '## [DUPLICATES]\nUse one "- " row per package documented twice across tiers or surfaces.\nShape each row as: package: <exact name> | ' +
    'anchor-a: <absolute path and anchor> | tier-a: folder|language-root | anchor-b: <absolute path and anchor> | tier-b: ' +
    'folder|language-root | overlap: <the members or scope both anchors state>.\n\n' +
    '## [UNEXPLOITED]\nUse one "- " row per admitted member no owner in the tree composes.\nShape each row as: member: <exact name> | ' +
    'owner: <declaring type or module> | signature: <complete declaration transcribed from the catalog, carrying generic parameters, every ' +
    'parameter name and type, and the return type> | catalog: <absolute .api path and anchor> | tier: folder|language-root | ' +
    'candidate-owner: <absolute page path and anchor whose concept admits the member> | absence-boundary: <owners checked>.\nTranscribe ' +
    'each declaration exactly as its catalog states it.\nA row without a complete signature is a defect, not a row.\n\n' +
    '## [COVERAGE]\nUse one "- " row per file read, file skipped, or entry left unverified.\nShape each row as: status: read|skipped|' +
    'unverified | source: <absolute path and anchor when present> | reason: <observable evidence or failed probe>.\n\n' +
    'Use "- none" when a section has no qualifying rows.\nKeep every row factual and anchored.\nDo not prescribe changes.\n</output_contract>';

const TMAP_LAW =
    '<role>\nYou are a read-only recon mapper for one language tests estate.\nTreat the paths named by the task and the required ' +
    'registry-depth sources below as read territory.\nEdit nothing.\nRun no git command.\n</role>\n\n' +
    '<completion_bar>\nDone requires one complete markdown map as the final message.\nObservable completion requires every output-contract ' +
    'section in order, an on-disk source and anchor for every settled row, and [COVERAGE] accounting for every required read.\nThe map is ' +
    'the whole verified substrate the build-out consumes: a build-out holding it re-derives no testing-package member and re-opens no ' +
    'catalog.\nEvery [CAPABILITY] row carries the exact transcribed declaration of its member.\nPlace every unverified file, capability, ' +
    'drift claim, or spine claim in [COVERAGE] with the failed probe.\nExclude unverified claims from settled sections.\n' +
    '</completion_bar>\n\n' +
    '<context_gathering>\nRead fully through EOF in this order:\n1. Read the tests README, inventory the language tests tree named by the ' +
    'task, and read every file in that tree.\n2. Read every root spine file named by the task.\n3. Read each applicable language-libs ' +
    'README package-registry section, the testing-relevant .api catalogs the task names, and every observability page the task names.\n\n' +
    'Treat the named tests tree, named spine files, package registries, named catalogs, and named observability pages as the census ' +
    'boundary.\nDo not expand into unlisted implementation pages.\nTranscribe each catalog declaration you cite at the point of reading; a ' +
    'member recorded as a bare name costs a second read the budget does not carry.\nRecord an absent task-named or required path in ' +
    '[COVERAGE] with its absolute attempted path and filesystem probe.\nDo not substitute a likely path for a missing path.\nUse at most ' +
    '100 tool calls across discovery and verification.\nCount every tool invocation against the single budget.\nRead in small batches that ' +
    'preserve complete output.\nDo not concatenate the territory into one command.\nReserve enough calls to re-open every cited anchor and ' +
    'every transcribed declaration during verification.\nStop discovery when the estate map is complete.\nWhen the remaining budget is ' +
    'required for verification, route unresolved discovery to [COVERAGE] instead of re-reading.\n</context_gathering>\n\n' +
    '<verification>\nRe-open every source anchor cited by a settled row.\nRe-confirm each _testkit and harness fact against its owning ' +
    'file.\nRe-open the catalog row behind every [CAPABILITY] declaration and confirm the transcription character for character across ' +
    'owning type, member name, generic parameters, every parameter name and type, and the return type.\nTreat a [CAPABILITY] row carrying ' +
    'a bare member name, a catalog heading, or a paraphrased declaration as an incomplete-signature defect: complete it from the catalog ' +
    'or move it to [COVERAGE] as unverified.\nVerify each [GAPS] absence claim across the complete named tests-tree boundary.\nName the ' +
    'build-out axis governing each verified gap.\nVerify each [DRIFT] claim by comparing the tests-estate claim with the current libs ' +
    'registry or observability anchor.\nVerify each [SPINE] claim against both the root-config row and the affected tests-estate ' +
    'evidence.\nTreat a substring, inferred relationship, or unmatched name as unverified.\nRemove any settled row whose evidence fails ' +
    're-confirmation.\nRecord the removed claim and failed probe in [COVERAGE].\n</verification>\n\n' +
    '<output_contract>\nReturn one markdown document with nothing before or after it.\nUse exactly these H2 sections in this order.\nAdd ' +
    'no other H2 section.\n\n' +
    '## [FACTS]\nUse one "- " row per verified _testkit or harness inventory fact.\nShape each row as: source: <absolute path> | anchor: ' +
    '<symbol or heading> | owner: <_testkit or harness owner> | fact: <verified current capability>.\n\n' +
    '## [CAPABILITY]\nUse one "- " row per testing-package member the spine cites or a build-out axis admits.\nSet status "cited" when a ' +
    'spine row or existing harness names the member, and "admitted" when an axis admits it and no estate file composes it.\nShape each row ' +
    'as: status: cited|admitted | member: <exact name> | owner: <declaring type or module> | signature: <complete declaration transcribed ' +
    'from the catalog, carrying generic parameters, every parameter name and type, and the return type> | catalog: <absolute .api path and ' +
    'anchor> | tier: folder|language-root | axis: <exact task spelling, admitted rows only> | absence-boundary: <estate files checked, ' +
    'admitted rows only>.\nTranscribe each declaration exactly as its catalog states it.\nA row without a complete signature is a defect, ' +
    'not a row.\n\n' +
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
const uniq = (xs) => [...new Set(xs)];

const baseOf = (f) => {
    const segs = f.path.replace(/\/+$/, '').split('/');
    const tail = segs[segs.length - 1];
    return tail === '.planning' ? (f.language === 'cross' ? 'libs' : 'branch') : tail;
};
const fid = (f) => (SHORT[f.language] || 'xx') + '-' + slugOf(baseOf(f));
const bid = (f, i) => fid(f) + '-b' + (i + 1);
const planningRootOf = (f) => (f.path.endsWith('/.planning') ? f.path : f.path + '/.planning');
// Units arrive dependency-ordered (a minting unit precedes every unit extending it); packing fills in that order so order survives,
// the ceiling caps every writer's page load, and a starved tail folds back inside the overflow allowance.
const packBatches = (f, units) => {
    const bins = [];
    for (const u of units.slice().sort((a, b) => a.order - b.order)) {
        const last = bins[bins.length - 1];
        const merged = last ? uniq(last.pages.concat(u.pages || [])) : null;
        if (last && merged.length <= BATCH_PAGES) {
            last.units.push(u);
            last.pages = merged;
        } else bins.push({ units: [u], pages: uniq(u.pages || []) });
    }
    if (bins.length > 1) {
        const tail = bins[bins.length - 1];
        const prev = bins[bins.length - 2];
        const merged = uniq(prev.pages.concat(tail.pages));
        if (tail.pages.length * 2 < BATCH_PAGES && merged.length <= BATCH_PAGES * BATCH_OVERFLOW) {
            prev.units.push(...tail.units);
            prev.pages = merged;
            bins.pop();
        }
    }
    return bins.map((b, i) => ({
        id: bid(f, i),
        title: b.units.map((u) => u.title).join(' + '),
        members: b.units.map((u) => u.title),
        heavy: b.units.some((u) => u.heavy),
        cards: uniq(b.units.flatMap((u) => u.cards || [])),
        pages: b.pages,
        anchors: uniq(b.units.flatMap((u) => u.anchors || [])),
        packages: uniq(b.units.flatMap((u) => u.packages || [])),
        rippleSurface: b.units.flatMap((u) => u.rippleSurface || []),
        blockers: b.units.flatMap((u) => u.blockers || []),
    }));
};
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
        ? 'libs-wide for this batch — every touchpoint page its cards name across ' +
          REPO +
          '/libs plus ' +
          f.path +
          '; both ends of every touched seam land in the same pass, and a cross-libs card closes only when every folder ripple is on disk'
        : f.path + ' only';
const memoryClause = (f) =>
    f.language === 'csharp' || f.language === 'cross'
        ? ', and the memory index at /Users/bardiasamiee/.claude/projects/-Users-bardiasamiee-Documents-99-Github-Rasm/memory/MEMORY.md — ' +
          'open every reference_* entry naming a surface this batch composes (RhinoCommon, GH2, Eto, LanguageExt, Thinktecture, telemetry traps)'
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
          ' IN FULL from disk. Every settled row arrives twice-verified against disk, and its [CAPABILITY] rows carry exact transcribed ' +
          'declarations — owning type, generic parameters, parameter list with types, return type. Compose those spellings straight into ' +
          'fences; re-open a catalog ONLY where a fence settles a member the map routed to [COVERAGE] unverified. Its status:required ' +
          '[CAPABILITY] rows are the members your cards name, its status:unexploited rows your composition queue, [DEFECTS] your repair ' +
          'queue, [RESEARCH] your debt queue, [SEAMS] your both-ends obligation. Re-deriving what the map already settled is wasted ' +
          'budget, not diligence.'
        : '— NONE landed for this batch; your own full territory read replaces it.';
const guideClause = (u, guideR) =>
    u.blockers.length
        ? ' BLOCKED CARDS: read the guidance dossier ' +
          (guideR && guideR.ok ? 'at ' + guideR.report + ' IN FULL from disk' : '— the guidance leg failed; run its verification routes yourself') +
          '; guidance is decision input (verified facts, spellings, route answers), never design — the design is yours. Implement each unblock; ' +
          'a blocker proven genuinely unresolvable (external dependency, frozen wire) closes its card back to [BLOCKED] with a sharpened arming ' +
          'condition citing the dossier path; every other blocked card resolves and realizes in this pass.'
        : '';
const tmapClause = (mapR) =>
    mapR && mapR.ok
        ? 'at ' +
          mapR.report +
          ' IN FULL from disk. Every settled row arrives twice-verified, and its [CAPABILITY] rows carry the exact transcribed ' +
          'declarations of the testing packages the spine cites — compose those spellings straight into the rails and re-open a catalog ' +
          'ONLY where a rail settles a member the map routed to [COVERAGE] unverified. Its [GAPS] rows are your build-out queue, [DRIFT] ' +
          'your repair queue, [SPINE] your alignment queue. Re-deriving what the map already settled is wasted budget, not diligence.'
        : '— NONE landed; your own full estate read replaces it, including the testing-package catalogs at their owning tiers.';
const receiptDoc = (id, stage) =>
    'Write your COMPLETE receipt document as one JSON object to ' +
    OUT +
    '/' +
    id +
    '-' +
    stage +
    '.json (Write tool, absolute path): {folder, stage, filesTouched: [absolute paths], crossFolderRows: [{targetFile, language, change, ' +
    'origin}], rippleLedger: [{endpoint, disposition: "landed"|"deferred"|"out-of-theme", evidence}] — one row per batch rippleSurface ' +
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

const cfgOf = (o) => {
    const rows = [];
    if (o.effort) rows.push('"model_reasoning_effort":"' + o.effort + '"');
    if (o.web) rows.push('"web_search":"live"');
    return rows.length ? 'config={' + rows.join(',') + '}, ' : '';
};
const probeOf = (v, report) =>
    v.sections
        ? 'grep -oE "^## \\[(' +
          v.sections.join('|') +
          ')\\]" ' +
          report +
          ' | sort -u | wc -l — a count under ' +
          v.sections.length +
          ' means a missing contract section, a malformed product'
        : v.marker
          ? 'grep -c "' + v.marker + '" ' + report + ' — the count must equal ' + v.count + ', and a mismatch means a dropped section'
          : 'jq -e ' + JSON.stringify(v.jq) + ' ' + report + ' >/dev/null — a nonzero exit means a missing or malformed product';

// Sandbox decides authorship: a read-only delegate cannot write, so the wrapper lands the content; a writing delegate lands its own product.
const relayPrompt = (o) => {
    const many = Array.isArray(o.rows);
    const sandbox = o.sandbox || 'read-only';
    const authored = sandbox === 'workspace-write';
    return (
        'DISPATCH ROLE: a delegate performs the complete work below through ' +
        (many ? String(o.rows.length) + ' SEQUENTIAL blocking delegate calls, one per ROW' : 'one blocking delegate call') +
        '; never perform, edit, judge, research, or relay the work yourself. (1) ToolSearch "select:mcp__codex__codex"' +
        (many ? ' once' : '') +
        '. (2) ' +
        (many ? 'Per row call' : 'Call') +
        ' mcp__codex__codex' +
        (many ? '' : ' ONCE') +
        ' with model="gpt-5.6-sol", cwd="' +
        REPO +
        '", sandbox="' +
        sandbox +
        '", approval-policy="never", ' +
        cfgOf(o) +
        '"developer-instructions" = the LANE LAW block below VERBATIM, prompt = ' +
        (many ? 'the ROW TASK block below with that row values substituted' : 'the TASK block below VERBATIM') +
        '. On a tool error retry ' +
        (many ? 'that row ONCE, then record it failed and continue' : 'ONCE with a sharpened prompt — your whole recovery budget') +
        '. (3) ' +
        (authored
            ? 'The delegate lands the product itself at ' + o.report + ' as its final act.'
            : 'Each tool result is a JSON envelope {threadId, content}; ' +
              (many
                  ? 'concatenate the CONTENT texts (never the envelopes) in row order and Write the result'
                  : 'Write the CONTENT text (never the envelope) unmodified') +
              ' to ' +
              o.report +
              ' (delete any leftover file there first).') +
        ' (4) Verify with one Bash call: ' +
        probeOf(o.verify, o.report) +
        '; on a miss re-derive the product from the tool result once and re-probe, and a second miss returns ok=false with the probe ' +
        'output. (5) ' +
        o.ret +
        '\n\nLANE LAW:\n\n' +
        o.law +
        (many ? '\n\nROW TASK:\n\n' + o.rowTask + '\n\nROWS:\n\n' + JSON.stringify(o.rows) : '\n\nTASK:\n\n' + o.task)
    );
};

const DISCOVER_LAW =
    '<role>\nYou are a read-only roster resolver for the Rasm planning corpus.\nTreat the libs tree as read territory.\nEdit nothing ' +
    'other than the product the task names.\nRun no git command.\n</role>\n\n' +
    '<completion_bar>\nDone requires the product file holding one JSON object and nothing else.\nEvery folder row carries an absolute ' +
    'path, its language, and an open-card count derived from the exact pattern the task states.\nA folder whose card files could not be ' +
    'read is omitted, never guessed at.\n</completion_bar>\n\n' +
    '<context_gathering>\nEnumerate directories first, then count within the enumerated set.\nUse at most 40 tool calls.\nCount every ' +
    'tool invocation against the single budget.\nBatch the counting sweep rather than probing one folder per call.\n' +
    '</context_gathering>\n\n' +
    '<verification>\nRe-probe every folder admitted to the roster and confirm at least one card file exists at its root.\nConfirm each ' +
    'count against the stated pattern rather than a similar one.\nDrop any row whose path fails re-confirmation.\n</verification>\n\n' +
    '<output_contract>\nWrite one JSON object {folders: [{path, language, openCards}]} to the path the task names.\nEvery path value is ' +
    'absolute.\nYour final message is the same JSON object only, with no prose outside it and no code fence.\n</output_contract>';

const AUDIT_LAW =
    '<role>\nYou are a read-only closure auditor for one theme slice of the Rasm planning corpus.\nTreat the card files and design pages ' +
    'the task names as read territory.\nEdit nothing other than the product the task names.\nRun no git command.\nFix nothing.\n</role>\n\n' +
    '<completion_bar>\nDone requires the product file holding one JSON object and nothing else.\nEvery roster entry is derived from a ' +
    'current on-disk marker, never from a prior receipt claim.\nAn unreachable card file is rostered with its failed probe, never silently ' +
    'dropped.\nCounts only: state no verdict beyond the rosters the output contract names.\n</completion_bar>\n\n' +
    '<context_gathering>\nRead the card file of every selected card the task names, then sweep the terminal research sections across the ' +
    'roots the task names, then the receipt documents the task names.\nUse at most 90 tool calls.\nCount every tool invocation against the ' +
    'single budget.\nBatch each sweep rather than probing one file per call.\n</context_gathering>\n\n' +
    '<verification>\nRe-open every card admitted to a roster and confirm its current leader marker.\nConfirm each deferred-endpoint and ' +
    'residual claim against current disk before admitting it.\nDrop any entry whose evidence fails re-confirmation.\n</verification>\n\n' +
    '<output_contract>\nWrite one JSON object {openCards: [{file, card}], blockedNoTrigger: [{file, card}], blockedCards, researchRows, ' +
    'researchByLanguage} to the path the task names.\nresearchByLanguage is a list of "language: N" strings.\nEvery path value is ' +
    'absolute.\nYour final message is the same JSON object only, with no prose outside it and no code fence.\n</output_contract>';

const discoverTask = (report) =>
    'Goal: resolve the card-owning folder roster from disk and write it as one JSON object.\n' +
    'Folders: every package directory directly under ' +
    REPO +
    '/libs/csharp, ' +
    REPO +
    '/libs/python, and ' +
    REPO +
    '/libs/typescript that carries IDEAS.md or TASKLOG.md at its root (skip node_modules and dot-directories); plus each branch tier ' +
    REPO +
    '/libs/{csharp,python,typescript}/.planning where card files exist; plus the cross-libs tier ' +
    REPO +
    '/libs/.planning.\n' +
    'Per folder count OPEN cards: lines matching ^\\[[A-Za-z0-9_-]+\\]-\\[(ACTIVE|QUEUED|BLOCKED)\\] inside the [01]-[OPEN] section of ' +
    'IDEAS.md plus TASKLOG.md ("(none)" is zero). Language derives from the path segment under libs/; libs/.planning is language "cross".' +
    (SCOPE.length ? ' Restrict the roster to folders matching any of: ' + JSON.stringify(SCOPE) + '.' : '') +
    '\nDone when: ' +
    report +
    ' holds one JSON object {folders: [{path, language, openCards}]} and nothing else.';

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
    'one final research-drain unit ordered last; a non-pertinent row on a page no unit touches stays. A unit is the SMALLEST COHERENT ' +
    'PAGE-DISJOINT card cluster: cards sharing a page or one owner surface cluster into one unit, and two clusters sharing no page stay ' +
    'separate units. Units are NOT writer-sized — a packer downstream merges them into writer batches under a page ceiling, and it packs ' +
    'cleanly only when units are page-disjoint. So never one unit per card when those cards share a page (the packer cannot split one ' +
    'page across two writers), and never one unit spanning clusters that share no page (an unsplittable lump the packer cannot balance). ' +
    'A unit whose own page count already exceeds a writer pass is still ONE unit when its pages are genuinely interdependent — say so in ' +
    'note; do not fracture an interdependent cluster to hit a size. Report every page a unit edits: the packer balances on that count, so ' +
    'an understated page list produces an overloaded writer and a starved sibling. Per unit: ' +
    'order (dependency position, 1 first — a unit consuming another unit fence output orders after it, and a unit that MINTS a page or ' +
    'sub-folder orders before every unit extending it; the packer preserves this order inside and across batches, so it is load-bearing); ' +
    'title; heavy (true when the ' +
    'unit spans 3+ pages or creates any page); cards (exact "FILE [ID]" spellings); pages (ABSOLUTE page paths the unit edits, index docs ' +
    'included when touched); anchors (owner symbols and fence names); packages (admitted package names the unit composes); rippleSurface — ' +
    'the unit ripple contract: for each selected card, every consumer, unlock endpoint, or counterpart the card NAMES in its Unlocks, ' +
    'Shape, Ripple, or thesis lands one row {endpoint (the folder or page it names, absolute where resolvable — cross-folder and ' +
    'cross-language included), source ("FILE [ID] Unlocks|Shape|Ripple")} — [] only when no selected card names one; blockers — every ' +
    'BLOCKED card in the unit lands one row {card, question (the blocker restated as an answerable question), route (the resolution route: ' +
    'an assay api member target, a .api catalog, a live doc, a seam-owner page)} — [] when none.' +
    (f.language === 'cross'
        ? ' CROSS-LIBS DECOMPOSITION: a cross-libs card is a multi-folder work unit — decompose it into per-folder ripple executions inside ' +
          'ONE unit: pages list every touchpoint page across libs/ grouped by folder, anchors name both ends of every seam, the title names ' +
          'the folders spanned; the card closes at this tier only when every folder ripple lands.'
        : '') +
    ' LEFTOVERS: every non-theme OPEN card lands one row {item (exact "FILE [ID]" spelling), hint (one short phrase naming the future ' +
    'functionality set it belongs to)}; a hard non-pertinent research cluster on pages no unit touches lands {item: "RESEARCH <absolute ' +
    'page path>", hint}. Zero theme-relevant work = units [] (leftovers still rostered) with note naming what you checked. Units are ' +
    'navigation facts, never verdicts or designs.';

const discoverWrapPrompt = () => {
    const report = OUT + '/discover.json';
    return relayPrompt({
        report,
        law: DISCOVER_LAW,
        task: discoverTask(report),
        verify: { jq: '.folders | length > 0' },
        effort: 'medium',
        ret: 'Return folders = the roster array parsed from the landed product, one {path, language, openCards} row per folder.',
    });
};

const mapTask = (f, b) =>
    'Map work batch "' +
    b.title +
    '" of ' +
    f.path +
    '. BATCH PAGES (read in full): ' +
    JSON.stringify(b.pages) +
    '. CARDS: ' +
    JSON.stringify(b.cards) +
    '. ANCHORS: ' +
    JSON.stringify(b.anchors) +
    '. PACKAGES: ' +
    JSON.stringify(b.packages) +
    '. API TIERS: ' +
    apiTiersOf(f) +
    '. FOLDER: ' +
    f.path +
    '. The writer consuming this map composes from it directly and does not re-open what you settle: transcribe every member the batch ' +
    'cards will compose at exact declaration depth, and route every member you could not verify to [COVERAGE] rather than settling it. A ' +
    'row that restates a catalog heading without its signature costs the writer a re-read and is a lane defect.';

const mapWrapPrompt = (f, b) =>
    relayPrompt({
        report: OUT + '/' + b.id + '-map.md',
        law: MAP_LAW,
        task: mapTask(f, b),
        verify: { sections: MAP_SECTIONS },
        effort: b.heavy ? '' : 'medium',
        ret:
            'Return ok, report, entries = the "- " row count under [DEFECTS], research = the "- " row count under [RESEARCH] (Read the ' +
            'file to count per section; "- none" counts zero), headline = "<defects>d <unexploited>u <research>r", failure empty — or ' +
            'ok=false with the error text VERBATIM.',
    });

const guideWrapPrompt = (f, b) =>
    relayPrompt({
        report: OUT + '/' + b.id + '-guidance.md',
        law: GUIDE_LAW,
        rows: b.blockers,
        rowTask: 'Resolve this blocker into decision inputs. CARD: <card>. QUESTION: <question>. ROUTE: <route>. FOLDER: ' + f.path + '.',
        verify: { marker: '^## \\[BLOCKER\\]', count: b.blockers.length },
        effort: 'medium',
        web: true,
        ret:
            'Return ok, report, entries = the RESOLVABLE verdict count, research = 0, headline = "<resolvable>/<total> resolvable", ' +
            'failure = the failed row cards or empty.',
    });

const implPrompt = (f, b, mapR, guideR) =>
    'IMPLEMENT work batch "' +
    b.title +
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
    '. Then run your OWN blind DESIGN pass over the batch pages: owner shape, collapse rulings, capability bar, and the defect verdict ' +
    'are yours to derive from disk and are never inherited. FACTS are the opposite — member spellings, seam endpoints, registry state, ' +
    'and research debt arrive settled; read the fact map ' +
    mapClause(mapR) +
    guideClause(b, guideR) +
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
    'repaired to zero FAIL. BATCH: units (land in this order) ' +
    JSON.stringify(b.members) +
    '; cards ' +
    JSON.stringify(b.cards) +
    '; pages ' +
    JSON.stringify(b.pages) +
    '; anchors ' +
    JSON.stringify(b.anchors) +
    '; packages ' +
    JSON.stringify(b.packages) +
    '. The batch is several dependency-ordered units packed into one pass: a unit that mints a page or owner lands before every unit ' +
    'extending it, and a later unit composing an earlier unit fence output reads the landed fence, never the card. Every card of every ' +
    'unit reaches a terminal disposition in this pass — a unit left unstarted is a residuals row naming it, never a silent omission. ' +
    receiptDoc(b.id, 'implement') +
    thinReceipt(f, 'implement');

const critTask = (f, b, impl, mapR) =>
    'Goal: audit and repair work batch "' +
    b.title +
    '" of ' +
    f.path +
    ' IN PLACE — doctrinal conformance and capability completeness across every page the batch touched.\n' +
    'Theme: ' +
    THEME_LAW +
    '\n' +
    'Context: write territory ' +
    territoryOf(f) +
    '; doctrine root files ' +
    doctrineOf(f) +
    '; batch pages ' +
    JSON.stringify(b.pages) +
    '; cards ' +
    JSON.stringify(b.cards) +
    '; anchors ' +
    JSON.stringify(b.anchors) +
    '; packages ' +
    JSON.stringify(b.packages) +
    '; api tiers ' +
    apiTiersOf(f) +
    '; rippleSurface ' +
    JSON.stringify(b.rippleSurface) +
    '; fact map ' +
    ((mapR && mapR.ok && mapR.report) || 'none — your own read replaces it') +
    '; implement receipt ' +
    ((impl && impl.report) || 'none') +
    '; implement-touched files ' +
    JSON.stringify((impl && impl.filesTouched) || []) +
    '.\n' +
    'Derive your own defect list from the pages on disk BEFORE opening either prior product. The map and the implement receipt are ' +
    'unverified prior claims to refute against current disk; the touched-file list is navigation only and bounds nothing.\n' +
    'A non-theme open card sharing a card file with batch cards stays byte-untouched — an expected leftover for a future slice, never ' +
    'dispositioned, never reworded.\n' +
    'Constraints: absolute paths everywhere; no source-tree file; no central manifest; no package install; no git command.\n' +
    'Done when: every defect your audit confirms is FIXED in place — never noted — and the receipt document is written to ' +
    OUT +
    '/' +
    b.id +
    '-critique.json.';

const critWrapPrompt = (f, b, impl, mapR) =>
    relayPrompt({
        report: OUT + '/' + b.id + '-critique.json',
        law: CRIT_LAW,
        task: critTask(f, b, impl, mapR),
        sandbox: 'workspace-write',
        verify: { jq: '.filesTouched and .cardsClosed' },
        ret:
            'Return the thin receipt: folder ' +
            f.path +
            ', stage "critique", ok, report, filesTouched = jq ".filesTouched", crossFolderRows/cardsClosed/residuals = the jq array ' +
            'lengths, researchResolved = jq ".researchResolved.resolved", packagesMissing = jq ".packagesMissing" rows verbatim, one-line ' +
            'headline, failure empty — or ok=false with the error text VERBATIM.',
    });

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
    'get_latest_package_version for csharp, the live registry for python/typescript; rejection vocabulary is closed: reject ONLY when an ' +
    'already-admitted package supersedes the capability, when existence or installability fails verification, when the license gate ' +
    'fails — this estate is fully OSS with zero commercial intent, any license granting full free use to an OSS project admits (copyleft ' +
    'included) and only payment-required or paid-tier-gated capability rejects — or when any chain step below fails unrecoverably after ' +
    'one self-heal attempt (EXECUTION FAILURE — the failed step named with its evidence), returning admitted=false with the ruling. (2) Land the ' +
    'central manifest row in its ' +
    'owning group (Directory.Packages.props label group / pyproject.toml lean unpinned / pnpm-workspace.yaml cluster). (3) Prove the ' +
    'install gate green (dotnet restore over the consuming closure / uv sync / pnpm install), self-healing or reverting what cannot ' +
    'resolve — a reverted admission returns admitted=false with the resolver evidence. (4) Author the .api catalog at the correct tier — ' +
    'folder overlay vs language-root substrate per the two-tier law — with verified members only; an isolation or app-scoped claim lands ' +
    'ONLY with per-instance member evidence (a per-instance handle, factory, or scope parameter verified on the decompiled/installed ' +
    'surface), a process-global surface stating its host-wide scope honestly and the single-owner admission it demands. (5) Land the README registry row at the ' +
    'consuming folder. (6) Land the csproj reference where consumed (csharp). (7) Run the docgen gate script at ' +
    REPO +
    '/.claude/skills/docgen/scripts/prose_gate.py (invocation per its --help) over the new files, repaired to zero FAIL. Any of steps ' +
    '(4)–(7) failing unrecoverably after one self-heal attempt is an EXECUTION FAILURE that REVERTS this admission whole — the manifest ' +
    'row and every partial artifact landed this call (catalog, README row, csproj reference) removed so the install gate stays green — ' +
    'returning admitted=false with the failed step named and its evidence; a reverted admission is a clean rejection, never a ' +
    'half-landed package. Write your full ' +
    'admission report to ' +
    OUT +
    '/admit-' +
    slugOf(row.package) +
    '.md, then return: package, language, ok, admitted, report, filesTouched, headline, failure.';

const composePrompt = (f, b, landed) =>
    'COMPOSE pass for work batch "' +
    b.title +
    '" of ' +
    f.path +
    ' — packages admitted from this batch implement and critique rows: ' +
    JSON.stringify(landed.map((a) => ({ package: a.package, language: a.language, catalog: a.report }))) +
    '. Write territory: ' +
    territoryOf(f) +
    '. ' +
    PATH_LAW +
    ' Read each admitted package catalog at its landed tier IN FULL, then compose its members into the batch owning fences wherever the ' +
    'batch concept admits them — as rows, cases, fields, operations, or policy values on existing owners, at the campaign bar. ' +
    FENCE_LAW +
    ' A package whose capability the batch concept does not admit records one residuals row naming the ruling; edit nothing the admitted ' +
    'catalogs do not motivate. ' +
    ROW_LAW +
    ' BATCH pages: ' +
    JSON.stringify(b.pages) +
    '. ' +
    receiptDoc(b.id, 'compose') +
    thinReceipt(f, 'compose');

const drainPrompt = (L, receiptPaths) =>
    'DRAIN ' +
    L.key +
    ' — terminal cross-folder reconciler. Write territory: EXACTLY ' +
    REPO +
    '/' +
    L.root +
    '; never a central manifest, and never ' +
    (L.key === 'cross'
        ? 'a language tree under ' + REPO + '/libs/{csharp,python,typescript} — those are the sibling drains territory'
        : 'a sibling language tree, never ' + REPO + '/libs/.planning — that is the cross drain territory') +
    '. ' +
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

const rtPrompt = (L, drainR, receiptPaths, admitRows, residualRows, censusR) =>
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
    'recorded. (5) .API ULTRA-STACKING and (6) FOUR-SURFACE CLOSURE both ACT ON the closure census at ' +
    ((censusR && censusR.ok && censusR.report) || '(no census document — enumerate the surfaces yourself)') +
    ', read IN FULL from disk — verified inventory, not a lead: do not re-enumerate the catalogs, registries, or manifests it covers. Its ' +
    '[UNEXPLOITED] rows are admitted capability no owner composes, each carrying its exact transcribed declaration — close each in the ' +
    'owning fence or record the ruling that refuses it. Its [SURFACES], [ORPHANS], and [DUPLICATES] rows are the manifest <-> .api ' +
    'catalogs (both tiers) <-> README registries <-> project manifests (csproj rows for csharp) drift set — repair each at its owning ' +
    'surface. A census row you refute against current disk records refuted with its citation; a drift the census missed and your own pass ' +
    'exposes you fix and name. A folder .api file duplicating a branch-substrate catalog collapses to a folder-specific overlay or deletes ' +
    'per the two-tier law. Closure repairs on the central manifest are yours (orphan and duplicate rows only, surgical anchored Edits — a ' +
    'tests-estate lane may hold test rows in the same file); a NEW admission routes as a residual row. (7) DOCGEN-ZERO PROOF — run the ' +
    'docgen gate script at ' +
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
    ' You are the sole actor in this lane: dispatch nothing, delegate nothing, spawn no sub-agent. Every read, verification, and write is ' +
    'yours. The census already carries the enumeration volume that once justified a leg — where the remaining sweep still exceeds your ' +
    'context, narrow by evidence (attack the owners the campaign touched, in receipt order) and return what you could not reach as ' +
    'residualRows naming the unswept boundary; a silent partial sweep reported as complete is the one unrecoverable failure here. ' +
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

const censusTask = (L) =>
    'Census the ' +
    L.key +
    ' four closure surfaces. Territory: the central manifest (' +
    (L.key === 'csharp' ? 'Directory.Packages.props' : L.key === 'python' ? 'pyproject.toml' : 'pnpm-workspace.yaml') +
    ' at ' +
    REPO +
    '), every .api catalog under the language-root tier ' +
    REPO +
    '/libs/' +
    L.key +
    '/.api, every folder-tier .api catalog under ' +
    REPO +
    '/' +
    L.root +
    ', the README package-registry section of every package folder under that root, and the project manifests (' +
    (L.key === 'csharp' ? 'every .csproj under the root' : L.key === 'python' ? 'the pyproject tool sections' : 'each package.json under the root') +
    '). Grade [UNEXPLOITED] against the owners on the design pages under that root: an admitted member no owner composes is a row.';

const censusWrapPrompt = (L) =>
    relayPrompt({
        report: OUT + '/census-' + L.key + '.md',
        law: CENSUS_LAW,
        task: censusTask(L),
        verify: { sections: CENSUS_SECTIONS },
        effort: 'medium',
        ret:
            'Return ok, report, entries = the "- " row count under [ORPHANS] plus [DUPLICATES], research = 0, headline = "<orphans>o ' +
            '<duplicates>d <unexploited>u", failure empty — or ok=false with the error text VERBATIM.',
    });

const testsMapTask = (L) =>
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
    ' README registries and observability pages). BUILD-OUT AXES to grade [GAPS] and [CAPABILITY] against: ' +
    AXES[L.key] +
    '. The build-out consuming this map composes from it directly and does not re-open what you settle: transcribe every testing-package ' +
    'member the spine cites or an axis admits at exact declaration depth.';

const testsMapWrapPrompt = (L) =>
    relayPrompt({
        report: OUT + '/tests-' + L.key + '-map.md',
        law: TMAP_LAW,
        task: testsMapTask(L),
        verify: { sections: TMAP_SECTIONS },
        effort: 'medium',
        ret:
            'Return ok, report, entries = the "- " row count under [GAPS] ("- none" counts zero), research = 0, headline = "<gaps>g ' +
            '<drift>d <spine>s", failure empty — or ok=false with the error text VERBATIM.',
    });

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
    '/, then the tests map ' +
    tmapClause(mapR) +
    ' Build-out axes (a floor, hunted past): ' +
    AXES[L.key] +
    '; shared logic and capabilities consolidated into the _testkits; drift and stale content fixed against the post-implementation libs ' +
    'surface; the root spine aligned. ' +
    PATH_LAW +
    ' GATE: after the final edit run ' +
    GATES[L.key] +
    ', batched ONCE, and repair what it reports to green before returning; gateClean is ' +
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

const auditTask = (roots, selected, residuals, report) =>
    'Goal: prove slice closure for theme "' +
    THEME.title +
    '". Count, fix nothing, edit nothing. SELECTED CARDS (this slice contract, exact "FILE [ID]" rows): ' +
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
    'Constraints: read only; edit no file other than the product; no git command; counts only, no verdicts beyond the rosters.\n' +
    'Done when: ' +
    report +
    ' holds one JSON object {openCards: [{file, card}], blockedNoTrigger: [{file, card}], blockedCards, researchRows, researchByLanguage: ' +
    '["language: N"]} and nothing else — no prose, no code fence. An unreachable card file is rostered in openCards with the failed probe ' +
    'as its card value, never silently dropped.';

const auditWrapPrompt = (roots, selected, residuals) => {
    const report = OUT + '/audit.json';
    return relayPrompt({
        report,
        law: AUDIT_LAW,
        task: auditTask(roots, selected, residuals, report),
        verify: { jq: 'has("openCards") and has("researchRows")' },
        effort: 'medium',
        ret: 'Return openCards, blockedNoTrigger, blockedCards, researchRows, researchByLanguage exactly as the landed product reports them.',
    });
};

// --- [COMPOSITION] -------------------------------------------------------------------

if (!CAMP) return { skipped: true, reason: 'args.camp (absolute campaign home) is required' };

const disco = await guard(
    slot(() =>
        agent(discoverWrapPrompt(), {
            label: 'relay:discover',
            phase: 'Plan',
            model: 'sonnet',
            effort: 'low',
            schema: DISCOVERY,
            stallMs: STALL_MS,
        }),
    ),
);
let folders = (disco?.folders ?? []).filter((f) => f.path && f.language);
if (SCOPE.length)
    folders = folders.filter((f) => SCOPE.some((s) => f.path === s || f.path.endsWith('/' + s.replace(/\/+$/, '')) || f.path.includes(s)));
if (!folders.length) return { skipped: true, reason: 'no card-owning folders resolved', scope: SCOPE };
log(folders.length + ' folder(s), ' + sum(folders, (f) => f.openCards) + ' open card(s)');

// --- [FOLDER_CHAINS]

const manifestQueue = {};
const admitSeen = {}; // one chain per package@language — a settled ruling is shared, an execution failure evicts so a later requester retries
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
    p.then(
        (r) => {
            if (!(r && r.ok)) delete admitSeen[seenKey];
        },
        () => {
            delete admitSeen[seenKey];
        },
    );
    return p;
};
const missingOf = (rec) => (rec && rec.ok && rec.packagesMissing) || [];
const runAdmissions = async (rows, origin) => {
    const seen = new Set();
    const wanted = (rows || []).filter((r) => {
        if (!r || !r.package || !LANGS.some((L) => L.key === r.language)) return false;
        const k = r.language + ':' + slugOf(r.package);
        if (seen.has(k)) return false;
        seen.add(k);
        return true;
    });
    return (await Promise.all(wanted.map((r) => admit(r, origin)))).filter(Boolean);
};

const runFolderChain = async (f, plan) => {
    const batches = packBatches(f, plan?.units ?? []);
    if (!batches.length) {
        log((plan ? 'no-op' : 'PLAN FAILED') + ': ' + f.path);
        return { folder: f.path, language: f.language, planOk: !!plan, batches: [], leftovers: plan?.leftovers ?? [] };
    }
    log(f.path + ': ' + (plan?.units ?? []).length + ' unit(s) packed into ' + batches.length + ' batch(es)');
    const out = [];
    for (const b of batches) {
        const [mapR, guideR] = await Promise.all([
            guard(
                slot(() =>
                    agent(mapWrapPrompt(f, b), {
                        label: 'relay:map:' + b.id,
                        phase: 'Map',
                        model: 'sonnet',
                        effort: 'low',
                        schema: RECEIPT,
                        stallMs: STALL_MS,
                    }),
                ),
            ),
            b.blockers.length
                ? guard(
                      slot(() =>
                          agent(guideWrapPrompt(f, b), {
                              label: 'relay:guide:' + b.id,
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
        const impl = await guard(
            slot(() =>
                agent(implPrompt(f, b, mapR, guideR), {
                    label: 'impl:' + b.id,
                    phase: 'Implement',
                    model: 'fable',
                    schema: STAGE,
                    stallMs: STALL_MS,
                }),
            ),
        );
        const crit =
            impl && impl.ok
                ? await guard(
                      slot(() =>
                          agent(critWrapPrompt(f, b, impl, mapR), {
                              label: 'relay:crit:' + b.id,
                              phase: 'Critique',
                              model: 'sonnet',
                              effort: 'low',
                              schema: STAGE,
                              stallMs: STALL_MS,
                          }),
                      ),
                  )
                : null;
        // One admission barrier per batch: implement and critique findings enter the chain together, then one compose lands them.
        const admitted = await runAdmissions(missingOf(impl).concat(missingOf(crit)), f.path);
        const landed = admitted.filter((a) => a && a.ok && a.admitted);
        const comp = landed.length
            ? await guard(
                  slot(() =>
                      agent(composePrompt(f, b, landed), {
                          label: 'compose:' + b.id,
                          phase: 'Compose',
                          model: 'opus',
                          schema: STAGE,
                          stallMs: STALL_MS,
                      }),
                  ),
              )
            : null;
        out.push({
            batch: b.id,
            members: b.members,
            cards: b.cards,
            blockers: b.blockers.length,
            mapR,
            guideR,
            impl,
            crit,
            comp,
            admissions: admitted,
        });
    }
    return { folder: f.path, language: f.language, planOk: true, batches: out, leftovers: plan?.leftovers ?? [] };
};

const planLane = (f) =>
    guard(
        slot(() =>
            agent(planPrompt(f), {
                label: 'plan:' + fid(f),
                phase: 'Plan',
                model: 'opus',
                schema: UNITS,
                stallMs: STALL_MS,
            }),
        ),
    );

const folderRecs = folders.filter((f) => f.language !== 'cross');
const crossRecs = folders.filter((f) => f.language === 'cross');
const crossPlanPs = crossRecs.map((f) => planLane(f)); // cross plans start beside the folder plans; only their chains wait
const chains = (await pipeline(folderRecs, planLane, (plan, f) => runFolderChain(f, plan))).filter(Boolean);
const crossChains = []; // cross-libs batches run after every folder chain — their touchpoints span the trees the folder writers just closed
for (let i = 0; i < crossRecs.length; i++) {
    const c = await runFolderChain(crossRecs[i], await crossPlanPs[i]);
    if (c) crossChains.push(c);
}
const allChains = chains.concat(crossChains);

const batchRecs = allChains.flatMap((c) => c.batches.map((x) => ({ ...x, folder: c.folder, language: c.language })));
const receiptPaths = batchRecs.flatMap((x) =>
    [x.impl && x.impl.ok && x.impl.report, x.crit && x.crit.ok && x.crit.report, x.comp && x.comp.ok && x.comp.report].filter(Boolean),
);
const admissions = batchRecs.flatMap((x) => x.admissions || []);
log(batchRecs.length + ' batch(es), ' + receiptPaths.length + ' receipt(s), ' + admissions.length + ' admission(s)');

// --- [SEAL_AND_TESTS]

let drainUnrouted = [];
const implFailedAll = batchRecs.length > 0 && !batchRecs.some((x) => x.impl && x.impl.ok);
if (implFailedAll) log('SEAL SKIPPED: every implement lane failed — ' + batchRecs.length + ' batch(es) left unsealed on disk');
const sealP = receiptPaths.length
    ? (async () => {
          // Barrier is genuine: drain residualRows route ACROSS trees, so every drain must land before any census or redteam starts.
          const drainRs = await parallel(
              DRAINS.map(
                  (D) => () =>
                      guard(
                          slot(() =>
                              agent(drainPrompt(D, receiptPaths), {
                                  label: 'drain:' + D.key,
                                  phase: 'Drain',
                                  model: 'fable',
                                  schema: DRAINR,
                                  stallMs: STALL_MS,
                              }),
                          ),
                      ),
              ),
          );
          const drainOf = (key) => drainRs[DRAINS.findIndex((D) => D.key === key)] || null;
          const allResiduals = drainRs.flatMap((dr) => (dr && dr.residualRows) || []);
          const residualFor = (D) => allResiduals.filter((r) => String(r).includes('/' + D.root + '/'));
          const routed = new Set(DRAINS.flatMap((D) => residualFor(D)));
          drainUnrouted = allResiduals.filter((r) => !routed.has(r));
          if (drainUnrouted.length) log(drainUnrouted.length + ' drain residual(s) target no drained tree — carried to the audit');
          // Arming is evidence, not label: a cross-libs batch edits language trees directly, so the receipts decide.
          const touchedTree = (root) =>
              batchRecs.some((x) =>
                  [x.impl, x.crit, x.comp].some((r) => r && r.ok && (r.filesTouched || []).some((p) => String(p).includes('/' + root + '/'))),
              );
          const armed = LANGS.filter((L) => {
              const dr = drainOf(L.key);
              return touchedTree(L.root) || (dr && dr.applied > 0) || residualFor(L).length > 0;
          });
          const censusRs = {};
          for (const x of (
              await parallel(
                  armed.map(
                      (L) => () =>
                          guard(
                              slot(() =>
                                  agent(censusWrapPrompt(L), {
                                      label: 'relay:census:' + L.key,
                                      phase: 'Census',
                                      model: 'sonnet',
                                      effort: 'low',
                                      schema: RECEIPT,
                                      stallMs: STALL_MS,
                                  }),
                              ),
                          ).then((r) => ({ key: L.key, r })),
                  ),
              )
          ).filter(Boolean))
              censusRs[x.key] = x.r;
          const seals = await parallel(
              LANGS.map((L) => () => {
                  const dr = drainOf(L.key);
                  if (!armed.some((A) => A.key === L.key)) return Promise.resolve({ language: L.key, census: null, redteam: null });
                  const admitRows = admissions
                      .filter((a) => a && a.language === L.key)
                      .map((a) => ({ package: a.package, admitted: !!(a.ok && a.admitted), catalog: a.report || '' }));
                  return guard(
                      slot(() =>
                          agent(rtPrompt(L, dr, receiptPaths, admitRows, residualFor(L), censusRs[L.key] || null), {
                              label: 'rt:' + L.key,
                              phase: 'Redteam',
                              model: 'fable',
                              schema: REDTEAMR,
                              stallMs: STALL_MS,
                          }),
                      ),
                  ).then((rt) => ({ language: L.key, census: censusRs[L.key] || null, redteam: rt }));
              }),
          );
          return { drains: DRAINS.map((D, i) => ({ key: D.key, r: drainRs[i] || null })), seals };
      })()
    : Promise.resolve({ drains: [], seals: [] });

const testsP = TESTS_ON
    ? pipeline(
          LANGS,
          (L) =>
              guard(
                  slot(() =>
                      agent(testsMapWrapPrompt(L), {
                          label: 'relay:tmap:' + L.key,
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
                          schema: TESTSR,
                          stallMs: STALL_MS,
                      }),
                  ),
              ),
      )
    : Promise.resolve([]);

const [sealRaw, testsRaw] = await Promise.all([sealP, testsP]);
const drainRecs = (sealRaw && sealRaw.drains) || [];
const sealed = ((sealRaw && sealRaw.seals) || []).filter(Boolean);
const tests = testsRaw.filter(Boolean);

// --- [AUDIT]

const selectedCards = batchRecs.flatMap((x) => x.cards || []);
const audit = await guard(
    slot(() =>
        agent(
            auditWrapPrompt(
                folders.map((f) => f.path),
                selectedCards,
                drainUnrouted,
            ),
            {
                label: 'relay:audit',
                phase: 'Audit',
                model: 'sonnet',
                effort: 'low',
                schema: AUDITR,
                stallMs: STALL_MS,
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
    batches: batchRecs.length,
    selectedCards: selectedCards.length,
    implemented: batchRecs.filter((x) => x.impl && x.impl.ok).length,
    critiqued: batchRecs.filter((x) => x.crit && x.crit.ok).length,
    composed: batchRecs.filter((x) => x.comp && x.comp.ok).length,
    failedBatches: batchRecs.filter((x) => !(x.impl && x.impl.ok)).map((x) => x.batch),
    sealSkipped: implFailedAll,
    cardsClosed: sum(
        batchRecs,
        (x) => ((x.impl && x.impl.cardsClosed) || 0) + ((x.crit && x.crit.cardsClosed) || 0) + ((x.comp && x.comp.cardsClosed) || 0),
    ),
    crossFolderRows: sum(
        batchRecs,
        (x) => ((x.impl && x.impl.crossFolderRows) || 0) + ((x.crit && x.crit.crossFolderRows) || 0) + ((x.comp && x.comp.crossFolderRows) || 0),
    ),
    residualClaims: sum(
        batchRecs,
        (x) => ((x.impl && x.impl.residuals) || 0) + ((x.crit && x.crit.residuals) || 0) + ((x.comp && x.comp.residuals) || 0),
    ),
    researchBefore: sum(batchRecs, (x) => (x.mapR && x.mapR.research) || 0),
    researchResolved: sum(
        batchRecs,
        (x) => ((x.impl && x.impl.researchResolved) || 0) + ((x.crit && x.crit.researchResolved) || 0) + ((x.comp && x.comp.researchResolved) || 0),
    ),
    researchAfter: audit ? audit.researchRows : -1,
    researchByLanguage: audit ? audit.researchByLanguage : [],
    admissions: admissions.map((a) => ({ package: a.package, language: a.language, admitted: !!(a.ok && a.admitted), report: a.report || '' })),
    drains: drainRecs.map((d) => ({
        language: d.key,
        ok: !!(d.r && d.r.ok),
        applied: (d.r && d.r.applied) || 0,
        report: (d.r && d.r.report) || '',
    })),
    censuses: sealed.filter((s) => s.census).map((s) => ({ language: s.language, ok: !!s.census.ok, report: s.census.report || '' })),
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
    residuals: drainRecs
        .flatMap((d) => (d.r && d.r.residualRows) || [])
        .concat(sealed.flatMap((s) => (s.redteam && s.redteam.residualRows) || []))
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
        .concat(batchRecs.flatMap((x) => [x.mapR && x.mapR.ok && x.mapR.report, x.guideR && x.guideR.ok && x.guideR.report].filter(Boolean)))
        .concat(admissions.map((a) => a.report).filter(Boolean))
        .concat(drainRecs.map((d) => d.r && d.r.report).filter(Boolean))
        .concat(sealed.flatMap((s) => [s.census && s.census.report, s.redteam && s.redteam.report].filter(Boolean)))
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
