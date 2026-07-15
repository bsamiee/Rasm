export const meta = {
    name: 'reframe',
    whenToUse:
        'The standing hostile PROSE + STRUCTURE rebuild for any libs/ planning corpus: pass target folders (a package folder, a branch, several, any language mix); it censuses each folder .planning pages + README/ARCHITECTURE, then per folder runs frame-recon -> reframe-writer -> critique -> red-team CONCURRENTLY, destroying imported frames and rebuilding every page template-true at the docgen bar while conserving ALL semantic content, then one terminal drain loop and a doctrine lander close the run.',
    description:
        'The hostile prose/structure rebuild of planning corpora — the ANTI-ANCHORING engine, sibling to the rebuild engine on the prose dimension and sharing its substrate verbatim. Fence CODE bodies (signatures, types, cases, fields, bodies, design decisions) are UNTOUCHED surfaces; fence comments, section-divider grammar, page structure, README/ARCHITECTURE shape, seam representations, and every durable prose line are the territory. The prime mandate: an IMPORTED FRAME is the prime contaminant — a page wearing another corpus styling (a libs/ spec wearing docs/stacks card framing, a doc predating the docgen templates) poisons every future rebuild that reads it as context and inherits its shape; reframe destroys the frame, rebuilds the page template-true to its own file kind, and conserves ALL semantic content and law (capability conservation is absolute — it deletes noise and frames, never meaning). args = a target folder, an array of folders, or a targets object; empty = no-op; targets may MIX languages, each deriving its file-kind templates and casing from its own branch. Plan (1 sonnet) censuses the target folders into per-folder units with page counts. Per unit, a pipelined chain: (a) FRAME RECON — one gpt-5.6-terra codex lane (read-only, recon law) maps frame defects as FINDINGS never prescriptions — imported/cross-corpus frame poison, drift from the docgen README/ARCHITECTURE/spec templates, text seam-maps that should be mermaid, dead/no-op prose, comment-discipline violations inside fences, section-grammar violations (CLAUDE.md section 09), and the docgen defect classes (STALE_MIRROR, TWIN_TRUTH, META_FRAME, ENUMERATION_ANCHOR, REPORT_FRAME, hedges) — anchored, with coverage; (b) REFRAME WRITER — fable, ordered to load the docgen, skill-writer, and mermaid-diagramming skills BEFORE any edit, reading docs/standards/ (all four pages), the docgen templates, and docs/laws/ at source, then rebuilding README.md + ARCHITECTURE.md template-true (they predate the templates — full re-frame per docgen rewriting.md, never inheriting the source frame), converting text seam maps to one clean mermaid diagram, re-framing poisoned pages, culling dead prose, and repairing comment discipline inside fences WITHOUT touching fence code, running the docgen prose gate to zero FAILs; (c) SOL CRITIQUE — a gpt-5.6-sol codex lane (workspace-write, native-twin fable), predicate-positive conformance audit against the docgen register/templates/section grammar, fixing in place, fixlog to disk; (d) FABLE RED-TEAM — predicate-negative with fold-forward duty, hunting RESIDUAL ANCHORING (any page still carrying the old frame, poisoned styling, a stale mirror, twin truth) under the counterfactual: would a cold agent rebuilding from this page inherit poison? Close: pooled deferred rows drain through the terminal fable DRAIN LOOP; pooled harvest nominations route through the doctrine lander; orphaned critique fixlogs drain with the backlog. Nothing follows the drain.',
    phases: [
        {
            title: 'Plan',
            detail: 'one thin agent censuses the target folders into per-folder reframe units — the .planning page set plus the README/ARCHITECTURE governing docs, with page counts',
            model: 'sonnet',
        },
        {
            title: 'Reframe',
            detail: 'all folder units concurrent under the agent-level slot cap: per unit a gpt-5.6-terra frame-recon lane (read-only) then reframe-writer (fable), critique (gpt-5.6-sol codex lane, fixlog to disk), red-team (fable, folding the critique rows forward) chained behind their own unit; navigation-fact handoffs, libs-wide bounded ripple authority',
        },
        {
            title: 'Close',
            detail: 'ONE terminal fable drain loop reads the pooled deferred backlog + index rows + orphaned critique fixlogs, applies every row, drains the backlog, hunts past it; then the doctrine lander adjudicates the pooled harvest nominations against the live doctrine surfaces',
        },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT_DIR = '/Users/bardiasamiee/Documents/99.Github/Rasm'; // absolute resolution root for every relative path; codex lanes pin it as cwd, native terminal lanes as WORKING ROOT
const CAP = 14; // runtime concurrency clamp is min(16, cores-2) = 14 on this machine; matching it keeps the stagger honest
const STAGGER_MS = 1500;
const STALL = 300000;
const DRAIN_ROUNDS = 4; // terminal drain fixpoint cap; the progress gate (no shrinkage -> stop) is the real bound
const RETRY_ATTEMPTS = 2; // re-dispatches per dead critical lane; the count bounds spend, the backoff buys recovery time
const RETRY_BACKOFF = 1800000; // usage-limit deaths clear on reset or an operator credit top-up; each attempt waits the window out first
const CODEX_STALL = 7500000; // wrapper stall sits ABOVE the client MCP ceiling (fleet codex.toolTimeoutSec = 7200s): the client aborts a wedged call first; this guards only a dead wrapper
const CODEX = true; // frame-recon + critique lanes run on gpt-5.6 via the codex wrapper; false restores native lanes (terra->opus, sol->fable)

// --- [INPUTS] --------------------------------------------------------------------------

const normTarget = (t) => String(t).trim().replace(/\/+$/, '').replace(/^\/+/, '');
// Hosts may deliver object args JSON-encoded; decode before shape dispatch.
const argsIn = typeof args === 'string' && /^\s*[\[{]/.test(args) ? JSON.parse(args) : args;
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
const langOf = (t) =>
    t.indexOf('libs/csharp') === 0 ? 'cs' : t.indexOf('libs/python') === 0 ? 'py' : t.indexOf('libs/typescript') === 0 ? 'ts' : null;
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))].filter((t) => langOf(t));
const REJECTED = [...new Set(rawTargets.filter(Boolean).map(normTarget))].filter((t) => !langOf(t));
// Per-instance scratch dir — frame-recon report files + critique fixlogs. Minted deterministically from the normalized target set
// (clock/randomness would break resume): one FLAT dir under .claude/scratch/, a basename slug plus an FNV-1a tail so distinct sets never collide.
const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH =
    '.claude/scratch/' +
    ('reframe-' + TARGETS.map((t) => t.split('/').pop().toLowerCase()).join('-')).replace(/[^a-z0-9.-]+/g, '-').slice(0, 60) +
    '-' +
    fnv1a(JSON.stringify(TARGETS));

// --- [MODELS] --------------------------------------------------------------------------

const DISCOVER_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['units', 'unresolved'],
    properties: {
        units: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['folder', 'planning', 'context_root', 'pages'],
                properties: {
                    folder: { type: 'string' }, // the package folder, repo-relative (e.g. libs/typescript/geo)
                    planning: { type: 'string' }, // the .planning tree holding the design pages
                    context_root: { type: 'string' }, // the dir whose README.md/ARCHITECTURE.md/IDEAS.md/.api govern the planning
                    pages: { type: 'integer' },
                },
            },
        },
        unresolved: { type: 'array', items: { type: 'string' } },
    },
};

const COVERAGE = {
    type: 'object',
    additionalProperties: false,
    required: ['requested', 'read', 'skipped', 'unverified'],
    properties: {
        requested: { type: 'array', items: { type: 'string' } },
        read: { type: 'array', items: { type: 'string' } },
        skipped: { type: 'array', items: { type: 'string' } },
        unverified: { type: 'array', items: { type: 'string' } },
    },
};

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const ANCHOR_DEFECT = {
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: { type: 'string' },
        line: { type: 'integer' },
        role: { type: 'string', enum: ['defect', 'frame', 'seam', 'counterpart', 'absence'] },
        note: { type: 'string' },
    },
};

// Frame-recon product: frame/prose/structure defects as FINDINGS, never designs — the writer rules every rebuild.
const FRAME_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['findings', 'coverage', 'summary'],
    properties: {
        findings: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'class', 'severity', 'claim', 'mechanism', 'owner', 'reject', 'acceptance', 'anchors'],
                properties: {
                    page: { type: 'string' },
                    class: {
                        type: 'string',
                        enum: [
                            'imported-frame',
                            'template-drift',
                            'text-seam',
                            'dead-prose',
                            'comment-discipline',
                            'section-grammar',
                            'stale-mirror',
                            'twin-truth',
                            'meta-frame',
                            'enumeration-anchor',
                            'report-frame',
                            'hedge',
                            'coupling',
                        ],
                    },
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: { type: 'string' }, // the observed frame/prose defect as fact
                    mechanism: { type: 'string' }, // WHY it poisons a future rebuild — factual, zero repair verbs
                    owner: { type: 'string' }, // the file kind / template / register clause whose application resolves it
                    reject: { type: 'array', items: { type: 'string' } }, // forms the repair must NOT take
                    acceptance: { type: 'array', items: { type: 'string' } }, // signals proving the frame is gone
                    anchors: { type: 'array', items: ANCHOR_DEFECT },
                },
            },
        },
        coverage: COVERAGE,
        summary: { type: 'string' },
    },
};

// Thin wire receipt: the lane's PRODUCT stays on disk at `report`; only status + count + headline travel inline.
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

const SEAMS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['file', 'counterpart', 'bothEnds'],
        properties: { file: { type: 'string' }, counterpart: { type: 'string' }, bothEnds: { type: 'boolean' } },
    },
};

const DELTAS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['symbol', 'change'],
        properties: { symbol: { type: 'string' }, change: { type: 'string' } },
    },
}; // navigation facts: which page/section/seam re-framed, as data, zero adjectives

const DEFERRED = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['files', 'claim'],
        properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } },
    },
}; // the counted backlog: cross-unit + second-order prose/seam ripples

const INDEXROWS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['doc', 'row'],
        properties: { doc: { type: 'string' }, row: { type: 'string' } },
    },
}; // doc = owning-package index doc, central manifest, or IDEAS.md; row = the exact row text (single-writer via the drain loop)

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
}; // doctrine nominations — generalizable lessons only; the terminal doctrine lander adjudicates every row

// Writer and review fixlogs share one field core; only the verdict vocabulary forks by stage. Required-but-empty arrays are
// attestations: forced seamsTouched/deltas/deferred/indexRows make "repair both ends / record the backlog / route decisions
// single-writer" structurally checkable, never wishful prose. `logSchema(extra)` composes the core plus the stage's verdict key.
const LOG_CORE = {
    files: { type: 'array', items: { type: 'string' } },
    summary: { type: 'string' },
    seamsTouched: SEAMS,
    deltas: DELTAS,
    deferred: DEFERRED,
    indexRows: INDEXROWS,
    harvest: HARVEST,
};
const logSchema = (extra) => ({
    type: 'object',
    additionalProperties: false,
    required: Object.keys(LOG_CORE).concat(Object.keys(extra)),
    properties: Object.assign({}, LOG_CORE, extra),
});
const REFRAME_LOG = logSchema({ verdict: { type: 'string', enum: ['reframed', 'rebuilt', 'refined', 'clean'] } });
const REVIEW_LOG = logSchema({ verdict: { type: 'string', enum: ['fixed', 'clean'] } });

// Required-but-possibly-empty `beyond` is an attestation: the fixer's own hunt ran, not only the row list.
const FIXER_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'indexApplied', 'resolved', 'backlogDrained', 'beyond', 'rejected', 'remaining', 'harvest', 'tranches', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        harvest: HARVEST,
        tranches: { type: 'array', items: { type: 'string' } }, // one checkpoint receipt line per tranche fed this round — a fed tranche absent here is unconsumed

        indexApplied: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['doc', 'action'],
                properties: { doc: { type: 'string' }, action: { type: 'string' } },
            },
        },
        resolved: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['target', 'action'],
                properties: { target: { type: 'string' }, action: { type: 'string' } },
            },
        },
        backlogDrained: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['claim', 'action'],
                properties: { claim: { type: 'string' }, action: { type: 'string' } },
            },
        },
        beyond: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['target', 'action'],
                properties: { target: { type: 'string' }, action: { type: 'string' } },
            },
        },
        rejected: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['finding', 'reason'],
                properties: { finding: { type: 'string' }, reason: { type: 'string' } },
            },
        },
        remaining: DEFERRED, // rows verified still-open; the drain loop re-feeds them until empty or no progress
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

// LANG carries routing data only — the doctrine content is docgen + docs/standards, reached at source, never paraphrased here.
const LANG = {
    cs: {
        key: 'cs',
        name: 'C#',
        root: 'libs/csharp',
        casing: 'PascalCase',
        marker: '//',
        docBloat: 'XML-doc',
        corpus: 'libs/csharp planning corpus (markdown design specs of intended C# packages)',
    },
    py: {
        key: 'py',
        name: 'Python',
        root: 'libs/python',
        casing: 'snake_case',
        marker: '#',
        docBloat: 'docstring',
        corpus: 'libs/python planning corpus (markdown design specs of intended Python modules)',
    },
    ts: {
        key: 'ts',
        name: 'TypeScript',
        root: 'libs/typescript',
        casing: 'camelCase',
        marker: '//',
        docBloat: 'TSDoc',
        corpus: 'libs/typescript planning corpus (markdown design specs of intended TypeScript modules)',
    },
};

// --- [SHARED_BLOCKS]

// Every rigor law appears exactly once, here; stages compose subsets. Block order in prompts:
// stable per-language context first, the frame + docgen law next, the stage task + output contract LAST.
const CONTEXT = (L) =>
    'Rasm monorepo — ' +
    L.corpus +
    '. These pages are markdown design specs FAR from done: the rebuild-* workflows will later realize them, RELYING on ' +
    'prose + comments to carry the intent, invariant, seam, and rationale the code fences alone cannot hold. This run rebuilds ' +
    'PROSE and STRUCTURE only.';

// reg selects the register by the EXECUTING model: 'codex' neutral+de-conflicted for a codex lane (frame, sol
// critique), 'claude' the hostile estate register a native lane (writer, red-team) reads as sharpening. Substance is
// identical — two naivety axes, floor-hunted-past, no-churn, verify-against-template — only the phrasing forks.
const STANCE = (reg) =>
    reg === 'codex'
        ? 'REVIEW POSTURE — the pages were framed by ANOTHER engineer and are unverified: verify every structural choice against ' +
          'the file kind`s template and the docgen register before accepting it; a prior clean verdict or confident, ' +
          'official-looking prose is not evidence — a page that reads as authoritative is exactly the one a future rebuild ' +
          'anchors to hardest, so check its framing rather than trust it. NAIVETY is a defect on two axes. COVERAGE — the pass ' +
          'worked a thin slice: short pages, page tops, the first screen of long fences, while deep sections, bottom-half ' +
          'fences, and table cells kept their frame. APPROACH — enumerated spot-fixes where one rule owns the space: a recurring ' +
          'hedge, restatement, frame, or divider defect is a FAMILY swept corpus-wide with one rule, the found instances mere ' +
          'pointers to it. Every enumerated defect list in these prompts is a floor hunted past, never the complete set. NO ' +
          'CHURN: an edit requires a named violated register law or template rule and the concrete line that breaks it; a clean ' +
          'verdict from a check that finds nothing is a first-class result, proven by adding nothing.'
        : 'STANCE — every pass is hostile: author, critique, and red-team alike. The pages under review were framed by ANOTHER ' +
          'engineer and are under adversarial review; hold every page`s framing poisoned, drifted, or noise-bloated until it survives ' +
          'a real attack; the burden of proof is on the page, never on you. "Already clean", "good enough", and a prior clean verdict ' +
          'are rejected self-assessments. Dense, confident, official-looking prose is the PRIME suspect: a page that reads as ' +
          'authoritative is exactly the one a future rebuild anchors to hardest, so disbelieve its framing and verify every structural ' +
          'choice against the file kind`s template and the docgen register. NAIVETY is a defect on two axes. COVERAGE — the pass worked ' +
          'a thin slice: short pages, page tops, the first screen of long fences, while deep sections, bottom-half fences, and table ' +
          'cells kept their frame. APPROACH — enumerated spot-fixes where one rule owns the space: a recurring hedge, restatement, ' +
          'frame, or divider defect is a FAMILY swept corpus-wide with one rule, the found instances mere pointers to it. Every ' +
          'enumerated defect list in these prompts is a FLOOR hunted past, never the complete set. NO CHURN: an edit requires a named ' +
          'violated register law or template rule and the concrete line that breaks it; a clean verdict earned by an attack that finds ' +
          'nothing is a first-class result, proven by adding nothing — a reviewer forced to edit invents defects.';

const TERRITORY = (L) =>
    'TERRITORY — reframe rebuilds PROSE and STRUCTURE, never code. UNTOUCHED (a change here is a defect the next stage reverts): ' +
    'every code fence`s design — signatures, types, cases, fields, bodies, ordering, and the design decisions a page makes; do ' +
    'NOT add, remove, or reorder a fence`s design content. THE TERRITORY: fence comments and their `' +
    L.marker +
    ' --- [SECTION]` divider grammar, page prose (leads, cards, section text, bullets, table cells), section structure and ' +
    'labels, README.md and ARCHITECTURE.md page shape, and seam REPRESENTATIONS (a text seam-map rebuilt as one mermaid ' +
    'diagram). CAPABILITY CONSERVATION IS ABSOLUTE: reframe deletes noise and imported frames, never MEANING — every ' +
    'load-bearing intent, invariant, boundary, rationale, seam, and cross-reference a future rebuild needs SURVIVES, refined. ' +
    'When unsure a line is load-bearing, KEEP it refined, never delete: removing real context to hit a leanness target is a ' +
    'defect worse than slight bloat.';

const FRAME_LAW =
    'FRAME LAW (the prime mandate) — an IMPORTED FRAME is the prime contaminant. A page wearing another corpus`s styling (a ' +
    'libs/ spec wearing docs/stacks card framing, an ARCHITECTURE page wearing a README`s shape, any doc predating the docgen ' +
    'templates) POISONS every future rebuild that reads it as context and inherits its shape. Destroy the frame; rebuild the ' +
    'page TEMPLATE-TRUE to its OWN file kind, its heading census matching the template; conserve ALL semantic content and law. ' +
    'ANTI-ANCHORING: loaded prose is the working model the next agent generates inside, so every shape the prose names becomes a ' +
    'wall — a frozen member roster anchors the rebuild to today`s census, a forbidden-alternative litany to the anti-shape, a ' +
    'report/process frame to the authoring session. Every such anchor is destroyed and replaced with the EXTENSION RULE (how one ' +
    'new row, case, or member lands) stated where the anchor stood, the roster living on the surface tooling keeps true.';

const DOCGEN_LAW =
    'DOCGEN — this run`s doctrine is docgen, not the code stacks. Load the `docgen` skill AND the `skill-writer` skill via the ' +
    'Skill tool BEFORE any edit; load `mermaid-diagramming` before authoring any diagram. Read AT SOURCE, in full: ' +
    '`docs/standards/` — `design-doctrine.md`, `information-structure.md`, `style-guide.md`, `formatting.md`; the docgen ' +
    'templates (`readme`, `architecture`, `spec`, `api-catalog`, `ideas`, `tasklog`) for the file kind of each page you touch; ' +
    'and `docs/laws/` (README + topology + patterns + scars). Hold the docgen register (OWNING_SUBJECT, DECISION_PER_LINE, ' +
    'NAMED_SURFACE, CAPABILITY, the FACT_LAW classes), the named defect catalog (STALE_MIRROR, TWIN_TRUTH, META_FRAME, ' +
    'ENUMERATION_ANCHOR, REPORT_FRAME, HEDGE, CAPABILITY_GATE, LEGACY_COMPAT, IMPORTED_POSTURE, COUPLING, and siblings), and the ' +
    'comment discipline as binding law at the source; this prompt never restates them. README.md and ARCHITECTURE.md are ' +
    're-framed GROUND-UP per the docgen `references/rewriting.md` procedure — never inherit the source frame.';

const DIVIDERS = (L) =>
    'DIVIDER GRAMMAR (CLAUDE.md [09]): a CANONICAL top-level section divider is `' +
    L.marker +
    ' --- [LABEL]` + dash-fill to the language width, using a canonical label ' +
    '(`TYPES`/`CONSTANTS`/`MODELS`/`ERRORS`/`SERVICES`/`OPERATIONS`/`COMPOSITION`/`EXPORTS`) or a precise domain extension ' +
    '(`[TABLES]`/`[BOUNDARIES]`/`[REPOSITORIES]`/`[POLICIES]`/`[INDEXES]`/`[MIDDLEWARE]`/`[ENTRY]`). A SUB-SECTION divider uses ' +
    'the LOOSER `' +
    L.marker +
    ' --- [LABEL]` form with NO dash-fill. KEEP every divider; repair a malformed one to its correct form; a phantom or ' +
    'mislabeled divider is corrected only after reading the enclosing section structure; NEVER invent a divider, convert a loose ' +
    'sub-section into a dash-filled canonical one or vice versa, or relabel a canonical section to a drift label ' +
    '(`SCHEMA`/`FUNCTIONS`/`HELPERS`/`UTILS`/`MISC`). Dividers are STRUCTURE, exempt from the comment-trim mandate.';

const COMMENT_LAW = (L) =>
    'FENCE COMMENTS — every comment inside a code fence is agent-facing framing for the future rebuild agent: one in-situ ' +
    'constraint the code cannot show — intent, invariant, contract, boundary. DELETE a comment that restates the code, ' +
    'narrates, or carries task/session/subagent/review-label/proof/history/process framing. REFINE every survivor for maximum ' +
    'signal, a comment line filling toward the 150-column width before wrapping; a block is 1-2 lines, 3-4 only for a genuinely ' +
    'subtle multi-part invariant, and a wrapped block never ends on a runt. No ' +
    L.docBloat +
    ' bloat. Comment repair is read-and-rewrite judgment ONLY — never a sed/regex/scripted bulk pass. NEVER touch the fence CODE ' +
    'while repairing its comments.';

const SEAM_MERMAID =
    'SEAM REPRESENTATIONS — a page representing cross-page or cross-package seams as ASCII art, an indented text tree, or a ' +
    'prose adjacency list is a REPRESENTATION defect: rebuild it as ONE clean mermaid fence per the `mermaid-diagramming` skill ' +
    '(YAML frontmatter, ELK layout, Dracula theme tokens), carrying EVERY edge the text carried and none invented. A ' +
    'representation is a fenced diagram verified from disk, never restated in prose beside it (FACT_LAW REPRESENTATION class).';

const RIPPLE_LAW =
    'RIPPLE LAW — every defect you find you FIX NOW via Edit/Write; the fix-log reports edits ALREADY MADE, never a to-do, a ' +
    'ledger, or a would/should hedge. The writing is YOURS: never delegate authoring to another agent — a delegate may only ' +
    'fetch information. Ripple authority is LIBS-WIDE, corrective AND generative, under evidence bounds that are never radius. ' +
    '(1) EVIDENCE — an out-of-unit edit traces to a resolvable anchor: a broken cross-reference far end, a seam counterpart, an ' +
    'index claim; an edit with no anchor is drift, forbidden. (2) EXPAND-FORM — a foreign edit made while sibling units run is ' +
    'ADDITIVE (fix the far end of a reference, re-frame a poisoned counterpart page at ITS file kind`s template bar); a rename ' +
    'or structural collapse of a foreign surface is recorded in `deferred` for the terminal fixer, never raced; wire-canonical ' +
    'names stay frozen. (3) DEPTH — a first-order ripple (your edit broke or opened it directly) is repaired both ends now and ' +
    'recorded in `seamsTouched`; a second-order ripple or a counterpart inside a concurrent unit`s scope is recorded in ' +
    '`deferred` as {files, claim}. (4) DECISION — decision-carrying shared surfaces are SINGLE-WRITER: the owning-package index ' +
    'docs (ARCHITECTURE.md + README.md at the path before `/.planning/`), IDEAS.md, and the central manifests take exact rows ' +
    'via `indexRows` for the terminal fixer to apply once — a README dependency section rebuilt to match the central manifest ' +
    'and the `.api` catalog inventory reports any manifest drift it proves as an `indexRows` row, never a hand-raced edit.';

const CURRENT_STATE =
    'CURRENT STATE — sibling units land work concurrently with yours. Before any edit, re-read the CURRENT on-disk state of ' +
    'your pages AND every sibling page your pages cross-reference or ripple into; landed sibling work is picked up as found, ' +
    'never assumed. A cross-reference far end a sibling re-framed is COMPOSED, not re-derived; a conflict between your framing ' +
    'and a landed sibling resolves to the stronger form, never a revert.';

const INFO_LAW =
    'You provide INFORMATION, never prescriptions: exact disk locations and anchors, the current framing at each site, the file ' +
    'kind each page should wear, seam endpoints on both sides, the register clause each defect violates. The writer decides how ' +
    'to rebuild; a finding that tells it what to write instead of what is true is a defect. ENTRY FORM: prose fields carry ' +
    'fact; `anchors` carry one coordinate per row (role names what it proves; `note` is the shortest literal witness under 20 ' +
    'words, or empty when path+line suffice; an `absence` anchor names where the expected thing was searched and not found). ' +
    'COVERAGE is part of the product: `requested` = your assigned scope, `read` = what you actually full-read, ' +
    '`skipped`/`unverified` = what you did not reach — an honest skip beats a silent one.';

const ANTI_ANCHOR =
    'ANTI-ANCHOR LAW: your report carries FINDINGS, never designs — frame/prose/structure defects named against the docgen ' +
    'register and the file-kind templates read at source (name the register law, defect class, or template rule whose ' +
    'application would most deeply re-frame the page — never the resulting prose); a rewritten passage, a prescribed section ' +
    'spine, or a pre-authored diagram ANCHORS and WEAKENS the writer and is your defect — the writer rules every rebuild.';

const SELF_CHECK =
    'MANDATORY SELF-VERIFY (second pass, before returning): re-derive every finding from disk — re-open each ' +
    'cited anchor and confirm it states what the finding claims, trace each seam to both endpoints. A finding that fails ' +
    're-confirmation is corrected or deleted, never returned; a guess, a skimmed summary, or a vague/hedged finding is a defect. ' +
    'Completeness is part of correctness: after the re-read, hunt once more for the frame the first pass missed.';

const HARVEST_LAW =
    'HARVEST (required key, usually empty): nominate ONLY findings that generalize beyond this unit — a re-framing pattern ' +
    'reusable across folders, a frame-poison class no docgen defect names, a register or template rule that would have caught a ' +
    'defect BEFORE review. Each row: altitude (stacks|reviewer|constitution|planning|readme|laws), lang, claim (the generalized ' +
    'law, one sentence, SYMBOL-FREE — every concrete spelling lives in anchors, so the lander adjudicates the law without ' +
    're-deriving its locality), anchors (file:line evidence), existingClause (the exact doctrine or template clause it would harden, ' +
    'quoted with its path — or "absent" plus the surfaces searched). A unit-local fix never nominates; an empty array is the ' +
    'normal verdict — the doctrine lander refutes weak rows, so nominate substance, never volume.';

const GIT_GROUND =
    'DELTA GROUNDING — run `git diff --stat` then `git diff -- <your unit`s pages and index docs>` to see exactly what this run ' +
    'changed before judging it; `git status` surfaces new files. The diff is orientation, CURRENT disk is truth — the repo ' +
    'carries pre-run uncommitted work, so an unfamiliar hunk is verified against disk, never assumed to be this run`s.';

const READ_FIRST = (L, u) =>
    'READ FIRST, IN ORDER, BEFORE ANY EDIT. (1) Load `docgen` + `skill-writer` via the Skill tool; load `mermaid-diagramming` ' +
    'before any diagram. (2) Read `docs/standards/` (all four pages), the docgen templates for the file kinds present, and ' +
    '`docs/laws/` — at source, in full; a `docs/laws/topology.md` row whose [SURFACE] your edits touch binds its counterparts ' +
    'into the SAME pass. (3) SCOPE — read the owning-package charter at `' +
    u.context_root +
    '` (README.md + ARCHITECTURE.md + IDEAS.md) and enumerate BOTH `.api` tiers (`' +
    L.root +
    '/.api/` and the folder `.api/`) with a REAL `ls` — the charter is the INTENT authority for what each page owns, and the ' +
    '`.api` inventory is the VERIFICATION GROUND for any prose claim about a member, package, or capability (a claim neither ' +
    'the fence nor either tier verifies is a PHANTOM corrected from the catalogs or deleted). (4) Read EVERY design page under `' +
    u.planning +
    '` IN FULL — never a skim, grep-jump, or section-sample.';

const OWN_PASS = (artifact) =>
    'OWN PASS FIRST — the input ladder is binding, in order: (1) your own blind cold frame-read, (2) the frame-recon report. ' +
    'Rung (1) is the PRIMARY product and it is a DISK ARTIFACT, not a reading step: cold-read every target page from CURRENT ' +
    'disk and WRITE your own frame-defect-and-reframe list to `' +
    artifact +
    '` — imported frames, template drift, stale mirrors, twin truths, text seam maps, dead prose, comment-discipline and ' +
    'divider defects, and every register or template rule each page breaks — BEFORE opening the frame-recon report. The recon ' +
    'may only ADD rows to that file, each tagged [recon]; reading the pages without writing the list is a failed rung, not a ' +
    'cold pass. Rung (2) grounds, verifies, and widens YOUR pass — it never scopes, substitutes for, or caps it. TRIPWIRE: a ' +
    'diff dominated by [recon]-tagged rows has failed — the recon covers a MINORITY of what the reframe demands, and the ' +
    'majority of your edits must come from your own attack.';

const reconBlock = (framed, unmapped) =>
    'FRAME REPORT — the frame-recon product is ON DISK as a JSON report; the receipt below is navigation, never the product. ' +
    'CONSUMPTION: (a) UNMAPPED territory below (a dead recon) gets your OWN cold frame-read FIRST — that dimension over your ' +
    'pages is yours to derive; (b) read the ok report IN FULL from disk — its findings are the frame defects mapped for you, ' +
    'each anchor a jump coordinate you re-open before editing; (c) the report POINTS, you VERIFY and EXCEED it — a frame defect ' +
    'it missed is still yours, and its `owner`/`reject`/`acceptance` are the constraint boundary while the DESIGN is yours.\n' +
    'RECEIPT: ' +
    JSON.stringify(framed) +
    '\nUNMAPPED: ' +
    JSON.stringify(unmapped);

// --- [OPERATIONS] ----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms));
// Agent-level slot scheduler: CAP agents in flight across ALL unit chains, staggered launch, work-conserving backfill the moment a
// slot frees. The single governor for every agent call.
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
// Bounded re-dispatch for a dead CRITICAL lane (usage-limit or transport death): attempt-counted with a backoff before each; the
// final death isolates the lane but NEVER the chain — every downstream stage still runs against current disk.
const retryLane = async (fn) => {
    for (let a = 0; a < RETRY_ATTEMPTS; a++) {
        await sleep(RETRY_BACKOFF);
        const r = await fn();
        if (r) return r;
    }
    return null;
};

// Codex dispatch: the sonnet wrapper makes one blocking Codex MCP call, writes the envelope's content
// to the lane report, and returns mechanical orchestration data. Lane law rides developer-instructions;
// the prompt carries only the task; the output contract sits LAST.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
// Stage-distinct own-pass artifact path — one per unit per consuming stage, so a writer, critique, and red-team never share a file.
const ownPassArt = (folder, stage) => SCRATCH + '/' + fileTag(folder.split('/').pop()) + '-' + stage + '-ownpass.md';
const laneLaw = (schema, o) =>
    (o.fix
        ? '<completion_bar>\nDone is every page in your named scope worked to its full depth with its fixlog entry written — ' +
          'proof-complete, never effort-spent, never early. Complete every named move before yielding; do not stop at analysis ' +
          'or a partial edit. If the chosen approach resists, pick the next-best one and proceed; a move the territory genuinely ' +
          'admits no edit for returns as a deferred row naming its blocker. Your layer is review-and-repair of the named scope: ' +
          'a finding outside it lands as a typed deferred/index row, never an edit — and re-verifying unchanged work or ' +
          're-reading covered territory adds no evidence; move to the next deliverable instead.\n</completion_bar>\n\n' +
          '<verification>\nAfter editing, re-read each changed file and confirm it is coherent and nothing it carried was lost. ' +
          'Fix what fails before yielding; a check you did not run is never claimed as run.\n</verification>'
        : '<context_gathering>\nTerritory: the exact files and directories the task names. Do not open files outside it, ' +
          'including skill or instruction files (.claude/, CLAUDE.md, AGENTS.md) beyond those the task explicitly names.\nBudget: ' +
          'at most ' +
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
    const root = ROOT_DIR;
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
            'VERBATIM. If the call errors with a TIMEOUT or idle abort, the codex session CONTINUES server-side' +
            (o.writes
                ? ' and writes its own report — do NOT re-dispatch (a retry mints a duplicate concurrent writer on the same ' +
                  'files): poll `jq -e . <report path>` with Bash every 120s for up to 40 minutes; the report appearing IS ' +
                  'completion — proceed to step (4) from its content. Only a NON-timeout error retries the identical call ONCE.'
                : ' but its product is lost to this wrapper — retry the identical call ONCE, as with any other error.') +
            ' If the retry errors, skip step (3) and return the error through step (4).',
        'LANE LAW:\n\n' + laneLaw(schema, o),
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
              ' >/dev/null. If the file is missing or invalid, extract the CONTENT text from the tool result envelope {threadId, content} ' +
              'and Write it to that path verbatim (the product JSON, never the envelope), then re-verify.'
            : '(3) The tool result is a JSON envelope {threadId, content} whose content field holds the final-message text. ' +
              'Write that CONTENT text (the product JSON, unescaped) — never the envelope — with the Write tool to this absolute path: ' +
              report +
              '. Do not normalize, reformat, summarize, or extract the text before writing it. Then verify with one Bash call: jq -e . ' +
              report +
              ' >/dev/null — a Write that drops the tail mints invalid JSON; on failure rewrite once from the tool result, and a second ' +
              'failure returns through step (4) with the error.',
        '(4) Parse the tool result text only for mechanical orchestration data. Return ok=true, report=' +
            base +
            '-report.json, entries=the length of result["' +
            o.hl.arr +
            '"], headline="<entries> ' +
            o.hl.arr +
            (o.hl.group ? ' | <' + o.hl.group + ' tallies>' : '') +
            ' | top: <most frequent first file or none>", and failure empty. On a second tool error return ok=false, entries=0, ' +
            'report and headline empty, and failure equal to the error text VERBATIM.',
    ].join('\n\n');
};
// Every codex-dispatched lane routes here: terra by default, sol where o.model says so; CODEX=false restores a native run. QUOTA FALLBACK: a codex
// receipt whose failure matches usage/quota/limit re-dispatches the SAME task natively at the role's Claude twin (terra->opus, sol->fable,
// luna->sonnet); the caller owns re-dispatch, the sonnet wrapper never executes work itself. The roster row carries `scope` from the ORCHESTRATOR so
// a failed lane's unmapped territory is exact even when the lane died before writing anything.
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
        {
            label: o.label,
            phase: o.phase,
            model: o.nativeModel || twinOf(o.model),
            effort: 'high',
            schema: RECEIPT,
            stallMs: o.stallMs || STALL,
        },
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
const chunk = (arr, n) => {
    const o = [];
    for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n));
    return o;
};
const Lof = (folder) => LANG[langOf(folder)] || LANG.cs;
// Navigation handoff: FACTS ONLY — files, structural deltas, seam rows, backlog. Never verdicts or adjectives.
const navOf = (logs) => {
    const rows = logs.filter(Boolean);
    return {
        files: [...new Set(rows.flatMap((r) => r.files || []))],
        deltas: rows.flatMap((r) => r.deltas || []),
        seams: rows.flatMap((r) => r.seamsTouched || []),
        deferred: rows.flatMap((r) => r.deferred || []),
    };
};

// Prompt builders — each task states only its own action; shared law is composed by name from the blocks above.
const planPrompt = () =>
    [
        'Rasm monorepo — the libs/{csharp,python,typescript} planning corpora (markdown design specs). Targets may mix ' +
            'languages; each folder derives its own file-kind templates and casing downstream.',
        'TASK: thin census + classify (read-only, do NOT edit). TARGET FOLDERS (repo-relative): ' +
            JSON.stringify(TARGETS) +
            '. Each target is a package folder OR a branch/parent folder holding several. EXPAND with a real recursive listing ' +
            'per target — run find <target> -name *.md — and enumerate every PACKAGE FOLDER under it that owns design pages in a ' +
            '`.planning/` tree. For each such folder return {folder (the package dir), planning (the `.planning` dir holding the ' +
            'design pages), context_root (the dir whose README.md/ARCHITECTURE.md/IDEAS.md + `.api/` govern that planning — for a ' +
            'normal package folder = the folder itself), pages (the count of `.md` design pages under the planning tree, verified ' +
            'from disk)}. Include any branch-level or language-wide `.planning` tier under a target as its own unit with ' +
            'context_root the same dir. EXCLUDE folders with no `.planning` (host-bound source packages such as ' +
            'Rasm.Rhino/Rasm.Grasshopper). A mis-scoped or renamed target with no resolvable folder is reported in `unresolved`. ' +
            'Verify every unit against disk — the planning tree exists and holds at least one design page, the context_root holds ' +
            'the governing docs. The unit list is scope resolution, an initial pointer: downstream agents full-read every page ' +
            'under each planning tree themselves. Do not cd; do not edit anything.',
    ].join('\n\n');

const framePrompt = (L, u) =>
    [
        CONTEXT(L),
        STANCE('codex'),
        TERRITORY(L),
        FRAME_LAW,
        DIVIDERS(L),
        INFO_LAW,
        SELF_CHECK,
        ANTI_ANCHOR,
        READ_FIRST(L, u),
        'TASK: READ-ONLY FRAME RECON over EVERY design page under `' +
            u.planning +
            '` plus the owning-package `' +
            u.context_root +
            '/README.md` and `' +
            u.context_root +
            '/ARCHITECTURE.md` (investigate, do NOT edit; read-only is the only concession). Map the FRAME defects as findings, ' +
            'each graded and anchored: (imported-frame) a page wearing another ' +
            'corpus`s styling or shape; (template-drift) a README/ARCHITECTURE/spec page whose structure diverges from its docgen ' +
            'template, its heading census wrong; (text-seam) a cross-page/package seam drawn as ASCII art, a text tree, or a prose ' +
            'adjacency list where one mermaid diagram belongs; (dead-prose) narration, restatement, no-op or superseded prose ' +
            'carrying no load; (comment-discipline) a fence comment restating code, narrating, or carrying process framing, or a ' +
            'stack past the width/runt law; (section-grammar) a malformed/invented/mis-converted `' +
            L.marker +
            ' --- [SECTION]` divider or a drift label; plus the docgen defect classes STALE_MIRROR, TWIN_TRUTH, META_FRAME, ' +
            'ENUMERATION_ANCHOR, REPORT_FRAME, HEDGE, and COUPLING read at their catalog source. For each finding set `owner` to ' +
            'the file kind / template / register clause whose application resolves it, `reject` to the forms the repair must not ' +
            'take, `acceptance` to the signals proving the frame is gone. Findings NAME the defect and the violated law, NEVER the ' +
            'resulting prose — the writer rules every rebuild. Return per-page `findings` + `coverage`.',
    ].join('\n\n');

const writerPrompt = (L, u, framed, unmapped) =>
    [
        CONTEXT(L),
        STANCE('claude'),
        OWN_PASS(ownPassArt(u.folder, 'reframe')),
        TERRITORY(L),
        FRAME_LAW,
        DOCGEN_LAW,
        DIVIDERS(L),
        COMMENT_LAW(L),
        SEAM_MERMAID,
        RIPPLE_LAW,
        CURRENT_STATE,
        READ_FIRST(L, u),
        reconBlock(framed, unmapped),
        'TASK: HOSTILE PROSE + STRUCTURE REBUILD of the folder `' +
            u.folder +
            '` IN PLACE — every design page under `' +
            u.planning +
            '` plus the owning-package README.md + ARCHITECTURE.md. Destroy every imported frame and rebuild each page ' +
            'TEMPLATE-TRUE to its own file kind: README.md and ARCHITECTURE.md PREDATE the templates — re-frame them GROUND-UP per ' +
            'the docgen `references/rewriting.md` procedure, heading census matching the template, never inheriting the source ' +
            'frame; convert every text seam map to ONE clean mermaid diagram carrying every edge; re-frame poisoned pages to the ' +
            'docgen register; cull dead/no-op/superseded prose; repair comment discipline inside every fence and correct every ' +
            'malformed divider — WITHOUT touching any fence`s code semantics. Conserve ALL load-bearing content: intent, ' +
            'invariant, boundary, rationale, seam, cross-reference, RESEARCH marker — every one survives, refined. When you ' +
            'rebuild the README dependency section, make it AGREE with the central manifest and both `.api` catalog tiers, ' +
            'reporting any manifest drift you prove as an `indexRows` row. After editing, run the docgen prose gate — ' +
            '`uv run .claude/skills/docgen/scripts/prose_gate.py <every touched .md>` — and repair to ZERO FAILs before ' +
            'returning. Return the fix-log — `deltas` carries every re-framed page/section/seam as data, `deferred` the backlog ' +
            'rows, `indexRows` the single-writer decision rows, `seamsTouched` the both-ends ripples. ' +
            HARVEST_LAW,
    ].join('\n\n');

const critiquePrompt = (L, u, framed, unmapped, nav) =>
    [
        CONTEXT(L),
        STANCE('codex'),
        OWN_PASS(ownPassArt(u.folder, 'crit')),
        TERRITORY(L),
        FRAME_LAW,
        DOCGEN_LAW,
        DIVIDERS(L),
        COMMENT_LAW(L),
        SEAM_MERMAID,
        RIPPLE_LAW,
        CURRENT_STATE,
        READ_FIRST(L, u),
        reconBlock(framed, unmapped),
        'NAVIGATION (facts from the pass that re-framed these pages — locations only, no assessments; it changes where you look ' +
            'FIRST, never what you conclude): ' +
            JSON.stringify(nav),
        GIT_GROUND,
        'TASK: PREDICATE-POSITIVE CONFORMANCE AUDIT; fix EACH page in place across `' +
            u.planning +
            '` plus the README.md + ARCHITECTURE.md. Your own-pass artifact (OWN PASS FIRST above) precedes NAVIGATION — derive ' +
            'your findings there before consulting it. Verify each required law HOLDS and cite the clause; every miss is ' +
            'repaired NOW, a fix never a ledger note; a cross-file hit is yours per RIPPLE LAW:\n' +
            '- TEMPLATE CONFORMANCE: every README/ARCHITECTURE/spec/api-catalog/ideas/tasklog page matches its docgen template — ' +
            'heading census, section spine, marker system; a residual imported frame or a heading the template does not carry is ' +
            'a fail.\n' +
            '- REGISTER: OWNING_SUBJECT, DECISION_PER_LINE, NAMED_SURFACE, CAPABILITY, PROHIBITION_BY_STRUCTURE held on every ' +
            'line; a paraphrased nameable surface, a hedge, a mechanism-leak, or a weak-verb clause is repaired.\n' +
            '- FACT_LAW: every fact is LAW, REPRESENTATION, or REGISTRY; a STALE_MIRROR restating regenerable structure is ' +
            'deleted or demoted to a fence; a TWIN_TRUTH recited from its owner collapses to a pointer.\n' +
            '- SEAM REPRESENTATIONS: every seam map is a mermaid fence, none restated in prose; every edge preserved.\n' +
            '- DIVIDERS + FENCE COMMENTS: canonical grammar, agent-facing signal only, no ' +
            L.docBloat +
            ' bloat; NO fence code altered.\n' +
            '- PROSE GATE: run `uv run .claude/skills/docgen/scripts/prose_gate.py <every touched .md>` and repair to zero FAILs.\n' +
            'Return the fix-log — `deltas`, `deferred`, `indexRows`, `seamsTouched` exact. ' +
            HARVEST_LAW,
    ].join('\n\n');

const redteamPrompt = (L, u, framed, unmapped, nav, crit, critReport) =>
    [
        CONTEXT(L),
        STANCE('claude'),
        OWN_PASS(ownPassArt(u.folder, 'rt')),
        TERRITORY(L),
        FRAME_LAW,
        DOCGEN_LAW,
        DIVIDERS(L),
        COMMENT_LAW(L),
        SEAM_MERMAID,
        RIPPLE_LAW,
        CURRENT_STATE,
        READ_FIRST(L, u),
        reconBlock(framed, unmapped),
        'NAVIGATION (locations only, no assessments): ' + JSON.stringify(nav),
        'PRIOR CLAIMS (UNVERIFIED): the sol critique fixlog ' +
            (crit && crit.ok
                ? 'is ON DISK at ' + crit.report
                : 'wrapper died, but the lane writes its fixlog before any ceiling can kill the call — check ' +
                  critReport +
                  ' FIRST; absent or unparseable, your cold attack is the only review this unit gets: judge from CURRENT disk alone. Present') +
            ' — read it IN FULL from disk; its edits and verdicts are refutation targets you judge against CURRENT disk, never ' +
            'a settled record. FOLD-FORWARD DUTY: its surviving `seamsTouched`, `deltas`, `deferred`, and `indexRows` rows fold ' +
            'into YOUR return (re-verified against current disk, deduped) — your fix-log is the unit`s consolidated record. Its ' +
            '`harvest` rows are NOT yours to fold: the doctrine lander sweeps every critique fixlog from disk directly — ' +
            'nomination transport never rides a living fold.',
        GIT_GROUND,
        'TASK: ADVERSARIAL PREDICATE-NEGATIVE RED-TEAM; fix EACH page in place across `' +
            u.planning +
            '` plus the README.md + ARCHITECTURE.md — the LAST and MOST AGGRESSIVE pass. Assume the writer and critique missed ' +
            'things and their claims above are wrong until disk proves them. Your own-pass attack artifact (OWN PASS FIRST above) ' +
            'precedes the claims. Hunt RESIDUAL ANCHORING, the pre-mortem:\n' +
            '(A) COUNTERFACTUAL — a counterfactual REBUILDS the page`s framing with its central assumption removed, never merely ' +
            'questions it: read each page as the COLD REBUILD AGENT that must work from it alone, name the framing assumption it ' +
            'stands on (the inherited corpus shape, the frozen member roster, the report/process frame, the prose restating a ' +
            'diagram), derive the form the page takes WITHOUT it, and where that form is cleaner BUILD IT IN PLACE denser NOW — ' +
            'a fundamentally cleaner framing once seen is never defended against, and "the current frame also reads fine" is not ' +
            'a refutation. Any page still carrying the old frame, another corpus`s styling, a stale mirror, a twin truth, a ' +
            'frozen roster, or a report/process frame would inherit POISON to that cold rebuild and is re-framed.\n' +
            '(B) LONG-TAIL — attack where prior passes fade: deep sections, long pages, bottom-half fences, table cells, the last ' +
            'pages of the tree; the subtle surviving hedge, the still-bloated line, the stale or contradicted fact, the comment ' +
            'that does not earn its place.\n' +
            '(C) BOUNDARY — a fact owned elsewhere copied here (COUPLING/TWIN_TRUTH) collapses to a pointer at the owner; a seam ' +
            'representation split across prose and a diagram unifies to the diagram; both ends of every cross-reference verified.\n' +
            '(D) GENERATIVE — a re-framing this unit`s work opens in a sibling folder is realized now under RIPPLE LAW, or landed ' +
            'as a fully-specified `indexRows` row.\n' +
            'Then a FULL COLD RE-REVIEW of every conformance dimension by name — template census, register, FACT_LAW, seam ' +
            'representations, dividers, fence-comment discipline, prose gate to zero FAILs — each judged against CURRENT disk, no ' +
            'fence code touched anywhere. The folder must end objectively higher-signal, template-true, and free of every ' +
            'inheritable frame. Return the consolidated fix-log — `deltas`, `deferred`, `indexRows`, `seamsTouched`, `harvest` ' +
            'exact. ' +
            HARVEST_LAW,
    ].join('\n\n');

const fixerPrompt = (langs, rows, backlog, orphans, folders, round) =>
    [
        'WORKING ROOT: ' +
            ROOT_DIR +
            ' — every relative repo path in this brief resolves against this absolute root; read, write, and edit ONLY under it, ' +
            'never another checkout of the repository.',
        round
            ? 'DRAIN ROUND ' +
              round +
              ' — the backlog rows below were verified STILL-OPEN by the prior round; fix each at its root NOW, and a row you ' +
              'genuinely cannot land carries its named blocker and owner in `remaining`. Every other tranche re-arrives in full ' +
              'so a dead or partial prior round loses nothing — the checkpoint ledger is the consumption truth: skip every ' +
              'tranche it receipts, drain the rest.'
            : '',
        'Rasm monorepo — the libs/ planning corpora, PROSE + STRUCTURE only. This run`s doctrine is docgen: load the `docgen` and ' +
            '`skill-writer` skills via the Skill tool BEFORE any edit; load `mermaid-diagramming` before any diagram. Read ' +
            '`docs/standards/` (all four pages), the docgen templates, and `docs/laws/` at source. Per-language file-kind ' +
            'templates + casing:\n' +
            langs.map((k) => '- ' + LANG[k].name + ': casing ' + LANG[k].casing + ', divider marker `' + LANG[k].marker + '`.').join('\n'),
        GIT_GROUND,
        'CHECKPOINT LEDGER: `' +
            SCRATCH +
            '/fixer-checkpoint.md` — read it FIRST and skip every tranche it already receipts (an interrupted drain re-enters, ' +
            'never restarts); append one line per tranche AS EACH COMPLETES (each index-row apply, the backlog block, each ' +
            'critique fixlog, the own hunt). HARVEST FILE: append each `harvest` nomination to `' +
            SCRATCH +
            '/fixer-harvest.jsonl` (one JSON row per line) the moment it is minted — the doctrine lander sweeps the file, so a ' +
            'killed round loses no nomination; your returned `harvest` carries the same rows. Your returned `tranches` lists the ' +
            'checkpoint receipt line of every tranche fed this round — a fed tranche absent from that list is unconsumed, and the ' +
            'round has failed its mandate.',
        'TASK: TERMINAL DRAIN (WRITER — you are the run`s LAST agent, nothing follows you; full write authority over the ' +
            're-framed corpus and libs-wide ripple authority with the expand-form bound LIFTED — collapse, rename, and structural ' +
            'contract are yours now that no sibling writer runs; and you are the run`s SOLE writer for the owning-package index ' +
            'docs, IDEAS.md, and the central manifests). Re-framed folders: ' +
            JSON.stringify(folders) +
            '.\nTRANCHE ORDER IS EXECUTION ORDER — apply the tranches below in the order listed.\n' +
            [
                rows.length
                    ? '(1) INDEX ROWS: apply every reported row to its owning doc exactly once — dedupe semantically identical ' +
                      'rows, keep each doc`s section grammar and docgen template shape; a central-manifest row hand-edits the ' +
                      'grouped manifest at the SYMBOL anchor (never a line number) preserving label-group order; an IDEAS row ' +
                      'lands as a fully-specified card in the named IDEAS.md: ' +
                      JSON.stringify(rows) +
                      '.'
                    : '',
                backlog.length
                    ? '(2) DEFERRED BACKLOG (cross-unit and second-order prose/seam ripples the writers recorded — drain it: ' +
                      're-verify each {files, claim} on current disk, fix what holds, reject what disk already resolved): ' +
                      JSON.stringify(backlog) +
                      '.'
                    : '',
                orphans.length
                    ? '(2b) CRITIQUE FIXLOGS — every unit critique, folded-forward or orphaned (a live red-team folds ' +
                      'judgment-lossy and a dead one folds nothing); the paths are deterministic, so one absent on disk is ' +
                      'skipped with a one-line note in `summary`, never an error — read each present file IN FULL and drain the ' +
                      'deferred/index/seam rows still open under the same law (a row a red-team already landed disk-resolves and ' +
                      'drops); the fixlog `harvest` arrays are the doctrine lander`s to sweep, never yours to fold: ' +
                      JSON.stringify(orphans) +
                      '.'
                    : '',
                '(3) OWN HUNT: hunt PAST the row list on your own authority — imported frames, template drift, stale mirrors, ' +
                    'twin truths, text seam maps, dead prose, comment-discipline and divider defects over the re-framed pages ' +
                    'and the governance surface as you work them — and fix what the writers missed; `beyond` enumerates those ' +
                    'fixes, an empty `beyond` attests your hunt found nothing, never that it did not run. Every ripple an edit ' +
                    'exposes is YOURS in the same pass — cross-reference far ends both sides, index docs, manifest rows; ' +
                    'wire-canonical names stay frozen; a foreign-language repair holds that file kind`s template bar.',
                '(4) GATE: run `uv run .claude/skills/docgen/scripts/prose_gate.py <every touched .md>` and repair to zero FAILs ' +
                    'before returning.',
            ]
                .filter(Boolean)
                .join('\n') +
            '\nReturn the final fixlog — `remaining` carries ONLY rows verified still-open on current disk and genuinely ' +
            'blocked, each claim naming its blocker and owner; a row disk already resolved is culled with proof in `rejected`, ' +
            'and an empty `remaining` attests the drain closed. ' +
            HARVEST_LAW,
    ]
        .filter(Boolean)
        .join('\n\n');

const doctrinePrompt = (rows, orphans) =>
    'WORKING ROOT: ' +
    ROOT_DIR +
    ' — every relative repo path resolves against this absolute root; read, write, and edit ONLY under it.\n\n' +
    'TASK: DOCTRINE LANDER — the durable-learning terminal of a hostile reframe run. Read `docs/laws/README.md` ' +
    'FIRST — it owns the corpus admission and page-shape law; obey it over any restatement. Load the `docgen` skill AND the ' +
    '`skill-writer` skill via the Skill tool BEFORE any durable edit; load `mermaid-diagramming` before touching any ' +
    "diagram. NOMINATIONS (unverified, biased toward their authors' own work — refute by default): " +
    JSON.stringify(rows) +
    '\nAlso sweep `' +
    SCRATCH +
    '/fixer-harvest.jsonl` (absent = none): rows there missing from NOMINATIONS are nominations too — a killed fixer round ' +
    'reaches you only through that file.\n' +
    'Also read the `harvest` array of every critique fixlog at these deterministic paths (an absent or invalid file skips; no ' +
    'other agent transports these rows): ' +
    JSON.stringify(orphans) +
    ' — dedupe them against NOMINATIONS and adjudicate them identically.\n' +
    'ADJUDICATE each row per the admission bar: cold-read its target surface IN FULL, verify its anchors on ' +
    'CURRENT disk; LAND NOTHING is a first-class verdict.\n' +
    'TOPOLOGY RE-PROOF: re-verify every `docs/laws/topology.md` row whose [SURFACE] this run touched — cull a row ' +
    'whose coupling no longer holds, land a coupling this run proved.\n' +
    'GATE: run `uv run .claude/skills/docgen/scripts/prose_gate.py <every touched .md>` and repair to zero FAILs ' +
    'before returning. Return landed/refined/rejected (each rejection with its reason)/files/summary.';

// --- [COMPOSITION] ---------------------------------------------------------------------

const processUnit = async (u) => {
    const L = Lof(u.folder);
    const tag = fileTag(u.folder.split('/').pop());
    // (a) frame recon: one read-only gpt-5.6-terra codex lane maps the frame defects; product to disk, receipt on the wire.
    const frame = await slot(() =>
        recon(framePrompt(L, u), {
            label: 'frame:' + tag,
            phase: 'Reframe',
            schema: FRAME_SCHEMA,
            scope: [u.folder],
            hl: { arr: 'findings', group: 'class' },
        }),
    );
    const framed = frame && frame.ok ? frame : null;
    const unmapped = framed ? [] : [{ lane: 'frame:' + tag, scope: u.folder }];
    if (!framed) log(tag + ' — frame recon did not land; writer cold-reads the frame');
    // (b) reframe writer: fable authors the ground-up template-true rebuild in place; a dead critical writer earns bounded re-dispatch.
    const wopt = (label) => ({ label, phase: 'Reframe', model: 'fable', effort: 'high', schema: REFRAME_LOG, stallMs: STALL });
    const fix =
        (await slot(() => agent(writerPrompt(L, u, framed, unmapped), wopt('reframe:' + tag)))) ||
        (await retryLane(() => slot(() => agent(writerPrompt(L, u, framed, unmapped), wopt('reframe:' + tag + ':r1')))));
    // CHAIN CONTINUATION: a dead writer never blocks the reviews — the critique's conformance audit and the red-team's pre-mortem
    // still improve the pages as they stand on disk; navigation simply arrives empty.
    const nav = navOf(fix ? [fix] : []);
    // (c) sol critique: a workspace-write codex lane running the predicate-positive conformance audit in place; fixlog to disk,
    // receipt on the wire. The report path is DETERMINISTIC, so a dead receipt never severs the fold — the lane writes its fixlog
    // before the wrapper ceiling can kill the call, and the red-team + terminal drain verify the path on disk instead of trusting ok.
    const critReport = SCRATCH + '/' + fileTag('crit:' + tag) + '-report.json';
    const crit = await slot(() =>
        recon(critiquePrompt(L, u, framed, unmapped, nav), {
            label: 'crit:' + tag,
            phase: 'Reframe',
            schema: REVIEW_LOG,
            writes: true,
            fix: true,
            model: 'gpt-5.6-sol',
            nativeModel: 'fable',
            stallMs: CODEX_STALL,
            scope: [u.folder],
            hl: { arr: 'files' },
        }),
    );
    const critR = crit && crit.ok ? crit : null;
    // (d) fable red-team: predicate-negative, folds the critique fixlog`s operational rows forward; terminal stage of the unit chain.
    const ropt = (label) => ({ label, phase: 'Reframe', model: 'fable', effort: 'high', schema: REVIEW_LOG, stallMs: STALL });
    const rt =
        (await slot(() => agent(redteamPrompt(L, u, framed, unmapped, nav, critR, critReport), ropt('rt:' + tag)))) ||
        (await retryLane(() => slot(() => agent(redteamPrompt(L, u, framed, unmapped, nav, critR, critReport), ropt('rt:' + tag + ':r1')))));
    return { folder: u.folder, pages: u.pages || 0, fix, crit: critR, critReport, rt };
};

if (REJECTED.length) log('Rejected targets outside libs/{csharp,python,typescript}: ' + REJECTED.join(', '));
if (!TARGETS.length) {
    log('No targets — pass a target folder, an array of folders, or {targets}. Empty args is a no-op.');
    return { targets: [], total: 0 };
}

phase('Plan');
const plan = await slot(() =>
    agent(planPrompt(), { label: 'plan', phase: 'Plan', model: 'sonnet', effort: 'low', schema: DISCOVER_SCHEMA, stallMs: STALL }),
);
// Guard the planner-emitted roster: a unit dispatches only when its folder resolves to a language route under libs/, never on a stray path.
const UNITS = ((plan && plan.units) || []).filter((u) => u && u.folder && u.planning && langOf(normTarget(u.folder)));
const UNRESOLVED = (plan && plan.unresolved) || [];
if (UNRESOLVED.length) log('Unresolved targets (mis-scoped or renamed): ' + UNRESOLVED.join(', '));
log(
    'Plan: ' +
        UNITS.length +
        ' reframe unit(s) across ' +
        [...new Set(UNITS.map((u) => langOf(u.folder)).filter(Boolean))].join('+') +
        '; CAP=' +
        CAP,
);
if (!UNITS.length) {
    log('No folders resolved under the targets');
    return { targets: TARGETS, total: 0 };
}

phase('Reframe');
// Every unit chain launches freely via Promise.all; the agent-level slot scheduler is the only governor.
const done = (await Promise.all(UNITS.map((u) => processUnit(u).catch(() => null)))).filter(Boolean);
const LANDED = done.filter((d) => d.fix).map((d) => d.folder);
const FAILED = done.filter((d) => !d.fix).map((d) => d.folder);
// The critique fixlog lives on disk; the red-team folds its OPERATIONAL rows forward, so aggregation reads fix + rt only.
const ROWS = done.flatMap((d) => ((d.fix && d.fix.indexRows) || []).concat((d.rt && d.rt.indexRows) || []));
const SEAM_ROWS = done.flatMap((d) => ((d.fix && d.fix.seamsTouched) || []).concat((d.rt && d.rt.seamsTouched) || []));
const BACKLOG = done.flatMap((d) => ((d.fix && d.fix.deferred) || []).concat((d.rt && d.rt.deferred) || []));
// EVERY critique fixlog reaches the terminal drain (operational rows) AND the doctrine lander (harvest arrays) — keyed on the
// DETERMINISTIC path, never the receipt: nomination transport never rides a living fold, so the lander reads the disk artifact
// itself; a dead wrapper does not erase a written fixlog, and operational rows already landed disk-resolve and drop in the sweep.
const ORPHANS = done.filter((d) => d.critReport).map((d) => d.critReport);
// Writer and red-team are native lanes whose harvest rides their own wire return; the critique's harvest is the lander's disk sweep.
const HARVEST_ROWS = done.flatMap((d) => ((d.fix && d.fix.harvest) || []).concat((d.rt && d.rt.harvest) || []));
log(
    'Reframe: ' +
        LANDED.length +
        '/' +
        UNITS.length +
        ' folder(s) re-framed; ' +
        SEAM_ROWS.length +
        ' seam row(s), ' +
        BACKLOG.length +
        ' deferred backlog row(s), ' +
        ROWS.length +
        ' index row(s)' +
        (FAILED.length ? ' — FAILED (reported, run continues): ' + FAILED.join(', ') : ''),
);
if (!LANDED.length) {
    log('Nothing re-framed — no close to run');
    return { targets: TARGETS, units: UNITS.length, landed: 0, failed: FAILED };
}

phase('Close');
const LANDED_LANGS = [...new Set(LANDED.map((f) => langOf(f)).filter(Boolean))];
// Terminal DRAIN LOOP: one serial fable closer per round takes the residual set, verifies every row against live disk (freshness is
// its duty — no concurrent writers, no collisions), fixes at root, and loops until empty; a round without shrinkage stops with the
// blocked set final. Every round re-receives the FULL tranche set (index rows, orphan fixlogs, backlog): the checkpoint ledger is the
// consumption truth, so a dead or partial round loses nothing and a live one skips what it already receipted — only the backlog narrows.
let fixer = null;
let fixerHarvest = [];
let residuals = BACKLOG;
let lastOpen = Infinity;
for (let round = 0; round < DRAIN_ROUNDS; round++) {
    const fire = (suffix) =>
        slot(() =>
            agent(fixerPrompt(LANDED_LANGS, ROWS, residuals, ORPHANS, LANDED, round), {
                label: (round ? 'fixer:r' + round : 'fixer') + suffix,
                phase: 'Close',
                model: 'fable',
                effort: 'high',
                schema: FIXER_SCHEMA,
                stallMs: STALL,
            }),
        );
    fixer = (await fire('')) || (await retryLane(() => fire(':a1'))); // dead critical terminal earns bounded re-dispatch
    if (!fixer) break; // dead round after retries: the residual set survives to the run return, and every disk tranche stays checkpoint-re-enterable
    fixerHarvest = fixerHarvest.concat(fixer.harvest || []);
    const open = fixer.remaining || [];
    residuals = open;
    if (!open.length || open.length >= lastOpen) break;
    lastOpen = open.length;
}
const POOLED_HARVEST = HARVEST_ROWS.concat(fixerHarvest);
// DOCTRINE LANDER: the run's durable-learning terminal — wire nominations plus the critique fixlog harvest arrays and the fixer
// harvest jsonl, adjudicated against the live doctrine surfaces; refutation-first, land-nothing legal, admission law owned by
// docs/laws. A dead fixer still fires it (its nominations survive in the jsonl), and critique fixlogs on disk fire it too — the
// lander is those arrays' ONLY transport.
const doctrine =
    HARVEST_ROWS.length || ORPHANS.length || !fixer
        ? await slot(() =>
              agent(doctrinePrompt(POOLED_HARVEST, ORPHANS), {
                  label: 'doctrine',
                  phase: 'Close',
                  model: 'fable',
                  effort: 'high',
                  schema: DOCTRINE_SCHEMA,
                  stallMs: STALL,
              }),
          )
        : null;

return {
    targets: TARGETS,
    languages: LANDED_LANGS,
    units: UNITS.length,
    landed: LANDED.length,
    failed: FAILED,
    seamRows: SEAM_ROWS.length,
    backlog: BACKLOG.length,
    indexRows: ROWS.length,
    residuals,
    fixer: fixer && {
        files: (fixer.files || []).length,
        indexApplied: (fixer.indexApplied || []).length,
        resolved: (fixer.resolved || []).length,
        backlogDrained: (fixer.backlogDrained || []).length,
        beyond: (fixer.beyond || []).length,
        rejected: (fixer.rejected || []).length,
        summary: fixer.summary,
    },
    doctrine: doctrine && {
        nominated: POOLED_HARVEST.length,
        landed: (doctrine.landed || []).length,
        refined: (doctrine.refined || []).length,
        rejected: (doctrine.rejected || []).length,
        files: doctrine.files || [],
        summary: doctrine.summary,
    },
};
