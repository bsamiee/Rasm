export const meta = {
    name: 'brief',
    description:
        'Durable polyglot campaign-brief author over libs/{python,csharp,typescript} planning corpora. args = {targets, upstream, deep, mandate, review, gold} — targets a folder path or an ORDERED array (a waterfall: each later brief consumes every earlier one as finalized law with surgical ripple authority back); upstream = pre-existing finalized brief paths (any language) joining the corpus; deep = true or a target-path subset gaining 2 OSS-ecosystem research lanes; mandate = a scope-expansion law string for all targets or a {targetPath: text} map; review = extra brief paths for the terminal cross-corpus review, or false to skip it; gold = the exemplar brief (default RASM-PY-ARTIFACTS-BRIEF.md). Per target: 5 surveyors (corpus halves + api/manifest tiers + seam/consumer census + cross-folder strata census; +2 deep lanes) all on gpt-5.6-terra via codex dispatch wrappers (sonnet shells; surveyors write dossiers workspace-write, deep lanes add live web search; CODEX flag false restores native lanes; every lane leaves its dossier + typed report on disk and returns a thin receipt) -> 1 author (a single-phase decision-complete brief that never requires a second document, carrying the bidirectional CROSS_FOLDER enablement section, the section-utility anti-chaff law, and the header campaign law) -> 4 sequential adversarial passes (architecture, capability incl. the cross-folder audit, roster under the integration-first/seal-challenge/package-waterfall laws, cold-read + hedge-kill + chaff-sweep + RIPPLE AUDIT re-verifying every claimed upstream edit on disk). Terminal: when 1+ briefs were produced, 3 sequential review passes (initial/critique/redteam) cross-align the WHOLE corpus in place. Output naming RASM-<PY|CS|TS>-<NAME>-BRIEF.md. 10-12 agents per target + 3 review.',
    whenToUse:
        'The standing brief engine: author one brief, or a dependency-ordered waterfall of them, in any language mix, with the cross-corpus review built in. Empty args = no-op.',
};

// --- [CONSTANTS] -------------------------------------------------------------------------

const STALL = 480000;
const CODEX_STALL = 1500000; // wrapper stall sits above the xhigh blocking-call ceiling (1200s): a silent live MCP call is legal waiting, never a stall
const CODEX = true; // survey/strata + deep research lanes run on gpt-5.6-terra via the codex wrapper; false restores native lanes
const SCRATCH = '.claude/scratch/brief'; // per-lane MCP reports and dossiers

const LANG = {
    python: {
        tag: 'PY',
        doctrine: 'docs/stacks/python/',
        tiers: 'libs/python/.api/ (branch substrate) + the folder .api/ (domain)',
        manifest: 'the root pyproject.toml — lean unpinned names, bounds only on resolver evidence, one owning manifest',
        verify: 'PyPI JSON + two corroborating web sources per candidate (license, wheels incl. the <3.15 band where relevant, maintenance); paid/gated/proprietary REJECTED',
        law:
            'the shared-tier weave is corpus law: expression tagged_union/Result/Option/Block/Map as the one ADT and dispatch spine, anyio at ' +
            'concurrent seams, runtime guarded/railed retry over the POLICY rows, msgspec one-shot ingress, beartype public entries',
    },
    csharp: {
        tag: 'CS',
        doctrine: 'docs/stacks/csharp/ plus the docs/stacks/csharp/domain/ shards',
        tiers: 'libs/csharp/.api/ (shared substrate) + the package .api/ (domain)',
        manifest: 'Directory.Packages.props (hand-edited, label-grouped; never dotnet add) + the target .csproj',
        verify:
            'uv run python -m tools.assay api over restored assemblies (member truth, verified-local wins) + the nuget MCP (feed truth) + two ' +
            'corroborating web sources; license gate enforced (OSS or free-for-OSS commercial; pay-tiered/seat-licensed/proprietary-gated REJECTED)',
        law:
            'the WORKSPACE_LAW strata govern placement: KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP, depending strictly upward; ' +
            'AEC peers never reference each other',
    },
    typescript: {
        tag: 'TS',
        doctrine: 'docs/stacks/typescript/',
        tiers: 'libs/typescript/.api/ (branch) + the folder .api/ (domain)',
        manifest: 'pnpm-workspace.yaml + the workspace catalog — central version ownership, no per-package drift',
        verify: 'the npm registry + two corroborating web sources per candidate (license, types, maintenance); paid/gated/proprietary REJECTED',
        law: 'the Effect-native platform doctrine governs: services/layers/runtime wiring, one rail, schema-first boundaries per docs/stacks/typescript',
    },
};

// --- [INPUTS] ----------------------------------------------------------------------------

// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = typeof args === 'string' && /^\s*[\[{]/.test(args) ? JSON.parse(args) : args;
const TARGETS =
    typeof argsIn === 'string' && argsIn.trim()
        ? [argsIn.trim().replace(/\/+$/, '')]
        : Array.isArray(argsIn)
          ? argsIn.filter((t) => typeof t === 'string' && t.trim()).map((t) => t.trim().replace(/\/+$/, ''))
          : argsIn && typeof argsIn === 'object' && argsIn.targets
            ? (Array.isArray(argsIn.targets) ? argsIn.targets : [argsIn.targets])
                  .filter((t) => typeof t === 'string' && t.trim())
                  .map((t) => t.trim().replace(/\/+$/, ''))
            : [];
const UPSTREAM =
    argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.upstream) ? argsIn.upstream.filter((u) => typeof u === 'string' && u.trim()) : [];
const DEEP = argsIn && typeof argsIn === 'object' ? argsIn.deep : false;
const MANDATE = argsIn && typeof argsIn === 'object' ? argsIn.mandate : '';
const REVIEW_EXTRA =
    argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.review) ? argsIn.review.filter((r) => typeof r === 'string' && r.trim()) : [];
const REVIEW_OFF = !!(argsIn && typeof argsIn === 'object' && argsIn.review === false);
const GOLD =
    argsIn && typeof argsIn === 'object' && typeof argsIn.gold === 'string' && argsIn.gold.trim() ? argsIn.gold.trim() : 'RASM-PY-ARTIFACTS-BRIEF.md';
const langOf = (t) => {
    const m = t.match(/libs\/(python|csharp|typescript)(\/|$)/);
    return m ? LANG[m[1]] : null;
};
const nameOf = (t) => ((t.split('/').pop() || '').replace(/^Rasm\.?/, '') || t.split('/').pop() || '').toUpperCase().replace(/[^A-Z0-9]/g, '');
const outOf = (t) => 'RASM-' + (langOf(t) || { tag: 'X' }).tag + '-' + nameOf(t) + '-BRIEF.md';
const deepFor = (t) => DEEP === true || (Array.isArray(DEEP) && DEEP.includes(t));
const mandateFor = (t) =>
    typeof MANDATE === 'string' ? MANDATE.trim() : MANDATE && typeof MANDATE === 'object' && typeof MANDATE[t] === 'string' ? MANDATE[t].trim() : '';

// --- [MODELS] ----------------------------------------------------------------------------

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

const SURVEY_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['dossier', 'entries', 'coverage', 'summary'],
    properties: {
        dossier: { type: 'string' }, // path to the lane's full markdown dossier
        entries: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['target', 'kind', 'files', 'info', 'anchors', 'members'],
                properties: {
                    target: { type: 'string' }, // page, seam, catalog, or package the entry grounds
                    kind: { type: 'string', enum: ['state', 'defect', 'pressure', 'seam', 'stacking', 'candidate', 'roster'] },
                    files: { type: 'array', items: { type: 'string' } }, // files the author must open for this entry
                    info: { type: 'string' }, // the evidence as prose fact; a candidate states the ruling it argues plus its fatal proofs
                    anchors: { type: 'array', items: ANCHOR }, // exact coordinates backing the entry
                    members: { type: 'array', items: { type: 'string' } },
                },
            },
        }, // verified member/package spellings backing a stacking or roster entry
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

// Thin wire receipt: the lane's PRODUCT stays on disk at `report`; only status + counts travel inline.
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

const AUTHOR_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['brief', 'verdict_count', 'evidence_rows', 'thesis'],
    properties: {
        brief: { type: 'string' },
        verdict_count: { type: 'number' },
        evidence_rows: { type: 'number' },
        thesis: { type: 'string' },
    },
};

const PASS_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['edits', 'findings', 'roster_changes', 'upstream_ripples', 'ripple_audit', 'final_verdict', 'top_risks'],
    properties: {
        edits: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['section', 'what', 'why'],
                properties: { section: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } },
            },
        },
        findings: { type: 'array', items: { type: 'string' } },
        roster_changes: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['action', 'package', 'concern', 'verification'],
                properties: {
                    action: { type: 'string', enum: ['ADD', 'REMOVE', 'REPLACE', 'INTEGRATE'] },
                    package: { type: 'string' },
                    concern: { type: 'string' },
                    verification: { type: 'string' },
                },
            },
        },
        upstream_ripples: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['brief', 'what', 'why'],
                properties: { brief: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } },
            },
        },
        ripple_audit: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['claimed', 'verdict'],
                properties: { claimed: { type: 'string' }, verdict: { type: 'string', enum: ['LANDED', 'APPLIED-BY-ME', 'CORRECTED'] } },
            },
        },
        final_verdict: { type: 'string' },
        top_risks: { type: 'array', items: { type: 'string' } },
    },
};

const REVIEW_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['edits', 'opportunities', 'alignments', 'final_verdict', 'residual_risks'],
    properties: {
        edits: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['brief', 'what', 'why'],
                properties: { brief: { type: 'string' }, what: { type: 'string' }, why: { type: 'string' } },
            },
        },
        opportunities: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['enabler', 'consumer', 'what'],
                properties: { enabler: { type: 'string' }, consumer: { type: 'string' }, what: { type: 'string' } },
            },
        },
        alignments: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['seam', 'fix'],
                properties: { seam: { type: 'string' }, fix: { type: 'string' } },
            },
        },
        final_verdict: { type: 'string' },
        residual_risks: { type: 'array', items: { type: 'string' } },
    },
};

// --- [DOCTRINE] --------------------------------------------------------------------------

const ROSTER_LAW =
    'PACKAGE ROSTER LAW: central version ownership per the language manifest; per-package catalogs live in the .api tiers and every ' +
    'admission/removal carries its catalog motion. The roster is OPEN — modern/bleeding-edge additions, verified removals, and categorical-best ' +
    'replacements are all in scope, one categorical-best owner per concern. INTEGRATION-FIRST: an admitted package with zero page consumers is an ' +
    'integration MANDATE before it is ever a removal candidate — realize it as a row/arm on the owning polymorphic axis (named page, named row); a ' +
    'REMOVE requires proven redundancy (the concern already lands on a stronger admitted owner, named), feed-verified upstream abandonment, or a ' +
    'charter/license conflict — absence of consumers alone is NEVER a REMOVE reason. SEAL-CHALLENGE: a closed sweep, NEVER row, or rejection list ' +
    'in the target corpus is challengeable design pressure, not inherited law — a verdict may re-open it as a parameterized axis (provider rows on ' +
    'one closed dispatch surface); deference to an on-disk seal is argued from architecture, never assumed. PACKAGE WATERFALL: where a package ' +
    'reaches full power only with a counterpart elsewhere (an upstream seam, a decoder, a companion admission), record the enablement BOTH ' +
    'directions — upstream briefs gain the owner extension through the ripple authority, downstream folders gain a named forward-obligation row — ' +
    'so an addition lands clean across the chain, never a folder-local orphan. DOMAIN-GAP research is mandatory where the target scope demands ' +
    'capability no admitted package owns: name the concern + the categorical-best candidate + the binding surface, verified per the language rail. ' +
    'Never silently narrow the domain to the current roster.';

const CROSS_FOLDER_LAW =
    'CROSS-FOLDER LAW: high-value capability is never planned in isolation. The brief names enablement rows in RELATED ' +
    'FOLDERS — the realized corpora on disk, independent of any upstream brief — in BOTH directions: a base/lower-stratum extension that unlocks a ' +
    'stronger form in the target (the base folder gains a fully-specified IDEAS-row obligation, the target verdict composes it), and a target ' +
    'capability that opens doors in consumers or siblings (the consumer opportunity named with its seam). Every row binds at declared wire/seam ' +
    'boundaries — content keys, frozen wire names, entry/receipt ports — never a coupling to a sibling interior; cross-language rows bind at the ' +
    'wire only. A cross-folder row without a named seam is a defect.';

const CHAFF_LAW =
    'SECTION UTILITY LAW: the brief is LAW the rebuild engine executes directly — it never requires a second document, a follow-up ' +
    "design pass, or a DECISION file. Every section, verdict clause, evidence row, and table row must change the executing agent's behavior; " +
    'boilerplate framing, restated doctrine, generic methodology, empty filler sections, and prose that describes rather than rules are deleted on ' +
    'sight. A genuine evidence-gated hinge is DECIDED in the brief — a ruled default plus the deciding criteria that would flip it — never deferred.';

const RIPPLE_LAW =
    'CORPUS + WATERFALL LAW: the finalized briefs listed as CORPUS are law for this target — read each FULLY; this target is their ' +
    'CONSUMER (every upstream capability it could compose instead of hand-rolling is a named opportunity; every assumption an upstream brief ' +
    'changes is a named migration pressure), and cross-LANGUAGE corpus rows bind at wire/content-key seams only, decoded at the boundary, never a ' +
    'coupling. RIPPLE AUTHORITY: when THIS target demands a capability an upstream brief lacks, EDIT that brief surgically in place — an owner ' +
    'extension (a verdict clause, an evidence row, an escalation delta, a package row) framed as consumer pressure with this target named as the ' +
    'demanding consumer, EXTENDING its numbering (a new Vn/En), never a rewrite, never re-planning the upstream, and every such edit recorded in ' +
    'your return as upstream_ripples so the terminal audit can re-verify it on disk.';

const ENTRY_LAW =
    'REPORT FORM — the JSON report is the wire product the author consumes; the dossier carries the full prose. `entries` carry ' +
    'one fact, defect, pressure, seam, stacking gap, or verdict candidate each: `info` is prose evidence (a candidate states the ruling it argues ' +
    'plus its independently-fatal proofs — the AUTHOR owns the decision; a candidate is pressure with evidence, never settled law); `files` lists ' +
    'what the author must open for the entry; `anchors` carry one coordinate per row (role names what it proves; `note` is the shortest literal ' +
    'witness under 20 words, or empty when path+line suffice; an `absence` anchor names where the expected thing was searched and not found); ' +
    '`members` = verified member/package spellings backing a stacking or roster entry. COVERAGE is part of the product: `requested` = your ' +
    'assigned scope, `read` = what you actually full-read, `skipped`/`unverified` = what you did not reach — an honest skip beats a silent one.';

const preOf = (t, corpus) => {
    const L = langOf(t),
        m = mandateFor(t);
    return (
        'Rasm monorepo. Target: ' +
        t +
        '/.planning/ (markdown design pages ' +
        'of intended code; the fences are the product). ' +
        L.doctrine +
        ' governs every fence; ' +
        L.law +
        '; libs/.planning/architecture.md governs ' +
        'the cross-branch map. Both .api tiers are member truth: ' +
        L.tiers +
        '. Manifest: ' +
        L.manifest +
        '. Verification rail: ' +
        L.verify +
        '. ' +
        'Your stance is HOSTILE: assume the corpus is naive with illusory depth until disk proves otherwise; NAIVETY is a defect on two axes — ' +
        'COVERAGE (a thin slice of the owned domain) and APPROACH (enumerated instances where a parameterized generator belongs; rosters are seed ' +
        'DATA). Hunt architectural/flow/logic fundamental-approach problems, underutilized admitted capability, concern mixing, duplicate mechanisms, ' +
        'dead typed carriers, hardcoding, prose-vs-fence splits (a capability claimed in prose but absent from the fence is ILLUSORY), unwired ' +
        'declared seams, parallel rails where one owner belongs, and folder-architecture rot (one-file folders, flat sprawl, structures not conducive ' +
        'to growth). ' +
        CROSS_FOLDER_LAW +
        ' ' +
        CHAFF_LAW +
        (corpus.length ? ' CORPUS (finalized briefs, dependency order): ' + JSON.stringify(corpus) + '. ' + RIPPLE_LAW : '') +
        (m ? '\nSCOPE MANDATE (binding): ' + m : '')
    );
};

// --- [OPERATIONS] ------------------------------------------------------------------------

// Codex dispatch: the sonnet wrapper makes one blocking Codex MCP call, writes the envelope's content
// to the lane report, and returns mechanical orchestration data. Lane law rides developer-instructions
// (role split, battery-validated); the prompt carries only the task; the output contract sits LAST.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const dossierOf = (label) => SCRATCH + '/' + fileTag(label) + '-dossier.md';
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
    const base = SCRATCH + '/' + fileTag(label);
    const root = '/Users/bardiasamiee/Documents/99.Github/Rasm';
    const report = root + '/' + base + '-report.json';
    const model = o.model || 'gpt-5.6-terra';
    const hl = o.hl || { arr: 'entries', group: 'kind' };
    return [
        'DISPATCH ROLE: ' +
            model +
            ' performs the complete TASK below through one blocking Codex MCP call. Follow exactly four steps; ' +
            'never perform, edit, judge, soften, summarize, or relay the task yourself.',
        '(1) Call ToolSearch with query "select:mcp__codex__codex". If one Bash probe shows command -v forge-fleet-emit ' +
            'resolving, run forge-fleet-emit --kind codex --model ' +
            model +
            ' --label ' +
            JSON.stringify(fileTag(label)) +
            ' --state start now and --state stop right after step (2); when the tool is absent skip both silently.',
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
        '(4) Parse the tool result text only for mechanical orchestration data. Return ok=true, report=' +
            base +
            '-report.json, entries=the length of result["' +
            hl.arr +
            '"], headline="<entries> ' +
            hl.arr +
            (hl.group ? ' | <' + hl.group + ' tallies>' : '') +
            ' | top: <most frequent first file or none>", and failure empty. On a second tool error return ok=false, entries=0, ' +
            'report and headline empty, and failure equal to the error text VERBATIM.',
    ].join('\n\n');
};
// Every survey/research lane routes here: terra by default; CODEX=false restores a fully native run. QUOTA
// FALLBACK: a codex receipt whose failure matches usage/quota/limit re-dispatches the SAME task natively at
// the role's Claude twin (terra->opus, sol->fable, luna->sonnet) — the caller owns the re-dispatch; the
// sonnet wrapper never executes work itself. The roster row carries `scope` from the ORCHESTRATOR (never the
// lane's self-report) so a failed lane's unmapped territory is exact even when it died before writing anything.
const twinOf = (m) => (/-sol/.test(m || '') ? 'fable' : /-luna/.test(m || '') ? 'sonnet' : 'opus');
const nativeLane = (task, o) =>
    agent(
        task +
            '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
            SCRATCH +
            '/' +
            fileTag(o.label) +
            '-report.json (Write tool, absolute path under the repo root): ' +
            JSON.stringify(o.schema) +
            ' — then return ONLY the receipt: ok, report path, entries count, one-line mechanical headline, failure empty.',
        { label: o.label, phase: o.phase, model: twinOf(o.model), effort: 'high', schema: RECEIPT, stallMs: o.stallMs || STALL },
    );
const recon = (task, o) =>
    (CODEX
        ? agent(codexPrompt(o.label, task, o.schema, o), {
              label: (o.model && o.model.indexOf('-sol') >= 0 ? 'sol:' : 'terra:') + o.label,
              phase: o.phase,
              model: 'sonnet',
              effort: 'low',
              schema: RECEIPT,
              stallMs: o.stallMs || CODEX_STALL,
          }).then((r) => (r && !r.ok && /usage|quota|limit/i.test(r.failure || '') ? nativeLane(task, o) : r))
        : nativeLane(task, o)
    ).then((r) => ({
        lane: o.label,
        scope: o.scope || [],
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'lane died'),
    }));
const surveyPrompt = (pre, dossier, lane, scope) =>
    [
        pre,
        ENTRY_LAW,
        'TASK: READ-ONLY SURVEY, lane = ' +
            lane +
            '. Scope: ' +
            scope +
            '. Deep-read fully — ' +
            'never skim. WRITE a dense dossier to ' +
            dossier +
            ' (Write tool, path resolved absolute under the repository root): per-page {verdict 1-10, ' +
            'defects with file:line, split/merge/move pressure, the owner charter as it SHOULD be}, cross-cutting {duplication, concern mixing, ' +
            'hardcoding-vs-generator, dead carriers, unwired seams, unmined capability with catalog anchors}, and the 5-10 strongest VERDICT CANDIDATES ' +
            '(campaign-defining structural rulings with evidence). Dense, evidence-first, zero narration. Your JSON report distills the dossier into typed ' +
            'entries — every verdict candidate (kind=candidate), every anchored defect and pressure, every seam, stacking, and state fact — with `dossier` ' +
            'set to ' +
            dossier +
            '. NO edits outside ' +
            SCRATCH +
            '/.',
    ].join('\n');
const deepPrompt = (pre, dossier, lane, focus) =>
    [
        pre,
        ROSTER_LAW,
        ENTRY_LAW,
        'TASK: ECOSYSTEM RESEARCH, lane = ' +
            lane +
            '. ' +
            focus +
            ' Web research ' +
            'with two corroborating sources per candidate and the license gate enforced; verify real, maintained, current. WRITE the dossier to ' +
            dossier +
            ' (Write tool, path resolved absolute under the repository root): per-candidate {package, concern owned, binding surface, license, ' +
            'maturity signals, verdict ADMIT-CANDIDATE/REFERENCE-ONLY/REJECT with the reason}. Your JSON report carries one kind=roster entry per ' +
            'candidate — `members` = the verified package spelling, `info` = concern + binding surface + license + verdict with reason, `anchors` empty ' +
            'where the evidence is web-only (the dossier carries the sources) — with `dossier` set to ' +
            dossier +
            '. NO edits outside ' +
            SCRATCH +
            '/.',
    ].join('\n');
const authorPrompt = (pre, t, out, roster, unmapped) =>
    [
        pre,
        ROSTER_LAW,
        'TASK: AUTHOR the campaign brief ' +
            out +
            ' (repo root) for the ground-up ' +
            'restructure+rebuild of ' +
            t +
            '/.planning/. GOLD STANDARD: read ' +
            GOLD +
            ' (repo root) COMPLETELY first — match its density, structure, ' +
            'and law-grade voice. The brief carries: [00]-SHARED_LAW (a VERDICT with independently-fatal proofs anchored file:line; TELOS with the ' +
            '5x-consumer bar; STRUCTURAL_AUTHORITY incl. split/merge/move/new-folder freedom; the placement/strata law; GENERATOR_LAW; the seam/entry/' +
            'rail law; roster reconciliation), [01] NUMBERED BINDING VERDICTS (V1..Vn — each a structural ruling with a recommended-shape floor and a ' +
            'ruled default where decidable NOW; a hedge carries its deciding criteria), [02] the EVIDENCE REGISTER (E-rows with file:line anchors + the ' +
            'sound-surfaces line), [03] CAPABILITY ESCALATION (per-plane now->target grades with concrete deltas), [04] PACKAGE_PRESSURE (mine-to-depth ' +
            'rows with stub anchors; roster rows under the roster law), [05] BUILD_LEGS (dependency-ordered legs + per-leg closeout + acceptance proofs), ' +
            '[06] CROSS_FOLDER (the bidirectional enablement rows per the cross-folder law — each row {folder, direction, capability, seam}, fully ' +
            'specified from the strata dossier and re-verified on disk; base-extension rows land as fully-specified IDEAS-row obligations the campaign ' +
            'applies), [07] OUT_OF_SCOPE. The brief is SINGLE-PHASE and decision-complete per the section utility law: the rebuild engine consumes it ' +
            'directly, no second document ever follows it, and every structural hinge is DECIDED — a ruled default plus the deciding criteria that would ' +
            'flip it. HEADER LAW: line 3 of the brief is the campaign line, 1-3 lines: its track order relative to the corpus, the Workflow invocation ' +
            '(rebuild.js args), and its one sequencing constraint. SOURCES — CONSUMPTION PROTOCOL, in order: (a) UNMAPPED scope below is your direct-hunt ' +
            "queue — a failed lane's territory gets your own cold read FIRST; (b) read every ok survey REPORT FILE IN FULL from disk, shared-surface " +
            "lanes (api-tiers, census, strata, ecosystem) before the corpus halves, and read each report's `dossier` markdown IN FULL alongside it; " +
            'entries overlap across lanes — cluster by target as you read; (c) entries are EVIDENCE with jump-coordinate anchors, never settled law: ' +
            're-verify on disk every anchor behind a claim the brief makes (MANDATORY); navigation-only entries re-verify only when touched; a verdict ' +
            'candidate is pressure you adopt, strengthen, or reject on your own authority. Plus the corpus as law. ' +
            'UNMAPPED: ' +
            JSON.stringify(unmapped) +
            ' ROSTER: ' +
            JSON.stringify(roster) +
            '. Every claim anchored; agent-facing declarative; no ' +
            'provenance, no hedging, no restated doctrine. Return the path + counts + a one-line thesis.',
    ].join('\n');
const passPrompts = (pre, brief) => [
    [
        pre,
        'TASK: ADVERSARIAL PASS 1 of 4 — ARCHITECTURE. You WRITE: fix and improve ' +
            brief +
            ' in place. Interrogate the brief AS AN ' +
            'ARCHITECTURE: do its verdicts compose into one coherent domain map (draw the post-campaign dependency graph — cycles, owners with two ' +
            'masters, fuzzy boundaries get boundary law); is the folder plan growth-conducive (no one-file folders, no flat sprawl; a weak plan is a ' +
            'defect you fix with a stronger one); is the leg order a true topological order (name and dispose every inversion on disk); are the strata/' +
            'placement calls right; is [01] executable without guessing. Verify every claim you add on disk. Never dilute; ~1.2x length cap.',
    ].join('\n'),
    [
        pre,
        'TASK: ADVERSARIAL PASS 2 of 4 — CAPABILITY/OUTPUT GRADE. You WRITE: fix and improve ' +
            brief +
            " in place. Walk the target's " +
            'FLAGSHIP outputs backward (the 2-3 hardest real deliverables a world-class version must produce, including what the corpus briefs now ' +
            'make possible) and find every chain break or vague link — each becomes a verdict extension, escalation delta, or named obligation, with ' +
            'honest ingress boundaries (what arrives from siblings/upstream vs computed here). Where a verdict or [03] row settles for parity or ' +
            'repair, RAISE it to what the world-class version owns, backed by real package surfaces (verify per the language rail; no vapor). Audit ' +
            '[06] CROSS_FOLDER against the flagship walk: every chain link that crosses a folder boundary either composes a named enablement row or ' +
            'gains one, both directions, seam-bound. Apply the ripple authority where a demand belongs upstream. Never dilute; ~1.25x cap.',
    ].join('\n'),
    [
        pre,
        ROSTER_LAW,
        'TASK: ADVERSARIAL PASS 3 of 4 — ROSTER + API ULTRA-STACKING. You WRITE: fix and improve ' +
            brief +
            ' in place. Inventory ' +
            'BOTH .api tiers + the manifest; for every stub the brief does not cite, judge mined-vs-unmined against the owning pages and add verified ' +
            'mine-to-depth rows (stub anchors mandatory); audit existing [04] rows against the stubs and fix contradicted spellings. Then the ROSTER ' +
            'OVERHAUL under the roster law: integration mandates for zero-consumer admissions, supersession sweep (categorical-best), DOMAIN-GAP ' +
            'research with ADD rows, every motion verified per the language rail. Never dilute; ~1.25x cap.',
    ].join('\n'),
    [
        pre,
        'TASK: ADVERSARIAL PASS 4 of 4 — COLD RE-READ + RIPPLE AUDIT, the last hands on this brief. You WRITE: fix in place. Read the ENTIRE ' +
            'brief twice, as a first reader and as the most hostile one: cross-reference closure (every verdict <-> evidence row <-> escalation delta ' +
            '<-> package row <-> leg assignment <-> cross-folder row; fix every dangling end); hedge-kill (where "the campaign decides" is decidable ' +
            'NOW, decide it with a ruled default; where genuinely open, verify deciding criteria exist — a two-phase deferral or DECISION-file pointer ' +
            'is a defect rewritten to a decided verdict); CHAFF SWEEP (delete every section, row, or clause that fails the section utility law); ' +
            'executability dry-run ([01]+[05] alone must run without guessing); ' +
            'HEADER check (line 3 carries track order + the invocation + the one constraint). RIPPLE AUDIT: re-verify ON DISK every upstream ' +
            'ripple the author and passes 1-3 claimed — read the upstream brief at the claimed site; a claimed-but-absent edit is APPLIED by you now ' +
            '(owner-extension form), a wrong one CORRECTED; record each in ripple_audit. Return the final verdict + top risks.',
    ].join('\n'),
];

const reviewPre = (scope) =>
    'Rasm monorepo. THE CORPUS, in dependency order (earlier = upstream foundation, later = consumer; languages may ' +
    'mix — cross-language rows bind at wire/content-key seams only): ' +
    JSON.stringify(scope) +
    ' — each a finalized campaign brief a rebuild ' +
    'workflow executes against its folder. Read EVERY brief COMPLETELY and in order before any edit. You WRITE: improvements land in the briefs ' +
    'in place as owner extensions (a verdict clause, an evidence row, an escalation delta, a package row, a leg obligation — never a parallel ' +
    'section, never a rewrite of settled law). LAWS: (1) FIT, NEVER COUPLED — a later brief composes an earlier folder at declared seams and ' +
    'never re-plans it; an earlier brief never reaches forward except as recorded consumer pressure; (2) ALIGNMENT is bidirectional — a seam one ' +
    'brief names and the counterpart is silent on is a defect fixed on BOTH sides; conflicting spellings/shapes for one seam resolve to one; ' +
    '(3) OPPORTUNITY law — where an earlier brief lands a capability that makes a stronger feature possible in a later folder, the later brief ' +
    'gains the row (with the enabling brief named as the seam), and where a later brief needs something no earlier brief provides, the earlier ' +
    'brief gains the consumer-pressure extension; (4) every claim you add is disk-verified or brief-anchored — no vapor; (5) each brief must ' +
    'remain internally closed after your edits (dispositions complete, partitions acyclic, hedges criteria-bearing); (6) every brief is single-phase and ' +
    "decision-complete — every edit preserves the brief's own header contract and numbered verdict/evidence grammar, EXTENDING the numbering " +
    '(a new Vn/En row) rather than restructuring it, and a deferral to a second document is a defect decided in place; ' +
    '(7) INTEGRATION-FIRST roster audit — re-judge every REMOVE/strike/' +
    'prune row across the corpus: zero consumers is an integration mandate, never a removal reason — a removal survives only on proven ' +
    'redundancy (stronger admitted owner named), feed-verified abandonment, or charter/license conflict, and one failing that bar rewrites to a ' +
    'realization row on its owning axis; record cross-folder package enablement both directions. The goal: the rebuild workflows inherit ' +
    'guidance with zero gaps, zero contradictions, and every cross-folder possibility named.';
const reviewPrompts = (scope) => [
    [
        reviewPre(scope),
        'TASK: PASS 1 of 3 — INITIAL HOLISTIC. First full read of the corpus as one program. Per brief: fix mistakes, close gaps, ' +
            'kill silences on concerns a sibling brief treats as load-bearing. Collectively: walk the dependency chain both directions — enumerate ' +
            'every predecessor-enabled opportunity no brief names yet and every consumer-demand hole (what a later brief assumes upstream that ' +
            'upstream never promises). Apply every finding in place.',
    ].join('\n'),
    [
        reviewPre(scope),
        'TASK: PASS 2 of 3 — CRITIQUE. Verify every pass-1 edit against disk and the briefs (an unanchored or wrong edit is ' +
            'repaired, never tolerated). Then the mechanical floor per brief: cross-reference closure, disposition completeness, seam-ledger ' +
            'consistency ACROSS briefs (one seam, one spelling, both sides), header campaign lines current, corpus references valid after pass-1 ' +
            'edits. Fix everything you find in place.',
    ].join('\n'),
    [
        reviewPre(scope),
        'TASK: PASS 3 of 3 — RED-TEAM COLD READ, the last hands on this corpus. Read the whole set twice as its future ' +
            'implementing agents will: hostile, fresh, lens-by-lens — counterfactual (would faithful execution actually produce world-class folders, ' +
            'or is a brief load-bearing on an unstated assumption?), long-tail (rare-but-real cases no brief covers), boundary (every cross-folder and ' +
            'cross-language seam honest?), sprawl (do two briefs quietly plan the same owner twice?), completeness (is any [03] target unreachable ' +
            'from the verdicts as written?). Fix in place; return the corpus verdict + residual risks.',
    ].join('\n'),
];

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!TARGETS.length) {
    log('brief: pass {targets, upstream?, deep?, mandate?, review?} — targets a path or ordered array. No-op.');
    return { targets: 0, produced: [] };
}
const bad = TARGETS.filter((t) => !langOf(t));
if (bad.length) {
    log('brief: unrecognized language root for ' + JSON.stringify(bad) + ' — targets must live under libs/{python,csharp,typescript}.');
    return { targets: TARGETS.length, produced: [], rejected: bad };
}

const corpus = [...UPSTREAM];
const produced = [];
for (let ti = 0; ti < TARGETS.length; ti++) {
    const t = TARGETS[ti];
    const L = langOf(t);
    const name = nameOf(t);
    const out = outOf(t);
    const pre = preOf(t, corpus);
    const P = L.tag + ':' + name.toLowerCase();
    const laneLabel = (lane) => 'survey:' + L.tag.toLowerCase() + '-' + name.toLowerCase() + ':' + lane;

    phase(P + ' survey');
    const surveyLanes = [
        {
            lane: 'corpus-a',
            scope: 'the FIRST half of the target .planning pages (alphabetical by path) FULLY, plus the folder README/ARCHITECTURE/TASKLOG/IDEAS where present',
        },
        {
            lane: 'corpus-b',
            scope: 'the SECOND half of the target .planning pages FULLY, plus every page the first half seams to at the depth fit requires',
        },
        {
            lane: 'api-tiers',
            scope:
                'BOTH .api tiers COMPLETE (' +
                L.tiers +
                ') + the manifest (' +
                L.manifest +
                '); judge every stub ' +
                'mined-vs-unmined against the owning pages; verify roster claims per the language rail where versions are cited',
        },
        {
            lane: 'census',
            scope:
                'the seam/consumer census: every cross-page and cross-package/cross-language edge the target carries, the ' +
                "governance surfaces (ledger/router/cards) vs realized truth, and the corpus briefs' clauses that name this target",
        },
        {
            lane: 'strata',
            scope:
                'the CROSS-FOLDER enablement census over realized corpora on disk (independent of any brief): the folders this ' +
                'target composes or feeds — its language kernel/base strata below, its consumers above, its cross-language wire counterparts — each read ' +
                'at the depth fit requires; per related folder return BOTH directions with the binding seam named: the base extension that would unlock a ' +
                'stronger form in the target, and the target capability that would open a door in that folder',
        },
    ];
    const deepLanes = deepFor(t)
        ? [
              {
                  lane: 'ecosystem-a',
                  focus:
                      "Sweep the OSS ecosystem for the target's CORE domain concerns: the categorical-best owners for capability " +
                      'the mandate/telos demands, judged against what the roster already admits.',
              },
              {
                  lane: 'ecosystem-b',
                  focus:
                      'Sweep the ADJACENT/emerging lanes: bleeding-edge or cross-domain packages that could raise the capability ' +
                      'ceiling, plus supersession candidates for weak admitted owners.',
              },
          ]
        : [];
    const roster = (
        await parallel([
            ...surveyLanes.map(
                (l) => () =>
                    recon(surveyPrompt(pre, dossierOf(laneLabel(l.lane)), l.lane, l.scope), {
                        label: laneLabel(l.lane),
                        phase: P + ' survey',
                        schema: SURVEY_SCHEMA,
                        writes: true,
                        scope: [l.scope],
                    }),
            ),
            ...deepLanes.map(
                (l) => () =>
                    recon(deepPrompt(pre, dossierOf(laneLabel(l.lane)), l.lane, l.focus), {
                        label: laneLabel(l.lane),
                        phase: P + ' survey',
                        schema: SURVEY_SCHEMA,
                        writes: true,
                        web: true,
                        scope: [l.focus],
                    }),
            ),
        ])
    ).filter(Boolean);
    const surveyed = roster.filter((r) => r.ok);
    const total = surveyed.reduce((a, r) => a + r.entries, 0);
    const unmapped = roster.filter((r) => !r.ok).flatMap((r) => r.scope.map((s) => ({ lane: r.lane, scope: s })));
    log(
        P +
            ' survey: ' +
            total +
            ' entries across ' +
            surveyed.length +
            '/' +
            roster.length +
            ' lanes' +
            (surveyed.length < roster.length
                ? ' — FAILED: ' +
                  roster
                      .filter((r) => !r.ok)
                      .map((r) => r.lane)
                      .join(', ')
                : ''),
    );

    phase(P + ' author');
    const authored = await agent(authorPrompt(pre, t, out, roster, unmapped), {
        label: 'author:' + name.toLowerCase(),
        phase: P + ' author',
        effort: 'high',
        schema: AUTHOR_SCHEMA,
        stallMs: STALL,
    });
    if (!authored) {
        log(P + ': author produced nothing — aborting this target; resume re-runs it.');
        continue;
    }
    log(P + ' author: ' + authored.brief + ' — ' + authored.verdict_count + ' verdicts, ' + authored.evidence_rows + ' E-rows');

    phase(P + ' refine');
    const PASS_LABELS = ['architecture', 'capability', 'roster', 'cold-read'];
    let lastPass = null;
    for (let i = 0; i < 4; i++) {
        const p = await agent(passPrompts(pre, authored.brief)[i], {
            label: 'pass:' + PASS_LABELS[i],
            phase: P + ' refine',
            effort: 'high',
            schema: PASS_SCHEMA,
            stallMs: STALL,
        });
        if (p) lastPass = p;
        log(
            P +
                ' pass ' +
                (i + 1) +
                '/4 (' +
                PASS_LABELS[i] +
                '): ' +
                (p ? (p.edits || []).length + ' edit(s), ' + (p.upstream_ripples || []).length + ' ripple(s)' : 'NO RESULT — rerun via resume'),
        );
    }
    produced.push({
        target: t,
        brief: authored.brief,
        thesis: authored.thesis,
        final_verdict: (lastPass && lastPass.final_verdict) || '',
        top_risks: (lastPass && lastPass.top_risks) || [],
    });
    corpus.push(authored.brief);
}

// --- [REVIEW]

let review = null;
if (produced.length && !REVIEW_OFF) {
    phase('review');
    const scope = [...corpus, ...REVIEW_EXTRA.filter((r) => !corpus.includes(r))];
    const REVIEW_LABELS = ['initial', 'critique', 'redteam'];
    const passes = [];
    for (let i = 0; i < 3; i++) {
        const p = await agent(reviewPrompts(scope)[i], {
            label: 'review:' + REVIEW_LABELS[i],
            phase: 'review',
            effort: 'high',
            schema: REVIEW_SCHEMA,
            stallMs: STALL,
        });
        passes.push(p);
        log(
            'review ' +
                (i + 1) +
                '/3 (' +
                REVIEW_LABELS[i] +
                '): ' +
                (p
                    ? (p.edits || []).length +
                      ' edit(s), ' +
                      (p.opportunities || []).length +
                      ' opportunit(ies), ' +
                      (p.alignments || []).length +
                      ' alignment(s)'
                    : 'NO RESULT — rerun via resume'),
        );
    }
    review = {
        scope,
        passes: passes.map((p, i) => ({ pass: REVIEW_LABELS[i], edits: p ? (p.edits || []).length : -1 })),
        opportunities: passes.filter(Boolean).flatMap((p) => p.opportunities || []),
        alignments: passes.filter(Boolean).flatMap((p) => p.alignments || []),
        final_verdict: (passes[2] && passes[2].final_verdict) || 'REVIEW PASS 3 MISSING — rerun',
        residual_risks: (passes[2] && passes[2].residual_risks) || [],
    };
}

return { targets: TARGETS, produced, review };
