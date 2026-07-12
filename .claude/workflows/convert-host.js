export const meta = {
    name: 'convert-host',
    description:
        'Convert Rasm.Rhino and Rasm.Grasshopper from durable host-bound source into full planning folders, model-tiered so mapping never spends a writer. ALL reconnaissance runs read-only on gpt-5.6-terra through codex wrapper agents (CODEX flag; false restores native lanes): hostile source census slices, current-only host-API MINING lanes (assay decompile over RhinoCommon/Eto/Grasshopper2 - never Rhino 7 or GH1 material; verified member inventories PLUS new catalog-grouping gaps, information never designs), and a 3-slice kernel mining fan over the rebuilt libs/csharp/Rasm corpus (every universal owner the host layers must compose instead of re-deriving, every expand-form extension site) - each lane writing its COMPLETE typed JSON product to the workflow scratch and returning a thin receipt {ok, report, entries, headline, failure}. A Catalog phase owns the .api tier on opus under docgen: one planner per folder consumes the mining reports and rules the catalog roster (existing stubs deepened, new capability groupings admitted as new files, never god or mini files, the mature sibling .api tiers as form exemplars), then opus authors fan out per disjoint write group CONCURRENT with the fable architect - which rules the re-derived sub-domain map + unit dependency edges from census evidence (splits and merges follow evidence, never the inherited layout or a pre-ruled example) and executes the scaffold. Build runs ALL units across both folders CONCURRENT under one agent-level slot cap (a unit waits only on the implement stage of its declared dependency units): fable GROUND-UP IMPLEMENTS, one gpt-5.6-sol ultra codex lane CRITIQUES in place (workspace-write; doctrinal conformance + capability completeness + two-tier ultra-stacking, fixlog to disk), fable RED-TEAMS terminal and folds the critique fixlog rows into the unit record - navigation-fact handoffs, seam-ledger coordination, bidirectional kernel ripple authority under the evidence/expand-form/depth bounds; opus authors the per-folder index docs from the landed tree. Close is a read-only terra acceptance fan (one lane per folder: FULL census-disposition, symbol, boundary, kernel, and law audit) feeding ONE terminal fable law fixer owning central manifests, index rows, the deferred backlog, orphaned critique fixlogs, and every verified acceptance finding - nothing follows the fixer.',
    whenToUse: 'Launch once to open the host-boundary planning campaign. Ephemeral - delete after the campaign lands.',
    phases: [
        {
            title: 'Survey',
            detail: 'census slices + host-API mining + 3-slice kernel mining, all read-only on gpt-5.6-terra via codex wrappers (sonnet dispatch shells); products to disk, receipts on the wire; CODEX=false restores native lanes',
            model: 'sonnet',
        },
        {
            title: 'Catalog',
            detail: 'per folder: one opus planner rules the .api roster from the mining reports (stubs deepened, new groupings admitted, no god or mini files), then opus authors fan out per disjoint group under docgen - concurrent with the architect',
            model: 'opus',
        },
        { title: 'Architect' },
        {
            title: 'Build',
            detail: 'per unit: fable ground-up implement, gpt-5.6-sol ultra codex critique in place (fixlog to disk), fable terminal red-team folding the critique rows; opus index docs per folder from the landed tree',
        },
        {
            title: 'Close',
            detail: 'read-only acceptance fan per folder on gpt-5.6-terra (codex wrappers; full disposition + symbol/boundary/kernel/law audit, products to disk) then ONE terminal fable law fixer: manifests, index rows, backlog drain, orphaned critique fixlogs, acceptance findings re-verified as signals, law-doc sweep',
        },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAP = 14; // runtime concurrency clamp is min(16, cores-2) = 14 on this machine
const STAGGER_MS = 1500;
const STALL = 300000;
const SOL_STALL = 2400000; // sol ultra critique holds one blocking MCP call up to the 1800s tier ceiling; stall detection must outlast it
const MAX_UNITS = 6;
const UNIT_PAGES = 8; // pages per unit ceiling; editing fidelity degrades past ~8 dense pages per writer
const CAT_AUTHORS = 5; // catalog-author fan ceiling per folder; the planner packs the ruled roster into at most this many disjoint write groups
const CODEX = true; // survey/kernel/critique/acceptance lanes ride codex wrappers (terra; sol for critique); false restores native lanes
const CODEX_DIR = '.claude/scratch/convert-host'; // per-run MCP reports and unit seam ledgers

// Kernel mining slices: the Rasm planning corpus is too large for one honest full-read lane.
const KERNEL_SLICES = [
    { key: 'core', dirs: 'Domain/, Numerics/, Spatial/' },
    { key: 'motion', dirs: 'Parametric/, Processing/, Solving/' },
    { key: 'visual', dirs: 'Analysis/, Drawing/, Meshing/' },
];

const FOLDERS = [
    {
        key: 'rhino',
        root: 'libs/csharp/Rasm.Rhino',
        name: 'Rasm.Rhino',
        host: 'RhinoCommon (Rhino 9 WIP) + Eto.Forms/Eto.Drawing',
        census: [
            { key: 'a', paths: 'Blocks/ and Camera/' },
            { key: 'b', paths: 'Commands/, Capture.cs, and Events.cs' },
            { key: 'c', paths: 'Exchange/' },
            { key: 'd', paths: 'UI/' },
        ],
        mining: [
            {
                key: 'rcdoc',
                catalogs: 'api-rhinocommon-document.md, api-rhinocommon-commands.md, api-rhinocommon-blocks.md, api-rhinocommon-fileio.md',
                charge:
                    'RhinoCommon document + command substrate on Rhino 9 WIP: RhinoDoc lifecycle and events, the object tables ' +
                    '(objects/layers/materials/groups/views/named views/instance definitions), RhinoApp/Command infrastructure, the ' +
                    'full Get* interactive families, transaction/undo records, units and tolerance regimes, block/instance-definition ' +
                    'surfaces, file IO incl. the format-engine roster and FilePdf, and every Rhino-9-new capability those surfaces carry.',
            },
            {
                key: 'rcvis',
                catalogs: 'api-rhinocommon-display.md, api-rhinocommon-geometry.md, api-rhino-ui.md',
                charge:
                    'RhinoCommon display + UI on Rhino 9 WIP: the geometry-to-display pipeline, DisplayConduit and custom draw, ' +
                    'ViewCapture, render pipeline hooks, display modes and materials, viewport/camera control, and Rhino.UI ' +
                    'panels/dialogs/pages/gumball/toolbars - plus every Rhino-9-new display/UI capability the current assemblies carry.',
            },
            {
                key: 'eto',
                catalogs: 'api-eto-forms.md, api-eto-runtime.md, api-eto-drawing.md, api-macos-native.md',
                charge:
                    'Eto.Forms + Eto.Drawing as shipped with Rhino 9: the FULL control roster, layout containers (dynamic/table/' +
                    'pixel/stack), data binding (IndirectBinding/BindableBinding, MVVM patterns), dialogs and semi-modal, custom drawing ' +
                    '(Drawable, Graphics, paths/brushes/pens/text), styles and platform handlers, Rhino UI integration surfaces ' +
                    '(Panels.RegisterPanel, RhinoEtoApp, EtoExtensions), toolbars, clipboard/drag-drop/notifications, plus the ' +
                    'platform-gated AppKit/CoreAnimation vsync-and-cosmetics seam - the capability set a generator-shaped UI layer ' +
                    'should own so any native Rhino UI element is a row, never hand assembly.',
            },
        ],
    },
    {
        key: 'gh',
        root: 'libs/csharp/Rasm.Grasshopper',
        name: 'Rasm.Grasshopper',
        host: 'Grasshopper2 SDK (GH2 on Rhino 9 WIP) + Eto',
        census: [
            { key: 'a', paths: 'Components/' },
            { key: 'b', paths: 'UI/Canvas.cs, UI/Ui.cs, UI/Document.cs, and UI/Editor.cs' },
            { key: 'c', paths: 'UI/Events.cs, UI/Input.cs, UI/Interaction.cs, and UI/Layout.cs' },
            { key: 'd', paths: 'UI/Motion.cs, UI/Paint.cs, and UI/Wire.cs' },
        ],
        mining: [
            {
                key: 'gh2',
                catalogs: 'api-gh2-components.md, api-gh2-document.md',
                charge:
                    'The Grasshopper2 SDK as shipped with Rhino 9 WIP: the component model (Component base, construction-time ' +
                    'inputs/outputs, Access levels, the pin/parameter families), the document/solver model (Document, DocumentMethods, ' +
                    'ObjectList, Connectivity/Connections, SolutionServer lifecycle, expiry), data model (Garden trees/paths/pears), ' +
                    'special objects, undo actions, plugin registration, and every GH2-vs-GH1 paradigm break - GH1 idioms ' +
                    '(GH_Component, IGH_*, RegisterInputParams, SolveInstance signatures) are LEGACY POISON: naming one as current is a ' +
                    'defect. GH2 documentation is sparse; the decompile IS the truth.',
            },
            {
                key: 'gh2ui',
                catalogs: 'api-gh2-canvas.md, api-gh2-editor.md, api-gh2-flex.md, api-gh2-interaction.md',
                charge:
                    'GH2 canvas + chrome UI: canvas paint phases and picking, skinning, wire shapes, snapping/alignment, the ' +
                    'flex/animation substrate (Motion.Pacer/Spring, RepaintRequest, Subscription, MotionEquations), editor/toolbar/' +
                    'input-panel/tooltip chrome, and floating buttons - the capability set a higher-order layer should own so custom ' +
                    'component UI and canvas overlays are rows. GH2 documentation is sparse; the decompile IS the truth.',
            },
            {
                key: 'etogh',
                catalogs: 'api-eto-forms.md, api-eto-runtime.md, api-eto-drawing.md, api-macos-native.md',
                charge:
                    'Eto.Forms + Eto.Drawing as hosted inside the GH2 process: the FULL control roster, layout containers ' +
                    '(dynamic/table/pixel/stack), data binding (IndirectBinding/BindableBinding, MVVM patterns), dialogs and ' +
                    'semi-modal, custom drawing (Drawable, Graphics, paths/brushes/pens/text), styles and platform handlers, ' +
                    'Eto-hosted panels/dialogs inside GH2, clipboard/drag-drop/notifications, plus the platform-gated AppKit/' +
                    'CoreAnimation gesture/vsync/cosmetics seam - the capability set so any GH2-native panel, dialog, or overlay ' +
                    'chrome is a row, never hand assembly.',
            },
        ],
    },
];

// --- [INPUTS] --------------------------------------------------------------------------

const argsIn = typeof args === 'string' && /^\s*[\[{]/.test(args) ? JSON.parse(args) : args;
const wanted = Array.isArray(argsIn)
    ? argsIn
    : argsIn && typeof argsIn === 'object' && Array.isArray(argsIn.targets)
      ? argsIn.targets
      : typeof argsIn === 'string' && argsIn.trim()
        ? [argsIn.trim()]
        : null;
const ACTIVE = wanted ? FOLDERS.filter((f) => wanted.some((w) => String(w).indexOf(f.name) >= 0 || String(w).indexOf(f.key) >= 0)) : FOLDERS;

// --- [MODELS] --------------------------------------------------------------------------

// One anchor = one fact at one coordinate; interpretation never lives in an anchor row.
const anchorOf = (roles) => ({
    type: 'object',
    additionalProperties: false,
    required: ['path', 'line', 'role', 'note'],
    properties: {
        path: { type: 'string' },
        line: { type: 'integer' },
        role: { type: 'string', enum: roles },
        note: { type: 'string' },
    },
});
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
// Recon/inventory product (research + kernel recon lanes): facts with anchors, never prescriptions.
const MAP = {
    type: 'object',
    additionalProperties: false,
    required: ['entries', 'coverage', 'summary'],
    properties: {
        entries: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['target', 'kind', 'files', 'info', 'anchors', 'members'],
                properties: {
                    target: { type: 'string' }, // catalog, kernel owner, capability cluster, or seam the entry grounds
                    kind: { type: 'string', enum: ['owner', 'catalog', 'capability', 'gap', 'seam'] },
                    files: { type: 'array', items: { type: 'string' } }, // files the reader must open for this entry
                    info: { type: 'string' }, // the fact: current shape, extension site, seam endpoints — prose truth, zero prescriptions
                    anchors: { type: 'array', items: anchorOf(['state', 'ruling', 'catalog', 'counterpart', 'absence']) },
                    members: { type: 'array', items: { type: 'string' } },
                },
            },
        }, // verified member spellings backing the entry
        coverage: COVERAGE,
        summary: { type: 'string' },
    },
};
// Defect-shaped product (census register + acceptance audit): inventory fields + constraint-boundary fields per entry.
const REGISTER = {
    type: 'object',
    additionalProperties: false,
    required: ['entries', 'coverage', 'summary'],
    properties: {
        entries: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: [
                    'claimKey',
                    'target',
                    'kind',
                    'files',
                    'info',
                    'members',
                    'severity',
                    'claim',
                    'mechanism',
                    'owner',
                    'reject',
                    'acceptance',
                    'anchors',
                ],
                properties: {
                    claimKey: { type: 'string' }, // <kind>|<owner>|<primary symbol or absence route> — stable across lanes, never lane wording
                    target: { type: 'string' }, // the .cs file (census), csproj (manifest), or miss label (acceptance)
                    kind: { type: 'string', enum: ['register', 'manifest', 'symbol', 'disposition', 'boundary', 'kernel', 'law'] },
                    files: { type: 'array', items: { type: 'string' } }, // files the reader must open or edit first
                    info: { type: 'string' }, // inventory prose: owned capability + host-agnostic vs host-bound split — facts only
                    members: { type: 'array', items: { type: 'string' } }, // exact host members composed, verified spellings
                    severity: { type: 'string', enum: ['blocker', 'major', 'minor'] }, // bound to consequence, never prose confidence
                    claim: { type: 'string' }, // the observed fact or defect (census: quality verdict, naivety axes named)
                    mechanism: { type: 'string' }, // WHY the current form fails — factual, zero repair verbs
                    owner: { type: 'string' }, // canonical absorber (census: the collapse-signal polymorphic owner)
                    reject: { type: 'array', items: { type: 'string' } }, // deleted forms (census: what dies as pattern)
                    acceptance: { type: 'array', items: { type: 'string' } }, // proof signals (census: what survives as concept)
                    anchors: { type: 'array', items: anchorOf(['defect', 'ruling', 'catalog', 'counterpart', 'absence']) },
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
const UNIT = {
    type: 'object',
    additionalProperties: false,
    required: ['key', 'pages', 'owns', 'charter', 'after'],
    properties: {
        key: { type: 'string' },
        pages: { type: 'array', items: { type: 'string' } },
        owns: { type: 'string' }, // the ownership boundary: which vocabulary/owners THIS unit mints vs composes
        charter: { type: 'string' },
        after: { type: 'array', items: { type: 'string' } },
    },
}; // unit keys whose IMPLEMENT must land first (vocabulary producers)
const ARCH_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['preSwap', 'units', 'packageDeltas', 'summary'],
    properties: {
        preSwap: { type: 'string' },
        units: { type: 'array', items: UNIT },
        packageDeltas: { type: 'array', items: { type: 'string' } },
        summary: { type: 'string' },
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
};
const INDEXROWS = {
    type: 'array',
    items: {
        type: 'object',
        additionalProperties: false,
        required: ['doc', 'row'],
        properties: { doc: { type: 'string' }, row: { type: 'string' } },
    },
};
const FIXLOG = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'verdict', 'summary', 'deltas', 'deferred', 'indexRows', 'phantoms'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        verdict: { type: 'string', enum: ['authored', 'rebuilt', 'refined', 'clean'] },
        phantoms: { type: 'array', items: { type: 'string' } },
        summary: { type: 'string' },
        deltas: DELTAS,
        deferred: DEFERRED,
        indexRows: INDEXROWS,
    },
};
const DOCS_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'summary'],
    properties: {
        files: { type: 'array', items: { type: 'string' } },
        summary: { type: 'string' },
    },
};
// Catalog-roster ruling: disjoint write groups over the folder .api tier — kept stubs plus admitted new files.
const CAT_PLAN = {
    type: 'object',
    additionalProperties: false,
    required: ['groups', 'dropped', 'summary'],
    properties: {
        groups: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['key', 'catalogs'],
                properties: {
                    key: { type: 'string' },
                    catalogs: {
                        type: 'array',
                        items: {
                            type: 'object',
                            additionalProperties: false,
                            required: ['file', 'status', 'charter'],
                            properties: {
                                file: { type: 'string' }, // catalog filename under <folder>/.api/
                                status: { type: 'string', enum: ['stub', 'new'] },
                                charter: { type: 'string' }, // the owned capability territory, one line
                            },
                        },
                    },
                },
            },
        },
        dropped: { type: 'array', items: { type: 'string' } }, // proposed groupings rejected, each with its ruling
        summary: { type: 'string' },
    },
};

// --- [DOCTRINE] ------------------------------------------------------------------------

const CONTEXT =
    'Rasm monorepo - libs/csharp planning corpus (markdown specs of intended C# package designs). ' +
    'CLAUDE.md manifest + WORKSPACE_LAW strata govern (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP; ' +
    'depend strictly upward). docs/stacks/csharp is the FLOOR, never the ceiling - every fence meets it and pushes past ' +
    'it to the strongest form the doctrine admits.';

const MANDATE =
    'CAMPAIGN LAW - THE HOST-BOUNDARY CONVERSION. Rasm.Rhino and Rasm.Grasshopper are planning folders (the ' +
    'index-doc shells, .planning/, and folder .api stub catalogs exist on disk); this campaign realizes them: each folder ' +
    'entirely captures its host API surface and builds higher-order abstractions, value, and capability so countless future ' +
    'Rhino/GH2 apps and plugins compose rows instead of re-deriving host plumbing. BINDING RULES: ' +
    '(1) Each folder references ONLY the Rasm kernel - leverage Rasm surfaces where they fit, NEVER any other sibling ' +
    'package, never a coupling to Element/Materials/Bim/Compute/Persistence, and NEVER Rasm.AppUi: Eto is THE native UI ' +
    'framework and each folder owns a full Eto sub-domain able to build any native UI element with native host integration. ' +
    '(2) KERNEL UNIFICATION: host-agnostic logic - easing/spring/interpolation math, perceptual color blending, pure ' +
    'geometry/numeric algorithms - COMPOSES the Rasm kernel, never a second in-folder derivation; where the kernel lacks a ' +
    'universal owner the concept demands, the kernel owner is EXTENDED in place in expand-form (a case, row, field, or ' +
    'operation on the existing Rasm planning page - the kernel recon map names the exact sites) with the host folder ' +
    'composing it; renaming or collapsing a kernel surface is recorded in `deferred` for the law tail, never raced. ' +
    '(3) CURRENT HOST ONLY: Rhino 9 WIP RhinoCommon, Grasshopper2 SDK, current Eto - a Rhino 6/7-era pattern or a GH1 idiom ' +
    '(GH_Component, IGH_*, SolveInstance, RegisterInputParams) presented as current is a phantom-class defect; every host ' +
    'member is verified via `uv run python -m tools.assay api` decompile over the host assemblies (assay blocked: the .api ' +
    'catalogs, Context7, and official McNeel material own the fallback - and the claim is marked catalog-verified, never ' +
    'guessed). (4) Planning-folder form per libs/.planning/README.md: design pages at <pkg>/.planning/<SubDomain>/<page>.md ' +
    '- PascalCase sub-folders, lowercase page names - one page per eventual source file, one dense polymorphic owner per ' +
    'page at 400-700+ LOC fence mass, transcription-complete fences, zero fence comments beyond canonical section dividers. ' +
    '(5) The existing source is CONSIDERABLE BUT NOT GOOD: capture every capability from the census, rebuild it ground-up ' +
    'denser/richer/parameterized, kill every naive pattern (both axes: thin COVERAGE slices, enumerated APPROACH rosters ' +
    'that one generator should generate), and extend to the full domain the host admits. Anticipate the FIVE-TIMES demand - ' +
    "model each owner for five times today's cases, fields, and consumers; every extension cites one gap source: HOST (a " +
    'verified member the concept admits that the census-era source ignored), DOMAIN (an attribute, state, relationship, or ' +
    'operation the real concept demands), or CONSUMER (a contract future Rhino/GH2 apps and plugins require). ' +
    '(6) Buildout over removal: a ' +
    'capability is dropped only as an explicit ruled kill; a phantom member is the sole silent deletion. (7) You fix what ' +
    'you notice anywhere in the repo EXCEPT the serialized surfaces: the owning-folder index docs (the docs agent writes ' +
    'them once), and the central manifests + cross-repo law docs (the law tail writes them once) - report exact rows in ' +
    '`indexRows` instead of editing those.';

const READ_FIRST =
    'READ FIRST, IN ORDER, BEFORE ANY EDIT. (1) DOCTRINE - enumerate docs/stacks/csharp/ with a real ' +
    'ls (never memory), read the README and EVERY root page it routes IN FULL in atlas order; then enumerate ' +
    'docs/stacks/csharp/domain/ through its router README and read every shard your pages touch (the Rhino/geometry/' +
    'host shards are mandatory here). This prompt does not restate doctrine; read it at source and conform every fence ' +
    'to it. (2) .API - ls BOTH catalog tiers in full: the shared substrate libs/csharp/.api/ AND the folder .api/, plus ' +
    'the kernel folder catalogs libs/csharp/Rasm/.api/ (the RhinoCommon-adjacent truths live there); layer the ' +
    'Thinktecture/LanguageExt rails onto the host surfaces, never the host set alone. (3) AUTHORING LAW - ' +
    'libs/.planning/README.md in full (doc-set, page grammar, card law, banned hedges) and docs/standards/' +
    'style-guide.md. (4) KERNEL - the Rasm package README.md + ARCHITECTURE.md so kernel leverage composes real ' +
    'surfaces, never guesses; the kernel recon report carries the exact unified-owner sites.';

const STANCE =
    'STANCE - every pass is hostile; the pages under review were authored by ANOTHER engineer and are under ' +
    'adversarial review. Hold every fence naive, shallow, or illusory until it survives a real attack; the burden of proof ' +
    'is on the code. Dense, confident, package-fluent code is the PRIME suspect. NAIVETY is a defect on two axes: COVERAGE ' +
    '(a 2-case family for a 20-case domain) and APPROACH (an enumerated roster where one parameterized generator should ' +
    'generate the space - the roster demotes to seed DATA). ILLUSORY code is the primary target: doctrine vocabulary ' +
    '([Union]/[SmartEnum<TKey>]/[ValueObject]/rails), cited hosts, confident prose, hollow body. Every collapse-signal ' +
    'list is a FLOOR. NO CHURN: an edit requires a named violated law or invariant and the concrete case that breaks it; a ' +
    'clean verdict earned by an attack that finds nothing is a first-class result, proven by adding nothing.';

const WRITE_FULLY =
    'WRITE FULLY - every fix you identify you make NOW; the fix-log reports edits already made, ' +
    'never a to-do or a hedge. A first-order cross-file ripple your edit causes is YOURS in the same pass; a second-order ' +
    'ripple (exposed by a ripple repair) or a surface a concurrent unit owns is recorded in `deferred` as {files, claim} - ' +
    'the law tail drains the backlog; nothing is silently dropped. Latest modern C# 14 on net10; apply the ' +
    'docs/stacks/csharp file-organization + section-order law; total generated Switch, no silent _ arm; prose per ' +
    'docs/standards/style-guide.md - declarative present-tense fact, every symbol backticked, zero meta framing, zero ' +
    'provenance, never fragile count-based prose.';

const CURRENT_STATE =
    'CURRENT STATE - sibling units land work concurrently with yours. Before any edit, re-read the ' +
    'CURRENT on-disk state of your pages AND every sibling page your pages compose; landed sibling work is picked up as ' +
    'found. A vocabulary owner your charter marks as composed (not minted) is read from disk as it NOW stands; a conflict ' +
    'between your design and a landed sibling resolves to the stronger form, never a revert. SEAM LEDGER: append one row ' +
    'per cross-unit event to your unit ledger `' +
    CODEX_DIR +
    '/<folder>-<unit>-seams.md` (`SEAM_CHANGED | <files> | ' +
    '<symbol fact, old -> new>` when a shared name/signature you mint moves; `RIPPLE_REPAIRED | <files> | <fact>` when you ' +
    'repair a counterpart). Before any edit outside your unit pages, ls `' +
    CODEX_DIR +
    '/` and read every sibling ' +
    '`*-seams.md` row whose files intersect yours - a RIPPLE_REPAIRED row is work you do NOT redo.';

const GIT_GROUND =
    'DELTA GROUNDING - run `git status` and `git diff --stat -- <your unit pages>` to see what this run ' +
    'changed before judging it; the diff is orientation, CURRENT disk is truth.';

const INFO_LAW =
    'You provide INFORMATION, never prescriptions: exact disk locations and anchors, the current shape at ' +
    'each surface, verified member spellings, gaps. The architect and unit writers decide how to build; an entry that ' +
    'tells them what to write instead of what is true is a defect. ENTRY FORM: `info` is prose truth; `anchors` carry one ' +
    'coordinate per row (role names what it proves; `note` is the shortest literal witness under 20 words, or empty when ' +
    'path+line suffice; an `absence` anchor names where the expected thing was searched and not found); `files` lists ' +
    'what the reader must open for the entry; `members` carry verified spellings only. An underutilized-capability entry ' +
    'is INVENTORY, never instruction: verified members, current usage anchors, the concept that admits it - the writer ' +
    'decides whether it composes. COVERAGE is part of the product: `requested` = your assigned scope, `read` = what you ' +
    'actually full-read, `skipped`/`unverified` = what you did not reach - an honest skip beats a silent one.';

const EVIDENCE_LAW =
    'ENTRY FORM - you deliver TRUTH, never an implementation: `claim` states the observed fact or ' +
    'defect; `mechanism` states WHY the current form fails as fact; `anchors` carry one coordinate per row (role names ' +
    'what the coordinate proves; `note` is the shortest literal witness - a symbol, member spelling, or fragment under ' +
    '20 words - or empty when path+line suffice; an `absence` anchor names where the expected thing was searched and not ' +
    'found); `owner` names the canonical owner that must absorb the resolution (the owning axis, row roster, or ' +
    'polymorphic owner - never a new local shape); `reject` lists the forms the rebuild must not take; `acceptance` ' +
    'lists the signals that prove resolution. NEVER write add/replace/implement/promote/delete as instruction - the ' +
    'writer owns the design; you own the constraint boundary. `claimKey` = <kind>|<owner>|<primary symbol or absence ' +
    'route>, identical for the same fact regardless of lane or wording. `severity` binds to consequence: blocker = ' +
    'campaign-blocking, major = rebuild-ruling correctness, minor = local cleanup - never prose confidence. OUTPUT ' +
    'BOUNDS: the task states your register bound - a census register is COMPLETE (every assigned file appears as one ' +
    'entry); an audit scope returns only confirmed misses, 0 only when the hostile pass comes back empty, and then ' +
    '`summary` names the probes that produced nothing; never manufacture an entry, never delete a confirmed one. ' +
    'COVERAGE is part of the product: `requested` = your assigned scope, `read` = what you actually full-read, ' +
    '`skipped`/`unverified` = what you did not reach or could not confirm - an honest skip beats a silent one.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sleep = (ms) => new Promise((res) => setTimeout(res, ms));
// Agent-level slot scheduler: CAP agents in flight across ALL folders and units, staggered launch,
// work-conserving backfill. The single governor for every agent call.
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
// Codex dispatch: the sonnet wrapper makes one blocking Codex MCP call, writes its result verbatim, and returns mechanical orchestration data.
const fileTag = (label) => label.replace(/[^A-Za-z0-9_.-]+/g, '-');
const codexPrompt = (label, task, schema, o) => {
    const base = CODEX_DIR + '/' + fileTag(label);
    const root = '/Users/bardiasamiee/Documents/99.Github/Rasm';
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
            ', config={"model_reasoning_effort":"' +
            (o.codexEffort || 'high') +
            '"}, and prompt set to the COMPLETE task text below followed by its final-message contract. ' +
            'If the call errors, retry the identical call ONCE; if the retry errors, skip step (3) and return the error through step (4). ' +
            'TASK:\n\n' +
            task +
            '\n\nFinal message: ONLY a JSON object matching this shape (no fences, no prose): ' +
            JSON.stringify(schema),
        '(3) Write the tool result text VERBATIM with the Write tool to this absolute path: ' +
            report +
            '. Do not normalize, reformat, summarize, or extract the text before writing it.',
        '(4) Parse the tool result text only to compute entries.length and the kind tallies. Return ok=true, report=' +
            base +
            '-report.json, entries=that count, headline="<entries> entries | <kind tallies> | top: <most frequent first file or none>", and ' +
            'failure empty. On a second tool error return ok=false, entries=0, report and headline empty, and failure equal to the error text.',
    ].join('\n\n');
};
// Every codex-dispatched lane routes here: terra by default, sol where o.model says so; CODEX=false
// restores a native lane at o.nativeModel (opus default). The roster row carries `scope` from the
// ORCHESTRATOR (never the lane's self-report) so a failed lane's unmapped territory is exact even
// when the lane died before writing anything.
const recon = (task, o) =>
    (CODEX
        ? agent(codexPrompt(o.label, task, o.schema, o), {
              label: (o.model && o.model.indexOf('-sol') >= 0 ? 'sol:' : 'terra:') + o.label,
              phase: o.phase,
              model: 'sonnet',
              effort: 'low',
              schema: RECEIPT,
              stallMs: o.stallMs || STALL,
          })
        : agent(
              task +
                  '\n\nPRODUCT TO DISK: write your COMPLETE product as one JSON file matching this schema at ' +
                  CODEX_DIR +
                  '/' +
                  fileTag(o.label) +
                  '-report.json (Write tool, absolute path under the repo root): ' +
                  JSON.stringify(o.schema) +
                  ' — then return ONLY the receipt: ok, report path, entries count, one-line mechanical headline, failure empty.',
              { label: o.label, phase: o.phase, model: o.nativeModel || 'opus', effort: 'high', schema: RECEIPT, stallMs: o.stallMs || STALL },
          )
    )
        .then((r) => ({
            lane: o.label,
            scope: o.scope || [],
            ok: !!(r && r.ok && r.report),
            report: (r && r.report) || '',
            entries: (r && r.entries) || 0,
            headline: (r && r.headline) || '',
            failure: (r && r.failure) || (r ? '' : 'lane died'),
        }))
        .catch(() => ({ lane: o.label, scope: o.scope || [], ok: false, report: '', entries: 0, headline: '', failure: 'lane died' }));
// Navigation handoff: FACTS ONLY - files, symbol deltas, backlog. Never verdicts, summaries, or adjectives.
const navOf = (logs) => {
    const rows = logs.filter(Boolean);
    return {
        files: [...new Set(rows.flatMap((r) => r.files || []))],
        deltas: rows.flatMap((r) => r.deltas || []),
        deferred: rows.flatMap((r) => r.deferred || []),
    };
};

const censusPrompt = (folder, lane) =>
    [
        CONTEXT,
        MANDATE,
        EVIDENCE_LAW,
        'TASK: HOSTILE READ-ONLY SOURCE CENSUS over ' +
            folder.root +
            ' - your assigned slice: ' +
            lane.paths +
            '. ' +
            'Read every assigned .cs file IN FULL. For EACH file emit one `register` entry: `target` = the file; `info` = the ' +
            'owned capability in concept terms plus the HOST-AGNOSTIC vs HOST-BOUND split (pure math/algebra liftable to the ' +
            'Rasm kernel vs genuinely host-coupled mechanism); `members` = the exact host members it composes (' +
            folder.host +
            '); `claim` = the quality verdict with the naivety axes named; `mechanism` = why the current form fails; `owner` = ' +
            'the collapse signal (the one polymorphic owner the sibling capabilities belong inside); `reject` = what dies as ' +
            'pattern; `acceptance` = what survives as concept (the rebuild intent). Also inventory ' +
            folder.root +
            '/' +
            folder.name +
            '.csproj as one `manifest` entry: references, packages, host assembly bindings in `info`/`members`. ' +
            'The register must be COMPLETE - every assigned file appears; downstream disposition audits key off your entries.',
    ].join('\n\n');

const miningPrompt = (folder, lane) =>
    [
        CONTEXT,
        MANDATE,
        INFO_LAW,
        'TASK: CURRENT-ONLY HOST-API MINING for ' +
            folder.name +
            ' - lane ' +
            lane.key +
            ' (read-only; investigate, do NOT edit - opus catalog authors consume your report). ' +
            lane.charge +
            ' ' +
            'PRIMARY ROUTE: `uv run python -m tools.assay api` decompile over the installed host assemblies - enumerate ' +
            'namespaces, then drill the surfaces your charge names; quote verified member signatures with their assembly ' +
            'anchors. SECONDARY: existing catalogs under libs/csharp/Rasm/.api/ and libs/csharp/.api/, Context7, and current ' +
            'McNeel developer material - marked catalog-verified when assay cannot reach a surface. FORBIDDEN: any Rhino 6/7-era ' +
            'or GH1 pattern presented as current; training-data recall without verification. YOUR STUB TERRITORY at ' +
            folder.root +
            '/.api/: ' +
            lane.catalogs +
            ' - each stub declares its owned namespace scope; read each and mine its territory to operator DEPTH. ' +
            'REPORT PRODUCT: (1) one `catalog` entry per assigned stub - the verified member inventory for its declared scope: ' +
            'exact signatures in `members` (deep - the full useful surface, not a sample), integration shape per capability ' +
            'cluster in `info`, what the census-era source missed. (2) `gap` entries: host capability territory NO stub in the ' +
            'folder covers - each a candidate NEW catalog grouping sized for one focused file (never a god file, never a mini ' +
            'file): name the territory, exemplar verified members, and the concept that admits it, as fact. (3) `capability`/' +
            '`seam` entries for cross-catalog findings. Verified anchors ONLY; a fake anchor or phantom member is your defect.',
    ].join('\n\n');

const kernelPrompt = (slice) =>
    [
        CONTEXT,
        MANDATE,
        INFO_LAW,
        'TASK: READ-ONLY KERNEL MINING over libs/csharp/Rasm - the rebuilt kernel planning corpus the host folders compose. ' +
            'Your slice: the ' +
            slice.dirs +
            ' sub-domains. Read the package README.md + ARCHITECTURE.md, then EVERY design page under ' +
            'libs/csharp/Rasm/.planning/ inside your slice IN FULL, and ls libs/csharp/Rasm/.api/ (read any catalog your slice ' +
            'pages cite). PRODUCT - information, never prescriptions: (1) `owner` entries: every kernel owner in your slice the ' +
            'host-boundary layers must COMPOSE instead of re-deriving - easing/spring/interpolation/timeline math, perceptual ' +
            'color algebra, pure geometry/numeric algorithms, parametric/motion vocabulary - each with the exact page, owner ' +
            'name, and member spellings quoted with file:line anchors; mine to operator depth: the full capability an owner ' +
            'carries, not the fraction the census-era source used. (2) `gap` entries: host-agnostic capability the census-era ' +
            'host source hand-rolls (46-curve easing families, damped-spring integrators, OKLab/OKLCH blending, repeat/yoyo ' +
            'cycle arithmetic) that NO kernel owner in your slice carries yet - name the closest existing kernel owner and the ' +
            'expand-form extension site (the page + section where a case/row/field/operation would land) as fact. (3) `seam` ' +
            'entries: every kernel surface in your slice whose shape the host folders depend on, quoted. MANDATORY SELF-VERIFY: ' +
            're-open every cited anchor before returning; a guess or vague entry is deleted.',
    ].join('\n\n');

const catalogPlanPrompt = (folder, miningR) =>
    [
        CONTEXT,
        MANDATE,
        'TASK: RULE the .api catalog roster for ' +
            folder.root +
            '/.api/ - judgment only, do NOT author catalogs (your authors do; you MAY NOT write any catalog content). ' +
            'EVIDENCE: the mining reports are ON DISK as typed JSON - read every ok report IN FULL; a failed lane below means ' +
            'its stub territory is ruled from the stubs themselves plus your own targeted assay probes: ' +
            JSON.stringify(miningR.map((m) => ({ lane: m.lane, ok: m.ok, report: m.report, entries: m.entries }))) +
            '. Then ls ' +
            folder.root +
            '/.api/ (the stub roster) and ls libs/csharp/Rasm.Bim/.api/ + libs/csharp/.api/ (the mature form exemplars: many ' +
            'focused per-capability files, none god, none mini). RULE: (a) every existing stub keeps its file and scope unless ' +
            'two stubs genuinely own one concept (merge ruling) or one stub owns two (split ruling); (b) every mining `gap` ' +
            'grouping is admitted as a NEW catalog file, merged into a kept file, or rejected with a ruling in `dropped` - a ' +
            'silently ignored gap is your defect; (c) each catalog carries a one-line charter naming its owned capability ' +
            'territory, disjoint from every sibling. PARTITION the ruled roster into at most ' +
            CAT_AUTHORS +
            " write groups of related catalogs, balanced by expected depth (a group is one author's whole write scope; no " +
            'catalog appears twice). Return {groups, dropped, summary}.',
    ].join('\n\n');

const catalogAuthorPrompt = (folder, group, miningR) =>
    [
        CONTEXT,
        MANDATE,
        'TASK: AUTHOR the .api catalogs of group ' +
            group.key +
            ' for ' +
            folder.root +
            '/.api/ - EXACTLY these files, yours alone this run (no other lane writes them): ' +
            JSON.stringify(group.catalogs) +
            '. LOAD the `docgen` skill via the Skill tool BEFORE writing - the catalogs are durable agent-facing markdown and ' +
            'pass its prose gate. FORM: read two mature exemplars first - one folder-tier catalog from libs/csharp/Rasm.Bim/' +
            '.api/ and one substrate catalog from libs/csharp/.api/ - and match their structure, register, and density. ' +
            'EVIDENCE: the mining reports on disk carry the verified member inventories - read the ones covering your files IN ' +
            'FULL: ' +
            JSON.stringify(miningR.filter((m) => m.ok).map((m) => m.report)) +
            '. Every member you write beyond a report is verified yourself via `uv run python -m tools.assay api` decompile ' +
            '(assay blocked: Context7/official McNeel material, marked catalog-verified); a member you cannot verify is ' +
            'OMITTED, never guessed - a GH1 or Rhino 6/7-era idiom presented as current is a phantom-class defect. CONTENT per ' +
            "catalog: the file's charter, verified member signatures organized by capability cluster, the integration shape " +
            'per cluster, and a [STACKING] section layering the Thinktecture/LanguageExt substrate rails onto the host ' +
            'surfaces - inventory and integration FACT only, never design prescriptions, never process narration, zero ' +
            'provenance. A `new`-status file is authored whole from its charter; a `stub` file is DEEPENED in place preserving ' +
            'its scope and name. Return {files, summary}.',
    ].join('\n\n');

const architectPrompt = (folder, survey, kernelReports, unmapped, catRoster) =>
    [
        CONTEXT,
        MANDATE,
        READ_FIRST,
        STANCE,
        'TASK: RULE + SCAFFOLD the ' +
            folder.name +
            ' planning folder. EVIDENCE - the survey REPORT FILES are your ' +
            "reconnaissance. CONSUMPTION: (a) UNMAPPED scope below gets your own cold read FIRST - a failed lane's territory " +
            'is your direct census; (b) read every ok report IN FULL from disk - the kernel mining reports (' +
            (kernelReports.length ? kernelReports.join(', ') : 'ALL UNMAPPED: read libs/csharp/Rasm README + ARCHITECTURE + .planning directly') +
            ') and the ' +
            'host-API mining reports before the census slices; entries overlap across lanes, dedupe by claimKey/target as you read; ' +
            "(c) each entry's anchors are jump coordinates - spot-verify what you build on, and re-open every anchor behind an " +
            'edit; (d) `owner`/`reject`/`acceptance` on a census entry are its constraint boundary - honor them; the DESIGN is ' +
            'yours. UNMAPPED: ' +
            JSON.stringify(unmapped) +
            ' ROSTER: ' +
            JSON.stringify(survey) +
            '. The ruled .api catalog roster (opus authors land these concurrently; cite catalogs in unit charters by file): ' +
            catRoster +
            '. ' +
            'RULE the architecture: RE-DERIVE the ' +
            'sub-domain map from the census evidence alone - a sub-domain earns 3+ pages or folds; rule every split and every ' +
            'merge from the evidence, never from the inherited file layout and never from a pre-ruled example: concern-mixing ' +
            'in a source file is genuine split pressure, a cohesive concern is genuine keep-together pressure, and the landed ' +
            'map carries no god-surface and no one-page shell sub-domain; ' +
            'an Eto sub-domain is MANDATORY and owns full native UI construction as generator rows. Emit the complete page ' +
            'roster (PascalCase sub-folders, lowercase page names, one dense owner per page), the disposition of every census ' +
            'register entry (absorbed into a named page or explicitly killed with a ruling), the host-agnostic rows routed to the ' +
            'kernel per MANDATE rule (2) (each with its kernel extension site from the mining), and any packageDeltas (central ' +
            'Directory.Packages.props motions - REPORT them, never edit the central manifest; folder .csproj edits are yours). ' +
            'PARTITION the roster into at most ' +
            MAX_UNITS +
            ' build units of at most ' +
            UNIT_PAGES +
            ' pages each: units ' +
            'run CONCURRENTLY, so each unit carries `owns` (the vocabulary/owners it MINTS - no two units mint the same owner), ' +
            'a charter (collapse rulings, census-entry dispositions, host-capability targets from the research catalogs, kernel ' +
            'compose/extend rows), and `after` (the unit keys whose IMPLEMENT must land before this unit starts - ONLY true ' +
            'vocabulary dependence, typically the foundation unit alone; an empty after is the default, and after may only name ' +
            'units earlier in your emitted order). THEN EXECUTE the scaffold: record the current HEAD hash as preSwap; create ' +
            folder.root +
            '/.planning/<SubDomain>/ dirs; git rm every existing .cs source file and source sub-folder (the ' +
            'census captured them; git history recovers them; the sibling architect runs concurrently - on a git index.lock ' +
            'failure, wait briefly and retry) - keep the .csproj, packages.lock.json, the index-doc shells ' +
            '(IDEAS.md, TASKLOG.md), and the .api/ catalogs; apply ruled folder .csproj edits. Do NOT commit. Return {preSwap, ' +
            'units, packageDeltas, summary}.',
    ].join('\n\n');

const implementPrompt = (folder, unit, preSwap, reports, kernelReports, scopes) =>
    [
        CONTEXT,
        MANDATE,
        READ_FIRST,
        STANCE,
        WRITE_FULLY,
        CURRENT_STATE,
        'TASK: GROUND-UP AUTHOR unit ' +
            unit.key +
            ' of ' +
            folder.name +
            ' - build freely and ambitiously to the full ' +
            'bar; the trailing critique and red-team passes carry the attack. EXACTLY these pages: ' +
            unit.pages.join(', ') +
            '. OWNS: ' +
            unit.owns +
            '. CHARTER: ' +
            unit.charter +
            ' Evidence reports (typed JSON products on disk - read the ' +
            "ones your charter touches IN FULL; each entry's anchors are jump coordinates, re-open any anchor behind an edit): " +
            reports.join(', ') +
            '; kernel mining reports: ' +
            (kernelReports.length ? kernelReports.join(', ') : 'ALL UNMAPPED - read libs/csharp/Rasm README + ARCHITECTURE + .planning directly') +
            '; the folder .api catalogs are freshly authored and verified - compose them at operator depth. ' +
            'CONCURRENT UNIT SCOPES (a page another unit owns is composed from disk, never edited; a needed change there is a ' +
            '`deferred` row): ' +
            scopes +
            '. Old source recovers via git show ' +
            preSwap +
            ':<path> when a census entry needs ' +
            'depth beyond its register. Before authoring EACH page, restate in one line the owner it holds, the vocabulary it ' +
            'mints vs composes, and the doctrine laws that bind it - then build against that restatement. Construct in ' +
            'LIFECYCLE order: admit raw once, canonical owner by OWNER_CHOOSER, stacked rail/aspect over a thin pure core, ' +
            'projection, egress, BOTH ingress and egress parameterized; collapse parallel shapes into ONE [Union]/' +
            '[SmartEnum<TKey>]/[ValueObject<T>]/[ComplexValueObject]/source-generated case family IN THE SAME FILE; one ' +
            'polymorphic entrypoint per modality. Host-agnostic math COMPOSES or EXTENDS the kernel per MANDATE rule (2). ' +
            'Every host member verified per the mandate route before it is written. Return the fix-log - `deltas` carries every ' +
            'minted symbol/wire as data, `deferred` the backlog rows, both exact.',
    ].join('\n\n');

const critiquePrompt = (folder, unit, scopes, nav) =>
    [
        CONTEXT,
        MANDATE,
        READ_FIRST,
        STANCE,
        WRITE_FULLY,
        CURRENT_STATE,
        'NAVIGATION (facts from the pass that landed these pages - locations only, no assessments; it changes where you look ' +
            'FIRST, never what you conclude): ' +
            JSON.stringify(nav),
        GIT_GROUND,
        'TASK: CRITIQUE - your role law is libs/.planning/campaign-method.md [04] CRITIQUE, read at source and held to the ' +
            'letter: the mechanical line-by-line doctrinal-conformance and capability-completeness audit, every hit a fix made ' +
            'now, never a note; the named checklists are a FLOOR you hunt past. Your mandate is PREDICATE-POSITIVE: verify each ' +
            'required law holds and cite the clause. FORM YOUR OWN DEFECT LIST FIRST - read each page cold from CURRENT disk ' +
            'before consulting NAVIGATION. Fix EACH page of unit ' +
            unit.key +
            ' in place: ' +
            unit.pages.join(', ') +
            '. ' +
            'CONCURRENT UNIT SCOPES (foreign pages composed, never edited; changes there go to `deferred`): ' +
            scopes +
            '. ' +
            '- COLLAPSE_SCAN: run the docs/stacks/csharp README [03] table on every fence. ' +
            '- OWNER_CHOOSER (shapes.md [01]): re-derive every shape from its discriminants; kill every parallel DTO, ' +
            'one-field wrapper, and null/default ghost. ' +
            '- KNOB_TEST: delete each parameter - where the value reconstructs it, collapse to a policy value or input-shape ' +
            'discriminant. ' +
            '- ASPECTS + RAILS: audit against surfaces-and-dispatch.md and rails-and-effects.md at their owning pages. ' +
            '- HOST TRUTH: every RhinoCommon/Eto/GH2 member re-verified against the folder .api catalogs; a legacy idiom (GH1, ' +
            'Rhino 7-era) is a phantom - delete or rebuild on the current surface. ' +
            '- ULTRA-STACKING (both tiers): ls the folder .api/ AND libs/csharp/.api/ in full; an admitted capability the ' +
            "page's concept admits that no fence exploits - a host surface the folder catalog carries, or a Thinktecture/" +
            'LanguageExt/substrate rail the shared tier carries unlayered onto the host set - is a defect closed by growing ' +
            'the owner, never noted. ' +
            '- KERNEL UNIFICATION: host-agnostic math re-derived in the fence instead of composing/extending the kernel is a ' +
            'defect fixed per MANDATE rule (2). ' +
            '- BOUNDARY: the folder references ONLY the Rasm kernel; a coupling to any other sibling or to AppUi is a defect ' +
            'fixed now. ' +
            '- CAPABILITY-COMPLETENESS + ILLUSION: the body implements what names and prose promise; close every admitted host ' +
            'capability the owner omits per the charter and the .api catalogs; attack both naivety axes. ' +
            'Return the fix-log - `deltas` and `deferred` exact.',
    ].join('\n\n');

const redteamPrompt = (folder, unit, scopes, nav, crit) =>
    [
        CONTEXT,
        MANDATE,
        READ_FIRST,
        STANCE,
        WRITE_FULLY,
        CURRENT_STATE,
        'NAVIGATION (locations only, no assessments): ' + JSON.stringify(nav),
        crit && crit.ok
            ? 'PRIOR CLAIMS (UNVERIFIED): the sol critique fixlog is ON DISK at ' +
              crit.report +
              ' - read it IN FULL from disk; its edits and verdicts are refutation targets you judge against CURRENT disk, ' +
              'never a settled record. FOLD-FORWARD DUTY: its surviving `deferred` and `indexRows` rows are folded into YOUR ' +
              "return (re-verified against current disk, deduped) - your fixlog is the unit's consolidated record; a dropped " +
              'critique row is a silent loss.'
            : 'PRIOR CLAIMS: the critique lane did not land - your cold attack is the only review this unit gets; judge from ' +
              'CURRENT disk alone.',
        GIT_GROUND,
        'TASK: RED-TEAM - your role law is libs/.planning/campaign-method.md [04] RED-TEAM, read at source and held to the ' +
            'letter: the terminal and most aggressive review; every defect repaired in place, and the work ends objectively ' +
            'DENSER and MORE CAPABLE than critique left it. Your mandate is PREDICATE-NEGATIVE - a pre-mortem, not a second ' +
            'conformance audit. FORM YOUR OWN ATTACK FIRST - cold-read each page from CURRENT disk before consulting the claims. ' +
            'Fix EACH page of unit ' +
            unit.key +
            ' in place: ' +
            unit.pages.join(', ') +
            '. CONCURRENT UNIT SCOPES (foreign ' +
            'pages composed, never edited; changes there go to `deferred`): ' +
            scopes +
            '. ' +
            '(A) COUNTERFACTUAL on the core owner/algebra/dispatch - does a denser generated family, a derived table, a ' +
            'parameterized generator over the enumerated space, or a deeper LanguageExt/Thinktecture/host/kernel primitive ' +
            'collapse the whole fence? A fundamentally stronger design is built, never defended against. (B) ' +
            'ANTICIPATORY_COLLAPSE - the diff of the next feature: the next control kind, command, conduit, component family, or ' +
            'canvas widget lands as one row with every consumer untouched or loudly broken. (C) LONG-TAIL - empty/singular/' +
            'plural/stream/malformed/concurrent/cancelled/partial-failure/host-teardown; undo records, document events, and ' +
            'solution expiry handled where the host demands them. (D) BOUNDARY/STRATA - kernel-only references; host types never ' +
            'leak past the folder contract; host-agnostic math composes the kernel per MANDATE rule (2). (E) SPRAWL + PHANTOMS - ' +
            'hand-re-derived host capability, flat code below the operator depth the packages reach, a phantom or legacy member ' +
            '(delete), a thin wrapper; and the inverse: an edit that ADDED surface where the doctrine demands collapse is ' +
            'regression rebuilt denser. (F) a FULL COLD RE-REVIEW of every conformance dimension by name. Return the fix-log.',
    ].join('\n\n');

const docsPrompt = (folder, unitReports) =>
    [
        CONTEXT,
        MANDATE,
        WRITE_FULLY,
        'TASK: the index docs for ' +
            folder.root +
            ' per libs/.planning/README.md [02] - you are their ONE writer. LOAD the `docgen` skill via the Skill tool ' +
            'BEFORE writing; the docs pass its prose gate. Read ' +
            'libs/csharp/Rasm/README.md and libs/csharp/Rasm/ARCHITECTURE.md IN FULL as the sibling form exemplars, plus ' +
            'docs/standards/style-guide.md and docs/standards/formatting.md - the new docs match that structure, styling, and ' +
            'grammar exactly. README.md: the page router over the landed .planning tree + the domain-package registry (host ' +
            'assemblies + any folder additions) + the substrate section pointing at the branch registry. ARCHITECTURE.md: the ' +
            'standardized intro, the codemap tree naming every sub-domain with one-line charters (the eventual source tree, ' +
            'never the .planning scaffold), [02]-[SEAMS] carrying only genuine cross-folder rows (kernel consumption is a ' +
            'codemap fact, not a seam; the folder has no cross-language seam). IDEAS.md and TASKLOG.md exist as shells - ' +
            "PRESERVE them untouched; deferred-concept cards are the terminal law fixer's authority, never yours. Verify every " +
            'router row against disk. Unit results: ' +
            JSON.stringify(unitReports) +
            '. Return {files, summary}.',
    ].join('\n\n');

const acceptPrompt = (r, rows) =>
    [
        CONTEXT,
        EVIDENCE_LAW,
        'TASK: READ-ONLY ACCEPTANCE over the converted ' +
            r.folder +
            ' (' +
            r.root +
            ') - investigate, never edit. ' +
            '(1) `symbol` - cross-page symbol sweep: every cross-page symbol a landed fence composes resolves on a sibling ' +
            'owner with a matching signature; (2) `disposition` - FULL census-disposition audit: read EVERY register entry ' +
            'from the census reports under ' +
            CODEX_DIR +
            '/ (census-' +
            r.key +
            '-*-report.json) and confirm each landed in a page or carries an explicit architect kill - a capability that ' +
            'silently vanished is a `disposition` miss; (3) `boundary` - no reference beyond the Rasm kernel, no AppUi, no ' +
            'GH1/legacy member cited as current; (4) `kernel` - no host-agnostic math re-derived in a fence where the kernel ' +
            'owns or gained the owner; (5) `law` - the folder README router rows and ARCHITECTURE codemap resolve against the ' +
            'landed .planning tree, and no out-of-scope/no-planning/durable-source claim survives in the cross-repo law docs. ' +
            'KNOWN-FAILED PAGES (never landed; a disposition miss routed to one is still reported, marked failed-unit ' +
            'territory in `info`): ' +
            JSON.stringify(r.failedPages || []) +
            '. PENDING ROWS - the terminal law fixer applies these after you; a miss those rows already close is DROPPED, ' +
            'never reported: ' +
            JSON.stringify(rows) +
            '. Each confirmed miss is one entry of the matching kind with file-evidence anchors; zero misses is a valid ' +
            'empty result, and then `summary` names the probes that produced nothing.',
    ].join('\n\n');

const lawPrompt = (results, backlog, accepts, accUnmapped, orphans) =>
    [
        CONTEXT,
        MANDATE,
        WRITE_FULLY,
        "TASK: TERMINAL LAW FIXER - you are the run's LAST agent, nothing follows you: the ONE writer for central manifests " +
            'and cross-repo law docs, the drain for the deferred backlog, and the resolver for every acceptance finding. ' +
            "(0) ORPHANED CRITIQUE FIXLOGS (units whose red-team never landed, so these on-disk fixlogs' `deferred` and " +
            '`indexRows` rows were never folded forward - read each IN FULL from disk and drain those rows under the same law ' +
            'as (2) and (3)): ' +
            JSON.stringify(orphans) +
            '. ' +
            '(1) CENTRAL MANIFEST: apply the collected packageDeltas to Directory.Packages.props at the correct label groups, ' +
            'newest stable versions verified via the nuget MCP; reflect any new package in the owning folder README registry if ' +
            'the docs agent missed it: ' +
            JSON.stringify(results.map((r) => ({ folder: r.folder, packageDeltas: r.packageDeltas }))) +
            '. ' +
            '(2) DEFERRED BACKLOG (second-order and cross-unit ripples the writers recorded - re-verify each {files, claim} on ' +
            'current disk, fix what holds, reject what disk already resolved; kernel-surface collapse/rename rows are yours to ' +
            'apply now that no concurrent writer runs; a verified claim genuinely outside campaign scope lands as a ' +
            'fully-specified card in the owning folder IDEAS.md or TASKLOG.md, never dropped): ' +
            JSON.stringify(backlog) +
            '. ' +
            '(3) INDEX ROWS reported by writers (apply each to its owning doc exactly once, deduped): ' +
            JSON.stringify(results.flatMap((r) => r.indexRows || [])) +
            '. ' +
            '(4) ACCEPTANCE REPORTS - the acceptance products are ON DISK as JSON report files; the receipts below are ' +
            'navigation, never the product. UNMAPPED territory (a dead lane) is your direct audit queue - run those audit ' +
            'dimensions over that folder yourself. Read every ok report IN FULL from disk; each finding is a SIGNAL, not law: ' +
            're-open its anchors before editing, honor its `owner`/`reject`/`acceptance` constraint boundary, and implement ' +
            'the STRONGEST resolution that boundary admits - never a single-point patch where a denser root-level repair is ' +
            'available; a finding with a dead anchor or already resolved on disk is rejected with reason. ROSTER: ' +
            JSON.stringify(accepts.map((a) => ({ lane: a.lane, ok: a.ok, report: a.report, entries: a.entries, headline: a.headline }))) +
            ' UNMAPPED: ' +
            JSON.stringify(accUnmapped) +
            '. FAILED PAGES (reported, never landed - never author them here; correct any index or law claim that pretends ' +
            'they landed): ' +
            JSON.stringify(results.flatMap((r) => r.failedPages || [])) +
            '. ' +
            '(5) LAW DOCS - both folders are planning folders and the cross-repo docs already state it; VERIFY and fix residuals: ' +
            'libs/.planning/planning-targets.md (both in the CSHARP Planning Folders row), libs/.planning/architecture.md, ' +
            'libs/csharp/.planning/README.md (upgrade the two HOST-BOUNDARY router rows to README.md links now the files exist), ' +
            'libs/csharp/.planning/ARCHITECTURE.md. (6) SWEEP: rg for Rasm.Rhino and Rasm.Grasshopper across libs/**/*.md, ' +
            'README.md, and docs/** (excluding docs/stacks) - fix every remaining stale out-of-scope/no-planning/durable-source ' +
            'claim. PROSE LAW: docs/standards/style-guide.md - declarative present-tense fact, zero meta framing, structure named ' +
            'by members and law, never by count. Return {files, summary}.',
    ].join('\n\n');

// --- [COMPOSITION] ---------------------------------------------------------------------

if (!ACTIVE.length) {
    log('No matching targets - pass Rasm.Rhino / Rasm.Grasshopper paths or run with empty args for both.');
    return { targets: [], total: 0 };
}
log('Converting: ' + ACTIVE.map((f) => f.name).join(', ') + '; CAP=' + CAP);

phase('Survey');
// Shared 3-slice kernel mining fan; awaited before each folder's catalog planner and architect.
const kernelRecon = Promise.all(
    KERNEL_SLICES.map((s) =>
        slot(() =>
            recon(kernelPrompt(s), {
                label: 'kernel:' + s.key,
                phase: 'Survey',
                schema: MAP,
                scope: ['libs/csharp/Rasm/.planning: ' + s.dirs],
            }),
        ),
    ),
);

const results = (
    await Promise.all(
        ACTIVE.map(async (folder) => {
            const lanes = folder.census
                .map((lane) => ({ kind: 'census', lane, schema: REGISTER, scope: [folder.root + ': ' + lane.paths] }))
                .concat(
                    folder.mining.map((lane) => ({
                        kind: 'mining',
                        lane,
                        schema: MAP,
                        scope: lane.catalogs.split(',').map((s) => folder.root + '/.api/' + s.trim()),
                    })),
                );
            const roster = await Promise.all(
                lanes.map((entry) =>
                    slot(() =>
                        recon(entry.kind === 'census' ? censusPrompt(folder, entry.lane) : miningPrompt(folder, entry.lane), {
                            label: entry.kind + ':' + folder.key + ':' + entry.lane.key,
                            phase: 'Survey',
                            schema: entry.schema,
                            scope: entry.scope,
                        }),
                    ),
                ),
            );
            const mapped = roster.filter((r) => r.ok);
            if (!mapped.length) throw new Error(folder.name + ': no survey report landed');
            const kmaps = await kernelRecon;
            const survey = roster.concat(kmaps);
            const unmapped = survey.filter((r) => !r.ok).flatMap((r) => r.scope.map((sc) => ({ lane: r.lane, scope: sc })));
            const kernelReports = kmaps.filter((k) => k.ok).map((k) => k.report);
            const miningR = roster.filter((r) => r.lane.indexOf('mining:') === 0);
            const reports = survey.filter((r) => r.ok).map((r) => r.report);
            log(
                folder.name +
                    ': ' +
                    survey.filter((r) => r.ok).reduce((a, r) => a + r.entries, 0) +
                    ' survey entries across ' +
                    survey.filter((r) => r.ok).length +
                    '/' +
                    survey.length +
                    ' lanes' +
                    (unmapped.length
                        ? ' — FAILED: ' +
                          survey
                              .filter((r) => !r.ok)
                              .map((r) => r.lane)
                              .join(', ')
                        : ''),
            );

            // Catalog roster ruling (opus); a dead planner falls back to the static stub grouping so authors still land.
            const catPlan = await slot(() =>
                agent(catalogPlanPrompt(folder, miningR), {
                    label: 'catplan:' + folder.key,
                    phase: 'Catalog',
                    model: 'opus',
                    effort: 'high',
                    schema: CAT_PLAN,
                    stallMs: STALL,
                }),
            ).catch(() => null);
            const groups =
                catPlan && catPlan.groups && catPlan.groups.length
                    ? catPlan.groups.slice(0, CAT_AUTHORS)
                    : folder.mining.map((l) => ({
                          key: l.key,
                          catalogs: l.catalogs.split(',').map((s) => ({ file: s.trim(), status: 'stub', charter: '' })),
                      }));
            log(
                folder.name +
                    ': catalog roster ' +
                    groups.reduce((n, g) => n + g.catalogs.length, 0) +
                    ' file(s) in ' +
                    groups.length +
                    ' group(s)' +
                    (catPlan ? '' : ' — planner FAILED, static stub fallback'),
            );

            // Catalog authors (opus, disjoint write groups) run CONCURRENT with the architect; units wait on both.
            const authorsP = Promise.all(
                groups.map((g) =>
                    slot(() =>
                        agent(catalogAuthorPrompt(folder, g, miningR), {
                            label: 'catalog:' + folder.key + ':' + g.key,
                            phase: 'Catalog',
                            model: 'opus',
                            effort: 'high',
                            schema: DOCS_SCHEMA,
                            stallMs: STALL,
                        }),
                    ).catch(() => null),
                ),
            );
            const archP = slot(() =>
                agent(architectPrompt(folder, survey, kernelReports, unmapped, JSON.stringify(groups)), {
                    label: 'architect:' + folder.key,
                    phase: 'Architect',
                    model: 'fable',
                    effort: 'high',
                    schema: ARCH_SCHEMA,
                    stallMs: STALL,
                }),
            );
            const [authors, arch] = await Promise.all([authorsP, archP]);
            const authored = authors.filter(Boolean);
            log(folder.name + ': ' + authored.length + '/' + groups.length + ' catalog group(s) authored');
            if (!arch || !arch.units || !arch.units.length) throw new Error(folder.name + ': architect did not land');
            const units = arch.units.slice(0, MAX_UNITS);
            const scopes = JSON.stringify(units.map((u) => ({ unit: u.key, owns: u.owns, pages: u.pages })));
            log(folder.name + ': ' + units.length + ' unit(s), ' + units.reduce((n, u) => n + u.pages.length, 0) + ' pages ruled');

            // Concurrent unit chains: a unit waits ONLY on the IMPLEMENT stage of its declared `after` units
            // (vocabulary producers, earlier-indexed only - cycles impossible), never on sibling chains.
            const implDone = new Map();
            units.forEach((u) => {
                let release;
                implDone.set(u.key, {
                    p: new Promise((res) => {
                        release = res;
                    }),
                    release,
                });
            });
            const keyIndex = new Map(units.map((u, i) => [u.key, i]));
            const unitReports = await Promise.all(
                units.map(async (unit, i) => {
                    const deps = (unit.after || []).filter((k) => implDone.has(k) && keyIndex.get(k) < i);
                    await Promise.all(deps.map((k) => implDone.get(k).p));
                    const fix = await slot(() =>
                        agent(implementPrompt(folder, unit, arch.preSwap, reports, kernelReports, scopes), {
                            label: 'impl:' + folder.key + ':' + unit.key,
                            phase: 'Build',
                            model: 'fable',
                            effort: 'high',
                            schema: FIXLOG,
                            stallMs: STALL,
                        }),
                    ).catch(() => null);
                    implDone.get(unit.key).release(); // dependents launch even on failure; they compose current disk
                    if (!fix)
                        return {
                            unit: unit.key,
                            pages: unit.pages.length,
                            pageList: unit.pages,
                            verdict: 'failed',
                            deferred: [],
                            indexRows: [],
                            critReport: '',
                            rtLanded: false,
                        };
                    // Sol ultra critique: a workspace-write codex lane editing this unit's pages in place; fixlog to disk,
                    // receipt on the wire; the red-team reads the fixlog from disk and folds its rows forward.
                    const crit = await slot(() =>
                        recon(critiquePrompt(folder, unit, scopes, navOf([fix])), {
                            label: 'crit:' + folder.key + ':' + unit.key,
                            phase: 'Build',
                            schema: FIXLOG,
                            writes: true,
                            model: 'gpt-5.6-sol',
                            codexEffort: 'ultra',
                            nativeModel: 'fable',
                            stallMs: SOL_STALL,
                            scope: unit.pages,
                        }),
                    );
                    const critR = crit && crit.ok ? crit : null;
                    const rt = await slot(() =>
                        agent(redteamPrompt(folder, unit, scopes, navOf([fix]), critR), {
                            label: 'rt:' + folder.key + ':' + unit.key,
                            phase: 'Build',
                            model: 'fable',
                            effort: 'high',
                            schema: FIXLOG,
                            stallMs: STALL,
                        }),
                    ).catch(() => null);
                    return {
                        unit: unit.key,
                        pages: unit.pages.length,
                        verdict: (rt && rt.verdict) || (critR ? 'refined' : fix.verdict),
                        deferred: [fix, rt].filter(Boolean).flatMap((r) => r.deferred || []),
                        indexRows: [fix, rt].filter(Boolean).flatMap((r) => r.indexRows || []),
                        critReport: critR ? critR.report : '',
                        rtLanded: !!rt,
                    };
                }),
            );

            // A folder where no unit landed has no tree to document; the run result carries the failure.
            const docs = unitReports.some((u) => u.verdict !== 'failed')
                ? await slot(() =>
                      agent(
                          docsPrompt(
                              folder,
                              unitReports.map((u) => ({ unit: u.unit, pages: u.pages, verdict: u.verdict })),
                          ),
                          {
                              label: 'docs:' + folder.key,
                              phase: 'Build',
                              model: 'opus',
                              effort: 'high',
                              schema: DOCS_SCHEMA,
                              stallMs: STALL,
                          },
                      ),
                  )
                : null;
            return {
                folder: folder.name,
                key: folder.key,
                root: folder.root,
                preSwap: arch.preSwap,
                units: unitReports,
                packageDeltas: arch.packageDeltas || [],
                deferred: unitReports.flatMap((u) => u.deferred),
                indexRows: unitReports.flatMap((u) => u.indexRows),
                failedPages: unitReports.filter((u) => u.verdict === 'failed').flatMap((u) => u.pageList || []),
                docs: docs ? docs.summary : 'dropped',
            };
        }).map((p) =>
            p.catch((e) => {
                log('folder failed: ' + e.message);
                return null;
            }),
        ),
    )
).filter(Boolean);

if (!results.length) {
    log('No folder landed - nothing to close');
    return { folders: [], law: 'skipped', acceptance: 'skipped' };
}

phase('Close');
const BACKLOG = results.flatMap((r) => r.deferred);
const ROWS = results.flatMap((r) => r.indexRows || []);
const ORPHANS = results.flatMap((r) => r.units.filter((u) => u.critReport && !u.rtLanded).map((u) => u.critReport));
log(
    'Close: ' +
        BACKLOG.length +
        ' deferred backlog row(s), ' +
        results.flatMap((r) => r.packageDeltas).length +
        ' package delta(s), ' +
        results.flatMap((r) => r.failedPages || []).length +
        ' failed page(s), ' +
        ORPHANS.length +
        ' orphaned critique fixlog(s)',
);
const accepts = await Promise.all(
    results.map((r) => slot(() => recon(acceptPrompt(r, ROWS), { label: 'accept:' + r.key, phase: 'Close', schema: REGISTER, scope: [r.root] }))),
);
const ACC_UNMAPPED = accepts.filter((a) => !a.ok).flatMap((a) => a.scope.map((sc) => ({ lane: a.lane, scope: sc })));
log(
    'Acceptance: ' +
        accepts.filter((a) => a.ok).reduce((n, a) => n + a.entries, 0) +
        ' finding(s) across ' +
        accepts.filter((a) => a.ok).length +
        '/' +
        accepts.length +
        ' lane(s)' +
        (ACC_UNMAPPED.length
            ? ' — FAILED: ' +
              accepts
                  .filter((a) => !a.ok)
                  .map((a) => a.lane)
                  .join(', ')
            : ''),
);
const law = await slot(() =>
    agent(lawPrompt(results, BACKLOG, accepts, ACC_UNMAPPED, ORPHANS), {
        label: 'law',
        phase: 'Close',
        model: 'fable',
        effort: 'high',
        schema: DOCS_SCHEMA,
        stallMs: STALL,
    }),
);

return {
    folders: results.map((r) => ({
        folder: r.folder,
        preSwap: r.preSwap,
        units: r.units.map((u) => ({ unit: u.unit, verdict: u.verdict })),
        packageDeltas: r.packageDeltas,
        failedPages: r.failedPages,
        docs: r.docs,
    })),
    backlog: BACKLOG.length,
    acceptance: accepts.map((a) => ({ lane: a.lane, ok: a.ok, entries: a.entries, headline: a.headline || a.failure })),
    law: law ? law.summary : 'dropped',
};
