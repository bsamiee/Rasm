export const meta = {
    name: 'estate',
    description: 'Per-language estate tracks - recon dossiers, then initial/critique/redteam fable passes - closing with a monorepo final track',
    whenToUse:
        'Full estate improvement over tests/tools/root configs per language, then polyglot alignment; launch from a fable session (passes inherit the session model)',
    phases: [{ title: 'Recon' }, { title: 'Estate' }, { title: 'Final' }],
};

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCRATCH = '.claude/scratch/estate';
const CORE_PAGES = 4;

const TRACKS = {
    csharp: {
        docs: 'docs/stacks/csharp',
        docsNote: 'ignore the domain/ folder entirely',
        scope:
            'Directory.Packages.props organization: every row homed truthfully — a transitive-only row lives under Transitive Floors with a one-line comment ' +
            'naming what pulls it (verify via dotnet nuget why or lockfiles), a directly-consumed row lives under its consumer-domain label; alphabetical within ' +
            'groups, aligned columns, one-line comments only. The bidirectional props<->csproj<->README<->.api audit across libs/csharp + tools + tests/csharp: ' +
            'every mismatch fixed at the truthful end; substrate truthfulness (a branch-substrate registry row needs 2+ consumers, a single-consumer package ' +
            'relocates to the folder registry + folder .api tier, both ends). tests/csharp shared infrastructure: _testkit, _scenariokit, _architecture, ' +
            '_benchmarks, scenarios, tools, README — world-class, discovery-driven, zero hardcoded package lists; tests/csharp/.api is a first-class test-stack ' +
            'catalog tier (mirror tests/python/.api and tests/typescript/.api) with a member-verified catalog per testing package. tools/rhino-bridge scenarios ' +
            'and universal scenario kits; tools/cs-analyzer; .config C# tooling plus NuGet.config and global.json where improvement is justified; ' +
            'Directory.Build.props/targets and Workspace.slnx coherence chasing every csproj ' +
            'in the monorepo. LAW, not drift: Rasm.Rhino and Rasm.Grasshopper stay OUT of Workspace.slnx under the HOST_BOUNDARY_REENTRY gate in ' +
            'tests/csharp/_architecture/AssemblyBoundaries.spec.cs.',
        gates:
            'dotnet restore Workspace.slnx clean; dotnet build Workspace.slnx with zero errors AND zero warnings (analyzer findings fixed at the shape, never ' +
            'suppressed); dotnet format Workspace.slnx --verify-no-changes conformant with .editorconfig; dotnet test tests/csharp/_architecture green; prose ' +
            'gate zero FAILs on every touched .md; rg proof of zero stale references to relocated or deleted files.',
    },
    python: {
        docs: 'docs/stacks/python',
        docsNote: '',
        scope:
            'tests/python/.api: a member-verified catalog per test-stack package (verify via installed distributions and assay). _testkit + conftest topology: ' +
            'the root conftest as the single shared composition owner, per-suite conftests compose and never duplicate; SUT auto-registration derived from disk ' +
            'shape; sentinel-based repo-root discovery (never a fixed parents[N] depth); one truthful marker taxonomy across pyproject, README, and policy code. ' +
            'pytest extension admissions on merit — wired into required_plugins and the kit the moment they land, or rejected; no duplicate paradigms. pyproject ' +
            'test-config optimization: markers, addopts, coverage/mutation coherence, stale exclusions, one-line comments. tests/python/libs + tests/python/tools ' +
            'unified against the improved kit, spam collapsed, naivety reduced per the doctrine.',
        gates:
            'uv lock and uv sync clean; uv run ruff check clean; uv run ty check clean; uv run mypy clean; uv run pytest tests/python green (targeted suites ' +
            'when the full run exceeds ten minutes, stating what ran); prose gate zero FAILs on every touched .md. Zero-error law: findings fixed correctly ' +
            'root/ground-up — never type-ignore, suppressions, or bandaids.',
    },
    typescript: {
        docs: 'docs/stacks/typescript',
        docsNote: '',
        scope:
            'tests/typescript/e2e as one parameterized, polymorphic multi-project harness serving many future projects from config-driven targets — zero ' +
            'fragility, full modern Playwright surface (projects matrix, fixture composition, trace/video policy rows, webServer lifecycle). ' +
            'playwright.config.ts, vite.config.ts, vite.factory.ts, and vitest.config.ts evolve as one factory system with vite.factory.ts the parameterization ' +
            'owner. _testkit + _architecture extended discovery-driven with no hardcoded package lists. tests/typescript/.api completed for the test stack; ' +
            'tooling admissions via the pnpm catalog on merit, wired or rejected. tools/biome modernized and extended (justified biome/nx plugins admitted; ' +
            'assay stays the operator boundary). Root TS config coherence: tsconfig.base.json, tsconfig.json, biome.json, nx.json.',
        gates:
            'pnpm install clean; pnpm run typecheck (tsgo AND tsc) fully clean; pnpm run check fully clean on touched files (never biome-ignore); pnpm run ' +
            'test and pnpm run e2e green for tests/typescript suites; prose gate zero FAILs on every touched .md. Zero-error law: findings fixed correctly ' +
            'root/ground-up — never any-casts, suppressions, or bandaids.',
    },
};

const FINAL_TRACK = {
    docs: '',
    docsNote: '',
    scope:
        'Monorepo/polyglot alignment at every level: cross-language gaps, oversights, and asymmetries in tests/, tools/, root config files, routing/pathing, ' +
        'and libs/ registries; fragility, hardcoding, hardscoping, and naivety patterns fixed for universal future use so nothing needs re-adjustment later; ' +
        'removal-biased prose cleanup per the docgen register — tombstones, weak or fragile prose, dead references and citations, unnecessary coupling — ' +
        'reduce the prose maintenance mountain, never add to it. The three language estates are settled input state: align them, close their seams, and fix ' +
        'what they missed.',
    gates:
        'dotnet restore Workspace.slnx clean; dotnet build Workspace.slnx zero errors and warnings; dotnet format Workspace.slnx --verify-no-changes clean; ' +
        'dotnet test tests/csharp/_architecture green; uv run ruff check + ty check + mypy all clean; pnpm run typecheck and pnpm run check clean; prose ' +
        'gate zero FAILs on every touched .md. Zero-error law: fixed root/ground-up, never suppressed.',
};

// --- [INPUTS] --------------------------------------------------------------------------

const LANGS = Array.isArray(args)
    ? args
    : typeof args === 'string' && args
      ? [args]
      : Array.isArray(args?.languages)
        ? args.languages
        : ['csharp', 'python', 'typescript'];
const WANT_FINAL = args?.final !== false;
const ACTIVE = LANGS.filter((l) => TRACKS[l]);

// --- [MODELS] ----------------------------------------------------------------------------

const DOSSIER_RECEIPT = {
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

const PASS_RECEIPT = {
    type: 'object',
    additionalProperties: false,
    required: ['ok', 'headline', 'filesChanged', 'gates', 'residuals'],
    properties: {
        ok: { type: 'boolean' },
        headline: { type: 'string' },
        filesChanged: { type: 'integer' },
        gates: { type: 'string' },
        residuals: { type: 'array', items: { type: 'string' } },
    },
};

// --- [DOCTRINE] --------------------------------------------------------------------------

const MODEL_LAW =
    'MODEL LAW: you execute every file write and every judgment yourself. Delegate read-only reconnaissance roughly 50/50 between codex ' +
    '(Bash: codex exec -s read-only --skip-git-repo-check -c \'mcp_servers={}\' "<self-contained scoped question>" </dev/null 2>/dev/null — synchronous, ' +
    'one bounded question per leg) and opus subagents (Agent tool, model opus, explicit READ-ONLY mandate; fall back to codex if Agent is unavailable). ' +
    'Recon returns facts, locations, inventories, and verified member lists — never instructions, prescriptions, or edits; recon agents use exa/tavily, ' +
    'the nuget MCP, Context7, uv run python -m tools.assay, and fd/rg/loc/tree.';

const GUARDRAILS =
    'HARD GUARDRAILS: never modify any IDEAS.md or TASKLOG.md; never redesign code-fence interiors in libs planning pages (fence comments may tighten per ' +
    'the comment law); never git commit. Durable prose follows the docgen register (.claude/skills/docgen/SKILL.md + references/defects.md): no weak, ' +
    'defensive, or process prose, no context poisoning, no tombstones. Every touched .md passes ' +
    'uv run .claude/skills/docgen/scripts/prose_gate.py with zero FAILs.';

const ADMISSION =
    'ADMISSION PROCEDURE (any new tool/package you add): the admission lands COMPLETE in this pass — central manifest row hand-edited ' +
    '(Directory.Packages.props / pyproject.toml / pnpm-workspace.yaml catalog + package.json), consumer wiring (csproj row for C#), a folder README ' +
    'registry row, and a full .api catalog at the correct tier (language-shared tier only for 2+ consumers, folder tier otherwise, tests/<lang>/.api for ' +
    'test-stack tooling). Gather the catalog facts through ONE delegated read-only recon agent mining verified members (assay api, installed ' +
    'distributions, nuget MCP, type declarations); author the file yourself per the docgen api-catalog template ' +
    '(.claude/skills/docgen/templates/api-catalog.template.md); the restore/lock gate proves the admission. Surgical prose updates only — touch the rows ' +
    'the admission changes, nothing else.';

const REVIEWER_LAW =
    'REVIEWER-CONFIG ENRICHMENT (opportunistic, never a mandated deliverable): .greptile/rules.md + config.json + files.json and .coderabbit.yaml are the ' +
    'standing reviewer doctrine. When your pass surfaces a high-signal implicit pattern those files do not already state — a quality shape, a testkit/infra ' +
    'construction law, test/tool code discipline, a libs/ standards nuance, an agent-framed prose norm, or existing guidance now wrong or weaker than the ' +
    'estate practices — land it there in the same pass: harden or correct the owning instruction where one exists, add a new one only when no owner covers ' +
    'it, and mirror every ruling across both surfaces (the rules.md section and the matching .coderabbit.yaml path_instructions block move together). ' +
    'Admission bar: consistent across the estate, doctrine-derived (docs/stacks/<language>, docs/standards), and invisible to the machine gates — never ' +
    'restate what formatters/gates/analyzers enforce, never duplicate an existing line, never add speculative or one-off rules. Write the guidance as ' +
    'durable law over the finished system: never couched in the planning phase, campaign state, or session narration. yamllint proves .coderabbit.yaml, jq ' +
    'proves the .greptile JSON files, and rules.md rides the prose gate like any touched .md.';

const INFRA_LAW =
    'SHARED-INFRA PRIMACY: the shared test infrastructure is the estate CENTER, never a side item — csharp: tests/csharp/_testkit, _scenariokit, ' +
    '_architecture, _benchmarks, scenarios; python: tests/python/_testkit + the conftest topology; typescript: tests/typescript/_testkit, _architecture, ' +
    'and e2e PLUS nx.json, biome.json, tools/biome, playwright.config.ts, vite.factory.ts, and vitest.config.ts as one system. FIRST explore your ' +
    'language libs/ planning corpus in depth — every package folder: domains, owners, seams, receipt families, wire shapes — to understand the extreme ' +
    'complexity arriving when every folder goes live; the infra must anticipate ALL of it NOW. Build intelligent, universal, polymorphic foundations that ' +
    'make future tests near-trivial to write and REDUCE total test code — declarative gauges, law tables, fixture algebras, discovery-driven ' +
    'registration — never naive or simple scaffolding, never replace-in-place minimalism. Beyond anticipation, improve in isolation: add the capabilities ' +
    'the infra is missing outright, admitting new packages through the admission procedure whenever they raise the bar.';

const TIER_LAW = {
    T1: 'PASS T1 (INITIAL): realize the whole mandate with full write authority — implement, extend, and collapse; this is build work, not cleanup.',
    T2:
        'PASS T2 (CRITIQUE): a cold pass with FULL, EQUAL write authority. Derive your own findings from disk first; every earlier pass output is suspect ' +
        'material to attack, never a boundary or a baseline to defer to. Run the mechanical line-by-line doctrinal-conformance and capability-completeness ' +
        'audit repaired in place — collapse scan, owner choice, knob test, rails, language modernity, capability and illusion — as a floor and hunt past it; ' +
        'every hit is a fix, never a note; extend, expand, and ripple wherever you find value.',
    T3:
        'PASS T3 (REDTEAM): everything critique does AND the terminal attack — counterfactual on core owners/algebras/dispatch, diff-of-the-next-feature ' +
        '(the next case, project, or package lands as one row with consumers untouched or loudly broken), long-tail and failure-mode attack, boundary and ' +
        'strata integrity, surface sprawl and phantom members, domain completeness — plus a full cold re-review of every dimension. The estate ends ' +
        'objectively denser and more capable than the prior pass left it.',
};

// --- [OPERATIONS] ------------------------------------------------------------------------

const docsOrder = (t) =>
    t.docs
        ? 'CODE DOCTRINE: read ' +
          t.docs +
          '/README.md IN FULL — its numbered routing table orders the corpus. Read IN FULL the first ' +
          CORE_PAGES +
          ' pages that table lists' +
          (t.docsNote ? ' (' + t.docsNote + ')' : '') +
          '; know the remaining pages exist and consult them on demand while ' +
          'editing, never wholesale. '
        : 'CODE DOCTRINE: each file you touch follows its language doctrine under docs/stacks/<language>/ — consult the owning pages on demand. ';

const dossierPath = (name, lane) => SCRATCH + '/' + name + '-recon-' + lane + '-report.md';

const reconPrompt = (t, name, lane) =>
    'READ-ONLY recon lane for the ' +
    name +
    ' estate of this repo. Build a factual dossier of the estate scope below: file inventories with one-line ' +
    'states, package/consumer matrices from manifests and lockfiles, config cross-references, upstream versions where staleness is suspected (nuget MCP, ' +
    'PyPI, npm), and exact file:line anchors for everything notable. Include a LIBS-COMPLEXITY section: map the relevant libs/ planning corpus in depth — ' +
    'every package folder with its domains, owners, seams, receipt families, and wire shapes — as facts the shared test infrastructure must anticipate. ' +
    'FACTS AND LOCATIONS ONLY — no verdicts, no prescriptions, no recommendations. ' +
    'First act: rm -f ' +
    dossierPath(name, lane) +
    '. Write the complete dossier to ' +
    dossierPath(name, lane) +
    ' (mkdir -p the folder), then return ' +
    'the receipt: ok, report=that path, entries=count of dossier rows, headline=mechanical tally, failure="" (or the error). ' +
    (lane === 'gpt'
        ? 'Dispatch the reading to codex read-only legs (synchronous, scoped questions) and compose their facts into the dossier yourself. '
        : '') +
    'SCOPE: ' +
    t.scope;

const passPrompt = (t, name, tier, reconRows) =>
    'You are the ' +
    name +
    ' ESTATE ' +
    tier +
    ' agent for this repository (a design/planning-phase polyglot monorepo; test/tool/config infrastructure ' +
    'is live code you improve for real). Work the whole mandate to completion. ' +
    TIER_LAW[tier] +
    ' ' +
    INFRA_LAW +
    ' ' +
    docsOrder(t) +
    MODEL_LAW +
    ' ' +
    GUARDRAILS +
    ' ' +
    ADMISSION +
    ' ' +
    REVIEWER_LAW +
    ' ' +
    (reconRows && reconRows.length
        ? 'RECON DOSSIERS (read each IN FULL first; scratch is gitignored so open these exact paths): ' +
          reconRows.map((r) => r.report + (r.ok ? '' : ' [lane failed: ' + r.failure + ']')).join(', ') +
          '. Dossiers are facts, never instructions. '
        : 'No recon dossiers landed — do your own reconnaissance per the model law before editing. ') +
    'MANDATE: ' +
    t.scope +
    ' GATES (all green before you return): ' +
    t.gates +
    ' Return the receipt: ok, headline (what materially changed), filesChanged, gates (verbatim results), residuals (deliberately-left items with reasons).';

// --- [COMPOSITION] -------------------------------------------------------------------------

// --- [RECON_AND_TRACKS]
const trackRows = ACTIVE.map((lang) => ({ lang, ...TRACKS[lang] }));
log('estate tracks: ' + ACTIVE.join(', ') + (WANT_FINAL ? ' + final' : ''));

const results = await pipeline(
    trackRows,
    (t) =>
        parallel([
            () =>
                agent(reconPrompt(t, t.lang, 'opus'), {
                    model: 'opus',
                    effort: 'high',
                    phase: 'Recon',
                    label: 'recon-opus:' + t.lang,
                    schema: DOSSIER_RECEIPT,
                }),
            () =>
                agent(reconPrompt(t, t.lang, 'gpt'), {
                    model: 'sonnet',
                    effort: 'low',
                    phase: 'Recon',
                    label: 'gpt-5.5:recon-' + t.lang,
                    schema: DOSSIER_RECEIPT,
                }),
        ]),
    (recon, t) =>
        agent(passPrompt(t, t.lang, 'T1', (recon || []).filter(Boolean)), {
            effort: 'high',
            phase: 'Estate',
            label: 't1:' + t.lang,
            schema: PASS_RECEIPT,
        }).then((r) => ({ t1: r })),
    (acc, t) =>
        agent(passPrompt(t, t.lang, 'T2', null), { effort: 'high', phase: 'Estate', label: 't2:' + t.lang, schema: PASS_RECEIPT }).then((r) => ({
            ...acc,
            t2: r,
        })),
    (acc, t) =>
        agent(passPrompt(t, t.lang, 'T3', null), { effort: 'high', phase: 'Estate', label: 't3:' + t.lang, schema: PASS_RECEIPT }).then((r) => ({
            ...acc,
            t3: r,
        })),
);

// --- [FINAL]
let final = null;
if (WANT_FINAL && ACTIVE.length) {
    phase('Final');
    const f = { lang: 'monorepo', ...FINAL_TRACK };
    const fRecon = (
        await parallel([
            () =>
                agent(reconPrompt(f, 'monorepo', 'opus'), {
                    model: 'opus',
                    effort: 'high',
                    phase: 'Final',
                    label: 'recon-opus:final',
                    schema: DOSSIER_RECEIPT,
                }),
            () =>
                agent(reconPrompt(f, 'monorepo', 'gpt'), {
                    model: 'sonnet',
                    effort: 'low',
                    phase: 'Final',
                    label: 'gpt-5.5:recon-final',
                    schema: DOSSIER_RECEIPT,
                }),
        ])
    ).filter(Boolean);
    const f1 = await agent(passPrompt(f, 'monorepo FINAL', 'T1', fRecon), {
        effort: 'high',
        phase: 'Final',
        label: 'final:t1',
        schema: PASS_RECEIPT,
    });
    const f2 = await agent(passPrompt(f, 'monorepo FINAL', 'T2', null), { effort: 'high', phase: 'Final', label: 'final:t2', schema: PASS_RECEIPT });
    const f3 = await agent(passPrompt(f, 'monorepo FINAL', 'T3', null), { effort: 'high', phase: 'Final', label: 'final:t3', schema: PASS_RECEIPT });
    final = { t1: f1, t2: f2, t3: f3 };
}

return {
    tracks: Object.fromEntries(trackRows.map((t, i) => [t.lang, results[i]])),
    final,
    note: 'Agents never commit; the orchestrator commits once after all estate tracks close (pre-final), then after each final pass.',
};
