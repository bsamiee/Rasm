export const meta = {
    name: 'spec-uplift',
    whenToUse:
        'After a libs/typescript package version bump: pass the batch manifest as args.batches and it corrects API drift and weaves the newly admitted package capability into the .planning spec pages at the docs/stacks/typescript floor; no args = safe no-op.',
    description:
        'Focused explore -> implement -> critique -> redteam pipeline over libs/typescript .planning spec batches, streaming each batch through all four stages with no barrier. Every stage is a codex lane behind a thin sonnet wrapper: explore is a read-only gpt-5.6-terra mapping lane emitting a per-page dossier (corrections, improvements, ripples) plus a hasWork gate; implement is a workspace-write gpt-5.6-sol writer on the own-pass-first ladder (doctrine at source, own blind pass, then the dossier as unverified prior claims) rewriting fences ground-up; critique is a workspace-write sol predicate-positive conformance audit with rebuilder authority; redteam is a workspace-write sol predicate-negative pre-mortem plus full cold re-review, folding the critique deferred rows into its consolidated fixlog. A batch whose explore proves the bump changed nothing no-ops before implement. Quota-dead codex legs re-dispatch natively (terra to opus, sol to fable). One terminal fable closer drains the pooled cross-batch deferred rows against live disk.',
    phases: [
        {
            title: 'Explore',
            detail: 'per batch: read-only terra mapping lane — dossier of corrections, improvements, ripples, hasWork',
            model: 'sonnet',
        },
        { title: 'Implement', detail: 'per batch with work: workspace-write sol writer rewrites the fences and lands the fixlog', model: 'sonnet' },
        {
            title: 'Critique',
            detail: 'per implemented batch: workspace-write sol conformance audit repairs in place, fixlog to disk',
            model: 'sonnet',
        },
        {
            title: 'Redteam',
            detail: 'per implemented batch: workspace-write sol pre-mortem + cold re-review, folds critique deferred rows forward',
            model: 'sonnet',
        },
        { title: 'Drain', detail: 'one fable closer verifies and resolves the pooled cross-batch deferred rows against live disk', model: 'fable' },
    ],
};

// --- [CONSTANTS] -------------------------------------------------------------------------

const ROOT_DIR = '/Users/bardiasamiee/Documents/99.Github/Rasm';
const STALL = 300000;
const CODEX_STALL = 7500000; // wrapper stall sits ABOVE the client MCP ceiling: the client aborts a wedged call first; this guards only a dead wrapper
const CODEX = true; // every stage rides the codex wrapper; false restores native lanes throughout
const EXPLORE_CALLS = 60;
const WRITE_CALLS = 200;

// --- [INPUTS] ----------------------------------------------------------------------------

const absPath = (p) => (String(p).indexOf('/') === 0 ? String(p) : ROOT_DIR + '/' + String(p).replace(/^\/+/, ''));
// args normalizer: the runtime may hand `args` through as a JSON STRING (the documented stringified-args
// delivery), so a string is parsed once (guarded) into the structured form every later line reads directly.
let normArgs = args;
if (typeof normArgs === 'string') {
    try {
        normArgs = JSON.parse(normArgs);
    } catch {
        normArgs = null;
    }
}
const rawBatches = normArgs && typeof normArgs === 'object' && Array.isArray(normArgs.batches) ? normArgs.batches : [];
const BATCHES = rawBatches
    .filter((b) => b && typeof b === 'object' && typeof b.id === 'string' && Array.isArray(b.pages) && b.pages.length)
    .map((b) => ({
        id: String(b.id),
        pages: [...new Set(b.pages.filter(Boolean).map(absPath))].slice(0, 3),
        apis: Array.isArray(b.apis) ? [...new Set(b.apis.filter(Boolean).map(absPath))] : [],
        packages: Array.isArray(b.packages) ? [...new Set(b.packages.filter(Boolean).map(String))] : [],
        highDelta: !!b.high_delta,
    }));
const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH =
    '.claude/scratch/' +
    ('spec-uplift-' + BATCHES.map((b) => b.id).join('-')).replace(/[^a-z0-9.-]+/g, '-').slice(0, 60) +
    '-' +
    fnv1a(JSON.stringify(BATCHES));

// --- [MODELS] ----------------------------------------------------------------------------

const S = { type: 'string' };
const SL = { type: 'array', items: S };

const RECEIPT = {
    // Thin wire receipt: the lane's PRODUCT stays on disk at `report`; only status + count + headline travel inline.
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'headline', 'failure'],
    properties: { ok: { type: 'boolean' }, report: S, entries: { type: 'integer' }, headline: S, failure: S },
};

const RECEIPT_WORK = {
    // Explore receipt: RECEIPT plus the hasWork gate the implement stage branches on — mechanical lift of the product's own boolean.
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'headline', 'failure', 'hasWork'],
    properties: {
        ok: { type: 'boolean' },
        report: S,
        entries: { type: 'integer' },
        headline: S,
        failure: S,
        hasWork: { type: 'boolean' },
    },
};

const DOSSIER_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['pages', 'hasWork', 'summary'],
    properties: {
        pages: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'packages', 'api_anchors', 'corrections', 'improvements', 'ripples'],
                properties: {
                    page: S,
                    packages: SL,
                    api_anchors: SL,
                    corrections: {
                        type: 'array',
                        items: {
                            type: 'object',
                            additionalProperties: false,
                            required: ['fence_anchor', 'stale_symbol', 'what_changed'],
                            properties: { fence_anchor: S, stale_symbol: S, what_changed: S },
                        },
                    },
                    improvements: {
                        type: 'array',
                        items: {
                            type: 'object',
                            additionalProperties: false,
                            required: ['package', 'api_anchor', 'capability', 'why_denser'],
                            properties: { package: S, api_anchor: S, capability: S, why_denser: S },
                        },
                    },
                    ripples: SL,
                },
            },
        },
        hasWork: { type: 'boolean' },
        summary: S,
    },
};

const DEFERRED = {
    // Cross-batch ripple rows: the files slot is a LIST so a row spanning pages names them all — the drain clusters on it.
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['files', 'claim'],
        properties: { files: SL, claim: S },
    },
};

const FIXLOG_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['pages', 'deferred', 'summary'],
    properties: {
        pages: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'changes', 'adopted', 'corrected', 'verdict'],
                properties: {
                    page: S,
                    changes: SL,
                    adopted: SL, // prior-stage claims verified on disk and taken
                    corrected: SL, // prior-stage claims refuted or exceeded, with the evidence
                    verdict: { type: 'string', enum: ['rewritten', 'refined', 'fixed', 'clean'] },
                },
            },
        },
        deferred: DEFERRED,
        summary: S,
    },
};

const DRAIN_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['resolved', 'rejected', 'remaining', 'files', 'summary'],
    properties: {
        resolved: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['claim', 'action'], properties: { claim: S, action: S } },
        },
        rejected: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['claim', 'reason'], properties: { claim: S, reason: S } },
        },
        remaining: DEFERRED, // rows verified still-open and genuinely blocked, each naming its blocker
        files: SL,
        summary: S,
    },
};

// --- [DOCTRINE] --------------------------------------------------------------------------

const STACK_DIR = ROOT_DIR + '/docs/stacks/typescript';

const budgetBlock = (calls) =>
    '<context_gathering>\nTerritory: the exact files and directories the task names. Instruction files (.claude/, CLAUDE.md, AGENTS.md) and skill ' +
    'bundles are out of scope. Budget: at most ' +
    calls +
    ' tool calls total. Read in small batches; never concatenate the territory into one command — tool output truncates and the data is lost. ' +
    'Stop as soon as the product is complete; if something is still uncertain at the budget, proceed and record it in the product instead of ' +
    're-reading.\n</context_gathering>';

const contractBlock = (schema) =>
    '<output_contract>\nYour final message is a single JSON object with exactly this shape: ' +
    JSON.stringify(schema) +
    '\n- JSON only: no prose before or after it, no code fences, no markdown.\n- Every key shown is required; use null for a value you could not ' +
    'determine and [] for an empty list; never guess.\n</output_contract>';

const cadenceBlock =
    "<work_cadence>\nRead the stable law corpus once, first; then work PAGE BY PAGE — derive one page's edits, land them, advance. Edits land as " +
    'derived and never pool toward the end. A stable input — a doctrine page, catalog, fixlog — is read ONCE; re-open only the exact span behind ' +
    'an edit. At the budget, land what is derived and record every remaining row in the fixlog deferred field.\n</work_cadence>';

const verifyWriteBlock =
    '<verification>\nAfter editing, re-read each changed page and confirm it is coherent and nothing it carried was lost; fix what fails before ' +
    'yielding. Verification is READING: the corpus is markdown design pages — never compile, build, or run test gates against it; member truth ' +
    'rides the named .api catalogs and the published types under node_modules/ only.\n</verification>';

const EXPLORE_LAW =
    budgetBlock(EXPLORE_CALLS) +
    '\n\n<mapping_law>\nThis is a read-only mapping lane: the product is information — locations, verified member spellings, capability the ' +
    'packages admit — never prescriptions. No code sketches, no prescribed shapes, no repair instructions: a correction names the stale symbol ' +
    'and what changed, an improvement names the member and the capability it adds, and the implementer owns every design decision.\n</mapping_law>' +
    '\n\n<verification>\nBefore the final message, confirm every cited symbol appears verbatim in the cited catalog or package types; anything ' +
    'unconfirmed is dropped, never asserted.\n</verification>';

const IMPL_LAW =
    '<layer_pin>\nThis is an implementation lane: rewrite the named spec pages in place. Never migrate into research or review, and never re-open ' +
    'design questions beyond the named pages; an out-of-scope discovery lands as a deferred row in the fixlog, never as an edit outside them.' +
    '\n</layer_pin>' +
    '\n\n<read_ladder>\n(1) Read every root page of ' +
    STACK_DIR +
    '/ IN FULL at source — the quality floor for every fence, never summarized or skipped. (2) Read the batch spec pages and their .api catalogs ' +
    'and form your own defect and improvement list from the pages on disk BEFORE opening any report. (3) Only then read the explore dossier at ' +
    'the path the task names — unverified prior claims from another engineer: verify each against current disk, adopt what confirms, and exceed ' +
    'it where your own pass found more.\n</read_ladder>' +
    '\n\n<design_and_scope_constraints>\nEdit only the named pages plus the fixlog report file. Rewrite fences ground-up rather than patching: ' +
    'weave new package capability into the owning shape as if it had always existed; densify without deleting capability; break API freely when ' +
    'the collapse improves the design; one canonical owner per concept; exhaustive dispatch on every discriminated union; Effect rails and Schema ' +
    'boundaries at every edge; zero any, throw, or enum. Choose the simplest valid interpretation of any ambiguity and proceed.' +
    '\n</design_and_scope_constraints>' +
    '\n\n' +
    cadenceBlock +
    '\n\n' +
    budgetBlock(WRITE_CALLS) +
    '\n\n' +
    verifyWriteBlock;

const CRIT_LAW =
    "<layer_pin>\nThis is a review lane holding a writer's full authority: audit the named pages and repair every confirmed miss in place. Scope " +
    'bounds where you look first, never what you may fix inside the named pages; a cross-batch need lands as a deferred row, never a foreign edit.' +
    '\n</layer_pin>' +
    '\n\n<read_ladder>\n(1) Read every root page of ' +
    STACK_DIR +
    '/ IN FULL at source. (2) COLD PASS: derive your own conformance defect list from the pages on disk before opening any prior claim. (3) The ' +
    "implement fixlog at the path the task names is another engineer's unverified claims — check each against disk, never treat it as a settled " +
    'record.\n</read_ladder>' +
    '\n\n<audit_law>\nPredicate-positive conformance: verify each required law of the doctrine holds on each page — rails, boundaries, owner ' +
    'shape, dispatch exhaustiveness, typing, naming, section order, capability completeness — cite the clause, and repair every miss in place. ' +
    "Authority is a rebuilder's: collapse parallel shapes into stronger owners and extend thin owners to the full domain their concept carries. " +
    'A clean verdict after a pass that finds nothing is a first-class result; never invent an edit.\n</audit_law>' +
    '\n\n' +
    cadenceBlock +
    '\n\n' +
    budgetBlock(WRITE_CALLS) +
    '\n\n' +
    verifyWriteBlock;

const RT_LAW =
    "<layer_pin>\nThis is the terminal review lane for its batch, holding a writer's full authority: attack the named pages, repair every " +
    "confirmed defect in place, and return the batch's one consolidated fixlog. A cross-batch need lands as a deferred row, never a foreign edit." +
    '\n</layer_pin>' +
    '\n\n<read_ladder>\n(1) Read every root page of ' +
    STACK_DIR +
    '/ IN FULL at source. (2) COLD PASS: derive your own defect list from the pages on disk before opening any prior claim.\n</read_ladder>' +
    '\n\n<attack_law>\nPredicate-negative pre-mortem on each page: counterfactual the core owner and dispatch shape — would a different owner ' +
    'collapse the page; diff-of-the-next-feature — one new case, field, or policy row must land as one declaration inside the owner; long-tail ' +
    'and failure-mode attack — empty, singular, plural, stream, malformed, concurrent, cancelled, partial-failure; boundary and ownership ' +
    'integrity. Then one full cold re-review of every conformance dimension by name — rails, boundaries, owner shape, dispatch exhaustiveness, ' +
    'typing, naming, section order, capability completeness. A clean verdict after a failed attack is a first-class result; never invent an edit.' +
    '\n</attack_law>' +
    '\n\n<fold_forward>\nThe critique fixlog at the path the task names carries unverified deferred rows: re-verify each against current disk, ' +
    "resolve what the named pages own, and fold every surviving cross-batch row into your own fixlog deferred field — your fixlog is the batch's " +
    'one consolidated record.\n</fold_forward>' +
    '\n\n' +
    cadenceBlock +
    '\n\n' +
    budgetBlock(WRITE_CALLS) +
    '\n\n' +
    verifyWriteBlock;

// --- [OPERATIONS] ------------------------------------------------------------------------

const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const reportRel = (label) => SCRATCH + '/' + fileTag(label) + '-report.json';

// Codex dispatch: the sonnet wrapper makes one blocking Codex MCP call — call, write, verify, receipt; never the work itself.
const codexPrompt = (task, o) => {
    const base = reportRel(o.label);
    const report = ROOT_DIR + '/' + base;
    return [
        'DISPATCH ROLE: ' +
            o.model +
            ' performs the complete TASK below through one blocking Codex MCP call. Follow exactly four steps; never perform, edit, judge, ' +
            'soften, summarize, or relay the task yourself.',
        '(1) Call ToolSearch with query "select:mcp__codex__codex".',
        '(2) Call the loaded mcp__codex__codex tool ONCE with model="' +
            o.model +
            '", sandbox=' +
            (o.writes ? '"workspace-write"' : '"read-only"') +
            ', cwd=' +
            JSON.stringify(ROOT_DIR) +
            (o.codexEffort ? ', config={"model_reasoning_effort":"' + o.codexEffort + '"}' : '') +
            ', "developer-instructions" set to the LANE LAW block below VERBATIM, and prompt set to the TASK block below VERBATIM. If the call ' +
            'errors with a TIMEOUT or idle abort, the server aborts the codex turn — ' +
            (o.writes
                ? 'edits already landed on disk stay landed, but the final message and report are gone: check the report ONCE with `jq -e . ' +
                  report +
                  '`; if present, proceed to step (4) from its content; if absent, skip step (3) and return ok=false through step (4) with the ' +
                  'error text plus "report absent" — NEVER re-dispatch (a duplicate concurrent writer races the landed edits); the orchestrator ' +
                  'owns recovery. Only a NON-timeout error retries the identical call ONCE.'
                : 'a TIMEOUT means the lane was over-scoped: return ok=false through step (4), never an identical retry; a NON-timeout error ' +
                  'retries the identical call ONCE.') +
            ' If the retry errors, skip step (3) and return the error through step (4).',
        'LANE LAW:\n\n' + o.law + '\n\n' + contractBlock(o.product),
        'TASK:\n\n' +
            task +
            (o.writes
                ? '\n\nREPORT FILE (final act): before returning your final message, write that COMPLETE final-message JSON verbatim to ' +
                  report +
                  ' yourself.'
                : ''),
        o.writes
            ? '(3) The lane wrote the report itself. Verify with one Bash call: jq -e . ' +
              report +
              ' >/dev/null. If the file is missing or invalid, extract the CONTENT text from the tool result envelope {threadId, content} and ' +
              'Write it to that path verbatim (the product JSON, never the envelope), then re-verify.'
            : '(3) The tool result is a JSON envelope {threadId, content} whose content field holds the final-message text. Write that CONTENT ' +
              'text (the product JSON, unescaped) — never the envelope — with the Write tool to this absolute path: ' +
              report +
              '. Do not normalize, reformat, summarize, or extract the text before writing it. Then verify with one Bash call: jq -e . ' +
              report +
              ' >/dev/null — a Write that drops the tail mints invalid JSON; on failure rewrite once from the tool result, and a second failure ' +
              'returns through step (4) with the error.',
        '(4) Parse the tool result text only for mechanical orchestration data. Return ok=true, report=' +
            base +
            ', entries=the length of result["' +
            o.hl +
            '"], headline="<entries> ' +
            o.hl +
            ' | top: <first page basename or none>"' +
            (o.hasWork ? ', hasWork=result["hasWork"] === true' : '') +
            ', and failure empty. On a second tool error return ok=false, entries=0' +
            (o.hasWork ? ', hasWork=false' : '') +
            ', report and headline empty, and failure equal to the error text VERBATIM.',
    ].join('\n\n');
};

// QUOTA FALLBACK target: the same task re-dispatched natively at the role's Claude twin (terra -> opus, sol -> fable); the
// wrapper never executes work itself, so the caller owns this re-dispatch.
const nativeLane = (task, o) =>
    agent(
        task +
            '\n\nLANE LAW:\n\n' +
            o.law +
            '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
            ROOT_DIR +
            '/' +
            reportRel(o.label) +
            ' (Write tool, exactly this absolute path): ' +
            JSON.stringify(o.product) +
            ' — then return ONLY the receipt: ok, report = ' +
            reportRel(o.label) +
            ' (this repo-relative form, matching codex-lane receipts), entries = the length of the product ' +
            o.hl +
            ' array, one-line mechanical headline, failure empty' +
            (o.hasWork ? ', hasWork mirroring the product hasWork boolean' : '') +
            '.',
        { label: o.label, phase: o.phase, model: o.nativeModel, effort: 'high', schema: o.receipt, stallMs: STALL },
    );

const lane = (task, o) =>
    (CODEX
        ? agent(codexPrompt(task, o), {
              label: (o.model.indexOf('-sol') >= 0 ? 'sol:' : 'terra:') + o.label,
              phase: o.phase,
              model: 'sonnet',
              effort: 'low',
              schema: o.receipt,
              stallMs: CODEX_STALL,
          }).then((r) => (r && !r.ok && /usage|quota|limit/i.test(r.failure || '') ? nativeLane(task, o) : r))
        : nativeLane(task, o)
    ).then((r) => ({
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'lane died'),
        hasWork: !!(r && r.hasWork),
    }));

const batchBlock = (b) =>
    'Spec pages (each read IN FULL): ' +
    b.pages.join(', ') +
    '\nAPI catalogs (each read IN FULL): ' +
    b.apis.join(', ') +
    '\nPackages just updated: ' +
    b.packages.join(', ');

const explorePrompt = (b) =>
    '<task>\nMap the API delta and the unexploited capability for one batch of libs/typescript planning spec pages after a package version bump. ' +
    'Each page is a design document whose TypeScript code fences compose the packages below; the bump may have made cited symbols stale, and the ' +
    'new versions may admit capability no fence exploits yet.\n</task>' +
    '\n\n<territory>\n' +
    batchBlock(b) +
    '\nOne hop further: a sibling spec page that a batch page names as a seam may be opened to trace a ripple; no page beyond one hop.' +
    (b.highDelta
        ? '\nThe bump is flagged high-delta: expect real signature and capability movement in these packages.'
        : '\nThe bump is flagged low-delta: an unchanged used surface yields empty corrections and hasWork=false.') +
    '\nSymbol source of truth: the named .api catalogs first, then the published types under node_modules/ for a member a catalog omits.' +
    '\n</territory>' +
    '\n\n<product>\nOne dossier entry per spec page: the packages it composes, api_anchors (the catalog sections read), corrections (each a ' +
    'fence_anchor citing a stale symbol, the stale_symbol spelling, and what_changed in the current version), improvements (each a package ' +
    "member with its api_anchor, the capability it adds, and why the page's owning shape is denser with it), and ripples (the sibling pages the " +
    'corrections touch). Set hasWork=true only when at least one page carries a real correction or a material improvement; a bumped package ' +
    'whose used surface is unchanged is not work.\n</product>';

const implementPrompt = (b, ex) =>
    '<task>\nRewrite the TypeScript code fences of the spec pages below at the docs/stacks/typescript floor: correct every API drift the package ' +
    'version bump introduced and weave the newly admitted package capability into the owning shapes.\n</task>' +
    '\n\n<territory>\n' +
    batchBlock(b) +
    '\nQuality floor (read first, at source): every root page of ' +
    STACK_DIR +
    '/.' +
    (ex && ex.ok
        ? '\nExplore dossier (unverified prior claims — open only AFTER your own pass): ' + ROOT_DIR + '/' + ex.report
        : '\nNo explore dossier landed for this batch: your own pass is the sole ground; derive the delta from the catalogs yourself.') +
    '\n</territory>' +
    '\n\n<product>\nThe edited pages on disk, plus the fixlog JSON: per page its changes, adopted (dossier claims verified on disk and taken), ' +
    'corrected (dossier claims refuted or exceeded, with the evidence), and verdict; deferred carries only cross-batch ripple rows a sibling ' +
    'page outside this batch owns, each row listing every file it spans.\n</product>';

const critiquePrompt = (b, impl) =>
    "<task>\nAudit the spec pages below against the docs/stacks/typescript doctrine after another engineer's uplift pass, and repair every " +
    'confirmed miss in place.\n</task>' +
    '\n\n<territory>\n' +
    batchBlock(b) +
    '\nDoctrine (read first, at source): every root page of ' +
    STACK_DIR +
    '/.\nImplement fixlog (unverified prior claims — open only AFTER your cold pass): ' +
    ROOT_DIR +
    '/' +
    impl.report +
    '\n</territory>' +
    '\n\n<product>\nThe repaired pages on disk, plus the fixlog JSON: per page its changes, adopted (prior claims that held), corrected (prior ' +
    'claims refuted, with the evidence), and verdict; deferred carries only cross-batch ripple rows, each row listing every file it spans.' +
    '\n</product>';

const redteamPrompt = (b, crit) =>
    '<task>\nRun the terminal adversarial pass over the spec pages below after two prior passes, repair every confirmed defect in place, and ' +
    "return the batch's consolidated fixlog.\n</task>" +
    '\n\n<territory>\n' +
    batchBlock(b) +
    '\nDoctrine (read first, at source): every root page of ' +
    STACK_DIR +
    '/.' +
    (crit && crit.ok
        ? '\nCritique fixlog (unverified prior claims and deferred rows — open only AFTER your cold pass): ' + ROOT_DIR + '/' + crit.report
        : '\nNo critique fixlog landed for this batch: your own attack is the sole ground, and your deferred field starts empty.') +
    '\n</territory>' +
    '\n\n<product>\nThe repaired pages on disk, plus the consolidated fixlog JSON: per page its changes, adopted, corrected, and verdict; ' +
    'deferred folds your own surviving cross-batch rows together with the critique rows you re-verified and could not resolve here, each row ' +
    'listing every file it spans.\n</product>';

const drainPrompt = (rtReports, orphanReports) =>
    'TERMINAL DRAIN: you are the one closer for a spec-uplift run over libs/typescript planning pages. Every batch chain has finished; its ' +
    'consolidated fixlog sits on disk. Read each fixlog below IN FULL from disk.\n\nConsolidated redteam fixlogs (deferred rows to drain): ' +
    (rtReports.join(', ') || 'none') +
    '\nOrphaned critique fixlogs (their redteam died; every row unverified): ' +
    (orphanReports.join(', ') || 'none') +
    '\n\nEvery deferred row is an unverified claim about a cross-batch seam ripple. Re-verify each against CURRENT disk first — a row a later ' +
    'stage already resolved is culled with its receipt, never re-fixed. Fix each confirmed row at its root across every file the row names, at ' +
    'the docs/stacks/typescript bar (read the root doctrine pages at source before editing): both ends of a seam repaired in the same pass, new ' +
    'capability woven into the owning shape, zero any/throw/enum, exhaustive dispatch. A row genuinely blocked lands in remaining with its ' +
    'blocker named. Return only the drained ledger.';

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!BATCHES.length) return { batches: 0, implemented: 0, no_op: 0, deferred_drained: 0, failures: [] };

log(BATCHES.length + ' batch(es) — scratch ' + SCRATCH);

const results = (
    await pipeline(
        BATCHES,
        (b) =>
            lane(explorePrompt(b), {
                label: 'explore:' + b.id,
                phase: 'Explore',
                model: 'gpt-5.6-terra',
                nativeModel: 'opus',
                codexEffort: 'high',
                writes: false,
                law: EXPLORE_LAW,
                product: DOSSIER_SCHEMA,
                receipt: RECEIPT_WORK,
                hasWork: true,
                hl: 'pages',
            }).then((ex) => ({ b, ex })),
        async (prev, b) => {
            if (!prev) return null;
            // Anti-churn gate: a landed explore proving the bump changed nothing skips the writer chain entirely.
            if (prev.ex.ok && !prev.ex.hasWork) {
                log(b.id + ' — no work (bump left the used surface unchanged)');
                return Object.assign({}, prev, { noop: true, impl: null });
            }
            const impl = await lane(implementPrompt(b, prev.ex), {
                label: 'impl:' + b.id,
                phase: 'Implement',
                model: 'gpt-5.6-sol',
                nativeModel: 'fable',
                writes: true,
                law: IMPL_LAW,
                product: FIXLOG_SCHEMA,
                receipt: RECEIPT,
                hl: 'pages',
            });
            return Object.assign({}, prev, { noop: false, impl });
        },
        async (prev, b) => {
            if (!prev) return null;
            if (prev.noop || !prev.impl || !prev.impl.ok) return prev; // failure isolation: a dead implement skips its reviews
            const crit = await lane(critiquePrompt(b, prev.impl), {
                label: 'crit:' + b.id,
                phase: 'Critique',
                model: 'gpt-5.6-sol',
                nativeModel: 'fable',
                writes: true,
                law: CRIT_LAW,
                product: FIXLOG_SCHEMA,
                receipt: RECEIPT,
                hl: 'pages',
            });
            return Object.assign({}, prev, { crit });
        },
        async (prev, b) => {
            if (!prev) return null;
            if (prev.noop || !prev.impl || !prev.impl.ok) return prev;
            // Redteam entries count DEFERRED rows — the mechanical non-empty signal the drain gate reads off the receipt.
            const rt = await lane(redteamPrompt(b, prev.crit), {
                label: 'rt:' + b.id,
                phase: 'Redteam',
                model: 'gpt-5.6-sol',
                nativeModel: 'fable',
                writes: true,
                law: RT_LAW,
                product: FIXLOG_SCHEMA,
                receipt: RECEIPT,
                hl: 'deferred',
            });
            return Object.assign({}, prev, { rt });
        },
    )
).filter(Boolean);

// --- [DRAIN]

phase('Drain');
const rtReports = results.filter((r) => r.rt && r.rt.ok && r.rt.entries > 0).map((r) => ROOT_DIR + '/' + r.rt.report);
const orphanReports = results.filter((r) => r.crit && r.crit.ok && (!r.rt || !r.rt.ok)).map((r) => ROOT_DIR + '/' + r.crit.report);
let drain = null;
if (rtReports.length || orphanReports.length) {
    drain = await agent(drainPrompt(rtReports, orphanReports), {
        label: 'drain',
        phase: 'Drain',
        model: 'fable',
        effort: 'high',
        schema: DRAIN_SCHEMA,
        stallMs: STALL,
    });
} else {
    log('no cross-batch deferred rows — drain skipped');
}

const stageFailures = results.flatMap((r) => {
    const f = [];
    if (r.ex && !r.ex.ok) f.push({ id: r.b.id, stage: 'explore', failure: r.ex.failure });
    if (r.impl && !r.impl.ok) f.push({ id: r.b.id, stage: 'implement', failure: r.impl.failure });
    if (r.crit && !r.crit.ok) f.push({ id: r.b.id, stage: 'critique', failure: r.crit.failure });
    if (r.rt && !r.rt.ok) f.push({ id: r.b.id, stage: 'redteam', failure: r.rt.failure });
    return f;
});
const dropped = BATCHES.filter((b) => !results.some((r) => r.b.id === b.id)).map((b) => ({
    id: b.id,
    stage: 'pipeline',
    failure: 'stage threw; batch dropped',
}));

return {
    batches: BATCHES.length,
    implemented: results.filter((r) => r.impl && r.impl.ok).length,
    no_op: results.filter((r) => r.noop).length,
    deferred_drained: drain ? (drain.resolved || []).length : 0,
    failures: stageFailures.concat(dropped),
};
