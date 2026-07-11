export const meta = {
    name: 'rebuild',
    whenToUse:
        'The standing hostile rebuild pass for any libs/ planning corpus: pass targets (file / sub-folder / package root, any number, any language mix); it plans, triple-lens-discovers, hostile-implements, critiques, and red-teams every page batch CONCURRENTLY at each owning-language doctrine bar with libs-wide ripple authority, then a read-only finder fan and one terminal fixer close the run.',
    description:
        'Language-agnostic rebuild engine over libs/{csharp,python,typescript} planning corpora. args = a target path, an array of paths, or {targets}; empty = no-op; targets may MIX languages — each batch derives its doctrine, both .api tiers, casing, and member-verification rail from its own owning package. Plan (1 opus) expands targets to pages in dependency order under the owning-package charter, seam-cohesive pages adjacent. Build runs ALL batches (even-split, max 8 pages) concurrently under one agent-level slot scheduler: per batch THREE concurrent recon lenses (context/seams + two-tier .api stacking + doctrine-bar quality attack, each second-pass self-verified, information never prescriptions) write their complete products as on-disk JSON reports beside two grounding dossiers and return thin receipts on the wire - recon and finder lanes run on gpt-5.6-terra dispatched through codex wrapper agents (CODEX flag; false restores native lanes); then implement (fable), critique (ONE gpt-5.6-sol codex lane per batch, workspace-write, running the full conformance audit in place with its fixlog written to disk), and redteam (fable, terminal — reading the critique fixlog from disk as refutation targets and FOLDING its surviving deferred/index/seam rows into the batch record) chain behind their own batch only, consuming the lens reports IN FULL from disk with failed-lens territory cold-read first; a batch whose redteam dies leaves its critique fixlog ORPHANED, drained by the terminal fixer. Handoffs between chain stages carry NAVIGATION FACTS ONLY (files, symbol deltas, seam rows, deferred backlog) — never self-assessment; reviewers derive their own defect list from disk first and receive prior claims strictly as refutation targets. Cross-batch coordination runs through per-batch seam-ledger files in scratch (typed fact rows, read by area before any cross-batch edit). Every writer holds LIBS-WIDE ripple authority under four non-arbitrary bounds: evidence (anchored ripples only), expand-form (concurrent foreign edits are additive; collapse serializes to the fixer), depth (first-order fixed both ends now; second-order recorded to the backlog), and decision/propagation (index docs + central manifests + IDEAS rows single-writer via the terminal fixer; ruled-contract propagation distributed). Generative openings land now as owner-grammar growth anywhere in libs/, or as a fully-specified IDEAS row the fixer applies this run. Close: read-only finder fan per language on gpt-5.6-terra (landed pages + seam-ledger verification + one governance finder per language), typed anchored findings graded substantive|hypothetical written as on-disk reports with thin receipts, stale findings discarded; ONE terminal fable fixer reads every ok report IN FULL from disk (failed-finder territory cold-read first), applies every index row, re-verifies findings as signals, drains the deferred backlog, hunts past everything, and returns the final fixlog. Nothing follows the fixer.',
    phases: [
        {
            title: 'Plan',
            detail: 'one thin agent expands the targets into the dependency-ordered, seam-cohesion-adjacent page list under each owning-package charter: existing pages as rebuild, charter-demanded absent pages as new, settled pages skipped',
            model: 'opus',
        },
        {
            title: 'Build',
            detail: 'all batches concurrent under the agent-level slot cap: per batch a 3-lens gpt-5.6-terra recon burst (codex wrappers; products to disk, receipts on the wire) then implement (fable), critique (gpt-5.6-sol codex lane, fixlog to disk), redteam (fable, folding the critique rows forward) chained behind their own batch; navigation-fact handoffs, seam-ledger coordination, libs-wide bounded ripple authority',
        },
        {
            title: 'Close',
            detail: 'read-only gpt-5.6-terra finder fan (codex wrappers; products to disk, receipts on the wire) per language over landed pages + the seam ledger, plus one governance finder per language; ONE terminal fable fixer reads the reports from disk, applies index rows, resolves findings as signals, drains the deferred backlog, hunts past them, returns the final fixlog',
        },
    ],
};

// --- [CONSTANTS] -------------------------------------------------------------------------

const CAP = 14; // runtime concurrency clamp is min(16, cores-2) = 14 on this machine; matching it keeps the stagger honest
const STAGGER_MS = 1500;
const STALL = 300000;
const CODEX_STALL = 1500000; // wrapper stall sits above the xhigh blocking-call ceiling (1200s): a silent live MCP call is legal waiting, never a stall
const SOL_STALL = 2400000; // sol critique holds one long blocking MCP call at the operator-default tier; stall detection must outlast it
const BATCH_MAX = 8; // even-split ceiling; editing fidelity degrades past ~8 dense pages per writer under full doctrine reads
const FINDER_PAGES = 8; // landed pages per close-phase finder
const SCRATCH = '.claude/scratch/rebuild'; // lane report files + grounding dossiers + per-batch seam-ledger files
const CODEX = true; // recon/finder lanes run on gpt-5.6-terra via the codex wrapper; false restores native opus lanes

// --- [INPUTS] ----------------------------------------------------------------------------

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

// --- [MODELS] ----------------------------------------------------------------------------

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
        dossierPhantoms: { type: 'array', items: { type: 'string' } },
    },
};

const REVIEW_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'verdict', 'summary', 'seamsTouched', 'deltas', 'deferred', 'beyondMap', 'indexRows', 'extended'],
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
    required: ['files', 'indexApplied', 'resolved', 'backlogDrained', 'beyond', 'rejected', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
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
        summary: { type: 'string' },
    },
};

// --- [DOCTRINE] --------------------------------------------------------------------------

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

// --- [OPERATIONS] ------------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms));
// Agent-level slot scheduler: CAP agents in flight across ALL batch chains, staggered launch,
// work-conserving backfill the moment a slot frees. The single governor for every agent call.
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
// (role split, battery-validated); the prompt carries only the task; the output contract sits LAST.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
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
            o.hl.arr +
            '"], headline="<entries> ' +
            o.hl.arr +
            (o.hl.group ? ' | <' + o.hl.group + ' tallies>' : '') +
            ' | top: <most frequent first file or none>", and failure empty. On a second tool error return ok=false, entries=0, ' +
            'report and headline empty, and failure equal to the error text VERBATIM.',
    ].join('\n\n');
};
// Every codex-dispatched lane routes here: terra by default, sol where o.model says so; CODEX=false restores
// a fully native run. QUOTA FALLBACK: a codex receipt whose failure matches usage/quota/limit re-dispatches
// the SAME task natively at the role's Claude twin (terra->opus, sol->fable, luna->sonnet) — the caller owns
// the re-dispatch; the sonnet wrapper never executes work itself. The roster row carries `scope` from the
// ORCHESTRATOR (never the lane's self-report) so a failed lane's unmapped territory is exact even when the
// lane died before writing anything.
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
// Even split: ceil(n/max) batches of near-equal size — no runt tail heavying batch 0 and starving the last.
const evenChunk = (arr, max) => chunk(arr, Math.ceil(arr.length / (Math.ceil(arr.length / max) || 1)));
const pkgOf = (p) => p.split('/.planning/')[0]; // package = the write-partition key (index docs live at its root)
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

// Every rigor law appears exactly once, here; stages compose subsets. Block order in prompts:
// stable per-language law first (byte-identical across a batch's stages), batch-variable material
// second, the stage task + output contract LAST — nothing load-bearing mid-prompt.
const CONTEXT = (L) => 'Rasm monorepo — ' + L.corpus + '. ' + L.strata + ' ' + L.stackFloor;

const STANCE = (L) =>
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
    'that finds nothing is a first-class result, proven by adding nothing.';

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

const SELF_CHECK =
    'MANDATORY SELF-VERIFY (second pass, before returning): adversarially re-derive every entry from disk — ' +
    're-open each cited anchor and confirm it states what the entry claims, re-verify each member spelling against its ' +
    'catalog, trace each seam to both endpoints. An entry that fails re-confirmation is corrected or deleted, never returned; ' +
    'a guess, an assumption, a skimmed summary, or a vague/hedged entry is a defect. Completeness is part of correctness: ' +
    'after the re-read, hunt once more for what the first pass missed — an omitted load-bearing fact is as wrong as a false one.';

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

const ANTI_ANCHOR = (L) =>
    'ANTI-ANCHOR LAW: your report and dossier carry FINDINGS, never designs — quality defects graded ' +
    'against the doctrine read at source (name the law and the ' +
    L.stack +
    ' pattern whose application would most deeply ' +
    'transform the page — the collapse, the owner form, the rail — never the resulting code) and capability inventory in ' +
    'catalog-anchored spellings; a fence sketch, a prescribed shape, or a pre-ruled design ANCHORS and WEAKENS the rebuild ' +
    'and is your defect — the implement agent rules every design.';

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
    'RECON REPORTS — the lens products are ON DISK as JSON report files; the ' +
    'receipts below are navigation, never the product. CONSUMPTION: (a) UNMAPPED territory below (a dead lens) gets your own ' +
    'cold read FIRST — that lens dimension over your pages is yours to derive; (b) read every ok report IN FULL from disk — ' +
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

const preamble = (L, batch, dossiers, scopes, roster, unmapped) =>
    [CONTEXT(L), STANCE(L), BUILD_LAW(L), BODY(L), VERIFY(L), RIPPLE_LAW, CURRENT_STATE, PROSE_COMMENTS(L)]
        .concat(L.mechanics ? [L.mechanics] : [])
        .concat([
            readFirst(L, pkgOf(batch[0].page), dossiers),
            LEDGER(scratchBase(pkgOf(batch[0].page), batch[0].i || 0), scopes),
            reconBlock(roster, unmapped),
        ]);

// Prompt builders — each task states only its own action; shared checks are referenced by name.
const planPrompt = () =>
    [
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

const ctxLensPrompt = (L, batch, dossier) =>
    [
        CONTEXT(L),
        STANCE(L),
        INFO_LAW,
        SELF_CHECK,
        ANTI_ANCHOR(L),
        readFirst(L, pkgOf(batch[0].page), ''),
        'TASK: HOSTILE READ-ONLY CONTEXT + SEAM LENS over these ' +
            batch.length +
            ' pages — read-only is the only concession; ' +
            'the hunt is as adversarial as every writing pass (investigate, do NOT edit): ' +
            batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') +
            '. For a rebuild page read the page IN FULL; for a `new` page ' +
            'read its concept in the owning-package charter plus its nearest sibling pages. Read the folder at large — the sibling ' +
            'pages each composes and the owning-package index docs — as full-file reads. For EACH page return: `owns` (the ONE ' +
            'ownership boundary sentence — which owner/vocabulary/concern THIS page owns versus its siblings, so no two concurrent ' +
            'writers author the same polymorphic surface), `contextNote` (sibling owners/seams composed, folder position, any ' +
            'folder-wide gap routed here), `seams` (every cross-page and cross-package symbol/wire/consumer edge, both endpoints ' +
            'named), and DOMAIN gaps — attributes, sub-kinds, states, relationships, operations the real concept demands that the ' +
            'page omits — folded into `contextNote` as named gaps. Each worklist entry also carries `files` (what the consumer must ' +
            'open) and typed `anchors` per the entry form. GROUNDING DOSSIER: write `' +
            dossier +
            '` — Tier-1: the branch ' +
            'ARCHITECTURE.md [02]-[SEAMS] rows covering these pages quoted verbatim with `file:line` anchors, folder-context anchors, ' +
            'charter intent anchors; Tier-2: pointer rows (path + one line) for every sibling page composed. FORBIDDEN in the ' +
            'dossier: doctrine digests, removal framing, unanchored claims, any prescriptive design. Return worklist + coverage.',
    ].join('\n\n');

const apiLensPrompt = (L, batch, dossier) =>
    [
        CONTEXT(L),
        STANCE(L),
        VERIFY(L),
        INFO_LAW,
        SELF_CHECK,
        ANTI_ANCHOR(L),
        'TASK: HOSTILE READ-ONLY TWO-TIER STACKING LENS over these ' +
            batch.length +
            ' pages (investigate, do NOT edit): ' +
            batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') +
            '. `ls` BOTH catalog tiers in full — the shared substrate `' +
            L.root +
            '/.api/` AND the folder `' +
            pkgOf(batch[0].page) +
            '/.api/` — read every catalog relevant to these pages, and ' +
            'DIFF the complete admitted inventory against the whole folder: DISBELIEVE the pages — prose claiming a package is ' +
            'composed is verified against the fence body; attack every admitted catalog (both tiers) for the members, combinators, ' +
            'generated surfaces, and native pipelines the concept ADMITS but no fence exploits — a capability no page exploits is a ' +
            'named integration gap ROUTED to EVERY page whose concept admits it, never one "best" owner alone. SINGLE-CONSUMER ' +
            'EXPANSION: a package with a catalog at ANY tier consumed by only ONE page is expansion pressure on its siblings — name ' +
            'the package, its unexploited members in exact spellings, and each candidate page. Discovery has ZERO removal authority: ' +
            'an underutilized catalog is always a buildout target (which owner grows which case/row/field/operation), never removal ' +
            'evidence. For EACH page return `apiUsed`, `apiUnderutilized` ({catalog, capability}: exact catalog-anchored spelling + ' +
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

const barLensPrompt = (L, batch) =>
    [
        CONTEXT(L),
        STANCE(L),
        EVIDENCE_LAW,
        SELF_CHECK,
        ANTI_ANCHOR(L),
        'TASK: HOSTILE READ-ONLY DOCTRINE-BAR ATTACK over these ' +
            batch.length +
            ' pages (investigate, do NOT edit): ' +
            batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') +
            '. Enumerate `' +
            L.stack +
            '/` with a real `ls` and read the ' +
            'README and EVERY root page it routes IN FULL' +
            (L.key === 'cs' ? ' (and the domain/ shards these pages touch)' : '') +
            '; then read each target page IN FULL and attack its quality against the doctrine AT SOURCE — EXTREMELY adversarial: the ' +
            'page is presumed ' +
            L.slur +
            ' until proven otherwise. Hunt where doctrine is not followed AND where a doctrine law ' +
            'could be applied more deeply for a stronger form: collapse signals ungathered, owner forms weaker than the discriminants ' +
            'demand, rails split or dual-paradigm, knobs where policy values belong, naive bodies below the admitted combinator ' +
            'surface, ' +
            L.docBloat +
            ' bloat, file-organization drift, both naivety axes. Return per-page `findings` in the ' +
            'FINDING FORM — `law` names the doctrine law/pattern at its source, `claimKey` = <law>|<owner>|<primary symbol>, typed ' +
            '`anchors` at exact coordinates — and `weak` (pages whose overall verdict is weak). Findings name the law and the ' +
            'defect, NEVER the resulting code — the implement agent rules every design.',
    ].join('\n\n');

const implementPrompt = (L, batch, dossiers, scopes, roster, unmapped) =>
    preamble(L, batch, dossiers, scopes, roster, unmapped)
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
                'LAW. ' +
                L.modern +
                '; ' +
                L.fileOrg +
                '; high-signal all-backticked prose. Return the fix-log — `deltas` carries every ' +
                'moved symbol/wire as data, `deferred` carries the backlog rows, both exact.',
        ])
        .join('\n\n');

const critiquePrompt = (L, batch, dossiers, scopes, roster, unmapped, nav) =>
    preamble(L, batch, dossiers, scopes, roster, unmapped)
        .concat([
            'NAVIGATION (facts from the pass that landed these pages — locations only, no assessments; it changes where you look ' +
                'FIRST, never what you conclude): ' +
                JSON.stringify(nav),
            GIT_GROUND,
            'TASK: HOSTILE DOCTRINAL-CONFORMANCE + CAPABILITY AUDIT; fix EACH page in place: ' +
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
                'Return the batched fix-log — `deltas` and `deferred` exact.',
        ])
        .join('\n\n');

const redteamPrompt = (L, batch, dossiers, scopes, roster, unmapped, nav, crit) =>
    preamble(L, batch, dossiers, scopes, roster, unmapped)
        .concat([
            'NAVIGATION (locations only, no assessments): ' + JSON.stringify(nav),
            crit && crit.ok
                ? 'PRIOR CLAIMS (UNVERIFIED): the sol critique fixlog is ON DISK at ' +
                  crit.report +
                  ' — read it IN FULL from disk; its edits and verdicts are refutation targets you judge against CURRENT disk, never ' +
                  'a settled record. FOLD-FORWARD DUTY: its surviving `seamsTouched`, `deltas`, `deferred`, `beyondMap`, and `indexRows` ' +
                  "rows are folded into YOUR return (re-verified against current disk, deduped) — your fix-log is the batch's " +
                  'consolidated record; a dropped critique row is a silent loss.'
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
                '`deferred` exact.',
        ])
        .join('\n\n');

const finderPrompt = (L, pages, i, seams) =>
    [
        CONTEXT(L),
        HUNT,
        EVIDENCE_LAW,
        SELF_CHECK,
        GIT_GROUND,
        'TASK: HOSTILE READ-ONLY FINDER, slice ' +
            i +
            ' (investigate, do NOT edit). The run just landed a hostile rebuild over ' +
            'these pages: ' +
            JSON.stringify(pages) +
            '. The landed corpus is presumed defective until your attack finds nothing. ' +
            'Read each page IN FULL fresh from CURRENT disk, plus the sibling owners each composes and every .api catalog its fences ' +
            'cite (both tiers where cited) — understand what the run changed before judging it. Hunt the classes above across the ' +
            'slice. SEAM SIGNALS (rows the run reported — verify BOTH ends of each on current disk; an end missing on disk is a ' +
            'finding): ' +
            JSON.stringify(seams) +
            '. STALE DISCARD: judge only CURRENT disk — a defect already resolved on disk, at ' +
            'either end of a seam, is DROPPED, never reported. Findings are INFORMATION for the terminal fixer, never prescriptions: ' +
            'name the defect, the law or catalog member it violates, the exact file anchor, and the grade — never the resulting ' +
            'code, a fence sketch, or a ruled design. Return typed anchored graded findings.',
    ].join('\n\n');

const govFinderPrompt = (L, pkgs, pages, rows) =>
    [
        CONTEXT(L),
        HUNT,
        EVIDENCE_LAW,
        SELF_CHECK,
        "TASK: HOSTILE READ-ONLY GOVERNANCE FINDER (investigate, do NOT edit). Audit the owning packages' index surface end to " +
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

const fixerPrompt = (langs, roster, unmapped, rows, backlog, failed, pages, orphans) =>
    [
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
            'indexRows rows were never folded forward — read each IN FULL from disk and drain those rows under the same law): ' +
            JSON.stringify(orphans) +
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
            '. Return the final fixlog.',
    ].join('\n\n');

// --- [COMPOSITION] -----------------------------------------------------------------------

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

phase('Build');
// Lanes keep sub-folder grouping + plan dependency/cohesion order for batch composition; nothing
// serializes on them — the agent-level slot scheduler is the only governor.
const lanes = [...new Set(PAGES.map((p) => pkgOf(p.page)))].map((pkg) => ({ pkg, pages: PAGES.filter((p) => pkgOf(p.page) === pkg) }));
const BATCHES = lanes.flatMap((lane) => evenChunk(lane.pages, BATCH_MAX).map((pages, i) => ({ pkg: lane.pkg, i, pages })));
const SCOPES = JSON.stringify(BATCHES.map((b) => ({ batch: b.pkg.split('/').pop() + ':b' + b.i, pages: b.pages.map((p) => p.page) })));
const built = (
    await Promise.all(
        BATCHES.map(async (b) => {
            const tag = b.pkg.split('/').pop() + ':b' + b.i;
            const L = Lof(b.pkg);
            const batch = b.pages.map((p) => Object.assign({}, p, { i: b.i }));
            const pageScope = batch.map((p) => p.page);
            const ctxDossier = dossierPath('recon:ctx:' + tag);
            const apiDossier = dossierPath('recon:api:' + tag);
            // 3-lens recon burst: independent read-only mappers run concurrently; the chain waits on all three receipts.
            // ctx/api lenses write their dossier files, so their codex sandbox is workspace-write; bar is read-only.
            const [ctx, api, bar] = await Promise.all([
                slot(() =>
                    recon(ctxLensPrompt(L, batch, ctxDossier), {
                        label: 'recon:ctx:' + tag,
                        phase: 'Build',
                        schema: CTX_SCHEMA,
                        writes: true,
                        scope: pageScope,
                        hl: { arr: 'worklist', group: 'kind' },
                    }),
                ),
                slot(() =>
                    recon(apiLensPrompt(L, batch, apiDossier), {
                        label: 'recon:api:' + tag,
                        phase: 'Build',
                        schema: API_SCHEMA,
                        writes: true,
                        scope: pageScope,
                        hl: { arr: 'worklist' },
                    }),
                ),
                slot(() =>
                    recon(barLensPrompt(L, batch), {
                        label: 'recon:bar:' + tag,
                        phase: 'Build',
                        schema: BAR_SCHEMA,
                        scope: pageScope,
                        hl: { arr: 'findings', group: 'severity' },
                    }),
                ),
            ]);
            const roster = [ctx, api, bar].filter(Boolean);
            const unmapped = roster.filter((r) => !r.ok).flatMap((r) => r.scope.map((sc) => ({ lane: r.lane, scope: sc })));
            if (unmapped.length)
                log(
                    tag +
                        ' — FAILED lens(es): ' +
                        roster
                            .filter((r) => !r.ok)
                            .map((r) => r.lane)
                            .join(', '),
                );
            const dossiers = [ctx && ctx.ok && ctxDossier, api && api.ok && apiDossier].filter(Boolean).join('`, `');
            const fix = await slot(() =>
                agent(implementPrompt(L, batch, dossiers, SCOPES, roster, unmapped), {
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
                recon(critiquePrompt(L, batch, dossiers, SCOPES, roster, unmapped, navOf([fix])), {
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
                agent(redteamPrompt(L, batch, dossiers, SCOPES, roster, unmapped, navOf([fix]), critR), {
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
                    ? recon(govFinderPrompt(LANG[t.lang], t.pkgs, t.pages, ROWS), {
                          label: 'finder:gov:' + t.lang,
                          phase: 'Close',
                          schema: FINDINGS_SCHEMA,
                          scope: t.pkgs,
                          hl: { arr: 'findings', group: 'class' },
                      })
                    : recon(finderPrompt(LANG[t.lang], t.pages, t.i, t.seams), {
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
const fixer = await slot(() =>
    agent(fixerPrompt(LANDED_LANGS, found, UNMAPPED, ROWS, BACKLOG, FAILED, LANDED, ORPHANS), {
        label: 'fixer',
        phase: 'Close',
        model: 'fable',
        effort: 'high',
        schema: FIXER_SCHEMA,
        stallMs: STALL,
    }),
);
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
    fixer: fixer && {
        files: (fixer.files || []).length,
        indexApplied: (fixer.indexApplied || []).length,
        resolved: (fixer.resolved || []).length,
        backlogDrained: (fixer.backlogDrained || []).length,
        beyond: (fixer.beyond || []).length,
        rejected: (fixer.rejected || []).length,
        summary: fixer.summary,
    },
};
