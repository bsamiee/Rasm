export const meta = {
    name: 'finalize',
    whenToUse:
        'The finalization engine — the complement to rebuild: where rebuild improves and extends, finalize corrects and closes. Run it over a package (or folder subset) whose build passes have landed: deep per-file mapping of misalignments, broken or partial logic threads, split-brain, unnecessary differentiation, entry-point sprawl, and unleveraged libs-wide seams; one consolidated work dossier; one implementor with full project authority that fully drains; a read-only critique fan piped into one terminal red-team; a doctrine lander closing the loop.',
    description:
        'Language-agnostic finalization pass over one libs/{csharp,python,typescript} package planning corpus. args = a package root, an array of planning sub-folders, or {targets} — {root} retargets an isolated checkout, empty = no-op; the language derives from the root and selects the doctrine root pages, both .api tiers, the manifest, and the member-verification rail. Scope resolves targets to the owning package root and its per-sub-folder page units in dependency order (an oversize sub-folder splits into ceiling-bounded segments). Map fans two read-only lenses per unit — an interior FLOW lens (per-file logic threads end to end, partial or naive features, split-brain, differentiation, entry-point sprawl, dead ends) and an exterior SEAM lens (libs-wide boundary and wire alignment, unleveraged upstream capability, hand-rolled reimplementation, manifest and .api drift) — each writing a typed findings report; one MERGE lane then consolidates every report into a single deduped, thread-joined, owner-ordered work dossier, the one artifact the implementor consumes. Implement is ONE writer over the whole target with full project write authority: own blind pass first, then the dossier as grounding to verify and exceed; every finding and every ripple drains in the same pass, index docs and manifests are its own to apply, and the deferred backlog carries only rows a named blocker genuinely forbids. Review fans one read-only critique per unit over the implemented corpus — own verdict first, navigation facts second — piped by deterministic report path into ONE terminal red-team that drains every critique row, the implementor backlog, and its own hunt to a fixpoint under a checkpoint ledger. Close adjudicates pooled harvest nominations — wire rows, every critique report harvest array, and the red-team harvest file, all swept from disk — into docs/laws and the reviewer surfaces. Stage law lives in the prompt blocks; CODEX=false restores native lanes throughout.',
    phases: [
        {
            title: 'Scope',
            detail: 'targets resolve to the owning package root and its per-sub-folder page units, emitted in dependency order — foundations first, a launch bias only',
        },
        {
            title: 'Map',
            detail: 'two read-only lenses per unit segment — interior logic flow, exterior seam integration — each writing a typed findings report; one merge lane consolidates every report into the single deduped, thread-joined work dossier the implementor consumes',
        },
        {
            title: 'Implement',
            detail: 'ONE writer over the whole target with full project authority: own blind pass first, the work dossier as grounding to verify and exceed, every finding and ripple drained in the same pass, index and manifest rows applied directly',
        },
        {
            title: 'Review',
            detail: 'one read-only critique per unit over the implemented corpus — own verdict first, navigation second — piped by deterministic report path into ONE terminal red-team that drains every row to a fixpoint under a checkpoint ledger',
        },
        {
            title: 'Close',
            detail: 'a doctrine lander adjudicates pooled harvest nominations swept from wire rows, every critique report, and the red-team harvest file; landing nothing is a first-class verdict',
        },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAP = 14;
const STAGGER_MS = 1500;
const STALL = 480000;
const WRAPPER_STALL = 1500000; // stallMs never observes a live blocking MCP call — it guards only out-of-call wrapper wedges; the watchdog clocks below are the binding bound
const LANE_CLOCK = 2700000; // codex-lane wall-clock watchdog (~2.5x the observed medium-lane median): a nested-call wedge inside codex otherwise holds the slot to the session MCP ceiling
const CRIT_CLOCK = 3600000; // sol-high critique watchdog: full conformance over a finalized unit legitimately runs long; the ceiling exists for wedges, never depth
const UNIT_MAX = 8; // unit segmentation ceiling — map and critique lanes stay page-congruent on any folder size
const RETRY_BACKOFFS = [60000, 1800000]; // agent() returns null causeless, so the ladder covers both death classes: a fast first attempt catches transient transport deaths, the long second waits out a usage-limit window
const CODEX = true; // recon lanes ride the codex wrapper (sol-medium maps + merge, sol-high critiques); false restores native lanes

// --- [INPUTS] --------------------------------------------------------------------------

const normTarget = (t) => String(t).trim().replace(/\/+$/, '').replace(/^\/+/, '');
const langOf = (t) =>
    t.indexOf('libs/csharp') === 0 ? 'cs' : t.indexOf('libs/python') === 0 ? 'py' : t.indexOf('libs/typescript') === 0 ? 'ts' : null;
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
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(normTarget))].filter((t) => langOf(t));
const REJECTED = [...new Set(rawTargets.filter(Boolean).map(normTarget))].filter((t) => !langOf(t));
const LANG_KEYS = [...new Set(TARGETS.map(langOf))];
const LANG_KEY = LANG_KEYS.length === 1 && LANG_KEYS[0] ? LANG_KEYS[0] : null;
const ROOT_DIR =
    isObj && typeof argsIn.root === 'string' && argsIn.root.trim()
        ? argsIn.root.trim().replace(/\/+$/, '')
        : '/Users/bardiasamiee/Documents/99.Github/Rasm';
const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH =
    '.claude/scratch/' +
    ('finalize-' + TARGETS.map((t) => t.split('/').pop().toLowerCase()).join('-')).replace(/[^a-z0-9.-]+/g, '-').slice(0, 60) +
    '-' +
    fnv1a(JSON.stringify(TARGETS));

// --- [MODELS] --------------------------------------------------------------------------

const S = { type: 'string' };

const PLAN_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['root', 'folders'],
    properties: {
        root: S,
        folders: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['folder', 'pages'],
                properties: { folder: S, pages: { type: 'array', items: S } },
            },
        },
    },
}; // ARRAY ORDER IS DEPENDENCY ORDER — launch bias only, nothing serializes on it

const ANCHOR = {
    // One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: S,
        line: { type: 'integer' },
        role: { type: 'string', enum: ['defect', 'ruling', 'catalog', 'counterpart', 'absence'] },
        note: S,
    },
};

const COVERAGE = {
    type: 'object',
    additionalProperties: false,
    required: ['requested', 'read', 'skipped', 'unverified'],
    properties: {
        requested: { type: 'array', items: S },
        read: { type: 'array', items: S },
        skipped: { type: 'array', items: S },
        unverified: { type: 'array', items: S },
    },
};

const HARVEST = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['altitude', 'lang', 'claim', 'anchors', 'existingClause'],
        properties: {
            altitude: { type: 'string', enum: ['stacks', 'reviewer', 'constitution', 'planning', 'readme', 'laws'] },
            lang: S,
            claim: S,
            anchors: { type: 'array', items: S },
            existingClause: S,
        },
    },
}; // doctrine nominations — generalizable lessons only; the terminal doctrine lander adjudicates every row

const DEFERRED = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['files', 'claim'],
        properties: { files: { type: 'array', items: S }, claim: S },
    },
}; // named-blocker rows ONLY — the run fully drains; this is never a routing channel

const DELTAS = {
    // navigation facts: what moved, as data, zero adjectives
    type: 'array',
    items: { type: 'object', additionalProperties: false, required: ['symbol', 'change'], properties: { symbol: S, change: S } },
};

// Map and critique lanes share one findings product; `harvest` rides the report so the doctrine lander sweeps it from disk directly.
const FINDINGS_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['findings', 'harvest', 'coverage', 'summary'],
    properties: {
        findings: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: [
                    'claimKey',
                    'target',
                    'files',
                    'kind',
                    'grade',
                    'severity',
                    'claim',
                    'anchors',
                    'mechanism',
                    'owner',
                    'reject',
                    'acceptance',
                ],
                properties: {
                    // Schema-enforced key grammar — free-text keys fracture per lane and corroboration never collides into one key.
                    claimKey: { type: 'string', pattern: '^[a-z0-9_-]+(\\.[a-z0-9_-]+){3}$' },
                    target: S, // short display label for the defect
                    files: { type: 'array', items: S }, // files the consumer must open or edit first
                    kind: {
                        type: 'string',
                        enum: [
                            'underutilized',
                            'handrolled',
                            'phantom_forgotten',
                            'phantom_lie',
                            'splitbrain',
                            'differentiation',
                            'naive',
                            'partial',
                            'flow',
                            'seam',
                            'stale',
                        ],
                    },
                    grade: { type: 'string', enum: ['substantive', 'hypothetical'] }, // substantive = concrete on-disk defect; hypothetical = requires an invented implausible input
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: S, // the observed defect as fact
                    anchors: { type: 'array', items: ANCHOR },
                    mechanism: S, // WHY the current form fails the doctrine/domain — factual, zero repair verbs
                    owner: S, // canonical owner that must absorb the resolution
                    reject: { type: 'array', items: S }, // forms the repair must NOT take
                    acceptance: { type: 'array', items: S }, // signals proving resolution
                },
            },
        },
        harvest: HARVEST,
        coverage: COVERAGE,
        summary: S,
    },
};

// The merge lane authors ONE content artifact — the work dossier — and returns only this thin receipt: the per-unit jump index plus coverage.
const MERGE_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['index', 'coverage', 'summary'],
    properties: {
        index: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['unit', 'sections'],
                properties: { unit: S, sections: { type: 'array', items: S } }, // sections = the dossier headers carrying this unit's work items
            },
        },
        coverage: COVERAGE,
        summary: S,
    },
};

const RECEIPT = {
    // Thin wire receipt: the lane's PRODUCT stays on disk at `report`; only status + count + headline travel inline.
    // `thread` is the codex MCP threadId — the rollout-file key under ~/.codex/sessions/ AND the `codex exec resume` handle,
    // so a dead codex lane stays joinable and recoverable; native lanes return ''.
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'report', 'entries', 'headline', 'failure', 'thread'],
    properties: { ok: { type: 'boolean' }, report: S, entries: { type: 'integer' }, headline: S, failure: S, thread: S },
};

const IMPL_SCHEMA = {
    // Required-but-possibly-empty deferred/harvest are attestations: nothing blocked, nothing generalizable.
    type: 'object',
    additionalProperties: false,
    required: ['files', 'verdict', 'collapsed', 'realized', 'deltas', 'indexApplied', 'deferred', 'harvest', 'summary'],
    properties: {
        files: { type: 'array', items: S },
        verdict: { type: 'string', enum: ['fixed', 'clean'] },
        collapsed: S,
        realized: S,
        deltas: DELTAS,
        indexApplied: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['doc', 'action'], properties: { doc: S, action: S } },
        },
        deferred: DEFERRED, // named-blocker rows only — the red-team drains them; anything else is a failed drain
        harvest: HARVEST,
        summary: S,
    },
};

const RT_SCHEMA = {
    // `tranches` receipts every fed tranche; `beyond` attests the own hunt ran; `remaining` carries named-blocker rows only.
    type: 'object',
    additionalProperties: false,
    required: ['files', 'resolved', 'rejected', 'beyond', 'remaining', 'tranches', 'harvest', 'summary'],
    properties: {
        files: { type: 'array', items: S },
        resolved: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: { target: S, action: S } },
        },
        rejected: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['finding', 'reason'], properties: { finding: S, reason: S } },
        },
        beyond: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: { target: S, action: S } },
        },
        remaining: DEFERRED,
        tranches: { type: 'array', items: S }, // one checkpoint receipt line per tranche fed — a fed tranche absent here is unconsumed
        harvest: HARVEST,
        summary: S,
    },
};

const DOCTRINE_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['landed', 'refined', 'rejected', 'files', 'summary'],
    properties: {
        landed: { type: 'array', items: S },
        refined: { type: 'array', items: S },
        rejected: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['claim', 'reason'], properties: { claim: S, reason: S } },
        },
        files: { type: 'array', items: S },
        summary: S,
    },
};

// --- [DOCTRINE] ------------------------------------------------------------------------

const LANG = {
    // LANG carries routing data and engine-parameter rows ONLY — doctrine content is reached at source, never paraphrased here.
    cs: {
        name: 'C#',
        root: 'libs/csharp',
        shared: 'libs/csharp/.api',
        manifest: 'the package `.csproj` and the central `Directory.Packages.props` block for this package',
        verify:
            '`uv run python -m tools.assay api` (assay blocked or unavailable: the `.api` catalogs + the nuget MCP for feed ' +
            'truth — version/deprecation lookups only, `get_latest_package_version`-class calls, never a full ' +
            '`get_package_context` dump on a large package — + Context7/exa/tavily own the fallback)',
        stackLaw:
            'the docs/stacks/csharp ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full; the ' +
            'domain/ sub-folder enters only for the shards your pages touch, chosen from its router README',
    },
    py: {
        name: 'Python',
        root: 'libs/python',
        shared: 'libs/python/.api',
        manifest: 'the root `pyproject.toml` rows this package consumes',
        verify: '`uv run python -m tools.assay api resolve <pkg>` (blocked or gated: the `.api` catalogs + PyPI feed truth + Context7/exa/tavily own the fallback)',
        stackLaw:
            'the docs/stacks/python ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full in the ' +
            'README [01]-[ATLAS] order; the domain/ and numerics/ sub-folders enter only where a page concern touches them',
    },
    ts: {
        name: 'TypeScript',
        root: 'libs/typescript',
        shared: 'libs/typescript/.api',
        manifest: 'the `pnpm-workspace.yaml` / package manifest rows this area consumes',
        verify: 'the published types in node_modules (`uv run python -m tools.assay api` over node_modules declarations where a member is novel)',
        stackLaw: 'the docs/stacks/typescript ROOT pages ONLY — enumerate the root with a real ls and read EVERY root `.md` in full',
    },
};
const L = LANG[LANG_KEY] || LANG.cs; // inert fallback: composition no-ops before any agent call when LANG_KEY is null

// --- [SHARED_BLOCKS]

const ROOT_LAW =
    'WORKING ROOT: ' +
    ROOT_DIR +
    ' — every relative repo path in this brief resolves against this absolute root; read, write, and edit ONLY under it, never ' +
    'another checkout of the repository.';
const CONTEXT =
    ROOT_LAW +
    '\n\nRasm monorepo — ' +
    (LANG_KEY ? L.name : 'planning') +
    ' planning corpus (markdown design pages; code fences are the product). CLAUDE.md manifest law governs.';

const LAW =
    'FINALIZATION LAW — this pass corrects and CLOSES; the rebuild engine improves and extends. The targets: split-brain and ' +
    'dual paradigms unified onto ONE rail per the doctrine; unnecessary differentiation (parallel shapes, sibling name families, ' +
    'twin entries for one concept — within a page, across pages, or across folders) collapsed into the polymorphic owner IN ' +
    'PLACE, never a new file; duplication and indirection reduced; PHANTOM code adjudicated — a cited member/type/seam that does ' +
    'not exist is FORGOTTEN FUNCTIONALITY when intent evidence stands (seams, README rows, growth clauses, sibling contracts — ' +
    'realize it NOW at full depth, as if always there) or a LIE when none does (delete it); PARTIAL logic threads finished — a ' +
    'chain whose admission, owner, rail, projection, or egress end is thin, missing, or naive is extended to full capability at ' +
    'every end; entry points unified — one polymorphic entry per concern, sprawl collapsed; logic flow smoothed END-TO-END ' +
    '(admission once -> canonical owner -> unified rail -> projection -> egress; no dead ends, no rail breaches, no unreachable ' +
    'cases, no orphaned carriers); libs-wide seams LEVERAGED — an upstream capability the target hand-rolls or ignores is wired ' +
    'in, a stale seam counterpart is aligned both ends; density pushed and LOC reduced THROUGH density, never through capability ' +
    'loss. ALL capability is preserved — deletion exists only for proven lies and true duplicates, and underutilization is an ' +
    'integration defect this pass closes by wiring, never by removal.';

const INFO_LAW =
    'FINDING FORM — you deliver TRUTH, never an implementation: `claim` states the observed defect; `mechanism` states WHY it ' +
    'fails the doctrine or the domain as fact; `anchors` carry one coordinate per row (role names what it proves; `note` is the ' +
    'shortest literal witness under 20 words, or empty when path+line suffice; an `absence` anchor names where the expected ' +
    'thing was searched and not found); a `defect` anchor lands ON the line implementing the mechanism — a nearby declaration ' +
    'or a blank line taxes the consumer, whose re-open of every anchor is mandatory — and every anchor path resolves on current ' +
    'disk at emit: a phantom or doubled-segment path is a dead read the consumer inherits; `owner` names the canonical owner ' +
    'that must absorb the resolution (the owning axis, roster, registry, or seam vocabulary — never a new local shape); ' +
    '`reject` lists forms the repair must not take; `acceptance` the signals proving resolution. NEVER write ' +
    'add/replace/implement/promote/delete as instruction and never sketch a fence — the writer owns the design; you own the ' +
    'constraint boundary. `claimKey` follows one mechanical grammar — <package>.<page-stem>.<owner>.<defect-slug>, exactly four ' +
    'dot-separated lowercase segments (e.g. `appui.dashboards.lttb.unwired-decimator`) — so the same defect keys identically ' +
    'from any lane by construction. `severity` binds to consequence (blocker = closure-blocking, major = correctness, minor = ' +
    'local cleanup), never prose confidence — grade honestly across all three; a report whose every finding shares one grade ' +
    'has averaged, not graded. OUTPUT BOUNDS: retention scales with the scope — keep every finding that survives the second ' +
    'pass, lead with the strongest, and 0 is legal only when that pass returns empty, `summary` then naming the probes that ' +
    'produced nothing. COVERAGE is part of the product: `requested` = assigned scope, `read` = actually full-read, ' +
    '`skipped`/`unverified` = not reached or unconfirmed — an honest skip beats a silent one.';

const SELF_CHECK =
    'SELF-VERIFY (second pass, before returning): re-derive every entry from disk — re-open each cited anchor and confirm it ' +
    'states what the entry claims, re-verify each member spelling against its catalog, trace each seam to both endpoints. ' +
    'Correct or delete any entry that fails re-confirmation; never return a guess, an assumption, a skimmed summary, or a ' +
    'vague/hedged entry. Completeness is part of correctness: after the re-read, hunt once more for what the first pass missed ' +
    '— an omitted load-bearing fact is as wrong as a false one.';

// Register fork — substance identical (burden of proof on the work, both naivety axes, no-churn); phrasing per executing model:
// the hostile register measurably makes a codex lane over-read and probe out of territory, so codex rows stay de-conflicted.
const REG = {
    claude: {
        stance:
            'STANCE — the pass is hostile: the pages were authored by ANOTHER engineer and are under adversarial review; hold ' +
            'every fence naive, partial, or illusory until it survives a real attack — the burden of proof is on the code, never ' +
            'on you. Dense, confident, package-fluent code is the PRIME suspect for hollowness: a name promising capability the ' +
            'body omits, a logic thread finished at one end only, decorative density carrying nothing. NAIVETY is a defect on ' +
            'two orthogonal axes: COVERAGE — the owner models a thin slice of its concept; APPROACH — an enumerated roster where ' +
            'one parameterized generator should generate the space. NO CHURN: an edit requires a named violated law or invariant ' +
            'and the concrete case that breaks it; a clean verdict earned by an attack that finds nothing is a first-class ' +
            'result, proven by adding nothing.',
    },
    codex: {
        stance:
            'REVIEW POSTURE — the pages are unverified work by another engineer: verify every claim a fence makes against the ' +
            'real domain and the catalogued package surface before accepting it; a prior clean verdict or confident prose is not ' +
            'evidence. NAIVETY is a defect on two orthogonal axes: COVERAGE — the owner models a thin slice of its concept; ' +
            'APPROACH — an enumerated roster where one parameterized generator should generate the space. A name promising ' +
            'capability the body omits, a logic thread finished at one end only, and a hollow confident body are the primary ' +
            'targets. NO CHURN: a finding requires a named violated law or invariant and the concrete case that breaks it; a ' +
            'clean verdict from a check that finds nothing is a first-class result.',
    },
};

const LAWS_READ =
    'LAWS: read `docs/laws/` IN FULL (README + topology + patterns + scars; short registry pages) — a topology row whose ' +
    '[SURFACE] your corrections touch binds its obligated counterparts into the SAME pass, and every patterns row binds each ' +
    'branch it names.';

const BOUNDARY =
    'BOUNDARY LAW: cross-package and cross-language wire-canonical seam names are FROZEN — repair a seam recording, never ' +
    'rename the wire. Strata hold: no downward dependency, no host-type leak into a host-neutral owner. A foreign-language ' +
    "repair holds that branch's doctrine bar (read that branch stack README before a non-trivial foreign edit).";

const HARVEST_LAW =
    'HARVEST (required key, usually empty): nominate ONLY findings that generalize beyond this run — a collapse pattern ' +
    'reusable across folders, a naivety class no doctrine clause names, a review rule that catches the defect BEFORE review, a ' +
    'hard-won cross-surface coupling. Each row: altitude (stacks|reviewer|constitution|planning|readme|laws), lang, claim (the ' +
    'generalized law, one sentence, SYMBOL-FREE — every concrete spelling lives in anchors, so the lander adjudicates the law ' +
    'without re-deriving its locality), anchors (file:line evidence), existingClause (the exact clause it hardens, quoted with ' +
    'its path — or "absent" plus the surfaces searched). A run-local fix never nominates; an empty array is the normal verdict ' +
    '— the doctrine lander refutes weak rows, so nominate substance, never volume.';

const OWN_PASS = (artifact) =>
    'OWN PASS FIRST — the input ladder is binding, in order: (1) your own blind hostile pass, (2) the recon products. Rung (1) ' +
    'is the PRIMARY product and it is a DISK ARTIFACT, not a reading step: cold-read every target page from CURRENT disk and ' +
    'WRITE your own defect-and-work list to `' +
    artifact +
    '` — split-brain, differentiation, partial threads, naive owners, phantoms, flow breaks, entry-point sprawl, unleveraged ' +
    'seams — BEFORE opening any recon product. The recon may only ADD rows to that file, each tagged [recon]; reading the pages ' +
    'without writing the list is a failed rung, not a cold pass. TRIPWIRE: a diff dominated by [recon]-tagged rows has failed — ' +
    'the recon covers a MINORITY of what the work demands, and the majority of your edits must come from your own attack.';

const GIT_GROUND =
    'DELTA GROUNDING — run `git diff --stat` then `git diff -- <the pages and their seam files>` before judging; `git status` ' +
    'surfaces new files. The diff is orientation, CURRENT disk is truth — the repo can carry pre-run uncommitted work, so an ' +
    "unfamiliar hunk is verified against disk, never assumed to be this run's.";

// --- [OPERATIONS] ----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms));
// Agent-level slot scheduler: CAP agents in flight across the whole run, staggered launch, work-conserving backfill.
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
// Bounded re-dispatch for a dead CRITICAL lane: attempt-counted with a per-attempt backoff; the final death isolates the lane
// but NEVER the chain — every downstream stage still runs against current disk.
const retryLane = async (fn) => {
    for (const backoff of RETRY_BACKOFFS) {
        await sleep(backoff);
        const r = await fn();
        if (r) return r;
    }
    return null;
};

// Run telemetry: every lane brackets itself on ONE shared ledger — one O_APPEND line per event, `<utc-iso> | <label> | <event>[ | <verdict> | <count>]`.
// The ledger is the workflow-agnostic observability seam a watcher tails for phase/stall/failure signals; native lanes self-stamp through the `run`
// dispatch owner, codex lanes are stamped by their sonnet wrapper around the blocking MCP call so the bracket times the codex call itself.
const LEDGER_LOG = ROOT_DIR + '/' + SCRATCH + '/run-telemetry.log';
const TLM = (label) =>
    'TELEMETRY (mechanical): FIRST act — one Bash append of one line to `' +
    LEDGER_LOG +
    '`: `<utc-iso> | ' +
    label +
    ' | start` (shell `>>` with `date -u +%FT%TZ`; never rewrite the file). FINAL act before returning — append the matching ' +
    '`<utc-iso> | ' +
    label +
    ' | end | <one-word verdict> | <primary entry count>`. A lane that cannot finish appends `| fail | <reason slug>` instead of `end`.';
const run = (prompt, opts) => agent(prompt + '\n\n' + TLM(opts.label), opts);
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const reportOf = (label) => SCRATCH + '/' + fileTag(label) + '-report.json'; // deterministic — consumers survive a dead receipt by checking the path

const laneLaw = (schema, o) =>
    (o.fix
        ? '<completion_bar>\nDone is every item in your named scope worked to its full depth with its product entry written — proof-complete, ' +
          'never effort-spent, never early. Complete every named move before yielding; do not stop at analysis or a partial edit. If the ' +
          'chosen approach resists, pick the next-best one and proceed. Re-verifying unchanged work or re-reading covered territory adds no ' +
          'evidence; move to the next deliverable instead.\n</completion_bar>\n\n<verification>\nAfter editing, re-read each changed file and ' +
          'confirm it is coherent and nothing it carried was lost. Fix what fails before yielding; a check you did not run is never claimed ' +
          'as run.\n</verification>'
        : '<context_gathering>\nTerritory: the exact files and directories the task names. Do not open files outside it; ' +
          'instruction files (.claude/, CLAUDE.md, AGENTS.md) are always out of scope.\nBudget: at most ' +
          (o.calls || 60) +
          ' tool calls total. Read in small batches (a handful of files per command, line-capped); never concatenate the whole ' +
          'territory into one command - tool output truncates and the data is lost.\nStop as soon as the product is complete. ' +
          'If something is still uncertain at the budget, proceed and record the residue in coverage.unverified instead of ' +
          're-reading.\n</context_gathering>\n\n<verification>\nBefore the final message, confirm every cited spelling appears ' +
          'verbatim in the cited file; anything unconfirmed is recorded in coverage.unverified, never asserted.\n</verification>') +
    '\n\n<tool_bounds>\nA nested MCP tool call is bounded: prefer the lightest variant that answers the question (a version ' +
    'lookup over a full package-context dump), give every such call a hard time budget, and when a call does not settle ' +
    'promptly, record the item as a gap/unverified row and move on — an unbounded wait on one lookup never stalls the task.\n' +
    '</tool_bounds>\n\n<output_contract>\nYour final message is a single JSON object with exactly this shape: ' +
    JSON.stringify(schema) +
    '\n- JSON only: no prose before or after it, no code fences, no markdown.\n- Every key shown is required.\n' +
    '- Use null for a value you could not determine and [] for an empty list; never guess.\n</output_contract>';

const codexPrompt = (label, task, schema, o) => {
    const report = ROOT_DIR + '/' + reportOf(label);
    const model = o.model || 'gpt-5.6-sol';
    return [
        'DISPATCH ROLE: ' +
            model +
            ' performs the complete TASK below through one blocking Codex MCP call. Follow exactly four steps; ' +
            'never perform, edit, judge, soften, summarize, or relay the task yourself.',
        '(1) Load the `codex` skill via the Skill tool FIRST — its [09] sessions and recovery law governs this call. Then call ' +
            'ToolSearch with query "select:mcp__codex__codex,mcp__codex__codex-reply", and append one Bash line to `' +
            LEDGER_LOG +
            '`: `<utc-iso> | ' +
            label +
            ' | codex-start` (shell `>>` with `date -u +%FT%TZ`; never rewrite the file).',
        '(2) Call the loaded mcp__codex__codex tool ONCE with model="' +
            model +
            '", sandbox=' +
            (o.writes ? '"workspace-write"' : '"read-only"') +
            ', cwd=' +
            JSON.stringify(ROOT_DIR) +
            (o.codexEffort ? ', config={"model_reasoning_effort":"' + o.codexEffort + '"}' : '') +
            ', "developer-instructions" set to the LANE LAW block below VERBATIM, and prompt set to the TASK block below VERBATIM. ' +
            (o.writes
                ? "On any call error run the codex skill's blocking-caller recovery ladder with this lane's disk product at " +
                  report +
                  ' — verify it FIRST (the lane writes it as its final act; a valid report proceeds to step (4) as success); the ' +
                  'reply nudge tells the session to finish the TASK and write the report file as specified; a fresh identical call ' +
                  'is the last resort (a second writer over the same pages).'
                : "On any call error run the codex skill's blocking-caller recovery ladder — this lane writes no product itself, so " +
                  'the reply re-emission of the complete final-message JSON is the first rung and one identical retry the second; a ' +
                  'failed ladder skips step (3) and returns through step (4).'),
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
            : '(3) The tool result is a JSON envelope {threadId, content} whose content field holds the final-message text. Write that ' +
              'CONTENT text (the product JSON, unescaped) — never the envelope — with the Write tool to this absolute path: ' +
              report +
              '. Do not normalize, reformat, summarize, or extract the text before writing it. Then verify with one Bash call: jq -e . ' +
              report +
              ' >/dev/null — a Write that drops the tail mints invalid JSON; on failure rewrite once from the tool result, and a second ' +
              'failure returns through step (4) with the error.',
        '(4) One Bash append of one line to the same ledger: `<utc-iso> | ' +
            label +
            ' | codex-end | <ok or fail> | <entries> | <threadId from the result envelope>` — the threadId keys the codex-side ' +
            'session record, so it is never omitted. Then parse the tool result text only for mechanical orchestration data. Return ok=true, report=' +
            reportOf(label) +
            ', entries=the length of result["' +
            o.hl.arr +
            '"], headline="<entries> ' +
            o.hl.arr +
            (o.hl.group ? ' | <' + o.hl.group + ' tallies>' : '') +
            ' | top: <most frequent first file or none>", thread=the threadId from the result envelope, and failure empty. On a ' +
            'second tool error return ok=false, entries=0, report and headline empty, thread=the threadId if any envelope ' +
            'returned one else empty, and failure equal to the error text VERBATIM.',
    ].join('\n\n');
};

// QUOTA FALLBACK: a codex receipt whose failure matches usage/quota/limit re-dispatches the SAME task natively at the role's
// Claude twin (sol->opus, luna->sonnet, terra->opus); the caller owns the re-dispatch, the sonnet wrapper never executes work.
// The roster row carries `scope` from the ORCHESTRATOR so a lane that died before writing still names its territory exactly.
const twinOf = (m) => (/-luna/.test(m || '') ? 'sonnet' : 'opus'); // native fallback twins; fable's ONE seat is the terminal red-team, never a fallback
const nativeLane = (task, o) =>
    agent(
        task +
            '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
            (ROOT_DIR + '/' + reportOf(o.label)) +
            ' (Write tool, exactly this absolute path): ' +
            JSON.stringify(o.schema) +
            ' — then return ONLY the receipt: ok, report = ' +
            reportOf(o.label) +
            ' (this repo-relative form, matching codex-lane receipts), entries count, one-line mechanical headline, failure ' +
            'empty, thread empty.',
        { label: o.label, phase: o.phase, model: o.nativeModel || twinOf(o.model), effort: 'high', schema: RECEIPT, stallMs: o.stallMs || STALL },
    );
const recon = (taskOf, o) => {
    const task = typeof taskOf === 'function' ? taskOf : () => taskOf;
    const wrapper = { label: 'sol:' + o.label, phase: o.phase, model: 'sonnet', effort: 'low', schema: RECEIPT, stallMs: o.stallMs || WRAPPER_STALL };
    // WATCHDOG: the race frees the slot and hands the chain the standard dead-lane shape at the wall-clock ceiling; the abandoned
    // call keeps running harness-side as an ignored zombie (a late report in scratch is harmless), and the codex session stays
    // recoverable through the rollout store. Cancellation does not exist on this surface — slot recovery is the whole point.
    return (
        CODEX
            ? Promise.race([
                  agent(codexPrompt(o.label, task('codex'), o.schema, o), wrapper),
                  sleep(o.clockMs || LANE_CLOCK).then(() => ({
                      ok: false,
                      report: '',
                      entries: 0,
                      headline: '',
                      failure: 'watchdog: wall-clock ceiling — call abandoned, slot freed; session recoverable via the rollout store',
                  })),
              ]).then((r) => (r && !r.ok && /usage|quota|limit/i.test(r.failure || '') ? nativeLane(task('claude'), o) : r))
            : nativeLane(task('claude'), o)
    ).then((r) => ({
        lane: o.label,
        scope: o.scope || [],
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'lane died'),
        thread: (r && r.thread) || '',
    }));
};
const chunk = (arr, n) => {
    const o = [];
    for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n));
    return o;
};
// Even split: ceil(n/max) segments of near-equal size — no runt tail heavying segment 0 and starving the last.
const evenChunk = (arr, max) => chunk(arr, Math.ceil(arr.length / (Math.ceil(arr.length / max) || 1)));

// Prompt builders — each task states only its own action; shared laws are composed by name.
const planPrompt = () =>
    [
        CONTEXT,
        'TASK: thin enumerate (read-only, do NOT edit). TARGETS (repo-relative): ' +
            JSON.stringify(TARGETS) +
            '. Each target is a package root (e.g. ' +
            L.root +
            '/<Package>) or a planning sub-folder (<root>/.planning/<Folder>). Resolve the ONE owning package root. Return root ' +
            '(the package root) and folders — one entry per planning sub-folder in scope (a package-root target expands to EVERY ' +
            'sub-folder under its .planning tree, root-level pages pooling as one `_root` entry; explicit sub-folder targets ' +
            'restrict to exactly those): {folder, pages: repo-relative *.md paths under it}. EXCLUDE ' +
            'IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md and any _*.md campaign document. EMIT `folders` IN DEPENDENCY ORDER — ' +
            'foundations before their consumers, judged from the package README.md/ARCHITECTURE.md composition order; the engine ' +
            'launches in your emitted order and never re-sorts. Use find/ls; do NOT cd.',
    ].join('\n\n');

const flowLensPrompt = (u, reg) =>
    [
        CONTEXT,
        REG[reg].stance,
        LAW,
        INFO_LAW,
        SELF_CHECK,
        'TASK: read-only INTERIOR FLOW LENS over unit ' +
            u.tag +
            ' — the pages: ' +
            u.pages.join(', ') +
            ' (investigate, do NOT edit). Read EVERY page IN FULL, top to bottom, plus every sibling page each composes or is ' +
            'composed by (seam partners both directions, entry owners, vocabulary sources — full reads, never skims), and the ' +
            'package README.md + ARCHITECTURE.md at ' +
            u.root +
            '. HUNT, per file and across the unit: LOGIC THREADS traced end to end — admission -> canonical owner -> rail -> ' +
            'projection -> egress; a thread whose any end is missing, thin, or naive is a `partial` finding naming BOTH the ' +
            'strong and the weak end; `flow` (rail breaches, dead ends, unreachable cases, admission done twice or never, ' +
            'orphaned carriers); `splitbrain` (one concern spelled two ways, dual paradigms on one rail); `differentiation` ' +
            '(parallel shapes, sibling name families, twin entries one polymorphic owner should carry — including a concept a ' +
            'sibling page or folder also models); ENTRY POINTS (every entry into the unit named; sprawl where one polymorphic ' +
            'entry should discriminate is `differentiation`); `naive` (mini fields, capability-thin owners, enumerated rosters ' +
            'where a generator belongs); `phantom_forgotten` vs `phantom_lie` (a cited member/type/seam that does not exist — ' +
            'classify by intent evidence, name it); `stale` (references to renamed/moved/dead owners). Judge CURRENT disk only. ' +
            'Return the findings product; an empty census is EARNED by the full read, never a first-pass concession.',
    ].join('\n\n');

const seamLensPrompt = (u, reg) =>
    [
        CONTEXT,
        REG[reg].stance,
        LAW,
        INFO_LAW,
        SELF_CHECK,
        'TASK: read-only EXTERIOR SEAM LENS over unit ' +
            u.tag +
            ' — the pages: ' +
            u.pages.join(', ') +
            ' (investigate, do NOT edit). Ground first: the branch architecture at ' +
            L.root +
            '/.planning/ (README + ARCHITECTURE — strata and dependency law), the package README.md + ARCHITECTURE.md at ' +
            u.root +
            ', ' +
            L.manifest +
            ', and a real `ls` of BOTH .api tiers (' +
            L.shared +
            '/ AND the package .api/). Then read every unit page IN FULL and HUNT the integration axis: `seam` (a cross-page or ' +
            'cross-package boundary/wire/contract the unit composes that is misaligned, recorded at one end only, or behind the ' +
            'counterpart as it NOW stands on disk — read the foreign end, name both endpoints); `underutilized` (a catalog ' +
            'capability either tier admits for these concepts that no fence exploits, or exploits shallowly — one call where the ' +
            'surface carries a family; name the concrete member in its exact catalog spelling); `handrolled` (page logic ' +
            're-deriving what an admitted package or an upstream libs/ owner already owns — duplication pressure, name the ' +
            'owning surface); `stale` (manifest, .api anchor, or index-doc rows contradicting the pages); cross-folder ' +
            '`differentiation` (a concept this unit models that another folder also models — route to the canonical owner as ' +
            'fact). Verify every cited member via ' +
            L.verify +
            ' — never memory; a member you cannot verify is recorded in coverage.unverified, never asserted. Judge CURRENT disk ' +
            'only. Return the findings product; an empty census is EARNED by the full read.',
    ].join('\n\n');

const mergePrompt = (rosterPaths, dossier) =>
    [
        CONTEXT,
        'TASK: WORK-DOSSIER MERGE — you consolidate the map fan for the implementor; you WRITE exactly one content file, the ' +
            'work dossier. Read EVERY map report listed here IN FULL from disk (a missing or invalid file is skipped and named ' +
            'in coverage.skipped): ' +
            JSON.stringify(rosterPaths) +
            '. CONSOLIDATE: (1) DEDUPE by `claimKey` — the same key across lanes is ONE defect; merge its evidence, keep the ' +
            'strongest anchors, never restate it twice. (2) JOIN cross-unit threads — findings that are legs of ONE logic chain, ' +
            'seam, or owner story become ONE work item naming every end and every file; a chain left as scattered per-unit rows ' +
            'is a failed merge. (3) ORDER by owner then severity then dependency — shared owners, registries, and vocabulary ' +
            'sources before their consumers; blockers first within an owner. (4) PRESERVE the constraint boundary of every ' +
            'surviving item verbatim — mechanism, owner, reject, acceptance, anchors; you never add prescriptions, designs, or ' +
            'fence sketches, and you never drop a finding except as an exact duplicate (dedupe is the only legal drop — name ' +
            'each drop and its surviving key in the dossier tail). AUTHOR `' +
            dossier +
            '` as markdown: a leading CROSS-UNIT THREADS section, one section per unit for the rest, each item ID-keyed with ' +
            'its claimKey; a section with nothing carries the checks that proved emptiness, never a bare NONE. THE DOSSIER IS ' +
            'YOUR SOLE CONTENT ARTIFACT — return ONLY `index` (each unit mapped to the dossier section headers carrying it), ' +
            'coverage, and summary; content lives in the dossier alone, never restated on the wire.',
    ].join('\n\n');

const implementPrompt = (pages, dossier, mergeOk, rosterPaths, unmapped) =>
    [
        CONTEXT,
        REG.claude.stance,
        LAW,
        BOUNDARY,
        OWN_PASS(SCRATCH + '/impl-ownpass.md'),
        "AUTHORITY — you are the run's ONE writer: full write authority over the ENTIRE project — the target pages, sibling " +
            'folders, other packages, other languages (at their branch doctrine bar per BOUNDARY LAW), the package index docs ' +
            '(README.md + ARCHITECTURE.md), IDEAS.md, the central manifests, and the `.api` anchors. No sibling writer runs, so ' +
            'nothing routes, nothing races, nothing waits: FULLY DRAIN — every finding you confirm and every ripple an edit ' +
            'exposes is landed in the same pass at its root; `deferred` carries ONLY a row a named blocker genuinely forbids ' +
            '(the blocker and its owner named), and an empty `deferred` attests the drain closed. Index and manifest rows are ' +
            'YOURS to apply directly — `indexApplied` receipts each one.',
        'READ FIRST, IN ORDER — (1) ' +
            L.stackLaw +
            '; (2) `docs/laws/` per LAWS below; (3) the repo `.editorconfig` rules for your language — every error-severity ' +
            'rule is a compile gate, and a claim contradicting one is a fiction to correct; (4) the package README.md + ' +
            'ARCHITECTURE.md and ' +
            L.manifest +
            '.',
        LAWS_READ,
        'TASK: FINALIZE these pages IN PLACE: ' +
            pages.join(', ') +
            '.\nCONSUMPTION, after your own pass per OWN PASS FIRST: ' +
            (mergeOk
                ? 'the WORK DOSSIER at `' +
                  dossier +
                  '` is your consolidated recon — deduped, thread-joined, owner-ordered; read it IN FULL, work it in its own ' +
                  'order (cross-unit threads first, then per-unit sections), and spot-verify every anchor an edit builds on ' +
                  '(re-open MANDATORY — a finding already corrected on disk or whose anchors do not re-confirm is dropped with ' +
                  'its reason in `summary`, never re-fixed).'
                : 'the merge lane died — its raw map reports are your recon; read every listed report IN FULL from disk, dedupe ' +
                  'by `claimKey` as you read (the same key across lanes is ONE defect), and order by owner then severity: ' +
                  JSON.stringify(rosterPaths) +
                  '.') +
            (unmapped.length
                ? ' UNMAPPED units (no surviving map lane) get your own cold hostile read FIRST, hunting the finding kinds yourself: ' +
                  JSON.stringify(unmapped) +
                  '.'
                : '') +
            '\nEach item is a SIGNAL, not law: `mechanism`/`owner`/`reject`/`acceptance` are its constraint boundary — honor ' +
            'the owner and rejected forms, but the DESIGN is yours: implement the densest root-level resolution the boundary ' +
            'admits, never a single-point patch. Apply the finalization law to EVERY page — unify split-brain, collapse ' +
            'differentiation in place, finish every partial thread at every end, realize every phantom_forgotten at full depth, ' +
            'delete every phantom_lie, grow naive owners to the real concept, smooth flow end-to-end, wire every underutilized ' +
            'capability into its owner, replace hand-rolled logic with the owning surface, align every stale seam both ends. ' +
            'CAPABILITY-COMPLETENESS is mandatory: verify every owner body implements what its names and prose promise — a ' +
            'named-but-omitted capability is a defect closed NOW at the same bar as any finding. Verify members via ' +
            L.verify +
            ' — never memory. Return the fix-log — `files` every file edited anywhere in the project, `deltas` every moved ' +
            'symbol/wire as data, both exact. ' +
            HARVEST_LAW,
    ].join('\n\n');

const critiquePrompt = (u, nav, reg) =>
    [
        CONTEXT,
        REG[reg].stance,
        LAW,
        INFO_LAW,
        SELF_CHECK,
        GIT_GROUND,
        'TASK: read-only CRITIQUE of the FINALIZED corpus, unit ' +
            u.tag +
            ' — the pages: ' +
            u.pages.join(', ') +
            ' (investigate, do NOT edit; your findings feed the terminal red-team WRITER, so every finding must be actionable ' +
            'at its anchor). OWN VERDICT FIRST: derive your own defect list from the pages on CURRENT disk before consulting ' +
            'the navigation below — it changes where you look first, never what you conclude. NAVIGATION (facts from the ' +
            'implementor — locations only, no assessments): ' +
            JSON.stringify(nav) +
            '. AUDIT: every finalization-law target on every page (split-brain, differentiation, partial threads, phantoms, ' +
            'naive owners, flow, entry sprawl, unleveraged seams, hand-rolled logic, stale references) — the implementor is ' +
            'presumed to have missed things; verify what its deltas claim against disk, check every seam it touched at BOTH ' +
            'ends, and hunt what it never touched. A defect already resolved on CURRENT disk is DROPPED, never reported. ' +
            'Verify cited members via ' +
            L.verify +
            '. Return the findings product in the FINDING FORM, findings emitted in page order.',
    ].join('\n\n');

const redteamPrompt = (pages, critPaths, backlog, unmapped) =>
    [
        CONTEXT,
        REG.claude.stance,
        LAW,
        BOUNDARY,
        LAWS_READ,
        GIT_GROUND,
        OWN_PASS(SCRATCH + '/rt-ownpass.md'),
        'CHECKPOINT LEDGER: `' +
            SCRATCH +
            '/rt-checkpoint.md` — read it FIRST and skip every tranche it already receipts (an interrupted drain re-enters, ' +
            'never restarts); append one line per tranche AS EACH COMPLETES. HARVEST FILE: append each `harvest` nomination to `' +
            SCRATCH +
            '/rt-harvest.jsonl` (one JSON row per line) the moment it is minted — the doctrine lander sweeps the file, so a ' +
            'killed pass loses no nomination; your returned `harvest` carries the same rows. Your returned `tranches` lists the ' +
            'checkpoint receipt line of every tranche fed — a fed tranche absent from that list is unconsumed, and the pass has ' +
            'failed its mandate.',
        "TASK: TERMINAL RED-TEAM (WRITER — the run's LAST corrector, nothing follows you but the doctrine lander): full write " +
            'authority over the entire project. The finalized pages: ' +
            JSON.stringify(pages) +
            '. Assume the implementor and the critiques missed things and their claims are wrong until disk proves them; your ' +
            'own-pass attack artifact precedes every input. TRANCHE ORDER IS EXECUTION ORDER:\n' +
            '(1) CRITIQUE REPORTS — products ON DISK at deterministic paths (an absent or invalid file means that unit gets ' +
            'your own cold attack instead — the read-only fan owes you nothing): ' +
            JSON.stringify(critPaths) +
            '. Read each present report IN FULL; group findings by `claimKey` then `owner`, blockers first (the same key ' +
            'across lanes is ONE defect with corroborating evidence). Each finding is a SIGNAL: re-open its anchors before ' +
            'editing (MANDATORY); honor `mechanism`/`owner`/`reject`/`acceptance` as the constraint boundary and implement the ' +
            'STRONGEST resolution it admits, never a single-point patch; a finding with a dead anchor, already resolved on ' +
            'disk, or graded `hypothetical` with no substantive re-derivation is rejected with its reason.\n' +
            (backlog.length
                ? '(2) IMPLEMENTOR BACKLOG (named-blocker rows — re-verify each {files, claim} on current disk; no sibling ' +
                  'writer runs now, so a blocker that has cleared is yours to land): ' +
                  JSON.stringify(backlog) +
                  '.\n'
                : '') +
            (unmapped.length ? '(2b) UNMAPPED units from the map fan — your own cold read: ' + JSON.stringify(unmapped) + '.\n' : '') +
            '(3) PRE-MORTEM — beyond the reports, on your own authority: COUNTERFACTUAL on the core owners (a counterfactual ' +
            'REBUILDS with the central assumption removed — the chosen owner kind, the hand-enumerated space, the call-site ' +
            'dispatch — and where the rebuilt form is stronger, BUILD IT IN PLACE; "the current shape also works" is never a ' +
            'refutation); LONG-TAIL (empty/singular/plural/stream/malformed/concurrent/cancelled/partial-failure); the diff of ' +
            'the next feature (one row lands with every consumer untouched or loudly broken); then a FULL COLD RE-REVIEW of ' +
            'every finalization-law dimension against CURRENT disk.\n' +
            '(4) OWN HUNT: hunt PAST every signal list and fix what the fan missed; `beyond` enumerates those fixes, and an ' +
            'empty `beyond` attests the hunt found nothing, never that it did not run.\n' +
            'Every ripple an edit exposes is YOURS in the same pass — seam counterparts both ends, consumer sites, index docs, ' +
            'manifest rows, .api anchors; wire-canonical names stay frozen. Return the fix-log — `remaining` carries ONLY rows ' +
            'verified still-open and genuinely blocked, each naming its blocker and owner; a rejected finding rides `rejected` ' +
            'with its disk fact; an empty `remaining` attests the run fully drained. ' +
            HARVEST_LAW,
    ].join('\n\n');

const doctrinePrompt = (rows, critPaths) =>
    [
        ROOT_LAW,
        'TASK: DOCTRINE LANDER — the durable-learning terminal of a finalization run. Read `docs/laws/README.md` FIRST — it ' +
            'owns the corpus admission and page-shape law; obey it over any restatement. ROUTING EMPHASIS (orders where you look ' +
            'first, never overrides the admission bar): a finalization run corrects and closes a landed corpus, so its lessons ' +
            'weigh toward planning-corpus law and reviewer rules first. Load the `docgen` skill AND the `skill-writer` skill via ' +
            'the Skill tool BEFORE any durable edit; load `mermaid-diagramming` before touching any diagram.\n' +
            "NOMINATIONS (unverified, biased toward their authors' own work — refute by default): " +
            JSON.stringify(rows) +
            '\nAlso sweep `' +
            SCRATCH +
            '/rt-harvest.jsonl` (absent = none) AND the `harvest` array of every critique report at these deterministic paths ' +
            '(an absent or invalid file skips; no other agent transports these rows): ' +
            JSON.stringify(critPaths) +
            ' — dedupe them against NOMINATIONS and adjudicate them identically.\n' +
            'ADJUDICATE each row per the admission bar: cold-read its target surface IN FULL, verify its anchors on CURRENT ' +
            'disk; LAND NOTHING is a first-class verdict.\n' +
            'TOPOLOGY RE-PROOF: re-verify every `docs/laws/topology.md` row whose [SURFACE] this run touched — cull a row whose ' +
            'coupling no longer holds, land a coupling this run proved.\n' +
            'GATE: run `uv run .claude/skills/docgen/scripts/prose_gate.py <every touched .md>` and repair to zero FAILs before ' +
            'returning. Return landed/refined/rejected (each rejection with its reason)/files/summary.',
    ].join('\n\n');

// --- [COMPOSITION] ---------------------------------------------------------------------

if (REJECTED.length) log('Rejected targets outside libs/{csharp,python,typescript}: ' + REJECTED.join(', '));
if (!TARGETS.length) {
    log('No targets — pass a package root, an array of planning sub-folders, or {targets}. Empty args is a no-op.');
    return { targets: [], units: 0 };
}
if (!LANG_KEY) {
    log('Targets must live under ONE language root (libs/csharp | libs/python | libs/typescript). Got: ' + JSON.stringify(TARGETS));
    return { targets: TARGETS, units: 0 };
}

phase('Scope');
// One bounded re-attempt: a silently dead scope agent would no-op the whole run.
const planOpts = { label: 'scope', phase: 'Scope', model: 'sonnet', effort: 'low', schema: PLAN_SCHEMA, stallMs: STALL };
const plan = (await slot(() => run(planPrompt(), planOpts))) || (await slot(() => run(planPrompt(), { ...planOpts, label: 'scope:r1' })));
const ROOT = (plan && plan.root) || TARGETS[0];
// R6 guard: model-emitted page paths are validated against the resolved root before any lane dispatches on them.
const FOLDERS = ((plan && plan.folders) || [])
    .map((f) => ({ folder: f.folder, pages: (f.pages || []).filter((p) => typeof p === 'string' && p.indexOf(ROOT + '/') === 0) }))
    .filter((f) => f.folder && f.pages.length);
const PAGES = FOLDERS.flatMap((f) => f.pages);
// Unit = the map/critique granularity: one `.planning/<sub>` folder, oversize folders split into ceiling-bounded segments.
const UNITS = FOLDERS.flatMap((f) => {
    const segs = f.pages.length > UNIT_MAX ? evenChunk(f.pages, UNIT_MAX) : [f.pages];
    return segs.map((pages, i) => {
        const name = f.folder + (segs.length > 1 ? '.' + (i + 1) : '');
        return { folder: f.folder, name, tag: ROOT.split('/').pop() + '.' + name, pages, root: ROOT };
    });
});
log('Scope[' + LANG_KEY + ']: ' + UNITS.length + ' unit segment(s), ' + PAGES.length + ' page(s) at ' + ROOT + '; CAP=' + CAP);
if (!UNITS.length) {
    log('No pages resolved under the targets');
    return { targets: TARGETS, language: LANG_KEY, units: 0 };
}

phase('Map');
// Two sol-medium lenses per unit — interior flow, exterior seam — findings reports on disk, thin receipts on the wire.
const mapRoster = (
    await Promise.all(
        UNITS.flatMap((u) => [
            slot(() =>
                recon((reg) => flowLensPrompt(u, reg), {
                    label: 'map:flow:' + u.tag,
                    phase: 'Map',
                    schema: FINDINGS_SCHEMA,
                    scope: u.pages,
                    hl: { arr: 'findings', group: 'kind' },
                    codexEffort: 'medium',
                }),
            ),
            slot(() =>
                recon((reg) => seamLensPrompt(u, reg), {
                    label: 'map:seam:' + u.tag,
                    phase: 'Map',
                    schema: FINDINGS_SCHEMA,
                    scope: u.pages,
                    hl: { arr: 'findings', group: 'kind' },
                    codexEffort: 'medium',
                }),
            ),
        ]),
    )
).filter(Boolean);
const MAP_REPORTS = mapRoster.filter((r) => r.ok).map((r) => r.report);
// A unit is UNMAPPED only when BOTH its lenses died — one surviving lens still grounds it.
const mappedPages = new Set(mapRoster.filter((r) => r.ok).flatMap((r) => r.scope));
const UNMAPPED = UNITS.filter((u) => !u.pages.some((p) => mappedPages.has(p))).map((u) => ({ unit: u.tag, pages: u.pages }));
log(
    'Map: ' +
        mapRoster.filter((r) => r.ok).length +
        '/' +
        mapRoster.length +
        ' lens(es) ok, ' +
        mapRoster.filter((r) => r.ok).reduce((a, r) => a + r.entries, 0) +
        ' finding(s)' +
        (UNMAPPED.length ? ' — UNMAPPED unit(s): ' + UNMAPPED.map((x) => x.unit).join(', ') : ''),
);

// One merge lane consolidates the fan into the single work dossier; its death degrades to raw-report consumption, never blocks.
const WORK_DOSSIER = SCRATCH + '/work-dossier.md';
const merge = MAP_REPORTS.length
    ? await slot(() =>
          recon(mergePrompt(MAP_REPORTS, WORK_DOSSIER), {
              label: 'map:merge',
              phase: 'Map',
              schema: MERGE_SCHEMA,
              scope: PAGES,
              hl: { arr: 'index' },
              writes: true,
              codexEffort: 'medium',
              calls: 120,
          }),
      )
    : null;
const MERGE_OK = !!(merge && merge.ok);
log(
    'Map: merge ' +
        (MERGE_OK ? 'consolidated -> ' + WORK_DOSSIER : MAP_REPORTS.length ? 'DIED — implementor consumes raw reports' : 'skipped (no ok reports)'),
);

phase('Implement');
// The first full-project writer, but NOT the run's terminal reconciler — the red-team drains its backlog after it, so it rides opus.
const implFire = (suffix) =>
    slot(() =>
        run(implementPrompt(PAGES, WORK_DOSSIER, MERGE_OK, MAP_REPORTS, UNMAPPED), {
            label: 'implement' + suffix,
            phase: 'Implement',
            model: 'opus',
            effort: 'high',
            schema: IMPL_SCHEMA,
            stallMs: STALL,
        }),
    );
const impl = (await implFire('')) || (await retryLane(() => implFire(':r1')));
// CHAIN CONTINUATION: a dead implementor never blocks the reviews — the critique fan and the red-team still correct the
// corpus as it stands on disk; navigation simply arrives empty.
const nav = impl ? { files: impl.files || [], deltas: impl.deltas || [] } : { files: [], deltas: [] };
const BACKLOG = (impl && impl.deferred) || [];
log(
    'Implement: ' +
        (impl
            ? impl.verdict +
              '; ' +
              (impl.files || []).length +
              ' file(s), ' +
              (impl.indexApplied || []).length +
              ' index row(s), ' +
              BACKLOG.length +
              ' blocked row(s)'
            : 'DIED after retries — reviews continue against current disk'),
);

phase('Review');
// One sol-high read-only critique per unit; products at deterministic paths so the red-team survives any dead receipt.
const critRoster = (
    await Promise.all(
        UNITS.map((u) =>
            slot(() =>
                recon((reg) => critiquePrompt(u, nav, reg), {
                    label: 'crit:' + u.tag,
                    phase: 'Review',
                    schema: FINDINGS_SCHEMA,
                    scope: u.pages,
                    hl: { arr: 'findings', group: 'kind' },
                    codexEffort: 'high',
                    clockMs: CRIT_CLOCK,
                }),
            ),
        ),
    )
).filter(Boolean);
const CRIT_PATHS = UNITS.map((u) => reportOf('crit:' + u.tag)); // deterministic — every path checked on disk regardless of receipt
log(
    'Review: ' +
        critRoster.filter((r) => r.ok).length +
        '/' +
        critRoster.length +
        ' critique(s) ok, ' +
        critRoster.filter((r) => r.ok).reduce((a, r) => a + r.entries, 0) +
        ' finding(s)',
);

// Terminal RED-TEAM: the run's ONE fable seat — the last corrector, full-repo authority draining every critique row, the
// implementor backlog, and its own hunt to a fixpoint, so the premium judgment spend lands where every residual is absorbed.
const rtFire = (suffix) =>
    slot(() =>
        run(redteamPrompt(PAGES, CRIT_PATHS, BACKLOG, UNMAPPED), {
            label: 'redteam' + suffix,
            phase: 'Review',
            model: 'fable',
            effort: 'high',
            schema: RT_SCHEMA,
            stallMs: STALL,
        }),
    );
const rt = (await rtFire('')) || (await retryLane(() => rtFire(':a1')));
log(
    'Review: red-team ' +
        (rt
            ? (rt.files || []).length +
              ' file(s), ' +
              (rt.resolved || []).length +
              ' resolved, ' +
              (rt.rejected || []).length +
              ' rejected, ' +
              (rt.remaining || []).length +
              ' remaining'
            : 'DIED after retries — checkpoint and harvest file survive on disk'),
);

phase('Close');
// The lander is the ONLY transport for critique-report harvest arrays and the red-team harvest file — a dead writer loses nothing.
const HARVEST_ROWS = ((impl && impl.harvest) || []).concat((rt && rt.harvest) || []);
const doctrine =
    HARVEST_ROWS.length || CRIT_PATHS.length || !rt
        ? await slot(() =>
              run(doctrinePrompt(HARVEST_ROWS, CRIT_PATHS), {
                  label: 'doctrine',
                  phase: 'Close',
                  model: 'opus',
                  effort: 'high',
                  schema: DOCTRINE_SCHEMA,
                  stallMs: STALL,
              }),
          )
        : null;
return {
    targets: TARGETS,
    language: LANG_KEY,
    root: ROOT,
    units: UNITS.length,
    pages: PAGES.length,
    mapped: mapRoster.filter((r) => r.ok).length,
    unmapped: UNMAPPED,
    merged: MERGE_OK,
    implement: impl && {
        verdict: impl.verdict,
        files: (impl.files || []).length,
        indexApplied: (impl.indexApplied || []).length,
        collapsed: impl.collapsed,
        realized: impl.realized,
        summary: impl.summary,
    },
    critiques: critRoster.filter((r) => r.ok).length,
    redteam: rt && {
        files: (rt.files || []).length,
        resolved: (rt.resolved || []).length,
        rejected: (rt.rejected || []).length,
        beyond: (rt.beyond || []).length,
        summary: rt.summary,
    },
    remaining: (rt && rt.remaining) || BACKLOG,
    doctrine: doctrine && {
        nominated: HARVEST_ROWS.length,
        landed: (doctrine.landed || []).length,
        refined: (doctrine.refined || []).length,
        rejected: (doctrine.rejected || []).length,
        files: doctrine.files || [],
        summary: doctrine.summary,
    },
};
