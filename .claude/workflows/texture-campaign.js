export const meta = {
    name: 'texture-campaign',
    description:
        'Execute one phase of the Materials texture-generation campaign against .claude/scratch/texture-campaign/CAMPAIGN.md. args = {phase: 0..6}, bare number accepted, empty = no-op. Phase 0 runs the Forge build probes, seven codex recon dossiers, and the wire-roster freeze; phase 1 the three single-writer package-admission gates; phases 2-4 the C#, python, and TS design waves as implement(opus) -> critique(codex) -> redteam(opus) territory chains; phase 5 the cross-tier single writer; phase 6 the close (docgen zero, codex cold-verify, residual drain, custodian dispatch).',
    whenToUse: 'Landing the Materials texture campaign phase by phase.',
    phases: [
        { title: 'Probe', detail: 'native build-probe lanes: sdist builds at the 3.15 floor, Forge rows + redeploy, provision evidence' },
        { title: 'Recon', detail: 'codex read lanes; dossier to campaign home, thin receipt on the wire' },
        { title: 'Freeze', detail: 'wire field rosters frozen as the PH0 dossier artifact binding phases 2-5' },
        { title: 'Write', detail: 'implementation writers, one per territory — fable in territory chains, opus on central manifests' },
        { title: 'Critique', detail: 'native predicate-positive conformance audit repaired in place', model: 'opus' },
        { title: 'RedTeam', detail: 'opus predicate-negative pre-mortem, rebuilt in place' },
        { title: 'Gate', detail: 'phase acceptance: restore/lock/install, assay static, docgen, verdict rows' },
        { title: 'Drain', detail: 'pooled residual carry merge; phase-6 fixpoint drain and custodian dispatch' },
    ],
};

// --- [CONSTANTS] ------------------------------------------------------------------------

const REPO = '/Users/bardiasamiee/Documents/99.Github/Rasm';
const FORGE = '/Users/bardiasamiee/Documents/99.Github/Parametric_Forge';
const HOME = '.claude/scratch/texture-campaign';
const CAMPAIGN = HOME + '/CAMPAIGN.md';
// Cross-phase artifacts live in the durable campaign home, never per-instance run scratch: phase N
// resolves phase N-1 products by stable path across separate Workflow invocations.
const FREEZE = HOME + '/wire-freeze.md';
const PROBES_CORE = HOME + '/probes-core.json';
const PROBES_FORGE = HOME + '/probes-forge.json';
const DOSSIERS = HOME + '/dossiers';
const CARRY = HOME + '/residuals-open.json';
const PROBE_PLAN = '/Users/bardiasamiee/.claude/plans/i-want-you-to-pure-journal-agent-ac917727000dbabe7.md';
const LANE_SH = '.claude/skills/codex/scripts/codex-lane.sh';
const CODEX_MODEL = 'gpt-5.6-terra';
const DRAIN_ROUNDS = 3;

// --- [INPUTS] ---------------------------------------------------------------------------

const parsed = typeof args === 'string' && /^\s*[[{]/.test(args) ? JSON.parse(args) : args;
const rawPhase = parsed && typeof parsed === 'object' ? parsed.phase : parsed;
const PHASE = Number.isFinite(Number(rawPhase)) ? String(Number(rawPhase)) : '';
const BASE = (parsed && typeof parsed === 'object' && parsed.base) || '';

const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH = HOME + '/run-p' + (PHASE || 'x') + '-' + fnv1a(JSON.stringify({ phase: PHASE, base: BASE }));

// --- [MODELS] ---------------------------------------------------------------------------

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

// --- [DOCTRINE] -------------------------------------------------------------------------

const LAW_GROUND =
    'GROUND: this repo is in a planning phase — the artifact is the design corpus, and to implement is to author or deepen a ' +
    'markdown CODE FENCE inside a design page. No source tree lands. Read, at source and in full, before any edit: the campaign ' +
    'root doc ' +
    CAMPAIGN +
    ' (its [ADOPTED_AMENDMENTS] SUPERSEDE any conflicting earlier line), the frozen wire rosters at ' +
    FREEZE +
    ' (binding law for every wire, channel, transfer, and vocabulary fence you touch), the repo CLAUDE.md, ' +
    'libs/.planning/{ARCHITECTURE,RULINGS,campaign-method,README}.md, docs/laws/{scars,topology}.md, and the route-owned code ' +
    'doctrine under docs/stacks/<language>/ for every language you touch. For a design page under libs/<language>/, ALSO read ' +
    'every file in libs/<language>/.planning/ and both .api tiers that own the packages your fences compose — the shared ' +
    'libs/<language>/.api/ substrate and the folder <root>/.api/ tier. That doctrine read is never delegated and never ridden ' +
    'off a summary.';

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
    'MINTING A NEW FILE: probe the path with `fd -H` before authoring; find a file already there and EXTEND it rather than ' +
    'replace it, whatever your own draft holds. SEARCH WITH `rg --hidden` for any corpus census: a plain rg skips dot-directories, ' +
    'so `.planning/` and `.api/` return a silent false zero, and a negative conclusion drawn from one is unproven.';

const LAW_RESIDUAL =
    'RESIDUAL SHAPE: a residual is work you could not land because a LIVE sibling territory in this run owns the file, or because ' +
    'a member could not be verified. Every residual names the FULL file list it spans, the claim as fact, the canonical owner, and ' +
    'its class: `capability` (missing or wrong design capability), `truth` (member, catalog, or manifest correctness), `seam` ' +
    '(cross-file ownership or counterpart obligation) — all three drain later. `cosmetic` (padding, numbering, marker spelling) is ' +
    'DISCARDED unread — fix one only in a file already open for a substantive reason, and never class a formatting defect as ' +
    'capability to smuggle it past this gate.';

const LAW_HARVEST =
    'HARVEST: required but usually EMPTY. Nominate only a generalizable lesson — a reusable collapse pattern, an unnamed naivety ' +
    'class, a hard-won coupling, a review rule that would catch the defect class before review — each citing the exact existing ' +
    'clause it hardens or proving absence across the surfaces you searched. A stage-local fix NEVER nominates.';

const DOCTRINE = LAW_GROUND + '\n\n' + LAW_WRITE + '\n\n' + LAW_MEMBER + '\n\n' + LAW_PROSE + '\n\n' + LAW_RIPPLE;

// Codex lanes take the same demands de-conflicted and neutral: intensifiers that sharpen a native lane
// make a codex lane over-probe out of territory.
const CODEX_READ_LAW =
    '<context_gathering>\nTerritory: the exact files and directories the task names, plus their sibling README, ARCHITECTURE, and ' +
    'RULINGS files and the .api catalogs the task names. Do not open files outside it, and do not open .claude/ skill or ' +
    'instruction files other than the campaign doc the task names.\nBudget: at most 80 tool calls. Read in small batches; never ' +
    'concatenate whole directories into one command — tool output truncates and the data is lost.\nStop when the product is ' +
    'complete; residual uncertainty becomes an entry in coverage.unverified, never a re-read.\n</context_gathering>\n\n' +
    '<product>\nA consumer-scoped map for a later writer: information, never prescriptions. Anchored current-state facts, ' +
    'verified member spellings with signatures, seam endpoints with both ends named, capability the concept admits but nothing ' +
    'exploits. An entry telling the writer what to build is a defect. A probe returning nothing proves absence only after a ' +
    'second differently-shaped probe agrees.\n</product>\n\n<output_contract>\nYour final message is a single JSON object with ' +
    'exactly these keys: "facts" (array of {topic, statement, anchors: [{path, line, role, note}]}), "members" (array of ' +
    '{package, symbol, signature, tier, status: "used"|"underutilized"|"unused"|"absent"|"unverified", route}), "seams" (array ' +
    'of {concept, ends: [string], note}), "coverage" ({requested, read, skipped, unverified} — arrays of paths), "summary" ' +
    '(string). JSON only: no prose around it, no code fences. Use [] for empty lists; never guess.\n</output_contract>';

// --- [OPERATIONS] -----------------------------------------------------------------------

const pagesOf = (t) => t.pages.join(', ');
const shapeReceipt = (label) => (r) => ({
    key: label,
    ok: !!(r && r.ok && r.report),
    report: (r && r.report) || '',
    entries: (r && r.entries) || 0,
    headline: (r && r.headline) || '',
    failure: (r && r.failure) || (r ? '' : 'lane died'),
});

// Codex read lane: wrapper writes law/task, one backgrounded lane run, --out materializes the dossier.
// Quota-dead lanes re-dispatch natively at opus so the report path is filled either way.
const codexRead = (key, task, report, phaseTitle) =>
    agent(
        'DISPATCH ROLE: codex performs the complete TASK below through one supervised lane run; never perform, edit, judge, or ' +
            'relay the work yourself. (1) Write the LANE LAW block below VERBATIM to ' +
            report +
            '.lane/law.md and the TASK block below VERBATIM to ' +
            report +
            '.lane/task.md, composing neither. Delete any leftover report with one Bash call: rm -f ' +
            report +
            " — a stale file from a prior run otherwise passes step (3) as this run's product. (2) Run ONE Bash call with run_in_background true: " +
            LANE_SH +
            ' --task ' +
            report +
            '.lane/task.md --law ' +
            report +
            '.lane/law.md --dir ' +
            report +
            '.lane --cwd ' +
            REPO +
            ' --model ' +
            CODEX_MODEL +
            ' --out ' +
            report +
            '; then WAIT for it: call TaskOutput on the returned task id with block=true and timeout 600000, and REPEAT that ' +
            'blocking call until the task reports completed — NEVER end your turn while the lane runs, and never poll with ' +
            'sleep. When it completes, Read ' +
            report +
            '.lane/receipt.json. Recovery is ONCE-only: a receipt reason "crash" overwrites the task ' +
            'file with "continue and complete the lane, then land the receipt" and re-runs the same command plus --resume <the ' +
            'receipt thread_id>; any other failed receipt re-runs the same command untouched. (3) Verify with one Bash call: ' +
            'jq -e ".facts" ' +
            report +
            ' >/dev/null — the contract key, never bare parseability; on a miss rewrite once from the last agent_message item ' +
            'text in ' +
            report +
            '.lane/events.jsonl (jq -rs, Write that) and re-probe, then return ok=false after a second miss. (4) Return ok, the ' +
            'report path, entries = facts+members+seams counts summed, headline = one line, failure empty — or ok=false with the ' +
            'receipt reason and failure text VERBATIM.\n\nLANE LAW:\n\n' +
            CODEX_READ_LAW +
            '\n\nTASK:\n\n' +
            task,
        { label: 'codex:' + key, phase: phaseTitle, model: 'sonnet', effort: 'low', schema: RECEIPT },
    )
        .then((r) =>
            r && !r.ok && /usage|quota|limit/i.test(r.failure || '')
                ? agent(
                      task +
                          '\n\nYou are the native fallback for a dead codex lane. Produce the SAME product: write the complete ' +
                          'dossier JSON (keys facts, members, seams, coverage, summary) to ' +
                          report +
                          ' with the Write tool, then return ONLY the thin receipt.',
                      { label: 'recon:' + key, phase: phaseTitle, model: 'opus', effort: 'medium', schema: RECEIPT },
                  )
                : r,
        )
        .then(shapeReceipt(key));

// Critique runs NATIVE at opus: the codex write-lane wrapper proved flaky inside workflow subagents
// (early returns before the backgrounded lane finished), so the chain is fable(write) -> opus(critique)
// -> opus(redteam). The fixlog still lands on disk — the redteam's read contract is unchanged.
const critique = (t, fixlogPath) =>
    agent(
        'ROLE: CRITIQUE reviewer with full writer authority over ' +
            pagesOf(t) +
            '. The pages were authored by ANOTHER engineer and are naive, shallow, or illusory until they survive a real attack; ' +
            'the burden of proof is on the work, and dense, confident, idiom-fluent output is the PRIME suspect for hollowness. ' +
            DOCTRINE +
            '\n\nCOLD PASS FIRST: derive your own defect list from the pages on disk before consulting anything else. Your ' +
            'objective is predicate-POSITIVE — the clause-by-clause conformance and capability-completeness audit: every required ' +
            'law of the campaign doc and wire-freeze rosters holds on these pages; fences meet the language doctrine under ' +
            'docs/stacks/; every external member cited verifies against its .api catalog (an unverifiable member is repaired or ' +
            'becomes a residual, never left standing); names, arities, and vocabularies agree with the frozen rosters; index docs ' +
            'and counterpart obligations landed. Repair every miss IN PLACE and cite the clause. Go BEYOND fixing: types and ' +
            'operations sharing a discriminant COLLAPSE into stronger owners, thin owners EXTEND to their full domain, and a ' +
            'fundamentally stronger design once seen is BUILT. NO CHURN: every edit names a violated law and the concrete case; ' +
            'a clean verdict from an attack that finds nothing is a first-class result. CHARTER THE WRITER WORKED TO: ' +
            t.charter +
            '\n\nWrite your COMPLETE fixlog JSON (keys files, deltas, landed, beyond, residual, harvest, summary) to ' +
            fixlogPath +
            ' with the Write tool (delete any prior file at that path first), then return the thin receipt. ' +
            LAW_RESIDUAL +
            ' ' +
            LAW_HARVEST,
        { label: 'critique:' + t.key, phase: 'Critique', model: 'opus', effort: 'high', schema: RECEIPT },
    );

const writePrompt = (t) =>
    'ROLE: implementation writer owning ' +
    pagesOf(t) +
    ' in the texture campaign. ' +
    DOCTRINE +
    '\n\nCONSUMPTION LADDER, in this order and no other: (1) YOUR OWN BLIND PASS FIRST — open your territory pages (and for a new ' +
    'page its named siblings) and derive your own defect list, collapse targets, and design rulings from disk BEFORE opening any ' +
    'recon product; the majority of your diff comes from your own attack. (2) THEN read your recon dossier at ' +
    DOSSIERS +
    '/' +
    t.dossier +
    '.json IN FULL as grounding to verify and exceed — never a ceiling. (3) The campaign doc section named in your charter is ' +
    'decision-complete design law: realize it fully, and where disk contradicts it, disk wins for facts and the campaign doc wins ' +
    'for decisions.\n\nCHARTER: ' +
    t.charter +
    '\n\n' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST +
    '\n\nWrite every change in place with the Edit and Write tools. Report files touched, symbol deltas as data, what landed ' +
    'against the charter, what you fixed BEYOND it, residuals, and harvest.';

const redteamPrompt = (t, fixlogPath) =>
    'ROLE: RED-TEAM rebuilder with full writer authority over ' +
    pagesOf(t) +
    '. You are the terminal pre-mortem on this territory and you RECONSTRUCT rather than annotate. ' +
    DOCTRINE +
    '\n\nCOLD PASS FIRST: derive your own attack from the pages on disk. A critique lane wrote its fixlog to ' +
    fixlogPath +
    ' — read it AFTER your cold pass and treat every row as an UNVERIFIED CLAIM to refute against the current pages, never a ' +
    'settled record; drain any residual row there whose file you own.\n\nYOUR OBJECTIVE is predicate-NEGATIVE: COUNTERFACTUAL — ' +
    'remove the central owner, enumeration, dispatch, or hand-rolled kernel and land the stronger form the removal exposes. ' +
    'GROWTH — owner data absorbs the next case, dimension, modality, provider, or consumer; the proof of a correct shape is the ' +
    'diff of the next feature: one declaration inside the owner, every consumer untouched or loudly broken. LONG TAIL — empty, ' +
    'singular, plural, stream, malformed, concurrent, cancelled, partial-failure all preserve the declared rails. COMPOSITION — ' +
    're-derive package choice, lower-stratum ownership, policy resolution, routing, lifecycle. INTEGRITY — repair downward ' +
    'dependency, duplicated ownership, host leakage, sibling-interior coupling, sprawl, and phantom members at every touched end. ' +
    'COLD CLOSE — re-judge every conformance dimension by name against the rebuilt result before your verdict.\n\nCHARTER: ' +
    t.charter +
    '\n\n' +
    LAW_RESIDUAL +
    ' ' +
    LAW_HARVEST;

// One territory chain: opus write -> codex critique (write lane) -> opus redteam. Redteam reads the
// critique fixlog from disk; only thin receipts cross the wire.
const chain = (t) =>
    agent(writePrompt(t), { label: 'write:' + t.key, phase: 'Write', model: 'fable', effort: 'high', schema: FIXLOG }).then((fix) =>
        critique(t, SCRATCH + '/' + t.key + '-critique-fixlog.json').then(() =>
            agent(redteamPrompt(t, SCRATCH + '/' + t.key + '-critique-fixlog.json'), {
                label: 'redteam:' + t.key,
                phase: 'RedTeam',
                model: 'opus',
                effort: 'high',
                schema: FIXLOG,
            }).then((red) => ({ key: t.key, fix, red })),
        ),
    );

const wave = async (territories) => (await parallel(territories.map((t) => () => chain(t)))).filter(Boolean);

const pooledResiduals = (chains) =>
    chains
        .flatMap((c) => [c.fix, c.red].filter(Boolean))
        .flatMap((s) => s.residual || [])
        .filter((r) => r && r.class !== 'cosmetic');

// Residuals carry across phases through one durable file; a small opus lane merges because the
// orchestrator holds no filesystem.
const carryMerge = (rows, note) =>
    rows.length
        ? agent(
              'Merge residual rows into the campaign carry file ' +
                  CARRY +
                  ' (JSON array; create it if absent, preserve existing rows, dedupe on files+claim). Rows to merge, verbatim: ' +
                  JSON.stringify(
                      rows
                          .map((r) => ({ files: (r.files || []).slice().sort(), claim: r.claim, owner: r.owner, class: r.class }))
                          .sort((a, b) => (a.files.join(',') + a.claim).localeCompare(b.files.join(',') + b.claim)),
                  ) +
                  ' Context: ' +
                  note +
                  '. Return the receipt: ok, report = the carry path, entries = total rows after merge, headline, failure empty.',
              { label: 'carry', phase: 'Drain', model: 'opus', effort: 'low', schema: RECEIPT },
          )
        : null;

// --- [COMPOSITION] ----------------------------------------------------------------------

if (!PHASE) {
    log('no phase — pass {phase: 0..6}');
    return { skipped: true, reason: 'no phase argument' };
}
log('texture-campaign phase ' + PHASE + ' · scratch ' + SCRATCH);

// --- [PHASE_0]

if (PHASE === '0') {
    const RECON = [
        {
            key: 'cs-raster',
            task:
                'Recon dossier for the C# Raster/ substrate territory of the texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL first (sections: Materials + kernel deep map, C# DESIGN, ADOPTED_AMENDMENTS 1-4, 19). Territory: ' +
                'libs/csharp/Rasm.Materials/.planning/ — README.md, ARCHITECTURE.md, RULINGS.md, the Appearance/ pages texture.md ' +
                'and graph.md IN FULL, and the folder .api/ catalog roster; libs/csharp/Rasm.Materials/*.csproj; the kernel pages ' +
                'the campaign leverages (search libs/csharp/Rasm/.planning for: identity/ContentHash + XxHash128 streaming leg, ' +
                'Deterministic splitmix64, ParallelHelper struct-IAction exemplars, Span2D/MemoryOwner charters, CholeskySparse, ' +
                'SampleKind blue noise) and libs/csharp/.api/ catalogs for CommunityToolkit.HighPerformance, MathNet.Numerics, ' +
                'System.Numerics.Tensors. Map: exact existing type shapes (TextureSource, TextureUv, ShadeVec4, CompiledGraph.Shade ' +
                'signature), FaultBand roster + code allocation mechanics, MipPolicy/pyramid absence, every kernel member the new ' +
                'Raster/ pages will compose with verified signatures, and the section-marker/fence grammar the folder pages use.',
        },
        {
            key: 'cs-appearance',
            task:
                'Recon dossier for the C# Appearance-edit territory of the texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL first. Territory: libs/csharp/Rasm.Materials/.planning/Appearance/ — texture.md, graph.md, acquisition.md, ' +
                'interchange.md, observability.md, benchmarks.md, analytics.md IN FULL, plus folder README/ARCHITECTURE/RULINGS/' +
                'IDEAS/TASKLOG and the .api catalogs for MessagePack, Unicolour, UnitsNet. CRITICAL EXTRACT for the wire freeze: the ' +
                'EXACT OpenPbrSurface column set (all columns, spelled), MaterialParameters columns, MaterialWire Key layout, ' +
                'AppearanceSummary.Of exact factory signature, MtlxDocument NodeCategory rows touching image/tiledimage, the ' +
                'MaterialsInstruments row grammar, BenchWorkload row grammar, DatasetWire.Admission current arity vs ' +
                'AnalyticsSchema.Admit. Also map the interchange egress naming grammar and every PROTO/wire row shape.',
        },
        {
            key: 'cs-hosts',
            task:
                'Recon dossier for the C# host territories (Compute + AppUi) of the texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL first (amendments 10, 17, 21). Territory: libs/csharp/Rasm.Compute/.planning/Model/{inference,providers,' +
                'sessions}.md IN FULL + Compute README/ARCHITECTURE/RULINGS + its api-silk-webgpu.md and ONNX catalogs; ' +
                'libs/csharp/Rasm.AppUi/.planning/Render/{shading,pathtrace}.md IN FULL + AppUi README/ARCHITECTURE + its ' +
                'Silk.NET.WebGPU .api catalogs; libs/csharp/.api/ silk entries if any. Map: ShadeUniforms.From exact shape, ' +
                'LightSource.Environment exact row, the [BOUNDARY] seam declarations both ends, ORT session/provider row grammar, ' +
                'where the two folder Silk catalogs live (exact paths) for the substrate promotion, and the Directory.Packages.props ' +
                'rows for Silk.NET/ONNX (verify Native.WGPU 2.23.0 pin present).',
        },
        {
            key: 'cs-ripples',
            task:
                'Recon dossier for the C# ripple territory (Element/Bim/Persistence) of the texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL first (amendments 11, 12, 18). Territory: libs/csharp/Rasm.Element/.planning/Graph/element.md + ' +
                'Projection/fault.md + Graph/wire.md IN FULL; libs/csharp/Rasm.Bim/.planning/Semantics/appearance.md + ' +
                'Exchange/{export,format}.md IN FULL; libs/csharp/Rasm.Persistence/.planning/Query/{cache,columnar}.md IN FULL; ' +
                'each folder README/ARCHITECTURE/RULINGS; libs/csharp/.planning/{ARCHITECTURE,RULINGS}.md. Map: EVERY ' +
                'AppearanceSummary.Of call site corpus-wide with its exact spelled arity (rg --hidden), the IfcSurfaceStyle Author ' +
                'ctor exact shape, the Tinted/MeshOf MaterialBuilder fold, KhrExtension row states, ArtifactKind + RetentionClass + ' +
                'LandingArm row grammars, FaultBand 2460 availability, and the branch ARCHITECTURE S4 roster + mermaid edge grammar.',
        },
        {
            key: 'py',
            task:
                'Recon dossier for the python territory of the texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL first (PYTHON DESIGN + amendments 5, 14, 15, 25). Territory: libs/python/artifacts/.planning/ — ' +
                'graphic/raster/{io,process,measure}.md, core/receipt.md, scene/stage.md, bench.md IN FULL, README/ARCHITECTURE/' +
                'RULINGS/IDEAS/TASKLOG, .api/imagecodecs.md and .api/pyvips catalogs; libs/python/runtime/.planning/transport/' +
                'shapes.md IN FULL (PROTO_VOCABULARY + WIRE_REGISTRY grammar); libs/python/.planning/{ARCHITECTURE,RULINGS}.md; ' +
                'pyproject.toml (the <3.15 gate rows, pyvips row, colour-science row, imagecodecs row). Map: ArtifactKind token ' +
                'roster + all six receipt landing sites with line anchors, ConvertFormat/_VIPS_SUFFIX/_CODEC_KWARGS row grammar, ' +
                'Kernel/KernelTrait/lane.offload exact spellings, ContentIdentity.key, stage.Texture current shape, the bench ' +
                'CORPUS row grammar, and the exact [[tool.uv.dependency-metadata]] colour-cxf precedent block.',
        },
        {
            key: 'ts',
            task:
                'Recon dossier for the TypeScript territory of the texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL first (TS DESIGN + amendments 5, 16, 26). Territory: libs/typescript/core/.planning/ codec/format pages ' +
                'carrying the interchange census (find them; also observe/convention.md) IN FULL; libs/typescript/data/.planning/' +
                'object/{file,store,stream}.md IN FULL + data README/RULINGS + data/.api roster; libs/typescript/ui/.planning/' +
                'viewer/scene.md IN FULL; libs/typescript/iac/.planning/program/source.md IN FULL; libs/typescript/.planning/' +
                '{ARCHITECTURE,RULINGS}.md; pnpm-workspace.yaml catalog rows (aws-sdk, sharp, three). Map: the census/_families/' +
                'landing/schema row grammar with exact anchors, Glb.AssetRoster + assetPath derivation both ends, _addressed ' +
                'publisher fold, _CACHE_POSTURE absence, sharp _GATE + Derive.Spec + fanout fold shapes, three 0.185.1 ' +
                'RGBELoader/EXRLoader/PMREMGenerator/KTX2Loader presence in node_modules typings, and the convention.md ' +
                'counter/histogram row grammar.',
        },
        {
            key: 'cross',
            task:
                'Recon dossier for the cross-tier territory of the texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL first (CROSS-CUTTING DESIGN + amendments 5, 26, 27, 28). Territory: tests/contracts/MANIFEST.md + ' +
                'tests/contracts/README.md + tests/RULINGS.md IN FULL (entry schema, ledger row + H3 record grammar, DESIGN-PIN ' +
                'marker, existing appearance entries); libs/.planning/ARCHITECTURE.md ([04]-[GEOMETRY_FLOW] exact bullet 2 text, ' +
                '[07] single-producer law text) + libs/.planning/RULINGS.md row grammar; docs/laws/topology.md (row grammar, rows ' +
                '[17] and the numbering tail — verify next free indices around [40]); docs/glossary/domain.md + estate.md (section ' +
                'grammar, [NOT] line grammar); libs/csharp/.planning/IDEAS.md card template. Map every grammar with exact anchors ' +
                'and the highest existing index numbers so later writers append without collision.',
        },
    ];

    const probeCore = () =>
        agent(
            'ROLE: native build-probe lane for texture-campaign phase 0. You run BUILDS, not doc work. Read ' +
                CAMPAIGN +
                ' section "Python/TS package research [LANDED]" and the staged probe procedure at ' +
                PROBE_PLAN +
                ' IN FULL, then execute probes 1-3 exactly as staged, with build dirs under ' +
                REPO +
                '/' +
                HOME +
                '/probes/ instead of the stale scratchpad path the procedure names: (1) OpenEXR 3.4.x sdist build at the 3.15 ' +
                'floor via forge-scientific-env, write+read-back verify with named channels; (2) opencolorio 2.5.x same pattern, ' +
                'note vendored-vs-system dep resolution from the build log; (3) colour-science 0.4.7 cap bypass — prove which ' +
                'mechanism works. Long builds: run each install in ONE Bash call with timeout 600000 and the log teed to disk. ' +
                'ALSO probe scikit-image 0.26.0 the same venv pattern (sdist, meson+cython). Per probe record VERDICT ' +
                '(BUILDS-AT-FLOOR-CLEAN | NEEDS-FORGE-ROW <nixpkgs attrs> | BLOCKED <reason>), wall seconds, evidence lines. ' +
                'NEVER touch ' +
                REPO +
                '/.venv or pyproject.toml; scratch venvs only; no cleanup. Write the COMPLETE verdicts JSON (keys: probes ' +
                '[{package, verdict, rows_needed, wall_secs, evidence}], summary) to ' +
                PROBES_CORE +
                ' and return the thin receipt (report = that path, entries = probe count).',
            { label: 'probe:core', phase: 'Probe', model: 'opus', effort: 'medium', schema: RECEIPT },
        );

    const probeForge = (coreReceipt) =>
        agent(
            'ROLE: Forge-row lane for texture-campaign phase 0. The core probe verdicts sit at ' +
                PROBES_CORE +
                ' (headline: ' +
                ((coreReceipt && coreReceipt.headline) || 'missing — read the file, and if absent derive needed rows from the campaign doc') +
                '). Read ' +
                CAMPAIGN +
                ' section "Python/TS package research" and the staged procedure at ' +
                PROBE_PLAN +
                ' IN FULL. Work: (1) In ' +
                FORGE +
                '/modules/home/programs/languages/scientific-tools.nix land the drafted rows in the file OWN style: a ' +
                'pkgs.ktx-tools.overrideAttrs lifting meta.platforms to unix, bound into artifactNativeLibs AND ' +
                'scientificRuntimeTools; three wrapper env exports LIBKTX_VERSION=4.4.2, LIBKTX_INCLUDE_DIR, LIBKTX_LIB_DIR; plus ' +
                'any nixpkgs attr rows the core verdicts prove needed (openexr, imath, or others named NEEDS-FORGE-ROW). Verify ' +
                'attr existence with the nixos MCP nix tool before landing each row; format with alejandra. (2) Run forge-redeploy ' +
                '--switch (one Bash call, timeout 600000, log to disk; on failure read the log, repair the nix, retry once). ' +
                '(3) Probe pyktx 4.4.2 per the staged pattern in a scratch venv under ' +
                REPO +
                '/' +
                HOME +
                '/probes/. (4) Re-run any core probe that returned NEEDS-FORGE-ROW for a row you landed. (5) From ' +
                REPO +
                ' run: uv run python -m tools.assay provision — capture the ktx CLI evidence rows (ktx, ktx2check, toktx on PATH ' +
                'with versions). (6) Verify the vtk/usd-core overlay route: nix eval on python3Packages.vtk and ' +
                'python3Packages.openusd via the nixos MCP or nix eval (versions + darwin platform support); record the verdict, ' +
                'land NO overlay machinery. NEVER touch ' +
                REPO +
                '/.venv or pyproject.toml. Write the COMPLETE verdicts JSON (keys: forgeRows [{row, file, landed}], redeploy ' +
                '{ok, evidence}, probes [{package, verdict, rows_needed, wall_secs, evidence}], provision {ok, evidence}, overlay ' +
                '{vtk, openusd}, summary) to ' +
                PROBES_FORGE +
                ' and return the thin receipt.',
            { label: 'probe:forge', phase: 'Probe', model: 'opus', effort: 'high', schema: RECEIPT },
        );

    const probeChain = () =>
        probeCore().then((core) =>
            probeForge(core).then((forge) => ({ core: shapeReceipt('probe-core')(core), forge: shapeReceipt('probe-forge')(forge) })),
        );

    const results = (
        await parallel([probeChain, ...RECON.map((t) => () => codexRead(t.key, t.task, DOSSIERS + '/' + t.key + '.json', 'Recon'))])
    ).filter(Boolean);
    const probes = results.find((r) => r && r.core);
    const recon = results.filter((r) => r && !r.core);
    log(
        recon.filter((d) => d.ok).length +
            '/' +
            recon.length +
            ' recon lanes landed · probes ' +
            (probes && probes.forge && probes.forge.ok ? 'ok' : 'CHECK'),
    );

    const freeze = await agent(
        'ROLE: wire-roster FREEZE writer for texture-campaign phase 0 — the single most consequential artifact of the campaign. ' +
            'Read ' +
            CAMPAIGN +
            ' IN FULL (amendments 5-9, 19 are your charter), then the recon dossiers at ' +
            [DOSSIERS + '/cs-appearance.json', DOSSIERS + '/py.json', DOSSIERS + '/ts.json', DOSSIERS + '/cross.json'].join(', ') +
            ' IN FULL, then open the pages they anchor wherever a roster-relevant fact needs primary-source confirmation — the ' +
            'OpenPbrSurface column set MUST come from the live page, never the dossier alone. Write ' +
            FREEZE +
            ' as the binding wire law for phases 2-5: (1) SHARED SCHEMA FRAGMENT vocabulary — the channel/role roster DERIVED ' +
            'from the OpenPbrSurface column set + geometric channels (normal/height/AO/curvature), each row with per-role ' +
            'colorspace, neutral value, mip law; NormalConvention {GL|DX}; roughness-vs-gloss transfer; ORM/MRA packing order; ' +
            'transfer-tag roster {Linear|Srgb|Raw|Pq|Hlg}; AlphaMode {Straight|Associated|None}; KtxPayload {RawBcn|Uastc|Etc1s} ' +
            '+ web wire-legality. (2) [TEXTURE_SET_BY_KEY] full field roster (C#-produced baked set document, proto ' +
            'rasm.materials.textureset.v1). (3) [ASSET_SET_MANIFEST] full field roster (python-produced ingest/IBL manifest). ' +
            '(4) EnvironmentLightWire, StageRequest, StageResult field rosters. (5) SH9 band order + normalization: ONE frozen ' +
            'spelling with a golden fixture vector. (6) Egress naming grammar materials/texture/<key>/<channel>.<ext>. Every ' +
            'roster is a table; every field carries type, units where physical, and producing branch. The doc states law in the ' +
            'campaign register — no narration, no alternatives. Language-side spellings (C# PascalCase, python snake_case, TS ' +
            'camelCase) are TRANSCRIPTIONS: state the canonical concept name once and the three casings per roster row where ' +
            'they differ mechanically. Return the thin receipt (report = the freeze path).',
        { label: 'freeze', phase: 'Freeze', model: 'opus', effort: 'high', schema: RECEIPT },
    );

    return {
        phase: 0,
        probes: probes || null,
        recon: recon.map((d) => ({ key: d.key, ok: d.ok, entries: d.entries, report: d.report, failure: d.failure })),
        freeze: shapeReceipt('freeze')(freeze),
        gate: 'verify: ' + PROBES_CORE + ', ' + PROBES_FORGE + ', ' + FREEZE + ' on disk with clean verdicts before phase 1',
    };
}

// --- [PHASE_1]

if (PHASE === '1') {
    const MANIFESTS = [
        {
            key: 'cs-packages',
            dossier: 'cs-hosts',
            pages: [
                'Directory.Packages.props',
                'libs/csharp/Rasm.Materials/*.csproj',
                'libs/csharp/Rasm.Materials/README.md',
                'libs/csharp/.api/',
                'libs/csharp/Rasm.Materials/.api/',
            ],
            charter:
                'C# package-admission gate per CAMPAIGN "PACKAGES C#" + amendments 21/23. Central manifest rows: ' +
                'SixLabors.ImageSharp 4.0.0, TextureCompressor 0.1.0 + its Ktx and Hdr subpackages, TinyEXR.NET 1.1.0 — each ' +
                'pin-exact with the TextureCompressor hold comment; verify each id+version on the nuget MCP first. Materials ' +
                'csproj: label-grouped ItemGroups "Raster Imaging" + "GPU Bake" (Silk.NET.WebGPU trio consumption). README: ' +
                'RASTER_IMAGING card + substrate-package rows. Silk.NET substrate promotion: MOVE the AppUi and Compute ' +
                'api-silk-webgpu catalogs to libs/csharp/.api/ (merge into one, retire both folder copies, repoint every anchor ' +
                'citing them — rg --hidden for citations); AppUi/Compute/Materials READMEs each gain the substrate row. ADD 5 ' +
                'catalogs under libs/csharp/Rasm.Materials/.api/ (ImageSharp, TextureCompressor, TinyEXR, plus the two the new ' +
                'pages need most per the dossier) — authored from live surfaces via assay api rails at member depth with ' +
                '[STACKING]. Magick.NET: IDEAS card only. GATE: dotnet restore with locked mode expectations (regen ' +
                'packages.lock.json files), then uv run python -m tools.assay static over touched C# surfaces; repair to green.',
        },
        {
            key: 'py-packages',
            dossier: 'py',
            pages: ['pyproject.toml', 'uv.lock', 'libs/python/artifacts/.planning/.api/imagecodecs.md'],
            charter:
                'Python package gate per CAMPAIGN "Python/TS package research" + probe verdicts at ' +
                PROBES_CORE +
                ' and ' +
                PROBES_FORGE +
                ' (read both IN FULL; every gate drop must cite its probe verdict). pyproject: pyvips[binary] repair; ' +
                '[[tool.uv.dependency-metadata]] override for colour-science 0.4.7 (colour-cxf precedent block shape); DROP the ' +
                '<3.15 gates the probes proved (scikit-image; OpenEXR/opencolorio admit if their probes prove clean — new lean ' +
                'unpinned rows; pyktx admits only on a clean post-Forge probe verdict; vtk/usd-core drop ONLY if the overlay ' +
                'verdict landed a working route, else leave gated and record the residual). imagecodecs .api catalog: full ' +
                're-scope to the live surface (EXR incl DWAA/DWAB/HTJ2K, rgbe, 16-bit PNG, float TIFF, JXL, AVIF, WebP, lcms2, ' +
                'BCn/DDS decode, meshopt) + [STACKING]; verify members by live reflection against .venv. GATE: ' +
                'forge-scientific-env uv lock && forge-scientific-env uv sync at the repo root (quiet-window single writer — you ' +
                'are the only venv mutator this phase), then import-probe each admitted package; repair to green.',
        },
        {
            key: 'ts-packages',
            dossier: 'ts',
            pages: ['pnpm-workspace.yaml', 'libs/typescript/data/.api/', 'libs/typescript/data/README.md'],
            charter:
                'TS package gate per CAMPAIGN "Python/TS package research". pnpm catalog: @aws-sdk 3.1093.0→3.1096.0 bump; ADMIT ' +
                'ktx-parse 1.1.0, @gltf-transform/core 4.4.2, @gltf-transform/extensions 4.4.2, @gltf-transform/functions 4.4.2, ' +
                'meshoptimizer 1.2.0 — verify each on the registry first; NO @effect-aws, NO gltf-transform CLI. Wire the admits ' +
                'into the owning package.json manifests per catalog convention. ADD the 5 data/.api catalogs (ktx-parse, the ' +
                'three gltf-transform packages — or one merged gltf-transform catalog if the tier convention merges scoped ' +
                'families, follow the existing roster convention — and meshoptimizer) authored from node_modules typings at ' +
                'member depth with [STACKING]; data README registry rows both ways. GATE: pnpm install under catalogMode strict; ' +
                'then verify the served-asset facts the campaign names (basis_transcoder.js/wasm, meshopt_decoder.module.js ' +
                'paths inside the installed packages) and record exact paths as facts in your fixlog for the PH4 iac writer. ' +
                'Repair to green.',
        },
    ];
    const writes = (
        await parallel(
            MANIFESTS.map(
                (m) => () =>
                    agent(writePrompt(m), { label: 'admit:' + m.key, phase: 'Write', model: 'opus', effort: 'high', schema: FIXLOG }).then((r) => ({
                        key: m.key,
                        fix: r,
                        red: null,
                    })),
            ),
        )
    ).filter(Boolean);
    log('admission writers landed: ' + writes.map((w) => w.key + '(' + ((w.fix && w.fix.files) || []).length + ')').join(' · '));

    const verdict = await agent(
        'ROLE: phase-1 acceptance verifier for the texture campaign, with writer authority to repair small breaks. Read ' +
            CAMPAIGN +
            ' phase table row 1. Re-run all three gates from scratch and paste real output as evidence: (1) dotnet restore ' +
            '(locked mode) at the C# root; (2) forge-scientific-env uv lock --check && forge-scientific-env uv sync at ' +
            REPO +
            '; (3) pnpm install with catalogMode strict. Then uv run python -m tools.assay static over the touched manifest ' +
            'surfaces. Then touch-point alignment both ways for EVERY package admitted this phase: central manifest row + ' +
            'project manifest/csproj + README registry row + owning .api catalog — repair an orphan at its owner. A failing gate ' +
            'you cannot repair lands as a blocked residual with the full command output. Writer fixlogs (verify, never trust): ' +
            writes.map((w) => w.key + ' :: ' + (((w.fix && w.fix.files) || []).slice().sort().join(' ') || 'none')).join('  ///  '),
        { label: 'gate:ph1', phase: 'Gate', model: 'opus', effort: 'high', schema: VERDICT },
    );
    await carryMerge(pooledResiduals(writes).concat((verdict && verdict.blocked) || []), 'phase 1 package gate');
    return { phase: 1, writers: writes.map((w) => w.key), verdict };
}

// --- [PHASE_2]

if (PHASE === '2') {
    const WAVE_A = [
        {
            key: 'raster-substrate',
            dossier: 'cs-raster',
            pages: [
                'libs/csharp/Rasm.Materials/.planning/Raster/plane.md',
                'libs/csharp/Rasm.Materials/.planning/Raster/codec.md',
                'libs/csharp/Rasm.Materials/.planning/Raster/filter.md',
            ],
            charter:
                'AUTHOR the three Raster/ substrate pages per CAMPAIGN "C# DESIGN" (plane/codec/filter bullets) + amendments 1, ' +
                '7, 8, 9, 19. New folder: mint Raster/ under the Materials .planning tier with these pages in the folder page ' +
                'grammar (the campaign doc bullets are the decision-complete design; the wire-freeze transfer/alpha/ktx rosters ' +
                'are binding). RasterFault 2460 {Decode,Encode,Device,Tile} homes here on codec.md. Run-ready fences at the ' +
                'docs/stacks/csharp bar; every ImageSharp/TinyEXR/TextureCompressor member verified against the PH1 catalogs.',
        },
        {
            key: 'raster-set',
            dossier: 'cs-raster',
            pages: [
                'libs/csharp/Rasm.Materials/.planning/Raster/tile.md',
                'libs/csharp/Rasm.Materials/.planning/Raster/set.md',
                'libs/csharp/Rasm.Materials/.planning/Raster/press.md',
                'libs/csharp/Rasm.Materials/.planning/Raster/gpu.md',
            ],
            charter:
                'AUTHOR the four Raster/ synthesis pages per CAMPAIGN "C# DESIGN" (tile/set/press/gpu bullets) + amendments 1-4, ' +
                '6, 8, 19, 20. TextureChannel derives from the wire-freeze roster (which projects OpenPbrSurface + geometric ' +
                'channels); SetBind lands on set.md; press.md batches on CompiledGraph.ShadeSpan (the graph.md mint is a wave-B ' +
                'sibling — cite the frozen signature from the campaign doc amendment 2, spelled identically); gpu.md is the ONLY ' +
                'page spelling Silk.NET.WebGPU, WGSL kernel table {NoiseField, CheckerField, GradientField, MathFold, MixFold, ' +
                'EquirectToCube, IrradianceSh, PrefilterSpecular} with WGSL bodies as fence law + golden-vector fixture rows. ' +
                'Content-identity law per amendment 3 stated where it binds.',
        },
        {
            key: 'appearance-new',
            dossier: 'cs-appearance',
            pages: ['libs/csharp/Rasm.Materials/.planning/Appearance/environment.md', 'libs/csharp/Rasm.Materials/.planning/Appearance/neural.md'],
            charter:
                'AUTHOR the two new Appearance pages per CAMPAIGN "C# DESIGN" (environment/neural bullets) + amendments 17, 19 + ' +
                'the ONNX research section (model rows, license classes, EP facts) + architecture decisions 1-2. environment.md: ' +
                'SkyModel (Hosek-Wilkie = content-keyed fitted-coefficient data asset with digest row), EnvironmentMap, ' +
                'IblPrefilter → IblProducts (SH9 per the frozen band-order spelling in wire-freeze), EnvironmentLight record for ' +
                'the AppUi seam. neural.md: PbrStage, LicenseClass, ModelCard frozen row table with the 7 researched rows ' +
                '(StableDelight Blocked; PhysicalChannelForbidden column), StagePlan → StageRequest wire per the frozen roster; ' +
                'text→material external-service seam ruling.',
        },
    ];
    const WAVE_B = [
        {
            key: 'materials-edits',
            dossier: 'cs-appearance',
            pages: [
                'libs/csharp/Rasm.Materials/.planning/Appearance/texture.md',
                'libs/csharp/Rasm.Materials/.planning/Appearance/graph.md',
                'libs/csharp/Rasm.Materials/.planning/Appearance/acquisition.md',
                'libs/csharp/Rasm.Materials/.planning/Appearance/interchange.md',
                'libs/csharp/Rasm.Materials/.planning/Appearance/observability.md',
                'libs/csharp/Rasm.Materials/.planning/Appearance/benchmarks.md',
                'libs/csharp/Rasm.Materials/.planning/Appearance/analytics.md',
                'libs/csharp/Rasm.Materials/.planning/README.md',
                'libs/csharp/Rasm.Materials/.planning/ARCHITECTURE.md',
                'libs/csharp/Rasm.Materials/.planning/RULINGS.md',
                'libs/csharp/Rasm.Materials/.planning/IDEAS.md',
                'libs/csharp/Rasm.Materials/.planning/TASKLOG.md',
            ],
            charter:
                'EDIT the seven Appearance pages per CAMPAIGN "EDITED PAGES" (Materials block) + amendments 2, 4, 11, 19, 20: ' +
                'texture.md NoisePeriod + period-wrap golden vectors; graph.md MINTS CompiledGraph.ShadeSpan exactly per ' +
                'amendment 2 + the plane-ops-outside-DAG boundary line; acquisition.md NeuralPlanes returning set + averaged row ' +
                '+ Provenance cols; interchange.md TextureSetWire/EnvironmentLightWire/StageRequest/StageResult per the frozen ' +
                'rosters + proto row + egress naming + AppearanceSummary arity align + MtlxDocument baked-filename binding row; ' +
                'observability +4 fact cases + UCUM-partitioned instrument rows; benchmarks +5 workloads incl PressGpuParity and ' +
                'the ShadeSpan texels/sec-at-4k row; analytics spine repair + materials.texture dataset. INDEX CLOSURE per ' +
                'amendment 13: ARCHITECTURE codemap + README router entries for all 9 new pages + the Materials↔Compute ' +
                'inference [WIRE] pair at the Materials [03]-[SEAMS] end. RULINGS 8 shape + 4-6 package rows, the IDEAS and ' +
                '~TASKLOG cards the campaign doc rosters for this folder — all per the folder card/row templates.',
        },
        {
            key: 'compute',
            dossier: 'cs-hosts',
            pages: [
                'libs/csharp/Rasm.Compute/.planning/Model/inference.md',
                'libs/csharp/Rasm.Compute/.planning/Model/providers.md',
                'libs/csharp/Rasm.Compute/.planning/Model/sessions.md',
                'libs/csharp/Rasm.Compute/.planning/ARCHITECTURE.md',
            ],
            charter:
                'EDIT the three Compute Model pages per CAMPAIGN "EDITED PAGES" (Compute block) + amendment 17 + the ONNX ' +
                'research facts: inference.md InferTiled + TilePlan (fixed-shape tiles, reflect-pad, overlap feather, partition ' +
                'assertion); providers.md CoreML MLProgram pin + CPU-golden parity row + WebGpu EP as a RESEARCH row with its ' +
                'assay verification route (CPU-EP the guaranteed floor); sessions.md per-bucket warmup. ARCHITECTURE gains the ' +
                'Materials↔Compute inference [WIRE] pair at the Compute [03]-[SEAMS] end (StageRequest/StageResult per frozen ' +
                'roster). Compute folder RULINGS/TASKLOG rows the work settles.',
        },
        {
            key: 'appui',
            dossier: 'cs-hosts',
            pages: [
                'libs/csharp/Rasm.AppUi/.planning/Render/shading.md',
                'libs/csharp/Rasm.AppUi/.planning/Render/pathtrace.md',
                'libs/csharp/Rasm.AppUi/.planning/ARCHITECTURE.md',
            ],
            charter:
                'EDIT the two AppUi Render pages per CAMPAIGN "EDITED PAGES" (AppUi block) + amendment 10: shading.md ' +
                'ShadeUniforms texture-slot rows + sampler bind-group growth at the declared [BOUNDARY] seam (Materials supplies ' +
                'planes/closures — AppUi never re-mints them); pathtrace.md SurfacePoint real UV, MaterialOf texture-aware, ' +
                'LIGHT_RIG Environment arm resolving the Materials EnvironmentLight record — directional lookup + luminance-CDF ' +
                'importance sampling replacing the uniform dome. ARCHITECTURE: the AppUi boundary label widens at the AppUi end ' +
                'per the both-endpoints seam law (the branch-tier S4 roster row is phase-5 territory — record a seam residual ' +
                'naming it).',
        },
        {
            key: 'ripples',
            dossier: 'cs-ripples',
            pages: [
                'libs/csharp/Rasm.Element/.planning/Graph/element.md',
                'libs/csharp/Rasm.Element/.planning/Projection/fault.md',
                'libs/csharp/Rasm.Bim/.planning/Semantics/appearance.md',
                'libs/csharp/Rasm.Bim/.planning/Exchange/export.md',
                'libs/csharp/Rasm.Bim/.planning/Exchange/format.md',
                'libs/csharp/Rasm.Persistence/.planning/Query/cache.md',
                'libs/csharp/Rasm.Persistence/.planning/Query/columnar.md',
            ],
            charter:
                'LAND the C# ripple set per CAMPAIGN "C# ripple map" + "EDITED PAGES" + amendments 11, 12, 18: Element ' +
                'element.md#NODE_MODEL settles AppearanceSummary.Of arity (Fin spelling) — then align EVERY call site the ' +
                'dossier anchors; Projection/fault.md 2460 row; Bim Semantics/appearance.md IfcSurfaceStyleWithTextures ingest ' +
                'arm + Author 5-arg null-slot fill + arity repair; Exchange/export.md glTF MaterialChannel texture leg on the ' +
                'Tinted/MeshOf fold (public SharpGLTF surface only); Exchange/format.md KhrExtension row status only; ' +
                'Persistence cache.md ArtifactKind.TextureSet with provenance-derived retention (press-baked Cache, ' +
                'neural-acquired durable); columnar.md LandingArm.MaterialsTexture partition channel. Folder RULINGS/TASKLOG ' +
                'rows where these folders settle decisions.',
        },
    ];
    const a = await wave(WAVE_A);
    log('wave A chains: ' + a.map((c) => c.key).join(' · '));
    const b = await wave(WAVE_B);
    log('wave B chains: ' + b.map((c) => c.key).join(' · '));

    const verdict = await agent(
        'ROLE: phase-2 acceptance verifier for the texture campaign, writer authority for repairs. Checks, each with real ' +
            'command evidence: (1) every page the phase names exists and is docgen-conformant — load the docgen skill gate over ' +
            'the touched Materials/Compute/AppUi/Element/Bim/Persistence pages and repair to zero; (2) every external member a ' +
            'NEW fence cites verifies on the assay api rail (spot-verify the highest-risk twenty: ImageSharp EXR encoder ' +
            'options, TextureCompressor KTX2 writer, TinyEXR deep API, Silk.NET WebGPU compute dispatch, MathNet FFT, ' +
            'CholeskySparse) — an unverifiable member is repaired or landed as a RESEARCH row; (3) the wire-freeze rosters and ' +
            'the landed interchange/set/neural fences agree name-for-name; (4) AppearanceSummary.Of arity is identical at ' +
            'Element/Bim/Materials (rg --hidden all call sites); (5) index closure: README router rows + ARCHITECTURE codemap + ' +
            'seam pairs both ends. Repair what you can; blocked rows carry evidence.',
        { label: 'gate:ph2', phase: 'Gate', model: 'opus', effort: 'high', schema: VERDICT },
    );
    await carryMerge(pooledResiduals(a.concat(b)).concat((verdict && verdict.blocked) || []), 'phase 2 C# wave');
    return { phase: 2, chains: a.concat(b).map((c) => c.key), verdict };
}

// --- [PHASE_3]

if (PHASE === '3') {
    const WAVE_A = [
        {
            key: 'py-lift',
            dossier: 'py',
            pages: ['libs/python/artifacts/.planning/graphic/raster/io.md', 'libs/python/artifacts/.planning/graphic/raster/process.md'],
            charter:
                'EDIT the raster codec surfaces per CAMPAIGN "PYTHON EDITS": process.md +ConvertFormat.JXL 8-bit arm; io.md ' +
                'codec-table rows per the codec growth law (one ConvertFormat row + _VIPS_SUFFIX/_CODEC_KWARGS/_VIPS_KWARGS ' +
                'entries). The deep-pixel estate is a SIBLING territory (graphic/texture/) — never fold float formats into the ' +
                'uint8 Frame rail; the 8-bit funnel stays this page law.',
        },
        {
            key: 'py-texture',
            dossier: 'py',
            pages: [
                'libs/python/artifacts/.planning/graphic/texture/plane.md',
                'libs/python/artifacts/.planning/graphic/texture/derive.md',
                'libs/python/artifacts/.planning/graphic/texture/set.md',
                'libs/python/artifacts/.planning/graphic/texture/ibl.md',
                'libs/python/artifacts/.planning/graphic/texture/ingest.md',
            ],
            charter:
                'AUTHOR the five-page graphic/texture/ sub-domain per CAMPAIGN "PYTHON NEW PAGES" + amendments 5, 14, 15, 19, ' +
                '20, 25 + the probe verdicts at ' +
                PROBES_CORE +
                ' and ' +
                PROBES_FORGE +
                ' (read both — the _DEEP_CODEC OpenEXR rows and the pyktx dual-leg row depend on their verdicts; a failed probe ' +
                'means the row lands as the sentinel/floor form the campaign doc defines). TextureRole/_ROLE_SPACE/' +
                'NormalConvention transcribe the wire-freeze fragment exactly. Fences at the docs/stacks/python bar over the ' +
                'imagecodecs/pyvips/numpy members the PH1 catalogs verify.',
        },
    ];
    const WAVE_B = [
        {
            key: 'py-rows',
            dossier: 'py',
            pages: [
                'libs/python/artifacts/.planning/core/receipt.md',
                'libs/python/artifacts/.planning/scene/stage.md',
                'libs/python/runtime/.planning/transport/shapes.md',
                'libs/python/artifacts/.planning/bench.md',
            ],
            charter:
                'LAND the python row growth per CAMPAIGN "PYTHON EDITS" + amendment 5: receipt.md texture token + case + Texture ' +
                'mint + slot/_facts arms + _BAND map row (all six landing sites the dossier anchors); stage.md Texture widened + ' +
                'PbrMap + the full UsdPreviewSurface sampler set; shapes.md FOUR Structs + PROTO rows — MaterialWire/' +
                'OpenPbrGroupsWire/AppearanceSummaryWire decode landings + AssetSetManifest python-minted, field rosters ' +
                'transcribed from wire-freeze exactly; bench.md 3 CORPUS rows (ktx_encode provision-gated, citing the phase-0 ' +
                'provision evidence).',
        },
        {
            key: 'py-closure',
            dossier: 'py',
            pages: [
                'libs/python/artifacts/.planning/RULINGS.md',
                'libs/python/artifacts/.planning/README.md',
                'libs/python/artifacts/.planning/ARCHITECTURE.md',
                'libs/python/artifacts/.planning/IDEAS.md',
                'libs/python/artifacts/.planning/TASKLOG.md',
            ],
            charter:
                'CLOSE the artifacts folder per CAMPAIGN "PYTHON EDITS" + amendments 25, 28: RULINGS — OpenEXR ' +
                'admitted-via-forge flip (citing the probe verdict), the pyktx DUAL-LEG policy row (ktx CLI floor immovable; ' +
                'pyktx in-process on clean post-Forge probe; retirement clause fires by design), deep-pixel/preview split, ' +
                'bit-depth-follows-referent SHAPE row; README router + codemap entries for graphic/texture/; ARCHITECTURE ' +
                'codemap + seams; the imagecodecs card re-scope; IDEAS/TASKLOG cards the campaign rosters for this folder.',
        },
    ];
    const a = await wave(WAVE_A);
    log('wave A chains: ' + a.map((c) => c.key).join(' · '));
    const b = await wave(WAVE_B);
    log('wave B chains: ' + b.map((c) => c.key).join(' · '));

    const verdict = await agent(
        'ROLE: phase-3 acceptance verifier, writer authority for repairs. Checks with real evidence: (1) docgen gate zero over ' +
            'the touched python pages (load the docgen skill); (2) every imagecodecs/pyvips/numpy member cited in new fences ' +
            'verifies by live reflection against ' +
            REPO +
            '/.venv; (3) the four shapes.md wire rosters match wire-freeze name-for-name; (4) the receipt landing sites are all ' +
            'six present and mutually consistent; (5) provision evidence cited where the campaign requires (ktx rows). Repair ' +
            'what you can; blocked rows carry evidence.',
        { label: 'gate:ph3', phase: 'Gate', model: 'opus', effort: 'high', schema: VERDICT },
    );
    await carryMerge(pooledResiduals(a.concat(b)).concat((verdict && verdict.blocked) || []), 'phase 3 python wave');
    return { phase: 3, chains: a.concat(b).map((c) => c.key), verdict };
}

// --- [PHASE_4]

if (PHASE === '4') {
    const WAVE = [
        {
            key: 'ts-core',
            dossier: 'ts',
            pages: ['libs/typescript/core/.planning/'],
            charter:
                'LAND the core interchange growth per CAMPAIGN "TS EDITS" + amendments 5, 26: the census pages gain BOTH ' +
                'families (TextureSetWire C#-produced + AssetSetManifest python-produced) — census row, _families entry, landing ' +
                'class with maps[{role,digest,file,colorSpace}], schema row each, field rosters transcribed from wire-freeze ' +
                'exactly; PbrGroups map fields land as the BLOCKED idea card armed on the C# projection change, authority = ' +
                'Tier-0 [07] + interchange C#-sole-producer law; observe/convention.md +assetTransformed {object} counter + ' +
                '+assetTranscodeDuration ms histogram. Locate the exact owning pages from your dossier; your territory is the ' +
                'core .planning tree.',
        },
        {
            key: 'ts-asset',
            dossier: 'ts',
            pages: [
                'libs/typescript/data/.planning/object/asset.md',
                'libs/typescript/data/.planning/README.md',
                'libs/typescript/data/.planning/RULINGS.md',
                'libs/typescript/data/.planning/IDEAS.md',
                'libs/typescript/data/.planning/TASKLOG.md',
            ],
            charter:
                'AUTHOR data/object/asset.md per CAMPAIGN "TS NEW PAGE" + amendments 8, 16: ASSET_GATE (ktx-parse read/DFD ' +
                'validation; gltf-transform NodeIO with a CLOSED admitted-extension roster, never ALL_EXTENSIONS) + ' +
                'TRANSFORM_ROWS roster over @gltf-transform/functions and meshoptimizer + spawned-ktx rows; AssetPipe COMPOSES ' +
                'the object-plane fanout spine parameterized by an engine-plane row — it never duplicates file.md FANOUT. ' +
                'Effect rails + tagged fault at the docs/stacks/typescript bar over the PH1-catalogued members. Folder closure: ' +
                'README router, RULINGS rows (effect-aws peer-ceiling reject, gltf-cli sharp-pin reject, KTX2-encode-is-CLI-seam), ' +
                'IDEAS/TASKLOG cards.',
        },
        {
            key: 'ts-ui',
            dossier: 'ts',
            pages: ['libs/typescript/ui/.planning/viewer/scene.md', 'libs/typescript/ui/.planning/README.md'],
            charter:
                'EDIT ui viewer/scene.md per CAMPAIGN "TS EDITS": Glb.assetDir(asset) directory form → KTX2Loader.' +
                'setTranscoderPath from ONE digest dir; the new ENVIRONMENT_FOLD cluster — RGBELoader/EXR → PMREMGenerator.' +
                'fromEquirectangular once per arrival, keyed + disposed, agnostic to which branch produced the environment ' +
                'asset. Verify every three 0.185.1 member against node_modules typings. README router rows; ui folder cards ' +
                'where the work settles decisions.',
        },
        {
            key: 'ts-iac',
            dossier: 'ts',
            pages: ['libs/typescript/iac/.planning/program/source.md'],
            charter:
                'EDIT iac program/source.md per CAMPAIGN "TS EDITS": _Asset.siblings column + _addressedAll same-digest ' +
                'directory publish; three caller rows draco/ktx2/meshopt with the EXACT served artifacts the PH1 ts-packages ' +
                'fixlog verified inside the installed packages (basis_transcoder.js+wasm, meshopt_decoder.module.js; ' +
                'msc_basis_transcoder is dead — never serve it); ONE _CACHE_POSTURE row assets/* → ' +
                'public,max-age=31536000,immutable read by every provider arm. iac folder cards where the work settles.',
        },
    ];
    const chains = await wave(WAVE);
    log('chains: ' + chains.map((c) => c.key).join(' · '));

    const verdict = await agent(
        'ROLE: phase-4 acceptance verifier, writer authority for repairs. Checks with real evidence: (1) docgen gate zero over ' +
            'touched TS pages; (2) census↔landing pairs agree for BOTH new families and match wire-freeze name-for-name; (3) ' +
            'every three/gltf-transform/ktx-parse/meshoptimizer member cited verifies against node_modules typings; (4) the ' +
            'assets/<digest> law has exactly the publishers and consumers the campaign rules — no third derivation; (5) ' +
            '_CACHE_POSTURE row present and read by every provider arm. Repair what you can; blocked rows carry evidence.',
        { label: 'gate:ph4', phase: 'Gate', model: 'opus', effort: 'high', schema: VERDICT },
    );
    await carryMerge(pooledResiduals(chains).concat((verdict && verdict.blocked) || []), 'phase 4 TS wave');
    return { phase: 4, chains: chains.map((c) => c.key), verdict };
}

// --- [PHASE_5]

if (PHASE === '5') {
    const t = {
        key: 'cross-tier',
        dossier: 'cross',
        pages: [
            'tests/contracts/MANIFEST.md',
            'tests/contracts/',
            'libs/.planning/ARCHITECTURE.md',
            'libs/.planning/RULINGS.md',
            'docs/laws/topology.md',
            'docs/glossary/domain.md',
            'docs/glossary/estate.md',
            'libs/csharp/.planning/ARCHITECTURE.md',
            'libs/csharp/.planning/RULINGS.md',
            'libs/csharp/.planning/IDEAS.md',
            'libs/typescript/.planning/ARCHITECTURE.md',
            'libs/typescript/.planning/RULINGS.md',
        ],
        charter:
            'THE cross-tier single writer per CAMPAIGN "CROSS-CUTTING DESIGN" + amendments 5, 10, 26, 27, 28. Every anchor you ' +
            'write MUST resolve on disk — phases 2-4 landed the surfaces; verify each before citing. (1) tests/contracts/: the ' +
            'frozen shared schema fragment file (transcribed from ' +
            FREEZE +
            ', homed per the contracts README convention), then MANIFEST entries [TEXTURE_SET_BY_KEY], [ASSET_SET_MANIFEST], ' +
            '[MATERIAL_WIRE], [SIGNED_ARTIFACT] — ledger row + H3 record in field order, DESIGN-PIN with the blockers the ' +
            'campaign names; GLB_BY_KEY untouched. (2) Tier-0 ARCHITECTURE [04]-[GEOMETRY_FLOW] bullet-2 truth repair exactly as ' +
            'ruled; NO new section. (3) csharp branch ARCHITECTURE [02]-[STRATA] S4 roster row gains Rasm.Materials + mermaid ' +
            'edge; branch RULINGS WebGPU substrate-promotion row naming the Silk.NET 3.x swap-point owner. (4) TS branch RULINGS ' +
            'digest-directory row (_addressedAll ↔ Glb.assetDir, one wave) + branch ARCHITECTURE routing row. (5) topology rows ' +
            '[40]-[43] per the campaign roster (verify free indices first). (6) glossary domain.md [07]-[APPEARANCE] + estate.md ' +
            'asset address — topology row [17] coupling honored, no .greptile edit. (7) cross-libs RULINGS row: photo→PBR mints ' +
            'no corpus entry; csharp branch COLLAPSE row: raster encode owners stay plural per stratum. (8) the [BLOCKED] ' +
            'Rhino-RenderMaterial-adapter IDEAS card at libs/csharp/.planning/IDEAS.md, armed on a product shell landing. (9) ' +
            'DRAIN the carry file ' +
            CARRY +
            ': every open row whose owner is a phase-5 surface lands here; re-verify each against disk first.',
    };
    const fix = await agent(writePrompt(t), { label: 'write:cross-tier', phase: 'Write', model: 'opus', effort: 'high', schema: FIXLOG });
    const verdict = await agent(
        'ROLE: phase-5 acceptance verifier, writer authority for repairs. Checks with real evidence: (1) every MANIFEST ' +
            'minter/consumer coordinate opens to a real anchor on disk; (2) the shared fragment and the three branch ' +
            'transcriptions (C# TextureChannel rosters, python TextureRole/_ROLE_SPACE, TS colorSpace literals) agree with ' +
            FREEZE +
            ' name-for-name; (3) topology row indices collide with nothing and each coupling row two-ends resolves; (4) glossary ' +
            'sections conform to band-page grammar; (5) uv run python -m tools.assay static over any touched fence surfaces. ' +
            'Writer fixlog to verify, never trust: ' +
            (((fix && fix.files) || []).slice().sort().join(' ') || 'none'),
        { label: 'gate:ph5', phase: 'Gate', model: 'opus', effort: 'high', schema: VERDICT },
    );
    await carryMerge(
        ((fix && fix.residual) || []).filter((r) => r.class !== 'cosmetic').concat((verdict && verdict.blocked) || []),
        'phase 5 cross-tier',
    );
    return { phase: 5, files: (fix && fix.files) || [], verdict };
}

// --- [PHASE_6]

if (PHASE === '6') {
    const baseNote = BASE
        ? 'The campaign base commit is ' + BASE + ' — the touched set is git diff --name-only ' + BASE + '..HEAD plus uncommitted changes.'
        : 'Derive the touched set from git: every commit whose message names the texture campaign plus uncommitted changes.';

    const docgen = await agent(
        'ROLE: phase-6 docgen closer for the texture campaign, full writer authority. ' +
            baseNote +
            ' Load the docgen skill, run its gate over every touched markdown surface, and repair to ZERO — structural moves ' +
            'allowed, capability deletion forbidden. Then verify MANIFEST anchors still resolve after your repairs. Report the ' +
            'fixlog.',
        { label: 'docgen', phase: 'Gate', model: 'opus', effort: 'high', schema: FIXLOG },
    );

    const VERIFY = [
        {
            key: 'verify-cs',
            task:
                'Cold-verify the C# half of the landed texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL (C# DESIGN + amendments) and ' +
                FREEZE +
                ', then the landed pages: libs/csharp/Rasm.Materials/.planning/Raster/ (all), Appearance/{environment,neural,' +
                'texture,graph,acquisition,interchange,observability,benchmarks,analytics}.md, the Compute Model pages, AppUi ' +
                'Render pages, Element/Bim/Persistence ripple pages, Materials README/ARCHITECTURE/RULINGS. Verify each campaign ' +
                'claim LANDED and each amendment held (1-4, 6-13, 17-23); verify AppearanceSummary.Of arity uniform corpus-wide; ' +
                'hunt fabricated members, phantom rosters, wire-freeze divergence, missing index rows. Facts with anchors; a ' +
                'failed claim is a fact row with role defect.',
        },
        {
            key: 'verify-py-ts',
            task:
                'Cold-verify the python and TS halves of the landed texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL (PYTHON + TS DESIGN + amendments 5, 14-16, 24-28) and ' +
                FREEZE +
                ', then: libs/python/artifacts/.planning/graphic/texture/ (all five), graphic/raster/{io,process}.md, ' +
                'core/receipt.md, scene/stage.md, libs/python/runtime/.planning/transport/shapes.md, bench.md, artifacts ' +
                'README/ARCHITECTURE/RULINGS, pyproject.toml; libs/typescript core census pages, data/object/asset.md, ui ' +
                'viewer/scene.md, iac program/source.md, data README/RULINGS, pnpm-workspace.yaml. Verify every claim landed, ' +
                'rosters transcribe wire-freeze exactly, probe-gated rows cite real verdicts, census↔landing pairs agree. Facts ' +
                'with anchors; failures are fact rows with role defect.',
        },
        {
            key: 'verify-gov',
            task:
                'Cold-verify the governance half of the landed texture campaign. Read ' +
                CAMPAIGN +
                ' IN FULL (CROSS-CUTTING + Verification sections) and ' +
                FREEZE +
                ', then: tests/contracts/MANIFEST.md + the schema fragment + tests/contracts/README.md, ' +
                'libs/.planning/{ARCHITECTURE,RULINGS}.md, docs/laws/topology.md rows [17] and [40]-[43], docs/glossary/' +
                '{domain,estate}.md, both branch ARCHITECTURE/RULINGS files, Directory.Packages.props + pyproject.toml + ' +
                'pnpm-workspace.yaml package rows, and the .api catalogs the campaign added or moved. Verify every ' +
                'minter/consumer coordinate resolves, every touch-point set aligns both ways, no dual-homed catalogs, the ' +
                'Tier-0 repair landed verbatim. Facts with anchors; failures are fact rows with role defect.',
        },
    ];
    const verifies = (await parallel(VERIFY.map((v) => () => codexRead(v.key, v.task, SCRATCH + '/' + v.key + '.json', 'Recon')))).filter(Boolean);
    log(verifies.filter((v) => v.ok).length + '/' + verifies.length + ' cold-verify lanes landed');

    let open = null;
    for (let round = 0; round < DRAIN_ROUNDS; round++) {
        const drain = await agent(
            'ROLE: residual-drain closer, round ' +
                (round + 1) +
                ' of at most ' +
                DRAIN_ROUNDS +
                ', full writer authority. Inputs: the campaign carry file ' +
                CARRY +
                (round === 0
                    ? ' AND the cold-verify reports at ' +
                      verifies
                          .filter((v) => v.ok)
                          .map((v) => v.report)
                          .sort()
                          .join(', ') +
                      ' (read each IN FULL; every defect-role fact is a residual row)'
                    : ' (the prior round left only these open rows: ' + JSON.stringify(open) + ')') +
                '. For each row: re-verify against CURRENT disk (a row a later stage already resolved is culled with proof); fix ' +
                'the rest at root per the campaign doctrine — read ' +
                CAMPAIGN +
                ' and ' +
                FREEZE +
                ' before any wire or vocabulary edit; discard cosmetic-class rows. Rewrite ' +
                CARRY +
                ' to exactly the still-open remainder (each row naming its blocker and owner) and report resolved/open.',
            { label: 'drain:' + (round + 1), phase: 'Drain', model: 'opus', effort: 'high', schema: DRAINLOG },
        );
        const stillOpen = (drain && drain.open) || [];
        log('drain round ' + (round + 1) + ': ' + (((drain && drain.resolved) || []).length || 0) + ' resolved, ' + stillOpen.length + ' open');
        if (!stillOpen.length || (open && stillOpen.length >= open.length)) {
            open = stillOpen;
            break;
        }
        open = stillOpen;
    }

    const custody = await agent(
        'Dispatch one infra-custodian pass over the texture campaign infra set and IMPLEMENT its verdict rows yourself, ' +
            'treating each as a signal you re-verify on disk. ' +
            baseNote +
            ' The infra set: every RULINGS file the campaign touched (cross-libs, both branches, Materials, artifacts, data), ' +
            'tests/contracts/MANIFEST.md, docs/laws/topology.md, the folder README/ARCHITECTURE index docs, and the touched ' +
            '.api catalog roster. Then read docs/laws/README.md admission ladder and adjudicate the pooled harvest nominations ' +
            'refutation-first — landing nothing is a first-class verdict.',
        { label: 'custody', phase: 'Drain', model: 'opus', effort: 'high', agentType: 'infra-custodian', schema: FIXLOG },
    );

    return {
        phase: 6,
        docgen: { files: ((docgen && docgen.files) || []).length, summary: (docgen && docgen.summary) || '' },
        verifies: verifies.map((v) => ({ key: v.key, ok: v.ok, entries: v.entries })),
        open: open || [],
        custody: { files: ((custody && custody.files) || []).length, summary: (custody && custody.summary) || '' },
    };
}

log('unknown phase ' + PHASE);
return { skipped: true, reason: 'unknown phase ' + PHASE };
