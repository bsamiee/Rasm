export const meta = {
    name: 'docgen-pass',
    whenToUse:
        'Seven-phase docgen conformance sweep over any folder of durable markdown: a finder per folder-group runs the cross-surface ownership interrogation and surfaces every prose/comment/structure defect against the live defects.md catalog; a per-file fixer rebuilds the page template-true at the register under silent-removal, token-trim, and proven-demotion discipline, and on a .planning/ page extracts each folder-wide ruling into a transcribe-ready RULINGS dossier; an adversarial red-team then git-diffs HEAD, repairs the fixer litter, hardens the dossier, and drives a further 20-30% reduction; a grader hostilely re-verifies losslessness and opens every demotion target; a bounded remediator repairs confirmed residuals; a per-owner collector adjudicates the dossiers and transcribes the survivors into RULINGS.md. First-pass prose cut ~50% then a further 20-30% off the current state, both subordinate to the zero-capability-loss demotion ledger, non-load-bearing fence comments cut, spec pages carrying their [01]-[INDEX] and terminal [RESEARCH] marker, folder-wide law moved off design pages into the decision registry with no duplication, fence code bodies untouched, prose gate the mechanical floor.',
    description:
        'Targets any folder (libs/csharp/Rasm/.planning, a single sub-folder, a .api tier, docs/standards — each a different workload). args = a folder path, an array of paths, or {targets}; empty = no-op. Every agent is Opus. Route: one census lists every .md under the targets with total/prose/comment line counts, deduped by path. Analyze: one finder per folder-group reads the full docgen law plus the owner surfaces it forks against (global + project CLAUDE.md [02], the folder README/ARCHITECTURE/RULINGS registries), runs the ownership interrogation, hunts imported frames against each file-kind template, and returns per-file findings (class, anchor, problem, direction, move), cross-file duplication routed to the losing file, index/research status — no edits. Fix: one Opus fixer per file rebuilds every prose surface and fence comment in place to the register, removal the default repair, spreads restructured not brute-deleted, every demotion proven by opening its target across all candidate owners, and returns a demotion ledger plus honest before/after counts. When a page is under a .planning/ tree the fixer also extracts folder-wide rulings mis-homed as page prose — removing each from the page (no duplication) and writing a transcribe-ready guard-grade row to a per-file dossier under .claude/scratch. Redteam: one Opus red-team per fixed page git-diffs HEAD to see the fixer output, repairs its litter (tombstones, substitute negations, fresh twins, re-narrating pointers), hardens the dossier, and drives a further 20-30% reduction under silent-removal and token-trim discipline with capability conserved, its receipt superseding the fixer receipt. Verify: one Opus grader per page re-derives losslessness of the final Fix+Redteam state from disk, opens every demotion target including the RULINGS dossiers, and runs the cold-rebuild frame counterfactual. Remediate: one Opus fixer per flagged page restores over-deleted law. Collect: one Opus collector per owning package/branch adjudicates its dossiers (dedup, drop already-homed, guard-grade repair, tier deferral) against the existing RULINGS.md and transcribes the survivors into it. Cap 14 agents per phase.',
    phases: [
        {
            title: 'Route',
            detail: 'one Opus census: every .md under each target with total, prose, and in-fence-comment line counts, deduped by path — the baseline every finder and fixer derives from',
        },
        {
            title: 'Analyze',
            detail: 'one Opus finder per folder-group under the pooled cap: docgen law read in full, the owner surfaces opened as fork targets, the ownership interrogation run, per-file findings and cross-file duplication returned, no edits',
        },
        {
            title: 'Fix',
            detail: 'one Opus fixer per file under the pooled cap: all prose and fence comments rebuilt in place to the register, removal the default repair, every demotion proven against its target, a folder-wide ruling on a planning page removed and written to a transcribe-ready RULINGS dossier, the file landed template-true, gate run to clean, ledgered receipt returned',
        },
        {
            title: 'Redteam',
            detail: 'one Opus red-team per OK-fixed page under the pooled cap: git-diffs HEAD to see the fixer output, repairs its litter (tombstones, substitute negations, fresh twins, re-narrating pointers), hardens the rulings dossier, and drives a further 20-30% reduction off the current disk state under silent-removal and token-trim discipline, capability conserved; its receipt supersedes the fixer',
        },
        {
            title: 'Verify',
            detail: 'one Opus grader per page under the pooled cap: the final Fix+Redteam state re-derived hostilely from disk against the HEAD baseline, losslessness graded, every demotion target opened for pointer-set-widening and false over-deletion, the cold-rebuild counterfactual run for residual imported frame, residuals returned, no edits',
        },
        {
            title: 'Remediate',
            detail: 'one Opus fixer per grader-flagged page under the pooled cap: over-deleted law restored from the baseline, every demotion re-proven, the gate re-run — fires only for pages a grader flagged',
        },
        {
            title: 'Collect',
            detail: 'one Opus collector per owning package/branch that got extractions: its dossiers adjudicated against the existing RULINGS.md (dedup, drop already-homed, guard-grade repair, tier deferral) and the survivors transcribed into RULINGS.md, gate run — fires only when a planning page extracted a folder-wide ruling',
        },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = '/Users/bardiasamiee/Documents/99.Github/Rasm';
const CHAIN = ['/Users/bardiasamiee/.claude/CLAUDE.md', ROOT + '/CLAUDE.md']; // always-loaded fork surfaces: global [02] + project [02]
const CAP = 14; // concurrent agents per phase; the 16-core harness caps agent() at 14 in lockstep
const GROUP = 4; // finder files per analyze agent
const PROSE_LO = 45; // prose reduction band the receipt grades against, percent of baseline prose lines
const PROSE_HI = 60;
const COMMENT_TARGET = 50; // non-load-bearing fence comments cut, percent of baseline comment lines
const REDTEAM_FURTHER = '20-30'; // further reduction the red-team drives off the post-Fix disk state
const TOTAL_HI = 80; // upper reduction the run's off-band grade tolerates after Fix + Redteam; over-deletion is owned by Verify, not this soft band

// prose lines = non-blank lines outside fences; the ONE measure census, finders, and receipts share
const PROSE_MEASURE = "awk '/^```/{f=!f;next} !f&&NF{n++} END{print n+0}'";
// comment lines = in-fence lines opening on //, #, --, or ; EXCLUDING section dividers and shebangs; the shared comment measure
const COMMENT_MEASURE =
    'awk \'/^```/{f=!f;next} f{s=$0; sub(/^[ \\t]+/,"",s); if(s ~ /^(\\/\\/|#|--|;)/ && s !~ /^(\\/\\/|#|--|;)[ \\t]*---[ \\t]*\\[/ && s !~ /^#!/) c++} END{print c+0}\'';

// --- [INPUTS] --------------------------------------------------------------------------

const norm = (t) =>
    String(t)
        .trim()
        .replace(new RegExp('^' + ROOT.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + '/?'), '')
        .replace(/^\/+/, '')
        .replace(/\/+$/, '');
const argsIn = args;
const isObj = !!argsIn && typeof argsIn === 'object' && !Array.isArray(argsIn);
const rawTargets = Array.isArray(argsIn)
    ? argsIn
    : isObj && Array.isArray(argsIn.targets)
      ? argsIn.targets
      : isObj && typeof argsIn.targets === 'string'
        ? [argsIn.targets]
        : typeof argsIn === 'string' && argsIn.trim()
          ? [argsIn]
          : [];
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(norm))].filter(Boolean);

// per-run scratch for the RULINGS extraction dossiers; deterministic from the target set (clock/randomness would break resume)
const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH =
    '.claude/scratch/docgen-pass-' +
    TARGETS.map((t) => t.split('/').pop().toLowerCase())
        .join('-')
        .replace(/[^a-z0-9.-]+/g, '-')
        .slice(0, 50) +
    '-' +
    fnv1a(JSON.stringify(TARGETS));
const DOSSIER_DIR = SCRATCH + '/rulings';

// --- [ROUTING] -------------------------------------------------------------------------

const kindOf = (p) => {
    const b = p.split('/').pop();
    if (b === 'README.md') return 'readme';
    if (b === 'ARCHITECTURE.md') return 'architecture';
    if (b === 'RULINGS.md') return 'rulings';
    if (b === 'IDEAS.md') return 'ideas';
    if (b === 'TASKLOG.md') return 'tasklog';
    if (p.includes('/.api/') || /^api-.*\.md$/.test(b)) return 'api';
    if (p.includes('/.planning/')) return 'spec';
    return 'page';
};
const TEMPLATE = {
    readme: 'readme.template.md',
    architecture: 'architecture.template.md',
    rulings: 'rulings.template.md',
    ideas: 'ideas.template.md',
    tasklog: 'tasklog.template.md',
    api: 'api-catalog.template.md',
    spec: 'spec.template.md',
    page: '',
};
const templatePath = (kind) => (TEMPLATE[kind] ? ROOT + '/.claude/skills/docgen/templates/' + TEMPLATE[kind] : '');
const chunk = (xs, n) => Array.from({ length: Math.ceil(xs.length / n) }, (_, i) => xs.slice(i * n, i * n + n));

// RULINGS extraction routing — only files under a `.planning/` tree own a RULINGS.md; the owner is the path component above `.planning`
const underPlanning = (p) => p.includes('/.planning/');
const ownerOf = (p) => (underPlanning(p) ? p.slice(0, p.indexOf('/.planning')) : '');
// nearest-tier RULINGS.md candidates for an owner; package RULINGS sits at the owner root, branch RULINGS inside `.planning`
const rulingsCandidates = (owner) => [ROOT + '/' + owner + '/RULINGS.md', ROOT + '/' + owner + '/.planning/RULINGS.md'];
const flatSlug = (p) => p.replace(/[^a-zA-Z0-9]+/g, '_');
const dossierPath = (p) => DOSSIER_DIR + '/' + flatSlug(p) + '.md';

// owner registries a page forks against: its own directory, the package-root folder registries, and the branch registries.
// Folder README/ARCHITECTURE/RULINGS live at the package root (the component above `.planning`), never inside it; branch registries live at `libs/<lang>/.planning`.
const ownerSurfaces = (paths) => {
    const names = ['README.md', 'ARCHITECTURE.md', 'RULINGS.md'];
    const set = new Set();
    const add = (base) => { for (const n of names) set.add(ROOT + '/' + (base ? base + '/' : '') + n); };
    for (const p of paths) {
        add(p.lastIndexOf('/') >= 0 ? p.slice(0, p.lastIndexOf('/')) : ''); // registries sitting as the file's own directory siblings
        const pi = p.indexOf('/.planning');
        if (pi >= 0) {
            const owner = p.slice(0, pi); // package root owns the folder registries
            add(owner);
            const cut = owner.lastIndexOf('/');
            if (cut >= 0) add(owner.slice(0, cut) + '/.planning'); // libs/<lang>/.planning owns the branch cross-folder registries
        }
    }
    return [...set];
};

// --- [MODELS] --------------------------------------------------------------------------

const S = { type: 'string' };
const I = { type: 'integer' };
const B = { type: 'boolean' };

const ROUTE_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'missing'],
    properties: {
        files: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['path', 'total', 'prose', 'comments'],
                properties: { path: S, total: I, prose: I, comments: I },
            },
        },
        missing: { type: 'array', items: S }, // targets that resolved to no folder or no pages
    },
};

const FINDING = {
    type: 'object',
    additionalProperties: false,
    required: ['class', 'anchor', 'problem', 'direction', 'move'],
    properties: {
        class: S, // exact [NN]-[CLASS] heading from defects.md, e.g. ENUMERATION_ANCHOR, SAME_DECISION_SPREAD, DELEGATED_ANATOMY, ROSTER_PREAMBLE
        anchor: S, // heading or first few words of the offending passage
        problem: S, // one sentence: why it is fragile or poison
        direction: S, // one sentence: the reframe or the owner it demotes to; for merge/split it NAMES the distinct clauses/facet to preserve
        // fixer disposition, so the fixer need not re-derive it:
        // delete = regenerable frame, no payload; demote = payload to the named owner; merge = same-decision spread, keep distinct clauses;
        // split = compound fork-boundary bullet, mechanism out + facet re-homed; reframe = in-place
        move: { type: 'string', enum: ['delete', 'demote', 'merge', 'split', 'reframe'] },
    },
};
const CROSSFILE = {
    type: 'object',
    additionalProperties: false,
    required: ['fact', 'ownerSibling'],
    properties: {
        fact: S, // the decision this file restates
        ownerSibling: S, // the sibling that owns it, opened and confirmed to carry the FULL fact
    },
};
const ANALYZE_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files'],
    properties: {
        files: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['path', 'findings', 'crossFile', 'indexStatus', 'researchStatus'],
                properties: {
                    path: S,
                    findings: { type: 'array', items: FINDING },
                    crossFile: { type: 'array', items: CROSSFILE }, // duplicates routed to THIS file as the loser; empty when none
                    indexStatus: { type: 'string', enum: ['ok', 'stale', 'missing', 'n/a'] },
                    researchStatus: { type: 'string', enum: ['ok', 'stale', 'missing', 'n/a'] },
                },
            },
        },
    },
};

const DEMOTION = {
    type: 'object',
    additionalProperties: false,
    required: ['payload', 'owner', 'verified', 'setWidth'],
    properties: {
        payload: S, // the decision/invariant/member moved out of prose
        owner: S, // the fence, fence comment, or sibling doc + anchor it demoted into
        verified: B, // target OPENED and confirmed to carry every demoted member
        setWidth: { type: 'string', enum: ['exact', 'wider', 'narrower'] }, // demoted set vs the owner's real set
    },
};
const FIX_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: [
        'file',
        'ok',
        'kind',
        'beforeProse',
        'afterProse',
        'proseReductionPct',
        'beforeComments',
        'afterComments',
        'commentReductionPct',
        'demotions',
        'capabilityConserved',
        'indexLanded',
        'researchRows',
        'rulingsExtracted',
        'rulingsDossier',
        'gateClean',
        'notes',
    ],
    properties: {
        file: S,
        ok: B,
        kind: S,
        beforeProse: I,
        afterProse: I,
        proseReductionPct: I,
        beforeComments: I,
        afterComments: I,
        commentReductionPct: I,
        demotions: { type: 'array', items: DEMOTION }, // proven-demotion ledger; empty only for a pure deletion of regenerable frame
        capabilityConserved: B, // every baseline decision/invariant/boundary/trap survives in prose or a verified demotion
        indexLanded: { type: 'string', enum: ['yes', 'no', 'n/a'] }, // [01]-[INDEX] present after the pass, no when a spec could not land it, n/a for non-spec kinds
        researchRows: I, // terminal [RESEARCH] row count; 0 for (none); -1 for kinds with no research section
        rulingsExtracted: I, // folder-wide rulings removed from this page into the dossier; 0 when none applicable; -1 when the file is not under a .planning/ tree
        rulingsDossier: S, // repo-relative dossier path written for a planning file, "" for a non-planning file
        gateClean: B,
        notes: S, // off-band honesty, judgment-tier gate rows resolved; empty when clean
    },
};
const VERIFY_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['file', 'grade', 'losslessness', 'overDeletion', 'demotionsProven', 'newTwin', 'frameClean', 'gateClean', 'residuals'],
    properties: {
        file: S,
        grade: { type: 'string', enum: ['A', 'A-', 'B+', 'B', 'C'] },
        losslessness: B, // every decision/invariant/boundary/trap from the baseline still present
        overDeletion: { type: 'array', items: FINDING }, // law cut with no proven owner, or a demotion target that does not hold the full set
        demotionsProven: B, // every pointer's target OPENED and confirmed to hold all demoted members at exact set width
        newTwin: B, // false when a fold re-duplicated a rule at its destination
        frameClean: B, // false when the cold-rebuild counterfactual proves a page still wears an inheritable imported frame
        gateClean: B,
        residuals: { type: 'array', items: FINDING }, // remaining defects for the remediator
    },
};

const COLLECT_ROW = {
    type: 'object',
    additionalProperties: false,
    required: ['section', 'row'],
    properties: {
        section: { type: 'string', enum: ['PACKAGES', 'SHAPE', 'COLLAPSE', 'STRUCTURE', 'PROCESS'] },
        row: S, // the transcribed RULINGS row, exact template grammar
    },
};
const COLLECT_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['owner', 'rulingsFile', 'created', 'admitted', 'rejected', 'deferred', 'gateClean'],
    properties: {
        owner: S, // the package/branch dir whose RULINGS.md this collector owns
        rulingsFile: S, // the resolved (or created) RULINGS.md path it wrote
        created: B, // true when no RULINGS.md existed and it was minted from the template
        admitted: { type: 'array', items: COLLECT_ROW }, // rows transcribed into RULINGS.md this pass
        rejected: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['row', 'reason'],
                properties: { row: S, reason: { type: 'string', enum: ['already-homed', 'merged-with-sibling', 'not-guard-grade'] } },
            },
        },
        deferred: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['row', 'tier'],
                properties: { row: S, tier: { type: 'string', enum: ['branch', 'cross-libs'] } }, // scoped wider than this owner; surfaced, never mis-homed
            },
        },
        gateClean: B,
    },
};

// --- [DOCTRINE] ------------------------------------------------------------------------

const D = ROOT + '/.claude/skills/docgen/';
const STD = ROOT + '/docs/standards/';
const LAW_FILES = [
    D + 'SKILL.md',
    D + 'references/structure.md',
    D + 'references/defects.md',
    D + 'references/rewriting.md',
    STD + 'style-guide.md',
    STD + 'information-structure.md',
    STD + 'formatting.md',
    D + 'examples/intros.md',
    D + 'examples/lists.md',
    D + 'examples/tables.md',
].join(', ');

const PREAMBLE =
    'WORKING ROOT: ' +
    ROOT +
    ' — every relative path resolves against it; read, write, and edit only under it. ' +
    'Durable prose is agent-facing LAW: a future agent loads this document with no memory of why it was written, and every prose line and every fence comment ' +
    "either changes that agent's next action or poisons it. Two enemies live in an overgrown doc. FRAGILE prose needs updating when the corpus moves without this " +
    'page moving — member rosters, scalar counts, sibling-shape mirrors, version/freshness anchors, cross-references, self-descriptions; each is stale the day after ' +
    "it is written and silently lies thereafter. POISON prose walls the next rebuild — enumeration anchors freezing today's census as the contract, deleted-form " +
    'litanies anchoring to the anti-shape, process-ledger tombstones and decision tags, seals and frozen chants, hedges and weak permission verbs marking undecided ' +
    'ownership. What survives is only LAW an agent cannot regenerate from disk: decisions, invariants, boundaries, ownership rulings, traps — each with an owning ' +
    'subject and owning verb, stated once, timeless.';

const ACID =
    'THE ACID TEST — for every prose sentence: does it stay true, unchanged, across any doctrine-conforming rebuild of the fences it accompanies? Prose couples to ' +
    'intent; the fence couples to shape. A sentence whose truth depends on the current fence body is FRAGILE (it drifts when the fence improves); a sentence naming ' +
    "the current shape is POISON (it walls the next rebuild inside today's design). Both cure the same way: state what the rebuild must preserve, and let the fence " +
    'own the shape. PROSE-CODE BOUNDARY: prose names owners, code carries mechanism — a prose line names at most the owning symbol as a code span and states its law; ' +
    'signatures, parameter lists, member chains, and option rosters belong to the fence, not prose.';

const arm = (extra) =>
    'ARM FIRST — read these law files IN FULL before opening any target (reading a target first inherits its frame): ' +
    LAW_FILES +
    (extra ? ', ' + extra : '') +
    '. Read every target as EVIDENCE, never as guidance.';

const RULINGS_TEMPLATE = D + 'templates/rulings.template.md';
const CAMPAIGN_METHOD = ROOT + '/libs/.planning/campaign-method.md';
// the RULINGS extraction charter — appended to finder, fixer, and collector when a target is under a .planning/ tree
const rulingsLaw =
    '\n\nRULINGS EXTRACTION — a `.planning/` design page sometimes carries FOLDER-WIDE law mis-homed as page-local prose: a settled decision that governs the ' +
    'owning package or branch, whose re-litigation guard (the one why that stops it being re-argued) has NO durable home. That belongs in the folder RULINGS.md ' +
    'decision registry, never restated on a design page. Read IN FULL its owning law before acting: ' +
    RULINGS_TEMPLATE +
    ' (admission bar, closed section vocabulary, tier placement, `(none)` convention, guard-grade row anatomy) and ' +
    CAMPAIGN_METHOD +
    ' [03] rulings discipline. ADMISSION BAR (all must hold): the content is a SETTLED decision (open work or a violation is a card, NOT a ruling — leave it); it ' +
    'governs SIBLINGS or the folder, not just this one page (a page-local design decision STAYS on the page); its why has NO durable home (RULINGS, a README/ARCHITECTURE ' +
    'registry, a manifest, or a fence does not already carry both the what and the why); and it seats in exactly one closed section — `[PACKAGES]` admission/rejection, ' +
    '`[SHAPE]` owner-choice discriminant, `[COLLAPSE]` density refusal, `[STRUCTURE]` layout/retirement, `[PROCESS]` folder-learned working law. A guard-grade row pairs a ' +
    'subject code-span, a decision-verb in owning voice (KEEP / rejected / homes at / stays / does-not-re-enter), the one load-bearing why, the recurring WRONG MOVE it ' +
    'guards against, the correct route, and — for an evidence-bound call — the reopen predicate; one decision per row. NOT FORCED: most pages carry no such ruling; extraction ' +
    'is the rare, high-confidence case, and forcing page-local design into RULINGS pollutes the registry and strips the page. When unsure a line is folder-wide vs ' +
    'page-local, KEEP it on the page.';

// the ownership interrogation — the dominant diagnostic; appended to finder, fixer, and grader mandates with the owner surfaces they fork against
const ownershipClause = (paths) =>
    '\n\nOWNERSHIP INTERROGATION — the dominant diagnostic, run FIRST on every law sentence before ruling it load-bearing: who ELSE already binds this fact? ' +
    'Resolve against three owner classes, each naming a fork. (a) The always-loaded instruction chain — global and project CLAUDE.md at ' +
    CHAIN.join(' and ') +
    ' (their [02] prose-comment and implementation-standard sections): a rule true for every agent this session is CHAIN_RESTATEMENT — delete. ' +
    '(b) A sibling durable doc with a nearer or equal claim — the folder README, ARCHITECTURE, RULINGS registry, or a docgen template: ' +
    'that is TWIN_TRUTH or DELEGATED_ANATOMY — demote to a one-line pointer naming the owner and its one consumed concern. ' +
    '(c) A deterministic tool — the prose gate, an analyzer, a template comment: that is TOOL_ENFORCED_LEAK — defer by name. ' +
    'OPEN these owner surfaces as binding law you fork AGAINST, never as targets to edit (skip any that do not exist on disk): ' +
    JSON.stringify(ownerSurfaces(paths)) +
    '. A sentence that reads load-bearing in isolation is the prime suspect — the fork is invisible until the page is held against these surfaces and no script runs this audit. ' +
    'A demotion is UNPROVEN until you open the named owner and confirm it carries EVERY forked member, across ALL candidate owners — not the first that holds part of it; ' +
    'stopping at the first owner both manufactures false over-deletions and misses real forks. Flag a fork against an owner surface as a finding whose direction names that owner.';

// silent-removal and token-trim discipline — shared by the fixer and the red-team; the register bans force-keeping and grades the cut in tokens, not lines alone
const silentAndTrim =
    '\n\nSILENT REMOVAL — never force-keep a line to protect a floor or out of caution. A line whose load already stands elsewhere — the positive law standing, a fence, ' +
    'a sibling owner, the register itself — is DELETED silently: no tombstone, no "removed X" note, no apologetic pointer minted to stand in for the cut. Keeping weak prose ' +
    '"just in case" is the defect this pass kills; the only survivor is a line whose deletion loses law no other surface holds. A comment the code already shows is deleted ' +
    'whole, not tightened.' +
    '\n\nTRIM TO THE TOKEN — reduction is not line-level alone. Inside every surviving line, cut to the load-bearing words: strip filler, hedges, redundant qualifiers, ' +
    'restated context, and throat-clearing lead-ins; state identical law in a third fewer TOKENS. A page hits its line band and still reads bloated at the character level — ' +
    'measure the cut in words and characters, not lines, and a dense-looking survivor that still carries a cuttable clause is unfinished.';

// the terminal RESEARCH marker, transcribed BYTE-FOR-BYTE from spec.template.md — the exact block every spec page carries under its `## [NN]-[RESEARCH]` heading
const RESEARCH_MARKER =
    '<!-- source-only: research row template:\n' +
    '[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.\n' +
    '[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.\n' +
    '-->';
// terminal RESEARCH-section law for spec pages — appended to the fixer and red-team; the marker is a byte-exact transcription requirement, never a paraphrase
const researchLaw =
    '\n\nTERMINAL [RESEARCH] SECTION — a spec page ALWAYS ends with a `## [NN]-[RESEARCH]` section; NN is the contiguous section number after your edits settle the count. ' +
    'Directly beneath that heading sits the source-only marker comment, and it is a BYTE-FOR-BYTE transcription from ' +
    D +
    'templates/spec.template.md — copy this exact block verbatim, every line, every backtick and token: never collapse it to one line, never drop the ' +
    '`[SPLIT_MEMBER]` example line, never reword or reformat it.\n' +
    RESEARCH_MARKER +
    '\nBelow the marker come the rows: `(none)` alone on its line when the page has no open research, otherwise one row per line as `- [TOKEN]-[OPEN|BLOCKED]: <exact ' +
    'question>; <verification route>` with TOKEN an UPPERCASE_SNAKE slug and every existing row rewritten to that exact grammar. CASES: a page MISSING the marker (rows but no ' +
    'source-only comment) gets the exact block inserted above its rows; a page with a MANGLED marker (collapsed, truncated, or reworded) has it REPLACED with the exact block; ' +
    'a page with NO research carries the marker plus `(none)`. NEVER invent rows to fill the section, NEVER delete the section or its marker, and keep the `## [01]-[INDEX]` in ' +
    'sync with the real section numbers after any renumber the RESEARCH section forces.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzeMandate = (group) =>
    PREAMBLE +
    '\n\n' +
    ACID +
    '\n\nYOU ARE THE FINDER. You read a group of sibling files and surface every defect for the per-file fixer that follows — you make NO edits. ' +
    'Holding all ' +
    group.length +
    ' files at once is ONE lever — you see cross-file TWIN_TRUTH and SAME_DECISION_SPREAD a single-file fixer is blind to; the DOMINANT lever is the ownership interrogation below.' +
    '\n\n' +
    arm('') +
    ownershipClause(group.map((f) => f.path)) +
    '\n\nTARGETS (read each IN FULL as evidence): ' +
    JSON.stringify(group.map((f) => ({ path: f.path, kind: f.kind }))) +
    '.' +
    '\n\nFOR EACH FILE return a findings list. Classify every suspect passage by its exact `[NN]-[CLASS]` heading in defects.md — the catalog you read IN FULL is the ' +
    'authoritative roster (all 27 classes), never a list restated here that drifts the moment a class is minted. Hunt the two newest classes explicitly, an out-of-date reflex ' +
    'misses them: `[26]-[DELEGATED_ANATOMY]` — a page that legally owns a delegated discipline re-spelling the sub-anatomy (row grammar, marker set, touch-point roster, (none) ' +
    'rule) a template or the chain owns, past the obligation delegated to it; keep the obligation, pointer the anatomy, and audit the same entry for a sibling anatomy left ' +
    'inline. `[27]-[ROSTER_PREAMBLE]` — a header entry enumerating a member set whose following detail entries each already expand one member; delete the header when the ' +
    'details carry every member. IMPORTED FRAME — hunt it as the prime structural contaminant: a page whose heading census, section spine, or card grammar does not match its ' +
    'file-kind template (a spec wearing card framing, an ARCHITECTURE wearing a README shape, any doc predating the docgen templates) poisons every future rebuild that reads ' +
    'it as context and inherits its shape; flag it `META_FRAME`, direction = rebuild template-true to the file kind. Cover prose AND fence comment lines; a bloated multi-line ' +
    'comment block, a comment restating adjacent code, or a narration comment is a ' +
    'finding. Do not flag fence code bodies, mermaid bodies, or section-divider comment lines.' +
    '\n\nEach finding: {class (the `[NN]-[CLASS]` name), anchor (heading or opening words), problem (one sentence, why fragile or poison), direction (one sentence, the reframe ' +
    'or the owner it demotes to), move (delete|demote|merge|split|reframe)}. move encodes the fixer disposition so it need not re-derive it: delete = regenerable frame, no ' +
    'payload; demote = payload to the named owner in direction; merge = same-decision spread, and direction NAMES every distinct clause to keep; split = compound ' +
    'fork-boundary bullet, and direction names the mechanism to demote and the facet to re-home; reframe = in-place.' +
    '\n\nCROSS-FILE: with all ' +
    group.length +
    ' files in view, find each decision restated across siblings (TWIN_TRUTH or SAME_DECISION_SPREAD across files) and route each duplicate to the ONE file that should LOSE ' +
    "its copy — record it in that loser file's crossFile array as {fact, ownerSibling}. Name the winning sibling AND confirm by opening it that it carries the FULL fact, not a " +
    'narrower form; an unopened winner is an unproven route and the loser keeps its copy until proven. Empty array when none.' +
    '\n\nINDEX/RESEARCH: for a spec-kind file report indexStatus and researchStatus. researchStatus is `ok` ONLY when the terminal `## [NN]-[RESEARCH]` section is present AND ' +
    'its source-only marker comment matches the spec.template.md block byte-for-byte AND every row conforms to `- [TOKEN]-[OPEN|BLOCKED]: <question>; <route>`; a marker that ' +
    'is missing, collapsed to one line, truncated (the `[SPLIT_MEMBER]` line dropped), or reworded is `stale`, and an absent section is `missing`. indexStatus is `ok` when ' +
    '`## [01]-[INDEX]` is present and its numbers match the real sections, else `stale`/`missing`. For every non-spec kind both are n/a.' +
    (group.some((f) => underPlanning(f.path))
        ? rulingsLaw +
          '\n\nAs the FINDER, flag each folder-wide ruling candidate you find in a planning target as a normal finding with move=demote and direction beginning ' +
          '"RULINGS [SECTION]: " naming the section and the guard — the fixer extracts it. Flag ONLY high-confidence admission-bar passes; a page-local design decision ' +
          'is not a candidate.'
        : '') +
    '\n\nReturn {files:[{path, findings, crossFile, indexStatus, researchStatus}]}. No edits, no gate, no prose written to disk.';

const fixMandate = (f, seed) => {
    const tpl = templatePath(f.kind);
    const spec = f.kind === 'spec';
    const hasSeed = !!seed;
    const planning = underPlanning(f.path);
    const dossier = dossierPath(f.path);
    return (
        PREAMBLE +
        '\n\n' +
        ACID +
        '\n\nYOU ARE THE FIXER for ONE file. A finder pass already read this file with its siblings; its findings are HINTS that seed your work, never the ceiling — ' +
        'you run your own full hostile hunt over the whole page. A rewrite is a reframe from the extracted payload, never a shorter paraphrase of the old sentence.' +
        '\n\n' +
        arm(
            tpl
                ? 'and the file-kind template you land this file against: ' + tpl
                : 'this file is a general durable page — its shape is the docgen page-shape law in formatting.md [03]-[PAGE_SHAPE]',
        ) +
        ownershipClause([f.path]) +
        '\n\nTARGET: ' +
        f.path +
        ' (kind: ' +
        f.kind +
        ') — read it IN FULL as evidence. Baselines: prose ' +
        f.prose +
        ' lines, comments ' +
        f.comments +
        ' lines. Also read the nearest ancestor README.md as your register exemplar if one exists — leads that legislate in owning voice, dense with the ' +
        "folder's own vocabulary, zero narration; your leads read as the same hand." +
        '\n\nFINDER FINDINGS for this file' +
        (hasSeed ? ' (seed, not ceiling):' : ' (NONE — the finder pass DROPPED this file; run a full cold hostile hunt, no hints exist):') +
        '\n' +
        JSON.stringify(hasSeed ? { findings: seed.findings, crossFile: seed.crossFile } : { findings: [], crossFile: [] }) +
        '\n\nTERRITORY — prose surfaces you rebuild: H1 leads, every section bullet and paragraph, list entries, table leads, and COMMENT LINES inside fences. ' +
        'UNTOUCHED — fence code bodies (signatures, types, members, cases, fields, logic, imports), mermaid diagram bodies, and section-divider comment lines ' +
        '(style-correct a wrong divider label, never delete one). You are a prose surgeon, never a design editor: no owner, case, member, or design decision changes.' +
        '\n\nLAND THE FILE TEMPLATE-TRUE for its kind against ' +
        (tpl || 'the docgen page-shape') +
        '. ' +
        (spec
            ? 'This is a SPEC page: exactly two strong lead paragraphs at the README exemplar bar — a telos lead (the capability this page owns, its piece in the ' +
              'folder system, the boundary it holds) and a composition lead (the settled facts a rebuild composes before editing: reused axes, seam obligations, ' +
              'wire names, rails, policy rows); then `## [01]-[INDEX]` (one line per cluster, `- [NN]-[TOKEN]: <hook>`, built when absent, rebuilt when stale); then the ' +
              'cluster sections renumbered contiguously. The terminal RESEARCH section is specified below and is mandatory.'
            : 'Land every section, marker, and terminal section its template declares; card and registry kinds keep their own closed marker vocabulary and terminal ' +
              'markers exactly as the template comment declares them.') +
        (spec ? researchLaw : '') +
        '\n\nPROSE REDUCTION: land the page at ' +
        PROSE_LO +
        '-' +
        PROSE_HI +
        '% fewer PROSE lines than the ' +
        f.prose +
        ' baseline. PROSE is non-blank lines OUTSIDE code fences, NOT total file lines — fence code bodies stay UNTOUCHED, so a fence-heavy page keeps nearly its whole ' +
        'total-line count while its prose halves; never chase a total-file reduction and never touch a fence to hit a number. The band applies to the prose measure alone. ' +
        'The demotion ledger, never the band, is the master: zero capability loss outranks any percent. The cut comes from defect-kill and density — ' +
        'delete narration, restatement of fence content, enumeration anchors, deleted-form litanies, process ledger, hedges, meta frames, twin truths, chain restatements; ' +
        'tighten every survivor to the load-bearing-word law. REMOVAL IS THE DEFAULT REPAIR, reframe the exception: a forked, hedged, restated, or chain-bound line is ' +
        'deleted outright — no replacement, no note explaining the removal, and never a substitute `never X` / `not Y` clause minted to stand in for the cut (fresh ' +
        'NEGATION_ONLY sediment; naming a forbidden form primes its re-emission). The positive law already standing forecloses the removed form. Reframe only where the line ' +
        'carries load no other surface holds.' +
        silentAndTrim +
        '\n\nCOLLAPSE IS A RESTRUCTURE, never a brute-delete: a SAME_DECISION_SPREAD merges pure restatements into the single strongest spelling and keeps every DISTINCT ' +
        'clause as its own clause or entry — only true restatements die, and a neighbor separating a different failure class or carrying a false-positive discriminator is ' +
        'distinct law. A nuance lost in the collapse is the same defect as the accumulation; resolution must rise while lines fall, and a merged form is never a concatenated ' +
        'mega-entry where a structured container serves. After every fold, re-scan the destination for a fresh twin the fold minted. SEDIMENT: an oversized single list item ' +
        'is a compressed section wearing a hyphen — classify its fragments (law, mechanism, consequence, exception, example, duplicate) and route each to its container per ' +
        'rewriting.md [05]-[LIST_REPAIR], never shred it into sibling bullets unclassified; a guard justified only by a past failure the current surface, tooling, or agent can ' +
        'no longer commit dies with its dead threat.' +
        '\n\nDEMOTE-VERIFY — capability leaves prose only by PROVEN demotion, and capability loss is the one forbidden move: every decision, invariant, boundary ruling, and ' +
        'trap survives, in fewer words or demoted to the owner that keeps it true. Before a prose copy dies, OPEN the destination owner and confirm it carries EVERY demoted ' +
        'member — across ALL candidate owners (the ownership-interrogation surfaces above), not the first that holds part of it; a fact a RULINGS registry owns more precisely ' +
        'than the architecture doc is a FALSE over-deletion if you stop at the doc. When a prose enumeration demotes to a roster owner, re-verify the surviving pointer inherits ' +
        "NO wider set than the law states — three managers must not arrive as the owner's eight (POINTER-SET-WIDENING). An unopened target is an unproven demotion; if no opened " +
        'owner holds the full set, the law is NOT a fork — keep it here. SPLIT BEFORE DEMOTE at a fork boundary: a compound bullet fusing a charter/bar facet with ' +
        'genuinely-forkable mechanism is split first — mechanism demoted to its owner, facet re-homed at its peer bar — never cut whole because its mechanism has an owner ' +
        'elsewhere; the mirror over-cut is a distinct clause that only RESEMBLES a fork, so classify before cutting. Each crossFile entry is a cross-file demotion: open the ' +
        "named ownerSibling, confirm the full fact, delete this file's copy, and record it as a demotions row. RECORD every demotion: {payload (the decision/member moved out " +
        'of prose), owner (the fence, fence comment, or sibling doc + anchor it demoted into), verified (target opened and confirmed), setWidth (exact|wider|narrower vs the ' +
        "owner's real set)}. A pure deletion of regenerable frame demotes nothing and records nothing. Set capabilityConserved true only when every baseline decision, " +
        'invariant, boundary, and trap survives in prose or a verified demotion.' +
        '\n\nCOMMENT LADDER: cut non-load-bearing fence comments by ~' +
        COMMENT_TARGET +
        '%. Per comment: delete a no-load comment whole (narration, code restatement, a human-facing tour); tighten a load-bearing survivor in place (3 lines to ' +
        '2, 2 to 1 wherever the prose fits); re-wrap toward the 150-column cap, lead packed and tail carrying real width; inline a one-line survivor governing one ' +
        'line as its trailing tail. A constraint the code cannot show survives the cut; a section divider is never deleted. The surviving comment count is a ' +
        'consequence of the ladder, not a quota — a page with only load-bearing comments reports a low cut and that is correct.' +
        '\n\nHONESTY OVER BAND: if an honest full-register rebuild lands outside a band, the register wins — report the true numbers and say why in notes. A padded ' +
        'count, an invented research row, an unproven demotion recorded verified, or an unrun gate reported clean is a defect worse than a missed band.' +
        (planning
            ? rulingsLaw +
              '\n\nEXTRACT-AND-DOSSIER: the ownership interrogation already opened this folder`s RULINGS.md, so you know what it already homes. For each admission-bar' +
              '-passing folder-wide ruling THIS page ORIGINATES that RULINGS does NOT already carry: (1) REMOVE it from the page — the page then composes the ruling ' +
              'silently, no restatement and no pointer that re-narrates it (a page restating a ruling RULINGS ALREADY homes is a plain deletion demoted to the existing ' +
              'owner, never a dossier candidate — no duplication). (2) Author the transcribe-ready RULINGS row in exact template grammar. (3) Record it as a demotion ' +
              'whose owner is the dossier + section, verified true, setWidth exact, so the grader reads the removal as a PROVEN move never a loss. WRITE the dossier: ' +
              '`mkdir -p ' +
              DOSSIER_DIR +
              '` then write ' +
              dossier +
              ' — per candidate a `## [SECTION]` line (the target RULINGS section), the row line beneath it, and a `<!-- source: ' +
              f.path +
              '#<anchor> | tier: package|branch|cross-libs | dedup: <short-key> -->` metadata comment for the collector. A page with NO folder-wide homeless ruling ' +
              'writes ' +
              dossier +
              ' containing exactly `(none applicable)` on one line — extraction is NEVER forced. Set rulingsExtracted to the row count (0 for none applicable), ' +
              'rulingsDossier to ' +
              dossier +
              '.'
            : '\n\nRULINGS: this file is not under a `.planning/` tree and owns no RULINGS.md — set rulingsExtracted -1 and rulingsDossier "".') +
        '\n\nGATE: after the rebuild run `cd ' +
        ROOT +
        ' && uv run .claude/skills/docgen/scripts/prose_gate.py fix --write ' +
        f.path +
        '` then `uv run .claude/skills/docgen/scripts/prose_gate.py ' +
        f.path +
        '`; resolve judgment-tier SKIP rows by hand; gateClean is the second command exiting clean.' +
        '\n\nRECEIPT: re-measure afterProse (' +
        PROSE_MEASURE +
        ') and afterComments (' +
        COMMENT_MEASURE +
        '). Return {file, ok, kind, beforeProse (' +
        f.prose +
        '), afterProse, proseReductionPct, beforeComments (' +
        f.comments +
        '), afterComments, commentReductionPct, demotions (the proven-demotion ledger, empty for a pure-frame deletion), capabilityConserved, indexLanded (' +
        (spec ? 'yes when [01]-[INDEX] is present, no if a spec page could not land it' : 'n/a') +
        '), researchRows (' +
        (spec ? 'terminal row count, 0 for (none)' : '-1') +
        '), rulingsExtracted (' +
        (planning ? 'rows written to the dossier, 0 for none applicable' : '-1') +
        '), rulingsDossier (' +
        (planning ? dossier : '""') +
        '), gateClean, notes}.'
    );
};

const redteamMandate = (f, fix) => {
    const tpl = templatePath(f.kind);
    const spec = f.kind === 'spec';
    const planning = underPlanning(f.path);
    const dossier = dossierPath(f.path);
    return (
        PREAMBLE +
        '\n\n' +
        ACID +
        '\n\nYOU ARE THE RED-TEAM for ONE already-fixed page — the terminal, most aggressive pass. A predecessor ran one ~' +
        PROSE_LO +
        '-' +
        PROSE_HI +
        '% reduction pass over this page; you hold its output NAIVE, INCOMPLETE, and LITTERED until disk proves otherwise, and you both REPAIR its weak work AND drive a ' +
        'SECOND reduction. Read ' +
        CAMPAIGN_METHOD +
        ' [03] RED-TEAM discipline: a pre-mortem that rebuilds rather than annotates, a full cold re-review of every conformance dimension, the current shape never its own ' +
        'defense ("it also reads fine" refutes nothing).' +
        '\n\n' +
        arm(tpl ? 'and the file-kind template: ' + tpl : 'the docgen page-shape law in formatting.md [03]-[PAGE_SHAPE]') +
        ownershipClause([f.path]) +
        '\n\nSEE WHAT THE PREDECESSOR DID: `cd ' +
        ROOT +
        ' && git show HEAD:' +
        f.path +
        '` is the PRE-PASS baseline; the file now on disk is the predecessor`s output. Diff them — what it cut, kept, and reworded. Its receipt: ' +
        JSON.stringify({ afterProse: fix.afterProse, afterComments: fix.afterComments, proseReductionPct: fix.proseReductionPct, notes: fix.notes }) +
        '.' +
        '\n\nHUNT WHAT A FIRST PASS ALWAYS MISSES: litter it introduced (a tombstone note explaining its own removal, a substitute `never X` minted for a cut, a fold that ' +
        'left a fresh twin at the destination, a pointer that re-narrates instead of naming its owner), weak or bloated survivors it kept out of caution, sediment it softened ' +
        'instead of killing, an imported frame it left standing, and char-level padding under a line band it already hit. Read ' +
        f.path +
        ' IN FULL; the predecessor left it at ~' +
        fix.afterProse +
        ' prose lines — drive a FURTHER ' +
        REDTEAM_FURTHER +
        '% reduction off THAT current state (not off the original baseline).' +
        silentAndTrim +
        '\n\nTERRITORY and CAPABILITY are the fixer`s exactly: prose surfaces and fence COMMENTS only; fence code bodies, mermaid bodies, and section dividers UNTOUCHED. ' +
        'Capability conservation is absolute — aggression cuts NOISE, never LAW: every decision, invariant, boundary ruling, and trap survives in fewer words or a proven ' +
        'demotion, and a grader re-derives losslessness from the HEAD baseline after you. Land the page template-true against ' +
        (tpl || 'the docgen page-shape') +
        (spec ? ', its `## [01]-[INDEX]` synced to the real section numbers.' : '.') +
        (spec ? researchLaw : '') +
        (planning
            ? rulingsLaw +
              '\n\nRULINGS DOSSIER: the predecessor may have written ' +
              dossier +
              '. If it exists and carries rows, HARDEN it — every row to guard-grade anatomy, non-guard or page-local content removed, language sharpened to high signal. ' +
              'If the predecessor MISSED a genuine admission-bar folder-wide ruling still sitting in the page, extract it now (remove from the page, add the row, record the ' +
              'demotion). NEVER duplicate a row the dossier or the folder RULINGS.md already holds. Reflect the dossier`s post-hardening state in rulingsExtracted and rulingsDossier.'
            : '') +
        '\n\nGATE then RECEIPT: run `cd ' +
        ROOT +
        ' && uv run .claude/skills/docgen/scripts/prose_gate.py fix --write ' +
        f.path +
        '` then the check; gateClean is the check exiting clean. Return the SAME shape a fixer returns — {file, ok, kind, beforeProse (the ORIGINAL baseline ' +
        f.prose +
        ', so proseReductionPct reads the cumulative HEAD->now cut), afterProse (your final re-measure), proseReductionPct, beforeComments (' +
        f.comments +
        '), afterComments, commentReductionPct, demotions (the cumulative proven ledger), capabilityConserved, indexLanded, researchRows, rulingsExtracted (' +
        (planning ? 'the dossier`s final row count' : '-1') +
        '), rulingsDossier (' +
        (planning ? dossier : '""') +
        '), gateClean, notes}.'
    );
};

const verifyMandate = (f) =>
    PREAMBLE +
    '\n\n' +
    ACID +
    '\n\nYOU ARE THE GRADER for ONE page a fixer then a red-team rebuilt. You re-derive the final page correctness HOSTILELY from disk and edit NOTHING. ' +
    'Compare the current file against its pre-pass state — `cd ' +
    ROOT +
    ' && git show HEAD:' +
    f.path +
    '` is the baseline (if the page is new to HEAD, grade on internal losslessness alone). Grade four axes. (1) LOSSLESSNESS — every decision, invariant, boundary ruling, ' +
    'and trap in the baseline survives, in fewer words or demoted into a fence comment or a sibling owner; a dropped law is a FAIL. A FOLDER-WIDE ruling correctly extracted ' +
    "to a RULINGS dossier is such a demotion, never a loss — open the fixer's rulingsDossier and confirm the full ruling with its guard is present; a page-local design " +
    'decision extracted as if it were a ruling IS an over-deletion. (2) NO OVER-DELETION — a law removed as a ' +
    '"fork" is correct only if its owner PROVABLY holds it; open every demotion target and confirm the FULL member set (POINTER-SET-WIDENING: a 3-member enumeration demoted ' +
    'to a roster owner that now carries 8 is wrong law; and the mirror — a false over-deletion where the true owner is a RULINGS registry the fixer never opened). ' +
    '(3) NO NEW TWIN — a fold that re-duplicated a rule at its destination. (4) NO RESIDUAL FRAME — read the page as the COLD REBUILD AGENT that must work from it alone: name ' +
    'the framing assumption it stands on (an inherited corpus shape, a heading census that does not match its file-kind template, a frozen member roster, a report/process ' +
    "frame, prose restating a diagram), derive the form the page takes WITHOUT it, and set frameClean false when a cold rebuild from this page would inherit that frame; " +
    '"the current frame also reads fine" is not a refutation, and a residual frame is also a residual finding with move=reframe. (5) GATE — the prose gate exits clean on the ' +
    'current file. ' +
    arm('') +
    ownershipClause([f.path]) +
    '\n\nEach overDeletion and residual finding carries the full FINDING shape {class, anchor, problem, direction, move} — move is the corrective the remediator applies ' +
    '(reframe to restore over-deleted law or destroy a residual frame, merge or split where a fold or fork was mishandled). Return {file, grade, losslessness, overDeletion, ' +
    'demotionsProven, newTwin, frameClean, gateClean, residuals}. Edit NOTHING.';

const collectMandate = (owner, rulingsPaths, dossiers) =>
    PREAMBLE +
    '\n\nYOU ARE THE COLLECTOR for one owner`s RULINGS.md — you transcribe the folder-wide rulings the fixers extracted into the decision registry, adjudicating each ' +
    'candidate strictly. Nothing is admitted by default.' +
    rulingsLaw +
    '\n\n' +
    arm('and the RULINGS owner law above') +
    '\n\nRESOLVE THE TARGET: the RULINGS.md for owner `' +
    owner +
    '` is the nearest EXISTING of ' +
    JSON.stringify(rulingsPaths) +
    ' (test -f each, package tier before branch tier). If NEITHER exists, mint the first from ' +
    RULINGS_TEMPLATE +
    ' — its charter lead plus the five sections each marked `(none)`, and set created true. Read the resolved file IN FULL; it is the admission authority.' +
    '\n\nREAD THE DOSSIERS this owner`s fixers wrote: ' +
    JSON.stringify(dossiers) +
    '. Skip any whose sole content is `(none applicable)`.' +
    '\n\nADJUDICATE every candidate row: (a) DEDUP — the same ruling extracted from two pages is ONE row, merged to the single strongest guard-grade spelling ' +
    '(reason=merged-with-sibling for the dropped copy). (b) ALREADY-HOMED — the resolved RULINGS.md, a README/ARCHITECTURE registry, a manifest, or a fence already ' +
    'carries the fact AND its why → reject (reason=already-homed); the fixer`s page-removal already stands and no new row is minted (no duplication). (c) GUARD-GRADE — a ' +
    'row missing its wrong-move, route, or reopen predicate, or carrying two decisions, is repaired to anatomy or split into siblings; an irreparably non-guard candidate ' +
    'is rejected (reason=not-guard-grade). (d) TIER — a ruling scoped WIDER than this owner (branch- or cross-libs-spanning) is NEVER forced into this file; record it in ' +
    'deferred with its tier, and write nothing outside `' +
    owner +
    '`.' +
    '\n\nTRANSCRIBE the survivors into the resolved RULINGS.md: append each at its section`s tail in arrival order (adjacency is arrival, never taxonomy), replace a ' +
    'section`s `(none)` when you first populate it, keep the closed section order, and renumber headings contiguously. Subjects and members are code spans in verified ' +
    'spelling; slugs UPPERCASE_SNAKE; paths repo-relative. Do NOT re-open or edit the source design pages — they are already trimmed; you write RULINGS.md alone.' +
    '\n\nGATE: run `cd ' +
    ROOT +
    ' && uv run .claude/skills/docgen/scripts/prose_gate.py fix --write <rulingsFile>` then the check on the same file; resolve judgment-tier SKIP rows by hand; ' +
    'gateClean is the check exiting clean.' +
    '\n\nRETURN {owner, rulingsFile, created, admitted:[{section, row}], rejected:[{row, reason}], deferred:[{row, tier}], gateClean}.';

const pool = async (thunks, cap, retries = 1) => {
    const results = new Array(thunks.length);
    let next = 0;
    const lane = async () => {
        while (next < thunks.length) {
            const i = next++;
            for (let a = 0; a <= retries; a++) {
                const r = await thunks[i]().catch(() => null);
                if (r) {
                    results[i] = r;
                    break;
                }
            }
        }
    };
    await Promise.all(Array.from({ length: Math.min(cap, thunks.length) }, lane));
    return results;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

if (!TARGETS.length) {
    log('No targets — pass a folder path, an array of paths, or {targets}. Empty args is a no-op.');
    return { skipped: true };
}

phase('Route');
const census = await agent(
    PREAMBLE +
        '\n\nCENSUS these folders: ' +
        JSON.stringify(TARGETS) +
        '. For each, enumerate every .md file recursively (fd -e md . <folder>). For every file record total lines (wc -l), prose lines via exactly ' +
        PROSE_MEASURE +
        ' <file>, and comment lines via exactly ' +
        COMMENT_MEASURE +
        ' <file>. A target that does not exist or holds no .md files goes to missing. Return the flat file list across all targets; no analysis, no reading of ' +
        'page bodies.',
    { label: 'census', phase: 'Route', model: 'opus', effort: 'low', schema: ROUTE_SCHEMA },
);
if (!census || !census.files.length) {
    log('Census returned no files' + (census && census.missing.length ? '; missing: ' + census.missing.join(', ') : ''));
    return { skipped: true, missing: census ? census.missing : TARGETS };
}
if (census.missing.length) log('Missing targets skipped: ' + census.missing.join(', '));

// dedup by path so overlapping parent/child targets never spawn concurrent in-place fixers on the same file
const seenPath = new Set();
const files = census.files
    .filter((f) => !seenPath.has(f.path) && seenPath.add(f.path))
    .map((f) => ({ ...f, kind: kindOf(f.path) }))
    .sort((a, b) => a.path.localeCompare(b.path));

// census file by path; Redteam and Remediate resolve an agent-returned receipt/grade back to its census file here and SKIP an unmatched one, so undefined never reaches a mandate (the `f.path` crash)
const fileByPath = new Map(files.map((f) => [f.path, f]));

// group FOLDER-FIRST so a spec sees its own README/ARCHITECTURE/RULINGS as siblings, not a global path-sort chunk that scatters them
const byFolder = {};
for (const f of files) {
    const dir = f.path.lastIndexOf('/') >= 0 ? f.path.slice(0, f.path.lastIndexOf('/')) : '';
    (byFolder[dir] || (byFolder[dir] = [])).push(f);
}
const groups = Object.values(byFolder).flatMap((g) => chunk(g, GROUP));
const baseProse = files.reduce((a, f) => a + f.prose, 0);
const baseComments = files.reduce((a, f) => a + f.comments, 0);
log(files.length + ' pages, ' + groups.length + ' finder group(s), ' + baseProse + ' prose + ' + baseComments + ' comment baseline lines');

phase('Analyze');
const analyses = (
    await pool(
        groups.map(
            (g) => () =>
                agent(analyzeMandate(g), {
                    label: 'find:' + g[0].path.split('/').slice(-2)[0] + '+' + g.length,
                    phase: 'Analyze',
                    model: 'opus',
                    effort: 'high',
                    schema: ANALYZE_SCHEMA,
                }),
        ),
        CAP,
    )
).filter(Boolean);
const analysisByPath = new Map();
for (const a of analyses) for (const fr of a.files) analysisByPath.set(fr.path, fr);
log(analysisByPath.size + '/' + files.length + ' files analyzed');

phase('Fix');
const fixReceipts = (
    await pool(
        files.map(
            (f) => () =>
                agent(fixMandate(f, analysisByPath.get(f.path)), {
                    label: 'fix:' + f.path.split('/').slice(-2).join('/'),
                    phase: 'Fix',
                    model: 'opus',
                    effort: 'high',
                    schema: FIX_SCHEMA,
                }),
        ),
        CAP,
    )
).filter(Boolean);

// Redteam: adversarial cold second pass over every OK-fixed page — repairs the fixer's litter, hardens the rulings dossier, drives a further reduction. Its receipt
// supersedes the fixer's; a file whose fix failed or whose red-team dropped keeps the fixer receipt so downstream Verify and the return still account for it.
phase('Redteam');
const redteamed = (
    await pool(
        fixReceipts
            .filter((r) => r.ok && fileByPath.has(r.file))
            .map(
                (r) => () =>
                    agent(redteamMandate(fileByPath.get(r.file), r), {
                        label: 'redteam:' + r.file.split('/').slice(-1)[0],
                        phase: 'Redteam',
                        model: 'opus',
                        effort: 'high',
                        schema: FIX_SCHEMA,
                    }),
            ),
        CAP,
    )
).filter(Boolean);
const redByFile = new Map(redteamed.map((r) => [r.file, r]));
const receipts = fixReceipts.map((r) => redByFile.get(r.file) || r);

phase('Verify');
const okReceipts = receipts.filter((r) => r.ok);
// grade by iterating the census files that carry an ok receipt, never by re-finding from a receipt: an agent-returned receipt whose `file` does not match a census path
// resolved to undefined and crashed verifyMandate at `f.path`. A receipt with no matching census file is simply not graded and surfaces under `unverified`.
const okPaths = new Set(okReceipts.map((r) => r.file));
const grades = (
    await pool(
        files
            .filter((f) => okPaths.has(f.path))
            .map(
                (f) => () =>
                    agent(verifyMandate(f), {
                        label: 'grade:' + f.path.split('/').slice(-1)[0],
                        phase: 'Verify',
                        model: 'opus',
                        effort: 'high',
                        schema: VERIFY_SCHEMA,
                    }),
            ),
        CAP,
    )
).filter(Boolean);

phase('Remediate');
const needFix = grades.filter(
    (g) =>
        !g.losslessness ||
        g.overDeletion.length ||
        !g.demotionsProven ||
        g.newTwin === false ||
        g.frameClean === false ||
        !g.gateClean ||
        g.residuals.length,
);
const remediated = (
    await pool(
        needFix
            .filter((g) => fileByPath.has(g.file))
            .map(
                (g) => () =>
                    agent(
                        fixMandate(fileByPath.get(g.file), {
                            findings: [...g.overDeletion, ...g.residuals],
                            crossFile: [],
                        }) +
                            '\n\nSECOND PASS — a grader re-derived your predecessor from disk and found residuals: ' +
                            JSON.stringify(g) +
                            '. Restore any over-deleted law from `git show HEAD:' +
                            g.file +
                            '`, re-home a mis-folded twin, prove every demotion by opening its target across all candidate owners, then re-run the gate.',
                        {
                            label: 'remed:' + g.file.split('/').slice(-1)[0],
                            phase: 'Remediate',
                            model: 'opus',
                            effort: 'high',
                            schema: FIX_SCHEMA,
                        },
                    ),
            ),
        CAP,
    )
).filter(Boolean);

const remByFile = new Map(remediated.map((r) => [r.file, r]));
const finalReceipts = receipts.map((r) => remByFile.get(r.file) || r);
const gradeByFile = new Map(grades.map((g) => [g.file, g]));

// Collect fires ONLY when a .planning/ page actually extracted a folder-wide ruling; a .api/ tier, a tools/ target, or any folder with no .planning/ files skips it
// whole — no phase, no dossier collection, no wasted agents. Owners are disjoint (one RULINGS.md each), so no write collision.
const owners = [...new Set(finalReceipts.filter((r) => r.ok && r.rulingsExtracted > 0).map((r) => ownerOf(r.file)))].filter(Boolean);
let collected = [];
if (owners.length) {
    phase('Collect');
    collected = (
        await pool(
            owners.map((owner) => () => {
                const dossiers = files.filter((f) => underPlanning(f.path) && ownerOf(f.path) === owner).map((f) => dossierPath(f.path));
                return agent(collectMandate(owner, rulingsCandidates(owner), dossiers), {
                    label: 'rulings:' + owner.split('/').pop(),
                    phase: 'Collect',
                    model: 'opus',
                    effort: 'high',
                    schema: COLLECT_SCHEMA,
                });
            }),
            CAP,
        )
    ).filter(Boolean);
    log(collected.length + '/' + owners.length + ' RULINGS.md owner(s) collected');
}

const beforeProseT = finalReceipts.reduce((a, r) => a + r.beforeProse, 0);
const afterProseT = finalReceipts.reduce((a, r) => a + r.afterProse, 0);
const beforeCommT = finalReceipts.reduce((a, r) => a + r.beforeComments, 0);
const afterCommT = finalReceipts.reduce((a, r) => a + r.afterComments, 0);
const dropped = files.length - receipts.length;
if (dropped) log(dropped + ' file agent(s) returned nothing after retry — rerun those files or resume');
return {
    targets: TARGETS,
    pages: files.length,
    fixed: receipts.length,
    remediated: remediated.length,
    proseReductionPct: beforeProseT ? Math.round(100 - (afterProseT / beforeProseT) * 100) : 0,
    commentReductionPct: beforeCommT ? Math.round(100 - (afterCommT / beforeCommT) * 100) : 0,
    proseOffBand: finalReceipts
        .filter((r) => r.ok && (r.proseReductionPct < PROSE_LO || r.proseReductionPct > TOTAL_HI))
        .map((r) => ({ file: r.file, pct: r.proseReductionPct, notes: r.notes })),
    capabilityLoss: finalReceipts.filter((r) => r.ok && !r.capabilityConserved).map((r) => ({ file: r.file, notes: r.notes })),
    demotionsUnverified: finalReceipts.flatMap((r) =>
        (r.demotions || [])
            .filter((d) => !d.verified || d.setWidth !== 'exact')
            .map((d) => ({ file: r.file, payload: d.payload, owner: d.owner, verified: d.verified, setWidth: d.setWidth })),
    ),
    indexMissing: finalReceipts.filter((r) => r.indexLanded === 'no').map((r) => r.file),
    gateUnclean: finalReceipts.filter((r) => !r.gateClean).map((r) => r.file),
    // rulings summary appears ONLY for a run that scoped in .planning/ files; a pure .api/, tools/, or non-libs cleanup returns none of it
    ...(files.some((f) => underPlanning(f.path))
        ? {
              rulingsExtracted: finalReceipts.filter((r) => r.ok).reduce((a, r) => a + Math.max(0, r.rulingsExtracted), 0),
              rulingsAdmitted: collected.reduce((a, c) => a + c.admitted.length, 0),
              rulingsFilesWritten: collected.map((c) => ({ owner: c.owner, file: c.rulingsFile, created: c.created, admitted: c.admitted.length })),
              rulingsRejected: collected.flatMap((c) => c.rejected.map((x) => ({ owner: c.owner, reason: x.reason, row: x.row }))),
              rulingsDeferred: collected.flatMap((c) => c.deferred.map((x) => ({ owner: c.owner, tier: x.tier, row: x.row }))),
              rulingsGateUnclean: collected.filter((c) => !c.gateClean).map((c) => c.rulingsFile),
              dossierDir: owners.length ? SCRATCH : undefined,
          }
        : {}),
    belowBar: grades
        .filter((g) => !g.losslessness || g.overDeletion.length || !g.demotionsProven || g.newTwin === false || g.frameClean === false)
        .map((g) => ({ file: g.file, grade: g.grade, remediated: remByFile.has(g.file) })),
    unverified: okReceipts.filter((r) => !gradeByFile.has(r.file)).map((r) => r.file),
    failed: finalReceipts.filter((r) => !r.ok).map((r) => ({ file: r.file, notes: r.notes })),
    dropped,
};
