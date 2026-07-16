export const meta = {
    name: 'trim-prose',
    whenToUse:
        'Prose-density pass over one or more .planning folders: every page rebuilt to the docgen register at 40-50% of its prose volume, two-lead intro enforced, terminal [RESEARCH] marker section landed, fence code bodies untouched.',
    description:
        'Focused prose refactor over libs/ planning pages. args = a .planning folder path, an array of paths, or {targets} — a package root resolves to its .planning; empty = no-op. One discovery agent censuses every page per target with a shared prose-line measurement; then one opus rewriter per file (pooled cap) reads the full docgen skill, the docs/standards prose owners, and the spec template, then rebuilds ALL prose and fence comments in place: telos + composition leads, [01]-[INDEX], terminal [RESEARCH] marker section, 50-60% prose reduction by defect-kill and density with zero capability loss, fence code bodies and section dividers untouched, prose gate run as the mechanical floor. Returns per-file receipts with honest before/after counts.',
    phases: [
        {
            title: 'Discover',
            detail: 'one census agent per run: every .md under each target .planning folder with total and prose line counts from the one shared measurement, the baseline every rewriter and receipt derives from',
        },
        {
            title: 'Rewrite',
            detail: 'one opus agent per file under the pooled cap: docgen law read in full, target read in full, all prose and fence comments rebuilt in place to the register at 40-50% volume, research section landed, gate run, typed receipt returned',
        },
    ],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAP = 14;
const CUT_LO = 50; // reduction band the receipt grades against, percent of baseline prose lines
const CUT_HI = 60;
const MEASURE = "awk '/^```/{f=!f;next} !f&&NF{n++} END{print n+0}'"; // prose lines = non-blank lines outside fences; the ONE measurement discovery, rewriters, and receipts share

// --- [INPUTS] --------------------------------------------------------------------------

const norm = (t) => String(t).trim().replace(/\/+$/, '').replace(/^\/+/, '');
const toPlanning = (t) => (t.endsWith('/.planning') || t.includes('/.planning/') ? t : t + '/.planning');
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
const TARGETS = [...new Set(rawTargets.filter(Boolean).map(norm).map(toPlanning))].filter((t) => t.startsWith('libs/'));
const ROOT = '/Users/bardiasamiee/Documents/99.Github/Rasm';

// --- [MODELS] --------------------------------------------------------------------------

const S = { type: 'string' };
const I = { type: 'integer' };
const DISCOVER_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['files', 'missing'],
    properties: {
        files: {
            type: 'array',
            items: {
                type: 'object',
                additionalProperties: false,
                required: ['path', 'total', 'prose'],
                properties: { path: S, total: I, prose: I },
            },
        },
        missing: { type: 'array', items: S }, // targets that resolved to no folder or no pages
    },
};
const REWRITE_SCHEMA = {
    type: 'object',
    additionalProperties: false,
    required: ['file', 'ok', 'beforeProse', 'afterProse', 'reductionPct', 'researchRows', 'gateClean', 'notes'],
    properties: {
        file: S,
        ok: { type: 'boolean' },
        beforeProse: I,
        afterProse: I,
        reductionPct: I,
        researchRows: I,
        gateClean: { type: 'boolean' },
        notes: S, // capability demotions, judgment-tier gate rows resolved, anything a reader of the run needs; empty when clean
    },
};

// --- [DOCTRINE] ------------------------------------------------------------------------

const LAW_FILES =
    ROOT +
    '/.claude/skills/docgen/SKILL.md, ' +
    ROOT +
    '/.claude/skills/docgen/references/structure.md, ' +
    ROOT +
    '/.claude/skills/docgen/references/defects.md, ' +
    ROOT +
    '/.claude/skills/docgen/references/rewriting.md, ' +
    ROOT +
    '/.claude/skills/docgen/templates/spec.template.md, ' +
    ROOT +
    '/.claude/skills/docgen/examples/intros.md, ' +
    ROOT +
    '/docs/standards/style-guide.md, ' +
    ROOT +
    '/docs/standards/information-structure.md, ' +
    ROOT +
    '/docs/standards/formatting.md';

const PREAMBLE =
    'WORKING ROOT: ' +
    ROOT +
    ' — every relative path resolves against it; read, write, and edit only under it. ' +
    'Rasm libs/ planning corpus: each page is a decision-complete design blueprint whose transcription-complete code FENCES are the work product; ' +
    'prose exists ONLY to carry what fences cannot — decisions, invariants, boundaries, ownership rulings, traps. ' +
    "Every prose line is agent-facing law loaded by a rebuild agent with no memory of why it was written: it changes that agent's next action or it poisons it.";

const rewriteMandate = (file, prose) =>
    PREAMBLE +
    '\n\nWHY THIS PASS EXISTS — the page is maintained by agents, forever: every future rebuild loads this prose as its working model, so weak prose compounds. ' +
    'You are hunting two enemies. FRAGILE prose: anything that needs updating when the corpus moves without this page moving — member rosters, counts, ' +
    'sibling-shape mirrors, version/freshness anchors, cross-references, self-descriptions; each is stale the day after it is written and silently lies thereafter. ' +
    "POISON prose: anything that walls in the next rebuild — enumeration anchors that freeze today's census as the contract, deleted-form litanies that anchor " +
    'to the anti-shape, tombstones (process ledger, decision tags, ship-status, resolved-research residue, "this replaces X" narration), seals and frozen chants, ' +
    'hedges and weak permission verbs that mark undecided ownership. What survives is only LAW an agent cannot regenerate from disk: decisions, invariants, ' +
    'boundaries, ownership rulings, traps — each with an owning subject and owning verb, stated once, timeless. The prize is a page that rebuilds cleaner, ' +
    'reads faster, anchors nothing, and never needs a prose-sync pass again. Reduction is the consequence of this hunt, never the objective.' +
    '\n\nTHE ACID TEST — apply to every sentence you keep or write: does it stay true, unchanged, across any doctrine-conforming rebuild of the fences it ' +
    'accompanies? Prose couples to intent; the fence couples to shape. A sentence whose truth depends on the current fence body is FRAGILE — it drifts the ' +
    "moment the fence improves; a sentence that names the current shape is POISON — it walls the next rebuild inside today's design. Both cure the same way: " +
    'state what the rebuild must preserve, and let the fence own the shape. The page passes when a stronger fence body lands with zero prose edits.' +
    '\n\nPROSE-CODE BOUNDARY: prose names owners; code carries mechanism. A prose line names at most the owning symbol as a code span and states its law — ' +
    'signatures, parameter lists, member chains, option rosters, and per-member behavior belong to the fence; carried in prose they are drift debt the next ' +
    'fence edit falsifies. A list entry is one focused decision: one owner, one charter phrase; an enumeration tail or second clause demotes to the owning ' +
    'fence or splits into a sibling entry. A rebuilt block reads as one clean explanation of the file and its decisions — high-signal for the agent working ' +
    'the page in isolation, never a maintenance burden that must be re-synced when siblings move.' +
    '\n\nARM FIRST — read these law files IN FULL before opening the target (reading the target first inherits its frame): ' +
    LAW_FILES +
    '. Then read ' +
    file.split('/.planning/')[0] +
    "/README.md — the package README whose intro register is your exemplar: leads that legislate in owning voice, dense with the folder's own vocabulary, " +
    'zero narration. Your two leads must read like they were authored by the same hand.' +
    '\n\nTARGET: ' +
    file +
    ' — read it IN FULL, as evidence never guidance. Baseline prose count: ' +
    prose +
    ' lines (non-blank lines outside fences, measured by ' +
    MEASURE +
    ').' +
    '\n\nREBUILD every prose surface of this page in place under the docgen register and defect catalog. The page has overgrown: prose accumulated as narration, ' +
    'restatement, enumeration anchors, hedges, and mirrored fence content. Your pass is hostile — every existing sentence is suspect until it earns its slot, ' +
    'and a rewrite is a reframe from the extracted payload, never a shorter paraphrase of the old sentence.' +
    '\n\nTERRITORY — prose surfaces you rebuild: the H1 leads, every section card bullet and paragraph, list entries, table leads, and COMMENT LINES inside fences. ' +
    'UNTOUCHED — fence code bodies: signatures, types, members, cases, fields, logic, imports; mermaid diagram bodies; section-divider comment lines inside fences ' +
    '(style-correct a divider label if wrong, never delete one). You are a prose surgeon, never a design editor: no owner, case, member, or design decision changes.' +
    '\n\nSHAPE the page lands in (the spec template is the schema):' +
    '\n1. H1, then EXACTLY two STRONG lead paragraphs at the README exemplar bar — the telos lead (the capability this page owns, its piece in the folder system, ' +
    'the boundary it holds — an agent reads it and knows exactly what this owner is FOR) and the composition lead (the settled core facts an agent needs before ' +
    'editing: reused axes, seam obligations, wire names, rails, policy rows — the working-context paragraph that spares the reader a corpus crawl). ' +
    'Fold any third-plus intro paragraph into these two or demote its content to the owning cluster.' +
    '\n2. `## [01]-[INDEX]` — one line per cluster, `- [NN]-[TOKEN]: <hook>`, never a card restatement. Build it when absent, rebuild it when stale.' +
    '\n3. The existing cluster sections, renumbered contiguously when your edits shift positions.' +
    '\n4. Terminal `## [NN]-[RESEARCH]` — every row `- [TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>`, `(none)` when empty. ' +
    'Reformat any existing research content into that exact grammar; add the section when absent; NEVER invent rows to fill it.' +
    '\n\nREDUCTION: land the page at ' +
    CUT_LO +
    '-' +
    CUT_HI +
    '% fewer prose lines than the ' +
    prose +
    ' baseline. The cut comes from defect-kill and density — ' +
    'delete narration, restatement of fence content, enumeration anchors, deleted-form litanies, process ledger, hedges, meta frames, twin truths; ' +
    'tighten every survivor to the load-bearing-word law. Capability loss is the one forbidden move: every decision, invariant, boundary ruling, and trap survives ' +
    'the cut — in fewer words, or demoted into the fence comment that owns it. Softening is inheritance: ' +
    'a defect class is removed by reframing the passage, never by dieting it. ' +
    'If an honest full-register rebuild lands outside the band, the register wins — report the true number and say why in notes.' +
    '\n\nFENCE COMMENTS: run the comment ladder on every comment — delete no-load comments whole, tighten survivors, re-wrap toward the 150-column cap, ' +
    'inline one-line survivors as trailing tails; a constraint the code cannot show survives, never the delete.' +
    '\n\nGATE: after the rebuild run `cd ' +
    ROOT +
    ' && uv run .claude/skills/docgen/scripts/prose_gate.py fix --write ' +
    file +
    '` then ' +
    '`uv run .claude/skills/docgen/scripts/prose_gate.py ' +
    file +
    '`; resolve judgment-tier SKIP rows by hand; gateClean is the second command exiting clean.' +
    '\n\nRECEIPT: measure afterProse with the same awk; return {file, ok, beforeProse, afterProse, ' +
    'reductionPct (integer percent), researchRows (row count, 0 for (none)), ' +
    'gateClean, notes}. Honest numbers only — a padded count or an unrun gate reported clean is a defect worse than a missed band.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const pool = async (thunks, cap) => {
    const results = new Array(thunks.length);
    let next = 0;
    const lane = async () => {
        while (next < thunks.length) {
            const i = next++;
            results[i] = await thunks[i]().catch(() => null);
        }
    };
    await Promise.all(Array.from({ length: Math.min(cap, thunks.length) }, lane));
    return results;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

if (!TARGETS.length) {
    log('No targets — pass a .planning folder path (or its package root), an array, or {targets}. Empty args is a no-op.');
    return { skipped: true };
}

phase('Discover');
const census = await agent(
    PREAMBLE +
        '\n\nCENSUS these planning folders: ' +
        JSON.stringify(TARGETS) +
        '. For each, enumerate every .md file' +
        ' recursively (fd -e md . <folder>). For every file record total lines (wc -l) and prose lines via' +
        ' exactly: ' +
        MEASURE +
        ' <file>. A target folder that does not exist or holds no .md files goes to' +
        ' missing. Return the flat file list across all targets; no analysis, no reading of page bodies.',
    { label: 'census', phase: 'Discover', model: 'sonnet', effort: 'low', schema: DISCOVER_SCHEMA },
);
if (!census || !census.files.length) {
    log('Census returned no files' + (census && census.missing.length ? '; missing: ' + census.missing.join(', ') : ''));
    return { skipped: true, missing: census ? census.missing : TARGETS };
}
if (census.missing.length) log('Missing targets skipped: ' + census.missing.join(', '));
const baseline = census.files.reduce((a, f) => a + f.prose, 0);
log(census.files.length + ' pages across ' + TARGETS.length + ' target(s), ' + baseline + ' baseline prose lines');

phase('Rewrite');
const receipts = (
    await pool(
        census.files.map(
            (f) => () =>
                agent(rewriteMandate(f.path, f.prose), {
                    label: 'trim:' + f.path.split('/').slice(-2).join('/'),
                    phase: 'Rewrite',
                    model: 'opus',
                    effort: 'high',
                    schema: REWRITE_SCHEMA,
                }),
        ),
        CAP,
    )
).filter(Boolean);

const inBand = receipts.filter((r) => r.ok && r.reductionPct >= CUT_LO && r.reductionPct <= CUT_HI);
const offBand = receipts.filter((r) => r.ok && (r.reductionPct < CUT_LO || r.reductionPct > CUT_HI));
const failed = receipts.filter((r) => !r.ok);
const dropped = census.files.length - receipts.length;
const beforeTotal = receipts.reduce((a, r) => a + r.beforeProse, 0);
const afterTotal = receipts.reduce((a, r) => a + r.afterProse, 0);
if (dropped) log(dropped + ' page agent(s) returned nothing — rerun those files or resume');
return {
    targets: TARGETS,
    pages: census.files.length,
    rewritten: receipts.length,
    corpusReductionPct: beforeTotal ? Math.round(100 - (afterTotal / beforeTotal) * 100) : 0,
    inBand: inBand.length,
    offBand: offBand.map((r) => ({ file: r.file, reductionPct: r.reductionPct, notes: r.notes })),
    failed: failed.map((r) => ({ file: r.file, notes: r.notes })),
    gateUnclean: receipts.filter((r) => !r.gateClean).map((r) => r.file),
    dropped,
};
