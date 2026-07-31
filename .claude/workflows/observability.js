export const meta = {
    name: 'observability',
    description:
        'Close the observability + analytics campaign against .claude/scratch/observability/CAMPAIGN.md. args = {phase: 6}, bare number accepted, empty = no-op. Eight deep-read recon delegates write dossiers to run scratch, then eight lenses run with full writer authority — conformance, cross-branch parity, backend-family integration, strata leverage, evidence and execution pdelegates, fence seams, escalation arms, and .api ultra-stack truth. The four capability lenses each carry a critique and red-team pair; the four proof lenses run light. One serial custody closer follows, settling governance truth, the scars, the proof estate, and the two escalated adjudications. A residual drain fixpoint closes the run.',
    whenToUse: 'Closing out the observability + analytics campaign in the Rasm planning corpus.',
    phases: [
        { title: 'Recon', detail: 'deep-read mapping delegates; dossier to disk, thin receipt on the wire' },
        { title: 'Kernel', detail: 'serial barrier writer for owners the territory fan composes' },
        { title: 'Implement', detail: 'one lens per territory, full writer authority over its disjoint page set' },
        { title: 'Critique', detail: 'predicate-positive conformance and capability audit, repaired in place' },
        { title: 'RedTeam', detail: 'predicate-negative pre-mortem, rebuilt in place' },
        { title: 'Serial', detail: 'custody closer over shared governance surfaces', model: 'fable' },
        { title: 'Drain', detail: 'cluster pooled residuals by shared file, fix and verify each cluster' },
        { title: 'Verify', detail: 'adversarial proof on disk against the campaign verification section' },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAMPAIGN = '.claude/scratch/observability/CAMPAIGN.md';
// Durable across runs, in the campaign home rather than per-instance run scratch: a residual a phase
// cannot close because a LATER phase owns the file has no other path forward across a gated boundary.
const CARRY = '.claude/scratch/observability/residuals-open.json';
const REPO = '/Users/bardiasamiee/Documents/99.Github/Rasm';
const DRAIN_ROUNDS = 3;
// Ceiling on concurrent drain delegates per round. Clusters stay atomic and are PACKED into delegates by
// weight, so one delegate closes several file-disjoint clusters instead of one delegate per cluster — the
// unpacked form spawns a delegate per residual island and most do a few minutes of work.
const DRAIN_DELEGATES = 6;

// One row per phase. `territories` fan concurrently (each owns a disjoint page set and runs its own
// implement -> critique -> redteam chain). `barrier` runs to completion first. `serial` runs in order
// after the fan, for closers that share one governance surface. `light` territories skip the reviewer
// pair, for mechanical truth work a verify already covers.
const PHASE_ROWS = {
    6: {
        title: 'Campaign finalize',
        territories: [
            {
                key: 'conformance',
                kind: 'verify',
                light: true,
                pages: [
                    'libs/csharp/Rasm.AppHost/.planning/Observability/telemetry.md',
                    'libs/python/runtime/.planning/observability/telemetry.md',
                    'libs/typescript/core/.planning/observe/convention.md',
                    'libs/typescript/runtime/.planning/otel/emit.md',
                ],
                charter:
                    'Tier-0 [08] conformance, re-walked cell by cell across all three branches, at zero FAIL and zero PARTIAL. Prove ' +
                    'each cell by probing the owning fence on disk: the resource triple carrying service.namespace, the governance ' +
                    'row reading delta and base2_exponential, every exporter arm setting gzip, every get_meter / get_tracer / Mint ' +
                    'call site passing version and schema_url, the global propagator registration, the exemplar filter and ' +
                    'reservoir, the tenant view with its cardinality cap, and the absent-tenant arm. The semconv schema pin is the ' +
                    'SAME literal in all three branches. No metric name anywhere carries a unit suffix, and every UCUM unit rides ' +
                    'the descriptor rather than the name. Repair every failing cell at its owner. ' +
                    'Then attack the table ITSELF: a row Tier-0 asserts that no branch can satisfy is a false law, and a branch ' +
                    'capability no row governs is an ungoverned surface — both are findings, and both repair here.',
            },
            {
                key: 'parity',
                kind: 'verify',
                pages: [
                    'libs/csharp/Rasm/.planning/Domain/telemetry.md',
                    'libs/python/runtime/.planning/observability/metrics.md',
                    'libs/typescript/core/.planning/observe/board.md',
                ],
                charter:
                    'The MISSING-LOGIC lens, and the only delegate whose product is what no branch has. Build the cross-branch capability ' +
                    'census: for every observability, telemetry, metrics, hooks, logging, receipt, journal, worker, job, and analysis ' +
                    'concept the estate names, record which of the three branches realizes it, which declares it, and which carries ' +
                    'NOTHING. Read the owning pages in full in all three branches before ruling on any concept — a concept spelled ' +
                    'differently per branch reads as absent to a grep, and that is the failure mode this lens exists to beat. ' +
                    'Rank every gap by whether the branch NEEDS the concept: a browser runtime legitimately carries no process ' +
                    'profiler, and role-aware absence is parity rather than a hole. A real gap gets its capability LANDED at the ' +
                    'owning page in that branch, transcribed from whichever branch holds the reference form, in that branch idiom and ' +
                    'never as a copied fence. A gap you cannot land gets a card at its owning folder carrying the reference anchor. ' +
                    'Parity runs bidirectionally: where the weaker branch holds the better shape, the reference form moves the other way.',
            },
            {
                key: 'backends',
                kind: 'verify',
                pages: [
                    'libs/typescript/iac/.planning/operate/observe.md',
                    'libs/typescript/data/.planning/delegate/olap.md',
                    'libs/csharp/Rasm.Persistence/.planning/Query/columnar.md',
                    'libs/python/data/.planning/tabular/lakehouse.md',
                ],
                charter:
                    'FULL analysis-backend integration behind one backend family. The terminal property: selecting a different backend ' +
                    'changes VALUES, never SHAPE. Read .claude/scratch/observability/ALIGNMENT-FINDINGS.md in full — it carries the ' +
                    'fork census this lens closes, and READ ITS [07.1] FIRST — two entries in that file were themselves refuted on ' +
                    'disk, so treat every row there as a hypothesis you re-probe rather than a constraint you inherit. The rows still ' +
                    'standing: four disjoint residence rosters, a literal case switch that throws, alert rules bound to one datasource ' +
                    'regardless of the selected store, a store family and a residence family answering different column sets, and a ' +
                    'scalar-only cross-branch type map carrying none of the OTLP wide-event column shapes. ' +
                    'Every backend the estate names — prometheus, victoriametrics, mimir, clickhouse, the object-pdelegate lake, ' +
                    'timescaledb — is a ROW answering the same column set, deriving its dialect from its own row. Land the setup each ' +
                    'backend needs to WORK rather than merely to be named: schema, ingest door, read door, retention, tenancy, ' +
                    'translation strategy, degradation. A residence a query owner cannot address either gains its reach or declares ' +
                    'the gap structurally, and a pdelegate advertised as present while nothing plants it is the defect this lens refuses ' +
                    'to leave standing. ULTRA-STACK every owning catalog: clickhouse, prometheus, victoriametrics, duckdb and its ' +
                    'extensions, the Arrow and Flight surfaces, deltalake, pyiceberg, and the collector contract — a hand-rolled query ' +
                    'builder, retry, partition planner, or type map standing beside a catalogued member is a defect you rebuild on the ' +
                    'package.',
            },
            {
                key: 'strata',
                kind: 'verify',
                pages: [
                    'libs/csharp/.planning/ARCHITECTURE.md',
                    'libs/python/.planning/ARCHITECTURE.md',
                    'libs/typescript/.planning/ARCHITECTURE.md',
                ],
                charter:
                    'The NEVER-REINVENT-THE-WHEEL lens, run inside each branch against its own strata ladder. Every folder composes the ' +
                    'owner ABOVE it rather than re-deriving that owner locally: C# folders reach the Rasm kernel capsule for causal ' +
                    'frame, instrument spec, SLO vocabulary, receipt envelope, and board pack rather than minting twins — one such twin ' +
                    'already stood as a same-named uncompilable fork of the columnar custodian types; Python folders climb the runtime ' +
                    'ladder for workers, journal, receipts, metrics, and hooks rather than opening private delegates; TypeScript folders ' +
                    'reach core for convention, query, and board, and reach runtime for the emit and journal pdelegates. Census every ' +
                    'branch for a locally-minted type, rail, registry, or fold whose concept an upper stratum already owns, and ' +
                    'COLLAPSE it onto that owner. ' +
                    'Direction is law: a lower stratum never imports a higher one, an S2 peer never composes a sibling peer, and a ' +
                    'concept shared between peers homes at their common owner. Verify each collapse against the branch ARCHITECTURE ' +
                    'strata rows, and repair the row where the landed design is right and the row is stale. COUPLING is the twin ' +
                    'defect: a folder reaching sideways into sibling internals, or a page naming another project surface, collapses ' +
                    'onto a declared seam.',
            },
            {
                key: 'pdelegates',
                kind: 'verify',
                pages: [
                    'libs/python/runtime/.planning/observability/journal.md',
                    'libs/typescript/data/.planning/journal/fact.md',
                    'libs/csharp/Rasm.AppHost/.planning/Observability/hooks.md',
                ],
                charter:
                    'The evidence and execution pdelegates across all three branches: journal, worker, job, queue, scheduler, hook, receipt, ' +
                    'and log. Prove each pdelegate REAL end to end rather than declared — an append-only journal carries a port, an ' +
                    'implementer, a retention vocabulary, an erasure mechanism, and a tally read; a worker or job pdelegate carries an ' +
                    'admission gate, a bounded intake with stated back-pressure, a drain with a readiness handshake, a failure rail, ' +
                    'and a receipt; a hook pdelegate carries a closed point vocabulary and a stated veto-versus-observe contract. ' +
                    'Verify port and implementer agree MEMBER FOR MEMBER on signature, arity, rail shape, and return type in every ' +
                    'branch carrying both ends — a divergence is a fence-seam break repaired at both ends. ' +
                    'Hunt the defect class this campaign proved costliest: a value ASSERTED rather than READ, a counter forging zero, ' +
                    'an unbounded retry behind an admission proving names but not behavior, a concurrent consumer silently splitting a ' +
                    'stream, and float arithmetic anywhere near money or a settlement boundary. ' +
                    'The evidence-versus-series law holds in every branch: a missing metric point is a dashboard gap, a missing journal ' +
                    'row is an evidence or billing defect, and every derived pdelegate carries zero authority.',
            },
            {
                key: 'seams',
                kind: 'verify',
                light: true,
                pages: ['libs/csharp/Rasm/.planning/Domain/telemetry.md', 'libs/python/runtime/.planning/observability/metrics.md'],
                charter:
                    'The fence-seam sweep and the census gate, run corpus-wide rather than over an anchor list. Grep EVERY composing ' +
                    'fence in libs/csharp for InstrumentRow(, InstrumentSet.Of(, ReceiptFan.Of(, InstrumentSpec, AlertSeverity, ' +
                    'PanelKind, TraceCarrier, and ColumnType, and confirm each matches its kernel declaration on spelling, arity, and ' +
                    'rail shape — the boundary is zero occurrences of any retired form anywhere in the corpus. ' +
                    'Then the census equality probe from BOTH sides: the domain set in the Python INSTRUMENTS census equals the domain ' +
                    'set every producer records, and every rasm.* measure name resolves to a census row. Then every rasm.* domain ' +
                    'segment in all three branches resolves to the Tier-0 domain vocabulary. ' +
                    'Then the generated-dispatch sweep, which already caught a compile break and a runtime throw: every total switch ' +
                    'over a union that gained an arm still covers it, and every catch-all arm absorbing a NEW case earns an explicit ' +
                    'arm rather than silently misfiling it.',
            },
            {
                key: 'arms',
                kind: 'verify',
                light: true,
                pages: [
                    'libs/.planning/ARCHITECTURE.md',
                    'libs/typescript/iac/.planning/operate/observe.md',
                    'libs/typescript/iac/.planning/program/spec.md',
                ],
                charter:
                    'Arm resolution and endpoint resolution — the two checks that failed hardest at campaign start. Every Tier-0 ' +
                    '[FLEET_ESCALATION] and [PROFILE_SWAP] coordinate resolves to a named spec value or a landed row AT THE OWNER IT ' +
                    'NAMES: open each named owner and confirm the row exists there, since three of four arms named surfaces that did ' +
                    'not exist. Then every URL the iac pdelegate publishes derives from _urls and survives the Helm fullname collapse — ' +
                    'check each release name against its chart name and confirm the rendered service name, since that collapse is ' +
                    'exactly what made the collector endpoint dead. No endpoint is spelled outside _urls. ' +
                    'Then the provisioning-truth probe the datasource defect taught: every field a chart, provider, or driver actually ' +
                    'READS is the field the row supplies — a row spelling a key its consumer ignores provisions a door nothing connects ' +
                    'through, and only the provider contract proves which keys are live.',
            },
            {
                key: 'truth',
                kind: 'verify',
                light: true,
                pages: ['libs/csharp/.api/', 'libs/python/.api/', 'libs/typescript/runtime/.api/', 'libs/typescript/iac/.api/'],
                charter:
                    'Member and manifest truth, and the ULTRA-STACK audit. Every .api row this campaign added or corrected re-verifies ' +
                    'on its language rail, and every member any fence composes carries a catalog row at its owning tier. A member that ' +
                    'will not verify is DELETED and its consuming fence repaired, never left standing. ' +
                    'Then the stacking audit, which is the larger half: for every package a fence composes, read the catalog IN FULL ' +
                    'and find the capability the fence does NOT reach. Hand-rolled retries, backoffs, codecs, buffers, caches, ' +
                    'partition planners, type maps, statistical folds, and query builders standing beside a catalogued member are ' +
                    'defects — rebuild each on the package and record the collapse. Shallow use of a member whose surface carries a ' +
                    'family reads as UNDERUTILIZED and repairs the same way. ' +
                    'Then touch-point alignment in both directions: every composed package carries its central-manifest row, its folder ' +
                    'README registry row, and its owning .api catalog, with an orphan repaired at its owner rather than removed. No ' +
                    'dual-homed catalog pair survives, and no folder-tier file redirects to a substrate-tier catalogue. Re-confirm ' +
                    'every manifest version on its feed and unpin anything whose incompatibility has lapsed.',
            },
        ],
        serial: [
            {
                key: 'custody',
                pages: [
                    'libs/.planning/ARCHITECTURE.md',
                    'libs/.planning/RULINGS.md',
                    'docs/laws/topology.md',
                    'docs/laws/scars.md',
                    'tests/contracts/MANIFEST.md',
                    'tests/contracts/README.md',
                    'tests/RULINGS.md',
                    'libs/.planning/IDEAS.md',
                    'libs/.planning/TASKLOG.md',
                ],
                charter:
                    'The campaign custody close, and the pass leaving the governance estate TRUTHFUL rather than merely complete. ' +
                    'Dispatch the infra-custodian subagent over the touched infra set and IMPLEMENT its verdict rows yourself, treating ' +
                    'each as a signal you re-verify on disk rather than law. ' +
                    'TIER-0 ARCHITECTURE: [08] describes the conformance system, the analytics pdelegate, the backend family, and the hook ' +
                    'pdelegate exactly as they stand on disk — open the owning fences and re-derive every claim, because a row carried from ' +
                    'the campaign doc that the corpus refutes is the defect this stage exists to catch. Estate law and transcribed rows ' +
                    'only: a branch mechanism, a package name, or a folder detail at Tier-0 moves down. ' +
                    'RULINGS AT EVERY TIER: each row states a settled decision its own tier OWNS — a cross-branch decision at ' +
                    'libs/.planning, a branch nuance at that branch, a folder-only decision at that folder, a maintenance coupling or ' +
                    'regression scar at docs/laws. Delete any row that is fake, refuted by disk, or a restatement at another altitude. ' +
                    'SCARS: land the defect classes this campaign proved, refutation-first. The largest is that a RECORDED BLOCKER IS A ' +
                    'HYPOTHESIS, NEVER A CONSTRAINT — eight cards blocked on conditions already met, several naming members already ' +
                    'catalogued. Also land: a value asserted rather than read, a counter forging zero, a same-named type twin between ' +
                    'strata peers, and a provisioned row whose consumer ignores its keys. ' +
                    'THE PROOF ESTATE IS YOURS ALONE — no lens writes tests/, so the contract corpus closes here or nowhere. Read ' +
                    'tests/README.md and tests/RULINGS.md as binding law first. Every wire this campaign minted, renamed, widened, or ' +
                    'retired reconciles against tests/contracts/MANIFEST.md: an entry whose minters or consumers no longer resolve is ' +
                    'repaired at the entry, a wire crossing a branch boundary with no entry earns one under the README entry schema, ' +
                    'and a ledger row pointing at a dropped surface is re-anchored rather than deleted. Verify each minter and consumer ' +
                    'coordinate opens to a real symbol on disk before you let its row stand. Land the TELEMETRY_CONVENTION shape as the ' +
                    'conformance rows now stand rather than as the campaign doc described them, and record in tests/RULINGS.md what the ' +
                    'parity proof does and does not cover — role-aware absence is part of the shape, never an exception to it. ' +
                    'Read .claude/scratch/observability/RECEIPTS.md and land every row still OPEN at the owner it names, and read ' +
                    '.claude/scratch/observability/ALIGNMENT-FINDINGS.md whole — its [08] contested DDL ownership and its [02.1] SLO ' +
                    'renderer axis are the two adjudications every delegate deliberately escalated rather than decided, and both home ' +
                    'here because no narrower tier contains all their surfaces. ' +
                    'Keep index-tier entries under 150 columns and spend no pass auditing prose beyond that. ' +
                    'Confirm every card closes against landed work with none closed by deleting its history, and confirm ' +
                    'observability-initial-plan.md is gone from the repo root.',
            },
        ],
    },
};

// --- [INPUTS] --------------------------------------------------------------------------

const parsed = typeof args === 'string' && /^\s*[[{]/.test(args) ? JSON.parse(args) : args;
const rawPhase = parsed && typeof parsed === 'object' ? parsed.phase : parsed;
const PHASE = Number.isFinite(Number(rawPhase)) ? String(Number(rawPhase)) : '';
const ROW = PHASE_ROWS[PHASE] || null;

const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH = '.claude/scratch/observability-p' + (PHASE || '0') + '-' + fnv1a(JSON.stringify({ phase: PHASE }));

// --- [MODELS] --------------------------------------------------------------------------

const ANCHOR = {
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: { type: 'string' },
        line: { type: 'integer' },
        role: { type: 'string', enum: ['state', 'defect', 'ruling', 'catalog', 'counterpart', 'absence'] },
        note: { type: 'string' },
    },
};

const DOSSIER = {
    type: 'object',
    additionalProperties: false,
    required: ['facts', 'members', 'seams', 'coverage', 'summary'],
    properties: {
        facts: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['topic', 'statement', 'anchors'],
                properties: { topic: { type: 'string' }, statement: { type: 'string' }, anchors: { type: 'array', items: ANCHOR } },
            },
        },
        members: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['package', 'symbol', 'signature', 'tier', 'status', 'route'],
                properties: {
                    package: { type: 'string' },
                    symbol: { type: 'string' },
                    signature: { type: 'string' },
                    tier: { type: 'string' },
                    status: { type: 'string', enum: ['used', 'underutilized', 'unused', 'absent', 'unverified'] },
                    route: { type: 'string' },
                },
            },
        },
        seams: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['concept', 'ends', 'note'],
                properties: { concept: { type: 'string' }, ends: { type: 'array', items: { type: 'string' } }, note: { type: 'string' } },
            },
        },
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
        summary: { type: 'string' },
    },
};

const RECEIPT = {
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

// `class` is the drain's admission gate, schema-enforced so it cannot be argued around in prose: the
// drain works capability, truth, and seam rows and DROPS cosmetic ones. Classifying by enum beats
// pattern-matching claim text, which fractures per delegate and silently admits the next spelling.
const RESIDUAL = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['files', 'claim', 'owner', 'class'],
        properties: {
            files: { type: 'array', items: { type: 'string' } },
            claim: { type: 'string' },
            owner: { type: 'string' },
            class: { type: 'string', enum: ['capability', 'truth', 'seam', 'cosmetic'] },
        },
    },
};

const HARVEST = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['lesson', 'hardens', 'evidence'],
        properties: { lesson: { type: 'string' }, hardens: { type: 'string' }, evidence: { type: 'string' } },
    },
};

const FIXLOG = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'deltas', 'landed', 'beyond', 'residual', 'harvest', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        deltas: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['symbol', 'change'],
                properties: { symbol: { type: 'string' }, change: { type: 'string' } },
            },
        },
        landed: { type: 'array', items: { type: 'string' } },
        beyond: { type: 'array', items: { type: 'string' } },
        residual: RESIDUAL,
        harvest: HARVEST,
        summary: { type: 'string' },
    },
};

const CARRYIN = {
    type: 'object',
    additionalProperties: false,
    required: ['existed', 'rows'],
    properties: { existed: { type: 'boolean' }, rows: RESIDUAL },
};

const DRAINLOG = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'resolved', 'open', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        resolved: { type: 'array', items: { type: 'string' } },
        open: RESIDUAL,
        summary: { type: 'string' },
    },
};

const VERDICT = {
    type: 'object',
    additionalProperties: false,
    required: ['checks', 'repaired', 'blocked', 'summary'],
    properties: {
        checks: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['check', 'passed', 'evidence'],
                properties: { check: { type: 'string' }, passed: { type: 'boolean' }, evidence: { type: 'string' } },
            },
        },
        repaired: { type: 'array', items: { type: 'string' } },
        blocked: RESIDUAL,
        summary: { type: 'string' },
    },
};

// --- [DOCTRINE] ------------------------------------------------------------------------

const LAW_GROUND =
    'GROUND: this repo is in a planning phase — the artifact is the design corpus, and to implement is to author or deepen a ' +
    'markdown CODE FENCE inside a design page. No source tree lands. Read, at source and in full, before any edit: the campaign ' +
    'root doc ' +
    CAMPAIGN +
    ', the repo CLAUDE.md, libs/.planning/{ARCHITECTURE,RULINGS,campaign-method,README}.md, docs/laws/{scars,topology}.md, and the ' +
    'route-owned code doctrine under docs/stacks/<language>/ for every language you touch. For a design page under ' +
    'libs/<language>/, ALSO read every file in libs/<language>/.planning/ and both .api tiers that own the packages your fences ' +
    'compose — the shared libs/<language>/.api/ substrate and the folder <root>/.api/ tier. That doctrine read is never delegated ' +
    'and never ridden off a summary.';

const LAW_WRITE =
    'WRITER BAR: author ground-up, never patch. New capability weaves into the owning shape as if designed in from the start — a ' +
    'case, row, field, operation, or policy value INSIDE an existing owner before any new surface appears. No shims, aliases, ' +
    'migration layers, obsolete markers, or append-beside; the API breaks when the collapse improves the system. Capability is ' +
    'conserved absolutely: densify, never delete, and zero current consumers never lowers the bar. Assume 10x the complexity and ' +
    'demand on every surface — a naive, shallow, or surface-level form is rejected and rebuilt. Variation lives in input shape, ' +
    'policy values, and table rows behind one polymorphic entrypoint per concern; anything that can vary is parameterized, never ' +
    'hardcoded. Mine every admitted package to the operator depth it ships — a hand-rolled reimplementation of shipped capability ' +
    'is a defect. Naivety is intolerable on three axes: COVERAGE (a thin slice of a domain that carries far more), APPROACH ' +
    '(enumerated instances where a parameterized algorithmic owner generates the space), AUTHORITY (a profile, provider, receipt, ' +
    'or external package treated as the semantic owner).';

const LAW_MEMBER =
    'MEMBER TRUTH: every external member you write verifies first on its language rail — uv run python -m tools.assay api query ' +
    '--key <key> --symbol <Symbol> for C# host and NuGet surfaces, live reflection against ' +
    REPO +
    '/.venv for Python distributions, node_modules inspection plus the context7 MCP for TypeScript packages, and the nuget MCP ' +
    'for package existence and newest version. An unverifiable member is NEVER authored — it becomes a residual naming the ' +
    'catalog and the consuming fence. When a fence and its catalog disagree, resolve on the rail and correct the losing surface.';

const LAW_PROSE =
    'PROSE: declarative, assertive, present tense, active voice; every word load-bearing. No hedging, no future-gating, no meta ' +
    'commentary, no narration trails, no counts or version literals that go stale by construction, no emojis. Never open a ' +
    'sentence or a leader tail on an article. Tables repair in place at the 150-column cap. Prose is a system prompt for a cold ' +
    'agent reader — human-facing narration is a defect.';

const LAW_RIPPLE =
    'RIPPLE: fix every defect you find at its ROOT in the same pass, including defects outside your listed pages, EXCEPT where a ' +
    'live sibling territory in this same run owns the file — those become residuals. Your listed pages are where you look FIRST, ' +
    'never the bound on what you may fix. When you edit a page, land its counterpart obligations the same pass: the folder README ' +
    'router row, the folder ARCHITECTURE codemap and seam ledger at BOTH ends, the .api catalog row, and the central manifest row. ' +
    'A settled decision with no home lands as a RULINGS row at the narrowest owning tier; a new-owner or scope-expansion finding ' +
    'lands as a complete card at the narrowest tier following that file own template comment. docs/laws/topology.md binds ' +
    'counterpart obligations — consult it before any multi-surface edit. ' +
    'MINTING A NEW FILE escapes the page-disjointness your territory rests on, because a path that does not yet exist appears in ' +
    'no territory roster and two delegates can mint it concurrently, the later write erasing the earlier whole. Probe the path with ' +
    '`fd -H` before authoring; find a file already there and EXTEND it rather than replace it, whatever your own draft holds. ' +
    'SEARCH WITH `rg --hidden` for any corpus census: a plain directory rg skips every dot-directory, so `.planning/` and `.api/` ' +
    'return a silent false zero, and a negative conclusion drawn from one is unproven.';

const LAW_RESIDUAL =
    'RESIDUAL SHAPE: a residual is work you could not land because a LIVE sibling territory in this run owns the file, or because ' +
    'a member could not be verified. Every residual names the FULL file list it spans (so the drain can cluster by shared file), ' +
    'the claim as fact, the canonical owner, and its class. A residual is never a note-to-self and never a substitute for fixing ' +
    'something you can reach. ' +
    'CLASS decides whether the drain ever sees the row: `capability` is missing or wrong design capability, `truth` is a member, ' +
    'catalog, or manifest correctness defect, `seam` is a cross-file ownership or counterpart obligation — the drain works all ' +
    'three. `cosmetic` is table padding, column width, header or index numbering, section-marker spelling, and every other ' +
    'mechanical prose-gate finding; the drain DISCARDS it unread. Fix a cosmetic defect only in a file you already have open for ' +
    'a substantive reason, and otherwise leave it: it costs a later pass nothing to fix incidentally, and sweeping it across the ' +
    'corpus buries the campaign diff under noise nobody asked for. Classifying a formatting defect as `capability` to smuggle it ' +
    'past this gate is the defect the enum exists to foreclose.';

const LAW_HARVEST =
    'HARVEST: required but usually EMPTY. Nominate only a generalizable lesson — a reusable collapse pattern, an unnamed naivety ' +
    'class, a hard-won coupling, a review rule that would catch the defect class before review — each citing the exact existing ' +
    'clause it hardens or proving absence across the surfaces you searched. A stage-local fix NEVER nominates.';

const LAW_TIER =
    'TIER DISCIPLINE: specificity increases downward and a fact lives at exactly ONE altitude. Tier-0 ' +
    '`libs/.planning/ARCHITECTURE.md` states estate-spanning law and the conformance rows every branch transcribes — never a ' +
    'branch mechanism, never a package name, never a folder detail. `libs/.planning/RULINGS.md` settles cross-branch decisions ' +
    'alone. `docs/laws/` owns repo maintenance law — edit couplings, cross-branch patterns, regression scars — and never ' +
    'restates a design decision a RULINGS row owns. A branch `RULINGS.md` carries that branch system nuance; a folder ' +
    'ARCHITECTURE and RULINGS carry what only that folder decides. A fact appearing at two altitudes is SEDIMENT: delete the ' +
    'copy at the wider tier when the narrower one owns it, delete the narrower when the wider governs, and never leave both ' +
    'with a cross-reference standing in for the cut. A row restating what its own section lead already says is no-op content ' +
    'and is removed, not reworded.';

const LAW_SHAPE =
    'PROSE SHAPE: write durable prose as owner-voice law in the register the corpus already holds, carry one decision per list ' +
    'entry, and keep index-tier entries under 150 columns — a bullet running past that carries payload belonging one tier down, ' +
    'so demote the mechanism to its owner rather than lengthening the line or dropping the capability. ' +
    'Spend no pass hunting prose defects for their own sake: the corpus pattern and the docgen templates already carry the ' +
    'shape, and a page whose facts are right reads fine without a register audit.';

const LAW_SMELL =
    'FIX SMELLS AS YOU PASS, in every fence you open for any reason — a smell you read and leave standing is a smell you ' +
    'authored. Repair a value ASSERTED where the surface exposes a read; a counter or tally whose construction can only forge ' +
    'zero; a catch-all arm silently absorbing a case the union now names; an unbounded retry standing behind an admission that ' +
    'proves member names rather than member behavior; mutable accumulation where a fold, projection, or combinator belongs; ' +
    'exception control flow inside domain logic; imperative branching a bounded vocabulary, dispatch table, or match can own; ' +
    'float arithmetic anywhere near money, a settlement boundary, or an exact-decimal claim; a hardcoded literal or stringy call ' +
    'that belongs in a policy row; and a concurrent consumer that silently splits a stream instead of refusing the second reader. ' +
    'Collapse rather than patch: merge near-duplicate types, enums, and operations into one denser polymorphic owner inside the ' +
    'SAME fence, and never extract to a new file or delete capability to reduce lines. ' +
    'Delete a fence comment that narrates or restates its code, keeping any real constraint it carried. Chase no further comment ' +
    'or prose census beyond what the fences you edit put in front of you.';

const DOCTRINE =
    LAW_GROUND +
    '\n\n' +
    LAW_WRITE +
    '\n\n' +
    LAW_MEMBER +
    '\n\n' +
    LAW_PROSE +
    '\n\n' +
    LAW_TIER +
    '\n\n' +
    LAW_SHAPE +
    '\n\n' +
    LAW_SMELL +
    '\n\n' +
    LAW_RIPPLE;

// --- [OPERATIONS] ----------------------------------------------------------------------

const pagesOf = (t) => t.pages.join(', ');

const reconPrompt = (t) =>
    'ROLE: read-only mapping delegate for the observability campaign. You WRITE NOTHING except your own dossier file. ' +
    'Read ' +
    CAMPAIGN +
    ' IN FULL first — it is the campaign blueprint and names your territory charter. ' +
    'TERRITORY: ' +
    pagesOf(t) +
    '. CHARTER THIS TERRITORY WILL EXECUTE: ' +
    t.charter +
    ' YOUR JOB: read every territory page IN FULL — never a grep, never a skim, never the first N lines — plus the folder ' +
    'README/ARCHITECTURE/RULINGS that govern them, the branch ARCHITECTURE strata rows above them, the COUNTERPART pages in the ' +
    'other two branches, and BOTH .api tiers owning the packages their fences compose, each catalog read END TO END. ' +
    'Produce a consumer-scoped map for the writer that follows you, and cast the net three to four times wider than the charter ' +
    'literally names — the writer can discard a fact it does not need and can never recover one you skipped. ' +
    '(1) FACTS — current-state statements anchored to path and line, each carrying a topic label from this closed set so the ' +
    'writer can route them: STATE for the charter defects verified present or already absent; PARITY for a capability a sibling ' +
    'branch realizes that this territory lacks, or holds in a weaker shape, with the reference anchor named in that sibling; ' +
    'STRATA for a locally-minted type, rail, registry, or fold whose concept an upper stratum already owns, with that owner ' +
    'named; BACKEND for anything backend-specific leaking into a family surface, a family row answering a column its siblings do ' +
    'not, or a pdelegate advertised while nothing plants it; SMELL for a code smell in a fence — a value asserted rather than read, a ' +
    'counter that can only forge zero, a catch-all arm absorbing a new case, an unbounded retry, a mutable accumulation where a ' +
    'fold belongs, a stringy call, a hardcoded literal that should ride a policy row; SPAM for comment or prose bloat, meaning a ' +
    'comment restating its code, narration, a human-facing tour, a line a fresh agent regenerates from disk, or two lines ' +
    'carrying one rule. ' +
    '(2) MEMBERS — every package member the territory composes or plainly could, classified used / underutilized / unused / ' +
    'absent / unverified, each with its verified signature, its owning .api tier, and the route you verified it on; shallow usage ' +
    'of a member whose surface carries a family counts as UNDERUTILIZED, and a catalogued capability no fence reaches is the ' +
    'highest-value row you can return. (3) SEAMS — every cross-file and cross-territory endpoint this territory touches, both ' +
    'ends named, with any arity or spelling divergence between the two ends called out. ' +
    'LAW: your product carries INFORMATION, never prescriptions — anchored facts, verified spellings, seam endpoints, capability ' +
    'the concept admits but nothing exploits. An entry telling the writer what to build is this delegate defect. EMPTINESS IS NOT ' +
    'EVIDENCE: a probe returning nothing proves absence only after you re-run it in a second form; garbled output is your own ' +
    'tooling error, never a property of the territory. Record honest skips in coverage. ' +
    LAW_MEMBER;

const writePrompt = (t, dossierPath, priorNav) =>
    'ROLE: implementation writer owning ' +
    pagesOf(t) +
    ' in the observability campaign. ' +
    DOCTRINE +
    '\n\nCONSUMPTION LADDER, in this order and no other: (1) YOUR OWN BLIND PASS FIRST — open your territory pages and derive your ' +
    'own defect list, collapse targets, and design rulings from disk BEFORE opening any recon product; the majority of your diff ' +
    'comes from your own attack, and a diff that maps one-to-one onto the recon rows has failed this mandate. (2) THEN read the ' +
    'recon dossier at ' +
    dossierPath +
    ' IN FULL as grounding to verify and exceed — never a ceiling, never a settled question you relitigate. ' +
    (priorNav ? '(3) Navigation from an already-landed sibling stage, location facts only: ' + priorNav + '. ' : '') +
    '\n\nCHARTER: ' +
    t.charter +
    '\n\n' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST +
    '\n\nWrite every change in place with the Edit and Write tools. Report files touched, symbol deltas as data, what landed against ' +
    'the charter, what you fixed BEYOND the charter on your own authority, residuals, and harvest.';

const critiquePrompt = (t, dossierPath, nav) =>
    'ROLE: CRITIQUE reviewer with full writer authority over ' +
    pagesOf(t) +
    '. The pages were authored by ANOTHER engineer and are naive, shallow, or illusory until they survive a real attack; the ' +
    'burden of proof is on the work. Dense, confident, idiom-fluent output is the PRIME suspect for hollowness — a name promising ' +
    'capability the body omits, decorative density carrying nothing, a stub dressed as finished work. ' +
    DOCTRINE +
    '\n\nCOLD PASS FIRST: derive your own defect list from the pages on disk before consulting anything below. ' +
    'Then the recon dossier at ' +
    dossierPath +
    ' as grounding, and these location facts to reach touched territory fast: ' +
    nav +
    '\n\nYOUR OBJECTIVE is predicate-POSITIVE — the clause-by-clause conformance and capability-completeness audit, distinct from ' +
    'the red-team that follows you: DOCTRINE, every route-owned clause holds at its owning fence; SHAPE, the collapse, owner, knob, ' +
    'aspect, rail, language, and entry-point laws hold and repeated structure folds into its algebraic owner; CAPABILITY, bodies ' +
    'deliver every capability their names, cards, packages, and boundaries promise; TRUTH, both .api tiers support every external ' +
    'member and capability claim; OWNERSHIP, strata, seams, and rulings agree at every touched end. ' +
    'Cite the clause for every repair. Go BEYOND fixing: types, constants, and functions sharing a discriminant COLLAPSE into ' +
    'stronger owners, thin owners EXTEND to the full domain their concept carries, and a fundamentally stronger design once seen ' +
    'is BUILT. ' +
    'CHARTER THE WRITER WORKED TO: ' +
    t.charter +
    '\n\nNO CHURN: every edit names a violated law or invariant and the concrete case that breaks it. A clean verdict earned by an ' +
    'attack that finds nothing is a first-class result. ' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

const redteamPrompt = (t, nav, claims) =>
    'ROLE: RED-TEAM rebuilder with full writer authority over ' +
    pagesOf(t) +
    '. You are the terminal pre-mortem on this territory and you RECONSTRUCT rather than annotate. ' +
    DOCTRINE +
    '\n\nCOLD PASS FIRST: derive your own attack from the pages on disk. Location facts to reach territory fast: ' +
    nav +
    ' A prior reviewer filed these as landed — treat them strictly as UNVERIFIED CLAIMS to refute against the current pages, ' +
    'never as a settled record: ' +
    claims +
    '\n\nYOUR OBJECTIVE is predicate-NEGATIVE: COUNTERFACTUAL — remove the central owner, enumeration, dispatch, or hand-rolled ' +
    'kernel and land the stronger form the removal exposes. GROWTH — owner data absorbs the next case, dimension, modality, ' +
    'provider, or consumer, and the proof of a correct shape is the diff of the next feature: one declaration inside the owner, ' +
    'every consumer untouched or loudly broken. LONG TAIL — runtime edge states and failure modes (empty, singular, plural, ' +
    'stream, malformed, concurrent, cancelled, partial-failure) preserve the declared rails, parameterization, and boundaries. ' +
    'COMPOSITION — re-derive package choice, lower-stratum ownership, policy resolution, routing, lifecycle, and caller ' +
    'orchestration. INTEGRITY — repair downward dependency, duplicated ownership, host leakage, sibling-interior coupling, sprawl, ' +
    'and phantom members at every touched end. COLD CLOSE — re-judge every conformance dimension by name against the rebuilt ' +
    'result before your verdict. ' +
    'CHARTER: ' +
    t.charter +
    '\n\n' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

const finalizePrompt = (t, dossierPath, nav) =>
    'ROLE: campaign-terminal VERIFY delegate owning one lens over the WHOLE landed observability campaign. You are adversarial, never ' +
    'confirming, and you hold full WRITER authority — you repair what you disprove rather than reporting it. ' +
    'The campaign you are checking closed six cards as [COMPLETE] that a later audit refuted; assume this run did the same to ' +
    'itself and hunt accordingly. ' +
    DOCTRINE +
    '\n\nLENS: ' +
    t.charter +
    '\n\nMETHOD: run every probe as a REAL command against current disk, never from the campaign doc claims. Re-derive whether each ' +
    'claimed change was NECESSARY and prove on disk that it LANDED — a claim that does not reproduce is a finding. Status flips ' +
    'against evidence alone: a gap closes only against a cited .api line, a grep result, or harness output pasted as evidence. ' +
    'EMPTINESS IS NOT EVIDENCE — a probe returning nothing proves absence only after you re-run it in a second form; a malformed ' +
    'flag, wrong glob, ignored directory, or path typo returns exactly the silence a clean surface returns, and garbled output is ' +
    'your own tooling error, never a property of the territory. ' +
    'Where a check fails, repair at the ROOT: a single-point patch is itself the defect you repair wherever a denser ' +
    'reconstruction of the same files is available. ' +
    '\n\nTHE CHARTER IS A FLOOR, NEVER A CEILING. Your lens names where to look hardest; it never bounds what you fix. Every ' +
    'defect you meet while inside a file is yours — a parity gap, a strata violation, a backend leaking into a family surface, a ' +
    'code smell, comment or prose spam, a seam whose two ends disagree. Repair it rather than carding it wherever your fence ' +
    'reaches, and reserve a residual for work genuinely owned elsewhere. ' +
    'ULTRA-STACK as you repair: before writing any fence body, read the owning .api catalogs IN FULL and rebuild on the ' +
    'catalogued member rather than hand-rolling beside it — a retry, backoff, codec, buffer, cache, partition planner, type map, ' +
    'statistical fold, or query builder standing next to a package that ships it is a defect, and a catalogued capability no ' +
    'fence reaches is capability the estate paid for and never took. Land the catalog row for any new member in the same pass. ' +
    '\n\nGrounding, read AFTER your own cold probes have run: the recon dossier at ' +
    dossierPath +
    ', and these location facts from the phases that landed: ' +
    nav +
    '\n\nReport each probe you ran and its verdict in `landed` as `<check> :: PASS|FAIL :: <evidence>`, what you repaired in ' +
    '`files` and `beyond`, and anything genuinely unreachable as a residual. ' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

const serialPrompt = (t, landedNav) =>
    'ROLE: serial closer owning ' +
    pagesOf(t) +
    ' in the observability campaign. These are shared governance surfaces, so you run alone — no sibling is writing them. ' +
    DOCTRINE +
    '\n\nCHARTER: ' +
    t.charter +
    '\n\nEvery claim you land verifies against CURRENT disk — earlier stages in this run already changed the corpus, and a claim ' +
    'carried from the campaign doc that disk now refutes is corrected, not transcribed. Location facts from the stages that ran ' +
    'before you: ' +
    landedNav +
    '\n\n' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

// Stage-one route: a territory declares its kind, and the row picks the builder. A verify-kind
// territory carries a lens over the whole landed campaign, never a page set to author.
const STAGE_ONE = { write: writePrompt, verify: finalizePrompt };
const openPrompt = (t, dossierPath, nav) => (STAGE_ONE[t.kind] || writePrompt)(t, dossierPath, nav);

// Navigation crosses stages as a STABLE STRING, never a nested object. Cache keys hash prompt text, and
// a resume rebuilds a prior result by parsing the journal — that copy never re-serializes byte-identically
// to the live validated one, so a stringified object misses cache and re-runs a completed stage.
const navOf = (r) => {
    const uniq = (xs) => [...new Set((xs || []).filter(Boolean))].sort();
    const files = uniq(r && r.files);
    const deltas = uniq(((r && r.deltas) || []).map((d) => (d && d.symbol ? d.symbol + '~' + (d.change || '') : '')));
    const beyond = uniq(r && r.beyond);
    return (
        'FILES: ' + (files.join(' | ') || 'none') + ' // DELTAS: ' + (deltas.join(' | ') || 'none') + ' // BEYOND: ' + (beyond.join(' | ') || 'none')
    );
};
const navGroup = (rows) => rows.map((x) => x.key + ' >> ' + navOf(x)).join('  ///  ') || 'none';

const delegate = (t, promptText) =>
    agent(
        promptText +
            '\n\nPRODUCT TO DISK: write your COMPLETE dossier as one JSON file matching this schema at ' +
            REPO +
            '/' +
            SCRATCH +
            '/' +
            t.key +
            '-recon-dossier.json (Write tool, absolute path; delete any prior file at that path first). Schema: ' +
            JSON.stringify(DOSSIER) +
            ' — then return ONLY the receipt: ok, the report path, the entries count, a one-line mechanical headline, failure empty.',
        { label: 'recon:' + t.key, phase: 'Recon', effort: 'high', schema: RECEIPT },
    ).then((r) => ({
        key: t.key,
        pages: t.pages,
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'delegate died'),
    }));

const clusterByFile = (rows) => {
    const parent = rows.map((_, i) => i);
    const find = (i) => (parent[i] === i ? i : (parent[i] = find(parent[i])));
    const union = (a, b) => {
        const ra = find(a);
        const rb = find(b);
        if (ra !== rb) parent[ra] = rb;
    };
    const byFile = new Map();
    rows.forEach((r, i) => {
        (r.files || []).forEach((f) => {
            if (byFile.has(f)) union(byFile.get(f), i);
            else byFile.set(f, i);
        });
    });
    const groups = new Map();
    rows.forEach((_, i) => {
        const k = find(i);
        if (!groups.has(k)) groups.set(k, []);
        groups.get(k).push(rows[i]);
    });
    return [...groups.values()];
};

const dedupe = (rows) => [...new Map(rows.map((r) => [(r.files || []).slice().sort().join(',') + '|' + r.claim, r])).values()];

// Cost of one cluster: its rows plus the distinct files those rows span. Balancing on row COUNT alone
// pairs a one-row-fifty-file cluster with a fifty-row-one-file cluster and recreates the long pole.
const weigh = (cl) => cl.length + new Set(cl.flatMap((r) => r.files || [])).size;
const clusterKey = (cl) =>
    cl
        .map((r) => (r.files || []).slice().sort().join(',') + '|' + r.claim)
        .sort()
        .join('~');

// Longest-processing-time greedy: heaviest cluster first into the lightest delegate. Clusters are
// file-disjoint by construction, so any packing of them stays disjoint across delegates and the delegates
// still write concurrently without collision. Ties break on a content key, never on iteration order,
// so the packing is identical on a resume.
const pack = (clusters, delegates) => {
    const bins = Array.from({ length: Math.max(1, Math.min(delegates, clusters.length)) }, () => ({ weight: 0, clusters: [] }));
    clusters
        .slice()
        .sort((a, b) => weigh(b) - weigh(a) || clusterKey(a).localeCompare(clusterKey(b)))
        .forEach((cl) => {
            const lightest = bins.reduce((min, b) => (b.weight < min.weight ? b : min), bins[0]);
            lightest.clusters.push(cl);
            lightest.weight += weigh(cl);
        });
    return bins.filter((b) => b.clusters.length);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

if (!ROW) {
    log('no phase — pass {phase: 1..5}; available: ' + Object.keys(PHASE_ROWS).join(', '));
    return { skipped: true, reason: 'no phase argument', available: Object.keys(PHASE_ROWS) };
}

log('phase ' + PHASE + ' — ' + ROW.title + ' · ' + ROW.territories.length + ' territories · scratch ' + SCRATCH);

// --- [RECON]

phase('Recon');
const barrierDelegates = ROW.barrier ? (ROW.barrier.parallel ? ROW.barrier.parallel : [ROW.barrier]) : [];
const reconTargets = barrierDelegates.concat(ROW.territories);
const dossiers = (await parallel(reconTargets.map((t) => () => delegate(t, reconPrompt(t))))).filter(Boolean);
const dossierOf = (key) => {
    const d = dossiers.find((x) => x.key === key);
    return d && d.ok ? d.report : '(no dossier — this delegate failed; do your own cold read of every territory page first)';
};
log(dossiers.filter((d) => d.ok).length + '/' + dossiers.length + ' recon delegates landed');

// --- [KERNEL]

let barrierLogs = [];
if (barrierDelegates.length) {
    phase('Kernel');
    barrierLogs = (
        await parallel(
            barrierDelegates.map(
                (b) => () =>
                    agent(writePrompt(b, dossierOf(b.key), null), {
                        label: 'kernel:' + b.key,
                        phase: 'Kernel',
                        effort: 'high',
                        schema: FIXLOG,
                    }).then((r) => ({ key: b.key, ...(r || {}) })),
            ),
        )
    ).filter(Boolean);
    log('barrier landed: ' + barrierLogs.map((b) => b.key + '(' + (b.files || []).length + ' files)').join(' · '));
}
const barrierNav = barrierLogs.length ? navGroup(barrierLogs) : null;

// --- [IMPLEMENT]

const chains = (
    await pipeline(
        ROW.territories,
        (t) =>
            agent(openPrompt(t, dossierOf(t.key), barrierNav), {
                label: (t.kind === 'verify' ? 'lens:' : 'write:') + t.key,
                phase: 'Implement',
                effort: 'high',
                schema: FIXLOG,
            }),
        (fix, t) =>
            t.light
                ? { fix, crit: null }
                : agent(critiquePrompt(t, dossierOf(t.key), navOf(fix)), {
                      label: 'critique:' + t.key,
                      phase: 'Critique',
                      effort: 'high',
                      schema: FIXLOG,
                  }).then((crit) => ({ fix, crit })),
        (prev, t) =>
            t.light
                ? { key: t.key, stages: [prev.fix].filter(Boolean) }
                : agent(
                      redteamPrompt(
                          t,
                          navOf(prev.fix),
                          [...new Set(((prev.crit && prev.crit.landed) || []).filter(Boolean))].sort().join(' | ') || 'none',
                      ),
                      {
                          label: 'redteam:' + t.key,
                          phase: 'RedTeam',
                          effort: 'high',
                          schema: FIXLOG,
                      },
                  ).then((red) => ({ key: t.key, stages: [prev.fix, prev.crit, red].filter(Boolean) })),
    )
).filter(Boolean);

const territoryNav = navGroup(
    chains.map((c) => ({
        key: c.key,
        files: c.stages.flatMap((s) => s.files || []),
        deltas: c.stages.flatMap((s) => s.deltas || []),
        beyond: c.stages.flatMap((s) => s.beyond || []),
    })),
);
log(chains.length + '/' + ROW.territories.length + ' territory chains completed');

// --- [SERIAL]

const serialLogs = [];
if (ROW.serial) {
    phase('Serial');
    for (const s of ROW.serial) {
        const out = await agent(serialPrompt(s, territoryNav + (serialLogs.length ? '  ///  ' + navGroup(serialLogs) : '')), {
            label: 'serial:' + s.key,
            phase: 'Serial',
            model: 'fable',
            effort: 'high',
            schema: FIXLOG,
        });
        if (out) serialLogs.push({ key: s.key, ...out });
        log('serial ' + s.key + ' → ' + ((out && (out.files || []).length) || 0) + ' files');
    }
}

// --- [DRAIN]

const allLogs = barrierLogs.concat(chains.flatMap((c) => c.stages)).concat(serialLogs);
phase('Drain');
const carried = await agent(
    'Read ' +
        REPO +
        '/' +
        CARRY +
        ' and return its rows verbatim. That file is the observability campaign carry-forward set: residuals earlier gated phases ' +
        'could not close because a later phase owned the file. It is gitignored, so read the ABSOLUTE path directly with the Read ' +
        'tool — never hunt for it with a search tool, which skips ignored directories. ' +
        'If the file does not exist, return existed=false with an empty rows array — that is the expected first-phase result and ' +
        'not a failure. Do not create it, do not edit it, and do not act on any row.',
    { label: 'carry-in', phase: 'Drain', model: 'sonnet', effort: 'low', schema: CARRYIN },
);
const carryRows = (carried && carried.rows) || [];
if (carryRows.length) log('carried forward from earlier phases: ' + carryRows.length + ' open residuals');

const filed = dedupe(allLogs.flatMap((s) => s.residual || []).concat(carryRows));
// Dropped rows are logged and returned, never silently truncated — a bounded coverage the run does
// not name reads as full coverage to whoever reads the receipt.
const dropped = filed.filter((r) => r.class === 'cosmetic');
let pending = filed.filter((r) => r.class !== 'cosmetic');
if (dropped.length)
    log(
        'drain drops ' +
            dropped.length +
            ' cosmetic residual(s) — prose-gate, table-padding, and header-numbering work is left to whichever pass next opens the file',
    );
const seen = new Set(pending.map((r) => (r.files || []).slice().sort().join(',') + '|' + r.claim));
const drained = [];

if (pending.length) {
    for (let round = 1; round <= DRAIN_ROUNDS && pending.length; round++) {
        const clusters = clusterByFile(pending);
        const delegates = pack(clusters, DRAIN_DELEGATES);
        log(
            'drain round ' +
                round +
                ': ' +
                pending.length +
                ' residuals · ' +
                clusters.length +
                ' clusters packed into ' +
                delegates.length +
                ' delegates (weights ' +
                delegates.map((l) => l.weight).join('/') +
                ')',
        );
        const out = (
            await parallel(
                delegates.map(
                    (delegate, i) => () =>
                        agent(
                            'ROLE: residual closer for the observability campaign. Every row below was deferred by a writer because a ' +
                                'live sibling owned the file or a member could not be verified; that sibling has now landed, so the ' +
                                'territory is yours. ' +
                                DOCTRINE +
                                '\n\nYou hold ' +
                                delegate.clusters.length +
                                ' INDEPENDENT residual clusters, delivered as an array of arrays. Every cluster is file-disjoint from ' +
                                'every other cluster in your delegate AND from every cluster held by a concurrent sibling delegate, so no ' +
                                'sibling is writing any file you touch. Work them one cluster at a time, closing each fully before ' +
                                'opening the next — a cluster is the coherence boundary, and interleaving them loses the shared-file ' +
                                'context that made them one cluster. Report one merged result across every cluster you held.' +
                                '\n\nRe-verify EVERY row against current disk FIRST and cull with proof any row a later stage already ' +
                                'resolved — a stale residual is the common case, not the exception. Fix the rest at their ROOT, reading ' +
                                'every listed file in full. Report only rows you could NOT close, each naming its blocker and owner.' +
                                '\n\nSCOPE: cosmetic defects were filtered out before you were dispatched and are NOT your work. A ' +
                                'mechanical prose-gate finding you notice mid-fix — table padding, column width, header or index ' +
                                'numbering, marker spelling — is corrected only inside a file you already opened to close a ' +
                                'substantive row, and never pursued into a file outside these clusters. A corpus-wide sweep is ' +
                                'reserved for draining a surface THIS campaign retired, where zero remaining occurrences is the ' +
                                'proof; it is never licensed by formatting drift that predates the campaign.' +
                                '\n\nCLUSTERS: ' +
                                JSON.stringify(delegate.clusters),
                            { label: 'drain:' + round + ':' + i, phase: 'Drain', effort: 'high', schema: DRAINLOG },
                        ),
                ),
            )
        ).filter(Boolean);
        const changed = out.some((o) => (o.files || []).length);
        drained.push(...out.flatMap((o) => o.resolved || []));
        const next = [];
        for (const o of out)
            for (const r of o.open || []) {
                const k = (r.files || []).slice().sort().join(',') + '|' + r.claim;
                if (!seen.has(k)) {
                    seen.add(k);
                    next.push(r);
                }
            }
        if (!changed) {
            pending = next;
            log('drain round ' + round + ' changed no file — stopping');
            break;
        }
        pending = next;
    }
}

// --- [VERIFY]

phase('Verify');
const verdict = await agent(
    'ROLE: terminal VERIFY delegate for phase ' +
        PHASE +
        ' (' +
        ROW.title +
        ') of the observability campaign. You are adversarial, never confirming, and you WRITE the improvement your proof exposes. ' +
        DOCTRINE +
        '\n\nRun the checks in section [06]-[VERIFICATION] of ' +
        CAMPAIGN +
        ' that apply to THIS phase, on disk, with real commands — the fence-seam greps, the census equality probe, the conformance ' +
        'table re-walk, the arm-resolution check, the endpoint-resolution check, and uv run python -m tools.assay docs check over ' +
        'every touched page. Re-derive whether each claimed change was NECESSARY and prove on disk that it LANDED; a claim that does ' +
        'not reproduce is a finding. Where a check fails, REPAIR it at the root rather than reporting it — a single-point patch is ' +
        'itself the defect you repair wherever a denser reconstruction of the same files is available. Status flips against evidence ' +
        'alone: a gap marks closed only against a cited .api line or harness output. ' +
        'EMPTINESS IS NOT EVIDENCE — re-run any probe that returns nothing in a second form before concluding absence, and read a ' +
        'garbled result as your own tooling error. ' +
        '\n\nPAGES THIS PHASE TOUCHED: ' +
        JSON.stringify([...new Set(allLogs.flatMap((s) => s.files || []))]) +
        '\n\nCLAIMS the stages filed as landed. You held none of these positions. Treat every one as an UNVERIFIED claim to refute ' +
        'against the current pages — a claim that does not reproduce on disk is this phase primary finding, and the campaign exists ' +
        'because six such claims stood as [COMPLETE] against a corpus that refuted them: ' +
        JSON.stringify(allLogs.flatMap((s) => (s.landed || []).map((c) => ({ from: s.key || '', claim: c })))) +
        '\n\nRESIDUALS STILL OPEN after the drain — verify each is genuinely blocked rather than merely deferred, and close every one ' +
        'you can reach: ' +
        JSON.stringify(pending) +
        '\n\nLEDGER, required: append one `## [NN]-[PHASE_' +
        PHASE +
        ']` section to ' +
        REPO +
        '/.claude/scratch/observability/OPEN-LEDGER.md in the exact shape the existing phase-1 sections use — a check table with ' +
        'evidence, owner, and state, then a carried-residual table. Before appending, walk every OPEN row already in that file and ' +
        'update its state against current disk: a row this phase closed flips to CLOSED with the probe result or page anchor that ' +
        'proves it, and a row still open keeps its state and gains nothing. Never delete a row and never soften one to close it — ' +
        'the file is the terminal audit that proves nothing was missed between phases, so a row silently dropped defeats its whole ' +
        'purpose. A probe that produced nothing gets recorded as such; an absent row reads as an unrun check, never a clean one.' +
        '\n\nCARRY-FORWARD, your last act and NOT optional: write the rows you could not close to ' +
        REPO +
        '/' +
        CARRY +
        ' as a JSON object {"existed": true, "rows": [...]} matching ' +
        JSON.stringify(CARRYIN) +
        ' — each row carrying the FULL file list it spans, the claim as fact, and the owner. This file is how a residual survives ' +
        'the gated boundary to the phase that owns its files; a row you drop here is lost from the campaign entirely. Write the file ' +
        'even when the set is empty, with rows as an empty array, so the next phase reads a true state rather than a stale one. ' +
        (PHASE === '6'
            ? 'THIS IS THE TERMINAL PHASE: a non-empty set here is a campaign-level failure, not a handoff — say so plainly in your ' +
              'summary and name each blocker and its owner, because no later phase exists to receive it.'
            : 'Rows whose files a LATER phase owns are correct to carry; rows you merely found inconvenient are not.'),
    { label: 'verify:p' + PHASE, phase: 'Verify', effort: 'high', schema: VERDICT },
);

const harvest = dedupe(allLogs.flatMap((s) => s.harvest || []).map((h) => ({ files: [h.hardens], claim: h.lesson, owner: h.evidence }))).map((h) => ({
    lesson: h.claim,
    hardens: h.files[0],
    evidence: h.owner,
}));

return {
    phase: Number(PHASE),
    title: ROW.title,
    scratch: SCRATCH,
    recon: { landed: dossiers.filter((d) => d.ok).length, failed: dossiers.filter((d) => !d.ok).map((d) => d.key) },
    barrier: barrierLogs.map((b) => ({ key: b.key, files: (b.files || []).length, summary: b.summary })),
    territories: chains.map((c) => ({
        key: c.key,
        stages: c.stages.length,
        files: [...new Set(c.stages.flatMap((s) => s.files || []))],
        beyond: c.stages.flatMap((s) => s.beyond || []),
    })),
    serial: serialLogs.map((s) => ({ key: s.key, files: (s.files || []).length, summary: s.summary })),
    drain: { resolved: drained.length, open: pending, droppedCosmetic: dropped },
    verify: verdict,
    harvest,
};
