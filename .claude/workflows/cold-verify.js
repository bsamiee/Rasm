export const meta = {
    name: 'cold-verify',
    whenToUse:
        'Campaign closure gate: after a rebuild campaign lands, verify the whole target corpus against its root DECISION/brief and fix every miss in place. args = {doc, root} or an array of such pairs; campaigns verify in parallel lanes. The resolver is the terminal finalizer — findings resolve in-run, never as a report, and no phase follows it.',
    description:
        'Cold-verify pass over one or more landed campaigns. Per campaign: one sonnet plan partitions the target folder into balanced verification slices; gpt-5.5 (codex) verifiers fan out through sonnet dispatch wrappers (CODEX flag; false restores native opus), each reading the root doc IN FULL plus its slice pages IN FULL, hunting missing/wrong/faked/naive work with typed anchored findings (one verifier owns the governance lane: index docs, manifest rows, csproj/README registries, .api anchors, acceptance traces, rider receipts; one verifier owns the cross-libs ripple lane: every sibling seam ledger, consumer anchor, counterpart obligation, and frozen wire name the campaign touches outside the target root). Every verifier runs a mandatory second-pass self-verify: each finding adversarially re-derived from disk before return, vague or unconfirmed findings deleted, and a clean verdict asserted only after the second hostile pass returns empty. ONE terminal fable resolver then finalizes the campaign with LIBS-WIDE ripple authority — verifier findings are SIGNALS, not law: it re-verifies each on disk, implements the strongest fix where a suggestion was weak or short-sighted, hunts and fixes what the verifiers missed on its own authority, resolves every ripple its edits expose anywhere under libs/ (sibling counterparts repaired in place both ends, except where the doc rules a counterpart recorded-only), and pushes touched pages past the ruling per the floor law. No phase follows the resolver.',
    phases: [
        { title: 'Plan', detail: 'per campaign: enumerate pages, partition into balanced slices', model: 'sonnet' },
        {
            title: 'Verify',
            detail: 'per campaign: slice verifiers + one governance verifier + per-branch cross-libs ripple verifiers on gpt-5.5 via codex wrappers (sonnet shells), read-only, self-verified typed anchored findings',
            model: 'sonnet',
        },
        {
            title: 'Resolve',
            detail: 'per campaign: one terminal fable finalizer — findings as signals, own hunt beyond them, every ripple resolved in-run, libs-wide',
            model: 'fable',
        },
    ],
};

// --- [CONSTANTS] -------------------------------------------------------------------------

const SLICES = 4;
const STALL = 300000;
const CODEX = true; // verifier fan lanes run on gpt-5.5 via the codex wrapper; false restores native opus lanes
const SCRATCH = '.claude/scratch/cold-verify'; // per-workflow run scratch: task/schema/report/stderr per lane

// --- [INPUTS] ----------------------------------------------------------------------------

const argsIn = typeof args === 'string' && /^\s*[\[{]/.test(args) ? JSON.parse(args) : args;
const CAMPS = (Array.isArray(argsIn) ? argsIn : [argsIn]).filter((c) => c && c.doc && c.root);
if (!CAMPS.length) {
    log('No campaigns — pass {doc, root} or an array of pairs.');
    return { campaigns: 0 };
}

// --- [MODELS] ----------------------------------------------------------------------------

const PLAN = {
    type: 'object',
    additionalProperties: false,
    required: ['slices', 'governance'],
    properties: {
        slices: { type: 'array', items: { type: 'array', items: { type: 'string' } } },
        governance: { type: 'array', items: { type: 'string' } },
    },
};

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const ANCHOR = {
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: { type: 'string' },
        line: { type: 'integer' },
        role: { type: 'string', enum: ['defect', 'ruling', 'catalog', 'counterpart', 'absence'] },
        note: { type: 'string' },
    },
};

const FINDINGS = {
    type: 'object',
    additionalProperties: false,
    required: ['findings', 'coverage', 'summary'],
    properties: {
        findings: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['claimKey', 'target', 'files', 'class', 'severity', 'claim', 'anchors', 'mechanism', 'owner', 'reject', 'acceptance'],
                properties: {
                    claimKey: { type: 'string' }, // <class>|<owner>|<primary symbol or absence route> — stable across lanes, never lane wording
                    target: { type: 'string' }, // short display label for the defect
                    files: { type: 'array', items: { type: 'string' } }, // files the resolver must open or edit first
                    class: { type: 'string', enum: ['missing', 'wrong', 'faked', 'naive', 'drift', 'phantom'] },
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence: closure-blocking | campaign-correctness | local cleanup
                    claim: { type: 'string' }, // the observed defect as fact
                    anchors: { type: 'array', items: ANCHOR },
                    mechanism: { type: 'string' }, // WHY the current form fails the ruling/doctrine — factual, zero repair verbs
                    owner: { type: 'string' }, // canonical owner that must absorb the resolution
                    reject: { type: 'array', items: { type: 'string' } }, // forms the repair must NOT take (deleted forms)
                    acceptance: { type: 'array', items: { type: 'string' } },
                },
            },
        }, // signals proving resolution
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

// Required-but-possibly-empty `beyond` is an attestation: the resolver's own hunt ran, not only the signal list.
const FIXLOG = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'resolved', 'beyond', 'rejected', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        resolved: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['target', 'action'],
                properties: {
                    target: { type: 'string' },
                    action: { type: 'string' },
                },
            },
        },
        beyond: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['target', 'action'],
                properties: {
                    target: { type: 'string' },
                    action: { type: 'string' },
                },
            },
        },
        rejected: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['finding', 'reason'],
                properties: {
                    finding: { type: 'string' },
                    reason: { type: 'string' },
                },
            },
        },
        summary: { type: 'string' },
    },
};

// --- [SHARED_BLOCKS]

const CTX = (c) =>
    'Rasm monorepo, planning phase. The campaign over ' +
    c.root +
    ' is LANDED; ' +
    c.doc +
    ' at the repo ' +
    'root is the binding ruling it executed. This pass earns the cold close: the corpus is presumed defective until an ' +
    'attack finds nothing. All .md prose follows docs/standards/style-guide.md — declarative agent-facing law, no ' +
    'provenance, no process narration, no hedges. Never run git commit.';

const HUNT =
    'HUNT CLASSES: missing (a ruled motion, page, owner, case, band, seam row, or rider with no landed ' +
    'counterpart), wrong (landed but contradicting the ruling or the doctrine), faked (claimed done — prose asserts what ' +
    'the fence body omits, a receipt without the edit, a re-anchor to a dead target), naive (landed thin — a slice of the ' +
    'ruled capability, an underutilized admitted package, a ceiling-read of a floor ruling), drift (two landed surfaces ' +
    'disagreeing — page vs index doc vs manifest vs .api), phantom (a cited member, page, or anchor that does not exist). ' +
    'Verify cited external members against the .api catalogs; never trust page prose about itself.';

const EVIDENCE_LAW =
    'FINDING FORM — you deliver TRUTH, never an implementation: `claim` states the observed defect; ' +
    '`mechanism` states WHY it fails the ruling or doctrine as fact; `anchors` carry one coordinate per row (role names ' +
    'what the coordinate proves; `note` is the shortest literal witness — a symbol, member spelling, or fragment under ' +
    '20 words — or empty when path+line suffice; an `absence` anchor names where the expected thing was searched and not ' +
    'found); `owner` names the canonical owner that must absorb the resolution (the owning axis, row roster, registry, or ' +
    'seam vocabulary — never a new local shape); `reject` lists the deleted forms the repair must not take; `acceptance` ' +
    'lists the signals that prove resolution. NEVER write add/replace/implement/promote/delete as instruction — the ' +
    'executor owns the design; you own the constraint boundary. `claimKey` = <class>|<owner>|<primary symbol or absence ' +
    'route>, identical for the same defect regardless of lane or wording. `severity` binds to consequence: blocker = ' +
    'closure-blocking, major = campaign correctness, minor = local cleanup — never prose confidence. OUTPUT BOUNDS: an ' +
    'ordinary scope yields 3-8 retained findings; 0 only when the second hostile pass comes back empty, and then ' +
    '`summary` names the probes that produced nothing; never manufacture a finding to fill the range, never delete a ' +
    'confirmed one to stay inside it. COVERAGE is part of the product: `requested` = your assigned scope, `read` = what ' +
    'you actually full-read, `skipped`/`unverified` = what you did not reach or could not confirm — an honest skip beats ' +
    'a silent one.';

const SELF_CHECK =
    'MANDATORY SELF-VERIFY (second pass, before returning): attack your OWN findings — re-open every ' +
    'cited anchor and try to REFUTE each finding from disk; a finding that fails re-confirmation is deleted, one that ' +
    'survives carries exact anchors and concrete evidence. A vague finding — no precise anchor, hedged appears/seems ' +
    'wording, a class without a demonstrated instance — is a defect. Then re-read your scope once more hunting what the ' +
    'first pass skimmed past; a clean verdict is asserted only after this second hostile pass returns empty.';

// --- [OPERATIONS] ------------------------------------------------------------------------

// gpt-5.5 dispatch: the sonnet wrapper's ONLY job is dispatch-and-relay — it writes the task + schema to
// SCRATCH, launches codex DETACHED (it outlives any single Bash call), waits for the typed -o report by
// liveness (never relaunching a live run), and returns a thin RECEIPT — the product stays on disk for the
// terminal reader. It never does, edits, judges, or relays the work.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const codexPrompt = (label, task, schema, writes) => {
    const base = SCRATCH + '/' + fileTag(label);
    const rpt = fileTag(label) + '-report.json'; // unique per lane; pgrep matches the -o path on the codex cmdline
    const rptPat = '[' + rpt.slice(0, 1) + ']' + rpt.slice(1); // self-excluding pgrep/pkill pattern
    return [
        'DISPATCH ROLE: gpt-5.5 (codex) performs the TASK below in its own context; you only launch it and return a thin ' +
            'RECEIPT for its on-disk report. Never perform, edit, judge, soften, summarize, or RELAY the work itself.',
        '(1) Files FIRST, with the WRITE TOOL — never a shell heredoc and never a relative path (cwd drift and heredoc quoting land files where codex cannot find them, killing every launch on a missing schema file). From the repository root (your starting cwd): mkdir -p ' +
            SCRATCH +
            "; purge stale lane artifacts (a leftover report would READY instantly with last run's data): rm -f " +
            base +
            '-report.json ' +
            base +
            '-stderr.log; Write the TASK block below verbatim to ' +
            base +
            '-task.md; Write this JSON ' +
            'Schema exactly to ' +
            base +
            '-schema.json — both paths resolved ABSOLUTE under the repository root: ' +
            JSON.stringify(schema),
        '(2) Launch codex DETACHED from the repo root — ONE Bash call from the repo root, which FIRST verifies the files: test -s ' +
            base +
            '-task.md && test -s ' +
            base +
            '-schema.json || echo ' +
            'FILES-MISSING — on FILES-MISSING redo (1), NEVER launch without both. THEN the command below VERBATIM, never ' +
            'retyped or reflowed (every token matters: dropping </dev/null makes codex block forever on stdin, ' +
            'zero-CPU, no report): ' +
            'codex exec -s ' +
            (writes ? 'workspace-write' : 'read-only') +
            ' --skip-git-repo-check --ephemeral -c mcp_servers={} ' +
            '--output-schema ' +
            base +
            '-schema.json -o ' +
            base +
            '-report.json "Do the task in ' +
            base +
            '-task.md ' +
            'from the repository root. Final message: JSON per the output schema." </dev/null >/dev/null 2>' +
            base +
            '-stderr.log &',
        '(3) WAIT for the answer. codex runs at high effort and is slow (often 5-15 min); an absent report WHILE codex ' +
            'is still running is NORMAL, never failure — do NOT relaunch a live run. Poll with sequential Bash calls, each ' +
            'with the Bash timeout parameter 280000: for i in $(seq 1 13); do [ -s ' +
            base +
            '-report.json ] && break; ' +
            'pgrep -f "' +
            rptPat +
            '" >/dev/null || break; sleep 20; done; if [ -s ' +
            base +
            '-report.json ]; then echo ' +
            'READY; elif pgrep -f "' +
            rptPat +
            '" >/dev/null; then echo RUNNING; else echo GONE; fi. Repeat the poll call ' +
            'while it prints RUNNING; stop on READY; on GONE go to (4). LIVENESS IS NOT HEALTH: after the 4th RUNNING ' +
            'poll (~20 min wall) the run is WEDGED, not slow — kill it (pkill -f "' +
            rptPat +
            '") and go to (4) as GONE. ' +
            'Cap at 7 poll calls total.',
        '(4) READY: do NOT relay the report body through your output — build the MECHANICAL headline with jq (never your own ' +
            "judgment): entries=$(jq '.findings | length' " +
            base +
            '-report.json); classes=$(jq -r \'[.findings[].class] | group_by(.) | map(.[0] + "x" + (length|tostring)) | join(",")\' ' +
            base +
            '-report.json); top=$(jq -r \'[.findings[].files[0]] | group_by(.) | max_by(length) | .[0] // "none"\' ' +
            base +
            '-report.json). ' +
            'Return the RECEIPT: ok=true, report=' +
            base +
            '-report.json, entries=that count, headline="<entries> findings | <classes> | top: <top>", failure empty. ' +
            'GONE with no report: tail -5 ' +
            base +
            '-stderr.log FIRST — that tail IS the crash reason; relaunch the (2) command once (detached, never ' +
            'foreground) and resume polling; a second GONE returns ok=false, entries=0, report and headline empty, failure=the stderr tail in one line.',
        'TASK — write verbatim to the task file, then dispatch:',
        task,
    ].join('\n\n');
};

// Every heavy read/investigate lane routes here: gpt-5.5 wrapper when CODEX, native opus otherwise.
// The roster row carries `scope` from the ORCHESTRATOR (never the lane's self-report) so a failed lane's
// unmapped territory is exact even when the lane died before writing anything.
const recon = (task, o) =>
    (CODEX
        ? agent(codexPrompt(o.label, task, o.schema, !!o.writes), {
              label: 'gpt-5.5:' + o.label,
              phase: o.phase,
              model: 'sonnet',
              effort: 'low',
              schema: RECEIPT,
              stallMs: STALL,
          })
        : agent(
              task +
                  '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
                  SCRATCH +
                  '/' +
                  fileTag(o.label) +
                  '-report.json (Write tool, absolute path under the repo root): ' +
                  JSON.stringify(o.schema) +
                  ' — then return ONLY the receipt: ok, report path, entries count, one-line mechanical headline, failure empty.',
              { label: o.label, phase: o.phase, model: 'opus', effort: 'high', schema: RECEIPT, stallMs: STALL },
          )
    ).then((r) => ({
        lane: o.label,
        scope: o.scope || [],
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'lane died'),
    }));

// --- [COMPOSITION] -----------------------------------------------------------------------

const lanes = await parallel(
    CAMPS.map((c) => async () => {
        const tag = c.root.split('/').pop();
        const plan = await agent(
            CTX(c) +
                '\n\nTASK: thin enumerate (read-only). List every design page under ' +
                c.root +
                '/.planning/ (real ls/find, never memory) and partition them into ' +
                SLICES +
                ' balanced slices grouped by ' +
                'sub-folder in dependency order. Return `governance` as the campaign governance surface: the package README.md and ' +
                'ARCHITECTURE.md, the .csproj or language manifest rows for this package, the .api folder path, and ' +
                c.doc +
                '.',
            { label: 'plan:' + tag, phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN, stallMs: STALL },
        );
        const slices = ((plan && plan.slices) || []).filter((s) => s && s.length);
        const gov = (plan && plan.governance) || [];
        const verifyTasks = slices.map(
            (pages, i) => () =>
                recon(
                    CTX(c) +
                        '\n\n' +
                        HUNT +
                        '\n\n' +
                        EVIDENCE_LAW +
                        '\n\n' +
                        SELF_CHECK +
                        '\n\nTASK: HOSTILE READ-ONLY VERIFY, ' +
                        'slice ' +
                        i +
                        '. Read ' +
                        c.doc +
                        ' IN FULL — every ruling, page row, band, seam, rider, and acceptance trace that ' +
                        'touches your pages. Then read each of these pages IN FULL plus every .api catalog its fences cite: ' +
                        JSON.stringify(pages) +
                        '. Verify each ruling landed PROPERLY — integrated as if always designed that way, at the ' +
                        'ruled band/signature/charter, frozen names byte-identical — and attack past the ruling: the floor law means a ' +
                        'page that merely met its disposition without depth is a naive finding. Return typed anchored findings.',
                    { label: 'verify:' + tag + ':s' + i, phase: 'Verify', schema: FINDINGS, scope: pages },
                ),
        );
        verifyTasks.push(() =>
            recon(
                CTX(c) +
                    '\n\n' +
                    HUNT +
                    '\n\n' +
                    EVIDENCE_LAW +
                    '\n\n' +
                    SELF_CHECK +
                    '\n\nTASK: HOSTILE READ-ONLY GOVERNANCE VERIFY. Read ' +
                    c.doc +
                    ' IN FULL, then audit the governance surface end to end: ' +
                    JSON.stringify(gov) +
                    '. Every acceptance trace ' +
                    'resolves on disk (page exists, entry carries the ruled signature, seam anchor present); every rider has its landed ' +
                    'receipt; the README router/package groups, ARCHITECTURE codemap + seams ledger (canonical [KIND] tags, mirrored ' +
                    'endpoints), central manifest rows, and .api anchors agree with the landed page set — a disagreement between any ' +
                    'two surfaces is a drift finding. Return typed anchored findings.',
                { label: 'verify:' + tag + ':gov', phase: 'Verify', schema: FINDINGS, scope: gov },
            ),
        );
        const BRANCHES = ['libs/csharp', 'libs/python', 'libs/typescript'];
        BRANCHES.forEach((branch) =>
            verifyTasks.push(() =>
                recon(
                    CTX(c) +
                        '\n\n' +
                        HUNT +
                        '\n\n' +
                        EVIDENCE_LAW +
                        '\n\n' +
                        SELF_CHECK +
                        '\n\nTASK: ' +
                        'HOSTILE READ-ONLY CROSS-LIBS RIPPLE VERIFY over ' +
                        branch +
                        ' (its branch .planning core, its .api tier, and ' +
                        'every package folder inside it EXCEPT ' +
                        c.root +
                        '). Read ' +
                        c.doc +
                        ' IN FULL, then hunt every cross-folder ' +
                        'touchpoint the campaign touches inside your scope (real grep/listing, never memory): sibling ARCHITECTURE seam ' +
                        'ledgers naming this package (mirrored glyphs and [KIND] tags, anchors into pages the campaign split, renamed, ' +
                        're-pointed, or deleted), consumer pages citing target anchors, every counterpart obligation or authorized 1-hop ' +
                        'edit the doc records (each landed at its named home exactly as sealed, or recorded where the doc rules ' +
                        'recorded-only), shared manifest rows and .api tiers, frozen wire names held byte-identical on both ends. A stale ' +
                        'sibling anchor, a one-sided seam edit, a missing authorized edit, an unrecorded counterpart, or a sibling ' +
                        'interior edited past the doc ruling is a finding. Zero touchpoints in your scope is a valid empty result. ' +
                        'Return typed anchored findings.',
                    { label: 'verify:' + tag + ':ripple:' + branch.split('/').pop(), phase: 'Verify', schema: FINDINGS, scope: [branch] },
                ),
            ),
        );
        const roster = (await parallel(verifyTasks)).filter(Boolean);
        const verified = roster.filter((r) => r.ok);
        const total = verified.reduce((a, r) => a + r.entries, 0);
        const unmapped = roster.filter((r) => !r.ok).flatMap((r) => r.scope.map((s) => ({ lane: r.lane, scope: s })));
        log(
            tag +
                ': ' +
                total +
                ' finding(s) across ' +
                verified.length +
                '/' +
                roster.length +
                ' lanes' +
                (verified.length < roster.length
                    ? ' — FAILED: ' +
                      roster
                          .filter((r) => !r.ok)
                          .map((r) => r.lane)
                          .join(', ')
                    : ''),
        );
        const fix = await agent(
            CTX(c) +
                '\n\n' +
                HUNT +
                '\n\nTASK: TERMINAL FINALIZE (WRITER — full authority over ' +
                c.root +
                ', its manifest rows, ' +
                c.doc +
                ' where a finding proves the doc itself wrong, AND libs-wide ripple authority: a ' +
                'ripple a fix exposes anywhere under libs/ — sibling seam ledgers, mirrored rows, consumer anchors, index docs, ' +
                '.api catalogs — is repaired in place at the sibling in the same pass, both ends; where the doc rules a ' +
                'counterpart RECORDED with a demanding consumer rather than edited, the recording IS the fix and the sibling ' +
                "interior stays unedited past that ruling. You are the run's LAST agent, no phase follows you). " +
                'CONSUMPTION PROTOCOL, in order: (a) read ' +
                c.doc +
                ' IN FULL — it is the ruling; (b) UNMAPPED scope below is ' +
                "your direct-hunt queue — a failed lane's territory gets your own cold read, first; (c) read every ok report " +
                'file IN FULL from disk, governance and ripple lanes before page slices — group findings by `claimKey` as you ' +
                'read (the same key across lanes is ONE defect with corroborating evidence, never several priorities) and order ' +
                'work by `severity` then `owner` (shared owners and registries before their consumers, cross-folder seams before ' +
                'local prose); (d) each finding is a SIGNAL: re-open its anchors before editing — anchors behind an edit, cited ' +
                'members, seams, and manifest rows re-verify MANDATORY; navigation-only entries in untouched groups re-verify ' +
                'only when touched (re-proving findings you will not act on is waste); (e) `mechanism`/`owner`/`reject`/' +
                "`acceptance` are the finding's constraint boundary — honor the owner and the rejected forms, but the DESIGN is " +
                'yours: implement the densest root-level resolution the boundary admits, never a single-point patch; a finding ' +
                'whose anchors do not re-confirm is rejected with reason. Then hunt PAST the signal list on your own authority — ' +
                'the hunt classes above over the corpus and governance surface as you work it — and fix what the verifiers ' +
                'missed; `beyond` enumerates those fixes, and an empty `beyond` attests your own hunt found nothing, never that ' +
                'it did not run. Every ripple an edit exposes is YOURS in the same pass, anywhere under libs/: seam counterparts ' +
                'both ends, consumer sites, index docs, manifest rows, .api anchors — the run ends finalized, nothing deferred. ' +
                'The floor law governs every page you touch: exceed the ruling with denser, deeper, more capable form. Frozen ' +
                'signatures and wire names stay byte-identical. ' +
                'UNMAPPED: ' +
                JSON.stringify(unmapped) +
                ' ROSTER: ' +
                JSON.stringify(roster),
            { label: 'resolve:' + tag, phase: 'Resolve', model: 'fable', effort: 'high', schema: FIXLOG, stallMs: STALL },
        );
        return {
            campaign: c.root,
            lanes: roster.length,
            failedLanes: roster.filter((r) => !r.ok).map((r) => r.lane),
            findings: total,
            resolved: (fix && fix.resolved && fix.resolved.length) || 0,
            beyond: (fix && fix.beyond && fix.beyond.length) || 0,
            rejected: (fix && fix.rejected && fix.rejected.length) || 0,
            summary: (fix && fix.summary) || '',
        };
    }),
);

return { campaigns: lanes.filter(Boolean) };
