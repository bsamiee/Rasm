export const meta = {
    name: 'rebuild',
    whenToUse:
        'The standing hostile rebuild pass for any libs/ planning corpus: pass targets (file / sub-folder / package root, any number, any language mix); it maps every .planning sub-folder, ideates per package, hostile-rebuilds every page batch concurrently at the owning-language doctrine bar, and closes with a finder fan plus one terminal fixer.',
    description:
        'Language-agnostic hostile-rebuild engine over the libs/{csharp,python,typescript} planning corpora. args = a target path, an array of paths, or {targets} — languages mix freely, {root} retargets an isolated checkout, empty = no-op; every page derives doctrine, both .api tiers, casing, and its member-verification rail from its owning package. Plan (opus) expands targets to pages in dependency + seam-cohesion order under the owning-package charter. Map fans one opus deep-map lane and one gpt-5.6-terra full-source .api + manifest inventory lane per .planning sub-folder unit — an oversize sub-folder splits into ceiling-bounded segments, so map and batch seams stay congruent — plus one ctx+api feeder pair per strata-legal upstream package (planner-derived from the branch architecture, kernel always present) projecting feeder depth onto the targets; every lane writes a dossier the batches reuse. Ideate runs two lanes per package with disjoint charters: a corrections census (opus, the non-binding fix addendum) and a bigger-ideas worklist (opus, new capability beyond correction). Build packs whole sub-folder units into batches under the packing ceiling, all concurrent under one slot scheduler; per batch a gpt-5.6-sol doctrine-bar lens, then opus implement, gpt-5.6-sol critique (workspace-write, fixlog to disk), and opus redteam folding the critique rows forward — every writer under the own-pass-first input ladder (own blind hostile pass primary, map dossiers grounding, census addendum, ideas ambition) with libs-wide ripple authority under the four bounds and seam-ledger coordination; handoffs carry navigation facts only. Close: a read-only gpt-5.6-sol (medium) finder fan plus one governance finder per language, opus slice drains, then ONE terminal fable fixer draining findings, the deferred backlog, and unclaimed census and idea rows in a fixpoint loop whose finishing round also lands the doctrine adjudication (pooled harvest nominations plus every critique fixlog harvest array from disk); a standalone opus lander fires only as the dead-fixer fallback. Stage law lives in the prompt blocks; CODEX=false restores native lanes throughout.',
    phases: [
        {
            title: 'Plan',
            detail: 'targets expand to the dependency-ordered, seam-cohesion-adjacent page list under each owning-package charter: existing pages rebuild, charter-demanded absences new, settled pages skip',
            model: 'opus',
        },
        {
            title: 'Map',
            detail: 'per sub-folder unit segment: an opus deep-map lane (ownership, seams, cross-folder relevance, domain gaps — information, never code) beside a terra full-source .api + manifest inventory lane, each writing the per-unit dossier the batches reuse; plus one ctx+api feeder pair per strata-legal upstream package, projecting kernel and sibling depth onto the targets',
            model: 'opus',
        },
        {
            title: 'Ideate',
            detail: 'per package, two disjoint lanes: the corrections census (opus, the deduped disk-verified fix addendum) and the bigger-ideas worklist (fable, new capability beyond correction under the value bar)',
        },
        {
            title: 'Build',
            detail: 'sub-folder-packed batches, all concurrent: per batch a terra doctrine-bar lens, then opus implement, sol critique (fixlog to disk), opus redteam folding the critique forward — every writer on the own-pass-first ladder with bounded libs-wide ripple authority and seam-ledger coordination',
        },
        {
            title: 'Close',
            detail: 'a read-only sol finder fan plus one governance finder per language over the landed corpus and seam ledger; opus slice drains; ONE terminal fable fixer drains findings, backlog, and unclaimed census and idea rows, its finishing round landing the doctrine adjudication (standalone opus lander only as dead-fixer fallback)',
        },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAP = 14;
const STAGGER_MS = 1500;
const STALL = 300000;
const DRAIN_ROUNDS = 4; // terminal drain fixpoint cap; the progress gate (no shrinkage -> stop) is the real bound
const WRAPPER_STALL = 1500000; // stallMs never observes a live blocking MCP call (run-proven: a 43-min blocked wrapper under a 25-min stall survived) — this guards only out-of-call wrapper wedges; the watchdog clocks below are the binding bound
const LANE_CLOCK = 2700000; // codex-lane wall-clock watchdog (~2.5x observed peer median): a nested-call wedge inside codex otherwise holds the slot to the session MCP ceiling
const CRIT_CLOCK = 7500000; // sol critique watchdog: ABOVE the 2h MCP tool ceiling, so the MCP error — not the race — is the normal death path (the conditioned
// recovery ladder then verifies the checkpointed fixlog on disk); a lower race turned naturally-finishing 87-110 min lanes into zombie writers overlapping the
// chained redteam on the same pages. The watchdog now catches only wrappers wedged past the ceiling; it exists for wedges, never depth.
const BATCH_MAX = 8; // unit-segment + batch-packing ceiling; per-segment maps + census legwork carry the navigation, so a writer holds a full dense batch
const FINDER_PAGES = 8; // landed pages per close-phase finder
const CODEX = true; // recon lanes ride the codex wrapper (terra maps, sol-medium finders, sol critique); false restores native lanes

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
    ('rebuild-' + TARGETS.map((t) => t.split('/').pop().toLowerCase()).join('-')).replace(/[^a-z0-9.-]+/g, '-').slice(0, 60) +
    '-' +
    fnv1a(JSON.stringify(TARGETS));

// --- [MODELS] --------------------------------------------------------------------------

const S = { type: 'string' };

const PLAN_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['packages', 'pages', 'vocabularyOwners', 'unresolved'],
    properties: {
        packages: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['name', 'root', 'planning', 'api', 'note', 'feeders'],
                properties: {
                    name: S,
                    root: S,
                    planning: S,
                    api: S,
                    note: S,
                    // Strata-legal upstream package roots (branch-architecture dependency direction), referenced or not; the kernel always rides first.
                    feeders: { type: 'array', items: S },
                },
            },
        },
        pages: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'kind'],
                properties: { page: S, kind: { type: 'string', enum: ['new', 'rebuild'] } },
            },
        }, // ARRAY ORDER IS DEPENDENCY + COHESION ORDER — the engine never re-sorts
        // Corpus-wide registry/vocabulary pages every sibling cites (fault bands, token tiers): downstream lanes cite the owner instead of re-deriving it.
        vocabularyOwners: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['page', 'ownsVocabulary'], properties: { page: S, ownsVocabulary: S } },
        },
        unresolved: { type: 'array', items: S },
    },
};

const ANCHOR_DEFECT = {
    // One anchor = one fact at one coordinate; interpretation never lives in an anchor row. Concurrent lanes move lines
    // constantly, so a bare line number is stale by consumption time: `symbol` (the nearest enclosing heading, member, or
    // fence identifier) is the identity handle joins and dedupe key on, and `quote` (a short verbatim snippet at the
    // coordinate) is the content address a consumer greps to re-find the exact fact after drift.
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'symbol', 'quote', 'role', 'note'],
    properties: {
        path: S,
        line: { type: 'integer' },
        symbol: S,
        quote: S,
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

// Map lanes author ONE content artifact — the dossier — and return only this thin receipt: the per-page jump index into it plus
// coverage attestation. A second full-content representation on the wire or in a report file is double-authoring nobody reads.
const MAP_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['index', 'coverage', 'summary'],
    properties: {
        index: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['page', 'sections'],
                properties: { page: S, sections: { type: 'array', items: S } }, // sections = the dossier headers grounding this page
            },
        },
        coverage: COVERAGE,
        summary: S,
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
                    claimKey: S, // <law>|<owner>|<primary symbol> — stable across lanes, never lane wording
                    page: S,
                    law: S,
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: S, // the observed defect as fact
                    mechanism: S, // WHY it fails the law — factual, zero repair verbs
                    owner: S, // canonical owner that must absorb the resolution
                    reject: { type: 'array', items: S }, // forms the repair must NOT take
                    acceptance: { type: 'array', items: S }, // signals proving resolution
                    files: { type: 'array', items: S },
                    anchors: { type: 'array', items: ANCHOR_DEFECT },
                },
            },
        },
        weak: { type: 'array', items: S },
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

const SEAMS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['file', 'counterpart', 'bothEnds'],
        properties: { file: S, counterpart: S, bothEnds: { type: 'boolean' } },
    },
};

const DELTAS = {
    // navigation facts: what moved, as data, zero adjectives; `key` (<concern>|<owner>|<primary symbol>, stable across lanes,
    // never lane wording) is the handle downstream stages close the row against in `dispositions` — a row continuing an
    // upstream claim REUSES that claim's existing key, never re-mints one.
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['key', 'symbol', 'change'],
        properties: { key: S, symbol: S, change: S },
    },
};

const DEFERRED = {
    // the counted backlog: second-order + live-batch-scope ripples; `key` follows the DELTAS grammar
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['key', 'files', 'claim'],
        properties: { key: S, files: { type: 'array', items: S }, claim: S },
    },
};

const DISPOSITIONS = {
    // The closing rail for upstream rows a stage is OBLIGATED to land — census rows, covering ideas, prior-claim delta and
    // deferred keys — each closed BY KEY with the disk fact grounding its fate, so silence and prose-only dispositions are
    // unspellable. Empty attests the stage had no upstream rows to close.
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['key', 'fate', 'fact'],
        properties: { key: S, fate: { type: 'string', enum: ['landed', 'refuted', 'declined', 'deferred'] }, fact: S },
    },
};

const BEYOND = {
    type: 'array',
    items: { type: 'object', additionalProperties: false, required: ['catalog', 'member'], properties: { catalog: S, member: S } },
};

const INDEXROWS = {
    // doc = index doc, central manifest, or IDEAS.md; row = the exact row text
    type: 'array',
    items: { type: 'object', additionalProperties: false, required: ['doc', 'row'], properties: { doc: S, row: S } },
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

// Required-but-empty arrays are attestations: forced seamsTouched/beyondMap/indexRows/deltas/deferred/dispositions/dossierPhantoms
// make "read fully / exceed the reports / repair both ends / record the backlog / close every obligated row" structurally
// checkable, never wishful prose. ONE fix-log core serves every writing stage; a stage adds only the keys its charter earns —
// twin literals drift, one owner never does.
const LOG_CORE = {
    files: { type: 'array', items: S },
    summary: S,
    seamsTouched: SEAMS,
    deltas: DELTAS,
    deferred: DEFERRED,
    dispositions: DISPOSITIONS,
    beyondMap: BEYOND,
    indexRows: INDEXROWS,
    harvest: HARVEST,
};
const logSchema = (extra) => ({
    type: 'object',
    additionalProperties: false,
    required: Object.keys(LOG_CORE).concat(Object.keys(extra)),
    properties: Object.assign({}, LOG_CORE, extra),
});
// Self-grade verdicts never travel: no downstream stage consumes one, and a "fixed" self-assessment riding the disk
// fixlog into a later reviewer is pure anchoring surface — facts, deltas, and dispositions are the record.
const FIXLOG_SCHEMA = logSchema({
    collapsed: S,
    extended: S,
    dossierPhantoms: { type: 'array', items: S },
});
const REVIEW_SCHEMA = logSchema({ extended: S });

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
                required: ['claimKey', 'target', 'files', 'class', 'severity', 'claim', 'anchors', 'mechanism', 'owner', 'reject', 'acceptance'],
                properties: {
                    // Schema-enforced key grammar: prose alone fractured into 4+ formats across one run's lanes, defeating cross-lane
                    // dedupe; segments derive from the owning CODE SYMBOL and defect class, never the page, slice, or lane wording —
                    // symbol-derived keys are how two finders hitting one defect mint one key (run-proven: page-derived owner segments
                    // minted 6+ divergent keys for identical cross-slice defects and exact-key dedupe collapsed none).
                    claimKey: { type: 'string', pattern: '^[a-z0-9_-]+(\\.[a-z0-9_-]+){3}$' },
                    target: S, // short display label for the defect
                    files: { type: 'array', items: S }, // files the fixer must open or edit first
                    class: { type: 'string', enum: ['missing', 'wrong', 'faked', 'naive', 'drift', 'phantom'] },
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: S, // the observed defect as fact
                    anchors: { type: 'array', items: ANCHOR_DEFECT },
                    mechanism: S, // WHY it fails the law/doctrine — factual, zero repair verbs
                    owner: S, // canonical owner that must absorb the resolution
                    reject: { type: 'array', items: S }, // forms the repair must NOT take
                    acceptance: { type: 'array', items: S }, // signals proving resolution
                },
            },
        },
        coverage: COVERAGE,
        summary: S,
    },
};

const FIXER_SCHEMA = {
    // Required-but-possibly-empty `beyond` is an attestation: the fixer's own hunt ran, not only the signal list.
    type: 'object',
    additionalProperties: false,
    required: [
        'files',
        'indexApplied',
        'resolved',
        'backlogDrained',
        'beyond',
        'rejected',
        'remaining',
        'harvest',
        'tranches',
        'ideaLedger',
        'doctrine',
        'summary',
    ],
    properties: {
        files: { type: 'array', items: S },
        harvest: HARVEST,
        doctrine: {
            // Terminal-round product: performed=true ONLY on the round that empties `remaining` and executes the doctrine
            // landing as its final act; every other round returns performed=false with empty arrays, and a performed=true
            // return is what stands down the fallback lander.
            type: 'object',
            additionalProperties: false,
            required: ['performed', 'landed', 'refined', 'rejected'],
            properties: {
                performed: { type: 'boolean' },
                landed: { type: 'array', items: S },
                refined: { type: 'array', items: S },
                rejected: {
                    type: 'array',
                    items: { type: 'object', additionalProperties: false, required: ['claim', 'reason'], properties: { claim: S, reason: S } },
                },
            },
        },
        tranches: { type: 'array', items: S }, // one checkpoint receipt line per tranche fed this round — a fed tranche absent here is unconsumed
        ideaLedger: {
            // One row per dossier idea — a prose-only silent-drop prohibition has dropped silently; the required ledger makes every fate visible.
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['idea', 'fate', 'note'],
                properties: { idea: S, fate: { type: 'string', enum: ['landed', 'carded', 'declined'] }, note: S },
            },
        },
        indexApplied: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['doc', 'action'], properties: { doc: S, action: S } },
        },
        resolved: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: { target: S, action: S } },
        },
        backlogDrained: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['claim', 'action'], properties: { claim: S, action: S } },
        },
        beyond: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['target', 'action'], properties: { target: S, action: S } },
        },
        rejected: {
            type: 'array',
            items: { type: 'object', additionalProperties: false, required: ['finding', 'reason'], properties: { finding: S, reason: S } },
        },
        remaining: DEFERRED, // rows verified still-open and genuinely blocked; the drain loop re-feeds them until empty or no progress
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
    // LANG carries routing data and engine-parameter rows ONLY — doctrine content is reached through READ_FIRST at the source, never paraphrased here.
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
            'docs/stacks/csharp is the FLOOR, never the ceiling — every fence pushes past it to the strongest form the doctrine ' +
            'admits; the tools/cs-analyzer gate enforces it (a true positive is architecture pressure, a false positive rule ' +
            'pressure, never a suppression).',
        apiTiers:
            'the SHARED substrate catalogs `libs/csharp/.api/*.md` (Thinktecture generated owners, LanguageExt ' +
            'rails/effects/schedules/immutable collections, QuikGraph, Mapperly and siblings) AND the folder catalogs ' +
            '`<package>/.api/*.md` — the universal Thinktecture/LanguageExt rails layered onto the domain packages, never the folder set alone.',
        verify:
            '`uv run python -m tools.assay api` (assay blocked or unavailable: the `.api` catalogs, the nuget MCP for feed ' +
            'truth — version/deprecation lookups only, `get_latest_package_version`-class calls, never a full ' +
            '`get_package_context` dump on a large package — and Context7/exa/tavily for the official surface own the fallback)',
        vocab: '(`[Union]`/`[SmartEnum<TKey>]`/`[ValueObject]`/`Fold`/the rails)',
        slur: 'naive, surface-level code dressed in the right vocabulary',
        illusion: 'a `.api`/host member cited but never verified',
        docBloat: 'XML-doc',
        collapseInto:
            'ONE `[Union]` / `[SmartEnum<TKey>]` / `[ValueObject<T>]` / `[ComplexValueObject]` / source-generated case ' + 'family IN THE SAME FILE',
        gapPkg:
            'LIBRARY_DEPTH: an IFC schema gives a zone quantities, space boundaries, and properties the page never reads — ' +
            'stacking that full surface IS new functionality woven into the owner, not a denser spelling of the same call',
        gapDomain:
            'a BIM zone owns boundary/area/volume, per-kind attributes (a fire compartment a rating, a thermal zone a setpoint, ' +
            'a load group its combinations, an MEP system its medium/flow/pressure), adjacency/nesting topology, and ' +
            'coverage/aggregation/spatial-query operations, never a flat member-id set; a profile owns section properties, grade, ' +
            'fabrication + code-check inputs, not width/height; a durable store owns constraints, indexes, partitions, RLS, ' +
            'migration, and lifecycle, not naive columns',
        ownerGrammar:
            'a CASE in the existing closed family, a ROW or richer data on the existing smart-enum, a FIELD or a composed ' +
            '`[ValueObject]`/`[ComplexValueObject]` on the existing record, an OPERATION on the existing surface, or a POLICY_VALUE on the existing vocabulary',
        deepPkgs: 'LanguageExt/Thinktecture/MathNet/CSparse',
        body:
            'nested `Bind`/`Map` lambda towers where LINQ query syntax or one composed `Eff`/`Fin` pipeline reads flat; ' +
            '`Match(_ => unit)` and swallowed `IfFail` where a typed failure case belongs; manual loop/accumulator plumbing where ' +
            '`Fold`/`Traverse`/`Sequence`/`Partition` compose the join; helper statics and one-off records orbiting an owner',
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
            'docs/stacks/python is the bar and docs/stacks/csharp the density/ambition FLOOR — match its richness, never import C#-shaped idioms.',
        apiTiers:
            'the SHARED/universal branch catalogs `libs/python/.api/*.md` (anyio, expression, msgspec, pydantic, ' +
            'pydantic-settings, beartype, structlog, stamina, numpy, psutil, opentelemetry-*) AND the folder catalogs ' +
            '`<package>/.api/*.md` — the shared rails layered ON TOP OF the folder domain packages, never the folder set alone.',
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
            'tolerance family, not a single linear case; a layer codec owns the full ISO 13567 + NCS discipline/major/minor/status structure, not a flat string',
        ownerGrammar:
            'a CASE in the existing closed `@tagged_union`/`Literal`/`StrEnum` family, a ROW or richer data on the ' +
            'existing `frozendict` table, a FIELD on the existing `msgspec.Struct`/Pydantic model/frozen dataclass/`TypedDict`, an ' +
            'OPERATION on the existing surface, or a POLICY_VALUE on the existing vocabulary',
        deepPkgs: 'the admitted both-tier catalogs (expression/msgspec/pydantic/anyio + the folder domain packages)',
        body:
            'nested try/except and if-ladders where the `expression` Result/Option pipeline or one `match` expression reads ' +
            'flat; bare `except` and silently discarded `Result` where a typed failure case belongs; manual loop/accumulator ' +
            'plumbing where fold/traverse/partition combinators compose the join; module-level helpers and one-off aliases orbiting an owner',
        exhaust: 'total `match` + `assert_never` over the FULL case set',
        modern: 'py3.15-modern only',
        mechanics:
            'MECHANICAL EXECUTABILITY — a fence is a signature-and-implementation contract: mentally compile and type-check each ' +
            'against the real cross-page owners it imports, then hunt these defect classes at their owning doctrine sites and fix ' +
            'each by growing the existing owner: FENCE-PARSES (`language.md` CLOSED_MATCH_SITE) · MODEL-COHERENCE (README ' +
            'CORPUS_LAW) · TOTAL-DISPATCH (`shapes.md` families) · SINGLE-FACT-EVIDENCE (`rails-and-effects.md` STATE_RECEIPTS + ' +
            '`boundaries.md` BYTE_IDENTITY) · LOOP-OFFLOAD (`concurrency.md` OFFLOAD_LANE) · HANDLE-LIFETIME + BINARY-KERNEL ' +
            '(`boundaries.md` CAPSULE_OWNER) · IDENTITY-REGIME (`boundaries.md` MEMO_KEY) · TEMPLATE-SAFETY (`language.md` ' +
            'TEMPLATE_STRUCTURE_SITE) · STREAM-OVER-MATERIALIZE (`iteration.md` LAZY_COMBINATORS) · NO-EXCEPTION-HOTLOOP ' +
            '(`rails-and-effects.md` EXPRESSION_SPINE) · DERIVED-NOT-PARALLEL + PER-MODE PAYLOADS (README DERIVED_LOGIC). The ' +
            'defect definitions live at the sites; read them there.',
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
            '`<folder>/.api/*.md`, cross-checked against the published node_modules types — the shared Effect ecosystem layered ' +
            'ON TOP OF the area packages, never the folder set alone.',
        verify: 'the published types in node_modules (`uv run python -m tools.assay api` over node_modules declarations where a member is novel)',
        vocab: '(`Schema.Class`/`TaggedClass` families, tagged unions, `Effect`/`Layer`, value-derived vocabulary tables)',
        slur: 'naive JavaScript-in-TypeScript dressed in the right vocabulary',
        illusion: '`any`/unsafe `as`/non-null `!` smuggled under a confident surface; a member cited but unverifiable against node_modules',
        docBloat: 'TSDoc',
        collapseInto:
            'ONE deep `Schema.Class`/`TaggedClass`/`TaggedError` family — embedded sub-schemas, brand-in-field ' +
            'refinements, class-carried methods and statics — or ONE tagged discriminated union + exhaustive match, IN THE SAME ' +
            'FILE; CLASS-FIRST: a module-level type alias, interface, or bare `Struct` standing where a class family could carry ' +
            'invariants, statics, and derived projections is a defect, and `Schema.Struct` survives only as an anonymous single-consumer field block',
        gapPkg:
            'BOTH tiers: the shared `libs/typescript/.api/` Effect substrate rails AND the folder domain packages, cross-checked ' +
            'against node_modules; stacking that full surface IS new functionality woven into the owner, not naive Promise/try-catch glue',
        gapDomain:
            'a chart owns scale/axis/series/interaction/annotation families and zoom/brush/tooltip/series-key operations, not ' +
            'two naive renders; a service owns retry/breaker/telemetry/validation/cache layers internally, not a bare fetch; a ' +
            'machine owns hierarchical/parallel regions, guarded transitions, timers, and history as data, not a switch ladder; ' +
            'a projection owns the full transform/diff/patch family the domain needs',
        ownerGrammar:
            'a CASE in the existing tagged discriminated union, a FIELD or embedded sub-schema on the existing ' +
            '`Schema.Class` family, an OVERLOAD or `Function.dual` twin on the existing entrypoint, a STATIC or derived ' +
            'projection on the existing class, a member on the existing `Effect.Service`, a ROW in the existing ' +
            'const-union/table, or a POLICY value on the existing vocabulary',
        deepPkgs:
            'the Effect ecosystem (`Effect`/`Layer`/`Context`/`Schema`/`Stream` + platform/experimental/cluster/workflow/sql/rpc/ai) + the area packages',
        body:
            'nested `Effect.flatMap(Effect.flatMap(...))` and pipe-inside-pipe pyramids where `Effect.gen`/`Do`/one flat pipe ' +
            'owns the sequence; `catchAll(() => Effect.void)` blanket swallows where typed `catchTag`/`catchTags` or a ruled ' +
            'ignore belongs; `flatMap` where `map` serves, manual fold/partition plumbing where ' +
            '`zipWith`/`all`/`validate`/`partition` compose the join, run-and-discard where `tap`/`tapError`/`tapBoth` belongs, ' +
            'sequential steps where `zip`/`all` with concurrency expresses the parallel join; loose module-level consts, ' +
            'aliases, and option-bags orbiting an owner instead of integrating as statics, fields, or derived projections',
        exhaust: 'exhaustive `Match.exhaustive` dispatch (or a checked `never` sink)',
        modern: 'ultra-advanced modern TS only',
        mechanics: '',
        fileOrg: 'apply the `docs/stacks/typescript` file-organization + section-order law',
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms));
// Agent-level slot scheduler: CAP agents in flight across ALL batch chains, staggered launch, work-conserving backfill the moment a slot frees.
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
const RETRY_BACKOFFS = [60000, 1800000]; // agent() returns null causeless, so the ladder covers both death classes: a fast first attempt catches transient transport deaths, the long second waits out a usage-limit window
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
const wopts = (label, phase, model, schema, over) => Object.assign({ label, phase, model, effort: 'high', schema, stallMs: STALL }, over);
const ropts = (label, phase, schema, scope, hl, over) => Object.assign({ label, phase, schema, scope, hl }, over);

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

// Codex dispatch: the sonnet wrapper makes one blocking Codex MCP call.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const laneLaw = (schema, o) =>
    (o.fix
        ? '<completion_bar>\nDone is every page in your named scope worked to its full depth with its product entry written — proof-complete, ' +
          'never effort-spent, never early. A page is DONE the moment its named checks pass and its row is checkpointed to the report file; ' +
          'the run is done when the last page row is written — that write is the terminal act, and no global re-verification sweep, ' +
          'freshness re-check, or post-patch inspection follows it. Complete every named move before yielding; do not stop at analysis or a ' +
          'partial edit. If the chosen approach resists, pick the next-best one and proceed; a move the territory genuinely admits no edit ' +
          'for returns as a deferred row naming its blocker. Your layer is review-and-repair of the named scope; out-of-scope findings ' +
          "follow RIPPLE LAW's four bounds — a first-order ripple your own edit opens is repaired both ends now, everything past it lands " +
          "as a typed deferred/index row. Consolidate a page's edits into the fewest coherent apply_patch blocks — a stream of tiny " +
          'patches re-feeds the whole working context each turn and buys no correctness. Re-verifying unchanged work or re-reading covered ' +
          'territory adds no evidence; a fence that passes its named checks is attested and you move to the next deliverable.' +
          '\n</completion_bar>\n\n<verification>\nVerification is per-page and inline, never a phase: confirm an edit at its changed region ' +
          'once as you make it, confirm nothing the page carried is lost, checkpoint the page row, and move to the next page. A check you ' +
          'did not run is never claimed as run. Once the last page row is written the lane is done — no batch-level freshness re-check, ' +
          'post-patch inspection, or deferred-state re-verification follows; repeat validation of attested work produces no evidence and ' +
          'only spends the lane toward the ceiling. Cross-page freshness and deferred drain belong to the downstream stages.\n' +
          '</verification>'
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
    '\n\n<tool_bounds>\nA nested MCP tool call is bounded: prefer the lightest variant that answers the question (a version ' +
    'lookup over a full package-context dump), give every such call a hard time budget, and when a call does not settle ' +
    'promptly, record the item as a gap/unverified row and move on — an unbounded wait on one lookup never stalls the task.\n' +
    '</tool_bounds>\n\n<output_contract>\nYour final message is a single JSON object with exactly this shape: ' +
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
        '(1) Load the `codex` skill via the Skill tool FIRST — its [09] sessions and recovery law governs this call. Then call ' +
            'ToolSearch with query "select:mcp__codex__codex,mcp__codex__codex-reply", and append one Bash line to `' +
            (ROOT_DIR + '/' + SCRATCH + '/run-telemetry.log') +
            '`: `<utc-iso> | ' +
            label +
            ' | codex-start` (shell `>>` with `date -u +%FT%TZ`; never rewrite the file).',
        '(2) Call the loaded mcp__codex__codex tool ONCE with model="' +
            model +
            '", sandbox=' +
            (o.writes ? '"workspace-write"' : '"read-only"') +
            ', cwd=' +
            JSON.stringify(root) +
            ', config=' +
            JSON.stringify(Object.assign({ features: { multi_agent: false } }, o.codexEffort ? { model_reasoning_effort: o.codexEffort } : {})) +
            ', "developer-instructions" set to the LANE LAW block below VERBATIM, and prompt set to the TASK block below ' +
            'VERBATIM. ' +
            (o.writes
                ? "On any call error run the codex skill's blocking-caller recovery ladder with this lane's disk product at " +
                  report +
                  ' — verify it FIRST (the lane checkpoints it per page; a valid report proceeds to step (4) as success); the ' +
                  'reply nudge tells the session to finish the TASK and write the report file as specified. A fresh identical call ' +
                  'is legal ONLY when the failed call died EARLY — a transport-class error well under this lane' +
                  "'s wall clock; a full-duration ceiling kill means the session did real work and its edits hold the disk, so " +
                  'after the disk verify you return ok=false with the threadId, NEVER a second writer over the same pages.'
                : "On any call error run the codex skill's blocking-caller recovery ladder — this lane writes no product itself, so " +
                  'the reply re-emission of the complete final-message JSON is the first rung and one identical retry the second; a ' +
                  'failed ladder skips step (3) and returns through step (4).'),
        'LANE LAW:\n\n' + laneLaw(schema, o),
        'TASK:\n\n' +
            task +
            (o.writes
                ? '\n\nREPORT FILE (running deliverable, never a terminal dump): ' +
                  report +
                  ' is your product. The moment a page closes its audit-and-repair, rewrite that file with the COMPLETE current ' +
                  'fixlog JSON — every finished page row so far, a whole-object rewrite each time, never an append fragment, so the ' +
                  'file is valid JSON at every checkpoint and a run cut short still leaves every closed page on disk. Your final ' +
                  'message is that same JSON after the last page row lands — a confirming rewrite, never the first write.'
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
        '(4) One Bash append of one line to the same ledger: `<utc-iso> | ' +
            label +
            ' | codex-end | <ok or fail> | <entries> | <threadId from the result envelope>` — the threadId keys the codex-side ' +
            'session record, so it is never omitted. Then parse the tool result text only for mechanical orchestration data. Return ok=true, report=' +
            base +
            '-report.json, entries=the length of result["' +
            o.hl.arr +
            '"], headline="<entries> ' +
            o.hl.arr +
            (o.hl.group ? ' | <' + o.hl.group + ' tallies>' : '') +
            ' | top: <most frequent first file or none>", thread=the threadId from the result envelope, and failure empty. On a ' +
            'second tool error return ok=false, entries=0, report and headline empty, thread=the threadId if any envelope ' +
            'returned one else empty, and failure equal to the error text VERBATIM.',
    ].join('\n\n');
};

// QUOTA FALLBACK: a codex receipt whose failure matches usage/quota/limit re-dispatches the SAME task natively at the role's Claude twin (terra->opus,
// sol->fable, luna->sonnet) — the caller owns the re-dispatch, the sonnet wrapper never executes work itself. The roster row carries `scope` from the
// ORCHESTRATOR (never the lane's self-report) so a failed lane's unmapped territory is exact even when the lane died before writing anything.
const twinOf = (m) => (/-luna/.test(m || '') ? 'sonnet' : 'opus'); // native fallback twins; fable's ONE seat is the terminal fixer, never a fallback
const nativeLane = (task, o) => {
    const report = SCRATCH + '/' + fileTag(o.label) + '-report.json';
    return run(
        task +
            '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
            (ROOT_DIR + '/' + report) +
            ' (Write tool, exactly this absolute path): ' +
            JSON.stringify(o.schema) +
            ' — then return ONLY the receipt: ok, report = ' +
            report +
            ' (this repo-relative form, matching codex-lane receipts), entries count, one-line mechanical headline, failure ' +
            'empty, thread empty.',
        { label: o.label, phase: o.phase, model: o.nativeModel || twinOf(o.model), effort: 'high', schema: RECEIPT, stallMs: o.stallMs || STALL },
    );
};

const recon = (taskOf, o) => {
    // o.native forces the native branch (the opus deep-map lane rides it: a capable native model, not a codex wrapper).
    const task = typeof taskOf === 'function' ? taskOf : () => taskOf;
    const wrapper = {
        label: (o.model && o.model.indexOf('-sol') >= 0 ? 'sol:' : 'terra:') + o.label,
        phase: o.phase,
        model: 'sonnet',
        effort: 'low',
        schema: RECEIPT,
        stallMs: o.stallMs || WRAPPER_STALL,
    };
    // WATCHDOG: the race frees the slot and hands the chain the standard dead-lane shape at the wall-clock ceiling; the abandoned
    // call keeps running harness-side as an ignored zombie (a late report in scratch is harmless), and the codex session stays
    // recoverable through the rollout store. Cancellation does not exist on this surface — slot recovery is the whole point.
    return (
        CODEX && !o.native
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
// Even split: ceil(n/max) batches of near-equal size — no runt tail heavying batch 0 and starving the last.
const evenChunk = (arr, max) => chunk(arr, Math.ceil(arr.length / (Math.ceil(arr.length / max) || 1)));
const pkgOf = (p) => p.split('/.planning/')[0]; // package = the write-partition key (index docs live at its root)
const subOf = (p) => {
    // Sub-folder = the map/batch granularity unit: one mapper pair and one batch-ownership seam per `.planning/<sub>`; root-level pages pool as '_root'.
    const rest = p.split('/.planning/')[1] || '';
    return rest.includes('/') ? rest.split('/')[0] : '_root';
};
const Lof = (pkg) => LANG[langOf(pkg)] || LANG.cs;
// Scratch paths follow one grammar: SCRATCH + '/' + fileTag(<label>) + '-<artifact>'. Seam ledgers key on the batch tag; dossiers key on their recon lane label
const scratchBase = (pkg, i) => SCRATCH + '/' + fileTag(pkg.split('/').pop() + ':b' + i);
const dossierPath = (lensLabel) => SCRATCH + '/' + fileTag(lensLabel) + '-dossier.md';
const normalizePages = (pl) => {
    // Preserves plan emission order (dependency + cohesion order); dedupe by page, first wins.
    const seen = new Set();
    const out = [];
    for (const p of (pl && pl.pages) || []) {
        if (!p || !p.page || seen.has(p.page)) continue;
        seen.add(p.page);
        out.push({ page: p.page, kind: p.kind === 'new' ? 'new' : 'rebuild' });
    }
    return out;
};

const navOf = (logs) => {
    // Navigation handoff: FACTS ONLY — files, symbol deltas, seam rows, backlog, dispositions (each a refutation target for
    // the reviews: a closed row's grounding fact is attackable evidence, not a settled verdict). Never verdicts or adjectives.
    const rows = logs.filter(Boolean);
    return {
        files: [...new Set(rows.flatMap((r) => r.files || []))],
        deltas: rows.flatMap((r) => r.deltas || []),
        seams: rows.flatMap((r) => r.seamsTouched || []),
        deferred: rows.flatMap((r) => r.deferred || []),
        dispositions: rows.flatMap((r) => r.dispositions || []),
    };
};

// --- [SHARED_BLOCKS]

// Every rigor law appears exactly once, here; stages compose subsets. Block order in prompts: stable per-language law first (byte-identical across a
// batch's stages), batch-variable material second, the stage task + output contract LAST — nothing load-bearing mid-prompt.
const ROOT_LAW =
    'WORKING ROOT: ' +
    ROOT_DIR +
    ' — every relative repo path in this brief resolves against this absolute root; read, write, and edit ONLY under it, never ' +
    'another checkout of the repository.';
const CONTEXT = (L) =>
    ROOT_LAW +
    '\n\nRasm monorepo — ' +
    L.corpus +
    '. ' +
    L.strata +
    ' ' +
    L.stackFloor +
    ' Markdown moves through read-and-rewrite edits alone: a scripted transform (`python`, `sed`, `awk`, regex) writing any ' +
    '`.planning` or `.api` file is a defect regardless of output correctness — correct bytes from a banned tool still fail.';

// Register table — one row set per EXECUTING model, keyed by recon()'s dispatch branch. Substance is identical across rows (burden of proof on the
// work, both naivety axes, illusion hunting, no-churn, second-pass self-verify, findings-never-designs); only phrasing forks: claude carries the
// estate hostile register, codex the same demands de-conflicted and neutral — probe-measured: the hostile register makes a codex lane over-read,
// probe out of territory, and spend more input tokens for equal output (the codex skill's prompt-contract law). Register-neutral rows (selfCheck,
// antiAnchor) live once as shared constants — a forked copy is a drift bill with no probe evidence behind it.
const SELF_CHECK =
    'SELF-VERIFY (second pass, before returning): re-derive every entry from disk — re-open each cited anchor and confirm it ' +
    'states what the entry claims, re-verify each member spelling against its catalog, trace each seam to both endpoints. ' +
    'Correct or delete any entry that fails re-confirmation; never return a guess, an assumption, a skimmed summary, or a ' +
    'vague/hedged entry. An absence claim is bounded by what you actually read: full-file coverage grounds a file-wide ' +
    'absence; a partial read grounds only the range read, stated as that range (`<file>:1-NNN searched`), never a whole-file ' +
    'or corpus-wide assertion. Completeness is part of correctness: after the re-read, hunt once more for what the first pass ' +
    'missed — an omitted load-bearing fact is as wrong as a false one.';
const ANTI_ANCHOR = (L) =>
    'ANTI-ANCHOR LAW: your report and dossier carry FINDINGS, never designs — quality defects graded against the doctrine read at source (name the law and the ' +
    L.stack +
    ' pattern whose application would most deeply transform the page — the collapse, the owner form, the rail — never the ' +
    'resulting code) and capability inventory in catalog-anchored spellings; a fence sketch, a prescribed shape, or a pre-ruled ' +
    'design ANCHORS and WEAKENS the rebuild and is a defect — the implement agent rules every design.';
const REG = {
    claude: {
        stance: (L) =>
            'STANCE — every pass is hostile: author, critique, and red-team alike. The pages were authored by ANOTHER engineer ' +
            'and are under adversarial review; hold every fence naive, shallow, or illusory until it survives a real attack — ' +
            'the burden of proof is on the code, never on you. "Mature", "already strong", "good enough", and a prior clean ' +
            'verdict are rejected self-assessments — most of this corpus is ' +
            L.slur +
            '. Dense, confident, package-fluent code is the PRIME suspect for hollowness: disbelieve every claim a fence makes ' +
            'about itself and verify it against the real domain and the catalogued package surface. NAIVETY is a defect on two ' +
            'orthogonal axes: COVERAGE — the owner models a thin slice of its concept (a 2-case family for a 20-case domain, ' +
            'three fields where the concept carries fifteen); APPROACH — an enumerated roster where one parameterized generator ' +
            'should GENERATE the space (the roster demotes to seed DATA over named parameters). ILLUSORY code is the primary target: doctrine vocabulary ' +
            L.vocab +
            ', cited packages, confident prose, hollow body — a phantom (' +
            L.illusion +
            '), a name promising capability the body omits, decorative density carrying nothing, a stub dressed as a finished ' +
            'design. Every collapse-signal list in these prompts is a FLOOR, never the complete set. NO CHURN: an edit requires ' +
            'a named violated law or invariant and the concrete case that breaks it — no reproduction, no edit; a clean verdict ' +
            'earned by an attack that finds nothing is a first-class result, proven by adding nothing.',
        selfCheck: SELF_CHECK,
        antiAnchor: ANTI_ANCHOR,
        ctx: (n) =>
            'TASK: HOSTILE READ-ONLY CONTEXT + SEAM LENS over these ' +
            n +
            ' pages — read-only is the only concession; the hunt is as adversarial as every writing pass (investigate, do NOT ' +
            'edit): ',
        api: (n) => 'TASK: HOSTILE READ-ONLY TWO-TIER STACKING LENS over these ' + n + ' pages (investigate, do NOT edit): ',
        apiVerify:
            'DISBELIEVE the pages — prose claiming a package is ' +
            'composed is verified against the fence body; attack every admitted catalog (both tiers) for the members, combinators, ' +
            'generated surfaces, and native pipelines the concept ADMITS but no fence exploits',
        bar: (n) => 'TASK: HOSTILE READ-ONLY DOCTRINE-BAR ATTACK over these ' + n + ' pages (investigate, do NOT edit): ',
        barAttack: (L) =>
            'attack its quality against the doctrine AT SOURCE — EXTREMELY adversarial: the page is presumed ' +
            L.slur +
            ' until proven otherwise. Hunt',
        finder: (i) => 'TASK: HOSTILE READ-ONLY FINDER, slice ' + i + ' (investigate, do NOT edit).',
        finderStance: 'The landed corpus is presumed defective until your attack finds nothing. ',
        gov: 'TASK: HOSTILE READ-ONLY GOVERNANCE FINDER (investigate, do NOT edit).',
        audit: 'TASK: HOSTILE DOCTRINAL-CONFORMANCE + CAPABILITY AUDIT; fix EACH page in place: ',
    },
    codex: {
        stance: (L) =>
            'REVIEW POSTURE — the pages are unverified work by another engineer: verify every claim a fence makes against the ' +
            'real domain and the catalogued package surface before accepting it; a prior clean verdict or confident prose is ' +
            'not evidence. NAIVETY is a defect on two orthogonal axes: COVERAGE — the owner models a thin slice of its concept ' +
            '(a 2-case family for a 20-case domain, three fields where the concept carries fifteen); APPROACH — an enumerated ' +
            'roster where one parameterized generator should GENERATE the space (the roster demotes to seed DATA over named ' +
            'parameters). Dense, confident, package-fluent code is the prime suspect for hollowness. ILLUSORY code is the primary target: doctrine vocabulary ' +
            L.vocab +
            ', cited packages, confident prose, hollow body — a phantom (' +
            L.illusion +
            '), a name promising capability the body omits, a stub dressed as a finished design. Every collapse-signal list in ' +
            'these prompts is a floor, never the complete set. NO CHURN: an edit requires a named violated law or invariant and ' +
            'the concrete case that breaks it; a clean verdict from a check that finds nothing is a first-class result.',
        selfCheck: SELF_CHECK,
        antiAnchor: ANTI_ANCHOR,
        ctx: (n) => 'TASK: read-only CONTEXT + SEAM LENS over these ' + n + ' pages (investigate, do NOT edit): ',
        api: (n) => 'TASK: read-only TWO-TIER STACKING LENS over these ' + n + ' pages (investigate, do NOT edit): ',
        apiVerify:
            'verify prose claiming a package is composed against the fence body — never accept the ' +
            'claim; check every admitted catalog (both tiers) for the members, combinators, generated surfaces, and native ' +
            'pipelines the concept ADMITS but no fence exploits',
        bar: (n) => 'TASK: read-only DOCTRINE-BAR review over these ' + n + ' pages (investigate, do NOT edit): ',
        barAttack: () =>
            'assess its quality against the doctrine AT SOURCE — the page is presumed below the bar until your review proves ' +
            'otherwise: the burden of proof sits on the page, never on you, and a finding-free verdict is earned only by a ' +
            'review that checked every applicable law and found nothing. Report',
        finder: (i) => 'TASK: read-only FINDER, slice ' + i + ' (investigate, do NOT edit).',
        finderStance:
            'The landed corpus is presumed to carry residual defects until your independent verification finds none; what the ' +
            'run reports about itself is unproven. ',
        gov: 'TASK: read-only GOVERNANCE FINDER (investigate, do NOT edit).',
        audit: 'TASK: DOCTRINAL-CONFORMANCE + CAPABILITY AUDIT; fix EACH page in place: ',
    },
};

const BUILD_LAW = (L) =>
    'BUILD LAW — buildout over removal, always; removal authority covers ONE case, the PHANTOM (a cited member that does not ' +
    'exist). An underutilized catalog, orphan-looking admission, or weak fence is an INTEGRATION target: the capability lands as ' +
    L.ownerGrammar +
    ' — inside the existing owner, reshaped as if always carried — or is wired into its owning sibling page in the same pass. ' +
    'Never a parallel type, a sibling shape, or flat appended code; never extract a file to cut LOC; never regress capability. ' +
    'A NEW page is admitted on one ground only: a genuinely new owner the domain demands that no existing owner can absorb, ' +
    'authored in the TARGET package and wired into the folder seam owners — never as extraction relief. Structural collapse and ' +
    'CAPABILITY completeness are orthogonal — a fully collapsed owner can still model a naive slice; close both. Every extension ' +
    'cites one gap source: PACKAGE — an admitted member the concept admits but the page ignores (' +
    L.gapPkg +
    '); DOMAIN — an attribute, metric, sub-kind, relationship, state, or operation the real concept demands (' +
    L.gapDomain +
    '); CONSUMER — a contract a sibling or downstream owner will require. A genuinely needed NEW external package is admitted ' +
    'additively: author its README registry row and `.api` catalog yourself; report its central-manifest row in `indexRows`. ' +
    'Byte-count is a weak proxy — assess each owner against its full domain and both-tier surface regardless of size, and model ' +
    'it for FIVE TIMES today\'s cases, fields, and consumers: a thin slice built "for now" is the COVERAGE defect by definition. ' +
    'CHANNEL LAW — a canary/beta/pre-release channel is admissible where the bleeding edge genuinely adds capability, judged on ' +
    'capability delta, maintenance signal, and integration merit, pinned exact with the typing posture recorded in the catalog.';

const BODY = (L) =>
    'FENCE-BODY LAW — the interior of every fence is judged at the same bar as its shapes; a correct owner ' +
    'carrying a naive body is a defect. Rebuilt on sight: ' +
    L.body +
    '. The optimal body is dense, flat, ' +
    'expression-shaped, and reads as one algebra — the admitted combinator surface is the material, never hand-rolled ' +
    'control flow, nesting, or extraction to loose helpers.';

// Review stages CONSUME the api-lens dossier instead of re-mining both tiers — the map lane already paid that census.
const VERIFY = (L, consume) =>
    'VERIFY — cite only members confirmed via ' +
    L.verify +
    '; a member you cannot verify is a phantom to delete. ' +
    (consume
        ? "The api-lens dossier already enumerated both tiers' `apiUsed` and `apiUnderutilized` for these pages: verify each " +
          'cited member at its anchor and stack every `apiUnderutilized` row into its owner. Hunt past the dossier only where ' +
          'it attests incompleteness (`beyondMap`) or a fence cites a member absent from it — never a blanket re-mine of ' +
          'catalogs the dossier already covered.'
        : 'Mine BOTH .api tiers to operator depth: ' + L.apiTiers) +
    ' An admitted capability the concept admits that no owner exploits is a defect to close.';

const RIPPLE_LAW =
    'RIPPLE LAW — every fix you identify you make NOW via Edit/Write; the fix-log reports edits already made, never a to-do, ' +
    'a ledger, or a would/should hedge. The writing is YOURS — a delegate may only fetch information. Ripple authority is ' +
    'LIBS-WIDE (any file under libs/, any language, corrective AND generative) under four bounds that are evidence, never ' +
    'radius. (1) EVIDENCE — an out-of-scope edit traces to a resolvable anchor: a seam-ledger row, a consumer anchor, an index ' +
    'claim, or a wire row in the branch ARCHITECTURE.md [02]-[SEAMS] ledger; an anchorless edit is drift, forbidden. ' +
    '(2) EXPAND-FORM — a foreign edit made while sibling batches run is ADDITIVE only (add the case, row, field, operation, or ' +
    'counterpart); renaming, removing, or collapsing a foreign surface is recorded in `deferred` for the terminal fixer, never ' +
    'raced. Wire-canonical names stay frozen; a foreign-language counterpart is repaired at ITS branch doctrine bar (read that ' +
    'branch stack README before a non-trivial foreign edit) with surgical anchored edits, never a foreign-interior rebuild. ' +
    '(3) DEPTH — a first-order ripple (your edit broke or opened it directly) is repaired both ends now and recorded in ' +
    "`seamsTouched`; a second-order ripple or a counterpart inside a concurrent batch's scope is recorded in `deferred` as " +
    '{files, claim} — the fixer drains the backlog this run; nothing drops silently. (4) DECISION/PROPAGATION — ' +
    'decision-carrying shared surfaces are single-writer: the owning-package index docs (ARCHITECTURE.md + README.md at the ' +
    'path before `/.planning/`), IDEAS.md, and the central manifests take exact rows via `indexRows` for the terminal fixer to ' +
    'apply once; distributing an already-ruled contract is yours. GENERATIVE openings: a capability your work opens elsewhere ' +
    'is realized NOW when it lands in expand-form owner grammar on an existing owner; an opening demanding a new owner outside ' +
    'the target package lands as a fully-specified IDEAS row via `indexRows`, never a vague note.';

const CURRENT_STATE =
    'CURRENT STATE — sibling batches land work concurrently with yours. Before any edit, re-read the CURRENT on-disk state of ' +
    'your pages AND every sibling page they compose or ripple into; landed sibling work is picked up as found, never assumed ' +
    'from the dossier snapshot (dossiers ground verified `.api` extracts, never sibling page state). A seam counterpart a ' +
    'sibling landed is COMPOSED, not re-derived; a conflict resolves to the stronger form, never a revert.';

const LEDGER = (base, scopes) =>
    'SEAM LEDGER — cross-batch coordination is typed fact rows on disk, never prose. Your batch ledger is `' +
    base +
    '-seams.md`: append one row PER EVENT AS IT LANDS (a shell `>>` append the moment the edit commits — a ledger written once ' +
    'at the end is invisible to every concurrent sibling and has failed its purpose), each row ' +
    '`<seq> | <batch> | <stage> | <TYPE> | <files> | <fact>` — seq your running counter, stage impl|crit|rt, TYPE one of ' +
    '`SEAM_CHANGED` (a shared name, signature, or contract you own moves: old -> new), `RIPPLE_REPAIRED` (you repaired a ' +
    'counterpart, so no sibling redoes it), `SEAM_CONFLICT` (collision with a landed sibling row; resolve to the stronger form ' +
    'per CURRENT STATE). Before ANY edit outside your batch pages, `ls` `' +
    SCRATCH +
    '/` and read every sibling `*-seams.md` row whose files intersect yours — a RIPPLE_REPAIRED row is work you do NOT redo, a ' +
    'SEAM_CHANGED row a contract you compose. Rows are facts, zero adjectives, authored ONCE: your returned `seamsTouched` ' +
    'rows are these ledger rows verbatim, never a second derivation. CONCURRENT BATCH SCOPES (a counterpart inside ' +
    "another live batch's scope is recorded in `deferred`, never edited): " +
    scopes;

const PROSE_COMMENTS = (L) =>
    'PROSE + COMMENTS — apply docs/standards/style-guide.md, information-structure.md, and formatting.md. The page is a design ' +
    'spec: lead each section with the controlling contract, one idea per paragraph, close on the consequence; no provenance, ' +
    'narration, freshness disclaimers, or hedges. Backtick every symbol, type, field, function, operator, package ID, path, ' +
    'command, flag, and literal; name the exact member over paraphrased behavior; trimming never reduces technical density. ' +
    'Fences comment for the next agent only: keep the canonical section-divider headers; beyond them zero comments, 1-2 lines ' +
    'only for a truly subtle invariant or boundary; no restating the code, no ' +
    L.docBloat +
    ' bloat.';

const INFO_LAW =
    'You provide INFORMATION, never prescriptions: exact disk anchors, the current shape at each site, seam endpoints both ' +
    'sides, verified member spellings, gaps. The implement agent decides how to build; an entry that says what to write ' +
    'instead of what is true is a defect. ENTRY FORM: prose fields carry fact; `anchors` carry one coordinate per row (role ' +
    'names what it proves; `note` is the shortest literal witness under 20 words, or empty when path+line suffice; an ' +
    '`absence` anchor names where the expected thing was searched and not found); `files` lists what the consumer must open. ' +
    'An underutilized-capability entry is INVENTORY: verified members, usage anchors, the admitting concept — composition is ' +
    "the implement agent's call. DOSSIER ANCHOR FORM — in the dossier body an anchor is ONE inline token, `<path>:NNN` " +
    '(a range only when the fact truly spans lines), carried inside its fact row: ' +
    '`- <owner-symbol> omits the <axis> arm the concept admits — <page>.md:NNN | catalog <api-file>.md:NNN` — never a ' +
    'multi-line key/value block per anchor and never a repeated files list per entry; the keyed anchor object exists only in ' +
    'the returned wire JSON. VERIFIED-CORRECT is first-class dossier content: per page, one attestation row per seam or ' +
    'member spelling you re-confirmed correct — the writer spends its re-open budget on flagged rows first; an attestation ' +
    'never waives the consumer re-open behind an edit. COVERAGE is part of the product: `requested` = assigned scope, ' +
    '`read` = actually full-read, `skipped`/`unverified` = not reached — an honest skip beats a silent one.';

const EVIDENCE_LAW =
    'FINDING FORM — you deliver TRUTH, never an implementation: `claim` states the observed defect; `mechanism` states WHY it ' +
    'fails the law as fact; `anchors` carry one coordinate per row (role names what it proves; `note` is the shortest literal ' +
    'witness under 20 words, or empty when path+line suffice; an `absence` anchor names where the expected thing was searched ' +
    'and not found); a `defect` anchor lands ON the line implementing the mechanism — a nearby declaration or a blank line ' +
    'taxes the consumer, whose re-open of every anchor is mandatory — and every anchor path resolves on current disk at emit: ' +
    'a phantom or doubled-segment path is a dead read the consumer inherits; `owner` names the canonical owner that must ' +
    'absorb the resolution (the owning axis, roster, registry, or ' +
    'seam vocabulary — never a new local shape); `reject` lists forms the repair must not take; `acceptance` the signals ' +
    'proving resolution. NEVER write add/replace/implement/promote/delete as instruction — the writer owns the design, you the ' +
    'constraint boundary. `claimKey` follows one mechanical grammar — <package>.<page-stem>.<owner>.<defect-slug>, exactly ' +
    'four dot-separated lowercase segments (e.g. `appui.dashboards.lttb.unwired-decimator`) — so the same defect keys ' +
    'identically from any lane by construction, and cross-lane corroboration of one defect surfaces as one key. BODY PROOF: a claim of ' +
    'inert/discarded/not-emitted/erased behavior anchors the IMPLEMENTING body (the generated partial, delegate, or extension ' +
    'body where the behavior would live), never the call site alone — a call-site-only inertness claim is unverified. ' +
    '`severity` binds to consequence (blocker = run-blocking, major = corpus correctness, minor = local cleanup), never prose ' +
    'confidence — grade honestly across all three; a sweep returning no minors attests none exist, never that minors were ' +
    'promoted. OUTPUT BOUNDS: retention scales with the scope — keep every finding that survives the second hostile pass, ' +
    'lead with the strongest, and 0 is legal only when that pass returns empty, `summary` then naming the probes that produced ' +
    'nothing; never manufacture a finding, never delete a confirmed one. COVERAGE is part of the product: `requested` = ' +
    'assigned scope, `read` = actually full-read, `skipped`/`unverified` = not reached or unconfirmed — an honest skip beats a ' +
    'silent one, and a page read but yielding no finding is still attested in `read`.';

const HARVEST_LAW =
    'HARVEST (required key, usually empty): nominate ONLY findings that generalize beyond this batch — a collapse pattern ' +
    'reusable across folders, a naivety class no doctrine clause names, a review rule that catches the defect BEFORE review, a ' +
    'hard-won cross-surface coupling. Each row: altitude (stacks|reviewer|constitution|planning|readme|laws), lang, claim (the ' +
    'generalized law, one sentence, SYMBOL-FREE — every concrete spelling lives in anchors, so the lander adjudicates the law ' +
    'without re-deriving its locality), anchors (file:line evidence), existingClause (the exact clause it hardens, quoted with ' +
    'its path — or "absent" plus the surfaces searched). A batch-local fix never nominates; an empty array is the normal ' +
    'verdict — the doctrine lander refutes weak rows, so nominate substance, never volume.';

const OWN_PASS = (artifact) =>
    'WORK ORDER — binding, in order: (1) the doctrine and `.api` reads per READ FIRST below; (2) cold-read every target page ' +
    'from CURRENT disk and WRITE your own defect-and-ambition list to `' +
    artifact +
    '` — collapse targets, naivety kills, body rebuilds, design rulings, and every capability the page names or its prose ' +
    'promises but the body omits — the PRIMARY product, a DISK ARTIFACT, not a reading step; (3) the map dossiers, ' +
    'corrections census, bigger-ideas worklist, and every upstream review artifact (a critique fixlog, a crit own-pass, a ' +
    'prior-attempt record) GROUND and widen that list, never scope, substitute for, or cap it; (4) audit and repair. Rung ' +
    '(2) precedes every rung-3 read — upstream review material is rung-3-class, so opening it before your list is written is ' +
    'the same failed rung as skipping the list — and rung-3 sources may only ADD rows to the artifact, each tagged [recon]. ' +
    'Every artifact row is a finding GROUNDED at writing time; an imperative, TODO, or deferred-verification note is not a ' +
    'row — what you cannot ground now you either verify now or omit.';

const CORRECTIONS = (path) =>
    'CORRECTIONS CENSUS (ladder rung 3) — `' +
    path +
    '` carries the folder-wide fix census consolidated from the map lanes, sectioned per sub-folder: drift, phantoms, ' +
    'catalog-true spelling repairs, seam and wire mismatches, wiring gaps. ADDITIONAL, never the plan: after your own pass, ' +
    'land every row that intersects your pages (re-verified on disk — a row disk already resolves is dropped) and leave ' +
    'foreign rows alone; the terminal fixer drains the remainder. A pass that only lands census rows has failed OWN PASS FIRST.';

const IDEAS = (path) =>
    'BIGGER-IDEAS WORKLIST (ladder rung 4) — `' +
    path +
    '` carries per-sub-folder capability AMBITIONS: new dimensions, modalities, families, and operations the domain admits ' +
    'BEYOND correction. Read the entries covering your pages IN FULL and close EVERY covering idea in `dispositions` by its ' +
    'entry ID: a writer lands it at the strongest form disk admits or declines it with the forbidding disk fact or doctrine ' +
    'law; a reviewer re-verifies each upstream `landed` claim holds at FULL strength on current disk — a thin or partial ' +
    'landing is an attack surface you finish, and a covering idea no prior stage closed is yours to close now. An idea ' +
    'neither landed nor justified-declined is a silent loss the next stage charges back. Ambition and information, never a ' +
    'prescription, a design, or a ceiling.';

const readFirst = (L, pkg, dossiers, scoped) =>
    [
        'READ FIRST, IN ORDER, BEFORE ANY EDIT — no fence is judged before this read lands.',
        scoped
            ? // Review-stage doctrine scope: every APPLIED law still reads in full at source; only unexercised remainder drops to
              // on-demand — the blanket full-roster preload overflowed the review window and forced compaction thrash (run-proven).
              '(1) DOCTRINE — enumerate `' +
              L.stack +
              '/` with a real `ls` (never memory), then read the README IN FULL. The audit runs a named law set — the README ' +
              '[02]-[DOCTRINE] laws, the [03]-[COLLAPSE_SCAN] table, OWNER_CHOOSER (`shapes.md` [01]), RAIL_CHOOSER ' +
              '(`rails-and-effects.md` [01]), and the aspect two-weave (`surfaces-and-dispatch.md` AND `rails-and-effects.md`) ' +
              '— read each IN FULL at its source, hold it as fact, conform every fence to it; a summary never substitutes for ' +
              "the read. The exercised set extends mechanically: every doctrine page a bar finding's `law` cites, every page " +
              'a census row cites, and every further root page the README routes that a fence in this batch exercises is read ' +
              'IN FULL at source the moment it enters the set; a page nothing exercises needs no full read this pass — scope ' +
              'is by what the fences exercise, never a fidelity cut on any law you apply, and no law is ever applied from ' +
              'memory.'
            : '(1) DOCTRINE — enumerate `' +
              L.stack +
              '/` with a real `ls` (never memory), then read the README and EVERY root page it routes IN FULL in the README ' +
              '[01]-[ATLAS] order — never a partial, skim, grep-jump, or section-sample; a root page absent from the atlas is ' +
              'still mandatory law. The README [02]-[DOCTRINE] laws, the [03]-[COLLAPSE_SCAN] table, OWNER_CHOOSER (`shapes.md` ' +
              '[01]), RAIL_CHOOSER (`rails-and-effects.md` [01]), and the aspect two-weave (`surfaces-and-dispatch.md` AND ' +
              '`rails-and-effects.md`) are binding law AT THE SOURCE — read it there, hold it as fact, conform every fence to it; ' +
              'a summary never substitutes for the read.',
        L.key === 'cs'
            ? '(1b) Enumerate `docs/stacks/csharp/domain/` with a real `ls` through its router README, then read every shard ' +
              'the page concerns touch — chosen from the enumerated set, never from memory; shard conformance is a hard gate.'
            : '',
        '(1c) ANALYZER LAW — read the repo `.editorconfig` rules for your language: every `error`-severity rule is a COMPILE ' +
            'GATE (`dotnet_style_namespace_match_folder = true:error` means namespace ALWAYS equals folder path); a claim ' +
            'contradicting an error-level analyzer rule is a FICTION to correct, never law to compose.',
        '(1d) LAWS — read `docs/laws/` IN FULL (README + topology + patterns + scars; short registry pages): a topology row ' +
            'whose [SURFACE] your edits touch binds its obligated counterparts into the SAME pass, and every patterns row binds each branch it names.',
        '(2) .API — `ls` BOTH catalog tiers in full — the shared substrate `' +
            L.root +
            '/.api/` AND the folder `' +
            pkg +
            '/.api/` — then read every catalog relevant to these pages, layering the shared rails (' +
            L.deepPkgs +
            ') ON TOP OF the folder domain packages, never the folder set alone.',
        dossiers
            ? 'The grounding dossiers for this batch — `' +
              dossiers +
              '` — carry verified two-tier extracts: Tier-1 verbatim member/seam extracts with `file:line` anchors (read fully; ' +
              'SPOT-VERIFY the anchors — a fake anchor goes in `dossierPhantoms`), Tier-2 pointer rows (path + one-line scope) ' +
              'for the long tail — resolve a pointer with a real read the moment an edit touches its territory, never guess past ' +
              'it. Hunt PAST both lanes — members composed beyond them are enumerated in `beyondMap`. Absent or stale, run the ' +
              'full two-tier `ls`+read yourself.'
            : '',
        '(3) SCOPE — read the owning-package charter (ARCHITECTURE.md + README.md + IDEAS.md) as the INTENT authority for what ' +
            'each page owns and which pages are settled. A charter-settled page is out of scope; every page in your batch is ' +
            'rebuilt to the strongest form the doctrine admits.',
    ]
        .filter(Boolean)
        .join('\n');

const reconBlock = (roster, unmapped) =>
    'RECON REPORTS (ladder rung 2) — the map DOSSIERS are the read product (Tier-1 quoted anchors, Tier-2 pointers); each map ' +
    "report.json is that dossier's per-page INDEX — open it to find your pages' entries, then read those dossier sections, " +
    'never the whole artifact. The bar report IS its own product. The receipts below are navigation, ' +
    'never the product. CONSUMPTION, after your own cold pass per OWN PASS FIRST: (a) UNMAPPED territory below (a dead lens) ' +
    'gets your own cold read — that lens dimension over your pages is yours to derive; (b) read the bar report IN FULL from ' +
    'disk and the dossier sections your pages own, grounding lenses before the bar defect lens; (c) anchors are jump coordinates — re-open ' +
    "every anchor behind an edit (MANDATORY); navigation-only entries re-verify only when touched; (d) a bar finding's " +
    '`mechanism`/`owner`/`reject`/`acceptance` are its constraint boundary — honor the owner and rejected forms, but the ' +
    'DESIGN is yours. The reports POINT; you VERIFY and EXCEED them: compose every `apiUsed` catalog at full operator depth, ' +
    'stack every `apiUnderutilized` {catalog, capability} INTO the owning page as a case, row, field, or operation, and close ' +
    "every bar finding at its law. Confirm coverage against the catalogs your batch's fences cite plus the dossier's " +
    'enumerated set — a fence citing a member outside that set triggers a targeted catalog read, never a blind sweep of every ' +
    'tier. Members ' +
    'composed beyond the reports are enumerated in `beyondMap` — an empty `beyondMap` attests the reports were genuinely ' +
    'complete, never a license to treat them as a ceiling.\nROSTER: ' +
    JSON.stringify(roster) +
    '\nUNMAPPED: ' +
    JSON.stringify(unmapped);

const GIT_GROUND =
    'DELTA GROUNDING — run `git diff --stat` then `git diff -- <your batch pages and their seam files>` before judging; ' +
    '`git status` surfaces new files. The diff is orientation, CURRENT disk is truth — the repo can carry pre-run uncommitted ' +
    "work, so an unfamiliar hunk is verified against disk, never assumed to be this run's.";

const HUNT =
    'HUNT CLASSES: missing (an owner, case, field, seam counterpart, or capability the charter or landed design demands with ' +
    'no counterpart on disk), wrong (landed but contradicting doctrine, charter, or analyzer law), faked (prose asserts what ' +
    'the fence body omits, a name promising capability the body lacks), naive (a thin slice of the concept, an underutilized ' +
    'admitted package, either naivety axis), drift (two landed surfaces disagreeing — page vs sibling vs index doc vs manifest ' +
    'vs .api), phantom (a cited member, page, or anchor that does not exist). Every finding carries a file anchor, names the ' +
    'law or catalog member it violates, and is CONCRETE on current disk — a defect real only under an invented, implausible ' +
    'input is not a finding and is never reported. `claimKey` segments derive from the owning code symbol and defect class, ' +
    'never from your slice, page name, or wording: two lanes hitting the same defect MUST mint the identical key — the fixer ' +
    'dedupes on it exactly. Verify cited external members against the .api catalogs; never trust page prose ' +
    'about itself.';

const preamble = (L, batch, dossiers, ideate, scopes, roster, unmapped, reg, stage) =>
    [
        CONTEXT(L),
        REG[reg].stance(L),
        OWN_PASS(scratchBase(pkgOf(batch[0].page), batch[0].i || 0) + '-' + (stage || 'impl') + '-ownpass.md'),
        BUILD_LAW(L),
        BODY(L),
        VERIFY(L, stage === 'crit' || stage === 'rt'),
        RIPPLE_LAW,
        CURRENT_STATE,
        PROSE_COMMENTS(L),
    ]
        .concat(L.mechanics ? [L.mechanics] : [])
        .concat([
            readFirst(L, pkgOf(batch[0].page), dossiers, stage === 'crit'),
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
            '. The OWNING PACKAGE of a page is the path before `/.planning/`. EXPAND with a real recursive listing per target ' +
            '— run find <target-or-its-.planning-tree> -name *.md; a design page lives INSIDE the .planning tree, so a ' +
            'package-root ls alone NEVER proves an empty page set. Validate against `libs/.planning/planning-targets.md` (a ' +
            'mis-scoped or renamed target is reported in `unresolved`; a deliberately page-less target skips silently). Return ' +
            '`packages` (one entry per owning package: {name, root, planning, api, feeders}). FEEDERS: read the owning ' +
            "branch's `libs/<language>/.planning/` architecture pages (the strata/dependency law) and emit each package's " +
            'strata-legal upstream feeder roots — every package the target may compose under the dependency direction, the ' +
            "branch kernel FIRST, whether or not the target's manifest references it today; a feeder is a package root path, " +
            'never a target of this run. VOCABULARY OWNERS: emit `vocabularyOwners` — the pages owning a corpus-wide registry ' +
            'or vocabulary every sibling cites (a fault-band registry, a token tier), each with its one-line `ownsVocabulary` ' +
            'fact — so downstream lanes cite the owner instead of re-deriving it. PAGES: expand each target — a ROOT to ' +
            'every design page under its planning tree, a SUB-FOLDER to every page under it, a FILE to itself; union + dedup; ' +
            'exclude IDEAS.md/TASKLOG.md/README.md/ARCHITECTURE.md.',
        'SCOPE LAW — the owning-package charter (ARCHITECTURE.md + README.md + IDEAS.md) owns scope: every existing design ' +
            'page under the targets enters as `rebuild`; a page the charter demands but disk lacks enters as `new`; a ' +
            'charter-settled page is SKIPPED, never re-litigated.',
        'EMIT `pages` IN DEPENDENCY + COHESION ORDER — grouped by sub-folder, foundations before consumers, pages sharing an ' +
            'owner, seam, or wire contract ADJACENT within their group (the engine batches contiguous runs of your order, so ' +
            'adjacency keeps coupled pages inside one writer); alphabetical only as the final tiebreak. DEPENDENCY DOMINATES ' +
            'COHESION for cross-cutting owners: a sub-folder owning a registry or vocabulary cited ACROSS sub-folders ranks as ' +
            'a foundation ahead of its consumers, even when same-folder cohesion would place it later — cohesion orders peers, ' +
            'never buries a shared foundation. The engine never re-sorts.',
    ].join('\n\n');

const correctionsPrompt = (L, pkg, mapIndex, dossier) =>
    [
        CONTEXT(L),
        'TASK: CORRECTIONS CENSUS AUTHOR for `' +
            pkg +
            '` — read-only over the corpus; you WRITE exactly one file, the census dossier. The Map phase produced per-SUB-FOLDER ' +
            'deep-map and two-tier .api inventory dossiers; read EVERY dossier listed here IN FULL. Each row carries its ' +
            "sub-folder's complete `pages` roster — the page tree is settled, never re-derived with tree/fd/ls: " +
            JSON.stringify(mapIndex),
        'AUTHOR `' +
            dossier +
            '` — one markdown census, a section per sub-folder. Consolidate every CORRECTION the map lanes surfaced or your own ' +
            'verification finds: drift between surfaces (page vs sibling vs index doc vs manifest vs .api), phantom members, ' +
            'catalog-true spelling repairs, seam and wire mismatches, contradicted analyzer law, and WIRING GAPS (an uncomposed ' +
            'admitted member the page concept plainly demands). ONE ROW GRAMMAR for hit and miss alike: every sub-folder ' +
            'section is a table `[PAGE] | [ANCHOR] | [DEFECT] | [PROOF]`, each row ID-keyed, every defect-site `[ANCHOR]` a ' +
            '`path:line` — never a bare symbol; a defect spanning pages is ONE row whose `[PAGE]` cell lists every site, ' +
            'never a copy per folder; an empty section carries one ' +
            'reserved `[NONE-VERIFIED]` row naming the checks run with their anchors — a bare [NONE] is a lazy miss, never an ' +
            'earned one. CORPUS INVARIANTS live once in a leading sweep section (registry closure, conventions, anchor ' +
            'resolution — checks that hold across all pages); a row carrying a page-specific defect anchor NEVER lands in the ' +
            "sweep — it belongs to its owning folder section, so a batch writer's folder-indexed read is COMPLETE for its " +
            'pages; folder sections assert only folder-local closure and never ' +
            'restate the sweep. DEDUPE across dossiers; VERIFY each row on ' +
            'current disk before writing it — a row disk already resolves is dropped. FORBIDDEN: new-capability ambitions (the ' +
            'ideas lane owns them — a row that widens what the package IS is dropped here, not diluted into a fix), ' +
            'prescriptions, fence sketches, removal framing, process or self-verification narration, self-counts in headers.',
        REG.claude.selfCheck,
        'Return ONLY the receipt: ok, report = the census path (repo-relative), entries = the row count, a one-line headline, failure empty.',
    ].join('\n\n');

const ideasPrompt = (L, pkg, mapIndex, dossier) =>
    [
        CONTEXT(L),
        'TASK: BIGGER-IDEAS AUTHOR for `' +
            pkg +
            '` — read-only over the corpus; you WRITE exactly one file, the ideas dossier. The Map phase produced per-SUB-FOLDER ' +
            'deep-map and two-tier .api inventory dossiers; read EVERY dossier listed here IN FULL, then the package charter ' +
            '(ARCHITECTURE.md + README.md + IDEAS.md) and the pages your ideas grow on — dossiers FIRST, then charter, then ' +
            "pages; each row carries its sub-folder's complete `pages` roster, so the page tree is settled, never re-derived " +
            'with tree/fd/ls: ' +
            JSON.stringify(mapIndex),
        'AUTHOR `' +
            dossier +
            '` — one markdown dossier, a section per sub-folder plus a terminal CROSS-FOLDER section. Every entry is a NEW ' +
            'capability the domain admits and the corpus lacks — a new dimension, modality, family, case class, operation ' +
            'family, generator over an enumerated space, or cross-boundary enablement that widens what the package IS — ' +
            'grounded in real domain demand (' +
            L.gapDomain +
            ') and the admitted two-tier package depth. VALUE BAR — the census/ideas partition is severity of imagination, not ' +
            'anchor quality: a stale label, wrong spelling, dropped wire column, or single uncomposed member is a CORRECTION ' +
            'and drops here; an entry earns its row by naming capability whose absence a domain expert would call a gap in the ' +
            "PRODUCT, not a defect in the prose; an entry whose ground reuses another entry's owner and catalog member with no " +
            'new modality MERGES into that entry — size governs by collapse, never by count. Every entry OPENS with one ' +
            'structured lead line — `owner-page | ground | anchors | size (S|M|L) | counterpartState (landed | must-build, ' +
            'per end)` — then the WHY prose: why it widens the package — never the resulting code, a fence sketch, or a ruled ' +
            'shape; the lead line lets the batch writer triage build-vs-decline at a glance, and each anchor states whether ' +
            'its counterpart is landed on disk or must be built. CROSS-FOLDER: an idea whose ' +
            'value crosses a sub-folder or package boundary names BOTH ends and the seam it rides in the terminal CROSS-FOLDER table, AND ' +
            'carries a one-line back-reference in its own sub-folder entry — a batch writer discovers its cross-folder ' +
            'obligations inside its own section, never by scanning the terminal table.',
        "SECOND-PASS CULL (before returning): re-open every entry's anchor on disk; delete any entry disk already realizes, " +
            'any correction in disguise, and any entry whose value you cannot state as a concrete new capability of a named ' +
            'owner. Boldness is never the cull criterion — the cull removes false and small entries, never ambitious ones; few ' +
            'large ideas beat many verified trivia.',
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
            '. For a rebuild page read the page IN FULL; for a `new` page read its concept in the owning-package charter plus ' +
            'its nearest siblings. Read the folder at large — the sibling pages each composes and the index docs — as full-file ' +
            'reads. THE DOSSIER IS YOUR SOLE CONTENT ARTIFACT — write `' +
            dossier +
            '` with one `## <page>` section per page carrying: `owns` (the ONE ownership boundary sentence — which ' +
            'owner/vocabulary/concern THIS page owns versus its siblings, so no two concurrent writers author the same ' +
            'polymorphic surface), `contextNote` (sibling owners/seams composed, folder position, folder-wide gaps routed here, ' +
            'plus DOMAIN gaps — attributes, sub-kinds, states, relationships, operations the real concept demands that the page ' +
            'omits, as named gaps), `seams` (every cross-page and cross-package symbol/wire/consumer edge, both endpoints ' +
            'named, each row carrying a STATUS verdict — realized-both-ends | one-end-open | unverified — so the writer ' +
            'partitions its re-open budget), each fact per the ENTRY FORM with typed anchors. CROSS-PACKAGE RELEVANCE: also mine ' +
            'what the OTHER packages hold that is relevant to each page — kernel and sibling-package owners it composes or its ' +
            'concept plainly touches, imports, consumer sites, ripple targets both ends — as verified anchored seam facts, so a ' +
            'writer NAVIGATES (trust, then verify at the anchor) instead of exploring; relevance is fact, never a suggested ' +
            'change. Ground the sections in Tier-1 quoted evidence — the branch ARCHITECTURE.md [02]-[SEAMS] rows covering ' +
            'these pages verbatim with `file:line` anchors, folder-context and charter-intent anchors — with Tier-2 pointer ' +
            'rows (path + one line) for every sibling page composed. FORBIDDEN: doctrine digests, removal framing, unanchored ' +
            'claims, prescriptive designs. Return ONLY `index` (each page mapped to the dossier section headers grounding it — ' +
            'the consumer jumps by page, never walks the artifact) + coverage + summary; content lives in the dossier alone, ' +
            'never restated on the wire.',
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
            '. THE CENSUS IS EVERYTHING THESE PAGES CAN CONSUME, never the two nearest directories: `ls` BOTH catalog tiers in ' +
            'full — the shared substrate `' +
            L.root +
            '/.api/` AND the folder `' +
            pkgOf(batch[0].page) +
            "/.api/` — then read the folder's own manifest IN FULL (every referenced package is a source, catalog or not — an " +
            'admitted package with no catalog still censuses via ' +
            L.verify +
            ') and DIFF the central package manifest against it: a central-manifest package absent from the folder manifest ' +
            'whose domain fits these pages is a CANDIDATE row — {catalog, capability} with the package name, the domain fit, ' +
            'and each admitting page; the writers own the adoption call, never you. Read every catalog relevant to these pages ' +
            'and DIFF the complete admitted inventory against the whole folder: ' +
            REG[reg].apiVerify +
            ' — a capability no page exploits is a named integration gap ROUTED to EVERY page whose concept admits it, never ' +
            'one "best" owner alone; a member FAMILY no page in the folder touches at all is the HIGHEST-value row — an ' +
            'extension seed for capability the concept admits but the folder never built, graded equally with corrections ' +
            '(seed the API surface, never the design). SINGLE-CONSUMER EXPANSION: a package with a catalog at ANY tier ' +
            'consumed by only ONE page is expansion pressure on its siblings — name the package, its unexploited members in ' +
            'exact spellings, and each candidate page. Discovery has ZERO removal authority: an underutilized catalog is ' +
            'always a buildout target, never removal evidence. DEPTH GRADING: a composed member counts as underutilized when ' +
            'the usage is shallow — one call where the surface carries a family, a default-arg call where the policy axis ' +
            'matters, a scalar use of a batch/stream-capable member; grade used-but-shallow with the same {catalog, ' +
            'capability} rows as unused. THE DOSSIER IS YOUR SOLE CONTENT ARTIFACT — write `' +
            dossier +
            '` with one `## <page>` section per page carrying `apiUsed`, `apiUnderutilized` ({catalog, capability}: exact ' +
            'catalog-anchored spelling + integration shape as fact), the candidate rows, and `stackingInventory` (capability ' +
            "names + the doctrine patterns the page's concept admits, as inventory fact — never a prescribed design), each " +
            'fact per the ENTRY FORM with typed anchors; verify every cited member via ' +
            L.verify +
            ' — never list a phantom, and RAIL HONESTY binds every verification claim: when the member-verification rail is ' +
            'unavailable in your sandbox, mark affected rows catalog-anchored-only, never verified — a member confirmed only ' +
            'against catalog prose has not been verified. A page section with zero `apiUnderutilized` rows carries the checks ' +
            'that earned the emptiness (catalogs read, families diffed) — a silent empty is a lazy miss, never an earned one. ' +
            'Ground the sections in Tier-1 quoted `.api` member blocks with `file:line` anchors for ' +
            'every cited member plus the manifest rows read, with Tier-2 pointer rows carrying CAPABILITY SCOPE (catalog path ' +
            '+ the capability cluster it holds for these pages), never a bare filename inventory — a "not cited by these ' +
            'pages" dismissal row is a FAILED entry; a catalog with genuinely zero bearing is silence. FORBIDDEN: doctrine ' +
            'digests, unanchored claims, prescriptive designs, `ls`-dump listings. Return ONLY `index` (each page mapped to ' +
            'the dossier section headers grounding it — the consumer jumps by page, never walks the artifact) + coverage + ' +
            'summary; content lives in the dossier alone, never restated on the wire.',
    ].join('\n\n');

// Feeder lanes map an UPSTREAM package's surface projected onto the run targets: what the targets hand-roll that the feeder owns, what feeder
// depth could extend them, and where each target's manifest stands. Dossier sections and index rows key on the TARGET package root, never a page.
const feederLensPrompt = (L, feeder, targets, dossier, reg, lane) =>
    [
        CONTEXT(L),
        REG[reg].stance(L),
        VERIFY(L),
        INFO_LAW,
        REG[reg].selfCheck,
        'TASK: FEEDER ' +
            (lane === 'ctx' ? 'DEEP MAP' : 'API INVENTORY') +
            ' of upstream package `' +
            feeder +
            '` PROJECTED onto the run targets ' +
            JSON.stringify(targets) +
            '. Ground in the branch architecture first: read `' +
            L.root +
            "/.planning/` (README + ARCHITECTURE — the strata and dependency law) so the projection follows the branch's " +
            'feed direction. ' +
            (lane === 'ctx'
                ? 'Read the feeder charter (README + ARCHITECTURE) and its .planning page roster; per TARGET package author a ' +
                  'dossier section keyed by the target root: `owns` = the feeder owner surface, `contextNote` = the relevance ' +
                  'FACT — a target concept the feeder already owns (hand-rolled duplication pressure), a feeder owner the ' +
                  'target could compose to extend or deepen a concept, or a wire shape both must honor — and `seams` naming ' +
                  'both endpoints. Relevance is verified fact with anchors, never a suggested change; a feeder surface with no ' +
                  'plausible bearing on any target is silence, not a section.'
                : 'Census the feeder API surface COMPLETELY: `ls` the feeder `.api/` catalogs and the shared substrate tier `' +
                  L.root +
                  "/.api/`, read every catalog whose domain touches a target concept, and read each target's folder manifest " +
                  'plus the central manifest — per TARGET package author a dossier section keyed by the target root: `apiUsed` ' +
                  '= feeder members the target pages already compose, `apiUnderutilized` = {catalog, capability} rows for deep ' +
                  'members the target does not exploit (correction pressure AND extension seed — capability the target ' +
                  'concept admits for NEW functionality counts equally), `stackingInventory` = the capability clusters + ' +
                  'manifest status as fact (referenced in the target manifest, or available in the central manifest only — ' +
                  'name the missing reference row; the writers own the adoption call, never you). Verify every cited ' +
                  'member via ' +
                  L.verify +
                  '; never list a phantom.') +
            ' THE DOSSIER IS YOUR SOLE CONTENT ARTIFACT — write `' +
            dossier +
            '` — Tier-1: quoted anchor blocks with `file:line` for every cited surface; Tier-2: pointer rows (path + one-line ' +
            'scope) for the remainder, sectioned per target package. FORBIDDEN: doctrine digests, unanchored claims, ' +
            'prescriptive designs, bare package listings (a list without member depth and target bearing is a failed lane). ' +
            'Return ONLY `index` (each target root mapped to its dossier section headers) + coverage + summary; content lives ' +
            'in the dossier alone, never restated on the wire.',
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
            '/` with a real `ls` and read the README and EVERY root page it routes IN FULL' +
            (L.key === 'cs' ? ' (and the domain/ shards these pages touch)' : '') +
            '; then read each target page IN FULL and ' +
            REG[reg].barAttack(L) +
            ' where doctrine is not followed AND where a doctrine law applies more deeply for a stronger form: collapse signals ' +
            'ungathered, owner forms weaker than the discriminants demand, rails split or dual-paradigm, knobs where policy ' +
            'values belong, naive bodies below the admitted combinator surface, declared-but-unread columns (a schema column, ' +
            'field, or policy row no fence reads is illusory capability), ' +
            L.docBloat +
            ' bloat, file-organization drift, both naivety axes. Return per-page `findings` in the FINDING FORM, EMITTED IN ' +
            'PAGE ORDER — `law` names the doctrine law at its source, `claimKey` = <law>|<owner>|<primary symbol>, typed ' +
            '`anchors` at exact coordinates — and `weak` (pages whose overall verdict is weak). Grade `severity` across the ' +
            'full scale: a compile-break or phantom member is a blocker, a genuinely page-local cleanup is minor — a report ' +
            'whose every finding shares one grade has averaged, not graded. Findings name the law and the defect, NEVER the ' +
            'resulting code — the implement agent rules every design.',
    ].join('\n\n');

const implementPrompt = (L, batch, dossiers, ideate, scopes, roster, unmapped) =>
    preamble(L, batch, dossiers, ideate, scopes, roster, unmapped, 'claude')
        .concat([
            'TASK: HOSTILE IMPLEMENT of these ' +
                batch.length +
                ' pages IN PLACE, each per its kind: ' +
                batch.map((p) => p.page + ' [' + p.kind + ']').join(', ') +
                '.\n' +
                'kind=`new`: GROUND-UP AUTHOR the page (it may open a new sub-folder) to the full doctrine + domain-complete ' +
                'capability bar, in the code-fence-first form of its mature siblings, wired into the folder entry/receipt seam ' +
                'owners. kind=`rebuild`: HOSTILE GROUND-UP REBUILD in place. Before authoring EACH page, restate in one line the ' +
                'owner it holds, the seams and frozen wire names it must honor, and the doctrine laws that bind it — then build ' +
                'against that restatement. Construct in LIFECYCLE order — admit raw once, canonical owner by OWNER_CHOOSER, ' +
                'stacked rail/aspect over a thin pure core, projection, egress, BOTH ingress and egress parameterized; collapse parallel shapes into ' +
                L.collapseInto +
                '; one polymorphic entrypoint per modality. CAPABILITY-COMPLETENESS IS MANDATORY, NOT OPTIONAL: for every owner ' +
                'you author, the body implements what its names and prose promise — a named-but-omitted capability is a defect ' +
                'you close NOW, at the same bar as a bar finding, and the BIGGER-IDEAS entries your pages own resolve at the ' +
                'strongest form disk admits or close as `declined` in `dispositions` with the forbidding fact (rung 4 is an ' +
                "obligation with a typed ledger, never a tail you may skip). COMPOSE the map DOSSIERS' `apiUsed` sections at full operator " +
                'depth, STACK every dossier `apiUnderutilized` row into the owner (the wire reports carry only indexes — the ' +
                'dossier sections are the API channel), CLOSE every bar finding at its law, CONFIRM no other admitted catalog is ' +
                'missing, CLOSE the concept capability gaps per BUILD LAW, and land every CORRECTIONS CENSUS row intersecting ' +
                'your pages — each intersecting row closed in `dispositions` BY ITS KEY (landed | refuted with the disk fact | ' +
                'deferred to its concurrent owner), so no row goes silent. ' +
                L.modern +
                '; ' +
                L.fileOrg +
                '; high-signal all-backticked prose. Write the complete fix-log JSON to `' +
                scratchBase(pkgOf(batch[0].page), batch[0].i || 0) +
                '-impl-fixlog.json` as your final act, then return the SAME fix-log — one authored record, two transports; ' +
                '`deltas` carries every moved symbol/wire as data, ' +
                '`deferred` the backlog rows, both exact. ' +
                HARVEST_LAW,
        ])
        .join('\n\n');

const critiquePrompt = (L, batch, dossiers, ideate, scopes, roster, unmapped, nav, reg) =>
    preamble(L, batch, dossiers, ideate, scopes, roster, unmapped, reg, 'crit')
        .concat([
            'NAVIGATION (facts from the pass that landed these pages — locations only, no assessments; it changes where you look ' +
                'FIRST, never what you conclude): ' +
                JSON.stringify(nav),
            GIT_GROUND,
            REG[reg].audit +
                batch.map((p) => p.page).join(', ') +
                '. Your own-pass artifact (OWN PASS FIRST above) precedes NAVIGATION; then use the navigation to reach every ' +
                'touched seam fast. Audit every fence against the ' +
                'doctrine read at source, never a summary; repair every hit now — a fix, never a ledger note; a cross-file hit is ' +
                'yours per RIPPLE LAW. Your mandate is PREDICATE-POSITIVE: verify each required law holds, cite the clause, ' +
                'repair every miss.\n' +
                '- COLLAPSE_SCAN: run the README [03] table on every fence — any signal triggers the move; shapes sharing an ' +
                'identity regime, admission path, payload timing, or consumer collapse into ONE owner, and a shape survives only ' +
                'on a genuinely distinct discriminant; the table is a FLOOR you hunt past.\n' +
                '- OWNER_CHOOSER (`shapes.md` [01]): re-derive every shape from the 5 discriminants — admission, identity regime, ' +
                'variant arity, payload timing, openness — and replace any non-discriminant-correct owner; kill every parallel ' +
                'DTO, one-field wrapper, field-rename shape, and null/default ghost.\n' +
                '- KNOB_TEST: delete each parameter — where the value reconstructs it, collapse the knob to a policy value or ' +
                'input-shape discriminant; move every timeout/retry/deadline off the signature onto the carrier or a ' +
                'composition-time aspect.\n' +
                '- ASPECTS (`surfaces-and-dispatch.md` AND `rails-and-effects.md`), RAILS + closed-fault + accumulate-vs-abort ' +
                '(`rails-and-effects.md` [01]), STRATA/MEMBERS (' +
                L.modern +
                '; both .api tiers maximized per VERIFY; ' +
                L.fileOrg +
                '): audit each at its owning page.\n' +
                '- SEAM ALIGNMENT: every cross-page symbol the batch composes is checked against the sibling owner as it NOW ' +
                'stands on disk — a landed counterpart is composed, a signature mismatch corrects at the weaker end, a conflict ' +
                'resolves to the stronger form, never a revert.\n' +
                '- CAPABILITY-COMPLETENESS + ILLUSION: verify the body implements what names and prose promise; close any ' +
                'admitted capability the owner omits per BUILD LAW; attack both naivety axes.\n' +
                'Return the batched fix-log — `deltas` and `deferred` exact, `dispositions` closing every census row, covering ' +
                'idea, and prior-claim key you were obligated to land. HARVEST (required key, usually empty): only ' +
                'findings that generalize beyond this batch, per the fixlog schema row grammar — substance, never volume; the ' +
                'red-team is the standing harvest author, so nominate only what its pass would otherwise lose.',
        ])
        .join('\n\n');

const redteamPrompt = (L, batch, dossiers, ideate, scopes, roster, unmapped, nav, critOk, critReport) =>
    preamble(L, batch, dossiers, ideate, scopes, roster, unmapped, 'claude', 'rt')
        .concat([
            'NAVIGATION (locations only, no assessments): ' + JSON.stringify(nav),
            'PRIOR CLAIMS (UNVERIFIED): the sol critique fixlog at ' +
                critReport +
                (critOk
                    ? ''
                    : ' (its wrapper receipt died; the checkpointed fixlog holds every page closed before death — check the path ' +
                      "FIRST. Absent, unparseable, or short of this batch's roster, the critique's own-pass at `" +
                      scratchBase(pkgOf(batch[0].page), batch[0].i || 0) +
                      '-crit-ownpass.md` is the surviving claims set for the uncovered pages: mine it as an unlanded-work ' +
                      'CHECKLIST — every finding it raises for your pages is a prior claim you verify landed or land yourself — ' +
                      'and route its harvest-grade findings through YOUR `harvest` with each claim prefixed `rescued:`, the one ' +
                      'transport a dead fixlog leaves; with neither file on disk your cold attack is the only review this batch ' +
                      'gets: judge from CURRENT disk alone)') +
                ' — read it IN FULL; its edits and verdicts are refutation targets judged against CURRENT disk, never a ' +
                'settled record. FOLD-FORWARD DUTY: its surviving `seamsTouched`, `deltas`, `deferred`, `beyondMap`, and ' +
                '`indexRows` rows fold into YOUR return AND your seam-ledger file as TYPED ROWS, each re-stamped with its key ' +
                'and crit provenance — a surviving row rendered as prose is Close-invisible and a FAILED fold — re-verified ' +
                'against current disk and deduped, and every prior-claim row closes in `dispositions` BY ITS KEY — your ' +
                "fix-log is the batch's consolidated record. Its `harvest` rows are NOT yours to fold: the doctrine lander " +
                'sweeps every critique fixlog from disk directly — nomination transport never rides a living fold.',
            GIT_GROUND,
            'TASK: ADVERSARIAL ARCHITECT RED-TEAM; fix EACH page in place: ' +
                batch.map((p) => p.page).join(', ') +
                '. Assume the author and critique missed things and their claims are wrong until disk proves them. Your own-pass ' +
                'attack artifact (OWN PASS FIRST above) precedes the claims. Your mandate is ' +
                'PREDICATE-NEGATIVE — a pre-mortem, never a second conformance audit:\n' +
                '(A) COUNTERFACTUAL on the core owner/algebra/dispatch — a counterfactual REBUILDS the design with its central ' +
                'assumption removed, never merely questions it: name the assumption the current shape stands on (the chosen ' +
                'owner kind, the hand-enumerated space, the call-site dispatch, the hand-rolled kernel), derive the form the ' +
                'page takes WITHOUT it — a denser owner (' +
                L.collapseInto +
                '), a derived table, a parameterized generator over the enumerated space, a deeper admitted-package primitive (' +
                L.deepPkgs +
                ') — and where the rebuilt form is stronger, BUILD IT IN PLACE; a stronger design once seen is never defended ' +
                'against, and "the current shape also works" is not a refutation. The counterfactual extends to every ' +
                'COMPOSITION decision: re-derive each import and admitted-package choice — an overlooked lower-stratum owner ' +
                'or catalog capability that elevates a functionality or deletes hand-rolling is a rebuild you take; collapse ' +
                'consumer overhead by internalizing orchestration behind the entry point (policy resolution, routing, ' +
                'lifecycle automatic — never pushed onto callers, never at capability cost); pressure every owner against ' +
                'growth — parameterization, downstream consumers, entry-point shape — at the 5x-demand future. ' +
                '(B) ANTICIPATORY_COLLAPSE — compute the diff of the next feature: the next case/dimension/modality lands as ' +
                'one row with every consumer untouched or loudly broken (' +
                L.exhaust +
                '). (C) LONG-TAIL — empty/singular/plural/stream/malformed/concurrent/cancelled/partial-failure/version-skew; ' +
                'accumulate-vs-abort correct for the real boundary; ingress AND egress parameterized. (D) BOUNDARY/STRATA — ' +
                'grade every concern against `libs/.planning/architecture.md` and the branch ARCHITECTURE.md [02]-[SEAMS] ' +
                'ledger (read the ledger, never a summary): a concern owned twice, a downward dependency, a host-type leak, or ' +
                'coupling to a sibling interior is fixed both ends per RIPPLE LAW. (E) SPRAWL + PHANTOMS — hand-re-derived ' +
                'package capability, flat code below the operator depth the packages reach, a phantom member (delete), a thin ' +
                'wrapper; and the inverse: an edit this run made that ADDED surface where doctrine demands collapse is ' +
                'regression you rebuild denser; a verified `beyondMap` member absent from its owning `.api` table is catalog ' +
                'DRIFT you patch in-pass, never a report-only row. (F) CAPABILITY-COMPLETENESS + ILLUSION per STANCE and BUILD LAW. ' +
                "(G) GENERATIVE — the capability this batch's work opens at other levels or languages per RIPPLE LAW: realize " +
                'owner-grammar openings now, land new-owner openings as fully-specified IDEAS rows via `indexRows`. Then a FULL ' +
                'COLD RE-REVIEW of every conformance dimension by name — COLLAPSE_SCAN, OWNER_CHOOSER, KNOB_TEST, ASPECTS, ' +
                'RAILS, ' +
                L.modern +
                ', ' +
                L.fileOrg +
                ', both-tier .api maximization, prose + comment hygiene — each judged against CURRENT disk, with the doctrine ' +
                'as the FLOOR, never the ceiling: a fence that merely conforms is unfinished where a stronger form exists. ' +
                'VERIFY every PRIOR ' +
                'CLAIMS seam landed BOTH ends; make any missed repair yourself. Return the batched fix-log — `deltas` and ' +
                '`deferred` exact, `dispositions` closing every prior-claim key and covering idea. ' +
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
            ' The run just landed a hostile rebuild over these pages: ' +
            JSON.stringify(pages) +
            '. ' +
            REG[reg].finderStance +
            'Read each page IN FULL fresh from CURRENT disk, plus the sibling owners each composes and every .api catalog its ' +
            'fences cite (both tiers) — understand what the run changed before judging it. Hunt the classes above across the ' +
            'slice. SMOOTHING: additionally hunt cross-page duplication and split-brain — two pages (or a page and a sibling ' +
            'owner) modeling one concept in parallel shapes, a concern split across pages that one owner should hold, a collapse ' +
            'the run left ungathered — each a `drift` or `naive` finding routed to the canonical owner. SEAM SIGNALS (verify ' +
            'BOTH ends on current disk; an end missing on disk is a finding): ' +
            JSON.stringify(seams) +
            '. STALE DISCARD: judge only CURRENT disk — a defect already resolved, at either end of a seam, is DROPPED, never ' +
            'reported. Findings are INFORMATION for the terminal fixer: name the defect, the law or catalog member it violates, ' +
            'the exact anchor, and the grade — never the resulting code, a fence sketch, or a ruled design. Return typed ' +
            'anchored graded findings.',
    ].join('\n\n');

const govFinderPrompt = (L, pkgs, pages, rows, seams, backlog, reg) =>
    [
        CONTEXT(L),
        HUNT,
        EVIDENCE_LAW,
        REG[reg].selfCheck,
        REG[reg].gov +
            " Audit the owning packages' index surface end to end for these packages: " +
            JSON.stringify(pkgs) +
            " — each package's README.md and ARCHITECTURE.md (at the path before `/.planning/`), its central manifest rows, and " +
            'its `.api/` anchors — against the landed page set: ' +
            JSON.stringify(pages) +
            '. REQUIRED PROBE — the manifest triangle: README package claims, the folder project manifest rows, and the central ' +
            'package manifest are cross-checked THREE ways (a README-advertised package missing from the folder manifest, a ' +
            'folder row absent from the central manifest, a central version with no consumer in the folder) — every leg walked, ' +
            'never sampled. A disagreement between any two surfaces is a `drift` finding; a claim about a landed page is verified against the ' +
            'page on CURRENT disk, never against the claim. REQUIRED PROBE — the seam registry: cross-check each package ' +
            'ARCHITECTURE.md seam map against the branch `' +
            L.root +
            '/.planning/ARCHITECTURE.md` [02]-[SEAMS] registry — a cross-package edge present in one and absent from the other ' +
            'is a `drift` finding. REQUIRED PROBE — foreign counterparts: for every row below whose file or counterpart lies ' +
            'OUTSIDE the audited packages, read that foreign page on CURRENT disk and grade the obligation — already satisfied ' +
            "(drop), real and open (a finding at the owning end), or MIS-FRAMED because it contradicts the foreign owner's " +
            "standing ruling (a `wrong` finding at the row's origin page, never a demand on the foreign owner). SEAMS: " +
            JSON.stringify(seams) +
            ' DEFERRED: ' +
            JSON.stringify(backlog) +
            '. PENDING INDEX ROWS — the terminal fixer applies these after you; a ' +
            'gap these rows already close is DROPPED, not reported: ' +
            JSON.stringify(rows) +
            '. Return typed anchored graded findings.',
    ].join('\n\n');

// Slice drain: one writer per ok page-slice finder report — page-disjoint by construction, so the drains run concurrent; index
// docs, manifests, and cross-slice territory stay the terminal reconciler's alone and reach it as deferred/index rows.
const drainPrompt = (L, f) =>
    [
        CONTEXT(L),
        GIT_GROUND,
        'TASK: SLICE DRAIN (WRITER) — drain every finding in the finder report at `' +
            f.report +
            '` (read it IN FULL from disk; a missing or invalid file returns a clean fixlog noting it in `summary` and nothing ' +
            'else). Your slice pages: ' +
            JSON.stringify(f.scope) +
            ' — full write authority over them and same-package ripple their repairs demand; an edit to any index doc, central ' +
            'manifest, or foreign-slice page is FORBIDDEN — route it as a `deferred` or `indexRows` row for the terminal ' +
            'reconciler, the sole writer of those surfaces. CONSUMPTION: group findings by `owner` then file, blockers first ' +
            '(`claimKey` is the dedupe key — one grammar, so the same key is ONE defect); each finding is a SIGNAL, not law — ' +
            're-open its anchors before editing (MANDATORY), honor `mechanism`/`owner`/`reject`/`acceptance` as the constraint ' +
            'boundary, and implement the STRONGEST resolution the boundary admits, never a single-point patch; a finding with a ' +
            'dead anchor, already resolved on disk, or graded hypothetical with no substantive re-derivation is rejected with ' +
            'its reason in `summary`. Return the fixlog — `deltas` exact, `deferred` and `indexRows` carrying every routed row. ' +
            HARVEST_LAW,
    ].join('\n\n');

// One doctrine charter, two carriers: the fixer's terminal round is the primary transport, the standalone lander only the
// dead-fixer/blocked-loop fallback — twin literals would drift, one owner never does.
const doctrineCharter = (nominations, orphans) =>
    'Read `docs/laws/README.md` FIRST — it owns the corpus admission and page-shape law; obey it over any restatement. Load ' +
    'the `docgen` skill AND the `skill-writer` skill via the Skill tool BEFORE any durable edit; load `mermaid-diagramming` ' +
    'before touching any diagram. ' +
    "NOMINATIONS (unverified, biased toward their authors' own work — refute by default): " +
    JSON.stringify(nominations) +
    '\nAlso sweep `' +
    SCRATCH +
    '/fixer-harvest.jsonl` (absent = none): rows there missing from NOMINATIONS are nominations too — a killed fixer round ' +
    'reaches the landing only through that file.\nAlso read the `harvest` array of every critique fixlog at these ' +
    'deterministic paths (an absent or invalid file skips; no other agent transports these rows): ' +
    JSON.stringify(orphans) +
    ' — dedupe them against NOMINATIONS and adjudicate them identically.\n' +
    'ADJUDICATE each row per the admission bar: cold-read its target surface IN FULL, verify its anchors on CURRENT disk; ' +
    'LAND NOTHING is a first-class verdict.\n' +
    'TOPOLOGY RE-PROOF: re-verify every `docs/laws/topology.md` row whose [SURFACE] this run touched — cull a row whose ' +
    'coupling no longer holds, land a coupling this run proved.\n' +
    'GATE: run `uv run .claude/skills/docgen/scripts/prose_gate.py <every touched .md>` and repair to zero FAILs.';

const fixerPrompt = (langs, roster, unmapped, rows, backlog, failed, pages, orphans, census, ideas, harvest, round) =>
    [
        ROOT_LAW,
        round
            ? 'DRAIN ROUND ' +
              round +
              ' — the backlog rows below were verified STILL-OPEN by the prior round; fix each at its root NOW, and a row you ' +
              'genuinely cannot land carries its named blocker and owner in `remaining`. Every other tranche re-arrives in full ' +
              'so a dead or partial prior round loses nothing — the checkpoint ledger is the consumption truth: skip every ' +
              'tranche it receipts, drain the rest.'
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
        'CHECKPOINT LEDGER: `' +
            SCRATCH +
            '/fixer-checkpoint.md` — read it FIRST and skip every tranche it already receipts (an interrupted drain re-enters, ' +
            'never restarts); append one line per tranche AS EACH COMPLETES (each report drained, the backlog block, each ' +
            'census and ideas dossier, the index-row apply, the own hunt). HARVEST FILE: append each `harvest` nomination to `' +
            SCRATCH +
            '/fixer-harvest.jsonl` (one JSON row per line) the moment it is minted — the terminal doctrine landing (5) sweeps the file, so a ' +
            'killed round loses no nomination; your returned `harvest` carries the same rows. Your returned `tranches` lists ' +
            'the checkpoint receipt line of every tranche fed this round — a fed tranche absent from that list is unconsumed, ' +
            'and the round has failed its mandate.',
        "TASK: TERMINAL FIX (WRITER — the run's LAST agent, nothing follows you): full write authority over the landed corpus, " +
            'libs-wide ripple authority with the expand-form bound LIFTED (collapse, rename, and contract are yours now that no ' +
            "sibling writer runs), and the run's SOLE writer for the owning-package index docs (ARCHITECTURE.md + README.md), " +
            'IDEAS.md, and the central manifests. Landed pages: ' +
            JSON.stringify(pages) +
            '.\nTRANCHE ORDER IS EXECUTION ORDER — the capability tranches (2c, 2d) precede the finder roster (3) by design; ' +
            'never demote them behind mechanical page fixes.\n' +
            [
                rows.length
                    ? '(1) INDEX ROWS: apply every reported row to its owning doc exactly once — dedupe semantically identical rows, keep ' +
                      "each doc's section grammar, verify every landed page is truthfully reflected; a central-manifest row hand-edits the " +
                      'grouped manifest at the SYMBOL anchor (never a line number), preserving label-group order; an IDEAS row lands as a ' +
                      'fully-specified card in the named IDEAS.md: ' +
                      JSON.stringify(rows) +
                      '.'
                    : '',
                backlog.length
                    ? '(2) DEFERRED BACKLOG (second-order and cross-batch ripples — re-verify each {files, claim} on current disk, fix ' +
                      'what holds, reject what disk already resolved): ' +
                      JSON.stringify(backlog) +
                      '.'
                    : '',
                orphans.length
                    ? '(2b) CRITIQUE FIXLOGS — every batch critique, folded-forward or orphaned (a live redteam folds judgment-lossy and a ' +
                      'dead one folds nothing); the paths are deterministic, so one absent on disk is skipped with a one-line note in ' +
                      '`summary`, never an error — read each present file IN FULL and drain the seam/deferred/index rows still open under ' +
                      'the same law (a row a redteam already landed disk-resolves and drops); the fixlog `harvest` arrays are DOCTRINE ' +
                      'NOMINATIONS for stage (5), never rows to fold into corpus fixes here: ' +
                      JSON.stringify(orphans) +
                      '.'
                    : '',
                census.length
                    ? '(2c) CORRECTIONS CENSUS dossiers (the writers landed only the rows intersecting their own pages — read each IN ' +
                      'FULL; every row NOT already resolved on current disk is yours: land it at its root or reject with reason): ' +
                      JSON.stringify(census) +
                      '.'
                    : '',
                ideas.length
                    ? '(2d) IDEAS WORKLIST dossiers (read each IN FULL; cross-check every idea against current disk — a dead batch stage ' +
                      'leaves ideas unclaimed or landed at one end of a cross-folder seam only): an unclaimed or half-landed idea is ' +
                      'yours — land it at its owner to full depth, finish the missing seam end, or reject with reason; an idea whose ' +
                      'remaining work genuinely exceeds a terminal pass returns as a fully-specified card in the owning IDEAS.md, never ' +
                      'a silent drop. Your returned `ideaLedger` carries one row PER DOSSIER IDEA — fate landed|carded|declined with ' +
                      'the landing page, the card, or the forbidding disk fact in `note`; a dossier idea absent from the ledger is the ' +
                      'silent drop the ledger exists to kill: ' +
                      JSON.stringify(ideas) +
                      '.'
                    : '',
                roster.length || unmapped.length
                    ? '(3) FINDER REPORTS — products ON DISK as JSON report files; the ROSTER receipts are navigation, never the product. ' +
                      'The roster carries the governance finders plus any slice whose drain lane died undrained — drained slices already ' +
                      'landed and reach you only as routed rows. CONSUMPTION, in order: (a) UNMAPPED scope is your direct-hunt queue — a ' +
                      "failed finder's territory gets your own cold read first; (b) read every roster report IN FULL from disk, " +
                      'governance before page slices — group findings by `owner` then file, blockers first (`claimKey` is the dedupe key: ' +
                      'one grammar, so the same key across lanes is ONE defect with corroborating evidence; shared owners and registries ' +
                      'before consumers, cross-folder seams before local prose); (c) each ' +
                      'finding is a SIGNAL, not law: re-open its anchors before editing — anchors behind an edit, cited members, seams, ' +
                      'and manifest rows re-verify MANDATORY; (d) `mechanism`/`owner`/`reject`/`acceptance` are the constraint boundary — ' +
                      'honor the owner and rejected forms, but the DESIGN is yours: implement the STRONGEST resolution the boundary ' +
                      'admits, never a single-point patch, landing the denser root-level reconstruction where the implied fix is weak; a ' +
                      'finding with a dead anchor or already resolved on disk is ' +
                      'rejected with reason. UNMAPPED: ' +
                      JSON.stringify(unmapped) +
                      ' ROSTER: ' +
                      JSON.stringify(roster) +
                      '.'
                    : '',
                '(4) OWN HUNT: hunt PAST the signal list — the hunt classes over the landed pages and governance surface — and fix ' +
                    'what the finders missed; `beyond` enumerates those fixes, and an empty `beyond` attests the hunt found nothing, ' +
                    'never that it did not run.',
                '(5) DOCTRINE LANDING — the durable-learning terminal, YOURS only on the round that fully drains the residual set: ' +
                    'when your `remaining` returns empty, execute it as your FINAL act and return `doctrine.performed=true` with its ' +
                    'landed/refined/rejected rows; a round ending with a non-empty `remaining` returns `doctrine.performed=false` ' +
                    '(empty arrays) and leaves the landing to the finishing round. Your own `harvest` rows minted this round are ' +
                    'nominations too, adjudicated under the same refute-by-default bar. ' +
                    doctrineCharter(harvest, orphans),
                'Every ripple an edit exposes is YOURS in the same pass — seam counterparts both ends, consumer sites, index docs, ' +
                    "manifest rows, .api anchors; wire-canonical names stay frozen; a foreign-language repair holds that branch's " +
                    'doctrine bar.' +
                    (failed.length
                        ? ' FAILED PAGES (reported, not landed — never author them here; correct any claim that pretends they landed): ' +
                          JSON.stringify(failed) +
                          '.'
                        : '') +
                    ' Return the final fixlog — `remaining` carries ONLY rows verified still-open and genuinely blocked, each naming ' +
                    'its blocker and owner; a row disk already resolved is culled with proof in `rejected`, and an empty `remaining` ' +
                    'attests the drain closed. ' +
                    HARVEST_LAW,
            ]
                .filter(Boolean)
                .join('\n'),
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
const plan = await slot(() => run(planPrompt(), wopts('plan', 'Plan', 'opus', PLAN_SCHEMA)));
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
// beside a terra two-tier .api inventory lane (codex) PER `.planning/<sub>` — package-level mapping dilutes depth on a large
// package and starves the batches of per-page grounding. Products are per-unit dossiers + reports on disk; receipts on the wire.
const PKGS = [...new Set(PAGES.map((p) => pkgOf(p.page)))];
// An oversize sub-folder splits into ceiling-bounded SEGMENTS here, once — map lanes and batches both consume the segmented
// units, so every batch's dossiers cover exactly its pages and the mapper fan scales with the writer fan on any folder size.
const UNITS = PKGS.flatMap((pkg) => {
    const pkgPages = PAGES.filter((p) => pkgOf(p.page) === pkg);
    return [...new Set(pkgPages.map((p) => subOf(p.page)))].flatMap((sub) => {
        const pages = pkgPages.filter((p) => subOf(p.page) === sub);
        const segs = pages.length > BATCH_MAX ? evenChunk(pages, BATCH_MAX) : [pages];
        return segs.map((seg, i) => {
            const name = sub + (segs.length > 1 ? '.' + (i + 1) : '');
            return { pkg, sub, name, key: pkg + '|' + name, tag: pkg.split('/').pop() + '.' + name, pages: seg };
        });
    });
});
const unitMap = {};
// Feeder fan: one ctx+api pair per strata-legal upstream package (planner-derived, kernel always present), projected onto the
// same-language run targets. FEEDERS derive from the Plan product alone — no unit-map data feeds them — so the feeder fan and
// the unit fan run in ONE concurrent pool; a barrier between them holds feeder lanes hostage to the slowest unit lane.
const FEEDERS = [...new Set(((plan && plan.packages) || []).flatMap((p) => p.feeders || []))].filter((f) => langOf(f) && !PKGS.includes(f));
const feederMap = {};
await Promise.all([
    ...UNITS.map(async (u) => {
        const L = Lof(u.pkg);
        const unitPages = u.pages;
        const scope = unitPages.map((p) => p.page);
        const tag = u.tag;
        const ctxDossier = dossierPath('map:ctx:' + tag);
        const apiDossier = dossierPath('map:api:' + tag);
        const [ctx, api] = await Promise.all([
            slot(() =>
                recon(
                    (reg) => ctxLensPrompt(L, unitPages, ctxDossier, reg),
                    ropts('map:ctx:' + tag, 'Map', MAP_SCHEMA, scope, { arr: 'index' }, { native: true, nativeModel: 'opus', stallMs: STALL }),
                ),
            ),
            slot(() =>
                recon(
                    (reg) => apiLensPrompt(L, unitPages, apiDossier, reg),
                    ropts('map:api:' + tag, 'Map', MAP_SCHEMA, scope, { arr: 'index' }, { writes: true }),
                ),
            ),
        ]);
        unitMap[u.key] = { ctx, api, ctxDossier, apiDossier };
    }),
    ...FEEDERS.map(async (feeder) => {
        const L = Lof(feeder);
        const targets = PKGS.filter((p) => Lof(p).key === L.key);
        if (!targets.length) return;
        const name = feeder.split('/').pop();
        const ctxDossier = dossierPath('map:feeder-ctx:' + name);
        const apiDossier = dossierPath('map:feeder-api:' + name);
        const [ctx, api] = await Promise.all([
            slot(() =>
                recon(
                    (reg) => feederLensPrompt(L, feeder, targets, ctxDossier, reg, 'ctx'),
                    ropts(
                        'map:feeder-ctx:' + name,
                        'Map',
                        MAP_SCHEMA,
                        targets,
                        { arr: 'index' },
                        { native: true, nativeModel: 'opus', stallMs: STALL },
                    ),
                ),
            ),
            slot(() =>
                recon(
                    (reg) => feederLensPrompt(L, feeder, targets, apiDossier, reg, 'api'),
                    ropts('map:feeder-api:' + name, 'Map', MAP_SCHEMA, targets, { arr: 'index' }, { writes: true }),
                ),
            ),
        ]);
        feederMap[feeder] = { ctx, api, ctxDossier, apiDossier };
    }),
]);
const mapOk = Object.values(unitMap).filter((m) => (m.ctx && m.ctx.ok) || (m.api && m.api.ok)).length;
const FEEDER_DOSSIERS = Object.values(feederMap)
    .flatMap((m) => [m.ctx && m.ctx.ok && m.ctxDossier, m.api && m.api.ok && m.apiDossier])
    .filter(Boolean);
log(
    'Map: ' +
        UNITS.length +
        ' unit segment(s) across ' +
        PKGS.length +
        ' package(s) mapped; ' +
        mapOk +
        ' with a live dossier; ' +
        FEEDERS.length +
        ' feeder package(s) projected',
);

phase('Ideate');
// TWO lanes per owning package with disjoint charters: a corrections census (opus — the fix addendum the batches land as rung 3)
// and a bigger-ideas worklist (opus — the capability ambition, rung 4). One merged log regresses to a fixlog and the ambition dies.
// Either lane absent (dead map or dead ideate), the executors run without that rung.
const pkgIdeate = {};
await Promise.all(
    PKGS.map(async (pkg) => {
        const L = Lof(pkg);
        const tag = pkg.split('/').pop();
        // Each row carries its unit's page roster — engine-held data the lanes otherwise re-derive with tree/fd (run-proven waste).
        const mapIndex = UNITS.filter((u) => u.pkg === pkg)
            .map((u) => ({
                sub: u.name,
                pages: u.pages.map((p) => p.page),
                deepMap: (unitMap[u.key].ctx?.ok && unitMap[u.key].ctxDossier) || null,
                inventory: (unitMap[u.key].api?.ok && unitMap[u.key].apiDossier) || null,
            }))
            .concat(
                FEEDERS.filter((f) => feederMap[f] && Lof(f).key === L.key).map((f) => ({
                    sub: 'feeder:' + f.split('/').pop(),
                    deepMap: (feederMap[f].ctx?.ok && feederMap[f].ctxDossier) || null,
                    inventory: (feederMap[f].api?.ok && feederMap[f].apiDossier) || null,
                })),
            )
            .filter((r) => r.deepMap || r.inventory);
        if (!mapIndex.length) {
            pkgIdeate[pkg] = { fix: '', idea: '' };
            return;
        }
        const fixDossier = dossierPath('ideate:fix:' + tag);
        const ideaDossier = dossierPath('ideate:idea:' + tag);
        const [fix, idea] = await Promise.all([
            slot(() => run(correctionsPrompt(L, pkg, mapIndex, fixDossier), wopts('ideate:fix:' + tag, 'Ideate', 'opus', RECEIPT))),
            slot(() => run(ideasPrompt(L, pkg, mapIndex, ideaDossier), wopts('ideate:idea:' + tag, 'Ideate', 'opus', RECEIPT))),
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
const IDEA_PATHS = PKGS.map((pkg) => pkgIdeate[pkg] && pkgIdeate[pkg].idea).filter(Boolean);

phase('Build');
// Batch composition packs WHOLE sub-folder units (in plan order) up to BATCH_MAX, splitting only an oversize unit — a batch's
// ownership seam then aligns with its map dossiers instead of slicing across sub-folders. Nothing serializes on the lanes;
// the agent-level slot scheduler is the only governor.
// Packs WHOLE unit segments (each already ceiling-bounded) into batches; a batch carries its source units so its dossier
// roster is exact, never re-derived from page paths.
const packBatches = (units, max) => {
    const out = [];
    let cur = [];
    let n = 0;
    for (const u of units) {
        if (cur.length && n + u.pages.length > max) {
            out.push(cur);
            cur = [];
            n = 0;
        }
        cur.push(u);
        n += u.pages.length;
    }
    if (cur.length) out.push(cur);
    return out;
};
const BATCHES = PKGS.flatMap((pkg) =>
    packBatches(
        UNITS.filter((u) => u.pkg === pkg),
        BATCH_MAX,
    ).map((units, i) => ({ pkg, i, units, pages: units.flatMap((u) => u.pages) })),
);
const SCOPES = JSON.stringify(BATCHES.map((b) => ({ batch: b.pkg.split('/').pop() + ':b' + b.i, pages: b.pages.map((p) => p.page) })));
const built = (
    await Promise.all(
        BATCHES.map(async (b) => {
            const tag = b.pkg.split('/').pop() + ':b' + b.i;
            const L = Lof(b.pkg);
            const batch = b.pages.map((p) => Object.assign({}, p, { i: b.i }));
            const pageScope = batch.map((p) => p.page);
            // ctx + api grounding come from the Map phase (per-unit-segment, reused); only the page-scoped bar lens runs per batch.
            const pms = b.units.map((u) => unitMap[u.key]).filter(Boolean);
            // Bar is doctrine JUDGMENT feeding the implementer's mandatory close-every-finding rung — sol over the terra
            // inventory default, pinned medium: the operator default effort is high, and an unpinned lane inherits it.
            const bar = await slot(() =>
                recon(
                    (reg) => barLensPrompt(L, batch, reg),
                    ropts(
                        'recon:bar:' + tag,
                        'Build',
                        BAR_SCHEMA,
                        pageScope,
                        { arr: 'findings', group: 'severity' },
                        { model: 'gpt-5.6-sol', codexEffort: 'medium' },
                    ),
                ),
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
                .concat(FEEDER_DOSSIERS)
                .join('`, `');
            const ideate = pkgIdeate[b.pkg] || { fix: '', idea: '' };
            const fix =
                (await slot(() =>
                    run(implementPrompt(L, batch, dossiers, ideate, SCOPES, roster, unmapped), wopts('impl:' + tag, 'Build', 'opus', FIXLOG_SCHEMA)),
                )) ||
                (await retryLane(() =>
                    slot(() =>
                        run(
                            implementPrompt(L, batch, dossiers, ideate, SCOPES, roster, unmapped),
                            wopts('impl:' + tag + ':r1', 'Build', 'opus', FIXLOG_SCHEMA),
                        ),
                    ),
                ));
            // CHAIN CONTINUATION: a dead implement never blocks the reviews — the critique's conformance audit and the redteam's
            // pre-mortem still improve the pages as they stand on disk; navigation simply arrives empty.
            const nav = navOf(fix ? [fix] : []);
            // Sol critique: a workspace-write codex lane running the full conformance audit in place; fixlog to disk, receipt on the
            // wire. The report path is DETERMINISTIC and the lane CHECKPOINTS it per closed page, so a dead receipt never severs the
            // fold — a ceiling-killed lane still leaves every closed page's row, and downstream consumers verify the path on disk
            // instead of trusting ok.
            const critReport = SCRATCH + '/' + fileTag('crit:' + tag) + '-report.json';
            const crit = await slot(() =>
                recon(
                    (reg) => critiquePrompt(L, batch, dossiers, ideate, SCOPES, roster, unmapped, nav, reg),
                    ropts(
                        'crit:' + tag,
                        'Build',
                        REVIEW_SCHEMA,
                        pageScope,
                        { arr: 'files' },
                        {
                            writes: true,
                            fix: true,
                            model: 'gpt-5.6-sol',
                            nativeModel: 'opus',
                            clockMs: CRIT_CLOCK,
                        },
                    ),
                ),
            );
            const critOk = !!(crit && crit.ok);
            const rt =
                (await slot(() =>
                    run(
                        redteamPrompt(L, batch, dossiers, ideate, SCOPES, roster, unmapped, nav, critOk, critReport),
                        wopts('rt:' + tag, 'Build', 'opus', REVIEW_SCHEMA),
                    ),
                )) ||
                (await retryLane(() =>
                    slot(() =>
                        run(
                            redteamPrompt(L, batch, dossiers, ideate, SCOPES, roster, unmapped, nav, critOk, critReport),
                            wopts('rt:' + tag + ':r1', 'Build', 'opus', REVIEW_SCHEMA),
                        ),
                    ),
                ));
            return { pkg: b.pkg, pages: b.pages, fix, critReport, rt };
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
// EVERY critique fixlog reaches the fixer (operational rows) AND the doctrine lander (harvest arrays) — keyed on the
// DETERMINISTIC path, never the receipt: nomination transport never rides a living agent's fold; a live folder has returned
// zero of a fixlog's nomination rows under an explicit mechanical mandate, so the adjudicator reads the disk artifact itself.
const ORPHANS = built.filter((d) => d.critReport).map((d) => d.critReport);
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
    const langBacklog = BACKLOG.filter((r) => (r.files || []).some((f) => langOf(f) === k));
    // evenChunk over chunk: fixed-size chunking leaves a runt tail slice whose finder over-mines its few pages while the full
    // slices dilute — balanced slices keep per-finding depth uniform across the fan.
    return evenChunk(langPages, FINDER_PAGES)
        .map((pages, i) => ({ gov: false, lang: k, pages, seams: langSeams, i }))
        .concat([{ gov: true, lang: k, pkgs: [...new Set(langPages.map(pkgOf))], pages: langPages, seams: langSeams, backlog: langBacklog }]);
});
const found = (
    await Promise.all(
        finderTasks.map((t) =>
            slot(() =>
                t.gov
                    ? recon(
                          (reg) => govFinderPrompt(LANG[t.lang], t.pkgs, t.pages, ROWS, t.seams, t.backlog, reg),
                          ropts(
                              'finder:gov:' + t.lang,
                              'Close',
                              FINDINGS_SCHEMA,
                              t.pkgs,
                              { arr: 'findings', group: 'class' },
                              { model: 'gpt-5.6-sol', codexEffort: 'medium' },
                          ),
                      )
                    : recon(
                          (reg) => finderPrompt(LANG[t.lang], t.pages, t.i, t.seams, reg),
                          ropts(
                              'finder:' + t.lang + ':s' + t.i,
                              'Close',
                              FINDINGS_SCHEMA,
                              t.pages,
                              { arr: 'findings', group: 'class' },
                              { model: 'gpt-5.6-sol', codexEffort: 'medium' },
                          ),
                      ),
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
// SLICE DRAINS: one concurrent opus writer per ok page-slice finder report — page-disjoint by construction; their deferred and
// index rows fold into the terminal reconciler, and a dead drain hands its report back to the reconciler roster undrained.
const sliceFinders = found.filter((f) => f.ok && f.lane.indexOf(':gov:') < 0);
const drainPairs = await Promise.all(
    sliceFinders.map(async (f) => {
        const L = LANG[f.lane.split(':')[1]] || LANG[LANDED_LANGS[0]];
        const d = await slot(() => run(drainPrompt(L, f), wopts('drain:' + f.lane.replace('finder:', ''), 'Close', 'opus', REVIEW_SCHEMA)));
        return { f, d };
    }),
);
const drained = drainPairs.filter((x) => x.d);
const fixerRoster = found.filter((f) => !f.ok || f.lane.indexOf(':gov:') >= 0).concat(drainPairs.filter((x) => !x.d).map((x) => x.f));
const DRAIN_ROWS = drained.flatMap((x) => x.d.indexRows || []);
const DRAIN_BACKLOG = drained.flatMap((x) => x.d.deferred || []);
log('Close: ' + drained.length + '/' + sliceFinders.length + ' slice drain(s) landed; ' + DRAIN_BACKLOG.length + ' routed row(s) to the reconciler');

// Terminal DRAIN LOOP: the run's ONE fable seat — full-repo authority absorbs every residual, so the premium judgment spend
// concentrates where scope is widest; every batch and slice lane rides opus/sol. One serial reconciler per round takes the
// residual set — governance findings, undrained slice
// reports, routed rows, the fixlog sweep — verifies every row against live disk (freshness is its duty — sole writer of index
// docs and manifests, no collisions), fixes at root, and loops until empty; a round without shrinkage stops with the blocked set final.
// Every round re-receives the FULL tranche set: the checkpoint ledger receipts consumption, so a dead or partial round loses
// nothing and a live one skips what it already landed — only the backlog narrows round over round.
let fixer = null;
let fixerHarvest = drained.flatMap((x) => x.d.harvest || []);
let residuals = BACKLOG.concat(DRAIN_BACKLOG);
const ALL_ROWS = ROWS.concat(DRAIN_ROWS);
// Build-stage nominations are engine-known before the loop; the terminal round adjudicates them plus the jsonl sweep plus its
// own rows, so the finishing fixer holds the complete pool without a second agent.
const BUILT_HARVEST = built.flatMap((d) => ((d.fix && d.fix.harvest) || []).concat((d.rt && d.rt.harvest) || []));
let lastOpen = Infinity;
for (let round = 0; round < DRAIN_ROUNDS; round++) {
    const fire = (suffix) =>
        slot(() =>
            run(
                fixerPrompt(
                    LANDED_LANGS,
                    fixerRoster,
                    UNMAPPED,
                    ALL_ROWS,
                    residuals,
                    FAILED,
                    LANDED,
                    ORPHANS,
                    CENSUS_PATHS,
                    IDEA_PATHS,
                    BUILT_HARVEST,
                    round,
                ),
                wopts((round ? 'fixer:r' + round : 'fixer') + suffix, 'Close', 'fable', FIXER_SCHEMA),
            ),
        );
    fixer = (await fire('')) || (await retryLane(() => fire(':a1')));
    if (!fixer) break; // dead round after retries: the residual set survives to the run return, and every disk tranche stays checkpoint-re-enterable
    fixerHarvest = fixerHarvest.concat(fixer.harvest || []);
    const open = fixer.remaining || [];
    residuals = open;
    if (!open.length || open.length >= lastOpen) break;
    lastOpen = open.length;
}
// DOCTRINE FALLBACK: the terminal fixer round is the doctrine landing's primary carrier; this standalone lander fires ONLY
// when no round performed it — a dead fixer (its nominations live in the incremental harvest file) or a loop that ended
// blocked without draining. Refutation-first, land-nothing legal, admission law owned by docs/laws.
const HARVEST_ROWS = BUILT_HARVEST.concat(fixerHarvest);
const doctrinePerformed = !!(fixer && fixer.doctrine && fixer.doctrine.performed);
const doctrine = doctrinePerformed
    ? fixer.doctrine
    : HARVEST_ROWS.length || ORPHANS.length || !fixer
      ? await slot(() =>
            run(
                ROOT_LAW +
                    '\n\nTASK: DOCTRINE LANDER — the durable-learning terminal of this run (the fixer loop ended without ' +
                    'performing the landing, so it falls to you). ' +
                    doctrineCharter(HARVEST_ROWS, ORPHANS) +
                    '\nReturn landed/refined/rejected (each rejection with its reason)/files/summary.',
                wopts('doctrine', 'Close', 'opus', DOCTRINE_SCHEMA),
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
