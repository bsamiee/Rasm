export const meta = {
    name: 'rebuild',
    whenToUse:
        'The standing hostile rebuild pass for any libs/ planning corpus: pass targets (file / sub-folder / package root, any number, any language mix); it plans, maps every .planning sub-folder with its own opus + terra lens pair, ideates per package as two lanes (corrections census + bigger-ideas worklist), then hostile-implements, critiques, and red-teams every sub-folder-aligned page batch CONCURRENTLY at each owning-language doctrine bar under the own-pass-first input ladder with libs-wide ripple authority, and a read-only finder fan plus one terminal fixer close the run.',
    description:
        'Language-agnostic rebuild engine over libs/{csharp,python,typescript} planning corpora. args = a target path, an array of paths, or {targets}; empty = no-op; targets may MIX languages — each batch derives its doctrine, both .api tiers, casing, and member-verification rail from its own owning package. Plan (1 opus) expands targets to pages in dependency order under the owning-package charter, seam-cohesive pages adjacent. Map runs ONCE per SUB-FOLDER UNIT (<pkg>/.planning/<sub>; root-level pages pool as one unit): an OPUS deep-map lane (architecture, ownership boundaries per page, seams, cross-folder relationships, domain gaps, underutilized capability — INFORMATION anchored to disk, never code) beside a gpt-5.6-terra two-tier .api inventory lane (codex wrapper, verified member + stacking inventory), each writing a per-unit dossier every batch touching that unit reuses — package-level mapping dilutes depth on a large package and is the rejected form. Ideate runs per owning package as TWO lanes with disjoint charters: a CORRECTIONS CENSUS (opus — drift, phantoms, spelling repairs, wire mismatches, wiring gaps, per sub-folder, deduped and disk-verified; the non-binding fix addendum) and a BIGGER-IDEAS worklist (fable — NEW capabilities, dimensions, modalities, families the domain admits beyond correction, under a value bar that routes correction-shaped entries back to the census; ambition, never prescriptions or a ceiling). Build packs WHOLE sub-folder units into batches (max 8 pages, oversize units split) so batch ownership aligns with the map dossiers, ALL batches concurrent under one agent-level slot scheduler capped at the runtime concurrency clamp: per batch ONE page-scoped gpt-5.6-terra doctrine-bar quality lens (codex wrapper; product to disk, receipt on the wire; CODEX flag false restores native lanes) then implement (fable), critique (ONE gpt-5.6-sol codex lane per batch, workspace-write, full conformance audit in place with its fixlog to disk), and redteam (fable, terminal — reading the critique fixlog from disk as refutation targets and FOLDING its surviving deferred/index/seam rows into the batch record) chained behind their own batch only. Every batch writer works the OWN-PASS-FIRST input ladder: (1) its own blind hostile pass — cold defect list, collapses, naivety kills, body rebuilds derived from disk BEFORE any recon opens, the majority of the diff by mandate (a diff mapping one-to-one onto recon rows fails the tripwire); (2) the unit map dossiers as grounding to verify and exceed; (3) the corrections census rows intersecting its pages as an addendum; (4) the bigger-ideas entries its pages own, realized at the strongest form disk admits. Handoffs between chain stages carry NAVIGATION FACTS ONLY (files, symbol deltas, seam rows, deferred backlog) — never self-assessment; reviewers derive their own defect list from disk first and receive prior claims strictly as refutation targets. Cross-batch coordination runs through per-batch seam-ledger files in scratch. Every writer holds LIBS-WIDE ripple authority under four non-arbitrary bounds: evidence (anchored ripples only), expand-form (concurrent foreign edits additive; collapse serializes to the fixer), depth (first-order fixed both ends now; second-order recorded to the backlog), and decision/propagation (index docs + central manifests + IDEAS rows single-writer via the terminal fixer; ruled-contract propagation distributed). Close: read-only finder fan per language on gpt-5.6-terra (landed pages + seam-ledger verification + cross-page duplication/collapse + split-brain smoothing + one governance finder per language), typed anchored findings graded substantive|hypothetical written as on-disk reports with thin receipts, stale findings discarded; ONE terminal fable fixer reads every ok report IN FULL from disk (failed-finder territory cold-read first), applies every index row, re-verifies findings as signals, drains the deferred backlog AND the unclaimed corrections-census rows, hunts past everything, and returns the final fixlog. Nothing follows the fixer.',
    phases: [
        {
            title: 'Plan',
            detail: 'one thin agent expands the targets into the dependency-ordered, seam-cohesion-adjacent page list under each owning-package charter: existing pages as rebuild, charter-demanded absent pages as new, settled pages skipped',
            model: 'opus',
        },
        {
            title: 'Map',
            detail: 'per .planning sub-folder unit, ONCE: an OPUS deep-map lane (architecture, ownership, seams, cross-folder relationships, domain gaps, underutilized capability — information, no code) beside a gpt-5.6-terra two-tier .api inventory lane (codex wrapper), each writing a per-unit dossier every batch touching that unit reuses',
            model: 'opus',
        },
        {
            title: 'Ideate',
            detail: 'per owning package, TWO lanes: a corrections census (opus — the deduped disk-verified fix addendum, per sub-folder) and a bigger-ideas worklist (fable — new capabilities beyond correction under the value bar); disjoint charters, never one merged log',
        },
        {
            title: 'Build',
            detail: 'sub-folder-packed batches all concurrent under the agent-level slot cap: per batch ONE gpt-5.6-terra doctrine-bar lens (codex wrapper; product to disk, receipt on the wire) then implement (fable), critique (gpt-5.6-sol codex lane, fixlog to disk), redteam (fable, folding the critique rows forward) chained behind their own batch; every writer works the own-pass-first four-rung input ladder — own blind hostile pass primary, map dossiers grounding, census rows addendum, ideas ambition; navigation-fact handoffs, seam-ledger coordination, libs-wide bounded ripple authority',
        },
        {
            title: 'Close',
            detail: 'read-only gpt-5.6-terra finder fan (codex wrappers; products to disk, receipts on the wire) per language over landed pages + the seam ledger + cross-page duplication/collapse + split-brain smoothing, plus one governance finder per language; ONE terminal fable fixer reads the reports from disk, applies index rows, resolves findings as signals, drains the deferred backlog and the unclaimed census rows, hunts past them, returns the final fixlog',
        },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAP = 14; // runtime concurrency clamp is min(16, cores-2) = 14 on this machine; matching it keeps the stagger honest
const STAGGER_MS = 1500;
const STALL = 300000;
const DRAIN_ROUNDS = 4; // terminal drain fixpoint cap; the progress gate (no shrinkage -> stop) is the real bound
const CODEX_STALL = 1500000; // wrapper stall sits above the codex effort tier's blocking-call ceiling: a silent live MCP call is legal waiting, never a stall
const SOL_STALL = 2400000; // sol critique holds one long blocking MCP call at the operator-default tier; stall detection must outlast it
const BATCH_MAX = 10; // packing ceiling; per-sub-folder maps + census legwork carry the navigation, so a writer holds ~10 dense pages without fidelity loss
const FINDER_PAGES = 8; // landed pages per close-phase finder
const CODEX = true; // recon/finder lanes run on gpt-5.6-terra via the codex wrapper; false restores native opus lanes

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
// Absolute repo root for codex-lane cwd + report paths; args.root retargets an isolated checkout (worktree validation).
const ROOT_DIR =
    isObj && typeof argsIn.root === 'string' && argsIn.root.trim()
        ? argsIn.root.trim().replace(/\/+$/, '')
        : '/Users/bardiasamiee/Documents/99.Github/Rasm';
// Per-instance scratch dir — lane report files + grounding dossiers + per-batch seam-ledger files. Minted deterministically from the
// normalized target set (clock/randomness would break resume): one FLAT dir per instance under .claude/scratch/, a human-readable
// basename slug plus an FNV-1a tail so distinct target sets never share a directory and a resume rehydrates the same one.
const fnv1a = (s) => {
    let h = 0x811c9dc5;
    for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 0x01000193);
    return (h >>> 0).toString(16).padStart(8, '0').slice(0, 6);
};
const SCRATCH =
    '.claude/scratch/' +
    ('rebuild-' + TARGETS.map((t) => t.split('/').pop().toLowerCase()).join('-')).replace(/[^a-z0-9.-]+/g, '-').slice(0, 60) +
    '-' +
    fnv1a(JSON.stringify(TARGETS));

// --- [MODELS] --------------------------------------------------------------------------

const UNDERUTIL = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['catalog', 'capability'],
        properties: { catalog: { type: 'string' }, capability: { type: 'string' } },
    },
};

const PLAN_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['packages', 'pages', 'unresolved'],
    properties: {
        packages: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['name', 'root', 'planning', 'api', 'note'],
                properties: {
                    name: { type: 'string' },
                    root: { type: 'string' },
                    planning: { type: 'string' },
                    api: { type: 'string' },
                    note: { type: 'string' },
                },
            },
        },
        pages: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'kind'],
                properties: {
                    page: { type: 'string' },
                    kind: { type: 'string', enum: ['new', 'rebuild'] },
                },
            },
        }, // ARRAY ORDER IS DEPENDENCY + COHESION ORDER — the engine never re-sorts
        unresolved: { type: 'array', items: { type: 'string' } },
    },
};

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const ANCHOR_INFO = {
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

const ANCHOR_DEFECT = {
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

const CTX_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['worklist', 'coverage', 'summary'],
    properties: {
        worklist: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'kind', 'owns', 'contextNote', 'seams', 'files', 'anchors'],
                properties: {
                    page: { type: 'string' },
                    kind: { type: 'string', enum: ['new', 'rebuild'] },
                    owns: { type: 'string' },
                    contextNote: { type: 'string' },
                    seams: { type: 'array', items: { type: 'string' } },
                    files: { type: 'array', items: { type: 'string' } }, // files the consumer must open for this entry
                    anchors: { type: 'array', items: ANCHOR_INFO },
                },
            },
        },
        coverage: COVERAGE,
        summary: { type: 'string' },
    },
};

const API_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['worklist', 'coverage', 'summary'],
    properties: {
        worklist: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'apiUsed', 'apiUnderutilized', 'stackingInventory', 'files', 'anchors'],
                properties: {
                    page: { type: 'string' },
                    apiUsed: { type: 'array', items: { type: 'string' } },
                    apiUnderutilized: UNDERUTIL,
                    stackingInventory: { type: 'string' }, // capability inventory as fact — catalog members + admitting concepts, never a prescribed design
                    files: { type: 'array', items: { type: 'string' } },
                    anchors: { type: 'array', items: ANCHOR_INFO },
                },
            },
        },
        coverage: COVERAGE,
        summary: { type: 'string' },
    },
};

const BAR_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['findings', 'weak', 'coverage', 'summary'],
    properties: {
        findings: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['claimKey', 'page', 'law', 'severity', 'claim', 'mechanism', 'owner', 'reject', 'acceptance', 'files', 'anchors'],
                properties: {
                    claimKey: { type: 'string' }, // <law>|<owner>|<primary symbol> — stable across lanes, never lane wording
                    page: { type: 'string' },
                    law: { type: 'string' },
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: { type: 'string' }, // the observed defect as fact
                    mechanism: { type: 'string' }, // WHY it fails the law — factual, zero repair verbs
                    owner: { type: 'string' }, // canonical owner that must absorb the resolution
                    reject: { type: 'array', items: { type: 'string' } }, // forms the repair must NOT take
                    acceptance: { type: 'array', items: { type: 'string' } }, // signals proving resolution
                    files: { type: 'array', items: { type: 'string' } },
                    anchors: { type: 'array', items: ANCHOR_DEFECT },
                },
            },
        },
        weak: { type: 'array', items: { type: 'string' } },
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
}; // navigation facts: what moved, as data, zero adjectives

const DEFERRED = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['files', 'claim'],
        properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } },
    },
}; // the counted backlog: second-order + live-batch-scope ripples

const BEYOND = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['catalog', 'member'],
        properties: { catalog: { type: 'string' }, member: { type: 'string' } },
    },
};

const INDEXROWS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['doc', 'row'],
        properties: { doc: { type: 'string' }, row: { type: 'string' } },
    },
}; // doc = index doc, central manifest, or IDEAS.md; row = the exact row text

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

// Required-but-empty arrays are attestations: forced seamsTouched/beyondMap/indexRows/deltas/deferred/dossierPhantoms
// make "read fully / exceed the reports / repair both ends / record the backlog" structurally checkable, never wishful prose.
const FIXLOG_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: [
        'files',
        'verdict',
        'summary',
        'seamsTouched',
        'deltas',
        'deferred',
        'beyondMap',
        'indexRows',
        'harvest',
        'dossierPhantoms',
        'collapsed',
        'extended',
    ],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        verdict: { type: 'string', enum: ['authored', 'rebuilt', 'refined', 'clean'] },
        collapsed: { type: 'string' },
        extended: { type: 'string' },
        summary: { type: 'string' },
        seamsTouched: SEAMS,
        deltas: DELTAS,
        deferred: DEFERRED,
        beyondMap: BEYOND,
        indexRows: INDEXROWS,
        harvest: HARVEST,
        dossierPhantoms: { type: 'array', items: { type: 'string' } },
    },
};

const REVIEW_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'verdict', 'summary', 'seamsTouched', 'deltas', 'deferred', 'beyondMap', 'indexRows', 'harvest', 'extended'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        verdict: { type: 'string', enum: ['fixed', 'clean'] },
        extended: { type: 'string' },
        summary: { type: 'string' },
        seamsTouched: SEAMS,
        deltas: DELTAS,
        deferred: DEFERRED,
        beyondMap: BEYOND,
        indexRows: INDEXROWS,
        harvest: HARVEST,
    },
};

const FINDINGS_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['findings', 'coverage', 'summary'],
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
                    'class',
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
                    claimKey: { type: 'string' }, // <class>|<owner>|<primary symbol or absence route> — stable across lanes, never lane wording
                    target: { type: 'string' }, // short display label for the defect
                    files: { type: 'array', items: { type: 'string' } }, // files the fixer must open or edit first
                    class: { type: 'string', enum: ['missing', 'wrong', 'faked', 'naive', 'drift', 'phantom'] },
                    grade: { type: 'string', enum: ['substantive', 'hypothetical'] }, // substantive = concrete on-disk defect; hypothetical = requires an invented implausible input
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: { type: 'string' }, // the observed defect as fact
                    anchors: { type: 'array', items: ANCHOR_DEFECT },
                    mechanism: { type: 'string' }, // WHY it fails the law/doctrine — factual, zero repair verbs
                    owner: { type: 'string' }, // canonical owner that must absorb the resolution
                    reject: { type: 'array', items: { type: 'string' } }, // forms the repair must NOT take
                    acceptance: { type: 'array', items: { type: 'string' } },
                },
            },
        }, // signals proving resolution
        coverage: COVERAGE,
        summary: { type: 'string' },
    },
};

// Required-but-possibly-empty `beyond` is an attestation: the fixer's own hunt ran, not only the signal list.
const FIXER_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'indexApplied', 'resolved', 'backlogDrained', 'beyond', 'rejected', 'remaining', 'harvest', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        harvest: HARVEST,
        indexApplied: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['doc', 'action'],
                properties: {
                    doc: { type: 'string' },
                    action: { type: 'string' },
                },
            },
        },
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
        backlogDrained: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['claim', 'action'],
                properties: {
                    claim: { type: 'string' },
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
        remaining: DEFERRED, // rows verified still-open and genuinely blocked; the drain loop re-feeds them until empty or no progress
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

// LANG carries routing data and engine-parameter rows ONLY — doctrine content is reached through READ_FIRST at the source, never paraphrased here.
const LANG = {
    cs: {
        key: 'cs',
        name: 'C#',
        root: 'libs/csharp',
        stack: 'docs/stacks/csharp',
        casing: 'PascalCase',
        corpus: 'libs/csharp planning corpus (markdown specs of intended C# package designs)',
        strata:
            'CLAUDE.md manifest + WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; ' +
            'depend strictly upward; a host-neutral owner only where a non-Rhino runtime consumes the contract).',
        stackFloor:
            'docs/stacks/csharp is the FLOOR, never the ceiling — every fence meets it and pushes past it to the strongest ' +
            'form the doctrine admits; the tools/cs-analyzer compiled-doctrine gate enforces it (a true positive is architecture ' +
            'pressure, a false positive is rule pressure, never a suppression).',
        apiTiers:
            'the SHARED substrate catalogs `libs/csharp/.api/*.md` (Thinktecture generated owners, LanguageExt ' +
            'rails/effects/schedules/immutable collections, QuikGraph, Mapperly and siblings) AND the folder catalogs ' +
            '`<package>/.api/*.md`, always layering the universal Thinktecture/LanguageExt rails onto the domain packages, never the ' +
            'folder set alone.',
        verify:
            '`uv run python -m tools.assay api` (assay blocked or unavailable: the `.api` catalogs, the nuget MCP for feed ' +
            'truth, and Context7/exa/tavily for the official surface own the fallback)',
        vocab: '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject]`/`Fold`/the rails)',
        slur: 'naive, surface-level code dressed in the right vocabulary',
        illusion: 'a `.api`/host member cited but never verified',
        docBloat: 'XML-doc',
        collapseInto:
            'ONE `[Union]` / `[SmartEnum<TKey>]` / `[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case ' + 'family IN THE SAME FILE',
        gapPkg:
            'LIBRARY_DEPTH: e.g. an IFC schema gives a zone its quantities, space boundaries, and properties the page never ' +
            'reads — stacking that full surface IS new functionality woven into the owner, not a denser spelling of the same call',
        gapDomain:
            'a BIM zone owns its boundary/area/volume, per-kind attributes — a fire compartment a rating, a thermal zone a ' +
            'setpoint, a load group its combinations, an MEP system its medium/flow/pressure — adjacency/nesting topology, and ' +
            'coverage/aggregation/spatial-query operations, not a flat member-id set alone; a profile owns section properties, grade, ' +
            'fabrication + code-check inputs, not width/height; a durable store owns its constraints, indexes, partitions, RLS, ' +
            'migration, and lifecycle, not naive columns',
        ownerGrammar:
            'a CASE in the existing closed family, a ROW or richer data on the existing smart-enum, a FIELD or a composed ' +
            '`[ValueObject]`/`[ComplexValueObject]` on the existing record, an OPERATION on the existing surface, or a POLICY_VALUE ' +
            'on the existing vocabulary',
        deepPkgs: 'LanguageExt/Thinktecture/MathNet/CSparse',
        body:
            'nested `Bind`/`Map` lambda towers where LINQ query syntax or one composed `Eff`/`Fin` pipeline reads flat; ' +
            '`Match(_ => unit)` and swallowed `IfFail` where a typed failure case belongs; manual loop/accumulator plumbing ' +
            'where `Fold`/`Traverse`/`Sequence`/`Partition` compose the join; helper statics and one-off records orbiting an ' +
            'owner instead of living on it',
        exhaust: 'total generated `Switch`, no silent `_` arm',
        modern: 'Latest modern C# 14 on net10',
        mechanics: '',
        fileOrg: 'apply the `docs/stacks/csharp` file-organization + section-order law',
    },
    py: {
        key: 'py',
        name: 'Python',
        root: 'libs/python',
        stack: 'docs/stacks/python',
        casing: 'snake_case',
        corpus: 'libs/python planning corpus (markdown specs of intended Python module designs)',
        strata: 'CLAUDE.md manifest law governs.',
        stackFloor:
            'docs/stacks/python is the bar and docs/stacks/csharp is the density/ambition FLOOR — match its richness, ' +
            'never import C#-shaped idioms.',
        apiTiers:
            'the SHARED/universal branch catalogs `libs/python/.api/*.md` (anyio, expression, msgspec, pydantic, ' +
            'pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*) AND the folder catalogs ' +
            '`<package>/.api/*.md`, always layering the shared/universal rails ON TOP OF the folder-specific domain packages, never ' +
            'the folder set alone.',
        verify:
            '`uv run --frozen python -m tools.assay api resolve <pkg>` (a gated/uninstalled package, or a blocked/unavailable ' +
            'assay, falls back to its catalog/official surface)',
        vocab: '(`@tagged_union`/`frozendict`/`Result`/`Option`/the rails)',
        slur: 'naive, surface-level, old-style Python dressed in the right vocabulary',
        illusion: 'a `.api` member cited but never verified',
        docBloat: 'docstring',
        collapseInto: 'one closed `@tagged_union`/`Literal`/`StrEnum` family, a derived `frozendict` table, or a fold IN THE SAME FILE',
        gapPkg: 'BOTH tiers; stacking that full surface IS new functionality woven into the owner, not a denser spelling of the same call',
        gapDomain:
            'a dimension owner owns the full ISO 129-1 linear/aligned/angular/radial/diameter/ordinate/chain/baseline + ' +
            'tolerance family, not a single linear case; a layer codec owns the full ISO 13567 + NCS discipline/major/minor/status ' +
            'structure, not a flat string',
        ownerGrammar:
            'a CASE in the existing closed `@tagged_union`/`Literal`/`StrEnum` family, a ROW or richer data on the ' +
            'existing `frozendict` table, a FIELD on the existing `msgspec.Struct`/Pydantic model/frozen dataclass/`TypedDict`, an ' +
            'OPERATION on the existing surface, or a POLICY_VALUE on the existing vocabulary',
        deepPkgs: 'the admitted both-tier catalogs (expression/msgspec/pydantic/anyio + the folder domain packages)',
        body:
            'nested try/except and if-ladders where the `expression` Result/Option pipeline or one `match` expression ' +
            'reads flat; bare `except` and silently discarded `Result` where a typed failure case belongs; manual ' +
            'loop/accumulator plumbing where fold/traverse/partition combinators compose the join; module-level helper ' +
            'functions and one-off aliases orbiting an owner instead of living on it',
        exhaust: 'total `match` + `assert_never` over the FULL case set',
        modern: 'py3.15-modern only',
        mechanics:
            'MECHANICAL EXECUTABILITY — a fence is a signature-and-implementation contract: mentally compile and type-check ' +
            'each one against the real cross-page owners it imports, then hunt these defect classes at their owning doctrine sites ' +
            'and fix each in place by growing the existing owner: FENCE-PARSES (`language.md` CLOSED_MATCH_SITE) · MODEL-COHERENCE ' +
            '(README CORPUS_LAW) · TOTAL-DISPATCH (`shapes.md` families) · SINGLE-FACT-EVIDENCE (`rails-and-effects.md` ' +
            'STATE_RECEIPTS + `boundaries.md` BYTE_IDENTITY) · LOOP-OFFLOAD (`concurrency.md` OFFLOAD_LANE) · HANDLE-LIFETIME + ' +
            'BINARY-KERNEL (`boundaries.md` CAPSULE_OWNER) · IDENTITY-REGIME (`boundaries.md` MEMO_KEY) · TEMPLATE-SAFETY ' +
            '(`language.md` TEMPLATE_STRUCTURE_SITE) · STREAM-OVER-MATERIALIZE (`iteration.md` LAZY_COMBINATORS) · ' +
            'NO-EXCEPTION-HOTLOOP (`rails-and-effects.md` EXPRESSION_SPINE) · DERIVED-NOT-PARALLEL + PER-MODE PAYLOADS (README ' +
            'DERIVED_LOGIC). The defect definitions live at the sites; read them there.',
        fileOrg: 'apply the `docs/stacks/python` file-organization + section-order law',
    },
    ts: {
        key: 'ts',
        name: 'TypeScript',
        root: 'libs/typescript',
        stack: 'docs/stacks/typescript',
        casing: 'camelCase',
        corpus: 'libs/typescript planning corpus (markdown specs of intended TypeScript module designs)',
        strata: 'CLAUDE.md manifest law governs.',
        stackFloor: 'docs/stacks/typescript composed in full is the bar — author ultra-advanced TS only, discarding naive idioms wholesale.',
        apiTiers:
            'the SHARED/universal `libs/typescript/.api/*.md` Effect substrate rails AND the folder catalogs ' +
            '`<folder>/.api/*.md`, cross-checked against the published types in node_modules, always layering the shared Effect ' +
            'ecosystem end-to-end ON TOP OF the area-specific packages, never the folder set alone.',
        verify: 'the published types in node_modules (`uv run python -m tools.assay api` over node_modules declarations where a member is novel)',
        vocab: '(`Schema.Class`/`TaggedClass` families, tagged unions, `Effect`/`Layer`, value-derived vocabulary tables)',
        slur: 'naive JavaScript-in-TypeScript dressed in the right vocabulary',
        illusion: '`any`/unsafe `as`/non-null `!` smuggled under a confident surface; a member cited but unverifiable against node_modules',
        docBloat: 'TSDoc',
        collapseInto:
            'ONE deep `Schema.Class`/`TaggedClass`/`TaggedError` family — embedded sub-schemas, brand-in-field ' +
            'refinements, class-carried methods and statics — or ONE tagged discriminated union + exhaustive match, IN THE SAME ' +
            'FILE; CLASS-FIRST: a module-level type alias, interface, or bare `Struct` standing where a class family could carry ' +
            'invariants, statics, and derived projections is a defect, and `Schema.Struct` survives only as an anonymous ' +
            'single-consumer field block',
        gapPkg:
            'BOTH tiers: the shared `libs/typescript/.api/` Effect substrate rails AND the folder domain packages, ' +
            'cross-checked against the published node_modules types; stacking that full surface IS new functionality woven into the ' +
            'owner, not naive Promise/try-catch glue',
        gapDomain:
            'a chart owns scale/axis/series/interaction/annotation families and zoom/brush/tooltip/series-key operations, ' +
            'not two naive renders; a service owns retry/breaker/telemetry/validation/cache layers internally, not a bare fetch; a ' +
            'machine owns hierarchical/parallel regions, guarded transitions, timers, and history as data, not a switch ladder; a ' +
            'projection owns the full transform/diff/patch family the domain needs',
        ownerGrammar:
            'a CASE in the existing tagged discriminated union, a FIELD or embedded sub-schema on the existing ' +
            '`Schema.Class` family, an OVERLOAD or `Function.dual` twin on the existing entrypoint, a STATIC or derived ' +
            'projection on the existing class, a member on the existing `Effect.Service`, a ROW in the existing ' +
            'const-union/table, or a POLICY value on the existing vocabulary',
        deepPkgs:
            'the Effect ecosystem (`Effect`/`Layer`/`Context`/`Schema`/`Stream` + platform/experimental/cluster/' +
            'workflow/sql/rpc/ai) + the area packages',
        body:
            'nested `Effect.flatMap(Effect.flatMap(...))` and pipe-inside-pipe pyramids where `Effect.gen`/`Do`/one flat ' +
            'pipe owns the sequence; `catchAll(() => Effect.void)` blanket swallows where typed `catchTag`/`catchTags` or an ' +
            'explicitly ruled ignore belongs; `flatMap` where `map` serves, manual fold/partition plumbing where ' +
            '`zipWith`/`all`/`validate`/`partition` compose the join, run-and-discard where `tap`/`tapError`/`tapBoth` ' +
            'belongs, sequential steps where `zip`/`all` with concurrency expresses the parallel join; loose module-level ' +
            'consts, aliases, and option-bags orbiting an owner instead of integrating as statics, fields, or derived ' +
            'projections',
        exhaust: 'exhaustive `Match.exhaustive` dispatch (or a checked `never` sink)',
        modern: 'ultra-advanced modern TS only',
        mechanics: '',
        fileOrg: 'apply the `docs/stacks/typescript` file-organization + section-order law',
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms));
// Agent-level slot scheduler: CAP agents in flight across ALL batch chains, staggered launch, work-conserving backfill the moment a
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

// Codex dispatch: the sonnet wrapper makes one blocking Codex MCP call, writes the envelope's content
// to the lane report, and returns mechanical orchestration data. Lane law rides developer-instructions
// (role split, battery-validated); the prompt carries only the task — built REG.codex-shaped by recon()'s
// dispatch branch, never the estate hostile register verbatim; the output contract sits LAST.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const laneLaw = (schema, o) =>
    (o.fix
        ? '<persistence>\nComplete every named move before yielding; do not stop at analysis or a partial edit. If the chosen ' +
          'approach resists, pick the next-best one and proceed. Return without an applied edit only if the territory genuinely ' +
          'admits none.\n</persistence>\n\n<verification>\nAfter editing, re-read each changed file and confirm it is coherent ' +
          'and nothing it carried was lost. Fix what fails before yielding.\n</verification>'
        : '<context_gathering>\nTerritory: the exact files and directories the task names. Do not open files outside it; ' +
          'instruction files (.claude/, CLAUDE.md, AGENTS.md) are always out of scope, while a skill of your own matching an ' +
          'artifact-authoring task is in-contract.\nBudget: at most ' +
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
            'VERBATIM. If the call errors, retry the identical call ONCE; if the retry errors, skip step (3) and return the ' +
            'error through step (4).',
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
// Every codex-dispatched lane routes here: terra by default, sol where o.model says so; CODEX=false restores a fully native run. The task arrives as
// a REGISTER-KEYED BUILDER: the codex branch takes REG.codex (neutral, de-conflicted), every native execution — o.native, CODEX=false, the quota
// fallback — takes REG.claude (the full estate register), so the register follows the EXECUTING model, never the lane name.

// QUOTA FALLBACK: a codex receipt whose failure matches usage/quota/limit re-dispatches the SAME task natively at the role's Claude twin (terra->opus,
// sol->fable, luna->sonnet) — the caller owns the re-dispatch, the sonnet wrapper never executes work itself. The roster row carries `scope` from the
// ORCHESTRATOR (never the lane's self-report) so a failed lane's unmapped territory is exact even when the lane died before writing anything.
const twinOf = (m) => (/-sol/.test(m || '') ? 'fable' : /-luna/.test(m || '') ? 'sonnet' : 'opus');
const nativeLane = (task, o) =>
    agent(
        task +
            '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
            ROOT_DIR +
            '/' +
            SCRATCH +
            '/' +
            fileTag(o.label) +
            '-report.json (Write tool, exactly this absolute path): ' +
            JSON.stringify(o.schema) +
            ' — then return ONLY the receipt: ok, report = ' +
            SCRATCH +
            '/' +
            fileTag(o.label) +
            '-report.json (this repo-relative form, matching codex-lane receipts), entries count, one-line mechanical headline, failure empty.',
        {
            label: o.label,
            phase: o.phase,
            model: o.nativeModel || twinOf(o.model),
            effort: 'high',
            schema: RECEIPT,
            stallMs: o.stallMs || STALL,
        },
    );
// o.native forces the native branch (the opus deep-map lane rides it: a capable native model, not a codex wrapper).
const recon = (taskOf, o) => {
    const task = typeof taskOf === 'function' ? taskOf : () => taskOf;
    return (
        CODEX && !o.native
            ? agent(codexPrompt(o.label, task('codex'), o.schema, o), {
                  label: (o.model && o.model.indexOf('-sol') >= 0 ? 'sol:' : 'terra:') + o.label,
                  phase: o.phase,
                  model: 'sonnet',
                  effort: 'low',
                  schema: RECEIPT,
                  stallMs: o.stallMs || CODEX_STALL,
              }).then((r) => (r && !r.ok && /usage|quota|limit/i.test(r.failure || '') ? nativeLane(task('claude'), o) : r))
            : nativeLane(task('claude'), o)
    ).then((r) => ({
        lane: o.label,
        scope: o.scope || [],
        ok: !!(r && r.ok && r.report),
        report: (r && r.report) || '',
        entries: (r && r.entries) || 0,
        headline: (r && r.headline) || '',
        failure: (r && r.failure) || (r ? '' : 'lane died'),
    }));
};
const chunk = (arr, n) => {
    const o = [];
    for (let i = 0; i < arr.length; i += n) o.push(arr.slice(i, i + n));
    return o;
};
// Even split: ceil(n/max) batches of near-equal size — no runt tail heavying batch 0 and starving the last.
const evenChunk = (arr, max) => chunk(arr, Math.ceil(arr.length / (Math.ceil(arr.length / max) || 1)));
const pkgOf = (p) => p.split('/.planning/')[0]; // package = the write-partition key (index docs live at its root)
// Sub-folder = the map/batch granularity unit: one mapper pair and one batch-ownership seam per `.planning/<sub>`; root-level pages pool as '_root'.
const subOf = (p) => {
    const rest = p.split('/.planning/')[1] || '';
    return rest.includes('/') ? rest.split('/')[0] : '_root';
};
const Lof = (pkg) => LANG[langOf(pkg)] || LANG.cs;
// Scratch paths follow one grammar: SCRATCH + '/' + fileTag(<label>) + '-<artifact>'. Seam ledgers key on the
// batch tag; dossiers key on their recon lane label; lane reports key on the lane label via codexPrompt.
const scratchBase = (pkg, i) => SCRATCH + '/' + fileTag(pkg.split('/').pop() + ':b' + i);
const dossierPath = (lensLabel) => SCRATCH + '/' + fileTag(lensLabel) + '-dossier.md';
// Preserves plan emission order (dependency + cohesion order); dedupe by page, first wins.
const normalizePages = (pl) => {
    const seen = new Set();
    const out = [];
    for (const p of (pl && pl.pages) || []) {
        if (!p || !p.page || seen.has(p.page)) continue;
        seen.add(p.page);
        out.push({ page: p.page, kind: p.kind === 'new' ? 'new' : 'rebuild' });
    }
    return out;
};

// Navigation handoff: FACTS ONLY — files, symbol deltas, seam rows, backlog. Never verdicts, summaries, or adjectives.
const navOf = (logs) => {
    const rows = logs.filter(Boolean);
    return {
        files: [...new Set(rows.flatMap((r) => r.files || []))],
        deltas: rows.flatMap((r) => r.deltas || []),
        seams: rows.flatMap((r) => r.seamsTouched || []),
        deferred: rows.flatMap((r) => r.deferred || []),
    };
};

// --- [SHARED_BLOCKS]

// Every rigor law appears exactly once, here; stages compose subsets. Block order in prompts: stable per-language law first (byte-identical across a
// batch's stages), batch-variable material second, the stage task + output contract LAST — nothing load-bearing mid-prompt.
// Subagents keep the launching session's ORIGINAL project cwd even when the run targets a worktree; only explicit path authority moves a lane, so
// every prompt states the root and natives get exact absolute paths.
const ROOT_LAW =
    'WORKING ROOT: ' +
    ROOT_DIR +
    ' — every relative repo path in this brief resolves against this absolute root; read, write, and edit ONLY under it, never ' +
    'another checkout of the repository.';
const CONTEXT = (L) => ROOT_LAW + '\n\nRasm monorepo — ' + L.corpus + '. ' + L.strata + ' ' + L.stackFloor;

// Register table — one row set per EXECUTING model, keyed by recon()'s dispatch branch. Substance is identical across rows (burden of proof on the
// work, both naivety axes, illusion hunting, no-churn, second-pass self-verify, findings-never-designs); only phrasing forks: claude carries the
// estate hostile register, codex the same demands de-conflicted and neutral — probe-measured: the hostile register makes a codex lane over-read,
// probe out of territory, and spend more input tokens for equal output (the codex skill's prompt-contract law).
const REG = {
    claude: {
        stance: (L) =>
            'STANCE — every pass is hostile: author, critique, and red-team alike. The pages under review were ' +
            'authored by ANOTHER engineer and are under adversarial review; hold every fence naive, shallow, or illusory until it ' +
            'survives a real attack; the burden of proof is on the code, never on you. "Mature", "already strong", "good enough", and ' +
            'a prior clean verdict are rejected self-assessments — most of this corpus is ' +
            L.slur +
            '. Dense, confident, ' +
            'package-fluent code is the PRIME suspect for hollowness: disbelieve every claim a fence makes about itself and verify it ' +
            'against the real domain and the catalogued package surface. NAIVETY is a defect on two orthogonal axes. COVERAGE — the ' +
            'owner models a thin slice of its concept: a 2-case family for a 20-case domain, three fields where the concept carries ' +
            'fifteen. APPROACH — an enumerated roster of styles, variants, or arms where one parameterized generator should GENERATE ' +
            'the space; the roster demotes to seed DATA over named parameters. ILLUSORY code is the primary target: doctrine ' +
            'vocabulary ' +
            L.vocab +
            ', cited packages, confident prose, hollow body — a phantom (' +
            L.illusion +
            '), a name ' +
            'promising capability the body omits, decorative density carrying nothing, a stub dressed as a finished design. Every ' +
            'collapse-signal list in these prompts is a FLOOR, never the complete set. NO CHURN: an edit requires a named violated ' +
            'law or invariant and the concrete case that breaks it — no reproduction, no edit; a clean verdict earned by an attack ' +
            'that finds nothing is a first-class result, proven by adding nothing.',
        selfCheck:
            'MANDATORY SELF-VERIFY (second pass, before returning): adversarially re-derive every entry from disk — ' +
            're-open each cited anchor and confirm it states what the entry claims, re-verify each member spelling against its ' +
            'catalog, trace each seam to both endpoints. An entry that fails re-confirmation is corrected or deleted, never returned; ' +
            'a guess, an assumption, a skimmed summary, or a vague/hedged entry is a defect. Completeness is part of correctness: ' +
            'after the re-read, hunt once more for what the first pass missed — an omitted load-bearing fact is as wrong as a false one.',
        antiAnchor: (L) =>
            'ANTI-ANCHOR LAW: your report and dossier carry FINDINGS, never designs — quality defects graded ' +
            'against the doctrine read at source (name the law and the ' +
            L.stack +
            ' pattern whose application would most deeply ' +
            'transform the page — the collapse, the owner form, the rail — never the resulting code) and capability inventory in ' +
            'catalog-anchored spellings; a fence sketch, a prescribed shape, or a pre-ruled design ANCHORS and WEAKENS the rebuild ' +
            'and is your defect — the implement agent rules every design.',
        ctx: (n) =>
            'TASK: HOSTILE READ-ONLY CONTEXT + SEAM LENS over these ' +
            n +
            ' pages — read-only is the only concession; ' +
            'the hunt is as adversarial as every writing pass (investigate, do NOT edit): ',
        api: (n) => 'TASK: HOSTILE READ-ONLY TWO-TIER STACKING LENS over these ' + n + ' pages (investigate, do NOT edit): ',
        apiVerify:
            'DISBELIEVE the pages — prose claiming a package is ' +
            'composed is verified against the fence body; attack every admitted catalog (both tiers) for the members, combinators, ' +
            'generated surfaces, and native pipelines the concept ADMITS but no fence exploits',
        bar: (n) => 'TASK: HOSTILE READ-ONLY DOCTRINE-BAR ATTACK over these ' + n + ' pages (investigate, do NOT edit): ',
        barAttack: (L) =>
            'attack its quality against the doctrine AT SOURCE — EXTREMELY adversarial: the ' +
            'page is presumed ' +
            L.slur +
            ' until proven otherwise. Hunt',
        finder: (i) => 'TASK: HOSTILE READ-ONLY FINDER, slice ' + i + ' (investigate, do NOT edit).',
        finderStance: 'The landed corpus is presumed defective until your attack finds nothing. ',
        gov: 'TASK: HOSTILE READ-ONLY GOVERNANCE FINDER (investigate, do NOT edit).',
        audit: 'TASK: HOSTILE DOCTRINAL-CONFORMANCE + CAPABILITY AUDIT; fix EACH page in place: ',
    },
    codex: {
        stance: (L) =>
            'REVIEW POSTURE — the pages are unverified work by another engineer: verify every claim a fence makes ' +
            'against the real domain and the catalogued package surface before accepting it; a prior clean verdict or confident ' +
            'prose is not evidence. NAIVETY is a defect on two orthogonal axes. COVERAGE — the owner models a thin slice of its ' +
            'concept: a 2-case family for a 20-case domain, three fields where the concept carries fifteen. APPROACH — an ' +
            'enumerated roster of styles, variants, or arms where one parameterized generator should GENERATE the space; the ' +
            'roster demotes to seed DATA over named parameters. ILLUSORY code is the primary target: doctrine vocabulary ' +
            L.vocab +
            ', cited packages, confident prose, hollow body — a phantom (' +
            L.illusion +
            '), a name promising capability the body ' +
            'omits, a stub dressed as a finished design. Every collapse-signal list in these prompts is a floor, never the ' +
            'complete set. NO CHURN: an edit requires a named violated law or invariant and the concrete case that breaks it; ' +
            'a clean verdict from a check that finds nothing is a first-class result.',
        selfCheck:
            'SELF-VERIFY (second pass, before returning): re-derive every entry from disk — re-open each cited anchor ' +
            'and confirm it states what the entry claims, re-verify each member spelling against its catalog, trace each seam to ' +
            'both endpoints. Correct or delete any entry that fails re-confirmation; never return a guess, an assumption, or a ' +
            'hedged entry. Completeness is part of correctness: after the re-read, hunt once more for what the first pass missed ' +
            '— an omitted load-bearing fact is as wrong as a false one.',
        antiAnchor: (L) =>
            'ANTI-ANCHOR LAW: your report and dossier carry FINDINGS, never designs — quality defects graded ' +
            'against the doctrine read at source (name the law and the ' +
            L.stack +
            ' pattern whose application would most deeply ' +
            'transform the page — the collapse, the owner form, the rail — never the resulting code) and capability inventory in ' +
            'catalog-anchored spellings; a fence sketch, a prescribed shape, or a pre-ruled design anchors the rebuild and is a ' +
            'defect — the implement agent rules every design.',
        ctx: (n) => 'TASK: read-only CONTEXT + SEAM LENS over these ' + n + ' pages (investigate, do NOT edit): ',
        api: (n) => 'TASK: read-only TWO-TIER STACKING LENS over these ' + n + ' pages (investigate, do NOT edit): ',
        apiVerify:
            'verify prose claiming a package is composed against the fence body — never accept the ' +
            'claim; check every admitted catalog (both tiers) for the members, combinators, generated surfaces, and native ' +
            'pipelines the concept ADMITS but no fence exploits',
        bar: (n) => 'TASK: read-only DOCTRINE-BAR review over these ' + n + ' pages (investigate, do NOT edit): ',
        barAttack: () => 'assess its quality against the doctrine AT SOURCE — treat the page as unproven until verified. Report',
        finder: (i) => 'TASK: read-only FINDER, slice ' + i + ' (investigate, do NOT edit).',
        finderStance: 'Verify the landed corpus independently; treat what the run reports about itself as unproven. ',
        gov: 'TASK: read-only GOVERNANCE FINDER (investigate, do NOT edit).',
        audit: 'TASK: DOCTRINAL-CONFORMANCE + CAPABILITY AUDIT; fix EACH page in place: ',
    },
};

const BUILD_LAW = (L) =>
    'BUILD LAW — buildout over removal, always. Removal authority is reserved to ONE case: a PHANTOM — a ' +
    'cited member that does not exist. An underutilized catalog, an orphan-looking admission, or a weak fence is an INTEGRATION ' +
    'target: the capability lands as ' +
    L.ownerGrammar +
    ' — inside the existing owner, reshaped as if always carried — or is ' +
    'wired into its owning sibling page in the same pass. Never a parallel type, a sibling shape, or flat appended code; never ' +
    'extract a file to cut LOC; never regress existing capability. A NEW page is admitted on exactly one ground: a genuinely ' +
    'new owner the domain demands that no existing owner can absorb, authored in the TARGET package and wired into the folder ' +
    'seam owners — never as extraction relief. Structural collapse and CAPABILITY completeness are orthogonal — a fully ' +
    'collapsed owner can still model a naive slice; close both. Every extension cites exactly one gap source. PACKAGE — a ' +
    'member the admitted surface exposes that the concept admits but the page ignores (' +
    L.gapPkg +
    '). DOMAIN — an ' +
    'attribute, metric, sub-kind, relationship, state, or operation the real concept demands (' +
    L.gapDomain +
    '). CONSUMER — ' +
    'a contract a sibling or downstream owner will require. A genuinely needed NEW external package is admitted additively: ' +
    'author its README registry row and its `.api` catalog yourself; its central-manifest row is reported in `indexRows`. ' +
    'Byte-count is a weak proxy: assess every owner against its full domain and both-tier package surface independently of ' +
    "size. Anticipate the FIVE-TIMES demand: model each owner for five times today's cases, fields, and consumers — a thin " +
    'slice built "for now" is the COVERAGE defect by definition. CHANNEL LAW — a canary/beta/pre-release channel is admissible ' +
    'wherever the bleeding edge genuinely adds capability: judged on capability delta, maintenance signal, and integration ' +
    'merit, never rejected for channel alone, pinned exact with the typing posture recorded in the catalog.';

const BODY = (L) =>
    'FENCE-BODY LAW — the interior of every fence is judged at the same bar as its shapes; a correct owner ' +
    'carrying a naive body is a defect. Rebuilt on sight: ' +
    L.body +
    '. The optimal body is dense, flat, ' +
    'expression-shaped, and reads as one algebra — the admitted combinator surface is the material, never hand-rolled ' +
    'control flow, nesting, or extraction to loose helpers.';

const VERIFY = (L) =>
    'VERIFY — cite only members confirmed via ' +
    L.verify +
    '; a member you cannot verify is a phantom to ' +
    'delete. Mine BOTH .api tiers to operator depth: ' +
    L.apiTiers +
    ' An admitted capability the concept admits that no ' +
    'owner exploits is a defect to close.';

const RIPPLE_LAW =
    'RIPPLE LAW — every fix you identify you make NOW via Edit/Write; the fix-log is a report of edits ' +
    'already made, never a to-do, a ledger, or a would/should hedge. The writing is YOURS: never delegate authoring to ' +
    'another agent — a delegate may only fetch information. Your ripple authority is LIBS-WIDE — any file under ' +
    'libs/, any language, corrective AND generative — under four bounds that are evidence, never radius. (1) EVIDENCE — an ' +
    'out-of-scope edit traces to a resolvable anchor: a seam-ledger row, a consumer anchor, an index claim, or a wire row in ' +
    'the branch ARCHITECTURE.md [02]-[SEAMS] ledger; an edit with no anchor is drift, forbidden. (2) EXPAND-FORM — a foreign ' +
    'edit made while sibling batches run is ADDITIVE only: add the case, row, field, operation, or counterpart; renaming, ' +
    'removing, or collapsing a foreign surface is recorded in `deferred` for the terminal fixer, never raced. Wire-canonical ' +
    'names stay frozen; a foreign-language counterpart is repaired at ITS branch doctrine bar (read that branch stack README ' +
    'before a non-trivial foreign edit) with surgical anchored edits, never a foreign-interior rebuild. (3) DEPTH — a ' +
    'first-order ripple (your edit broke or opened it directly) is repaired both ends in this pass and recorded in ' +
    "`seamsTouched`; a second-order ripple (exposed by a ripple repair) or a counterpart INSIDE a concurrent batch's scope " +
    'is recorded in `deferred` as {files, claim} — the fixer drains the backlog this run; nothing is silently dropped. ' +
    '(4) DECISION/PROPAGATION — decision-carrying shared surfaces are single-writer: the owning-package index docs ' +
    '(ARCHITECTURE.md + README.md at the path before `/.planning/`), IDEAS.md, and the central manifests take exact rows via ' +
    '`indexRows` for the terminal fixer to apply once; propagating an already-ruled contract is yours to distribute. ' +
    'GENERATIVE openings: a capability your work opens elsewhere — same language or not, same folder or not — is realized ' +
    'NOW when it lands in expand-form owner grammar on an existing owner; an opening that demands a new owner outside the ' +
    'target package lands as a fully-specified IDEAS row via `indexRows`, never a vague note.';

const CURRENT_STATE =
    'CURRENT STATE — sibling batches land work concurrently with yours. Before any edit, re-read the ' +
    'CURRENT on-disk state of your pages AND every sibling page your pages compose or ripple into; landed sibling work is ' +
    'picked up as found, never assumed from the dossier snapshot — the dossiers ground verified `.api` extracts, never ' +
    'sibling page state. A seam counterpart a sibling batch landed is COMPOSED, not re-derived; a conflict between your ' +
    'design and a landed sibling resolves to the stronger form, never a revert.';

const LEDGER = (base, scopes) =>
    'SEAM LEDGER — cross-batch coordination is typed fact rows on disk, never prose. Your ' +
    'batch ledger is `' +
    base +
    '-seams.md`: append one row per cross-file event as you work — ' +
    '`SEAM_CHANGED | <files> | <symbol/wire fact, old -> new>` when a shared name, signature, or contract you own moves; ' +
    '`RIPPLE_REPAIRED | <files> | <fact>` when you repair a counterpart, so no sibling redoes it; ' +
    '`SEAM_CONFLICT | <files> | <both values>` when your decision collides with a landed sibling row (then resolve to the ' +
    'stronger form per CURRENT STATE). Before ANY edit outside your batch pages, `ls` `' +
    SCRATCH +
    '/` and read every ' +
    'sibling `*-seams.md` row whose files intersect yours — a RIPPLE_REPAIRED row is work you do NOT redo; a SEAM_CHANGED ' +
    'row is a contract you compose. Rows are facts with zero adjectives. CONCURRENT BATCH SCOPES (a counterpart inside ' +
    "another live batch's scope is recorded in `deferred`, never edited): " +
    scopes;

const PROSE_COMMENTS = (L) =>
    'PROSE + COMMENTS — apply docs/standards/style-guide.md, information-structure.md, and ' +
    'formatting.md. The page is a design spec: lead each section with the controlling contract, one idea per paragraph, close ' +
    'on the consequence; no provenance, process narration, freshness disclaimers, or hedges. Backtick every symbol, type, ' +
    'field, function, operator, package ID, path, command, flag, and literal; name the exact member instead of paraphrasing ' +
    'behavior; trimming never reduces technical density. Code fences comment for the next agent only: keep the canonical ' +
    'section-divider headers; beyond them default to zero comments, 1-2 lines only for a truly subtle invariant, contract, or ' +
    'boundary; no restating the code, no narration, no ' +
    L.docBloat +
    ' bloat.';

const INFO_LAW =
    'You provide INFORMATION, never prescriptions: exact disk locations and anchors, the current shape at each ' +
    'site, seam endpoints on both sides, verified member spellings, gaps. The implement agent decides how to build; a worklist ' +
    'entry that tells it what to write instead of what is true is a defect. ENTRY FORM: prose fields carry fact; `anchors` ' +
    'carry one coordinate per row (role names what it proves; `note` is the shortest literal witness under 20 words, or empty ' +
    'when path+line suffice; an `absence` anchor names where the expected thing was searched and not found); `files` lists ' +
    'what the consumer must open for the entry. An underutilized-capability entry is INVENTORY, never instruction: verified ' +
    'members, current usage anchors, the concept that admits it — the implement agent decides whether it composes. COVERAGE ' +
    'is part of the product: `requested` = your assigned scope, `read` = what you actually full-read, `skipped`/`unverified` ' +
    '= what you did not reach — an honest skip beats a silent one.';

const EVIDENCE_LAW =
    'FINDING FORM — you deliver TRUTH, never an implementation: `claim` states the observed defect; ' +
    '`mechanism` states WHY it fails the law or doctrine as fact; `anchors` carry one coordinate per row (role names ' +
    'what the coordinate proves; `note` is the shortest literal witness — a symbol, member spelling, or fragment under ' +
    '20 words — or empty when path+line suffice; an `absence` anchor names where the expected thing was searched and not ' +
    'found); `owner` names the canonical owner that must absorb the resolution (the owning axis, row roster, registry, or ' +
    'seam vocabulary — never a new local shape); `reject` lists the deleted forms the repair must not take; `acceptance` ' +
    'lists the signals that prove resolution. NEVER write add/replace/implement/promote/delete as instruction — the ' +
    'writer owns the design; you own the constraint boundary. `claimKey` is identical for the same defect regardless of ' +
    'lane or wording. `severity` binds to consequence: blocker = run-blocking, major = corpus correctness, minor = local ' +
    'cleanup — never prose confidence. OUTPUT BOUNDS: an ordinary scope yields 3-8 retained findings; 0 only when the ' +
    'second hostile pass comes back empty, and then `summary` names the probes that produced nothing; never manufacture a ' +
    'finding to fill the range, never delete a confirmed one to stay inside it. COVERAGE is part of the product: ' +
    '`requested` = your assigned scope, `read` = what you actually full-read, `skipped`/`unverified` = what you did not ' +
    'reach or could not confirm — an honest skip beats a silent one.';

const HARVEST_LAW =
    'HARVEST (required key, usually empty): nominate ONLY findings that generalize beyond this batch — a collapse pattern reusable ' +
    'across folders, a naivety class no doctrine clause names, a review rule that would have caught a defect BEFORE review, a ' +
    'cross-surface coupling discovered the hard way. Each row: altitude (stacks|reviewer|constitution|planning|readme|laws), lang, ' +
    'claim (the generalized law, one sentence), anchors (file:line evidence), existingClause (the exact doctrine or reviewer clause ' +
    'it would harden, quoted with its path — or "absent" plus the surfaces searched). A batch-local fix never nominates; an empty ' +
    'array is the normal verdict — the terminal doctrine lander refutes weak rows, so nominate substance, never volume.';

// The four-input ladder for every batch writer, in binding order: (1) OWN blind hostile pass — the primary product; (2) the map dossiers —
// two-tier .api stacking + context grounding; (3) the corrections census — additional fixes, never the plan; (4) the bigger-ideas worklist —
// ambition beyond. OWN_PASS is the ladder's law; CORRECTIONS and IDEAS carry rungs 3 and 4.
const OWN_PASS =
    'OWN PASS FIRST — the input ladder is binding, in this order: (1) your own blind hostile pass, (2) the map dossiers, (3) the ' +
    'corrections census, (4) the bigger-ideas worklist. Rung (1) is the PRIMARY product and the mandate: cold-read every target ' +
    'page from CURRENT disk and derive your own defect list, collapse targets, naivety kills, body rebuilds, and design rulings ' +
    'BEFORE opening any recon report, census, or worklist. Rungs (2)-(4) are secondary inputs that ground, extend, and widen YOUR ' +
    'pass — they never scope it, never substitute for it, never cap it. TRIPWIRE: a pass whose diff maps one-to-one onto the recon ' +
    'rows has failed this mandate — the recon covers a MINORITY of what the rebuild demands, and the majority of your edits must ' +
    'come from your own attack: collapses, naivety kills, body rebuilds, and capability closure no report named.';

const CORRECTIONS = (path) =>
    path
        ? 'CORRECTIONS CENSUS (ladder rung 3) — `' +
          path +
          '` carries the folder-wide fix census consolidated from the map lanes, sectioned per sub-folder: drift, phantoms, ' +
          'catalog-true spelling repairs, seam and wire mismatches, wiring gaps. ADDITIONAL, never the plan: after your own pass, ' +
          'land every row that intersects your pages (re-verified on disk — a row disk already resolves is dropped) and leave ' +
          'foreign rows alone; the terminal fixer drains the remainder. A pass that only lands census rows has failed OWN PASS FIRST.'
        : '';

const IDEAS = (path) =>
    path
        ? 'BIGGER-IDEAS WORKLIST (ladder rung 4) — `' +
          path +
          '` carries per-sub-folder capability AMBITIONS: new dimensions, modalities, families, and operations the domain admits ' +
          'BEYOND correction. Read the entries covering your pages IN FULL, spot-verify each against current disk, and build the ' +
          'STRONGEST form it points toward or a stronger one you see. An idea disk already realizes, or the doctrine forbids, is ' +
          'dropped; an idea you decline is not a defect. Ambition and information, never a prescription, a design, or a ceiling.'
        : '';

const readFirst = (L, pkg, dossiers) =>
    [
        'READ FIRST, IN ORDER, BEFORE ANY EDIT — no fence is judged before this read lands.',
        '(1) DOCTRINE — enumerate `' +
            L.stack +
            '/` with a real `ls` (never memory), then read the README and EVERY root page it ' +
            'routes IN FULL in the README [01]-[ATLAS] order — top-to-bottom, never a partial, skim, grep-jump, or section-sample; a ' +
            'root page on disk but absent from the atlas is still mandatory law. The README [02]-[DOCTRINE] laws, the ' +
            '[03]-[COLLAPSE_SCAN] table, OWNER_CHOOSER (`shapes.md` [01]), RAIL_CHOOSER (`rails-and-effects.md` [01]), and the aspect ' +
            'two-weave (`surfaces-and-dispatch.md` AND `rails-and-effects.md` — both owners) are binding law AT THE SOURCE. This ' +
            'prompt does not restate the doctrine; read it there, hold it as fact, and conform every fence to it — a summary is never ' +
            'a substitute for the read.',
        L.key === 'cs'
            ? '(1b) Enumerate `docs/stacks/csharp/domain/` with a real `ls` through its router README, then read every ' +
              'shard the page concerns touch — chosen from the enumerated set, truthfully, never from memory or skipped; shard ' +
              'conformance is a hard gate.'
            : '',
        '(1c) ANALYZER LAW — read the repo `.editorconfig` rules for your language: every rule at `error` severity is a COMPILE ' +
            'GATE the fences must satisfy (`dotnet_style_namespace_match_folder = true:error` means namespace ALWAYS equals folder ' +
            'path — a namespace matrix, mapping table, or doc claim that contradicts an error-level analyzer rule is a FICTION to ' +
            'correct, never law to compose).',
        '(1d) LAWS — read `docs/laws/` IN FULL (README + topology + patterns + scars; short registry pages): a topology row whose ' +
            '[SURFACE] your edits touch binds its obligated counterparts into the SAME pass, and every patterns row binds each ' +
            'branch it names.',
        '(2) .API — `ls` BOTH catalog tiers in full — the shared substrate `' +
            L.root +
            '/.api/` AND the folder `' +
            pkg +
            '/.api/` — then read every catalog relevant to these pages, layering the shared rails (' +
            L.deepPkgs +
            ') ON TOP OF the ' +
            'folder domain packages, never the folder set alone.',
        dossiers
            ? 'The grounding dossiers for this batch — `' +
              dossiers +
              '` — carry verified two-tier extracts in two lanes: ' +
              'Tier-1 verbatim member/seam extracts with `file:line` anchors (read fully; SPOT-VERIFY the anchors — a fake anchor goes ' +
              'in `dossierPhantoms`), and Tier-2 pointer rows (path + one-line scope) for the long tail — resolve a pointer with a ' +
              'real read the moment an edit touches its territory, never guess past it. Hunt PAST both lanes — members you compose ' +
              'beyond them are enumerated in `beyondMap`. Absent or stale, run the full two-tier `ls`+read yourself.'
            : '',
        '(3) SCOPE — read the owning-package charter — ARCHITECTURE.md + README.md + IDEAS.md — as the INTENT authority for what ' +
            'each page owns and which pages are settled. A page the charter marks landed or settled is out of scope; every page in ' +
            'your batch is rebuilt to the strongest form the doctrine admits.',
    ]
        .filter(Boolean)
        .join('\n');

const reconBlock = (roster, unmapped) =>
    'RECON REPORTS (ladder rung 2) — the lens products are ON DISK as JSON report files; the ' +
    'receipts below are navigation, never the product. CONSUMPTION, after your own cold pass per OWN PASS FIRST: (a) UNMAPPED ' +
    'territory below (a dead lens) gets your own ' +
    'cold read — that lens dimension over your pages is yours to derive; (b) read every ok report IN FULL from disk — ' +
    'the ctx and api grounding lenses before the bar defect lens; entries overlap across lenses, cluster by page as you ' +
    "read; (c) each entry's anchors are jump coordinates — re-open every anchor behind an edit (MANDATORY); " +
    "navigation-only entries re-verify only when touched; (d) a bar finding's `mechanism`/`owner`/`reject`/`acceptance` are " +
    'its constraint boundary — honor the owner and the rejected forms, but the DESIGN is yours. The reports POINT; you ' +
    'VERIFY and EXCEED them: compose every `apiUsed` catalog at full operator depth, stack every `apiUnderutilized` ' +
    '{catalog, capability} INTO the owning page as a case, row, field, or operation, close every bar finding at its law, ' +
    'and independently confirm no other relevant admitted catalog (either tier) is missing. Members you compose beyond the ' +
    'reports are enumerated in `beyondMap` — an empty `beyondMap` is an attestation that the reports were genuinely ' +
    'complete, not a license to treat them as a ceiling.\nROSTER: ' +
    JSON.stringify(roster) +
    '\nUNMAPPED: ' +
    JSON.stringify(unmapped);

const GIT_GROUND =
    'DELTA GROUNDING — run `git diff --stat` then `git diff -- <your batch pages and their seam files>` to ' +
    'see exactly what this run changed before judging it; `git status` surfaces new files. The diff is orientation, CURRENT ' +
    'disk is truth — the repo carries pre-run uncommitted work, so an unfamiliar hunk is verified against disk, never assumed ' +
    "to be this run's.";

const HUNT =
    'HUNT CLASSES: missing (an owner, case, field, seam counterpart, or capability the charter or the landed design ' +
    'demands with no counterpart on disk), wrong (landed but contradicting the doctrine, the charter, or the analyzer law), ' +
    'faked (claimed done — prose asserts what the fence body omits, a name promising capability the body lacks), naive (landed ' +
    'thin — a slice of the concept, an underutilized admitted package, either naivety axis per the corpus stance), drift (two ' +
    'landed surfaces disagreeing — page vs sibling vs index doc vs manifest vs .api), phantom (a cited member, page, or anchor ' +
    'that does not exist). Every finding carries a file anchor, names the law or catalog member it violates, and is graded: ' +
    '`substantive` (a concrete defect on current disk) or `hypothetical` (real only under an invented, implausible input). ' +
    'Verify cited external members against the .api catalogs; never trust page prose about itself.';

const preamble = (L, batch, dossiers, ideate, scopes, roster, unmapped, reg) =>
    [CONTEXT(L), REG[reg].stance(L), OWN_PASS, BUILD_LAW(L), BODY(L), VERIFY(L), RIPPLE_LAW, CURRENT_STATE, PROSE_COMMENTS(L)]
        .concat(L.mechanics ? [L.mechanics] : [])
        .concat([
            readFirst(L, pkgOf(batch[0].page), dossiers),
            LEDGER(scratchBase(pkgOf(batch[0].page), batch[0].i || 0), scopes),
            reconBlock(roster, unmapped),
        ])
        .concat(ideate && ideate.fix ? [CORRECTIONS(ideate.fix)] : [])
        .concat(ideate && ideate.idea ? [IDEAS(ideate.idea)] : []);

// Prompt builders — each task states only its own action; shared checks are referenced by name.
const planPrompt = () =>
    [
        ROOT_LAW,
        'Rasm monorepo — the libs/{csharp,python,typescript} planning corpora (markdown design specs). ' +
            "Targets may mix languages; each page's owning package derives its own doctrine downstream.",
        'TASK: thin enumerate + classify (read-only, do NOT edit). TARGETS (repo-relative): ' +
            JSON.stringify(TARGETS) +
            '. The ' +
            'OWNING PACKAGE of a page is the path before `/.planning/`. EXPAND with a real recursive listing per target — run ' +
            'find <target-or-its-.planning-tree> -name *.md — a design page lives INSIDE the .planning tree, so a package-root ' +
            'ls alone NEVER proves an empty page set. Validate the expansion against ' +
            '`libs/.planning/planning-targets.md` (a mis-scoped or renamed target is reported in `unresolved`, a deliberately ' +
            'page-less target is skipped silently). Return `packages` (one entry per distinct owning package: {name, root, planning, ' +
            'api}). PAGES: expand each target — a ROOT to every design page under its planning tree, a SUB-FOLDER to every page under ' +
            'it, a FILE to itself; union + dedup; exclude IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md.',
        'SCOPE LAW — the owning-package charter (ARCHITECTURE.md + README.md + IDEAS.md) owns scope: every existing design page ' +
            'under the targets enters as kind `rebuild`; a page the charter demands but disk lacks enters as kind `new`; a page the ' +
            'charter marks landed or settled is SKIPPED — excluded from the page list, never re-litigated.',
        'EMIT `pages` IN DEPENDENCY + COHESION ORDER — grouped by sub-folder, foundations before their consumers, and pages that ' +
            'share an owner, a seam, or a wire contract ADJACENT within their group (the engine batches contiguous runs of your ' +
            'emitted order, so adjacency keeps coupled pages inside one writer); alphabetical only as the final tiebreak. The engine ' +
            'never re-sorts.',
    ].join('\n\n');

// Ideate runs per owning package as TWO lanes with disjoint charters: the corrections census (the fix addendum, ladder rung 3) and the
// bigger-ideas worklist (the ambition, ladder rung 4) — merged into one log they collapse into a fixlog and the ambition dies.
const correctionsPrompt = (L, pkg, mapIndex, dossier) =>
    [
        ROOT_LAW,
        CONTEXT(L),
        'TASK: CORRECTIONS CENSUS AUTHOR for `' +
            pkg +
            '` — read-only over the corpus; you WRITE exactly one file, the census dossier. The Map phase produced per-SUB-FOLDER ' +
            'deep-map and two-tier .api inventory dossiers; read EVERY dossier listed here IN FULL: ' +
            JSON.stringify(mapIndex),
        'AUTHOR `' +
            dossier +
            '` — one markdown census, a section per sub-folder. Consolidate every CORRECTION the map lanes surfaced or your own ' +
            'verification finds: drift between surfaces (page vs sibling vs index doc vs manifest vs .api), phantom members, ' +
            'catalog-true spelling repairs, seam and wire mismatches, contradicted analyzer law, and WIRING GAPS — an uncomposed ' +
            'admitted member the page concept plainly demands. Each row carries the owning page, the exact `file:line` anchor, the ' +
            'defect as fact, and the catalog or doctrine coordinate that proves it. DEDUPE across dossiers (one row per defect); ' +
            'VERIFY each row on current disk before writing it — a row disk already resolves is dropped. FORBIDDEN: new-capability ' +
            'ambitions (the ideas lane owns them — a row that widens what the package IS gets dropped here, not diluted into a fix), ' +
            'prescriptions, fence sketches, removal framing.',
        REG.claude.selfCheck,
        'Return ONLY the receipt: ok, report = the census path (repo-relative), entries = the row count, a one-line headline, failure empty.',
    ].join('\n\n');

const ideasPrompt = (L, pkg, mapIndex, dossier) =>
    [
        ROOT_LAW,
        CONTEXT(L),
        'TASK: BIGGER-IDEAS AUTHOR for `' +
            pkg +
            '` — read-only over the corpus; you WRITE exactly one file, the ideas dossier. The Map phase produced per-SUB-FOLDER ' +
            'deep-map and two-tier .api inventory dossiers; read EVERY dossier listed here IN FULL, then the package charter ' +
            '(ARCHITECTURE.md + README.md + IDEAS.md) and the pages your ideas grow on: ' +
            JSON.stringify(mapIndex),
        'AUTHOR `' +
            dossier +
            '` — one markdown dossier, a section per sub-folder plus a terminal CROSS-FOLDER section. Every entry is a NEW ' +
            'capability the domain admits and the corpus lacks: a new dimension, modality, family, case class, operation family, ' +
            'generator over an enumerated space, or cross-boundary enablement — an extension that widens what the package IS. ' +
            'Ground each in real domain demand (' +
            L.gapDomain +
            ') and the admitted two-tier package depth. VALUE BAR — the census/ideas partition is severity of imagination, not ' +
            'anchor quality: a stale label, a wrong spelling, a dropped wire column, or a single uncomposed member is a CORRECTION ' +
            'and belongs to the census lane — drop it here; an entry earns its row by naming capability whose absence a domain ' +
            'expert would call a gap in the PRODUCT, not a defect in the prose. Name the owner it grows on, the domain or catalog ' +
            'ground, the anchor, and WHY it widens the package — never the resulting code, a fence sketch, or a ruled shape. ' +
            'CROSS-FOLDER: for every idea whose value crosses the package boundary, name BOTH ends and the seam it rides.',
        "SECOND-PASS CULL (before returning): re-open every entry's anchor on disk; delete any entry disk already realizes, any " +
            'correction in disguise, and any entry whose value you cannot state as a concrete new capability of a named owner. ' +
            'Boldness is never the cull criterion — the cull removes false and small entries, never ambitious ones; a dossier of ' +
            'few large ideas beats a dossier of many verified trivia.',
        REG.claude.selfCheck,
        'Return ONLY the receipt: ok, report = the dossier path (repo-relative), entries = the idea count, a one-line headline, failure empty.',
    ].join('\n\n');

const ctxLensPrompt = (L, batch, dossier, reg) =>
    [
        CONTEXT(L),
        REG[reg].stance(L),
        INFO_LAW,
        REG[reg].selfCheck,
        REG[reg].antiAnchor(L),
        readFirst(L, pkgOf(batch[0].page), ''),
        REG[reg].ctx(batch.length) +
            batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') +
            '. For a rebuild page read the page IN FULL; for a `new` page ' +
            'read its concept in the owning-package charter plus its nearest sibling pages. Read the folder at large — the sibling ' +
            'pages each composes and the owning-package index docs — as full-file reads. For EACH page return: `owns` (the ONE ' +
            'ownership boundary sentence — which owner/vocabulary/concern THIS page owns versus its siblings, so no two concurrent ' +
            'writers author the same polymorphic surface), `contextNote` (sibling owners/seams composed, folder position, any ' +
            'folder-wide gap routed here), `seams` (every cross-page and cross-package symbol/wire/consumer edge, both endpoints ' +
            'named), and DOMAIN gaps — attributes, sub-kinds, states, relationships, operations the real concept demands that the ' +
            'page omits — folded into `contextNote` as named gaps. CROSS-PACKAGE RELEVANCE: for each page also mine what the OTHER ' +
            'packages hold that is relevant to it — the kernel and sibling-package owners it composes or its concept plainly touches, ' +
            'the imports and consumer sites, and every ripple target on both ends — as verified anchors in `seams`/`anchors`, so a ' +
            'writer NAVIGATES (trust, then verify at the anchor) instead of exploring; relevance is fact, never a suggested change. ' +
            'Each worklist entry also carries `files` (what the consumer must ' +
            'open) and typed `anchors` per the entry form. GROUNDING DOSSIER: write `' +
            dossier +
            '` — Tier-1: the branch ' +
            'ARCHITECTURE.md [02]-[SEAMS] rows covering these pages quoted verbatim with `file:line` anchors, folder-context anchors, ' +
            'charter intent anchors; Tier-2: pointer rows (path + one line) for every sibling page composed. FORBIDDEN in the ' +
            'dossier: doctrine digests, removal framing, unanchored claims, any prescriptive design. Return worklist + coverage.',
    ].join('\n\n');

const apiLensPrompt = (L, batch, dossier, reg) =>
    [
        CONTEXT(L),
        REG[reg].stance(L),
        VERIFY(L),
        INFO_LAW,
        REG[reg].selfCheck,
        REG[reg].antiAnchor(L),
        REG[reg].api(batch.length) +
            batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') +
            '. `ls` BOTH catalog tiers in full — the shared substrate `' +
            L.root +
            '/.api/` AND the folder `' +
            pkgOf(batch[0].page) +
            '/.api/` — read every catalog relevant to these pages, and ' +
            'DIFF the complete admitted inventory against the whole folder: ' +
            REG[reg].apiVerify +
            ' — a capability no page exploits is a ' +
            'named integration gap ROUTED to EVERY page whose concept admits it, never one "best" owner alone. SINGLE-CONSUMER ' +
            'EXPANSION: a package with a catalog at ANY tier consumed by only ONE page is expansion pressure on its siblings — name ' +
            'the package, its unexploited members in exact spellings, and each candidate page. Discovery has ZERO removal authority: ' +
            'an underutilized catalog is always a buildout target (which owner grows which case/row/field/operation), never removal ' +
            'evidence. DEPTH GRADING: a member the page already composes counts as underutilized when the usage is shallow — one ' +
            'call where the surface carries a family, a default-arg call where the policy axis matters, a scalar use of a batch/' +
            'stream-capable member; grade used-but-shallow with the same {catalog, capability} rows as unused. For EACH page ' +
            'return `apiUsed`, `apiUnderutilized` ({catalog, capability}: exact catalog-anchored spelling + ' +
            "integration shape as fact), `stackingInventory` (capability names + the doctrine patterns the page's concept admits, " +
            'stated as inventory fact — never a prescribed design), plus `files` (what the consumer must open) and typed `anchors` ' +
            'per the entry form. Verify every cited member via ' +
            L.verify +
            '; never list a phantom. ' +
            'GROUNDING DOSSIER: write `' +
            dossier +
            '` — Tier-1: quoted `.api` member blocks with `file:line` anchors for every ' +
            'member the worklist cites plus the real `ls` inventories of both tiers; Tier-2: pointer rows (catalog path + one-line scope) ' +
            'for the remainder of both tiers. FORBIDDEN: doctrine digests, unanchored claims, prescriptive designs. Return worklist + ' +
            'coverage.',
    ].join('\n\n');

const barLensPrompt = (L, batch, reg) =>
    [
        CONTEXT(L),
        REG[reg].stance(L),
        EVIDENCE_LAW,
        REG[reg].selfCheck,
        REG[reg].antiAnchor(L),
        REG[reg].bar(batch.length) +
            batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') +
            '. Enumerate `' +
            L.stack +
            '/` with a real `ls` and read the ' +
            'README and EVERY root page it routes IN FULL' +
            (L.key === 'cs' ? ' (and the domain/ shards these pages touch)' : '') +
            '; then read each target page IN FULL and ' +
            REG[reg].barAttack(L) +
            ' where doctrine is not followed AND where a doctrine law ' +
            'could be applied more deeply for a stronger form: collapse signals ungathered, owner forms weaker than the discriminants ' +
            'demand, rails split or dual-paradigm, knobs where policy values belong, naive bodies below the admitted combinator ' +
            'surface, ' +
            L.docBloat +
            ' bloat, file-organization drift, both naivety axes. Return per-page `findings` in the ' +
            'FINDING FORM — `law` names the doctrine law/pattern at its source, `claimKey` = <law>|<owner>|<primary symbol>, typed ' +
            '`anchors` at exact coordinates — and `weak` (pages whose overall verdict is weak). Findings name the law and the ' +
            'defect, NEVER the resulting code — the implement agent rules every design.',
    ].join('\n\n');

const implementPrompt = (L, batch, dossiers, ideate, scopes, roster, unmapped) =>
    preamble(L, batch, dossiers, ideate, scopes, roster, unmapped, 'claude')
        .concat([
            'TASK: HOSTILE IMPLEMENT of these ' +
                batch.length +
                ' pages IN PLACE, each per its kind: ' +
                batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') +
                '.\n' +
                'kind=`new`: GROUND-UP AUTHOR the page (it does not exist; it may open a new sub-folder) to the full doctrine + ' +
                'domain-complete capability bar, in the code-fence-first design-page form of its mature siblings, wired into the folder ' +
                'entry/receipt seam owners where the folder has them. kind=`rebuild`: HOSTILE GROUND-UP REBUILD in place. Before ' +
                'authoring EACH page, restate in one line the owner it holds, the seams and frozen wire names it must honor, and the ' +
                'doctrine laws that bind it — then build against that restatement. Construct in LIFECYCLE order — admit raw once, ' +
                'canonical owner by OWNER_CHOOSER, stacked rail/aspect over a thin pure core, projection, egress, BOTH ingress and ' +
                'egress parameterized; collapse parallel shapes into ' +
                L.collapseInto +
                '; one polymorphic entrypoint per modality. ' +
                "COMPOSE the reports' `apiUsed` at full operator depth, STACK every `apiUnderutilized` into the owner, CLOSE every " +
                'bar finding at its law, and CONFIRM no other admitted catalog is missing. CLOSE the concept capability gaps per BUILD ' +
                'LAW. Then work the remaining ladder rungs: land every CORRECTIONS CENSUS row intersecting your pages, and realize the ' +
                'BIGGER-IDEAS entries your pages own at the strongest form disk admits. ' +
                L.modern +
                '; ' +
                L.fileOrg +
                '; high-signal all-backticked prose. Return the fix-log — `deltas` carries every ' +
                'moved symbol/wire as data, `deferred` carries the backlog rows, both exact. ' +
                HARVEST_LAW,
        ])
        .join('\n\n');

const critiquePrompt = (L, batch, dossiers, ideate, scopes, roster, unmapped, nav, reg) =>
    preamble(L, batch, dossiers, ideate, scopes, roster, unmapped, reg)
        .concat([
            'NAVIGATION (facts from the pass that landed these pages — locations only, no assessments; it changes where you look ' +
                'FIRST, never what you conclude): ' +
                JSON.stringify(nav),
            GIT_GROUND,
            REG[reg].audit +
                batch.map((p) => p.page).join(', ') +
                '. FORM YOUR OWN DEFECT LIST FIRST — read each page cold from CURRENT disk and ' +
                'derive your findings before consulting NAVIGATION; then use the navigation to reach every touched seam fast. Audit ' +
                'every fence against the doctrine you read at source, never a summary; repair every hit now — a fix, never a ledger ' +
                'note; a cross-file hit is yours per RIPPLE LAW. Your mandate is PREDICATE-POSITIVE: verify each required law holds and ' +
                'cite the clause; every miss is repaired.\n' +
                '- COLLAPSE_SCAN: run the README [03] table on every fence — any signal triggers the move; shapes sharing an identity ' +
                'regime, an admission path, a payload timing, or a consumer collapse into ONE owner, and a shape survives only on a ' +
                'genuinely distinct discriminant; the table is a FLOOR you hunt past.\n' +
                '- OWNER_CHOOSER (`shapes.md` [01]): re-derive every shape from the 5 discriminants — admission, identity regime, ' +
                'variant arity, payload timing, openness — and replace any non-discriminant-correct owner; kill every parallel DTO, ' +
                'one-field wrapper, field-rename shape, and null/default ghost.\n' +
                '- KNOB_TEST: delete each parameter — where the value reconstructs it, collapse the knob to a policy value or ' +
                'input-shape discriminant; move every timeout/retry/deadline off the signature onto the carrier or a composition-time ' +
                'aspect.\n' +
                '- ASPECTS (`surfaces-and-dispatch.md` AND `rails-and-effects.md` — both owners), RAILS + closed-fault + ' +
                'accumulate-vs-abort (`rails-and-effects.md` [01]), STRATA/MEMBERS (' +
                L.modern +
                '; both .api tiers maximized per ' +
                'VERIFY; ' +
                L.fileOrg +
                '): audit each at its owning page.\n' +
                '- SEAM ALIGNMENT: every cross-page symbol the batch composes is checked against the sibling owner as it NOW stands on ' +
                'disk — a counterpart a sibling batch landed is composed, a signature mismatch corrects at the weaker end, a conflict ' +
                'resolves to the stronger form, never a revert.\n' +
                '- CAPABILITY-COMPLETENESS + ILLUSION: verify the body implements what names and prose promise; close any admitted ' +
                'capability the owner omits by growing it per BUILD LAW; attack both naivety axes.\n' +
                'Return the batched fix-log — `deltas` and `deferred` exact. ' +
                HARVEST_LAW,
        ])
        .join('\n\n');

const redteamPrompt = (L, batch, dossiers, ideate, scopes, roster, unmapped, nav, crit) =>
    preamble(L, batch, dossiers, ideate, scopes, roster, unmapped, 'claude')
        .concat([
            'NAVIGATION (locations only, no assessments): ' + JSON.stringify(nav),
            crit && crit.ok
                ? 'PRIOR CLAIMS (UNVERIFIED): the sol critique fixlog is ON DISK at ' +
                  crit.report +
                  ' — read it IN FULL from disk; its edits and verdicts are refutation targets you judge against CURRENT disk, never ' +
                  'a settled record. FOLD-FORWARD DUTY: its surviving `seamsTouched`, `deltas`, `deferred`, `beyondMap`, `indexRows`, ' +
                  'and `harvest` rows are folded into YOUR return (re-verified against current disk, deduped) — your fix-log is the ' +
                  "batch's consolidated record; a dropped critique row is a silent loss."
                : 'PRIOR CLAIMS: the critique lane did not land — your cold attack is the only review this batch gets; judge from ' +
                  'CURRENT disk alone.',
            GIT_GROUND,
            'TASK: ADVERSARIAL ARCHITECT RED-TEAM; fix EACH page in place: ' +
                batch.map((p) => p.page).join(', ') +
                '. Assume the ' +
                'author and critique missed things and their claims above are wrong until disk proves them. FORM YOUR OWN ATTACK FIRST ' +
                '— cold-read each page from CURRENT disk before consulting the claims. Your mandate is PREDICATE-NEGATIVE — a ' +
                'pre-mortem, not a second conformance audit:\n' +
                '(A) COUNTERFACTUAL on the core owner/algebra/dispatch — does a denser owner (' +
                L.collapseInto +
                '), a derived table, ' +
                'a parameterized generator over the enumerated space, or a deeper admitted-package primitive (' +
                L.deepPkgs +
                ') ' +
                'collapse the whole fence? A fundamentally stronger design is built, never defended against. (B) ANTICIPATORY_COLLAPSE ' +
                '— compute the diff of the next feature: the next case/dimension/modality lands as one row with every consumer ' +
                'untouched or loudly broken (' +
                L.exhaust +
                '). (C) LONG-TAIL — empty/singular/plural/stream/malformed/concurrent/' +
                'cancelled/partial-failure/version-skew; accumulate-vs-abort correct for the real boundary; ingress AND egress ' +
                'parameterized. (D) BOUNDARY/STRATA — grade every concern against `libs/.planning/architecture.md` and the branch ' +
                'ARCHITECTURE.md [02]-[SEAMS] ledger (read the ledger, never a summary of it): a concern owned twice, a downward ' +
                'dependency, a host-type leak, or coupling to a sibling interior is a defect fixed both ends per RIPPLE LAW. (E) SPRAWL ' +
                '+ PHANTOMS — hand-re-derived package capability, flat code below the operator depth the packages reach, a phantom ' +
                'member (delete), a thin wrapper; and the inverse: an edit this run made that ADDED surface where the doctrine demands ' +
                'collapse is regression you rebuild denser. (F) CAPABILITY-COMPLETENESS + ILLUSION per STANCE and BUILD LAW. (G) ' +
                "GENERATIVE — the capability this batch's work opens at other levels or languages per RIPPLE LAW: realize owner-grammar " +
                'openings now, land new-owner openings as fully-specified IDEAS rows via `indexRows`. Then a FULL COLD RE-REVIEW of ' +
                'every conformance dimension by name — COLLAPSE_SCAN, OWNER_CHOOSER, KNOB_TEST, ASPECTS, RAILS, ' +
                L.modern +
                ', ' +
                L.fileOrg +
                ', both-tier .api maximization, prose + comment hygiene — each judged against CURRENT disk. VERIFY every ' +
                'PRIOR CLAIMS seam landed on BOTH ends; make any missed repair yourself. Return the batched fix-log — `deltas` and ' +
                '`deferred` exact. ' +
                HARVEST_LAW,
        ])
        .join('\n\n');

const finderPrompt = (L, pages, i, seams, reg) =>
    [
        CONTEXT(L),
        HUNT,
        EVIDENCE_LAW,
        REG[reg].selfCheck,
        GIT_GROUND,
        REG[reg].finder(i) +
            ' The run just landed a hostile rebuild over ' +
            'these pages: ' +
            JSON.stringify(pages) +
            '. ' +
            REG[reg].finderStance +
            'Read each page IN FULL fresh from CURRENT disk, plus the sibling owners each composes and every .api catalog its fences ' +
            'cite (both tiers where cited) — understand what the run changed before judging it. Hunt the classes above across the ' +
            'slice. SMOOTHING: additionally hunt cross-page duplication and split-brain — two pages (or a page and a sibling owner) ' +
            'modeling one concept in parallel shapes, a concern split across pages that one owner should hold, or a collapse the run ' +
            'left ungathered — each a `drift` or `naive` finding routed to the canonical owner. SEAM SIGNALS (rows the run reported — ' +
            'verify BOTH ends of each on current disk; an end missing on disk is a ' +
            'finding): ' +
            JSON.stringify(seams) +
            '. STALE DISCARD: judge only CURRENT disk — a defect already resolved on disk, at ' +
            'either end of a seam, is DROPPED, never reported. Findings are INFORMATION for the terminal fixer, never prescriptions: ' +
            'name the defect, the law or catalog member it violates, the exact file anchor, and the grade — never the resulting ' +
            'code, a fence sketch, or a ruled design. Return typed anchored graded findings.',
    ].join('\n\n');

const govFinderPrompt = (L, pkgs, pages, rows, reg) =>
    [
        CONTEXT(L),
        HUNT,
        EVIDENCE_LAW,
        REG[reg].selfCheck,
        REG[reg].gov +
            " Audit the owning packages' index surface end to " +
            'end for these packages: ' +
            JSON.stringify(pkgs) +
            " — each package's README.md and ARCHITECTURE.md (at the path " +
            'before `/.planning/`), its central manifest rows, and its `.api/` anchors — against the landed page set: ' +
            JSON.stringify(pages) +
            '. A disagreement between any two surfaces is a `drift` finding; a claim about a landed page is ' +
            'verified against the page on CURRENT disk, never against the claim. PENDING INDEX ROWS — the terminal fixer applies ' +
            'these after you; a gap these rows already close is DROPPED, not reported: ' +
            JSON.stringify(rows) +
            '. Return typed ' +
            'anchored graded findings.',
    ].join('\n\n');

const fixerPrompt = (langs, roster, unmapped, rows, backlog, failed, pages, orphans, census, round) =>
    [
        ROOT_LAW,
        round
            ? 'DRAIN ROUND ' +
              round +
              ' — every backlog row below was verified STILL-OPEN by the prior round; the index rows, finder reports, and orphan ' +
              'fixlogs are already consumed. Fix each row at its root NOW; a row you genuinely cannot land carries its named ' +
              'blocker and owner in `remaining`.'
            : '',
        'Rasm monorepo — the libs/ planning corpora. Per-language doctrine bars:\n' +
            langs
                .map(
                    (k) =>
                        '- ' +
                        LANG[k].name +
                        ': ' +
                        LANG[k].stack +
                        '/ read at source; ' +
                        LANG[k].modern +
                        '; ' +
                        LANG[k].fileOrg +
                        '; verify members via ' +
                        LANG[k].verify +
                        '.',
                )
                .join('\n'),
        HUNT,
        GIT_GROUND,
        "TASK: TERMINAL FIX (WRITER — you are the run's LAST agent, nothing follows you; full write authority over the landed " +
            'corpus and libs-wide ripple authority with the expand-form bound LIFTED — collapse, rename, and contract are yours now ' +
            "that no sibling writer runs; and you are the run's SOLE writer for the owning-package index docs (ARCHITECTURE.md + " +
            'README.md), IDEAS.md, and the central manifests). Landed pages: ' +
            JSON.stringify(pages) +
            '.\n' +
            '(1) INDEX ROWS: apply every reported row below to its owning doc exactly once — dedupe semantically identical rows, ' +
            "keep each doc's section grammar, verify every page landed this run is truthfully reflected; a central-manifest row " +
            'hand-edits the grouped manifest at the SYMBOL anchor (never a line number), preserving label-group order; an IDEAS row ' +
            'lands as a fully-specified card in the named IDEAS.md: ' +
            JSON.stringify(rows) +
            '.\n' +
            '(2) DEFERRED BACKLOG (second-order and cross-batch ripples the writers recorded — drain it: re-verify each {files, ' +
            'claim} on current disk, fix what holds, reject what disk already resolved): ' +
            JSON.stringify(backlog) +
            '.\n' +
            "(2b) ORPHANED CRITIQUE FIXLOGS (batches whose redteam never landed, so these on-disk fixlogs' seamsTouched/deferred/" +
            'indexRows/harvest rows were never folded forward — read each IN FULL from disk, drain the seam/deferred/index rows ' +
            "under the same law, and fold each fixlog's surviving harvest rows into your own `harvest` return, re-verified against " +
            'current disk and deduped): ' +
            JSON.stringify(orphans) +
            '.\n' +
            '(2c) CORRECTIONS CENSUS dossiers (the per-package fix census the batch writers consumed as an addendum — read each IN ' +
            'FULL from disk; the writers landed only the rows intersecting their own pages, so every row NOT already resolved on ' +
            'current disk is yours: land it at its root or reject it with reason): ' +
            JSON.stringify(census) +
            '.\n' +
            '(3) FINDER REPORTS — the finder products are ON DISK as JSON report files; the ROSTER receipts below are navigation, ' +
            'never the product. CONSUMPTION PROTOCOL, in order: (a) UNMAPPED scope below is your direct-hunt queue — a failed ' +
            "finder's territory gets your own cold read, first; (b) read every ok report IN FULL from disk, governance finders " +
            'before page slices — group findings by `claimKey` as you read (the same key across lanes is ONE defect with ' +
            'corroborating evidence, never several priorities) and order work by `severity` then `owner` (shared owners and ' +
            'registries before their consumers, cross-folder seams before local prose); (c) each finding is a SIGNAL, not law: ' +
            're-open its anchors before editing — anchors behind an edit, cited members, seams, and manifest rows re-verify ' +
            'MANDATORY; navigation-only entries in untouched groups re-verify only when touched; (d) `mechanism`/`owner`/`reject`/' +
            "`acceptance` are the finding's constraint boundary — honor the owner and the rejected forms, but the DESIGN is yours: " +
            'implement the STRONGEST resolution the boundary admits, never a single-point patch; where the implied fix is weak or ' +
            'short-sighted, land the denser root-level reconstruction instead; a finding with a dead anchor, already resolved on ' +
            'disk, or graded `hypothetical` with no substantive re-derivation is rejected with reason. ' +
            'UNMAPPED: ' +
            JSON.stringify(unmapped) +
            ' ROSTER: ' +
            JSON.stringify(roster) +
            '.\n' +
            '(4) OWN HUNT: hunt PAST the signal list on your own authority — the hunt classes above over the landed pages and the ' +
            'governance surface as you work them — and fix what the finders missed; `beyond` enumerates those fixes, and an empty ' +
            '`beyond` attests your hunt found nothing, never that it did not run.\n' +
            'Every ripple an edit exposes is YOURS in the same pass — seam counterparts both ends, consumer sites, index docs, ' +
            "manifest rows, .api anchors; wire-canonical names stay frozen; a foreign-language repair holds that branch's doctrine " +
            'bar. FAILED PAGES (reported, not landed — never author them here; correct any index or sibling claim that pretends ' +
            'they landed): ' +
            JSON.stringify(failed) +
            '. Return the final fixlog — `remaining` carries ONLY rows verified still-open on current disk and genuinely blocked, ' +
            'each claim naming its blocker and owner; a row disk already resolved is culled with proof in `rejected`, and an empty ' +
            '`remaining` attests the drain closed. ' +
            HARVEST_LAW,
    ]
        .filter(Boolean)
        .join('\n\n');

// --- [COMPOSITION] ---------------------------------------------------------------------

if (REJECTED.length) log('Rejected targets outside libs/{csharp,python,typescript}: ' + REJECTED.join(', '));
if (!TARGETS.length) {
    log('No targets — pass a target path, an array of paths, or {targets}. Empty args is a no-op.');
    return { targets: [], total: 0 };
}

phase('Plan');
const plan = await slot(() =>
    agent(planPrompt(), { label: 'plan', phase: 'Plan', model: 'opus', effort: 'high', schema: PLAN_SCHEMA, stallMs: STALL }),
);
const PAGES = normalizePages(plan);
const UNRESOLVED = (plan && plan.unresolved) || [];
if (UNRESOLVED.length) log('Unresolved targets (mis-scoped or renamed): ' + UNRESOLVED.join(', '));
const LANGS_IN = [...new Set(PAGES.map((p) => langOf(p.page)).filter(Boolean))];
log(
    'Plan: ' +
        PAGES.length +
        ' pages (' +
        PAGES.filter((p) => p.kind === 'new').length +
        ' new, ' +
        PAGES.filter((p) => p.kind === 'rebuild').length +
        ' rebuild) across ' +
        LANGS_IN.join('+') +
        '; CAP=' +
        CAP +
        ', BATCH_MAX=' +
        BATCH_MAX,
);
if (!PAGES.length) {
    log('No pages resolved under the targets');
    return { targets: TARGETS, total: 0 };
}

phase('Map');
// Corpus map ONCE per SUB-FOLDER unit, reused by every batch touching that unit: an opus deep-map lane (context/seams, native)
// beside a terra two-tier .api inventory lane (codex) PER `.planning/<sub>` — package-level mapping dilutes depth ~10x on a large
// package and starves the batches of per-page grounding. Products are per-unit dossiers + reports on disk; receipts on the wire.
const PKGS = [...new Set(PAGES.map((p) => pkgOf(p.page)))];
const UNITS = PKGS.flatMap((pkg) => {
    const pkgPages = PAGES.filter((p) => pkgOf(p.page) === pkg);
    return [...new Set(pkgPages.map((p) => subOf(p.page)))].map((sub) => ({
        pkg,
        sub,
        pages: pkgPages.filter((p) => subOf(p.page) === sub),
    }));
});
const unitMap = {};
await Promise.all(
    UNITS.map(async (u) => {
        const L = Lof(u.pkg);
        const unitPages = u.pages.map((p) => Object.assign({}, p, { i: 0 }));
        const scope = unitPages.map((p) => p.page);
        const tag = u.pkg.split('/').pop() + '.' + u.sub;
        const ctxDossier = dossierPath('map:ctx:' + tag);
        const apiDossier = dossierPath('map:api:' + tag);
        const [ctx, api] = await Promise.all([
            slot(() =>
                recon((reg) => ctxLensPrompt(L, unitPages, ctxDossier, reg), {
                    label: 'map:ctx:' + tag,
                    phase: 'Map',
                    schema: CTX_SCHEMA,
                    native: true,
                    nativeModel: 'opus',
                    scope,
                    hl: { arr: 'worklist', group: 'kind' },
                    stallMs: STALL,
                }),
            ),
            slot(() =>
                recon((reg) => apiLensPrompt(L, unitPages, apiDossier, reg), {
                    label: 'map:api:' + tag,
                    phase: 'Map',
                    schema: API_SCHEMA,
                    writes: true,
                    scope,
                    hl: { arr: 'worklist' },
                }),
            ),
        ]);
        unitMap[u.pkg + '|' + u.sub] = { ctx, api, ctxDossier, apiDossier };
    }),
);
const mapOk = Object.values(unitMap).filter((m) => (m.ctx && m.ctx.ok) || (m.api && m.api.ok)).length;
log('Map: ' + UNITS.length + ' sub-folder unit(s) across ' + PKGS.length + ' package(s) mapped; ' + mapOk + ' with a live dossier');

phase('Ideate');
// TWO lanes per owning package with disjoint charters: a corrections census (opus — the fix addendum the batches land as rung 3)
// and a bigger-ideas worklist (fable — the capability ambition, rung 4). One merged log regresses to a fixlog and the ambition dies.
// Either lane absent (dead map or dead ideate), the executors run without that rung.
const pkgIdeate = {};
await Promise.all(
    PKGS.map(async (pkg) => {
        const L = Lof(pkg);
        const tag = pkg.split('/').pop();
        const mapIndex = UNITS.filter((u) => u.pkg === pkg)
            .map((u) => ({
                sub: u.sub,
                deepMap: (unitMap[pkg + '|' + u.sub].ctx?.ok && unitMap[pkg + '|' + u.sub].ctxDossier) || null,
                inventory: (unitMap[pkg + '|' + u.sub].api?.ok && unitMap[pkg + '|' + u.sub].apiDossier) || null,
            }))
            .filter((r) => r.deepMap || r.inventory);
        if (!mapIndex.length) {
            pkgIdeate[pkg] = { fix: '', idea: '' };
            return;
        }
        const fixDossier = dossierPath('ideate:fix:' + tag);
        const ideaDossier = dossierPath('ideate:idea:' + tag);
        const [fix, idea] = await Promise.all([
            slot(() =>
                agent(correctionsPrompt(L, pkg, mapIndex, fixDossier), {
                    label: 'ideate:fix:' + tag,
                    phase: 'Ideate',
                    model: 'opus',
                    effort: 'high',
                    schema: RECEIPT,
                    stallMs: STALL,
                }),
            ),
            slot(() =>
                agent(ideasPrompt(L, pkg, mapIndex, ideaDossier), {
                    label: 'ideate:idea:' + tag,
                    phase: 'Ideate',
                    model: 'fable',
                    effort: 'high',
                    schema: RECEIPT,
                    stallMs: STALL,
                }),
            ),
        ]);
        pkgIdeate[pkg] = { fix: (fix && fix.ok && fixDossier) || '', idea: (idea && idea.ok && ideaDossier) || '' };
        log(
            'Ideate ' +
                tag +
                ': ' +
                (pkgIdeate[pkg].fix ? (fix.entries || 0) + ' correction(s)' : 'no census') +
                ' + ' +
                (pkgIdeate[pkg].idea ? (idea.entries || 0) + ' bigger-idea(s)' : 'no ideas'),
        );
    }),
);
const CENSUS_PATHS = PKGS.map((pkg) => pkgIdeate[pkg] && pkgIdeate[pkg].fix).filter(Boolean);

phase('Build');
// Batch composition packs WHOLE sub-folder units (in plan order) up to BATCH_MAX, splitting only an oversize unit — a batch's
// ownership seam then aligns with its map dossiers instead of slicing across sub-folders. Nothing serializes on the lanes;
// the agent-level slot scheduler is the only governor.
const packBatches = (units, max) => {
    const out = [];
    let cur = [];
    for (const u of units)
        for (const seg of u.pages.length > max ? evenChunk(u.pages, max) : [u.pages]) {
            if (cur.length && cur.length + seg.length > max) {
                out.push(cur);
                cur = [];
            }
            cur = cur.concat(seg);
        }
    if (cur.length) out.push(cur);
    return out;
};
const BATCHES = PKGS.flatMap((pkg) =>
    packBatches(
        UNITS.filter((u) => u.pkg === pkg),
        BATCH_MAX,
    ).map((pages, i) => ({ pkg, i, pages })),
);
const SCOPES = JSON.stringify(BATCHES.map((b) => ({ batch: b.pkg.split('/').pop() + ':b' + b.i, pages: b.pages.map((p) => p.page) })));
const built = (
    await Promise.all(
        BATCHES.map(async (b) => {
            const tag = b.pkg.split('/').pop() + ':b' + b.i;
            const L = Lof(b.pkg);
            const batch = b.pages.map((p) => Object.assign({}, p, { i: b.i }));
            const pageScope = batch.map((p) => p.page);
            // ctx + api grounding come from the Map phase (per-sub-folder units, reused); only the page-scoped bar lens runs per batch.
            const pms = [...new Set(batch.map((p) => subOf(p.page)))].map((s) => unitMap[b.pkg + '|' + s]).filter(Boolean);
            const bar = await slot(() =>
                recon((reg) => barLensPrompt(L, batch, reg), {
                    label: 'recon:bar:' + tag,
                    phase: 'Build',
                    schema: BAR_SCHEMA,
                    scope: pageScope,
                    hl: { arr: 'findings', group: 'severity' },
                }),
            );
            const roster = pms
                .flatMap((m) => [m.ctx, m.api])
                .concat([bar])
                .filter(Boolean);
            // A failed Map lane carries its WHOLE unit as scope; intersect with this batch so cold-read stays on its own pages.
            const unmapped = roster
                .filter((r) => !r.ok)
                .flatMap((r) => (r.scope || []).filter((sc) => pageScope.includes(sc)).map((sc) => ({ lane: r.lane, scope: sc })));
            if (unmapped.length)
                log(
                    tag +
                        ' — FAILED lens(es): ' +
                        roster
                            .filter((r) => !r.ok)
                            .map((r) => r.lane)
                            .join(', '),
                );
            const dossiers = pms
                .flatMap((m) => [m.ctx && m.ctx.ok && m.ctxDossier, m.api && m.api.ok && m.apiDossier])
                .filter(Boolean)
                .join('`, `');
            const ideate = pkgIdeate[b.pkg] || { fix: '', idea: '' };
            const fix = await slot(() =>
                agent(implementPrompt(L, batch, dossiers, ideate, SCOPES, roster, unmapped), {
                    label: 'impl:' + tag,
                    phase: 'Build',
                    model: 'fable',
                    effort: 'high',
                    schema: FIXLOG_SCHEMA,
                    stallMs: STALL,
                }),
            );
            if (!fix) return { pkg: b.pkg, pages: b.pages, fix: null, crit: null, rt: null }; // failure isolation: a dead implement skips its reviews
            // Sol critique: a workspace-write codex lane running the full conformance audit in place;
            // fixlog to disk, receipt on the wire; the redteam reads the fixlog from disk and folds its rows forward.
            const crit = await slot(() =>
                recon((reg) => critiquePrompt(L, batch, dossiers, ideate, SCOPES, roster, unmapped, navOf([fix]), reg), {
                    label: 'crit:' + tag,
                    phase: 'Build',
                    schema: REVIEW_SCHEMA,
                    writes: true,
                    fix: true,
                    model: 'gpt-5.6-sol',
                    nativeModel: 'fable',
                    stallMs: SOL_STALL,
                    scope: pageScope,
                    hl: { arr: 'files' },
                }),
            );
            const critR = crit && crit.ok ? crit : null;
            const rt = await slot(() =>
                agent(redteamPrompt(L, batch, dossiers, ideate, SCOPES, roster, unmapped, navOf([fix]), critR), {
                    label: 'rt:' + tag,
                    phase: 'Build',
                    model: 'fable',
                    effort: 'high',
                    schema: REVIEW_SCHEMA,
                    stallMs: STALL,
                }),
            );
            return { pkg: b.pkg, pages: b.pages, fix, crit: critR, rt };
        }).map((p) => p.catch(() => null)),
    )
).filter(Boolean);
const FAILED = built.filter((d) => !d.fix).flatMap((d) => d.pages.map((p) => p.page));
const LANDED = built.filter((d) => d.fix).flatMap((d) => d.pages.map((p) => p.page));
// The critique fixlog lives on disk; the redteam folds its rows into its own return, so aggregation
// reads fix + rt only. A batch whose redteam died leaves the critique fixlog ORPHANED for the fixer.
const ROWS = built.flatMap((d) => ((d.fix && d.fix.indexRows) || []).concat((d.rt && d.rt.indexRows) || []));
const SEAM_ROWS = built.flatMap((d) => ((d.fix && d.fix.seamsTouched) || []).concat((d.rt && d.rt.seamsTouched) || []));
const BACKLOG = built.flatMap((d) => ((d.fix && d.fix.deferred) || []).concat((d.rt && d.rt.deferred) || []));
const ORPHANS = built.filter((d) => d.crit && !d.rt).map((d) => d.crit.report);
log(
    'Build: ' +
        LANDED.length +
        '/' +
        PAGES.length +
        ' pages landed across ' +
        BATCHES.length +
        ' batch(es); ' +
        SEAM_ROWS.length +
        ' seam row(s), ' +
        BACKLOG.length +
        ' deferred backlog row(s)' +
        (FAILED.length ? ' — FAILED (reported, run continues): ' + FAILED.join(', ') : ''),
);
if (!LANDED.length) {
    log('Nothing landed — no close to run');
    return { targets: TARGETS, batches: BATCHES.length, landed: 0, failed: FAILED };
}

phase('Close');
const LANDED_LANGS = [...new Set(LANDED.map((p) => langOf(p)).filter(Boolean))];
const finderTasks = LANDED_LANGS.flatMap((k) => {
    const langPages = LANDED.filter((p) => langOf(p) === k);
    const langSeams = SEAM_ROWS.filter((s) => langOf(s.file) === k || langOf(s.counterpart) === k);
    return chunk(langPages, FINDER_PAGES)
        .map((pages, i) => ({ gov: false, lang: k, pages, seams: langSeams, i }))
        .concat([{ gov: true, lang: k, pkgs: [...new Set(langPages.map(pkgOf))], pages: langPages }]);
});
const found = (
    await Promise.all(
        finderTasks.map((t) =>
            slot(() =>
                t.gov
                    ? recon((reg) => govFinderPrompt(LANG[t.lang], t.pkgs, t.pages, ROWS, reg), {
                          label: 'finder:gov:' + t.lang,
                          phase: 'Close',
                          schema: FINDINGS_SCHEMA,
                          scope: t.pkgs,
                          hl: { arr: 'findings', group: 'class' },
                      })
                    : recon((reg) => finderPrompt(LANG[t.lang], t.pages, t.i, t.seams, reg), {
                          label: 'finder:' + t.lang + ':s' + t.i,
                          phase: 'Close',
                          schema: FINDINGS_SCHEMA,
                          scope: t.pages,
                          hl: { arr: 'findings', group: 'class' },
                      }),
            ).catch(() => null),
        ),
    )
).filter(Boolean);
const FOUND = found.filter((f) => f.ok).reduce((a, f) => a + f.entries, 0);
const UNMAPPED = found.filter((f) => !f.ok).flatMap((f) => f.scope.map((sc) => ({ lane: f.lane, scope: sc })));
log(
    'Close: ' +
        FOUND +
        ' finding(s) across ' +
        found.filter((f) => f.ok).length +
        '/' +
        found.length +
        ' finder(s)' +
        (UNMAPPED.length
            ? ' — FAILED: ' +
              found
                  .filter((f) => !f.ok)
                  .map((f) => f.lane)
                  .join(', ')
            : '') +
        '; ' +
        ROWS.length +
        ' index row(s) + ' +
        BACKLOG.length +
        ' backlog row(s) pending',
);
// Terminal DRAIN LOOP: one serial fable closer per round takes the full residual set, verifies every
// row against live disk (freshness is its duty — no concurrent writers, no collisions), fixes at root,
// and loops until the set is empty; a round without shrinkage stops the loop with the blocked set final.
let fixer = null;
let fixerHarvest = [];
let residuals = BACKLOG;
let orphanQueue = ORPHANS;
let lastOpen = Infinity;
for (let round = 0; round < DRAIN_ROUNDS; round++) {
    fixer = await slot(() =>
        agent(
            fixerPrompt(
                LANDED_LANGS,
                round ? [] : found,
                round ? [] : UNMAPPED,
                round ? [] : ROWS,
                residuals,
                FAILED,
                LANDED,
                orphanQueue,
                round ? [] : CENSUS_PATHS,
                round,
            ),
            { label: round ? 'fixer:r' + round : 'fixer', phase: 'Close', model: 'fable', effort: 'high', schema: FIXER_SCHEMA, stallMs: STALL },
        ),
    );
    if (!fixer) break; // dead round: the fed-in residual and orphan sets survive to the run return, never zeroed by a lost closer
    fixerHarvest = fixerHarvest.concat(fixer.harvest || []);
    const open = fixer.remaining || [];
    orphanQueue = [];
    residuals = open;
    if (!open.length || open.length >= lastOpen) break;
    lastOpen = open.length;
}
// DOCTRINE LANDER: the run's durable-learning terminal — pooled harvest nominations adjudicated against
// the live doctrine surfaces; refutation-first, land-nothing legal, admission law owned by docs/laws.
const HARVEST_ROWS = built.flatMap((d) => ((d.fix && d.fix.harvest) || []).concat((d.rt && d.rt.harvest) || [])).concat(fixerHarvest);
const doctrine = HARVEST_ROWS.length
    ? await slot(() =>
          agent(
              'TASK: DOCTRINE LANDER — the durable-learning terminal of this run. Read `docs/laws/README.md` ' +
                  'FIRST — it owns the corpus admission and page-shape law; obey it over any restatement. Load ' +
                  'the `docgen` skill AND the `skill-writer` skill via the Skill tool BEFORE any durable edit; load ' +
                  '`mermaid-diagramming` before touching any diagram. ' +
                  "NOMINATIONS (unverified, biased toward their authors' own work — refute by default): " +
                  JSON.stringify(HARVEST_ROWS) +
                  '\nADJUDICATE each row per the admission bar: cold-read its target surface IN FULL, verify its anchors on ' +
                  'CURRENT disk; LAND NOTHING is a first-class verdict.\n' +
                  'TOPOLOGY RE-PROOF: re-verify every `docs/laws/topology.md` row whose [SURFACE] this run touched — cull a row ' +
                  'whose coupling no longer holds, land a coupling this run proved.\n' +
                  'GATE: run `uv run .claude/skills/docgen/scripts/prose_gate.py <every touched .md>` and repair to zero FAILs ' +
                  'before returning. Return landed/refined/rejected (each rejection with its reason)/files/summary.',
              { label: 'doctrine', phase: 'Close', model: 'fable', effort: 'high', schema: DOCTRINE_SCHEMA, stallMs: STALL },
          ),
      )
    : null;
return {
    targets: TARGETS,
    languages: LANDED_LANGS,
    batches: BATCHES.length,
    landed: LANDED.length,
    failed: FAILED,
    seamRows: SEAM_ROWS.length,
    backlog: BACKLOG.length,
    findings: FOUND,
    failedFinders: found.filter((f) => !f.ok).map((f) => f.lane),
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
        nominated: HARVEST_ROWS.length,
        landed: (doctrine.landed || []).length,
        refined: (doctrine.refined || []).length,
        rejected: (doctrine.rejected || []).length,
        files: doctrine.files || [],
        summary: doctrine.summary,
    },
};
